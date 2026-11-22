// GardenLanterns (GD, Phase 4): the lanterns, the lantern posts and the fireflies of beat 5 (MASTER_PROMPT 14.5.5; LOOK_V2 6
// beat 5 and 8.1 KS14 / KS15), and the lantern field that lights the crown undersides (critique INT-04).
//   Tree lanterns: 520 per tree (8 840 for the 17 trees), 0.35 m icosahedra (12 vertices, 20 triangles) baked into one static
//      mesh per tree, drawn by NocFoliage _LeafMode 3 (emission Lantern x 2.0, 0.1 Hz +-6 % pulse on the scene clock, >= 2.2 px
//      with the energy kept: the vertex hook grows the ball about its centre and scales the emission by (r / r')^2). Every
//      vertex carries the ball radius and the lantern hash in uv0 (the centre is position - normal x radius). They hang in
//      strands of 2-4 (mean 3, about 173 strands a tree) from anchors among the leaves of the lower half of the lobed crown
//      (GardenLeafCards.CrownShape; anchor at 1.04-1.08 of the lobed radius, just under the outer card shell at 1.02), one step
//      and then 0.5-0.8 m apart down an invisible cord with +-0.12 m of sideways jitter, all under the leaves (GD r04: lanterns
//      among the leaves showed through 1-px gaps of the alpha-to-coverage cards as single orange pixels, image_metrics NaN-like
//      19-28 per still: sparkle once the leaves sway). GD lookdev r01: 520 independent points per crown read as an even scatter of fairy lights and
//      their field was too even to light the leaves locally; r02's strands of 3-7 lit warm pools on the leaves but, seen up
//      50 deg, their vertical dotted lines converged to the zenith like rain streaks; r03's 1-4 hid a third of the lanterns among
//      the leaves (KS15 19 blobs, 14.5.5 asks >= 25); 2-4 hung a step below the anchor keeps the pools without the streaks. The KS15 crowns T3 / T7 hang 33 m up, 37-42 m from the eye: a ball is 8-9 px there, a strand step 14-20 px.
//   Lantern field: every crown-core and card vertex gets in colour A the irradiance of the lanterns around it,
//      E = sum (0.25 + 0.75 max(0, n.l)) / (d^2 + d0^2) x (1 - (d / dc)^2)^2 (d0 1.5 m: a ball and its glow are not a point;
//      dc 8 m), A = E / 4 in bytes. NocFoliage turns it into the lantern light scattered through the canopy (the light of
//      lerp(Lantern, WinNeutral, 0.4), tinted by the leaf colour raised to _ScatterPow: warm near a lantern, greener deeper).
//   Lantern posts: 10 dark steel posts 3.3 m tall on the planter line (6.9 m off the centre line, 2.6 m past a planter's centre,
//      alternating sides: the lane is 6.0 + 0.38 m, so nothing below 2.2 m may come nearer), a 0.5 m crook arm toward the path
//      and a cluster of 3 lanterns (smooth spheres: the posts are seen from 2-20 m, where an icosahedron's outline read as a
//      hexagon, r01) hanging at 2.47-2.87 m (bottoms >= 2.29 m, over the lane edge), each cluster with one real light
//      lerp(Lantern, WinNeutral, 0.4) r 14 i 3.5 (LOOK_V2 6: the 10 lantern clusters): warm pools on the moss verge, the hedges,
//      the benches and the path edge. (GD-t01: r02's posts at 4.6 m stood inside the 6 m lane.)
//   Fireflies: 600 motes (r 1.2 cm, _LeafMode 4) over the moss on both sides of the path, 0.4-3.5 m up and 8.3-26 m off the
//      centre line (outside the 6.0 + 0.38 m lane by more than their 1.8 m reach); the vertex hook moves each on a slow wander
//      (1.5 m, 0.05 m/s) plus a 0.3 m orbit; >= 2.2 px, energy kept,
//      faded out nearer than 1.5-4 m (no blob at the lens).
// Load-time only: nothing here runs per frame. All placement is hashed from the tree seed (deterministic).
using System.Collections.Generic;
using UnityEngine;

