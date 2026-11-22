// TramSchedule: the route, the timing and the state machine of the tram T_A (MASTER_PROMPT 14.5.2 "Moving", 14.7 H7, 14.10
// T-TRAIN; owner BV, Phase 4 wave A). Pure functions of (s, tau): EditMode tests (TramTests) and the TramDriver share them.
//
// Timing (14.5.2, H7): the tram is launched when s first crosses s_launch = 230 and then runs on real time tau (seconds since the
// launch) at 7 m/s: on the straight the head is at x(tau) = 205 - 7 tau on the north track z 227. Capture mode (stills and
// sequences) uses tau = max(0, (s - 230) / 2.5), a pure function of s. Meeting points: the head is abeam the walker
// (x = s - 184.7) at s = (389.7 + 1610 / v) / (1 + 7 / v): 272.03 at 2.5 m/s, 258.18 at 1.5 m/s; at s 264 in capture mode the
// head is at x 109.8 (KS06: x 109.8..169.8). The walker's axis is z 195, the track z 227: the tram passes at 32.0 m.
//
// Route (arc length p of the head from its launch point (205, 227)): the straight north track westward to x -16, a right-hand
// quarter circle r 35 about (-16, 262) to (-51, 262), then due north through the depot portal in the north podium's street face
// (x -53.6..-48.4 at z 270, west of the arcade) into the depot tunnel. Negative p (cars behind the head at launch) continue the
// straight east into the terminus (x <= 265 at the tail).
//
// Never a pop (14.6, Part 10 item 36): the tram is drawn with Megastructure/NocTram, which discards every fragment beyond two
// planes, both inside sealed meshes: x > ClipX inside the terminus hall (2 cm behind its east interior wall x 214.45; the 50 m of the
// tram that stick out of the 10 m hall at launch are never drawn), and z > ClipZ, 8 m inside the depot tunnel (seen from
// s >= 218 the tunnel shows at most 4 m of its depth: the portal faces south and the walker looks at it at >= 48 deg). The tram
// is done (fully clipped) once its tail is past the depot clip: p_head >= HiddenP (tau 50.33 s).
//
// Reset (H7: "if s falls 12 m below s_launch the vehicle resets"): the reset is armed when s < 218 and applied at the first
// moment the tram is invisible (still at the terminus, or done inside the depot), so a walker who turns back never sees the
// tram jump; the next crossing of s 230 launches it again (proposed decision BV-D1).
//
// Dwell (DREAD 6, BV fix wave): before the launch the tram waits at the stop with its south doors open on the lit empty saloon
// (DoorOpen: fully open from s 196.25, 12 s at 2.5 m/s before the chime), closes them with AU's chime at DreadScore.ChimeS 226.25
// and leaves at s 230 (unchanged). The platform side faces the terminus hall's south half; from the median the open doors are
// inside the hall (the walker sees the stop through the 5.2 m portal only, at 150-200 m).
using System;
using UnityEngine;

namespace Megastructure
{
    public enum TramPhase { Waiting = 0, Running = 1, Done = 2 }

    /// <summary>The play-mode state of the tram (one per driver; tests step it directly).</summary>
    public struct TramState
    {
        public TramPhase Phase;
        public double Tau;          // s since the launch (real time, NocState.Dt)
        public bool ResetArmed;     // s fell below s_launch - 12 while the tram was running
        public bool Armed;          // the next crossing of s_launch launches the tram
        public double PrevS;        // s of the previous frame (crossing detection)
        public bool Init;
    }

    public static class TramSchedule
    {
        // ---- timing (14.5.2, H7)
        public const double LaunchS = 230.0, ResetDrop = 12.0, Speed = 7.0, CaptureSpeed = 2.5;
        /// <summary>Head x at launch (the terminus portal plane x 205) and the north track's z (14.5.2).</summary>
        public const double StartX = 205.0, TrackZ = 227.0, WalkAxisZ = 195.0;

        // ---- body (14.5.2: 60 x 3.4 x 3.6 m); five modules of 11.68 m with four 0.4 m gangways
        public const double Length = 60.0, Width = 3.4, Height = 3.6, BodyY0 = 0.3, BodyY1 = 3.9;
        public const int Cars = 5;
        public const double CarL = 11.68, Gangway = 0.4;
        /// <summary>Bogie centres from the car's ends (the car's chord between them follows the track).</summary>
        public const double BogieInset = 2.2;
        /// <summary>The car that carries the pantograph (from the head, 0-based) and the overhead contact wire height.</summary>
        public const int PantoCar = 2;
        public const double WireY = 5.6, MessengerY = 6.7, PoleZ = 230.4, PolePitch = 30.0, PoleH = 7.4;

