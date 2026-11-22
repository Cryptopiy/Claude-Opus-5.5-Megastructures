// DreadScore: the score of the D-034 dread layer as pure functions of progress s (m) and time (docs/DREAD.md section 1 and
// the sound cues of sections 2-6; docs/AUDIO.md section 11). Everything is keyed to s; durations given in seconds by
// DREAD.md are converted to metres at the reference speed 2.5 m/s (the cinematic walk the video is cut from, 14.12), so they
// are exact in the video, proportionally longer at 1.5 m/s, and held while the player stands still.
//   1.1 echo across the Gulf: image-source taps off the face planes x_A 215.3 and x_B 815.3: A1 at 2 d_A / 343 s, B1 at
//       2 d_B / 343 s, AB2 (the face-to-face round trip, 1 200 m) at 3.50 s; gain 1 / (1 + d / 150) (d the listener-face
//       distance; for AB2 d = 600 with one more reflection loss), a low-pass that falls with the path length; at mid-Gulf the
//       arrivals are spaced by 600 m = 1.75 s (DREAD 1.1); taps under 0.03 s merge into the room (no comb filtering).
//   1.1 source events: the horn far down the rift at s 440 and s 700; metal strikes inside the faces every 25-40 s.
//   1.2 the sub-bass swells under KS04, KS09, KS10, KS13, KS17: rise 8 s (20 m), hold 1 s, decay 8 s (20 m); fundamental
//       35-55 Hz on a pad chord tone, harmonics to about 160 Hz.
//   1.3 the structural groans every 20-60 s from s 120 on, placed on the nearest large mass.
//   1.4 the silence drops 2.0 s before KS09 and KS17: rain, the city bed, the X beds and passes duck by 21 dB over 0.6 s,
//       the pad by 12 dB (after its own reverb), the wind by 4 dB, the koi by 21 dB; back over 1.5 s (fix wave C07: only
//       breath, wind and the swell remain).
//   THE ARK (D-043, Logs/ark/ARK.md, replaces the 2.1 hull): its course is mirrored from ARK.md until DR's Ark course is merged
//   (the marked block); the drone, the silence before its bow, the cover under its belly, its groans and its horn calls read it;
//   3.2 the falling panel reads DR's PanelCourse (the one source of truth, D-038 / D-040), so the creak follows the picture; 5.4 the far rumbles under the sheet lightning glows;
//   6 the tram's door chime before it leaves the terminus (T and the canyon bed duck 6 dB around it, C53); 2.2 the machine
//   hums of DR's elevator cabins and gantries (DreadCabinCourse, integration round: AU-28 / DR's request). One-shots keep 15 m
//   clear of every train / tram meet (C08). Every dread position comes from DreadCourse.cs; nothing is copied.
// Pure C# (DreadCourse's constants and functions only); the event table is built once (static), nothing allocates afterwards.
using System;
// D-043: the Ark's course (ArkCourseMirror until the merge, then DR's HullCourse: "using ArkC = Megastructure.HullCourse;")
using ArkC = Megastructure.ArkCourseMirror;

namespace Megastructure
{
    public enum DreadKind { Horn = 0, Strike = 1, Groan = 2, Creak = 3, Chime = 4, Rumble = 5 }

    /// <summary>One one-shot of the dread layer: fired when s first crosses S, heard Delay seconds later from (X, Y, Z).</summary>
    public struct DreadEvent
    {
        public double S;          // trigger progress (m)
        public DreadKind Kind;
        public int Seq;           // index in the table (seeds the voice)
        public double X, Y, Z;    // source position (m); for groans the mass is chosen at fire time (NearestMass)
        public double Hz;         // fundamental (horn, strike, groan), or the first chime note
        public double Db;         // RMS level (dBFS) of the dry sound at the listener while it sounds
        public double Delay;      // seconds between the trigger and the onset (flight time, or the glow-to-rumble lag)
        public double Seconds;    // nominal length of the sound
        public int Ark;           // 0 a fixed source; 1 the Ark's bow, 2 its stern (position, flight time, Doppler set at fire time)
    }

    public static class DreadScore
    {
        // ------------------------------------------------------------------ constants (DREAD.md, 14.4)
        public const double SoundSpeed = 343.0, RefSpeed = 2.5;
        public const double FaceAX = 215.3, FaceBX = 815.3, GulfWidth = FaceBX - FaceAX, EchoAttenD = 150.0;
        /// <summary>Second reflection loss of the face-to-face path AB2 (concrete and glass faces at grazing distance).</summary>
        public const double SecondReflectionLoss = 0.7;
        /// <summary>Taps shorter than this merge with the direct sound and the room (no comb filter at the face).</summary>
        public const double EchoMergeSeconds = 0.03, EchoFullSeconds = 0.15;
        public const int EchoTapCount = 3;
        /// <summary>The echo bus is heard inside the Gulf: smootherstep in x over 40 m at each face.</summary>
        public const double EchoFadeX0 = 195.0, EchoFadeX1 = 235.0, EchoFadeX2 = 795.0, EchoFadeX3 = 835.0;

        /// <summary>Metres of walk for a duration in seconds at the reference speed (2.5 m/s).</summary>
        public static double M(double seconds) => seconds * RefSpeed;

        // ------------------------------------------------------------------ 1.1 echo taps
        /// <summary>Listener-to-face distances (m) at x, clamped to the Gulf (outside it the bus fades out).</summary>
        public static void FaceDistances(double x, out double dA, out double dB)
        {
            double xc = NocMath.Clamp(x, FaceAX, FaceBX);
            dA = xc - FaceAX; dB = FaceBX - xc;
        }

        /// <summary>Echo tap k (0 = face A, 1 = face B, 2 = the face-to-face round trip AB2) at listener x: delay (s), gain
        /// (before the Gulf weight), low-pass cutoff (Hz) and the x of the image the echo comes from (its pan).</summary>
        public static void EchoTap(int k, double x, out double delay, out double gain, out double lpHz, out double imageX)
        {
            FaceDistances(x, out double dA, out double dB);
            double path, d;
            if (k == 0) { d = dA; path = 2.0 * dA; imageX = FaceAX; }
            else if (k == 1) { d = dB; path = 2.0 * dB; imageX = FaceBX; }
            else { d = GulfWidth; path = 2.0 * GulfWidth; imageX = dA < dB ? FaceBX : FaceAX; }
            delay = path / SoundSpeed;
            gain = 1.0 / (1.0 + d / EchoAttenD) * (k == 2 ? SecondReflectionLoss : 1.0);
            gain *= NocMath.Smootherstep(EchoMergeSeconds, EchoFullSeconds, delay);
            lpHz = EchoLowPassHz(path);
        }

