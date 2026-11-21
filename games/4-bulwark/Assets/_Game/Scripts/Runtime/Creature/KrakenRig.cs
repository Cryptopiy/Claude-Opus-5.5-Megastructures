// KrakenRig: the kraken's pose model (Addendum A1, kraken owner). The choreography (next wave) drives it; KrakenCreature draws it.
//
// Limbs: 8 Tripo arms (rest centre lines from the manifest, cut at uCut ~286 m and continued by procedural tentacles to
// 1200-1500 m) and 2 feeding tentacles (1650 / 1750 m, from the mouth, a hooked club at the end). Each limb is a chain of
// K = 96 control points at non-uniform arc u_i (KrakenMath.ControlIndex; denser at the thin tip). The CURRENT chain is
// rebuilt each frame by integrating rotations along the limb, which keeps every segment's rest length exactly:
//     q_{i+1} = steer_i * (q_i * restBend_i * bend_i),   P_{i+1} = P_i + q_i * restDir_i * len_i
// restBend / restDir reproduce the Tripo arm's rest shape when nothing acts; bend_i is the sum of the animated curvature
// fields (travelling waves, tip curls, the Withdraw spiral) and steer_i turns the chain towards preset / reach directions
// over an alignment length (rope-in-a-current behaviour: smooth, heavy, never a kink).
// Presets (weights blend; the remainder up to 1 is Lurk): Lurk, Swim (mantle-first, arms streamed back, slow jet pulses of
// the mantle that open and close the arm crown), Breach (arms spread on the surface, tips curling up), ReachUp (masked limbs
// rise to ReachHeight and arch over back into the sea), Slap (masked limbs: rise, whip down, lie; driven by a phase 0..1 per
// limb), GlideAlong (masked limbs pressed on a plane, suckers towards it, sliding), Withdraw (spiral curl, body sinks by the
// choreography). Per-limb reach targets (steer towards a point up to an effector arc, the rest wraps around it).
// Everything is a pure function of the inputs and the unwrapped ambient clock t (double; phases reduced in double, so the
// float clock wrap never jumps): captures are reproducible; heavy inertia is designed into the fields (periods 11-31 s,
// alignment lengths 30-500 m) and the choreography eases its inputs (KrakenRig.Ease helpers).
// Output: Pcur / Qcur per point (kraken local), packed for the GPU by Pack() (BwCreatureSkin.hlsl layout).
using System;
using UnityEngine;

namespace Megastructure.Creature
{
    public enum KrakenPreset { Lurk = 0, Swim = 1, Breach = 2, ReachUp = 3, Slap = 4, GlideAlong = 5, Withdraw = 6 }

    public sealed class KrakenLimb
    {
        public int Index;
        public bool Feeding;
        /// <summary>Span of the power-law taper (Length - U0; the shader's _KrLimb2.z).</summary>
        public float Span => Length - (Feeding ? 0f : U0);
        public float Length, U0, R0, MapLambda, MapA = 0.45f;
        public int K;
        public float[] U, SegLen, Radius;
        public Vector3[] RestP, RestDir, CurP;
        public Quaternion[] RestQ, RestBend, CurQ;
        public float PhaseA, PhaseB, PhaseC;
        // rest radius profile of the Tripo part (u <= U0), from the manifest
        public float[] ProfU, ProfR;
        public float Wetness = 1f;
        /// <summary>Feeding tentacles are not in A1 (eight arms); built, rig-driven, hidden unless the choreography shows them.</summary>
        public bool Visible = true;
        /// <summary>A kelp streamer (flat ribbon of half-width R0 tapering to 40 %; rigid to the head, hanging in the current).</summary>
        public bool IsKelp;

        /// <summary>Radius (m) at arc u: the Tripo arm's profile up to U0, then the A1 1.2 power law
        /// r0 (1 - (u - U0) / span)^1.43 with a 0.3 m floor (0.6 m diameter), a rounded tip over the last 0.5 %, and for the
        /// feeding tentacles a swollen club over the last 9 %.</summary>
        public float RadiusAt(float u)
        {
            if (IsKelp) return R0 * (1f - 0.6f * Mathf.Clamp01(u / Mathf.Max(Length, 1f)));
            float r;
            if (!Feeding && u < U0 && ProfU != null) r = Interp(ProfU, ProfR, u);
            else r = Mathf.Max(0.3f, (float)KrakenMath.TaperRadius(u, Feeding ? 0f : U0, R0, Span));
            float tip = Mathf.Clamp01((Length - u) / (0.005f * Length));
            r *= Mathf.Sqrt(tip) * 0.85f + 0.15f * tip;
            if (Feeding) r *= 1f + 0.9f * KrakenMath.Smooth(0.86f, 0.93f, u / Length) * (1f - KrakenMath.Smooth(0.95f, 0.995f, u / Length));
            return Mathf.Max(r, 0.04f);
        }

        static float Interp(float[] xs, float[] ys, float x)
        {
            if (x <= xs[0]) return ys[0];
            int n = xs.Length;
            if (x >= xs[n - 1]) return ys[n - 1];
            int lo = 0, hi = n - 1;
            while (hi - lo > 1) { int m = (lo + hi) >> 1; if (xs[m] <= x) lo = m; else hi = m; }
            float t = (x - xs[lo]) / Mathf.Max(xs[hi] - xs[lo], 1e-6f);
            return Mathf.Lerp(ys[lo], ys[hi], t);
        }

        /// <summary>Fractional control index of arc u (the shader's KrControlIndex).</summary>
        public float IndexOf(float u) => (float)KrakenMath.ControlIndex(u, Length, MapLambda, MapA, K);

        /// <summary>Rest point of the chain at arc u (Catmull-Rom of RestP; the shader's Prest).</summary>
        public Vector3 RestAt(float u) => Eval(RestP, u);
        /// <summary>Current point at arc u (Catmull-Rom of CurP; the shader's Pcur).</summary>
        public Vector3 CurrentAt(float u) => Eval(CurP, u);

        public Quaternion RestRotAt(float u)
        {
            float x = Mathf.Clamp(IndexOf(Mathf.Max(u, 0f)), 0f, K - 1.0001f);
            int i = (int)x;
            return KrakenMath.Nlerp(RestQ[i], RestQ[Mathf.Min(i + 1, K - 1)], x - i);
        }

