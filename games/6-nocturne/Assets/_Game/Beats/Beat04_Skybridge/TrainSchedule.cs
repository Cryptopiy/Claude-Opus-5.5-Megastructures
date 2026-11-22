// TrainSchedule: the tubes, the timing and the state machine of the trains T1-T7 of beat 4 (MASTER_PROMPT 14.5.4, 14.7 H7, 14.10
// T-TRAIN; owner TR, Phase 4 wave B). Pure functions of (s, tau) in double: TrainTests, the visibility audit and TrainDriver share
// them. The design is BV's tram (TramSchedule): launch keyed to s, then real time.
//
// Timing (14.5.4): train i is launched when s first crosses its s_launch and then runs on real time tau (seconds since the launch):
// the nose is at x(tau) = x0 + dir 38 tau on its tube axis (dir -1 west, +1 east); capture mode (stills and sequences) uses
// tau = max(0, (s - s_launch) / 2.5), a pure function of s. Meeting points (nose abeam the walker, x_w = s - 184.70):
//   s = (x0 + 184.70 - dir 38 s_L / v) / (1 - dir 38 / v):
//   T1 602.0 (595.3 at 1.5 m/s), T3 653.6 (648.2), T4 699.3 (695.7), T2 759.1 (756.4), T7 816.3 (809.5): every pass differs by
//   6.8 m or less between 1.5 and 2.5 m/s (14.5.4: 7 m). T6 is not a meeting: its nose passes under the glass strip centre x 531.3
//   at s 715.8 (2.5 m/s) and 707.5 (1.5 m/s), both while the walker is on the strip (s 706-726).
//
// Tubes (index): 0 N (z 207) and 1 S (z 183), r 4.5, axis y 2.0, open between the gate mouths x 384 and 683, blind tunnels 11.9 m
// into the gate blocks (SkybridgeGate, ends x 372.1 / 694.9); 2 upper (y 118, z 195, r 5), open between the canyon bridge's collar
// lip x 233.25 and the keystone's lip x 812.5 (GE's portals; tunnels to x 216.25 and 817.5); 3 lower (y -142, z 209, r 5), open
// between the Gulf portal lips x 216.3 and 814.3 (AB's GulfPortal; tunnels to x 185.3 and 845.3).
//
// Never a pop (14.6, Part 10 item 36; "nothing shows before it emerges"): Megastructure/NocTrain multiplies the shaded colour by
// a darkness factor that falls from 1 at a mouth plane to 0 at MouthFade metres inside the tunnel, and discards every fragment
// beyond the tunnel ends (clip planes 3 cm behind the end discs, inside the sealed masses). The tunnel linings (TrainBuilder) are
// drawn with the same material, so a mouth reads as a black hole and a train emerges from it and vanishes into it. Before its
// launch a train is disabled with its nose on the origin mouth plane (every fragment inside the tunnel); after it is done (the
// tail past the destination tunnel end, every fragment clipped) it is disabled again. Its moving lights fade in after the
// nose has emerged and out before they reach a mouth (there are no shadow maps: a light inside a tunnel would shine through it).
//
// Reset (H7: "if s falls 12 m below s_launch the vehicle resets"): armed when s < s_launch - 12, applied only while the train is
// invisible (waiting or done); the next crossing of s_launch launches it again (BV-D1's rule).
using System;
using UnityEngine;

namespace Megastructure
{
    public enum TrainPhase { Waiting = 0, Running = 1, Done = 2 }

    public struct TrainState
    {
        public TrainPhase Phase;
        public double Tau;
        public bool ResetArmed;
        public bool Armed;
        public bool Init;
    }

