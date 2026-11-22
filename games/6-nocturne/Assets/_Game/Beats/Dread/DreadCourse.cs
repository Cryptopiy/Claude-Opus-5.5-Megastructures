// DreadCourse: the courses of the D-034 objects of package DR (docs/DREAD.md 2.1 the hull over the rift, 3.1 the shadow below;
// owner DR, wave B). Pure functions of (s, tau): DreadTests and DreadDriver share them.
//
// Timing (the 14.0 / H7 rule for vehicles: launched by s, then real time): each object waits parked until s first crosses its
// launch s_L, then moves on real seconds tau since the launch (NocState.Dt, so a pause stops it), with a smooth start (constant
// acceleration over Ramp seconds, then the cruise speed: no velocity jump, nothing pops). Capture mode (stills and sequences) and a
// run that starts past s_L use tau = max(0, (s - s_L) / 2.5), a pure function of s (the cinematic pace). Never a reset: once
// launched an object keeps moving away (a walker who turns back never sees it jump).
//
// The hull (2.1, fix wave D-040 C01: it flies OVER the walker): a 1 800 x 260 m ribbed body (keel y 418, belly 432, back 478 inside
// the deck at 460) on the rift's axis x 515.3 (faces 215.3 / 815.3), moving north (+z) at 18 m/s from its parked centre z -660
// (stern -1 560, bow 240), launched at s 430 with a 10 s smooth start: for tau >= 10 its centre is z = 18 tau - 750, exactly AU's
// DreadScore course (195 + 18 (tau - 52.5)), abeam of the walk line z 195 at tau 52.5 = s 561.25 at 2.5 m/s (s 508.75 at 1.5), where
// the s 553 sheet lightning (AT) outlines it overhead. Before the launch it waits INSIDE the cloud deck, 170 m up (lowest point
// y 575: the cloud mix of NocHull (455..530, billows +-30) and of its lights (380..560) is 1 there, so the parked hull is the sky; the
// boulevard (KS04 / KS05, beat 2) never sees it), and sinks out of the deck over the first 24 s after the launch (smootherstep: no
// velocity step), so it condenses out of the cloud base over the Edge and crosses overhead; AT's deck occluder fades in with it.
// At s 496 (FX's KS09 long look-up) its belly fills the sky from overhead to the bow 430 m north; at s 720 (beat 4) it recedes north
// along the rift (bow 1 100 m north at 21 deg). It recedes into the haze (T < 3e-5 past stern z 9 500, renderers off).
//
// The shadow (3.1, fix wave C42 / DR-05 ruling: seen through the KS13 strip): a 900 m body (half-width 81 m plus two fin pairs,
// half-height 60 m plus a dorsal crest) at y -700 (between the y -520 subject layer above and the y -880 river below) on x 470,
// moving south (-z) at 5 m/s from its parked centre z 384, launched at s 600 with a 20 s smooth start: its centre is under the strip
// (z 195) at s 719.4 at 2.5 m/s (KS13 s 718: z 199; at 1.5 m/s z 41, still inside the strip's z band of +-440 m at that depth), its
// back lights a row across the strip. It fades in over s 590-622 and out over s 860-900. It darkens AT's haze plane (_NocHazeOcc)
// and dims AB's lower rivers inside its outline (NocStreak _NocAbyssOccluder, half-length 350: half-width 63 m about x 470), so
// the river x 470 / y -880 goes dark under it while the x 560 / y -520 subject layer stays lit (r_x 1.43).
using System;
using UnityEngine;

namespace Megastructure
{
    /// <summary>The play-mode launch state of one DR object (one per driver; tests step it directly).</summary>
    public struct DreadLaunchState
    {
        public bool Init;
        public bool Launched;
        public double Tau;      // real seconds since the launch
        public double PrevS;
    }

    public static class DreadClock
    {
        /// <summary>Capture-mode tau (stills, sequences, a start past the launch): max(0, (s - s_L) / 2.5).</summary>
        public static double CaptureTau(double s, double launchS) => Math.Max(0.0, (s - launchS) / NocturneConstants.CinematicSpeed);

