// VisibilitySubjects: the named subjects of every key shot (MASTER_PROMPT 14.10 "Must be visible" column and the 14.5 key views
// KS00-KS19) as world-space sample points derived from the SPEC geometry (14.4, 14.5.x, the Phase-3 geometry agreements), never
// from builder meshes, each with the spec's projected numbers (NDC at 16:9, frame +-1, y up) where 14.5 states them.
// Derivations that the spec leaves implicit are recorded next to each subject (they reproduce the spec's own NDC numbers; see
// docs/logs/V.md): the KS01 bar window and sign sit on the recess back wall x -3.0 (counter x -3.0..-2.1); the boulevard blades are
// 14 x 7 m slabs x [18 + 36k, 32 + 36k] z 263..270 (north, y 40-240) and x [6 + 36k, 20 + 36k] z 120..127 (south, y 60-260); the KS02
// roof edges are the podium tops (x -2.2, y 22 and x 2.2, y 31: slit half-angles 6.19 / 4.29 deg); trains and the tram are placed by
// their 14.5.2 / 14.5.4 launch schedules in capture mode (tau = (s - s_launch) / 2.5); face B above y 0 was absent for z 115..275
// (agreement 5, the garden notch; GE's notch gate closes it above y 129), so the spec's "face B at x 0" of KS04 / KS07 is checked on
// the notch flanks.
// Rules: Whole = every sample inside the frame with the 3 % margin; Part = at least MinPartSamples samples inside the margin (large
// or open subjects: faces, facades, lanes, the cloud); Info = reported only.
using System;
using System.Collections.Generic;

namespace Megastructure
{
    public enum VisRule { Info = 0, Whole = 1, Part = 2 }

    /// <summary>Frame-composition check of a shot (ray grid over the frame).</summary>
    public enum VisComposition { None = 0, Drop = 1, Strip = 2, Wall = 3 }

    public sealed class VisSubject
    {
        public string name, note = "";
        public VisRule rule;
        public D3[] pts;
        /// <summary>Spec NDC bounds at 16:9 (NaN where the spec gives none) and the spec's wording.</summary>
        public double sx0 = double.NaN, sx1 = double.NaN, sy0 = double.NaN, sy1 = double.NaN;
        public string spec = "";
        /// <summary>Readability minimum: projected width in pixels at 1080p (0 = none).</summary>
        public double minWidthPx1080;
        /// <summary>Subject lies on a surface that may itself be built (Gulf faces, facades): wider self-hit tolerance.</summary>
        public bool surface;
        /// <summary>Built occluders whose renderer path contains one of these (case-insensitive) are the subject itself and are
        /// stepped over (a built face, bridge, layer or blade never hides its own spec sample points).</summary>
        public string[] self = Array.Empty<string>();
    }

    public sealed class VisShot
    {
        public string id, owner;
        public KeyShot ks;
        public readonly List<VisSubject> subjects = new List<VisSubject>();
        public VisComposition composition;
        /// <summary>Spec composition shares at 16:9 (sky, pier/deck, drop) or (wall share at 16:9, at 21:9) for KS16.</summary>
        public double[] specShares;
        public bool koi;            // the koi lint (KS17, KS18: whole koi; KS19: the body, D-029 GB-01)
        public bool koiWhole;
        /// <summary>D-029 (GB-01): the koi body is a partial subject of the shot (KS19: window, lamp and koi body; the head stands at
        /// 44.3 deg behind the 50.6 deg roof edge and is not a subject): at least MinPartSamples core samples visible inside the
        /// margin in every wave phase; the head is reported only.</summary>
        public bool koiBody;
        public double koiSpecMargin = double.NaN;
        public double[] koiSpecX;   // spec koi x range per aspect (16:9, 16:10, 21:9) as x0, x1 pairs
        public double koiSpecY0 = double.NaN, koiSpecY1 = double.NaN;
        public double headX0 = double.NaN, headX1 = double.NaN, headY0 = double.NaN, headY1 = double.NaN;
        public double readA = double.NaN, readB = double.NaN;   // spec readable window (s)
        public bool TDrop => ks.pitch < -5.0;
    }

    public static class VisibilitySubjects
    {
        public const int MinPartSamples = 3;

        // ------------------------------------------------------------------ sample-point builders
        public static D3[] Box(double x0, double x1, double y0, double y1, double z0, double z1, int n = 3)
        {
            var l = new List<D3>();
            for (int i = 0; i < n; i++)
                for (int j = 0; j < n; j++)
                    for (int k = 0; k < n; k++)
                    {
                        bool surf = i == 0 || j == 0 || k == 0 || i == n - 1 || j == n - 1 || k == n - 1;
                        if (!surf) continue;
                        l.Add(new D3(Lerp(x0, x1, i, n), Lerp(y0, y1, j, n), Lerp(z0, z1, k, n)));
                    }
            return l.ToArray();
        }

