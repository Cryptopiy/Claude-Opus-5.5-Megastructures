// AlleyWindows: the windows of the two podium walls of the alley (MASTER_PROMPT 14.5.1 "warm light in a cold wet dark"; the 14.10
// beat-1 luma budget counts 8 % neon and 17 % walls at 45). A grid of 1.0 x 1.4 m openings every 4.5 m along z and 3.2 m up from
// y 3.6 (west podium 5 floors, east 8), none over the bar frontage, the vending niche, a fire escape, a blade sign, an AC unit or
// the catwalk; two columns on the dead-end face above the door. Deterministic hash per (side, column, floor).
// Phase 4 wave B (AL, INT-06: "flat white or cream window cards floating on the walls"): EVERY opening of the grid is a window now,
// 45 % lit (60 % warm, 40 % cold) and the rest dark at stand-by light; each is
//   * a glass panel 2 cm proud of the wall, following it on the profile's flanks, drawn by NocAlleyWindow (the 14 cm reveal, the
//     frame, mullion and transom, the room behind with its lamp, furniture and a curtain or blind; G1Mat.AlleyWindow). Its vertex
//     colour carries the window's state: R = random, G = lit, B = covering x 32 (0 none, 1 open curtains, 2 a sheer, 3 a blind,
//     4 one curtain half drawn), A = cold;
//   * a precast sill (W + 0.16 x 0.06 m, 7 cm proud) and a drip head (W + 0.10 x 0.05 m, 3 cm proud), chamfered, along the wall.
// The mean wall radiance these give is the alley's wet-mirror wall macro (NocSurfaceMath.AlleyWallMacro: the lit share and the
// room light are unchanged, 0.45 x WinWarm 0.9 / WinCold 0.6 x the shop-glass gain 0.6).
using UnityEngine;

namespace Megastructure
{
    public static class AlleyWindows
    {
        /// <summary>INT-21: the window spill pools on the facing wall (light colour lerp(c, white, 0.6) x gain).</summary>
        public const float SpillWarm = 0.95f, SpillCold = 0.75f;
        public const float SillOut = 0.07f, SillIn = 0.02f, SillH = 0.06f, SillOver = 0.08f, HeadOut = 0.03f, HeadH = 0.05f, HeadOver = 0.05f;

        /// <summary>Number of window panels built and lit (the last Build; logged by the beat).</summary>
        public static int Built, Lit;

