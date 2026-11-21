// Beat08Foot: the foot of the dam (the base station of the elevator, the apron) and the LANTERN PLATFORM 3.5 km west
// (ADDENDUM A1 00 LEAD DECISION 1): the shuttle station (deck strips either side of the track slot, a canopy with tungsten
// lamps, a buffer block), the plaza (1.2 m paving flags, kerbs, a stone plinth inside the walk's arc), eight edge lanterns
// (BwLantern x2) and the Keeper's Lantern (x6, the goal: 1.9 m beyond the rest point and 0.9 m to its right), a rail along the
// track slot, and the edge of the town west of it (houses with lit windows 110-450 m away). The track itself (guideway, magnet
// rails, marker lights) and the lowland are analytic in BwFarField, so the real slabs leave a slot where it runs. Deck and
// plaza tops are at y -5999.0 (1 m above the lowland, the cabin floor level); everything outside the walk's lane + 0.35 m.
using System;
using UnityEngine;

namespace Megastructure.Beats
{
    public sealed partial class Beat08Ending
    {
        public const double DeckY = BulwarkConst.CabinFloorFootY, DeckBottom = -6000.3;
        /// <summary>The track slot (guideway z 1495.4..1499.4 in BwFarField) left open in every real slab: z 1495.2..1499.6.</summary>
        public const double SlotZ0 = 1494.2, SlotZ1 = 1500.6;
        public const double PlazaX0 = -3519.0, PlazaX1 = -3477.0, PlazaZ1 = 1525.0;

        // ================================================================ the foot: base station and apron
        void BuildFoot(WorldContext ctx, Transform root)
        {
            var deck = new MeshBuilder();
            var apron = new MeshBuilder();
            var steel = new MeshBuilder();
            var lamps = new MeshBuilder();
            // deck strips either side of the cabin slot (the shuttle leaves west through it)
            Box(deck, -17.0, DeckBottom, 1488.8, -6.02, DeckY, SlotZ0);
            Box(deck, -17.0, DeckBottom, SlotZ1, -6.02, DeckY, 1506.0);
            // canopy: four columns, two roof pieces either side of the shaft (the rails and the cabin pass between)
            foreach (var c in new[] { (-16.4, 1489.3), (-16.4, 1505.5), (-6.7, 1489.3), (-6.7, 1505.5) })
                MeshKit.ChamferBox(steel, V(c.Item1, 0.5 * (DeckY + -5990.0), c.Item2), V(0.35, -5990.0 - DeckY, 0.35), 0.02f);
            Box(steel, -17.4, -5990.0, 1488.6, -6.02, -5989.6, 1494.0);
            Box(steel, -17.4, -5990.0, 1500.8, -6.02, -5989.6, 1506.2);
            Box(lamps, -16.0, -5990.08, 1491.0, -13.0, -5990.0, 1491.2);
            Box(lamps, -16.0, -5990.08, 1503.8, -13.0, -5990.0, 1504.0);
            // the apron at the dam foot (15 cm proud of the lowland), split along the track slot
            Box(apron, -140.0, -6000.15, 1340.0, -6.02, -5999.85, 1495.2);
            Box(apron, -140.0, -6000.15, 1499.6, -6.02, -5999.85, 1660.0);
            Flush(ctx, root, ref deck, "b8_foot_deck", m_dark);
            Flush(ctx, root, ref apron, "b8_foot_apron", m_concrete);
            Flush(ctx, root, ref steel, "b8_foot_canopy", m_paint);
            Flush(ctx, root, ref lamps, "b8_foot_lamps", m_sodium);
        }

