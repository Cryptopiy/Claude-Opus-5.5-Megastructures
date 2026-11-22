// OldQuarterBuilder: the Old Quarter and the goal of Heart Square (MASTER_PROMPT 14.4, 14.5.7 Geometry, 14.11 "the goal object",
// 14.2.3 row 7). The tenement x 1 156 - 1 170, z 379 - 397, 14.4 m (4 storeys of 3.6 m) is a brick shell whose west face is cut
// for its openings: the goal window 1.2 x 1.5 m (x 1 156, y 4.4 - 5.9, z 387.3 - 388.5) with a 0.3 m reveal and a lamp-lit room
// behind it, 18 dark windows 1.2 x 1.5 m (recessed 0.18 m) and the entrance door. HT (INT-08, Phase 4 wave A):
// - the goal window has a box-frame lining, an upper sash with a meeting rail and a glazing bar, and the lower sash raised 0.34 m
//   (the lamp is seen through the open bottom: the KS19 ray to its top crosses the frame plane at 4.70 m, under the raised
//   rail at 4.775 m); a stone sill outside; the half-drawn curtain is a pleated sheet of deep rust cloth hung from a rod (the
//   outdoor goal light shades its folds; its glow is the lamp through the cloth); the room behind (back wall, side walls,
//   ceiling, floor) glows at a lowered base and the lamp's own light (GoalLamp r 5.5 i 4) adds the falloff into the room; a
//   picture and a shelf of books hang on the back wall (the part KS19 sees, y 5.64 - 7.0);
// - the other windows are openings with brick reveals, dark glass at the back, a frame lining, a mullion and a transom, stone
//   sills and lintels (two of them glow faintly cold behind drawn curtains); the neighbours' proud panes get frames and sills;
// - the entrance: a recessed door under the entrance light, a stone step and a wall lantern for the light (LOOK_V2 6 beat 7);
// - HT-03 / HT-08 (fourth start): a cast-iron downpipe with clips, hopper head and shoe on the pier between the two south window
//   columns (the south half of the face was plain: KS19 texture region 1 meso 0.95); the lantern gets a dark cage (corner posts,
//   a mid band, a finial) around its frosted glass.
// The table lamp (GoalLamp x 3.5 sphere at its 14.5.7 height y 5.0) stands on a dresser; the real point light of 14.2.3 (r 14,
// i 5) at (1 155.6, 5.2, 387.9). The balcony: slab 2.6 x 1.4 m at y 3.4 - 3.6 (D-029, GB-18), railing 0.9 m (NocLine rods 0.02 m
// every 0.11 m and a 0.05 m top rail on the three open sides), a 0.35 m pot with a 0.6 m plant of 12 dark leaf blades.
// Neighbours 11.0 m (z 355 - 377) and 12.5 m (z 399 - 421) 1 m behind the tenement's face, a back row to x 1 202. No amber
// anywhere but the window (14.5.7). The wet floor reflects the quarter's west face (H4 wall plane, dark) and the window (H4
// source, weight 1). Every goal-window part is named goalWindow* (VisibilitySubjects: the window's and the lamp's own geometry).
// Fix wave p4_01 (HT): C18 the room x 1.7 and its light x 1.6 (aperture 165-195), the balcony slab in a CC0 concrete skin glowing
// with the window's bounce (underside 50-80); C35 the railing's side returns at a 0.33 m pitch (no hatch at 1080p); D-034 6 an
// empty lit room (table, chair pushed back, cup, a cold desk lamp on, nobody) behind the ground-floor window of column -1; FX's
// request: the ring towers east of the spire and SG's arcade face as kind-0 wall planes of the wet plaza (FX's NocWallWindows).
using System.Collections.Generic;
using UnityEngine;

namespace Megastructure
{
    public static class OldQuarterBuilder
    {
        public static readonly Vector3 Origin = new Vector3(1170f, 0f, 388f);

        static Vector3 W(double x, double y, double z) => new Vector3((float)x, (float)y, (float)z) - Origin;

        /// <summary>An axis-aligned box (world extents) with a small bevel.</summary>
        static void Box(MeshBuilder mb, double x0, double x1, double y0, double y1, double z0, double z1, float bevel = 0.003f)
        {
            double m = 0.45 * System.Math.Min(x1 - x0, System.Math.Min(y1 - y0, z1 - z0));
            MeshKit.ChamferBox(mb, W(0.5 * (x0 + x1), 0.5 * (y0 + y1), 0.5 * (z0 + z1)),
                new Vector3((float)(x1 - x0), (float)(y1 - y0), (float)(z1 - z0)), Mathf.Min(bevel, (float)m));
        }

        /// <summary>An axis-aligned box shell of flat outward quads (world extents), optional west face omitted.</summary>
        static void Shell(MeshBuilder mb, double x0, double x1, double y0, double y1, double z0, double z1, bool west)
        {
            if (west) G4Kit.OrientedQuad(mb, W(x0, y0, z0), W(x0, y1, z0), W(x0, y1, z1), W(x0, y0, z1), Vector3.left);
            G4Kit.OrientedQuad(mb, W(x1, y0, z0), W(x1, y1, z0), W(x1, y1, z1), W(x1, y0, z1), Vector3.right);
            G4Kit.OrientedQuad(mb, W(x0, y0, z0), W(x0, y1, z0), W(x1, y1, z0), W(x1, y0, z0), Vector3.back);
            G4Kit.OrientedQuad(mb, W(x0, y0, z1), W(x0, y1, z1), W(x1, y1, z1), W(x1, y0, z1), Vector3.forward);
            G4Kit.OrientedQuad(mb, W(x0, y1, z0), W(x0, y1, z1), W(x1, y1, z1), W(x1, y1, z0), Vector3.up);
        }

        // ------------------------------------------------------------------ openings of the tenement's west face
        /// <summary>An opening of the west face: kind 0 the goal window, 1 a dark window, 2 the door.</summary>
        public struct Opening
        {
            public double y0, y1, z0, z1;
            public int kind, storey, column;
        }

        /// <summary>z of the entrance (the entrance light's z: the goal window's centre + EntranceDz).</summary>
        public static double EntranceZ => HeartLayout.GoalZC + EntranceDz;

        /// <summary>The openings: windows 1.2 x 1.5 m, 0.8 m above each storey floor, columns every 3 m about the goal window, 1.5 m
        /// clear of the corners; the goal window at storey 1, column 0; the door replaces the ground-floor window it overlaps.</summary>
        public static List<Opening> Openings()
        {
            var list = new List<Opening>();
            double dz0 = EntranceZ - 0.5 * HeartLayout.DoorW, dz1 = EntranceZ + 0.5 * HeartLayout.DoorW;
            for (int k = 0; k < 4; k++)
                for (int j = -8; j <= 8; j++)
                {
                    double zc = HeartLayout.GoalZC + HeartLayout.WinPitch * j;
                    double z0 = zc - 0.5 * HeartLayout.WinW, z1 = zc + 0.5 * HeartLayout.WinW;
                    if (z0 < HeartLayout.TenZ0 + 1.5 || z1 > HeartLayout.TenZ1 - 1.5) continue;
                    if (k == 1 && j == 0)
                    {
                        list.Add(new Opening { y0 = HeartLayout.GoalY0, y1 = HeartLayout.GoalY1, z0 = HeartLayout.GoalZ0, z1 = HeartLayout.GoalZ1, kind = 0, storey = k, column = j });
                        continue;
                    }
                    if (k == 0 && z1 > dz0 - 0.3 && z0 < dz1 + 0.3) continue;   // the door
                    double y0 = HeartLayout.Storey * k + HeartLayout.WinSill;
                    list.Add(new Opening { y0 = y0, y1 = y0 + HeartLayout.WinH, z0 = z0, z1 = z1, kind = 1, storey = k, column = j });
                }
            list.Add(new Opening { y0 = 0.0, y1 = HeartLayout.DoorH, z0 = dz0, z1 = dz1, kind = 2, storey = 0, column = 99 });
            return list;
        }

