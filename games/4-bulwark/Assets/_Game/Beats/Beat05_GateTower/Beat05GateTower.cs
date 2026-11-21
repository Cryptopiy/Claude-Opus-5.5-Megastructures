// Beat05GateTower: beat 5, the gate tower atrium and the descent (MASTER_PROMPT 14.5.5: KS13, KS14; 14.2.5 the bright-to-
// dark rule at the portal). Replaces the graybox of beat 5 ([ReplacesGraybox("beat5")]): inside the tower shell (beat 4's,
// inner faces x -5.5 / 5.5, z 920.5 / 955.5) the atrium floor, the 9 m ceiling with four beams, the six Tungsten pendants
// (pool anchors with the portal lamp boost, sway on impacts), the guard rail between the pedestrian way and the vehicle
// lane, the open well and the slab that closes over the stairs, the vehicle deck beside the well; from the stair mouth
// (s 934) the lane-driven stairs: 270 treads and risers on the exact BwPath.FloorY with ochre steel nosings, 14 landings
// with k painted chevrons on landing k, walls with handrail grooves and ochre handrails, wet streaks growing from landing
// 6 to 14 (BwConcrete drip weight in vertex B), a caged lamp in a wall niche per landing, the three portholes (bores
// through to the sea face, cyan glass, a cyan pool light each), the soffit; then the 4.4 x 4.0 exit hall with a flush lamp
// every 8 m to the gallery doorway at s 1070 (beat 6's wall). Pure numbers: Beat05Layout (Beat05Tests).
// Exposure: the 14.2.5 rule is the AtmosphereDriver's (EV rise s 912-952) and the lamp gain _BwSurface.z (portal boost
// 2^(1.3 (1 - smootherstep((s - 920) / 40)))): LampTungsten follows it (_GainFollow 1) and every Tungsten anchor here is
// registered with lampBoost, so emission and real light rise together.
using System;
using System.Collections.Generic;
using Megastructure.Rendering;
using UnityEngine;

namespace Megastructure.Beats
{
    [ReplacesGraybox("beat5")]
    public sealed class Beat05GateTower : IBeatBuilder, IWorldUpdater
    {
        public string Name => "Beat05_GateTower";

        Material m_wire, m_rod, m_flange, m_floor, m_tread, m_floorWet, m_wall, m_ceiling, m_dark, m_steel, m_ochre, m_paint, m_nosing, m_rail, m_handrail, m_tungsten;
        readonly Material[] m_port = new Material[3];
        public readonly List<Material> RuntimeMaterials = new List<Material>();
        public Transform Root { get; private set; }
        int m_tris;
        LocalLightPool m_pool;
        readonly List<Transform> m_pendants = new List<Transform>(6);
        readonly List<int> m_pendantLights = new List<int>(6);
        readonly List<Vector3> m_pendantPivot = new List<Vector3>(6);
        readonly float[] m_pendantAngle = new float[6];
        /// <summary>Kraken shade of portholes 0-2 (landings 5, 8, 11), 0..1: the water behind the glass darkens to 15 % (Addendum
        /// A1 K6, luma 98 -> <= 20). Hook for the kraken choreography (writer); read every frame by this builder.</summary>
        public static readonly float[] PortholeShade = new float[3];
        public const string PortholeShader = "Megastructure/BwB5Porthole";
        readonly float[] m_shade = { -1f, -1f, -1f };
        static readonly int IdShade = Shader.PropertyToID("_Shade");
        /// <summary>Porthole glass radiance (x SeaTeal) by landing 5, 8, 11: deeper water is darker (14.5.5 luma 161, 125, 98).</summary>
        public static readonly float[] PortholeIntensity = { 13.0f, 7.5f, 4.8f };   // interiors-08: 3.4 / 2.4 / 1.7 gave water luma 84 / 70 / 60, interiors-12: 11 / 7.5 / 4.8 gave 143 / 120 / 99 (spec 161 / 125 / 98)

        // ================================================================ build
        public void Build(WorldContext ctx)
        {
            var root = new GameObject(Name).transform;
            root.SetParent(ctx.Root, false);
            Root = root;
            CreateMaterials(ctx);
            m_pool = LocalLightPool.Ensure(ctx);
            BuildAtrium(ctx, root);
            BuildStairs(ctx, root);
            BuildLamps(ctx, root);
            BuildPortholes(ctx, root);
            BuildMotes(ctx, root);
            BuildDrip(ctx, root);
            ctx.PrewarmSets.Add(new PrewarmSet { Name = "Beat05_Atrium", Eye = new Vector3(-1.5f, 1.7f, 926f), YawDeg = 0f, PitchDeg = 14f, Enable = _ => { } });
            ctx.PrewarmSets.Add(new PrewarmSet { Name = "Beat05_Stairs", Eye = new Vector3(1.11f, -25.3f, 985f), YawDeg = 3f, PitchDeg = -24f, Enable = _ => { } });
            ctx.Register(this);
            ctx.Log("Beat05_GateTower: atrium, " + BulwarkConst.StairSteps + " steps, " + Beat05Layout.Landings + " landings, 3 portholes, exit hall ("
                    + m_tris + " triangles)");
        }

        void CreateMaterials(WorldContext ctx)
        {
            var lib = ctx.Materials;
            m_floor = Copy(lib != null ? lib.Concrete : null, "b5_floor");
            Set(m_floor, "_WetFollow", 0f); Set(m_floor, "_Wet", 0.08f);
            // the treads and landings: the darker basalt concrete (ConcreteDark), damp from landing 6 on: under lamps 2 m above,
            // ConcreteMid treads read near white and flattened the flights (interiors-09 land6); the ochre nosings now carry the rhythm
            m_tread = Copy(lib != null ? lib.ConcreteDark : null, "b5_treads");
            Set(m_tread, "_WetFollow", 0f); Set(m_tread, "_Wet", 0.1f);
            m_floorWet = Copy(lib != null ? lib.ConcreteDark : null, "b5_treads_wet");
            Set(m_floorWet, "_WetFollow", 0f); Set(m_floorWet, "_Wet", 0.45f);
            m_wall = Copy(lib != null ? lib.Concrete : null, "b5_wall");
            Set(m_wall, "_WetFollow", 0f); Set(m_wall, "_DripStrength", 0.9f);
            m_ceiling = Copy(lib != null ? lib.Concrete : null, "b5_ceiling");
            Set(m_ceiling, "_WetFollow", 0f); Set(m_ceiling, "_DripStrength", 0.2f);
            m_dark = Copy(lib != null ? lib.ConcreteDark : null, "b5_dark");
            Set(m_dark, "_WetFollow", 0f); Set(m_dark, "_Wet", 0.5f);
            m_flange = Copy(lib != null ? lib.ConcreteDark : null, "b5_flange");
            Set(m_flange, "_WetFollow", 0f); Set(m_flange, "_Wet", 0f); Set(m_flange, "_Smoothness", 0.15f);
            m_steel = lib != null ? lib.Steel : null;
            m_ochre = lib != null ? lib.SteelOchre : null;
            // nosings: a worn, darker ochre steel angle (seen from above a 33.7 deg flight the front 3 cm of every tread is
            // most of what shows: full SafetyOchre turned the flight into one orange slab, interiors-02 KS14)
            m_nosing = Copy(m_ochre, "b5_nosing");
            if (m_nosing != null && m_nosing.HasProperty("_BaseColor"))
            {
                var o = BulwarkPalette.Linear(Pal.SafetyOchre);
                m_nosing.SetColor("_BaseColor", new Color(o.r * 0.55f, o.g * 0.55f, o.b * 0.55f, 1f));
            }
            Set(m_nosing, "_ChipAmount", 0.04f);
            // ochre floor paint (chevrons, kerb): the concrete recoloured to SafetyOchre
            m_paint = Copy(lib != null ? lib.Concrete : null, "b5_paint_ochre");
            if (m_paint != null && m_paint.HasProperty("_BaseColor")) m_paint.SetColor("_BaseColor", BulwarkPalette.Linear(Pal.SafetyOchre));
            Set(m_paint, "_WetFollow", 0f); Set(m_paint, "_DripStrength", 0f); Set(m_paint, "_MacroTone", 0.05f);
            m_rail = lib != null ? lib.Rail : null;
            m_handrail = lib != null ? lib.Handrail : null;
            m_tungsten = lib != null ? lib.LampTungsten : null;
            // thin steel through BwLine (H7): the landing-lamp cage bars 12 mm, the pendant rods 24 mm
            var sp = BulwarkPalette.Linear(Pal.SteelPaint);
            m_wire = Copy(lib != null ? lib.Rail : null, "b5_cage_wire");
            if (m_wire != null) { m_wire.SetFloat("_LineWidth", 0.012f); m_wire.SetColor("_BaseColor", new Color(sp.r * 0.6f, sp.g * 0.6f, sp.b * 0.6f, 1f)); }
            m_rod = Copy(lib != null ? lib.Rail : null, "b5_pendant_rod");
            if (m_rod != null) { m_rod.SetFloat("_LineWidth", 2f * (float)Beat05Layout.PendantRodR); m_rod.SetColor("_BaseColor", sp); }
            var ps = Shader.Find(PortholeShader);
            if (ps == null) Debug.LogWarning("[BW] Beat05: shader " + PortholeShader + " not found (RebuildAll): emissive stand-in discs");
            for (int i = 0; i < 3; i++)
            {
                Material m;
                if (ps != null)
                {
                    m = new Material(ps) { name = "b5_porthole_" + i };
                    RuntimeMaterials.Add(m);
                    m.SetColor("_Color", BulwarkPalette.Linear(Pal.SeaTeal));
                    m.SetFloat("_Intensity", PortholeIntensity[i]);
                    m.SetFloat("_Phase", 2.1f * i);
                }
                else
                {
                    m = Copy(lib != null ? lib.LampSodium : null, "b5_porthole_" + i);
                    if (m != null)
                    {
                        if (m.HasProperty("_EmissionColor")) m.SetColor("_EmissionColor", BulwarkPalette.Linear(Pal.SeaTeal));
                        Set(m, "_Intensity", PortholeIntensity[i]); Set(m, "_GainFollow", 0f); Set(m, "_GlassFalloff", 0.25f);
                    }
                }
                m_port[i] = m;
            }
        }

