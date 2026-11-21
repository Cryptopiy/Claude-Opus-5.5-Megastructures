// Beat06Encounter: the kraken encounter of the window gallery (Addendum A1 1.8 with LEAD DECISIONS 2-4; replaces the whale of
// 14.5.6). Staged with the kraken owner's KrakenRig / KrakenCreature (Runtime/Creature, used as it is: one creature built
// here, shown only inside the hall, s 1066-1372). Everything is a pure function of s (the rig's own slow fields run on the
// unwrapped ambient clock): the same pose at 1.5 and 2.5 m/s, reproducible in captures.
//   s 1104       far voice through the glass (Call, underwater); the snow starts to drift (Beat06Layout.SnowBulk)
//   s 1122-1165  "the light goes out": the upper veil dims to 62 % (the simplified tier of A1 1.10: no shaft-occlusion map in
//                this wave; the rig cannot place an arm across a given pane precisely enough, see the report)
//   s 1150-1262  the sucker arm (limb 5): GlideAlong pressed flat on the glass (the rig keeps it >= its radius off a plane
//                1.0 m water-side of the outer face), sliding diagonally south and up across the panes; lamp A (s 1204-1245)
//                lights it grazing; the glass creaks (GlassCreak 1196), suckers attach (SuckerPops 1206) and re-grip (1236);
//                it never lets go in view (fix wave: the plane it is pressed on comes in from the black, s 1150-1200)
//   s 1228-1250  the eyes approach in the dark (33-75 m: the water turns their amber glow green, emission kept 5 %)
//   s 1252-1280  the main eye holds abeam pane 6, 25.5 m off the glass (Beat06Layout header), tracks the walker (<= 10 deg/s,
//                40 deg limit); lamp B (1247-1287); the pupil snaps to a slit at 1256 (EyeSwell 1251, GlassCreak 1256); a
//                slow blink at 1272
//   s 1280-1372  the slow withdrawal (Withdraw preset, the eye path of Beat06Layout, <= 7.8 m/s), the strike sound ahead
//                (DamShudder 1300), the eyes sink along a 30 deg slope (KS18 at 1320: yaw 63, pitch -27 from the KS18 eye),
//                the deepest groan (1312)
// The creature is drawn with the 14.7 H3 object model (KrakenCreature.SetUnderwater: weight 1, the in-air path = the camera's
// distance to the outer glass); the portal (BwWaterPortal) keeps its pixels through SV_Depth and BwWaterPortalScatter adds the
// in-scatter in front of it.
using Megastructure.Creature;
using UnityEngine;