        /// <summary>One play frame: launch on the first crossing of s_L (or at once, at the cinematic pace, when the first frame is
        /// already past it), then tau += dt. Returns tau (0 while parked).</summary>
        public static double Step(ref DreadLaunchState st, double s, double dt, double launchS)
        {
            if (!st.Init)
            {
                st.Init = true;
                st.PrevS = s;
                if (s >= launchS) { st.Launched = true; st.Tau = CaptureTau(s, launchS); }
                return st.Launched ? st.Tau : 0.0;
            }
            if (st.Launched) st.Tau += Math.Max(0.0, dt);
            else if (s >= launchS) { st.Launched = true; st.Tau = 0.0; }
            st.PrevS = s;
            return st.Launched ? st.Tau : 0.0;
        }

        /// <summary>Distance travelled after tau seconds with a smooth start: v tau^2 / (2 ramp) below the ramp, then v (tau - ramp / 2)
        /// (position C1: no velocity jump).</summary>
        public static double Travel(double tau, double speed, double ramp)
        {
            if (tau <= 0.0) return 0.0;
            if (ramp <= 0.0 || tau >= ramp) return speed * (tau - 0.5 * Math.Max(ramp, 0.0));
            return speed * tau * tau / (2.0 * ramp);
        }

        public static double Smooth01(double a, double b, double x)
        {
            double t = Math.Max(0.0, Math.Min(1.0, (x - a) / (b - a)));
            return t * t * (3.0 - 2.0 * t);
        }
    }

    /// <summary>D-043 THE ARK (replaces DREAD 2.1's 1 800 m hull; owner DR; the one source of truth, AU and AT read it): a 5 000 m
    /// ship, beam 440 m (the rift's neck is 600 m: 80 m clear a side), belly (keel plate) y 330, its back at y 590 in the cloud deck,
    /// on the rift axis x 515.3, coming DOWN the rift from the far north haze toward the walker and passing over the walk line z 195
    /// bow first. Direction (DR, D-043 note): the walker's beat-3 and beat-4 gazes all look north (+z: KS08 world yaw 28 / pitch +24,
    /// KS09 yaw 20, KS10 yaw 355, the s 624-660 bridge look yaw 0, KS12 yaw 0), so the Ark comes from the north (+z) and moves south
    /// (-z): in beat 3 its lights and then its dark mass come out of the haze far up the rift inside those frames, and once its bow
    /// has passed overhead the walker crosses the bridge under its belly looking north along 5 km of ship whose stern stays lost in
    /// the haze (ARK.md's "from the south" would put it behind every beat-3 gaze and its bow 150-700 m ahead of the s 624-660 look).
    /// Here the bow is the LEADING (south) end: BowZ = CentreZ - L/2, SternZ = CentreZ + L/2.
    /// Course: hidden (renderers off) until s first crosses LaunchS 380 (the walker inside the boulevard canyon, whose walls hide the
    /// rift's north), then real seconds tau: travel X(tau) = Vc tau + Dv K (1 - e^(-tau/K)) (speed Vc + Dv e^(-tau/K): 72 m/s at the
    /// launch 3.4 km out, where only its growth reads, decelerating to 28.4 m/s as it passes overhead and to the 27 m/s cruise):
    ///   s 400 (the Edge) bow 2.9 km out (fog T 0.3: a constellation of lights), KS08 (s 428) 2.3 km, s 470 1.6 km, KS09 (s 520) 0.9 km,
    ///   the bow over z 195 at tau 86.4 = s 596 (2.5 m/s; s 509.6 at 1.5 m/s: over the pier), the stern over z 195 at tau 270 = s 1056
    ///   (the garden's end; s 785 at 1.5 m/s). Capture mode: tau = (s - 380) / 2.5.</summary>
    public static class HullCourse
    {
        public const double LaunchS = 380.0;
        /// <summary>Cruise speed (m/s, reached asymptotically; 28.4 at the pass) and the approach excess Dv decaying over K s.</summary>
        public const double Speed = 27.0, ApproachDv = 45.0, ApproachK = 25.0;
        /// <summary>No smooth-start ramp: the launch happens hidden (renderers off until the launch, the canyon hides the rift's north).</summary>
        public const double Ramp = 0.0;
        public const double Length = 5000.0, Beam = 440.0, HalfBeam = 220.0;
        public const double AxisX = 515.3;
        public const double WalkZ = 195.0;
        /// <summary>tau at which the BOW is over the walk line z 195 (s 596 at 2.5 m/s). AbeamTau keeps AU's name: the bow pass.</summary>
        public const double BowPassTau = 86.4, AbeamTau = BowPassTau;
        /// <summary>Section: belly (keel plate) y 330, the widest tread y 470, the back (spine) y 590 (inside the deck at 460).</summary>
        public const double BellyY0 = 330.0, KeelY = 322.0, TopY = 590.0, CentreY = 460.0, HalfUp = TopY - CentreY, HalfDown = CentreY - BellyY0;
        /// <summary>The ship is never lifted (it comes out of the haze, not out of the deck): Lift 0; kept for AU / AT.</summary>
        public const double Lift0 = 0.0, SinkTau = 10.0;
        /// <summary>The deck veil (DR's own analytic occluder quad, NocDreadVeil): off (AT's _NocDeckOcc instead).</summary>
        public const double VeilY = 456.0, VeilHalfW = 390.0, VeilHalfL = 1250.0, VeilR0 = 0.35, VeilK = 0.55;
        public const bool VeilOn = false;
        /// <summary>AT's deck hook _NocDeckOcc (a soft ellipse of beam 0.15 x its half-length): half-length 1 470 (half-width 220 = the
        /// beam) about the ship's centre, strength 0.2 x Emerge (cycle a01: at 0.45 the deck seen over the ship's back, inside its
        /// 5 km footprint, darkened with it and the silhouette lost its contrast).</summary>
        public const double DeckOccHalf = 1470.0, DeckOccW = 0.2;
        /// <summary>Renderers off once the whole ship (its north end, the stern) is 6.5 km south of the walk line (T &lt; 1e-3).</summary>
        public const double HideDist = 6500.0, HideStern = WalkZ - HideDist;