        static void Set(Material m, string prop, float v) { if (m != null && m.HasProperty(prop)) m.SetFloat(prop, v); }

        Material Copy(Material src, string name)
        {
            if (src == null) return null;
            var m = new Material(src) { name = name };
            RuntimeMaterials.Add(m);
            return m;
        }

        void Flush(WorldContext ctx, Transform parent, ref MeshBuilder mb, string name, Material mat, bool shadows = true)
        {
            if (mb.VertexCount > 0 && mat != null)
            {
                ctx.AddMesh(parent, name, mb.ToMesh(name), mat, shadows, true);
                m_tris += mb.TriangleCount;
            }
            mb = new MeshBuilder();
        }

        static Color32 Ao(byte r, byte g = 0, byte b = 0) => new Color32(r, g, b, 255);
        static Vector3 V(double x, double y, double z) => new Vector3((float)x, (float)y, (float)z);

        static Vector3 P(double s, double lat, double y)
        {
            BwPath.World(s, lat, out double x, out _, out double z);
            return new Vector3((float)x, (float)y, (float)z);
        }

        // ================================================================ atrium (z 920 .. 956 inside the tower shell)
        void BuildAtrium(WorldContext ctx, Transform root)
        {
            var floor = new MeshBuilder();
            var ceil = new MeshBuilder();
            var wall = new MeshBuilder();
            double x0 = Beat05Layout.TowerInX0, x1 = Beat05Layout.TowerInX1, tz0 = BulwarkConst.TowerZ0, tz1 = BulwarkConst.TowerZ1;
            // floor and ceiling run on under / into the tower's 0.5 m walls (x -6 .. 6, z 920 .. 956): the wall faces then stand on
            // them and no crack can open along the junction (interiors-final3 KS13 4K: one grey pixel at the floor-wall edge)
            double fx0 = BulwarkConst.TowerXMin, fx1 = BulwarkConst.TowerXMax;
            double aZ = BulwarkConst.AtriumZ1, cy0 = Beat05Layout.CeilingY0;
            double zWell = WellClosedZ();
            // atrium floor z 920 .. 934 (the pedestrian way and the vehicle lane); slab edge at the stair mouth faces the well
            floor.Color = Ao(255);
            InteriorQuads.RectY(floor, 0.0, fx0, fx1, tz0, aZ, +1);
            InteriorQuads.RectZ(floor, aZ, BulwarkConst.StairMouthXMin, BulwarkConst.StairMouthXMax, Beat05Layout.WellDeckUnderside, 0.0, +1);
            // the slab that closes the floor over the stairs (north of the open well), its south edge 0.12 m below the deck
            // underside (no sight line into the hollow over the stair soffit; headroom 2.48 >= 2.4)
            InteriorQuads.RectY(floor, 0.0, fx0, fx1, zWell, tz1, +1);
            InteriorQuads.RectZ(floor, zWell, x0, x1, Beat05Layout.WellDeckUnderside - Beat05Layout.WellSkirt, 0.0, -1);
            InteriorQuads.RectY(floor, Beat05Layout.WellDeckUnderside - Beat05Layout.WellSkirt, x0, x1, zWell, zWell + 0.6, -1);
            // the space west of the well behind the well wall (z 934 .. zWell): its floor (never seen from the walk)
            InteriorQuads.RectY(floor, 0.0, fx0, BulwarkConst.StairMouthXMin - 0.2, aZ, zWell, +1);
            Flush(ctx, root, ref floor, "b5_atrium_floor", m_floor);

            // ceiling y 9 over the whole tower interior with four beams (0.5 x 0.8) across it
            ceil.Color = Ao(235);
            InteriorQuads.RectY(ceil, cy0, fx0, fx1, tz0, tz1, -1);
            foreach (double bz in Beat05Layout.BeamZ)
            {
                double b0 = bz - Beat05Layout.BeamHalf, b1 = bz + Beat05Layout.BeamHalf, by = cy0 - Beat05Layout.BeamDepth;
                ceil.Color = Ao(255, 0, 0);
                InteriorQuads.RectY(ceil, by, fx0, fx1, b0, b1, -1);
                ceil.Color = Ao(220, 60, 0);
                InteriorQuads.RectZ(ceil, b0, fx0, fx1, by, cy0, -1);
                InteriorQuads.RectZ(ceil, b1, fx0, fx1, by, cy0, +1);
            }
            Flush(ctx, root, ref ceil, "b5_atrium_ceiling", m_ceiling);

            // the well wall west of the stair mouth (x -5.5 .. -3.3 at z 934, to the ceiling), both faces
            wall.Color = Ao(255);
            double wx1 = BulwarkConst.StairMouthXMin;
            InteriorQuads.RectZ(wall, aZ - 0.3, fx0, wx1, 0.0, cy0, -1);
            InteriorQuads.RectX(wall, wx1, aZ - 0.3, aZ, 0.0, cy0, +1);
            InteriorQuads.RectZ(wall, aZ, fx0, wx1, 0.0, cy0, +1);
            // low pilasters on the tower's side walls under the beams (scale: 9 m, the beams carry the 70 m shaft above)
            foreach (double bz in Beat05Layout.BeamZ)
            {
                InteriorQuads.CBox(wall, x0, 0.0, bz - 0.3, x0 + 0.35, cy0 - 0.02, bz + 0.3, 0.02f);
                InteriorQuads.CBox(wall, x1 - 0.35, 0.0, bz - 0.3, x1, cy0 - 0.02, bz + 0.3, 0.02f);
            }
            Flush(ctx, root, ref wall, "b5_atrium_walls", m_wall);

            // guard rail 1.1 m (rails 1.1 / 0.55, posts every 2 m) from the south wall to the stair mouth at x 0.55, then 0.25 m
            // outside the stairwell's east wall along the open well to the closing line
            var rails = new MeshBuilder();
            var posts = new MeshBuilder();
            float rr = LineRadius(m_rail, 0.05f);
            var g0 = new Vector3((float)Beat05Layout.GuardX, 0f, (float)(tz0 + 0.6));
            var g1 = new Vector3((float)Beat05Layout.GuardX, 0f, (float)aZ);
            Vector3 we = P(Beat05Layout.WellClosedS(), Beat05Layout.WellRailLateral(), 0.0);
            var g2 = new Vector3(we.x, 0f, (float)zWell);
            GuardRun(rails, posts, g0, g1, rr, true);
            GuardRun(rails, posts, g1, g2, rr, false);
            InteriorQuads.CBox(posts, g2.x - 0.05, 0.0, g2.z - 0.05, g2.x + 0.05, BulwarkConst.GuardRailHeight - 2.0 * rr, g2.z + 0.05, 0.01f);
            Flush(ctx, root, ref rails, "b5_rails_atrium", m_rail, false);
            Flush(ctx, root, ref posts, "b5_atrium_posts", m_steel);
            // vehicle lane kerb (0.15 m) along the guard rail line, ochre-banded (the vehicle lane x 0.8 .. 5.5)
            var kerb = new MeshBuilder();
            kerb.Color = Ao(255, 0, 0);
            InteriorQuads.CBox(kerb, Beat05Layout.GuardX + 0.1, 0.0, tz0 + 0.6, Beat05Layout.GuardX + 0.25, 0.15, aZ - 0.02, 0.015f);
            Flush(ctx, root, ref kerb, "b5_kerb", m_paint);
        }