namespace Megastructure.Beats
{
    public sealed partial class Beat06WindowGallery : IKrakenEncounter
    {
        KrakenCreature m_kc;
        /// <summary>The frame this encounter last posed the creature (frame, s, ambient t): the show owner's KrakenDirector calls
        /// Pose through IKrakenEncounter, and this beat's own OnFrame poses it too when the director is absent or runs later;
        /// whichever comes first poses it, the other is a no-op (one pose per frame, the same pure function of s and t).</summary>
        int m_posedFrame = -1;
        double m_posedS = double.NaN, m_posedT = double.NaN;
        /// <summary>Main eyes (scaled in their sockets inside the hall, critic S2-2): transform, material, manifest radius.</summary>
        Transform[] m_mainEye;
        Material[] m_mainEyeMat;
        float[] m_mainEyeR;
        int[] m_mainEyeIdx;
        bool m_eyeScaled;
        static readonly int IdEyeC = Shader.PropertyToID("_EyeC");
        /// <summary>Main eyeball scale inside the hall (critic S2-2, T-KRAKEN iris 1296 +-80 px at 4K at KS17). The Tripo head
        /// keeps the eye centre >= 23.3 m from the glass (Beat06Layout header), so at the 25.5 m hold the manifest's 20 m eyeball
        /// shows a 9.6 m iris = 330 px at 1080p (x2 stills: 657-661 px at 4K). The eyeball is a procedural sphere sunk in the
        /// carved socket (BwCreatureEye: iris, slit and lids are angular, in the socket frame of _EyeC), so it scales about its
        /// centre without changing its look. Measured (pose ks17a1, iris mask R > 150, R > 1.3 G, width at 1080p): x1.0 330 px
        /// (master), x1.5 567, x1.6 629, x1.63 651 = 1302 at 4K (T-KRAKEN 1296 +-80); x1.95 gave 863 (the eyeball then bulges out
        /// of its socket). The front of the x1.63 eyeball (radius 16.3) stays 25.5 - 16.3 = 9.2 m behind the outer glass. Whole
        /// creature scaling cannot do it (the clearance scales with the head: x1.9 gives only +14 %). -b6-eyescale (QA).</summary>
        public const float MainEyeScale = 1.63f;
        /// <summary>Lamp A's light on the skin relative to LampSkinGain (BwWaterPortalLamp _LampGainA): the low mount and the
        /// effective water path (LampKneeA) raise the irradiance on the suckers several times, so the skin gain of lamp A drops
        /// by this factor (captures gallery-fix02 .. 02h, -b6-lampgaina 0.15 .. 0.5: 0.3 keeps the brightest rims under 240).</summary>
        public const float LampSkinGainA = 0.3f;
        /// <summary>Lamp A's effective water paths on the skin (BwWaterPortalLamp _LampKneeA / _LampSlopeA): full to the knee,
        /// the slope beyond. Pressed on the dam face (GlassPlaneLateral) the arm is 4.5-12 m of water from the glass and 3-20 m
        /// from the lamp; on top of the lamp pass the creature's own model and the portal scatter add the teal water in front.
        /// Captures (KK4 warm pixels R >= 1.3 G, R > 45): knee 3 / slope 0.25: 0; knee 8 / 0.1: 0; knee 2 / 0.15: 50 252 px;
        /// knee 1.5 / 0.1: 72 141 px, median (132, 87, 94) = R/G 1.52, with 8 877 bright pale-green sucker-rim px (G >= 1.5 R):
        /// the A1 1.8 "red underside, pale hooked suckers" seen through the water.</summary>
        public const float LampKneeA = 1.5f, LampSlopeA = 0.1f;
        Vector3 m_eyeLocal;
        bool m_wasActive;
        /// <summary>True while this encounter showed the creature last frame (the show owner's KrakenDirector shares the one
        /// creature and toggles it in its own crest windows: outside the hall it is hidden only once, on the way out).</summary>
        bool m_shownByHall;
        double m_lastS = double.NaN, m_lastT = double.NaN, m_speed = double.NaN;
        /// <summary>KrakenRig.Evaluate's GlideAlong turn of a limb (the x2 rig): fixed (PhaseC - 0.5) x GlideFixedTurn rad plus a
        /// GlideSweepAmp rad sweep of period GlideSweepPeriod s on the ambient clock (cancelled here, see PoseFrame).</summary>
        public const float GlideFixedTurn = 0.9f, GlideSweepAmp = 0.07f;
        public const double GlideSweepPeriod = 71.0;
        /// <summary>Limbs used by the staging (KrakenRig indices).</summary>
        public const int CrossLimb = 2, GlassLimb = 5;
        /// <summary>Render queue of the creature's materials in this beat (Geometry+20, after the portal's Geometry+10).</summary>
        public const int CreatureQueue = 2020;
        /// <summary>Emission kept from the water's extinction near the glass (KrakenCreature.SetUnderwater .z), faded to 0 with the
        /// eye's distance: amber close (R/G 1.7 at the hold, 15.5 m of water), green far (the water's c removes red first).</summary>
        public const float EmissionKeep = 0.18f;
        /// <summary>The survey lamps' light on the skin (BwWaterPortalLamp): one copy of each creature material, drawn after the
        /// creature (Geometry+21) with Graphics.RenderMesh while a lamp is lit.</summary>
        public const string LampShader = "Megastructure/BwWaterPortalLamp";
        public const int LampQueue = CreatureQueue + 1;
        /// <summary>Gain of the lamp light on the skin over the reveals' irradiance scale (BwWaterPortalLamp _LampGain). The
        /// reveal concrete is 2-6 m from a lamp; the arm's pressed underside is 8-20 m away and seen through 3-10 m more water
        /// (c_G 0.085: e^-1.7 .. e^-2.5 for the round trip), so it needs about 30x to read at the level of the lamp cone's
        /// in-scatter around it (captures gallery-19a at 40: the sucker rims clip toward white; gallery-20 at 30: kept).</summary>
        public const float LampSkinGain = 30f;
        /// <summary>Lamp B over lamp A on the skin: the eye hold is 25.5 m off the glass (the Tripo head, Beat06Layout header)
        /// instead of the 6 m A1 staged it at; inverse square (25/6)^2 = 17 and the extra water e^(-0.085 x 2 x 19) = 1/25 would
        /// leave the socket unlit, so lamp B gets this factor (tuned in captures; -b6-lampgainb).</summary>
        public const float LampSkinGainB = 6f;
        static readonly int IdKrFade = Shader.PropertyToID("_KrFade");
        MeshRenderer[] m_lampMr;
        Mesh[] m_lampMesh;
        Material[] m_lampSrc, m_lampMat;
        /// <summary>Draws issued by the lamp pass in the last frame (QA log).</summary>
        public int LampDraws { get; private set; }

