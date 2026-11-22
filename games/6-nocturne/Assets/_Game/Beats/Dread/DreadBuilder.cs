// DreadBuilder: the IBeatBuilder of package DR (docs/DREAD.md sections 2 and 3, decision D-034; wave B), discovered by
// GameBootstrap like every beat builder. It builds, once at load, under BeatRoot(2) (beat 3, always active, 14.6):
//   Dread_Ark     THE ARK (D-043, HullBuilder: a 5 000 x 440 x 260 m stepped ship, spine, pods, gondolas; NocArk), its lamp rows and
//                 engine glows (NocDreadLight); hidden until its launch at s 380, then down the rift from the north haze (HullCourse);
//   Dread_Shadow  the shadow below (ShadowBuilder: the body, NocDreadBody; its back lights, NocDreadLight), parked at z 384;
//   Dread_Panel   the falling sign panel (PanelBuilder, NocHull with a backlit face), on face B at y 150, z 800 until s 470;
//   Dread_Cabins  32 lift cabins in eight shafts on the Gulf faces (DreadCabins; NocDreadLight movers on the traffic clock, drawn
//                 as lit cab boxes) and the shafts' steady guide lights (Dread_ShaftGuides);
//   Dread_Gantry0/1 and their rails: two maintenance gantries sliding along face B at 0.5 m/s (DreadCabins);
// and registers DreadDriver, which moves both by DreadCourse and drives the globals. Materials are made here from their shaders
// (Shader.Find; every Megastructure/ shader is always included by BuildWindows / RebuildAll), never MaterialLibrary rows. No
// realtime lights, no shadows, nothing on the wet-floor registry. Skipped at tier 2 (graybox).
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Megastructure
{
    public sealed class DreadBuilder : IBeatBuilder
    {
        public string Name => "Dread";

        /// <summary>Counts of the last build (logged; DreadTests builds the parts directly).</summary>
        public static int Meshes, Failures, HullLights, ShadowLights;
        public static long HullTriangles, ShadowTriangles;
        public static DreadDriver Driver;

        public void Build(WorldContext ctx)
        {
            Driver = null;
            if (ctx.Tier >= 2) { ctx.Log("Dread: skipped at tier 2 (graybox)"); return; }
            // QA A/B only (never set in a release run): NOC_DREAD_OFF=1 builds none of DR's objects (the globals stay zero = identity)
            if (Environment.GetEnvironmentVariable("NOC_DREAD_OFF") == "1") { ctx.Log("Dread: skipped (NOC_DREAD_OFF=1, QA A/B)"); return; }
            Transform root = ctx.BeatRoot(2);
            Meshes = 0; Failures = 0;

            // ---- THE ARK (D-043; replaces the 2.1 hull): hidden until its launch at s 380, then down the rift from the north haze
            var hull = new GameObject("Dread_Ark").transform;
            hull.SetParent(root, false);
            hull.position = new Vector3((float)HullCourse.AxisX, 0f, (float)HullCourse.ParkBow);
            var hullR = new List<Renderer>(3);
            var hmb = HullBuilder.BuildHull();
            HullTriangles = hmb.TriangleCount;
            Mesh hm = Checked(hmb, "Dread_ArkBody");
            var hullMat = Make("Megastructure/NocArk", "Dread_Ark");
            if (hullMat != null)
            {
                // the ambient fields are Vector properties (raw linear); the paint is a Color property, gamma-converted in this linear
                // project, so it is set from its gamma encoding (fix wave r07: the hull's 0.035 had drawn as 0.0027)
                hullMat.SetColor(BaseColorId, new Color(Mathf.LinearToGammaSpace(HullPaint.x), Mathf.LinearToGammaSpace(HullPaint.y),
                                                        Mathf.LinearToGammaSpace(HullPaint.z), 1f));
                hullMat.SetVector(BellyLightId, HullBellyLight);
                hullMat.SetVector(SideLightId, HullSideLight);
                hullMat.SetVector(TopLightId, HullTopLight);
                hullMat.SetFloat(WinGainId, ArkWinGain);
                hullMat.SetFloat(PortGainId, ArkPortGain);
                hullMat.SetFloat(CabGainId, ArkCabGain);
                // perf: a closed, outward-wound body the walker is never inside: back faces culled (Cull Off shaded the hidden far side)
                hullMat.SetFloat(CullId, 2f);
            }
            var hullLightMat = Make("Megastructure/NocDreadLight", "Dread_ArkLights");
            var hl = HullBuilder.BuildLights();
            HullLights = hl.Count;
            if (hullMat != null) hullR.Add(ctx.AddMesh(hull, "body", hm, hullMat));
            if (hullLightMat != null) hullR.Add(ctx.AddMesh(hull, "lights", DreadLights.ToMesh("Dread_ArkLights", hl, 300f), hullLightMat));

            // ---- the shadow below (3.1)
            var shadow = new GameObject("Dread_Shadow").transform;
            shadow.SetParent(root, false);
            shadow.position = new Vector3((float)ShadowCourse.AxisX, (float)ShadowCourse.CentreY, (float)ShadowCourse.ParkCentre);
            var shadowR = new List<Renderer>(2);
            var smb = ShadowBuilder.BuildBody();
            ShadowTriangles = smb.TriangleCount;
            Mesh sm = Checked(smb, "Dread_ShadowBody");
            var bodyMat = Make("Megastructure/NocDreadBody", "Dread_ShadowBody");
            if (bodyMat != null) bodyMat.SetVector(SheenId, BodySheen);      // a Vector property: raw linear (see the hull)
            var shadowLightMat = Make("Megastructure/NocDreadLight", "Dread_ShadowLights");
            var sl = ShadowBuilder.BuildLights();
            ShadowLights = sl.Count;
            if (bodyMat != null) shadowR.Add(ctx.AddMesh(shadow, "body", sm, bodyMat));
            if (shadowLightMat != null) shadowR.Add(ctx.AddMesh(shadow, "lights", DreadLights.ToMesh("Dread_ShadowLights", sl, 50f), shadowLightMat));

            // ---- the falling panel (3.2)
            var panel = new GameObject("Dread_Panel").transform;
            panel.SetParent(root, false);
            panel.SetPositionAndRotation(PanelCourse.Centre(0.0), PanelCourse.Rotation(0.0));
            var panelR = new List<Renderer>(1);
            var panelMat = Make("Megastructure/NocHull", "Dread_Panel");
            if (panelMat != null)
            {
                panelMat.SetColor(BaseColorId, new Color(0.05f, 0.05f, 0.052f, 1f));
                Color e = NocturnePalette.Linear(NocPal.SignWhite) * PanelGlow;
                panelMat.SetVector(EmissionId, new Vector4(e.r, e.g, e.b, 0f));       // linear (SetVector: no colour-space conversion)
                panelR.Add(ctx.AddMesh(panel, "panel", Checked(PanelBuilder.Build(), "Dread_Panel"), panelMat));
            }

            // ---- elevators and gantries (2.2)
            var cabinMat = Make("Megastructure/NocDreadLight", "Dread_Cabins");
            if (cabinMat != null)
            {
                ctx.AddMesh(root, "Dread_Cabins", DreadCabins.CabinMesh(), cabinMat);
                // DR-07: the shafts' guide lights (two steady rows a shaft), so a shaft reads as a structure in a still
                var guides = DreadCabins.GuideLights();
                GuideLights = guides.Count;
                ctx.AddMesh(root, "Dread_ShaftGuides", DreadLights.ToMesh("Dread_ShaftGuides", guides, 10f), cabinMat);
            }
            Cabins = DreadCabins.CabinCount;
            var gantryMat = Make("Megastructure/NocHull", "Dread_Gantry");
            var gantryLightMat = Make("Megastructure/NocDreadLight", "Dread_GantryLights");
            var gantries = new Transform[DreadCabins.Gantries.GetLength(0)];
            if (gantryMat != null)
            {
                gantryMat.SetColor(BaseColorId, new Color(0.045f, 0.043f, 0.040f, 1f));
                Color e = NocturnePalette.Linear(NocPal.WinWarm) * GantryWindowGlow;
                gantryMat.SetVector(EmissionId, new Vector4(e.r, e.g, e.b, 0f));
                Mesh gm = Checked(DreadCabins.GantryMesh(), "Dread_Gantry");
                Mesh gl = DreadLights.ToMesh("Dread_GantryLights", DreadCabins.GantryLights(), 10f);
                for (int i = 0; i < gantries.Length; i++)
                {
                    var g = new GameObject("Dread_Gantry" + i).transform;
                    g.SetParent(root, false);
                    g.position = GantryPos(i, 0.0);
                    ctx.AddMesh(g, "truss", gm, gantryMat);
                    if (gantryLightMat != null) ctx.AddMesh(g, "lights", gl, gantryLightMat);
                    gantries[i] = g;
                }
                ctx.AddMesh(root, "Dread_GantryRails", Checked(DreadCabins.RailMesh(), "Dread_GantryRails"), gantryMat);
            }

            Driver = new DreadDriver(hull, hullR.ToArray(), shadow, shadowR.ToArray(), bodyMat, shadowLightMat, panel, panelR.ToArray(), gantries);
            ctx.Register(Driver);
            ctx.Log(FormattableString.Invariant(
                $"Dread: {Meshes} meshes ({Failures} validation failures); ARK {HullTriangles} triangles, {HullLights} lights, {HullBuilder.GondolaSpecs().Count} gondolas, launch s {HullCourse.LaunchS:F0} (bow {HullCourse.ParkDistance:F0} m north of z 195), bow over z 195 at s {HullCourse.AbeamS(2.5):F1} (2.5 m/s) / {HullCourse.AbeamS(1.5):F1} (1.5), stern at s {HullCourse.SternPassS(2.5):F1} / {HullCourse.SternPassS(1.5):F1}, bow distance KS08 {HullCourse.BowDistance(HullCourse.TauAtS(428.0)):F0} m, KS09 {HullCourse.BowDistance(HullCourse.TauAtS(520.0)):F0} m; shadow {ShadowTriangles} triangles, {ShadowLights} lights, parked centre z {ShadowCourse.ParkCentre:F0}, launch s {ShadowCourse.LaunchS:F0} at {ShadowCourse.Speed:F0} m/s (KS13 centre z {ShadowCourse.CentreZAtS(718.0):F0}); {Cabins} cabins, {GuideLights} guide lights"));
        }

        public static int Cabins, GuideLights;
        /// <summary>The gantry hoist's operator window (WinWarm x 0.5, linear).</summary>
        public const float GantryWindowGlow = 0.5f;

        /// <summary>World position of gantry i's frame (face side of its bottom chord) at scene time t.</summary>
        public static Vector3 GantryPos(int i, double t)
            => new Vector3((float)DreadCabinCourse.GantryX, (float)DreadCabinCourse.GantryY(i), (float)DreadCabinCourse.GantryZ(i, t));

        /// <summary>The falling panel's backlit face: SignWhite x 0.9 (linear; r01's 0.3 read as one more lit window at 800 m).</summary>
        public const float PanelGlow = 0.9f;
        static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        static readonly int WinGainId = Shader.PropertyToID("_WinGain");
        static readonly int PortGainId = Shader.PropertyToID("_PortGain");
        static readonly int CabGainId = Shader.PropertyToID("_CabGain");
        static readonly int CullId = Shader.PropertyToID("_Cull");
        /// <summary>The Ark's procedural lights (NocArk, linear x palette): deck windows 0.6, portholes 0.7, gondola cabins 0.9.</summary>
        public const float ArkWinGain = 0.6f, ArkPortGain = 0.7f, ArkCabGain = 0.9f;
        static readonly int BellyLightId = Shader.PropertyToID("_BellyLight");
        static readonly int SideLightId = Shader.PropertyToID("_SideLight");
        static readonly int TopLightId = Shader.PropertyToID("_TopLight");
        static readonly int SheenId = Shader.PropertyToID("_Sheen");
        /// <summary>The hull's linear paint and ambient fields (C01 fix wave): albedo 0.05 (dark painted steel); light from below the city
        /// and the lit haze 0.15 / 0.112 / 0.075 (the deck over the rift reads luma 50-90: lit by the same city), from the sides the
        /// faces' windows 0.06, from above the deck 0.04. Belly radiance 0.0075 + the floor: luma about 25 under a deck of 50-90.</summary>
        public static readonly Vector4 HullPaint = new Vector4(0.050f, 0.051f, 0.054f, 1f);
        public static readonly Vector4 HullBellyLight = new Vector4(0.150f, 0.112f, 0.075f, 1f);
        public static readonly Vector4 HullSideLight = new Vector4(0.060f, 0.058f, 0.055f, 1f);
        public static readonly Vector4 HullTopLight = new Vector4(0.040f, 0.038f, 0.034f, 1f);
        /// <summary>The shadow's back sheen (raw linear, x n.y^2: the haze light from above on its spine and crest).</summary>
        public static readonly Vector4 BodySheen = new Vector4(0.006f, 0.005f, 0.004f, 1f);   // r09: 0.020 lit the visible back: -22 % only
        static readonly int EmissionId = Shader.PropertyToID("_EmissionColor");

        static Mesh Checked(MeshBuilder mb, string name)
        {
            Mesh m = mb.ToMesh(name);
            var rep = MeshValidation.Validate(m);
            Meshes++;
            if (!rep.Ok) { Failures++; Debug.LogError("[NOC-FAIL] DreadBuilder: " + rep.Describe()); }
            return m;
        }

        static Material Make(string shaderName, string name)
        {
            var sh = Shader.Find(shaderName);
            if (sh == null) { Debug.LogError("[NOC-FAIL] DreadBuilder: shader " + shaderName + " not found"); return null; }
            return new Material(sh) { name = name };
        }
    }
}