        // ---- route
        public const double CurveX0 = -16.0, CurveR = 35.0;
        public const double PortalX = -51.0, PortalZ = 270.0, PortalW = 5.2, PortalH = 6.4;
        /// <summary>Clip planes of NocTram (world): x > ClipX inside the terminus, z > ClipZ inside the depot tunnel.</summary>
        public const double ClipX = 214.47, ClipZ = 278.0;
        /// <summary>The depot tunnel's depth behind the portal (z 270..300) and its interior width / height.</summary>
        public const double TunnelZ1 = 300.0;

        public static double StraightL => StartX - CurveX0;                    // 221.0
        public static double ArcL => CurveR * Math.PI * 0.5;                    // 54.978
        public static double CurveEndZ => TrackZ + CurveR;                      // 262.0
        public static double PortalP => StraightL + ArcL + (PortalZ - CurveEndZ);   // 283.98: the head at the portal plane
        /// <summary>The head's p when the tail (and its 0.11 m coupler cover) has passed the depot clip plane: nothing is drawn.</summary>
        public static double HiddenP => StraightL + ArcL + (ClipZ - CurveEndZ) + Length + HiddenMargin;   // 352.28
        public const double HiddenMargin = 0.3;
        public static double HiddenTau => HiddenP / Speed;                       // 50.33 s

        /// <summary>Capture-mode tau (stills and sequences): max(0, (s - 230) / 2.5).</summary>
        public static double CaptureTau(double s) => Math.Max(0.0, (s - LaunchS) / CaptureSpeed);

        /// <summary>Arc length of the head after tau seconds.</summary>
        public static double HeadP(double tau) => Speed * Math.Max(0.0, tau);

        /// <summary>Head x on the straight (14.5.2: x(tau) = 205 - 7 tau).</summary>
        public static double HeadX(double tau) => StartX - Speed * Math.Max(0.0, tau);

        /// <summary>The route point (x, z) and unit heading at arc length p (negative p: the straight continued east).</summary>
        public static void Point(double p, out double x, out double z, out double hx, out double hz)
        {
            if (p <= StraightL) { x = StartX - p; z = TrackZ; hx = -1.0; hz = 0.0; return; }
            double a = p - StraightL;
            if (a <= ArcL)
            {
                double th = a / CurveR, c = Math.Cos(th), sn = Math.Sin(th);
                x = CurveX0 - CurveR * sn; z = CurveEndZ - CurveR * c;
                hx = -c; hz = sn;
                return;
            }
            x = CurveX0 - CurveR; z = CurveEndZ + (a - ArcL); hx = 0.0; hz = 1.0;
        }

        public static Vector3 PointV(double p, double y = 0.0)
        {
            Point(p, out double x, out double z, out _, out _);
            return new Vector3((float)x, (float)y, (float)z);
        }

        /// <summary>Arc offset of car k's front end behind the head (k 0 = the head car).</summary>
        public static double CarFront(int k) => k * (CarL + Gangway);

        /// <summary>The walker's s at which the head is abeam (x_head = s - 184.7) for a constant walking speed v (m/s).</summary>
        public static double AbeamS(double v)
        {
            double off = NocturneConstants.EastLegOffset;
            // 205 - 7 (s - 230) / v = s - off  ->  s = (205 + off + 7 * 230 / v) / (1 + 7 / v)
            return (StartX + off + Speed * LaunchS / v) / (1.0 + Speed / v);
        }

        /// <summary>True when every fragment of the tram lies beyond a clip plane (nothing drawn) for a head at p.</summary>
        public static bool Hidden(double p) => p >= HiddenP;

        /// <summary>True while the head has not left the terminus portal (the tram can be reset without a visible jump).</summary>
        public static bool AtTerminus(double p) => p <= 0.3;

        public static void Reset(ref TramState st)
        {
            st.Phase = TramPhase.Waiting; st.Tau = 0.0; st.ResetArmed = false; st.Armed = true; st.Init = false; st.PrevS = double.NaN;
        }

