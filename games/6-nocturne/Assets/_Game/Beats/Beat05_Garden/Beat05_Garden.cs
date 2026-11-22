// Beat05_Garden: the Phase-3 graybox of beat 5, the hanging garden (s 880 - 1080; MASTER_PROMPT 14.5.5, the "Graybox" tier plus the
// key-shot subjects of KS14 / KS15 and the crowns KS07 sees): the terrace x 695-914, z 115-275 with its walk surfaces, slab, keel
// and balustrades (GardenTerrace), benches and planters (GardenFurniture), and the 17 trees (9 hero, 8 back; GardenTree) with
// NocFoliage crowns. Light kit of 14.2.3 row 5: 8 leaf point lights (LeafGlow, r 30, i 3) under the hero crowns; wet sources:
// one LeafGlow disc under every crown; wet walls: face B's notch walls (z 115 / 275, G2's geometry) over the beat.
// GD (Phase 4, critique INT-04): the 8 840 tree lanterns (520 per tree, one mesh per tree), the lantern field that lights the
// crown cores and cards (vertex colour A), the 10 lantern posts with their cluster lights (LOOK_V2 6) and the 600 fireflies
// (GardenLanterns). Dimensions: GardenDims. Beat root always active (14.6); meshes validated; no per-frame work.
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Megastructure
{
    public sealed class Beat05_Garden : IBeatBuilder
    {
        public const int BeatIndex = 4;
        public string Name => "Beat05_Garden";

        public void Build(WorldContext ctx)
        {
            Transform root = ctx.BeatRoot(BeatIndex);
            var mats = new GardenMaterials(ctx.Materials);
            long t0 = ctx.Triangles;
            GardenTerrace.Build(ctx, root, mats);
            GardenFurniture.Build(ctx, root, mats);
            BuildTrees(ctx, root, mats);
            if (GardenLanterns.Enabled) BuildLanternPostsAndFireflies(ctx, root, mats);
            RegisterWalls();
            ctx.Log(FormattableString.Invariant(
                $"Beat05_Garden: terrace x {GardenDims.TerraceX0}-{GardenDims.TerraceX1} z {GardenDims.TerraceZ0}-{GardenDims.TerraceZ1}, {GardenDims.Trees.Length} trees, triangles {ctx.Triangles - t0}"));
        }

        /// <summary>LOOK_V2 6 beat 5 (PA): the 8 leaf point lights lerp(LeafGlow, WinNeutral, 0.5) r 30 i 1.0.</summary>
        public const float LeafLightRangeV2 = 30f, LeafLightIntensityV2 = 1.0f;

        static void BuildTrees(WorldContext ctx, Transform root, GardenMaterials m)
        {
            var g = BridgeGardenKit.Group(root, "trees");
            var trunks = new MeshBuilder();
            var limbs = new MeshBuilder();                                       // GD fix wave C33: bark lit by the lantern field
            var crowns = new MeshBuilder();
            var trees = GardenDims.Trees;
            // GD: the lanterns first; their field lights the crown cores and cards (colour A)
            var field = new GardenLanterns.FieldGrid();
            var lanterns = new List<Vector4>[trees.Length];
            for (int i = 0; i < trees.Length; i++)
            {
                lanterns[i] = new List<Vector4>(GardenLanterns.PerTree);
                if (GardenLanterns.Enabled) GardenLanterns.Place(trees[i], 1000 + i, lanterns[i]);
                foreach (var l in lanterns[i]) field.Add(l);
            }
            Color glow = BridgeGardenKit.Lin(NocPal.LeafGlow);
            Color leafLight = Color.Lerp(glow, NocturnePalette.Linear(NocPal.WinNeutral), 0.5f);   // PA (LOOK_V2 6 beat 5)
            leafLight.a = 1f;
            for (int i = 0; i < trees.Length; i++)
            {
                var t = trees[i];
                GardenTree.Build(t, 1000 + i, trunks, crowns, m.Limbs != null ? limbs : null);
                // wet source: a LeafGlow disc under the crown (the glowing underside reflected in the path and moss)
                var src = new Vector3(t.X, t.CrownBottom + 2f, t.CrownZ);
                NocWetRegistry.AddSource(src, src, GardenDims.CrownWetRhoFrac * t.CrownR, glow * GardenDims.CrownWetRadiance, GardenDims.SOf(t.X, t.CrownZ));
                if (t.Hero && i < GardenDims.LeafLights)
                {
                    // leaf point between the trunk and the path, at half the crown-bottom height
                    float toAxis = Mathf.Sign(GardenDims.AxisZ - t.Z);
                    var lp = new Vector3(t.X, 0.5f * t.CrownBottom, t.Z + toAxis * (GardenTree.FootprintRadius(t.R0) + 4f));
                    BridgeGardenKit.PointLight(g, "leaf_" + t.Name, lp, leafLight, LeafLightRangeV2, LeafLightIntensityV2);
                }
            }
            GardenLanterns.SetCrownGlobals(trees);
            GardenLanterns.ApplyField(field, crowns, 0, crowns.VertexCount);
            GardenTree.ForEachPendingCards(mb => GardenLanterns.ApplyField(field, mb, 0, mb.VertexCount));
            GardenLanterns.ApplyField(field, limbs, 0, limbs.VertexCount);
            BridgeGardenKit.Emit(ctx, g, "trunks", trunks, m.Bark);
            BridgeGardenKit.Emit(ctx, g, "limbs", limbs, m.Limbs);
            GardenTree.EmitCrowns(ctx, g, crowns, m);
            int n = 0;
            for (int i = 0; i < trees.Length; i++)
            {
                var mb = new MeshBuilder();
                GardenLanterns.Balls(mb, lanterns[i], GardenLanterns.Radius, trees[i].CrownCentre, trees[i].CrownRadii);
                n += lanterns[i].Count;
                BridgeGardenKit.Emit(ctx, g, "lanterns_" + trees[i].Name, mb, m.Lanterns);
            }
            ctx.Log(FormattableString.Invariant($"Beat05_Garden: {n} tree lanterns in {trees.Length} meshes, field over {field.Count} lanterns"));
        }

        /// <summary>GD: the 10 lantern posts (steel, 3 lanterns each, one cluster light each) and the 600 fireflies.</summary>
        static void BuildLanternPostsAndFireflies(WorldContext ctx, Transform root, GardenMaterials m)
        {
            var g = BridgeGardenKit.Group(root, "lanterns");
            var steel = new MeshBuilder();
            var balls = new MeshBuilder();
            GardenLanterns.BuildPosts(g, steel, balls, null);
            BridgeGardenKit.Emit(ctx, g, "post_steel", steel, m.PostSteel);
            BridgeGardenKit.Emit(ctx, g, "post_lanterns", balls, m.Lanterns);
            var flies = new MeshBuilder();
            int nf = GardenLanterns.BuildFireflies(flies);
            BridgeGardenKit.Emit(ctx, g, "fireflies", flies, m.Fireflies);
            ctx.Log(FormattableString.Invariant($"Beat05_Garden: {GardenLanterns.PostCount} lantern posts, {nf} fireflies"));
        }

        /// <summary>
        /// Face B's notch walls as wet-floor walls (14.7 H4): south z 115 (G2, x 815.3-914, facing north) and north z 275 (G2's wall
        /// x 815.3-834 + G4's screen-slab side x 834-874, facing south), y 0-560. They start fading in at s 892, when G2's four Gulf
        /// face walls (s 400-880 + 12 m fade, G2-D6) are gone, so the 4-wall cap never drops one (no pop).
        /// </summary>
        public static void RegisterWalls()
        {
            double s0 = GardenDims.WallS0, s1 = NocturneConstants.BeatBoundaries[BeatIndex + 1];
            // Look v2 (-surf-v 2, TEXTURE_PLAN 2.11): the notch walls are section mode (GulfFace): FA's section mean at mid-range
            Color macro = NocLookGlobals.SurfV == 2 ? NocSurfaceMath.FacadeMacro(1, NocFacadeMath.RegionD, 0.5 * (s0 + s1)) : BridgeGardenKit.Lin(NocPal.WinCold, 0.03f);
            float xs = 0.5f * (GardenDims.FaceBX + GardenDims.TerraceX1), hs = 0.5f * (GardenDims.TerraceX1 - GardenDims.FaceBX);
            float xn = 0.5f * (GardenDims.FaceBX + GardenDims.PromX0), hn = 0.5f * (GardenDims.PromX0 - GardenDims.FaceBX);
            NocWetRegistry.AddWall(s0, s1, xs, GardenDims.TerraceZ0, 0f, 1f, hs, 560f, macro);
            NocWetRegistry.AddWall(s0, s1, xn, GardenDims.TerraceZ1, 0f, -1f, hn, 560f, macro);
        }
    }
}