        /// <summary>A west-facing face at x over y0..y1, z0..z1 with rectangular holes: the cells of the grid through every hole edge
        /// that lie outside the holes (one grid, so no T-junction).</summary>
        static void FaceWithHoles(MeshBuilder mb, double x, double y0, double y1, double z0, double z1, List<Opening> holes)
        {
            var ys = new SortedSet<double> { y0, y1 };
            var zs = new SortedSet<double> { z0, z1 };
            foreach (var h in holes) { ys.Add(System.Math.Max(y0, h.y0)); ys.Add(h.y1); zs.Add(h.z0); zs.Add(h.z1); }
            // HT-t03: the goal window's constants (387.3, 4.4) and the column / storey arithmetic (387.9 - 0.6, 3.6 + 0.8) differ
            // in the last bits: merge grid lines closer than 1 mm, or the slivers between them are degenerate triangles
            var ya = Merged(ys);
            var za = Merged(zs);
            for (int i = 0; i + 1 < ya.Count; i++)
                for (int j = 0; j + 1 < za.Count; j++)
                {
                    double yc = 0.5 * (ya[i] + ya[i + 1]), zc = 0.5 * (za[j] + za[j + 1]);
                    bool hole = false;
                    foreach (var h in holes) if (yc > h.y0 && yc < h.y1 && zc > h.z0 && zc < h.z1) { hole = true; break; }
                    if (!hole) G4Kit.OrientedQuad(mb, W(x, ya[i], za[j]), W(x, ya[i + 1], za[j]), W(x, ya[i + 1], za[j + 1]), W(x, ya[i], za[j + 1]), Vector3.left);
                }
        }

        /// <summary>Sorted grid values with every run closer than 1 mm merged into its first value.</summary>
        static List<double> Merged(SortedSet<double> values)
        {
            var l = new List<double>(values.Count);
            foreach (double v in values) if (l.Count == 0 || v - l[l.Count - 1] > 1e-3) l.Add(v);
            return l;
        }

        /// <summary>The four reveal quads of an opening, depth d behind the face at x0 (facing into the opening).</summary>
        static void Reveal(MeshBuilder mb, double x0, double d, double y0, double y1, double z0, double z1, bool bottom)
        {
            double xr = x0 + d;
            if (bottom) G4Kit.OrientedQuad(mb, W(x0, y0, z0), W(xr, y0, z0), W(xr, y0, z1), W(x0, y0, z1), Vector3.up);
            G4Kit.OrientedQuad(mb, W(x0, y1, z0), W(xr, y1, z0), W(xr, y1, z1), W(x0, y1, z1), Vector3.down);
            G4Kit.OrientedQuad(mb, W(x0, y0, z0), W(xr, y0, z0), W(xr, y1, z0), W(x0, y1, z0), Vector3.forward);
            G4Kit.OrientedQuad(mb, W(x0, y0, z1), W(xr, y0, z1), W(xr, y1, z1), W(x0, y1, z1), Vector3.back);
        }

        /// <summary>The tenement shell: the west face cut for every opening, the goal window's 0.3 m reveal, the other faces and the
        /// roof (the dark windows' and the door's reveals are TenementReveals).</summary>
        public static MeshBuilder Tenement()
        {
            var mb = new MeshBuilder();
            double x0 = HeartLayout.TenX0, y0 = HeartLayout.BuildingY0, y1 = HeartLayout.TenH, z0 = HeartLayout.TenZ0, z1 = HeartLayout.TenZ1;
            Shell(mb, x0, HeartLayout.TenX1, y0, y1, z0, z1, west: false);
            var holes = Openings();
            FaceWithHoles(mb, x0, y0, y1, z0, z1, holes);
            Reveal(mb, x0, HeartLayout.WallT, HeartLayout.GoalY0, HeartLayout.GoalY1, HeartLayout.GoalZ0, HeartLayout.GoalZ1, true);
            return mb;
        }

        /// <summary>Brick reveals of the dark windows (DarkRevealD) and of the door (DoorRevealD, no bottom: the floor).</summary>
        public static MeshBuilder TenementReveals()
        {
            var mb = new MeshBuilder();
            foreach (var o in Openings())
            {
                if (o.kind == 1) Reveal(mb, HeartLayout.TenX0, HeartLayout.DarkRevealD, o.y0, o.y1, o.z0, o.z1, true);
                if (o.kind == 2) Reveal(mb, HeartLayout.TenX0, HeartLayout.DoorRevealD, 0.0, o.y1, o.z0, o.z1, false);
            }
            return mb;
        }

        /// <summary>Glazing of the tenement's dark windows (a 2 cm pane at the back of each reveal); `glow` collects the two windows
        /// with drawn curtains lit faintly cold (storey 2 column -1, storey 3 column +1).</summary>
        public static void TenementPanes(MeshBuilder dark, MeshBuilder glow)
        {
            double xb = HeartLayout.TenX0 + HeartLayout.DarkRevealD;
            foreach (var o in Openings())
            {
                if (o.kind != 1) continue;
                if (IsEmptyRoom(o)) continue;                                   // D-034 6: open to the empty lit room behind
                bool lit = (o.storey == 2 && o.column == -1) || (o.storey == 3 && o.column == 1);
                Box(lit ? glow : dark, xb - 0.02, xb + 0.005, o.y0 - 0.005, o.y1 + 0.005, o.z0 - 0.005, o.z1 + 0.005, 0.002f);
            }
        }

        /// <summary>D-034 6: the dark window that opens onto the empty lit room.</summary>
        public static bool IsEmptyRoom(Opening o) => o.kind == 1 && o.storey == HeartLayout.EmptyStorey && o.column == HeartLayout.EmptyColumn;

        /// <summary>The empty room's opening (D-034 6).</summary>
        public static Opening EmptyRoomOpening()
        {
            foreach (var o in Openings()) if (IsEmptyRoom(o)) return o;
            return default;
        }

        /// <summary>D-034 6 (lead ruling): the empty lit room. `shell` the inward-facing room box (back wall, side walls, floor,
        /// ceiling), `wood` a table and a chair pushed back from it, half turned toward the window, `steel` the cup, the desk lamp's
        /// base, stem and shade, `bulb` the lamp's globe under its shade. Nobody.</summary>
        public static void EmptyRoom(MeshBuilder shell, MeshBuilder wood, MeshBuilder steel, MeshBuilder bulb)
        {
            double xa = HeartLayout.TenX0 + HeartLayout.DarkRevealD, xb = HeartLayout.EmptyX1, yb = HeartLayout.EmptyY1;
            double za = HeartLayout.EmptyZ0, zb = HeartLayout.EmptyZ1;
            G4Kit.OrientedQuad(shell, W(xb, 0.0, za), W(xb, yb, za), W(xb, yb, zb), W(xb, 0.0, zb), Vector3.left);
            G4Kit.OrientedQuad(shell, W(xa, 0.0, za), W(xa, yb, za), W(xb, yb, za), W(xb, 0.0, za), Vector3.forward);
            G4Kit.OrientedQuad(shell, W(xa, 0.0, zb), W(xa, yb, zb), W(xb, yb, zb), W(xb, 0.0, zb), Vector3.back);
            G4Kit.OrientedQuad(shell, W(xa, 0.002, za), W(xa, 0.002, zb), W(xb, 0.002, zb), W(xb, 0.002, za), Vector3.up);
            G4Kit.OrientedQuad(shell, W(xa, yb, za), W(xa, yb, zb), W(xb, yb, zb), W(xb, yb, za), Vector3.down);
            // the table: a 1.0 x 0.7 m top at 0.76 m on four legs, its long side along z, 1.3 m behind the window
            double tx = 1157.55, tz = 384.75, ty = 0.76;
            Box(wood, tx - 0.35, tx + 0.35, ty - 0.035, ty, tz - 0.5, tz + 0.5, 0.004f);
            for (int i = 0; i < 4; i++)
            {
                double lx = tx + ((i & 1) == 0 ? -0.3 : 0.3), lz = tz + ((i & 2) == 0 ? -0.45 : 0.45);
                Box(wood, lx - 0.025, lx + 0.025, 0.002, ty - 0.035, lz - 0.025, lz + 0.025, 0.002f);
            }
            // the chair, pushed back toward the window and turned 25 deg (someone has just got up): seat, legs, back with two rails
            Vector3 cc = W(1156.85, 0.0, 385.2);
            Quaternion q = Quaternion.Euler(0f, 25f, 0f);
            ChairPart(wood, cc, q, new Vector3(0f, 0.455f, 0f), new Vector3(0.42f, 0.03f, 0.42f));
            for (int i = 0; i < 4; i++)
                ChairPart(wood, cc, q, new Vector3((i & 1) == 0 ? -0.18f : 0.18f, 0.22f, (i & 2) == 0 ? -0.18f : 0.18f), new Vector3(0.035f, 0.44f, 0.035f));
            ChairPart(wood, cc, q, new Vector3(-0.19f, 0.71f, -0.18f), new Vector3(0.03f, 0.48f, 0.03f));
            ChairPart(wood, cc, q, new Vector3(-0.19f, 0.71f, 0.18f), new Vector3(0.03f, 0.48f, 0.03f));
            ChairPart(wood, cc, q, new Vector3(-0.19f, 0.88f, 0f), new Vector3(0.025f, 0.08f, 0.40f));
            ChairPart(wood, cc, q, new Vector3(-0.19f, 0.66f, 0f), new Vector3(0.025f, 0.05f, 0.40f));
            // a cup by the table's window edge, the desk lamp at its far end
            MeshKit.Cylinder(steel, W(tx - 0.2, ty, tz + 0.18), W(tx - 0.2, ty + 0.09, tz + 0.18), 0.04f, 12, true);
            MeshKit.Cylinder(steel, W(tx + 0.12, ty, tz - 0.32), W(tx + 0.12, ty + 0.02, tz - 0.32), 0.08f, 12, true);
            MeshKit.Cylinder(steel, W(tx + 0.12, ty + 0.02, tz - 0.32), W(tx + 0.12, ty + 0.40, tz - 0.32), 0.012f, 8, true);
            MeshKit.Cylinder(steel, W(tx + 0.12, ty + 0.38, tz - 0.32), W(tx + 0.12, ty + 0.47, tz - 0.32), 0.10f, 16, false);
            MeshKit.Sphere(bulb, W(tx + 0.12, ty + 0.37, tz - 0.32), 0.035f, 12, 6);
        }

