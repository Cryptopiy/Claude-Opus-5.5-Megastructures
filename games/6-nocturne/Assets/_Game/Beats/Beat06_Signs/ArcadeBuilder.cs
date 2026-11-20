// ArcadeBuilder: the arcade block of beat 6 (MASTER_PROMPT 14.4: x 914 - 990, z 236 - 360, 38 m high; 14.5.6; 14.6: its corner
// hides the plaza until s 1 250; SG Phase 4 wave A: INT-02 / INT-34 / CRIT0 #22, the "flat striped posters" seen from the Heart).
// A real building instead of a stripe box:
//  - the plinth (y -40..0.15: a 15 cm step up into the arcade, so its floor is never co-planar with the promenade or the plaza disc)
//    in granite; a colonnade on the west (promenade), north (plaza approach) and east (plaza) faces: columns 0.8 m, the shop glass
//    line 4 m back under the soffit of the upper floors (y 6.6), a steel fascia band, downlights in the soffit;
//  - one shop per bay: NocShopfront rooms (lit interiors with shelves and depth, material variants of BV's row), steel frames and
//    a centre mullion, piers between shops; every bay has a panel state (G4Kit.PickShop, BV's NocShopfront vertex colour: lit,
//    dim, half-lowered shutter, dark; 1 in 6 closed behind a roller shutter); each open bay hangs a small NocNeon blade sign; rooms
//    and signs are cool on the faces seen from the Heart (LOOK_V2 R6: no warm light in beat 7);
//  - the upper floors (y 6.6..38) clad in FA's S2 metal-panel skin with ribbon windows and LED-strip housings (the screen-wall
//    family, NocFacade, world-derived windows); on the faces toward the Heart two service catwalks (grating, rails, brackets)
//    with maintenance lights under them (NocHalos), and cable risers on the three faces: the lit back of the screen district.
// The south face (z 236) and the plinth's sides lie inside NotchEastBlock's lower part or under the floors (never seen). Nothing
// rises above the 38 m roof line (KS17's koi composition lists the roof as an occluder). Not an H4 wall plane (14.7 H4 lists the
// screen wall). Every part is pure (BuildParts: no scene objects), so Beat06SignsTests validates and counts them.
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Megastructure
{
    public static class ArcadeBuilder
    {
        public static readonly Vector3 Origin = new Vector3(
            (float)(0.5 * (SignsLayout.ArcX0 + SignsLayout.ArcX1)), 0f, (float)(0.5 * (SignsLayout.ArcZ0 + SignsLayout.ArcZ1)));

        /// <summary>Material of a part.</summary>
        public enum Mat { Stone, Concrete, Steel, Facade, Grating, Rail, Downlight, MaintLens, ShopCold, ShopWhite, ShopNeutral, ShopBlue, ShopClosed }

        public sealed class Part
        {
            public string Name;
            public Mat Mat;
            public readonly MeshBuilder Mb = new MeshBuilder();
        }

        /// <summary>A NocNeon sign instance (centre, rotation, size) with its palette slot.</summary>
        public struct Sign { public Vector3 centre; public Quaternion rotation; public Vector2 size; public int palette; }

        /// <summary>Everything the block is made of (pure). Signs and maintenance-light heads are returned for the batches / halos.</summary>
        public static List<Part> BuildParts(List<Sign> signs, List<Vector3> maintHeads)
        {
            var parts = new List<Part>();
            var byMat = new Dictionary<Mat, Part>();
            Part P(Mat m) { if (!byMat.TryGetValue(m, out var p)) { p = new Part { Name = "arcade" + m, Mat = m }; byMat[m] = p; parts.Add(p); } return p; }
            Vector3 O = Origin;
            double x0 = SignsLayout.ArcX0, x1 = SignsLayout.ArcX1, z0 = SignsLayout.ArcZ0, z1 = SignsLayout.ArcZ1;
            double r = SignsLayout.ArcRecess, soff = SignsLayout.ArcSoffit, top = SignsLayout.ArcH, fy = 0.15;

            // plinth, core, soffit, roof
            Box(P(Mat.Stone).Mb, O, x0, x1, SignsLayout.ArcY0, fy, z0, z1);
            Box(P(Mat.Concrete).Mb, O, x0 + r, x1 - r, fy - 0.01, soff - 0.05, z0, z1 - r);
            G4Kit.OrientedQuad(P(Mat.Concrete).Mb, V(x0, soff, z0) - O, V(x1, soff, z0) - O, V(x1, soff, z1) - O, V(x0, soff, z1) - O, Vector3.down);
            G4Kit.OrientedQuad(P(Mat.Concrete).Mb, V(x0, top, z0) - O, V(x1, top, z0) - O, V(x1, top, z1) - O, V(x0, top, z1) - O, Vector3.up);

            // upper floors: the S2 LED-housing skin (west, north, east; the south face lies on the east block)
            G4Kit.FacadeBox(P(Mat.Facade).Mb, O, x0, x1, z0, z1, soff, top, NocFacadeMath.SkinOverride(NocFacadeMath.OverrideScreenWall), 0,
                west: true, east: true, south: false, north: true);

            // fascia band round the soffit edge
            double fp = SignsLayout.FasciaProud, f1 = SignsLayout.FasciaY1;
            Box(P(Mat.Steel).Mb, O, x0 - fp, x0 + 0.1, soff - 0.02, f1, z0 + 0.2, z1 + fp);
            Box(P(Mat.Steel).Mb, O, x0 - fp, x1 + fp, soff - 0.025, f1 + 0.005, z1 - 0.1, z1 + fp);
            Box(P(Mat.Steel).Mb, O, x1 - 0.1, x1 + fp, soff - 0.03, f1 + 0.01, z0 + 0.2, z1 + fp);

            // the three colonnades: (face, bays) west x 914 (n -x), north z 360 (n +z), east x 990 (n +x)
            for (int face = 0; face < 3; face++)
            {
                int n = face == 0 ? SignsLayout.BaysWest : (face == 1 ? SignsLayout.BaysNorth : SignsLayout.BaysEast);
                double a = face == 1 ? x0 : z0, b = face == 1 ? x1 : z1, pitch = (b - a) / n;
                for (int k = 0; k <= n; k++)
                {
                    if (face == 1 && (k == 0 || k == n)) continue;                    // the corner columns belong to the west / east rows
                    double c = Math.Max(a + 0.4, Math.Min(b - 0.4, a + k * pitch));
                    Column(P(Mat.Concrete).Mb, O, face, c, fy);
                }
                for (int k = 0; k < n; k++)
                {
                    double ca = a + k * pitch, cb = ca + pitch, cm = 0.5 * (ca + cb);
                    uint h = NocHash.H3((uint)(face * 64 + k + 11), 0x41524344u, 3u, 0);
                    double u = NocHash.U01(h);
                    bool closed = u < 1.0 / 6.0;
                    // the room's light colour (closed bays keep a colour too: their shutters leak it)
                    double uc = closed ? NocHash.U01(NocHash.Pcg(h ^ 0x434F4C52u)) : u;
                    Mat room = face == 0 ? (uc < 0.40 ? Mat.ShopCold : (uc < 0.62 ? Mat.ShopWhite : (uc < 0.84 ? Mat.ShopNeutral : Mat.ShopBlue)))
                                         : (uc < 0.48 ? Mat.ShopCold : (uc < 0.78 ? Mat.ShopWhite : Mat.ShopBlue));
                    var state = G4Kit.PickShop(h, closed);
                    Shop(P(room).Mb, P(Mat.Steel).Mb, P(Mat.Concrete).Mb, O, face, ca, cb, fy, G4Kit.ShopColour(state));
                    if (state.Open && signs != null)
                    {
                        int pal = (int)(NocHash.Pcg(h) % 3u);
                        int slot = pal == 0 ? 1 : (pal == 1 ? 4 : 5);
                        if (face == 0 && NocHash.U01(NocHash.Pcg(h ^ 0x414D4252u)) < 0.25) slot = 2;   // a few amber shop signs on the promenade (x k_a)
                        signs.Add(ShopSign(face, cm, slot));
                    }
                    // two soffit downlights per bay
                    for (int j = 0; j < 2; j++) Downlight(P(Mat.Downlight).Mb, O, face, ca + (0.25 + 0.5 * j) * pitch);
                }
            }

            // the back side seen from the Heart: catwalks on the north and east faces, maintenance lights, cable risers
            foreach (double cy in SignsLayout.CatwalkY)
            {
                Catwalk(P(Mat.Grating).Mb, P(Mat.Steel).Mb, P(Mat.Rail).Mb, O, true, cy);
                Catwalk(P(Mat.Grating).Mb, P(Mat.Steel).Mb, P(Mat.Rail).Mb, O, false, cy);
                for (double x = x0 + 4.5; x < x1; x += SignsLayout.MaintPitch) maintHeads?.Add(Maint(P(Mat.Steel).Mb, P(Mat.MaintLens).Mb, O, V(x, cy - 0.32, z1 + 0.75)));
                for (double z = z0 + 4.5; z < z1; z += SignsLayout.MaintPitch) maintHeads?.Add(Maint(P(Mat.Steel).Mb, P(Mat.MaintLens).Mb, O, V(x1 + 0.75, cy - 0.32, z)));
            }
            double rw = 0.5 * SignsLayout.RiserW, rd = SignsLayout.RiserD;
            foreach (double z in SignsLayout.RiserWestZ) Box(P(Mat.Steel).Mb, O, x0 - rd, x0 + 0.05, f1 + 0.1, top - 0.6, z - rw, z + rw);
            foreach (double x in SignsLayout.RiserNorthX) Box(P(Mat.Steel).Mb, O, x - rw, x + rw, f1 + 0.1, top - 0.6, z1 - 0.05, z1 + rd);
            foreach (double z in SignsLayout.RiserEastZ) Box(P(Mat.Steel).Mb, O, x1 - 0.05, x1 + rd, f1 + 0.1, top - 0.6, z - rw, z + rw);
            return parts;
        }

        static Vector3 V(double x, double y, double z) => new Vector3((float)x, (float)y, (float)z);

        /// <summary>Axis-aligned box (world bounds), relative to origin.</summary>
        internal static void Box(MeshBuilder mb, Vector3 o, double x0, double x1, double y0, double y1, double z0, double z1)
        {
            Vector3 lo = V(x0, y0, z0) - o, hi = V(x1, y1, z1) - o;
            mb.Box(0.5f * (lo + hi), hi - lo);
        }

        /// <summary>Point on face f (0 west, 1 north, 2 east) at along-face coordinate c (z on west / east, x on north), `inward`
        /// metres inside the face line, height y.</summary>
        static Vector3 OnFace(int f, double c, double inward, double y)
        {
            if (f == 0) return V(SignsLayout.ArcX0 + inward, y, c);
            if (f == 1) return V(c, y, SignsLayout.ArcZ1 - inward);
            return V(SignsLayout.ArcX1 - inward, y, c);
        }

        static Vector3 Outward(int f) => f == 0 ? Vector3.left : (f == 1 ? Vector3.forward : Vector3.right);
        static Vector3 Along(int f) => f == 1 ? Vector3.right : Vector3.forward;

        static void Column(MeshBuilder mb, Vector3 o, int f, double c, double fy)
        {
            Vector3 p = OnFace(f, c, SignsLayout.ArcColInset, 0.5 * (fy + SignsLayout.ArcSoffit));
            float s = (float)SignsLayout.ArcCol;
            MeshKit.ChamferBox(mb, p - o, new Vector3(s, (float)(SignsLayout.ArcSoffit - fy + 0.04), s), 0.03f);
        }

        /// <summary>One shop of bay [ca, cb] on face f: the room glass (NocShopfront quad, UV0 panel metres, UV1 size), steel frame
        /// and centre mullion 2 cm proud, piers between shops (concrete).</summary>
        static void Shop(MeshBuilder room, MeshBuilder steel, MeshBuilder conc, Vector3 o, int f, double ca, double cb, double fy, Color32 state)
        {
            double r = SignsLayout.ArcRecess, pier = 0.5 * SignsLayout.ShopPier, fr = SignsLayout.ShopFrame;
            double y0 = fy + 0.15, y1 = SignsLayout.ShopY1;
            // the glass line runs along the core (x 918 - 986, z 236 - 356): the corner bays are cut back to it
            double lo = f == 1 ? SignsLayout.ArcX0 + r : SignsLayout.ArcZ0, hi = f == 1 ? SignsLayout.ArcX1 - r : SignsLayout.ArcZ1 - r;
            bool lastBay = cb >= hi - 1e-6;
            ca = Math.Max(ca, lo); cb = Math.Min(cb, hi);
            if (cb - ca < 3.0) return;
            double ga = ca + pier + fr, gb = cb - pier - fr;
            Vector3 n = Outward(f), t = Along(f);
            // glass 2 cm in front of the core face
            Vector3 A = OnFace(f, ga, r - 0.02, y0), B = OnFace(f, gb, r - 0.02, y0), C = OnFace(f, gb, r - 0.02, y1), D = OnFace(f, ga, r - 0.02, y1);
            Color32 keep = room.Color;
            room.Color = state;                                         // BV's NocShopfront panel state (G4Kit.PickShop)
            G4Kit.ShopQuad(room, A - o, B - o, C - o, D - o, n);
            room.Color = keep;
            // frame: sill, head, jambs, mullion (boxes from the face out 0.12 m)
            void FrameBox(double a, double b, double ya, double yb, double depthOut)
            {
                Vector3 p0 = OnFace(f, a, r + 0.03, ya), p1 = OnFace(f, b, r - depthOut, yb);
                Box(steel, o, Math.Min(p0.x, p1.x), Math.Max(p0.x, p1.x), ya, yb, Math.Min(p0.z, p1.z), Math.Max(p0.z, p1.z));
            }
            FrameBox(ga - fr, gb + fr, y0 - fr, y0, 0.12);
            FrameBox(ga - fr, gb + fr, y1, y1 + fr, 0.12);
            FrameBox(ga - fr, ga, y0, y1, 0.12);
            FrameBox(gb, gb + fr, y0, y1, 0.12);
            double gm = 0.5 * (ga + gb);
            FrameBox(gm - 0.05, gm + 0.05, y0, y1, 0.09);
            // pier at the bay's start (the end pier is the next bay's start; the last one closes the row)
            Vector3 q0 = OnFace(f, ca - pier, r + 0.02, fy - 0.01), q1 = OnFace(f, ca + pier, r - 0.18, SignsLayout.ArcSoffit - 0.05);
            Box(conc, o, Math.Min(q0.x, q1.x), Math.Max(q0.x, q1.x), q0.y, q1.y, Math.Min(q0.z, q1.z), Math.Max(q0.z, q1.z));
            if (lastBay)
            {
                q0 = OnFace(f, cb - pier, r + 0.02, fy - 0.01); q1 = OnFace(f, cb + pier, r - 0.18, SignsLayout.ArcSoffit - 0.05);
                Box(conc, o, Math.Min(q0.x, q1.x), Math.Max(q0.x, q1.x), q0.y, q1.y, Math.Min(q0.z, q1.z), Math.Max(q0.z, q1.z));
            }
            // the band above the glass (y 4.55 - 6.55): the core face; a steel sign rail across the bay
            FrameBox(ca + pier, cb - pier, y1 + 0.35, y1 + 0.5, 0.06);
        }

        /// <summary>The bay's hanging blade sign: perpendicular to the face, 0.6 x 1.9 m, at the bay centre just inside the face line.</summary>
        static Sign ShopSign(int f, double cm, int palette)
        {
            double w = SignsLayout.ShopSignW, h = SignsLayout.ShopSignH;
            Vector3 c = OnFace(f, cm, SignsLayout.ArcColInset + 0.25 + 0.5 * w, SignsLayout.ShopSignY1 - 0.5 * h);
            // the quad spans local x (w) and y (h) and faces local -z: on the west / east faces the sign plane is x-y (identity),
            // on the north face it is z-y (yaw 90)
            Quaternion q = f == 1 ? Quaternion.Euler(0f, 90f, 0f) : Quaternion.identity;
            return new Sign { centre = c, rotation = q, size = new Vector2((float)w, (float)h), palette = palette };
        }

        static void Downlight(MeshBuilder mb, Vector3 o, int f, double c)
        {
            double s = 0.5 * SignsLayout.DownlightSize;
            Vector3 p = OnFace(f, c, SignsLayout.DownlightIn, SignsLayout.ArcSoffit - 0.01);
            G4Kit.OrientedQuad(mb, p + V(-s, 0, -s) - o, p + V(s, 0, -s) - o, p + V(s, 0, s) - o, p + V(-s, 0, s) - o, Vector3.down);
        }

        /// <summary>A service catwalk along the north (z 360) or east (x 990) face at deck height y: grating 1.2 m deep, an edge
        /// angle, rails at 0.55 and 1.1 m on posts every 2 m (NocLine tubes), brackets every 4 m. The north one covers the corner.</summary>
        static void Catwalk(MeshBuilder grate, MeshBuilder steel, MeshBuilder rail, Vector3 o, bool north, double y)
        {
            double d = SignsLayout.CatwalkD;
            double a = north ? SignsLayout.ArcX0 : SignsLayout.ArcZ0, b = north ? SignsLayout.ArcX1 + d : SignsLayout.ArcZ1;
            Vector3 P(double along, double outw, double yy) => north ? V(along, yy, SignsLayout.ArcZ1 + outw) : V(SignsLayout.ArcX1 + outw, yy, along);
            G4Kit.OrientedQuad(grate, P(a, 0.02, y) - o, P(b, 0.02, y) - o, P(b, d, y) - o, P(a, d, y) - o, Vector3.up);
            Vector3 e0 = P(a, d - 0.04, y - 0.06), e1 = P(b, d - 0.04, y - 0.06);
            MeshKit.ChamferMember(steel, e0 - o, e1 - o, 0.08f, 0.14f, 0.01f, Vector3.up);
            Vector3 i0 = P(a, 0.06, y - 0.06), i1 = P(b, 0.06, y - 0.06);
            MeshKit.ChamferMember(steel, i0 - o, i1 - o, 0.08f, 0.14f, 0.01f, Vector3.up);
            foreach (double hr in new[] { 0.55, SignsLayout.RailH })
                MeshKit.Cylinder(rail, P(a, d - 0.04, y + hr) - o, P(b, d - 0.04, y + hr) - o, 0.025f, 6, false);
            for (double s = a + 0.5; s <= b - 0.3; s += SignsLayout.RailPost)
                MeshKit.Cylinder(rail, P(s, d - 0.04, y - 0.02) - o, P(s, d - 0.04, y + SignsLayout.RailH) - o, 0.022f, 6, false);
            for (double s = a + 2.0; s <= b - 1.0; s += 4.0)
                MeshKit.ChamferMember(steel, P(s, 0.05, y - 1.1) - o, P(s, d - 0.1, y - 0.1) - o, 0.07f, 0.07f, 0.01f, Vector3.up);
        }

        /// <summary>A maintenance light: a steel housing 0.4 x 0.18 x 0.25 m under the catwalk, its lens facing down; returns the lens centre.</summary>
        static Vector3 Maint(MeshBuilder steel, MeshBuilder lens, Vector3 o, Vector3 c)
        {
            MeshKit.ChamferBox(steel, c - o, new Vector3(0.4f, 0.18f, 0.25f), 0.02f);
            Vector3 l = c + new Vector3(0f, -0.095f, 0f);
            G4Kit.OrientedQuad(lens, l + new Vector3(-0.16f, 0f, -0.09f) - o, l + new Vector3(0.16f, 0f, -0.09f) - o,
                l + new Vector3(0.16f, 0f, 0.09f) - o, l + new Vector3(-0.16f, 0f, 0.09f) - o, Vector3.down);
            return l;
        }

        public static void Build(WorldContext ctx, Transform root)
        {
            var lib = ctx.Materials;
            var signs = new List<Sign>(48);
            var heads = new List<Vector3>(64);
            var parts = BuildParts(signs, heads);
            var mats = new Dictionary<Mat, Material>();
            long tris = 0;
            foreach (var p in parts)
            {
                var mr = G4Kit.Add(ctx, root, p.Name, p.Mb, Resolve(lib, p.Mat, mats), Origin);
                if (mr != null) tris += p.Mb.TriangleCount;
            }
            G4Kit.NeonSigns(lib, "Beat06_ArcadeSigns", signs);
            Color white = G4Kit.Lin(NocPal.SignWhite);
            NocHalos.Queue("ArcadeMaint", heads, G4Kit.Scale(white, SignsLayout.MaintGain), 8.0);
            ctx.Log(System.FormattableString.Invariant($"Beat06 arcade: x {SignsLayout.ArcX0}-{SignsLayout.ArcX1}, z {SignsLayout.ArcZ0}-{SignsLayout.ArcZ1}, {SignsLayout.ArcH} m; {parts.Count} parts, {tris} triangles, {signs.Count} shop signs, {heads.Count} maintenance lights"));
        }

        static Material Resolve(MaterialLibrary lib, Mat m, Dictionary<Mat, Material> cache)
        {
            if (lib == null) return null;
            if (cache.TryGetValue(m, out var cached)) return cached;
            Material r;
            switch (m)
            {
                case Mat.Stone: r = lib.StoneWall != null ? lib.StoneWall : lib.Concrete; break;
                case Mat.Concrete: r = lib.Concrete; break;
                case Mat.Steel: r = lib.SteelPainted; break;
                case Mat.Facade: r = lib.Facade; break;
                case Mat.Grating: r = lib.Grating != null ? lib.Grating : lib.SteelPainted; break;
                case Mat.Rail: r = G4Kit.Line(lib.Line, 0.05, G4Kit.Lin(NocPal.MetalPaint), Color.black, "Beat06_ArcadeRail"); break;
                case Mat.Downlight: r = G4Kit.Emissive(lib.SteelPainted, G4Kit.Lin(NocPal.WinCold), SignsLayout.DownlightGain, "Beat06_ArcadeDownlight"); break;
                case Mat.MaintLens: r = G4Kit.Emissive(lib.SteelPainted, G4Kit.Lin(NocPal.SignWhite), SignsLayout.MaintGain, "Beat06_ArcadeMaintLens"); break;
                case Mat.ShopCold: r = G4Kit.ShopRoom(lib, "Beat06_ShopCold", NocPal.WinCold, SignsLayout.ShopLight, 7.0f, 0.45f, 0.12f, 61); break;
                case Mat.ShopWhite: r = G4Kit.ShopRoom(lib, "Beat06_ShopWhite", NocPal.SignWhite, SignsLayout.ShopLight * 0.9, 9.0f, 0.40f, 0.10f, 62); break;
                case Mat.ShopNeutral: r = G4Kit.ShopRoom(lib, "Beat06_ShopNeutral", NocPal.WinNeutral, SignsLayout.ShopLight * 0.85, 6.0f, 0.50f, 0.14f, 63); break;
                case Mat.ShopBlue: r = G4Kit.ShopRoom(lib, "Beat06_ShopBlue", NocPal.WinBlue, SignsLayout.ShopLight * 0.8, 5.0f, 0.55f, 0.16f, 64); break;
                default: r = G4Kit.ShopRoom(lib, "Beat06_ShopClosed", NocPal.WinCold, SignsLayout.ShopClosed, 6.0f, 0.45f, 0.12f, 65); break;
            }
            cache[m] = r;
            return r;
        }
    }
}
