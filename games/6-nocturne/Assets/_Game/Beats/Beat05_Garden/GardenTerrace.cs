// GardenTerrace: the hanging-garden terrace of 14.5.5 (x 695-914, z 115-275, floor top y 0): the walk surfaces, the slab, the keel
// that closes the hanging part's underside over the Gulf, and the 1.1 m balustrade on every open edge.
// Walk surfaces (14.4): the 3.0 m flagstone path (lib.FloorFor(s) = FloorGardenPath) from gate B's east face (s 879.7) through
// corner C2 to the promenade; the wet-stone promenade (FloorStone, the beat-6 surface) x 874-914 from the end of C2 (z 225) to
// z 275 (G4 floors north of it); everything else is moss ground (a NocWet variant with the Moss token and maps: the walkable moss
// verges of the 6 m lane). All pieces are flat at y 0 and share every vertex on their common edges (deck end at x 695, gate B's
// face, slab sides): convex pieces are fans from their centre, the two pieces bounded by the C2 arc are fans from a kernel point
// (the far corner (914, 115) and the arc centre), so no ear clipping drops a collinear vertex and no T-junction opens a crack.
// The slab is 6 m deep west of x 818 (over the Gulf; 2.7 m into face B) with a stepped three-tier keel down to y -72 below it,
// and 2 m deep in face B's notch (G2's mass must stay below y -2 there).
using System.Collections.Generic;
using UnityEngine;

namespace Megastructure
{
    public static class GardenTerrace
    {
        static readonly List<Vector2> s_l = new List<Vector2>(64), s_r = new List<Vector2>(64);

        /// <summary>Left (north / west) and right edge points of the flagstone path, gate B to the end of C2 (exact path maths).</summary>
        public static void PathEdges(List<Vector2> left, List<Vector2> right, float halfWidth, out Vector2 arcCentre)
        {
            left.Clear(); right.Clear();
            NocLanes.Path.Arc(1, out double d0, out double d1, out _, out double cx, out double cz);
            arcCentre = new Vector2((float)cx, (float)cz);
            Add(left, right, GardenDims.PathS0, halfWidth);
            // the start lies on the straight east leg at gate B's face: snap it exactly onto the deck end's vertex lines
            // (the fillet maths lands within ~1e-5 m, one float ulp off at x 695)
            left[0] = new Vector2(GardenDims.TerraceX0, GardenDims.AxisZ + halfWidth);
            right[0] = new Vector2(GardenDims.TerraceX0, GardenDims.AxisZ - halfWidth);
            for (int i = 0; i <= GardenDims.ArcSegments; i++) Add(left, right, d0 + (d1 - d0) * i / GardenDims.ArcSegments, halfWidth);
        }

        static void Add(List<Vector2> left, List<Vector2> right, double s, float hw)
        {
            NocLanes.Path.CorridorPoint(s, -hw, out double lx, out double lz);
            NocLanes.Path.CorridorPoint(s, hw, out double rx, out double rz);
            left.Add(new Vector2((float)lx, (float)lz));
            right.Add(new Vector2((float)rx, (float)rz));
        }

