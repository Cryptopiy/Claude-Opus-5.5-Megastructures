// MistVolumeMath: the C# twin of Shaders/Include/BwMistVolume.hlsl (MASTER_PROMPT 14.7 H2, 14.5.3). Pure double maths,
// no UnityEngine state, so EditMode tests (MistTests) check the numbers the shader uses: the plume density and its peak
// horizontal optical depth 1.0, the spray veil (optical depth 0.02), the curtain (the 14.5.3 parabola, Gaussian radius
// 3 m at 90 m growing to 60 m at 1 km, water fraction 0 at 1000 m), the wall-spray ellipsoids (8 x 12 x 8 m, 0.6 s in,
// 2.4 s out), the fixed 24-sample grid, the Henyey-Greenstein phase (g 0.55) with the multiple-scattering octaves, and
// the rainbow phase term exactly as H2 (primary exp(-((theta - theta_c) / 0.9)^2) per channel with theta_c = 42.0, 41.0,
// 40.3 deg, secondary at 50.8, 52.0, 53.5 weight 0.25, dark band 0.6 between 42.5 and 50).
// Every constant here has a #define twin in BwMistVolume.hlsl (MIST_*); MistTests.Hlsl_ConstantsMatchTheTwin parses the
// include and compares them, so the two cannot drift.
//
// Units: density rho is dimensionless; extinction = Sigma rho per metre (transmittance exp(-sum 0.02 rho dt), H2).
using System;

namespace Megastructure.Rendering
{
    public static class MistVolumeMath
    {
        // ------------------------------------------------------------------ integration (H2)
        /// <summary>Extinction per unit density (1/m): transmittance exp(-sum 0.02 rho dt).</summary>
        public const double Sigma = 0.02;
        /// <summary>Fixed samples between the analytic entry and exit of the bounding volume (no jitter).</summary>
        public const int Samples = 24;
        /// <summary>Sample spacing exponent: t_i = t0 + (t1 - t0) u^p, u in [0, 1] (denser near the eye, still fixed).</summary>
        public const double SpacingPower = 1.6;
        /// <summary>Density modulation 0.65 + 0.35 fbm3(p / 60 m + wind t) (H2, continuous in time).</summary>
        public const double FbmBase = 0.65, FbmAmp = 0.35, FbmScale = 60.0;
        /// <summary>Plume breathing wind (m/s): the mist climbs the lee face at 5 m/s (14.5.3 "plume breathing (5 m/s)") and
        /// drifts slowly west and north.</summary>
        public const double WindX = -1.0, WindY = 5.0, WindZ = 0.5;

        // ------------------------------------------------------------------ plume (14.5.3, H2)
        public const double PlumeZ = BulwarkConst.PlumeZ;               // 562
        public const double PlumeSigmaZ = BulwarkConst.PlumeSigmaZ;     // 150
        public const double PlumeScaleH = BulwarkConst.PlumeScaleHeight; // 130
        public const double PlumeXFace = BulwarkConst.PlumeXMax;        // -6 (the lee face)
        public const double PlumeXFar = BulwarkConst.PlumeXMin;         // -2500
        /// <summary>Density fade next to the lee face (m, smoothstep): "faded near the lee edge" (14.5.3 pitfalls), so the
        /// rails and the deck edge are not swallowed by mist starting 0.1 m behind them.</summary>
        public const double PlumeFaceFade = 40.0;
        /// <summary>Soft end of the plume toward x -2500 (m, smoothstep).</summary>
        public const double PlumeFarFade = 200.0;
        /// <summary>Bounding box of the plume (the 24 samples run between its analytic entry and exit).</summary>
        public const double PlumeYMin = -6000.0, PlumeYMax = 700.0, PlumeZHalf = 600.0;   // 4 sigma in z, exp(-700/130) = 0.005 at the top
        /// <summary>Profile along x: the mist is densest where it climbs the lee face and thins outward, exp(-(-6 - x) / 600 m)
        /// (logged arbitration: with the uniform profile the optical depth ACROSS the plume, sigma 150 m in z, is only
        /// 0.02 rho0 376 m = 0.16, so the plume KS06 must show was +1..6 levels in the round 3 captures; concentrated toward
        /// the face it is 0.64 near the face while the peak horizontal optical depth ALONG x stays exactly 1.0).</summary>
        public const double PlumeXScale = 600.0;
        public static double PlumeXProfile(double x)
            => Smooth((PlumeXFace - x) / PlumeFaceFade) * Smooth((x - PlumeXFar) / PlumeFarFade) * Math.Exp(-(PlumeXFace - x) / PlumeXScale);
        /// <summary>Base density: a horizontal ray along x through the core (z 562, y below 0) has optical depth exactly
        /// PeakOpticalDepth (1.0) with the fbm term at its maximum 1.0 (numerical integral of the x profile, 0.25 m steps).</summary>
        public static readonly double PlumeRho0 = BulwarkConst.PlumePeakOpticalDepth / (Sigma * IntegrateXProfile());

        static double IntegrateXProfile()
        {
            double sum = 0.0, dx = 0.25;
            for (double x = PlumeXFace - 0.5 * dx; x > PlumeXFar; x -= dx) sum += PlumeXProfile(x) * dx;
            return sum;
        }

        /// <summary>Plume base density at a world point (no fbm).</summary>
        public static double PlumeDensity(double x, double y, double z)
        {
            if (x > PlumeXFace || x < PlumeXFar) return 0.0;
            double dz = z - PlumeZ;
            double g = Math.Exp(-dz * dz / (2.0 * PlumeSigmaZ * PlumeSigmaZ));
            double v = Math.Exp(-Math.Max(y, 0.0) / PlumeScaleH);
            return PlumeRho0 * g * v * PlumeXProfile(x);
        }

        // ------------------------------------------------------------------ spray veil (H2: optical depth 0.02 below 400 m
        // within 1.5 km of the lee face, s 560-640). "Altitude" is read as height above the crest road (y < 400): every
        // KS08 arch ray (elevation -2.49 .. +25.4 deg from y 1.7) is below y 714 within 1.5 km of the face, while no arch
        // ray reaches 6000 + y < 400 (y < -5600) within 1.5 km (that needs a pitch below -75 deg; the bow circle reaches only
        // -58.6 deg). Logged decision (DEVELOPMENT.md, the mist report).
        public const double VeilOpticalDepth = 0.02, VeilWidth = 1500.0, VeilTop = 400.0, VeilTopFade = 100.0;
        public const double VeilZHalf = 1500.0, VeilZFade = 300.0, VeilXFade = 150.0;
        /// <summary>Soft start of the veil at the lee face (m, smoothstep; Wave X). Without it the veil began at full density at
        /// x -6 and a ray grazing the face plane (looking along the dam, back640: yaw -160) crossed a 2.5-level vertical seam
        /// along the vanishing line of the face (mistx-01: luma 136 -> 139 at column 663, absent with -mist off).</summary>
        public const double VeilFaceFade = 150.0;
        public const double VeilS0 = 560.0, VeilS1 = 640.0, VeilSFade = 15.0;
        /// <summary>Veil density: a horizontal ray across the 1.5 km (x -6 .. -1506) has optical depth 0.02 (both smoothstep
        /// edges integrate to half their width).</summary>
        public static readonly double VeilRho = VeilOpticalDepth / (Sigma * (VeilWidth - 0.5 * VeilXFade - 0.5 * VeilFaceFade));