        static double Lerp(double a, double b, int i, int n) => n <= 1 ? 0.5 * (a + b) : a + (b - a) * i / (n - 1);

        /// <summary>Grid on the plane x = const.</summary>
        public static D3[] PlaneX(double x, double y0, double y1, double z0, double z1, int ny, int nz)
        {
            var p = new D3[ny * nz];
            for (int j = 0; j < ny; j++) for (int k = 0; k < nz; k++) p[j * nz + k] = new D3(x, Lerp(y0, y1, j, ny), Lerp(z0, z1, k, nz));
            return p;
        }

        /// <summary>Grid on the plane z = const.</summary>
        public static D3[] PlaneZ(double z, double x0, double x1, double y0, double y1, int nx, int ny)
        {
            var p = new D3[nx * ny];
            for (int i = 0; i < nx; i++) for (int j = 0; j < ny; j++) p[i * ny + j] = new D3(Lerp(x0, x1, i, nx), Lerp(y0, y1, j, ny), z);
            return p;
        }

        /// <summary>Grid on the plane y = const.</summary>
        public static D3[] PlaneY(double y, double x0, double x1, double z0, double z1, int nx, int nz)
        {
            var p = new D3[nx * nz];
            for (int i = 0; i < nx; i++) for (int k = 0; k < nz; k++) p[i * nz + k] = new D3(Lerp(x0, x1, i, nx), y, Lerp(z0, z1, k, nz));
            return p;
        }

        public static D3[] Line(D3 a, D3 b, int n)
        {
            var p = new D3[n];
            for (int i = 0; i < n; i++) { double t = n <= 1 ? 0.5 : (double)i / (n - 1); p[i] = a + (b - a) * t; }
            return p;
        }

        /// <summary>Gulf face A x_A(z) = 215.3 - F(|z - 195|) (14.4): grid over z and y.</summary>
        public static D3[] FaceA(double z0, double z1, int nz, double y0, double y1, int ny)
        {
            var l = new List<D3>(nz * ny);
            for (int k = 0; k < nz; k++)
            {
                double z = Lerp(z0, z1, k, nz);
                for (int j = 0; j < ny; j++) l.Add(new D3(CityField.GulfXA(z), Lerp(y0, y1, j, ny), z));
            }
            return l.ToArray();
        }

        /// <summary>Gulf face B x_B(z) (14.4); above y 0 the garden notch z 115..275 is cut away (agreement 5).</summary>
        public static D3[] FaceB(double z0, double z1, int nz, double y0, double y1, int ny)
        {
            var l = new List<D3>(nz * ny);
            for (int k = 0; k < nz; k++)
            {
                double z = Lerp(z0, z1, k, nz);
                for (int j = 0; j < ny; j++)
                {
                    double y = Lerp(y0, y1, j, ny);
                    if (y > 0.0 && z > 115.0 && z < 275.0) continue;
                    l.Add(new D3(CityField.GulfXB(z), y, z));
                }
            }
            return l.ToArray();
        }

        public static D3[] Ellipsoid(D3 c, double rx, double ry, double rz, int nLat = 5, int nLon = 12)
        {
            var l = new List<D3> { c + new D3(0, ry, 0), c - new D3(0, ry, 0) };
            for (int i = 1; i < nLat; i++)
            {
                double th = Math.PI * i / nLat;
                for (int j = 0; j < nLon; j++)
                {
                    double ph = 2.0 * Math.PI * j / nLon;
                    l.Add(c + new D3(rx * Math.Sin(th) * Math.Cos(ph), ry * Math.Cos(th), rz * Math.Sin(th) * Math.Sin(ph)));
                }
            }
            return l.ToArray();
        }

        /// <summary>The spire surface (14.5.7: R 32 at y 0 tapering to 20 at y 460, 20 to 560) as seen from the eye: the two
        /// silhouette lines and the near meridian (samples on the far side would be hidden by the spire itself).</summary>
        public static D3[] Spire(KeyShot k, double y0, double y1, int ny)
        {
            var l = new List<D3>();
            double dx = NocturneConstants.SpireX - k.x, dz = NocturneConstants.SpireZ - k.z, dh = Math.Sqrt(dx * dx + dz * dz);
            double azc = Math.Atan2(dx, dz);
            for (int j = 0; j < ny; j++)
            {
                double y = Lerp(y0, y1, j, ny), r = VisibilitySpecWorld.SpireRadius(y);
                double delta = Math.Acos(Math.Min(1.0, r / Math.Max(r + 1e-6, dh)));   // angle at the axis between the eye and a tangent point
                for (int side = -1; side <= 1; side++)
                {
                    // world direction from the axis toward the eye is azc + pi; the tangent points lie +-delta from it
                    double a = azc + Math.PI + side * delta * 0.98;
                    l.Add(new D3(NocturneConstants.SpireX + r * Math.Sin(a), y, NocturneConstants.SpireZ + r * Math.Cos(a)));
                }
            }
            return l.ToArray();
        }

