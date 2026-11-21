// PlayerTuning / PlayerMath / StopRamp / FootstepCadence: pure, frame-rate independent player logic in double.
// Extracted from Thread's PlayerMath.cs (PlayerTuning, CritDamp, WrapDeg, the StopRamp and FootstepCadence patterns).
// NOT imported: Thread's EyeFollower (a 0.12 s TIME follower; Part 9 / 14.12 smooth the eye in DISTANCE: BwPath.EyeY is
// the 2.5 m triangular-kernel average of the floor), the helix half-width and the salt/stone/metal surfaces.
// Cadence is speed-dependent (14.3): stride 0.75 m at 1.5 m/s, 0.85 m at 2.5 m/s, 0.90 m on stairs.
using System;

namespace Megastructure.Player
{
    /// <summary>Player tuning that is not a Part 14 number. Each value cites its source.</summary>
    public static class PlayerTuning
    {
        // Part 9: raw mouse counts to degrees (no acceleration, no smoothing lag).
        public const float MouseDegPerCount = 0.08f;
        // Gamepad look is rate based with a quadratic response.
        public const float StickDegPerSec = 120f;
        // Pitch clamp +-88 degrees (never roll).
        public const float PitchLimit = 88f;
        // Look input above these thresholds counts as "player input" (cancels the autowalk, hides the hint).
        public const float LookThresholdCounts = 3f;
        public const float StickThreshold = 0.3f;
        // The Ring's cancel rule: mouse over 2 px per frame, stick over 0.3, move over 0.2, Shift press.
        public const float CancelMousePx = 2f;
        public const float CancelMove = 0.2f;
        // Cancel grace after the cinematic starts (and after unpause), Thread AutoWalker.
        public const float CancelGrace = 0.3f;

        // Movement integration step cap (s): a load hitch never teleports the player.
        public const double MaxStepDt = 0.1;
        // Stop ramp (14.11): v0 floor so T = 2 d / v0 stays finite when the trigger is reached very slowly.
        public const double StopRampMinSpeed = 0.3;
        // First footstep after 1 - FirstStepPhase of a stride (no footstep before the player first moves).
        public const double FirstStepPhase = 0.55;
        // Footstep stereo pan: alternate +-0.15.
        public const float FootstepPan = 0.15f;
        // Analytic vs physical: a CharacterController rejection larger than this (xz, m) is counted (D-01).
        public const double RejectionTolerance = 0.01;

        // Physical safety (14.4, D-03, C-18): corridor colliders on layer 30, safety volumes on layer 31.
        public const int CorridorLayer = 30;
        public const int SafetyLayer = 31;
        public const int CorridorMask = 1 << CorridorLayer;
        public const int SafetyMask = 1 << SafetyLayer;
        // Collider generation (14.4 / D-03): boxes 1 m thick, 6 m high, every 4 m with 1 m overlap.
        public const float WallThickness = 1f;
        public const float WallHeight = 6f;
        public const float SegmentLength = 4f;
        public const float SegmentOverlap = 1f;
        // Fallback floors 3 cm under the visible deck (The Ring).
        public const float FallbackFloorDrop = 0.03f;
        // Ray grid spacing of the coverage check (along s and across the lane).
        public const float RayGridStep = 0.5f;
    }

    public static class PlayerMath
    {
        /// <summary>Capsule-centre lateral clamped to BwPath.Lane(s) (14.4: the single lane function).</summary>
        public static double ClampLateral(double s, double lateral)
        {
            BwPath.Lane(s, out double lo, out double hi);
            return lateral < lo ? lo : (lateral > hi ? hi : lateral);
        }

        public static double ClampS(double s) => s < 0 ? 0 : (s > BwPath.Length ? BwPath.Length : s);

        /// <summary>s clamp over the walked path AND the ending timeline (EndingPath, s 0..EndS).</summary>
        public static double ClampEndingS(double s) => s < 0 ? 0 : (s > EndingPath.EndS ? EndingPath.EndS : s);

        /// <summary>Mean floor slope |dy/ds| over +-0.6 m (stairs read about 0.67; the gallery ramp 0.12).</summary>
        public static double FloorSlope(double s)
        {
            double a = Math.Max(0.0, s - 0.6), b = Math.Min(BwPath.Length, s + 0.6);
            return b > a ? Math.Abs(BwPath.FloorY(b) - BwPath.FloorY(a)) / (b - a) : 0.0;
        }

        public static bool OnStairs(double s) => FloorSlope(s) > 0.3;

        /// <summary>
        /// Footstep surface at s (14.3), FIRST PASS by beat for the audio owner: gallery concrete, crest road, spillway bridge
        /// grating, armour-slope road, tower stairs steel (concrete off the stairs), window gallery concrete, sluice tunnel wet
        /// concrete, lee ledge rough ledge from the tunnel mouth (s 1422.85).
        /// </summary>
        public static FootSurface Surface(double s)
        {
            if (s > BwPath.Length) return FootSurface.Concrete;   // the lantern platform paving (the ride makes no steps)
            switch (BwPath.BeatAt(s))
            {
                case 1: return FootSurface.Concrete;
                case 2: return FootSurface.Road;
                case 3: return SlotSpan(s) ? FootSurface.Grating : FootSurface.Road;   // piers are road (audio request)
                case 4: return FootSurface.Road;
                case 5: return OnStairs(s) ? FootSurface.SteelStair : FootSurface.Concrete;
                case 6: return FootSurface.Concrete;
                default: return s < 1422.85 ? FootSurface.WetConcrete : FootSurface.Ledge;
            }
        }

