// GrayboxDam: the fixed graybox pieces of beats 1-4 (road mesh z -300..1800) and the far props. See GrayboxPath.cs for the
// file map. All numbers from BulwarkConst (14.4, 14.5.1-14.5.4, 14.12); assumptions the spec leaves open are named where
// they are made. Nothing visible stands inside the capsule reach of the lane (lane edge + PLAYER_R 0.35) except the cart
// (a PathGuide obstacle). Every piece is keyed to one part (beat1..beat7, sea, sky: the part map in GrayboxPath.cs).
using System;
using Megastructure.Player;
using UnityEngine;

namespace Megastructure.Beats
{
    public sealed partial class GrayboxPath
    {
        // ================================================================ beat 1: wave gallery (graybox: box corridor, lamps, box door)
        void BuildGallery(WorldContext ctx, Transform root)
        {
            if (m_off[K1]) return;   // replaced: no shell, lamps or door leaves (SetDoorGap then does nothing)
            var mb = new MeshBuilder();
            double sx0 = BulwarkConst.ShellXMin, sx1 = BulwarkConst.ShellXMax, wi0 = BulwarkConst.GalleryWallXMin, wi1 = BulwarkConst.GalleryWallXMax;
            double sy1 = BulwarkConst.ShellYMax, clear = BulwarkConst.GalleryClearHeight, dz = BulwarkConst.DoorPlaneZ, dz1 = BulwarkConst.DoorWallNorthZ;
            double z0 = BulwarkConst.ShellZMin;
            // west wall with the niches 1.0 x 2.1 x 0.8 every 24 m (14.5.1; z 6 + 24 k assumed, so the valve wheels at z 30 and
            // 78 sit in two of them: west-wall props stay outside the capsule reach of the corridor lane, x -3.15 - 0.35 = -3.5),
            // roof, south end wall. The 0.8 m niche is deeper than the 0.6 m wall: its back and cheeks stand 0.2 m proud of
            // the shell's outer face (x -4.1), on the lee strip where nobody walks.
            double zw = z0;
            for (double zn = NicheZ0; zn + 0.5 * BulwarkConst.NicheW < dz; zn += BulwarkConst.NichePitch)
            {
                double n0 = zn - 0.5 * BulwarkConst.NicheW, n1 = zn + 0.5 * BulwarkConst.NicheW, nh = BulwarkConst.NicheH, back = wi0 - BulwarkConst.NicheD;
                BoxMinMax(mb, sx0, 0.0, zw, wi0, sy1, n0);
                BoxMinMax(mb, sx0, nh, n0, wi0, sy1, n1);
                BoxMinMax(mb, back - 0.2, 0.0, n0 - 0.2, back, nh + 0.2, n1 + 0.2);
                BoxMinMax(mb, back, 0.0, n0 - 0.2, sx0, nh + 0.2, n0);
                BoxMinMax(mb, back, 0.0, n1, sx0, nh + 0.2, n1 + 0.2);
                BoxMinMax(mb, back, nh, n0, sx0, nh + 0.2, n1);
                zw = n1;
            }
            BoxMinMax(mb, sx0, 0.0, zw, wi0, sy1, dz);
            BoxMinMax(mb, wi0, clear, z0, wi1, sy1, dz);
            BoxMinMax(mb, wi0, 0.0, z0, wi1, clear, -0.4);
            // east wall with the ten wave slits (x 0.5, 0.3 wide, y 1.0-2.8, z 6 + 12 k)
            double hw = 0.5 * BulwarkConst.SlitWidth, zc = z0;
            for (int k = 0; k < BulwarkConst.SlitCount; k++)
            {
                double z = BulwarkConst.SlitZ0 + BulwarkConst.SlitPitch * k;
                BoxMinMax(mb, wi1, 0.0, zc, sx1, sy1, z - hw);
                BoxMinMax(mb, wi1, 0.0, z - hw, sx1, BulwarkConst.SlitY0, z + hw);
                BoxMinMax(mb, wi1, BulwarkConst.SlitY1, z - hw, sx1, sy1, z + hw);
                zc = z + hw;
            }
            BoxMinMax(mb, wi1, 0.0, zc, sx1, sy1, dz);
            // door wall with the leaf pockets (2.1 m each side) and the lintel (z 120 .. 120.7)
            BoxMinMax(mb, wi0 - 2.1, 0.0, dz, wi0, sy1, dz1);
            BoxMinMax(mb, wi1, 0.0, dz, wi1 + 2.1, sy1, dz1);
            BoxMinMax(mb, wi0, BulwarkConst.DoorOpeningHeight, dz, wi1, sy1, dz1);
            FlushKey(ctx, K1, ref mb, "gb_gallery_shell", m_concrete);

            // slit channels 1.2 m deep: cheeks through the 0.6 m outer baffle zone and a baffle plate (no sea visible); the
            // storm light reaches the slit off the baffle (stand-in glow on its inner face)
            var dark = new MeshBuilder();
            var glow = new MeshBuilder();
            double bx0 = sx1, bx1 = BulwarkConst.SlitX + BulwarkConst.SlitDepth;   // 1.1 .. 1.7
            for (int k = 0; k < BulwarkConst.SlitCount; k++)
            {
                double z = BulwarkConst.SlitZ0 + BulwarkConst.SlitPitch * k;
                BoxMinMax(dark, bx0, BulwarkConst.SlitY0 - 0.2, z - hw - 0.2, bx1, BulwarkConst.SlitY1 + 0.2, z - hw);
                BoxMinMax(dark, bx0, BulwarkConst.SlitY0 - 0.2, z + hw, bx1, BulwarkConst.SlitY1 + 0.2, z + hw + 0.2);
                BoxMinMax(dark, bx0, BulwarkConst.SlitY1, z - hw, bx1, BulwarkConst.SlitY1 + 0.2, z + hw);
                BoxMinMax(dark, bx0, BulwarkConst.SlitY0 - 0.2, z - hw, bx1, BulwarkConst.SlitY0, z + hw);
                BoxMinMax(dark, bx1, BulwarkConst.SlitY0 - 0.4, z - 0.6, bx1 + 0.25, BulwarkConst.SlitY1 + 0.4, z + 0.6);
                // glow quad on the baffle's inner face (faces -x, toward the gallery)
                float gx = (float)(bx1 - 0.005);
                glow.FlatQuad(new Vector3(gx, (float)BulwarkConst.SlitY0, (float)(z + hw)), new Vector3(gx, (float)BulwarkConst.SlitY1, (float)(z + hw)),
                              new Vector3(gx, (float)BulwarkConst.SlitY1, (float)(z - hw)), new Vector3(gx, (float)BulwarkConst.SlitY0, (float)(z - hw)));
            }
            FlushKey(ctx, K1, ref dark, "gb_gallery_baffles", m_dark);
            FlushKey(ctx, K1, ref glow, "gb_lamp_slits", m_storm, false);

            // caged lamps at z 12 k (k 1-9), 0.45 x 0.2 m, hung on the centre line under the roof (position assumed:
            // 14.5.1 gives size and z only); real point lights on the 4 nearest (9 m, 1.6)
            var lamps = new MeshBuilder();
            var rods = new MeshBuilder();
            float lx = (float)BulwarkConst.StartX, ly = (float)(clear - 0.15);
            for (int k = 1; k <= BulwarkConst.GalleryLampCount; k++)
            {
                float z = (float)(BulwarkConst.GalleryLampPitch * k);
                lamps.Box(new Vector3(lx, ly, z), new Vector3((float)BulwarkConst.GalleryLampW, (float)BulwarkConst.GalleryLampH, 0.2f));
                rods.Box(new Vector3(lx, ly + 0.1f + 0.025f, z), new Vector3(0.05f, 0.05f, 0.05f));
                AddAnchor(K1, new Vector3(lx, ly - 0.3f, z), Pal.SodiumWarm, 9f, 1.6f);
            }
            FlushKey(ctx, K1, ref lamps, "gb_lamp_gallery", m_sodium, false);
            // valve wheels 0.5 m at z 30 and 78 (scale cue) on the back of their niches, 0.12 m proud of it with a stem
            float vx = (float)(wi0 - BulwarkConst.NicheD + 0.12), vr = 0.5f * (float)BulwarkConst.ValveWheelDiameter;
            foreach (double vz in new[] { BulwarkConst.ValveWheelZ1, BulwarkConst.ValveWheelZ2 })
            {
                MeshKit.Torus(rods, new Vector3(vx, 1.3f, (float)vz), Vector3.right, vr - 0.03f, 0.03f, 24, 8);
                BoxMinMax(rods, wi0 - BulwarkConst.NicheD, 1.27, vz - 0.03, vx, 1.33, vz + 0.03);
            }
            FlushKey(ctx, K1, ref rods, "gb_gallery_fittings", m_steel);

            // door leaves 2.0 x 3.4 (sliding into the pockets; OnFrame sets the gap from s), SteelPaint with an ochre seam band
            double lw = BulwarkConst.DoorLeafW, lh = BulwarkConst.DoorOpeningHeight, lz = 0.5 * (dz + dz1);
            m_leafW = Leaf(ctx, Group(K1), "gb_door_leaf_w", new Vector3((float)(wi0 + 0.5 * lw), (float)(0.5 * lh), (float)lz), lw, lh, +1);
            m_leafE = Leaf(ctx, Group(K1), "gb_door_leaf_e", new Vector3((float)(wi1 - 0.5 * lw), (float)(0.5 * lh), (float)lz), lw, lh, -1);
            m_leafW0 = m_leafW.localPosition;
            m_leafE0 = m_leafE.localPosition;
            // the door is a sky opening for the gallery while it is open (LocalLightPool; the gap is set in SetDoorGap)
            m_doorOpening = AddOpening(K1, "door", -1.5, -1.5, 0.0, BulwarkConst.DoorOpeningHeight, dz, true, false);
            SetDoorGap(0f);
        }