        public static void Build(WorldContext ctx, Transform root, GardenMaterials m)
        {
            PathEdges(s_l, s_r, GardenDims.PathHalfWidth, out Vector2 c);
            int n = s_l.Count;
            float X0 = GardenDims.TerraceX0, X1 = GardenDims.TerraceX1, Z0 = GardenDims.TerraceZ0, Z1 = GardenDims.TerraceZ1;
            float xe = GardenDims.SlabEastInFace, px = GardenDims.PromX0;
            Vector2 L0 = s_l[0], R0 = s_r[0], L1 = s_l[1], R1 = s_r[1], Le = s_l[n - 1], Re = s_r[n - 1];
            float cz = c.y;
            // points where the deck end (z 189, 192.5, 193.5, 196.5, 197.5, 201) and gate B (z 175, 215) meet the west edge
            float o0 = SkybridgeDims.OpeningZ0, o1 = SkybridgeDims.OpeningZ1;
            float d0 = SkybridgeDims.AxisZ - SkybridgeDims.DeckHalfWidth, d1 = SkybridgeDims.AxisZ + SkybridgeDims.DeckHalfWidth;
            float g0 = SkybridgeDims.GateZ0, g1 = SkybridgeDims.GateZ1;

            // ---- path ribbon (flagstone path)
            var path = new MeshBuilder();
            int first = path.VertexCount;
            for (int i = 0; i < n; i++)
            {
                path.Vertex(new Vector3(s_l[i].x, 0f, s_l[i].y), Vector3.up, s_l[i]);
                path.Vertex(new Vector3(s_r[i].x, 0f, s_r[i].y), Vector3.up, s_r[i]);
            }
            for (int i = 0; i + 1 < n; i++)
            {
                int a = first + 2 * i;
                MeshKit.Quad(path, a, a + 2, a + 3, a + 1);
            }
            BridgeGardenKit.Emit(ctx, root, "path", path, m.Path);

            // ---- moss ground: A (south-west rectangle), B (south-east, fan from (914, 115)), C (north-west), D (inside C2, fan from the arc centre)
            var moss = new MeshBuilder();
            var loop = new List<Vector2>(64);
            loop.Add(new Vector2(X0, Z0)); loop.Add(new Vector2(xe, Z0)); loop.Add(new Vector2(R1.x, Z0)); loop.Add(R1); loop.Add(R0);
            loop.Add(new Vector2(X0, d0)); loop.Add(new Vector2(X0, o0)); loop.Add(new Vector2(X0, g0));
            FanCentroid(moss, loop, 0f);

            loop.Clear();
            loop.Add(new Vector2(X1, Z0)); loop.Add(new Vector2(X1, cz)); loop.Add(Re);
            for (int i = n - 2; i >= 1; i--) loop.Add(s_r[i]);
            loop.Add(new Vector2(R1.x, Z0));
            FanApex(moss, loop, 0f);

            loop.Clear();
            loop.Add(L0); loop.Add(L1); loop.Add(new Vector2(L1.x, cz)); loop.Add(new Vector2(L1.x, Z1)); loop.Add(new Vector2(xe, Z1)); loop.Add(new Vector2(X0, Z1));
            loop.Add(new Vector2(X0, g1)); loop.Add(new Vector2(X0, o1)); loop.Add(new Vector2(X0, d1));
            FanCentroid(moss, loop, 0f);

            loop.Clear();
            loop.Add(new Vector2(L1.x, cz));                                       // apex: the arc centre, on the edge to rect C
            for (int i = 1; i < n; i++) loop.Add(s_l[i]);
            loop.Add(new Vector2(px, cz)); loop.Add(new Vector2(px, Z1)); loop.Add(new Vector2(L1.x, Z1));
            FanApex(moss, loop, 0f);
            BridgeGardenKit.Emit(ctx, root, "moss", moss, m.Moss);

            // ---- promenade (wet stone, beat 6 surface) x 874-914, z 225-275
            var prom = new MeshBuilder();
            loop.Clear();
            loop.Add(new Vector2(px, cz)); loop.Add(Le); loop.Add(Re); loop.Add(new Vector2(X1, cz)); loop.Add(new Vector2(X1, Z1)); loop.Add(new Vector2(px, Z1));
            FanCentroid(prom, loop, 0f);
            BridgeGardenKit.Emit(ctx, root, "promenade", prom, m.Promenade);

            // ---- slab, keel (concrete)
            var slab = new MeshBuilder();
            float yG = -GardenDims.SlabDepthGulf, yN = -GardenDims.SlabDepthNotch;
            // hanging part x 695..818: sides (west split around gate B), soffit
            Side(slab, new[] { X0, xe }, Z0, yG, Vector3.back);
            Side(slab, new[] { X0, xe }, Z1, yG, Vector3.forward);
            BridgeGardenKit.OrientedQuad(slab, new Vector3(X0, yG, Z0), new Vector3(X0, 0f, Z0), new Vector3(X0, 0f, g0), new Vector3(X0, yG, g0), Vector3.left);
            BridgeGardenKit.OrientedQuad(slab, new Vector3(X0, yG, g1), new Vector3(X0, 0f, g1), new Vector3(X0, 0f, Z1), new Vector3(X0, yG, Z1), Vector3.left);
            // behind gate block B only below its underside (y -3.5): above it the block's east face closes the slab
            float gb = SkybridgeDims.GateBottom;
            BridgeGardenKit.OrientedQuad(slab, new Vector3(X0, yG, g0), new Vector3(X0, gb, g0), new Vector3(X0, gb, g1), new Vector3(X0, yG, g1), Vector3.left);
            loop.Clear();
            loop.Add(new Vector2(X0, Z0)); loop.Add(new Vector2(xe, Z0)); loop.Add(new Vector2(xe, Z1)); loop.Add(new Vector2(X0, Z1));
            loop.Add(new Vector2(X0, g1)); loop.Add(new Vector2(X0, g0));
            FanCentroid(slab, loop, yG, false);
            BridgeGardenKit.OrientedQuad(slab, new Vector3(xe, yG, Z0), new Vector3(xe, yN, Z0), new Vector3(xe, yN, Z1), new Vector3(xe, yG, Z1), Vector3.right);
            // notch part x 818..914 (2 m)
            Side(slab, new[] { xe, R1.x, X1 }, Z0, yN, Vector3.back, xe);
            Side(slab, new[] { xe, L1.x, px, X1 }, Z1, yN, Vector3.forward, xe);
            SideX(slab, X1, new[] { Z0, cz, Z1 }, yN, Vector3.right);
            loop.Clear();
            loop.Add(new Vector2(xe, Z0)); loop.Add(new Vector2(R1.x, Z0)); loop.Add(new Vector2(X1, Z0)); loop.Add(new Vector2(X1, cz)); loop.Add(new Vector2(X1, Z1));
            loop.Add(new Vector2(px, Z1)); loop.Add(new Vector2(L1.x, Z1)); loop.Add(new Vector2(xe, Z1));
            FanCentroid(slab, loop, yN, false);
            // keel: three stepped tiers under the hanging part (no top faces: they would lie on the soffit above)
            int keelFaces = BridgeGardenKit.AllFaces & ~BridgeGardenKit.FacePY & ~BridgeGardenKit.FacePX;
            BridgeGardenKit.OpenBox(slab, new Vector3(702f, -22f, 122f), new Vector3(xe, yG, 268f), keelFaces);
            BridgeGardenKit.OpenBox(slab, new Vector3(716f, -44f, 134f), new Vector3(xe, -22f, 256f), keelFaces);
            BridgeGardenKit.OpenBox(slab, new Vector3(735f, -72f, 148f), new Vector3(xe, -44f, 242f), keelFaces);
            BridgeGardenKit.Emit(ctx, root, "slab", slab, m.Concrete);

            // ---- balustrades 1.1 m on the open edges (2 cm inside the slab edges, bottom 5 cm inside the slab, no bottom faces)
            var bal = new MeshBuilder();
            float t = GardenDims.BalustradeThick, hb = GardenDims.BalustradeHeight, e = 0.02f;
            int bf = BridgeGardenKit.AllFaces & ~BridgeGardenKit.FaceNY;
            BridgeGardenKit.OpenBox(bal, new Vector3(X0 + e, -0.05f, Z0 + e), new Vector3(X0 + e + t, hb, g0 - e), bf);
            BridgeGardenKit.OpenBox(bal, new Vector3(X0 + e, -0.05f, g1 + e), new Vector3(X0 + e + t, hb, Z1 - e), bf);
            BridgeGardenKit.OpenBox(bal, new Vector3(X0 + 2f * e + t, -0.05f, Z0 + e), new Vector3(GardenDims.FaceBX, hb, Z0 + e + t), bf);
            BridgeGardenKit.OpenBox(bal, new Vector3(X0 + 2f * e + t, -0.05f, Z1 - e - t), new Vector3(GardenDims.FaceBX, hb, Z1 - e), bf);
            BridgeGardenKit.OpenBox(bal, new Vector3(X1 - e - t, -0.05f, Z0 + e), new Vector3(X1 - e, hb, Z1 - e), bf);
            BridgeGardenKit.Emit(ctx, root, "balustrade", bal, m.Stone);
        }