        void BuildEncounter(WorldContext ctx, Transform root)
        {
            m_kc = KrakenCreature.Build(ctx, root);
            if (m_kc == null || m_kc.Rig == null) { m_kc = null; Debug.LogError("[BW-FAIL] Beat06: the kraken did not build (encounter off)"); return; }
            m_eyeLocal = Vector3.zero;
            foreach (var e in m_kc.Assets.Manifest.eyes)
                if (e.name == "main_r") m_eyeLocal = new Vector3(e.centre[0], e.centre[1], e.centre[2]);
            m_kc.HeadWetness = 0f;
            for (int k = 0; k < KrakenRig.LimbCount; k++) m_kc.Rig.SetWetness(k, 0f);
            // drawn after the portal (Geometry+10): only the portal's depth and the interior occlude it (14.7 H3, T6)
            foreach (var r in m_kc.Root.GetComponentsInChildren<Renderer>(true))
                if (r.sharedMaterial != null) r.sharedMaterial.renderQueue = CreatureQueue;
            m_kc.Visible = false;
            m_kc.Sync(Vector3.zero, 64f, 1080f, 0.0, Vector3.zero, 0f);
            BuildLampPass();
            // the main eyes (scaled in their sockets inside the hall)
            var eyes = new System.Collections.Generic.List<int>();
            foreach (var e in m_kc.Assets.Manifest.eyes) if (e.name.StartsWith("main")) eyes.Add(m_kc.EyeIndex(e.name));
            m_mainEye = new Transform[eyes.Count]; m_mainEyeMat = new Material[eyes.Count]; m_mainEyeR = new float[eyes.Count]; m_mainEyeIdx = eyes.ToArray();
            int j = 0;
            foreach (var e in m_kc.Assets.Manifest.eyes)
            {
                if (!e.name.StartsWith("main")) continue;
                var t = m_kc.Root.Find("kraken_eye_" + e.name);
                var mr = t != null ? t.GetComponent<MeshRenderer>() : null;
                m_mainEye[j] = t;
                m_mainEyeMat[j] = mr != null ? mr.sharedMaterial : null;
                m_mainEyeR[j] = e.radius;
                j++;
            }
            // the show owner's director yields the shared creature to this encounter in s 1066-1372 (KrakenDirector.Encounter)
            KrakenDirector.Encounter = this;
        }

        // ---- IKrakenEncounter (the show owner's KrakenDirector calls these every frame it runs)
        bool IKrakenEncounter.Wants(in FrameState f) =>
            m_kc != null && f.s >= Beat06Layout.CreatureS0 && f.s <= Beat06Layout.CreatureS1 && !CliArgs.Has("-b6-nokraken");

        void IKrakenEncounter.Pose(in FrameState f, KrakenCreature kc)
        {
            if (kc != m_kc) return;
            PoseFrame(in f);
        }