        static double WellClosedZ() { BwPath.Point(Beat05Layout.WellClosedS(), out _, out double z); return z; }

        static float LineRadius(Material m, float fallback)
        {
            if (m != null && m.HasProperty("_LineWidth")) { float w = m.GetFloat("_LineWidth"); if (w > 1e-4f) return 0.5f * w; }
            return fallback;
        }

        static void GuardRun(MeshBuilder rails, MeshBuilder posts, Vector3 a, Vector3 b, float rr, bool firstPost)
        {
            Vector3 up = Vector3.up;
            InteriorQuads.Tube(rails, a + up * (float)(BulwarkConst.GuardRailHeight - rr), b + up * (float)(BulwarkConst.GuardRailHeight - rr), rr, 10, 2f);
            InteriorQuads.Tube(rails, a + up * (0.55f - rr), b + up * (0.55f - rr), rr, 10, 2f);
            float len = (b - a).magnitude;
            int n = Mathf.Max(1, Mathf.CeilToInt(len / 2f));
            for (int i = firstPost ? 0 : 1; i < n; i++)
            {
                Vector3 p = Vector3.Lerp(a, b, i / (float)n);
                InteriorQuads.CBox(posts, p.x - 0.05, 0.0, p.z - 0.05, p.x + 0.05, BulwarkConst.GuardRailHeight - 2.0 * rr, p.z + 0.05, 0.01f);
            }
        }

        // ================================================================ stairs, walls, soffit, exit hall (lane-driven)
        static List<double> Knots()
        {
            var k = new List<double>(2048);
            double s0 = Beat05Layout.LaneStartS, s1 = Beat05Layout.EndS;
            for (double s = s0; s < s1; s += 2.0) k.Add(s);
            k.Add(s1);
            void Fine(double a, double b) { for (double s = a; s < b; s += 0.5) k.Add(s); k.Add(b); }
            Fine(BwPath.A1StartS, BwPath.A1EndS);
            Fine(BwPath.StairsEndS, BwPath.A2EndS);
            Fine(BulwarkConst.LaneHallOpenS0, BulwarkConst.LaneHallOpenS1);
            for (int i = 0; i < BulwarkConst.StairSteps; i++)
            {
                double r = BwPath.StairRiserS(i);
                k.Add(r);
                k.Add(r - Beat05Layout.NosingDepth);
            }
            for (int f = 0; f < BulwarkConst.StairFlights; f++) { double fs = BwPath.StairFlightStartS(f); k.Add(fs); k.Add(fs + BulwarkConst.StairFlightRun); }
            for (int l = 1; l <= Beat05Layout.Landings; l++)
            {
                double c = Beat05Layout.LandingCentre(l);
                k.Add(c - Beat05Layout.PortholeR); k.Add(c + Beat05Layout.PortholeR);
                k.Add(c - Beat05Layout.LampNicheHalf); k.Add(c + Beat05Layout.LampNicheHalf);
            }
            k.Add(BwPath.StairsStartS); k.Add(BwPath.StairsEndS); k.Add(Beat05Layout.WellClosedS());
            for (int h = 0; h < Beat05Layout.ExitHallLamps; h++) { k.Add(Beat05Layout.ExitHallLampS(h) - 0.25); k.Add(Beat05Layout.ExitHallLampS(h) + 0.25); }
            k.Sort();
            var o = new List<double>(k.Count);
            foreach (double s in k)
            {
                if (s < s0 - 1e-9 || s > s1 + 1e-9) continue;
                if (o.Count > 0 && s - o[o.Count - 1] < 1e-4) continue;
                o.Add(s);
            }
            return o;
        }

        static double CeilingAt(double s, int side)
        {
            double lo = Beat05Layout.CeilingY(s - 1e-6), hi = Beat05Layout.CeilingY(s + 1e-6);
            if (Math.Abs(hi - lo) < 1e-3) return Beat05Layout.CeilingY(s);
            return side > 0 ? hi : lo;
        }

        // quads in lane coordinates (the stair leg is straight at heading 3 deg; arcs and blends are split at 0.5 m)
        static void FloorQuad(MeshBuilder mb, double a, double b, double l0, double l1, double m0, double m1, double ya, double yb)
            => InteriorQuads.Face(mb, P(a, l0, ya), P(b, m0, yb), P(b, m1, yb), P(a, l1, ya), Vector3.up);
        static void CeilQuad(MeshBuilder mb, double a, double b, double l0, double l1, double m0, double m1, double ya, double yb)
            => InteriorQuads.Face(mb, P(a, l0, ya), P(a, l1, ya), P(b, m1, yb), P(b, m0, yb), Vector3.down);
        static void SideQuad(MeshBuilder mb, double a, double b, double la, double lb, double ya0, double yb0, double ya1, double yb1, bool faceRight)
        {
            if (ya1 - ya0 < 1e-5 && yb1 - yb0 < 1e-5) return;
            BwPath.Right(0.5 * (a + b), out double rx, out double rz);
            var n = new Vector3((float)rx, 0f, (float)rz) * (faceRight ? 1f : -1f);
            InteriorQuads.Face(mb, P(a, la, ya0), P(a, la, ya1), P(b, lb, yb1), P(b, lb, yb0), n);
        }
        static void CrossQuad(MeshBuilder mb, double s, double l0, double l1, double y0, double y1, bool forward)
        {
            BwPath.Tangent(s, out double tx, out double tz);
            var n = new Vector3((float)tx, 0f, (float)tz) * (forward ? 1f : -1f);
            InteriorQuads.Face(mb, P(s, l0, y0), P(s, l1, y0), P(s, l1, y1), P(s, l0, y1), n);
        }
        /// <summary>A box in lane coordinates (s, lateral, y), straight legs only (6 faces).</summary>
        static void LaneBox(MeshBuilder mb, double s0, double s1, double l0, double l1, double y0, double y1)
        {
            FloorQuad(mb, s0, s1, l0, l1, l0, l1, y1, y1);
            CeilQuad(mb, s0, s1, l0, l1, l0, l1, y0, y0);
            SideQuad(mb, s0, s1, l0, l0, y0, y0, y1, y1, false);
            SideQuad(mb, s0, s1, l1, l1, y0, y0, y1, y1, true);
            CrossQuad(mb, s0, l0, l1, y0, y1, false);
            CrossQuad(mb, s1, l0, l1, y0, y1, true);
        }

        static bool InStairs(double a, double b) => a >= BwPath.StairsStartS - 1e-9 && b <= BwPath.StairsEndS + 1e-9;

        /// <summary>Landing k (1..14) whose s range contains s, else 0.</summary>
        static int LandingAt(double s)
        {
            for (int k = 1; k <= Beat05Layout.Landings; k++)
            {
                Beat05Layout.Landing(k, out double a, out double b, out _);
                if (s > a - 1e-6 && s < b + 1e-6) return k;
            }
            return 0;
        }

        static int PortholeIndex(int landing) => Array.IndexOf(Beat05Layout.PortholeLanding, landing);

