using System.Collections.Generic;
using UnityEngine;

namespace Megastructure
{
    /// <summary>
    /// Structural generators imported from Abyss's MeshKit.Structures: the helical ramp slab (optional) and the Warren
    /// truss with gussets. Abyss's hexagonal shaft and honeycomb generators (HexCorner, HexFaceNormal, HexApothemOf,
    /// HexWallPanel, HexAnnulus, MitredRing, HexTube) are not imported (game content).
    /// </summary>
    public static partial class MeshKit
    {
        // ------------------------------------------------------------------------------------------ HelicalAnnulus

        /// <summary>
        /// Helical ramp slab (optional): ring sector between rIn and rOut around the vertical axis through centreXZ, from angle
        /// phi0 to phi1 (radians, from +X toward +Z), top surface height falling/rising linearly from y0 (at phi0) to y1 (at phi1),
        /// slab <paramref name="thickness"/> below the top. Surfaces: top (exact helicoid normal), bottom, inner wall (facing the
        /// axis), outer wall (facing out), flat end caps. The ledge alpha of 14.5.3 (alpha 0 at -Z from C1, pi/2 at +X) maps to
        /// phi = alpha - pi/2. UV0: top and bottom planar (x, z); walls (r * phi, y); caps (r, y).
        /// </summary>
        public static void HelicalAnnulus(MeshBuilder mb, Vector2 centreXZ, float rIn, float rOut, float phi0, float phi1, float y0,
            float y1, int segments, float thickness, bool capEnds = true, bool innerWall = true, bool outerWall = true,
            int radialDivisions = 1)
        {
            int seg = Mathf.Max(1, segments), rd = Mathf.Max(1, radialDivisions);
            float r0 = Mathf.Max(0f, Mathf.Min(rIn, rOut)), r1 = Mathf.Max(rIn, rOut);
            float dphi = phi1 - phi0;
            if (r1 - r0 < 1e-6f || Mathf.Abs(dphi) < 1e-6f) return;
            float th = Mathf.Max(0f, thickness);
            float slope = (y1 - y0) / dphi;
            Vector3 c3 = new Vector3(centreXZ.x, 0f, centreXZ.y);
            int row = rd + 1;
            Reserve(mb, 2 * (seg + 1) * row + 4 * (seg + 1) + 8, 2 * seg * rd * 6 + 4 * seg * 6 + 12);

            for (int surf = 0; surf < 2; surf++)
            {
                bool top = surf == 0;
                if (!top && th <= 1e-6f) break;
                int first = mb.VertexCount;
                for (int k = 0; k <= seg; k++)
                {
                    float f = (float)k / seg;
                    float ph = phi0 + dphi * f;
                    float cp = Mathf.Cos(ph), sp = Mathf.Sin(ph);
                    float yy = Mathf.Lerp(y0, y1, f) - (top ? 0f : th);
                    for (int j = 0; j <= rd; j++)
                    {
                        float r = Mathf.Lerp(r0, r1, (float)j / rd);
                        Vector3 n = SafeNormalize(new Vector3(slope * sp, r, -slope * cp), Vector3.up);
                        if (!top) n = -n;
                        Vector3 p = c3 + new Vector3(r * cp, yy, r * sp);
                        mb.Vertex(p, n, new Vector2(p.x, p.z));
                    }
                }
                for (int k = 0; k < seg; k++)
                for (int j = 0; j < rd; j++)
                {
                    int a = first + k * row + j;
                    Quad(mb, a, a + 1, a + row + 1, a + row);
                }
            }

            if (th > 1e-6f)
            {
                for (int wall = 0; wall < 2; wall++)
                {
                    bool inner = wall == 0;
                    if (inner ? !innerWall : !outerWall) continue;
                    if (inner && r0 <= 1e-6f) continue;
                    float r = inner ? r0 : r1, sg = inner ? -1f : 1f;
                    int first = mb.VertexCount;
                    for (int k = 0; k <= seg; k++)
                    {
                        float f = (float)k / seg;
                        float ph = phi0 + dphi * f;
                        float cp = Mathf.Cos(ph), sp = Mathf.Sin(ph);
                        float yt = Mathf.Lerp(y0, y1, f);
                        Vector3 n = new Vector3(cp * sg, 0f, sp * sg);
                        mb.Vertex(c3 + new Vector3(r * cp, yt, r * sp), n, new Vector2(r * ph, yt));
                        mb.Vertex(c3 + new Vector3(r * cp, yt - th, r * sp), n, new Vector2(r * ph, yt - th));
                    }
                    for (int k = 0; k < seg; k++)
                    {
                        int a = first + 2 * k;
                        Quad(mb, a, a + 1, a + 3, a + 2);
                    }
                }

                if (capEnds)
                {
                    float dirSign = dphi > 0f ? 1f : -1f;
                    for (int end = 0; end < 2; end++)
                    {
                        float ph = end == 0 ? phi0 : phi1;
                        float yt = end == 0 ? y0 : y1;
                        float cp = Mathf.Cos(ph), sp = Mathf.Sin(ph);
                        Vector3 n = new Vector3(-sp, 0f, cp) * (dirSign * (end == 0 ? -1f : 1f));
                        int a = mb.Vertex(c3 + new Vector3(r0 * cp, yt, r0 * sp), n, new Vector2(r0, yt));
                        int b = mb.Vertex(c3 + new Vector3(r1 * cp, yt, r1 * sp), n, new Vector2(r1, yt));
                        int c = mb.Vertex(c3 + new Vector3(r1 * cp, yt - th, r1 * sp), n, new Vector2(r1, yt - th));
                        int d = mb.Vertex(c3 + new Vector3(r0 * cp, yt - th, r0 * sp), n, new Vector2(r0, yt - th));
                        Quad(mb, a, b, c, d);
                    }
                }
            }
        }