        /// <summary>Centre z of the first west-wall niche (assumed: 6 + 24 k puts the valve wheels of z 30 and 78 in niches).</summary>
        const double NicheZ0 = 6.0;

        /// <summary>One sliding leaf (0.5 m thick) with an ochre seam band on its meeting edge (seamSide +1: the band on +x).</summary>
        Transform Leaf(WorldContext ctx, Transform root, string name, Vector3 centre, double w, double h, int seamSide)
        {
            var mb = new MeshBuilder();
            mb.Box(Vector3.zero, new Vector3((float)w - 0.02f, (float)h - 0.01f, 0.5f));
            var mr = ctx.AddMesh(root, name, mb.ToMesh(name), m_steel);
            mr.transform.localPosition = centre;
            var band = new MeshBuilder();
            float bx = seamSide * (0.5f * (float)w - 0.06f);
            band.Box(new Vector3(bx, 0f, -0.27f), new Vector3(0.08f, (float)h - 0.06f, 0.04f));
            band.Box(new Vector3(0f, 1.1f - 0.5f * (float)h, -0.27f), new Vector3((float)w - 0.04f, 0.08f, 0.04f));
            var br = ctx.AddMesh(mr.transform, name + "_band", band.ToMesh(name + "_band"), m_ochre);
            br.transform.localPosition = Vector3.zero;
            return mr.transform;
        }

