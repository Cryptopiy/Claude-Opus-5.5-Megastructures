// Beat06WindowGallery: beat 6, the window gallery (MASTER_PROMPT 14.5.6 MONEY SHOT 3, 14.7 H3; Addendum A1 1.8: the kraken
// encounter replaces the whale). Replaces the graybox of beat 6 ([ReplacesGraybox("beat6")]). Files:
//   Beat06Layout.cs          pure geometry and the s-keyed timeline (double; Beat06Tests)
//   Beat06WindowGallery.cs   core: build order, materials (library + runtime BwWaterPortal / BwWaterPortalScatter / BwSnow),
//                            the hall (floor ramp, west wall with pipes, cable tray, hatches and bollard lamps, 28 m ceiling with
//                            beams, the glazed trench, the rail, the piers and the steel glazing frames, the doorway wall at
//                            s 1070), the ten pane portals, the 6000-flake snow mesh, the survey lamps, the water globals
//   Beat06Encounter.cs       the kraken encounter staged with KrakenRig / KrakenCreature (Runtime/Creature, used as it is):
//                            darkness, the arm above, suckers on pane 5 under lamp A, the eyes in the dark, the eye in pane 6
//                            under lamp B (pupil, blink), the withdrawal, the sink below (KS18); KrakenCalls at those moments
// Light: the bollard lamps and the two survey spots are LocalLightPool anchors (at most 4 real local lights, 14.2.3); the
// water's own light is analytic (BwWater.hlsl). Sky, fog, exposure and the sun (the shaft angle) come from AtmosphereDriver.
// Everything is built once behind the load fade; per frame only globals, two light gains and the creature's pose change.
using System;
using System.Collections.Generic;
using Megastructure.Rendering;
using UnityEngine;

namespace Megastructure.Beats
{
    [ReplacesGraybox("beat6")]
    public sealed partial class Beat06WindowGallery : IBeatBuilder, IWorldUpdater, IKeyShotSubjects
    {
        public string Name => "Beat06_WindowGallery";
        public const string PortalShader = "Megastructure/BwWaterPortal";
        public const string ScatterShader = "Megastructure/BwWaterPortalScatter";
        public const string SnowShader = "Megastructure/BwSnow";

        Material m_conc, m_wet, m_dark, m_steel, m_rail, m_lamp, m_portal, m_scatter, m_snow;
        /// <summary>Materials created at runtime by this builder (the EditMode audit destroys them).</summary>
        public readonly List<Material> RuntimeMaterials = new List<Material>();
        public Transform Root { get; private set; }
        LocalLightPool m_pool;
        int m_lampA = -1, m_lampB = -1;
        int m_tris;

        // the hall frame (the gallery leg is straight)
        Vector3 m_tan, m_right;
        double m_planeW, m_tanW;

        static readonly int IdPlane = Shader.PropertyToID("_BwWPlane");
        static readonly int IdTan = Shader.PropertyToID("_BwWTan");
        static readonly int IdState = Shader.PropertyToID("_BwWState");
        static readonly int IdLampA0 = Shader.PropertyToID("_BwWLampA0");
        static readonly int IdLampA1 = Shader.PropertyToID("_BwWLampA1");
        static readonly int IdLampB0 = Shader.PropertyToID("_BwWLampB0");
        static readonly int IdLampB1 = Shader.PropertyToID("_BwWLampB1");
        static readonly int IdLampCol = Shader.PropertyToID("_BwWLampCol");
        static readonly int IdEye = Shader.PropertyToID("_BwWEye");
        static readonly int IdSnow = Shader.PropertyToID("_BwWSnow");
        static readonly int IdArm0 = Shader.PropertyToID("_BwWArm0");
        static readonly int IdArm1 = Shader.PropertyToID("_BwWArm1");
        static readonly int IdArm2 = Shader.PropertyToID("_BwWArm2");
        static readonly int IdArm3 = Shader.PropertyToID("_BwWArm3");

        /// <summary>Survey-lamp spot intensity (URP units; the creature's flank at 20-30 m) and the lamps' water radiance scale.</summary>
        public const float LampIntensity = 900f, LampWaterRadiance = 60f;
        Vector3 m_lampAPos, m_lampBPos, m_lampADir, m_lampBDir;

        // ================================================================ build
        public void Build(WorldContext ctx)
        {
            var root = new GameObject(Name).transform;
            root.SetParent(ctx.Root, false);
            Root = root;
            ComputeFrame();
            CreateMaterials(ctx);
            m_pool = LocalLightPool.Ensure(ctx);

            BuildHall(ctx, root);
            BuildDoorway(ctx, root);
            BuildWestWallFittings(ctx, root);
            BuildGlazing(ctx, root);
            BuildPortals(ctx, root);
            BuildSnow(ctx, root);
            BuildLamps(ctx, root);
            BuildEncounter(ctx, root);      // Beat06Encounter.cs
            PublishStatic();

            ctx.PrewarmSets.Add(Prewarm("Hall6", BulwarkConst.KeyShots[14]));
            ctx.Register(this);
            ctx.RegisterSubjects(this);
            ctx.Log("Beat06_WindowGallery: hall s 1070-1370, " + Beat06Layout.Panes + " pane portals, " + BulwarkConst.SnowQuads
                    + " snow flakes, " + Beat06Layout.Bollards + " bollards + 2 survey lamps (" + m_tris + " triangles), encounter "
                    + (m_kc != null ? "built" : "MISSING"));
        }

        static PrewarmSet Prewarm(string name, KeyShot k) => new PrewarmSet
        {
            Name = name, Eye = new Vector3((float)k.x, (float)k.y, (float)k.z), YawDeg = (float)k.yaw, PitchDeg = (float)k.pitch, Enable = _ => { },
        };

