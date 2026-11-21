// VisibilityAudit: the 11.13 visibility and reachability audit as a reusable pure class (MASTER_PROMPT 11.13, 14.10.2,
// 14.5 key views, Part 10 items 35 and 39). No scene access: the caller may pass an occlusion tester (the EditMode test
// ray-casts against the real built geometry of the beat builders); the analytic rule of 11.13 for subjects below the
// horizon (a subject at elevation e is visible from eye height h only if the nearest open edge of the walkable floor along
// its azimuth is closer than h / tan|e|) is built in, from the spec's floor layout (14.4: crest deck x -6..6 with the
// see-through spillway gratings over the lane, gallery floor to the trench edge at axis + 3.0, ledge slab x -12.4..-6.0
// z 1415.2..1503, enclosed elsewhere).
//
// For every key shot of 14.10.2 (BulwarkConst.KeyShots) and every named subject of Part 14 (spec bounds, catalogue below;
// subjects not built yet are evaluated from their specified bounds; assumptions are written into each subject's note):
//   - projects the subject through the real key-shot pose (position, yaw, pitch, vFOV; Hor+) at 16:9, 16:10 and 21:9 with the
//     3 % margin (boxes are clipped at the near plane edge by edge: exact screen bounds of the visible part);
//   - sizes in px at 1080p and 4K (16:9), expected rows / columns / sizes of the spec compared with their tolerance;
//   - occlusion: unoccluded fraction from the caller's tester (built geometry) and from the analytic floor rule;
//   - the pose: inside BwPath.Lane, on the cinematic path (lateral = CinematicLateral, eye = EyeY);
//   - the AutoWalker follower: the real WalkPlanner + GazeSchedule simulated from 40 m earlier at 2.5 m/s (1/60 s steps,
//     seed 1), its pose and lag at the key shot's s, and every subject re-projected at the follower pose; lag > 2 deg is a
//     finding;
//   - a reachability sweep of the named subjects along the whole cinematic walk (follower pose every 1 m): seconds on screen
//     at 2.5 m/s, fully inside, unoccluded.
// Spec-level impossibilities are reported as findings with their arithmetic (the lead decides; Part 1.6, 14.10.5).
using System;
using System.Collections.Generic;
using System.Globalization;
using Megastructure.Player;

namespace Megastructure.QA
{
    public static class VisibilityAudit
    {
        public const double Margin = 0.03;
        public const double MaxFollowerLagDeg = 2.0;
        public const double FollowerLeadIn = 40.0;
        public const double FollowerSpeed = 2.5;
        public const double FollowerDt = 1.0 / 60.0;
        public const double MinUnoccluded = 0.30;   // CompositionLint.MIN_VISIBLE
        const double Near = 0.05;

        public enum SubjectKind { Box, Points, Directions }
        public enum Rule { Inside, InFrame, Fills, Absent, Row, Info }

        public struct Aspect
        {
            public string Name; public int W, H;
            public Aspect(string n, int w, int h) { Name = n; W = w; H = h; }
        }
        /// <summary>16:9 (1080p; 4K has the same normalised frame), 16:10, 21:9 (11.2).</summary>
        public static readonly Aspect[] Aspects = { new Aspect("16:9", 1920, 1080), new Aspect("16:10", 1680, 1050), new Aspect("21:9", 2560, 1080) };

        /// <summary>A camera pose (world yaw from +Z toward +X, pitch up positive, vertical FOV, Hor+).</summary>
        public struct Pose
        {
            public double x, y, z, yaw, pitch, fov;
            public Pose(double x, double y, double z, double yaw, double pitch, double fov) { this.x = x; this.y = y; this.z = z; this.yaw = yaw; this.pitch = pitch; this.fov = fov; }
            public D3 Eye => new D3(x, y, z);
        }

        /// <summary>A named subject with its spec bounds and the shot rule.</summary>
        public sealed class Subject
        {
            public string Name, Note = "", Source = "";
            public SubjectKind Kind = SubjectKind.Box;
            public Rule Rule = Rule.Inside;
            public bool MustBeVisible = true;
            public D3 Min, Max;               // Box
            public D3[] Points;               // world points (Points) or unit directions (Directions)
            public double Reach = 20000.0;    // Directions: length of the occlusion rays (m)
            public bool BelowHorizon;         // apply the analytic floor rule of 11.13
            public double ExpectRow4K = double.NaN, ExpectCol4K = double.NaN, PosTol4K = double.NaN;
            public double ExpectHeight4K = double.NaN, ExpectWidth4K = double.NaN, SizeTol4K = double.NaN;
            public double MinHeight4K = double.NaN, MinWidth4K = double.NaN;
            public double SweepS0 = double.NaN, SweepS1 = double.NaN;   // reachability sweep window (named subjects)
            public double MinVisible = MinUnoccluded;   // long thin subjects seen through an opening use a lower floor
            public double SpecSeconds = double.NaN;     // sweep: the spec's on-screen time at 2.5 m/s (compared with the visible time)
            public bool SpecSecondsWhole;               // SpecSeconds counts the whole subject inside the frame (bow), not any part
            public Func<double, Subject> AtS;  // moving subjects (whale): the subject at progress s (sweep)
            public string FillsExemptAspect;   // Rule.Fills: an aspect accepted by a 14.10.5 ruling (KS15 at 21:9)
        }

        /// <summary>Occlusion tester: true when the segment from origin along the unit direction is blocked within maxDist;
        /// blocker names what was hit (renderer name and distance) for the report.</summary>
        public delegate bool OcclusionTester(D3 origin, D3 dir, double maxDist, out string blocker);

        // ================================================================ maths
        public static void Basis(double yaw, double pitch, out D3 f, out D3 r, out D3 u) => QaProjection.Basis(yaw, pitch, out f, out r, out u);

        public static D3 AzEl(double azDeg, double elDeg) => QaProjection.AzElDir(azDeg, elDeg);

        /// <summary>Dip (deg) of the horizon of a sphere of radius R seen from height h above it (stable form, 14.7 H4).</summary>
        public static double DipDeg(double h, double R)
        {
            h = Math.Max(h, 0.0);
            double s2 = h * (2.0 * R + h) / ((R + h) * (R + h));
            return Math.Asin(Math.Sqrt(Math.Max(0.0, Math.Min(1.0, s2)))) / BwMath.Deg;
        }
        public static double SeaDipDeg(double eyeY) => DipDeg(eyeY - BulwarkConst.SeaY, BulwarkConst.EarthRadius + BulwarkConst.SeaSphereOffset);
        public static double LowlandDipDeg(double eyeY) => DipDeg(eyeY - BulwarkConst.LowlandY, BulwarkConst.EarthRadius);

        /// <summary>Camera-space coordinates (right, up, forward) of a world vector.</summary>
        static void Cam(in Pose p, D3 v, out double cx, out double cy, out double cz)
        {
            Basis(p.yaw, p.pitch, out D3 f, out D3 r, out D3 u);
            cx = D3.Dot(v, r); cy = D3.Dot(v, u); cz = D3.Dot(v, f);
        }

        static double TanV(in Pose p) => Math.Tan(0.5 * p.fov * BwMath.Deg);

        /// <summary>Row (px, top origin) of a world direction at a frame height, NaN when behind.</summary>
        public static double RowOf(in Pose p, D3 dir, int heightPx)
        {
            Cam(p, dir, out _, out double cy, out double cz);
            if (cz <= 1e-9) return double.NaN;
            double ndcY = cy / (cz * TanV(p));
            return (1.0 - ndcY) * 0.5 * heightPx;
        }

        /// <summary>Column (px, left origin) of a world direction at a frame size, NaN when behind.</summary>
        public static double ColOf(in Pose p, D3 dir, int widthPx, int heightPx)
        {
            Cam(p, dir, out double cx, out _, out double cz);
            if (cz <= 1e-9) return double.NaN;
            double ndcX = cx / (cz * TanV(p) * widthPx / heightPx);
            return (ndcX + 1.0) * 0.5 * widthPx;
        }

        /// <summary>Projected extent of a subject in NDC (y up) at an aspect.</summary>
        public struct Extent
        {
            public bool Any, PartlyBehind;
            public int Samples, InFront, Inside;
            public double X0, Y0, X1, Y1;
            public double InsideFraction => Samples > 0 ? (double)Inside / Samples : 0.0;
            public bool InsideMargin(double m) => Any && !PartlyBehind && X0 >= -1 + 2 * m && X1 <= 1 - 2 * m && Y0 >= -1 + 2 * m && Y1 <= 1 - 2 * m;
            public bool Touches => Any && X1 > -1 && X0 < 1 && Y1 > -1 && Y0 < 1;
        }

        static readonly D3[] s_corner = new D3[8];

        public static Extent Project(Subject sub, in Pose p, double aspect)
        {
            var e = new Extent { X0 = double.MaxValue, Y0 = double.MaxValue, X1 = double.MinValue, Y1 = double.MinValue };
            double tv = TanV(p), th = tv * aspect;
            void Add(double cx, double cy, double cz)
            {
                double z = Math.Max(cz, 1e-9);
                double nx = cx / (z * th), ny = cy / (z * tv);
                if (nx < e.X0) e.X0 = nx; if (nx > e.X1) e.X1 = nx;
                if (ny < e.Y0) e.Y0 = ny; if (ny > e.Y1) e.Y1 = ny;
                e.Any = true;
            }
            D3 eye = p.Eye;
            if (sub.Kind == SubjectKind.Box)
            {
                for (int i = 0; i < 8; i++)
                {
                    var c = new D3((i & 1) != 0 ? sub.Max.x : sub.Min.x, (i & 2) != 0 ? sub.Max.y : sub.Min.y, (i & 4) != 0 ? sub.Max.z : sub.Min.z);
                    Cam(p, c - eye, out double cx, out double cy, out double cz);
                    s_corner[i] = new D3(cx, cy, cz);
                }
                e.Samples = 8;
                for (int i = 0; i < 8; i++)
                {
                    if (s_corner[i].z >= Near)
                    {
                        e.InFront++;
                        Add(s_corner[i].x, s_corner[i].y, s_corner[i].z);
                        double nx = s_corner[i].x / (s_corner[i].z * th), ny = s_corner[i].y / (s_corner[i].z * tv);
                        if (Math.Abs(nx) <= 1 && Math.Abs(ny) <= 1) e.Inside++;
                    }
                    else e.PartlyBehind = true;
                }
                for (int i = 0; i < 8; i++)
                    for (int bit = 1; bit <= 4; bit <<= 1)
                    {
                        if ((i & bit) != 0) continue;
                        D3 a = s_corner[i], c = s_corner[i | bit];
                        if ((a.z >= Near) == (c.z >= Near)) continue;
                        double k = (Near - a.z) / (c.z - a.z);
                        Add(a.x + (c.x - a.x) * k, a.y + (c.y - a.y) * k, Near);
                    }
            }
            else
            {
                bool dirs = sub.Kind == SubjectKind.Directions;
                foreach (D3 q in sub.Points)
                {
                    e.Samples++;
                    Cam(p, dirs ? q : q - eye, out double cx, out double cy, out double cz);
                    if (cz < (dirs ? 1e-9 : Near)) { e.PartlyBehind = true; continue; }
                    e.InFront++;
                    Add(cx, cy, cz);
                    double nx = cx / (cz * th), ny = cy / (cz * tv);
                    if (Math.Abs(nx) <= 1 && Math.Abs(ny) <= 1) e.Inside++;
                }
            }
            if (!e.Any) { e.X0 = e.Y0 = e.X1 = e.Y1 = 0; }
            return e;
        }

        /// <summary>Unit direction of the view ray through NDC (x, y) of a pose at an aspect.</summary>
        public static D3 RayDir(in Pose p, double aspect, double ndcX, double ndcY)
        {
            Basis(p.yaw, p.pitch, out D3 f, out D3 r, out D3 u);
            double tv = TanV(p), th = tv * aspect;
            return (f + r * (ndcX * th) + u * (ndcY * tv)).Normalized;
        }

        /// <summary>Slab test: entry / exit parameters of the ray o + t d against a box.</summary>
        public static bool RayBox(D3 o, D3 d, D3 mn, D3 mx, out double tIn, out double tOut)
        {
            tIn = double.NegativeInfinity; tOut = double.PositiveInfinity;
            for (int a = 0; a < 3; a++)
            {
                double oa = a == 0 ? o.x : (a == 1 ? o.y : o.z), da = a == 0 ? d.x : (a == 1 ? d.y : d.z);
                double lo = a == 0 ? mn.x : (a == 1 ? mn.y : mn.z), hi = a == 0 ? mx.x : (a == 1 ? mx.y : mx.z);
                if (Math.Abs(da) < 1e-12) { if (oa < lo || oa > hi) return false; continue; }
                double t0 = (lo - oa) / da, t1 = (hi - oa) / da;
                if (t0 > t1) { double k = t0; t0 = t1; t1 = k; }
                if (t0 > tIn) tIn = t0;
                if (t1 < tOut) tOut = t1;
                if (tIn > tOut) return false;
            }
            return tOut >= 0;
        }

        // ================================================================ analytic floor rule (11.13)
        /// <summary>True when the spec's walkable floor at height floorY is solid at plan point (x, z) for an eye at progress
        /// sEye: crest deck x -6..6 except the spillway grating spans (lane width, see-through); window gallery floor from the
        /// west wall to the trench edge (axis + 3.0); the ledge slab x -12.4..-6.0, z 1415.2..1503; enclosed sections solid.</summary>
        public static bool FloorSolidAt(double sEye, double x, double z)
        {
            if (sEye >= BulwarkConst.L1EndS - 0.5 && sEye < BulwarkConst.TowerZ0)
            {
                if (x < BulwarkConst.RoadXMin || x > BulwarkConst.RoadXMax) return false;
                if (z >= BulwarkConst.SlotZ0 && z <= BulwarkConst.SpillwayZ1)
                {
                    double u = (z - BulwarkConst.SlotZ0) % BulwarkConst.PierPitch;
                    bool slot = u <= BulwarkConst.SlotLength && z < BulwarkConst.SlotZ0 + BulwarkConst.PierPitch * BulwarkConst.SlotCount;
                    double g0 = BulwarkConst.LaneRoadXMin - BulwarkConst.PlayerR, g1 = BulwarkConst.LaneRoadXMax + BulwarkConst.PlayerR;
                    if (slot && x > g0 && x < g1) return false;   // grating over the lane width: see-through
                }
                return true;
            }
            if (sEye >= BulwarkConst.GalleryStartS && sEye < BulwarkConst.HallEndWallS)
            {
                double s = BwPath.Project(x, z, sEye);
                double lat = BwPath.Lateral(x, z, s);
                return lat >= BulwarkConst.HallWestWall && lat <= BulwarkConst.HallRail;
            }
            if (sEye >= BulwarkConst.TunnelMouthS - 0.5)
            {
                if (x >= BulwarkConst.LeeFaceX) return true;   // the tunnel floor inside the dam
                return x >= BulwarkConst.LedgeXMin && z >= BulwarkConst.LeeFaceCrossZ - 0.5 * BulwarkConst.TunnelMouthCut && z <= BulwarkConst.LedgeZ1;
            }
            return true;
        }