        /// <summary>True over a spillway slot span (14.4: slots k 0-13 at z 424 + 20 k .. 440 + 20 k; z = s on L2).</summary>
        public static bool SlotSpan(double s)
        {
            double u = s - BulwarkConst.SlotZ0;
            if (u < 0.0 || u >= 14.0 * BulwarkConst.PierPitch) return false;
            return u % BulwarkConst.PierPitch < BulwarkConst.SlotLength;
        }

        /// <summary>Stride per footstep (14.3): 0.75 m at 1.5 m/s, 0.85 m at 2.5 m/s (linear between, clamped), 0.90 m on stairs.</summary>
        public static double Stride(double speed, bool stairs)
        {
            if (stairs) return FrameworkConst.StrideStairs;
            double t = (speed - FrameworkConst.WalkSpeed) / (FrameworkConst.BriskSpeed - FrameworkConst.WalkSpeed);
            t = t < 0 ? 0 : (t > 1 ? 1 : t);
            return FrameworkConst.StrideWalk + (FrameworkConst.StrideBrisk - FrameworkConst.StrideWalk) * t;
        }

        /// <summary>Footstep gain from the plan speed: 1.0 at 1.5 m/s, 1.2 at 2.5 m/s, softer when creeping.</summary>
        public static float FootstepGain(double speed)
        {
            double g = 0.7 + 0.2 * speed;
            return (float)(g < 0.75 ? 0.75 : (g > 1.2 ? 1.2 : g));
        }

        /// <summary>
        /// Exact critically damped step toward a constant target (no overshoot from rest). omega = 2 / smoothTime, the
        /// same convention as Mathf.SmoothDamp. Frame-rate independent.
        /// </summary>
        public static void CritDamp(ref double x, ref double v, double target, double smoothTime, double dt)
        {
            if (dt <= 0) return;
            double w = 2.0 / Math.Max(1e-4, smoothTime);
            double e = x - target;
            double b = v + w * e;
            double ex = Math.Exp(-w * dt);
            x = target + (e + b * dt) * ex;
            v = (v - w * b * dt) * ex;
        }

        public static double WrapDeg(double a) => BwMath.WrapDeg(a);
    }

    /// <summary>
    /// The end-of-path velocity ramp of 14.11: v(t) = v0 (1 - smoothstep(0, T, t)). The distance is v0 T / 2; with
    /// T = 2 (1500 - s0) / v0 the walker rests exactly at s 1500 (T = 1.2 s at 2.5 m/s from the trigger at 1498.5).
    /// The position is the closed-form integral, so the landing point does not depend on the frame rate.
    /// </summary>
    public struct StopRamp
    {
        public double s0, v0, T, rest;

        public static StopRamp Begin(double s0, double v0) => Begin(s0, v0, FrameworkConst.GoalRestS);

        /// <summary>The ramp to an explicit rest s (the cabin at 1500, the Keeper's Lantern at EndingPath.SRest).</summary>
        public static StopRamp Begin(double s0, double v0, double restS)
        {
            var r = new StopRamp();
            double d = Math.Max(0.0, restS - s0);
            r.rest = restS;
            r.s0 = s0;
            r.v0 = Math.Max(v0, PlayerTuning.StopRampMinSpeed);
            r.T = d > 1e-9 ? 2.0 * d / r.v0 : 0.0;
            return r;
        }

        /// <summary>Plan distance at time t after the trigger: s0 + v0 T (u - u^3 + u^4 / 2), u = t / T.</summary>
        public double PositionAt(double t)
        {
            if (T <= 0 || t >= T) return Math.Max(s0, rest);
            double u = t <= 0 ? 0 : t / T;
            return s0 + v0 * T * (u - u * u * u + 0.5 * u * u * u * u);
        }

        public double SpeedAt(double t) => T <= 0 ? 0 : v0 * (1.0 - BwMath.Smoothstep(0, T, t));
        /// <summary>Remaining fraction of the initial speed (lateral motion decays with the same ramp).</summary>
        public double SpeedFactor(double t) => T <= 0 ? 0 : 1.0 - BwMath.Smoothstep(0, T, t);
        public bool Done(double t) => t >= T;
    }

    /// <summary>Footstep cadence: one footfall per stride of plan distance travelled (stride by speed), alternating feet.</summary>
    public struct FootstepCadence
    {
        public double phase;     // 0..1 within the current step (0 = footfall)
        public bool left;        // foot of the last footfall
        public int count;

        public static FootstepCadence Create()
        {
            var c = new FootstepCadence();
            c.phase = PlayerTuning.FirstStepPhase;
            return c;
        }

        /// <summary>Advance by a travelled plan distance (m) at progress s and speed; true when a footfall happens.</summary>
        public bool Advance(double distance, double s, double speed)
        {
            if (distance <= 0) return false;
            phase += distance / PlayerMath.Stride(speed, PlayerMath.OnStairs(s));
            if (phase < 1.0) return false;
            phase -= Math.Floor(phase);   // a huge step (hitch) emits one footfall, never a burst
            left = !left;
            count++;
            return true;
        }

        public float Pan => left ? -PlayerTuning.FootstepPan : PlayerTuning.FootstepPan;
    }
}
