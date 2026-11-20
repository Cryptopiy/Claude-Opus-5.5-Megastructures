using System;
using System.Globalization;
using NUnit.Framework;
using UnityEngine;

namespace Megastructure.Tests
{
    /// <summary>
    /// T-KOI (14.10) and 14.5.7 Acceptance (1) on KoiMath (V; D-032 2 shape by HT): L = the length of the reference spine (the
    /// 900 m plane curve the body follows; for the pre-D-032 orbit arc it is the integral of R dphi) 900.0 +-0.5 m, the head on the
    /// orbit, width 96.0 +-0.5 m, the head azimuth table within 0.5 deg (also through the 14.7 H6 phase follower at 1.5 and 2.5 m/s),
    /// the outline (a straight head and trunk, the bend behind), clearances over a full lap and all 20 wave phases >= 100 m to the
    /// spire surface (R 32 tapering to 20 at y 460), the screen-wall slab (x 834-874, z 275-520), the arcade and the ring towers
    /// (conservative cylinders R 456 (CityField exclusion, 14.4 "outside R 456") and R 470 (14.5.7 hero towers)), top y &lt;= 408 and
    /// lowest y &gt;= 160 for the body (the arcade rule 38 + 100 = 138 with a margin; 14.5.7's 192 was the orbit's), the veil below
    /// the cloud base, the documented 183-point composition sample set, the D-009 per-frame displacement gate. Numbers: [KOI].
    /// </summary>
    public sealed class KoiMathTests
    {
        static string F(double v) => v.ToString("0.000", CultureInfo.InvariantCulture);
        static void Report(string line) { TestContext.Out.WriteLine("[KOI] " + line); Debug.Log("[KOI] " + line); }

        const double Deg = Math.PI / 180.0;

        CultureInfo m_culture;
        [OneTimeSetUp] public void Invariant() { m_culture = CultureInfo.CurrentCulture; CultureInfo.CurrentCulture = CultureInfo.InvariantCulture; }
        [OneTimeTearDown] public void Restore() { if (m_culture != null) CultureInfo.CurrentCulture = m_culture; }

        // ------------------------------------------------------------------ length, width, profile
        [Test]
        public void BodyLengthIs900()
        {
            // D-032 2: L is the length of the reference spine, measured here as the polyline through its polar tables (independent
            // of the RK4 integration's own step) over 90 000 pieces
            int n = 90000;
            double L = 0.0, xp = 0.0, zp = 0.0;
            for (int i = 0; i <= n; i++)
            {
                double a = KoiMath.Length * i / n, r = KoiMath.RefRadius(a), al = KoiMath.RefAngle(a);
                double x = r * Math.Cos(al), z = -r * Math.Sin(al);
                if (i > 0) L += Math.Sqrt((x - xp) * (x - xp) + (z - zp) * (z - zp));
                xp = x; zp = z;
            }
            Assert.AreEqual(900.0, L, 0.5, "L = length of the reference spine (D-032 2)");
            // the head stays on the orbit (and keeps the azimuth table): spine point a = 0 minus its wave is the orbit point
            double worstHead = 0.0, minTrue = double.MaxValue, maxTrue = 0.0;
            for (int k = 0; k < 72; k++)
            {
                double phiH = k * 5.0 * Deg + 0.1, tw = 0.3 * k;
                D3 h = KoiMath.SpinePoint(0.0, phiH, tw), o = KoiMath.OrbitPoint(phiH);
                double w = KoiMath.Wave(0.0, tw);
                D3 hw = h - new D3(Math.Cos(phiH), 0.0, Math.Sin(phiH)) * w;
                worstHead = Math.Max(worstHead, (hw - o).Length);
                // the world spine (unwaved) is the reference spine on the tilted, slightly elliptic orbit: its true 3D length
                double lt = 0.0; D3 prev = default;
                for (int i = 0; i <= 1800; i++)
                {
                    double a = KoiMath.Length * i / 1800.0;
                    D3 p = KoiMath.SpinePoint(a, phiH, 0.0) - new D3(Math.Cos(KoiMath.AzimuthAt(a, phiH)), 0.0, Math.Sin(KoiMath.AzimuthAt(a, phiH))) * KoiMath.Wave(a, 0.0);
                    if (i > 0) lt += (p - prev).Length;
                    prev = p;
                }
                minTrue = Math.Min(minTrue, lt); maxTrue = Math.Max(maxTrue, lt);
            }
            Assert.Less(worstHead, 1e-6, "the head keeps the orbit (D-032 2)");
            double span = KoiMath.HeadAzimuth(0) - KoiMath.AzimuthAt(KoiMath.Length, KoiMath.HeadAzimuth(0));
            Report($"L: reference spine {L:F4} m; head off the orbit {worstHead:E2} m; unwaved world spine (orbit tilt +-60 m, ellipticity at the head, drop) {F(minTrue)}-{F(maxTrue)} m over 72 head azimuths; body spans {F(span / Deg)} deg of azimuth (orbit arc: 197.6)");
        }