        /// <summary>Low-pass of an echo path (Hz): 6 kHz / (1 + L / 250): air absorption and the faces' scattering.</summary>
        public static double EchoLowPassHz(double pathM) => 6000.0 / (1.0 + Math.Max(0.0, pathM) / 250.0);

        /// <summary>Weight of the echo bus at x (1 inside the Gulf, 0 in the canyon and beyond face B).</summary>
        public static double EchoWeight(double x) => NocMath.Smootherstep(EchoFadeX0, EchoFadeX1, x) * (1.0 - NocMath.Smootherstep(EchoFadeX2, EchoFadeX3, x));

        /// <summary>The longest echo delay the bus has to hold (s).</summary>
        public static double MaxEchoSeconds => 2.0 * GulfWidth / SoundSpeed;

        // ------------------------------------------------------------------ 1.2 sub-bass swells
        public static readonly string[] SwellNames = { "KS04", "KS09", "KS10", "KS13", "KS17" };
        public static readonly double[] SwellS = { 250.0, 520.0, 548.0, 718.0, 1296.0 };
        /// <summary>Fundamentals (Hz): a tone of the pad chord sounding at the key shot, in 35-55 Hz: F1 under Bbmaj9 (C1), Eb1 under
        /// Ebmaj7#11 (C3), F1 under Fmaj9 (C4), D1 under Dm11 (C5), G1 under Gmaj9#11 (C8).</summary>
        public static readonly double[] SwellHz = { 43.654, 38.891, 43.654, 36.708, 48.999 };
        /// <summary>Peak RMS of each swell (dBFS): under the 14.3 rows (beat median -30..-31 dBFS), audible as pressure.</summary>
        public static readonly double[] SwellPeakDb = { -38.0, -36.0, -35.0, -37.0, -36.0 };
        public const double SwellRiseSeconds = 8.0, SwellHoldSeconds = 1.0, SwellDecaySeconds = 8.0;
        /// <summary>Highest harmonic frequency of a swell (Hz; DREAD 1.2 "harmonics up to about 160 Hz": the 4th-5th harmonics reach
        /// 147-196 Hz, the band a laptop speaker starts to play, measured by Tools/audio/dread_analysis.py).</summary>
        public const double SwellTopHz = 200.0;
        public const int SwellPartials = 5;
        public static readonly double[] SwellPartialAmp = { 1.0, 0.7, 0.55, 0.45, 0.35 };

        /// <summary>The small-speaker layer of each swell (fix wave C09: laptops and phones play nothing under 150-200 Hz): harmonics
        /// 5-12 of the fundamental that are tones of the pad chord at the key shot, 180-500 Hz, on the swell's envelope. The first
        /// (strongest) sits in the third octave of the pad's 250 Hz octave where the rest is weakest (dread_analysis.py f01 thirds),
        /// so it reaches the rest there while the layer stays under the pad's octave.
        /// KS04 F1 under Bbmaj9: C4 A3 F4; KS09 Eb1 under Ebmaj7#11: G3 Bb3 Eb4; KS10 F1 under Fmaj9: C4 A3 F4;
        /// KS13 D1 under Dm11 (voiced D A F C G): A3 D4 A4; KS17 G1 under Gmaj9#11 (voiced G D F# A C#): D4 G4 A4.</summary>
        public static readonly int[][] SwellUpperH =
        {
            new[] { 6, 5, 8 }, new[] { 5, 6, 8 }, new[] { 6, 5, 8 }, new[] { 6, 8, 12 }, new[] { 6, 8, 9 },
        };
        public static readonly double[][] SwellUpperAmp =
        {
            new[] { 1.0, 0.3, 0.2 }, new[] { 1.0, 0.6, 0.4 }, new[] { 1.0, 0.5, 0.35 }, new[] { 1.0, 0.71, 0.5 }, new[] { 1.0, 0.71, 0.58 },
        };
        public const int SwellUpperMax = 5;
        public const double SwellUpperLoHz = 180.0, SwellUpperHiHz = 500.0;
        /// <summary>Level of the small-speaker layer re the sub partials (dB; the swell's total peak stays SwellPeakDb). Set by
        /// Tools/audio/dread_analysis.py: the best laptop third octave within about 3 dB of the rest, under the pad in its octaves.</summary>
        public static readonly double[] SwellUpperDb = { 0.0, -8.5, -8.5, -6.0, -6.0 };
        /// <summary>The Ark's small-speaker harmonics (ArkDrone.UpHarm on the bow wave and the pods) re their part (dB).</summary>
        public const double HullUpperDb = -3.0;

        /// <summary>Envelope 0..1 of swell k at s: smootherstep rise over 20 m to the key shot, hold 2.5 m, decay over 20 m.</summary>
        public static double SwellEnvelope(int k, double s)
        {
            double s0 = SwellS[k], r = M(SwellRiseSeconds), h = M(SwellHoldSeconds), d = M(SwellDecaySeconds);
            if (s <= s0 - r || s >= s0 + h + d) return 0.0;
            return NocMath.Smootherstep(s0 - r, s0, s) * (1.0 - NocMath.Smootherstep(s0 + h, s0 + h + d, s));
        }

        /// <summary>Amplitude of partial h (1-based) of swell k (0 above SwellTopHz).</summary>
        public static double SwellPartialGain(int k, int h)
        {
            double f = h * SwellHz[k];
            if (f > SwellTopHz + 1e-9) return 0.0;
            return SwellPartialAmp[h - 1];
        }

        // ------------------------------------------------------------------ 1.4 silence drops
        public static readonly double[] DropS = { 520.0, 1296.0 };
        public const double DropDepthDb = -21.0, DropLeadSeconds = 2.0, DropFallSeconds = 0.6, DropReturnSeconds = 1.5;
        /// <summary>Depths of the other layers in the drop (C07): the pad (after its FDN, so its tail goes too), the wind (the breath
        /// of the place stays), the koi tone; R, D, H, T, the X beds and the X passes take DropDepthDb.</summary>
        public const double DropPadDb = -15.0, DropWindDb = -4.0, DropKoiDb = -21.0;