        /// <summary>11.13 analytic rule for a ray below the horizon: blocked when the ray meets the floor level (eye height h above
        /// it) on solid floor. dEdge = horizontal distance to the first open floor point along the azimuth (NaN: none in 60 m).</summary>
        public static bool FloorBlocks(double sEye, D3 eye, D3 dir, bool wantEdge, out double dEdge, out double dHit)
        {
            double floorY = BwPath.FloorY(sEye);
            double h = eye.y - floorY;
            dEdge = double.NaN; dHit = double.NaN;
            double hl = Math.Sqrt(dir.x * dir.x + dir.z * dir.z);
            if (wantEdge && hl > 1e-9)
            {
                double ax = dir.x / hl, az = dir.z / hl;
                for (double d = 0.02; d <= 60.0; d += 0.02)
                    if (!FloorSolidAt(sEye, eye.x + ax * d, eye.z + az * d)) { dEdge = d; break; }
            }
            if (dir.y >= -1e-9 || h <= 0) return false;
            double t = h / -dir.y;
            dHit = t * hl;
            return FloorSolidAt(sEye, eye.x + dir.x * t, eye.z + dir.z * t);
        }

        // ================================================================ follower (the real AutoWalker core)
        public struct Follower
        {
            public double sStart, s, lateral, x, y, z, yaw, pitch;
            public double lagDeg, yawLagDeg, pitchLagDeg, posErr;
            public bool fromRest;
            public Pose ToPose(double fov) => new Pose(x, y, z, yaw, pitch, fov);
        }

        /// <summary>Simulates the cinematic follower (WalkPlanner + GazeSchedule, seed) from <paramref name="leadIn"/> m before
        /// sTarget at the cinematic speed (from rest when that start lies before s 0), 1/60 s steps; the last step lands on sTarget.</summary>
        public static Follower SimulateFollower(double sTarget, in Pose target, int seed = 1, double leadIn = FollowerLeadIn, double speed = FollowerSpeed)
        {
            var f = new Follower();
            double s0 = sTarget - leadIn;
            f.fromRest = s0 < 0.0;
            if (f.fromRest) s0 = 0.0;
            f.sStart = s0;
            var w = new WalkPlanner(seed);
            double lead = 2.0 / WalkPlanner.LookOmega * speed;
            GazeSchedule.Evaluate(Math.Min(BwPath.Length, s0 + (f.fromRest ? 0.0 : lead)), out double y0, out double p0);
            if (f.fromRest) { y0 = BwPath.HeadingDeg(0.0); p0 = 0.0; }
            w.Begin(s0, f.fromRest ? 0.0 : speed, BwPath.CinematicLateral(s0), y0, p0, speed);
            double s = s0, t = s0 / speed;
            int guard = 0;
            while (s < sTarget - 1e-9 && guard++ < 200000)
            {
                double v = Math.Max(1e-6, w.SpeedCommand > 0 ? WalkPlanner.RampSpeed(w.V0, w.Cruise, w.Elapsed + FollowerDt) : speed);
                double dt = FollowerDt;
                if (s + v * dt > sTarget) dt = Math.Max(1e-6, (sTarget - s) / v);
                w.Step(dt, s, t);
                s += w.SpeedCommand * dt;
                t += dt;
            }
            f.s = s;
            f.lateral = w.LateralTarget;
            BwPath.World(s, f.lateral, out f.x, out _, out f.z);
            f.y = BwPath.EyeY(s);
            f.yaw = w.OutYaw; f.pitch = w.OutPitch;
            D3 a = AzEl(f.yaw, f.pitch), b = AzEl(target.yaw, target.pitch);
            f.lagDeg = Math.Acos(Math.Max(-1.0, Math.Min(1.0, D3.Dot(a, b)))) / BwMath.Deg;
            f.yawLagDeg = BwMath.DeltaDeg(target.yaw, f.yaw);
            f.pitchLagDeg = f.pitch - target.pitch;
            f.posErr = (new D3(f.x, f.y, f.z) - target.Eye).Length;
            return f;
        }

        /// <summary>The cinematic walk from s 0 (rest) to the goal, sampled every <paramref name="step"/> m of s: follower poses.</summary>
        public static List<Pose> SimulateWalk(double step, List<double> sOut, int seed = 1, double speed = FollowerSpeed)
        {
            var list = new List<Pose>((int)(BwPath.Length / step) + 2);
            var w = new WalkPlanner(seed);
            w.Begin(0.0, 0.0, BwPath.CinematicLateral(0.0), BwPath.HeadingDeg(0.0), 0.0, speed);
            double s = 0.0, t = 0.0, next = 0.0;
            int guard = 0;
            while (next <= BwPath.Length - 1.0 && guard++ < 400000)
            {
                w.Step(FollowerDt, s, t);
                s += w.SpeedCommand * FollowerDt;
                t += FollowerDt;
                while (s >= next && next <= BwPath.Length - 1.0)
                {
                    BwPath.World(next, w.LateralTarget, out double x, out _, out double z);
                    list.Add(new Pose(x, BwPath.EyeY(next), z, w.OutYaw, w.OutPitch, BulwarkSchedule.Vfov(next)));
                    sOut?.Add(next);
                    next += step;
                }
            }
            return list;
        }

        // ================================================================ catalogue (spec bounds)
        static D3 V(double x, double y, double z) => new D3(x, y, z);
        static Subject Box(string name, double x0, double y0, double z0, double x1, double y1, double z1, Rule rule, string note = "", string source = "")
            => new Subject { Name = name, Kind = SubjectKind.Box, Min = V(Math.Min(x0, x1), Math.Min(y0, y1), Math.Min(z0, z1)), Max = V(Math.Max(x0, x1), Math.Max(y0, y1), Math.Max(z0, z1)), Rule = rule, Note = note, Source = source };
        static Subject Dirs(string name, D3[] dirs, Rule rule, double reach, string note = "", string source = "")
            => new Subject { Name = name, Kind = SubjectKind.Directions, Points = dirs, Rule = rule, Reach = reach, Note = note, Source = source };
        static Subject Pts(string name, D3[] pts, Rule rule, string note = "", string source = "")
            => new Subject { Name = name, Kind = SubjectKind.Points, Points = pts, Rule = rule, Note = note, Source = source };

        static Subject Row(string name, D3 dir, double row4K, double tol, string note, string source)
        {
            var s = Dirs(name, new[] { dir }, Rule.Row, 20000.0, note, source);
            s.ExpectRow4K = row4K; s.PosTol4K = tol;
            return s;
        }

        static D3 PathPoint(double s, double lat, double y) { BwPath.World(s, lat, out double x, out _, out double z); return V(x, y, z); }

        static KeyShot Shot(string id)
        {
            foreach (var k in BulwarkConst.KeyShots) if (k.tag.StartsWith(id, StringComparison.Ordinal)) return k;
            throw new ArgumentException("unknown key shot " + id);
        }

        static bool TryShot(string id, out KeyShot shot)
        {
            foreach (var k in BulwarkConst.KeyShots) if (KeyId(k.tag) == id) { shot = k; return true; }
            shot = default;
            return false;
        }

        static Pose PoseOf(KeyShot k) => new Pose(k.x, k.y, k.z, k.yaw, k.pitch, k.fov);

        /// <summary>Sun direction of 14.2.3 at s (beats 3 and 7 formulas of BulwarkConst, independent of the schedule code).</summary>
        public static void SunAt(double s, out double az, out double el)
        {
            if (s >= BulwarkConst.SunB7AzS0)
            {
                az = BulwarkConst.SunB7Az0 + (BulwarkConst.SunB7Az1 - BulwarkConst.SunB7Az0) * BwMath.Clamp01((s - BulwarkConst.SunB7AzS0) / (BulwarkConst.SunB7AzS1 - BulwarkConst.SunB7AzS0));
                el = BulwarkConst.SunB7El0 - BulwarkConst.SunB7ElDrop * BwMath.Smootherstep01((s - BulwarkConst.SunB7ElS0) / BulwarkConst.SunB7ElWindow);
                return;
            }
            double u = Math.Min(s, BulwarkConst.SunB3S1) - BulwarkConst.SunB3S0;
            az = BulwarkConst.SunB3Az0 + BulwarkConst.SunB3AzRate * u;
            el = BulwarkConst.SunB3El0 + BulwarkConst.SunB3ElRate * u;
        }

        /// <summary>Points of a circle of angular radius rDeg round the unit axis, phi from the top (0) both ways to the given
        /// lowest elevation; n samples evenly in phi between the two horizon crossings.</summary>
        public static D3[] ArcAboveElevation(D3 axis, double rDeg, double minElDeg, int n)
        {
            D3 up = V(0, 1, 0);
            D3 e1 = (up - axis * D3.Dot(up, axis)).Normalized, e2 = D3.Cross(axis, e1);
            double cr = Math.Cos(rDeg * BwMath.Deg), sr = Math.Sin(rDeg * BwMath.Deg);
            D3 At(double phi) => axis * cr + (e1 * Math.Cos(phi) + e2 * Math.Sin(phi)) * sr;
            double ElOf(D3 d) => Math.Asin(Math.Max(-1, Math.Min(1, d.y))) / BwMath.Deg;
            double lo = 0.0, hi = Math.PI;
            if (ElOf(At(hi)) >= minElDeg) lo = hi;
            else for (int it = 0; it < 80; it++) { double m = 0.5 * (lo + hi); if (ElOf(At(m)) >= minElDeg) lo = m; else hi = m; }
            var pts = new D3[n];
            for (int i = 0; i < n; i++) pts[i] = At(-lo + 2.0 * lo * i / Math.Max(1, n - 1));
            return pts;
        }

        static D3[] DiscDirs(double az, double el, double radiusDeg, int n)
        {
            D3 c = AzEl(az, el);
            D3 up = V(0, 1, 0);
            D3 e1 = (up - c * D3.Dot(up, c)).Normalized, e2 = D3.Cross(c, e1);
            double cr = Math.Cos(radiusDeg * BwMath.Deg), sr = Math.Sin(radiusDeg * BwMath.Deg);
            var pts = new D3[n + 1];
            pts[0] = c;
            for (int i = 0; i < n; i++) { double ph = 2 * Math.PI * i / n; pts[i + 1] = c * cr + (e1 * Math.Cos(ph) + e2 * Math.Sin(ph)) * sr; }
            return pts;
        }

        /// <summary>Directions toward the sea horizon (eye at y) for azimuths a0..a1.</summary>
        static D3[] SeaHorizon(double eyeY, double a0, double a1, double step)
        {
            var l = new List<D3>();
            double dip = SeaDipDeg(eyeY);
            for (double a = a0; a <= a1 + 1e-9; a += step) l.Add(AzEl(a, -dip));
            return l.ToArray();
        }

        /// <summary>Lowland directions (below the visible horizon) over an azimuth / elevation grid.</summary>
        static D3[] LowlandGrid(double eyeY, double az0, double az1, double el0, double el1)
        {
            var l = new List<D3>();
            double dip = LowlandDipDeg(eyeY);
            for (double a = az0; a <= az1 + 1e-9; a += (az1 - az0) / 12.0)
                for (double e = el0; e <= el1 + 1e-9; e += (el1 - el0) / 8.0)
                    if (e < -dip - 0.05) l.Add(AzEl(a, e));
            return l.ToArray();
        }

        // ---------------------------------------------------------------- KrakenProxy (A1 4.9; the whale of 14.5.6 is removed)
        /// <summary>End clock (t_g) of the ending key shots (Beat08Ending stills: KS27 1.5, KS28 8.5, KS28b 20, KS29 30).</summary>
        public static double EndShotTg(string id)
        {
            // the KeyShot row's own end clock (BulwarkConst.KeyShots tg, fix wave R5) first; the A1 4.9 values otherwise
            if (TryShot(id, out KeyShot k) && !double.IsNaN(k.tg) && k.tg >= 0.0) return k.tg;
            switch (id) { case "KS27": return 1.5; case "KS28": return 8.5; case "KS28b": return 20.0; case "KS29": return 30.0; default: return -1.0; }
        }

        /// <summary>
        /// The kraken show's pose at s (KrakenDirectorChoreo.Evaluate: the same pure function the director, the sea, the mist
        /// and the audio read; sEff = s as in captures; ambient = the cinematic clock at s; tg = the end clock or -1) as audit
        /// subjects: each posed arm's waypoint polyline ("kraken_arm_L<limb>") and the head ("kraken_head": the root and the
        /// head top above the sea). When the show does not pose the creature at s (the beat-6 encounter owns KK4 / KS17), one
        /// Info subject says so (never a finding: the encounter's own capture checks cover it).
        /// </summary>
        public static void KrakenProxy(double s, double tg, Rule rule, string source, List<Subject> into)
        {
            var pose = new Megastructure.Creature.KrakenShowPose();
            try { Megastructure.Creature.KrakenDirectorChoreo.Evaluate(s, s / FrameworkConst.CinematicSpeed, tg, pose); }
            catch (Exception e) { into.Add(new Subject { Name = "kraken_proxy_error", Kind = SubjectKind.Points, Points = new D3[0], Rule = Rule.Info, MustBeVisible = false, Note = e.Message, Source = source }); return; }
            int arms = 0;
            if (pose.Visible)
            {
                for (int i = 0; i < pose.ArmCount; i++)
                {
                    var a = pose.Arms[i];
                    if (a.Weight <= 0f || a.Count < 2) continue;
                    var pts = new D3[a.Count];
                    for (int k = 0; k < a.Count; k++) pts[k] = V(a.P[k].x, a.P[k].y, a.P[k].z);
                    into.Add(Pts("kraken_arm_L" + a.Limb, pts, rule, "KrakenDirectorChoreo arm waypoints at s " + F(s) + (tg >= 0 ? " t_g " + F(tg) : "") + " (" + a.Count + " points, scale " + F(pose.Scale) + ")", source));
                    arms++;
                }
                if (tg < 0.0)
                {
                    var r = pose.Root;
                    double top = BulwarkConst.SeaY + pose.HeadTopAboveSea;
                    into.Add(Pts("kraken_head", new[] { V(r.x, r.y, r.z), V(r.x, Math.Max(r.y, top), r.z) }, rule, "KrakenDirectorChoreo root and head top (sea + " + F(pose.HeadTopAboveSea) + " m) at s " + F(s), source));
                }
            }
            if (!pose.Visible || (arms == 0 && tg >= 0.0))
                into.Add(new Subject { Name = "kraken_not_posed", Kind = SubjectKind.Points, Points = new D3[0], Rule = Rule.Info, MustBeVisible = false,
                                       Note = "the show choreography does not pose the creature at s " + F(s) + (tg >= 0 ? " t_g " + F(tg) : "") + " (the beat-6 encounter or no event)", Source = source });
        }