        static void ChairPart(MeshBuilder mb, Vector3 centre, Quaternion q, Vector3 local, Vector3 size) => MeshKit.ChamferBox(mb, centre + q * local, size, 0.003f, q);

        /// <summary>The desk lamp's bulb centre (the empty room's light).</summary>
        public static Vector3 EmptyLampPos => new Vector3(1157.67f, 0.76f + 0.37f, 384.43f);

        /// <summary>Joinery of a dark window opening (x0 face): lining in the reveal at the pane, a mullion and a transom at 2/3.</summary>
        static void DarkJoinery(MeshBuilder mb, double xFace, double depth, double y0, double y1, double z0, double z1)
        {
            double fw = HeartLayout.DarkFrameW, xa = xFace + depth - 0.07, xb = xFace + depth - 0.015;
            Box(mb, xa, xb, y0, y0 + fw, z0, z1);
            Box(mb, xa, xb, y1 - fw, y1, z0, z1);
            Box(mb, xa, xb, y0 + fw, y1 - fw, z0, z0 + fw);
            Box(mb, xa, xb, y0 + fw, y1 - fw, z1 - fw, z1);
            double zc = 0.5 * (z0 + z1), yt = y0 + (y1 - y0) * 2.0 / 3.0;
            Box(mb, xa + 0.01, xb, y0 + fw, y1 - fw, zc - 0.03, zc + 0.03);
            Box(mb, xa + 0.01, xb, yt - 0.025, yt + 0.025, z0 + fw, z1 - fw);
        }

        /// <summary>Stone sill under and lintel over an opening on a face at x (west-facing).</summary>
        static void SillLintel(MeshBuilder mb, double x, double y0, double y1, double z0, double z1, bool lintel)
        {
            Box(mb, x - HeartLayout.DarkSillProud, x + 0.02, y0 - HeartLayout.DarkSillT, y0, z0 - HeartLayout.DarkSillOver, z1 + HeartLayout.DarkSillOver, 0.008f);
            if (lintel)
                Box(mb, x - HeartLayout.LintelProud, x + 0.02, y1, y1 + HeartLayout.LintelH, z0 - HeartLayout.LintelOver, z1 + HeartLayout.LintelOver, 0.005f);
        }

        /// <summary>The tenement's window joinery (dark windows), the door (into `door`, or the joinery if null) and stone (sills,
        /// lintels, the goal window's lintel, the step).</summary>
        public static void TenementTrim(MeshBuilder joinery, MeshBuilder stone, MeshBuilder door = null)
        {
            door = door ?? joinery;
            foreach (var o in Openings())
            {
                if (o.kind == 1)
                {
                    DarkJoinery(joinery, HeartLayout.TenX0, HeartLayout.DarkRevealD, o.y0, o.y1, o.z0, o.z1);
                    SillLintel(stone, HeartLayout.TenX0, o.y0, o.y1, o.z0, o.z1, true);
                }
                else if (o.kind == 0)
                    Box(stone, HeartLayout.TenX0 - HeartLayout.LintelProud, HeartLayout.TenX0 + 0.02, o.y1, o.y1 + HeartLayout.LintelH,
                        o.z0 - HeartLayout.LintelOver, o.z1 + HeartLayout.LintelOver, 0.005f);
                else
                {
                    // the door: lintel, a stone step, the leaf (stiles, a mid rail, a top rail under the transom light)
                    Box(stone, HeartLayout.TenX0 - HeartLayout.LintelProud, HeartLayout.TenX0 + 0.02, o.y1, o.y1 + HeartLayout.LintelH,
                        o.z0 - HeartLayout.LintelOver, o.z1 + HeartLayout.LintelOver, 0.005f);
                    Box(stone, HeartLayout.TenX0 - 0.32, HeartLayout.TenX0 + HeartLayout.DoorRevealD, -0.05, 0.14, o.z0 - 0.15, o.z1 + 0.15, 0.01f);
                    double xd = HeartLayout.TenX0 + HeartLayout.DoorRevealD;
                    Box(door, xd - 0.05, xd, 0.14, o.y1, o.z0, o.z1);
                    Box(door, xd - 0.07, xd - 0.05, 0.14, o.y1, o.z0, o.z0 + 0.08);
                    Box(door, xd - 0.07, xd - 0.05, 0.14, o.y1, o.z1 - 0.08, o.z1);
                    Box(door, xd - 0.07, xd - 0.05, 1.05, 1.15, o.z0 + 0.08, o.z1 - 0.08);
                    Box(door, xd - 0.07, xd - 0.05, o.y1 - 0.48, o.y1 - 0.40, o.z0 + 0.08, o.z1 - 0.08);
                    // HT-10 (fifth start): the leaf read as a flat grey slab under the lantern (HT_r01 KS19): a bottom rail, a centre
                    // muntin and four raised fielded panels (the lantern's light catches their top edges), a lever handle; the
                    // transom light above the top rail is a dark pane between glazing bars
                    double zc = 0.5 * (o.z0 + o.z1);
                    Box(door, xd - 0.07, xd - 0.05, 0.14, 0.30, o.z0 + 0.08, o.z1 - 0.08);
                    Box(door, xd - 0.07, xd - 0.05, 0.30, o.y1 - 0.48, zc - 0.04, zc + 0.04);
                    foreach (var (pa, pb) in new[] { (0.36, 0.99), (1.21, o.y1 - 0.54) })
                        foreach (var (qa, qb) in new[] { (o.z0 + 0.14, zc - 0.10), (zc + 0.10, o.z1 - 0.14) })
                        {
                            Box(door, xd - 0.068, xd - 0.05, pa, pb, qa, qb);                               // the field
                            Box(door, xd - 0.085, xd - 0.068, pa + 0.05, pb - 0.05, qa + 0.05, qb - 0.05);  // the raised centre
                        }
                    Box(door, xd - 0.16, xd - 0.07, 1.00, 1.03, o.z1 - 0.26, o.z1 - 0.12);                   // lever
                    Box(door, xd - 0.085, xd - 0.07, 0.94, 1.12, o.z1 - 0.15, o.z1 - 0.11);                  // back plate
                    Box(door, xd - 0.07, xd - 0.05, o.y1 - 0.40, o.y1, zc - 0.02, zc + 0.02);                 // transom bar
                }
            }
        }

