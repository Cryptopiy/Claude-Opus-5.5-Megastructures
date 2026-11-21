// BulwarkPalette: the only place a colour is typed (MASTER_PROMPT 14.2.1). sRGB hex is the source; linear RGB is
// derived with the exact piecewise sRGB EOTF (T-PAL: within 0.004 of the spec's linear column).
// ToHlsl() produces Assets/_Game/Shaders/Include/BulwarkPalette.hlsl (PAL_<Token> float3 macros, linear); PaletteTests
// checks that the committed file equals ToHlsl() byte for byte. The ScriptableObject side (tokens, FillFromTable, Get)
// keeps Thread's palette API for the asset writer and the material factory.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using UnityEngine;

namespace Megastructure
{
    /// <summary>Palette tokens. 0-20: the 14.2.1 table in order; 21-26: the other colours named by 14.2.1 (sky-state
    /// zeniths, noon key, cool city light) and the key colours of 14.2.3 (beat 2 and beat 7).</summary>
    public enum Pal
    {
        ConcreteMid, ConcreteDark, ConcreteWet, SteelPaint, LaneWhite, SafetyOchre, SeaStorm, SeaTeal, FoamWhite,
        StormHorizon, BreakGold, NoonHorizon, DuskAmber, DuskRose, DuskViolet, DuskDeep, Tungsten, SodiumWarm, CityWarm,
        Lantern, TitleText,
        StormZenith, NoonZenith, NoonKey, CityCool, StormKey, DuskKey,
        // Addendum A1 1.1 rule 10 kraken tokens (append-only, kraken owner): skin grading targets of Tools/kraken and BwCreature*
        KrakenSkinDark, KrakenMoss, KrakenUnderRed, KrakenSucker, KrakenEyeGlow, KrakenEyeMilky,
    }

    [Serializable]
    public struct PaletteToken
    {
        public string name;
        public string hex;
        public Color linear;   // linear-light RGB (what shaders receive)
    }

    [CreateAssetMenu(menuName = "Megastructure/Bulwark Palette")]
    public class BulwarkPalette : ScriptableObject
    {
        /// <summary>Token, sRGB hex. Order = enum order (tests check it).</summary>
        public static readonly (Pal id, string hex)[] Table =
        {
            (Pal.ConcreteMid, "#7F8281"),    // road, walls, tower
            (Pal.ConcreteDark, "#4B4F52"),   // basalt: lee face, tunnel, ledge, units
            (Pal.ConcreteWet, "#363C41"),    // wet zone, armour units
            (Pal.SteelPaint, "#5E6A73"),     // doors, rails, gates
            (Pal.LaneWhite, "#D8D5CB"),      // lane marks and digits
            (Pal.SafetyOchre, "#C79A33"),    // door seams, handrails
            (Pal.SeaStorm, "#3F5C68"),       // storm sea
            (Pal.SeaTeal, "#1F727C"),        // clear sea
            (Pal.FoamWhite, "#EAF1F3"),      // foam, spray
            (Pal.StormHorizon, "#8B969C"),   // storm horizon
            (Pal.BreakGold, "#FFC987"),      // sun-break key
            (Pal.NoonHorizon, "#BCD5E7"),    // noon horizon
            (Pal.DuskAmber, "#F4A35A"),      // dusk horizon
            (Pal.DuskRose, "#C9708A"),       // dusk mid sky (12 deg)
            (Pal.DuskViolet, "#5A4A8D"),     // dusk upper sky (35 deg)
            (Pal.DuskDeep, "#1E2552"),       // dusk zenith
            (Pal.Tungsten, "#FFC27A"),       // tower lamps (x3)
            (Pal.SodiumWarm, "#FFB04A"),     // corridor lamps (x2.5)
            (Pal.CityWarm, "#FFC070"),       // city lights
            (Pal.Lantern, "#FFB562"),        // lantern (x6)
            (Pal.TitleText, "#EDEBE5"),      // UI text
            (Pal.StormZenith, "#5F6E79"),    // storm sky zenith (14.2.1 sky states)
            (Pal.NoonZenith, "#3F7FC2"),     // noon sky zenith
            (Pal.NoonKey, "#FFF3E3"),        // noon key light (beat 4 sun, 14.2.3)
            (Pal.CityCool, "#BEE2FF"),       // cool city lights (lerp partner of CityWarm)
            (Pal.StormKey, "#FFD9B0"),       // beat 2 hidden sun key (14.2.3)
            (Pal.DuskKey, "#FF9A4A"),        // beat 7 dusk key (14.2.3)
            (Pal.KrakenSkinDark, "#1E242B"), // kraken back: wet blue-black (A1 1.1)
            (Pal.KrakenMoss, "#3D4A3A"),     // kraken moss / algae, kelp: grey-green (A1 1.1)
            (Pal.KrakenUnderRed, "#7A2418"), // kraken arm undersides: blood-red / rust (A1 1.1)
            (Pal.KrakenSucker, "#D8C2B0"),   // kraken suckers and hooks: pale bone-pink (A1 1.1)
            (Pal.KrakenEyeGlow, "#FF6127"),  // kraken eye emission, linear (1.0, 0.12, 0.02) x E (A1 1.2)
            (Pal.KrakenEyeMilky, "#B9C2BE"), // the milky lesser eye that never glows (A1 1.1)
        };

