// TramBuilder: the tram T_A of the boulevard and everything it runs on (MASTER_PROMPT 14.5.2 "Moving", 14.7 H7; owner BV, Phase 4
// wave A; TramSchedule has the route and the timing):
//   * the vehicle: five articulated modules of 11.68 m (60.0 m with four 0.4 m gangways), 3.4 m wide, body y 0.3..3.9 (14.5.2:
//     60 x 3.4 x 3.6), each module one mesh in its own frame (origin at its centre on the rail top, +z forward), drawn with
//     Megastructure/NocTram (zones by vertex colour R: paint, the continuous window band of the saloon, headlights, tail
//     lights, dark running gear, windscreens, bare steel, destination band); cab ends with a 0.35 m windscreen rake on the head
//     and the tail module, a single-arm pantograph on the middle module, roof equipment, bogies, bellows. About 1 900 triangles.
//     BV fix wave (DREAD 6, C29): two plug-sliding doors per module and side (car-local z -2.9 / +2.9, 1.32 x 2.57 m): on the
//     north side the closed leaves are part of the module; on the south (platform) side each module has a lit doorway (the saloon's
//     ZWindow light) behind two moving leaf meshes (tramDoorsA / B, children of the module) that TramDriver slides 0.64 m apart
//     while the tram waits at the stop (TramSchedule.DoorOpen). Leaves: paint with a glazed upper panel (ZWindow), dark edges.
//   * the overhead line: the contact wire (y 5.6) along the whole route from inside the terminus into the depot tunnel, a sagging
//     messenger (y 6.7 at the arms), steel poles 7.4 m every 30 m on the route's right-hand side (z 230.4 on the straight: north
//     of the track, outside the tram sight sector z 195-225), cantilever arms and droppers;
//   * the depot branch: rails along the r 35 curve and into the tunnel; the depot portal in the north podium's street face
//     (x -53.6..-48.4, y 0..6.4 at z 270, west of the arcade: StreetFacade leaves the opening) with a stone surround, a 30 m
//     concrete tunnel (dark, one dim bulkhead lamp near the mouth), its end wall at z 300; the tram's clip plane z 278 lies 8 m in.
// The terminus end (the hall behind the 5.2 x 6.4 m portal, the clip plane x 214.47, 2 cm behind its east interior wall) is TramTerminus's.
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Megastructure
{
    public static class TramBuilder
    {
        // ---- vertex zones of NocTram (vertex colour R = zone x 32)
        public const int ZPaint = 0, ZWindow = 1, ZHead = 2, ZTail = 3, ZDark = 4, ZScreen = 5, ZSteel = 6, ZDisplay = 7;

        // ---- body section (car-local metres: x right, y up, z forward)
        public const float HalfW = 1.7f, Y0 = 0.3f, YShoulder = 3.45f, YRoof = 3.9f, RoofHalfW = 1.4f;
        public const float WinY0 = 1.05f, WinY1 = 2.75f, WinInset = 0.03f, WinEndMargin = 0.45f;
        public const float CabRake = 0.35f, CabFrontY = 1.2f, CabSideWinZ0 = 1.65f, CabSideWinZ1 = 0.55f, CabBand = 1.95f;

        static float L => (float)TramSchedule.CarL;

        static void Zone(MeshBuilder mb, int zone, float seed)
            => mb.Color = new Color32((byte)(zone * 32), 0, (byte)Mathf.Clamp(Mathf.RoundToInt(seed * 255f), 0, 255), 255);

        /// <summary>A flat quad wound to face `want`, with explicit UV0 (metres).</summary>
        static void Q(MeshBuilder mb, Vector3 a, Vector3 b, Vector3 c, Vector3 d, Vector2 ua, Vector2 ub, Vector2 uc, Vector2 ud, Vector3 want)
        {
            Vector3 n = Vector3.Cross(b - a, c - a);
            if (Vector3.Dot(n, want) < 0f) { mb.FlatQuad(a, d, c, b, ua, ud, uc, ub); return; }
            mb.FlatQuad(a, b, c, d, ua, ub, uc, ud);
        }

        /// <summary>A convex planar polygon (fan) facing `want`, UV0 = (z, y) metres.</summary>
        static void Poly(MeshBuilder mb, IList<Vector3> pts, Vector3 want)
        {
            Vector3 n = Vector3.Cross(pts[1] - pts[0], pts[2] - pts[0]);
            bool flip = Vector3.Dot(n, want) < 0f;
            Vector3 nn = want.normalized;
            int i0 = mb.Vertex(pts[0], nn, new Vector2(pts[0].x, pts[0].y));
            int prev = mb.Vertex(pts[1], nn, new Vector2(pts[1].x, pts[1].y));
            for (int i = 2; i < pts.Count; i++)
            {
                int cur = mb.Vertex(pts[i], nn, new Vector2(pts[i].x, pts[i].y));
                if (flip) mb.Tri(i0, cur, prev); else mb.Tri(i0, prev, cur);
                prev = cur;
            }
        }

        // =========================================================================================================== the vehicle

        /// <summary>
        /// Module k (0 = head) in its own frame. Cab at the front for k 0 (headlights), at the rear for k 4 (tail lights); the
        /// gangway bellows ride on the front of modules 1..4; the pantograph on module PantoCar.
        /// </summary>
        public static void Car(MeshBuilder mb, int k)
        {
            float seed = 0.13f + 0.17f * k;
            float za = -0.5f * L, zb = 0.5f * L;
            bool cabFront = k == 0, cabRear = k == TramSchedule.Cars - 1;
            float zA = cabRear ? za + CabRake : za, zB = cabFront ? zb - CabRake : zb;   // the box part (side walls, roof)

            for (int side = -1; side <= 1; side += 2)
            {
                float x = side * HalfW;
                Vector3 outN = new Vector3(side, 0f, 0f);
                var holes = new List<Vector4>(3);   // (z0, z1, y0, y1) in car-local z
                float w0 = (cabRear ? za + CabBand : za + WinEndMargin), w1 = (cabFront ? zb - CabBand : zb - WinEndMargin);
                holes.Add(new Vector4(w0, w1, WinY0, WinY1));
                var holeZone = new List<int>(3) { ZWindow };
                if (cabFront) { holes.Add(new Vector4(zb - CabSideWinZ0, zb - CabSideWinZ1, WinY0 + 0.2f, WinY1)); holeZone.Add(ZScreen); }
                if (cabRear) { holes.Add(new Vector4(za + CabSideWinZ1, za + CabSideWinZ0, WinY0 + 0.2f, WinY1)); holeZone.Add(ZScreen); }
                Zone(mb, ZPaint, seed);
                Wall(mb, x, side, zA, zB, Y0, YShoulder, holes);
                // the cab end columns (the raked windscreen's side triangle-trapezoids)
                if (cabFront) Q(mb, P(x, Y0, zB), P(x, YShoulder, zB), P(x, CabFrontY, zb), P(x, Y0, zb), U(zB, Y0), U(zB, YShoulder), U(zb, CabFrontY), U(zb, Y0), outN);
                if (cabRear) Q(mb, P(x, Y0, za), P(x, CabFrontY, za), P(x, YShoulder, zA), P(x, Y0, zA), U(za, Y0), U(za, CabFrontY), U(zA, YShoulder), U(zA, Y0), outN);
                // inset glass and its dark gaskets (reveals)
                for (int h = 0; h < holes.Count; h++)
                {
                    var o = holes[h];
                    float xi = x - side * WinInset;
                    Zone(mb, holeZone[h], seed);
                    Q(mb, P(xi, o.z, o.x), P(xi, o.w, o.x), P(xi, o.w, o.y), P(xi, o.z, o.y), U(o.x, o.z), U(o.x, o.w), U(o.y, o.w), U(o.y, o.z), outN);
                    Zone(mb, ZDark, seed);
                    Q(mb, P(x, o.z, o.x), P(xi, o.z, o.x), P(xi, o.z, o.y), P(x, o.z, o.y), U(o.x, 0f), U(o.x, WinInset), U(o.y, WinInset), U(o.y, 0f), Vector3.up);
                    Q(mb, P(x, o.w, o.x), P(xi, o.w, o.x), P(xi, o.w, o.y), P(x, o.w, o.y), U(o.x, 0f), U(o.x, WinInset), U(o.y, WinInset), U(o.y, 0f), Vector3.down);
                    Q(mb, P(x, o.z, o.x), P(x, o.w, o.x), P(xi, o.w, o.x), P(xi, o.z, o.x), U(0f, o.z), U(0f, o.w), U(WinInset, o.w), U(WinInset, o.z), Vector3.forward);
                    Q(mb, P(x, o.z, o.y), P(x, o.w, o.y), P(xi, o.w, o.y), P(xi, o.z, o.y), U(0f, o.z), U(0f, o.w), U(WinInset, o.w), U(WinInset, o.z), Vector3.back);
                }
                // shoulder (raked from the wall top to the roof edge)
                Zone(mb, ZPaint, seed);
                Q(mb, P(x, YShoulder, zA), P(side * RoofHalfW, YRoof, zA), P(side * RoofHalfW, YRoof, zB), P(x, YShoulder, zB),
                    U(zA - za, YShoulder), U(zA - za, YRoof), U(zB - za, YRoof), U(zB - za, YShoulder), new Vector3(side * 0.83f, 0.56f, 0f));
            }
            // doors (DREAD 6): the south side's lit doorways (the moving leaves are TramDriver's), the north side's closed leaves
            Doorways(mb, seed);
            foreach (float dc in DoorZ) { Leaf(mb, +1, dc - DoorLeafW, dc, seed); Leaf(mb, +1, dc, dc + DoorLeafW, seed); }

            // roof and underside
            Zone(mb, ZPaint, seed);
            Q(mb, P(-RoofHalfW, YRoof, zA), P(RoofHalfW, YRoof, zA), P(RoofHalfW, YRoof, zB), P(-RoofHalfW, YRoof, zB),
                new Vector2(zA - za, -RoofHalfW), new Vector2(zA - za, RoofHalfW), new Vector2(zB - za, RoofHalfW), new Vector2(zB - za, -RoofHalfW), Vector3.up);
            Zone(mb, ZDark, seed);
            Q(mb, P(-HalfW, Y0, za), P(HalfW, Y0, za), P(HalfW, Y0, zb), P(-HalfW, Y0, zb), U(0, 0), U(0, 1), U(1, 1), U(1, 0), Vector3.down);

            // ends
            if (cabFront) Cab(mb, zb, +1, ZHead, seed); else EndFace(mb, zb, +1, seed);
            if (cabRear) Cab(mb, za, -1, ZTail, seed); else EndFace(mb, za, -1, seed);

            // gangway bellows on the front of modules 1..4 (into the 0.4 m gap toward module k - 1)
            if (k > 0)
            {
                Zone(mb, ZDark, seed);
                float g = (float)TramSchedule.Gangway;
                MeshKit.ChamferBox(mb, new Vector3(0f, 2.0f, zb + 0.5f * g), new Vector3(2.9f, 3.1f, g + 0.04f), 0.02f);
            }
            // bogies (dark, between the rails and the body)
            Zone(mb, ZDark, seed);
            float bz = 0.5f * L - (float)TramSchedule.BogieInset;
            foreach (float zc in new[] { -bz, bz })
            {
                MeshKit.ChamferBox(mb, new Vector3(0f, 0.19f, zc), new Vector3(2.5f, 0.24f, 2.4f), 0.02f);
                foreach (float xs in new[] { -1.0f, 1.0f })                      // wheel sets' side frames, slightly proud
                    MeshKit.ChamferBox(mb, new Vector3(xs * 0.78f, 0.17f, zc), new Vector3(0.14f, 0.3f, 2.1f), 0.01f);
            }
            // roof gear: equipment boxes (steel) on modules 0, 1, 3, 4; the pantograph on the middle module
            Zone(mb, ZSteel, seed);
            if (k == TramSchedule.PantoCar) Pantograph(mb);
            else
            {
                float len = k == 0 || k == TramSchedule.Cars - 1 ? 2.6f : 3.8f;
                float zc = k == 0 ? -1.2f : (k == TramSchedule.Cars - 1 ? 1.2f : 0f);
                MeshKit.ChamferBox(mb, new Vector3(0f, YRoof + 0.17f, zc), new Vector3(1.9f, 0.34f, len), 0.03f);
                Zone(mb, ZDark, seed);
                MeshKit.ChamferBox(mb, new Vector3(0f, YRoof + 0.38f, zc), new Vector3(1.4f, 0.08f, len - 0.4f), 0.01f);   // grille
            }
        }

        // ---- doors (DREAD 6, BV fix wave): car-local door centres, leaf width and the leaves' planes (outside the body wall)
        public static readonly float[] DoorZ = { -2.9f, 2.9f };
        public const float DoorLeafW = 0.66f, DoorY0 = 0.38f, DoorY1 = 2.95f, DoorGlassY0 = 1.10f, DoorGlassY1 = 2.70f, DoorFrame = 0.07f;
        public const float LeafOut = 0.035f, LeafIn = 0.012f, DoorwayOut = 0.004f;

        /// <summary>The south (platform) side's doorways: a lit panel (ZWindow: the saloon's light) 4 mm outside the body wall over
        /// each door opening and a dark threshold under it; the leaves close over them.</summary>
        static void Doorways(MeshBuilder mb, float seed)
        {
            float x = -HalfW - DoorwayOut;
            Vector3 n = Vector3.left;
            foreach (float dc in DoorZ)
            {
                float z0 = dc - DoorLeafW, z1 = dc + DoorLeafW;
                Zone(mb, ZWindow, seed);
                Q(mb, P(x, DoorY0, z0), P(x, DoorY1, z0), P(x, DoorY1, z1), P(x, DoorY0, z1), U(z0, DoorY0), U(z0, DoorY1), U(z1, DoorY1), U(z1, DoorY0), n);
                Zone(mb, ZDark, seed);
                Q(mb, P(x, Y0, z0), P(x, DoorY0, z0), P(x, DoorY0, z1), P(x, Y0, z1), U(z0, Y0), U(z0, DoorY0), U(z1, DoorY0), U(z1, Y0), n);
            }
        }

        /// <summary>One plug-sliding door leaf on `side` (car-local z0..z1): its outer face (paint, a glazed upper panel inset by the
        /// frame width, ZWindow) LeafOut outside the body wall and dark edges back to LeafIn. UV v = y (the livery bands carry on).</summary>
        public static void Leaf(MeshBuilder mb, int side, float z0, float z1, float seed)
        {
            float xo = side * (HalfW + LeafOut), xi = side * (HalfW + LeafIn);
            Vector3 n = new Vector3(side, 0f, 0f);
            float f = DoorFrame, g0 = DoorGlassY0, g1 = DoorGlassY1;
            Zone(mb, ZPaint, seed);
            Q(mb, P(xo, DoorY0, z0), P(xo, g0, z0), P(xo, g0, z1), P(xo, DoorY0, z1), U(z0, DoorY0), U(z0, g0), U(z1, g0), U(z1, DoorY0), n);
            Q(mb, P(xo, g1, z0), P(xo, DoorY1, z0), P(xo, DoorY1, z1), P(xo, g1, z1), U(z0, g1), U(z0, DoorY1), U(z1, DoorY1), U(z1, g1), n);
            Q(mb, P(xo, g0, z0), P(xo, g1, z0), P(xo, g1, z0 + f), P(xo, g0, z0 + f), U(z0, g0), U(z0, g1), U(z0 + f, g1), U(z0 + f, g0), n);
            Q(mb, P(xo, g0, z1 - f), P(xo, g1, z1 - f), P(xo, g1, z1), P(xo, g0, z1), U(z1 - f, g0), U(z1 - f, g1), U(z1, g1), U(z1, g0), n);
            Zone(mb, ZWindow, seed);
            Q(mb, P(xo, g0, z0 + f), P(xo, g1, z0 + f), P(xo, g1, z1 - f), P(xo, g0, z1 - f), U(z0 + f, g0), U(z0 + f, g1), U(z1 - f, g1), U(z1 - f, g0), n);
            Zone(mb, ZDark, seed);
            Q(mb, P(xi, DoorY0, z0), P(xi, DoorY1, z0), P(xo, DoorY1, z0), P(xo, DoorY0, z0), U(0f, DoorY0), U(0f, DoorY1), U(LeafOut, DoorY1), U(LeafOut, DoorY0), Vector3.back);
            Q(mb, P(xi, DoorY0, z1), P(xi, DoorY1, z1), P(xo, DoorY1, z1), P(xo, DoorY0, z1), U(0f, DoorY0), U(0f, DoorY1), U(LeafOut, DoorY1), U(LeafOut, DoorY0), Vector3.forward);
            Q(mb, P(xi, DoorY1, z0), P(xi, DoorY1, z1), P(xo, DoorY1, z1), P(xo, DoorY1, z0), U(z0, 0f), U(z1, 0f), U(z1, LeafOut), U(z0, LeafOut), Vector3.up);
            Q(mb, P(xi, DoorY0, z0), P(xi, DoorY0, z1), P(xo, DoorY0, z1), P(xo, DoorY0, z0), U(z0, 0f), U(z1, 0f), U(z1, LeafOut), U(z0, LeafOut), Vector3.down);
        }

        /// <summary>The south side's moving leaves of one module: dir -1 the leaves that open toward -z (the -z half of each door),
        /// +1 the others; built closed, in car-local coordinates (TramDriver offsets the child by dir x opening x DoorTravel).</summary>
        public static void DoorLeaves(MeshBuilder mb, int k, int dir)
        {
            float seed = 0.13f + 0.17f * k;
            foreach (float dc in DoorZ)
            {
                if (dir < 0) Leaf(mb, -1, dc - DoorLeafW, dc, seed);
                else Leaf(mb, -1, dc, dc + DoorLeafW, seed);
            }
        }

        static Vector3 P(float x, float y, float z) => new Vector3(x, y, z);
        static Vector2 U(float u, float v) => new Vector2(u + 0.5f * L, v);

        /// <summary>A side wall plane x (outward side) from z0..z1, y0..y1 with rectangular holes, on the conformal grid.</summary>
        static void Wall(MeshBuilder mb, float x, int side, float z0, float z1, float y0, float y1, List<Vector4> holes)
        {
            var zs = new List<float> { z0, z1 }; var ys = new List<float> { y0, y1 };
            foreach (var o in holes) { zs.Add(o.x); zs.Add(o.y); ys.Add(o.z); ys.Add(o.w); }
            zs.Sort(); ys.Sort();
            Vector3 n = new Vector3(side, 0f, 0f);
            for (int i = 0; i + 1 < zs.Count; i++)
            {
                if (zs[i + 1] - zs[i] < 1e-4f || zs[i] < z0 - 1e-4f || zs[i + 1] > z1 + 1e-4f) continue;
                for (int j = 0; j + 1 < ys.Count; j++)
                {
                    if (ys[j + 1] - ys[j] < 1e-4f) continue;
                    float zc = 0.5f * (zs[i] + zs[i + 1]), yc = 0.5f * (ys[j] + ys[j + 1]);
                    bool open = false;
                    foreach (var o in holes) if (zc > o.x && zc < o.y && yc > o.z && yc < o.w) { open = true; break; }
                    if (open) continue;
                    Q(mb, P(x, ys[j], zs[i]), P(x, ys[j + 1], zs[i]), P(x, ys[j + 1], zs[i + 1]), P(x, ys[j], zs[i + 1]),
                        U(zs[i], ys[j]), U(zs[i], ys[j + 1]), U(zs[i + 1], ys[j + 1]), U(zs[i + 1], ys[j]), n);
                }
            }
        }

        /// <summary>The flat end of an intermediate module (paint, the section outline) at z (dir +1 front, -1 rear).</summary>
        static void EndFace(MeshBuilder mb, float z, int dir, float seed)
        {
            Zone(mb, ZPaint, seed);
            var pts = new List<Vector3>
            {
                P(-HalfW, Y0, z), P(HalfW, Y0, z), P(HalfW, YShoulder, z), P(RoofHalfW, YRoof, z), P(-RoofHalfW, YRoof, z), P(-HalfW, YShoulder, z),
            };
            Poly(mb, pts, new Vector3(0f, 0f, dir));
        }

        /// <summary>A cab end at z (dir +1 front of the head module, -1 rear of the tail module): the lower front with two lamp
        /// lenses (zone lamp: headlights or tail lights) and a dark coupler cover, the raked windscreen in a paint frame, the cap
        /// above it (the section's shoulder outline) with the destination band.</summary>
        static void Cab(MeshBuilder mb, float z, int dir, int lamp, float seed)
        {
            Vector3 fwd = new Vector3(0f, 0f, dir);
            float zr = z - dir * CabRake;
            Zone(mb, ZPaint, seed);
            Q(mb, P(-HalfW, Y0, z), P(-HalfW, CabFrontY, z), P(HalfW, CabFrontY, z), P(HalfW, Y0, z), U(-HalfW, Y0), U(-HalfW, CabFrontY), U(HalfW, CabFrontY), U(HalfW, Y0), fwd);
            // the raked plane: (xx, t) -> (xx, 1.2 + t (3.45 - 1.2), z - dir t rake); frame strips and the windscreen
            Vector3 S(float xx, float t) => P(xx, CabFrontY + t * (YShoulder - CabFrontY), z - dir * t * CabRake);
            Vector3 nRake = new Vector3(0f, CabRake, dir * (YShoulder - CabFrontY)).normalized;
            float fx = 1.52f, t0 = 0.06f, t1 = 0.93f;
            void Strip(float xa, float xb, float ta, float tb, int zone)
            {
                Zone(mb, zone, seed);
                Q(mb, S(xa, ta), S(xa, tb), S(xb, tb), S(xb, ta), U(xa, ta * 2.3f), U(xa, tb * 2.3f), U(xb, tb * 2.3f), U(xb, ta * 2.3f), nRake);
            }
            Strip(-HalfW, -fx, 0f, 1f, ZPaint);
            Strip(fx, HalfW, 0f, 1f, ZPaint);
            Strip(-fx, fx, 0f, t0, ZPaint);
            Strip(-fx, fx, t1, 1f, ZPaint);
            Strip(-fx, fx, t0, t1, ZScreen);
            // cap above the windscreen (facing forward) and the destination band on it
            Zone(mb, ZPaint, seed);
            var cap = new List<Vector3> { P(-HalfW, YShoulder, zr), P(HalfW, YShoulder, zr), P(RoofHalfW, YRoof, zr), P(-RoofHalfW, YRoof, zr) };
            Poly(mb, cap, fwd);
            Zone(mb, ZDisplay, seed);
            MeshKit.ChamferBox(mb, new Vector3(0f, 0.5f * (YShoulder + YRoof) - 0.02f, zr + dir * 0.015f), new Vector3(1.9f, 0.24f, 0.05f), 0.01f);
            // lamp lenses (proud 2-3 cm) and the coupler cover
            Zone(mb, lamp, seed);
            foreach (float xs in new[] { -1.12f, 1.12f })
                MeshKit.ChamferBox(mb, new Vector3(xs, 0.76f, z + dir * 0.02f), new Vector3(0.40f, 0.17f, 0.05f), 0.01f);
            Zone(mb, ZDark, seed);
            MeshKit.ChamferBox(mb, new Vector3(0f, 0.42f, z + dir * 0.05f), new Vector3(2.4f, 0.18f, 0.12f), 0.02f);
        }

        /// <summary>A single-arm pantograph on the roof (centre z 0): base frame, insulators, lower arm, upper arm, collector head
        /// with its carbon strip just under the contact wire (top 5.585 m) and horns.</summary>
        static void Pantograph(MeshBuilder mb)
        {
            float y0 = YRoof;
            MeshKit.ChamferBox(mb, new Vector3(0f, y0 + 0.08f, 0f), new Vector3(1.5f, 0.1f, 1.6f), 0.02f);
            foreach (float xs in new[] { -0.6f, 0.6f })
                foreach (float zs in new[] { -0.65f, 0.65f })
                    MeshKit.ChamferBox(mb, new Vector3(xs, y0 + 0.02f, zs), new Vector3(0.14f, 0.12f, 0.14f), 0.02f);
            float head = (float)TramSchedule.WireY - 0.015f;
            var hinge = new Vector3(0f, y0 + 0.16f, -0.85f);
            var knee = new Vector3(0f, y0 + 0.95f, 0.85f);
            var top = new Vector3(0f, head - 0.08f, -0.25f);
            MeshKit.ChamferMember(mb, hinge + new Vector3(-0.22f, 0f, 0f), knee + new Vector3(-0.05f, 0f, 0f), 0.07f, 0.09f, 0.01f, Vector3.up);
            MeshKit.ChamferMember(mb, hinge + new Vector3(0.22f, 0f, 0f), knee + new Vector3(0.05f, 0f, 0f), 0.07f, 0.09f, 0.01f, Vector3.up);
            MeshKit.ChamferMember(mb, knee, top, 0.06f, 0.06f, 0.01f, Vector3.up);
            MeshKit.ChamferMember(mb, new Vector3(0f, y0 + 0.16f, -0.55f), knee + new Vector3(0f, -0.1f, -0.15f), 0.03f, 0.03f, 0.005f, Vector3.up);   // the push rod
            MeshKit.ChamferMember(mb, new Vector3(-0.82f, head - 0.035f, -0.25f), new Vector3(0.82f, head - 0.035f, -0.25f), 0.06f, 0.05f, 0.008f, Vector3.forward);
            MeshKit.ChamferMember(mb, new Vector3(-0.3f, head - 0.08f, -0.25f), new Vector3(0.3f, head - 0.08f, -0.25f), 0.05f, 0.04f, 0.008f, Vector3.forward);
            MeshKit.ChamferMember(mb, new Vector3(-0.82f, head - 0.035f, -0.25f), new Vector3(-1.0f, head - 0.2f, -0.25f), 0.04f, 0.04f, 0.006f, Vector3.forward);
            MeshKit.ChamferMember(mb, new Vector3(0.82f, head - 0.035f, -0.25f), new Vector3(1.0f, head - 0.2f, -0.25f), 0.04f, 0.04f, 0.006f, Vector3.forward);
        }

        // =========================================================================================== the vehicle in the world

        static readonly int IdClipA = Shader.PropertyToID("_ClipA"), IdClipB = Shader.PropertyToID("_ClipB");

        /// <summary>The tram material: the library's Tram row (MaterialFactory.Boulevard) or, before a RebuildAll created it, one made
        /// from the shader with the same values; the clip planes always from TramSchedule. Null without the shader (EditMode).</summary>
        public static Material TramMaterial(MaterialLibrary lib)
        {
            Material m = null;
            if (lib != null && lib.Tram != null) m = new Material(lib.Tram) { name = "Beat02_Tram" };
            else
            {
                var sh = Shader.Find("Megastructure/NocTram");
                if (sh == null) return null;
                m = new Material(sh) { name = "Beat02_Tram" };
                m.SetColor("_BaseColor", G1Materials.Lin(NocPal.MetalPaint, 1f).gamma);
                m.SetColor("_WinLight", G1Materials.Lin(NocPal.WinNeutral, 1f).gamma);
                m.SetFloat("_WinGain", TramLook.WinGain);
                m.SetColor("_HeadLight", G1Materials.Lin(NocPal.SignWhite, 1f).gamma);
                m.SetFloat("_HeadGain", TramLook.HeadGain);
                m.SetColor("_TailLight", G1Materials.Lin(NocPal.AviationRed, 1f).gamma);
                m.SetFloat("_TailGain", TramLook.TailGain);
                m.SetColor("_Display", G1Materials.Lin(NocPal.SodiumAmber, 1f).gamma);
                m.SetFloat("_DisplayGain", TramLook.DisplayGain);
            }
            m.SetVector(IdClipA, new Vector4(1f, 0f, 0f, -(float)TramSchedule.ClipX));
            m.SetVector(IdClipB, new Vector4(0f, 0f, 1f, -(float)TramSchedule.ClipZ));
            return m;
        }

        /// <summary>The five module meshes (validated), their renderers under root, the three lights and the driver (registered).
        /// Returns the driver, or null at tier >= 1 (14.5.2 Simplified: no tram). failed counts invalid meshes.</summary>
        public static TramDriver Create(WorldContext ctx, Transform root, ref int failed)
        {
            if (ctx.Tier >= 1) return null;
            var mat = TramMaterial(ctx.Materials);
            int n = TramSchedule.Cars;
            var cars = new Transform[n];
            var rs = new Renderer[3 * n];
            var doorA = new Transform[n]; var doorB = new Transform[n];
            for (int k = 0; k < n; k++)
            {
                var mb = new MeshBuilder();
                Car(mb, k);
                var rep = MeshValidation.Validate(mb, "Beat02_tramCar" + k);
                if (!rep.Ok) { failed++; Debug.LogError("[NOC-FAIL] Beat02 tram: " + rep); }
                var mr = ctx.AddMesh(root, "tramCar" + k, mb.ToMesh("Beat02_tramCar" + k), mat);
                TramDriver.Place(0.0, k, out Vector3 pos, out Quaternion rot);
                mr.transform.SetPositionAndRotation(pos, rot);
                cars[k] = mr.transform; rs[k] = mr;
                // DREAD 6: the south side's two moving leaf meshes, children of the module (car-local, built closed)
                for (int dir = -1; dir <= 1; dir += 2)
                {
                    var lb = new MeshBuilder();
                    DoorLeaves(lb, k, dir);
                    string nm = "tramDoors" + (dir < 0 ? "A" : "B") + k;
                    var lrep = MeshValidation.Validate(lb, "Beat02_" + nm);
                    if (!lrep.Ok) { failed++; Debug.LogError("[NOC-FAIL] Beat02 tram doors: " + lrep); }
                    var lr = ctx.AddMesh(mr.transform, nm, lb.ToMesh("Beat02_" + nm), mat);
                    lr.transform.localPosition = Vector3.zero; lr.transform.localRotation = Quaternion.identity;
                    if (dir < 0) doorA[k] = lr.transform; else doorB[k] = lr.transform;
                    rs[n + 2 * k + (dir < 0 ? 0 : 1)] = lr;
                }
            }
            var head = MakeLight(root, "tramHeadlight", G1Materials.Lin(NocPal.SignWhite, 1f), TramDriver.HeadRange, TramDriver.HeadIntensity);
            var saloon = new Light[TramDriver.SaloonCars.Length];
            for (int i = 0; i < saloon.Length; i++)
                saloon[i] = MakeLight(root, "tramSaloon" + i, G1Materials.Lin(NocPal.WinNeutral, 1f), TramDriver.SaloonRange, 0f);
            var d = new TramDriver(cars, rs, head, saloon, doorA, doorB);
            ctx.Register(d);
            return d;
        }

        static Light MakeLight(Transform root, string name, Color linear, float range, float intensity)
        {
            var go = new GameObject(name);
            go.transform.SetParent(root, false);
            go.transform.position = TramSchedule.PointV(1.2, 0.95);
            var li = go.AddComponent<Light>();
            li.type = LightType.Point;
            li.color = linear;
            li.range = range;
            li.intensity = intensity;
            li.shadows = LightShadows.None;
            li.renderMode = LightRenderMode.ForcePixel;
            return li;
        }

        /// <summary>Triangles of the five modules (budget log and tests).</summary>
        public static long VehicleTriangles()
        {
            long t = 0;
            for (int k = 0; k < TramSchedule.Cars; k++)
            {
                var mb = new MeshBuilder(); Car(mb, k); DoorLeaves(mb, k, -1); DoorLeaves(mb, k, 1); t += mb.TriangleCount;
            }
            return t;
        }

        // ============================================================================================ the static route furniture

        /// <summary>The p values of the overhead-line poles (every 30 m from p 6 to the portal).</summary>
        public static List<double> PoleP()
        {
            // every 30 m on the straight; every 11 m on the curve (pull-offs: the messenger chord stays within 0.45 m of the wire)
            var l = new List<double>(24);
            double p = 6.0;
            for (; p < TramSchedule.StraightL - 4.0; p += TramSchedule.PolePitch) l.Add(p);
            double a0 = TramSchedule.StraightL + 1.0, a1 = TramSchedule.StraightL + TramSchedule.ArcL - 1.0;
            int n = (int)Math.Ceiling((a1 - a0) / 11.0);
            for (int i = 0; i <= n; i++) l.Add(a0 + (a1 - a0) * i / n);
            l.Add(TramSchedule.PortalP - 4.0);
            return l;
        }

        /// <summary>The pole position (on the route's right-hand side, 3.4 m from the track centre).</summary>
        public static Vector3 PolePos(double p)
        {
            TramSchedule.Point(p, out double x, out double z, out double hx, out double hz);
            double off = TramSchedule.PoleZ - TramSchedule.TrackZ;
            return new Vector3((float)(x + hz * off), 0f, (float)(z - hx * off));
        }

        public static void BuildRoute(G1Parts parts)
        {
            OverheadLine(parts);
            BranchRails(parts);
            DepotPortal(parts);
        }

        static void OverheadLine(G1Parts parts)
        {
            var pts = new List<Vector3>(64); var frames = new List<LoftFrame>(64); var sec = new CrossSection();
            float wy = (float)TramSchedule.WireY, my = (float)TramSchedule.MessengerY, r = 0.5f * G1Materials.Cable;
            double p0 = -(TramSchedule.ClipX - 0.3 - TramSchedule.StartX), p1 = TramSchedule.PortalP + 18.0;
            // the contact wire in chunks of <= 120 m of route (chunk-local origins)
            int chunk = 0;
            for (double a = p0; a < p1 - 1e-6; a += 120.0, chunk++)
            {
                double b = Math.Min(p1, a + 120.0);
                var mid = TramSchedule.PointV(0.5 * (a + b), wy);
                var mb = parts.Get("tramWire" + chunk, G1Mat.Line6mm, new Vector3(Mathf.Round(mid.x), wy, Mathf.Round(mid.z)));
                pts.Clear();
                int n = Mathf.Max(1, Mathf.CeilToInt((float)(b - a) / 3f));
                for (int i = 0; i <= n; i++)
                {
                    double p = a + (b - a) * i / n;
                    // straight runs need only their ends (the loft frames stay exact), the curve every <= 3 m
                    if (i > 0 && i < n && (p < TramSchedule.StraightL - 1.0 || p > TramSchedule.StraightL + TramSchedule.ArcL + 1.0)) continue;
                    pts.Add(TramSchedule.PointV(p, wy));
                }
                G1Mesh.Tube(mb, pts, r, 6, frames, sec);
            }
            // poles, arms, droppers and the messenger spans between the arms
            var poles = PoleP();
            var steel = parts.Get("tramPoles", G1Mat.SteelPainted, new Vector3(100f, 3.7f, 230f));
            var steelW = parts.Get("tramPolesW", G1Mat.SteelPainted, new Vector3(-40f, 3.7f, 245f));
            var footing = parts.Get("tramPoleFootings", G1Mat.Concrete, new Vector3(100f, 0.1f, 230f));
            var rods = parts.Get("tramDroppers", G1Mat.LineRod, new Vector3(100f, 6f, 228f));
            Vector3 prevTip = Vector3.zero; bool havePrev = false; int span = 0;
            foreach (double p in poles)
            {
                Vector3 b = PolePos(p);
                var track = TramSchedule.PointV(p, 0.0);
                var mb = b.x > 40f ? steel : steelW;
                float H = (float)TramSchedule.PoleH;
                MeshKit.Cylinder(mb, b + new Vector3(0f, 0.05f, 0f), b + new Vector3(0f, H, 0f), 0.13f, 12, true);
                MeshKit.ChamferBox(footing, b + new Vector3(0f, 0.1f, 0f), new Vector3(0.6f, 0.2f, 0.6f), 0.02f);
                // cantilever arm over the track at y 6.95, brace from the pole at 5.9 to the arm's middle
                Vector3 armA = b + new Vector3(0f, 6.95f, 0f), armB = new Vector3(track.x, 6.95f, track.z) + (track - b).normalized * 0.4f;
                MeshKit.ChamferMember(mb, armA, armB, 0.09f, 0.11f, 0.01f, Vector3.up);
                MeshKit.ChamferMember(mb, b + new Vector3(0f, 5.85f, 0f), Vector3.Lerp(armA, armB, 0.6f), 0.06f, 0.06f, 0.008f, Vector3.up);
                Vector3 tip = new Vector3(track.x, my, track.z);
                pts.Clear(); pts.Add(new Vector3(track.x, wy, track.z)); pts.Add(new Vector3(track.x, 6.95f, track.z));
                G1Mesh.Tube(rods, pts, 0.5f * G1Materials.Rod, 6, frames, sec);
                if (havePrev)
                {
                    var mid = 0.5f * (prevTip + tip);
                    var ms = parts.Get("tramMessenger" + (span / 4), G1Mat.Line6mm, new Vector3(Mathf.Round(mid.x), my, Mathf.Round(mid.z)));
                    G1Mesh.Catenary(prevTip, tip, 0.45f, 12, pts);
                    G1Mesh.Tube(ms, pts, r, 6, frames, sec);
                    span++;
                }
                prevTip = tip; havePrev = true;
            }
            PoleLamps(parts);
        }

        // Fifth start (BV_r01 KS06 / KS04: the tram ran through the darkest stretch of the street, the sector x 79-175 where the
        // north-row median lamps are left out, so its line had no light of its own): a luminaire on every overhead-line pole of
        // the straight east of x 0, as on many tram streets. The pole goes on as a mast to y 8.95; a 1.5 m arm reaches north over
        // the carriageway (away from the track and the walker: z 230.4 -> 231.9, outside the sight sector z 195-225) and carries
        // the median lamps' luminaire (dark shell over a LampWarm x 1.6 lens) with its in-air halo. No realtime light (the 28
        // budget); the lenses at x 60..170 are wet-floor sources, so their streaks run across the north road and onto the median
        // (mirror point z ~201 for the eye at z 195).
        public const float PoleMastTop = 8.95f, PoleLampArm = 1.5f;
        public const float PoleLampWetX0 = 60f, PoleLampWetX1 = 170f;

        /// <summary>Base points (x, 0, z) of the poles that carry a luminaire (the straight, x >= 0).</summary>
        public static List<Vector3> PoleLampBases()
        {
            var l = new List<Vector3>(8);
            foreach (double p in PoleP())
            {
                if (p >= TramSchedule.StraightL - 4.0) continue;
                Vector3 b = PolePos(p);
                if (b.x >= 0f) l.Add(b);
            }
            return l;
        }

        /// <summary>The lens centre of a pole luminaire (its wet-floor source point).</summary>
        public static Vector3 PoleLampLens(Vector3 b)
            => new Vector3(b.x, PoleMastTop - MedianPromenade.LensDrop - 0.5f * MedianPromenade.LensDepth, b.z + PoleLampArm);

        static void PoleLamps(G1Parts parts)
        {
            var mast = parts.Get("tramPoleLampMasts", G1Mat.LinePost, new Vector3(100f, 8f, 231f));
            var lens = parts.Get("tramPoleLampLenses", G1Mat.EmitLamp, new Vector3(100f, 8.5f, 232f));
            var shell = parts.Get("tramPoleLampHousings", G1Mat.SteelPainted, new Vector3(100f, 8.7f, 232f));
            var pts = new List<Vector3>(3); var frames = new List<LoftFrame>(3); var sec = new CrossSection();
            var halos = new List<Vector3>(8);
            float hz = 0.5f * (float)BoulevardLayout.LampHead, top = PoleMastTop;
            foreach (var b in PoleLampBases())
            {
                float zi = b.z + PoleLampArm;
                // mast from inside the pole top, then the arm into the shell's tail (one NocLine tube, as the median posts)
                pts.Clear();
                pts.Add(new Vector3(b.x, (float)TramSchedule.PoleH - 0.3f, b.z));
                pts.Add(new Vector3(b.x, top - 0.2f, b.z));
                pts.Add(new Vector3(b.x, top - 0.2f, zi - (hz - 0.15f)));
                G1Mesh.Tube(mast, pts, 0.5f * G1Materials.Post, 8, frames, sec);
                halos.Add(MedianPromenade.Luminaire(lens, shell, b.x, zi, -1f, top));
            }
            NocHalos.Queue("TramPoleLamps", halos, G1Materials.Lin(NocPal.LampWarm, MedianPromenade.LampRadiance), MedianPromenade.LampLightRange);
        }

        /// <summary>The pole luminaires' wet-floor sources (x 60..170; Beat02_Boulevard.RegisterWet).</summary>
        public static void RegisterSources()
        {
            foreach (var b in PoleLampBases())
            {
                if (b.x < PoleLampWetX0 || b.x > PoleLampWetX1) continue;
                var p = PoleLampLens(b);
                NocWetRegistry.AddSource(p, p, MedianPromenade.LampRho, G1Materials.Lin(NocPal.LampWarm, MedianPromenade.LampRadiance));
            }
        }

        /// <summary>Rails of the depot branch: the r 35 curve and the run north into the tunnel (z 262..298).</summary>
        static void BranchRails(G1Parts parts)
        {
            var pts = new List<Vector3>(48); var frames = new List<LoftFrame>(48); var sec = new CrossSection();
            var mb = parts.Get("tramBranchRails", G1Mat.LineRail, new Vector3(-35f, 0f, 250f));
            float r = 0.5f * G1Materials.Rail, y = StreetLevel.RailAxisY, g = 0.5f * (float)BoulevardLayout.Gauge;
            double a = TramSchedule.StraightL + 0.5, b = TramSchedule.StraightL + TramSchedule.ArcL + (TramSchedule.TunnelZ1 - 2.0 - TramSchedule.CurveEndZ);
            foreach (float off in new[] { -g, g })
            {
                pts.Clear();
                for (double p = a; p <= b + 1e-6; p += 1.5)
                {
                    TramSchedule.Point(p, out double x, out double z, out double hx, out double hz);
                    pts.Add(new Vector3((float)(x + hz * off), y, (float)(z - hx * off)));
                }
                G1Mesh.Tube(mb, pts, r, 8, frames, sec);
            }
        }

        /// <summary>The depot's bulkhead light (a pool at the mouth, not a beacon) and its window in camera x (BoulevardLights:
        /// seen from the crossing and C1, behind the walker further east).</summary>
        public static G1Light DepotLight()
        {
            float xc = (float)TramSchedule.PortalX, hw = 0.5f * (float)TramSchedule.PortalW, z0 = (float)TramSchedule.PortalZ;
            return new G1Light { name = "depotLight", position = new Vector3(xc - hw + 0.6f, 4.4f, z0 + 3.0f), colour = G1Materials.Lin(NocPal.LampWarm, 1f), range = 9f, intensity = 0.9f };
        }
        public const double DepotLightX0 = -80.0, DepotLightX1 = 10.0;   // behind the walker east of x 10 (faded out by x 26)

        /// <summary>The depot portal in the north podium face (z 270) and its tunnel (z 270..300).</summary>
        static void DepotPortal(G1Parts parts)
        {
            float xc = (float)TramSchedule.PortalX, hw = 0.5f * (float)TramSchedule.PortalW, h = (float)TramSchedule.PortalH;
            float z0 = (float)TramSchedule.PortalZ, z1 = (float)TramSchedule.TunnelZ1;
            var origin = new Vector3(xc, 3f, 280f);
            var stone = parts.Get("depotSurround", G1Mat.StoneWall, origin);
            var conc = parts.Get("depotTunnel", G1Mat.Concrete, origin);
            var floor = parts.Get("depotFloor", G1Mat.Concrete, origin);
            // surround: jambs 0.8 m and a lintel 1.0 m, 0.35 m proud of the face (their backs 5 cm behind the face plane)
            G1Mesh.Chamfer(stone, new Vector3(xc - hw - 0.8f, 0f, z0 - 0.35f), new Vector3(xc - hw, h + 1.0f, z0 + 0.05f), 0.03f);
            G1Mesh.Chamfer(stone, new Vector3(xc + hw, 0f, z0 - 0.35f), new Vector3(xc + hw + 0.8f, h + 1.0f, z0 + 0.05f), 0.03f);
            G1Mesh.Chamfer(stone, new Vector3(xc - hw - 0.01f, h, z0 - 0.35f), new Vector3(xc + hw + 0.01f, h + 1.0f, z0 + 0.05f), 0.03f);
            // tunnel: walls, vault (flat), end wall, floor; from the surround's back plane inward
            float zf = z0 + 0.05f;                                                  // behind the surround's inner faces
            G1Mesh.Quad(conc, new Vector3(xc - hw, 0f, zf), new Vector3(xc - hw, h, zf), new Vector3(xc - hw, h, z1), new Vector3(xc - hw, 0f, z1), Vector3.right);
            G1Mesh.Quad(conc, new Vector3(xc + hw, 0f, zf), new Vector3(xc + hw, h, zf), new Vector3(xc + hw, h, z1), new Vector3(xc + hw, 0f, z1), Vector3.left);
            G1Mesh.Quad(conc, new Vector3(xc - hw, h, zf), new Vector3(xc + hw, h, zf), new Vector3(xc + hw, h, z1), new Vector3(xc - hw, h, z1), Vector3.down);
            G1Mesh.Quad(conc, new Vector3(xc - hw, 0f, z1), new Vector3(xc - hw, h, z1), new Vector3(xc + hw, h, z1), new Vector3(xc + hw, 0f, z1), Vector3.back);
            G1Mesh.Box(floor, new Vector3(xc - hw, -1f, z0), new Vector3(xc + hw, 0f, z1), G1Mesh.PY);
            // a dim bulkhead lamp on the tunnel's west wall 3 m in (a light pool at the mouth, not a beacon)
            var lamp = parts.Get("depotLamp", G1Mat.EmitLamp, origin);
            G1Mesh.Chamfer(lamp, new Vector3(xc - hw + 0.02f, 4.6f, z0 + 2.7f), new Vector3(xc - hw + 0.14f, 4.85f, z0 + 3.3f), 0.01f);
            // its light (DepotLight) is BoulevardLights' (the beat-2 light budget)
            // a small sign over the portal (NocNeon, warm white)
            parts.Sign(new Vector3(xc, h + 1.75f, z0 - 0.38f), Quaternion.identity, 3.2f, 0.55f, 5);
        }
    }
}
