// MistQaSet: the mist owner's capture set "mistqa" (QaRigs; Wave X), the 14.5.3 / 14.10.3 frame tests of beat 3 and the
// kraken sprays that the built-in sets do not cover. Registered by Beat03SpillwayBridge.Build only when a capture run names
// the set (-capture <dir> -capture-set mistqa); it adds sequences only and changes nothing in the world.
//   seq_mist_ks09_static  14.5.3 (4): KS09 (-4, 1.7, 572) yaw -90 pitch -58, 120 frames at 30 fps, clock running: no
//                         firefly, water block-mean std <= 3 (Tools/qa/sequences.py flicker test);
//   seq_mist_ks09_pan58   T3 / 14.5.3 (4): the same pose, 0.5 px/frame yaw pan, 30 frames, clock frozen (grating moire);
//   seq_mist_ks09_pan89   T3: looking straight down through slot 7 (pitch -89), the same pan;
//   seq_mist_ks08_static  14.5.3 (5): plume flicker at KS08, 120 frames, clock running;
//   seq_mist_k5_cycle     the K5 whip and slap (MistVolumeQa "kcycle": 12 s loop from -mist-qa-t0) seen from KS11
//                         (0.9, 1.7, 824) yaw 25 pitch +6 vFOV 64, 360 frames: spray rise / fall steps (<= 5 % density per
//                         frame), the column height, no flashing. Run it with -mist-qa-sea kcycle -mist-qa-t0 329.6.
using System.Collections.Generic;
using Megastructure.QA;

namespace Megastructure.Beats
{
    public static class MistQaSet
    {
        public const string Set = "mistqa";
        /// <summary>Ambient clock of the KS11 sequence (s 824 / 2.5): pass it as -mist-qa-t0 so the cycle starts at frame 0.</summary>
        public const double K5CycleT0 = 824.0 / 2.5;

        public static List<SequenceSpec> Sequences(int w, int h)
        {
            var list = new List<SequenceSpec>(5);
            list.Add(Static("seq_mist_ks09_static", "14.5.3 (4): KS09 water and grating, clock running", 572.0, -4.0, 1.7, 572.0, -90.0, -58.0, 66.0, ShotPlan.StaticFrames, w, h));
            list.Add(Pan("seq_mist_ks09_pan58", "T3: grating at pitch -58, 0.5 px/frame yaw pan", 572.0, -4.0, 1.7, 572.0, -90.0, -58.0, 66.0, w, h));
            list.Add(Pan("seq_mist_ks09_pan89", "T3: grating at pitch -89, 0.5 px/frame yaw pan", 572.0, -4.0, 1.7, 572.0, -90.0, -89.0, 66.0, w, h));
            list.Add(Static("seq_mist_ks08_static", "14.5.3 (5): plume flicker at KS08, clock running", 604.0, -4.0, 1.7, 604.0, -75.0, -2.0, 66.0, ShotPlan.StaticFrames, w, h));
            var k5 = Static("seq_mist_k5_cycle", "K5 whip and slap from KS11 (MistVolumeQa kcycle)", 824.0, 0.9, 1.7, 824.0, 25.0, 6.0, 64.0, 360, w, h);
            k5.ambient0 = K5CycleT0;
            list.Add(k5);
            return list;
        }

        static SequenceSpec Static(string name, string purpose, double s, double x, double y, double z, double yaw, double pitch, double fov, int frames, int w, int h)
            => new SequenceSpec
            {
                name = name, set = Set, purpose = purpose, mode = SeqMode.Static, fps = ShotPlan.SeqFps, width = w, height = h, frames = frames,
                s0 = s, s1 = s, x = x, y = y, z = z, yaw = yaw, pitch = pitch, fov = fov,
                ambient0 = ShotPlan.AmbientForS(s), advanceAmbient = true,
            };

        static SequenceSpec Pan(string name, string purpose, double s, double x, double y, double z, double yaw, double pitch, double fov, int w, int h)
            => new SequenceSpec
            {
                name = name, set = Set, purpose = purpose, mode = SeqMode.PanYaw, fps = ShotPlan.SeqFps, width = w, height = h, frames = ShotPlan.PanFrames,
                s0 = s, s1 = s, x = x, y = y, z = z, yaw = yaw, pitch = pitch, fov = fov,
                panStepDeg = QaProjection.PanStepDeg(ShotPlan.PanPx, fov, h), ambient0 = ShotPlan.AmbientForS(s), advanceAmbient = false,
            };
    }
}