        /// <summary>A train of 14.5.4 (7 x 24 m cars + 6 m nose, 3.4 m wide; body y axis +- 1.7) in a tube at (zAxis, yAxis);
        /// nose at xNose, the body trailing toward -dir. Returns the nose box and the car box.</summary>
        static void Train(double xNose, int dir, double yAxis, double zAxis, out D3[] nose, out D3[] cars)
        {
            double xn1 = xNose - dir * 6.0, xe = xNose - dir * 174.0;
            nose = Box(Math.Min(xNose, xn1), Math.Max(xNose, xn1), yAxis - 1.7, yAxis + 1.7, zAxis - 1.7, zAxis + 1.7, 3);
            var l = new List<D3>();
            for (int i = 0; i <= 28; i++)
            {
                double x = xn1 + (xe - xn1) * i / 28.0;
                l.Add(new D3(x, yAxis + 1.7, zAxis - 1.7)); l.Add(new D3(x, yAxis + 1.7, zAxis + 1.7));
                l.Add(new D3(x, yAxis - 1.7, zAxis - 1.7)); l.Add(new D3(x, yAxis - 1.7, zAxis + 1.7));
            }
            cars = l.ToArray();
        }

        static VisSubject S(VisShot shot, string name, VisRule rule, D3[] pts, double x0, double x1, double y0, double y1, string spec, string note = "")
        {
            var v = new VisSubject { name = name, rule = rule, pts = pts, sx0 = x0, sx1 = x1, sy0 = y0, sy1 = y1, spec = spec, note = note };
            shot.subjects.Add(v);
            return v;
        }

        const double N = double.NaN;

        static VisShot Shot(List<VisShot> list, string id, string owner, double readA, double readB)
        {
            var ks = NocturneConstants.KeyShots;
            for (int i = 0; i < ks.Length; i++)
                if (ks[i].tag.StartsWith(id, StringComparison.Ordinal))
                {
                    var sh = new VisShot { id = id, owner = owner, ks = ks[i], readA = readA, readB = readB };
                    list.Add(sh);
                    return sh;
                }
            throw new ArgumentException("unknown key shot " + id);
        }