        /// <summary>Scales the main eyeballs about their centres (k = 1 restores the manifest size for the show owner).</summary>
        void ScaleMainEyes(float k)
        {
            if (m_mainEye == null) return;
            for (int i = 0; i < m_mainEye.Length; i++)
            {
                if (m_mainEye[i] == null) continue;
                m_mainEye[i].localScale = Vector3.one * (m_mainEyeR[i] * k);
                if (m_mainEyeMat[i] != null && m_mainEyeIdx[i] >= 0)
                {
                    Vector3 c = m_kc.EyeWorld(m_mainEyeIdx[i]);
                    m_mainEyeMat[i].SetVector(IdEyeC, new Vector4(c.x, c.y, c.z, m_mainEyeR[i] * m_kc.Rig.Scale * k));
                }
            }
            m_eyeScaled = k != 1f;
        }

        /// <summary>One BwWaterPortalLamp material per creature skin renderer (a copy of its BwCreature material: maps, _KrTube and
        /// _Cull carry over). The eyes (BwCreatureEye) are left out: their own emission dominates.</summary>
        void BuildLampPass()
        {
            var sh = Shader.Find(LampShader);
            if (sh == null) { Debug.LogError("[BW-FAIL] Beat06: shader " + LampShader + " not found (RebuildAll: Always Included Shaders)"); return; }
            var mrs = new System.Collections.Generic.List<MeshRenderer>();
            var meshes = new System.Collections.Generic.List<Mesh>();
            var srcs = new System.Collections.Generic.List<Material>();
            var mats = new System.Collections.Generic.List<Material>();
            foreach (var mr in m_kc.Root.GetComponentsInChildren<MeshRenderer>(true))
            {
                var src = mr.sharedMaterial;
                if (src == null || src.shader == null || src.shader.name != KrakenCreature.BodyShader) continue;
                var mf = mr.GetComponent<MeshFilter>();
                if (mf == null || mf.sharedMesh == null) continue;
                var m = new Material(src) { name = "b6_lamp_" + src.name };
                m.shader = sh;
                m.renderQueue = LampQueue;
                m.SetFloat("_LampGain", (float)CliArgs.GetDouble("-b6-lampgain", LampSkinGain));   // QA: tuning without a rebuild
                m.SetFloat("_LampGainB", (float)CliArgs.GetDouble("-b6-lampgainb", LampSkinGainB));
                m.SetFloat("_LampGainA", (float)CliArgs.GetDouble("-b6-lampgaina", LampSkinGainA));
                m.SetFloat("_LampKneeA", (float)CliArgs.GetDouble("-b6-lampknee", LampKneeA));
                m.SetFloat("_LampSlopeA", (float)CliArgs.GetDouble("-b6-lampslope", LampSlopeA));
                RuntimeMaterials.Add(m);
                mrs.Add(mr); meshes.Add(mf.sharedMesh); srcs.Add(src); mats.Add(m);
            }
            m_lampMr = mrs.ToArray(); m_lampMesh = meshes.ToArray(); m_lampSrc = srcs.ToArray(); m_lampMat = mats.ToArray();
        }

        /// <summary>Queues the lamp pass for every skin renderer the creature shows this frame (after its Sync: LOD flags,
        /// cross-fade and skinned bounds are current). Nothing is drawn while both lamps are dark.</summary>
        void DrawLampPass(double s)
        {
            LampDraws = 0;
            if (m_lampMr == null || !m_kc.Visible) return;
            if (Beat06Layout.LampA(s) + Beat06Layout.LampB(s) <= 1e-4) return;
            for (int i = 0; i < m_lampMr.Length; i++)
            {
                var mr = m_lampMr[i];
                if (mr == null || !mr.enabled || !mr.gameObject.activeInHierarchy) continue;
                var m = m_lampMat[i];
                m.SetVector(IdKrFade, m_lampSrc[i].GetVector(IdKrFade));
                var rp = new RenderParams(m)
                {
                    layer = mr.gameObject.layer,
                    worldBounds = mr.bounds,
                    shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off,
                    receiveShadows = false,
                    lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off,
                    reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off,
                };
                Graphics.RenderMesh(in rp, m_lampMesh[i], 0, mr.localToWorldMatrix);
                LampDraws++;
            }
        }