        /// <summary>Key-shot id of a KeyShots tag: the part before the first '_' ("KS04_crest_sea" -> "KS04", "KK1_arm_tower" -> "KK1").</summary>
        public static string KeyId(string tag)
        {
            if (string.IsNullOrEmpty(tag)) return "";
            int u = tag.IndexOf('_');
            return u > 0 ? tag.Substring(0, u) : tag;
        }

        /// <summary>Appends the subjects of a key shot (id "KS01".."KS29", "KK1".."KK4") with their spec bounds and acceptance
        /// numbers. Rows that are not in BulwarkConst.KeyShots yet simply never ask (the cases below use only their own data).</summary>
        public static void SubjectsForShot(string id, List<Subject> into)
        {
            switch (id)
            {
                case "KS01":
                {
                    var door = Box("door", BulwarkConst.DoorOpeningXMin, 0, BulwarkConst.DoorPlaneZ - 0.01, BulwarkConst.DoorOpeningXMax, BulwarkConst.DoorOpeningHeight, BulwarkConst.DoorPlaneZ, Rule.Inside, "the closed door face at z 120 (4.0 x 3.4)", "14.5.1 KS01: 65 x 56 px at 4K, acceptance >= 60 x 50");
                    door.ExpectWidth4K = 65; door.ExpectHeight4K = 56; door.SizeTol4K = 4; door.MinWidth4K = 60; door.MinHeight4K = 50;
                    into.Add(door);
                    for (int k = 1; k <= BulwarkConst.GalleryLampCount; k++)
                    {
                        double z = BulwarkConst.GalleryLampPitch * k;
                        into.Add(Box("lamp_z" + z.ToString("0", CultureInfo.InvariantCulture), BulwarkConst.StartX - 0.225, BulwarkConst.GalleryClearHeight - 0.25, z - 0.1, BulwarkConst.StartX + 0.225, BulwarkConst.GalleryClearHeight - 0.05, z + 0.1, Rule.Inside,
                                     "0.45 x 0.2 m caged lamp; hung on the centre line under the 3.6 m ceiling (assumed: 14.5.1 gives size and z only)", "14.5.1 KS01: nine lamps converging"));
                    }
                    break;
                }
                case "KS02":
                {
                    double z = 66.0, hw = 0.5 * BulwarkConst.SlitWidth;
                    var slit = Box("slit_z66", BulwarkConst.SlitX, BulwarkConst.SlitY0, z - hw, BulwarkConst.SlitX + 0.01, BulwarkConst.SlitY1, z + hw, Rule.Inside, "277 px at the rows farthest from the centre (14.10.5 ruling; spec 270 on axis)", "14.5.1 KS02: one slit 270 px wide at 2.0 m");
                    slit.ExpectWidth4K = BulwarkConst.KS02SlitWidth4K; slit.SizeTol4K = BulwarkConst.KS02SlitTol4K;
                    into.Add(slit);
                    var nb = Box("slit_neighbours_z54_z78", BulwarkConst.SlitX, BulwarkConst.SlitY0, 54 - hw, BulwarkConst.SlitX + 0.01, BulwarkConst.SlitY1, 78 + hw, Rule.Info, "neighbours 80 deg off axis (14.5.1): reported only", "14.5.1");
                    nb.MustBeVisible = false;
                    into.Add(nb);
                    break;
                }
                case "KS03":
                {
                    into.Add(Box("door_opening", BulwarkConst.DoorOpeningXMin, 0, BulwarkConst.DoorPlaneZ, BulwarkConst.DoorOpeningXMax, BulwarkConst.DoorOpeningHeight, BulwarkConst.DoorWallNorthZ, Rule.Inside, "the open door 59 deg wide at 3.5 m", "14.5.1 KS03"));
                    into.Add(Box("road", BulwarkConst.RoadXMin, -0.5, BulwarkConst.DoorWallNorthZ + 0.3, BulwarkConst.RoadXMax, 0, 900, Rule.InFrame, "the crest road beyond the door", "14.5.1 KS03"));
                    var pl = Box("parapet_line", BulwarkConst.ParapetXMin, 0, BulwarkConst.DoorWallNorthZ + 0.3, BulwarkConst.ParapetXMax, BulwarkConst.ParapetTop, 420, Rule.InFrame, "seen through the door opening (visible floor 10 %)", "14.5.1 KS03");
                    pl.MinVisible = 0.10; into.Add(pl);
                    into.Add(Dirs("sea_horizon", SeaHorizon(1.7, -10, 70, 0.5), Rule.InFrame, 5000, "sea horizon (dip from 9.7 m) for az -10..70; the parapet hides it below az ~1.3 deg, the door jamb beyond ~25.5 deg; spec: right half", "14.5.1 KS03"));
                    var lr = Box("lee_rail", BulwarkConst.LeeRailX - 0.05, 0, BulwarkConst.DoorWallNorthZ + 0.3, BulwarkConst.LeeRailX + 0.05, BulwarkConst.LeeRailTop, 300, Rule.InFrame, "spec: left half; seen through the door opening (visible floor 10 %)", "14.5.1 KS03");
                    lr.MinVisible = 0.10; into.Add(lr);
                    break;
                }
                case "KS04":
                {
                    var k = Shot("KS04");
                    into.Add(Row("horizon_row", AzEl(k.yaw, -SeaDipDeg(k.y)), 840, 8, "sea horizon at the centre column", "14.10.2 / 14.5.2 acceptance (2): row 840 +-8 of 2160"));
                    double d = (BulwarkConst.ParapetXMax - k.x) / Math.Sin(k.yaw * BwMath.Deg);
                    D3 edge = V(BulwarkConst.ParapetXMax, BulwarkConst.ParapetTop, k.z + d * Math.Cos(k.yaw * BwMath.Deg));
                    into.Add(Row("parapet_row", (edge - V(k.x, k.y, k.z)).Normalized, 1390, 10, "parapet outer top edge (x 6.0, y 1.1) at the centre column", "14.5.2 acceptance (2): row 1390 +-10"));
                    SeaState.KS04PlumeBounds(out UnityEngine.Vector3 pMin, out UnityEngine.Vector3 pMax);
                    into.Add(Box("plume_z175", pMin.x, pMin.y, pMin.z, pMax.x, pMax.y, pMax.z, Rule.Inside, "forced KS04 plume: the collision jet 30 m off the wall at z 175 (x 36), peak 10 m, at its 0.9 s still age (SeaState.KS04PlumeBounds = MistVolumeMath.SprayPose)", "14.5.1 Captures / 14.5.2 KS04"));
                    into.Add(Box("parapet", BulwarkConst.ParapetXMin, 0, 140, BulwarkConst.ParapetXMax, BulwarkConst.ParapetTop, 200, Rule.InFrame, "", "14.5.2 KS04"));
                    break;
                }
                case "KS05":
                {
                    var k = Shot("KS05");
                    into.Add(Row("lowland_horizon_row", AzEl(k.yaw, -LowlandDipDeg(k.y)), 180, 12, "lowland horizon (dip 2.486) at the centre column", "14.5.2 acceptance (3): row 180 +-12"));
                    double d = (k.x - BulwarkConst.LeeFaceX) / Math.Sin(-k.yaw * BwMath.Deg);
                    double zEdge = k.z + d * Math.Cos(k.yaw * BwMath.Deg);
                    into.Add(Row("deck_strip_row", (V(BulwarkConst.LeeFaceX, 0, zEdge) - V(k.x, k.y, k.z)).Normalized, BulwarkConst.KS05DeckStripRow4K, BulwarkConst.KS05DeckStripTol4K,
                                 "deck lee edge (x -6.0, y 0) at the centre column: row 1628 (14.10.5 ruling; the spec's 1661 uses the -48.6 deg ray, 1.5 m perpendicular)", "14.5.2 KS05: deck strip rows 1661-2160"));
                    double zPost = Math.Round((zEdge - 1.0) / BulwarkConst.LeeRailPostPitch) * BulwarkConst.LeeRailPostPitch + 1.0;
                    double ps = 0.5 * BulwarkConst.LeeRailPostSize;
                    into.Add(Box("rail_post", BulwarkConst.LeeRailX - ps, 0, zPost - ps, BulwarkConst.LeeRailX + ps, BulwarkConst.LeeRailTop, zPost + ps, Rule.Inside, "the post nearest the centre column (posts every 2 m at odd z, phase assumed)", "14.5.2 KS05: rail post at 1.5 m"));
                    into.Add(Box("lee_rail", BulwarkConst.LeeRailX - 0.05, 0.45, k.z - 8, BulwarkConst.LeeRailX + 0.05, BulwarkConst.LeeRailTop, k.z + 8, Rule.InFrame, "", "14.5.2 KS05"));
                    var low = Dirs("lowland", LowlandGrid(k.y, k.yaw - 40, k.yaw + 40, -60, -3), Rule.InFrame, 20000, "hazy fields below the horizon (grid az +-40, el -3..-60)", "14.5.2 KS05");
                    low.BelowHorizon = true;
                    into.Add(low);
                    break;
                }
                case "KS06":
                {
                    double z = BulwarkConst.LampPostZ0 + BulwarkConst.LampPostPitch * 4;   // k 4: lee side
                    var post = Box("lamp_post_z350", BulwarkConst.LampPostLeeX - 0.1, 0, z - 0.1, BulwarkConst.LampPostLeeX + 0.1, BulwarkConst.LampPostHeight, z + 0.1, Rule.Inside, "", "14.5.2 KS06: lee lamp post z 350 310 px tall");
                    post.ExpectHeight4K = 310; post.SizeTol4K = 8;
                    into.Add(post);
                    var cart = Box("cart", BulwarkConst.CartX - 0.5 * BulwarkConst.CartW, 0, BulwarkConst.CartZ - 0.5 * BulwarkConst.CartL, BulwarkConst.CartX + 0.5 * BulwarkConst.CartW, BulwarkConst.CartH, BulwarkConst.CartZ + 0.5 * BulwarkConst.CartL, Rule.Inside, "", "14.5.2 KS06: cart 109 px at 30 m");
                    cart.ExpectHeight4K = BulwarkConst.KS06CartHeight4K; cart.SizeTol4K = BulwarkConst.KS06CartTol4K;   // 115 (ruling; spec 109)
                    into.Add(cart);
                    into.Add(Plume());
                    break;
                }
                case "KS07":
                {
                    for (int p = 4; p <= 5; p++)
                    {
                        double z0 = BulwarkConst.PierZ0 + BulwarkConst.PierPitch * p, z1 = z0 + BulwarkConst.PierLength;
                        string zc = (z0 + 2).ToString("0", CultureInfo.InvariantCulture);
                        into.Add(Box("pier_z" + zc, BulwarkConst.SeaFaceX, -0.6, z0, BulwarkConst.PierEastX, BulwarkConst.PierTopY, z1, Rule.InFrame, "pier cap x 6..46 above the deck underside (below it the deck hides the pier)", "14.5.3 KS07"));
                        into.Add(Box("pier_z" + zc + "_east_end", BulwarkConst.PierEastX - 6, -0.6, z0, BulwarkConst.PierEastX, BulwarkConst.PierTopY, z1, Rule.Inside, "", "14.5.3 KS07: piers running to x 46"));
                        into.Add(Box("gate_house_z" + zc, BulwarkConst.GateHouseX0, BulwarkConst.PierTopY, z0, BulwarkConst.GateHouseX1, BulwarkConst.PierTopY + BulwarkConst.GateHouseH, z1,
                            p == 5 ? Rule.InFrame : Rule.Inside,
                            p == 5 ? "cut by the left frame edge by the KS07 pose (14.10.5 lead ruling, Wave X2): near corner (9, 520) at bearing 15.9 deg, 54.1 deg left of yaw 70 (half-FOV 49.1 deg at 16:9)" : "",
                            "14.5.3 KS07"));
                    }
                    into.Add(Box("funnel_slot4", BulwarkConst.SeaFaceX, BulwarkConst.LipY, 504, BulwarkConst.PierEastX, BulwarkConst.DrawdownY0, 520, Rule.InFrame, "the drawn-down water between piers 4 and 5 (read as the water: the deck edge 5.3 m along the view and pier 4's 0.5 m cap hide it)", "14.5.3 KS07"));
                    into.Add(Dirs("sea_horizon", SeaHorizon(1.7, 20, 120, 1.0), Rule.InFrame, 5000, "", "14.5.3 KS07"));
                    break;
                }
                case "KS08":
                {
                    var k = Shot("KS08");
                    SunAt(k.s, out double saz, out double sel);
                    D3 anti = AzEl(saz + 180.0, -sel);
                    var bow = Dirs("rainbow_primary_141", ArcAboveElevation(anti, BulwarkConst.RainbowPrimaryDeg, -LowlandDipDeg(k.y), 141), Rule.Inside, 3000,
                                   "primary bow 42 deg round the antisolar point (sun az " + saz.ToString("0.0", CultureInfo.InvariantCulture) + " el " + sel.ToString("0.0", CultureInfo.InvariantCulture) + "), 141 points above the visible horizon", "14.5.3 KS08 / acceptance (2)");
                    into.Add(bow);
                    var sun = Dirs("sun_disc", DiscDirs(saz, sel, 0.27, 16), Rule.Absent, 30000, "the sun must not be in frame", "14.5.3 acceptance (2)");
                    sun.MustBeVisible = false;
                    into.Add(sun);
                    var plume = Plume(); plume.Note += "; spec: at left";
                    into.Add(plume);
                    // lit patch: the cloud-hole disc 22 km west at az 285, radius 12 km x smootherstep((s - 530) / 70) (on the lowland)
                    double rr = BulwarkConst.CloudHoleRadius * BwMath.Smootherstep01((k.s - BulwarkConst.CloudHoleS0) / BulwarkConst.CloudHoleWindow);
                    var patch = Pts("lit_patch", LowlandDisc(k, BulwarkConst.CloudHoleAz, BulwarkConst.CloudHoleDistance, rr, 24), Rule.InFrame,
                                    "cloud-hole disc on the lowland (y -6000 minus the curvature drop), centred 22 km from the eye at az 285, radius " + (rr / 1000).ToString("0.0", CultureInfo.InvariantCulture) + " km", "14.5.3 KS08: lit patch in the centre");
                    patch.BelowHorizon = true;
                    into.Add(patch);
                    break;
                }
                case "KS09":
                {
                    double z0 = BulwarkConst.SlotZ0 + BulwarkConst.PierPitch * 7, z1 = z0 + BulwarkConst.SlotLength;
                    into.Add(Box("grating_slot7", BulwarkConst.LaneRoadXMin - BulwarkConst.PlayerR, -0.05, z0, BulwarkConst.LaneRoadXMax + BulwarkConst.PlayerR, 0, z1, Rule.InFrame, "grating over the lane width (see-through)", "14.5.3 KS09"));
                    var water = Box("water_below", BulwarkConst.LipX, BulwarkConst.LipY - 0.1, z0, BulwarkConst.RoadXMax, BulwarkConst.LipY + 3.5 * 12.0 / 52.0, z1, Rule.InFrame, "draw-down water under the deck (y -11.5 at the lip)", "14.5.3 KS09: water about 12 m below");
                    water.BelowHorizon = true; into.Add(water);
                    var lip = Box("lip", BulwarkConst.LipX - 0.05, BulwarkConst.SillY, z0, BulwarkConst.LipX, BulwarkConst.LipY, z1, Rule.InFrame, "", "14.5.3 KS09");
                    lip.BelowHorizon = true; into.Add(lip);
                    var sheet = Pts("sheet_first_40m", SheetPoints(z0, z1, 40.0), Rule.InFrame, "sheet centre line x = -6 - 8.3 t, y = -11.5 - 4.9 t^2 for its first 40 m of arc", "14.5.3 KS09");
                    sheet.BelowHorizon = true; into.Add(sheet);
                    break;
                }
                case "KS10":
                {
                    var k = Shot("KS10");
                    for (int i = 0; i < BulwarkConst.IsletX.Length; i++)
                    {
                        double cx = BulwarkConst.IsletX[i], cz = BulwarkConst.IsletZ[i], r = BulwarkConst.IsletRadiusMax;
                        var isl = Pts("islet_" + (i + 1) + "_" + cx.ToString("0", CultureInfo.InvariantCulture) + "_" + cz.ToString("0", CultureInfo.InvariantCulture), Cylinder(cx, cz, r, BulwarkConst.SeaY, BulwarkConst.IsletCrestYMax), Rule.Inside,
                                      "round pile: footprint radius 70 (max of 45-70), crest y +22 (max): worst case for framing", "14.5.4 KS10 / acceptance (2): at least two fully inside at three aspects");
                        isl.MinWidth4K = 200;   // a complete 40 m unit at least 200 px tip to tip (checked on the unit size at this distance)
                        isl.Min = V(cx - r, BulwarkConst.SeaY, cz - r); isl.Max = V(cx + r, BulwarkConst.IsletCrestYMax, cz + r);
                        // Wave X reef: frame the visible pile (Beat04ArmourSlope.IsletPile: 1.8 R wide, sea to crest), not the R-radius AABB
                        // (islet 1's AABB corner sits at bearing 126.7 deg; the pile's tangent edge at 119.4 deg is inside the 16:10 limit 121.2)
                        var pile = Megastructure.Beats.Beat04ArmourSlope.IsletPile(i);
                        isl.Min = V(pile.min.x, pile.min.y, pile.min.z); isl.Max = V(pile.max.x, pile.max.y, pile.max.z);
                        if (i == 3) { isl.Note = "partly behind islet 1 by design (bearing 94.9 deg inside islet 1's span 101.9 +- 17 deg)"; isl.MinVisible = 0.25; }
                        into.Add(isl);
                    }
                    into.Add(Row("horizon_row", AzEl(k.yaw, -SeaDipDeg(k.y)), 992, 8, "", "14.5.4 acceptance (2): row 992 +-8"));
                    double d = (BulwarkConst.ParapetXMax - k.x) / Math.Sin(k.yaw * BwMath.Deg);
                    D3 edge = V(BulwarkConst.ParapetXMax, BulwarkConst.ParapetTop, k.z + d * Math.Cos(k.yaw * BwMath.Deg));
                    into.Add(Row("parapet_row", (edge - V(k.x, k.y, k.z)).Normalized, 1325, 10, "", "14.5.4 acceptance (2): row 1325 +-10"));
                    foreach (double dist in new[] { 80.0, 200.0 })
                    {
                        double ax = k.x + dist * Math.Sin(k.yaw * BwMath.Deg), az = k.z + dist * Math.Cos(k.yaw * BwMath.Deg);
                        var tip = Box("belt_tip_" + dist.ToString("0", CultureInfo.InvariantCulture) + "m", ax - 0.5, BulwarkConst.SeaY, az - 0.5, ax + 0.5, BulwarkConst.BeltTipYMax, az + 0.5, Rule.Inside,
                                      "tallest tip (y -8..-1.5) on the centre ray", "14.5.4 acceptance (2): 132 +-20 px at 80 m, 54 +-10 at 200");
                        tip.ExpectHeight4K = dist < 100 ? 132 : 54; tip.SizeTol4K = dist < 100 ? 20 : 10;
                        tip.BelowHorizon = false;
                        into.Add(tip);
                    }
                    break;
                }
                case "KS11":
                {
                    into.Add(Box("plume_z845", BulwarkConst.SeaFaceX, 0, 841, BulwarkConst.SeaFaceX + 8, 14, 849, Rule.Inside, "14 m forced plume at the sea face (8 m wide ellipsoid bounds assumed)", "14.5.4 KS11"));
                    into.Add(Tower(Rule.InFrame));
                    break;
                }
                case "KS12":
                {
                    var t = Tower(Rule.Inside);
                    t.ExpectHeight4K = 1629; t.SizeTol4K = 12;
                    into.Add(t);
                    var top = Box("tower_top_edge", BulwarkConst.TowerXMin, BulwarkConst.TowerHeight - 0.05, BulwarkConst.TowerZ0 - 0.06, BulwarkConst.TowerXMax, BulwarkConst.TowerHeight, BulwarkConst.TowerZ0 - 0.01, Rule.Inside, "top edge of the south face (+49 deg, frame top 56)", "14.5.4 KS12: rows 273-1902");
                    top.ExpectRow4K = 273; top.PosTol4K = 8;
                    into.Add(top);
                    into.Add(Box("portal", BulwarkConst.PortalXMin, 0, BulwarkConst.TowerZ0, BulwarkConst.PortalXMax, BulwarkConst.PortalHeight, BulwarkConst.TowerZ0 + 0.5, Rule.Inside, "portal glow near the bottom", "14.5.4 KS12"));
                    break;
                }
                case "KS13":
                {
                    foreach (double pz in BulwarkConst.PendantZ)
                        for (int side = -1; side <= 1; side += 2)
                        {
                            double r = 0.5 * BulwarkConst.PendantSize, px = side * BulwarkConst.PendantX;
                            var pd = Box("pendant_" + (side < 0 ? "w" : "e") + "_z" + pz.ToString("0", CultureInfo.InvariantCulture), px - r, BulwarkConst.PendantY - r, pz - r, px + r, BulwarkConst.PendantY + r, pz + r, pz > 929 ? Rule.Inside : Rule.Info, pz > 929 ? "" : "above / behind the frame by geometry (reported)", "14.5.5 KS13: pendant lamps");
                            pd.MustBeVisible = pz > 929;
                            into.Add(pd);
                        }
                    var mouth = Box("stair_mouth", BulwarkConst.StairMouthXMin, 0, BulwarkConst.StairMouthZ - 0.01, BulwarkConst.StairMouthXMax, BulwarkConst.StairHeadroom, BulwarkConst.StairMouthZ, Rule.Inside, "the mouth plane z 934, 3.6 m wide", "14.5.5 KS13: 842 px wide at 8 m");
                    mouth.ExpectWidth4K = BulwarkConst.KS13MouthWidth4K; mouth.SizeTol4K = BulwarkConst.KS13MouthTol4K;   // 917 (ruling; spec 842)
                    into.Add(mouth);
                    var arch = Box("north_arch", BulwarkConst.ArchXMin, 0, BulwarkConst.ArchZ - 0.51, BulwarkConst.ArchXMax, BulwarkConst.ArchHeight, BulwarkConst.ArchZ - 0.5, Rule.Inside, "the arch plane at the inner face of the north wall (z 955.5), 4.7 x 5.5", "14.5.5 KS13: 293 px wide at 30 m");
                    arch.ExpectWidth4K = BulwarkConst.KS13ArchWidth4K; arch.SizeTol4K = BulwarkConst.KS13ArchTol4K;   // 319 (ruling; spec 293)
                    into.Add(arch);
                    into.Add(Box("guard_rail", 0.5, 0, BulwarkConst.TowerZ0 + 0.5, 0.6, BulwarkConst.GuardRailHeight, BulwarkConst.AtriumZ1 - 0.3, Rule.InFrame, "x 0.55 assumed (between portal and vehicle arch)", "14.5.5 KS13"));
                    break;
                }
                case "KS14":
                {
                    var k = Shot("KS14");
                    double sk = BwPath.Project(k.x, k.z, k.s);
                    int flight = (int)Math.Floor((sk - BwPath.StairsStartS) / BulwarkConst.StairFlightPitch);
                    double l0 = BwPath.StairFlightStartS(flight) + BulwarkConst.StairFlightRun, l1 = l0 + BulwarkConst.StairLanding;
                    double yl = BwPath.FloorY(l0 + 0.6), hw = 0.5 * BulwarkConst.StairWidth;
                    var landing = Pts("next_landing", Rect(l0, l1, -hw, hw, yl), Rule.InFrame, "landing after flight " + flight + " (s " + l0.ToString("0.00", CultureInfo.InvariantCulture) + "-" + l1.ToString("0.00", CultureInfo.InvariantCulture) + ", y " + yl.ToString("0.0", CultureInfo.InvariantCulture) + ")", "14.5.5 KS14: rows 1573-1778");
                    // rows 1704-2122 from the 14.4 stair layout (ruling; spec 1573-1778)
                    double lr0 = BulwarkConst.KS14LandingRow0_4K, lr1 = BulwarkConst.KS14LandingRow1_4K;
                    landing.ExpectRow4K = 0.5 * (lr0 + lr1); landing.PosTol4K = 0.5 * (lr1 - lr0) + 10;
                    landing.ExpectHeight4K = lr1 - lr0; landing.SizeTol4K = 10;
                    into.Add(landing);
                    double f0 = l1, f1 = f0 + BulwarkConst.StairFlightRun;
                    var next = Pts("following_flight", RectStairs(f0, f1, -hw, hw), Rule.InFrame, "flight " + (flight + 1) + " (tread line)", "14.5.5 KS14: the following flight up to row 1497");
                    next.ExpectRow4K = BulwarkConst.KS14FlightTopRow4K; next.PosTol4K = 10;   // compared with its top row (1521, ruling; spec 1497)
                    into.Add(next);
                    // the riser about 3 m ahead (14.5.5: a 0.2 m riser = 117 px at 3 m)
                    double sr = double.NaN;
                    for (int i = 0; i < BulwarkConst.StairSteps; i++) { double ri = BwPath.StairRiserS(i); if (ri - sk >= 3.0) { sr = ri; break; } }
                    double ytop = BwPath.FloorY(sr - 1e-6), ybot = BwPath.FloorY(sr + 1e-6);
                    var riser = Pts("riser_3m", new[] { PathPoint(sr, -0.5, ytop), PathPoint(sr, 0.5, ytop), PathPoint(sr, 0.5, ybot), PathPoint(sr, -0.5, ybot) }, Rule.Inside, "riser at s " + sr.ToString("0.00", CultureInfo.InvariantCulture), "14.5.5 KS14: 0.2 m riser = 117 px at 3 m");
                    riser.ExpectHeight4K = BulwarkConst.KS14Riser3mHeight4K; riser.SizeTol4K = 6;   // 57 (ruling; spec 117 is a face-on riser)
                    into.Add(riser);
                    break;
                }
                case "KS15":
                {
                    var pane = PaneSubject(1, Rule.Fills, "pane k 1 fills the frame (no pier visible); 21:9 exempt: the top-corner rays meet the glass at lateral +-10.48 m (ruling: the piers frame the glass)", "14.5.6 KS15");
                    pane.FillsExemptAspect = BulwarkConst.KS15FillExemptAspect;
                    into.Add(pane);
                }
                    break;
                case "KS16":
                {
                    var k = Shot("KS16");
                    var pane = PaneSubject(3, Rule.InFrame, "pane k 3 above the eye (shafts across the upper veil)", "14.5.6 KS16");
                    pane.Min = V(pane.Min.x, k.y, pane.Min.z);
                    into.Add(pane);
                    break;
                }
                case "KS17":
                    // A1 4.9: the kraken eye replaces the whale (the beat-6 encounter poses it; the show proxy reports Info)
                    KrakenProxy(1268.0, -1.0, Rule.InFrame, "A1 4.9 KS17 win_eye", into);
                    break;
                case "KK1":
                    // A1 4.9: arm 733 +-40 px tall at bearing 46.7 and the tower 284 +-15 px at bearing 0.8, both inside all aspects
                    into.Add(Tower(Rule.Inside));
                    KrakenProxy(378.0, -1.0, Rule.Inside, "A1 4.9 KK1 arm_tower", into);
                    break;
                case "KK2":
                    // A1 4.9: the head at bearing 56.3 (top at 15.7 deg), the eyes above the islet silhouette, the draped arm
                    KrakenProxy(806.0, -1.0, Rule.InFrame, "A1 4.9 KK2 head_eyes", into);
                    break;
                case "KK3":
                    KrakenProxy(1008.0, -1.0, Rule.InFrame, "A1 4.9 KK3 porthole", into);
                    break;
                case "KK4":
                    // A1 4.9: the arm on the glass in lamp A (the beat-6 encounter poses it; the show proxy reports Info)
                    KrakenProxy(1232.0, -1.0, Rule.InFrame, "A1 4.9 KK4 arm_glass", into);
                    break;
                case "KS22": case "KS23": case "KS24": case "KS25": case "KS26": case "KS26a": case "KS26b":
                {
                    // ride shots (EndingPath): the lowland horizon (Info: the ride poses are the ending builder's); tolerant of an
                    // absent row (the ending builder adds KS22-KS29 to BulwarkConst.KeyShots, lead ruling R5)
                    if (!TryShot(id, out KeyShot k)) break;
                    var hz = Dirs("lowland_horizon", new[] { AzEl(k.yaw, -LowlandDipDeg(k.y)) }, Rule.Info, 30000, "horizon dip " + F(LowlandDipDeg(k.y)) + " deg from eye y " + F(k.y), "A1 4.9 " + id);
                    hz.MustBeVisible = false;
                    into.Add(hz);
                    break;
                }
                case "KS27":
                {
                    // A1 4.9 KS27: the Keeper's Lantern in the lower middle (post + drum), no crest in the frame
                    double x = EndingPath.KeeperX, z = EndingPath.KeeperZ, y0 = EndingPath.FloorFootY;
                    into.Add(Box("keeper_lantern", x - 0.2, y0, z - 0.2, x + 0.2, y0 + BulwarkConst.KeeperPost + BulwarkConst.LanternGlass, z + 0.2, Rule.Inside, "post " + F(BulwarkConst.KeeperPost) + " m + glass drum " + F(BulwarkConst.LanternGlass) + " m, 1.9 m ahead / 0.4 m right of the rest", "A1 4.9 KS27"));
                    into.Add(CrestLine(Rule.Absent, "A1 4.9 KS27: no crest in frame"));
                    break;
                }
                case "KS28": case "KS28b": case "KS29":
                {
                    // A1 4.9 KS28: the crest a straight line near the top; KS28b (t_g 20): the crest gone dark, the same framing;
                    // KS29 (t_g 30, pitch 36): the stinger columns above it
                    into.Add(CrestLine(Rule.InFrame, "A1 4.9 " + id + ": the crest line"));
                    KrakenProxy(EndingPath.SRest, EndShotTg(id), id == "KS29" ? Rule.InFrame : Rule.Info, "lead task S2-1 " + id + " stinger at t_g " + F(EndShotTg(id)), into);
                    if (id != "KS29") foreach (var sub in into) if (sub.Name.StartsWith("kraken_", StringComparison.Ordinal)) sub.MustBeVisible = false;
                    break;
                }
                case "KS18":
                {
                    var k = Shot("KS18");
                    var below = PaneSubject(PaneAtS(1320.0), Rule.InFrame, "the glass below the floor level (pane at s 1320; trench edge at axis + 3.0)", "14.5.6 KS18: black below");
                    below.Max = V(below.Max.x, BwPath.FloorY(1320.0), below.Max.z);
                    below.BelowHorizon = true;
                    into.Add(below);
                    var r30 = Dirs("view_el_m30", new[] { AzEl(k.yaw, -30.0) }, Rule.InFrame, 30, "the window at pitch -30 (luma 17)", "14.5.6 KS18 / H3 table");
                    r30.BelowHorizon = true;
                    into.Add(r30);
                    var r60 = Dirs("view_el_m60", new[] { AzEl(k.yaw, -60.0) }, Rule.Info, 30, "pitch -60 (luma 8) lies below the KS18 frame (bottom -56)", "14.5.6 KS18");
                    r60.BelowHorizon = true; r60.MustBeVisible = false;
                    into.Add(r60);
                    break;
                }
                case "KS19":
                {
                    double lat = 0.5 * BulwarkConst.TunnelWidth;
                    var pts = new List<D3>();
                    for (double s = BwPath.A3EndS; s <= BulwarkConst.TunnelMouthS + 1e-6; s += 1.0)
                        for (double h = 0; h <= BulwarkConst.GratingPanelHeight + 1e-6; h += BulwarkConst.GratingPanelHeight / 4)
                            pts.Add(PathPoint(s, lat, BulwarkConst.FloorYTunnel + h));
                    into.Add(Pts("grating_panel", pts.ToArray(), Rule.InFrame, "east grating panel 2.8 x 43 m at the tunnel wall (lateral +1.6)", "14.5.7 KS19: grating 1.8 m away"));
                    break;
                }
                case "KS20":
                {
                    var k = Shot("KS20");
                    into.Add(Row("horizon_row", AzEl(k.yaw, -LowlandDipDeg(k.y)), 948, 8, "", "14.5.7 acceptance (2): row 948 +-8"));
                    SunAt(k.s, out double saz, out double sel);
                    var sun = Dirs("sun_disc", DiscDirs(saz, sel, 0.276, 16), Rule.Inside, 30000, "sun az " + saz.ToString("0.0", CultureInfo.InvariantCulture) + " el " + sel.ToString("0.000", CultureInfo.InvariantCulture) + " (16 px disc at 4K)", "14.5.7 KS20: at (1050, 918) at 4K");
                    sun.ExpectCol4K = 1050; sun.ExpectRow4K = 918; sun.PosTol4K = 8;
                    into.Add(sun);
                    var lan = Lantern(false);
                    lan.ExpectCol4K = 2792; lan.PosTol4K = 10; lan.ExpectHeight4K = BulwarkConst.KS20LanternHeight4K; lan.SizeTol4K = BulwarkConst.KS20LanternTol4K;   // 53 px (ruling; spec 47 on axis)
                    into.Add(lan);
                    var vp = Dirs("north_vanishing_point", new[] { V(0, 0, 1) }, Rule.Inside, 1, "face and ledge converge here", "14.5.7 KS20: column 2783");
                    vp.ExpectCol4K = BulwarkConst.KS20VanishingCol4K; vp.PosTol4K = BulwarkConst.KS20VanishingTol4K;   // 2792 (ruling; spec 2783)
                    into.Add(vp);
                    into.Add(Pts("jets", JetPoints(), Rule.InFrame, "five outlet jets x = -6 - 40 t, y = -90 - 4.9 t^2 (t 0..14.3 s, to 1 km of fall)", "14.5.7 KS20: three jet threads"));
                    var low = Dirs("lowland", LowlandGrid(k.y, k.yaw - 45, k.yaw + 45, -20, -3), Rule.InFrame, 20000, "lowland fields below the horizon above the slab edge (el -3..-20; the frame's lower part is the ledge slab by design)", "14.5.7 KS20");
                    low.BelowHorizon = true;
                    into.Add(low);
                    into.Add(Box("ledge_rail", BulwarkConst.LedgeXMin, BulwarkConst.FloorYTunnel, k.z, BulwarkConst.LedgeXMin + 0.12, BulwarkConst.FloorYTunnel + BulwarkConst.LedgeRail, BulwarkConst.LedgeZ1, Rule.InFrame, "", "14.5.7 KS20: rail and slab in the foreground"));
                    break;
                }
                case "KS21":
                {
                    var lan = Lantern(false);
                    lan.ExpectHeight4K = 197; lan.SizeTol4K = 6;
                    into.Add(lan);
                    into.Add(Lantern(true));
                    break;
                }
            }
            foreach (var s in into) if (string.IsNullOrEmpty(s.Source)) s.Source = id;
        }

