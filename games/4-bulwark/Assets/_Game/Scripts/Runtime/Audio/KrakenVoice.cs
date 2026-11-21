// KrakenVoice: the synthesised voice of the kraken (ADDENDUM_A1 section 2, LEAD DECISION 3) for every KrakenCallKind of the
// KrakenCalls bus, Unity-free and allocation-free on the audio thread. One KrakenVoice = one event; the KrakenBus (bottom)
// owns a fixed pool, spatialises each voice at its world position against the listener pose of BwAudioParams, sends it
// into its own large FDN (an ENORMOUS outdoor reverb, the hall reverb under water, a short one through the wall), high-
// passes the bus at 15 Hz (2.7), limits it with its own 5 ms look-ahead limiter (ceiling -6 dBFS) and reports the bed duck
// (6-10 dB, 2.6) to BwDsp.
// Recipes (2.3; every one keeps its energy in the 80-800 Hz band so laptops and phones hear it, LEAD DECISION 3):
//   Call       KV glottal pulse train f0 24 -> 30 Hz in 1 s then -> 21 Hz by 7/8 of the event, jitter 7 %, shimmer +-3 dB,
//              period doubling on 30 % of the periods, a 1.5 s chaotic segment at the peak (+-30 % f0) with an octave-below
//              subharmonic fading in; three gliding formants "oo -> aa -> oo" (F1 220 -> 420 -> 200, F2 560 -> 950 -> 480,
//              F3 1150 -> 1500 -> 1000 Hz, SVF band-pass Q 6); pulse-synchronous turbulence (the grinding fry) through the
//              same formants; roughness AM 40-60 Hz (random walk, depth 0.35 -> 0.6 at the peak); KB body 19/38/57/76 Hz;
//              KC flesh creak 90/210/470 Hz (Q 8) accelerating 6 -> 30 /s. Attack 1.5 s, release 3 s.
//   SurfaceRoar KW brown LP 600 Hz + pink BP 1.8 kHz (Q 0.8) with random 0.5-2 s swells, sheet rumble brown LP 120 Hz,
//              KB body, a low groan (KV at -10 dB) and flesh creaks.
//   Slap       KL sines 28 + 44 Hz (T60 2.5 s) + a 160 -> 60 Hz body thud (T60 0.6 s, the audible weight) + 3 ms crack (pink
//              HP 1 kHz) + spray hiss (pink BP 2 kHz, 1.5 s) + brown LP 300 Hz tail (3 s).
//   DamShudder KX sines 24 + 31 Hz (T60 4 s), brown LP 150 Hz rumble (3 s), concrete modes 60/130/310 Hz (Q 12) struck and
//              then ground by stick-slip 6 -> 30 /s (the groan of the structure), dust trickle (2-6 kHz grains).
//   GlassCreak KC glass 230/517/940/1460 Hz (Q 40) stick-slip 6 -> 30 /s + the muffled close growl (KV, low formants) +
//              clicks + KB body.
//   SuckerPops KS thocks (3 ms click LP 3 kHz + 150 -> 70 Hz burst 35 ms + 70 Hz thump 60 ms + wet hiss 250 ms) at
//              0.8 -> 2.5 /s in a travelling wave across the pan, KH hook scrape grains (600-2400 Hz, 25 /s), glass creak.
//   EyeSwell   2 s of silence (the beds are already ducked), then a vibrating infrasonic swell: KB 19/38/57/76 Hz with
//              roughness AM, a deep growl (f0 18-22 Hz with the subharmonic, formants 180/450/900 Hz), the glass groan.
//   Stinger    the call recipe, deeper (subharmonic from the start), as phrases every 10 s over a sustained groan (the voice
//              never drops under 0.4) with flesh creaks and an 80-300 Hz harmonic layer, so one event can last about 30 s;
//              heard from 7.5 km (fix wave S2-2: RefDb -30.5 at 7500 m, the air low-pass floor 800 Hz, not 250).
//   Strike     (fix wave S3-4; the beat-6 strike on the glass at s 1300: a DamShudder published in beat 6, or kind 8) concrete
//              modes 60/130/310/620 Hz struck and ground, a 31 Hz pane boom, the 230/517 Hz glass ring (Q 120), the KG crack
//              at +0.4 s (clicks into 2.1/3.4 kHz Q 30 + 8 ms HP noise); no wall low-pass, the hall reverb.
// Small speakers (fix wave S2-1, LEAD DECISION 3): every kind carries energy above 250 Hz: the outdoor voice (Call, the roar's
// groan, Stinger) waveshapes its glottal pulses (tanh) before the formants and raises F2/F3 by 6-8 dB, adds an 80-300 Hz
// harmonic band; the roar a 600-1500 Hz pink layer; the slap a 300-1200 Hz body crack (80 ms band noise + the thud's 2nd and
// 3rd harmonics) and spray 1.5; the shudder its 310 Hz mode +6 dB and 600-900 Hz grinding; calls and the stinger a far tail
// (pre-delay d / 343 s, the bus FDN at T60 5 s) so the voice sounds kilometres wide. Impacts (Slap, DamShudder, Strike) duck
// the beds from their arrival (0.25 s attack), not 1.5 s before it.
// Spatialisation (2.2): level = RefDb(kind) - 10 log10(d / RefDistance(kind)) (compressed distance law: art-directed
// levels of 2.4 win; clamped -15 .. +3 dB) + 20 log10(strength); air low-pass fc = max(250, 8000 / (1 + d / 150)) Hz; pan by
// the bearing relative to the listener yaw (equal power); sources behind get a 2.5 kHz low-pass and -2 dB. Under water:
// two cascaded low-passes at 450 Hz (24 dB/oct, "behind glass"), pan by bearing, the hall reverb. Listener inside the
// concrete (wall variant): two low-passes at 320 Hz, no pan.
using System;
using System.Runtime.CompilerServices;

namespace Megastructure.Audio
{
    /// <summary>A kraken sound event for the audio thread (AudioDirector converts each new KrakenCall into one).</summary>
    public struct KrakenEvent
    {
        public int kind;            // KrakenCallKind
        public float strength;      // 0..1 (KrakenCall.Strength)
        public float duration;      // s (clamped to the recipe minimum)
        public bool underwater;
        public float x, y, z;       // world position of the source (m)
        public float delay;         // seconds from now until the sound starts (scheduled ahead, d/343 for impacts outdoors)
    }

    public sealed class KrakenEventRing : SpscRing<KrakenEvent> { }

    /// <summary>Per-kind constants of the kraken recipes (levels at the listener, 3 s RMS of the loud part, strength 1).</summary>
    public static class KrakenRecipe
    {
        public const int KindCount = 9;
        public const int Call = 0, SurfaceRoar = 1, Slap = 2, DamShudder = 3, GlassCreak = 4, SuckerPops = 5, EyeSwell = 6, Stinger = 7, Strike = 8;
        public static readonly string[] Names = { "call", "surface_roar", "slap", "dam_shudder", "glass_creak", "sucker_pops", "eye_swell", "stinger", "strike" };