        [Test]
        public void OutlineReadsAsAFish_StraightTrunkBendBehind()
        {
            // D-032 2 / INT-10: a carangiform swimmer bends behind its shoulders. Turning of the reference spine over the head and
            // trunk (a 0-400) against the orbit circle's 400 / 260 = 88.1 deg, and over the rear (400-900); the tail flicks back
            // (curvature below the rear's at the tip); the crab (head heading vs the orbit tangent) stays small; the spine never
            // runs radially (alpha increases), so N points outward everywhere
            double front = 0.0, rear = 0.0, kTip = KoiMath.Curvature(KoiMath.Length), kRearMax = 0.0;
            int n = 9000;
            for (int i = 0; i < n; i++)
            {
                double a = KoiMath.Length * (i + 0.5) / n, k = KoiMath.Curvature(a) * KoiMath.Length / n;
                if (a < 400.0) front += k; else rear += k;
                if (a > 400.0) kRearMax = Math.Max(kRearMax, KoiMath.Curvature(a));
                KoiMath.RefAngle(a, out double dal);
                Assert.Greater(dal, 0.0, "alpha increases along the body");
            }
            front /= Deg; rear /= Deg;
            Assert.Less(front, 0.8 * 400.0 / KoiMath.OrbitR0 / Deg, "head and trunk straighter than the orbit arc (<= 80 % of its turning)");
            Assert.Greater(rear, 500.0 / KoiMath.OrbitR0 / Deg, "the bend is in the rear half");
            Assert.Less(kTip, kRearMax - 0.001, "the tail flicks back out (S)");
            Assert.LessOrEqual(KoiMath.CrabDeg, 25.0, "crab angle at the head");
            KoiMath.RefRadius(0.0, out double dr0);
            Assert.AreEqual(-Math.Sin(KoiMath.CrabDeg * Deg), dr0, 1e-6, "the spine leaves the head turned CrabDeg inward");
            // the vertical S (Dy) is level at the head and at the tail end, so it joins its held values C1 (the shader takes dDy/da
            // from the same function: a slope at a = 0 against a held value tilted the head ring by 7 cm in HT-t01)
            KoiMath.Drop(0.0, out double dy0); KoiMath.Drop(KoiMath.Length, out double dyL);
            Assert.AreEqual(0.0, dy0, 1e-12); Assert.AreEqual(0.0, dyL, 1e-12);
            double c0 = (KoiMath.Drop(0.05) - KoiMath.Drop(-0.05)) / 0.1, cL = (KoiMath.Drop(KoiMath.Length + 0.05) - KoiMath.Drop(KoiMath.Length - 0.05)) / 0.1;
            Assert.AreEqual(0.0, c0, 1e-4); Assert.AreEqual(0.0, cL, 1e-4);
            Report($"outline: turning a 0-400 {F(front)} deg (orbit 88.1), a 400-900 {F(rear)} deg (orbit 110.2); curvature front {KoiMath.KFront:F5}, rear {KoiMath.KRear:F5}, tip {kTip:F5} 1/m; crab {F(KoiMath.CrabDeg)} deg; r_ref {F(KoiMath.RefRadius(0))} -> min {F(MinRef())} -> tail {F(KoiMath.RefRadius(KoiMath.Length))} m");
        }

        static double MinRef() { double m = double.MaxValue; for (int i = 0; i <= 900; i++) m = Math.Min(m, KoiMath.RefRadius(i)); return m; }