        Vector3 Eval(Vector3[] P, float u)
        {
            float x = Mathf.Clamp(IndexOf(Mathf.Max(u, 0f)), 0f, K - 1.0001f);
            int i = (int)x;
            float f = x - i;
            return KrakenMath.CatmullRom(P[Mathf.Max(i - 1, 0)], P[i], P[Mathf.Min(i + 1, K - 1)], P[Mathf.Min(i + 2, K - 1)], f);
        }
    }

    public sealed class KrakenRig
    {
        public const int ControlPoints = 96;
        public const int ArmCount = 8, LimbCount = 10, PresetCount = 7;
        /// <summary>Kelp streamers (A1 1.1 rule 2: 6-10 per side, 20-60 m long, 2-4 m wide, damped strings in the current):
        /// chains LimbCount .. ChainCount - 1 of the GPU buffer.</summary>
        public const int KelpCount = 16, ChainCount = LimbCount + KelpCount;
        public const float TileRadiiDefault = 10.3f;
        /// <summary>Slot of the autonomous one-tip curl cycle (s).</summary>
        public const double CurlSlot = 64.0;
        /// <summary>Arm length from the crown centre (A1 1.2).</summary>
        public const float ArmLength = 1300f;
        /// <summary>Lead decision 2 (A1 00): no visible part moves faster than this (m/s, world) at constant preset weights;
        /// the fields below are sized for it (KrakenTests.Rig_IsDeterministicContinuousAndSlow measures every preset).</summary>
        public const float MaxVisibleSpeed = 8f;

        public readonly KrakenLimb[] Limbs = new KrakenLimb[LimbCount];
        /// <summary>The kelp chains (empty until BuildKelp).</summary>
        public KrakenLimb[] Kelp = new KrakenLimb[0];
        /// <summary>Chain k of the GPU buffer: a limb (k &lt; LimbCount) or a kelp streamer (null when absent).</summary>
        public KrakenLimb Chain(int k) => k < LimbCount ? Limbs[k] : (k - LimbCount < Kelp.Length ? Kelp[k - LimbCount] : null);
        public float TileRadii = TileRadiiDefault;
        /// <summary>World direction of the slow current that streams the kelp (horizontal).</summary>
        public Vector3 Current = new Vector3(0.6f, 0f, 0.8f);

        // ------------------------------------------------------------------ inputs (the choreography's API)
        readonly float[] m_preset = new float[PresetCount];
        readonly float[,] m_mask = new float[PresetCount, LimbCount];
        readonly float[] m_slapPhase = new float[LimbCount];
        readonly Vector3[] m_reachTarget = new Vector3[LimbCount];
        readonly float[] m_reachWeight = new float[LimbCount];
        readonly float[] m_reachU = new float[LimbCount];
        readonly float[] m_twist = new float[LimbCount];      // axial roll (rad) applied over u 150-400 m (A1 1.3 Twist)
        readonly float[] m_curl = new float[LimbCount];       // extra coil of the distal 15 % (A1 1.3 Curl), 0..1
        float m_autoCurl = 1f;
        Vector3 m_planeP, m_planeN = Vector3.right, m_planeSlide = Vector3.forward;
        float m_reachHeight = 260f, m_slapHeight = 220f, m_agitation = 1f;
        /// <summary>Kraken root pose in the world (the arm crown), set by the choreography.</summary>
        public Vector3 RootPosition;
        public float HeadingDeg, PitchDeg, RollDeg;
        public float Scale = 1f;
        /// <summary>Mantle contraction 0..1 of the current frame (for the shader and the audio of the next wave).</summary>
        public float MantleContraction { get; private set; }
        public float Breathing { get; private set; }
        public double Time { get; private set; }
        /// <summary>Tests: no animated field and no steering (the chain must reproduce the rest pose exactly).</summary>
        public bool RestPoseOnly;

        /// <summary>World matrix of the kraken root (crown), including the slow heave and sway of the body.</summary>
        public Matrix4x4 LocalToWorld { get; private set; } = Matrix4x4.identity;
        public Matrix4x4 WorldToLocal { get; private set; } = Matrix4x4.identity;

        public KrakenRig(KrakenManifest man)
        {
            if (man != null && man.tentacleTileRadii > 0) TileRadii = man.tentacleTileRadii;
            for (int k = 0; k < LimbCount; k++) Limbs[k] = BuildLimb(k, man);
            ResetInputs();
            Evaluate(0.0);
        }

        /// <summary>Builds the kelp streamers hanging from anchors on the mantle (kraken local; outward = the surface normal).
        /// Lengths 20-60 m, half-widths 1-2 m, deterministic per index.</summary>
        public void BuildKelp(Vector3[] anchors, Vector3[] outward)
        {
            int n = Mathf.Min(KelpCount, anchors != null ? anchors.Length : 0);
            Kelp = new KrakenLimb[n];
            for (int j = 0; j < n; j++)
            {
                var L = new KrakenLimb { Index = LimbCount + j, K = ControlPoints, IsKelp = true, MapA = 1f };
                L.Length = 20f + 40f * KrakenMath.Hash01((uint)(j * 31 + 7));
                L.R0 = 1f + 1f * KrakenMath.Hash01((uint)(j * 17 + 3));
                L.U0 = 0f; L.MapLambda = 1e4f;
                L.PhaseA = KrakenMath.Hash01((uint)(j * 13 + 1)) * 6.2831853f;
                L.PhaseB = KrakenMath.Hash01((uint)(j * 29 + 5)) * 6.2831853f;
                L.PhaseC = KrakenMath.Hash01((uint)(j * 41 + 9));
                int K = L.K;
                L.U = new float[K]; L.SegLen = new float[K]; L.Radius = new float[K];
                L.RestP = new Vector3[K]; L.RestQ = new Quaternion[K]; L.RestDir = new Vector3[K]; L.RestBend = new Quaternion[K];
                L.CurP = new Vector3[K]; L.CurQ = new Quaternion[K];
                Vector3 outw = outward[j]; outw.y = 0f; outw = outw.sqrMagnitude > 1e-4f ? outw.normalized : Vector3.right;
                Quaternion q = Quaternion.LookRotation(Vector3.down, outw);      // hangs down, the flat face outward
                for (int i = 0; i < K; i++)
                {
                    float u = L.Length * i / (K - 1);
                    L.U[i] = u; L.Radius[i] = L.RadiusAt(u);
                    L.RestP[i] = anchors[j] + Vector3.down * u - outw * 0.5f;   // starts just inside the skin
                    L.RestQ[i] = q;
                }
                for (int i = 0; i < K - 1; i++) { L.SegLen[i] = L.Length / (K - 1); L.RestDir[i] = Vector3.forward; L.RestBend[i] = Quaternion.identity; }
                L.RestBend[K - 1] = Quaternion.identity; L.RestDir[K - 1] = Vector3.forward;
                Array.Copy(L.RestP, L.CurP, K); Array.Copy(L.RestQ, L.CurQ, K);
                Kelp[j] = L;
            }
        }