        static readonly double s_d0 = Travel(BowPassTau);
        /// <summary>The bow's distance north of z 195 at the launch (3 422 m).</summary>
        public static double ParkDistance => s_d0;
        public static double ParkBow => WalkZ + s_d0;
        public static double ParkCentre => ParkBow + 0.5 * Length;
        public static double ParkStern => ParkBow + Length;

        /// <summary>Distance travelled (m, toward -z) tau s after the launch (0 before it).</summary>
        public static double Travel(double tau)
        {
            if (tau <= 0.0) return 0.0;
            return Speed * tau + ApproachDv * ApproachK * (1.0 - Math.Exp(-tau / ApproachK));
        }
        /// <summary>Speed (m/s, positive; the motion is toward -z) tau s after the launch; 0 parked.</summary>
        public static double Velocity(double tau) => tau <= 0.0 ? 0.0 : Speed + ApproachDv * Math.Exp(-tau / ApproachK);
        /// <summary>The bow's (leading, south end's) distance north of the walk line z 195 (negative once it has passed).</summary>
        public static double BowDistance(double tau) => s_d0 - Travel(tau);

        /// <summary>Bow (leading, south end) z, centre z, stern (trailing, north end) z tau s after the launch.</summary>
        public static double BowZ(double tau) => WalkZ + BowDistance(tau);
        public static double CentreZ(double tau) => BowZ(tau) + 0.5 * Length;
        public static double SternZ(double tau) => BowZ(tau) + Length;
        public static double Lift(double tau) => 0.0;
        /// <summary>0 parked .. 1 ten seconds after the launch (the deck occluder's weight; the ship is 3 km out then).</summary>
        public static double Emerge(double tau) => DreadClock.Smooth01(0.0, SinkTau, tau);
        public static double BellyY(double tau) => BellyY0;
        /// <summary>s at which the bow passes over z 195 for a walker at constant speed v (596 at 2.5 m/s, 509.6 at 1.5).</summary>
        public static double AbeamS(double v) => LaunchS + BowPassTau * v;
        /// <summary>tau at which the stern passes over z 195 (270.3 s).</summary>
        public static double SternPassTau() => SolveTravel(s_d0 + Length);
        public static double SternPassS(double v) => LaunchS + SternPassTau() * v;
        /// <summary>tau at which Travel(tau) = x (Newton on the monotone travel).</summary>
        public static double SolveTravel(double x)
        {
            double t = Math.Max(0.0, (x - ApproachDv * ApproachK) / Speed);
            for (int i = 0; i < 30; i++) { double v = Velocity(Math.Max(t, 1e-9)); t -= (Travel(t) - x) / v; if (t < 0.0) t = 0.0; }
            return t;
        }
        public static double TauAtS(double s) => DreadClock.CaptureTau(s, LaunchS);
        public static double CentreZAtS(double s) => CentreZ(TauAtS(s));
        public static double SternZAtS(double s) => SternZ(TauAtS(s));
        public static double BowZAtS(double s) => BowZ(TauAtS(s));
        public static double SternZAtSpeed(double s, double v) => SternZ(Math.Max(0.0, (s - LaunchS) / v));
        public static double CentreZAtSpeed(double s, double v) => CentreZ(Math.Max(0.0, (s - LaunchS) / v));
        public static double BowZAtSpeed(double s, double v) => BowZ(Math.Max(0.0, (s - LaunchS) / v));
        public static double CentreOfStern(double stern) => stern - 0.5 * Length;
        /// <summary>Hidden once the stern (the north end) is HideDist south of the walk line (AT reads it with SternZAtS).</summary>
        public static bool Hidden(double stern) => stern < HideStern;
        /// <summary>Hidden before the launch too (parked, the renderers off).</summary>
        public static bool HiddenAt(double tau) => tau <= 0.0 || Hidden(SternZ(tau));

