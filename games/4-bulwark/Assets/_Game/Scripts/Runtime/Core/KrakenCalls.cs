// KrakenCalls: the shared bus of the kraken's audible events (Addendum A1 section 2). One writer: the kraken choreography
// (KrakenDirector) publishes each event when it starts (also in capture/offline-audio mode, keyed to s and the ambient clock).
// Reader: the audio owner (Runtime/Audio) synthesises the sound (calls, surface roar, glass creak, sucker pops, the
// dam shudder) at the given world position and ducks the beds. Lead-owned contract: extend, never break.
using UnityEngine;

namespace Megastructure
{
    public enum KrakenCallKind
    {
        /// <summary>Deep distant moan/groan (sound before sight), long reverb.</summary>
        Call = 0,
        /// <summary>A body part breaking the surface: water roar, cascading water.</summary>
        SurfaceRoar = 1,
        /// <summary>A tentacle slapping the sea or the dam: a heavy boom and spray.</summary>
        Slap = 2,
        /// <summary>The dam structure shuddering after a contact: low rumble, creaks, dust.</summary>
        DamShudder = 3,
        /// <summary>Underwater, behind the glass: muffled close growl, clicks, the glass creaking.</summary>
        GlassCreak = 4,
        /// <summary>Suckers attaching to / releasing from the glass.</summary>
        SuckerPops = 5,
        /// <summary>The close eye moment: silence, then a vibrating infrasonic swell.</summary>
        EyeSwell = 6,
        /// <summary>The final stinger (far, faint call over the dam seen from the foot).</summary>
        Stinger = 7,
    }

    public struct KrakenCall
    {
        public KrakenCallKind Kind;
        /// <summary>World position of the source (m).</summary>
        public Vector3 Position;
        /// <summary>0..1.</summary>
        public float Strength;
        /// <summary>Start on the ambient clock (s) and duration (s). T0 is the PICTURE time of the event: the audio starts the
        /// sound at T0 (publish up to 20 s ahead for sound-before-sight calls) and adds the air delay d / 343 s itself for
        /// Slap and SurfaceRoar outdoors (do not add it here). A call published after T0 plays its remaining part (dropped
        /// under 1 s). Strength 1 = the recipe level at its reference distance, never under the ducked bed + 6 dB.</summary>
        public double T0;
        public float Duration;
        /// <summary>True when the listener hears it through water and glass (beat 6).</summary>
        public bool Underwater;
    }

    public static class KrakenCalls
    {
        public const int Max = 16;
        static readonly KrakenCall[] s_items = new KrakenCall[Max];
        static int s_count;
        static int s_serial;

        public static int Count => s_count;
        public static KrakenCall Get(int i) => s_items[i];
        /// <summary>Increments on every Publish (readers detect new events cheaply).</summary>
        public static int Serial => s_serial;

        /// <summary>Writer: publishes one event (drops the oldest when full).</summary>
        public static void Publish(in KrakenCall c)
        {
            if (s_count == Max) { for (int i = 1; i < Max; i++) s_items[i - 1] = s_items[i]; s_count = Max - 1; }
            s_items[s_count++] = c;
            s_serial++;
        }

        /// <summary>Writer: removes events that ended before ambient time t.</summary>
        public static void Expire(double t)
        {
            int w = 0;
            for (int i = 0; i < s_count; i++) if (t <= s_items[i].T0 + s_items[i].Duration + 1.0) s_items[w++] = s_items[i];
            s_count = w;
        }

        public static void Clear() { s_count = 0; }
    }
}