namespace Megastructure
{
    public static class GardenLanterns
    {
        /// <summary>Test hook (FoliageVisibilityTests): false builds the garden without its lanterns, posts, fireflies and field.</summary>
        public static bool Enabled = true;

        // ---- tree lanterns (14.5.5: 520 instanced 0.35 m icosahedra per tree, 8 840 in all)
        public const int PerTree = 520;
        public const float Radius = 0.175f;
        public const int StrandMin = 2, StrandMax = 4;                          // lanterns per strand (mean 3; r02 had 3-7, r03 1-4)
        public const float AnchorK0 = 1.04f, AnchorK1 = 1.08f, StepMin = 0.5f, StepMax = 0.8f, StrandJitter = 0.12f;
        public const float DropMin = 1f, DropMax = 6f;                           // the lowest lantern hangs at most DropMax below its anchor
        public const float HemisphereTop = -0.08f;                              // lanterns only under the crown's equator (d.y)
        public const float LowestY = 8f;

        // ---- lantern field (vertex colour A of the crown cores and cards)
        public const float FieldSoft = 1.0f, FieldCut = 8f, FieldScale = 4f, FieldWrap = 0.25f, FieldCell = 4f;

        // ---- lantern posts (LOOK_V2 6 beat 5: 10 lantern clusters r 14 i 3.5)
        public const int PostCount = 10, PostLanterns = 3;
        public const float PostTop = 3.3f, PostRadius = 0.045f, ArmLength = 0.5f, ArmRadius = 0.03f, HangerRadius = 0.012f;
        public const float PostLightRange = 14f, PostLightIntensity = 3.5f, PostLightY = 2.67f;
        public const float HangTop = 0.35f, HangStep = 0.2f;                    // lantern drops below the arm (2.47-2.87 m: bottoms >= 2.29)
        public const float PostAlong = 2.6f;                                    // between the planter's end and the next bench
        public const float PostLateral = 6.9f;                                  // on the planter line, between planter and bench (lane 6.0 + 0.38)
        public const int PostLanternRings = 8, PostLanternSegments = 12;        // smooth spheres (UV sphere)

        // ---- fireflies (14.5.5: 600 NocStreak-like motes, min 2.2 px, drift 0.05 m/s, orbits 0.3 m)
        public const int FireflyCount = 600;
        public const float FireflyRadius = 0.012f, FireflyLatMin = 8.3f, FireflyLatMax = 26f, FireflyYMin = 0.4f, FireflyYMax = 3.5f;
        public const double FireflyS0 = 884.0, FireflyS1 = 1074.0;

        /// <summary>The light every lantern casts (LOOK_V2 R4): lerp(Lantern, WinNeutral, 0.4), linear.</summary>
        public static Color LightColour()
        {
            Color c = Color.Lerp(NocturnePalette.Linear(NocPal.Lantern), NocturnePalette.Linear(NocPal.WinNeutral), 0.4f);
            c.a = 1f;
            return c;
        }

        static float H(int seed, int i, int k) => (float)NocMath.U01(NocMath.Pcg3(seed * 7919 + 17, i, 9100 + k));

