// SeaClipmap: the near-sea mesh of 14.5.2 ("SeaClipmap 7 levels x 128 x 128 quads, spacing 0.5 x 2^k (0.5-32 m),
// half-extent 32-2048 m, geomorph over the outer 15 %"), drawn by Megastructure/BwSea (BwSea.shader does all the placing).
// One static mesh in LEVEL-LOCAL integer coordinates: each vertex stores (i, 0, j) in cells of its level and uv0 = (level,
// group). Level 0 is the full 128 x 128 grid; levels 1-6 are rings. A ring k is centred on the camera snapped to 2 x its
// spacing, the finer level k-1 on the camera snapped to the ring's spacing, so the finer level sits at offset o = 0 or 1
// cell per axis inside the ring and covers [-32 + o, 32 + o]. The ring therefore holds
//   group 0  the fixed part: every cell outside [-32, 33]^2 (always drawn)
//   group 1..4  trim strips, one cell wide: column x -32 (drawn when o_x = 1), column x 32 (o_x = 0), row z -32 (o_z = 1),
//               row z 32 (o_z = 0), each over cells -31..31
//   group 5..8  the four trim corners (drawn unless the finer level covers them)
// so exactly the 129 cells between the two squares are drawn: no gap, no overlap, no co-planar double surface (the shader
// collapses an inactive group to an outside-the-depth-range point). Trim groups own their vertices; their positions equal
// the ring's bit for bit (integers x power-of-two spacings), so the mesh is watertight. Triangles: 32 768 + 6 x 24 830 =
// 181 748 at most (the 14.5.2 "229 k" counts 7 full grids; rings avoid drawing 6 overlapping inner squares).
// Plus one DISTURBANCE ring (level 7: 64 m cells, half-extent 4096 m, +24 830 triangles = 206 578): BwSea lays it flat on
// the sea sphere and draws it only inside the support of an active SeaDisturbances entry, so the kraken's breach rings,
// collars and slicks continue beyond the 2048 m wave clipmap (Addendum A1: K1's ring field reaches 2.5 km from KS04) while
// the far field keeps drawing the plain sea there.
// The mesh bounds are huge (the vertex stage moves it with the camera): never frustum-culled while any sea can be in view.
using System.Collections.Generic;
using UnityEngine;

namespace Megastructure.Beats
{
    public static class SeaClipmap
    {
        public const int Levels = BulwarkConst.SeaClipLevels;            // 7
        public const int Quads = BulwarkConst.SeaClipQuads;              // 128 per side
        public const int Half = Quads / 2;                               // 64
        public const int Hole = Half / 2;                                // 32: the finer level spans [-32 + o, 32 + o]
        public const double Spacing0 = BulwarkConst.SeaClipSpacing0;     // 0.5 m
        /// <summary>Disturbance-only rings beyond the wave levels (BwSea SEA_DIST_LEVEL 7).</summary>
        public const int DisturbLevels = 1;

        /// <summary>Spacing of level k (m).</summary>
        public static double Spacing(int k) => Spacing0 * (1 << k);
        /// <summary>Half-extent of level k (m): 32, 64, ... 2048.</summary>
        public static double HalfExtent(int k) => Spacing(k) * Half;

        /// <summary>Builds the clipmap mesh (level-local coordinates, see the header).</summary>
        public static Mesh Build(out int triangles)
        {
            var pos = new List<Vector3>(100000);
            var uv = new List<Vector2>(100000);
            var idx = new List<int>(560000);
            for (int k = 0; k < Levels + DisturbLevels; k++)
            {
                if (k == 0) Patch(pos, uv, idx, k, 0, -Half, Half, -Half, Half, null);
                else
                {
                    // fixed ring: the 129 x 129 vertex grid, cells outside [-32, 33]^2
                    Patch(pos, uv, idx, k, 0, -Half, Half, -Half, Half, (ci, cj) => ci < -Hole || ci > Hole || cj < -Hole || cj > Hole);
                    Patch(pos, uv, idx, k, 1, -Hole, -Hole + 1, -Hole + 1, Hole, null);          // column x -32..-31, z -31..32
                    Patch(pos, uv, idx, k, 2, Hole, Hole + 1, -Hole + 1, Hole, null);            // column x 32..33
                    Patch(pos, uv, idx, k, 3, -Hole + 1, Hole, -Hole, -Hole + 1, null);          // row z -32..-31, x -31..32
                    Patch(pos, uv, idx, k, 4, -Hole + 1, Hole, Hole, Hole + 1, null);            // row z 32..33
                    Patch(pos, uv, idx, k, 5, -Hole, -Hole + 1, -Hole, -Hole + 1, null);         // corner (x low, z low)
                    Patch(pos, uv, idx, k, 6, Hole, Hole + 1, -Hole, -Hole + 1, null);           // corner (x high, z low)
                    Patch(pos, uv, idx, k, 7, -Hole, -Hole + 1, Hole, Hole + 1, null);           // corner (x low, z high)
                    Patch(pos, uv, idx, k, 8, Hole, Hole + 1, Hole, Hole + 1, null);             // corner (x high, z high)
                }
            }
            var mesh = new Mesh { name = "sea_clipmap", indexFormat = UnityEngine.Rendering.IndexFormat.UInt32 };
            mesh.SetVertices(pos);
            var nrm = new List<Vector3>(pos.Count);
            for (int i = 0; i < pos.Count; i++) nrm.Add(Vector3.up);
            mesh.SetNormals(nrm);
            mesh.SetUVs(0, uv);
            mesh.SetIndices(idx, MeshTopology.Triangles, 0, false);
            // the vertex stage places the rings round the camera (anywhere within the 1.8 km world): never cull
            mesh.bounds = new Bounds(new Vector3(0f, (float)BulwarkConst.SeaY, 750f), new Vector3(16000f, 60f, 16000f));
            triangles = idx.Count / 3;
            return mesh;
        }

