// QA_KrakenRig: the kraken look-dev rig (Addendum A1, kraken owner). Active only when a capture run names the set "kraken"
// (-capture <dir> -capture-set kraken; QaRigs), never in the game. It builds the creature (KrakenCreature), an underwater
// booth (a 55 m sphere drawn with BwCreatureVeil, x 2600 / y -66 / z 1210, far east of the dam) and registers stills and
// sequences; per shot (QaRigs.CurrentShot) it poses the kraken for that station from the shot's ambient clock (deterministic).
// Stations (camera pose, lighting s, kraken root / heading / presets):
//   storm800 / storm2k     surfaced 800 m and ~1.8-2 km off the crest in the beat-2 storm light (s 300), tentacles rising
//                          (the far station stays inside the 2048 m sea clipmap: the far-field sea at the far plane does
//                          not hide submerged parts beyond it)
//   noon800 / noon2k       the same at noon (s 800, beat 4), the six islets in front for scale
//   islet                  ReachUp next to the beat-4 islets (reach targets beside the 40 m units)
//   crestnear              two tentacles towering 240 m right beside the crest, 60-120 m away
//   head600                A1 K4: the head risen behind the islets 597 m away, its right eye turned to the walker (noon,
//                          s 800; KrakenCreature.HeadingFacingEye)
//   eyeair                 a main eye in air at 90 m (storm light): eye, lids, socket, wet crust, the stare
//   joint                  the crown lifted 60 m out of the sea (QA only), one arm raised: the Tripo arm -> procedural
//                          tentacle joint at u 286 m seen from 80 m (camera placed at build time from the evaluated pose)
//   studio                 the whole animal hoisted 150 m above the sea (QA only), Lurk, 760 m away at noon: materials
//                          without water, spray or fog in the way
//   leeclip                the dam clip of the creature shaders (_DamClip / _BwDamClip, the show's stinger): a 2.5x animal
//                          raising four arms 2 km behind the crest, seen from the foot of the lee face 3.5 km west (the
//                          lantern platform, A1 00 decision 1): only what rises above the crest line may show
//   dolly                  sequence only (kr_seq_dolly): the camera flies from 250 m to 2400 m away from the breached head
//                          in 20 s (storm light, clock running): every body / tube LOD cross-fade, the eyes following
//   uwclose / uwfront      underwater close-ups in the booth (14.7 H3 object model): the right main eye at 32 m (3/4 view),
//                          the left main eye straight on at 28 m (heading 243: its socket normal faces the camera)
// Shots: kr_<station> (1920 x 1080); with -kraken-aspects also _1680x1050 and _2560x1080; with -kraken-4k also _3840x2160
// (pass -Long to run-player). Sequences: kr_seq_storm800 (20 s at 30 fps, clock running: motion, pops), kr_seq_uwclose
// (10 s), kr_pan_storm800 (0.5 px / frame yaw pan, frozen clock: shimmer). Flags: -kraken-aspects, -kraken-4k,
// -kraken-lod N (force a LOD for LOD captures), -kraken-perf <station> (no capture: the camera is held at the station pose
// for a -perf-log run with -progress at the station's s), -kraken-off (build it but never draw it: the perf baseline),
// -kraken-nodist (publish no SeaDisturbances: the creature without the sea's foam and the mist's spray).
using System.Collections.Generic;
using Megastructure.Creature;
using Megastructure.Player;
using Megastructure.QA;
using UnityEngine;

namespace Megastructure.Beats
{
    public sealed class QaKrakenRig : IBeatBuilder, IWorldUpdater
    {
        public string Name => "QA_KrakenRig";
        public const string Set = "kraken";
        static readonly Vector3 Booth = new Vector3(2600f, -66f, 1210f);
        const float BoothRadius = 55f;

