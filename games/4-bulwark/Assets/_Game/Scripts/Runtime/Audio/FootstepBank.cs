// FootstepBank: procedural footstep clips (MASTER_PROMPT 14.3), rendered once at startup on the main thread and mixed
// INSIDE BwDsp (through the reverb and the limiter; lesson 3.2 #14), so the player dump matches the offline render.
// Pattern imported from Thread's FootstepBank (FootstepRenderer: seeded excitation, per-variant jitter, fade-out,
// peak normalisation); Thread's salt / stone / metal recipes and its 0.78 / 0.80 m steps are NOT imported.
// Bulwark recipes (14.3; first pass for the audio owner):
//   concrete     modal resonators 190 / 420 / 1100 Hz, T60 80 / 50 / 30 ms
//   wet concrete the concrete recipe plus a 6 kHz tick
//   road         the concrete recipe, a little darker (the asphalt wearing course of the crest road)
//   grating      310 / 880 / 2100 Hz plus a 4 kHz ring
//   steel stair  260 / 700 / 1800 Hz
//   ledge        the concrete recipe plus a 1.5 kHz scrape
// Each surface has BwAudioSpec.FootstepVariants variants with +-4 % pitch; levels jitter +-0.8 dB per step (BwDsp);
// clip peak -30 dBFS. Stride by speed lives in PlayerMath.Stride (0.75 / 0.85 m, 0.90 m on stairs).
using System;

namespace Megastructure.Audio
{
    public sealed class FootstepBank
    {
        public const int SurfaceCount = 6;
        const double ClipSeconds = 0.30;
        const double ExciteT60 = 0.004;

        struct Recipe
        {
            public double f0, f1, f2, t0, t1, t2, g0, g1, g2;
            public double extraHz, extraT60, extraGain;   // tick / ring / scrape (0 = none)
            public bool scrape;                            // noise band instead of a sine partial
            public double lowpassHz;
        }

        static readonly Recipe[] s_recipes =
        {
            new Recipe { f0 = 190, f1 = 420, f2 = 1100, t0 = 0.080, t1 = 0.050, t2 = 0.030, g0 = 1.0, g1 = 0.6, g2 = 0.35, lowpassHz = 9000 },                                   // concrete
            new Recipe { f0 = 190, f1 = 420, f2 = 1100, t0 = 0.080, t1 = 0.050, t2 = 0.030, g0 = 1.0, g1 = 0.6, g2 = 0.35, extraHz = 6000, extraT60 = 0.012, extraGain = 0.25, lowpassHz = 12000 },   // wet concrete
            new Recipe { f0 = 175, f1 = 390, f2 = 1000, t0 = 0.085, t1 = 0.050, t2 = 0.028, g0 = 1.0, g1 = 0.55, g2 = 0.25, lowpassHz = 6000 },                                   // road
            new Recipe { f0 = 310, f1 = 880, f2 = 2100, t0 = 0.090, t1 = 0.070, t2 = 0.050, g0 = 0.8, g1 = 0.7, g2 = 0.5, extraHz = 4000, extraT60 = 0.250, extraGain = 0.12, lowpassHz = 14000 }, // grating
            new Recipe { f0 = 260, f1 = 700, f2 = 1800, t0 = 0.120, t1 = 0.080, t2 = 0.060, g0 = 1.0, g1 = 0.7, g2 = 0.45, lowpassHz = 12000 },                                   // steel stair
            new Recipe { f0 = 190, f1 = 420, f2 = 1100, t0 = 0.070, t1 = 0.045, t2 = 0.028, g0 = 1.0, g1 = 0.6, g2 = 0.35, extraHz = 1500, extraT60 = 0.060, extraGain = 0.3, scrape = true, lowpassHz = 9000 }, // ledge
        };

        readonly float[][][] m_clips;   // [surface][variant][sample]

        public int SampleRate { get; }

