// PlayerMath: pure, frame-rate independent player logic in double (MASTER_PROMPT Part 9, 14.11), from Thread's PlayerMath.
// No MonoBehaviour state: PlayerController, AutoWalker / WalkPlanner and the EditMode tests share these exact functions, so
// the stop ramp, the stride and the head bob are tested as shipped. Thread's EyeFollower is not imported (Nocturne has no
// ramps, stairs or lifts: the eye is always feet + 1.7 m, 14.4) and its helix head-bob terms are gone.
using System;

namespace Megastructure
{
    /// <summary>Player tuning that is not a Part 14 number (those live in NocturneConstants).</summary>
    public static class PlayerTuning
    {
        // Part 9: raw mouse counts to degrees ("about 0.06 to 0.10 deg/count").
        public const float MouseDegPerCount = 0.08f;
        // Gamepad look is rate based with a quadratic response.
        public const float StickDegPerSec = 120f;
        // Pitch clamp +-88 degrees (never roll).
        public const float PitchLimit = 88f;
        // Look input above these thresholds counts as "player input" (cancels the autowalk, hides the hint).
        public const float LookThresholdCounts = 3f;
        public const float StickThreshold = 0.3f;
        // Movement integration step cap (s): a load hitch never teleports the player.
        public const double MaxStepDt = 0.1;
        // Stop ramp: v0 floor so T = 2 d / v0 stays finite when the trigger is reached very slowly.
        public const double StopRampMinSpeed = 0.3;
        // Footstep stereo pan (alternate feet).
        public const float FootstepPan = 0.15f;

        // Physical safety (14.4): corridor colliders live on layer 30 ("Corridor"), the safety triggers on 31 ("Safety").
        // Walls sit at lane half-width + PlayerRadius + WallGap. 14.4 defines the lane as "collider gap minus 0.70 m" = the
        // region the player's centre can reach. A CharacterController keeps its skin width (PhysX contact offset) between
        // its capsule and a collider, so the reachable centre is face - radius - skin: with WallGap = ControllerSkin the
        // reachable half-width is exactly the lane of 14.4 (the WallBot lane-reach probe measures it). The collider gap is
        // therefore lane + 2 (0.35 + 0.03) = lane + 0.76 m (logged decision, audit S3 item 15).
        public const int CorridorLayer = 30;
        public const int SafetyLayer = 31;
        public const int CorridorMask = 1 << CorridorLayer;
        public const float ControllerSkin = 0.03f;
        public const float WallGap = ControllerSkin;
        public const float SlabTopBelowFloor = 0.0f;   // the floor box top is the walk plane y 0 (14.4: 1 m thick floor boxes)
    }

    public static class PlayerMath
    {
        // ------------------------------------------------------------------ corridor
        /// <summary>Lateral offset clamped to the walkable half-width of 14.4 (NocLanes).</summary>
        public static double ClampLateral(double s, double lateral)
        {
            double h = NocLanes.LaneHalfWidth(s);
            return lateral < -h ? -h : (lateral > h ? h : lateral);
        }

        public static double ClampS(double s) => s < 0 ? 0 : (s > NocLanes.Path.Length ? NocLanes.Path.Length : s);

        /// <summary>Feet within this horizontal distance (m) of the placement point keep the exact placement s (GE's GB-19 request,
        /// carried by FX): the float feet position projects a few 1e-5 m short of s 400 (float resolution 3e-5 m there), so a
        /// -beat N start re-entered beat N - 1 once before the walk began.</summary>
        public const double AnchorRadius = 1e-3;

        /// <summary>Progress s of the feet: the exact placement s while the feet stay within <see cref="AnchorRadius"/> of the
        /// placement point (dxz = horizontal distance from it), else the projection on the centre line.</summary>
        public static double AnchoredS(double projectedS, bool anchored, double anchorS, double dxz)
            => anchored && dxz < AnchorRadius ? anchorS : projectedS;

        // ------------------------------------------------------------------ steps (14.3, IMPORT_PLAN 3.6)
        /// <summary>Stride (m): lerp(0.75, 0.85, InverseLerp(1.5, 2.5, v)), a continuous function of the speed (Part 3.1 (e)).</summary>
        public static double Stride(double speed)
            => NocMath.Lerp(NocturneConstants.StrideWalk, NocturneConstants.StrideBrisk, NocMath.InverseLerp(NocturneConstants.WalkSpeed, NocturneConstants.BriskSpeed, speed));

        /// <summary>Footstep gain from the plan speed: 1.0 at 1.5 m/s, 1.2 at 2.5 m/s, softer when creeping (continuous).</summary>
        public static float FootstepGain(double speed)
        {
            double g = 0.7 + 0.2 * speed;
            return (float)(g < 0.75 ? 0.75 : (g > 1.2 ? 1.2 : g));
        }

        // ------------------------------------------------------------------ head bob (Part 9, 14.1: 0.008 m, never roll)
        /// <summary>Vertical bob offset (zero mean, lowest at the footfall), scaled by speed / walk speed (0 at rest).</summary>
        public static double HeadBob(double stepPhase, double speed)
        {
            double k = NocMath.Clamp01(speed / NocturneConstants.WalkSpeed);
            double a = Math.Min(NocturneConstants.HeadBob, NocturneConstants.HeadBobMax);
            return -0.5 * a * k * Math.Cos(2.0 * Math.PI * stepPhase);
        }

        // ------------------------------------------------------------------ critically damped helpers
        /// <summary>Exact critically damped step toward a constant target. omega = 2 / smoothTime (Mathf.SmoothDamp convention).</summary>
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

        public static double WrapDeg(double a) => NocMath.WrapDeg(a);
    }

    /// <summary>
    /// The end-of-path velocity ramp of 14.11: v(t) = v0 (1 - smoothstep(0, T, t)). The distance is v0 T / 2; with
    /// T = 2 (1500 - s0) / v0 the player rests exactly at s = 1500 (T = 6 / v0 when the trigger fires at 1497: 2.4 s at
    /// 2.5 m/s, 4.0 s at 1.5 m/s). The position is the closed-form integral, so the landing point does not depend on the frame rate.
    /// </summary>
    public struct StopRamp
    {
        public double s0, v0, T;

        public static StopRamp Begin(double s0, double v0)
        {
            var r = new StopRamp();
            double d = Math.Max(0.0, NocturneConstants.GoalS - s0);
            r.s0 = s0;
            r.v0 = Math.Max(v0, PlayerTuning.StopRampMinSpeed);
            r.T = d > 1e-9 ? 2.0 * d / r.v0 : 0.0;
            return r;
        }

        /// <summary>Progress at time t after the trigger: s0 + v0 T (u - u^3 + u^4 / 2), u = t / T.</summary>
        public double PositionAt(double t)
        {
            if (T <= 0 || t >= T) return Math.Max(s0, NocturneConstants.GoalS);
            double u = t <= 0 ? 0 : t / T;
            return s0 + v0 * T * (u - u * u * u + 0.5 * u * u * u * u);
        }

        public double SpeedAt(double t) => T <= 0 ? 0 : v0 * (1.0 - NocMath.Smoothstep(0, T, t));
        /// <summary>Remaining fraction of the initial speed (lateral motion decays with the same ramp).</summary>
        public double SpeedFactor(double t) => T <= 0 ? 0 : 1.0 - NocMath.Smoothstep(0, T, t);
        public bool Done(double t) => t >= T;
    }
}
