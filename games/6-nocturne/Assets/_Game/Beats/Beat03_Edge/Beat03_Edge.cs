// Beat03_Edge: the IBeatBuilder of beat 3, The Edge (s 400-560; MASTER_PROMPT 14.5.3, Phase-3 graybox tier with the real shaders),
// one builder class per hero object in this folder (7.2, Part 3.1 rule 2):
//   TonguePier (deck, hull skirt, glass balustrade, plaza barrier, lamps, benches, balustrade lights), GulfFace (faces A and B with
//   terraces, backs, caps, the garden notch walls, the lower tube portal panels), GulfPortal (collars, light rings, tunnels),
//   CrossBridge (six cross-gulf spans with deck lamps and lit portals), AbyssTraffic (AB, Phase 4 wave A: the eight H7 traffic
//   layers and the cross-bridge lanes as NocStreak kind-4 vehicle rivers, replacing the Phase-3 TrafficLayerLines strokes).
// All content lives under BeatRoot(2) (always active, 14.6) and is built once at load; meshes are validated (MeshValidation) and
// a failure logs [NOC-FAIL]. Materials: MaterialLibrary materials, plus runtime variants derived once from them (agreement 9):
// emissive NocSurface heads / pucks / rings / portal glows (_EmissionColor, linear), NocLine tubes of their own width (_LineWidth:
// the rail 0.06 m, the bridge light strips 0.6 m) and the NocStreak variants of AbyssTraffic and the bridge deck lamps (kind 5). H4: the 14 lamps and 14 balustrade lights are wet sources, the
// Gulf faces are the beat's 4 wet wall planes (face A flanks and face B north / south of the notch) for s 400-880 (beats 3 and 4;
// face B from s 424 so G1's two boulevard walls and these never exceed the 4-wall cap around the boundary). 14.2.3: 24 realtime
// local lights (14 balustrade + 10 lamps).
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Megastructure
{
    public sealed class Beat03_Edge : IBeatBuilder
    {
        public string Name => "Beat03_Edge";

        /// <summary>Counts of the last build (logged; Beat03EdgeTests builds the parts directly).</summary>
        public static int Parts, Meshes, Failures, RealtimeLights, WetSources;
        public static long Triangles;

        /// <summary>Every part of the beat (pure: no scene objects), in build order.</summary>
        public static List<EdgePart> BuildParts(List<EdgeLight> lights)
        {
            var parts = new List<EdgePart>(256);
            TonguePier.Build(parts, lights);
            GulfFace.Build(0, parts);
            GulfFace.Build(1, parts);
            GulfPortal.Build(0, parts);
            GulfPortal.Build(1, parts);
            CrossBridge.Build(parts);
            return parts;
        }

        public void Build(WorldContext ctx)
        {
            Transform root = ctx.BeatRoot(2);
            var lib = ctx.Materials;
            long tris0 = ctx.Triangles;
            var lights = new List<EdgeLight>(32);
            var parts = BuildParts(lights);
            var mats = lib != null ? Resolve(lib) : null;
            Parts = parts.Count; Meshes = 0; Failures = 0;
            foreach (var p in parts)
            {
                if (p.Mb.VertexCount == 0) continue;
                Mesh mesh = p.Mb.ToMesh("Beat03_" + p.Name);
                var rep = MeshValidation.Validate(mesh);
                if (!rep.Ok) { Failures++; Debug.LogError("[NOC-FAIL] Beat03_Edge: " + rep.Describe()); }
                Meshes++;
                if (mats == null) continue;
                if (p.Mat == EdgeMat.Glass)
                {
                    Place(ctx.AddMesh(root, p.Name + "_back", mesh, lib.GlassBack), p.Origin);
                    Place(ctx.AddMesh(root, p.Name + "_front", mesh, lib.GlassFront), p.Origin);
                }
                else Place(ctx.AddMesh(root, p.Name, mesh, mats[(int)p.Mat]), p.Origin);
            }

            // light kit (14.2.3 row 3) and the H4 wet sources
            RealtimeLights = 0; WetSources = 0;
            foreach (var l in lights)
            {
                if (l.Realtime)
                {
                    var go = new GameObject(l.Linear.b > l.Linear.r ? "balLight" : "lamp");
                    go.transform.SetParent(root, false);
                    go.transform.position = l.Pos;
                    var lt = go.AddComponent<Light>();
                    lt.type = LightType.Point;
                    lt.color = l.Linear.gamma;
                    lt.intensity = l.Intensity;
                    lt.range = l.Range;
                    lt.shadows = LightShadows.None;
                    RealtimeLights++;
                }
                NocWetRegistry.AddSource(l.Pos, l.Pos, l.Rho, l.Linear * l.Radiance);
                WetSources++;
            }
            RegisterWalls(lib);
            // AB (INT-01): the vehicle rivers of the eight layers and the six bridges, and the bridges' deck lamps
            AbyssTraffic.Build(lib);
            BridgeLamps(lib);
            Triangles = ctx.Triangles - tris0;
            ctx.Log(FormattableString.Invariant(
                $"Beat03_Edge: {Meshes} meshes ({Failures} validation failures), {Triangles} triangles, {RealtimeLights} realtime lights, {WetSources} wet sources, 4 wet walls; {AbyssTraffic.LayerVehicles} layer + {AbyssTraffic.BridgeVehicles} bridge vehicles, {CrossBridge.Lamps.Count} bridge lamps; tongue w(235/265/295/315/340/355/363.3/370) = {EdgeDims.TongueHalfWidth(235):F2}/{EdgeDims.TongueHalfWidth(265):F2}/{EdgeDims.TongueHalfWidth(295):F2}/{EdgeDims.TongueHalfWidth(315):F2}/{EdgeDims.TongueHalfWidth(340):F2}/{EdgeDims.TongueHalfWidth(355):F2}/{EdgeDims.TongueHalfWidth(363.3):F2}/{EdgeDims.TongueHalfWidth(370):F2}; bridge spans {EdgeDims.BridgeSpan(0):F0}/{EdgeDims.BridgeSpan(1):F0}/{EdgeDims.BridgeSpan(2):F0}/{EdgeDims.BridgeSpan(3):F0}/{EdgeDims.BridgeSpan(4):F0}/{EdgeDims.BridgeSpan(5):F0} m"));
        }

        static void Place(MeshRenderer mr, Vector3 origin) { mr.transform.localPosition = origin; }

        /// <summary>Material per EdgeMat slot: library materials and the runtime variants (derived once, agreement 9).</summary>
        static Material[] Resolve(MaterialLibrary lib)
        {
            var m = new Material[Enum.GetValues(typeof(EdgeMat)).Length];
            m[(int)EdgeMat.Floor] = lib.FloorFor(480.0);
            m[(int)EdgeMat.Stone] = lib.StoneWall;
            m[(int)EdgeMat.Facade] = lib.Facade;
            m[(int)EdgeMat.Concrete] = lib.Concrete;
            m[(int)EdgeMat.Steel] = lib.SteelPainted;
            m[(int)EdgeMat.Moss] = lib.Moss;
            m[(int)EdgeMat.LampHead] = Emissive(lib.Concrete, NocPal.LampWarm, NocturnePalette.EmissionSign, "Beat03_lampHead");   // PA: LampWarm x 1.6
            // G2 r01: at x1.8 the pucks bloomed into 100 px halos on the pier (look_l, KS09); x0.75 keeps them below ~1.0 exposed
            m[(int)EdgeMat.BalLight] = Emissive(lib.Concrete, NocPal.SignWhite, BalLightGain, "Beat03_balLight");   // PA: SignWhite
            m[(int)EdgeMat.PortalRing] = Emissive(lib.Concrete, NocPal.NeonCyan, NocturnePalette.EmissionTube, "Beat03_portalRing");
            // AB (INT-03, INT-18): the balustrade's top rail is a lit handrail (a warm 3 000 K LED diffuser rail), the line that leads
            // the eye along both edges of the tongue to the bridge and carries the pier's light in KS07-KS10
            m[(int)EdgeMat.Rail] = LineVariant(lib.Line, EdgeDims.RailTube, NocturnePalette.Linear(NocPal.LampWarm), RailGlowGain, "Beat03_rail");
            // AB: the bridge edge strips become a dim LED handrail line (the lamps carry the light now); the portal glow is WinNeutral
            m[(int)EdgeMat.BridgeLight] = LineVariant(lib.Line, 2.0 * EdgeDims.BridgeLightR, NocturnePalette.Linear(NocPal.WinNeutral), BridgeStripGain, "Beat03_bridgeLight");
            m[(int)EdgeMat.TunnelMouth] = Emissive(lib.Concrete, NocPal.WinNeutral, PortalGlowGain, "Beat03_portalGlow");
            return m;
        }

        static Material Emissive(Material baseMat, NocPal token, float gain, string name)
        {
            if (baseMat == null) return null;
            Color c = NocturnePalette.Linear(token) * gain;
            var mat = new Material(baseMat) { name = name };
            mat.SetVector(ShaderIdsEmission, new Vector4(c.r, c.g, c.b, 1f));       // linear HDR (SetVector: no colour-space conversion)
            return mat;
        }

        static Material LineVariant(Material baseMat, double width, Color linear, float gain, string name)
        {
            if (baseMat == null) return null;
            var mat = new Material(baseMat) { name = name };
            mat.SetFloat(LineWidthId, (float)width);
            if (gain > 0f) mat.SetVector(ShaderIdsEmission, new Vector4(linear.r * gain, linear.g * gain, linear.b * gain, 1f));
            return mat;
        }

        /// <summary>Emission of the balustrade pucks (x WinCold, linear).</summary>
        public const float BalLightGain = 1.0f;
        /// <summary>AB: radiance of the balustrade lights' wet-floor sources (the r01 0.4 of the puck; the lens slit of INT-18 is
        /// brighter (BalLightGain) but about 1 / 5 of the area, so its reflection keeps the old level).</summary>
        public const float BalWetGain = 0.4f;
        /// <summary>AB: emission of the lit handrail (x LampWarm, linear HDR).</summary>
        public const float RailGlowGain = 1.2f;
        /// <summary>AB: cross-bridge edge strip (x WinNeutral, Phase 3 had SignWhite x 1.2), portal glow (x WinNeutral), deck lamp heads
        /// (NocStreak kind 5: LampWarm x gain, head 0.6 m).</summary>
        public const float BridgeStripGain = 0.35f, PortalGlowGain = 0.9f, BridgeLampGain = 4.0f;

        /// <summary>The cross bridges' deck lamp heads (CrossBridge.Lamps) as one NocStreak kind-5 batch (steady LampWarm points).</summary>
        static void BridgeLamps(MaterialLibrary lib)
        {
            if (lib == null || lib.Beacon == null || CrossBridge.Lamps.Count == 0) return;
            var m = new Material(lib.Beacon) { name = "Beat03_bridgeLamps" };
            Color c = NocturnePalette.Linear(NocPal.LampWarm);
            m.SetFloat(Shader.PropertyToID("_Kind"), 5f);
            m.SetFloat(Shader.PropertyToID("_Gain"), BridgeLampGain);
            m.SetVector(Shader.PropertyToID("_Tint"), new Vector4(c.r, c.g, c.b, 1f));
            var b = new InstancedBatch("BridgeLamps", NocTraffic.PointQuad(), m, chunkSize: 400f);
            for (int i = 0; i < CrossBridge.Lamps.Count; i++)
            {
                Vector3 p = CrossBridge.Lamps[i];
                b.Add(NocTraffic.PointMatrix(p.x, p.y, p.z, CrossBridge.LampHeadSize, NocHash.U01(NocHash.H3((uint)i, 0x4C4D50u, 0u, 0))));
            }
            NocCameraBatches.Register(b);
        }

        static readonly int ShaderIdsEmission = Shader.PropertyToID("_EmissionColor");
        static readonly int LineWidthId = Shader.PropertyToID("_LineWidth");

        /// <summary>The 4 wet wall planes of the Gulf faces (H4): face A flanks z -105..120 and 270..495 (x 215.3, facing +x), face B
        /// south / north of the notch z -105..115 and 275..495 (x 815.3, facing -x), up to y 560, macro = the section-mode mean.</summary>
        public static void RegisterWalls(MaterialLibrary lib)
        {
            // Look v2 (-surf-v 2, TEXTURE_PLAN 2.11): FA's section-mode mean radiance at the middle of the walls' s range
            Color macro = NocLookGlobals.SurfV == 2 ? NocSurfaceMath.FacadeMacro(1, NocFacadeMath.RegionD, 0.5 * (400.0 + 880.0)) : SectionMacro(lib);
            float top = (float)EdgeDims.FaceTop, xa = (float)EdgeDims.FaceXA, xb = (float)EdgeDims.FaceXB;
            double flat0 = EdgeDims.TongueZ - EdgeDims.FlareD0, flat1 = EdgeDims.TongueZ + EdgeDims.FlareD0;
            AddWall(400.0, 880.0, xa, 1f, flat0, EdgeDims.CanyonZ0, top, macro);
            AddWall(400.0, 880.0, xa, 1f, EdgeDims.CanyonZ1, flat1, top, macro);
            AddWall(424.0, 880.0, xb, -1f, flat0, EdgeDims.NotchZ0, top, macro);
            AddWall(424.0, 880.0, xb, -1f, EdgeDims.NotchZ1, flat1, top, macro);
        }

        static void AddWall(double s0, double s1, float x, float nx, double z0, double z1, float top, Color macro)
            => NocWetRegistry.AddWall(s0, s1, x, (float)(0.5 * (z0 + z1)), nx, 0f, (float)(0.5 * (z1 - z0)), top, macro, 0);

        /// <summary>Mean radiance of the section mode (14.2.5 NocSection bays): lit 0.40 x window cover (8.2 x 3.2) / (9 x 4) x the
        /// mean hue (82 % WinCold, 8 % NeonCyan, 6 % NeonMagenta, 4 % SignWhite) x the facade window gain.</summary>
        public static Color SectionMacro(MaterialLibrary lib)
        {
            // PA (LOOK_V2 4): FA's v2 mean window hue at mid-range (w_f, n_f at s 640), scaled to the luminance of the v1 82/8/6/4 mix
            Color v1 = 0.82f * NocturnePalette.Linear(NocPal.WinCold) + 0.08f * NocturnePalette.Linear(NocPal.NeonCyan)
                     + 0.06f * NocturnePalette.Linear(NocPal.NeonMagenta) + 0.04f * NocturnePalette.Linear(NocPal.SignWhite);
            C3 h2 = NocFacadeMath.MeanHueV2(NocSchedule.WarmShare(640.0), NocSchedule.NeutralShare(640.0));
            float k2 = (float)(NocMath.Luma709(v1.r, v1.g, v1.b) / System.Math.Max(h2.Luma, 1e-9));
            Color hue = new Color((float)h2.r * k2, (float)h2.g * k2, (float)h2.b * k2, 1f);
            float gain = 0.2f;
            if (lib != null && lib.Facade != null && lib.Facade.HasProperty("_WindowGain")) gain = lib.Facade.GetFloat("_WindowGain");
            float cover = (float)(NocturneConstants.FacadeSecWinW * NocturneConstants.FacadeSecWinH / (NocturneConstants.FacadeSecP * NocturneConstants.FacadeSecH));
            Color c = hue * ((float)NocturneConstants.FacadeSecLit * cover * gain);
            c.a = 1f;
            return c;
        }
    }
}