        /// <summary>Veil weight by progress: 0 outside s 545-655, 1 inside s 560-640 (smoothstep ramps).</summary>
        public static double VeilWeight(double s)
            => Smooth((s - (VeilS0 - VeilSFade)) / VeilSFade) * (1.0 - Smooth((s - VeilS1) / VeilSFade));

        public static double VeilDensity(double x, double y, double z, double weight)
        {
            if (weight <= 0.0 || x > PlumeXFace || x < PlumeXFace - VeilWidth) return 0.0;
            double fx = Smooth((x - (PlumeXFace - VeilWidth)) / VeilXFade) * Smooth((PlumeXFace - x) / VeilFaceFade);
            double fy = 1.0 - Smooth((y - (VeilTop - VeilTopFade)) / VeilTopFade);
            double dz = Math.Abs(z - PlumeZ);
            double fz = 1.0 - Smooth((dz - (VeilZHalf - VeilZFade)) / VeilZFade);
            return weight * VeilRho * fx * fy * fz;
        }

        // ------------------------------------------------------------------ curtain (14.5.3 sheets beyond 90 m)
        /// <summary>Sheet parabola x = -6 - 8.3 t, y = -11.5 - 4.9 t^2 as a surface y = -11.5 - k (x + 6)^2.</summary>
        public const double LipX = BulwarkConst.LipX, LipY = BulwarkConst.LipY, LipSpeed = BulwarkConst.LipSpeed;
        public static readonly double CurtainK = BulwarkConst.SheetGravityHalf / (BulwarkConst.LipSpeed * BulwarkConst.LipSpeed);
        /// <summary>The curtain takes over from the near sheets (mesh, BwSheet) at 90 m of drop: it fades in over 70-95 m,
        /// while the mesh dissolves by alpha-to-coverage over 80-108 m.</summary>
        public const double CurtainD0 = 90.0, CurtainFadeIn0 = 70.0, CurtainFadeIn1 = 95.0;
        /// <summary>Gaussian radius 3 m at 90 m growing to 60 m at 1 km (power law): r = 3 (D / 90)^e.</summary>
        public const double CurtainR0 = 3.0, CurtainR1 = 60.0, CurtainD1 = 1000.0;
        public static readonly double CurtainRExp = Math.Log(CurtainR1 / CurtainR0) / Math.Log(CurtainD1 / CurtainD0);
        /// <summary>Normal optical depth of the falling water (opaque white curtain) and of its mist share; the water fraction
        /// falls to 0 at 1000 m, where the plume takes over (the curtain itself ends by 1100 m).</summary>
        public const double CurtainWaterTau = 5.0, CurtainMistTau = 0.8, CurtainEnd0 = 900.0, CurtainEnd1 = 1100.0;

        /// <summary>Drop below the lip at flight time t (s).</summary>
        public static double CurtainDropAt(double t) => BulwarkConst.SheetGravityHalf * t * t;
        /// <summary>Centre-line x at drop D below the lip.</summary>
        public static double CurtainX(double drop) => LipX - LipSpeed * Math.Sqrt(Math.Max(drop, 0.0) / BulwarkConst.SheetGravityHalf);
        /// <summary>Gaussian radius (m) at drop D (clamped to the 90 m value above it).</summary>
        public static double CurtainRadius(double drop) => CurtainR0 * Math.Pow(Math.Max(drop, CurtainD0) / CurtainD0, CurtainRExp);
        /// <summary>Water fraction: 1 at 90 m, 0 at 1000 m (then mist).</summary>
        public static double WaterFraction(double drop)
        {
            double u = Clamp01((drop - CurtainD0) / (CurtainD1 - CurtainD0));
            return (1.0 - u) * (1.0 - u);
        }
        /// <summary>Normal optical depth of the curtain at drop D (fade-in, water + mist, end).</summary>
        public static double CurtainNormalTau(double drop)
        {
            double fin = Smooth((drop - CurtainFadeIn0) / (CurtainFadeIn1 - CurtainFadeIn0));
            double fout = 1.0 - Smooth((drop - CurtainEnd0) / (CurtainEnd1 - CurtainEnd0));
            double w = WaterFraction(drop);
            return fin * fout * (CurtainWaterTau * w + CurtainMistTau * (1.0 - w));
        }
        /// <summary>Across-slot profile 0..1: the 14 slots (16 m) blurred by the Gaussian radius (the piers' 4 m gaps fill in
        /// as the curtain spreads).</summary>
        public static double CurtainSlotProfile(double z, double radius)
        {
            double r = Math.Max(radius, 0.05) * Math.Sqrt(2.0), sum = 0.0;
            for (int k = 0; k < BulwarkConst.SlotCount; k++)
            {
                double a = BulwarkConst.SlotZ0 + BulwarkConst.PierPitch * k, b = a + BulwarkConst.SlotLength;
                sum += 0.5 * (Erf((z - a) / r) - Erf((z - b) / r));
            }
            return Clamp01(sum);
        }

        /// <summary>Ray parameter s of the forward intersection of o + s d with the curtain surface (x below -6), or -1.</summary>
        public static double CurtainHit(D3 o, D3 d)
        {
            double k = CurtainK, ox = o.x - LipX;
            double a = k * d.x * d.x, b = 2.0 * k * d.x * ox + d.y, c = k * ox * ox + (o.y - LipY);
            double best = -1.0;
            if (Math.Abs(a) < 1e-12)
            {
                if (Math.Abs(b) > 1e-12) best = -c / b;
            }
            else
            {
                double disc = b * b - 4.0 * a * c;
                if (disc < 0.0) return -1.0;
                double sq = Math.Sqrt(disc);
                double q = -0.5 * (b + (b >= 0 ? sq : -sq));
                double s0 = q / a, s1 = Math.Abs(q) > 1e-12 ? c / q : -1.0;
                double lo = Math.Min(s0, s1), hi = Math.Max(s0, s1);
                foreach (double s in new[] { lo, hi })
                {
                    if (s <= 0.0) continue;
                    if (o.x + d.x * s <= LipX) { best = s; break; }
                }
                return best;
            }
            if (best > 0.0 && o.x + d.x * best <= LipX) return best;
            return -1.0;
        }

