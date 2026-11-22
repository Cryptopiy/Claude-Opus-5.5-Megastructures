// AlleyMaterials: the materials of beat 1's own shaders (Phase 4 wave B, AL; agreement 9: created at load from Shader.Find with their
// properties set once, MaterialFactory / MaterialLibrary untouched). G1Materials.Make resolves the AL slots of G1Mat through here:
//   * AlleyWindow: Megastructure/NocAlleyWindow (framed, recessed windows with rooms, curtains and blinds; INT-06), room light
//     WinWarm x 0.9 / WinCold x 0.6 x the shop-glass gain 0.6 (the old flat cards' mean), room depth 3.5 m, reveal 0.14 m;
//   * AlleyRoomBar / AlleyRoomA / AlleyRoomB: Megastructure/NocAlleyRoom (the noodle bar, INT-07, and the two empty rooms of
//     docs/DREAD.md 6) with each room's analytic lamps (positions and colours from NoodleBar / AlleyRooms);
//   * AlleySign: Megastructure/NocAlleyRoom without lamps, the blade signs' light-box faces (kind 4, self-lit x 3);
//   * Steam / SteamBar: Megastructure/NocSteam (the four vents, the bar's pots with a warm kitchen glow);
//   * AlleyPool: Megastructure/NocAlleyPool (light pools on the walls, INT-21);
//   * AlleyGlow: Megastructure/NocAlleyGlow (the lit boulevard haze seen through the mouth, KS00);
//   * AlleyWorkPool (fix wave, D-039 (6)): Megastructure/NocAlleyPool in spot mode, the two catwalk work floodlights' wash on the
//     KS02 walls (positions, aims and cones from AlleyFixtures.WorkLamp).
// Fix wave C37: the empty rooms (AlleyRoomA / B) are enclosed (_Interior 1: own lamp only, 10 % of the sky ambient through the
// window) and their matte surfaces take the library's CC0 plaster (MaterialLibrary.Plaster's _AlbedoTex2, the texprep set
// Plaster, tile from its _Tile2) with stains. C38: the vents' steam peak opacity 0.30 -> 0.45 (the in-scatter is in NocSteam).
// Colour rule (DEVELOPMENT.md): a Color property is stored gamma-encoded, so a linear value L is passed as L.gamma.
// Without the shader (EditMode, a build that lost it) every slot returns null and G1Materials falls back to the library material.
using UnityEngine;

namespace Megastructure
{
    public static class AlleyMaterials
    {
        public const float WindowRoomDepth = 3.5f, WindowReveal = 0.14f;
        static readonly int IdEmission = Shader.PropertyToID("_EmissionColor"), IdCold = Shader.PropertyToID("_ColdColor");
        static readonly int IdGain = Shader.PropertyToID("_RoomGain"), IdDepth = Shader.PropertyToID("_RoomDepth"), IdReveal = Shader.PropertyToID("_Reveal");
        static readonly int IdRevealColor = Shader.PropertyToID("_RevealColor"), IdBase = Shader.PropertyToID("_BaseColor");
        static readonly int IdLamp0 = Shader.PropertyToID("_Lamp0"), IdLamp0C = Shader.PropertyToID("_Lamp0Color");
        static readonly int IdLamp1 = Shader.PropertyToID("_Lamp1"), IdLamp1C = Shader.PropertyToID("_Lamp1Color");
        static readonly int IdSelfLit = Shader.PropertyToID("_SelfLit"), IdFill = Shader.PropertyToID("_Fill");
        static readonly int IdGlow = Shader.PropertyToID("_Glow"), IdOpacity = Shader.PropertyToID("_Opacity");
        static readonly int IdPool = Shader.PropertyToID("_PoolColor");
        static readonly int IdInterior = Shader.PropertyToID("_Interior"), IdInteriorAmb = Shader.PropertyToID("_InteriorAmb");
        static readonly int IdWallTex = Shader.PropertyToID("_WallTex"), IdWallTexOn = Shader.PropertyToID("_WallTexOn");
        static readonly int IdWallTile = Shader.PropertyToID("_WallTile"), IdStain = Shader.PropertyToID("_Stain");
        static readonly int IdAlbedoTex2 = Shader.PropertyToID("_AlbedoTex2"), IdTile2 = Shader.PropertyToID("_Tile2");
        static readonly int IdSpot = Shader.PropertyToID("_Spot"), IdSpotA = Shader.PropertyToID("_SpotA"), IdSpotADir = Shader.PropertyToID("_SpotADir");
        static readonly int IdSpotB = Shader.PropertyToID("_SpotB"), IdSpotBDir = Shader.PropertyToID("_SpotBDir");
        static readonly int IdSpotInner = Shader.PropertyToID("_SpotInner"), IdSpotColor = Shader.PropertyToID("_SpotColor");
        /// <summary>C37: the share of the sky ambient that reaches an enclosed room through its window.</summary>
        public const float InteriorAmbient = 0.10f;

