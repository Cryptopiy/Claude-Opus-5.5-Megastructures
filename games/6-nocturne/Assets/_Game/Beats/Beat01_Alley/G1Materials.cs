// G1Materials: the material slots of beats 1-2 (agreement 9: variants are derived at load from MaterialLibrary materials with
// new Material(lib.X) and their properties set once; MaterialFactory, MaterialLibrary and the shared shaders are not touched).
// Colour rule (DEVELOPMENT.md, MaterialFactory header): a Color property is stored gamma-encoded, so a linear palette value L is
// passed as L.gamma and the shader receives exactly L (x the HDR factor of 14.2.1).
//   * emissive panels: NocGraybox (lib.GrayMarker) with a dark albedo and _EmissionColor = token x factor:
//     bar window and warm shops WinWarm x 1.2 (14.2.1 windows), cold shops WinCold x 0.6, vending screens NeonCyan x 0.35 (dim),
//     the mouth glow panel CloudHot x 1.0; lamp heads use lib.GrayMarkerWarm as built (SodiumAmber x 1.8, signs);
//   * NocLine tubes: lib.Line with the true width per slot (cables 6 mm, tram rails 0.07 m, rods / railings 0.04 m, lamp posts
//     0.20 m; the mesh radius equals half the width, NocLine pushes it to >= 1.75 px);
//   * NocNeon: lib.Neon with _Palette 0..3 (the blade colour cycle of 14.5.2), -1 = the library material (hash colour).
using System.Collections.Generic;
using UnityEngine;

namespace Megastructure
{
    public static class G1Materials
    {
        public const float WinWarmFactor = 1.2f, ShopColdFactor = 0.6f, ScreenFactor = 0.35f, MouthFactor = 1.0f, ShopWarmFactor = 0.9f;
        public const float Cable = 0.006f, Rail = 0.07f, Rod = 0.04f, Post = 0.20f;

        static MaterialLibrary s_lib;
        static readonly Dictionary<G1Mat, Material> s_cache = new Dictionary<G1Mat, Material>(32);
        static readonly Material[] s_neon = new Material[9];   // palette -1 (library) and the fixed slots 0-7 (PA)

        static readonly int IdEmission = Shader.PropertyToID("_EmissionColor");
        static readonly int IdBase = Shader.PropertyToID("_BaseColor");
        static readonly int IdWidth = Shader.PropertyToID("_LineWidth");
        static readonly int IdPalette = Shader.PropertyToID("_Palette");

        static void Bind(MaterialLibrary lib)
        {
            if (s_lib == lib) return;
            s_lib = lib;
            s_cache.Clear();
            for (int i = 0; i < s_neon.Length; i++) s_neon[i] = null;
        }

        /// <summary>The material of a slot (derived variants cached per library; null without a library, e.g. in EditMode tests).</summary>
        public static Material Get(MaterialLibrary lib, G1Mat slot)
        {
            if (lib == null) return null;
            Bind(lib);
            if (s_cache.TryGetValue(slot, out var m)) return m;
            m = Make(lib, slot);
            s_cache[slot] = m;
            return m;
        }

        /// <summary>NocNeon with a fixed palette colour (pal 0..3) or the library material (pal -1).</summary>
        public static Material Neon(MaterialLibrary lib, int pal)
        {
            if (lib == null || lib.Neon == null) return null;
            Bind(lib);
            int i = Mathf.Clamp(pal + 1, 0, 8);
            if (s_neon[i] != null) return s_neon[i];
            if (pal < 0) return s_neon[i] = lib.Neon;
            var m = new Material(lib.Neon) { name = "G1_Neon_" + pal };
            m.SetFloat(IdPalette, pal);
            return s_neon[i] = m;
        }

