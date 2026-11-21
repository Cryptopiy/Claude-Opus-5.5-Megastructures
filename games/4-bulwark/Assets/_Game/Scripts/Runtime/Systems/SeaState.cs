// SeaState: the exterior sea of beats 1-7 (MASTER_PROMPT 14.7 H1, 14.2.4, 14.3 event coupling, 14.5.2, 14.6 Rule P).
// Owner: the sea owner (Wave B). Two jobs, both on the ambient clock (BwClock) so frozen-clock stills reproduce:
//   1. The wave field. SeaSpectrum builds the 32 components of each sea state of 14.2.4 (beat 2 Hs 2.8 / Tp 9, beat 3
//      2.0 / 8, beat 4 3.2 / 11) exactly per H1: wavenumbers geometric from 0.6 kp to 2 pi / 0.5 m, amplitudes from
//      S(k) = 0.5 k^-3 exp(-(kp/k)^2) (a_i^2 = 2 S(k_i) dk_i) scaled to Hs = 4 sqrt(sum a^2 / 2), omega = sqrt(g k),
//      directions west (270 deg) +-35 deg on a golden-ratio sequence, phases from a PCG hash. Each frame SeaState uploads
//      the active set(s) to BwSeaWaves.hlsl: during the 30 m Rule-P windows (s 405-435 and 680-720) BOTH neighbouring
//      sets are drawn with energy weights (a_i x sqrt(w)), so no component ever changes its k or omega (changing k or
//      omega of a running wave makes its phase race: the swell would visibly speed up across the window). Phases are
//      evaluated in DOUBLE on the CPU relative to an origin O near the camera (snapped to 64 m):
//      Phi_i = k_i d_i . O - omega_i t + phi0_i (mod 2 pi), and the shader adds k_i d_i . (x - O): no float jitter at
//      z 1500 and none after an hour of ambient clock (the float _BwTime wraps at 3600 s; this never uses it).
//      The wall clapotis is the mirror image about x = 6 (14.7 H1: eta(x) + eta(12 - x)): its phases Phi'_i are uploaded
//      too; the shader blends the image out over 150-400 m from the wall.
//   2. Wave impacts (14.3, 14.5.2, 14.7 H2). Each impact is one wave group slamming the sea face: a wall jet at (6, Z)
//      and, 1.0 s later, the jet where the reflected crest meets the next incoming one offshore (X = 6 + 18..42 m; the
//      "collision jet" seen at reflective sea walls, 18-42 m out). Both go on the WaveImpacts bus (this is its only writer); the boom
//      goes once to BwServices.Audio.Impact. Times sit on two jittered lattices (9 s in beats 1-3 and 5-7, 6 s in beat 4,
//      Rule P by a per-impact hash), so the mean intervals are exact and any 60 s holds at least 3 impacts; Z = the
//      camera z when the impact starts + an offset from a golden-ratio sequence over [-20, +48] m (ahead-biased: the
//      walker sees them). Capture stills: the clock is frozen per shot, the alive set is rebuilt from (t, camera), and the
//      forced impacts of 14.5.1 "Captures" are added, anchored to the shot's clock s / 2.5: KS02 a mist breath at z 66,
//      KS04 a plume 10 m high at z 175 (its collision jet 30 m off the wall at x 36, after the wall slam at x 6: a wall jet
//      at z 175 is outside the KS04 frame at every aspect, DEVELOPMENT.md decision), KS11 14 m at z 845 (strength 1.0).
//      SeaState.ForceImpact(z, strength, t0) adds one from any caller.
// Zero allocations per frame (fixed arrays). The C# twin of the surface (SeaSpectrum.Surface) feeds SeaTests.
using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace Megastructure
{
    /// <summary>One component of the wave sum (double precision on the CPU).</summary>
    public struct SeaWave
    {
        public double K;        // wavenumber (rad/m)
        public double DirX;     // unit propagation direction (x, z); west = (-1, 0)
        public double DirZ;
        public double A;        // amplitude (m)
        public double Omega;    // sqrt(g k) (rad/s)
        public double Phase0;   // PCG phase (rad)
        public double Wavelength => 2.0 * Math.PI / K;
    }

    /// <summary>
    /// The 32-component spectrum of 14.7 H1 as pure functions (no UnityEngine state; EditMode tests use them directly).
    /// </summary>
    public static class SeaSpectrum
    {
        public const int N = 32;                                         // BulwarkConst.SeaComponents
        public const double Gravity = 9.81;
        public const double PeakFactor = 0.6;                            // longest wavelength 2 pi / (0.6 kp)
        public const double MinWavelength = BulwarkConst.SeaMinWavelength;   // 0.5 m
        public const double DirCentreDeg = 270.0;                        // waves travel west, toward the dam (sea is east)
        public const double SpreadDeg = BulwarkConst.SeaDirSpreadDeg;    // +-35 deg
        public const double Steepness = BulwarkConst.SeaSteepness;       // Gerstner horizontal displacement Q a (0.6)
        public const double S2Floor = BulwarkConst.SeaSlopeVarFloor;     // 0.008
        public const double WallX = BulwarkConst.SeaFaceX;               // the mirror of the clapotis image wave (x 6)
        public const double ImageFade0 = 150.0, ImageFade1 = 400.0;      // image wave blended out over 150-400 m (H1)
        public const double GoldenRatioFrac = 0.6180339887498949;
        public const uint PhaseSeed = 0x5EA5EEDu;
        /// <summary>Whitecap tail of the Jacobian: J_t = 1 - FoamSigmas x sigma_J (2.05 sigma = 2.0 % of the surface).</summary>
        public const double FoamSigmas = 2.05;
        /// <summary>Whitecaps form on the crests of waves of 3 m and longer (periods over 1.4 s: foam evolves, never flickers);
        /// BwSeaWaves.hlsl BWSEA_FOAM_LMIN.</summary>
        public const double FoamMinWavelength = 3.0;
        /// <summary>Upper end of the whitecap band: the swell (40 m and longer) is not steep; it carries the whitecaps.</summary>
        public const double FoamMaxWavelength = 40.0;

        /// <summary>Slope variance of the components of at least FoamMinWavelength (the whitecap Jacobian's band).</summary>
        public static double FoamBandVariance(SeaWave[] w, int n)
        {
            double s = 0.0;
            for (int i = 0; i < n; i++) if (w[i].Wavelength >= FoamMinWavelength && w[i].Wavelength <= FoamMaxWavelength) { double ak = w[i].A * w[i].K; s += 0.5 * ak * ak; }
            return s;
        }

        /// <summary>Peak wavenumber kp = (2 pi / Tp)^2 / g (deep water).</summary>
        public static double PeakK(double tp) { double w = 2.0 * Math.PI / tp; return w * w / Gravity; }

        /// <summary>Peak wavelength 2 pi / kp (126.5 / 99.9 / 188.9 m for Tp 9 / 8 / 11; 14.7 H1 "126, 100, 189").</summary>
        public static double PeakWavelength(double tp) => 2.0 * Math.PI / PeakK(tp);

        /// <summary>
        /// Fills <paramref name="into"/>[0..n) with the spectrum of (hs, tp), sorted long to short. setIndex selects the
        /// PCG phases (one sequence per sea state). n = 32 is the game; tests use n = 256 as the reference.
        /// </summary>
        public static void Build(double hs, double tp, int n, int setIndex, SeaWave[] into)
        {
            double kp = PeakK(tp);
            double k0 = PeakFactor * kp, k1 = 2.0 * Math.PI / MinWavelength;
            double r = Math.Pow(k1 / k0, 1.0 / (n - 1));
            double band = Math.Sqrt(r) - 1.0 / Math.Sqrt(r);              // dk_i = k_i (sqrt r - 1/sqrt r), geometric bins
            double sumA2 = 0.0;
            for (int i = 0; i < n; i++)
            {
                double k = k0 * Math.Pow(r, i);
                double s = 0.5 * Math.Pow(k, -3.0) * Math.Exp(-(kp / k) * (kp / k));
                double a2 = 2.0 * s * k * band;
                sumA2 += a2;
                // golden-ratio directions (well spread, deterministic) inside the +-35 deg fan; the longest wave exactly west
                double u = Frac(0.5 + i * GoldenRatioFrac);
                double az = (DirCentreDeg + SpreadDeg * (2.0 * u - 1.0)) * Math.PI / 180.0;
                uint h = BwMath.Pcg(PhaseSeed + (uint)(setIndex * 7919) + BwMath.Pcg((uint)i));
                into[i] = new SeaWave
                {
                    K = k,
                    DirX = Math.Sin(az),
                    DirZ = Math.Cos(az),
                    A = Math.Sqrt(a2),
                    Omega = Math.Sqrt(Gravity * k),
                    Phase0 = 2.0 * Math.PI * ((h >> 8) * (1.0 / 16777216.0)),
                };
            }
            double hs0 = 4.0 * Math.Sqrt(0.5 * sumA2);
            double scale = hs0 > 0.0 ? hs / hs0 : 0.0;
            for (int i = 0; i < n; i++) into[i].A *= scale;
        }

        static double Frac(double x) => x - Math.Floor(x);

        /// <summary>Significant height 4 sqrt(sum a^2 / 2) (14.7 H1).</summary>
        public static double Hs(SeaWave[] w, int n)
        {
            double s = 0.0;
            for (int i = 0; i < n; i++) s += w[i].A * w[i].A;
            return 4.0 * Math.Sqrt(0.5 * s);
        }

        /// <summary>Slope variance of the wave sum, sum (a k)^2 / 2 (both axes together).</summary>
        public static double SlopeVariance(SeaWave[] w, int n)
        {
            double s = 0.0;
            for (int i = 0; i < n; i++) { double ak = w[i].A * w[i].K; s += 0.5 * ak * ak; }
            return s;
        }

        /// <summary>Eulerian mean-level drop of the Gerstner sum, Q sum a^2 k / 2 (first order); the shader adds it back.</summary>
        public static double MeanLevelDrop(SeaWave[] w, int n, double q)
        {
            double s = 0.0;
            for (int i = 0; i < n; i++) s += 0.5 * q * w[i].A * w[i].A * w[i].K;
            return s;
        }

        static readonly SeaWave[] s_tmp = new SeaWave[N];

        /// <summary>
        /// Roughness of the fully filtered sea (every component unresolved): s2 = 0.008 + sum (a k)^2 / 2 of the 32-component
        /// sea state (hs, tp). This is the variance the near sea reaches at the 2048 m clipmap edge, so the far-field sea
        /// sphere must use it too (T7 crossover). 0.02135 / 0.01843 / 0.01639 for beats 2 / 3 / 4.
        /// </summary>
        public static double FarVariance(double hs, double tp)
        {
            lock (s_tmp)
            {
                Build(hs, tp, N, 0, s_tmp);
                return S2Floor + SlopeVariance(s_tmp, N);
            }
        }

        /// <summary>Standard deviation of the Gerstner Jacobian, Q sqrt(sum (a k)^2 / 2) (first order; simulated 0.0693 for
        /// beat 2, where the first-order value is 0.0693).</summary>
        public static double JacobianSigma(SeaWave[] w, int n, double q) => q * Math.Sqrt(SlopeVariance(w, n));

        /// <summary>
        /// The C# twin of the surface (BwSeaWaves.hlsl) at the undisplaced point (x0, z0) and ambient time t: Gerstner
        /// displacement (q = steepness) of every component, plus the wall image wave with weight imageW, plus the mean-level
        /// correction. Outputs the displaced offset (dx, dy, dz) relative to (x0, -8 + 0, z0) and the Jacobian of the
        /// horizontal map (area of the displaced surface per unit undisplaced area). Double precision.
        /// </summary>
        public static void Surface(SeaWave[] w, int n, double weightSqrt, double x0, double z0, double t, double q, double imageW,
                                   ref double dx, ref double dy, ref double dz, ref double jxx, ref double jzz, ref double jxz)
        {
            for (int i = 0; i < n; i++)
            {
                SeaWave c = w[i];
                double a = c.A * weightSqrt;
                double th = c.K * (c.DirX * x0 + c.DirZ * z0) - c.Omega * t + c.Phase0;
                double cs = Math.Cos(th), sn = Math.Sin(th);
                dy += a * cs;
                dx -= q * a * c.DirX * sn;
                dz -= q * a * c.DirZ * sn;
                double ak = a * c.K * cs;
                jxx -= q * ak * c.DirX * c.DirX;
                jzz -= q * ak * c.DirZ * c.DirZ;
                jxz -= q * ak * c.DirX * c.DirZ;
                dy += 0.5 * q * a * a * c.K;   // mean-level correction (Eulerian mean of the Gerstner sum = 0 to first order)
                if (imageW > 0.0)
                {
                    // mirror about x = WallX: eta(2 Xw - x, z); its horizontal displacement mirrors too (x flips sign)
                    double a2 = a * imageW;
                    double th2 = c.K * (c.DirX * (2.0 * WallX - x0) + c.DirZ * z0) - c.Omega * t + c.Phase0;
                    double c2 = Math.Cos(th2), s2 = Math.Sin(th2);
                    dy += a2 * c2;
                    dx += q * a2 * c.DirX * s2;
                    dz -= q * a2 * c.DirZ * s2;
                    double ak2 = a2 * c.K * c2;
                    jxx -= q * ak2 * c.DirX * c.DirX;
                    jzz -= q * ak2 * c.DirZ * c.DirZ;
                    jxz += q * ak2 * c.DirX * c.DirZ;
                    dy += 0.5 * q * a2 * a2 * c.K;
                }
            }
        }

        /// <summary>Weight of the wall image wave at world x (1 within 150 m of the wall, smooth to 0 at 400 m).</summary>
        public static double ImageWeight(double x)
        {
            double d = x - WallX;
            if (d < 0.0) d = 0.0;
            return 1.0 - BwMath.Smoothstep(ImageFade0, ImageFade1, d);
        }

        /// <summary>Vertex (ring) weight of a component, 1 - smoothstep(0.17, 0.3, spacing / lambda): a ring resolves wavelengths
        /// above 5.9 spacings, none below 3.3 (H1 had 8 / 4; rebalanced in the X2 fix wave, critic S3-2: the 117-173 m swell
        /// near the spectral peak stays in the 16 m / 32 m rings and stands up as crest silhouettes to 2 km). Twin of
        /// BWSEA_RING_R0 / R1 (BwSeaWaves.hlsl).</summary>
        public const double RingR0 = 0.17, RingR1 = 0.3;
        public static double RingWeight(double spacing, double lambda) => 1.0 - BwMath.Smoothstep(RingR0, RingR1, spacing / lambda);

        /// <summary>Pixel weight of a component for footprint f along its direction, 1 - smoothstep(0.5, 1.0, 3 f / lambda):
        /// whole above 6 px per wavelength, zero at 3 px (H1 had 4 f; X2 S3-2). Twin of BWSEA_PIX_K. The shader projects the
        /// footprint on each component's direction (8:1 anisotropy at most); along the view it is this one.</summary>
        public const double PixelK = 3.0;
        public static double FootprintWeight(double f, double lambda) => 1.0 - BwMath.Smoothstep(0.5, 1.0, PixelK * f / lambda);

        /// <summary>Unresolved slope variance in roughness, s2 = 0.008 + sum (1 - w^2)(a k)^2 / 2 for footprint f (H1).</summary>
        public static double FilteredS2(SeaWave[] w, int n, double f)
        {
            double s = S2Floor;
            for (int i = 0; i < n; i++)
            {
                double ww = FootprintWeight(f, w[i].Wavelength);
                double ak = w[i].A * w[i].K;
                s += (1.0 - ww * ww) * 0.5 * ak * ak;
            }
            return s;
        }

        /// <summary>Resolved RMS slope sqrt(sum w^2 (a k)^2) for footprint f (H1 "resolved slope 0.116 at 30 m ..."; this
        /// spectrum gives 0.163 / 0.075 / 0 at 30 m / 300 m / 1 km, DEVELOPMENT.md).</summary>
        public static double ResolvedSlope(SeaWave[] w, int n, double f)
        {
            double s = 0.0;
            for (int i = 0; i < n; i++)
            {
                double ww = FootprintWeight(f, w[i].Wavelength);
                double ak = w[i].A * w[i].K;
                s += ww * ww * ak * ak;
            }
            return Math.Sqrt(s);
        }

        /// <summary>Pixel footprint along the view (m) on the sea for an eye h above it at horizontal distance d, render
        /// height px and vFOV (deg): d^2 p / h with p the vertical pixel angle (H1: 0.6 m at 100 m, 60 m at 1 km at 4K).</summary>
        public static double FootprintAt(double d, double h, double pixelsHigh, double vfovDeg)
        {
            double p = 2.0 * Math.Tan(0.5 * vfovDeg * Math.PI / 180.0) / pixelsHigh;
            return d * d * p / Math.Max(h, 1e-3);
        }

        /// <summary>Beckmann peak 1 / (pi s2) (H1: at most 39.8 with the 0.008 floor).</summary>
        public static double BeckmannPeak(double s2) => 1.0 / (Math.PI * Math.Max(s2, 1e-9));
    }

    /// <summary>
    /// The sea's per-frame driver (an IWorldUpdater registered by the Beat02 crest builder; it runs for the whole walk:
    /// beats 1-7 hear the booms). Writes the BwSeaWaves.hlsl globals and the WaveImpacts bus.
    /// </summary>
    public sealed class SeaState : IWorldUpdater
    {
        public static SeaState Instance { get; private set; }

        // ---------------------------------------------------------------- spectrum sets (beats 2, 3, 4 of 14.2.4)
        public const int SetCount = 3;
        public const int MaxComponents = 2 * SeaSpectrum.N;              // two sets inside a Rule-P window
        /// <summary>Rule-P windows of the sea state (14.6): 2 -> 3 over s 405-435, 3 -> 4 over s 680-720.</summary>
        public const double Win23S0 = 405.0, Win23S1 = 435.0, Win34S0 = 680.0, Win34S1 = 720.0;
        static readonly double[] s_setHs = { BulwarkConst.SeaHs[1], BulwarkConst.SeaHs[2], BulwarkConst.SeaHs[3] };
        static readonly double[] s_setTp = { BulwarkConst.SeaTp[1], BulwarkConst.SeaTp[2], BulwarkConst.SeaTp[3] };
        readonly SeaWave[][] m_sets = new SeaWave[SetCount][];
        readonly double[] m_setVar = new double[SetCount];
        readonly double[] m_setDrop = new double[SetCount];
        readonly double[] m_setFoamVar = new double[SetCount];

        // ---------------------------------------------------------------- shader globals (BwSeaWaves.hlsl)
        public static readonly int IdK = Shader.PropertyToID("_BwSeaK");
        public static readonly int IdP = Shader.PropertyToID("_BwSeaP");
        public static readonly int IdInfo = Shader.PropertyToID("_BwSeaInfo");
        public static readonly int IdOrigin = Shader.PropertyToID("_BwSeaOrigin");
        public static readonly int IdWall = Shader.PropertyToID("_BwSeaWall");
        public static readonly int IdFoam = Shader.PropertyToID("_BwSeaFoam");
        public static readonly int IdImpact = Shader.PropertyToID("_BwSeaImpact");
        public static readonly int IdSpill = Shader.PropertyToID("_BwSeaSpill");
        /// <summary>The kraken disturbance clock (BwSeaWaves.hlsl _BwSeaDistClock): the ambient clock mod
        /// <see cref="SeaDisturbField.ClockPeriod"/> s, from the double clock (every disturbance period divides it).</summary>
        public static readonly int IdDistClock = Shader.PropertyToID("_BwSeaDistClock");
        readonly Vector4[] m_k = new Vector4[MaxComponents];
        readonly Vector4[] m_p = new Vector4[MaxComponents];
        readonly Vector4[] m_imp = new Vector4[FoamImpacts];
        /// <summary>Origin O of the camera-relative phases: the camera snapped to this grid (m).</summary>
        public const double OriginSnap = 64.0;

        // ---------------------------------------------------------------- impacts
        public const double LatticeCrest = BulwarkConst.ImpactIntervalCrest;   // 9 s (beats 1-3, 5-7)
        public const double LatticeReef = BulwarkConst.ImpactIntervalReef;     // 6 s (beat 4)
        /// <summary>Jitter of a lattice impact time: +-0.35 of the interval (intervals 2.7-15.3 s at 9 s; 3 or more per 60 s).</summary>
        public const double LatticeJitter = 0.35;
        /// <summary>Delay of the offshore collision jet after the wall slam (s).</summary>
        public const double CollisionDelay = 1.0;
        /// <summary>Impact z offsets ahead of the camera: golden-ratio sequence over [Ahead0, Ahead1] (m).</summary>
        public const double Ahead0 = -20.0, Ahead1 = 48.0;
        /// <summary>Offshore distance of the collision jet beyond the wall (m).</summary>
        public const double Collision0 = 18.0, Collision1 = 42.0;
        /// <summary>Beat-4 lattice weight (Rule P at 680-720, back to the crest lattice at the portal 920-950).</summary>
        public static double ReefWeight(double s) => BwMath.Smootherstep(Win34S0, Win34S1, s) * (1.0 - BwMath.Smootherstep(920.0, 950.0, s));

        /// <summary>Foam patches of the impacts (sea shader): the last N impacts, age up to FoamLife.</summary>
        public const int FoamImpacts = 6;
        public const double FoamLife = 9.0;

        readonly WaveImpact[] m_live = new WaveImpact[16];
        int m_liveCount;
        double m_lastT = double.NaN;
        readonly WaveImpact[] m_capForced = new WaveImpact[4];
        int m_capCount;
        double m_loggedS = double.NaN;

        // forced impacts (captures, ForceImpact)
        static readonly WaveImpact[] s_forced = new WaveImpact[8];
        static int s_forcedCount;

        /// <summary>Total impacts started since load (QA log).</summary>
        public int ImpactsStarted { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics() { Instance = null; s_forcedCount = 0; }

        public SeaState()
        {
            for (int i = 0; i < SetCount; i++)
            {
                m_sets[i] = new SeaWave[SeaSpectrum.N];
                SeaSpectrum.Build(s_setHs[i], s_setTp[i], SeaSpectrum.N, i, m_sets[i]);
                m_setVar[i] = SeaSpectrum.SlopeVariance(m_sets[i], SeaSpectrum.N);
                m_setDrop[i] = SeaSpectrum.MeanLevelDrop(m_sets[i], SeaSpectrum.N, SeaSpectrum.Steepness);
                m_setFoamVar[i] = SeaSpectrum.FoamBandVariance(m_sets[i], SeaSpectrum.N);
            }
            Instance = this;
        }

        public SeaWave[] Set(int i) => m_sets[i];

        /// <summary>Energy weights of the three sets at progress s (sum 1; at most two non-zero).</summary>
        public static void SetWeights(double s, out double w2, out double w3, out double w4)
        {
            double a = BwMath.Smootherstep(Win23S0, Win23S1, s), b = BwMath.Smootherstep(Win34S0, Win34S1, s);
            w2 = 1.0 - a;
            w3 = a * (1.0 - b);
            w4 = b;
        }

        /// <summary>Full roughness (s2 floor + slope variance) of the blended sea at s: what the far-field sea must use.</summary>
        public double FarVarianceAt(double s)
        {
            SetWeights(s, out double w2, out double w3, out double w4);
            return SeaSpectrum.S2Floor + w2 * m_setVar[0] + w3 * m_setVar[1] + w4 * m_setVar[2];
        }

        static double[] s_varCache;

        /// <summary>Pure static twin of <see cref="FarVarianceAt"/> (BulwarkSchedule / the far field: _BwSea0.w at s), the same
        /// energy weights as the uploaded wave sets: 0.02135 in beat 2, 0.01843 in beat 3, 0.01639 in beat 4.</summary>
        public static double FarVarianceAtS(double s)
        {
            if (s_varCache == null)
            {
                var v = new double[SetCount];
                for (int i = 0; i < SetCount; i++) v[i] = SeaSpectrum.FarVariance(s_setHs[i], s_setTp[i]) - SeaSpectrum.S2Floor;
                s_varCache = v;
            }
            SetWeights(s, out double w2, out double w3, out double w4);
            return SeaSpectrum.S2Floor + w2 * s_varCache[0] + w3 * s_varCache[1] + w4 * s_varCache[2];
        }

        // ================================================================ per frame
        public void OnFrame(in FrameState f)
        {
            double t = f.ambientTime;
            UploadWaves(f.s, t, f.cameraPos);
            UpdateImpacts(f, t);
        }

        /// <summary>Writes the BwSeaWaves.hlsl globals for progress s, ambient time t and a camera at cam.</summary>
        public void UploadWaves(double s, double t, Vector3 cam)
        {
            SetWeights(s, out double w2, out double w3, out double w4);
            int nA = 0, n = 0;
            double var = SeaSpectrum.S2Floor, drop = 0.0, foamVar = 0.0;
            double ox = Math.Round(cam.x / OriginSnap) * OriginSnap, oz = Math.Round(cam.z / OriginSnap) * OriginSnap;
            // set A = the lower-index set with weight, set B = the next one (only inside a window)
            for (int set = 0; set < SetCount; set++)
            {
                double w = set == 0 ? w2 : (set == 1 ? w3 : w4);
                if (w <= 1e-6) continue;
                if (n >= MaxComponents) break;
                double ws = Math.Sqrt(w);
                SeaWave[] src = m_sets[set];
                for (int i = 0; i < SeaSpectrum.N; i++)
                {
                    SeaWave c = src[i];
                    double a = c.A * ws;
                    double kx = c.K * c.DirX, kz = c.K * c.DirZ;
                    double ph = kx * ox + kz * oz - c.Omega * t + c.Phase0;
                    double phImg = -kx * ox + kz * oz + 2.0 * kx * SeaSpectrum.WallX - c.Omega * t + c.Phase0;
                    m_k[n] = new Vector4((float)kx, (float)kz, (float)c.K, (float)a);
                    m_p[n] = new Vector4((float)Wrap2Pi(ph), (float)Wrap2Pi(phImg), (float)(a * c.K), (float)c.Wavelength);
                    n++;
                }
                if (nA == 0) nA = n;
                var += w * m_setVar[set];
                drop += w * m_setDrop[set];
                foamVar += w * m_setFoamVar[set];
            }
            for (int i = n; i < MaxComponents; i++) { m_k[i] = Vector4.zero; m_p[i] = new Vector4(0f, 0f, 0f, 1f); }
            double sigmaJ = SeaSpectrum.Steepness * Math.Sqrt(Math.Max(foamVar, 0.0));   // the whitecap band (3-40 m)
            double jt = 1.0 - SeaSpectrum.FoamSigmas * sigmaJ;
            Shader.SetGlobalVectorArray(IdK, m_k);
            Shader.SetGlobalVectorArray(IdP, m_p);
            Shader.SetGlobalVector(IdInfo, new Vector4(nA, n, (float)SeaSpectrum.Steepness, (float)jt));
            Shader.SetGlobalVector(IdOrigin, new Vector4((float)ox, (float)BulwarkConst.SeaY, (float)oz, (float)SeaSpectrum.S2Floor));
            Shader.SetGlobalVector(IdWall, new Vector4((float)SeaSpectrum.WallX, (float)SeaSpectrum.ImageFade0, (float)SeaSpectrum.ImageFade1, (float)var));
            // foam streak drift: the wind (from the east) carries old foam west at 0.35 m/s, in double; the value-noise
            // lattices are not periodic, so the offset only wraps after 65536 m (52 hours of ambient clock; float spacing
            // there 4 mm, far below the 0.9 m finest foam cell)
            double drift = 0.35 * t;
            drift -= 65536.0 * Math.Floor(drift / 65536.0);
            Shader.SetGlobalVector(IdFoam, new Vector4((float)sigmaJ, (float)drift, (float)SeaSpectrum.FoamSigmas, 1f));
            Shader.SetGlobalVector(IdSpill, new Vector4((float)BulwarkConst.SpillwayZ0, (float)BulwarkConst.SpillwayZ1, (float)BulwarkConst.PierEastX, (float)drop));
            Shader.SetGlobalFloat(IdDistClock, (float)SeaDisturbField.Clock(t));
        }

        static double Wrap2Pi(double x) { const double tp = 2.0 * Math.PI; return x - tp * Math.Floor(x / tp); }

        // ================================================================ impacts
        /// <summary>Adds a forced impact (captures, tests): a wall jet at (x, z) peaking 0.6 s after t0 (ambient clock).</summary>
        public static void ForceImpact(double z, double strength, double t0, double x = BulwarkConst.SeaFaceX, double height = double.NaN)
        {
            if (s_forcedCount >= s_forced.Length) { for (int i = 1; i < s_forced.Length; i++) s_forced[i - 1] = s_forced[i]; s_forcedCount--; }
            s_forced[s_forcedCount++] = new WaveImpact
            {
                Z = (float)z, X = (float)x, Strength = (float)BwMath.Clamp01(strength), T0 = t0,
                Height = (float)(double.IsNaN(height) ? HeightFor(strength) : height),
            };
        }

        public static void ClearForced() { s_forcedCount = 0; }

        /// <summary>Wall-jet height above the crest for an impact strength (0.3 -> 6 m, 1.0 -> 14 m; 14.5.2 "plumes 6-14 m").</summary>
        public static double HeightFor(double strength)
            => BulwarkConst.PlumeHeightMin + (BulwarkConst.PlumeHeightMax - BulwarkConst.PlumeHeightMin)
               * BwMath.Clamp01((strength - BulwarkConst.ImpactStrengthMin) / (BulwarkConst.ImpactStrengthMax - BulwarkConst.ImpactStrengthMin));

        /// <summary>
        /// Lattice impact n of a lattice (interval T, salt): start time, strength and the golden-ratio offset index. Pure.
        /// </summary>
        public static void LatticeImpact(double interval, uint salt, long n, out double t0, out double strength, out double member,
                                         out double ahead, out double offshore)
        {
            uint h0 = BwMath.Pcg((uint)n ^ salt), h1 = BwMath.Pcg(h0 + 0x9E37u), h2 = BwMath.Pcg(h1 + 0x7F4Au), h3 = BwMath.Pcg(h2 + 0x2545u);
            double u0 = (h0 >> 8) * (1.0 / 16777216.0), u1 = (h1 >> 8) * (1.0 / 16777216.0);
            double u2 = (h2 >> 8) * (1.0 / 16777216.0), u3 = (h3 >> 8) * (1.0 / 16777216.0);
            t0 = interval * (n + LatticeJitter * (2.0 * u0 - 1.0));
            strength = BulwarkConst.ImpactStrengthMin + (BulwarkConst.ImpactStrengthMax - BulwarkConst.ImpactStrengthMin) * u1 * (0.55 + 0.45 * u1);
            member = u2;
            // golden-ratio (low-discrepancy) offsets: consecutive impacts cover [Ahead0, Ahead1] evenly
            double g = (0.37 + n * SeaSpectrum.GoldenRatioFrac + (salt & 0xFF) * 0.0137);
            g -= Math.Floor(g);
            ahead = Ahead0 + (Ahead1 - Ahead0) * g;
            offshore = Collision0 + (Collision1 - Collision0) * u3;
        }

        /// <summary>True when lattice impact n of the crest (A) / reef (B) lattice exists at progress s (Rule-P membership).</summary>
        public static bool LatticeMember(bool reefLattice, double member, double s)
        {
            double w = ReefWeight(s);
            return reefLattice ? member < w : member >= w;
        }

        const uint SaltA = 0xC0A57u, SaltB = 0x4EEF5u;

        void UpdateImpacts(in FrameState f, double t)
        {
            bool jump = double.IsNaN(m_lastT) || t < m_lastT - 1e-6 || t - m_lastT > 1.0;
            double zCam = f.cameraPos.z;
            if (jump)
            {
                // rebuild the alive set from (t, camera): impacts that started within the foam life + the collision delay
                m_liveCount = 0;
                double ta = t - CollisionDelay - FoamLife;
                ScanLattice(LatticeCrest, SaltA, false, ta, t, f.s, zCam, false);
                ScanLattice(LatticeReef, SaltB, true, ta, t, f.s, zCam, false);
            }
            else if (t > m_lastT)
            {
                ScanLattice(LatticeCrest, SaltA, false, m_lastT, t, f.s, zCam, !f.capture);
                ScanLattice(LatticeReef, SaltB, true, m_lastT, t, f.s, zCam, !f.capture);
            }
            m_lastT = t;
            m_capCount = 0;
            if (f.capture) ForceCaptureImpacts(f, t);
            if (f.capture && !(Math.Abs(f.s - m_loggedS) <= 1e-6))
            {
                m_loggedS = f.s;
                Debug.Log($"[BW] sea: capture s={f.s:F2} t={t:F2}: {m_liveCount} lattice impacts alive, {m_capCount} forced");
            }

            // drop old ones (foam keeps them FoamLife s), publish the plume-visible ones on the bus. Order: the capture
            // impacts first, then the forced ones, then the lattice (oldest first): a reader that keys anything to the bus
            // index (the mist's spray noise) sees the capture plume of a key shot keep index 0 while lattice impacts come
            // and go during a static sequence.
            int w = 0;
            for (int i = 0; i < m_liveCount; i++)
                if (t - m_live[i].T0 <= FoamLife + CollisionDelay) m_live[w++] = m_live[i];
            m_liveCount = w;
            WaveImpacts.Clear();
            for (int i = 0; i < m_capCount; i++)
                if (t - m_capForced[i].T0 <= WaveImpacts.Lifetime) WaveImpacts.Publish(m_capForced[i]);
            for (int i = 0; i < s_forcedCount; i++)
                if (t - s_forced[i].T0 <= WaveImpacts.Lifetime && s_forced[i].T0 - t < 5.0) WaveImpacts.Publish(s_forced[i]);
            // the bus holds WaveImpacts.Max: when the lattice would overflow it, its oldest entries give way (Publish would
            // otherwise drop index 0, the capture plume)
            int eligible = 0;
            for (int i = 0; i < m_liveCount; i++)
                if (t - m_live[i].T0 <= WaveImpacts.Lifetime) eligible++;
            int skip = Math.Max(0, eligible - (WaveImpacts.Max - WaveImpacts.Count));
            for (int i = 0; i < m_liveCount; i++)
            {
                if (t - m_live[i].T0 > WaveImpacts.Lifetime) continue;
                if (skip > 0) { skip--; continue; }
                WaveImpacts.Publish(m_live[i]);
            }
            UploadImpactFoam(t);
        }

        /// <summary>Starts every lattice impact with t0 in (ta, tb] (the windows of consecutive frames are disjoint, so each
        /// impact starts once); boom = play the sound (play mode only, never on a rebuild).</summary>
        void ScanLattice(double interval, uint salt, bool reef, double ta, double tb, double s, double zCam, bool boom)
        {
            long n0 = (long)Math.Floor(ta / interval) - 1, n1 = (long)Math.Ceiling(tb / interval) + 1;
            for (long n = n0; n <= n1; n++)
            {
                LatticeImpact(interval, salt, n, out double t0, out double strength, out double member, out double ahead, out double offshore);
                if (t0 <= ta || t0 > tb) continue;
                if (!LatticeMember(reef, member, s)) continue;
                Start(t0, strength, zCam + ahead, offshore, boom);
            }
        }

        void Start(double t0, double strength, double z, double offshore, bool boom)
        {
            double h = HeightFor(strength);
            Add(new WaveImpact { Z = (float)z, X = (float)BulwarkConst.SeaFaceX, Strength = (float)strength, T0 = t0, Height = (float)h });
            // the collision jet offshore, 1.0 s later, lower (0.7 x) and slightly ahead along the wall
            Add(new WaveImpact { Z = (float)(z + 2.0), X = (float)(BulwarkConst.SeaFaceX + offshore), Strength = (float)(0.8 * strength), T0 = t0 + CollisionDelay, Height = (float)(0.7 * h) });
            ImpactsStarted++;
            if (boom)
            {
                var audio = BwServices.Audio;
                if (audio != null)
                {
                    float pan = PanFor(z);
                    try { audio.Impact((float)strength, pan, Audio.BulwarkScore.ImpactSubOnly(GameState.S)); }
                    catch (Exception e) { Debug.LogWarning("[BW] sea: audio impact failed: " + e.Message); }
                }
            }
        }

        void Add(in WaveImpact w)
        {
            if (m_liveCount == m_live.Length) { for (int i = 1; i < m_live.Length; i++) m_live[i - 1] = m_live[i]; m_liveCount--; }
            m_live[m_liveCount++] = w;
        }

        /// <summary>Stereo pan of an impact at the sea face (x 6, z) for the current camera: +1 = right.</summary>
        static float PanFor(double z)
        {
            Camera cam = GameState.MainCamera;
            if (cam == null) return 0.3f;
            Vector3 d = new Vector3((float)BulwarkConst.SeaFaceX, 0f, (float)z) - cam.transform.position;
            d.y = 0f;
            float m = d.magnitude;
            if (m < 1e-3f) return 0f;
            float right = Vector3.Dot(cam.transform.right, d / m);
            return Mathf.Clamp(right, -0.8f, 0.8f);
        }

        // ---------------------------------------------------------------- the forced impacts of 14.5.1 "Captures"
        /// <summary>Forced capture impacts (key shot, z, x, strength, height, age at the shot's clock): KS02 mist breath, KS04
        /// plume (the collision jet offshore and the wall slam 1 s before it, see the header), KS11 the 14 m plume.</summary>
        static readonly (int shot, double z, double x, double strength, double height, double age)[] s_captureForced =
        {
            (1, 66.0, BulwarkConst.SeaFaceX, 0.4, double.NaN, 0.8),
            (3, 175.0, BulwarkConst.SeaFaceX + KS04PlumeOffshore, 1.0, KS04PlumeHeight, CapturePlumeAge),
            (3, 175.0, BulwarkConst.SeaFaceX, 1.0, double.NaN, CapturePlumeAge + CollisionDelay),
            (10, 845.0, BulwarkConst.SeaFaceX, 1.0, BulwarkConst.PlumeHeightMax, CapturePlumeAge),
        };
        /// <summary>
        /// KS04 forced plume (decision, DEVELOPMENT.md row requested): the collision jet 30 m off the wall at z 175 (x 36), peak
        /// height 10 m above the crest, shown 0.9 s after its start. A wall jet at z 175 is outside the KS04 frame at every
        /// aspect (at the sea face it lies 73 deg left of the view axis; inside at 16:10 needs x >= 15.4); at x 24 (the first
        /// choice, 18 m off) the spray's box leaves the frame top once it is taller than 8 m. At x 36 the spray box of
        /// Rendering.MistVolumeMath.SprayPose at age 0.9 (rise 0.9 s: centre (35.6, 4.0, 175), semi-axes 4.6 x 6.9 x 4.6, top
        /// 10.9 m) projects to |ndc| <= 0.8 at 16:9, 16:10 and 21:9 (margin 0.94; SeaTests checks the box corners).
        /// </summary>
        public const double KS04PlumeOffshore = 30.0, KS04PlumeHeight = 10.0;
        /// <summary>Age of a forced capture plume at its key shot's clock (s): the burst has reached its height (the mist
        /// owner's SprayRise 0.9 s) while its white-water core is still dense (SprayCoreBoost fades over 0.6-1.4 s).</summary>
        public const double CapturePlumeAge = 0.9;

        /// <summary>World box of the KS04 forced plume at the KS04 still (the spray ellipsoid's bounds above the crest road,
        /// y >= 0): the subject the visibility audit and the capture lint check (VisibilityAudit "plume_z175").</summary>
        public static void KS04PlumeBounds(out Vector3 min, out Vector3 max)
        {
            double x = BulwarkConst.SeaFaceX + KS04PlumeOffshore;
            bool reef = 175.0 >= BulwarkConst.SpillwayZ1;
            Rendering.MistVolumeMath.SprayPose(x, 175.0, KS04PlumeHeight, CapturePlumeAge, reef ? Rendering.MistVolumeMath.SprayDriftReef : Rendering.MistVolumeMath.SprayDriftStorm,
                                     out double cx, out double cy, out double cz, out double grow);
            double ax = Rendering.MistVolumeMath.SprayAx * grow, ay = Rendering.MistVolumeMath.SprayAy * grow, az = Rendering.MistVolumeMath.SprayAz * grow;   // strength 1: size 1
            min = new Vector3((float)(cx - ax), (float)Math.Max(cy - ay, 0.0), (float)(cz - az));
            max = new Vector3((float)(cx + ax), (float)(cy + ay), (float)(cz + az));
        }

        void ForceCaptureImpacts(in FrameState f, double t)
        {
            var shots = BulwarkConst.KeyShots;
            for (int i = 0; i < s_captureForced.Length; i++)
            {
                var c = s_captureForced[i];
                if (c.shot >= shots.Length) continue;
                KeyShot k = shots[c.shot];
                if (Math.Abs(f.s - k.s) > 0.5) continue;
                Vector3 e = f.cameraPos;
                double dx = e.x - k.x, dy = e.y - k.y, dz = e.z - k.z;
                if (dx * dx + dy * dy + dz * dz > 0.25) continue;
                if (m_capCount >= m_capForced.Length) break;
                // anchored to the shot's clock (the progress branch s / 2.5 m/s the harness sets for the still): the still
                // shows the plume at its capture age, and a sequence that starts there with the clock running sees it rise and
                // fade like any impact (no frozen plume in a moving sea)
                double tShot = k.s / BulwarkConst.Cine;
                m_capForced[m_capCount++] = new WaveImpact
                {
                    Z = (float)c.z, X = (float)c.x, Strength = (float)c.strength, T0 = tShot - c.age,
                    Height = (float)(double.IsNaN(c.height) ? HeightFor(c.strength) : c.height),
                };
            }
        }

        void UploadImpactFoam(double t)
        {
            int n = 0;
            for (int i = m_liveCount - 1; i >= 0 && n < FoamImpacts; i--)
            {
                double age = t - m_live[i].T0;
                if (age < 0.0 || age > FoamLife) continue;
                m_imp[n++] = new Vector4(m_live[i].X, m_live[i].Z, (float)age, m_live[i].Strength);
            }
            for (int i = 0; i < m_capCount && n < FoamImpacts; i++)
                m_imp[n++] = new Vector4(m_capForced[i].X, m_capForced[i].Z, (float)(t - m_capForced[i].T0), m_capForced[i].Strength);
            for (int i = 0; i < s_forcedCount && n < FoamImpacts; i++)
            {
                double age = t - s_forced[i].T0;
                if (age < 0.0 || age > FoamLife) continue;
                m_imp[n++] = new Vector4(s_forced[i].X, s_forced[i].Z, (float)age, s_forced[i].Strength);
            }
            for (int i = n; i < FoamImpacts; i++) m_imp[i] = new Vector4(0f, -1e6f, 1e3f, 0f);
            Shader.SetGlobalVectorArray(IdImpact, m_imp);
        }

        /// <summary>
        /// The plumes a static camera at the KS04 pose would see over [t0, t0 + seconds] (pure; SeaTests "3 plumes in 60 s").
        /// Counts collision jets and wall jets whose 8 x 12 x 8 m spray centre lies inside the 16:10 frame with a 3 % margin
        /// while it lives. The camera z drives the offsets (static camera: constant).
        /// </summary>
        public static int CountPlumesInFrame(double t0, double seconds, double s, double camX, double camY, double camZ,
                                             double yawDeg, double pitchDeg, double vfovDeg, double aspect)
        {
            int count = 0;
            for (int lat = 0; lat < 2; lat++)
            {
                double interval = lat == 0 ? LatticeCrest : LatticeReef;
                uint salt = lat == 0 ? SaltA : SaltB;
                long n0 = (long)Math.Floor(t0 / interval) - 1, n1 = (long)Math.Ceiling((t0 + seconds) / interval) + 1;
                for (long n = n0; n <= n1; n++)
                {
                    LatticeImpact(interval, salt, n, out double ti, out double strength, out double member, out double ahead, out double offshore);
                    if (ti < t0 || ti > t0 + seconds) continue;
                    if (!LatticeMember(lat == 1, member, s)) continue;
                    double z = camZ + ahead, h = HeightFor(strength);
                    bool seen = InFrame(BulwarkConst.SeaFaceX, 0.5 * h, z, camX, camY, camZ, yawDeg, pitchDeg, vfovDeg, aspect)
                             || InFrame(BulwarkConst.SeaFaceX + offshore, 0.35 * h, z + 2.0, camX, camY, camZ, yawDeg, pitchDeg, vfovDeg, aspect);
                    if (seen) count++;
                }
            }
            return count;
        }

        /// <summary>True when the world point lies in front of the camera inside the frame with a 3 % margin (each side).</summary>
        public static bool InFrame(double x, double y, double z, double camX, double camY, double camZ, double yawDeg, double pitchDeg,
                                   double vfovDeg, double aspect)
        {
            double yaw = yawDeg * Math.PI / 180.0, pitch = pitchDeg * Math.PI / 180.0;
            double fx = Math.Sin(yaw) * Math.Cos(pitch), fy = Math.Sin(pitch), fz = Math.Cos(yaw) * Math.Cos(pitch);
            double rx = Math.Cos(yaw), rz = -Math.Sin(yaw);
            double ux = -Math.Sin(yaw) * Math.Sin(pitch), uy = Math.Cos(pitch), uz = -Math.Cos(yaw) * Math.Sin(pitch);
            double dx = x - camX, dy = y - camY, dz = z - camZ;
            double cz = dx * fx + dy * fy + dz * fz;
            if (cz <= 0.1) return false;
            double tv = Math.Tan(0.5 * vfovDeg * Math.PI / 180.0);
            double nx = (dx * rx + dz * rz) / (cz * tv * aspect), ny = (dx * ux + dy * uy + dz * uz) / (cz * tv);
            return Math.Abs(nx) <= 0.94 && Math.Abs(ny) <= 0.94;
        }

        // ---------------------------------------------------------------- per-camera origin (capture cameras too)
        /// <summary>Re-uploads the phases for a camera that renders now (beginCameraRendering), so the origin always sits
        /// near the rendering camera. Cheap: 64 components.</summary>
        public static void OnBeginCamera(ScriptableRenderContext ctx, Camera cam)
        {
            var s = Instance;
            if (s == null || cam == null || cam.cameraType == CameraType.Preview) return;
            s.UploadWaves(ProgressDirector.S, BwClock.Ambient, cam.transform.position);
        }
    }
}
