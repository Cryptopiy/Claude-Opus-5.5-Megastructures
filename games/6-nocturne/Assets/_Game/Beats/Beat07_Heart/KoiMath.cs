// KoiMath: the pure mathematics of the 900 m hologram koi of beat 7 (MASTER_PROMPT 14.5.7 "The koi", 14.7 H6, 14.12 "Koi",
// DEVELOPMENT D-009, D-026). Double precision, no state, no allocation; UnityEngine only for the Vector3 conveniences at the end.
// G4's KoiBuilder / koi driver build and move the mesh from these functions; the EditMode tests (KoiMathTests: T-KOI) and the
// 11.13 visibility audit (VisibilityAudit: KS17-KS19 composition lint) sample the same functions, so what is tested is what ships.
//
// Coordinates: Unity world, +X east, +Z north, Y up. The orbit centre is the spire axis S = (1 330, 388) (14.4). The azimuth phi
// is measured from +X toward +Z (counter-clockwise seen from above), in RADIANS inside this class; the head azimuth table of
// 14.5.7 is in degrees (HeadAzimuthDeg).
//
//   orbit           R(phi) = 260 + 18 sin(2 phi + 0.7),  y(phi) = 300 + 60 sin(phi + 0.4)
//   arc integral    A(phi) = integral R dphi = 260 phi - 9 cos(2 phi + 0.7)            (closed form, exact)
//   head            phi_h = 6.64 + 422.4 u degrees,  u = (s - 1 260) / 240 + E              (14.5.7, 14.7 H6)
//   body (D-032 2, HT 2026-11-16: the koi's outline; was "the body trails the head on the same orbit")
//                   a = arc length behind the head along the REFERENCE SPINE, a plane curve of 900.0 m in the head's co-rotating
//                   frame (orbit circle R0 260, untilted): it starts at the head on the orbit, its tangent turned CrabDeg inward
//                   from the orbit tangent, with the curvature k(a) = KFront + (KRear - KFront) smoothstep(KA0, KA1, a)
//                   - KFlick smoothstep(KFlickA0, L, a): a nearly straight head and trunk (a carangiform swimmer bends behind its
//                   shoulders), the bend in the rear half, the tail flicking back out (an S). Its polar coordinates about S,
//                   r_ref(a) and alpha(a) (clockwise azimuth behind the head), come from an RK4 integration at load (tables every
//                   0.5 m, cubic Hermite with exact derivatives). The world spine: phi = phi_h - alpha(a),
//                   r = r_ref(a) + (R(phi) - 260) m(a) + w(a, t_w), y = y(phi) + Dy(a), m(a) = 1 - smoothstep(0, 110, a) (the orbit's
//                   +-18 m ellipticity is kept at the head and fades behind it), Dy the PCHIP through DropX / DropY with zero end
//                   slopes (a gentle vertical S: the trunk dips 8 m, rises 4 m, the tail sinks 20 m; level at the head and at the
//                   veil, so the profile is C1 where it is held constant beyond the knots; the KS19 body rule of D-029 bounds the
//                   tail's drop: with -40 m at a 600 the body sank behind the tenement, 0 core samples visible). L = 900.0 m is the length of the reference spine (the old definition, integral of R dphi over
//                   the unwaved orbit, is the same measure for the orbit circle); the head stays on the orbit at every s (a = 0:
//                   alpha 0, r_ref 260, m 1, Dy 0) and keeps the azimuth table.
//   radius          rho(a) = 48 f(a / L), f the monotone cubic (Fritsch-Carlson PCHIP) through 0 -> 0.22, 0.06 -> 0.60,
//                   0.18 -> 0.92, 0.30 -> 1.00, 0.50 -> 0.90, 0.80 -> 0.45, 1.0 -> 0.20 (no overshoot: the maximum is the 0.30
//                   knot, width 2 x 48 = 96.0 m)
//   ring frame      oriented along the WAVED spine (D-009): F = -dC/da (toward the head), N = normalize(Y x F) (horizontal,
//                   outward from S), B = F x N (up-ish); ring point C + rho (cos theta N + sin theta B), theta 0 out, 90 top,
//                   180 in, 270 bottom
//   tail veil       D-009 / HT: 140 m long x 140 m span (half-span 70 m at the lobe tips), its centre line on the spine for a in
//                   [L - 30, L + 110] (it overlaps the last 30 m of the tail; the wave amplitude is held at its tail value beyond
//                   L), half-span lerp(rho(L - 30), 70, v) along the veil axis cos(30 deg) Y + sin(30 deg) N (rolled 30 deg from
//                   the vertical, so the forked fan reads from below; D-009's vertical veil was edge-on in KS18), split: the trailing
//                   edge runs from the lobe tips (a = L + 110) to the notch (2/3 of the length)
//   fins            frames for sampling and for the graybox (14.5.7: pectorals 120 x 60 m, dorsal 90 x 40 m; cut 7 of 14.8):
//                   pectoral root chord 60 m along the spine from a = 108 (0.12 L) on the low flank (PectoralRootTheta below the
//                   side axis on each side), extending 120 m along d = normalize(PectoralDirF F +- PectoralDirN N + PectoralDirB B)
//                   (swept back, splayed out and down so the pair reads from below; every koi part stays inside horizontal
//                   radius 356 m about S and 100 m from the spire surface, which bounds the splay);
//                   dorsal root along the top (theta 90) from a = 216 (0.24 L) for 90 m, height 16 -> 40 (peak at 25 %) -> 12 m
//   wave time       t_w = t_k in play (t_k = seconds since s first reached 1 260), (s - 1 260) / v in capture mode with
//                   v = 1.5 m/s (14.7 H6; D-026: the 12 s koi sequences at 2.5 m/s cover 20 s of wave time); the composition
//                   checks use the 20 phases t_w = k s, k = 0..19 (one 20 s period)
//   phase follower  E = critically damped follower (omega 0.8 rad/s) of max(0, t_k / 160 - (s - 1 260) / 240), u held
//                   non-decreasing (StepPhase); at any constant speed E = 0 and the head is exactly on the 14.5.7 table
//
// Composition sample set (documented here, used by the visibility audit and the tests; SampleCount = 183):
//   0..163    core body: 41 stations a_i = 22.5 i m (i = 0..40, head to tail tip) x 4 ring points j = 0..3 (theta 0, 90, 180,
//             270 deg: out, top, in, bottom); index 4 i + j. This is the spec's count of 164 koi sample points (14.5.7 KS17:
//             "28 (the last 140 m of the tail)" = stations 34..40 x 4). Head = stations 0 and 1 (indices 0..7, a <= 22.5 m).
//   164..171  pectoral fins: outer fin then inner fin, each root leading, root trailing, tip trailing, tip leading
//   172..175  dorsal fin: root front, peak (25 %), trailing top, root back
//   176..182  tail veil: root top, root bottom, mid top (v 0.5), mid bottom, upper lobe tip, lower lobe tip, notch
//   Each sample carries its kind and the arc length of its body root (D-009: at KS17 a fin sample whose body root is hidden by
//   the arcade is exempt from the frame margin).
using System;
using UnityEngine;