        [Test]
        public void WidthIs96AndProfileIsMonotoneThroughTheKnots()
        {
            double[] kx = { 0.0, 0.06, 0.18, 0.30, 0.50, 0.80, 1.0 };
            double[] ky = { 0.22, 0.60, 0.92, 1.00, 0.90, 0.45, 0.20 };
            for (int k = 0; k < kx.Length; k++) Assert.AreEqual(ky[k], KoiMath.Profile(kx[k]), 1e-12, "profile knot " + kx[k]);
            double maxW = 0.0, atA = 0.0;
            for (int i = 0; i <= 90000; i++)
            {
                double a = KoiMath.Length * i / 90000.0;
                double w = 2.0 * KoiMath.Radius(a);
                if (w > maxW) { maxW = w; atA = a; }
            }
            Assert.AreEqual(96.0, maxW, 0.5, "maximum width (14.5.7)");
            // monotone between knots: no value outside the range of the two neighbouring knots
            for (int k = 0; k < kx.Length - 1; k++)
            {
                double lo = Math.Min(ky[k], ky[k + 1]), hi = Math.Max(ky[k], ky[k + 1]);
                for (int i = 0; i <= 200; i++)
                {
                    double x = kx[k] + (kx[k + 1] - kx[k]) * i / 200.0;
                    double f = KoiMath.Profile(x);
                    Assert.IsTrue(f >= lo - 1e-12 && f <= hi + 1e-12, $"profile overshoot at x {F(x)}: {F(f)} outside [{lo}, {hi}]");
                }
            }
            Report($"width: max {F(maxW)} m at a {F(atA)} m (a/L {F(atA / KoiMath.Length)}); head radius {F(KoiMath.Radius(0))}, tail {F(KoiMath.Radius(KoiMath.Length))}");
        }

        // ------------------------------------------------------------------ head azimuth
        [Test]
        public void HeadAzimuthTableWithinHalfDegree()
        {
            for (int i = 0; i < KoiMath.TableS.Length; i++)
            {
                double deg = KoiMath.HeadAzimuthDeg(KoiMath.PhaseU(KoiMath.TableS[i]));
                Assert.AreEqual(KoiMath.TableDeg[i], deg, 0.5, "phi_h at s " + KoiMath.TableS[i]);
            }
            Assert.AreEqual(1.76, KoiMath.HeadDegSpan / KoiMath.PhaseD, 1e-12, "14.12 phi_h = 6.64 + 1.76 (s - 1260)");
        }

        [Test]
        public void PhaseFollowerKeepsTheTableAtConstantSpeed()
        {
            foreach (double v in new[] { 1.5, 2.5 })
            {
                var st = new KoiPhaseState();
                double dt = 1.0 / 60.0, s = 1200.0, worstDeg = 0.0, maxE = 0.0, uPrev = double.NegativeInfinity;
                int next = 0;
                while (s < 1500.0 + 2.0 * v * dt)
                {
                    double u = KoiMath.StepPhase(ref st, s, dt);
                    Assert.GreaterOrEqual(u, uPrev, "u held non-decreasing");
                    uPrev = u;
                    maxE = Math.Max(maxE, Math.Abs(st.E));
                    while (next < KoiMath.TableS.Length && s >= KoiMath.TableS[next] - 1e-9)
                    {
                        // the table s is crossed inside this frame: compare at the frame's s (the table is linear in s)
                        double expect = KoiMath.HeadAzimuthDeg(KoiMath.PhaseU(s));
                        worstDeg = Math.Max(worstDeg, Math.Abs(KoiMath.HeadAzimuthDeg(u) - expect));
                        Assert.AreEqual(KoiMath.TableDeg[next], KoiMath.HeadAzimuthDeg(KoiMath.PhaseU(KoiMath.TableS[next])), 0.5);
                        next++;
                    }
                    s += v * dt;
                }
                Assert.AreEqual(KoiMath.TableS.Length, next, "every table s crossed");
                Assert.Less(worstDeg, 0.5, $"head off the table at {v} m/s");
                Report($"follower at {v} m/s, 60 fps: max |E| {maxE:E2}, worst head deviation from the table {worstDeg:E2} deg");
            }
        }

        [Test]
        public void PhaseFollowerLetsALingeringPlayerWatchTheKoiSwim()
        {
            var st = new KoiPhaseState();
            double dt = 1.0 / 60.0, s = 1250.0;
            for (int i = 0; i < 60 * 20; i++) { KoiMath.StepPhase(ref st, s, dt); s += 1.5 * dt; }   // walk to s 1280
            double u0 = st.U;
            for (int i = 0; i < 60 * 30; i++) KoiMath.StepPhase(ref st, s, dt);                     // stand 30 s
            double du = st.U - u0;
            // standing 30 s: the target grows by 30 / 160 = 0.1875 in u (79 deg of head azimuth); the omega 0.8 follower lags it
            Assert.Greater(du, 0.12, "the koi keeps swimming while the player lingers");
            Assert.Less(du, 0.1875 + 1e-9, "never ahead of the time target");
            for (int i = 0; i < 60 * 10; i++) { KoiMath.StepPhase(ref st, s, dt); s -= 1.5 * dt; }   // walk back 15 m
            Assert.GreaterOrEqual(st.U, u0 + du - 1e-12, "u never decreases (walking back)");
            Report($"lingering 30 s at s 1280: u +{F(du)} ({F(du * KoiMath.HeadDegSpan)} deg of head azimuth; target +0.1875)");
        }

