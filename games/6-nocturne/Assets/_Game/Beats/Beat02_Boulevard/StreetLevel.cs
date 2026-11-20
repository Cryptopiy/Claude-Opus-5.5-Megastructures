// StreetLevel: the walkable street of beat 2 and what lies on it (MASTER_PROMPT 14.5.2, 14.4; BoulevardLayout):
//   * the street floor: wet stone (lib.FloorFor(s): FloorStone, NocWet), top y 0, 1 m thick, z 120..270 from x -1 500 (far into the
//     CityField canyon: its edge lies 0.06 deg below the horizon from the median) to x 215.3 where G2's tongue deck begins
//     (agreement 2: no gap, no overlap; no end face at the seam), in 200 m chunks with chunk-local origins (7.5); the arcade floor
//     strips z 116..120 (south, without the alley mouth x -4.5..4.5) and 270..274 (north) for x -42..215.25; exposed edges of the
//     slab west of the hero facades are Concrete;
//   * tram tracks (14.5.2): two rails 0.07 m (NocLine tubes) per track at z 163 and 227, gauge 1.435 m, from x -1 500 to the
//     buffer stop at x 205 (south) and into the terminus (north), embedded in the street (GE, D-029 GB-16: the rail head stands
//     8 mm proud, the rest lies below the paving, like a grooved street rail; they lay 7 cm proud where the walk crosses the south
//     track at s 162-164; the bollard lines leave a gap there).
using System.Collections.Generic;
using UnityEngine;

namespace Megastructure
{
    public static class StreetLevel
    {
        public static void Build(G1Parts parts)
        {
            Floors(parts);
            Rails(parts);
        }

        static void Floors(G1Parts parts)
        {
            float z0 = (float)BoulevardLayout.SouthZ, z1 = (float)BoulevardLayout.NorthZ, t = (float)BoulevardLayout.FloorThickness;
            float x0 = (float)BoulevardLayout.StreetFloorX0, x1 = (float)BoulevardLayout.FacadeX1, ch = (float)BoulevardLayout.FloorChunk;
            int n = Mathf.CeilToInt((x1 - x0) / ch);
            for (int i = 0; i < n; i++)
            {
                float a = x1 - ch * (i + 1), b = x1 - ch * i;
                a = Mathf.Max(a, x0);
                var origin = new Vector3(Mathf.Round(0.5f * (a + b)), 0f, (float)BoulevardLayout.AxisZ);
                var top = parts.Get("street" + i, G1Mat.FloorStone, origin);
                G1Mesh.Box(top, new Vector3(a, -t, z0), new Vector3(b, 0f, z1), G1Mesh.PY);
                // exposed slab edges and underside where no podium covers them (west of x -90 south / -60 north)
                var side = parts.Get("streetEdge" + i, G1Mat.Concrete, origin);
                G1Mesh.Box(side, new Vector3(a, -t, z0), new Vector3(b, 0f, z1), G1Mesh.NY);
                float sa = Mathf.Min(b, (float)BoulevardLayout.SouthX0), na = Mathf.Min(b, (float)BoulevardLayout.NorthX0);
                if (sa > a) G1Mesh.Box(side, new Vector3(a, -t, z0), new Vector3(sa, 0f, z1), G1Mesh.NZ);
                if (na > a) G1Mesh.Box(side, new Vector3(a, -t, z0), new Vector3(na, 0f, z1), G1Mesh.PZ);
                if (i == n - 1) G1Mesh.Box(side, new Vector3(a, -t, z0), new Vector3(b, 0f, z1), G1Mesh.NX);
            }
            // arcade floors
            var arcade = parts.Get("arcadeFloors", G1Mat.FloorStone, new Vector3(85f, 0f, 195f));
            float ax0 = -42f, ax1 = (float)BoulevardLayout.ClosureX1, d = (float)BoulevardLayout.ArcadeDepth, px = (float)BoulevardLayout.MouthPierX;
            G1Mesh.Box(arcade, new Vector3(ax0, -t, z0 - d), new Vector3(-px, 0f, z0), G1Mesh.PY);
            G1Mesh.Box(arcade, new Vector3(px, -t, z0 - d), new Vector3(ax1, 0f, z0), G1Mesh.PY);
            G1Mesh.Box(arcade, new Vector3(ax0, -t, z1), new Vector3(ax1, 0f, z1 + d), G1Mesh.PY);
        }

        /// <summary>Rail head height above the street (GB-16: embedded, within the floor probe's 1 cm) and the rail axis height.</summary>
        public const float RailTop = 0.008f;
        public static float RailAxisY => RailTop - 0.5f * G1Materials.Rail;

        static void Rails(G1Parts parts)
        {
            var pts = new List<Vector3>(2); var frames = new List<LoftFrame>(2); var sec = new CrossSection();
            float r = 0.5f * G1Materials.Rail, x0 = (float)BoulevardLayout.StreetFloorX0, ch = (float)BoulevardLayout.FloorChunk;
            foreach (double zt in new[] { BoulevardLayout.TrackSouthZ, BoulevardLayout.TrackNorthZ })
            {
                float xEnd = zt < BoulevardLayout.AxisZ ? (float)BoulevardLayout.TrackX1 : (float)BoulevardLayout.TerminusX1 - 1.2f;
                int n = Mathf.CeilToInt((xEnd - x0) / ch);
                for (int i = 0; i < n; i++)
                {
                    float a = xEnd - ch * (i + 1), b = xEnd - ch * i;
                    a = Mathf.Max(a, x0);
                    var mb = parts.Get("rails" + (zt < BoulevardLayout.AxisZ ? "S" : "N") + i, G1Mat.LineRail, new Vector3(Mathf.Round(0.5f * (a + b)), 0f, (float)zt));
                    foreach (float dz in new[] { -0.5f * (float)BoulevardLayout.Gauge, 0.5f * (float)BoulevardLayout.Gauge })
                    {
                        pts.Clear(); pts.Add(new Vector3(a, RailAxisY, (float)zt + dz)); pts.Add(new Vector3(b, RailAxisY, (float)zt + dz));
                        G1Mesh.Tube(mb, pts, r, 8, frames, sec);
                    }
                }
            }
            // buffer stop of the south track at x 205 (steel block 1.0 m, outside every lane: 21 m south of the median axis)
            var stop = parts.Get("bufferStop", G1Mat.SteelPainted, new Vector3(205f, 0.5f, (float)BoulevardLayout.TrackSouthZ));
            G1Mesh.Chamfer(stop, new Vector3((float)BoulevardLayout.TrackX1 - 0.2f, 0f, (float)BoulevardLayout.TrackSouthZ - 1.2f),
                new Vector3((float)BoulevardLayout.TrackX1 + 0.8f, 1.0f, (float)BoulevardLayout.TrackSouthZ + 1.2f), 0.03f);
        }
    }
}