        void ComputeFrame()
        {
            const double sm = 1200.0;
            BwPath.Tangent(sm, out double tx, out double tz);
            BwPath.Right(sm, out double rx, out double rz);
            BwPath.World(sm, 0.0, out double ox, out _, out double oz);
            m_tan = new Vector3((float)tx, 0f, (float)tz);
            m_right = new Vector3((float)rx, 0f, (float)rz);
            m_planeW = ox * rx + oz * rz;
            m_tanW = sm - (ox * tx + oz * tz);
        }

        // ---------------------------------------------------------------- materials
        void CreateMaterials(WorldContext ctx)
        {
            var lib = ctx.Materials;
            m_conc = lib != null ? lib.Concrete : null;
            m_wet = lib != null ? lib.ConcreteWet : null;
            m_dark = lib != null ? lib.ConcreteDark : null;
            m_steel = lib != null ? lib.Steel : null;
            m_lamp = lib != null ? lib.LampTungsten : null;
            m_rail = lib != null ? lib.Rail : null;
            if (m_rail != null && m_rail.HasProperty("_LineWidth") && Mathf.Abs(m_rail.GetFloat("_LineWidth") - (float)Beat06Layout.RailDiameter) > 1e-4f)
            {
                m_rail = Copy(m_rail, "b6_rail_010");
                m_rail.SetFloat("_LineWidth", (float)Beat06Layout.RailDiameter);
            }
            // the hall's floor: damp concrete (condensation on the cold side of the dam), no rain follow
            m_wet = Copy(m_wet, "b6_floor_damp");
            if (m_wet != null && m_wet.HasProperty("_WetFollow")) m_wet.SetFloat("_WetFollow", 0f);
            if (m_wet != null && m_wet.HasProperty("_Wet")) m_wet.SetFloat("_Wet", 0.45f);
            m_portal = NewMat(PortalShader, "b6_water_portal");
            m_scatter = NewMat(ScatterShader, "b6_water_scatter");
            m_snow = NewMat(SnowShader, "b6_snow");
        }

        Material NewMat(string shader, string name)
        {
            var sh = Shader.Find(shader);
            if (sh == null) { Debug.LogError("[BW-FAIL] Beat06: shader " + shader + " not found (RebuildAll: Always Included Shaders)"); return null; }
            var m = new Material(sh) { name = name };
            RuntimeMaterials.Add(m);
            return m;
        }

        Material Copy(Material src, string name)
        {
            if (src == null) return null;
            var m = new Material(src) { name = name };
            RuntimeMaterials.Add(m);
            return m;
        }

        // ================================================================ the hall
        /// <summary>Overlap of the hall's surfaces at every inside corner (m).</summary>
        const double Seal = 0.3;

        static List<double> Knots()
        {
            var k = new List<double>();
            for (double s = Beat06Layout.S0; s < Beat06Layout.S1; s += 2.0) k.Add(s);
            k.Add(Beat06Layout.S1);
            for (int p = 0; p < Beat06Layout.Panes; p++) { double c = Beat06Layout.PaneS(p); k.Add(c - 10.0); k.Add(c + 10.0); }
            k.Sort();
            var o = new List<double>(k.Count);
            foreach (double s in k) if (o.Count == 0 || s - o[o.Count - 1] > 1e-4) o.Add(s);
            return o;
        }

        void BuildHall(WorldContext ctx, Transform root)
        {
            var floor = new MeshBuilder();
            var wall = new MeshBuilder();
            var ceil = new MeshBuilder();
            var trench = new MeshBuilder();
            var piers = new MeshBuilder();
            var rails = new MeshBuilder();
            var posts = new MeshBuilder();
            var knots = Knots();
            double W = Beat06Layout.West, Rl = Beat06Layout.Rail, Gi = Beat06Layout.GlassIn, H = Beat06Layout.Height, Td = Beat06Layout.Trench;
            float rr = 0.5f * (float)Beat06Layout.RailDiameter;
            var ao0 = new Color32(140, 0, 0, 255);   // corner AO at the floor / wall junction
            for (int i = 0; i + 1 < knots.Count; i++)
            {
                double a = knots[i], b = knots[i + 1], mid = 0.5 * (a + b);
                double ya = Beat06Layout.FloorY(a), yb = Beat06Layout.FloorY(b);
                // floor from the west wall to the trench edge, faces up
                Quad(floor, P(a, W, ya), P(b, W, yb), P(b, Rl, yb), P(a, Rl, ya), ao0, ao0, Open, Open);
                // west wall (faces +lateral), in rows of <= 2 m with the corner AO at the floor and the soffit (WallGrid)
                // (every inside corner overlaps by Seal: each surface runs on behind the one it meets, so no T-junction of the
                // two tessellations can open a one-pixel crack onto the far field; pose_w1085p40 of gallery-final2)
                WallGrid(wall, a, b, W, ya - 0.5, yb - 0.5, ya + H + Seal, yb + H + Seal, true, ya, yb, H);
                // soffit (faces down), 2 m cells across
                CeilGrid(ceil, a, b, W - Seal, Gi + Seal, ya + H, yb + H);
                // trench: inner wall (faces the glass), floor, then the pier face at the glass plane where no pane is
                WallGrid(trench, a, b, Rl, ya - Td - Seal, yb - Td - Seal, ya, yb, true, ya - Td, yb - Td, Td);
                FloorGrid(trench, a, b, Rl, Gi + Seal, ya - Td, yb - Td);
                if (Beat06Layout.PaneAt(mid) < 0)
                    WallGrid(piers, a, b, Gi, ya - Td - Seal, yb - Td - Seal, ya + H + Seal, yb + H + Seal, false, ya - Td, yb - Td, H + Td);
                // rail: two 0.10 m tubes (tops 1.10 / 0.55) on the trench edge, posts every 2 m on the trench wall's face
                double rx = Rl + Beat06Layout.RailSetback;
                Tube(rails, P(a, rx, ya + Beat06Layout.RailTop - rr), P(b, rx, yb + Beat06Layout.RailTop - rr), rr, 12);
                Tube(rails, P(a, rx, ya + Beat06Layout.RailMid - rr), P(b, rx, yb + Beat06Layout.RailMid - rr), rr, 12);
            }
            for (double sp = Beat06Layout.S0 + 1.0; sp < Beat06Layout.S1; sp += 2.0)
            {
                double y = Beat06Layout.FloorY(sp);
                OBox(posts, sp - 0.04, sp + 0.04, Rl, Rl + 0.08, y - 0.30, y + Beat06Layout.RailTop - 2 * rr);
            }
            // transverse ceiling beams over every pier centre (and the two end piers): 0.6 m wide, 1.4 m deep (scale cue); none
            // over a pane, so no beam ever crosses in front of the glass
            for (double s = 1072.5; s <= 1368.0; s = s < 1100.0 - 1e-6 ? 1100.0 : s + 30.0)
            {
                double y = Beat06Layout.FloorY(s) + H;
                Color32 bc = AoFor(P(s, 0.5 * (W + Gi), y - 1.4), Vector3.down, s, 200);
                Slab(ceil, s - 0.3, s + 0.3, W, Gi, y - 1.4, y - 1.4, y + 0.05, y + 0.05, bc);
            }
            Flush(ctx, root, ref floor, "b6_floor", m_wet, false);
            // the shell casts shadows: the hall lies inside the dam, so no sun may reach it (the dam body itself is not modelled
            // round the hall; the portal quads cast too, BwWaterPortal ShadowCaster)
            Flush(ctx, root, ref wall, "b6_west_wall", m_conc, true);
            Flush(ctx, root, ref ceil, "b6_ceiling", m_conc, true);
            Flush(ctx, root, ref trench, "b6_trench", m_dark, true);
            Flush(ctx, root, ref piers, "b6_piers", m_dark, true);
            Flush(ctx, root, ref rails, "b6_rail", m_rail, false);
            Flush(ctx, root, ref posts, "b6_rail_posts", m_steel, false);
        }

