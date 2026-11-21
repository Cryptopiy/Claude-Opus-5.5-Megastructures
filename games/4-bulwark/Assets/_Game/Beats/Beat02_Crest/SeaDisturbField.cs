// SeaDisturbField: the C# twin (double precision, pure) of the kraken disturbances of BwSeaWaves.hlsl (BwSeaDisturbAt,
// BwSeaDisturbHeight; Addendum A1). Owner: the sea owner. SeaTests check its properties (support radii, the Slap ring
// radius, the Kelvin angle of the Wake, the footprint filter's energy, clock periods dividing the 600 s wrap, no foam
// over a Shadow); SeaState uploads the clock it defines. Keep every formula in step with the HLSL.
using System;

namespace Megastructure
{
    public static class SeaDisturbField
    {
        /// <summary>The disturbance clock wraps every 600 s (BwSeaWaves.hlsl _BwSeaDistClock); every period below divides it.</summary>
        public const double ClockPeriod = 600.0;
        /// <summary>Breach ring waves: speed (m/s) and emission period (s); TentacleRise rings; Shadow tendril writhing.</summary>
        public const double BreachRingSpeed = 4.5, BreachRingPeriod = 2.5;
        public const double TentacleRingSpeed = 3.0, TentacleRingPeriod = 1.5;
        public const double TendrilPeriod = 6.0;
        /// <summary>The Shadow mantle's outline heaves with this period; the Breach waterline's 5th harmonic turns once per 30 s.</summary>
        public const double MantlePeriod = 12.0, WaterlinePeriod = 30.0;
        /// <summary>Kelvin wedge half-angle tan(19.47 deg) (deep-water ship waves).</summary>
        public const double KelvinTan = 0.353553;
        /// <summary>Variance of the pixel-footprint kernel per fq^2 (BWSEA_DIST_FILTER).</summary>
        public const double Filter = 0.1;

        /// <summary>The value uploaded as _BwSeaDistClock: the ambient clock t mod 600 s (from double).</summary>
        public static double Clock(double t) => t - ClockPeriod * Math.Floor(t / ClockPeriod);

        public struct Sample
        {
            public double Dark, Calm, Foam, White, Height;
        }

        static double Sat(double x) => x < 0.0 ? 0.0 : (x > 1.0 ? 1.0 : x);
        static double Smooth(double a, double b, double x) { double t = Sat((x - a) / (b - a)); return t * t * (3.0 - 2.0 * t); }
        static double Frac(double x) => x - Math.Floor(x);

        /// <summary>Energy-preserving footprint filter of exp(-x^2 / (2 sigma^2)) (BwSeaGaussF).</summary>
        public static double GaussF(double x, double sigma, double fq)
        {
            double s2 = Math.Max(sigma * sigma + Filter * fq * fq, 1e-8);
            return sigma / Math.Sqrt(s2) * Math.Exp(-0.5 * x * x / s2);
        }

        /// <summary>Gaussian crests of a ring train leaving its source (BwSeaRingTrain).</summary>
        public static double RingTrain(double e, double c, double P, double sigma, double fq, double clock)
        {
            double lam = c * P;
            double ph = (e - c * clock) / lam;
            double dq = (ph - Math.Round(ph, MidpointRounding.AwayFromZero)) * lam;
            double g = GaussF(dq, sigma, fq);
            double mean = sigma * 2.5066283 / lam;
            double k = Smooth(0.25, 0.6, fq / lam);
            return g + (mean - g) * k;
        }

