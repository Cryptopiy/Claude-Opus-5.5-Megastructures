// BwAudioSpec: the generic audio-engine constants (MASTER_PROMPT 14.3, lessons 3.2 #14; RESEARCH C). Extracted from the
// generic part of Thread's AudioSpec (Db, sine table, sample rate, limiter); Thread's 8 layers, levels, swells and
// windows are not imported. Per-layer Bulwark levels belong to BulwarkScore (W/S/B/F/T/P/V/C) and BulwarkConst.
using System;

namespace Megastructure.Audio
{
    public static class BwAudioSpec
    {
        public const int SampleRate = FrameworkConst.AudioSampleRate;   // offline renders and the requested device rate (48 kHz)
        public const int SineTableSize = 4096;
        public const double SmoothTau = 0.020;            // one-pole per-sample parameter smoothing (20 ms)
        public const double DcBlockHz = 20.0;             // master DC block (Thread used 10 Hz)

        // ------------------------------------------------------------ master limiter (RESEARCH C 5; C-24: The Ring's DspLimiter)
        /// <summary>Asymptote of the limiter output in dBFS (-1.42 dBFS: c = 10^(-1.42/20) = 0.849); true peak stays <= -1 dBFS.</summary>
        public const double LimiterCeilingDb = FrameworkConst.LimiterCeilingDb;
        public const double LimiterThresholdDb = -4.0;
        public const double LimiterLookaheadMs = 16.0;
        public const double LimiterHoldMs = 20.0;
        public const double LimiterReleaseMs = 250.0;
        public const double TruePeakMaxDb = -1.0;

        // ------------------------------------------------------------ reverb (FDN, two instances: small and hall)
        /// <summary>Default parameter cross-blend time of the FDN (s). Doorways use >= 2 s, hard edges 0.6 s (14.6).</summary>
        public const double ReverbBlendSeconds = 2.0;
        public const double ReverbBlendHardEdge = 0.6;
        public const double ReverbSizeSmall = 1.0, ReverbSizeHall = 2.6;
        public const double ReverbMaxPreDelay = 0.25;

        // ------------------------------------------------------------ footsteps (14.3)
        public const double FootPeakDb = FrameworkConst.FootPeakDb;   // -30 dBFS
        public const double FootLevelJitterDb = 0.8;
        public const double FootPitchJitter = 0.04;
        public const double FootPan = 0.15;
        public const int FootstepVariants = 6;

        // ------------------------------------------------------------ impacts: 14.3 layer B boom on the impact bus (14.11 intro impact)
        /// <summary>Boom tone: sines 41 and 63 Hz (weights 1 and 0.6); beat 6 only the 31 Hz sub.</summary>
        public const double BoomHz1 = 41.0, BoomHz2 = 63.0, BoomSubHz = 31.0, BoomTone2Weight = 0.6;
        /// <summary>T60 of the tone (1.6 s) and of the brown rumble (0.9 s); 4 ms attack; 3 ms click through a 180 Hz low-pass.</summary>
        public const double BoomDecayT60 = 1.6, BoomRumbleT60 = 0.9, BoomAttackMs = 4.0, BoomClickMs = 3.0, BoomClickLpHz = 180.0, BoomRumbleLpHz = 120.0;
        /// <summary>Tone peak at strength 1.0 (dBFS). Tone amplitude A = 10^(-10/20) / 1.6 = 0.198, tau = 1.6 / 6.908 = 0.232 s,
        /// energy 0.68 A^2 tau / 2 = 0.0031: one impact is about -25 dBFS RMS over its first second (-20 over the first 0.25 s);
        /// with the 9 s mean interval and a mean strength of 0.65 the tone part is near -38 dBFS RMS, which the score owner trims
        /// to the 14.3 layer level (B1 -35). The intro impact (strength 1.0) is the only sound of the black 0-1.0 s.</summary>
        public const double BoomPeakDb = -10.0;
        /// <summary>Click and rumble gains relative to the boom gain (linear).</summary>
        public const double BoomClickGain = 4.0, BoomRumbleGain = 0.5;
        public const int MaxBoomVoices = 4;

        // ------------------------------------------------------------ pause / ending (14.11)
        public const double PauseDuckDb = FrameworkConst.PauseDuckDb;
        public const double PauseDuckSeconds = FrameworkConst.PauseDuckTime;

        public static double Db(double db) => Math.Pow(10.0, db / 20.0);
        public static double ToDb(double gain) => 20.0 * Math.Log10(Math.Max(1e-30, gain));
        /// <summary>Per-sample multiplier for a T60 decay ("decay" in the spec always means T60).</summary>
        public static double T60Mul(double t60, double fs) => Math.Exp(-6.907755278982137 / (Math.Max(1e-4, t60) * fs));
    }
}