        // ------------------------------------------------------------------ sample set and frames
        [Test]
        public void SampleSetIsTheDocumented183Points()
        {
            Assert.AreEqual(164, KoiMath.CoreCount, "core = the spec's 164 sample points (41 stations x 4)");
            Assert.AreEqual(22.5, KoiMath.StationStep, 1e-12);
            Assert.AreEqual(183, KoiMath.SampleCount);
            int head = 0, body = 0, pec = 0, dor = 0, veil = 0, tail140 = 0;
            for (int i = 0; i < KoiMath.SampleCount; i++)
            {
                switch (KoiMath.SampleKind(i))
                {
                    case KoiPart.Head: head++; break;
                    case KoiPart.Body: body++; if (KoiMath.SampleRootA(i) >= KoiMath.Length - 140.0 - 1e-9) tail140++; break;
                    case KoiPart.Pectoral: pec++; break;
                    case KoiPart.Dorsal: dor++; break;
                    case KoiPart.Veil: veil++; break;
                }
            }
            Assert.AreEqual(8, head); Assert.AreEqual(156, body); Assert.AreEqual(8, pec); Assert.AreEqual(4, dor); Assert.AreEqual(7, veil);
            Assert.AreEqual(28, tail140, "the last 140 m of the tail = 28 samples (14.5.7 KS17)");
            var buf = new D3[KoiMath.SampleCount];
            KoiMath.Samples(KoiMath.HeadAzimuth(0.15), 3.0, buf);
            for (int i = 0; i < buf.Length; i++)
                Assert.IsFalse(double.IsNaN(buf[i].x) || double.IsNaN(buf[i].y) || double.IsNaN(buf[i].z), "sample " + i);
            // sample 1 is the head top: spine at a = 0 plus rho(0) along B
            KoiMath.Frame(0.0, KoiMath.HeadAzimuth(0.15), 3.0, out D3 c, out _, out _, out D3 b);
            Assert.AreEqual(0.0, (buf[1] - (c + b * KoiMath.Radius(0.0))).Length, 1e-9);
        }

        [Test]
        public void RingFramesFollowTheWavedSpineAndTheVeilIsRolled()
        {
            double phiH = KoiMath.HeadAzimuth(0.4), tw = 7.0;
            double maxTwist = 0.0;
            for (int i = 0; i <= 160; i++)
            {
                double a = KoiMath.Length * i / 160.0;
                KoiMath.Frame(a, phiH, tw, out D3 c, out D3 f, out D3 n, out D3 b);
                Assert.AreEqual(1.0, f.Length, 1e-9); Assert.AreEqual(1.0, n.Length, 1e-9); Assert.AreEqual(1.0, b.Length, 1e-9);
                Assert.AreEqual(0.0, D3.Dot(f, n), 1e-9); Assert.AreEqual(0.0, D3.Dot(f, b), 1e-9); Assert.AreEqual(0.0, D3.Dot(n, b), 1e-9);
                Assert.AreEqual(0.0, n.y, 1e-9, "side axis horizontal");
                Assert.Greater(b.y, 0.9, "B up-ish");
                // outward: N points away from S
                Assert.Greater((c.x - KoiMath.CentreX) * n.x + (c.z - KoiMath.CentreZ) * n.z, 0.0, "N outward");
                // the frame follows the WAVED spine: F differs from the unwaved orbit tangent by the wave slope
                double phi = KoiMath.AzimuthAt(a, phiH);
                var orbitTan = new D3(-Math.Sin(phi), 0, Math.Cos(phi));
                var fh = new D3(f.x, 0, f.z).Normalized;
                maxTwist = Math.Max(maxTwist, Math.Acos(NocMath.Clamp(D3.Dot(fh, orbitTan), -1, 1)) / Deg);
            }
            Assert.Greater(maxTwist, 2.0, "rings oriented along the waved spine (D-009: head-to-tail yaw 2.7-17.8 deg)");
            D3 top = KoiMath.VeilPoint(1.0, 1.0, phiH, tw), bot = KoiMath.VeilPoint(1.0, -1.0, phiH, tw);
            Assert.AreEqual(2.0 * KoiMath.VeilHalfSpan, (top - bot).Length, 1e-9, "veil span 140 m (HT)");
            Assert.AreEqual(2.0 * KoiMath.VeilHalfSpan * Math.Cos(KoiMath.VeilRollDeg * Deg), top.y - bot.y, 1e-9, "veil axis rolled 30 deg from the vertical");
            Report($"max horizontal angle between ring axis and unwaved orbit tangent at t_w 7: {F(maxTwist)} deg");
        }