        /// <summary>The lee crest edge (x LeeFaceX, y 0) over +-3 km of z round the elevator (seen from the lantern platform).</summary>
        static Subject CrestLine(Rule rule, string source)
        {
            var pts = new List<D3>();
            for (int i = -6; i <= 6; i++) pts.Add(V(BulwarkConst.LeeFaceX, 0.0, EndingPath.SpotZ + 500.0 * i));
            return Pts("crest_line", pts.ToArray(), rule, "lee crest edge x " + F(BulwarkConst.LeeFaceX) + ", y 0, z SpotZ +-3000", source);
        }

        static Subject Plume()
        {
            double z0 = BulwarkConst.PlumeZ - 2 * BulwarkConst.PlumeSigmaZ, z1 = BulwarkConst.PlumeZ + 2 * BulwarkConst.PlumeSigmaZ;
            return Box("plume", BulwarkConst.PlumeXMin, -1000, z0, BulwarkConst.PlumeXMax, BulwarkConst.PlumeTopY, z1, Rule.InFrame,
                       "mist volume x -2500..-6, z 562 +- 2 sigma (300 m), top +260 (bounds of 14.5.3 / H2)", "14.5.3");
        }

        static Subject Tower(Rule rule) => Box("tower", BulwarkConst.TowerXMin, 0, BulwarkConst.TowerZ0, BulwarkConst.TowerXMax, BulwarkConst.TowerHeight, BulwarkConst.TowerZ1, rule, "12 x 36 x 80 m", "14.5.4");