        // ------------------------------------------------------------------ the shots
        /// <summary>Every key shot of 14.10 with its spec subjects, in s order.</summary>
        public static List<VisShot> All()
        {
            var L = new List<VisShot>(20);
            VisShot k;

            // ---------------- beat 1 (G1): 14.5.1
            k = Shot(L, "KS00", "G1", 0, 25);
            S(k, "mouth_slit", VisRule.Whole, PlaneZ(120.0, -2.2, 2.2, 0.0, 11.9, 5, 5), N, N, 0.03, 0.20,
              "4.4 m wide at 120 m: 33 px at 1080p, slit y +0.03..+0.20").minWidthPx1080 = 30.0;
            k = Shot(L, "KS01", "G1", 20, 45.5);
            S(k, "bar_window", VisRule.Whole, PlaneX(-3.0, 1.0, 2.3, 43.0, 47.0, 3, 5), -0.15, -0.02, -0.27, 0.02,
              "bar window x -0.15..-0.02, y -0.27..+0.02 (about 250 px wide at 4K)", "window 4.0 x 1.3 m on the recess back wall x -3.0, z 43..47");
            S(k, "bar_sign", VisRule.Whole, PlaneX(-3.0, 3.0, 4.4, 42.0, 48.0, 3, 5), -0.21, 0.04, 0.06, 0.57,
              "sign above x -0.21..+0.04, y +0.06..+0.57", "sign 6.0 x 1.4 m at y 3.0-4.4 on x -3.0, z 42..48");
            S(k, "counter", VisRule.Info, Box(-3.0, -2.1, 0.0, 1.0, 43.0, 47.0), N, N, -0.51, -0.22, "counter y -0.51..-0.22");
            k = Shot(L, "KS02", "G1", 84, 98);
            S(k, "zenith_cloud", VisRule.Part, PlaneY(460.0, -40.0, 30.0, 78.0, 98.0, 8, 3), N, N, 0.74, 0.74,
              "zenith cloud at y +0.74", "cloud base y 460 over the slit");
            S(k, "catwalk", VisRule.Whole, Box(-2.2, 2.2, 8.8, 9.2, AlleyFixtures.CatwalkZ0, AlleyFixtures.CatwalkZ1), -0.21, 0.21, -0.93, -0.70,
              "catwalk x +-0.21, y -0.93..-0.70", "D-029 (GB-12): z 96.0-97.5 (the spec's 97.6 put the bottom edge at NDC -0.9404)");
            S(k, "roof_edge_west", VisRule.Part, Line(new D3(-2.2, 22.0, 60.0), new D3(-2.2, 22.0, 120.0), 13), -0.10, -0.07, N, N,
              "west roof edge x -0.10..-0.07 (half-angle 6.19 deg)");
            S(k, "roof_edge_east", VisRule.Part, Line(new D3(2.2, 31.0, 60.0), new D3(2.2, 31.0, 120.0), 13), 0.06, 0.07, N, N,
              "east roof edge x +0.06..+0.07 (half-angle 4.29 deg)");
            // fix wave (lead decision, panel p4_01): KS02's tower_flank is dropped - hidden by the alley's west podium from every eye
            // within +-6 m of the path (visibility.json p4_01: 0.00 unoccluded, spec and built)
            k = Shot(L, "KS03", "G1", 106, 123.5);
            S(k, "north_facade_base", VisRule.Whole, PlaneZ(270.0, -30.0, 30.0, 0.0, 40.0, 7, 3), -0.18, 0.18, -0.42, 0.0,
              "north facade base x +-0.18, y -0.42..0", "podium 40 m, x -30..30");
            S(k, "blade_n54", VisRule.Whole, Box(54.0, 68.0, 40.0, 110.0, 263.0, 270.0), 0.28, 0.41, 0.0, 0.66, "blade x 54 (lower 70 m) x +0.28..+0.41, y 0..+0.66");
            S(k, "blade_n90", VisRule.Whole, Box(90.0, 104.0, 40.0, 110.0, 263.0, 270.0), 0.46, 0.62, 0.0, 0.66, "blade x 90 (lower 70 m) x +0.46..+0.62, y 0..+0.66");

            // ---------------- beat 2 (G1): 14.5.2
            k = Shot(L, "KS04", "G1", 205, 253);
            // D-040 (2): KS04's pitch 22 -> 27 (BV's and FX's requests; projections of FX's proj.py, asserted by BV's
            // Beat02BoulevardTests.KeyShots_Subjects); the Box / Line / FaceB geometry is unchanged
            S(k, "blade_n198", VisRule.Whole, Box(198.0, 212.0, 40.0, 140.0, 263.0, 270.0), -0.48, -0.30, -0.34, 0.54, "north blade x 198 (y 40-140) x -0.48..-0.30, y -0.34..+0.54 (D-040 pitch 27)");
            S(k, "blade_s186", VisRule.Whole, Box(186.0, 200.0, 60.0, 160.0, 120.0, 127.0), 0.31, 0.49, -0.10, 0.74, "south blade x 186 (y 60-160) x +0.31..+0.49, y -0.10..+0.74 (D-040 pitch 27)");
            S(k, "canyon_end", VisRule.Part, Line(new D3(215.3, 0.0, 125.0), new D3(215.3, 0.0, 265.0), 15), N, N, -0.81, -0.81, "canyon end at y -0.81 (D-040 pitch 27)");
            S(k, "face_B", VisRule.Part, FaceB(-105.0, 495.0, 21, 0.0, 200.0, 5), 0.0, 0.0, -0.79, -0.33,
              "far Gulf face (x 815.3, 750 m away) at x 0, y -0.79..-0.33 (D-040)",
              "agreement 5 cuts face B above y 0 away for z 115..275 (garden notch): the face is checked on the notch flanks").surface = true;
            S(k, "lane_n185", VisRule.Part, Line(new D3(100.0, 185.0, 245.0), new D3(800.0, 185.0, 245.0), 29), -0.36, -0.10, N, N, "north lane (y 112) x -0.36..-0.10",
              "GE GB-06: the lane rises from the spec's y 112 to 185 (it would enter the garden crowns at face B; NocTraffic, GateDims)");
            k = Shot(L, "KS06", "G1", 255, 275.5);
            {
                double xHead = 205.0 - 7.0 * Math.Max(0.0, (264.0 - 230.0) / 2.5);   // 14.5.2: launched at s 230, x(tau) = 205 - 7 tau
                S(k, "tram", VisRule.Whole, Box(xHead, xHead + 60.0, 0.3, 3.9, 225.3, 228.7), -0.24, 0.23, -0.43, -0.29,
                  "tram (x 109.8-169.8) x -0.24..+0.23, y -0.43..-0.29", "head x " + xHead.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture) + " (capture mode tau = (s - 230) / 2.5)");
            }
            S(k, "north_facade", VisRule.Part, PlaneZ(270.0, 100.0, 260.0, 40.0, 110.0, 9, 3), N, N, 0.54, 0.66, "north facade (y 40-110) above at y +0.54..+0.66").surface = true;
            k = Shot(L, "KS05", "G1", 301, 337);
            {
                var cl = new List<D3>();
                cl.AddRange(Line(new D3(200.0, 460.0, 270.0), new D3(800.0, 460.0, 270.0), 25));
                cl.AddRange(Line(new D3(200.0, 460.0, 120.0), new D3(800.0, 460.0, 120.0), 25));
                S(k, "cloud_lines", VisRule.Part, cl.ToArray(), -0.12, 0.12, 0.13, 0.13, "facade-cloud lines at x +-0.12, y +0.13 (rising apart toward the top)", "facade tops at the cloud base y 460").surface = true;
            }
            S(k, "blade_n198_top", VisRule.Whole, Box(198.0, 212.0, 200.0, 240.0, 263.0, 270.0), -0.25, -0.25, 0.50, 0.59, "north blade x 198 top x -0.25, y +0.50..+0.59");
            S(k, "blade_s186_top", VisRule.Whole, Box(186.0, 200.0, 220.0, 260.0, 120.0, 127.0), 0.24, 0.24, 0.62, 0.71, "south blade x 186 top x +0.24, y +0.62..+0.71");
            S(k, "blade_n162_mid", VisRule.Whole, Box(162.0, 176.0, 100.0, 180.0, 263.0, 270.0), -0.42, -0.40, 0.53, 0.69, "blade x 162 mid x -0.42..-0.40, y +0.53..+0.69");
            S(k, "lane_skyway_320", VisRule.Part, Line(new D3(130.0, 320.0, 195.0), new D3(800.0, 320.0, 195.0), 28), N, N, -0.08, -0.08, "skyway lane (y 320) y -0.08");