        void EncounterFrame(in FrameState f, bool inHall)
        {
            if (m_kc == null) return;
            double s = f.s;
            if (!inHall)
            {
                if (m_wasActive)
                {
                    KrakenCreature.SetUnderwater(0f, -8f, 0.35f, 0f);
                    Shader.SetGlobalVector(IdEye, Vector4.zero);
                    m_wasActive = false;
                }
                // hide it only if the hall showed it last frame: outside s 1066-1372 the creature belongs to the show owner
                if (m_eyeScaled) ScaleMainEyes(1f);
                if (m_shownByHall) { m_kc.Visible = false; m_kc.Sync(f.cameraPos, f.fovV, f.renderHeight, f.ambientTime, f.cameraPos, 0f); m_shownByHall = false; }
                m_lastS = s;
                return;
            }
            PoseFrame(in f);
        }

        /// <summary>One pose of the creature for this frame (idempotent within a frame: see m_posedFrame).</summary>
        void PoseFrame(in FrameState f)
        {
            if (m_kc == null) return;
            double s = f.s;
            if (m_posedFrame == Time.frameCount && s == m_posedS && f.ambientTime == m_posedT) return;
            m_posedFrame = Time.frameCount; m_posedS = s; m_posedT = f.ambientTime;
            m_wasActive = true;
            m_kc.Visible = !CliArgs.Has("-b6-nokraken");   // QA: the water without the creature
            m_shownByHall = true;
            var rig = m_kc.Rig;
            rig.ResetInputs();
            // the game has ONE creature (its _KrCtrl buffer and limb arrays are globals): the show owner's KrakenDirector shares
            // this one (it finds it by its KrakenHost, poses it only in its own windows and hides the kraken in s 1060-1380);
            // so every frame of the hall restores what the show may have changed: scale, wetness (dry slime under water)
            rig.Scale = 1f;
            m_kc.HeadWetness = 0f;
            for (int k = 0; k < KrakenRig.LimbCount; k++) rig.SetWetness(k, 0f);

            // ---- presets: lurk (the remainder), the glass arm, the withdrawal
            double glass = Beat06Layout.GlassArm(s), withdraw = Beat06Layout.Withdraw(s);
            rig.SetPresetWeight(KrakenPreset.Lurk, 0f);
            // the sucker arm is GlideAlong-masked: the rig keeps a masked limb >= its radius water-side of the glass plane (from
            // u ~170 m; before that the limb runs below the hall, inside the dam's body, where nothing can see it)
            rig.SetPresetWeight(KrakenPreset.GlideAlong, 1f);   // full clamp for the whole hall; the plane comes in (S2-4)
            rig.SetPresetWeight(KrakenPreset.Withdraw, (float)withdraw);
            rig.SetLimbMask(KrakenPreset.GlideAlong, GlassLimb, 1f);
            rig.SetAgitation(0.6f);                          // slow and heavy (LEAD DECISION 2)
            rig.SetAutoCurl(0.5f);
            // the glass plane 1.0 m water-side of the outer face (margin for the chain's spline between control points), normal
            // toward the kraken (+lateral). The arm slides along it diagonally: south (toward the walker) and up at 45 deg, so
            // it rises out of the black across the panes (A1 1.8 "rises diagonally ... presses its underside flat"). The rig
            // turns its slide by a fixed per-limb offset ((PhaseC - 0.5) 1.2 rad) plus a slow +-0.3 rad sweep: the fixed part
            // is cancelled here.
            double planeLat = Beat06Layout.GlassPlaneAt(s) + (CliArgs.GetDouble("-b6-plane", Beat06Layout.GlassPlaneLateral) - Beat06Layout.GlassPlaneLateral);
            Vector3 gp = P(1235.0, planeLat, Beat06Layout.FloorY(1235.0));
            float a = (float)(Beat06Layout.GlassArmSlideDeg * System.Math.PI / 180.0);
            Vector3 slide = (-m_tan * Mathf.Cos(a) + Vector3.up * Mathf.Sin(a)).normalized;
            // the rig's slide turn of this limb: a fixed (PhaseC - 0.5) 1.2 rad plus a slow 0.30 rad sweep of period 41 s on the
            // ambient clock (KrakenRig GlideAlong). Both are cancelled: the sweep swings the arm's far part through the panes at
            // 0.30 x 2 pi / 41 = 0.046 rad/s, i.e. 7-12 m/s at 150-250 m along the arm, even while the walker stands
            // (static sequence gallery-seq27: the arm crossed pane 5 in 4 s), against LEAD DECISION 2
            // Fix wave (critic S2-1, the KK4 regression): the kraken owner's x2 rig changed this turn to a fixed (PhaseC - 0.5)
            // 0.9 rad plus a 0.07 rad sweep of period 71 s (KrakenRig.Evaluate, glideSweep); the old cancellation (1.2 / 0.30 /
            // 41 s) left the arm turned by up to 0.3 rad, so on master it lay above the KK4 frame and lamp A missed it. Mirror
            // of KrakenRig's glideSweep (shared change request: expose it as KrakenRig.GlideTurn(limb, t)).
            var gl = rig.Limbs[GlassLimb];
            float sweep = GlideSweepAmp * Mathf.Sin(2f * Mathf.PI * KrakenMath.Phase(f.ambientTime, GlideSweepPeriod) + gl.PhaseB);
            float offsetDeg = ((gl.PhaseC - 0.5f) * GlideFixedTurn + sweep) * Mathf.Rad2Deg;
            slide = Quaternion.AngleAxis(-offsetDeg, m_right) * slide;
            rig.SetGlidePlane(gp, m_right, slide);

            // ---- the body: the main eye on its path (Beat06Layout.EyeAt), the fixed orientation of the search
            Beat06Layout.EyeAt(s, out double el, out double ey, out double ez);
            Vector3 eye = P(ez, el, ey);
            var q = Quaternion.Euler((float)Beat06Layout.BodyPitch, (float)Beat06Layout.BodyHeading, (float)Beat06Layout.BodyRoll);
            Vector3 root = eye - q * m_eyeLocal;
            rig.SetRoot(root, (float)Beat06Layout.BodyHeading, (float)Beat06Layout.BodyPitch, (float)Beat06Layout.BodyRoll);
            rig.Evaluate(f.ambientTime);
            if (glass >= 0.0)
            {
                // aim the slide so the arm crosses pane 5 at mid-height: from where it meets the glass (u 300 of the first
                // evaluation) to (s 1236, floor + 10); one refinement, so the pose stays a pure function of (s, t)
                Vector3 pc = rig.LimbPointWorld(GlassLimb, 300f);
                double sc = m_tanW + Vector3.Dot(pc, m_tan);
                double dy = Beat06Layout.FloorY(1236.0) + CliArgs.GetDouble("-b6-armh", Beat06Layout.GlassArmHeight(s)) - pc.y, ds = System.Math.Max(sc - 1236.0, 20.0);
                float a2 = Mathf.Clamp((float)System.Math.Atan2(dy, ds), -0.5f, 1.05f);
                Vector3 slide2 = (-m_tan * Mathf.Cos(a2) + Vector3.up * Mathf.Sin(a2)).normalized;
                slide2 = Quaternion.AngleAxis(-offsetDeg, m_right) * slide2;
                rig.SetGlidePlane(gp, m_right, slide2);
                rig.Evaluate(f.ambientTime);
            }

            // ---- eyes: pupil and blink keyed to s; they track the walker during the hold, wander before
            float pupil = (float)Beat06Layout.Pupil(s);
            m_kc.SetPupil(0, pupil); m_kc.SetPupil(1, pupil);
            float blink = (float)Beat06Layout.Blink(s);
            m_kc.SetBlink(0, blink); m_kc.SetBlink(1, blink);
            float look = (float)(Beat06Layout.Smooth(1226.0, 1250.0, s) * (1.0 - Beat06Layout.Smooth(1300.0, 1330.0, s)));
            float lat = (float)(Vector3.Dot(f.cameraPos, m_right) - m_planeW);
            float airPath = Mathf.Max(0f, (float)Beat06Layout.GlassOut - lat);
            // emission kept from the water's extinction by the eye's water path (the eye shader clamps the iris at 3, so a fixed
            // keep cannot give both): 18 % within 16 m (amber, R/G >= 1.5 at the hold), 0 beyond 28 m (the water's own green)
            float rEye = Mathf.Max(0f, (float)el - (float)Beat06Layout.GlassOut - 10f);
            float keep = EmissionKeep * (1f - Mathf.SmoothStep(0f, 1f, (rEye - 16f) / 12f));
            KrakenCreature.SetUnderwater(1f, -8f, keep, airPath);
            m_kc.Sync(f.cameraPos, f.fovV, f.renderHeight, f.ambientTime, f.cameraPos, look, f.capture, Time.deltaTime);
            ScaleMainEyes(1f + ((float)CliArgs.GetDouble("-b6-eyescale", MainEyeScale) - 1f) * (float)Beat06Layout.EyeScaleAt(s));
            DrawLampPass(s);

            // the eye's amber light on the snow within 7 m (A1 1.8), strongest at the hold
            Vector3 ew = m_kc.EyeWorld(0);
            float glow = (float)(0.35 * Beat06Layout.Smooth(1236.0, 1252.0, s) * (1.0 - Beat06Layout.Smooth(1290.0, 1310.0, s)));
            Shader.SetGlobalVector(IdEye, new Vector4(ew.x, ew.y, ew.z, glow));

            PublishCalls(s, f.ambientTime, ew);
            if (f.capture && s != m_lastS && CliArgs.Has("-b6-debug")) DebugLog(s, ew);
            if (f.capture && CliArgs.Has("-b6-debug-motion")) DebugMotion(s, f.ambientTime);
            m_lastS = s;
        }