        /// <summary>3 s RMS (dBFS) at the reference distance, strength 1 (2.4: K4 voice -27 at 600 m, K4 water -28, K5 slap RMS -22
        /// at 209 m, KX RMS -20 (outdoors -24 at 100 m), glass -32, suckers -34, the lit eye -28, the stinger -40 from the foot).
        /// Every kind but the slap (limited by the -6 dBFS bus ceiling: RMS -22, peak -6) is 2 dB above 2.4 and the outdoor voice
        /// 5 dB, the roar 7 dB and the dam shudder 4 dB (LEAD DECISION 3, "clearly audible on any speaker":
        /// the player dump audio-qakraken01 at the crest (bed -24) showed the 2.4 levels at or under the ducked bed in the
        /// 80-800 Hz band); with the 6-10 dB bed duck every event stays at the 2.5 window ceilings (crest: bed -24 - 10 = -34,
        /// call -22, sum -21.7 vs ceiling -21). AudibilityMarginDb adds a floor: never less than the ducked bed + 6 dB.</summary>
        /// Fix wave S2-1: the outdoor call, roar, slap and shudder +2 dB more (the small-speaker gate of audio_analysis.py:
        /// at strength 1 at the reference distance over the crest / slope beds the loudest second clears HP 250 / HP 400 by
        /// 6 dB, an impact's loudest 50 ms by 8 dB; the 2.5 window ceilings -21 / -19 are exceeded by up to 1-2 dB at
        /// strength 1 for the loudest seconds: LEAD DECISION 3 wins).
        /// Fix wave S2-2: the stinger -30.5 at 7500 m (the platform is 7.3 km from it: -30.4 at strength 1, -32.3 at 0.8, over the
        /// -46 platform bed; it was -41.1). The strike (kind 8) -22 at 40 m (the window hall, ceiling -26 + the 10 dB duck).</summary>
        public static readonly double[] RefDb = { -20.0, -19.0, -18.0, -18.0, -28.0, -30.0, -25.0, -30.5, -22.0 };
        /// <summary>Reference distance (m) of RefDb; under water the call and the stinger use 30 m.</summary>
        public static readonly double[] RefDistance = { 600.0, 600.0, 200.0, 100.0, 20.0, 15.0, 20.0, 7500.0, 40.0 };
        public const double RefDistanceUnderwater = 30.0;
        /// <summary>The call under water (beat 6: K7 through the glass, the close voice at 1228, the second call at 1262, the deepest
        /// groan at 1312): 2.4 gives -28 / -26 / -30 under the 2.5 ceilings -30 / -26; -26 at 30 m here (+2 dB, LEAD DECISION 3).</summary>
        public const double RefDbUnderwaterCall = -26.0;
        /// <summary>Default and minimum duration (s).</summary>
        public static readonly double[] DefaultDuration = { 8.0, 10.0, 4.0, 5.0, 8.0, 10.0, 12.0, 30.0, 6.0 };
        public static readonly double[] MinDuration = { 5.0, 4.0, 3.0, 3.0, 3.0, 3.0, 6.0, 5.0, 3.0 };
        /// <summary>Bed duck while the event sounds (dB, 2.6: 6-10).</summary>
        public static readonly double[] DuckDb = { 10.0, 10.0, 8.0, 10.0, 8.0, 6.0, 10.0, 8.0, 10.0 };
        /// <summary>Audibility floor: a kraken event at strength 1 is never quieter than the ducked bed + this (dB), wherever the
        /// writer places it (a far call over the spillway roar of beat 3 included); lower strengths scale from the floor.</summary>
        // Fix wave X2 S2-1: 12 dB over the ducked bed (= 2 dB over the un-ducked bed at strength 1), and the floor scales with
        // 10 log10(strength), not 20: the player dump x2_k2 showed the far K1 voice (strength 0.7, 1180 m, -26.0 dBFS by the
        // distance law) only +5 dB over the crest bed at HP 250 / HP 400; the floor lifts it to about -22.5.
        public const double AudibilityMarginDb = 12.0;
        /// <summary>Kinds that arrive d / 343 s after the picture outdoors (2.1 rule 2: slaps, cascades and water roar).</summary>
        public static bool Delayed(int kind) => kind == SurfaceRoar || kind == Slap;
        /// <summary>Impacts duck the beds from their arrival (fix wave S2-1), the voices from DuckAttackSeconds before it.</summary>
        public static bool Impact(int kind) => kind == Slap || kind == DamShudder || kind == Strike;
        /// <summary>Kinds with the far tail (pre-delay d / 343 s into the bus FDN): the voices heard outdoors.</summary>
        public static bool FarTail(int kind) => kind == Call || kind == Stinger;
        public const double FarTailMaxSeconds = 5.5, FarTailMinSeconds = 0.25, FarTailSendDb = -3.0;
        /// <summary>Air low-pass floor of the stinger (Hz): heard from 7.3 km it would get the 250 Hz floor (fix wave S2-2).</summary>
        public const double StingerAirFloorHz = 800.0;
        /// <summary>Normalisation of the dry synthesis per kind (calibrated with the offline renders so that strength 1 at the
        /// reference distance gives RefDb; AudioTests.Kraken_LevelsMatchTheRecipes keeps it within +-1.5 dB).</summary>
        public static readonly double[] NormDb = { 12.1, 4.2, 19.1, 4.1, 10.5, 19.0, 16.2, 4.0, 9.3 };
        /// <summary>The call under water keeps the dark (unshaped) voice and its own normalisation.</summary>
        public const double NormDbUnderwaterCall = 19.0;

        /// <summary>Distance term limits (dB): a source at the listener (the beat 6 writer places some calls at the eye) gains at
        /// most +3 dB over its reference level; a very far one loses at most 15 dB (then the audibility floor holds it).</summary>
        public const double DistanceClampLoDb = -15.0, DistanceClampHiDb = 3.0;
        public const double DuckAttackSeconds = 1.5, DuckReleaseSeconds = 3.0, DuckAttackImpactSeconds = 0.25;
        /// <summary>An impact ducks the other kraken voices (the roar it lands in) by ImpactVoiceDuckDb from its arrival for
        /// ImpactVoiceDuckHold s (50 ms smoothing in, the spatial smoothing): the K5 slap at s 822 lands inside the K4 sink roar
        /// (player dump x2_k4: its loudest 50 ms only +1-2.5 dB over the roar before it).</summary>
        public const double ImpactVoiceDuckDb = 10.0, ImpactVoiceDuckHold = 1.2, ImpactVoiceDuckLead = 0.15;   // the duck leads the arrival by 150 ms (it is down when the blow lands)
        public const double BusHighPassHz = 15.0;
        public const double BusLimiterCeilingDb = -3.0, BusLimiterThresholdDb = -6.0, BusLimiterLookaheadMs = 5.0, BusLimiterReleaseMs = 200.0;
        public const double SpeedOfSound = 343.0;

        public static double LevelDb(int kind, double distance, double strength, bool underwater)
        {
            int k = Math.Max(0, Math.Min(KindCount - 1, kind));
            bool water = underwater && (k == Call || k == Stinger);
            double dRef = water ? RefDistanceUnderwater : RefDistance[k];
            double dd = -10.0 * Math.Log10(Math.Max(1.0, distance) / dRef);
            dd = Math.Max(DistanceClampLoDb, Math.Min(DistanceClampHiDb, dd));
            return (water ? RefDbUnderwaterCall : RefDb[k]) + dd + 20.0 * Math.Log10(Math.Max(1e-3, Math.Min(1.0, strength)));
        }

        /// <summary>Air absorption low-pass of 2.2 (Hz).</summary>
        public static double AirLowpassHz(double distance) => Math.Max(250.0, 8000.0 / (1.0 + Math.Max(0.0, distance) / 150.0));
        public static double AirLowpassHz(double distance, int kind) => Math.Max(kind == Stinger ? StingerAirFloorHz : 250.0, AirLowpassHz(distance));
    }

    /// <summary>Two-pole modal resonator normalised for impulse excitation (an impulse of 1 rings at amplitude ~1).</summary>
    public struct Reson
    {
        public float c1, c2, g, y1, y2;
        public void Set(double f, double q, double gain, double fs)
        {
            double ff = Math.Min(Math.Max(f, 5.0), 0.45 * fs);
            double R = Math.Exp(-Math.PI * ff / (Math.Max(0.5, q) * fs));
            double w = 2.0 * Math.PI * ff / fs;
            c1 = (float)(2.0 * R * Math.Cos(w)); c2 = (float)(R * R);
            g = (float)(gain * Math.Sin(w));
            y1 = 0f; y2 = 0f;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float Tick(float x)
        {
            float y = c1 * y1 - c2 * y2 + x;
            y2 = y1; y1 = y;
            return g * y;
        }
        public void Flush() { if (Math.Abs(y1) < 1e-20f) y1 = 0f; if (Math.Abs(y2) < 1e-20f) y2 = 0f; }
    }

    /// <summary>One kraken event being synthesised (mono) and spatialised (stereo). Pooled; Start() resets every state.</summary>
    public sealed class KrakenVoice
    {
        public bool Active;
        public int Kind;
        public bool Underwater;
        public float X, Y, Z;
        public int Delay;            // frames before the first sample
        public int N, Len;           // samples since the start, total length
        public float Gain;           // calibrated linear gain (level x strength)
        public float Strength;
        public float Duck;           // 0..1 of DuckDb while sounding (incl. the pre-roll)

        readonly double m_fs;
        XorShift32 m_rng;
        PinkKellet m_pink, m_pink2;
        BrownNoise m_brown, m_brown2;
        double m_dur;