        /// <summary>Frames and sills for the neighbours' proud panes (west faces at NbX0).</summary>
        public static void NeighbourTrim(MeshBuilder joinery, MeshBuilder stone)
        {
            void Face(double x, double z0, double z1, int storeys)
            {
                for (int k = 0; k < storeys; k++)
                {
                    double y0 = HeartLayout.Storey * k + HeartLayout.WinSill, y1 = y0 + HeartLayout.WinH;
                    for (int j = -8; j <= 8; j++)
                    {
                        double zc = HeartLayout.GoalZC + HeartLayout.WinPitch * j;
                        double wz0 = zc - 0.5 * HeartLayout.WinW, wz1 = zc + 0.5 * HeartLayout.WinW;
                        if (wz0 < z0 + 1.5 || wz1 > z1 - 1.5) continue;
                        double fw = HeartLayout.DarkFrameW, xa = x - 0.06, xb = x + 0.01;
                        Box(joinery, xa, xb, y0 - fw, y0, wz0 - fw, wz1 + fw);
                        Box(joinery, xa, xb, y1, y1 + fw, wz0 - fw, wz1 + fw);
                        Box(joinery, xa, xb, y0, y1, wz0 - fw, wz0);
                        Box(joinery, xa, xb, y0, y1, wz1, wz1 + fw);
                        Box(joinery, xa + 0.015, xb, y0, y1, zc - 0.03, zc + 0.03);
                        SillLintel(stone, x, y0 - fw, y1 + fw, wz0 - fw, wz1 + fw, false);
                    }
                }
            }
            Face(HeartLayout.NbX0, HeartLayout.NbSZ0, HeartLayout.NbSZ1, 3);
            Face(HeartLayout.NbX0, HeartLayout.NbNZ0, HeartLayout.NbNZ1, 3);
        }

        // ------------------------------------------------------------------ the goal window (INT-08)
        /// <summary>The goal window's joinery: box-frame lining, the upper sash (fixed: top rail, stiles, meeting rail, glazing bar)
        /// and the lower sash raised GoalSashRaise behind it.</summary>
        public static void GoalJoinery(MeshBuilder mb)
        {
            double x0 = HeartLayout.TenX0, wy0 = HeartLayout.GoalY0, wy1 = HeartLayout.GoalY1, wz0 = HeartLayout.GoalZ0, wz1 = HeartLayout.GoalZ1;
            double fw = HeartLayout.GoalFrameW, sw = HeartLayout.GoalSashW, la = x0 + 0.04, lb = x0 + HeartLayout.WallT;
            double sb = wy0 + 0.035;                                                     // top of the sill board
            Box(mb, la, lb, wy0, sb, wz0, wz1);                                          // sill board
            Box(mb, la, lb, wy1 - fw, wy1, wz0, wz1);                                    // head
            Box(mb, la, lb, sb, wy1 - fw, wz0, wz0 + fw);                                // jambs
            Box(mb, la, lb, sb, wy1 - fw, wz1 - fw, wz1);
            double iz0 = wz0 + fw, iz1 = wz1 - fw, ih = wy1 - fw, meet = HeartLayout.GoalMeetY;
            double ua = x0 + 0.07, ub = x0 + 0.11;                                       // upper sash plane
            Box(mb, ua, ub, ih - sw, ih, iz0, iz1);
            Box(mb, ua, ub, meet - sw, meet, iz0, iz1);
            Box(mb, ua, ub, meet, ih - sw, iz0, iz0 + sw);
            Box(mb, ua, ub, meet, ih - sw, iz1 - sw, iz1);
            Box(mb, ua + 0.008, ub - 0.008, meet, ih - sw, HeartLayout.GoalZC - 0.0125, HeartLayout.GoalZC + 0.0125);
            double va = x0 + 0.12, vb = x0 + 0.16;                                       // lower sash plane, raised
            double r0 = sb + HeartLayout.GoalSashRaise, r1 = meet + HeartLayout.GoalSashRaise;
            Box(mb, va, vb, r0, r0 + sw, iz0, iz1);
            Box(mb, va, vb, r1 - sw, r1, iz0, iz1);
            Box(mb, va, vb, r0 + sw, r1 - sw, iz0, iz0 + sw);
            Box(mb, va, vb, r0 + sw, r1 - sw, iz1 - sw, iz1);
        }

        /// <summary>The goal window's outer stone sill.</summary>
        public static void GoalSill(MeshBuilder mb)
        {
            double x0 = HeartLayout.TenX0, top = HeartLayout.GoalY0 - 0.01;
            Box(mb, x0 - HeartLayout.GoalSillProud, x0 + 0.04, top - HeartLayout.GoalSillT, top,
                HeartLayout.GoalZ0 - HeartLayout.GoalSillOver, HeartLayout.GoalZ1 + HeartLayout.GoalSillOver, 0.008f);
        }

        /// <summary>Room surfaces by lit level: 0 back wall, 1 side walls, 2 floor, 3 ceiling (inward-facing quads).</summary>
        public static MeshBuilder Room(int part)
        {
            var mb = new MeshBuilder();
            double xa = HeartLayout.TenX0 + HeartLayout.WallT, xb = HeartLayout.RoomX1;
            double ya = HeartLayout.RoomY0, yb = HeartLayout.RoomY1, za = HeartLayout.RoomZ0, zb = HeartLayout.RoomZ1;
            if (part == 0) G4Kit.OrientedQuad(mb, W(xb, ya, za), W(xb, yb, za), W(xb, yb, zb), W(xb, ya, zb), Vector3.left);
            if (part == 1)
            {
                G4Kit.OrientedQuad(mb, W(xa, ya, za), W(xa, yb, za), W(xb, yb, za), W(xb, ya, za), Vector3.forward);
                G4Kit.OrientedQuad(mb, W(xa, ya, zb), W(xa, yb, zb), W(xb, yb, zb), W(xb, ya, zb), Vector3.back);
            }
            if (part == 2) G4Kit.OrientedQuad(mb, W(xa, ya, za), W(xa, ya, zb), W(xb, ya, zb), W(xb, ya, za), Vector3.up);
            if (part == 3) G4Kit.OrientedQuad(mb, W(xa, yb, za), W(xa, yb, zb), W(xb, yb, zb), W(xb, yb, za), Vector3.down);
            return mb;
        }

        /// <summary>The back wall's furniture (dark): the picture's frame, the shelf and its books, a cornice; `canvas` the picture's
        /// dim canvas.</summary>
        public static void RoomProps(MeshBuilder dark, MeshBuilder canvas)
        {
            double xw = HeartLayout.RoomX1, b = HeartLayout.PictureBar;
            double pz0 = HeartLayout.PictureZ - 0.5 * HeartLayout.PictureW, pz1 = HeartLayout.PictureZ + 0.5 * HeartLayout.PictureW;
            double py0 = HeartLayout.PictureY - 0.5 * HeartLayout.PictureH, py1 = HeartLayout.PictureY + 0.5 * HeartLayout.PictureH;
            Box(dark, xw - 0.03, xw, py0, py0 + b, pz0, pz1);
            Box(dark, xw - 0.03, xw, py1 - b, py1, pz0, pz1);
            Box(dark, xw - 0.03, xw, py0 + b, py1 - b, pz0, pz0 + b);
            Box(dark, xw - 0.03, xw, py0 + b, py1 - b, pz1 - b, pz1);
            Box(canvas, xw - 0.012, xw, py0 + b, py1 - b, pz0 + b, pz1 - b, 0.002f);
            // shelf board on two brackets and books (heights 0.18 - 0.27 m, one leaning), a box at the end
            double sy = HeartLayout.ShelfY, sz0 = HeartLayout.ShelfZ0, sz1 = HeartLayout.ShelfZ1, sd = HeartLayout.ShelfD;
            Box(dark, xw - sd, xw, sy - 0.025, sy, sz0, sz1);
            Box(dark, xw - 0.16, xw, sy - 0.14, sy - 0.025, sz0 + 0.1, sz0 + 0.12);
            Box(dark, xw - 0.16, xw, sy - 0.14, sy - 0.025, sz1 - 0.12, sz1 - 0.1);
            double z = sz0 + 0.05;
            double[] hs = { 0.24, 0.27, 0.22, 0.25, 0.19, 0.26, 0.23, 0.21, 0.27, 0.18, 0.24 };
            double[] ws = { 0.04, 0.05, 0.035, 0.045, 0.03, 0.05, 0.04, 0.035, 0.055, 0.03, 0.045 };
            double[] ds = { 0.17, 0.19, 0.15, 0.18, 0.14, 0.2, 0.17, 0.16, 0.19, 0.14, 0.18 };
            for (int i = 0; i < hs.Length && z + ws[i] < sz1 - 0.2; i++)
            {
                if (i == 7)
                {
                    // a leaning book
                    Quaternion q = Quaternion.Euler(-18f, 0f, 0f);
                    MeshKit.ChamferBox(dark, W(xw - 0.01 - 0.5 * ds[i], sy + 0.5 * hs[i] * 0.95, z + 0.06), new Vector3((float)ds[i], (float)hs[i], (float)ws[i]), 0.003f, q);
                    z += 0.1;
                    continue;
                }
                Box(dark, xw - 0.01 - ds[i], xw - 0.01, sy, sy + hs[i], z, z + ws[i]);
                z += ws[i] + 0.004;
            }
            Box(dark, xw - 0.2, xw - 0.02, sy, sy + 0.12, sz1 - 0.19, sz1 - 0.02);
            // cornice where the back wall meets the ceiling
            Box(dark, xw - 0.06, xw, HeartLayout.RoomY1 - 0.07, HeartLayout.RoomY1, HeartLayout.RoomZ0, HeartLayout.RoomZ1, 0.004f);
        }