        /// <summary>The 520 lanterns of tree t (xyz centre, w hash in [0, 1)) in strands, deterministic from the seed.</summary>
        public static void Place(in GardenTreeSpec t, int seed, List<Vector4> outL)
        {
            outL.Clear();
            var shape = GardenLeafCards.Shape(t, seed);
            Vector3 c = t.CrownCentre, R = t.CrownRadii;
            for (int i = 0; outL.Count < PerTree && i < PerTree * 8; i++)
            {
                float y = -1f + (HemisphereTop + 1f) * H(seed, i, 0);
                float rr = Mathf.Sqrt(Mathf.Max(0f, 1f - y * y));
                float ph = 2f * Mathf.PI * H(seed, i, 1);
                var d = new Vector3(Mathf.Cos(ph) * rr, y, Mathf.Sin(ph) * rr);
                float L = shape.Factor(d);
                Vector3 a = c + Vector3.Scale(d, R) * (L * Mathf.Lerp(AnchorK0, AnchorK1, H(seed, i, 2)));
                int n = StrandMin + Mathf.Min(StrandMax - StrandMin, (int)(H(seed, i, 3) * (StrandMax - StrandMin + 1)));
                float step = Mathf.Lerp(StepMin, StepMax, H(seed, i, 4));
                float drop = 0f;
                for (int j = 0; j < n && outL.Count < PerTree; j++)
                {
                    drop += step;
                    if (drop > DropMax) break;
                    var p = new Vector3(a.x + StrandJitter * (2f * H(seed, i, 10 + 2 * j) - 1f), a.y - drop, a.z + StrandJitter * (2f * H(seed, i, 11 + 2 * j) - 1f));
                    if (p.y < LowestY || !GardenLeafCards.ClearOfNotchWalls(p)) continue;
                    outL.Add(new Vector4(p.x, p.y, p.z, H(seed, i, 30 + j)));
                }
            }
        }

        // ---- icosahedron (unit circumradius)
        static readonly Vector3[] s_ico = MakeIco();
        static readonly int[] s_icoTri =
        {
            0, 11, 5, 0, 5, 1, 0, 1, 7, 0, 7, 10, 0, 10, 11, 1, 5, 9, 5, 11, 4, 11, 10, 2, 10, 7, 6, 7, 1, 8,
            3, 9, 4, 3, 4, 2, 3, 2, 6, 3, 6, 8, 3, 8, 9, 4, 9, 5, 2, 4, 11, 6, 2, 10, 8, 6, 7, 9, 8, 1,
        };

        static Vector3[] MakeIco()
        {
            float f = 0.5f * (1f + Mathf.Sqrt(5f));
            var v = new[]
            {
                new Vector3(-1, f, 0), new Vector3(1, f, 0), new Vector3(-1, -f, 0), new Vector3(1, -f, 0),
                new Vector3(0, -1, f), new Vector3(0, 1, f), new Vector3(0, -1, -f), new Vector3(0, 1, -f),
                new Vector3(f, 0, -1), new Vector3(f, 0, 1), new Vector3(-f, 0, -1), new Vector3(-f, 0, 1),
            };
            for (int i = 0; i < v.Length; i++) v[i] = v[i].normalized;
            return v;
        }

        public const int BallVertices = 12, BallTriangles = 20;
        public static int SphereVertices => (PostLanternRings - 1) * (PostLanternSegments + 1) + 2;
        public static int SphereTriangles => 2 * PostLanternSegments * (PostLanternRings - 1);

        /// <summary>A smooth ball (UV sphere, radial normals, uv0 = (radius, hash), outward winding) for the near post lanterns.</summary>
        public static void Sphere(MeshBuilder mb, Vector3 c, float radius, float hash)
        {
            int first = mb.VertexCount, segs = PostLanternSegments, rings = PostLanternRings;
            mb.Vertex(c + Vector3.down * radius, Vector3.down, new Vector2(radius, hash));
            for (int i = 1; i < rings; i++)
            {
                float th = Mathf.PI * i / rings;
                for (int j = 0; j <= segs; j++)
                {
                    float ph = 2f * Mathf.PI * j / segs;
                    var d = new Vector3(Mathf.Sin(th) * Mathf.Cos(ph), -Mathf.Cos(th), Mathf.Sin(th) * Mathf.Sin(ph));
                    mb.Vertex(c + d * radius, d, new Vector2(radius, hash));
                }
            }
            mb.Vertex(c + Vector3.up * radius, Vector3.up, new Vector2(radius, hash));
            int top = mb.VertexCount - 1;
            for (int i = 0; i < rings - 1; i++)
                for (int j = 0; j < segs; j++)
                {
                    int a = first + 1 + i * (segs + 1) + j;
                    if (i == 0) SphereTri(mb, c, first, a + 1, a);
                    if (i == rings - 2) SphereTri(mb, c, top, a, a + 1);
                    else
                    {
                        SphereTri(mb, c, a, a + 1, a + segs + 2);
                        SphereTri(mb, c, a, a + segs + 2, a + segs + 1);
                    }
                }
        }