        // ================================================================ beats 2-4: crest road (graybox: deck, parapet, rail)
        void BuildCrest(WorldContext ctx, Transform root)
        {
            double sp0 = BulwarkConst.SpillwayZ0, sp1 = BulwarkConst.SpillwayZ1, tz0 = BulwarkConst.TowerZ0, tz1 = BulwarkConst.TowerZ1;
            // the road mesh, faces, parapet and lee rail run z -300..1800 (14.5.2 road mesh); the gallery shell sits on it
            double zEnd = BulwarkConst.RoadMeshZMax, zS = BulwarkConst.RoadMeshZMin;
            // every piece goes into the set of its crest band's part key (CBox / ForCrestBands, GrayboxPath)
            var deck = NewSet();
            // road deck between the faces (x -5.5..5.5): the crest, beat 4; under the gallery shell (x -4.1..1.1, z -0.6..120.7,
            // the 14.4 "road deck cut") the gallery floor is beat 1's, the strips beside it are the road's (beat 2); the
            // spillway bridge is its own 12 m deck; the stairwell opens z 934..956 inside the tower (the vehicle passage has its
            // own deck)
            double shZ0 = BulwarkConst.ShellZMin, shZ1 = BulwarkConst.ShellZMax, shX0 = BulwarkConst.ShellXMin, shX1 = BulwarkConst.ShellXMax;
            CBox(deck, -5.5, -0.5, zS, 5.5, 0.0, shZ0);
            CBox(deck, -5.5, -0.5, shZ0, shX0, 0.0, shZ1);
            if (!m_off[K1]) BoxMinMax(deck[K1], shX0, -0.5, shZ0, shX1, 0.0, shZ1);
            CBox(deck, shX1, -0.5, shZ0, 5.5, 0.0, shZ1);
            CBox(deck, -5.5, -0.5, shZ1, 5.5, 0.0, sp0);
            CBox(deck, -6.0, -0.6, sp0, 6.0, 0.0, sp1);
            CBox(deck, -5.5, -0.5, sp1, 5.5, 0.0, BulwarkConst.AtriumZ1);
            CBox(deck, -5.5, -0.5, tz1, 5.5, 0.0, zEnd);
            // far gate towers (36 x 12 x 80, z -1480, 3320, 5720) as plain boxes in the walkable tower's orientation (14.12
            // "Tower 12 x 36 x 80": 12 m across the dam, x -6..6, and 36 m along it); beat 2 (14.5.2 lists them)
            if (!m_off[K2])
                foreach (double z in BulwarkConst.FarTowerZ)
                    BoxMinMax(deck[K2], -0.5 * BulwarkConst.FarTowerD, 0.0, z, 0.5 * BulwarkConst.FarTowerD, BulwarkConst.FarTowerH, z + BulwarkConst.FarTowerW);
            FlushSet(ctx, deck, "gb_road_deck", m_concrete);

            // faces (dark basalt): sea face x 5.5..6 to y -20 (open over the spillway), parapet 0.5 x 1.1 (none on the
            // spillway, restored from z 704, continued north of the tower), lee face x -6..-5.5 (below the sill on the
            // spillway); north of the spillway the lee face is beat 4's to the tower and beat 7's beyond (the face the ledge
            // looks along, with the tunnel mouth hole, GrayboxInterior)
            var faces = NewSet();
            CBox(faces, BulwarkConst.ParapetXMin, BulwarkConst.SeaFaceBatterY, zS, BulwarkConst.SeaFaceX, 0.0, sp0);
            CBox(faces, BulwarkConst.ParapetXMin, BulwarkConst.SeaFaceBatterY, sp1, BulwarkConst.SeaFaceX, 0.0, zEnd);
            CBox(faces, BulwarkConst.ParapetXMin, 0.0, zS, BulwarkConst.ParapetXMax, BulwarkConst.ParapetTop, sp0);
            CBox(faces, BulwarkConst.ParapetXMin, 0.0, sp1, BulwarkConst.ParapetXMax, BulwarkConst.ParapetTop, tz0);
            CBox(faces, BulwarkConst.ParapetXMin, 0.0, tz1, BulwarkConst.ParapetXMax, BulwarkConst.ParapetTop, zEnd);
            double lx0 = BulwarkConst.LeeFaceX, lx1 = BulwarkConst.LeeFaceX + 0.5, ly0 = -1500.0;
            CBox(faces, lx0, ly0, zS, lx1, 0.0, sp0);
            CBox(faces, lx0, ly0, sp0, lx1, BulwarkConst.SillY, sp1);
            if (!m_off[K4]) BoxMinMax(faces[K4], lx0, ly0, sp1, lx1, 0.0, tz0);
            if (!m_off[K7]) BuildLeeFaceNorth(faces[K7], lx0, lx1, ly0, tz0, zEnd);
            FlushSet(ctx, faces, "gb_faces", m_dark);

            // lee rail: two round rails (tops 1.1 and 0.55) on posts 0.12 every 2 m at x -6.0 (14.5.2), z -0.6..920 and
            // 956..1800; sea-side rails on the spillway (x 5.95); each run split at the crest bands (posts on odd z, never on
            // a band edge, so the split runs keep the 2 m pitch)
            var rails = NewSet();
            var posts = NewSet();
            float rr = LineRadius(m_rail, 0.5f * (float)BulwarkConst.LeeRailDiameter);
            ForCrestBands(zS + 0.2, tz0 - 0.1, (k, a, b) => RailRun(rails[k], posts[k], BulwarkConst.LeeRailX, a, b, rr));
            ForCrestBands(tz1 + 0.1, zEnd, (k, a, b) => RailRun(rails[k], posts[k], BulwarkConst.LeeRailX, a, b, rr));
            ForCrestBands(sp0, sp1, (k, a, b) => RailRun(rails[k], posts[k], BulwarkConst.SeaFaceX - 0.05, a, b, rr));
            FlushSet(ctx, rails, "gb_rails_crest", m_rail, false);

            // lamp posts 9 m at z 150 + 50 k (k 0-5, 12-15): lee side for even k, on the parapet x 5.75 for odd k (post length
            // 9 m from its base). The lee posts stand at x -5.85, not the spec's -5.6: a 0.2 m post at -5.6 spans -5.7..-5.5,
            // inside the capsule reach of the road lane (-5.4 - 0.35 = -5.75), so the camera would pass 0.1 m from it (decision
            // logged). Bollards 0.9 m every 20 m on the lee kerb at x -5.85 for the same reason; cart 2.8 x 1.3 x 1.9 at
            // (2.0, 0, 330), inside the lane by the spec (the one exception of the capsule clearance test)
            foreach (int k in BulwarkConst.LampPostK)
            {
                double z = BulwarkConst.LampPostZ0 + BulwarkConst.LampPostPitch * k;
                bool lee = (k & 1) == 0;
                double x = lee ? LeeKerbX : BulwarkConst.LampPostSeaX;
                double yb = lee ? 0.0 : BulwarkConst.ParapetTop, yt = yb + BulwarkConst.LampPostHeight;
                int pk = CrestKey(z);
                if (m_off[pk]) continue;
                var pm = posts[pk];
                BoxMinMax(pm, x - 0.1, yb, z - 0.1, x + 0.1, yt, z + 0.1);
                double arm = lee ? 1.4 : -1.4;
                BoxMinMax(pm, Math.Min(x, x + arm), yt - 0.15, z - 0.06, Math.Max(x, x + arm), yt - 0.03, z + 0.06);
                BoxMinMax(pm, x + arm - 0.35, yt - 0.35, z - 0.15, x + arm + 0.35, yt - 0.12, z + 0.15);
            }
            for (double z = 140.0; z <= tz0 - 10.0; z += BulwarkConst.BollardPitch)
            {
                if (z > sp0 - 1.0 && z < sp1 + 1.0) continue;
                int bk = CrestKey(z);
                if (!m_off[bk]) BoxMinMax(posts[bk], LeeKerbX - 0.1, 0.0, z - 0.1, LeeKerbX + 0.1, BulwarkConst.BollardHeight, z + 0.1);
            }
            FlushSet(ctx, posts, "gb_crest_posts", m_steel);
            // the cart (beat 2): a PathGuide obstacle, so a manual player cannot walk through it (the cinematic path passes at
            // x -1.5, 2.9 m west of it); the obstacle is registered only while the graybox cart is built
            if (m_off[K2]) return;
            var cart = new MeshBuilder();
            double cx = BulwarkConst.CartX, cz = BulwarkConst.CartZ, cw = 0.5 * BulwarkConst.CartW, cl = 0.5 * BulwarkConst.CartL;
            BoxMinMax(cart, cx - cw, 0.35, cz - cl, cx + cw, BulwarkConst.CartH, cz + cl);
            for (int i = 0; i < 4; i++)
            {
                double wx = cx + ((i & 1) == 0 ? -cw + 0.1 : cw - 0.1), wz = cz + ((i & 2) == 0 ? -cl + 0.35 : cl - 0.35);
                BoxMinMax(cart, wx - 0.08, 0.0, wz - 0.2, wx + 0.08, 0.4, wz + 0.2);
            }
            FlushKey(ctx, K2, ref cart, "gb_cart", m_ochre);
            var cb = new Bounds(new Vector3((float)cx, (float)(0.5 * BulwarkConst.CartH), (float)cz), new Vector3((float)BulwarkConst.CartW, (float)BulwarkConst.CartH, (float)BulwarkConst.CartL));
            PathGuide.AddObstacle(cb, "gb_cart");
        }