        // ---------------------------------------------------------------------------------------------- API
        /// <summary>Root (arm crown) in the world, heading of the face (deg, 0 = +z north, 90 = +x east), pitch, roll.</summary>
        public void SetRoot(Vector3 worldPos, float headingDeg, float pitchDeg = 0f, float rollDeg = 0f)
        { RootPosition = worldPos; HeadingDeg = headingDeg; PitchDeg = pitchDeg; RollDeg = rollDeg; }

        public void SetPresetWeight(KrakenPreset p, float w) { m_preset[(int)p] = Mathf.Max(0f, w); }
        public float PresetWeight(KrakenPreset p) => m_preset[(int)p];
        /// <summary>Which limbs a masked preset (ReachUp, Slap, GlideAlong) moves, 0..1 per limb (default: none).</summary>
        public void SetLimbMask(KrakenPreset p, int limb, float w) { if ((uint)limb < LimbCount) m_mask[(int)p, limb] = Mathf.Clamp01(w); }
        public void SetSlapPhase(int limb, float phase01) { if ((uint)limb < LimbCount) m_slapPhase[limb] = Mathf.Clamp01(phase01); }
        /// <summary>Per-limb reach: steer towards worldTarget up to arc effectorU (m; &lt;= 0 = 70 % of the limb), the rest
        /// wraps around it. weight 0 = off.</summary>
        public void SetReach(int limb, Vector3 worldTarget, float weight, float effectorU = -1f)
        {
            if ((uint)limb >= LimbCount) return;
            m_reachTarget[limb] = worldTarget; m_reachWeight[limb] = Mathf.Clamp01(weight);
            m_reachU[limb] = effectorU > 0 ? effectorU : 0.7f * Limbs[limb].Length;
        }
        /// <summary>GlideAlong plane in the world: a point, the normal pointing towards the kraken, the slide direction.</summary>
        public void SetGlidePlane(Vector3 worldPoint, Vector3 worldNormal, Vector3 worldSlide)
        { m_planeP = worldPoint; m_planeN = worldNormal.normalized; m_planeSlide = worldSlide.normalized; }
        public void SetReachHeight(float metresAboveCrown) { m_reachHeight = Mathf.Max(20f, metresAboveCrown); }
        public void SetSlapHeight(float metresAboveCrown) { m_slapHeight = Mathf.Max(20f, metresAboveCrown); }
        /// <summary>A1 1.3 Twist: axial roll of a limb (deg, up to +-120; ease it over ~10 s) to show the red underside.</summary>
        public void SetTwist(int limb, float deg) { if ((uint)limb < LimbCount) m_twist[limb] = Mathf.Clamp(deg, -120f, 120f) * Mathf.Deg2Rad; }
        /// <summary>A1 1.3 Curl: extra coil of the distal 15 % of a limb, 0..1 (ease it over 8-12 s).</summary>
        public void SetCurl(int limb, float c) { if ((uint)limb < LimbCount) m_curl[limb] = Mathf.Clamp01(c); }
        /// <summary>Weight of the autonomous curl cycle (one tip at a time coils over ~10 s and uncoils; 0 = off).</summary>
        public void SetAutoCurl(float w) { m_autoCurl = Mathf.Clamp01(w); }
        /// <summary>Global restlessness of the travelling waves (1 = default; 0 = still, never use 0 in the game).</summary>
        public void SetAgitation(float a) { m_agitation = Mathf.Clamp(a, 0f, 3f); }
        /// <summary>Wetness of a limb's emerged skin (1 = freshly out of the water: sheets run off; 0 = dry slime).</summary>
        public void SetWetness(int limb, float w) { if ((uint)limb < LimbCount) Limbs[limb].Wetness = Mathf.Clamp01(w); }

        public void ResetInputs()
        {
            Array.Clear(m_preset, 0, m_preset.Length);
            Array.Clear(m_mask, 0, m_mask.Length);
            Array.Clear(m_slapPhase, 0, m_slapPhase.Length);
            Array.Clear(m_reachWeight, 0, m_reachWeight.Length);
            Array.Clear(m_twist, 0, m_twist.Length);
            Array.Clear(m_curl, 0, m_curl.Length);
            m_autoCurl = 1f;
            m_preset[(int)KrakenPreset.Lurk] = 1f;
            m_agitation = 1f;
        }

        /// <summary>Ease helpers for the choreography: smootherstep 0..1 of t over [t0, t0 + dur].</summary>
        public static float Ease(double t, double t0, double dur) => KrakenMath.Smoother(0f, 1f, (float)((t - t0) / Math.Max(dur, 1e-3)));

        // ---------------------------------------------------------------------------------------------- build
        static uint Seed(int k, uint salt) => (uint)(k * 7919 + 17) ^ salt;

