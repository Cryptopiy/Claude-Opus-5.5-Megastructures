// GardenLeafCards: the Look v2 leaf-cluster cards of one crown (TEXTURE_PLAN 4, FO; built only with -foliage-v 2; CRIT0 #10).
// The crown ellipsoid (r, 0.28 h, r) at 0.66 h becomes an opaque core at 0.90 of its radii (GardenTree.Crown, lib.LeafCore) plus
// square cards of 1.6-2.6 m on two shells at 0.94 and 1.02 of the radii (x the core's own lumps, so the cards follow its shape,
// +-1.5 % radial jitter so the shells are not two flat skins). Each card shows one of the 4 cluster sprites of the LeafCrown atlas
// (texprep composes them from LeafSet024 at true leaf size: 0.13-0.17 m leaves on a 2 m sprite, foliage.json), so a crown is
// thousands of leaf clusters with a broken silhouette and sky holes between the shells and the core.
// Placement: a Fibonacci lattice of n0 points on the unit sphere, jittered by 0.45 of its spacing in the tangent plane, thinned by
// the ellipsoid's area element (uniform density on the oblate crown) and by a low-frequency clump noise (density x (1 +- 0.45):
// clusters and gaps, not an even fur). n0 is chosen so the kept count is 1.5 x shell area / 4.4 m^2 (the mean card area of the
// 1.6-2.6 m sizes, 4.49 m^2), x 0.5 for the back trees.
// Every card corner stays inside 1.10 x the ellipsoid (the envelope V's audit may use, Envelope) and NotchClearance (1 m) inside
// face B's notch walls (z 115 / 275 east of x 815.3 - 1); a card that does not fit is shrunk to 1.2 m, else dropped.
// Card normal = the ellipsoid normal tilted by up to 50 deg about a random axis; in-plane rotation random.
// Vertex contract (NocFoliage _LeafMode 2):
//   normal  = the ellipsoid normal at the vertex (smooth over the whole crown: the shader's bent-normal target; within 50 deg
//             of the card normal, so the winding check of MeshValidation holds);
//   tangent = the card's u axis (w +1), set by GardenTree.EmitCrowns;
//   uv0     = sprite uv 0..1; uv1.x = sprite slice 0..3, uv1.y = card hash [0, 1) (hue jitter), uv1.zw = the octahedral card
//             normal (the same at the 4 corners, so interpolation is exact; the shader flips it on back faces, NOC_FRONT_FACE,
//             and the card's v axis is cross(card normal, u): GL normal maps, +Y = +v);
//   colour  = R card AO (inner shell 0.72, outer 1.0, x 0.85 .. 1 from the crown underside to its top), G = 255 on the sparse
//             bioluminescent cards (2.5 %), B = sky exposure (1 + d.y) / 2 (x 0.6 on the inner shell: the share of the lid that
//             reaches the card past the crown; it scales the transmitted lid light, no shadow maps), A = 255.
// Lobes (CrownShape): the crown surface is the ellipsoid x L(d) (GardenDims lobe constants): core and both shells follow it, the
// card normal is the lobed surface normal (finite differences) tilted by up to 50 deg, cards thin out toward the valley floors and
// their AO / sky exposure drop there (valley = 0 on a floor, 1 on a lobe top).
// Outer shell first in the index order (the nearer layer from any outside view; early-Z culls more of the inner one), and
// within a shell from the bottom up (the walk sees the crowns from below).
// Load-time only. No allocation per frame.
using System.Collections.Generic;
using UnityEngine;

namespace Megastructure
{
    public static class GardenLeafCards
    {
        /// <summary>Cluster sprites of the LeafCrown atlas (2 x 2 flipbook imported as a Texture2DArray of 4 slices).</summary>
        public const int Sprites = 4;
        /// <summary>Share of the cards that carry bioluminescent specks (LOOK_V2: LeafGlow only as sparse cells).</summary>
        public const float GlowCardFraction = 0.025f;
        public const float InnerShellAo = 0.72f, UndersideAo = 0.85f, RadialJitter = 0.015f;
        /// <summary>Sky exposure of the inner shell relative to the outer one (the outer cards shade it from the lid).</summary>
        public const float InnerShellExposure = 0.6f;
        /// <summary>Sky exposure of an outer-shell card on the crown's underside (it sees the sky past the crown's side).</summary>
        public const float OuterExposureMin = 0.25f;
        /// <summary>Cosine of the largest angle between a corner's bent-normal target and its card normal (54 deg).</summary>
        public static readonly float CosMaxBend = Mathf.Cos(54f * Mathf.Deg2Rad);

