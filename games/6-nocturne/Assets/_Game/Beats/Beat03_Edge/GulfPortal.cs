// GulfPortal: the lower tube portals (y -142, z 209, tube r 5; MASTER_PROMPT 14.5.4, 14.12, agreement 4) on face A (x 215.3) and
// face B (x 815.3). The face's own portal panel (GulfFace) has a round hole of r 6.3; this builder closes it with a concrete
// collar (outer cylinder r 6.3 from the face plane 1.0 m out toward the rift, front annulus r 5.6..6.3), a NeonCyan light ring
// (annulus r 5.3..5.6 on the lip, the portal reads at 140-600 m), and the tunnel (inward-facing cylinder r 5.3, 30 m into the
// mass, a dark end disc), so a train clipped to its tube (14.5.4) emerges from inside the face. r 5.3 keeps 0.3 m between G3's
// r 5 tube and the tunnel wall (no co-planar cylinders). Ring vertices use the same 32 angles as the panel's hole (watertight).
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Megastructure
{
    public static class GulfPortal
    {
        public const double RingInner = 5.3, RingOuter = 5.6;

        /// <summary>Appends the portal of face `face` (0 = A, 1 = B): concrete parts and the light ring.</summary>
        public static void Build(int face, List<EdgePart> parts)
        {
            double xf = EdgeDims.FaceX(face, EdgeDims.LowerTubeZ);
            double yc = EdgeDims.LowerTubeY, zc = EdgeDims.LowerTubeZ;
            float out1 = (float)(-EdgeDims.RecedeSign(face));     // +1 for A (the rift is at +x), -1 for B
            var origin = new Vector3((float)xf, (float)yc, (float)zc);
            string nm = face == 0 ? "PortalA" : "PortalB";
            var conc = new EdgePart(nm + "_collar", EdgeMat.Concrete, origin);
            var ring = new EdgePart(nm + "_ring", EdgeMat.PortalRing, origin);
            parts.Add(conc); parts.Add(ring);
            int N = EdgeDims.PortalSegments;
            float xLip = (float)(xf + out1 * EdgeDims.PortalLip), xEnd = (float)(xf - out1 * EdgeDims.PortalDepth);
            var axisOut = new Vector3(out1, 0f, 0f);
            for (int j = 0; j < N; j++)
            {
                double a0 = 2.0 * Math.PI * j / N, a1 = 2.0 * Math.PI * (j + 1) / N;
                Vector3 r0 = Radial(a0), r1 = Radial(a1);
                // outer cylinder r 6.3: face plane -> lip (normals outward from the axis)
                Cyl(conc.Mb, origin, (float)xf, xLip, (float)EdgeDims.PortalOuter, r0, r1, yc, zc, true);
                // inner tunnel r 5.3: lip -> end (normals toward the axis)
                Cyl(conc.Mb, origin, xLip, xEnd, (float)RingInner, r0, r1, yc, zc, false);
                // lip: concrete annulus 5.6..6.3 and the light ring 5.3..5.6 (facing the rift)
                Ann(conc.Mb, origin, xLip, (float)RingOuter, (float)EdgeDims.PortalOuter, r0, r1, yc, zc, axisOut);
                Ann(ring.Mb, origin, xLip, (float)RingInner, (float)RingOuter, r0, r1, yc, zc, axisOut);
                // the dark end disc of the tunnel (facing the rift)
                EdgeMesh.Tri(conc.Mb, origin, P(xEnd, yc, zc), P(xEnd, r0, (float)RingInner, yc, zc), P(xEnd, r1, (float)RingInner, yc, zc), axisOut);
            }
        }

        static Vector3 Radial(double a) => new Vector3(0f, (float)Math.Sin(a), (float)Math.Cos(a));
        static Vector3 P(double x, double y, double z) => new Vector3((float)x, (float)y, (float)z);
        static Vector3 P(float x, Vector3 radial, float r, double yc, double zc) => new Vector3(x, (float)yc + radial.y * r, (float)zc + radial.z * r);

        static void Cyl(MeshBuilder mb, Vector3 o, float xa, float xb, float r, Vector3 r0, Vector3 r1, double yc, double zc, bool outward)
        {
            Vector3 n0 = outward ? r0 : -r0, n1 = outward ? r1 : -r1;
            EdgeMesh.Quad(mb, o, P(xa, r0, r, yc, zc), P(xb, r0, r, yc, zc), P(xb, r1, r, yc, zc), P(xa, r1, r, yc, zc), n0, n0, n1, n1, n0 + n1);
        }

        static void Ann(MeshBuilder mb, Vector3 o, float x, float ri, float ro, Vector3 r0, Vector3 r1, double yc, double zc, Vector3 n)
            => EdgeMesh.FlatQuad(mb, o, P(x, r0, ri, yc, zc), P(x, r0, ro, yc, zc), P(x, r1, ro, yc, zc), P(x, r1, ri, yc, zc), n);
    }
}
