// GardenFurniture: the 12 benches (0.45 m) and the planters of 14.5.5 along the straight garden path, all outside the lane walls
// (lane 6.0 + 0.38: benches at lateral 7.6, planters at 6.9 with their inner face at 6.5), so they are the visible barrier of the
// lane and never meet the player's capsule or the camera sphere. Benches and planters are wet stone, the hedges on the planters
// the Moss surface. Chamfered boxes (2 cm bevels), bottoms 1-2 cm inside the floor (no co-planar faces).
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
            for (int k = 0; k < GardenDims.BenchCount; k++)
            {
                double s = GardenDims.BenchS0 + GardenDims.BenchPitch * k;
                int side = (k & 1) == 0 ? 1 : -1;
                Vector3 p = NocLanes.CorridorToWorld(s, side * GardenDims.BenchLateral, 0.0);
                float yaw = NocLanes.Path.HeadingAt(s);
                float hgt = GardenDims.BenchHeight + 0.01f;
                MeshKit.ChamferBox(stone, p + new Vector3(0f, 0.5f * hgt - 0.01f, 0f), new Vector3(GardenDims.BenchDepth, hgt, GardenDims.BenchLength), 0.02f, Quaternion.Euler(0f, yaw, 0f));
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
            BridgeGardenKit.Emit(ctx, g, "hedges", hedge, m.Hedge);
        }

        /// <summary>Planter count along the straight path (both sides).</summary>
        public static int PlanterCount()
        {
            int n = 0;
            for (int k = 0; GardenDims.PlanterS0 + GardenDims.PlanterPitch * k <= GardenDims.ArcS0 - 4.0; k++) n += 2;
            return n;
        }
    }
}
