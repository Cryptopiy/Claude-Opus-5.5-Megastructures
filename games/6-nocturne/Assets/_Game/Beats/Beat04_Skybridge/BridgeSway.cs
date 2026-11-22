// BridgeSway: D-034 4.3 (docs/DREAD.md "Bridge sway"; owner TR, Phase 4 wave B): in beat 4 the eye translates sideways by 2-3 cm
// at 0.12 Hz, more in the wind gusts, never rolls. Pure function of (s, scene time); the camera owner (FX, CameraRig.ApplyPose)
// adds it to the eye's z (the bridge lies on the east leg, heading +x, so z is the lateral axis):
//   dz(s, t) = w4(s) sin(pi u) (0.020 + 0.010 g(t)) sin(2 pi 0.12 t),
//   u = (x - 375.3) / 300 clamped to 0..1 (x = s - 184.70: the deck between the truss bearings sways most at mid-span),
//   w4 = NocRainMath.Gust(s) (the beat-4 weight, Rule P over 24 m around s 560 and 880),
//   g(t) = 0.5 - 0.5 cos(2 pi t / 17) (the rain-gust period of H5, NocturneConstants.RainGustPeriod).
// The extremes of sin(2 pi 0.12 t) are where AU's DreadVoices.BridgeGust puts its creaks (-cos(2 ph) peaks), on the same clock.
// Peak 3.0 cm, peak lateral speed 2 pi 0.12 x 0.03 = 0.023 m/s; a translation adds no rotation, so the 0.3 deg sway limit is not
// touched; the lane keeps its 2.15 m half-width minus 0.03 m (the camera sphere check stays clear).
using System;

namespace Megastructure
{
    public static class BridgeSway
    {
        public const double Hz = 0.12, Base = 0.020, GustAdd = 0.010;

        public static double SpanShape(double s)
        {
            double x = s - NocturneConstants.EastLegOffset;
            double u = (x - SkybridgeDims.SpanX0) / (SkybridgeDims.SpanX1 - SkybridgeDims.SpanX0);
            if (u <= 0.0 || u >= 1.0) return 0.0;
            return Math.Sin(Math.PI * u);
        }

        public static double GustEnvelope(double t) => 0.5 - 0.5 * Math.Cos(2.0 * Math.PI * t / NocturneConstants.RainGustPeriod);

        /// <summary>Lateral eye offset (m, +z north) at progress s and scene time t.</summary>
        public static double Offset(double s, double t)
        {
            double a = NocRainMath.Gust(s) * SpanShape(s);
            if (a <= 0.0) return 0.0;
            return a * (Base + GustAdd * GustEnvelope(t)) * Math.Sin(2.0 * Math.PI * Hz * t);
        }
    }
}