        // ------------------------------------------------------------------ clearances (T-KOI, 14.5.7)
        /// <summary>Distance from p to the spire surface: R 32 at y 0 tapering linearly to 20 at y 460, 20 above.</summary>
        public static double SpireDistance(D3 p)
        {
            double dx = p.x - KoiMath.CentreX, dz = p.z - KoiMath.CentreZ;
            double r = Math.Sqrt(dx * dx + dz * dz);
            const double k = 12.0 / 460.0;
            if (p.y >= 460.0) return r - 20.0;
            return (r - (32.0 - k * Math.Max(0.0, p.y))) / Math.Sqrt(1.0 + k * k);
        }

        public static double BoxDistance(D3 p, double x0, double x1, double y0, double y1, double z0, double z1)
        {
            double dx = Math.Max(0.0, Math.Max(x0 - p.x, p.x - x1));
            double dy = Math.Max(0.0, Math.Max(y0 - p.y, p.y - y1));
            double dz = Math.Max(0.0, Math.Max(z0 - p.z, p.z - z1));
            return Math.Sqrt(dx * dx + dy * dy + dz * dz);
        }

        static double Horizontal(D3 p) { double dx = p.x - KoiMath.CentreX, dz = p.z - KoiMath.CentreZ; return Math.Sqrt(dx * dx + dz * dz); }

        sealed class Clear
        {
            public double spire = double.MaxValue, wall = double.MaxValue, arcade = double.MaxValue, rMax = 0.0, top = double.MinValue, low = double.MaxValue;
            public void Add(D3 p)
            {
                spire = Math.Min(spire, SpireDistance(p));
                wall = Math.Min(wall, BoxDistance(p, 834, 874, -1500, 560, 275, 520));
                arcade = Math.Min(arcade, BoxDistance(p, 914, 990, 0, 38, 236, 360));
                rMax = Math.Max(rMax, Horizontal(p));
                top = Math.Max(top, p.y);
                low = Math.Min(low, p.y);
            }
            public string Line(string what) =>
                $"{what}: spire {F(spire)} m, screen wall {F(wall)} m, ring towers R456 {F(456 - rMax)} m / R470 {F(470 - rMax)} m (max radius {F(rMax)}), arcade {F(arcade)} m, top y {F(top)}, lowest y {F(low)}";
        }

