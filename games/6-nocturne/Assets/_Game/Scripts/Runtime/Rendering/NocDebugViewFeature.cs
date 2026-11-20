// NocDebugViewFeature: the debug views of TEXTURE_PLAN 7.1 (TP-0), active only with -debug-view N (NocLookGlobals).
// Sits in PC_Renderer AFTER NocGradeFeature (PipelineConfigurator), shader = Megastructure/NocDebugView. Game cameras only (the
// capture harness' SubmitRenderRequest renders included). Two passes per frame share a texture through a ContextItem:
//   views 1-8 and 10 (written by the lit template and the material hooks as raw display codes):
//     A at AfterRenderingTransparents (before the rain at BeforeRenderingPostProcessing): the MSAA-resolved camera colour is
//       copied into a float texture;
//     B at AfterRenderingPostProcessing (after the grade, same event, enqueued later): the copy is written back into the
//       camera colour, so neither the rain nor the grade nor the dither touch the codes (the grade output is sRGB 8-bit: a
//       linear value is encoded exactly back into its code; without the grade FinalBlit copies the float colour 1:1).
//   view 9 (overdraw of the chosen renderers, default every NocFacade renderer; -debug-shaders NocFacade+NocSurface):
//     A at AfterRenderingOpaques: every other opaque renderer primes a cleared depth (pass Prime, depth only), then the chosen
//       renderers are drawn twice into a cleared R16G16 float texture in the opaque order (CommonOpaque): pass Count (ZTest
//       LEqual, ZWrite On: fragments the forward pass shades without a prepass) into R, pass Complexity (ZTest Always: depth
//       complexity) into G;
//     B at AfterRenderingPostProcessing: R = 8 x shaded, G = 8 x complexity, B = 255 where covered (Tools/qa/debugview.py --view 9:
//       overdraw = sum R / sum covered, both / 8).
//   The overdraw renderers are re-tagged once the world is built to rendering-layer bit 30 only (plus the prepass bit 29;
//   MeshRenderer.renderingLayerMask; the URP opaque pass draws every rendering layer) and filtered by it; the Prime pass takes
//   every renderer with any other bit (2026-11-16: with bit 30 merely added, the facades primed themselves and counted 1.000).
// Zero cost and no pass when -debug-view is 0. Allocation-free per frame after the first tagging.
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

namespace Megastructure
{
    [DisallowMultipleRendererFeature("Noc Debug View")]
    public sealed class NocDebugViewFeature : ScriptableRendererFeature
    {
        /// <summary>Megastructure/NocDebugView (set by PipelineConfigurator; the reference also ships the shader).</summary>
        public Shader shader;

        /// <summary>Rendering-layer bit of the renderers counted by the overdraw view.</summary>
        public const int OverdrawLayerBit = 30;
        public const uint OverdrawLayerMask = 1u << OverdrawLayerBit;

        Material m_Material;
        CapturePass m_Capture;
        OutputPass m_Output;
        static int s_Tagged = -1;

        sealed class DebugData : ContextItem
        {
            public TextureHandle tex;
            public bool overdraw;
            public override void Reset() { tex = TextureHandle.nullHandle; overdraw = false; }
        }

        public override void Create()
        {
            if (m_Material == null && shader != null && shader.isSupported) m_Material = CoreUtils.CreateEngineMaterial(shader);
            m_Capture = new CapturePass();
            m_Output = new OutputPass();
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            int view = NocLookGlobals.DebugView;
            if (view <= 0 || m_Material == null) return;
            if (renderingData.cameraData.cameraType != CameraType.Game) return;
            bool overdraw = view == NocLookGlobals.DebugViewOverdraw;
            if (overdraw && s_Tagged <= 0 && NocRig.WorldReady) s_Tagged = TagOverdrawRenderers();
            m_Capture.Setup(m_Material, overdraw);
            m_Output.Setup(m_Material, overdraw);
            renderer.EnqueuePass(m_Capture);
            renderer.EnqueuePass(m_Output);
        }

        protected override void Dispose(bool disposing)
        {
            CoreUtils.Destroy(m_Material);
            m_Material = null;
        }

