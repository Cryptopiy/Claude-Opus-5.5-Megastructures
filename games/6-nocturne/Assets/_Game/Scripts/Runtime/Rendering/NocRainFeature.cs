// NocRainFeature: the rain renderer feature of MASTER_PROMPT 14.7 H5 / D-005 (R3 1.2 "approach A" with additive layers, R2 1.5).
// Sits in PC_Renderer BEFORE NocGradeFeature (PipelineConfigurator), with material = Rain.mat (shader Megastructure/NocRain; the
// material reference also ships the shader). Game cameras only (the capture harness' SubmitRenderRequest renders included).
// The pass is always enqueued so the one-time [NocProbe] line (D-008 facts read from the live player: graphics API, reversed Z,
// MSAA depth bind and formats, HDR colour format, Forward+ keyword, asset flags) is written even with -rain 0; it records no
// Render Graph pass when rho is 0 ("-rain 0 removes both passes", R2 1.7).
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Megastructure
{
    [DisallowMultipleRendererFeature("Noc Rain")]
    public sealed class NocRainFeature : ScriptableRendererFeature
    {
        /// <summary>Rain.mat (shader Megastructure/NocRain).</summary>
        public Material material;

        NocRainPass m_Pass;

        public override void Create()
        {
            if (m_Pass == null) m_Pass = new NocRainPass();
            m_Pass.Bind(material);
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (material == null) return;
            if (renderingData.cameraData.cameraType != CameraType.Game) return;
            if (m_Pass == null) m_Pass = new NocRainPass();
            if (!m_Pass.Bind(material)) return;
            renderer.EnqueuePass(m_Pass);
        }

        protected override void Dispose(bool disposing) { m_Pass = null; }
    }
}