        /// <summary>Shape 0..1 of the drops at s: smootherstep fall over 0.6 s from 2.0 s before the key shot, held to it, back over 1.5 s.</summary>
        public static double DropShape(double s)
        {
            double e = 0.0;
            for (int i = 0; i < DropS.Length; i++)
            {
                double k = DropS[i], a = k - M(DropLeadSeconds), b = a + M(DropFallSeconds), c = k + M(DropReturnSeconds);
                if (s <= a || s >= c) continue;
                e += NocMath.Smootherstep(a, b, s) - NocMath.Smootherstep(k, c, s);
            }
            return e;
        }

        /// <summary>Duck (dB, <= 0) of the rain and the city bed (R, D, H, T, the X beds and passes) at s.</summary>
        public static double DropDb(double s) => DropDepthDb * DropShape(s);

        public static double DropGain(double s) => DropGainAt(s, DropDepthDb);

        /// <summary>Gain of a layer whose full drop depth is depthDb (dB) at s.</summary>
        public static double DropGainAt(double s, double depthDb) => DropGainOf(DropShape(s), depthDb);

        /// <summary>Gain of a layer at drop shape e (0..1; NocMixer takes the larger of DropShape(s) and the Ark's silence).</summary>
        public static double DropGainOf(double e, double depthDb) => e <= 0.0 ? 1.0 : Math.Pow(10.0, depthDb * Math.Min(1.0, e) / 20.0);

        // ------------------------------------------------------------------ 6 the chime's clearing (C53)
        /// <summary>The boulevard traffic wash T and the canyon X bed duck by ChimeDuckDb (C53 asks about 6; 9 brings the chime's best
        /// third octave within about 3 dB of the rest while it stays under the pad) while the door chime sounds: fall over 0.6 s
        /// from 0.3 s before ChimeS, held 3.5 s past it (flight 0.5 s + the two notes), back over 1.5 s.</summary>
        public const double ChimeDuckDb = -9.0, ChimeDuckLeadSeconds = 0.3, ChimeDuckHoldSeconds = 3.5;

        public static double ChimeDuckShape(double s)
        {
            double k = ChimeS, a = k - M(ChimeDuckLeadSeconds), b = a + M(DropFallSeconds), c = k + M(ChimeDuckHoldSeconds), d = c + M(DropReturnSeconds);
            if (s <= a || s >= d) return 0.0;
            return NocMath.Smootherstep(a, b, s) - NocMath.Smootherstep(c, d, s);
        }

        public static double ChimeDuckGain(double s) { double e = ChimeDuckShape(s); return e == 0.0 ? 1.0 : Math.Pow(10.0, ChimeDuckDb * e / 20.0); }

        // ------------------------------------------------------------------ THE ARK (D-043, Logs/ark/ARK.md; replaces the 2.1 hull)
        // A 5 000 m ship coming DOWN the rift from the far north haze (DR's D-043 direction note: every beat-3 / beat-4 gaze looks
        // north), bow (the leading, south end) first, on the rift axis x 515.3, belly y 330 (328 m over the walker), its back in the
        // deck. Every Ark number of the audio comes from ArkC: DR's HullCourse of the abyss copy (20:41) mirrored in ArkCourseMirror
        // below until the merge, when the alias at the top of this file becomes "using ArkC = Megastructure.HullCourse;" and the mirror
        // is deleted (AU's report, shared change request to the lead).
        public const double ArkLaunchS = ArkC.LaunchS, ArkCruise = ArkC.Speed, ArkLength = ArkC.Length, ArkBeam = ArkC.Beam;
        public const double ArkX = ArkC.AxisX, ArkBellyY = ArkC.BellyY0, ArkTopY = ArkC.TopY;
        /// <summary>The motion is toward -z (the bow is the south end; SternZ = BowZ + Length).</summary>
        public const double ArkDir = -1.0;
        /// <summary>tau at which the bow is over the walk line z 195 (86.4 s: s 596 at 2.5 m/s, s 509.6 at 1.5).</summary>
        public const double ArkBowOverTau = ArkC.BowPassTau;
        public const double ArkBowOverS = ArkLaunchS + ArkBowOverTau * RefSpeed;
        /// <summary>tau at which the stern (the north end) is over z 195 (270.3 s: s 1 055.7 at 2.5 m/s, the garden's end).</summary>
        public static readonly double ArkSternOverTau = ArkC.SternPassTau();
        public static readonly double ArkSternOverS = ArkLaunchS + ArkSternOverTau * RefSpeed;
        public static double ArkBowZ(double tau) => ArkC.BowZ(tau);
        public static double ArkSternZ(double tau) => ArkC.SternZ(tau);
        public static double ArkCentreZ(double tau) => ArkC.CentreZ(tau);
        /// <summary>Speed (m/s, magnitude; 72 at the launch 3.4 km out, 28.4 over z 195, 27 cruise) and the signed velocity along +z.</summary>
        public static double ArkVelocity(double tau) => ArkC.Velocity(tau);
        public static double ArkVz(double tau) => ArkDir * ArkC.Velocity(tau);
        /// <summary>The walk axis z the Ark crosses (the tongue tip, the bridge axis).</summary>
        public const double WalkAxisZ = 195.0;
        /// <summary>Seconds until (negative) or since the bow is over the listener's z (at the ship's speed now).</summary>
        public static double ArkBowSeconds(double tau, double lz) => ArkDir * (ArkBowZ(tau) - lz) / Math.Max(1.0, ArkVelocity(tau));

