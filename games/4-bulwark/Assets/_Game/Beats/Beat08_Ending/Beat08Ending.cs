// Beat08Ending: the new ending of ADDENDUM A1 (section 00 LEAD DECISION 1; sun and sky physics of section 3.7): the glass
// EXPRESS ELEVATOR at the end of the lee ledge, the 75 s vertical descent on the outside of the lee face, the transfer at the
// foot, the 35 s maglev shuttle 3.5 km west across the lowland, the LANTERN PLATFORM at the edge of the town lights and the
// Keeper's Lantern (the goal; the final look back at the whole 6 km face). The timeline and every position is EndingPath
// (BwPath.cs: one coordinate s past 1500, the ride advancing s at 2.5 per second at every walking speed); the walker rides
// in PlayerController's ride mode, which writes EndingRide. Files:
//   Beat08Ending.cs     build order, materials (library copies + runtime BwGlass), per-frame updates (the cabin set moved by
//                       EndingPath.CabinOffset(s), doors, bridge plate, lamp anchors, the far field's motion filter
//                       _BwEndRide), the QA capture set "ending" (QaRigs: KS22-KS28 stills with poses from EndingPath)
//   Beat08Cabin.cs      the cabin (glass west / north / south, east spandrel + glass, glass floor strip, ceiling lamp, doors,
//                       handrails, carriage), the top station on the ledge end and the two continuous guide rails down the face
//   Beat08Foot.cs       the foot (base station, apron), the lantern platform (station canopy, plaza, plinth, lanterns, the
//                       Keeper's Lantern), the town edge west of it
// The guideway of the shuttle, its marker lights, the gallery rows and lamps of the face and the near lowland are analytic in
// BwFarField (dusk / ending owner) with a motion filter keyed to the cabin speed, so nothing near the cabin strobes at
// 110-152 m/s. Light: the cabin lamp, the station lamps and the lanterns are LocalLightPool anchors (at most 4 real lights).
using System;
using System.Collections.Generic;
using Megastructure.QA;
using Megastructure.Rendering;
using UnityEngine;

namespace Megastructure.Beats
{
    public sealed partial class Beat08Ending : IBeatBuilder, IWorldUpdater, IKeyShotSubjects
    {
        public string Name => "Beat08_Ending";
        public const string GlassShader = "Megastructure/BwGlass";
        /// <summary>QaRigs capture set of the ending stills (-capture-set ending).</summary>
        public const string QaSet = "ending";
        /// <summary>Exposure share of a frame (shutter 180 deg at 60 fps) for the far field's motion filter.</summary>
        public const double ShutterSeconds = 0.5 / 60.0;

        public readonly List<Material> RuntimeMaterials = new List<Material>();
        public Transform Root { get; private set; }
        public Transform Cabin { get; private set; }
        Transform m_doorS, m_doorN, m_bridge;