        /// <summary>Elevation (deg) of the bow's belly (x 515.3, y 330, z bow) seen from the eye (x, 1.7, z).</summary>
        public static double BowElevation(double eyeX, double eyeZ, double bow)
        {
            double dx = AxisX - eyeX, dz = bow - eyeZ;
            return Math.Atan2(BellyY0 - NocturneConstants.EyeHeight, Math.Sqrt(dx * dx + dz * dz)) * 180.0 / Math.PI;
        }
        public static double SternElevation(double eyeX, double eyeZ, double stern) => BowElevation(eyeX, eyeZ, stern);

        // ---- the searchlights (D-043: analytic cones in the haze drawn by AT; DR gives the emitters). Six lamps on the keel spine's
        // underside, each tilted SearchTilt from the vertical and sweeping its azimuth slowly about +-x (outward, toward the faces and
        // the abyss) with a period of 28-40 s (>= 20 s: D-010 / D-041), never containing the walker: the gain fades to 0 while the
        // lamp is within 300 m of the walk line z 195 (a cone tilted 25 deg, half-angle 3.5 deg plus a 5 deg margin, reaches 212 m
        // from below the lamp at the walker's height; the walker is within 80 m of z 195 everywhere in the rift, the garden notch
        // z 115-275 included), the fade itself over 200 m (7 s at 28 m/s). DR-t11 (21:07): at 220 m lamp 0 reached (615, 115).
        public const int SearchCount = 6;
        public const double SearchTilt = 25.0, SearchHalfAngle = 3.5, SearchSweep = 50.0, SearchFade0 = 300.0, SearchFade1 = 500.0;
        static readonly double[] s_searchZ = { 600.0, 1180.0, 1960.0, 2740.0, 3520.0, 4300.0 };
        static readonly double[] s_searchPeriod = { 31.0, 37.0, 28.0, 40.0, 34.0, 29.0 };
        /// <summary>Local z (from the bow) of searchlight i.</summary>
        public static double SearchLocalZ(int i) => s_searchZ[i];
        public static double SearchPeriod(int i) => s_searchPeriod[i];
        /// <summary>Searchlight i at tau (course) and scene time t (its sweep): world origin, unit direction (pointing down), gain 0..1.</summary>
        public static void Searchlight(int i, double tau, double t, out Vector3 origin, out Vector3 dir, out double gain)
        {
            double side = (i & 1) == 0 ? 1.0 : -1.0;
            double z = BowZ(tau) + s_searchZ[i];
            origin = new Vector3((float)(AxisX + 9.0 * side), (float)(KeelY - 1.0), (float)z);
            double az = (side > 0 ? 90.0 : -90.0) + SearchSweep * Math.Sin(2.0 * Math.PI * t / s_searchPeriod[i] + 1.7 * i);
            double tl = SearchTilt * Math.PI / 180.0, a = az * Math.PI / 180.0;
            dir = new Vector3((float)(Math.Sin(tl) * Math.Sin(a)), (float)-Math.Cos(tl), (float)(Math.Sin(tl) * Math.Cos(a)));
            double dz = Math.Abs(z - WalkZ);
            gain = tau <= 0.0 ? 0.0 : DreadClock.Smooth01(SearchFade0, SearchFade1, dz);
        }
    }