        KrakenLimb BuildLimb(int k, KrakenManifest man)
        {
            var L = new KrakenLimb { Index = k, K = ControlPoints, Feeding = k >= ArmCount };
            L.PhaseA = KrakenMath.Hash01(Seed(k, 0x51u)) * 6.2831853f;
            L.PhaseB = KrakenMath.Hash01(Seed(k, 0x93u)) * 6.2831853f;
            L.PhaseC = KrakenMath.Hash01(Seed(k, 0x2Fu));
            Vector3[] cp = null; Vector3[] cn = null; Vector3[] ct = null; float[] cu = null;
            if (!L.Feeding)
            {
                var a = man != null && man.arms != null && k < man.arms.Length ? man.arms[k] : null;
                if (a != null && a.u != null && a.u.Length > 3)
                {
                    int n = a.u.Length;
                    cu = a.u; cp = new Vector3[n]; cn = new Vector3[n]; ct = new Vector3[n];
                    for (int i = 0; i < n; i++) { cp[i] = KrakenAssets.V3(a.p, i); cn[i] = KrakenAssets.V3(a.n, i); ct[i] = KrakenAssets.V3(a.t, i); }
                    L.U0 = a.uCut; L.R0 = a.rCut; L.ProfU = a.u; L.ProfR = a.r;
                }
                else
                {   // fallback star arm (tests without assets)
                    float az = (k * 45f + 22.5f) * Mathf.Deg2Rad;
                    var dir = new Vector3(Mathf.Sin(az), 0f, Mathf.Cos(az));
                    cu = new[] { 0f, 286f }; cp = new[] { Vector3.zero, dir * 286f }; cn = new[] { Vector3.down, Vector3.down }; ct = new[] { dir, dir };
                    L.U0 = 286f; L.R0 = 12f; L.ProfU = new[] { 0f, 286f }; L.ProfR = new[] { 20f, 12f };
                }
                L.Length = ArmLength;                          // A1 1.2: each arm 1300 m from the crown centre
                L.MapLambda = L.Length / Mathf.Log(20f / 0.42f);
            }
            else
            {
                float side = k == ArmCount ? 1f : -1f;
                var root = new Vector3(6.5f * side, -4f, 6f);
                var dir = new Vector3(0.16f * side, -0.34f, 1f).normalized;
                cu = new[] { 0f, 40f }; cp = new[] { root, root + dir * 40f }; cn = new[] { Vector3.down, Vector3.down }; ct = new[] { dir, dir };
                L.U0 = 0f; L.R0 = 5.6f;
                L.Length = k == ArmCount ? 1650f : 1750f;
                L.Visible = false;                             // not in A1 (eight arms): the choreography may show them
                L.MapLambda = L.Length / Mathf.Log(8f / 0.5f);
            }
            int K = L.K;
            L.U = new float[K]; L.SegLen = new float[K]; L.Radius = new float[K];
            L.RestP = new Vector3[K]; L.RestQ = new Quaternion[K]; L.RestDir = new Vector3[K]; L.RestBend = new Quaternion[K];
            L.CurP = new Vector3[K]; L.CurQ = new Quaternion[K];
            for (int i = 0; i < K; i++)
            {
                float u = (float)KrakenMath.ArcOfIndex(i, L.Length, L.MapLambda, L.MapA, K);
                if (i == 0) u = 0f;
                if (i == K - 1) u = L.Length;
                L.U[i] = u;
                L.Radius[i] = L.RadiusAt(u);
                SampleCentre(cu, cp, cn, ct, u, out Vector3 p, out Vector3 nrm, out Vector3 tan);
                L.RestP[i] = p;
                L.RestQ[i] = Quaternion.LookRotation(tan, -nrm);
            }
            for (int i = 0; i < K - 1; i++)
            {
                L.SegLen[i] = Vector3.Distance(L.RestP[i + 1], L.RestP[i]);
                Quaternion inv = Quaternion.Inverse(L.RestQ[i]);
                Vector3 d = L.RestP[i + 1] - L.RestP[i];
                L.RestDir[i] = d.sqrMagnitude > 1e-12f ? (inv * d.normalized) : Vector3.forward;
                L.RestBend[i] = inv * L.RestQ[i + 1];
            }
            L.RestBend[K - 1] = Quaternion.identity; L.RestDir[K - 1] = Vector3.forward;
            Array.Copy(L.RestP, L.CurP, K); Array.Copy(L.RestQ, L.CurQ, K);
            return L;
        }

        /// <summary>Rest centre line at arc u: interpolation of the manifest samples, straight past the last one.</summary>
        static void SampleCentre(float[] cu, Vector3[] cp, Vector3[] cn, Vector3[] ct, float u, out Vector3 p, out Vector3 n, out Vector3 t)
        {
            int m = cu.Length;
            if (u >= cu[m - 1])
            {
                t = ct[m - 1].normalized;
                n = (cn[m - 1] - t * Vector3.Dot(cn[m - 1], t)).normalized;
                p = cp[m - 1] + t * (u - cu[m - 1]);
                return;
            }
            int lo = 0, hi = m - 1;
            while (hi - lo > 1) { int mid = (lo + hi) >> 1; if (cu[mid] <= u) lo = mid; else hi = mid; }
            float f = Mathf.Clamp01((u - cu[lo]) / Mathf.Max(cu[hi] - cu[lo], 1e-6f));
            p = Vector3.Lerp(cp[lo], cp[hi], f);
            t = Vector3.Lerp(ct[lo], ct[hi], f).normalized;
            Vector3 nn = Vector3.Lerp(cn[lo], cn[hi], f);
            n = (nn - t * Vector3.Dot(nn, t)).normalized;
            if (n.sqrMagnitude < 0.5f) n = Vector3.down;
        }

        // ---------------------------------------------------------------------------------------------- evaluate
        /// <summary>GlideAlong turn of a limb at ambient t (rad): the fixed fan plus the slow sweep (Evaluate and the beat-6 encounter use it).</summary>
        public static float GlideTurn(KrakenLimb L, double t) => 0.07f * Mathf.Sin(2f * Mathf.PI * KrakenMath.Phase(t, 71.0) + L.PhaseB) + (L.PhaseC - 0.5f) * 0.9f;