        Material m_tree;
        Material m_concrete, m_dark, m_steel, m_paint, m_paving, m_glass, m_glassFloor, m_lamp, m_sodium, m_lantern, m_keeper,
                 m_rail, m_guide, m_iron, m_cage, m_house, m_window;
        LocalLightPool m_pool;
        int m_cabinLight = -1, m_keeperLight = -1;
        readonly List<int> m_lanternLights = new List<int>();
        Vector3 m_cabinLamp;
        int m_tris;
        double m_lastS = double.NaN, m_lastT = double.NaN;
        bool m_ownSink;   // this rig wrote AtmosphereDriver.EndSinkOverride (the harness may feed it during seq_end: never clear another writer's value)
        // sets off during the ride and at the foot (ADDENDUM A1 3.12): every renderer of the other builders except the sky
        // dome and beat 7 (the ledge and the near face patch stay: they are the top of the wall). Inside the dam the
        // interiors face the lowland, and the analytic face of BwFarField writes no depth, so they would show through it.
        Transform m_worldRoot;
        readonly List<Renderer> m_offRenderers = new List<Renderer>();
        bool m_setsCollected, m_setsOff;
        public static readonly string[] KeepRoots = { "World_Sky", "Beat07_SluiceLedge", "Beat08_Ending" };
        static readonly int IdEndRide = Shader.PropertyToID("_BwEndRide");
        static readonly int IdEndLamps = Shader.PropertyToID("_BwEndLamps");
        static readonly int IdEndGlow = Shader.PropertyToID("_BwEndGlow");
        static readonly int IdEndCar = Shader.PropertyToID("_BwEndCar");
        /// <summary>The empty car returns up the rails (ADDENDUM A1 3.4): it leaves the foot when the walker is 2 m out of the
        /// cabin and climbs with the descent's profile; its clock is (s - departure) / 2.5 + t_g (s of the platform walk at the
        /// cinematic speed, then the end sequence): a warm point climbing the rail line in the final look back.</summary>
        public static double ReturnCarAltitude(double s, double tg)
        {
            double sDep = EndingPath.SFoot0 + 2.0;
            if (s < sDep) return -1.0;
            double t = (Math.Min(s, EndingPath.SRest) - sDep) / BulwarkConst.RideSRate + Math.Max(0.0, tg);
            return EndingPath.Descent(Math.Min(t, BulwarkConst.RideDescentTime), out _);
        }

        // ================================================================ build
        public void Build(WorldContext ctx)
        {
            var root = new GameObject(Name).transform;
            root.SetParent(ctx.Root, false);
            Root = root;
            m_worldRoot = ctx.Root;
            CreateMaterials(ctx);
            m_pool = LocalLightPool.Ensure(ctx);

            BuildTopStation(ctx, root);   // Beat08Cabin.cs
            BuildGuideRails(ctx, root);   // Beat08Cabin.cs
            BuildCabin(ctx, root);        // Beat08Cabin.cs
            BuildFoot(ctx, root);         // Beat08Foot.cs
            BuildPlatform(ctx, root);     // Beat08Foot.cs
            BuildTown(ctx, root);         // Beat08Foot.cs
            BuildTrees(ctx, root);        // Beat08Foot.cs

            Shader.SetGlobalVector(IdEndRide, Vector4.zero);
            ctx.PrewarmSets.Add(new PrewarmSet { Name = "Descent", Eye = Eye(1530.0), YawDeg = 270f, PitchDeg = -30f, Enable = null });
            ctx.PrewarmSets.Add(new PrewarmSet { Name = "Platform", Eye = Eye(EndingPath.SRest), YawDeg = 90f, PitchDeg = 29f, Enable = null });
            ctx.Register(this);
            ctx.RegisterSubjects(this);
            if (QaRigs.Requested(QaSet)) QaRigs.Register(QaSet, Stills, Sequences);
            ctx.Log("Beat08_Ending: elevator top station, cabin, guide rails, foot, lantern platform s " + EndingPath.SFoot0.ToString("0.0")
                    + "-" + EndingPath.SRest.ToString("0.0") + " (" + m_tris + " triangles)");
        }

        /// <summary>The walker's eye at ending s (cabin spot or the platform path), as a Vector3.</summary>
        public static Vector3 Eye(double s)
        {
            EndingPath.World(s, 0.0, out double x, out _, out double z);
            return new Vector3((float)x, (float)EndingPath.EyeY(s), (float)z);
        }

