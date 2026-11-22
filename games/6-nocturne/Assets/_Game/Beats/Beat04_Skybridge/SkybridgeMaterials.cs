// SkybridgeMaterials: the materials of beat 4, all from the MaterialLibrary (14.2.5): the steel deck floor (lib.FloorFor(s),
// NocWet), painted steel for the slab sides, truss, outriggers and gate frames, concrete for the gate blocks and the tube tunnels,
// NocGlass (back / front faces, D-008) for the train tubes, NocLine variants for the thin members (one material per true width,
// because the NocLine tube push needs radius = _LineWidth / 2; MetalPaint, LOOK_V2 2.2), and the emissive LampWarm lamp heads
// (a NocSurface variant, LampWarm x 1.6 = EmissionSign, LOOK_V2 6). TR fix wave: the upper face tube's rib arcs (C41, a NocLine
// variant lit on its lower facets, SignWhite x FaceLineGain) and the bridge tubes' glass copies with FX's Fresnel / reflection
// cap _FresnelMax 0.5 (INT-40 / C06, FX 02:25: the 9 m tubes read milky at grazing; a no-op until FX's NocGlass with the property
// is merged). The face tubes keep the library glass (uncapped: the end-on upper tube must not vanish, C41).
// Variants are derived once at build (properties set once); null when the library is missing (EditMode tests).
using UnityEngine;

namespace Megastructure
{
    public sealed class SkybridgeMaterials
    {
        public readonly Material Deck, Steel, Concrete, Tunnel, GlassBack, GlassFront, Post, TopRail, Rod, TrackRail, LampPole, LampArm, LampHead;
        public readonly Material FaceLine, TubeGlassBack, TubeGlassFront;
        /// <summary>FX's NocGlass _FresnelMax on the bridge tubes (FX log 02:25, "TR sets about 0.5").</summary>
        public const float TubeFresnelMax = 0.5f;

        public SkybridgeMaterials(MaterialLibrary lib)
        {
            if (lib == null) return;
            Deck = lib.FloorFor(0.5 * (NocturneConstants.BeatBoundaries[3] + NocturneConstants.BeatBoundaries[4]));
            Steel = lib.SteelPainted;
            Concrete = lib.Concrete;
            Tunnel = BridgeGardenKit.Derive(lib.Concrete, "Skybridge_Tunnel");
            BridgeGardenKit.SetFloat(Tunnel, "_Wetness", 0.1f);
            BridgeGardenKit.SetFloat(Tunnel, "_Streaks", 0f);
            BridgeGardenKit.SetFloat(Tunnel, "_Cull", (float)UnityEngine.Rendering.CullMode.Back);
            GlassBack = lib.GlassBack;
            GlassFront = lib.GlassFront;
            TubeGlassBack = GlassBack;
            TubeGlassFront = GlassFront;
            if (lib.GlassBack != null && lib.GlassFront != null)
            {
                TubeGlassBack = BridgeGardenKit.Derive(lib.GlassBack, "Skybridge_TubeGlassBack");
                TubeGlassFront = BridgeGardenKit.Derive(lib.GlassFront, "Skybridge_TubeGlassFront");
                TubeGlassBack.renderQueue = lib.GlassBack.renderQueue;      // back faces before front faces (D-008)
                TubeGlassFront.renderQueue = lib.GlassFront.renderQueue;
                BridgeGardenKit.SetFloat(TubeGlassBack, "_FresnelMax", TubeFresnelMax);
                BridgeGardenKit.SetFloat(TubeGlassFront, "_FresnelMax", TubeFresnelMax);
            }
            Post = Line(lib.Line, "Skybridge_RailPost", SkybridgeDims.PostD);
            TopRail = Line(lib.Line, "Skybridge_TopRail", SkybridgeDims.TopRailD);
            Rod = Line(lib.Line, "Skybridge_RailRod", SkybridgeDims.RodD);
            TrackRail = Line(lib.Line, "Skybridge_TrackRail", SkybridgeDims.TrackRailD);
            LampPole = Line(lib.Line, "Skybridge_LampPole", SkybridgeDims.LampPoleD);
            LampArm = Line(lib.Line, "Skybridge_LampArm", SkybridgeDims.LampArmD);
            LampHead = BridgeGardenKit.Derive(lib.SteelPainted, "Skybridge_LampHead");
            BridgeGardenKit.SetLinear(LampHead, "_EmissionColor", BridgeGardenKit.Lin(NocPal.LampWarm, NocturnePalette.EmissionSign));
            FaceLine = Line(lib.Line, "Skybridge_FaceTubeLine", SkybridgeTubes.FaceLineD);
            BridgeGardenKit.SetLinear(FaceLine, "_EmissionColor", BridgeGardenKit.Lin(NocPal.SignWhite, SkybridgeTubes.FaceLineGain));
            BridgeGardenKit.SetFloat(FaceLine, "_EmissionDown", 1f);
        }

        static Material Line(Material src, string name, float width)
        {
            var m = BridgeGardenKit.Derive(src, name);
            BridgeGardenKit.SetFloat(m, "_LineWidth", width);
            BridgeGardenKit.SetLinear(m, "_BaseColor", BridgeGardenKit.Lin(NocPal.MetalPaint));   // dark graphite paint (LOOK_V2 2.2)
            return m;
        }
    }
}