        static void SphereTri(MeshBuilder mb, Vector3 c, int a, int b, int d)
        {
            Vector3 n = Vector3.Cross(mb.Positions[b] - mb.Positions[a], mb.Positions[d] - mb.Positions[a]);
            if (n.sqrMagnitude < 1e-14f) return;
            if (Vector3.Dot(n, mb.Positions[a] + mb.Positions[b] + mb.Positions[d] - 3f * c) >= 0f) mb.Tri(a, b, d);
            else mb.Tri(a, d, b);
        }

        /// <summary>One ball (12 shared vertices, radial normals, uv0 = (radius, hash), outward winding) at centre c.</summary>
        public static void Ball(MeshBuilder mb, Vector3 c, float radius, float hash)
        {
            int first = mb.VertexCount;
            for (int v = 0; v < BallVertices; v++) mb.Vertex(c + s_ico[v] * radius, s_ico[v], new Vector2(radius, hash));
            for (int k = 0; k < s_icoTri.Length; k += 3)
            {
                int a = s_icoTri[k], b = s_icoTri[k + 1], d = s_icoTri[k + 2];
                Vector3 n = Vector3.Cross(s_ico[b] - s_ico[a], s_ico[d] - s_ico[a]);
                if (Vector3.Dot(n, s_ico[a] + s_ico[b] + s_ico[d]) >= 0f) mb.Tri(first + a, first + b, first + d);
                else mb.Tri(first + a, first + d, first + b);
            }
        }

        /// <summary>The lanterns of one list as balls; colour = a per-lantern warmth (+-4 % in G, B) in RGB.</summary>
        public static void Balls(MeshBuilder mb, List<Vector4> lanterns, float radius) => Balls(mb, lanterns, radius, Vector3.zero, Vector3.zero);

        /// <summary>As Balls, with the lanterns' crown (GD fix wave C32): uv1 = (centre x, z, y, horizontal radius), colour A =
        /// vertical / (2 x horizontal) radius; NocFoliage fades a lantern whose eye ray passes deeper through the crown's leaf fringe
        /// than the lantern hangs (the 1-px peeks through the card gaps). crownRadii zero: no crown (uv1 not written).</summary>
        public static void Balls(MeshBuilder mb, List<Vector4> lanterns, float radius, Vector3 crownCentre, Vector3 crownRadii)
        {
            Color32 keep = mb.Color;
            bool keepUv1 = mb.WriteUV1;
            Vector4 keepCur = mb.CurrentUV1;
            float rh = 0.5f * (crownRadii.x + crownRadii.z);
            bool crown = rh > 2.5f;
            byte alpha = 255;
            if (crown)
            {
                mb.WriteUV1 = true;
                mb.CurrentUV1 = new Vector4(crownCentre.x, crownCentre.z, crownCentre.y, rh);
                alpha = (byte)Mathf.Clamp(Mathf.RoundToInt(255f * crownRadii.y / (2f * rh)), 1, 255);
            }
            foreach (var l in lanterns)
            {
                float w = 1f + 0.08f * (Frac(l.w * 13.17f) - 0.5f);
                mb.Color = new Color32(255, (byte)Mathf.RoundToInt(Mathf.Clamp01(0.96f * w) * 255f), (byte)Mathf.RoundToInt(Mathf.Clamp01(0.92f * w) * 255f), alpha);
                Ball(mb, new Vector3(l.x, l.y, l.z), radius, l.w);
            }
            mb.Color = keep;
            mb.WriteUV1 = keepUv1;
            mb.CurrentUV1 = keepCur;
        }

        static float Frac(float x) => x - Mathf.Floor(x);

        // ---- the crowns as shader globals (GD fix wave C32: a tree lantern behind another crown's leaf fringe fades out)
        public static readonly int CrownsId = Shader.PropertyToID("_NocGardenCrowns"), CrownCountId = Shader.PropertyToID("_NocGardenCrownCount");
        public const int MaxCrowns = 17;
        static readonly Vector4[] s_crowns = new Vector4[2 * MaxCrowns];