            // ---------------- beat 3 (G2; gate frames G3): 14.5.3
            k = Shot(L, "KS07", "G2", 390, 409);
            S(k, "garden_crowns", VisRule.Whole, Box(720.0, 850.0, 40.0, 130.0, 140.0, 250.0), -0.08, 0.08, 0.04, 0.34,
              "garden crowns (box x 720-850, y 40-130) x +-0.08, y +0.04..+0.34 (362 px tall at 4K)");
            {
                var dl = new List<D3>();
                dl.AddRange(PlaneX(683.0, 0.0, 8.0, 189.0, 201.0, 3, 5));
                dl.AddRange(Line(new D3(683.0, 0.0, 195.0), new D3(695.3, 0.0, 195.0), 5));
                S(k, "far_deck_gate_opening", VisRule.Part, dl.ToArray(), -0.034, 0.034, -0.06, -0.03,
                  "far tongue tip and deck line at y -0.06..-0.03 through the 12 m gate opening (0.067 wide)");
            }
            S(k, "face_B_above", VisRule.Part, FaceB(-105.0, 495.0, 21, 293.0, 400.0, 3), N, N, 0.68, 0.95,
              "face B at y 293 projects to y +0.68 (y 400 to +0.95)", "notch flanks only (agreement 5)").surface = true;
            k = Shot(L, "KS08", "G2", 421, 440);
            S(k, "north_flank_face", VisRule.Part, FaceA(280.0, 1500.0, 21, 0.0, 300.0, 4), -0.62, -0.48, -0.17, 0.57,
              "north flank's east face near x -0.62..-0.48, y -0.17..+0.57 (far x -0.58..-0.50, y -0.22..+0.31)").surface = true;
            k = Shot(L, "KS09", "G2", 498, 534);
            k.composition = VisComposition.Drop; k.specShares = new[] { 0.26, 0.39, 0.36 };
            S(k, "face_A_strata", VisRule.Part, FaceA(300.0, 2500.0, 23, -600.0, 400.0, 6), -0.78, -0.66, -0.03, 0.48, "face A strata x -0.78..-0.66, y -0.03..+0.48").surface = true;
            S(k, "face_B", VisRule.Part, FaceB(300.0, 2500.0, 23, -600.0, 400.0, 6), 0.10, 0.38, 0.16, 0.48, "face B x +0.10..+0.38, y +0.16..+0.48").surface = true;
            S(k, "cross_bridge_850", VisRule.Whole, CrossBridge(850.0, -180.0), -0.63, 0.24, -0.01, 0.13, "cross bridge (z 850, y -180) x -0.63..+0.24, y -0.01..+0.13");
            S(k, "layer_520", VisRule.Whole, Line(new D3(560.0, -520.0, 1200.0), new D3(560.0, -520.0, 2500.0), 27), -0.21, -0.10, -0.24, 0.13,
              "traffic layer (y -520, z 1 200-2 500) x -0.21..-0.10, y -0.24..+0.13");
            k = Shot(L, "KS10", "G2", 534, 563.5);
            k.composition = VisComposition.Drop; k.specShares = new[] { 0.07, 0.50, 0.43 };
            S(k, "cross_bridge_850", VisRule.Whole, CrossBridge(850.0, -180.0), -0.69, 0.22, 0.29, 0.45, "cross bridge (z 850, y -180) x -0.69..+0.22, y +0.29..+0.45");
            S(k, "layer_520", VisRule.Whole, Line(new D3(560.0, -520.0, 1200.0), new D3(560.0, -520.0, 1800.0), 13), -0.19, -0.12, 0.07, 0.31,
              "layer y -520 (x 560, z 1 200-1 800) x -0.19..-0.12, y +0.07..+0.31");
            S(k, "layer_120", VisRule.Part, Line(new D3(350.0, -120.0, 300.0), new D3(350.0, -120.0, 3000.0), 28), -0.34, -0.33, 0.32, 0.70,
              "layer y -120 (x 350) x -0.34..-0.33, y +0.32..+0.70");
            S(k, "face_B", VisRule.Part, FaceB(300.0, 2500.0, 23, -600.0, 400.0, 6), 0.09, 0.09, 0.46, 0.46, "faces at x +0.09 and -0.81 (y +0.46, +0.32)").surface = true;
            S(k, "face_A", VisRule.Part, FaceA(300.0, 2500.0, 23, -600.0, 400.0, 6), -0.81, -0.81, 0.32, 0.32, "faces at x +0.09 and -0.81 (y +0.46, +0.32)").surface = true;

