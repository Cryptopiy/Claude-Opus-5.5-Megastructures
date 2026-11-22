// KoiDriver: the per-frame phase of the koi (MASTER_PROMPT 14.7 H6, 14.5.7, D-009, D-026), an IWorldUpdater registered by
// KoiBuilder. Play: u = KoiMath.StepPhase (t_k from the first crossing of s 1 260, the critically damped excess E with omega 0.8,
// u never decreasing; at any constant speed E = 0 and the head follows the 14.5.7 table exactly), wave time t_w = t_k. Capture mode
// (stills and sequences, D-026): u = (s - 1 260) / 240 and t_w = (s - 1 260) / 1.5, a pure function of s. The two uniforms of NocKoi
// (_KoiParams: phi_h wrapped to [0, 2 pi), frac(t_w / 20), the Kohaku patch saturation 1 - 0.75 smoothstep(1 440, 1 490, s) of
// LOOK_V2 6 and the scanline phase; _KoiTime: the fin / veil flow and caustic loop phases) are computed in double; the 12 analytic koi light points of 14.2.3 go to
// AtmosphereDriver.KoiSpine (uploaded with the next frame's globals) with KoiK ramped over the 24 m around the beat-7 boundary
// (Rule P; "koi light on" at 6 -> 7). Before s 1 260 the koi already swims with u = (s - 1 260) / 240 (it is never switched on or
// off: 14.6). Fix wave C27 (DREAD 5.2): the eye spots' pupils follow the camera at <= 6 deg/s (KoiEye, _KoiPupil). Zero
// allocations per frame.
using UnityEngine;

namespace Megastructure
{
    public sealed class KoiDriver : IWorldUpdater
    {
        static readonly int s_params = Shader.PropertyToID("_KoiParams");
        static readonly int s_time = Shader.PropertyToID("_KoiTime");
        static readonly int s_pupil = Shader.PropertyToID("_KoiPupil");

        readonly Material m_mat;
        KoiPhaseState m_phase;
        KoiEyeState m_eye;
        readonly D3[] m_lights = new D3[NocturneConstants.KoiSpinePoints];

        /// <summary>Last phase, head azimuth (rad, unwrapped) and wave time (QA, tests).</summary>
        public double U { get; private set; }
        public double HeadAzimuth { get; private set; }
        public double WaveTime { get; private set; }
        /// <summary>The eyes' gaze state (QA, tests).</summary>
        public KoiEyeState Eye => m_eye;

        public KoiDriver(Material mat) { m_mat = mat; KoiMath.ResetPhase(ref m_phase); }

        /// <summary>Koi light strength over the 24 m around s 1 260 (smootherstep 1 248 .. 1 272).</summary>
        public static double LightRamp(double s) => NocMath.Smootherstep(NocturneConstants.BeatBoundaries[6] - 12.0, NocturneConstants.BeatBoundaries[6] + 12.0, s);

        public void OnFrame(in FrameState f)
        {
            double s = f.s, u, tw;
            if (NocState.Capture)
            {
                u = KoiMath.PhaseU(s);
                tw = KoiMath.WaveTimeCapture(s);
            }
            else
            {
                u = KoiMath.StepPhase(ref m_phase, s, NocState.Dt);
                tw = KoiMath.WaveTimePlay(m_phase.Tk);
            }
            double phi = KoiMath.HeadAzimuth(u);
            U = u; HeadAzimuth = phi; WaveTime = tw;
            if (m_mat != null)
            {
                m_mat.SetVector(s_params, KoiShaderMath.Params(phi, tw, s));
                m_mat.SetVector(s_time, KoiShaderMath.TimeParams(tw));
            }
            // C27: the pupils turn toward the walker (at most 6 deg/s; a capture still snaps)
            m_eye.Step(phi, tw, new D3(f.cameraPos.x, f.cameraPos.y, f.cameraPos.z), s, NocState.Dt, NocState.Capture);
            if (m_mat != null) m_mat.SetVector(s_pupil, KoiEye.Pupil(m_eye.outer, m_eye.inner));

            KoiMath.SpineLightPoints(phi, tw, m_lights);
            for (int k = 0; k < m_lights.Length && k < AtmosphereDriver.KoiSpine.Length; k++)
                AtmosphereDriver.KoiSpine[k] = new Vector4((float)m_lights[k].x, (float)m_lights[k].y, (float)m_lights[k].z, 1f);
            AtmosphereDriver.KoiK = (float)LightRamp(s);
        }
    }
}
