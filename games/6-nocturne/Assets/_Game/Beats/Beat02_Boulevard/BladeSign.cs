// BladeSign: the twelve 14 x 200 m blades of the boulevard (MASTER_PROMPT 14.5.2, graybox: emissive blade slabs with glyph cells;
// BoulevardLayout). North blades x = 18 + 36 k (k 0-5) with their street face at z 263, y 40-240; south x = 6 + 36 k at z 127,
// y 60-260. Each blade: a dark steel slab 14 x 200 x 0.8 m (SteelPainted), the NocNeon face 3 cm proud of it toward the street
// (11 glyph cells of 14 x 18.2 m, strokes 0.055 W = 0.77 m >= 0.35 m, 14.5.2 pitfall) and a second NocNeon face on its back (the
// south blades' backs are seen from the alley: S6 fills the KS02 slit above the mouth), six steel brackets back to the shaft face
// (z 274 / 116). Colours (LOOK_V2 10, R1 / R7: no magenta, no red, no violet on a blade) cycle SignWhite, NeonCyan, WinNeutral,
// SodiumAmber, SignWhite, WinTungsten in x order over both sides (S6, N18, S42, ...).
// Every blade is a wet-floor source (a vertical strip on its face, half-width 7 m, the NocNeon mean radiance) and carries a
// point light r 40 i 4 in front of its lower part, coloured lerp(c, WinNeutral, 0.4) (LOOK_V2 6 / R4: a light that washes the
// street stays at sRGB saturation <= 0.45); BV: the light is created and weighed by BoulevardLights (the 14.2.3 budget).
// KS04 (s 250): north blade x 198 at x -0.45..-0.32 (its lower 100 m), south blade x 186 at x +0.32..+0.49.
using UnityEngine;

namespace Megastructure
{
    public static class BladeSign
    {
        public const float SourceRadiance = 0.25f, LightRange = 40f, LightIntensity = 4f, LightOut = 6f, LightUp = 30f;
        /// <summary>The LOOK_V2 10 six-colour blade cycle.</summary>
        public static readonly NocPal[] Cycle = { NocPal.SignWhite, NocPal.NeonCyan, NocPal.WinNeutral, NocPal.SodiumAmber, NocPal.SignWhite, NocPal.WinTungsten };
        /// <summary>G1Kit batches the NocNeon palette slots -1..7 (DONE SU in docs/logs/SU.md), so SignWhite (4), WinNeutral (5) and
        /// WinTungsten (7) have their own slots. With false, SignWhite and WinNeutral would share slot 3 (the retired violet slot,
        /// drawn as WinNeutral) and WinTungsten slot 2 (SodiumAmber).</summary>
        public const bool ExtendedPalettes = true;

        /// <summary>Blade i in x order (0..11): side (+1 north, -1 south), centre x, face z, bottom y.</summary>
        public static void Blade(int i, out int side, out double x, out double zFace, out double y0)
        {
            int k = i / 2;
            if ((i & 1) == 0) { side = -1; x = BoulevardLayout.SouthBladeX0 + BoulevardLayout.BladePitch * k; zFace = BoulevardLayout.SouthBladeZ; y0 = BoulevardLayout.SouthBladeY0; }
            else { side = 1; x = BoulevardLayout.NorthBladeX0 + BoulevardLayout.BladePitch * k; zFace = BoulevardLayout.NorthBladeZ; y0 = BoulevardLayout.NorthBladeY0; }
        }

        public static int Count => 2 * BoulevardLayout.BladesPerSide;
        /// <summary>The NocNeon _Palette slot of blade i (0 magenta, 1 cyan, 2 amber, 3 / 5 WinNeutral, 4 SignWhite, 6 red, 7 tungsten).</summary>
        public static int Palette(int i)
        {
            switch (Cycle[i % Cycle.Length])
            {
                case NocPal.NeonCyan: return 1;
                case NocPal.SodiumAmber: return 2;
                case NocPal.SignWhite: return ExtendedPalettes ? 4 : 3;
                case NocPal.WinTungsten: return ExtendedPalettes ? 7 : 2;
                default: return ExtendedPalettes ? 5 : 3;      // WinNeutral
            }
        }