        public static void Build(G1Parts parts)
        {
            var origin = new Vector3(0f, 10f, 60f);
            var glass = parts.Get("alleyWindows", G1Mat.AlleyWindow, origin);
            var trim = parts.Get("alleyWindowTrim", G1Mat.Concrete, origin);
            var pools = parts.Get("lightPools", G1Mat.AlleyPool, new Vector3(0f, 3f, 60f));
            float w = 0.5f * (float)AlleyLayout.WinW, h = (float)AlleyLayout.WinH;
            Built = Lit = 0;
            for (int side = -1; side <= 1; side += 2)
            {
                float top = (float)(side < 0 ? AlleyLayout.WestPodiumH : AlleyLayout.EastPodiumH) - 2f;
                int col = 0;
                for (double z = 2.0; z <= 118.0; z += AlleyLayout.WinPitch, col++)
                {
                    double za = z - w, zb = z + w;
                    if (zb > AlleyLayout.MouthZ - 0.3) continue;
                    // the panel follows the wall: its two vertical edges at the local wall line (flanks included), 2 cm proud
                    float xa = side * ((float)AlleyLayout.HalfWidth(za) - (float)AlleyLayout.WinProud);
                    float xb = side * ((float)AlleyLayout.HalfWidth(zb) - (float)AlleyLayout.WinProud);
                    int floor = 0;
                    for (float y = (float)AlleyLayout.WinY0; y + h <= top; y += (float)AlleyLayout.WinFloor, floor++)
                    {
                        if (Blocked(side, za, zb, y)) continue;
                        uint hsh = NocMath.Pcg((uint)(col * 131 + floor * 7 + (side > 0 ? 9001 : 0)) ^ 0x57494E44u);
                        Panel(glass, trim, hsh, side, (float)za, (float)zb, xa, xb, y, h, new Vector3(-side, 0f, 0f));
                        // INT-21: a lit window is a 1.0 x 1.4 m area light 4.4-6 m from the facing wall (E ~ L A / d^2 ~ 0.07 L: about
                        // the ambient again), a broad soft pool there
                        State(hsh, out bool lit);
                        if (lit)
                        {
                            bool cold = State(hsh, out _).a > 127;
                            Color lc = Color.Lerp(NocturnePalette.Linear(cold ? NocPal.WinCold : NocPal.WinWarm), Color.white, 0.6f) * ((cold ? SpillCold : SpillWarm) * AlleyFixtures.PoolLookDev);
                            float zc = (float)z, d = 2f * (float)AlleyLayout.HalfWidth(z);
                            float topW = (float)(-side < 0 ? AlleyLayout.WestPodiumH : AlleyLayout.EastPodiumH) - 0.05f;
                            float pz0 = Mathf.Max((float)AlleyLayout.DeadEndZ + 0.05f, zc - 3.6f), pz1 = Mathf.Min((float)AlleyLayout.MouthZ - 0.1f, zc + 3.6f);
                            AlleyFixtures.PoolPanel(pools, -side, pz0, pz1, Mathf.Max(0.05f, y + 0.7f - 3.6f), Mathf.Min(topW, y + 0.7f + 3.6f), zc, y + 0.7f, d, lc);
                        }
                    }
                }
            }
            // the dead-end face (z -2.5, x -2.2..2.2): two columns of windows above the door, seen at the end of every look back
            float zd = (float)AlleyLayout.DeadEndZ + (float)AlleyLayout.WinProud;
            int k = 0;
            for (float y = 4.4f; y + h <= (float)AlleyLayout.DeadEndH - 1.5f; y += (float)AlleyLayout.WinFloor)
                foreach (float xc in new[] { -1.1f, 1.1f })
                {
                    uint hsh = NocMath.Pcg((uint)(k++ * 37 + 5) ^ 0x44454144u);
                    DeadEndPanel(glass, trim, hsh, xc - w, xc + w, zd, y, h);
                }
        }

        /// <summary>The panel state of hash h: lit, cold, covering (0-4) and the random channel.</summary>
        public static Color32 State(uint h, out bool lit)
        {
            float u = (float)NocMath.U01(h);
            lit = u <= AlleyLayout.WinLit;
            bool cold = lit ? u >= AlleyLayout.WinLit * AlleyLayout.WinWarmShare : NocMath.U01(NocMath.Pcg(h + 3u)) < 0.4;
            float c = (float)NocMath.U01(NocMath.Pcg(h + 7u));
            int cover = c < 0.25f ? 0 : (c < 0.55f ? 1 : (c < 0.70f ? 2 : (c < 0.90f ? 3 : 4)));
            byte r = (byte)(NocMath.Pcg(h + 11u) >> 24);
            return new Color32(r, (byte)(lit ? 255 : 0), (byte)(cover * 32), (byte)(cold ? 255 : 0));
        }

        static void Panel(MeshBuilder glass, MeshBuilder trim, uint hsh, int side, float za, float zb, float xa, float xb, float y, float h, Vector3 inward)
        {
            var saved = glass.Color;
            glass.Color = State(hsh, out bool lit);
            G1Mesh.Quad(glass, new Vector3(xa, y, za), new Vector3(xa, y + h, za), new Vector3(xb, y + h, zb), new Vector3(xb, y, zb), inward);
            glass.Color = saved;
            Built++; if (lit) Lit++;
            // sill and drip head along the wall line (the wall may slant on the flanks: the boxes turn with it)
            float zc = 0.5f * (za + zb);
            float xw0 = side * (float)AlleyLayout.HalfWidth(za), xw1 = side * (float)AlleyLayout.HalfWidth(zb);
            WallBox(trim, side, xw0, za, xw1, zb, SillOver, y - SillH, y, SillOut, SillIn, 0.012f);
            WallBox(trim, side, xw0, za, xw1, zb, HeadOver, y + h + 0.06f, y + h + 0.06f + HeadH, HeadOut, SillIn, 0.01f);
        }

