// NocRainPass: the Render Graph passes of NocRainFeature (MASTER_PROMPT 14.7 H5, D-005; R3 1.2, R2 1.5). Per Game camera, at
// BeforeRenderingPostProcessing (550: after the transparent queues, before NocGrade at 600):
//   1. NocRainLight: UseTexture(cameraColor) (the native pass compiler resolves the MSAA colour here) -> 1/8-res RGBA16F, the exact
//      8 x 8 box of the HDR scene (16 bilinear taps);
//   2. NocRainBlur: 3 x 3 tent at 1/8 -> the local light Lloc and its hue;
//   3. NocRain: SetRenderAttachment(cameraColor, ReadWrite) + SetRenderAttachmentDepth(activeDepthTexture, Read) (both MSAA 4) and
//      four DrawProcedural full-screen triangles, one per layer (3, 6, 12, 20 m), Blend One One, each writing SV_Depth = its layer
//      depth so the hardware tests it against every MSAA sample of the camera depth (exact per-sample occlusion, no halo on rails).
// Matrices are camera-relative (view without translation) with GL.GetGPUProjectionMatrix(proj, true): the camera always renders
// into an intermediate texture (requiresIntermediateTexture), so ray reconstruction and SV_Depth use the scene's own projection.
// Per-frame values come from NocRainState (AtmosphereDriver) in double: phases as integer + fraction (R2 1.7). The first record
// writes the one [NocProbe] line of R3 5.6. Zero GC allocations per frame (static lambdas, pooled pass data, one static MPB).
using System;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

namespace Megastructure
{
    public sealed class NocRainPass : ScriptableRenderPass
    {
        const string kPassLight = "NocRainLight", kPassBlur = "NocRainBlur", kPassLayer = "NocRainLayer";

        static readonly int s_Src = Shader.PropertyToID("_NocRainSrc");
        static readonly int s_Light = Shader.PropertyToID("_NocRainLight");
        static readonly int s_SrcSize = Shader.PropertyToID("_NocRainSrcSize");
        static readonly int s_Screen = Shader.PropertyToID("_NocRainScreen");
        static readonly int s_IVP = Shader.PropertyToID("_NocRainIVP");
        static readonly int s_VP = Shader.PropertyToID("_NocRainVP");
        static readonly int s_A = Shader.PropertyToID("_NocRainA");
        static readonly int s_B = Shader.PropertyToID("_NocRainB");
        static readonly int s_L = Shader.PropertyToID("_NocRainL");
        static readonly int s_M = Shader.PropertyToID("_NocRainM");

        static readonly ProfilingSampler s_LightSampler = new ProfilingSampler("NocRain Light");
        static readonly ProfilingSampler s_BlurSampler = new ProfilingSampler("NocRain Blur");
        static readonly ProfilingSampler s_RainSampler = new ProfilingSampler("NocRain");
        static readonly MaterialPropertyBlock s_Mpb = new MaterialPropertyBlock();
        static readonly BaseRenderFunc<BlitData, RasterGraphContext> s_Blit = ExecuteBlit;
        static readonly BaseRenderFunc<RainData, RasterGraphContext> s_Rain = ExecuteRain;

        /// <summary>True once the [NocProbe] line was written (one per process).</summary>
        public static bool ProbeLogged;

        Material m_Material;
        Shader m_Shader;
        int m_PassLight = -1, m_PassBlur = -1, m_PassLayer = -1;
        bool m_Valid;

        public NocRainPass()
        {
            profilingSampler = s_RainSampler;
            renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
            requiresIntermediateTexture = true;
        }

        public bool Bind(Material material)
        {
            Shader shader = material != null ? material.shader : null;
            if (material == m_Material && shader == m_Shader) return m_Valid;
            m_Material = material;
            m_Shader = shader;
            m_Valid = false;
            if (material == null || shader == null || !shader.isSupported) return false;
            m_PassLight = material.FindPass(kPassLight);
            m_PassBlur = material.FindPass(kPassBlur);
            m_PassLayer = material.FindPass(kPassLayer);
            m_Valid = m_PassLight >= 0 && m_PassBlur >= 0 && m_PassLayer >= 0;
            if (!m_Valid) Debug.LogWarning("[NOC] NocRain: material shader lacks the NocRain passes; rain disabled");
            return m_Valid;
        }

        sealed class BlitData
        {
            public TextureHandle src;
            public Material material;
            public int pass;
            public Vector4 srcSize;
        }