        // ------------------------------------------------------------------ wall sprays (H2, WaveImpacts)
        /// <summary>Ellipsoid full size 8 x 12 x 8 m (semi-axes 4, 6, 4), 0.6 s in, 2.4 s out.</summary>
        public const double SprayAx = 4.0, SprayAy = 6.0, SprayAz = 4.0, SprayIn = 0.6, SprayOut = 2.4;
        /// <summary>Optical depth through the centre along x at strength 1 once the spray has become the mist carried over the
        /// road (14.5.4 "optical depth at most 0.5").</summary>
        public const double SprayPeakTau = 0.5;
        /// <summary>Optical depth of the young white-water burst at strength 1 (round 6: at 0.5 the KS04 forced plume was a
        /// +10 level smudge on the sea, wb-mist-05 pose_spray175). It thins to SprayPeakTau over SprayCoreFade0..1 s of age,
        /// before the drift (from 0.8 s) carries it over the road: thrown water is a dense white burst at the wall and
        /// becomes a thin mist as it spreads (mass conserved over a 1.5x growth gives 1 / 1.5^2 of the density at least).</summary>
        public const double SprayCoreTau = 2.5;
        public const double SprayCoreFade0 = 0.6, SprayCoreFade1 = 1.4;
        /// <summary>Rise time of the spray centre from the sea to its peak height (s): the burst reaches its height when its
        /// density peaks (0.6-0.9 s; a forced capture plume is shown at 0.8 s), then drifts and thins over 2.4 s; the luma of
        /// the frame still changes over more than 1 s (14.2.5 "plumes take 1-2 s"; the plume covers under 10 % of a frame).</summary>
        public const double SprayRise = 0.9;
        /// <summary>Drift of the spray mist over the road toward the lowland (m/s): 4 in the storm (beats 2-3), 9 on the reef
        /// (14.5.4 "carried over the road by a 9 m/s wind").</summary>
        public const double SprayDriftStorm = 4.0, SprayDriftReef = BulwarkConst.ReefWind;
        /// <summary>Forward share of a spray's ambient in-scatter (BwMistVolume.hlsl, the spray loop): white-water drops of
        /// 0.1-1 mm scatter with a Mie asymmetry of about 0.85 (Hansen and Travis 1974, large water drops), so most of the sky light a spray sends to
        /// the eye comes from the haze band just behind it; the ambient is lerp(dome average, band radiance along the ray,
        /// 0.85) with the albedo of water, 1.0 (SprayAlbedo). Round 8: with the isotropic dome average alone (0.55 of the horizon under the storm sky) every spray seen
        /// against the horizon was a dark smoke puff (KS06, pose_spray175, the kraken sprays).</summary>
        public const double SprayForward = 0.85;
        /// <summary>Forward share of the WALL sprays (Wave X): 0.6. With the white-water term (SprayWhiteWall 0.5) and 0.85 the
        /// storm wall spray had the radiance of the haze band behind it and the KS04 forced plume read as a faint glowing
        /// pillar (mistx-09: 4.6 k px over 4 levels against 35 k for the round-9 dark smoke); a little more of the dome
        /// average (the dark sea below, the grey deck above) gives the thin parts a grey body and leaves the thick core white.</summary>
        public const double SprayForwardWall = 0.6;
        /// <summary>Ambient albedo of white-water spray (pure water absorbs under 0.04 % across a 1 mm drop in the visible: the
        /// sun term of the mist is already lossless; the plume keeps MIST_ALBEDO 0.92 for its tuned bow and lowland).</summary>
        public const double SprayAlbedo = 1.0;
        /// <summary>Spray fbm tile (m, spray-local: the texture rides with the spray; 4 m cells, finest 1 m): the 60 m plume
        /// scale would leave an 8 m spray untextured.</summary>
        public const double SprayFbmScale = 16.0;
        /// <summary>Wall sprays sample their billow noise on a tile of SprayFbmScale x 0.5 = 8 m across (Wave X).</summary>
        public const double WallSprayTileK = 0.5;
        /// <summary>Spray billows: density x (0.35 + 0.65 smoothstep(0.15, 0.85, noise)) from two fetches of the tile (the
        /// plume's 0.65 + 0.35 fbm of H2 left an 8 m spray a smooth glowing blob in the round 2 captures).</summary>
        public const double SprayBillowBase = 0.35;

        /// <summary>Density envelope 0..1 of a spray at age t (s): smoothstep in over 0.6 s, out over the following 2.4 s.</summary>
        public static double SprayEnvelope(double age)
        {
            if (age <= 0.0 || age >= SprayIn + SprayOut) return 0.0;
            return Smooth(age / SprayIn) * (1.0 - Smooth((age - SprayIn) / SprayOut));
        }
        /// <summary>Peak density of a spray of the given strength (unit ellipsoid profile (1 - r^2)^2: tau = Sigma rho 16/15 a).</summary>
        public static double SprayRho(double strength) => SprayPeakTau * Clamp01(strength) / (Sigma * (16.0 / 15.0) * SprayAx);
        /// <summary>Density factor of the young burst over the drifting mist: SprayCoreTau / SprayPeakTau at the peak, 1 once
        /// the spray is mist (smoothstep over SprayCoreFade0..1 s).</summary>
        public static double SprayCoreBoost(double age)
            => 1.0 + (SprayCoreTau / SprayPeakTau - 1.0) * (1.0 - Smooth((age - SprayCoreFade0) / (SprayCoreFade1 - SprayCoreFade0)));
        /// <summary>The spray's density relative to the mist peak at age t: envelope x core boost (0 before birth and after 3 s).</summary>
        public static double SprayDensityFactor(double age) => SprayEnvelope(age) * SprayCoreBoost(age);
        /// <summary>Near-eye sprays (Wave X, 14.10.3 luma step, 14.2.5 photosensitivity): an impact within SprayNearD0 (10 m,
        /// horizontal) of the eye rises over SprayInNear (1.8 s) instead of 0.6 s and has no dense core; the factor blends to
        /// the normal one at SprayNearD1 (30 m). The life stays 3.0 s (WaveImpacts.Lifetime). Peak rates at nearness 1: in 1.5 /
        /// 1.8 s = 2.8 % per 30 fps frame, out 1.5 / 1.2 s = 4.2 %; a KS11 impact 6 m from the eye lifted the frame mean by
        /// up to 6.1 levels per frame with the 0.6 s rise and the x5 core (mistx-07-seqk5 frame 260).</summary>
        public const double SprayNearD0 = 10.0, SprayNearD1 = 30.0, SprayInNear = 1.8;
        public static double SprayNearness(double horizontalDistance) => 1.0 - Smooth((horizontalDistance - SprayNearD0) / (SprayNearD1 - SprayNearD0));
        public static double SprayDensityFactorNear(double age, double nearness)
        {
            double total = SprayIn + SprayOut;
            if (age <= 0.0 || age >= total) return 0.0;
            double near = Clamp01(nearness);
            double inT = SprayIn + near * (SprayInNear - SprayIn), outT = total - inT;
            double env = Smooth(age / inT) * (1.0 - Smooth((age - inT) / outT));
            return env * (1.0 + (1.0 - near) * (SprayCoreBoost(age) - 1.0));
        }
        /// <summary>Spray centre and growth at age t: rises from the sea surface to (height above the crest - semi-axis y),
        /// drifts west after 0.8 s, grows 50 % over its life.</summary>
        public static void SprayPose(double x, double z, double height, double age, double drift, out double cx, out double cy, out double cz, out double grow)
        {
            double rise = Smooth(age / SprayRise);
            double top = Math.Max(height, SprayAy + 1.0);
            cy = BulwarkConst.SeaY + (top - SprayAy - BulwarkConst.SeaY) * rise;
            cx = x - drift * Math.Max(0.0, age - 0.8);
            cz = z;
            grow = 1.0 + 0.5 * Clamp01(age / (SprayIn + SprayOut));
        }
        /// <summary>Wave X (show report: "a floating spray ball near the gate tower at KS11"): a wall spray is a jet thrown up
        /// FROM the sea. The 12 m ellipsoid of SprayPose (centre cy, semi-axis ay) gives the top of the thrown water, cy + ay;
        /// the drawn ellipsoid spans from SprayFootDepth under the mean sea to that top: semi-axis max(ay, (top - bottom) / 2),
        /// centre top - semi-axis. KS11 (14 m plume at 0.9 s, top 14.9): centre 2.95, semi-axis 11.95: at the parapet top
        /// (y 1.1) the axis density is (1 - (1.85 / 11.95)^2)^2 = 0.95 of the core, so the plume rises over the parapet (the
        /// old ellipsoid, centre 8.0, had 0.01 there and floated from y 3.5 up). The top (KS04PlumeBounds) is unchanged.
        /// (A split ellipsoid, upper half 6.9 m and lower 18 m, showed a 4-level seam at its centre plane in mistx-14 KS04.)</summary>
        public const double SprayFootDepth = 1.0;
        public static void WallColumn(double cy, double ay, out double cyc, out double ayc)
        {
            double top = cy + ay, bottom = BulwarkConst.SeaY - SprayFootDepth;
            ayc = Math.Max(ay, 0.5 * (top - bottom));
            cyc = top - ayc;
        }
        /// <summary>A wall jet rises just seaward of the sea face (m): its 4.6 m horizontal semi-axis centred on x 6 put a veil of
        /// spray on the road in front of the parapet (mistx-11 KS11); 2.5 m out, 1.6 m of the base overhangs the parapet (x 5.5)
        /// at under a third of the axis density, the rest stands over the water.</summary>
        public const double WallJetOffset = 2.5;