        /// <summary>Support radius (m) of an entry (BwSeaDistSupport): every field fades to 0 over its last 20 %.</summary>
        public static double Support(SeaDisturbanceKind kind, double A, double B)
        {
            double Bm = Math.Sqrt(A * B);
            switch (kind)
            {
                case SeaDisturbanceKind.Shadow: return 3.4 * A + 1.2 * B;
                case SeaDisturbanceKind.Breach: return 1.25 * Math.Max(A, B) * (1.0 + 6.0 * (1.5 * Bm + 20.0) / Bm);
                case SeaDisturbanceKind.TentacleRise: return 4.0 * (18.0 * A + 25.0) + 6.0 * A + 30.0;
                case SeaDisturbanceKind.Slap: return 6.0 * A + 30.0;
                default: return 1.6 * A + 6.0 * (10.0 * A + 40.0) + 3.0 * B;
            }
        }

        /// <summary>Slap: the foam ring's radius for the envelope strength S (the ring runs out as S decays).</summary>
        public static double SlapRingRadius(double A, double S) => A * (1.0 + 4.0 * (1.0 - Sat(S)));

        static void Mantle(double u, double v, double A, double B, double fq, double clock, out double dark, out double slick, out double swell)
        {
            double su = 0.5 * A, sv = 0.55 * B, uc = 0.25 * A;
            double su2 = su * su + Filter * fq * fq, sv2 = sv * sv + Filter * fq * fq;
            double amp = su * sv / Math.Sqrt(su2 * sv2);
            double du = u - uc;
            swell = amp * Math.Exp(-0.5 * (du * du / su2 + v * v / sv2));
            double ca = 0.62 * A, cb = 0.62 * B;
            double qa = du / ca, qb = v / cb;
            double th = Math.Atan2(qb, qa);
            double wob = 1.0 + 0.08 * Math.Sin(3.0 * th + 0.7) + 0.05 * Math.Sin(5.0 * th + 2.1 + 2.0 * Math.PI * clock / MantlePeriod);
            double q = Math.Sqrt(qa * qa + qb * qb) / wob;
            double kq = 0.32 * Math.Max(fq / ca, fq / cb);
            double body = 1.0 - Smooth(0.45 - kq, 1.0 + kq, q);
            slick = 1.0 - Smooth(0.5 - kq, 1.25 + kq, q);
            double tend = 0.0;
            for (int j = 0; j < 8; j++)
            {
                double len = A * (1.4 + 1.4 * Frac(0.618034 * j + 0.31));
                double t = (-0.2 * A - u) / len;
                if (t <= -0.05 || t >= 1.0) continue;
                double tc = Sat(t);
                double vj = B * 0.8 * (-0.77 + 0.22 * j);
                double vc = vj * (1.0 + 0.8 * tc) + 0.22 * B * tc * Math.Sin(2.0 * Math.PI * (1.3 * tc - clock / TendrilPeriod + 0.37 * j));
                double w = B * (0.16 * (1.0 - 0.7 * tc)) + 0.3;
                double env = Smooth(-0.05, 0.08, t) * Math.Sqrt(Math.Max(1.0 - tc, 0.0));
                tend = Math.Max(tend, env * GaussF(v - vc, w, fq));
            }
            dark = Math.Max(body, 0.9 * tend);
        }

