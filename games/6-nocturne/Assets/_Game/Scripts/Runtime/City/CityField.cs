// CityField: the one generator of the generic city (MASTER_PROMPT 14.7 H2 tiers, 14.4 places, 14.6). Pure C# in double, no
// UnityEngine dependency (CityTests runs it in EditMode). A tower lives in a 120 m cell (i, j) of a lattice anchored at the world
// origin: three stacked boxes with setbacks (y -3 300..ledge A..ledge B..560, every ledge above the eye so no roof is ever seen),
// a jittered footprint (34-64 m), 25 % of towers rotated 15-45 deg (yaw on the 0.5 deg face-id lattice), 6 % empty cells.
// Tiers: a 240 m cell (I, J) holds the four 120 m cells (2I..2I+1, 2J..2J+1). Its centre's distance to the ring centre
// (575, 195) decides the tier: T1 ring 1 to 1.8 km (707 cells of 120 m), T2 ring 2 to 6.5 km (~2 100 aggregates of 240 m).
// A T2 aggregate is the exact union of the four ring-1 towers of its cell (the same TowerAt call), so nothing changes at 1.8 km
// and the T1 / T2 crossover is exact (Part 10 item 34; D-018). Exclusions: every hero volume of 14.4 (alley podia corridor,
// boulevard canyon with its 150 m street continued west for the traffic lanes, the Gulf between its flaring faces plus 150 m of
// terraces, tongue, bridge, garden terrace, screen wall / promenade / arcade, the Heart plaza and ring towers within R 620).
// Beacons (AviationRed, 0.2 Hz) sit on 20 % of the towers within 4 km; 12 % of towers carry vertical edge lights.
using System;
using System.Collections.Generic;

namespace Megastructure
{
    /// <summary>One box of a tower in the tower frame: centre offset (ox, oz), half sizes (hx, hz), vertical span y0..y1 (m).</summary>
    public struct CityBox
    {
        public double ox, oz, hx, hz, y0, y1;
    }

    public sealed class CityTower
    {
        public int ci, cj;                 // 120 m cell
        public double cx, cz;              // footprint centre (world, m)
        public double yaw;                 // deg (0 = axis aligned), a multiple of 0.5
        public readonly CityBox[] boxes = new CityBox[3];
        public uint seed;
        public int edge;                   // edge-light kind 0 (none) .. 4
        public bool beacon;
        public int tier;                   // 1 or 2
        public double radius;              // bounding circle of the base box about (cx, cz)

        /// <summary>World xz of a box corner (sx, sz = +-1) of box b.</summary>
        public void Corner(int b, int sx, int sz, out double x, out double z)
        {
            CityBox q = boxes[b];
            double lx = q.ox + sx * q.hx, lz = q.oz + sz * q.hz;
            double r = yaw * (Math.PI / 180.0), c = Math.Cos(r), s = Math.Sin(r);
            x = cx + lx * c + lz * s;          // Unity yaw: local +X -> (cos, -sin), local +Z -> (sin, cos)
            z = cz - lx * s + lz * c;
        }
    }

    public static class CityField
    {
        const uint Salt = 0x43697479u;   // "City"

        /// <summary>A named exclusion rectangle (xz, m); the tower's bounding circle must stay outside.</summary>
        public struct Rect
        {
            public string name;
            public double x0, x1, z0, z1;
            public Rect(string n, double a, double b, double c, double d) { name = n; x0 = a; x1 = b; z0 = c; z1 = d; }
        }

        /// <summary>Hero volumes of 14.4 / 14.5 as rectangles (the Gulf and the Heart are analytic, below).</summary>
        public static readonly Rect[] Rects =
        {
            new Rect("alley podia corridor", -90, 90, -60, 125),            // 14.5.1: podia 22 / 31 m, shafts set back 30 / 34 m
            new Rect("boulevard canyon hero", -60, 230, 50, 340),           // 14.5.2: facade slabs z 120 / 270, x -40..215.3
            new Rect("boulevard street west", -7000, 230, 110, 280),        // the 150 m street and its traffic lanes to x -4 000
            new Rect("garden terrace", 690, 920, 100, 290),                 // 14.4: x 695..914, z 115..275 (+ the C2 turn)
            new Rect("screen wall, promenade, arcade", 820, 1000, 230, 540),// 14.4: x 834..990, z 236..520
        };

