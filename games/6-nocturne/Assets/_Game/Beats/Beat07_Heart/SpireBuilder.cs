// SpireBuilder: the central spire of Heart Square (MASTER_PROMPT 14.5.7 Geometry, 14.4 "Heart"): a 24-sided shaft at S = (1 330, 388)
// from y -3 300 to 560, corner radius 32 at the plaza tapering linearly to 20 at y 460 (20 above; 32 below the plaza), its faces
// NocFacade cold windows (CityMesher contract, world-derived u: the trapezoid faces are planar, so u and uMin / uMax are exact
// under interpolation); base drums R 48 / 42 / 36, 6 m each (StoneWall), each sunk 0.1 m into the one below so no cap is co-planar;
// 12 rib fins (MetalDark thin boxes, 4 m deep, 1.2 m thick) on every second corner from the top drum to the top, following the
// taper, each with a cyan-white NocLine strip 0.5 m wide on its outer edge (clip-space widening to 1.75 px: no aliasing at the
// 200 - 700 m the walk sees it from). Ring lights every 40 m are cut in the graybox (14.5.7 Simplified).
// GE (TEXTURE_PLAN 5 item 4, CRIT0 #21): the shaft carries FA's hero skin (SkinOverride(0): S0 with fins); five belts (one every
// 92 m from y 92 to the taper end 460: a 24-gon ring 0.8 m proud and 1.6 m tall in painted steel, so the taper reads as tiers of
// floors) each with a ring line on its outer face (the fins' strip colour, PA); aviation beacons (the city's Beacon material) on
// four fins at y 184, 276, 368 and 440 (below the cloud base, 460). The silhouette stays inside the 14.5.7 envelope R(y) + 4 m
// (fins), so the koi lint (KS17 / KS18 spire occlusion) is unchanged; the top stays in the cloud deck (no crown: decision 3).
// Belts, lines and beacons are built with the v2 relief only (ReliefBuilder.ReliefOn: -relief-v, default = -facade-v; D-030).
using System.Collections.Generic;
using UnityEngine;

namespace Megastructure
{
    public static class SpireBuilder
    {
        public static readonly Vector3 Origin = new Vector3((float)HeartLayout.SX, 0f, (float)HeartLayout.SZ);

        /// <summary>Corner k of the 24-gon at height y (world), angle 15 k deg from +X toward +Z.</summary>
        public static Vector3 Corner(int k, double y)
        {
            double a = 2.0 * System.Math.PI * k / HeartLayout.SpireSides, r = HeartLayout.SpireRadius(y);
            return new Vector3((float)(HeartLayout.SX + r * System.Math.Cos(a)), (float)y, (float)(HeartLayout.SZ + r * System.Math.Sin(a)));
        }

        /// <summary>The shaft faces: per side three planar trapezoids (y -3 300..0, 0..460, 460..560).</summary>
        public static MeshBuilder Shaft()
        {
            var mb = new MeshBuilder();
            double[] ys = { HeartLayout.SpireY0, 0.0, HeartLayout.SpireTaperY, HeartLayout.SpireY1 };
            for (int k = 0; k < HeartLayout.SpireSides; k++)
            {
                double am = 2.0 * System.Math.PI * (k + 0.5) / HeartLayout.SpireSides;
                var n = new Vector3((float)System.Math.Cos(am), 0f, (float)System.Math.Sin(am));
                for (int b = 0; b < 3; b++)
                {
                    // edge A = corner k + 1, edge B = corner k (seen from outside, A on the left: the order does not matter for NocFacade)
                    G4Kit.FacadeQuad(mb, Origin, Corner(k + 1, ys[b]), Corner(k + 1, ys[b + 1]), Corner(k, ys[b + 1]), Corner(k, ys[b]), n,
                        NocFacadeMath.SkinOverride(NocFacadeMath.OverrideSpire), 0);      // FA: the spire's hero skin (was seed 0xA7)
                }
            }
            return mb;
        }

        public static MeshBuilder Drums()
        {
            var mb = new MeshBuilder();
            double y = -1.0;
            for (int i = 0; i < HeartLayout.DrumR.Length; i++)
            {
                double y0 = i == 0 ? -1.0 : y - HeartLayout.DrumSink, y1 = HeartLayout.DrumH * (i + 1);
                MeshKit.Cylinder(mb, new Vector3(0f, (float)y0, 0f), new Vector3(0f, (float)y1, 0f), (float)HeartLayout.DrumR[i], 96, true);
                y = y1;
            }
            return mb;
        }