        // ---------------------------------------------------------------- materials
        void CreateMaterials(WorldContext ctx)
        {
            var lib = ctx.Materials;
            m_concrete = lib != null ? lib.Concrete : null;
            m_dark = lib != null ? lib.ConcreteDark : null;
            m_steel = lib != null ? lib.Steel : null;
            m_lamp = Copy(lib != null ? lib.LampTungsten : null, "b8_lamp_tungsten");
            if (m_lamp != null && m_lamp.HasProperty("_GainFollow")) m_lamp.SetFloat("_GainFollow", 0f);
            if (m_lamp != null && m_lamp.HasProperty("_Intensity")) m_lamp.SetFloat("_Intensity", 1.6f);   // a soft cabin light (x1.6)
            m_sodium = lib != null ? lib.LampSodium : null;
            m_lantern = Copy(lib != null ? lib.Lantern : null, "b8_lantern_x2");
            if (m_lantern != null && m_lantern.HasProperty("_Intensity")) m_lantern.SetFloat("_Intensity", 2.0f);
            m_keeper = lib != null ? lib.Lantern : null;   // x6, the goal object (14.11)
            // dark painted steel of the cabin frame, the station and the carriage
            m_paint = Copy(m_steel, "b8_steel_dark");
            if (m_paint != null) m_paint.SetColor("_BaseColor", Shade(BulwarkPalette.Linear(Pal.SteelPaint), 0.55f));
            // a matt powder coat: the chamfered frame edges must not sparkle in the low sun (Part 10 fireflies, KS21b / KS24)
            if (m_paint != null && m_paint.HasProperty("_Smoothness")) m_paint.SetFloat("_Smoothness", Mathf.Min(m_paint.GetFloat("_Smoothness"), 0.3f));
            // the trees (fix wave S2-1 h): rough foliage, no specular (they read as glossy lollipops with the concrete's sheen)
            m_tree = Copy(m_dark, "b8_tree_matt");
            if (m_tree != null && m_tree.HasProperty("_Smoothness")) m_tree.SetFloat("_Smoothness", 0f);
            if (m_tree != null) m_tree.SetColor("_BaseColor", Shade(BulwarkPalette.Linear(Pal.ConcreteDark), 0.6f));
            m_iron = Copy(m_steel, "b8_iron");
            if (m_iron != null)
            {
                m_iron.SetColor("_BaseColor", BulwarkPalette.Linear(Pal.ConcreteDark));
                if (m_iron.HasProperty("_ChipAmount")) m_iron.SetFloat("_ChipAmount", 0.05f);
            }
            // paving flags of the platform: the concrete look in 1.2 m flags
            m_paving = Copy(m_concrete, "b8_paving");
            if (m_paving != null)
            {
                if (m_paving.HasProperty("_PanelSize")) m_paving.SetVector("_PanelSize", new Vector4(1.2f, 1.2f, 1.2f, 0f));
                if (m_paving.HasProperty("_TieRadius")) m_paving.SetFloat("_TieRadius", 0f);
                if (m_paving.HasProperty("_WetFollow")) m_paving.SetFloat("_WetFollow", 0f);
                if (m_paving.HasProperty("_Wet")) m_paving.SetFloat("_Wet", 0.25f);
            }
            // rails (BwLine tubes): the handrails 0.05; the guide rails 0.20 (flat-shaded lines: nothing strobes at speed)
            m_rail = Copy(lib != null ? lib.Rail : null, "b8_handrail");
            if (m_rail != null) m_rail.SetFloat("_LineWidth", 0.05f);
            m_guide = Copy(lib != null ? lib.Rail : null, "b8_guide_rail");
            if (m_guide != null)
            {
                m_guide.SetFloat("_LineWidth", 0.20f);
                m_guide.SetColor("_BaseColor", Shade(BulwarkPalette.Linear(Pal.SteelPaint), 0.45f));
            }
            m_cage = Copy(lib != null ? lib.Rail : null, "b8_cage_line");
            if (m_cage != null)
            {
                m_cage.SetFloat("_LineWidth", 0.02f);
                m_cage.SetColor("_BaseColor", BulwarkPalette.Linear(Pal.ConcreteDark));
            }
            // the town edge: rendered houses and their windows (CityWarm, dim)
            m_house = Copy(m_concrete, "b8_house");
            if (m_house != null) m_house.SetColor("_BaseColor", Shade(BulwarkPalette.Linear(Pal.ConcreteMid), 0.8f));
            m_window = Copy(lib != null ? lib.LampTungsten : null, "b8_window");
            if (m_window != null)
            {
                m_window.SetColor("_EmissionColor", BulwarkPalette.Linear(Pal.CityWarm));
                m_window.SetFloat("_Intensity", 0.9f);
                if (m_window.HasProperty("_GainFollow")) m_window.SetFloat("_GainFollow", 0f);
            }
            var sh = Shader.Find(GlassShader);
            if (sh == null) Debug.LogError("[BW-FAIL] Beat08: shader " + GlassShader + " not found (RebuildAll: Always Included Shaders)");
            else
            {
                m_glass = new Material(sh) { name = "b8_glass" };
                m_glass.SetColor("_Tint", BulwarkPalette.Linear(Pal.SeaTeal));
                m_glass.SetFloat("_Absorb", 0.05f);
                RuntimeMaterials.Add(m_glass);
                m_glassFloor = new Material(sh) { name = "b8_glass_floor" };
                m_glassFloor.SetColor("_Tint", BulwarkPalette.Linear(Pal.SeaTeal));
                m_glassFloor.SetFloat("_Absorb", 0.09f);
                RuntimeMaterials.Add(m_glassFloor);
            }
        }