        static Material New(string shader, string name)
        {
            var sh = Shader.Find(shader);
            if (sh == null) return null;
            return new Material(sh) { name = name };
        }

        static Color G(Color linear) { linear.a = 1f; return linear.gamma; }

        public static Material Window()
        {
            var m = New("Megastructure/NocAlleyWindow", "AL_AlleyWindow");
            if (m == null) return null;
            m.SetColor(IdEmission, G(NocturnePalette.Linear(NocPal.WinWarm) * G1Materials.ShopWarmFactor));
            m.SetColor(IdCold, G(NocturnePalette.Linear(NocPal.WinCold) * G1Materials.ShopColdFactor));
            m.SetFloat(IdGain, G1Materials.RoomGain);
            m.SetFloat(IdDepth, WindowRoomDepth);
            m.SetFloat(IdReveal, WindowReveal);
            m.SetColor(IdRevealColor, G(NocturnePalette.Linear(NocPal.ConcreteDark) * 1.0f));
            return m;
        }

        /// <summary>A room material with its two analytic lamps (world position, intensity; linear colour).</summary>
        public static Material Room(string name, Vector4 lamp0, Color colour0, Vector4 lamp1, Color colour1, float selfLit, float fill)
        {
            var m = New("Megastructure/NocAlleyRoom", "AL_Room_" + name);
            if (m == null) return null;
            m.SetVector(IdLamp0, lamp0);
            m.SetColor(IdLamp0C, G(colour0));
            m.SetVector(IdLamp1, lamp1);
            m.SetColor(IdLamp1C, G(colour1));
            m.SetFloat(IdSelfLit, selfLit);
            m.SetFloat(IdFill, fill);
            return m;
        }

        /// <summary>C37: an enclosed room (own lamp only) with the library's CC0 plaster and stains on its matte surfaces.</summary>
        public static Material EnclosedRoom(string name, Vector4 lamp, Color colour, float selfLit, float fill, MaterialLibrary lib)
        {
            var m = Room(name, lamp, colour, new Vector4(0f, -50f, 0f, 0f), Color.black, selfLit, fill);
            if (m == null) return null;
            m.SetFloat(IdInterior, 1f);
            m.SetFloat(IdInteriorAmb, InteriorAmbient);
            m.SetFloat(IdStain, 1f);
            Material plaster = lib != null ? lib.Plaster : null;
            Texture tex = plaster != null && plaster.HasProperty(IdAlbedoTex2) ? plaster.GetTexture(IdAlbedoTex2) : null;
            if (tex != null)
            {
                m.SetTexture(IdWallTex, tex);
                m.SetFloat(IdWallTexOn, 1f);
                float tile = plaster.HasProperty(IdTile2) ? plaster.GetFloat(IdTile2) : 3.2f;
                m.SetFloat(IdWallTile, tile > 0.1f ? tile : 3.2f);
            }
            return m;
        }