    /// <summary>One tube: axis (y, z), radius, the open section between the mouth planes and the tunnel ends (world x).</summary>
    public struct TrainTube
    {
        public string Name;
        public double AxisY, AxisZ, R, MouthLo, MouthHi, EndLo, EndHi;
        public TrainTube(string n, double y, double z, double r, double endLo, double mouthLo, double mouthHi, double endHi)
        { Name = n; AxisY = y; AxisZ = z; R = r; EndLo = endLo; MouthLo = mouthLo; MouthHi = mouthHi; EndHi = endHi; }
        public double OpenLength => MouthHi - MouthLo;
    }

    /// <summary>One train of the 14.5.4 table.</summary>
    public struct TrainSpec
    {
        public string Name;
        public double LaunchS, X0;
        public int Tube, Dir;
        public TrainSpec(string n, double sl, int tube, int dir, double x0) { Name = n; LaunchS = sl; Tube = tube; Dir = dir; X0 = x0; }
    }

    public static class TrainSchedule
    {
        // ---- timing and body (14.5.4: 7 cars of 24 m plus a 6 m nose = 174 m, 3.4 m wide, 38 m/s)
        public const double Speed = 38.0, CaptureSpeed = 2.5, ResetDrop = 12.0;
        public const int Cars = 7;
        public const double CarPitch = 24.0, NoseL = 6.0, Gap = 0.6, Width = 3.4, BodyHalf = 1.7;
        public static double Length => Cars * CarPitch + NoseL;                       // 174.0
        /// <summary>Margin beyond a tunnel end before a train counts as fully clipped (the coupler cover and the tail lamps).</summary>
        public const double HiddenMargin = 0.5;
        /// <summary>Darkness inside a mouth: 1 at the mouth plane, 0 at MouthFade metres in (NocTrain _Fade.x).</summary>
        public const double MouthFade = 2.5;
        /// <summary>Clip planes lie this far behind the tunnel end discs (inside the sealed masses).</summary>
        public const double ClipBehindEnd = 0.03;

        // ---- tubes
        public const int TubeN = 0, TubeS = 1, TubeUpper = 2, TubeLower = 3;
        /// <summary>Depth of the bridge-tube tunnels behind the gate mouths (SkybridgeGate; the gate blocks are 12 m deep).</summary>
        public const double GateTunnelDepth = SkybridgeDims.TunnelDepth;

        public static readonly TrainTube[] Tubes =
        {
            new TrainTube("N", SkybridgeDims.TubeAxisY, SkybridgeDims.TubeNZ, SkybridgeDims.TubeR,
                SkybridgeDims.TubeX0 - GateTunnelDepth, SkybridgeDims.TubeX0, SkybridgeDims.TubeX1, SkybridgeDims.TubeX1 + GateTunnelDepth),
            new TrainTube("S", SkybridgeDims.TubeAxisY, SkybridgeDims.TubeSZ, SkybridgeDims.TubeR,
                SkybridgeDims.TubeX0 - GateTunnelDepth, SkybridgeDims.TubeX0, SkybridgeDims.TubeX1, SkybridgeDims.TubeX1 + GateTunnelDepth),
            // upper: GE's canyon-bridge portal (east face x 232.25, collar lip 1 m, tunnel 16 m) and keystone portal (west face 813.5,
            // lip 1 m, tunnel to 817.5)
            new TrainTube("upper", SkybridgeDims.UpperY, SkybridgeDims.UpperZ, SkybridgeDims.FaceTubeR,
                GateDims.CbX1 - GateDims.CbTunnelDepth, GateDims.CbX1 + EdgeDims.PortalLip,
                GateDims.KeyX0 - EdgeDims.PortalLip, GateDims.KeyX0 + (GateDims.KeyX1 - GateDims.KeyX0 - 0.5)),
            // lower: AB's Gulf portals (face planes 215.3 / 815.3, collar lip 1 m toward the rift, tunnels 30 m into the masses)
            new TrainTube("lower", SkybridgeDims.LowerY, SkybridgeDims.LowerZ, SkybridgeDims.FaceTubeR,
                SkybridgeDims.FaceAX - EdgeDims.PortalDepth, SkybridgeDims.FaceAX + EdgeDims.PortalLip,
                SkybridgeDims.FaceBX - EdgeDims.PortalLip, SkybridgeDims.FaceBX + EdgeDims.PortalDepth),
        };

