// Beat01Mesh: mesh helpers shared by the beat 1 and beat 5 builders (world-space quads that orient themselves, boxes in
// lane coordinates, tubes for BwLine, profile strips). Allocation only at build time.
using System;
using UnityEngine;

namespace Megastructure.Beats
{
    internal static class InteriorQuads
    {
        /// <summary>Quad a, b, c, d (a closed loop) emitted with the winding whose geometric normal points along
        /// <paramref name="facing"/> (MeshBuilder: clockwise seen from the front). Flat normal.</summary>
        public static void Face(MeshBuilder mb, Vector3 a, Vector3 b, Vector3 c, Vector3 d, Vector3 facing)
        {
            Vector3 n = Vector3.Cross(b - a, c - a) + Vector3.Cross(c - a, d - a);
            if (n.sqrMagnitude < 1e-14f) return;
            if (Vector3.Dot(n, facing) >= 0f) mb.FlatQuad(a, b, c, d);
            else mb.FlatQuad(a, d, c, b);
        }

        /// <summary>Triangle with the winding facing <paramref name="facing"/>.</summary>
        public static void Tri(MeshBuilder mb, Vector3 a, Vector3 b, Vector3 c, Vector3 facing)
        {
            Vector3 n = Vector3.Cross(b - a, c - a);
            if (n.sqrMagnitude < 1e-14f) return;
            n.Normalize();
            if (Vector3.Dot(n, facing) < 0f) { var t = b; b = c; c = t; n = -n; }
            int i0 = mb.Vertex(a, n), i1 = mb.Vertex(b, n), i2 = mb.Vertex(c, n);
            mb.Tri(i0, i1, i2);
        }

        /// <summary>Rectangle in the plane x = const, z [z0, z1], y [y0, y1], facing +x (sign +1) or -x.</summary>
        public static void RectX(MeshBuilder mb, double x, double z0, double z1, double y0, double y1, int sign)
        {
            if (Math.Abs(z1 - z0) < 1e-5 || Math.Abs(y1 - y0) < 1e-5) return;
            float fx = (float)x;
            Face(mb, new Vector3(fx, (float)y0, (float)z0), new Vector3(fx, (float)y1, (float)z0), new Vector3(fx, (float)y1, (float)z1), new Vector3(fx, (float)y0, (float)z1), new Vector3(sign, 0, 0));
        }

        /// <summary>Rectangle in the plane z = const, x [x0, x1], y [y0, y1], facing +z (sign +1) or -z.</summary>
        public static void RectZ(MeshBuilder mb, double z, double x0, double x1, double y0, double y1, int sign)
        {
            if (Math.Abs(x1 - x0) < 1e-5 || Math.Abs(y1 - y0) < 1e-5) return;
            float fz = (float)z;
            Face(mb, new Vector3((float)x0, (float)y0, fz), new Vector3((float)x0, (float)y1, fz), new Vector3((float)x1, (float)y1, fz), new Vector3((float)x1, (float)y0, fz), new Vector3(0, 0, sign));
        }

        /// <summary>Rectangle in the plane y = const, x [x0, x1], z [z0, z1], facing +y (sign +1) or -y.</summary>
        public static void RectY(MeshBuilder mb, double y, double x0, double x1, double z0, double z1, int sign)
        {
            if (Math.Abs(x1 - x0) < 1e-5 || Math.Abs(z1 - z0) < 1e-5) return;
            float fy = (float)y;
            Face(mb, new Vector3((float)x0, fy, (float)z0), new Vector3((float)x0, fy, (float)z1), new Vector3((float)x1, fy, (float)z1), new Vector3((float)x1, fy, (float)z0), new Vector3(0, sign, 0));
        }

        /// <summary>Axis-aligned box from min / max corners (24 vertices, hard edges).</summary>
        public static void Box(MeshBuilder mb, double x0, double y0, double z0, double x1, double y1, double z1)
        {
            var c = new Vector3((float)(0.5 * (x0 + x1)), (float)(0.5 * (y0 + y1)), (float)(0.5 * (z0 + z1)));
            var size = new Vector3((float)Math.Abs(x1 - x0), (float)Math.Abs(y1 - y0), (float)Math.Abs(z1 - z0));
            if (size.x < 1e-5f || size.y < 1e-5f || size.z < 1e-5f) return;
            mb.Box(c, size);
        }

        /// <summary>Chamfered axis-aligned box from min / max corners (MeshKit.ChamferBox: wear on the chamfers).</summary>
        public static void CBox(MeshBuilder mb, double x0, double y0, double z0, double x1, double y1, double z1, float bevel)
        {
            var c = new Vector3((float)(0.5 * (x0 + x1)), (float)(0.5 * (y0 + y1)), (float)(0.5 * (z0 + z1)));
            var size = new Vector3((float)Math.Abs(x1 - x0), (float)Math.Abs(y1 - y0), (float)Math.Abs(z1 - z0));
            if (size.x < 1e-5f || size.y < 1e-5f || size.z < 1e-5f) return;
            MeshKit.ChamferBox(mb, c, size, bevel);
        }

