// GardenTree: one giant tree of 14.5.5: the trunk as a loft of 24-gon rings (40 rings, denser at the base) from r0 (4-6 m by hash)
// with a root flare and 5 buttress lobes near the ground, tapering to 0.45 r0 at the top (inside the crown), branches, and the crown.
// A crown that must stay inside face B's notch walls leans toward the axis (GardenTreeSpec.CrownZ): the trunk axis bends with it.
// Trunk and branches use lib.Bark, the crown NocFoliage. Load-time only. (GD, Phase 4: the v1 graybox crown and its straight
// branches, -foliage-v 1, are deleted; the Look v2 tree below is the only one.)
//   Look v2 (TEXTURE_PLAN 4): six curved main limbs (quadratic Bezier lofts, 12-gon, leaving the
//      trunk at about 40 deg and rising steeply into the crown) and two sub-limbs per limb that fan out under the crown and enter
//      its underside, so the branching continues into the canopy (CRIT0 #25); the crown becomes the opaque CORE at 0.86 of the
//      LOBED crown surface (GardenLeafCards.CrownShape: sub-crowns with valleys; vertex colour R = the valley AO; lib.LeafCore),
//      and GardenLeafCards puts the leaf-cluster cards on two shells around it (lib.Leaves). Every limb end is pulled inside
//      0.85 x the core along its direction, so no open limb end shows in a valley.
// Bark UV0: x = the arc-length fraction around the ring x N_t x 1.8 m with
// a whole number N_t of trident_maple_bark tiles per limb (N_t from the perimeter at y 8 m for the trunk, at the base for a limb),
// so the texture wraps without a seam step and the grain follows the limb; y = metres along the limb. The trunk ring starts on the
// side facing away from the walk axis (z 195), so the u wrap is never in view. MeshKit.Loft / Lathe UV0 are metres, not wrapped.
using System.Collections.Generic;
using UnityEngine;

namespace Megastructure
{
    public static class GardenTree
    {
        static readonly CrossSection s_ring = new CrossSection();
        static readonly List<LoftFrame> s_frames = new List<LoftFrame>(48);
        static readonly List<float> s_ringY = new List<float>(48);
        static readonly List<Vector3> s_pts = new List<Vector3>(32);
        static float s_r0, s_yTop, s_ringOffset;
        static float s_phase;
        static float s_limbR0, s_limbR1;
        static int s_limbSides, s_limbRings;

        /// <summary>Height (m) of the trunk axis' lean offset at y (0 at the ground, full lean at the crown centre).</summary>
        public static float LeanAt(in GardenTreeSpec t, float y)
        {
            float u = Mathf.Clamp01(y / Mathf.Max(GardenDims.CrownCentreFrac * t.Height, 1f));
            return (t.CrownZ - t.Z) * u * u;
        }

        /// <summary>Trunk radius at height y before the buttress lobes: taper r0 -> 0.45 r0 over the trunk, times the root flare.</summary>
        public static float TrunkRadius(float r0, float y, float yTop)
        {
            float u = Mathf.Clamp01(y / Mathf.Max(yTop, 1f));
            float taper = Mathf.Lerp(1f, GardenDims.TaperEnd, u);
            float flare = 1f + GardenDims.FlareGain * Mathf.Exp(-Mathf.Max(y, 0f) / GardenDims.FlareHeight);
            return r0 * taper * flare;
        }

        /// <summary>Largest ground radius of the trunk (flare and buttress tips), for the spacing checks.</summary>
        public static float FootprintRadius(float r0) => r0 * (1f + GardenDims.FlareGain) * (1f + GardenDims.ButtressGain);

        /// <summary>Whole bark tiles around a limb of the given perimeter (at least 1).</summary>
        public static int BarkTiles(float perimeter) => Mathf.Max(1, Mathf.RoundToInt(perimeter / GardenDims.BarkTile));

        /// <summary>Bark tiles around the trunk: its perimeter at y 8 m (just above the flare, the part seen from the path).</summary>
        public static int TrunkBarkTiles(in GardenTreeSpec t)
            => BarkTiles(2f * Mathf.PI * TrunkRadius(t.R0, GardenDims.BarkRefY, GardenDims.TrunkTopFrac * t.Height));