        /// <summary>
        /// The pixel-stage field (BwSeaDisturbAt) at world (x, z) for an isotropic footprint fq (m per pixel along every
        /// direction: dP0 = (fq, 0), dP1 = (0, fq); 0 = a point sample).
        /// </summary>
        public static Sample At(double x, double z, SeaDisturbance[] items, int count, double clock, double fq)
        {
            var o = new Sample();
            for (int i = 0; i < count && i < SeaDisturbances.Max; i++)
            {
                SeaDisturbance it = items[i];
                double S = Sat(it.Strength);
                if (S <= 1e-4) continue;
                double A = Math.Max(it.HalfAlong, 0.5), B = Math.Max(it.HalfAcross, 0.5);
                double dx = x - it.X, dz = z - it.Z;
                double fqAll = 2.0 * fq;
                double sup = Support(it.Kind, A, B) + 1.2 * fqAll;
                double r2 = dx * dx + dz * dz;
                if (r2 > sup * sup) continue;
                S *= 1.0 - Smooth(0.8 * sup, sup, Math.Sqrt(r2));
                double fwx = Math.Sin(it.Heading), fwz = Math.Cos(it.Heading);
                double rtx = fwz, rtz = -fwx;
                double u = dx * fwx + dz * fwz, v = dx * rtx + dz * rtz;
                // isotropic footprint: |g . (fq, 0)| + |g . (0, fq)| = fq (|gx| + |gz|)
                double fqU = fq * (Math.Abs(fwx) + Math.Abs(fwz)) + 1e-4, fqV = fq * (Math.Abs(rtx) + Math.Abs(rtz)) + 1e-4;
                double Bm = Math.Sqrt(A * B);
                switch (it.Kind)
                {
                    case SeaDisturbanceKind.Shadow:
                    case SeaDisturbanceKind.Wake:
                    {
                        Mantle(u, v, A, B, Math.Max(fqU, fqV), clock, out double dk, out double sl, out double sw);
                        double k = it.Kind == SeaDisturbanceKind.Shadow ? 1.0 : 0.55;
                        double hS = Math.Min(Math.Max(0.03 * B, 0.2), 0.8) * S * k;
                        o.Dark = Math.Max(o.Dark, 0.92 * S * k * dk);
                        o.Calm = Math.Max(o.Calm, 0.85 * S * k * sl);
                        o.Height += hS * sw;
                        if (it.Kind == SeaDisturbanceKind.Wake)
                        {
                            double bh = -u - 0.6 * A, bh0 = Math.Max(bh, 0.0);
                            double fade = Math.Exp(-bh0 / (10.0 * A + 40.0));
                            double on = Smooth(-0.2 * A, 0.3 * A, bh);
                            double dv = Math.Abs(v) - (0.8 * B + KelvinTan * bh0);
                            double sa = 0.12 * B + 0.5 + 0.02 * bh0;
                            double fqA = fq * 1.4 + 1e-4;
                            double arm = on * fade * GaussF(dv, sa, fqA);
                            o.Foam = Math.Max(o.Foam, 0.7 * S * arm);
                            o.Height += Math.Min(Math.Max(0.04 * B + 0.15, 0.2), 0.8) * S * on * fade * GaussF(dv, 1.5 * sa, fqA);
                            double scar = on * Math.Exp(-bh0 / (8.0 * A + 30.0)) * GaussF(v, 0.7 * B + 0.05 * bh0, fqV);
                            o.Calm = Math.Max(o.Calm, 0.6 * S * scar);
                            double churn = on * Math.Exp(-bh0 / (3.0 * A + 20.0)) * GaussF(v, 0.6 * B + 0.03 * bh0, fqV);
                            o.Foam = Math.Max(o.Foam, Math.Max(0.15 * S * scar, 0.5 * S * churn));
                            double rho = Math.Sqrt(u * u / (A * A) + v * v / (B * B)) + 1e-5;
                            double bow = GaussF((rho - 1.08) * Bm, 0.08 * Bm + 0.4, fq * 1.4 + 1e-4) * Smooth(-0.2, 0.6, u / A);
                            o.Foam = Math.Max(o.Foam, 0.6 * S * bow);
                        }
                        break;
                    }
                    case SeaDisturbanceKind.Breach:
                    {
                        double rho = Math.Sqrt(u * u / (A * A) + v * v / (B * B)) + 1e-5;
                        double e = (rho - 1.0) * Bm;
                        double fqE = fq * 1.4 + 1e-4;
                        double thB = Math.Atan2(v / B, u / A);
                        double eW = e + Bm * 0.14 * (Math.Sin(3.0 * thB + 1.1) + 0.7 * Math.Sin(5.0 * thB + 2.3 + 2.0 * Math.PI * clock / WaterlinePeriod) + 0.5 * Math.Sin(8.0 * thB - 0.7));
                        double we = Math.Sqrt(0.0625 * Bm * Bm + Filter * fqE * fqE);
                        o.White = Math.Max(o.White, S * (1.0 - Smooth(-we, we, eW - 0.25 * Bm)));
                        double ringW = GaussF(eW - 0.35 * Bm, 0.2 * Bm + 1.0, fqE);
                        double out1 = Smooth(0.0, 4.0, e);
                        double rings = RingTrain(e, BreachRingSpeed, BreachRingPeriod, 1.2, fqE, clock) * Math.Exp(-Math.Max(e, 0.0) / (1.5 * Bm + 20.0)) * out1;
                        double pool = Math.Exp(-Math.Max(eW, 0.0) / (1.2 * Bm));
                        o.Foam = Math.Max(o.Foam, S * Math.Max(Math.Max(ringW, 0.3 * rings), 0.45 * pool));
                        double hB = Math.Min(Math.Max(0.15 * Bm, 1.5), 7.0) * S;
                        o.Height += hB * Math.Exp(-0.5 * rho * rho / 0.81);
                        double lam = BreachRingSpeed * BreachRingPeriod;
                        double att = Math.Exp(-2.0 * Math.Pow(Math.PI * fqE / lam, 2.0));
                        o.Height += 0.6 * S * Math.Exp(-Math.Max(e, 0.0) / (Bm + 25.0)) * out1 * att * Math.Cos(2.0 * Math.PI * (e - BreachRingSpeed * clock) / lam);
                        break;
                    }
                    case SeaDisturbanceKind.TentacleRise:
                    {
                        double r = Math.Sqrt(r2) + 1e-5;
                        double e = r - A;
                        double fqE = fq * 1.4 + 1e-4;
                        double wc = 0.35 * A + 0.5;
                        o.White = Math.Max(o.White, S * GaussF(Math.Max(e, 0.0), wc, fqE));
                        double out1 = Smooth(0.0, 1.0, e);
                        double rings = RingTrain(e, TentacleRingSpeed, TentacleRingPeriod, 0.5 + 0.1 * A, fqE, clock) * Math.Exp(-Math.Max(e, 0.0) / (2.0 * A + 5.0)) * out1;
                        double a = -dx, b = dz;
                        double sb = A + 0.12 * Math.Max(a, 0.0);
                        double L = 18.0 * A + 25.0;
                        double stream = Smooth(-A, A, a) * Math.Exp(-Math.Max(a, 0.0) / L) * GaussF(b, sb, fq + 1e-4);
                        o.Foam = Math.Max(o.Foam, S * Math.Max(0.25 * rings, 0.8 * stream));
                        double sc = 0.6 * A + 0.5;
                        o.Height += Math.Min(Math.Max(0.4 * A, 0.3), 2.0) * S * GaussF(e + 0.1 * A, sc, fqE);
                        break;
                    }
                    default:
                    {
                        double r = Math.Sqrt(r2) + 1e-5;
                        double fqR = fq * 1.4 + 1e-4;
                        double Rr = SlapRingRadius(A, S);
                        double sr = 0.18 * A + 0.8 + 0.1 * (Rr - A);
                        double ring = GaussF(r - Rr, sr, fqR);
                        double core = GaussF(r, 0.7 * A, fqR);
                        double inside = 1.0 - Smooth(0.4 * Rr, 1.05 * Rr, r);
                        o.Foam = Math.Max(o.Foam, Math.Max(Math.Sqrt(S) * ring, 0.4 * S * inside));
                        o.White = Math.Max(o.White, S * S * core);
                        double hS = Math.Min(Math.Max(0.25 * A, 0.4), 2.5);
                        o.Height += S * hS * GaussF(r - Rr, sr, fqR) - 0.6 * hS * S * S * GaussF(r, 0.6 * A, fqR);
                        break;
                    }
                }
            }
            o.Dark = Sat(o.Dark); o.Calm = Sat(o.Calm); o.Foam = Sat(o.Foam); o.White = Sat(o.White);
            return o;
        }
    }
}