        /// <summary>The token blade i displays (its NocNeon slot's colour).</summary>
        public static NocPal Token(int i)
        {
            switch (Palette(i))
            {
                case 1: return NocPal.NeonCyan;
                case 2: return NocPal.SodiumAmber;
                case 4: return NocPal.SignWhite;
                case 7: return NocPal.WinTungsten;
                default: return NocPal.WinNeutral;
            }
        }

        /// <summary>The emission colour of blade i (linear token; the wet-floor source).</summary>
        public static Color Colour(int i) => G1Materials.Lin(Token(i), 1f);

        /// <summary>The light colour of blade i: lerp(c, WinNeutral, 0.4) in linear (R4).</summary>
        public static Color LightColour(int i) => Color.Lerp(Colour(i), G1Materials.Lin(NocPal.WinNeutral, 1f), 0.4f);

        public static void Build(G1Parts parts)
        {
            float w = (float)BoulevardLayout.BladeW, h = (float)BoulevardLayout.BladeH, t = (float)BoulevardLayout.BladeThick;
            for (int i = 0; i < Count; i++)
            {
                Blade(i, out int side, out double xd, out double zd, out double yd);
                float x = (float)xd, zf = (float)zd, y0 = (float)yd;
                // the slab body lies behind the face (toward its facade): north z 263.0..263.8, south z 126.2..127.0
                float zb = zf + side * t;
                var body = parts.Get("blade" + i, G1Mat.SteelPainted, new Vector3(x, y0 + 0.5f * h, zf));
                G1Mesh.Chamfer(body, new Vector3(x - 0.5f * w, y0, Mathf.Min(zf, zb)), new Vector3(x + 0.5f * w, y0 + h, Mathf.Max(zf, zb)), 0.03f);
                // brackets: 2 per height, from the slab back to the shaft face (north 274, south 116), 0.3 m into the shaft
                float zShaft = (float)(side > 0 ? BoulevardLayout.NorthShaftZ0 : BoulevardLayout.SouthShaftZ1) + side * 0.3f;
                float bw = 0.5f * (float)BoulevardLayout.BracketW;
                foreach (double fy in BoulevardLayout.BracketY)
                {
                    float by = y0 + (float)fy * h;
                    foreach (float dx in new[] { -0.32f * w, 0.32f * w })
                        G1Mesh.Chamfer(body, new Vector3(x + dx - bw, by - bw, Mathf.Min(zb, zShaft)), new Vector3(x + dx + bw, by + bw, Mathf.Max(zb, zShaft)), 0.02f);
                }
                // the NocNeon face, 3 cm proud of the slab toward the street; local X = world x (W), local Y = up (H)
                float zg = zf - side * (float)BoulevardLayout.BladeGlyphOff;
                parts.Sign(new Vector3(x, y0 + 0.5f * h, zg), Quaternion.identity, w, h, Palette(i));
                // the back face carries glyphs too (the south blades' backs show in the alley's KS02 slit: S6 stands over the mouth)
                parts.Sign(new Vector3(x, y0 + 0.5f * h, zb + side * (float)BoulevardLayout.BladeGlyphOff), Quaternion.identity, w, h, Palette(i));
                // the blade's light (r 40 i 4 in front of its lower part) is BoulevardLights' (BV: the beat-2 light budget)
            }
        }

        public static void RegisterSources()
        {
            for (int i = 0; i < Count; i++)
            {
                Blade(i, out int side, out double x, out double zf, out double y0);
                float zg = (float)zf - side * 0.1f;
                NocWetRegistry.AddSource(new Vector3((float)x, (float)y0 + 8f, zg), new Vector3((float)x, (float)(y0 + BoulevardLayout.BladeH) - 8f, zg),
                    7f, Colour(i) * SourceRadiance);
            }
        }
    }
}