    /// <summary>DREAD 3.1: the shadow below (course, body, fade, AB's abyss occluder, AT's haze occluder).</summary>
    public static class ShadowCourse
    {
        public const double LaunchS = 600.0, Speed = 5.0, Ramp = 20.0;
        /// <summary>Fix wave C42: x 470 / y -700 (r03: x 690 / y -610 lay in the strip's upper-left corner as cloud shading): under the
        /// KS13 strip's view, between the y -520 subject layer (x 560) and the y -880 river (x 470), centre z 195 at s 719.4.</summary>
        public const double AxisX = 470.0, ParkCentre = 384.0, CentreY = -700.0;
        public const double Length = 900.0, HalfLength = 450.0, HalfWidth = 81.0, HalfHeight = 60.0;
        /// <summary>Fade in over s 590-622, out over s 860-900 (DREAD 3.1: beat 4, s 600-860).</summary>
        public const double FadeIn0 = 590.0, FadeIn1 = 622.0, FadeOut0 = 860.0, FadeOut1 = 900.0;
        /// <summary>Strength of AB's NocStreak occluder (lower rivers inside the outline dim by w x fade) and the body's opacity.</summary>
        public const double OccluderStrength = 0.85, Opacity = 0.92;   // r09: 0.85 -> 0.92 (KS13 body -22 % -> darker)
        /// <summary>The half-length passed to AB's occluder: its half-width is 0.18 x this = 63 m about x 470 (x 407..533), so the
        /// x 560 / y -520 subject layer stays outside (r_x 1.43) while the river x 470 / y -880 lies on its axis.</summary>
        public const double OccluderHalf = 350.0;
        /// <summary>AT's haze hook _NocHazeOcc (the plane y -650: the deep glow and the depths under the sheets x (1 - occ)): half-length
        /// OccluderHalf 350 (beam 0.15 x 350 = 52.5 m: x 417..522; r07's 450 / 67.5 m reached x 537 and darkened the haze behind the
        /// x 560 subject layer by 4 codes in KS13) at strength 0.5 x fade (the body itself adds the soft shape).</summary>
        public const double HazeOccW = 0.5;

        public static Vector4 HazeOccluder(double centreZ, double fade)
        {
            if (fade <= 0.0) return Vector4.zero;
            return new Vector4((float)AxisX, (float)centreZ, (float)OccluderHalf, (float)(HazeOccW * fade));
        }

        public static double CentreZ(double tau) => ParkCentre - DreadClock.Travel(tau, Speed, Ramp);
        public static double CentreZAtS(double s) => CentreZ(DreadClock.CaptureTau(s, LaunchS));
        public static double CentreZAtSpeed(double s, double v) => CentreZ(Math.Max(0.0, (s - LaunchS) / v));
        public static double Fade(double s) => DreadClock.Smooth01(FadeIn0, FadeIn1, s) * (1.0 - DreadClock.Smooth01(FadeOut0, FadeOut1, s));

        /// <summary>AB's _NocAbyssOccluder value: (centre x, centre z, half-length, strength x fade); all zero when faded out.</summary>
        public static Vector4 Occluder(double centreZ, double fade)
        {
            if (fade <= 0.0) return Vector4.zero;
            return new Vector4((float)AxisX, (float)centreZ, (float)OccluderHalf, (float)(OccluderStrength * fade));
        }
    }

    /// <summary>DREAD 3.2: the falling panel. A 12 x 8 m backlit sign panel (no text) on face B (x 815.3, the planar part) at y 150,
    /// z 800 (in the gaze of s 470: yaw 40, pitch -6), tears off at s 470: it swings out about its top edge (30 deg, tau 1 s), is
    /// pushed off the face (27 m, tau 3 s), drifts south with the wind (4 m/s, tau 3 s; clear of the z 850 cross bridge by 50 m),
    /// falls with drag (terminal 42 m/s, tau_d = 42 / 9.81 s: 662 m in 20 s, into the haze) and tumbles at 0.08 rev/s about a tilted
    /// axis (its lit face turns at most 0.16 Hz: the D-010 modulation limit 0.2 Hz). Hidden past tau 50 s (y below -1 700).</summary>
    public static class PanelCourse
    {
        public const double LaunchS = 470.0;
        public const double FaceX = 815.3, Y0 = 150.0, Z0 = 800.0, Width = 12.0, Height = 8.0, Thick = 0.4, Proud = 0.25;
        public const double OutDist = 27.0, OutTau = 3.0, DriftSpeed = 4.0, DriftTau = 3.0;
        public const double Terminal = 42.0, Gravity = 9.81, SwingDeg = 30.0, SwingTau = 1.0, TumbleHz = 0.08, HideTau = 50.0;

