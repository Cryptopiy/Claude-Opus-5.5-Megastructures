// Beat01WaveGallery: beat 1, the wave gallery (MASTER_PROMPT 14.5.1: KS01-KS03; Addendum A1 K0). Replaces the graybox of
// beat 1 ([ReplacesGraybox("beat1")]): the gallery shell with ribs and recessed panels, the ten wave slits in cast pilasters
// with their channels, baffle glow and exterior hoods, the five west-wall niches (valve wheels at z 30 and 78, cabinets),
// pipes, cable tray, the ochre band, the floor slab with the drain gutter (the 14.4 "road deck cut" under the shell), the
// nine caged SodiumWarm lamps (LocalLightPool anchors, 9 m, 1.6; sway on wave impacts and at the K0 moment), the heavy
// door (two 2.0 x 3.5 x 0.7 leaves in pocket housings, ochre bands, hand wheel) and its sky opening. Geometry rules and
// numbers: Beat01Layout (pure, tested by Beat01Tests).
// Light: lamps through LocalLightPool (at most 4 real lights); the slit glow is BwEmissive (StormHorizon, +-25 % with the
// impact clock). Fog, exposure and ambient come from the AtmosphereDriver (one writer). Per frame only transforms of the
// lamp pivots and the door leaves, the pool positions, the opening rectangle and the slit gains change (no allocation).
using System;
using System.Collections.Generic;
using Megastructure.Rendering;
using UnityEngine;

namespace Megastructure.Beats
{
    [ReplacesGraybox("beat1")]
    public sealed class Beat01WaveGallery : IBeatBuilder, IWorldUpdater
    {
        public string Name => "Beat01_WaveGallery";

        Material m_wall, m_floor, m_dark, m_steel, m_ochre, m_paint, m_grating, m_sodium, m_slitBase, m_wire;
        readonly Material[] m_slit = new Material[BulwarkConst.SlitCount];
        readonly float[] m_slitGain = new float[BulwarkConst.SlitCount];
        /// <summary>Materials created at runtime by this builder (the EditMode audit destroys them).</summary>
        public readonly List<Material> RuntimeMaterials = new List<Material>();
        public Transform Root { get; private set; }
        int m_tris;

        // ---- moving parts
        readonly Transform[] m_lampPivot = new Transform[Beat01Layout.LampTotal];
        readonly int[] m_lampLight = new int[Beat01Layout.LampTotal];
        readonly float[] m_lampAngle = new float[Beat01Layout.LampTotal];
        Transform m_leafW, m_leafE;
        Vector3 m_leafW0, m_leafE0;
        LocalLightPool m_pool;
        int m_opening = -1;
        float m_gap = -1f;
        double m_trigger = double.NaN, m_k0 = double.NaN;
        public const float SlitIntensity = 1.5f;   // x StormHorizon (linear): the storm light off the baffle
        /// <summary>Real light of each slit (storm light through the baffle onto the floor and the opposite wall): StormHorizon,
        /// 3.5 m, 0.35, rank 1.4 (the pool gives the lamps their lights first).</summary>
        public const float SlitSpillRange = 3.5f, SlitSpillIntensity = 0.35f, SlitSpillRank = 1.4f;
        /// <summary>Wave X2 fix (sea S3-3): each impact bursts across its slits: a whitewater sheet on the baffle face (BwB1Haze
        /// mode 3), spray clumps thrown through the channel (mode 4) and the slit's spill light flared by up to x(1 +
        /// SlitBurstSpill) with the same 0.6 s in / 2.4 s out envelope as the breath (no flash above 1 Hz). The baffle glow
        /// keeps the 14.5.1 +-25 %.</summary>
        public const float SlitBurstSpill = 1.2f, SlitSheetIntensity = 1.1f, SlitSprayIntensity = 0.9f;
        public const int SprayPerSlit = 36;
        readonly int[] m_slitLight = new int[BulwarkConst.SlitCount];
        readonly float[] m_slitSpill = new float[BulwarkConst.SlitCount];
        Material m_sheet, m_spray;
        static readonly int IdIntensity = Shader.PropertyToID("_Intensity");

        /// <summary>Current door gap (m) (QA / tests).</summary>
        public float DoorGap => Mathf.Max(0f, m_gap);

        // ================================================================ build
        public void Build(WorldContext ctx)
        {
            var root = new GameObject(Name).transform;
            root.SetParent(ctx.Root, false);
            Root = root;
            CreateMaterials(ctx);
            m_pool = LocalLightPool.Ensure(ctx);

            BuildFloor(ctx, root);
            BuildShell(ctx, root);
            BuildSlits(ctx, root);
            BuildNiches(ctx, root);
            BuildFittings(ctx, root);
            BuildLamps(ctx, root);
            BuildDoor(ctx, root);
            BuildHaze(ctx, root);

            ctx.Register(this);
            ctx.Log("Beat01_WaveGallery: ribs " + Beat01Layout.RibCount + ", slits " + BulwarkConst.SlitCount + ", niches " + Beat01Layout.NicheCount
                    + ", lamps " + BulwarkConst.GalleryLampCount + " + door-bay lamp, door (" + m_tris + " triangles)");
        }