namespace Megastructure
{
    /// <summary>Kind of a composition sample point (KoiMath sample set).</summary>
    public enum KoiPart { Body = 0, Head = 1, Pectoral = 2, Dorsal = 3, Veil = 4 }

    /// <summary>State of the 14.7 H6 phase follower (one per koi driver; value type, allocation-free).</summary>
    public struct KoiPhaseState
    {
        /// <summary>Koi clock t_k (s): seconds since s first reached 1 260 (0 before).</summary>
        public double Tk;
        /// <summary>Excess E and its velocity (critically damped, omega 0.8 rad/s).</summary>
        public double E, EVel;
        /// <summary>Current phase u (held non-decreasing) and the progress of the previous step.</summary>
        public double U, SPrev;
        public bool Started, Initialised;
    }

    public static class KoiMath
    {
        // ------------------------------------------------------------------ constants (14.5.7, 14.12 "Koi", D-009)
        /// <summary>Orbit centre S = the spire axis (14.4, 14.12 "Heart").</summary>
        public const double CentreX = 1330.0, CentreZ = 388.0;
        /// <summary>R(phi) = 260 + 18 sin(2 phi + 0.7) (14.5.7).</summary>
        public const double OrbitR0 = 260.0, OrbitRAmp = 18.0, OrbitRPhase = 0.7;
        /// <summary>y(phi) = 300 + 60 sin(phi + 0.4) (14.5.7).</summary>
        public const double OrbitY0 = 300.0, OrbitYAmp = 60.0, OrbitYPhase = 0.4;
        /// <summary>Body length L = integral of R dphi over the unwaved orbit (14.5.7, T-KOI 900.0 +-0.5).</summary>
        public const double Length = 900.0;
        /// <summary>Radius scale of the profile f (m) and the maximum width 2 x 48 (14.5.7).</summary>
        public const double RadiusScale = 48.0, MaxWidth = 96.0;
        /// <summary>Radial wave w = 40 (0.15 + 0.85 (a/L)^2) sin(2 pi (a/810 - t_w/20)) (14.5.7; D-009 keeps it).</summary>
        public const double WaveAmp = 40.0, WaveBase = 0.15, WaveTail = 0.85, WaveLength = 810.0, WavePeriod = 20.0;
        /// <summary>Head azimuth phi_h = 6.64 + 422.4 u degrees (= 6.64 + 1.76 (s - 1 260) at constant speed, 14.12).</summary>
        public const double HeadDeg0 = 6.64, HeadDegSpan = 422.4;
        /// <summary>Beat 7 start s, phase distance D 240 m, phase time T 160 s, follower omega 0.8 rad/s (14.7 H6, 14.12).</summary>
        public const double BeatS = 1260.0, PhaseD = 240.0, PhaseT = 160.0, FollowerOmega = 0.8;
        /// <summary>Capture-mode wave time speed: t_w = (s - 1 260) / 1.5 (14.7 H6, D-026).</summary>
        public const double CaptureSpeed = 1.5;
        /// <summary>Mesh resolution of 14.5.7 (160 segments x 28 ring vertices, 9.0 k triangles), for the builder.</summary>
        public const int BodySegments = 160, RingVertices = 28;
        /// <summary>Tail veil (D-009, HT): length, half-span at the trailing edge, notch position behind the veil root (m), roll of
        /// its axis from the vertical toward the side axis N (deg).</summary>
        public const double VeilLength = 140.0, VeilHalfSpan = 70.0, VeilNotch = 140.0 * 2.0 / 3.0, VeilRollDeg = 30.0;
        /// <summary>Arc length of the veil's root (m): the veil spans a = VeilA0 .. VeilA0 + 120 (VeilA(v)).</summary>
        public const double VeilA0 = Length - VeilOverlap;
        public const double VeilOverlap = 30.0;
        /// <summary>Pectoral fins (14.5.7 120 x 60 m): root start (0.12 L), root chord, length, direction in (F, N, B).</summary>
        public const double PectoralA0 = 108.0, PectoralChord = 60.0, PectoralLength = 120.0;
        public const double PectoralRootTheta = 50.0;                                  // deg below the side axis (low flank)
        public const double PectoralDirF = -0.55, PectoralDirN = 0.50, PectoralDirB = -0.60;
        /// <summary>Dorsal fin (14.5.7 90 x 40 m): root start (0.24 L), length, heights at the root front, peak, trailing end.</summary>
        public const double DorsalA0 = 216.0, DorsalLength = 90.0, DorsalH0 = 16.0, DorsalHPeak = 40.0, DorsalHEnd = 12.0, DorsalPeakAt = 0.25;
        /// <summary>The 14.5.7 head azimuth table (s, degrees).</summary>
        public static readonly double[] TableS = { 1260.0, 1296.0, 1350.0, 1400.0, 1460.0, 1500.0 };
        public static readonly double[] TableDeg = { 6.6, 70.0, 165.0, 253.0, 358.6, 429.0 };