        static Subject Lantern(bool withGlass)
        {
            double h = BulwarkConst.LanternPost + (withGlass ? BulwarkConst.LanternGlass : 0.0), r = withGlass ? 0.2 : 0.05;
            return Box(withGlass ? "lantern_with_glass" : "lantern_post", BulwarkConst.LanternX - r, BulwarkConst.LanternY, BulwarkConst.LanternZ - r, BulwarkConst.LanternX + r, BulwarkConst.LanternY + h, BulwarkConst.LanternZ + r, Rule.Inside,
                       withGlass ? "post 1.4 + glass drum 0.30" : "post 1.4 m (the spec's px heights use the post)", "14.5.7");
        }

        static int PaneAtS(double s) => (int)Math.Round((s - BulwarkConst.PaneS0) / BulwarkConst.PanePitch);

        /// <summary>Pane k (20 x 36 m, floor -8 to floor +28) on the glass plane at axis + 6.0 as a thin box.</summary>
        public static Subject PaneSubject(int k, Rule rule, string note, string source)
        {
            double c = BulwarkConst.PaneS0 + BulwarkConst.PanePitch * k, s0 = c - 0.5 * BulwarkConst.PaneWidth, s1 = c + 0.5 * BulwarkConst.PaneWidth;
            D3 a = PathPoint(s0, BulwarkConst.GlassAxisOffset, 0), b = PathPoint(s1, BulwarkConst.GlassAxisOffset, 0);
            double y0 = Math.Min(BwPath.FloorY(s0), BwPath.FloorY(s1)) + BulwarkConst.PaneFloorOffset;
            double y1 = Math.Max(BwPath.FloorY(s0), BwPath.FloorY(s1)) + BulwarkConst.PaneFloorOffset + BulwarkConst.PaneHeight;
            var sub = Box("pane_k" + k, Math.Min(a.x, b.x), y0, a.z, Math.Max(a.x, b.x) + 0.01, y1, b.z, rule, note, source);
            return sub;
        }

        static D3[] Rect(double s0, double s1, double l0, double l1, double y)
        {
            var l = new List<D3>();
            for (int i = 0; i <= 4; i++) for (int j = 0; j <= 4; j++) l.Add(PathPoint(s0 + (s1 - s0) * i / 4, l0 + (l1 - l0) * j / 4, y + 0.05));
            return l.ToArray();
        }

        static D3[] RectStairs(double s0, double s1, double l0, double l1)
        {
            // the back edge of every tread (3 cm before the next riser, 2 cm above it): seen from above-behind, a descending
            // flight shows each tread only behind the nosing of the step before (0.2 m riser / view slope of the tread hidden)
            var l = new List<D3>();
            for (int i = 0; i < BulwarkConst.StairSteps; i++)
            {
                double r = BwPath.StairRiserS(i);
                if (r <= s0 + 0.05 || r > s1 + 0.2) continue;
                double s = r - 0.03;
                for (int j = 0; j <= 2; j++) l.Add(PathPoint(s, l0 + (l1 - l0) * j / 2, BwPath.FloorY(s) + 0.02));
            }
            return l.ToArray();
        }

        static D3[] Cylinder(double cx, double cz, double r, double y0, double y1)
        {
            var l = new List<D3>();
            for (int k = 0; k < 24; k++)
            {
                double a = 2 * Math.PI * k / 24;
                l.Add(V(cx + r * Math.Cos(a), y0, cz + r * Math.Sin(a)));
                l.Add(V(cx + r * Math.Cos(a), y1, cz + r * Math.Sin(a)));
            }
            return l.ToArray();
        }

        static D3[] SheetPoints(double z0, double z1, double arc)
        {
            var l = new List<D3>();
            double t = 0, a = 0, dt = 0.01;
            var ts = new List<double> { 0 };
            while (a < arc && t < 10)
            {
                double vx = BulwarkConst.LipSpeed, vy = 2 * BulwarkConst.SheetGravityHalf * (t + 0.5 * dt);
                a += Math.Sqrt(vx * vx + vy * vy) * dt; t += dt;
                if (ts.Count < 2 || t - ts[ts.Count - 1] >= 0.2) ts.Add(t);
            }
            foreach (double tt in ts)
                foreach (double z in new[] { z0 + 0.5, 0.5 * (z0 + z1), z1 - 0.5 })
                    l.Add(V(BulwarkConst.LipX - BulwarkConst.LipSpeed * tt, BulwarkConst.LipY - BulwarkConst.SheetGravityHalf * tt * tt, z));
            return l.ToArray();
        }

        static D3[] JetPoints()
        {
            var l = new List<D3>();
            double t1 = Math.Sqrt(BulwarkConst.JetMistDistance / BulwarkConst.SheetGravityHalf);
            foreach (double z in BulwarkConst.JetZ)
                for (double t = 0; t <= t1 + 1e-9; t += t1 / 20)
                    l.Add(V(BulwarkConst.LeeFaceX - BulwarkConst.JetSpeed * t, BulwarkConst.JetY0 - BulwarkConst.SheetGravityHalf * t * t, z));
            return l.ToArray();
        }