        /// <summary>The lobed crown surface of one tree (v2): radius factor L(d) along a unit direction of the normalised crown.</summary>
        public sealed class CrownShape
        {
            readonly Vector3[] _dir;
            readonly float[] _cosW, _amp;
            readonly Vector3 _o;

            public CrownShape(int seed, bool hero)
            {
                int k = hero ? GardenDims.LobesHero : GardenDims.LobesBack;
                _dir = new Vector3[k]; _cosW = new float[k]; _amp = new float[k];
                _o = GardenTree.LumpOffsets(seed);
                float golden = Mathf.PI * (3f - Mathf.Sqrt(5f));
                float phase = 2f * Mathf.PI * BridgeGardenKit.Hash01(seed, 811);
                for (int i = 0; i < k; i++)
                {
                    float y = 1f - 2f * (i + 0.5f) / k;
                    float rr = Mathf.Sqrt(Mathf.Max(0f, 1f - y * y));
                    float phi = i * golden + phase;
                    var d = new Vector3(Mathf.Cos(phi) * rr, y, Mathf.Sin(phi) * rr);
                    Basis(d, out Vector3 e1, out Vector3 e2);
                    float j1 = H(seed, 7, i, 0) - 0.5f, j2 = H(seed, 7, i, 1) - 0.5f;
                    _dir[i] = (d + (e1 * j1 + e2 * j2) * (2f * GardenDims.LobeJitter)).normalized;
                    _cosW[i] = Mathf.Cos(Mathf.Lerp(GardenDims.LobeAngleMin, GardenDims.LobeAngleMax, H(seed, 7, i, 2)));
                    _amp[i] = Mathf.Lerp(GardenDims.LobeAmpMin, 1f, H(seed, 7, i, 3));
                }
            }

            /// <summary>Lobe union S(d) in [0, 1] (0 on a valley floor, 1 on the highest lobe top).</summary>
            public float Valley(Vector3 d)
            {
                float sum = 0f;
                for (int i = 0; i < _dir.Length; i++)
                {
                    float c = Vector3.Dot(d, _dir[i]);
                    if (c <= _cosW[i]) continue;
                    float t = (c - _cosW[i]) / (1f - _cosW[i]);
                    float b = _amp[i] * t * t * (3f - 2f * t);
                    float b2 = b * b;
                    sum += b2 * b2;                                            // p = 4 (GardenDims.LobeP)
                }
                return Mathf.Min(1f, Mathf.Sqrt(Mathf.Sqrt(sum)));
            }

            /// <summary>Radius factor L(d) = (floor + (1 - floor) S(d)) x (1 +- FineLump); valley returns S(d).</summary>
            public float Factor(Vector3 d, out float valley)
            {
                valley = Valley(d);
                float n = GardenTree.Noise3(d.x * 3.1f + _o.x, d.y * 3.1f + _o.y, d.z * 3.1f + _o.z);
                return (GardenDims.LobeFloor + (1f - GardenDims.LobeFloor) * valley) * (1f + GardenDims.FineLump * (2f * n - 1f));
            }

            public float Factor(Vector3 d) => Factor(d, out _);

            /// <summary>Point of the lobed surface at shell factor k along unit direction d (crown centre c, radii R).</summary>
            public Vector3 Point(Vector3 c, Vector3 R, float k, Vector3 d) => c + Vector3.Scale(d, R) * (k * Factor(d));

