// LocalLightPool: the shared manager of the at most 4 real local lights of MASTER_PROMPT 14.2.3 ("emissive lamps with
// additive pool quads, at most 4 real local lights", the Forward+ set of URP Lit, Part 8.6). Every beat registers its lamp
// ANCHORS (position, linear colour, intensity, range, optional spot cone) once at build; the pool owns the 4 Light
// components and, every frame (IWorldUpdater, after the camera moved), gives them to the nearest anchors in view.
//
// No pops (Part 10 #6, 14.10.3 luma step <= 1.2 levels per frame): the target weight of every anchor is a CONTINUOUS
// function of the camera pose, and the applied weight follows it through a rate limit.
//   1. priority p = q x view x rank, q = d / reach (reach = range + ReachExtra), view = 1 + BehindPenalty x
//      smoothstep(0, ViewFadeDeg, margin), margin = angle(forward, anchor) - asin(range / d) - half-diagonal FOV (the cone of
//      the widest aspect, 21:9, so every capture aspect gets the same set), rank = the anchor's own weight (1 by default,
//      < 1 = more important);
//   2. the 4 smallest p are picked; target t = (1 - smoothstep(1 - FadeFraction, 1, q)) x (1 - smoothstep(e - FadeFraction,
//      e, p)), e = p of the best anchor left out (1 + BehindPenalty when none): a light is at zero where its anchor leaves
//      the set, by distance (at its reach) or by rank (where the next anchor overtakes it), so lit = f(pose) is continuous;
//   3. walking and sequences: each slot's weight moves toward its target by at most dt / FadeSeconds (a whip pan or a gain
//      switch fades over 0.5 s instead of in one frame); a slot whose anchor left the set keeps it until its weight is 0, then
//      takes the next picked anchor, so never more than 4 lights shine;
//   4. capture stills (GameState.CaptureMode without AtmosphereDriver.SequenceMode), the first frame and teleports over 20 m:
//      the steady state (weight = target, slots filled in anchor order): a pure function of the pose, deterministic.
// Allocation-free per frame. A beat may move an anchor (lamp sway, SetPosition) or scale it (relay lamps, SetGain); gain 0
// takes it out of the pick. Light colour: Light.color is sRGB (URP linearises it), so the pool stores the gamma colour of
// the linear palette value. Shadows off (14.2.3: casters only from the sun within 150 m).
// Usage in a builder:  var pool = LocalLightPool.Ensure(ctx);  int id = pool.Add(pos, BulwarkPalette.Linear(Pal.SodiumWarm), 9f, 1.6f, "beat1");
// QA: the EditMode tests drive Step() on a pool without lights (Light-free constructor) and read SlotAnchor / SlotWeight.
//
// Sky OPENINGS (Wave B): the pool also owns up to 4 rectangular openings in z planes (doors, portals, arches) through which
// the exterior lights an interior. They are no Light components: the pool publishes them as the global _BwOpen (the only
// writer) and the lit pass adds BwOpeningLight (BwLighting.hlsl) to the interior ambient: each opening is an area light of
// exterior radiance for the points on its interior side (Lambert's polygon formula, as BwMouthLight does for the beat 7
// tunnel mouth), so the walls beside an open door take the daylight that spills in instead of the lamp ambient alone
// (KS03: the region ambient alone left the walls round the open door at the lamp colour x 0.06). Layout of _BwOpen[2k]:
// (xMin, xMax, yMin, yMax), _BwOpen[2k+1]: (z plane, side +1 = exterior at z > plane / -1 = at z < plane, reach m, on).
// Usage: int o = pool.AddOpening("beat1_door", xMin, xMax, yMin, yMax, zPlane, exteriorAtPlusZ, reach); per frame
// pool.SetOpeningX(o, xMin, xMax) (a sliding door) and pool.SetOpeningOn(o, gap > 0); C# twin: OpeningFactor.
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Megastructure.Rendering
{
    public sealed class LocalLightPool : IWorldUpdater
    {
        /// <summary>14.2.3: at most 4 real local lights.</summary>
        public const int MaxLights = 4;
        /// <summary>An anchor is a candidate within its range + 12 m of the camera.</summary>
        public const float ReachExtra = 12f;
        /// <summary>Distance and rank fade window, in units of q = d / reach (0.15: about 3 m of a 21 m reach).</summary>
        public const float FadeFraction = 0.15f;
        /// <summary>Time for a slot weight to go 0 -> 1 (or back) when its target jumps (rotation, gain switch).</summary>
        public const float FadeSeconds = 0.5f;
        /// <summary>Priority penalty of an anchor whose light sphere lies wholly outside the view cone (q x 1.6).</summary>
        public const float BehindPenalty = 0.6f;
        /// <summary>Angular margin (deg) over which the view penalty ramps in.</summary>
        public const float ViewFadeDeg = 20f;
        /// <summary>Aspect of the view cone used for the pick (21:9, the widest capture aspect).</summary>
        public const float ViewAspect = 64f / 27f;
        /// <summary>A camera jump larger than this (m) resets to the steady state (FrameworkConst.TeleportJump).</summary>
        public const float TeleportJump = (float)FrameworkConst.TeleportJump;

        /// <summary>One registered lamp.</summary>
        public struct Anchor
        {
            public Vector3 position;
            /// <summary>Linear colour (palette token value).</summary>
            public Color colorLinear;
            public float range, intensity;
            /// <summary>Runtime scale (relays, flicker-free dimming); 0 removes it from the pick.</summary>
            public float gain;
            /// <summary>Priority multiplier (1 normal, below 1 = picked first).</summary>
            public float rank;
            /// <summary>Spot cone full angle in degrees; 0 = point light.</summary>
            public float spotAngle;
            public Quaternion rotation;
            public string owner;
            /// <summary>The light follows the 14.2.5 portal lamp boost 2^(1.3 (1 - smootherstep((s - 920) / 40))) (atrium
            /// lamps: "lit interior surfaces net +0.49 EV at s 920"); BulwarkSchedule.LampBoost(s) of the frame.</summary>
            public bool lampBoost;
        }

        static LocalLightPool s_current;
        /// <summary>The pool of the most recently built world (null before the first Ensure).</summary>
        public static LocalLightPool Current => s_current;

        readonly List<Anchor> m_anchors = new List<Anchor>(128);
        readonly Light[] m_lights = new Light[MaxLights];
        readonly int[] m_slotAnchor = new int[MaxLights];
        readonly float[] m_slotWeight = new float[MaxLights];
        readonly int[] m_pick = new int[MaxLights + 1];
        readonly float[] m_pickP = new float[MaxLights + 1];
        readonly float[] m_target = new float[MaxLights];
        readonly Transform m_root;
        bool m_init;
        Vector3 m_lastCam;
        int m_lit;
        float m_lampBoost = 1f;
        /// <summary>The portal lamp boost applied to anchors with lampBoost (set per frame from s).</summary>
        public float LampBoostValue { get => m_lampBoost; set => m_lampBoost = Mathf.Max(0f, value); }

        /// <summary>A pool without Light components (EditMode tests and pure evaluation).</summary>
        public LocalLightPool() : this(null) { }

        /// <summary>A pool whose 4 Light components live under <paramref name="parent"/> (null: no lights).</summary>
        public LocalLightPool(Transform parent)
        {
            m_root = parent;
            for (int i = 0; i < MaxLights; i++) { m_slotAnchor[i] = -1; m_slotWeight[i] = 0f; }
            if (parent == null) return;
            for (int i = 0; i < MaxLights; i++)
            {
                var go = new GameObject("bw_local_light_" + i);
                go.transform.SetParent(parent, false);
                var l = go.AddComponent<Light>();
                l.type = LightType.Point;
                l.shadows = LightShadows.None;
                l.renderMode = LightRenderMode.ForcePixel;
                l.intensity = 0f;
                l.enabled = false;
                m_lights[i] = l;
            }
        }

        /// <summary>The pool of this world: created on first use (lights under ctx.Root, registered as an updater).</summary>
        public static LocalLightPool Ensure(WorldContext ctx)
        {
            if (ctx == null) return s_current ?? (s_current = new LocalLightPool());
            if (s_current != null && s_current.m_root != null && s_current.m_root == ctx.Root) return s_current;
            var pool = new LocalLightPool(ctx.Root);
            ctx.Register(pool);
            s_current = pool;
            return pool;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics() { s_current = null; }

        // ---------------------------------------------------------------- registration (build time)
        public int AnchorCount => m_anchors.Count;
        public Anchor GetAnchor(int id) => m_anchors[id];

        /// <summary>Registers a point lamp (linear colour). Returns its id. lampBoost: the light follows the 14.2.5 portal
        /// lamp boost (atrium lamps).</summary>
        public int Add(Vector3 position, Color colorLinear, float range, float intensity, string owner = null, float rank = 1f, bool lampBoost = false)
        {
            m_anchors.Add(new Anchor { position = position, colorLinear = colorLinear, range = Mathf.Max(0.1f, range), intensity = intensity, gain = 1f, rank = rank > 0f ? rank : 1f, spotAngle = 0f, rotation = Quaternion.identity, owner = owner, lampBoost = lampBoost });
            return m_anchors.Count - 1;
        }

        /// <summary>Registers a spot lamp (full cone angle in degrees, rotation = the light's forward). Returns its id.</summary>
        public int AddSpot(Vector3 position, Quaternion rotation, float spotAngleDeg, Color colorLinear, float range, float intensity, string owner = null, float rank = 1f)
        {
            int id = Add(position, colorLinear, range, intensity, owner, rank);
            var a = m_anchors[id];
            a.spotAngle = Mathf.Clamp(spotAngleDeg, 1f, 179f);
            a.rotation = rotation;
            m_anchors[id] = a;
            return id;
        }

        /// <summary>Moves an anchor (lamp sway; no allocation).</summary>
        public void SetPosition(int id, Vector3 position) { var a = m_anchors[id]; a.position = position; m_anchors[id] = a; }

        /// <summary>Scales an anchor (0 = off and out of the pick); the slot weight follows by the time fade.</summary>
        public void SetGain(int id, float gain) { var a = m_anchors[id]; a.gain = Mathf.Max(0f, gain); m_anchors[id] = a; }

        // ---------------------------------------------------------------- state (QA)
        /// <summary>Anchor id held by slot i (-1 = empty).</summary>
        public int SlotAnchor(int i) => m_slotAnchor[i];
        /// <summary>Applied weight of slot i (0..1; the light's intensity is anchor intensity x gain x weight).</summary>
        public float SlotWeight(int i) => m_slotWeight[i];
        /// <summary>Lights with a weight above zero after the last step.</summary>
        public int LitCount => m_lit;
        /// <summary>The Light of slot i (null in a Light-free pool).</summary>
        public Light SlotLight(int i) => m_lights[i];

        // ---------------------------------------------------------------- sky openings (_BwOpen; BwLighting.hlsl BwOpeningLight)
        /// <summary>Openings published in _BwOpen (the shader loop is unrolled over this count).</summary>
        public const int MaxOpenings = 4;
        static readonly int IdOpen = Shader.PropertyToID("_BwOpen");
        readonly Vector4[] m_open = new Vector4[2 * MaxOpenings];
        readonly string[] m_openOwner = new string[MaxOpenings];
        int m_openCount;

        /// <summary>Registers a rectangular opening in the plane z = zPlane (x xMin..xMax, y yMin..yMax) whose exterior lies at
        /// z &gt; zPlane (exteriorAtPlusZ) or z &lt; zPlane; points of the interior side within reach (m) of its centre take the
        /// exterior light through it. Returns the opening id, or -1 when the 4 slots are taken (logged).</summary>
        public int AddOpening(string owner, float xMin, float xMax, float yMin, float yMax, float zPlane, bool exteriorAtPlusZ, float reach, bool on = true)
        {
            if (m_openCount >= MaxOpenings) { Debug.LogWarning("[BW] LocalLightPool: no free opening slot for '" + owner + "' (" + MaxOpenings + " taken)"); return -1; }
            int id = m_openCount++;
            m_openOwner[id] = owner;
            m_open[2 * id] = new Vector4(Mathf.Min(xMin, xMax), Mathf.Max(xMin, xMax), Mathf.Min(yMin, yMax), Mathf.Max(yMin, yMax));
            m_open[2 * id + 1] = new Vector4(zPlane, exteriorAtPlusZ ? 1f : -1f, Mathf.Max(0.5f, reach), on ? 1f : 0f);
            return id;
        }

        /// <summary>Moves the x extent of an opening (a sliding door; no allocation).</summary>
        public void SetOpeningX(int id, float xMin, float xMax)
        {
            if (id < 0 || id >= m_openCount) return;
            var r = m_open[2 * id]; r.x = Mathf.Min(xMin, xMax); r.y = Mathf.Max(xMin, xMax); m_open[2 * id] = r;
        }

        /// <summary>Switches an opening on or off (a closed door lets no light through).</summary>
        public void SetOpeningOn(int id, bool on)
        {
            if (id < 0 || id >= m_openCount) return;
            var c = m_open[2 * id + 1]; c.w = on ? 1f : 0f; m_open[2 * id + 1] = c;
        }

        public int OpeningCount => m_openCount;
        public Vector4 OpeningRect(int id) => m_open[2 * id];
        public Vector4 OpeningPlane(int id) => m_open[2 * id + 1];
        public string OpeningOwner(int id) => m_openOwner[id];

        /// <summary>Publishes _BwOpen (the pool is its only writer; a pool without openings publishes zeros = no light).</summary>
        public void PublishOpenings() { Shader.SetGlobalVectorArray(IdOpen, m_open); }

        /// <summary>C# twin of BwOpeningLight's geometric factor for one opening: the cosine-weighted form factor F of the
        /// rectangle seen from p with normal n (vector irradiance by Lambert's polygon formula, F = n . Phi / 2 pi, clamped
        /// to 0..1), times the interior-side ramp (0 on the exterior side, 1 from 0.25 m inside the plane), the sill ramp
        /// (0 from 1 m below the opening's lower edge: no light through the floor into a stairwell) and the reach fade
        /// (1 - smoothstep(0.7 reach, reach, distance to the centre)). Phi also gives the mean direction of the light.</summary>
        public static double OpeningFactor(Vector4 rect, Vector4 plane, D3 n, D3 p, out D3 phi)
        {
            phi = new D3(0, 0, 0);
            if (plane.w < 0.5f) return 0.0;
            double zp = plane.x;
            double inside = Clamp01((zp - p.z) * plane.y / 0.25) * Clamp01(1.0 + p.y - rect.z);
            var c = new D3(0.5 * (rect.x + rect.y), 0.5 * (rect.z + rect.w), zp);
            double dist = (c - p).Length, reach = Math.Max(plane.z, 1e-3);
            if (inside <= 0.0 || dist > reach) return 0.0;
            var v = new D3[4];
            v[0] = (new D3(rect.x, rect.z, zp) - p).Normalized; v[1] = (new D3(rect.y, rect.z, zp) - p).Normalized;
            v[2] = (new D3(rect.y, rect.w, zp) - p).Normalized; v[3] = (new D3(rect.x, rect.w, zp) - p).Normalized;
            for (int i = 0; i < 4; i++) phi = phi + EdgeIrr(v[i], v[(i + 1) & 3]);
            if (phi.z * (zp - p.z) < 0.0) phi = phi * -1.0;   // the vector irradiance points through the opening
            double F = Clamp01(D3.Dot(n, phi) / (2.0 * Math.PI));
            double u = Clamp01((dist - 0.7 * reach) / (0.3 * reach));
            return F * inside * (1.0 - u * u * (3.0 - 2.0 * u));
        }

        static D3 EdgeIrr(D3 a, D3 b)
        {
            D3 x = D3.Cross(a, b);
            double l = x.Length;
            return l > 1e-9 ? x * (Math.Acos(Math.Max(-1.0, Math.Min(1.0, D3.Dot(a, b)))) / l) : x;
        }

        static double Clamp01(double x) => x < 0.0 ? 0.0 : (x > 1.0 ? 1.0 : x);

        // ---------------------------------------------------------------- per frame
        public void OnFrame(in FrameState f)
        {
            bool still = GameState.CaptureMode && !AtmosphereDriver.SequenceMode;
            float dt = Mathf.Clamp(Time.deltaTime, 0f, 0.25f);
            m_lampBoost = (float)BulwarkSchedule.LampBoost(f.s);
            Step(f.cameraPos, f.cameraForward, f.fovV > 1f ? f.fovV : 64f, dt, still);
            Apply();
            PublishOpenings();
        }

        /// <summary>One pool update for a camera pose (no allocation). steady: weights jump to their targets (stills,
        /// first frame, teleports); otherwise each slot moves toward its target by at most dt / FadeSeconds.</summary>
        public void Step(Vector3 cam, Vector3 forward, float fovV, float dt, bool steady)
        {
            if (!m_init || (cam - m_lastCam).sqrMagnitude > TeleportJump * TeleportJump) steady = true;
            m_init = true;
            m_lastCam = cam;
            Pick(cam, forward, fovV);

            if (steady)
            {
                // the pick in anchor order (deterministic, independent of the history)
                int n = 0;
                for (int i = 0; i < MaxLights; i++) { m_slotAnchor[i] = -1; m_slotWeight[i] = 0f; }
                for (int k = 0; k < MaxLights; k++)
                {
                    int best = -1, bestK = -1;
                    for (int j = 0; j < MaxLights; j++)
                    {
                        int a = m_pick[j];
                        if (a < 0 || m_target[j] <= 0f) continue;
                        bool used = false;
                        for (int i = 0; i < n; i++) if (m_slotAnchor[i] == a) { used = true; break; }
                        if (used) continue;
                        if (best < 0 || a < best) { best = a; bestK = j; }
                    }
                    if (best < 0) break;
                    m_slotAnchor[n] = best;
                    m_slotWeight[n] = m_target[bestK];
                    n++;
                }
            }
            else
            {
                float rate = FadeSeconds > 0f ? dt / FadeSeconds : 1f;
                // slots holding an anchor: toward its target (0 if it left the pick); a slot at 0 that left the pick is freed
                for (int i = 0; i < MaxLights; i++)
                {
                    int a = m_slotAnchor[i];
                    if (a < 0) continue;
                    float t = TargetOf(a);
                    m_slotWeight[i] = Mathf.MoveTowards(m_slotWeight[i], t, rate);
                    if (m_slotWeight[i] <= 0f && t <= 0f) { m_slotAnchor[i] = -1; m_slotWeight[i] = 0f; }
                }
                // picked anchors without a slot take a free one (in pick order) and ramp up from 0
                for (int j = 0; j < MaxLights; j++)
                {
                    int a = m_pick[j];
                    if (a < 0 || m_target[j] <= 0f || SlotOf(a) >= 0) continue;
                    for (int i = 0; i < MaxLights; i++)
                    {
                        if (m_slotAnchor[i] >= 0) continue;
                        m_slotAnchor[i] = a;
                        m_slotWeight[i] = Mathf.MoveTowards(0f, m_target[j], rate);
                        break;
                    }
                }
            }
            m_lit = 0;
            for (int i = 0; i < MaxLights; i++) if (m_slotAnchor[i] >= 0 && m_slotWeight[i] > 0f) m_lit++;
        }

        int SlotOf(int anchor)
        {
            for (int i = 0; i < MaxLights; i++) if (m_slotAnchor[i] == anchor) return i;
            return -1;
        }

        float TargetOf(int anchor)
        {
            for (int j = 0; j < MaxLights; j++) if (m_pick[j] == anchor) return m_target[j];
            return 0f;
        }

        /// <summary>The 4 best anchors by priority and their continuous target weights (m_pick, m_target).</summary>
        void Pick(Vector3 cam, Vector3 forward, float fovV)
        {
            int K = MaxLights + 1;
            for (int i = 0; i < K; i++) { m_pick[i] = -1; m_pickP[i] = float.MaxValue; }
            float fl = forward.magnitude;
            Vector3 fw = fl > 1e-6f ? forward / fl : Vector3.forward;
            float tanV = Mathf.Tan(0.5f * Mathf.Clamp(fovV, 1f, 170f) * Mathf.Deg2Rad);
            float halfDiag = Mathf.Atan(tanV * Mathf.Sqrt(1f + ViewAspect * ViewAspect)) * Mathf.Rad2Deg;
            for (int a = 0; a < m_anchors.Count; a++)
            {
                var an = m_anchors[a];
                if (an.gain <= 0f || an.intensity <= 0f) continue;
                float reach = an.range + ReachExtra;
                Vector3 dv = an.position - cam;
                float d2 = dv.sqrMagnitude;
                if (d2 >= reach * reach) continue;
                float d = Mathf.Sqrt(d2);
                float q = d / reach;
                float view = 1f;
                if (d > an.range)
                {
                    float c = Mathf.Clamp(Vector3.Dot(dv / d, fw), -1f, 1f);
                    float margin = Mathf.Acos(c) * Mathf.Rad2Deg - Mathf.Asin(Mathf.Clamp01(an.range / d)) * Mathf.Rad2Deg - halfDiag;
                    view = 1f + BehindPenalty * Smooth01(margin / ViewFadeDeg);
                }
                float p = q * view * an.rank;
                int slot = -1;
                for (int i = 0; i < K; i++) if (p < m_pickP[i]) { slot = i; break; }
                if (slot < 0) continue;
                for (int i = K - 1; i > slot; i--) { m_pickP[i] = m_pickP[i - 1]; m_pick[i] = m_pick[i - 1]; }
                m_pickP[slot] = p; m_pick[slot] = a;
            }
            float edge = m_pick[MaxLights] >= 0 ? m_pickP[MaxLights] : 1f + BehindPenalty;
            for (int j = 0; j < MaxLights; j++)
            {
                int a = m_pick[j];
                if (a < 0) { m_target[j] = 0f; continue; }
                var an = m_anchors[a];
                float q = (an.position - cam).magnitude / (an.range + ReachExtra);
                float wd = 1f - Smooth01((q - (1f - FadeFraction)) / FadeFraction);
                float we = 1f - Smooth01((m_pickP[j] - (edge - FadeFraction)) / FadeFraction);
                m_target[j] = Mathf.Clamp01(wd * we);
            }
        }

        static float Smooth01(float x) { x = Mathf.Clamp01(x); return x * x * (3f - 2f * x); }

        /// <summary>Writes the slots to the Light components (no allocation; properties set only when they change).</summary>
        public void Apply()
        {
            for (int i = 0; i < MaxLights; i++)
            {
                var l = m_lights[i];
                if (l == null) continue;
                int a = m_slotAnchor[i];
                float w = m_slotWeight[i];
                if (a < 0 || w <= 1e-4f) { if (l.enabled) l.enabled = false; continue; }
                var an = m_anchors[a];
                var type = an.spotAngle > 0f ? LightType.Spot : LightType.Point;
                if (l.type != type) l.type = type;
                if (type == LightType.Spot) { l.spotAngle = an.spotAngle; l.innerSpotAngle = 0.7f * an.spotAngle; l.transform.SetPositionAndRotation(an.position, an.rotation); }
                else l.transform.position = an.position;
                l.color = an.colorLinear.gamma;
                l.range = an.range;
                l.intensity = an.intensity * an.gain * w * (an.lampBoost ? m_lampBoost : 1f);
                if (!l.enabled) l.enabled = true;
            }
        }

        /// <summary>Light-weighted irradiance proxy at a point from the current slots (URP 1/d^2 with the smooth range
        /// window), for continuity tests: sum of intensity x gain x weight x attenuation.</summary>
        public float Irradiance(Vector3 p)
        {
            float e = 0f;
            for (int i = 0; i < MaxLights; i++)
            {
                int a = m_slotAnchor[i];
                if (a < 0) continue;
                var an = m_anchors[a];
                e += an.intensity * an.gain * m_slotWeight[i] * (an.lampBoost ? m_lampBoost : 1f) * Attenuation((p - an.position).magnitude, an.range);
            }
            return e;
        }

        /// <summary>URP punctual distance attenuation: 1/d^2 times (1 - (d/range)^4)^2.</summary>
        public static float Attenuation(float d, float range)
        {
            float d2 = Mathf.Max(d * d, 1e-4f), f = d2 / Mathf.Max(range * range, 1e-6f), sm = Mathf.Max(0f, 1f - f * f);
            return sm * sm / d2;
        }
    }
}