        sealed class Station
        {
            public string key;
            public double s, ambient;
            public float cx, cy, cz, yaw, pitch, fov;
            public Vector3 root; public float heading, pitchBody;
            public float lurk, swim, breach, reach, slap, glide, withdraw;
            public int[] masked = new int[0];
            public float reachHeight = 260f;
            public bool underwater;
            public string eyeAt; public Vector3 eyeWorld;          // place the kraken so this eye sits here
            public string faceEye;                                 // turn the head so this eye's socket faces the camera
            public float scale = 1f;                               // rig.Scale (the show's stinger animal is 2.5x)
            public bool leeClip;                                   // publish _BwDamClip for this lee-side camera
            public Vector3[] reachTargets; public int[] reachLimbs;
        }

        readonly List<Station> m_st = new List<Station>();
        KrakenCreature m_kc;
        GameObject m_booth;
        Station m_cur;
        static readonly SeaDisturbance[] s_dist = new SeaDisturbance[SeaDisturbances.Max];

        string m_perfStation;

        public void Build(WorldContext ctx)
        {
            // -kraken-perf <station>: no capture; the camera is held at the station pose (CameraRig override) so the
            // PerfMonitor (-perf-log, -perf-rt 3840x2160, -uncapped) measures the kraken's cost with -progress <station s>
            m_perfStation = CliArgs.Get("-kraken-perf");
            if (!QaRigs.Requested(Set) && string.IsNullOrEmpty(m_perfStation)) return;
            DefineStations();
            m_kc = KrakenCreature.Build(ctx, ctx.Root);
            if (m_kc == null || m_kc.Rig == null) { Debug.LogError("[BW-FAIL] QA_KrakenRig: the kraken did not build"); return; }
            int forceLod = CliArgs.GetInt("-kraken-lod", -1);
            m_kc.ForceLod = forceLod;
            KrakenCreature.Exclusive = m_kc;                  // a look-dev run shows only the QA kraken (globals: one at a time)
            m_kc.Visible = !CliArgs.Has("-kraken-off");
            BuildBooth(ctx);
            PlaceJointCamera();
            ctx.Register(this);
            if (QaRigs.Requested(Set)) QaRigs.Register(Set, Stills, Sequences);
            if (!string.IsNullOrEmpty(m_perfStation)) m_cur = m_st.Find(x => x.key == m_perfStation) ?? m_st[0];
            ctx.Log($"QA_KrakenRig: {m_st.Count} stations, booth at {Booth}, force LOD {forceLod}, perf station '{m_perfStation}'");
        }