        /// <summary>The silence before the bow passes overhead (keyed to the bow, not to s: at any walking speed it falls before the
        /// bow is over the listener): the rest of the mix falls over 2 s from 9 s before, holds 9 s through the pass, comes back over
        /// 5 s from 2 s after (ARK.md; R D H T, the X beds and passes, the koi -21 dB, the wind -12, the pad -18; the Ark and the
        /// footsteps stay). At 2.5 m/s: s 573.5-578.5 down, held to 601, back by 613.5 (T1's meet at 602 comes up with the return).</summary>
        public const double ArkDropFall0 = -9.0, ArkDropFall1 = -7.0, ArkDropRet0 = 2.0, ArkDropRet1 = 7.0;
        /// <summary>The Ark's silence goes deeper than the key-shot drops for the wind (the bridge's loudest bed: -12 dB, a breath
        /// stays) and the pad (-18 dB after its FDN); R D H T, the X beds and passes, the koi take DropDepthDb as at KS09 / KS17.</summary>
        public const double ArkDropWindDb = -12.0, ArkDropPadDb = -18.0;
        public static double ArkDropShape(double tau, double lz)
        {
            if (tau <= 0.0) return 0.0;
            double t = ArkBowSeconds(tau, lz);
            if (t <= ArkDropFall0 || t >= ArkDropRet1) return 0.0;
            return NocMath.Smootherstep(ArkDropFall0, ArkDropFall1, t) * (1.0 - NocMath.Smootherstep(ArkDropRet0, ArkDropRet1, t));
        }
        /// <summary>s range (2.5 m/s) of the Ark's own stretch: no face strike or face groan from the silence to the stern's pass.</summary>
        public const double ArkQuietS0 = ArkBowOverS + RefSpeed * ArkDropFall0 - CueClearM;
        public static readonly double ArkQuietS1 = ArkSternOverS + 20.0;

        /// <summary>Under the belly (0..1): rises as the bow passes the listener and falls as the stern does (400 m each, about 14 s),
        /// times the lateral cover (the beam's half 220 m, soft over 250 m). The city hum and the traffic wash duck by ArkCoverDb under
        /// it, the X beds and the train passes by ArkCoverBedDb (the hull shuts the city's sky; the crossing under it is hushed, no
        /// pass roars out of the silence); they come back as the stern slides away.</summary>
        public const double ArkCoverIn = -150.0, ArkCoverOut = 250.0, ArkCoverDb = -8.0, ArkCoverBedDb = -6.0;
        public static double ArkCover(double tau, double lx, double lz)
        {
            if (tau <= 0.0) return 0.0;
            double a = NocMath.Smootherstep(ArkCoverIn, ArkCoverOut, ArkDir * (ArkBowZ(tau) - lz)) * NocMath.Smootherstep(ArkCoverIn, ArkCoverOut, ArkDir * (lz - ArkSternZ(tau)));
            if (a <= 0.0) return 0.0;
            return a * (1.0 - NocMath.Smootherstep(0.5 * ArkBeam, 0.5 * ArkBeam + 250.0, Math.Abs(lx - ArkX)));
        }

        /// <summary>Peak RMS of the Ark's drone (dBFS: the body's pressure under the middle of the ship; the bow wave and the engine
        /// pods add as they pass).</summary>
        public const double ArkPeakDb = -38.0;
        /// <summary>The body's small-speaker layer (C09 for the Ark: laptops play nothing of the 13.75 Hz stack): 8, 12 and 16 x 27.5 Hz
        /// = A3, E4, A4 (A is a tone of every chord under the hull, Fmaj9 / Dm11 / Dmaj9; E of the first and the last, the 9th of Dm11),
        /// a centre and a 0.35 %-detuned side voice (the pressure breathes), on the body's gain, ArkBodyUpperDb re the body.</summary>
        public const double ArkBodyUpperDb = -2.0;
        /// <summary>Under a key-shot swell the Ark's small-speaker layers give way (x (1 - 0.8 envelope)): the swell's own chord tones
        /// carry the laptop band there and E stays under the pad in its octaves (D-038 (10); a03 at 1.5 m/s: KS10 +0.28 dB without it).</summary>
        public const double ArkUpperSwellDuck = 0.8;
        /// <summary>A call from farther than this (m) is not sounded (the ship is gone into the haze: at 1.5 m/s the stern is 5 km off
        /// by s 1 090).</summary>
        public const double ArkHornMaxDist = 3500.0;
        /// <summary>In the garden (beat 5, the warm one: the koi pond under the crown) the drone sits 2.5 dB lower (the 14.3 beat-5 row
        /// keeps its margin: +1.21 dB over Sum without it, a03 gates).</summary>
        public const double ArkGardenTrimDb = -2.5;
        /// <summary>The drone fades out once the stern is far in the haze (tau after the stern's pass).</summary>
        public static readonly double ArkEndTau0 = ArkSternOverTau + 110.0, ArkEndTau1 = ArkSternOverTau + 170.0;
        /// <summary>The engine pods along the belly (m from the bow; the last is the stern's main cluster) and their gains.</summary>
        public static readonly double[] ArkPodFromBow = { 1150.0, 2350.0, 3550.0, 4820.0 };
        public static readonly double[] ArkPodGain = { 0.75, 0.8, 0.85, 1.25 };
        /// <summary>Distance law of the bow wave and the pods: 1 / (1 + (r / r0)^2).</summary>
        public const double ArkBowR0 = 420.0, ArkPodR0 = 330.0;
        /// <summary>The bow wave's gain re ArkPeakDb (at r 0; 0.62 of it overhead): +3 dB over the first cut (a04: 0.9), so the pass
        /// itself, the long glide down in the silence, is the event (E re the rest in the silence +4.0 -> about +6 dB).</summary>
        public const double ArkBowGain = 1.27;
        /// <summary>z of a point fromBow m behind the bow (toward the stern).</summary>
        public static double ArkZBehindBow(double tau, double fromBow) => ArkBowZ(tau) - ArkDir * fromBow;

        /// <summary>Doppler ratio c / (c + v_r) of a source at (x, y, z) moving along z at the signed speed u, for a listener at (lx, ly, lz).</summary>
        public static double PointDoppler(double x, double y, double z, double u, double lx, double ly, double lz)
        {
            double dx = x - lx, dy = y - ly, dz = z - lz, r = Math.Sqrt(dx * dx + dy * dy + dz * dz);
            double vr = r > 1e-6 ? u * dz / r : 0.0;   // positive when receding
            return SoundSpeed / (SoundSpeed + vr);
        }

        /// <summary>The Ark's horn calls come from its bow (1) or stern (2): position at fire time, heard after the flight time and in
        /// tune (the voice plays the heard pitch, e.Hz: the emitted one carries the Doppler out); level e.Db at 1 700 m, 10 log10 of
        /// the distance ratio (clamped -8..+3 dB).</summary>
        public static void ArkHornSource(int part, double tau, double lx, double lz, out double x, out double y, out double z, out double dist, out double doppler)
        {
            x = ArkX; y = part == 2 ? ArkBellyY + 120.0 : ArkBellyY + 80.0;
            z = part == 2 ? ArkSternZ(tau) + ArkDir * 80.0 : ArkZBehindBow(tau, 60.0);
            double dx = x - lx, dy = y - NocturneConstants.EyeHeight, dz = z - lz;
            dist = Math.Sqrt(dx * dx + dy * dy + dz * dz);
            doppler = PointDoppler(x, y, z, ArkVz(tau), lx, NocturneConstants.EyeHeight, lz);
        }