        // ------------------------------------------------------------------ the body's shape (D-032 2, HT; header "body")
        /// <summary>Curvature of the reference spine (1/m): KFront to KA0, smoothstep to KRear by KA1, minus KFlick over KFlickA0..L
        /// (the orbit circle has 1 / 260 = 0.003846 everywhere).</summary>
        public const double KFront = 0.00185, KRear = 0.00420, KA0 = 200.7, KA1 = 276.1, KFlick = 0.00167, KFlickA0 = 853.4;
        /// <summary>The reference spine leaves the head turned this far inward from the orbit tangent (deg).</summary>
        public const double CrabDeg = 19.27;
        /// <summary>The orbit's R(phi) - 260 fades from the head (a = 0) to a = RModFade (m).</summary>
        public const double RModFade = 110.0;
        /// <summary>Vertical drop Dy(a) (m): PCHIP through these knots (0 at the head).</summary>
        public static readonly double[] DropX = { 0.0, 300.0, 600.0, 900.0 };
        public static readonly double[] DropY = { 0.0, -8.0, 4.0, -20.0 };
        /// <summary>Reference-spine tables: node spacing (m) and the last tabulated a (veil and tail cap included).</summary>
        public const double ShapeStep = 0.5, ShapeAMax = 1100.0;

        // composition sample set
        public const int Stations = 41, RingSamples = 4, CoreCount = Stations * RingSamples;   // 164
        public const double StationStep = Length / (Stations - 1);                            // 22.5 m
        public const int HeadStations = 2, HeadCount = HeadStations * RingSamples;            // indices 0..7
        public const int PectoralBase = CoreCount, DorsalBase = PectoralBase + 8, VeilBase = DorsalBase + 4;
        public const int SampleCount = VeilBase + 7;                                          // 183
        /// <summary>Wave phases of the composition checks: t_w = k x (20 / 20) s, k = 0..19.</summary>
        public const int WavePhases = 20;

        const double Tau = 2.0 * Math.PI;
        const double DerivStep = 0.05;   // m, central difference of the waved spine