        /// <summary>The doorway wall at s 1070 (14.6: the exit hall's 4.4 x 4.0 doorway into the 28 m gallery), two-sided.</summary>
        void BuildDoorway(WorldContext ctx, Transform root)
        {
            var mb = new MeshBuilder();
            double s = Beat06Layout.S0, y = Beat06Layout.FloorY(s), h = BulwarkConst.ExitDoorHeight, half = 0.5 * BulwarkConst.ExitDoorWidth;
            double W = Beat06Layout.West, Gi = Beat06Layout.GlassIn;
            CrossWall(mb, s, W, Gi, y + h, y + Beat06Layout.Height);
            CrossWall(mb, s, W, -half, y, y + h);
            CrossWall(mb, s, half, Gi, y - Beat06Layout.Trench, y + h);
            // a steel lintel and jambs round the doorway on the gallery side (0.25 wide, 0.06 proud)
            OBox(mb, s, s + 0.06, -half - 0.25, half + 0.25, y + h, y + h + 0.25);
            OBox(mb, s, s + 0.06, -half - 0.25, -half, y, y + h);
            OBox(mb, s, s + 0.06, half, half + 0.25, y, y + h);
            Flush(ctx, root, ref mb, "b6_doorway_wall", m_conc, true);
        }

        /// <summary>West wall fittings: three pipes and a cable tray along the whole ramp, hatches every 30 m, bollard lamps every
        /// 20 m (0.9 m, in the 0.15 m niche between the wall and the collider face; Tungsten tops, LocalLightPool anchors).</summary>
        void BuildWestWallFittings(WorldContext ctx, Transform root)
        {
            var pipes = new MeshBuilder();
            var steel = new MeshBuilder();
            var bodies = new MeshBuilder();
            var tops = new MeshBuilder();
            double W = Beat06Layout.West, s0 = Beat06Layout.S0 + 0.1, s1 = Beat06Layout.S1;
            double[] ph = { 3.2, 3.75, 4.25 };
            float[] pr = { 0.17f, 0.12f, 0.09f };
            for (int i = 0; i < 3; i++)
            {
                double lat = W + 0.12 + pr[i];
                Tube(pipes, P(s0, lat, Beat06Layout.FloorY(s0) + ph[i]), P(s1, lat, Beat06Layout.FloorY(s1) + ph[i]), pr[i], 14);
            }
            // pipe brackets every 5 m, cable tray at 7 m (a flat steel channel on brackets)
            for (double s = 1072.5; s < s1; s += 5.0)
            {
                double y = Beat06Layout.FloorY(s);
                OBox(steel, s - 0.05, s + 0.05, W, W + 0.62, y + 3.0, y + 3.06);
                OBox(steel, s - 0.05, s + 0.05, W, W + 0.75, y + 6.95, y + 7.0);
            }
            {
                double ya = Beat06Layout.FloorY(s0) + 7.0, yb = Beat06Layout.FloorY(s1) + 7.0;
                Quad(steel, P(s0, W, ya), P(s1, W, yb), P(s1, W + 0.7, yb), P(s0, W + 0.7, ya), Open, Open, Open, Open);
                Quad(steel, P(s0, W + 0.7, ya - 0.02), P(s1, W + 0.7, yb - 0.02), P(s1, W, yb - 0.02), P(s0, W, ya - 0.02), Open, Open, Open, Open);
                Quad(steel, P(s0, W + 0.7, ya - 0.02), P(s0, W + 0.7, ya + 0.14), P(s1, W + 0.7, yb + 0.14), P(s1, W + 0.7, yb - 0.02), Open, Open, Open, Open);
            }
            // hatches: steel doors 1.4 x 2.3, 0.04 m proud, with a 0.10 m frame 0.06 proud (outside the capsule reach -5.35)
            for (double s = 1095.0; s < s1; s += Beat06Layout.HatchPitch)
            {
                double y = Beat06Layout.FloorY(s), hw = 0.5 * Beat06Layout.HatchW, hh = Beat06Layout.HatchH;
                OBox(steel, s - hw, s + hw, W, W + 0.04, y + 0.05, y + hh);
                OBox(steel, s - hw - 0.1, s - hw, W, W + 0.06, y, y + hh + 0.1);
                OBox(steel, s + hw, s + hw + 0.1, W, W + 0.06, y, y + hh + 0.1);
                OBox(steel, s - hw - 0.1, s + hw + 0.1, W, W + 0.06, y + hh, y + hh + 0.1);
            }
            // bollards
            for (int j = 0; j < Beat06Layout.Bollards; j++)
            {
                double s = Beat06Layout.BollardS0 + Beat06Layout.BollardPitch * j, y = Beat06Layout.FloorY(s);
                double l0 = W + 0.005, l1 = W + 0.145;
                OBox(bodies, s - 0.1, s + 0.1, l0, l1, y - 0.05, y + Beat06Layout.BollardHeight - 0.1);
                OBox(tops, s - 0.1, s + 0.1, l0, l1, y + Beat06Layout.BollardHeight - 0.1, y + Beat06Layout.BollardHeight);
                if (m_pool != null) m_pool.Add(P(s, l1 + 0.4, y + 0.8), BulwarkPalette.Linear(Pal.Tungsten), 4f, 0.8f, "beat6");
            }
            Flush(ctx, root, ref pipes, "b6_pipes", m_steel, false);
            Flush(ctx, root, ref steel, "b6_wall_steel", m_steel, false);
            Flush(ctx, root, ref bodies, "b6_bollards", m_dark, false);
            Flush(ctx, root, ref tops, "b6_bollard_tops", m_lamp, false);
        }

