# Shaders/Beats (beat builders' shaders, Phase 3+)

One file per shader, named `Megastructure/Noc<Name>` (the pattern of the existing `Megastructure/Noc*` shaders: one
`UnityPerMaterial` CBUFFER for the SRP Batcher, shared lighting, then `NocFog` and `NocFloor` last in every forward pass,
no shadow maps, ShadowCaster/DepthOnly/DepthNormals kept).

Nothing else to register: every shader whose name starts with `Megastructure/` anywhere under `Assets/_Game/Shaders`
is discovered from the files (`ShaderCatalog` in `Scripts/Editor/EditorCommon.cs`), added to Always Included Shaders by
`RebuildAll` and `BuildWindows`, and compiled per pass by `Validate`. Never edit ProjectSettings for a shader.
Each file here belongs to the builder who created it.