        /// <summary>Cells [i0, i1) x [j0, j1) of level k as one vertex grid (shared vertices) with an optional cell filter.
        /// Quads wound clockwise seen from above (+y front, MeshBuilder convention), unused vertices dropped.</summary>
        static void Patch(List<Vector3> pos, List<Vector2> uv, List<int> idx, int k, int group, int i0, int i1, int j0, int j1,
                          System.Func<int, int, bool> keep)
        {
            int w = i1 - i0 + 1, h = j1 - j0 + 1;
            var map = new int[w * h];
            for (int t = 0; t < map.Length; t++) map[t] = -1;
            int V(int i, int j)
            {
                int key = (j - j0) * w + (i - i0);
                if (map[key] < 0)
                {
                    map[key] = pos.Count;
                    pos.Add(new Vector3(i, 0f, j));
                    uv.Add(new Vector2(k, group));
                }
                return map[key];
            }
            for (int cj = j0; cj < j1; cj++)
                for (int ci = i0; ci < i1; ci++)
                {
                    if (keep != null && !keep(ci, cj)) continue;
                    int a = V(ci, cj), b = V(ci, cj + 1), c = V(ci + 1, cj + 1), d = V(ci + 1, cj);
                    // alternate the diagonal in a checkerboard (symmetric tessellation: the geomorph sees no preferred axis)
                    if (((ci + cj) & 1) == 0) { idx.Add(a); idx.Add(b); idx.Add(c); idx.Add(a); idx.Add(c); idx.Add(d); }
                    else { idx.Add(a); idx.Add(b); idx.Add(d); idx.Add(b); idx.Add(c); idx.Add(d); }
                }
        }

        /// <summary>Cells of trim group g of a ring that are drawn for the finer level's offsets (ox, oz) in {0, 1} (C# twin of
        /// SeaTrimActive in BwSea.shader).</summary>
        public static bool TrimActive(int g, int ox, int oz)
        {
            bool xl = ox == 1, zl = oz == 1;
            switch (g)
            {
                case 1: return xl;
                case 2: return !xl;
                case 3: return zl;
                case 4: return !zl;
                case 5: return xl || zl;
                case 6: return !xl || zl;
                case 7: return xl || !zl;
                case 8: return !xl || !zl;
                default: return true;
            }
        }

        /// <summary>
        /// Coverage check (C# twin, SeaTests): for a ring and offsets (ox, oz), every cell of the square [-64, 64)^2 is covered
        /// exactly once by the ring's drawn cells or the finer level's square [-32 + o, 32 + o)^2. Returns the number of cells
        /// covered 0 or 2+ times (0 = watertight).
        /// </summary>
        public static int CoverageErrors(int ox, int oz)
        {
            int errors = 0;
            for (int cj = -Half; cj < Half; cj++)
                for (int ci = -Half; ci < Half; ci++)
                {
                    int n = 0;
                    bool fixedCell = ci < -Hole || ci > Hole || cj < -Hole || cj > Hole;
                    if (fixedCell) n++;
                    else
                    {
                        int g = GroupOf(ci, cj);
                        if (g > 0 && TrimActive(g, ox, oz)) n++;
                    }
                    bool inner = ci >= -Hole + ox && ci < Hole + ox && cj >= -Hole + oz && cj < Hole + oz;
                    if (inner) n++;
                    if (n != 1) errors++;
                }
            return errors;
        }

        /// <summary>Trim group of a cell inside [-32, 33)^2 (0 = inner, never drawn by the ring).</summary>
        static int GroupOf(int ci, int cj)
        {
            bool xLo = ci == -Hole, xHi = ci == Hole, zLo = cj == -Hole, zHi = cj == Hole;
            if (xLo && zLo) return 5;
            if (xHi && zLo) return 6;
            if (xLo && zHi) return 7;
            if (xHi && zHi) return 8;
            if (xLo) return 1;
            if (xHi) return 2;
            if (zLo) return 3;
            if (zHi) return 4;
            return 0;
        }
    }
}
