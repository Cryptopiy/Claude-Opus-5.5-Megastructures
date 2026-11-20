// PromenadeBuilder: the walking surface and street furniture of beat 6 (MASTER_PROMPT 14.4, 14.5.6 Geometry, 14.2.3 row 6, H4):
//  - the promenade floor x 874 - 914, z 275 - 520 (G3's terrace ends at z 275, geometry agreement 6): one 1 m slab, top y 0,
//    NocWet wet stone (lib.FloorFor(s) of beat 6); its west edge reaches 1 m into the screen-wall mass, its east face x 914 is
//    back to back with the arcade and the heart floor; it covers the lane + 1 m on the straight leg and the C3 corner west of
//    x 914 (the corner east of x 914 is the beat-7 heart floor);
//  - the planter row x 912.2 - 913.6 (the visible barrier of the lane x 891.7 - 911.7), 5.4 m planters with hedges, 0.6 m gaps
//    every 6 m from z 276 to 354;
//  - 12 lamp posts 6 m at x 912.9 (in planter gaps on the straight leg, z 282 - 354; north of the corner z 408 - 516): SG Phase 4
//    fixtures (a base plate, the NocLine post, an arm toward the walk, a luminaire housing with a SignWhite lens facing down),
//    each a real light (SignWhite r 12 i 3) under the lens, an H4 source and an in-air halo (NocHalos);
//  - planters with a granite coping; bollards 0.9 m every 6 m on the outer lane edge (west of the straight leg and the outside of
//    C3) and on the inside of C3 up to the beat boundary (beat 7 continues both lines): round painted steel with a domed cap,
//    every third one with a dim cool light ring.
// Bevels 2 cm on everything within 3 m of the lane (8.7); floors are plain boxes (flush seams with the neighbouring floors).
using System.Collections.Generic;
using UnityEngine;

namespace Megastructure
{
    public static class PromenadeBuilder
    {
        public static readonly Vector3 Origin = new Vector3(894f, 0f, 397.5f);

        /// <summary>The floor slab x 873 - 916, z 275 - 520, y -1..0 (relative to Origin).</summary>
        public static MeshBuilder Floor()
        {
            var mb = new MeshBuilder();
            float x0 = (float)SignsLayout.PromSlabX0, x1 = (float)SignsLayout.PromSlabX1, z0 = (float)SignsLayout.PromZ0, z1 = (float)SignsLayout.PromZ1;
            float t = (float)SignsLayout.FloorThickness;
            mb.Box(new Vector3(0.5f * (x0 + x1), -0.5f * t, 0.5f * (z0 + z1)) - Origin, new Vector3(x1 - x0, t, z1 - z0));
            return mb;
        }

        /// <summary>Centre z of planter k (between the gaps at z 276 + 6 k and 276 + 6 (k + 1)).</summary>
        public static int PlanterCount => (int)System.Math.Round((SignsLayout.PlanterGapZ1 - SignsLayout.PlanterGapZ0) / SignsLayout.PlanterPitch);

        public static void Planters(MeshBuilder boxes, MeshBuilder hedges) => Planters(boxes, hedges, null);

        public static void Planters(MeshBuilder boxes, MeshBuilder hedges, MeshBuilder coping)
        {
            float x0 = (float)SignsLayout.PlanterX0, x1 = (float)SignsLayout.PlanterX1, w = x1 - x0;
            float len = (float)(SignsLayout.PlanterPitch - SignsLayout.PlanterGap), h = (float)SignsLayout.PlanterH;
            float hh = (float)SignsLayout.HedgeH, inset = (float)SignsLayout.HedgeInset;
            for (int k = 0; k < PlanterCount; k++)
            {
                float zc = (float)(SignsLayout.PlanterGapZ0 + SignsLayout.PlanterPitch * (k + 0.5));
                // 2 cm below the floor top: the box base is never co-planar with the floor
                MeshKit.ChamferBox(boxes, new Vector3(0.5f * (x0 + x1), 0.5f * h - 0.02f, zc) - Origin, new Vector3(w, h + 0.04f, len), (float)SignsLayout.Bevel);
                MeshKit.ChamferBox(hedges, new Vector3(0.5f * (x0 + x1), h + 0.5f * hh - 0.05f, zc) - Origin, new Vector3(w - 2f * inset, hh + 0.1f, len - 2f * inset), (float)SignsLayout.Bevel);
                if (coping != null)
                {
                    float cp = (float)SignsLayout.CopingProud, ch = (float)SignsLayout.CopingH;
                    MeshKit.ChamferBox(coping, new Vector3(0.5f * (x0 + x1), h + 0.5f * ch - 0.01f, zc) - Origin, new Vector3(w + 2f * cp, ch, len + 2f * cp), 0.015f);
                }
            }
        }

