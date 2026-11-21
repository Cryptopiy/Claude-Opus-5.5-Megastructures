// GrayboxInterior: the lane-driven graybox pieces from s 934 to 1500 (beats 5-7) and their props. See GrayboxPath.cs.
// Floors on the exact BwPath.FloorY (the 270 stairs as treads and risers, the gallery ramp), walls 0.35 m outside BwPath.Lane
// (14.4 lanes = collider gap minus 0.70: stairs 3.6, exit hall 4.4, tunnel 3.2 wide; the gallery's west wall at axis -5.5,
// 14.5.6), ceilings from the 14.12 heights; near the tunnel mouth every piece is clipped to x >= -6.0 (the lee face plane),
// where the ledge slab and the face take over (no surface drawn twice). Fittings stay outside the capsule reach (lane edge +
// 0.35): the stair handrails lie in a groove of the wall (flush), the gallery rail sits 0.04 m beyond the trench edge on
// posts fixed to the trench wall's face.
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Megastructure.Beats
{
    public sealed partial class GrayboxPath
    {
        const double LaneStartS = 934.0;          // stair mouth: lane-driven floors / walls from here on
        const double FloorMargin = 0.75;          // floor strip beyond the capsule-centre lane (radius 0.35 + wall)
        const double WallOffset = 0.35;           // wall inner face beyond the lane edge = PLAYER_R (14.4; PathGuide collider face 0.38)
        const double GrooveDepth = 0.10, GrooveHalf = 0.08;   // handrail groove in the stair walls (0.16 high, 0.10 deep)
        const double GalleryRailSetback = 0.06;  // gallery rail axis beyond the trench edge: 0.10 m tube inner surface at axis + 3.01
        const double GalleryNiche = 0.15;         // window gallery west wall at axis -5.5 = lane -5.0 - 0.35 - 0.15 (bollard niche)
        const double BaseStep = 2.0, FineStep = 0.5;
        const double GalleryEastS0 = 1070.0, GalleryEastS1 = 1373.0;
        const double GlassLateral = BulwarkConst.GlassAxisOffset;   // glass plane at axis + 6.0
        const double TrenchDepth = BulwarkConst.HallTrenchDepth;    // 8 m glazed trench below the rail
        const double RailLateral = BulwarkConst.HallRail;           // rail and trench edge at axis + 3.0
        const double ChunkLength = 100.0;
        const double FaceX = BulwarkConst.LeeFaceX;                 // -6.0
        const double MouthClipS0 = BulwarkConst.TunnelMouthS - 8.0; // from here every lane-driven piece is clipped to x >= -6
        static double GratingS0 => BwPath.A3EndS;                   // the 43 m grating panel / conduit (s 1379.875 .. 1422.85)
        const double WellDeckUnderside = -0.5;                      // atrium floor slab underside over the stairs
        const double WellSkirt = 0.12;                              // the closing slab reaches this far below it (GrayboxDam)

        /// <summary>First s from the stair mouth where the stair ceiling (tread line + 2.6) lies at or below the atrium floor
        /// slab underside (-0.5): before it the stairwell is an open well in the atrium floor (no ceiling).</summary>
        static double WellClosedS()
        {
            double lo = LaneStartS, hi = BwPath.StairsStartS + BulwarkConst.StairFlightRun;
            for (int it = 0; it < 60; it++) { double m = 0.5 * (lo + hi); if (CeilingY(m) > WellDeckUnderside) lo = m; else hi = m; }
            return hi;
        }

        /// <summary>Lateral of the atrium guard rail along the open well: 0.25 m outside the stairwell's east wall.</summary>
        static double WellRailLateral()
        {
            LaneEnvelope(WellClosedS(), out _, out double hi);
            return hi + WallOffset + 0.25;
        }

        /// <summary>Lane limits widened to the capsule's footprint: the min / max of BwPath.Lane over s +- (PLAYER_R + 1 cm).
        /// A wall at this envelope + PLAYER_R stays outside every capsule the lane allows, also where the lane blends.</summary>
        static void LaneEnvelope(double s, out double lo, out double hi)
        {
            BwPath.Lane(s, out lo, out hi);
            double r = BulwarkConst.PlayerR + 0.01;
            for (int k = -8; k <= 8; k++)
            {
                if (k == 0) continue;
                BwPath.Lane(s + r * k / 8.0, out double l, out double h);
                if (l < lo) lo = l;
                if (h > hi) hi = h;
            }
        }

        /// <summary>World z of WellClosedS on the path (the atrium floor closes over the stairs from here).</summary>
        static double WellClosedZ() { BwPath.Point(WellClosedS(), out _, out double z); return z; }
        const double GratingS1 = BulwarkConst.TunnelMouthS;

        // ---------------------------------------------------------------- lane-driven pieces (s 934 .. 1500)
        void BuildLaneDriven(WorldContext ctx, Transform root)
        {
            s_wellClosedS = WellClosedS();
            var knots = Knots();
            var risers = new HashSet<long>();
            for (int i = 0; i < BulwarkConst.StairSteps; i++) risers.Add(Key(BwPath.StairRiserS(i)));

            // one builder per part key (beat5 s 934-1070, beat6 1070-1370, beat7 1370-1500: the key of a strip is the beat of
            // its midpoint; a replaced beat's strips are not built)
            var floorS = NewSet();
            var shellS = NewSet();
            var railsS = NewSet();
            var handS = NewSet();
            var glassS = NewSet();
            var conduitS = NewSet();
            float hr = LineRadius(m_handrail, 0.025f), rr = LineRadius(m_rail, 0.025f);
            bool grooveL = false, grooveR = false;   // the previous strip had a handrail groove on that side (end caps)
            double chunkStart = knots[0];
            int chunk = 0;
            double hall = 0.5 * BulwarkConst.LaneExitHallWidth;

            for (int i = 0; i + 1 < knots.Count; i++)
            {
                double a = knots[i], b = knots[i + 1], mid = 0.5 * (a + b);
                int key = BeatKey(BwPath.BeatAt(mid));
                bool skip = m_off[key];
                var floor = floorS[key]; var shell = shellS[key]; var rails = railsS[key];
                var hand = handS[key]; var glass = glassS[key]; var conduit = conduitS[key];
                bool clip = b > MouthClipS0;
                // the ledge leg (from the lane blend at s 1428) lies wholly west of the face: the slab and its rails take over
                // (the ledge lane's right wall would sit exactly on the face plane x -6.0)
                bool ledgeLeg = a >= BulwarkConst.LaneLedgeOpenS0 - 1e-9;
                // watertight seams: the floor is evaluated exactly at a knot unless a riser sits there (one-sided limits), so
                // neighbouring strips share bit-identical edge vertices (no T-junction cracks, lesson Part 10 #7)
                bool rA = risers.Contains(Key(a)), rB = risers.Contains(Key(b));
                double ya = rA ? BwPath.FloorY(a + 1e-6) : BwPath.FloorY(a), yb = rB ? BwPath.FloorY(b - 1e-6) : BwPath.FloorY(b);
                // wall bottoms follow the continuous tread line (0.5 m below it, always under the floor): shared knot vertices
                double ba = BwPath.TreadLineY(a) - 0.5, bb = BwPath.TreadLineY(b) - 0.5;
                BwPath.Lane(a, out double loA, out double hiA);
                BwPath.Lane(b, out double loB, out double hiB);
                // walls follow the lane's capsule envelope (the lane limits over s +- 0.36): where the lane blends (edge
                // slope up to 0.6 m/m) a wall at the plain lane edge + 0.35 would cut into the capsule on the narrow side
                LaneEnvelope(a, out double eloA, out double ehiA);
                LaneEnvelope(b, out double eloB, out double ehiB);
                bool gallery = a >= GalleryEastS0 - 1e-9 && b <= GalleryEastS1 + 1e-9;
                double ca = CeilingAt(a, +1), cb = CeilingAt(b, -1);
                // open well at the stair mouth: no ceiling, the west wall up to the atrium ceiling, the east wall up to the floor
                bool well = a < s_wellClosedS - 1e-9;
                double wlTopA = well ? BulwarkConst.AtriumHeight : ca, wlTopB = well ? BulwarkConst.AtriumHeight : cb;
                double wrTopA = well ? 0.0 : ca, wrTopB = well ? 0.0 : cb;
                double wlA = eloA - WallOffset, wlB = eloB - WallOffset, wrA = ehiA + WallOffset, wrB = ehiB + WallOffset;
                if (gallery)
                {
                    // west wall at axis -5.5 in the window gallery (lane axis -5.0 + radius 0.35 + 0.15 bollard niche)
                    wlA -= GalleryNiche * BwMath.Clamp01((-eloA - hall) / (-BulwarkConst.LaneGalleryLeft - hall));
                    wlB -= GalleryNiche * BwMath.Clamp01((-eloB - hall) / (-BulwarkConst.LaneGalleryLeft - hall));
                }

                if (!ledgeLeg && !skip)
                {
                // floor (treads, landings, ramps): exact FloorY; the gallery floor ends at the trench edge (axis + 3.0)
                double fr0 = gallery ? RailLateral : hiA + FloorMargin, fr1 = gallery ? RailLateral : hiB + FloorMargin;
                FloorQuad(floor, a, b, loA - FloorMargin, fr0, loB - FloorMargin, fr1, ya, yb, clip);
                // riser at a (the floor drops 0.2 m there; the face looks forward / down the stairs)
                if (rA)
                {
                    double yHigh = BwPath.FloorY(a - 1e-6);
                    CrossQuad(floor, a, loA - FloorMargin, hiA + FloorMargin, ya, yHigh, true);
                }

                // stair handrails both sides (tread line + 1.0), round 0.05 m (Handrail, D-07), laid in a 0.16 x 0.10 m groove of
                // the wall so the tube's face is 5 mm behind the wall plane: outside the capsule reach (lane edge + 0.35) at
                // the 14.4 stair width 3.6 (= lane 2.9 + 2 x 0.35). In the open well the east wall ends at the deck (y 0):
                // there the groove and the east handrail start where the band fits under the deck
                bool stairs = a >= BwPath.StairsStartS - 1e-9 && b <= BwPath.StairsEndS + 1e-9;
                double ha = BwPath.TreadLineY(a) + BulwarkConst.StairHandrailHeight - hr, hb = BwPath.TreadLineY(b) + BulwarkConst.StairHandrailHeight - hr;
                double g0a = ha - GrooveHalf, g0b = hb - GrooveHalf, g1a = ha + GrooveHalf, g1b = hb + GrooveHalf;
                bool gL = !ledgeLeg && stairs && g1a <= wlTopA && g1b <= wlTopB;
                bool gR = !ledgeLeg && stairs && !gallery && g1a <= wrTopA && g1b <= wrTopB;
                if (gL != grooveL) GrooveCap(shell, a, wlA, -GrooveDepth, BwPath.TreadLineY(a) + BulwarkConst.StairHandrailHeight - hr);
                if (gR != grooveR) GrooveCap(shell, a, wrA, +GrooveDepth, BwPath.TreadLineY(a) + BulwarkConst.StairHandrailHeight - hr);
                grooveL = gL; grooveR = gR;
                if (gL) Tube(hand, P(a, wlA - hr - 0.005, ha), P(b, wlB - hr - 0.005, hb), hr, 6);
                if (gR) Tube(hand, P(a, wrA + hr + 0.005, ha), P(b, wrB + hr + 0.005, hb), hr, 6);

                // west / left wall
                if (gL) GroovedWall(shell, a, b, wlA, wlB, ba, bb, wlTopA, wlTopB, g0a, g0b, g1a, g1b, -GrooveDepth, true, clip);
                else SideQuad(shell, a, b, wlA, wlB, ba, bb, wlTopA, wlTopB, true, clip);
                if (gallery)
                {
                    // beat 6: rail at axis + 3.0 (+ 0.06: the 0.10 m tube's inner surface at 3.01 lies outside the capsule reach 2.65 + 0.35),
                    // on 0.08 m posts fixed to the face of the trench wall (axis 3.0..3.08), the 3 m glazed trench, the glass
                    // plane at axis + 6.0: ten 20 x 36 m panes (veil shader) between 10 m concrete piers, panes k at s 1085 + 30 k
                    double rl = RailLateral + GalleryRailSetback;
                    Tube(rails, P(a, rl, ya + BulwarkConst.HallRailHeight - rr), P(b, rl, yb + BulwarkConst.HallRailHeight - rr), rr);
                    Tube(rails, P(a, rl, ya + 0.55 - rr), P(b, rl, yb + 0.55 - rr), rr);
                    if (Math.Floor(a / 2.0) != Math.Floor(b / 2.0))
                    {
                        float ph = (float)(BulwarkConst.HallRailHeight - 2 * rr) + 0.3f;
                        Vector3 f = P(a, RailLateral + 0.04, ya);
                        shell.Box(new Vector3(f.x, f.y + 0.5f * ph - 0.3f, f.z), new Vector3(0.08f, ph, 0.08f));
                    }
                    SideQuad(shell, a, b, RailLateral, RailLateral, ya - TrenchDepth, yb - TrenchDepth, ya, yb, true, false);
                    FloorQuad(shell, a, b, RailLateral, GlassLateral, RailLateral, GlassLateral, ya - TrenchDepth, yb - TrenchDepth, false);
                    bool pane = PaneAt(mid) >= 0;
                    SideQuad(pane ? glass : shell, a, b, GlassLateral, GlassLateral, ya - TrenchDepth, yb - TrenchDepth, ca, cb, false, false);
                    CeilingQuad(shell, a, b, wlA, GlassLateral, wlB, GlassLateral, ca, cb, false);
                }
                else
                {
                    // right wall; along the 43 m grating panel the lower 2.8 m is the bright conduit (graybox "bright quad")
                    bool grate = a >= GratingS0 - 1e-6 && b <= GratingS1 + 1e-6;
                    if (grate)
                    {
                        double ga = ya + BulwarkConst.GratingPanelHeight, gb = yb + BulwarkConst.GratingPanelHeight;
                        SideQuad(conduit, a, b, wrA, wrB, ba, bb, ga, gb, false, clip);
                        SideQuad(shell, a, b, wrA, wrB, ga, gb, ca, cb, false, clip);
                    }
                    else if (gR) GroovedWall(shell, a, b, wrA, wrB, ba, bb, wrTopA, wrTopB, g0a, g0b, g1a, g1b, +GrooveDepth, false, clip);
                    else SideQuad(shell, a, b, wrA, wrB, ba, bb, wrTopA, wrTopB, false, clip);
                    if (!well) CeilingQuad(shell, a, b, wlA, wrA, wlB, wrB, ca, cb, clip);
                    else
                    {
                        // the vehicle passage deck beside the open well (y -0.5..0): from the east wall's top edge to x 5.5
                        Vector3 da = P(a, wrA, 0.0), db = P(b, wrB, 0.0);
                        float dx = (float)(BulwarkConst.TowerXMax - 0.5);
                        shell.FlatQuad(da, db, new Vector3(dx, 0f, db.z), new Vector3(dx, 0f, da.z));
                        shell.FlatQuad(new Vector3(da.x, -0.5f, da.z), new Vector3(dx, -0.5f, da.z), new Vector3(dx, -0.5f, db.z), new Vector3(db.x, -0.5f, db.z));
                    }
                }
                }
                else
                {
                    // a replaced beat begins here: close an open handrail groove of the kept strip before it (its end cap)
                    if (skip && i > 0)
                    {
                        int pk = BeatKey(BwPath.BeatAt(0.5 * (knots[i - 1] + a)));
                        if (!m_off[pk])
                        {
                            if (grooveL) GrooveCap(shellS[pk], a, wlA, -GrooveDepth, BwPath.TreadLineY(a) + BulwarkConst.StairHandrailHeight - hr);
                            if (grooveR) GrooveCap(shellS[pk], a, wrA, +GrooveDepth, BwPath.TreadLineY(a) + BulwarkConst.StairHandrailHeight - hr);
                        }
                    }
                    grooveL = false; grooveR = false;
                }
                if (i + 2 == knots.Count && !skip)
                {
                    if (grooveL) GrooveCap(shell, b, wlB, -GrooveDepth, BwPath.TreadLineY(b) + BulwarkConst.StairHandrailHeight - hr);
                    if (grooveR) GrooveCap(shell, b, wrB, +GrooveDepth, BwPath.TreadLineY(b) + BulwarkConst.StairHandrailHeight - hr);
                }

                if (b - chunkStart >= ChunkLength || i + 2 == knots.Count)
                {
                    FlushSet(ctx, floorS, "gb_floor_" + chunk, m_concrete);
                    FlushSet(ctx, shellS, "gb_shell_" + chunk, m_concrete);
                    FlushSet(ctx, railsS, "gb_rails_" + chunk, m_rail, false);
                    FlushSet(ctx, handS, "gb_handrails_" + chunk, m_handrail != null ? m_handrail : m_rail, false);
                    FlushSet(ctx, glassS, "gb_glass_" + chunk, m_veil, false);
                    FlushSet(ctx, conduitS, "gb_conduit_" + chunk, m_conduit, false);
                    chunkStart = b;
                    chunk++;
                }
            }

            // doorway wall at s 1070 (exit hall 4.4 x 4.0 into the 28 m gallery) and the hall end wall at s 1373 (tunnel door)
            // the doorway wall at s 1070 is beat 6's (the gallery's south end), the hall end wall at s 1373 beat 7's
            var ends = new MeshBuilder();
            {
                double s = BulwarkConst.GalleryStartS, y = BwPath.FloorY(s), h = BulwarkConst.ExitDoorHeight, half = 0.5 * BulwarkConst.ExitDoorWidth;
                double west = BulwarkConst.HallWestWall;
                // two-sided: the exit hall must see only the 4.4 x 4.0 doorway (14.6), the gallery the wall behind it
                CrossWall(ends, s, west, GlassLateral, y + h, y + BulwarkConst.HallHeight);
                CrossWall(ends, s, west, -half, y, y + h);
                CrossWall(ends, s, half, GlassLateral, y - TrenchDepth, y + h);
            }
            FlushKey(ctx, K6, ref ends, "gb_end_walls", m_concrete);
            {
                double s = BulwarkConst.HallEndWallS, y = BwPath.FloorY(s), h = BulwarkConst.TunnelHeight, half = 0.5 * BulwarkConst.TunnelWidth;
                double west = BulwarkConst.HallWestWall;
                double top = BwPath.FloorY(GalleryEastS1 - 3.0) + BulwarkConst.HallHeight;
                CrossWall(ends, s, west, GlassLateral, y + h, top);
                CrossWall(ends, s, west, -half, y, y + h);
                CrossWall(ends, s, half, GlassLateral, y - TrenchDepth, y + h);
            }
            FlushKey(ctx, K7, ref ends, "gb_end_walls", m_concrete);
        }

        /// <summary>Pane index k (0-9) whose 20 m span contains s, else -1 (panes at s 1085 + 30 k, 14.4).</summary>
        public static int PaneAt(double s)
        {
            for (int k = 0; k < BulwarkConst.PaneCount; k++)
            {
                double c = BulwarkConst.PaneS0 + BulwarkConst.PanePitch * k;
                if (Math.Abs(s - c) <= 0.5 * BulwarkConst.PaneWidth) return k;
            }
            return -1;
        }

        /// <summary>s knots of the lane-driven strips: 2 m on straights, 0.5 m on arcs and lane blends, every stair riser,
        /// flight ends, lane knots, pane edges, the grating panel ends and section boundaries.</summary>
        static List<double> Knots()
        {
            var k = new List<double>(4096);
            for (double s = LaneStartS; s < BwPath.Length; s += BaseStep) k.Add(s);
            k.Add(BwPath.Length);
            void Fine(double s0, double s1) { for (double s = s0; s < s1; s += FineStep) k.Add(s); k.Add(s1); }
            Fine(BwPath.A1StartS, BwPath.A1EndS);
            Fine(BwPath.StairsEndS, BwPath.A2EndS);
            Fine(BwPath.GalleryEndS, BwPath.A3EndS);
            Fine(BwPath.TunnelEndS, BwPath.A4EndS);
            Fine(BulwarkConst.LaneHallOpenS0, BulwarkConst.LaneHallOpenS1);
            Fine(BulwarkConst.LaneGalleryOpenS0, BulwarkConst.LaneGalleryOpenS1);
            Fine(BulwarkConst.LaneTunnelFunnelS0, BulwarkConst.LaneTunnelFunnelS1);
            Fine(BulwarkConst.LaneLedgeOpenS0, BwPath.A4EndS);
            Fine(MouthClipS0, BulwarkConst.TunnelMouthS + 8.0);
            for (int i = 0; i < BulwarkConst.StairSteps; i++) k.Add(BwPath.StairRiserS(i));
            for (int f = 0; f < BulwarkConst.StairFlights; f++)
            {
                double fs = BwPath.StairFlightStartS(f);
                k.Add(fs);
                k.Add(fs + BulwarkConst.StairFlightRun);
            }
            for (int p = 0; p < BulwarkConst.PaneCount; p++)
            {
                double c = BulwarkConst.PaneS0 + BulwarkConst.PanePitch * p;
                k.Add(c - 0.5 * BulwarkConst.PaneWidth);
                k.Add(c + 0.5 * BulwarkConst.PaneWidth);
            }
            k.Add(BwPath.StairsStartS); k.Add(BwPath.StairsEndS);
            k.Add(BulwarkConst.GalleryStartS); k.Add(GalleryEastS1);
            k.Add(GratingS0); k.Add(GratingS1);
            k.Add(WellClosedS());
            k.Sort();
            var o = new List<double>(k.Count);
            foreach (double s in k)
            {
                if (s < LaneStartS - 1e-9 || s > BwPath.Length + 1e-9) continue;
                if (o.Count > 0 && s - o[o.Count - 1] < 1e-4) continue;
                o.Add(s);
            }
            return o;
        }

        static long Key(double s) => (long)Math.Round(s * 1e4);
        static double s_wellClosedS = double.NaN;

        /// <summary>Absolute ceiling y: stairs and A2 tread line + 2.6; exit hall floor + 4.0; window gallery floor + 28;
        /// tunnel floor + 3.4.</summary>
        static double CeilingY(double s)
        {
            if (s < BwPath.A2EndS) return BwPath.TreadLineY(s) + BulwarkConst.StairHeadroom;
            if (s < BulwarkConst.GalleryStartS) return BwPath.FloorY(s) + BulwarkConst.ExitHallHeight;
            if (s < BulwarkConst.HallEndWallS) return BwPath.FloorY(s) + BulwarkConst.HallHeight;
            return BwPath.FloorY(s) + BulwarkConst.TunnelHeight;
        }

        /// <summary>Ceiling y at a knot: exact where CeilingY is continuous (shared seam vertices), else the one-sided limit
        /// (side +1: the strip starting at s, -1: the strip ending at s).</summary>
        static double CeilingAt(double s, int side)
        {
            double lo = CeilingY(s - 1e-6), hi = CeilingY(s + 1e-6);
            if (Math.Abs(hi - lo) < 1e-3) return CeilingY(s);
            return side > 0 ? hi : lo;
        }

        // ---------------------------------------------------------------- quads in lane coordinates (optionally clipped to x >= -6)
        static void Emit(MeshBuilder mb, Vector3 a, Vector3 b, Vector3 c, Vector3 d, bool clip)
        {
            if (!clip) { mb.FlatQuad(a, b, c, d); return; }
            if (a.x >= FaceX && b.x >= FaceX && c.x >= FaceX && d.x >= FaceX) { mb.FlatQuad(a, b, c, d); return; }
            PolyClipX(mb, a, b, c, d, FaceX);
        }

        /// <summary>Upward-facing quad between s a and b, lateral [l0, l1] at a and [m0, m1] at b.</summary>
        static void FloorQuad(MeshBuilder mb, double a, double b, double l0, double l1, double m0, double m1, double ya, double yb, bool clip)
            => Emit(mb, P(a, l0, ya), P(b, m0, yb), P(b, m1, yb), P(a, l1, ya), clip);

        /// <summary>Downward-facing quad (ceiling).</summary>
        static void CeilingQuad(MeshBuilder mb, double a, double b, double l0, double l1, double m0, double m1, double ya, double yb, bool clip)
            => Emit(mb, P(a, l0, ya), P(a, l1, ya), P(b, m1, yb), P(b, m0, yb), clip);

        /// <summary>Vertical quad along s at lateral la (at a) .. lb (at b); faceRight: the face looks to +lateral.</summary>
        static void SideQuad(MeshBuilder mb, double a, double b, double la, double lb, double ya0, double yb0, double ya1, double yb1, bool faceRight, bool clip)
        {
            if (faceRight) Emit(mb, P(a, la, ya0), P(a, la, ya1), P(b, lb, yb1), P(b, lb, yb0), clip);
            else Emit(mb, P(a, la, ya0), P(b, lb, yb0), P(b, lb, yb1), P(a, la, ya1), clip);
        }

        /// <summary>Vertical quad across the path at s, lateral [l0, l1], y [y0, y1]; forward: the face looks along +s.</summary>
        static void CrossQuad(MeshBuilder mb, double s, double l0, double l1, double y0, double y1, bool forward)
        {
            if (forward) mb.FlatQuad(P(s, l0, y0), P(s, l1, y0), P(s, l1, y1), P(s, l0, y1));
            else mb.FlatQuad(P(s, l0, y0), P(s, l0, y1), P(s, l1, y1), P(s, l1, y0));
        }

        /// <summary>Side wall with a horizontal groove [g0, g1] recessed by <paramref name="depth"/> (signed lateral: negative
        /// on the left wall, positive on the right): the wall below and above the groove, its back, floor and soffit.</summary>
        static void GroovedWall(MeshBuilder mb, double a, double b, double la, double lb, double y0a, double y0b, double y1a, double y1b,
                                double g0a, double g0b, double g1a, double g1b, double depth, bool faceRight, bool clip)
        {
            SideQuad(mb, a, b, la, lb, y0a, y0b, g0a, g0b, faceRight, clip);
            SideQuad(mb, a, b, la, lb, g1a, g1b, y1a, y1b, faceRight, clip);
            SideQuad(mb, a, b, la + depth, lb + depth, g0a, g0b, g1a, g1b, faceRight, clip);
            FloorQuad(mb, a, b, Math.Min(la, la + depth), Math.Max(la, la + depth), Math.Min(lb, lb + depth), Math.Max(lb, lb + depth), g0a, g0b, clip);
            CeilingQuad(mb, a, b, Math.Min(la, la + depth), Math.Max(la, la + depth), Math.Min(lb, lb + depth), Math.Max(lb, lb + depth), g1a, g1b, clip);
        }

        /// <summary>End cap of a handrail groove at s (two-sided), wall at lateral la, groove recessed by depth, axis y h.</summary>
        static void GrooveCap(MeshBuilder mb, double s, double la, double depth, double h)
            => CrossWall(mb, s, Math.Min(la, la + depth), Math.Max(la, la + depth), h - GrooveHalf, h + GrooveHalf);

        /// <summary>A wall across the path seen from both sides (two opposite one-sided quads; culling shows one).</summary>
        static void CrossWall(MeshBuilder mb, double s, double l0, double l1, double y0, double y1)
        {
            CrossQuad(mb, s, l0, l1, y0, y1, true);
            CrossQuad(mb, s, l0, l1, y0, y1, false);
        }

        /// <summary>z where the graybox tunnel wall at <paramref name="lateral"/> crosses the lee face x -6.0 (bisection on the
        /// tunnel diagonal and A4, where x falls monotonically with s).</summary>
        static double WallCrossZ(double lateral)
        {
            double lo = BulwarkConst.TunnelMouthS - 15.0, hi = BwPath.A4EndS;
            for (int it = 0; it < 60; it++)
            {
                double m = 0.5 * (lo + hi);
                BwPath.World(m, lateral, out double x, out _, out _);
                if (x > FaceX) lo = m; else hi = m;
            }
            BwPath.World(0.5 * (lo + hi), lateral, out _, out _, out double z);
            return z;
        }

        // ================================================================ props of beats 5-7
        void BuildInteriorProps(WorldContext ctx, Transform root)
        {
            var lamps = new MeshBuilder();          // beat 5 exit hall ceiling lamps
            var lampsLow = new MeshBuilder();       // beat 5 landing lamps
            var bollardTops = new MeshBuilder();    // beat 6 bollard lamp tops
            var lampsSodium = new MeshBuilder();    // beat 7 tunnel cages
            var dark = new MeshBuilder();           // beat 6 bollard bodies
            // beat 5: a caged lamp per stair landing (on the left wall, 2.2 m above the landing), lamps every 8 m in the exit
            // hall ceiling; real lights (Tungsten, 8 m) through the light pool
            if (!m_off[K5])
            {
                for (int k = 0; k < BulwarkConst.StairFlights - 1; k++)
                {
                    double s = BwPath.StairFlightStartS(k) + BulwarkConst.StairFlightRun + 0.5 * BulwarkConst.StairLanding;
                    BwPath.Lane(s, out double lo, out _);
                    double y = BwPath.FloorY(s);
                    Vector3 p = P(s, lo - WallOffset + 0.12, y + 2.2);
                    lampsLow.Box(p, new Vector3(0.22f, 0.2f, 0.3f));          // caged landing lamp, Tungsten x1.5 (x3 blew the wall at 1.5 m)
                    AddAnchor(K5, P(s, lo + 0.6, y + 2.0), Pal.Tungsten, 8f, 0.8f);
                }
                for (double s = BulwarkConst.ExitHallS0 + 4.0; s < BulwarkConst.ExitHallS1 - 1.0; s += BulwarkConst.ExitHallLampPitch)
                {
                    double y = BwPath.FloorY(s) + BulwarkConst.ExitHallHeight - 0.12;
                    lamps.Box(P(s, 0.0, y), new Vector3(0.45f, 0.16f, 0.45f));
                    AddAnchor(K5, P(s, 0.0, y - 0.5), Pal.Tungsten, 8f, 1.6f);
                }
            }
            // beat 6: bollard lamps 0.9 m every 20 m against the west wall (0.14 m deep, between the wall at axis -5.5 and the
            // collider face at axis -5.35), Tungsten x1.5 tops
            if (!m_off[K6])
                for (double s = 1080.0; s <= 1360.0; s += BulwarkConst.BollardLampPitch)
                {
                    double y = BwPath.FloorY(s), l0 = BulwarkConst.HallWestWall + 0.005, l1 = BulwarkConst.HallWestWall + 0.145;
                    Vector3 b0 = P(s - 0.1, l0, y), b1 = P(s + 0.1, l1, y + BulwarkConst.BollardLampHeight - 0.1);
                    BoxMinMax(dark, b0.x, b0.y, b0.z, b1.x, b1.y, b1.z);
                    Vector3 t0 = P(s - 0.1, l0, y + BulwarkConst.BollardLampHeight - 0.1), t1 = P(s + 0.1, l1, y + BulwarkConst.BollardLampHeight);
                    BoxMinMax(bollardTops, t0.x, t0.y, t0.z, t1.x, t1.y, t1.z);
                    AddAnchor(K6, P(s, l1 + 0.4, y + 0.8), Pal.Tungsten, 4f, 0.8f);
                }
            // beat 7: SodiumWarm cages every 6 m in the tunnel ceiling (A3 end .. the mouth)
            if (!m_off[K7])
                for (double s = BwPath.A3EndS + 2.0; s < BulwarkConst.TunnelMouthS - 3.0; s += BulwarkConst.TunnelLampPitch)
                {
                    double y = BwPath.FloorY(s) + BulwarkConst.TunnelHeight - 0.12;
                    lampsSodium.Box(P(s, -0.3, y), new Vector3(0.45f, 0.16f, 0.25f));
                    AddAnchor(K7, P(s, -0.3, y - 0.4), Pal.SodiumWarm, 6f, 1.6f);
                }
            FlushKey(ctx, K5, ref lamps, "gb_lamp_interior", m_tungsten, false);
            FlushKey(ctx, K5, ref lampsLow, "gb_lamp_bollards", m_tungstenLow, false);
            FlushKey(ctx, K6, ref bollardTops, "gb_lamp_bollards", m_tungstenLow, false);
            FlushKey(ctx, K7, ref lampsSodium, "gb_lamp_tunnel", m_sodium, false);
            FlushKey(ctx, K6, ref dark, "gb_hall_bollards", m_dark);
            if (m_off[K7]) return;   // the ledge, its rails, the mouth face and the lantern are beat 7's

            // beat 7: the lee ledge slab x -12.4..-6.0, 0.8 m thick, from the z 1415.2 apron (DEVELOPMENT.md "Tunnel mouth
            // floor": the tunnel lane covers a triangle beyond the face for s 1419.0-1422.8) to z 1503; the outer lee face plane
            // round the mouth (exact hole of the clipped tunnel); rails 1.1 / 0.55 on posts every 2 m on the drop side and both ends
            var slab = new MeshBuilder();
            double ly = BulwarkConst.FloorYTunnel, lx0 = BulwarkConst.LedgeXMin, lx1 = BulwarkConst.LedgeXMax;
            double lz0 = MouthZ0, lz1 = BulwarkConst.LedgeZ1;
            BoxMinMax(slab, lx0, ly - BulwarkConst.LedgeThickness, lz0, lx1, ly, lz1);
            // brackets under the slab every 6 m (scale cue, "on brackets")
            for (double z = lz0 + 3.0; z < lz1; z += 6.0) BoxMinMax(slab, lx1 - 2.2, ly - 2.6, z - 0.2, lx1, ly - BulwarkConst.LedgeThickness, z + 0.2);
            FlushKey(ctx, K7, ref slab, "gb_ledge_slab", m_dark);

            var face = new MeshBuilder();
            double zL = WallCrossZ(-(BulwarkConst.LaneTunnelWidth * 0.5 + WallOffset)), zR = WallCrossZ(BulwarkConst.LaneTunnelWidth * 0.5 + WallOffset);
            double wz0 = HoleZ0, wz1 = HoleZ1, wy0 = ly - 0.3, wy1 = ly + BulwarkConst.TunnelHeight + 0.3, ty1 = ly + BulwarkConst.TunnelHeight;
            float fx = (float)FaceX;
            void WestQuad(double z0, double y0, double z1, double y1)
            {
                if (z1 - z0 < 1e-3 || y1 - y0 < 1e-3) return;
                face.FlatQuad(new Vector3(fx, (float)y0, (float)z1), new Vector3(fx, (float)y1, (float)z1), new Vector3(fx, (float)y1, (float)z0), new Vector3(fx, (float)y0, (float)z0));
            }
            WestQuad(wz0, wy0, wz1, ly);
            WestQuad(wz0, ty1, wz1, wy1);
            WestQuad(wz0, ly, Math.Min(zL, wz1), ty1);
            WestQuad(Math.Max(zR, wz0), ly, wz1, ty1);
            FlushKey(ctx, K7, ref face, "gb_mouth_face", m_dark);

            var rails = new MeshBuilder();
            var posts = new MeshBuilder();
            float rr = LineRadius(m_rail, 0.025f);
            double rx = lx0 + 0.06, rzS = lz0 + 0.06, rzN = lz1 - 0.06;
            LedgeRail(rails, posts, new Vector3((float)rx, (float)ly, (float)rzS), new Vector3((float)rx, (float)ly, (float)rzN), rr);
            LedgeRail(rails, posts, new Vector3((float)rx, (float)ly, (float)rzS), new Vector3((float)(lx1 - 0.2), (float)ly, (float)rzS), rr);
            LedgeRail(rails, posts, new Vector3((float)rx, (float)ly, (float)rzN), new Vector3((float)(lx1 - 0.2), (float)ly, (float)rzN), rr);
            FlushKey(ctx, K7, ref rails, "gb_rails_ledge", m_rail, false);

            // the Keeper's Lantern (-9.2, -90, 1499.0): iron post 1.4 on a 0.4 base plate, the glass as an emissive sphere
            // (graybox "lantern sphere", Lantern x6, 0.30 m), a 5 m real light (2.0)
            double lxp = BulwarkConst.LanternX, lzp = BulwarkConst.LanternZ, lyp = BulwarkConst.LanternY;
            double hb = 0.5 * BulwarkConst.LanternBase;
            BoxMinMax(posts, lxp - hb, lyp, lzp - hb, lxp + hb, lyp + 0.04, lzp + hb);
            BoxMinMax(posts, lxp - 0.04, lyp + 0.04, lzp - 0.04, lxp + 0.04, lyp + BulwarkConst.LanternPost, lzp + 0.04);
            FlushKey(ctx, K7, ref posts, "gb_ledge_posts", m_steel);
            var glassL = new MeshBuilder();
            var gc = new Vector3((float)lxp, (float)(lyp + BulwarkConst.LanternPost + 0.5 * BulwarkConst.LanternGlass), (float)lzp);
            MeshKit.Sphere(glassL, gc, 0.5f * (float)BulwarkConst.LanternGlass, 20, 10);
            FlushKey(ctx, K7, ref glassL, "gb_lamp_lantern", m_lantern, false);
            AddAnchor(K7, gc + new Vector3(0f, -0.1f, -0.3f), Pal.Lantern, (float)BulwarkConst.LanternLightRange, (float)BulwarkConst.LanternLightIntensity);
        }

        /// <summary>A ledge rail run a -> b at floor level: rails with tops 1.1 and 0.55, posts 0.12 every 2 m.</summary>
        static void LedgeRail(MeshBuilder rails, MeshBuilder posts, Vector3 a, Vector3 b, float radius)
        {
            Vector3 up = Vector3.up;
            float top = (float)BulwarkConst.LedgeRail;
            Tube(rails, a + up * (top - radius), b + up * (top - radius), radius);
            Tube(rails, a + up * (0.55f - radius), b + up * (0.55f - radius), radius);
            float len = (b - a).magnitude;
            int n = Mathf.Max(1, Mathf.RoundToInt(len / (float)BulwarkConst.LedgeRailPostPitch));
            float ps = 0.5f * (float)BulwarkConst.LedgeRailPost;
            for (int i = 0; i <= n; i++)
            {
                Vector3 p = Vector3.Lerp(a, b, i / (float)n);
                BoxMinMax(posts, p.x - ps, p.y, p.z - ps, p.x + ps, p.y + top - 2f * radius, p.z + ps);
            }
        }
    }
}