        sealed class RainData
        {
            public TextureHandle light;
            public Material material;
            public int pass;
            public Matrix4x4 vp, ivp;
            public Vector4 screen, lightScale, a, b;
            public readonly Vector4[] l = new Vector4[4];
            public readonly Vector4[] m = new Vector4[4];
        }

        static void ExecuteBlit(BlitData d, RasterGraphContext ctx)
        {
            MaterialPropertyBlock mpb = s_Mpb;
            mpb.Clear();
            Texture src = d.src;
            mpb.SetTexture(s_Src, src);
            mpb.SetVector(s_SrcSize, d.srcSize);
            ctx.cmd.DrawProcedural(Matrix4x4.identity, d.material, d.pass, MeshTopology.Triangles, 3, 1, mpb);
        }

        static void ExecuteRain(RainData d, RasterGraphContext ctx)
        {
            MaterialPropertyBlock mpb = s_Mpb;
            Texture light = d.light;
            for (int l = 0; l < 4; l++)
            {
                if (d.m[l].w <= 0f) continue;
                mpb.Clear();
                mpb.SetTexture(s_Light, light);
                mpb.SetVector(s_SrcSize, d.lightScale);
                mpb.SetVector(s_Screen, d.screen);
                mpb.SetMatrix(s_IVP, d.ivp);
                mpb.SetMatrix(s_VP, d.vp);
                mpb.SetVector(s_A, d.a);
                mpb.SetVector(s_B, d.b);
                mpb.SetVector(s_L, d.l[l]);
                mpb.SetVector(s_M, d.m[l]);
                ctx.cmd.DrawProcedural(Matrix4x4.identity, d.material, d.pass, MeshTopology.Triangles, 3, 1, mpb);
            }
        }

        void AddBlit(RenderGraph rg, string name, ProfilingSampler sampler, int pass, TextureHandle src, Vector4 srcSize, TextureHandle dst)
        {
            using (var builder = rg.AddRasterRenderPass<BlitData>(name, out var d, sampler))
            {
                d.src = src;
                d.material = m_Material;
                d.pass = pass;
                d.srcSize = srcSize;
                builder.UseTexture(src, AccessFlags.Read);
                builder.SetRenderAttachment(dst, 0, AccessFlags.WriteAll);
                builder.SetRenderFunc(s_Blit);
            }
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            if (!m_Valid || m_Material == null) return;
            var res = frameData.Get<UniversalResourceData>();
            var cam = frameData.Get<UniversalCameraData>();
            if (res.isActiveTargetBackBuffer) return;
            TextureHandle color = res.cameraColor, depth = res.activeDepthTexture;
            if (!color.IsValid() || !depth.IsValid()) return;
            TextureDesc cd = renderGraph.GetTextureDesc(color);
            if (!ProbeLogged) Probe(renderGraph, cam, cd, depth);
            if (NocRainState.Rho <= 0.0) return;

            int W = cd.width, H = cd.height;
            if (W < 8 || H < 8) return;
            int lw = (W + NocturneConstants.RainLightDiv - 1) / NocturneConstants.RainLightDiv;
            int lh = (H + NocturneConstants.RainLightDiv - 1) / NocturneConstants.RainLightDiv;
            var ld = new TextureDesc(lw, lh)
            {
                format = GraphicsFormat.R16G16B16A16_SFloat,
                filterMode = FilterMode.Bilinear,          // the ctor defaults are Point / Repeat (Thread R1 pitfall 2)
                wrapMode = TextureWrapMode.Clamp,
                msaaSamples = MSAASamples.None,
                clearBuffer = false,
                name = "_NocRainLight0",
            };
            TextureHandle light0 = renderGraph.CreateTexture(ld);
            ld.name = "_NocRainLight1";
            TextureHandle light1 = renderGraph.CreateTexture(ld);
            AddBlit(renderGraph, kPassLight, s_LightSampler, m_PassLight, color, new Vector4(W, H, 1f / W, 1f / H), light0);
            AddBlit(renderGraph, kPassBlur, s_BlurSampler, m_PassBlur, light0, new Vector4(lw, lh, 1f / lw, 1f / lh), light1);

            // camera-relative matrices (translation dropped), GPU projection into a texture (the intermediate camera colour)
            Matrix4x4 view = cam.GetViewMatrix();
            view.SetColumn(3, new Vector4(0f, 0f, 0f, 1f));
            Matrix4x4 proj = GL.GetGPUProjectionMatrix(cam.GetProjectionMatrix(), true);
            Matrix4x4 vp = proj * view;

            using (var builder = renderGraph.AddRasterRenderPass<RainData>("NocRain", out var d, s_RainSampler))
            {
                d.light = light1;
                d.material = m_Material;
                d.pass = m_PassLayer;
                d.vp = vp;
                d.ivp = vp.inverse;
                d.screen = new Vector4(W, H, 1f / W, 1f / H);
                d.lightScale = new Vector4(lw, lh, 1f / (lw * NocturneConstants.RainLightDiv), 1f / (lh * NocturneConstants.RainLightDiv));
                double t = NocRainState.Time;
                double nx = t / 9.0, ny = t / 13.0;
                nx -= NocturneConstants.RainNoiseCells * Math.Floor(nx / NocturneConstants.RainNoiseCells);
                float gain = Mathf.Pow(2f, GradeState.ExposureEV);
                d.a = new Vector4((float)(NocturneConstants.RainSlotP * NocRainState.Rho), (float)NocRainState.Slope, H / 1080f, (float)nx);
                d.b = new Vector4((float)(ny - 4096.0 * Math.Floor(ny / 4096.0)), (float)NocturneConstants.RainFadePx0, (float)NocturneConstants.RainFadePx1, gain);
                for (int l = 0; l < 4; l++)
                {
                    double z = NocturneConstants.RainLayerZ[l];
                    NocRainMath.Phases(z, NocRainState.Drift, t, out double iBase, out double uFrac, out double jBase, out double vFrac);
                    d.l[l] = new Vector4((float)uFrac, (float)vFrac, (float)z, (float)iBase);
                    d.m[l] = new Vector4((float)jBase, l, NocRainState.Noise ? 1f : 0f, 1f);
                }
                builder.UseTexture(light1, AccessFlags.Read);
                builder.SetRenderAttachment(color, 0, AccessFlags.ReadWrite);
                builder.SetRenderAttachmentDepth(depth, AccessFlags.Read);
                builder.SetRenderFunc(s_Rain);
            }
        }

