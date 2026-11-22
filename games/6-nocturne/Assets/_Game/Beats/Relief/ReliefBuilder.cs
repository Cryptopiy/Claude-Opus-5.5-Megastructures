// ReliefBuilder: GE's IBeatBuilder (TEXTURE_PLAN 5 / 8.3; discovered by reflection like every builder, GameBootstrap): the Q1-gate
// structures of D-029 (CanyonBridge, NotchGate, NotchEastBlock; GateDims) and, after FA-1, the geometry relief of TEXTURE_PLAN 5.
// Parts are pure (BuildParts: no scene objects, so ReliefTests build, validate and count them), turned into renderers under the
// always-active beat roots (14.6) at load; meshes are validated (MeshValidation) and a failure logs [NOC-FAIL]. Materials are
// library rows (agreement 9: runtime variants derived once): NocFacade for the building faces, NocSurface Concrete for structure,
// two emissive Concrete variants for the lit tunnel mouths (WinCold x 0.40) and the tube-portal collar lamps (GulfPortal.LampLinear:
// NeonCyan half-way to WinNeutral x 2.0 x 0.35, the lower tube portals' lamps of GulfPortal; fix wave C03).
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Megastructure
{
    public sealed class ReliefBuilder : IBeatBuilder
    {
        public string Name => "Relief";

        /// <summary>Counts of the last build (logged; ReliefTests builds the parts directly).</summary>
        public static int Meshes, Failures;
        public static long Triangles;

        /// <summary>Emission of the lit tunnel mouths (x WinCold, linear): a dim cold glow, never a neon accent.</summary>
        public const float TunnelGain = 0.40f;
        /// <summary>Emission of the sign-wall base's shopfronts (x WinCold / WinWarm): GE-r03 KS16 at 0.40 made the shop row the frame's
        /// brightest band under the screen; 0.28 keeps it below the screen's mean.</summary>
        public const float ShopGain = 0.28f;
        /// <summary>AB (INT-01, INT-03): the Gulf faces' light content, linear radiance x token: galleries (WinWarm / WinNeutral), atria
        /// (WinWarm, the lower bands brighter), terrace lip lights (WinNeutral). Light content, never brighter windows (D-032 5).</summary>
        // r03: AtriumLoGain 0.3 -> 0.12 (AB_r02 KS08: the hall glow sat above the lit windows' level and read as a beige card).
        // r04: AtriumLoGain 0.12 -> 0.04 (AB_r03 KS07 / KS11: still pale beige cards at 600 m; now a dark hall behind its lit floor
        // edges), galleries 1.4 / 1.2 -> 1.7 / 1.45 (lit glazed corridors with linear luminaires: the faces' highlight lines, D-032 5);
        // r06: 2.1 / 1.8 (AB_r05: the galleries beyond 800 m sat at luma 90-120 after the fog; KS07 / KS11 / KS12 p99 114 / 119 / 105).
        public const float GalleryWarmGain = 2.1f, GalleryCoolGain = 1.8f, AtriumHiGain = 1.6f, AtriumLoGain = 0.04f, LipGain = 0.9f;

        /// <summary>The relief of TEXTURE_PLAN 5 (Gulf ribs, T0 relief) is aligned to FA's v2 cells (TP-2: the v2 facade is the only
        /// path), so it is on by default; `-relief-v 1` drops it (a debug switch). The gate structures of D-029 are always built.</summary>
        public static bool ReliefOn => CliArgs.GetInt("-relief-v", 2) == 2;

        /// <summary>Every part, in build order (pure); relief: include the v2 relief (tests pass true).</summary>
        public static List<ReliefPart> BuildParts(bool relief)
        {
            var parts = new List<ReliefPart>(32);
            CanyonBridge.Build(parts);
            NotchGate.Build(parts);
            NotchEastBlock.Build(parts);
            PromenadeLintel.Build(parts);
            if (relief)
            {
                GulfRibBuilder.Build(parts);
                BoulevardRelief.Build(parts);
                SignBaseRelief.Build(parts);
            }
            return parts;
        }

        public void Build(WorldContext ctx)
        {
            var lib = ctx.Materials;
            long t0 = ctx.Triangles;
            bool relief = ReliefOn;
            var parts = BuildParts(relief);
            var mats = lib != null ? Resolve(lib) : null;
            Meshes = 0; Failures = 0;
            foreach (var p in parts)
            {
                if (p.Mb.VertexCount == 0) continue;
                Mesh mesh = p.Mb.ToMesh("Relief_" + p.Name);
                var rep = MeshValidation.Validate(mesh);
                if (!rep.Ok) { Failures++; Debug.LogError("[NOC-FAIL] Relief: " + rep.Describe()); }
                Meshes++;
                if (mats == null) continue;
                var mr = ctx.AddMesh(ctx.BeatRoot(p.Beat), p.Name, mesh, mats[(int)p.Mat]);
                mr.transform.localPosition = p.Origin;
            }
            Triangles = ctx.Triangles - t0;
            ctx.Log(FormattableString.Invariant(
                $"Relief: {Meshes} meshes ({Failures} validation failures), {Triangles} triangles, v2 relief {(relief ? "on" : "off")}; canyon bridge x {GateDims.CbX0}-{GateDims.CbX1} y {GateDims.CbY0}-{GateDims.CbY1}, notch gate soffit y {GateDims.NgY0}, east block x {GateDims.EbX0}..R {GateDims.EbR}, {GateDims.Lanes().Count} lane portals"));
        }

        static Material[] Resolve(MaterialLibrary lib)
        {
            var m = new Material[Enum.GetValues(typeof(ReliefMat)).Length];
            m[(int)ReliefMat.Facade] = lib.Facade;
            m[(int)ReliefMat.Concrete] = lib.Concrete;
            m[(int)ReliefMat.Steel] = lib.SteelPainted;
            m[(int)ReliefMat.TunnelLight] = Emissive(lib.Concrete, NocPal.WinCold, TunnelGain, "Relief_tunnelLight");
            m[(int)ReliefMat.PortalRing] = EmissiveLinear(lib.Concrete, GulfPortal.LampLinear(), "Relief_portalRing");
            m[(int)ReliefMat.ShopWarm] = Emissive(lib.Concrete, NocPal.WinWarm, ShopGain, "Relief_shopWarm");
            m[(int)ReliefMat.ShopCold] = Emissive(lib.Concrete, NocPal.WinCold, ShopGain, "Relief_shopCold");
            m[(int)ReliefMat.GalleryWarm] = Emissive(lib.Concrete, NocPal.WinWarm, GalleryWarmGain, "Relief_galleryWarm");
            m[(int)ReliefMat.GalleryCool] = Emissive(lib.Concrete, NocPal.WinNeutral, GalleryCoolGain, "Relief_galleryCool");
            m[(int)ReliefMat.AtriumHi] = Emissive(lib.Concrete, NocPal.WinWarm, AtriumHiGain, "Relief_atriumHi");
            m[(int)ReliefMat.AtriumLo] = Emissive(lib.Concrete, NocPal.WinWarm, AtriumLoGain, "Relief_atriumLo");
            m[(int)ReliefMat.LipLight] = Emissive(lib.Concrete, NocPal.WinNeutral, LipGain, "Relief_lipLight");
            return m;
        }

        static readonly int s_emission = Shader.PropertyToID("_EmissionColor");

        static Material Emissive(Material baseMat, NocPal token, float gain, string name)
            => EmissiveLinear(baseMat, NocturnePalette.Linear(token) * gain, name);

        static Material EmissiveLinear(Material baseMat, Color c, string name)
        {
            if (baseMat == null) return null;
            var mat = new Material(baseMat) { name = name };
            mat.SetVector(s_emission, new Vector4(c.r, c.g, c.b, 1f));       // linear HDR (SetVector: no colour-space conversion)
            return mat;
        }
    }
}