        /// <summary>Poses every limb for the unwrapped ambient time t (s). Pure function of the inputs and t; no allocation.</summary>
        public void Evaluate(double t)
        {
            Time = t;
            // body: slow heave and sway (huge things move slowly), on top of the choreography's root
            // (tip of a 1.3 km arm: 0.45 deg at 43 s -> 1.5 m/s; heave 2.2 m at 17 s -> 0.8 m/s)
            float heave = 2.2f * Mathf.Sin(2f * Mathf.PI * KrakenMath.Phase(t, 17.0));
            float swayP = 0.45f * Mathf.Sin(2f * Mathf.PI * KrakenMath.Phase(t, 43.0) + 0.7f);
            float swayR = 0.35f * Mathf.Sin(2f * Mathf.PI * KrakenMath.Phase(t, 53.0) + 2.1f);
            var rot = Quaternion.Euler(PitchDeg + swayP, HeadingDeg, RollDeg + swayR);
            LocalToWorld = Matrix4x4.TRS(RootPosition + Vector3.up * heave * Scale, rot, Vector3.one * Scale);
            WorldToLocal = LocalToWorld.inverse;

            // preset weights: normalise above 1, fill the rest with Lurk
            float sum = 0f;
            for (int p = 0; p < PresetCount; p++) sum += m_preset[p];
            float norm = sum > 1f ? 1f / sum : 1f;
            float lurk = m_preset[0] * norm + Mathf.Max(0f, 1f - sum);
            float swim = m_preset[1] * norm, breach = m_preset[2] * norm, reach = m_preset[3] * norm, slap = m_preset[4] * norm;
            float glide = m_preset[5] * norm, withdraw = m_preset[6] * norm;

            // jet pulse (Swim): 26 s cycle, contraction 9.1 s, slow 15 s refill (heavy: the crown opens and closes, never snaps)
            float jp = KrakenMath.Phase(t, 26.0);
            float contraction = KrakenMath.Smooth(0f, 0.35f, jp) * (1f - KrakenMath.Smooth(0.38f, 0.97f, jp));
            MantleContraction = contraction * swim + 0.15f * withdraw;
            Breathing = Mathf.Sin(2f * Mathf.PI * KrakenMath.Phase(t, 12.0));      // A1 1.3: +-4 % volume, 12 s
            float open = 1f - contraction;

            // plane in local space
            Vector3 lpP = WorldToLocal.MultiplyPoint3x4(m_planeP);
            Vector3 lpN = WorldToLocal.MultiplyVector(m_planeN).normalized;
            Vector3 lpS = WorldToLocal.MultiplyVector(m_planeSlide); lpS = (lpS - lpN * Vector3.Dot(lpS, lpN)).normalized;
            float invScale = 1f / Mathf.Max(Scale, 1e-3f);

            for (int k = 0; k < LimbCount; k++)
            {
                var L = Limbs[k];
                float mReach = reach * m_mask[3, k], mSlap = slap * m_mask[4, k], mGlide = glide * m_mask[5, k];
                float sp = m_slapPhase[k];
                float sRise = 1f - KrakenMath.Smooth(0.42f, 0.55f, sp);
                float sFall = KrakenMath.Smooth(0.45f, 0.55f, sp) * (1f - KrakenMath.Smooth(0.68f, 0.78f, sp));
                float sLie = KrakenMath.Smooth(0.70f, 0.80f, sp);
                float slapH = m_slapHeight * KrakenMath.Smooth(0f, 0.45f, sp);
                Vector3 reachT = WorldToLocal.MultiplyPoint3x4(m_reachTarget[k]);
                float reachW = m_reachWeight[k];
                float reachU = m_reachU[k] * invScale;
                float agit = m_agitation * (lurk * 1f + swim * 0.35f + breach * 0.8f + (reach + slap) * 1.1f + glide * 0.6f + withdraw * 0.25f);
                // a limb that reaches, slaps or glides is held: its own waves calm down (its tip hangs from a 300 m arch
                // or lies on a plane; free waves on top of it whipped the tip at 10-11 m/s, KrakenTests round 3)
                agit *= 1f - 0.5f * Mathf.Clamp01(mReach + mSlap + mGlide);
                float curlBase = lurk * 0.30f + swim * 0.12f - breach * 0.22f + reach * 0.32f + glide * 0.2f + withdraw * 0.1f;
                float curlLife = 0.88f + 0.12f * Mathf.Sin(2f * Mathf.PI * KrakenMath.Phase(t, 53.0 + 9.0 * L.PhaseC) + L.PhaseA);
                // gliding arms fan out (static per limb) and sweep slowly (+-4 deg at 71 s: A1 1.3 lateral sweeps <= 2 m/s)
                float glideSweep = GlideTurn(L, t);
                Vector3 glideDir = Quaternion.AngleAxis(glideSweep * Mathf.Rad2Deg, lpN) * lpS;

                // autonomous curl: one arm tip at a time coils over ~32 s and uncoils (slots of 64 s, a stepping order); the
                // lead's 8 m/s cap (A1 00 decision 2) wins over the 8-12 s of A1 1.3 (a 195 m coil in 10 s moves at ~40 m/s)
                double slot = Math.Floor(t / CurlSlot);
                int active = (int)(((long)slot * 5L) % ArmCount + ArmCount) % ArmCount;
                float bell = Mathf.Sin(Mathf.PI * KrakenMath.Phase(t, CurlSlot)); bell *= bell;
                float curlExtra = m_curl[k] + (k == active ? m_autoCurl * bell : 0f);

                Quaternion q = L.RestQ[0];
                Vector3 P = L.RestP[0];
                L.CurQ[0] = q; L.CurP[0] = P;
                for (int i = 0; i < L.K - 1; i++)
                {
                    float u = L.U[i], len = L.SegLen[i], r = L.Radius[i];
                    float x = u / L.Length;
                    float flex = Mathf.Clamp(Mathf.Sqrt(14f / Mathf.Max(r, 0.4f)), 0.6f, 1.8f);
                    float rootRamp = L.Feeding ? KrakenMath.Smooth(10f, 80f, u) : KrakenMath.Smooth(40f, 200f, u);
                    // ---- curvature fields (rad / m). Amplitudes and periods are sized for a 1.3 km lever and the lead's 8 m/s
                    // cap: each travelling wave moves the tip at ~3 m/s (v ~ kappa lambda L / T), the slow sway of the whole
                    // limb reads as enormous weight (KrakenTests.Rig_IsDeterministicContinuousAndSlow measures it)
                    float kx = agit * 0.00024f * flex * rootRamp * KrakenMath.Wave(u / 500f, t, 43.0, L.PhaseA);
                    float ky = agit * 0.00017f * flex * rootRamp * KrakenMath.Wave(u / 680f, t, 55.0, L.PhaseB);
                    float tw = agit * 0.00012f * rootRamp * KrakenMath.Wave(u / 900f, t, 61.0, L.PhaseA + L.PhaseB);
                    // tip curl: a logarithmic spiral of ~1.5 turns over the last 6 % (curvature ~ 1 / radius)
                    float tipZone = KrakenMath.Smooth(0.94f, 1f, x);
                    float curl = (curlBase + 0.5f * reachW * (u > reachU ? 1f : 0f) + mSlap * 0.25f) * curlLife;
                    kx += curl * tipZone * 0.12f / Mathf.Max(r, 0.25f);      // curl 0.3 -> ~1.4 turns
                    kx += withdraw * KrakenMath.Smooth(0.35f, 0.75f, x) / (40f * Mathf.Max(r, 0.35f));   // ~2.5 turns at full Withdraw
                    // A1 1.3 Curl: the distal 12 % coils (the choreography's SetCurl plus the autonomous one-tip cycle);
                    // 1 -> ~0.3 turn of a 156 m whip (2 rad), tip <= ~5 m/s when eased over >= 30 s
                    kx += curlExtra * KrakenMath.Smooth(0.88f, 1f, x) * 0.008f / Mathf.Max(r, 0.25f);
                    // A1 1.3 Twist: the axial roll is spread over u 150-400 m
                    tw += m_twist[k] * KrakenMath.Smooth(130f, 170f, u) * (1f - KrakenMath.Smooth(380f, 420f, u)) / 250f;
                    if (RestPoseOnly) { kx = 0f; ky = 0f; tw = 0f; }
                    Quaternion bend = Quaternion.Euler(kx * len * Mathf.Rad2Deg, ky * len * Mathf.Rad2Deg, tw * len * Mathf.Rad2Deg);
                    Quaternion qn = q * L.RestBend[i] * bend;

                    // ---- steering (direction, gain)
                    Vector3 radial = new Vector3(P.x, 0f, P.z);
                    radial = radial.sqrMagnitude > 1f ? radial.normalized : (q * Vector3.forward);
                    Vector3 acc = Vector3.zero; float gsum = 0f;
                    if (lurk > 0f) Steer(ref acc, ref gsum, lurk, len, 500f, (radial + Vector3.down * 0.35f).normalized, rootRamp);
                    // the jet pulse opens and closes the arm crown only (the first ~350 m); the long tentacles stream behind
                    float crown = 1f - KrakenMath.Smooth(250f, 500f, u);
                    // (crown opening 0.10: a 0.25 swing moved the 1.3 km tips at 14 m/s; 0.10 over 9 s keeps them under 6)
                    if (swim > 0f) Steer(ref acc, ref gsum, swim, len, 140f, (Vector3.forward + radial * (0.18f + 0.10f * open * crown) + Vector3.down * 0.05f).normalized, rootRamp);
                    if (breach > 0f) Steer(ref acc, ref gsum, breach, len, 120f, (radial + Vector3.down * 0.22f).normalized, rootRamp);
                    if (withdraw > 0f) Steer(ref acc, ref gsum, withdraw, len, 200f, (Vector3.down - radial * 0.3f).normalized, rootRamp);
                    float riseRamp = KrakenMath.Smooth(0.85f * Mathf.Max(L.U0, 60f) - 40f, 0.85f * Mathf.Max(L.U0, 60f) + 20f, u);
                    if (mReach > 0f)
                    {
                        Vector3 sway = (q * Vector3.right) * (0.08f * KrakenMath.Wave(u / 300f, t, 51.0, L.PhaseB));
                        // rise, then arch over: a smooth blend round the reach height (a hard switch on P.y flipped the
                        // steering of points that the waves carried across it: 50 m/s tip jumps, KrakenTests round 2)
                        float over = KrakenMath.Smooth(m_reachHeight - 60f, m_reachHeight + 20f, P.y);
                        Vector3 d = Vector3.Lerp((Vector3.up + radial * 0.25f + sway).normalized, (radial + Vector3.down * 0.6f).normalized, over);
                        Steer(ref acc, ref gsum, mReach, len, 60f, d.normalized, riseRamp);
                    }
                    if (mSlap > 0f)
                    {
                        float overS = KrakenMath.Smooth(slapH - 50f, slapH + 15f, P.y);
                        if (sRise > 0f) Steer(ref acc, ref gsum, mSlap * sRise, len, 60f, Vector3.Lerp((Vector3.up + radial * 0.3f).normalized, (radial + Vector3.down * 0.5f).normalized, overS).normalized, riseRamp);
                        if (sFall > 0f) Steer(ref acc, ref gsum, mSlap * sFall, len, 30f, (radial + Vector3.down * 2.5f).normalized, riseRamp);
                        if (sLie > 0f) Steer(ref acc, ref gsum, mSlap * sLie, len, 100f, (radial + Vector3.down * 0.3f).normalized, riseRamp);
                    }
                    float glideRamp = KrakenMath.Smooth(0.6f * Mathf.Max(L.U0, 60f) - 30f, 0.6f * Mathf.Max(L.U0, 60f) + 30f, u);
                    if (mGlide > 0f)
                    {
                        float dist = Vector3.Dot(P - lpP, lpN);
                        Vector3 d = glideDir - lpN * Mathf.Clamp((dist - r) / 40f, -1f, 3f);
                        Steer(ref acc, ref gsum, mGlide, len, 50f, d.normalized, glideRamp);
                    }
                    if (reachW > 0f && u <= reachU)
                    {
                        Vector3 d = reachT - P;
                        if (d.sqrMagnitude > 1f) Steer(ref acc, ref gsum, reachW, len, 80f, d.normalized, KrakenMath.Smooth(20f, 120f, u));
                    }
                    if (RestPoseOnly) gsum = 0f;
                    if (gsum > 1e-5f && acc.sqrMagnitude > 1e-10f)
                    {
                        // steering weakens smoothly when opposite pulls cancel (no flip of acc's direction), and a turn of
                        // nearly 180 deg uses the segment's own side axis (FromToRotation picks an arbitrary one there)
                        // and fades out for a segment that points straight against its pull (a coil's far side): the turn
                        // axis is ill-conditioned there (withdraw coils whipped at 16 m/s with a forced axis, round 3)
                        float coh = Mathf.Clamp01(2f * acc.magnitude / gsum);
                        Vector3 fwd = qn * Vector3.forward;
                        Vector3 aim = acc.normalized;
                        float g = Mathf.Min(gsum, 0.9f) * coh * KrakenMath.Smooth(-0.995f, -0.85f, Vector3.Dot(fwd, aim));
                        qn = Quaternion.AngleAxis(Vector3.Angle(fwd, aim) * g, StableAxis(fwd, aim, qn * Vector3.right)) * qn;
                    }
                    if (mGlide > 0f)
                    {   // roll: the sucker side (-Y) towards the plane
                        Vector3 fwd = qn * Vector3.forward;
                        Vector3 upWant = lpN - fwd * Vector3.Dot(lpN, fwd);
                        if (upWant.sqrMagnitude > 1e-6f)
                        {
                            Vector3 upNow = qn * Vector3.up;
                            float ang = Vector3.SignedAngle(upNow, upWant.normalized, fwd);
                            // fade out near +-180 deg (the sign of the roll flips there) and where fwd is along the normal
                            float g = mGlide * glideRamp * (1f - Mathf.Exp(-len / 40f))
                                    * KrakenMath.Smooth(175f, 140f, Mathf.Abs(ang)) * KrakenMath.Smooth(0.01f, 0.1f, upWant.sqrMagnitude);
                            qn = Quaternion.AngleAxis(ang * g, fwd) * qn;
                        }
                    }
                    Vector3 Pn = P + (q * L.RestDir[i]) * len;
                    if (mGlide > 0f)
                    {   // never through the plane
                        float dist = Vector3.Dot(Pn - lpP, lpN);
                        float rr = L.Radius[i + 1] * 1.02f;
                        if (dist < rr) Pn += lpN * ((rr - dist) * mGlide * glideRamp);
                    }
                    q = qn; P = Pn;
                    L.CurQ[i + 1] = q; L.CurP[i + 1] = P;
                }
            }
            EvaluateKelp(t);
        }

