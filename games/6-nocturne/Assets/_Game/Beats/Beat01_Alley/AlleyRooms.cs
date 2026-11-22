// AlleyRooms: traces of emptiness (docs/DREAD.md 6, D-034 item 6; Phase 4 wave B, AL): two street-level rooms beside the path, each
// seen through a 1.4 x 1.3 m window (y 0.9-2.2) in the alley wall: a table, a chair pulled out, a cup on the table, a lamp on, and
// nobody. Room 0 on the east wall at z 14.0-15.4 (between the doors at z 9 and 16), room 1 on the west wall at z 37.6-39.0 (just
// before the noodle bar's frontage: in KS01's frame, left of the bar, seen at 50 deg), both clear of the width stations (s 13, 37)
// and of every fixture.
// Each window: the wall strips leave the opening (AlleyBlocks), a 25 cm concrete reveal (jambs, sill, head; nothing proud of the
// wall: the capsule touches the wall plane), a painted steel frame with a mullion 12 cm in,
// an awning above (AlleyFixtures); no glass pane (NocGlass read as a milky sheet over the dark room). The room behind: 3.0 m deep, the window's width + 2.4 m, 2.7 m high (floor y 0), faded wallpaper,
// a wood floor, a plaster ceiling (NocAlleyRoom, G1Mat.AlleyRoomA / B: albedo and kind in the vertex colour); a table 0.8 x 0.8 x
// 0.74 m, a chair (seat 0.45 m, back to 0.92 m) pulled out and turned toward the window, a white cup (r 4 cm), a table lamp whose
// shade is self-lit and whose analytic light (Lamp(r): LampWarm-ish 2 700 K, i 1.6) pools on the table and the wall; one curtain
// half drawn. No realtime light (the 14.2.3 cap of 20).
// Fix wave C37 (KS01 mean 39.5 > 34; room 1 the flattest, brightest surface of the frame, mean 77, evenly lit grey): the rooms are
// enclosed (AlleyMaterials.EnclosedRoom: their own lamp only; the alley's realtime lights and 90 % of the sky ambient no longer
// reach inside), the walls and the ceiling take the CC0 plaster with damp blotches, tide marks under the ceiling and grime over the
// floor (NocAlleyRoom), the wallpaper a faded ochre / rose-beige 0.09-0.11 (was 0.17 / 0.15 grey-beige; AL_f02's dusty green read
// as a cold bunker), the lamp i 0.8 (was 1.6: emission 0.07 linear = code ~75 at the alley's exposure), the shade x 1.5 (was 4:
// a clipped white card) so the lamp reads warm.
using System.Collections.Generic;
using UnityEngine;

namespace Megastructure
{
    public static class AlleyRooms
    {
        public const int Count = 2;
        public const float WinY0 = 0.9f, WinY1 = 2.2f, WinW = 1.4f, Reveal = 0.25f, Depth = 3.0f, Height = 2.7f, GlassIn = 0.12f;
        static readonly int[] Side = { 1, -1 };
        static readonly float[] Z0 = { 14.0f, 37.6f };
        public const float SelfLit = 1.5f, Fill = 0.004f, LampI = 0.8f;
        public static Color LampColour => new Color(1.0f, 0.74f, 0.50f, 1f);

        /// <summary>Window r: wall side and its z extent.</summary>
        public static void Window(int r, out int side, out float za, out float zb) { side = Side[r]; za = Z0[r]; zb = Z0[r] + WinW; }

        /// <summary>True when z lies in a room window of the wall `side` (AlleyBlocks cuts the opening there).</summary>
        public static bool InWindow(int side, double z)
        {
            for (int r = 0; r < Count; r++) if (Side[r] == side && z > Z0[r] && z < Z0[r] + WinW) return true;
            return false;
        }

        /// <summary>The wall line x of room r (the wall is straight there: lane 1.85, wall 2.2).</summary>
        static float WallX(int r) => Side[r] * (float)AlleyLayout.HalfWidth(Z0[r] + 0.5f * WinW);

