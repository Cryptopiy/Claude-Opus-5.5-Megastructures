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
// HT (CRIT0 #21, Phase 4 wave A: "an engineered colossus, not a lined box"): GE's five 92 m belts become eleven ring girders
// (hoops) every 40 m (y 40 - 440, R(y) - 0.3 .. R(y) + 3.6: the fins pass through them and stand 0.4 m proud) carrying the
// 14.5.7 ring lights, which breathe at 0.1 Hz with a phase lag of 0.5 rad per ring (a slow pulse rising up the spire;
// SpireDriver, one NocLine material per ring); the 12 fins flare into buttresses below 96 m (outer edge R(y) + 4 + 9.5 at y 6,
// landing on the lowest drum) with their strips following the edge down; a podium of three granite steps (R 56 / 53 / 50.5,
// 0.16 m each) meets the plaza around the drums. Hoops, lights, flare and steps stay inside the 14.5.7 envelope R(y) + 4 above
// 96 m, so the koi's clearances and the KS17 / KS18 lint are unchanged.
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

        /// <summary>Heights of the fin's pieces: the flare (y FlareY0 - DrumSink .. FlareY1 in FlarePieces), then FlareY1 .. 460 in
        /// pieces of at most 40 m (NocLine widens per vertex: the strips need short pieces), then 460 .. 560.</summary>
        public static List<double> FinHeights()
        {
            var ys = new List<double>(32);
            double y0 = HeartLayout.FlareY0 - HeartLayout.DrumSink, y1 = HeartLayout.FlareY1;
            for (int i = 0; i < HeartLayout.FlarePieces; i++) ys.Add(y0 + (y1 - y0) * i / HeartLayout.FlarePieces);
            int n = (int)System.Math.Ceiling((HeartLayout.SpireTaperY - y1) / 40.0);
            for (int i = 0; i < n; i++) ys.Add(y1 + (HeartLayout.SpireTaperY - y1) * i / n);
            ys.Add(HeartLayout.SpireTaperY);
            ys.Add(HeartLayout.SpireY1);
            return ys;
        }

        /// <summary>Fin j (0..11) on corner 2 j: a prism between the radii R(y) - 0.4 and HeartLayout.FinOuter(y) (the buttress flare
        /// below 96 m, R(y) + 4 above), its pieces at FinHeights, closed at the bottom (on the lowest drum) and the top.</summary>
        public static void Fin(MeshBuilder mb, int j)
        {
            double a = 2.0 * System.Math.PI * (2 * j) / HeartLayout.SpireSides;
            var radial = new Vector3((float)System.Math.Cos(a), 0f, (float)System.Math.Sin(a));
            var tangent = new Vector3(-radial.z, 0f, radial.x);
            var ys = FinHeights();
            float ht = 0.5f * (float)HeartLayout.FinThick;
            Vector3 l = -tangent * ht, r = tangent * ht;
            for (int b = 0; b + 1 < ys.Count; b++)
            {
                double ya = ys[b], yb = ys[b + 1];
                Vector3 ia = radial * (float)(HeartLayout.SpireRadius(ya) - HeartLayout.FinInset) + Vector3.up * (float)ya;
                Vector3 oa = radial * (float)HeartLayout.FinOuter(ya) + Vector3.up * (float)ya;
                Vector3 ib = radial * (float)(HeartLayout.SpireRadius(yb) - HeartLayout.FinInset) + Vector3.up * (float)yb;
                Vector3 ob = radial * (float)HeartLayout.FinOuter(yb) + Vector3.up * (float)yb;
                // the two side faces and the outer edge face; the bottom of the first piece and the top of the last
                G4Kit.OrientedQuad(mb, ia + l, oa + l, ob + l, ib + l, -tangent);
                G4Kit.OrientedQuad(mb, ia + r, oa + r, ob + r, ib + r, tangent);
                G4Kit.OrientedQuad(mb, oa + l, oa + r, ob + r, ob + l, Vector3.Cross(ob - oa, tangent).normalized);
                if (b == 0) G4Kit.OrientedQuad(mb, ia + l, ia + r, oa + r, oa + l, Vector3.down);
                if (b + 2 == ys.Count) G4Kit.OrientedQuad(mb, ib + l, ib + r, ob + r, ob + l, Vector3.up);
            }
        }

        /// <summary>The strip tube of fin j: open cylinders of radius StripWidth / 2 along the fin's outer edge from the lowest drum to
        /// the top, one per fin piece (radial offset; NocLine).</summary>
        public static void Strip(MeshBuilder mb, int j)
        {
            double a = 2.0 * System.Math.PI * (2 * j) / HeartLayout.SpireSides;
            var radial = new Vector3((float)System.Math.Cos(a), 0f, (float)System.Math.Sin(a));
            var ys = FinHeights();
            ys[0] = HeartLayout.FlareY0 + 0.3;      // starts just above the drum's top
            float rr = 0.5f * (float)HeartLayout.StripWidth;
            for (int p = 0; p + 1 < ys.Count; p++)
            {
                double y0 = ys[p], y1 = ys[p + 1];
                Vector3 pa = radial * (float)(HeartLayout.FinOuter(y0) + rr) + Vector3.up * (float)y0;
                Vector3 pb = radial * (float)(HeartLayout.FinOuter(y1) + rr) + Vector3.up * (float)y1;
                MeshKit.Cylinder(mb, pa, pb, rr, 8, false);
            }
        }

        /// <summary>The podium: three granite steps (open cylinders with top caps) from y -1 to PlinthStep (i + 1), radii PlinthR.</summary>
        public static MeshBuilder Plinth()
        {
            var mb = new MeshBuilder();
            for (int i = 0; i < HeartLayout.PlinthR.Length; i++)
                MeshKit.Cylinder(mb, new Vector3(0f, -1f, 0f), new Vector3(0f, (float)(HeartLayout.PlinthStep * (i + 1)), 0f), (float)HeartLayout.PlinthR[i], 128, true);
            return mb;
        }

        /// <summary>Hoop heights: every HoopPitch from HoopPitch to HoopTop (11).</summary>
        public static List<double> HoopHeights()
        {
            var l = new List<double>(12);
            for (double y = HeartLayout.HoopPitch; y <= HeartLayout.HoopTop + 1e-6; y += HeartLayout.HoopPitch) l.Add(y);
            return l;
        }

        /// <summary>The ring girders: a 24-gon prism ring per hoop height (outer face, top and bottom), R(y) - 0.3 .. R(y) + HoopOut at
        /// its mid height (the taper is 0.04 m over 1.6 m: cylindrical), faces wound outward / up / down.</summary>
        public static MeshBuilder Hoops()
        {
            var mb = new MeshBuilder();
            int N = HeartLayout.SpireSides;
            foreach (double yc in HoopHeights())
            {
                double ya = yc - 0.5 * HeartLayout.HoopH, yb = yc + 0.5 * HeartLayout.HoopH, rs = HeartLayout.SpireRadius(yc);
                double ri = rs - BeltInset, ro = rs + HeartLayout.HoopOut;
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

        /// <summary>The ring light of hoop k: a NocLine tube (radius StripWidth / 2) around the hoop's outer face at its mid height, 24
        /// straight pieces (radial normals).</summary>
        public static MeshBuilder RingLight(double yc)
        {
            var mb = new MeshBuilder();
            int N = HeartLayout.SpireSides;
            float rr = 0.5f * (float)HeartLayout.StripWidth;
            double r = HeartLayout.SpireRadius(yc) + HeartLayout.HoopOut + rr - 0.15;      // outermost R(y) + 3.95: inside the envelope
            for (int k = 0; k < N; k++)
            {
                double a0 = 2.0 * System.Math.PI * k / N, a1 = 2.0 * System.Math.PI * (k + 1) / N;
                var pa = new Vector3((float)(r * System.Math.Cos(a0)), (float)yc, (float)(r * System.Math.Sin(a0)));
                var pb = new Vector3((float)(r * System.Math.Cos(a1)), (float)yc, (float)(r * System.Math.Sin(a1)));
                MeshKit.Cylinder(mb, pa, pb, rr, 8, false);
            }
            return mb;
        }

        /// <summary>HT (INT-12 "the wet mirror of the spire", fifth start): the fins whose strips face the walk (120 - 240 deg; the
        /// walk runs west of the spire), the path positions of the three copies of their H4 sources, and the chords' top. The floor in
        /// KS17's frame mirrors elevations 0 - 15 deg (pitch +18, vFOV 66: the frame's bottom is 15 deg down, 6.3 m out), i.e. the
        /// spire up to 100 m at 380 m, so one chord per fin from the drum to MirrorTop (at most 5 m off the flared strip at y 96: 0.65 deg
        /// at 380 m, inside the streak's lateral lobe of about 0.75 deg). The koi (y >= 184 m, 25 - 66 deg up from KS17's eye) is
        /// mirrored only by floor under the frame. Copies 92 m apart: a source's window rises over s_i - 72 .. s_i - 60 and falls over
        /// s_i + 20 .. s_i + 32 (smoothsteps, NocWetRegistry.SourceWeight), so neighbouring copies sum to exactly 1 and the streaks are
        /// steady from s 1 240 to 1 504 (fade-in from 1 228); at most 2 x 5 sources are active.</summary>
        public static readonly int[] MirrorFins = { 4, 5, 6, 7, 8 };
        public static readonly double[] MirrorS = { 1300.0, 1392.0, 1484.0 };
        public const double MirrorTop = 200.0;

        /// <summary>Registers the mirror sources of the strips (see MirrorFins): radiance = the strip's emission.</summary>
        public static void AddStripSources(Color radiance)
        {
            float rr = 0.5f * (float)HeartLayout.StripWidth;
            double y0 = HeartLayout.FlareY0 + 0.3, y1 = MirrorTop;
            foreach (double si in MirrorS)
                foreach (int j in MirrorFins)
                {
                    double a = 2.0 * System.Math.PI * (2 * j) / HeartLayout.SpireSides;
                    var radial = new Vector3((float)System.Math.Cos(a), 0f, (float)System.Math.Sin(a));
                    Vector3 p0 = Origin + radial * (float)(HeartLayout.FinOuter(y0) + rr) + Vector3.up * (float)y0;
                    Vector3 p1 = Origin + radial * (float)(HeartLayout.FinOuter(y1) + rr) + Vector3.up * (float)y1;
                    NocWetRegistry.AddSource(p0, p1, rr, radiance, si);
                }
        }

        /// <summary>The hoops' inset into the shaft (GE's belt inset), beacon fins and heights (GE).</summary>
        public const double BeltInset = 0.3;
        public static readonly int[] BeaconFins = { 0, 3, 6, 9 };
        public static readonly double[] BeaconY = { 184.0, 276.0, 368.0, 440.0 };

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
            G4Kit.Add(ctx, root, "spirePlinth", Plinth(), lib != null ? lib.StoneWall : null, Origin);
            var fins = new MeshBuilder();
            var strips = new MeshBuilder();
            for (int j = 0; j < HeartLayout.FinCount; j++) { Fin(fins, j); Strip(strips, j); }
            G4Kit.Add(ctx, root, "spireFins", fins, lib != null ? lib.SteelPainted : null, Origin);
            // PA (LOOK_V2 10): the strip mix is kept, its base is MetalPaint (was ConcreteDark)
            Color strip = G4Kit.Mix(G4Kit.Lin(NocPal.NeonCyan), G4Kit.Lin(NocPal.SignWhite), 0.5);
            Material stripMat = lib != null ? G4Kit.Line(lib.Line, HeartLayout.StripWidth, G4Kit.Lin(NocPal.MetalPaint), G4Kit.Scale(strip, HeartLayout.StripGain), "Beat07_SpireStrip") : null;
            G4Kit.Add(ctx, root, "spireStrips", strips, stripMat, Origin);
            if (lib != null) AddStripSources(G4Kit.Scale(strip, HeartLayout.StripGain));   // HT (INT-12): the strips in the wet plaza
            // HT: the hoops and their breathing ring lights (one NocLine material per ring, driven by SpireDriver)
            G4Kit.Add(ctx, root, "spireHoops", Hoops(), lib != null ? lib.SteelPainted : null, Origin);
            var hoopY = HoopHeights();
            var ringMats = new Material[hoopY.Count];
            Color ringBase = G4Kit.Scale(G4Kit.Lin(NocPal.NeonCyan), HeartLayout.RingLightGain);   // HT-06: subordinate to the strips
            for (int k = 0; k < hoopY.Count; k++)
            {
                ringMats[k] = lib != null ? G4Kit.Line(lib.Line, HeartLayout.StripWidth, G4Kit.Lin(NocPal.MetalPaint), ringBase, "Beat07_SpireRing" + k) : null;
                G4Kit.Add(ctx, root, "spireRingLight" + k, RingLight(hoopY[k]), ringMats[k], Origin);
            }
            if (lib != null) ctx.Register(new SpireDriver(ringMats, ringBase));
            // GE: beacons (TEXTURE_PLAN 5 relief: with the v2 facade, ReliefBuilder.ReliefOn)
            if (!ReliefBuilder.ReliefOn) return;
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
