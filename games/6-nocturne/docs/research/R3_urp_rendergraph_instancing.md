# R3: URP 17.6 Render Graph, MSAA depth, instancing, Forward+, UI Toolkit and 6000.6 gotchas for Nocturne

Researcher R3, read-only, 2026-11-13. Ground truth is the package source in this project. Path aliases:
- `URP/` = `Library/PackageCache/com.unity.render-pipelines.universal@8457e85b8184/` (package.json: 17.6.0)
- `CORE/` = `Library/PackageCache/com.unity.render-pipelines.core@2d66c71e606e/`
- `XML-CORE` = `C:\Program Files\Unity\Hub\Editor\6000.6.0f1\Editor\Data\Managed\UnityEngine\UnityEngine.CoreModule.xml`
- `XML-UIE` = the `UnityEngine.UIElementsModule.xml` next to it
- `CGINC/` = `C:\Program Files\Unity\Hub\Editor\6000.6.0f1\Editor\Data\Resources\CGIncludes\`

Note: the task text names the folder `...universal@17.6.0`. The real folder is `@8457e85b8184` (version 17.6.0). Core is `@2d66c71e606e`.

## 0. Summary

1. **This is the same URP/Core package (same content hash) that Thread and The Ring used.** Their research cites `@8457e85b8184` and `@2d66c71e606e`, the same folders as here, and both lock files say `17.6.0 builtin`. Every file:line in Thread `docs/research/R1..R3` and The Ring `docs/research/01, 02, 03, 06, 09` applies unchanged. Nocturne's risks come from how it uses the pipeline, not from a version change.
2. **Under MSAA, `_CameraDepthTexture` is not a resolve. It holds the FARTHEST of the 4 samples.** `CopyDepthPass.hlsl:35-54` uses `DEPTH_OP min` under `UNITY_REVERSED_Z`. On D3D11 depth cannot be hardware-resolved, so the camera depth attachment is created `bindTextureMS` and the copy goes through that MSAA path. A full-screen rain composite that tests against this texture draws rain over the edge pixels of thin near objects. That is exactly the halo Part 8.6 forbids.
3. **Recommended NocRain design ("approach A").**
   - A raster pass at `BeforeRenderingPostProcessing` (550) blends each rain layer straight into the still-multisampled `cameraColor`, before ThreadGrade (600).
   - `cameraDepth` is bound read-only as the depth attachment, and the shader outputs `SV_Depth` = the depth of the layer hit.
   - The GPU depth-tests that single depth **per sample** (D3D11 pixel-shader-stage spec), so occlusion by rails and wires is exact, needs no depth texture, and works on any API.
   - A tiny pre-pass reads the resolved colour for `Lloc`. This costs one extra MSAA resolve: about 0.14 ms at 1080p and about 0.55 ms at 4K.
4. **Signs, vehicles, lanterns, fireflies.** `Graphics.RenderMeshInstanced` is capped at 511 instances per call (1023 with `assumeuniformscaling`) and has no managed check. It also cannot carry per-instance colour without an MPB array or an instancing cbuffer, and that cbuffer breaks the SRP Batcher in non-instanced variants (`UnityInstancing.hlsl:495`). Recommendation: reuse **The Ring's `InstancedField`**. It wraps `RenderMeshPrimitives`/`RenderMeshIndirect`, reads a `StructuredBuffer` with explicit bounds, and was proven with 24 000 swarm panels in 3 draws. Use it for all four populations: about 6-10 draws, under 0.5 M triangles, almost no CPU per frame. Abyss `InstancedBatch` (14.7 H2) still works for signs if per-instance values are derived from a hash of the instance position.
5. **Forward+.**
   - Limits: 256 visible lights per camera on desktop, main light included (`UniversalRenderer.cs:656-659`, `ShaderConfig.cs.hlsl:12`). The per-object limit is ignored.
   - Z-bins are **logarithmic**: with near 0.05 and far 12 000 m there are about 1 365 bins, each 0.9 % of depth wide, so the 12 km far plane does not degrade clustering. Tiles are 16 px at 1080p and 32 px at 4K.
   - **Additional Lights must not be "Disabled"** in the asset, or the clusters hold zero local lights (`ForwardLights.cs:298`).
   - 32 unshadowed point lights cost well under 0.2 ms at 4K.
6. **UI Toolkit.** Use an editor-created `PanelSettings` asset with a generated `unity-theme://default` `.tss`, ScaleWithScreenSize 1920x1080, match height, and `LegacyRuntime.ttf` through `FontDefinition.FromFont`. The Ring proved this font path in a 6000.6 player log. `letter-spacing` accepts px or %, not em. **The hint string needs `WhiteSpace.Pre`**, or its runs of spaces collapse.
7. **Template traps to fix in PipelineConfigurator:** GPU Resident Drawer on, opaque texture on, depth texture on, LOD cross-fade on, shadows on, MSAA off, HDR 32-bit, and Windows APIs `[D3D11, D3D12]` (the project must be D3D11 only).

---

## 1. Render Graph full-screen pass after transparents, before ThreadGrade

### 1.1 Findings

**Frame order (`URP/Runtime/UniversalRendererRenderGraph.cs`).**
- Transparent forward pass: `:1318-1324`, writing `activeColorTexture` and `activeDepthTexture`.
- Then the depth copy if it is scheduled AfterTransparents, or `AfterRenderingTransparents` custom passes: `:1327-1330`.
- Then `BeforeRenderingPostProcessing` custom passes: `:1381`.
- Then URP post (off for us), then `AfterRenderingPostProcessing` custom passes: `:1464-1465`. They are recorded even with URP post off; Thread R1 §1.2 covers this.
- ThreadGrade runs at `AfterRenderingPostProcessing` (Thread `RT/Rendering/ThreadGradePass.cs:93`).
- Enum values: `AfterRenderingTransparents = 500`, `BeforeRenderingPostProcessing = 550`, `AfterRenderingPostProcessing = 600` (`URP/Runtime/Passes/ScriptableRenderPass.cs:148-158`).
- Passes are ordered by a stable insertion sort on the event (`URP/Runtime/ScriptableRenderer.cs:956, 1236-1249`). Same-event passes keep the renderer-feature list order.

**Camera colour under MSAA.**
- The single-camera intermediate colour is created with `bindTextureMS = useTileOnlyMode && msaa` (`:1737`), which is false on desktop.
- When a pass reads it with `UseTexture`, the native pass compiler gives the last writer a `Resolve` store action. If a later pass also uses it as an attachment, the store action is `StoreAndResolve` (`CORE/Runtime/RenderGraph/Compiler/NativePassCompiler.cs:1544-1605`).
- An **unsafe** reader always forces store and resolve (`:1551-1558`).
- The resolve is a box average in linear HDR. The resolved scene colour **is** available before bloom, so the "run after ThreadGrade on the 8-bit image" fallback in 14.7 H5 is not needed.