        // ------------------------------------------------------------------ orbit
        /// <summary>Orbit radius R(phi) (m), phi in radians.</summary>
        public static double OrbitRadius(double phi) => OrbitR0 + OrbitRAmp * Math.Sin(2.0 * phi + OrbitRPhase);
        /// <summary>Orbit altitude y(phi) (m).</summary>
        public static double OrbitY(double phi) => OrbitY0 + OrbitYAmp * Math.Sin(phi + OrbitYPhase);
        /// <summary>Antiderivative A(phi) = 260 phi - 9 cos(2 phi + 0.7) of R (exact).</summary>
        public static double ArcIntegral(double phi) => OrbitR0 * phi - 0.5 * OrbitRAmp * Math.Cos(2.0 * phi + OrbitRPhase);

        /// <summary>Phase u at constant speed (E = 0): (s - 1 260) / 240.</summary>
        public static double PhaseU(double s) => (s - BeatS) / PhaseD;
        /// <summary>Head azimuth in degrees: 6.64 + 422.4 u.</summary>
        public static double HeadAzimuthDeg(double u) => HeadDeg0 + HeadDegSpan * u;
        /// <summary>Head azimuth in radians.</summary>
        public static double HeadAzimuth(double u) => HeadAzimuthDeg(u) * (Math.PI / 180.0);

        /// <summary>Azimuth phi(a) of the body point a metres behind the head along the reference spine: phi_h - alpha(a) (D-032 2).</summary>
        public static double AzimuthAt(double a, double phiHead) => phiHead - RefAngle(a);

        /// <summary>Azimuth of the orbit point a metres of orbit arc behind the head: A(phi_h) - A(phi) = a (Newton; the pre-D-032
        /// body parameterisation, kept for comparison).</summary>
        public static double OrbitAzimuthAt(double a, double phiHead)
        {
            double target = ArcIntegral(phiHead) - a;
            double phi = phiHead - a / OrbitR0;
            for (int it = 0; it < 8; it++)
            {
                double f = ArcIntegral(phi) - target;
                double step = f / OrbitRadius(phi);
                phi -= step;
                if (Math.Abs(step) < 1e-13) break;
            }
            return phi;
        }

        // ------------------------------------------------------------------ wave, profile
        /// <summary>Amplitude envelope of the radial wave at a (m); held at its tail value beyond L (the veil).</summary>
        public static double WaveEnvelope(double a)
        {
            double r = Math.Min(Math.Max(a, 0.0), Length) / Length;
            return WaveAmp * (WaveBase + WaveTail * r * r);
        }

        /// <summary>Radial wave w(a, t_w) (m): 40 (0.15 + 0.85 (a/L)^2) sin(2 pi (a/810 - t_w/20)).</summary>
        public static double Wave(double a, double tw) => WaveEnvelope(a) * Math.Sin(Tau * (a / WaveLength - tw / WavePeriod));

        static readonly double[] s_kx = { 0.0, 0.06, 0.18, 0.30, 0.50, 0.80, 1.0 };
        static readonly double[] s_ky = { 0.22, 0.60, 0.92, 1.00, 0.90, 0.45, 0.20 };
        static readonly double[] s_km = Pchip(s_kx, s_ky);
        static readonly double[] s_dm = PchipClamped(DropX, DropY);
        static readonly double[] s_rr, s_rrd, s_al, s_ald;   // r_ref, dr_ref/da, alpha, dalpha/da at a = i ShapeStep

        static KoiMath()
        {
            BuildReferenceSpine(out s_rr, out s_rrd, out s_al, out s_ald);
        }

        /// <summary>PCHIP slopes with zero slopes at both ends (still monotone between knots; a C1 join to a held value).</summary>
        static double[] PchipClamped(double[] x, double[] y)
        {
            double[] m = Pchip(x, y);
            m[0] = 0.0; m[m.Length - 1] = 0.0;
            return m;
        }

        /// <summary>Fritsch-Carlson / Fritsch-Butland monotone cubic slopes (the PCHIP of scipy): no overshoot between knots.</summary>
        static double[] Pchip(double[] x, double[] y)
        {
            int n = x.Length;
            var h = new double[n - 1];
            var d = new double[n - 1];
            for (int k = 0; k < n - 1; k++) { h[k] = x[k + 1] - x[k]; d[k] = (y[k + 1] - y[k]) / h[k]; }
            var m = new double[n];
            for (int k = 1; k < n - 1; k++)
            {
                if (d[k - 1] * d[k] <= 0.0) { m[k] = 0.0; continue; }
                double w1 = 2.0 * h[k] + h[k - 1], w2 = h[k] + 2.0 * h[k - 1];
                m[k] = (w1 + w2) / (w1 / d[k - 1] + w2 / d[k]);
            }
            m[0] = PchipEnd(h[0], h[1], d[0], d[1]);
            m[n - 1] = PchipEnd(h[n - 2], h[n - 3], d[n - 2], d[n - 3]);
            return m;
        }

