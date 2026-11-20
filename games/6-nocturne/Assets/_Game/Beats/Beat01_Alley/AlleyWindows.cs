// AlleyWindows: lit windows in the two podium walls of the alley (MASTER_PROMPT 14.5.1 "warm light in a cold wet dark"; graybox
// life above the street: the 14.10 beat-1 luma budget counts 8 % neon and 17 % walls at 45). A grid of 1.0 x 1.4 m windows every
// 4.5 m along z and 3.2 m up from y 3.6 (west podium 6 floors, east 8), 45 % lit (60 % warm WinWarm x 0.9, 40 % cold WinCold
// x 0.6), each 2 cm proud of its wall and following it on the profile's flanks (y >= 3.6: far above the capsule); none over the bar
// frontage, the vending niche, a fire escape, a blade sign, an AC unit or the catwalk; two columns on the dead-end face above the
// door. Deterministic hash per (side, column, floor). The mean wall radiance this gives is the alley's wet-mirror wall macro.
using UnityEngine;

namespace Megastructure
{
    public static class AlleyWindows
    {
        public static void Build(G1Parts parts)
        {
            var origin = new Vector3(0f, 10f, 60f);
            var warm = parts.Get("alleyWinWarm", G1Mat.EmitShopWarm, origin);
            var cold = parts.Get("alleyWinCold", G1Mat.EmitShopCold, origin);
            float w = 0.5f * (float)AlleyLayout.WinW, h = (float)AlleyLayout.WinH;
            for (int side = -1; side <= 1; side += 2)
            {
                float top = (float)(side < 0 ? AlleyLayout.WestPodiumH : AlleyLayout.EastPodiumH) - 2f;
                int col = 0;
                for (double z = 2.0; z <= 118.0; z += AlleyLayout.WinPitch, col++)
                {
                    double za = z - w, zb = z + w;
                    // the quad follows the wall: its two vertical edges at the local wall line (flanks included), 2 cm proud
                    float xa = side * ((float)AlleyLayout.HalfWidth(za) - (float)AlleyLayout.WinProud);
                    float xb = side * ((float)AlleyLayout.HalfWidth(zb) - (float)AlleyLayout.WinProud);
                    if (zb > AlleyLayout.MouthZ - 0.3) continue;
                    int floor = 0;
                    for (float y = (float)AlleyLayout.WinY0; y + h <= top; y += (float)AlleyLayout.WinFloor, floor++)
                    {
                        if (Blocked(side, za, zb, y)) continue;
                        uint hsh = NocMath.Pcg((uint)(col * 131 + floor * 7 + (side > 0 ? 9001 : 0)) ^ 0x57494E44u);
                        float u = (float)NocMath.U01(hsh);
                        if (u > AlleyLayout.WinLit) continue;
                        var mb = u < AlleyLayout.WinLit * AlleyLayout.WinWarmShare ? warm : cold;
                        G1Mesh.Quad(mb, new Vector3(xa, y, (float)za), new Vector3(xa, y + h, (float)za), new Vector3(xb, y + h, (float)zb),
                            new Vector3(xb, y, (float)zb), new Vector3(-side, 0f, 0f));
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
                    float u = (float)NocMath.U01(hsh);
                    if (u > AlleyLayout.WinLit) continue;
                    var mb = u < AlleyLayout.WinLit * AlleyLayout.WinWarmShare ? warm : cold;
                    G1Mesh.Quad(mb, new Vector3(xc - w, y, zd), new Vector3(xc - w, y + h, zd), new Vector3(xc + w, y + h, zd), new Vector3(xc + w, y, zd), Vector3.forward);
                }
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
            for (int i = 0; i < AlleyLayout.AcZ.Length; i++)
            {
                if ((int)AlleyLayout.AcSide[i] != side) continue;
                if (zb > AlleyLayout.AcZ[i] - 1.0 && za < AlleyLayout.AcZ[i] + 1.0 && y1 > AlleyLayout.AcY[i] - 0.2 && y0 < AlleyLayout.AcY[i] + 0.8) return true;
            }
            return false;
        }
    }
}