            // ---------------- beat 4 (G3): 14.5.4
            k = Shot(L, "KS11", "G3", 589, 606);
            {
                double xNose = 683.0 - 38.0 * Math.Max(0.0, (600.0 - 584.5) / 2.5);   // T1: launch s 584.5, N tube, west from x 683
                Train(xNose, -1, 2.0, 207.0, out D3[] nose, out D3[] cars);
                S(k, "T1_nose", VisRule.Whole, nose, -0.14, -0.06, -0.12, 0.04, "T1's nose 34 m ahead, 12 m left: x -0.14..-0.06, y -0.12..+0.04 (165 px wide at 4K)",
                  "nose x " + xNose.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture) + " (capture tau = (s - 584.5) / 2.5)");
                S(k, "T1_cars", VisRule.Part, cars, 0.07, 0.15, -0.09, -0.04, "its cars converge to the right at x +0.07..+0.15, y -0.09..-0.04");
            }
            k = Shot(L, "KS12", "G3", 640, 666);
            k.composition = VisComposition.Drop; k.specShares = new[] { 0.22, 0.30, 0.48 };
            S(k, "face_A_far", VisRule.Part, FaceA(400.0, 3000.0, 27, -600.0, 400.0, 6), -0.54, -0.44, -0.09, 0.55, "face A far x -0.54..-0.44, y -0.09..+0.55").surface = true;
            S(k, "face_B_far", VisRule.Part, FaceB(400.0, 3000.0, 27, -600.0, 400.0, 6), 0.38, 0.62, N, N, "face B far x +0.38..+0.62").surface = true;
            S(k, "layer_520", VisRule.Whole, Line(new D3(560.0, -520.0, 1100.0), new D3(560.0, -520.0, 1800.0), 15), 0.01, 0.02, -0.27, 0.05,
              "traffic layer y -520 (z 1 100-1 800) x +0.01..+0.02, y -0.27..+0.05");
            {
                double xNose = 683.0 - 38.0 * Math.Max(0.0, (660.0 - 639.5) / 2.5);   // T3: launch s 639.5, N tube, west
                Train(xNose, -1, 2.0, 207.0, out D3[] nose, out D3[] cars);
                var all = new List<D3>(nose); all.AddRange(cars);
                S(k, "north_tube_T3", VisRule.Info, all.ToArray(), N, N, N, N, "the north tube and T3 (abeam at s 653.6) cross the upper-left foreground");
            }
            k = Shot(L, "KS13", "G3", 712, 730);
            k.composition = VisComposition.Strip;
            S(k, "glass_strip", VisRule.Part, PlaneY(0.0, VisibilitySpecWorld.StripX0, VisibilitySpecWorld.StripX1, 195.0 - VisibilitySpecWorld.StripHalf, 195.0 + VisibilitySpecWorld.StripHalf, 21, 5),
              -0.60, 0.60, -0.14, 0.83, "the glass strip as a trapezoid x +-0.60, y -0.14..+0.83").note = "x 521.3-541.3, 2.4 m wide, y 0 (the strip lies on the deck plane: self-hit tolerance)";
            S(k, "layer_520_below", VisRule.Part, Line(new D3(560.0, -520.0, 100.0), new D3(560.0, -520.0, 300.0), 9), 0.0, 0.0, -0.41, -0.41, "the layer at y -520 (x 0, y -0.41)");
            {
                double xNose = 215.3 + 38.0 * Math.Max(0.0, (716.0 - 695.0) / 2.5);   // T6: launch s 695, lower tube, east from x 215.3
                Train(xNose, 1, -142.0, 209.0, out D3[] nose, out D3[] cars);
                var all = new List<D3>(nose); all.AddRange(cars);
                S(k, "T6_lower_tube", VisRule.Part, all.ToArray(), -0.09, -0.09, -0.55, -0.26, "T6 in the lower tube at x -0.09, y -0.55..-0.26 (visible under the strip)");
            }
            k = Shot(L, "KS14", "G3", 820, 853);
            S(k, "gate_B_frame", VisRule.Whole, PlaneX(683.0, 0.0, 16.0, 189.0, 201.0, 5, 7), -0.26, 0.26, -0.64, 0.42, "gate B frame opening x -0.26..+0.26, y -0.64..+0.42",
              "the opening z 189..201 on the frame's front plane x 683, y 0..16 (it reproduces the spec's x +-0.258, y -0.637..+0.42)");
            S(k, "trees_above", VisRule.Part, Box(700.0, 900.0, 40.0, 130.0, 140.0, 250.0), N, N, N, N, "the glowing trees above");

