// NotchGate: face B closed again above the hanging garden (GE, D-029 GB-02 / GB-03 / GB-06; GateDims part 2). The garden notch of
// agreement 5 becomes a 160 m wide, 129 m tall gateway into face B:
//   * the lintel x 815.9..839.3, y 129..560, z 115..275 (its ends are the notch walls, so it has no end faces): the west face in the
//     NocFacade section mode with face B's own id and u (u = z - 195 where face B is planar), 0.6 m behind face B's plane (no edge
//     shared with face B's columns; the notch walls close the 0.6 m reveals), the east face toward the courtyard in facade mode, a
//     concrete soffit with transfer ribs every 16 m whose ends show 0.3 m on the courtyard side, the top cap at the face top;
//   * a concrete head beam along the opening (x 813.8..816.2, 1.5 m proud of face B, y 124.8..129.4, 1 m into the face's mass past
//     each notch wall);
//   * the keystone block of the upper tube's east portal hanging under the head (x 813.5..818.0, z 186..204, y 106.0..129.6): its
//     west face is a square panel around the collar hole, the collar 1 m proud, the NeonCyan collar lamps, a 4 m tunnel (the tube
//     ends at x 815.3 inside it). AB fix wave (C03): the west face is face B's skin (NocFacade section mode, face B's id and u, as
//     the lintel above it), so the portal sits in the face's window field instead of on a dark concrete square that framed the
//     ring like a reticle; the sides and the bottom stay concrete;
//   * one tunnel per boulevard lane through the lintel (7 x 5 m, lit 4 m at both mouths) with a 0.35 m concrete frame on both faces;
//     the lanes then cross the open courtyard above the crowns and end in the east block (NotchEastBlock).
// Crown clearances (FO's 1.10 envelopes): ribs 128.0 / soffit 129 over 124.6 (T4 at x 839.3), head beam 124.8 over 110.4 (T4 at x
// 816.2), keystone 106.0 over 102.5 (T4 at x 818, z 204): asserted >= 3 m in ReliefTests.
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Megastructure
{
    public static class NotchGate
    {
        public const uint IdEast = 0x6200u;
        /// <summary>Beat root: beat 5, the hanging garden.</summary>
        public const int Beat = 4;

        public static void Build(List<ReliefPart> parts)
        {
            double x0 = GateDims.NgX0, x1 = GateDims.NgX1, y0 = GateDims.NgY0, top = EdgeDims.FaceTop;
            double z0 = EdgeDims.NotchZ0, z1 = EdgeDims.NotchZ1;
            var origin = ReliefKit.P(Math.Round(0.5 * (x0 + x1)), 300.0, 195.0);
            var fac = new ReliefPart("NotchGate_facade", ReliefMat.Facade, origin, Beat);
            var conc = new ReliefPart("NotchGate_concrete", ReliefMat.Concrete, origin, Beat);
            var lit = new ReliefPart("NotchGate_tunnelLight", ReliefMat.TunnelLight, origin, Beat);
            var ring = new ReliefPart("NotchGate_portalRing", ReliefMat.PortalRing, origin, Beat);
            parts.Add(fac); parts.Add(conc); parts.Add(lit); parts.Add(ring);
            var fw = new FacadeWriter(fac.Mb, origin);
            var o = origin; var cb = conc.Mb;
            var holes = ReliefKit.LaneHoles();

            // west face: face B's id and u (section mode), behind face B's plane by 0.6 m
            float uMinB = (float)EdgeDims.ArcLength(1, EdgeDims.FaceZ0), uMaxB = (float)EdgeDims.ArcLength(1, EdgeDims.FaceZ1);
            ReliefKit.HoledFaceX(fw, x0, -1f, z0, z1, y0, top, holes, z => EdgeDims.ArcLength(1, z), GulfFace.IdB, FacadeWriter.ModeSection, uMinB, uMaxB);
            // east face toward the courtyard (facade mode), u = 275 - z (left to right seen from the garden)
            ReliefKit.HoledFaceX(fw, x1, 1f, z0, z1, y0, top, holes, z => z1 - z, IdEast, FacadeWriter.ModeFacade, 0f, (float)(z1 - z0));
            // top cap (roof mode) and soffit (concrete)
            fw.Flat(ReliefKit.P(x0, top, z0), ReliefKit.P(x1, top, z0), ReliefKit.P(x1, top, z1), ReliefKit.P(x0, top, z1), Vector3.up,
                (float)x0, (float)x1, (float)x1, (float)x0, IdEast + 1u, FacadeWriter.ModeRoof, (float)x0, (float)x1);
            EdgeMesh.FlatQuad(cb, o, ReliefKit.P(x0, y0, z0), ReliefKit.P(x1, y0, z0), ReliefKit.P(x1, y0, z1), ReliefKit.P(x0, y0, z1), Vector3.down);

            // head beam and transfer ribs (the ribs run from inside the head beam to 0.3 m past the east face)
            ReliefKit.Band(cb, o, GateDims.HeadX0, GateDims.HeadX1, GateDims.HeadY0, GateDims.HeadY1, GateDims.HeadZ0, GateDims.HeadZ1);
            for (double z = z0 + GateDims.RibPitch; z < z1 - 1.0; z += GateDims.RibPitch)
                ReliefKit.Band(cb, o, GateDims.HeadX1 - 0.2, x1 + 0.3, GateDims.RibY0, y0 + 0.2, z - 0.5 * GateDims.RibW, z + 0.5 * GateDims.RibW);

            // lane tunnels through the lintel, lit at both mouths, framed on both faces
            double depth = x1 - x0, litL = GateDims.PortalLit;
            foreach (var h in holes)
            {
                ReliefKit.TunnelX(lit.Mb, cb, o, x0, 1.0, depth - litL, litL, h, false);
                ReliefKit.TunnelX(lit.Mb, cb, o, x1, -1.0, litL, litL, h, false);
                ReliefKit.FrameX(cb, o, x0, -1.0, h, GateDims.PortalFrame, 0.4);
                ReliefKit.FrameX(cb, o, x1, 1.0, h, GateDims.PortalFrame, 0.4);
            }

            Keystone(cb, ring.Mb, o, fw);
        }

        /// <summary>The keystone block under the head with the upper tube's east portal in its west face.</summary>
        static void Keystone(MeshBuilder cb, MeshBuilder ring, Vector3 o, FacadeWriter fw)
        {
            double xa = GateDims.KeyX0, xb = GateDims.KeyX1, ya = GateDims.KeyY0, yb = GateDims.KeyY1, za = GateDims.KeyZ0, zb = GateDims.KeyZ1;
            double yc = SkybridgeDims.UpperY, zc = SkybridgeDims.UpperZ, h = 0.5 * (zb - za);   // square panel 18 m: z 186..204, y 109..127
            // west face (fix wave C03): face B's skin, u = ArcLength(1, z) like the lintel's west face (one sigma per face id)
            float uMinB = (float)EdgeDims.ArcLength(1, EdgeDims.FaceZ0), uMaxB = (float)EdgeDims.ArcLength(1, EdgeDims.FaceZ1);
            float ua = (float)EdgeDims.ArcLength(1, za), ub = (float)EdgeDims.ArcLength(1, zb), uc = (float)EdgeDims.ArcLength(1, zc);
            float uSign = ub >= ua ? 1f : -1f;
            ReliefKit.PanelWithHole(fw, xa, yc, zc, h, Vector3.left, uc, uSign, GulfFace.IdB, FacadeWriter.ModeSection, uMinB, uMaxB);
            var nl = Vector3.left;
            fw.Flat(ReliefKit.P(xa, ya, za), ReliefKit.P(xa, yc - h, za), ReliefKit.P(xa, yc - h, zb), ReliefKit.P(xa, ya, zb), nl, ua, ua, ub, ub, GulfFace.IdB, FacadeWriter.ModeSection, uMinB, uMaxB);
            fw.Flat(ReliefKit.P(xa, yc + h, za), ReliefKit.P(xa, yb, za), ReliefKit.P(xa, yb, zb), ReliefKit.P(xa, yc + h, zb), nl, ua, ua, ub, ub, GulfFace.IdB, FacadeWriter.ModeSection, uMinB, uMaxB);
            // sides, bottom, back (below the soffit), top in front of the lintel
            EdgeMesh.FlatQuad(cb, o, ReliefKit.P(xa, ya, za), ReliefKit.P(xb, ya, za), ReliefKit.P(xb, yb, za), ReliefKit.P(xa, yb, za), Vector3.back);
            EdgeMesh.FlatQuad(cb, o, ReliefKit.P(xa, ya, zb), ReliefKit.P(xb, ya, zb), ReliefKit.P(xb, yb, zb), ReliefKit.P(xa, yb, zb), Vector3.forward);
            EdgeMesh.FlatQuad(cb, o, ReliefKit.P(xa, ya, za), ReliefKit.P(xb, ya, za), ReliefKit.P(xb, ya, zb), ReliefKit.P(xa, ya, zb), Vector3.down);
            EdgeMesh.FlatQuad(cb, o, ReliefKit.P(xb, ya, za), ReliefKit.P(xb, GateDims.NgY0, za), ReliefKit.P(xb, GateDims.NgY0, zb), ReliefKit.P(xb, ya, zb), Vector3.right);
            EdgeMesh.FlatQuad(cb, o, ReliefKit.P(xa, yb, za), ReliefKit.P(GateDims.NgX0, yb, za), ReliefKit.P(GateDims.NgX0, yb, zb), ReliefKit.P(xa, yb, zb), Vector3.up);
            // the tube portal: collar out of the west face, tunnel 4 m into the block (the r 5 tube ends at x 815.3 inside it)
            ReliefKit.TubePortal(cb, ring, o, xa, -1.0, yc, zc, EdgeDims.PortalLip, xb - xa - 0.5);
        }
    }
}