        /// <summary>Steel glazing frames: jamb strips 0.45 m onto the pier faces at both pane edges (0.15 m proud), the head strip
        /// on the soffit and the sill strip on the trench floor (each 0.45 m, 0.15 proud). They close every pane seam.</summary>
        void BuildGlazing(WorldContext ctx, Transform root)
        {
            var mb = new MeshBuilder();
            double Gi = Beat06Layout.GlassIn, H = Beat06Layout.Height, Td = Beat06Layout.Trench;
            for (int k = 0; k < Beat06Layout.Panes; k++)
            {
                double c = Beat06Layout.PaneS(k), a = c - 10.0, b = c + 10.0;
                double ya = Beat06Layout.FloorY(a), yb = Beat06Layout.FloorY(b);
                OBox(mb, a - 0.45, a, Gi - 0.15, Gi, ya - Td - 0.05, ya + H + 0.05);
                OBox(mb, b, b + 0.45, Gi - 0.15, Gi, yb - Td - 0.05, yb + H + 0.05);
                // head strip (faces down) and sill strip (faces up) along the sloping edges: prisms between a and b
                Slab(mb, a, b, Gi - 0.45, Gi, ya + H - 0.15, yb + H - 0.15, ya + H + 0.02, yb + H + 0.02);
                Slab(mb, a, b, Gi - 0.45, Gi, ya - Td - 0.02, yb - Td - 0.02, ya - Td + 0.15, yb - Td + 0.15);
            }
            Flush(ctx, root, ref mb, "b6_glazing_frames", m_steel, false);
        }

        /// <summary>Ten pane quads (parallelograms on the ramp) at the inner glass face, each drawn twice: the portal (opaque,
        /// Geometry+10) and the in-scatter (additive, Transparent-10).</summary>
        void BuildPortals(WorldContext ctx, Transform root)
        {
            var mb = new MeshBuilder();
            double Gi = Beat06Layout.GlassIn;
            for (int k = 0; k < Beat06Layout.Panes; k++)
            {
                double c = Beat06Layout.PaneS(k), a = c - 10.0, b = c + 10.0;
                double ya = Beat06Layout.FloorY(a), yb = Beat06Layout.FloorY(b);
                // faces -lateral (toward the hall): a bottom, b bottom, b top, a top seen from the hall
                Quad(mb, P(a, Gi, ya + Beat06Layout.PaneBottom), P(b, Gi, yb + Beat06Layout.PaneBottom), P(b, Gi, yb + Beat06Layout.PaneTop), P(a, Gi, ya + Beat06Layout.PaneTop), Open, Open, Open, Open);
            }
            var mesh = mb.ToMesh("b6_panes");
            m_tris += mb.TriangleCount;
            if (m_portal != null) ctx.AddMesh(root, "b6_portals", mesh, m_portal, true, false);
            if (m_scatter != null && !CliArgs.Has("-b6-noscatter")) ctx.AddMesh(root, "b6_portal_scatter", mesh, m_scatter, false, false);
        }

