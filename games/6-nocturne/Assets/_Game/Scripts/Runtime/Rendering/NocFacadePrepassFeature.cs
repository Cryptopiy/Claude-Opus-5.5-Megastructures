// NocFacadePrepassFeature: the NocFacade depth prepass of TEXTURE_PLAN 2.12 / 8.3 TP-0. The overdraw spike (debug view 9,
// QA/reports/debugview_TP_r01dv9.md) measured 1.79 / 1.39 / 3.30 / 3.83 shaded facade fragments per covered pixel at KS04 / KS05 /
// KS09 / KS17, above the plan's 1.5: once FA's textured facade path samples its arrays per fragment, every hidden facade fragment
// costs the full shading. This feature draws the facade renderers' geometry into the camera depth before the opaques
// (BeforeRenderingOpaques, Megastructure/NocFacadePrepass: depth only, the clip position computed as NocVert does it, so the depth
// is bit-identical), so NocFacade's forward pass (ZTest LEqual today; FA may set ZWrite Off / ZTest Equal) shades about one
// fragment per pixel. Nothing else changes: no colour is written (checked pixel by pixel against a run without it, docs/logs/TP.md).
// Switch: -facade-prepass 0|1 (NocLookGlobals.FacadePrepass). The facade renderers are tagged once the world is built with
// rendering-layer bit 29 (MeshRenderer.renderingLayerMask; URP's opaque pass draws every rendering layer) and filtered by it.
// Game cameras only; allocation-free per frame after the tagging.
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

namespace Megastructure
{
    [DisallowMultipleRendererFeature("Noc Facade Prepass")]
    public sealed class NocFacadePrepassFeature : ScriptableRendererFeature
    {
        /// <summary>Megastructure/NocFacadePrepass (set by PipelineConfigurator; the reference also ships the shader).</summary>
        public Shader shader;

        /// <summary>Rendering-layer bit of the NocFacade renderers (the overdraw view uses bit 30).</summary>
        public const int LayerBit = 29;
        public const uint LayerMask = 1u << LayerBit;
        public const string FacadeShader = "Megastructure/NocFacade";

        Material m_Material;
        PrepassPass m_Pass;
        static int s_Tagged = -1;

        public override void Create()
        {
            if (m_Material == null && shader != null && shader.isSupported) m_Material = CoreUtils.CreateEngineMaterial(shader);
            m_Pass = new PrepassPass();
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (!NocLookGlobals.FacadePrepass || m_Material == null) return;
            if (renderingData.cameraData.cameraType != CameraType.Game) return;
            if (s_Tagged <= 0)
            {
                if (!NocRig.WorldReady) return;
                s_Tagged = TagFacadeRenderers();
            }
            m_Pass.Setup(m_Material);
            renderer.EnqueuePass(m_Pass);
        }

        protected override void Dispose(bool disposing)
        {
            CoreUtils.Destroy(m_Material);
            m_Material = null;
        }

        /// <summary>Sets LayerBit on every MeshRenderer whose material uses NocFacade; returns the count.</summary>
        public static int TagFacadeRenderers()
        {
            int n = 0;
            foreach (var r in Object.FindObjectsByType<MeshRenderer>(FindObjectsInactive.Exclude))
            {
                bool hit = false;
                foreach (var m in r.sharedMaterials) if (m != null && m.shader != null && m.shader.name == FacadeShader) { hit = true; break; }
                if (!hit) continue;
                r.renderingLayerMask |= LayerMask;
                n++;
            }
            Debug.Log(System.FormattableString.Invariant($"[NOC] NocFacadePrepass: {n} facade renderers in the depth prepass"));
            return n;
        }

        sealed class PrepassPass : ScriptableRenderPass
        {
            Material m_Mat;
            static readonly List<ShaderTagId> s_Tags = new List<ShaderTagId> { new ShaderTagId("UniversalForward"), new ShaderTagId("UniversalForwardOnly"), new ShaderTagId("SRPDefaultUnlit") };
            static readonly ProfilingSampler s_Sampler = new ProfilingSampler("NocFacadePrepass");
            sealed class Data { public RendererListHandle list; }

            public PrepassPass() { profilingSampler = s_Sampler; renderPassEvent = RenderPassEvent.BeforeRenderingOpaques; }

            public void Setup(Material mat) { m_Mat = mat; }

            public override void RecordRenderGraph(RenderGraph rg, ContextContainer frameData)
            {
                var res = frameData.Get<UniversalResourceData>();
                if (!res.activeDepthTexture.IsValid()) return;
                var renderingData = frameData.Get<UniversalRenderingData>();
                var cameraData = frameData.Get<UniversalCameraData>();
                var lightData = frameData.Get<UniversalLightData>();
                DrawingSettings ds = RenderingUtils.CreateDrawingSettings(s_Tags, renderingData, cameraData, lightData, SortingCriteria.CommonOpaque);
                ds.overrideMaterial = m_Mat;
                ds.overrideMaterialPassIndex = 0;
                var filter = new FilteringSettings(RenderQueueRange.opaque, -1, LayerMask);
                using (var b = rg.AddRasterRenderPass<Data>("NocFacadePrepass", out var d, s_Sampler))
                {
                    d.list = rg.CreateRendererList(new RendererListParams(renderingData.cullResults, ds, filter));
                    b.UseRendererList(d.list);
                    b.SetRenderAttachmentDepth(res.activeDepthTexture, AccessFlags.Write);
                    b.AllowPassCulling(false);
                    b.SetRenderFunc((Data data, RasterGraphContext ctx) => ctx.cmd.DrawRendererList(data.list));
                }
            }
        }
    }
}
