// VisibilitySpecWorld: the occluders of the 11.13 visibility audit as the SPEC describes them (MASTER_PROMPT 14.4, 14.5.1-14.5.7,
// the Phase-3 geometry agreements), analytic and independent of what the builders have built so far. The audit ray-casts every
// subject sample against this model ("spec" columns) and against the built meshes ("built" columns, VisibilityRaycaster), so a
// missing beat never hides an occlusion problem that the plan itself has (T-DROP, 14.10; the 11.13 rule "a subject below the horizon
// is visible from eye height h only if the nearest unobstructed edge along its azimuth is closer than h / tan(|elevation|)").
//
// Model (y up, metres; every walkable top at y 0, 14.4):
//   decks (class Deck, the plane y 0 inside a plan): alley floor, boulevard street x -40..215.3 z 120..270, tongue A x 215.3..375.3
//     |z - 195| <= w(x) (NocLanes.TongueHalfWidth), bridge deck x 375.3..695.3 |z - 195| <= 2.5 with the glass strip x 521.3..541.3
//     |z - 195| <= 1.2 as a hole (NocGlass is transparent), garden terrace x 695..914 z 115..275, promenade x 874..914 z 275..520,
//     the gap x 914..930 z 275..420, the plaza circle R 400 about S;
//   rails (class Rail): bridge rails at z 195 +- 2.5 (top rail 0.06 at y 1.04-1.10, rods 0.03 at y 0.35 and 0.70, posts 0.05 every
//     2 m), the tongue's glass balustrade from x 290 (glass is not an occluder; its 0.06 top rail is), the plaza barriers
//     |z - 195| 12.7..13.2, y 0..1.0 on x 225..290;
//   structures (class Structure): gate blocks A / B (z 175..215, 10 m, walkway opening z 189..201 x 8 m), gate frames (pylons 3 m at
//     z 186..189 / 201..204, lintel 2 m, 16 m high, x 372-376 / 683-689), alley podia (faces at the clear half-width lane + 0.35,
//     22 / 31 m) and tower shafts set back 30 / 34 m, the dead-end wall, the boulevard facade slabs z 110..120 (x 60..215.3) / 270..280 (x -40..215.3),
//     y 0..560; the 6 m slots are ignored), the spire (R 32 at y 0 tapering to 20 at y 460, 20 to 560, drums R 48 / 42 / 36 of 6 m),
//     the arcade x 914..990 z 236..360 y 0..38, the screen-wall slab x 834..874 z 275..520 y -1 500..560, the tenement and its
//     neighbours and the goal balcony slab (14.4 / 14.5.7).
// Not modelled (they are subjects or unspecified): the Gulf faces, cross bridges, FaceRib relief, tubes (glass), trains, the ring
// towers (their positions are G4's), CityField towers, trees, signs. Pure C#, allocation-free per ray.
using System;
using System.Collections.Generic;

namespace Megastructure
{
    /// <summary>Occluder class of a ray hit (composition shares: deck / rail count as "pier", everything else below the horizon as "drop").</summary>
    public enum VisClass { None = 0, Deck = 1, Rail = 2, Structure = 3, Built = 4 }

    /// <summary>A ray scene of the audit (spec model or built meshes).</summary>
    public interface IVisRayScene
    {
        string Name { get; }
        /// <summary>Nearest hit of the ray o + t d (d unit) with 0 &lt; t &lt; tMax; label = occluder name.</summary>
        bool Raycast(D3 o, D3 d, double tMax, out double t, out VisClass cls, out string label);
    }

    public sealed class VisibilitySpecWorld : IVisRayScene
    {
        struct Box { public double x0, x1, y0, y1, z0, z1; public VisClass cls; public string name; }

        readonly List<Box> m_boxes = new List<Box>(1024);
        readonly bool m_walkOnly;
        public string Name => m_walkOnly ? "spec-deck" : "spec";
        public int BoxCount => m_boxes.Count;

        public const double SX = NocturneConstants.SpireX, SZ = NocturneConstants.SpireZ;
        public const double BridgeHalf = 2.5, StripX0 = 521.3, StripX1 = 541.3, StripHalf = 1.2;

