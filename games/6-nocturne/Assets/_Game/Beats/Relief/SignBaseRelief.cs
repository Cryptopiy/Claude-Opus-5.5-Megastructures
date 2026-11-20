// SignBaseRelief: the base of the wall of signs (TEXTURE_PLAN 5 item 5, CRIT0 #22): a 2 km screen that meets the promenade floor
// as a bare LED face has no human scale; a real screen wall stands on a street-level arcade of shops. In front of the screen face
// (x 874, ScreenWallBuilder not edited) along z 281..515: a 6.6 m arcade (concrete columns 0.8 m every 9 m at x 880.6..881.4, a
// canopy slab y 6.0..6.6 out to x 882.2 with a painted-steel fascia and a dim light line under its edge) and one shopfront per bay
// on the plane x 874.3 (y 0.4..5.4, steel frames 0.2 m). SG (Phase 4 wave A, INT-34: "flat lit panes"): the panes are no longer
// relief parts; Beat06_Signs builds them from ShopBays as NocShopfront rooms with depth (4 in 5 cold, 1 in 5 neutral, 1 in 7
// closed) and hangs a small NocNeon blade sign under the canopy of every open bay (ShopSign).
// Everything stays west of x 882.3 (the walk lane is x 891.74..911.74, its wall face at 891.36) and below y 6.6 (KS16's wall point
// at (874, 10, 300) and the sight lines to the screen above it pass over the canopy: the eye's line crosses x 882 at y 7.6).
// TEXTURE_PLAN 5 relief: built with ReliefBuilder.ReliefOn (the v2 facade), like the other relief.
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Megastructure
{
    public static class SignBaseRelief
    {
        public const double X0 = 874.0, ShopX = 874.3, ColX0 = 880.6, ColX1 = 881.4, CanopyX1 = 882.2, Z0 = 281.0, Z1 = 515.0;
        public const double CanopyY0 = 6.0, CanopyY1 = 6.6, ShopY0 = 0.4, ShopY1 = 5.4, Pitch = 9.0, Frame = 0.2;
        /// <summary>Beat root: beat 6, the wall of signs.</summary>
        public const int Beat = 5;
        public static int Bays;

        public static void Build(List<ReliefPart> parts)
        {
            var origin = new Vector3(878f, 3f, 398f);
            var conc = new ReliefPart("SignBase_concrete", ReliefMat.Concrete, origin, Beat);
            var steel = new ReliefPart("SignBase_steel", ReliefMat.Steel, origin, Beat);
            var line = new ReliefPart("SignBase_canopyLight", ReliefMat.TunnelLight, origin, Beat);
            parts.Add(conc); parts.Add(steel); parts.Add(line);
            int n = (int)Math.Floor((Z1 - Z0) / Pitch);
            double z0 = 0.5 * (Z0 + Z1) - 0.5 * n * Pitch;
            Bays = n;
            // canopy, fascia, light line
            Box(conc, X0 - 0.1, CanopyX1, CanopyY0, CanopyY1, z0 - 0.4, z0 + n * Pitch + 0.4);
            Box(steel, CanopyX1 - 0.25, CanopyX1 + 0.08, CanopyY0 - 0.35, CanopyY1 + 0.04, z0 - 0.45, z0 + n * Pitch + 0.45);
            Quad(line.Mb, origin, CanopyX1 - 0.6, CanopyX1 - 0.35, CanopyY0 - 0.01, z0 + 0.3, z0 + n * Pitch - 0.3);
            for (int k = 0; k <= n; k++)
            {
                double zc = z0 + k * Pitch;
                Box(conc, ColX0, ColX1, -0.05, CanopyY0 + 0.05, zc - 0.4, zc + 0.4);                    // column (into floor and canopy)
                if (k == n) continue;
                // shopfront of bay k: a steel frame around the room glass (the glass itself: ShopBays, built by Beat06_Signs)
                double za = zc + 0.4 + Frame, zb = zc + Pitch - 0.4 - Frame;
                Box(steel, ShopX - 0.05, ShopX + 0.12, ShopY0 - Frame, ShopY0, za - Frame, zb + Frame);      // sill
                Box(steel, ShopX - 0.05, ShopX + 0.12, ShopY1, ShopY1 + Frame, za - Frame, zb + Frame);      // head
                Box(steel, ShopX - 0.05, ShopX + 0.12, ShopY0, ShopY1, za - Frame, za);                       // jambs
                Box(steel, ShopX - 0.05, ShopX + 0.12, ShopY0, ShopY1, zb, zb + Frame);
                Box(steel, ShopX - 0.05, ShopX + 0.10, ShopY0, ShopY1, 0.5 * (za + zb) - 0.06, 0.5 * (za + zb) + 0.06);   // mullion
                // the wall between the shops and below the canopy (concrete pier faces at x 874.15)
                Box(conc, X0 - 0.1, X0 + 0.15, -0.05, CanopyY0, zc - 0.4 - 0.001, za - Frame);
            }
            Box(conc, X0 - 0.1, X0 + 0.15, -0.05, CanopyY0, z0 + n * Pitch - 0.4, z0 + n * Pitch + 0.4);
            Box(conc, X0 - 0.1, X0 + 0.15, ShopY1 + Frame, CanopyY0, z0, z0 + n * Pitch);                   // band over the shops
            Box(conc, X0 - 0.1, X0 + 0.2, -0.05, ShopY0 - Frame, z0, z0 + n * Pitch);                       // plinth under the shops
        }

        static void Box(ReliefPart part, double x0, double x1, double y0, double y1, double z0, double z1)
        {
            var lo = new Vector3((float)x0, (float)y0, (float)z0) - part.Origin;
            var hi = new Vector3((float)x1, (float)y1, (float)z1) - part.Origin;
            part.Mb.Box(0.5f * (lo + hi), hi - lo);
        }

        /// <summary>The shop glass of every bay (world z range of the pane on the plane x ShopX, y ShopY0..ShopY1) and its kind:
        /// 0 cold, 1 neutral, 2 closed (the pre-SG 1 in 5 warm bays keep their hash and become neutral).</summary>
        public static List<(double za, double zb, int kind)> ShopBays()
        {
            var l = new List<(double, double, int)>(32);
            int n = (int)Math.Floor((Z1 - Z0) / Pitch);
            double z0 = 0.5 * (Z0 + Z1) - 0.5 * n * Pitch;
            for (int k = 0; k < n; k++)
            {
                double zc = z0 + k * Pitch;
                uint h = NocHash.H3(unchecked((uint)(k + 7000)), 0x53484F50u, 0u, 0);
                int kind = NocHash.U01(h) < 0.2 ? 1 : (NocHash.U01(NocHash.Pcg(h)) < 1.0 / 7.0 ? 2 : 0);
                l.Add((zc + 0.4 + Frame, zc + Pitch - 0.4 - Frame, kind));
            }
            return l;
        }

        /// <summary>The NocNeon blade sign of a bay: 0.6 x 1.8 m, perpendicular to the face, under the canopy at x 880.4..881.0.</summary>
        public static Vector3 ShopSignCentre(double za, double zb) => new Vector3(880.7f, 5.0f, (float)(0.5 * (za + zb)));
        public const double ShopSignW = 0.6, ShopSignH = 1.8;

        static void Quad(MeshBuilder mb, Vector3 o, double x0, double x1, double y, double z0, double z1)
            => EdgeMesh.FlatQuad(mb, o, ReliefKit.P(x0, y, z0), ReliefKit.P(x1, y, z0), ReliefKit.P(x1, y, z1), ReliefKit.P(x0, y, z1), Vector3.down);
    }
}