            /// <summary>Outward unit normal of the lobed surface at direction d (central differences over 0.01 rad).</summary>
            public Vector3 Normal(Vector3 c, Vector3 R, Vector3 d)
            {
                Basis(d, out Vector3 e1, out Vector3 e2);
                const float eps = 0.01f;
                Vector3 a = Point(c, R, 1f, (d + e1 * eps).normalized) - Point(c, R, 1f, (d - e1 * eps).normalized);
                Vector3 b = Point(c, R, 1f, (d + e2 * eps).normalized) - Point(c, R, 1f, (d - e2 * eps).normalized);
                Vector3 n = Vector3.Cross(a, b).normalized;
                Vector3 radial = Vector3.Scale(d, new Vector3(1f / R.x, 1f / R.y, 1f / R.z));
                return Vector3.Dot(n, radial) >= 0f ? n : -n;
            }
        }

        static readonly Dictionary<int, CrownShape> s_shapes = new Dictionary<int, CrownShape>(17);

        /// <summary>The lobed shape of the tree built with this seed (cached; load time only).</summary>
        public static CrownShape Shape(in GardenTreeSpec t, int seed)
        {
            if (!s_shapes.TryGetValue(seed, out var sh)) s_shapes[seed] = sh = new CrownShape(seed, t.Hero);
            return sh;
        }

        /// <summary>Unit direction of the normalised crown space for point p of tree t ((p - c) / R normalised).</summary>
        public static Vector3 NormDir(in GardenTreeSpec t, Vector3 p)
        {
            Vector3 R = t.CrownRadii, q = p - t.CrownCentre;
            return new Vector3(q.x / R.x, q.y / R.y, q.z / R.z).normalized;
        }

        public struct Stats
        {
            public int Inner, Outer, Shrunk, Dropped, Candidates;
            public int Cards => Inner + Outer;
        }

        /// <summary>Envelope of the cards of tree t: every card corner lies inside this ellipsoid (centre, radii).</summary>
        public static void Envelope(in GardenTreeSpec t, out Vector3 centre, out Vector3 radii)
        {
            centre = t.CrownCentre;
            radii = t.CrownRadii * GardenDims.EnvelopeFrac;
        }

        /// <summary>The opaque core's base radii (its lumps make it 0.86-0.92 of the crown radii).</summary>
        public static Vector3 CoreRadii(in GardenTreeSpec t) => t.CrownRadii * GardenDims.CoreFrac;

        /// <summary>Normalised ellipsoid radius of p for tree t (1 = on the crown ellipsoid (r, 0.28 h, r)).</summary>
        public static float NormRadius(in GardenTreeSpec t, Vector3 p)
        {
            Vector3 r = t.CrownRadii, q = p - t.CrownCentre;
            float x = q.x / r.x, y = q.y / r.y, z = q.z / r.z;
            return Mathf.Sqrt(x * x + y * y + z * z);
        }

        /// <summary>Face B's notch walls (z 115 / 275 for x >= 815.3): a point east of x 815.3 - 1 m stays 1 m inside them.</summary>
        public static bool ClearOfNotchWalls(Vector3 p)
        {
            if (p.x <= GardenDims.FaceBX - GardenDims.NotchClearance) return true;
            return p.z >= GardenDims.TerraceZ0 + GardenDims.NotchClearance && p.z <= GardenDims.TerraceZ1 - GardenDims.NotchClearance;
        }

        /// <summary>Surface area of the ellipsoid with semi-axes r (Thomsen's formula, p = 1.6075, error under 1.1 %).</summary>
        public static float EllipsoidArea(Vector3 r)
        {
            const float p = 1.6075f;
            float ab = Mathf.Pow(r.x * r.y, p), ac = Mathf.Pow(r.x * r.z, p), bc = Mathf.Pow(r.y * r.z, p);
            return 4f * Mathf.PI * Mathf.Pow((ab + ac + bc) / 3f, 1f / p);
        }

        /// <summary>Card density factor: 1 for the hero trees, 0.5 for the back trees.</summary>
        public static float Density(in GardenTreeSpec t) => t.Hero ? 1f : GardenDims.BackCardDensity;

        /// <summary>Wanted card count of one shell (factor k of the crown radii): 1.5 x area / 4.4 m^2 x density (x 0.7 for the
        /// inner shell, k &lt; 1).</summary>
        public static int TargetCount(in GardenTreeSpec t, float k)
            => Mathf.RoundToInt(GardenDims.CardCoverage * Density(t) * (k < 1f ? GardenDims.InnerShellDensity : 1f)
                                * EllipsoidArea(t.CrownRadii * k) / GardenDims.CardMeanArea);