        static Color Shade(Color c, float k) => new Color(c.r * k, c.g * k, c.b * k, 1f);

        Material Copy(Material src, string name)
        {
            if (src == null) return null;
            var m = new Material(src) { name = name };
            RuntimeMaterials.Add(m);
            return m;
        }

        // ================================================================ per frame
        /// <summary>Switches the other sets' renderers off once the doors are sealed (s >= SDescent0) and on again before.</summary>
        void ApplySets(double s)
        {
            bool off = s >= EndingPath.SDescent0;
            if (off == m_setsOff && m_setsCollected) return;
            if (!m_setsCollected && m_worldRoot != null)
            {
                m_setsCollected = true;
                for (int i = 0; i < m_worldRoot.childCount; i++)
                {
                    var c = m_worldRoot.GetChild(i);
                    if (Array.IndexOf(KeepRoots, c.name) >= 0) continue;
                    m_offRenderers.AddRange(c.GetComponentsInChildren<Renderer>(true));
                }
            }
            m_setsOff = off;
            for (int i = 0; i < m_offRenderers.Count; i++) if (m_offRenderers[i] != null) m_offRenderers[i].forceRenderingOff = off;
        }

        public void OnFrame(in FrameState f)
        {
            ApplyDistanceCull(f.cameraPos);
            double s = f.s;
            if (s != m_lastS)
            {
                m_lastS = s;
                ApplySets(s);
                EndingPath.CabinOffset(s, out double dx, out double dy);
                if (Cabin != null) Cabin.localPosition = new Vector3((float)dx, (float)dy, 0f);
                // doors: the south (boarding) door open until tauL 0.5, closed by tauL 3.0; the north door opens tauL 120-123
                double tl = EndingPath.TauL(s);
                float openS = s <= EndingPath.S0 ? 1f : 1f - (float)BwMath.Smootherstep01((tl - 0.5) / 2.5);
                float openN = s <= EndingPath.S0 ? 0f : (float)BwMath.Smootherstep01((tl - EndingPath.TauShuttle1) / BulwarkConst.RideDoorsTime);
                if (m_doorS != null) m_doorS.localPosition = new Vector3(DoorSlide * openS, 0f, 0f);
                if (m_doorN != null) m_doorN.localPosition = new Vector3(DoorSlide * openN, 0f, 0f);
                if (m_bridge != null)
                {
                    bool bridge = s < EndingPath.S0 + 2.0;
                    if (m_bridge.gameObject.activeSelf != bridge) m_bridge.gameObject.SetActive(bridge);
                }
                if (m_pool != null && m_cabinLight >= 0) m_pool.SetPosition(m_cabinLight, m_cabinLamp + new Vector3((float)dx, (float)dy, 0f));
                // the far field's motion filter (exposure 1/120 s of the cabin's velocity: the face and the ground blur along
                // the motion like a 180-degree shutter instead of strobing at 1.8-2.5 m per frame)
                EndingPath.Cabin(s, out _, out _, out _, out double vx, out double vy);
                bool riding = EndingPath.InRide(s);
                // z: the ending weight (0 on the ledge, 1 from the boarding on; smootherstep over s 1488-1498, inside the funnel and
                // the cabin): the far field's afterglow on the lee face and the jets' falling veils (BwFarField) fade in with it, so
                // the beat-7 key shots (KS19-KS21) keep their look and nothing pops
                float wEnd = (float)BwMath.Smootherstep01((s - 1488.0) / 10.0);
                Shader.SetGlobalVector(IdEndRide, new Vector4(riding ? (float)(Math.Abs(vx) * ShutterSeconds) : 0f,
                                                              riding ? (float)(Math.Abs(vy) * ShutterSeconds) : 0f, wEnd, 0f));
            }
            // the returning car: a far-field glow on the rail line (BwFarField _BwEndCar), lit interior ~3 m, from the foot up
            double tgNow = !double.IsNaN(AtmosphereDriver.EndSinkOverride) ? AtmosphereDriver.EndSinkOverride
                         : (GameState.GoalReached ? Megastructure.UI.UiDirector.EndingTime : -1.0);
            double carUp = ReturnCarAltitude(s, tgNow);
            Shader.SetGlobalVector(IdEndCar, carUp < 0.0 ? Vector4.zero
                : new Vector4((float)(EndingPath.SpotX - 0.3), (float)(EndingPath.FloorFootY + 1.5 + carUp), (float)EndingPath.SpotZ, 1f));

            // the lanterns' real lights follow the glass flicker (14.2.5)
            double t = f.ambientTime;
            if (t != m_lastT)
            {
                m_lastT = t;
                float g = (float)LanternFlicker.Gain(t);
                if (m_pool != null && m_keeperLight >= 0) m_pool.SetGain(m_keeperLight, g);
            }
            // per-shot QA state of the "ending" stills: the after-goal sun sink of KS28b
            if (f.capture)
            {
                string shot = QaRigs.CurrentShot;
                double sink = double.NaN;
                if (!string.IsNullOrEmpty(shot) && shot.StartsWith("b08_", StringComparison.Ordinal))
                {
                    for (int i = 0; i < s_shots.Length; i++) if (shot.StartsWith(s_shots[i].tag, StringComparison.Ordinal)) sink = s_shots[i].tg;
                }
                if (!double.IsNaN(sink) ? sink != AtmosphereDriver.EndSinkOverride : m_ownSink)
                {
                    AtmosphereDriver.EndSinkOverride = sink;
                    m_ownSink = !double.IsNaN(sink);
                }
            }
        }