        // ================================================================ the lantern platform
        void BuildPlatform(WorldContext ctx, Transform root)
        {
            var deck = new MeshBuilder();
            var plaza = new MeshBuilder();
            var stone = new MeshBuilder();
            var steel = new MeshBuilder();
            var lamps = new MeshBuilder();
            var rails = new MeshBuilder();
            // station deck south of the slot, the buffer block at the track end, the plaza north of the slot
            Box(deck, PlazaX0, DeckBottom, 1487.6, -3497.0, DeckY, SlotZ0);
            Box(deck, PlazaX0, DeckBottom, SlotZ0, -3514.5, DeckY - 0.002, SlotZ1);
            Box(plaza, PlazaX0, DeckBottom, SlotZ1, PlazaX1, DeckY, PlazaZ1);
            // kerbs 0.15 high on the plaza's west, north and east edges
            Box(stone, PlazaX0, DeckY, SlotZ1, PlazaX0 + 0.3, DeckY + 0.15, PlazaZ1);
            Box(stone, PlazaX0 + 0.3, DeckY, PlazaZ1 - 0.3, PlazaX1 - 0.3, DeckY + 0.15, PlazaZ1);
            Box(stone, PlazaX1 - 0.3, DeckY, SlotZ1, PlazaX1, DeckY + 0.15, PlazaZ1);
            // the plinth inside the walk's arc (a scale cue; the lane's inner edge is 5 m from the arc centre)
            EndingPath.Foot.PointAt(EndingPath.Foot.PieceStart(1), out double ax, out double az);
            Vector3 pc = V(ax + BulwarkConst.FootArcR, DeckY, az);
            Beat07SluiceLedge.Prism(stone, pc, 2.2f, 2.2f, 0.6f, 8);
            Beat07SluiceLedge.Prism(stone, pc + Vector3.up * 0.6f, 1.6f, 1.5f, 0.12f, 8);
            // a rail along the track slot east of the station (outside the lane)
            float ry = (float)DeckY;
            Beat07SluiceLedge.Tube(rails, V(-3497.5, ry + 1.05, SlotZ1 + 0.25), V(PlazaX1 + 0.4, ry + 1.05, SlotZ1 + 0.25), 0.025f, 8);
            Beat07SluiceLedge.Tube(rails, V(-3497.5, ry + 0.55, SlotZ1 + 0.25), V(PlazaX1 + 0.4, ry + 0.55, SlotZ1 + 0.25), 0.025f, 8);
            for (double x = -3497.5; x <= PlazaX1 + 0.41; x += 2.0)
                MeshKit.ChamferBox(steel, V(x, DeckY + 0.53, SlotZ1 + 0.25), V(0.08, 1.06, 0.08), 0.008f);
            // station canopy over the cabin: four columns, the roof, three tungsten bars (one real light)
            foreach (var c in new[] { (-3519.6, 1488.4), (-3519.6, 1501.3), (-3498.4, 1488.4), (-3498.4, 1501.3) })
                MeshKit.ChamferBox(steel, V(c.Item1, 0.5 * (DeckY + -5994.4), c.Item2), V(0.3, -5994.4 - DeckY, 0.3), 0.02f);
            Box(steel, -3520.5, -5994.4, 1487.8, -3497.5, -5994.0, 1502.0);
            for (int k = 0; k < 3; k++)
                Box(lamps, -3516.0 + 6.0 * k, -5994.47, 1490.0, -3513.0 + 6.0 * k, -5994.4, 1490.2);
            if (m_pool != null)
                m_pool.Add(V(-3509.0, -5995.0, 1491.0), BulwarkPalette.Linear(Pal.Tungsten), 8f, 1.2f, "beat8-station", 1f);

            // lanterns on the plaza edges (x2) and the Keeper's Lantern (x6, the goal)
            var iron = new MeshBuilder();
            var drum = new MeshBuilder();
            var cage = new MeshBuilder();
            var kIron = new MeshBuilder();
            var kDrum = new MeshBuilder();
            var kCage = new MeshBuilder();
            var spots = new[] { (-3517.6, 1506.0), (-3517.6, 1515.5), (-3511.0, 1523.6), (-3501.0, 1523.6), (-3491.0, 1523.6),
                                (-3481.0, 1523.6), (-3478.4, 1512.0), (-3478.4, 1503.0) };
            float dy = (float)(BulwarkConst.LanternPost + Beat07Layout.DrumRing + 0.5 * Beat07Layout.DrumHeight);
            foreach (var p in spots)
            {
                Beat07SluiceLedge.LanternMeshes(iron, drum, cage, (float)p.Item1, (float)DeckY, (float)p.Item2);
                if (m_pool != null)
                    m_lanternLights.Add(m_pool.Add(V(p.Item1, DeckY + dy, p.Item2), BulwarkPalette.Linear(Pal.Lantern), 4.5f, 0.8f, "beat8-lantern", 1f));
            }
            Beat07SluiceLedge.LanternMeshes(kIron, kDrum, kCage, (float)EndingPath.KeeperX, (float)DeckY, (float)EndingPath.KeeperZ, (float)BulwarkConst.KeeperPost);
            float kdy = (float)(BulwarkConst.KeeperPost + Beat07Layout.DrumRing + 0.5 * Beat07Layout.DrumHeight);
            if (m_pool != null)
                m_keeperLight = m_pool.Add(V(EndingPath.KeeperX, DeckY + kdy, EndingPath.KeeperZ), BulwarkPalette.Linear(Pal.Lantern),
                                           (float)BulwarkConst.LanternLightRange, (float)BulwarkConst.LanternLightIntensity, "beat8-keeper", 0.3f);

            // the far field's lantern pools on the meadow (BwFarField _BwEndLamps): the eight edge lanterns and the Keeper
            var pools = new Vector4[10];
            for (int i = 0; i < spots.Length && i < 9; i++) pools[i] = new Vector4((float)spots[i].Item1, (float)(DeckY + dy), (float)spots[i].Item2, 5.0f);
            pools[9] = new Vector4((float)EndingPath.KeeperX, (float)(DeckY + kdy), (float)EndingPath.KeeperZ, 4.0f);
            Shader.SetGlobalVectorArray(IdEndLamps, pools);
            // the plaza as one glow for the far field (the lantern meshes are off beyond 300 m): centre 1.5 m above the paving
            Shader.SetGlobalVector(IdEndGlow, new Vector4(-3498.0f, (float)(DeckY + 1.5), 1512.0f, 16.0f));
            Flush(ctx, root, ref deck, "b8_platform_deck", m_dark);
            Flush(ctx, root, ref plaza, "b8_platform_plaza", m_paving != null ? m_paving : m_concrete);
            Flush(ctx, root, ref stone, "b8_platform_stone", m_dark);
            Flush(ctx, root, ref steel, "b8_platform_steel", m_paint);
            Flush(ctx, root, ref lamps, "b8_platform_lamps", m_lamp);
            Flush(ctx, root, ref rails, "b8_platform_rail", m_rail);
            Flush(ctx, root, ref iron, "b8_lanterns_iron", m_iron);
            Flush(ctx, root, ref drum, "b8_lanterns_glass", m_lantern);
            Flush(ctx, root, ref cage, "b8_lanterns_cage", m_cage);
            Flush(ctx, root, ref kIron, "b8_keeper_iron", m_iron);
            Flush(ctx, root, ref kDrum, "b8_keeper_glass", m_keeper);
            Flush(ctx, root, ref kCage, "b8_keeper_cage", m_cage);
        }