        /// <summary>Builds tree t: the trunk into `trunk`, the limbs into `limbs` (GD fix wave C33: their own mesh, drawn by NocFoliage
        /// mode 5 with the lantern field; null = into `trunk` as before), the core into `crown`, the cards pending.</summary>
        public static void Build(in GardenTreeSpec t, int seed, MeshBuilder trunk, MeshBuilder crown, MeshBuilder limbs = null)
        {
            float h = t.Height;
            float yTop = GardenDims.TrunkTopFrac * h;
            s_r0 = t.R0; s_yTop = yTop;
            s_phase = 2f * Mathf.PI * BridgeGardenKit.Hash01(seed, 7);
            // the ring starts on the side away from the walk axis: the 24-gon is rotated by a whole number of steps (6 of 15 deg),
            // so the vertex set and the triangles are those of the unrotated ring and only the u wrap moves
            s_ringOffset = (t.Z >= GardenDims.AxisZ ? 0.5f : -0.5f) * Mathf.PI;

            // ---- trunk loft (vertical frames on the leaning axis; rings denser near the flare)
            s_frames.Clear(); s_ringY.Clear();
            int rings = GardenDims.TrunkRings;
            for (int k = 0; k < rings; k++)
            {
                float u = (float)k / (rings - 1);
                float y = -0.3f + (yTop + 0.3f) * Mathf.Pow(u, 1.6f);
                s_ringY.Add(y);
            }
            for (int k = 0; k < rings; k++)
            {
                float y = s_ringY[k];
                var p = new Vector3(t.X, y, t.Z + LeanAt(t, y));
                float yPrev = s_ringY[Mathf.Max(0, k - 1)], yNext = s_ringY[Mathf.Min(rings - 1, k + 1)];
                Vector3 a = new Vector3(t.X, yPrev, t.Z + LeanAt(t, yPrev)), b = new Vector3(t.X, yNext, t.Z + LeanAt(t, yNext));
                Vector3 fwd = (b - a).normalized;
                s_frames.Add(new LoftFrame(p, Quaternion.LookRotation(fwd, Vector3.forward)));
            }
            int first = trunk.VertexCount;
            MeshKit.LoftVarying(trunk, Section, s_frames, false, false);
            WrapBarkLoft(trunk, first, GardenDims.TrunkSides + 1, 0, TrunkBarkTiles(t));

            BuildLimbsV2(t, seed, limbs ?? trunk);

            // ---- core + leaf-cluster cards (the cards wait for EmitCrowns, one mesh per tree)
            CoreV2(crown, t, seed);
            var cards = new PendingCards { Name = t.Name, Mesh = new MeshBuilder(), Tangents = new List<Vector4>(4096) };
            cards.Stats = GardenLeafCards.Build(t, seed, cards.Mesh, cards.Tangents);
            string name = t.Name;
            s_pending.RemoveAll(p => p.Name == name);
            s_pending.Add(cards);
        }

        sealed class PendingCards
        {
            public string Name;
            public MeshBuilder Mesh;
            public List<Vector4> Tangents;
            public GardenLeafCards.Stats Stats;
        }

        static readonly List<PendingCards> s_pending = new List<PendingCards>(17);

        /// <summary>Card meshes built by Build and not yet emitted (tests).</summary>
        public static int PendingCardMeshes => s_pending.Count;

        /// <summary>Runs f on every card mesh built and not yet emitted (GD: the lantern field into colour A).</summary>
        public static void ForEachPendingCards(System.Action<MeshBuilder> f)
        {
            foreach (var p in s_pending) f(p.Mesh);
        }