        // ================================================================ QA: the ending stills (QaRigs set "ending")
        struct EndShot
        {
            public string tag; public double s, yaw, pitch, tg, fov; public int w, h;
            public EndShot(string tag, double s, double yaw, double pitch, double tg = double.NaN, int w = 1920, int h = 1080, double fov = 66.0)
            { this.tag = tag; this.s = s; this.yaw = yaw; this.pitch = pitch; this.tg = tg; this.w = w; this.h = h; this.fov = fov; }
        }

        /// <summary>KS22+ (ADDENDUM A1 4.9 renumbered for the vertical elevator): the poses are the walker's eye at s (EndingPath),
        /// the gaze of the ride keys (GazeSchedule.RideKeys) at the moments the addendum names; vFOV 66 (beat 7).</summary>
        static readonly EndShot[] s_shots =
        {
            new EndShot("b08_Ending_KS21b_cabin_door", 1496.0, 0.0, -4.0),                      // the walk into the lit cabin
            new EndShot("b08_Ending_KS22_ride_drop", 1530.0, 270.0, -30.0),                      // tau 8: 5.8 km of void below
            new EndShot("b08_Ending_KS22b_glass_floor", 1540.0, 270.0, -58.0),                   // through the glass floor
            new EndShot("b08_Ending_KS23_ride_sunset", 1571.2, 300.0, -3.0),                     // the sun centre on the horizon (4388 m)
            new EndShot("b08_Ending_KS23b_face_rush", 1600.0, 10.0, -25.0),                      // along the face at 110 m/s (motion filter)
            new EndShot("b08_Ending_KS24_ride_lookup", 1616.0, 25.0, 52.0),                      // up the face through the roof glass
            new EndShot("b08_Ending_KS24b_ride_mist", 1640.0, 270.0, -10.0),                     // the mist rivers at eye level
            new EndShot("b08_Ending_KS25_ride_lanterns", 1680.0, 262.0, -12.0),                  // the guideway to the platform lights
            new EndShot("b08_Ending_KS26a_transfer", 1705.0, 80.0, 65.0),                        // up the 6 km wall from its foot
            new EndShot("b08_Ending_KS26_shuttle_back", 1822.0, 90.0, 32.0),                     // the wall receding behind, the crest entering (3.26 km)
            new EndShot("b08_Ending_KS26b_platform_walk", 1865.0, 330.0, 3.0),                   // north-west: afterglow, town (walk d 7.5)
            new EndShot("b08_Ending_KS27_keeper", 0.0, 90.0, 0.0, 1.5),                          // s = rest (filled below)
            new EndShot("b08_Ending_KS28_dam_reveal", 0.0, 90.0, 29.0, 8.5),                     // THE money shot
            new EndShot("b08_Ending_KS28_dam_reveal_4k", 0.0, 90.0, 29.0, 8.5, 3840, 2160),
            new EndShot("b08_Ending_KS28_dam_reveal_219", 0.0, 90.0, 29.0, 8.5, 2560, 1080),   // A1 4.9: 21:9 at 2560 x 1080
            new EndShot("b08_Ending_KS28b_crest_dark", 0.0, 90.0, 29.0, 20.0),                   // after the last light leaves the crest (t_g 19)
            new EndShot("b08_Ending_KS29_lift", 0.0, 90.0, 36.0, 30.0),                          // after the tilt-up (the stinger's frame)
            new EndShot("b08_Ending_KS28c_crest_zoom", 0.0, 90.0, 58.5, 8.5, 1920, 1080, 8.0),   // QA: the crest line and the jets at 6.9 km
            new EndShot("b08_Ending_KS28d_foot_zoom", 0.0, 90.0, 1.0, 8.5, 1920, 1080, 8.0),     // QA: the foot, its lamps and the roads
        };

