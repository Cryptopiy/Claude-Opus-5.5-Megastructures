// ShaderIds: property ids of the global uniforms of Shaders/Include/BulwarkAtmosphere.hlsl and BwCommon.hlsl (the C# side
// of the shader contract) plus the per-material property names MaterialFactory writes. Skeleton of Thread's ShaderIds with
// Bulwark names. Rule (lesson 3.2 #5): a GLOBAL name never equals a material property name; globals carry _Bw, material
// properties do not (except _BwFade, which is a per-renderer property-block value, never a global).
using UnityEngine;

namespace Megastructure
{
    public static class ShaderIds
    {
        // ---------------------------------------------------------------- globals (AtmosphereDriver is the one writer)
        public static readonly int BwCam = Shader.PropertyToID("_BwCam");
        public static readonly int BwScreen = Shader.PropertyToID("_BwScreen");
        public static readonly int BwTime = Shader.PropertyToID("_BwTime");
        public static readonly int BwSunDir = Shader.PropertyToID("_BwSunDir");
        public static readonly int BwSunColor = Shader.PropertyToID("_BwSunColor");
        public static readonly int BwSkyZenith = Shader.PropertyToID("_BwSkyZenith");
        public static readonly int BwSkyHorizon = Shader.PropertyToID("_BwSkyHorizon");
        public static readonly int BwAmbSky = Shader.PropertyToID("_BwAmbSky");
        public static readonly int BwAmbMid = Shader.PropertyToID("_BwAmbMid");
        public static readonly int BwAmbGround = Shader.PropertyToID("_BwAmbGround");
        public static readonly int BwFog0 = Shader.PropertyToID("_BwFog0");
        public static readonly int BwFog1 = Shader.PropertyToID("_BwFog1");
        public static readonly int BwSunHole = Shader.PropertyToID("_BwSunHole");
        public static readonly int BwBeamFade = Shader.PropertyToID("_BwBeamFade");
        /// <summary>The grade's 64 x 64 blue-noise rank tile, bound globally for tier fades (D-08, BwCommon.hlsl).</summary>
        public static readonly int BwBlueNoise = Shader.PropertyToID("_BwBlueNoise");

        // ---------------------------------------------------------------- per-renderer / per-batch (property blocks)
        /// <summary>Tier fade of DetailTier / InstancedBatch: near = c, far = -(1 - c) (D-08).</summary>
        public static readonly int BwFade = Shader.PropertyToID("_BwFade");

        // ---------------------------------------------------------------- per-material names (MaterialFactory)
        public const string BaseColor = "_BaseColor";
        public const string WetColor = "_WetColor";
        public const string Wet = "_Wet";
        public const string Smoothness = "_Smoothness";
        public const string Metallic = "_Metallic";
        public const string Emission = "_Emission";
        public const string EmissionColor = "_EmissionColor";
        public const string SrcBlend = "_SrcBlend";
        public const string DstBlend = "_DstBlend";
        public const string ZWrite = "_ZWrite";
        public const string Cull = "_Cull";
        public const string AlphaToMask = "_AlphaToMask";
        public const string MinWidthPx = "_MinWidthPx";
        public const string LineWidth = "_LineWidth";
        public const string AlphaFloor = "_AlphaFloor";
        public const string FadeNear = "_FadeNear";
        public const string FadeFar = "_FadeFar";
    }
}
