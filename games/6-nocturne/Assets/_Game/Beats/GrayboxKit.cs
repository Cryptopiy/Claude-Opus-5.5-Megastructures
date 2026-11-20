// GrayboxKit: the shared helper of the seven Phase-2 smoke builders (Part 3.1 rule 2; IMPORT_PLAN section 5). Each beat lays
// only a floor slab with its 14.4 surface (lib.FloorFor at the beat's mid s) along its s range of the real 1 500 m path (lane half-width + 1 m on each side, top at the walk plane
// y 0, 0.5 m skirts), a pair of emissive marker posts at its start boundary and low marker boxes every 30 m along the left lane
// edge, so the whole path is walkable and readable in captures and every beat fires once in order. The sky is the scene's
// camera-following NocSky dome (14.2.2 gradient). No game content: the beat production phase replaces these builders.
// Meshes are built with Abyss's MeshBuilder / MeshKit and validated (MeshValidation) at build time.
using System;
using UnityEngine;

namespace Megastructure
{
    public static class GrayboxKit
    {
        public const float FloorMargin = 1.0f;      // floor beyond the lane edge (under the invisible walls)
        public const float SkirtDepth = 0.5f;
        public const float MarkerSpacing = 30f;
        /// <summary>Lateral distance of the start posts outside the lane edge (m).</summary>
        public const double PostOffset = 4.0;
        public const float Bevel = 0.02f;           // 1-3 cm bevels on near geometry (8.7)

        /// <summary>Builds the graybox of beat b (0..6) into its BeatRoot.</summary>
        public static void Build(WorldContext ctx, int b)
        {
            double s0 = NocturneConstants.BeatBoundaries[b], s1 = NocturneConstants.BeatBoundaries[b + 1];
            if (b == NocturneConstants.BeatCount - 1) s1 = NocLanes.Path.Length;
            Transform root = ctx.BeatRoot(b);
            var lib = ctx.Materials;
            string name = "Beat" + (b + 1).ToString("00", System.Globalization.CultureInfo.InvariantCulture) + "_" + NocturneConstants.BeatNames[b];

            // the beat's 14.4 walking surface (NocWet / NocSurface from the library: FloorFor at the beat's mid s; GrayFloor when
            // the library lacks it), so the graybox already shows the wet-floor reflections of the real floors
            Mesh floor = FloorMesh(s0, s1, name + "_floor");
            ctx.AddMesh(root, "floor", floor, FloorMaterial(lib, s0, s1));

            // start posts (emissive) on both sides, PostOffset outside the lane edge: at lane + 0.6 m the bright posts and their
            // bloom left the frame edge within one frame at 1.5 m/s and stepped the frame-mean luma by 3.0-3.3 levels (14.10
            // limit 1.2; a1_boundaries seq_b12 / seq_b34); 4 m out the exit is about 4.7 x gentler (smaller and slower)
            var mk = new MeshBuilder();
            for (int side = -1; side <= 1; side += 2)
            {
                double h = NocLanes.LaneHalfWidth(s0 + 0.5) + PostOffset;
                Vector3 p = NocLanes.CorridorToWorld(s0 + 0.5, side * h, 1.5);
                MeshKit.ChamferBox(mk, p, new Vector3(0.25f, 3.0f, 0.25f), Bevel, Quaternion.Euler(0f, NocLanes.Path.HeadingAt(s0 + 0.5), 0f));
            }
            Mesh posts = mk.ToMesh(name + "_posts");
            ctx.AddMesh(root, "startPosts", posts, lib != null ? (b == NocturneConstants.BeatCount - 1 ? lib.GrayMarkerWarm : lib.GrayMarker) : null);

            // low boxes along the left edge every 30 m (motion cue in captures)
            var bx = new MeshBuilder();
            for (double s = s0 + MarkerSpacing * 0.5; s < s1 - 1.0; s += MarkerSpacing)
            {
                double h = NocLanes.LaneHalfWidth(s) + 0.8;
                Vector3 p = NocLanes.CorridorToWorld(s, -h, 0.2);
                MeshKit.ChamferBox(bx, p, new Vector3(0.4f, 0.4f, 0.4f), Bevel, Quaternion.Euler(0f, NocLanes.Path.HeadingAt(s), 0f));
            }
            if (bx.VertexCount > 0) ctx.AddMesh(root, "edgeMarkers", bx.ToMesh(name + "_markers"), lib != null ? lib.GrayWall : null);

            ValidateMeshes(ctx, name, floor, posts);
        }

        /// <summary>The floor material of the s range s0..s1: lib.FloorFor(0.5 (s0 + s1)) (the 14.4 surface of that beat).</summary>
        public static Material FloorMaterial(MaterialLibrary lib, double s0, double s1) => lib != null ? lib.FloorFor(0.5 * (s0 + s1)) : null;