        /// <summary>
        /// One play-mode frame: s this frame, dt (real seconds, 0 while paused). Launch on the first crossing of s 230 (or at once
        /// when the walk starts beyond it); arm the reset below s 218; apply it only while the tram is invisible. Returns tau.
        /// </summary>
        public static double Step(ref TramState st, double s, double dt)
        {
            if (!st.Init) { st.Init = true; st.PrevS = s; st.Armed = true; st.Phase = TramPhase.Waiting; st.Tau = 0.0; st.ResetArmed = false; }
            if (s < LaunchS - ResetDrop) { st.ResetArmed = true; }
            switch (st.Phase)
            {
                case TramPhase.Waiting:
                    if (st.ResetArmed) { st.Armed = true; st.ResetArmed = false; }
                    if (st.Armed && s >= LaunchS) { st.Phase = TramPhase.Running; st.Tau = 0.0; st.Armed = false; }
                    break;
                case TramPhase.Running:
                    st.Tau += Math.Max(0.0, dt);
                    if (st.ResetArmed && AtTerminus(HeadP(st.Tau))) { st.Phase = TramPhase.Waiting; st.Tau = 0.0; st.Armed = true; st.ResetArmed = false; }
                    else if (Hidden(HeadP(st.Tau)))
                    {
                        if (st.ResetArmed) { st.Phase = TramPhase.Waiting; st.Tau = 0.0; st.Armed = true; st.ResetArmed = false; }
                        else st.Phase = TramPhase.Done;
                    }
                    break;
                case TramPhase.Done:
                    if (st.ResetArmed) { st.Phase = TramPhase.Waiting; st.Tau = 0.0; st.Armed = true; st.ResetArmed = false; }
                    break;
            }
            st.PrevS = s;
            return st.Phase == TramPhase.Running ? st.Tau : (st.Phase == TramPhase.Done ? HiddenTau : 0.0);
        }

        /// <summary>The headlight's strength (1 on the street, fading to 0 over the last 12 m before the depot portal: a light
        /// inside the podium would shine through its walls, there are no shadow maps).</summary>
        public static double HeadlightFade(double pHead) => 1.0 - NocMath.Smootherstep(PortalP - 14.0, PortalP - 2.0, pHead);


        // ---- DREAD 6 (D-034; lead ruling 2026-11-21, panel p4_01 C29): the empty tram's dwell at the boulevard stop. T_A stands
        // at the terminus platform (p 0, the platform on its south side, z 212-225) with its south doors open on the lit empty
        // saloon; the doors are fully open DwellSeconds (12 s at the reference 2.5 m/s) before the chime, close with AU's door chime
        // at s DreadScore.ChimeS (226.25 = 230 - 1.5 s x 2.5 m/s; AU's request: doorsClosing = s >= DreadScore.ChimeS) and are shut
        // 1.2 s later (s 229.25), before the launch at s 230: the 14.5.2 schedule, the abeam s and KS06 are unchanged.
        public const double DwellSeconds = 12.0, DoorOpenSeconds = 1.6, DoorCloseSeconds = 1.2, DoorRateSeconds = 0.8;
        /// <summary>The leaves' travel (each of the two leaves of a door slides this far along the body, plug-sliding).</summary>
        public const double DoorTravel = 0.64;
        /// <summary>s where the doors start closing (the chime) and where they start opening (fully open 12 s before the chime).</summary>
        public static double DoorsCloseS => DreadScore.ChimeS;
        public static double DoorsOpenS => DoorsCloseS - DreadScore.M(DwellSeconds + DoorOpenSeconds);
        /// <summary>The doors' opening (0 shut .. 1 open) as a function of s: capture mode uses it directly, play mode follows it
        /// at most 1 / DoorRateSeconds (0.8 s) per second (a teleport never snaps a leaf; shut by s 229.7 at 2.5 m/s). Shut from s 229.25 on.</summary>
        public static double DoorOpen(double s)
            => NocMath.Smootherstep(DoorsOpenS, DoorsOpenS + DreadScore.M(DoorOpenSeconds), s)
             * (1.0 - NocMath.Smootherstep(DoorsCloseS, DoorsCloseS + DreadScore.M(DoorCloseSeconds), s));
        /// <summary>True when the doors are closing or shut for good at s (AU's chime rule).</summary>
        public static bool DoorsClosing(double s) => s >= DreadScore.ChimeS;
        /// <summary>One play-mode step of the doors toward DoorOpen(s); only while the tram waits at the stop (else shut).</summary>
        public static double StepDoors(double current, double s, double dt, bool waiting)
        {
            double target = waiting ? DoorOpen(s) : 0.0;
            double rate = Math.Max(0.0, dt) / DoorRateSeconds;
            if (target > current) return Math.Min(target, current + rate);
            return Math.Max(target, current - rate);
        }

        /// <summary>A light riding at arc position p: 0 inside the terminus (p below -4) or the depot, 1 on the street.</summary>
        public static double RideLightFade(double p)
            => NocMath.Smootherstep(-6.0, 2.0, p) * (1.0 - NocMath.Smootherstep(PortalP - 12.0, PortalP - 1.0, p));
    }
}