        public static double ArkHornDistanceDb(double dist) => NocMath.Clamp(-10.0 * Math.Log10(Math.Max(1.0, dist) / 1700.0), -8.0, 3.0);

        /// <summary>The calls fire on the Ark's own clock (the ship calls at the same points of its course at any walking speed): call i
        /// sounds when tau first crosses (HornS[i] - ArkLaunchS) / 2.5, i.e. at s HornS[i] on the 2.5 m/s walk (tau 24 / 144 / 284;
        /// at 1.5 m/s s 416 / 596 / 806, each heard on a tone of the chord sounding then).</summary>
        public static double ArkCallTau(double s) => (s - ArkLaunchS) / RefSpeed;

        /// <summary>Hz and length of Ark groan k (22-30 Hz combs, 7-9.5 s).</summary>
        public static double ArkGroanHz(int k) => 22.0 + 8.0 * Rnd(32, k);
        public static double ArkGroanSeconds(int k) => 7.0 + 2.5 * Rnd(33, k);

        /// <summary>Source of Ark groan k for a listener at (lx, lz): the belly point over the listener (clamped into the hull), moved by
        /// the hash up to 70 m across and 160 m along. Returns the distance from the ear.</summary>
        public static double ArkGroanSource(int k, double tau, double lx, double lz, out double x, out double y, out double z)
        {
            double h1 = Rnd(34, k), h2 = Rnd(35, k), b = ArkBowZ(tau), st = ArkSternZ(tau);
            x = NocMath.Clamp(lx, ArkX - 0.4 * ArkBeam, ArkX + 0.4 * ArkBeam) + 70.0 * (2.0 * h1 - 1.0);
            z = NocMath.Clamp(lz + 160.0 * (2.0 * h2 - 1.0), Math.Min(b, st) + 60.0, Math.Max(b, st) - 60.0);
            y = ArkBellyY + 12.0;
            double dx = x - lx, dy = y - NocturneConstants.EyeHeight, dz = z - lz;
            return Math.Sqrt(dx * dx + dy * dy + dz * dz);
        }

        /// <summary>Level (dBFS) of an Ark groan heard from d m: ArkGroanDb overhead (328 m), less 8 log10 of the (1 + d / 100) ratio.</summary>
        public static double ArkGroanLevelDb(double d) => ArkGroanDb - 8.0 * Math.Log10((1.0 + d / 100.0) / (1.0 + (ArkBellyY - NocturneConstants.EyeHeight) / 100.0));

        // ------------------------------------------------------------------ 3.2 the falling panel, 5.4 lightning, 6 the tram chime
        /// <summary>The panel (DR's PanelCourse): tears off face B at s 470; the creak sounds from its centre at the tear,
        /// PanelCourse.Centre(0) = (814.85, 150, 800) (integration round, DR's request; was the face point x 815.3).</summary>
        public const double PanelS = PanelCourse.LaunchS;
        public static readonly double PanelX = PanelCourse.Centre(0.0).x, PanelY = PanelCourse.Centre(0.0).y, PanelZ = PanelCourse.Centre(0.0).z;
        /// <summary>Sheet-lightning glows (s; DREAD 5.4: at most one per 25 s = 62.5 m, none in beats 1 and 5; s 553 outlines the hull).</summary>
        public static readonly double[] LightningS = { 205.0, 345.0, 553.0, 790.0, 1140.0, 1420.0 };
        /// <summary>The far rumble follows its glow by this lag (s): a cloud flash some 1 km away.</summary>
        public const double RumbleLagSeconds = 3.0;
        /// <summary>The door chime sounds 1.5 s before the tram leaves the terminus (TramSchedule.LaunchS 230; DREAD 6).</summary>
        public const double ChimeLeadSeconds = 1.5, ChimeX = 210.0, ChimeY = 2.5, ChimeZ = 227.0;
        public static double ChimeS => NocturneScore.Vehicles[0].SLaunch - M(ChimeLeadSeconds);

        // ------------------------------------------------------------------ 1.1 / 1.3 schedules
        /// <summary>Horns (the Ark's calls, D-043): s 440 from its bow far in the southern haze (Eb2 + Bb2 over C3), s 740 from the bow far
        /// north (F2 + C3 over C5 Dm11; C08: clear of T4's and T2's meets), s 1 090 from the stern sliding away north (D2 + A2 over the
        /// C6 -> C7 change: tones of both chords), heard in tune (the emitted pitch takes the Doppler out).</summary>
        public static readonly double[] HornS = { 440.0, 740.0, 1090.0 };
        public static readonly double[] HornHz = { 77.782, 87.307, 73.416 };
        public static readonly double[] HornDbs = { HornDb, HornDb, HornDb - 2.5 };
        public static readonly int[] HornPart = { 1, 1, 2 };
        /// <summary>Strikes: inside the Gulf (s 405-990), intervals 25-40 s at 2.5 m/s (62.5-100 m).</summary>
        public const double StrikeS0 = 405.0, StrikeS1 = 990.0, StrikeGapMin = 25.0, StrikeGapMax = 40.0;
        /// <summary>Groans: from s 120 on, intervals 20-60 s at 2.5 m/s (50-150 m); none after the goal trigger.</summary>
        public const double GroanS0 = 120.0, GroanS1 = 1440.0, GroanGapMin = 20.0, GroanGapMax = 60.0;
        /// <summary>The koi's S7 (s 1 340-1 380) stays free of groans; the last groan is before s 1 440 (the warm ending).</summary>
        public const double GroanQuietS0 = 1336.0, GroanQuietS1 = 1384.0;
        /// <summary>No groan or strike within this many metres of a horn, the creak, a swell peak or a drop (each cue alone).</summary>
        public const double CueClearM = 8.0;
        /// <summary>No horn, strike or groan within this many metres of a vehicle meet at 2.5 m/s (C08 asks 15; 18 keeps the strike after
        /// T2 out of its pass tail: NocturneScore.MeetS: the tram at
        /// s 272, T1-T7 on the bridge and the face tubes): the passes are the loudest seconds of the walk (C08).</summary>
        public const double MeetClearM = 18.0;