        /// <summary>QA (-b6-debug): where the staging put the eye and the two staged limbs, in hall coordinates (s, lateral, y).</summary>
        void DebugLog(double s, Vector3 eye)
        {
            var sb = new System.Text.StringBuilder(256);
            sb.Append("[BW] b6 kraken s=").Append(s.ToString("0.0")).Append(" eye=").Append(Hall(eye));
            sb.Append(" root=").Append(Hall(m_kc.Rig.LocalToWorld.MultiplyPoint3x4(Vector3.zero)));
            sb.Append(" inHall=").Append(HallIntrusions(out string worst)).Append(worst).Append(" lampDraws=").Append(LampDraws);
            foreach (int limb in new[] { CrossLimb, GlassLimb })
                foreach (float u in new[] { 300f, 450f, 520f, 640f, 800f })
                    sb.Append(" L").Append(limb).Append("u").Append(u.ToString("0")).Append("=").Append(Hall(m_kc.Rig.LimbPointWorld(limb, u)));
            Debug.Log(sb.ToString());
        }

        /// <summary>QA (-b6-debug-motion, every captured frame): the glass arm's points in the panes' view, to the centimetre,
        /// and the ambient clock, so a static sequence measures their speed at a fixed s (LEAD DECISION 2: <= 8 m/s).</summary>
        void DebugMotion(double s, double t)
        {
            var sb = new System.Text.StringBuilder(200);
            sb.Append("[BW] b6 motion s=").Append(s.ToString("0.00")).Append(" t=").Append(t.ToString("0.0000"));
            foreach (float u in new[] { 300f, 450f, 520f, 640f, 800f })
            {
                Vector3 p = m_kc.Rig.LimbPointWorld(GlassLimb, u);
                sb.Append(" u").Append(u.ToString("0")).Append("=").Append(p.x.ToString("0.00")).Append(",").Append(p.y.ToString("0.00")).Append(",").Append(p.z.ToString("0.00"));
            }
            Debug.Log(sb.ToString());
        }