        /// <summary>The table lamp's light of room r (world xyz of the shade, intensity).</summary>
        public static Vector4 Lamp(int r)
        {
            Table(r, out Vector3 t);
            Vector3 p = t + new Vector3(Side[r] * 0.18f, 0.74f + 0.36f, 0.2f);
            return new Vector4(p.x, p.y, p.z, LampI);
        }

        /// <summary>Centre of room r's table on the floor (1.3 m behind the wall, 0.9 m north of the window's middle: the walk sees
        /// the rooms from the south, so the table, the chair, the cup and the lamp stand in the part of the room the window shows).</summary>
        static void Table(int r, out Vector3 c)
        {
            c = new Vector3(WallX(r) + Side[r] * (Reveal + 1.3f), 0f, Z0[r] + 0.5f * WinW + 0.9f);
        }

        const int Matte = 0, WoodK = 2, Lit = 3, Steel = 5, Fabric = 6, Ceramic = 7;
        static void Paint(MeshBuilder mb, float r, float g, float b, int kind) => mb.Color = new Color32((byte)(r * 255f), (byte)(g * 255f), (byte)(b * 255f), (byte)(kind * 32));

        public static void Build(G1Parts parts)
        {
            var pts = new List<Vector3>(4); var frames = new List<LoftFrame>(4); var sec = new CrossSection();
            for (int r = 0; r < Count; r++)
            {
                int s = Side[r];
                float za = Z0[r], zb = za + WinW, xw = WallX(r), xr = xw + s * Reveal, xg = xw + s * GlassIn;
                var origin = new Vector3(xw, 1.5f, 0.5f * (za + zb));
                var rev = parts.Get("roomReveal" + r, G1Mat.ConcreteAlley, origin);
                // reveal: jambs, sill (faces up), head (faces down)
                G1Mesh.Quad(rev, new Vector3(xw, WinY0, za), new Vector3(xw, WinY1, za), new Vector3(xr, WinY1, za), new Vector3(xr, WinY0, za), Vector3.forward);
                G1Mesh.Quad(rev, new Vector3(xw, WinY0, zb), new Vector3(xw, WinY1, zb), new Vector3(xr, WinY1, zb), new Vector3(xr, WinY0, zb), Vector3.back);
                G1Mesh.Quad(rev, new Vector3(xw, WinY0, za), new Vector3(xr, WinY0, za), new Vector3(xr, WinY0, zb), new Vector3(xw, WinY0, zb), Vector3.up);
                G1Mesh.Quad(rev, new Vector3(xw, WinY1, za), new Vector3(xr, WinY1, za), new Vector3(xr, WinY1, zb), new Vector3(xw, WinY1, zb), Vector3.down);
                // frame (painted steel) at the glass plane: four bars and a mullion, 5 cm
                var frame = parts.Get("roomFrame" + r, G1Mat.SteelPainted, origin);
                float f = 0.05f, gx0 = Mathf.Min(xg - s * 0.02f, xg + s * 0.02f), gx1 = Mathf.Max(xg - s * 0.02f, xg + s * 0.02f);
                G1Mesh.Chamfer(frame, new Vector3(gx0, WinY0, za), new Vector3(gx1, WinY0 + f, zb), 0.006f);
                G1Mesh.Chamfer(frame, new Vector3(gx0, WinY1 - f, za), new Vector3(gx1, WinY1, zb), 0.006f);
                G1Mesh.Chamfer(frame, new Vector3(gx0, WinY0 + f, za), new Vector3(gx1, WinY1 - f, za + f), 0.006f);
                G1Mesh.Chamfer(frame, new Vector3(gx0, WinY0 + f, zb - f), new Vector3(gx1, WinY1 - f, zb), 0.006f);
                G1Mesh.Chamfer(frame, new Vector3(gx0, WinY0 + f, 0.5f * (za + zb) - 0.02f), new Vector3(gx1, WinY1 - f, 0.5f * (za + zb) + 0.02f), 0.006f);
                // no glass pane: NocGlass read as a milky sheet over the dark room (AL_r03 KS01); the frame carries the window

                var mb = parts.Get("roomInterior" + r, r == 0 ? G1Mat.AlleyRoomA : G1Mat.AlleyRoomB, origin);
                BuildRoom(mb, r, s, xr, za, zb, pts, frames, sec);
            }
        }