        static Material Make(MaterialLibrary lib, G1Mat slot)
        {
            switch (slot)
            {
                case G1Mat.FloorAlley: return lib.FloorFor(60.0);
                case G1Mat.FloorStone: return lib.FloorFor(260.0);
                case G1Mat.Concrete: return lib.Concrete;
                case G1Mat.StoneWall: return lib.StoneWall;
                case G1Mat.SteelPainted: return lib.SteelPainted;
                case G1Mat.SteelBare: return lib.SteelBare;
                case G1Mat.Asphalt: return lib.Asphalt;
                case G1Mat.Facade: return lib.Facade;
                case G1Mat.EmitSodium: return lib.GrayMarkerWarm;
                case G1Mat.EmitWarm: return Room(lib, "G1_WinWarm", NocPal.WinWarm, WinWarmFactor, 3.0f, 0.45f, 0.12f, 1) ?? Emissive(lib, "G1_WinWarm", NocPal.WinWarm, WinWarmFactor);
                case G1Mat.EmitShopWarm: return Room(lib, "G1_ShopWarm", NocPal.WinWarm, ShopWarmFactor, 4.0f, 0.45f, 0.12f, 2) ?? Emissive(lib, "G1_ShopWarm", NocPal.WinWarm, ShopWarmFactor);
                case G1Mat.EmitCold: return Room(lib, "G1_WinCold", NocPal.WinCold, WinWarmFactor, 4.0f, 0.45f, 0.12f, 3) ?? Emissive(lib, "G1_WinCold", NocPal.WinCold, WinWarmFactor);
                case G1Mat.EmitShopCold: return Room(lib, "G1_ShopCold", NocPal.WinCold, ShopColdFactor, 4.0f, 0.45f, 0.12f, 4) ?? Emissive(lib, "G1_ShopCold", NocPal.WinCold, ShopColdFactor);
                case G1Mat.EmitScreen: return Room(lib, "G1_Screen", NocPal.NeonCyan, ScreenFactor, 0.35f, 0.22f, 0.07f, 5) ?? Emissive(lib, "G1_Screen", NocPal.NeonCyan, ScreenFactor);
                case G1Mat.EmitMouth: return Emissive(lib, "G1_Mouth", NocPal.CloudHot, MouthFactor);
                case G1Mat.EmitLamp: return Emissive(lib, "G1_Lamp", NocPal.LampWarm, NocturnePalette.EmissionSign);
                case G1Mat.Grating: return lib.Grating != null ? lib.Grating : lib.SteelPainted;
                case G1Mat.ConcreteAlley: return NocLookGlobals.SurfV == 2 && lib.ConcreteAlley != null ? lib.ConcreteAlley : lib.Concrete;
                case G1Mat.Wood: return NocLookGlobals.SurfV == 2 && lib.Wood != null ? lib.Wood : lib.SteelPainted;
                case G1Mat.TileWall: return NocLookGlobals.SurfV == 2 && lib.TileWall != null ? lib.TileWall : lib.Concrete;
                case G1Mat.Line6mm: return Line(lib, "G1_Cable", Cable, NocPal.ConcreteDark, 0.6f);
                case G1Mat.LineRail: return Line(lib, "G1_Rail", Rail, NocPal.SteelF0, 1f);            // PA (LOOK_V2 10): bare-steel F0
                case G1Mat.LineRod: return Line(lib, "G1_Rod", Rod, NocPal.ConcreteDark, 1.05f);
                case G1Mat.LinePost: return Line(lib, "G1_Post", Post, NocPal.ConcreteDark, 1.05f);
                case G1Mat.GlassBack: return lib.GlassBack;
                case G1Mat.GlassFront: return lib.GlassFront;
                case G1Mat.AlleyWindow: return AlleyMaterials.Make(slot, lib) ?? Room(lib, "G1_ShopWarm", NocPal.WinWarm, ShopWarmFactor, 4.0f, 0.45f, 0.12f, 2) ?? Emissive(lib, "G1_ShopWarm", NocPal.WinWarm, ShopWarmFactor);
                case G1Mat.AlleyRoomBar: case G1Mat.AlleyRoomA: case G1Mat.AlleyRoomB: case G1Mat.AlleySign: return AlleyMaterials.Make(slot, lib) ?? lib.Concrete;
                case G1Mat.Steam: case G1Mat.SteamBar: case G1Mat.AlleyPool: case G1Mat.AlleyGlow: case G1Mat.AlleyWorkPool: return AlleyMaterials.Make(slot, lib);
            }
            return lib.GrayWall;
        }

        static Material Emissive(MaterialLibrary lib, string name, NocPal token, float factor)
        {
            var src = lib.GrayMarker != null ? lib.GrayMarker : lib.GrayWall;
            if (src == null) return null;
            var m = new Material(src) { name = name };
            Color e = NocturnePalette.Linear(token) * factor;
            e.a = 1f;
            m.SetColor(IdEmission, e.gamma);
            Color b = NocturnePalette.Linear(NocPal.WetAsphalt);
            m.SetColor(IdBase, b.gamma);
            return m;
        }

        /// <summary>Look v2 (-surf-v 2, TEXTURE_PLAN 3.5 SU-4; CRIT0 #24): the lit-room glass of an emissive slot, derived from
        /// lib.Shopfront with the slot's light (token x factor, the same mean x the room gain 0.6), room depth (m), shelf pitch, product
        /// width and seed; null on v1 or without the row (the caller keeps the flat emissive panel).</summary>
        static Material Room(MaterialLibrary lib, string name, NocPal token, float factor, float depth, float pitch, float itemW, int seed)
        {
            if (NocLookGlobals.SurfV != 2 || lib.Shopfront == null) return null;
            var m = new Material(lib.Shopfront) { name = name + "_Room" };
            Color e = NocturnePalette.Linear(token) * factor;
            e.a = 1f;
            m.SetColor(IdEmission, e.gamma);
            m.SetFloat(IdRoomDepth, depth);
            m.SetFloat(IdShelfPitch, pitch);
            m.SetFloat(IdItemWidth, itemW);
            m.SetFloat(IdSeed, seed);
            return m;
        }

        static readonly int IdRoomDepth = Shader.PropertyToID("_RoomDepth"), IdShelfPitch = Shader.PropertyToID("_ShelfPitch");
        static readonly int IdItemWidth = Shader.PropertyToID("_ItemWidth"), IdSeed = Shader.PropertyToID("_Seed");

        static Material Line(MaterialLibrary lib, string name, float width, NocPal token, float factor)
        {
            if (lib.Line == null) return null;
            var m = new Material(lib.Line) { name = name };
            m.SetFloat(IdWidth, width);
            Color c = NocturnePalette.Linear(token) * factor;
            c.a = 1f;
            m.SetColor(IdBase, c.gamma);
            return m;
        }

        /// <summary>Look v2 (-surf-v 2): the shop glass shows its room at x RoomGain (0.6) of the slot's emission, so the wet-floor
        /// sources of shop windows and screens scale by it too (1 on v1).</summary>
        public const float RoomGain = 0.6f;
        public static float ShopSourceGain => NocLookGlobals.SurfV == 2 ? RoomGain : 1f;

        /// <summary>Linear radiance of a token x factor (wet-floor sources and walls, NocWetRegistry).</summary>
        public static Color Lin(NocPal token, float factor)
        {
            Color c = NocturnePalette.Linear(token) * factor;
            c.a = 1f;
            return c;
        }
    }
}