        /// <summary>C# twin of BwMistVolume.hlsl MistSprayTau WITHOUT the noise, the erosion and the near fade (each at most 1,
        /// so this is an upper bound of the drawn optical depth): the chord of slot (C, R, S) along o + d t, t in [t0, t1],
        /// d unit; the lean a shear of the axis.</summary>
        public static double SprayChordTauBound(UnityEngine.Vector4 C, UnityEngine.Vector4 R, UnityEngine.Vector4 S, D3 o, D3 d, double t0, double t1)
        {
            if (!(C.w > 0f)) return 0.0;
            double cyw = Math.Cos(R.w), syw = Math.Sin(R.w);
            D3 rel = new D3(o.x - C.x, o.y - C.y, o.z - C.z);
            rel.x -= S.x * rel.y; rel.z -= S.y * rel.y;
            D3 dd = d; dd.x -= S.x * d.y; dd.z -= S.y * d.y;
            D3 oL = new D3(rel.x * cyw - rel.z * syw, rel.y, rel.x * syw + rel.z * cyw);
            D3 dL = new D3(dd.x * cyw - dd.z * syw, dd.y, dd.x * syw + dd.z * cyw);
            double ix = 1.0 / Math.Max(R.x, 1e-3), iy = 1.0 / Math.Max(R.y, 1e-3), iz = 1.0 / Math.Max(R.z, 1e-3);
            return Sigma * C.w * UnitChord(oL, dL, ix, iy, iz, t0, t1);
        }

        static double UnitChord(D3 o, D3 d, double ix, double iy, double iz, double ta, double tb)
        {
            if (!(tb > ta)) return 0.0;
            D3 o2 = new D3(o.x * ix, o.y * iy, o.z * iz), d2 = new D3(d.x * ix, d.y * iy, d.z * iz);
            double A = D3.Dot(d2, d2);
            if (A < 1e-12) return 0.0;
            double Bh = D3.Dot(o2, d2), sm = -Bh / A, q = 1.0 - (D3.Dot(o2, o2) - Bh * Bh / A);
            if (q <= 0.0) return 0.0;
            double h = Math.Sqrt(q / A);
            double u0 = Math.Max(-h, ta - sm), u1 = Math.Min(h, tb - sm);
            if (u1 <= u0) return 0.0;
            double h2 = h * h, h4 = h2 * h2;
            double F1 = h4 * u1 - (2.0 / 3.0) * h2 * u1 * u1 * u1 + u1 * u1 * u1 * u1 * u1 * 0.2;
            double F0 = h4 * u0 - (2.0 / 3.0) * h2 * u0 * u0 * u0 + u0 * u0 * u0 * u0 * u0 * 0.2;
            return A * A * Math.Max(F1 - F0, 0.0);
        }

        /// <summary>Optical depth of the chord [u0, u1] (relative to the closest approach) through a unit-profile ellipsoid:
        /// tau = Sigma rho A^2 (F(u1) - F(u0)), F(u) = h^4 u - 2/3 h^2 u^3 + u^5 / 5, A = |d'|^2, h = half chord.</summary>
        public static double EllipsoidChordTau(double rho, double A, double h, double u0, double u1)
        {
            u0 = Math.Max(u0, -h); u1 = Math.Min(u1, h);
            if (u1 <= u0) return 0.0;
            Func<double, double> F = u => h * h * h * h * u - (2.0 / 3.0) * h * h * u * u * u + u * u * u * u * u / 5.0;
            return Sigma * rho * A * A * (F(u1) - F(u0));
        }