        void BuildStairs(WorldContext ctx, Transform root)
        {
            var knots = Knots();
            var risers = new HashSet<long>();
            for (int i = 0; i < BulwarkConst.StairSteps; i++) risers.Add(Key(BwPath.StairRiserS(i)));
            var floorDry = new MeshBuilder();
            var floorWet = new MeshBuilder();
            var walls = new MeshBuilder();
            var soffit = new MeshBuilder();
            var deck = new MeshBuilder();
            var hand = new MeshBuilder();
            var nosing = new MeshBuilder();
            var niche = new MeshBuilder();
            var backing = new MeshBuilder();
            float hr = LineRadius(m_handrail, 0.025f);
            bool grooveL = false, grooveR = false;
            double wellS = Beat05Layout.WellClosedS();
            double wetS = Beat05Layout.LandingCentre(6) - 0.6 - 1e-6;   // treads from landing 6 on are damp (the material swaps at its start)
            double gh = Beat05Layout.GrooveHalf, gd = Beat05Layout.GrooveDepth;
            for (int i = 0; i + 1 < knots.Count; i++)
            {
                double a = knots[i], b = knots[i + 1], mid = 0.5 * (a + b);
                bool rA = risers.Contains(Key(a)), rB = risers.Contains(Key(b));
                double ya = rA ? BwPath.FloorY(a + 1e-6) : BwPath.FloorY(a), yb = rB ? BwPath.FloorY(b - 1e-6) : BwPath.FloorY(b);
                double ba = BwPath.TreadLineY(a) - 0.5, bb = BwPath.TreadLineY(b) - 0.5;
                BwPath.Lane(a, out double loA, out double hiA);
                BwPath.Lane(b, out double loB, out double hiB);
                Beat05Layout.LaneEnvelope(a, out double eloA, out double ehiA);
                Beat05Layout.LaneEnvelope(b, out double eloB, out double ehiB);
                double ca = CeilingAt(a, +1), cb = CeilingAt(b, -1);
                bool well = a < wellS - 1e-9;
                double wlTopA = well ? BulwarkConst.AtriumHeight : ca, wlTopB = well ? BulwarkConst.AtriumHeight : cb;
                double wrTopA = well ? 0.0 : ca, wrTopB = well ? 0.0 : cb;
                double wlA = eloA - Beat05Layout.WallOffset, wlB = eloB - Beat05Layout.WallOffset, wrA = ehiA + Beat05Layout.WallOffset, wrB = ehiB + Beat05Layout.WallOffset;
                bool stairs = InStairs(a, b);
                int landing = stairs ? LandingAt(mid) : 0;

                // ---- floor: treads, landings, hall; the front 5 cm of every tread is the ochre nosing (built below)
                var fl = mid > wetS ? floorWet : floorDry;
                bool nose = stairs && risers.Contains(Key(b)) && b - a < Beat05Layout.NosingDepth + 1e-4;
                fl.Color = Ao(255);
                if (!nose) FloorQuad(fl, a, b, wlA, wrA, wlB, wrB, ya, yb);
                if (rA)
                {
                    double yHigh = BwPath.FloorY(a - 1e-6);
                    fl.Color = Ao(225);
                    CrossQuad(fl, a, wlA, wrA, ya, yHigh, true);
                }
                if (nose)
                {
                    // ochre steel nosing angle: 5 cm on the tread, 3.5 cm down the riser, 4 mm proud of both (no co-planar faces)
                    nosing.Color = Ao(255, 0, 0);
                    LaneBox(nosing, a, b + Beat05Layout.NosingProud, wlA + 0.005, wrA - 0.005, ya - Beat05Layout.NosingDrop, ya + Beat05Layout.NosingProud);
                }

                // ---- handrail grooves and the handrails (tread line + 1.0, 0.05 m round, 5 mm behind the wall plane)
                double ha = BwPath.TreadLineY(a) + BulwarkConst.StairHandrailHeight - hr, hb = BwPath.TreadLineY(b) + BulwarkConst.StairHandrailHeight - hr;
                double g0a = ha - gh, g0b = hb - gh, g1a = ha + gh, g1b = hb + gh;
                bool gL = stairs && g1a <= wlTopA && g1b <= wlTopB;
                bool gR = stairs && g1a <= wrTopA && g1b <= wrTopB;
                if (gL != grooveL) GrooveCap(walls, a, wlA, -gd, BwPath.TreadLineY(a) + BulwarkConst.StairHandrailHeight - hr);
                if (gR != grooveR) GrooveCap(walls, a, wrA, +gd, BwPath.TreadLineY(a) + BulwarkConst.StairHandrailHeight - hr);
                grooveL = gL; grooveR = gR;
                if (gL) InteriorQuads.Tube(hand, P(a, wlA - hr - 0.005, ha), P(b, wlB - hr - 0.005, hb), hr, 8, 2f);
                if (gR) InteriorQuads.Tube(hand, P(a, wrA + hr + 0.005, ha), P(b, wrB + hr + 0.005, hb), hr, 8, 2f);

                // wet streak weight in vertex B (BwConcrete drip boost), AO darker at the wall foot
                byte wet = (byte)Mathf.RoundToInt(255f * (float)Beat05Layout.Wetness(mid));
                walls.Color = Ao(240, 0, wet);

                // ---- west wall (lamp niche on each landing)
                bool lampNiche = landing > 0 && Math.Abs(mid - Beat05Layout.LandingCentre(landing)) < Beat05Layout.LampNicheHalf;
                if (lampNiche)
                {
                    double ly = BwPath.FloorY(mid);
                    double n0 = ly + Beat05Layout.LampNicheY0, n1 = ly + Beat05Layout.LampNicheY1;
                    Grooved(walls, a, b, wlA, wlB, ba, bb, g0a, g0b, g1a, g1b, -gd, true, gL);
                    SideQuad(walls, a, b, wlA, wlB, gL ? g1a : ba, gL ? g1b : bb, n0, n0, true);
                    SideQuad(walls, a, b, wlA, wlB, n1, n1, wlTopA, wlTopB, true);
                    double nb = wlA - Beat05Layout.LampNicheDepth;
                    niche.Color = Ao(190);
                    SideQuad(niche, a, b, nb, nb, n0, n0, n1, n1, true);
                    FloorQuad(niche, a, b, nb, wlA, nb, wlB, n0, n0);
                    CeilQuad(niche, a, b, nb, wlA, nb, wlB, n1, n1);
                    if (Math.Abs(a - (Beat05Layout.LandingCentre(landing) - Beat05Layout.LampNicheHalf)) < 1e-4) CrossQuad(niche, a, nb, wlA, n0, n1, true);
                    if (Math.Abs(b - (Beat05Layout.LandingCentre(landing) + Beat05Layout.LampNicheHalf)) < 1e-4) CrossQuad(niche, b, nb, wlB, n0, n1, false);
                }
                else Grooved(walls, a, b, wlA, wlB, ba, bb, g0a, g0b, g1a, g1b, -gd, true, gL, wlTopA, wlTopB);

                // ---- east wall (portholes on landings 5, 8, 11); in the open well it ends at the deck (y 0)
                int ph = landing > 0 ? PortholeIndex(landing) : -1;
                bool port = ph >= 0 && Math.Abs(mid - Beat05Layout.LandingCentre(landing)) < Beat05Layout.PortholeR;
                if (port)
                {
                    double ly = BwPath.FloorY(mid), pc = ly + Beat05Layout.PortholeEye, r = Beat05Layout.PortholeR;
                    Grooved(walls, a, b, wrA, wrB, ba, bb, g0a, g0b, g1a, g1b, +gd, false, gR);
                    SideQuad(walls, a, b, wrA, wrB, gR ? g1a : ba, gR ? g1b : bb, pc - r, pc - r, false);
                    SideQuad(walls, a, b, wrA, wrB, pc + r, pc + r, wrTopA, wrTopB, false);
                }
                else if (wrTopA > ba + 1e-4 || wrTopB > bb + 1e-4) Grooved(walls, a, b, wrA, wrB, ba, bb, g0a, g0b, g1a, g1b, +gd, false, gR, wrTopA, wrTopB);

                // ---- crack backings 0.12 m behind both walls (behind the 0.10 m handrail groove), same concrete: the niche and
                // porthole strips split the walls in T-junctions; a crack must show concrete, never the void (isolated bright
                // pixels in interiors-final). Holes: the lamp niche (west) and the porthole square (east).
                {
                    double bw = BackingGap;
                    if (lampNiche)
                    {
                        double ly = BwPath.FloorY(mid);
                        SideQuad(backing, a, b, wlA - bw, wlB - bw, ba, bb, ly + Beat05Layout.LampNicheY0, ly + Beat05Layout.LampNicheY0, true);
                        SideQuad(backing, a, b, wlA - bw, wlB - bw, ly + Beat05Layout.LampNicheY1, ly + Beat05Layout.LampNicheY1, wlTopA, wlTopB, true);
                    }
                    else SideQuad(backing, a, b, wlA - bw, wlB - bw, ba, bb, wlTopA, wlTopB, true);
                    if (port)
                    {
                        double ly = BwPath.FloorY(mid), pc = ly + Beat05Layout.PortholeEye, r = Beat05Layout.PortholeR;
                        SideQuad(backing, a, b, wrA + bw, wrB + bw, ba, bb, pc - r, pc - r, false);
                        SideQuad(backing, a, b, wrA + bw, wrB + bw, pc + r, pc + r, wrTopA, wrTopB, false);
                    }
                    else if (wrTopA > ba + 1e-4 || wrTopB > bb + 1e-4) SideQuad(backing, a, b, wrA + bw, wrB + bw, ba, bb, wrTopA, wrTopB, false);
                }

                // ---- soffit (stairs, A2) / hall ceiling with flush lamp openings; the open well has none (atrium ceiling)
                if (!well)
                {
                    soffit.Color = Ao(225);
                    bool hallLamp = false;
                    for (int h = 0; h < Beat05Layout.ExitHallLamps; h++) if (Math.Abs(mid - Beat05Layout.ExitHallLampS(h)) < 0.25) hallLamp = true;
                    if (hallLamp)
                    {
                        CeilQuad(soffit, a, b, wlA, -0.25, wlB, -0.25, ca, cb);
                        CeilQuad(soffit, a, b, 0.25, wrA, 0.25, wrB, ca, cb);
                        // backing over the split strip (above the lamp glass at +0.03): no crack shows the void
                        CeilQuad(backing, a, b, wlA, wrA, wlB, wrB, ca + 0.05, cb + 0.05);
                    }
                    else CeilQuad(soffit, a, b, wlA, wrA, wlB, wrB, ca, cb);
                }
                else
                {
                    // the vehicle deck beside the open well (y -0.5 .. 0): from the east wall's top edge to x 5.5
                    Vector3 da = P(a, wrA, 0.0), db = P(b, wrB, 0.0);
                    float dx = (float)BulwarkConst.TowerXMax;   // on under the tower's east wall
                    deck.Color = Ao(255);
                    InteriorQuads.Face(deck, da, db, new Vector3(dx, 0f, db.z), new Vector3(dx, 0f, da.z), Vector3.up);
                    InteriorQuads.Face(deck, new Vector3(da.x, -0.5f, da.z), new Vector3(dx, -0.5f, da.z), new Vector3(dx, -0.5f, db.z), new Vector3(db.x, -0.5f, db.z), Vector3.down);
                }
                if (i + 2 == knots.Count)
                {
                    if (grooveL) GrooveCap(walls, b, wlB, -gd, BwPath.TreadLineY(b) + BulwarkConst.StairHandrailHeight - hr);
                    if (grooveR) GrooveCap(walls, b, wrB, +gd, BwPath.TreadLineY(b) + BulwarkConst.StairHandrailHeight - hr);
                }
            }
            // the deck edge along the well (the east wall's top: x of the wall .. a 0.5 m face down to the slab underside)
            Flush(ctx, root, ref floorDry, "b5_treads", m_tread != null ? m_tread : m_floor);
            Flush(ctx, root, ref floorWet, "b5_treads_wet", m_floorWet);
            Flush(ctx, root, ref walls, "b5_walls", m_wall);
            Flush(ctx, root, ref soffit, "b5_soffit", m_ceiling);
            Flush(ctx, root, ref deck, "b5_vehicle_deck", m_floor);
            Flush(ctx, root, ref hand, "b5_handrails", m_handrail != null ? m_handrail : m_rail, false);
            Flush(ctx, root, ref nosing, "b5_nosings", m_nosing != null ? m_nosing : m_ochre, false);
            Flush(ctx, root, ref niche, "b5_lamp_niches", m_dark);
            Flush(ctx, root, ref backing, "b5_wall_backing", m_wall, false);
            BuildChevrons(ctx, root);
        }