        /// <summary>Kelp: damped strings hanging in world-down with the slow current and a travelling sway (pure in t).</summary>
        void EvaluateKelp(double t)
        {
            if (Kelp.Length == 0) return;
            Vector3 g = WorldToLocal.MultiplyVector(Vector3.down).normalized;
            Vector3 cur = WorldToLocal.MultiplyVector(Current); cur = cur.sqrMagnitude > 1e-6f ? cur.normalized : Vector3.zero;
            float slow = 0.22f + 0.10f * Mathf.Sin(2f * Mathf.PI * KrakenMath.Phase(t, 37.0));
            for (int j = 0; j < Kelp.Length; j++)
            {
                var L = Kelp[j];
                Quaternion q = L.RestQ[0];
                Vector3 P = L.RestP[0];
                L.CurQ[0] = q; L.CurP[0] = P;
                for (int i = 0; i < L.K - 1; i++)
                {
                    float u = L.U[i], len = L.SegLen[i];
                    Quaternion qn = q * L.RestBend[i];
                    if (!RestPoseOnly)
                    {
                        float wv = KrakenMath.Wave(u / 18f, t, 13.0 + 6.0 * L.PhaseC, L.PhaseA);
                        Vector3 side = q * Vector3.right;
                        Vector3 D = (g + cur * (slow + 0.15f * L.PhaseC) + side * (0.22f * wv * KrakenMath.Smooth(0f, 8f, u))).normalized;
                        float gain = 1f - Mathf.Exp(-len / 4f);
                        Vector3 fwd = qn * Vector3.forward;
                        qn = Quaternion.Slerp(Quaternion.identity, Quaternion.FromToRotation(fwd, D), gain) * qn;
                    }
                    Vector3 Pn = P + (q * L.RestDir[i]) * len;
                    q = qn; P = Pn;
                    L.CurQ[i + 1] = q; L.CurP[i + 1] = P;
                }
            }
        }