        /// <summary>
        /// Emits the crowns of every tree built since the last call: the cores ("crowns", m.LeafCore) and one card mesh per tree ("crown_cards_&lt;name&gt;", m.Leaves, with the card
        /// tangents; 32-bit indices above 65 535 vertices). Logs the card counts. Called once by Beat05_Garden.BuildTrees.
        /// </summary>
        public static void EmitCrowns(WorldContext ctx, Transform group, MeshBuilder crowns, GardenMaterials m)
        {
            BridgeGardenKit.Emit(ctx, group, "crowns", crowns, m.LeafCore != null ? m.LeafCore : m.Foliage);
            if (s_pending.Count == 0) return;
            Material leaves = m.Leaves != null ? m.Leaves : m.Foliage;
            int cards = 0, shrunk = 0, dropped = 0;
            long tris = 0;
            foreach (var p in s_pending)
            {
                if (p.Mesh.VertexCount == 0) continue;
                Mesh mesh = p.Mesh.ToMesh(group.name + "_crown_cards_" + p.Name);
                mesh.SetTangents(p.Tangents);
                var r = MeshValidation.Validate(mesh);
                if (!r.Ok) Debug.LogError(System.FormattableString.Invariant($"[NOC-FAIL] {group.name}: mesh {mesh.name} failed validation: {r}"));
                ctx.AddMesh(group, "crown_cards_" + p.Name, mesh, leaves);
                cards += p.Stats.Cards; shrunk += p.Stats.Shrunk; dropped += p.Stats.Dropped;
                tris += p.Mesh.TriangleCount;
            }
            ctx.Log(System.FormattableString.Invariant($"GardenTree: foliage v2, {s_pending.Count} card meshes, {cards} cards ({shrunk} shrunk, {dropped} dropped), {tris} card triangles"));
            s_pending.Clear();
        }

        /// <summary>
        /// v2: six curved main limbs (12-gon lofts, 14 rings) from the trunk axis at 0.20-0.27 h to a point inside the core
        /// (normalised horizontal 0.35, vertical -0.55: radius 0.65 < 0.86, the lowest core lump), leaving the trunk at about
        /// 40 deg and rising steeply; two sub-limbs per limb (10-gon, 10 rings) branch off below the crown and fan out to points
        /// inside the core's underside (normalised horizontal 0.50-0.70, vertical -0.62..-0.45), so the limbs read as a branching
        /// structure entering the canopy. Every end is inside the core (closed by it); seams face up and inward (hidden from below).
        /// </summary>
        static void BuildLimbsV2(in GardenTreeSpec t, int seed, MeshBuilder trunk)
        {
            float h = t.Height, rC = t.CrownR, vr = GardenDims.CrownVertFrac * h;
            Vector3 cc = t.CrownCentre;
            for (int k = 0; k < GardenDims.Branches; k++)
            {
                float az = s_phase + k * (2f * Mathf.PI / GardenDims.Branches) + 0.35f * (BridgeGardenKit.Hash01(seed, 20 + k) - 0.5f);
                float y0 = (0.20f + 0.07f * BridgeGardenKit.Hash01(seed, 30 + k)) * h;
                var a = new Vector3(t.X, y0, t.Z + LeanAt(t, y0));
                var o = new Vector3(Mathf.Cos(az), 0f, Mathf.Sin(az));
                var b = InsideCore(t, seed, cc + new Vector3(0.35f * rC * o.x, -0.55f * vr, 0.35f * rC * o.z));
                Vector3 d = b - a;
                Vector3 dh = new Vector3(d.x, 0f, d.z);
                var ctrl = a + 0.70f * dh + new Vector3(0f, 0.25f * d.y, 0f);
                float ra = Mathf.Max(0.11f * t.R0, 0.3f), rb = Mathf.Max(GardenDims.BranchMinRadius + 0.05f, 0.04f * t.R0);
                Limb(trunk, a, ctrl, b, ra, rb, 12, 14, o);

                for (int j = 0; j < 2; j++)
                {
                    float us = 0.40f + 0.22f * BridgeGardenKit.Hash01(seed, 40 + 2 * k + j);
                    Vector3 ps = Bezier(a, ctrl, b, us);
                    float daz = (j == 0 ? -1f : 1f) * (0.35f + 0.30f * BridgeGardenKit.Hash01(seed, 60 + 2 * k + j));
                    var os = new Vector3(Mathf.Cos(az + daz), 0f, Mathf.Sin(az + daz));
                    float hr = 0.50f + 0.20f * BridgeGardenKit.Hash01(seed, 80 + 2 * k + j);
                    float vy = -0.62f + 0.17f * BridgeGardenKit.Hash01(seed, 100 + 2 * k + j);
                    var e = InsideCore(t, seed, cc + new Vector3(hr * rC * os.x, vy * vr, hr * rC * os.z));
                    Vector3 ds = e - ps;
                    Vector3 dsh = new Vector3(ds.x, 0f, ds.z);
                    var cs = ps + 0.60f * dsh + new Vector3(0f, 0.20f * ds.y, 0f);
                    float rLimb = Mathf.Lerp(ra, rb, Mathf.Pow(us, 0.8f));
                    float sa = Mathf.Max(0.55f * rLimb, 0.18f), sb = Mathf.Max(GardenDims.BranchMinRadius, 0.35f * sa);
                    Limb(trunk, ps, cs, e, sa, sb, 10, 10, os);
                }
            }
        }