        static double PchipEnd(double h0, double h1, double d0, double d1)
        {
            double m = ((2.0 * h0 + h1) * d0 - h0 * d1) / (h0 + h1);
            if (Math.Sign(m) != Math.Sign(d0)) return 0.0;
            if (Math.Sign(d0) != Math.Sign(d1) && Math.Abs(m) > 3.0 * Math.Abs(d0)) return 3.0 * d0;
            return m;
        }

        /// <summary>Radius profile f(x), x = a / L in [0, 1] (clamped): the monotone cubic through the 14.5.7 knots.</summary>
        public static double Profile(double x)
        {
            if (x <= 0.0) return s_ky[0];
            if (x >= 1.0) return s_ky[s_ky.Length - 1];
            int k = 0;
            while (k < s_kx.Length - 2 && x > s_kx[k + 1]) k++;
            double h = s_kx[k + 1] - s_kx[k], t = (x - s_kx[k]) / h;
            double t2 = t * t, t3 = t2 * t;
            return (2 * t3 - 3 * t2 + 1) * s_ky[k] + (t3 - 2 * t2 + t) * h * s_km[k] + (-2 * t3 + 3 * t2) * s_ky[k + 1] + (t3 - t2) * h * s_km[k + 1];
        }

        /// <summary>Body radius rho(a) = 48 f(a / L) (m).</summary>
        public static double Radius(double a) => RadiusScale * Profile(a / Length);

        // ------------------------------------------------------------------ reference spine (D-032 2, HT)
        /// <summary>Curvature k(a) of the reference spine (1/m).</summary>
        public static double Curvature(double a) =>
            KFront + (KRear - KFront) * NocMath.Smoothstep(KA0, KA1, a) - KFlick * NocMath.Smoothstep(KFlickA0, Length, a);

        /// <summary>RK4 integration of the reference spine in the head's frame (head at (260, 0), forward +z, the body leaving along
        /// -z turned CrabDeg toward S), tabulated as polar coordinates about S: r_ref, alpha (clockwise, unwrapped) and their
        /// derivatives. Load time only.</summary>
        static void BuildReferenceSpine(out double[] rr, out double[] rrd, out double[] al, out double[] ald)
        {
            int n = (int)Math.Round(ShapeAMax / ShapeStep) + 1;
            rr = new double[n]; rrd = new double[n]; al = new double[n]; ald = new double[n];
            double h = ShapeStep, th = -0.5 * Math.PI - CrabDeg * (Math.PI / 180.0), x = OrbitR0, z = 0.0, prevAng = 0.0, turns = 0.0;
            for (int i = 0; i < n; i++)
            {
                double a = i * h;
                double tx = Math.Cos(th), tz = Math.Sin(th), r2 = x * x + z * z, r = Math.Sqrt(r2);
                double ang = -Math.Atan2(z, x);
                if (i > 0)
                {
                    double d = ang - prevAng;
                    if (d < -Math.PI) turns += 2.0 * Math.PI; else if (d > Math.PI) turns -= 2.0 * Math.PI;
                }
                prevAng = ang;
                rr[i] = r; rrd[i] = (x * tx + z * tz) / r;
                al[i] = ang + turns; ald[i] = -(x * tz - z * tx) / r2;
                if (i == n - 1) break;
                // RK4 on (th, x, z): th' = -k(a), x' = cos th, z' = sin th
                double k1 = -Curvature(a), k2 = -Curvature(a + 0.5 * h), k4 = -Curvature(a + h);
                double t1 = th, t2 = th + 0.5 * h * k1, t3 = th + 0.5 * h * k2, t4 = th + h * k2;
                x += h / 6.0 * (Math.Cos(t1) + 2.0 * Math.Cos(t2) + 2.0 * Math.Cos(t3) + Math.Cos(t4));
                z += h / 6.0 * (Math.Sin(t1) + 2.0 * Math.Sin(t2) + 2.0 * Math.Sin(t3) + Math.Sin(t4));
                th += h / 6.0 * (k1 + 4.0 * k2 + k4);
            }
        }