        /// <summary>Rotation axis from a to b (unit vectors): the cross product, blended towards `side` (perpendicular to a)
        /// as the angle approaches 180 deg, so the turn direction never flips from one frame to the next.</summary>
        static Vector3 StableAxis(Vector3 a, Vector3 b, Vector3 side)
        {
            Vector3 c = Vector3.Cross(a, b);
            float w = KrakenMath.Smooth(-0.80f, -0.98f, Vector3.Dot(a, b));
            Vector3 s = side - a * Vector3.Dot(side, a);
            if (s.sqrMagnitude < 1e-8f) s = Vector3.Cross(a, Vector3.up);
            if (Vector3.Dot(s, c) < 0f) s = -s;
            Vector3 axis = (c.sqrMagnitude > 1e-12f ? c.normalized : s.normalized) * (1f - w) + s.normalized * w;
            return axis.sqrMagnitude > 1e-10f ? axis.normalized : Vector3.up;
        }

        static void Steer(ref Vector3 acc, ref float gsum, float w, float segLen, float alignLen, Vector3 dir, float ramp)
        {
            float g = w * ramp * (1f - Mathf.Exp(-segLen / Mathf.Max(alignLen, 1f)));
            acc += dir * g; gsum += g;
        }

        // ---------------------------------------------------------------------------------------------- outputs
        /// <summary>Packs every limb for _KrCtrl: per point (rest xyz, 0), (current xyz, radius), q_cur * q_rest^-1.
        /// buffer.Length must be ChainCount * ControlPoints * 3 (limbs, then kelp; absent kelp chains are identity).</summary>
        public void Pack(Vector4[] buffer)
        {
            int o = 0;
            for (int k = 0; k < ChainCount; k++)
            {
                var L = Chain(k);
                if (L == null)
                {
                    for (int i = 0; i < ControlPoints; i++)
                    {
                        buffer[o++] = Vector4.zero; buffer[o++] = Vector4.zero; buffer[o++] = new Vector4(0f, 0f, 0f, 1f);
                    }
                    continue;
                }
                for (int i = 0; i < L.K; i++)
                {
                    Vector3 r = L.RestP[i], c = L.CurP[i];
                    Quaternion d = L.CurQ[i] * Quaternion.Inverse(L.RestQ[i]);
                    buffer[o++] = new Vector4(r.x, r.y, r.z, L.U[i]);
                    buffer[o++] = new Vector4(c.x, c.y, c.z, L.Radius[i]);
                    buffer[o++] = new Vector4(d.x, d.y, d.z, d.w);
                }
            }
        }