        /// <summary>Contrast stretch of the clump noise (r03: 2.5 saturated large areas at -1, so whole lobes went bald and showed
        /// the core as a smooth dome; 1.8 keeps clusters and gaps without bald lobes).</summary>
        public const float ClumpStretch = 1.8f;

        /// <summary>Clump factor in [-1, 1] along unit direction d (contrast-stretched low-frequency value noise).</summary>
        public static float Clump(Vector3 d, Vector3 o, int shell)
        {
            float f = GardenDims.ClumpFreq;
            float n = GardenTree.Noise3(d.x * f + o.y + 5.3f * shell, d.y * f + o.z, d.z * f + o.x);
            return Mathf.Clamp(ClumpStretch * (2f * n - 1f), -1f, 1f);
        }

        static readonly Vector3[] s_corner = new Vector3[4];

        /// <summary>Builds the cards of tree t into mb (outer shell, then inner) and their tangents into tangents.</summary>
        public static Stats Build(in GardenTreeSpec t, int seed, MeshBuilder mb, List<Vector4> tangents)
        {
            var st = new Stats();
            Vector3 o = GardenTree.LumpOffsets(seed);
            bool uv1 = mb.WriteUV1;
            mb.WriteUV1 = true;
            Shell(t, seed, 1, GardenDims.ShellOuter, mb, tangents, o, ref st);
            Shell(t, seed, 0, GardenDims.ShellInner, mb, tangents, o, ref st);
            mb.WriteUV1 = uv1;
            return st;
        }