        /// <summary>Fin j (0..11) on corner 2 j: two prisms (top drum .. 460, 460 .. 560) between the radii R(y) - 0.4 and R(y) + 4.</summary>
        public static void Fin(MeshBuilder mb, int j)
        {
            double a = 2.0 * System.Math.PI * (2 * j) / HeartLayout.SpireSides;
            var radial = new Vector3((float)System.Math.Cos(a), 0f, (float)System.Math.Sin(a));
            var tangent = new Vector3(-radial.z, 0f, radial.x);
            double[] ys = { HeartLayout.DrumH * HeartLayout.DrumR.Length - HeartLayout.DrumSink, HeartLayout.SpireTaperY, HeartLayout.SpireY1 };
            float ht = 0.5f * (float)HeartLayout.FinThick;
            for (int b = 0; b < 2; b++)
            {
                double ya = ys[b], yb = ys[b + 1];
                Vector3 ia = radial * (float)(HeartLayout.SpireRadius(ya) - HeartLayout.FinInset) + Vector3.up * (float)ya;
                Vector3 oa = radial * (float)(HeartLayout.SpireRadius(ya) + HeartLayout.FinDepth) + Vector3.up * (float)ya;
                Vector3 ib = radial * (float)(HeartLayout.SpireRadius(yb) - HeartLayout.FinInset) + Vector3.up * (float)yb;
                Vector3 ob = radial * (float)(HeartLayout.SpireRadius(yb) + HeartLayout.FinDepth) + Vector3.up * (float)yb;
                Vector3 l = -tangent * ht, r = tangent * ht;
                // the two side faces, the outer edge face, the bottom and the top
                G4Kit.OrientedQuad(mb, ia + l, oa + l, ob + l, ib + l, -tangent);
                G4Kit.OrientedQuad(mb, ia + r, oa + r, ob + r, ib + r, tangent);
                G4Kit.OrientedQuad(mb, oa + l, oa + r, ob + r, ob + l, Vector3.Cross(ob - oa, tangent).normalized);
                G4Kit.OrientedQuad(mb, ia + l, ia + r, oa + r, oa + l, Vector3.down);
                G4Kit.OrientedQuad(mb, ib + l, ib + r, ob + r, ob + l, Vector3.up);
            }
        }

        /// <summary>The strip tube of fin j: open cylinders of radius StripWidth / 2 along the fin's outer edge (radial normals for NocLine).</summary>
        public static void Strip(MeshBuilder mb, int j)
        {
            double a = 2.0 * System.Math.PI * (2 * j) / HeartLayout.SpireSides;
            var radial = new Vector3((float)System.Math.Cos(a), 0f, (float)System.Math.Sin(a));
            double[] ys = { HeartLayout.DrumH * HeartLayout.DrumR.Length, HeartLayout.SpireTaperY, HeartLayout.SpireY1 };
            float rr = 0.5f * (float)HeartLayout.StripWidth;
            for (int b = 0; b < 2; b++)
            {
                // pieces of at most 40 m: NocLine widens per vertex, so long pieces would interpolate the widening linearly
                int pieces = (int)System.Math.Ceiling((ys[b + 1] - ys[b]) / 40.0);
                for (int p = 0; p < pieces; p++)
                {
                    double y0 = ys[b] + (ys[b + 1] - ys[b]) * p / pieces, y1 = ys[b] + (ys[b + 1] - ys[b]) * (p + 1) / pieces;
                    Vector3 pa = radial * (float)(HeartLayout.SpireRadius(y0) + HeartLayout.FinDepth + rr) + Vector3.up * (float)y0;
                    Vector3 pb = radial * (float)(HeartLayout.SpireRadius(y1) + HeartLayout.FinDepth + rr) + Vector3.up * (float)y1;
                    MeshKit.Cylinder(mb, pa, pb, rr, 8, false);
                }
            }
        }

        /// <summary>Belt heights (every 92 m to the taper end), belt size, ring-line radius, beacon fins and heights (GE).</summary>
        public static readonly double[] BeltY = { 92.0, 184.0, 276.0, 368.0, 460.0 };
        public const double BeltProud = 0.8, BeltH = 1.6, BeltInset = 0.3;
        public static readonly int[] BeaconFins = { 0, 3, 6, 9 };
        public static readonly double[] BeaconY = { 184.0, 276.0, 368.0, 440.0 };

        /// <summary>The belts: a 24-gon prism ring per height (outer face, top and bottom), from R(y) - 0.3 to R(y) + 0.8 at its mid
        /// height (the taper is 0.04 m over 1.6 m: the ring is cylindrical), its faces wound outward / up / down.</summary>
        public static MeshBuilder Belts()
        {
            var mb = new MeshBuilder();
            int N = HeartLayout.SpireSides;
            foreach (double yc in BeltY)
            {
                double ya = yc - 0.5 * BeltH, yb = yc + 0.5 * BeltH, r = HeartLayout.SpireRadius(yc);
                double ri = r - BeltInset, ro = r + BeltProud;
                for (int k = 0; k < N; k++)
                {
                    double a0 = 2.0 * System.Math.PI * k / N, a1 = 2.0 * System.Math.PI * (k + 1) / N, am = 0.5 * (a0 + a1);
                    Vector3 d0 = new Vector3((float)System.Math.Cos(a0), 0f, (float)System.Math.Sin(a0)), d1 = new Vector3((float)System.Math.Cos(a1), 0f, (float)System.Math.Sin(a1));
                    Vector3 nm = new Vector3((float)System.Math.Cos(am), 0f, (float)System.Math.Sin(am));
                    Vector3 Y(double y) => Vector3.up * (float)y;
                    G4Kit.OrientedQuad(mb, d0 * (float)ro + Y(ya), d0 * (float)ro + Y(yb), d1 * (float)ro + Y(yb), d1 * (float)ro + Y(ya), nm);
                    G4Kit.OrientedQuad(mb, d0 * (float)ri + Y(yb), d0 * (float)ro + Y(yb), d1 * (float)ro + Y(yb), d1 * (float)ri + Y(yb), Vector3.up);
                    G4Kit.OrientedQuad(mb, d0 * (float)ri + Y(ya), d0 * (float)ro + Y(ya), d1 * (float)ro + Y(ya), d1 * (float)ri + Y(ya), Vector3.down);
                }
            }
            return mb;
        }