        /// <summary>Points of a disc on the lowland (y -6000 minus the curvature drop) round the plan point at distance dist and
        /// azimuth az from the eye.</summary>
        static D3[] LowlandDisc(KeyShot k, double az, double dist, double radius, int n)
        {
            var l = new List<D3>();
            double cx = k.x + dist * Math.Sin(az * BwMath.Deg), cz = k.z + dist * Math.Cos(az * BwMath.Deg);
            void AddP(double px, double pz)
            {
                double d = Math.Sqrt((px - k.x) * (px - k.x) + (pz - k.z) * (pz - k.z));
                l.Add(V(px, BulwarkConst.LowlandY - BwCurvature.DropD(d), pz));
            }
            AddP(cx, cz);
            for (int ring = 1; ring <= 2; ring++)
                for (int i = 0; i < n; i++)
                {
                    double ph = 2 * Math.PI * i / n, r = radius * ring / 2.0;
                    AddP(cx + r * Math.Cos(ph), cz + r * Math.Sin(ph));
                }
            return l.ToArray();
        }

        /// <summary>Named subjects of Part 14 for the reachability sweep (spec bounds, beat windows of s).</summary>
        public static List<Subject> NamedSubjects()
        {
            var l = new List<Subject>();
            void W(Subject s, double s0, double s1) { s.SweepS0 = s0; s.SweepS1 = s1; s.Rule = Rule.InFrame; l.Add(s); }
            W(Box("door", BulwarkConst.DoorOpeningXMin, 0, BulwarkConst.DoorPlaneZ, BulwarkConst.DoorOpeningXMax, BulwarkConst.DoorOpeningHeight, BulwarkConst.DoorWallNorthZ, Rule.InFrame, "", "14.5.1"), 0, 120);
            {
                var slit = Box("slit_z66", BulwarkConst.SlitX, BulwarkConst.SlitY0, 65.85, BulwarkConst.SlitX + 0.01, BulwarkConst.SlitY1, 66.15, Rule.InFrame, "spec: in frame about 3 s at 2.5", "14.5.1");
                slit.SpecSeconds = 3.0;
                W(slit, 40, 90);
                // the lit back of the same slit channel (the storm glow on the baffle, 1.2 m behind the opening): the
                // channel's cheeks hide it at grazing angles, so its visible time is what the walker really sees lit
                var glow = Box("slit_z66_glow", BulwarkConst.SlitX + BulwarkConst.SlitDepth - 0.01, BulwarkConst.SlitY0, 65.85, BulwarkConst.SlitX + BulwarkConst.SlitDepth, BulwarkConst.SlitY1, 66.15, Rule.InFrame, "the baffle glow at the back of the 1.2 m channel (graybox stand-in of the storm light); visible only within about +-14 deg of square (atan(0.3 / 1.2))", "14.5.1");
                W(glow, 40, 90);
            }
            W(Box("far_tower_z3320", -6, 0, 3320, 6, 80, 3356, Rule.InFrame, "58 px tall at 2.4 km", "14.5.2"), 120, 920);
            W(Plume(), 120, 700);
            W(Box("lamp_post_z350", BulwarkConst.LampPostLeeX - 0.1, 0, 349.9, BulwarkConst.LampPostLeeX + 0.1, 9, 350.1, Rule.InFrame, "", "14.5.2"), 280, 350);
            {
                var cart = Box("cart", 1.35, 0, 328.6, 2.65, 1.9, 331.4, Rule.InFrame, "spec: on screen about 12 s (s 300-330)", "14.5.2 KS06");
                cart.SpecSeconds = 12.0;
                W(cart, 280, 340);
            }
            {
                SunAt(604, out double saz, out double sel);
                var bow = Dirs("rainbow_primary_141", ArcAboveElevation(AzEl(saz + 180, -sel), 42.0, -LowlandDipDeg(1.7), 141), Rule.InFrame, 3000, "spec: whole arch in frame s 594-622 (10.9 s at 2.5)", "14.5.3");
                bow.SpecSeconds = 10.9;
                bow.SpecSecondsWhole = true;
                W(bow, 560, 660);
            }
            for (int i = 0; i < BulwarkConst.IsletX.Length; i++)
                W(Pts("islet_" + (i + 1), Cylinder(BulwarkConst.IsletX[i], BulwarkConst.IsletZ[i], 70, BulwarkConst.SeaY, 22), Rule.InFrame, "", "14.5.4"), 700, 920);
            W(Tower(Rule.InFrame), 700, 920);
            for (int i = 0; i < BulwarkConst.PortholeS.Length; i++)
            {
                double s = BulwarkConst.PortholeS[i];
                BwPath.Lane(s, out _, out double hi);
                D3 c = PathPoint(s, hi + WallLat, BwPath.EyeY(s) - 0.2);
                W(Box("porthole_s" + s.ToString("0", CultureInfo.InvariantCulture), c.x - 0.02, c.y - 0.3, c.z - 0.3, c.x + 0.02, c.y + 0.3, c.z + 0.3, Rule.InFrame, "0.6 m round in the east wall at the landing (height assumed at eye - 0.2)", "14.5.5"), s - 30, s + 10);
            }
            for (int k = 0; k < BulwarkConst.PaneCount; k++) W(PaneSubject(k, Rule.InFrame, "", "14.5.6"), 1070, 1370);
            for (int i = 0; i < BulwarkConst.SurveyLampS.Length; i++)
            {
                double s = BulwarkConst.SurveyLampS[i];
                D3 c = PathPoint(s, BulwarkConst.GlassAxisOffset, BwPath.FloorY(s) + 4.0);
                W(Box("survey_lamp_" + (i == 0 ? "A" : "B"), c.x, c.y - 0.5, c.z - 0.5, c.x + 1, c.y + 0.5, c.z + 0.5, Rule.InFrame, "pier reveal at the glass plane (height assumed floor + 4)", "14.5.6"), s - 40, s + 20);
            }
            W(Lantern(true), 1370, 1500);
            W(Pts("jets", JetPoints(), Rule.InFrame, "", "14.5.7"), 1422, 1500);
            {
                SunAt(1452, out double saz, out double sel);
                W(Dirs("sun_disc_dusk", DiscDirs(saz, sel, 0.276, 8), Rule.InFrame, 30000, "", "14.5.7"), 1422, 1500);
            }
            return l;
        }

        const double WallLat = 0.35;   // wall plane beyond the lane edge (14.4: lane = collider gap minus 2 x 0.35)

        // ================================================================ evaluation
        public sealed class AspectResult
        {
            public string Aspect;
            public Extent E;
            public bool InsideMargin, InFramePart, CentreOk, FillsFrame;
        }

        public sealed class SubjectResult
        {
            public Subject Sub;
            public AspectResult[] Aspects;
            public double W1080 = double.NaN, H1080 = double.NaN, W4K = double.NaN, H4K = double.NaN;   // 16:9 unclipped px
            public double Row4K = double.NaN, Col4K = double.NaN, TopRow4K = double.NaN;
            public double UnoccludedBuilt = -1, UnoccludedAnalytic = -1, EdgeDistance = double.NaN;
            public string TopBlocker = "";   // the most frequent blocker of the built-geometry rays (name and first distance)
            public int OcclusionSamples;
            public bool Pass = true;
            public readonly List<string> Reasons = new List<string>();
            public double UnitPx4K = double.NaN;   // KS10: a 40 m unit at the subject's distance
            public string PxNote = "";             // why the px sizes are n/a (a box partly behind the camera)
        }

        public sealed class ShotResult
        {
            public KeyShot Shot;
            public string Id;
            public Pose Pose;
            public double SProj, Lateral, LaneLo, LaneHi, EyeDy, CineLateral, SDiff;
            public bool InLane, OnCinePath, StaticStill;
            public Follower F;
            public readonly List<SubjectResult> Target = new List<SubjectResult>();
            public readonly List<SubjectResult> AtFollower = new List<SubjectResult>();
        }

        public sealed class Finding
        {
            public string Id, Shot, Subject, Kind, Text, Arithmetic;
        }

        public sealed class SweepResult
        {
            public Subject Sub;
            public double SecondsInFrame, SecondsInside, BestS = double.NaN, BestSize4K, FirstS = double.NaN, LastS = double.NaN;
            /// <summary>In frame AND unoccluded (built ray casts and, below the horizon, the analytic floor rule) at least
            /// MinVisible at that pose; SecondsInsideVisible: fully inside the 3 % margin and visible.</summary>
            public double SecondsVisible, SecondsInsideVisible, FirstVisibleS = double.NaN, LastVisibleS = double.NaN;
            public double UnoccludedAtBest = -1;
            public int Poses, OccludedPoses;
        }

        public sealed class Report
        {
            public readonly List<ShotResult> Shots = new List<ShotResult>();
            public readonly List<Finding> Findings = new List<Finding>();
            public readonly List<SweepResult> Sweep = new List<SweepResult>();
            public bool BuiltOcclusion;
            public string BuiltDescription = "";
        }

        /// <summary>Evaluates one subject at one pose (all aspects, sizes, expectations, occlusion).</summary>
        public static SubjectResult Evaluate(Subject sub, in Pose p, double sEye, OcclusionTester occ)
        {
            var r = new SubjectResult { Sub = sub, Aspects = new AspectResult[Aspects.Length] };
            for (int a = 0; a < Aspects.Length; a++)
            {
                double asp = (double)Aspects[a].W / Aspects[a].H;
                var e = Project(sub, p, asp);
                var ar = new AspectResult { Aspect = Aspects[a].Name, E = e };
                ar.InsideMargin = e.InsideMargin(Margin);
                double vx0 = Math.Max(-1, e.X0), vx1 = Math.Min(1, e.X1), vy0 = Math.Max(-1, e.Y0), vy1 = Math.Min(1, e.Y1);
                ar.InFramePart = e.Any && vx1 > vx0 && vy1 > vy0 && (sub.Kind == SubjectKind.Box || e.Inside > 0);
                double cx = 0.5 * (vx0 + vx1), cy = 0.5 * (vy0 + vy1);
                ar.CentreOk = ar.InFramePart && Math.Abs(cx) <= 0.9 && Math.Abs(cy) <= 0.9;
                if (sub.Rule == Rule.Fills)
                {
                    bool all = true;
                    for (int c = 0; c < 4 && all; c++)
                    {
                        D3 d = RayDir(p, asp, (c & 1) == 0 ? -1 : 1, (c & 2) == 0 ? -1 : 1);
                        all = RayBox(p.Eye, d, sub.Min, sub.Max, out double ti, out _) && ti > 0;
                    }
                    ar.FillsFrame = all;
                }
                r.Aspects[a] = ar;
            }
            // px sizes at 16:9 (unclipped extent); n/a for a subject partly behind the camera (its extent then runs through the
            // near-plane clip points, so the 'size' would be meaningless, e.g. the plume box of KS08)
            var e169 = r.Aspects[0].E;
            if (e169.Any && e169.PartlyBehind && sub.Kind != SubjectKind.Directions)
                r.PxNote = "n/a: partly behind the camera (only the in-frame part is evaluated: rules and occlusion)";
            else if (e169.Any)
            {
                r.W1080 = 0.5 * (e169.X1 - e169.X0) * 1920; r.H1080 = 0.5 * (e169.Y1 - e169.Y0) * 1080;
                r.W4K = 2 * r.W1080; r.H4K = 2 * r.H1080;
                r.TopRow4K = (1 - e169.Y1) * 0.5 * 2160;
                r.Row4K = (1 - 0.5 * (e169.Y0 + e169.Y1)) * 0.5 * 2160;
                r.Col4K = (0.5 * (e169.X0 + e169.X1) + 1) * 0.5 * 3840;
            }
            if (sub.Kind == SubjectKind.Directions && sub.Points.Length >= 1 && sub.Rule != Rule.Absent)
            {
                r.Row4K = RowOf(p, sub.Points[0], 2160);
                r.Col4K = ColOf(p, sub.Points[0], 3840, 2160);
            }

            // rule
            bool insideAll = true, partAll = true, fillsAll = true, anyInside = false;
            foreach (var ar in r.Aspects)
            {
                insideAll &= ar.InsideMargin;
                partAll &= ar.InFramePart && (sub.Kind != SubjectKind.Box || ar.CentreOk);
                fillsAll &= ar.FillsFrame || ar.Aspect == sub.FillsExemptAspect;
                anyInside |= ar.E.Inside > 0;
            }
            switch (sub.Rule)
            {
                case Rule.Inside: if (!insideAll) Fail(r, "not inside the frame with the 3 % margin at " + FailedAspects(r, ar => ar.InsideMargin)); break;
                case Rule.InFrame: if (!partAll) Fail(r, "no centred in-frame part at " + FailedAspects(r, ar => ar.InFramePart && (sub.Kind != SubjectKind.Box || ar.CentreOk))); break;
                case Rule.Fills: if (!fillsAll) Fail(r, "does not fill the frame (a frame corner misses the subject) at " + FailedAspects(r, ar => ar.FillsFrame)); break;
                case Rule.Absent: if (anyInside) Fail(r, "must be absent but is in frame"); break;
            }
            // expectations (4K, 16:9)
            if (!double.IsNaN(sub.ExpectRow4K))
            {
                bool top = sub.Kind == SubjectKind.Box || sub.Name == "following_flight";   // boxes and the flight: their top row
                Check(r, top ? "top row" : "row", top ? r.TopRow4K : r.Row4K, sub.ExpectRow4K, sub.PosTol4K, true);
            }
            Check(r, "column", r.Col4K, sub.ExpectCol4K, sub.PosTol4K, true);
            Check(r, "height px", r.H4K, sub.ExpectHeight4K, sub.SizeTol4K, true);
            Check(r, "width px", r.W4K, sub.ExpectWidth4K, sub.SizeTol4K, true);
            if (!double.IsNaN(sub.MinHeight4K) && !(r.H4K >= sub.MinHeight4K)) Fail(r, "height " + F(r.H4K) + " px < " + F(sub.MinHeight4K) + " at 4K");
            if (!double.IsNaN(sub.MinWidth4K) && sub.Name.StartsWith("islet", StringComparison.Ordinal))
            {
                D3 c = (sub.Min + sub.Max) * 0.5;
                double dist = (c - p.Eye).Length;
                r.UnitPx4K = BulwarkConst.UnitSize / dist * QaProjection.FocalPx(p.fov, 2160);
                if (r.UnitPx4K < sub.MinWidth4K) r.Reasons.Add("a 40 m unit is " + F(r.UnitPx4K) + " px at " + F(dist) + " m (< 200 px; informative, acceptance needs two islets)");
            }
            else if (!double.IsNaN(sub.MinWidth4K) && !(r.W4K >= sub.MinWidth4K)) Fail(r, "width " + F(r.W4K) + " px < " + F(sub.MinWidth4K) + " at 4K");

            // occlusion (16:9 in-frame part)
            Occlusion(r, sub, p, sEye, occ);
            if (sub.MustBeVisible && sub.Rule != Rule.Absent && sub.Rule != Rule.Info && sub.Rule != Rule.Row)
            {
                if (r.UnoccludedAnalytic >= 0 && r.UnoccludedAnalytic < sub.MinVisible) Fail(r, "hidden by the walkable floor (11.13 analytic rule): unoccluded " + F(r.UnoccludedAnalytic * 100) + " %");
            }
            return r;
        }