        public static Material Steam(bool bar)
        {
            var m = New("Megastructure/NocSteam", bar ? "AL_SteamBar" : "AL_Steam");
            if (m == null) return null;
            Color glow = bar ? NocturnePalette.Linear(NocPal.LampWarm) * 0.30f : Color.black;
            m.SetColor(IdGlow, G(glow));
            m.SetFloat(IdOpacity, bar ? 0.5f : VentOpacity);
            m.SetFloat(IdScatter, VentScatter);
            m.SetColor(IdLampColor, G(G1Materials.Lin(NocPal.LampWarm, 1f)));
            m.SetFloat(IdLampScale, AlleyFixtures.SteamLampScale);
            return m;
        }

        /// <summary>C38: peak opacity of the vents' plumes (r00-p4_01: 0.30, invisible).</summary>
        public const float VentOpacity = 0.45f, VentScatter = 1.2f;   // KS00 4K rows 950-1100 at x 1995: scatter 2.0 +32-42 %, 1.4 +23-28 % (AL_f02 / f03)
        static readonly int IdScatter = Shader.PropertyToID("_Scatter"), IdLampColor = Shader.PropertyToID("_LampColor"), IdLampScale = Shader.PropertyToID("_LampScale");

        /// <summary>D-039 (6): the two catwalk work floodlights' wash on the walls (NocAlleyPool spot mode).</summary>
        public static Material WorkPool()
        {
            var m = New("Megastructure/NocAlleyPool", "AL_WorkPool");
            if (m == null) return null;
            m.SetFloat(IdSpot, 1f);
            for (int k = 0; k < 2; k++)
            {
                AlleyFixtures.WorkLamp(k, out Vector3 pos, out Vector3 aim);
                var p = new Vector4(pos.x, pos.y, pos.z, AlleyFixtures.WorkLampIntensity);
                var a = new Vector4(aim.x, aim.y, aim.z, Mathf.Cos(AlleyFixtures.WorkLampOuterDeg * Mathf.Deg2Rad));
                m.SetVector(k == 0 ? IdSpotA : IdSpotB, p);
                m.SetVector(k == 0 ? IdSpotADir : IdSpotBDir, a);
            }
            m.SetFloat(IdSpotInner, Mathf.Cos(AlleyFixtures.WorkLampInnerDeg * Mathf.Deg2Rad));
            m.SetColor(IdSpotColor, G(AlleyFixtures.WorkLampPoolColour()));
            return m;
        }

        public static Material Pool()
        {
            var m = New("Megastructure/NocAlleyPool", "AL_Pool");
            if (m == null) return null;
            return m;
        }

        /// <summary>The material of an AL slot (null when the shader is missing: the caller falls back).</summary>
        public static Material Make(G1Mat slot, MaterialLibrary lib = null)
        {
            switch (slot)
            {
                case G1Mat.AlleyWindow: return Window();
                case G1Mat.AlleyRoomBar: return Room("Bar", NoodleBar.Lamp0, NoodleBar.LampColour, NoodleBar.Lamp1, NoodleBar.LampColour, NoodleBar.SelfLit, 0.015f);
                case G1Mat.AlleyRoomA: return EnclosedRoom("A", AlleyRooms.Lamp(0), AlleyRooms.LampColour, AlleyRooms.SelfLit, AlleyRooms.Fill, lib);
                case G1Mat.AlleyRoomB: return EnclosedRoom("B", AlleyRooms.Lamp(1), AlleyRooms.LampColour, AlleyRooms.SelfLit, AlleyRooms.Fill, lib);
                case G1Mat.AlleySign: return Room("Sign", new Vector4(0f, -50f, 0f, 0f), Color.black, new Vector4(0f, -50f, 0f, 0f), Color.black, AlleyFixtures.LightBoxGain, 0f);
                case G1Mat.Steam: return Steam(false);
                case G1Mat.SteamBar: return Steam(true);
                case G1Mat.AlleyPool: return Pool();
                case G1Mat.AlleyGlow: return New("Megastructure/NocAlleyGlow", "AL_MouthGlow");
                case G1Mat.AlleyWorkPool: return WorkPool();
            }
            return null;
        }
    }
}