        // ------------------------------------------------------------------ sea-disturbance sprays (Addendum A1: the kraken)
        // The kraken choreography publishes where its body and tentacles touch the sea (Core/SeaDisturbances, one writer).
        // The bus carries only the instantaneous state (strength 0..1, height above the sea), so the mist keeps one tracker
        // per disturbance (matched frame to frame by kind and position) with critically damped followers of its density and
        // height: the spray RISES in 1-2 s when the body breaks the surface and FALLS over 3-6 s after it sinks, smoothly
        // (no step: density change at most 5 % per 30 fps frame). Capture stills snap the followers to their targets (H6:
        // the time branch equals the pose). Every spray is an ellipsoid of the wall-spray kind (closed-form chord), centred
        // on the mean sea surface (the lower half is under the water, so the profile is a dome: dense at the waterline,
        // thinning upward). Wave X shapes (the creature must stay readable: KK2 needs the eyes at +76 m clear, KK1 the
        // 180 m arm; the round-9 column of 1.4 x the body height buried both):
        //   Breach       a white FOOT skirt at the waterline (1.15 x the half extents + 6 m, vertical semi-axis 1.4 x
        //                min(0.3 h + 10, 40) m: at most 56 m, under the eyes; optical depth 2.5 through the centre,
        //                leaning 0.25 downwind) and a CASCADE of water sheeting off the body (1.05 x + 3 m, vertical semi-axis
        //                1.1 x (h + 4): the profile keeps under 6 % at the top of the body; optical depth 0.6, nearly upright,
        //                lean 0.05, noise falling at 5 m/s) whose amount follows the vertical speed of the body
        //                (CascadeActivity);
        //   TentacleRise a FOOT (2.2 x the waterline radius + 4 m, vertical semi-axis 1.4 x min(0.25 h + 6, 4 r + 6, 30))
        //                and the same CASCADE as a sheath (1.4 x r + 1 m, 1.08 x (h + 3), optical depth 0.8);
        //   Slap         a BURST column (0.8 x the half extents + 4 m, vertical semi-axis max(h, 1.6 x along + 12 m) x (0.5 +
        //                0.5 strength): 52 m for the K5 slap of half length 25 m; its visible top (4 levels over the no-mist
        //                render) is 0.966 of the semi-axis (mistx-04 KS11 at 62 m: 59.9 m, 537 px at 4K), so 50.2 m = 452 px
        //                at 4K against Addendum A1 K5 445 +- 40; optical depth 2.0; rise 1.2 s, fall 3.4 s).
        // Followers: critically damped, omega 3.2 rad/s up (95 % in 1.48 s, peak rate 0.368 omega = 3.9 % per 30 fps frame)
        // and 1.1 rad/s down (95 % in 4.31 s); the slap 4.0 / 1.4 (1.19 s / 3.39 s, peak 4.9 % per frame); the cascade
        // 3.2 up / 1.0 down (water drains off a body that stopped moving within 4.7 s).
        public const double DistRiseOmega = 3.2, DistFallOmega = 1.1;
        public const double SlapRiseOmega = 4.0, SlapFallOmega = 1.4;
        public const double VeilRiseOmega = 1.6, VeilFallOmega = 0.8;
        public const double CascadeRiseOmega = 3.2, CascadeFallOmega = 1.0;
        public const double BreachColumnTau = 2.5, BreachVeilTau = 0.6, TentacleTau = 1.6, SlapTau = 3.0, TentacleCascadeTau = 0.8;
        public const double BreachColumnScale = 1.15, BreachColumnPad = 6.0;
        /// <summary>Breach foot height: min(0.3 h + 10 m, 30 m) before the dome stretch, x (0.5 + 0.5 strength) (Wave X, show
        /// request: "cap the spray height of Breach and TentacleRise at about min(HeightAboveSea, 30 m) and scale it by
        /// Strength"; with 40 m the K4 foot's semi-axis was 56 m and its upper third veiled the head). K4 at +179 m: 1.4 x 30 =
        /// 42 m at strength 1, 27 m at the show's held strength 0.3; the eyes (A1: +68-76 m, 20 m tall) stay clear.</summary>
        public const double BreachFootTopK = 0.3, BreachFootTopPad = 10.0, BreachFootTopMax = 30.0;
        /// <summary>The cascade off a breaching head never reaches the eyes (show report KK2: "the Breach spray around the head
        /// veils the eye"): its vertical semi-axis is at most BreachCascadeEyeK h + BreachCascadeEyePad (0.35 x 179 + 12 =
        /// 74.7 m for K4: the profile (1 - (66 / 74.7)^2)^2 = 0.047 of the centre at the eyes' lower edge, optical depth under
        /// 0.03 at full activity; nothing above 74.7 m).</summary>
        public const double BreachCascadeEyeK = 0.35, BreachCascadeEyePad = 12.0;
        public static double BreachCascadeTop(double h)
            => Math.Min(BreachVeilTop * (Math.Max(h, 0.0) + BreachTopPad), BreachCascadeEyeK * Math.Max(h, 0.0) + BreachCascadeEyePad);
        /// <summary>Vertical semi-axis of a foot = 1.4 x its visible height: the dome profile (1 - (y / a)^2)^2 keeps 0.24 of
        /// its base density at that height and 0.79 half way up (round 9: with the semi-axis at the height itself the upper
        /// third had under 12 % and vanished).</summary>
        public const double DomeStretch = 1.4;
        /// <summary>Cascade: horizontal 1.05 x the half extents + 3 m (breach), 1.4 x r + 1 m (tentacle); vertical semi-axis
        /// 1.1 x (h + 4) (breach), 1.08 x (h + 3) (tentacle).</summary>
        public const double BreachVeilScale = 1.05, BreachVeilPad = 3.0, BreachVeilTop = 1.1, BreachTopPad = 4.0;
        public const double TentacleCascadeScale = 1.4, TentacleCascadePad = 1.0, TentacleCascadeTop = 1.08, TentacleTopPad = 3.0;
        public const double TentacleScale = 2.2, TentaclePad = 4.0;
        /// <summary>Tentacle foot height: min(0.25 h + 6, 4 r + 6, 30) m before the dome stretch.</summary>
        public const double TentacleFootTopK = 0.25, TentacleFootTopPad = 6.0, TentacleFootTopR = 4.0, TentacleFootTopMax = 30.0;
        /// <summary>Slap burst: along 0.8 x the half length + 4 m (the impact line of the whip), across 0.55 x + 4 m (Wave X, show
        /// report "the K5 slap column is barely visible at KS11": 0.8 across made a 48 m wide dome of a 52 m column; seen
        /// 8 deg off its line from KS11 the burst is now 36 m wide, a column).
        /// The column: vertical semi-axis 2.1 x 25 + 12 = 64.5 m for K5 (was 1.6: 52 m). The show publishes the slap 62 m along
        /// the whip's topple line, (213.7, 991.0), 270 m from KS11 (A1 assumed 209 m): its visible top (0.97 of the semi-axis
        /// by the 4-level rule at optical depth 3) is 62.6 m above the sea (y 54.6), 52.9 m above the eye: 11.1 deg over the
        /// horizon, + 2.1 deg down to the sea at 270 m = 13.1 deg; projected at KS11 (pitch +6, the column 27 deg off the view
        /// axis) at 4K: 1728 x (tan 5.1 + tan 8.1) = 398 px x 1 / cos 27 = 446 px (A1 445 +- 40); its dense part
        /// (optical depth 0.3 and up, (1 - u^2)^2.5 >= 0.1: u 0.77) reaches 50 m (A1 "a 50 m spray column").
        /// Fix wave X2 S3-4: measured in the merged game (mistx-21 KS11 4K, the slap drawn at full weight) the visible top is
        /// only 0.71 of the upper semi-axis (white over the dark K4 head from the waterline row 1280 to row 910: 370 px), so
        /// SlapTopMin is 25 m: V = 2.1 x 25 + 25 = 77.5 m, visible top 0.3 V + 0.71 x 0.7 V = 61.8 m above the sea, ~440 px
        /// at 4K from KS11 (A1 445 +- 40).</summary>
        public const double SlapAlongScale = 0.8, SlapAcrossScale = 0.55, SlapPad = 4.0, SlapTopMin = 25.0, SlapTopAlong = 2.1;
        /// <summary>The slap is an EVENT, not a state (Wave X): the bus strength of the K5 slap decays right after its 0.2 s attack
        /// (KrakenDirectorChoreo.SlapEnvelope: 0.69 at KS11, 0.8 s after the impact), so following it gave a 44 m column at 69 %
        /// density. The burst's density target is smoothstep(0, SlapLatch, strength) (1 down to strength 0.3, i.e. 2.1 s after
        /// the impact; then it falls with the slap follower) and its height is the full SlapTop while the slap is on the bus.</summary>
        public const double SlapLatch = 0.3;
        /// <summary>Slap column shape (Wave X, mistx-12 KS11: centred on the sea the dome's densest part was under the water and
        /// the burst read as a short curling jet): centre at 0.3 V above the sea (V = SlapTop), semi-axis 0.7 V (the part under
        /// the sea is hidden by it; at the waterline (1 - (0.3 / 0.7)^2)^2 = 0.67 of the core density). Visible top (0.97 of the
        /// semi-axis by the 4-level rule) 0.3 V + 0.68 V = 0.98 V = 63.1 m for K5; at 50 m above the sea the axis density is
        /// (1 - (30.6 / 45.2)^2)^2 = 0.29 of the core (the sea-centred dome: 0.16).</summary>
        public const double SlapCentreK = 0.3, SlapUpperK = 0.7;
        /// <summary>White-water share of the slap burst (a fully white column; the other kraken sprays SprayWhite 0.85).</summary>
        public const double SlapWhite = 1.0;
        /// <summary>Downwind lean (horizontal metres per metre of height) of the breach foot, the tentacle foot, the slap (0.08:
        /// x 2.25 on the reef = 10 deg, an upright column; 0.15 (19 deg) still read as a curling jet at KS11 in mistx-11, 0.35
        /// leaned it 38 deg, mistx-02); the cascades
        /// hug the body (falling water follows it, not the wind).</summary>
        public const double BreachLean = 0.25, TentacleLean = 0.3, SlapLean = 0.08, CascadeLean = 0.05;
        /// <summary>Cascade activity: |dh/dt| / 4 m/s (a body rising or sinking at half the 8 m/s cap of the lead decision
        /// sheds water at the full rate), at most 1, times the strength; followed with CascadeRise/FallOmega.</summary>
        public const double CascadeRate = 4.0;
        public static double CascadeActivity(double heightRate, double strength) => Clamp01(Math.Abs(heightRate) / CascadeRate) * Clamp01(strength);
        /// <summary>Height factor of a foot at strength s: 0.5 + 0.5 s (show request: scale the spray height by Strength).</summary>
        public static double FootStrengthScale(double strength) => 0.5 + 0.5 * Clamp01(strength);
        /// <summary>Vertical semi-axis of the breach foot for a body h m above the sea at strength s.</summary>
        public static double BreachFootTop(double h, double strength = 1.0)
            => DomeStretch * Math.Min(BreachFootTopK * Math.Max(h, 0.0) + BreachFootTopPad, BreachFootTopMax) * FootStrengthScale(strength);
        /// <summary>Vertical semi-axis of the tentacle foot for an arm h m tall of waterline radius r at strength s.</summary>
        public static double TentacleFootTop(double h, double r, double strength = 1.0)
            => DomeStretch * Math.Min(Math.Min(TentacleFootTopK * Math.Max(h, 0.0) + TentacleFootTopPad, TentacleFootTopR * r + TentacleFootTopPad), TentacleFootTopMax)
               * FootStrengthScale(strength);
        /// <summary>Vertical semi-axis of a slap burst (strength 1 = the full column; see SlapLatch).</summary>
        public static double SlapTop(double h, double halfAlong, double strength)
            => Math.Max(h, SlapTopAlong * halfAlong + SlapTopMin) * (0.5 + 0.5 * Clamp01(strength));
        /// <summary>Density target of the slap burst for the bus strength (see SlapLatch).</summary>
        public static double SlapDensityTarget(double strength) => strength > 0.0 ? Smooth(strength / SlapLatch) : 0.0;
        /// <summary>Noise scroll (m/s, + up): thrown spray rises, cascading water falls.</summary>
        public const double DistScrollUp = 2.0, DistScrollDown = -5.0;
        /// <summary>Spray-local noise tile: 0.4 x the larger horizontal semi-axis within 16..64 m (an 8 m wall spray keeps
        /// its 16 m tile); vertical tiles 2.2 x taller for cascading water (streaks), 1.4 x for bursts.</summary>
        public const double DistTileScale = 0.4, DistTileMin = 16.0, DistTileMax = 64.0, DistTileStreak = 2.2, DistTileBurst = 1.4;
        /// <summary>Spray slots of the 8-emitter budget (14.7 H2: plume + curtain + 6 sprays).</summary>
        public const int SprayBudget = 6;
        /// <summary>Kraken priority in the slots (Addendum A1 5.2 "wall-spray slots with kraken priority"): a sea-disturbance
        /// spray's score is x8: a key-shot subject 209 m away (the K5 slap, score 0.17) outranks a fresh wall spray 12 m in front of the eye (0.84) in the worst case of six live wall sprays at KS11.</summary>
        public const double KrakenPriority = 8.0;
        /// <summary>The K5 slap column is the key subject of its window (A1 4.9: KS11 445 +- 40 px at bearing 49.5; fix wave X2
        /// S3-4): within SlapPriorityRange of the eye its score is x SlapPriority on top of the kraken priority, so the K4 arm
        /// crossings, the head collar and the near wall sprays never push it into the cut-off fade (x2-show: no column at KS11,
        /// s 824; a 44 m column at a partial weight at s 828). The far Kf slap (1.2 km behind the walker, never seen) keeps the
        /// plain kraken priority, so it takes no wall-spray slot in beat 2.</summary>
        public const double SlapPriority = 50.0, SlapPriorityRange = 600.0;
        /// <summary>Priority factor of a slap burst whose column centre is dist m from the eye.</summary>
        public static double SlapPriorityFor(double dist) => dist <= SlapPriorityRange ? KrakenPriority * SlapPriority : KrakenPriority;
        /// <summary>Cut-off fade: a kept spray whose score is within 30 % of the best excluded one is faded by smoothstep, so a
        /// swap at the budget edge is continuous (both are at weight 0 when their scores cross).</summary>
        public const double CutoffBand = 0.3;
        /// <summary>White water (BwMistVolume.hlsl spray loop, Wave X): the radiance of a thick exterior spray is
        /// lerp(mist, white water lit like the foam, SprayWhite (1 - exp(-tau / SprayWhiteTau))) along each ray; the white
        /// water faces the eye with an up tilt (-d.x, SprayNormalUp, -d.z).</summary>
        public const double SprayWhite = 0.85, SprayWhiteTau = 0.8, SprayNormalUp = 1.2;
        /// <summary>White-water share of the WALL sprays (per slot, _BwMistSprayW.x): 0.3, not the kraken's 0.85. A wall spray is
        /// born 4-10 m from the eye in 0.6 s (WaveImpacts); fully white it lifted the KS11 frame mean from 130 to 161 with
        /// 7.5 levels per frame (mistx-05-seqk5 frames 255-262; 14.10.3 allows 1.2-2.0, 14.2.5 no area > 10 % changing > 15 %
        /// in 0.33 s). At 0.3 a thick wall spray is about 1.1 x the haze behind it: lighter than smoke, no strobe. Kraken
        /// sprays stand 200-1500 m away and change through the 5 %-per-frame followers. Final 0.5 with the near-eye envelope
        /// (SprayDensityFactorNear): at 0.3 the KS04 forced plume (35 m) nearly vanished in the storm haze.</summary>
        public const double SprayWhiteWall = 0.5;
        /// <summary>White-water share of a wall spray by its nearness to the eye (Wave X, mistx-13 KS04: the collision jet 32 m
        /// out read as a faint glass pillar under the storm sky at 0.5): SprayWhite (0.85, the kraken's) for sprays born 30 m
        /// and farther, SprayWhiteWall (0.5) within 10 m (the strobe rule above), smooth between (SprayNearness).</summary>
        public static double SprayWhiteFor(double nearness) => SprayWhite + (SprayWhiteWall - SprayWhite) * Clamp01(nearness);
        /// <summary>Near-eye fade of every spray chord (BwMistVolume.hlsl MistSprayTau): x smoothstep(SprayNear0, SprayNear1, the
        /// chord midpoint distance in m). A wall spray drifting over the road at 9 m/s engulfed the KS11 camera and lifted the
        /// frame mean by 29 levels at up to 6.8 levels per frame (mistx-06-seqk5). Wave X: 3 / 12 m (was 1.5 / 6): the wall
        /// column from the water (WallColumn) carried over the road changed 41 % of the KS11 frame by more than 12 levels at
        /// its peak (mistx-17-seqk5 frames 285-290; 14.5.4 (4): no plume over 25 % of the frame); the forced KS11 plume (21 m,
        /// chord midpoints from 17 m) keeps its full density, a spray at the parapet beside the walker (7.6 m) about half.</summary>
        public const double SprayNear0 = 3.0, SprayNear1 = 12.0;
        /// <summary>Ragged outline of every spray (BwMistVolume.hlsl MistSprayTau, Wave X): the chord's optical depth x
        /// smoothstep(0, SprayErodeSoft, noise - SprayErode + SprayErodeIn sqrt(q)), q = 1 - (closest approach / radius)^2: full on
        /// the axis (noise >= 0 always passes: -0.55 + 0.9 = 0.35), noise peaks only at the silhouette (0.65 / 1.0 erased the KS04
        /// forced plume: 4062 px over 4 levels against 35136 in round 9, mistx-06).</summary>
        public const double SprayErode = 0.55, SprayErodeIn = 0.9, SprayErodeSoft = 0.35;
        /// <summary>Extra erosion toward the top of a spray: - SprayErodeTop x (height over the vertical semi-axis, 0..1).</summary>
        public const double SprayErodeTop = 0.2;