        static void DeadEndPanel(MeshBuilder glass, MeshBuilder trim, uint hsh, float x0, float x1, float zd, float y, float h)
        {
            var saved = glass.Color;
            glass.Color = State(hsh, out bool lit);
            G1Mesh.Quad(glass, new Vector3(x0, y, zd), new Vector3(x0, y + h, zd), new Vector3(x1, y + h, zd), new Vector3(x1, y, zd), Vector3.forward);
            glass.Color = saved;
            Built++; if (lit) Lit++;
            float zw = (float)AlleyLayout.DeadEndZ;
            G1Mesh.Chamfer(trim, new Vector3(x0 - SillOver, y - SillH, zw - SillIn), new Vector3(x1 + SillOver, y, zw + SillOut), 0.012f);
            G1Mesh.Chamfer(trim, new Vector3(x0 - HeadOver, y + h + 0.06f, zw - SillIn), new Vector3(x1 + HeadOver, y + h + 0.06f + HeadH, zw + HeadOut), 0.01f);
        }

        /// <summary>A chamfered box lying along the wall segment (xw0, za)-(xw1, zb) grown by `over` at both ends, from y0 to y1,
        /// `outD` proud of the wall into the alley and `inD` into it (turned with the wall on the profile's flanks).</summary>
        public static void WallBox(MeshBuilder mb, int side, float xw0, float za, float xw1, float zb, float over, float y0, float y1, float outD, float inD, float bevel)
        {
            var a = new Vector3(xw0, 0f, za); var b = new Vector3(xw1, 0f, zb);
            Vector3 dir = b - a; float len = dir.magnitude;
            if (len < 1e-4f) return;
            dir /= len;
            Vector3 inward = new Vector3(-side, 0f, 0f);
            // the wall's inward normal on a slanted segment: perpendicular to dir in the horizontal plane, toward the alley
            Vector3 nrm = Vector3.Cross(Vector3.up, dir);
            if (Vector3.Dot(nrm, inward) < 0f) nrm = -nrm;
            Vector3 mid = 0.5f * (a + b) + nrm * (0.5f * (outD - inD));
            mid.y = 0.5f * (y0 + y1);
            var rot = Quaternion.LookRotation(dir, Vector3.up);         // local z along the wall, local x across it
            MeshKit.ChamferBox(mb, mid, new Vector3(outD + inD, y1 - y0, len + 2f * over), bevel, rot);
        }

        /// <summary>True when a window over za..zb at height y..y + 1.4 (+-0.3 m) would touch a fixture of that wall.</summary>
        static bool Blocked(int side, double za, double zb, double y)
        {
            double y0 = y - 0.3, y1 = y + AlleyLayout.WinH + 0.3;
            if (side < 0 && zb > AlleyLayout.BarZ0 - 1.0 && za < AlleyLayout.BarZ1 + 1.0 && y0 < 5.5) return true;          // bar frontage + sign
            if (side > 0 && zb > VendingMachines.NicheZ0 - 0.5 && za < VendingMachines.NicheZ1 + 0.5 && y0 < 3.0) return true;
            if (zb > AlleyLayout.CatwalkZ0 - 1.0 && za < AlleyLayout.CatwalkZ1 + 1.0 && y1 > 8.0 && y0 < 11.0) return true;
            double[] esc = side < 0 ? AlleyLayout.EscapeWestZ : AlleyLayout.EscapeEastZ;
            foreach (double e in esc) if (zb > e - 0.6 && za < e + AlleyLayout.EscapeLandingL + 0.6) return true;      // whole height
            for (int i = 0; i < AlleyLayout.BladeSignCount; i++)
            {
                if (AlleyFixtures.SignSide(i) != side) continue;
                Vector3 c = AlleyFixtures.SignCentre(i);
                if (zb > c.z - 0.8 && za < c.z + 0.8 && y1 > c.y - 1.2 && y0 < c.y + 1.2) return true;
            }
            for (int i = 0; i < AlleyFixtures.AcCount; i++)
            {
                AlleyFixtures.Ac(i, out int s, out float z, out float yy, out _);
                if (s != side) continue;
                if (zb > z - 1.0 && za < z + 1.0 && y1 > yy - 0.2 && y0 < yy + 0.8) return true;
            }
            return false;
        }
    }
}