        // ------------------------------------------------------------------------------------------ WarrenTruss

        // Section proportions as fractions of memberSize (height = in the truss plane, width = flange width across the plane).
        // Chords carry the full section. The diagonals of even bays (family A) and odd bays (family B) meet at every node, so
        // the two families differ from each other and from the chords: lower (0.9), narrower flanges (0.8 / 0.7) and thinner
        // webs (0.04 / 0.03 against 0.08). No diagonal face is then coplanar with a chord face or with the other family where
        // the members overlap at a node (14.7.5 / CONTRACTS 2: no co-planar faces); the near-coplanar webs that remain inside
        // a node are buried in its gusset plate.
        const float TrussChordWeb = 0.08f, TrussChordFlangeT = 0.1f;
        const float TrussDiagHeight = 0.9f, TrussDiagFlangeT = 0.08f;
        const float TrussDiagWidthA = 0.8f, TrussDiagWidthB = 0.7f;
        const float TrussDiagWebA = 0.04f, TrussDiagWebB = 0.03f;

        /// <summary>
        /// Planar Warren truss hanging below the top chord start -> end (bridge 14.5.2: 6 m deep, 25 bays, 0.4 m I-section members;
        /// gantry 14.5.6: 4 m, 33 bays, 0.3 m). The truss plane contains the chord and <paramref name="up"/>. Members per bay: one
        /// top chord segment, one bottom chord segment and one diagonal zig-zagging (T0-B1, B1-T2, T2-B3, ...), i.e. 3 x bays members
        /// (75 for 25 bays), each authored at final length between node centres and closed; returns that member count.
        /// Chords use the full memberSize section; diagonals are inset (0.9 high, flanges 0.8 / 0.7 wide, thinner webs,
        /// alternating per bay) so overlapping members never share a face plane.
        /// <paramref name="gussets"/> adds one gusset plate per diagonal node (bays + 1 plates: T0, B1, T2, ...; 44 triangles each):
        /// a chamfered plate in the truss plane, a little thicker than the chord web, reaching from inside the chord's far flange
        /// past the chord toward the diagonals; it hides the node overlaps of the webs. All parts are 44-triangle closed solids.
        /// With <paramref name="thinLine"/> every part writes ThinLine UV1 / UV2 (gussets use their chord's axis and radius, so
        /// they widen together with the chord).
        /// </summary>
        public static int WarrenTruss(MeshBuilder mb, Vector3 start, Vector3 end, float depth, int bays, float memberSize, Vector3 up,
            TrussMemberShape shape = TrussMemberShape.IBeam, bool thinLine = false, bool gussets = true)
        {
            Vector3 axis = end - start;
            float len = axis.magnitude;
            int nb = Mathf.Max(1, bays);
            if (len < 1e-4f || depth <= 0f || memberSize <= 0f) return 0;
            float s = memberSize;
            Vector3 dir = axis / len;
            Vector3 down = -PerpendicularUp(dir, up);
            Vector3 planeNormal = Vector3.Cross(dir, down).normalized;
            Vector3 drop = down * depth;
            float diagH = TrussDiagHeight * s, diagT = TrussDiagFlangeT * s;
            int count = 0;
            for (int i = 0; i < nb; i++)
            {
                Vector3 t0 = start + axis * ((float)i / nb), t1 = start + axis * ((float)(i + 1) / nb);
                Vector3 b0 = t0 + drop, b1 = t1 + drop;
                TrussMember(mb, t0, t1, s, s, TrussChordWeb * s, TrussChordFlangeT * s, planeNormal, shape, thinLine);
                TrussMember(mb, b0, b1, s, s, TrussChordWeb * s, TrussChordFlangeT * s, planeNormal, shape, thinLine);
                if ((i & 1) == 0)
                    TrussMember(mb, t0, b1, diagH, TrussDiagWidthA * s, TrussDiagWebA * s, diagT, planeNormal, shape, thinLine);
                else
                    TrussMember(mb, b0, t1, diagH, TrussDiagWidthB * s, TrussDiagWebB * s, diagT, planeNormal, shape, thinLine);
                count += 3;
            }
            if (gussets) TrussGussets(mb, start, axis, len, nb, depth, s, dir, down, thinLine);
            return count;
        }