        void DefineStations()
        {
            m_st.Add(new Station { key = "storm800", s = 300, ambient = 1000, cx = 4.2f, cy = 1.7f, cz = 300f, yaw = 80f, pitch = 2f, fov = 64f,
                root = new Vector3(806f, -66f, 380f), heading = 255f, breach = 0.6f, reach = 0.6f, masked = new[] { 1, 6 }, reachHeight = 280f });
            m_st.Add(new Station { key = "storm2k", s = 300, ambient = 1063, cx = 4.2f, cy = 1.7f, cz = 310f, yaw = 72f, pitch = 1f, fov = 64f,
                root = new Vector3(1800f, -52f, 640f), heading = 245f, breach = 0.5f, reach = 0.7f, masked = new[] { 0, 3, 6 }, reachHeight = 380f });
            m_st.Add(new Station { key = "noon800", s = 800, ambient = 1131, cx = 3.0f, cy = 1.7f, cz = 776f, yaw = 78f, pitch = -1f, fov = 64f,
                root = new Vector3(860f, -66f, 860f), heading = 255f, breach = 0.5f, reach = 0.5f, masked = new[] { 2, 5 }, reachHeight = 300f });
            m_st.Add(new Station { key = "noon2k", s = 800, ambient = 1197, cx = 3.0f, cy = 1.7f, cz = 790f, yaw = 80f, pitch = 1f, fov = 64f,
                root = new Vector3(1760f, -58f, 1010f), heading = 250f, swim = 0.4f, breach = 0.4f, reach = 0.6f, masked = new[] { 7 }, reachHeight = 420f });
            m_st.Add(new Station { key = "islet", s = 800, ambient = 1250, cx = 3.0f, cy = 1.7f, cz = 770f, yaw = 62f, pitch = 5f, fov = 64f,
                root = new Vector3(440f, -170f, 960f), heading = 240f, lurk = 1f,
                reachLimbs = new[] { 1, 2 }, reachTargets = new[] { new Vector3(300f, 250f, 850f), new Vector3(560f, 190f, 1015f) } });
            m_st.Add(new Station { key = "crestnear", s = 300, ambient = 1311, cx = 3.5f, cy = 1.7f, cz = 250f, yaw = 60f, pitch = 12f, fov = 64f,
                root = new Vector3(280f, -150f, 390f), heading = 250f, lurk = 1f,
                reachLimbs = new[] { 6, 7 }, reachTargets = new[] { new Vector3(70f, 240f, 300f), new Vector3(125f, 175f, 352f) } });
            // A1 K4 (1.4 / 1.6): crown at (500, 1120) 12 m deep, facing 236 (the walker), seen from the crest at s ~800
            m_st.Add(new Station { key = "head600", s = 800, ambient = 1555, cx = 3.0f, cy = 1.7f, cz = 790f, yaw = 56f, pitch = 6f, fov = 64f,
                root = new Vector3(500f, -12f, 1120f), heading = 236f, breach = 0.7f, lurk = 0.3f, faceEye = "main_r" });
            // a main eye 90 m away in air, socket normal towards the camera (heading 178 = 240 - 61.8, the eye's own yaw)
            m_st.Add(new Station { key = "eyeair", s = 300, ambient = 1613, cx = 380f, cy = 6f, cz = 980f, yaw = 60f, pitch = 4f, fov = 50f,
                heading = 178f, breach = 0.6f, lurk = 0.4f, eyeAt = "main_r", eyeWorld = new Vector3(380f + 0.866f * 90f, 12f, 980f + 0.5f * 90f) });
            // the crown above the sea (ref. 1), arm 3 raised; the camera is placed by PlaceJointCamera after the build
            m_st.Add(new Station { key = "joint", s = 300, ambient = 1677, cx = 0f, cy = 0f, cz = 0f, yaw = 0f, pitch = 0f, fov = 55f,
                root = new Vector3(700f, 60f, 300f), heading = 250f, breach = 0.5f, reach = 0.5f, masked = new[] { 3 }, reachHeight = 260f });
            m_st.Add(new Station { key = "studio", s = 800, ambient = 1733, cx = 8.0f, cy = 40f, cz = 600f, yaw = 79.5f, pitch = 11f, fov = 64f,
                root = new Vector3(760f, 150f, 740f), heading = 230f, lurk = 1f, faceEye = "main_r" });
            m_st.Add(new Station { key = "leeclip", s = 1490, ambient = 1859, cx = -3500f, cy = -5998.3f, cz = 1497f, yaw = 90f, pitch = 56f, fov = 66f,
                root = new Vector3(700f, 100f, 1497f), heading = 270f, reach = 1f, masked = new[] { 1, 2, 5, 6 }, reachHeight = 2400f,
                scale = 2.5f, leeClip = true });
            // dolly: camera from (900,20,700) + 250 d to + 2400 d, d = (-0.29, 0, -0.96) (never west of x 200: the dam
            // stays out of the path); the station pose is the path's middle (the eye turns to it)
            m_st.Add(new Station { key = "dolly", s = 300, ambient = 1791, cx = 900f - 0.29f * 1325f, cy = 20f, cz = 700f - 0.96f * 1325f, yaw = 16.8f, pitch = 2f, fov = 64f,
                root = new Vector3(900f, -20f, 700f), breach = 0.6f, reach = 0.4f, masked = new[] { 1, 6 }, reachHeight = 300f, faceEye = "main_r" });
            m_st.Add(new Station { key = "uwclose", s = 300, ambient = 1433, cx = Booth.x, cy = Booth.y, cz = Booth.z, yaw = 0f, pitch = 4f, fov = 60f,
                heading = 90f, lurk = 1f, underwater = true, eyeAt = "main_r", eyeWorld = Booth + new Vector3(0f, 2f, 32f) });
            m_st.Add(new Station { key = "uwfront", s = 300, ambient = 1499, cx = Booth.x, cy = Booth.y, cz = Booth.z, yaw = 0f, pitch = 8f, fov = 60f,
                heading = 243f, lurk = 1f, underwater = true, eyeAt = "main_l", eyeWorld = Booth + new Vector3(0f, 3f, 28f) });
        }