        public static double Drop(double tau)
        {
            if (tau <= 0.0) return 0.0;
            double td = Terminal / Gravity;
            return Terminal * (tau - td * (1.0 - Math.Exp(-tau / td)));
        }

        /// <summary>Centre of the panel after tau seconds (parked: on the face, its back 0.25 m proud of x 815.3).</summary>
        public static Vector3 Centre(double tau)
        {
            double t = Math.Max(0.0, tau);
            double x = FaceX - Proud - 0.5 * Thick - OutDist * (1.0 - Math.Exp(-t / OutTau));
            double z = Z0 - DriftSpeed * (t - DriftTau * (1.0 - Math.Exp(-t / DriftTau)));
            return new Vector3((float)x, (float)(Y0 - Drop(t)), (float)z);
        }

        /// <summary>Orientation after tau: the swing out about the top edge's axis (z), then the slow tumble (0 at tau 0: C0 and C1).</summary>
        public static Quaternion Rotation(double tau)
        {
            double t = Math.Max(0.0, tau);
            double swing = SwingDeg * (1.0 - Math.Exp(-t / SwingTau));
            // tumble angle: 360 TumbleHz (t - 2 (1 - e^(-t / 2))): starts at rate 0, reaches 0.08 rev/s
            double tumble = 360.0 * TumbleHz * (t - 2.0 * (1.0 - Math.Exp(-t / 2.0)));
            var axis = new Vector3(0.35f, 0.25f, 1f).normalized;
            return Quaternion.AngleAxis((float)tumble, axis) * Quaternion.AngleAxis((float)-swing, Vector3.forward);
        }

        public static double TauAtS(double s) => DreadClock.CaptureTau(s, LaunchS);
        public static bool Hidden(double tau) => tau > HideTau;
    }

    /// <summary>The pure courses of the elevator cabins and the gantries (docs/DREAD.md 2.2; integration round, AU-28 / DR's request):
    /// Mesh-free, so the audio (DreadScore's machine hums) and AU's offline harness place the hums on the very cabins and gantries
    /// of the picture. DreadCabins (meshes, lights) and DreadBuilder.GantryPos read these; nothing is copied.
    ///   Cabins: 8 shafts (4 on face A, 4 on face B) x 4 cabins, y = y0 + L frac(phase + v t / L) (down: y1 - ...) on the traffic
    ///   clock (wrapped mod 65 536 s: v is quantised so the wrap is a whole number of shaft periods, so the raw clock gives the same y),
    ///   the light ramping to 0 over the last 2 % at both hidden ends. Gantries: z(t) = z_c + A sin(2 pi t / P + phi) on the scene
    ///   clock, peak speed 0.5 m/s, at x 815.3 - GantryGap - 0.4 and the truss bottom y.</summary>
    public static class DreadCabinCourse
    {
        public const double Y0 = -2400.0, Y1 = 600.0, ShaftL = 3000.0, Proud = 2.0, ClockWrap = 65536.0;

        /// <summary>Shafts (face x of the cabin centre line, z): four on face A (facing +x), four on face B (facing -x).</summary>
        public static readonly double[,] Shafts =
        {
            { 215.3 + Proud, -60.0 }, { 215.3 + Proud, 30.0 }, { 215.3 + Proud, 350.0 }, { 215.3 + Proud, 440.0 },
            { 815.3 - Proud, -70.0 }, { 815.3 - Proud, 40.0 }, { 815.3 - Proud, 330.0 }, { 815.3 - Proud, 420.0 },
        };
        public const int CabinsPerShaft = 4;
        public static int ShaftCount => Shafts.GetLength(0);
        public static int CabinCount => ShaftCount * CabinsPerShaft;