        /// <summary>The spec occluder model; walkOnly keeps the decks and rails only (the T-DROP occluders of 14.10: "deck, rails and
        /// tongue edges").</summary>
        public VisibilitySpecWorld(bool walkOnly = false)
        {
            m_walkOnly = walkOnly;
            // bridge rails (14.5.4): posts 0.05 every 2 m, top rail 0.06, two rods 0.03
            for (int side = -1; side <= 1; side += 2)
            {
                double zi = 195.0 + side * BridgeHalf;
                double za = Math.Min(zi, zi + side * 0.06), zb = Math.Max(zi, zi + side * 0.06);
                Add(375.3, 695.3, 1.04, 1.10, za, zb, VisClass.Rail, "bridge top rail");
                Add(375.3, 695.3, 0.35, 0.38, za, zb, VisClass.Rail, "bridge rod");
                Add(375.3, 695.3, 0.70, 0.73, za, zb, VisClass.Rail, "bridge rod");
                for (double x = 376.3; x < 695.3; x += 2.0) Add(x - 0.025, x + 0.025, 0.0, 1.10, za, zb, VisClass.Rail, "bridge post");
                // tongue glass balustrade from x 290 (top rail only; the glass is transparent)
                for (double x = 290.0; x < 375.3; x += 1.0)
                {
                    double w = NocLanes.TongueHalfWidth(x + 0.5);
                    double z0 = 195.0 + side * (w - 0.06), z1 = 195.0 + side * w;
                    Add(x, x + 1.0, 1.04, 1.10, Math.Min(z0, z1), Math.Max(z0, z1), VisClass.Rail, "tongue balustrade rail");
                }
                // plaza barrier (planters / benches 1.0 m) at the lane edge +-12.7 on x 225..290
                double b0 = 195.0 + side * 12.7, b1 = 195.0 + side * 13.2;
                Add(225.0, 290.0, 0.0, 1.0, Math.Min(b0, b1), Math.Max(b0, b1), VisClass.Rail, "plaza barrier");
            }
            // gate blocks (z 175..215, 10 m, walkway opening z 189..201 x 8 m) and frames (pylons 3 m, lintel 2 m, 16 m)
            foreach (var g in new[] { (372.0, 384.0, 372.0, 376.0, "gate A"), (683.0, 695.0, 683.0, 689.0, "gate B") })
            {
                Add(g.Item1, g.Item2, 0.0, 10.0, 175.0, 189.0, VisClass.Structure, g.Item5 + " block");
                Add(g.Item1, g.Item2, 0.0, 10.0, 201.0, 215.0, VisClass.Structure, g.Item5 + " block");
                Add(g.Item1, g.Item2, 8.0, 10.0, 189.0, 201.0, VisClass.Structure, g.Item5 + " block lintel");
                Add(g.Item3, g.Item4, 0.0, 16.0, 186.0, 189.0, VisClass.Structure, g.Item5 + " frame pylon");
                Add(g.Item3, g.Item4, 0.0, 16.0, 201.0, 204.0, VisClass.Structure, g.Item5 + " frame pylon");
                Add(g.Item3, g.Item4, 14.0, 16.0, 189.0, 201.0, VisClass.Structure, g.Item5 + " frame lintel");
            }
            // alley (14.5.1): podia faces at the clear half-width (lane + 0.35), 22 m west / 31 m east; shafts set back 30 / 34 m
            for (double s = 0.0; s < 120.0; s += 1.0)
            {
                double h = NocLanes.LaneHalfWidth(s + 0.5) + 0.35;
                Add(-60.0, -h, 0.0, 22.0, s, s + 1.0, VisClass.Structure, "alley west podium");
                Add(h, 60.0, 0.0, 31.0, s, s + 1.0, VisClass.Structure, "alley east podium");
            }
            Add(-60.0, 60.0, 0.0, 31.0, -6.0, -2.5, VisClass.Structure, "alley dead-end wall");
            Add(-90.0, -30.0, 22.0, 560.0, -60.0, 120.0, VisClass.Structure, "alley west tower");
            Add(34.0, 90.0, 31.0, 560.0, -60.0, 120.0, VisClass.Structure, "alley east tower");
            // boulevard facade slabs (14.5.2) and G1's closure to face A (agreement 2); the south slab starts east of the alley block
            // (the alley podia and shafts above stand in for it over x -90..60, so the mouth and the slit stay open)
            Add(60.0, 215.3, 0.0, 560.0, 110.0, 120.0, VisClass.Structure, "boulevard south facade");
            Add(-40.0, 215.3, 0.0, 560.0, 270.0, 280.0, VisClass.Structure, "boulevard north facade");
            // Heart and promenade (14.4, 14.5.6, 14.5.7)
            Add(914.0, 990.0, 0.0, 38.0, 236.0, 360.0, VisClass.Structure, "arcade");
            Add(834.0, 874.0, -1500.0, 560.0, 275.0, 520.0, VisClass.Structure, "screen wall slab");
            // the tenement (14.5.7) with the goal room behind the window (HeartLayout: room x 1 156.3-1 159.6, y 3.6-7.0, z 385.4-390.4;
            // window z 387.3-388.5, y 4.4-5.9 in the 0.3 m wall), so the lamp inside is seen through the window (D-029 GB-01)
            {
                double x0 = HeartLayout.TenX0, x1 = HeartLayout.TenX1, z0 = HeartLayout.TenZ0, z1 = HeartLayout.TenZ1, h = HeartLayout.TenH;
                double rz0 = HeartLayout.RoomZ0, rz1 = HeartLayout.RoomZ1, ry0 = HeartLayout.RoomY0, ry1 = HeartLayout.RoomY1, rx1 = HeartLayout.RoomX1;
                double wx1 = x0 + HeartLayout.WallT;
                Add(x0, x1, 0.0, h, z0, rz0, VisClass.Structure, "tenement");
                Add(x0, x1, 0.0, h, rz1, z1, VisClass.Structure, "tenement");
                Add(x0, x1, 0.0, ry0, rz0, rz1, VisClass.Structure, "tenement");
                Add(x0, x1, ry1, h, rz0, rz1, VisClass.Structure, "tenement");
                Add(rx1, x1, ry0, ry1, rz0, rz1, VisClass.Structure, "tenement");
                Add(x0, wx1, ry0, HeartLayout.GoalY0, rz0, rz1, VisClass.Structure, "tenement");
                Add(x0, wx1, HeartLayout.GoalY1, ry1, rz0, rz1, VisClass.Structure, "tenement");
                Add(x0, wx1, HeartLayout.GoalY0, HeartLayout.GoalY1, rz0, HeartLayout.GoalZ0, VisClass.Structure, "tenement");
                Add(x0, wx1, HeartLayout.GoalY0, HeartLayout.GoalY1, HeartLayout.GoalZ1, rz1, VisClass.Structure, "tenement");
            }
            // the goal balcony slab (14.5.7: 2.6 x 1.4 m on the west face, x 1 154.6-1 156, centred on the window z 387.9), at the
            // D-029 / GB-18 height y 3.4-3.6 (OldQuarterBuilder.BalconyTop / BalconyT; the spec's 4.2 hid the window's lowest 0.35 m
            // in KS19); its railing (0.02 m rods), pot and plant are not modelled
            Add(HeartLayout.BalX0, HeartLayout.BalX1, OldQuarterBuilder.BalconyTop - OldQuarterBuilder.BalconyT, OldQuarterBuilder.BalconyTop,
                HeartLayout.GoalZC - 0.5 * HeartLayout.BalW, HeartLayout.GoalZC + 0.5 * HeartLayout.BalW, VisClass.Structure, "goal balcony slab");
            Add(1156.0, 1170.0, 0.0, 11.0, 355.0, 377.0, VisClass.Structure, "old quarter neighbour S");
            Add(1156.0, 1170.0, 0.0, 12.5, 399.0, 421.0, VisClass.Structure, "old quarter neighbour N");
        }