        /// <summary>One step of a critically damped follower toward a constant target (exact solution over dt; any dt).</summary>
        public static void Follow(ref double x, ref double v, double target, double omega, double dt)
        {
            if (!(dt > 0.0)) return;
            if (double.IsInfinity(dt)) { x = target; v = 0.0; return; }
            double e = x - target, c = v + omega * e, ex = Math.Exp(-omega * dt);
            x = target + (e + c * dt) * ex;
            v = (v - omega * c * dt) * ex;
        }
        /// <summary>Follower step with the rise omega when the target is above x, the fall omega otherwise.</summary>
        public static void FollowAsym(ref double x, ref double v, double target, double rise, double fall, double dt)
            => Follow(ref x, ref v, target, target >= x ? rise : fall, dt);
        /// <summary>Peak density of a unit-profile ellipsoid whose chord through the centre along semi-axis a has optical depth tau.</summary>
        public static double RhoForTau(double tau, double a) => Math.Max(tau, 0.0) / (Sigma * (16.0 / 15.0) * Math.Max(a, 0.1));
        /// <summary>Optical depth through the centre along semi-axis a of an ellipsoid of peak density rho.</summary>
        public static double TauForRho(double rho, double a) => Sigma * rho * (16.0 / 15.0) * a;
        /// <summary>Priority of a spray (14.7 H2 budget, Addendum A1 "strength x screen size"): the optical depth through its
        /// centre times its angular size squared (the larger semi-axis over the eye distance, at most 1.5 rad).</summary>
        public static double SprayScore(double tauCentre, double rMax, double dist)
        {
            double a = Math.Min(rMax / Math.Max(dist, 1.0), 1.5);
            return Math.Max(tauCentre, 0.0) * a * a;
        }
        /// <summary>Weight of a kept spray against the best excluded score (1 when no spray is excluded).</summary>
        public static double CutoffWeight(double score, double excluded)
            => excluded <= 0.0 ? 1.0 : (score <= 0.0 ? 0.0 : Smooth((score - excluded) / (CutoffBand * score)));
        /// <summary>Spray-local noise tile (m) for a spray of the given larger horizontal semi-axis.</summary>
        public static double DistTile(double rMax) => Math.Max(DistTileMin, Math.Min(DistTileMax, DistTileScale * rMax));