        /// <summary>Applies a station's rig inputs (presets, masks, reach, root) without evaluating.</summary>
        void Pose(Station st, Vector3 cam)
        {
            var rig = m_kc.Rig;
            rig.ResetInputs();
            rig.SetPresetWeight(KrakenPreset.Lurk, st.lurk);
            rig.SetPresetWeight(KrakenPreset.Swim, st.swim);
            rig.SetPresetWeight(KrakenPreset.Breach, st.breach);
            rig.SetPresetWeight(KrakenPreset.ReachUp, st.reach);
            rig.SetPresetWeight(KrakenPreset.Slap, st.slap);
            rig.SetPresetWeight(KrakenPreset.GlideAlong, st.glide);
            rig.SetPresetWeight(KrakenPreset.Withdraw, st.withdraw);
            foreach (int k in st.masked) { rig.SetLimbMask(KrakenPreset.ReachUp, k, 1f); rig.SetLimbMask(KrakenPreset.GlideAlong, k, 1f); rig.SetLimbMask(KrakenPreset.Slap, k, 1f); }
            rig.SetReachHeight(st.reachHeight);
            rig.SetGlidePlane(new Vector3(6.5f, 0f, 0f), Vector3.right, new Vector3(0f, 0.25f, 1f));
            if (st.reachLimbs != null)
                for (int i = 0; i < st.reachLimbs.Length; i++) rig.SetReach(st.reachLimbs[i], st.reachTargets[i], 0.9f, 0.72f * rig.Limbs[st.reachLimbs[i]].Length);
            Vector3 root = st.root;
            float heading = st.heading;
            if (st.faceEye != null) heading = m_kc.HeadingFacingEye(m_kc.EyeIndex(st.faceEye), root, cam);
            if (st.eyeAt != null)
            {   // place the kraken so the named eye lands at eyeWorld (rotation by the heading only)
                var man = m_kc.Assets.Manifest;
                foreach (var e in man.eyes)
                    if (e.name == st.eyeAt)
                    {
                        var local = new Vector3(e.centre[0], e.centre[1], e.centre[2]);
                        root = st.eyeWorld - Quaternion.Euler(0f, heading, 0f) * local;
                    }
            }
            rig.SetRoot(root, heading, st.pitchBody);
            rig.Scale = st.scale;
            // the creature shaders' dam clip reads this global (KrakenDirector writes it in the game; the QA rig for its
            // lee-side station, and clears it elsewhere so a station never inherits it)
            Shader.SetGlobalVector(s_idDamClip, st.leeClip ? new Vector4(1f, cam.x, cam.y, 0f) : Vector4.zero);
        }
        static readonly int s_idDamClip = Shader.PropertyToID("_BwDamClip");