        /// <summary>The 6000-flake snow mesh (BwSnow places every flake in its vertex shader; the mesh holds seeds).</summary>
        void BuildSnow(WorldContext ctx, Transform root)
        {
            if (m_snow == null) return;
            int n = BulwarkConst.SnowQuads;
            var pos = new Vector3[4 * n];
            var uv0 = new Vector2[4 * n];
            var uv1 = new Vector4[4 * n];
            var idx = new int[6 * n];
            Vector2[] corners = { new Vector2(-1f, -1f), new Vector2(1f, -1f), new Vector2(1f, 1f), new Vector2(-1f, 1f) };
            for (int i = 0; i < n; i++)
            {
                uint h = (uint)(i * 2654435761u) ^ 0x9E3779B9u;
                var seed = new Vector3(Hash01(h, 1), Hash01(h, 2), Hash01(h, 3));
                var info = new Vector4(0.6f + 0.8f * Hash01(h, 4), 0.3f + 0.7f * Hash01(h, 5), Hash01(h, 6), 0f);
                for (int c = 0; c < 4; c++)
                {
                    // object positions = the seed (+ a 1e-4 corner spread so no triangle is degenerate)
                    pos[4 * i + c] = seed + new Vector3(corners[c].x, corners[c].y, 0f) * 1e-4f;
                    uv0[4 * i + c] = corners[c];
                    uv1[4 * i + c] = info;
                }
                idx[6 * i] = 4 * i; idx[6 * i + 1] = 4 * i + 2; idx[6 * i + 2] = 4 * i + 1;
                idx[6 * i + 3] = 4 * i; idx[6 * i + 4] = 4 * i + 3; idx[6 * i + 5] = 4 * i + 2;
            }
            var mesh = new Mesh { name = "b6_snow", indexFormat = UnityEngine.Rendering.IndexFormat.UInt32 };
            mesh.vertices = pos; mesh.uv = uv0; mesh.SetUVs(1, uv1); mesh.triangles = idx;
            // the vertex shader moves every flake into the box in front of the camera's panes: bounds cover the whole hall's sea side
            Vector3 c0 = P(1220.0, 30.0, -70.0);
            mesh.bounds = new Bounds(c0, new Vector3(420f, 260f, 420f));
            mesh.UploadMeshData(true);
            var go = new GameObject("b6_snow");
            go.transform.SetParent(root, false);
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            var mr = go.AddComponent<MeshRenderer>();
            mr.sharedMaterial = m_snow;
            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            mr.receiveShadows = false;
            mr.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
            mr.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
            mr.allowOcclusionWhenDynamic = false;
            ctx.Triangles += 2 * n;
            m_tris += 2 * n;
        }

        static float Hash01(uint h, uint salt)
        {
            uint x = h ^ (salt * 0x85EBCA6Bu);
            x ^= x >> 16; x *= 0x7FEB352Du; x ^= x >> 15; x *= 0x846CA68Bu; x ^= x >> 16;
            return (x & 0xFFFFFF) / 16777216f;
        }

        /// <summary>The two survey lamps (A1 1.8): spots outside the glass on the piers' outer faces, housings in the reveals,
        /// LocalLightPool anchors with rank 0.3 (they win a slot while lit).</summary>
        void BuildLamps(WorldContext ctx, Transform root)
        {
            m_lampAPos = P(Beat06Layout.LampAS, Beat06Layout.LampLateral, Beat06Layout.FloorY(Beat06Layout.LampAS) + Beat06Layout.LampAHeight);
            m_lampBPos = P(Beat06Layout.LampBS, Beat06Layout.LampLateral, Beat06Layout.FloorY(Beat06Layout.LampBS) + Beat06Layout.LampBHeight);
            m_lampADir = Aim(CliArgs.GetDouble("-b6-lampayaw", Beat06Layout.LampAYaw), CliArgs.GetDouble("-b6-lampapitch", Beat06Layout.LampAPitch));
            m_lampBDir = Aim(Beat06Layout.LampBYaw, Beat06Layout.LampBPitch);
            if (m_pool == null) return;
            var col = BulwarkPalette.Linear(Pal.Tungsten);
            m_lampA = m_pool.AddSpot(m_lampAPos, Quaternion.LookRotation(m_lampADir, Vector3.up), 2f * (float)Beat06Layout.LampHalfAngle, col,
                                     (float)Beat06Layout.LampRange, LampIntensity, "beat6", 0.3f);
            m_lampB = m_pool.AddSpot(m_lampBPos, Quaternion.LookRotation(m_lampBDir, Vector3.up), 2f * (float)Beat06Layout.LampHalfAngle, col,
                                     (float)Beat06Layout.LampRange, LampIntensity, "beat6", 0.3f);
            m_pool.SetGain(m_lampA, 0f);
            m_pool.SetGain(m_lampB, 0f);
        }

        /// <summary>World direction of a yaw (deg from +z toward +x) and pitch (deg up).</summary>
        static Vector3 Aim(double yawDeg, double pitchDeg)
        {
            double y = yawDeg * Math.PI / 180.0, p = pitchDeg * Math.PI / 180.0;
            return new Vector3((float)(Math.Sin(y) * Math.Cos(p)), (float)Math.Sin(p), (float)(Math.Cos(y) * Math.Cos(p))).normalized;
        }

        void PublishStatic()
        {
            Shader.SetGlobalVector(IdPlane, new Vector4(m_right.x, 0f, m_right.z, (float)m_planeW));
            Shader.SetGlobalVector(IdTan, new Vector4(m_tan.x, 0f, m_tan.z, (float)m_tanW));
            var c = BulwarkPalette.Linear(Pal.Tungsten);
            Shader.SetGlobalVector(IdLampCol, new Vector4(c.r * LampWaterRadiance, c.g * LampWaterRadiance, c.b * LampWaterRadiance, (float)Beat06Layout.LampRange));
            float cosA = Mathf.Cos((float)Beat06Layout.LampHalfAngle * Mathf.Deg2Rad);
            Shader.SetGlobalVector(IdLampA1, new Vector4(m_lampADir.x, m_lampADir.y, m_lampADir.z, cosA));
            Shader.SetGlobalVector(IdLampB1, new Vector4(m_lampBDir.x, m_lampBDir.y, m_lampBDir.z, cosA));
            Shader.SetGlobalVector(IdLampA0, new Vector4(m_lampAPos.x, m_lampAPos.y, m_lampAPos.z, 0f));
            Shader.SetGlobalVector(IdLampB0, new Vector4(m_lampBPos.x, m_lampBPos.y, m_lampBPos.z, 0f));
            Shader.SetGlobalVector(IdState, new Vector4(1f, 0f, 0f, 0f));
            Shader.SetGlobalVector(IdEye, Vector4.zero);
            Shader.SetGlobalVector(IdSnow, Vector4.zero);
            Shader.SetGlobalVector(IdArm0, Vector4.zero);
        }

        /// <summary>World point of the hall's affine frame (s along the straight gallery leg extended both ways, lateral, y):
        /// the arm's knee and root lie north of s 1035 where BwPath turns onto another leg, so they are placed in this frame.</summary>
        Vector3 H(double s, double lat, double y)
        {
            Vector3 p = m_right * (float)(lat + m_planeW) + m_tan * (float)(s - m_tanW);
            p.y = (float)y;
            return p;
        }