        void Add(double x0, double x1, double y0, double y1, double z0, double z1, VisClass c, string n)
        {
            if (m_walkOnly && c != VisClass.Rail && c != VisClass.Deck) return;
            m_boxes.Add(new Box { x0 = x0, x1 = x1, y0 = y0, y1 = y1, z0 = z0, z1 = z1, cls = c, name = n });
        }

        // ------------------------------------------------------------------ decks (plane y 0)
        /// <summary>True when (x, z) lies on a walkable deck of the plan (the plane y 0); hole = inside the glass strip.</summary>
        public static bool OnDeck(double x, double z, out string name)
        {
            double dz = Math.Abs(z - 195.0);
            name = null;
            if (x >= -2.6 && x <= 2.6 && z >= -2.5 && z <= 120.0) { name = "alley floor"; return true; }
            if (x >= -40.0 && x <= 215.3 && z >= 120.0 && z <= 270.0) { name = "boulevard street"; return true; }
            if (x >= 215.3 && x <= 375.3 && dz <= NocLanes.TongueHalfWidth(x)) { name = "tongue A deck"; return true; }
            if (x >= 375.3 && x <= 695.3 && dz <= BridgeHalf)
            {
                if (x >= StripX0 && x <= StripX1 && dz <= StripHalf) { name = null; return false; }   // glass strip: transparent
                name = "bridge deck"; return true;
            }
            if (x >= 695.0 && x <= 914.0 && z >= 115.0 && z <= 275.0) { name = "garden terrace"; return true; }
            if (x >= 874.0 && x <= 914.0 && z >= 275.0 && z <= 520.0) { name = "promenade"; return true; }
            if (x >= 914.0 && x <= 930.0 && z >= 275.0 && z <= 420.0) { name = "promenade gap"; return true; }
            double hx = x - SX, hz = z - SZ;
            if (hx * hx + hz * hz <= 400.0 * 400.0) { name = "plaza"; return true; }
            return false;
        }

        /// <summary>True when (x, z) lies inside the glass strip of the bridge (14.5.4).</summary>
        public static bool InStrip(double x, double z) => x >= StripX0 && x <= StripX1 && Math.Abs(z - 195.0) <= StripHalf;