        /// <summary>The goal of beat 7: a 14.4 m tenement block and the warm 1.2 x 1.5 m window (14.4, 14.11) as graybox boxes.</summary>
        public static void BuildGoal(WorldContext ctx)
        {
            Transform root = ctx.BeatRoot(NocturneConstants.BeatCount - 1);
            var lib = ctx.Materials;
            var block = new MeshBuilder();
            // tenement x 1156-1170, z 379-397, 14.4 m (the window wall is its west face x 1156)
            MeshKit.ChamferBox(block, new Vector3(1163f, 7.2f, 388f), new Vector3(14f, 14.4f, 18f), Bevel);
            ctx.AddMesh(root, "tenement", block.ToMesh("Beat07_tenement"), lib != null ? lib.GrayWall : null);
            var win = new MeshBuilder();
            float y0 = (float)NocturneConstants.GoalWindowY0, y1 = (float)NocturneConstants.GoalWindowY1;
            float z0 = (float)NocturneConstants.GoalWindowZ0, z1 = (float)NocturneConstants.GoalWindowZ1;
            // 6 cm proud of the facade (no co-planar faces, 8.7)
            MeshKit.ChamferBox(win, new Vector3((float)NocturneConstants.GoalWindowX - 0.03f, 0.5f * (y0 + y1), 0.5f * (z0 + z1)), new Vector3(0.06f, y1 - y0, z1 - z0), 0.005f);
            ctx.AddMesh(root, "goalWindow", win.ToMesh("Beat07_goalWindow"), lib != null ? lib.GrayMarkerWarm : null);
        }

        /// <summary>Floor ribbon from s0 to s1: top at y 0, half-width lane + margin, skirts down to -0.5 m.</summary>
        public static Mesh FloorMesh(double s0, double s1, string name)
        {
            var mb = new MeshBuilder();
            int n = Math.Max(1, (int)Math.Ceiling((s1 - s0) / 1.0));
            int[] lt = new int[n + 1], rt = new int[n + 1];
            var up = Vector3.up;
            for (int i = 0; i <= n; i++)
            {
                double s = s0 + (s1 - s0) * i / n;
                double h = NocLanes.LaneHalfWidth(Math.Min(s, NocLanes.Path.Length)) + FloorMargin;
                lt[i] = mb.Vertex(NocLanes.CorridorToWorld(s, -h, 0.0), up, new Vector2((float)-h, (float)s));
                rt[i] = mb.Vertex(NocLanes.CorridorToWorld(s, h, 0.0), up, new Vector2((float)h, (float)s));
            }
            for (int i = 0; i < n; i++) mb.Quad(lt[i], lt[i + 1], rt[i + 1], rt[i]);
            // skirts (flat-shaded, outward)
            for (int i = 0; i < n; i++)
            {
                double sa = s0 + (s1 - s0) * i / n, sb = s0 + (s1 - s0) * (i + 1) / n;
                for (int side = -1; side <= 1; side += 2)
                {
                    double ha = NocLanes.LaneHalfWidth(Math.Min(sa, NocLanes.Path.Length)) + FloorMargin;
                    double hb = NocLanes.LaneHalfWidth(Math.Min(sb, NocLanes.Path.Length)) + FloorMargin;
                    Vector3 ta = NocLanes.CorridorToWorld(sa, side * ha, 0.0), tb = NocLanes.CorridorToWorld(sb, side * hb, 0.0);
                    Vector3 ba = ta + Vector3.down * SkirtDepth, bb = tb + Vector3.down * SkirtDepth;
                    Vector3 outward = (NocLanes.CorridorToWorld(sa, side * (ha + 1.0), 0.0) - ta).normalized;
                    OrientedQuad(mb, ta, tb, bb, ba, outward);
                }
            }
            return mb.ToMesh(name);
        }

        /// <summary>A flat quad whose winding is chosen so its normal points along `want`.</summary>
        static void OrientedQuad(MeshBuilder mb, Vector3 a, Vector3 b, Vector3 c, Vector3 d, Vector3 want)
        {
            Vector3 n = Vector3.Cross(b - a, c - a);
            if (Vector3.Dot(n, want) >= 0f) mb.FlatQuad(a, b, c, d);
            else mb.FlatQuad(a, d, c, b);
        }

        static void ValidateMeshes(WorldContext ctx, string name, params Mesh[] meshes)
        {
            foreach (var m in meshes)
            {
                if (m == null) continue;
                var r = MeshValidation.Validate(m);
                if (!r.Ok) Debug.LogError(FormattableString.Invariant($"[NOC-FAIL] {name}: mesh {m.name} failed validation: {r}"));
            }
        }
    }
}
