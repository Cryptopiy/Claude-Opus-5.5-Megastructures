// SpireDriver: the breathing of the spire's ring lights (MASTER_PROMPT 14.5.7 "ring lights every 40 m"; HT, CRIT0 #21), an
// IWorldUpdater registered by SpireBuilder. Ring k (y 40 (k + 1)) has its own NocLine material; its emission is the base colour
// x Breath(k, t): between RingLightLow and 1 with a raised cosine at RingBreathHz (0.1 Hz, under the 0.2 Hz photosensitivity limit
// of 14.10; no flash: the steepest change is 0.45 pi 0.1 = 14 % of the peak per second), ring k lagging k RingLag rad, so a slow
// pulse rises up the spire while the eleven rings together stay within +-4.1 % of their mean (|sum e^(-0.5 i k)| = 1.54; 0.225 x 1.54 / 8.53). t is the scene clock (stepped by the
// captures, so stills and sequences are deterministic). Zero allocations per frame.
using UnityEngine;

namespace Megastructure
{
    public sealed class SpireDriver : IWorldUpdater
    {
        static readonly int s_emission = Shader.PropertyToID("_EmissionColor");

        readonly Material[] m_rings;
        readonly Color m_base;

        public SpireDriver(Material[] rings, Color baseEmission) { m_rings = rings; m_base = baseEmission; }

        /// <summary>Brightness factor of ring k at scene time t (s): RingLightLow .. 1.</summary>
        public static double Breath(int k, double t)
        {
            double c = System.Math.Cos(2.0 * System.Math.PI * HeartLayout.RingBreathHz * t - k * HeartLayout.RingLag);
            return HeartLayout.RingLightLow + (1.0 - HeartLayout.RingLightLow) * (0.5 + 0.5 * c);
        }

        public void OnFrame(in FrameState f)
        {
            for (int k = 0; k < m_rings.Length; k++)
            {
                Material m = m_rings[k];
                if (m == null) continue;
                float w = (float)Breath(k, f.time);
                m.SetVector(s_emission, new Vector4(m_base.r * w, m_base.g * w, m_base.b * w, 1f));
            }
        }
    }
}
