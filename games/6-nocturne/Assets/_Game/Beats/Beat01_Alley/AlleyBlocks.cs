// AlleyBlocks: the masses of beat 1 (MASTER_PROMPT 14.5.1, AlleyBuilder graybox-plus; dimensions in AlleyLayout):
//   * the two alley walls: vertical strips at x = -+HalfWidth(z) sampled every 0.5 m from the dead-end (z -2.5) to the mouth
//     (z 120), y -1..22 (west) / -1..31 (east), Concrete; openings for the noodle bar (z 43..47, y 0..2.3) and the vending niche
//     (z 61.9..67.1, y 0..2.4) and 18 side doors 1.0 x 2.1 m, whose interiors NoodleBar / VendingMachines / AlleyDoors build;
//   * the podium roofs (y 22 / 31, the closed tops of both blocks; invisible from the walk, measured by the tests);
//   * the dead-end block (x -2.6..2.6, z -60..-2.5, 26 m) with the 0.9 x 2.0 m steel door recessed 8 cm, frame and reveals;
//   * the two towers above the podia (NocFacade, FacadeKit): west x -90..-30, z -60..116 from y 22; east x 34..90, z -60..86 from
//     y 31 (plus a 1 m closer at x 33..34 so the back of the boulevard slot 33..39 is closed), all to y 560 (the cloud mix).
// The street faces of both blocks (z 120: podium front, arcade, piers at the mouth) belong to the boulevard facade (StreetFacade).
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Megastructure
{
    public static class AlleyBlocks
    {
        /// <summary>Seeds of the tower faces (window field, edge lights).</summary>
        public const uint WestSeed = 0xFFFFFFF2u /* NocFacadeMath.SkinOverride(OverrideAlleyWest): S1 precast (FA) */, EastSeed = 0xFFFFFFF3u /* SkinOverride(OverrideAlleyEast): S4 tiled with loggias (FA) */;

        /// <summary>The z samples of the wall strips: every ProfileStep metres plus every opening edge (exact openings).</summary>
        public static List<double> WallSamples()
        {
            var zs = new List<double>(260);
            for (double z = AlleyLayout.DeadEndZ; z < AlleyLayout.MouthZ - 1e-9; z += AlleyLayout.ProfileStep) zs.Add(z);
            zs.Add(AlleyLayout.MouthZ);
            foreach (double e in new[] { AlleyLayout.CounterZ0, AlleyLayout.CounterZ1, VendingMachines.NicheZ0, VendingMachines.NicheZ1 })
                if (!zs.Contains(e)) zs.Add(e);
            foreach (double[] doors in new[] { AlleyLayout.DoorZWest, AlleyLayout.DoorZEast })
                foreach (double d in doors)
                    foreach (double e in new[] { d - 0.5 * AlleyLayout.SideDoorW, d + 0.5 * AlleyLayout.SideDoorW })
                        if (!zs.Contains(e)) zs.Add(e);
            zs.Sort();
            return zs;
        }

        public static void Build(G1Parts parts)
        {
            var zs = WallSamples();
            var origin = new Vector3(0f, 0f, 60f);
            var west = parts.Get("alleyWallW", G1Mat.ConcreteAlley, origin);     // Look v2: ConcreteAlley (v1: Concrete)
            var east = parts.Get("alleyWallE", G1Mat.ConcreteAlley, origin);
            for (int i = 0; i + 1 < zs.Count; i++)
            {
                double za = zs[i], zb = zs[i + 1], zm = 0.5 * (za + zb);
                float xa = (float)AlleyLayout.HalfWidth(za), xb = (float)AlleyLayout.HalfWidth(zb);
                // west wall (faces +x): opening for the bar
                bool bar = zm > AlleyLayout.CounterZ0 && zm < AlleyLayout.CounterZ1;
                bool doorW = AlleyDoors.InDoor(AlleyLayout.DoorZWest, zm);
                WallStrip(west, -xa, -xb, (float)za, (float)zb, (float)AlleyLayout.WallBottom, (float)AlleyLayout.WestPodiumH,
                    bar || doorW ? 0f : float.NaN, bar ? (float)AlleyLayout.WindowY1 : (float)AlleyLayout.SideDoorH, Vector3.right);
                // east wall (faces -x): opening for the vending niche
                bool niche = zm > VendingMachines.NicheZ0 && zm < VendingMachines.NicheZ1;
                bool doorE = AlleyDoors.InDoor(AlleyLayout.DoorZEast, zm);
                WallStrip(east, xa, xb, (float)za, (float)zb, (float)AlleyLayout.WallBottom, (float)AlleyLayout.EastPodiumH,
                    niche || doorE ? 0f : float.NaN, niche ? (float)VendingMachines.NicheTop : (float)AlleyLayout.SideDoorH, Vector3.left);
            }
            Roofs(parts, zs);
            DeadEnd(parts);
            Towers(parts);
        }

        /// <summary>A wall strip between z a and b at x xa / xb from y0 to y1; with an opening (open0 = its bottom, not NaN) the
        /// strip is split into y0..open0 (hidden in the floor below y 0) and open1..y1.</summary>
        static void WallStrip(MeshBuilder mb, float xa, float xb, float za, float zb, float y0, float y1, float open0, float open1, Vector3 want)
        {
            if (float.IsNaN(open0))
            {
                G1Mesh.Quad(mb, new Vector3(xa, y0, za), new Vector3(xa, y1, za), new Vector3(xb, y1, zb), new Vector3(xb, y0, zb), want);
                return;
            }
            if (open0 > y0) G1Mesh.Quad(mb, new Vector3(xa, y0, za), new Vector3(xa, open0, za), new Vector3(xb, open0, zb), new Vector3(xb, y0, zb), want);
            G1Mesh.Quad(mb, new Vector3(xa, open1, za), new Vector3(xa, y1, za), new Vector3(xb, y1, zb), new Vector3(xb, open1, zb), want);
        }

        /// <summary>Podium roofs: west y 22 from the wall to x -90, east y 31 from the wall to x 90 (x 33 north of z 86); the
        /// strips south of the dead-end face run to the dead-end block's sides.</summary>
        static void Roofs(G1Parts parts, List<double> zs)
        {
            var origin = new Vector3(0f, 0f, 30f);
            var mb = parts.Get("alleyRoofs", G1Mat.Concrete, origin);
            float yw = (float)AlleyLayout.WestPodiumH, ye = (float)AlleyLayout.EastPodiumH;
            float bx0 = (float)AlleyLayout.BlockX0, bx1 = (float)AlleyLayout.BlockX1;
            for (int i = 0; i + 1 < zs.Count; i++)
            {
                float za = (float)zs[i], zb = (float)zs[i + 1];
                float xa = (float)AlleyLayout.HalfWidth(za), xb = (float)AlleyLayout.HalfWidth(zb);
                G1Mesh.Quad(mb, new Vector3(bx0, yw, za), new Vector3(-xa, yw, za), new Vector3(-xb, yw, zb), new Vector3(bx0, yw, zb), Vector3.up);
                float ex = 0.5f * (za + zb) < AlleyLayout.EastBlockNarrowZ ? bx1 : (float)AlleyLayout.EastBlockNarrowX;
                G1Mesh.Quad(mb, new Vector3(xa, ye, za), new Vector3(ex, ye, za), new Vector3(ex, ye, zb), new Vector3(xb, ye, zb), Vector3.up);
            }
            float zd = (float)AlleyLayout.DeadEndZ, z0 = (float)AlleyLayout.BlockZ0, h = (float)AlleyLayout.DeadEndHalf;
            G1Mesh.Box(mb, new Vector3(bx0, 0f, z0), new Vector3(-h, yw, zd), G1Mesh.PY);
            G1Mesh.Box(mb, new Vector3(h, 0f, z0), new Vector3(bx1, ye, zd), G1Mesh.PY);
            G1Mesh.Box(mb, new Vector3(-h, 0f, z0), new Vector3(h, (float)AlleyLayout.DeadEndH, zd), G1Mesh.PY);
        }

        /// <summary>The dead-end face at z -2.5 with the recessed steel door (0.9 x 2.0 m), its reveals and frame.</summary>
        static void DeadEnd(G1Parts parts)
        {
            var origin = new Vector3(0f, 0f, -2.5f);
            var wall = parts.Get("deadEnd", G1Mat.ConcreteAlley, origin);
            float z = (float)AlleyLayout.DeadEndZ, hw = (float)AlleyLayout.HalfWidth(0.0), h22 = (float)AlleyLayout.WestPodiumH;
            float top = (float)AlleyLayout.DeadEndH, yb = (float)AlleyLayout.WallBottom;
            float dw = 0.5f * (float)AlleyLayout.DoorW, dh = (float)AlleyLayout.DoorH, rec = (float)AlleyLayout.DoorRecess;
            Vector3 f = Vector3.forward;
            // around the opening: left, right, above (the face spans the alley width below the west podium top)
            G1Mesh.Quad(wall, new Vector3(-hw, yb, z), new Vector3(-hw, h22, z), new Vector3(-dw, h22, z), new Vector3(-dw, yb, z), f);
            G1Mesh.Quad(wall, new Vector3(dw, yb, z), new Vector3(dw, h22, z), new Vector3(hw, h22, z), new Vector3(hw, yb, z), f);
            G1Mesh.Quad(wall, new Vector3(-dw, dh, z), new Vector3(-dw, h22, z), new Vector3(dw, h22, z), new Vector3(dw, dh, z), f);
            G1Mesh.Quad(wall, new Vector3(-dw, yb, z), new Vector3(-dw, 0f, z), new Vector3(dw, 0f, z), new Vector3(dw, yb, z), f);
            // above the west podium the block shows over the roof edge from x -2.6
            G1Mesh.Quad(wall, new Vector3(-(float)AlleyLayout.DeadEndHalf, h22, z), new Vector3(-(float)AlleyLayout.DeadEndHalf, top, z),
                new Vector3(hw, top, z), new Vector3(hw, h22, z), f);
            // reveals (jambs face inward, lintel faces down)
            G1Mesh.Quad(wall, new Vector3(-dw, 0f, z), new Vector3(-dw, dh, z), new Vector3(-dw, dh, z - rec), new Vector3(-dw, 0f, z - rec), Vector3.right);
            G1Mesh.Quad(wall, new Vector3(dw, 0f, z), new Vector3(dw, dh, z), new Vector3(dw, dh, z - rec), new Vector3(dw, 0f, z - rec), Vector3.left);
            G1Mesh.Quad(wall, new Vector3(-dw, dh, z), new Vector3(dw, dh, z), new Vector3(dw, dh, z - rec), new Vector3(-dw, dh, z - rec), Vector3.down);
            // the door leaf and its frame (steel), the frame 2 cm proud of the wall
            var steel = parts.Get("deadEndDoor", G1Mat.SteelPainted, origin);
            G1Mesh.Quad(steel, new Vector3(-dw, 0f, z - rec), new Vector3(-dw, dh, z - rec), new Vector3(dw, dh, z - rec), new Vector3(dw, 0f, z - rec), f);
            float fr = (float)AlleyLayout.DoorFrame;
            G1Mesh.Chamfer(steel, new Vector3(-dw - fr, 0f, z - 0.01f), new Vector3(-dw, dh + fr, z + 0.02f), 0.01f);
            G1Mesh.Chamfer(steel, new Vector3(dw, 0f, z - 0.01f), new Vector3(dw + fr, dh + fr, z + 0.02f), 0.01f);
            G1Mesh.Chamfer(steel, new Vector3(-dw, dh, z - 0.01f), new Vector3(dw, dh + fr, z + 0.02f), 0.01f);
            // handle
            G1Mesh.Chamfer(steel, new Vector3(dw - 0.16f, 0.98f, z - rec), new Vector3(dw - 0.06f, 1.04f, z - rec + 0.06f), 0.008f);
        }

        /// <summary>The towers above the podia (NocFacade): the KS02 slit of towers and the boulevard's south wall west of the mouth.</summary>
        static void Towers(G1Parts parts)
        {
            var w = parts.Get("towerW", G1Mat.Facade, new Vector3(-60f, 0f, 30f));
            FacadeKit.Box(w, AlleyLayout.BlockX0, AlleyLayout.WestTowerX, AlleyLayout.BlockZ0, AlleyLayout.WestTowerZ1,
                AlleyLayout.WestPodiumH, AlleyLayout.TowerTop, G1Mesh.Sides, WestSeed, FacadeKit.EdgeNone);
            var e = parts.Get("towerE", G1Mat.Facade, new Vector3(62f, 0f, 20f));
            FacadeKit.Box(e, AlleyLayout.EastTowerX, AlleyLayout.BlockX1, AlleyLayout.BlockZ0, AlleyLayout.EastTowerZ1,
                AlleyLayout.EastPodiumH, AlleyLayout.TowerTop, G1Mesh.PX | G1Mesh.NX | G1Mesh.NZ, EastSeed, FacadeKit.EdgeCyan);
            // north face of the east tower above the boulevard podium (y 40): only where no south slab covers it (BoulevardLayout)
            double y40 = BoulevardLayout.PodiumH;
            foreach (var seg in BoulevardLayout.SouthSlabGaps(AlleyLayout.EastBlockNarrowX, AlleyLayout.BlockX1))
                FacadeKit.Face(e, seg.x, AlleyLayout.EastTowerZ1, seg.y, AlleyLayout.EastTowerZ1, y40, AlleyLayout.TowerTop, 0, 1, EastSeed, FacadeKit.EdgeNone);
            // the 1 m closer x 33..34 (its west face toward the alley, its own north face is part of the gap list above)
            FacadeKit.Face(e, AlleyLayout.EastBlockNarrowX, AlleyLayout.EastTowerZ1 - 1.0, AlleyLayout.EastBlockNarrowX, AlleyLayout.EastTowerZ1,
                y40, AlleyLayout.TowerTop, -1, 0, EastSeed, FacadeKit.EdgeNone);
            FacadeKit.Face(e, AlleyLayout.EastTowerX, AlleyLayout.EastTowerZ1 - 1.0, AlleyLayout.EastBlockNarrowX, AlleyLayout.EastTowerZ1 - 1.0,
                y40, AlleyLayout.TowerTop, 0, -1, EastSeed, FacadeKit.EdgeNone);
        }
    }
}
