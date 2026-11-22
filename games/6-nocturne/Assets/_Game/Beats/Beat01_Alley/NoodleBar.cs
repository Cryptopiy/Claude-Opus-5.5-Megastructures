// NoodleBar: the warm light of beat 1 (MASTER_PROMPT 14.5.1 NoodleBar, KS01 alley_noodle; dimensions in AlleyLayout).
// The west wall stands at x -3.0 over the 6.0 m flat of the profile (s 36-56), so the recess x -3.0..-2.2, z 40..52 is the bar's
// frontage. In the wall: an opening z 43..47 (4.0 m) up to y 2.3; in it the counter 4.0 x 0.9 x 1.0 m (front 3 cm behind the wall
// plane, so the capsule that touches the wall at lane + 0.35 never enters it; its top is the sill of the 4.0 x 1.3 m window
// y 1.0-2.3). Awning y 2.6-3.0 over the counter, 0.8 m out (above the camera sphere, y 1.9); sign 6.0 x 1.4 m at y 3.0-4.4 (NocNeon
// amber, glyph cells rotated along the sign: no real text), 6 cm proud; the bar light WinWarm r 9 i 6 at (-2.0, 2.2, 45.5).
// Phase 4 wave B (AL, INT-07: "the noodle bar must read as a noodle bar, not a pastel bookshop"): a modelled room behind the
// opening, all NocAlleyRoom (G1Mat.AlleyRoomBar: albedo and surface kind in the vertex colour, its own two analytic lamps):
//   * a 25 cm concrete reveal (jambs, head); the room x -3.25..-6.6, z 41.8..50.2, y 0.02..3.0 (a 2 cm threshold step: never
//     co-planar with the alley floor);
//   * white wall tiles (10 cm, grout) on the back and side walls up to 1.6 m, painted plaster above, a dark tiled floor;
//   * the kitchen line along the north wall (the wall KS01's grazing view looks along): a steel counter, three stock pots with dark
//     broth, stacked bowls; steam rising from the pots (NocSteam, a warm kitchen glow, G1Mat.SteamBar) in front of three glyph menu
//     light-boxes (hashed strokes, no real text); on the back wall a shelf of bowls and two more menus;
//   * four stools inside behind the window counter (red seats, 0.78 m: customers sit facing the street), two pendant shades over
//     the counter (the warm pool on it), a short dark teal-navy noren in the top of the opening, two red paper lanterns under the awning
//     (KS01's one red accent);
//   * lamps: Lamp0 over the counter (-3.7, 2.15, 45.0) i 2.6, Lamp1 over the kitchen line (-4.95, 2.5, 49.0) i 1.8, LampWarm.
// KS01 (s 36, yaw -14, pitch +4, vFOV 62): the opening projects to x -0.15..-0.02, y -0.27..+0.02; the counter front y -0.50..-0.22;
// through it the view runs along the north wall (menus, the kitchen end, the steam).
using System.Collections.Generic;
using UnityEngine;

namespace Megastructure
{
    public static class NoodleBar
    {
        /// <summary>LOOK_V2 6 beat 1 (PA): the bar light WinWarm r 9 i 6 (v1 i 7).</summary>
        public const float BarLightIntensityV2 = 6f;
        /// <summary>The room (AL): reveal depth, the room's far x, z extent, ceiling.</summary>
        public const float Reveal = 0.25f, RoomBackX = -6.6f, RoomZ0 = 41.8f, RoomZ1 = 50.2f, RoomCeil = 3.0f, RoomFloor = 0.02f;
        /// <summary>The room's analytic lamps (world xyz, intensity) and their colour, the self-lit gain of shades and menus.</summary>
        public static readonly Vector4 Lamp0 = new Vector4(-3.7f, 2.15f, 45.0f, 2.6f), Lamp1 = new Vector4(-4.95f, 2.5f, 49.0f, 1.8f);
        public static Color LampColour => G1Materials.Lin(NocPal.LampWarm, 1f);
        public const float SelfLit = 4f;

        // surface kinds of NocAlleyRoom (vertex alpha x 32)
        const int Matte = 0, Tiles = 1, WoodK = 2, Lit = 3, Menu = 4, Steel = 5, Fabric = 6, Ceramic = 7;

        static void Paint(MeshBuilder mb, float r, float g, float b, int kind) => mb.Color = new Color32((byte)(r * 255f), (byte)(g * 255f), (byte)(b * 255f), (byte)(kind * 32));