        /// <summary>Writes the crowns (even = centre, odd = radii; at most 17) into _NocGardenCrowns / _NocGardenCrownCount. Load time only.</summary>
        public static int SetCrownGlobals(GardenTreeSpec[] trees)
        {
            System.Array.Clear(s_crowns, 0, s_crowns.Length);
            int n = Mathf.Min(trees != null ? trees.Length : 0, MaxCrowns);
            for (int i = 0; i < n; i++)
            {
                Vector3 c = trees[i].CrownCentre, r = trees[i].CrownRadii;
                s_crowns[2 * i] = new Vector4(c.x, c.y, c.z, 0f);
                s_crowns[2 * i + 1] = new Vector4(r.x, r.y, r.z, 0f);
            }
            Shader.SetGlobalVectorArray(CrownsId, s_crowns);
            Shader.SetGlobalFloat(CrownCountId, Enabled ? n : 0);
            return n;
        }

        // ---- the lantern field

        /// <summary>A 2D (xz) bucket grid over a set of lanterns, for the field sums.</summary>
        public sealed class FieldGrid
        {
            readonly Dictionary<long, List<Vector3>> _cells = new Dictionary<long, List<Vector3>>(4096);
            public int Count { get; private set; }

            static long Key(int ix, int iz) => ((long)ix << 32) ^ (uint)iz;

            public void Add(Vector3 p)
            {
                long k = Key(Mathf.FloorToInt(p.x / FieldCell), Mathf.FloorToInt(p.z / FieldCell));
                if (!_cells.TryGetValue(k, out var list)) { list = new List<Vector3>(16); _cells[k] = list; }
                list.Add(p);
                Count++;
            }

            /// <summary>Irradiance of the lantern field at p on a surface of normal n (unit lantern power, see the header).</summary>
            public float At(Vector3 p, Vector3 n)
            {
                int r = Mathf.CeilToInt(FieldCut / FieldCell);
                int cx = Mathf.FloorToInt(p.x / FieldCell), cz = Mathf.FloorToInt(p.z / FieldCell);
                float cut2 = FieldCut * FieldCut, soft2 = FieldSoft * FieldSoft, e = 0f;
                for (int ix = cx - r; ix <= cx + r; ix++)
                    for (int iz = cz - r; iz <= cz + r; iz++)
                    {
                        if (!_cells.TryGetValue(Key(ix, iz), out var list)) continue;
                        for (int i = 0; i < list.Count; i++)
                        {
                            Vector3 l = list[i] - p;
                            float d2 = l.sqrMagnitude;
                            if (d2 >= cut2) continue;
                            float inv = 1f / Mathf.Sqrt(Mathf.Max(d2, 1e-6f));
                            float cosT = (l.x * n.x + l.y * n.y + l.z * n.z) * inv;
                            float w = 1f - d2 / cut2;
                            e += (FieldWrap + (1f - FieldWrap) * Mathf.Max(0f, cosT)) / (d2 + soft2) * (w * w);
                        }
                    }
                return e;
            }
        }

        /// <summary>Writes the lantern field into colour A of the vertices [first, end) of mb (A = E / FieldScale).</summary>
        public static void ApplyField(FieldGrid grid, MeshBuilder mb, int first, int end)
        {
            for (int v = first; v < end; v++)
            {
                float e = grid.At(mb.Positions[v], mb.Normals[v]);
                Color32 c = mb.Colors[v];
                c.a = (byte)Mathf.RoundToInt(255f * Mathf.Clamp01(e / FieldScale));
                mb.Colors[v] = c;
            }
        }

        // ---- lantern posts

        /// <summary>Planter index k and side of post i (every planter k = 1..10 along the straight path, alternating sides).</summary>
        public static void PostSlot(int i, out double s, out int side)
        {
            s = GardenDims.PlanterS0 + GardenDims.PlanterPitch * (i + 1) + PostAlong;
            side = (i & 1) == 0 ? 1 : -1;
        }

