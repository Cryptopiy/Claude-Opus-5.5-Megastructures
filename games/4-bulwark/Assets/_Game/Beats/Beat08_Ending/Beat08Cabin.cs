// Beat08Cabin: the glass express-elevator cabin, its top station at the end of the lee ledge and the two guide rails down
// the lee face (ADDENDUM A1 00 LEAD DECISION 1). Cabin interior x -12.0..-7.0 (west glass 2.80 m from the walker's spot,
// east wall 2.20 m), z 1494.4..1500.4, 3.0 m high, floor y -90 at the top; the walker stands at the end of the walked path
// (-9.199, 1497.114). West wall: floor-to-ceiling glass with three mullions; north and south: glass with 1.6 m sliding doors
// (south: boarding, north: the exit at the platform); east: a 1.1 m steel spandrel and glass above it (the face 1 m away);
// glass floor strip x -12.0..-10.6 on three joists (the look straight down 5.9 km); a 2 m tungsten ceiling lamp (a
// LocalLightPool anchor moved with the cabin); handrails 1.0 m; a carriage with guide shoes on the rails. Every cabin mesh is
// built in world coordinates at the top position under one transform that OnFrame moves by EndingPath.CabinOffset(s).
// Frame members are chamfered boxes (bevels on near geometry); the glass is BwGlass (transparent, analytic sky reflection).
using UnityEngine;

namespace Megastructure.Beats
{
    public sealed partial class Beat08Ending
    {
        /// <summary>Chamfer of the cabin's frame members: 1 mm (it was 10-12 mm). The sunlit chamfer facets of the 7-12 cm mullions
        /// were sub-pixel bright lines at the ledge (ending-23: KS20 4K 41 firefly px, KS21 7, on the mullion edges); a 1 mm facet
        /// carries a tenth of that energy, below the firefly threshold, and still breaks the hard edge in close-ups.</summary>
        const float FrameChamfer = 0.001f;
        /// <summary>Door leaf slide (m along +x when open).</summary>
        public const float DoorSlide = 1.62f;
        public const double DoorX0 = -10.0, DoorX1 = -8.4, DoorHeight = 2.3;
        public const double RailX = -6.25, RailZ0 = 1495.0, RailZ1 = 1499.8, RailRadius = 0.1, RailTopY = -82.7;