**Camera depth under MSAA.**
- The depth attachment is created with `desc.bindTextureMS = hasMSAA && RenderingUtils.ShouldDepthAttachmentBindMS()` (`:1781`).
- `ShouldDepthAttachmentBindMS()` returns `!MultisampleDepthResolveSupported() && supportsMultisampledTextures` (`URP/Runtime/RenderingUtils.cs:334-359`). `MultisampleDepthResolveSupported()` returns `SystemInfo.supportsMultisampleResolveDepth && ...ResolveStencil`.
- D3D11 has no depth resolve, so bindMS is expected to be true on D3D11. This is UNVERIFIED at runtime: log it once (§5.6).
- `CopyDepthPass.Render` (`URP/Runtime/Passes/CopyDepthPass.cs:175-190`):
  - With bindMS, it sets `_DEPTH_MSAA_4` and the shader loops over the samples with `DEPTH_OP` (`URP/Shaders/Utils/CopyDepthPass.hlsl:35-54`).
  - **`#if UNITY_REVERSED_Z: DEPTH_DEFAULT_VALUE 1.0, DEPTH_OP min`.** Reversed Z puts far at 0, so `min` keeps the **farthest** sample.
  - The destination `_CameraDepthTexture` is `R32_SFloat` (a colour format) and has no MSAA (`:1817-1837`).
- Consequence: if any of a pixel's 4 samples sees sky, the depth texture reports sky for that pixel. A depth-texture test then counts every pixel at the edge of a rail, cable or lamp post as unoccluded.

**Depth-copy schedule.**
- The copy is placed as late as possible, based on the earliest event that reads depth (`CalculateDepthCopySchedule`, `:946-978`).
- A custom pass with `ConfigureInput(ScriptableRenderPassInput.Depth)` at event ≥ 500 gets `AfterTransparents` (`URP/Runtime/UniversalRenderer.cs:699-725`).
- The renderer's `copyDepthMode` (enum `AfterOpaques=0, AfterTransparents=1, ForcePrepass=2`, `URP/Runtime/UniversalRendererData.cs:16-24`) **applies only when the camera/asset "Depth Texture" flag is on** (`UniversalRenderer.cs:729-744`).
- Nocturne has no depth-writing transparents, so AfterOpaques and AfterTransparents give the same content.
- The template has `m_RequireDepthTexture: 1` and `m_CopyDepthMode: 0`. That costs an MSAA depth copy every frame: about 0.5 ms at 4K (reads 4 x 8 B x 8.3 M samples).

**Builder rules (`CORE/Runtime/RenderGraph/RenderGraphBuilders.cs`).**
- A texture cannot be both `UseTexture`d and an attachment in one pass (`:366-369`, `:455-476`).
- `SetRenderAttachment` on a depth format, or `SetRenderAttachmentDepth` on a colour format, throws (`:479-494`).
- All attachments of a pass must share size and sample count, otherwise `k_AttachmentsDoNotMatch` (`NativePassCompiler.cs:1739-1740`).
- `AccessFlags.Write` keeps the load action; `WriteAll = Write|Discard` drops it; `ReadWrite` is needed for blending (`CORE/Runtime/RenderGraph/RenderGraph.cs:31-50`).
- The 6000.x `PropagateAttachmentOrientation` UV check is only active for the on-tile renderer. Desktop uses `BottomLeft` (`URP/Runtime/UniversalRenderPipelineRenderGraph.cs:11-12`), so it does not affect us.

**Raster vs unsafe pass.**
- Raster passes (`AddRasterRenderPass`, `RenderGraph.cs:1061`) allow one attachment set and a `RasterCommandBuffer`. They can merge and get exact load/store actions.
- Unsafe passes give a full `CommandBuffer` but break merges and force StoreAndResolve on MSAA inputs.
- `RenderGraphUtils.AddBlitPass` records an unsafe pass (Thread R1 §1.7).
- Use raster passes only.

**D3D11 MSAA semantics.**
- "A pixel shader is invoked once per covered pixel while a depth/stencil test occurs for each covered multisample. Samples that pass the depth/stencil test are updated with the pixel shader output color."
- `SV_Depth` "replaces the interpolated depth value for depth testing" (Microsoft, Pixel Shader Stage).
- So one `SV_Depth` per pixel is tested against each sample's own stored depth. This is the "per-sample" composite of Part 8.6, done by the hardware.

**Supporting facts.**
- `ZTest LEqual` in ShaderLab works unchanged under reversed Z: URP's own Lit passes declare `ZTest LEqual` (`URP/Shaders/Lit.shader:204, 256`) and render correctly on D3D.
- `_ZBufferParams` for reversed Z is `x = -1 + f/n, y = 1, z = x/f, w = 1/f` (`URP/ShaderLibrary/UnityInput.hlsl:62-75`), and `LinearEyeDepth(d) = 1/(z d + w)` (`CORE/ShaderLibrary/Common.hlsl:1206-1209`).

### 1.2 Recommendation for NocRain ("approach A": hardware per-sample occlusion)

Feature `NocRainFeature` goes in `PC_Renderer` **before** `ThreadGradeFeature` in the feature list, with `renderPassEvent = BeforeRenderingPostProcessing` and `requiresIntermediateTexture = true`. It records two raster passes:

1. **`NocRainLight`** (Lloc):
   - `UseTexture(resourceData.cameraColor, Read)`; this resolves the MSAA colour.
   - Writes a new 1/8-resolution texture: `R16_SFloat` or `RGBA16F`, Bilinear, Clamp, `msaaSamples None`. Set filter and wrap explicitly, because `TextureDesc(w,h)` defaults to Point and Repeat (Thread R1 pitfall 2).
   - Use 4 bilinear taps per output texel (an 8x8 box), plus an optional 3x3 blur pass at 1/8 resolution.
2. **`NocRain`**:
   - `SetRenderAttachment(resourceData.cameraColor, 0, AccessFlags.ReadWrite)` and `SetRenderAttachmentDepth(resourceData.activeDepthTexture, AccessFlags.Read)`. Both are MSAA 4, so the attachments match.
   - `UseTexture(lightTex)`.
   - One `DrawProcedural(3 vertices)` per layer (z = 3, 6, 12, 20 m, plus an optional far veil), with `ZTest LEqual`, `ZWrite Off`, `Cull Off` and premultiplied `Blend One OneMinusSrcAlpha`.

No `_CameraDepthTexture` is needed, so turn the asset Depth Texture off unless another effect needs it (§5).