        /// <summary>The one [NocProbe] line (R3 5.6): the D-008 facts as the player really runs them.</summary>
        static void Probe(RenderGraph rg, UniversalCameraData cam, TextureDesc cd, TextureHandle depth)
        {
            ProbeLogged = true;
            TextureDesc dd = rg.GetTextureDesc(depth);
            var asset = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
            string a = asset == null ? "asset none" : FormattableString.Invariant(
                $"hdr {asset.supportsHDR} precision {asset.hdrColorBufferPrecision} msaa {asset.msaaSampleCount} depthTex {asset.supportsCameraDepthTexture} opaqueTex {asset.supportsCameraOpaqueTexture} mainShadows {asset.supportsMainLightShadows} addShadows {asset.supportsAdditionalLightShadows} addLights {asset.additionalLightsRenderingMode} lodCrossFade {asset.enableLODCrossFade} srpBatcher {asset.useSRPBatcher}");
            string mode = "?";
            try
            {
                // the renderer's actual mode (internal in URP 17.6; read once for this log line only)
                var pi = cam.renderer != null ? cam.renderer.GetType().GetProperty("renderingModeActual", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic) : null;
                if (pi != null) mode = pi.GetValue(cam.renderer).ToString();
            }
            catch (Exception) { mode = "unreadable"; }
            Camera c = cam.camera;
            string camInfo = c == null ? "camera none" : FormattableString.Invariant(
                $"clear {c.clearFlags} skybox {(RenderSettings.skybox != null ? RenderSettings.skybox.name : "none")} hdr {c.allowHDR} msaa {c.allowMSAA} near {c.nearClipPlane} far {c.farClipPlane}");
            Debug.Log(FormattableString.Invariant(
                $"[NocProbe] api {SystemInfo.graphicsDeviceType} reversedZ {SystemInfo.usesReversedZBuffer} msaaDepthResolve {SystemInfo.supportsMultisampleResolveDepth} msTextures {SystemInfo.supportsMultisampledTextures} colour {cd.format} x{(int)cd.msaaSamples} depth {dd.format} x{(int)dd.msaaSamples} bindMS {dd.bindTextureMS} renderingMode {mode} | {a} | {camInfo}"));
        }
    }
}
