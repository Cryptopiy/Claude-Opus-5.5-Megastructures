// Beat07Tunnel (SluiceBuilder, 14.5.7): the sluice tunnel from the hall end wall (s 1373) to the lee face and the conduit
// behind its east grating panel. See Beat07SluiceLedge.cs and Beat07Layout.cs for the numbers.
//   Tunnel shell (lane coordinates on BwPath, knots 0.5 m on A3, 1.0 m on the diagonal, 0.25 m at the mouth; every piece
//   clipped to x >= FaceX so the mouth is the exact oblique cut): the wet floor (ConcreteWet: 14.5.7 "wet floor",
//   the cages mirrored in its film) with AO bands along the walls; walls and soffit in ConcreteDark (formwork, corner AO bands in
//   vertex R); the east side is the grating panel 2.8 m tall between a 0.15 m wall cap and a 0.6 m lintel, its frame posts
//   every 2.0 m BEHIND the grating plane (outside the capsule reach: the lane edge + 0.35 is the panel plane).
//   SodiumWarm cages every 6 m in the soffit at lateral -0.55 (housing, glass, BwLine guard bars; LocalLightPool anchors 7 m,
//   1.6); a cable tray and a pipe on the west wall above 2.3 m (above the capsule's 1.9 m reach).
//   Conduit (BwConduit, conduit coordinates u / v / y): water surface (grid displaced in the shader: standing cross waves,
//   roll waves, advected streaks; it plunges into the drop shaft from u 37.5), far wall with the cold lights painted in the
//   shader, soffit, the tunnel-side wall faces below the floor and above the panel, the culvert head wall and culvert
//   (the water emerges from the dark), the drop shaft and its trash rack, the solid end wall at the panel's north end.
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Megastructure.Beats
{
    public sealed partial class Beat07SluiceLedge
    {
        const double MouthFineS0 = 1412.0;
        /// <summary>Last s with tunnel pieces east of the face (the east wall, lateral +1.6, crosses it near s 1427.8).</summary>
        const double TunnelS1 = 1428.4;

        static List<double> TunnelKnots()
        {
            var k = new List<double>(256);
            double s0 = Beat07Layout.TunnelS0, a3 = BwPath.A3EndS;
            for (double s = s0; s < a3; s += 0.5) k.Add(s);
            for (double s = a3; s < MouthFineS0; s += 1.0) k.Add(s);
            for (double s = MouthFineS0; s < TunnelS1; s += 0.25) k.Add(s);
            k.Add(TunnelS1);
            k.Add(Beat07Layout.PanelS0); k.Add(Beat07Layout.PanelS1);
            for (double s = Math.Ceiling(Beat07Layout.PanelS0 / Beat07Layout.DeckBearerPitch) * Beat07Layout.DeckBearerPitch; s < TunnelS1; s += Beat07Layout.DeckBearerPitch) k.Add(s);
            k.Sort();
            var o = new List<double>(k.Count);
            foreach (double s in k) if (o.Count == 0 || s - o[o.Count - 1] > 1e-4) o.Add(s);
            return o;
        }

        // ================================================================ tunnel shell, panel, lamps
        void BuildTunnel(WorldContext ctx, Transform root)
        {
            var knots = TunnelKnots();
            var shell = new MeshBuilder();     // walls, soffit, lintel (ConcreteDark)
            var trough = new MeshBuilder();    // the wet floor (ConcreteWet)
            var panel = new MeshBuilder();     // east grating panel (wall grating)
            var steel = new MeshBuilder();     // bearers, panel frame, lamp housings, tray, pipe
            double W = Beat07Layout.WallLateral, F = Beat07Layout.FloorY, Cy = Beat07Layout.CeilingY, Tb = F - Beat07Layout.TroughDepth;
            double pt = Beat07Layout.PanelTopY;
            const double band = 0.35;          // corner AO band height (m)
            Color32 aoCorner = Ao(150), aoTrough = Ao(120);

            for (int i = 0; i + 1 < knots.Count; i++)
            {
                double a = knots[i], b = knots[i + 1];
                bool clip = b > MouthFineS0 - 1e-6;
                // the wet floor (14.5.7 "wet floor": ConcreteWet, the cages mirrored in its film), AO bands along both walls
                Emit(trough, P(a, -W, F), P(b, -W, F), P(b, -W + band, F), P(a, -W + band, F), aoCorner, aoCorner, Open, Open, clip);
                Emit(trough, P(a, -W + band, F), P(b, -W + band, F), P(b, W - band, F), P(a, W - band, F), clip);
                Emit(trough, P(a, W - band, F), P(b, W - band, F), P(b, W, F), P(a, W, F), Open, Open, aoCorner, aoCorner, clip);
                // soffit: AO bands along both walls
                Ceiling(shell, a, b, -W, -W + band, Cy, aoCorner, Open, clip);
                Ceiling(shell, a, b, -W + band, W - band, Cy, Open, Open, clip);
                Ceiling(shell, a, b, W - band, W, Cy, Open, aoCorner, clip);
                // west wall (faces +lateral): trough part, floor band, middle, soffit band
                Side(shell, a, b, -W, Tb, F, true, aoTrough, aoCorner, clip);
                Side(shell, a, b, -W, F, F + band, true, aoCorner, Open, clip);
                Side(shell, a, b, -W, F + band, Cy - band, true, Open, Open, clip);
                Side(shell, a, b, -W, Cy - band, Cy, true, Open, aoCorner, clip);
                // east side (faces -lateral)
                bool inPanel = a >= Beat07Layout.PanelS0 - 1e-6 && b <= Beat07Layout.PanelS1 + 1e-6;
                Side(shell, a, b, W, Tb, F, false, aoTrough, aoCorner, clip);
                if (inPanel)
                {
                    Emit(panel, P(a, W, F), P(b, W, F), P(b, W, pt), P(a, W, pt), false);
                    Side(shell, a, b, W, pt, Cy, false, Open, aoCorner, clip);
                }
                else
                {
                    Side(shell, a, b, W, F, F + band, false, aoCorner, Open, clip);
                    Side(shell, a, b, W, F + band, Cy - band, false, Open, Open, clip);
                    Side(shell, a, b, W, Cy - band, Cy, false, Open, aoCorner, clip);
                }
            }


            // panel frame behind the grating plane (lateral 1.62 .. 1.74): posts every 2.0 m, top and bottom members
            double pv = W + 0.08;
            for (double u = 0.0; u <= Beat07Layout.PanelLength + 1e-6; u += Beat07Layout.PanelModule)
            {
                double uu = Math.Min(u, Beat07Layout.PanelLength - 0.04);
                Vector3 bot = C(uu + 0.03, pv, F), top = C(uu + 0.03, pv, pt);
                MeshKit.ChamferMember(steel, bot, top, 0.06f, 0.12f, 0.006f, AxisV());
            }
            MeshKit.ChamferMember(steel, C(0.0, pv, pt - 0.05), C(Beat07Layout.PanelLength, pv, pt - 0.05), 0.12f, 0.1f, 0.006f, Vector3.up);
            MeshKit.ChamferMember(steel, C(0.0, pv, F + 0.05), C(Beat07Layout.PanelLength, pv, F + 0.05), 0.12f, 0.1f, 0.006f, Vector3.up);

            // cable tray (0.3 x 0.08 at 2.75 m) and a pipe (0.16 at 2.35 m) on the west wall, both above the capsule reach
            {
                double s0 = Beat07Layout.TunnelS0 + 1.0, s1 = BulwarkConst.TunnelMouthS - 6.0;
                var trayPts = new List<Vector3>();
                for (double s = s0; s <= s1 + 1e-6; s += 1.0) trayPts.Add(P(s, -W + 0.15, F + 2.75));
                for (int i = 0; i + 1 < trayPts.Count; i++) MeshKit.ChamferMember(steel, trayPts[i], trayPts[i + 1], 0.3f, 0.06f, 0.008f, Vector3.up);
                for (double s = s0 + 0.5; s < s1; s += 1.5)
                {
                    // tray brackets into the wall
                    MeshKit.ChamferMember(steel, P(s, -W, F + 2.68), P(s, -W + 0.3, F + 2.68), 0.04f, 0.04f, 0.004f, Vector3.up);
                }
            }
            var pipe = new MeshBuilder();
            {
                double s0 = Beat07Layout.TunnelS0 + 1.0, s1 = BulwarkConst.TunnelMouthS - 6.0;
                Vector3 prev = P(s0, -W + 0.12, F + 2.35);
                for (double s = s0 + 1.0; s <= s1 + 1e-6; s += 1.0)
                {
                    Vector3 p = P(s, -W + 0.12, F + 2.35);
                    Tube(pipe, prev, p, 0.08f, 12);
                    prev = p;
                }
                for (double s = s0 + 1.0; s < s1; s += 3.0)
                    MeshKit.ChamferMember(steel, P(s, -W, F + 2.35), P(s, -W + 0.2, F + 2.35), 0.05f, 0.18f, 0.005f, Vector3.up);
            }

            // SodiumWarm cages every 6 m in the soffit: housing (steel), glass (LampSodium), guard bars (BwLine), light anchors
            var glass = new MeshBuilder();
            var guard = new MeshBuilder();
            for (int k = 0; k < Beat07Layout.LampCount; k++)
            {
                double s = Beat07Layout.LampS(k);
                float hd = (float)BwPath.HeadingDeg(s);
                var rot = Quaternion.Euler(0f, hd, 0f);
                Vector3 c = P(s, Beat07Layout.LampLateral, Cy);
                MeshKit.ChamferBox(steel, c + Vector3.down * 0.06f, new Vector3(0.34f, 0.12f, 0.66f), 0.012f, rot);
                MeshKit.ChamferBox(glass, c + Vector3.down * 0.15f, new Vector3(0.24f, 0.07f, 0.52f), 0.01f, rot);
                Vector3 fw = rot * Vector3.forward, rt = rot * Vector3.right;
                float gy = -0.215f;
                for (int j = -1; j <= 1; j++)
                    Tube(guard, c + Vector3.up * gy + rt * (0.1f * j) - fw * 0.3f, c + Vector3.up * gy + rt * (0.1f * j) + fw * 0.3f, 0.01f, 6);
                for (int j = -1; j <= 1; j += 2)
                    Tube(guard, c + Vector3.up * gy + fw * (0.22f * j) - rt * 0.15f, c + Vector3.up * gy + fw * (0.22f * j) + rt * 0.15f, 0.01f, 6);
                if (m_pool != null)
                    m_pool.Add(c + Vector3.down * 0.45f, BulwarkPalette.Linear(Pal.SodiumWarm), (float)Beat07Layout.LampRange, (float)Beat07Layout.LampIntensityAt(k), "beat7");
            }

            // the hall end wall at s 1373 (14.6 "hall end wall at s 1373, door left into the tunnel"; the graybox map gives it to
            // beat 7): its hall face across the whole hall section (west of the hall's west wall to beyond the glass plane, the
            // trench bottom to the hall ceiling) with the 3.2 x 3.4 doorway; the doorway's reveal is the tunnel shell from s 1373;
            // a steel door frame on the hall face, outside the doorway (lateral >= 1.6: beyond the funnel lane's capsule reach)
            {
                var endw = new MeshBuilder();
                double s = Beat07Layout.TunnelS0, y = F, top = y + BulwarkConst.HallHeight, bot = y - BulwarkConst.HallTrenchDepth - 0.5;
                double l0 = -9.0, l1 = BulwarkConst.GlassAxisOffset + 2.0;
                CrossFace(endw, s, l0, l1, Cy, top);                  // above the door
                CrossFace(endw, s, l0, -W, bot, Cy);                  // west of the door
                CrossFace(endw, s, W, l1, bot, Cy);                   // east of the door
                CrossFace(endw, s, -W, W, bot, F - Beat07Layout.TroughDepth);   // below the trough
                Flush(ctx, root, ref endw, "b7_hall_end_wall", m_endWall != null ? m_endWall : m_dark, false);
                // door frame: jambs and lintel 0.18 wide, 0.08 proud of the hall face
                float hd = (float)BwPath.HeadingDeg(s);
                var rot = Quaternion.Euler(0f, hd, 0f);
                Vector3 back = rot * Vector3.back;
                for (int side = -1; side <= 1; side += 2)
                {
                    Vector3 jb = P(s, side * (W + 0.09), F), jt = P(s, side * (W + 0.09), Cy + 0.18);
                    MeshKit.ChamferMember(steel, jb + back * 0.04f, jt + back * 0.04f, 0.18f, 0.08f, 0.008f, back);
                }
                MeshKit.ChamferMember(steel, P(s, -W - 0.18, Cy + 0.09) + back * 0.04f, P(s, W + 0.18, Cy + 0.09) + back * 0.04f, 0.18f, 0.08f, 0.008f, back);
            }

            var hallRails = new MeshBuilder();
            BuildHallEnd(ctx, root, hallRails);
            Flush(ctx, root, ref hallRails, "b7_hall_end_rails", m_rail, false);

            Flush(ctx, root, ref shell, "b7_tunnel_shell", m_dark, false);
            Flush(ctx, root, ref trough, "b7_tunnel_floor", m_wet, false);
            Flush(ctx, root, ref panel, "b7_grating_panel", m_gratingWall != null ? m_gratingWall : m_grating, false);
            Flush(ctx, root, ref steel, "b7_tunnel_steel", m_steel, false);
            Flush(ctx, root, ref pipe, "b7_tunnel_pipe", m_rail, false);
            Flush(ctx, root, ref glass, "b7_lamp_sodium", m_sodium, false);
            Flush(ctx, root, ref guard, "b7_lamp_guards", m_cageLine, false);
        }

        // ================================================================ the last 3 m of the hall (s 1370 .. 1373)
        // The graybox map gives the lane-driven strips of s >= 1370 to beat 7, so the hall's last 3 m (from the A3 start to the
        // end wall) are built here with the graybox hall's section and materials, so the window-gallery stand-in runs on to the
        // end wall without a gap: floor to the trench edge (axis + 3.0), the west wall on the funnel lane's capsule envelope
        // (+ 0.35), the rail at axis + 3.06 (tops 1.10 / 0.55), the 8 m trench, the pier face at axis + 6.0, the 28 m soffit.
        void BuildHallEnd(WorldContext ctx, Transform root, MeshBuilder rails)
        {
            var hall = new MeshBuilder();
            double s0 = BulwarkConst.GalleryEndS, s1 = Beat07Layout.TunnelS0, rl = BulwarkConst.HallRail, gl = BulwarkConst.GlassAxisOffset;
            double td = BulwarkConst.HallTrenchDepth, hh = BulwarkConst.HallHeight;
            float rr = 0.5f * (float)Beat07Layout.RailDiameter;
            for (double a = s0; a < s1 - 1e-6; a += 0.5)
            {
                double b = Math.Min(s1, a + 0.5);
                double ya = BwPath.FloorY(a), yb = BwPath.FloorY(b);
                LaneEnvelope(a, out double eloA, out _);
                LaneEnvelope(b, out double eloB, out _);
                BwPath.Lane(a, out double loA, out _);
                BwPath.Lane(b, out double loB, out _);
                double wlA = eloA - BulwarkConst.PlayerR, wlB = eloB - BulwarkConst.PlayerR;
                double ca = ya + hh, cb = yb + hh;
                // floor (faces up) from the lane edge - 0.75 to the trench edge
                Emit(hall, P(a, loA - 0.75, ya), P(b, loB - 0.75, yb), P(b, rl, yb), P(a, rl, ya), false);
                // west wall (faces +lateral) from 0.5 m below the floor to the soffit
                Emit(hall, P(a, wlA, ya - 0.5), P(a, wlA, ca), P(b, wlB, cb), P(b, wlB, yb - 0.5), false);
                // trench: its inner wall (faces +lateral), floor, the pier face at the glass plane (faces -lateral)
                Emit(hall, P(a, rl, ya - td), P(a, rl, ya), P(b, rl, yb), P(b, rl, yb - td), false);
                Emit(hall, P(a, rl, ya - td), P(b, rl, yb - td), P(b, gl, yb - td), P(a, gl, ya - td), false);
                Emit(hall, P(a, gl, ya - td), P(b, gl, yb - td), P(b, gl, cb), P(a, gl, ca), false);
                // soffit (faces down)
                Emit(hall, P(a, wlA, ca), P(a, gl, ca), P(b, gl, cb), P(b, wlB, cb), false);
                // rail (0.10 round tubes) on the trench edge
                double rx = rl + 0.06;
                Tube(rails, P(a, rx, ya + Beat07Layout.RailTop - rr), P(b, rx, yb + Beat07Layout.RailTop - rr), rr, 12);
                Tube(rails, P(a, rx, ya + Beat07Layout.RailMid - rr), P(b, rx, yb + Beat07Layout.RailMid - rr), rr, 12);
            }
            Flush(ctx, root, ref hall, "b7_hall_end", m_endWall != null ? m_endWall : m_dark, false);
        }

        /// <summary>Lane limits widened to the capsule's footprint (min / max of BwPath.Lane over s +- 0.36), the graybox rule.</summary>
        static void LaneEnvelope(double s, out double lo, out double hi)
        {
            BwPath.Lane(s, out lo, out hi);
            double r = BulwarkConst.PlayerR + 0.01;
            for (int k = -8; k <= 8; k++)
            {
                if (k == 0) continue;
                BwPath.Lane(s + r * k / 8.0, out double l, out double h);
                if (l < lo) lo = l;
                if (h > hi) hi = h;
            }
        }

        static Vector3 AxisV()
        {
            BwPath.Right(Beat07Layout.PanelS0, out double rx, out double rz);
            return new Vector3((float)rx, 0f, (float)rz);
        }

        /// <summary>Vertical quad along s at lateral lat, y0..y1; faceRight: the face looks to +lateral.</summary>
        static void Side(MeshBuilder mb, double a, double b, double lat, double y0, double y1, bool faceRight, Color32 c0, Color32 c1, bool clip)
        {
            if (faceRight) Emit(mb, P(a, lat, y0), P(a, lat, y1), P(b, lat, y1), P(b, lat, y0), c0, c1, c1, c0, clip);
            else Emit(mb, P(a, lat, y0), P(b, lat, y0), P(b, lat, y1), P(a, lat, y1), c0, c0, c1, c1, clip);
        }

        /// <summary>Vertical quad across the path at s facing back along -s (toward the hall), lateral l0..l1, y y0..y1.</summary>
        static void CrossFace(MeshBuilder mb, double s, double l0, double l1, double y0, double y1)
        {
            if (l1 - l0 < 1e-4 || y1 - y0 < 1e-4) return;
            Emit(mb, P(s, l0, y0), P(s, l0, y1), P(s, l1, y1), P(s, l1, y0), false);
        }

        /// <summary>Downward-facing quad (soffit) between laterals l0 < l1 at height y; colours at l0 and l1.</summary>
        static void Ceiling(MeshBuilder mb, double a, double b, double l0, double l1, double y, Color32 c0, Color32 c1, bool clip)
            => Emit(mb, P(a, l0, y), P(a, l1, y), P(b, l1, y), P(b, l0, y), c0, c1, c1, c0, clip);

        // ================================================================ conduit
        void BuildConduit(WorldContext ctx, Transform root)
        {
            var wall = new MeshBuilder();     // BwConduit walls
            var water = new MeshBuilder();    // BwConduit water
            var steel = new MeshBuilder();    // trash rack
            double v0 = Beat07Layout.ConduitV0, vi = v0 + 0.15, v1 = Beat07Layout.ConduitV1;
            double yF = Beat07Layout.ConduitFloorY, yC = Beat07Layout.ConduitCeilingY, F = Beat07Layout.FloorY, pt = Beat07Layout.PanelTopY;
            double uC = Beat07Layout.CulvertU, uE = Beat07Layout.EndU, uS = Beat07Layout.ShaftU0, ySoffit = Beat07Layout.CulvertSoffitY;
            double uBack = uC - 12.0, yShaft = yF - 7.0;

            // far wall (faces -v), with the shaft below the floor line from uS
            QuadUV(wall, uBack, uE, v1, yF, yC, false);
            QuadUV(wall, uS, uE, v1, yShaft, yF, false);
            // tunnel-side wall faces (face +v): below the deck and above the panel along the panel, full height before it
            QuadUV(wall, 0.0, uE, vi, yF, F, true);
            QuadUV(wall, 0.0, uE, vi, pt, yC, true);
            QuadUV(wall, uBack, 0.0, vi, yF, yC, true);
            QuadUV(wall, uS, uE, vi, yShaft, yF, true);
            // wall cap under the panel (faces up) and the lintel's underside (faces down), between the panel plane and vi
            Emit(wall, C(0.0, v0, F), C(uE, v0, F), C(uE, vi, F), C(0.0, vi, F), false);
            Emit(wall, C(0.0, v0, pt), C(0.0, vi, pt), C(uE, vi, pt), C(uE, v0, pt), false);
            // soffit (faces down)
            Emit(wall, C(uC, vi, yC), C(uC, v1, yC), C(uE, v1, yC), C(uE, vi, yC), false);
            // end wall (faces -u) down into the shaft; shaft upstream lip wall (faces +u) and bottom
            // (seen from upstream +v is on the viewer's right; seen from downstream it is on the left)
            Emit(wall, C(uE, vi, yShaft), C(uE, vi, yC), C(uE, v1, yC), C(uE, v1, yShaft), false);
            Emit(wall, C(uS, v1, yShaft), C(uS, v1, yF), C(uS, vi, yF), C(uS, vi, yShaft), false);
            Emit(wall, C(uS, vi, yShaft), C(uE, vi, yShaft), C(uE, v1, yShaft), C(uS, v1, yShaft), false);
            // culvert head wall (faces +u) above the culvert soffit, the culvert soffit, its back wall
            Emit(wall, C(uC, v1, ySoffit), C(uC, v1, yC), C(uC, vi, yC), C(uC, vi, ySoffit), false);
            Emit(wall, C(uBack, vi, ySoffit), C(uBack, v1, ySoffit), C(uC, v1, ySoffit), C(uC, vi, ySoffit), false);
            Emit(wall, C(uBack, v1, yF), C(uBack, v1, ySoffit), C(uBack, vi, ySoffit), C(uBack, vi, yF), false);
            // conduit floor (under the water, seen only near the shaft lip and in the culvert)
            Emit(wall, C(uBack, vi, yF), C(uS, vi, yF), C(uS, v1, yF), C(uBack, v1, yF), false);
            // the culvert walls continue the side walls between uBack and uC (already in the far / near wall quads above:
            // those run from uBack); above the culvert soffit they are behind the head wall

            // water surface: grid (u 0.5 m, 0.25 m near the shaft; v 0.5 m), displaced in the shader
            {
                var us = new List<double>();
                for (double u = uBack + 0.5; u < uS - 3.0; u += 0.5) us.Add(u);
                for (double u = uS - 3.0; u < uE - 0.05; u += 0.25) us.Add(u);
                us.Add(uE - 0.05);
                int nv = (int)Math.Ceiling((v1 - vi) / 0.5);
                int first = water.VertexCount;
                for (int i = 0; i < us.Count; i++)
                    for (int j = 0; j <= nv; j++)
                    {
                        double v = vi + 0.02 + (v1 - vi - 0.04) * j / nv;
                        water.Vertex(C(us[i], v, Beat07Layout.WaterY), Vector3.up);
                    }
                int row = nv + 1;
                for (int i = 0; i + 1 < us.Count; i++)
                    for (int j = 0; j < nv; j++)
                    {
                        int A = first + i * row + j, B = A + row;
                        // faces up: (u_i, v_j) -> (u_i+1, v_j) -> (u_i+1, v_j+1) -> (u_i, v_j+1) is clockwise from above when v
                        // is to the right of u (the conduit frame is the tunnel's right-handed plan frame)
                        water.Quad(A, B, B + 1, A + 1);
                    }
            }

            // trash rack over the shaft: bars along v every 0.6 m at the floor line
            for (double u = uS + 0.4; u < uE - 0.2; u += 0.6)
                MeshKit.ChamferMember(steel, C(u, vi + 0.05, yF - 0.08), C(u, v1 - 0.05, yF - 0.08), 0.1f, 0.16f, 0.01f, Vector3.up);

            Flush(ctx, root, ref wall, "b7_conduit_walls", m_cwall, false);
            Flush(ctx, root, ref water, "b7_conduit_water", m_water, false);
            Flush(ctx, root, ref steel, "b7_conduit_rack", m_steel, false);
        }

        /// <summary>Vertical quad in conduit coordinates at v, u0..u1, y0..y1; facePlusV: the face looks to +v.</summary>
        static void QuadUV(MeshBuilder mb, double u0, double u1, double v, double y0, double y1, bool facePlusV)
        {
            if (facePlusV) Emit(mb, C(u0, v, y0), C(u0, v, y1), C(u1, v, y1), C(u1, v, y0), false);
            else Emit(mb, C(u0, v, y0), C(u1, v, y0), C(u1, v, y1), C(u0, v, y1), false);
        }
    }
}