        /// <summary>Control points of every limb inside the hall's visible volume (s 1065-1376, lateral -6 .. outer glass + the
        /// limb radius, y trench bottom - 1 .. ceiling + 1): must be 0 (a limb there would show inside the hall or the glass).</summary>
        public int HallIntrusions(out string worst)
        {
            worst = "";
            if (m_kc == null) return 0;
            var rig = m_kc.Rig;
            var M = rig.LocalToWorld;
            int n = 0; double deepest = 0.0;
            for (int k = 0; k < KrakenRig.LimbCount; k++)
            {
                var L = rig.Limbs[k];
                if (!L.Visible) continue;
                for (int i = 0; i < L.K; i++)
                {
                    Vector3 p = M.MultiplyPoint3x4(L.CurP[i]);
                    double sp = m_tanW + Vector3.Dot(p, m_tan), lat = Vector3.Dot(p, m_right) - m_planeW;
                    if (sp < 1065.0 || sp > 1376.0) continue;
                    double fy = Beat06Layout.FloorY(System.Math.Min(System.Math.Max(sp, 1070.0), 1370.0));
                    if (p.y < fy - 9.0 || p.y > fy + 29.0) continue;
                    double into = Beat06Layout.GlassOut + L.Radius[i] * rig.Scale - lat;
                    if (lat < -6.0 || into <= 0.0) continue;
                    n++;
                    if (into > deepest) { deepest = into; worst = " worst L" + k + "u" + L.U[i].ToString("0") + "=" + Hall(p); }
                }
            }
            return n;
        }

