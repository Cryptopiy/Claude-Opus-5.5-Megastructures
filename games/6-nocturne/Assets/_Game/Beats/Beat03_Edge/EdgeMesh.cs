// EdgeMesh: the small mesh vocabulary of beat 3 (MASTER_PROMPT 8.7, 14.5.3) on top of Abyss's MeshBuilder / MeshKit:
//   * EdgePart: one named mesh of a hero builder (its MeshBuilder, chunk-local origin and material slot), so the builders stay
//     pure (EditMode tests build, validate and count them without a scene) and Beat03_Edge turns them into renderers;
//   * EdgeLight: a lamp of the 14.2.3 light kit (realtime point light and / or H4 wet source);
//   * FacadeWriter: quads in the NocFacade vertex contract (uv0 = (u metres along the face, 16-bit face id), uv1 = (uMin, uMax),
//     colour R = AO, B = mode 0 facade / 0.5 section / 1 roof, A = 0 no edge light), wound by the wanted normal;
//   * Quad (any material, wound by the wanted normal), a smooth tube along a polyline (NocLine tubes: radial normals, no caps),
//     a rotated 12-triangle box for far truss members.
using System.Collections.Generic;
using UnityEngine;

namespace Megastructure
{
    /// <summary>Material slots of beat 3 (resolved to MaterialLibrary materials or runtime variants in Beat03_Edge).</summary>
    /// AB: LayerLine / LayerLineNear (the Phase-3 layer strokes) are gone; TunnelMouth is the cross-bridge portal glow.
    public enum EdgeMat { Floor, Stone, Facade, Concrete, Steel, Glass, Rail, LampHead, BalLight, PortalRing, BridgeLight, TunnelMouth, Moss }

    public sealed class EdgePart
    {
        public string Name;
        public MeshBuilder Mb;
        public EdgeMat Mat;
        public Vector3 Origin;      // chunk-local origin (positions in Mb are relative to it)
        public EdgePart(string name, EdgeMat mat, Vector3 origin) { Name = name; Mat = mat; Origin = origin; Mb = new MeshBuilder(); }
    }

    public struct EdgeLight
    {
        public Vector3 Pos;
        public Color Linear;        // linear colour of the emitter
        public float Range, Intensity;
        public bool Realtime;
        public float Rho;           // wet-source glow half-width (m)
        public float Radiance;      // wet-source radiance factor (x Linear, <= 3 by the brightest channel)
    }

    /// <summary>Writes NocFacade quads (the CityMesher vertex contract) into a MeshBuilder.</summary>
    public struct FacadeWriter
    {
        public const byte ModeFacade = 0, ModeSection = 128, ModeRoof = 255;
        public MeshBuilder Mb;
        public Vector3 Origin;

        public FacadeWriter(MeshBuilder mb, Vector3 origin) { Mb = mb; Origin = origin; mb.WriteUV1 = true; }

        /// <summary>Quad a-b-c-d (cyclic, either orientation) with per-corner normals and u; wound so it faces `want`.</summary>
        public void Quad(Vector3 a, Vector3 b, Vector3 c, Vector3 d, Vector3 na, Vector3 nb, Vector3 nc, Vector3 nd,
            float ua, float ub, float uc, float ud, uint faceId, byte mode, float uMin, float uMax, Vector3 want)
        {
            Mb.Color = new Color32(255, 0, mode, 0);
            // FA (TQ-09): u extents nudged off the v2 shader's 1/16 m snap half-steps (<= 1 mm), so whole-bay decisions never flicker
            Mb.CurrentUV1 = new Vector4(NocFacadeMath.SafeExtent(uMin), NocFacadeMath.SafeExtent(uMax), 0f, 0f);
            float id = faceId & 0xFFFFu;
            int i0 = Mb.Vertex(a - Origin, na, new Vector2(ua, id));
            int i1 = Mb.Vertex(b - Origin, nb, new Vector2(ub, id));
            int i2 = Mb.Vertex(c - Origin, nc, new Vector2(uc, id));
            int i3 = Mb.Vertex(d - Origin, nd, new Vector2(ud, id));
            EdgeMesh.Wind(Mb, i0, i1, i2, i3, a, b, c, want);
        }

        /// <summary>Flat quad (one normal for all corners).</summary>
        public void Flat(Vector3 a, Vector3 b, Vector3 c, Vector3 d, Vector3 n, float ua, float ub, float uc, float ud,
            uint faceId, byte mode, float uMin, float uMax)
            => Quad(a, b, c, d, n, n, n, n, ua, ub, uc, ud, faceId, mode, uMin, uMax, n);
    }

    public static class EdgeMesh
    {
        /// <summary>Emits quad (i0..i3, cyclic) wound so cross(b - a, c - a) points along `want` (MeshBuilder front-face rule).</summary>
        public static void Wind(MeshBuilder mb, int i0, int i1, int i2, int i3, Vector3 a, Vector3 b, Vector3 c, Vector3 want)
        {
            Vector3 cr = Vector3.Cross(b - a, c - a);
            if (Vector3.Dot(cr, want) >= 0f) mb.Quad(i0, i1, i2, i3);
            else mb.Quad(i0, i3, i2, i1);
        }