        // glottal source (KV)
        double m_gPh, m_gF0, m_gPeriodJit = 1.0;
        float m_gAmp = 1f, m_gPrevFlow, m_gNextAmp = 1f;
        int m_gPeriod;
        bool m_gDouble;
        double m_amPh, m_amHz = 50.0, m_amTarget = 50.0;
        SvfTpt m_f1, m_f2, m_f3, m_t1, m_t2, m_t3, m_bodyLp, m_breathLp, m_harm;
        int m_formantCounter;
        // small-speaker layers (fix wave S2-1): roar 600-1500 Hz, slap body crack 300-1200 Hz, shudder grinding 600-900 Hz
        SvfTpt m_wHi, m_slapBody, m_grindBp;
        float m_grindEnv;
        // the strike (kind 8): glass ring 230/517 Hz and the KG crack 2.1/3.4 kHz
        Reson m_g0, m_g1, m_c0, m_c1;
        // body sines (KB) and the tonal decays (KL, KX)
        double m_ph0, m_ph1, m_ph2, m_ph3, m_ph4, m_ph5, m_ph6;
        // creak (KC): stick-slip into modes
        Reson m_m0, m_m1, m_m2, m_m3;
        double m_creakNext;
        // water (KW)
        SvfTpt m_wLp, m_wBp, m_wSheet, m_wMid;
        float m_swell = 0.5f, m_swellTarget = 0.5f;
        double m_swellNext;
        // thocks (KS) and scrape grains (KH)
        double m_popNext, m_grainNext;
        int m_popN = 100000, m_grainN = 100000, m_grainLen = 1;
        float m_popPan, m_popAmp;
        double m_chirpPh;
        SvfTpt m_popClickLp, m_popHissHp, m_grainBp, m_crackHp, m_sprayBp, m_tailLp, m_rumbleLp, m_dustBp;
        float m_grainAmp;
        // click (GlassCreak)
        double m_clickNext;
        int m_clickN = 100000;
        // spatialiser state (smoothed per sample toward the per-block targets)
        public float TgtL, TgtR, TgtSend;
        float m_gl, m_gr, m_send;
        public SvfTpt Air1, Air2, Behind;
        public float BehindMix;
        public bool SnapSpatial;
        /// <summary>Far tail (fix wave S2-1): send gain into the bus's far delay line and the delay (frames; -1 = not yet set).</summary>
        public float TgtFar;
        public int FarDelay = -1;
        float m_far;
        /// <summary>Gain target of the bus's impact duck (1 = none; KrakenBus.Control), smoothed per sample.</summary>
        public float TgtKDuck = 1f;
        float m_kduck = 1f;

        readonly float m_grindDecay;

        public KrakenVoice(double fs) { m_fs = fs; m_grindDecay = (float)Math.Exp(-1.0 / (0.03 * fs)); }

        public void Start(in KrakenEvent e, uint seed, float gain)
        {
            Active = true;
            Kind = Math.Max(0, Math.Min(KrakenRecipe.KindCount - 1, e.kind));
            Underwater = e.underwater;
            X = e.x; Y = e.y; Z = e.z;
            Strength = Math.Max(0f, Math.Min(1f, e.strength));
            m_dur = Math.Max(KrakenRecipe.MinDuration[Kind], e.duration > 0f ? e.duration : KrakenRecipe.DefaultDuration[Kind]);
            Len = (int)(m_dur * m_fs);
            Delay = Math.Max(0, (int)(e.delay * m_fs));
            N = 0;
            Gain = gain;
            Duck = 0f;
            m_rng = new XorShift32(seed);
            m_pink = default; m_pink2 = default;
            m_brown.Init(20.0, m_fs); m_brown2.Init(20.0, m_fs);
            m_gPh = 0.0; m_gF0 = 24.0; m_gPeriodJit = 1.0; m_gAmp = 1f; m_gNextAmp = 1f; m_gPrevFlow = 0f; m_gPeriod = 0; m_gDouble = false;
            m_amPh = 0.0; m_amHz = 50.0; m_amTarget = 50.0;
            m_f1.Clear(); m_f2.Clear(); m_f3.Clear(); m_t1.Clear(); m_t2.Clear(); m_t3.Clear();
            m_bodyLp.Set(120.0, 0.7, m_fs); m_bodyLp.Clear();
            m_breathLp.Set(2500.0, 0.7, m_fs); m_breathLp.Clear();
            m_formantCounter = 0;
            m_ph0 = m_ph1 = m_ph2 = m_ph3 = m_ph4 = m_ph5 = m_ph6 = 0.0;
            m_creakNext = 0.05 + 0.1 * m_rng.Next01();
            m_wLp.Set(600.0, 0.7, m_fs); m_wLp.Clear();
            m_wBp.Set(1800.0, 0.8, m_fs); m_wBp.Clear();
            m_wSheet.Set(120.0, 0.7, m_fs); m_wSheet.Clear();
            m_wMid.Set(380.0, 0.7, m_fs); m_wMid.Clear();
            m_swell = 0.5f; m_swellTarget = 0.7f; m_swellNext = 0.0;
            m_popNext = 0.3; m_grainNext = 0.0; m_popN = 100000; m_grainN = 100000; m_grainLen = 1; m_popPan = 0f; m_popAmp = 0f; m_chirpPh = 0.0;
            m_popClickLp.Set(3000.0, 0.7, m_fs); m_popClickLp.Clear();
            m_popHissHp.Set(2000.0, 0.7, m_fs); m_popHissHp.Clear();
            m_grainBp.Set(1200.0, 2.0, m_fs); m_grainBp.Clear();
            m_crackHp.Set(1000.0, 0.7, m_fs); m_crackHp.Clear();
            m_sprayBp.Set(2000.0, 0.8, m_fs); m_sprayBp.Clear();
            m_tailLp.Set(300.0, 0.7, m_fs); m_tailLp.Clear();
            m_rumbleLp.Set(150.0, 0.7, m_fs); m_rumbleLp.Clear();
            m_dustBp.Set(4000.0, 1.5, m_fs); m_dustBp.Clear();
            m_harm.Set(160.0, 0.6, m_fs); m_harm.Clear();
            m_wHi.Set(950.0, 0.75, m_fs); m_wHi.Clear();
            m_slapBody.Set(600.0, 0.6, m_fs); m_slapBody.Clear();
            m_grindBp.Set(750.0, 1.8, m_fs); m_grindBp.Clear();
            m_grindEnv = 0f;
            m_g0.Set(230.0, 120.0, 1.0, m_fs); m_g1.Set(517.0, 120.0, 0.8, m_fs);
            m_c0.Set(2100.0, 30.0, 1.0, m_fs); m_c1.Set(3400.0, 30.0, 0.8, m_fs);
            m_grainAmp = 0f;
            m_clickNext = 0.4; m_clickN = 100000;
            switch (Kind)
            {
                case KrakenRecipe.GlassCreak:
                case KrakenRecipe.SuckerPops:
                    SetModes(230, 517, 940, 1460, 40.0); break;
                case KrakenRecipe.EyeSwell:
                    SetModes(230, 517, 940, 1460, 30.0); break;
                case KrakenRecipe.DamShudder:
                case KrakenRecipe.Strike:
                    SetModes(60, 130, 310, 620, 12.0); break;
                default:
                    SetModes(90, 210, 470, 940, 8.0); break;   // flesh (the fourth mode an octave above the third, quiet)
            }
            Air1.Clear(); Air2.Clear(); Behind.Clear();
            SnapSpatial = true;
            m_gl = m_gr = m_send = 0f;
            TgtFar = 0f; m_far = 0f; FarDelay = -1;
            TgtKDuck = 1f; m_kduck = 1f;
        }

        void SetModes(double a, double b, double c, double d, double q)
        {
            m_m0.Set(a, q, 1.0, m_fs); m_m1.Set(b, q, 0.8, m_fs); m_m2.Set(c, q, 0.55, m_fs); m_m3.Set(d, q, Kind == KrakenRecipe.GlassCreak || Kind == KrakenRecipe.SuckerPops || Kind == KrakenRecipe.EyeSwell ? 0.35 : 0.15, m_fs);
        }

        /// <summary>Seconds since the start (negative during the pre-roll).</summary>
        public double Time => (N - Delay) / m_fs;
        public double Duration => m_dur;

