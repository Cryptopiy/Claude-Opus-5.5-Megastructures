// GardenFurniture: the 12 benches (0.45 m) and the planters of 14.5.5 along the straight garden path, all outside the lane walls
// (lane 6.0 + 0.38: benches at lateral 7.6, planters at 6.9 with their inner face at 6.5), so they are the visible barrier of the
// lane and never meet the player's capsule or the camera sphere. Planters are wet stone, the hedges on them the Moss surface.
// GD (Phase 4, task 5): a bench is a wooden seat (lib.Wood, 0.5 x 0.06 x 1.8 m, top at 0.45 m) on two stone plinths (0.42 x 0.41 x
// 0.42 m, their tops 1 cm inside the seat), lit by the lantern posts. Chamfered boxes (1.5-2 cm bevels), bottoms 1-2 cm inside the
// floor (no co-planar faces).
using UnityEngine;

namespace Megastructure
{
    public static class GardenFurniture
    {
        public static void Build(WorldContext ctx, Transform root, GardenMaterials m)
        {
            var g = BridgeGardenKit.Group(root, "furniture");
            var stone = new MeshBuilder();
            var hedge = new MeshBuilder();
            var wood = new MeshBuilder();
            for (int k = 0; k < GardenDims.BenchCount; k++)
            {
                double s = GardenDims.BenchS0 + GardenDims.BenchPitch * k;
                int side = (k & 1) == 0 ? 1 : -1;
                Vector3 p = NocLanes.CorridorToWorld(s, side * GardenDims.BenchLateral, 0.0);
                float yaw = NocLanes.Path.HeadingAt(s);
                var rot = Quaternion.Euler(0f, yaw, 0f);
                float seatH = BenchSeatThick, seatY = GardenDims.BenchHeight - 0.5f * seatH;
                MeshKit.ChamferBox(wood, p + new Vector3(0f, seatY, 0f), new Vector3(GardenDims.BenchDepth, seatH, GardenDims.BenchLength), 0.015f, rot);
                float legTop = GardenDims.BenchHeight - seatH + 0.01f, legH = legTop + 0.01f;
                for (int e = -1; e <= 1; e += 2)
                    MeshKit.ChamferBox(stone, p + rot * new Vector3(0f, 0f, e * BenchLegOffset) + new Vector3(0f, 0.5f * legH - 0.01f, 0f),
                        new Vector3(BenchLegSize, legH, BenchLegSize), 0.02f, rot);
            }
            for (int k = 0; ; k++)
            {
                double s = GardenDims.PlanterS0 + GardenDims.PlanterPitch * k;
                if (s > GardenDims.ArcS0 - 4.0) break;
                for (int side = -1; side <= 1; side += 2)
                {
                    float lat = GardenDims.PlanterLateral;
                    Vector3 p = NocLanes.CorridorToWorld(s, side * lat, 0.0);
                    float yaw = NocLanes.Path.HeadingAt(s);
                    var rot = Quaternion.Euler(0f, yaw, 0f);
                    float ph = GardenDims.PlanterHeight + 0.02f;
                    MeshKit.ChamferBox(stone, p + new Vector3(0f, 0.5f * ph - 0.02f, 0f), new Vector3(GardenDims.PlanterDepth, ph, GardenDims.PlanterLength), 0.02f, rot);
                    MeshKit.ChamferBox(hedge, p + new Vector3(0f, GardenDims.PlanterHeight + 0.15f, 0f), new Vector3(GardenDims.PlanterDepth - 0.2f, 0.4f, GardenDims.PlanterLength - 0.3f), 0.05f, rot);
                }
            }
            BridgeGardenKit.Emit(ctx, g, "benchesPlanters", stone, m.Stone);
            BridgeGardenKit.Emit(ctx, g, "benchSeats", wood, m.Wood != null ? m.Wood : m.Stone);
            BridgeGardenKit.Emit(ctx, g, "hedges", hedge, m.Hedge);
        }

        /// <summary>GD: bench seat thickness, plinth size and plinth offset from the bench centre along its length (m).</summary>
        public const float BenchSeatThick = 0.06f, BenchLegSize = 0.42f, BenchLegOffset = 0.62f;

        /// <summary>Planter count along the straight path (both sides).</summary>
        public static int PlanterCount()
        {
            int n = 0;
            for (int k = 0; GardenDims.PlanterS0 + GardenDims.PlanterPitch * k <= GardenDims.ArcS0 - 4.0; k++) n += 2;
            return n;
        }
    }
}