        /// <summary>The joint station's camera: 80 m from the arm's Tripo-to-tentacle joint (u = U0) of the raised limb,
        /// from the dam side and 25 m above, looking at it (the pose at the station's ambient time; deterministic).</summary>
        void PlaceJointCamera()
        {
            var st = m_st.Find(x => x.key == "joint");
            if (st == null) return;
            Pose(st, Vector3.zero);
            m_kc.Rig.Evaluate(st.ambient);
            int k = st.masked[0];
            var L = m_kc.Rig.Limbs[k];
            Vector3 j = m_kc.Rig.LimbPointWorld(k, L.U0);
            Vector3 t = (m_kc.Rig.LimbPointWorld(k, L.U0 + 20f) - m_kc.Rig.LimbPointWorld(k, L.U0 - 20f)).normalized;
            Vector3 side = Vector3.Cross(Vector3.up, t); if (side.sqrMagnitude < 1e-4f) side = Vector3.right; side.Normalize();
            if (Vector3.Dot(side, new Vector3(-1f, 0f, 0f)) < 0f) side = -side;      // the camera on the dam side (west)
            Vector3 cam = j + side * 80f + Vector3.up * 25f;
            cam.y = Mathf.Max(cam.y, 4f);
            Vector3 d = j - cam;
            st.cx = cam.x; st.cy = cam.y; st.cz = cam.z;
            st.yaw = Mathf.Atan2(d.x, d.z) * Mathf.Rad2Deg;
            st.pitch = Mathf.Atan2(d.y, new Vector2(d.x, d.z).magnitude) * Mathf.Rad2Deg;      // + = up (station convention)
            Debug.Log($"[BW] QA_KrakenRig: joint of limb {k} at {j} (u {L.U0:F0} m, r {L.RadiusAt(L.U0):F1} m); camera {cam} yaw {st.yaw:F1} pitch {st.pitch:F1}");
        }

        void BuildBooth(WorldContext ctx)
        {
            var sh = Shader.Find("Megastructure/BwCreatureVeil");
            if (sh == null) { Debug.LogWarning("[BW] QA_KrakenRig: BwCreatureVeil not found, no booth"); return; }
            var tmp = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            var mesh = tmp.GetComponent<MeshFilter>().sharedMesh;
            tmp.SetActive(false);
            Object.DestroyImmediate(tmp);
            m_booth = new GameObject("QA_KrakenBooth");
            m_booth.transform.SetParent(ctx.Root, false);
            m_booth.transform.position = Booth;
            m_booth.transform.localScale = Vector3.one * (2f * BoothRadius);
            m_booth.AddComponent<MeshFilter>().sharedMesh = mesh;
            var mr = m_booth.AddComponent<MeshRenderer>();
            mr.sharedMaterial = new Material(sh) { name = "BwCreatureVeil" };
            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            mr.receiveShadows = false;
            m_booth.SetActive(false);
        }

        Station Find(string shot)
        {
            if (string.IsNullOrEmpty(shot)) return m_cur;
            Station best = null; int bestLen = 0;
            foreach (var st in m_st)
                if (shot.IndexOf("_" + st.key, System.StringComparison.OrdinalIgnoreCase) >= 0 && st.key.Length > bestLen) { best = st; bestLen = st.key.Length; }
            return best ?? m_cur ?? m_st[0];
        }

        public void OnFrame(in FrameState f)
        {
            if (m_kc == null || m_kc.Rig == null) return;
            var st = string.IsNullOrEmpty(m_perfStation) ? Find(QaRigs.CurrentShot) : m_cur;
            m_cur = st;
            if (st == null) return;
            if (!string.IsNullOrEmpty(m_perfStation) && CameraRig.Instance != null)
                CameraRig.Instance.SetPoseOverride(new Vector3(st.cx, st.cy, st.cz), st.yaw, st.pitch, st.fov);
            var rig = m_kc.Rig;
            Pose(st, new Vector3(st.cx, st.cy, st.cz));
            rig.Evaluate(f.ambientTime);
            if (m_booth != null) m_booth.SetActive(st.underwater);
            KrakenCreature.SetUnderwater(st.underwater ? 1f : 0f, -8f, 0.35f, 0f);
            m_kc.Sync(f.cameraPos, f.fovV, f.renderHeight, f.ambientTime, f.cameraPos, 1f, f.capture, Time.deltaTime);
            // the QA rig is the only writer of SeaDisturbances while it runs (KrakenCreature.Exclusive hides any other kraken)
            int n = CliArgs.Has("-kraken-nodist") ? 0 : rig.CollectSeaDisturbances(-8f, Vector3.zero, m_kc.HeadLocal, s_dist);
            SeaDisturbances.Begin();
            for (int i = 0; i < n; i++) SeaDisturbances.Set(s_dist[i]);
            SeaDisturbances.Publish();
        }