        // ------------------------------------------------------------------ sun-break gold (fix wave X2 S3-1)
        /// <summary>Sun-break gold window (beat 3 'awe, relief, gold'; x2-stills KS08: the veil inside the bow read khaki,
        /// saturation 0.08): weight 0 -> 1 over s 560-580, 1 -> 0 over 620-640 (_BwMistGold.x). Through it the mist's sun
        /// in-scatter turns toward BreakGold at the same luminance (GoldSun), the sky-dome ambient part of the way (GoldAmb),
        /// and a forward lobe toward the sun is added: GoldLobe x pi x HG(c, GoldLobeG), at most GoldLobeClamp (x the
        /// direct-sun share), so the mist toward the sun glows gold without blowing out.</summary>
        public const double GoldS0 = 560.0, GoldS1 = 640.0, GoldFade = 20.0;
        public const double GoldSun = 1.0, GoldAmb = 1.0, GoldLobe = 0.35, GoldLobeG = 0.75, GoldLobeClamp = 0.75;
        /// <summary>Weight of the sun-break gold at progress s.</summary>
        public static double GoldWeight(double s) => Smooth((s - GoldS0) / GoldFade) * (1.0 - Smooth((s - (GoldS1 - GoldFade)) / GoldFade));

        // ------------------------------------------------------------------ phase functions
        public const double HgG = 0.55;
        /// <summary>Single-scattering albedo of the droplets (MIST_ALBEDO).</summary>
        public const double Albedo = 0.92;
        /// <summary>Henyey-Greenstein phase (1/sr) at the cosine of the scattering angle.</summary>
        public static double Hg(double cosTheta, double g)
        {
            double den = 1.0 + g * g - 2.0 * g * cosTheta;
            return (1.0 - g * g) / (4.0 * Math.PI * Math.Pow(Math.Max(den, 1e-6), 1.5));
        }
        /// <summary>Multiple-scattering octaves (Wrenninge 2013 "Oz"): p(g) + 0.5 p(g / 2) + 0.25 p(g / 4). The single-scatter
        /// HG of 14.7 H2 stays the first octave; the two softer octaves stand in for the light a dense plume scatters more
        /// than once (without them a sunlit plume of optical depth 1 would be 3 times darker than the sky it stands in).</summary>
        public static double PhaseMs(double cosTheta)
            => Hg(cosTheta, HgG) + 0.5 * Hg(cosTheta, 0.5 * HgG) + 0.25 * Hg(cosTheta, 0.25 * HgG);
        /// <summary>Clamp of pi x phase (forward peak toward the sun: keeps sprays against the sun below the bloom clamp).</summary>
        public const double PhaseClamp = 1.5;