        /// <summary>Dresser and lamp foot (dark), the curtain rod (dark), the pleated curtain, the lamp sphere.</summary>
        public static void Interior(MeshBuilder dark, MeshBuilder curtain, MeshBuilder lamp)
        {
            double lx = HeartLayout.LampX, lz = HeartLayout.LampZ, ly = LampCentreY;
            double topY = ly - HeartLayout.LampR - 0.5;
            MeshKit.ChamferBox(dark, W(lx, 0.5 * (HeartLayout.RoomY0 + topY) - 0.01, lz), new Vector3(0.7f, (float)(topY - HeartLayout.RoomY0 + 0.02), 1.0f), 0.01f);
            MeshKit.Cylinder(dark, W(lx, topY - 0.01, lz), W(lx, ly - HeartLayout.LampR + 0.02, lz), 0.05f, 12, true);
            // the rod across the room's width at CurtainRodY, with finials
            double cx = HeartLayout.CurtainX, ry = HeartLayout.CurtainRodY;
            MeshKit.Cylinder(dark, W(cx, ry, HeartLayout.RoomZ0 + 0.15), W(cx, ry, HeartLayout.RoomZ1 - 0.15), 0.012f, 8, true);
            MeshKit.Sphere(dark, W(cx, ry, HeartLayout.RoomZ0 + 0.15), 0.025f, 8, 4);
            MeshKit.Sphere(dark, W(cx, ry, HeartLayout.RoomZ1 - 0.15), 0.025f, 8, 4);
            Curtain(curtain);
            MeshKit.Sphere(lamp, W(lx, ly, lz), (float)HeartLayout.LampR, 16, 8);
        }

        /// <summary>The pleated curtain: x = CurtainX + amp(z, y) sin(2 pi (z1 - z) / pitch), the amplitude growing from the leading edge
        /// (z1) to the gathered side and 30 % toward the hem; smooth normals from the surface's slope (facing -x).</summary>
        public static void Curtain(MeshBuilder mb)
        {
            double z0 = HeartLayout.CurtainZ0, z1 = HeartLayout.CurtainZ1, yb = HeartLayout.CurtainBottom, yt = HeartLayout.CurtainTop;
            double pitch = HeartLayout.CurtainPitch, k = 2.0 * System.Math.PI / pitch;
            double a0 = HeartLayout.CurtainAmpEdge, da = HeartLayout.CurtainAmpGather - HeartLayout.CurtainAmpEdge;
            int nz = (int)System.Math.Ceiling((z1 - z0) / pitch * 10.0), ny = 6;
            int first = mb.VertexCount;
            for (int iy = 0; iy <= ny; iy++)
            {
                double y = yb + (yt - yb) * iy / ny, hem = 1.0 + 0.3 * (1.0 - (y - yb) / (yt - yb));
                for (int iz = 0; iz <= nz; iz++)
                {
                    double z = z1 - (z1 - z0) * iz / nz, g = (z1 - z) / (z1 - z0);
                    double amp = (a0 + da * g) * hem;
                    double ph = k * (z1 - z), s = System.Math.Sin(ph), c = System.Math.Cos(ph);
                    double x = HeartLayout.CurtainX + amp * s;
                    double dxdz = -amp * k * c - da * hem * s / (z1 - z0);
                    double dxdy = -(a0 + da * g) * 0.3 / (yt - yb) * s;
                    var n = new Vector3(-1f, (float)dxdy, (float)dxdz).normalized;
                    mb.Vertex(W(x, y, z), n, new Vector2((float)z, (float)y));
                }
            }
            int row = nz + 1;
            for (int iy = 0; iy < ny; iy++)
                for (int iz = 0; iz < nz; iz++)
                {
                    int a = first + iy * row + iz;
                    Vector3 p0 = mb.Positions[a], p1 = mb.Positions[a + 1], p2 = mb.Positions[a + row];
                    if (Vector3.Dot(Vector3.Cross(p1 - p0, p2 - p0), Vector3.left) >= 0f) mb.Quad(a, a + 1, a + 1 + row, a + row);
                    else mb.Quad(a, a + row, a + 1 + row, a + 1);
                }
        }

        /// <summary>Lamp sphere centre height: 14.5.7's y 5.0 (the 0.45 m raise of Phase 3 is undone: the lowered slab clears it).</summary>
        public const double LampCentreY = HeartLayout.LampY;
        /// <summary>Balcony slab top and thickness (D-029, GB-18): y 3.4 - 3.6, so the KS19 sight line to the window's bottom edge
        /// (1.7 + 2.7 x 6.42 / 7.82 = 3.92 m at the slab's front edge) passes 0.32 m above it. HeartLayout keeps the spec's 4.2.</summary>
        public const double BalconyTop = 3.6, BalconyT = 0.2;

        public static MeshBuilder BalconySlab()
        {
            var mb = new MeshBuilder();
            double zc = HeartLayout.GoalZC;
            // 0.1 m into the wall (no face on the facade plane)
            double x0 = HeartLayout.BalX0, x1 = HeartLayout.BalX1 + 0.1;
            MeshKit.ChamferBox(mb, W(0.5 * (x0 + x1), BalconyTop - 0.5 * BalconyT, zc),
                new Vector3((float)(x1 - x0), (float)BalconyT, (float)HeartLayout.BalW), 0.02f);
            return mb;
        }

        /// <summary>Railing: rods (tubes of radius RodW / 2) and the top rail (tubes of radius RailTop / 2), three open sides.</summary>
        public static void Railing(MeshBuilder rods, MeshBuilder rails)
        {
            double xf = HeartLayout.BalX0 + HeartLayout.RailInset, xw = HeartLayout.BalX1 - 0.02;
            double za = HeartLayout.GoalZC - 0.5 * HeartLayout.BalW + HeartLayout.RailInset, zb = HeartLayout.GoalZC + 0.5 * HeartLayout.BalW - HeartLayout.RailInset;
            double yb = BalconyTop - 0.01, yt = BalconyTop + HeartLayout.RailH;
            float rr = 0.5f * (float)HeartLayout.RodW, rt = 0.5f * (float)HeartLayout.RailTop;
            var corners = new[] { new Vector2((float)xw, (float)za), new Vector2((float)xf, (float)za), new Vector2((float)xf, (float)zb), new Vector2((float)xw, (float)zb) };
            for (int side = 0; side < 3; side++)
            {
                Vector2 a = corners[side], b = corners[side + 1];
                float len = (b - a).magnitude;
                int n = Mathf.Max(1, Mathf.RoundToInt(len / (float)HeartLayout.RodStep));
                // C35: the side returns (sides 0 and 2, seen nearly edge-on from KS19) at RodStepSide, the front at RodStep
                if (side != 1) n = Mathf.Max(1, Mathf.RoundToInt(len / (float)HeartLayout.RodStepSide));
                for (int i = (side == 0 ? 0 : 1); i <= n; i++)
                {
                    Vector2 p = Vector2.Lerp(a, b, (float)i / n);
                    MeshKit.Cylinder(rods, W(p.x, yb, p.y), W(p.x, yt, p.y), rr, 6, false);
                }
                MeshKit.Cylinder(rails, W(a.x, yt, a.y), W(b.x, yt, b.y), rt, 8, false);
            }
        }