        /// <summary>The arm above panes 1-3 (Beat06Layout.CrossArmAt) as the portal's SDF globals; off outside its window.</summary>
        void PublishCrossArm(double s, double t)
        {
            if (s < Beat06Layout.CrossArmS0 || s > Beat06Layout.CrossArmS1 || CliArgs.Has("-b6-nokraken"))
            {
                Shader.SetGlobalVector(IdArm0, Vector4.zero);
                return;
            }
            Beat06Layout.CrossArmAt(s, out double ts, out double tl, out double ty);
            var m = Beat06Layout.CrossArmMid;
            var k = Beat06Layout.CrossArmKnee;
            var r = Beat06Layout.CrossArmRoot;
            // the free end sways slowly on the ambient clock (<= 0.27 m/s); the mid follows with a third of it
            double sy = Beat06Layout.CrossArmSwayY * Math.Sin(2.0 * Math.PI * t / Beat06Layout.CrossArmSwayYPeriod);
            double sl = Beat06Layout.CrossArmSwayL * Math.Sin(2.0 * Math.PI * t / Beat06Layout.CrossArmSwayLPeriod + 1.3);
            Vector3 tip = H(ts, tl + sl, ty + sy), mid = H(ts + m[0], tl + m[1] + sl / 3.0, ty + m[2] + sy / 3.0);
            Vector3 knee = H(ts + k[0], tl + k[1], ty + k[2]), rootP = H(ts + r[0], tl + r[1], ty + r[2]);
            Shader.SetGlobalVector(IdArm0, new Vector4(tip.x, tip.y, tip.z, (float)Beat06Layout.CrossArmTipR));
            Shader.SetGlobalVector(IdArm1, new Vector4(mid.x, mid.y, mid.z, (float)Beat06Layout.CrossArmMidR));
            Shader.SetGlobalVector(IdArm2, new Vector4(knee.x, knee.y, knee.z, (float)Beat06Layout.CrossArmKneeR));
            Shader.SetGlobalVector(IdArm3, new Vector4(rootP.x, rootP.y, rootP.z, (float)Beat06Layout.CrossArmRootR));
        }

        // ================================================================ per frame
        public void OnFrame(in FrameState f)
        {
            double s = f.s, t = f.ambientTime;
            bool inHall = s >= Beat06Layout.CreatureS0 && s <= Beat06Layout.CreatureS1;
            float gA = (float)Beat06Layout.LampA(s), gB = (float)Beat06Layout.LampB(s);
            if (m_pool != null)
            {
                if (m_lampA >= 0) m_pool.SetGain(m_lampA, gA);
                if (m_lampB >= 0) m_pool.SetGain(m_lampB, gB);
            }
            Shader.SetGlobalVector(IdLampA0, new Vector4(m_lampAPos.x, m_lampAPos.y, m_lampAPos.z, gA));
            Shader.SetGlobalVector(IdLampB0, new Vector4(m_lampBPos.x, m_lampBPos.y, m_lampBPos.z, gB));
            double clock = t - 2400.0 * Math.Floor(t / 2400.0);
            double sway = Beat06Layout.SwayDeg(s) * Math.PI / 180.0 * Math.Sin(2.0 * Math.PI * 0.6 * t);
            Shader.SetGlobalVector(IdState, new Vector4((float)Beat06Layout.VeilGain(s), (float)Beat06Layout.Amber(s), (float)sway, (float)clock));
            double drift = BulwarkConst.SnowDrift * t;
            drift -= BulwarkConst.SnowBoxY * Math.Floor(drift / BulwarkConst.SnowBoxY);
            Shader.SetGlobalVector(IdSnow, new Vector4(0f, (float)-drift, (float)Beat06Layout.SnowBulk(s), inHall ? 1f : 0f));
            PublishCrossArm(s, t);
            EncounterFrame(f, inHall);       // Beat06Encounter.cs
        }

        // ================================================================ key-shot subjects (11.13)
        public void CollectSubjects(string shotId, List<KeyShotSubject> into)
        {
            if (shotId == "KS15")
            {
                // pane 1 fills the frame (14.5.6): the subject is the patch of glass at the view centre (anchor rule)
                var k = BulwarkConst.KeyShots[14];
                double y = k.y + (Beat06Layout.GlassIn) * Math.Tan(k.pitch * Math.PI / 180.0);
                into.Add(new KeyShotSubject("b6_pane1", new Bounds(P(Beat06Layout.PaneS(1), Beat06Layout.GlassIn, y), new Vector3(2f, 2f, 2f))));
            }
            // the eye subject belongs to the A1 KS17 (s 1268, the eye abeam); the 14.10.2 table still holds the whale pose
            // (s 1230) until the table owner applies the A1 row (shared change request): no subject for the old pose
            if (shotId == "KS17" && m_kc != null && m_kc.Rig != null && BulwarkConst.KeyShots[16].s > 1260.0)
            {
                Beat06Layout.EyeAt(Beat06Layout.EyeHoldS, out double l, out double y, out double z);
                Vector3 e = P(z, l, y);
                into.Add(new KeyShotSubject("b6_eye", new Bounds(e, new Vector3(20f, 20f, 20f))));
            }
        }

        // ================================================================ mesh helpers
        void Flush(WorldContext ctx, Transform root, ref MeshBuilder mb, string name, Material mat, bool shadows)
        {
            if (mb.VertexCount > 0 && mat != null)
            {
                var mr = ctx.AddMesh(root, name, mb.ToMesh(name), mat, shadows, true);
                mr.shadowCastingMode = shadows ? UnityEngine.Rendering.ShadowCastingMode.On : UnityEngine.Rendering.ShadowCastingMode.Off;
                m_tris += mb.TriangleCount;
            }
            mb = new MeshBuilder();
        }