        public const string HlslGuard = "BULWARK_PALETTE_INCLUDED";
        public const string HlslAssetPath = "Assets/_Game/Shaders/Include/BulwarkPalette.hlsl";

        public List<PaletteToken> tokens = new List<PaletteToken>();

        static double[][] s_linear;

        // ---------------------------------------------------------------- pure maths (no engine calls)
        public static double SrgbToLinear(double c) => c <= 0.04045 ? c / 12.92 : Math.Pow((c + 0.055) / 1.055, 2.4);
        public static double LinearToSrgb(double c) => c <= 0.0031308 ? c * 12.92 : 1.055 * Math.Pow(c, 1.0 / 2.4) - 0.055;

        /// <summary>sRGB 8-bit channel (0 = R, 1 = G, 2 = B) of "#RRGGBB".</summary>
        public static int HexChannel(string hex, int channel)
        {
            string h = hex.TrimStart('#');
            if (h.Length != 6) throw new ArgumentException("BulwarkPalette: hex must be #RRGGBB, got " + hex);
            return Convert.ToInt32(h.Substring(2 * channel, 2), 16);
        }

        /// <summary>Linear RGB (double) of "#RRGGBB" with the exact sRGB EOTF.</summary>
        public static void HexToLinearRgb(string hex, out double r, out double g, out double b)
        {
            r = SrgbToLinear(HexChannel(hex, 0) / 255.0);
            g = SrgbToLinear(HexChannel(hex, 1) / 255.0);
            b = SrgbToLinear(HexChannel(hex, 2) / 255.0);
        }

        /// <summary>sRGB hex of a token from the code table.</summary>
        public static string Hex(Pal id)
        {
            foreach (var t in Table) if (t.id == id) return t.hex;
            throw new ArgumentOutOfRangeException(nameof(id));
        }

        /// <summary>Linear RGB (double) of a token.</summary>
        public static void LinearRgb(Pal id, out double r, out double g, out double b)
        {
            if (s_linear == null)
            {
                var c = new double[Table.Length][];
                foreach (var t in Table)
                {
                    HexToLinearRgb(t.hex, out double lr, out double lg, out double lb);
                    c[(int)t.id] = new[] { lr, lg, lb };
                }
                s_linear = c;
            }
            var v = s_linear[(int)id];
            r = v[0]; g = v[1]; b = v[2];
        }

        /// <summary>Text of Assets/_Game/Shaders/Include/BulwarkPalette.hlsl: LF line ends, no BOM (write it as UTF-8
        /// without BOM), values "0.######" invariant with at least one decimal (Thread's F6 format).</summary>
        public static string ToHlsl()
        {
            var sb = new StringBuilder();
            sb.Append("// GENERATED from BulwarkPalette.ToHlsl() (Megastructure.BulwarkPalette.Table, MASTER_PROMPT 14.2.1) - do not edit.\n");
            sb.Append("// Linear RGB of each sRGB token (exact sRGB EOTF). PaletteTests checks this file equals ToHlsl().\n");
            sb.Append("#ifndef ").Append(HlslGuard).Append('\n');
            sb.Append("#define ").Append(HlslGuard).Append('\n');
            foreach (var t in Table)
            {
                HexToLinearRgb(t.hex, out double r, out double g, out double b);
                sb.Append("#define PAL_").Append(t.id.ToString()).Append(" float3(")
                  .Append(F6(r)).Append(", ").Append(F6(g)).Append(", ").Append(F6(b)).Append(")\n");
            }
            sb.Append("#endif\n");
            return sb.ToString();
        }

        /// <summary>"0.######" invariant, trailing zeros trimmed, at least one decimal (identical to Tools/gen_hlsl.py f6).</summary>
        public static string F6(double v)
        {
            string s = v.ToString("0.000000", CultureInfo.InvariantCulture).TrimEnd('0');
            if (s.EndsWith(".", StringComparison.Ordinal)) s += "0";
            return s;
        }

        // ---------------------------------------------------------------- Unity side (Thread's palette API)
        /// <summary>sRGB 8-bit hex to linear-light colour.</summary>
        public static Color HexToLinear(string hex)
        {
            HexToLinearRgb(hex, out double r, out double g, out double b);
            return new Color((float)r, (float)g, (float)b, 1f);
        }

        /// <summary>Linear colour of a token from the code table (identical to the asset by construction).</summary>
        public static Color Linear(Pal id)
        {
            LinearRgb(id, out double r, out double g, out double b);
            return new Color((float)r, (float)g, (float)b, 1f);
        }

        public static Vector3 LinearV(Pal id)
        {
            LinearRgb(id, out double r, out double g, out double b);
            return new Vector3((float)r, (float)g, (float)b);
        }

        public void FillFromTable()
        {
            tokens.Clear();
            foreach (var t in Table)
                tokens.Add(new PaletteToken { name = t.id.ToString(), hex = t.hex, linear = HexToLinear(t.hex) });
        }

        public Color Get(Pal id)
        {
            string n = id.ToString();
            foreach (var t in tokens) if (t.name == n) return t.linear;
            return Linear(id);
        }
    }
}