        /// <summary>x of the lee posts and bollards: 0.2 m boxes at -5.95..-5.75, just outside the road lane's capsule reach.</summary>
        public const double LeeKerbX = -5.85;

        /// <summary>Lee face north of the spillway with the tunnel mouth hole (the oblique cut of 14.5.7 spans z 1415.2-1425.6
        /// at x -6.0 and 1.54 m less at x -5.5; the tunnel walls clipped at x -6 fill its sides).</summary>
        static void BuildLeeFaceNorth(MeshBuilder mb, double lx0, double lx1, double ly0, double z0, double z1)
        {
            double holeZ0 = HoleZ0, holeZ1 = HoleZ1;
            double holeY0 = BulwarkConst.FloorYTunnel - 0.3, holeY1 = BulwarkConst.FloorYTunnel + BulwarkConst.TunnelHeight + 0.3;
            BoxMinMax(mb, lx0, ly0, z0, lx1, 0.0, holeZ0);
            BoxMinMax(mb, lx0, holeY1, holeZ0, lx1, 0.0, holeZ1);
            BoxMinMax(mb, lx0, ly0, holeZ0, lx1, holeY0, holeZ1);
            BoxMinMax(mb, lx0, ly0, holeZ1, lx1, 0.0, z1);
        }

        /// <summary>Tunnel mouth z range at x -6.0: centre crossing z 1420.4 +- half the 10.4 m oblique cut.</summary>
        const double MouthZ0 = BulwarkConst.LeeFaceCrossZ - 0.5 * BulwarkConst.TunnelMouthCut, MouthZ1 = BulwarkConst.LeeFaceCrossZ + 0.5 * BulwarkConst.TunnelMouthCut;
        /// <summary>Rectangular hole through the 0.5 m face box round the mouth (the graybox tunnel is 3.2 m wide, 14.5.7, so its
        /// walls cross x -6 near z 1415.2 / 1425.6 and 1.54 m further south at x -5.5); the outer face plane closes it.</summary>
        const double HoleZ0 = MouthZ0 - 1.6, HoleZ1 = MouthZ1 + 0.6;