        /// <summary>Limb-end clamp (v2): p pulled toward the crown centre to at most 0.85 x the lobed core radius along its direction.</summary>
        public static Vector3 InsideCore(in GardenTreeSpec t, int seed, Vector3 p)
        {
            Vector3 d = GardenLeafCards.NormDir(t, p);
            float coreN = GardenDims.CoreFrac * GardenLeafCards.Shape(t, seed).Factor(d);
            float rn = GardenLeafCards.NormRadius(t, p);
            float lim = LimbEndInCore * coreN;
            return rn > lim ? t.CrownCentre + (p - t.CrownCentre) * (lim / rn) : p;
        }

        /// <summary>Limb ends stay inside this fraction of the core radius.</summary>
        public const float LimbEndInCore = 0.85f;

        /// <summary>The v2 core of tree t: the lobed crown surface at CoreFrac, vertex colour R = the valley AO (1 on lobe tops).</summary>
        public static void CoreV2(MeshBuilder mb, in GardenTreeSpec t, int seed)
            => Crown(mb, t.CrownCentre, t.CrownRadii * GardenDims.CoreFrac, seed, GardenLeafCards.Shape(t, seed));

        static Vector3 Bezier(Vector3 a, Vector3 c, Vector3 b, float u)
        {
            float v = 1f - u;
            return v * v * a + 2f * u * v * c + u * u * b;
        }

        /// <summary>A tapered limb along the quadratic Bezier a-c-b (radius ra -> rb), open ends, bark UV0 wrapped.</summary>
        static void Limb(MeshBuilder mb, Vector3 a, Vector3 c, Vector3 b, float ra, float rb, int sides, int rings, Vector3 outward)
        {
            s_pts.Clear();
            for (int i = 0; i < rings; i++) s_pts.Add(Bezier(a, c, b, (float)i / (rings - 1)));
            // section up = -(tangential): the ring's first point (u = 0, the wrap) lies on the upper inner side of the limb
            var upHint = new Vector3(-outward.z, 0f, outward.x);
            MeshKit.PathFrames(s_pts, upHint, s_frames, false, 89f);
            s_limbR0 = ra; s_limbR1 = rb; s_limbSides = sides; s_limbRings = rings;
            int first = mb.VertexCount;
            MeshKit.LoftVarying(mb, LimbSection, s_frames, false, false);
            WrapBarkLoft(mb, first, sides + 1, 0, BarkTiles(2f * Mathf.PI * ra));
        }

        static CrossSection LimbSection(int k)
        {
            float u = (float)k / Mathf.Max(s_limbRings - 1, 1);
            return MeshKit.Circle(Mathf.Lerp(s_limbR0, s_limbR1, Mathf.Pow(u, 0.8f)), s_limbSides, s_ring);
        }

