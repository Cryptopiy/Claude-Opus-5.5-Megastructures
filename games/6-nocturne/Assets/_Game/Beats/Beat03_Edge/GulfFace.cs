// GulfFace: the two cut faces of the rift (MASTER_PROMPT 14.4, 14.5.3 GulfFace; Phase-3 graybox tier with the real NocFacade).
// Each face is a ruled surface with vertical rulings along its plan curve x_A(z) = 215.3 - F(|z - 195|) or x_B(z) = 815.3 + F / F2
// (EdgeDims), z -6 500..6 900, y -3 300..560, built per 400 m terrace cell as one chunk mesh (chunk-local origin at the cell
// centre: every vertex within ~1 km horizontally of it):
//   * the front surface in the NocFacade section mode (bays 9 x 4 m: the cut floors every 4 m read as slab lines), smooth normals
//     along the curve, u = arc length along the plan curve (continuous across chunks, so the bays never jump at a seam);
//   * the terraces: below y -360 the face steps back every 300 m by 30 + 90 hash(k, cell) m (cumulative; G2-D1), with the ledges
//     (roof mode) and the jog walls between cells whose steps differ;
//   * level 0 (y -360..560) is a closed mass (agreement 7): a back face (NocFacade facade mode) 60 m (A) / 98.7 m (B) behind the
//     front, the top cap at y 560 inside the cloud mix, end caps at z -6 500 and 6 900;
//   * face A above y 0 only outside the canyon z 120..270 (agreement 2: there the boulevard facades of G1 close the flanks, so the
//     flank ends carry no cap that would be co-planar with them); face B above y -0.5 cut away for the garden notch z 115..275
//     (agreement 5) with its two side walls (z 115: x_B..914; z 275: x_B..834, where the screen wall of G4 begins: G2-D3), the
//     notch floor of the mass at y -0.5 lies inside G3's terrace slab;
//   * the lower tube portal (y -142, z 209, r 5, agreement 4) on both faces as a 16 m panel with a round hole (GulfPortal adds
//     the collar and the tunnel). The upper tube and the ten boulevard lane portals fall into the canyon / notch openings of
//     agreements 2 and 5 and have no face to sit in: reported (G2-D4), not built.
// No co-planar faces: every quad lies on its own plane (fronts, backs, ledges at distinct heights, caps at 560 / -0.5).
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Megastructure
{
    public static class GulfFace
    {
        /// <summary>Base face ids (16-bit, NocFacade): level 0 of A / B, backs, receded levels + cell, jogs, walls.</summary>
        public const uint IdA = 0xA000u, IdB = 0xB000u, IdBack = 0x0800u, IdLevel = 0x0100u, IdJog = 0x0C00u, IdWall = 0x0E00u;
        /// <summary>Plan sampling: 20 m where the faces flare (chord sagitta <= 0.1 m), 100 m elsewhere.</summary>
        public const double StepCurve = 20.0, StepFlat = 100.0;

        public static double PortalBandLo => EdgeDims.LowerTubeY - EdgeDims.PortalPanelHalf;
        public static double PortalBandHi => EdgeDims.LowerTubeY + EdgeDims.PortalPanelHalf;
        public static double PortalZ0 => EdgeDims.LowerTubeZ - EdgeDims.PortalPanelHalf;
        public static double PortalZ1 => EdgeDims.LowerTubeZ + EdgeDims.PortalPanelHalf;

        /// <summary>Height where the level-0 face is cut for the canyon (A, y 0) or the notch (B, y -0.5).</summary>
        public static double GroundY(int face) => face == 0 ? 0.0 : EdgeDims.NotchFloorY;

        /// <summary>The sorted plan samples (z) of a face: ends, cell boundaries, flare breaks, canyon / notch edges, portal column,
        /// the back-face blend of B, refined to StepCurve inside the flares and the blend and to StepFlat elsewhere.</summary>
        public static List<double> Samples(int face)
        {
            var b = new List<double> { EdgeDims.FaceZ0, EdgeDims.FaceZ1 };
            for (double z = Math.Ceiling(EdgeDims.FaceZ0 / EdgeDims.TerraceCell) * EdgeDims.TerraceCell; z < EdgeDims.FaceZ1; z += EdgeDims.TerraceCell)
                if (z > EdgeDims.FaceZ0) b.Add(z);
            double c = EdgeDims.TongueZ;
            foreach (double d in new[] { -EdgeDims.FlareD1, -EdgeDims.FlareD0, EdgeDims.FlareD0, EdgeDims.FlareD1 }) b.Add(c + d);
            if (face == 1)
            {
                b.Add(c + EdgeDims.Flare2D0); b.Add(c + EdgeDims.Flare2D1); b.Add(EdgeDims.NotchZ0); b.Add(EdgeDims.NotchZ1); b.Add(EdgeDims.BackBBlendZ0); b.Add(EdgeDims.BackBBlendZ1);
                for (int k = 0; k < EdgeDims.BackBSteps; k++) b.Add(EdgeDims.BackBStepZ(k));     // GB-15 setbacks
            }
            else { b.Add(EdgeDims.CanyonZ0); b.Add(EdgeDims.CanyonZ1); }
            b.Add(PortalZ0); b.Add(PortalZ1);
            b.Sort();
            var outList = new List<double>();
            for (int i = 0; i < b.Count; i++)
            {
                if (outList.Count > 0 && b[i] - outList[outList.Count - 1] < 1e-6) continue;
                if (outList.Count > 0)
                {
                    double z0 = outList[outList.Count - 1], z1 = b[i];
                    double step = Curved(face, 0.5 * (z0 + z1)) ? StepCurve : StepFlat;
                    int n = Math.Max(1, (int)Math.Ceiling((z1 - z0) / step - 1e-9));
                    for (int k = 1; k < n; k++) outList.Add(z0 + (z1 - z0) * k / n);
                }
                outList.Add(b[i]);
            }
            return outList;
        }

        static bool Curved(int face, double z)
        {
            double d = z - EdgeDims.TongueZ, ad = Math.Abs(d);
            if (face == 1 && d > 0) return ad > EdgeDims.Flare2D0 && ad < EdgeDims.Flare2D1;
            return ad > EdgeDims.FlareD0 && ad < EdgeDims.FlareD1;
        }

        /// <summary>Unit front normal of face `face` at z (toward the rift).</summary>
        public static Vector3 FrontNormal(int face, double z)
        {
            double s = EdgeDims.RecedeSign(face), dx = EdgeDims.DXdZ(face, z);
            var n = new Vector3((float)(-s), 0f, (float)(s * dx));
            return n.normalized;
        }

        static Vector3 P(double x, double y, double z) => new Vector3((float)x, (float)y, (float)z);

        /// <summary>Builds the chunk meshes of face `face` (0 = A, 1 = B) into `parts` (EdgeMat.Facade); returns the chunk count.</summary>
        public static int Build(int face, List<EdgePart> parts)
        {
            List<double> zs = Samples(face);
            int n = zs.Count;
            var U = new double[n];
            for (int i = 0; i < n; i++) U[i] = EdgeDims.ArcLength(face, zs[i]);
            float uMin = (float)U[0], uMax = (float)U[n - 1];
            double sg = EdgeDims.RecedeSign(face);
            double gY = GroundY(face);
            uint id0 = face == 0 ? IdA : IdB;
            var chunks = new Dictionary<int, EdgePart>();
            string fname = face == 0 ? "FaceA" : "FaceB";
            double[] bands = { EdgeDims.TerraceFirstY, PortalBandLo, PortalBandHi, gY, EdgeDims.FaceTop };

            for (int i = 0; i < n - 1; i++)
            {
                double z0 = zs[i], z1 = zs[i + 1], zm = 0.5 * (z0 + z1);
                int cell = EdgeDims.CellOf(zm);
                var fw = Writer(chunks, parts, face, cell, fname);
                Vector3 n0 = FrontNormal(face, z0), n1 = FrontNormal(face, z1);
                double x0 = EdgeDims.FaceX(face, z0), x1 = EdgeDims.FaceX(face, z1);
                float u0 = (float)U[i], u1 = (float)U[i + 1];
                bool above = EdgeDims.AboveGround(face, zm);
                bool portalCol = z0 >= PortalZ0 - 1e-6 && z1 <= PortalZ1 + 1e-6;

                // level 0 front bands
                for (int bnd = 0; bnd < bands.Length - 1; bnd++)
                {
                    double ya = bands[bnd], yb = bands[bnd + 1];
                    if (ya >= gY - 1e-6 && !above) continue;
                    if (portalCol && bnd == 1) continue;                       // the portal panel (below)
                    fw.Quad(P(x0, ya, z0), P(x0, yb, z0), P(x1, yb, z1), P(x1, ya, z1), n0, n0, n1, n1, u0, u0, u1, u1,
                        id0, FacadeWriter.ModeSection, uMin, uMax, n0 + n1);
                }
                // receded levels and their ledges
                double rPrev = 0.0;
                for (int k = 1; k < EdgeDims.TerraceLevels; k++)
                {
                    double r = EdgeDims.Recession(face, k, cell);
                    double ya = EdgeDims.LevelBottom(k), yb = EdgeDims.LevelTop(k);
                    uint idk = LevelId(face, k, cell);
                    fw.Quad(P(x0 + sg * r, ya, z0), P(x0 + sg * r, yb, z0), P(x1 + sg * r, yb, z1), P(x1 + sg * r, ya, z1), n0, n0, n1, n1,
                        u0, u0, u1, u1, idk, FacadeWriter.ModeSection, uMin, uMax, n0 + n1);
                    fw.Flat(P(x0 + sg * rPrev, yb, z0), P(x0 + sg * r, yb, z0), P(x1 + sg * r, yb, z1), P(x1 + sg * rPrev, yb, z1), Vector3.up,
                        u0, u0, u1, u1, idk, FacadeWriter.ModeRoof, uMin, uMax);
                    rPrev = r;
                }
                // back face (level 0), top cap or notch floor
                double xb0 = EdgeDims.BackX(face, z0 + 1e-6), xb1 = EdgeDims.BackX(face, z1 - 1e-6);
                Vector3 backWant = new Vector3((float)sg, 0f, 0f);
                Vector3 nb = BackNormal(P(xb0, 0, z0), P(xb1, 0, z1), backWant);
                fw.Flat(P(xb0, EdgeDims.TerraceFirstY, z0), P(xb0, gY, z0), P(xb1, gY, z1), P(xb1, EdgeDims.TerraceFirstY, z1), nb,
                    u0, u0, u1, u1, (face == 0 ? IdA : IdB) | IdBack, FacadeWriter.ModeFacade, uMin, uMax);
                if (above)
                {
                    fw.Flat(P(xb0, gY, z0), P(xb0, EdgeDims.FaceTop, z0), P(xb1, EdgeDims.FaceTop, z1), P(xb1, gY, z1), nb,
                        u0, u0, u1, u1, (face == 0 ? IdA : IdB) | IdBack, FacadeWriter.ModeFacade, uMin, uMax);
                    fw.Flat(P(x0, EdgeDims.FaceTop, z0), P(xb0, EdgeDims.FaceTop, z0), P(xb1, EdgeDims.FaceTop, z1), P(x1, EdgeDims.FaceTop, z1), Vector3.up,
                        u0, u0, u1, u1, id0, FacadeWriter.ModeRoof, uMin, uMax);
                }
                else if (face == 1)
                {
                    fw.Flat(P(x0, gY, z0), P(xb0, gY, z0), P(xb1, gY, z1), P(x1, gY, z1), Vector3.up,
                        u0, u0, u1, u1, id0, FacadeWriter.ModeRoof, uMin, uMax);
                }
            }

            // jog walls between cells whose steps differ (levels >= 1)
            for (int i = 1; i < n - 1; i++)
            {
                double zb = zs[i];
                if (Math.Abs(zb / EdgeDims.TerraceCell - Math.Round(zb / EdgeDims.TerraceCell)) > 1e-9) continue;
                int cR = EdgeDims.CellOf(zb + 1e-6), cL = cR - 1;
                var fw = Writer(chunks, parts, face, cR, fname);
                double xb = EdgeDims.FaceX(face, zb);
                for (int k = 1; k < EdgeDims.TerraceLevels; k++)
                {
                    double rL = EdgeDims.Recession(face, k, cL), rR = EdgeDims.Recession(face, k, cR);
                    if (Math.Abs(rL - rR) < 1e-3) continue;
                    double ya = EdgeDims.LevelBottom(k), yb = EdgeDims.LevelTop(k);
                    Vector3 want = rL > rR ? Vector3.back : Vector3.forward;   // the jog faces the more receded (open) cell
                    fw.Flat(P(xb + sg * rL, ya, zb), P(xb + sg * rL, yb, zb), P(xb + sg * rR, yb, zb), P(xb + sg * rR, ya, zb), want,
                        (float)rL, (float)rL, (float)rR, (float)rR, (face == 0 ? IdA : IdB) | IdJog | (uint)k, FacadeWriter.ModeSection, 0f, 2000f);
                }
            }

            // end caps (z -6 500 facing -z, z 6 900 facing +z): level 0 between front and back, receded levels 60 m deep
            for (int e = 0; e < 2; e++)
            {
                double ze = e == 0 ? EdgeDims.FaceZ0 : EdgeDims.FaceZ1;
                int cell = EdgeDims.CellOf(e == 0 ? ze + 1e-6 : ze - 1e-6);
                var fw = Writer(chunks, parts, face, cell, fname);
                Vector3 want = e == 0 ? Vector3.back : Vector3.forward;
                double xf = EdgeDims.FaceX(face, ze), xbk = EdgeDims.BackX(face, ze);
                fw.Flat(P(xf, EdgeDims.TerraceFirstY, ze), P(xf, EdgeDims.FaceTop, ze), P(xbk, EdgeDims.FaceTop, ze), P(xbk, EdgeDims.TerraceFirstY, ze), want,
                    0f, 0f, (float)Math.Abs(xbk - xf), (float)Math.Abs(xbk - xf), id0 | IdWall, FacadeWriter.ModeSection, 0f, 2000f);
                for (int k = 1; k < EdgeDims.TerraceLevels; k++)
                {
                    double r = EdgeDims.Recession(face, k, cell), th = face == 0 ? EdgeDims.ThickA : EdgeDims.ThickB;
                    double xa = xf + sg * r, xz = xf + sg * (r + th);
                    fw.Flat(P(xa, EdgeDims.LevelBottom(k), ze), P(xa, EdgeDims.LevelTop(k), ze), P(xz, EdgeDims.LevelTop(k), ze), P(xz, EdgeDims.LevelBottom(k), ze), want,
                        0f, 0f, (float)th, (float)th, id0 | IdWall | (uint)k, FacadeWriter.ModeSection, 0f, 2000f);
                }
            }

            if (face == 1)
            {
                NotchWalls(Writer(chunks, parts, 1, EdgeDims.CellOf(EdgeDims.TongueZ), fname));
                BackSteps(Writer(chunks, parts, 1, EdgeDims.CellOf(EdgeDims.BackBStepZ(1)), fname));
            }
            // the lower tube portal panel (planar: the faces are planes for |z - 195| <= 300)
            PortalPanel(face, Writer(chunks, parts, face, EdgeDims.CellOf(EdgeDims.LowerTubeZ), fname), (float)EdgeDims.ArcLength(face, EdgeDims.LowerTubeZ), uMin, uMax);
            return chunks.Count;
        }

        static FacadeWriter Writer(Dictionary<int, EdgePart> chunks, List<EdgePart> parts, int face, int cell, string fname)
        {
            if (!chunks.TryGetValue(cell, out var part))
            {
                double zc = (cell + 0.5) * EdgeDims.TerraceCell;
                zc = Math.Max(EdgeDims.FaceZ0, Math.Min(EdgeDims.FaceZ1, zc));
                var origin = new Vector3((float)Math.Round(EdgeDims.FaceX(face, zc)), 0f, (float)zc);
                part = new EdgePart(FormattableString.Invariant($"{fname}_c{cell}"), EdgeMat.Facade, origin);
                chunks[cell] = part;
                parts.Add(part);
            }
            return new FacadeWriter(part.Mb, part.Origin);
        }

        public static uint LevelId(int face, int k, int cell) => ((face == 0 ? IdA : IdB) | IdLevel) + (uint)(k * 48 + ((cell + 24) & 31));

        static Vector3 BackNormal(Vector3 a, Vector3 b, Vector3 want)
        {
            Vector3 t = b - a; t.y = 0f;
            Vector3 n = new Vector3(t.z, 0f, -t.x).normalized;
            return Vector3.Dot(n, want) >= 0f ? n : -n;
        }

        /// <summary>The garden notch side walls of face B (agreement 5; G2-D3): z 115 (x_B..914, facing north into the notch) and
        /// z 275 (x_B..834, facing south), y -0.5..560, NocFacade section mode, u = x.</summary>
        static void NotchWalls(FacadeWriter fw)
        {
            double y0 = EdgeDims.NotchFloorY, y1 = EdgeDims.FaceTop;
            double za = EdgeDims.NotchZ0, zb = EdgeDims.NotchZ1;
            double xa = EdgeDims.XB(za), xa1 = EdgeDims.NotchX1, xb = EdgeDims.XB(zb), xb1 = EdgeDims.NotchNorthX1;
            fw.Flat(P(xa, y0, za), P(xa, y1, za), P(xa1, y1, za), P(xa1, y0, za), Vector3.forward,
                (float)xa, (float)xa, (float)xa1, (float)xa1, IdB | IdWall | 0x10u, FacadeWriter.ModeSection, (float)xa, (float)xa1);
            fw.Flat(P(xb, y0, zb), P(xb, y1, zb), P(xb1, y1, zb), P(xb1, y0, zb), Vector3.back,
                (float)xb, (float)xb, (float)xb1, (float)xb1, IdB | IdWall | 0x11u, FacadeWriter.ModeSection, (float)xb, (float)xb1);
            // the back jog of the mass at z 275 under the promenade (x 850..914, y -360..-0.5, facing north; closure, hidden by floors)
            double xn = EdgeDims.BackX(1, zb + 1e-6), xs = EdgeDims.BackX(1, zb - 1e-6);
            fw.Flat(P(xn, EdgeDims.TerraceFirstY, zb), P(xn, y0, zb), P(xs, y0, zb), P(xs, EdgeDims.TerraceFirstY, zb), Vector3.forward,
                (float)xn, (float)xn, (float)xs, (float)xs, IdB | IdWall | 0x12u, FacadeWriter.ModeFacade, (float)xn, (float)xs);
        }

        /// <summary>GB-15 (GE): the south-facing jog walls of face B's back between z 700 and 1 000 (x from the lower setback to the
        /// next, y -360..560, facade mode, u = x).</summary>
        static void BackSteps(FacadeWriter fw)
        {
            for (int k = 0; k < EdgeDims.BackBSteps; k++)
            {
                double z = EdgeDims.BackBStepZ(k), xa = EdgeDims.BackX(1, z - 1e-6), xb = EdgeDims.BackX(1, z + 1e-6);
                if (xb - xa < 1e-3) continue;
                fw.Flat(P(xa, EdgeDims.TerraceFirstY, z), P(xa, EdgeDims.FaceTop, z), P(xb, EdgeDims.FaceTop, z), P(xb, EdgeDims.TerraceFirstY, z), Vector3.back,
                    (float)xa, (float)xa, (float)xb, (float)xb, IdB | IdBack | (0x20u + (uint)k), FacadeWriter.ModeFacade, (float)xa, (float)xb);
            }
        }

        /// <summary>
        /// The portal panel: the 16 x 16 m square of the face around the lower tube (y -142, z 209) with a round hole of the collar's
        /// outer radius, fanned from the 4 corners (36 triangles; only the corners lie on the square's edges, so the neighbouring
        /// face quads meet it without T-junctions). Circle points at 32 equal angles, the corners at 45, 135, 225, 315 degrees.
        /// </summary>
        static void PortalPanel(int face, FacadeWriter fw, float uc, float uMin, float uMax)
        {
            double x = EdgeDims.FaceX(face, EdgeDims.LowerTubeZ), yc = EdgeDims.LowerTubeY, zc = EdgeDims.LowerTubeZ, h = EdgeDims.PortalPanelHalf;
            int N = EdgeDims.PortalSegments;
            Vector3 nrm = FrontNormal(face, zc);
            var mb = fw.Mb;
            mb.Color = new Color32(255, 0, FacadeWriter.ModeSection, 0);
            mb.CurrentUV1 = new Vector4(NocFacadeMath.SafeExtent(uMin), NocFacadeMath.SafeExtent(uMax), 0f, 0f);     // FA (TQ-09)
            float id = face == 0 ? IdA : IdB;
            int[] ring = new int[N];
            for (int j = 0; j < N; j++)
            {
                double a = 2.0 * Math.PI * j / N;
                Vector3 p = P(x, yc + EdgeDims.PortalOuter * Math.Sin(a), zc + EdgeDims.PortalOuter * Math.Cos(a));
                ring[j] = mb.Vertex(p - fw.Origin, nrm, new Vector2((float)(uc + EdgeDims.PortalOuter * Math.Cos(a)), id));
            }
            // corners C0 (+z, -y) at 315 deg, C1 (+z, +y) 45, C2 (-z, +y) 135, C3 (-z, -y) 225 (angle measured from +z toward +y)
            double[,] cz = { { 1, -1 }, { 1, 1 }, { -1, 1 }, { -1, -1 } };
            int[] corner = new int[4];
            for (int c = 0; c < 4; c++)
            {
                Vector3 p = P(x, yc + h * cz[c, 1], zc + h * cz[c, 0]);
                corner[c] = mb.Vertex(p - fw.Origin, nrm, new Vector2((float)(uc + h * cz[c, 0]), id));
            }
            int q = N / 4, half = N / 8;
            for (int e = 0; e < 4; e++)
            {
                int ca = corner[e], cb = corner[(e + 1) % 4];
                int jStart = (7 * half + e * q) % N;                // C0 sits at 315 deg = j 7N/8
                for (int t = 0; t < half; t++) TriW(mb, ca, ring[(jStart + t) % N], ring[(jStart + t + 1) % N], nrm);
                int jMid = (jStart + half) % N;
                TriW(mb, ca, ring[jMid], cb, nrm);
                for (int t = 0; t < half; t++) TriW(mb, cb, ring[(jMid + t) % N], ring[(jMid + t + 1) % N], nrm);
            }
        }

        static void TriW(MeshBuilder mb, int a, int b, int c, Vector3 want)
        {
            Vector3 pa = mb.Positions[a], pb = mb.Positions[b], pc = mb.Positions[c];
            if (Vector3.Dot(Vector3.Cross(pb - pa, pc - pa), want) >= 0f) mb.Tri(a, b, c);
            else mb.Tri(a, c, b);
        }
    }
}