        /// <summary>True when s is within MeetClearM of a vehicle meet at the reference speed.</summary>
        public static bool NearMeet(double s)
        {
            for (int i = 0; i < NocturneScore.Vehicles.Length; i++) if (Math.Abs(s - NocturneScore.MeetS(i, RefSpeed)) < MeetClearM) return true;
            return false;
        }

        /// <summary>Levels (dBFS, peak 1 s RMS at the listener): strikes at 0 m (less 10 log10(1 + d / 150) with distance), groans
        /// on a near mass (less 8 log10(1 + d / 100)), the far rumbles, the chime from the terminus 160 m away, the horn and the creak.
        /// Set by Tools/audio/dread_analysis.py: each cue within reach of the rest in its best laptop band, the horn and the creak
        /// under the pad in the pad's own octaves (DREAD: "never mask the pad"), AAC / Opus 128 kbit/s keep 96-100 % of E.</summary>
        public const double StrikeDb = -33.0, GroanDb = -36.0, RumbleDb = -37.0, ChimeDb = -40.0, HornDb = -41.5, CreakDb = -43.0;

        public static readonly DreadEvent[] Events = BuildEvents();

        static double Rnd(ulong stream, int i) => NocAudio.Hash01(0xD8EAD0000UL + stream, (ulong)i);

        static bool Clear(double s)
        {
            for (int i = 0; i < HornS.Length; i++) if (Math.Abs(s - HornS[i]) < CueClearM + 10.0) return false;
            if (Math.Abs(s - PanelS) < CueClearM + 10.0) return false;
            if (Math.Abs(s - ChimeS) < CueClearM + 4.0) return false;
            for (int i = 0; i < SwellS.Length; i++) if (s > SwellS[i] - CueClearM && s < SwellS[i] + CueClearM) return false;
            for (int i = 0; i < DropS.Length; i++) if (s > DropS[i] - M(DropLeadSeconds) - CueClearM && s < DropS[i] + CueClearM) return false;
            for (int i = 0; i < LightningS.Length; i++) if (Math.Abs(s - LightningS[i] - M(RumbleLagSeconds)) < CueClearM + 4.0) return false;
            if (NearMeet(s)) return false;
            if (s > ArkQuietS0 && s < ArkQuietS1) return false;   // the Ark's stretch: its own groans, no face strikes or groans
            return true;
        }

        /// <summary>The first of s, s + 8, ..., s + 56 that is clear of the other cues (NaN when none is): a slot that falls on a
        /// horn, the creak, a swell peak or a drop moves on instead of vanishing. Groans also keep 15 m from strikes and out of S7.</summary>
        static double Shifted(double s, System.Collections.Generic.List<DreadEvent> list, bool groan)
        {
            for (int t = 0; t < 8; t++)
            {
                double c = s + 8.0 * t;
                bool free = Clear(c);
                if (groan)
                {
                    if (c > GroanQuietS0 && c < GroanQuietS1) free = false;
                    for (int j = 0; j < list.Count && free; j++) if (list[j].Kind == DreadKind.Strike && Math.Abs(list[j].S - c) < 15.0) free = false;
                }
                if (free) return c;
            }
            return double.NaN;
        }

        static DreadEvent[] BuildEvents()
        {
            var list = new System.Collections.Generic.List<DreadEvent>();
            // the Ark's horn calls (D-043): a fifth on chord tones, from the bow or the stern (position, flight and Doppler at fire time;
            // X / Y / Z here are the reference walk's, 2.5 m/s, for the table and the tests)
            for (int i = 0; i < HornS.Length; i++)
            {
                D3 p = NocMath.PathPoint(HornS[i]);
                ArkHornSource(HornPart[i], (HornS[i] - ArkLaunchS) / RefSpeed, p.x, p.z, out double hx, out double hy, out double hz, out double hd, out _);
                list.Add(new DreadEvent { S = HornS[i], Kind = DreadKind.Horn, X = hx, Y = hy, Z = hz, Hz = HornHz[i], Db = HornDbs[i], Delay = hd / SoundSpeed, Seconds = 9.0, Ark = HornPart[i] });
            }
            // the panel tearing off face B (heard after its flight time)
            {
                D3 p = NocMath.PathPoint(PanelS);
                double dx = PanelX - p.x, dy = PanelY - NocturneConstants.EyeHeight, dz = PanelZ - p.z, d = Math.Sqrt(dx * dx + dy * dy + dz * dz);
                list.Add(new DreadEvent { S = PanelS, Kind = DreadKind.Creak, X = PanelX, Y = PanelY, Z = PanelZ, Hz = 310.0, Db = CreakDb, Delay = d / SoundSpeed, Seconds = 4.0 });
            }
            // the tram's door chime from the terminus (D5 then Bb4: chord tones of C1 Bbmaj9)
            {
                double cs = ChimeS; D3 p = NocMath.PathPoint(cs);
                double dx = ChimeX - p.x, dz = ChimeZ - p.z, d = Math.Sqrt(dx * dx + dz * dz);
                list.Add(new DreadEvent { S = cs, Kind = DreadKind.Chime, X = ChimeX, Y = ChimeY, Z = ChimeZ, Hz = 587.33, Db = ChimeDb, Delay = d / SoundSpeed, Seconds = 3.5 });
            }
            // far rumbles under the lightning glows
            for (int i = 0; i < LightningS.Length; i++)
                list.Add(new DreadEvent { S = LightningS[i], Kind = DreadKind.Rumble, X = 515.3, Y = 460.0, Z = 195.0 + 900.0 * (Rnd(7, i) - 0.5), Hz = 0.0, Db = RumbleDb, Delay = RumbleLagSeconds, Seconds = 7.0 });
            // metal strikes inside the faces
            {
                double s = StrikeS0 + M(StrikeGapMin) * Rnd(1, 0);
                int k = 0;
                while (s < StrikeS1)
                {
                    double s0 = s;
                    s = Shifted(s, list, false);
                    if (!double.IsNaN(s))
                    {
                        D3 p = NocMath.PathPoint(s);
                        bool faceB = Rnd(2, k) < (p.x < 515.3 ? 0.35 : 0.65);
                        double fx = faceB ? FaceBX : FaceAX, fz = p.z + 260.0 * (2.0 * Rnd(3, k) - 1.0), fy = -90.0 + 240.0 * Rnd(4, k);
                        double dx = fx - p.x, dy = fy - NocturneConstants.EyeHeight, dz = fz - p.z, d = Math.Sqrt(dx * dx + dy * dy + dz * dz);
                        list.Add(new DreadEvent { S = s, Kind = DreadKind.Strike, X = fx, Y = fy, Z = fz, Hz = 170.0 + 130.0 * Rnd(5, k), Db = StrikeDb - 10.0 * Math.Log10(1.0 + d / 150.0), Seconds = 4.5 });
                    }
                    s = (double.IsNaN(s) ? s0 : s) + M(StrikeGapMin + (StrikeGapMax - StrikeGapMin) * Rnd(6, k));
                    k++;
                }
            }
            // structural groans (the mass is placed at fire time: NearestMass)
            {
                double s = GroanS0 + M(GroanGapMin) * Rnd(11, 0);
                int k = 0;
                while (s < GroanS1)
                {
                    double sg = Shifted(s, list, true);
                    if (!double.IsNaN(sg) && sg < GroanS1)
                        list.Add(new DreadEvent { S = sg, Kind = DreadKind.Groan, Hz = 34.0 + 30.0 * Rnd(12, k), Db = GroanDb, Seconds = 4.0 + 2.5 * Rnd(13, k) });
                    s = (double.IsNaN(sg) ? s : sg) + M(GroanGapMin + (GroanGapMax - GroanGapMin) * Rnd(14, k));
                    k++;
                }
            }
            list.Sort((a, b) => a.S.CompareTo(b.S));
            var arr = list.ToArray();
            for (int i = 0; i < arr.Length; i++) arr[i].Seq = i;
            return arr;
        }

