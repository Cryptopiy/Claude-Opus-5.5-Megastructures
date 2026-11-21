// ReefLayout: the one placement function of the beat 4 reef (MASTER_PROMPT 14.5.4 "one function ReefUnitAt(cell) in C# and
// HLSL feeds islets, belt, horns and foam"). Pure and deterministic (PCG hashes of BwMath, twins of BwCommon.hlsl BwPcg*),
// no UnityEngine objects are created; EditMode tests and the kraken builder (islet hulls, Addendum A1 1.4) read it.
//
// The unit "Colossopod": four arms on the tetrahedral directions (109.47 deg apart), each a truncated cone from radius 6.0 at
// the centre to 3.6 at its end face, centre-to-end-face L = 40 / (2 sin(54.7356 deg)) = 40 / 1.63299 = 24.495 m, so the
// distance between any two end-face centres ("tip to tip") is exactly 40.0 m.
//
// Grid (14.5.4): 28 m cells, jitter +-8 m, random orientation (uniform quaternion from three hashes), three layers (layer 0 the
// top, layers 1 and 2 14 m and 28 m deeper: always under the troughs, so never built). Rows j 0-11 cover the southern reef
// z 704-1040 (336 = 12 x 28), rows 12-48 the northern reef z 1560-2596; columns i 0-56 cover x 6-1602 (clipped at 1600).
// The top tip of a layer-0 unit lies in y -14..-1.5 (nothing above -1.5: "awash"), skewed to the top (TipSkew). Cells whose centre lies inside the reef
// channel (x 900-1100, z 704-1040, Addendum A1 1.4) or within an islet's radius hold no grid unit (the islet's own units
// stand there). A unit never enters the sea face (x 6 to y -20, then the 1:8 batter) or the last spillway pier (z 700-704,
// x 6-46): it is pushed east / north until 0.5 m clear (only the parts above y -14 matter: the sea hides the rest).
//
// Islets (14.5.4): six piles at the crest centres of BulwarkConst.IsletX/Z, crest y {22, 18, 14, 20, 12, 16} and radius
// {45, 55, 50, 60, 65, 70} (the graybox stand-ins the kraken builder works against; islet 1 r 45 per the Wave A audit),
// {13, 16, 14, 17, 15, 19} whole units in three layers: a top unit whose highest point is the crest, a middle ring at 0.36 R
// (tops 7-11 m below the crest) and an outer ring at R - 16 (tops -1..+6 m, awash). Each unit is the best of 24 hashed
// candidates (least interpenetration with the units already placed, capsule distances), so arms rest on arms.
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Megastructure.Beats
{
    public static class ReefLayout
    {
        // ---------------------------------------------------------------- the unit
        public const float TipToTip = 40.0f;
        /// <summary>Centre to end-face centre (m): 40 / sqrt(8/3).</summary>
        public static readonly float ArmLength = TipToTip / Mathf.Sqrt(8f / 3f);
        public const float ArmR0 = 6.0f, ArmR1 = 3.6f;
        /// <summary>End-face chamfer (m, 45 deg).</summary>
        public const float EndChamfer = 0.35f;
        /// <summary>Hub sphere radius (m): rounds the crotches between the arms.</summary>
        public const float HubRadius = 6.3f;

        /// <summary>Arm directions in the unit frame: one up, three at y -1/3 (azimuths 0, 120, 240 deg).</summary>
        public static readonly Vector3[] ArmDir =
        {
            new Vector3(0f, 1f, 0f),
            new Vector3(0.9428090f, -1f / 3f, 0f),
            new Vector3(-0.4714045f, -1f / 3f, 0.8164966f),
            new Vector3(-0.4714045f, -1f / 3f, -0.8164966f),
        };

        public static float ArmRadiusAt(float t) => ArmR0 + (ArmR1 - ArmR0) * Mathf.Clamp01(t / ArmLength);

        // ---------------------------------------------------------------- the grid
        public const float Cell = 28.0f, Jitter = 8.0f;
        public const float GridX0 = 6.0f;
        public const int Columns = 57;
        public const int SouthRows = 12, NorthRows = 37, Rows = SouthRows + NorthRows;
        public const float SouthZ0 = 704.0f, NorthZ0 = 1560.0f;
        public const float TipYMin = -14.0f, TipYMax = -1.5f;
        /// <summary>Tips skew to the top: tip = -2.3 - 11.7 u^2.2 (inside the spec's -14..-1.5). 14.5.4's KS10 px list (173 px
        /// at 60 m, 132 at 80, 89 at 120, 54 at 200, 22 at 500, 11 at 1 km; 4K vFOV 64 = 33.75 px/deg) means the tallest tip of
        /// any 20 m distance band stands 5.4-5.7 m above the mean sea: the highest tip is capped at y -2.3 (5.7 m = the 1 km
        /// value, 11 px = 0.326 deg) and the draw is skewed so the tallest of the ~3.4 units of the 80 m band lies at
        /// -2.3 - 11.7 x (1 / 4.4)^2.2 = -2.76 (5.24 m, 125 px) and of the ~8.6 units of the 200 m band at -2.38 (5.62 m, 54 px).
        /// A uniform draw to -1.5 gave the realised 80 px (reef tests02: top -4.68 at 80 m).</summary>
        public const float TipSkew = 2.2f, TipTop = -2.3f;
        public const float LayerDrop = 14.0f;
        public const int Layers = 3;
        /// <summary>Triangles whose three vertices lie below this y are never built (the deepest trough is about -11.5).</summary>
        public const float ClipY = -13.0f;
        public const float ChannelX0 = 900f, ChannelX1 = 1100f, ChannelZ0 = 704f, ChannelZ1 = 1040f;
        public const float BeltX1 = 140f;
        /// <summary>Units nearer than this to the wall use LOD0, farther ones LOD1. 14.5.4 puts the unit-to-horn crossover at
        /// 400 m; there an arm (r 3.6 m) is 2 x 3.6 / 400 rad = 35 px wide at 4K and LOD1's octagon shows its facets, so LOD0
        /// runs to 600 m (23 px; most units there show only a clipped tip of about 200 triangles).</summary>
        public const float Lod0MaxX = 600f;
        public const uint SaltGrid = 0x52454546u;   // "REEF"
        public const uint SaltIslet = 0x49534C54u;  // "ISLT"

        public static float RowZ0(int j) => j < SouthRows ? SouthZ0 + Cell * j : NorthZ0 + Cell * (j - SouthRows);
        public static Vector2 CellCentre(int i, int j) => new Vector2(GridX0 + Cell * (i + 0.5f), RowZ0(j) + 0.5f * Cell);

        // ---------------------------------------------------------------- islets
        public static readonly float[] IsletCrestY = { 22f, 18f, 14f, 20f, 12f, 16f };
        public static readonly float[] IsletRadius = { 45f, 55f, 50f, 60f, 65f, 70f };
        public static readonly int[] IsletUnits = { 13, 16, 14, 17, 15, 19 };
        public static int IsletCount => IsletCrestY.Length;
        public static Vector2 IsletCentre(int k) => new Vector2((float)BulwarkConst.IsletX[k], (float)BulwarkConst.IsletZ[k]);

        public enum Kind { Belt = 0, Horn = 1, Islet = 2 }

        public struct Unit
        {
            public Vector3 centre;
            public Quaternion rot;
            public Kind kind;
            public int islet;        // -1 for grid units
            public int cellI, cellJ;
            public int topArm;       // index of the highest arm
            public bool gauge;       // carries a gauge platform on its top arm (banded arm)
            public Vector3 Tip(int a) => centre + rot * (ArmDir[a] * ArmLength);
            public Vector3 Dir(int a) => rot * ArmDir[a];
            public float TopY => TopPoint(this);
        }

        static float U(uint h) => BwMath.U01(h);

        /// <summary>Uniform random rotation from three uniforms (Shoemake).</summary>
        public static Quaternion RandomRotation(float u1, float u2, float u3)
        {
            float a = Mathf.Sqrt(1f - u1), b = Mathf.Sqrt(u1);
            float t1 = 2f * Mathf.PI * u2, t2 = 2f * Mathf.PI * u3;
            var q = new Quaternion(a * Mathf.Sin(t1), a * Mathf.Cos(t1), b * Mathf.Sin(t2), b * Mathf.Cos(t2));
            return Normalize(q);
        }

        static Quaternion Normalize(Quaternion q)
        {
            float m = Mathf.Sqrt(q.x * q.x + q.y * q.y + q.z * q.z + q.w * q.w);
            return m > 1e-8f ? new Quaternion(q.x / m, q.y / m, q.z / m, q.w / m) : Quaternion.identity;
        }

        /// <summary>Highest point of a unit (the rim of its highest end face, or the hub sphere).</summary>
        public static float TopPoint(in Unit u) => u.centre.y + TopReach(u.rot, out _);

        /// <summary>Height of the unit's highest point above its centre for rotation q; arm = the highest arm.</summary>
        public static float TopReach(Quaternion q, out int arm)
        {
            float best = HubRadius;
            arm = 0;
            float bestDy = -2f;
            for (int a = 0; a < 4; a++)
            {
                Vector3 d = q * ArmDir[a];
                float rim = ArmR1 * Mathf.Sqrt(Mathf.Max(0f, 1f - d.y * d.y));
                float h = d.y * ArmLength + rim;
                if (h > best) best = h;
                if (d.y > bestDy) { bestDy = d.y; arm = a; }
            }
            return best;
        }

        /// <summary>Layer-0 grid unit of cell (i, j), or false when the cell holds none (outside the reef, the channel, an islet).</summary>
        public static bool ReefUnitAt(int i, int j, out Unit u) => ReefUnitAt(i, j, 0, out u);

        public static bool ReefUnitAt(int i, int j, int layer, out Unit u)
        {
            u = default;
            if (i < 0 || i >= Columns || j < 0 || j >= Rows || layer < 0 || layer >= Layers) return false;
            Vector2 c = CellCentre(i, j);
            if (c.x > (float)BulwarkConst.ReefX1) return false;
            if (j < SouthRows && c.x > ChannelX0 && c.x < ChannelX1 && c.y > ChannelZ0 && c.y < ChannelZ1) return false;
            for (int k = 0; k < IsletCount; k++)
                if ((c - IsletCentre(k)).magnitude < IsletRadius[k]) return false;
            uint h0 = BwMath.Pcg3(i, j, (int)(SaltGrid + (uint)layer));
            uint h1 = BwMath.Pcg(h0), h2 = BwMath.Pcg(h1), h3 = BwMath.Pcg(h2), h4 = BwMath.Pcg(h3), h5 = BwMath.Pcg(h4);
            float x = c.x + Jitter * (2f * U(h0) - 1f);
            float z = c.y + Jitter * (2f * U(h1) - 1f);
            Quaternion q = RandomRotation(U(h2), U(h3), U(h4));
            float tip = TipTop - (TipTop - TipYMin) * Mathf.Pow(U(h5), TipSkew) - LayerDrop * layer;
            float reach = TopReach(q, out int top);
            u.centre = new Vector3(x, tip - reach, z);
            u.rot = q;
            u.kind = c.x < BeltX1 ? Kind.Belt : Kind.Horn;
            u.islet = -1;
            u.cellI = i; u.cellJ = j;
            u.topArm = top;
            u.gauge = false;
            PushClear(ref u);
            return true;
        }

        // ---------------------------------------------------------------- keep-out: sea face, batter, last spillway pier
        /// <summary>x of the sea face at height y (vertical to y -20, then the 1:8 batter).</summary>
        public static float SeaFaceX(float y) => (float)BulwarkConst.SeaFaceX + Mathf.Max(0f, ((float)BulwarkConst.SeaFaceBatterY - y) / (float)BulwarkConst.SeaFaceBatter);

        /// <summary>Pushes a unit east (off the sea face) and north (off the last spillway pier z 700-704, x &lt; 46) until its
        /// visible parts (y &gt; -14) are 0.5 m clear.</summary>
        static void PushClear(ref Unit u)
        {
            for (int pass = 0; pass < 2; pass++)
            {
                float dx = 0f, dz = 0f;
                for (int a = 0; a < 4; a++)
                {
                    Vector3 d = u.Dir(a);
                    for (int s = 0; s <= 6; s++)
                    {
                        float t = ArmLength * s / 6f;
                        Vector3 p = u.centre + d * t;
                        if (p.y < -14f) continue;
                        float r = ArmRadiusAt(t);
                        dx = Mathf.Max(dx, SeaFaceX(p.y) + 0.5f + r - p.x);
                        if (p.x - r < (float)BulwarkConst.PierEastX + 0.5f && p.z - r < (float)BulwarkConst.SpillwayZ1 + 0.5f && p.y < 1.0f)
                            dz = Mathf.Max(dz, (float)BulwarkConst.SpillwayZ1 + 0.5f + r - p.z);
                    }
                }
                if (dx <= 0f && dz <= 0f) break;
                u.centre += new Vector3(Mathf.Max(dx, 0f), 0f, Mathf.Max(dz, 0f));
            }
        }

        // ---------------------------------------------------------------- the islet piles
        static List<Unit>[] s_islets;

        public static IReadOnlyList<Unit> Islet(int k)
        {
            if (s_islets == null)
            {
                var all = new List<Unit>[IsletCount];
                for (int i = 0; i < IsletCount; i++) all[i] = BuildIslet(i);
                s_islets = all;
            }
            return s_islets[k];
        }

        static List<Unit> BuildIslet(int k)
        {
            var list = new List<Unit>(IsletUnits[k]);
            Vector2 c = IsletCentre(k);
            float R = IsletRadius[k], crest = IsletCrestY[k];
            int n = IsletUnits[k];
            int nMid = Mathf.RoundToInt((n - 1) * 0.36f), nOut = n - 1 - nMid;
            // top: the highest point exactly at the crest; candidates near the centre
            Place(list, k, 0, 0, c, 0f, 0f, crest, crest, true);
            for (int m = 0; m < nMid; m++)
                Place(list, k, 1, m, c, 0.36f * R, 2f * Mathf.PI * (m + 0.5f) / nMid, crest - 11f, crest - 7f, false);
            for (int m = 0; m < nOut; m++)
                Place(list, k, 2, m, c, R - 16f, 2f * Mathf.PI * (m + 0.25f) / nOut, -1f, 6f, false);
            return list;
        }

        /// <summary>Best of 24 hashed candidates for one islet unit: ring radius +-12 %, angle +-0.3 of the ring spacing, top
        /// height in [topLo, topHi]; the one with the least interpenetration with the units already placed wins.</summary>
        static void Place(List<Unit> list, int k, int ring, int m, Vector2 c, float radius, float angle, float topLo, float topHi, bool exactTop)
        {
            float best = float.MaxValue;
            Unit bu = default;
            int nRing = Math.Max(1, ring == 1 ? Mathf.RoundToInt((IsletUnits[k] - 1) * 0.36f) : IsletUnits[k] - 1 - Mathf.RoundToInt((IsletUnits[k] - 1) * 0.36f));
            for (int cand = 0; cand < 24; cand++)
            {
                uint h0 = BwMath.Pcg3(k * 64 + ring * 23 + m, cand, (int)SaltIslet);
                uint h1 = BwMath.Pcg(h0), h2 = BwMath.Pcg(h1), h3 = BwMath.Pcg(h2), h4 = BwMath.Pcg(h3), h5 = BwMath.Pcg(h4);
                float r = ring == 0 ? 4f * U(h0) : radius * (0.88f + 0.24f * U(h0));
                float a = ring == 0 ? 2f * Mathf.PI * U(h1) : angle + (2f * U(h1) - 1f) * 0.3f * 2f * Mathf.PI / nRing;
                Quaternion q = RandomRotation(U(h2), U(h3), U(h4));
                if (ring == 0)
                {
                    // the crest unit stands on three arms (one arm within 20 deg of vertical): the iconic silhouette
                    Vector3 up = q * ArmDir[0];
                    if (up.y < Mathf.Cos(20f * Mathf.Deg2Rad))
                    {
                        Quaternion fix = Quaternion.FromToRotation(up, Vector3.Lerp(Vector3.up, up, 0.25f).normalized);
                        q = Normalize(fix * q);
                    }
                }
                float top = exactTop ? topHi : Mathf.Lerp(topLo, topHi, U(h5));
                float reach = TopReach(q, out int ta);
                var u = new Unit
                {
                    centre = new Vector3(c.x + r * Mathf.Sin(a), top - reach, c.y + r * Mathf.Cos(a)),
                    rot = q, kind = Kind.Islet, islet = k, cellI = -1, cellJ = -1, topArm = ta, gauge = false,
                };
                float pen = 0f;
                for (int o = 0; o < list.Count; o++) pen += Penetration(u, list[o]);
                // outer-ring units lean outward a little less often into the pile: prefer the ones whose top arm points out
                if (pen < best) { best = pen; bu = u; }
            }
            list.Add(bu);
        }

        // ---------------------------------------------------------------- distances (capsule approximation of the arms)
        /// <summary>Signed distance from p to one unit (arms as cones sampled as capsules, hub sphere); negative inside.</summary>
        public static float Distance(in Unit u, Vector3 p)
        {
            float d = (p - u.centre).magnitude - HubRadius;
            for (int a = 0; a < 4; a++)
            {
                Vector3 dir = u.Dir(a);
                float t = Mathf.Clamp(Vector3.Dot(p - u.centre, dir), 0f, ArmLength);
                Vector3 q = u.centre + dir * t;
                float da = (p - q).magnitude - ArmRadiusAt(t);
                if (da < d) d = da;
            }
            return d;
        }

        /// <summary>Sum of the overlaps of the arm capsules of two units (m; 0 = no contact).</summary>
        public static float Penetration(in Unit a, in Unit b)
        {
            if ((a.centre - b.centre).sqrMagnitude > (2f * ArmLength + 2f * ArmR0) * (2f * ArmLength + 2f * ArmR0)) return 0f;
            float pen = 0f;
            for (int i = 0; i < 4; i++)
                for (int j = 0; j < 4; j++)
                {
                    SegmentClosest(a.centre, a.Tip(i), b.centre, b.Tip(j), out float s, out float t, out float dist);
                    float ra = ArmRadiusAt(s * ArmLength), rb = ArmRadiusAt(t * ArmLength);
                    pen += Mathf.Max(0f, ra + rb - dist);
                }
            return pen;
        }

        /// <summary>Closest points of segments p0-p1 and q0-q1 (parameters s, t in [0, 1]) and their distance.</summary>
        public static void SegmentClosest(Vector3 p0, Vector3 p1, Vector3 q0, Vector3 q1, out float s, out float t, out float dist)
        {
            Vector3 d1 = p1 - p0, d2 = q1 - q0, r = p0 - q0;
            float a = Vector3.Dot(d1, d1), e = Vector3.Dot(d2, d2), f = Vector3.Dot(d2, r);
            float c = Vector3.Dot(d1, r), b = Vector3.Dot(d1, d2);
            float den = a * e - b * b;
            s = den > 1e-6f ? Mathf.Clamp01((b * f - c * e) / den) : 0f;
            t = e > 1e-6f ? (b * s + f) / e : 0f;
            if (t < 0f) { t = 0f; s = a > 1e-6f ? Mathf.Clamp01(-c / a) : 0f; }
            else if (t > 1f) { t = 1f; s = a > 1e-6f ? Mathf.Clamp01((b - c) / a) : 0f; }
            dist = ((p0 + d1 * s) - (q0 + d2 * t)).magnitude;
        }

        // ---------------------------------------------------------------- every unit, the gauges
        static List<Unit> s_all;

        /// <summary>Every built unit: the layer-0 grid units whose top reaches above ClipY, then the islet units (stable order).</summary>
        public static IReadOnlyList<Unit> All()
        {
            if (s_all != null) return s_all;
            var list = new List<Unit>(2400);
            for (int j = 0; j < Rows; j++)
                for (int i = 0; i < Columns; i++)
                    if (ReefUnitAt(i, j, out var u) && u.TopY > ClipY) list.Add(u);
            for (int k = 0; k < IsletCount; k++) list.AddRange(Islet(k));
            // gauges: on one tip in five within 300 m of the wall among the units whose top shows (top >= -5), plus the crest
            // unit of islet 1 and islet 3 (KS10 and KK2: "a gauge platform within 200 m"); only near-vertical top arms
            int seen = 0;
            for (int n = 0; n < list.Count; n++)
            {
                var u = list[n];
                if (u.centre.x > 6f + (float)BulwarkConst.GaugeRange) continue;
                if (u.TopY < -5f) continue;
                Vector3 d = u.Dir(u.topArm);
                if (d.y < 0.55f) continue;
                bool force = u.kind == Kind.Islet && (u.islet == 0 || u.islet == 2) && IsCrestUnit(list, n);
                seen++;
                if (force || seen % BulwarkConst.GaugeOneIn == 0) { u.gauge = true; list[n] = u; }
            }
            s_all = list;
            return s_all;
        }

        static bool IsCrestUnit(List<Unit> list, int n)
        {
            var u = list[n];
            if (u.kind != Kind.Islet) return false;
            for (int m = 0; m < list.Count; m++)
                if (list[m].kind == Kind.Islet && list[m].islet == u.islet) return m == n;   // the first of an islet is its crest unit
            return false;
        }

        /// <summary>Signed distance from p to the nearest unit of islet k (the islet hull for the kraken's 2 m clearance).</summary>
        public static float IsletHullDistance(int k, Vector3 p)
        {
            float d = float.MaxValue;
            var l = Islet(k);
            for (int i = 0; i < l.Count; i++) d = Mathf.Min(d, Distance(l[i], p));
            return d;
        }

        static Unit[] s_grid;
        static bool[] s_gridHas;

        /// <summary>The built layer-0 grid unit of cell (i, j) from a cache (false outside the grid, for empty cells and for
        /// units entirely below ClipY).</summary>
        public static bool GridUnit(int i, int j, out Unit u)
        {
            u = default;
            if (i < 0 || i >= Columns || j < 0 || j >= Rows) return false;
            if (s_grid == null)
            {
                var g = new Unit[Columns * Rows];
                var h = new bool[Columns * Rows];
                for (int jj = 0; jj < Rows; jj++)
                    for (int ii = 0; ii < Columns; ii++)
                        if (ReefUnitAt(ii, jj, out var v) && v.TopY > ClipY) { g[jj * Columns + ii] = v; h[jj * Columns + ii] = true; }
                s_grid = g;
                s_gridHas = h;
            }
            if (!s_gridHas[j * Columns + i]) return false;
            u = s_grid[j * Columns + i];
            return true;
        }

        /// <summary>Signed distance from p to the nearest built unit (grid cells around p and every islet).</summary>
        public static float HullDistance(Vector3 p)
        {
            float d = float.MaxValue;
            for (int k = 0; k < IsletCount; k++)
                if ((new Vector2(p.x, p.z) - IsletCentre(k)).magnitude < IsletRadius[k] + 2f * ArmLength) d = Mathf.Min(d, IsletHullDistance(k, p));
            int ci = Mathf.FloorToInt((p.x - GridX0) / Cell);
            for (int j = 0; j < Rows; j++)
            {
                float z0 = RowZ0(j);
                if (p.z < z0 - 2f * Cell || p.z > z0 + 3f * Cell) continue;
                for (int i = ci - 2; i <= ci + 2; i++)
                    if (GridUnit(i, j, out var u)) d = Mathf.Min(d, Distance(u, p));
            }
            return d;
        }
    }
}
