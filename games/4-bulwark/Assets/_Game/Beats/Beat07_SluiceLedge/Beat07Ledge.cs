// Beat07Ledge (LedgeBuilder + LanternBuilder, 14.5.7, 14.11): the near lee face, the ledge and the Keeper's Lantern.
//   Near lee face: the plane x = FaceX (3 mm west of the analytic plane of BwFarField, so nothing is coplanar with the graybox
//   face), z 1000..1800, y -120..0 (Part 7.5 / 14.4: geometry within 1800 m in z and 120 m in y; beyond it the analytic face of
//   BwFarField, whose shading follows BwConcrete's formwork panels, macro tone and lighting so the edges do not show), with the
//   exact mouth hole of the clipped tunnel and 0.12 m pockets for the mooring rings; corner AO bands where the slab meets it.
//   Ledge: slab x -12.4..FaceX, z 1415.2..1503, 0.8 m thick (built from quads so the slab top carries AO strips at the face and
//   the kerbs), a 0.12 m kerb on its three free edges, brackets every 6 m, posts 0.12 every 2 m on the kerbs, two round rails
//   0.10 (BwLine tubes, tops 1.10 and 0.55), mooring rings 0.3 m every 6 m in the face pockets (scale cues).
//   Lantern (-9.2, -90, 1499.0): base plate 0.4 with four bolts, iron post 1.4 (collar, shaft, capital), octagonal glass drum
//   0.30 x 0.34 (BwLantern, x6, the 14.2.5 flicker) between iron rings, eight BwLine cage bars and two cage hoops, an octagonal
//   roof and a carrying ring; a 5 m real light (2.0) through LocalLightPool, its gain = LanternFlicker.Gain(t) each frame.
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Megastructure.Beats
{
    public sealed partial class Beat07SluiceLedge
    {
        // ================================================================ face and ledge
        void BuildFaceAndLedge(WorldContext ctx, Transform root)
        {
            var face = new MeshBuilder();
            var slab = new MeshBuilder();
            var iron = new MeshBuilder();
            var posts = new MeshBuilder();
            var rails = new MeshBuilder();
            double F = Beat07Layout.FloorY, Cy = Beat07Layout.CeilingY, fx = Beat07Layout.FaceX;
            Beat07Layout.Mouth(out double mz0, out double mz1);
            double z0 = Beat07Layout.FaceZ0, z1 = Beat07Layout.FaceZ1, lz0 = Beat07Layout.LedgeZ0, lz1 = Beat07Layout.LedgeZ1;
            double under = F - Beat07Layout.LedgeThickness;
            Color32 ao = Ao(150), aoDeep = Ao(110);

            // ---- ring positions and pockets (z centres, every 6 m along the ledge, clear of the mouth)
            var rings = new List<double>();
            for (double z = Beat07Layout.RingZ0; z < lz1 - 3.0; z += Beat07Layout.RingPitch) if (z > mz1 + 1.0) rings.Add(z);
            double pY0 = Beat07Layout.RingY - 0.25, pY1 = Beat07Layout.RingY + 0.27, pHalf = 0.2;

            // ---- face bands (faces -x)
            FaceRect(face, z0, z1, Beat07Layout.FaceY0, under - 0.4, Open, Open);
            FaceRect(face, z0, lz0, under - 0.4, under, Open, Open);
            FaceRect(face, lz0, lz1, under - 0.4, under, Open, ao);          // just below the slab underside
            FaceRect(face, lz1, z1, under - 0.4, under, Open, Open);
            FaceRect(face, z0, lz0, under, F, Open, Open);
            FaceRect(face, lz1, z1, under, F, Open, Open);
            // band of the mouth (y F..Cy): left / right of the hole; sub-bands for the AO strip and the pockets
            double[] rows = { F, F + 0.4, pY0, pY1, Cy };
            for (int ri = 0; ri + 1 < rows.Length; ri++)
            {
                double ya = rows[ri], yb = rows[ri + 1];
                bool aoRow = ri == 0, pocketRow = ri == 2;
                // segments along z avoiding the mouth (and the pockets on the pocket row)
                var cuts = new List<(double a, double b)> { (z0, mz0), (mz1, z1) };
                if (pocketRow)
                {
                    var outCuts = new List<(double a, double b)>();
                    foreach (var c in cuts)
                    {
                        double a = c.a;
                        foreach (double zr in rings)
                        {
                            if (zr - pHalf <= a || zr + pHalf >= c.b) continue;
                            outCuts.Add((a, zr - pHalf));
                            a = zr + pHalf;
                        }
                        outCuts.Add((a, c.b));
                    }
                    cuts = outCuts;
                }
                foreach (var c in cuts)
                {
                    if (aoRow)
                    {
                        // AO only where the slab top meets the face
                        double sa = Math.Max(c.a, lz0), sb = Math.Min(c.b, lz1);
                        if (sa > c.a) FaceRect(face, c.a, sa, ya, yb, Open, Open);
                        if (sb > sa) FaceRect(face, sa, sb, ya, yb, ao, Open);
                        if (c.b > sb && sb >= sa) FaceRect(face, Math.Max(sb, c.a), c.b, ya, yb, Open, Open);
                    }
                    else FaceRect(face, c.a, c.b, ya, yb, Open, Open);
                }
            }
            FaceRect(face, z0, z1, Cy, Beat07Layout.FaceY1, Open, Open);

            // ---- pockets and mooring rings
            float px0 = (float)fx, px1 = (float)(fx + Beat07Layout.PocketDepth);
            foreach (double zr in rings)
            {
                float za = (float)(zr - pHalf), zb = (float)(zr + pHalf), ya = (float)pY0, yb = (float)pY1;
                // back (faces -x)
                Emit(face, new Vector3(px1, ya, zb), new Vector3(px1, yb, zb), new Vector3(px1, yb, za), new Vector3(px1, ya, za), aoDeep, aoDeep, aoDeep, aoDeep, false);
                // top (faces down), bottom (faces up)
                Emit(face, new Vector3(px0, yb, za), new Vector3(px1, yb, za), new Vector3(px1, yb, zb), new Vector3(px0, yb, zb), Open, aoDeep, aoDeep, Open, false);
                Emit(face, new Vector3(px0, ya, za), new Vector3(px0, ya, zb), new Vector3(px1, ya, zb), new Vector3(px1, ya, za), Open, Open, aoDeep, aoDeep, false);
                // sides: at za (faces +z), at zb (faces -z)
                Emit(face, new Vector3(px1, ya, za), new Vector3(px1, yb, za), new Vector3(px0, yb, za), new Vector3(px0, ya, za), aoDeep, aoDeep, Open, Open, false);
                Emit(face, new Vector3(px0, ya, zb), new Vector3(px0, yb, zb), new Vector3(px1, yb, zb), new Vector3(px1, ya, zb), Open, Open, aoDeep, aoDeep, false);
                // ring plate, eye and the 0.3 m ring hanging against the back
                Vector3 back = new Vector3(px1 - 0.01f, (float)(Beat07Layout.RingY + 0.17), (float)zr);
                MeshKit.ChamferBox(iron, back, new Vector3(0.02f, 0.09f, 0.12f), 0.004f);
                MeshKit.Torus(iron, back + new Vector3(-0.035f, -0.01f, 0f), Vector3.forward, 0.028f, 0.009f, 16, 6);
                float rr = 0.5f * (float)Beat07Layout.RingDiameter;
                MeshKit.Torus(iron, new Vector3(px1 - 0.06f, (float)Beat07Layout.RingY + 0.152f - rr, (float)zr), Vector3.right, rr - 0.012f, 0.012f, 36, 8);
            }

            // ---- slab: top with AO strips (kerbs on the three free edges are built separately), sides, underside
            double kx = Beat07Layout.KerbX1, kh = Beat07Layout.KerbH, kw = 0.23;
            double tz0 = lz0 + kw, tz1 = lz1 - kw;       // inner edges of the south / north kerbs
            double xa = Beat07Layout.LedgeX0, xb = fx;
            // slab top: x kx..xb, z tz0..tz1; AO bands 0.3 m along the kerbs and 0.4 m along the face (not under the mouth)
            {
                double[] xs = { kx, kx + 0.3, xb - 0.4, xb };
                byte[] xr = { 170, 255, 255, 150 };
                double[] zs = { tz0, tz0 + 0.3, mz0, mz1, tz1 - 0.3, tz1 };
                for (int i = 0; i + 1 < xs.Length; i++)
                    for (int j = 0; j + 1 < zs.Length; j++)
                    {
                        if (zs[j + 1] <= zs[j]) continue;
                        bool mouth = zs[j] >= mz0 - 1e-6 && zs[j + 1] <= mz1 + 1e-6;
                        byte ra = SlabAo(xr[i], zs[j], tz0, tz1, mouth && i == 2), rb = SlabAo(xr[i + 1], zs[j], tz0, tz1, mouth && i + 1 == 3);
                        byte rc = SlabAo(xr[i + 1], zs[j + 1], tz0, tz1, mouth && i + 1 == 3), rd = SlabAo(xr[i], zs[j + 1], tz0, tz1, mouth && i == 2);
                        // faces up: from above x right, z up: (x_i, z_j) -> (x_i, z_j+1) -> (x_i+1, z_j+1) -> (x_i+1, z_j)
                        Emit(slab, V(xs[i], F, zs[j]), V(xs[i], F, zs[j + 1]), V(xs[i + 1], F, zs[j + 1]), V(xs[i + 1], F, zs[j]), Ao(ra), Ao(rd), Ao(rc), Ao(rb), false);
                    }
            }
            // kerbs: drop side (x xa..kx, full z), south and north (kx..xb), 0.12 high, with a 2 cm chamfer on the inner top edge
            Kerb(slab, xa, kx, lz0, lz1, F, kh, 0);                     // inner face looks +x
            Kerb(slab, kx, xb, lz0, tz0, F, kh, 1);                     // inner face looks +z
            // north kerb split at the cabin door gap (ADDENDUM A1 00: the ledge ends at the elevator's top station), with end
            // faces at the gap and the slab top through it (the threshold to the cabin's bridge plate)
            double gx0 = Beat07Layout.DoorGapX0, gx1 = Beat07Layout.DoorGapX1;
            Kerb(slab, kx, gx0, tz1, lz1, F, kh, 2);                    // inner face looks -z
            Kerb(slab, gx1, xb, tz1, lz1, F, kh, 2);
            Emit(slab, V(gx0, F, tz1), V(gx0, F, lz1), V(gx1, F, lz1), V(gx1, F, tz1), Ao(235), Ao(235), Ao(235), Ao(235), false);
            Emit(slab, V(gx0, F, tz1), V(gx0, F + kh, tz1), V(gx0, F + kh, lz1), V(gx0, F, lz1), Ao(200), Open, Open, Ao(200), false);   // faces +x
            Emit(slab, V(gx1, F, lz1), V(gx1, F + kh, lz1), V(gx1, F + kh, tz1), V(gx1, F, tz1), Ao(200), Open, Open, Ao(200), false);   // faces -x
            // outer drop face (faces -x) up to the kerb top, south end (faces -z), north end (faces +z), underside (faces down)
            Emit(slab, V(xa, under, lz1), V(xa, F + kh, lz1), V(xa, F + kh, lz0), V(xa, under, lz0), false);
            Emit(slab, V(xa, under, lz0), V(xa, F + kh, lz0), V(xb, F + kh, lz0), V(xb, under, lz0), false);
            Emit(slab, V(xb, under, lz1), V(xb, F + kh, lz1), V(xa, F + kh, lz1), V(xa, under, lz1), false);
            Emit(slab, V(xa, under, lz0), V(xb, under, lz0), V(xb, under, lz1), V(xa, under, lz1), false);
            // brackets (corbels) under the slab every 6 m: sloped underside from 2.6 m deep at the face to 0.5 m at x -11.6
            for (double z = lz0 + 3.0; z < lz1 - 1.0; z += Beat07Layout.BracketPitch)
            {
                double za = z - 0.5 * Beat07Layout.BracketWidth, zb = z + 0.5 * Beat07Layout.BracketWidth, bx = Beat07Layout.BracketX1;
                double yFace = under - Beat07Layout.BracketDepthFace, yOut = under - 0.5;
                // outer end (faces -x)
                Emit(slab, V(bx, yOut, zb), V(bx, under, zb), V(bx, under, za), V(bx, yOut, za), false);
                // underside (faces down and west): from the outer end to the face
                Emit(slab, V(bx, yOut, za), V(xb, yFace, za), V(xb, yFace, zb), V(bx, yOut, zb), false);
                // sides (za faces -z, zb faces +z): quads with the sloped bottom edge
                Emit(slab, V(bx, yOut, za), V(bx, under, za), V(xb, under, za), V(xb, yFace, za), false);
                Emit(slab, V(xb, yFace, zb), V(xb, under, zb), V(bx, under, zb), V(bx, yOut, zb), false);
            }

            // ---- rails: drop side along z at RailX, south and north along x; posts every ~2 m on the kerbs
            float r = 0.5f * (float)Beat07Layout.RailDiameter;
            float yTop = (float)(F + Beat07Layout.RailTop) - r, yMid = (float)(F + Beat07Layout.RailMid) - r;
            float rx = (float)Beat07Layout.RailX, rzS = (float)Beat07Layout.RailZS, rzN = (float)Beat07Layout.RailZN, rxF = (float)(fx - 0.06);
            RailRun(rails, posts, new Vector3(rx, 0f, rzS), new Vector3(rx, 0f, rzN), yTop, yMid, (float)(F + kh), r);
            RailRun(rails, posts, new Vector3(rx, 0f, rzS), new Vector3(rxF, 0f, rzS), yTop, yMid, (float)(F + kh), r);
            // the north rail stops at both sides of the door gap (end posts there)
            RailRun(rails, posts, new Vector3(rx, 0f, rzN), new Vector3((float)gx0 - 0.08f, 0f, rzN), yTop, yMid, (float)(F + kh), r);
            RailRun(rails, posts, new Vector3((float)gx1 + 0.08f, 0f, rzN), new Vector3(rxF, 0f, rzN), yTop, yMid, (float)(F + kh), r);
            // wall plates where the end rails meet the face
            MeshKit.ChamferBox(posts, new Vector3((float)fx - 0.015f, yTop, rzS), new Vector3(0.03f, 0.16f, 0.16f), 0.006f);
            MeshKit.ChamferBox(posts, new Vector3((float)fx - 0.015f, yMid, rzS), new Vector3(0.03f, 0.16f, 0.16f), 0.006f);
            MeshKit.ChamferBox(posts, new Vector3((float)fx - 0.015f, yTop, rzN), new Vector3(0.03f, 0.16f, 0.16f), 0.006f);
            MeshKit.ChamferBox(posts, new Vector3((float)fx - 0.015f, yMid, rzN), new Vector3(0.03f, 0.16f, 0.16f), 0.006f);

            Flush(ctx, root, ref face, "b7_lee_face", m_face, false);
            Flush(ctx, root, ref slab, "b7_ledge_slab", m_slab != null ? m_slab : m_dark, false);
            Flush(ctx, root, ref iron, "b7_ledge_rings", m_iron, false);
            Flush(ctx, root, ref posts, "b7_ledge_posts", m_steel, false);
            Flush(ctx, root, ref rails, "b7_ledge_rails", m_rail, false);
        }

        static Vector3 V(double x, double y, double z) => new Vector3((float)x, (float)y, (float)z);

        /// <summary>AO of a slab-top vertex: the x band value, darkened toward the end kerbs; none under the mouth's face line.</summary>
        static byte SlabAo(byte xBand, double z, double tz0, double tz1, bool mouthFaceEdge)
        {
            double k = Math.Min(1.0, Math.Min(z - tz0, tz1 - z) / 0.3);
            double r = Math.Min(xBand, 170 + (255 - 170) * Math.Max(0.0, k));
            if (mouthFaceEdge) r = 255;
            return (byte)Math.Round(Math.Max(0.0, Math.Min(255.0, r)));
        }

        /// <summary>Face rectangle (faces -x) at FaceX, z a..b, y y0..y1, colours at the bottom and the top.</summary>
        static void FaceRect(MeshBuilder mb, double za, double zb, double y0, double y1, Color32 cBottom, Color32 cTop)
        {
            if (zb - za < 1e-4 || y1 - y0 < 1e-4) return;
            float x = (float)Beat07Layout.FaceX;
            Emit(mb, new Vector3(x, (float)y0, (float)zb), new Vector3(x, (float)y1, (float)zb), new Vector3(x, (float)y1, (float)za), new Vector3(x, (float)y0, (float)za),
                 cBottom, cTop, cTop, cBottom, false);
        }

        /// <summary>A kerb box on the slab top (x0..x1, z0..z1, height h above y) with a 2 cm chamfer on the inner top edge;
        /// inner = 0: the face toward +x, 1: toward +z, 2: toward -z (the outer faces are the slab's sides).</summary>
        static void Kerb(MeshBuilder mb, double x0, double x1, double z0, double z1, double y, double h, int inner)
        {
            const double ch = 0.02;
            double yt = y + h;
            Color32 wear = new Color32(255, 255, 0, 255), aoK = Ao(200);
            if (inner == 0)
            {
                // top (faces up) to the chamfer, chamfer (up and +x), inner face (+x)
                Emit(mb, V(x0, yt, z0), V(x0, yt, z1), V(x1 - ch, yt, z1), V(x1 - ch, yt, z0), false);
                Emit(mb, V(x1 - ch, yt, z0), V(x1 - ch, yt, z1), V(x1, yt - ch, z1), V(x1, yt - ch, z0), wear, wear, wear, wear, false);
                Emit(mb, V(x1, y, z0), V(x1, yt - ch, z0), V(x1, yt - ch, z1), V(x1, y, z1), aoK, Open, Open, aoK, false);
            }
            else if (inner == 1)
            {
                Emit(mb, V(x0, yt, z0), V(x0, yt, z1 - ch), V(x1, yt, z1 - ch), V(x1, yt, z0), false);
                Emit(mb, V(x0, yt, z1 - ch), V(x0, yt - ch, z1), V(x1, yt - ch, z1), V(x1, yt, z1 - ch), wear, wear, wear, wear, false);
                Emit(mb, V(x1, y, z1), V(x1, yt - ch, z1), V(x0, yt - ch, z1), V(x0, y, z1), aoK, Open, Open, aoK, false);
            }
            else
            {
                Emit(mb, V(x0, yt, z0 + ch), V(x0, yt, z1), V(x1, yt, z1), V(x1, yt, z0 + ch), false);
                Emit(mb, V(x0, yt - ch, z0), V(x0, yt, z0 + ch), V(x1, yt, z0 + ch), V(x1, yt - ch, z0), wear, wear, wear, wear, false);
                Emit(mb, V(x0, y, z0), V(x0, yt - ch, z0), V(x1, yt - ch, z0), V(x1, y, z0), aoK, Open, Open, aoK, false);
            }
        }

        /// <summary>A rail run a -> b (plan points, y ignored): rails at yTop / yMid (centres), posts 0.12 every ~2 m standing
        /// on yBase up to the top rail.</summary>
        static void RailRun(MeshBuilder rails, MeshBuilder posts, Vector3 a, Vector3 b, float yTop, float yMid, float yBase, float r)
        {
            Tube(rails, new Vector3(a.x, yTop, a.z), new Vector3(b.x, yTop, b.z), r, 12);
            Tube(rails, new Vector3(a.x, yMid, a.z), new Vector3(b.x, yMid, b.z), r, 12);
            float len = new Vector2(b.x - a.x, b.z - a.z).magnitude;
            int n = Mathf.Max(1, Mathf.RoundToInt(len / (float)Beat07Layout.PostPitch));
            float ps = (float)Beat07Layout.PostSize, top = yTop - 0.4f * r;
            for (int i = 0; i <= n; i++)
            {
                Vector3 p = Vector3.Lerp(a, b, i / (float)n);
                MeshKit.ChamferBox(posts, new Vector3(p.x, 0.5f * (yBase + top), p.z), new Vector3(ps, top - yBase, ps), 0.01f);
                // cap plate under the top rail
                MeshKit.ChamferBox(posts, new Vector3(p.x, top + 0.005f, p.z), new Vector3(ps + 0.02f, 0.012f, ps + 0.02f), 0.003f);
            }
        }

        // ================================================================ the Keeper's Lantern (14.11; moved to the lantern platform)
        /// <summary>The lantern's meshes with its base at (x, y, z) (iron, glass drum, cage bars); scale 1 = the 14.5.7 lantern.
        /// Used by Beat08Ending for the Keeper's Lantern and the platform lanterns (ADDENDUM A1 00).</summary>
        internal static void LanternMeshes(MeshBuilder iron, MeshBuilder glass, MeshBuilder cage, float x, float y, float z, float postHeight = -1f)
        {
            float hb = 0.5f * (float)Beat07Layout.LanternBase, post = postHeight > 0f ? postHeight : (float)Beat07Layout.LanternPost;
            // base plate 0.4 x 0.04 with four hex bolts
            MeshKit.ChamferBox(iron, new Vector3(x, y + 0.02f, z), new Vector3(2f * hb, 0.04f, 2f * hb), 0.008f);
            for (int i = 0; i < 4; i++)
            {
                float bx = (i & 1) == 0 ? -0.15f : 0.15f, bz = (i & 2) == 0 ? -0.15f : 0.15f;
                MeshKit.HexBolt(iron, new Vector3(x + bx, y + 0.04f, z + bz), Vector3.up, 0.035f, 15f * i);
            }
            // post: collar 0.14 x 0.16, tapered shaft 0.08 -> 0.06 (octagonal), capital 0.12 x 0.05
            Prism(iron, new Vector3(x, y + 0.04f, z), 0.065f, 0.06f, 0.16f, 8);
            Prism(iron, new Vector3(x, y + 0.2f, z), 0.04f, 0.03f, post - 0.27f, 8);
            Prism(iron, new Vector3(x, y + post - 0.07f, z), 0.035f, 0.06f, 0.05f, 8);
            Prism(iron, new Vector3(x, y + post - 0.02f, z), 0.06f, 0.06f, 0.02f, 8);
            // drum: base ring, glass (octagon 0.30 across flats, 0.34 tall), top ring, roof, carrying ring
            float r0 = 0.5f * (float)Beat07Layout.LanternGlass, yB = y + post;
            float ring = (float)Beat07Layout.DrumRing, dh = (float)Beat07Layout.DrumHeight, roof = (float)Beat07Layout.RoofHeight;
            Prism(iron, new Vector3(x, yB, z), r0 + 0.02f, r0 + 0.02f, ring, 8);
            Prism(glass, new Vector3(x, yB + ring, z), r0, r0, dh, 8, 22.5f, false, false);
            Prism(iron, new Vector3(x, yB + ring + dh, z), r0 + 0.02f, r0 + 0.02f, 0.025f, 8);
            Prism(iron, new Vector3(x, yB + ring + dh + 0.025f, z), r0 + 0.035f, 0.03f, roof, 8);
            MeshKit.Torus(iron, new Vector3(x, yB + ring + dh + 0.025f + roof + 0.03f, z), Vector3.right, 0.03f, 0.007f, 16, 6);
            // cage: eight bars at the octagon corners just outside the glass, two hoops (BwLine 0.02 m tubes)
            float rc = (r0 + 0.012f) / Mathf.Cos(Mathf.PI / 8f);
            float ya = yB + ring, yb = yB + ring + dh;
            for (int i = 0; i < 8; i++)
            {
                float a = (22.5f + 45f * i) * Mathf.Deg2Rad;
                var d = new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a)) * rc;
                Tube(cage, new Vector3(x, ya, z) + d, new Vector3(x, yb, z) + d, 0.01f, 6);
            }
            for (int h = 1; h <= 2; h++)
            {
                float yy = ya + dh * h / 3f;
                for (int i = 0; i < 8; i++)
                {
                    float a0 = (22.5f + 45f * i) * Mathf.Deg2Rad, a1 = (22.5f + 45f * (i + 1)) * Mathf.Deg2Rad;
                    Tube(cage, new Vector3(x + Mathf.Cos(a0) * rc, yy, z + Mathf.Sin(a0) * rc), new Vector3(x + Mathf.Cos(a1) * rc, yy, z + Mathf.Sin(a1) * rc), 0.01f, 6);
                }
            }
        }
    }
}