        public static Vector3 LampPosition(int i) => new Vector3((float)SignsLayout.LampX, 0f, (float)SignsLayout.LampZ[i]);

        public static void Lamps(MeshBuilder posts, MeshBuilder heads) => Lamps(posts, heads, null);

        /// <summary>The lamp fixtures: posts (NocLine tubes, 0.16 m), housings (steel: base plate, arm, luminaire, cap) into `heads`,
        /// and the lenses (emissive quads facing down) into `lenses` (null: the lens rides with the housing).</summary>
        public static void Lamps(MeshBuilder posts, MeshBuilder heads, MeshBuilder lenses)
        {
            float r = 0.5f * (float)SignsLayout.LampPostWidth, h = (float)SignsLayout.LampH;
            for (int i = 0; i < SignsLayout.LampZ.Length; i++)
            {
                Vector3 p = LampPosition(i) - Origin;
                // NocLine tube: open cylinder with radial normals (the tube width is the material's _LineWidth)
                MeshKit.Cylinder(posts, p + new Vector3(0f, -0.05f, 0f), p + new Vector3(0f, h, 0f), r, 8, false);
                float bs = (float)SignsLayout.LampBase;
                MeshKit.ChamferBox(heads, p + new Vector3(0f, 0.5f * (float)SignsLayout.LampBaseH - 0.02f, 0f), new Vector3(bs, (float)SignsLayout.LampBaseH + 0.04f, bs), 0.02f);
                MeshKit.ChamferBox(heads, p + new Vector3(0f, h + 0.04f, 0f), new Vector3(0.2f, 0.1f, 0.2f), 0.02f);
                Vector3 lum = LensCentre(i) - Origin + new Vector3(0f, 0.5f * (float)SignsLayout.LumH + 0.005f, 0f);
                MeshKit.ChamferMember(heads, p + new Vector3(0f, (float)SignsLayout.LampArmY, 0f), lum + new Vector3(0.2f, 0f, 0f), 0.07f, 0.09f, 0.01f, Vector3.up);
                MeshKit.ChamferBox(heads, lum, new Vector3((float)SignsLayout.LumW, (float)SignsLayout.LumH, (float)SignsLayout.LumD), 0.02f);
                Vector3 c = LensCentre(i) - Origin;
                float lw = 0.5f * (float)SignsLayout.LensW, ld = 0.5f * (float)SignsLayout.LensD;
                G4Kit.OrientedQuad(lenses ?? heads, c + new Vector3(-lw, 0f, -ld), c + new Vector3(lw, 0f, -ld), c + new Vector3(lw, 0f, ld), c + new Vector3(-lw, 0f, ld), Vector3.down);
            }
        }

        /// <summary>World centre of lamp i's lens (the luminaire hangs 0.55 m toward the walk from the post at y 5.9; x 912.35).</summary>
        public static Vector3 LensCentre(int i)
            => LampPosition(i) + new Vector3(-(float)SignsLayout.LampArm, (float)SignsLayout.LampArmY - 0.01f, 0f);

        /// <summary>Bollard foot points of beat 6: outer edge s 1 154 - 1 260 (left of the walk), inner edge of C3 s 1 236.4 - 1 260.</summary>
        public static void BollardPoints(List<Vector3> pts, List<float> yaws)
        {
            G4Kit.AlongEdge(1154.0, NocturneConstants.BeatBoundaries[6], -1, SignsLayout.BollardOffset, SignsLayout.BollardStep, pts, yaws);
            G4Kit.AlongEdge(SignsLayout.StraightS1, NocturneConstants.BeatBoundaries[6], +1, SignsLayout.BollardOffset, SignsLayout.BollardStep, pts, yaws);
        }

        /// <summary>Round painted-steel bollards (r 0.11 -> 0.10 m, 12 sides) with a domed cap, feet 2 cm below the floor (HT's plaza
        /// line uses the same bollard).</summary>
        public static void Bollards(MeshBuilder mb, List<Vector3> pts, List<float> yaws, Vector3 origin)
        {
            float h = (float)SignsLayout.BollardH, r = 0.5f * (float)SignsLayout.BollardW;
            for (int i = 0; i < pts.Count; i++)
            {
                Vector3 b = pts[i] - origin;
                MeshKit.TaperedCylinder(mb, b + new Vector3(0f, -0.02f, 0f), b + new Vector3(0f, h - 0.05f, 0f), r, r - 0.01f, 12, true);
                MeshKit.TaperedCylinder(mb, b + new Vector3(0f, h - 0.05f, 0f), b + new Vector3(0f, h, 0f), r - 0.005f, 0.6f * r, 12, true);
            }
        }