        /// <summary>World position of arc u on a limb (the shader's centre line).</summary>
        public Vector3 LimbPointWorld(int limb, float u) => LocalToWorld.MultiplyPoint3x4(Limbs[limb].CurrentAt(u));

        /// <summary>World AABB of a limb's arc range [u0, u1] (control points plus their radius).</summary>
        public Bounds LimbBoundsWorld(int limb, float u0, float u1)
        {
            var L = Chain(limb);
            if (L == null) return new Bounds(LocalToWorld.MultiplyPoint3x4(Vector3.zero), Vector3.one);
            Vector3 mn = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue), mx = -mn;
            for (int i = 0; i < L.K; i++)
            {
                if (L.U[i] < u0 - 60f || L.U[i] > u1 + 60f) continue;
                Vector3 p = LocalToWorld.MultiplyPoint3x4(L.CurP[i]);
                float r = (L.Radius[i] * 1.5f + 2f) * Scale;
                mn = Vector3.Min(mn, p - Vector3.one * r); mx = Vector3.Max(mx, p + Vector3.one * r);
            }
            if (mn.x > mx.x) return new Bounds(LocalToWorld.MultiplyPoint3x4(Vector3.zero), Vector3.one);
            var b = new Bounds(); b.SetMinMax(mn, mx); return b;
        }

        /// <summary>
        /// Sea disturbances of the current pose (SeaDisturbances contract; the choreography publishes them): the body as
        /// Breach (above the surface) / Shadow (within 60 m below), a Wake when it moves near the surface (bodyVelocity, world
        /// m/s), and a TentacleRise where a limb crosses the surface (largest radius first). Returns the count written.
        /// </summary>
        public int CollectSeaDisturbances(float seaY, Vector3 bodyVelocity, Bounds headLocal, SeaDisturbance[] into)
        {
            int n = 0;
            if (into == null || into.Length == 0) return 0;
            Vector3 hc = LocalToWorld.MultiplyPoint3x4(headLocal.center);
            float top = hc.y + headLocal.extents.y * Scale;
            float half = Mathf.Max(headLocal.extents.x, headLocal.extents.z) * Scale;
            float heading = HeadingDeg * Mathf.Deg2Rad;
            // Breach strength follows the RISE (bodyVelocity.y): white water while the head breaks the surface, a quiet foam
            // collar (0.15) once it stands still (A1 1.3: water sheets for 6-10 s, then only the collar); a head that stood
            // 150 m out of the sea under a permanent full-strength spray column read as a white veil (look round 3)
            float rise = Mathf.Clamp01(Mathf.Abs(bodyVelocity.y) / 3f);
            if (top > seaY)
                into[n++] = new SeaDisturbance { Kind = SeaDisturbanceKind.Breach, X = hc.x, Z = hc.z, HalfAlong = half * 0.9f, HalfAcross = half * 0.75f, Heading = heading, Strength = Mathf.Clamp01(0.15f + 0.85f * rise * Mathf.Clamp01((top - seaY) / 30f + 0.3f)), HeightAboveSea = top - seaY };
            else if (top > seaY - 60f)
                into[n++] = new SeaDisturbance { Kind = SeaDisturbanceKind.Shadow, X = hc.x, Z = hc.z, HalfAlong = half * 1.1f, HalfAcross = half * 0.9f, Heading = heading, Strength = Mathf.Clamp01(1f - (seaY - top) / 60f), HeightAboveSea = 0f };
            Vector2 v = new Vector2(bodyVelocity.x, bodyVelocity.z);
            if (n < into.Length && v.magnitude > 0.5f && top > seaY - 40f)
                into[n++] = new SeaDisturbance { Kind = SeaDisturbanceKind.Wake, X = hc.x, Z = hc.z, HalfAlong = half * 2.5f, HalfAcross = half, Heading = Mathf.Atan2(v.x, v.y), Strength = Mathf.Clamp01(v.magnitude / 6f), HeightAboveSea = 0f };
            // limb crossings, thickest first (simple selection without allocation)
            for (int pass = 0; pass < 2 && n < into.Length; pass++)
            {
                for (int k = 0; k < LimbCount && n < into.Length; k++)
                {
                    var L = Limbs[k];
                    Vector3 a = LocalToWorld.MultiplyPoint3x4(L.CurP[0]);
                    for (int i = 0; i < L.K - 1 && n < into.Length; i++)
                    {
                        Vector3 b = LocalToWorld.MultiplyPoint3x4(L.CurP[i + 1]);
                        if ((a.y - seaY) * (b.y - seaY) < 0f)
                        {
                            float f = (seaY - a.y) / (b.y - a.y);
                            float r = Mathf.Lerp(L.Radius[i], L.Radius[i + 1], f) * Scale;
                            bool thick = r >= 4f;
                            if ((pass == 0) == thick)
                            {
                                // the height of the water sheath: the emerged arc that stays inside the mist owner's VERTICAL
                                // sheath (radius 1.8 r + 1.5 m round the waterline point), capped at 60 m; an arching arm is
                                // wet along its own skin (BwCreature water sheets), not under a 300 m column of falling water
                                Vector3 c = Vector3.Lerp(a, b, f);
                                float sheath = 1.8f * r + 1.5f, hmax = 0f;
                                for (int j = i + 1; j < L.K; j++)
                                {
                                    Vector3 pj = LocalToWorld.MultiplyPoint3x4(L.CurP[j]);
                                    float h = pj.y - seaY;
                                    if (h < 0f || new Vector2(pj.x - c.x, pj.z - c.z).magnitude > sheath) break;
                                    hmax = Mathf.Max(hmax, h);
                                }
                                hmax = Mathf.Min(hmax, 60f);
                                into[n++] = new SeaDisturbance { Kind = SeaDisturbanceKind.TentacleRise, X = c.x, Z = c.z, HalfAlong = r, HalfAcross = r, Heading = 0f, Strength = Mathf.Clamp(r / 10f, 0.35f, 1f), HeightAboveSea = hmax };
                            }
                        }
                        a = b;
                    }
                }
            }
            return n;
        }
    }
}