        string Hall(Vector3 p)
        {
            double sp = m_tanW + Vector3.Dot(p, m_tan), lat = Vector3.Dot(p, m_right) - m_planeW;
            return "(" + sp.ToString("0") + "," + lat.ToString("0.0") + "," + p.y.ToString("0") + ")";
        }

        /// <summary>KrakenCalls at the A1 1.8 moments: once when s crosses each key while walking (a jump of more than 6 m, a
        /// teleport or a capture pose, publishes nothing).</summary>
        void PublishCalls(double s, double t, Vector3 eye)
        {
            KrakenCalls.Expire(t);
            // walking speed (m/s of s per ambient second), smoothed over ~1 s: the durations are scaled to it (critic S2-6)
            if (!double.IsNaN(m_lastS) && !double.IsNaN(m_lastT) && t > m_lastT && s > m_lastS && s - m_lastS <= 6.0)
            {
                double v = (s - m_lastS) / (t - m_lastT), a = 1.0 - System.Math.Exp(-(t - m_lastT));
                m_speed = double.IsNaN(m_speed) ? v : m_speed + (v - m_speed) * a;
            }
            m_lastT = t;
            if (double.IsNaN(m_lastS) || s <= m_lastS || s - m_lastS > 6.0) return;
            var c = Beat06Layout.Calls;
            for (int i = 0; i < c.Length / 3; i++)
            {
                double key = c[3 * i];
                if (!(m_lastS < key && s >= key)) continue;
                var id = (Beat06Layout.CallId)i;
                KrakenCallKind kind;
                Vector3 pos;
                switch (id)
                {
                    case Beat06Layout.CallId.GlassCreak: case Beat06Layout.CallId.EyeGroan:
                        kind = KrakenCallKind.GlassCreak; pos = P(key + 6.0, Beat06Layout.GlassOut, Beat06Layout.FloorY(key) + 6.0); break;
                    case Beat06Layout.CallId.SuckersAttach: case Beat06Layout.CallId.SuckersRegrip:
                        kind = KrakenCallKind.SuckerPops; pos = P(1235.0, Beat06Layout.GlassOut + 1.0, Beat06Layout.FloorY(1235.0) + 8.0); break;
                    // the voice and the swell outside the glass (not at the eye: at distance ~0 the audio's near-source clamp
                    // adds +3 dB and centres the pan; the audio owner's request): beyond pane 6 / ahead in the water
                    case Beat06Layout.CallId.EyeSwell:
                        kind = KrakenCallKind.EyeSwell; pos = P(1256.0, Beat06Layout.GlassOut + 12.0, Beat06Layout.FloorY(1256.0) + 8.0); break;
                    case Beat06Layout.CallId.DamShudder:
                        kind = KrakenCallKind.DamShudder; pos = P(1330.0, Beat06Layout.GlassOut, -70.0); break;
                    default:
                        kind = KrakenCallKind.Call; pos = P(key + 10.0, Beat06Layout.GlassOut + 25.0, Beat06Layout.FloorY(key) + 4.0); break;
                }
                KrakenCalls.Publish(new KrakenCall
                {
                    Kind = kind, Position = pos, Strength = (float)c[3 * i + 2], T0 = t, Duration = (float)Beat06Layout.CallDuration(i, m_speed),
                    Underwater = kind != KrakenCallKind.DamShudder,
                });
                if (CliArgs.Has("-b6-debug")) Debug.Log("[BW] b6 call " + id + " " + kind + " at s=" + s.ToString("0.0") + " t=" + t.ToString("0.00"));
            }
        }
    }
}