        /// <summary>The post geometry (steel), the 3 lanterns of each cluster (balls) and the post lights.</summary>
        public static void BuildPosts(Transform lightParent, MeshBuilder steel, MeshBuilder balls, List<Light> lights)
        {
            Color lc = LightColour();
            for (int i = 0; i < PostCount; i++)
            {
                PostSlot(i, out double s, out int side);
                float lat = side * PostLateral;
                Vector3 foot = NocLanes.CorridorToWorld(s, lat, 0.0);
                Vector3 toAxis = (NocLanes.CorridorToWorld(s, 0.0, 0.0) - foot);
                toAxis.y = 0f;
                toAxis.Normalize();
                Vector3 b0 = foot + new Vector3(0f, -0.05f, 0f), b1 = foot + new Vector3(0f, PostTop, 0f);
                MeshKit.TaperedCylinder(steel, b0, b1, PostRadius, 0.8f * PostRadius, 8, true);
                Vector3 a0 = b1 - new Vector3(0f, 0.08f, 0f) - toAxis * 0.02f, a1 = a0 + toAxis * ArmLength;
                MeshKit.TaperedCylinder(steel, a0, a1, ArmRadius, 0.8f * ArmRadius, 8, true);
                Color32 keep = balls.Color;
                balls.Color = new Color32(255, 245, 235, 255);
                Vector3 sum = Vector3.zero;
                for (int k = 0; k < PostLanterns; k++)
                {
                    float hash = H(4100 + i, k, 0);
                    Vector3 along = Vector3.Cross(Vector3.up, toAxis);
                    Vector3 c = a1 - toAxis * 0.04f + along * (0.22f * (k - 1)) + new Vector3(0f, -(HangTop + HangStep * ((k + i) % 3)), 0f);
                    MeshKit.TaperedCylinder(steel, new Vector3(c.x, a1.y - ArmRadius, c.z), c + new Vector3(0f, Radius * 0.9f, 0f), HangerRadius, HangerRadius, 4, false);
                    Sphere(balls, c, Radius, hash);
                    sum += c;
                }
                balls.Color = keep;
                Vector3 lp = sum / PostLanterns;
                var light = lightParent != null ? BridgeGardenKit.PointLight(lightParent, "lantern_post_" + i, lp, lc, PostLightRange, PostLightIntensity) : null;
                if (lights != null && light != null) lights.Add(light);
            }
        }

        // ---- fireflies

        /// <summary>The 600 firefly motes (balls of FireflyRadius; the vertex hook moves them).</summary>
        public static int BuildFireflies(MeshBuilder mb)
        {
            int n = 0;
            Color32 keep = mb.Color;
            mb.Color = new Color32(255, 255, 255, 255);
            var trees = GardenDims.Trees;
            for (int i = 0; n < FireflyCount && i < FireflyCount * 8; i++)
            {
                double s = FireflyS0 + (FireflyS1 - FireflyS0) * H(77, i, 0);
                float side = H(77, i, 1) < 0.5f ? -1f : 1f;
                float lat = side * Mathf.Lerp(FireflyLatMin, FireflyLatMax, H(77, i, 2) * H(77, i, 2));    // denser near the path
                float y = Mathf.Lerp(FireflyYMin, FireflyYMax, H(77, i, 3) * H(77, i, 3));               // denser near the moss
                Vector3 p = NocLanes.CorridorToWorld(s, lat, y);
                if (p.x < GardenDims.TerraceX0 + 3f || p.x > GardenDims.TerraceX1 - 3f || p.z < GardenDims.TerraceZ0 + 3f || p.z > GardenDims.TerraceZ1 - 3f) continue;
                bool clear = true;
                foreach (var t in trees)
                {
                    float dx = p.x - t.X, dz = p.z - t.Z, rr = GardenTree.FootprintRadius(t.R0) + 2.5f;
                    if (dx * dx + dz * dz < rr * rr) { clear = false; break; }
                }
                if (!clear) continue;
                Ball(mb, p, FireflyRadius, H(77, i, 4));
                n++;
            }
            mb.Color = keep;
            return n;
        }
    }
}