        // ================================================================ cabin
        void BuildCabin(WorldContext ctx, Transform root)
        {
            Cabin = new GameObject("b8_cabin").transform;
            Cabin.SetParent(root, false);
            double X0 = BulwarkConst.CabinX0, X1 = BulwarkConst.CabinX1, Z0 = BulwarkConst.CabinZ0, Z1 = BulwarkConst.CabinZ1;
            double F = EndingPath.FloorTopY, T = F + BulwarkConst.CabinHeight, fr = BulwarkConst.CabinFrame, fd = BulwarkConst.CabinFloorDepth;
            double gx = BulwarkConst.CabinGlassFloorX1;
            var frame = new MeshBuilder();
            var floor = new MeshBuilder();
            var glass = new MeshBuilder();
            var gfloor = new MeshBuilder();
            var lamp = new MeshBuilder();
            var rails = new MeshBuilder();

            // ---- floor: opaque part, the perimeter beams of the glass strip, three joists under the glass
            Box(floor, gx, F - fd, Z0 - fr, X1 + fr, F, Z1 + fr);   // the walker stands 0.55 m east of the glass strip
            Box(frame, X0 - fr, F - fd, Z0 - fr, gx, F, Z0);
            Box(frame, X0 - fr, F - fd, Z1, gx, F, Z1 + fr);
            Box(frame, X0 - fr, F - fd, Z0, X0, F, Z1);
            for (int k = 1; k <= 3; k++)
            {
                double z = Z0 + 1.5 * k;
                Box(frame, X0, F - 0.32, z - 0.04, gx, F - 0.03, z + 0.04);
            }
            Pane(gfloor, V(X0, F, Z0), V(X0, F, Z1), V(gx, F, Z1), V(gx, F, Z0));

            // ---- corner posts and the roof
            foreach (var c in new[] { (X0 - 0.5 * fr, Z0 - 0.5 * fr), (X0 - 0.5 * fr, Z1 + 0.5 * fr), (X1 + 0.5 * fr, Z0 - 0.5 * fr), (X1 + 0.5 * fr, Z1 + 0.5 * fr) })
                MeshKit.ChamferBox(frame, V(c.Item1, 0.5 * (F + T), c.Item2), V(fr, T - F, fr), FrameChamfer);
            // glass roof (the look up the face from the cabin): perimeter beams, a central beam along z carrying the lamp
            double rb = 0.3;
            Box(frame, X0 - fr, T, Z0 - fr, X1 + fr, T + rb, Z0);
            Box(frame, X0 - fr, T, Z1, X1 + fr, T + rb, Z1 + fr);
            Box(frame, X0 - fr, T, Z0, X0, T + rb, Z1);
            Box(frame, X1, T, Z0, X1 + fr, T + rb, Z1);
            Box(frame, -9.65, T, Z0, -9.35, T + rb, Z1);
            float ry = (float)(T + 0.15);
            Pane(glass, V(X0, ry, Z0), V(X0, ry, Z1), V(X1, ry, Z1), V(X1, ry, Z0));
            // the cabin light: two 2.6 m tungsten strips on the inner faces of the north and south head rails (out of the look
            // west in the descent, east on the shuttle and up through the roof); one real light rides with the cabin
            Box(lamp, -11.3, T - 0.24, Z1 - 0.05, -8.7, T - 0.1, Z1 - 0.002);
            Box(lamp, -11.3, T - 0.24, Z0 + 0.002, -8.7, T - 0.1, Z0 + 0.05);
            m_cabinLamp = V(-9.5, T - 0.35, 1497.4);
            if (m_pool != null)
                m_cabinLight = m_pool.Add(m_cabinLamp, BulwarkPalette.Linear(Pal.Tungsten), 6.5f, 1.6f, "beat8-cabin", 0.4f);

            // ---- west glass wall (plane x X0 - 0.06) with sill, head and three mullions
            float wx = (float)(X0 - 0.06);
            Pane(glass, V(wx, F, Z1), V(wx, T, Z1), V(wx, T, Z0), V(wx, F, Z0));
            Box(frame, X0 - fr, F, Z0, X0, F + 0.06, Z1);
            Box(frame, X0 - fr, T - 0.08, Z0, X0, T, Z1);
            for (int k = 1; k <= 3; k++) MeshKit.ChamferBox(frame, V(X0 - 0.06, 0.5 * (F + T), Z0 + 1.5 * k), V(0.12, T - F, 0.07), FrameChamfer);

            // ---- east: spandrel 1.1 m (steel) + glass above, three mullions
            Box(frame, X1, F, Z0, X1 + fr, F + 1.1, Z1);
            float ex = (float)(X1 + 0.06);
            Pane(glass, V(ex, F + 1.1, Z0), V(ex, T, Z0), V(ex, T, Z1), V(ex, F + 1.1, Z1));
            Box(frame, X1, T - 0.08, Z0, X1 + fr, T, Z1);
            for (int k = 1; k <= 3; k++) MeshKit.ChamferBox(frame, V(X1 + 0.06, 0.5 * (F + 1.1 + T), Z0 + 1.5 * k), V(0.12, T - F - 1.1, 0.07), FrameChamfer);

            // ---- north and south walls: fixed glass either side of the door and above it, door posts, sill, head
            for (int side = 0; side < 2; side++)
            {
                double zp = side == 0 ? Z1 + 0.06 : Z0 - 0.06;   // fixed glass plane
                double zf0 = side == 0 ? Z1 : Z0 - fr, zf1 = side == 0 ? Z1 + fr : Z0;
                float z = (float)zp;
                Pane(glass, V(X0, F, z), V(X0, T, z), V(DoorX0, T, z), V(DoorX0, F, z));
                Pane(glass, V(DoorX1, F, z), V(DoorX1, T, z), V(X1, T, z), V(X1, F, z));
                Pane(glass, V(DoorX0, F + DoorHeight, z), V(DoorX0, T, z), V(DoorX1, T, z), V(DoorX1, F + DoorHeight, z));
                Box(frame, X0, T - 0.08, zf0, X1, T, zf1);
                Box(frame, X0, F, zf0, DoorX0, F + 0.06, zf1);
                Box(frame, DoorX1, F, zf0, X1, F + 0.06, zf1);
                MeshKit.ChamferBox(frame, V(DoorX0 - 0.05, 0.5 * (F + T), 0.5 * (zf0 + zf1)), V(0.1, T - F, fr), FrameChamfer);
                MeshKit.ChamferBox(frame, V(DoorX1 + 0.05, 0.5 * (F + T), 0.5 * (zf0 + zf1)), V(0.1, T - F, fr), FrameChamfer);
                Box(frame, DoorX0, F + DoorHeight, zf0, DoorX1, F + DoorHeight + 0.08, zf1);
                // the sliding leaf inside the wall (plane 3 cm inside the interior), its own transform
                var leaf = new GameObject(side == 0 ? "b8_door_north" : "b8_door_south").transform;
                leaf.SetParent(Cabin, false);
                if (side == 0) m_doorN = leaf; else m_doorS = leaf;
                double zl = side == 0 ? Z1 - 0.03 : Z0 + 0.03;
                var lg = new MeshBuilder();
                var lf = new MeshBuilder();
                Pane(lg, V(DoorX0, F + 0.04, zl), V(DoorX0, F + DoorHeight - 0.02, zl), V(DoorX1, F + DoorHeight - 0.02, zl), V(DoorX1, F + 0.04, zl));
                Box(lf, DoorX0, F + 0.01, zl - 0.02, DoorX0 + 0.05, F + DoorHeight - 0.01, zl + 0.02);
                Box(lf, DoorX1 - 0.05, F + 0.01, zl - 0.02, DoorX1, F + DoorHeight - 0.01, zl + 0.02);
                Box(lf, DoorX0 + 0.05, F + 0.01, zl - 0.02, DoorX1 - 0.05, F + 0.06, zl + 0.02);
                Box(lf, DoorX0 + 0.05, F + DoorHeight - 0.06, zl - 0.02, DoorX1 - 0.05, F + DoorHeight - 0.01, zl + 0.02);
                Flush(ctx, leaf, ref lg, leaf.name + "_glass", m_glass);
                Flush(ctx, leaf, ref lf, leaf.name + "_frame", m_paint);
            }

            // ---- handrails 1.0 m, 0.1 m inside the glass (west, and north / south either side of the doors)
            float hy = (float)(F + 1.0);
            Beat07SluiceLedge.Tube(rails, V(X0 + 0.1, hy, Z0 + 0.1), V(X0 + 0.1, hy, Z1 - 0.1), 0.025f, 8);
            Beat07SluiceLedge.Tube(rails, V(X0 + 0.1, hy, Z1 - 0.1), V(DoorX0 - 0.15, hy, Z1 - 0.1), 0.025f, 8);
            Beat07SluiceLedge.Tube(rails, V(X0 + 0.1, hy, Z0 + 0.1), V(DoorX0 - 0.15, hy, Z0 + 0.1), 0.025f, 8);

            // ---- carriage on the east side: two vertical members and four guide shoes on the rails (3 cm clear)
            foreach (double rz in new[] { RailZ0, RailZ1 })
            {
                Box(frame, X1 + fr, F - fd, rz - 0.3, RailX - 0.25, T + 0.5, rz + 0.3);
                Box(frame, RailX - 0.25, F + 0.2, rz - 0.18, RailX - RailRadius - 0.03, F + 0.6, rz + 0.18);
                Box(frame, RailX - 0.25, T - 0.4, rz - 0.18, RailX - RailRadius - 0.03, T, rz + 0.18);
            }

            // ---- the bridge plate over the 0.4 m gap between the ledge end (z 1494.0) and the cabin (retracts at departure)
            m_bridge = new GameObject("b8_bridge_plate").transform;
            m_bridge.SetParent(Cabin, false);
            var br = new MeshBuilder();
            Box(br, DoorX0, F - 0.025, BulwarkConst.LedgeEndZ - 0.06, DoorX1, F + 0.006, Z0 - fr);
            Flush(ctx, m_bridge, ref br, "b8_bridge_plate", m_paint);

            Flush(ctx, Cabin, ref floor, "b8_cabin_floor", m_dark);
            Flush(ctx, Cabin, ref frame, "b8_cabin_frame", m_paint);
            Flush(ctx, Cabin, ref lamp, "b8_cabin_lamp", m_lamp);
            Flush(ctx, Cabin, ref rails, "b8_cabin_handrails", m_rail);
            Flush(ctx, Cabin, ref gfloor, "b8_cabin_glass_floor", m_glassFloor);
            Flush(ctx, Cabin, ref glass, "b8_cabin_glass", m_glass);
        }

