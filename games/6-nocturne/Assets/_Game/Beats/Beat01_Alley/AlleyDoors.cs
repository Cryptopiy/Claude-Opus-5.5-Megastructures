// AlleyDoors: the street-level doors of the alley walls (MASTER_PROMPT 14.5.1 scale cue "door 2.0 m"; graybox clutter that gives
// the walls a human scale where the free-look stills see them at eye height). 9 doors per wall (AlleyLayout.DoorZWest / DoorZEast),
// following the wall on the profile's flanks: the wall strips leave a 1.0 x 2.1 m opening (AlleyBlocks); here the reveals (jambs and
// lintel, 12 cm deep), the steel leaf at the back of the recess with a handle, and a lit transom 2 cm proud above the door
// (y 2.2-2.45, warm or cold by hash). Everything below y 2.1 lies behind the wall plane (the capsule touches the wall plane).
using UnityEngine;

namespace Megastructure
{
    public static class AlleyDoors
    {
        /// <summary>True when z lies inside one of the door openings of the list.</summary>
        public static bool InDoor(double[] doors, double z)
        {
            foreach (double d in doors) if (System.Math.Abs(z - d) < 0.5 * AlleyLayout.SideDoorW) return true;
            return false;
        }

        public static void Build(G1Parts parts)
        {
            var origin = new Vector3(0f, 1f, 60f);
            var reveal = parts.Get("doorReveals", G1Mat.Concrete, origin);
            var leaf = parts.Get("doorLeaves", G1Mat.SteelPainted, origin);
            var warm = parts.Get("doorTransomWarm", G1Mat.EmitShopWarm, origin);
            var cold = parts.Get("doorTransomCold", G1Mat.EmitShopCold, origin);
            float hw = 0.5f * (float)AlleyLayout.SideDoorW, h = (float)AlleyLayout.SideDoorH, dep = (float)AlleyLayout.SideDoorDepth;
            for (int side = -1; side <= 1; side += 2)
            {
                double[] doors = side < 0 ? AlleyLayout.DoorZWest : AlleyLayout.DoorZEast;
                for (int i = 0; i < doors.Length; i++)
                {
                    float z = (float)doors[i], z0 = z - hw, z1 = z + hw;
                    // the wall line at each edge of the opening (doors on the profile's flanks follow the slanted wall)
                    float xw0 = side * (float)AlleyLayout.HalfWidth(z0), xw1 = side * (float)AlleyLayout.HalfWidth(z1);
                    float xb0 = xw0 + side * dep, xb1 = xw1 + side * dep;
                    var inward = new Vector3(-side, 0f, 0f);
                    // jambs (face into the opening), lintel (faces down)
                    G1Mesh.Quad(reveal, new Vector3(xw0, 0f, z0), new Vector3(xw0, h, z0), new Vector3(xb0, h, z0), new Vector3(xb0, 0f, z0), Vector3.forward);
                    G1Mesh.Quad(reveal, new Vector3(xw1, 0f, z1), new Vector3(xw1, h, z1), new Vector3(xb1, h, z1), new Vector3(xb1, 0f, z1), Vector3.back);
                    G1Mesh.Quad(reveal, new Vector3(xw0, h, z0), new Vector3(xb0, h, z0), new Vector3(xb1, h, z1), new Vector3(xw1, h, z1), Vector3.down);
                    // leaf at the back of the recess, a handle 6 cm proud of it (still 6 cm behind the wall plane)
                    G1Mesh.Quad(leaf, new Vector3(xb0, 0f, z0), new Vector3(xb0, h, z0), new Vector3(xb1, h, z1), new Vector3(xb1, 0f, z1), inward);
                    float hz = (i & 1) == 0 ? z1 - 0.12f : z0 + 0.12f;
                    float xbh = Mathf.Lerp(xb0, xb1, (hz - z0) / (z1 - z0));
                    float hx0 = Mathf.Min(xbh, xbh - side * 0.06f), hx1 = Mathf.Max(xbh, xbh - side * 0.06f);
                    G1Mesh.Chamfer(leaf, new Vector3(hx0, 1.0f, hz - 0.03f), new Vector3(hx1, 1.06f, hz + 0.03f), 0.008f);
                    // lit transom above the door
                    uint hsh = NocMath.Pcg((uint)(i * 97 + (side > 0 ? 501 : 0)) ^ 0x54524E53u);
                    var mb = NocMath.U01(hsh) < 0.6 ? warm : cold;
                    float ta = z0 + 0.05f, tb = z1 - 0.05f;
                    float xta = side * ((float)AlleyLayout.HalfWidth(ta) - (float)AlleyLayout.TransomProud);
                    float xtb = side * ((float)AlleyLayout.HalfWidth(tb) - (float)AlleyLayout.TransomProud);
                    G1Mesh.Quad(mb, new Vector3(xta, (float)AlleyLayout.TransomY0, ta), new Vector3(xta, (float)AlleyLayout.TransomY1, ta),
                        new Vector3(xtb, (float)AlleyLayout.TransomY1, tb), new Vector3(xtb, (float)AlleyLayout.TransomY0, tb), inward);
                }
            }
        }
    }
}