        static long Key(double s) => (long)Math.Round(s * 1e4);
        /// <summary>Depth of the crack backings behind the stair and hall walls (m): behind the 0.10 m handrail groove.</summary>
        public const double BackingGap = 0.12;

        /// <summary>A side wall strip with the handrail groove (when <paramref name="groove"/>) from the wall foot up to the groove
        /// top; with top values it continues to them (else the caller closes the rest).</summary>
        static void Grooved(MeshBuilder mb, double a, double b, double la, double lb, double y0a, double y0b,
                            double g0a, double g0b, double g1a, double g1b, double depth, bool faceRight, bool groove,
                            double topA = double.NaN, double topB = double.NaN)
        {
            bool full = !double.IsNaN(topA);
            if (!groove)
            {
                if (full) SideQuad(mb, a, b, la, lb, y0a, y0b, topA, topB, faceRight);
                return;
            }
            SideQuad(mb, a, b, la, lb, y0a, y0b, g0a, g0b, faceRight);
            if (full) SideQuad(mb, a, b, la, lb, g1a, g1b, topA, topB, faceRight);
            SideQuad(mb, a, b, la + depth, lb + depth, g0a, g0b, g1a, g1b, faceRight);
            FloorQuad(mb, a, b, Math.Min(la, la + depth), Math.Max(la, la + depth), Math.Min(lb, lb + depth), Math.Max(lb, lb + depth), g0a, g0b);
            CeilQuad(mb, a, b, Math.Min(la, la + depth), Math.Max(la, la + depth), Math.Min(lb, lb + depth), Math.Max(lb, lb + depth), g1a, g1b);
        }

        static void GrooveCap(MeshBuilder mb, double s, double la, double depth, double h)
        {
            double l0 = Math.Min(la, la + depth), l1 = Math.Max(la, la + depth);
            CrossQuad(mb, s, l0, l1, h - Beat05Layout.GrooveHalf, h + Beat05Layout.GrooveHalf, true);
            CrossQuad(mb, s, l0, l1, h - Beat05Layout.GrooveHalf, h + Beat05Layout.GrooveHalf, false);
        }

        // ---------------------------------------------------------------- chevrons on landing k (k of them, ochre paint)
        void BuildChevrons(WorldContext ctx, Transform root)
        {
            var mb = new MeshBuilder();
            mb.Color = Ao(255, 0, 0);
            for (int k = 1; k <= Beat05Layout.Landings; k++)
            {
                Beat05Layout.Landing(k, out double s0, out double s1, out double y);
                int n = Beat05Layout.Chevrons(k);
                double span = 2.6, pitch = span / n, w = Math.Min(0.24, 0.8 * pitch), sc = 0.5 * (s0 + s1);
                double depth = 0.22, arm = 0.045, yt = y + 0.003;
                for (int c = 0; c < n; c++)
                {
                    double lc = -0.5 * span + pitch * (c + 0.5);
                    // a V pointing down the path: two strokes from (sc - depth/2, lc +- w/2) to the apex (sc + depth/2, lc)
                    Stroke(mb, sc - 0.5 * depth, lc - 0.5 * w, sc + 0.5 * depth, lc, arm, y, yt);
                    Stroke(mb, sc - 0.5 * depth, lc + 0.5 * w, sc + 0.5 * depth, lc, arm, y, yt);
                }
            }
            Flush(ctx, root, ref mb, "b5_chevrons", m_paint, false);
        }

        static void Stroke(MeshBuilder mb, double sa, double la, double sb, double lb, double width, double y0, double y1)
        {
            Vector3 a = P(sa, la, y1), b = P(sb, lb, y1);
            Vector3 d = b - a; d.y = 0f;
            Vector3 n = new Vector3(-d.z, 0f, d.x).normalized * (0.5f * (float)width);
            InteriorQuads.Face(mb, a - n, a + n, b + n, b - n, Vector3.up);
        }