        static string FailedAspects(SubjectResult r, Func<AspectResult, bool> ok)
        {
            var l = new List<string>();
            foreach (var ar in r.Aspects) if (!ok(ar)) l.Add(ar.Aspect);
            return string.Join(", ", l);
        }

        static void Fail(SubjectResult r, string why) { r.Pass = false; r.Reasons.Add(why); }

        static void Check(SubjectResult r, string what, double got, double expect, double tol, bool applies)
        {
            if (!applies || double.IsNaN(expect)) return;
            if (double.IsNaN(got) || Math.Abs(got - expect) > tol)
                Fail(r, what + " " + F(got) + " vs spec " + F(expect) + " +-" + F(tol) + " (4K)");
        }

        static string F(double v) => double.IsNaN(v) ? "n/a" : v.ToString("0.#", CultureInfo.InvariantCulture);

        static void Occlusion(SubjectResult r, Subject sub, in Pose p, double sEye, OcclusionTester occ, int grid = 8, int maxPoints = 64)
        {
            var rays = new List<(D3 dir, double dist)>(64);
            D3 eye = p.Eye;
            double asp = 16.0 / 9.0;
            if (sub.Kind == SubjectKind.Box)
            {
                var e = r.Aspects[0].E;
                double x0 = Math.Max(-1, e.X0), x1 = Math.Min(1, e.X1), y0 = Math.Max(-1, e.Y0), y1 = Math.Min(1, e.Y1);
                if (e.Any && x1 > x0 && y1 > y0)
                    for (int iy = 0; iy < grid; iy++)
                        for (int ix = 0; ix < grid; ix++)
                        {
                            D3 d = RayDir(p, asp, x0 + (x1 - x0) * (ix + 0.5) / grid, y0 + (y1 - y0) * (iy + 0.5) / grid);
                            if (!RayBox(eye, d, sub.Min, sub.Max, out double ti, out double to)) continue;
                            double entry = Math.Max(ti, Near);
                            if (to < entry) continue;
                            rays.Add((d, entry));
                        }
            }
            else
            {
                int step = Math.Max(1, sub.Points.Length / Math.Max(1, maxPoints));
                for (int i = 0; i < sub.Points.Length; i += step)
                {
                    D3 q = sub.Points[i];
                    if (sub.Kind == SubjectKind.Directions) rays.Add((q.Normalized, sub.Reach));
                    else { D3 v = q - eye; double d = v.Length; if (d > 1e-6) rays.Add((v * (1.0 / d), d)); }
                }
            }
            r.OcclusionSamples = rays.Count;
            if (rays.Count == 0) return;
            if (occ != null)
            {
                int vis = 0;
                var counts = new Dictionary<string, int>();
                var first = new Dictionary<string, string>();
                foreach (var ray in rays)
                {
                    if (!occ(eye, ray.dir, Math.Max(0.0, ray.dist - 0.05), out string blocker)) { vis++; continue; }
                    string key = blocker ?? "?";
                    int at = key.IndexOf('@');
                    string name = at > 0 ? key.Substring(0, at) : key;
                    counts.TryGetValue(name, out int c);
                    counts[name] = c + 1;
                    if (!first.ContainsKey(name)) first[name] = key;
                }
                r.UnoccludedBuilt = (double)vis / rays.Count;
                int best = 0;
                foreach (var kv in counts) if (kv.Value > best) { best = kv.Value; r.TopBlocker = first[kv.Key] + " (" + kv.Value + " rays)"; }
            }
            if (sub.BelowHorizon)
            {
                int vis = 0;
                foreach (var ray in rays)
                {
                    bool blocked = FloorBlocks(sEye, eye, ray.dir, double.IsNaN(r.EdgeDistance), out double dEdge, out double dHit);
                    if (double.IsNaN(r.EdgeDistance)) r.EdgeDistance = dEdge;
                    if (blocked && dHit < ray.dist) continue;
                    vis++;
                }
                r.UnoccludedAnalytic = (double)vis / rays.Count;
            }
        }

        /// <summary>Runs the whole audit: every key shot (target and follower pose), the findings and the reachability sweep.</summary>
        public static Report Run(OcclusionTester occ, Action<double> poseWorld = null, string builtDescription = "", double sweepStep = 1.0, int seed = 1)
        {
            var rep = new Report { BuiltOcclusion = occ != null, BuiltDescription = builtDescription };
            var subs = new List<Subject>(32);
            foreach (var k in BulwarkConst.KeyShots)
            {
                string id = KeyId(k.tag);
                bool ending = k.s > BwPath.Length + 1e-6;   // the ride and the platform (EndingPath): end-camera / ride poses
                var sr = new ShotResult { Shot = k, Id = id, Pose = PoseOf(k), StaticStill = id == "KS02" || ending };
                if (ending)
                {
                    // pose check against the ending timeline: the eye of EndingPath at s (cabin spot / platform path)
                    SequenceSpec.EyeAt(k.s, out double ex, out double ey, out double ez);
                    double d = Math.Sqrt((ex - k.x) * (ex - k.x) + (ey - k.y) * (ey - k.y) + (ez - k.z) * (ez - k.z));
                    sr.SProj = k.s; sr.SDiff = 0; sr.InLane = true; sr.OnCinePath = d <= 0.05; sr.EyeDy = k.y - ey;
                    // a static still (the ride / the end camera, no AutoWalker): the follower is the target pose itself
                    sr.F = new Follower { sStart = k.s, s = k.s, x = k.x, y = k.y, z = k.z, yaw = k.yaw, pitch = k.pitch, fromRest = true };
                    if (d > 0.5) rep.Findings.Add(new Finding { Id = id + "-pose-path", Shot = id, Kind = "pose", Text = "ending key-shot pose not at the EndingPath eye", Arithmetic = "eye (" + F3(ex) + ", " + F3(ey) + ", " + F3(ez) + ") at s " + F3(k.s) + ", distance " + F3(d) + " m" });
                    poseWorld?.Invoke(k.s);
                    subs.Clear();
                    SubjectsForShot(id, subs);
                    foreach (var sub in subs)
                    {
                        var r = Evaluate(sub, sr.Pose, k.s, occ);
                        sr.Target.Add(r);
                        sr.AtFollower.Add(r);   // ToJson pairs every target result with a follower result
                        if (!r.Pass && (sub.MustBeVisible || sub.Rule == Rule.Absent)) rep.Findings.Add(new Finding { Id = id + "-" + sub.Name, Shot = id, Subject = sub.Name, Kind = "target", Text = string.Join("; ", r.Reasons), Arithmetic = Arith(r, sr.Pose) });
                    }
                    rep.Shots.Add(sr);
                    continue;
                }
                // pose checks
                sr.SProj = BwPath.Project(k.x, k.z, k.s);
                sr.SDiff = sr.SProj - k.s;
                sr.Lateral = BwPath.Lateral(k.x, k.z, sr.SProj);
                BwPath.Lane(sr.SProj, out sr.LaneLo, out sr.LaneHi);
                sr.InLane = sr.Lateral >= sr.LaneLo - 1e-3 && sr.Lateral <= sr.LaneHi + 1e-3;
                sr.EyeDy = k.y - BwPath.EyeY(sr.SProj);
                sr.CineLateral = BwPath.CinematicLateral(sr.SProj);
                sr.OnCinePath = Math.Abs(sr.Lateral - sr.CineLateral) <= 0.05 && Math.Abs(sr.EyeDy) <= 0.05 && Math.Abs(sr.SDiff) <= 0.5;
                if (!sr.InLane) rep.Findings.Add(new Finding { Id = id + "-pose-lane", Shot = id, Kind = "pose", Text = "key-shot pose outside BwPath.Lane", Arithmetic = "lateral " + F3(sr.Lateral) + " outside [" + F3(sr.LaneLo) + ", " + F3(sr.LaneHi) + "] at s " + F3(sr.SProj) });
                if (!sr.OnCinePath && !sr.StaticStill) rep.Findings.Add(new Finding { Id = id + "-pose-path", Shot = id, Kind = "pose", Text = "key-shot pose not on the cinematic path", Arithmetic = "lateral " + F3(sr.Lateral) + " vs CinematicLateral " + F3(sr.CineLateral) + ", eye dy " + F3(sr.EyeDy) + ", s diff " + F3(sr.SDiff) });

                poseWorld?.Invoke(k.s);
                subs.Clear();
                SubjectsForShot(id, subs);
                foreach (var sub in subs)
                {
                    var r = Evaluate(sub, sr.Pose, sr.SProj, occ);
                    sr.Target.Add(r);
                    if (!r.Pass && sub.MustBeVisible) rep.Findings.Add(new Finding { Id = id + "-" + sub.Name, Shot = id, Subject = sub.Name, Kind = "target", Text = string.Join("; ", r.Reasons), Arithmetic = Arith(r, sr.Pose) });
                    else if (!r.Pass && sub.Rule == Rule.Absent) rep.Findings.Add(new Finding { Id = id + "-" + sub.Name, Shot = id, Subject = sub.Name, Kind = "target", Text = string.Join("; ", r.Reasons), Arithmetic = Arith(r, sr.Pose) });
                    if (occ != null && sub.MustBeVisible && r.UnoccludedBuilt >= 0 && r.UnoccludedBuilt < sub.MinVisible && sub.Rule != Rule.Row && sub.Rule != Rule.Info)
                        rep.Findings.Add(new Finding { Id = id + "-" + sub.Name + "-built", Shot = id, Subject = sub.Name, Kind = "built-occlusion", Text = "hidden by the built geometry: unoccluded " + F(r.UnoccludedBuilt * 100) + " % of " + r.OcclusionSamples + " rays; main blocker " + r.TopBlocker, Arithmetic = builtDescription });
                }

                // follower
                sr.F = SimulateFollower(k.s, sr.Pose, seed);
                var fp = sr.F.ToPose(k.fov);
                foreach (var sub in subs)
                {
                    var r = Evaluate(sub, fp, sr.F.s, occ);
                    sr.AtFollower.Add(r);
                    if (!sr.StaticStill && !r.Pass && sub.MustBeVisible)
                    {
                        bool targetOk = false;
                        foreach (var t in sr.Target) if (t.Sub == sub) targetOk = t.Pass;
                        if (targetOk) rep.Findings.Add(new Finding { Id = id + "-" + sub.Name + "-follower", Shot = id, Subject = sub.Name, Kind = sub.Rule == Rule.Row ? "follower-row" : "follower", Text = "passes at the target pose but not at the follower pose: " + string.Join("; ", r.Reasons), Arithmetic = "follower yaw " + F3(sr.F.yaw) + " pitch " + F3(sr.F.pitch) + " (lag " + F3(sr.F.lagDeg) + " deg, yaw " + F3(sr.F.yawLagDeg) + ", pitch " + F3(sr.F.pitchLagDeg) + ")" });
                    }
                }
                if (!sr.StaticStill && sr.F.lagDeg > MaxFollowerLagDeg)
                    rep.Findings.Add(new Finding { Id = id + "-lag", Shot = id, Kind = "follower-lag", Text = "AutoWalker follower lags the key-shot pose by " + F3(sr.F.lagDeg) + " deg (> 2)", Arithmetic = "simulated from s " + F3(sr.F.sStart) + " at 2.5 m/s: yaw " + F3(sr.F.yaw) + " vs " + F3(k.yaw) + ", pitch " + F3(sr.F.pitch) + " vs " + F3(k.pitch) + "; gaze keys " + GazeAround(k.s) });
                rep.Shots.Add(sr);
            }
            Sweep(rep, occ, poseWorld, sweepStep, seed);
            return rep;
        }

        static string GazeAround(double s)
        {
            var k = GazeSchedule.Keys;
            for (int i = 0; i + 1 < k.Length; i++)
                if (k[i].S <= s && k[i + 1].S >= s)
                    return "(" + F(k[i].S) + "," + F(k[i].Yaw) + "," + F(k[i].Pitch) + ") (" + F(k[i + 1].S) + "," + F(k[i + 1].Yaw) + "," + F(k[i + 1].Pitch) + ")";
            return "";
        }

        static string F3(double v) => double.IsNaN(v) ? "n/a" : v.ToString("0.###", CultureInfo.InvariantCulture);

        static string Arith(SubjectResult r, in Pose p)
        {
            var sb = new System.Text.StringBuilder();
            sb.Append("pose (").Append(F3(p.x)).Append(", ").Append(F3(p.y)).Append(", ").Append(F3(p.z)).Append(") yaw ").Append(F3(p.yaw)).Append(" pitch ").Append(F3(p.pitch)).Append(" vFOV ").Append(F3(p.fov));
            sb.Append("; focal 4K ").Append(F(QaProjection.FocalPx(p.fov, 2160))).Append(" px");
            foreach (var ar in r.Aspects)
                sb.Append("; ").Append(ar.Aspect).Append(" ndc x [").Append(F3(ar.E.X0)).Append(", ").Append(F3(ar.E.X1)).Append("] y [").Append(F3(ar.E.Y0)).Append(", ").Append(F3(ar.E.Y1)).Append("] limit +-").Append(F3(1 - 2 * Margin)).Append(ar.E.PartlyBehind ? " (partly behind)" : "");
            sb.Append("; 4K w x h ").Append(F(r.W4K)).Append(" x ").Append(F(r.H4K)).Append(", row ").Append(F(r.Row4K)).Append(", col ").Append(F(r.Col4K));
            if (!double.IsNaN(r.EdgeDistance)) sb.Append("; nearest open floor edge ").Append(F3(r.EdgeDistance)).Append(" m");
            return sb.ToString();
        }

