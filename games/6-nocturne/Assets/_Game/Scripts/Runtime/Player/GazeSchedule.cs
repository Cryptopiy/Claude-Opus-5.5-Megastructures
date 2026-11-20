using System;

namespace Megastructure
{
    /// <summary>
    /// Cinematic gaze schedule (14.5 "Gaze" lines), imported from The Ring's GazeSchedule with the cinematic edits of Part 3.1
    /// (IMPORT_PLAN 3.3, 4.4 b, f): keys are keyed to the progress s in metres (never to time), all yaws are relative to the
    /// path heading at s, smootherstep between consecutive keys. Holds, hold steps, glances, the sway overlay and the
    /// lateral offset table of The Ring are deleted (Nocturne has none: the walk never pauses, and 14.5 asks for no lateral
    /// offset, so the key-shot eyes are on the centre line). The AutoWalker's critically damped follower (omega 2.5 rad/s),
    /// the 22 / 18 deg/s rate limits and the handheld sway are applied on top (WalkPlanner). Pure, allocation-free.
    /// </summary>
    public static class GazeSchedule
    {
        /// <summary>Key-schedule gaze at progress s: world yaw (deg, heading + relative yaw) and pitch (deg).</summary>
        public static void Evaluate(double s, double headingDeg, out double yaw, out double pitch)
        {
            var k = NocturneConstants.GazeKeys;
            int last = k.Length - 1;
            if (s <= k[0].s) { yaw = headingDeg + k[0].yaw; pitch = k[0].pitch; return; }
            if (s >= k[last].s) { yaw = headingDeg + k[last].yaw; pitch = k[last].pitch; return; }
            int i = 0;
            while (i < last - 1 && s >= k[i + 1].s) i++;
            double u = NocMath.Smootherstep(k[i].s, k[i + 1].s, s);
            yaw = headingDeg + NocMath.Lerp(k[i].yaw, k[i + 1].yaw, u);
            pitch = NocMath.Lerp(k[i].pitch, k[i + 1].pitch, u);
        }

        /// <summary>Key-schedule gaze at s with the heading of the 14.4 path at s (QA cameras, key-shot poses).</summary>
        public static void Evaluate(double s, out double yaw, out double pitch)
            => Evaluate(s, NocLanes.Path.HeadingDeg(NocMath.Clamp(s, 0.0, NocLanes.Path.Length)), out yaw, out pitch);

        /// <summary>Lateral camera offset in the lane (Part 9 allows it when Part 14 asks; 14.5 asks for none): always 0.</summary>
        public static double LateralOffset(double s) => 0.0;
    }
}
