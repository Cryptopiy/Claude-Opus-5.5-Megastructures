// GulfRibBuilder: the FaceRib relief of the Gulf faces (TEXTURE_PLAN 5 item 3, CRIT0 #6, MASTER_PROMPT 14.5.3 "FaceRib pieces per
// face within |z - 195| < 1 200: slab cantilevers 3-25 m, pipe bundles, stair towers ..."; lead decision 3: approved). The cut
// faces of the rift read as flat walls; a real section carries what was cut and what was built onto it: rooms plugged into the
// cut, stair and lift shafts, balcony decks with their soffit lights, risers, and the terraces' built edges. Separate meshes on top
// of G2's GulfFace (no G2 file edited).
//   Every piece sits on FA's section grid (NocFacadeMath.CellOf(section): 9 m structural columns in u, 4 m floors, 88 m bands; u is
//   the arc length of the face's plan curve from z 195, y is world y): it starts and ends on column lines and floor lines
//   (ReliefTests.GulfRibs_OnTheSectionGrid, within 1 cm).
//   One hash per (face, level, band, column) picks the piece:
//     6 % pods: 1-3 bays x 2-5 floors, 4-12 m out, NocFacade section mode on the front and the two sides (the face's own lit
//         rooms), a concrete soffit and roof (GE-r01: dark concrete pieces read as black dashes on the lit section at 600 m; GE-r03:
//         facade-mode pods read as pale panels under the v2 skins);
//     3 % stair / lift towers: one bay, 10-40 floors, 6 m out, section mode front and sides, a concrete cap;
//     6 % balcony decks: 1-3 bays, 3-10 m out, a 0.5 m concrete slab with a dim soffit light line along its front edge;
//     2 % riser bundles (3-4 pipes r 0.8-1.1 on a column line, 40-200 m, plain brackets every 24 m), only within 500 m of the walk;
//   levels 0-2 (y -960..560); the terraces' overhang lips get a concrete edge beam (levels 1-5, |z - 195| < 2 000).
//   Never where the faces are open or carry other structure: the canyon mouth and the tongue's hull (face A), the garden notch and
//   the gate structures (face B), the lower tube portals, the cross bridges and the sight lines to them; never across a 400 m
//   terrace cell (the jog walls).
// Pieces are merged per face and material (8 draws). Plain boxes (12 triangles): chamfers are invisible at 100-1 200 m.
// AB (Phase 4 wave A, INT-01 "the lower faces fade into the haze"): lit structure runs down into the depths:
//   * the pieces continue below the deck to level 5 (y -1 860; 14.5.3 "900 FaceRib pieces per face within |z - 195| < 1 200");
//   * galleries: lit glazed corridors on a floor line (2.0 m tall (r03; 1.6 before), 0.4 m proud, 6-24 bays), one hash per (face, level, band,
//     24-column segment) at 65 % (r03; 50 % in r02, 30 % in r01), within |z - 195| < 2 000 so they run down the rift as horizontal light lines at every depth
//     (warm WinWarm or neutral WinNeutral, never neon);
//   * atria (14.5.3 "glowing atria R 12-30 m"): a lit multi-storey hall seen through the cut, 3-7 bays wide and 2R-3R tall, as
//     stacked emissive bands two floors high with the dark slab gaps between them (1.2 m), brighter at the bottom (the hall's floor
//     light), in a concrete surround (jambs 2 m, head and sill 2 m, 1.2 m proud); one hash per (face, level, band, 8-column segment)
//     at 4 %, levels 0-5, |z - 195| < 1 200; r03: steel curtain-wall mullions every 3 m in front of the hall (r02's plain glow read
//     as a flat beige card), a dimmer hall (AtriumLoGain 0.12) behind 0.6 m luminaire lines, and never across another piece;
//   * terrace lip lights: an emissive 0.6 m strip on the front of every terrace edge beam (levels 1-5), the long lines that converge
//     down the rift.
// Emissive pieces use NocSurface variants with a flat linear emission (ReliefBuilder.Resolve: GalleryWarm / GalleryCool /
// AtriumHi / AtriumLo / LipLight); all of them are light content (D-032 5: p99 by light, never by brighter windows).
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Megastructure
{
    public static class GulfRibBuilder
    {
        public const double Range = 1200.0, PipeRange = 500.0, EdgeRange = 2000.0;
        /// <summary>Half-length (z) of the arc-length table: the widest piece range (edges, galleries).</summary>
        public const double FrameRange = 2000.0;
        public const double ColP = 9.0, FloorH = 4.0, BandH = 88.0, SlabT = 0.5;
        public const double PPod = 0.06, PTower = 0.03, PDeck = 0.06, PPipe = 0.02;
        /// <summary>Levels with pieces (AB: 0..5, y -1 860..560; GE had 0..2).</summary>
        public const int Levels = 6;
        /// <summary>Galleries (AB): probability per 24-column segment (r02: 0.5; r01 0.3 left the faces below the deck nearly dark;
        /// r03: 0.65, D-032 5 light content: AB_r02 KS10 has 0.22 % of its pixels at luma >= 120 without the handrail), length 6-24
        /// bays, height (r03: 1.6 -> 2.0 m, a storey-high glazed corridor), proud, range |z - 195|.</summary>
        public const double PGallery = 0.75, GalleryH = 2.0, GalleryProud = 0.4, GalleryRange = 2000.0;   // r06: 0.65 -> 0.75 (KS07 / KS11 / KS12 p99)
        /// <summary>r04: probability of a second gallery in the same band and segment (another floor, >= 2.5 floors away) and the
        /// share of a gallery's inner bays left dark (AB_r03 KS12 p99 87.7: more corridors; uniform bars read as LED tubes).</summary>
        public const double PGallery2 = 0.55, GalleryDarkBay = 0.18;   // r06: 0.45 -> 0.55
        /// <summary>ARK (D-043): above the deck (bands from y 0 up) the faces carry more corridors - the first and second at PGalleryUp /
        /// PGallery2Up and a third at PGallery3Up: the layers that lit KS07 / KS08 / KS11 left the walker's level, and the Ark's dark
        /// mass reads as a silhouette against lit walls (every corridor it passes in front of goes out).</summary>
        public const double PGalleryUp = 0.92, PGallery2Up = 0.85, PGallery3Up = 0.6;
        public const int GallerySeg = 24, GalleryMin = 6;
        /// <summary>Atria (AB): probability per 8-column segment, radius 12-30 m, slab gap, surround width / proud, glow proud.</summary>
        public const double PAtrium = 0.04, AtriumRMin = 12.0, AtriumRMax = 30.0, AtriumGap = 1.2, AtriumFrame = 2.0, AtriumFrameProud = 1.2, AtriumGlowProud = 0.35;
        public const int AtriumSeg = 8, AtriumBandFloors = 2;
        /// <summary>Atrium floor-plate edge lights (AB r02): height (m), share lit (r03: 1.0 -> 0.6 m, a luminaire line, not a band).</summary>
        public const double AtriumLineH = 0.6, AtriumLineLit = 0.75;
        /// <summary>Atrium curtain-wall mullions (AB r03; AB_r02 KS07 / KS08: the atria read as flat beige cards): pitch, width (m),
        /// out of the face from / to (m; in front of the hall glow and the floor-plate lines).</summary>
        public const double AtriumMullionP = 3.0, AtriumMullionW = 0.35, AtriumMullionN0 = 0.45, AtriumMullionN1 = 1.0;
        /// <summary>Terrace lip light (AB): height of the strip on the edge beam's front, proud of the beam.</summary>
        public const double LipLightH = 0.6, LipLightProud = 0.05;
        /// <summary>Terrace edge beam run length (m; AB: 20 m instead of GE's 100 m, so the straight beams follow the flared faces
        /// within 0.1 m: sagitta 20^2 / 8 x the plan curvature, at most 1.9e-3 / m).</summary>
        public const double EdgeRun = 20.0;
        /// <summary>Beat root: beat 3, The Edge.</summary>
        public const int Beat = 2;
        /// <summary>Face ids of the pieces (16-bit): base | face << 8 | kind.</summary>
        public const uint IdBase = 0x6800u;

        /// <summary>Counts of the last BuildParts (tests and the log).</summary>
        public static int Pods, Towers, Decks, Pipes, Edges, Galleries, Atria;
        /// <summary>Every piece of the last build on the section grid: face, level, u span (arc length), y span, kind (ReliefTests: the
        /// edges lie on FA's cell boundaries, NocFacadeMath.CellOf(section)).</summary>
        public static readonly List<(int face, int level, double u0, double u1, double y0, double y1, char kind)> Pieces = new List<(int, int, double, double, double, double, char)>(4096);

        /// <summary>Arc length / plan position / frame of a face (2 m table of z(u)).</summary>
        sealed class FaceFrame
        {
            readonly int face;
            readonly double z0, step;
            readonly double[] u;
            public FaceFrame(int face)
            {
                this.face = face;
                z0 = EdgeDims.TongueZ - FrameRange - 40.0; step = 2.0;
                int n = (int)Math.Ceiling(2.0 * (FrameRange + 40.0) / step) + 1;
                u = new double[n];
                int mid = (int)Math.Round((EdgeDims.TongueZ - z0) / step);
                double a = 0.0;
                for (int i = mid + 1; i < n; i++) { a += Seg(i - 1); u[i] = a; }
                a = 0.0;
                for (int i = mid - 1; i >= 0; i--) { a -= Seg(i); u[i] = a; }
            }
            double Seg(int i)
            {
                double za = z0 + step * i, zb = za + step, xa = EdgeDims.FaceX(face, za), xb = EdgeDims.FaceX(face, zb);
                return Math.Sqrt((xb - xa) * (xb - xa) + step * step);
            }
            public double ZOfU(double uu)
            {
                int lo = 0, hi = u.Length - 1;
                if (uu <= u[lo]) return z0;
                if (uu >= u[hi]) return z0 + step * hi;
                while (hi - lo > 1) { int m = (lo + hi) >> 1; if (u[m] <= uu) lo = m; else hi = m; }
                return z0 + step * (lo + (uu - u[lo]) / (u[hi] - u[lo]));
            }
            /// <summary>The face point at arc length uu on level k (y 0), its outward normal (toward the rift) and tangent (+z).</summary>
            public void At(double uu, int level, out Vector3 p, out Vector3 n, out Vector3 t)
            {
                double z = ZOfU(uu), sg = EdgeDims.RecedeSign(face), dx = EdgeDims.DXdZ(face, z);
                double x = EdgeDims.FaceX(face, z) + sg * EdgeDims.Recession(face, level, EdgeDims.CellOf(z));
                p = new Vector3((float)x, 0f, (float)z);
                n = new Vector3((float)(-sg), 0f, (float)(sg * dx)).normalized;
                t = new Vector3((float)dx, 0f, 1f).normalized;
            }
        }

        sealed class Writers
        {
            public ReliefPart Fac, Conc, Steel, Light, GalWarm, GalCool, AtrHi, AtrLo, Lip;
        }

        public static void Build(List<ReliefPart> parts)
        {
            Pods = Towers = Decks = Pipes = Edges = Galleries = Atria = 0;
            Pieces.Clear();
            for (int f = 0; f < 2; f++) BuildFace(f, parts);
        }

        static Writers Make(int f, List<ReliefPart> parts)
        {
            // one part per face and material; every vertex lies within ~2.1 km of the origin (float error < 0.3 mm)
            var origin = new Vector3((float)Math.Round(EdgeDims.FaceX(f, EdgeDims.TongueZ)), -200f, (float)EdgeDims.TongueZ);
            string nm = f == 0 ? "GulfRibA" : "GulfRibB";
            var w = new Writers
            {
                Fac = new ReliefPart(nm + "_facade", ReliefMat.Facade, origin, Beat),
                Conc = new ReliefPart(nm + "_concrete", ReliefMat.Concrete, origin, Beat),
                Steel = new ReliefPart(nm + "_steel", ReliefMat.Steel, origin, Beat),
                Light = new ReliefPart(nm + "_light", ReliefMat.TunnelLight, origin, Beat),
                GalWarm = new ReliefPart(nm + "_galleryWarm", ReliefMat.GalleryWarm, origin, Beat),
                GalCool = new ReliefPart(nm + "_galleryCool", ReliefMat.GalleryCool, origin, Beat),
                AtrHi = new ReliefPart(nm + "_atriumHi", ReliefMat.AtriumHi, origin, Beat),
                AtrLo = new ReliefPart(nm + "_atriumLo", ReliefMat.AtriumLo, origin, Beat),
                Lip = new ReliefPart(nm + "_lipLight", ReliefMat.LipLight, origin, Beat),
            };
            parts.Add(w.Fac); parts.Add(w.Conc); parts.Add(w.Steel); parts.Add(w.Light);
            parts.Add(w.GalWarm); parts.Add(w.GalCool); parts.Add(w.AtrHi); parts.Add(w.AtrLo); parts.Add(w.Lip);
            return w;
        }

        static void BuildFace(int f, List<ReliefPart> parts)
        {
            var frame = new FaceFrame(f);
            var w = Make(f, parts);
            double uMin = EdgeDims.ArcLength(f, EdgeDims.TongueZ - Range), uMax = EdgeDims.ArcLength(f, EdgeDims.TongueZ + Range);
            int i0 = (int)Math.Ceiling(uMin / ColP), i1 = (int)Math.Floor(uMax / ColP);
            int floorsInBand = (int)Math.Round(BandH / FloorH);
            for (int k = 0; k < Levels; k++)
            {
                double yLo = EdgeDims.LevelBottom(k), yHi = EdgeDims.LevelTop(k);
                if (k == 0) yLo = Math.Max(yLo, EdgeDims.TerraceFirstY);
                int b0 = (int)Math.Floor(yLo / BandH), b1 = (int)Math.Ceiling(yHi / BandH) - 1;
                for (int b = b0; b <= b1; b++)
                    for (int i = i0; i <= i1; i++)
                    {
                        uint h = NocHash.H3(unchecked((uint)(i + 100000)), unchecked((uint)(b + 1000)), (uint)(f * 8 + k), 0x5249);
                        double r = NocHash.U01(h), r2 = NocHash.U01(NocHash.H3(h, 1u, 2u, 3)), r3 = NocHash.U01(NocHash.H3(h, 4u, 5u, 6));
                        double r4 = NocHash.U01(NocHash.H3(h, 7u, 8u, 9));
                        double yj = b * BandH + (int)(r3 * floorsInBand) * FloorH;          // a floor line of the band
                        double ua = i * ColP;
                        if (r < PPod)
                        {
                            int n = 1 + (int)(r2 * 3.0), fl = 2 + (int)(r4 * 4.0);
                            double ya = yj, yb = yj + fl * FloorH, d = 4.0 + 8.0 * NocHash.U01(NocHash.H3(h, 10u, 0u, 1));
                            if (ya < yLo + 2.0 || yb > yHi - 2.0 || !Clear(f, frame, ua, ua + n * ColP, ya, yb, d, k)) continue;
                            Pod(w, frame, f, k, ua, ua + n * ColP, ya, yb, d, (uint)(i * 31 + b));
                            Pieces.Add((f, k, ua, ua + n * ColP, ya, yb, 'p')); Pods++;
                        }
                        else if (r < PPod + PTower)
                        {
                            int fl = 10 + (int)(r2 * 31.0);
                            double ya = yj, yb = Math.Min(yHi - 2.0, ya + fl * FloorH);
                            yb = ya + Math.Floor((yb - ya) / FloorH) * FloorH;
                            if (ya < yLo + 2.0 || yb - ya < 40.0 || !Clear(f, frame, ua, ua + ColP, ya, yb + 1.0, 6.5, k)) continue;
                            Tower(w, frame, f, k, ua, ua + ColP, ya, yb, (uint)(i * 37 + b));
                            Pieces.Add((f, k, ua, ua + ColP, ya, yb, 't')); Towers++;
                        }
                        else if (r < PPod + PTower + PDeck)
                        {
                            int n = 1 + (int)(r2 * 3.0);
                            double d = 3.0 + 7.0 * r4;
                            if (yj < yLo + 2.0 || yj > yHi - 4.0 || !Clear(f, frame, ua, ua + n * ColP, yj - 1.0, yj + 1.5, d, k)) continue;
                            Deck(w, frame, k, ua, ua + n * ColP, yj, d);
                            Pieces.Add((f, k, ua, ua + n * ColP, yj, yj + SlabT, 's')); Decks++;
                        }
                        else if (r < PPod + PTower + PDeck + PPipe)
                        {
                            if (Math.Abs(frame.ZOfU(ua) - EdgeDims.TongueZ) > PipeRange) continue;
                            int fl = 10 + (int)(r2 * 41.0);
                            double ya = yj, yb = Math.Min(yHi - 2.0, ya + fl * FloorH);
                            if (ya < yLo + 2.0 || yb - ya < 40.0 || !Clear(f, frame, ua - 2.5, ua + 2.5, ya, yb, 4.0, k)) continue;
                            Risers(w, frame, k, ua, ya, yb, h);
                            Pieces.Add((f, k, ua, ua, ya, yb, 'r')); Pipes++;
                        }
                    }
            }
            Galleries_(frame, f, w);
            Atria_(frame, f, w);
            TerraceEdges(frame, f, w);
        }

        /// <summary>True when a piece over the column span [ua, ub] and heights [ya, yb], `depth` m out of the face at level k, stays
        /// clear of every opening and every other structure, inside one terrace cell and inside the face's z range.</summary>
        static bool Clear(int f, FaceFrame frame, double ua, double ub, double ya, double yb, double depth, int k, double range = Range)
        {
            double za = frame.ZOfU(ua), zb = frame.ZOfU(ub);
            if (Math.Abs(za - EdgeDims.TongueZ) > range + 10.0 || Math.Abs(zb - EdgeDims.TongueZ) > range + 10.0) return false;
            if (k >= 1 && EdgeDims.CellOf(za + 1e-3) != EdgeDims.CellOf(zb - 1e-3)) return false;
            double z0 = Math.Min(za, zb) - 6.0, z1 = Math.Max(za, zb) + 6.0;
            bool Hits(double zs0, double zs1, double ys0, double ys1) => z1 > zs0 && z0 < zs1 && yb > ys0 && ya < ys1;
            if (f == 0 && Hits(EdgeDims.CanyonZ0 - 30.0, EdgeDims.CanyonZ1 + 30.0, -140.0, 600.0)) return false;   // canyon mouth, bridge, hull
            if (f == 1 && Hits(EdgeDims.NotchZ0 - 30.0, EdgeDims.NotchZ1 + 30.0, -100.0, 600.0)) return false;     // notch, gate, terrace keel
            if (Hits(EdgeDims.LowerTubeZ - 20.0, EdgeDims.LowerTubeZ + 20.0, EdgeDims.LowerTubeY - 20.0, EdgeDims.LowerTubeY + 20.0)) return false;
            // the cross bridges and the sight lines from the Edge to their ends (KS09 / KS10: the bridge at z 850, y -180, whole)
            for (int i = 0; i < EdgeDims.BridgeZ.Length; i++)
                if (Hits(EdgeDims.BridgeZ[i] - 60.0, EdgeDims.BridgeZ[i] + 60.0, EdgeDims.BridgeY[i] - 50.0, EdgeDims.BridgeY[i] + 30.0)) return false;
            // ARK: the air crossings' portals (CrossBridge.AirZ / AirBase: 16 x 8 m opening, 3 m surround, the sill); a03: a +-40 m
            // margin cost KS11 1 187 px of corridors >= 120, so only the portal's own footprint (+ Hits' 6 m) stays clear
            for (int i = 0; i < CrossBridge.AirZ.Length; i++)
                if (Hits(CrossBridge.AirZ[i] - 11.0, CrossBridge.AirZ[i] + 11.0, CrossBridge.AirBase(i) - 4.0, CrossBridge.AirBase(i) + 12.0)) return false;
            return true;
        }

        /// <summary>True when a piece already built on face f (any level, any kind) overlaps the arc-length span [ua, ub] and the
        /// height span [ya, yb] (AB r03: atria keep clear of pods, towers, decks, risers and galleries).</summary>
        static bool Overlaps(int f, double ua, double ub, double ya, double yb)
        {
            foreach (var p in Pieces)
            {
                if (p.face != f) continue;
                double u0 = Math.Min(p.u0, p.u1) - (p.kind == 'r' ? 3.0 : 0.0), u1 = Math.Max(p.u0, p.u1) + (p.kind == 'r' ? 3.0 : 0.0);
                if (u1 > ua && u0 < ub && p.y1 > ya && p.y0 < yb) return true;
            }
            return false;
        }

        // ------------------------------------------------------------------ shapes
        /// <summary>The 8 corners of a box on the face: tangent span [ua, ub], y [ya, yb], out of the face [n0, n1]; frame at its centre.</summary>
        static Vector3[] Corners(FaceFrame frame, int k, double ua, double ub, double ya, double yb, double n0, double n1, out Vector3 nrm, out Vector3 tan)
        {
            double um = 0.5 * (ua + ub);
            frame.At(um, k, out Vector3 p, out nrm, out tan);
            float h = (float)(0.5 * (ub - ua));
            var c = new Vector3[8];
            int q = 0;
            for (int s = 0; s < 2; s++)             // s: tangent side (0 = -h, 1 = +h)
                for (int o = 0; o < 2; o++)         // o: out (0 = n0, 1 = n1)
                    for (int v = 0; v < 2; v++)     // v: y (0 = ya, 1 = yb)
                        c[q++] = p + tan * (s == 0 ? -h : h) + nrm * (float)(o == 0 ? n0 : n1) + Vector3.up * (float)(v == 0 ? ya : yb);
            return c;
        }

        static int Ix(int s, int o, int v) => s * 4 + o * 2 + v;

        /// <summary>An emissive strip on the face (AB): quads of at most `step` m along the face, every corner on the face's own plan
        /// curve (frame.At per corner) `proud` m out, so a long strip follows the flared faces exactly.</summary>
        static void Strip(ReliefPart part, FaceFrame frame, int k, double ua, double ub, double y0, double y1, double proud, double step = ColP)
        {
            int n = Math.Max(1, (int)Math.Ceiling((ub - ua) / step - 1e-6));
            var up = Vector3.up;
            for (int i = 0; i < n; i++)
            {
                double u0 = ua + (ub - ua) * i / n, u1 = ua + (ub - ua) * (i + 1) / n;
                frame.At(u0, k, out Vector3 p0, out Vector3 n0, out _);
                frame.At(u1, k, out Vector3 p1, out Vector3 n1, out _);
                float pr = (float)proud;
                Vector3 a = p0 + n0 * pr + up * (float)y0, b = p0 + n0 * pr + up * (float)y1;
                Vector3 c = p1 + n1 * pr + up * (float)y1, d = p1 + n1 * pr + up * (float)y0;
                EdgeMesh.Quad(part.Mb, part.Origin, a, b, c, d, n0, n0, n1, n1, n0 + n1);
            }
        }

        /// <summary>A plain box (12 triangles) on the face (positions relative to the part's origin).</summary>
        static void Box(ReliefPart part, FaceFrame frame, int k, double ua, double ub, double ya, double yb, double n0, double n1)
        {
            var c = Corners(frame, k, ua, ub, ya, yb, n0, n1, out Vector3 n, out Vector3 t);
            var mb = part.Mb; var o = part.Origin;
            EdgeMesh.FlatQuad(mb, o, c[Ix(0, 1, 0)], c[Ix(0, 1, 1)], c[Ix(1, 1, 1)], c[Ix(1, 1, 0)], n);          // front
            EdgeMesh.FlatQuad(mb, o, c[Ix(0, 0, 0)], c[Ix(0, 0, 1)], c[Ix(1, 0, 1)], c[Ix(1, 0, 0)], -n);         // back
            EdgeMesh.FlatQuad(mb, o, c[Ix(0, 0, 0)], c[Ix(0, 1, 0)], c[Ix(0, 1, 1)], c[Ix(0, 0, 1)], -t);         // side -
            EdgeMesh.FlatQuad(mb, o, c[Ix(1, 0, 0)], c[Ix(1, 1, 0)], c[Ix(1, 1, 1)], c[Ix(1, 0, 1)], t);          // side +
            EdgeMesh.FlatQuad(mb, o, c[Ix(0, 0, 1)], c[Ix(0, 1, 1)], c[Ix(1, 1, 1)], c[Ix(1, 0, 1)], Vector3.up);
            EdgeMesh.FlatQuad(mb, o, c[Ix(0, 0, 0)], c[Ix(0, 1, 0)], c[Ix(1, 1, 0)], c[Ix(1, 0, 0)], Vector3.down);
        }

        /// <summary>A pod: facade front and sides (u in metres along each face, ids per piece), concrete roof and soffit; 0.5 m into
        /// the face's mass (no face on the face plane).</summary>
        static void Pod(Writers w, FaceFrame frame, int f, int k, double ua, double ub, double ya, double yb, double d, uint seed)
        {
            var c = Corners(frame, k, ua, ub, ya, yb, -0.5, d, out Vector3 n, out Vector3 t);
            var fw = new FacadeWriter(w.Fac.Mb, w.Fac.Origin);
            uint id = IdBase | ((uint)f << 8) | (seed & 0x7Fu);
            float wdt = (float)(ub - ua), dd = (float)(d + 0.5);
            // front: u along the face; sides: u from the face outward
            // section mode, like the face itself (GE-r03 with -facade-v 2: facade-mode pods read as pale panels on the dark section)
            byte M = FacadeWriter.ModeSection;
            fw.Flat(c[Ix(0, 1, 0)], c[Ix(0, 1, 1)], c[Ix(1, 1, 1)], c[Ix(1, 1, 0)], n, 0f, 0f, wdt, wdt, id, M, 0f, wdt);
            fw.Flat(c[Ix(0, 0, 0)], c[Ix(0, 0, 1)], c[Ix(0, 1, 1)], c[Ix(0, 1, 0)], -t, 0f, 0f, dd, dd, id + 0x80u, M, 0f, dd);
            fw.Flat(c[Ix(1, 1, 0)], c[Ix(1, 1, 1)], c[Ix(1, 0, 1)], c[Ix(1, 0, 0)], t, 0f, 0f, dd, dd, id + 0x80u, M, 0f, dd);
            var mb = w.Conc.Mb; var o = w.Conc.Origin;
            EdgeMesh.FlatQuad(mb, o, c[Ix(0, 0, 1)], c[Ix(0, 1, 1)], c[Ix(1, 1, 1)], c[Ix(1, 0, 1)], Vector3.up);
            EdgeMesh.FlatQuad(mb, o, c[Ix(0, 0, 0)], c[Ix(0, 1, 0)], c[Ix(1, 1, 0)], c[Ix(1, 0, 0)], Vector3.down);
        }

        /// <summary>A stair / lift tower over one bay: facade front and sides, a concrete cap 0.6 m with a 0.25 m overhang.</summary>
        static void Tower(Writers w, FaceFrame frame, int f, int k, double ua, double ub, double ya, double yb, uint seed)
        {
            Pod(w, frame, f, k, ua + 0.3, ub - 0.3, ya, yb, 6.0, seed ^ 0x55u);
            Box(w.Conc, frame, k, ua + 0.05, ub - 0.05, yb - 0.02, yb + 0.6, -0.5, 6.25);
        }

        /// <summary>A balcony deck: the floor's slab band carried d m out over whole bays (0.5 m into the mass), a dim light line under
        /// its front edge (0.25 m wide, 0.4 m back from the edge).</summary>
        static void Deck(Writers w, FaceFrame frame, int k, double ua, double ub, double y, double d)
        {
            Box(w.Conc, frame, k, ua, ub, y, y + SlabT, -0.5, d);
            var c = Corners(frame, k, ua + 0.3, ub - 0.3, y - 0.01, y - 0.01, d - 0.65, d - 0.4, out Vector3 n, out Vector3 t);
            EdgeMesh.FlatQuad(w.Light.Mb, w.Light.Origin, c[Ix(0, 0, 0)], c[Ix(0, 1, 0)], c[Ix(1, 1, 0)], c[Ix(1, 0, 0)], Vector3.down);
        }

        /// <summary>3-4 risers r 0.8-1.1 m on a column line, 1.8 m out, from ya to yb; plain brackets every 24 m.</summary>
        static void Risers(Writers w, FaceFrame frame, int k, double u, double ya, double yb, uint h)
        {
            int n = 3 + (int)(NocHash.U01(NocHash.H3(h, 11u, 0u, 0)) * 2.0);
            double r = 0.8 + 0.3 * NocHash.U01(NocHash.H3(h, 12u, 0u, 0)), gap = 2.0 * r + 0.3;
            var mb = w.Steel.Mb;
            for (int p = 0; p < n; p++)
            {
                double up = u + (p - 0.5 * (n - 1)) * gap;
                frame.At(up, k, out Vector3 pt, out Vector3 nn, out Vector3 t);
                Vector3 c = pt + nn * (float)(1.0 + r) - w.Steel.Origin;
                MeshKit.Cylinder(mb, new Vector3(c.x, (float)ya, c.z), new Vector3(c.x, (float)yb, c.z), (float)r, 8, false);
            }
            double half = 0.5 * (n - 1) * gap + r + 0.3;
            for (double y = ya + 4.0; y < yb - 1.0; y += 24.0) Box(w.Steel, frame, k, u - half, u + half, y, y + 0.5, -0.3, 1.6 + 2.0 * r);
        }

        /// <summary>Galleries (AB): one hash per (face, level, band, GallerySeg-column segment); a lit band GalleryH tall on a floor
        /// line (above the slab), GalleryProud out of the face, 6-24 whole bays, warm or neutral.</summary>
        static void Galleries_(FaceFrame frame, int f, Writers w)
        {
            double uMin = EdgeDims.ArcLength(f, EdgeDims.TongueZ - GalleryRange), uMax = EdgeDims.ArcLength(f, EdgeDims.TongueZ + GalleryRange);
            int s0 = (int)Math.Ceiling(uMin / (ColP * GallerySeg)), s1 = (int)Math.Floor(uMax / (ColP * GallerySeg)) - 1;
            int floorsInBand = (int)Math.Round(BandH / FloorH);
            for (int k = 0; k < Levels; k++)
            {
                double yLo = EdgeDims.LevelBottom(k), yHi = EdgeDims.LevelTop(k);
                if (k == 0) yLo = Math.Max(yLo, EdgeDims.TerraceFirstY);
                int b0 = (int)Math.Floor(yLo / BandH), b1 = (int)Math.Ceiling(yHi / BandH) - 1;
                for (int b = b0; b <= b1; b++)
                    for (int sg = s0; sg <= s1; sg++)
                    {
                        uint h = NocHash.H3(unchecked((uint)(sg + 50000)), unchecked((uint)(b + 3000)), (uint)(f * 8 + k), 0x47414C);
                        bool up = b * BandH >= 0.0;                                    // ARK: bands above the deck
                        if (NocHash.U01(h) >= (up ? PGalleryUp : PGallery)) continue;
                        double r2 = NocHash.U01(NocHash.H3(h, 1u, 0u, 0)), r3 = NocHash.U01(NocHash.H3(h, 2u, 0u, 0));
                        double r4 = NocHash.U01(NocHash.H3(h, 3u, 0u, 0)), r5 = NocHash.U01(NocHash.H3(h, 4u, 0u, 0));
                        int n = Math.Min(GallerySeg, GalleryMin + (int)(r2 * (GallerySeg - GalleryMin + 1)));
                        int start = (int)(r3 * (GallerySeg - n + 1));
                        double ua = (sg * GallerySeg + start) * ColP, ub = ua + n * ColP;
                        double yj = b * BandH + (1 + (int)(r4 * (floorsInBand - 2))) * FloorH;
                        Gallery(frame, f, k, w, h, ua, ub, yj, yLo, yHi, r5);
                        // r04 (D-032 5, AB_r03 KS12 p99 87.7): a second corridor on another floor of the same band and segment
                        uint h2 = NocHash.H3(h, 7u, 0u, 0);
                        if (NocHash.U01(h2) >= (up ? PGallery2Up : PGallery2)) continue;
                        double r6 = NocHash.U01(NocHash.H3(h2, 1u, 0u, 0)), r7 = NocHash.U01(NocHash.H3(h2, 2u, 0u, 0)), r8 = NocHash.U01(NocHash.H3(h2, 3u, 0u, 0));
                        int n2 = Math.Min(GallerySeg, GalleryMin + (int)(r6 * (GallerySeg - GalleryMin + 1)));
                        double ua2 = (sg * GallerySeg + (int)(r7 * (GallerySeg - n2 + 1))) * ColP, ub2 = ua2 + n2 * ColP;
                        double yj2 = b * BandH + (1 + (int)(r8 * (floorsInBand - 2))) * FloorH;
                        if (Math.Abs(yj2 - yj) < 2.5 * FloorH) continue;
                        Gallery(frame, f, k, w, h2, ua2, ub2, yj2, yLo, yHi, 1.0 - r5);
                        // ARK: a third corridor above the deck, on a floor >= 2.5 floors from both
                        if (!up) continue;
                        uint h3 = NocHash.H3(h, 11u, 0u, 0);
                        if (NocHash.U01(h3) >= PGallery3Up) continue;
                        double r9 = NocHash.U01(NocHash.H3(h3, 1u, 0u, 0)), r10 = NocHash.U01(NocHash.H3(h3, 2u, 0u, 0)), r11 = NocHash.U01(NocHash.H3(h3, 3u, 0u, 0));
                        int n3 = Math.Min(GallerySeg, GalleryMin + (int)(r9 * (GallerySeg - GalleryMin + 1)));
                        double ua3 = (sg * GallerySeg + (int)(r10 * (GallerySeg - n3 + 1))) * ColP, ub3 = ua3 + n3 * ColP;
                        double yj3 = b * BandH + (1 + (int)(r11 * (floorsInBand - 2))) * FloorH;
                        if (Math.Abs(yj3 - yj) < 2.5 * FloorH || Math.Abs(yj3 - yj2) < 2.5 * FloorH) continue;
                        Gallery(frame, f, k, w, h3, ua3, ub3, yj3, yLo, yHi, r9);
                    }
            }
        }

        /// <summary>One gallery: a lit glazed corridor GalleryH tall on the floor line yj over [ua, ub] (whole bays), warm when r &lt; 0.6;
        /// r04: its bays are lit in runs, a bay dark with GalleryDarkBay (AB_r03: uniform bars read as LED tubes, not corridors; a dark
        /// bay is 9 m, >= 4 px at 2 km, so the breaks never alias).</summary>
        static void Gallery(FaceFrame frame, int f, int k, Writers w, uint h, double ua, double ub, double yj, double yLo, double yHi, double r)
        {
            if (yj < yLo + 2.0 || yj + FloorH > yHi - 2.0 || !Clear(f, frame, ua, ub, yj, yj + FloorH, GalleryProud, k, GalleryRange)) return;   // (no Overlaps: AB_r04 lost most galleries to it)
            var part = r < 0.6 ? w.GalWarm : w.GalCool;
            int n = (int)Math.Round((ub - ua) / ColP);
            int run = -1;
            for (int j = 0; j <= n; j++)
            {
                bool lit = j < n && (j == 0 || j == n - 1 || NocHash.U01(NocHash.H3(h, 40u + (uint)j, 0u, 0)) >= GalleryDarkBay);
                if (lit && run < 0) run = j;
                if (!lit && run >= 0)
                {
                    Strip(part, frame, k, ua + run * ColP, ua + j * ColP, yj + SlabT + 0.2, yj + SlabT + 0.2 + GalleryH, GalleryProud);
                    run = -1;
                }
            }
            Pieces.Add((f, k, ua, ub, yj, yj + FloorH, 'g')); Galleries++;
        }

        /// <summary>Atria (AB, 14.5.3 "glowing atria R 12-30 m"): one hash per (face, level, band, AtriumSeg-column segment); width
        /// 2R in whole bays (3-7), height 2R-3R in whole double floors: a dim warm hall (AtriumLo over the whole opening) with the
        /// lit edges of its floor plates in front of it (AtriumHi, AtriumLineH tall every AtriumBandFloors floors, 75 % of them lit;
        /// r01: full bright bands read as flat beige cards with blinds), in a concrete surround.</summary>
        static void Atria_(FaceFrame frame, int f, Writers w)
        {
            double uMin = EdgeDims.ArcLength(f, EdgeDims.TongueZ - Range), uMax = EdgeDims.ArcLength(f, EdgeDims.TongueZ + Range);
            int s0 = (int)Math.Ceiling(uMin / (ColP * AtriumSeg)), s1 = (int)Math.Floor(uMax / (ColP * AtriumSeg)) - 1;
            int floorsInBand = (int)Math.Round(BandH / FloorH);
            double band = AtriumBandFloors * FloorH;
            for (int k = 0; k < Levels; k++)
            {
                double yLo = EdgeDims.LevelBottom(k), yHi = EdgeDims.LevelTop(k);
                if (k == 0) yLo = Math.Max(yLo, EdgeDims.TerraceFirstY);
                int b0 = (int)Math.Floor(yLo / BandH), b1 = (int)Math.Ceiling(yHi / BandH) - 1;
                for (int b = b0; b <= b1; b++)
                    for (int sg = s0; sg <= s1; sg++)
                    {
                        uint h = NocHash.H3(unchecked((uint)(sg + 70000)), unchecked((uint)(b + 5000)), (uint)(f * 8 + k), 0x415452);
                        if (NocHash.U01(h) >= PAtrium) continue;
                        double R = AtriumRMin + (AtriumRMax - AtriumRMin) * NocHash.U01(NocHash.H3(h, 1u, 0u, 0));
                        int nb = Math.Max(3, Math.Min(7, (int)Math.Round(2.0 * R / ColP)));
                        int bands = Math.Max(2, (int)Math.Round((2.0 + NocHash.U01(NocHash.H3(h, 2u, 0u, 0))) * R / band));
                        int start = (int)(NocHash.U01(NocHash.H3(h, 3u, 0u, 0)) * (AtriumSeg - nb + 1));
                        double ua = (sg * AtriumSeg + start) * ColP, ub = ua + nb * ColP;
                        double ya = b * BandH + (int)(NocHash.U01(NocHash.H3(h, 4u, 0u, 0)) * floorsInBand) * FloorH, yb = ya + bands * band;
                        double fr = AtriumFrame;
                        if (ya - fr < yLo + 2.0 || yb + fr > yHi - 2.0 || !Clear(f, frame, ua - fr, ub + fr, ya - fr, yb + fr, AtriumFrameProud, k)) continue;
                        // r03 (AB_r02 KS08: a riser bundle ran down through an atrium): never across another piece of this face
                        if (Overlaps(f, ua - fr - 6.0, ub + fr + 6.0, ya - fr - 2.0, yb + fr + 2.0)) continue;
                        // surround: jambs (outside the bays), head, sill
                        Box(w.Conc, frame, k, ua - fr, ua, ya - fr, yb + fr, -0.5, AtriumFrameProud);
                        Box(w.Conc, frame, k, ub, ub + fr, ya - fr, yb + fr, -0.5, AtriumFrameProud);
                        Box(w.Conc, frame, k, ua + 0.02, ub - 0.02, yb, yb + fr, -0.5, AtriumFrameProud);
                        Box(w.Conc, frame, k, ua + 0.02, ub - 0.02, ya - fr, ya, -0.5, AtriumFrameProud);
                        // the hall behind (dim) and the lit floor-plate edges in front of it
                        Strip(w.AtrLo, frame, k, ua, ub, ya, yb, AtriumGlowProud);
                        for (int j = 1; j < bands; j++)
                        {
                            if (NocHash.U01(NocHash.H3(h, 20u + (uint)j, 0u, 0)) > AtriumLineLit) continue;
                            double y0 = ya + j * band - 0.5 * AtriumLineH;
                            Strip(w.AtrHi, frame, k, ua, ub, y0, y0 + AtriumLineH, AtriumGlowProud + 0.25);
                        }
                        // r03: the curtain wall's mullions in front of the hall and its floor-plate lines (steel, full height)
                        for (double um = ua + AtriumMullionP; um < ub - 0.5 * AtriumMullionP; um += AtriumMullionP)
                            Box(w.Steel, frame, k, um - 0.5 * AtriumMullionW, um + 0.5 * AtriumMullionW, ya, yb, AtriumMullionN0, AtriumMullionN1);
                        Pieces.Add((f, k, ua, ub, ya, yb, 'a')); Atria++;
                    }
            }
        }

        /// <summary>A concrete edge beam (1.5 m tall, 0.8 m deep, 0.2 m out) along the lip of every terrace overhang within |z - 195|
        /// &lt; 2 000, levels 1-5 (y -360 .. -1 560; deeper ones lie in the haze): the face steps back going down (14.5.3), so the lip is
        /// the bottom edge of the upper level's front.</summary>
        static void TerraceEdges(FaceFrame frame, int f, Writers w)
        {
            for (int k = 1; k <= 5; k++)
            {
                double y = EdgeDims.LevelTop(k);
                double za = EdgeDims.TongueZ - EdgeRange, zb = EdgeDims.TongueZ + EdgeRange;
                for (double zc0 = Math.Floor(za / EdgeDims.TerraceCell) * EdgeDims.TerraceCell; zc0 < zb; zc0 += EdgeDims.TerraceCell)
                {
                    double z0 = Math.Max(za, zc0) + 1.0, z1 = Math.Min(zb, zc0 + EdgeDims.TerraceCell) - 1.0;
                    if (z1 <= z0) continue;
                    double ua = EdgeDims.ArcLength(f, z0), ub = EdgeDims.ArcLength(f, z1);
                    int m = Math.Max(1, (int)Math.Ceiling((ub - ua) / EdgeRun));
                    for (int s = 0; s < m; s++)
                    {
                        double u0 = ua + (ub - ua) * s / m + (s == 0 ? 0.0 : 0.02), u1 = ua + (ub - ua) * (s + 1) / m;
                        Box(w.Conc, frame, k - 1, u0, u1, y - 0.1, y + 1.4, -0.6, 0.2);
                        // AB: the lip light, a 0.6 m emissive strip on the beam's front (0.05 m proud), 0.3 m in from each run end
                        Strip(w.Lip, frame, k - 1, u0 + 0.3, u1 - 0.3, y + 0.2, y + 0.2 + LipLightH, 0.2 + LipLightProud, EdgeRun);
                    }
                    Edges++;
                }
            }
        }
    }
}
