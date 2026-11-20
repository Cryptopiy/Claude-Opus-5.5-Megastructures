// PlazaFurnitureBuilder: the street furniture of Heart Square (MASTER_PROMPT 14.5.7: twenty plaza lamps 6 m, WinCold, no amber;
// 14.2.3 row 7: r 12 i 3; 14.4: visible barriers behind the invisible lane walls). Two lamp rows 16 m either side of the east leg
// (x 950 - 1 130 every 20 m), each lamp a NocLine post, a WinCold head (x 2, an H4 source) and a real light 0.35 m under the head;
// bollards 0.9 m every 6 m on both lane edges from the beat boundary (s 1 260) to the goal (they follow the 12 m lane and its
// funnel to 3.0 at the tenement, 0.65 m outside the lane edge); a low planter 1.3 - 2.3 m beyond the goal behind the PathGuide end
// cap (below the KS19 frame: its hedge top at 0.65 m is far under the frame's lowest ray there).
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
        public const double PlazaLampIntensityV2 = 2.5, PlazaLampHeadGainV2 = 1.6;

        public static void Build(WorldContext ctx, Transform root)
        {
            var lib = ctx.Materials;
            Color cold = G4Kit.Lin(NocPal.SignWhite);       // LOOK_V2 6 beat 7: 20 plaza lamps SignWhite r 12 i 2.5
            var posts = new MeshBuilder();
            var heads = new MeshBuilder();
            float h = (float)HeartLayout.PlazaLampH;
            for (int i = 0; i < HeartLayout.PlazaLampCount; i++)
            {
                Vector3 p = LampPosition(i);
                MeshKit.Cylinder(posts, p - Origin + new Vector3(0f, -0.05f, 0f), p - Origin + new Vector3(0f, h, 0f), 0.09f, 8, false);
                MeshKit.ChamferBox(heads, p - Origin + new Vector3(0f, h + 0.075f, 0f), new Vector3(0.7f, 0.25f, 0.7f), 0.02f);
                Vector3 head = p + new Vector3(0f, h + 0.1f, 0f);
                G4Kit.PointLight(root, "plazaLamp" + i, head - new Vector3(0f, 0.35f, 0f), cold, HeartLayout.PlazaLampRange, PlazaLampIntensityV2);
                NocWetRegistry.AddSource(head, head, 0.3f, G4Kit.Scale(cold, PlazaLampHeadGainV2));
                NocHalos.Queue("PlazaLamps", head, G4Kit.Scale(cold, PlazaLampHeadGainV2), HeartLayout.PlazaLampRange);   // LOOK_V2 6 halo
            }
            Color metal = G4Kit.Lin(NocPal.MetalPaint);
            G4Kit.Add(ctx, root, "plazaLampPosts", posts, lib != null ? G4Kit.Line(lib.Line, 0.18, metal, Color.black, "Beat07_LampPost") : null, Origin);
            G4Kit.Add(ctx, root, "plazaLampHeads", heads, lib != null ? G4Kit.Emissive(lib.SteelPainted, cold, PlazaLampHeadGainV2, "Beat07_LampHead") : null, Origin);

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
            ctx.Log(System.FormattableString.Invariant($"Beat07 plaza: {HeartLayout.PlazaLampCount} lamps, {pts.Count} bollards"));
        }
    }
}