        static double Hermite(double[] v, double[] d, double a, out double deriv)
        {
            double u = a / ShapeStep;
            int n = v.Length;
            if (u <= 0.0) { deriv = d[0]; return v[0] + d[0] * a; }
            if (u >= n - 1) { deriv = d[n - 1]; return v[n - 1] + d[n - 1] * (a - (n - 1) * ShapeStep); }
            int i = (int)u;
            double t = u - i, h = ShapeStep, t2 = t * t, t3 = t2 * t;
            deriv = ((6 * t2 - 6 * t) * v[i] + (3 * t2 - 4 * t + 1) * h * d[i] + (-6 * t2 + 6 * t) * v[i + 1] + (3 * t2 - 2 * t) * h * d[i + 1]) / h;
            return (2 * t3 - 3 * t2 + 1) * v[i] + (t3 - 2 * t2 + t) * h * d[i] + (-2 * t3 + 3 * t2) * v[i + 1] + (t3 - t2) * h * d[i + 1];
        }

        /// <summary>Radius r_ref(a) of the reference spine about S (m) and its derivative along a.</summary>
        public static double RefRadius(double a, out double drda) => Hermite(s_rr, s_rrd, a, out drda);
        public static double RefRadius(double a) => Hermite(s_rr, s_rrd, a, out _);
        /// <summary>Clockwise azimuth alpha(a) of the reference spine behind the head (rad) and its derivative along a.</summary>
        public static double RefAngle(double a, out double dada) => Hermite(s_al, s_ald, a, out dada);
        public static double RefAngle(double a) => Hermite(s_al, s_ald, a, out _);

        /// <summary>Vertical drop Dy(a) (m; PCHIP through DropX / DropY with zero end slopes, held beyond the end knots, C1) and its derivative.</summary>
        public static double Drop(double a, out double dyda)
        {
            int n = DropX.Length;
            if (a <= DropX[0]) { dyda = 0.0; return DropY[0]; }
            if (a >= DropX[n - 1]) { dyda = 0.0; return DropY[n - 1]; }
            int k = 0;
            while (k < n - 2 && a > DropX[k + 1]) k++;
            double h = DropX[k + 1] - DropX[k], t = (a - DropX[k]) / h, t2 = t * t, t3 = t2 * t;
            dyda = ((6 * t2 - 6 * t) * DropY[k] + (3 * t2 - 4 * t + 1) * h * s_dm[k] + (-6 * t2 + 6 * t) * DropY[k + 1] + (3 * t2 - 2 * t) * h * s_dm[k + 1]) / h;
            return (2 * t3 - 3 * t2 + 1) * DropY[k] + (t3 - 2 * t2 + t) * h * s_dm[k] + (-2 * t3 + 3 * t2) * DropY[k + 1] + (t3 - t2) * h * s_dm[k + 1];
        }
        public static double Drop(double a) => Drop(a, out _);

        /// <summary>Weight m(a) of the orbit's ellipticity R(phi) - 260 along the body: 1 at the head, 0 from RModFade.</summary>
        public static double RModWeight(double a) => 1.0 - NocMath.Smoothstep(0.0, RModFade, a);

        // ------------------------------------------------------------------ spine and frames
        /// <summary>Waved spine point C(a) (world metres) for the head azimuth phiHead (rad) and wave time tw (s).</summary>
        public static D3 SpinePoint(double a, double phiHead, double tw)
        {
            double phi = AzimuthAt(a, phiHead);
            double r = RefRadius(a) + (OrbitRadius(phi) - OrbitR0) * RModWeight(a) + Wave(a, tw);
            return new D3(CentreX + r * Math.Cos(phi), OrbitY(phi) + Drop(a), CentreZ + r * Math.Sin(phi));
        }

        /// <summary>Unwaved orbit point at azimuth phi (rad).</summary>
        public static D3 OrbitPoint(double phi)
        {
            double r = OrbitRadius(phi);
            return new D3(CentreX + r * Math.Cos(phi), OrbitY(phi), CentreZ + r * Math.Sin(phi));
        }

        /// <summary>Ring frame at a, oriented along the waved spine (D-009): centre C, forward F (toward the head), side N
        /// (horizontal, outward from S), up-ish B = F x N.</summary>
        public static void Frame(double a, double phiHead, double tw, out D3 c, out D3 f, out D3 n, out D3 b)
        {
            c = SpinePoint(a, phiHead, tw);
            D3 ahead = SpinePoint(a - DerivStep, phiHead, tw), behind = SpinePoint(a + DerivStep, phiHead, tw);
            f = (ahead - behind).Normalized;
            n = D3.Cross(new D3(0, 1, 0), f).Normalized;
            b = D3.Cross(f, n);
        }

        /// <summary>Body surface point at a and ring angle theta (rad: 0 out, pi/2 top, pi in, 3 pi/2 bottom).</summary>
        public static D3 RingPoint(double a, double theta, double phiHead, double tw)
        {
            Frame(a, phiHead, tw, out D3 c, out _, out D3 n, out D3 b);
            double rho = Radius(a);
            return c + n * (rho * Math.Cos(theta)) + b * (rho * Math.Sin(theta));
        }