        /// <summary>The pot (dark) and the 12 leaf blades of the plant (thin boxes fanning out and up).</summary>
        public static void Plant(MeshBuilder pot, MeshBuilder leaves)
        {
            double px = HeartLayout.BalX0 + 0.35, pz = HeartLayout.GoalZC - 0.5 * HeartLayout.BalW + 0.35, py = BalconyTop;
            MeshKit.Cylinder(pot, W(px, py - 0.01, pz), W(px, py + HeartLayout.PotH, pz), (float)HeartLayout.PotR, 16, true);
            double top = py + HeartLayout.PotH - 0.03, bladeLen = HeartLayout.PlantH * 1.05;
            for (int i = 0; i < HeartLayout.PlantBlades; i++)
            {
                double az = 360.0 * i / HeartLayout.PlantBlades + (i % 2 == 0 ? 0.0 : 13.0);
                double tilt = i % 3 == 0 ? 12.0 : (i % 3 == 1 ? 24.0 : 34.0);
                Quaternion q = Quaternion.Euler(0f, (float)az, 0f) * Quaternion.Euler((float)tilt, 0f, 0f);
                Vector3 dir = q * Vector3.up;
                Vector3 c = W(px, top, pz) + dir * (float)(0.5 * bladeLen * (0.8 + 0.2 * (i % 2)));
                MeshKit.ChamferBox(leaves, c, new Vector3(0.05f, (float)(bladeLen * (0.8 + 0.2 * (i % 2))), 0.01f), 0.002f, q);
            }
        }

        /// <summary>Dark window panes on a west face x (world) for storeys rows, columns at z = 387.9 + 3 j inside z0..z1, skipping the goal
        /// (the neighbours' proud panes; the tenement's windows are openings: TenementPanes).</summary>
        public static void Windows(MeshBuilder mb, double x, double z0, double z1, int storeys)
        {
            float d = 2f * (float)HeartLayout.WinProud;
            for (int k = 0; k < storeys; k++)
            {
                double yc = HeartLayout.Storey * k + HeartLayout.WinSill + 0.5 * HeartLayout.WinH;
                for (int j = -8; j <= 8; j++)
                {
                    double zc = HeartLayout.GoalZC + HeartLayout.WinPitch * j;
                    if (zc - 0.5 * HeartLayout.WinW < z0 + 1.5 || zc + 0.5 * HeartLayout.WinW > z1 - 1.5) continue;
                    if (k == 1 && j == 0 && x == HeartLayout.TenX0) continue;   // the goal window is the opening
                    MeshKit.ChamferBox(mb, W(x, yc, zc), new Vector3(d, (float)HeartLayout.WinH, (float)HeartLayout.WinW), 0.005f);
                }
            }
        }

        public static MeshBuilder Neighbours()
        {
            var mb = new MeshBuilder();
            MeshKit.ChamferBox(mb, W(0.5 * (HeartLayout.NbX0 + HeartLayout.NbX1), 0.5 * (HeartLayout.BuildingY0 + HeartLayout.NbSH), 0.5 * (HeartLayout.NbSZ0 + HeartLayout.NbSZ1)),
                new Vector3((float)(HeartLayout.NbX1 - HeartLayout.NbX0), (float)(HeartLayout.NbSH - HeartLayout.BuildingY0), (float)(HeartLayout.NbSZ1 - HeartLayout.NbSZ0)), 0.02f);
            MeshKit.ChamferBox(mb, W(0.5 * (HeartLayout.NbX0 + HeartLayout.NbX1), 0.5 * (HeartLayout.BuildingY0 + HeartLayout.NbNH), 0.5 * (HeartLayout.NbNZ0 + HeartLayout.NbNZ1)),
                new Vector3((float)(HeartLayout.NbX1 - HeartLayout.NbX0), (float)(HeartLayout.NbNH - HeartLayout.BuildingY0), (float)(HeartLayout.NbNZ1 - HeartLayout.NbNZ0)), 0.02f);
            for (int i = 0; i < HeartLayout.BackH.Length; i++)
            {
                double z0 = HeartLayout.BackZ[i] + (i == 0 ? 0.0 : 0.5), z1 = HeartLayout.BackZ[i + 1] - (i == HeartLayout.BackH.Length - 1 ? 0.0 : 0.5);
                MeshKit.ChamferBox(mb, W(0.5 * (HeartLayout.BackX0 + HeartLayout.BackX1), 0.5 * (HeartLayout.BuildingY0 + HeartLayout.BackH[i]), 0.5 * (z0 + z1)),
                    new Vector3((float)(HeartLayout.BackX1 - HeartLayout.BackX0), (float)(HeartLayout.BackH[i] - HeartLayout.BuildingY0), (float)(z1 - z0)), 0.02f);
            }
            return mb;
        }

        /// <summary>The tenement entrance light (LOOK_V2 6 beat 7): offset from the wall, height, z offset from the goal window
        /// (distance to the window centre sqrt(0.4^2 + 2.35^2 + 4^2) = 4.66 m >= 4), range and intensity.</summary>
        public const double EntranceOff = 0.4, EntranceY = 2.8, EntranceDz = 4.0, EntranceRange = 8.0, EntranceIntensity = 1.5;

        /// <summary>The entrance lantern (the source of the entrance light's pool): a wall bracket and plate, a dark cap and base
        /// (housing), and the frosted glass body (`glass`, emissive) around the light.</summary>
        public static void Lantern(MeshBuilder housing, MeshBuilder glass)
        {
            double x = HeartLayout.TenX0 - EntranceOff, y = EntranceY, z = EntranceZ;
            Box(housing, x + 0.04, HeartLayout.TenX0 + 0.02, y + 0.17, y + 0.2, z - 0.015, z + 0.015);                   // arm
            Box(housing, HeartLayout.TenX0 - 0.02, HeartLayout.TenX0 + 0.01, y - 0.05, y + 0.26, z - 0.05, z + 0.05);      // wall plate
            Box(housing, x - 0.11, x + 0.11, y + 0.13, y + 0.17, z - 0.11, z + 0.11, 0.01f);                            // cap
            Box(housing, x - 0.09, x + 0.09, y - 0.16, y - 0.13, z - 0.09, z + 0.09, 0.008f);                            // base
            Box(glass, x - 0.08, x + 0.08, y - 0.13, y + 0.13, z - 0.08, z + 0.08, 0.004f);
            // HT-08: the cage around the glass: four corner posts, a mid band, a finial on the cap
            const double g = 0.08, pw = 0.012;
            for (int i = 0; i < 4; i++)
            {
                double sx = (i & 1) == 0 ? -g : g, sz = (i & 2) == 0 ? -g : g;
                Box(housing, x + sx - pw, x + sx + pw, y - 0.125, y + 0.125, z + sz - pw, z + sz + pw, 0.002f);
            }
            Box(housing, x - g - 0.006, x + g + 0.006, y - 0.012, y + 0.012, z - g - 0.006, z + g + 0.006, 0.002f);
            Box(housing, x - 0.02, x + 0.02, y + 0.17, y + 0.24, z - 0.02, z + 0.02, 0.004f);
        }