        /// <summary>
        /// Rewrites UV0.x of a closed smooth loft (rings of `columns` vertices, the last column closing the ring at u = perimeter) as
        /// the arc-length fraction x tiles x 1.8 m: a whole number of bark tiles around every ring (no seam step).
        /// </summary>
        public static void WrapBarkLoft(MeshBuilder mb, int first, int columns, int capVertices, int tiles)
        {
            int ringVerts = mb.VertexCount - first - capVertices;
            float span = tiles * GardenDims.BarkTile;
            for (int r = first; r + columns <= first + ringVerts; r += columns)
            {
                float total = mb.UV0[r + columns - 1].x;
                float inv = total > 1e-6f ? 1f / total : 0f;
                for (int c = 0; c < columns; c++)
                {
                    Vector2 uv = mb.UV0[r + c];
                    mb.UV0[r + c] = new Vector2(c == columns - 1 ? span : uv.x * inv * span, uv.y);
                }
            }
        }

        /// <summary>Ring k of the trunk loft: 24-gon with the buttress lobes near the ground.</summary>
        static CrossSection Section(int k)
        {
            float y = s_ringY[k];
            float r = TrunkRadius(s_r0, y, s_yTop);
            float bw = GardenDims.ButtressGain * Mathf.Exp(-Mathf.Max(y, 0f) / GardenDims.ButtressHeight);
            s_ring.Clear(true);
            int n = GardenDims.TrunkSides;
            for (int i = 0; i < n; i++)
            {
                float a = 2f * Mathf.PI * i / n + s_ringOffset;
                float c = Mathf.Cos(GardenDims.Buttresses * (a - s_phase));
                float lobe = c > 0f ? c * c * c : 0f;
                float rr = r * (1f + bw * lobe);
                s_ring.Add(new Vector2(rr * Mathf.Cos(a), rr * Mathf.Sin(a)));
            }
            return s_ring;
        }

        static readonly List<Vector3> s_cp = new List<Vector3>(1200);
        static readonly List<Vector3> s_cn = new List<Vector3>(1200);
        static readonly List<int> s_ci = new List<int>(7000);

        /// <summary>The noise offsets of a crown's lumps (shared by the crown, the core and the card shells).</summary>
        public static Vector3 LumpOffsets(int seed)
            => new Vector3(31.7f * BridgeGardenKit.Hash01(seed, 61), 17.3f * BridgeGardenKit.Hash01(seed, 62), 23.9f * BridgeGardenKit.Hash01(seed, 63));

        static readonly List<float> s_cv = new List<float>(1200);