        /// <summary>The ring lines: one NocLine tube (radius StripWidth / 2) around each belt's outer face at its mid height, 24
        /// straight pieces (radial normals).</summary>
        public static MeshBuilder BeltLines()
        {
            var mb = new MeshBuilder();
            int N = HeartLayout.SpireSides;
            float rr = 0.5f * (float)HeartLayout.StripWidth;
            foreach (double yc in BeltY)
            {
                double r = HeartLayout.SpireRadius(yc) + BeltProud + rr - 0.05;
                for (int k = 0; k < N; k++)
                {
                    double a0 = 2.0 * System.Math.PI * k / N, a1 = 2.0 * System.Math.PI * (k + 1) / N;
                    var pa = new Vector3((float)(r * System.Math.Cos(a0)), (float)yc, (float)(r * System.Math.Sin(a0)));
                    var pb = new Vector3((float)(r * System.Math.Cos(a1)), (float)yc, (float)(r * System.Math.Sin(a1)));
                    MeshKit.Cylinder(mb, pa, pb, rr, 8, false);
                }
            }
            return mb;
        }

        /// <summary>World positions of the spire's aviation beacons (0.6 m outside the fins' outer edges).</summary>
        public static List<Vector3> BeaconPositions()
        {
            var l = new List<Vector3>(16);
            foreach (int j in BeaconFins)
            {
                double a = 2.0 * System.Math.PI * (2 * j) / HeartLayout.SpireSides;
                foreach (double y in BeaconY)
                {
                    double r = HeartLayout.SpireRadius(y) + HeartLayout.FinDepth + 0.6;
                    l.Add(Origin + new Vector3((float)(r * System.Math.Cos(a)), (float)y, (float)(r * System.Math.Sin(a))));
                }
            }
            return l;
        }

        public static void Build(WorldContext ctx, Transform root)
        {
            var lib = ctx.Materials;
            G4Kit.Add(ctx, root, "spireShaft", Shaft(), lib != null ? lib.Facade : null, Origin);
            G4Kit.Add(ctx, root, "spireDrums", Drums(), lib != null ? lib.StoneWall : null, Origin);
            var fins = new MeshBuilder();
            var strips = new MeshBuilder();
            for (int j = 0; j < HeartLayout.FinCount; j++) { Fin(fins, j); Strip(strips, j); }
            G4Kit.Add(ctx, root, "spireFins", fins, lib != null ? lib.SteelPainted : null, Origin);
            // PA (LOOK_V2 10): the strip mix is kept, its base is MetalPaint (was ConcreteDark)
            Color strip = G4Kit.Mix(G4Kit.Lin(NocPal.NeonCyan), G4Kit.Lin(NocPal.SignWhite), 0.5);
            Material stripMat = lib != null ? G4Kit.Line(lib.Line, HeartLayout.StripWidth, G4Kit.Lin(NocPal.MetalPaint), G4Kit.Scale(strip, HeartLayout.StripGain), "Beat07_SpireStrip") : null;
            G4Kit.Add(ctx, root, "spireStrips", strips, stripMat, Origin);
            // GE: belts, ring lines, beacons (TEXTURE_PLAN 5 relief: with the v2 facade, ReliefBuilder.ReliefOn)
            if (!ReliefBuilder.ReliefOn) return;
            G4Kit.Add(ctx, root, "spireBelts", Belts(), lib != null ? lib.SteelPainted : null, Origin);
            G4Kit.Add(ctx, root, "spireBeltLines", BeltLines(), stripMat, Origin);
            if (lib != null && lib.Beacon != null)
            {
                var b = new InstancedBatch("SpireBeacons", NocTraffic.PointQuad(), lib.Beacon, chunkSize: 600f);
                int i = 0;
                foreach (var p in BeaconPositions())
                    b.Add(NocTraffic.PointMatrix(p.x, p.y, p.z, NocturneConstants.CityBeaconSize, (0.13 * i++) % 1.0));
                NocCameraBatches.Register(b);
            }
        }
    }
}