        // ------------------------------------------------------------------------------------------- envelopes and helpers
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static double Env(double t, double dur, double att, double rel)
        {
            if (t <= 0.0 || t >= dur) return 0.0;
            double a = t < att ? 0.5 - 0.5 * Math.Cos(Math.PI * t / att) : 1.0;
            double rs = dur - rel;
            double r = t > rs ? 0.5 + 0.5 * Math.Cos(Math.PI * (t - rs) / rel) : 1.0;
            return a * r;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static double Bell(double t, double c, double w) { double x = (t - c) / w; return Math.Exp(-x * x); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        float White() => m_rng.NextBipolar();

        /// <summary>Body swell KB: sines 19 (-10 dB), 38 (0), 57 (-6), 76 Hz (-8) doubled by 114 and 152 Hz (-12, -16: 2.1 rule 4,
        /// the harmonics carry the effect on small speakers) + a little brown LP 50 Hz.</summary>
        float Body(double f0Scale)
        {
            double fs = m_fs;
            m_ph0 += 19.0 * f0Scale / fs; if (m_ph0 >= 1.0) m_ph0 -= 1.0;
            m_ph1 += 38.0 * f0Scale / fs; if (m_ph1 >= 1.0) m_ph1 -= 1.0;
            m_ph2 += 57.0 * f0Scale / fs; if (m_ph2 >= 1.0) m_ph2 -= 1.0;
            m_ph3 += 76.0 * f0Scale / fs; if (m_ph3 >= 1.0) m_ph3 -= 1.0;
            m_ph5 += 114.0 * f0Scale / fs; if (m_ph5 >= 1.0) m_ph5 -= 1.0;
            m_ph6 += 152.0 * f0Scale / fs; if (m_ph6 >= 1.0) m_ph6 -= 1.0;
            float s = 0.32f * SineTable.Lookup(m_ph0) + SineTable.Lookup(m_ph1) + 0.5f * SineTable.Lookup(m_ph2) + 0.4f * SineTable.Lookup(m_ph3)
                    + 0.25f * SineTable.Lookup(m_ph5) + 0.16f * SineTable.Lookup(m_ph6);
            float br = m_bodyLp.Low(m_brown2.Next(White())) * 0.25f;
            return 0.45f * s + br;
        }

        /// <summary>Stick-slip creak: impulses at rate(t) (+-15 % jitter) into the four modes. amp 0..1.</summary>
        float Creak(double t, double rate, float amp)
        {
            float x = 0f;
            if (t >= m_creakNext)
            {
                x = amp * (0.5f + 0.5f * m_rng.Next01()) * (m_rng.Next01() < 0.5f ? -1f : 1f);
                double r = Math.Max(0.5, rate) * (1.0 + 0.15 * White());
                m_creakNext = t + 1.0 / r;
            }
            return m_m0.Tick(x) + m_m1.Tick(x) + m_m2.Tick(x) + m_m3.Tick(x);
        }

        /// <summary>
        /// KV glottal voice: Rosenberg flow derivative at f0 (per-period jitter, shimmer, period doubling, chaos, subharmonic)
        /// through three formants plus pulse-synchronous turbulence (the grinding fry), roughness AM, and a body low-pass.
        /// u = formant position 0 (oo) .. 1 (aa); low = darker formants (under water, the eye).
        /// </summary>
        float Voice(double f0, double jitter, double sub, double amDepth, double u, bool low, float bright = 0f, float harm = 0f)
        {
            double fs = m_fs;
            // formant targets (re-set every 64 samples: the glide is slow)
            if ((m_formantCounter++ & 63) == 0)
            {
                double k = low ? 0.8 : 1.0;
                double F1 = k * (210.0 + (420.0 - 210.0) * u), F2 = k * (520.0 + (950.0 - 520.0) * u), F3 = k * (1075.0 + (1500.0 - 1075.0) * u);
                m_f1.Set(F1, 6.0, fs); m_f2.Set(F2, 6.0, fs); m_f3.Set(F3, 6.0, fs);
                m_t1.Set(F1 * 1.05, 3.0, fs); m_t2.Set(F2 * 1.05, 3.0, fs); m_t3.Set(F3 * 1.05, 3.0, fs);
                // roughness AM random walk 40-60 Hz
                if (m_rng.Next01() < 0.05f) m_amTarget = 40.0 + 20.0 * m_rng.Next01();
                m_amHz += 0.02 * (m_amTarget - m_amHz);
            }
            double ph = m_gPh + m_gF0 * m_gPeriodJit / fs;
            if (ph >= 1.0)
            {
                ph -= 1.0;
                m_gPeriod++;
                m_gF0 = f0;
                double j = jitter * (2.0 * m_rng.NextDouble01() - 1.0);
                m_gPeriodJit = 1.0 + j;
                // shimmer +-3 dB, period doubling on 30 % of the periods, the subharmonic (every other period louder)
                double sh = Math.Pow(10.0, 3.0 * (2.0 * m_rng.NextDouble01() - 1.0) / 20.0);
                if (!m_gDouble && m_rng.Next01() < 0.3f) m_gDouble = true;
                else if (m_gDouble && m_rng.Next01() < 0.4f) m_gDouble = false;
                double alt = (m_gPeriod & 1) == 0 ? 1.0 : (m_gDouble ? 0.55 : 1.0);
                double subW = (m_gPeriod & 1) == 0 ? 1.0 + sub : 1.0 - 0.6 * sub;
                m_gAmp = (float)(sh * alt * subW);
            }
            m_gPh = ph;
            // Rosenberg flow: rise 0..Tp (raised cosine), fall Tp..Tp+Tn (quarter cosine), closed after; abrupt closure (the
            // bright outdoor voice closes in 3 % of the period: more energy above 250 Hz)
            const double Tp = 0.42;
            double Tn = 0.06 - 0.03 * bright;
            double flow;
            if (ph < Tp) flow = 0.5 - 0.5 * Math.Cos(Math.PI * ph / Tp);
            else if (ph < Tp + Tn) flow = Math.Cos(0.5 * Math.PI * (ph - Tp) / Tn);
            else flow = 0.0;
            float fl = (float)flow * m_gAmp;
            float exc = (fl - m_gPrevFlow) * (float)(fs / Math.Max(10.0, m_gF0)) * 0.08f;   // derivative, about unit peaks
            m_gPrevFlow = fl;
            // small speakers (fix wave S2-1): waveshape the pulses before the formants (tanh: odd harmonics up the spectrum)
            if (bright > 0f) exc += bright * 1.2f * (float)Math.Tanh(4.0 * exc);
            float turb = m_breathLp.Low(m_pink2.Next(White())) * (0.15f + fl) * 0.9f;
            // F2 / F3 +7 dB for the bright voice (0.7 -> 1.57, 0.4 -> 0.9)
            float g2 = 0.7f + 1.1f * bright, g3 = 0.4f + 1.2f * bright;
            float v = m_f1.BandNorm(exc) + g2 * m_f2.BandNorm(exc) + g3 * m_f3.BandNorm(exc);
            v += 0.6f * (m_t1.BandNorm(turb) + (0.6f + 0.6f * bright) * m_t2.BandNorm(turb) + (0.35f + 0.45f * bright) * m_t3.BandNorm(turb));
            v += (0.12f - 0.05f * bright) * m_bodyLp.Low(exc);   // fundamental and the first harmonics (the weight)
            if (harm > 0f) v += harm * m_harm.BandNorm(exc);   // the 80-300 Hz harmonics (what a phone plays of the voice)
            m_amPh += m_amHz / fs; if (m_amPh >= 1.0) m_amPh -= 1.0;
            float am = 1f - (float)amDepth * (0.5f + 0.5f * SineTable.Lookup(m_amPh));
            return v * am;
        }

        /// <summary>Random swells for the water (new target every 0.5-2 s, smoothed).</summary>
        float Swell(double t)
        {
            if (t >= m_swellNext)
            {
                m_swellTarget = 0.35f + 0.65f * m_rng.Next01();
                m_swellNext = t + 0.5 + 1.5 * m_rng.NextDouble01();
            }
            m_swell += (float)(1.5 / m_fs) * (m_swellTarget - m_swell);
            return m_swell;
        }

        // ------------------------------------------------------------------------------------------- the per-kind programs
        /// <summary>One mono sample of the dry synthesis (before the calibrated gain); advances the voice.</summary>
        public float Next()
        {
            int n = N++;
            if (n < Delay) return 0f;
            double t = (n - Delay) / m_fs, D = m_dur;
            if (t >= D + 0.05) { Active = false; return 0f; }
            switch (Kind)
            {
                case KrakenRecipe.Call: return CallSample(t, D, false);
                case KrakenRecipe.Stinger: return StingerSample(t, D);
                case KrakenRecipe.Strike: return StrikeSample(t, D);
                case KrakenRecipe.SurfaceRoar: return RoarSample(t, D);
                case KrakenRecipe.Slap: return SlapSample(t, D);
                case KrakenRecipe.DamShudder: return ShudderSample(t, D);
                case KrakenRecipe.GlassCreak: return GlassSample(t, D);
                case KrakenRecipe.SuckerPops: return SuckerSample(t, D);
                default: return EyeSample(t, D);
            }
        }

        float CallSample(double t, double D, bool deep)
        {
            double env = Env(t, D, 1.5, 3.0);
            if (env <= 0.0) return 0f;
            double tPeak = 0.35 * D;
            // f0: 24 -> 30 Hz in the first second, then down to 21 Hz by 7/8 of the event (deep: 0.85 x)
            double f0 = t < 1.0 ? 24.0 + 6.0 * BwMath.Smoothstep(0.0, 1.0, t) : 30.0 - 9.0 * BwMath.Smoothstep(1.0, 0.875 * D, t);
            if (deep) f0 *= 0.85;
            bool chaos = t >= tPeak && t < tPeak + 1.5;
            double jitter = chaos ? 0.30 : 0.07;
            double sub = deep ? 0.6 : 0.8 * BwMath.Smoothstep(tPeak, tPeak + 1.5, t) * (1.0 - 0.4 * BwMath.Smoothstep(tPeak + 1.5, D, t));
            double amDepth = 0.35 + 0.25 * Bell(t, tPeak + 0.75, 1.2);
            double u = Math.Sin(Math.PI * Math.Min(1.0, t / (0.9 * D)));   // oo -> aa -> oo
            float v = Voice(f0, jitter, sub, amDepth, u * u, Underwater, Underwater ? 0f : 1f, Underwater ? 0f : 0.5f);
            double bodyEnv = Env(t, D, 3.0, 3.0);
            float b = Body(1.0) * (float)((Underwater ? 0.15 : 0.08) * bodyEnv);
            float c = Creak(t, 6.0 + 24.0 * t / D, 0.25f) * 0.5f;
            double crest = Underwater ? 1.0 : 1.0 + 0.75 * Bell(t, tPeak + 0.6, 1.1);
            return (float)(env * crest) * (v + c) + b;
        }

        /// <summary>The final stinger (fix wave S2-2): call phrases every 10 s over a sustained groan (never under 0.4 of the
        /// phrase level), deep (0.85 f0, the subharmonic from the start), bright (waveshaped, F2/F3 up, the 80-300 Hz harmonic
        /// layer: what a laptop plays of it) and flesh creaks swelling with the phrases; the sub body kept as support.</summary>
        float StingerSample(double t, double D)
        {
            double env = Env(t, D, 2.0, 4.0);
            if (env <= 0.0) return 0f;
            const double P = 10.0;
            int k = (int)(t / P);
            double tp = t - k * P;
            double len = Math.Max(4.0, Math.Min(P, D - k * P));
            double phrase = 0.4 + 0.6 * Env(tp, len, 1.2, 2.5);
            double f0p = tp < 1.0 ? 24.0 + 6.0 * BwMath.Smoothstep(0.0, 1.0, tp) : 30.0 - 9.0 * BwMath.Smoothstep(1.0, 0.875 * P, tp);
            double f0 = 0.85 * (19.0 + (f0p - 19.0) * (phrase - 0.4) / 0.6);
            double tPeak = 0.35 * P;
            bool chaos = tp >= tPeak && tp < tPeak + 1.5;
            double amDepth = 0.35 + 0.25 * Bell(tp, tPeak + 0.75, 1.2);
            double u = Math.Sin(Math.PI * Math.Min(1.0, tp / (0.9 * P)));
            float v = Voice(f0, chaos ? 0.3 : 0.07, 0.6, amDepth, u * u * phrase, false, 1f, 1f);
            float b = Body(0.85) * (float)(0.08 * Env(t, D, 3.0, 4.0));
            float c = Creak(t, 3.0 + 9.0 * (phrase - 0.4) / 0.6, 0.45f) * 0.7f;
            return (float)env * ((float)phrase * v + c) + b;
        }

        /// <summary>The strike on the window gallery's glass (fix wave S3-4; s 1300): concrete modes struck then ground, the 31 Hz
        /// pane boom, the 230/517 Hz glass ring and the KG crack at +0.4 s (2.1 / 3.4 kHz, Q 30).</summary>
        float StrikeSample(double t, double D)
        {
            if (t >= D) return 0f;
            double fs = m_fs;
            double att = t < 0.004 ? t / 0.004 : 1.0;
            double fadeOut = t > D - 1.0 ? 0.5 + 0.5 * Math.Cos(Math.PI * (t - (D - 1.0)) / 1.0) : 1.0;
            bool first = N - Delay == 1;
            // the pane boom
            m_ph0 += 31.0 / fs; if (m_ph0 >= 1.0) m_ph0 -= 1.0;
            float boom = (float)(1.0 * att * Math.Exp(-6.9078 * t / 2.5)) * SineTable.Lookup(m_ph0);
            // concrete: struck, then the stick-slip grinding of the structure (faster than the shudder: it is close)
            double groanEnv = Env(t, D, 0.3, 1.0);
            float creak = 0f;
            if (t >= m_creakNext)
            {
                creak = (float)groanEnv * 0.5f * (0.4f + 0.6f * m_rng.Next01()) * (m_rng.Next01() < 0.5f ? -1f : 1f);
                double rate = 8.0 + 22.0 * BwMath.Smoothstep(0.0, 0.6 * D, t);
                m_creakNext = t + 1.0 / (rate * (1.0 + 0.15 * White()));
            }
            float x = (first ? 1.5f : 0f) + creak;
            float modes = (0.6f * m_m0.Tick(x) + 1.5f * m_m1.Tick(x) + 3.0f * m_m2.Tick(x) + 1.3f * m_m3.Tick(x)) * 1.2f;
            if (x != 0f) m_grindEnv = Math.Min(1f, m_grindEnv + Math.Abs(x));
            m_grindEnv *= m_grindDecay;
            float grind = 1.2f * (float)groanEnv * m_grindEnv * m_grindBp.BandNorm(White());
            // the KG crack: a burst of clicks over 40 ms from +0.4 s into the 2.1 / 3.4 kHz modes, plus 8 ms of HP noise
            float cx = 0f;
            if (t >= 0.4 && t < 0.44 && m_rng.Next01() < (float)(150.0 / fs)) cx = (0.6f + 0.4f * m_rng.Next01()) * (m_rng.Next01() < 0.5f ? -1f : 1f);
            if (t >= 0.4 && t < 0.4 + 1.0 / fs) cx = 1f;
            float crack = 1.6f * (m_c0.Tick(cx) + 0.8f * m_c1.Tick(cx));
            if (t >= 0.4 && t < 0.408) crack += 0.35f * m_crackHp.High(m_pink.Next(White())) * (float)(1.0 - (t - 0.4) / 0.008);
            // the glass rings: struck at 0, struck again by the crack
            float gx = (first ? 0.9f : 0f) + 0.5f * cx;
            float glass = 0.9f * (m_g0.Tick(gx) + 0.8f * m_g1.Tick(gx));
            float rumble = (float)(0.5 * Math.Exp(-6.9078 * t / 2.0)) * m_rumbleLp.Low(m_brown.Next(White()));
            return (float)fadeOut * (boom + modes + grind + crack + glass + rumble);
        }

        float RoarSample(double t, double D)
        {
            double env = Env(t, D, 1.0, 3.0);
            if (env <= 0.0) return 0f;
            float sw = Swell(t);
            float br = m_brown.Next(White());
            float pk = m_pink.Next(White());
            sw = sw * sw * 1.4f;   // deeper swells: the loudest second stands out of the sea
            float roar = m_wLp.Low(br) * 0.1f + 1.6f * m_wMid.BandNorm(pk) + 0.5f * m_wBp.BandNorm(pk)
                       + 1.1f * m_wHi.BandNorm(pk);   // the 600-1500 Hz pink roar (small speakers, fix wave S2-1)
            float sheet = m_wSheet.Low(br) * 0.1f;
            float groan = Voice(22.0 + 4.0 * Math.Sin(t * 0.7), 0.1, 0.5, 0.45, 0.3, true, 1f, 0.4f) * 0.5f;
            float c = Creak(t, 4.0 + 10.0 * t / D, 0.35f) * 0.6f;
            float b = Body(0.95) * 0.12f;
            // the break crests early (2 s), then the cascade subsides 6 dB from 3.5 to 8 s: a long roar (K2 / K4 rises and sinks,
            // 28-30 s) is not a 30 s wall at full level, and the K5 slap 4.6 s into the K4 sink lands on a falling roar
            double crest = (1.0 + 0.85 * Bell(t, Math.Min(2.0, 0.3 * D), 0.8)) * (1.0 - 0.5 * BwMath.Smoothstep(3.5, 8.0, t));
            return (float)(env * crest) * (sw * roar + sheet + groan + c + b);
        }

        float SlapSample(double t, double D)
        {
            if (t >= D) return 0f;
            double fs = m_fs;
            double att = t < 0.005 ? 0.5 - 0.5 * Math.Cos(Math.PI * t / 0.005) : 1.0;
            double fadeOut = t > D - 0.5 ? 0.5 + 0.5 * Math.Cos(Math.PI * (t - (D - 0.5)) / 0.5) : 1.0;
            // low tones 28 + 44 Hz, T60 2.5 s
            m_ph0 += 28.0 / fs; if (m_ph0 >= 1.0) m_ph0 -= 1.0;
            m_ph1 += 44.0 / fs; if (m_ph1 >= 1.0) m_ph1 -= 1.0;
            double eTone = Math.Exp(-6.9078 * t / 2.5);
            float tone = (float)(0.45 * eTone * (SineTable.Lookup(m_ph0) + 0.8 * SineTable.Lookup(m_ph1)));
            // the audible body thud: 200 -> 70 Hz over 0.25 s, T60 0.7 s
            double fThud = 70.0 + 130.0 * Math.Exp(-t / 0.08);
            m_ph2 += fThud / fs; if (m_ph2 >= 1.0) m_ph2 -= 1.0;
            double eThud = Math.Exp(-6.9078 * t / 0.7);
            double ph2b = m_ph2 * 2.0, ph3b = m_ph2 * 3.0;
            float thud = (float)(2.4 * eThud * SineTable.Lookup(m_ph2));
            // small speakers (fix wave S2-1): the thud's 2nd and 3rd harmonics (140-600 Hz) and an 80 ms 300-1200 Hz body crack
            float thudH = (float)(eThud * (1.6 * SineTable.Lookup(ph2b - Math.Floor(ph2b)) + 1.2 * SineTable.Lookup(ph3b - Math.Floor(ph3b))));
            double eBody = t < 0.002 ? t / 0.002 : Math.Exp(-6.9078 * (t - 0.002) / 0.08);
            float body = (float)(5.0 * eBody) * m_slapBody.BandNorm(White());
            // 3 ms crack (pink HP 1 kHz)
            float w = White();
            float crack = t < 0.015 ? (float)(1.5 * (1.0 - t / 0.015)) * m_crackHp.High(m_pink.Next(w)) : 0f;
            // spray hiss pink BP 2 kHz (attack 30 ms, T60 1.5 s) and the brown LP 300 Hz tail (T60 3 s)
            double eSpray = (t < 0.03 ? t / 0.03 : 1.0) * Math.Exp(-6.9078 * t / 1.5);
            float spray = (float)(4.0 * eSpray) * m_sprayBp.BandNorm(m_pink2.Next(White()));
            double eTail = (t < 0.05 ? t / 0.05 : 1.0) * Math.Exp(-6.9078 * t / 3.0);
            float tail = (float)(0.6 * eTail) * m_tailLp.Low(m_brown.Next(w));
            return (float)(att * fadeOut) * (tone + thud + thudH + body + crack + spray + tail);
        }

        float ShudderSample(double t, double D)
        {
            if (t >= D) return 0f;
            double fs = m_fs;
            double att = t < 0.01 ? t / 0.01 : 1.0;
            double fadeOut = t > D - 0.8 ? 0.5 + 0.5 * Math.Cos(Math.PI * (t - (D - 0.8)) / 0.8) : 1.0;
            m_ph0 += 24.0 / fs; if (m_ph0 >= 1.0) m_ph0 -= 1.0;
            m_ph1 += 31.0 / fs; if (m_ph1 >= 1.0) m_ph1 -= 1.0;
            float tone = (float)(0.45 * Math.Exp(-6.9078 * t / 4.0) * (SineTable.Lookup(m_ph0) + 0.8 * SineTable.Lookup(m_ph1)));
            float w = White();
            float rumble = (float)(0.8 * Math.Exp(-6.9078 * t / 3.0)) * m_rumbleLp.Low(m_brown.Next(w));
            // the strike on the concrete modes (an impulse at t 0) then the grinding groan of the structure
            float strike = N - Delay == 1 ? 1.5f : 0f;
            double rate = 6.0 + 24.0 * BwMath.Smoothstep(0.0, 0.7 * D, t);
            double groanEnv = Env(t, D, 0.6, 1.2);
            float creak = 0f;
            if (t >= m_creakNext)
            {
                creak = (float)groanEnv * (0.4f + 0.6f * m_rng.Next01()) * (m_rng.Next01() < 0.5f ? -1f : 1f);
                m_creakNext = t + 1.0 / (rate * (1.0 + 0.15 * White()));
            }
            float x = strike + creak;
            // the groan carries it on small speakers: the 310 Hz mode +6 dB (fix wave S2-1)
            float modes = (0.6f * m_m0.Tick(x) + 1.5f * m_m1.Tick(x) + 3.4f * m_m2.Tick(x) + 7.0f * m_m3.Tick(x)) * 1.7f;
            // 600-900 Hz grinding: band noise gated by the stick-slip (each slip opens it, 30 ms decay)
            if (x != 0f) m_grindEnv = Math.Min(1f, m_grindEnv + Math.Abs(x));
            m_grindEnv *= m_grindDecay;
            float grind = 3.0f * (float)groanEnv * m_grindEnv * m_grindBp.BandNorm(White());
            // the blow itself: a 30 ms concrete crack (pink HP 1 kHz) and a 0.3 s burst of the 600-900 Hz grinding
            if (t < 0.05) grind += (float)(6.0 * (1.0 - t / 0.05)) * m_crackHp.High(m_pink.Next(White()));
            if (t < 0.6) grind += (float)(8.0 * Math.Exp(-t / 0.12)) * m_grindBp.BandNorm(White());
            // dust trickle: sparse 2-6 kHz grains over 4 s
            float dust = 0f;
            if (t < 4.0 && m_rng.Next01() < 0.0015f) m_grainN = 0;
            if (m_grainN < 96) { dust = 0.3f * m_dustBp.BandNorm(White()) * (1f - m_grainN / 96f); m_grainN++; }
            return (float)(att * fadeOut) * (tone + rumble + modes + grind + dust);
        }

        float GlassSample(double t, double D)
        {
            double env = Env(t, D, 1.2, 2.0);
            if (env <= 0.0) return 0f;
            double rate = 6.0 + 24.0 * BwMath.Smoothstep(0.0, 0.8 * D, t);
            float glass = Creak(t, rate, 0.8f) * 0.9f;
            float growl = Voice(23.0 + 3.0 * Math.Sin(0.9 * t), 0.09, 0.5, 0.5, 0.25 + 0.3 * Bell(t, 0.5 * D, 0.3 * D), true) * 0.55f;
            // sparse clicks (2 ms, 3-8 /s)
            float click = 0f;
            if (t >= m_clickNext) { m_clickN = 0; m_clickNext = t + 1.0 / (3.0 + 5.0 * m_rng.Next01()); }
            if (m_clickN < 96) { click = 0.6f * m_popClickLp.Low(White()) * (1f - m_clickN / 96f); m_clickN++; }
            float b = Body(1.0) * 0.3f;
            return (float)env * (glass + growl + click + b);
        }

        float SuckerSample(double t, double D)
        {
            double env = Env(t, D, 0.5, 1.5);
            if (env <= 0.0) return 0f;
            double fs = m_fs;
            // thocks at 0.8 -> 2.5 /s, the travelling wave sweeps the pan offset from -0.6 to +0.6
            if (t >= m_popNext)
            {
                double rate = 0.8 + 1.7 * BwMath.Smoothstep(0.0, 0.7 * D, t);
                m_popNext = t + 1.0 / (rate * (1.0 + 0.2 * White()));
                m_popN = 0; m_chirpPh = 0.0;
                m_popAmp = 0.7f + 0.3f * m_rng.Next01();
                m_popPan = (float)(-0.6 + 1.2 * t / D);
            }
            float pop = 0f;
            if (m_popN < (int)(0.3 * fs))
            {
                double tp = m_popN / fs;
                float w = White();
                float click = tp < 0.003 ? 2.5f * m_popClickLp.Low(w) : 0f;
                double fChirp = 70.0 + 80.0 * Math.Exp(-tp / 0.012);
                m_chirpPh += fChirp / fs; if (m_chirpPh >= 1.0) m_chirpPh -= 1.0;
                float burst = tp < 0.035 ? (float)(1.2 * (1.0 - tp / 0.035)) * SineTable.Lookup(m_chirpPh) * (0.7f + 0.3f * w) : 0f;
                m_ph4 += 70.0 / fs; if (m_ph4 >= 1.0) m_ph4 -= 1.0;
                float thump = (float)(1.3 * Math.Exp(-6.9078 * tp / 0.06)) * SineTable.Lookup(m_ph4);
                float hiss = (float)(0.35 * Math.Exp(-6.9078 * tp / 0.25)) * m_popHissHp.High(m_pink.Next(White()));
                pop = m_popAmp * (click + burst + thump + hiss);
                m_popN++;
            }
            // hook scrape: 20 ms grains of 600-2400 Hz noise at 25 /s while sliding (the middle of the event)
            double slide = Bell(t, 0.55 * D, 0.25 * D);
            if (t >= m_grainNext)
            {
                m_grainNext = t + 0.04 * (0.7 + 0.6 * m_rng.Next01());
                m_grainN = 0; m_grainLen = (int)(0.02 * fs);
                m_grainBp.Set(600.0 + 1800.0 * m_rng.Next01(), 2.5, fs);
                m_grainAmp = (float)(0.25 * slide * (0.5 + 0.5 * m_rng.Next01()));
            }
            float grain = 0f;
            if (m_grainN < m_grainLen)
            {
                float wv = (float)(0.5 - 0.5 * Math.Cos(2.0 * Math.PI * m_grainN / Math.Max(1, m_grainLen - 1)));
                grain = m_grainAmp * wv * m_grainBp.BandNorm(White());
                m_grainN++;
            }
            float glass = Creak(t, 3.0 + 6.0 * t / D, 0.3f) * 0.5f;
            return (float)env * (pop + grain + glass);
        }

        /// <summary>The pan offset of the current thock (sucker travelling wave); 0 for the other kinds.</summary>
        public float PanOffset => Kind == KrakenRecipe.SuckerPops ? m_popPan : 0f;

        float EyeSample(double t, double D)
        {
            // 2 s of silence, then the swell (attack 4 s, release 4 s)
            const double Hush = 2.0;
            double te = t - Hush, De = D - Hush;
            double env = Env(te, De, 4.0, 4.0);
            if (env <= 0.0) return 0f;
            double f0 = 21.0 + 4.0 * BwMath.Smoothstep(0.0, 0.5 * De, te) - 2.0 * BwMath.Smoothstep(0.5 * De, De, te);
            float growl = Voice(f0, 0.12, 0.9, 0.55, 0.35 + 0.3 * Bell(te, 0.5 * De, 0.3 * De), true) * 1.0f;
            // vibrating body: the KB partials under a 40-60 Hz roughness (the same AM phase as the voice)
            float body = Body(1.0) * 0.5f * (0.9f - 0.5f * (0.5f + 0.5f * SineTable.Lookup(m_amPh)));
            float glass = Creak(te, 3.0 + 7.0 * te / De, 0.5f) * 0.6f;
            return (float)env * (growl + body + glass);
        }

        /// <summary>Per-block flush of the filter states (denormals).</summary>
        public void Flush()
        {
            m_f1.Flush(); m_f2.Flush(); m_f3.Flush(); m_t1.Flush(); m_t2.Flush(); m_t3.Flush(); m_bodyLp.Flush(); m_breathLp.Flush();
            m_m0.Flush(); m_m1.Flush(); m_m2.Flush(); m_m3.Flush();
            m_wLp.Flush(); m_wBp.Flush(); m_wSheet.Flush(); m_wMid.Flush(); m_popClickLp.Flush(); m_popHissHp.Flush(); m_grainBp.Flush();
            m_crackHp.Flush(); m_sprayBp.Flush(); m_tailLp.Flush(); m_rumbleLp.Flush(); m_dustBp.Flush();
            m_harm.Flush(); m_wHi.Flush(); m_slapBody.Flush(); m_grindBp.Flush();
            m_g0.Flush(); m_g1.Flush(); m_c0.Flush(); m_c1.Flush();
            if (Math.Abs(m_grindEnv) < 1e-20f) m_grindEnv = 0f;
            Air1.Flush(); Air2.Flush(); Behind.Flush();
            m_pink.Flush(); m_pink2.Flush();
        }

        /// <summary>Spatialised stereo sample: dry mono through the air / glass / wall low-passes and the equal-power pan, plus
        /// the reverb send. smooth = per-sample one-pole coefficient of the spatial gains.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Render(float smooth, ref float l, ref float r, ref float send, out float far)
        {
            float x = Next() * Gain;
            if (SnapSpatial) { m_gl = TgtL; m_gr = TgtR; m_send = TgtSend; m_far = TgtFar; SnapSpatial = false; }
            m_gl += smooth * (TgtL - m_gl); m_gr += smooth * (TgtR - m_gr); m_send += smooth * (TgtSend - m_send);
            m_far += smooth * (TgtFar - m_far);
            m_kduck += (TgtKDuck > m_kduck ? 0.05f * smooth : smooth) * (TgtKDuck - m_kduck);   // in 50 ms, back over 1 s
            float y = Air2.Low(Air1.Low(x * m_kduck));
            if (BehindMix > 0f) y += BehindMix * (Behind.Low(y) - y);
            l += m_gl * y; r += m_gr * y;
            send += m_send * y;
            far = m_far * y;
        }
    }

    /// <summary>
    /// The kraken bus: voice pool, spatialiser (per 32-sample block), its own FDN, HP 15 Hz, look-ahead limiter (-6 dBFS) and the
    /// bed duck. Main thread: Trigger (lock-free ring). Audio thread: Control per block with the listener pose, Tick per sample.
    /// </summary>
    public sealed class KrakenBus
    {
        public const int MaxVoices = 6;
        readonly double m_fs;
        readonly KrakenVoice[] m_v = new KrakenVoice[MaxVoices];
        readonly KrakenEventRing m_ring = new KrakenEventRing();
        readonly FdnReverb8 m_reverb;
        readonly DspLimiter m_limiter = new DspLimiter();
        SvfTpt m_hpL, m_hpR;
        readonly float m_spatialSmooth;
        readonly int m_seed;
        uint m_counter;
        float m_duck = 1f;              // current bed duck gain (linear)
        readonly float m_duckAtt, m_duckRel;   // dB per sample
        readonly int m_sleepAfter;
        float m_duckDb;
        int m_ctx = -1;
        float m_duckDbLast;
        int m_idle = int.MaxValue / 2;   // samples since a voice was last active (the bus sleeps after 12 s: tails rung out)
        // the far tail (fix wave S2-1): each call voice writes its far send d / 343 s ahead into this line (scatter write, fixed
        // per voice: no pitch glide); the line is read into the FDN input, so the voice returns from kilometres away
        readonly float[] m_farBuf, m_farBufR;   // L and R: the far return comes back from the source's side
        readonly int m_farLen;
        int m_farW;
        readonly float m_duckAttFast;
        public int Started { get; private set; }
        public DspLimiter Limiter => m_limiter;
        /// <summary>Linear gain the beds take this sample (1 = no duck).</summary>
        public float DuckGain => m_duck;
        public int ActiveCount { get { int c = 0; for (int i = 0; i < MaxVoices; i++) if (m_v[i].Active) c++; return c; } }

        /// <summary>T60 of the outdoor bus reverb (s): 5 s with the far tail (fix wave S2-1: 4-6 s, kilometres wide).</summary>
        public const double OutdoorT60 = 5.0;

        public KrakenBus(double fs, int seed)
        {
            m_fs = fs; m_seed = seed;
            for (int i = 0; i < MaxVoices; i++) m_v[i] = new KrakenVoice(fs);
            // a big room: delays x 3.2 of the base primes (the "enormous" outdoor reverb, the window hall)
            m_reverb = new FdnReverb8(fs, 3.2, BwAudioSpec.ReverbMaxPreDelay);
            m_reverb.BlendSeconds = 1.0;
            m_reverb.SetTargets(OutdoorT60, 70.0, -6.0, 3000.0);
            m_reverb.Snap();
            m_farLen = (int)((KrakenRecipe.FarTailMaxSeconds + 0.5) * fs);
            m_farBuf = new float[m_farLen];
            m_farBufR = new float[m_farLen];
            m_duckAttFast = (float)(1.0 / (KrakenRecipe.DuckAttackImpactSeconds * fs));
            m_limiter.Configure(fs, KrakenRecipe.BusLimiterThresholdDb, KrakenRecipe.BusLimiterCeilingDb, KrakenRecipe.BusLimiterLookaheadMs, KrakenRecipe.BusLimiterReleaseMs);
            m_hpL.Set(KrakenRecipe.BusHighPassHz, 0.7071, fs); m_hpR.Set(KrakenRecipe.BusHighPassHz, 0.7071, fs);
            m_spatialSmooth = (float)(1.0 - Math.Exp(-1.0 / (0.05 * fs)));
            m_duckAtt = (float)(1.0 / (KrakenRecipe.DuckAttackSeconds * fs));
            m_sleepAfter = (int)(12.0 * fs);
            m_duckRel = (float)(1.0 / (KrakenRecipe.DuckReleaseSeconds * fs));
        }

        /// <summary>Main thread (or the offline renderer): queue one event.</summary>
        public bool Trigger(in KrakenEvent e) => m_ring.Push(e);

        /// <summary>Audio thread, once per block: start queued voices and update every voice's spatial targets.</summary>
        public void Control(int n, double lx, double ly, double lz, double lyawDeg, bool listenerIndoors, double bedDb = -120.0)
        {
            while (m_ring.Pop(out KrakenEvent e)) StartVoice(e);
            // an impact in its first ImpactVoiceDuckHold seconds ducks the other voices (it punches through the roar)
            bool impactNow = false;
            for (int i = 0; i < MaxVoices; i++)
            {
                var vi = m_v[i];
                if (vi.Active && KrakenRecipe.Impact(vi.Kind) && vi.Time > -KrakenRecipe.ImpactVoiceDuckLead && vi.Time < KrakenRecipe.ImpactVoiceDuckHold) { impactNow = true; break; }
            }
            float kd = (float)BwAudioSpec.Db(-KrakenRecipe.ImpactVoiceDuckDb);
            for (int i = 0; i < MaxVoices; i++)
            {
                var vi = m_v[i];
                if (!vi.Active) continue;
                vi.TgtKDuck = impactNow && !KrakenRecipe.Impact(vi.Kind) ? kd : 1f;
            }
            int ctx = 0;   // 0 outdoors, 1 under water (beat 6), 2 through the wall
            for (int i = 0; i < MaxVoices; i++)
            {
                var v = m_v[i];
                if (!v.Active) continue;
                if (v.Underwater || v.Kind == KrakenRecipe.Strike) ctx = 1;
                else if (listenerIndoors && ctx == 0) ctx = 2;
                double dx = v.X - lx, dy = v.Y - ly, dz = v.Z - lz;
                double d = Math.Sqrt(dx * dx + dy * dy + dz * dz);
                double bearing = Math.Atan2(dx, dz) * (180.0 / Math.PI);
                double rel = (bearing - lyawDeg) * (Math.PI / 180.0);
                double pan = Math.Sin(rel), front = Math.Cos(rel);
                double fc, behindMix = 0.0, panScale = 1.0, sendDb;
                bool twoPole;
                if (v.Underwater) { fc = 450.0; twoPole = true; sendDb = -3.0; }
                else if (v.Kind == KrakenRecipe.Strike) { fc = 9000.0; twoPole = false; sendDb = -2.0; }   // in the hall: no wall, the hall reverb
                else if (listenerIndoors) { fc = 320.0; twoPole = true; panScale = 0.0; sendDb = -8.0; }
                else
                {
                    fc = KrakenRecipe.AirLowpassHz(d, v.Kind); twoPole = false; sendDb = -4.0 + 3.0 * Math.Min(1.0, d / 1500.0);
                    behindMix = front < 0.0 ? -front : 0.0;
                }
                bool far = KrakenRecipe.FarTail(v.Kind) && !v.Underwater && !listenerIndoors;
                v.TgtFar = far ? (float)BwAudioSpec.Db(KrakenRecipe.FarTailSendDb) : 0f;
                if (v.FarDelay < 0)
                    v.FarDelay = (int)(Math.Max(KrakenRecipe.FarTailMinSeconds, Math.Min(KrakenRecipe.FarTailMaxSeconds, d / KrakenRecipe.SpeedOfSound)) * m_fs);
                double p = Math.Max(-1.0, Math.Min(1.0, panScale * pan + v.PanOffset));
                double lev = BwAudioSpec.Db(-2.0 * behindMix);
                v.TgtL = (float)(lev * Math.Sqrt(0.5 * (1.0 - p)));
                v.TgtR = (float)(lev * Math.Sqrt(0.5 * (1.0 + p)));
                v.TgtSend = (float)BwAudioSpec.Db(sendDb);
                v.BehindMix = (float)behindMix;
                // the filters are re-set per block (the distance changes slowly); the two-pole pair = 24 dB/oct
                v.Air1.Set(fc, 0.7071, m_fs);
                v.Air2.Set(twoPole ? fc : 20000.0, 0.7071, m_fs);
                v.Behind.Set(2500.0, 0.7071, m_fs);
                // the gain follows the distance (level law of KrakenRecipe), fixed per event in Start; re-evaluate here
                double floorDb = bedDb - KrakenRecipe.DuckDb[v.Kind] + KrakenRecipe.AudibilityMarginDb + 10.0 * Math.Log10(Math.Max(1e-3, v.Strength));
                double levelDb = Math.Max(KrakenRecipe.LevelDb(v.Kind, d, v.Strength, v.Underwater), floorDb)
                               + (v.Underwater && v.Kind == KrakenRecipe.Call ? KrakenRecipe.NormDbUnderwaterCall : KrakenRecipe.NormDb[v.Kind]);
                v.Gain = (float)BwAudioSpec.Db(levelDb);
            }
            if (ctx != m_ctx)
            {
                m_ctx = ctx;
                if (ctx == 1) m_reverb.SetTargets(5.5, 25.0, -3.0, 900.0);        // the window hall, under water (HF ratio 0.35)
                else if (ctx == 2) m_reverb.SetTargets(2.4, 15.0, -9.0, 1200.0);  // through the wall
                else m_reverb.SetTargets(OutdoorT60, 70.0, -6.0, 3000.0);         // outdoors: an enormous space over the sea
            }
            m_reverb.Control(n);
        }

        void StartVoice(in KrakenEvent e)
        {
            int slot = -1;
            double oldest = -1.0;
            for (int i = 0; i < MaxVoices; i++)
            {
                if (!m_v[i].Active) { slot = i; break; }
                double age = m_v[i].Time / Math.Max(0.1, m_v[i].Duration);
                if (age > oldest) { oldest = age; slot = i; }   // steal the voice nearest its end
            }
            m_counter++;
            uint seed = XorShift32.Stream(m_seed, 0x4B52u + m_counter).NextUInt();
            m_v[slot].Start(e, seed, 0f);
            m_idle = 0;
            Started++;
        }

        /// <summary>One stereo sample of the bus (after the HP and the limiter). master = the world fade (UI); the stinger
        /// ignores it (it sounds over the end card).</summary>
        public void Tick(float master, out double l, out double r)
        {
            if (m_idle > m_sleepAfter && m_duckDb <= 0f) { l = 0.0; r = 0.0; return; }   // asleep: no voice, tails gone
            float dl = 0f, dr = 0f, send = 0f, sl = 0f, sr = 0f, ssend = 0f;
            float duckDb = 0f;
            bool fast = false;
            if (m_idle < int.MaxValue - 1) m_idle++;
            for (int i = 0; i < MaxVoices; i++)
            {
                var v = m_v[i];
                if (!v.Active) continue;
                m_idle = 0;
                // the duck covers the pre-roll's last 1.5 s and the event (2.6); it hands back over the voice's last 2.5 s (the
                // voice's own release), so the bed returns as the call fades instead of leaving a hole after it
                // impacts (slap, shudder, strike) duck from their arrival, fast (fix wave S2-1); the voices from 1.5 s before
                double tv = v.Time;
                bool imp = KrakenRecipe.Impact(v.Kind);
                if (tv > (imp ? 0.0 : -KrakenRecipe.DuckAttackSeconds) && tv < v.Duration)
                {
                    float hand = (float)Math.Min(1.0, Math.Max(0.0, (v.Duration - tv) / 2.5));
                    float dd = (float)KrakenRecipe.DuckDb[v.Kind] * Math.Min(1f, 0.4f + v.Strength) * hand;
                    if (dd > duckDb) { duckDb = dd; fast = imp; }
                }
                float fv;
                bool sting = v.Kind == KrakenRecipe.Stinger;
                if (sting) v.Render(m_spatialSmooth, ref sl, ref sr, ref ssend, out fv);
                else v.Render(m_spatialSmooth, ref dl, ref dr, ref send, out fv);
                if (fv != 0f && v.FarDelay >= 0)
                {
                    int idx = m_farW + Math.Min(v.FarDelay, m_farLen - 1);
                    if (idx >= m_farLen) idx -= m_farLen;
                    float fg = sting ? fv : master * fv;
                    m_farBuf[idx] += fg * v.TgtL;
                    m_farBufR[idx] += fg * v.TgtR;
                }
            }
            float farL = m_farBuf[m_farW], farR = m_farBufR[m_farW];
            m_farBuf[m_farW] = 0f; m_farBufR[m_farW] = 0f;
            if (++m_farW >= m_farLen) m_farW = 0;
            // duck in dB: attack over 1.5 s (impacts 0.25 s), release over 3 s (full depth)
            if (duckDb > m_duckDb) m_duckDb = Math.Min(duckDb, m_duckDb + (fast ? m_duckAttFast : m_duckAtt) * 10f);
            else m_duckDb = Math.Max(duckDb, m_duckDb - m_duckRel * 10f);
            if (m_duckDb != m_duckDbLast) { m_duck = (float)Math.Pow(10.0, -m_duckDb / 20.0); m_duckDbLast = m_duckDb; }
            // the far return: a panned, distant echo (-6 dB) and the FDN input (the wall of sound kilometres away)
            float inL = master * dl + sl + 0.5f * farL, inR = master * dr + sr + 0.5f * farR, sIn = master * send + ssend;
            m_reverb.Tick(sIn + farL, sIn + farR, out float wl, out float wr);
            double ol = m_hpL.High(inL + wl), or = m_hpR.High(inR + wr);
            m_limiter.Process(ref ol, ref or);
            l = ol; r = or;
        }

        public void Flush()
        {
            for (int i = 0; i < MaxVoices; i++) if (m_v[i].Active) m_v[i].Flush();
            m_reverb.Flush(); m_hpL.Flush(); m_hpR.Flush();
        }

        public void Clear()
        {
            for (int i = 0; i < MaxVoices; i++) m_v[i].Active = false;
            Array.Clear(m_farBuf, 0, m_farLen); Array.Clear(m_farBufR, 0, m_farLen);
            m_reverb.Clear(); m_hpL.Clear(); m_hpR.Clear();
        }
    }
}
