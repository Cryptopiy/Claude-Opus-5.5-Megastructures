// PlazaFurnitureBuilder: the street furniture of Heart Square (MASTER_PROMPT 14.5.7: twenty plaza lamps 6 m, WinCold, no amber;
// 14.2.3 row 7: r 12 i 3; 14.4: visible barriers behind the invisible lane walls). Two lamp rows 16 m either side of the east leg
// (x 950 - 1 130 every 20 m), each lamp a NocLine post, a WinCold head (x 2, an H4 source) and a real light 0.35 m under the head;
// bollards 0.9 m every 6 m on both lane edges from the beat boundary (s 1 260) to the goal (they follow the 12 m lane and its
// funnel to 3.0 at the tenement, 0.65 m outside the lane edge); a low planter 1.3 - 2.3 m beyond the goal behind the PathGuide end
// cap (below the KS19 frame: its hedge top at 0.65 m is far under the frame's lowest ray there).
// HT (INT-12, Phase 4 wave A): each lamp is a real fixture (a lathe: cast plinth, tapered pole, collar; a dark disc luminaire
// housing over a frosted bowl, SignWhite x 2.0 (HT-05: a flat lens was a 1-2 px sliver from afar); the light 0.35 m under the
// housing rim, the halo and the H4 source at the bowl's centre), and ten granite benches with slatted wooden seats stand
// between the lamp columns, 2.5 m outside the lane (scale cues).
using System.Collections.Generic;
using UnityEngine;

namespace Megastructure
{
    public static class PlazaFurnitureBuilder
    {
        public static readonly Vector3 Origin = new Vector3(1040f, 0f, (float)HeartLayout.EastLegZ);

        /// <summary>World foot point of plaza lamp i (0..19): row i % 2 (0 south, 1 north), column i / 2.</summary>
        public static Vector3 LampPosition(int i)
        {
            int col = i / 2, side = (i % 2) * 2 - 1;
            return new Vector3((float)(HeartLayout.PlazaLampX0 + HeartLayout.PlazaLampStep * col), 0f, (float)(HeartLayout.EastLegZ + side * HeartLayout.PlazaLampLateral));
        }

        public static void BollardPoints(List<Vector3> pts, List<float> yaws)
        {
            double s0 = NocturneConstants.BeatBoundaries[6], s1 = NocturneConstants.GoalS;
            G4Kit.AlongEdge(s0, s1, -1, 0.65, 6.0, pts, yaws);
            G4Kit.AlongEdge(s0, s1, +1, 0.65, 6.0, pts, yaws);
        }

        /// <summary>LOOK_V2 6 / 2.3: plaza lamps i 2.5 (HeartLayout keeps the v1 3.0), heads SignWhite x 1.6 (v1 2.0).</summary>
        /// Fix wave (lead ruling HT-12, D-040): brighter lamp pools on the dark uniform plaza floor, i 2.5 -> 7.0 (HT_r09 at 4.0: the pools
        /// still faint against the wet granite; r 12 kept; not
        /// brighter windows); the facade planes in the wet mirror: OldQuarterBuilder.RegisterPlazaWalls.
        public const double PlazaLampIntensityV2 = 7.0, PlazaLampHeadGainV2 = 1.6;

        /// <summary>The lamp fixture's lathe profiles (radius, height; bottom to top along the outside): plinth, pole and collar;
        /// the luminaire housing from its lens rim up over the cap to the axis (open underneath, where the lens sits).</summary>
        static readonly Vector2[] s_lampProfile =
        {
            new Vector2(0.20f, -0.05f), new Vector2(0.20f, 0.40f), new Vector2(0.17f, 0.45f), new Vector2(0.11f, 0.50f),
            new Vector2(0.09f, 0.56f), new Vector2(0.055f, 5.50f), new Vector2(0.075f, 5.52f), new Vector2(0.075f, 5.60f),
            new Vector2(0.06f, 5.62f), new Vector2(0.06f, 5.72f),
        };
        static readonly Vector2[] s_housingProfile =
        {
            new Vector2(0.31f, 5.705f), new Vector2(0.42f, 5.74f), new Vector2(0.42f, 5.80f), new Vector2(0.33f, 5.88f),
            new Vector2(0.12f, 5.96f), new Vector2(0.0f, 5.975f),
        };
        /// <summary>The frosted bowl under the housing (HT-05): from the axis at LampBowlY up to the housing's rim (R 0.31 at
        /// LampLensY), so the lit body is 0.22 m tall seen from the side.</summary>
        static readonly Vector2[] s_bowlProfile =
        {
            new Vector2(0.0f, (float)HeartLayout.LampBowlY), new Vector2(0.22f, 5.50f), new Vector2(0.285f, 5.54f),
            new Vector2(0.31f, 5.62f), new Vector2(0.31f, (float)HeartLayout.LampLensY + 0.005f),
        };

        /// <summary>One lamp fixture at its foot point (relative to Origin): the dark lathes into `body`, the frosted bowl into `lens`.</summary>
        public static void LampFixture(MeshBuilder body, MeshBuilder lens, Vector3 foot)
        {
            body.Push(foot, Quaternion.identity);
            MeshKit.Lathe(body, s_lampProfile, 16);
            MeshKit.Lathe(body, s_housingProfile, 24);
            body.Pop();
            lens.Push(foot, Quaternion.identity);
            MeshKit.Lathe(lens, s_bowlProfile, 24);
            lens.Pop();
        }