            // ---------------- beat 5 (G3): 14.5.5
            k = Shot(L, "KS15", "G3", 957, 1011.5);
            S(k, "crown_T3", VisRule.Part, Ellipsoid(new D3(806.0, 0.66 * 88.0, 232.0), 28.0, 0.28 * 88.0, 28.0), N, N, N, N,
              "crown T3 at (806, 232), bottom 33 m up, 37-42 m away", "crown centred at 0.66 h, radii (r, 0.28 h, r)");
            S(k, "crown_T7", VisRule.Part, Ellipsoid(new D3(800.0, 0.66 * 92.0, 160.0), 30.0, 0.28 * 92.0, 30.0), N, N, N, N, "crown T7 at (800, 160)");

            // ---------------- beat 6 (G4): 14.5.6
            k = Shot(L, "KS16", "G4", 1134, 1196.5);
            k.composition = VisComposition.Wall; k.specShares = new[] { 0.59, 0.46 };
            S(k, "wall_p10_300", VisRule.Whole, new[] { new D3(874.0, 10.0, 300.0) }, -0.15, -0.15, -0.62, -0.62, "wall point (y 10, z 300) x -0.15, y -0.62");
            S(k, "wall_p150_350", VisRule.Whole, new[] { new D3(874.0, 150.0, 350.0) }, 0.13, 0.13, 0.80, 0.80, "wall point (y 150, z 350) x +0.13, y +0.80");
            S(k, "wall_p300_420", VisRule.Whole, new[] { new D3(874.0, 300.0, 420.0) }, 0.19, 0.19, 0.93, 0.93, "wall point (y 300, z 420) x +0.19, y +0.93");

            // ---------------- beat 7 (G4; koi maths V): 14.5.7
            k = Shot(L, "KS17", "G4", 1277, 1323);
            k.koi = true; k.koiWhole = true; k.koiSpecMargin = 0.085;
            k.koiSpecX = new[] { -0.50, 0.72, -0.55, 0.80, -0.38, 0.55 }; k.koiSpecY0 = 0.10; k.koiSpecY1 = 0.92;
            k.headX0 = -0.47; k.headX1 = -0.39; k.headY0 = 0.40; k.headY1 = 0.61;
            S(k, "spire", VisRule.Part, Spire(k.ks, 0.0, 560.0, 15), -0.10, -0.08, N, N, "spire x -0.10..-0.08 (276 px at 4K)").surface = true;
            S(k, "goal_window", VisRule.Info, PlaneX(1156.0, 4.4, 5.9, 387.3, 388.5, 4, 3), N, N, N, N, "the goal window is 5.1 x 6.4 px at 1080p (10.2 x 12.7 at 4K) at 212 m");
            k = Shot(L, "KS18", "G4", 1337, 1376.5);
            k.koi = true; k.koiWhole = true; k.koiSpecMargin = 0.178;
            k.koiSpecX = new[] { -0.37, 0.56, -0.41, 0.62, -0.28, 0.43 }; k.koiSpecY0 = -0.54; k.koiSpecY1 = 0.82;
            k.headX0 = -0.24; k.headX1 = -0.11; k.headY0 = 0.44; k.headY1 = 0.76;
            S(k, "spire", VisRule.Info, Spire(k.ks, 0.0, 560.0, 15), N, N, N, N, "20 of 164 sample points pass behind the spire").surface = true;
            k = Shot(L, "KS19", "G4", 1460, 1500);
            k.koi = true; k.koiWhole = false; k.koiBody = true; k.koiSpecMargin = 0.11;
            k.headX0 = -0.66; k.headX1 = -0.52; k.headY0 = 0.54; k.headY1 = 0.89;
            S(k, "goal_window", VisRule.Whole, PlaneX(1156.0, 4.4, 5.9, 387.3, 388.5, 4, 3), -0.15, -0.03, -0.26, 0.02,
              "window x -0.15..-0.03, y -0.26..+0.02 (138 x 172 px at 1080p, 276 x 345 at 4K, 8 m away)").minWidthPx1080 = 6.0;
            // D-029 (GB-01): the table lamp is a named subject of KS19 (14.5.7: a 0.2 m sphere at (1 157.2, 5.0, 387.9), 1.2 m inside)
            S(k, "goal_lamp", VisRule.Whole, Box(HeartLayout.LampX - HeartLayout.LampR, HeartLayout.LampX + HeartLayout.LampR, HeartLayout.LampY - HeartLayout.LampR,
              HeartLayout.LampY + HeartLayout.LampR, HeartLayout.LampZ - HeartLayout.LampR, HeartLayout.LampZ + HeartLayout.LampR), N, N, N, N,
              "the table lamp (0.2 m sphere at (1 157.2, 5.0, 387.9)) seen through the window", "D-029 GB-01: KS19 subjects are the window, the lamp and the koi body");
            S(k, "spire", VisRule.Info, Spire(k.ks, 0.0, 560.0, 15), N, N, N, N, "spire shaft behind the tenement (intended: hidden)").surface = true;

