// MaterialFactory.Look: the light, sky and post rows of the material contract (owner PA, PA-2; TEXTURE_PLAN 8.3): NocNeon, the
// NocStreak tails / heads / beacons, the NocRain pass, NocSky and NocGrade. Created by TP-0 with the rows moved VERBATIM from
// MaterialFactory.cs (no picture changes) and handed over to PA.
using System.Collections.Generic;
using UnityEngine.Rendering;

namespace Megastructure.EditorTools
{
    public static partial class MaterialFactory
    {
        /// <summary>Light, rain, sky and post rows (PA).</summary>
        static List<MatSpec> LookRows()
        {
            return new List<MatSpec>
            {
                Make("Neon", ShaderNeon).F("_Gain", NocturneConstants.NeonGain).F("_Palette", -1).F("_Panel", 0.03).Inst(),
                Make("StreakTail", ShaderStreak).F("_Kind", 0).F("_Gain", NocturneConstants.TrafficTailGain).F("_Ramp", NocturneConstants.TrafficRampLane)
                    .F("_Size", NocturneConstants.StreakWidth).Inst(),
                Make("StreakHead", ShaderStreak).F("_Kind", 1).F("_Gain", NocturneConstants.TrafficHeadGain).F("_Ramp", NocturneConstants.TrafficRampLane)
                    .F("_Size", NocturneConstants.StreakWidth).Inst(),
                Make("Beacon", ShaderStreak).F("_Kind", 2).F("_Gain", NocturneConstants.CityBeaconGain).F("_Ramp", 0).F("_Size", NocturneConstants.CityBeaconSize).Inst(),
                Make("Rain", ShaderRain),
                // ---- sky and post
                Make("Sky", NocPaths.ShaderSky).F("_SkyRadius", NocturneConstants.SkyDomeRadius),
                Make("Grade", NocPaths.ShaderGrade),
            };
        }
    }
}