        public static void Build(G1Parts parts)
        {
            var origin = new Vector3(-3f, 0f, 45f);
            float xw = -(float)AlleyLayout.HalfWidth(45.0);                       // -3.0 (the wall plane)
            float xr = xw - Reveal;                                                // -3.25 (the room's front)
            float z0 = (float)AlleyLayout.CounterZ0, z1 = (float)AlleyLayout.CounterZ1;
            float y1 = (float)AlleyLayout.WindowY1, yc = (float)AlleyLayout.CounterH;

            // the reveal of the opening (concrete, faces into the opening)
            var rev = parts.Get("barReveal", G1Mat.ConcreteAlley, origin);
            G1Mesh.Quad(rev, new Vector3(xw, 0f, z0), new Vector3(xw, y1, z0), new Vector3(xr, y1, z0), new Vector3(xr, 0f, z0), Vector3.forward);
            G1Mesh.Quad(rev, new Vector3(xw, 0f, z1), new Vector3(xw, y1, z1), new Vector3(xr, y1, z1), new Vector3(xr, 0f, z1), Vector3.back);
            G1Mesh.Quad(rev, new Vector3(xw, y1, z0), new Vector3(xr, y1, z0), new Vector3(xr, y1, z1), new Vector3(xw, y1, z1), Vector3.down);

            var room = parts.Get("barRoom", G1Mat.AlleyRoomBar, origin);
            BuildRoom(room, xr);

            // counter: front, top and back (its ends meet the reveal's jambs; its bottom sits on the floor)
            var counter = parts.Get("barCounter", G1Mat.Wood, origin);            // Look v2: a wood counter (v1: SteelPainted)
            float cf = xw - (float)AlleyLayout.CounterRecess, cb = cf - (float)AlleyLayout.CounterDepth;
            G1Mesh.Box(counter, new Vector3(cb, 0f, z0), new Vector3(cf, yc, z1), G1Mesh.PX | G1Mesh.PY | G1Mesh.NX);
            // counter top edge trim (brass-like steel band, 2 cm proud of the front, bevelled)
            G1Mesh.Chamfer(counter, new Vector3(cf - 0.04f, yc - 0.05f, z0 + 0.02f), new Vector3(cf + 0.02f, yc + 0.012f, z1 - 0.02f), 0.01f);

            // steam from the stock pots into the hood
            var steam = parts.Get("barSteam", G1Mat.SteamBar, origin);
            for (int i = 0; i < 3; i++)
            {
                Vector3 pp = Pot(i);
                AlleyFixtures.Plume(steam, new Vector3(pp.x, 1.22f, pp.z), 0.75f, 1.75f, (byte)(31 + 70 * i));
            }

            // awning over the frontage (bevelled box, 0.8 m out from the wall)
            var awn = parts.Get("barAwning", G1Mat.SteelPainted, origin);
            G1Mesh.Chamfer(awn, new Vector3(xw - 0.05f, (float)AlleyLayout.AwningY0, (float)AlleyLayout.AwningZ0),
                new Vector3(xw + (float)AlleyLayout.AwningOut, (float)AlleyLayout.AwningY1, (float)AlleyLayout.AwningZ1), 0.02f);

            // sign: NocNeon amber, W 1.4 (vertical) x H 6.0 (along z), facing +x (local X -> world up, local Y -> world +z)
            float sy = 0.5f * (float)(AlleyLayout.SignY0 + AlleyLayout.SignY1), sz = 0.5f * (float)(AlleyLayout.SignZ0 + AlleyLayout.SignZ1);
            parts.Sign(new Vector3(xw + (float)AlleyLayout.SignOff, sy, sz), Quaternion.LookRotation(Vector3.right, Vector3.forward),
                (float)(AlleyLayout.SignY1 - AlleyLayout.SignY0), (float)(AlleyLayout.SignZ1 - AlleyLayout.SignZ0), 2);
            // sign back plate (steel, behind the glyph plane)
            G1Mesh.Chamfer(awn, new Vector3(xw - 0.02f, (float)AlleyLayout.SignY0 - 0.05f, (float)AlleyLayout.SignZ0 - 0.05f),
                new Vector3(xw + (float)AlleyLayout.SignOff - 0.02f, (float)AlleyLayout.SignY1 + 0.05f, (float)AlleyLayout.SignZ1 + 0.05f), 0.01f);

            parts.Light("barLight", new Vector3((float)AlleyLayout.BarLightX, (float)AlleyLayout.BarLightY, (float)AlleyLayout.BarLightZ),
                G1Materials.Lin(NocPal.WinWarm, 1f), (float)AlleyLayout.BarLightRange, BarLightIntensityV2);
        }