        // ------------------------------------------------------------------ rainbow (H2, exactly as specified)
        public static readonly double[] BowPrimary = { 42.0, 41.0, 40.3 };     // R, G, B (deg from the antisolar point)
        public static readonly double[] BowSecondary = { 50.8, 52.0, 53.5 };
        public const double BowWidth = 0.9, BowSecondaryWeight = 0.25;
        public const double DarkBand = 0.6, DarkBandIn = 42.5, DarkBandOut = 50.0, DarkBandEdge = 0.5;
        /// <summary>Clamp of the accumulated bow radiance per pixel (linear HDR, H2 "clamp 2.0").</summary>
        public const double BowClamp = 2.0;
        /// <summary>K: bow phase gain (calibrated for an arch 12-30 levels above its surroundings, see the mist report).</summary>
        public const double BowK = 0.30;

        /// <summary>Bow phase term of channel c (0 R, 1 G, 2 B) at angle theta (deg) from the antisolar direction.</summary>
        public static double Bow(int c, double thetaDeg)
        {
            double p = (thetaDeg - BowPrimary[c]) / BowWidth, q = (thetaDeg - BowSecondary[c]) / BowWidth;
            return Math.Exp(-p * p) + BowSecondaryWeight * Math.Exp(-q * q);
        }
        /// <summary>Alexander's dark band: 0.6 between 42.5 and 50 deg, smooth +-0.5 deg edges, 1 elsewhere.</summary>
        public static double Band(double thetaDeg)
        {
            double a = Smooth((thetaDeg - (DarkBandIn - DarkBandEdge)) / (2.0 * DarkBandEdge));
            double b = Smooth((thetaDeg - (DarkBandOut - DarkBandEdge)) / (2.0 * DarkBandEdge));
            return 1.0 - (1.0 - DarkBand) * a * (1.0 - b);
        }
        /// <summary>Angle (deg) from the antisolar point at which channel c of the primary peaks (the Gaussian centre).</summary>
        public static double PrimaryPeakDeg(int c) => BowPrimary[c];
        /// <summary>Elevation (deg) of the top of the primary for channel c with the sun at elevation sunEl: the arch top of
        /// 14.5.3 is read on the R channel (the red outer edge, "red outermost"): 42.0 - 16.6 = 25.4 deg at s 604.</summary>
        public static double BowTopElevation(double sunElDeg, int c = 0) => BowPrimary[c] - sunElDeg;

        // ------------------------------------------------------------------ the plume's face onset (MIST_ONSET_*)
        /// <summary>The plume starts at x -6 - dx, dx = (OnsetBase + OnsetTop smoothstep(OnsetY0, OnsetY1, y)) n, n in [0, 1]
        /// from the 400 m noise: hugging the face below the crest (at most 12 m off it), leaning off it above (up to 152 m).
        /// Removes the straight edge of the face plane seen edge-on from the crest (wb-mist-05 pose_plume_b4). The horizontal
        /// optical depth along x below the crest loses at most rho0 x 12 m x Sigma = 2 % of the 1.0 peak.</summary>
        public const double OnsetBase = 12.0, OnsetTop = 140.0, OnsetY0 = -60.0, OnsetY1 = 220.0;
        public static double OnsetReach(double y) => OnsetBase + OnsetTop * SmoothStep(OnsetY0, OnsetY1, y);
        /// <summary>Offset of the onset for noise n in [0, 1]: below the crest n x reach (0..12 m), above it the shoulder always
        /// leans off by at least 35 % of the reach (the straight face plane never shows edge-on above the deck).</summary>
        public static double OnsetOffset(double y, double n)
        {
            double r = SmoothStep(OnsetY0, OnsetY1, y), m = Clamp01(n);
            return (OnsetBase + OnsetTop * r) * (m + (0.35 + 0.65 * m - m) * r);
        }

        /// <summary>Share of the storm's hidden key (14.2.3 row 2) that lights the mist: 0, the sky dome carries the deck's
        /// glow (MIST_STORM_KEY); the direct sun of the break (disc visibility) lights it fully.</summary>
        public const double StormKey = 0.0;

        // ------------------------------------------------------------------ the dam's shadow (MIST_DAM_*)
        /// <summary>Shadow edge of the crest (m), its half width at the face (m) and the sun's penumbra (tan 0.267 deg).</summary>
        public const double DamCrestY = 0.5, DamSoft0 = 0.3, SunPenumbra = 0.0047;

        /// <summary>Lit fraction (0..1) of a point west of the lee face for the direct sun along l (unit, toward the sun): the
        /// sun ray from p meets the face plane x -6 at height p.y + l.y (-6 - p.x) / l.x; below the crest edge the dam blocks
        /// it. 1 when the sun is not in the east (l.x below 0.02) or p is east of the face.</summary>
        public static double DamLitAt(D3 p, D3 l)
        {
            if (l.x < 0.02 || p.x >= PlumeXFace) return 1.0;
            double dist = (PlumeXFace - p.x) / l.x;
            double yHit = p.y + l.y * dist - DamCrestY;
            return Clamp01(yHit / (2.0 * (DamSoft0 + SunPenumbra * dist)) + 0.5);
        }

        static double RampInt(double u) => u <= 0.0 ? 0.0 : (u >= 1.0 ? u - 0.5 : 0.5 * u * u);

        /// <summary>Exact average of the lit fraction over the ray segment o + d t, t in [ta, tb] (the shader's MistDamLit):
        /// the crossing height is linear in t, so the average of the clamped ramp is a difference of its integral; the edge
        /// width is taken at the segment midpoint.</summary>
        public static double DamLitSegment(D3 o, D3 d, D3 l, double ta, double tb)
        {
            if (l.x < 0.02) return 1.0;
            double il = 1.0 / l.x;
            double A = o.y + l.y * (PlumeXFace - o.x) * il - DamCrestY, B = d.y - l.y * d.x * il;
            double tm = 0.5 * (ta + tb);
            double dist = Math.Max((PlumeXFace - (o.x + d.x * tm)) * il, 0.0);
            double inv = 1.0 / (2.0 * (DamSoft0 + SunPenumbra * dist));
            double ua = (A + B * ta) * inv + 0.5, ub = (A + B * tb) * inv + 0.5;
            if (Math.Abs(ub - ua) < 1e-4) return Clamp01(0.5 * (ua + ub));
            return Clamp01((RampInt(ub) - RampInt(ua)) / (ub - ua));
        }

        // ------------------------------------------------------------------ fixed sample grid
        /// <summary>Normalised position of sample boundary i (0..Samples) along [t0, t1].</summary>
        public static double SampleEdge(int i) => Math.Pow(Clamp01(i / (double)Samples), SpacingPower);

        // ------------------------------------------------------------------ helpers
        public static double Clamp01(double x) => x < 0.0 ? 0.0 : (x > 1.0 ? 1.0 : x);
        /// <summary>smoothstep(0, 1, x).</summary>
        public static double Smooth(double x) { x = Clamp01(x); return x * x * (3.0 - 2.0 * x); }
        /// <summary>smoothstep(a, b, x).</summary>
        public static double SmoothStep(double a, double b, double x) => Smooth((x - a) / (b - a));
        /// <summary>erf (Winitzki 2008, max error 1.3e-4); the shader uses the same form.</summary>
        public static double Erf(double x)
        {
            const double a = 0.147;
            double x2 = x * x;
            double v = Math.Sqrt(1.0 - Math.Exp(-x2 * (4.0 / Math.PI + a * x2) / (1.0 + a * x2)));
            return x >= 0.0 ? v : -v;
        }
    }
}