        // ================================================================ top station (static, at the ledge end)
        void BuildTopStation(WorldContext ctx, Transform root)
        {
            var steel = new MeshBuilder();
            var conc = new MeshBuilder();
            var lamps = new MeshBuilder();
            // two cantilever beams from the face over the shaft, the canopy and the hoist house on it
            foreach (double z in new[] { 1493.75, 1501.05 })
                MeshKit.ChamferBox(steel, V(-9.3, -83.3, z), V(6.6, 0.6, 0.3), 0.02f);
            Box(steel, -12.9, -83.0, 1493.3, -6.02, -82.7, 1501.5);
            Box(conc, -11.2, -82.7, 1494.4, -6.02, -80.8, 1500.4);
            // two sodium lamp bars under the canopy at the shaft's west edge
            Box(lamps, -12.6, -83.06, 1494.0, -12.0, -83.0, 1494.12);
            Box(lamps, -12.6, -83.06, 1500.7, -12.0, -83.0, 1500.82);
            Flush(ctx, root, ref steel, "b8_top_station_steel", m_paint);
            Flush(ctx, root, ref conc, "b8_top_station_house", m_dark);
            Flush(ctx, root, ref lamps, "b8_top_station_lamps", m_sodium);
        }

        // ================================================================ guide rails (two continuous tubes down the face)
        void BuildGuideRails(WorldContext ctx, Transform root)
        {
            // continuous BwLine tubes (no fixings: at 110 m/s and 60 fps the cabin moves 1.83 m per frame, so any period under
            // 3.7 m would strobe and single brackets would flash for one frame); from the station canopy to the foot deck
            foreach (double rz in new[] { RailZ0, RailZ1 })
            {
                var mb = new MeshBuilder();
                Beat07SluiceLedge.Tube(mb, V(RailX, RailTopY, rz), V(RailX, EndingPath.FloorFootY, rz), (float)RailRadius, 8);
                Flush(ctx, root, ref mb, "b8_guide_rail_" + (rz < 1497 ? "s" : "n"), m_guide);
            }
        }
    }
}
