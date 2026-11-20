// MaterialLibrary.Look: the light, rain, sky and post fields of the material library (owner PA, PA-2; TEXTURE_PLAN 8.3). Created
// by TP-0 with the fields moved VERBATIM from MaterialLibrary.cs (serialised names unchanged) and handed over to PA.
using UnityEngine;

namespace Megastructure
{
    public partial class MaterialLibrary
    {
        [Header("Lights (A2b: 14.7 H2, H5, H7)")]
        public Material Neon;             // NocNeon: instanced glyph signs (colour by hash: the LOOK_V2 R6 / R7 weights)
        public Material StreakTail;       // NocStreak lane vehicles, red tails toward amber x k_a (east-bound boulevard lanes)
        public Material StreakHead;       // NocStreak lane vehicles, white heads toward WinNeutral (west-bound; LOOK_V2 10)
        public Material Beacon;           // NocStreak points: AviationRed beacons (0.2 Hz); NocHalos derives the kind-3 halos
        public Material Rain;             // NocRain: the rain pass (NocRainFeature)

        [Header("Sky / post")]
        public Material Sky;              // NocSky placeholder dome (camera-following, 14.2.2 gradient)
        public Material Grade;            // NocGrade (bloom + grade + dither), used by NocGradeFeature
    }
}
