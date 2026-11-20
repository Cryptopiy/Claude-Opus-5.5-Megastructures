// TramLook: the light levels of the tram T_A (owner BV; LOOK_V2 6 "required subjects on the darker field": the KS06 tram shows
// a continuous WinNeutral window band at linear 0.25-0.32 (luma 135-155) and SignWhite headlights with a core >= 2.0). One
// source for MaterialFactory.Boulevard's Tram row, the runtime fallback material (TramBuilder) and the tests.
namespace Megastructure
{
    public static class TramLook
    {
        /// <summary>Saloon band mean (x WinNeutral, luminance 0.932): 0.30 x 0.932 = 0.28 linear.</summary>
        public const float WinGain = 0.30f;
        public const float HeadGain = 3.0f, TailGain = 1.2f, DisplayGain = 0.35f;
    }
}