        /// <summary>Sets OverdrawLayerBit on every MeshRenderer whose material uses one of NocLookGlobals.OverdrawShaders.</summary>
        public static int TagOverdrawRenderers()
        {
            var names = new HashSet<string>(NocLookGlobals.OverdrawShaders);
            int n = 0;
            foreach (var r in Object.FindObjectsByType<MeshRenderer>(FindObjectsInactive.Exclude))
            {
                bool hit = false;
                foreach (var m in r.sharedMaterials) if (m != null && m.shader != null && names.Contains(m.shader.name)) { hit = true; break; }
                if (!hit) continue;
                // only bit 30 (plus the prepass bit 29 when it is set): the Prime pass draws every renderer that has any other
                // rendering-layer bit, so a counted renderer must have none (debug view 9 only; nothing reads bit 0 for lighting)
                r.renderingLayerMask = OverdrawLayerMask | (r.renderingLayerMask & NocFacadePrepassFeature.LayerMask);
                n++;
            }
            Debug.Log(System.FormattableString.Invariant($"[NOC] NocDebugView: overdraw view counts {n} renderers ({string.Join(", ", NocLookGlobals.OverdrawShaders)})"));
            return n;
        }

        // ------------------------------------------------------------------------------------------------ A: capture
        sealed class CapturePass : ScriptableRenderPass
        {
            Material m_Mat;
            bool m_Overdraw;
            static readonly ShaderTagId[] s_Tags = { new ShaderTagId("UniversalForward"), new ShaderTagId("UniversalForwardOnly"), new ShaderTagId("SRPDefaultUnlit") };
            static readonly List<ShaderTagId> s_TagList = new List<ShaderTagId>(s_Tags);
            static readonly ProfilingSampler s_Sampler = new ProfilingSampler("NocDebugView Capture");

            sealed class CopyData { public TextureHandle src; public Material mat; }
            sealed class CountData { public RendererListHandle prime, count, complexity; }

            public CapturePass() { profilingSampler = s_Sampler; requiresIntermediateTexture = true; }

            public void Setup(Material mat, bool overdraw)
            {
                m_Mat = mat;
                m_Overdraw = overdraw;
                renderPassEvent = overdraw ? RenderPassEvent.AfterRenderingOpaques : RenderPassEvent.AfterRenderingTransparents;
            }

            public override void RecordRenderGraph(RenderGraph rg, ContextContainer frameData)
            {
                var res = frameData.Get<UniversalResourceData>();
                if (res.isActiveTargetBackBuffer || !res.cameraColor.IsValid()) return;
                TextureDesc cd = rg.GetTextureDesc(res.cameraColor);
                var dd = frameData.Create<DebugData>();
                dd.overdraw = m_Overdraw;
                var desc = new TextureDesc(cd.width, cd.height)
                {
                    format = m_Overdraw ? GraphicsFormat.R16G16_SFloat : GraphicsFormat.R16G16B16A16_SFloat,
                    filterMode = FilterMode.Point,
                    wrapMode = TextureWrapMode.Clamp,
                    msaaSamples = MSAASamples.None,
                    clearBuffer = m_Overdraw,
                    clearColor = Color.clear,
                    name = "_NocDbgTex",
                };
                dd.tex = rg.CreateTexture(desc);
                if (!m_Overdraw)
                {
                    using (var b = rg.AddRasterRenderPass<CopyData>("NocDebugView Copy", out var d, s_Sampler))
                    {
                        d.src = res.cameraColor;
                        d.mat = m_Mat;
                        b.UseTexture(res.cameraColor, AccessFlags.Read);        // an MSAA camera colour is resolved here
                        b.SetRenderAttachment(dd.tex, 0, AccessFlags.WriteAll);
                        b.SetRenderFunc((CopyData data, RasterGraphContext ctx) =>
                        {
                            s_Mpb.Clear();
                            s_Mpb.SetTexture(s_Src, data.src);
                            ctx.cmd.DrawProcedural(Matrix4x4.identity, data.mat, PassCopy, MeshTopology.Triangles, 3, 1, s_Mpb);
                        });
                    }
                    return;
                }
                var renderingData = frameData.Get<UniversalRenderingData>();
                var cameraData = frameData.Get<UniversalCameraData>();
                var lightData = frameData.Get<UniversalLightData>();
                var depthDesc = new TextureDesc(cd.width, cd.height)
                {
                    format = GraphicsFormat.D32_SFloat,
                    msaaSamples = MSAASamples.None,
                    clearBuffer = true,
                    name = "_NocDbgDepth",
                };
                TextureHandle depth = rg.CreateTexture(depthDesc);
                var filter = new FilteringSettings(RenderQueueRange.opaque, -1, OverdrawLayerMask);
                var others = new FilteringSettings(RenderQueueRange.opaque, -1, ~(OverdrawLayerMask | NocFacadePrepassFeature.LayerMask));
                DrawingSettings dsp = RenderingUtils.CreateDrawingSettings(s_TagList, renderingData, cameraData, lightData, SortingCriteria.CommonOpaque);
                dsp.overrideMaterial = m_Mat;
                dsp.overrideMaterialPassIndex = PassPrime;
                DrawingSettings ds = RenderingUtils.CreateDrawingSettings(s_TagList, renderingData, cameraData, lightData, SortingCriteria.CommonOpaque);
                ds.overrideMaterial = m_Mat;
                ds.overrideMaterialPassIndex = PassCount;
                DrawingSettings ds2 = RenderingUtils.CreateDrawingSettings(s_TagList, renderingData, cameraData, lightData, SortingCriteria.CommonOpaque);
                ds2.overrideMaterial = m_Mat;
                ds2.overrideMaterialPassIndex = PassComplexity;
                using (var b = rg.AddRasterRenderPass<CountData>("NocDebugView Overdraw", out var d, s_Sampler))
                {
                    d.prime = rg.CreateRendererList(new RendererListParams(renderingData.cullResults, dsp, others));
                    d.count = rg.CreateRendererList(new RendererListParams(renderingData.cullResults, ds, filter));
                    d.complexity = rg.CreateRendererList(new RendererListParams(renderingData.cullResults, ds2, filter));
                    b.UseRendererList(d.prime);
                    b.UseRendererList(d.count);
                    b.UseRendererList(d.complexity);
                    b.SetRenderAttachment(dd.tex, 0, AccessFlags.Write);
                    b.SetRenderAttachmentDepth(depth, AccessFlags.Write);
                    b.SetRenderFunc((CountData data, RasterGraphContext ctx) =>
                    {
                        ctx.cmd.DrawRendererList(data.prime);
                        ctx.cmd.DrawRendererList(data.count);
                        ctx.cmd.DrawRendererList(data.complexity);
                    });
                }
            }
        }

