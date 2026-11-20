// NocTexConstants: the sampling library of Look v2 (docs/TEXTURE_PLAN.md 1.2, 1.5, 1.6, 2.5; owner TP). Exported as NC_<Name>
// into Shaders/Generated/NocTexConstantsGen.hlsl (gen_hlsl.py / ShaderGen: `public const` literals only; every name starts with
// "Tex" so it never collides with the other constants classes). Read by Shaders/Include/NocTexturing.hlsl and its C# twin
// Scripts/Runtime/Rendering/NocTexturingMath.cs, the importer (Scripts/Editor/NocTextureImport.cs) and TextureTests.
namespace Megastructure
{
    public static class NocTexConstants
    {
        // ------------------------------------------------------------------ hex tiling (Mikkelsen 2022; TEXTURE_PLAN 1.6)
        /// <summary>Hexes per tile (the hex lattice period is tile / TexHexRate; tiling.py checks both periods).</summary>
        public const double TexHexRate = 1.25;
        /// <summary>Barycentric sharpness w^gamma and the luminance weighting lerp(1, Y, beta) of the colour taps.</summary>
        public const double TexHexGamma = 7.0, TexHexBeta = 0.6;
        /// <summary>S-curve contrast of the blend weights: r 0.70 for colour and smoothness, 0.5 (= off) for normals and height.</summary>
        public const double TexHexGainColour = 0.70, TexHexGainNormal = 0.5;
        /// <summary>Tap cut: w' = max(w - TexHexCut, 0) renormalised, so a tap under the cut is skipped and the weights stay
        /// continuous (a plain skip-and-renormalise would jump by up to the cut at the boundary).</summary>
        public const double TexHexCut = 0.02;
        /// <summary>Rotation strength of rotating hex sets (radians: the full +-pi of Mikkelsen's LoadRot2x2); offsets-only sets use 0.</summary>
        public const double TexHexRotate = 1.0;
        /// <summary>Height-aware hex (moss, ground): w_i' = w_i (h_i + TexHeightBias)^TexHeightPow, normalised, shared by every map.</summary>
        public const double TexHeightBias = 0.05, TexHeightPow = 8.0;

        // ------------------------------------------------------------------ module and panel shuffle (TEXTURE_PLAN 1.6, 2.5)
        /// <summary>Per-module tone +-TexModuleTone and smoothness +-TexModuleSmooth, faded to their means by
        /// smoothstep(TexModuleFadePx0, TexModuleFadePx1, module px).</summary>
        public const double TexModuleTone = 0.06, TexModuleSmooth = 0.04, TexModuleFadePx0 = 2.0, TexModuleFadePx1 = 4.0;
        /// <summary>Per-panel tone of the facade skins (+-12 %), faded by smoothstep(2, 4, panel px).</summary>
        public const double TexPanelTone = 0.12, TexPanelFadePx0 = 2.0, TexPanelFadePx1 = 4.0;

        // ------------------------------------------------------------------ triplanar and fade to the mean (TEXTURE_PLAN 1.6)
        /// <summary>Triplanar weights |n|^TexTriPow, normalised; a plane at or under TexTriCut is skipped (w' = max(w - cut, 0)).</summary>
        public const double TexTriPow = 4.0, TexTriCut = 0.01;
        /// <summary>w_tex = smoothstep(TexFadePx0, TexFadePx1, tile / fp_box): under it every map returns its last mip, the normal is flat.</summary>
        public const double TexFadePx0 = 2.0, TexFadePx1 = 4.0;
        /// <summary>SampleLevel of the last mip: any level at or above the mip count clamps to the 1 x 1 mip.</summary>
        public const double TexLastMip = 16.0;
        /// <summary>Micro detail (MicroArr, 0.5 m period): w_det = smoothstep(TexMicroPx0, TexMicroPx1, 0.5 / fp).</summary>
        public const double TexMicroTile = 0.5, TexMicroPx0 = 4.0, TexMicroPx1 = 8.0;

        // ------------------------------------------------------------------ import (TEXTURE_PLAN 1.5)
        /// <summary>Toksvig: alpha'^2 = alpha^2 + min(2 variance, TexToksvigCap^2), alpha = (1 - s)^2.</summary>
        public const double TexToksvigCap = 0.20;
        /// <summary>anisoLevel 16 for facade, paving, asphalt, deck and arrays; 8 for the rest; ForceEnable raises 8 to 9.</summary>
        public const int TexAnisoHigh = 16, TexAnisoLow = 8, TexAnisoForcedMin = 9;
        /// <summary>The imported last mip's linear mean per channel (albedo variation): 0.5 +- TexMeanTol.</summary>
        public const double TexMeanTol = 0.01;
        /// <summary>Texture VRAM: plan cap and hard cap (MiB; TEXTURE_PLAN 9).</summary>
        public const double TexVramCapMiB = 900.0, TexVramHardMiB = 2048.0;
    }
}