        /// <summary>Two round rails (1.1 and 0.55) along z at x, posts 0.12 every 2 m (into <paramref name="posts"/>).</summary>
        static void RailRun(MeshBuilder rails, MeshBuilder posts, double x, double z0, double z1, float radius)
        {
            const double chunk = 100.0;
            for (double a = z0; a < z1 - 1e-6; a += chunk)
            {
                double b = Math.Min(z1, a + chunk);
                Tube(rails, new Vector3((float)x, (float)(BulwarkConst.LeeRailTop - radius), (float)a), new Vector3((float)x, (float)(BulwarkConst.LeeRailTop - radius), (float)b), radius);
                Tube(rails, new Vector3((float)x, (float)(BulwarkConst.LeeRailMid - radius), (float)a), new Vector3((float)x, (float)(BulwarkConst.LeeRailMid - radius), (float)b), radius);
            }
            double ps = 0.5 * BulwarkConst.LeeRailPostSize;
            for (double z = Math.Ceiling(z0 / BulwarkConst.LeeRailPostPitch) * BulwarkConst.LeeRailPostPitch + 1.0; z < z1; z += BulwarkConst.LeeRailPostPitch)
                BoxMinMax(posts, x - ps, 0.0, z - ps, x + ps, BulwarkConst.LeeRailTop - 2.0 * radius, z + ps);
        }

        // ================================================================ beat 3: spillway (graybox: bridge, box gates, fogged cone plume)
        void BuildSpillway(WorldContext ctx, Transform root)
        {
            if (m_off[K3]) return;   // the whole bridge, its water and the plume cone are beat 3's
            var conc = new MeshBuilder();
            var dark = new MeshBuilder();
            var gates = new MeshBuilder();
            var water = new MeshBuilder();
            double xe = BulwarkConst.PierEastX, lip = BulwarkConst.LipX;
            // 15 piers (4 m, z 420 + 20 k), x -6..46, sill -14 to under the deck and to +0.5 east of it; gate houses 5 x 4 x 5
            for (int k = 0; k < BulwarkConst.PierCount; k++)
            {
                double z0 = BulwarkConst.PierZ0 + BulwarkConst.PierPitch * k, z1 = z0 + BulwarkConst.PierLength;
                BoxMinMax(conc, lip, BulwarkConst.SillY, z0, xe, -0.6, z1);
                BoxMinMax(conc, BulwarkConst.SeaFaceX, -0.6, z0, xe, BulwarkConst.PierTopY, z1);
                BoxMinMax(conc, BulwarkConst.GateHouseX0, BulwarkConst.PierTopY, z0, BulwarkConst.GateHouseX1, BulwarkConst.PierTopY + BulwarkConst.GateHouseH, z1);
            }
            // 14 slots (16 m): sill floor, drawn-down water (y -8.0 at x 46 to -11.5 at the lip), box gates at x 9-12 raised
            // 5 m (bottom y -9; top at the road, assumed)
            for (int k = 0; k < BulwarkConst.SlotCount; k++)
            {
                double z0 = BulwarkConst.SlotZ0 + BulwarkConst.PierPitch * k, z1 = z0 + BulwarkConst.SlotLength;
                BoxMinMax(dark, lip, BulwarkConst.SillY - 1.0, z0, xe, BulwarkConst.SillY, z1);
                water.FlatQuad(new Vector3((float)lip, (float)BulwarkConst.LipY, (float)z0), new Vector3((float)lip, (float)BulwarkConst.LipY, (float)z1),
                               new Vector3((float)xe, (float)BulwarkConst.DrawdownY0, (float)z1), new Vector3((float)xe, (float)BulwarkConst.DrawdownY0, (float)z0));
                BoxMinMax(gates, BulwarkConst.RadialGateX0, BulwarkConst.SillY + BulwarkConst.RadialGateRaise, z0 + 0.1, BulwarkConst.RadialGateX1, -0.7, z1 - 0.1);
            }
            FlushKey(ctx, K3, ref conc, "gb_spill_piers", m_concrete);
            FlushKey(ctx, K3, ref dark, "gb_spill_sill", m_dark);
            FlushKey(ctx, K3, ref gates, "gb_spill_gates", m_steel);
            FlushKey(ctx, K3, ref water, "gb_water_spill", m_sea, false);

            // the plume (14.5.3 / 14.7 H2): mist climbing the lee face round z 562, top +260; graybox = a tilted cone whose
            // shader integrates an analytic density along the view ray (BwGrayboxMist: peak horizontal optical depth 1.0,
            // sigma 150 m along z, soft to zero at the silhouette); at y 0 the cone spans x -258..-27, west of the face
            if (m_mist != null)
            {
                var mist = new MeshBuilder();
                MeshKit.TaperedCylinder(mist, PlumeBase, PlumeApex, PlumeBaseR, PlumeApexR, 48, false);
                FlushKey(ctx, K3, ref mist, "gb_mist_plume", m_mist, false);
            }
        }

        /// <summary>Graybox plume cone (assumed): base radius 420 at (-400, -900, 562), apex radius 30 at (-70, 255, 562).</summary>
        static readonly Vector3 PlumeBase = new Vector3(-400f, -900f, (float)BulwarkConst.PlumeZ);
        static readonly Vector3 PlumeApex = new Vector3(-70f, (float)BulwarkConst.PlumeTopY - 5f, (float)BulwarkConst.PlumeZ);
        const float PlumeBaseR = 420f, PlumeApexR = 30f;