        /// <summary>World point at path s, lateral, absolute y.</summary>
        static Vector3 P(double s, double lat, double y)
        {
            BwPath.World(s, lat, out double x, out _, out double z);
            return new Vector3((float)x, (float)y, (float)z);
        }

        static readonly Color32 Open = new Color32(255, 0, 0, 255);

        /// <summary>Planar quad a, b, c, d (clockwise seen from the front, as MeshBuilder.FlatQuad) with per-corner colours.</summary>
        static void Quad(MeshBuilder mb, Vector3 a, Vector3 b, Vector3 c, Vector3 d, Color32 ca, Color32 cb, Color32 cc, Color32 cd)
        {
            Vector3 n = Vector3.Cross(b - a, c - a) + Vector3.Cross(c - a, d - a);
            if (n.sqrMagnitude < 1e-14f) return;
            n.Normalize();
            Color32 saved = mb.Color;
            int i0;
            mb.Color = ca; i0 = mb.Vertex(a, n);
            mb.Color = cb; mb.Vertex(b, n);
            mb.Color = cc; mb.Vertex(c, n);
            mb.Color = cd; mb.Vertex(d, n);
            mb.Color = saved;
            mb.Tri(i0, i0 + 1, i0 + 2);
            mb.Tri(i0, i0 + 2, i0 + 3);
        }

        // ---------------------------------------------------------------- region-ambient compensation (vertex AO)
        // The interior boxes of the shared atmosphere (BulwarkSchedule.InteriorBoxes) do not yet cover the whole 28 m hall: its
        // ceiling rises above the box top (y -45) for s < ~1180 and the glass plane passes the box side (x 14) for s > ~1320,
        // so those surfaces would take the bright EXTERIOR trilight inside a closed hall (Part 10 #36). Until the table is
        // widened (shared change request), the shell's vertex AO scales the region ambient there down to the interior level:
        // AO = |interior| / |lerp(exterior(n), interior, w)| with w the interior weight at the shader's probe point (p + n
        // edge) computed from the same table, so it is exactly 1 wherever the table already covers the hall and follows any
        // later change of the table automatically.
        static Color32 AoFor(Vector3 p, Vector3 n, double s, byte baseAo)
        {
            double e = BulwarkSchedule.InteriorEdge;
            Vector3 q = p + n * (float)e;
            double w = BulwarkSchedule.InteriorWeightSoft(q.x, q.y, q.z, e);
            if (w > 0.999) return new Color32(baseAo, 0, 0, 255);
            var g = BulwarkSchedule.Evaluate(Math.Max(1070.0, Math.Min(1370.0, s)));
            double u = n.y, sky = Math.Max(u, 0.0), ground = Math.Max(-u, 0.0), midW = 1.0 - sky - ground;
            double ext = Lum(g.AmbExtSky) * sky + Lum(g.AmbExtMid) * midW + Lum(g.AmbExtGround) * ground;
            double inn = Lum(g.AmbInt) * (sky + midW + 0.8 * ground);
            double mix = ext + (inn - ext) * w;
            double k = mix > 1e-6 ? Math.Max(0.03, Math.Min(1.0, inn / mix)) : 1.0;
            return new Color32((byte)Math.Round(baseAo * k), 0, 0, 255);
        }

        static double Lum(Color c) => 0.2126 * c.r + 0.7152 * c.g + 0.0722 * c.b;

        /// <summary>A vertical wall strip between s a and b at lateral lat, from y0 to y1 (linear along s), in rows of <= 2 m;
        /// faceRight: the face looks to +lateral. Corner AO within 1.2 m of the bottom and 1.5 m of the top of a strip of the
        /// given height above floorA/B, times the region compensation.</summary>
        void WallGrid(MeshBuilder mb, double a, double b, double lat, double y0a, double y0b, double y1a, double y1b, bool faceRight,
                      double floorA, double floorB, double height)
        {
            int rows = Math.Max(1, (int)Math.Ceiling(Math.Max(y1a - y0a, y1b - y0b) / 2.0));
            Vector3 n = faceRight ? m_right : -m_right;
            for (int r = 0; r < rows; r++)
            {
                double t0 = r / (double)rows, t1 = (r + 1) / (double)rows;
                double ya0 = y0a + (y1a - y0a) * t0, ya1 = y0a + (y1a - y0a) * t1, yb0 = y0b + (y1b - y0b) * t0, yb1 = y0b + (y1b - y0b) * t1;
                Vector3 pa0 = P(a, lat, ya0), pa1 = P(a, lat, ya1), pb0 = P(b, lat, yb0), pb1 = P(b, lat, yb1);
                Color32 ca0 = AoFor(pa0, n, a, CornerAo(ya0 - floorA, height)), ca1 = AoFor(pa1, n, a, CornerAo(ya1 - floorA, height));
                Color32 cb0 = AoFor(pb0, n, b, CornerAo(yb0 - floorB, height)), cb1 = AoFor(pb1, n, b, CornerAo(yb1 - floorB, height));
                if (faceRight) Quad(mb, pa0, pa1, pb1, pb0, ca0, ca1, cb1, cb0);
                else Quad(mb, pa0, pb0, pb1, pa1, ca0, cb0, cb1, ca1);
            }
        }

        static byte CornerAo(double h, double height)
        {
            double k = 1.0;
            if (h < 1.2) k = Math.Min(k, 0.55 + 0.45 * Math.Max(0.0, h) / 1.2);
            if (height - h < 1.5) k = Math.Min(k, 0.55 + 0.45 * Math.Max(0.0, height - h) / 1.5);
            return (byte)Math.Round(255.0 * k);
        }