        /// <summary>The nearest large mass to the listener at s (groans, DREAD 1.3): the canyon walls of beats 2, 5 and 6, the Gulf
        /// faces and the bridge structure of beats 3-4, the spire of beat 7; the hash picks a side or the far face now and then.
        /// Returns the source position and its distance.</summary>
        public static double NearestMass(double s, int seq, out double x, out double y, out double z)
        {
            D3 p = NocMath.PathPoint(NocMath.Clamp(s, 0.0, NocturneConstants.GoalS));
            double h1 = Rnd(21, seq), h2 = Rnd(22, seq), h3 = Rnd(23, seq);
            int beat = NocMath.BeatIndex(s);
            if (beat == 2 || beat == 3)
            {
                if (beat == 3 && h1 < 0.3) { x = p.x + 40.0 * (2.0 * h2 - 1.0); y = -30.0; z = p.z + 8.0 * (2.0 * h3 - 1.0); }   // the bridge under the deck
                else
                {
                    bool near = h1 < 0.8;
                    bool faceA = (p.x < 0.5 * (FaceAX + FaceBX)) == near;
                    x = faceA ? FaceAX : FaceBX; y = -60.0 + 160.0 * h2; z = p.z + 120.0 * (2.0 * h3 - 1.0);
                }
            }
            else if (beat == 6) { x = NocturneConstants.SpireX; y = 120.0 + 200.0 * h2; z = NocturneConstants.SpireZ; }
            else
            {
                // canyon walls: the side by the hash, 24-40 m off the walk, high in the tower
                NocLanes.Path.TangentAt(NocMath.Clamp(s, 0.0, NocturneConstants.GoalS), out double tx, out double tz);
                double side = h1 < 0.5 ? -1.0 : 1.0, off = 24.0 + 16.0 * h3;
                x = p.x - side * tz * off + 20.0 * (2.0 * h2 - 1.0); z = p.z + side * tx * off; y = 25.0 + 70.0 * h2;
            }
            double dx = x - p.x, dy = y - NocturneConstants.EyeHeight, dz = z - p.z;
            return Math.Sqrt(dx * dx + dy * dy + dz * dz);
        }

        // ------------------------------------------------------------------ 2.2 machine hums (DR's cabins and gantries)
        /// <summary>The hums sit on DR's objects (integration round: AU-28 and DR's request; DreadCabinCourse is the one source of
        /// truth, D-038 / D-040): hum i &lt; 8 is shaft i's (on the loudest of its 4 cabins, at the cabin light's ramp, so a cabin's
        /// hidden wrap never jumps; the traffic clock), hums 8 and 9 the two gantries (DreadBuilder.GantryPos, the scene clock).
        /// Fundamentals (Hz) and peaks (dBFS within 20 m, falling as (20 / d)^1.2 beyond) are the audio's. The audio's former shafts
        /// (two boulevard towers, face A at z 238, gantries at y 30 / -20) are gone: no cabin rides there in the picture.</summary>
        public static readonly double[] HumHz = { 111.0, 117.0, 123.0, 114.0, 120.0, 126.0, 108.0, 129.0, 50.0, 50.0 };
        public const double CabinHumDb = -48.0, GantryHumDb = -46.0, HumNearM = 20.0, HumFall = 1.2;
        public static int HumCount => DreadCabinCourse.ShaftCount + DreadCabinCourse.GantryCount;
        public static bool HumIsGantry(int i) => i >= DreadCabinCourse.ShaftCount;
        public static double HumDb(int i) => HumIsGantry(i) ? GantryHumDb : CabinHumDb;
        static readonly DreadCabinCourse.Cabin[] s_cabins = DreadCabinCourse.Cabins();

        /// <summary>The distance law of the hums: 1 within 20 m, (20 / d)^1.2 beyond.</summary>
        public static double HumDistanceGain(double d) => d < HumNearM ? 1.0 : Math.Pow(HumNearM / d, HumFall);

        /// <summary>Source of hum i at scene time t (the traffic clock is its wrap mod 65 536 s, which the cabins' quantised speeds
        /// make exact) for a listener at (lx, ly, lz): the position and its gain (distance law x the cabin light's ramp).</summary>
        public static double HumSource(int i, double t, double lx, double ly, double lz, out double x, out double y, out double z)
        {
            if (HumIsGantry(i))
            {
                int g = i - DreadCabinCourse.ShaftCount;
                x = DreadCabinCourse.GantryX; y = DreadCabinCourse.GantryY(g); z = DreadCabinCourse.GantryZ(g, t);
                return HumDistanceGain(Dist(x - lx, y - ly, z - lz));
            }
            double best = 0.0;
            x = DreadCabinCourse.Shafts[i, 0]; z = DreadCabinCourse.Shafts[i, 1]; y = DreadCabinCourse.Y0;
            for (int c = 0; c < DreadCabinCourse.CabinsPerShaft; c++)
            {
                double yc = DreadCabinCourse.CabinY(in s_cabins[i * DreadCabinCourse.CabinsPerShaft + c], t, out double ramp);
                double g = ramp * HumDistanceGain(Dist(x - lx, yc - ly, z - lz));
                if (g > best) { best = g; y = yc; }
            }
            return best;
        }