        // ================================================================ beat 4: surf strip and the gate tower (atrium = beat 5 entry)
        void BuildReefAndTower(WorldContext ctx, Transform root)
        {
            if (!m_off[K4]) BuildSurfAndIslets(ctx, root);

            // gate tower z 920..956, x -6..6, 80 m: portal x -3.3..0.3 (3.4 high) and vehicle arch x 0.8..5.5 (5.5 high) in the
            // south face, the bright north arch at z 956, atrium ceiling y 9 (z 920..934) continued over the vehicle passage
            var mb = new MeshBuilder();
            double tz0 = BulwarkConst.TowerZ0, tz1 = BulwarkConst.TowerZ1, th = BulwarkConst.TowerHeight, t = 0.5;
            double px0 = BulwarkConst.PortalXMin, px1 = BulwarkConst.PortalXMax, ph = BulwarkConst.PortalHeight;
            double ax0 = BulwarkConst.ArchXMin, ax1 = BulwarkConst.ArchXMax, ah = BulwarkConst.ArchHeight;
            double aZ = BulwarkConst.AtriumZ1, ahgt = BulwarkConst.AtriumHeight;
            // the shell (beat 4, 14.5.4 TowerBuilder) into mb, the atrium interior (beat 5) into atr
            var atr = new MeshBuilder();
            // south face
            BoxMinMax(mb, -6.0, 0.0, tz0, px0, th, tz0 + t);
            BoxMinMax(mb, px0, ph, tz0, px1, th, tz0 + t);
            BoxMinMax(mb, px1, 0.0, tz0, ax0, th, tz0 + t);
            BoxMinMax(mb, ax0, ah, tz0, ax1, th, tz0 + t);
            BoxMinMax(mb, ax1, 0.0, tz0, 6.0, th, tz0 + t);
            // side walls, roof
            BoxMinMax(mb, -6.0, 0.0, tz0 + t, -6.0 + t, th, tz1 - t);
            BoxMinMax(mb, 6.0 - t, 0.0, tz0 + t, 6.0, th, tz1 - t);
            BoxMinMax(mb, -6.0 + t, th - t, tz0 + t, 6.0 - t, th, tz1 - t);
            // north face with the bright arch
            BoxMinMax(mb, -6.0, 0.0, tz1 - t, ax0, th, tz1);
            BoxMinMax(mb, ax0, ah, tz1 - t, ax1, th, tz1);
            BoxMinMax(mb, ax1, 0.0, tz1 - t, 6.0, th, tz1);
            // atrium ceiling y 9 over the whole tower interior (z 920.5..955.5)
            BoxMinMax(atr, -6.0 + t, ahgt, tz0 + t, 6.0 - t, ahgt + 0.3, tz1 - t);
            // the stairwell opens as a WELL in the atrium floor (x -3.3..0.3 at z 934) with the atrium open above it, so the
            // KS13 line of sight from (-1.5, 1.7, 926) to the north arch (it crosses z 934 at x -0.89..0.37, y 1.25..2.71) and
            // the 14.6 look back from the first flight stay open; west of the well a wall to the ceiling; beside the well the
            // vehicle passage deck runs from the stairwell's east wall (lateral + 1.80 on the 3 deg stair heading, so x 0.30 at
            // z 934 and 0.59 at the closing line: GrayboxInterior lays it, a fixed x 0.3 edge would overhang the stairs) to
            // x 5.5, separated by the 1.1 m guard rail (14.5.5); north of the open part of the well (where the stair ceiling,
            // tread line + 2.6, drops below the deck underside) one slab closes the whole floor over the stairs. The slab is
            // axis-aligned, the stair ceiling starts on the closing knot line across the 3 deg stair heading (lateral L lies
            // L sin 3 deg = up to 0.094 m north or south of zWell, the ceiling 0.667 x 0.094 = 0.063 m lower or higher there), so
            // the slab reaches WellSkirt 0.12 m below the deck underside: no sight line enters the hollow between the slab and
            // the stair ceiling (Wave B: a 1 px bright crack at KS13, row 927 of int-03). Headroom under it 2.6 - 0.12 = 2.48
            // (14.5.5: at least 2.4)
            BoxMinMax(atr, -6.0 + t, 0.0, aZ - 0.3, BulwarkConst.StairMouthXMin, ahgt, aZ);
            double zWell = WellClosedZ();
            BoxMinMax(atr, -6.0 + t, WellDeckUnderside - WellSkirt, zWell, 6.0 - t, 0.0, tz1);
            FlushKey(ctx, K4, ref mb, "gb_tower", m_concrete);
            FlushKey(ctx, K5, ref atr, "gb_atrium", m_concrete);
            // the shell's openings light the atrium and the vehicle passage (LocalLightPool sky openings): the south portal and
            // vehicle arch (exterior south of z 920) and the bright north arch (exterior north of z 956)
            AddOpening(K4, "portal", px0, px1, 0.0, ph, tz0, false, true);
            AddOpening(K4, "arch_s", ax0, ax1, 0.0, ah, tz0, false, true);
            AddOpening(K4, "arch_n", ax0, ax1, 0.0, ah, tz1, true, true);
            if (m_off[K5]) return;   // pendants, their lamps and the guard rail are beat 5's

            // atrium: six Tungsten pendants 0.4 m at x -3 / +3, z 923 / 927 / 931, y 6.0 (rods to the ceiling); the 1.1 m guard
            // rail between the pedestrian way and the vehicle lane (x 0.55 assumed, the gap between portal and arch) to z 934,
            // then 0.25 m outside the stairwell's east wall along the open well to the closing line (it must not hang over the
            // 3 deg stairs, whose east wall passes x 0.55 at z 939.6)
            var pend = new MeshBuilder();
            var rods = new MeshBuilder();
            foreach (double pz in BulwarkConst.PendantZ)
                for (int side = -1; side <= 1; side += 2)
                {
                    var c = new Vector3((float)(side * BulwarkConst.PendantX), (float)BulwarkConst.PendantY, (float)pz);
                    MeshKit.Sphere(pend, c, 0.5f * (float)BulwarkConst.PendantSize, 16, 8);
                    BoxMinMax(rods, c.x - 0.02, c.y + 0.18, c.z - 0.02, c.x + 0.02, ahgt, c.z + 0.02);
                    AddAnchor(K5, c - new Vector3(0f, 0.4f, 0f), Pal.Tungsten, 8f, 3.0f, true);   // follows the portal lamp boost (14.2.5)
                }
            FlushKey(ctx, K5, ref pend, "gb_lamp_pendants", m_tungsten, false);
            var rails = new MeshBuilder();
            float rr = LineRadius(m_rail, 0.05f);
            double gx = 0.5 * (px1 + ax0);
            var g0 = new Vector3((float)gx, 0f, (float)(tz0 + t + 0.1));
            var g1 = new Vector3((float)gx, 0f, (float)aZ);
            Vector3 wellEnd = P(WellClosedS(), WellRailLateral(), 0.0);
            var g2 = new Vector3(wellEnd.x, 0f, (float)zWell);
            GuardRun(rails, rods, g0, g1, rr, true);
            GuardRun(rails, rods, g1, g2, rr, false);
            BoxMinMax(rods, g2.x - 0.05, 0.0, g2.z - 0.05, g2.x + 0.05, BulwarkConst.GuardRailHeight - 2.0 * rr, g2.z + 0.05);
            FlushKey(ctx, K5, ref rails, "gb_rails_atrium", m_rail, false);
            FlushKey(ctx, K5, ref rods, "gb_atrium_fittings", m_steel);
        }