        // ------------------------------------------------------------------ the Gulf (14.4, 14.5.3): ruled faces flaring F / F2
        public static double GulfF(double d) => 700.0 * NocMath.Smoothstep(300.0, 1800.0, d);
        public static double GulfF2(double d) => 700.0 * NocMath.Smoothstep(805.0, 2305.0, d);
        public static double GulfXA(double z) => 215.3 - GulfF(Math.Abs(z - 195.0));
        public static double GulfXB(double z) => 815.3 + (z < 195.0 ? GulfF(Math.Abs(z - 195.0)) : GulfF2(Math.Abs(z - 195.0)));

        /// <summary>True when a footprint circle (x, z, r) touches a hero volume; `which` names it.</summary>
        public static bool Excluded(double x, double z, double r, out string which)
        {
            foreach (var q in Rects)
            {
                double dx = Math.Max(q.x0 - x, Math.Max(0.0, x - q.x1)), dz = Math.Max(q.z0 - z, Math.Max(0.0, z - q.z1));
                if (dx * dx + dz * dz < r * r || (x >= q.x0 && x <= q.x1 && z >= q.z0 && z <= q.z1)) { which = q.name; return true; }
            }
            // Gulf: the rift between the faces plus the terrace margin, checked over the circle's z extent
            double m = NocturneConstants.CityGulfMargin + r;
            for (int k = -2; k <= 2; k++)
            {
                double zz = z + 0.5 * k * r;
                if (x > GulfXA(zz) - m && x < GulfXB(zz) + m) { which = "Gulf (rift, faces, tongue, bridge)"; return true; }
            }
            double hx = x - NocturneConstants.SpireX, hz = z - NocturneConstants.SpireZ;
            if (hx * hx + hz * hz < Sq(NocturneConstants.CityHeartRadius + r)) { which = "Heart plaza and ring towers"; return true; }
            which = null;
            return false;
        }

        static double Sq(double v) => v * v;

        static double U(int i, int j, uint k) => NocHash.U01(NocHash.H3(unchecked((uint)(i + 1048576)), unchecked((uint)(j + 1048576)), Salt + k, 0));

