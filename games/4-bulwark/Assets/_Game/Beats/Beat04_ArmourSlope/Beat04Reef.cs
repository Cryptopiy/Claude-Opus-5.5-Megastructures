// Beat04ArmourSlope reef (14.5.4 "Geometry"): every unit of ReefLayout.All() baked into static world-space chunks of
// BwArmour (the library Armour material): islet piles one renderer per islet (LOD0), grid units in 200 m chunks, LOD0 for
// x < 400 and LOD1 beyond; each unit clipped at y -13 (ColossopodMesh.AppendClipped: only what can show above the troughs
// is built). No LOD switching at run time, so nothing can pop (acceptance 6 by construction).
// Gauge platforms (14.5.4 "Gauges"): on every gauge unit (ReefLayout: one tip in five within 300 m, plus the crest units
// of islets 1 and 3) a level 2.4 m SteelOchre platform 1.0 m above the rim of the banded top arm on four steel legs, a
// 1.1 m round handrail with a 0.55 m knee rail (BwLine, 0.05 m), a short ladder through the rail gap, and a 3.5 m
// instrument mast with a box: the human reference on the unit.
// Surf (BwSurf): 2.0 m grids over the southern reef x 6.5-800 (and 5 m over the northern reef to x 800), density baked per
// vertex from the hull distance at the mean waterline (exp(-d / 5 m)) and the islet skirts; quads whose four densities are
// all below 0.03 are not built.
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Megastructure.Beats
{
    public sealed partial class Beat04ArmourSlope
    {
        public const float ChunkSize = 200f;
        /// <summary>Surf grid spacing (m): 2.0 in the south, so the bilinear foam collar sits within 1.0 m of the hull outline
        /// (14.5.4 acceptance 3: half a cell at most); 5 m over the northern reef (seen only beyond 600 m). BwSurf's own
        /// _Spacing (2.5 m: the wave components the overlay carries) is independent of the mesh spacing.</summary>
        public const float SurfSpacing = 2.0f, SurfSpacingNorth = 5f;
        public const float SurfX0 = 6.5f, SurfX1 = 800f;
        public const float GaugeDeck = 2.4f, GaugeAbove = 1.0f, GaugeRailH = 1.1f, GaugeMast = 3.5f;

        /// <summary>Unit counts and triangles of the reef (for the build log and the tests).</summary>
        public int UnitsBuilt { get; private set; }
        public long ReefTriangles { get; private set; }
        public int GaugeCount { get; private set; }
        public long SurfTriangles { get; private set; }
        /// <summary>World bounds of each islet's renderer (key-shot subjects).</summary>
        public readonly Bounds[] IsletBounds = new Bounds[6];

        void BuildReef(WorldContext ctx, Transform root)
        {
            var units = ReefLayout.All();
            var lod0 = ColossopodMesh.Build(0, -1);
            var lod1 = ColossopodMesh.Build(1, -1);
            var chunks = new Dictionary<long, MeshBuilder>();
            var isl = new MeshBuilder[ReefLayout.IsletCount];
            var remap = new List<int>(4096);
            long tris = 0;
            int built = 0;
            var gauges = new List<ReefLayout.Unit>();
            var buckets = Buckets(units);
            for (int n = 0; n < units.Count; n++)
            {
                var u = units[n];
                MeshBuilder src;
                bool near = u.kind == ReefLayout.Kind.Islet || u.centre.x < ReefLayout.Lod0MaxX;
                // a gauge unit: its own mesh with the tide-gauge strip of the top arm facing the road (west)
                if (u.gauge) { src = ColossopodMesh.Build(0, u.topArm, Quaternion.Inverse(u.rot) * Vector3.left); gauges.Add(u); }
                else src = near ? lod0 : lod1;
                MeshBuilder dst;
                if (u.kind == ReefLayout.Kind.Islet)
                {
                    dst = isl[u.islet] ?? (isl[u.islet] = new MeshBuilder());
                }
                else
                {
                    long key = ((long)Mathf.FloorToInt(u.centre.x / ChunkSize) << 32) ^ (long)(uint)(Mathf.FloorToInt(u.centre.z / ChunkSize) * 2 + (near ? 0 : 1));
                    if (!chunks.TryGetValue(key, out dst)) { dst = new MeshBuilder(); chunks.Add(key, dst); }
                }
                int v0 = dst.VertexCount;
                byte tone = (byte)(BwMath.Pcg((uint)n * 2654435761u + 17u) >> 24);
                int t = ColossopodMesh.AppendClipped(dst, src, Matrix4x4.TRS(u.centre, u.rot, Vector3.one), ReefLayout.ClipY, remap, tone);
                if (t > 0) { tris += t; built++; }
                if (near) ContactOcclusion(dst, v0, units, n, buckets);
            }
            for (int k = 0; k < isl.Length; k++)
                if (isl[k] != null)
                {
                    var mr = Add(ctx, root, isl[k], "b4_islet" + (k + 1), m_armour);
                    if (mr != null) IsletBounds[k] = mr.bounds;
                }
            var keys = new List<long>(chunks.Keys);
            keys.Sort();
            foreach (long key in keys) Add(ctx, root, chunks[key], "b4_reef_" + key.ToString("x"), m_armour);
            UnitsBuilt = built;
            ReefTriangles = tris;
            BuildGauges(ctx, root, gauges);
        }

        // ---------------------------------------------------------------- contact occlusion (vertex R)
        /// <summary>Bucket size (m) of the neighbour query: a unit reaches 30.5 m from its centre, so units whose centres are
        /// more than 2 x 30.5 + 8 m apart cannot occlude each other.</summary>
        const float BucketSize = 70f;

        static Dictionary<long, List<int>> Buckets(IReadOnlyList<ReefLayout.Unit> units)
        {
            var b = new Dictionary<long, List<int>>();
            for (int n = 0; n < units.Count; n++)
            {
                long key = BucketKey(Mathf.FloorToInt(units[n].centre.x / BucketSize), Mathf.FloorToInt(units[n].centre.z / BucketSize));
                if (!b.TryGetValue(key, out var l)) { l = new List<int>(8); b.Add(key, l); }
                l.Add(n);
            }
            return b;
        }

        static long BucketKey(int i, int j) => ((long)i << 32) ^ (uint)j;

        /// <summary>The islets are beyond the 150 m shadow distance (14.2.3), so the piles get their depth from a baked contact
        /// term instead: every vertex of a unit (from index v0) darkens with its distance to the nearest OTHER unit
        /// (0.45 at contact, 1 from 7 m), multiplied into vertex R (AO). Built once at load; LOD0 units only.</summary>
        static void ContactOcclusion(MeshBuilder mb, int v0, IReadOnlyList<ReefLayout.Unit> units, int self, Dictionary<long, List<int>> buckets)
        {
            var u = units[self];
            int bi = Mathf.FloorToInt(u.centre.x / BucketSize), bj = Mathf.FloorToInt(u.centre.z / BucketSize);
            var near = new List<int>(16);
            for (int dj = -1; dj <= 1; dj++)
                for (int di = -1; di <= 1; di++)
                    if (buckets.TryGetValue(BucketKey(bi + di, bj + dj), out var l))
                        foreach (int m in l)
                            if (m != self && (units[m].centre - u.centre).sqrMagnitude < 70f * 70f) near.Add(m);
            if (near.Count == 0) return;
            for (int v = v0; v < mb.VertexCount; v++)
            {
                Vector3 p = mb.Positions[v];
                float d = float.MaxValue;
                for (int k = 0; k < near.Count; k++) d = Mathf.Min(d, ReefLayout.Distance(units[near[k]], p));
                float occ = Mathf.Lerp(0.45f, 1f, Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((d - 0.2f) / 6.8f)));
                var c = mb.Colors[v];
                c.r = (byte)Mathf.RoundToInt(c.r * occ);
                mb.Colors[v] = c;
            }
        }

        // ---------------------------------------------------------------- gauge platforms
        /// <summary>The deck top of a gauge unit's platform (world): over the top arm's end-face centre, 1.0 m above its
        /// highest rim point.</summary>
        public static Vector3 GaugeDeckTop(in ReefLayout.Unit u)
        {
            Vector3 tip = u.Tip(u.topArm), d = u.Dir(u.topArm);
            float rim = (ReefLayout.ArmR1 - ReefLayout.EndChamfer) * Mathf.Sqrt(Mathf.Max(0f, 1f - d.y * d.y));
            return new Vector3(tip.x, tip.y + rim + GaugeAbove, tip.z);
        }

        void BuildGauges(WorldContext ctx, Transform root, List<ReefLayout.Unit> gauges)
        {
            var deck = new MeshBuilder();
            var legs = new MeshBuilder();
            var rails = new MeshBuilder();
            var boxes = new MeshBuilder();
            float hd = 0.5f * GaugeDeck;
            float rr = 0.025f;
            foreach (var u in gauges)
            {
                Vector3 top = GaugeDeckTop(u);
                Vector3 tip = u.Tip(u.topArm), d = u.Dir(u.topArm);
                // deck 2.4 x 2.4 x 0.1 (SteelOchre, grating read at this distance)
                MeshKit.ChamferBox(deck, top - new Vector3(0f, 0.05f, 0f), new Vector3(GaugeDeck, 0.1f, GaugeDeck), 0.01f);
                // four legs down to the end-face plane
                for (int c = 0; c < 4; c++)
                {
                    float sx = (c & 1) == 0 ? -1f : 1f, sz = (c & 2) == 0 ? -1f : 1f;
                    var p = new Vector3(top.x + sx * (hd - 0.12f), 0f, top.z + sz * (hd - 0.12f));
                    // y on the face plane through tip with normal d: d.x (x - tx) + d.y (y - ty) + d.z (z - tz) = 0
                    float yFace = tip.y - (d.x * (p.x - tip.x) + d.z * (p.z - tip.z)) / Mathf.Max(d.y, 0.2f);
                    float y0 = yFace - 0.15f, y1 = top.y - 0.1f;
                    if (y1 - y0 > 0.05f) MeshKit.ChamferBox(legs, new Vector3(p.x, 0.5f * (y0 + y1), p.z), new Vector3(0.12f, y1 - y0, 0.12f), 0.01f);
                }
                // handrail posts at the corners and mid-sides, 1.1 m; rails at 1.1 and 0.55 (gap on the south side for the ladder)
                for (int c = 0; c < 4; c++)
                {
                    float sx = (c & 1) == 0 ? -1f : 1f, sz = (c & 2) == 0 ? -1f : 1f;
                    // posts start 6 cm inside the 0.1 m deck: the BwLine tube's open, clip-widened end never shows
                    var b = new Vector3(top.x + sx * (hd - 0.05f), top.y - 0.06f, top.z + sz * (hd - 0.05f));
                    Tube(rails, b, b + new Vector3(0f, GaugeRailH + 0.06f, 0f), rr, 6);
                }
                Vector3 c00 = new Vector3(top.x - hd + 0.05f, top.y, top.z - hd + 0.05f), c10 = new Vector3(top.x + hd - 0.05f, top.y, top.z - hd + 0.05f);
                Vector3 c01 = new Vector3(top.x - hd + 0.05f, top.y, top.z + hd - 0.05f), c11 = new Vector3(top.x + hd - 0.05f, top.y, top.z + hd - 0.05f);
                foreach (float h in new[] { GaugeRailH - rr, 0.55f - rr })
                {
                    var up = new Vector3(0f, h, 0f);
                    Tube(rails, c01 + up, c11 + up, rr, 6);
                    Tube(rails, c00 + up, c01 + up, rr, 6);
                    Tube(rails, c10 + up, c11 + up, rr, 6);
                    // south side: two short pieces either side of the 0.7 m ladder gap
                    Tube(rails, c00 + up, c00 + up + new Vector3(0.85f, 0f, 0f), rr, 6);
                    Tube(rails, c10 + up - new Vector3(0.85f, 0f, 0f), c10 + up, rr, 6);
                }
                // ladder: stringers and rungs 0.3 m from the deck edge down 2.4 m
                for (int s = -1; s <= 1; s += 2)
                {
                    var a = new Vector3(top.x + s * 0.25f, top.y + 1.0f, top.z - hd - 0.05f);
                    Tube(rails, a, a - new Vector3(0f, 3.4f, 0f), rr, 6);
                }
                for (int r = 0; r < 8; r++)
                {
                    var a = new Vector3(top.x - 0.25f, top.y - 0.3f * r - 0.1f, top.z - hd - 0.05f);
                    Tube(rails, a, a + new Vector3(0.5f, 0f, 0f), 0.018f, 6);
                }
                // instrument mast (steel, 3.5 m) with a box and a cross arm at the top
                var m0 = new Vector3(top.x + 0.6f, top.y, top.z + 0.6f);
                MeshKit.TaperedCylinder(legs, m0, m0 + new Vector3(0f, GaugeMast, 0f), 0.06f, 0.04f, 10, true);
                // the instrument cabinet and the gauge head are dark-painted (ConcreteDark tone): their small flat sunlit tops
                // stayed lone bright pixels in SteelPaint at 150-300 m (reef-05 NaN-like specks)
                MeshKit.ChamferBox(boxes, m0 + new Vector3(-0.35f, 0.45f, -0.35f), new Vector3(0.6f, 0.9f, 0.45f), 0.02f);
                // cross arm and a radar-gauge head big enough to stay >= 2 px at 150 m in 1080p (0.4 m = 2.4 px): no lone
                // sunlit pixel against the sky
                MeshKit.ChamferBox(boxes, m0 + new Vector3(0f, GaugeMast - 0.2f, 0f), new Vector3(1.2f, 0.1f, 0.1f), 0.01f);
                MeshKit.ChamferBox(boxes, m0 + new Vector3(0.45f, GaugeMast - 0.42f, 0f), new Vector3(0.4f, 0.4f, 0.4f), 0.02f);
            }
            GaugeCount = gauges.Count;
            Add(ctx, root, deck, "b4_gauge_decks", m_gaugeOchre ?? m_ochre);
            Add(ctx, root, legs, "b4_gauge_steel", m_gaugeSteel ?? m_steel);
            Add(ctx, root, boxes, "b4_gauge_boxes", m_dark);
            Add(ctx, root, rails, "b4_gauge_rails", m_handrail, false);
        }

        // ---------------------------------------------------------------- surf grids (BwSurf)
        /// <summary>Baked surf density at (x, z) of the mean waterline: hull foam max(exp(-d / 1.8 m), 0.45 exp(-d / 7 m)) (1 inside), the islet skirts
        /// (0.42 from the pile to R - 5, 0 at R + 25), a broader collar round the islet units (0.85 exp(-d / 4 m)), faded out over
        /// x 600-800.</summary>
        /// <summary>The foam RING of the units (14.5.4 acceptance 3: it coincides with the footprints): 1 inside the hull's
        /// waterline section, exp(-d / 1.8 m) outside (d = distance to the nearest hull at the mean waterline).</summary>
        public static float SurfCollar(float x, float z)
        {
            float d = ReefLayout.HullDistance(new Vector3(x, (float)BulwarkConst.SeaY, z));
            return d <= 0f ? 1f : Mathf.Exp(-d / 1.8f);
        }

        public static float SurfDensity(float x, float z)
        {
            var p = new Vector3(x, (float)BulwarkConst.SeaY, z);
            float d = ReefLayout.HullDistance(p);
            // a solid white-water collar at the hull (e-fold 1.8 m: SurfCollar) and a sparse lacy field farther out (e-fold
            // 7 m, x 0.45)
            float hull = d <= 0f ? 1f : Mathf.Max(Mathf.Exp(-d / 1.8f), 0.45f * Mathf.Exp(-d / 7f));
            float skirt = 0f, pile = 0f;
            for (int k = 0; k < ReefLayout.IsletCount; k++)
            {
                float r = (new Vector2(x, z) - ReefLayout.IsletCentre(k)).magnitude, R = ReefLayout.IsletRadius[k];
                skirt = Mathf.Max(skirt, 1f - Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(R - 5f, R + 25f, r)));
                // the swell crashes on the piles: a broader white collar round the islet units (e-fold 4 m)
                if (r < R + 2f * ReefLayout.ArmLength)
                {
                    float di = ReefLayout.IsletHullDistance(k, p);
                    pile = Mathf.Max(pile, di <= 0f ? 1f : 0.85f * Mathf.Exp(-di / 4f));
                }
            }
            float dens = Mathf.Max(Mathf.Max(hull, pile), 0.42f * skirt);
            return dens * (1f - Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(600f, SurfX1, x)));
        }

        void BuildSurf(WorldContext ctx, Transform root)
        {
            if (m_surf == null) return;
            long t0 = 0;
            t0 += SurfGrid(ctx, root, "b4_surf_s", SurfX0, SurfX1, (float)BulwarkConst.ReefZ0 + 0.5f, (float)BulwarkConst.ReefZ1 + 40f, SurfSpacing);
            t0 += SurfGrid(ctx, root, "b4_surf_n", SurfX0, SurfX1, (float)BulwarkConst.ReefNorthZ0 - 40f, 2000f, SurfSpacingNorth);
            SurfTriangles = t0;
        }

        long SurfGrid(WorldContext ctx, Transform root, string name, float x0, float x1, float z0, float z1, float sp)
        {
            int nx = Mathf.CeilToInt((x1 - x0) / sp), nz = Mathf.CeilToInt((z1 - z0) / sp);
            var dens = new float[(nx + 1) * (nz + 1)];
            for (int j = 0; j <= nz; j++)
                for (int i = 0; i <= nx; i++)
                    dens[j * (nx + 1) + i] = SurfDensity(x0 + sp * i, z0 + sp * j);
            long tris = 0;
            // chunks of 200 m along x so the grids frustum-cull
            int per = Mathf.Max(1, Mathf.RoundToInt(ChunkSize / sp));
            for (int ci = 0; ci < nx; ci += per)
            {
                var mb = new MeshBuilder();
                var map = new Dictionary<int, int>();
                int ce = Mathf.Min(nx, ci + per);
                for (int j = 0; j < nz; j++)
                    for (int i = ci; i < ce; i++)
                    {
                        int a = j * (nx + 1) + i, b = a + 1, c = a + nx + 1, d = c + 1;
                        if (dens[a] < 0.03f && dens[b] < 0.03f && dens[c] < 0.03f && dens[d] < 0.03f) continue;
                        int va = SurfVertex(mb, map, a, nx, x0, z0, sp, dens), vb = SurfVertex(mb, map, b, nx, x0, z0, sp, dens);
                        int vc = SurfVertex(mb, map, c, nx, x0, z0, sp, dens), vd = SurfVertex(mb, map, d, nx, x0, z0, sp, dens);
                        mb.Quad(va, vc, vd, vb);   // clockwise from above: (x, z) -> (x, z+) -> (x+, z+) -> (x+, z)
                    }
                if (mb.VertexCount == 0) continue;
                tris += mb.TriangleCount;
                var mr = Add(ctx, root, mb, name + "_" + ci, m_surf, false);
                if (mr != null)
                {
                    // the sea displaces the vertices by up to +-4 m (Gerstner + the kraken's mounds): bounds must contain it
                    var bnd = mr.bounds;
                    bnd.Expand(new Vector3(8f, 24f, 8f));
                    mr.localBounds = new Bounds(bnd.center, bnd.size);
                }
            }
            return tris;
        }

        static int SurfVertex(MeshBuilder mb, Dictionary<int, int> map, int g, int nx, float x0, float z0, float sp, float[] dens)
        {
            if (map.TryGetValue(g, out int v)) return v;
            int i = g % (nx + 1), j = g / (nx + 1);
            mb.Color = new Color32((byte)Mathf.RoundToInt(255f * Mathf.Clamp01(dens[g])), 0, 0, 255);
            v = mb.Vertex(new Vector3(x0 + sp * i, (float)BulwarkConst.SeaY, z0 + sp * j), Vector3.up);
            map.Add(g, v);
            return v;
        }
    }
}
