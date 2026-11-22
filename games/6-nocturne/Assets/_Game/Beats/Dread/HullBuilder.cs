// HullBuilder: the geometry of THE ARK (D-043, Logs/ark/ARK.md; owner DR), replacing DREAD 2.1's 1 800 m hull. Pure MeshBuilder
// output in the ship's local frame: x across (the rift axis at 0), y = world y, z along the body from the BOW (0, the leading south
// end) to the stern (5 000, north); DreadDriver places the frame at (515.3, 0, bow z). The ship moves toward -z (HullCourse).
// Silhouette first (seen from below and from ahead, 300 m to 5 km away):
//   - the body: an inverted ziggurat lofted over ~260 rings: a keel plate (half-width 70 m at y 330), seven steps up and out (rise
//     20 m, outsets 14..30 m, each riser leaning 3 m out so its decks face down-outward), the widest tread at y 470 (half-beam
//     220), a last 16 m riser and a boxy superellipse back to the spine at y 590 (inside the cloud deck at 460: it dissolves);
//     ribs every 100 m (two rings scaled out 2-3 m), a plan taper over the forward 700 m to a 22 m prow with the keel rising to
//     y 414 at the tip (the bow seen head-on is a stepped dark wedge), a slight stern taper and caps at both ends;
//   - the keel spine (24 x 8 m, y 322..330, z 520..4 760: where the keel no longer rises) and sixteen engine pods (r 8 m, 90 m long) along the keel plate, their aft (north)
//     faces carrying slow cold glows;
//   - 36 hanging gondolas (the size of the walker's own buildings: 16-22 m wide, 30-64 m long, 20-28 m tall) under the three outer
//     treads on four hangers each, never below y 380;
//   - vertex colour: R = AO, B = kind (0 hull plate: windows on the risers / portholes on the treads; 200 gondola: lit cabins;
//     100 spine / pods: no windows); uv0 = (local z, local y), uv1 = (local x, 0) for NocArk's procedural decks (4 m floors);
//   - the lights (DreadLights, NocDreadLight): two white rows under the spine, a lamp row on every tread edge (white inner, warm
//     outer, aviation red on the widest), the pods' cold glows and six stern engine halos.
// About 40 k triangles (DreadTests: the Ark < 250 k with its lights).
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Megastructure
{
    public static class HullBuilder
    {
        /// <summary>The stepped section (half): step half-widths w[k] at y[k] (keel plate k = 0 .. widest tread k = 7).</summary>
        public static readonly double[] StepW = { 70.0, 84.0, 100.0, 118.0, 140.0, 164.0, 190.0, 216.0 };
        public static readonly double[] StepY = { 330.0, 350.0, 370.0, 390.0, 410.0, 430.0, 450.0, 470.0 };
        public const double RiserLean = 3.0, TopRiser = 16.0, ArcSeg = 8, ArcP = 0.45;
        /// <summary>Cycle a01 (DR_a01): a 700 m taper to a 22 m prow read as an egg / a blimp from below and ahead. The bow is a WALL:
        /// 62 % of the beam at the stem (a 268 m wide stepped front with its decks lit), rounding out to the full beam over 320 m.</summary>
        public const double BowLen = 320.0, BowTip = 0.62, BowKeelK = 0.82, BowRise = 320.0, SternTaper = 0.12, SternLen = 300.0;
        public const double RibPitch = 100.0, RibX = 1.014, RibY = 1.02, RibPivotY = 470.0;
        public const double SpineHalf = 12.0, SpineZ0 = 520.0, SpineZ1 = 4760.0;
        public const double PodR = 8.0, PodLen = 90.0, PodX = 48.0, PodY = 322.0;
        public const int PodSeg = 12;
        public static readonly double[] PodZ = { 520.0, 1100.0, 1680.0, 2260.0, 2840.0, 3420.0, 4000.0, 4580.0 };
        public const double LowestY = 300.0;
        public const byte KindHull = 0, KindSpine = 100, KindGondola = 200;

        static double S(double a, double b, double x) => DreadClock.Smooth01(a, b, x);

        /// <summary>Plan scale of the section at z (0.62 at the stem, 1 from z 320 to 4 700, 0.88 at the stern).</summary>
        public static double PlanScale(double z)
        {
            double u = Math.Max(0.0, Math.Min(1.0, z / BowLen));
            double bow = BowTip + (1.0 - BowTip) * Math.Sqrt(Math.Max(0.0, 1.0 - (1.0 - u) * (1.0 - u)));
            double stern = 1.0 - SternTaper * S(HullCourse.Length - SternLen, HullCourse.Length, z);
            return bow * stern;
        }

        /// <summary>Vertical compression of the section below y 470 at z (the keel rises a little toward the stem: 0.82 at z 0).</summary>
        public static double KeelScale(double z) => BowKeelK + (1.0 - BowKeelK) * S(0.0, BowRise, z);

        /// <summary>Map a design-section point (x, y) to the section at z (plan taper, keel rise; and a rib's scale).</summary>
        static Vector2 MapPoint(double x, double y, double z, bool rib)
        {
            double sx = PlanScale(z), ky = KeelScale(z);
            double yy = y < RibPivotY ? RibPivotY - (RibPivotY - y) * ky : y;
            if (rib) { x *= RibX; yy = RibPivotY - (RibPivotY - yy) * RibY; }
            return new Vector2((float)(x * sx), (float)yy);
        }

        /// <summary>The design half-section polyline (x &gt;= 0) from the bottom centre (0, 330) to the spine (0, 590).</summary>
        public static List<Vector2> HalfSection()
        {
            var p = new List<Vector2>(32) { new Vector2(0f, (float)StepY[0]), new Vector2((float)StepW[0], (float)StepY[0]) };
            for (int k = 0; k < StepW.Length - 1; k++)
            {
                p.Add(new Vector2((float)(StepW[k] + RiserLean), (float)StepY[k + 1]));     // riser k (leaning out)
                p.Add(new Vector2((float)StepW[k + 1], (float)StepY[k + 1]));               // tread k (its underside faces down)
            }
            double y7 = StepY[StepY.Length - 1], w7 = StepW[StepW.Length - 1];
            double yb = y7 + TopRiser;
            p.Add(new Vector2((float)w7, (float)yb));
            for (int i = 1; i <= ArcSeg; i++)
            {
                double th = 0.5 * Math.PI * i / ArcSeg;
                double c = Math.Pow(Math.Cos(th), ArcP), s = Math.Pow(Math.Sin(th), ArcP);
                p.Add(new Vector2((float)(w7 * c), (float)(yb + (HullCourse.TopY - yb) * s)));
            }
            p[p.Count - 1] = new Vector2(0f, (float)HullCourse.TopY);
            return p;
        }

        /// <summary>The full closed section's segments (pairs of design points), right half then the mirrored left half, outward normal
        /// = the clockwise rotation of the segment direction on the right half.</summary>
        public static List<Vector2[]> Segments()
        {
            var h = HalfSection();
            var segs = new List<Vector2[]>(64);
            // keel plate as one segment across (from -w0 to +w0)
            segs.Add(new[] { new Vector2(-h[1].x, h[1].y), h[1] });
            for (int i = 1; i < h.Count - 1; i++) segs.Add(new[] { h[i], h[i + 1] });
            for (int i = h.Count - 2; i >= 1; i--) segs.Add(new[] { new Vector2(-h[i + 1].x, h[i + 1].y), new Vector2(-h[i].x, h[i].y) });
            return segs;
        }

        /// <summary>Ring stations (z, rib): every 35 m over the prow taper, then per 100 m cell rings at +0, +46, +48 (rib), +52 (rib), +54,
        /// a slight stern taper, the stern at 5 000.</summary>
        public static List<KeyValuePair<double, bool>> RingStations()
        {
            var list = new List<KeyValuePair<double, bool>>(300);
            double[] bow = { 0.0, 3.0, 9.0, 18.0, 30.0, 45.0, 64.0, 87.0, 114.0, 145.0, 180.0, 220.0, 265.0, 315.0 };
            foreach (double z in bow) list.Add(new KeyValuePair<double, bool>(z, false));
            for (double z0 = 400.0; z0 < HullCourse.Length - 1e-6; z0 += RibPitch)
            {
                list.Add(new KeyValuePair<double, bool>(z0, false));
                if (z0 + 54.0 >= HullCourse.Length) continue;
                list.Add(new KeyValuePair<double, bool>(z0 + 46.0, false));
                list.Add(new KeyValuePair<double, bool>(z0 + 48.0, true));
                list.Add(new KeyValuePair<double, bool>(z0 + 52.0, true));
                list.Add(new KeyValuePair<double, bool>(z0 + 54.0, false));
            }
            list.Add(new KeyValuePair<double, bool>(HullCourse.Length, false));
            return list;
        }

        /// <summary>Triangle a, b, c wound so its winding normal agrees with the mean vertex normal (MeshValidation).</summary>
        public static void TriO(MeshBuilder mb, int a, int b, int c)
        {
            Vector3 pa = mb.Positions[a];
            Vector3 cr = Vector3.Cross(mb.Positions[b] - pa, mb.Positions[c] - pa);
            Vector3 m = mb.Normals[a] + mb.Normals[b] + mb.Normals[c];
            if (Vector3.Dot(cr, m) < 0f) mb.Tri(a, c, b); else mb.Tri(a, b, c);
        }

        /// <summary>A flat quad facing away from the point inside.</summary>
        public static void OutQuad(MeshBuilder mb, Vector3 a, Vector3 b, Vector3 c, Vector3 d, Vector3 inside)
        {
            Vector3 n = Vector3.Cross(b - a, c - a);
            Vector3 ctr = 0.25f * (a + b + c + d);
            if (Vector3.Dot(n, ctr - inside) < 0f) mb.FlatQuad(a, d, c, b); else mb.FlatQuad(a, b, c, d);
        }

        static int V(MeshBuilder mb, Vector3 p, Vector3 n)
        {
            mb.CurrentUV1 = new Vector4(p.x, 0f, 0f, 0f);
            return mb.Vertex(p, n, new Vector2(p.z, p.y));
        }

        /// <summary>A box (outward normals, flat faces) with the ship's uv layout.</summary>
        static void BoxUV(MeshBuilder mb, Vector3 lo, Vector3 hi)
        {
            Vector3 c = 0.5f * (lo + hi);
            Vector3[] P =
            {
                new Vector3(lo.x, lo.y, lo.z), new Vector3(hi.x, lo.y, lo.z), new Vector3(hi.x, hi.y, lo.z), new Vector3(lo.x, hi.y, lo.z),
                new Vector3(lo.x, lo.y, hi.z), new Vector3(hi.x, lo.y, hi.z), new Vector3(hi.x, hi.y, hi.z), new Vector3(lo.x, hi.y, hi.z),
            };
            int[,] F = { { 0, 1, 2, 3 }, { 5, 4, 7, 6 }, { 4, 0, 3, 7 }, { 1, 5, 6, 2 }, { 4, 5, 1, 0 }, { 3, 2, 6, 7 } };
            for (int f = 0; f < 6; f++)
            {
                Vector3 a = P[F[f, 0]], b = P[F[f, 1]], cc = P[F[f, 2]], d = P[F[f, 3]];
                Vector3 n = Vector3.Cross(b - a, cc - a).normalized;
                if (Vector3.Dot(n, 0.25f * (a + b + cc + d) - c) < 0f) n = -n;
                int ia = V(mb, a, n), ib = V(mb, b, n), ic = V(mb, cc, n), id = V(mb, d, n);
                TriO(mb, ia, ib, ic);
                TriO(mb, ia, ic, id);
            }
        }

        /// <summary>The Ark: body, spine, pods and gondolas in the local frame.</summary>
        public static MeshBuilder BuildHull()
        {
            var mb = new MeshBuilder { WriteUV1 = true };
            var rings = RingStations();
            var segs = Segments();
            int M = segs.Count;
            int[][] prev = null;
            var first = new List<Vector3>(2 * M);
            var last = new List<Vector3>(2 * M);
            for (int k = 0; k < rings.Count; k++)
            {
                double z = rings[k].Key;
                bool rib = rings[k].Value;
                double dz = 1.0;
                var cur = new int[M][];
                for (int s = 0; s < M; s++)
                {
                    cur[s] = new int[2];
                    Vector2 d0 = segs[s][0], d1 = segs[s][1];
                    Vector2 a = MapPoint(d0.x, d0.y, z, rib), b = MapPoint(d1.x, d1.y, z, rib);
                    Vector2 t = b - a;
                    var n2 = new Vector2(t.y, -t.x);
                    if (n2.sqrMagnitude < 1e-12f) n2 = new Vector2(0f, -1f);
                    n2.Normalize();
                    for (int e = 0; e < 2; e++)
                    {
                        Vector2 dd = e == 0 ? d0 : d1;
                        Vector2 p = e == 0 ? a : b;
                        Vector2 pa = MapPoint(dd.x, dd.y, Math.Max(0.0, z - dz), rib), pb = MapPoint(dd.x, dd.y, Math.Min(HullCourse.Length, z + dz), rib);
                        double span = Math.Min(HullCourse.Length, z + dz) - Math.Max(0.0, z - dz);
                        Vector2 dpdz = (pb - pa) / (float)span;
                        var n3 = new Vector3(n2.x, n2.y, -(n2.x * dpdz.x + n2.y * dpdz.y)).normalized;
                        mb.Color = new Color32(StepAO(d0, d1, dd, rib), 0, KindHull, 255);
                        var pos = new Vector3(p.x, p.y, (float)z);
                        cur[s][e] = V(mb, pos, n3);
                        if (k == 0) first.Add(pos);
                        if (k == rings.Count - 1) last.Add(pos);
                    }
                }
                if (prev != null)
                    for (int s = 0; s < M; s++)
                    {
                        TriO(mb, prev[s][0], prev[s][1], cur[s][1]);
                        TriO(mb, prev[s][0], cur[s][1], cur[s][0]);
                    }
                prev = cur;
            }
            mb.Color = new Color32(200, 0, KindHull, 255);
            Cap(mb, first, 0.0, -1f);
            Cap(mb, last, HullCourse.Length, 1f);
            Spine(mb);
            for (int i = 0; i < PodZ.Length; i++) { Pod(mb, PodX, PodZ[i]); Pod(mb, -PodX, PodZ[i] + 40.0); }
            Gondolas(mb);
            mb.Color = new Color32(255, 0, 0, 255);
            return mb;
        }

        /// <summary>Vertex AO of a section point (cycle a01: from below the steps read as one dark plane): a tread's inner end 0.5, its
        /// outer edge 1; the keel plate 0.92; a riser's foot 1, its head (under the tread above) 0.58; the back 0.8; ribs a touch lighter.</summary>
        static byte StepAO(Vector2 d0, Vector2 d1, Vector2 p, bool rib)
        {
            double ao;
            bool flat = Math.Abs(d0.y - d1.y) < 0.01f;
            if (flat && d0.y <= StepY[StepY.Length - 1] + 0.01)
            {
                if (Math.Abs(d0.y - StepY[0]) < 0.01f) ao = 0.92;
                else ao = Math.Abs(p.x) < Math.Max(Math.Abs(d0.x), Math.Abs(d1.x)) - 0.01f ? 0.50 : 1.0;
            }
            else if (Math.Abs(d0.x - d1.x) < 4.0f && p.y <= StepY[StepY.Length - 1] + TopRiser + 0.01)
                ao = p.y > Math.Min(d0.y, d1.y) + 0.01f ? 0.58 : 1.0;
            else ao = 0.8;
            if (rib) ao = Math.Min(1.0, ao + 0.12);
            return (byte)Math.Round(255.0 * ao);
        }

        static void Cap(MeshBuilder mb, List<Vector3> pts, double z, float nz)
        {
            var n = new Vector3(0f, 0f, nz);
            Vector2 c2 = MapPoint(0.0, 460.0, z, false);
            mb.Color = new Color32(200, 0, KindHull, 255);
            int c = V(mb, new Vector3(0f, c2.y, (float)z), n);
            var idx = new int[pts.Count];
            for (int i = 0; i < pts.Count; i++) idx[i] = V(mb, pts[i], n);
            // segments are consecutive pairs (2 points each)
            for (int i = 0; i + 1 < pts.Count; i += 2) TriO(mb, c, idx[i], idx[i + 1]);
        }

        /// <summary>Keel spine: a box 24 m wide from y 322 to the keel plate (330), z 260..4 760, tapered ends (ramped 40 m).</summary>
        static void Spine(MeshBuilder mb)
        {
            mb.Color = new Color32(210, 0, KindSpine, 255);
            const double seg = 250.0;
            for (double z0 = SpineZ0; z0 < SpineZ1 - 1e-6; z0 += seg)
            {
                double z1 = Math.Min(SpineZ1, z0 + seg);
                BoxUV(mb, new Vector3((float)-SpineHalf, (float)HullCourse.KeelY, (float)z0), new Vector3((float)SpineHalf, (float)(StepY[0] + 0.5), (float)(z1 - 2.0)));
            }
        }

        /// <summary>An engine pod: a 12-sided cylinder (r 8) along z under the keel plate, centre y 322, from z0 to z0 + 90; capped.</summary>
        static void Pod(MeshBuilder mb, double x, double z0)
        {
            mb.Color = new Color32(215, 0, KindSpine, 255);
            int N = PodSeg;
            float z1 = (float)(z0 + PodLen);
            var ra = new int[N]; var rb = new int[N];
            for (int i = 0; i < N; i++)
            {
                double a = 2.0 * Math.PI * (i + 0.5) / N;
                var d = new Vector3((float)Math.Cos(a), (float)Math.Sin(a), 0f);
                var p = new Vector3((float)(x + PodR * d.x), (float)(PodY + PodR * d.y), (float)z0);
                ra[i] = V(mb, p, d);
                rb[i] = V(mb, new Vector3(p.x, p.y, z1), d);
            }
            for (int i = 0; i < N; i++) { int j = (i + 1) % N; TriO(mb, ra[i], ra[j], rb[j]); TriO(mb, ra[i], rb[j], rb[i]); }
            for (int end = 0; end < 2; end++)
            {
                float zz = end == 0 ? (float)z0 : z1;
                var n = new Vector3(0f, 0f, end == 0 ? -1f : 1f);
                int c = V(mb, new Vector3((float)x, (float)PodY, zz), n);
                var ring = new int[N];
                for (int i = 0; i < N; i++)
                {
                    double a = 2.0 * Math.PI * (i + 0.5) / N;
                    ring[i] = V(mb, new Vector3((float)(x + PodR * Math.Cos(a)), (float)(PodY + PodR * Math.Sin(a)), zz), n);
                }
                for (int i = 0; i < N; i++) TriO(mb, c, ring[i], ring[(i + 1) % N]);
            }
        }

        /// <summary>The gondolas: (tread k, side, local z, length, height, hang). Under tread k (y StepY[k + 1], x from StepW[k] + lean to
        /// StepW[k + 1]): width = the tread minus 2.5 m a side.</summary>
        public static List<double[]> GondolaSpecs()
        {
            var g = new List<double[]>(40);
            int idx = 0;
            for (int k = 4; k <= 6; k++)
                for (int side = -1; side <= 1; side += 2)
                    for (int j = 0; j < 6; j++)
                    {
                        uint h = DreadCabinCourse.H3((uint)k, (uint)(side + 1), (uint)j);
                        double u = DreadCabinCourse.U01(h), u2 = DreadCabinCourse.U01(DreadCabinCourse.H3((uint)j, (uint)k, 0x60D0u + (uint)(side + 1)));
                        double z = 900.0 + j * 640.0 + (k - 4) * 190.0 + (side > 0 ? 0.0 : 310.0) + 120.0 * (u - 0.5);
                        double len = 30.0 + 34.0 * u2;
                        double hgt = 20.0 + 8.0 * u;
                        double hang = 8.0 + 8.0 * u2;
                        if (z + len > HullCourse.Length - SternLen - 20.0) continue;
                        g.Add(new double[] { k, side, z, len, hgt, hang, idx++ });
                    }
            return g;
        }

        /// <summary>Cycle a03: the keel gondolas, a second, lower layer of hanging blocks (26 x 50 x 22 m, 4 m under the keel plate, bottom
        /// y 304) between the spine and the pods, half-way between the hangar bays (local z 940 + 400 k): from the bridge looking north
        /// they stand against the lit bays and the porthole runs as layered silhouettes, and they are the walker's buildings in size.</summary>
        public const double KeelGondolaX0 = 14.0, KeelGondolaX1 = 39.0, KeelGondolaLen = 50.0, KeelGondolaH = 22.0, KeelGondolaHang = 4.0;
        public static List<double> KeelGondolaZ()
        {
            var z = new List<double>(24);
            for (double c = 940.0; c + KeelGondolaLen < 4500.0; c += 400.0) z.Add(c - 0.5 * KeelGondolaLen);
            return z;
        }

        static void Gondolas(MeshBuilder mb)
        {
            foreach (double z in KeelGondolaZ())
                for (int side = -1; side <= 1; side += 2)
                {
                    double zz = z + (side > 0 ? 0.0 : 120.0);
                    float x0 = (float)(side * KeelGondolaX0), x1 = (float)(side * KeelGondolaX1);
                    float yTop = (float)(StepY[0] - KeelGondolaHang), yBot = (float)(yTop - KeelGondolaH);
                    mb.Color = new Color32(235, 0, KindGondola, 255);
                    BoxUV(mb, new Vector3(Math.Min(x0, x1), yBot, (float)zz), new Vector3(Math.Max(x0, x1), yTop, (float)(zz + KeelGondolaLen)));
                    mb.Color = new Color32(200, 0, KindSpine, 255);
                    float xm = 0.5f * (x0 + x1);
                    for (int c = 0; c < 2; c++)
                    {
                        float cz = (float)(zz + (c == 0 ? 0.2 : 0.8) * KeelGondolaLen);
                        BoxUV(mb, new Vector3(xm - 2f, yTop, cz - 2f), new Vector3(xm + 2f, (float)(StepY[0] + 0.5), cz + 2f));
                    }
                }
            foreach (var g in GondolaSpecs())
            {
                int k = (int)g[0], side = (int)g[1];
                double z = g[2], len = g[3], hgt = g[4], hang = g[5];
                double xIn = StepW[k] + RiserLean + 2.5, xOut = StepW[k + 1] - 2.5;
                double yTop = StepY[k + 1] - hang, yBot = yTop - hgt;
                float x0 = (float)(side * xIn), x1 = (float)(side * xOut);
                mb.Color = new Color32(235, 0, KindGondola, 255);
                BoxUV(mb, new Vector3(Math.Min(x0, x1), (float)yBot, (float)z), new Vector3(Math.Max(x0, x1), (float)yTop, (float)(z + len)));
                // the roof frame: a slab 2 m thick, 1 m proud, and four 3 m hangers up to the tread
                mb.Color = new Color32(200, 0, KindSpine, 255);
                BoxUV(mb, new Vector3(Math.Min(x0, x1) - 1f, (float)yTop, (float)z - 1f), new Vector3(Math.Max(x0, x1) + 1f, (float)(yTop + 2.0), (float)(z + len + 1.0)));
                float xm = 0.5f * (x0 + x1), hx = 0.32f * Math.Abs(x1 - x0);
                for (int c = 0; c < 4; c++)
                {
                    float cx = xm + ((c & 1) == 0 ? -hx : hx);
                    float cz = (float)(z + ((c & 2) == 0 ? 0.18 : 0.82) * len);
                    BoxUV(mb, new Vector3(cx - 1.5f, (float)(yTop + 2.0), cz - 1.5f), new Vector3(cx + 1.5f, (float)(StepY[k + 1] + 0.5), cz + 1.5f));
                }
            }
        }

        // ------------------------------------------------------------------ lights
        /// <summary>Lamp rows: the spine's two white rows (pitch 30 m), the tread edges (pitch 24 m, staggered), the widest tread red
        /// (48 m); the pods' cold aft glows; six stern engine halos.</summary>
        public static List<DreadLight> BuildLights()
        {
            var list = new List<DreadLight>(4096);
            for (double z = SpineZ0 + 10.0; z <= SpineZ1 - 10.0; z += SpinePitch)
                for (int sgn = -1; sgn <= 1; sgn += 2)
                    list.Add(new DreadLight(new Vector3((float)(sgn * (SpineHalf - 1.0)), (float)(HullCourse.KeelY - 0.8), (float)z), SpineLampSize, SpineLampGain, DreadLights.White));
            for (int k = 0; k < StepW.Length - 1; k++)
            {
                bool red = k == StepW.Length - 2;
                double pitch = red ? 2.0 * TreadPitch : TreadPitch;
                int col = red ? DreadLights.Red : (k < 3 ? DreadLights.White : DreadLights.Warm);
                float gain = red ? RedGain : (k < 3 ? TreadWhiteGain : TreadWarmGain);
                float size = red ? RedSize : TreadLampSize;
                for (double z = 30.0 + 6.0 * k; z <= HullCourse.Length - 20.0; z += pitch)
                    for (int sgn = -1; sgn <= 1; sgn += 2)
                    {
                        Vector2 p = MapPoint(StepW[k + 1] - 0.6, StepY[k + 1] - 0.9, z, false);
                        list.Add(new DreadLight(new Vector3(sgn * p.x, p.y, (float)z), size, gain, col));
                    }
            }
            // the ribs' cross rows on the keel plate (cycle a01: the belly read as a starfield; the ribs make it a structure)
            for (double z0 = 400.0; z0 + 54.0 < HullCourse.Length; z0 += RibPitch)
                foreach (double x in RibLampX)
                {
                    Vector2 p = MapPoint(x, StepY[0], z0 + 50.0, true);
                    list.Add(new DreadLight(new Vector3(p.x, p.y - 1.0f, (float)(z0 + 50.0)), TreadLampSize, RibLampGain, DreadLights.Warm));
                }
            // the bow wall: a lamp row along every step's front edge (the decks of the stem: a lit wall ahead)
            for (int k = 0; k < StepW.Length; k++)
            {
                double w = StepW[k] * PlanScale(0.0);
                Vector2 py = MapPoint(0.0, StepY[k], 0.0, false);
                int n = (int)Math.Floor(2.0 * w / BowLampPitch);
                for (int j = 0; j <= n; j++)
                {
                    double x = -w + 2.0 * w * j / Math.Max(n, 1);
                    bool red = k == StepW.Length - 1;
                    list.Add(new DreadLight(new Vector3((float)x, py.y - 0.9f, -0.8f), red ? RedSize : TreadLampSize, red ? RedGain : BowLampGain,
                                            red ? DreadLights.Red : ((j & 1) == 0 ? DreadLights.White : DreadLights.Warm)));
                }
            }
            for (int i = 0; i < PodZ.Length; i++)
            {
                list.Add(new DreadLight(new Vector3((float)PodX, (float)PodY, (float)(PodZ[i] + PodLen + 1.5)), PodGlowSigma, PodGlowGain, DreadLights.Cold, true));
                list.Add(new DreadLight(new Vector3((float)-PodX, (float)PodY, (float)(PodZ[i] + 40.0 + PodLen + 1.5)), PodGlowSigma, PodGlowGain, DreadLights.Cold, true));
            }
            // six stern engine glows in two rows low on the stern wall (cycle a04: at y 470 they sat in the cloud mix)
            double sx = PlanScale(HullCourse.Length);
            for (int e = 0; e < 6; e++)
            {
                double x = (-100.0 + 40.0 * e) * sx;
                list.Add(new DreadLight(new Vector3((float)x, e % 2 == 0 ? 372f : 402f, (float)(HullCourse.Length + 3.0)), SternSigma, SternGain, DreadLights.Cold, true));
            }
            // the bow's two lower corners: steady white beacons (the stem's width read from 3 km; never blinking, D-010)
            for (int sgn = -1; sgn <= 1; sgn += 2)
            {
                Vector2 c = MapPoint(StepW[0] - 1.0, StepY[0], 0.0, false);
                list.Add(new DreadLight(new Vector3(sgn * c.x, c.y - 1.5f, -2.0f), BeaconSigma, BeaconGain, DreadLights.White, true));
            }
            return list;
        }

        public const double SpinePitch = 30.0, TreadPitch = 12.0;   // cycle a02: 24 m dots read as a dotted wireframe; 12 m reads as lit edges
        public const float SpineLampSize = 2.2f, SpineLampGain = 1.6f, TreadLampSize = 1.8f, TreadWhiteGain = 0.8f, TreadWarmGain = 0.9f;
        public const float RedSize = 2.6f, RedGain = 2.4f;
        /// <summary>The pods' cold aft glows (sigma 5 m, gain 0.5) and the stern engines (sigma 18 m, gain 0.22: lost in the haze).</summary>
        public const float PodGlowSigma = 2.0f, PodGlowGain = 0.25f, SternSigma = 12.0f, SternGain = 0.22f;
        /// <summary>The bow's corner beacons (steady; cycle a05: sigma 3 m / 0.6 read as two headlights, now 1.4 m / 0.45).</summary>
        public const float BeaconSigma = 1.4f, BeaconGain = 0.45f;
        /// <summary>Rib cross rows (x on the keel plate, clear of the spine and the pods), the bow wall's rows (pitch 10 m).</summary>
        public static readonly double[] RibLampX = { -64.0, -30.0, -18.0, 18.0, 30.0, 64.0 };
        public const float RibLampGain = 0.9f, BowLampGain = 1.2f;
        public const double BowLampPitch = 10.0;

        /// <summary>The half-width (m) of the widest tread at z (the beam / 2 x the plan scale).</summary>
        public static double HalfBeamAt(double z) => StepW[StepW.Length - 1] * PlanScale(z);

        /// <summary>The deck veil quad (y 456) about the ship's middle (off: HullCourse.VeilOn).</summary>
        public static MeshBuilder BuildVeil()
        {
            var mb = new MeshBuilder();
            float y = (float)HullCourse.VeilY, hw = (float)HullCourse.VeilHalfW, hl = (float)HullCourse.VeilHalfL, zc = (float)(0.5 * HullCourse.Length);
            mb.FlatQuad(new Vector3(-hw, y, zc - hl), new Vector3(hw, y, zc - hl), new Vector3(hw, y, zc + hl), new Vector3(-hw, y, zc + hl));
            return mb;
        }
    }
}