        [Test]
        public void ClearancesOverAFullLapAndAllWavePhases()
        {
            var body = new Clear();
            var pec = new Clear();
            var dor = new Clear();
            var veil = new Clear();
            const int thetas = 16;
            for (int lap = 0; lap < 360; lap++)
            {
                double phiH = lap * Deg;
                for (int k = 0; k < KoiMath.WavePhases; k++)
                {
                    double tw = KoiMath.PhaseWaveTime(k);
                    for (int i = 0; i <= 120; i++)
                    {
                        double a = KoiMath.Length * i / 120.0;
                        KoiMath.Frame(a, phiH, tw, out D3 c, out _, out D3 n, out D3 b);
                        double rho = KoiMath.Radius(a);
                        for (int j = 0; j < thetas; j++)
                        {
                            double th = 2.0 * Math.PI * j / thetas;
                            body.Add(c + n * (rho * Math.Cos(th)) + b * (rho * Math.Sin(th)));
                        }
                    }
                    for (int side = -1; side <= 1; side += 2)
                        for (int u = 0; u <= 2; u++)
                            for (int v = 0; v <= 2; v++) pec.Add(KoiMath.PectoralPoint(side, u * 0.5, v * 0.5, phiH, tw));
                    for (int u = 0; u <= 4; u++) dor.Add(KoiMath.DorsalPoint(u * 0.25, 1.0, phiH, tw));
                    dor.Add(KoiMath.DorsalPoint(KoiMath.DorsalPeakAt, 1.0, phiH, tw));
                    for (int v = 0; v <= 4; v++)
                        for (int sg = -1; sg <= 1; sg += 2) veil.Add(KoiMath.VeilPoint(v * 0.25, sg, phiH, tw));
                }
            }
            Report(body.Line("body (orbit arc: spire 159, wall 125, towers 120, top 408, lowest 192)"));
            Report(pec.Line("pectoral fins"));
            Report(dor.Line("dorsal fin"));
            Report(veil.Line("tail veil (D-009 120 x 120, vertical)"));
            foreach (var c in new[] { body, pec, dor, veil })
            {
                Assert.GreaterOrEqual(c.spire, 100.0, "clearance to the spire surface >= 100 m");
                Assert.GreaterOrEqual(c.wall, 100.0, "clearance to the screen-wall slab >= 100 m");
                Assert.GreaterOrEqual(456.0 - c.rMax, 100.0, "clearance to the ring towers (R 456 cylinder) >= 100 m");
                Assert.Less(c.top, NocturneConstants.CloudBaseY, "below the cloud base y 460 (D-009)");
                Assert.GreaterOrEqual(c.arcade, 100.0, "clearance to the arcade (x 914-990, z 236-360, 38 m) >= 100 m");
            }
            Assert.LessOrEqual(body.top, 408.0 + 0.5, "body top y 408 (14.5.7)");
            Assert.GreaterOrEqual(body.low, 160.0, "body lowest y 160 (D-032 2: the tail sinks 20 m; arcade 38 + 100 = 138)");
        }

        // ------------------------------------------------------------------ per-frame displacement (14.7 H6, D-009)
        [Test]
        public void PerFrameDisplacementGate()
        {
            // play at 2.5 m/s, 60 fps: u advances 2.5 / 60 / 240 per frame, t_w by 1 / 60 s
            double dt = 1.0 / 60.0, du = 2.5 * dt / KoiMath.PhaseD;
            var p0 = new D3[KoiMath.SampleCount];
            var p1 = new D3[KoiMath.SampleCount];
            var pw = new D3[KoiMath.SampleCount];
            double maxSurf = 0.0, maxWave = 0.0, maxSpine = 0.0, maxSpineWave = 0.0;
            for (int step = 0; step < 240; step++)
            {
                double u = step * (1.0 / 240.0);
                double tw = 0.37 * step;
                double h0 = KoiMath.HeadAzimuth(u), h1 = KoiMath.HeadAzimuth(u + du);
                KoiMath.Samples(h0, tw, p0);
                KoiMath.Samples(h1, tw + dt, p1);
                KoiMath.Samples(h0, tw + dt, pw);
                for (int i = 0; i < KoiMath.CoreCount; i++)
                {
                    maxSurf = Math.Max(maxSurf, (p1[i] - p0[i]).Length);
                    maxWave = Math.Max(maxWave, (pw[i] - p0[i]).Length);
                }
                for (int i = 0; i <= 90; i++)
                {
                    double a = KoiMath.Length * i / 90.0;
                    D3 c0 = KoiMath.SpinePoint(a, h0, tw);
                    maxSpine = Math.Max(maxSpine, (KoiMath.SpinePoint(a, h1, tw + dt) - c0).Length);
                    maxSpineWave = Math.Max(maxSpineWave, (KoiMath.SpinePoint(a, h0, tw + dt) - c0).Length);
                }
            }
            Report($"per frame at 2.5 m/s, 60 fps: spine total {F(maxSpine)} m (D-009 gate 0.45), spine wave part {F(maxSpineWave)} m (gate 0.209); surface samples total {F(maxSurf)} m, wave part {F(maxWave)} m (a surface point at radius R + w + rho about S moves (R + w + rho) / R faster than the spine)");
            Assert.LessOrEqual(maxSpine, 0.45, "D-009: total body (spine) displacement <= 0.45 m per frame at 60 fps");
            Assert.LessOrEqual(maxSpineWave, 0.2094 + 1e-3, "D-009: wave part 40 x 2 pi / 20 / 60 = 0.2094 m per frame");
            // surface points: the spine bound times the largest (R + w + rho) / R = (242 + 57.7) / 242 = 1.24 ratio of the orbit
            Assert.LessOrEqual(maxSurf, 0.45 * 1.24, "surface samples within the radius ratio of the spine gate");
        }
    }
}
