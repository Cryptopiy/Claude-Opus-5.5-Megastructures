// CityTests: EditMode tests of the generic city (MASTER_PROMPT 14.7 H2 tiers, 14.4 places, 14.6, Part 10 item 34) and of the H7
// lane system (NocTraffic):
//   ring sizes (707 cells of 120 m to 1.8 km, ~2 100 aggregates of 240 m to 6.5 km), every hero volume of 14.4 kept clear (path
//   corridor, boulevard street and lanes, Gulf between its flaring faces, garden, screen wall, Heart R 456 and the ring towers),
//   chunk-local origins within 200 m, ledges above the eye, 25 % rotated 15-45 deg on the 0.5 deg lattice, the triangle budget;
//   T1 / T2 crossover at 1.8 km: a ray-cast render (facade mean emission) of the tier boundary with ring 2 built as aggregates and
//   with ring 1 extended through it, mean linear radiance within 3 % (D-018: identical by construction);
//   traffic: phases continuous across the 65 536 s time wrap, alpha ramps at T < 0.2 or inside a portal at the boulevard key
//   poses, streak lengths 14 / 7 / 3.5 / 2.2 px at 100 / 200 / 400 / 800 m (H7), 1 720 boulevard vehicles.
using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Megastructure.Tests
{
    public class CityTests
    {
        static List<CityTower> AllTowers()
        {
            var all = new List<CityTower>(10000);
            CityField.CellRange(out int i0, out int i1, out int j0, out int j1);
            for (int I = i0; I <= i1; I++)
                for (int J = j0; J <= j1; J++)
                {
                    int ring = CityField.RingOf(I, J);
                    if (ring > 0) CityField.CellTowers(I, J, ring, all);
                }
            return all;
        }

        [Test]
        public void RingSizes_MatchTheSpec()
        {
            CityField.CellRange(out int i0, out int i1, out int j0, out int j1);
            int c1 = 0, c2 = 0;
            for (int I = i0; I <= i1; I++)
                for (int J = j0; J <= j1; J++) { int r = CityField.RingOf(I, J); if (r == 1) c1++; else if (r == 2) c2++; }
            CityField.Count(out _, out _, out int t1, out int t2);
            Debug.Log(FormattableString.Invariant($"[NOC] city rings: {4 * c1} T1 cells of 120 m (spec 707), {c2} T2 cells of 240 m (spec ~2 100); after the hero exclusions {t1} T1 towers, {t2} T2 towers"));
            Assert.AreEqual(707, 4 * c1, 707 * 0.05, "ring 1: 120 m cells to 1.8 km");
            Assert.AreEqual(2127, c2, 2127 * 0.05, "ring 2: 240 m cells 1.8 - 6.5 km");
            Assert.Greater(t1, 250); Assert.Greater(t2, 4000);
        }

        [Test]
        public void HeroVolumes_StayClear()
        {
            var towers = AllTowers();
            // the walked path with its lanes: no tower footprint within 40 m of the centre line
            for (double s = 0; s <= NocturneConstants.PathLength; s += 2.0)
            {
                D3 p = NocMath.PathPoint(s);
                foreach (var t in towers)
                {
                    double d = Math.Sqrt((t.cx - p.x) * (t.cx - p.x) + (t.cz - p.z) * (t.cz - p.z)) - t.radius;
                    Assert.Greater(d, 40.0, FormattableString.Invariant($"tower ({t.cx:F0}, {t.cz:F0}) near the path at s {s}"));
                }
            }
            foreach (var t in towers)
            {
                // boulevard street (z 120..270) west of the Gulf: the lanes of 14.5.2 fly here
                if (t.cx < 230) Assert.IsTrue(t.cz + t.radius < 110 || t.cz - t.radius > 280, "tower in the boulevard street");
                // the Gulf between the faces (both flaring) plus the terrace margin
                Assert.IsTrue(t.cx + t.radius < CityField.GulfXA(t.cz) - 100 || t.cx - t.radius > CityField.GulfXB(t.cz) + 100, "tower in the Gulf");
                double hd = Math.Sqrt((t.cx - NocturneConstants.SpireX) * (t.cx - NocturneConstants.SpireX) + (t.cz - NocturneConstants.SpireZ) * (t.cz - NocturneConstants.SpireZ));
                Assert.Greater(hd - t.radius, 600.0, "Heart plaza R 400, ring towers R 470-560 (CityField excludes R < 456)");
                foreach (var r in CityField.Rects)
                {
                    bool inside = t.cx > r.x0 - t.radius && t.cx < r.x1 + t.radius && t.cz > r.z0 - t.radius && t.cz < r.z1 + t.radius;
                    if (inside)
                    {
                        double dx = Math.Max(r.x0 - t.cx, Math.Max(0, t.cx - r.x1)), dz = Math.Max(r.z0 - t.cz, Math.Max(0, t.cz - r.z1));
                        Assert.GreaterOrEqual(dx * dx + dz * dz, t.radius * t.radius, "tower overlaps " + r.name);
                    }
                }
            }
        }

        [Test]
        public void Towers_Shape_Rotation_Ledges_ChunkOrigins()
        {
            var towers = AllTowers();
            int rot = 0;
            double cell = NocturneConstants.CityCell2, maxR = 0;
            foreach (var t in towers)
            {
                if (t.yaw != 0)
                {
                    rot++;
                    double a = Math.Abs(t.yaw);
                    Assert.IsTrue(a >= 15 && a <= 45, "rotation 15-45 deg");
                    Assert.AreEqual(0.0, a / 0.5 - Math.Round(a / 0.5), 1e-9, "yaw on the 0.5 deg face lattice");
                }
                Assert.AreEqual(-3300.0, t.boxes[0].y0); Assert.AreEqual(560.0, t.boxes[2].y1);
                Assert.Greater(t.boxes[1].y0, 25.0, "every ledge above the eye (no roof is ever seen)");
                Assert.Greater(t.boxes[2].y0 - t.boxes[1].y0, 50.0);
                for (int b = 1; b < 3; b++) { Assert.Greater(t.boxes[b].hx, 0.55 * t.boxes[0].hx); Assert.Greater(t.boxes[b].hz, 0.55 * t.boxes[0].hz); }
                int I = (int)Math.Floor(t.ci / 2.0), J = (int)Math.Floor(t.cj / 2.0);
                double ox = (I + 0.5) * cell, oz = (J + 0.5) * cell;
                for (int b = 0; b < 3; b++)
                    for (int sx = -1; sx <= 1; sx += 2)
                        for (int sz = -1; sz <= 1; sz += 2)
                        {
                            t.Corner(b, sx, sz, out double x, out double z);
                            maxR = Math.Max(maxR, Math.Sqrt((x - ox) * (x - ox) + (z - oz) * (z - oz)));
                        }
            }
            double frac = (double)rot / towers.Count;
            Debug.Log(FormattableString.Invariant($"[NOC] city towers {towers.Count}: rotated {100 * frac:F1} %, max vertex distance to its chunk origin {maxR:F1} m"));
            Assert.AreEqual(0.25, frac, 0.03, "25 % rotated");
            Assert.LessOrEqual(maxR, 200.0, "chunk-local origins <= 200 m (Part 7.5)");
        }

        [Test]
        public void TriangleBudget_AndBackFaceCull()
        {
            var towers = AllTowers();
            var faces = new CityField.Face[4];
            int kept = 0, culled = 0;
            foreach (var t in towers)
                for (int b = 0; b < 3; b++)
                {
                    CityField.Faces(t, b, faces);
                    for (int f = 0; f < 4; f++) { if (CityField.FacesWalk(faces[f])) kept++; else culled++; }
                }
            Debug.Log(FormattableString.Invariant($"[NOC] city faces kept {kept}, culled {culled} (back faces for every camera), triangles {2 * kept}"));
            Assert.Less(2 * kept, 160000, "city triangles (D-018 budget)");
            // a culled face must be a back face for every point of the walk region (sampled densely along the path)
            int checkedFaces = 0;
            foreach (var t in towers)
            {
                if (checkedFaces > 3000) break;
                for (int b = 0; b < 3; b++)
                {
                    CityField.Faces(t, b, faces);
                    for (int f = 0; f < 4; f++)
                    {
                        if (CityField.FacesWalk(faces[f])) continue;
                        checkedFaces++;
                        for (double s = 0; s <= NocturneConstants.PathLength; s += 25)
                            for (double lat = -12; lat <= 12; lat += 12)
                            {
                                D3 p = NocMath.CorridorToWorld(s, lat, 1.7);
                                Assert.LessOrEqual((p.x - faces[f].ax) * faces[f].nx + (p.z - faces[f].az) * faces[f].nz, 1.0, "culled face seen from s " + s);
                            }
                    }
                }
            }
        }

        // ---------------------------------------------------------------- T1 / T2 crossover (Part 10 item 34)
        static bool HitBox(CityTower t, int b, double ox, double oy, double oz, double dx, double dy, double dz, out double tHit, out double nx, out double nz)
        {
            tHit = double.MaxValue; nx = nz = 0;
            CityBox q = t.boxes[b];
            double r = t.yaw * Math.PI / 180, c = Math.Cos(r), s = Math.Sin(r);
            // ray into the box frame (local x axis (c, -s), local z axis (s, c)), centred on the box centre
            double bx = t.cx + q.ox * c + q.oz * s, bz = t.cz - q.ox * s + q.oz * c;
            double lx = (ox - bx) * c - (oz - bz) * s, lz = (ox - bx) * s + (oz - bz) * c;
            double ldx = dx * c - dz * s, ldz = dx * s + dz * c;
            double t0 = 0, t1 = 1e9;
            int axis = -1; double sign = 0;
            if (!Slab(lx, ldx, q.hx, ref t0, ref t1, 0, ref axis, ref sign)) return false;
            if (!Slab(lz, ldz, q.hz, ref t0, ref t1, 1, ref axis, ref sign)) return false;
            double y = oy + dy * t0;
            if (axis < 0 || y < q.y0 || y > q.y1) return false;
            tHit = t0;
            double lnx = axis == 0 ? sign : 0, lnz = axis == 1 ? sign : 0;
            nx = lnx * c + lnz * s; nz = -lnx * s + lnz * c;
            return true;
        }

        static bool Slab(double o, double d, double h, ref double t0, ref double t1, int ax, ref int axis, ref double sign)
        {
            if (Math.Abs(d) < 1e-12) return Math.Abs(o) <= h;
            double a = (-h - o) / d, b = (h - o) / d;
            double sg = -1;
            if (a > b) { double tmp = a; a = b; b = tmp; sg = 1; }
            if (a > t0) { t0 = a; axis = ax; sign = sg; }
            t1 = Math.Min(t1, b);
            return t0 <= t1;
        }

        static double RenderMean(List<CityTower> towers, double camX, double camZ, double yawDeg, double pitchDeg)
        {
            const int W = 160, H = 90;
            double f = (H / 2.0) / Math.Tan(33.0 * Math.PI / 180), sum = 0;
            double yr = yawDeg * Math.PI / 180, pr = pitchDeg * Math.PI / 180;
            var faceArr = new CityField.Face[4];
            for (int j = 0; j < H; j++)
                for (int i = 0; i < W; i++)
                {
                    double x = (i + 0.5 - W / 2.0) / f, y = (H / 2.0 - j - 0.5) / f;
                    // camera: forward (sin yaw cos pitch, sin pitch, cos yaw cos pitch)
                    double fx = Math.Sin(yr) * Math.Cos(pr), fy = Math.Sin(pr), fz = Math.Cos(yr) * Math.Cos(pr);
                    double rx = Math.Cos(yr), rz = -Math.Sin(yr);
                    double ux = -Math.Sin(yr) * Math.Sin(pr), uy = Math.Cos(pr), uz = -Math.Cos(yr) * Math.Sin(pr);
                    double dx = fx + x * rx + y * ux, dy = fy + y * uy, dz = fz + x * rz + y * uz;
                    double l = Math.Sqrt(dx * dx + dy * dy + dz * dz); dx /= l; dy /= l; dz /= l;
                    double best = double.MaxValue, bnx = 0, bnz = 0; CityTower bt = null;
                    foreach (var t in towers)
                        for (int b = 0; b < 3; b++)
                            if (HitBox(t, b, camX, 1.7, camZ, dx, dy, dz, out double th, out double nx, out double nz) && th < best) { best = th; bnx = nx; bnz = nz; bt = t; }
                    if (bt == null) continue;
                    double hx = camX + dx * best, hy = 1.7 + dy * best, hz = camZ + dz * best;
                    double yawN = Math.Atan2(bnx, bnz) * 180 / Math.PI;
                    yawN = Math.Round(yawN / 0.5) * 0.5;
                    // TP-2: the v1 Macro is deleted; the Look v2 stratum mean of the band (per-face lit factor x the window
                    // hue mean) stands in for the face's unresolved emission
                    uint fid = NocFacadeMath.FaceId(yawN, hx * bnx + hz * bnz);
                    double fac = NocFacadeMath.FactorV2(NocFacadeMath.StratumV2(fid, (int)Math.Floor(hy / NocturneConstants.FacadeBand)));
                    C3 hue = NocFacadeMath.MeanHueV2(0.1, 0.0);
                    C3 m = new C3(hue.r * fac, hue.g * fac, hue.b * fac);
                    sum += 0.2126 * m.r + 0.7152 * m.g + 0.0722 * m.b;
                }
            return sum / (W * H);
        }

        [Test]
        public void Tiers_At1800m_MatchWithin3Percent()
        {
            // a view from the alley end of the path toward the west where the 1.8 km boundary crosses open city
            double camX = -20, camZ = 60, yaw = 250;
            var tiered = new List<CityTower>(); var t1only = new List<CityTower>();
            CityField.CellRange(out int i0, out int i1, out int j0, out int j1);
            for (int I = i0; I <= i1; I++)
                for (int J = j0; J <= j1; J++)
                {
                    int ring = CityField.RingOf(I, J);
                    if (ring == 0) continue;
                    double cx = (I + 0.5) * 240 - camX, cz = (J + 0.5) * 240 - camZ, d = Math.Sqrt(cx * cx + cz * cz);
                    if (d < 1100 || d > 2600) continue;
                    double az = Math.Atan2(cx, cz) * 180 / Math.PI; if (az < 0) az += 360;
                    if (Math.Abs(az - yaw) > 50) continue;
                    CityField.CellTowers(I, J, ring, tiered);            // ring 2 as aggregates of 240 m cells
                    for (int a = 0; a < 2; a++)                            // ring 1 extended: every 120 m cell as a T1 tower
                        for (int b = 0; b < 2; b++)
                            if (CityField.TowerAt(2 * I + a, 2 * J + b, out CityTower t)) { t.tier = 1; t1only.Add(t); }
                }
            int nT2 = 0; foreach (var t in tiered) if (t.tier == 2) nT2++;
            Assert.Greater(nT2, 20, "the view contains ring-2 aggregates");
            double mTier = RenderMean(tiered, camX, camZ, yaw, 2), mT1 = RenderMean(t1only, camX, camZ, yaw, 2);
            double rel = Math.Abs(mTier - mT1) / Math.Max(mT1, 1e-9);
            Debug.Log(FormattableString.Invariant($"[NOC] T1/T2 crossover at 1.8 km: mean linear radiance tiered {mTier:E4}, ring 1 extended {mT1:E4}, difference {100 * rel:F3} % ({nT2} T2 towers in view)"));
            Assert.Greater(mT1, 0);
            Assert.Less(rel, 0.03, "both tiers at 1.8 km within 3 % (Part 10 item 34, H2)");
        }

        // ---------------------------------------------------------------- traffic (H7)
        [Test]
        public void Traffic_WrapContinuity_RampsHidden_StreakLengths()
        {
            var east = new List<TrafficLane>(); var west = new List<TrafficLane>();
            NocTraffic.BoulevardLanes(east, west);
            var lanes = new List<TrafficLane>(east); lanes.AddRange(west);
            Assert.AreEqual(10, lanes.Count);
            var phis = new List<double>(); var vs = new List<double>();
            int total = 0;
            var st = new NocAtmoState();
            foreach (var lane in lanes)
            {
                phis.Clear(); vs.Clear();
                NocTraffic.Vehicles(lane, 11u, phis, vs);
                total += phis.Count;
                double L = lane.Length;
                for (int i = 0; i < phis.Count; i++)
                {
                    Assert.AreEqual(NocTraffic.Phase(phis[i], vs[i], L, 0.0), NocTraffic.Phase(phis[i], vs[i], L, NocturneConstants.TrafficTimeWrap), 1e-7, "no jump at the time wrap");
                    Assert.AreEqual(14.0, vs[i], 14.0 * 0.03 + L / NocturneConstants.TrafficTimeWrap, "14 m/s +-3 % (+ the wrap quantum L / P)");
                }
                // every partially transparent vehicle at the boulevard key poses is in haze (T < 0.2) or inside face B's portal
                foreach (string tag in new[] { "KS03_boulevard_mouth", "KS04_boulevard_axis", "KS06_tram", "KS05_boulevard_up" })
                {
                    KeyShot k = Array.Find(NocturneConstants.KeyShots, q => q.tag == tag);
                    NocFogMath.StateAt(st, k.s, 0.0, 1.0, true);
                    st.CamX = k.x; st.CamY = k.y; st.CamZ = k.z;
                    for (double p = 0.001; p < 1.0; p += 0.001)
                    {
                        double a = NocTraffic.Alpha(p, lane.ramp);
                        if (a <= 0.02 || a >= 0.98) continue;
                        double x = lane.ax + (lane.bx - lane.ax) * p, y = lane.ay, z = lane.az;
                        bool portal = x >= CityField.GulfXB(z) - 1e-6;
                        double T = NocFogMath.FogT(st, x, y, z);
                        Assert.IsTrue(portal || T < 0.2, FormattableString.Invariant($"{lane.name} ramp at x {x:F0} visible at {tag}: T {T:F3}"));
                    }
                }
            }
            Assert.AreEqual(1720, total, 1720 * 0.05, "boulevard vehicles (H7: 1 720; the lanes run on through the portal ramp, +4 %)");
            // streak lengths at 1080p, vFOV 66 (f = 832 px): 14 m/s -> 14, 7, 3.5, 2.2 px at 100, 200, 400, 800 m
            double f = 540.0 / Math.Tan(33.0 * Math.PI / 180);
            double[] dist = { 100, 200, 400, 800 }, want = { 14, 7, 3.5, 2.2 };
            for (int i = 0; i < 4; i++)
            {
                double fp = dist[i] / f;
                Assert.AreEqual(want[i], NocTraffic.StreakLength(14.0, fp) / fp, 0.15, "streak px at " + dist[i] + " m");
            }
        }
    }
}
