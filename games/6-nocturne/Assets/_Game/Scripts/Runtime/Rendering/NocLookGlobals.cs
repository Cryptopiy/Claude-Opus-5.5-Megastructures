// NocLookGlobals: the Look v2 globals of TEXTURE_PLAN 1.6, 7.1 and 7.6 (TP-0), read once from the command line and written as
// uniform shader globals (never keywords) before the first scene loads:
//   -tex 0|1            _NocTexDetail  (-1 off, +1 on; default on)
//   -debug-view N       _NocDebugView  (0 off; 1..10 the debug views of TEXTURE_PLAN 7.1; 9 = overdraw, NocDebugViewFeature)
//   -foliage-v 1|2      _NocFoliageV   (feature flag: 1 = the v1 foliage path, 2 = Look v2, the default; GD deletes v1 in wave B)
//   -facade-v, -surf-v  removed in TP-2 (FX, Phase 4 wave A): the v1 facade and surface paths are deleted, FacadeV and SurfV
//                       read 2 for the callers that still test them; passing either flag logs a warning and changes nothing
//   -debug-shaders a+b  the shader names (without "Megastructure/") whose renderers the overdraw view counts (default NocFacade)
//   -facade-prepass 0|1 NocFacadePrepassFeature: the NocFacade depth prepass (TEXTURE_PLAN 2.12; default DefaultFacadePrepass)
// _NocParity stays 0 in the player (ShaderParityTests set it). A global nobody wrote reads 0 in the shaders, and 0 always means
// the default there (NocCommon.hlsl), so editor previews and EditMode renders need no setup.
using System;
using UnityEngine;

namespace Megastructure
{
    public static class NocLookGlobals
    {
        /// <summary>Look v2 versions (TEXTURE_PLAN 7.6). Facade and surface: the only path since TP-2 (the v1 paths are deleted);
        /// foliage: the default of -foliage-v (v1 until wave B).</summary>
        public const int DefaultFacadeV = 2, DefaultSurfV = 2, DefaultFoliageV = 2;   // FO accepted 2026-11-16 02:45, SU 03:25, FA at INT 04:12
        public const int DebugViewMax = 10;
        /// <summary>Default of -facade-prepass: on. The depth prepass changes no picture (TP_r02 vs TP_r02p: 20 / 20 shots within 1
        /// code) and with FA's v2 facades it gave 453.5 fps against 394.4, 1 % low 136.6 against 78.7 (docs/logs/TP.md 01:45).</summary>
        public const bool DefaultFacadePrepass = true;
        /// <summary>Debug view 9 (TEXTURE_PLAN 7.1): overdraw of the chosen renderers, drawn by NocDebugViewFeature.</summary>
        public const int DebugViewOverdraw = 9;

        public static bool TexDetail { get; private set; } = true;
        public static int DebugView { get; private set; }
        /// <summary>Always 2 (TP-2): kept for the beat builders that still test it.</summary>
        public static int FacadeV => DefaultFacadeV;
        /// <summary>Always 2 (TP-2): kept for the beat builders that still test it.</summary>
        public static int SurfV => DefaultSurfV;
        public static int FoliageV { get; private set; } = DefaultFoliageV;
        public static bool FacadePrepass { get; private set; } = DefaultFacadePrepass;
        /// <summary>Flags deleted in TP-2: a warning if passed, no effect.</summary>
        public static readonly string[] RemovedFlags = { "-facade-v", "-surf-v" };
        /// <summary>Shader names (full, "Megastructure/..." added) whose renderers debug view 9 counts.</summary>
        public static string[] OverdrawShaders { get; private set; } = { "Megastructure/NocFacade" };

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Init()
        {
            Read();
            Apply();
            if (DebugView != 0 || !TexDetail || FoliageV != DefaultFoliageV || FacadePrepass != DefaultFacadePrepass)
                Debug.Log(FormattableString.Invariant($"[NOC] NocLookGlobals: tex {(TexDetail ? 1 : 0)} debug-view {DebugView} foliage-v {FoliageV} facade-prepass {(FacadePrepass ? 1 : 0)}"));
            foreach (string gone in RemovedFlags)
                if (CliArgs.Has(gone)) Debug.LogWarning("[NOC] NocLookGlobals: " + gone + " was removed in TP-2 (the v1 facade / surface paths are deleted); Look v2 is the only path");
        }

        static int Flag(string arg, int fallback)
        {
            int v = CliArgs.GetInt(arg, fallback);
            return v == 1 || v == 2 ? v : fallback;
        }

        /// <summary>Reads the flags from CliArgs (tests may call CliArgs.Override first).</summary>
        public static void Read()
        {
            TexDetail = CliArgs.GetInt("-tex", 1) != 0;
            int dv = CliArgs.GetInt("-debug-view", 0);
            DebugView = dv >= 0 && dv <= DebugViewMax ? dv : 0;
            FoliageV = Flag("-foliage-v", DefaultFoliageV);
            FacadePrepass = CliArgs.GetInt("-facade-prepass", DefaultFacadePrepass ? 1 : 0) != 0;
            string ds = CliArgs.Get("-debug-shaders");
            if (!string.IsNullOrEmpty(ds))
            {
                string[] parts = ds.Split(new[] { ';', '+' }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < parts.Length; i++)
                    parts[i] = parts[i].StartsWith("Megastructure/", StringComparison.Ordinal) ? parts[i] : "Megastructure/" + parts[i];
                if (parts.Length > 0) OverdrawShaders = parts;
            }
        }

        /// <summary>Writes the globals (idempotent).</summary>
        public static void Apply()
        {
            Shader.SetGlobalFloat(ShaderIds.TexDetail, TexDetail ? 1f : -1f);
            Shader.SetGlobalFloat(ShaderIds.DebugView, DebugView);
            Shader.SetGlobalFloat(ShaderIds.FoliageV, FoliageV);
            Shader.SetGlobalFloat(ShaderIds.Parity, 0f);
        }
    }
}
