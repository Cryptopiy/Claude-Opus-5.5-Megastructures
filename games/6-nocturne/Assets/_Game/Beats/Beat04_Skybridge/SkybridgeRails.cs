// SkybridgeRails: the two deck rails of 14.5.4 (1.10 m: posts 0.05 m every 2 m, a 0.06 m top rail, two 0.03 m rods), side-mounted
// just outside the 5.0 m deck (inner faces at z 195 +- 2.5, outside the lane walls at +- 2.53 only by the post radius), running
// gate face to gate face (x 384-683, ends 5 cm inside the faces). NocLine tubes (8-gon, smooth radial normals, no caps: the
// shader pushes every vertex about the member axis to the 1.75 px minimum width), one material per true width.
using UnityEngine;

namespace Megastructure
{
    public static class SkybridgeRails
    {
        /// <summary>Lateral distance of a member's axis from the bridge axis: its inner face lies on the 2.5 m deck edge.</summary>
        public static float AxisOffset(float diameter) => SkybridgeDims.DeckHalfWidth + 0.5f * diameter;

        /// <summary>Height of the top of the top rail above the deck (EditMode acceptance 1.10 +- 0.01).</summary>
        public static float TopRailCentreY => SkybridgeDims.RailHeight - 0.5f * SkybridgeDims.TopRailD;

        public static void Build(WorldContext ctx, Transform root, SkybridgeMaterials m)
        {
            var g = BridgeGardenKit.Group(root, "rails");
            const int seg = 8;
            var posts = new MeshBuilder();
            var top = new MeshBuilder();
            var rods = new MeshBuilder();
            float xa = SkybridgeDims.RailX0 - SkybridgeDims.RailEmbed, xb = SkybridgeDims.RailX1 + SkybridgeDims.RailEmbed;
            for (int side = -1; side <= 1; side += 2)
            {
                float zp = SkybridgeDims.AxisZ + side * AxisOffset(SkybridgeDims.PostD);
                float zt = SkybridgeDims.AxisZ + side * AxisOffset(SkybridgeDims.TopRailD);
                float zr = SkybridgeDims.AxisZ + side * AxisOffset(SkybridgeDims.RodD);
                int count = PostsPerSide();
                for (int k = 0; k < count; k++)
                {
                    float x = SkybridgeDims.PostX0 + SkybridgeDims.PostPitch * k;
                    MeshKit.Cylinder(posts, new Vector3(x, SkybridgeDims.PostBottom, zp), new Vector3(x, TopRailCentreY, zp), 0.5f * SkybridgeDims.PostD, seg, false);
                }
                MeshKit.Cylinder(top, new Vector3(xa, TopRailCentreY, zt), new Vector3(xb, TopRailCentreY, zt), 0.5f * SkybridgeDims.TopRailD, seg, false);
                MeshKit.Cylinder(rods, new Vector3(xa, SkybridgeDims.RodY0, zr), new Vector3(xb, SkybridgeDims.RodY0, zr), 0.5f * SkybridgeDims.RodD, seg, false);
                MeshKit.Cylinder(rods, new Vector3(xa, SkybridgeDims.RodY1, zr), new Vector3(xb, SkybridgeDims.RodY1, zr), 0.5f * SkybridgeDims.RodD, seg, false);
            }
            BridgeGardenKit.Emit(ctx, g, "posts", posts, m.Post);
            BridgeGardenKit.Emit(ctx, g, "topRail", top, m.TopRail);
            BridgeGardenKit.Emit(ctx, g, "rods", rods, m.Rod);
        }

        /// <summary>Number of posts per side (EditMode: one every 2 m between the gate faces).</summary>
        public static int PostsPerSide()
        {
            return (int)System.Math.Floor((SkybridgeDims.RailX1 - 0.4f - SkybridgeDims.PostX0) / SkybridgeDims.PostPitch + 1e-4) + 1;
        }
    }
}
