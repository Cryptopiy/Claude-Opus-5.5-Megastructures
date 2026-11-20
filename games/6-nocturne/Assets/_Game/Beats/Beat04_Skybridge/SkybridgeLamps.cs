// SkybridgeLamps: the 21 deck lamps of 14.5.4 (4.2 m posts, LampWarm 3 200 K LED heads) with the light kit of LOOK_V2 6 row 4
// (r 12, i 3.5, shadowless point lights) and one wet-floor area source per head (14.7 H4, D-003: a 0.35 m disc of LampWarm x 1.6,
// so the lamp streaks in the wet deck). Posts stand on brackets bolted to the slab side outside the rails (lateral 2.80, beyond the lane
// wall at 2.53), the arm reaches over the deck to the head at lateral 1.85 and y 4.0-4.1 (above any eye). Alternating sides,
// 14.8 m apart between the gate faces (see SkybridgeDims). Pole and arm are NocLine tubes (thin members). AB (Phase 4 wave A,
// INT-18 "white blobs"): the head is a fixture, a painted-steel housing (0.62 x 0.10 x 0.28, chamfered) with the emissive lens
// (0.50 x 0.012 x 0.20, LampWarm) set into its underside, 6 mm proud of it: the walker (always below the 4 m heads) sees a
// lit lens in a dark housing, never a glowing brick; the halo (NocHalos) and the light stay at the lens.
using UnityEngine;

namespace Megastructure
{
    public static class SkybridgeLamps
    {
        /// <summary>LOOK_V2 6 beat 4: lamp intensity 3.5 (SkybridgeDims keeps the v1 4.0); range 12 m.</summary>
        public const float LampIntensityV2 = 3.5f;

        public static void Build(WorldContext ctx, Transform root, SkybridgeMaterials m)
        {
            var g = BridgeGardenKit.Group(root, "lamps");
            var poles = new MeshBuilder();
            var arms = new MeshBuilder();
            var heads = new MeshBuilder();
            var housings = new MeshBuilder();
            var brackets = new MeshBuilder();
            Color warm = BridgeGardenKit.Lin(NocPal.LampWarm);
            float hh = SkybridgeDims.LampHeight;
            for (int k = 0; k < SkybridgeDims.LampCount; k++)
            {
                float x = SkybridgeDims.LampX(k);
                int side = SkybridgeDims.LampSide(k);                       // +1 = south (right of the walk, -z)
                float zPole = SkybridgeDims.AxisZ - side * SkybridgeDims.LampLateral;
                float zHead = SkybridgeDims.AxisZ - side * SkybridgeDims.LampHeadLateral;
                float zEdge = SkybridgeDims.AxisZ - side * (SkybridgeDims.DeckHalfWidth - 0.02f);   // 2 cm into the slab side
                float zOut = SkybridgeDims.AxisZ - side * (SkybridgeDims.LampLateral + 0.06f);
                float zArm = SkybridgeDims.AxisZ - side * (SkybridgeDims.LampHeadLateral - 0.1f);
                // bracket from the slab side to under the pole
                MeshKit.ChamferBox(brackets, new Vector3(x, -0.275f, 0.5f * (zEdge + zOut)), new Vector3(0.2f, 0.35f, Mathf.Abs(zOut - zEdge)), 0.01f);
                MeshKit.Cylinder(poles, new Vector3(x, -0.42f, zPole), new Vector3(x, hh, zPole), 0.5f * SkybridgeDims.LampPoleD, 10, false);
                float ay = hh - 0.08f;
                MeshKit.Cylinder(arms, new Vector3(x, ay, zPole), new Vector3(x, ay, zArm), 0.5f * SkybridgeDims.LampArmD, 8, false);
                var headC = new Vector3(x, hh - 0.15f, zHead);
                MeshKit.ChamferBox(housings, headC, new Vector3(0.62f, 0.10f, 0.28f), 0.012f);                        // housing
                MeshKit.ChamferBox(heads, headC + new Vector3(0f, -0.050f, 0f), new Vector3(0.50f, 0.012f, 0.20f), 0.003f);   // lens
                NocHalos.Queue("SkybridgeLamps", headC + new Vector3(0f, -0.056f, 0f), warm * NocturnePalette.EmissionSign, SkybridgeDims.LampRange);   // LOOK_V2 6 halo (at the lens)
                var lightPos = new Vector3(x, hh - 0.35f, zHead);
                BridgeGardenKit.PointLight(g, "lamp_" + k.ToString("00", System.Globalization.CultureInfo.InvariantCulture), lightPos, warm,
                    SkybridgeDims.LampRange, LampIntensityV2);
                var src = new Vector3(x, hh - 0.21f, zHead);
                NocWetRegistry.AddSource(src, src, SkybridgeDims.LampWetRho, warm * NocturnePalette.EmissionSign, SkybridgeDims.SOfX(x));
            }
            BridgeGardenKit.Emit(ctx, g, "brackets", brackets, m.Steel);
            BridgeGardenKit.Emit(ctx, g, "poles", poles, m.LampPole);
            BridgeGardenKit.Emit(ctx, g, "arms", arms, m.LampArm);
            BridgeGardenKit.Emit(ctx, g, "housings", housings, m.Steel);
            BridgeGardenKit.Emit(ctx, g, "heads", heads, m.LampHead);
        }
    }
}