        /// <summary>The tower of 120 m cell (ci, cj), or false (empty cell or excluded). Deterministic, tier-independent.</summary>
        public static bool TowerAt(int ci, int cj, out CityTower t)
        {
            t = null;
            double cell = NocturneConstants.CityCell1;
            if (U(ci, cj, 1) < NocturneConstants.CityEmptyFraction) return false;
            double hx = NocMath.Lerp(NocturneConstants.CityHalfMin, NocturneConstants.CityHalfMax, U(ci, cj, 2));
            double hz = NocMath.Lerp(NocturneConstants.CityHalfMin, NocturneConstants.CityHalfMax, U(ci, cj, 3));
            double yaw = 0.0;
            if (U(ci, cj, 4) < NocturneConstants.CityRotFraction)
            {
                double a = NocMath.Lerp(NocturneConstants.CityRotMin, NocturneConstants.CityRotMax, U(ci, cj, 5));
                a = Math.Round(a / NocturneConstants.CityYawQuantum) * NocturneConstants.CityYawQuantum;
                yaw = U(ci, cj, 6) < 0.5 ? -a : a;
            }
            double r = yaw * (Math.PI / 180.0), c = Math.Abs(Math.Cos(r)), s = Math.Abs(Math.Sin(r));
            double ex = c * hx + s * hz, ez = s * hx + c * hz;                          // AABB half extents of the base box
            double jx = Math.Max(0.0, 0.5 * cell - ex - NocturneConstants.CityCellMargin);
            double jz = Math.Max(0.0, 0.5 * cell - ez - NocturneConstants.CityCellMargin);
            double cx = (ci + 0.5) * cell + (2.0 * U(ci, cj, 7) - 1.0) * jx;
            double cz = (cj + 0.5) * cell + (2.0 * U(ci, cj, 8) - 1.0) * jz;
            double rad = Math.Sqrt(hx * hx + hz * hz);
            if (Excluded(cx, cz, rad, out _)) return false;

            t = new CityTower { ci = ci, cj = cj, cx = cx, cz = cz, yaw = yaw, radius = rad };
            t.seed = NocHash.H3(unchecked((uint)(ci + 1048576)), unchecked((uint)(cj + 1048576)), Salt, 1);
            double yA = NocMath.Lerp(NocturneConstants.CityLedgeA0, NocturneConstants.CityLedgeA1, U(ci, cj, 9));
            double yB = NocMath.Lerp(yA + NocturneConstants.CityLedgeGap, NocturneConstants.CityLedgeB1, U(ci, cj, 10));
            t.boxes[0] = new CityBox { hx = hx, hz = hz, y0 = NocturneConstants.CityShaftY0, y1 = yA };
            // setbacks: 3-9 m per box (<= 22 % of the half size), split unevenly between the two sides of each axis (>= 1 m per side)
            CityBox prev = t.boxes[0];
            for (int b = 1; b < 3; b++)
            {
                // at most 22 % of the half size per step, so a slim tower keeps >= 60 % of its footprint at the top
                double sbx = Math.Min(NocMath.Lerp(NocturneConstants.CitySetbackMin, NocturneConstants.CitySetbackMax, U(ci, cj, 11u + 4u * (uint)b)), 0.22 * prev.hx);
                double sbz = Math.Min(NocMath.Lerp(NocturneConstants.CitySetbackMin, NocturneConstants.CitySetbackMax, U(ci, cj, 12u + 4u * (uint)b)), 0.22 * prev.hz);
                double fx = NocMath.Lerp(-0.5, 0.5, U(ci, cj, 13u + 4u * (uint)b)) * (sbx - 2.0);
                double fz = NocMath.Lerp(-0.5, 0.5, U(ci, cj, 14u + 4u * (uint)b)) * (sbz - 2.0);
                var q = new CityBox { hx = prev.hx - sbx, hz = prev.hz - sbz, ox = prev.ox + fx, oz = prev.oz + fz };
                q.y0 = b == 1 ? yA : yB;
                q.y1 = b == 1 ? yB : NocturneConstants.CityShaftY1;
                t.boxes[b] = q;
                prev = q;
            }
            t.edge = U(ci, cj, 30) < NocturneConstants.FacadeEdgeFraction ? 1 + (int)Math.Min(3.0, Math.Floor(4.0 * U(ci, cj, 31))) : 0;
            double dc = Math.Sqrt(Sq(cx - NocturneConstants.CityCentreX) + Sq(cz - NocturneConstants.CityCentreZ));
            t.beacon = dc < NocturneConstants.CityBeaconRange && U(ci, cj, 32) < NocturneConstants.CityBeaconFraction;
            return true;
        }

        /// <summary>Tier of the 240 m cell (I, J) by its centre: 1 within 1.8 km, 2 within 6.5 km, else 0.</summary>
        public static int RingOf(int I, int J)
        {
            double cell = NocturneConstants.CityCell2;
            double x = (I + 0.5) * cell - NocturneConstants.CityCentreX, z = (J + 0.5) * cell - NocturneConstants.CityCentreZ;
            double d = Math.Sqrt(x * x + z * z);
            return d <= NocturneConstants.CityRing1 ? 1 : (d <= NocturneConstants.CityRing2 ? 2 : 0);
        }

        /// <summary>The towers of 240 m cell (I, J): its four 120 m cells (the T2 aggregate is exactly their union).</summary>
        public static void CellTowers(int I, int J, int tier, List<CityTower> into)
        {
            for (int a = 0; a < 2; a++)
                for (int b = 0; b < 2; b++)
                    if (TowerAt(2 * I + a, 2 * J + b, out CityTower t)) { t.tier = tier; into.Add(t); }
        }

