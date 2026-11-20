// SkybridgeTubes: the glass train tubes of 14.5.4 as plain open cylinders (graybox tier) with NocGlass on two renderers (back
// faces at queue 3000, then front faces at 3001: D-008, H6 order), plus a pair of thin rails (NocLine, 0.07 m, at the tube axis
// +- 0.8 m, just above the glass bottom) in the two bridge tubes so they read as railways. G3: an opaque 2.9 m track bed read as a
// black crack across the sky in the face tubes and took 0.08 of KS12's drop share (V's audit) in the bridge tubes. Bridge tubes
// N (z 207) and S (z 183): r 4.5, axis y 2.0, x 384-683 between the gate mouths. Face-to-face tubes: upper (y 118, z 195) and
// lower (y -142, z 209), r 5, x 215.3 (face A) to 815.3 (face B); G2 builds their portals in the Gulf faces. No trains yet
// (Phase 4: trains are clipped to the open section of their tube).
using UnityEngine;

namespace Megastructure
{
    public static class SkybridgeTubes
    {
        public static void Build(WorldContext ctx, Transform root, SkybridgeMaterials m)
        {
            var g = BridgeGardenKit.Group(root, "tubes");
            float r = SkybridgeDims.TubeR, y = SkybridgeDims.TubeAxisY, xa = SkybridgeDims.TubeX0, xb = SkybridgeDims.TubeX1;
            var glass = new MeshBuilder();
            var rails = new MeshBuilder();
            foreach (float z in new[] { SkybridgeDims.TubeSZ, SkybridgeDims.TubeNZ })
            {
                MeshKit.Cylinder(glass, new Vector3(xa, y, z), new Vector3(xb, y, z), r, SkybridgeDims.TubeSegments, false);
                foreach (float dz in new[] { -SkybridgeDims.TrackRailOffset, SkybridgeDims.TrackRailOffset })
                {
                    float yr = RailY(dz);
                    MeshKit.Cylinder(rails, new Vector3(xa, yr, z + dz), new Vector3(xb, yr, z + dz), 0.5f * SkybridgeDims.TrackRailD, 8, false);
                }
            }
            BridgeGardenKit.EmitGlass(ctx, g, "bridgeTubes", glass, m.GlassBack, m.GlassFront);

            var faceGlass = new MeshBuilder();
            float fa = SkybridgeDims.FaceAX, fb = SkybridgeDims.FaceBX, fr = SkybridgeDims.FaceTubeR;
            MeshKit.Cylinder(faceGlass, new Vector3(fa, SkybridgeDims.UpperY, SkybridgeDims.UpperZ), new Vector3(fb, SkybridgeDims.UpperY, SkybridgeDims.UpperZ), fr, 64, false);
            MeshKit.Cylinder(faceGlass, new Vector3(fa, SkybridgeDims.LowerY, SkybridgeDims.LowerZ), new Vector3(fb, SkybridgeDims.LowerY, SkybridgeDims.LowerZ), fr, 64, false);
            BridgeGardenKit.EmitGlass(ctx, g, "faceTubes", faceGlass, m.GlassBack, m.GlassFront);
            BridgeGardenKit.Emit(ctx, g, "trackRails", rails, m.TrackRail);
        }

        /// <summary>Rail axis height at lateral offset dz from the tube axis: its bottom 2.5 cm above the glass.</summary>
        public static float RailY(float dz)
        {
            float r = SkybridgeDims.TubeR;
            float glass = SkybridgeDims.TubeAxisY - Mathf.Sqrt(r * r - dz * dz);
            return glass + 0.025f + 0.5f * SkybridgeDims.TrackRailD;
        }

        /// <summary>Tube diameter (EditMode acceptance 9.0 +- 0.02).</summary>
        public static float BridgeTubeDiameter => 2f * SkybridgeDims.TubeR;
    }
}
