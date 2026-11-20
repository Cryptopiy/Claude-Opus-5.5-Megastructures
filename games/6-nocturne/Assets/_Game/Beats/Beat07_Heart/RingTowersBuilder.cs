// RingTowersBuilder: the 22 hero towers around Heart Square (MASTER_PROMPT 14.5.7: R 470 - 560 about S, bearings within +-40 deg of
// west excluded, NocFacade with warm fraction 0; CityField excludes the Heart within R 620, D-018). Each tower is a CityTower
// (three stacked boxes y -3 300..ledge A..ledge B..560, setbacks 3 - 6 m) meshed by the city's own CityMesher, so its windows,
// face ids and back-face cull are exactly the city's. Tower k sits on bearing 220 + 280 (k + 0.5) / 22 deg (+-1.5 deg by hash), its
// +Z face turned toward S (yaw on the 0.5 deg lattice), the inner face at >= 470 m from S and every corner within 560 m.
// One mesh per tower with its own origin at the footprint centre (7.5: chunk-local origins).
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Megastructure
{
    public static class RingTowersBuilder
    {
        const uint Salt = 0x52696E67u;   // "Ring"

        static double U(int k, uint i) => NocHash.U01(NocHash.H3(unchecked((uint)(k + 1024)), Salt, i, 0));

        /// <summary>The 22 towers (deterministic).</summary>
        public static List<CityTower> Towers()
        {
            var list = new List<CityTower>(HeartLayout.RingTowerCount);
            double span = 360.0 - 2.0 * HeartLayout.RingWestClear;            // 280 deg
            double start = 180.0 + HeartLayout.RingWestClear;                  // 220 deg
            for (int k = 0; k < HeartLayout.RingTowerCount; k++)
            {
                double bearing = start + span * (k + 0.5) / HeartLayout.RingTowerCount + 1.5 * (2.0 * U(k, 1) - 1.0);
                double hz = NocMath.Lerp(HeartLayout.RingHalfMin, HeartLayout.RingHalfMax - 2.0, U(k, 2));   // radial half size
                double hx = NocMath.Lerp(HeartLayout.RingHalfMin, HeartLayout.RingHalfMax, U(k, 3));         // tangential half size
                double rMin = HeartLayout.RingInner + hz + 0.5;
                double rMax = Math.Sqrt(HeartLayout.RingOuter * HeartLayout.RingOuter - hx * hx) - hz - 0.5;
                double rc = NocMath.Lerp(rMin, rMax, U(k, 4));
                double b = bearing * NocMath.Deg;
                double cx = HeartLayout.SX + rc * Math.Cos(b), cz = HeartLayout.SZ + rc * Math.Sin(b);
                // +Z face toward S: Unity yaw of the direction (S - c)
                double yaw = Math.Atan2(-Math.Cos(b), -Math.Sin(b)) / NocMath.Deg;
                yaw = Math.Round(yaw / NocturneConstants.CityYawQuantum) * NocturneConstants.CityYawQuantum;
                var t = new CityTower { ci = 100000 + k, cj = 100000, cx = cx, cz = cz, yaw = yaw, radius = Math.Sqrt(hx * hx + hz * hz), tier = 1 };
                t.seed = NocHash.H3(unchecked((uint)(k + 1024)), Salt, 99u, 1);
                double yA = NocMath.Lerp(140.0, 220.0, U(k, 5));
                double yB = NocMath.Lerp(yA + 90.0, Math.Min(yA + 170.0, 400.0), U(k, 6));
                t.boxes[0] = new CityBox { hx = hx, hz = hz, y0 = NocturneConstants.CityShaftY0, y1 = yA };
                double sb1 = NocMath.Lerp(3.0, 6.0, U(k, 7)), sb2 = NocMath.Lerp(3.0, 6.0, U(k, 8));
                t.boxes[1] = new CityBox { hx = hx - sb1, hz = hz - sb1, y0 = yA, y1 = yB };
                t.boxes[2] = new CityBox { hx = hx - sb1 - sb2, hz = hz - sb1 - sb2, y0 = yB, y1 = NocturneConstants.CityShaftY1 };
                t.edge = (k % 4 == 1) ? 1 : ((k % 7 == 3) ? 3 : 0);   // a few cyan / white edge lights
                list.Add(t);
            }
            return list;
        }

        /// <summary>Distance from S of the nearest and farthest footprint corner of the base box (tests).</summary>
        public static void RadialExtent(CityTower t, out double rIn, out double rOut)
        {
            rIn = double.MaxValue; rOut = 0.0;
            for (int sx = -1; sx <= 1; sx += 2)
                for (int sz = -1; sz <= 1; sz += 2)
                {
                    t.Corner(0, sx, sz, out double x, out double z);
                    double d = Math.Sqrt((x - HeartLayout.SX) * (x - HeartLayout.SX) + (z - HeartLayout.SZ) * (z - HeartLayout.SZ));
                    rIn = Math.Min(rIn, d); rOut = Math.Max(rOut, d);
                }
            // the inner face's mid point is nearer than its corners
            double b = t.yaw * NocMath.Deg;
            double fx = t.cx + t.boxes[0].hz * Math.Sin(b), fz = t.cz + t.boxes[0].hz * Math.Cos(b);
            rIn = Math.Min(rIn, Math.Sqrt((fx - HeartLayout.SX) * (fx - HeartLayout.SX) + (fz - HeartLayout.SZ) * (fz - HeartLayout.SZ)));
        }

        public static void Build(WorldContext ctx, Transform root)
        {
            var lib = ctx.Materials;
            var one = new List<CityTower>(1);
            int faces = 0, culled = 0;
            foreach (var t in Towers())
            {
                one.Clear(); one.Add(t);
                var mb = new MeshBuilder();
                faces += CityMesher.Append(mb, one, t.cx, t.cz, ref culled);
                G4Kit.Add(ctx, root, "ringTower" + (t.ci - 100000), mb, lib != null ? lib.Facade : null, new Vector3((float)t.cx, 0f, (float)t.cz));
            }
            ctx.Log(FormattableString.Invariant($"Beat07 ring towers: {HeartLayout.RingTowerCount} towers, {faces} faces ({culled} never facing the walk, not built)"));
        }
    }
}