        // ================================================================ trees east of the platform (Wave X)
        // From the plaza at 2.7 m the lowland between 1 km and the foot is a 1-2 px sliver; real silhouettes give the final look
        // its yardstick: a row of trees along the platform road's south kerb (z 1440, x -3150 .. -1830, every 44 m: their tops
        // sink toward the foot of the wall like the lamp string beside them) and copses north and south of the track (8-16 m
        // tall, canopies of 2-3 overlapping spheres on a trunk). Nothing within 115 m of the track (the shuttle passes at
        // 152 m/s: at 115 m the nearest tree moves at most 1.3 rad/s), none within 70 m of the plaza. Dark, lit by the scene;
        // one mesh, switched off beyond 4.5 km of its centre (the upper descent), so it never sparkles from altitude.
        void BuildTrees(WorldContext ctx, Transform root)
        {
            var trees = new MeshBuilder();
            void Tree(double x, double z, uint h)
            {
                double u0 = BwMath.U01(h), u1 = BwMath.U01(BwMath.Pcg(h + 1u)), u2 = BwMath.U01(BwMath.Pcg(h + 2u));
                double ht = 8.0 + 8.0 * u0, rc = 0.26 * ht * (0.8 + 0.4 * u1);
                double y0 = BulwarkConst.LowlandY - 0.3, yTrunk = BulwarkConst.LowlandY + 0.42 * ht;
                MeshKit.TaperedCylinder(trees, V(x, y0, z), V(x, yTrunk + 0.3 * rc, z), (float)(0.03 * ht), (float)(0.018 * ht), 6, false);
                MeshKit.Sphere(trees, V(x, BulwarkConst.LowlandY + ht - rc, z), (float)rc, 14, 8);
                double a = 6.2832 * u2;
                MeshKit.Sphere(trees, V(x + 0.55 * rc * Math.Cos(a), BulwarkConst.LowlandY + ht - 1.55 * rc, z + 0.55 * rc * Math.Sin(a)), (float)(0.8 * rc), 12, 7);
                if (u1 > 0.4)
                    MeshKit.Sphere(trees, V(x - 0.5 * rc * Math.Cos(a + 1.3), BulwarkConst.LowlandY + ht - 1.4 * rc, z - 0.5 * rc * Math.Sin(a + 1.3)), (float)(0.7 * rc), 12, 7);
            }
            // the road row
            for (int i = 0; i * 44.0 <= 1320.0; i++)
            {
                uint h = BwMath.Pcg((uint)(7300 + 13 * i));
                if (BwMath.U01(BwMath.Pcg(h + 9u)) < 0.12) continue;   // a gap here and there
                Tree(-3150.0 + 44.0 * i + 6.0 * (BwMath.U01(BwMath.Pcg(h + 5u)) - 0.5), 1440.0 - 3.0 * BwMath.U01(BwMath.Pcg(h + 6u)), h);
            }
            // copses: (centre x, centre z, radius, count)
            var copses = new (double x, double z, double r, int n)[]
            {
                (-3150.0, 1760.0, 70.0, 12), (-2700.0, 1980.0, 110.0, 16), (-2350.0, 1700.0, 60.0, 9), (-1900.0, 2150.0, 120.0, 15),
                (-3250.0, 1240.0, 60.0, 10), (-2600.0, 1130.0, 90.0, 13), (-2050.0, 1300.0, 55.0, 8), (-1700.0, 900.0, 130.0, 14),
            };
            for (int c = 0; c < copses.Length; c++)
            {
                var cp = copses[c];
                for (int i = 0; i < cp.n; i++)
                {
                    uint h = BwMath.Pcg((uint)(8100 + 101 * c + 7 * i));
                    double r = cp.r * Math.Sqrt(BwMath.U01(BwMath.Pcg(h + 3u))), a = 6.2832 * BwMath.U01(BwMath.Pcg(h + 4u));
                    double x = cp.x + r * Math.Cos(a), z = cp.z + r * Math.Sin(a);
                    if (Math.Abs(z - 1497.4) < 115.0) continue;
                    if ((x + 3498.0) * (x + 3498.0) + (z - 1512.0) * (z - 1512.0) < 70.0 * 70.0) continue;
                    Tree(x, z, h);
                }
            }
            Flush(ctx, root, ref trees, "b8_trees", m_tree != null ? m_tree : m_dark);
        }

