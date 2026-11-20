// BoulevardRelief: T0 relief on the boulevard's hero slabs (TEXTURE_PLAN 5 item 1; CRIT0 #4 "the towers read as flat walls"): the
// street faces of G1's slabs (StreetFacade: south slabs 2-6 at z 116, north slabs -1..6 at z 274, seeds SeedSouth / SeedNorth +
// k + 1) carry real geometry where FA's skin puts it, within 150 m of the walk (y 40..160 above the 40 m podium, 79-170 m away):
//   * S4 (render with loggias): every residential window cell gets a balcony: the cell's slab band (y 0..0.25 of the cell) carried
//     0.9 m out over the opening width + 0.2 m each side, a 0.9 m rail panel and a top rail on its edge;
//   * S0 (curtain wall): vertical fins on every cell edge of the residential and commercial bands' window rows, 0.12 m wide
//     (FaS0Mull, on the mullion line) and 0.45 m deep;
//   * S1 / S2 / S3: an AC unit (0.9 x 0.6 x 0.45 m) under one window in eight (hash), centred on the opening, below its sill.
// Every element lies on FA's cell API (NocFacadeMath: SkinByte / SkinOf / StratumV2 / CellOf / Bays / Rows with the face's own u
// range and y0 / y1, exactly as FacadeKit writes them): whole bays only, the core strip and the mechanical stripes skipped
// (ReliefTests: on cell boundaries within 1 cm). Built with the v2 facade only (-relief-v, default = -facade-v): with the v1
// shader the window field is v1's, and the default picture stays unchanged until FA's package is accepted (D-030).
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Megastructure
{
    public static class BoulevardRelief
    {
        public const double YMin = BoulevardLayout.PodiumH, YMax = 160.0;
        public const double BalDepth = 0.9, BalSide = 0.2, RailH = 0.9, RailT = 0.05, TopRail = 0.06;
        public const double FinDepth = 0.45, AcW = 0.9, AcH = 0.6, AcD = 0.45;
        /// <summary>Beat root: beat 2, the boulevard.</summary>
        public const int Beat = 1;

        public static int Balconies, Fins, AcUnits, Faces;
        /// <summary>Every element: face (0 south, 1 north), u span, y span, kind (b / f / a) for ReliefTests.</summary>
        public static readonly List<(int face, double u0, double u1, double y0, double y1, char kind, double uMin, double uMax, uint seed)> Elements
            = new List<(int, double, double, double, double, char, double, double, uint)>(8192);

        /// <summary>A slab street face: x range, the face plane z and its outward normal z, the seed of its box.</summary>
        public struct SlabFace { public double xa, xb, z, nz; public uint seed; public int region; }

        /// <summary>The street faces of the hero slabs exactly as StreetFacade builds them (FacadeKit.Box PZ / NZ faces).</summary>
        public static List<SlabFace> SlabFaces()
        {
            var l = new List<SlabFace>(16);
            for (int side = 0; side < 2; side++)
            {
                bool south = side == 0;
                uint seed = south ? StreetFacade.SeedSouth : StreetFacade.SeedNorth;
                double zs0 = south ? BoulevardLayout.SouthShaftZ0 : BoulevardLayout.NorthShaftZ0;
                double zs1 = south ? BoulevardLayout.SouthShaftZ1 : BoulevardLayout.NorthShaftZ1;
                var slabs = new List<(double, double, int)>(8);
                if (!south) slabs.Add((BoulevardLayout.NorthWestSlabX0, BoulevardLayout.NorthWestSlabX1, -1));
                for (int k = south ? BoulevardLayout.SouthFirstSlab : 0; k < BoulevardLayout.SlabCount; k++) slabs.Add((BoulevardLayout.SlabXa(k), BoulevardLayout.SlabXb(k), k));
                for (int i = 0; i < slabs.Count; i++)
                {
                    var (xa, xb, k) = slabs[i];
                    if (i == slabs.Count - 1) xb = BoulevardLayout.ClosureX1;
                    l.Add(new SlabFace
                    {
                        xa = xa, xb = xb, z = south ? zs1 : zs0, nz = south ? 1.0 : -1.0, seed = seed + (uint)(k + 1),
                        region = NocFacadeMath.Region(0.5 * (xa + xb), 0.5 * (zs0 + zs1)),
                    });
                }
            }
            return l;
        }

        public static void Build(List<ReliefPart> parts)
        {
            Balconies = Fins = AcUnits = Faces = 0;
            Elements.Clear();
            var origin = new Vector3(80f, 100f, (float)BoulevardLayout.AxisZ);
            var steel = new ReliefPart("BoulevardRelief_steel", ReliefMat.Steel, origin, Beat);
            var conc = new ReliefPart("BoulevardRelief_concrete", ReliefMat.Concrete, origin, Beat);
            parts.Add(steel); parts.Add(conc);
            foreach (var f in SlabFaces()) BuildFace(f, steel, conc);
        }

        static void BuildFace(SlabFace f, ReliefPart steel, ReliefPart conc)
        {
            Faces++;
            int fi = f.nz > 0 ? 0 : 1;
            // FacadeKit's u: the street face of a south slab runs a = (xa, z) -> b = (xb, z), u = x; a north slab's runs (xb, z) ->
            // (xa, z) with the normal -z, u = -x (CityField.FaceFrame: tangent = yaw + 90 deg)
            double uSign = f.nz > 0 ? 1.0 : -1.0;
            double uMin = Math.Min(uSign * f.xa, uSign * f.xb), uMax = Math.Max(uSign * f.xa, uSign * f.xb);
            var skin = NocFacadeMath.SkinOf(NocFacadeMath.SkinByte(f.seed, f.region), 0);
            int b0 = (int)Math.Floor(YMin / NocturneConstants.FacadeBand), b1 = (int)Math.Floor(YMax / NocturneConstants.FacadeBand);
            for (int band = b0; band <= b1; band++)
            {
                int type = NocFacadeMath.StratumV2(skin.key, band);
                var c = NocFacadeMath.CellOf(skin, type);
                NocFacadeMath.Bays(skin, c, type, uMin, uMax, out int iF, out int iL, out int core0, out int core1);
                NocFacadeMath.Rows(skin, c, type, band, YMin, BoulevardLayout.ShaftY1, out int jF, out int jL);
                for (int j = jF; j <= jL; j++)
                {
                    double yc = band * c.bandH + j * c.Pv;                   // the cell's bottom (its slab band starts here)
                    if (yc < YMin || yc + c.Pv > YMax) continue;
                    for (int i = iF; i <= iL; i++)
                    {
                        if (!NocFacadeMath.BayHasWindow(i, iF, iL, core0, core1)) continue;
                        double u0 = i * c.Pu, x0 = uSign * (u0 + c.ox0), x1 = uSign * (u0 + c.ox1);
                        double xl = Math.Min(x0, x1), xr = Math.Max(x0, x1);
                        if (NearCanyonBridge(xl, xr, yc)) continue;
                        if (skin.id == NocFacadeMath.SkinS4 && type == NocFacadeMath.StratumRes)
                        {
                            Balcony(steel, conc, f, xl - BalSide, xr + BalSide, yc, c.slab);
                            Elements.Add((fi, u0 + c.ox0 - BalSide, u0 + c.ox1 + BalSide, yc, yc + c.slab, 'b', uMin, uMax, f.seed)); Balconies++;
                        }
                        else if (skin.id == NocFacadeMath.SkinS0 && (type == NocFacadeMath.StratumRes || type == NocFacadeMath.StratumCom))
                        {
                            // one fin per cell edge (u0), over this row's height (rows stack: the fins read as continuous lines)
                            double xe = uSign * u0;
                            Box(steel, f, xe - 0.5 * NocFacadeConstants.FaS0Mull, xe + 0.5 * NocFacadeConstants.FaS0Mull, yc, yc + c.Pv, 0.0, FinDepth);
                            Elements.Add((fi, u0, u0, yc, yc + c.Pv, 'f', uMin, uMax, f.seed)); Fins++;
                        }
                        else if (skin.id != NocFacadeMath.SkinS0 && skin.id != NocFacadeMath.SkinS4 && type == NocFacadeMath.StratumRes
                                 && NocHash.U01(NocHash.H3(f.seed, unchecked((uint)(i + 100000)), unchecked((uint)(band * 64 + j)), 7)) < 0.125)
                        {
                            double xm = 0.5 * (x0 + x1), ya = yc + c.oy0 - 0.05 - AcH;
                            if (ya < yc + c.slab + 0.02) continue;
                            Box(conc, f, xm - 0.5 * AcW, xm + 0.5 * AcW, ya, ya + AcH, 0.0, AcD);
                            Elements.Add((fi, u0 + 0.5 * (c.ox0 + c.ox1) - 0.5 * AcW, u0 + 0.5 * (c.ox0 + c.ox1) + 0.5 * AcW, ya, ya + AcH, 'a', uMin, uMax, f.seed)); AcUnits++;
                        }
                    }
                }
            }
        }

        /// <summary>The canyon bridge (x >= 212.25, y 108..132 with its fascias 106.4..133.2) passes through slab 6's last bays.</summary>
        static bool NearCanyonBridge(double xl, double xr, double yc) => xr > GateDims.CbX0 - 1.5 && yc > GateDims.CbY0 - 6.0 && yc < GateDims.CbY1 + 3.0;

        /// <summary>A balcony on the face: the slab (concrete, the cell's slab band, 0.9 m out, 0.1 m into the wall), the rail panel and
        /// the top rail (painted steel) on its edge.</summary>
        static void Balcony(ReliefPart steel, ReliefPart conc, SlabFace f, double xl, double xr, double y, double slab)
        {
            Box(conc, f, xl, xr, y, y + slab, -0.1, BalDepth);
            Box(steel, f, xl + 0.02, xr - 0.02, y + slab, y + slab + RailH, BalDepth - 0.08, BalDepth - 0.08 + RailT);
            Box(steel, f, xl, xr, y + slab + RailH, y + slab + RailH + TopRail, BalDepth - 0.1, BalDepth - 0.1 + TopRail);
        }

        /// <summary>An axis-aligned box in front of the face: x [xl, xr], y [ya, yb], from n0 to n1 m out of the face (plain, 12 triangles).</summary>
        static void Box(ReliefPart part, SlabFace f, double xl, double xr, double ya, double yb, double n0, double n1)
        {
            double za = f.z + f.nz * n0, zb = f.z + f.nz * n1;
            var mb = part.Mb; var o = part.Origin;
            var lo = new Vector3((float)xl, (float)ya, (float)Math.Min(za, zb)) - o;
            var hi = new Vector3((float)xr, (float)yb, (float)Math.Max(za, zb)) - o;
            mb.Box(0.5f * (lo + hi), hi - lo);
        }
    }
}