        /// <summary>The three stock pots on the north-wall kitchen line (base centre on the counter top, y 0.92).</summary>
        public static Vector3 Pot(int i) => new Vector3(-4.25f - 0.7f * i, 0.92f, RoomZ1 - 0.35f);

        static void Cyl(MeshBuilder mb, Vector3 b, float r, float h, int sides, bool top, List<Vector3> pts, List<LoftFrame> frames, CrossSection sec)
        {
            pts.Clear(); pts.Add(b); pts.Add(b + new Vector3(0f, h, 0f));
            G1Mesh.Tube(mb, pts, r, sides, frames, sec);
            if (top) MeshKit.Disc(mb, b + new Vector3(0f, h, 0f), Vector3.up, r, sides);
        }

        static void BuildRoom(MeshBuilder mb, float xr)
        {
            var saved = mb.Color;
            var pts = new List<Vector3>(4); var frames = new List<LoftFrame>(4); var sec = new CrossSection();
            float xb = RoomBackX, za = RoomZ0, zb = RoomZ1, yt = RoomCeil, yf = RoomFloor, tileTop = 1.6f;
            // floor (dark tiles), its 2 cm threshold face toward the opening, ceiling (warm plaster)
            Paint(mb, 0.10f, 0.085f, 0.07f, Tiles);
            G1Mesh.Quad(mb, new Vector3(xb, yf, za), new Vector3(xr, yf, za), new Vector3(xr, yf, zb), new Vector3(xb, yf, zb), Vector3.up);
            G1Mesh.Quad(mb, new Vector3(xr, 0f, za), new Vector3(xr, yf, za), new Vector3(xr, yf, zb), new Vector3(xr, 0f, zb), Vector3.right);
            Paint(mb, 0.36f, 0.30f, 0.22f, Matte);
            G1Mesh.Quad(mb, new Vector3(xb, yt, za), new Vector3(xr, yt, za), new Vector3(xr, yt, zb), new Vector3(xb, yt, zb), Vector3.down);
            // back wall x -6.6 and side walls z 41.8 / 50.2: tiles to 1.6 m, plaster above (deep red-brown)
            Paint(mb, 0.44f, 0.41f, 0.35f, Tiles);
            G1Mesh.Quad(mb, new Vector3(xb, yf, za), new Vector3(xb, tileTop, za), new Vector3(xb, tileTop, zb), new Vector3(xb, yf, zb), Vector3.right);
            G1Mesh.Quad(mb, new Vector3(xb, yf, za), new Vector3(xb, tileTop, za), new Vector3(xr, tileTop, za), new Vector3(xr, yf, za), Vector3.forward);
            G1Mesh.Quad(mb, new Vector3(xb, yf, zb), new Vector3(xb, tileTop, zb), new Vector3(xr, tileTop, zb), new Vector3(xr, yf, zb), Vector3.back);
            Paint(mb, 0.24f, 0.11f, 0.07f, Matte);
            G1Mesh.Quad(mb, new Vector3(xb, tileTop, za), new Vector3(xb, yt, za), new Vector3(xb, yt, zb), new Vector3(xb, tileTop, zb), Vector3.right);
            G1Mesh.Quad(mb, new Vector3(xb, tileTop, za), new Vector3(xb, yt, za), new Vector3(xr, yt, za), new Vector3(xr, tileTop, za), Vector3.forward);
            G1Mesh.Quad(mb, new Vector3(xb, tileTop, zb), new Vector3(xb, yt, zb), new Vector3(xr, yt, zb), new Vector3(xr, tileTop, zb), Vector3.back);
            // a dark wood dado rail at the tile top on every wall (5 cm)
            Paint(mb, 0.12f, 0.07f, 0.04f, WoodK);
            G1Mesh.Box(mb, new Vector3(xb, tileTop - 0.03f, za + 0.02f), new Vector3(xb + 0.03f, tileTop + 0.03f, zb - 0.02f), G1Mesh.PX | G1Mesh.PY | G1Mesh.NY);
            G1Mesh.Box(mb, new Vector3(xb + 0.02f, tileTop - 0.03f, zb - 0.03f), new Vector3(xr, tileTop + 0.03f, zb), G1Mesh.NZ | G1Mesh.PY | G1Mesh.NY);
            G1Mesh.Box(mb, new Vector3(xb + 0.02f, tileTop - 0.03f, za), new Vector3(xr, tileTop + 0.03f, za + 0.03f), G1Mesh.PZ | G1Mesh.PY | G1Mesh.NY);

            // kitchen line along the north wall (the wall KS01's view runs along): steel counter, pots with broth, stacked bowls; the
            // menu light-boxes above it, steam rising from the pots in front of them
            float kz = zb - 0.7f;
            Paint(mb, 0.30f, 0.30f, 0.31f, Steel);
            G1Mesh.Box(mb, new Vector3(-6.45f, yf, kz), new Vector3(-3.5f, 0.92f, zb), G1Mesh.PX | G1Mesh.NX | G1Mesh.PY | G1Mesh.NZ);
            for (int i = 0; i < 3; i++)
            {
                Vector3 pp = Pot(i);
                Paint(mb, 0.34f, 0.34f, 0.35f, Steel);
                Cyl(mb, new Vector3(pp.x, 0.92f, pp.z), 0.21f, 0.30f, 20, false, pts, frames, sec);
                MeshKit.Torus(mb, new Vector3(pp.x, 1.2f, pp.z), Vector3.up, 0.21f, 0.012f, 20, 6);
                Paint(mb, 0.09f, 0.05f, 0.025f, Ceramic);                                           // broth
                MeshKit.Disc(mb, new Vector3(pp.x, 1.19f, pp.z), Vector3.up, 0.20f, 20);
            }
            Paint(mb, 0.62f, 0.60f, 0.55f, Ceramic);
            for (int i = 0; i < 4; i++)
            {
                float bx = -6.1f + 0.26f * (i & 1), bz = kz + 0.2f + 0.26f * (i >> 1);
                for (int k = 0; k < 3; k++) Cyl(mb, new Vector3(bx, 0.92f + 0.06f * k, bz), 0.11f, 0.055f, 16, k == 2, pts, frames, sec);
            }
            // back wall: a wood shelf with bowl stacks and two menu light-boxes above it
            Paint(mb, 0.14f, 0.08f, 0.045f, WoodK);
            G1Mesh.Box(mb, new Vector3(xb, 1.50f, za + 0.8f), new Vector3(xb + 0.28f, 1.54f, zb - 1.2f), G1Mesh.PX | G1Mesh.PY | G1Mesh.NY);
            Paint(mb, 0.62f, 0.60f, 0.55f, Ceramic);
            for (float bz2 = za + 1.1f; bz2 < zb - 1.4f; bz2 += 0.9f)
                for (int k = 0; k < 4; k++) Cyl(mb, new Vector3(xb + 0.15f, 1.54f + 0.06f * k, bz2), 0.11f, 0.055f, 16, k == 3, pts, frames, sec);

            // menu light-boxes on the north wall (facing -z) over the kitchen line, two on the back wall
            Paint(mb, 0.34f, 0.28f, 0.19f, Menu);
            foreach (var m in new[] { new Vector2(-3.6f, -4.4f), new Vector2(-4.6f, -5.4f), new Vector2(-5.6f, -6.35f) })
                G1Mesh.Quad(mb, new Vector3(m.x, 1.75f, zb - 0.015f), new Vector3(m.x, 2.35f, zb - 0.015f), new Vector3(m.y, 2.35f, zb - 0.015f), new Vector3(m.y, 1.75f, zb - 0.015f), Vector3.back);
            foreach (var m in new[] { new Vector2(za + 1.0f, za + 2.6f), new Vector2(za + 3.0f, za + 4.6f) })
                G1Mesh.Quad(mb, new Vector3(xb + 0.015f, 1.9f, m.x), new Vector3(xb + 0.015f, 2.5f, m.x), new Vector3(xb + 0.015f, 2.5f, m.y), new Vector3(xb + 0.015f, 1.9f, m.y), Vector3.right);

            // four stools behind the window counter (red seats at 0.78 m, steel legs and foot rings)
            for (int i = 0; i < 4; i++)
            {
                var b = new Vector3(-4.3f, yf, 43.6f + 0.95f * i);
                Paint(mb, 0.26f, 0.27f, 0.28f, Steel);
                MeshKit.Disc(mb, b + new Vector3(0f, 0.01f, 0f), Vector3.up, 0.19f, 16);
                Cyl(mb, b, 0.028f, 0.74f, 8, false, pts, frames, sec);
                MeshKit.Torus(mb, b + new Vector3(0f, 0.28f, 0f), Vector3.up, 0.15f, 0.012f, 16, 6);
                Paint(mb, 0.36f, 0.05f, 0.035f, Matte);
                Cyl(mb, b + new Vector3(0f, 0.72f, 0f), 0.18f, 0.06f, 18, true, pts, frames, sec);
            }

            // two pendant shades over the counter with cords (self-lit warm), the warm pool on the counter comes from Lamp0
            foreach (float pz in new[] { 44.0f, 46.0f })
            {
                Paint(mb, 0.05f, 0.05f, 0.05f, Matte);
                Cyl(mb, new Vector3(-3.7f, 2.40f, pz), 0.006f, yt - 2.40f, 6, false, pts, frames, sec);
                Paint(mb, 0.95f, 0.72f, 0.42f, Lit);
                Cyl(mb, new Vector3(-3.7f, 2.20f, pz), 0.17f, 0.20f, 20, true, pts, frames, sec);
            }

            // the noren: four dark teal-navy panels in the top of the opening (y 1.95-2.28), 12 cm behind the wall plane
            Paint(mb, 0.025f, 0.055f, 0.07f, Fabric);
            float nx = -3.12f;
            for (int i = 0; i < 4; i++)
            {
                float a = 43.03f + i * 0.99f, b2 = a + 0.97f;
                G1Mesh.Quad(mb, new Vector3(nx, 1.95f, a), new Vector3(nx, 2.28f, a), new Vector3(nx, 2.28f, b2), new Vector3(nx, 1.95f, b2), Vector3.right);
                G1Mesh.Quad(mb, new Vector3(nx - 0.005f, 1.95f, a), new Vector3(nx - 0.005f, 2.28f, a), new Vector3(nx - 0.005f, 2.28f, b2), new Vector3(nx - 0.005f, 1.95f, b2), Vector3.left);
            }

            // two red paper lanterns under the awning, flanking the opening (self-lit, with black caps)
            foreach (float lz in new[] { 42.1f, 47.9f })
            {
                var c = new Vector3(-2.62f, 2.38f, lz);
                Paint(mb, 0.55f, 0.07f, 0.035f, Lit);
                MeshKit.Sphere(mb, c, 0.15f, 16, 8);
                Paint(mb, 0.03f, 0.03f, 0.03f, Matte);
                Cyl(mb, c + new Vector3(0f, 0.12f, 0f), 0.07f, 0.04f, 12, true, pts, frames, sec);
                Cyl(mb, c + new Vector3(0f, -0.16f, 0f), 0.07f, 0.04f, 12, false, pts, frames, sec);
                Cyl(mb, c + new Vector3(0f, 0.16f, 0f), 0.005f, 2.6f - 2.54f, 6, false, pts, frames, sec);
            }
            mb.Color = saved;
        }

        /// <summary>Wet-floor sources of the bar (14.7 H4): the window strip and the sign, at their own s.</summary>
        public static void RegisterSources()
        {
            float xw = -(float)AlleyLayout.HalfWidth(45.0);
            NocWetRegistry.AddSource(new Vector3(xw - 0.2f, 1.65f, (float)AlleyLayout.CounterZ0 + 0.3f), new Vector3(xw - 0.2f, 1.65f, (float)AlleyLayout.CounterZ1 - 0.3f),
                0.65f, G1Materials.Lin(NocPal.WinWarm, G1Materials.WinWarmFactor * G1Materials.ShopSourceGain), 45.0);
            NocWetRegistry.AddSource(new Vector3(xw + 0.06f, 3.7f, (float)AlleyLayout.SignZ0 + 0.5f), new Vector3(xw + 0.06f, 3.7f, (float)AlleyLayout.SignZ1 - 0.5f),
                0.7f, G1Materials.Lin(NocPal.SodiumAmber, 0.5f), 45.0);
        }
    }
}