        // ================================================================ lamps (pendants, landing cages, hall ceiling)
        void BuildLamps(WorldContext ctx, Transform root)
        {
            var tung = BulwarkPalette.Linear(Pal.Tungsten);
            // ---- six pendants 0.4 m at x -3 / +3, z 923 / 927 / 931, y 6.0: glass globe, steel cap, rod to the ceiling (pivot)
            var globe = new MeshBuilder();
            var cap = new MeshBuilder();
            float pr = 0.5f * (float)BulwarkConst.PendantSize, cy0 = (float)Beat05Layout.CeilingY0, py = (float)BulwarkConst.PendantY;
            MeshKit.Sphere(globe, new Vector3(0f, py - cy0, 0f), pr * 0.92f, 20, 10);
            cap.Color = Ao(255);
            MeshKit.TaperedCylinder(cap, new Vector3(0f, py - cy0 + 0.12f, 0f), new Vector3(0f, py - cy0 + 0.26f, 0f), pr * 0.75f, 0.05f, 16, true);
            var rod = new MeshBuilder();
            InteriorQuads.Tube(rod, new Vector3(0f, py - cy0 + 0.26f, 0f), new Vector3(0f, 0f, 0f), (float)Beat05Layout.PendantRodR, 6, 1f, false);
            InteriorQuads.CBox(cap, -0.1, -0.03, -0.1, 0.1, 0.0, 0.1, 0.005f);
            Mesh gm = globe.ToMesh("b5_pendant_globe"), cm = cap.ToMesh("b5_pendant_cap"), rm = rod.ToMesh("b5_pendant_rod");
            int idx = 0;
            foreach (double pz in BulwarkConst.PendantZ)
                for (int side = -1; side <= 1; side += 2)
                {
                    var pivot = new GameObject("b5_pendant_" + idx).transform;
                    pivot.SetParent(root, false);
                    var pp = new Vector3((float)(side * BulwarkConst.PendantX), cy0, (float)pz);
                    pivot.localPosition = pp;
                    ctx.AddMesh(pivot, "b5_lamp_pendant", gm, m_tungsten, false, false);
                    ctx.AddMesh(pivot, "b5_pendant_cap", cm, m_steel, true, true);
                    ctx.AddMesh(pivot, "b5_pendant_rod", rm, m_rod != null ? m_rod : m_steel, false, true);
                    m_tris += globe.TriangleCount + cap.TriangleCount;
                    m_pendants.Add(pivot);
                    m_pendantPivot.Add(pp);
                    m_pendantLights.Add(m_pool.Add(new Vector3(pp.x, py - 0.4f, pp.z), tung, Beat05Layout.PendantRange, Beat05Layout.PendantIntensity, "beat5", 1f, true));
                    idx++;
                }
            // ---- a caged lamp in the west-wall niche of every landing (glass 0.36 x 0.22, cage bars flush with the wall plane)
            var glass = new MeshBuilder();
            var bars = new MeshBuilder();
            var wires = new MeshBuilder();
            for (int k = 1; k <= Beat05Layout.Landings; k++)
            {
                double sc = Beat05Layout.LandingCentre(k), y = BwPath.FloorY(sc);
                Beat05Layout.LaneEnvelope(sc, out double lo, out _);
                double wl = lo - Beat05Layout.WallOffset, nb = wl - Beat05Layout.LampNicheDepth;
                double n0 = y + Beat05Layout.LampNicheY0, n1 = y + Beat05Layout.LampNicheY1;
                LaneBox(glass, sc - 0.18, sc + 0.18, nb + 0.01, nb + 0.09, n0 + 0.09, n1 - 0.09);
                bars.Color = Ao(255);
                for (int j = 0; j < 4; j++)
                {
                    double bs = sc - 0.15 + 0.1 * j;
                    InteriorQuads.Tube(wires, P(bs, wl - 0.019, n0 + 0.01), P(bs, wl - 0.019, n1 - 0.01), 0.006f, 6, 1f, false);
                }
                LaneBox(bars, sc - 0.24, sc + 0.24, nb, wl - 0.005, n0, n0 + 0.03);
                // a spot at the niche mouth aimed 45 deg down into the well (Wave X resume): the niche lintel shades the soffit
                // 0.25 m above it, so no light goes straight up. The former point light 0.6 m under the soffit made a hot spot
                // there that swept the frame top on the descent (seq_b04_b05: +3.1 levels per frame at s 940.8, landing 1).
                Vector3 lp = P(sc, wl + Beat05Layout.LandingSpotInset, 0.5 * (n0 + n1));
                Vector3 aim = P(sc, wl + Beat05Layout.LandingSpotInset + 1.0, 0.5 * (n0 + n1) - 1.0) - lp;
                int spot = m_pool.AddSpot(lp, Quaternion.LookRotation(aim.normalized, Vector3.up), Beat05Layout.LandingSpotAngle, tung, Beat05Layout.LandingLampRange, Beat05Layout.LandingLampIntensity, "beat5");
                if (k == DripLanding) { m_dripLamp = spot; m_dripLampPos = lp; }
            }
            // ---- exit hall: flush ceiling lamps every 8 m (0.5 x 0.5 openings in the soffit, a glass panel 2 cm above)
            for (int h = 0; h < Beat05Layout.ExitHallLamps; h++)
            {
                double s = Beat05Layout.ExitHallLampS(h), y = BwPath.FloorY(s) + BulwarkConst.ExitHallHeight;
                CeilQuad(glass, s - 0.25, s + 0.25, -0.25, 0.25, -0.25, 0.25, y + 0.03, y + 0.03);
                bars.Color = Ao(200);
                SideQuad(bars, s - 0.25, s + 0.25, -0.25, -0.25, y, y, y + 0.03, y + 0.03, true);
                SideQuad(bars, s - 0.25, s + 0.25, 0.25, 0.25, y, y, y + 0.03, y + 0.03, false);
                CrossQuad(bars, s - 0.25, -0.25, 0.25, y, y + 0.03, true);
                CrossQuad(bars, s + 0.25, -0.25, 0.25, y, y + 0.03, false);
                m_pool.Add(P(s, 0.0, y - 0.5), tung, Beat05Layout.HallLampRange, Beat05Layout.HallLampIntensity, "beat5", 1f, true);
            }
            Flush(ctx, root, ref glass, "b5_lamp_cages", m_tungsten, false);
            Flush(ctx, root, ref bars, "b5_lamp_fittings", m_steel);
            Flush(ctx, root, ref wires, "b5_lamp_wires", m_wire != null ? m_wire : m_steel, false);
        }

        // ================================================================ portholes (landings 5, 8, 11)
        void BuildPortholes(WorldContext ctx, Transform root)
        {
            var bore = new MeshBuilder();
            var rim = new MeshBuilder();
            var flange = new MeshBuilder();
            var cyan = BulwarkPalette.Linear(Pal.SeaTeal);
            for (int i = 0; i < Beat05Layout.PortholeLanding.Length; i++)
            {
                int k = Beat05Layout.PortholeLanding[i];
                double sc = Beat05Layout.LandingCentre(k), y = BwPath.FloorY(sc) + Beat05Layout.PortholeEye;
                Beat05Layout.LaneEnvelope(sc, out _, out double hi);
                double wr = hi + Beat05Layout.WallOffset;
                Vector3 c = P(sc, wr, y);
                BwPath.Right(sc, out double rx, out double rz);
                BwPath.Tangent(sc, out double tx, out double tz);
                var right = new Vector3((float)rx, 0f, (float)rz);
                var along = new Vector3((float)tx, 0f, (float)tz);
                float len = (float)((Beat05Layout.SeaFaceX(y) - c.x) / Math.Max(1e-3, rx));
                float r = (float)Beat05Layout.PortholeR;
                Vector3 end = c + right * (len - (float)Beat05Layout.GlassInset);
                // the square-to-round wall face round the bore mouth (the wall strip leaves a 0.6 x 0.6 hole)
                RingFace(rim, c, along, right, r);
                bore.Color = Ao(170);
                InteriorQuads.Bore(bore, c, end, r, 32);
                // a steel retaining flange round the glass (inside the bore, never past the wall plane)
                MeshKit.Torus(flange, end - right * 0.03f, right, r - 0.028f, 0.032f, 32, 8);   // outer edge r + 0.004: into the bore wall
                var g = new MeshBuilder();
                // the glass disc with its local UV (-1..1, +y up) for BwB5Porthole
                {
                    int c0 = g.Vertex(end, -right, Vector2.zero);
                    const int n = 40;
                    for (int j = 0; j <= n; j++)
                    {
                        float ang = 2f * Mathf.PI * j / n;
                        Vector2 uv = new Vector2(Mathf.Cos(ang), Mathf.Sin(ang));
                        g.Vertex(end + (along * uv.x + Vector3.up * uv.y) * r, -right, uv);
                    }
                    for (int j = 0; j < n; j++)
                    {
                        Vector3 pa = g.Positions[c0 + 1 + j], pb = g.Positions[c0 + 2 + j];
                        Vector3 nn = Vector3.Cross(pa - end, pb - end);
                        if (Vector3.Dot(nn, -right) >= 0f) g.Tri(c0, c0 + 1 + j, c0 + 2 + j); else g.Tri(c0, c0 + 2 + j, c0 + 1 + j);
                    }
                }
                Flush(ctx, root, ref g, "b5_lamp_porthole_" + k, m_port[i], false);
                m_pool.Add(c - right * 0.3f, cyan, 4f, 0.5f, "beat5_porthole", 1f, false);
            }
            Flush(ctx, root, ref bore, "b5_porthole_bores", m_dark);
            Flush(ctx, root, ref rim, "b5_porthole_rims", m_wall);
            Flush(ctx, root, ref flange, "b5_porthole_flanges", m_flange != null ? m_flange : m_dark);   // dry dark flange: steel paint / wet basalt glinted white
        }

