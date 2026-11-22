// DreadCamera: the D-034 camera envelopes that CameraRig applies (FX, fix wave p4_01; docs/DREAD.md 4.2). Pure functions of the
// progress s (D-038 (7): DREAD's seconds are metres at the cinematic 2.5 m/s), allocation-free.
//   Vertigo(s): 0 outside s VertigoS0..VertigoS2; smootherstep 0 -> 1 over VertigoS0..VertigoS1 (4 s), 1 -> 0 over
//   VertigoS1..VertigoS2 (3 s). CameraRig adds VertigoFov x w to the vFOV (68 -> 74 at the plunge) and, in the cinematic walk,
//   leans the eye VertigoLean x w m along the gaze. The 14.2.4 vFOV schedule (FovKnot*) is untouched.
namespace Megastructure
{
    public static class DreadCamera
    {
        /// <summary>D-034 4.2 vertigo at the plunge (KS10): vFOV + VertigoFov (68 -> 74) over s VertigoS0 -> VertigoS1 (4 s at 2.5 m/s,
        /// smootherstep), back by VertigoS2 (3 s), inside the s 538-548 hold of the plunge pose; the eye leans VertigoLean m along the
        /// gaze in the cinematic walk (CameraRig). The KS10 still keeps its row's 68 deg (pose override). An exception to the 62-68
        /// schedule recorded in D-034: peak vFOV rate 1.875 x 6 / 4 x ... = 2.8 deg/s up, 3.75 deg/s down (FovKnot* stay the 14.2.4
        /// schedule). Kept out of NocturneConstants on purpose: no shader reads them (no generated-HLSL churn).</summary>
        public const double VertigoS0 = 537.0, VertigoS1 = 547.0, VertigoS2 = 554.5, VertigoFov = 6.0, VertigoLean = 0.35;

        /// <summary>Vertigo weight 0..1 at progress s (KS10, D-034 4.2).</summary>
        public static double Vertigo(double s)
        {
            if (s <= VertigoS0 || s >= VertigoS2) return 0.0;
            if (s <= VertigoS1) return NocMath.Smootherstep(VertigoS0, VertigoS1, s);
            return 1.0 - NocMath.Smootherstep(VertigoS1, VertigoS2, s);
        }

        /// <summary>Largest |dw/ds| of the vertigo envelope (smootherstep peak 1.875 / span), for the rate checks.</summary>
        public static double VertigoMaxSlope()
            => 1.875 / System.Math.Min(VertigoS1 - VertigoS0, VertigoS2 - VertigoS1);
    }
}
