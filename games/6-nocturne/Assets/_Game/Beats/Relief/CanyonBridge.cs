// CanyonBridge: the six-storey skybridge across the canyon mouth at the face-A plane (GE, D-029 GB-02; GateDims part 1). It carries
// the upper tube's west portal (y 118, z 195) on its east face, so the tube no longer starts in mid-air over the tongue root.
//   body x 212.25..232.25, y 108..132, z 98..292: NocFacade facade mode on the long faces (west toward the boulevard, east toward
//   the Gulf) and on the two end faces that stand in front of face A's flanks (x 215.3..232.25 at z 98 / 292), roof mode on the roof;
//   the west face from x 212.25 (the last north blade x 198..212 projects to z 263); the parts inside the slab-6 shafts and face A's
//   flank masses are left out where they are buried (west face z < 116 / > 274, end faces x < 215.3);
//   concrete: the soffit, chamfered fascia bands along both long faces (2.4 m at the soffit, 1.0 m at the roof, 0.5 m proud), a
//   parapet, soffit ribs every 8 m between the fascias;
//   the tube portal: a 14 m panel with the r 6.3 hole on the east face, collar 1 m proud, NeonCyan ring, tunnel 16 m into the body.
// Every lane passes 6.5 m or more above the parapet (lowest lane y 140; parapet top 133.2). Nothing reaches below y 105.6.
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Megastructure
{
    public static class CanyonBridge
    {
        public const uint IdBody = 0x6100u;
        /// <summary>Beat root: beat 3, The Edge (the bridge stands at the beat 2 / 3 boundary, s 400).</summary>
        public const int Beat = 2;
        /// <summary>The visible z range of the west face (between the slab-6 shaft faces z 116 / 274).</summary>
        public const double WestZ0 = 116.0, WestZ1 = 274.0;

        public static void Build(List<ReliefPart> parts)
        {
            double x0 = GateDims.CbX0, x1 = GateDims.CbX1, y0 = GateDims.CbY0, y1 = GateDims.CbY1, z0 = GateDims.CbZ0, z1 = GateDims.CbZ1;
            var origin = ReliefKit.P(Math.Round(0.5 * (x0 + x1)), Math.Round(0.5 * (y0 + y1)), 195.0);
            var fac = new ReliefPart("CanyonBridge_facade", ReliefMat.Facade, origin, Beat);
            var conc = new ReliefPart("CanyonBridge_concrete", ReliefMat.Concrete, origin, Beat);
            var ring = new ReliefPart("CanyonBridge_portalRing", ReliefMat.PortalRing, origin, Beat);
            parts.Add(fac); parts.Add(conc); parts.Add(ring);
            var fw = new FacadeWriter(fac.Mb, origin);
            byte F = FacadeWriter.ModeFacade;
            float uMin = (float)z0, uMax = (float)z1;

            // west face (toward the boulevard), u = z
            ReliefKit.HoledFaceX(fw, x0, -1f, WestZ0, WestZ1, y0, y1, null, z => z, IdBody, F, (float)WestZ0, (float)WestZ1);
            // east face (toward the Gulf) around the tube panel, u = 600 - z (continuous along the bridge seen from the east)
            double yc = SkybridgeDims.UpperY, zc = SkybridgeDims.UpperZ, h = GateDims.CbPanelHalf;
            var panel = new List<ReliefHole> { new ReliefHole(zc - h, zc + h, yc - h, yc + h) };
            Func<double, double> uE = z => 600.0 - z;
            ReliefKit.HoledFaceX(fw, x1, 1f, z0, z1, y0, y1, panel, uE, IdBody + 1u, F, (float)(600.0 - z1), (float)(600.0 - z0));
            ReliefKit.PanelWithHole(fw, x1, yc, zc, h, Vector3.right, (float)uE(zc), -1f, IdBody + 1u, F, (float)(600.0 - z1), (float)(600.0 - z0));
            // end faces in front of face A (x 215.3..x1), u = x
            double xa = EdgeDims.FaceXA;
            fw.Flat(ReliefKit.P(xa, y0, z0), ReliefKit.P(xa, y1, z0), ReliefKit.P(x1, y1, z0), ReliefKit.P(x1, y0, z0), Vector3.back,
                (float)xa, (float)xa, (float)x1, (float)x1, IdBody + 2u, F, (float)xa, (float)x1);
            fw.Flat(ReliefKit.P(xa, y0, z1), ReliefKit.P(xa, y1, z1), ReliefKit.P(x1, y1, z1), ReliefKit.P(x1, y0, z1), Vector3.forward,
                (float)xa, (float)xa, (float)x1, (float)x1, IdBody + 3u, F, (float)xa, (float)x1);
            // roof (roof mode) and soffit (concrete): [x0, xa] x [116, 274] and [xa, x1] x {[z0, 116], [116, 274], [274, z1]} (conformal)
            var o = origin; var cb = conc.Mb;
            var zr = new[] { z0, WestZ0, WestZ1, z1 };
            fw.Flat(ReliefKit.P(x0, y1, WestZ0), ReliefKit.P(xa, y1, WestZ0), ReliefKit.P(xa, y1, WestZ1), ReliefKit.P(x0, y1, WestZ1), Vector3.up,
                (float)x0, (float)xa, (float)xa, (float)x0, IdBody + 4u, FacadeWriter.ModeRoof, (float)x0, (float)x1);
            EdgeMesh.FlatQuad(cb, o, ReliefKit.P(x0, y0, WestZ0), ReliefKit.P(xa, y0, WestZ0), ReliefKit.P(xa, y0, WestZ1), ReliefKit.P(x0, y0, WestZ1), Vector3.down);
            for (int i = 0; i < 3; i++)
            {
                fw.Flat(ReliefKit.P(xa, y1, zr[i]), ReliefKit.P(x1, y1, zr[i]), ReliefKit.P(x1, y1, zr[i + 1]), ReliefKit.P(xa, y1, zr[i + 1]), Vector3.up,
                    (float)xa, (float)x1, (float)x1, (float)xa, IdBody + 4u, FacadeWriter.ModeRoof, (float)x0, (float)x1);
                EdgeMesh.FlatQuad(cb, o, ReliefKit.P(xa, y0, zr[i]), ReliefKit.P(x1, y0, zr[i]), ReliefKit.P(x1, y0, zr[i + 1]), ReliefKit.P(xa, y0, zr[i + 1]), Vector3.down);
            }

            // fascia bands along the long faces (into the shafts at their ends; the deepest edge of the soffit, 2.4 m from y 106.4),
            // top fascias, parapets; the end fascias / parapets sit 3 cm lower than the long ones (no shared plane at the corners)
            double p = GateDims.CbFasciaProud;
            double ylo = y0 - 1.6, yhi = ylo + GateDims.CbFasciaLow;
            double ytl = y1 - GateDims.CbFasciaTop + 0.04, yth = y1 + 0.04;          // 4 cm above the roof
            double pt = GateDims.CbParapetT, ph = GateDims.CbParapet;
            ReliefKit.Band(cb, o, x0 - p, x0 + 0.3, ylo, yhi, WestZ0 - 1.0, WestZ1 + 1.0);
            ReliefKit.Band(cb, o, x1 - 0.3, x1 + p, ylo, yhi, z0, z1);
            ReliefKit.Band(cb, o, x0 - p, x0 + 0.3, ytl, yth, WestZ0 - 1.0, WestZ1 + 1.0);
            ReliefKit.Band(cb, o, x1 - 0.3, x1 + p, ytl, yth, z0, z1);
            ReliefKit.Band(cb, o, x0 - p + 0.05, x0 - p + 0.05 + pt, yth - 0.02, yth + ph, WestZ0 - 1.0, WestZ1 + 1.0);
            ReliefKit.Band(cb, o, x1 + p - 0.05 - pt, x1 + p - 0.05, yth - 0.02, yth + ph, z0 - 0.40, z1 + 0.40);
            foreach (double ze in new[] { z0, z1 })
            {
                double s = ze < 195.0 ? -1.0 : 1.0, za = ze - s * 0.3, zb = ze + s * p;
                ReliefKit.Band(cb, o, xa - 0.4, x1 + 0.2, ylo - 0.03, yhi - 0.03, Math.Min(za, zb), Math.Max(za, zb));
                ReliefKit.Band(cb, o, xa - 0.4, x1 + 0.2, ytl - 0.03, yth - 0.03, Math.Min(za, zb), Math.Max(za, zb));
                double zp0 = ze + s * (p - 0.05 - pt), zp1 = ze + s * (p - 0.05);
                ReliefKit.Band(cb, o, xa - 0.4, x1 + p - 0.05 - pt + 0.02, yth - 0.05, yth + ph - 0.03, Math.Min(zp0, zp1), Math.Max(zp0, zp1));
            }
            // soffit ribs across the bridge every 8 m, 0.1 m into the fascias, hanging 1.4 m (above the fascia bottom 106.4)
            for (double z = Math.Ceiling(z0 / GateDims.CbRibPitch) * GateDims.CbRibPitch; z <= z1 - 2.0; z += GateDims.CbRibPitch)
            {
                double xr0 = z < WestZ0 || z > WestZ1 ? xa - 0.2 : x0 + 0.2;
                ReliefKit.Band(cb, o, xr0, x1 - 0.2, y0 - GateDims.CbRibD, y0 + 0.2, z - 0.5 * GateDims.CbRibW, z + 0.5 * GateDims.CbRibW);
            }

            // the upper tube's west portal (collar out of the east face, tunnel 16 m into the body)
            ReliefKit.TubePortal(cb, ring.Mb, o, x1, 1.0, yc, zc, EdgeDims.PortalLip, GateDims.CbTunnelDepth);
        }
    }
}
