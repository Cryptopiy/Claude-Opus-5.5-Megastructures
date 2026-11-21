// KrakenCreature: builds and draws the kraken (Addendum A1, kraken owner): the Tripo body in 4 LODs (330k / 80k / 20k / 4k
// triangles, shared smooth normals so a LOD change never changes the shading), the 10 procedural tentacles in 3 LODs
// (KrakenTentacleMesh), ten procedural eyeballs in the carved sockets (BwCreatureEye), and the GPU skinning buffer _KrCtrl
// that KrakenRig fills. LOD selection is a pure function of the camera pose (projected size, with a dithered cross-fade band
// of x0.8..x1.25 around each threshold: complementary blue-noise masks, no pop, reproducible in captures).
// Usage (the choreography, next wave; the QA rig now):
//     var kc = KrakenCreature.Build(ctx, parent);           // once, behind the load fade
//     kc.Rig.SetRoot(...); kc.Rig.SetPresetWeight(...); ...; // per frame, from (t, s)
//     kc.Rig.Evaluate(f.ambientTime);
//     kc.Sync(f.cameraPos, f.fovV, f.renderHeight, f.ambientTime, lookTarget, lookWeight);
// Underwater rendering (the window-portal builder): KrakenCreature.SetUnderwater(weight, seaSurfaceY, emissionKeep, airPath).
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Megastructure.Creature
{
    public sealed class KrakenCreature
    {
        public const string BodyShader = "Megastructure/BwCreature";
        public const string EyeShader = "Megastructure/BwCreatureEye";
        // body LOD thresholds: projected head radius in pixels (LOD i above Th[i])
        public static readonly float[] BodyLodPx = { 420f, 130f, 36f };
        // tube LOD thresholds: projected radius of the limb's nearest point in pixels (LOD0 = the relief geometry, +-7 % of
        // the radius: worth its vertices only above ~36 px, where the relief is ~2.5 px; 16 px cost 0.6 ms more at 4K)
        public static readonly float[] TubeLodPx = { 36f, 2.5f };
        /// <summary>Glow of the lesser eyes relative to the main eye (fix wave X2, KS17 "the lesser eye reads as a nose").</summary>
        public const float LesserEyeGlow = 0.3f;

        public KrakenRig Rig { get; private set; }
        public KrakenAssets Assets { get; private set; }
        public Transform Root { get; private set; }
        public Bounds HeadLocal { get; private set; }
        public int ForceLod = -1;                 // QA: force a body / tube LOD (-1 = automatic)
        /// <summary>The skinning buffer and limb tables are shader globals, so only one kraken can draw at a time. When set
        /// (the QA rig sets itself), every other instance hides itself in Sync.</summary>
        public static KrakenCreature Exclusive;
        public int Triangles { get; private set; }
        public bool Visible = true;

        readonly MeshRenderer[] m_body = new MeshRenderer[KrakenAssets.BodyLods];
        readonly Material[] m_bodyMat = new Material[KrakenAssets.BodyLods];
        readonly MeshRenderer[,] m_tube = new MeshRenderer[KrakenRig.ChainCount, KrakenTentacleMesh.Lods];
        readonly Material[,] m_tubeMat = new Material[KrakenRig.ChainCount, KrakenTentacleMesh.Lods];
        /// <summary>Size of the shader's limb arrays (BwCreatureSkin.hlsl KR_MAX_LIMBS).</summary>
        public const int MaxChains = 32;
        readonly List<Transform> m_eyes = new List<Transform>();
        readonly List<Vector3> m_eyeCentre = new List<Vector3>();
        readonly List<Vector3> m_eyeNormal = new List<Vector3>();
        readonly List<float> m_eyeRadius = new List<float>();
        readonly List<Material> m_eyeMats = new List<Material>();
        readonly List<string> m_eyeName = new List<string>();
        KrakenCascade m_cascade;                   // fix wave X2 (S2-3): the water pouring off the emerged parts
        readonly List<Vector3> m_eyeDir = new List<Vector3>();      // follower state (world look direction)
        readonly List<float> m_pupil = new List<float>();           // 0 slit (light) .. 1 round (dark)
        readonly List<float> m_blinkOverride = new List<float>();   // < 0 = the automatic slow blink
        /// <summary>Wetness of the head's emerged skin (1 = fresh out of the water).</summary>
        public float HeadWetness = 1f;
        /// <summary>Eye tracking (A1 1.2): at most this many deg/s, with this lag (s); bypassed in capture stills.</summary>
        public float EyeMaxRate = 10f, EyeLatency = 1.5f;
        static readonly int IdEyeC = Shader.PropertyToID("_EyeC");
        static readonly int IdSockN = Shader.PropertyToID("_SockN");
        static readonly int IdSockU = Shader.PropertyToID("_SockU");
        static readonly int IdBlink = Shader.PropertyToID("_Blink");
        static readonly int IdPupil = Shader.PropertyToID("_Pupil");
        ComputeBuffer m_buf;
        Vector4[] m_pack;
        readonly Vector4[] m_limb = new Vector4[MaxChains], m_limb2 = new Vector4[MaxChains];

        static readonly int IdCtrl = Shader.PropertyToID("_KrCtrl");
        static readonly int IdLimb = Shader.PropertyToID("_KrLimb");
        static readonly int IdLimb2 = Shader.PropertyToID("_KrLimb2");
        static readonly int IdMantle = Shader.PropertyToID("_KrMantle");
        static readonly int IdHead = Shader.PropertyToID("_KrHead");
        static readonly int IdHeadAxis = Shader.PropertyToID("_KrHeadAxis");
        static readonly int IdPhase = Shader.PropertyToID("_KrPhase");
        static readonly int IdUnderwater = Shader.PropertyToID("_KrUnderwater");
        static readonly int IdTile = Shader.PropertyToID("_KrTileInfo");
        static readonly int IdFade = Shader.PropertyToID("_KrFade");
        static readonly int IdTube = Shader.PropertyToID("_KrTube");

        /// <summary>Global underwater mode of the creature shaders (14.7 H3 object model L0 exp(-c r) + veil (1 - exp(-c r))).
        /// weight 0 = off (air, BwFogApply); airPath = metres of the view ray in air before the water (the glass distance).</summary>
        public static void SetUnderwater(float weight, float seaSurfaceY, float emissionKeep, float airPath)
        {
            Shader.SetGlobalVector(IdUnderwater, new Vector4(Mathf.Clamp01(weight), seaSurfaceY, Mathf.Clamp01(emissionKeep), Mathf.Max(0f, airPath)));
        }

        public static KrakenCreature Build(WorldContext ctx, Transform parent)
        {
            var kc = new KrakenCreature();
            kc.BuildInternal(ctx, parent);
            return kc;
        }

        void BuildInternal(WorldContext ctx, Transform parent)
        {
            Assets = KrakenAssets.Load();
            if (!Assets.Ok) { Debug.LogError("[BW-FAIL] kraken assets: " + Assets.Error); return; }
            Rig = new KrakenRig(Assets.Manifest);
            var go = new GameObject("Kraken");
            Root = go.transform;
            Root.SetParent(parent, false);
            go.AddComponent<KrakenHost>().Owner = this;
            var man = Assets.Manifest;
            var hmin = new Vector3(man.headMin[0], man.headMin[1], man.headMin[2]);
            var hmax = new Vector3(man.headMax[0], man.headMax[1], man.headMax[2]);
            var hb = new Bounds(); hb.SetMinMax(hmin, hmax); HeadLocal = hb;

            var sh = Shader.Find(BodyShader);
            var esh = Shader.Find(EyeShader);
            if (sh == null || esh == null) { Debug.LogError("[BW-FAIL] kraken: shader " + (sh == null ? BodyShader : EyeShader) + " not found (RebuildAll adds it to Always Included)"); return; }
            for (int l = 0; l < KrakenAssets.BodyLods; l++)
            {
                var mat = NewMat(sh, false);
                m_bodyMat[l] = mat;
                m_body[l] = AddRenderer(Root, "kraken_body_lod" + l, Assets.Body[l], mat);
                Triangles += (int)(Assets.Body[l].GetIndexCount(0) / 3);
            }
            // kelp (A1 1.1 rule 2): anchors on the mantle sides, picked from the readable body LOD1 before its upload
            FindKelpAnchors(Assets.Body[1], man, out Vector3[] ka, out Vector3[] kn);
            Rig.BuildKelp(ka, kn);
            for (int k = 0; k < KrakenRig.LimbCount + Rig.Kelp.Length; k++)
                for (int l = 0; l < KrakenTentacleMesh.Lods; l++)
                {
                    var mesh = KrakenTentacleMesh.Build(Rig.Chain(k), l, Assets, Rig.TileRadii);
                    mesh.UploadMeshData(true);
                    var mat = NewMat(sh, true);
                    if (k >= KrakenRig.LimbCount) mat.SetFloat(IdTube, 3f);
                    m_tubeMat[k, l] = mat;
                    m_tube[k, l] = AddRenderer(Root, mesh.name, mesh, mat);
                    if (k >= KrakenRig.LimbCount) m_tube[k, l].shadowCastingMode = ShadowCastingMode.Off;   // kelp ribbons: no shadow pass
                    Triangles += (int)(mesh.GetIndexCount(0) / 3);
                }
            for (int l = 0; l < KrakenAssets.BodyLods; l++) Assets.Body[l].UploadMeshData(true);
            // eyes (A1 1.2): per side a main eye (20 m eyeball) and three lesser eyes (8, 6 milky, 4 m) on an arc above-behind
            var eyeMesh = KrakenTentacleMesh.EyeSphere();
            eyeMesh.UploadMeshData(true);
            float eyeZ = 0f; int mains = 0;
            foreach (var e in man.eyes)
            {
                var c = new Vector3(e.centre[0], e.centre[1], e.centre[2]);
                var n = new Vector3(e.normal[0], e.normal[1], e.normal[2]).normalized;
                var t = new GameObject("kraken_eye_" + e.name).transform;
                t.SetParent(Root, false);
                t.localPosition = c;
                t.localRotation = Quaternion.LookRotation(n, Vector3.up);
                t.localScale = Vector3.one * e.radius;
                var mr = t.gameObject.AddComponent<MeshRenderer>();
                t.gameObject.AddComponent<MeshFilter>().sharedMesh = eyeMesh;
                var em = new Material(esh) { name = "BwCreatureEye_" + e.name };
                em.SetFloat("_Milky", e.name.Contains("milky") ? 1f : 0f);
                // fix wave X2 (KS17: the 8 m lesser eye sits right above the main eye and, glowing as bright, read as a nose):
                // the lesser eyes glow at LesserEyeGlow of the main eye, so the main eye is the one that looks
                if (!e.name.StartsWith("main"))
                {
                    em.SetFloat("_Glow", em.GetFloat("_Glow") * LesserEyeGlow);
                    em.SetFloat("_GlowWater", em.GetFloat("_GlowWater") * LesserEyeGlow);
                }
                mr.sharedMaterial = em;
                Setup(mr);
                mr.shadowCastingMode = ShadowCastingMode.Off;      // inside its socket: the socket casts the shadow
                m_eyes.Add(t); m_eyeCentre.Add(c); m_eyeNormal.Add(n); m_eyeRadius.Add(e.radius); m_eyeMats.Add(em);
                m_eyeName.Add(e.name); m_eyeDir.Add(Vector3.zero); m_pupil.Add(0f); m_blinkOverride.Add(-1f);
                if (e.name.StartsWith("main")) { eyeZ += c.z; mains++; }
                Triangles += (int)(eyeMesh.GetIndexCount(0) / 3);
            }
            if (mains > 0) eyeZ /= mains;
            // no beak: Addendum A1 1.1 rule 9 ("no beak shown"); KrakenTentacleMesh.Beak() stays for a future decision
            // static globals
            for (int k = 0; k < KrakenRig.ChainCount; k++)
            {
                var L = Rig.Chain(k);
                if (L == null) { m_limb[k] = new Vector4(1f, 1f, 1f, KrakenRig.ControlPoints); m_limb2[k] = new Vector4(0f, 1f, 1f, 0f); continue; }
                m_limb[k] = new Vector4(L.Length, L.MapLambda, L.MapA, L.K);
                m_limb2[k] = new Vector4(L.Feeding ? 0f : L.U0, L.R0, L.Span, L.Wetness);
            }
            for (int k = KrakenRig.ChainCount; k < MaxChains; k++) { m_limb[k] = new Vector4(1f, 1f, 1f, KrakenRig.ControlPoints); m_limb2[k] = new Vector4(0f, 1f, 1f, 0f); }
            Shader.SetGlobalVector(IdHead, new Vector4(hb.center.x, hb.center.y, hb.center.z, Mathf.Max(hb.extents.x, hb.extents.z)));
            var axis = new Vector3(0f, 0.42f, -1f).normalized;
            Shader.SetGlobalVector(IdHeadAxis, new Vector4(axis.x, axis.y, axis.z, eyeZ - 8f));
            Shader.SetGlobalVector(IdTile, new Vector4(Rig.TileRadii, KrakenTentacleMesh.ReliefScale, 0f, 0f));
            SetUnderwater(0f, -8f, 0.35f, 0f);
            m_cascade = KrakenCascade.Build(Root);
            if (m_cascade != null) Triangles += m_cascade.Triangles;
            m_pack = new Vector4[KrakenRig.ChainCount * KrakenRig.ControlPoints * 3];
            m_buf = new ComputeBuffer(m_pack.Length, 16, ComputeBufferType.Structured);
            Rig.Pack(m_pack);
            m_buf.SetData(m_pack);
            Shader.SetGlobalBuffer(IdCtrl, m_buf);
            if (ctx != null)
            {
                ctx.Triangles += Triangles;
                ctx.Log($"kraken: built ({Triangles} triangles in all LODs, {m_eyes.Count} eyes, {KrakenRig.LimbCount} limbs + {Rig.Kelp.Length} kelp x {KrakenRig.ControlPoints} control points)");
            }
        }

        /// <summary>Kelp anchors on the mantle's lower flanks (rigid head vertices 35-80 m above the crown, facing outward,
        /// away from the eyes and the face), spread by farthest-point sampling. Deterministic.</summary>
        static void FindKelpAnchors(Mesh m, KrakenManifest man, out Vector3[] pos, out Vector3[] nrm)
        {
            pos = new Vector3[0]; nrm = new Vector3[0];
            if (m == null || !m.isReadable) return;
            var v = m.vertices; var n = m.normals; var cols = new List<Color>(); m.GetColors(cols);
            var hc = new Vector3(0.5f * (man.headMin[0] + man.headMax[0]), 0f, 0.5f * (man.headMin[2] + man.headMax[2]));
            var cand = new List<int>();
            for (int i = 0; i < v.Length; i++)
            {
                if (cols[i].r < 200f || v[i].y < 35f || v[i].y > 80f || Mathf.Abs(n[i].y) > 0.45f || v[i].z > 25f) continue;
                Vector3 rad = new Vector3(v[i].x - hc.x, 0f, v[i].z - hc.z);
                if (rad.sqrMagnitude < 1f || Vector3.Dot(new Vector3(n[i].x, 0f, n[i].z).normalized, rad.normalized) < 0.5f) continue;
                bool nearEye = false;
                foreach (var e in man.eyes)
                    if (Vector3.Distance(v[i], new Vector3(e.centre[0], e.centre[1], e.centre[2])) < 2.2f * e.radius + 8f) { nearEye = true; break; }
                if (!nearEye) cand.Add(i);
            }
            if (cand.Count == 0) return;
            int count = Mathf.Min(KrakenRig.KelpCount, cand.Count);
            pos = new Vector3[count]; nrm = new Vector3[count];
            var minD = new float[cand.Count];
            int first = 0;
            for (int c = 1; c < cand.Count; c++) if (v[cand[c]].x > v[cand[first]].x) first = c;
            for (int c = 0; c < cand.Count; c++) minD[c] = float.MaxValue;
            int pick = first;
            for (int j = 0; j < count; j++)
            {
                pos[j] = v[cand[pick]]; nrm[j] = n[cand[pick]];
                int best = 0; float bd = -1f;
                for (int c = 0; c < cand.Count; c++)
                {
                    minD[c] = Mathf.Min(minD[c], Vector3.Distance(v[cand[c]], pos[j]));
                    if (minD[c] > bd) { bd = minD[c]; best = c; }
                }
                pick = best;
            }
        }

        Material NewMat(Shader sh, bool tube)
        {
            var m = new Material(sh) { name = tube ? "BwCreature_tube" : "BwCreature_body", enableInstancing = false };
            m.SetTexture("_AlbedoMap", Assets.Albedo); m.SetTexture("_NormalMap", Assets.Normal); m.SetTexture("_MaskMap", Assets.Mask);
            m.SetTexture("_TentAlbedo", Assets.TentAlbedo); m.SetTexture("_TentNormal", Assets.TentNormal); m.SetTexture("_TentMask", Assets.TentMask);
            m.SetFloat(IdTube, tube ? 1f : 0f);
            m.SetVector(IdFade, new Vector4(1f, 0f, 0f, 0f));
            return m;
        }

        static MeshRenderer AddRenderer(Transform parent, string name, Mesh mesh, Material mat)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            var mr = go.AddComponent<MeshRenderer>();
            mr.sharedMaterial = mat;
            Setup(mr);
            return mr;
        }

        static void Setup(MeshRenderer mr)
        {
            mr.shadowCastingMode = ShadowCastingMode.On;
            mr.receiveShadows = true;
            mr.lightProbeUsage = LightProbeUsage.Off;
            mr.reflectionProbeUsage = ReflectionProbeUsage.Off;
            mr.allowOcclusionWhenDynamic = false;
            mr.motionVectorGenerationMode = MotionVectorGenerationMode.ForceNoMotion;
        }

        internal void Release()
        {
            if (m_buf != null) { m_buf.Release(); m_buf = null; }
        }

        // ---------------------------------------------------------------------------------------------- per frame
        /// <summary>After Rig.Evaluate: uploads the chain, places the root, aims the eyes, picks LODs and bounds for this camera.
        /// lookWeight 0 = the eyes wander along their sockets; 1 = they follow lookTarget (the camera: creepy).</summary>
        public void Sync(Vector3 camPos, float fovV, float renderHeight, double t, Vector3 lookTarget, float lookWeight, bool capture = true, float dt = 0f)
        {
            if (Rig == null || m_buf == null) return;
            bool show = Visible && (Exclusive == null || Exclusive == this);
            Root.gameObject.SetActive(show);
            if (!show) return;
            // the creature owns its own visibility: the ending switches every other set's renderers off once the cabin doors
            // are sealed (Beat08Ending.ApplySets: forceRenderingOff on all world roots but its KeepRoots), which took the
            // creature (under the beat-6 / show root) with them, so the stinger never drew in the real end path (lead QA S2-1)
            KeepRendering();
            var M = Rig.LocalToWorld;
            Root.SetPositionAndRotation(M.GetColumn(3), M.rotation);
            Root.localScale = Vector3.one * Rig.Scale;
            Rig.Pack(m_pack);
            m_buf.SetData(m_pack);
            Shader.SetGlobalBuffer(IdCtrl, m_buf);
            for (int k = 0; k < KrakenRig.LimbCount; k++) m_limb2[k].w = Rig.Limbs[k].Wetness;
            for (int k = KrakenRig.LimbCount; k < KrakenRig.LimbCount + Rig.Kelp.Length; k++) m_limb2[k].w = HeadWetness;
            Shader.SetGlobalVectorArray(IdLimb, m_limb);
            Shader.SetGlobalVectorArray(IdLimb2, m_limb2);
            float eyeGain = 1f + 0.10f * Mathf.Sin(2f * Mathf.PI * KrakenMath.Phase(t, 5.0));      // +-10 % at 0.2 Hz (A1 1.2; <= 0.5 Hz)
            Shader.SetGlobalVector(IdMantle, new Vector4(Rig.MantleContraction, Rig.Breathing, HeadWetness, eyeGain));
            double flow = t * 2.2;
            // x: passing-cloud bands (A1 1.3 says 0.15 Hz at 25 m/s; slowed to 0.07 Hz, 11.7 m/s by the lead's "nothing
            // quick", decision 2), y: water-sheet flow (m), z: sucker wave (period 9 s: 4.2 m/s along the arm)
            // w: the eyes' emission clamp before exposure, so that it is the A1 1.2 "HDR clamp 3.0 AFTER exposure" whatever
            // the schedule's EV (-0.9 at noon .. +1.0 in beat 6: 5.6 .. 1.5)
            float eyeClamp = 3f / Mathf.Pow(2f, Megastructure.GradeState.ExposureEV);
            Shader.SetGlobalVector(IdPhase, new Vector4(KrakenMath.Phase(t, 14.3), (float)(flow - 512.0 * System.Math.Floor(flow / 512.0)), KrakenMath.Phase(t, 9.0), eyeClamp));

            float tanHalf = Mathf.Tan(0.5f * Mathf.Max(fovV, 1f) * Mathf.Deg2Rad);
            float pxPerRad = 0.5f * Mathf.Max(renderHeight, 1f) / tanHalf;          // pixels per (size / distance)
            // body
            Bounds body = BodyBoundsWorld();
            float dHead = Mathf.Max(Vector3.Distance(camPos, M.MultiplyPoint3x4(HeadLocal.center)) - 0.5f * HeadLocal.extents.magnitude * Rig.Scale, 1f);
            float pxHead = HeadLocal.extents.magnitude * Rig.Scale / dHead * pxPerRad;
            ApplyLod(m_body, m_bodyMat, null, -1, pxHead, BodyLodPx, body);
            // tubes
            for (int k = 0; k < KrakenRig.LimbCount + Rig.Kelp.Length; k++)
            {
                var L = Rig.Chain(k);
                float best = 0f;
                float u0 = KrakenTentacleMesh.StartU(L);
                for (int i = 0; i < L.K; i++)
                {
                    if (L.U[i] < u0) continue;
                    float d = Mathf.Max(Vector3.Distance(camPos, M.MultiplyPoint3x4(L.CurP[i])), 0.5f);
                    best = Mathf.Max(best, L.Radius[i] * Rig.Scale / d);
                }
                if (!L.Visible) { for (int l = 0; l < KrakenTentacleMesh.Lods; l++) m_tube[k, l].enabled = false; continue; }
                ApplyLod(null, null, m_tube, k, best * pxPerRad, TubeLodPx, Rig.LimbBoundsWorld(k, u0, L.Length));
            }
            // eyes: wander, or track the target (<= EyeMaxRate deg/s with EyeLatency lag; snapped in capture stills), the
            // slit kept level (statocyst), automatic slow blinks per side every 20-40 s, per-eye pupil and lid state
            for (int e = 0; e < m_eyes.Count; e++)
            {
                Vector3 cw = M.MultiplyPoint3x4(m_eyeCentre[e]);
                Vector3 nw = M.MultiplyVector(m_eyeNormal[e]).normalized;
                float limit = (m_eyeRadius[e] > 7f ? 40f : 50f) * Mathf.Deg2Rad;
                float ph = e * 1.7f;
                Vector3 side = Vector3.Cross(Vector3.up, nw); if (side.sqrMagnitude < 1e-4f) side = Vector3.right; side.Normalize();
                Vector3 up = Vector3.Cross(nw, side);
                Vector3 wander = (nw + side * (0.30f * Mathf.Sin(2f * Mathf.PI * KrakenMath.Phase(t, 11.0 + e) + ph))
                                     + up * (0.16f * Mathf.Sin(2f * Mathf.PI * KrakenMath.Phase(t, 17.0 + 0.7 * e) + 2f * ph))).normalized;
                Vector3 toT = lookTarget - cw;
                Vector3 look = toT.sqrMagnitude > 1f ? Vector3.Slerp(wander, toT.normalized, Mathf.Clamp01(lookWeight)) : wander;
                float ang = Vector3.Angle(nw, look) * Mathf.Deg2Rad;
                if (ang > limit) look = Vector3.Slerp(nw, look, limit / ang).normalized;
                Vector3 cur = m_eyeDir[e];
                if (capture || cur.sqrMagnitude < 0.5f || dt <= 0f) cur = look;
                else
                {   // first-order lag (time constant = latency) with a rate cap: heavy, never twitchy
                    float a = Vector3.Angle(cur, look);
                    float stepDeg = Mathf.Min(a * (1f - Mathf.Exp(-dt / Mathf.Max(EyeLatency, 0.05f))), EyeMaxRate * dt);
                    if (a > 1e-3f) cur = Vector3.Slerp(cur, look, stepDeg / a).normalized;
                }
                m_eyeDir[e] = cur;
                m_eyes[e].rotation = Quaternion.LookRotation(cur, Vector3.up);
                var em = m_eyeMats[e];
                em.SetVector(IdEyeC, new Vector4(cw.x, cw.y, cw.z, m_eyeRadius[e] * Rig.Scale));
                em.SetVector(IdSockN, nw);
                em.SetVector(IdSockU, up);
                em.SetFloat(IdPupil, m_pupil[e]);
                float blink = m_blinkOverride[e] >= 0f ? m_blinkOverride[e] : AutoBlink(t, m_eyeName[e].EndsWith("_l") ? 1 : 0);
                em.SetFloat(IdBlink, blink);
            }
            // the water pouring off the emerged, wet parts (S2-3); nothing under water (beat 6) or once dry
            m_cascade?.Update(Rig, HeadLocal, HeadWetness, t, (float)BulwarkConst.SeaY, camPos);
        }

        /// <summary>Slow blink (A1 1.2): per side every 20-40 s, lid closing 1.2 s, hold 0.6 s, opening 1.5 s. Pure in t.</summary>
        public static float AutoBlink(double t, int side)
        {
            double period = 20.0 + 20.0 * KrakenMath.Hash01((uint)(side * 97 + 5));
            double p = t + 7.3 * side;
            double ph = p - period * System.Math.Floor(p / period);
            if (ph < 1.2) return KrakenMath.Smooth(0f, 1.2f, (float)ph);
            if (ph < 1.8) return 1f;
            if (ph < 3.3) return 1f - KrakenMath.Smooth(1.8f, 3.3f, (float)ph);
            return 0f;
        }

        /// <summary>Choreography hooks per eye (index into Assets.Manifest.eyes): pupil 0 slit .. 1 round (the A1 1.2 contraction
        /// takes 1.2 s: the caller eases it), blink override 0..1 (&lt; 0 = automatic).</summary>
        public void SetPupil(int eye, float roundness) { if ((uint)eye < (uint)m_pupil.Count) m_pupil[eye] = Mathf.Clamp01(roundness); }
        public void SetBlink(int eye, float closed) { if ((uint)eye < (uint)m_blinkOverride.Count) m_blinkOverride[eye] = closed < 0f ? -1f : Mathf.Clamp01(closed); }

        Renderer[] m_allRenderers;
        /// <summary>Clears forceRenderingOff (set by another owner's bulk switch-off) on every renderer of the creature.</summary>
        void KeepRendering()
        {
            if (m_allRenderers == null) m_allRenderers = Root.GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < m_allRenderers.Length; i++)
            {
                var r = m_allRenderers[i];
                if (r != null && r.forceRenderingOff) { r.forceRenderingOff = false; ForcedOffCleared++; }
            }
        }
        /// <summary>Renderers whose forceRenderingOff this creature cleared (diagnostics).</summary>
        public int ForcedOffCleared { get; private set; }

        /// <summary>QA diagnostics (the stinger in the real end path): active state, enabled / culled renderers, the bounds and
        /// material of the first enabled renderer of a limb, and the creature shader globals.</summary>
        public string DebugState(int limb)
        {
            if (Root == null) return "no root";
            int en = 0, vis = 0;
            var sb = new System.Text.StringBuilder();
            for (int l = 0; l < KrakenAssets.BodyLods; l++) if (m_body[l] != null && m_body[l].enabled) { en++; if (m_body[l].isVisible) vis++; }
            string tube = "none";
            for (int k = 0; k < KrakenRig.ChainCount; k++)
                for (int l = 0; l < KrakenTentacleMesh.Lods; l++)
                {
                    var mr = m_tube[k, l];
                    if (mr == null || !mr.enabled) continue;
                    en++; if (mr.isVisible) vis++;
                    if (k == limb && tube == "none")
                    {
                        var m = m_tubeMat[k, l];
                        tube = $"lod{l} vis {mr.isVisible} bounds c {mr.bounds.center} s {mr.bounds.size} q {(m != null ? m.renderQueue : -1)} sh {(m != null && m.shader != null ? m.shader.name : "-")} fade {(m != null ? m.GetVector(IdFade).ToString() : "-")} layer {mr.gameObject.layer} activeH {mr.gameObject.activeInHierarchy}";
                    }
                }
            sb.Append($"forcedOffCleared {ForcedOffCleared} visible {Visible} rootActive {Root.gameObject.activeInHierarchy} exclusive {(Exclusive == null ? "none" : (Exclusive == this ? "this" : "other"))} scale {Rig?.Scale} renderers on {en} drawn {vis}; limb{limb} {tube}; ");
            sb.Append($"_BwDamClip {Shader.GetGlobalVector("_BwDamClip")} _KrUnderwater {Shader.GetGlobalVector(IdUnderwater)} _KrFadeG {Shader.GetGlobalVector(IdFade)}");
            return sb.ToString();
        }

        Bounds BodyBoundsWorld()
        {
            var M = Rig.LocalToWorld;
            Vector3 c = M.MultiplyPoint3x4(HeadLocal.center);
            var b = new Bounds(c, HeadLocal.size * Rig.Scale * 1.6f);
            for (int k = 0; k < KrakenRig.ArmCount; k++) b.Encapsulate(Rig.LimbBoundsWorld(k, 0f, Rig.Limbs[k].U0 + 10f));
            return b;
        }

        void ApplyLod(MeshRenderer[] body, Material[] bodyMat, MeshRenderer[,] tube, int limb, float px, float[] th, Bounds bounds)
        {
            int n = th.Length + 1;
            int lodA; float fadeA;
            LodBlend(px, th, ForceLod, out lodA, out fadeA);
            // shadow proxy: while the finest LOD is shown alone, the next LOD casts the shadow instead (ShadowsOnly, full
            // coverage): 4x fewer shadow-pass vertices for the body, 10x for the relief tubes; its faces lie inside the finer
            // surface (decimation, inscribed 16-gon), so the finer one is never self-shadowed by it. Inside a cross-fade band
            // both LODs cast with their complementary dithers (the union is the full shadow), as before.
            bool noShadow = tube != null && limb >= KrakenRig.LimbCount;      // kelp ribbons never cast
            bool proxy = lodA == 0 && fadeA >= 0.999f && n > 1 && !noShadow;
            for (int l = 0; l < n; l++)
            {
                MeshRenderer mr = body != null ? body[l] : tube[limb, l];
                Material mat = body != null ? bodyMat[l] : m_tubeMat[limb, l];
                bool on;
                Vector4 fade;
                var mode = ShadowCastingMode.On;
                // z = the LOD index (BwCreature: the relief tube's normals morph to smooth while it fades out)
                if (l == lodA) { on = fadeA > 0.001f; fade = new Vector4(fadeA, 0f, l, 0f); if (proxy) mode = ShadowCastingMode.Off; }
                else if (l == lodA + 1 && fadeA < 0.999f) { on = true; fade = new Vector4(fadeA, 1f, l, 0f); }
                else if (proxy && l == 1) { on = true; fade = new Vector4(1f, 0f, l, 0f); mode = ShadowCastingMode.ShadowsOnly; }
                else { on = false; fade = new Vector4(1f, 0f, l, 0f); }
                if (noShadow) mode = ShadowCastingMode.Off;
                mr.enabled = on;
                if (on) { mat.SetVector(IdFade, fade); mr.bounds = bounds; if (mr.shadowCastingMode != mode) mr.shadowCastingMode = mode; }
            }
        }

        /// <summary>Pure LOD choice: thresholds th (pixels, descending); the finer LOD lodA with coverage fadeA, the next one
        /// takes the complement inside the band x0.8 .. x1.25 of each threshold.</summary>
        public static void LodBlend(float px, float[] th, int force, out int lodA, out float fadeA)
        {
            if (force >= 0) { lodA = Mathf.Min(force, th.Length); fadeA = 1f; return; }
            for (int i = 0; i < th.Length; i++)
            {
                float hi = th[i] * 1.25f, lo = th[i] * 0.8f;
                if (px >= hi) { lodA = i; fadeA = 1f; return; }
                if (px > lo)
                {
                    lodA = i;
                    fadeA = KrakenMath.Smooth(Mathf.Log(lo), Mathf.Log(hi), Mathf.Log(px));
                    return;
                }
            }
            lodA = th.Length; fadeA = 1f;
        }

        /// <summary>World centre and radius of an eye (for the choreography's look and audio).</summary>
        public Vector3 EyeWorld(int i) => Rig.LocalToWorld.MultiplyPoint3x4(m_eyeCentre[i]);
        public int EyeCount => m_eyes.Count;
        /// <summary>Index of an eye by its manifest name ("main_r", "main_l", "l8_r", ...), -1 when absent.</summary>
        public int EyeIndex(string name) => m_eyeName.IndexOf(name);

        /// <summary>
        /// The heading (deg) that turns the socket of eye `eye` horizontally towards worldTarget for a kraken whose crown
        /// stands at rootWorld (yaw only; the sockets look sideways, A1 1.1 rule 7 "it watches": the head turns so one eye
        /// stares at the walker; from more than ~45 deg off its socket axis an eye hides in its deep socket).
        /// </summary>
        public float HeadingFacingEye(int eye, Vector3 rootWorld, Vector3 worldTarget)
        {
            if ((uint)eye >= (uint)m_eyeNormal.Count) return Rig != null ? Rig.HeadingDeg : 0f;
            return HeadingFacing(m_eyeCentre[eye], m_eyeNormal[eye], rootWorld, worldTarget);
        }

        /// <summary>Static form of <see cref="HeadingFacingEye"/> from the manifest (no creature needed).</summary>
        public static float HeadingFacingEye(KrakenManifest man, string eyeName, Vector3 rootWorld, Vector3 worldTarget)
        {
            if (man != null && man.eyes != null)
                foreach (var e in man.eyes)
                    if (e.name == eyeName)
                        return HeadingFacing(new Vector3(e.centre[0], e.centre[1], e.centre[2]), new Vector3(e.normal[0], e.normal[1], e.normal[2]), rootWorld, worldTarget);
            return 0f;
        }

        static float HeadingFacing(Vector3 eyeLocal, Vector3 eyeNormal, Vector3 rootWorld, Vector3 target)
        {
            float eyeYaw = Mathf.Atan2(eyeNormal.x, eyeNormal.z) * Mathf.Rad2Deg;
            // a few fixed-point steps: the eye's own offset from the root changes the direction to the target slightly
            float h = 0f;
            for (int it = 0; it < 3; it++)
            {
                Vector3 e = rootWorld + Quaternion.Euler(0f, h, 0f) * eyeLocal;
                Vector3 d = target - e;
                float want = Mathf.Atan2(d.x, d.z) * Mathf.Rad2Deg;
                h = Mathf.Repeat(want - eyeYaw, 360f);
            }
            return h;
        }
    }

    /// <summary>Releases the kraken's GPU buffer with its GameObject.</summary>
    public sealed class KrakenHost : MonoBehaviour
    {
        internal KrakenCreature Owner;
        void OnDestroy() { Owner?.Release(); }
    }
}
