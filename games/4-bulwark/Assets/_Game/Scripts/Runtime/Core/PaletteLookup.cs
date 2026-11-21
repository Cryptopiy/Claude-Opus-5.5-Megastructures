// PaletteLookup: framework access to BulwarkPalette tokens BY NAME (e.g. "TitleText", "ConcreteMid"; case- and
// underscore-insensitive), so imported systems
// depend only on the palette table's shape ((id, hex) pairs, 14.2.1), never on the enum member names. Colours are still
// typed only in BulwarkPalette.cs (T-PAL). sRGB 8-bit hex -> linear with the exact piecewise sRGB EOTF.
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Megastructure
{
    public static class PaletteLookup
    {
        static Dictionary<string, Color> s_map;

        static void Build()
        {
            var map = new Dictionary<string, Color>(StringComparer.Ordinal);
            foreach (var e in BulwarkPalette.Table) map[Key(e.id.ToString())] = HexToLinear(e.hex);
            s_map = map;
        }

        /// <summary>Linear colour of a token; false (and magenta, so a typo is visible) when the token does not exist.</summary>
        public static bool TryLinear(string token, out Color c)
        {
            if (s_map == null) Build();
            if (s_map.TryGetValue(Key(token), out c)) return true;
            c = new Color(1f, 0f, 1f, 1f); // pal:ok (missing-token marker)
            return false;
        }

        /// <summary>Lookup key: case- and underscore-insensitive, so "TitleText" also finds an enum member TITLE_TEXT.</summary>
        static string Key(string token) => token.Replace("_", "").ToLowerInvariant();

        public static Color Linear(string token)
        {
            if (!TryLinear(token, out Color c)) Debug.LogError("[BW-FAIL] palette token '" + token + "' missing in BulwarkPalette.Table");
            return c;
        }

        /// <summary>Gamma-space (sRGB) colour of a token, for UI Toolkit styles.</summary>
        public static Color Srgb(string token) => Linear(token).gamma;

        public static Color HexToLinear(string hex)
        {
            string h = hex.TrimStart('#');
            int r = Convert.ToInt32(h.Substring(0, 2), 16), g = Convert.ToInt32(h.Substring(2, 2), 16), b = Convert.ToInt32(h.Substring(4, 2), 16);
            return new Color((float)SrgbToLinear(r / 255.0), (float)SrgbToLinear(g / 255.0), (float)SrgbToLinear(b / 255.0), 1f);
        }

        public static double SrgbToLinear(double c) => c <= 0.04045 ? c / 12.92 : Math.Pow((c + 0.055) / 1.055, 2.4);
        public static double LinearToSrgb(double c) => c <= 0.0031308 ? c * 12.92 : 1.055 * Math.Pow(c, 1.0 / 2.4) - 0.055;

        /// <summary>Test hook: forget the cache (after the table changed in an EditMode test).</summary>
        public static void Reset() { s_map = null; }
    }
}