        static void RingFace(MeshBuilder mb, Vector3 c, Vector3 along, Vector3 right, float r)
        {
            mb.Color = Ao(240);
            InteriorQuads.SquareRing(mb, c, along, Vector3.up, r, r - 0.012f, -right, 32);   // 12 mm lip over the bore mouth: no sliver where the two polygons differ
        }

        // ================================================================ dust motes in the lamp light (BwB1Haze mode 0)
        /// <summary>Motes under each atrium pendant and in front of each landing lamp: the air of the tower made visible (the
        /// beat-1 haze shader, mode 0: whole-period drift, 1.25 px floor with the energy kept, Henyey-Greenstein toward the lamp).</summary>
        public const int MotesPerPendant = 160, MotesPerLanding = 60;
        public const float MoteIntensity = 1.1f, MoteSize = 0.005f, DustWhiteMix = 0.45f;
        Material m_motes;

        static float Hash01(uint i)
        {
            i ^= i >> 16; i *= 0x7feb352du; i ^= i >> 15; i *= 0x846ca68bu; i ^= i >> 16;
            return (i & 0xFFFFFFu) / 16777216f;
        }

        void BuildMotes(WorldContext ctx, Transform root)
        {
            var sh = Shader.Find(Beat01WaveGallery.HazeShader);
            if (sh == null) { Debug.LogWarning("[BW] Beat05: shader " + Beat01WaveGallery.HazeShader + " not found (RebuildAll): no dust motes"); return; }
            m_motes = new Material(sh) { name = "b5_mist_motes" };
            m_motes.SetColor("_Color", Color.Lerp(BulwarkPalette.Linear(Pal.Tungsten), BulwarkPalette.Linear(Pal.FoamWhite), DustWhiteMix));
            m_motes.SetFloat("_Intensity", MoteIntensity); m_motes.SetFloat("_Mode", 0f); m_motes.SetFloat("_Size", MoteSize); m_motes.SetFloat("_FadeFar", 24f);
            RuntimeMaterials.Add(m_motes);
            var pos = new List<Vector3>(8000); var uv0 = new List<Vector4>(8000); var uv1 = new List<Vector4>(8000); var idx = new List<int>(12000);
            uint seq = 501u;
            void Add(Vector3 p, Vector3 lamp, float w)
            {
                float sd = Hash01(seq++);
                int first = pos.Count;
                for (int c = 0; c < 4; c++)
                {
                    pos.Add(p);
                    uv0.Add(new Vector4((c & 1) == 0 ? -1f : 1f, (c & 2) == 0 ? -1f : 1f, sd, w));
                    uv1.Add(new Vector4(lamp.x, lamp.y, lamp.z, 0f));
                }
                idx.Add(first); idx.Add(first + 2); idx.Add(first + 3); idx.Add(first); idx.Add(first + 3); idx.Add(first + 1);
            }
            // atrium: a 40 deg cone under each pendant, 0.4 .. 5.6 m below the globe, kept inside the tower
            float tanA = Mathf.Tan(40f * Mathf.Deg2Rad);
            foreach (double pz in BulwarkConst.PendantZ)
                for (int side = -1; side <= 1; side += 2)
                {
                    var lp = new Vector3((float)(side * BulwarkConst.PendantX), (float)BulwarkConst.PendantY - 0.25f, (float)pz);
                    int n = 0, guard = 0;
                    while (n < MotesPerPendant && guard++ < 20 * MotesPerPendant)
                    {
                        float u1 = Hash01(seq++), u2 = Hash01(seq++), u3 = Hash01(seq++);
                        float hgt = 0.4f + 5.2f * Mathf.Pow(u1, 1f / 3f);
                        float rmax = hgt * tanA, r = rmax * Mathf.Sqrt(u2), ang = 2f * Mathf.PI * u3;
                        var p = lp + new Vector3(r * Mathf.Cos(ang), -hgt, r * Mathf.Sin(ang));
                        if (p.x < (float)Beat05Layout.TowerInX0 + 0.2f || p.x > (float)Beat05Layout.TowerInX1 - 0.2f || p.y < 0.1f
                            || p.z < (float)Beat05Layout.TowerInZ0 + 0.2f || p.z > (float)BulwarkConst.AtriumZ1 - 0.4f) continue;
                        Add(p, lp, Mathf.Sqrt(Mathf.Max(0f, 1f - r / Mathf.Max(rmax, 1e-3f))) / (1f + (hgt / 2.5f) * (hgt / 2.5f)));
                        n++;
                    }
                }
            // landings: a lobe in front of each wall lamp (1.6 m into the stairwell, 2 m down, 1.4 m along)
            for (int k = 1; k <= Beat05Layout.Landings; k++)
            {
                double sc = Beat05Layout.LandingCentre(k), y = BwPath.FloorY(sc);
                Beat05Layout.LaneEnvelope(sc, out double lo, out double hi);
                Vector3 lamp = P(sc, lo - Beat05Layout.WallOffset, y + 0.5 * (Beat05Layout.LampNicheY0 + Beat05Layout.LampNicheY1));
                for (int j = 0; j < MotesPerLanding; j++)
                {
                    float a = Hash01(seq++), b = Hash01(seq++), c = Hash01(seq++);
                    double lat = lo - Beat05Layout.WallOffset + 0.15 + 1.9 * Mathf.Pow(a, 0.7f);
                    double ss = sc + (b - 0.5) * 1.6, yy = y + 0.3 + 2.0 * c;
                    if (lat > hi + Beat05Layout.WallOffset - 0.15) continue;
                    var p = P(ss, lat, yy);
                    float d = (p - lamp).magnitude;
                    Add(p, lamp, 1f / (1f + (d / 0.9f) * (d / 0.9f)));
                }
            }
            var mesh = new Mesh { name = "b5_mist_motes" };
            mesh.indexFormat = pos.Count > 65000 ? UnityEngine.Rendering.IndexFormat.UInt32 : UnityEngine.Rendering.IndexFormat.UInt16;
            mesh.SetVertices(pos);
            mesh.SetUVs(0, uv0);
            mesh.SetUVs(1, uv1);
            mesh.SetTriangles(idx, 0, false);
            mesh.bounds = new Bounds(new Vector3(1f, -24f, 990f), new Vector3(16f, 60f, 150f));   // atrium to the last landing
            var go = new GameObject("b5_mist_motes");
            go.transform.SetParent(root, false);
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            var mr = go.AddComponent<MeshRenderer>();
            mr.sharedMaterial = m_motes;
            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            mr.receiveShadows = false;
            mr.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
            mr.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
            m_tris += idx.Count / 3;
        }