        /// <summary>The rainwater downpipe (HT-03; HeartLayout.Downpipe*): the pipe from the shoe to the hopper head, a shoe kicked
        /// out at the foot, the hopper head under the roof edge with its outlet into the wall, wall clips every DownpipeClipStep.</summary>
        public static void Downpipe(MeshBuilder mb)
        {
            double x = HeartLayout.TenX0 - HeartLayout.DownpipeOff, z = HeartLayout.DownpipeZ, top = HeartLayout.DownpipeTop;
            float r = (float)HeartLayout.DownpipeR;
            MeshKit.Cylinder(mb, W(x, 0.25, z), W(x, top - 0.30, z), r, 12, true);
            MeshKit.Cylinder(mb, W(x, 0.25, z), W(x - 0.12, 0.08, z), r, 12, true);
            Box(mb, x - 0.09, HeartLayout.TenX0 + 0.01, top - 0.32, top, z - 0.14, z + 0.14, 0.01f);
            for (double y = 1.2; y < top - 0.5; y += HeartLayout.DownpipeClipStep)
                Box(mb, x - r - 0.01, HeartLayout.TenX0 + 0.01, y - 0.03, y + 0.03, z - r - 0.012, z + r + 0.012, 0.004f);
        }

        /// <summary>FX's request (fix wave, INT-13 / D-032 3): the lit facades the Heart plaza mirrors, as kind-0 wall planes (FX's
        /// NocWallWindows, gain 2.5): the ring towers' inner faces east of the spire (one plane at RingInner, z 388 +- 330: KS17's
        /// floor mirrors elevations 0-15 deg, where these towers stand behind the spire) over s 1 250 - 1 515, and SG's arcade north
        /// face (z ArcZ1, x ArcX0 - ArcX1, 38 m) beside the KS17 eye until s 1 350. With the tenement's kind-1 plane (s 1 380 -
        /// 1 515) at most 3 walls are active at any s (12 m crossfades: the arcade's ends at 1 362, the tenement's starts at 1 368).</summary>
        public static void RegisterPlazaWalls()
        {
            Color ring = NocSurfaceMath.FacadeMacro(0, NocFacadeMath.RegionD, 1380.0);
            NocWetRegistry.AddWall(1250.0, 1515.0, (float)(HeartLayout.SX + HeartLayout.RingInner), (float)HeartLayout.SZ, -1f, 0f, 330f, 450f, ring, 0);
            Color arc = NocSurfaceMath.FacadeMacro(0, NocFacadeMath.RegionD, 1300.0);
            NocWetRegistry.AddWall(1250.0, 1350.0, (float)(0.5 * (SignsLayout.ArcX0 + SignsLayout.ArcX1)), (float)SignsLayout.ArcZ1, 0f, 1f,
                (float)(0.5 * (SignsLayout.ArcX1 - SignsLayout.ArcX0)), (float)SignsLayout.ArcH, arc, 0);
        }