        /// <summary>Tail veil point (D-009, HT): v in [0, 1] from the root (a = VeilA0 = L - 30) to the trailing edge
        /// (a = VeilA0 + 140), k in [-1, 1] the fraction of the local half-span lerp(rho(VeilA0), 70, v) along VeilAxis (+1 up).</summary>
        public static D3 VeilPoint(double v, double k, double phiHead, double tw)
        {
            double a = VeilA(v);
            Frame(a, phiHead, tw, out D3 c, out _, out D3 n, out _);
            double half = NocMath.Lerp(Radius(VeilA0), VeilHalfSpan, NocMath.Clamp01(v));
            return c + VeilAxis(n) * (k * half);
        }

        /// <summary>The veil's span axis at a station with side axis n: cos(roll) Y + sin(roll) n.</summary>
        public static D3 VeilAxis(D3 n)
        {
            double rl = VeilRollDeg * (Math.PI / 180.0);
            return new D3(0.0, Math.Cos(rl), 0.0) + n * Math.Sin(rl);
        }

        /// <summary>Arc length of veil station v in [0, 1] (root VeilA0, trailing edge VeilA0 + VeilLength).</summary>
        public static double VeilA(double v) => VeilA0 + VeilLength * v;

        /// <summary>Pectoral fin point: side +1 outer, -1 inner fin; u in [0, 1] along the root chord (front to back), v in [0, 1]
        /// from the root to the tip (120 m along d).</summary>
        public static D3 PectoralPoint(int side, double u, double v, double phiHead, double tw)
        {
            double a = PectoralA0 + PectoralChord * NocMath.Clamp01(u);
            Frame(a, phiHead, tw, out D3 c, out D3 f, out D3 n, out D3 b);
            double rho = Radius(a), th = PectoralRootTheta * (Math.PI / 180.0);
            double sg = side >= 0 ? 1.0 : -1.0;
            D3 root = c + n * (sg * rho * Math.Cos(th)) - b * (rho * Math.Sin(th));
            D3 d = (f * PectoralDirF + n * (sg * PectoralDirN) + b * PectoralDirB).Normalized;
            return root + d * (PectoralLength * v);
        }

        /// <summary>Dorsal fin height profile (m) at u in [0, 1] along its root: 16 -> 40 (u 0.25) -> 12.</summary>
        public static double DorsalHeight(double u)
        {
            u = NocMath.Clamp01(u);
            return u <= DorsalPeakAt ? NocMath.Lerp(DorsalH0, DorsalHPeak, u / DorsalPeakAt)
                                     : NocMath.Lerp(DorsalHPeak, DorsalHEnd, (u - DorsalPeakAt) / (1.0 - DorsalPeakAt));
        }

        /// <summary>Dorsal fin point: u along the root (front to back), v in [0, 1] from the body top to the fin edge.</summary>
        public static D3 DorsalPoint(double u, double v, double phiHead, double tw)
        {
            double a = DorsalA0 + DorsalLength * NocMath.Clamp01(u);
            Frame(a, phiHead, tw, out D3 c, out _, out _, out D3 b);
            return c + b * (Radius(a) + DorsalHeight(u) * NocMath.Clamp01(v));
        }

        // ------------------------------------------------------------------ composition sample set
        /// <summary>Kind of sample i (0..SampleCount-1).</summary>
        public static KoiPart SampleKind(int i)
        {
            if (i < HeadCount) return KoiPart.Head;
            if (i < CoreCount) return KoiPart.Body;
            if (i < DorsalBase) return KoiPart.Pectoral;
            if (i < VeilBase) return KoiPart.Dorsal;
            return KoiPart.Veil;
        }

        /// <summary>True for the 164 core body samples (head included).</summary>
        public static bool IsCore(int i) => i >= 0 && i < CoreCount;

        /// <summary>Arc length of the body root of sample i (m): its station, the fin's root chord middle, or the tail tip for the veil.</summary>
        public static double SampleRootA(int i)
        {
            if (i < CoreCount) return (i / RingSamples) * StationStep;
            if (i < DorsalBase) return PectoralA0 + 0.5 * PectoralChord;
            if (i < VeilBase) return DorsalA0 + 0.5 * DorsalLength;
            return VeilA0;
        }

