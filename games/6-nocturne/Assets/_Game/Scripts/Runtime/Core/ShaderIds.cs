// ShaderIds: property ids of the global uniforms declared in Shaders/Include/NocCommon.hlsl, NocAtmosphere.hlsl,
// NocLighting.hlsl and NocMirror.hlsl (the C# side of the shader contract) plus the per-material property names used by
// MaterialFactory. Lesson 5: a global name never equals a material property name (globals are _Noc*, material properties are
// _Base*, _Emission*, _Albedo*, ... ; ProjectTests checks it). Palette colours never travel as globals: shaders use PAL_*.
using UnityEngine;

namespace Megastructure
{
    public static class ShaderIds
    {
        // globals (NocCommon.hlsl / NocAtmosphere.hlsl), written once per frame by the AtmosphereDriver
        /// <summary>x = scene time wrapped to 3600 s, y = rain time wrapped, z = s (m), w = beat index.</summary>
        public static readonly int Time = Shader.PropertyToID("_NocTime");
        /// <summary>x = regional sigma (1/m), y = alley haze extra (D-006), z = sheet strength, w = rain rho.</summary>
        public static readonly int Fog = Shader.PropertyToID("_NocFog");
        /// <summary>Haze sheets: x, y, z = sheet heights with drift (m), w = sheet width (m).</summary>
        public static readonly int FogSheets = Shader.PropertyToID("_NocFogSheets");
        /// <summary>H3 sky modulation: x, y, z = sheet elevations e_k (deg), w = amplitude (0.10).</summary>
        public static readonly int SheetE = Shader.PropertyToID("_NocSheetE");
        /// <summary>Cloud noise scroll: xy octave 1 (m, wrapped), zw octave 2.</summary>
        public static readonly int Cloud = Shader.PropertyToID("_NocCloud");
        /// <summary>Cloud glow centres: segments (ax, az, bx, bz), parameters (amplitude, sigma, ring radius, 0), count.</summary>
        public static readonly int GlowSeg = Shader.PropertyToID("_NocGlowSeg");
        public static readonly int GlowPar = Shader.PropertyToID("_NocGlowPar");
        public static readonly int GlowCount = Shader.PropertyToID("_NocGlowCount");
        /// <summary>Ambient trilight (linear, x the beat multiplier; LightCal is applied in the shader): sky, equator, ground.</summary>
        public static readonly int AmbSky = Shader.PropertyToID("_NocAmbSky");
        public static readonly int AmbEquator = Shader.PropertyToID("_NocAmbEquator");
        public static readonly int AmbGround = Shader.PropertyToID("_NocAmbGround");
        /// <summary>Linear floor colour (NocFloor), w = LightCal.</summary>
        public static readonly int Floor = Shader.PropertyToID("_NocFloor");
        /// <summary>x = render height (px), y = minimum line width (px), z = 1 / height, w = 0.</summary>
        public static readonly int Screen = Shader.PropertyToID("_NocScreen");
        /// <summary>Analytic koi light strength (14.2.3; 0 outside beat 7) and its 12 spine points (xyz, w unused).</summary>
        public static readonly int KoiK = Shader.PropertyToID("_NocKoiK");
        public static readonly int KoiSpine = Shader.PropertyToID("_NocKoiSpine");
        /// <summary>NocFacade (14.7 H2): x = epoch phase t / 150 (double-wrapped mod 65 536), y = warm fraction, z = scene time.</summary>
        public static readonly int Facade = Shader.PropertyToID("_NocFacade");
        /// <summary>D-034 5.1 darkness wave (NocDarkWave): A = (cx, cz, cos yaw, sin yaw), B = (hx, hz, front y, ramp m), C.x = amount.</summary>
        public static readonly int DarkWaveA = Shader.PropertyToID("_NocDarkWaveA");
        public static readonly int DarkWaveB = Shader.PropertyToID("_NocDarkWaveB");
        public static readonly int DarkWaveC = Shader.PropertyToID("_NocDarkWaveC");
        /// <summary>NocStreak (14.7 H7): x = traffic time (double-wrapped mod 65 536 s), y = scene time wrapped mod 3 600 s.</summary>
        public static readonly int Traffic = Shader.PropertyToID("_NocTraffic");
        /// <summary>Instanced-batch fade (detail tiers; Abyss _AbFade).</summary>
        public static readonly int Fade = Shader.PropertyToID("_NocFade");

        // Look v2 (TEXTURE_PLAN 1.6, 7.1, 7.4, 7.6; NocCommon.hlsl), written once at start-up by NocLookGlobals; 0 (unset) = the default
        /// <summary>-tex 0|1: -1 = texture detail off, +1 = on (0, never written, reads as on).</summary>
        public static readonly int TexDetail = Shader.PropertyToID("_NocTexDetail");
        /// <summary>-debug-view N (TEXTURE_PLAN 7.1): 0 = off, 1..10 = the debug view.</summary>
        public static readonly int DebugView = Shader.PropertyToID("_NocDebugView");
        /// <summary>Feature flags -facade-v / -surf-v / -foliage-v: 2 = the Look v2 path, 1 (or 0) = v1.</summary>
        public static readonly int FoliageV = Shader.PropertyToID("_NocFoliageV");
        /// <summary>ShaderParityTests (TEXTURE_PLAN 7.4): 1 = NocFog and NocFloor skipped.</summary>
        public static readonly int Parity = Shader.PropertyToID("_NocParity");

        // wet floor (NocMirror.hlsl, 14.7 H4): written by NocWetRegistry.Upload through the AtmosphereDriver
        /// <summary>Sources: P0 (xyz, w = glow half-width rho), P1 (xyz), L (rgb = L_eff x crossfade), count.</summary>
        public static readonly int SrcP0 = Shader.PropertyToID("_NocSrcP0");
        public static readonly int SrcP1 = Shader.PropertyToID("_NocSrcP1");
        public static readonly int SrcL = Shader.PropertyToID("_NocSrcL");
        public static readonly int SrcCount = Shader.PropertyToID("_NocSrcCount");
        /// <summary>Wall planes: A (centre x, z, normal x, z), B (half length, top y, kind, weight), C (macro rgb, 0), count.</summary>
        public static readonly int WallA = Shader.PropertyToID("_NocWallA");
        public static readonly int WallB = Shader.PropertyToID("_NocWallB");
        public static readonly int WallC = Shader.PropertyToID("_NocWallC");
        public static readonly int WallCount = Shader.PropertyToID("_NocWallCount");

        // per-material (NocGraybox / NocLine / NocSky / NocSurface / NocWet / NocGlass)
        public const string BaseColor = "_BaseColor";
        public const string EmissionColor = "_EmissionColor";
        public const string Smoothness = "_Smoothness";
        public const string Metallic = "_Metallic";
        public const string MinWidthPx = "_MinWidthPx";
        public const string LineWidth = "_LineWidth";
        public const string SrcBlend = "_SrcBlend";
        public const string DstBlend = "_DstBlend";
        public const string ZWrite = "_ZWrite";
        public const string Cull = "_Cull";
        public const string AlbedoTex = "_AlbedoTex";
        public const string NormalTex = "_NormalTex";
        public const string MaskTex = "_MaskTex";
    }
}
