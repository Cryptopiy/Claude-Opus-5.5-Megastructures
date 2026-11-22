// TrainBuilder: the trains T1-T7 of beat 4 and what they run on and vanish into (MASTER_PROMPT 14.5.4, 14.7 H7; owner TR, Phase 4
// wave B; TrainSchedule has the tubes and the timing):
//   * the vehicle (one shared mesh in the train frame: +x forward, nose tip at x 0, y and z about the tube axis): a 6 m nose and
//     7 cars on a 24 m pitch (174.0 m; 0.6 m gangways with dark bellows), the body 3.4 x 3.4 m centred on the tube axis (the
//     visibility audit's box, so KS11's nose lands on x -0.14..-0.06, y -0.12..+0.04), a continuous window strip on both sides
//     (v 1.65..2.65 above the body bottom, inset 3 cm, dark gaskets), a raked windscreen and two SignWhite headlights on the nose,
//     red tail lamps on the last car; under every car two magnet bogies (dark frames, steel side frames, arms that wrap the
//     guideway's top) and three equipment cases; roof gear (two equipment boxes with grilles per car). About 5 k triangles.
//   * per tube one static mesh in the tube's material: the guideway beam (0.9 m wide, top 2.55 m below the axis, its bottom
//     corners 5 mm above the glass) from tunnel end to tunnel end, and for the four face-tube mouths a lining (inward cylinder
//     r 5.25 inside GE's / AB's r 5.3 tunnels and an end disc) that is black beyond MouthFade (the gate tunnels of the bridge
//     tubes are SkybridgeGate's, drawn with the same material);
//   * per train its renderer and its moving lights: a SignWhite headlight 4 m ahead of the nose, low over the beam (r 25, i 3; LOOK_V2 6 row 4) and
//     WinNeutral wash lights riding in the cars (r 14, i 1.0), which light the tube glass (NocGlass GGX highlights) and the deck
//     as the train passes; all fade in after they have left the origin mouth and out before they reach the destination mouth.
// Materials: Megastructure/NocTrain, one per tube (clip planes and mouth planes of TrainSchedule), the Tram row's MetalPaint
// textures when the library has them. Null without the shader (EditMode tests): nothing is built then.
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Megastructure
{
    public static class TrainBuilder
    {
        // ---- vertex zones of NocTrain (vertex colour R = zone x 25)
        public const int ZPaint = 0, ZWindow = 1, ZHead = 2, ZTail = 3, ZDark = 4, ZScreen = 5, ZSteel = 6, ZLiner = 7, ZGuide = 8, ZRoofGlass = 9;

        // ---- body (train frame, metres about the tube axis)
        public const float Half = 1.7f, WinY0 = -0.05f, WinY1 = 0.95f, WallTop = 1.15f, WinInset = 0.03f, WinEndMargin = 0.5f;
        /// <summary>Guideway top below the axis (the magnet arms wrap it), beam half width.</summary>
        public const float GuideTop = -2.55f, GuideHalf = 0.45f;
        public const float BogieInset = 3.0f;
        /// <summary>Light levels (LOOK_V2 6: train headlights SignWhite with a bloom core; the window strip WinNeutral mean 0.28).</summary>
        public const float WinGain = 0.30f, HeadGain = 10.0f, TailGain = 1.2f;
        public const float HeadRange = 25f, HeadIntensity = 3f, WashRange = 14f, WashIntensity = 1.0f;
        /// <summary>The headlight's point light rides 4 m ahead of the nose, 0.45 m over the guideway (it lights the beam and the
        /// glass bottom ahead; its mirror image in the far tube wall stays low, near the track, not a blob at eye level).</summary>
        public const float HeadAhead = 4.0f, HeadY = -2.1f;
        /// <summary>Liner radius inside GE's / AB's r 5.3 portal tunnels (5 cm clear; the r 5 tube glass runs inside it).</summary>
        public const float LinerR = 5.25f;

        // the half profile of the body section (z >= 0), bottom centre to roof centre
        // (the window band's sill and head are profile points too, so the car ends meet the walls on shared vertices)
        static readonly Vector2[] s_prof =
        {
            new Vector2(0f, -1.7f), new Vector2(1.55f, -1.7f), new Vector2(1.7f, -1.55f), new Vector2(1.7f, WinY0), new Vector2(1.7f, WinY1),
            new Vector2(1.7f, WallTop), new Vector2(1.62f, 1.42f), new Vector2(1.42f, 1.62f), new Vector2(1.05f, 1.70f), new Vector2(0f, 1.70f),
        };
        /// <summary>Index of the wall top in the profile (the shoulder starts there).</summary>
        const int WallTopIdx = 5;

        static void Zone(MeshBuilder mb, int zone, float seed)
            => mb.Color = new Color32((byte)(zone * 25), 0, (byte)Mathf.Clamp(Mathf.RoundToInt(seed * 255f), 0, 255), 255);

        /// <summary>Car k's rear and front x in the train frame (car 0 behind the nose; the last car ends at -174).</summary>
        public static void CarSpan(int k, out float xr, out float xf)
        {
            xf = -(float)(TrainSchedule.NoseL + TrainSchedule.CarPitch * k);
            xr = -(float)(TrainSchedule.NoseL + TrainSchedule.CarPitch * (k + 1)) + (k < TrainSchedule.Cars - 1 ? (float)TrainSchedule.Gap : 0f);
        }

        static Vector3 P(float x, float y, float z) => new Vector3(x, y, z);

        static void Q(MeshBuilder mb, Vector3 a, Vector3 b, Vector3 c, Vector3 d, Vector2 ua, Vector2 ub, Vector2 uc, Vector2 ud, Vector3 want)
        {
            Vector3 n = Vector3.Cross(b - a, c - a);
            if (Vector3.Dot(n, want) < 0f) { mb.FlatQuad(a, d, c, b, ua, ud, uc, ub); return; }
            mb.FlatQuad(a, b, c, d, ua, ub, uc, ud);
        }

        /// <summary>A quad with explicit per-vertex normals, wound to face `want`.</summary>
        static void QN(MeshBuilder mb, Vector3 a, Vector3 b, Vector3 c, Vector3 d, Vector3 na, Vector3 nb, Vector3 nc, Vector3 nd,
            Vector2 ua, Vector2 ub, Vector2 uc, Vector2 ud, Vector3 want)
        {
            int ia = mb.Vertex(a, na, ua), ib = mb.Vertex(b, nb, ub), ic = mb.Vertex(c, nc, uc), id = mb.Vertex(d, nd, ud);
            Vector3 n = Vector3.Cross(b - a, c - a);
            if (Vector3.Dot(n, want) < 0f) mb.Quad(ia, id, ic, ib); else mb.Quad(ia, ib, ic, id);
        }

        /// <summary>A convex planar polygon (fan) facing `want`, UV0 = (z, y) metres.</summary>
        static void Poly(MeshBuilder mb, IList<Vector3> pts, Vector3 want)
        {
            Vector3 n = Vector3.Cross(pts[1] - pts[0], pts[2] - pts[0]);
            bool flip = Vector3.Dot(n, want) < 0f;
            Vector3 nn = want.normalized;
            int i0 = mb.Vertex(pts[0], nn, new Vector2(pts[0].z, pts[0].y));
            int prev = mb.Vertex(pts[1], nn, new Vector2(pts[1].z, pts[1].y));
            for (int i = 2; i < pts.Count; i++)
            {
                int cur = mb.Vertex(pts[i], nn, new Vector2(pts[i].z, pts[i].y));
                if (flip) mb.Tri(i0, cur, prev); else mb.Tri(i0, prev, cur);
                prev = cur;
            }
        }

        /// <summary>The full section (both halves, counter-clockwise seen from +x) at x, profile scaled by (sz, y map).</summary>
        static List<Vector3> Section(float x, float sz, float yb, float yt)
        {
            var pts = new List<Vector3>(16);
            for (int j = 0; j < s_prof.Length; j++) pts.Add(P(x, MapY(s_prof[j].y, yb, yt), s_prof[j].x * sz));
            for (int j = s_prof.Length - 2; j >= 1; j--) pts.Add(P(x, MapY(s_prof[j].y, yb, yt), -s_prof[j].x * sz));
            return pts;
        }

        static float MapY(float y, float yb, float yt) => yb + (y + Half) / (2f * Half) * (yt - yb);

        // ============================================================================================================ the vehicle

        /// <summary>The whole train in its own frame (one mesh).</summary>
        public static void Train(MeshBuilder mb)
        {
            Nose(mb);
            for (int k = 0; k < TrainSchedule.Cars; k++) Car(mb, k);
        }

        /// <summary>Car k: shell with the window strips, ends, bellows toward car k - 1, bogies, cases and roof gear.</summary>
        public static void Car(MeshBuilder mb, int k)
        {
            float seed = 0.11f + 0.12f * k;
            CarSpan(k, out float xr, out float xf);
            float L = xf - xr;
            Vector2 U(float x, float y) => new Vector2(x - xr, y + Half);

            // bottom (dark underframe)
            Zone(mb, ZDark, seed);
            Q(mb, P(xr, -Half, -1.55f), P(xf, -Half, -1.55f), P(xf, -Half, 1.55f), P(xr, -Half, 1.55f), U(xr, -1.55f), U(xf, -1.55f), U(xf, 1.55f), U(xr, 1.55f), Vector3.down);
            for (int side = -1; side <= 1; side += 2)
            {
                float s = side;
                Vector3 outN = new Vector3(0f, 0f, s);
                Zone(mb, ZPaint, seed);
                // bottom chamfer (split like the walls above it)
                foreach (var seg in new[] { new Vector2(xr, xr + WinEndMargin), new Vector2(xr + WinEndMargin, xf - WinEndMargin), new Vector2(xf - WinEndMargin, xf) })
                    Q(mb, P(seg.x, -Half, 1.55f * s), P(seg.y, -Half, 1.55f * s), P(seg.y, -1.55f, Half * s), P(seg.x, -1.55f, Half * s),
                        U(seg.x, -Half), U(seg.y, -Half), U(seg.y, -1.55f), U(seg.x, -1.55f), new Vector3(0f, -0.7f, 0.7f * s));
                // lower wall and upper wall band, split at the window ends (no T-junction with the reveals)
                float w0 = xr + WinEndMargin, w1 = xf - WinEndMargin;
                float[] xs = { xr, w0, w1, xf };
                for (int q = 0; q < 3; q++)
                {
                    float a = xs[q], b = xs[q + 1];
                    Q(mb, P(a, -1.55f, Half * s), P(b, -1.55f, Half * s), P(b, WinY0, Half * s), P(a, WinY0, Half * s), U(a, -1.55f), U(b, -1.55f), U(b, WinY0), U(a, WinY0), outN);
                    Q(mb, P(a, WinY1, Half * s), P(b, WinY1, Half * s), P(b, WallTop, Half * s), P(a, WallTop, Half * s), U(a, WinY1), U(b, WinY1), U(b, WallTop), U(a, WallTop), outN);
                }
                // window band: end posts, inset glass strip, gaskets
                Q(mb, P(xr, WinY0, Half * s), P(w0, WinY0, Half * s), P(w0, WinY1, Half * s), P(xr, WinY1, Half * s), U(xr, WinY0), U(w0, WinY0), U(w0, WinY1), U(xr, WinY1), outN);
                Q(mb, P(w1, WinY0, Half * s), P(xf, WinY0, Half * s), P(xf, WinY1, Half * s), P(w1, WinY1, Half * s), U(w1, WinY0), U(xf, WinY0), U(xf, WinY1), U(w1, WinY1), outN);
                float zi = (Half - WinInset) * s;
                Zone(mb, ZWindow, seed);
                Q(mb, P(w0, WinY0, zi), P(w1, WinY0, zi), P(w1, WinY1, zi), P(w0, WinY1, zi), U(w0, WinY0), U(w1, WinY0), U(w1, WinY1), U(w0, WinY1), outN);
                Zone(mb, ZDark, seed);
                Q(mb, P(w0, WinY0, Half * s), P(w1, WinY0, Half * s), P(w1, WinY0, zi), P(w0, WinY0, zi), U(w0, 0f), U(w1, 0f), U(w1, WinInset), U(w0, WinInset), Vector3.up);
                Q(mb, P(w0, WinY1, Half * s), P(w1, WinY1, Half * s), P(w1, WinY1, zi), P(w0, WinY1, zi), U(w0, 0f), U(w1, 0f), U(w1, WinInset), U(w0, WinInset), Vector3.down);
                Q(mb, P(w0, WinY0, Half * s), P(w0, WinY1, Half * s), P(w0, WinY1, zi), P(w0, WinY0, zi), U(0f, WinY0), U(0f, WinY1), U(WinInset, WinY1), U(WinInset, WinY0), Vector3.right);
                Q(mb, P(w1, WinY0, Half * s), P(w1, WinY1, Half * s), P(w1, WinY1, zi), P(w1, WinY0, zi), U(0f, WinY0), U(0f, WinY1), U(WinInset, WinY1), U(WinInset, WinY0), Vector3.left);
                // shoulder and roof (smooth normals across the profile)
                Zone(mb, ZPaint, seed);
                for (int j = WallTopIdx; j < s_prof.Length - 1; j++)
                {
                    Vector2 a = s_prof[j], b = s_prof[j + 1];
                    Vector3 na = ProfNormal(j, s), nb = ProfNormal(j + 1, s);
                    QN(mb, P(xr, a.y, a.x * s), P(xf, a.y, a.x * s), P(xf, b.y, b.x * s), P(xr, b.y, b.x * s), na, na, nb, nb,
                        U(xr, a.y), U(xf, a.y), U(xf, b.y), U(xr, b.y), 0.5f * (na + nb));
                }
            }
            // ends: the full section at the rear and the front (the bellows cover their centres)
            Zone(mb, ZPaint, seed);
            Poly(mb, Section(xf, 1f, -Half, Half), Vector3.right);              // car 0: closes the joint with the nose
            Poly(mb, Section(xr, 1f, -Half, Half), Vector3.left);
            if (k == TrainSchedule.Cars - 1)
            {
                Zone(mb, ZTail, seed);
                foreach (float zs in new[] { -1.15f, 1.15f })
                    MeshKit.ChamferBox(mb, P(xr - 0.015f, -0.95f, zs), new Vector3(0.05f, 0.18f, 0.36f), 0.008f);
                Zone(mb, ZDark, seed);
                MeshKit.ChamferBox(mb, P(xr - 0.025f, -1.45f, 0f), new Vector3(0.06f, 0.2f, 1.4f), 0.015f);
            }
            // gangway bellows into the gap toward car k - 1 (5 cm into both end faces)
            if (k > 0)
            {
                Zone(mb, ZDark, seed);
                float g = (float)TrainSchedule.Gap;
                MeshKit.ChamferBox(mb, P(xf + 0.5f * g, -0.05f, 0f), new Vector3(g + 0.1f, 2.9f, 2.6f), 0.03f);
            }
            // magnet bogies: dark frame, steel side frames, the arms that wrap the guideway top
            foreach (float xb in new[] { xr + BogieInset, xf - BogieInset })
            {
                Zone(mb, ZDark, seed);
                MeshKit.ChamferBox(mb, P(xb, -2.02f, 0f), new Vector3(2.8f, 0.64f, 2.5f), 0.03f);
                foreach (float zs in new[] { -0.64f, 0.64f })
                    MeshKit.ChamferBox(mb, P(xb, -2.58f, zs), new Vector3(2.6f, 0.56f, 0.2f), 0.02f);
                Zone(mb, ZSteel, seed);
                foreach (float zs in new[] { -1.32f, 1.32f })
                    MeshKit.ChamferBox(mb, P(xb, -2.05f, zs), new Vector3(2.5f, 0.42f, 0.16f), 0.015f);
            }
            // equipment cases between the bogies
            Zone(mb, ZDark, seed);
            float c0 = xr + BogieInset + 1.7f, c1 = xf - BogieInset - 1.7f, cl = (c1 - c0) / 3f;
            for (int i = 0; i < 3; i++)
                MeshKit.ChamferBox(mb, P(c0 + cl * (i + 0.5f), -1.93f, 0f), new Vector3(cl - 0.5f, 0.48f, 2.3f), 0.02f);
            // roof glazing between the roof gear (zone 9: the lit ceiling seen from above, e.g. T6 under the KS13 strip)
            Zone(mb, ZRoofGlass, seed);
            float g0 = xr + 0.28f * L, g1 = xr + 0.72f * L;
            foreach (var seg in new[] { new Vector2(xr + 0.6f, g0 - 1.9f), new Vector2(g0 + 1.9f, g1 - 1.9f), new Vector2(g1 + 1.9f, xf - 0.6f) })
                if (seg.y - seg.x > 0.5f)
                    MeshKit.ChamferBox(mb, P(0.5f * (seg.x + seg.y), Half + 0.025f, 0f), new Vector3(seg.y - seg.x, 0.05f, 0.9f), 0.01f);
            // roof gear
            foreach (float f in new[] { 0.28f, 0.72f })
            {
                float xc = xr + f * L;
                Zone(mb, ZSteel, seed);
                MeshKit.ChamferBox(mb, P(xc, Half + 0.13f, 0f), new Vector3(3.4f, 0.30f, 1.8f), 0.03f);
                Zone(mb, ZDark, seed);
                MeshKit.ChamferBox(mb, P(xc, Half + 0.30f, 0f), new Vector3(3.0f, 0.06f, 1.4f), 0.01f);
            }
        }

        static Vector3 ProfNormal(int j, float side)
        {
            Vector2 a = s_prof[Mathf.Max(0, j - 1)], b = s_prof[Mathf.Min(s_prof.Length - 1, j + 1)];
            Vector2 t = b - a;
            // outward normal of the profile (z, y) going from bottom to top on the +z side: (t.y, -t.x) rotated outward
            Vector2 n = new Vector2(t.y, -t.x).normalized;
            if (j == WallTopIdx) n = new Vector2(1f, 0.35f).normalized;         // the wall top blends into the shoulder
            return new Vector3(0f, n.y, n.x * side).normalized;
        }

        // nose stations: x, z scale, bottom y, top y (the profile is mapped bottom..top)
        static readonly float[] s_nx = { -6.0f, -4.5f, -3.0f, -1.5f, 0f };
        static readonly float[] s_nz = { 1.0f, 1.0f, 0.97f, 0.90f, 0.78f };
        static readonly float[] s_nb = { -1.70f, -1.70f, -1.70f, -1.66f, -1.58f };
        static readonly float[] s_nt = { 1.70f, 1.60f, 1.25f, 0.55f, -0.15f };

        /// <summary>The 6 m nose: a loft of the section over five stations (a raked windscreen on the upper part of the middle two
        /// spans), a flat front with two headlights and the coupler cover.</summary>
        public static void Nose(MeshBuilder mb)
        {
            const float seed = 0.05f;
            int ns = s_nx.Length;
            var secs = new List<Vector3>[ns];
            for (int i = 0; i < ns; i++) secs[i] = Section(s_nx[i], s_nz[i], s_nb[i], s_nt[i]);
            int m = secs[0].Count;
            for (int i = 0; i + 1 < ns; i++)
            {
                for (int j = 0; j < m; j++)
                {
                    int j1 = (j + 1) % m;
                    Vector3 a = secs[i][j], b = secs[i + 1][j], c = secs[i + 1][j1], d = secs[i][j1];
                    // profile index of the segment: 0..6 on the +z half (j), mirrored on the -z half
                    int pj = j < s_prof.Length - 1 ? j : (m - 1 - j);
                    bool upper = pj >= WallTopIdx;
                    bool screen = upper && (i == 1 || i == 2) && pj >= WallTopIdx + 1;
                    Zone(mb, screen ? ZScreen : (pj == 0 ? ZDark : ZPaint), seed);
                    Vector3 na = LoftNormal(secs, i, j), nb = LoftNormal(secs, i + 1, j), nc = LoftNormal(secs, i + 1, j1), nd = LoftNormal(secs, i, j1);
                    Vector3 want = Vector3.Cross(b - a, d - a);
                    Vector3 mid = 0.25f * (a + b + c + d);
                    // outward from the section's own centre (the tip's roof lies below the tube axis)
                    Vector3 radial = new Vector3(0f, mid.y - 0.25f * (s_nb[i] + s_nt[i] + s_nb[i + 1] + s_nt[i + 1]), mid.z);
                    if (Vector3.Dot(want, radial) < 0f) want = -want;
                    QN(mb, a, b, c, d, na, nb, nc, nd, new Vector2(a.x + 6f, a.y + Half), new Vector2(b.x + 6f, b.y + Half),
                        new Vector2(c.x + 6f, c.y + Half), new Vector2(d.x + 6f, d.y + Half), want);
                }
            }
            Zone(mb, ZPaint, seed);
            Poly(mb, secs[ns - 1], Vector3.right);
            Zone(mb, ZHead, seed);
            foreach (float zs in new[] { -0.72f, 0.72f })
                MeshKit.ChamferBox(mb, P(0.015f, -1.0f, zs), new Vector3(0.05f, 0.22f, 0.55f), 0.01f);
            Zone(mb, ZDark, seed);
            MeshKit.ChamferBox(mb, P(0.025f, -1.42f, 0f), new Vector3(0.06f, 0.18f, 1.3f), 0.015f);
        }

        static Vector3 LoftNormal(List<Vector3>[] secs, int i, int j)
        {
            int m = secs[i].Count;
            Vector3 along = secs[Mathf.Min(secs.Length - 1, i + 1)][j] - secs[Mathf.Max(0, i - 1)][j];
            Vector3 across = secs[i][(j + 1) % m] - secs[i][(j + m - 1) % m];
            Vector3 n = Vector3.Cross(across, along).normalized;
            Vector3 radial = new Vector3(0f, secs[i][j].y - 0.5f * (s_nb[i] + s_nt[i]), secs[i][j].z);
            if (Vector3.Dot(n, radial) < 0f) n = -n;
            return n;
        }

        // ================================================================================================ tube furniture

        /// <summary>The guideway beam of tube t in world space, from tunnel end to tunnel end (inside the darkness there).</summary>
        public static void Guideway(MeshBuilder mb, int t)
        {
            var tb = TrainSchedule.Tubes[t];
            float r = (float)tb.R, yc = (float)tb.AxisY, zc = (float)tb.AxisZ;
            float bottom = -Mathf.Sqrt(r * r - GuideHalf * GuideHalf) + 0.005f;     // the bottom corners 5 mm above the glass
            float x0 = (float)tb.EndLo + 0.05f, x1 = (float)tb.EndHi - 0.05f;
            Zone(mb, ZGuide, 0.5f);
            float h = GuideTop - bottom;
            MeshKit.ChamferBox(mb, new Vector3(0.5f * (x0 + x1), yc + 0.5f * (GuideTop + bottom), zc), new Vector3(x1 - x0, h, 2f * GuideHalf), 0.02f);
        }

        /// <summary>The lining of one face-tube mouth: an inward cylinder r LinerR from 2 cm inside the lip plane to 4 cm before the
        /// tunnel end, and its end disc (facing the mouth). dirIn = +1 when the tunnel runs toward +x.</summary>
        public static void Liner(MeshBuilder mb, int t, bool lo)
        {
            var tb = TrainSchedule.Tubes[t];
            float yc = (float)tb.AxisY, zc = (float)tb.AxisZ;
            float dirIn = lo ? -1f : 1f;
            float xm = (float)(lo ? tb.MouthLo : tb.MouthHi) + 0.02f * dirIn;
            float xe = (float)(lo ? tb.EndLo : tb.EndHi) - 0.04f * dirIn;
            const int seg = 32;
            Zone(mb, ZLiner, 0.3f);
            int first = mb.VertexCount;
            float len = Mathf.Abs(xe - xm);
            for (int k = 0; k <= seg; k++)
            {
                float a = 2f * Mathf.PI * k / seg;
                float cy = Mathf.Sin(a), cz = Mathf.Cos(a);
                var inward = new Vector3(0f, -cy, -cz);
                float arc = a * LinerR;
                mb.Vertex(new Vector3(xm, yc + LinerR * cy, zc + LinerR * cz), inward, new Vector2(0f, arc));
                mb.Vertex(new Vector3(xe, yc + LinerR * cy, zc + LinerR * cz), inward, new Vector2(len, arc));
            }
            for (int k = 0; k < seg; k++)
            {
                int a0 = first + 2 * k, b0 = first + 2 * (k + 1);
                // facing the axis
                Vector3 pa = mb.Positions[a0], pb = mb.Positions[b0], pc = mb.Positions[a0 + 1];
                Vector3 n = Vector3.Cross(pb - pa, pc - pa);
                Vector3 want = new Vector3(0f, yc, zc) - new Vector3(0f, pa.y, pa.z);
                if (Vector3.Dot(n, want) >= 0f) mb.Quad(a0, b0, b0 + 1, a0 + 1); else mb.Quad(a0, a0 + 1, b0 + 1, b0);
            }
            MeshKit.Disc(mb, new Vector3(xe, yc, zc), new Vector3(-dirIn, 0f, 0f), LinerR + 0.02f, seg);
        }

        // ================================================================================================ materials, world

        static readonly int IdSection = Shader.PropertyToID("_Section"), IdFade = Shader.PropertyToID("_Fade");

        /// <summary>The section vector of tube t: (clip lo, mouth lo, mouth hi, clip hi).</summary>
        public static Vector4 SectionOf(int t)
        {
            var tb = TrainSchedule.Tubes[t];
            return new Vector4((float)(tb.EndLo - TrainSchedule.ClipBehindEnd), (float)tb.MouthLo, (float)tb.MouthHi, (float)(tb.EndHi + TrainSchedule.ClipBehindEnd));
        }

        /// <summary>One NocTrain material per tube (null without the shader: EditMode).</summary>
        public static Material[] Materials(MaterialLibrary lib)
        {
            var sh = Shader.Find("Megastructure/NocTrain");
            if (sh == null) return null;
            var mats = new Material[TrainSchedule.Tubes.Length];
            for (int t = 0; t < mats.Length; t++)
            {
                var m = new Material(sh) { name = "Beat04_Train_" + TrainSchedule.Tubes[t].Name };
                if (lib != null && lib.Tram != null)
                {
                    foreach (var p in new[] { "_AlbedoTex", "_NormalTex", "_MaskTex" })
                        if (lib.Tram.HasProperty(p)) m.SetTexture(p, lib.Tram.GetTexture(p));
                    if (lib.Tram.HasProperty("_Tile")) m.SetFloat("_Tile", lib.Tram.GetFloat("_Tile"));
                }
                BridgeGardenKit.SetLinear(m, "_BaseColor", BridgeGardenKit.Lin(NocPal.MetalPaint));
                BridgeGardenKit.SetLinear(m, "_WinLight", BridgeGardenKit.Lin(NocPal.WinNeutral));
                BridgeGardenKit.SetLinear(m, "_HeadLight", BridgeGardenKit.Lin(NocPal.SignWhite));
                BridgeGardenKit.SetLinear(m, "_TailLight", BridgeGardenKit.Lin(NocPal.AviationRed));
                m.SetFloat("_WinGain", WinGain);
                m.SetFloat("_HeadGain", HeadGain);
                m.SetFloat("_TailGain", TailGain);
                m.SetVector(IdSection, SectionOf(t));
                m.SetVector(IdFade, new Vector4((float)TrainSchedule.MouthFade, 0f, 0f, 0f));
                mats[t] = m;
            }
            return mats;
        }

        /// <summary>Wash lights per train: two in the bridge tubes, one in the face tubes (far from everything but their glass).</summary>
        public static int WashCount(int i) => TrainSchedule.Trains[i].Tube <= TrainSchedule.TubeS ? 2 : 1;

        /// <summary>Train-frame x of wash light w (riding in the cars, at a third and two thirds of the train).</summary>
        public static float WashX(int i, int w) => WashCount(i) == 1 ? -0.5f * (float)TrainSchedule.Length : -(float)TrainSchedule.Length * (w + 1) / 3f;

        /// <summary>Builds the guideways, the face-tube linings, the seven trains, their lights and the driver (registered).
        /// Returns the driver, or null without the shader. failed counts invalid meshes.</summary>
        public static TrainDriver Create(WorldContext ctx, Transform root, Material[] mats, ref int failed)
        {
            if (mats == null) return null;
            var g = BridgeGardenKit.Group(root, "trains");
            for (int t = 0; t < TrainSchedule.Tubes.Length; t++)
            {
                var mb = new MeshBuilder();
                Guideway(mb, t);
                if (t >= TrainSchedule.TubeUpper) { Liner(mb, t, true); Liner(mb, t, false); }
                var rep = MeshValidation.Validate(mb, "Beat04_tube_" + TrainSchedule.Tubes[t].Name);
                if (!rep.Ok) { failed++; Debug.LogError("[NOC-FAIL] Beat04 tube furniture: " + rep); }
                ctx.AddMesh(g, "tube_" + TrainSchedule.Tubes[t].Name, mb.ToMesh("Beat04_tube_" + TrainSchedule.Tubes[t].Name), mats[t]);
            }

            var tm = new MeshBuilder();
            Train(tm);
            var trep = MeshValidation.Validate(tm, "Beat04_train");
            if (!trep.Ok) { failed++; Debug.LogError("[NOC-FAIL] Beat04 train: " + trep); }
            Mesh mesh = tm.ToMesh("Beat04_train");
            int n = TrainSchedule.Trains.Length;
            var tr = new Transform[n];
            var rs = new Renderer[n];
            var heads = new Light[n];
            var wash = new Light[n][];
            Color white = BridgeGardenKit.Lin(NocPal.SignWhite), warm = BridgeGardenKit.Lin(NocPal.WinNeutral);
            for (int i = 0; i < n; i++)
            {
                var spec = TrainSchedule.Trains[i];
                // 14.5.4 fallbacks: Simplified (tier 1) keeps T1 and T4 only, Graybox (tier 2) no trains
                if (!Kept(ctx.Tier, spec.Name)) continue;
                var mr = ctx.AddMesh(g, "train_" + spec.Name, mesh, mats[spec.Tube]);
                mr.transform.SetPositionAndRotation(TrainSchedule.ParkPos(i), TrainSchedule.FrameRot(i));
                mr.enabled = false;
                tr[i] = mr.transform; rs[i] = mr;
                heads[i] = BridgeGardenKit.PointLight(g, spec.Name + "_head", TrainSchedule.FramePos(i, 0.0), white, HeadRange, 0f);
                heads[i].enabled = false;
                wash[i] = new Light[WashCount(i)];
                for (int w = 0; w < wash[i].Length; w++)
                {
                    wash[i][w] = BridgeGardenKit.PointLight(g, spec.Name + "_wash" + w, TrainSchedule.FramePos(i, 0.0), warm, WashRange, 0f);
                    wash[i][w].enabled = false;
                }
            }
            var d = new TrainDriver(tr, rs, heads, wash);
            ctx.Register(d);
            ctx.Log(FormattableString.Invariant($"Beat04 trains: {n} trains x {tm.TriangleCount} triangles, tube furniture for {TrainSchedule.Tubes.Length} tubes"));
            return d;
        }

        /// <summary>The 14.5.4 fallback tiers: Full all seven, Simplified T1 and T4, Graybox none.</summary>
        public static bool Kept(int tier, string name) => tier <= 0 || (tier == 1 && (name == "T1" || name == "T4"));

        /// <summary>Triangles of one train (budget log and tests).</summary>
        public static int TrainTriangles()
        {
            var mb = new MeshBuilder();
            Train(mb);
            return mb.TriangleCount;
        }
    }
}
