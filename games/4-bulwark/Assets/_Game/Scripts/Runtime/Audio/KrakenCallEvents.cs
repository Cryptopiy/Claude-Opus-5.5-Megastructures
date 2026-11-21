// KrakenCallEvents: KrakenCall (the Core contract) -> KrakenEvent (the audio thread's event), shared by AudioDirector (the
// player) and OfflineAudioRenderer (the offline renders of the show's call table; fix wave X2 S3-3). UnityEngine.Vector3 only
// (no engine state), so the offline harness and EditMode tests use the same rule as the player.
using System;
using UnityEngine;

namespace Megastructure.Audio
{
    public static class KrakenCallEvents
    {
        /// <summary>True where a DamShudder is the strike on the glass (fix wave S3-4): the listener in beat 6, the window
        /// gallery (Beat06Encounter publishes the s 1300 strike as a DamShudder that is not under water).</summary>
        public static bool IsStrike(double listenerS) => listenerS >= BulwarkConst.BeatS[5] && listenerS < BulwarkConst.BeatS[6];

        /// <summary>
        /// delay = T0 - now (sound scheduled ahead is honoured), plus the air delay d / 343 s for slaps and water roar outdoors
        /// (ADDENDUM_A1 2.1 rule 2: T0 is the picture's time). A call that arrives late starts now with the remaining duration;
        /// false when less than 1 s of it is left. listenerS (optional): a DamShudder heard in the window gallery is the strike.
        /// </summary>
        public static bool ToEvent(in KrakenCall c, Vector3 listener, double now, out KrakenEvent e, double listenerS = -1.0)
        {
            int kind = Math.Max(0, Math.Min(KrakenRecipe.KindCount - 1, (int)c.Kind));
            if (kind == KrakenRecipe.DamShudder && !c.Underwater && IsStrike(listenerS)) kind = KrakenRecipe.Strike;
            double dx = c.Position.x - listener.x, dy = c.Position.y - listener.y, dz = c.Position.z - listener.z;
            double dist = Math.Sqrt(dx * dx + dy * dy + dz * dz);
            double delay = c.T0 - now;
            if (!c.Underwater && KrakenRecipe.Delayed(kind)) delay += dist / KrakenRecipe.SpeedOfSound;
            double dur = c.Duration > 0f ? c.Duration : KrakenRecipe.DefaultDuration[kind];
            if (delay < 0.0) { dur += delay; delay = 0.0; }
            e = new KrakenEvent
            {
                kind = kind, strength = Math.Max(0f, Math.Min(1f, c.Strength)), duration = (float)dur, underwater = c.Underwater,
                x = c.Position.x, y = c.Position.y, z = c.Position.z, delay = (float)Math.Min(delay, 20.0),
            };
            return dur >= 1.0;
        }
    }
}