        /// <summary>The real game in the descent (CaptureHarness RealEnd: teleport into the ride at s0, the ride clock runs at the
        /// sequence rate, the look is the path heading (north) at the pitch): 7.2 s at 30 fps through the north glass at full
        /// speed (110 m/s) past the gallery rows: the temporal check of the motion filter (flicker, spikes, luma steps). The
        /// harness stops it at its no-trigger limit (the goal is 266 m of s away), which it notes in the sequence record.</summary>
        public static List<SequenceSpec> Sequences(int w, int h)
        {
            return new List<SequenceSpec>
            {
                new SequenceSpec
                {
                    name = "seq_ride_descent", set = QaSet, purpose = "descent at 110 m/s: face, gallery rows, guide rails (temporal)",
                    mode = SeqMode.RealEnd, fps = 30, width = w, height = h, frames = 216, s0 = 1580.0, s1 = 1600.0, speed = BulwarkConst.RideSRate,
                    pitch = -20.0,
                },
            };
        }

        public static List<StillShot> Stills()
        {
            var list = new List<StillShot>(s_shots.Length);
            foreach (var e in s_shots)
            {
                double s = e.s > 0.0 ? e.s : EndingPath.SRest;
                Vector3 p = s <= BwPath.Length ? BwPath.EyePoint(s, 0.0) : Eye(s);
                list.Add(new StillShot
                {
                    tag = e.tag, set = QaSet, baseTag = e.tag, variant = e.w == 1920 ? "1080" : (e.w == 3840 ? "4k" : "219"), width = e.w, height = e.h,
                    s = s, x = p.x, y = p.y, z = p.z, yaw = e.yaw, pitch = e.pitch, fov = e.fov,
                    ambient = ShotPlan.AmbientForS(s), gameFov = e.fov == 66.0, framing = false,
                });
            }
            return list;
        }

