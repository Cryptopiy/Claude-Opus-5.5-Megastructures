// MedianPromenade: the street furniture of the boulevard (MASTER_PROMPT 14.5.2 median promenade; 14.4 lanes; BoulevardLayout):
//   * the visible barriers that back the invisible lane walls (14.4 "backed by visible barriers"): 0.9 m bollards (r 0.1) along both
//     lane edges of the crossing and of C1 (s 120.6-208.7), centres at lane + 0.51 m (surface 3 cm outside the collider face at
//     lane + 0.38), about every 2.9 m of edge (about 60, 14.5.2), offset along the edge curve's own normal (the mouth funnel),
//     a 2.2 m gap where the south tram track crosses; the 0.9 m median edge barriers (the median's outer 0.3 m, |dz| 11.70..12.00:
//     median 24.0 m) from x -135 to 213.8 (the lane widens onto the tongue from s 396), the south one interrupted where the C1
//     lane crosses the median edge;
//   * 40 sodium lamp posts 12 m every 14 m in the barrier lines (NocLine rods 0.2 m, emissive heads SodiumAmber x 1.8 on 1.6 m
//     arms over the median, above the camera), taken from x 208 westward on both rows, skipping the C1 lane (>= 0.6 m outside
//     the lanes) and, on the north row, the tram sight sector x 79-175 (14.5.2); heads with x >= -20 are wet-floor sources (the
//     far-west ones are seen only when looking back, and all of them would project to s ~171), the 16 easternmost carry
//     point lights r 14 i 4 (14.2.3: with the 12 blades, 28 lights in beat 2);
//   * 3 kiosks 3 x 3 x 2.6 m, 6 benches 0.45 m and planters 1.0 m on the median west of C1 (outside every lane), 6 benches in
//     the north arcade, planters on the road side of the median barriers (none in the sight sector).
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Megastructure
{
    public static class MedianPromenade
    {
        public const double BollardS0 = 120.6, BollardS1 = 208.7, BollardStep = 0.05;
        public const double BollardLateral = 0.35 + 0.06 + 0.1;      // lane + radius + gap + bollard radius
        public const double LampLateral = 11.85, LampClear = 0.6, LampWetX0 = -20.0;
        public const int LampLights = 16;
        /// <summary>LOOK_V2 6 beat 2: 24 median lamps LampWarm (3 200 K LED) r 14 i 3.5, head radiance LampWarm x 1.6.</summary>
        public const float LampLightRange = 14f, LampLightIntensity = 3.5f, LampRadiance = 1.6f, LampRho = 0.45f;

        /// <summary>The bollard positions (world xz, y 0) along both lane edges (deterministic).</summary>
        public static List<Vector3> BollardPositions()
        {
            var list = new List<Vector3>(80);
            for (int side = -1; side <= 1; side += 2)
            {
                double acc = BoulevardLayout.BollardSpacing;   // place one at the start
                Vector3 prev = Vector3.zero;
                bool first = true;
                for (double s = BollardS0; s <= BollardS1 + 1e-9; s += BollardStep)
                {
                    Vector3 p = EdgeOffset(s, side, BollardLateral);
                    if (!first) acc += Vector3.Distance(p, prev);
                    prev = p; first = false;
                    if (acc < BoulevardLayout.BollardSpacing) continue;
                    // gaps: the south tram track, the median barriers (outer edge near z 206.7, inner edge near z 183.0)
                    if (Math.Abs(p.z - BoulevardLayout.TrackSouthZ) < 1.1) continue;
                    if (side < 0 && p.z > BoulevardLayout.MedianZ1 - 1.1) continue;
                    if (side > 0 && p.z > BoulevardLayout.MedianZ0 - 0.5) continue;
                    if (side < 0 && Math.Abs(p.z - (BoulevardLayout.MedianZ0 + 0.15)) < 0.6) continue;
                    if (p.z < BoulevardLayout.SouthZ + 0.45) continue;            // clear of the podium / pier faces at z 120
                    list.Add(p);
                    acc = 0.0;
                }
            }
            return list;
        }

        /// <summary>The point `offset` metres outside the lane edge of `side` at s, along the edge curve's own outward normal
        /// (in the steep mouth funnel s 120-124 the edge runs almost sideways, so a lateral offset would leave only
        /// 0.51 cos 75 deg = 0.13 m to the reachable region; the normal offset keeps the full distance).</summary>
        public static Vector3 EdgeOffset(double s, int side, double offset)
        {
            const double e = 0.02;
            Vector3 a = NocLanes.CorridorToWorld(s - e, side * NocLanes.LaneHalfWidth(s - e), 0.0);
            Vector3 b = NocLanes.CorridorToWorld(s + e, side * NocLanes.LaneHalfWidth(s + e), 0.0);
            Vector3 c = NocLanes.CorridorToWorld(s, side * NocLanes.LaneHalfWidth(s), 0.0);
            Vector3 t = (b - a).normalized;
            // outward: left of the tangent for the left edge (side -1), right of it for the right edge
            Vector3 n = side < 0 ? new Vector3(-t.z, 0f, t.x) : new Vector3(t.z, 0f, -t.x);
            return c + n * (float)offset;
        }

        /// <summary>x where the C1 outer lane edge (at the bollard line) crosses the south barrier line (z 183.15).</summary>
        public static double SouthBarrierGapX0()
        {
            double best = 0.0, bestD = 1e9;
            for (double s = 171.0; s <= 208.7; s += 0.01)
            {
                Vector3 p = NocLanes.CorridorToWorld(s, -(NocLanes.LaneHalfWidth(s) + BollardLateral), 0.0);
                double d = Math.Abs(p.z - (BoulevardLayout.MedianZ0 + 0.15));
                if (d < bestD) { bestD = d; best = p.x; }
            }
            return best;
        }

        /// <summary>The 40 lamp posts (x, row: -1 south z 183.15, +1 north z 206.85), from x 208 westward.</summary>
        public static List<Vector3> LampPositions(List<Vector3> bollards)
        {
            var list = new List<Vector3>(BoulevardLayout.LampCount);
            for (int k = 0; list.Count < BoulevardLayout.LampCount && k < 60; k++)
            {
                double x = 208.0 - BoulevardLayout.LampPitch * k;
                for (int row = 1; row >= -1 && list.Count < BoulevardLayout.LampCount; row -= 2)
                {
                    double z = BoulevardLayout.AxisZ + row * LampLateral;
                    if (row > 0 && x > BoulevardLayout.SectorX0 - 0.5 && x < BoulevardLayout.SectorX1 + 0.5) continue;
                    var p = new Vector3((float)x, 0f, (float)z);
                    if (NocLanes.DistanceOutsideLanes(p, 200.0) < LampClear) continue;
                    bool nearBollard = false;
                    foreach (var b in bollards) if ((b - p).sqrMagnitude < 1.0f) { nearBollard = true; break; }
                    if (nearBollard) continue;
                    list.Add(new Vector3((float)x, row, (float)z));
                }
            }
            return list;
        }

        public static void Build(G1Parts parts)
        {
            var bollards = BollardPositions();
            var bmb = parts.Get("bollards", G1Mat.SteelPainted, new Vector3(5f, 0f, 165f));
            foreach (var p in bollards)
            {
                MeshKit.Cylinder(bmb, p, p + Vector3.up * (float)BoulevardLayout.BollardH, (float)BoulevardLayout.BollardR, 12, true);
            }
            Barriers(parts);
            Lamps(parts, LampPositions(bollards));
            Furniture(parts);
        }

        static void Barriers(G1Parts parts)
        {
            var mb = parts.Get("medianBarriers", G1Mat.Concrete, new Vector3(40f, 0.45f, 195f));
            float h = (float)BoulevardLayout.BarrierH, t = (float)BoulevardLayout.BarrierT;
            float zs0 = (float)BoulevardLayout.MedianZ0, zn1 = (float)BoulevardLayout.MedianZ1;
            float xw = (float)BoulevardLayout.BarrierX0, xe = (float)BoulevardLayout.BarrierX1;
            float gap0 = (float)SouthBarrierGapX0() - 0.4f;
            // north barrier: one run; south: west part to the C1 crossing, east part from the end of the C1 inner arc (x 24)
            Run(mb, xw, xe, zn1 - t, zn1, h);
            Run(mb, xw, gap0, zs0, zs0 + t, h);
            Run(mb, 24f, xe, zs0, zs0 + t, h);
        }

        /// <summary>A barrier run in pieces of at most 40 m (bevelled boxes, one shared plane per side, abutting end to end).</summary>
        static void Run(MeshBuilder mb, float x0, float x1, float z0, float z1, float h)
        {
            int n = Mathf.Max(1, Mathf.CeilToInt((x1 - x0) / 40f));
            for (int i = 0; i < n; i++)
            {
                float a = Mathf.Lerp(x0, x1, i / (float)n), b = Mathf.Lerp(x0, x1, (i + 1) / (float)n);
                // 1 cm joints so neighbouring end faces never share a plane
                G1Mesh.Chamfer(mb, new Vector3(a + (i > 0 ? 0.005f : 0f), 0f, z0), new Vector3(b - (i < n - 1 ? 0.005f : 0f), h, z1), 0.02f);
            }
        }

        static void Lamps(G1Parts parts, List<Vector3> lamps)
        {
            var post = parts.Get("lampPosts", G1Mat.LinePost, new Vector3(40f, 6f, 195f));
            var head = parts.Get("lampHeads", G1Mat.EmitLamp, new Vector3(40f, 11.5f, 195f));   // LampWarm x 1.6 (LOOK_V2 6; v1 SodiumAmber)
            var housing = parts.Get("lampHousings", G1Mat.SteelPainted, new Vector3(40f, 11.6f, 195f));
            var pts = new List<Vector3>(3); var frames = new List<LoftFrame>(3); var sec = new CrossSection();
            var halos = new List<Vector3>(lamps.Count);
            float H = (float)BoulevardLayout.LampH, r = 0.5f * G1Materials.Post;
            float hz = 0.5f * (float)BoulevardLayout.LampHead, hx = 0.5f * (float)BoulevardLayout.LampHeadW;
            for (int i = 0; i < lamps.Count; i++)
            {
                float x = lamps[i].x, row = lamps[i].y, z = lamps[i].z;
                float zi = z - row * (float)BoulevardLayout.LampArm;                 // arm end over the median
                // post and arm as one NocLine tube (>= 1.75 px at any distance: a 0.12 m box arm aliased to 1 px specks); the arm
                // runs into the housing's back end above the lens, so no dark line crosses the bright lens
                pts.Clear(); pts.Add(new Vector3(x, 0f, z)); pts.Add(new Vector3(x, H - 0.2f, z)); pts.Add(new Vector3(x, H - 0.2f, zi + row * (hz - 0.15f)));
                G1Mesh.Tube(post, pts, r, 8, frames, sec);
                // BV (Phase 4, KS05 / CRIT0 #18: the heads were flat white slabs): a real luminaire. A dark painted-steel shell (a
                // lower body 1.0 x 0.50 x 0.16 m that overhangs the lens by 4 cm all round, a 0.06 m roof plate stepping in, a tapered
                // tail where the arm enters) and under it the drop lens (LampWarm x 1.6: 0.80 x 0.42, 0.22 m deep with 5 cm rounded
                // edges, the lens front dimmer in no way: it is the emitter). The lens keeps the old head's 0.22 m depth and 0.8 m
                // length, so at 150 m it still covers >= 2 x 4 px (the 14.10 neon mask) and its halo sits where it was.
                float yl1 = H - LensDrop, yl0 = yl1 - LensDepth;                     // lens top (inside the shell) and bottom
                G1Mesh.Chamfer(head, new Vector3(x - hx + 0.02f, yl0, zi - hz), new Vector3(x + hx - 0.02f, yl1 + 0.02f, zi + hz), 0.05f);
                G1Mesh.Chamfer(housing, new Vector3(x - hx - 0.04f, yl1, zi - hz - 0.06f), new Vector3(x + hx + 0.04f, yl1 + 0.16f, zi + hz + 0.06f), 0.03f);
                G1Mesh.Chamfer(housing, new Vector3(x - hx + 0.03f, yl1 + 0.16f, zi - hz + 0.05f), new Vector3(x + hx - 0.03f, yl1 + 0.22f, zi + hz - 0.05f), 0.02f);
                float zt0 = zi + row * (hz + 0.06f), zt1 = zi + row * (hz + 0.30f);   // the tail toward the post, over the arm
                G1Mesh.Chamfer(housing, new Vector3(x - 0.13f, yl1 + 0.04f, Mathf.Min(zt0, zt1)), new Vector3(x + 0.13f, yl1 + 0.18f, Mathf.Max(zt0, zt1)), 0.02f);
                halos.Add(new Vector3(x, yl0 + 0.08f, zi));
            }
            // LOOK_V2 6 light in the air: every head gets an in-scatter halo (sigma 0.8 m for r 14, NocHalos)
            NocHalos.Queue("BoulevardLamps", halos, G1Materials.Lin(NocPal.LampWarm, LampRadiance), LampLightRange);
            // the lamps' light: downward spots under every lens, weighed by BoulevardLights (the 14.2.3 budget)
        }

        /// <summary>The lens of a lamp head: its top 0.30 m under the post top (H), 0.22 m deep (the old head's depth).</summary>
        public const float LensDrop = 0.30f, LensDepth = 0.22f;

        public static void RegisterSources()
        {
            var lamps = LampPositions(BollardPositions());
            float H = (float)BoulevardLayout.LampH;
            foreach (var l in lamps)
            {
                if (l.x < LampWetX0) continue;
                var p = new Vector3(l.x, H - LensDrop - 0.5f * LensDepth, l.z - l.y * (float)BoulevardLayout.LampArm);
                NocWetRegistry.AddSource(p, p, LampRho, G1Materials.Lin(NocPal.LampWarm, LampRadiance));
            }
        }

        static void Furniture(G1Parts parts)
        {
            var mb = parts.Get("furniture", G1Mat.SteelPainted, new Vector3(-30f, 1f, 195f));
            var stone = parts.Get("planters", G1Mat.StoneWall, new Vector3(60f, 0.5f, 195f));
            var glow = parts.Get("kioskGlow", G1Mat.EmitShopWarm, new Vector3(-36f, 1.5f, 195f));
            float kw = 0.5f * (float)BoulevardLayout.KioskW, kh = (float)BoulevardLayout.KioskH;
            // kiosks on the median west of C1 (x -50, -36, -22), each with a lit hatch on its east face
            for (int i = 0; i < BoulevardLayout.Kiosks; i++)
            {
                float x = -50f + 14f * i, z = (float)BoulevardLayout.AxisZ;
                G1Mesh.Chamfer(mb, new Vector3(x - kw, 0f, z - kw), new Vector3(x + kw, kh, z + kw), 0.03f);
                G1Mesh.Chamfer(mb, new Vector3(x - kw - 0.25f, kh, z - kw - 0.25f), new Vector3(x + kw + 0.25f, kh + 0.15f, z + kw + 0.25f), 0.02f);
                G1Mesh.Quad(glow, new Vector3(x + kw + 0.01f, 1.0f, z - 0.9f), new Vector3(x + kw + 0.01f, 2.1f, z - 0.9f),
                    new Vector3(x + kw + 0.01f, 2.1f, z + 0.9f), new Vector3(x + kw + 0.01f, 1.0f, z + 0.9f), Vector3.right);
            }
            // benches: 6 on the west median (facing the kiosks), 6 in the north arcade (bay centres x = 6 k)
            float bl = 0.5f * (float)BoulevardLayout.BenchL, bd = 0.5f * (float)BoulevardLayout.BenchD, bh = (float)BoulevardLayout.BenchH;
            for (int i = 0; i < 6; i++)
            {
                float x = -57f + 14f * (i / 2), z = (float)BoulevardLayout.AxisZ + ((i & 1) == 0 ? -6.5f : 6.5f);
                Bench(mb, x, z, bl, bd, bh);
            }
            float[] arcadeX = { -24f, -12f, 12f, 24f, 48f, 60f };
            float za = (float)(BoulevardLayout.NorthZ + 0.5 * BoulevardLayout.ArcadeDepth);
            foreach (float x in arcadeX) Bench(mb, x, za, bl, bd, bh);
            // planters: road side of the median barriers between the lamps (x = 201 - 14 k), none in the sight sector
            float ph = (float)BoulevardLayout.PlanterH;
            for (int k = 0; k < 14; k++)
            {
                float x = 201f - 14f * k;
                if (x < 26f) break;
                G1Mesh.Chamfer(stone, new Vector3(x - 1.2f, 0f, (float)BoulevardLayout.MedianZ0 - 1.4f), new Vector3(x + 1.2f, ph, (float)BoulevardLayout.MedianZ0 - 0.2f), 0.03f);
                bool sector = x > BoulevardLayout.SectorX0 - 1.5 && x < BoulevardLayout.SectorX1 + 1.5;
                if (!sector)
                    G1Mesh.Chamfer(stone, new Vector3(x - 1.2f, 0f, (float)BoulevardLayout.MedianZ1 + 0.2f), new Vector3(x + 1.2f, ph, (float)BoulevardLayout.MedianZ1 + 1.4f), 0.03f);
            }
            // planters around the west median furniture
            for (int i = 0; i < 4; i++)
            {
                float x = -57f + 14f * i;
                G1Mesh.Chamfer(stone, new Vector3(x - 1.0f, 0f, (float)BoulevardLayout.AxisZ - 0.8f), new Vector3(x + 1.0f, ph, (float)BoulevardLayout.AxisZ + 0.8f), 0.03f);
            }
        }

        static void Bench(MeshBuilder mb, float x, float z, float bl, float bd, float bh)
        {
            G1Mesh.Chamfer(mb, new Vector3(x - bl, bh - 0.06f, z - bd), new Vector3(x + bl, bh, z + bd), 0.01f);
            G1Mesh.Chamfer(mb, new Vector3(x - bl + 0.1f, 0f, z - bd + 0.05f), new Vector3(x - bl + 0.2f, bh - 0.06f, z + bd - 0.05f), 0.01f);
            G1Mesh.Chamfer(mb, new Vector3(x + bl - 0.2f, 0f, z - bd + 0.05f), new Vector3(x + bl - 0.1f, bh - 0.06f, z + bd - 0.05f), 0.01f);
        }
    }
}