        /// <summary>Guard rail run a -> b at deck level: round rails with tops 1.1 and 0.55 m, 0.1 m posts every 2 m (first post
        /// at a when <paramref name="firstPost"/>).</summary>
        static void GuardRun(MeshBuilder rails, MeshBuilder posts, Vector3 a, Vector3 b, float rr, bool firstPost)
        {
            Vector3 up = Vector3.up;
            Tube(rails, a + up * (float)(BulwarkConst.GuardRailHeight - rr), b + up * (float)(BulwarkConst.GuardRailHeight - rr), rr);
            Tube(rails, a + up * (0.55f - rr), b + up * (0.55f - rr), rr);
            float len = (b - a).magnitude;
            int n = Mathf.Max(1, Mathf.CeilToInt(len / 2f));
            for (int i = firstPost ? 0 : 1; i < n; i++)
            {
                Vector3 p = Vector3.Lerp(a, b, i / (float)n);
                BoxMinMax(posts, p.x - 0.05, 0.0, p.z - 0.05, p.x + 0.05, BulwarkConst.GuardRailHeight - 2.0 * rr, p.z + 0.05);
            }
        }

        // ================================================================ beat 4 graybox "flat surf strip with foam" and the islets
        /// <summary>Foam density across the belt (x, density): the belt runs x 6..140 (14.5.4); the parapet hides the first
        /// 45 m from the road, so the visible part is x 51.5..140 from the KS10 pose (sight line over the parapet's outer top
        /// edge (6.0, 1.1) from the eye (3.0, 1.7) meets the sea y -8 at x 3 + 9.7 / 0.2 = 51.5).</summary>
        static readonly double[] s_beltX = { BulwarkConst.BeltX0, 45.0, 100.0, BulwarkConst.BeltX1 };
        static readonly double[] s_beltD = { 1.0, 0.95, 0.7, 0.0 };
        /// <summary>Graybox islet stand-ins (assumed within 14.5.4: crest y +10..+22, radius 45-70; islet 1 at 45 m so it fits
        /// every aspect of KS10, its crest 22 = the 7.7 deg crest of KS10 at 150 m).</summary>
        static readonly double[] s_isletCrestY = { 22.0, 18.0, 14.0, 20.0, 12.0, 16.0 };
        static readonly double[] s_isletR = { 45.0, 55.0, 50.0, 60.0, 65.0, 70.0 };

        void BuildSurfAndIslets(WorldContext ctx, Transform root)
        {
            // flat surf strip over the belt (z 704..1040 and the northern reef z 1560..1800), 5 cm above the sea plane; foam
            // density in uv0.x (BwGrayboxGlow mode 4), 0 at the strip's ends over 20 m
            var surf = new MeshBuilder();
            float y = (float)(BulwarkConst.SeaY + 0.05);
            SurfStrip(surf, y, BulwarkConst.ReefZ0, BulwarkConst.ReefZ1);
            SurfStrip(surf, y, BulwarkConst.ReefNorthZ0, BulwarkConst.RoadMeshZMax);
            FlushKey(ctx, K4, ref surf, "gb_surf_strip", m_surf, false);

            // six islets: a faceted pile (frustum, 9 sides) to the crest and a foam skirt (density 1 at the waterline, 0 at
            // R + 18 m), 3 cm above the strip
            var piles = new MeshBuilder();
            var skirts = new MeshBuilder();
            for (int i = 0; i < BulwarkConst.IsletX.Length; i++)
            {
                float cx = (float)BulwarkConst.IsletX[i], cz = (float)BulwarkConst.IsletZ[i], r = (float)s_isletR[i], crest = (float)s_isletCrestY[i];
                MeshKit.TaperedCylinder(piles, new Vector3(cx, (float)BulwarkConst.SeaY - 4f, cz), new Vector3(cx, crest, cz), 0.62f * r, 0.12f * r, 9, true);
                Annulus(skirts, cx, cz, 0.55f * r, r + 18f, y + 0.03f, 48);
            }
            FlushKey(ctx, K4, ref piles, "gb_islet_piles", m_concrete);
            FlushKey(ctx, K4, ref skirts, "gb_islet_foam", m_surf, false);
        }