        /// <summary>The lobed crown surface (GardenLeafCards.CrownShape) scaled to radii, with shared vertices (poles + lat-1 rings of
        /// lon vertices), outward winding, smooth normals and the valley AO in vertex colour R.</summary>
        public static void Crown(MeshBuilder mb, Vector3 c, Vector3 radii, int seed, GardenLeafCards.CrownShape shape)
        {
            int lon = GardenDims.CrownLon, lat = GardenDims.CrownLat;
            s_cp.Clear(); s_cn.Clear(); s_ci.Clear(); s_cv.Clear();
            Vector3 o = LumpOffsets(seed);
            // south pole, rings, north pole
            AddPoint(c, radii, new Vector3(0f, -1f, 0f), o, shape);
            for (int i = 1; i < lat; i++)
            {
                float th = Mathf.PI * i / lat;
                for (int j = 0; j < lon; j++)
                {
                    float ph = 2f * Mathf.PI * j / lon;
                    var d = new Vector3(Mathf.Sin(th) * Mathf.Cos(ph), -Mathf.Cos(th), Mathf.Sin(th) * Mathf.Sin(ph));
                    AddPoint(c, radii, d, o, shape);
                }
            }
            AddPoint(c, radii, new Vector3(0f, 1f, 0f), o, shape);
            int north = s_cp.Count - 1;
            for (int j = 0; j < lon; j++) Tri(c, 0, 1 + (j + 1) % lon, 1 + j);
            for (int i = 0; i < lat - 2; i++)
                for (int j = 0; j < lon; j++)
                {
                    int a = 1 + i * lon + j, b = 1 + i * lon + (j + 1) % lon, a2 = a + lon, b2 = b + lon;
                    Tri(c, a, b, b2);
                    Tri(c, a, b2, a2);
                }
            int lastRing = 1 + (lat - 2) * lon;
            for (int j = 0; j < lon; j++) Tri(c, north, lastRing + j, lastRing + (j + 1) % lon);

            // accumulated (area-weighted) face normals
            for (int i = 0; i < s_cp.Count; i++) s_cn.Add(Vector3.zero);
            for (int t = 0; t < s_ci.Count; t += 3)
            {
                int a = s_ci[t], b = s_ci[t + 1], d = s_ci[t + 2];
                Vector3 n = Vector3.Cross(s_cp[b] - s_cp[a], s_cp[d] - s_cp[a]);
                s_cn[a] += n; s_cn[b] += n; s_cn[d] += n;
            }
            int first = mb.VertexCount;
            Color32 keep = mb.Color;
            for (int i = 0; i < s_cp.Count; i++)
            {
                Vector3 n = s_cn[i].sqrMagnitude > 1e-12f ? s_cn[i].normalized : (s_cp[i] - c).normalized;
                mb.Color = new Color32((byte)Mathf.RoundToInt(255f * Mathf.Lerp(GardenDims.ValleyAo, 1f, s_cv[i])), 0, 0, 255);
                mb.Vertex(s_cp[i], n, new Vector2(s_cp[i].x, s_cp[i].z));
            }
            mb.Color = keep;
            for (int t = 0; t < s_ci.Count; t += 3) mb.Tri(first + s_ci[t], first + s_ci[t + 1], first + s_ci[t + 2]);
        }

        static void AddPoint(Vector3 c, Vector3 radii, Vector3 d, Vector3 o, GardenLeafCards.CrownShape shape)
        {
            float k = shape.Factor(d, out float valley);
            s_cp.Add(c + Vector3.Scale(d, radii) * k);
            s_cv.Add(valley);
        }

        /// <summary>Appends triangle (a, b, d) wound outward (front face where cross(b - a, d - a) points away from the centre).</summary>
        static void Tri(Vector3 c, int a, int b, int d)
        {
            Vector3 n = Vector3.Cross(s_cp[b] - s_cp[a], s_cp[d] - s_cp[a]);
            Vector3 m = (s_cp[a] + s_cp[b] + s_cp[d]) / 3f - c;
            if (Vector3.Dot(n, m) >= 0f) { s_ci.Add(a); s_ci.Add(b); s_ci.Add(d); }
            else { s_ci.Add(a); s_ci.Add(d); s_ci.Add(b); }
        }

        /// <summary>3D value noise in [0, 1] (quintic, PCG lattice), the CPU twin of the shader's leaf-cell noise.</summary>
        public static float Noise3(float x, float y, float z)
        {
            int ix = Mathf.FloorToInt(x), iy = Mathf.FloorToInt(y), iz = Mathf.FloorToInt(z);
            float fx = x - ix, fy = y - iy, fz = z - iz;
            float ux = fx * fx * fx * (fx * (fx * 6f - 15f) + 10f), uy = fy * fy * fy * (fy * (fy * 6f - 15f) + 10f), uz = fz * fz * fz * (fz * (fz * 6f - 15f) + 10f);
            float H(int a, int b, int cc) => (float)NocMath.U01(NocMath.Pcg3(a, b, cc));
            float x00 = Mathf.Lerp(H(ix, iy, iz), H(ix + 1, iy, iz), ux), x10 = Mathf.Lerp(H(ix, iy + 1, iz), H(ix + 1, iy + 1, iz), ux);
            float x01 = Mathf.Lerp(H(ix, iy, iz + 1), H(ix + 1, iy, iz + 1), ux), x11 = Mathf.Lerp(H(ix, iy + 1, iz + 1), H(ix + 1, iy + 1, iz + 1), ux);
            return Mathf.Lerp(Mathf.Lerp(x00, x10, uy), Mathf.Lerp(x01, x11, uy), uz);
        }
    }
}