        // ================================================================ the drip at landing 9 (14.5.5 "Moving")
        /// <summary>A drop every DripPeriod s from the soffit over landing 9 (1.55 m east of the axis, clear of the chevrons) into a wet patch: BwB1Haze
        /// mode 2 with one (env 1, age = t mod period) slot; the drop fades in over 0.15 s and out at the floor, so the wrap of
        /// the age never shows (0.4 Hz, below the 1 Hz limit of 14.2.5).</summary>
        public const int DripLanding = 9;
        public const double DripPeriod = 2.5, DripLateral = 1.55;   // between the chevrons (to lateral 1.28) and the east wall (1.80)
        Material m_drip, m_stain;
        static readonly int IdB0 = Shader.PropertyToID("_B0");
        float m_dripAge = -1f;
        int m_dripLamp = -1;
        Vector3 m_dripLampPos;
        float m_stainLamp = -1f;
        /// <summary>Wet stain under the drip (Wave X2 fix S4-1: the opaque ConcreteWet 16-gon read as a flat faceted decal):
        /// a BwB1Haze mode-5 quad 2 mm above the landing, half sizes across / along the path (m); the soft fBm rim fades
        /// out by about 0.74 + 0.15 of noise of the lateral half (visible stain about 0.36 x 0.55 m), clear of the chevrons and
        /// inside the east wall.</summary>
        public const double StainHalfLat = 0.24, StainHalfS = 0.40;
        const float StainLocalS = 1.25f;

        void BuildDrip(WorldContext ctx, Transform root)
        {
            double sc = Beat05Layout.LandingCentre(DripLanding), y = BwPath.FloorY(sc), top = BwPath.TreadLineY(sc) + BulwarkConst.StairHeadroom - 0.02;
            var sh = Shader.Find(Beat01WaveGallery.HazeShader);
            if (sh == null) return;
            // the wet stain: premultiplied darkening with a soft irregular rim and the landing lamp's sheen (BwB1Haze mode 5),
            // 2 mm above the landing (below the 3 mm chevron paint); drawn before the other transparents (the drip mist)
            m_stain = new Material(sh) { name = "b5_drip_stain" };
            m_stain.SetColor("_Color", BulwarkPalette.Linear(Pal.Tungsten));
            m_stain.SetFloat("_Intensity", 1f); m_stain.SetFloat("_Mode", 5f);
            m_stain.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.One);
            m_stain.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            m_stain.SetVector(IdB0, new Vector4(m_dripLampPos.x, m_dripLampPos.y, m_dripLampPos.z, 0f));
            m_stain.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent - 10;
            RuntimeMaterials.Add(m_stain);
            {
                var smesh = new Mesh { name = "b5_drip_stain" };
                var sv = new List<Vector3>(4); var su = new List<Vector4>(4); var sn = new List<Vector3>(4);
                for (int k = 0; k < 4; k++)
                {
                    float lx = (k & 1) == 0 ? -1f : 1f, ly = (k & 2) == 0 ? -1f : 1f;
                    sv.Add(P(sc + ly * StainHalfS, DripLateral + lx * StainHalfLat, y + 0.002));
                    su.Add(new Vector4(lx, ly * StainLocalS, 0.37f, StainLocalS));   // lateral half = 1 local unit; along s 1.25 (an ellipse 1.33:1 along the path)
                    sn.Add(Vector3.up);
                }
                smesh.SetVertices(sv); smesh.SetNormals(sn); smesh.SetUVs(0, su);
                smesh.SetTriangles(new List<int> { 0, 2, 3, 0, 3, 1 }, 0, false);
                smesh.RecalculateBounds();
                var sgo = new GameObject("b5_drip_stain");
                sgo.transform.SetParent(root, false);
                sgo.AddComponent<MeshFilter>().sharedMesh = smesh;
                var smr = sgo.AddComponent<MeshRenderer>();
                smr.sharedMaterial = m_stain;
                smr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                smr.receiveShadows = false;
                smr.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
                smr.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
                m_tris += 2;
            }
            Vector3 c = P(sc, DripLateral, y + 0.002);
            m_drip = new Material(sh) { name = "b5_mist_drip" };
            m_drip.SetColor("_Color", Color.Lerp(BulwarkPalette.Linear(Pal.Tungsten), BulwarkPalette.Linear(Pal.FoamWhite), 0.6f));
            m_drip.SetFloat("_Intensity", 1.6f); m_drip.SetFloat("_Mode", 2f); m_drip.SetFloat("_FadeFar", 30f); m_drip.SetFloat("_FloorY", (float)y);
            RuntimeMaterials.Add(m_drip);
            float fall = (float)((top - y) / 0.75);   // about the free-fall time of 2.6 m (0.73 s)
            Vector3 p = P(sc, DripLateral, top);
            var mesh = new Mesh { name = "b5_mist_drip" };
            var verts = new List<Vector3> { p, p, p, p };
            var uv0 = new List<Vector4>(); var uv1 = new List<Vector4>();
            for (int k = 0; k < 4; k++) { uv0.Add(new Vector4((k & 1) == 0 ? -1f : 1f, (k & 2) == 0 ? -1f : 1f, 0.5f, 0f)); uv1.Add(new Vector4(fall, 0f, 0f, 0.0045f)); }
            mesh.SetVertices(verts); mesh.SetUVs(0, uv0); mesh.SetUVs(1, uv1);
            mesh.SetTriangles(new List<int> { 0, 2, 3, 0, 3, 1 }, 0, false);
            mesh.bounds = new Bounds(c + Vector3.up * 1.3f, new Vector3(2f, 4f, 2f));
            var go = new GameObject("b5_mist_drip");
            go.transform.SetParent(root, false);
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            var mr = go.AddComponent<MeshRenderer>();
            mr.sharedMaterial = m_drip;
            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            mr.receiveShadows = false;
            mr.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
            mr.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
        }

        // ================================================================ per frame: pendant sway on wave impacts (+-0.8 deg)
        public void OnFrame(in FrameState f)
        {
            for (int i = 0; i < 3; i++)
            {
                float sh = Mathf.Clamp01(PortholeShade[i]);
                if (m_port[i] == null || Mathf.Abs(sh - m_shade[i]) < 1e-4f) continue;
                m_shade[i] = sh;
                if (m_port[i].HasProperty(IdShade)) m_port[i].SetFloat(IdShade, sh);
            }
            if (m_drip != null && f.s > 960.0 && f.s < 1040.0)
            {
                float age = (float)(f.ambientTime - DripPeriod * Math.Floor(f.ambientTime / DripPeriod));
                if (age != m_dripAge) { m_dripAge = age; m_drip.SetVector(IdB0, new Vector4(1f, age, 0f, 0f)); }
            }
            if (m_stain != null && f.s > 960.0 && f.s < 1040.0)
            {
                // the sheen follows the landing lamp's real light: its intensity x the pool's slot weight (0 when not picked)
                float w = 0f;
                if (m_pool != null && m_dripLamp >= 0)
                    for (int i = 0; i < LocalLightPool.MaxLights; i++)
                        if (m_pool.SlotAnchor(i) == m_dripLamp) w = Mathf.Max(w, m_pool.SlotWeight(i));
                float li = Beat05Layout.LandingLampIntensity * w;
                if (Mathf.Abs(li - m_stainLamp) > 1e-3f)
                {
                    m_stainLamp = li;
                    m_stain.SetVector(IdB0, new Vector4(m_dripLampPos.x, m_dripLampPos.y, m_dripLampPos.z, li));
                }
            }
            if (f.s < 880.0 || f.s > 960.0) return;
            int n = WaveImpacts.Count;
            double t = f.ambientTime;
            for (int i = 0; i < m_pendants.Count; i++)
            {
                double a = 0.0, pz = m_pendantPivot[i].z;
                for (int j = 0; j < n; j++)
                {
                    var w = WaveImpacts.Get(j);
                    double dz = w.Z - pz;
                    a += Beat01Layout.SwayDeg(t - w.T0, 0.8 * Math.Min(1.0, w.Strength / 0.75) * Math.Exp(-(dz * dz) / (2.0 * 80.0 * 80.0)));
                }
                float af = (float)a;
                if (Mathf.Abs(af - m_pendantAngle[i]) < 1e-4f) continue;
                m_pendantAngle[i] = af;
                m_pendants[i].localRotation = Quaternion.AngleAxis(af, Vector3.forward);
                float arm = (float)(Beat05Layout.CeilingY0 - BulwarkConst.PendantY + 0.4);
                Vector3 pp = m_pendantPivot[i];
                m_pool.SetPosition(m_pendantLights[i], new Vector3(pp.x + arm * Mathf.Sin(af * Mathf.Deg2Rad), pp.y - arm * Mathf.Cos(af * Mathf.Deg2Rad), pp.z));
            }
        }
    }
}