        // ---------------------------------------------------------------- materials
        void CreateMaterials(WorldContext ctx)
        {
            var lib = ctx.Materials;
            // gallery concrete: no rain follow (inside the crest); drips below the formwork joints, a damp floor that mirrors the
            // sodium lamps (14.5.1 "inside the shed shell")
            m_wall = Copy(lib != null ? lib.Concrete : null, "b1_wall");
            Set(m_wall, "_WetFollow", 0f); Set(m_wall, "_DripStrength", 0.75f); Set(m_wall, "_Wet", 0.05f);
            m_floor = Copy(lib != null ? lib.Concrete : null, "b1_floor");
            Set(m_floor, "_WetFollow", 0f); Set(m_floor, "_Wet", 0.32f);
            m_dark = Copy(lib != null ? lib.ConcreteDark : null, "b1_dark");
            Set(m_dark, "_WetFollow", 0f); Set(m_dark, "_Wet", 0.25f);
            m_steel = lib != null ? lib.Steel : null;
            m_ochre = lib != null ? lib.SteelOchre : null;
            // ochre paint on concrete (the band at y 1.1): the gallery concrete recoloured to the SafetyOchre token
            m_paint = Copy(lib != null ? lib.Concrete : null, "b1_paint_ochre");
            // (65 % SafetyOchre over ConcreteMid: a weathered paint, full ochre read as a red stripe under the sodium light)
            if (m_paint != null && m_paint.HasProperty("_BaseColor")) m_paint.SetColor("_BaseColor", Color.Lerp(BulwarkPalette.Linear(Pal.ConcreteMid), BulwarkPalette.Linear(Pal.SafetyOchre), 0.65f));
            Set(m_paint, "_WetFollow", 0f); Set(m_paint, "_DripStrength", 0.3f); Set(m_paint, "_MacroTone", 0.04f);
            m_grating = Copy(lib != null ? lib.Grating : null, "b1_drain_grating");
            Set(m_grating, "_BarAxis", 1f);   // bars across the gutter
            m_sodium = lib != null ? lib.LampSodium : null;
            // lamp cage wires and hanger rods: BwLine tubes 12 mm (thin lines through BwLine, H7: never thinner than 1.5 px,
            // no isolated dark pixels where the cage crosses the bright glass; interiors-final image_metrics NaN-like)
            m_wire = Copy(lib != null ? lib.Rail : null, "b1_cage_wire");
            if (m_wire != null)
            {
                m_wire.SetFloat("_LineWidth", WireWidth);
                var sp = BulwarkPalette.Linear(Pal.SteelPaint);
                m_wire.SetColor("_BaseColor", new Color(sp.r * 0.6f, sp.g * 0.6f, sp.b * 0.6f, 1f));
            }
            // slit glow: BwEmissive (the LampSodium shader) recoloured to the storm light, one copy per slit (own flare)
            m_slitBase = lib != null ? lib.LampSodium : null;
            for (int k = 0; k < m_slit.Length; k++)
            {
                var m = Copy(m_slitBase, "b1_slit_" + k);
                if (m != null)
                {
                    if (m.HasProperty("_EmissionColor")) m.SetColor("_EmissionColor", BulwarkPalette.Linear(Pal.StormHorizon));
                    Set(m, "_Intensity", SlitIntensity); Set(m, "_GainFollow", 0f); Set(m, "_GlassFalloff", 0f); Set(m, "_Smoothness", 0.1f);
                }
                m_slit[k] = m;
                m_slitGain[k] = 1f;
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

        // ---------------------------------------------------------------- floor slab and drain (the deck cut x -4.1..1.1)
        void BuildFloor(WorldContext ctx, Transform root)
        {
            var mb = new MeshBuilder();
            double x0 = Beat01Layout.ShellX0, x1 = Beat01Layout.ShellX1, z0 = BulwarkConst.ShellZMin, z1 = BulwarkConst.ShellZMax;
            double g0 = Beat01Layout.DrainX0, g1 = Beat01Layout.DrainX1, gz0 = Beat01Layout.SouthWallZ, gz1 = Beat01Layout.HousingZ0;
            // top (y 0) with the gutter opening x 0.2..0.48, z -0.4..119.4
            InteriorQuads.RectY(mb, 0.0, x0, g0, z0, z1, +1);
            InteriorQuads.RectY(mb, 0.0, g1, x1, z0, z1, +1);
            InteriorQuads.RectY(mb, 0.0, g0, g1, z0, gz0, +1);
            InteriorQuads.RectY(mb, 0.0, g0, g1, gz1, z1, +1);
            // slab edge faces: south end and the north end beyond the door housings (the sides meet the road deck)
            InteriorQuads.RectZ(mb, z0, x0, x1, Beat01Layout.FloorBottom, 0.0, -1);
            InteriorQuads.RectZ(mb, z1, x0, x1, Beat01Layout.FloorBottom, 0.0, +1);
            Flush(ctx, root, ref mb, "b1_floor", m_floor);
            // the gutter: dark wet channel 0.14 deep under a flush grating (1 cm below the floor: never co-planar)
            var ch = new MeshBuilder();
            double gy = -Beat01Layout.DrainDepth;
            ch.Color = Ao(150);
            InteriorQuads.RectY(ch, gy, g0, g1, gz0, gz1, +1);
            InteriorQuads.RectX(ch, g0, gz0, gz1, gy, 0.0, +1);
            InteriorQuads.RectX(ch, g1, gz0, gz1, gy, 0.0, -1);
            InteriorQuads.RectZ(ch, gz0, g0, g1, gy, 0.0, +1);
            InteriorQuads.RectZ(ch, gz1, g0, g1, gy, 0.0, -1);
            Flush(ctx, root, ref ch, "b1_drain_channel", m_dark);
            var gr = new MeshBuilder();
            InteriorQuads.RectY(gr, -0.012, g0, g1, gz0, gz1, +1);
            Flush(ctx, root, ref gr, "b1_drain_grating", m_grating, false);
        }

        // ---------------------------------------------------------------- shell: ribs, panels, ceiling, end walls, outer faces
        static readonly List<Vector3> s_prof = new List<Vector3>(32), s_profN = new List<Vector3>(32);

        /// <summary>Profile points (x, y) offset by d with inward normals into the shared lists.</summary>
        static void ProfilePoints(double d, int from, int to)
        {
            s_prof.Clear(); s_profN.Clear();
            for (int i = from; i <= to; i++)
            {
                Beat01Layout.Profile(i, d, out double x, out double y, out double nx, out double ny);
                s_prof.Add(new Vector3((float)x, (float)y, 0f));
                s_profN.Add(new Vector3((float)nx, (float)ny, 0f));
            }
        }

        /// <summary>Lofts the current profile list along z [z0, z1] with smooth profile normals (arcs) facing inward.</summary>
        static void LoftZ(MeshBuilder mb, double z0, double z1)
        {
            if (z1 - z0 < 1e-5) return;
            for (int i = 0; i + 1 < s_prof.Count; i++)
            {
                Vector3 p = s_prof[i], q = s_prof[i + 1];
                Vector3 np = s_profN[i], nq = s_profN[i + 1];
                // hard edges at the wall-arc junctions (normals differ by more than 30 deg): flat per segment there
                Vector3 nf = (np + nq).normalized;
                bool smooth = Vector3.Dot(np, nq) > 0.85f;
                Vector3 n0 = smooth ? np : nf, n1 = smooth ? nq : nf;
                int a = mb.Vertex(new Vector3(p.x, p.y, (float)z0), n0, new Vector2((float)z0, p.y));
                int b = mb.Vertex(new Vector3(q.x, q.y, (float)z0), n1, new Vector2((float)z0, q.y));
                int c = mb.Vertex(new Vector3(q.x, q.y, (float)z1), n1, new Vector2((float)z1, q.y));
                int d = mb.Vertex(new Vector3(p.x, p.y, (float)z1), n0, new Vector2((float)z1, p.y));
                // winding: west wall going up gives +x (inward) with a, b, c, d (Beat01Layout.Profile order)
                Vector3 geo = Vector3.Cross(mb.Positions[b] - mb.Positions[a], mb.Positions[c] - mb.Positions[a]);
                if (Vector3.Dot(geo, nf) >= 0f) mb.Quad(a, b, c, d); else mb.Quad(a, d, c, b);
            }
        }

        /// <summary>Upper boundary y of the profile offset d at x (arcs and soffit), for end faces.</summary>
        static double ProfileTop(double x, double d)
        {
            double cW = Beat01Layout.X0 + Beat01Layout.R, cE = Beat01Layout.X1 - Beat01Layout.R, cy = Beat01Layout.H - Beat01Layout.R, rr = Beat01Layout.R + d;
            if (x < cW) { double u = x - cW; return cy + Math.Sqrt(Math.Max(0.0, rr * rr - u * u)); }
            if (x > cE) { double u = x - cE; return cy + Math.Sqrt(Math.Max(0.0, rr * rr - u * u)); }
            return Beat01Layout.H + d;
        }

        /// <summary>The face at z between the profile offset d (top) and lo(x) (bottom), over the x samples of the profile and
        /// the extra break points, facing +z or -z.</summary>
        static void EndFace(MeshBuilder mb, double z, double d, Func<double, double> lo, double[] breaks, int sign)
        {
            var xs = new List<double>(48);
            for (int i = 1; i <= 2 * Beat01Layout.ArcSegments + 2; i++) { Beat01Layout.Profile(i, d, out double x, out _, out _, out _); xs.Add(x); }
            xs.Add(Beat01Layout.X0 - d); xs.Add(Beat01Layout.X1 + d);
            if (breaks != null) xs.AddRange(breaks);
            xs.Sort();
            var n = new Vector3(0, 0, sign);
            for (int i = 0; i + 1 < xs.Count; i++)
            {
                double a = xs[i], b = xs[i + 1];
                if (b - a < 1e-5) continue;
                double m = 0.5 * (a + b);
                double ya0 = lo(m), ya1 = ProfileTop(a, d), yb1 = ProfileTop(b, d);
                if (ya1 - ya0 < 1e-5 && yb1 - ya0 < 1e-5) continue;
                InteriorQuads.Face(mb, new Vector3((float)a, (float)ya0, (float)z), new Vector3((float)a, (float)ya1, (float)z),
                                   new Vector3((float)b, (float)yb1, (float)z), new Vector3((float)b, (float)ya0, (float)z), n);
            }
        }

        void BuildShell(WorldContext ctx, Transform root)
        {
            var ribs = new MeshBuilder();
            var pan = new MeshBuilder();
            int A = Beat01Layout.ArcSegments, last = Beat01Layout.ProfileCount - 1;
            double rh = Beat01Layout.RibHalf, bv = Beat01Layout.Bevel, rec = Beat01Layout.Recess;
            // ---- ribs (z 3 + 6 k): inner face at the clear profile, 2 cm chamfers, side faces down to the panels
            ribs.Color = Ao(255, 0, 0);
            for (int k = 0; k < Beat01Layout.RibCount; k++)
            {
                double zc = Beat01Layout.RibZ(k);
                ProfilePoints(0.0, 0, last);
                LoftZ(ribs, zc - rh + bv, zc + rh - bv);
                for (int side = -1; side <= 1; side += 2)
                {
                    double zIn = zc + side * (rh - bv), zOut = zc + side * rh;
                    // chamfer strip (worn edge) and the side face, per profile segment
                    for (int i = 0; i < last; i++)
                    {
                        Beat01Layout.Profile(i, 0.0, out double ax, out double ay, out double anx, out double any);
                        Beat01Layout.Profile(i + 1, 0.0, out double bx, out double by, out _, out _);
                        Beat01Layout.Profile(i, bv, out double cx, out double cy, out _, out _);
                        Beat01Layout.Profile(i + 1, bv, out double dx, out double dy, out _, out _);
                        Beat01Layout.Profile(i, rec, out double ex, out double ey, out _, out _);
                        Beat01Layout.Profile(i + 1, rec, out double fx, out double fy, out _, out _);
                        var facing = new Vector3((float)anx, (float)any, side);
                        var wear = ribs.Color; ribs.Color = Ao(255, 200, 0);
                        InteriorQuads.Face(ribs, V(ax, ay, zIn), V(bx, by, zIn), V(dx, dy, zOut), V(cx, cy, zOut), facing);
                        ribs.Color = Ao(235, 0, 0);
                        InteriorQuads.Face(ribs, V(cx, cy, zOut), V(dx, dy, zOut), V(fx, fy, zOut), V(ex, ey, zOut), new Vector3(0, 0, side));
                        ribs.Color = wear;
                    }
                }
            }
            Flush(ctx, root, ref ribs, "b1_ribs", m_wall);

            // ---- panels between the ribs: arcs + soffit lofted, west wall with niche holes, east wall around the pilasters
            pan.Color = Ao(225, 0, 0);
            double zS = Beat01Layout.SouthWallZ, zN = Beat01Layout.HousingZ0;
            var bays = new List<(double a, double b)>();
            double za = zS;
            for (int k = 0; k < Beat01Layout.RibCount; k++)
            {
                double zc = Beat01Layout.RibZ(k);
                bays.Add((za, zc - rh));
                za = zc + rh;
            }
            bays.Add((za, zN));
            double wallTop = Beat01Layout.H - Beat01Layout.R;   // 3.3: the arcs start here
            foreach (var (a, b) in bays)
            {
                if (b - a < 1e-4) continue;
                ProfilePoints(rec, 1, 2 * A + 2);
                LoftZ(pan, a, b);
                // west wall x -3.65 with niche holes (1.0 x 2.1)
                var holesW = new List<(double z0, double z1, double y0, double y1)>();
                for (int n = 0; n < Beat01Layout.NicheCount; n++)
                {
                    double nz = Beat01Layout.NicheZ(n);
                    if (nz > a && nz < b) holesW.Add((nz - Beat01Layout.NicheHalf, nz + Beat01Layout.NicheHalf, 0.0, Beat01Layout.NicheTop));
                }
                WallWithHoles(pan, Beat01Layout.PanelX0, a, b, 0.0, wallTop, holesW, +1);
                // east wall x 0.65 except the slit pilasters (built in BuildSlits)
                var cut = new List<(double z0, double z1, double y0, double y1)>();
                for (int s = 0; s < BulwarkConst.SlitCount; s++)
                {
                    double sz = Beat01Layout.SlitZ(s);
                    if (sz > a && sz < b) cut.Add((sz - Beat01Layout.PilasterHalf, sz + Beat01Layout.PilasterHalf, 0.0, wallTop));
                }
                WallWithHoles(pan, Beat01Layout.PanelX1, a, b, 0.0, wallTop, cut, -1);
            }
            // ---- backings 3 cm behind both panel walls (same concrete): the wall pieces round the niches and pilasters meet in
            // T-junctions, and a rasterised crack there would show the void behind the shell (an isolated bright pixel,
            // interiors-final image_metrics); the backing has the niche and slit-channel holes only
            var holesAllW = new List<(double z0, double z1, double y0, double y1)>();
            for (int n = 0; n < Beat01Layout.NicheCount; n++)
            {
                double nz = Beat01Layout.NicheZ(n);
                holesAllW.Add((nz - Beat01Layout.NicheHalf, nz + Beat01Layout.NicheHalf, 0.0, Beat01Layout.NicheTop));
            }
            WallWithHoles(pan, Beat01Layout.PanelX0 - BackingGap, zS, zN, 0.0, wallTop, holesAllW, +1);
            var holesAllE = new List<(double z0, double z1, double y0, double y1)>();
            for (int k = 0; k < BulwarkConst.SlitCount; k++)
            {
                double sz = Beat01Layout.SlitZ(k);
                holesAllE.Add((sz - Beat01Layout.SlitHalf, sz + Beat01Layout.SlitHalf, BulwarkConst.SlitY0, BulwarkConst.SlitY1));
            }
            WallWithHoles(pan, Beat01Layout.PanelX1 + BackingGap, zS, zN, 0.0, wallTop, holesAllE, -1);
            // ---- south end wall (z -0.4, facing north) with a closed steel service door in it (built in BuildFittings)
            EndFace(pan, zS, rec, _ => 0.0, null, +1);
            // (the door end at z 119.4 is closed by the pocket housings and the lintel of BuildDoor: their south faces cover
            // the panel profile round the 4.0 x 3.4 opening, so no frame face is drawn here: no co-planar faces)
            Flush(ctx, root, ref pan, "b1_panels", m_wall);

            // ---- outer faces of the shed shell (seen from the crest looking back): west, east, roof, south
            var outer = new MeshBuilder();
            outer.Color = Ao(255, 0, 0);
            double sx0 = Beat01Layout.ShellX0, sx1 = Beat01Layout.ShellX1, top = Beat01Layout.ShellTop, z0 = BulwarkConst.ShellZMin;
            InteriorQuads.RectX(outer, sx0, z0, zN, 0.0, top, -1);
            InteriorQuads.RectX(outer, sx1, z0, zN, 0.0, top, +1);
            InteriorQuads.RectY(outer, top, sx0, sx1, z0, zN, +1);
            InteriorQuads.RectZ(outer, z0, sx0, sx1, 0.0, top, -1);
            // 2 cm-scale chamfer read at the roof edges: a 0.06 m drip-edge course along both eaves
            InteriorQuads.Box(outer, sx0 - 0.06, top - 0.18, z0, sx0, top + 0.02, zN);
            InteriorQuads.Box(outer, sx1, top - 0.18, z0, sx1 + 0.06, top + 0.02, zN);
            Flush(ctx, root, ref outer, "b1_shell_outer", m_wall);
        }

        static Vector3 V(double x, double y, double z) => new Vector3((float)x, (float)y, (float)z);

        /// <summary>Depth of the crack backings behind the panel walls (m).</summary>
        public const double BackingGap = 0.03;

        /// <summary>Vertical wall at x over z [a, b], y [y0, y1] with rectangular holes (sorted by z, inside the span), facing
        /// +x (sign +1) or -x.</summary>
        static void WallWithHoles(MeshBuilder mb, double x, double a, double b, double y0, double y1, List<(double z0, double z1, double y0, double y1)> holes, int sign)
        {
            double z = a;
            foreach (var h in holes)
            {
                InteriorQuads.RectX(mb, x, z, h.z0, y0, y1, sign);
                InteriorQuads.RectX(mb, x, h.z0, h.z1, y0, h.y0, sign);
                InteriorQuads.RectX(mb, x, h.z0, h.z1, h.y1, y1, sign);
                z = h.z1;
            }
            InteriorQuads.RectX(mb, x, z, b, y0, y1, sign);
        }

        // ---------------------------------------------------------------- slits: pilasters, channels, baffle glow, hoods
        void BuildSlits(WorldContext ctx, Transform root)
        {
            var pil = new MeshBuilder();
            var ch = new MeshBuilder();
            var hood = new MeshBuilder();
            double px = BulwarkConst.SlitX, pr = Beat01Layout.PanelX1, rec = Beat01Layout.Recess;
            double hw = Beat01Layout.SlitHalf, y0 = BulwarkConst.SlitY0, y1 = BulwarkConst.SlitY1, back = Beat01Layout.SlitBack;
            for (int k = 0; k < BulwarkConst.SlitCount; k++)
            {
                double sz = Beat01Layout.SlitZ(k), p0 = sz - Beat01Layout.PilasterHalf, p1 = sz + Beat01Layout.PilasterHalf;
                // pilaster front (x 0.5, facing -x) with the slit mouth, up to where the ceiling arc crosses x 0.5; its side returns
                // follow the arc (no open wedge between the pilaster and the recessed arc)
                double ft = ProfileTop(px, rec);
                pil.Color = Ao(255, 0, 0);
                InteriorQuads.RectX(pil, px, p0, sz - hw, 0.0, ft, -1);
                InteriorQuads.RectX(pil, px, sz + hw, p1, 0.0, ft, -1);
                InteriorQuads.RectX(pil, px, sz - hw, sz + hw, 0.0, y0, -1);
                InteriorQuads.RectX(pil, px, sz - hw, sz + hw, y1, ft, -1);
                pil.Color = Ao(235, 120, 0);
                for (int e = 0; e < 2; e++)
                {
                    double pz = e == 0 ? p0 : p1;
                    var fn = new Vector3(0, 0, e == 0 ? -1 : 1);
                    double xa0 = px;
                    for (int j = 0; j <= 6; j++)
                    {
                        double xb0 = px + (pr - px) * (j + 1) / 7.0;
                        InteriorQuads.Face(pil, V(xa0, 0.0, pz), V(xa0, ProfileTop(xa0, rec), pz), V(xb0, ProfileTop(xb0, rec), pz), V(xb0, 0.0, pz), fn);
                        xa0 = xb0;
                    }
                }
                // channel 1.2 m deep (ConcreteDark, darker toward the back: vertex AO), its sill, head and the baffle glow
                ch.Color = Ao(200, 0, 0);
                InteriorQuads.RectZ(ch, sz - hw, px, back, y0, y1, +1);
                InteriorQuads.RectZ(ch, sz + hw, px, back, y0, y1, -1);
                InteriorQuads.RectY(ch, y0, px, back, sz - hw, sz + hw, +1);
                InteriorQuads.RectY(ch, y1, px, back, sz - hw, sz + hw, -1);
                var glow = new MeshBuilder();
                InteriorQuads.RectX(glow, back, sz - hw, sz + hw, y0, y1, -1);
                Flush(ctx, root, ref glow, "b1_lamp_slit_" + k, m_slit[k], false);
                m_slitLight[k] = m_pool.Add(new Vector3((float)(px - 0.35), (float)(0.5 * (y0 + y1)), (float)sz), BulwarkPalette.Linear(Pal.StormHorizon), SlitSpillRange, SlitSpillIntensity, "beat1_slit", SlitSpillRank);
                // the exterior hood on the crest deck (x 1.1 .. 1.95): the baffle box the storm light leaks round
                hood.Color = Ao(255, 0, 0);
                double hz0 = sz - Beat01Layout.HoodHalfZ, hz1 = sz + Beat01Layout.HoodHalfZ, hx0 = Beat01Layout.ShellX1, hx1 = Beat01Layout.HoodX1;
                InteriorQuads.RectX(hood, hx1, hz0, hz1, Beat01Layout.HoodY0, Beat01Layout.HoodY1, +1);
                InteriorQuads.RectY(hood, Beat01Layout.HoodY1, hx0, hx1, hz0, hz1, +1);
                InteriorQuads.RectY(hood, Beat01Layout.HoodY0, hx0, hx1, hz0, hz1, -1);
                InteriorQuads.RectZ(hood, hz0, hx0, hx1, Beat01Layout.HoodY0, Beat01Layout.HoodY1, -1);
                InteriorQuads.RectZ(hood, hz1, hx0, hx1, Beat01Layout.HoodY0, Beat01Layout.HoodY1, +1);
            }
            Flush(ctx, root, ref pil, "b1_pilasters", m_wall);
            Flush(ctx, root, ref ch, "b1_slit_channels", m_dark);
            Flush(ctx, root, ref hood, "b1_slit_hoods", m_wall);
        }

        // ---------------------------------------------------------------- niches with valve wheels and cabinets
        void BuildNiches(WorldContext ctx, Transform root)
        {
            var lin = new MeshBuilder();
            var steel = new MeshBuilder();
            var ochre = new MeshBuilder();
            double xb = Beat01Layout.NicheBackX, xp = Beat01Layout.PanelX0, top = Beat01Layout.NicheTop, hw = Beat01Layout.NicheHalf;
            for (int n = 0; n < Beat01Layout.NicheCount; n++)
            {
                double nz = Beat01Layout.NicheZ(n);
                lin.Color = Ao(215, 0, 0);
                InteriorQuads.RectX(lin, xb, nz - hw, nz + hw, 0.0, top, +1);
                lin.Color = Ao(235, 90, 0);
                InteriorQuads.RectZ(lin, nz - hw, xb, xp, 0.0, top, +1);
                InteriorQuads.RectZ(lin, nz + hw, xb, xp, 0.0, top, -1);
                InteriorQuads.RectY(lin, top, xb, xp, nz - hw, nz + hw, -1);
                bool valve = Math.Abs(nz - BulwarkConst.ValveWheelZ1) < 0.5 || Math.Abs(nz - BulwarkConst.ValveWheelZ2) < 0.5;
                if (valve)
                {
                    // the branch from the lower main pipe down into the niche, a valve body, the stem and the 0.5 m wheel
                    float bx = (float)(xb + 0.09), vz = (float)nz, vy = (float)Beat01Layout.ValveY;
                    InteriorQuads.Tube(steel, new Vector3(bx, vy + 0.08f, vz), new Vector3(bx, (float)(Beat01Layout.PipeY[0] - 0.02), vz), 0.045f, 10, 1f, true);
                    InteriorQuads.Tube(steel, new Vector3(bx, (float)(Beat01Layout.PipeY[0] - 0.02), vz), new Vector3((float)Beat01Layout.PipeX, (float)(Beat01Layout.PipeY[0] - 0.02), vz), 0.045f, 10, 1f, true);
                    InteriorQuads.CBox(steel, xb, vy - 0.09, nz - 0.09, xb + 0.2, vy + 0.09, nz + 0.09, 0.015f);
                    float wx = (float)(xb + 0.24);
                    InteriorQuads.Tube(steel, new Vector3((float)(xb + 0.2), vy, vz), new Vector3(wx + 0.02f, vy, vz), 0.018f, 8, 1f, true);
                    float wr = 0.5f * (float)BulwarkConst.ValveWheelDiameter;
                    ochre.Color = Ao(255, 40, 0);
                    MeshKit.Torus(ochre, new Vector3(wx, vy, vz), Vector3.right, wr - 0.025f, 0.025f, 28, 8);
                    for (int sp = 0; sp < 3; sp++)
                    {
                        float ang = sp * Mathf.PI / 3f;
                        var dir = new Vector3(0f, Mathf.Cos(ang), Mathf.Sin(ang)) * (wr - 0.04f);
                        InteriorQuads.Tube(ochre, new Vector3(wx, vy, vz) - dir, new Vector3(wx, vy, vz) + dir, 0.014f, 6, 1f, false);
                    }
                    InteriorQuads.CBox(ochre, wx - 0.03, vy - 0.045, nz - 0.045, wx + 0.03, vy + 0.045, nz + 0.045, 0.01f);
                }
                else
                {
                    // a steel switch cabinet on the niche back (0.7 x 1.25 x 0.16) with an ochre warning band, conduit up to the tray
                    steel.Color = Ao(255, 0, 0);
                    InteriorQuads.CBox(steel, xb, 0.55, nz - 0.35, xb + 0.16, 1.8, nz + 0.35, 0.012f);
                    ochre.Color = Ao(255, 0, 0);
                    InteriorQuads.Box(ochre, xb + 0.16, 1.62, nz - 0.33, xb + 0.165, 1.7, nz + 0.33);
                    InteriorQuads.Tube(steel, new Vector3((float)(xb + 0.08), 1.8f, (float)nz + 0.2f), new Vector3((float)(xb + 0.08), (float)top, (float)nz + 0.2f), 0.025f, 8, 1f, false);
                }
            }
            Flush(ctx, root, ref lin, "b1_niches", m_wall);
            Flush(ctx, root, ref steel, "b1_niche_steel", m_steel);
            Flush(ctx, root, ref ochre, "b1_niche_ochre", m_ochre);
        }

        // ---------------------------------------------------------------- west-wall pipes, tray, band; south service door
        void BuildFittings(WorldContext ctx, Transform root)
        {
            var steel = new MeshBuilder();
            var ochre = new MeshBuilder();
            var dark = new MeshBuilder();
            double z0 = Beat01Layout.SouthWallZ, z1 = Beat01Layout.HousingZ0, xp = Beat01Layout.PanelX0;
            float px = (float)Beat01Layout.PipeX, pr = (float)Beat01Layout.PipeR;
            // two pipes the length of the gallery (through the ribs), brackets at every bay centre
            foreach (double py in Beat01Layout.PipeY)
                InteriorQuads.Tube(steel, new Vector3(px, (float)py, (float)z0), new Vector3(px, (float)py, (float)z1), pr, 14, 3f, false);
            // cable tray (x -3.65..-3.51, y 3.0..3.1): bottom and lip, three cables in it
            double tx1 = xp + Beat01Layout.TrayDepth, ty0 = Beat01Layout.TrayY0, ty1 = Beat01Layout.TrayY1;
            InteriorQuads.Box(steel, xp, ty0, z0, tx1, ty0 + 0.012, z1);
            InteriorQuads.Box(steel, tx1 - 0.012, ty0, z0, tx1, ty1, z1);
            for (int c = 0; c < 3; c++)
                InteriorQuads.Tube(dark, new Vector3((float)(xp + 0.03 + 0.035 * c), (float)(ty0 + 0.035), (float)z0), new Vector3((float)(xp + 0.03 + 0.035 * c), (float)(ty0 + 0.035), (float)z1), 0.018f, 8, 3f, false);
            for (int k = 0; k <= Beat01Layout.RibCount; k++)
            {
                double zc = k < Beat01Layout.RibCount ? Beat01Layout.RibZ(k) - 3.0 : Beat01Layout.RibZ(k - 1) + 1.6;
                if (zc < z0 + 0.3 || zc > z1 - 0.3) continue;
                foreach (double py in Beat01Layout.PipeY)
                {
                    InteriorQuads.CBox(steel, xp, py - pr - 0.04, zc - 0.04, px, py - pr, zc + 0.04, 0.01f);
                    InteriorQuads.CBox(steel, xp, py - 0.03, zc - 0.02, xp + 0.02, py + 0.03, zc + 0.02, 0.005f);
                }
                InteriorQuads.CBox(steel, xp, ty0 - 0.05, zc - 0.03, tx1, ty0, zc + 0.03, 0.01f);
            }
            // the ochre band at y 1.1 on the west panels (6 mm paint film over the bays, not over the niches or ribs)
            ochre.Color = Ao(255, 0, 0);
            double za = z0;
            for (int k = 0; k <= Beat01Layout.RibCount; k++)
            {
                double zb = k < Beat01Layout.RibCount ? Beat01Layout.RibZ(k) - Beat01Layout.RibHalf : z1;
                double zs = za;
                for (int n = 0; n < Beat01Layout.NicheCount; n++)
                {
                    double nz = Beat01Layout.NicheZ(n);
                    if (nz < za || nz > zb) continue;
                    InteriorQuads.Box(ochre, xp, Beat01Layout.BandY0, zs, xp + 0.006, Beat01Layout.BandY1, nz - Beat01Layout.NicheHalf);
                    zs = nz + Beat01Layout.NicheHalf;
                }
                InteriorQuads.Box(ochre, xp, Beat01Layout.BandY0, zs, xp + 0.006, Beat01Layout.BandY1, zb);
                if (k < Beat01Layout.RibCount) za = Beat01Layout.RibZ(k) + Beat01Layout.RibHalf;
            }
            // the closed service door in the south wall (where the walker came from): steel 1.2 x 2.2, 6 cm proud
            steel.Color = Ao(255, 0, 0);
            InteriorQuads.CBox(steel, Beat01Layout.DoorCx - 0.6, 0.0, z0, Beat01Layout.DoorCx + 0.6, 2.2, z0 + 0.06, 0.012f);
            ochre.Color = Ao(255, 0, 0);
            InteriorQuads.Box(ochre, Beat01Layout.DoorCx - 0.6, 2.2, z0, Beat01Layout.DoorCx + 0.6, 2.28, z0 + 0.04);
            InteriorQuads.CBox(steel, Beat01Layout.DoorCx + 0.38, 0.95, z0 + 0.06, Beat01Layout.DoorCx + 0.5, 1.05, z0 + 0.12, 0.01f);
            Flush(ctx, root, ref steel, "b1_fittings", m_steel);
            Flush(ctx, root, ref ochre, "b1_band", m_paint);
            Flush(ctx, root, ref dark, "b1_cables", m_dark);
        }

        // ---------------------------------------------------------------- the nine caged lamps (pivot at the ceiling panel)
        void BuildLamps(WorldContext ctx, Transform root)
        {
            // one mesh pair in pivot space, shared by the nine lamps
            var glass = new MeshBuilder();
            var cage = new MeshBuilder();
            float py = (float)Beat01Layout.LampPivotY;
            float g0 = (float)Beat01Layout.LampY0 - py, g1 = (float)Beat01Layout.LampY1 - py;   // -0.40 .. -0.20
            float hx = 0.5f * (float)BulwarkConst.GalleryLampW, hz = 0.1f;
            // glass tube 0.40 x 0.13 x 0.13 under a hood; cage of 12 mm bars; two hanger rods to the ceiling
            InteriorQuads.Box(glass, -hx + 0.025, g0 + 0.02, -hz + 0.035, hx - 0.025, g1 - 0.045, hz - 0.035);
            cage.Color = Ao(255, 0, 0);
            InteriorQuads.CBox(cage, -hx, g1 - 0.045, -hz, hx, g1, hz, 0.008f);
            var wire = new MeshBuilder();
            float wr = 0.5f * WireWidth;
            foreach (float cz in new[] { -hz + 0.015f, hz - 0.015f })
                foreach (float cy in new[] { g0 + 0.008f, g0 + 0.09f })
                    InteriorQuads.Tube(wire, new Vector3(-hx + 0.01f, cy, cz), new Vector3(hx - 0.01f, cy, cz), wr, 6, 1f, false);
            foreach (float cx in new[] { -hx + 0.015f, -0.07f, 0.07f, hx - 0.015f })
            {
                InteriorQuads.Tube(wire, new Vector3(cx, g0 + 0.006f, -hz + 0.01f), new Vector3(cx, g0 + 0.006f, hz - 0.01f), wr, 6, 1f, false);
                InteriorQuads.Tube(wire, new Vector3(cx, g0, -hz + 0.015f), new Vector3(cx, g1 - 0.045f, -hz + 0.015f), wr, 6, 1f, false);
                InteriorQuads.Tube(wire, new Vector3(cx, g0, hz - 0.015f), new Vector3(cx, g1 - 0.045f, hz - 0.015f), wr, 6, 1f, false);
            }
            InteriorQuads.Tube(wire, new Vector3(-0.17f, g1, 0f), new Vector3(-0.17f, 0f, 0f), wr, 6, 1f, false);
            InteriorQuads.Tube(wire, new Vector3(0.17f, g1, 0f), new Vector3(0.17f, 0f, 0f), wr, 6, 1f, false);
            InteriorQuads.CBox(cage, -0.22, -0.012, -0.05, 0.22, 0.0, 0.05, 0.004f);   // ceiling plate (12 mm, touches the panel)
            Mesh gm = glass.ToMesh("b1_lamp_glass"), cm = cage.ToMesh("b1_lamp_cage"), wm = wire.ToMesh("b1_lamp_wire");
            var colour = BulwarkPalette.Linear(Pal.SodiumWarm);
            // the nine lamps of 14.5.1 (k 1-9, z 12 k) and the door-bay lamp (index 9, z 118.3, raised 0.1 m: Beat01Layout)
            for (int i = 0; i < Beat01Layout.LampTotal; i++)
            {
                var pivot = new GameObject(i < BulwarkConst.GalleryLampCount ? "b1_lamp_" + (i + 1) : "b1_lamp_door").transform;
                pivot.SetParent(root, false);
                pivot.localPosition = new Vector3((float)Beat01Layout.LampX, py + (float)Beat01Layout.LampRaiseAt(i), (float)Beat01Layout.LampZAt(i));
                ctx.AddMesh(pivot, "b1_lamp_glass", gm, m_sodium, false, false);
                ctx.AddMesh(pivot, "b1_lamp_cage", cm, m_steel, true, true);
                if (m_wire != null) ctx.AddMesh(pivot, "b1_lamp_wire", wm, m_wire, false, true);
                m_tris += glass.TriangleCount + cage.TriangleCount + wire.TriangleCount;
                m_lampPivot[i] = pivot;
                m_lampLight[i] = m_pool.Add(LightPos(i, 0f), colour, Beat01Layout.LampRange, Beat01Layout.LampIntensityAt(i), "beat1");
            }
        }

        /// <summary>World position of lamp i's real light for a sway angle (deg) about the corridor axis.</summary>
        static Vector3 LightPos(int i, float angleDeg)
        {
            float arm = (float)(Beat01Layout.LampPivotY - Beat01Layout.LampLightY + (i < BulwarkConst.GalleryLampCount ? 0.0 : Beat01Layout.DoorLampLightDrop));
            float a = angleDeg * Mathf.Deg2Rad;
            float py = (float)(Beat01Layout.LampPivotY + Beat01Layout.LampRaiseAt(i));
            return new Vector3((float)Beat01Layout.LampX + arm * Mathf.Sin(a), py - arm * Mathf.Cos(a), (float)Beat01Layout.LampZAt(i));
        }

        // ---------------------------------------------------------------- the door
        void BuildDoor(WorldContext ctx, Transform root)
        {
            var con = new MeshBuilder();
            con.Color = Ao(255, 0, 0);
            double hz0 = Beat01Layout.HousingZ0, hz1 = Beat01Layout.HousingZ1, dz = Beat01Layout.DoorZ, dz1 = Beat01Layout.DoorZ1;
            double top = Beat01Layout.ShellTop, lh = Beat01Layout.LeafH;
            foreach (int side in new[] { -1, 1 })
            {
                // pocket housing: south block, north block, slot back and slot top (the 0.7 m leaf runs in z 120 .. 120.7)
                double ja = side < 0 ? Beat01Layout.DoorX0 : Beat01Layout.DoorX1;             // jamb
                double ex = side < 0 ? Beat01Layout.PocketX0 : Beat01Layout.PocketX1;          // outer end
                double xa = Math.Min(ja, ex), xb = Math.Max(ja, ex);
                InteriorQuads.CBox(con, xa, 0.0, hz0, xb, top, dz, 0.02f);
                InteriorQuads.CBox(con, xa, 0.0, dz1, xb, top, hz1, 0.02f);
                double backX0 = side < 0 ? ex : ex - 0.1, backX1 = side < 0 ? ex + 0.1 : ex;
                InteriorQuads.Box(con, backX0, 0.0, dz, backX1, top, dz1);
                InteriorQuads.Box(con, xa + (side < 0 ? 0.1 : 0.0), lh, dz, xb - (side < 0 ? 0.0 : 0.1), top, dz1);
            }
            // lintel over the opening (y 3.5 .. 4.2) and the head beams that hide the leaf tops (opening 3.4 high)
            InteriorQuads.Box(con, Beat01Layout.DoorX0, lh, hz0, Beat01Layout.DoorX1, top, hz1);
            InteriorQuads.CBox(con, Beat01Layout.DoorX0, Beat01Layout.OpenH, dz - 0.15, Beat01Layout.DoorX1, lh, dz, 0.015f);
            InteriorQuads.CBox(con, Beat01Layout.DoorX0, Beat01Layout.OpenH, dz1, Beat01Layout.DoorX1, lh, dz1 + 0.15, 0.015f);
            Flush(ctx, root, ref con, "b1_door_housing", m_wall);

            // leaves: SteelPaint 2.0 x 3.5 x 0.7 (2 cm chamfers), the ochre seam band on the meeting edge and a band at 1.1
            m_leafW = Leaf(ctx, root, "b1_door_leaf_w", +1);
            m_leafE = Leaf(ctx, root, "b1_door_leaf_e", -1);
            m_leafW0 = new Vector3((float)(Beat01Layout.DoorCx - 0.5 * Beat01Layout.LeafW), 0f, (float)(0.5 * (dz + dz1)));
            m_leafE0 = new Vector3((float)(Beat01Layout.DoorCx + 0.5 * Beat01Layout.LeafW), 0f, (float)(0.5 * (dz + dz1)));
            m_leafW.localPosition = m_leafW0;
            m_leafE.localPosition = m_leafE0;

            // the manual drive: a 0.6 m hand wheel on a gearbox on the east panel beside the door (x 0.65 -> 0.53)
            var steel = new MeshBuilder();
            var ochre = new MeshBuilder();
            float wx = (float)(Beat01Layout.PanelX1 - 0.1), wy = (float)Beat01Layout.HandWheelY, wz = (float)Beat01Layout.HandWheelZ;
            InteriorQuads.CBox(steel, Beat01Layout.PanelX1 - 0.07, wy - 0.2, wz - 0.16, Beat01Layout.PanelX1, wy + 0.2, wz + 0.16, 0.01f);
            InteriorQuads.Tube(steel, new Vector3(wx + 0.03f, wy, wz), new Vector3(wx - 0.02f, wy, wz), 0.025f, 8, 1f, true);
            float r = (float)Beat01Layout.HandWheelR;
            ochre.Color = Ao(255, 40, 0);
            MeshKit.Torus(ochre, new Vector3(wx - 0.03f, wy, wz), Vector3.right, r - 0.025f, 0.025f, 32, 8);
            for (int sp = 0; sp < 3; sp++)
            {
                float ang = sp * Mathf.PI / 3f + 0.3f;
                var dir = new Vector3(0f, Mathf.Cos(ang), Mathf.Sin(ang)) * (r - 0.04f);
                InteriorQuads.Tube(ochre, new Vector3(wx - 0.03f, wy, wz) - dir, new Vector3(wx - 0.03f, wy, wz) + dir, 0.014f, 6, 1f, false);
            }
            // drive shaft from the gearbox up and along the wall to the door housing (steel, behind the rib plane)
            InteriorQuads.Tube(steel, new Vector3((float)(Beat01Layout.PanelX1 - 0.05), wy + 0.2f, wz), new Vector3((float)(Beat01Layout.PanelX1 - 0.05), 3.2f, wz), 0.03f, 8, 1f, true);
            InteriorQuads.Tube(steel, new Vector3((float)(Beat01Layout.PanelX1 - 0.05), 3.2f, wz), new Vector3((float)(Beat01Layout.PanelX1 - 0.05), 3.2f, (float)hz0), 0.03f, 8, 1f, true);
            Flush(ctx, root, ref steel, "b1_door_drive", m_steel);
            Flush(ctx, root, ref ochre, "b1_door_wheel", m_ochre);

            // the door is a sky opening of the gallery while it is open (LocalLightPool _BwOpen; OnFrame sets the gap)
            m_opening = m_pool.AddOpening("beat1_door", (float)Beat01Layout.DoorCx, (float)Beat01Layout.DoorCx, 0f, (float)Beat01Layout.OpenH, (float)dz, true, 40f, false);
            SetGap(0f);
        }

        Transform Leaf(WorldContext ctx, Transform root, string name, int seamSide)
        {
            var go = new GameObject(name).transform;
            go.SetParent(root, false);
            double w = Beat01Layout.LeafW - 0.01, h = Beat01Layout.LeafH, t = Beat01Layout.LeafT - 0.02;
            var mb = new MeshBuilder();
            mb.Color = Ao(255, 0, 0);
            MeshKit.ChamferBox(mb, new Vector3(0f, (float)(0.5 * h), 0f), new Vector3((float)w, (float)h, (float)t), 0.02f);
            var mr = ctx.AddMesh(go, name, mb.ToMesh(name), m_steel);
            m_tris += mb.TriangleCount;
            var band = new MeshBuilder();
            float bx = seamSide * (0.5f * (float)w - 0.07f);
            float fz = -0.5f * (float)t - 0.004f;
            band.Box(new Vector3(bx, 0.5f * (float)h, fz), new Vector3(0.1f, (float)h - 0.1f, 0.008f));
            band.Box(new Vector3(0f, 1.1f, fz), new Vector3((float)w - 0.06f, 0.1f, 0.008f));
            band.Box(new Vector3(bx, 0.5f * (float)h, -fz), new Vector3(0.1f, (float)h - 0.1f, 0.008f));
            ctx.AddMesh(go, name + "_band", band.ToMesh(name + "_band"), m_ochre);
            m_tris += band.TriangleCount;
            return go;
        }

        /// <summary>Poses the door for progress s by the progress rule (the capture rule): the EditMode visibility audit calls it
        /// per key shot like GrayboxPath.SetDoorForS (shared change request: VisibilityAuditTests poses this builder too).</summary>
        public void SetDoorForS(double s) => SetGap((float)Beat01Layout.DoorGapProgress(s));

        void SetGap(float gap)
        {
            m_gap = gap;
            if (m_pool != null && m_opening >= 0)
            {
                m_pool.SetOpeningX(m_opening, (float)(Beat01Layout.DoorCx - 0.5 * gap), (float)(Beat01Layout.DoorCx + 0.5 * gap));
                m_pool.SetOpeningOn(m_opening, gap > 1e-3f);
            }
            if (m_leafW != null) m_leafW.localPosition = m_leafW0 + new Vector3(-0.5f * gap, 0f, 0f);
            if (m_leafE != null) m_leafE.localPosition = m_leafE0 + new Vector3(0.5f * gap, 0f, 0f);
        }

        // ---------------------------------------------------------------- the gallery air: dust motes and slit breaths (BwB1Haze)
        public const string HazeShader = "Megastructure/BwB1Haze";
        public const float WireWidth = 0.012f;
        public const int MotesPerLamp = 300;                 // 14.5.1: 300 motes per lamp cone (9 lamps: 2700 <= 3000)
        public const float MoteIntensity = 1.4f, MoteSize = 0.005f, BreathIntensity = 0.16f;
        public const int PuffsPerSlit = 14;
        Material m_motes, m_breath, m_grains;
        public const int GrainsPerSlit = 60;                 // 14.5.1: a falling grain sheet (60 soft quads) at each impact
        public const float GrainIntensity = 0.55f;
        /// <summary>Dust is lit by the sodium lamps but reads desaturated (grains of concrete, not embers): the light colour mixed
        /// with FoamWhite (palette tokens) by this fraction.</summary>
        public const float DustWhiteMix = 0.45f;
        static readonly int[] IdB = { Shader.PropertyToID("_B0"), Shader.PropertyToID("_B1"), Shader.PropertyToID("_B2"), Shader.PropertyToID("_B3"), Shader.PropertyToID("_B4") };
        readonly Vector4[] m_b = new Vector4[5];
        double m_breathQa = double.NaN;

        static float Hash01(uint i)
        {
            i ^= i >> 16; i *= 0x7feb352du; i ^= i >> 15; i *= 0x846ca68bu; i ^= i >> 16;
            return (i & 0xFFFFFFu) / 16777216f;
        }

        void BuildHaze(WorldContext ctx, Transform root)
        {
            var sh = Shader.Find(HazeShader);
            if (sh == null) { Debug.LogWarning("[BW] Beat01: shader " + HazeShader + " not found (RebuildAll: Always Included Shaders): no dust motes, no slit breath"); return; }
            m_motes = new Material(sh) { name = "b1_mist_motes" };
            m_motes.SetColor("_Color", Color.Lerp(BulwarkPalette.Linear(Pal.SodiumWarm), BulwarkPalette.Linear(Pal.FoamWhite), DustWhiteMix));
            m_motes.SetFloat("_Intensity", MoteIntensity); m_motes.SetFloat("_Mode", 0f); m_motes.SetFloat("_Size", MoteSize); m_motes.SetFloat("_FadeFar", 26f);
            RuntimeMaterials.Add(m_motes);
            m_breath = new Material(sh) { name = "b1_mist_breath" };
            m_breath.SetColor("_Color", Color.Lerp(BulwarkPalette.Linear(Pal.StormHorizon), BulwarkPalette.Linear(Pal.FoamWhite), 0.35f));
            m_breath.SetFloat("_Intensity", BreathIntensity); m_breath.SetFloat("_Mode", 1f); m_breath.SetFloat("_FadeFar", 60f);
            RuntimeMaterials.Add(m_breath);

            // ---- motes: volume-uniform in a 38 deg cone under each lamp (0.3 .. 3.3 m below the glass), inside the corridor
            var pos = new List<Vector3>(4 * MotesPerLamp * 9);
            var uv0 = new List<Vector4>(pos.Capacity);
            var uv1 = new List<Vector4>(pos.Capacity);
            var idx = new List<int>(6 * MotesPerLamp * 9);
            uint seq = 7u;
            float tanA = Mathf.Tan(38f * Mathf.Deg2Rad);
            for (int k = 1; k <= BulwarkConst.GalleryLampCount; k++)
            {
                var lp = new Vector3((float)Beat01Layout.LampX, (float)Beat01Layout.LampY0 - 0.05f, (float)Beat01Layout.LampZ(k));
                int n = 0, guard = 0;
                while (n < MotesPerLamp && guard++ < 20 * MotesPerLamp)
                {
                    float u1 = Hash01(seq++), u2 = Hash01(seq++), u3 = Hash01(seq++), sd = Hash01(seq++);
                    float hgt = 0.3f + 3.0f * Mathf.Pow(u1, 1f / 3f);
                    float rmax = hgt * tanA, r = rmax * Mathf.Sqrt(u2), ang = 2f * Mathf.PI * u3;
                    var p = lp + new Vector3(r * Mathf.Cos(ang), -hgt, r * Mathf.Sin(ang));
                    if (p.x < Beat01Layout.X0 + 0.15f || p.x > Beat01Layout.X1 - 0.15f || p.y < 0.08f) continue;
                    float w = Mathf.Sqrt(Mathf.Max(0f, 1f - r / Mathf.Max(rmax, 1e-3f))) / (1f + (hgt / 1.6f) * (hgt / 1.6f));
                    int first = pos.Count;
                    for (int c = 0; c < 4; c++)
                    {
                        pos.Add(p);
                        uv0.Add(new Vector4((c & 1) == 0 ? -1f : 1f, (c & 2) == 0 ? -1f : 1f, sd, w));
                        uv1.Add(new Vector4(lp.x, lp.y, lp.z, 0f));
                    }
                    idx.Add(first); idx.Add(first + 2); idx.Add(first + 3); idx.Add(first); idx.Add(first + 3); idx.Add(first + 1);
                    n++;
                }
            }
            HazeMesh(root, "b1_mist_motes", pos, uv0, uv1, idx, m_motes);

            // ---- breath: eight puffs per slit, from the mouth into the gallery (mostly -x, fanned in y and z)
            pos.Clear(); uv0.Clear(); uv1.Clear(); idx.Clear();
            for (int k = 0; k < BulwarkConst.SlitCount; k++)
            {
                var mouth = new Vector3((float)(BulwarkConst.SlitX - 0.1), (float)(0.5 * (BulwarkConst.SlitY0 + BulwarkConst.SlitY1)), (float)Beat01Layout.SlitZ(k));
                for (int j = 0; j < PuffsPerSlit; j++)
                {
                    float a = Hash01(seq++), b = Hash01(seq++), sd = Hash01(seq++);
                    var dir = new Vector3(-1f, (j - 0.5f * (PuffsPerSlit - 1)) * 0.05f, (a - 0.5f) * 1.1f).normalized;
                    float spread = 0.6f + 0.7f * b;
                    var p = mouth + new Vector3(0f, (j - 0.5f * (PuffsPerSlit - 1)) * 0.1f, 0f);
                    int first = pos.Count;
                    for (int c = 0; c < 4; c++)
                    {
                        pos.Add(p);
                        uv0.Add(new Vector4((c & 1) == 0 ? -1f : 1f, (c & 2) == 0 ? -1f : 1f, sd, k));
                        uv1.Add(new Vector4(dir.x, dir.y, dir.z, spread));
                    }
                    idx.Add(first); idx.Add(first + 2); idx.Add(first + 3); idx.Add(first); idx.Add(first + 3); idx.Add(first + 1);
                }
            }
            HazeMesh(root, "b1_mist_breath", pos, uv0, uv1, idx, m_breath);

            // ---- grains: 60 per slit bay, shaken off the soffit joints within 2.6 m of the slit (the bay between two ribs)
            m_grains = new Material(sh) { name = "b1_mist_grains" };
            m_grains.SetColor("_Color", Color.Lerp(BulwarkPalette.Linear(Pal.SodiumWarm), BulwarkPalette.Linear(Pal.FoamWhite), DustWhiteMix));
            m_grains.SetFloat("_Intensity", GrainIntensity); m_grains.SetFloat("_Mode", 2f); m_grains.SetFloat("_FadeFar", 24f);
            RuntimeMaterials.Add(m_grains);
            pos.Clear(); uv0.Clear(); uv1.Clear(); idx.Clear();
            for (int k = 0; k < BulwarkConst.SlitCount; k++)
            {
                double sz = Beat01Layout.SlitZ(k);
                for (int j = 0; j < GrainsPerSlit; j++)
                {
                    float a = Hash01(seq++), b = Hash01(seq++), c2 = Hash01(seq++), d = Hash01(seq++), e = Hash01(seq++);
                    var p = new Vector3(Mathf.Lerp((float)Beat01Layout.X0 + 0.2f, (float)Beat01Layout.X1 - 0.2f, a), (float)Beat01Layout.H - 0.02f,
                                        (float)sz + (b - 0.5f) * 5.2f);
                    int first = pos.Count;
                    for (int c = 0; c < 4; c++)
                    {
                        pos.Add(p);
                        uv0.Add(new Vector4((c & 1) == 0 ? -1f : 1f, (c & 2) == 0 ? -1f : 1f, e, k));
                        uv1.Add(new Vector4(0.6f + 0.8f * c2, 0.05f + 0.6f * d, (e - 0.5f) * 0.06f, 0.0025f + 0.003f * a));
                    }
                    idx.Add(first); idx.Add(first + 2); idx.Add(first + 3); idx.Add(first); idx.Add(first + 3); idx.Add(first + 1);
                }
            }
            HazeMesh(root, "b1_mist_grains", pos, uv0, uv1, idx, m_grains);

            // ---- slit burst (Wave X2 fix): a whitewater sheet on each baffle face (4 mm in front of the glow) ...
            var spray = Color.Lerp(BulwarkPalette.Linear(Pal.StormHorizon), BulwarkPalette.Linear(Pal.FoamWhite), 0.7f);
            m_sheet = new Material(sh) { name = "b1_mist_slit_sheet" };
            m_sheet.SetColor("_Color", spray);
            m_sheet.SetFloat("_Intensity", SlitSheetIntensity); m_sheet.SetFloat("_Mode", 3f); m_sheet.SetFloat("_FadeFar", 60f);
            RuntimeMaterials.Add(m_sheet);
            pos.Clear(); uv0.Clear(); uv1.Clear(); idx.Clear();
            double hw = Beat01Layout.SlitHalf, sy0 = BulwarkConst.SlitY0, sy1 = BulwarkConst.SlitY1;
            float bx = (float)(Beat01Layout.SlitBack - 0.004);
            for (int k = 0; k < BulwarkConst.SlitCount; k++)
            {
                double sz = Beat01Layout.SlitZ(k);
                float sd = Hash01(seq++);
                int first = pos.Count;
                for (int c = 0; c < 4; c++)
                {
                    double dz = (c & 1) == 0 ? -hw : hw, dy = (c & 2) == 0 ? 0.0 : sy1 - sy0;
                    pos.Add(new Vector3(bx, (float)(sy0 + dy), (float)(sz + dz)));
                    uv0.Add(new Vector4((float)dz, (float)dy, sd, k));
                    uv1.Add(Vector4.zero);
                }
                // faces -x (toward the gallery); Cull Off in the shader, the winding only matters for consistency
                idx.Add(first); idx.Add(first + 2); idx.Add(first + 3); idx.Add(first); idx.Add(first + 3); idx.Add(first + 1);
                // the sheet's light on both channel cheeks, 4 mm off the cheek faces (uv1.x = 1: cheek; uv0.x = depth from the baffle)
                for (int e = 0; e < 2; e++)
                {
                    float cz = (float)(e == 0 ? sz - hw + 0.004 : sz + hw - 0.004);
                    first = pos.Count;
                    for (int c = 0; c < 4; c++)
                    {
                        double d = (c & 1) == 0 ? 0.0 : Beat01Layout.SlitBack - BulwarkConst.SlitX, dy = (c & 2) == 0 ? 0.0 : sy1 - sy0;
                        pos.Add(new Vector3((float)(Beat01Layout.SlitBack - 0.004 - d * (1.0 - 0.004 / 1.2)), (float)(sy0 + dy), cz));
                        uv0.Add(new Vector4((float)d, (float)dy, sd, k));
                        uv1.Add(new Vector4(1f, 0f, 0f, 0f));
                    }
                    idx.Add(first); idx.Add(first + 2); idx.Add(first + 3); idx.Add(first); idx.Add(first + 3); idx.Add(first + 1);
                }
            }
            HazeMesh(root, "b1_mist_slit_sheet", pos, uv0, uv1, idx, m_sheet);
            // ... and spray clumps thrown off the baffle toward the mouth (1.2-3.4 m/s), faded on the sill and past the mouth
            m_spray = new Material(sh) { name = "b1_mist_slit_spray" };
            m_spray.SetColor("_Color", spray);
            m_spray.SetFloat("_Intensity", SlitSprayIntensity); m_spray.SetFloat("_Mode", 4f); m_spray.SetFloat("_FadeFar", 30f);
            m_spray.SetFloat("_Size", (float)BulwarkConst.SlitX); m_spray.SetFloat("_FloorY", (float)sy0);
            RuntimeMaterials.Add(m_spray);
            pos.Clear(); uv0.Clear(); uv1.Clear(); idx.Clear();
            for (int k = 0; k < BulwarkConst.SlitCount; k++)
            {
                double sz = Beat01Layout.SlitZ(k);
                for (int j = 0; j < SprayPerSlit; j++)
                {
                    float a = Hash01(seq++), b = Hash01(seq++), c2 = Hash01(seq++), d = Hash01(seq++), e = Hash01(seq++);
                    var p = new Vector3((float)(Beat01Layout.SlitBack - 0.06), (float)(sy0 + 0.15 + 1.5 * a), (float)(sz + (b - 0.5) * 0.22));
                    var vel = new Vector3(-(1.2f + 2.2f * c2), 0.3f + 2.4f * d, (b - 0.5f) * 0.3f);
                    float delay = 0.05f + 0.45f * a * e;   // the low clumps first: the front rises through the slit
                    int first = pos.Count;
                    for (int c = 0; c < 4; c++)
                    {
                        pos.Add(p);
                        uv0.Add(new Vector4((c & 1) == 0 ? -1f : 1f, (c & 2) == 0 ? -1f : 1f, e, k));
                        uv1.Add(new Vector4(vel.x, vel.y, vel.z, delay));
                    }
                    idx.Add(first); idx.Add(first + 2); idx.Add(first + 3); idx.Add(first); idx.Add(first + 3); idx.Add(first + 1);
                }
            }
            HazeMesh(root, "b1_mist_slit_spray", pos, uv0, uv1, idx, m_spray);
            for (int i = 0; i < m_b.Length; i++)
            {
                m_breath.SetVector(IdB[i], Vector4.zero); m_grains.SetVector(IdB[i], Vector4.zero);
                m_sheet.SetVector(IdB[i], Vector4.zero); m_spray.SetVector(IdB[i], Vector4.zero);
            }
            // QA: -b1-breath-qa AGE forces the z 66 slit's breath at that age (look-dev stills; the sea owner forces the KS02
            // impact in the key-shot set itself)
            if (CliArgs.Has("-b1-breath-qa")) m_breathQa = CliArgs.GetDouble("-b1-breath-qa", 1.2);
        }

        void HazeMesh(Transform root, string name, List<Vector3> pos, List<Vector4> uv0, List<Vector4> uv1, List<int> idx, Material mat)
        {
            var mesh = new Mesh { name = name };
            mesh.SetVertices(pos);
            mesh.SetUVs(0, uv0);
            mesh.SetUVs(1, uv1);
            mesh.SetTriangles(idx, 0, false);
            // the quads expand in the vertex shader: explicit bounds round the gallery (centres + 1.5 m)
            mesh.bounds = new Bounds(new Vector3(-1.5f, 2.1f, 60f), new Vector3(6f, 5.5f, 124f));
            var go = new GameObject(name);
            go.transform.SetParent(root, false);
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            var mr = go.AddComponent<MeshRenderer>();
            mr.sharedMaterial = mat;
            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            mr.receiveShadows = false;
            mr.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
            mr.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
            m_tris += idx.Count / 3;
        }

        /// <summary>Spill-light gain of slit k for a breath envelope env (1 at rest, 1 + SlitBurstSpill at a full burst; the
        /// envelope is scaled x2.2 and clamped like the sheet's, so a 0.45-strength impact already flares fully).</summary>
        public static float SpillGain(float env) => 1f + SlitBurstSpill * Mathf.Clamp01(env * 2.2f);

        void SetSpill(int k, float env)
        {
            if (m_pool == null || k < 0 || k >= m_slitLight.Length) return;
            float g = SpillGain(env);
            if (Mathf.Abs(g - m_slitSpill[k]) < 1e-3f) return;
            m_slitSpill[k] = g;
            m_pool.SetGain(m_slitLight[k], g);
        }

        /// <summary>Breath of slit k at ambient time t: (envelope 0..1, age s) of the strongest impact within reach.</summary>
        static void BreathAt(int k, double t, out float env, out float age)
        {
            env = 0f; age = 0f;
            double sz = Beat01Layout.SlitZ(k);
            for (int j = 0; j < WaveImpacts.Count; j++)
            {
                var w = WaveImpacts.Get(j);
                double a = t - w.T0;
                if (a < 0.0 || a > 3.0) continue;
                double dz = w.Z - sz;
                double e = (a < 0.6 ? BwMath.Smootherstep01(a / 0.6) : 1.0 - BwMath.Smootherstep01((a - 0.6) / 2.4))
                           * Math.Min(1.0, Math.Max(0.0, w.Strength)) * Math.Exp(-(dz * dz) / (2.0 * 12.0 * 12.0));
                if (e > env) { env = (float)e; age = (float)a; }
            }
        }

        // ================================================================ per frame
        public void OnFrame(in FrameState f)
        {
            double s = f.s, t = f.ambientTime;
            // ---- door (T2): timed from the trigger with the progress fallback; capture stills use the progress rule only
            if (s < BulwarkConst.DoorTriggerS - 2.0) m_trigger = double.NaN;
            else if (double.IsNaN(m_trigger) && s >= BulwarkConst.DoorTriggerS && !f.capture) m_trigger = t;
            double gap = f.capture ? Beat01Layout.DoorGapProgress(s) : Beat01Layout.DoorGap(s, double.IsNaN(m_trigger) ? double.NaN : t - m_trigger);
            if (Math.Abs(gap - m_gap) > 1e-5) SetGap((float)gap);
            if (s > 160.0) return;   // beyond the door's view nothing else of beat 1 moves

            // ---- K0 (A1 1.6): the lamps sway +-0.6 deg at s 66 without a boom (latched time; capture: progress time)
            if (s < Beat01Layout.K0S - 2.0) m_k0 = double.NaN;
            else if (double.IsNaN(m_k0) && s >= Beat01Layout.K0S && !f.capture) m_k0 = t;
            double tk0 = f.capture ? (s - Beat01Layout.K0S) / BulwarkConst.Cine : (double.IsNaN(m_k0) ? -1.0 : t - m_k0);

            int n = WaveImpacts.Count;
            for (int i = 0; i < m_lampPivot.Length; i++)
            {
                double lz = Beat01Layout.LampZAt(i);
                double a = Beat01Layout.SwayDeg(tk0, Beat01Layout.K0KickDeg);
                for (int j = 0; j < n; j++)
                {
                    var w = WaveImpacts.Get(j);
                    double dz = w.Z - lz;
                    a += Beat01Layout.SwayDeg(t - w.T0, Beat01Layout.ImpactKickDeg(w.Strength) * Math.Exp(-(dz * dz) / (2.0 * 60.0 * 60.0)));
                }
                float af = (float)a;
                if (Mathf.Abs(af - m_lampAngle[i]) < 1e-4f) continue;
                m_lampAngle[i] = af;
                m_lampPivot[i].localRotation = Quaternion.AngleAxis(af, Vector3.forward);
                m_pool.SetPosition(m_lampLight[i], LightPos(i, af));
            }
            // ---- slit breaths (BwB1Haze puffs): envelope and age per slit
            if (m_breath != null)
            {
                for (int i = 0; i < m_b.Length; i++)
                {
                    BreathAt(2 * i, t, out float e0, out float a0);
                    BreathAt(2 * i + 1, t, out float e1, out float a1);
                    if (!double.IsNaN(m_breathQa) && 2 * i + 1 == 5) { e1 = 1f; a1 = (float)m_breathQa; }
                    var v = new Vector4(e0, a0, e1, a1);
                    if (v == m_b[i]) continue;
                    m_b[i] = v;
                    m_breath.SetVector(IdB[i], v);
                    if (m_grains != null) m_grains.SetVector(IdB[i], v);
                    if (m_sheet != null) m_sheet.SetVector(IdB[i], v);
                    if (m_spray != null) m_spray.SetVector(IdB[i], v);
                    // the burst's storm light through the slit: the spill light flares with the same envelope
                    SetSpill(2 * i, e0);
                    SetSpill(2 * i + 1, e1);
                }
            }
            // ---- slit storm light +-25 % with the impact clock
            for (int k = 0; k < m_slit.Length; k++)
            {
                if (m_slit[k] == null) continue;
                double g = 0.0, sz = Beat01Layout.SlitZ(k);
                for (int j = 0; j < n; j++) { var w = WaveImpacts.Get(j); g += Beat01Layout.SlitFlare(t - w.T0, w.Strength, w.Z - sz); }
                float gain = 1f + (float)Math.Min(0.25, g);
                if (Mathf.Abs(gain - m_slitGain[k]) < 1e-3f) continue;
                m_slitGain[k] = gain;
                m_slit[k].SetFloat(IdIntensity, SlitIntensity * gain);
            }
        }
    }
}