        // ------------------------------------------------------------------ ray cast
        public bool Raycast(D3 o, D3 d, double tMax, out double t, out VisClass cls, out string label)
        {
            t = tMax; cls = VisClass.None; label = null;
            // decks: the plane y 0 from above
            if (d.y < -1e-9 && o.y > 0.0)
            {
                double tp = -o.y / d.y;
                if (tp < t && tp > 1e-6)
                {
                    double x = o.x + tp * d.x, z = o.z + tp * d.z;
                    if (OnDeck(x, z, out string n)) { t = tp; cls = VisClass.Deck; label = n; }
                }
            }
            for (int i = 0; i < m_boxes.Count; i++)
            {
                Box b = m_boxes[i];
                if (RayBox(o, d, b, out double tb) && tb < t) { t = tb; cls = b.cls; label = b.name; }
            }
            if (!m_walkOnly && RaySpire(o, d, t, out double ts)) { t = ts; cls = VisClass.Structure; label = "spire"; }
            return cls != VisClass.None;
        }

        static bool RayBox(D3 o, D3 d, in Box b, out double tHit)
        {
            double t0 = 1e-6, t1 = double.MaxValue;
            tHit = 0.0;
            if (!Slab(o.x, d.x, b.x0, b.x1, ref t0, ref t1)) return false;
            if (!Slab(o.y, d.y, b.y0, b.y1, ref t0, ref t1)) return false;
            if (!Slab(o.z, d.z, b.z0, b.z1, ref t0, ref t1)) return false;
            tHit = t0;
            return true;
        }

        static bool Slab(double o, double d, double lo, double hi, ref double t0, ref double t1)
        {
            if (Math.Abs(d) < 1e-12) return o >= lo && o <= hi;
            double a = (lo - o) / d, b = (hi - o) / d;
            if (a > b) { double k = a; a = b; b = k; }
            if (a > t0) t0 = a;
            if (b < t1) t1 = b;
            return t0 <= t1;
        }

        /// <summary>Spire radius at height y (14.5.7): 32 at y 0 tapering linearly to 20 at y 460, 20 above (to 560).</summary>
        public static double SpireRadius(double y) => y >= 460.0 ? 20.0 : 32.0 - 12.0 * Math.Max(0.0, y) / 460.0;

        /// <summary>Ray against the spire: cone y 0..460, cylinder R 20 y 460..560, drums R 48 / 42 / 36 over y 0..18.</summary>
        public static bool RaySpire(D3 o, D3 d, double tMax, out double tHit)
        {
            tHit = tMax;
            bool hit = false;
            const double k = 12.0 / 460.0;
            // cone: r(y) = 32 - k y  ->  (dx t + ex)^2 + (dz t + ez)^2 = (32 - k (oy + dy t))^2
            double ex = o.x - SX, ez = o.z - SZ, c0 = 32.0 - k * o.y;
            double A = d.x * d.x + d.z * d.z - k * k * d.y * d.y;
            double B = 2.0 * (ex * d.x + ez * d.z + k * d.y * c0);
            double C = ex * ex + ez * ez - c0 * c0;
            hit |= Quadratic(A, B, C, o, d, 0.0, 460.0, ref tHit);
            hit |= Cylinder(o, d, 20.0, 460.0, 560.0, ref tHit);
            hit |= Cylinder(o, d, 48.0, 0.0, 6.0, ref tHit);
            hit |= Cylinder(o, d, 42.0, 6.0, 12.0, ref tHit);
            hit |= Cylinder(o, d, 36.0, 12.0, 18.0, ref tHit);
            return hit;
        }

        static bool Cylinder(D3 o, D3 d, double r, double y0, double y1, ref double tHit)
        {
            double ex = o.x - SX, ez = o.z - SZ;
            double A = d.x * d.x + d.z * d.z, B = 2.0 * (ex * d.x + ez * d.z), C = ex * ex + ez * ez - r * r;
            return Quadratic(A, B, C, o, d, y0, y1, ref tHit);
        }

        static bool Quadratic(double A, double B, double C, D3 o, D3 d, double y0, double y1, ref double tHit)
        {
            if (Math.Abs(A) < 1e-12) return false;
            double disc = B * B - 4.0 * A * C;
            if (disc < 0.0) return false;
            double sq = Math.Sqrt(disc);
            double ta = (-B - sq) / (2.0 * A), tb = (-B + sq) / (2.0 * A);
            if (ta > tb) { double q = ta; ta = tb; tb = q; }
            return Root(ta, o, d, y0, y1, ref tHit) || Root(tb, o, d, y0, y1, ref tHit);
        }

        static bool Root(double tt, D3 o, D3 d, double y0, double y1, ref double tHit)
        {
            if (tt <= 1e-6 || tt >= tHit) return false;
            double y = o.y + tt * d.y;
            if (y < y0 || y > y1) return false;
            tHit = tt;
            return true;
        }
    }
}