        /// <summary>World centre of bench k (0 .. 2 BenchPerRow - 1): row k % 2 (0 south, 1 north), column k / 2.</summary>
        public static Vector3 BenchPosition(int k)
        {
            int col = k / 2, side = (k % 2) * 2 - 1;
            return new Vector3((float)(HeartLayout.BenchX0 + HeartLayout.BenchStep * col), 0f, (float)(HeartLayout.EastLegZ + side * HeartLayout.BenchLateral));
        }

        /// <summary>The benches (relative to Origin): two granite legs (`stone`) and a seat of slats along x (`wood`).</summary>
        public static void Benches(MeshBuilder stone, MeshBuilder wood)
        {
            float L = (float)HeartLayout.BenchL, D = (float)HeartLayout.BenchD, H = (float)HeartLayout.BenchH;
            for (int k = 0; k < 2 * HeartLayout.BenchPerRow; k++)
            {
                Vector3 c = BenchPosition(k) - Origin;
                float legH = H - 0.06f;
                for (int e = -1; e <= 1; e += 2)
                    MeshKit.ChamferBox(stone, c + new Vector3(e * (0.5f * L - 0.3f), 0.5f * legH - 0.02f, 0f), new Vector3(0.36f, legH + 0.04f, D - 0.06f), 0.015f);
                int n = HeartLayout.BenchSlats;
                float gap = 0.012f, sw = (D - gap * (n - 1)) / n;
                for (int i = 0; i < n; i++)
                {
                    float z = -0.5f * D + 0.5f * sw + i * (sw + gap);
                    MeshKit.ChamferBox(wood, c + new Vector3(0f, H - 0.03f, z), new Vector3(L, 0.05f, sw), 0.008f);
                }
            }
        }

        public static void Build(WorldContext ctx, Transform root)
        {
            var lib = ctx.Materials;
            Color cold = G4Kit.Lin(NocPal.SignWhite);       // LOOK_V2 6 beat 7: 20 plaza lamps SignWhite r 12 i 2.5
            var bodies = new MeshBuilder();
            var lenses = new MeshBuilder();
            for (int i = 0; i < HeartLayout.PlazaLampCount; i++)
            {
                Vector3 p = LampPosition(i);
                LampFixture(bodies, lenses, p - Origin);
                Vector3 lens = p + new Vector3(0f, (float)HeartLayout.LampGlowY, 0f);
                G4Kit.PointLight(root, "plazaLamp" + i, p + new Vector3(0f, (float)(HeartLayout.LampLensY - HeartLayout.LampLightDrop), 0f), cold, HeartLayout.PlazaLampRange, PlazaLampIntensityV2);
                NocWetRegistry.AddSource(lens, lens, 0.3f, G4Kit.Scale(cold, PlazaLampHeadGainV2));
                NocHalos.Queue("PlazaLamps", lens, G4Kit.Scale(cold, PlazaLampHeadGainV2), HeartLayout.PlazaLampRange);   // LOOK_V2 6 halo
            }
            G4Kit.Add(ctx, root, "plazaLampPosts", bodies, lib != null ? lib.SteelPainted : null, Origin);
            G4Kit.Add(ctx, root, "plazaLampHeads", lenses, lib != null ? G4Kit.Emissive(lib.SteelPainted, cold, HeartLayout.LampLensGain, "Beat07_LampLens") : null, Origin);
            var benchStone = new MeshBuilder();
            var benchWood = new MeshBuilder();
            Benches(benchStone, benchWood);
            G4Kit.Add(ctx, root, "plazaBenches", benchStone, lib != null ? lib.StoneWall : null, Origin);
            Material wood = lib != null ? (lib.Wood != null ? lib.Wood : lib.SteelPainted) : null;
            G4Kit.Add(ctx, root, "plazaBenchSeats", benchWood, wood, Origin);

            var pts = new List<Vector3>(128);
            var yaws = new List<float>(128);
            BollardPoints(pts, yaws);
            var bollards = new MeshBuilder();
            PromenadeBuilder.Bollards(bollards, pts, yaws, Origin);
            // the end planter across the walk beyond the goal (x 1 149.5 - 1 150.5)
            var end = new MeshBuilder();
            float x0 = (float)HeartLayout.EndPlanterX0, x1 = (float)HeartLayout.EndPlanterX1, eh = (float)HeartLayout.EndPlanterH;
            MeshKit.ChamferBox(end, new Vector3(0.5f * (x0 + x1), 0.5f * eh - 0.02f, (float)HeartLayout.EastLegZ) - Origin,
                new Vector3(x1 - x0, eh + 0.04f, 2f * (float)HeartLayout.EndPlanterHalfZ), 0.02f);
            var hedge = new MeshBuilder();
            MeshKit.ChamferBox(hedge, new Vector3(0.5f * (x0 + x1), eh + 0.5f * (float)HeartLayout.EndHedgeH - 0.05f, (float)HeartLayout.EastLegZ) - Origin,
                new Vector3(x1 - x0 - 0.2f, (float)HeartLayout.EndHedgeH + 0.1f, 2f * (float)HeartLayout.EndPlanterHalfZ - 0.2f), 0.02f);
            G4Kit.Add(ctx, root, "plazaBollards", bollards, lib != null ? lib.SteelPainted : null, Origin);
            G4Kit.Add(ctx, root, "endPlanter", end, lib != null ? lib.Concrete : null, Origin);
            G4Kit.Add(ctx, root, "endHedge", hedge, lib != null ? lib.Moss : null, Origin);
            ctx.Log(System.FormattableString.Invariant($"Beat07 plaza: {HeartLayout.PlazaLampCount} lamp fixtures, {2 * HeartLayout.BenchPerRow} benches, {pts.Count} bollards"));
        }
    }
}