        static void TrussMember(MeshBuilder mb, Vector3 a, Vector3 b, float height, float width, float web, float flangeT,
            Vector3 planeNormal, TrussMemberShape shape, bool thinLine)
        {
            Vector3 d = b - a;
            float len = d.magnitude;
            if (len < 1e-5f) return;
            Vector3 dir = d / len;
            Vector3 sectionUp = Vector3.Cross(planeNormal, dir); // in the truss plane, perpendicular to the member
            if (shape == TrussMemberShape.IBeam)
                IBeam(mb, a, b, height, width, web, flangeT, sectionUp, thinLine);
            else
                ChamferMember(mb, a, b, width, height, Mathf.Min(0.03f, 0.08f * Mathf.Min(width, height)), sectionUp, thinLine);
        }

        /// <summary>
        /// Gusset plates at the diagonal nodes (node k sits on the top chord for even k, on the bottom chord for odd k). Each plate
        /// lies in the truss plane, centred on it, aligned with the chord: thickness = chord web + 2 x max(6 mm, 0.02 s); across
        /// the chord from 0.45 s on the far side of the chord axis (buried in the far flange) to the reach toward the diagonals
        /// (at least 0.5 s past the chord face, and past the end of the diagonal-web overlap h_web / cos(theta)); along the chord
        /// far enough to contain both diagonals' sections at that reach, at most 0.8 bay, and kept 0.25 s inside the truss ends.
        /// Every plate face is offset from the chord and diagonal faces it is parallel to (>= 6 mm for the 0.3-0.4 m members).
        /// </summary>
        static void TrussGussets(MeshBuilder mb, Vector3 start, Vector3 axis, float len, int nb, float depth, float s, Vector3 dir,
            Vector3 down, bool thinLine)
        {
            float bay = len / nb;
            float hyp = Mathf.Sqrt(bay * bay + depth * depth);
            float cosT = Mathf.Max(bay / hyp, 0.05f), sinT = Mathf.Max(depth / hyp, 0.05f); // diagonal angle to the chord
            float chordHalf = 0.5f * s;
            float diagHalf = 0.5f * TrussDiagHeight * s;
            float diagWebHalf = diagHalf - TrussDiagFlangeT * s;
            float reach = Mathf.Max(chordHalf + 0.5f * s, diagWebHalf / cosT + 0.1f * s);
            reach = Mathf.Min(reach, 0.45f * depth);                   // plates of the two chords never meet
            if (reach < chordHalf + 0.05f * s) return;                 // truss too shallow for plates
            float back = chordHalf - 0.5f * TrussChordFlangeT * s;     // inside the far flange (0.05 s from both its faces)
            float half = Mathf.Min((reach * cosT + diagHalf) / sinT + 0.1f * s, 0.8f * bay);
            float endInset = 0.25f * s;
            float thick = TrussChordWeb * s + 2f * Mathf.Max(0.006f, 0.02f * s);
            float bevel = Mathf.Min(0.01f, 0.125f * thick);
            Vector3 drop = down * depth;
            for (int k = 0; k <= nb; k++)
            {
                bool top = (k & 1) == 0;
                Vector3 node = start + axis * ((float)k / nb);
                if (!top) node += drop;
                Vector3 inward = top ? down : -down;                   // toward the other chord
                float uMin = k > 0 ? -half : endInset, uMax = k < nb ? half : -endInset;
                if (uMax - uMin < 0.1f * s) continue;
                Vector3 centre = node + dir * (0.5f * (uMin + uMax)) + inward * (0.5f * (reach - back));
                var tl = BeginThinLine(mb, thinLine, dir, chordHalf, node);
                ChamferBox(mb, centre, new Vector3(thick, reach + back, uMax - uMin), bevel, Quaternion.LookRotation(dir, inward));
                EndThinLine(mb, tl);
            }
        }
    }
}