```csharp
// NocRainPass.RecordRenderGraph (sketch; same allocation-free pattern as ThreadGradePass: static lambdas, pooled PassData,
// one MPB copied per draw, names/IDs cached)
public override void RecordRenderGraph(RenderGraph rg, ContextContainer frameData)
{
    var res = frameData.Get<UniversalResourceData>();
    var cam = frameData.Get<UniversalCameraData>();
    if (res.isActiveTargetBackBuffer || settings.rho <= 0f) return;
    var t = cam.cameraType; if (t == CameraType.Preview || t == CameraType.Reflection) return;

    TextureHandle color = res.cameraColor, depth = res.activeDepthTexture;
    var cd = rg.GetTextureDesc(color);
    var ld = new TextureDesc(Mathf.Max(1, cd.width / 8), Mathf.Max(1, cd.height / 8))
    { format = GraphicsFormat.R16G16B16A16_SFloat, filterMode = FilterMode.Bilinear, wrapMode = TextureWrapMode.Clamp,
      msaaSamples = MSAASamples.None, clearBuffer = false, name = "_NocRainLight" };
    TextureHandle light = rg.CreateTexture(ld);

    using (var b = rg.AddRasterRenderPass<LightData>("NocRainLight", out var d, s_LightSampler))
    {   d.src = color; d.mat = m_Mat; d.mpb = m_Mpb;
        b.UseTexture(color, AccessFlags.Read);                 // MSAA colour resolved here (NativePassCompiler.cs:1574-1597)
        b.SetRenderAttachment(light, 0, AccessFlags.WriteAll);
        b.SetRenderFunc(static (LightData d, RasterGraphContext c) => DrawLight(d, c)); }

    // camera-relative GPU matrices; the intermediate RT is rendered with the "into texture" projection on D3D
    Matrix4x4 view = cam.GetViewMatrix(); view.SetColumn(3, new Vector4(0, 0, 0, 1));       // drop translation
    Matrix4x4 proj = GL.GetGPUProjectionMatrix(cam.GetProjectionMatrix(), true);
    Matrix4x4 vp = proj * view, ivp = vp.inverse;

    using (var b = rg.AddRasterRenderPass<RainData>("NocRain", out var d, s_RainSampler))
    {   d.light = light; d.vp = vp; d.ivp = ivp; d.p = layerParams; d.mat = m_Mat; d.mpb = m_Mpb;
        b.UseTexture(light, AccessFlags.Read);
        b.SetRenderAttachment(color, 0, AccessFlags.ReadWrite);      // blend into MSAA scene colour
        b.SetRenderAttachmentDepth(depth, AccessFlags.Read);         // per-sample depth test, no writes
        b.SetRenderFunc(static (RainData d, RasterGraphContext c) => DrawLayers(d, c)); }   // 4 DrawProcedural, 3 verts each
}
```

```hlsl
// Hidden/Noc/Rain, pass "RainLayer": ZTest LEqual  ZWrite Off  Cull Off  Blend One OneMinusSrcAlpha
float4x4 _NocRainVP, _NocRainIVP;      // camera-relative (translation removed), GPU flip applied
float4   _NocRainLayer;                // x radius z (m), y t_r (s), z rho_eff, w tilt (rad)
struct V2F { float4 positionCS : SV_POSITION; float2 ndc : TEXCOORD0; };
V2F VertRain(uint id : SV_VertexID)
{   float2 p = float2((id << 1) & 2, id & 2) * 2.0 - 1.0;   // full-screen triangle (-1,-1) (3,-1) (-1,3)
    V2F o; o.positionCS = float4(p, 0.5, 1.0); o.ndc = p; return o; }
half4 FragRain(V2F i, out float depthOut : SV_Depth) : SV_Target
{
    float4 h = mul(_NocRainIVP, float4(i.ndc, 0.5, 1.0));
    float3 d = normalize(h.xyz / h.w);                        // world-space ray direction (camera at origin)
    float  c = max(length(d.xz), 1e-4);                       // cos(elevation)
    float  t = _NocRainLayer.x / c;                           // distance to the camera-centred cylinder (H5)
    // ... arc = z atan2(d.x, d.z); q = z d.y / c + 9 t_r; cells 0.30 z x 0.80 z; capsule alpha a; colour from Lloc ...
    half a = RainAlpha(d, _NocRainLayer);                     // analytic, footprint-filtered (>= 2.2 px wide)
    clip(a - 1.0 / 1024.0);                                   // empty pixels cost no ROP/blend work
    float4 cs = mul(_NocRainVP, float4(d * t, 1.0));
    depthOut = cs.z / cs.w;                                   // reversed-Z on D3D; same projection as the depth buffer
    half3 rad = RainRadiance(i, a);                           // 0.35 (0.04 + 0.9 clamp(Lloc/0.8, 0, 1.5)) x tint
    return half4(rad * a, a);                                 // premultiplied
}
```

Pitfalls:
- The view-direction reconstruction and `SV_Depth` must use **the same** GPU projection as the scene, built with `renderIntoTexture = true` because the pass only draws into the intermediate RT. A mismatch shifts rain occlusion by one depth step or flips rain vertically. The test below catches it.
- `SV_Depth` disables early-Z. The cost of four full-screen layers at 4K is about 33 M invocations x 60-100 ALU, roughly 0.1-0.3 ms on an RTX 5070 (estimate).
- `clip` keeps blending to the roughly 1 % of pixels that hold drops.
- Rain lands on top of `NocGlass` and the koi. Both are ZWrite Off and rain is drawn after them, which is correct for the koi (always 200 m or more away) and nearly invisible for tube glass (alpha 0.12-0.7).
- Do **not** call `CreateTexture(cameraColor)` for outputs, because it inherits MSAA (Thread R1 pitfall 3).

**Alternative "B" (cheaper, approximate, API-dependent).** Run after the resolve:
- `UseTexture(cameraColor)` (the resolved colour) and `UseTexture(activeDepthTexture)` as `Texture2DMS<float,4>` (needs bindMS true).
- Per pixel, compute the drop once, count the samples whose own depth lies behind the layer (`f = n/4`), and output `Cres (1 - a f) + D a f` into a new non-MSAA RGBA16F texture, then set `resourceData.cameraColor` to it.
- The edge error is `0.25 a (S - R)`, which is invisible, and the frame does one resolve fewer.
- It breaks if `bindTextureMS` is false (a D3D12 or Vulkan depth resolve), so assert `rg.GetTextureDesc(depth).bindTextureMS` and fall back to A.

**Rejected "C":** testing against `_CameraDepthTexture`, because it holds the farthest sample (rain over rail edges).

### 1.3 Tests
- **T-RAIN-EDGE (Part 8.6):**
  - Setup: static camera `-freeze-rain`, rho 1, a `NocLine` rail of 1.75 px at 1.5-2 m in front of the 3 m layer.
  - Measure the rail's interior pixels: rain-on minus rain-off over 120 frames must be ≤ 0.5 level.
  - Measure edge pixels: the added energy must be ≤ the uncovered sample fraction x the streak energy, and the edge must never get brighter than its unoccluded neighbour.
- **Orientation:** at pitch +60 and yaw 90, streaks are vertical in world space within 1°, and window and RT captures agree (Part 11.2 parity).
- **Render Graph Viewer, once in the Editor:** `DrawTransparentObjects` stores `StoreAndResolve` on `_CameraTargetAttachment`, NocRain merges with nothing, and ThreadGrade reads the resolved colour.
- **Player log:** one `[NocDepth]` line with the bindMS and resolve facts (§5.6).

---

## 2. Instancing: RenderMeshInstanced, RenderMeshIndirect/Primitives, BRG, SRP Batcher

### 2.1 Findings