        public FootstepBank(int sampleRate, int seed)
        {
            SampleRate = sampleRate > 0 ? sampleRate : BwAudioSpec.SampleRate;
            int n = Math.Max(64, (int)(ClipSeconds * SampleRate));
            m_clips = new float[SurfaceCount][][];
            for (int s = 0; s < SurfaceCount; s++)
            {
                m_clips[s] = new float[BwAudioSpec.FootstepVariants][];
                for (int v = 0; v < BwAudioSpec.FootstepVariants; v++)
                {
                    var dst = new float[n];
                    Render(s, v, seed, dst);
                    m_clips[s][v] = dst;
                }
            }
        }

        /// <summary>The clip of (surface, variant); never null.</summary>
        public float[] Clip(int surface, int variant) => m_clips[surface][variant];

        void Render(int surface, int variant, int seed, float[] dst)
        {
            Recipe r = s_recipes[surface];
            var rng = XorShift32.Stream(seed, 0x5700u + (uint)(surface * 16 + variant));
            double fs = SampleRate;
            double pitch = 1.0 + (rng.Next01() * 2.0 - 1.0) * BwAudioSpec.FootPitchJitter;
            var lp = new SvfTpt(); lp.Set(Math.Min(r.lowpassHz, 0.45 * fs), 0.7, fs);
            var scrapeBp = new SvfTpt(); if (r.scrape) scrapeBp.Set(r.extraHz * pitch, 1.5, fs);
            // two-pole resonators y = 2 R cos(w) y1 - R^2 y2 + x, R from the T60
            double[] f = { r.f0 * pitch, r.f1 * pitch, r.f2 * pitch };
            double[] t60 = { r.t0, r.t1, r.t2 };
            double[] g = { r.g0, r.g1, r.g2 };
            var y1 = new double[3]; var y2 = new double[3]; var c1 = new double[3]; var c2 = new double[3]; var norm = new double[3];
            for (int k = 0; k < 3; k++)
            {
                double R = Math.Exp(-6.907755278982137 / (t60[k] * fs));
                double w = 2.0 * Math.PI * Math.Min(f[k], 0.45 * fs) / fs;
                c1[k] = 2.0 * R * Math.Cos(w); c2[k] = R * R;
                norm[k] = (1.0 - R) * 2.0;   // rough unit gain at resonance
            }
            double exc = 1.0, excMul = BwAudioSpec.T60Mul(ExciteT60, fs);
            double extraPh = 0, extraEnv = 1.0, extraMul = r.extraT60 > 0 ? BwAudioSpec.T60Mul(r.extraT60, fs) : 0.0;
            double extraInc = r.extraHz * pitch / fs;
            float peak = 1e-9f;
            for (int i = 0; i < dst.Length; i++)
            {
                double x = rng.NextBipolar() * exc;
                exc *= excMul;
                double sum = 0;
                for (int k = 0; k < 3; k++)
                {
                    double y = c1[k] * y1[k] - c2[k] * y2[k] + x;
                    y2[k] = y1[k]; y1[k] = y;
                    sum += g[k] * norm[k] * y;
                }
                if (r.extraGain > 0)
                {
                    double e = r.scrape ? scrapeBp.BandNorm(rng.NextBipolar()) : SineTable.Lookup(extraPh);
                    sum += r.extraGain * extraEnv * e;
                    extraEnv *= extraMul;
                    extraPh += extraInc; if (extraPh >= 1.0) extraPh -= 1.0;
                }
                float v = lp.Low((float)sum);
                dst[i] = v;
                float a = Math.Abs(v);
                if (a > peak) peak = a;
            }
            // fade the last 15 % and normalise the peak to -30 dBFS
            int fade = dst.Length * 15 / 100;
            for (int i = dst.Length - fade; i < dst.Length; i++) dst[i] *= (float)(0.5 + 0.5 * Math.Cos(Math.PI * (i - (dst.Length - fade)) / fade));
            float scale = (float)(BwAudioSpec.Db(BwAudioSpec.FootPeakDb) / peak);
            for (int i = 0; i < dst.Length; i++) dst[i] *= scale;
            dst[0] = 0f;
            dst[dst.Length - 1] = 0f;
        }
    }
}