        /// <summary>Speed quantised so that the clock's wrap (65 536 s) is a whole number of shaft periods (exact wrap).</summary>
        public static double QuantiseSpeed(double v) => Math.Round(v * ClockWrap / ShaftL) * ShaftL / ClockWrap;

        public struct Cabin
        {
            public int Shaft;
            public double X, Z, Speed, Phase;
            public int Dir;      // +1 up, -1 down
            public bool Warm;    // a WinWarm cab (70 %), else White (DreadLights)
        }

        /// <summary>Cabin c of shaft s: speed 3-8 m/s and phase from a hash per (shaft, cabin), alternating up / down.</summary>
        public static Cabin CabinAt(int s, int c)
        {
            double u1 = U01(H3((uint)s, (uint)c, 0xD4EADu)), u2 = U01(H3((uint)s, (uint)c, 0xD4EAEu));
            return new Cabin
            {
                Shaft = s, X = Shafts[s, 0], Z = Shafts[s, 1],
                Speed = QuantiseSpeed(3.0 + 5.0 * u1),
                Phase = (c + 0.6 * u2) / CabinsPerShaft,
                Dir = (c & 1) == 0 ? 1 : -1,
                Warm = u2 < 0.7,
            };
        }

        /// <summary>The 32 cabins, shaft by shaft (index = shaft x CabinsPerShaft + cabin).</summary>
        public static Cabin[] Cabins()
        {
            var a = new Cabin[CabinCount];
            for (int s = 0; s < ShaftCount; s++) for (int c = 0; c < CabinsPerShaft; c++) a[s * CabinsPerShaft + c] = CabinAt(s, c);
            return a;
        }

        /// <summary>Twin of NocDreadLight's mover: the cabin's y at traffic-clock time t (s), and its light ramp (0 at the hidden ends).</summary>
        public static double CabinY(in Cabin c, double t, out double ramp)
        {
            double p = c.Phase + c.Speed * t / ShaftL;
            p -= Math.Floor(p);
            ramp = DreadClock.Smooth01(0.0, 0.02, p) * (1.0 - DreadClock.Smooth01(0.98, 1.0, p));
            return c.Dir > 0 ? Y0 + ShaftL * p : Y1 - ShaftL * p;
        }

        // ---- gantries
        public const double GantryL = 50.0, GantryH = 6.0, GantryD = 5.0, GantryGap = 3.0, GantrySpeed = 0.5;
        /// <summary>Gantries: (y of the truss bottom, track centre z, phase rad, amplitude m) on the planar parts of face B: the north
        /// flank z 285..555 (north of the garden notch z 115..275) and the south flank z -110..110.</summary>
        public static readonly double[,] Gantries = { { 205.0, 420.0, 0.7, 110.0 }, { -330.0, 0.0, 2.6, 85.0 } };
        public static int GantryCount => Gantries.GetLength(0);
        /// <summary>x of a gantry's face-side chords (its transform origin): 3 m off face B plus the 0.4 m chord half-depth.</summary>
        public const double GantryX = 815.3 - GantryGap - 0.4;

        /// <summary>Period of gantry i: 2 pi A / 0.5 (peak speed 0.5 m/s).</summary>
        public static double GantryPeriod(int i) => 2.0 * Math.PI * Gantries[i, 3] / GantrySpeed;
        public static double GantryY(int i) => Gantries[i, 0];
        public static double GantryZ(int i, double t) => Gantries[i, 1] + Gantries[i, 3] * Math.Sin(2.0 * Math.PI * t / GantryPeriod(i) + Gantries[i, 2]);

        // the PCG3D hash of NocHash.H3 (k = 0) and NocHash.U01 (Rendering/NocFacadeMath.cs), here so this class needs nothing outside
        // System (AU's offline harness compiles DreadCourse.cs alone); DreadTests checks that the two agree
        public static uint H3(uint a, uint b, uint c)
        {
            unchecked
            {
                a = a * 1664525u + 1013904223u; b = b * 1664525u + 1013904223u; c = c * 1664525u + 1013904223u;
                a += b * c; b += c * a; c += a * b;
                a ^= a >> 16; b ^= b >> 16; c ^= c >> 16;
                a += b * c;
                return a;
            }
        }

        public static double U01(uint h) => (h >> 8) * (1.0 / 16777216.0);
    }
}