        /// <summary>Soffit strip (faces down) between s a and b, lateral l0..l1 in cells of <= 2 m, with the compensation.</summary>
        void CeilGrid(MeshBuilder mb, double a, double b, double l0, double l1, double ya, double yb)
        {
            int cols = Math.Max(1, (int)Math.Ceiling((l1 - l0) / 2.0));
            for (int c = 0; c < cols; c++)
            {
                double m0 = l0 + (l1 - l0) * c / cols, m1 = l0 + (l1 - l0) * (c + 1) / cols;
                Vector3 p00 = P(a, m0, ya), p01 = P(a, m1, ya), p11 = P(b, m1, yb), p10 = P(b, m0, yb);
                Quad(mb, p00, p01, p11, p10, AoFor(p00, Vector3.down, a, 140), AoFor(p01, Vector3.down, a, 140), AoFor(p11, Vector3.down, b, 140), AoFor(p10, Vector3.down, b, 140));
            }
        }

        /// <summary>Floor strip (faces up) between s a and b, lateral l0..l1 in cells of <= 2 m, with the compensation.</summary>
        void FloorGrid(MeshBuilder mb, double a, double b, double l0, double l1, double ya, double yb)
        {
            int cols = Math.Max(1, (int)Math.Ceiling((l1 - l0) / 2.0));
            for (int c = 0; c < cols; c++)
            {
                double m0 = l0 + (l1 - l0) * c / cols, m1 = l0 + (l1 - l0) * (c + 1) / cols;
                Vector3 p00 = P(a, m0, ya), p10 = P(b, m0, yb), p11 = P(b, m1, yb), p01 = P(a, m1, ya);
                Quad(mb, p00, p10, p11, p01, AoFor(p00, Vector3.up, a, 140), AoFor(p10, Vector3.up, b, 140), AoFor(p11, Vector3.up, b, 140), AoFor(p01, Vector3.up, a, 140));
            }
        }

        /// <summary>Vertical quad across the path at s, lateral [l0, l1], y [y0, y1]; forward: the face looks along +s.</summary>
        static void CrossQuad(MeshBuilder mb, double s, double l0, double l1, double y0, double y1, bool forward)
        {
            if (forward) Quad(mb, P(s, l0, y0), P(s, l1, y0), P(s, l1, y1), P(s, l0, y1), Open, Open, Open, Open);
            else Quad(mb, P(s, l0, y0), P(s, l0, y1), P(s, l1, y1), P(s, l1, y0), Open, Open, Open, Open);
        }

        static void CrossWall(MeshBuilder mb, double s, double l0, double l1, double y0, double y1)
        {
            CrossQuad(mb, s, l0, l1, y0, y1, true);
            CrossQuad(mb, s, l0, l1, y0, y1, false);
        }

        /// <summary>Box in hall coordinates: s [s0, s1], lateral [l0, l1], absolute y [y0, y1] (horizontal top and bottom).</summary>
        static void OBox(MeshBuilder mb, double s0, double s1, double l0, double l1, double y0, double y1)
            => Slab(mb, s0, s1, l0, l1, y0, y0, y1, y1);

        /// <summary>Hexahedron in hall coordinates with the bottom y0a..y0b and top y1a..y1b varying linearly along s (a slab on
        /// the ramp); six outward faces.</summary>
        static void Slab(MeshBuilder mb, double s0, double s1, double l0, double l1, double y0a, double y0b, double y1a, double y1b)
            => Slab(mb, s0, s1, l0, l1, y0a, y0b, y1a, y1b, Open);

        static void Slab(MeshBuilder mb, double s0, double s1, double l0, double l1, double y0a, double y0b, double y1a, double y1b, Color32 c)
        {
            var Open = c;
            Vector3 a0 = P(s0, l0, y0a), a1 = P(s0, l1, y0a), a2 = P(s0, l1, y1a), a3 = P(s0, l0, y1a);
            Vector3 b0 = P(s1, l0, y0b), b1 = P(s1, l1, y0b), b2 = P(s1, l1, y1b), b3 = P(s1, l0, y1b);
            Quad(mb, a0, a3, a2, a1, Open, Open, Open, Open);   // s0 face (looks along -s)
            Quad(mb, b0, b1, b2, b3, Open, Open, Open, Open);   // s1 face (+s)
            Quad(mb, a0, b0, b3, a3, Open, Open, Open, Open);   // l0 face (-lateral)
            Quad(mb, a1, a2, b2, b1, Open, Open, Open, Open);   // l1 face (+lateral)
            Quad(mb, a3, b3, b2, a2, Open, Open, Open, Open);   // top
            Quad(mb, a0, a1, b1, b0, Open, Open, Open, Open);   // bottom
        }

        /// <summary>Round tube from a to b (BwLine _LINE_TUBE contract for the rail; smooth normals), a ring at least every 2 m.</summary>
        static void Tube(MeshBuilder mb, Vector3 a, Vector3 b, float radius, int sides = 10)
        {
            Vector3 d = b - a;
            float len = d.magnitude;
            if (len < 1e-4f || radius <= 0f) return;
            Vector3 dir = d / len;
            Vector3 h = Mathf.Abs(dir.y) < 0.9f ? Vector3.up : Vector3.right;
            Vector3 u = Vector3.Cross(h, dir).normalized, v = Vector3.Cross(dir, u);
            int rings = Mathf.Max(2, Mathf.CeilToInt(len / 2f) + 1);
            int first = mb.VertexCount;
            for (int i = 0; i < rings; i++)
            {
                Vector3 c = a + d * (i / (float)(rings - 1));
                for (int j = 0; j < sides; j++)
                {
                    float ph = 2f * Mathf.PI * j / sides;
                    Vector3 n = u * Mathf.Cos(ph) + v * Mathf.Sin(ph);
                    mb.Vertex(c + n * radius, n);
                }
            }
            for (int i = 0; i + 1 < rings; i++)
                for (int j = 0; j < sides; j++)
                {
                    int jn = (j + 1) % sides;
                    int A = first + i * sides + j, D = first + i * sides + jn, Cc = first + (i + 1) * sides + jn, B = first + (i + 1) * sides + j;
                    mb.Quad(A, D, Cc, B);
                }
        }
    }
}
