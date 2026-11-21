// WaveImpacts: the shared, allocation-free bus of wave impacts against the sea face (14.3 event coupling, 14.5.2, 14.7 H2).
// One writer: SeaState (the sea owner) publishes every impact it starts (random ones and SeaState.ForceImpact for captures).
// Readers: the MistVolumeFeature (wall sprays, ellipsoids 8 x 12 x 8 m, 0.6 s in, 2.4 s out), lamp sway / dust in the beats,
// and audio (the sea owner also calls BwServices.Audio.Impact). Times are on the ambient clock (BwClock), so frozen-clock
// captures are reproducible. Lead-owned contract: extend, never break.
using UnityEngine;

namespace Megastructure
{
    /// <summary>One wave impact at the sea face (x = +6) of the crest.</summary>
    public struct WaveImpact
    {
        /// <summary>World z of the impact centre (m) on the sea face.</summary>
        public float Z;
        /// <summary>Lateral world x of the spray centre (m); 6 = at the wall, larger = offshore.</summary>
        public float X;
        /// <summary>0..1 (1.0 = the 14 m plume of KS11).</summary>
        public float Strength;
        /// <summary>Start time on the ambient clock (s).</summary>
        public double T0;
        /// <summary>Peak plume height above the crest (m).</summary>
        public float Height;
    }

    public static class WaveImpacts
    {
        /// <summary>At most 6 wall sprays at once (14.7 H2).</summary>
        public const int Max = 6;
        /// <summary>Lifetime of one impact's spray (0.6 s in + 2.4 s out, 14.7 H2).</summary>
        public const float Lifetime = 3.0f;

        static readonly WaveImpact[] s_active = new WaveImpact[Max];
        static int s_count;

        /// <summary>Number of impacts readable through <see cref="Get"/> (oldest first).</summary>
        public static int Count => s_count;

        /// <summary>The current impacts; read-only for consumers (index &lt; Count).</summary>
        public static WaveImpact Get(int i) => s_active[i];

        /// <summary>Called by SeaState only. Drops the oldest impact when full.</summary>
        public static void Publish(in WaveImpact w)
        {
            if (s_count == Max)
            {
                for (int i = 1; i < Max; i++) s_active[i - 1] = s_active[i];
                s_count = Max - 1;
            }
            s_active[s_count++] = w;
        }

        /// <summary>Called by SeaState once per frame: removes impacts older than Lifetime at ambient time t.</summary>
        public static void Expire(double t)
        {
            int w = 0;
            for (int i = 0; i < s_count; i++)
                if (t - s_active[i].T0 <= Lifetime) s_active[w++] = s_active[i];   // future (scheduled) impacts are kept too
            s_count = w;
        }

        /// <summary>Clears the bus (load, teleport, tests).</summary>
        public static void Clear() { s_count = 0; }
    }
}