        /// <summary>Strip x 6..140 between z0 and z1 as a grid of up-facing quads, uv0.x = belt density x end ramps.</summary>
        static void SurfStrip(MeshBuilder mb, float y, double z0, double z1)
        {
            double[] zs = { z0, z0 + 20.0, z1 - 20.0, z1 };
            double[] dz = { 0.0, 1.0, 1.0, 0.0 };
            for (int i = 0; i + 1 < s_beltX.Length; i++)
                for (int j = 0; j + 1 < zs.Length; j++)
                {
                    float xa = (float)s_beltX[i], xb = (float)s_beltX[i + 1], za = (float)zs[j], zb = (float)zs[j + 1];
                    mb.FlatQuad(new Vector3(xa, y, za), new Vector3(xa, y, zb), new Vector3(xb, y, zb), new Vector3(xb, y, za),
                                new Vector2((float)(s_beltD[i] * dz[j]), 0f), new Vector2((float)(s_beltD[i] * dz[j + 1]), 0f),
                                new Vector2((float)(s_beltD[i + 1] * dz[j + 1]), 0f), new Vector2((float)(s_beltD[i + 1] * dz[j]), 0f));
                }
        }

        /// <summary>Up-facing annulus (inner density 1, outer 0) round (cx, cz) at height y.</summary>
        static void Annulus(MeshBuilder mb, float cx, float cz, float r0, float r1, float y, int segments)
        {
            for (int k = 0; k < segments; k++)
            {
                float a0 = 2f * Mathf.PI * k / segments, a1 = 2f * Mathf.PI * (k + 1) / segments;
                var i0 = new Vector3(cx + r0 * Mathf.Sin(a0), y, cz + r0 * Mathf.Cos(a0));
                var o0 = new Vector3(cx + r1 * Mathf.Sin(a0), y, cz + r1 * Mathf.Cos(a0));
                var o1 = new Vector3(cx + r1 * Mathf.Sin(a1), y, cz + r1 * Mathf.Cos(a1));
                var i1 = new Vector3(cx + r0 * Mathf.Sin(a1), y, cz + r0 * Mathf.Cos(a1));
                mb.FlatQuad(i0, o0, o1, i1, new Vector2(1f, 0f), new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(1f, 0f));
            }
        }

        // ================================================================ sea plane (graybox stand-in for BwSea; beat 2 "plane")
        void BuildSea(WorldContext ctx, Transform root)
        {
            if (m_off[KeySea]) return;
            // at the mean sea level east of the sea face, open over the spillway channels (x 6..46, z 420..704)
            var mb = new MeshBuilder();
            float sy = (float)BulwarkConst.SeaY, x0 = (float)BulwarkConst.SeaFaceX, xe = (float)BulwarkConst.PierEastX, x1 = 4500f;
            float za = -1500f, zb = (float)BulwarkConst.SpillwayZ0, zc = (float)BulwarkConst.SpillwayZ1, zd = 3500f;
            mb.FlatQuad(new Vector3(x0, sy, za), new Vector3(x0, sy, zb), new Vector3(x1, sy, zb), new Vector3(x1, sy, za));
            mb.FlatQuad(new Vector3(xe, sy, zb), new Vector3(xe, sy, zc), new Vector3(x1, sy, zc), new Vector3(x1, sy, zb));
            mb.FlatQuad(new Vector3(x0, sy, zc), new Vector3(x0, sy, zd), new Vector3(x1, sy, zd), new Vector3(x1, sy, zc));
            FlushKey(ctx, KeySea, ref mb, "gb_sea", m_sea, false);
        }

        // ================================================================ sky (stand-in for WorldSky / BwFarField; graybox "sky")
        /// <summary>Inward sphere of radius 9 km round the path centre (inside the 12 km far plane from every camera position),
        /// shaded by BwGrayboxSky (BwSkyColor above the visible horizon, a fogged stand-in lowland / sea below). Only built when
        /// Megastructure.Beats.WorldSky does not exist, so the two never draw together.</summary>
        void BuildSky(WorldContext ctx, Transform root)
        {
            const int lon = 64, lat = 32;
            const float r = 9000f;
            var c = new Vector3(0f, 0f, 750f);
            var mb = new MeshBuilder();
            for (int j = 0; j <= lat; j++)
            {
                float v = Mathf.PI * j / lat;
                for (int i = 0; i <= lon; i++)
                {
                    float u = 2f * Mathf.PI * i / lon;
                    var n = new Vector3(Mathf.Sin(v) * Mathf.Sin(u), Mathf.Cos(v), Mathf.Sin(v) * Mathf.Cos(u));
                    mb.Vertex(c + r * n, -n);
                }
            }
            int row = lon + 1;
            for (int j = 0; j < lat; j++)
                for (int i = 0; i < lon; i++)
                {
                    int a = j * row + i, b = a + 1, d = a + row, e = d + 1;
                    if (j > 0) mb.Tri(a, b, e);            // inward winding (seen from the centre)
                    if (j < lat - 1) mb.Tri(a, e, d);
                }
            ctx.AddMesh(Group(KeySky), "gb_sky", mb.ToMesh("gb_sky"), m_sky, castShadows: false, receiveShadows: false);
        }
    }
}