        // ------------------------------------------------------------------ capture set
        List<StillShot> Stills()
        {
            var list = new List<StillShot>();
            bool aspects = CliArgs.Has("-kraken-aspects"), k4 = CliArgs.Has("-kraken-4k");
            foreach (var st in m_st)
            {
                list.Add(Still(st, "", ShotPlan.W1080, ShotPlan.H1080, "1080"));
                if (aspects)
                {
                    list.Add(Still(st, "_1680x1050", ShotPlan.W1610, ShotPlan.H1610, "1610"));
                    list.Add(Still(st, "_2560x1080", ShotPlan.W219, ShotPlan.H219, "219"));
                }
                if (k4) list.Add(Still(st, "_3840x2160", ShotPlan.W4K, ShotPlan.H4K, "4k"));
            }
            return list;
        }

        static StillShot Still(Station st, string suffix, int w, int h, string variant)
        {
            return new StillShot
            {
                tag = "kr_" + st.key + suffix, set = Set, baseTag = "kr_" + st.key, variant = variant, width = w, height = h,
                s = st.s, x = st.cx, y = st.cy, z = st.cz, yaw = st.yaw, pitch = st.pitch, fov = st.fov, ambient = st.ambient,
                gameFov = false, framing = false,
            };
        }

        List<SequenceSpec> Sequences(int w, int h)
        {
            var list = new List<SequenceSpec>();
            var a = m_st[0];
            list.Add(Seq("kr_seq_storm800", "20 s at 30 fps, clock running: kraken motion, LOD and pop check", a, 600, w, h, true));
            Station u = m_st.Find(x => x.key == "uwclose");
            if (u != null) list.Add(Seq("kr_seq_uwclose", "10 s at 30 fps underwater: eye tracking, mantle, murk", u, 300, w, h, true));
            Station dl = m_st.Find(x => x.key == "dolly");
            if (dl != null)
            {
                var dolly = Seq("kr_seq_dolly", "20 s fly-out 250 -> 2400 m: LOD cross-fades (no pop), eyes follow, motion", dl, 600, w, h, true);
                dolly.mode = SeqMode.MovingPose;
                dolly.x = 900.0 - 0.29 * 250.0; dolly.y = 20.0; dolly.z = 700.0 - 0.96 * 250.0;
                dolly.x1 = 900.0 - 0.29 * 2400.0; dolly.y1 = 20.0; dolly.z1 = 700.0 - 0.96 * 2400.0;
                dolly.yaw = dolly.yaw1 = 16.8; dolly.pitch = 4.0; dolly.pitch1 = 1.0;
                list.Add(dolly);
            }
            var pan = Seq("kr_pan_storm800", "0.5 px/frame yaw pan, frozen clock: skin shimmer", a, ShotPlan.PanFrames, w, h, false);
            pan.mode = SeqMode.PanYaw;
            pan.panStepDeg = ShotPlan.PanPx * a.fov / h;
            list.Add(pan);
            return list;
        }

        static SequenceSpec Seq(string name, string purpose, Station st, int frames, int w, int h, bool clock)
        {
            return new SequenceSpec
            {
                name = name, set = Set, purpose = purpose, mode = SeqMode.Static, fps = ShotPlan.SeqFps, width = w, height = h, frames = frames,
                s0 = st.s, s1 = st.s, x = st.cx, y = st.cy, z = st.cz, yaw = st.yaw, pitch = st.pitch, fov = st.fov,
                ambient0 = st.ambient, advanceAmbient = clock,
            };
        }
    }
}
