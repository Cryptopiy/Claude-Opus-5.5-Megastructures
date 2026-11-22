// Beat03_Edge: the IBeatBuilder of beat 3, The Edge (s 400-560; MASTER_PROMPT 14.5.3, Phase-3 graybox tier with the real shaders),
// one builder class per hero object in this folder (7.2, Part 3.1 rule 2):
//   TonguePier (deck, hull skirt, glass balustrade, plaza barrier, lamps, benches, balustrade lights), GulfFace (faces A and B with
//   terraces, backs, caps, the garden notch walls, the lower tube portal panels), GulfPortal (collars, light rings, tunnels),
//   CrossBridge (six cross-gulf spans with deck lamps and lit portals), AbyssTraffic (AB, Phase 4 wave A: the eight H7 traffic
//   layers and the cross-bridge lanes as NocStreak kind-4 vehicle rivers, replacing the Phase-3 TrafficLayerLines strokes).
// All content lives under BeatRoot(2) (always active, 14.6) and is built once at load; meshes are validated (MeshValidation) and
// a failure logs [NOC-FAIL]. Materials: MaterialLibrary materials, plus runtime variants derived once from them (agreement 9):
// emissive NocSurface heads / pucks / rings / portal glows (_EmissionColor, linear), NocLine tubes of their own width (_LineWidth:
// the rail 0.06 m, the bridge light strips 0.6 m) and the NocStreak variants of AbyssTraffic and the bridge deck lamps (kind 5). H4: the 14 lamps and 14 balustrade lights are wet sources (plus the lit handrail as 4 segment sources, integration round), the
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
                    bool spot = l.SpotDir.sqrMagnitude > 0.5f;
                    var go = new GameObject(spot ? "balLight" : "lamp");
                    go.transform.SetParent(root, false);
                    go.transform.position = l.Pos + l.LightOffset;
                    var lt = go.AddComponent<Light>();
                    lt.type = spot ? LightType.Spot : LightType.Point;
                    if (spot)
                    {
                        // fix wave C13: the step lights throw inward and down; the balustrade glass, shoe and housing lie behind the cone
                        go.transform.rotation = Quaternion.LookRotation(l.SpotDir, Vector3.up);
                        lt.spotAngle = l.SpotAngle;
                        lt.innerSpotAngle = l.SpotInner;
                    }
                    lt.color = l.Linear.gamma;
                    lt.intensity = l.Intensity;
                    lt.range = l.Range;
                    lt.shadows = LightShadows.None;
                    RealtimeLights++;
                }
                NocWetRegistry.AddSource(l.Pos, l.Pos, l.Rho, l.Linear * l.Radiance);
                WetSources++;
            }
            WetSources += RegisterRailSources();
            RegisterWalls(lib);
            // AB (INT-01): the vehicle rivers of the eight layers and the six bridges, and the bridges' deck lamps
            AbyssTraffic.Build(lib);
            BridgeLamps(lib);
            Triangles = ctx.Triangles - tris0;
            ctx.Log(FormattableString.Invariant(
                $"Beat03_Edge: {Meshes} meshes ({Failures} validation failures), {Triangles} triangles, {RealtimeLights} realtime lights, {WetSources} wet sources, 4 wet walls; {AbyssTraffic.LayerVehicles} layer + {AbyssTraffic.BridgeVehicles} bridge + {AbyssTraffic.AirVehicles} air-crossing vehicles, {CrossBridge.Lamps.Count} bridge lamps; tongue w(235/265/295/315/340/355/363.3/370) = {EdgeDims.TongueHalfWidth(235):F2}/{EdgeDims.TongueHalfWidth(265):F2}/{EdgeDims.TongueHalfWidth(295):F2}/{EdgeDims.TongueHalfWidth(315):F2}/{EdgeDims.TongueHalfWidth(340):F2}/{EdgeDims.TongueHalfWidth(355):F2}/{EdgeDims.TongueHalfWidth(363.3):F2}/{EdgeDims.TongueHalfWidth(370):F2}; bridge spans {EdgeDims.BridgeSpan(0):F0}/{EdgeDims.BridgeSpan(1):F0}/{EdgeDims.BridgeSpan(2):F0}/{EdgeDims.BridgeSpan(3):F0}/{EdgeDims.BridgeSpan(4):F0}/{EdgeDims.BridgeSpan(5):F0} m"));
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
            m[(int)EdgeMat.BalLight] = Emissive(lib.Concrete, NocPal.LampWarm, BalLightGain, "Beat03_balLight");   // r04: LampWarm (one 3 000 K family with the handrail and the ledge LED; PA had SignWhite)
            // fix wave C03: the collar lamps at GulfPortal.LampGain of EmissionTube (the continuous ring at 1.0 read as a HUD reticle)
            m[(int)EdgeMat.PortalRing] = EmissiveLinear(lib.Concrete, GulfPortal.LampLinear(), "Beat03_portalRing");
            // AB (INT-03, INT-18): the balustrade's top rail is a lit handrail, the line that leads the eye along both edges of the
            // tongue to the bridge. r03: a dark steel rail with a warm 3 000 K LED channel in its underside (NocLine _EmissionDown 1):
            // the r02 all-round glowing tube read as a 50 px flat clipped bar across KS09 / KS10
            m[(int)EdgeMat.Rail] = LineVariant(lib.Line, EdgeDims.RailTube, NocturnePalette.Linear(NocPal.LampWarm), RailGlowGain, "Beat03_rail", true);
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

        static Material EmissiveLinear(Material baseMat, Color c, string name)
        {
            if (baseMat == null) return null;
            var mat = new Material(baseMat) { name = name };
            mat.SetVector(ShaderIdsEmission, new Vector4(c.r, c.g, c.b, 1f));
            return mat;
        }

        static Material LineVariant(Material baseMat, double width, Color linear, float gain, string name, bool underside = false)
        {
            if (baseMat == null) return null;
            var mat = new Material(baseMat) { name = name };
            mat.SetFloat(LineWidthId, (float)width);
            if (gain > 0f) mat.SetVector(ShaderIdsEmission, new Vector4(linear.r * gain, linear.g * gain, linear.b * gain, 1f));
            mat.SetFloat(EmissionDownId, underside ? 1f : 0f);
            return mat;
        }

        /// <summary>Emission of the balustrade pucks (x WinCold, linear).</summary>
        public const float BalLightGain = 1.0f;
        /// <summary>AB: radiance of the balustrade lights' wet-floor sources (the r01 0.4 of the puck; the lens slit of INT-18 is
        /// brighter (BalLightGain) but about 1 / 5 of the area, so its reflection keeps the old level).</summary>
        public const float BalWetGain = 0.4f;
        /// <summary>AB: emission of the lit handrail's LED channel (x LampWarm, linear HDR; r03: the underside only, so 1.2 -> 1.6).</summary>
        public const float RailGlowGain = 1.6f;
        /// <summary>AB: cross-bridge edge strip (x WinNeutral, Phase 3 had SignWhite x 1.2), portal glow (x WinNeutral), deck lamp heads
        /// (NocStreak kind 5: LampWarm x gain, head 0.6 m).</summary>
        public const float BridgeStripGain = 1.25f, PortalGlowGain = 0.9f, BridgeLampGain = 6.0f;   // r04: lamps 4 -> 6, r05: edge strips 0.35 -> 0.8, r07: 1.25 (AB_r06 KS12: the strips of the 655 m / 1 185 m bridges sat at luma 100-110, KS12 p99 107.6)

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
        static readonly int EmissionDownId = Shader.PropertyToID("_EmissionDown");

        /// <summary>The 4 wet wall planes of the Gulf faces (H4): face A flanks z -105..120 and 270..495 (x 215.3, facing +x), face B
        /// south / north of the notch z -105..115 and 275..495 (x 815.3, facing -x), up to y 560, macro = the section-mode mean.
        /// Fix wave (C43, p4_01 KS07: the plaza floor y 600-1080 mean 29.2, std 4.3, while face B's window field fills the upper
        /// half unmirrored): face B's walls start at WallsBS0 (s 400, the tongue plaza and KS07 at s 402, where the walker faces
        /// them) and face A's at WallsAS0 (s 424: at s 400-424 face A is behind the walker, so its mirror is never seen looking
        /// forward). G1's two boulevard walls fade out over s 400-412, face B's fade in over 388-400 and face A's over 412-424:
        /// never more than 4 walls (the cap) at any s.</summary>
        public static void RegisterWalls(MaterialLibrary lib)
        {
            // Look v2 (TEXTURE_PLAN 2.11): FA's section-mode mean radiance at the middle of the walls' s range (TP-2: the v2 surfaces
            // are the only path, so SectionMacro is no longer used here)
            Color macro = NocSurfaceMath.FacadeMacro(1, NocFacadeMath.RegionD, 0.5 * (400.0 + 880.0));
            float top = (float)EdgeDims.FaceTop, xa = (float)EdgeDims.FaceXA, xb = (float)EdgeDims.FaceXB;
            double flat0 = EdgeDims.TongueZ - EdgeDims.FlareD0, flat1 = EdgeDims.TongueZ + EdgeDims.FlareD0;
            AddWall(WallsAS0, 880.0, xa, 1f, flat0, EdgeDims.CanyonZ0, top, macro);
            AddWall(WallsAS0, 880.0, xa, 1f, EdgeDims.CanyonZ1, flat1, top, macro);
            AddWall(WallsBS0, 880.0, xb, -1f, flat0, EdgeDims.NotchZ0, top, macro);
            AddWall(WallsBS0, 880.0, xb, -1f, EdgeDims.NotchZ1, flat1, top, macro);
        }

        /// <summary>Integration round (FX's C04 request: KS09's must-read "rail strip visible as a streak in the pier"): the lit
        /// handrail's LED channel (the rail's underside, y RailHeight - RailTube) as wet segment sources, two straight runs a side:
        /// x 305.3..353.25 (the straight neck w(x) = 3.2 and the start of the bow's taper) and 353.25..371.95 (the taper; the
        /// break point minimises the larger chord error: 3.0 cm at most, inside rho), radiance LampWarm x BalLightGain, rho 0.1 m. The pier floor looks down at a uniform dark sky, so only a
        /// registered source streaks on it (a point source makes no line). 4 sources: beat 3 stays <= 32 active
        /// (Beat03EdgeTests). Returns the number registered.</summary>
        public static int RegisterRailSources()
        {
            Color radiance = G1Materials.Lin(NocPal.LampWarm, BalLightGain);
            float y = (float)(EdgeDims.RailHeight - EdgeDims.RailTube);
            int n = 0;
            for (int s = -1; s <= 1; s += 2)
                for (int k = 0; k + 1 < RailSourceX.Length; k++)
                {
                    NocWetRegistry.AddSource(RailPoint(s, RailSourceX[k], y), RailPoint(s, RailSourceX[k + 1], y), 0.1f, radiance, double.NaN);
                    n++;
                }
            return n;
        }

        /// <summary>Break points (x) of the rail's wet segment sources (runs of 47.95 / 18.70 m, chords within 3 cm of the rail).</summary>
        public static readonly double[] RailSourceX = { EdgeDims.TongueFunnelX1, RailSourceBreakX, EdgeDims.RailEndX };
        public const double RailSourceBreakX = 353.25;

        /// <summary>Point of side s's rail centre line at x and height y (the line of TonguePier.RailLine).</summary>
        public static Vector3 RailPoint(int s, double x, float y)
            => new Vector3((float)x, y, (float)(EdgeDims.TongueZ + s * (EdgeDims.TongueHalfWidth(x) - EdgeDims.RailInset)));

        /// <summary>First s of face A's / face B's wall planes (12 m fades before them; C43).</summary>
        public const double WallsAS0 = 424.0, WallsBS0 = 400.0;

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