        // ---- the 14.5.4 table (launch s, tube, direction, origin), in T-number order
        public static readonly TrainSpec[] Trains =
        {
            new TrainSpec("T1", 584.5, TubeN, -1, SkybridgeDims.TubeX1),
            new TrainSpec("T2", 752.0, TubeS, -1, SkybridgeDims.TubeX1),
            new TrainSpec("T3", 639.5, TubeN, -1, SkybridgeDims.TubeX1),
            new TrainSpec("T4", 690.7, TubeS, +1, SkybridgeDims.TubeX0),
            new TrainSpec("T5", 600.0, TubeUpper, -1, SkybridgeDims.FaceBX),
            new TrainSpec("T6", 695.0, TubeLower, +1, SkybridgeDims.FaceAX),
            new TrainSpec("T7", 800.0, TubeN, +1, SkybridgeDims.TubeX0),
        };

        public static int Index(string name)
        {
            for (int i = 0; i < Trains.Length; i++) if (Trains[i].Name == name) return i;
            return -1;
        }

        /// <summary>Capture-mode tau of train i at s: max(0, (s - s_launch) / 2.5).</summary>
        public static double CaptureTau(int i, double s) => Math.Max(0.0, (s - Trains[i].LaunchS) / CaptureSpeed);

        /// <summary>Nose x of train i after tau seconds (14.5.4: x0 -+ 38 tau).</summary>
        public static double NoseX(int i, double tau) => Trains[i].X0 + Trains[i].Dir * Speed * Math.Max(0.0, tau);

        /// <summary>Tail x (the rear face of the last car) for a nose at noseX.</summary>
        public static double TailX(int i, double noseX) => noseX - Trains[i].Dir * Length;

        /// <summary>tau at which train i is fully clipped behind its destination tunnel end (tail + margin past the clip plane).</summary>
        public static double HiddenTau(int i)
        {
            var t = Trains[i]; var tb = Tubes[t.Tube];
            double end = t.Dir < 0 ? tb.EndLo - ClipBehindEnd : tb.EndHi + ClipBehindEnd;
            // tail x = nose - dir L must pass `end` by HiddenMargin: nose = end + dir (L + margin)
            double nose = end + t.Dir * (Length + HiddenMargin);
            return Math.Abs(nose - t.X0) / Speed;
        }

        /// <summary>The walker's s at which the nose of train i is abeam (x_nose = s - 184.70), constant walking speed v.</summary>
        public static double MeetS(int i, double v)
        {
            var t = Trains[i];
            double k = t.Dir * Speed / v;
            // x0 + dir V (s - sL) / v = s - off  ->  s (1 - k) = x0 + off - k sL
            return (t.X0 + NocturneConstants.EastLegOffset - k * t.LaunchS) / (1.0 - k);
        }

        /// <summary>The walker's s at which the nose of train i reaches world x, constant walking speed v.</summary>
        public static double SAtNoseX(int i, double x, double v)
        {
            var t = Trains[i];
            double tau = (x - t.X0) / (t.Dir * Speed);
            return t.LaunchS + v * tau;
        }

        /// <summary>Darkness factor of NocTrain at world x in tube `tube` (1 in the open section, 0 deeper than MouthFade).</summary>
        public static double Lit(int tube, double x)
        {
            var tb = Tubes[tube];
            double inside = Math.Max(tb.MouthLo - x, x - tb.MouthHi);
            return 1.0 - NocMath.Smoothstep(0.0, MouthFade, inside);
        }

        /// <summary>True when world x is drawn by NocTrain in tube `tube` (between the clip planes).</summary>
        public static bool Drawn(int tube, double x)
        {
            var tb = Tubes[tube];
            return x >= tb.EndLo - ClipBehindEnd && x <= tb.EndHi + ClipBehindEnd;
        }

