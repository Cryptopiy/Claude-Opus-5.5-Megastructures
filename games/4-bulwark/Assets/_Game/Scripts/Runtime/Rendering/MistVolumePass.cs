// MistVolumePass: the Render Graph passes behind MistVolumeFeature (MASTER_PROMPT 14.7 H2, Part 8.6 MSAA rule; research
// docs/RESEARCH.md B 7). Recorded per Game camera at RenderPassEvent.AfterRenderingSkybox (after the opaques and the
// far-field dome at Geometry+499, before the transparents and BwGrade):
//   MSAA path (the camera depth attachment is 4x and bindTextureMS, i.e. SystemInfo.supportsMultisampleResolveDepth is
//   false and supportsMultisampledTextures is true: Direct3D 11):
//     1. "BwMist mark"   depth-stencil attachment = a transient MSAA D24S8 of our own (the camera stencil cannot be used:
//                        Render Graph forbids UseTexture and SetRenderAttachmentDepth on the same texture in one pass,
//                        RenderGraphBuilders.CheckNotUseFragment / CheckUseFragment), UseTexture(camera depth) bound as
//                        Texture2DMS<float, 4> _BwMistDepthMS; marks edge pixels whose ray touches the mist (stencil 1).
//     2. "BwMist pixel"  colour attachment = the MSAA camera colour (ReadWrite, Blend One SrcAlpha), the stencil (read,
//                        NotEqual 1), the camera depth again as Texture2DMS: one integration per agreeing pixel.
//     3. "BwMist sample" the same attachments, stencil Equal 1, SV_SampleIndex: per-sample integration on edge pixels.
//   Fallback (logged once): one "BwMist single" pass reading a single-sample depth (the 1x depth attachment, or the
//   resolved _CameraDepthTexture with the nearest of its 3 x 3 neighbourhood when MSAA is on but not bindable).
// Zero GC allocations per frame: pooled PassData, one static MaterialPropertyBlock, a cached static render function.
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

namespace Megastructure.Rendering
{
    public sealed class MistVolumePass : ScriptableRenderPass
    {
        const string kMark = "BwMistMark", kPixel = "BwMistPixel", kSample = "BwMistSample", kSingle = "BwMistSingle";
        static readonly ProfilingSampler s_MarkSampler = new ProfilingSampler("BwMist Mark");
        static readonly ProfilingSampler s_PixelSampler = new ProfilingSampler("BwMist Pixel");
        static readonly ProfilingSampler s_SampleSampler = new ProfilingSampler("BwMist Sample");
        static readonly ProfilingSampler s_SingleSampler = new ProfilingSampler("BwMist Single");
        static readonly MaterialPropertyBlock s_Mpb = new MaterialPropertyBlock();
        static readonly BaseRenderFunc<PassData, RasterGraphContext> s_Exec = Exec;

        /// <summary>Path taken by the last recorded camera (diagnostics, the report): "msaa", "single", "single-3x3".</summary>
        public static string LastPath { get; private set; } = "none";
        static bool s_LoggedMsaa, s_LoggedFallback, s_LoggedSkip;

        Material m_Material;
        int m_PMark = -1, m_PPixel = -1, m_PSample = -1, m_PSingle = -1;
        bool m_Valid;

        sealed class PassData
        {
            public Material material;
            public int pass;
            public TextureHandle depth;
            public bool ms;
            public Vector4 screen;
            public float nearest;
        }

        public MistVolumePass()
        {
            profilingSampler = s_PixelSampler;
            renderPassEvent = RenderPassEvent.AfterRenderingSkybox;
            requiresIntermediateTexture = true;   // the MSAA camera depth is bindable only on the intermediate target
        }

        public bool Setup(Material material)
        {
            if (material == m_Material) return m_Valid;
            m_Material = material;
            m_Valid = false;
            if (material == null || material.shader == null || !material.shader.isSupported) return false;
            m_PMark = material.FindPass(kMark);
            m_PPixel = material.FindPass(kPixel);
            m_PSample = material.FindPass(kSample);
            m_PSingle = material.FindPass(kSingle);
            m_Valid = m_PMark >= 0 && m_PPixel >= 0 && m_PSample >= 0 && m_PSingle >= 0;
            if (!m_Valid) Debug.LogWarning("[BW] MistVolume: Megastructure/BwMist lacks its passes; mist disabled");
            return m_Valid;
        }

