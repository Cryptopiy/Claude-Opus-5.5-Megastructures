// SignsDriver: the per-frame part of beat 6 (MASTER_PROMPT 14.5.6 Light, D-010): uploads the wrapped phases of the scene clock
// as the global _NocScreenT = (frac(t / 58), frac(t / 60), t mod 600, waves), computed in double, read by NocScreen (the face) and
// by the H4 wall plane of kind 2 (NocMirror's NocScreenMirror), and gives the 12 wall lights the wave colour at their position
// (NocScreenMath, the shader's formulas). The colour of a light changes at the wave rate (0.017 Hz), far below the 0.2 Hz
// photosensitivity limit. Clock-driven only (the scene clock; -freeze-clock freezes it), so the static tests are frame-identical.
// Zero allocations per frame.
using UnityEngine;

namespace Megastructure
{
    public sealed class SignsDriver : IWorldUpdater
    {
        /// <summary>The global of NocScreenWave.hlsl (a _Noc* global: no material property has this name, lesson 5).</summary>
        public static readonly int ScreenTimeId = Shader.PropertyToID("_NocScreenT");

        readonly Material m_screen;
        readonly Light[] m_lights;
        readonly float[] m_z, m_y;
        readonly int m_waves;

        public SignsDriver(Material screen, Light[] lights, int waves)
        {
            m_screen = screen;
            m_lights = lights ?? new Light[0];
            m_waves = waves;
            m_z = new float[m_lights.Length];
            m_y = new float[m_lights.Length];
            for (int i = 0; i < m_lights.Length; i++)
            {
                if (m_lights[i] == null) continue;
                Vector3 p = m_lights[i].transform.position;
                m_z[i] = p.z; m_y[i] = p.y;
            }
            Shader.SetGlobalVector(ScreenTimeId, new Vector4(0f, 0f, 0f, waves));
        }

        public void OnFrame(in FrameState f)
        {
            NocScreenMath.TimePhases(f.time, out double p58, out double p60, out double t600);
            Shader.SetGlobalVector(ScreenTimeId, new Vector4((float)p58, (float)p60, (float)t600, m_waves));
            for (int i = 0; i < m_lights.Length; i++)
            {
                if (m_lights[i] == null) continue;
                D3 c = NocScreenMath.Ramp(NocScreenMath.Index(m_z[i], m_y[i], p58, p60, m_waves));
                m_lights[i].color = ScreenWallBuilder.UnitColour(c).gamma;
            }
        }
    }
}