        public static void Build(WorldContext ctx, Transform root)
        {
            var lib = ctx.Materials;
            Color warm = G4Kit.Lin(NocPal.GoalLamp);
            // Look v2 (TEXTURE_PLAN 3.5, 8.4): the tenement in Brick, the neighbours in Plaster (v1: StoneWall, Concrete)
            bool v2 = lib != null && lib.Brick != null && lib.Plaster != null;
            Material brick = lib != null ? (v2 ? lib.Brick : lib.StoneWall) : null;
            G4Kit.Add(ctx, root, "tenement", Tenement(), brick, Origin);
            G4Kit.Add(ctx, root, "tenementReveals", TenementReveals(), brick, Origin);
            G4Kit.Add(ctx, root, "neighbours", Neighbours(), lib != null ? (v2 ? lib.Plaster : lib.Concrete) : null, Origin);
            Material wood = lib != null ? (lib.Wood != null ? lib.Wood : lib.SteelPainted) : null;
            Material stone = lib != null ? lib.StoneWall : null;

            // the room: a lowered emissive base per surface (plaster glow, _EmitByAlbedo 1) and the lamp's own light for the falloff
            double[] roomGain = { HeartLayout.RoomBack, HeartLayout.RoomSide, HeartLayout.RoomFloor, HeartLayout.RoomCeil };
            for (int p = 0; p < 4; p++)
            {
                bool v2Room = lib != null && lib.Plaster != null;
                Material m = lib != null ? G4Kit.Emissive(v2Room ? lib.Plaster : lib.Concrete, warm, roomGain[p], "Beat07_Room" + p) : null;
                if (v2Room && m != null) m.SetFloat("_EmitByAlbedo", 1f);
                G4Kit.Add(ctx, root, "goalWindowRoom" + p, Room(p), m, Origin);
            }
            var props = new MeshBuilder();
            var canvas = new MeshBuilder();
            RoomProps(props, canvas);
            G4Kit.Add(ctx, root, "goalWindowProps", props, lib != null ? lib.SteelPainted : null, Origin);
            G4Kit.Add(ctx, root, "goalWindowCanvas", canvas, lib != null ? G4Kit.Emissive(lib.SteelPainted, warm, HeartLayout.RoomBack * 0.35, "Beat07_Canvas") : null, Origin);
            var dark = new MeshBuilder();
            var curtain = new MeshBuilder();
            var lamp = new MeshBuilder();
            Interior(dark, curtain, lamp);
            G4Kit.Add(ctx, root, "goalFurniture", dark, lib != null ? lib.SteelPainted : null, Origin);
            // Look v2 (TEXTURE_PLAN 3.5): an emissive Fabric curtain (the weave glows through); HT: pleated, deep rust albedo
            Material curtainSrc = lib != null && lib.Fabric != null ? lib.Fabric : (lib != null ? lib.Concrete : null);
            Material cm = lib != null ? G4Kit.Emissive(curtainSrc, warm, HeartLayout.CurtainGain, "Beat07_Curtain") : null;
            if (cm != null)
            {
                Color ca = HeartLayout.CurtainAlbedo;
                if (cm.HasProperty("_BaseColor")) cm.SetVector("_BaseColor", new Vector4(ca.r, ca.g, ca.b, 1f));
                if (cm.HasProperty("_BaseColor2")) cm.SetVector("_BaseColor2", new Vector4(ca.r, ca.g, ca.b, 1f));
            }
            G4Kit.Add(ctx, root, "goalCurtain", curtain, cm, Origin);
            // GB-18 (PA): the lamp core is GoalLamp with its blue x GoalLampCoreBlue at the same gain (hue 32.7, luma 210 at beat 7)
            Color core = new Color(warm.r, warm.g, warm.b * (float)NocturneConstants.GoalLampCoreBlue, 1f);
            G4Kit.Add(ctx, root, "goalLamp", lamp, lib != null ? G4Kit.Emissive(lib.SteelPainted, core, HeartLayout.LampGain, "Beat07_GoalLamp") : null, Origin);
            var goalJoinery = new MeshBuilder();
            GoalJoinery(goalJoinery);
            G4Kit.Add(ctx, root, "goalWindowJoinery", goalJoinery, wood, Origin);
            var goalSill = new MeshBuilder();
            GoalSill(goalSill);
            G4Kit.Add(ctx, root, "goalWindowSill", goalSill, stone, Origin);

            // C18: the slab in a CC0 concrete skin glowing with the window's warm bounce (emission follows the albedo)
            Material bal = null;
            if (lib != null)
            {
                bal = G4Kit.Emissive(lib.Concrete != null ? lib.Concrete : lib.StoneWall, warm, HeartLayout.BalconyBounce, "Beat07_BalconyBounce");
                if (bal != null && bal.HasProperty("_EmitByAlbedo")) bal.SetFloat("_EmitByAlbedo", 1f);
            }
            G4Kit.Add(ctx, root, "balcony", BalconySlab(), bal, Origin);
            var rods = new MeshBuilder();
            var rails = new MeshBuilder();
            Railing(rods, rails);
            Color metal = G4Kit.Lin(NocPal.MetalPaint);                                          // PA (LOOK_V2 10)
            G4Kit.Add(ctx, root, "railRods", rods, lib != null ? G4Kit.Line(lib.Line, HeartLayout.RodW, metal, Color.black, "Beat07_RailRod") : null, Origin);
            G4Kit.Add(ctx, root, "railTop", rails, lib != null ? G4Kit.Line(lib.Line, HeartLayout.RailTop, metal, Color.black, "Beat07_RailTop") : null, Origin);
            var pot = new MeshBuilder();
            var leaves = new MeshBuilder();
            Plant(pot, leaves);
            G4Kit.Add(ctx, root, "pot", pot, lib != null ? lib.SteelPainted : null, Origin);
            G4Kit.Add(ctx, root, "plant", leaves, lib != null ? lib.Moss : null, Origin);

            // the dark windows: the tenement's openings (glass at the back of the reveal, two with a faint cold curtain glow), the
            // neighbours' proud panes; frames, mullions, transoms, sills and lintels
            var panes = new MeshBuilder();
            var glowPanes = new MeshBuilder();
            TenementPanes(panes, glowPanes);
            Windows(panes, HeartLayout.NbX0, HeartLayout.NbSZ0, HeartLayout.NbSZ1, 3);
            Windows(panes, HeartLayout.NbX0, HeartLayout.NbNZ0, HeartLayout.NbNZ1, 3);
            Material glass = null;
            if (lib != null && lib.GlassDark != null)
                glass = lib.GlassDark;                      // Look v2 (TEXTURE_PLAN 3.5): dark panes, not textured concrete in KS19
            else if (lib != null)
            {
                glass = G4Kit.Tinted(lib.Concrete, G4Kit.Lin(NocPal.GlassDark), "Beat07_DarkGlass");   // PA (LOOK_V2 10)
                // moderately glossy: at 0.86 - 0.92 the panes beside the goal reflected the goal light as warm points (r01 KS17)
                glass.SetFloat("_SmoothMin", 0.45f);
                glass.SetFloat("_SmoothMax", 0.55f);
                glass.SetFloat("_NormalStrength", 0.1f);
                glass.SetFloat("_MacroAlbedo", 0.05f);
            }
            G4Kit.Add(ctx, root, "darkWindows", panes, glass, Origin);
            Material glowM = null;
            if (lib != null)
            {
                Material gs = lib.Fabric != null ? lib.Fabric : lib.Concrete;
                glowM = G4Kit.Emissive(gs, G4Kit.Lin(NocPal.WinCold), HeartLayout.DarkCurtainGlow, "Beat07_CurtainGlowCold");
                if (glowM != null && glowM.HasProperty("_BaseColor2")) glowM.SetVector("_BaseColor2", new Vector4(0.02f, 0.022f, 0.026f, 1f));
            }
            G4Kit.Add(ctx, root, "darkWindowsCurtained", glowPanes, glowM, Origin);
            var joinery = new MeshBuilder();
            var trim = new MeshBuilder();
            var doorLeaf = new MeshBuilder();
            TenementTrim(joinery, trim, doorLeaf);
            G4Kit.Add(ctx, root, "tenementDoor", doorLeaf, lib != null ? lib.SteelPainted : null, Origin);   // HT-10: dark paint
            NeighbourTrim(joinery, trim);
            G4Kit.Add(ctx, root, "darkWindowsJoinery", joinery, wood, Origin);
            G4Kit.Add(ctx, root, "tenementStoneTrim", trim, stone, Origin);

            G4Kit.PointLight(root, "goalLight", new Vector3((float)HeartLayout.GoalLightX, (float)HeartLayout.GoalLightY, (float)HeartLayout.GoalLightZ),
                warm, HeartLayout.GoalLightRange, HeartLayout.GoalLightIntensity);
            // INT-08 (HT): the lamp's own light in the room, a little above the globe
            G4Kit.PointLight(root, "goalRoomLight", new Vector3((float)HeartLayout.LampX, (float)(HeartLayout.LampY + 0.15), (float)HeartLayout.LampZ),
                warm, HeartLayout.RoomLightRange, HeartLayout.RoomLightIntensity);
            // LOOK_V2 6 beat 7 (PA): the tenement entrance light lerp(WinCold, WinNeutral, 0.5) r 8 i 1.5, y 2.8, 0.4 m off the wall,
            // 4 m north of the goal window (KS19's lower-left texture region: a warm window on a cool wall); HT: its lantern
            Color entrance = Color.Lerp(G4Kit.Lin(NocPal.WinCold), G4Kit.Lin(NocPal.WinNeutral), 0.5f);
            G4Kit.PointLight(root, "entranceLight", new Vector3((float)(HeartLayout.TenX0 - EntranceOff), (float)EntranceY, (float)EntranceZ),
                entrance, EntranceRange, EntranceIntensity);
            var housing = new MeshBuilder();
            var lanternGlass = new MeshBuilder();
            Lantern(housing, lanternGlass);
            G4Kit.Add(ctx, root, "entranceLantern", housing, lib != null ? lib.SteelPainted : null, Origin);
            G4Kit.Add(ctx, root, "entranceLanternGlass", lanternGlass, lib != null ? G4Kit.Emissive(lib.SteelPainted, entrance, HeartLayout.LanternGain, "Beat07_Lantern") : null, Origin);
            // D-034 6: the empty lit room (cold light; the goal window stays the only warm window)
            Color cold = G4Kit.Lin(NocPal.WinCold);
            var eShell = new MeshBuilder();
            var eWood = new MeshBuilder();
            var eSteel = new MeshBuilder();
            var eBulb = new MeshBuilder();
            EmptyRoom(eShell, eWood, eSteel, eBulb);
            Material eWall = null;
            if (lib != null)
            {
                eWall = G4Kit.Emissive(lib.Plaster != null ? lib.Plaster : lib.Concrete, cold, HeartLayout.EmptyGlow, "Beat07_EmptyRoom");
                if (eWall != null && eWall.HasProperty("_EmitByAlbedo")) eWall.SetFloat("_EmitByAlbedo", 1f);
            }
            G4Kit.Add(ctx, root, "emptyRoom", eShell, eWall, Origin);
            G4Kit.Add(ctx, root, "emptyRoomFurniture", eWood, wood, Origin);
            G4Kit.Add(ctx, root, "emptyRoomThings", eSteel, lib != null ? lib.SteelPainted : null, Origin);
            G4Kit.Add(ctx, root, "emptyRoomBulb", eBulb, lib != null ? G4Kit.Emissive(lib.SteelPainted, cold, HeartLayout.EmptyBulbGain, "Beat07_EmptyBulb") : null, Origin);
            G4Kit.PointLight(root, "emptyRoomLight", EmptyLampPos, cold, HeartLayout.EmptyLightRange, HeartLayout.EmptyLightIntensity);
            var pipe = new MeshBuilder();
            Downpipe(pipe);
            G4Kit.Add(ctx, root, "tenementDownpipe", pipe, lib != null ? lib.SteelPainted : null, Origin);
            // H4: the window as a vertical source (weight 1), the quarter's west face as a dark wall plane
            NocWetRegistry.AddSource(new Vector3((float)HeartLayout.GoalX, (float)(HeartLayout.GoalY0 + 0.1), (float)HeartLayout.GoalZC),
                new Vector3((float)HeartLayout.GoalX, (float)(HeartLayout.GoalY1 - 0.1), (float)HeartLayout.GoalZC), 0.6f, G4Kit.Scale(warm, 1.6));
            // Look v2 (-surf-v 2, TEXTURE_PLAN 2.11): the brick face's radiance under PA's rig (porosity 0.5) at the range's midpoint
            Color quarter = NocSurfaceMath.WallRadiance(NocPal.Brick, 0.5 * (1380.0 + 1515.0), -1.0, 0.0, 1.0, NocSurfaceConstants.SurfPorBrick);
            NocWetRegistry.AddWall(1380.0, 1515.0, (float)HeartLayout.TenX0, 388f, -1f, 0f, 33f, (float)HeartLayout.TenH, quarter, 1);
            // RegisterPlazaWalls() runs in ScreenWallBuilder (Beat06), before the screen wall: a kind-0 wall must be registered before
            // every wall whose activity changes during its own (here the screen wall's fade-out at s 1 277 and this tenement plane's
            // fade-in at s 1 368), or it changes slot and its mirrored windows re-roll in one frame (integration round, QA-F02)
            ctx.Log(System.FormattableString.Invariant($"Beat07 Old Quarter: tenement x {HeartLayout.TenX0}-{HeartLayout.TenX1}, z {HeartLayout.TenZ0}-{HeartLayout.TenZ1}, {HeartLayout.TenH} m; window x {HeartLayout.GoalX}, y {HeartLayout.GoalY0}-{HeartLayout.GoalY1}, z {HeartLayout.GoalZ0}-{HeartLayout.GoalZ1}; lamp y {LampCentreY}; {Openings().Count} openings, joinery {joinery.TriangleCount + goalJoinery.TriangleCount} triangles, curtain {curtain.TriangleCount}"));
        }
    }
}