        // ------------------------------------------------------------------------------------------------ B: output
        sealed class OutputPass : ScriptableRenderPass
        {
            Material m_Mat;
            bool m_Overdraw;
            static readonly ProfilingSampler s_Sampler = new ProfilingSampler("NocDebugView Output");
            sealed class OutData { public TextureHandle src; public Material mat; public int pass; }

            public OutputPass() { profilingSampler = s_Sampler; renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing; requiresIntermediateTexture = true; }

            public void Setup(Material mat, bool overdraw) { m_Mat = mat; m_Overdraw = overdraw; }

            public override void RecordRenderGraph(RenderGraph rg, ContextContainer frameData)
            {
                if (!frameData.Contains<DebugData>()) return;
                var dd = frameData.Get<DebugData>();
                var res = frameData.Get<UniversalResourceData>();
                if (!dd.tex.IsValid() || res.isActiveTargetBackBuffer || !res.cameraColor.IsValid()) return;
                using (var b = rg.AddRasterRenderPass<OutData>("NocDebugView Output", out var d, s_Sampler))
                {
                    d.src = dd.tex;
                    d.mat = m_Mat;
                    d.pass = dd.overdraw ? PassOverdraw : PassCopy;
                    b.UseTexture(dd.tex, AccessFlags.Read);
                    b.SetRenderAttachment(res.cameraColor, 0, AccessFlags.WriteAll);
                    b.SetRenderFunc((OutData data, RasterGraphContext ctx) =>
                    {
                        s_Mpb.Clear();
                        s_Mpb.SetTexture(s_Src, data.src);
                        ctx.cmd.DrawProcedural(Matrix4x4.identity, data.mat, data.pass, MeshTopology.Triangles, 3, 1, s_Mpb);
                    });
                }
            }
        }

        const int PassCount = 0, PassComplexity = 1, PassCopy = 2, PassOverdraw = 3, PassPrime = 4;
        static readonly int s_Src = Shader.PropertyToID("_NocDbgSrc");
        static readonly MaterialPropertyBlock s_Mpb = new MaterialPropertyBlock();
    }
}