        // ================================================================ the town edge (west of the platform)
        void BuildTown(WorldContext ctx, Transform root)
        {
            var walls = new MeshBuilder();
            var roofs = new MeshBuilder();
            var lit = new MeshBuilder();
            double cx = -3498.0, cz = 1512.0;
            const int n = 30;
            for (int i = 0; i < n; i++)
            {
                uint h = BwMath.Pcg((uint)(9100 + 7 * i));
                double u0 = BwMath.U01(h), u1 = BwMath.U01(BwMath.Pcg(h + 1u)), u2 = BwMath.U01(BwMath.Pcg(h + 2u)), u3 = BwMath.U01(BwMath.Pcg(h + 3u));
                double az = (205.0 + 130.0 * (i + 0.35 * u0) / n) * BwMath.Deg;   // bearings 205-335 (south-west to north-west)
                double d = 115.0 + 330.0 * u1;
                double x = cx + d * Math.Sin(az), z = cz + d * Math.Cos(az);
                double w = 7.0 + 4.0 * u2, dep = 6.0 + 3.0 * u3, ht = 5.0 + 2.0 * u0;
                double y0 = BulwarkConst.LowlandY - 0.3;
                Box(walls, x - 0.5 * w, y0, z - 0.5 * dep, x + 0.5 * w, BulwarkConst.LowlandY + ht, z + 0.5 * dep);
                // pitched roof along x (two sloped quads and the gable triangles)
                double yr = BulwarkConst.LowlandY + ht, yt = yr + 0.45 * dep;
                Vector3 a = V(x - 0.5 * w - 0.3, yr, z - 0.5 * dep - 0.3), b = V(x + 0.5 * w + 0.3, yr, z - 0.5 * dep - 0.3);
                Vector3 c = V(x + 0.5 * w + 0.3, yr, z + 0.5 * dep + 0.3), e = V(x - 0.5 * w - 0.3, yr, z + 0.5 * dep + 0.3);
                Vector3 r0 = V(x - 0.5 * w - 0.3, yt, z), r1 = V(x + 0.5 * w + 0.3, yt, z);
                roofs.FlatQuad(a, r0, r1, b);
                roofs.FlatQuad(c, r1, r0, e);
                Vector3 g0 = V(x - 0.5 * w, yr, z - 0.5 * dep), g1 = V(x - 0.5 * w, yr, z + 0.5 * dep), gt = V(x - 0.5 * w, yt - 0.2, z);
                Vector3 g2 = V(x + 0.5 * w, yr, z + 0.5 * dep), g3 = V(x + 0.5 * w, yr, z - 0.5 * dep), gu = V(x + 0.5 * w, yt - 0.2, z);
                Vector3 nW = Vector3.left, nE = Vector3.right;
                int i0 = walls.Vertex(g1, nW), i1 = walls.Vertex(gt, nW), i2 = walls.Vertex(g0, nW); walls.Tri(i0, i1, i2);
                int j0 = walls.Vertex(g3, nE), j1 = walls.Vertex(gu, nE), j2 = walls.Vertex(g2, nE); walls.Tri(j0, j1, j2);
                // lit windows on the east wall (toward the platform) and the north / south walls, 2 cm proud
                int lights = 2 + (int)(3.0 * u3);
                for (int k = 0; k < lights; k++)
                {
                    double wz = z - 0.5 * dep + dep * (k + 0.5) / lights;
                    float xe = (float)(x + 0.5 * w + 0.02);
                    double wy = BulwarkConst.LowlandY + 1.2 + (k % 2) * 2.4;
                    lit.FlatQuad(V(xe, wy, wz - 0.8), V(xe, wy + 1.5, wz - 0.8), V(xe, wy + 1.5, wz + 0.8), V(xe, wy, wz + 0.8));
                }
                double wx = x - 0.5 * w + w * (0.3 + 0.4 * u2);
                float zs = (float)(z + 0.5 * dep + 0.02);
                double wy2 = BulwarkConst.LowlandY + 1.2;
                lit.FlatQuad(V(wx + 0.8, wy2, zs), V(wx + 0.8, wy2 + 1.5, zs), V(wx - 0.8, wy2 + 1.5, zs), V(wx - 0.8, wy2, zs));
            }
            Flush(ctx, root, ref walls, "b8_town_walls", m_house);
            Flush(ctx, root, ref roofs, "b8_town_roofs", m_dark);
            Flush(ctx, root, ref lit, "b8_town_windows", m_window);
        }
    }
}