            foreach (var sh in L) foreach (var sub in sh.subjects) sub.self = SelfTags(sub.name);
            L.Sort((a, b) => a.ks.s.CompareTo(b.ks.s));
            return L;
        }

        /// <summary>Renderer-path tags of the built geometry that IS the subject (builder naming of Phase 3: Beat03_edge/FaceA_c*,
        /// CrossBridge_850_-180, TrafficLayer_x_y, Beat02_boulevard/blade*, Beat05_garden/trees/crowns, ...).</summary>
        public static string[] SelfTags(string name)
        {
            if (name.StartsWith("face_A", StringComparison.Ordinal) || name == "north_flank_face") return new[] { "FaceA" };
            if (name.StartsWith("face_B", StringComparison.Ordinal)) return new[] { "FaceB", "Notch" };
            if (name.StartsWith("cross_bridge", StringComparison.Ordinal)) return new[] { "CrossBridge_850" };
            if (name.StartsWith("layer_520", StringComparison.Ordinal)) return new[] { "TrafficLayer_560_-520" };
            if (name.StartsWith("layer_120", StringComparison.Ordinal)) return new[] { "TrafficLayer_350_-120" };
            if (name.StartsWith("blade", StringComparison.Ordinal)) return new[] { "blade" };
            if (name.StartsWith("lane", StringComparison.Ordinal)) return new[] { "lane", "traffic" };
            if (name.Contains("facade") || name == "cloud_lines") return new[] { "facade", "podium", "shaft" };
            if (name == "tram") return new[] { "tram" };
            if (name.Contains("crown") || name == "trees_above") return new[] { "crown", "trees" };
            if (name.StartsWith("T1_", StringComparison.Ordinal) || name.StartsWith("T6_", StringComparison.Ordinal) || name == "north_tube_T3") return new[] { "train" };
            if (name == "spire") return new[] { "spire" };
            if (name == "gate_B_frame") return new[] { "gateB" };
            if (name.StartsWith("wall_p", StringComparison.Ordinal)) return new[] { "screen" };
            if (name == "goal_window") return new[] { "tenement", "goalWindow", "darkWindows" };
            if (name == "goal_lamp") return new[] { "lamp", "goalLamp", "goalWindow", "curtain" };
            if (name == "catwalk") return new[] { "catwalk" };
            if (name.StartsWith("bar_", StringComparison.Ordinal) || name == "counter") return new[] { "bar", "noodle", "counter", "sign" };
            if (name.StartsWith("roof_edge", StringComparison.Ordinal)) return new[] { "alleyWall", "podium" };
            if (name == "tower_flank") return new[] { "tower" };
            if (name == "glass_strip") return new[] { "strip", "glass" };
            if (name == "canyon_end" || name.StartsWith("far_deck", StringComparison.Ordinal)) return new[] { "deck", "Tongue", "street", "floor" };
            return Array.Empty<string>();
        }

        /// <summary>A cross-gulf bridge (14.5.3: 14 m truss and deck) at (z, y) spanning face A to face B: points along both chords.</summary>
        public static D3[] CrossBridge(double z, double y)
        {
            double xa = CityField.GulfXA(z), xb = CityField.GulfXB(z);
            var l = new List<D3>();
            for (int i = 0; i <= 20; i++)
            {
                double x = xa + (xb - xa) * i / 20.0;
                l.Add(new D3(x, y, z - 7.0)); l.Add(new D3(x, y, z + 7.0)); l.Add(new D3(x, y - 14.0, z));
            }
            return l.ToArray();
        }
    }
}
