// EndingRide: the shared state of the new ending (Addendum A1 section 3, lead decision 2026-11-20): an express glass
// elevator down the lee face, then a horizontal maglev shuttle west to the lantern platform. One writer: the ending owner's
// ride controller. Readers: audio (ride hum, wind, deceleration), atmosphere (sun/sky by altitude), the kraken stinger,
// QA. Lead-owned contract: extend, never break.
using UnityEngine;

namespace Megastructure
{
    public enum EndingRidePhase
    {
        /// <summary>Not started (walking the path).</summary>
        None = 0,
        /// <summary>Doors closing at the top station on the ledge.</summary>
        Boarding = 1,
        /// <summary>The vertical descent down the lee face.</summary>
        Descent = 2,
        /// <summary>Transfer at the foot: the cabin couples to the shuttle track.</summary>
        Transfer = 3,
        /// <summary>The horizontal shuttle run west, away from the dam.</summary>
        Shuttle = 4,
        /// <summary>Arrived at the lantern platform; doors open; the walk to the lantern.</summary>
        Arrived = 5,
    }

    public static class EndingRide
    {
        public static EndingRidePhase Phase { get; private set; }
        /// <summary>0..1 progress within the current phase.</summary>
        public static float PhaseProgress { get; private set; }
        /// <summary>Eye altitude above the lowland (m) and horizontal distance from the lee face (m).</summary>
        public static float Altitude { get; private set; }
        public static float DistanceFromFace { get; private set; }
        /// <summary>Speed of the cabin (m/s, magnitude).</summary>
        public static float Speed { get; private set; }

        /// <summary>Writer only (the ride controller).</summary>
        public static void Set(EndingRidePhase phase, float phaseProgress, float altitude, float distanceFromFace, float speed)
        {
            Phase = phase; PhaseProgress = Mathf.Clamp01(phaseProgress);
            Altitude = altitude; DistanceFromFace = distanceFromFace; Speed = Mathf.Max(0f, speed);
        }

        public static void Reset() { Set(EndingRidePhase.None, 0f, 6000f, 0f, 0f); }
    }
}