        /// <summary>Quad with its own 4 vertices (any material), per-corner normals, planar uv0 = (u, v) given, wound toward `want`.</summary>
        public static void Quad(MeshBuilder mb, Vector3 origin, Vector3 a, Vector3 b, Vector3 c, Vector3 d,
            Vector3 na, Vector3 nb, Vector3 nc, Vector3 nd, Vector3 want)
        {
            int i0 = mb.Vertex(a - origin, na, new Vector2(a.x + a.z, a.y));
            int i1 = mb.Vertex(b - origin, nb, new Vector2(b.x + b.z, b.y));
            int i2 = mb.Vertex(c - origin, nc, new Vector2(c.x + c.z, c.y));
            int i3 = mb.Vertex(d - origin, nd, new Vector2(d.x + d.z, d.y));
            Wind(mb, i0, i1, i2, i3, a, b, c, want);
        }

        public static void FlatQuad(MeshBuilder mb, Vector3 origin, Vector3 a, Vector3 b, Vector3 c, Vector3 d, Vector3 n)
            => Quad(mb, origin, a, b, c, d, n, n, n, n, n);

        /// <summary>Triangle with its own vertices wound toward `want`.</summary>
        public static void Tri(MeshBuilder mb, Vector3 origin, Vector3 a, Vector3 b, Vector3 c, Vector3 n)
        {
            int i0 = mb.Vertex(a - origin, n), i1 = mb.Vertex(b - origin, n), i2 = mb.Vertex(c - origin, n);
            if (Vector3.Dot(Vector3.Cross(b - a, c - a), n) >= 0f) mb.Tri(i0, i1, i2);
            else mb.Tri(i0, i2, i1);
        }

        /// <summary>
        /// Smooth tube through the polyline (radial vertex normals, no caps): the NocLine tube contract (the shader pushes each vertex
        /// along its normal about the axis point position - normal x radius, so the mesh radius must equal _LineWidth / 2). The ring
        /// frame is the polyline tangent with a fixed reference axis (straight or gently curved lines only).
        /// </summary>
        public static void Tube(MeshBuilder mb, Vector3 origin, IList<Vector3> pts, float radius, int sides, Vector3 refAxis)
        {
            int n = pts.Count;
            if (n < 2) return;
            int baseIndex = mb.VertexCount;
            for (int i = 0; i < n; i++)
            {
                Vector3 t = (i == 0 ? pts[1] - pts[0] : (i == n - 1 ? pts[n - 1] - pts[n - 2] : pts[i + 1] - pts[i - 1])).normalized;
                Vector3 u = Vector3.Cross(refAxis, t);
                if (u.sqrMagnitude < 1e-8f) u = Vector3.Cross(Vector3.right, t);
                u.Normalize();
                Vector3 v = Vector3.Cross(t, u);
                for (int k = 0; k < sides; k++)
                {
                    float a = 2f * Mathf.PI * k / sides;
                    Vector3 nrm = u * Mathf.Cos(a) + v * Mathf.Sin(a);
                    mb.Vertex(pts[i] + nrm * radius - origin, nrm, new Vector2(radius * a, i));
                }
            }
            for (int i = 0; i < n - 1; i++)
                for (int k = 0; k < sides; k++)
                {
                    int k1 = (k + 1) % sides;
                    int a = baseIndex + i * sides + k, b = baseIndex + i * sides + k1;
                    int c = baseIndex + (i + 1) * sides + k1, d = baseIndex + (i + 1) * sides + k;
                    // outward: the radial normal; choose the winding by the geometric normal of (a, b, c)
                    Vector3 pa = mb.Positions[a], pb = mb.Positions[b], pc = mb.Positions[c];
                    Vector3 want = mb.Normals[a] + mb.Normals[b] + mb.Normals[c];
                    if (Vector3.Dot(Vector3.Cross(pb - pa, pc - pa), want) >= 0f) mb.Quad(a, b, c, d);
                    else mb.Quad(a, d, c, b);
                }
        }

        /// <summary>A 12-triangle box from a to b (width along `side`, height along the perpendicular), for far members (no bevel).</summary>
        public static void Member(MeshBuilder mb, Vector3 origin, Vector3 a, Vector3 b, float width, float height, Vector3 side)
        {
            Vector3 d = b - a;
            float len = d.magnitude;
            if (len < 1e-4f) return;
            Vector3 f = d / len;
            Vector3 up = Vector3.Cross(f, side);
            if (up.sqrMagnitude < 1e-8f) up = Vector3.up;
            up.Normalize();
            mb.Push(Matrix4x4.TRS(0.5f * (a + b) - origin, Quaternion.LookRotation(f, up), Vector3.one));
            mb.Box(Vector3.zero, new Vector3(width, height, len));
            mb.Pop();
        }

        /// <summary>Validates every part (MeshValidation) and returns the number of failures (logged).</summary>
        public static int ValidateAll(List<EdgePart> parts, List<Mesh> meshes, System.Text.StringBuilder log)
        {
            int bad = 0;
            for (int i = 0; i < meshes.Count; i++)
            {
                var r = MeshValidation.Validate(meshes[i]);
                if (!r.Ok) { bad++; if (log != null) log.AppendLine(r.Describe()); }
            }
            return bad;
        }
    }
}