        static void Shell(in GardenTreeSpec t, int seed, int shell, float k, MeshBuilder mb, List<Vector4> tangents, Vector3 o, ref Stats st)
        {
            Vector3 R = t.CrownRadii, c = t.CrownCentre;
            Vector3 invR = new Vector3(1f / R.x, 1f / R.y, 1f / R.z);
            float rMin = Mathf.Min(R.x, Mathf.Min(R.y, R.z));
            int target = TargetCount(t, k);
            if (target <= 0) return;
            var shape = Shape(t, seed);
            float golden = Mathf.PI * (3f - Mathf.Sqrt(5f));
            float phase = 2f * Mathf.PI * BridgeGardenKit.Hash01(seed, 700 + shell);
            int stream = 1 + shell;

            // acceptance = ellipsoid area element x L^2 (the lobed surface's area per solid angle) x clumps x valley thinning;
            // its mean over a 4 096-point Fibonacci set fixes n0 so the kept count meets the target
            const int probe = 4096;
            double acc = 0.0;
            for (int i = 0; i < probe; i++)
            {
                float y = 1f - 2f * (i + 0.5f) / probe;
                float rr = Mathf.Sqrt(Mathf.Max(0f, 1f - y * y));
                float phi = i * golden;
                var d = new Vector3(Mathf.Cos(phi) * rr, y, Mathf.Sin(phi) * rr);
                acc += Accept(d, invR, rMin, shape, o, shell, out _, out _);
            }
            float mean = Mathf.Max((float)(acc / probe), 1e-3f);
            int n0 = Mathf.CeilToInt(target / mean);
            float spacing = Mathf.Sqrt(4f * Mathf.PI / n0);

            // bottom first (i = n0 - 1 is the lowest lattice point): every view of the walk is from below, so the nearer cards
            // write their covered samples first and early-Z rejects more of the cards above them (perf A/B r04: p95 +1.36 ms)
            for (int i = n0 - 1; i >= 0; i--)
            {
                float y = 1f - 2f * (i + 0.5f) / n0;
                float rr = Mathf.Sqrt(Mathf.Max(0f, 1f - y * y));
                float phi = i * golden + phase;
                var d = new Vector3(Mathf.Cos(phi) * rr, y, Mathf.Sin(phi) * rr);
                Basis(d, out Vector3 e1, out Vector3 e2);
                float j1 = H(seed, stream, i, 0) - 0.5f, j2 = H(seed, stream, i, 1) - 0.5f;
                d = (d + (e1 * j1 + e2 * j2) * (2f * GardenDims.CardJitter * spacing)).normalized;

                float pAcc = Accept(d, invR, rMin, shape, o, shell, out float lobe, out float valley);
                if (H(seed, stream, i, 2) >= pAcc) continue;
                st.Candidates++;

                float kk = k * lobe + RadialJitter * (2f * H(seed, stream, i, 3) - 1f);
                Vector3 p = c + Vector3.Scale(d, R) * kk;
                Vector3 ns = shape.Normal(c, R, d);                          // lobed surface normal at the card centre
                Vector3 ne = Vector3.Scale(p - c, Vector3.Scale(invR, invR)).normalized;

                // card normal: ns tilted by up to 50 deg (sqrt: area-uniform inside the cone) about a random axis
                float tilt = GardenDims.CardTiltMaxDeg * Mathf.Sqrt(H(seed, stream, i, 4));
                float az = 2f * Mathf.PI * H(seed, stream, i, 5);
                Basis(ns, out Vector3 t1, out Vector3 t2);
                Vector3 axis = Mathf.Cos(az) * t1 + Mathf.Sin(az) * t2;
                Vector3 nc = (Quaternion.AngleAxis(tilt, axis) * ns).normalized;
                Basis(nc, out Vector3 b1, out Vector3 b2);
                float rot = 2f * Mathf.PI * H(seed, stream, i, 6);
                Vector3 a1 = Mathf.Cos(rot) * b1 + Mathf.Sin(rot) * b2;      // card u
                Vector3 a2 = Vector3.Cross(nc, a1);                           // card v (= bitangent cross(n, t))

                float size = Mathf.Lerp(GardenDims.CardMin, GardenDims.CardMax, H(seed, stream, i, 7));
                if (!Fits(t, p, a1, a2, 0.5f * size))
                {
                    size = GardenDims.CardMinShrunk;
                    if (!Fits(t, p, a1, a2, 0.5f * size)) { st.Dropped++; continue; }
                    st.Shrunk++;
                }

                int slice = Mathf.Min(Sprites - 1, (int)(H(seed, stream, i, 8) * Sprites));
                float hue = H(seed, stream, i, 9);
                bool glow = H(seed, stream, i, 10) < GlowCardFraction;
                float under = Mathf.Lerp(UndersideAo, 1f, 0.5f + 0.5f * d.y);
                float ao = (shell == 0 ? InnerShellAo : 1f) * under * Mathf.Lerp(GardenDims.ValleyAo, 1f, valley);
                // sky exposure: the lid's share above the card; an outer-shell card on the hull still sees about a quarter of the
                // sky past the crown's side (OuterExposureMin), an inner one only through the outer layer
                float lid = shell == 0 ? InnerShellExposure * Mathf.Clamp01(0.5f + 0.5f * d.y)
                                       : Mathf.Lerp(OuterExposureMin, 1f, Mathf.Clamp01(0.5f + 0.5f * d.y));
                float exposure = lid * Mathf.Lerp(GardenDims.ValleyExposure, 1f, valley);
                mb.Color = new Color32((byte)Mathf.RoundToInt(255f * ao), (byte)(glow ? 255 : 0), (byte)Mathf.RoundToInt(255f * exposure), 255);
                Corners(p, a1, a2, 0.5f * size);
                Vector2 oct = OctEncode(nc);
                int first = mb.VertexCount;
                for (int v = 0; v < 4; v++)
                {
                    Vector3 q = s_corner[v];
                    // bent-normal target: the lobed normal at the centre plus the ellipsoid's turn from the centre to the corner
                    Vector3 nq = Vector3.Scale(q - c, Vector3.Scale(invR, invR)).normalized;
                    Vector3 nv = (ns + (nq - ne)).normalized;
                    float cv = Vector3.Dot(nv, nc);                               // keep it within 54 deg of the card normal
                    if (cv < CosMaxBend) nv = Vector3.Slerp(nc, nv, Mathf.Acos(CosMaxBend) / Mathf.Acos(Mathf.Clamp(cv, -1f, 1f)));
                    mb.CurrentUV1 = new Vector4(slice, hue, oct.x, oct.y);
                    mb.Vertex(q, nv, CornerUv(v));
                    tangents.Add(new Vector4(a1.x, a1.y, a1.z, 1f));
                }
                // corners 0 (-u -v), 1 (+u -v), 2 (+u +v), 3 (-u +v): cross(c1 - c0, c2 - c0) = cross(a1, a1 + a2) = nc (front)
                mb.Tri(first, first + 1, first + 2);
                mb.Tri(first, first + 2, first + 3);
                if (shell == 0) st.Inner++; else st.Outer++;
            }
            mb.Color = new Color32(255, 0, 0, 255);
        }