        /// <summary>Index range of the 240 m cells that can lie within ring 2.</summary>
        public static void CellRange(out int i0, out int i1, out int j0, out int j1)
        {
            double cell = NocturneConstants.CityCell2, r = NocturneConstants.CityRing2 + cell;
            i0 = (int)Math.Floor((NocturneConstants.CityCentreX - r) / cell); i1 = (int)Math.Ceiling((NocturneConstants.CityCentreX + r) / cell);
            j0 = (int)Math.Floor((NocturneConstants.CityCentreZ - r) / cell); j1 = (int)Math.Ceiling((NocturneConstants.CityCentreZ + r) / cell);
        }

        /// <summary>Counts of the whole field: 240 m cells per ring, towers per tier (CityTests, the build log).</summary>
        public static void Count(out int cells1, out int cells2, out int towers1, out int towers2)
        {
            cells1 = cells2 = towers1 = towers2 = 0;
            CellRange(out int i0, out int i1, out int j0, out int j1);
            var tmp = new List<CityTower>(4);
            for (int I = i0; I <= i1; I++)
                for (int J = j0; J <= j1; J++)
                {
                    int ring = RingOf(I, J);
                    if (ring == 0) continue;
                    tmp.Clear();
                    CellTowers(I, J, ring, tmp);
                    if (ring == 1) { cells1++; towers1 += tmp.Count; }
                    else { if (tmp.Count > 0) cells2++; towers2 += tmp.Count; }
                }
        }

        // ------------------------------------------------------------------ faces
        /// <summary>A vertical face of a box: outward normal (nx, nz) with yaw yawN, its two edge points (world xz), y span.</summary>
        public struct Face
        {
            public double ax, az, bx, bz, nx, nz, yawN, y0, y1;
        }

        /// <summary>The four side faces of box b of tower t (outward normals, corners in world xz).</summary>
        public static void Faces(CityTower t, int b, Face[] into)
        {
            CityBox q = t.boxes[b];
            // (sx, sz) corner pairs per face, outward normal yaw = tower yaw + 0 / 90 / 180 / 270
            int[,] k = { { -1, 1, 1, 1 }, { 1, 1, 1, -1 }, { 1, -1, -1, -1 }, { -1, -1, -1, 1 } };
            for (int f = 0; f < 4; f++)
            {
                t.Corner(b, k[f, 0], k[f, 1], out double ax, out double az);
                t.Corner(b, k[f, 2], k[f, 3], out double bx, out double bz);
                double yawN = t.yaw + 90.0 * f;
                double r = yawN * (Math.PI / 180.0);
                into[f] = new Face { ax = ax, az = az, bx = bx, bz = bz, nx = Math.Sin(r), nz = Math.Cos(r), yawN = yawN, y0 = q.y0, y1 = q.y1 };
            }
        }

        /// <summary>True when some camera position of the walk region lies in front of the face (back faces for every camera
        /// are never built: an exact cull, nothing visible changes).</summary>
        public static bool FacesWalk(in Face f)
        {
            double[] xs = { NocturneConstants.CityCamX0, NocturneConstants.CityCamX1 };
            double[] zs = { NocturneConstants.CityCamZ0, NocturneConstants.CityCamZ1 };
            foreach (double x in xs)
                foreach (double z in zs)
                    if ((x - f.ax) * f.nx + (z - f.az) * f.nz > 1.0) return true;
            return false;
        }

        /// <summary>Face coordinate frame: u = dot(P.xz, t) with t the tangent (the normal turned +90 deg of yaw), plane offset
        /// = dot(P.xz, n). World-derived, so both tiers produce identical u and face ids.</summary>
        public static void FaceFrame(in Face f, out double tx, out double tz, out double uA, out double uB, out double offset)
        {
            double r = (f.yawN + 90.0) * (Math.PI / 180.0);
            tx = Math.Sin(r); tz = Math.Cos(r);
            uA = f.ax * tx + f.az * tz;
            uB = f.bx * tx + f.bz * tz;
            offset = f.ax * f.nx + f.az * f.nz;
        }
    }
}