        /// <summary>Side face z = const from the top (y 0) down to yb, split at the given x (the vertices the top pieces use).</summary>
        static void Side(MeshBuilder mb, float[] xs, float z, float yb, Vector3 want, float xStart = float.NegativeInfinity)
        {
            for (int i = 0; i + 1 < xs.Length; i++)
            {
                if (xs[i + 1] <= xStart) continue;
                BridgeGardenKit.OrientedQuad(mb, new Vector3(xs[i], yb, z), new Vector3(xs[i], 0f, z), new Vector3(xs[i + 1], 0f, z), new Vector3(xs[i + 1], yb, z), want);
            }
        }

        static void SideX(MeshBuilder mb, float x, float[] zs, float yb, Vector3 want)
        {
            for (int i = 0; i + 1 < zs.Length; i++)
                BridgeGardenKit.OrientedQuad(mb, new Vector3(x, yb, zs[i]), new Vector3(x, 0f, zs[i]), new Vector3(x, 0f, zs[i + 1]), new Vector3(x, yb, zs[i + 1]), want);
        }

        /// <summary>A star-shaped loop (x, z) at height y as a fan from the mean of its vertices (convex pieces).</summary>
        static void FanCentroid(MeshBuilder mb, List<Vector2> loop, float y, bool up = true)
        {
            Vector2 c = Vector2.zero;
            for (int i = 0; i < loop.Count; i++) c += loop[i];
            c /= loop.Count;
            var nrm = up ? Vector3.up : Vector3.down;
            int centre = mb.Vertex(new Vector3(c.x, y, c.y), nrm, c);
            int first = mb.VertexCount;
            for (int i = 0; i < loop.Count; i++) mb.Vertex(new Vector3(loop[i].x, y, loop[i].y), nrm, loop[i]);
            for (int i = 0; i < loop.Count; i++) MeshKit.Tri(mb, centre, first + i, first + (i + 1) % loop.Count);
        }

        /// <summary>A loop whose first vertex sees every other one (a kernel point on the boundary): a fan from it.</summary>
        static void FanApex(MeshBuilder mb, List<Vector2> loop, float y)
        {
            int first = mb.VertexCount;
            for (int i = 0; i < loop.Count; i++) mb.Vertex(new Vector3(loop[i].x, y, loop[i].y), Vector3.up, loop[i]);
            for (int i = 1; i + 1 < loop.Count; i++) MeshKit.Tri(mb, first, first + i, first + i + 1);
        }

        /// <summary>Path width at progress s (EditMode acceptance 3.0 +- 0.02): distance between the two edge points.</summary>
        public static float PathWidthAt(double s)
        {
            NocLanes.Path.CorridorPoint(s, -GardenDims.PathHalfWidth, out double lx, out double lz);
            NocLanes.Path.CorridorPoint(s, GardenDims.PathHalfWidth, out double rx, out double rz);
            return (float)System.Math.Sqrt((lx - rx) * (lx - rx) + (lz - rz) * (lz - rz));
        }
    }
}