        /// <summary>Card acceptance along unit direction d: area element x L^2 x clumps x valley thinning (each in [0, 1]).</summary>
        static float Accept(Vector3 d, Vector3 invR, float rMin, CrownShape shape, Vector3 o, int shell, out float lobe, out float valley)
        {
            Vector3 g = Vector3.Scale(d, invR);
            float pArea = g.magnitude * rMin;
            lobe = shape.Factor(d, out valley);
            float pClump = (1f + GardenDims.ClumpDepth * Clump(d, o, shell)) / (1f + GardenDims.ClumpDepth);
            float pValley = Mathf.Lerp(GardenDims.ValleyDensity, 1f, valley);
            float l2 = Mathf.Min(lobe * lobe, 1f);
            return pArea * l2 * pClump * pValley;
        }

        static Vector2 CornerUv(int v) => v == 0 ? new Vector2(0f, 0f) : v == 1 ? new Vector2(1f, 0f) : v == 2 ? new Vector2(1f, 1f) : new Vector2(0f, 1f);

        static void Corners(Vector3 p, Vector3 a1, Vector3 a2, float hs)
        {
            s_corner[0] = p - a1 * hs - a2 * hs;
            s_corner[1] = p + a1 * hs - a2 * hs;
            s_corner[2] = p + a1 * hs + a2 * hs;
            s_corner[3] = p - a1 * hs + a2 * hs;
        }

        /// <summary>True when all four corners of the card lie inside the envelope and clear of the notch walls.</summary>
        static bool Fits(in GardenTreeSpec t, Vector3 p, Vector3 a1, Vector3 a2, float hs)
        {
            Corners(p, a1, a2, hs);
            for (int v = 0; v < 4; v++)
            {
                if (NormRadius(t, s_corner[v]) > GardenDims.EnvelopeFrac) return false;
                if (!ClearOfNotchWalls(s_corner[v])) return false;
            }
            return true;
        }

        /// <summary>Hash in [0, 1) of (tree seed, shell stream, card, channel).</summary>
        static float H(int seed, int stream, int i, int ch)
            => (float)NocMath.U01(NocMath.Pcg3(seed * 8 + stream, i, 9001 + ch));

        /// <summary>Orthonormal pair (t, b) perpendicular to the unit vector n.</summary>
        static void Basis(Vector3 n, out Vector3 t, out Vector3 b)
        {
            Vector3 refAxis = Mathf.Abs(n.y) < 0.999f ? Vector3.up : Vector3.right;
            t = Vector3.Cross(refAxis, n).normalized;
            b = Vector3.Cross(n, t);
        }

        /// <summary>Octahedral encoding of a unit vector into [-1, 1]^2 (twin of NocFoliageOctDecode in NocFoliage.shader).</summary>
        public static Vector2 OctEncode(Vector3 n)
        {
            float l1 = Mathf.Abs(n.x) + Mathf.Abs(n.y) + Mathf.Abs(n.z);
            float x = n.x / l1, y = n.y / l1, z = n.z / l1;
            if (z < 0f)
            {
                float ox = (1f - Mathf.Abs(y)) * (x >= 0f ? 1f : -1f);
                float oy = (1f - Mathf.Abs(x)) * (y >= 0f ? 1f : -1f);
                x = ox; y = oy;
            }
            return new Vector2(x, y);
        }

        /// <summary>Inverse of OctEncode.</summary>
        public static Vector3 OctDecode(Vector2 e)
        {
            var n = new Vector3(e.x, e.y, 1f - Mathf.Abs(e.x) - Mathf.Abs(e.y));
            float tt = Mathf.Clamp01(-n.z);
            n.x += n.x >= 0f ? -tt : tt;
            n.y += n.y >= 0f ? -tt : tt;
            return n.normalized;
        }
    }
}