**`RenderMeshInstanced`.**
- Signatures with `T[]`, `List<T>` and `NativeArray<T>`, and `instanceCount`/`startInstance` (`XML-CORE:16147-16178`).
- 6000.6 docs: "You can only render a maximum of 1023 instances at once"; with objectToWorld plus worldToObject "you can render a maximum of 511 instances at once". `#pragma instancing_options assumeuniformscaling` removes worldToObject.
- It throws unless `material.enableInstancing` is set.
- The per-instance struct may carry only `objectToWorld`, `renderingLayerMask` and `prevObjectToWorld`. "The instanced rendering ignores any other members." So custom fields never reach the shader.
- Bounds are computed automatically unless `RenderParams.worldBounds` is set. They are used to "cull and sort all the instances ... as a single entity".
- Thread R3 §1.2 (IL) showed there is **no managed count check**, so the cap is silent.
- Abyss `InstancedBatch` (`RT/Rendering/InstancedBatch.cs`) chunks at 511, inflates bounds 10 %, carries no per-instance data except the matrix, and has one MPB per batch (fade only).

**`RenderMeshIndirect`, `RenderMeshPrimitives`, `RenderPrimitives`** (`XML-CORE:16137-16230`).
- No documented per-call instance cap.
- `RenderMeshIndirect` needs an `IndirectArguments` buffer. `RenderMeshPrimitives(rp, mesh, submesh, instanceCount)` needs none.
- Instance data comes from your own buffer. `unity_ObjectToWorld` is not meaningful for these draws (The Ring `InstancedField.cs` contract; the 6000.6 docs example uses its own `_ObjectToWorld`).
- `CGINC/UnityIndirect.cginc`: `GetIndirectInstanceID(svInstanceID)` returns `svInstanceID` on D3D. D3D's `SV_InstanceID` excludes `startInstance`; Vulkan's includes it (`:28-34`, `:64-70`).
- `worldBounds` "Defines world space bounds for the geometry. Used to cull and sort" (`XML-CORE:52532-52536`). It is never auto-computed for procedural draws.

**Per-instance data versus the SRP Batcher.**
- In non-instanced variants, `UNITY_INSTANCING_BUFFER_START(buf)` expands to `CBUFFER_START(buf)` (`CORE/ShaderLibrary/UnityInstancing.hlsl:491-498`).
- A material property in any cbuffer other than `UnityPerMaterial` makes the shader SRP-Batcher-incompatible (Unity manual, SRP Batcher compatibility).
- So `UNITY_INSTANCING_BUFFER` per-instance props break SRP batching for every MeshRenderer that shares the shader.
- In the instanced variant `unity_InstanceID = SV_InstanceID + unity_BaseInstanceID` (`UnityInstancing.hlsl:88-96, 189-196`). On D3D11 the per-instance arrays use flexible sizes (`:16-19, :260-261`).
- A `StructuredBuffer` declared outside `UnityPerMaterial` is neutral for the SRP Batcher (The Ring `RingSwarmPanel.shader:65-91`).

**BRG / GPU Resident Drawer.**
- Requirements: a raw-buffer platform, BRG variants Keep All, the `OccluderDepthDownscale` kernel (`CORE/Runtime/GPUDriven/GPUResidentDrawer.Validator.cs:44-64`), and `DOTS_INSTANCING_ON` variants in every shader.
- The template has `m_GPUResidentDrawerMode: 1` (`Assets/Settings/PC_RPAsset.asset:85`) and `m_BrgStripping: 2` (`ProjectSettings/GraphicsSettings.asset:49`).
- Thread decided GRD off. BRG gives Nocturne nothing beyond what procedural draws give: no per-instance culling is needed at these counts, and it brings DOTS variants plus longer builds.

**Engine instancing-variant stripping.** `m_InstancingStripping: 0` ("Strip Unused", `GraphicsSettings.asset:48`) keeps `INSTANCING_ON` only for **material assets** with Enable Instancing (The Ring 09 §2.5). RenderMeshInstanced paths therefore need a saved material asset with `enableInstancing = true`. Procedural draws need no instancing variant.

**The Ring's `InstancedField` is a tested library for exactly this** (`2nd .../Assets/_Game/Scripts/Runtime/Rendering/InstancedField.cs:8-45`):
- `RenderMeshPrimitives` (Primitives mode) or `RenderMeshIndirect` (Indirect mode).
- A `StructuredBuffer` bound through the field's MPB, explicit bend-inflated bounds, and zero per-frame allocation.
- **Capture contract:** in capture mode, `SubmitAllFor(cam)` runs before each manual render (lesson 3.2.8).
- Its shader contract: no `multi_compile_instancing`; the vertex hook writes world position and normal itself; `#pragma target 4.5`.
- Proven at 24 000 instances (`RingSwarmPanel.shader:1-20`, 3 indirect draws; player log line 75 "3 indirect draws ready").

### 2.2 Cost for Nocturne's populations (estimates; measure with PerfMonitor)

| Population | Count | Path recommended | Draws | Triangles | Per-frame CPU | GPU notes |
|---|---|---|---|---|---|---|
| Signs `NocNeon` (opaque) | 24 000 | InstancedField Primitives, 1 field per region (boulevard 12 000, alley 300, Gulf/bridges 7 000, plaza/promenade 1 500, ring/spire 3 200) | 5 | quad 48 k (thin box 288 k) | ~5 µs per draw | VS 96-576 k vertices; PS ~60-120 ALU on ≤12 % of the frame; < 0.1 ms at 4K |
| Vehicle streaks `NocStreak` (additive, 3010) | 3 900 (1 720 + 2 200) | Primitives, procedural quad (6 verts), position from (lane, phi, v) and wrapped time in the VS | 2 (boulevard, layers) | 7.8 k | none (static buffer) | negligible; overlap near vanishing points is additive and cheap |
| Garden lanterns | 8 840 x 20 tris | Primitives with the icosahedron mesh, wind sway = the same function as `NocFoliage` | 1-2 | 177 k | none | negligible |
| Fireflies | 600 | Primitives quad, orbit and drift in the VS | 1 | 1.2 k | none | negligible |
| Same with `RenderMeshInstanced` | 37 340 | InstancedBatch, 511 per call | ≥ 74 (signs ≥ 47 before spatial chunking; vehicles need CPU matrix rewrites) | same | matrix copy and native worldToObject per instance; order of 0.3-1 ms (UNVERIFIED) | same |

Total: under 0.5 M triangles (2.5 % of the 20 M budget) and about 10 draws.

### 2.3 Recommendation

- Copy The Ring's `InstancedField` (+ `InstancedFieldDriver`) as `NocInstancedField`. One `StructuredBuffer` struct per population, for example `struct SignData { float4 posW; float4 axisU_w; float4 axisV_h; uint4 colorSeedFlags; }`, 64 B.
- The vertex shader does the H2 footprint logic: project the sign, compute its pixel size, inflate the quad to at least 2.2 px and pass `energy = (true/drawn)^2`. This is exactly what a procedural draw makes easy.
- **Bounds:**
  - Union of the instance AABBs plus motion range.
  - Lanes: polyline AABB plus streak length.
  - Lanterns: crown AABB plus 1 m sway.
  - Fireflies: garden box plus orbit.
  - Signs: AABB plus 1 m.
  - One field per region so `BeatDirector` can toggle `Visible` and frustum culling still works.