        /// <summary>The light rings of every third bollard (a band 6 cm tall, 4 mm proud, at y 0.70-0.76).</summary>
        public static int BollardRings(MeshBuilder mb, List<Vector3> pts, Vector3 origin)
        {
            int n = 0;
            float r = 0.5f * (float)SignsLayout.BollardW + 0.004f;
            for (int i = 1; i < pts.Count; i += SignsLayout.BollardRingEvery)
            {
                Vector3 b = pts[i] - origin;
                MeshKit.Cylinder(mb, b + new Vector3(0f, (float)SignsLayout.BollardRingY0, 0f), b + new Vector3(0f, (float)SignsLayout.BollardRingY1, 0f), r - 0.003f, 12, false);
                n++;
            }
            return n;
        }

        /// <summary>LOOK_V2 2.3 / 6: promenade lamp heads SignWhite x 1.6 (SignsLayout keeps the v1 2.0); lights SignWhite r 12 i 3.</summary>
        public const double LampHeadGainV2 = 1.6;

        public static void Build(WorldContext ctx, Transform root)
        {
            var lib = ctx.Materials;
            double sMid = 0.5 * (NocturneConstants.BeatBoundaries[5] + NocturneConstants.BeatBoundaries[6]);
            G4Kit.Add(ctx, root, "promenadeFloor", Floor(), lib != null ? lib.FloorFor(sMid) : null, Origin);

            var planters = new MeshBuilder();
            var hedges = new MeshBuilder();
            var coping = new MeshBuilder();
            Planters(planters, hedges, coping);
            G4Kit.Add(ctx, root, "planters", planters, lib != null ? lib.Concrete : null, Origin);
            G4Kit.Add(ctx, root, "hedges", hedges, lib != null ? lib.Moss : null, Origin);
            G4Kit.Add(ctx, root, "planterCoping", coping, lib != null ? (lib.StoneWall != null ? lib.StoneWall : lib.Concrete) : null, Origin);

            var posts = new MeshBuilder();
            var heads = new MeshBuilder();
            var lenses = new MeshBuilder();
            Lamps(posts, heads, lenses);
            Color white = G4Kit.Lin(NocPal.SignWhite);
            Material postMat = lib != null ? G4Kit.Line(lib.Line, SignsLayout.LampPostWidth, G4Kit.Lin(NocPal.MetalPaint), Color.black, "Beat06_LampPost") : null;
            Material lensMat = lib != null ? G4Kit.Emissive(lib.SteelPainted, white, LampHeadGainV2, "Beat06_LampLens") : null;
            G4Kit.Add(ctx, root, "lampPosts", posts, postMat, Origin);
            G4Kit.Add(ctx, root, "lampFixtures", heads, lib != null ? lib.SteelPainted : null, Origin);
            G4Kit.Add(ctx, root, "lampLenses", lenses, lensMat, Origin);
            for (int i = 0; i < SignsLayout.LampZ.Length; i++)
            {
                Vector3 lens = LensCentre(i);
                G4Kit.PointLight(root, "lampLight" + i, lens - new Vector3(0f, 0.12f, 0f), white, SignsLayout.LampRange, SignsLayout.LampIntensity);
                NocWetRegistry.AddSource(lens, lens, (float)SignsLayout.LampSourceRho, G4Kit.Scale(white, LampHeadGainV2));
                NocHalos.Queue("PromenadeLamps", lens - new Vector3(0f, 0.05f, 0f), G4Kit.Scale(white, LampHeadGainV2), SignsLayout.LampRange);   // LOOK_V2 6 halo
            }

            var pts = new List<Vector3>(64);
            var yaws = new List<float>(64);
            BollardPoints(pts, yaws);
            var bollards = new MeshBuilder();
            Bollards(bollards, pts, yaws, Origin);
            G4Kit.Add(ctx, root, "bollards", bollards, lib != null ? lib.SteelPainted : null, Origin);
            var rings = new MeshBuilder();
            int nr = BollardRings(rings, pts, Origin);
            G4Kit.Add(ctx, root, "bollardRings", rings, lib != null ? G4Kit.Emissive(lib.SteelPainted, G4Kit.Lin(NocPal.SignWhite), SignsLayout.BollardRingGain, "Beat06_BollardRing") : null, Origin);
            ctx.Log(System.FormattableString.Invariant($"Beat06 promenade: floor x {SignsLayout.PromX0}-{SignsLayout.PromX1}, z {SignsLayout.PromZ0}-{SignsLayout.PromZ1}; {PlanterCount} planters, {SignsLayout.LampZ.Length} lamps (x {SignsLayout.LampX}), {pts.Count} bollards ({nr} lit)"));
        }
    }
}