        // ================================================================ key-shot subjects (11.13): none audited yet (the
        // VisibilityAudit catalogue covers KS01-KS21; the ending shots are QaRigs stills, see the report)
        public void CollectSubjects(string shotId, List<KeyShotSubject> into) { }

        // ================================================================ mesh helpers
        void Flush(WorldContext ctx, Transform parent, ref MeshBuilder mb, string name, Material mat, bool shadows = false)
        {
            if (mb.VertexCount > 0 && mat != null)
            {
                var mesh = mb.ToMesh(name);
                var mr = ctx.AddMesh(parent, name, mesh, mat, shadows, true);
                mr.shadowCastingMode = shadows ? UnityEngine.Rendering.ShadowCastingMode.On : UnityEngine.Rendering.ShadowCastingMode.Off;
                m_tris += mb.TriangleCount;
                for (int i = 0; i < s_farCull.Length; i++)
                    if (name == s_farCull[i].name) m_distCull.Add((mr, mesh.bounds.center, s_farCull[i].dist));
            }
            mb = new MeshBuilder();
        }

        // Small emissive parts become lone bright pixels that sparkle beyond a few hundred metres (0.06-0.3 m at 3.5-5.9 km is a
        // hundredth of a pixel: Part 10 sparkle): they are switched off by distance; the platform is replaced by BwFarField's
        // energy-preserving plaza glow (_BwEndGlow, faded in over 250-400 m).
        static readonly (string name, float dist)[] s_farCull =
        {
            ("b8_top_station_lamps", 300f), ("b8_foot_lamps", 300f), ("b8_platform_lamps", 300f),
            ("b8_lanterns_iron", 300f), ("b8_lanterns_glass", 300f), ("b8_lanterns_cage", 300f),
            ("b8_keeper_iron", 300f), ("b8_keeper_glass", 300f), ("b8_keeper_cage", 300f), ("b8_platform_rail", 300f),
            ("b8_town_windows", 1500f), ("b8_trees", 4500f),
            ("b8_top_station_steel", 2500f), ("b8_top_station_house", 2500f),   // lone bright sub-pixels on the crest from the platform
        };
        readonly List<(MeshRenderer r, Vector3 c, float d)> m_distCull = new List<(MeshRenderer, Vector3, float)>();

        void ApplyDistanceCull(Vector3 cam)
        {
            for (int i = 0; i < m_distCull.Count; i++)
            {
                var e = m_distCull[i];
                if (e.r == null) continue;
                bool off = (e.c - cam).sqrMagnitude > e.d * e.d;
                if (e.r.forceRenderingOff != off) e.r.forceRenderingOff = off;
            }
        }

        static Vector3 V(double x, double y, double z) => new Vector3((float)x, (float)y, (float)z);

        /// <summary>Axis-aligned box from min to max corners (flat-shaded, vertex colour = the current builder colour).</summary>
        static void Box(MeshBuilder mb, double x0, double y0, double z0, double x1, double y1, double z1) => Beat07SluiceLedge.BoxMinMax(mb, x0, y0, z0, x1, y1, z1);

        /// <summary>A glass pane (BwGlass is drawn with Cull Off: one quad, both sides).</summary>
        static void Pane(MeshBuilder mb, Vector3 a, Vector3 b, Vector3 c, Vector3 d) => mb.FlatQuad(a, b, c, d);
    }
}
