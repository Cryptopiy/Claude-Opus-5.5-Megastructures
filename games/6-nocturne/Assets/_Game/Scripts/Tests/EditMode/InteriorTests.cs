// InteriorTests (owner FA): the Look v2 box rooms (docs/TEXTURE_PLAN.md 2.8, FA-3) on the C# twin NocInteriorMath:
//   - RoomMean (the closed-form expectation the shader divides by) against a 64 x 64 integral of RoomRaw over the glass, for the
//     residential, commercial and section rooms at every depth, up to 60 deg of incidence in every direction: within 3 %
//     (TEXTURE_PLAN 2.8: "3 % against numeric integration up to 60 deg");
//   - the room reads as a room: frontal views see only the back wall, oblique views the side walls darker than the back wall,
//     upward views the ceiling, downward views the floor;
//   - no NaN at grazing (shifts up to 1 000 per metre of depth).
using System;
using NUnit.Framework;
using UnityEngine;

namespace Megastructure.Tests
{
    public class InteriorTests
    {
        // glass rectangles (inside the frames) and rooms of the strata: (g0, g1, h0, h1, W, yc)
        static readonly double[][] Rooms =
        {
            new[] { 0.52, 1.88, 1.02, 2.98, 2.4, 3.3 },     // S1 residential 1.6 x 2.2 opening, frame 0.12
            new[] { 0.58, 1.82, 0.98, 2.42, 2.4, 3.3 },     // S4 residential
            new[] { 0.30, 4.50, 0.70, 3.70, 4.8, 4.02 },    // commercial
            new[] { 0.50, 8.50, 0.60, 3.60, 9.0, 3.70 },    // section bay
        };

        [Test]
        public void RoomMean_MatchesTheIntegralUpTo60Deg()
        {
            double worst = 0; string at = "";
            foreach (var r in Rooms)
                foreach (double Dr in new[] { 6.0, 8.0, 10.0, 14.0 })
                    for (int th = 0; th <= 60; th += 10)
                        for (int ph = 0; ph < 360; ph += 15)
                        {
                            double k = Math.Tan(th * Math.PI / 180.0), kx = k * Math.Cos(ph * Math.PI / 180.0), ky = k * Math.Sin(ph * Math.PI / 180.0);
                            double m = NocInteriorMath.RoomMean(r[0], r[1], r[2], r[3], kx, ky, r[4], 0.0, r[5], Dr);
                            double n = NocInteriorMath.NumericMean(r[0], r[1], r[2], r[3], kx, ky, r[4], 0.0, r[5], Dr);
                            double e = Math.Abs(m / Math.Max(n, 1e-9) - 1.0);
                            if (e > worst) { worst = e; at = FormattableString.Invariant($"W {r[4]} Dr {Dr} theta {th} phi {ph}: closed {m:F4} / integral {n:F4}"); }
                        }
            Debug.Log(FormattableString.Invariant($"[FA] room mean vs 64 x 64 integral: worst {100 * worst:F2} % ({at})"));
            Assert.Less(worst, 0.03, "the closed-form room mean within 3 % of the integral up to 60 deg");
        }

        [Test]
        public void Room_ReadsAsARoom_AndNoNaN()
        {
            var r = Rooms[0];
            double W = r[4], yc = r[5], Dr = 8.0, xc = 0.5 * (r[0] + r[1]), yg = 0.5 * (r[2] + r[3]);
            double back = NocInteriorMath.RoomRaw(xc, yg, 0.0, 0.0, W, 0.0, yc, Dr, out int s0);
            Assert.AreEqual(0, s0, "frontal: the back wall");
            double side = NocInteriorMath.RoomRaw(xc, yg, 0.6, 0.0, W, 0.0, yc, Dr, out int s1);
            Assert.AreEqual(1, s1, "looking sideways: a side wall");
            Assert.Less(side, back, "side walls darker than the back wall");
            NocInteriorMath.RoomRaw(xc, yg, 0.0, 0.8, W, 0.0, yc, Dr, out int s2);
            Assert.AreEqual(2, s2, "looking up: the ceiling");
            NocInteriorMath.RoomRaw(xc, yg, 0.0, -0.8, W, 0.0, yc, Dr, out int s3);
            Assert.AreEqual(3, s3, "looking down: the floor");
            foreach (double k in new[] { 1e-7, 50.0, 1000.0, -1000.0 })
            {
                double a = NocInteriorMath.RoomRaw(xc, yg, k, -k, W, 0.0, yc, Dr, out _);
                double m = NocInteriorMath.RoomMean(r[0], r[1], r[2], r[3], k, -k, W, 0.0, yc, Dr);
                Assert.IsFalse(double.IsNaN(a) || double.IsNaN(m) || m <= 0.0, "no NaN, positive mean at grazing");
            }
        }
    }
}