        /// <summary>Fills the 183 composition samples for head azimuth phiHead (rad) and wave time tw (buffer >= SampleCount).</summary>
        public static void Samples(double phiHead, double tw, D3[] buffer)
        {
            for (int i = 0; i < Stations; i++)
            {
                double a = i * StationStep;
                Frame(a, phiHead, tw, out D3 c, out _, out D3 n, out D3 b);
                double rho = Radius(a);
                int o = i * RingSamples;
                buffer[o + 0] = c + n * rho;
                buffer[o + 1] = c + b * rho;
                buffer[o + 2] = c - n * rho;
                buffer[o + 3] = c - b * rho;
            }
            int p = PectoralBase;
            for (int side = 1; side >= -1; side -= 2)
            {
                buffer[p++] = PectoralPoint(side, 0.0, 0.0, phiHead, tw);
                buffer[p++] = PectoralPoint(side, 1.0, 0.0, phiHead, tw);
                buffer[p++] = PectoralPoint(side, 1.0, 1.0, phiHead, tw);
                buffer[p++] = PectoralPoint(side, 0.0, 1.0, phiHead, tw);
            }
            buffer[p++] = DorsalPoint(0.0, 0.0, phiHead, tw);
            buffer[p++] = DorsalPoint(DorsalPeakAt, 1.0, phiHead, tw);
            buffer[p++] = DorsalPoint(1.0, 1.0, phiHead, tw);
            buffer[p++] = DorsalPoint(1.0, 0.0, phiHead, tw);
            buffer[p++] = VeilPoint(0.0, 1.0, phiHead, tw);
            buffer[p++] = VeilPoint(0.0, -1.0, phiHead, tw);
            buffer[p++] = VeilPoint(0.5, 1.0, phiHead, tw);
            buffer[p++] = VeilPoint(0.5, -1.0, phiHead, tw);
            buffer[p++] = VeilPoint(1.0, 1.0, phiHead, tw);
            buffer[p++] = VeilPoint(1.0, -1.0, phiHead, tw);
            double vn = VeilNotch / VeilLength;
            buffer[p++] = VeilPoint(vn, 0.0, phiHead, tw);
        }

        /// <summary>The 12 analytic koi light points of 14.2.3 / H4 (spine at a = L (k + 0.5) / 12).</summary>
        public static void SpineLightPoints(double phiHead, double tw, D3[] buffer)
        {
            int n = Math.Min(buffer.Length, NocturneConstants.KoiSpinePoints);
            for (int k = 0; k < n; k++) buffer[k] = SpinePoint(Length * (k + 0.5) / NocturneConstants.KoiSpinePoints, phiHead, tw);
        }

        // ------------------------------------------------------------------ time and phase (14.7 H6)
        /// <summary>Wave time in play: t_w = t_k.</summary>
        public static double WaveTimePlay(double tk) => tk;
        /// <summary>Wave time in capture mode: (s - 1 260) / v, v = 1.5 m/s by default (14.7 H6, D-026).</summary>
        public static double WaveTimeCapture(double s, double speed = CaptureSpeed) => (s - BeatS) / Math.Max(1e-6, speed);
        /// <summary>Wave time of composition phase k (0..19): k x 20 / 20 s.</summary>
        public static double PhaseWaveTime(int k) => k * (WavePeriod / WavePhases);

        /// <summary>Resets the follower (start of a walk).</summary>
        public static void ResetPhase(ref KoiPhaseState st) { st = default; }

        /// <summary>One frame of the 14.7 H6 phase: advances t_k (from the first crossing of s 1 260, the crossing frame
        /// pro rata), steps the critically damped excess E (omega 0.8 rad/s) toward max(0, t_k / 160 - (s - 1 260) / 240), and
        /// returns u = (s - 1 260) / 240 + E held non-decreasing.</summary>
        public static double StepPhase(ref KoiPhaseState st, double s, double dt)
        {
            if (dt < 0.0) dt = 0.0;
            if (!st.Initialised) { st.SPrev = s; st.Initialised = true; st.U = double.NegativeInfinity; }
            if (st.Started) st.Tk += dt;
            else if (s >= BeatS)
            {
                st.Started = true;
                double ds = s - st.SPrev;
                double frac = ds > 1e-12 ? NocMath.Clamp01((s - BeatS) / ds) : 1.0;
                st.Tk = dt * frac;
            }
            double baseU = PhaseU(s);
            double target = st.Started ? Math.Max(0.0, st.Tk / PhaseT - baseU) : 0.0;
            st.E = NocMath.CriticalDamp(st.E, ref st.EVel, target, FollowerOmega, dt);
            double u = baseU + st.E;
            if (u < st.U) u = st.U;
            st.U = u;
            st.SPrev = s;
            return u;
        }

        // ------------------------------------------------------------------ Vector3 conveniences (builders)
        public static Vector3 ToVector3(D3 p) => new Vector3((float)p.x, (float)p.y, (float)p.z);
        public static Vector3 SpinePointV(double a, double phiHead, double tw) => ToVector3(SpinePoint(a, phiHead, tw));
        public static Vector3 RingPointV(double a, double theta, double phiHead, double tw) => ToVector3(RingPoint(a, theta, phiHead, tw));
    }
}
