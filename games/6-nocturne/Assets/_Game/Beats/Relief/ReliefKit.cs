// ReliefKit: the mesh vocabulary of GE's builders (Beats/Relief): named parts with chunk-local origins (ReliefPart, like Beat03's
// EdgePart, so EditMode tests build and count them without a scene), material slots, and the few shapes the gate structures need:
//   * HoledFaceX: an axis-aligned NocFacade face on a plane x = const with rectangular openings, cut on the conformal grid of every
//     opening's edges (no T-junction anywhere inside the face, so no crack sparkles at distance);
//   * TunnelX: the four inward walls of a rectangular tunnel along x (a lit mouth of PortalLit metres, then plain concrete), with an
//     optional dark back wall;
//   * Band: a chamfered concrete member (MeshKit.ChamferBox: 2 cm bevels at the scale of a building edge are invisible, so bands use
//     a 6 cm bevel) for fascias, head beams, ribs, ledges and parapets;
//   * TubePortal: the collar / collar lamps / tunnel of a face-to-face tube portal on a plane x = const, the same construction as
//     GulfPortal (r 6.3 collar 1 m proud, NeonCyan lamps on r 5.3..5.6, tunnel r 5.3, dark end disc), and the square panel with the
//     matching round hole (GulfFace.PortalPanel's corner fan). AB fix wave (C03): the lip carries GulfPortal.LampCount separate
//     lamps (GulfPortal.IsLamp), the rest of it is concrete: no continuous neon circle (the p4_01 "HUD reticle").
// Every face is wound toward its wanted normal (EdgeMesh.Wind); no two faces share a plane (offsets are recorded where they meet).
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Megastructure
{
    /// <summary>Material slots of the relief builders (resolved to library materials or runtime variants in ReliefBuilder).</summary>
    /// AB: GalleryWarm / GalleryCool / AtriumHi / AtriumLo / LipLight are the emissive light content of the Gulf faces (GulfRibBuilder).
    public enum ReliefMat { Facade, Concrete, Steel, TunnelLight, PortalRing, ShopWarm, ShopCold, GalleryWarm, GalleryCool, AtriumHi, AtriumLo, LipLight }

    public sealed class ReliefPart
    {
        public string Name;
        public MeshBuilder Mb;
        public ReliefMat Mat;
        public Vector3 Origin;
        /// <summary>Beat root index (0..6) the part is parented to (beat roots are always active, 14.6).</summary>
        public int Beat;
        public ReliefPart(string name, ReliefMat mat, Vector3 origin, int beat) { Name = name; Mat = mat; Origin = origin; Beat = beat; Mb = new MeshBuilder(); }
    }

    /// <summary>A rectangular opening on a face: a = the face's horizontal axis (z on an x-plane), b = y.</summary>
    public struct ReliefHole
    {
        public double a0, a1, b0, b1;
        public ReliefHole(double a0, double a1, double b0, double b1) { this.a0 = a0; this.a1 = a1; this.b0 = b0; this.b1 = b1; }
    }

    public static class ReliefKit
    {
        public const float BandBevel = 0.06f;

        public static Vector3 P(double x, double y, double z) => new Vector3((float)x, (float)y, (float)z);

        /// <summary>
        /// A NocFacade face on the plane x = const facing `nx` (+1 / -1), spanning z [z0, z1] x y [y0, y1], minus the openings, as the
        /// conformal grid of every opening edge (cells inside an opening are skipped). u = uOfZ(z) (metres along the face).
        /// </summary>
        public static void HoledFaceX(FacadeWriter fw, double x, float nx, double z0, double z1, double y0, double y1, IList<ReliefHole> holes,
            Func<double, double> uOfZ, uint faceId, byte mode, float uMin, float uMax)
        {
            var zs = new List<double> { z0, z1 };
            var ys = new List<double> { y0, y1 };
            if (holes != null)
                foreach (var h in holes)
                {
                    if (h.a1 <= z0 || h.a0 >= z1 || h.b1 <= y0 || h.b0 >= y1) continue;
                    zs.Add(Math.Max(z0, h.a0)); zs.Add(Math.Min(z1, h.a1));
                    ys.Add(Math.Max(y0, h.b0)); ys.Add(Math.Min(y1, h.b1));
                }
            zs = Unique(zs); ys = Unique(ys);
            var n = new Vector3(nx, 0f, 0f);
            for (int i = 0; i + 1 < zs.Count; i++)
                for (int j = 0; j + 1 < ys.Count; j++)
                {
                    double zc = 0.5 * (zs[i] + zs[i + 1]), yc = 0.5 * (ys[j] + ys[j + 1]);
                    if (InHole(holes, zc, yc)) continue;
                    float ua = (float)uOfZ(zs[i]), ub = (float)uOfZ(zs[i + 1]);
                    fw.Flat(P(x, ys[j], zs[i]), P(x, ys[j + 1], zs[i]), P(x, ys[j + 1], zs[i + 1]), P(x, ys[j], zs[i + 1]), n,
                        ua, ua, ub, ub, faceId, mode, uMin, uMax);
                }
        }

        static bool InHole(IList<ReliefHole> holes, double a, double b)
        {
            if (holes == null) return false;
            foreach (var h in holes) if (a > h.a0 && a < h.a1 && b > h.b0 && b < h.b1) return true;
            return false;
        }

        /// <summary>Sorted values with duplicates (within 1 mm) removed.</summary>
        public static List<double> Unique(List<double> v)
        {
            v.Sort();
            var o = new List<double>(v.Count);
            foreach (double d in v) if (o.Count == 0 || d - o[o.Count - 1] > 1e-3) o.Add(d);
            return o;
        }

        /// <summary>
        /// The inward walls of a rectangular tunnel from the face plane x = xFace, `depth` metres along `dir` (+1 / -1): the first
        /// `lit` metres into `mouth` (the lit portal), the rest into `wall`; a back wall facing the opening into `wall` when `cap`.
        /// </summary>
        public static void TunnelX(MeshBuilder mouth, MeshBuilder wall, Vector3 origin, double xFace, double dir, double depth, double lit, ReliefHole h, bool cap)
        {
            double xm = xFace + dir * Math.Min(lit, depth), xe = xFace + dir * depth;
            TunnelSegment(mouth, origin, xFace, xm, h);
            if (depth > lit + 1e-6) TunnelSegment(wall, origin, xm, xe, h);
            if (cap) EdgeMesh.FlatQuad(wall, origin, P(xe, h.b0, h.a0), P(xe, h.b1, h.a0), P(xe, h.b1, h.a1), P(xe, h.b0, h.a1), new Vector3((float)(-dir), 0f, 0f));
        }

        static void TunnelSegment(MeshBuilder mb, Vector3 o, double xa, double xb, ReliefHole h)
        {
            // floor (faces up), ceiling (down), south wall (faces +z), north wall (faces -z)
            EdgeMesh.FlatQuad(mb, o, P(xa, h.b0, h.a0), P(xb, h.b0, h.a0), P(xb, h.b0, h.a1), P(xa, h.b0, h.a1), Vector3.up);
            EdgeMesh.FlatQuad(mb, o, P(xa, h.b1, h.a0), P(xb, h.b1, h.a0), P(xb, h.b1, h.a1), P(xa, h.b1, h.a1), Vector3.down);
            EdgeMesh.FlatQuad(mb, o, P(xa, h.b0, h.a0), P(xb, h.b0, h.a0), P(xb, h.b1, h.a0), P(xa, h.b1, h.a0), Vector3.forward);
            EdgeMesh.FlatQuad(mb, o, P(xa, h.b0, h.a1), P(xb, h.b0, h.a1), P(xb, h.b1, h.a1), P(xa, h.b1, h.a1), Vector3.back);
        }

        /// <summary>A frame (four chamfered bars `bar` wide) around an opening on the plane x = xFace: from 0.1 m inside the face to
        /// `proud` metres out along `dir` (the outward normal); the bars' inner faces lie 2 cm outside the opening (no plane shared
        /// with the tunnel walls).</summary>
        public static void FrameX(MeshBuilder mb, Vector3 origin, double xFace, double dir, ReliefHole h, double bar, double proud)
        {
            double xa = xFace - dir * 0.1, xb = xFace + dir * proud;
            double x0 = Math.Min(xa, xb), x1 = Math.Max(xa, xb), g = 0.02;
            double a0 = h.a0 - g, a1 = h.a1 + g, b0 = h.b0 - g, b1 = h.b1 + g;
            Band(mb, origin, x0, x1, b0 - bar, b0, a0 - bar, a1 + bar);
            Band(mb, origin, x0, x1, b1, b1 + bar, a0 - bar, a1 + bar);
            Band(mb, origin, x0, x1, b0 + 0.01, b1 - 0.01, a0 - bar, a0);
            Band(mb, origin, x0, x1, b0 + 0.01, b1 - 0.01, a1, a1 + bar);
        }

        /// <summary>A chamfered axis-aligned box x [x0, x1] y [y0, y1] z [z0, z1] (positions relative to `origin`).</summary>
        public static void Band(MeshBuilder mb, Vector3 origin, double x0, double x1, double y0, double y1, double z0, double z1)
        {
            Vector3 c = P(0.5 * (x0 + x1), 0.5 * (y0 + y1), 0.5 * (z0 + z1)) - origin;
            MeshKit.ChamferBox(mb, c, new Vector3((float)Math.Abs(x1 - x0), (float)Math.Abs(y1 - y0), (float)Math.Abs(z1 - z0)), BandBevel);
        }

        // ------------------------------------------------------------------ tube portals (face-to-face tubes, r 5)
        /// <summary>
        /// Collar (outer cylinder r 6.3 from the face plane `lip` m toward `outDir`, its front annulus 5.6..6.3), the collar lamps
        /// (the GulfPortal.IsLamp segments of the annulus 5.3..5.6, into `ring`; the other segments into `conc`), the tunnel (inward
        /// cylinder r 5.3 from the lip `depth` m into the mass) and its dark end disc; 32 segments (the panel's hole uses the same
        /// angles, so the collar closes it watertight).
        /// </summary>
        public static void TubePortal(MeshBuilder conc, MeshBuilder ring, Vector3 origin, double xFace, double outDir, double yc, double zc, double lip, double depth)
        {
            int N = EdgeDims.PortalSegments;
            float xLip = (float)(xFace + outDir * lip), xEnd = (float)(xFace - outDir * depth);
            var axisOut = new Vector3((float)outDir, 0f, 0f);
            for (int j = 0; j < N; j++)
            {
                double a0 = 2.0 * Math.PI * j / N, a1 = 2.0 * Math.PI * (j + 1) / N;
                Vector3 r0 = new Vector3(0f, (float)Math.Sin(a0), (float)Math.Cos(a0)), r1 = new Vector3(0f, (float)Math.Sin(a1), (float)Math.Cos(a1));
                Cyl(conc, origin, (float)xFace, xLip, (float)EdgeDims.PortalOuter, r0, r1, yc, zc, true);
                Cyl(conc, origin, xLip, xEnd, (float)GulfPortal.RingInner, r0, r1, yc, zc, false);
                Ann(conc, origin, xLip, (float)GulfPortal.RingOuter, (float)EdgeDims.PortalOuter, r0, r1, yc, zc, axisOut);
                Ann(GulfPortal.IsLamp(j) ? ring : conc, origin, xLip, (float)GulfPortal.RingInner, (float)GulfPortal.RingOuter, r0, r1, yc, zc, axisOut);
                EdgeMesh.Tri(conc, origin, P(xEnd, yc, zc), RP(xEnd, r0, (float)GulfPortal.RingInner, yc, zc), RP(xEnd, r1, (float)GulfPortal.RingInner, yc, zc), axisOut);
            }
        }

        static Vector3 RP(float x, Vector3 radial, float r, double yc, double zc) => new Vector3(x, (float)yc + radial.y * r, (float)zc + radial.z * r);

        static void Cyl(MeshBuilder mb, Vector3 o, float xa, float xb, float r, Vector3 r0, Vector3 r1, double yc, double zc, bool outward)
        {
            Vector3 n0 = outward ? r0 : -r0, n1 = outward ? r1 : -r1;
            EdgeMesh.Quad(mb, o, RP(xa, r0, r, yc, zc), RP(xb, r0, r, yc, zc), RP(xb, r1, r, yc, zc), RP(xa, r1, r, yc, zc), n0, n0, n1, n1, n0 + n1);
        }

        static void Ann(MeshBuilder mb, Vector3 o, float x, float ri, float ro, Vector3 r0, Vector3 r1, double yc, double zc, Vector3 n)
            => EdgeMesh.FlatQuad(mb, o, RP(x, r0, ri, yc, zc), RP(x, r0, ro, yc, zc), RP(x, r1, ro, yc, zc), RP(x, r1, ri, yc, zc), n);

        /// <summary>
        /// A square NocFacade panel (half size h) on the plane x = const around a round hole of the collar radius (r 6.3, 32 angles),
        /// fanned from its four corners (GulfFace.PortalPanel's construction): only the corners lie on the square's edges, so the
        /// neighbouring face cells meet it without T-junctions. u = uc + uSign (z - zc): uSign is the face's du/dz (FA's sigma
        /// contract: one u direction over a whole face, FacadeSkinTests; REQUEST GE in docs/logs/FA.md).
        /// </summary>
        public static void PanelWithHole(FacadeWriter fw, double x, double yc, double zc, double h, Vector3 nrm, float uc, float uSign, uint faceId, byte mode, float uMin, float uMax)
        {
            int N = EdgeDims.PortalSegments;
            var mb = fw.Mb;
            mb.Color = new Color32(255, 0, mode, 0);
            mb.CurrentUV1 = new Vector4(NocFacadeMath.SafeExtent(uMin), NocFacadeMath.SafeExtent(uMax), 0f, 0f);     // FA (TQ-09)
            float id = faceId & 0xFFFFu;
            int[] ring = new int[N];
            for (int j = 0; j < N; j++)
            {
                double a = 2.0 * Math.PI * j / N;
                Vector3 p = P(x, yc + EdgeDims.PortalOuter * Math.Sin(a), zc + EdgeDims.PortalOuter * Math.Cos(a));
                ring[j] = mb.Vertex(p - fw.Origin, nrm, new Vector2((float)(uc + uSign * EdgeDims.PortalOuter * Math.Cos(a)), id));
            }
            double[,] cz = { { 1, -1 }, { 1, 1 }, { -1, 1 }, { -1, -1 } };
            int[] corner = new int[4];
            for (int c = 0; c < 4; c++)
                corner[c] = mb.Vertex(P(x, yc + h * cz[c, 1], zc + h * cz[c, 0]) - fw.Origin, nrm, new Vector2((float)(uc + uSign * h * cz[c, 0]), id));
            int q = N / 4, half = N / 8;
            for (int e = 0; e < 4; e++)
            {
                int ca = corner[e], cb = corner[(e + 1) % 4];
                int jStart = (7 * half + e * q) % N;
                for (int t = 0; t < half; t++) TriW(mb, ca, ring[(jStart + t) % N], ring[(jStart + t + 1) % N], nrm);
                int jMid = (jStart + half) % N;
                TriW(mb, ca, ring[jMid], cb, nrm);
                for (int t = 0; t < half; t++) TriW(mb, cb, ring[(jMid + t) % N], ring[(jMid + t + 1) % N], nrm);
            }
        }

        /// <summary>The same square panel with the collar hole for a plain (non-facade) material: uv0 = planar (x + z, y) like EdgeMesh.Quad.</summary>
        public static void PanelWithHolePlain(MeshBuilder mb, Vector3 origin, double x, double yc, double zc, double h, Vector3 nrm)
        {
            int N = EdgeDims.PortalSegments;
            int[] ring = new int[N];
            for (int j = 0; j < N; j++)
            {
                double a = 2.0 * Math.PI * j / N;
                Vector3 p = P(x, yc + EdgeDims.PortalOuter * Math.Sin(a), zc + EdgeDims.PortalOuter * Math.Cos(a));
                ring[j] = mb.Vertex(p - origin, nrm, new Vector2(p.x + p.z, p.y));
            }
            double[,] cz = { { 1, -1 }, { 1, 1 }, { -1, 1 }, { -1, -1 } };
            int[] corner = new int[4];
            for (int c = 0; c < 4; c++)
            {
                Vector3 p = P(x, yc + h * cz[c, 1], zc + h * cz[c, 0]);
                corner[c] = mb.Vertex(p - origin, nrm, new Vector2(p.x + p.z, p.y));
            }
            int q = N / 4, half = N / 8;
            for (int e = 0; e < 4; e++)
            {
                int ca = corner[e], cb = corner[(e + 1) % 4];
                int jStart = (7 * half + e * q) % N;
                for (int t = 0; t < half; t++) TriW(mb, ca, ring[(jStart + t) % N], ring[(jStart + t + 1) % N], nrm);
                int jMid = (jStart + half) % N;
                TriW(mb, ca, ring[jMid], cb, nrm);
                for (int t = 0; t < half; t++) TriW(mb, cb, ring[(jMid + t) % N], ring[(jMid + t + 1) % N], nrm);
            }
        }

        static void TriW(MeshBuilder mb, int a, int b, int c, Vector3 want)
        {
            Vector3 pa = mb.Positions[a], pb = mb.Positions[b], pc = mb.Positions[c];
            if (Vector3.Dot(Vector3.Cross(pb - pa, pc - pa), want) >= 0f) mb.Tri(a, b, c);
            else mb.Tri(a, c, b);
        }

        // ------------------------------------------------------------------ lane portals
        /// <summary>The openings of every boulevard lane on a face (lane (y, z) +- the portal half sizes).</summary>
        public static List<ReliefHole> LaneHoles()
        {
            var l = new List<ReliefHole>(12);
            foreach (var ln in GateDims.Lanes())
                l.Add(new ReliefHole(ln.z - GateDims.PortalHalfW, ln.z + GateDims.PortalHalfW, ln.y - GateDims.PortalHalfH, ln.y + GateDims.PortalHalfH));
            return l;
        }
    }
}
