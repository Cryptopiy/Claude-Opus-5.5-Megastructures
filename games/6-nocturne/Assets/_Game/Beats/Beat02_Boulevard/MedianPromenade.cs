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
//     the north arcade, planters on the road side of the median barriers (none in the sight sector);
//   * BV fix wave (panel p4_01 C15 scale-06: on the money stretch s 209-400 no human-scale object stood near the walker, the lower
//     thirds of KS04 / KS06 were dead paving): the promenade furniture east of C1 (PromenadeFurniture): 12 backless benches (wood
//     slats on two steel frames, seat 0.45 m, a warm LED under the seat that pools on the wet paving), 8 stone planters 1.0 m with
//     a soil bed and 2 kiosks 3 x 3 x 2.6 m (a lit hatch toward the axis, an awning, a small sign, the hatch's pool on the paving),
//     at |dz| 4.75..9.75 from the axis: off the autowalk line (capsule 0.35) and inside the wall bots' lines (+-10.6, capsule
//     10.25), the planters and kiosks only in the south half inside the tram sight sector (x 79-175: nothing above 0.5 m in z
//     195-225), nothing taller than 0.5 m across x 100 (the median-width probe). KS04 (x 65.3, pitch 27) sees a planter on
//     each side 14 m ahead (the tops; at pitch 27 the paving enters the frame at 16 m) and benches, planters and a kiosk at
//     21-40 m in its lower band, KS06 (x 79.3, yaw 57) two benches 8-13 m ahead under the tram. Each piece has a box collider
//     (Default layer: the CharacterController stops at it; PathGuide's corridor probes and kill volumes use layer 30 only).
//     The pools are NocAlleyPool quads 1 cm over the floor (queue Transparent-50: no occluder, no floor-probe hit).
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
            Promenade(parts);
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
            float hz = 0.5f * (float)BoulevardLayout.LampHead;
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
                halos.Add(Luminaire(head, housing, x, zi, row, H));
            }
            // LOOK_V2 6 light in the air: every head gets an in-scatter halo (sigma 0.8 m for r 14, NocHalos)
            NocHalos.Queue("BoulevardLamps", halos, G1Materials.Lin(NocPal.LampWarm, LampRadiance), LampLightRange);
            // the lamps' light: downward spots under every lens, weighed by BoulevardLights (the 14.2.3 budget)
        }

        /// <summary>The lens of a lamp head: its top 0.30 m under the post top (H), 0.22 m deep (the old head's depth).</summary>
        public const float LensDrop = 0.30f, LensDepth = 0.22f;

        /// <summary>One luminaire (lens into `head`, shell into `housing`) at the arm end (x, zi) under a post top at H; the post
        /// stands on the `row` side (zi + row x arm). Returns the halo point (8 cm above the lens bottom).</summary>
        public static Vector3 Luminaire(MeshBuilder head, MeshBuilder housing, float x, float zi, float row, float H)
        {
            float hz = 0.5f * (float)BoulevardLayout.LampHead, hx = 0.5f * (float)BoulevardLayout.LampHeadW;
            float yl1 = H - LensDrop, yl0 = yl1 - LensDepth;                         // lens top (inside the shell) and bottom
            G1Mesh.Chamfer(head, new Vector3(x - hx + 0.02f, yl0, zi - hz), new Vector3(x + hx - 0.02f, yl1 + 0.02f, zi + hz), 0.05f);
            G1Mesh.Chamfer(housing, new Vector3(x - hx - 0.04f, yl1, zi - hz - 0.06f), new Vector3(x + hx + 0.04f, yl1 + 0.16f, zi + hz + 0.06f), 0.03f);
            G1Mesh.Chamfer(housing, new Vector3(x - hx + 0.03f, yl1 + 0.16f, zi - hz + 0.05f), new Vector3(x + hx - 0.03f, yl1 + 0.22f, zi + hz - 0.05f), 0.02f);
            float zt0 = zi + row * (hz + 0.06f), zt1 = zi + row * (hz + 0.30f);       // the tail toward the post, over the arm
            G1Mesh.Chamfer(housing, new Vector3(x - 0.13f, yl1 + 0.04f, Mathf.Min(zt0, zt1)), new Vector3(x + 0.13f, yl1 + 0.18f, Mathf.Max(zt0, zt1)), 0.02f);
            return new Vector3(x, yl0 + 0.08f, zi);
        }

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

        // ---------------------------------------------------------------- the promenade east of C1 (BV fix wave, C15)
        public enum ObstacleKind { Bench = 0, Planter = 1, Kiosk = 2 }

        /// <summary>One piece of promenade furniture: centre x, offset from the axis z 195 (dz, + north), full size (x, y, z).</summary>
        public struct Obstacle { public ObstacleKind Kind; public float X, Dz; public Vector3 Size; }

        public const float PlanterL = 2.4f, PlanterD = 1.2f, KioskRoofOut = 0.25f, KioskRoofY = 0.15f;
        /// <summary>Collider height: above the 0.3 m step offset of the CharacterController for every piece (no climbing a bench).</summary>
        public const float ObstacleColliderH = 1.2f;
        /// <summary>Under-seat LED: its height over the floor (the pool's d) and its strength (dst x (1 + c E)); the kiosk hatch's.</summary>
        public const float SeatLedY = 0.45f, SeatLedGain = 4.0f, HatchGain = 2.4f;
        /// <summary>The planters' base: shadow-gap height and the LED in it (pool height d and strength).</summary>
        public const float PlanterBaseH = 0.10f, PlanterLedY = 0.35f, PlanterLedGain = 4.0f;

        // (x, dz, kind) east of C1; groups every 12-20 m, alternating sides; KS04 / KS06 foregrounds first
        static readonly float[] PromenadeLayout =
        {
            77.7f,  6.5f, 1,   74.0f,  6.5f, 0,                         // KS04 left foreground (planter 13.9 m ahead; west of the sector x 79)
            77.7f, -5.5f, 1,   74.0f, -5.5f, 0,                         // KS04 right foreground (KS04 pitch 27, D-040: ground in frame from 16 m)
            83.0f,  5.0f, 0,   86.6f,  5.0f, 0,   90.6f,  7.0f, 0,      // KS06 (6.3, 8.9, 12.8 m): benches only (sector, 0.45 m)
            88.0f, -6.5f, 1,
            104.0f, -8.0f, 2,  108.6f, -5.0f, 0,                        // kiosk 1 (south: it is 2.6 m tall)
            122.0f,  5.0f, 0,  126.0f, -5.0f, 0,  129.6f, -5.0f, 1,
            146.0f, -6.0f, 1,
            152.0f, -8.0f, 2,                                           // kiosk 2
            162.0f, -5.5f, 0,  165.6f, -5.5f, 1,  164.0f,  5.5f, 0,
            182.0f,  6.0f, 1,  185.6f,  6.0f, 0,  190.0f, -6.0f, 1,  193.6f, -6.0f, 0,   // east of the sector
        };

        /// <summary>The promenade furniture east of C1 (deterministic).</summary>
        public static List<Obstacle> PromenadeFurniture()
        {
            var list = new List<Obstacle>(PromenadeLayout.Length / 3);
            for (int i = 0; i + 2 < PromenadeLayout.Length; i += 3)
            {
                var k = (ObstacleKind)(int)PromenadeLayout[i + 2];
                Vector3 size = k == ObstacleKind.Bench ? new Vector3((float)BoulevardLayout.BenchL, (float)BoulevardLayout.BenchH, (float)BoulevardLayout.BenchD)
                             : k == ObstacleKind.Planter ? new Vector3(PlanterL, (float)BoulevardLayout.PlanterH, PlanterD)
                             : new Vector3((float)BoulevardLayout.KioskW, (float)BoulevardLayout.KioskH + KioskRoofY, (float)BoulevardLayout.KioskW);
                list.Add(new Obstacle { Kind = k, X = PromenadeLayout[i], Dz = PromenadeLayout[i + 1], Size = size });
            }
            return list;
        }

        /// <summary>The |dz| span an obstacle occupies (the kiosk roof overhangs its body by 0.25 m).</summary>
        public static void DzSpan(Obstacle o, out float near, out float far)
        {
            float h = 0.5f * o.Size.z + (o.Kind == ObstacleKind.Kiosk ? KioskRoofOut : 0f);
            near = Mathf.Abs(o.Dz) - h; far = Mathf.Abs(o.Dz) + h;
        }

        static void Promenade(G1Parts parts)
        {
            var wood = parts.Get("promenadeSeats", G1Mat.Wood, new Vector3(130f, 0.4f, 195f));
            var steel = parts.Get("promenadeFrames", G1Mat.SteelPainted, new Vector3(130f, 0.3f, 195f));
            var stone = parts.Get("promenadePlanters", G1Mat.Concrete, new Vector3(130f, 0.5f, 195f));
            var soil = parts.Get("promenadeSoil", G1Mat.Asphalt, new Vector3(130f, 1f, 195f));
            var kiosk = parts.Get("promenadeKiosks", G1Mat.SteelPainted, new Vector3(128f, 1.4f, 187f));
            var glow = parts.Get("promenadeKioskGlow", G1Mat.EmitShopWarm, new Vector3(128f, 1.5f, 188.5f));
            var pools = parts.Get("promenadePools", G1Mat.AlleyPool, new Vector3(130f, 0.01f, 195f));
            Color warm = Color.Lerp(G1Materials.Lin(NocPal.LampWarm, 1f), Color.white, 0.35f);
            float az = (float)BoulevardLayout.AxisZ;
            foreach (var o in PromenadeFurniture())
            {
                float x = o.X, z = az + o.Dz;
                switch (o.Kind)
                {
                    case ObstacleKind.Bench:
                        PromenadeBench(wood, steel, x, z);
                        // the under-seat LED (hidden by the seat from eye height) pools on the paving around the bench: two feet
                        foreach (float fx in new[] { -0.6f, 0f, 0.6f })
                            FloorPool(pools, x + fx - 1.2f, x + fx + 1.2f, z - 0.9f, z + 0.9f, x + fx, z, SeatLedY, warm * SeatLedGain);
                        break;
                    case ObstacleKind.Planter:
                        float hl = 0.5f * PlanterL, hd = 0.5f * PlanterD, ph = (float)BoulevardLayout.PlanterH;
                        // BV_r06 look_l / look_r: a plain block read as a black crate. A recessed base (a 6 cm shadow gap 0.10 m tall)
                        // with a warm LED in it that washes the paving around the planter, like the benches' (one design family)
                        G1Mesh.Chamfer(steel, new Vector3(x - hl + 0.06f, 0f, z - hd + 0.06f), new Vector3(x + hl - 0.06f, PlanterBaseH, z + hd - 0.06f), 0.006f);
                        G1Mesh.Chamfer(stone, new Vector3(x - hl, PlanterBaseH, z - hd), new Vector3(x + hl, ph - 0.07f, z + hd), 0.03f);
                        foreach (float sz in new[] { -1f, 1f })
                            foreach (float fx in new[] { -0.6f, 0.6f })
                                FloorPool(pools, x + fx - 1.0f, x + fx + 1.0f, sz < 0f ? z - hd - 0.9f : z + hd - 0.1f, sz < 0f ? z - hd + 0.1f : z + hd + 0.9f,
                                    x + fx, z + sz * (hd + 0.02f), PlanterLedY, warm * PlanterLedGain);
                        foreach (float sx in new[] { -1f, 1f })
                            FloorPool(pools, sx < 0f ? x - hl - 0.9f : x + hl - 0.1f, sx < 0f ? x - hl + 0.1f : x + hl + 0.9f, z - 0.9f, z + 0.9f,
                                x + sx * (hl + 0.02f), z, PlanterLedY, warm * PlanterLedGain);
                        // the coping slab, 4 cm proud all round (its chamfer catches the lamps: the planter reads as one)
                        G1Mesh.Chamfer(stone, new Vector3(x - hl - 0.04f, ph - 0.07f, z - hd - 0.04f), new Vector3(x + hl + 0.04f, ph, z + hd + 0.04f), 0.02f);
                        // the soil bed inside a 0.12 m rim, 1 cm over the coping (a dark bed)
                        float ri = 0.12f, ys = ph + 0.01f;
                        G1Mesh.Quad(soil, new Vector3(x - hl + ri, ys, z - hd + ri), new Vector3(x - hl + ri, ys, z + hd - ri),
                            new Vector3(x + hl - ri, ys, z + hd - ri), new Vector3(x + hl - ri, ys, z - hd + ri), Vector3.up);
                        break;
                    case ObstacleKind.Kiosk:
                        PromenadeKiosk(parts, kiosk, glow, pools, x, z, warm);
                        break;
                }
            }
        }

        /// <summary>A backless bench 1.8 x 0.5 m, seat top 0.45: three wood slats (0.14 x 0.04, 4 cm gaps) on two steel frames.</summary>
        static void PromenadeBench(MeshBuilder wood, MeshBuilder steel, float x, float z)
        {
            float bl = 0.5f * (float)BoulevardLayout.BenchL, bd = 0.5f * (float)BoulevardLayout.BenchD, bh = (float)BoulevardLayout.BenchH;
            const float slat = 0.14f, gap = 0.04f, th = 0.04f;
            for (int i = 0; i < 3; i++)
            {
                float z0 = z - bd + i * (slat + gap);
                G1Mesh.Chamfer(wood, new Vector3(x - bl, bh - th, z0), new Vector3(x + bl, bh, z0 + slat), 0.008f);
            }
            foreach (float fx in new[] { -0.62f, 0.62f })
            {
                // a flat steel frame: the cross rail under the slats and two legs
                G1Mesh.Chamfer(steel, new Vector3(x + fx - 0.04f, bh - th - 0.05f, z - bd + 0.02f), new Vector3(x + fx + 0.04f, bh - th - 0.002f, z + bd - 0.02f), 0.006f);
                G1Mesh.Chamfer(steel, new Vector3(x + fx - 0.04f, 0f, z - bd + 0.05f), new Vector3(x + fx + 0.04f, bh - th - 0.052f, z - bd + 0.11f), 0.006f);
                G1Mesh.Chamfer(steel, new Vector3(x + fx - 0.04f, 0f, z + bd - 0.11f), new Vector3(x + fx + 0.04f, bh - th - 0.052f, z + bd - 0.05f), 0.006f);
            }
            // the LED channel under the middle slat (dark steel; the light shows only as its pool)
            G1Mesh.Chamfer(steel, new Vector3(x - bl + 0.15f, bh - th - 0.03f, z - 0.04f), new Vector3(x + bl - 0.15f, bh - th - 0.002f, z + 0.04f), 0.004f);
        }

        /// <summary>A kiosk 3 x 3 x 2.6 m: body, roof slab, a lit hatch on its west face (toward the approaching walker; BV_r05: a
        /// hatch facing the axis showed KS04 a blank box) with a counter and an awning, a small sign over the awning, and the hatch's
        /// pool on the paving in front. Nobody serves (DREAD 6: traces of emptiness).</summary>
        static void PromenadeKiosk(G1Parts parts, MeshBuilder mb, MeshBuilder glow, MeshBuilder pools, float x, float z, Color warm)
        {
            float kw = 0.5f * (float)BoulevardLayout.KioskW, kh = (float)BoulevardLayout.KioskH;
            // a plinth 0.12 m (chamfer 6 mm, inset 3 cm) under the body: a floor probe from 5 cm (Q1IntegrationTests) meets only
            // the plinth's 6 mm bevel (BV-t07: the body's 3 cm bottom bevel read as a 0.023 m step at lat 6.51)
            G1Mesh.Chamfer(mb, new Vector3(x - kw + 0.03f, 0f, z - kw + 0.03f), new Vector3(x + kw - 0.03f, 0.12f, z + kw - 0.03f), 0.006f);
            G1Mesh.Chamfer(mb, new Vector3(x - kw, 0.12f, z - kw), new Vector3(x + kw, kh, z + kw), 0.03f);
            G1Mesh.Chamfer(mb, new Vector3(x - kw - KioskRoofOut, kh, z - kw - KioskRoofOut), new Vector3(x + kw + KioskRoofOut, kh + KioskRoofY, z + kw + KioskRoofOut), 0.02f);
            float xf = x - kw - 0.01f;
            // the hatch (lit interior, EmitShopWarm) 1.9 x 1.0 m over a 1.0 m counter
            G1Mesh.Quad(glow, new Vector3(xf, 1.05f, z - 0.95f), new Vector3(xf, 2.05f, z - 0.95f), new Vector3(xf, 2.05f, z + 0.95f), new Vector3(xf, 1.05f, z + 0.95f), Vector3.left);
            G1Mesh.Chamfer(mb, new Vector3(x - kw - 0.32f, 0.98f, z - 1.05f), new Vector3(x - kw, 1.04f, z + 1.05f), 0.008f);
            // the awning: a thin steel plate 0.7 m out over the hatch
            G1Mesh.Chamfer(mb, new Vector3(x - kw - 0.7f, 2.18f, z - 1.2f), new Vector3(x - kw, 2.24f, z + 1.2f), 0.008f);
            // a small sign on the fascia over the awning (white or tungsten), facing west
            uint h = NocMath.Pcg((uint)Mathf.RoundToInt(x * 8f) ^ 0xC105Cu);
            parts.Sign(new Vector3(x - kw - 0.02f, 2.43f, z), Quaternion.LookRotation(Vector3.right), 1.3f, 0.30f, NocMath.U01(h) < 0.5 ? 4 : 7);
            // the hatch light on the paving in front (source at the hatch centre, 1.55 m up; the panel runs under the kiosk, hidden)
            // (z +-2.5: the panel ends 0.6 m inside the lane edge at |dz| 11 - BV-t05: +-3.2 reached the wall bots' capsule)
            FloorPool(pools, x - kw - 4.5f, x - kw + 0.4f, z - 2.5f, z + 2.5f, x - kw - 0.05f, z, 1.55f, warm * HatchGain);
        }

        /// <summary>A light pool on the floor (NocAlleyPool, y 0.01): panel x0..x1 by z0..z1, the source's foot (fx, fz) at height d,
        /// colour = light x strength (dst x (1 + c E)); UV0 panel metres, UV1 (W, H, foot u, foot v).</summary>
        public static void FloorPool(MeshBuilder mb, float x0, float x1, float z0, float z1, float fx, float fz, float d, Color c)
            => FloorPoolAt(mb, x0, x1, z0, z1, fx, fz, 0.01f, d, c);

        /// <summary>FloorPool on a floor at height y (the quad lies at y; d is the source's height over that floor).</summary>
        public static void FloorPoolAt(MeshBuilder mb, float x0, float x1, float z0, float z1, float fx, float fz, float y, float d, Color c)
        {
            var saved = mb.Color; bool w1 = mb.WriteUV1; Vector4 c1 = mb.CurrentUV1;
            mb.Color = new Color(Mathf.Clamp01(c.r / 4f), Mathf.Clamp01(c.g / 4f), Mathf.Clamp01(c.b / 4f), Mathf.Clamp01(d / 4f));
            mb.WriteUV1 = true; mb.CurrentUV1 = new Vector4(x1 - x0, z1 - z0, fx - x0, fz - z0);
            Vector3 A = new Vector3(x0, y, z0), B = new Vector3(x0, y, z1), C = new Vector3(x1, y, z1), D = new Vector3(x1, y, z0);
            Vector2 uA = new Vector2(0f, 0f), uB = new Vector2(0f, z1 - z0), uC = new Vector2(x1 - x0, z1 - z0), uD = new Vector2(x1 - x0, 0f);
            if (Vector3.Dot(Vector3.Cross(B - A, C - A), Vector3.up) < 0f) mb.FlatQuad(A, D, C, B, uA, uD, uC, uB);
            else mb.FlatQuad(A, B, C, D, uA, uB, uC, uD);
            mb.Color = saved; mb.WriteUV1 = w1; mb.CurrentUV1 = c1;
        }

        /// <summary>The promenade furniture's box colliders (Default layer, under root): the walker stops at a bench, a planter or a
        /// kiosk instead of walking through it. Built once at load (no per-frame work). Returns the number of colliders.</summary>
        public static int AddColliders(Transform root)
        {
            var parent = new GameObject("promenadeColliders").transform;
            parent.SetParent(root, false);
            int n = 0;
            foreach (var o in PromenadeFurniture())
            {
                float hgt = Mathf.Max(o.Size.y, ObstacleColliderH);
                var go = new GameObject("promenadeObstacle" + n);
                go.transform.SetParent(parent, false);
                go.transform.position = new Vector3(o.X, 0.5f * hgt, (float)BoulevardLayout.AxisZ + o.Dz);
                var bc = go.AddComponent<BoxCollider>();
                bc.size = new Vector3(o.Size.x, hgt, o.Size.z);
                n++;
            }
            return n;
        }

        static void Bench(MeshBuilder mb, float x, float z, float bl, float bd, float bh)
        {
            G1Mesh.Chamfer(mb, new Vector3(x - bl, bh - 0.06f, z - bd), new Vector3(x + bl, bh, z + bd), 0.01f);
            G1Mesh.Chamfer(mb, new Vector3(x - bl + 0.1f, 0f, z - bd + 0.05f), new Vector3(x - bl + 0.2f, bh - 0.06f, z + bd - 0.05f), 0.01f);
            G1Mesh.Chamfer(mb, new Vector3(x + bl - 0.2f, 0f, z - bd + 0.05f), new Vector3(x + bl - 0.1f, bh - 0.06f, z + bd - 0.05f), 0.01f);
        }
    }
}