        static void Cyl(MeshBuilder mb, Vector3 b, float r, float h, int sides, bool top, List<Vector3> pts, List<LoftFrame> frames, CrossSection sec)
        {
            pts.Clear(); pts.Add(b); pts.Add(b + new Vector3(0f, h, 0f));
            G1Mesh.Tube(mb, pts, r, sides, frames, sec);
            if (top) MeshKit.Disc(mb, b + new Vector3(0f, h, 0f), Vector3.up, r, sides);
        }

        /// <summary>A box from two corners (any order) with the faces of mask (G1Mesh masks).</summary>
        static void Bx(MeshBuilder mb, Vector3 a, Vector3 b, int mask = G1Mesh.All) => G1Mesh.Box(mb, Vector3.Min(a, b), Vector3.Max(a, b), mask);

        static void BuildRoom(MeshBuilder mb, int r, int s, float xr, float za, float zb, List<Vector3> pts, List<LoftFrame> frames, CrossSection sec)
        {
            var saved = mb.Color;
            float xb = xr + s * Depth, z0 = za - 1.0f, z1 = zb + 1.4f, H = Height;
            Vector3 toWin = new Vector3(-s, 0f, 0f);
            // the room's shell (faces inward); the front wall's inner face is never seen from outside
            Paint(mb, r == 0 ? 0.11f : 0.10f, r == 0 ? 0.092f : 0.085f, r == 0 ? 0.064f : 0.07f, Matte);              // faded wallpaper (C37)
            G1Mesh.Quad(mb, new Vector3(xb, 0f, z0), new Vector3(xb, H, z0), new Vector3(xb, H, z1), new Vector3(xb, 0f, z1), toWin);
            G1Mesh.Quad(mb, new Vector3(xr, 0f, z0), new Vector3(xr, H, z0), new Vector3(xb, H, z0), new Vector3(xb, 0f, z0), Vector3.forward);
            G1Mesh.Quad(mb, new Vector3(xr, 0f, z1), new Vector3(xr, H, z1), new Vector3(xb, H, z1), new Vector3(xb, 0f, z1), Vector3.back);
            Paint(mb, 0.16f, 0.10f, 0.06f, WoodK);
            G1Mesh.Quad(mb, new Vector3(xr, 0f, z0), new Vector3(xb, 0f, z0), new Vector3(xb, 0f, z1), new Vector3(xr, 0f, z1), Vector3.up);
            Paint(mb, 0.24f, 0.23f, 0.21f, Matte);
            G1Mesh.Quad(mb, new Vector3(xr, H, z0), new Vector3(xb, H, z0), new Vector3(xb, H, z1), new Vector3(xr, H, z1), Vector3.down);
            // a skirting board and a dark picture frame on the back wall
            Paint(mb, 0.10f, 0.08f, 0.06f, WoodK);
            Bx(mb, new Vector3(xb, 0f, z0), new Vector3(xb - s * 0.02f, 0.10f, z1), s > 0 ? G1Mesh.NX | G1Mesh.PY : G1Mesh.PX | G1Mesh.PY);
            float pz = 0.5f * (z0 + z1) - 0.6f + 0.9f * r;
            Paint(mb, 0.05f, 0.04f, 0.035f, Matte);
            Bx(mb, new Vector3(xb, 1.35f, pz - 0.3f), new Vector3(xb - s * 0.025f, 1.75f, pz + 0.3f), s > 0 ? G1Mesh.NX | G1Mesh.PY | G1Mesh.NY | G1Mesh.PZ | G1Mesh.NZ : G1Mesh.PX | G1Mesh.PY | G1Mesh.NY | G1Mesh.PZ | G1Mesh.NZ);
            Paint(mb, 0.20f, 0.19f, 0.17f, Matte);
            Bx(mb, new Vector3(xb - s * 0.026f, 1.40f, pz - 0.25f), new Vector3(xb - s * 0.028f, 1.70f, pz + 0.25f), s > 0 ? G1Mesh.NX : G1Mesh.PX);

            // table: top 0.8 x 0.8 at 0.70-0.74, four legs
            Table(r, out Vector3 t);
            Paint(mb, 0.24f, 0.14f, 0.08f, WoodK);
            Bx(mb, new Vector3(t.x - 0.4f, 0.70f, t.z - 0.4f), new Vector3(t.x + 0.4f, 0.74f, t.z + 0.4f));
            foreach (float lx in new[] { -0.34f, 0.34f })
                foreach (float lz in new[] { -0.34f, 0.34f })
                    Bx(mb, new Vector3(t.x + lx - 0.02f, 0f, t.z + lz - 0.02f), new Vector3(t.x + lx + 0.02f, 0.70f, t.z + lz + 0.02f), G1Mesh.Sides);
            // chair: pulled out toward the window, turned 20 deg, seat 0.42 at 0.43-0.47, back to 0.92
            Vector3 cc = t + new Vector3(-s * 0.15f, 0f, -0.75f);
            mb.Push(Matrix4x4.TRS(cc, Quaternion.Euler(0f, 20f * s, 0f), Vector3.one));
            Paint(mb, 0.20f, 0.12f, 0.07f, WoodK);
            Bx(mb, new Vector3(-0.21f, 0.43f, -0.21f), new Vector3(0.21f, 0.47f, 0.21f));
            foreach (float lx in new[] { -0.18f, 0.18f })
                foreach (float lz in new[] { -0.18f, 0.18f })
                    Bx(mb, new Vector3(lx - 0.018f, 0f, lz - 0.018f), new Vector3(lx + 0.018f, 0.43f, lz + 0.018f), G1Mesh.Sides);
            Bx(mb, new Vector3(-0.21f, 0.47f, -0.21f), new Vector3(0.21f, 0.92f, -0.18f));
            mb.Pop();
            // the cup (white, r 4 cm, 9 cm) with dark coffee, near the chair's edge of the table
            var cup = t + new Vector3(-s * 0.12f, 0.74f, -0.22f);
            Paint(mb, 0.72f, 0.70f, 0.66f, Ceramic);
            Cyl(mb, cup, 0.04f, 0.09f, 14, false, pts, frames, sec);
            MeshKit.Torus(mb, cup + new Vector3(0f, 0.045f, 0.052f), Vector3.right, 0.022f, 0.006f, 10, 5);
            Paint(mb, 0.04f, 0.025f, 0.015f, Ceramic);
            MeshKit.Disc(mb, cup + new Vector3(0f, 0.075f, 0f), Vector3.up, 0.036f, 14);
            // the table lamp: base, stem, a self-lit shade
            Vector4 L = Lamp(r);
            var lb = new Vector3(L.x, 0.74f, L.z);
            Paint(mb, 0.08f, 0.07f, 0.06f, Steel);
            Cyl(mb, lb, 0.07f, 0.02f, 14, true, pts, frames, sec);
            Cyl(mb, lb, 0.01f, 0.30f, 6, false, pts, frames, sec);
            Paint(mb, 0.92f, 0.78f, 0.58f, Lit);
            Cyl(mb, new Vector3(L.x, L.y - 0.08f, L.z), 0.13f, 0.17f, 18, true, pts, frames, sec);
            // one curtain half drawn on the window's left (fabric, both faces)
            Paint(mb, r == 0 ? 0.30f : 0.22f, r == 0 ? 0.24f : 0.20f, r == 0 ? 0.18f : 0.24f, Fabric);
            float cx = xr + s * 0.06f, ca = za - 0.35f, cb = za + 0.45f;
            G1Mesh.Quad(mb, new Vector3(cx, 0.75f, ca), new Vector3(cx, 2.45f, ca), new Vector3(cx, 2.45f, cb), new Vector3(cx, 0.75f, cb), toWin);
            G1Mesh.Quad(mb, new Vector3(cx + s * 0.005f, 0.75f, ca), new Vector3(cx + s * 0.005f, 2.45f, ca), new Vector3(cx + s * 0.005f, 2.45f, cb), new Vector3(cx + s * 0.005f, 0.75f, cb), -toWin);
            mb.Color = saved;
        }
    }
}