- **Time for H7:** `t` wrapped by a double modulo on the CPU (14.7 H7). Float error at `(v/L) t ≤ 50` cycles is 18 mm on a 4.8 km lane, which is negligible.
- If the lead keeps Abyss `InstancedBatch` for signs (14.7 H2 literal):
  - Derive colour and glyph seed from a PCG hash of `UNITY_MATRIX_M._m03_m13_m23`. Never `UNITY_INSTANCING_BUFFER`.
  - Keep chunks at 511 or fewer, with explicit bounds.
  - Save the material asset with instancing on.
- Shader rules for the procedural fields:
  - `#pragma target 4.5`, no `multi_compile_instancing`.
  - `CBUFFER_START(UnityPerMaterial)` with identical layout in all passes; `StructuredBuffer` outside it.
  - Opaque signs need `DepthOnly` only if a depth texture is used. Shadows are off, so the `ShadowCaster` pass is stripped anyway.

### 2.4 Tests
- Draw count and timing: a development player with `ProfilerRecorder` "Draw Calls Count" and "Main Thread"/GPU frame time at the KS04, KS09, KS15 and KS17 poses (Part 11.7).
- Zero allocations per frame (heap growth, `GC.CollectionCount`).
- **Bounds test:** for each field, sample 200 instances × 10 time values. Every projected vertex lies inside `worldBounds` (EditMode, C# mirror of the VS maths).
- **Capture test:** KS04 with fields present in RT captures (fields re-submitted per capture camera).

---

## 3. Forward+ in URP 17.6

### 3.1 Findings

**Light limits.**
- `maxVisibleAdditionalLights` is 256 on desktop D3D11. It is 32 on mobile, GL Core and WebGPU (`URP/Runtime/UniversalRenderPipeline.cs:220-232`; `universal-config/.../ShaderConfig.cs.hlsl:10-12`).
- Under the cluster loop the culling cap is that same number and **includes the main light**: "mainlight is treated as any other light" (`URP/Runtime/UniversalRenderer.cs:656-663`). Unity's docs give 256 per camera for desktop Forward+.
- `maxPerObjectLights` (8) and the asset's `m_AdditionalLightsPerObjectLimit` (template 4) are used only by Forward. Under `USE_CLUSTER_LIGHT_LOOP`, `LIGHT_LOOP_BEGIN(lightCount)` ignores `lightCount` and iterates the cluster (`URP/ShaderLibrary/RealtimeLights.hlsl:35-46`).

**Additional Lights must be PerPixel, not Disabled.** `CreateLightData` gives `supportsAdditionalLights = mode != Disabled` (`UniversalRenderPipeline.cs:2239-2251`), and the clustering job uses `localLightCount = supportsAdditionalLights ? lights.Length : 0` (`URP/Runtime/ForwardLights.cs:298`). The template has `m_AdditionalLightsRenderingMode: 1` (PerPixel; enum `Disabled 0, PerPixel 1, PerVertex 2`, `UniversalRenderPipelineAsset.cs:233-249`). Keep it.

**Clustering geometry** (`ForwardLights.cs:300-350`; `UniversalRenderPipeline.cs:250-253`):
- `itemsPerTile = local lights + reflection probes (≤ 64)`, `wordsPerTile = ceil(items/32)`.
- Tile width doubles from 8 px until `tilesX*tilesY*wordsPerTile ≤ maxTileWords = 16 384`. With 1 word: 16 px at 1920x1080 (8 160 tiles), 16 px at 1680x1050 and 2560x1080, 32 px at 3840x2160.
- **Z-bins are logarithmic** for perspective cameras: `zBinScale = 4096 / ((log2 f - log2 n)(2 + wordsPerTile))`. With n 0.05 and f 12 000 (17.87 octaves) and 1 word: 1 365 bins, 76 per octave, each **0.91 % of depth** (9 cm at 10 m, 0.9 m at 100 m). The 12 km far plane costs nothing. With 33-64 items: 1 024 bins, 1.2 %.
- The shader side `ClusterInit` uses `log2(viewZ) * scale + offset`, clamped to the last bin (`URP/ShaderLibrary/Clustering.hlsl:28-102`).
- Keywords: the runtime sets `_CLUSTER_LIGHT_LOOP` (and deprecated `_FORWARD_PLUS`), and disables `_ADDITIONAL_LIGHTS(_VERTEX)` under Forward+ (`ForwardLights.cs:575-579`).
- Desktop compiles with `MAX_VISIBLE_LIGHTS 256`, so `MAX_LIGHTS_PER_TILE > 32` and `ClusterNext` uses the header-driven word loop (`Clustering.hlsl:110-140`; `Input.hlsl:8-41`).
- `_ENVIRONMENTREFLECTIONS_OFF` makes `CLUSTER_HAS_REFLECTION_PROBES` 0 (`Clustering.hlsl:9`). The Ring's template defines it (`RingLitCore.hlsl:18`).

**Shadows off** (Nocturne has no shadow maps):
- With `supportsMainLightShadows` and `supportsAdditionalLightShadows` false, culling drops shadow casters (`UniversalRenderer.cs:645-652`).
- The prefilter removes `_MAIN_LIGHT_SHADOWS` and `_CASCADE` and keeps the OFF variant (`URP/Editor/ShaderBuildPreprocessor.cs:1220-1239`; `URP/Runtime/Data/UniversalRenderPipelineAssetPrefiltering.cs:62-70`).
- `ShadowCaster` passes are stripped (The Ring 09 §2.3).
- Keep the Lit-style `multi_compile _ _MAIN_LIGHT_SHADOWS ...` line: its `_` variant is the one that survives.

### 3.2 Recommendation (handwritten lit shader)

Use The Ring's template unchanged (`2nd .../Shaders/Include/RingLighting.hlsl:43-73`, `RingLitPass.hlsl:141-152`):

```hlsl
#pragma multi_compile _ _CLUSTER_LIGHT_LOOP            // mandatory even with one light (Thread R2 §1.3)
#pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN  // OFF survives
#define _ENVIRONMENTREFLECTIONS_OFF 1                    // before the URP includes
...
InputData inputData = (InputData)0;                     // MUST be named inputData (LIGHT_LOOP_BEGIN reads it)
inputData.positionWS = positionWS;                      // drives the z-bin
inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(i.positionCS);  // drives the tile; from SV_POSITION
...
#if USE_CLUSTER_LIGHT_LOOP
[loop] for (uint li = 0; li < min(URP_FP_DIRECTIONAL_LIGHTS_COUNT, MAX_VISIBLE_LIGHTS); li++) { /* extra directionals */ }
#endif
LIGHT_LOOP_BEGIN(pixelLightCount)
    Light L = GetAdditionalLight(lightIndex, inputData.positionWS, half4(1,1,1,1));
    color += NocLight(L, ...) * _NocLightCal;          // one LightCal (14.2.3)
LIGHT_LOOP_END
```

- Make the "sky fill" directional light the **main** light (the brightest directional, or `RenderSettings.sun`). Then the extra-directional loop runs zero times.
- The `BeatDirector` must `Light.enabled = false` every light whose multiplier is 0 (other beats). Lights at intensity 0 still occupy cluster bits and cost loop iterations.
- **Cost of 32 unshadowed point lights:**
  - Pixel side: `ClusterInit` (~10 ALU plus 2 cbuffer loads) plus about 50-80 ALU per light that overlaps the pixel's tile and z-bin.
  - The 14.2.3 kits overlap 1-5 lights per pixel (beat 6 wall lights r 45 are the worst): about 0.05-0.2 ms at 4K for the opaque pass.
  - CPU side: the Burst clustering jobs plus an upload of 16 KB z-bins and 64 KB tiles, well under 0.1 ms.
- Keep light ranges tight. Ranges set the cluster footprint, and URP's attenuation fades smoothly to 0 at the range, so there is no pop.

### 3.3 Tests
- The Part 8.6 test scene: main light plus 4 point lights, every `Megastructure/Noc*` lit shader lights identically, in the **player**.
- Player log: `Shader.IsKeywordEnabled(GlobalKeyword "_CLUSTER_LIGHT_LOOP")` is true. `Temp/shader-stripping.json` after the build shows only the `_CLUSTER_LIGHT_LOOP` variant.
- A light at its range limit shows no tile seams at 16 px or 32 px tile boundaries (static capture, 4K vs 1080p).

---

## 4. UI Toolkit at runtime (title, hint, pause, ending)

### 4.1 Findings
- Members in 6000.6:
  - `PanelSettings.scaleMode` enum `ConstantPixelSize | ConstantPhysicalSize | ScaleWithScreenSize` (`XML-UIE:16461-16475`).
  - `screenMatchMode` `Expand | MatchWidthOrHeight | Shrink` (`:16490-16505`).
  - `match`, `referenceResolution` (Vector2Int) and the `textSettings` field (`:16584, 16598, 16637, 16667`).
  - `LengthUnit` has only `Percent` and `Pixel` (`XML-UIE:13567-13575`), so `letter-spacing` is px or % (no em).
- **Whitespace:** `WhiteSpace.Normal` and `NoWrap` collapse "sequences of whitespace", while `Pre` and `PreWrap` preserve them (`XML-UIE:27033-27057`). The 14.11 hint `"WASD  move    Mouse  look ..."` needs `WhiteSpace.Pre` (or `\u00A0` runs).
- PanelSettings must be an **editor-created asset**. A runtime `CreateInstance` gets its UI shaders through `Shader.Find`, which fails in the player.
  - Generate the theme `.tss` with `@import url("unity-theme://default");`.
  - Add the UIR and TextCore shaders to Always Included.
  - Keep the UIDocument enabled.
  - Sources: The Ring 06 §1.2-1.3, The Ring DEVELOPMENT 178-179, and its working code `2nd .../Scripts/Editor/Batch/ProjectSetup.cs:217-247`: ScaleWithScreenSize 1920x1080, MatchWidthOrHeight, match height, `clearColor = false`, and a check of `m_DefaultShader`.
- **Default runtime font in 6000.6:**
  - The theme font is probably Inter (an `Inter-Regular SDF` asset is in the editor resources). That it is the theme font is UNVERIFIED.
  - What is **proven in a 6000.6 player** is `Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf")` through `FontDefinition.FromFont`. The Ring's player log reads `[Ring] UI font in use: LegacyRuntime (mode Legacy), title 353 x 75 px; glyphs warmed` (`2nd .../QA/autotest/at01/player.log:51`).
  - The Ring's `UiDirector` measures the title after each font change and swaps modes if it measures blank (`RT/UI/UiDirector.cs:921-940`).
  - The Ring's player ran on D3D12; the font path is API-independent.
- **The overlay UI is drawn by URP into the back buffer after FinalBlit** (`rendersOverlayUI = rendersUIOverlay && resolveToScreen`, `URP/Runtime/FrameData/UniversalCameraData.cs:384`; URP forces `rendersUIOverlay = true`, `UniversalRenderPipeline.cs:2912-2926`).
  - So text is **not** in RT captures (SubmitRenderRequest) and is **not dithered** by ThreadGrade.
  - Keep the picture fades inside ThreadGrade before the dither, as Thread's `FadeController` does (`GradeState.FadeBlack`, `3rd .../RT/UI/FadeController.cs:2-9`). Use UI only for text.

### 4.2 Recommendation
- Reuse The Ring's `ProjectSetup.CreateOrUpdateUi` + `UiDirector`/`UiTimeline` with `Noc` names and these values:
  - Reference 1920x1080, MatchWidthOrHeight, `match = 1` (height, which agrees with Hor+). Panel scale is 1.0 at 2560x1080, 0.972 at 1680x1050, 2.0 at 4K.
  - Title: 67 px, `letterSpacing = 0.14 * 67 = 9.38 px`, TitleText colour at alpha 0.92, centred at 33 % height.
  - Ending: 51 px with the same tracking ratio (7.14 px).
  - Hint: `whiteSpace = WhiteSpace.Pre`.
  - Font: `UiFontMode.Legacy` (LegacyRuntime) by default, keeping the theme-font fallback.
- Check centring with tracking: measure `MeasureTextSize` with and without spacing. If the box grows by n x spacing instead of (n-1) x spacing (the trailing advance is UNVERIFIED), add `paddingLeft = letterSpacing` to keep the optical centre.
- Always Included: `Hidden/Internal-UIRDefault`, `Hidden/Internal-UIRAtlasBlitCopy`, `Hidden/TextCore/Distance Field SSD`, `Hidden/Internal-GUITextureClipText`, `Hidden/TextCore/Sprite`, `Hidden/Internal-UIE-ColorConversionBlit` (The Ring 06 §1.3 table), plus every `Megastructure/Noc*` shader.

### 4.3 Tests
- Player window screenshots (not RT captures) of the title at 1080p, 4K and 2560x1080: glyph cap height and advance match the expected px within 1 px; the log line shows the font in use and the title size.
- The hint keeps its double and quadruple spaces: the measured width with `Pre` is larger than with `Normal`.
- Fade frames: no banding in the last 60 frames of the ending fade (fade inside ThreadGrade).

---

## 5. 6000.6 / URP 17.6 facts that can bite Nocturne (beyond what Thread and The Ring documented)

### 5.1 Same package, different usage
The URP and Core folders are the same content hashes as in Thread and The Ring. Nothing in their ADRs is invalidated: Render Graph only, `_CLUSTER_LIGHT_LOOP`, 64-bit HDR, Option B post, depth priming ignored under MSAA, `TextureDesc` defaults, `AllocateWritableMeshData`, GRD off. The new risks are in §1 (MSAA depth = farthest sample) and in the items below.

### 5.2 Depth format, reversed Z, far 12 000 m
- `D32_SFloat_S8_UInt` is the Windows default and can be forced with `UniversalRendererData.depthAttachmentFormat = DepthFormat.Depth_32_Stencil_8`. `_CameraDepthTexture` from a copy is `R32_SFloat` (The Ring 09 §1.1-1.2). D3D11 uses reversed Z.
- **Depth precision** with n = 0.05: stored depth d ≈ n/z, relative float spacing 2^-24..2^-23, so Δz ≈ (6..12)e-8 x z. That gives 0.06-0.12 mm at 1 km and 0.7-1.4 mm at 12 km. The spec's 0.06 and 0.72 mm are the best case; the worst case is 2x.
- **Vertex precision dominates at distance:** positions at 4-8 km have a float step of 0.49 mm (2^-11).
- Rules:
  - Keep any stacked surfaces (a sign on a facade, a trim on a slab) **≥ 5 cm apart** at every distance up to 8 km. That is about 50-100x the combined error.
  - Never rely on depth bias.
  - Compute the dome direction from the object-space vertex (§5.3), not as `positionWS - cameraPos` at 11 km.
- Forward+ z-bins are logarithmic, so far 12 000 does not degrade light clustering (§3.1).

### 5.3 Camera-following sky dome at R 11 000 m in the Background queue
- Geometry: the planar far plane cuts at view-z 12 000. The dome's maximum view-z is 11 000 cos θ ≤ 11 000. Tessellation chords lie inside the sphere. So **no far-plane cut** in any orientation.
- The farthest real geometry, the Gulf faces at z -6 500 seen from the goal, is 7.1 km horizontally and about 7.8 km including depth. It is inside both the dome and the far plane.
- **Pitfall 1: URP's skybox paints over a ZWrite-Off dome.** URP draws the skybox after opaques whenever `camera.clearFlags == Skybox` and a skybox material exists (`URP/Runtime/UniversalRendererRenderGraph.cs:1273-1278`). It depth-tests against a buffer the dome never wrote, so it covers the dome everywhere.
  - Set `camera.clearFlags = SolidColor` (Floor colour) and `RenderSettings.skybox = null` in the build script.
  - Assert both in `Validate`.
- **Pitfall 2: capture cameras.** Place the dome **in the vertex shader** at `_WorldSpaceCameraPos + dirOS * R`, as The Ring ADR-003 does. Then every capture camera gets the right dome with no transform lag. Give the renderer huge bounds (30 km), or draw it with `RenderParams.worldBounds`.
  - Pass `dirOS` (exact) to the fragment for `NocSkyColor`.
- **Optional optimisation (not a correctness issue):** in Background (1000) the dome shades every pixel before the opaques; URP opaque sorting is RenderQueue first. Alternative: queue 2499 (last opaque), `ZTest LEqual`, `ZWrite Off`, and `positionCS.z = 0` under `UNITY_REVERSED_Z` (else `= w`). Then only uncovered samples shade, as URP's own skybox does. This saves the sky shader cost (cloud noise and glow Gaussians) on the roughly 50-75 % of pixels that geometry covers. If the spec's Background is kept, also keep ZTest Always/LEqual, and the depth value does not matter.

### 5.4 Transparent queues 3000-3030 and ordering
- URP's transparent pass draws `RenderQueueRange.transparent` (2501-5000) with `SortingCriteria.CommonTransparent` (`URP/Runtime/Passes/DrawObjectsPass.cs:204`; `UniversalRenderer.cs:411`).
- CommonTransparent = SortingLayer, RenderQueue, BackToFront, OptimizeStateChanges, applied in that order (Unity scripting reference). **The queue strictly wins over distance**, so 3000 < 3010 < 3020 < 3030 is deterministic.
- `RenderMesh*` and `RenderPrimitives` draws join the same sort, each as one entity at its `worldBounds` centre.
- **Glass "back faces then front faces":** a two-pass shader is drawn per object in tag order SRPDefaultUnlit then UniversalForward (`DrawObjectsPass.cs:87`). With several tubes, the order interleaves per object. Use **two materials**: back faces at 3000 (`Cull Front`) and front faces at 3001 (`Cull Back`). Then every back face is drawn before every front face.
- **Additive streaks behind glass:** NocStreak at 3010 is drawn after glass. A streak seen through a tube is then added unattenuated (error α x streak, with α 0.12-0.7). Traffic seen from the bridge is almost always behind the tubes, so consider drawing the bridge-beat streak fields at 2990, before the glass. Keep 3010 elsewhere if 14.7 H6 is followed literally. The koi (3030) never appears behind glass, so its order is safe.
- NocRain (§1) runs after all queues, and ThreadGrade after it. This matches the 14.7 H6 order: sky, opaque, glass, streak, steam, koi, then rain, bloom, dither.

### 5.5 Template settings to change (current values → Nocturne)
`Assets/Settings/PC_RPAsset.asset`, `PC_Renderer.asset`, ProjectSettings:

| Setting (file:line) | Template | Nocturne | Why |
|---|---|---|---|
| `m_MSAA` (PC_RPAsset:28) | 1 | 4 | Part 8.1 |
| `m_HDRColorBufferPrecision` (:27) | 0 (32-bit) | 1 (64-bit) | lesson 3.2.5 |
| `m_GPUResidentDrawerMode` (:85) | 1 | 0 | Thread ADR; BRG adds nothing (§2.1) |
| `m_RequireOpaqueTexture` (:23) | 1 | 0 | avoids a per-frame resolve and copy (~0.3-0.6 ms at 4K); glass has no refraction |
| `m_RequireDepthTexture` (:22) | 1 | 0, unless steam soft particles need it (then CopyDepthMode AfterOpaques; note it holds the farthest sample) | MSAA depth copy ≈ 0.5 ms at 4K; NocRain A needs none |
| `m_MainLightShadowsSupported`, `m_AdditionalLightShadowsSupported` (:45, :49) | 1, 1 | 0, 0 | 14.2.3: no shadow maps; strips ShadowCaster passes, culls no casters |
| `m_EnableLODCrossFade` (:33) | 1 | 0 | no LODGroups; strips `LOD_FADE_CROSSFADE` |
| `m_AdditionalLightsRenderingMode` (:47) | 1 PerPixel | keep 1 | 0 would remove all local lights from Forward+ (§3.1) |
| `m_ReflectionProbeBlending/Atlas` (:54, :56) | 1, 1 | keep (harmless) | with Forward+ it sets `DisablePerObjectCulling` (UniversalRenderer.cs:638-643), which saves CPU; shaders define `_ENVIRONMENTREFLECTIONS_OFF` |
| `m_RenderingMode` (PC_Renderer:56) | 2 Forward+ | keep | |
| `m_DepthPrimingMode` (:57) | 0 | keep 0 | ignored under MSAA anyway |
| `depthAttachmentFormat` | Default | `Depth_32_Stencil_8` explicit | Part 3.2.5 |
| Windows graphics APIs (ProjectSettings.asset:542-544) | `0200000012000000` = [D3D11, D3D12], auto off | [D3D11] only | Part 8.1; The Ring's player actually ran D3D12 (its player.log:20) |
| `m_InstancingStripping` (GraphicsSettings:48) | 0 Strip Unused | keep, with saved instanced material assets if RenderMeshInstanced is used | §2.1 |
| Camera | n/a | `clearFlags = SolidColor`, `RenderSettings.skybox = null`, `renderPostProcessing = false`, `antialiasing = None`, `allowMSAA = true`, `allowHDR = true`, near 0.05, far 12 000 | §5.3, Thread R1 |

### 5.6 One-time runtime probe (development player, first frame)
Extend The Ring 09's `DepthFormatProbeFeature` (`09_depth_format_shader_build.md:77-98`) to log:
- `SystemInfo.graphicsDeviceType`, `usesReversedZBuffer`, `supportsMultisampleResolveDepth`, `supportsMultisampledTextures`
- `rg.GetRenderTargetInfo(activeDepthTexture)`: format, msaaSamples, bindMS
- `rg.GetTextureDesc(cameraColor)`: format and msaaSamples, expected `R16G16B16A16_SFloat` x4
- `Shader.IsKeywordEnabled("_CLUSTER_LIGHT_LOOP")`
- the Forward+ tile width and z-bin count, computed with the §3.1 formula

The QA gate greps this one line, `[NocProbe]`.

---

## 6. Decisions proposed to the lead

1. NocRain is approach A: two raster passes at `BeforeRenderingPostProcessing`, blending into MSAA `cameraColor`, with the `activeDepthTexture` depth attachment read-only and `SV_Depth` per layer. Per-sample occlusion by hardware; no depth texture; Lloc from a 1/8-resolution resolved-colour pre-pass. Never test rain against `_CameraDepthTexture`, which holds the farthest of the MSAA samples.
2. The asset Depth Texture and Opaque Texture go off; GPU Resident Drawer off; shadows off; LOD cross-fade off; MSAA 4; HDR 64-bit; depth `Depth_32_Stencil_8` explicit; Windows API D3D11 only.
3. Signs, vehicles, lanterns and fireflies use The Ring's `InstancedField` (StructuredBuffer, `RenderMeshPrimitives`, explicit bounds, per-capture-camera submission), one field per region. Abyss `InstancedBatch` is acceptable for signs only with hash-derived per-instance data and 511 or fewer per call. Never `UNITY_INSTANCING_BUFFER` in shaders shared with MeshRenderers.
4. Forward+ stays: Additional Lights PerPixel, `_CLUSTER_LIGHT_LOOP` + `LIGHT_LOOP_BEGIN` with a local `inputData`, the sky fill as the main light, and lights with a zero multiplier disabled. The 32-light budget is far below the 256 cap, and logarithmic z-bins make far 12 000 free.
5. The sky dome is placed in the vertex shader around `_WorldSpaceCameraPos`, with `clearFlags = SolidColor` and no `RenderSettings.skybox`. Queue 2499 with far-plane z is optional as an optimisation.
6. Glass uses two materials (3000 back faces, 3001 front faces); consider streaks before glass on the bridge.
7. UI uses The Ring's PanelSettings asset pipeline, LegacyRuntime font, `letter-spacing` 9.38 px, `WhiteSpace.Pre` for the hint, and fades inside ThreadGrade.
8. Every stacked surface is ≥ 5 cm apart. Log one `[NocProbe]` line in the player.

---

## 7. Sources

Local (ground truth): every `URP/`, `CORE/`, `XML-CORE`, `XML-UIE` and `CGINC/` reference above, plus these read-only files from earlier games:
- Thread `docs/research/R1_render_graph_grade.md`, `R2_urp_lit_shader.md`, `R3_meshes_instancing.md`, `DEVELOPMENT.md`, `Assets/_Game/Scripts/Runtime/Rendering/ThreadGradePass.cs`, `Assets/_Game/Scripts/Runtime/UI/FadeController.cs`
- The Ring `docs/research/01, 02, 03, 06, 09`, `DEVELOPMENT.md`, `Assets/_Game/Scripts/Runtime/Rendering/InstancedField.cs`, `Assets/_Game/Shaders/RingSwarmPanel.shader`, `Assets/_Game/Shaders/Include/RingLighting.hlsl`, `Assets/_Game/Scripts/Editor/Batch/ProjectSetup.cs`, `Assets/_Game/Scripts/Runtime/UI/UiDirector.cs`, `QA/autotest/at01/player.log`
- Abyss `Assets/_Game/Scripts/Runtime/Rendering/InstancedBatch.cs`

Web (all accessed 2026-11-13; treated as untrusted data):

| URL | Used for |
|---|---|
| https://learn.microsoft.com/en-us/windows/win32/direct3d11/pixel-shader-stage | D3D11 MSAA: PS once per pixel, depth test per sample, `SV_Depth` replaces interpolated depth (basis of approach A) |
| https://learn.microsoft.com/en-us/windows/win32/api/d3d11/nf-d3d11-id3d11devicecontext-resolvesubresource | D3D11 resolve API (format-typed resolves; no depth-resolve mode), supporting that D3D11 reports no depth resolve |
| https://docs.unity.com/en-us/engine/6000.3/script-reference/unityengine/graphics/rendermeshinstanced (also the 6000.6 path) | 1023/511 limit, `assumeuniformscaling`, `enableInstancing` exception, auto bounds, ignored custom struct members |
| https://docs.unity.com/en-us/engine/6000.6/script-reference/unityengine/graphics/rendermeshindirect | Indirect requirements, `UnityIndirect.cginc` macros, bounds used for cull and sort, no stated instance cap |
| https://docs.unity3d.com/Manual/SRPBatcher-Materials.html (6000.6) | SRP Batcher rule: all material properties in one `UnityPerMaterial` cbuffer |
| https://docs.unity.com/en-us/engine/6000.6/script-reference/unityengine/rendering/sortingcriteria and https://docs.unity3d.com/ScriptReference/Rendering.SortingCriteria.html | CommonTransparent = SortingLayer, RenderQueue, BackToFront, OptimizeStateChanges; flags applied in that order |
| https://docs.unity3d.com/Manual/urp/rendering-paths-comparison.html (6000.6) and https://docs-alpha.unity3d.com/Manual/urp/rendering-paths-introduction.html | Forward+ 256 lights per camera on desktop including the main light; 32 on mobile |
| https://docs.unity.com/en-us/engine/current/manual/materials-and-shaders/shaders/writing-custom/shader-writing/sl/writing-shader-writing-shader-programs-hlsl/sl-platform-differences | Reversed Z on DirectX (1 near, 0 far), `UNITY_REVERSED_Z` |
| https://docs.unity3d.com/ja/2022.3/ScriptReference/SystemInfo-supportsMultisampleResolveDepth.html | Meaning of `supportsMultisampleResolveDepth` (value on D3D11 to be logged at runtime) |