        static double Dist(double dx, double dy, double dz) => Math.Sqrt(dx * dx + dy * dy + dz * dz);

        // ------------------------------------------------------------------ 4.3 the bridge sway (beat 4) and its gusts
        public const double SwayHz = 0.12;
        /// <summary>The sway of TR's BridgeSway (Beats/Beat04_Skybridge, the camera's lateral offset), mirrored so the DSP needs no
        /// picture code: span shape sin(pi u) between the truss bearings x 375.3 and 675.3, amplitude 2 cm + 1 cm x the gust
        /// envelope 0.5 - 0.5 cos(2 pi t / 17); 1 at the 3 cm peak. The creaks sit at the extremes of sin(2 pi 0.12 t).</summary>
        public const double SwaySpanX0 = 375.3, SwaySpanX1 = 675.3, SwayBase = 0.020, SwayGustAdd = 0.010;

        public static double SwayAmplitude(double s, double sceneT)
        {
            double u = (s - NocturneConstants.EastLegOffset - SwaySpanX0) / (SwaySpanX1 - SwaySpanX0);
            if (u <= 0.0 || u >= 1.0) return 0.0;
            double g = 0.5 - 0.5 * Math.Cos(2.0 * Math.PI * sceneT / NocturneConstants.RainGustPeriod);
            return Math.Sin(Math.PI * u) * (SwayBase + SwayGustAdd * g) / (SwayBase + SwayGustAdd);
        }
        /// <summary>Peak RMS of the bridge's gust sound (cable whistle, deck creak) at full gust (dBFS).</summary>
        public const double GustPeakDb = -49.0;

        // ------------------------------------------------------------------ the Ark's groans (declared last: they read the cue tables)
        /// <summary>The Ark's groans (its hull working overhead): keyed to the Ark's own clock tau, the first 0.8 s after the bow is over
        /// z 195 (inside the silence), then every 22-32 s until the stern has passed; each later slot 10 m clear (at 2.5 m/s) of the
        /// train meets, 18 m of the horns, off the swell peaks and the rumbles (moves on in 2 s steps).</summary>
        public const double ArkGroanDb = -35.0, ArkGroanMeetClearM = 10.0, ArkGroanGapMin = 22.0, ArkGroanGapMax = 32.0;
        public static readonly double[] ArkGroanTau = BuildArkGroans();

        static bool ArkGroanClear(double tau)
        {
            double s = ArkLaunchS + tau * RefSpeed;
            for (int i = 0; i < NocturneScore.Vehicles.Length; i++) if (Math.Abs(s - NocturneScore.MeetS(i, RefSpeed)) < ArkGroanMeetClearM) return false;
            for (int i = 0; i < HornS.Length; i++) if (Math.Abs(s - HornS[i]) < CueClearM + 10.0) return false;
            for (int i = 0; i < SwellS.Length; i++) if (Math.Abs(s - SwellS[i]) < CueClearM) return false;
            for (int i = 0; i < LightningS.Length; i++) if (Math.Abs(s - LightningS[i] - M(RumbleLagSeconds)) < CueClearM + 4.0) return false;
            return true;
        }

        static double[] BuildArkGroans()
        {
            var list = new System.Collections.Generic.List<double>();
            double tau = ArkBowOverTau + 0.8;
            for (int k = 0; k < 16 && tau < ArkSternOverTau + 6.0; k++)
            {
                double t = tau; int tries = 0;
                if (k > 0) while (!ArkGroanClear(t) && tries < 14) { t += 1.5; tries++; }
                if (tries < 14) list.Add(t);
                tau = t + ArkGroanGapMin + (ArkGroanGapMax - ArkGroanGapMin) * Rnd(31, k);
            }
            return list.ToArray();
        }
    }

    /// <summary>MIRROR of DR's D-043 HullCourse (the Ark) as it stands in the abyss copy at 20:41 (Beats/Dread/DreadCourse.cs; owner DR,
    /// the one source of truth): the same constants and formulas, so the audio follows the picture's course before the merge. At the
    /// merge the alias at the top of this file becomes "using ArkC = Megastructure.HullCourse;" and this class is deleted
    /// (AudioDreadTests.Ark_MirrorIsDrsCourse_D043 then compares nothing but the numbers).</summary>
    public static class ArkCourseMirror
    {
        public const double LaunchS = 380.0;
        public const double Speed = 27.0, ApproachDv = 45.0, ApproachK = 25.0;
        public const double Length = 5000.0, Beam = 440.0, HalfBeam = 220.0;
        public const double AxisX = 515.3;
        public const double WalkZ = 195.0;
        public const double BowPassTau = 86.4, AbeamTau = BowPassTau;
        public const double BellyY0 = 330.0, KeelY = 322.0, TopY = 590.0, CentreY = 460.0;
        static readonly double s_d0 = Travel(BowPassTau);
        public static double Travel(double tau)
        {
            if (tau <= 0.0) return 0.0;
            return Speed * tau + ApproachDv * ApproachK * (1.0 - Math.Exp(-tau / ApproachK));
        }
        public static double Velocity(double tau) => tau <= 0.0 ? 0.0 : Speed + ApproachDv * Math.Exp(-tau / ApproachK);
        public static double BowDistance(double tau) => s_d0 - Travel(tau);
        public static double BowZ(double tau) => WalkZ + BowDistance(tau);
        public static double CentreZ(double tau) => BowZ(tau) + 0.5 * Length;
        public static double SternZ(double tau) => BowZ(tau) + Length;
        public static double SternPassTau() => SolveTravel(s_d0 + Length);
        public static double SolveTravel(double x)
        {
            double t = Math.Max(0.0, (x - ApproachDv * ApproachK) / Speed);
            for (int i = 0; i < 30; i++) { double v = Velocity(Math.Max(t, 1e-9)); t -= (Travel(t) - x) / v; if (t < 0.0) t = 0.0; }
            return t;
        }
    }
}