        /// <summary>Round tube a -> b with smooth outward normals, a ring at least every <paramref name="ringStep"/> m, open
        /// ends (BwLine _LINE_TUBE contract, D-07) or capped (lit steel pipes).</summary>
        public static void Tube(MeshBuilder mb, Vector3 a, Vector3 b, float radius, int sides = 10, float ringStep = 2f, bool caps = false)
        {
            Vector3 d = b - a;
            float len = d.magnitude;
            if (len < 1e-4f || radius <= 0f) return;
            Vector3 dir = d / len;
            Vector3 h = Mathf.Abs(dir.y) < 0.9f ? Vector3.up : Vector3.right;
            Vector3 u = Vector3.Cross(h, dir).normalized, v = Vector3.Cross(dir, u);
            int rings = Mathf.Max(2, Mathf.CeilToInt(len / ringStep) + 1);
            int first = mb.VertexCount;
            for (int i = 0; i < rings; i++)
            {
                Vector3 c = a + d * (i / (float)(rings - 1));
                for (int j = 0; j < sides; j++)
                {
                    float ph = 2f * Mathf.PI * j / sides;
                    Vector3 n = u * Mathf.Cos(ph) + v * Mathf.Sin(ph);
                    mb.Vertex(c + n * radius, n, new Vector2(radius * ph, len * i / (rings - 1)));
                }
            }
            for (int i = 0; i + 1 < rings; i++)
                for (int j = 0; j < sides; j++)
                {
                    int jn = (j + 1) % sides;
                    int A = first + i * sides + j, D = first + i * sides + jn, C = first + (i + 1) * sides + jn, B = first + (i + 1) * sides + j;
                    mb.Quad(A, D, C, B);
                }
            if (!caps) return;
            for (int e = 0; e < 2; e++)
            {
                Vector3 c = e == 0 ? a : b, n = e == 0 ? -dir : dir;
                int cc = mb.Vertex(c, n);
                int r0 = mb.VertexCount;
                for (int j = 0; j < sides; j++)
                {
                    float ph = 2f * Mathf.PI * j / sides;
                    mb.Vertex(c + (u * Mathf.Cos(ph) + v * Mathf.Sin(ph)) * radius, n);
                }
                for (int j = 0; j < sides; j++)
                {
                    int jn = (j + 1) % sides;
                    if (e == 0) mb.Tri(cc, r0 + jn, r0 + j); else mb.Tri(cc, r0 + j, r0 + jn);
                }
            }
        }

        /// <summary>Inner surface of a round bore a -> b (normals toward the axis), open ends.</summary>
        public static void Bore(MeshBuilder mb, Vector3 a, Vector3 b, float radius, int sides = 24)
        {
            Vector3 d = b - a;
            float len = d.magnitude;
            if (len < 1e-4f || radius <= 0f) return;
            Vector3 dir = d / len;
            Vector3 h = Mathf.Abs(dir.y) < 0.9f ? Vector3.up : Vector3.right;
            Vector3 u = Vector3.Cross(h, dir).normalized, v = Vector3.Cross(dir, u);
            int rings = Mathf.Max(2, Mathf.CeilToInt(len / 1f) + 1);
            int first = mb.VertexCount;
            for (int i = 0; i < rings; i++)
            {
                Vector3 c = a + d * (i / (float)(rings - 1));
                for (int j = 0; j < sides; j++)
                {
                    float ph = 2f * Mathf.PI * j / sides;
                    Vector3 n = u * Mathf.Cos(ph) + v * Mathf.Sin(ph);
                    mb.Vertex(c + n * radius, -n, new Vector2(radius * ph, len * i / (rings - 1)));
                }
            }
            for (int i = 0; i + 1 < rings; i++)
                for (int j = 0; j < sides; j++)
                {
                    int jn = (j + 1) % sides;
                    int A = first + i * sides + j, D = first + i * sides + jn, C = first + (i + 1) * sides + jn, B = first + (i + 1) * sides + j;
                    mb.Quad(A, B, C, D);
                }
        }

        /// <summary>Flat face between a square of half-size <paramref name="half"/> and a circle of radius r (centre c, plane
        /// spanned by u, v), facing n: the wall around a round porthole.</summary>
        public static void SquareRing(MeshBuilder mb, Vector3 c, Vector3 u, Vector3 v, float half, float r, Vector3 n, int segments = 24)
        {
            for (int j = 0; j < segments; j++)
            {
                float a0 = 2f * Mathf.PI * j / segments, a1 = 2f * Mathf.PI * (j + 1) / segments;
                Vector3 p0 = c + (u * Mathf.Cos(a0) + v * Mathf.Sin(a0)) * r, p1 = c + (u * Mathf.Cos(a1) + v * Mathf.Sin(a1)) * r;
                Vector3 q0 = c + SquarePoint(a0, half, u, v), q1 = c + SquarePoint(a1, half, u, v);
                Face(mb, p0, q0, q1, p1, n);
            }
        }

        static Vector3 SquarePoint(float ang, float half, Vector3 u, Vector3 v)
        {
            float cx = Mathf.Cos(ang), cy = Mathf.Sin(ang);
            float m = Mathf.Max(Mathf.Abs(cx), Mathf.Abs(cy));
            return (u * cx + v * cy) * (half / Mathf.Max(m, 1e-6f));
        }
    }
}
