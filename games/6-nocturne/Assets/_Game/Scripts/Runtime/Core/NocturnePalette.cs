// NocturnePalette: the only place a colour is typed (MASTER_PROMPT 14.2.1 as replaced by docs/LOOK_V2.md section 2, palette v2:
// a dark, near-neutral night at a 4 300 K white balance; violet and magenta survive only as small neon accents), from Thread's
// ThreadPalette.
// The editor build script (ShaderGen / PaletteWriter) writes Assets/_Game/Settings/NocturnePalette.asset and the generated
// Assets/_Game/Shaders/Generated/NocPalette.hlsl (PAL_<Token>, linear) from the table below; Tools/gen_hlsl.py produces the
// byte-identical HLSL before the first RebuildAll. Materials read colours through MaterialFactory, shaders through PAL_*.
// The 14.2.1 table also lists linear values: Linear() computes them from the hex with the exact sRGB EOTF and the EditMode
// palette test checks every token against the table's 4-decimal values (SpecLinear).
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Megastructure
{
    public enum NocPal
    {
        NightZenith, NightMid, HazeHorizon, CloudBase, CloudHot, AbyssNear, AbyssGlow, Floor,
        NeonMagenta, NeonCyan, SodiumAmber, NeonViolet, SignWhite, AviationRed, WinCold, WinWarm, GoalLamp,
        WetAsphalt, ConcreteDark, Moss, Bark, LeafGlow, LeafWarm, Lantern, KoiBody, KoiPatch, KoiGlow, TitleText,
        // Look v2 (docs/LOOK_V2.md 2.2), appended after TitleText so the 14.2.1 indices stay stable
        CloudCool, HazeLow, LampWarm, WinTungsten, WinNeutral, WinBlue, ScreenTeal, ScreenBase,
        Plaster, Brick, Granite, MetalPaint, SteelF0, Leaf, GlassDark,
    }

    [Serializable]
    public struct PaletteToken
    {
        public string name;
        public string hex;
        public Color linear;   // linear-light RGB (what shaders receive)
    }

    [CreateAssetMenu(menuName = "Megastructure/Nocturne Palette")]
    public class NocturnePalette : ScriptableObject
    {
        // 14.2.1: token, sRGB hex (the order of this table is the order of NocPal)
        public static readonly (NocPal id, string hex)[] Table =
        {
            (NocPal.NightZenith, "#121417"), (NocPal.NightMid, "#1A1D21"), (NocPal.HazeHorizon, "#2E2925"), (NocPal.CloudBase, "#252422"),
            (NocPal.CloudHot, "#4D4034"), (NocPal.AbyssNear, "#24211F"), (NocPal.AbyssGlow, "#4F3D2E"), (NocPal.Floor, "#0A0C0F"),
            (NocPal.NeonMagenta, "#FF3366"), (NocPal.NeonCyan, "#1FE6FF"), (NocPal.SodiumAmber, "#FFA042"), (NocPal.NeonViolet, "#8A5CFF"),
            (NocPal.SignWhite, "#DBEAFF"), (NocPal.AviationRed, "#FF2A22"), (NocPal.WinCold, "#C5DDFF"), (NocPal.WinWarm, "#FFCE96"),
            (NocPal.GoalLamp, "#FFB866"), (NocPal.WetAsphalt, "#3D3C3A"), (NocPal.ConcreteDark, "#74736F"), (NocPal.Moss, "#3F5230"),
            (NocPal.Bark, "#4B423C"), (NocPal.LeafGlow, "#7FE0B0"), (NocPal.LeafWarm, "#D2E68A"), (NocPal.Lantern, "#FFB35C"),
            (NocPal.KoiBody, "#9EE0EE"), (NocPal.KoiPatch, "#FF6A2A"), (NocPal.KoiGlow, "#5CDCFF"), (NocPal.TitleText, "#F3EFE6"),
            (NocPal.CloudCool, "#2A3036"), (NocPal.HazeLow, "#3B3129"), (NocPal.LampWarm, "#FFDBAF"), (NocPal.WinTungsten, "#FFC07B"),
            (NocPal.WinNeutral, "#FFF6EB"), (NocPal.WinBlue, "#8EBAFF"), (NocPal.ScreenTeal, "#0E8C8C"), (NocPal.ScreenBase, "#101416"),
            (NocPal.Plaster, "#8B867E"), (NocPal.Brick, "#6C4D3F"), (NocPal.Granite, "#817E7C"), (NocPal.MetalPaint, "#3F4042"),
            (NocPal.SteelF0, "#C0BEBB"), (NocPal.Leaf, "#3C4B30"), (NocPal.GlassDark, "#242628"),
        };

        /// <summary>The linear column of the LOOK_V2 2.1 / 2.2 tables (4 decimals), in NocPal order: the palette test compares Linear() with it.</summary>
        public static readonly double[,] SpecLinear =
        {
            { 0.0060, 0.0070, 0.0086 }, { 0.0103, 0.0123, 0.0152 }, { 0.0273, 0.0222, 0.0185 }, { 0.0185, 0.0176, 0.0160 },
            { 0.0742, 0.0513, 0.0343 }, { 0.0176, 0.0152, 0.0137 }, { 0.0782, 0.0467, 0.0273 }, { 0.0030, 0.0037, 0.0048 },
            { 1.0000, 0.0331, 0.1329 }, { 0.0137, 0.7913, 1.0000 }, { 1.0000, 0.3515, 0.0545 }, { 0.2542, 0.1070, 1.0000 },
            { 0.7084, 0.8228, 1.0000 }, { 1.0000, 0.0232, 0.0160 }, { 0.5583, 0.7231, 1.0000 }, { 1.0000, 0.6172, 0.3050 },
            { 1.0000, 0.4793, 0.1329 }, { 0.0467, 0.0452, 0.0423 }, { 0.1746, 0.1714, 0.1590 }, { 0.0497, 0.0844, 0.0296 },
            { 0.0704, 0.0545, 0.0452 }, { 0.2122, 0.7454, 0.4342 }, { 0.6445, 0.7913, 0.2542 }, { 1.0000, 0.4508, 0.1070 },
            { 0.3419, 0.7454, 0.8550 }, { 1.0000, 0.1441, 0.0232 }, { 0.1070, 0.7157, 1.0000 }, { 0.8963, 0.8632, 0.7913 },
            { 0.0232, 0.0296, 0.0369 }, { 0.0437, 0.0307, 0.0222 }, { 1.0000, 0.7084, 0.4287 }, { 1.0000, 0.5271, 0.1981 },
            { 1.0000, 0.9216, 0.8308 }, { 0.2705, 0.4910, 1.0000 }, { 0.0044, 0.2623, 0.2623 }, { 0.0052, 0.0070, 0.0080 },
            { 0.2582, 0.2384, 0.2086 }, { 0.1500, 0.0742, 0.0497 }, { 0.2195, 0.2086, 0.2016 }, { 0.0497, 0.0513, 0.0545 },
            { 0.5271, 0.5149, 0.4969 }, { 0.0452, 0.0704, 0.0296 }, { 0.0176, 0.0194, 0.0212 },
        };

        /// <summary>HDR emission factors (LOOK_V2 2.3; palette tokens are appearances): tubes x2.0, signs x1.6, goal lamp x3.5 (the
        /// only core above 3.0), koi body x0.55, koi rim x1.6. EmissionWindow 1.2 is the v1 facade factor, kept for the v1 path
        /// only: Look v2 windows use the per-token luminance normalisation of LOOK_V2 section 4 (Y_med 0.12).</summary>
        public const float EmissionTube = 2.0f, EmissionSign = 1.6f, EmissionWindow = 1.2f, EmissionGoalLamp = 3.5f, EmissionKoiBody = 0.55f, EmissionKoiRim = 1.6f;

        public List<PaletteToken> tokens = new List<PaletteToken>();

        static Color[] s_cache;

        /// <summary>sRGB 8-bit hex to linear-light colour with the exact piecewise sRGB EOTF.</summary>
        public static Color HexToLinear(string hex)
        {
            string h = hex.TrimStart('#');
            int r = Convert.ToInt32(h.Substring(0, 2), 16), g = Convert.ToInt32(h.Substring(2, 2), 16), b = Convert.ToInt32(h.Substring(4, 2), 16);
            return new Color((float)SrgbToLinear(r / 255.0), (float)SrgbToLinear(g / 255.0), (float)SrgbToLinear(b / 255.0), 1f);
        }

        /// <summary>sRGB 8-bit hex to the display (gamma-encoded) colour: UI Toolkit colours are sRGB values.</summary>
        public static Color HexToSrgb(string hex)
        {
            string h = hex.TrimStart('#');
            int r = Convert.ToInt32(h.Substring(0, 2), 16), g = Convert.ToInt32(h.Substring(2, 2), 16), b = Convert.ToInt32(h.Substring(4, 2), 16);
            return new Color(r / 255f, g / 255f, b / 255f, 1f);
        }

        public static double SrgbToLinear(double c) => c <= 0.04045 ? c / 12.92 : Math.Pow((c + 0.055) / 1.055, 2.4);
        public static double LinearToSrgb(double c) => c <= 0.0031308 ? c * 12.92 : 1.055 * Math.Pow(c, 1.0 / 2.4) - 0.055;

        /// <summary>Linear colour of a token from the code table.</summary>
        public static Color Linear(NocPal id)
        {
            if (s_cache == null)
            {
                var c = new Color[Table.Length];
                foreach (var t in Table) c[(int)t.id] = HexToLinear(t.hex);
                s_cache = c;
            }
            return s_cache[(int)id];
        }

        public static Vector3 LinearV(NocPal id) { var c = Linear(id); return new Vector3(c.r, c.g, c.b); }

        public static string Hex(NocPal id)
        {
            foreach (var t in Table) if (t.id == id) return t.hex;
            return null;
        }

        /// <summary>Display (sRGB) colour of a token (UI).</summary>
        public static Color Srgb(NocPal id) => HexToSrgb(Hex(id));

        public void FillFromTable()
        {
            tokens.Clear();
            foreach (var t in Table)
                tokens.Add(new PaletteToken { name = t.id.ToString(), hex = t.hex, linear = HexToLinear(t.hex) });
        }

        public Color Get(NocPal id)
        {
            string n = id.ToString();
            foreach (var t in tokens) if (t.name == n) return t.linear;
            return Linear(id);
        }
    }
}
