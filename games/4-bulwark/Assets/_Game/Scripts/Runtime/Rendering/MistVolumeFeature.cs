// MistVolumeFeature: the one full-screen mist, curtain, spray and rainbow pass of Bulwark (MASTER_PROMPT 14.7 H2, 14.2.6
// BwMist). URP 17.6 Render Graph renderer feature on PC_Renderer, registered before BwGradeFeature by
// Editor/PipelineConfigurator (RebuildAll); `shader` = Megastructure/BwMist (the serialized reference also keeps the shader
// in the player build). The material is created at runtime from it. The pass (MistVolumePass) runs after the opaques and
// the far-field dome, before the transparents and the grade, reads the multisampled camera depth and blends over the MSAA
// colour; its inputs are the _BwMist* globals written each frame by MistVolumeState.Tick (beat 3's updater).
// Cameras: Game cameras only (the capture harness' SubmitRenderRequest renders are Game cameras); never the Scene view,
// previews or reflection probes. `-mist off` disables it (look-dev, the mist-off renders of the edge test).
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Megastructure.Rendering
{
    [DisallowMultipleRendererFeature("Bw Mist Volume")]
    public sealed class MistVolumeFeature : ScriptableRendererFeature
    {
        public const string ShaderName = "Megastructure/BwMist";

        /// <summary>Megastructure/BwMist (set by PipelineConfigurator).</summary>
        public Shader shader;

        Material m_Material;
        MistVolumePass m_Pass;

        public override void Create()
        {
            if (m_Pass == null) m_Pass = new MistVolumePass();
            // generate the noise tile behind the load fade in play mode, not on the first visible frame
            if (Application.isPlaying) MistVolumeNoise.Get();
        }

        bool EnsureMaterial()
        {
            if (m_Material != null) return true;
            Shader s = shader != null ? shader : Shader.Find(ShaderName);
            if (s == null || !s.isSupported) return false;
            m_Material = CoreUtils.CreateEngineMaterial(s);
            m_Material.name = "BwMist (runtime)";
            return m_Material != null;
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (!MistVolumeState.Enabled) return;
            if (renderingData.cameraData.cameraType != CameraType.Game) return;
            if (!EnsureMaterial()) return;
            if (m_Pass == null) m_Pass = new MistVolumePass();
            if (!m_Pass.Setup(m_Material)) return;
            renderer.EnqueuePass(m_Pass);
        }

        protected override void Dispose(bool disposing)
        {
            CoreUtils.Destroy(m_Material);
            m_Material = null;
            m_Pass = null;
        }
    }
}