        /// <summary>Signed distance of world x inside the open section of tube `tube` (negative inside a tunnel).</summary>
        public static double OpenDepth(int tube, double x)
        {
            var tb = Tubes[tube];
            return Math.Min(x - tb.MouthLo, tb.MouthHi - x);
        }

        /// <summary>A moving light's strength at world x: 0 at and behind a mouth, smootherstep to 1 at `ramp` metres into the open
        /// section (a light inside a tunnel would shine through its walls; the headlight starts 4 m out at launch).</summary>
        public static double LightFade(int tube, double x, double start, double ramp)
            => NocMath.Smootherstep(start, start + ramp, OpenDepth(tube, x));

        public static void Reset(ref TrainState st)
        {
            st.Phase = TrainPhase.Waiting; st.Tau = 0.0; st.ResetArmed = false; st.Armed = true; st.Init = false;
        }

        /// <summary>
        /// One play-mode frame of train i: s this frame, dt (real seconds, 0 while paused). Launch on the first crossing of s_launch
        /// (or at once when the walk starts beyond it); arm the reset below s_launch - 12; apply it only while the train is invisible.
        /// Returns tau (0 while waiting, HiddenTau once done).
        /// </summary>
        public static double Step(int i, ref TrainState st, double s, double dt)
        {
            double sl = Trains[i].LaunchS;
            if (!st.Init) { st.Init = true; st.Armed = true; st.Phase = TrainPhase.Waiting; st.Tau = 0.0; st.ResetArmed = false; }
            if (s < sl - ResetDrop) st.ResetArmed = true;
            switch (st.Phase)
            {
                case TrainPhase.Waiting:
                    if (st.ResetArmed) { st.Armed = true; st.ResetArmed = false; }
                    if (st.Armed && s >= sl) { st.Phase = TrainPhase.Running; st.Tau = 0.0; st.Armed = false; }
                    break;
                case TrainPhase.Running:
                    st.Tau += Math.Max(0.0, dt);
                    if (st.Tau >= HiddenTau(i))
                    {
                        st.Tau = HiddenTau(i);
                        if (st.ResetArmed) { st.Phase = TrainPhase.Waiting; st.Tau = 0.0; st.Armed = true; st.ResetArmed = false; }
                        else st.Phase = TrainPhase.Done;
                    }
                    break;
                case TrainPhase.Done:
                    if (st.ResetArmed) { st.Phase = TrainPhase.Waiting; st.Tau = 0.0; st.Armed = true; st.ResetArmed = false; }
                    break;
            }
            return st.Phase == TrainPhase.Running ? st.Tau : (st.Phase == TrainPhase.Done ? HiddenTau(i) : 0.0);
        }

        /// <summary>Capture-mode or play-mode visibility of train i at tau: drawn only while running (0 &lt; tau &lt; HiddenTau).</summary>
        public static bool Active(int i, double tau) => tau > 0.0 && tau < HiddenTau(i);

        /// <summary>World position of the train frame's origin (nose tip on the tube axis) and the yaw that points +x along dir.</summary>
        public static Vector3 FramePos(int i, double tau)
        {
            var tb = Tubes[Trains[i].Tube];
            return new Vector3((float)NoseX(i, tau), (float)tb.AxisY, (float)tb.AxisZ);
        }

        public static Quaternion FrameRot(int i) => Trains[i].Dir > 0 ? Quaternion.identity : Quaternion.Euler(0f, 180f, 0f);

        /// <summary>Where a train's (disabled) renderer is parked while it waits or is done: far under the world (nothing is built
        /// below y -3 300), so its 174 m body never lies over a walkable area for ray probes and audits (Q1IntegrationTests B).</summary>
        public const float ParkY = -4500f;
        public static Vector3 ParkPos(int i) => new Vector3((float)Trains[i].X0, ParkY, (float)Tubes[Trains[i].Tube].AxisZ);
    }
}