        static void Sweep(Report rep, OcclusionTester occ, Action<double> poseWorld, double step, int seed)
        {
            var walkS = new List<double>(2048);
            var walk = SimulateWalk(step, walkS, seed);
            foreach (var sub0 in NamedSubjects())
            {
                var res = new SweepResult { Sub = sub0 };
                double best = -1;
                for (int wi = 0; wi < walk.Count; wi++)
                {
                    var p = walk[wi];
                    double s = walkS[wi];
                    if (s < sub0.SweepS0 || s > sub0.SweepS1) continue;
                    var sub = sub0.AtS != null ? sub0.AtS(s) : sub0;
                    res.Poses++;
                    var e = Project(sub, p, 16.0 / 9.0);
                    bool part = e.Any && e.X1 > -1 && e.X0 < 1 && e.Y1 > -1 && e.Y0 < 1 && (sub.Kind == SubjectKind.Box || e.Inside > 0);
                    if (!part) continue;
                    res.SecondsInFrame += step / FollowerSpeed;
                    bool inside = e.InsideMargin(Margin);
                    if (inside) res.SecondsInside += step / FollowerSpeed;
                    if (double.IsNaN(res.FirstS)) res.FirstS = s;
                    res.LastS = s;
                    // occlusion at this pose (11.13: what a walking camera can actually see): 3 x 3 rays over the in-frame part
                    // of a box, at most 9 sample points / directions; built ray casts and the analytic floor rule
                    var qr = new SubjectResult { Sub = sub, Aspects = new[] { new AspectResult { Aspect = "16:9", E = e } } };
                    poseWorld?.Invoke(s);
                    Occlusion(qr, sub, p, s, occ, 3, 9);
                    bool visible = (qr.UnoccludedBuilt < 0 || qr.UnoccludedBuilt >= sub.MinVisible) && (qr.UnoccludedAnalytic < 0 || qr.UnoccludedAnalytic >= sub.MinVisible);
                    if (!visible) { res.OccludedPoses++; continue; }
                    res.SecondsVisible += step / FollowerSpeed;
                    if (inside) res.SecondsInsideVisible += step / FollowerSpeed;
                    if (double.IsNaN(res.FirstVisibleS)) res.FirstVisibleS = s;
                    res.LastVisibleS = s;
                    double size = 0.5 * Math.Max(Math.Min(1, e.Y1) - Math.Max(-1, e.Y0), 0) * 2160;
                    if (size > best) { best = size; res.BestS = s; res.BestSize4K = size; }
                }
                if (!double.IsNaN(res.BestS) && occ != null)
                {
                    Pose bp = default;
                    double bd = double.MaxValue;
                    for (int wi = 0; wi < walk.Count; wi++) { double d = Math.Abs(walkS[wi] - res.BestS); if (d < bd) { bd = d; bp = walk[wi]; } }
                    poseWorld?.Invoke(res.BestS);
                    var sub = sub0.AtS != null ? sub0.AtS(res.BestS) : sub0;
                    var r = Evaluate(sub, bp, res.BestS, occ);
                    res.UnoccludedAtBest = r.UnoccludedBuilt;
                }
                rep.Sweep.Add(res);
                if (res.SecondsInFrame <= 0) rep.Findings.Add(new Finding { Id = "sweep-" + sub0.Name, Subject = sub0.Name, Kind = "reachability", Text = "never in frame of the cinematic follower within s " + F(sub0.SweepS0) + "-" + F(sub0.SweepS1), Arithmetic = sub0.Note });
                else if (res.SecondsVisible <= 0) rep.Findings.Add(new Finding { Id = "sweep-" + sub0.Name + "-hidden", Subject = sub0.Name, Kind = "reachability", Text = "in frame " + F(res.SecondsInFrame) + " s but never unoccluded (>= " + F(sub0.MinVisible * 100) + " %) within s " + F(sub0.SweepS0) + "-" + F(sub0.SweepS1), Arithmetic = rep.BuiltDescription });
                if (!double.IsNaN(sub0.SpecSeconds))
                {
                    double got = sub0.SpecSecondsWhole ? res.SecondsInsideVisible : res.SecondsVisible;
                    if (Math.Abs(got - sub0.SpecSeconds) > Math.Max(1.0, 0.3 * sub0.SpecSeconds))
                        rep.Findings.Add(new Finding { Id = "sweep-" + sub0.Name + "-time", Subject = sub0.Name, Kind = "reachability-time", Text = (sub0.SpecSecondsWhole ? "whole subject inside and visible " : "visible ") + F(got) + " s at 2.5 m/s vs the spec's " + F(sub0.SpecSeconds) + " s (in frame " + F(res.SecondsInFrame) + " s, " + res.OccludedPoses + " occluded poses)", Arithmetic = "cinematic follower (WalkPlanner seed 1 + GazeSchedule) sampled every " + F(step) + " m of s; visible = built ray casts and the analytic floor rule >= " + F(sub0.MinVisible * 100) + " % of 9 rays; " + sub0.Note });
                }
            }
        }

        // ================================================================ JSON
        public static string ToJson(Report rep)
        {
            var j = new QaJson();
            j.BeginObject();
            j.Prop("spec", "MASTER_PROMPT 11.13 visibility and reachability audit (14.10.2 key shots, Part 14 named subjects)");
            j.Prop("margin", Margin).Prop("maxFollowerLagDeg", MaxFollowerLagDeg).Prop("followerLeadInM", FollowerLeadIn).Prop("followerSpeed", FollowerSpeed);
            j.Prop("follower", "the real WalkPlanner (seed 1) + GazeSchedule, 1/60 s steps, critically damped omega 2.5, rate limits 18 / 13 deg/s, sway <= 0.3 deg");
            j.Key("aspects").BeginArray(true);
            foreach (var a in Aspects) j.Value(a.Name + " " + a.W + "x" + a.H);
            j.EndArray();
            j.Prop("occlusionBuilt", rep.BuiltOcclusion ? rep.BuiltDescription : "not run (no occlusion tester)");
            j.Prop("occlusionAnalytic", "11.13 rule for subjects below the horizon: the spec floor layout (crest deck x -6..6, see-through gratings over the lane on the spillway slots, gallery floor to axis + 3.0, ledge slab x -12.4..-6.0 z 1415.2..1503)");
            int fails = 0;
            foreach (var f in rep.Findings) fails++;
            j.Prop("findingCount", fails);
            j.Key("shots").BeginArray();
            foreach (var sr in rep.Shots)
            {
                j.BeginObject();
                j.Prop("id", sr.Id).Prop("tag", sr.Shot.tag).Prop("s", sr.Shot.s);
                j.Key("pose").BeginObject(true).Prop("x", sr.Pose.x).Prop("y", sr.Pose.y).Prop("z", sr.Pose.z).Prop("yaw", sr.Pose.yaw).Prop("pitch", sr.Pose.pitch).Prop("vfov", sr.Pose.fov).EndObject();
                j.Key("poseCheck").BeginObject(true).Prop("sProjected", sr.SProj).Prop("sDiff", sr.SDiff).Prop("lateral", sr.Lateral).Prop("laneLo", sr.LaneLo).Prop("laneHi", sr.LaneHi)
                 .Prop("inLane", sr.InLane).Prop("eyeDy", sr.EyeDy).Prop("cinematicLateral", sr.CineLateral).Prop("onCinematicPath", sr.OnCinePath).Prop("staticStill", sr.StaticStill).EndObject();
                j.Key("follower").BeginObject(true).Prop("startS", sr.F.sStart).Prop("fromRest", sr.F.fromRest).Prop("s", sr.F.s).Prop("lateral", sr.F.lateral)
                 .Prop("x", sr.F.x).Prop("y", sr.F.y).Prop("z", sr.F.z).Prop("yaw", sr.F.yaw).Prop("pitch", sr.F.pitch)
                 .Prop("lagDeg", sr.F.lagDeg).Prop("yawLagDeg", sr.F.yawLagDeg).Prop("pitchLagDeg", sr.F.pitchLagDeg).Prop("positionErrorM", sr.F.posErr).Prop("lagFinding", !sr.StaticStill && sr.F.lagDeg > MaxFollowerLagDeg).EndObject();
                j.Key("subjects").BeginArray();
                for (int i = 0; i < sr.Target.Count; i++) WriteSubject(j, sr.Target[i], sr.AtFollower[i]);
                j.EndArray();
                j.EndObject();
            }
            j.EndArray();
            j.Prop("reachabilityVisibility", "a sweep pose counts as visible when the subject is in frame and at least MinVisible (30 %, lower for thin subjects seen through an opening) of 9 rays reach it: built ray casts and, below the horizon, the analytic floor rule");
            j.Key("reachabilitySweep").BeginArray();
            foreach (var w in rep.Sweep)
            {
                j.BeginObject(true).Prop("name", w.Sub.Name).Prop("source", w.Sub.Source).Prop("window", "s " + F(w.Sub.SweepS0) + "-" + F(w.Sub.SweepS1))
                 .Prop("secondsInFrameAt2p5", w.SecondsInFrame).Prop("secondsFullyInsideAt2p5", w.SecondsInside)
                 .Prop("secondsVisibleAt2p5", w.SecondsVisible).Prop("secondsFullyInsideVisibleAt2p5", w.SecondsInsideVisible).Prop("occludedPoses", w.OccludedPoses)
                 .Prop("specSecondsAt2p5", w.Sub.SpecSeconds).Prop("firstS", w.FirstS).Prop("lastS", w.LastS).Prop("firstVisibleS", w.FirstVisibleS).Prop("lastVisibleS", w.LastVisibleS)
                 .Prop("bestS", w.BestS).Prop("bestHeightPx4K", w.BestSize4K).Prop("unoccludedBuiltAtBest", w.UnoccludedAtBest).Prop("note", w.Sub.Note).EndObject();
            }
            j.EndArray();
            j.Key("findings").BeginArray();
            foreach (var f in rep.Findings)
                j.BeginObject().Prop("id", f.Id).Prop("shot", f.Shot ?? "").Prop("subject", f.Subject ?? "").Prop("kind", f.Kind).Prop("text", f.Text).Prop("arithmetic", f.Arithmetic ?? "").EndObject();
            j.EndArray();
            j.EndObject();
            return j.ToString();
        }

        static void WriteSubject(QaJson j, SubjectResult r, SubjectResult f)
        {
            var s = r.Sub;
            j.BeginObject();
            j.Prop("name", s.Name).Prop("kind", s.Kind.ToString()).Prop("rule", s.Rule.ToString()).Prop("mustBeVisible", s.MustBeVisible).Prop("source", s.Source).Prop("note", s.Note);
            if (s.Kind == SubjectKind.Box)
            {
                j.Key("min").BeginArray(true).Value(s.Min.x).Value(s.Min.y).Value(s.Min.z).EndArray();
                j.Key("max").BeginArray(true).Value(s.Max.x).Value(s.Max.y).Value(s.Max.z).EndArray();
            }
            else j.Prop("samples", s.Points.Length);
            j.Prop("pass", r.Pass);
            j.Key("reasons").BeginArray(true);
            foreach (var x in r.Reasons) j.Value(x);
            j.EndArray();
            WriteAspects(j, "aspects", r);
            j.Key("px1080").BeginArray(true).Value(r.W1080).Value(r.H1080).EndArray();
            if (!string.IsNullOrEmpty(r.PxNote)) j.Prop("pxNote", r.PxNote);
            j.Key("px4k").BeginArray(true).Value(r.W4K).Value(r.H4K).EndArray();
            j.Prop("row4k", r.Row4K).Prop("col4k", r.Col4K).Prop("topRow4k", r.TopRow4K);
            if (!double.IsNaN(s.ExpectRow4K)) j.Prop("expectRow4k", s.ExpectRow4K);
            if (!double.IsNaN(s.ExpectCol4K)) j.Prop("expectCol4k", s.ExpectCol4K);
            if (!double.IsNaN(s.ExpectHeight4K)) j.Prop("expectHeight4k", s.ExpectHeight4K);
            if (!double.IsNaN(s.ExpectWidth4K)) j.Prop("expectWidth4k", s.ExpectWidth4K);
            if (!double.IsNaN(r.UnitPx4K)) j.Prop("unit40mPx4k", r.UnitPx4K);
            j.Prop("unoccludedBuilt", r.UnoccludedBuilt).Prop("unoccludedAnalytic", r.UnoccludedAnalytic).Prop("occlusionRays", r.OcclusionSamples);
            if (!string.IsNullOrEmpty(r.TopBlocker)) j.Prop("mainBlocker", r.TopBlocker);
            if (!double.IsNaN(r.EdgeDistance)) j.Prop("nearestOpenFloorEdgeM", r.EdgeDistance);
            j.Key("atFollower").BeginObject();
            j.Prop("pass", f.Pass);
            j.Key("reasons").BeginArray(true);
            foreach (var x in f.Reasons) j.Value(x);
            j.EndArray();
            WriteAspects(j, "aspects", f);
            j.Key("px4k").BeginArray(true).Value(f.W4K).Value(f.H4K).EndArray();
            j.Prop("unoccludedBuilt", f.UnoccludedBuilt).Prop("unoccludedAnalytic", f.UnoccludedAnalytic);
            j.EndObject();
            j.EndObject();
        }

        static void WriteAspects(QaJson j, string key, SubjectResult r)
        {
            j.Key(key).BeginArray();
            foreach (var ar in r.Aspects)
            {
                j.BeginObject(true).Prop("aspect", ar.Aspect).Prop("any", ar.E.Any).Prop("partlyBehind", ar.E.PartlyBehind)
                 .Prop("insideMargin", ar.InsideMargin).Prop("inFramePart", ar.InFramePart).Prop("centreOk", ar.CentreOk);
                if (r.Sub.Rule == Rule.Fills) j.Prop("fillsFrame", ar.FillsFrame);
                if (r.Sub.Kind != SubjectKind.Box) j.Prop("samplesInside", ar.E.Inside).Prop("samples", ar.E.Samples);
                j.Key("ndc").BeginArray(true).Value(ar.E.X0).Value(ar.E.Y0).Value(ar.E.X1).Value(ar.E.Y1).EndArray();
                j.EndObject();
            }
            j.EndArray();
        }
    }
}