        static void Exec(PassData d, RasterGraphContext ctx)
        {
            MaterialPropertyBlock mpb = s_Mpb;
            mpb.Clear();
            Texture depth = d.depth;
            if (depth != null) mpb.SetTexture(d.ms ? MistVolumeState.IdDepthMS : MistVolumeState.IdDepth, depth);
            mpb.SetVector(MistVolumeState.IdScreen, d.screen);
            mpb.SetFloat(MistVolumeState.IdNearest3x3, d.nearest);
            ctx.cmd.DrawProcedural(Matrix4x4.identity, d.material, d.pass, MeshTopology.Triangles, 3, 1, mpb);
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            if (!m_Valid || m_Material == null || !MistVolumeState.Enabled) return;
            var res = frameData.Get<UniversalResourceData>();
            var cam = frameData.Get<UniversalCameraData>();
            if (res.isActiveTargetBackBuffer)
            {
                if (!s_LoggedSkip) { s_LoggedSkip = true; Debug.LogWarning("[BW] MistVolume: camera renders to the back buffer; mist skipped"); }
                return;
            }
            TextureHandle color = res.activeColorTexture, depth = res.activeDepthTexture;
            if (!color.IsValid() || !depth.IsValid()) return;
            TextureDesc cd = renderGraph.GetTextureDesc(color);
            TextureDesc dd = renderGraph.GetTextureDesc(depth);
            int W = cd.width, H = cd.height;
            if (cd.sizeMode != TextureSizeMode.Explicit || W < 1 || H < 1)
            {
                W = cam.cameraTargetDescriptor.width;
                H = cam.cameraTargetDescriptor.height;
            }
            if (W < 1 || H < 1) return;
            var screen = new Vector4(W, H, 1f / W, 1f / H);
            // the spray slots for this camera's eye (after every updater of the frame: the kraken's SeaDisturbances are current)
            MistVolumeState.CommitSprays(cam.worldSpaceCameraPos);

            bool msaa4 = dd.msaaSamples == MSAASamples.MSAA4x && cd.msaaSamples == MSAASamples.MSAA4x;
            bool ms = msaa4 && dd.bindTextureMS && SystemInfo.supportsMultisampledTextures != 0 && MistVolumeState.AllowMsaa;
            if (ms)
            {
                if (!s_LoggedMsaa)
                {
                    s_LoggedMsaa = true;
                    Debug.Log($"[BW] MistVolume: MSAA per-sample path (depth msaa={dd.msaaSamples} bindTextureMS={dd.bindTextureMS} " +
                              $"format={dd.format}; supportsMultisampleResolveDepth={SystemInfo.supportsMultisampleResolveDepth} " +
                              $"supportsMultisampleResolveStencil={SystemInfo.supportsMultisampleResolveStencil} " +
                              $"supportsMultisampledTextures={SystemInfo.supportsMultisampledTextures} reversedZ={SystemInfo.usesReversedZBuffer}; " +
                              $"own stencil D24S8 {W}x{H}x4)");
                }
                LastPath = "msaa";
                TextureDesc sd = dd;
                sd.name = "_BwMistStencil";
                sd.format = GraphicsFormat.D24_UNorm_S8_UInt;
                sd.bindTextureMS = false;
                sd.clearBuffer = true;
                sd.clearColor = Color.black;
                sd.useMipMap = false;
                sd.enableRandomWrite = false;
                sd.isShadowMap = false;
                sd.discardBuffer = false;
                TextureHandle stencil = renderGraph.CreateTexture(sd);

                using (var b = renderGraph.AddRasterRenderPass<PassData>("BwMist mark", out var d, s_MarkSampler))
                {
                    Fill(d, m_PMark, depth, true, screen, 0f);
                    b.SetRenderAttachmentDepth(stencil, AccessFlags.Write);
                    b.UseTexture(depth, AccessFlags.Read);
                    b.AllowPassCulling(false);
                    b.SetRenderFunc(s_Exec);
                }
                using (var b = renderGraph.AddRasterRenderPass<PassData>("BwMist pixel", out var d, s_PixelSampler))
                {
                    Fill(d, m_PPixel, depth, true, screen, 0f);
                    b.SetRenderAttachment(color, 0, AccessFlags.ReadWrite);
                    b.SetRenderAttachmentDepth(stencil, AccessFlags.Read);
                    b.UseTexture(depth, AccessFlags.Read);
                    b.AllowPassCulling(false);
                    b.SetRenderFunc(s_Exec);
                }
                using (var b = renderGraph.AddRasterRenderPass<PassData>("BwMist sample", out var d, s_SampleSampler))
                {
                    Fill(d, m_PSample, depth, true, screen, 0f);
                    b.SetRenderAttachment(color, 0, AccessFlags.ReadWrite);
                    b.SetRenderAttachmentDepth(stencil, AccessFlags.Read);
                    b.UseTexture(depth, AccessFlags.Read);
                    b.AllowPassCulling(false);
                    b.SetRenderFunc(s_Exec);
                }
                return;
            }

            // fallback: a single-sample depth
            TextureHandle src;
            float nearest;
            if (dd.msaaSamples == MSAASamples.None) { src = depth; nearest = 0f; LastPath = "single"; }
            else { src = res.cameraDepthTexture; nearest = 1f; LastPath = "single-3x3"; }
            if (!src.IsValid()) return;
            if (!s_LoggedFallback)
            {
                s_LoggedFallback = true;
                Debug.Log($"[BW] MistVolume: single-sample fallback ({LastPath}; depth msaa={dd.msaaSamples} bindTextureMS={dd.bindTextureMS}, " +
                          $"allowMsaa={MistVolumeState.AllowMsaa}): edge pixels take {(nearest > 0f ? "the nearest of the 3 x 3 resolved depth (a 1 px dark outline is the accepted cost)" : "their only sample")}");
            }
            using (var b = renderGraph.AddRasterRenderPass<PassData>("BwMist single", out var d, s_SingleSampler))
            {
                Fill(d, m_PSingle, src, false, screen, nearest);
                b.SetRenderAttachment(color, 0, AccessFlags.ReadWrite);
                b.UseTexture(src, AccessFlags.Read);
                b.AllowPassCulling(false);
                b.SetRenderFunc(s_Exec);
            }
        }

        void Fill(PassData d, int pass, TextureHandle depth, bool ms, Vector4 screen, float nearest)
        {
            d.material = m_Material;
            d.pass = pass;
            d.depth = depth;
            d.ms = ms;
            d.screen = screen;
            d.nearest = nearest;
        }
    }
}
