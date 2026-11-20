// SkybridgeFace: a planar wall x = const with rectangular and circular openings, built without T-junctions (Part 10 item 7).
// Local coordinates u = world z, v = world y. The face is the union of solid rectangular cells and "square rings" (a square
// with a circular hole: the tube mouths); openings are simply the regions with no cell. Every corner of every cell and every
// boundary sample of every ring is registered on its line (u = const or v = const); each cell is then emitted as a fan from its
// centre over ALL registered points on its four edges, so two neighbours always share every vertex along their common edge.
// Points of other meshes that meet the face (deck edges, trench floors) are registered with AddPoint for the same reason.
// The ring's square samples are the radial projections of the circle samples, snapped exactly onto the square at the side
// midpoints and corners (segment counts divisible by 8).
using System.Collections.Generic;
using UnityEngine;

namespace Megastructure
{
    public sealed class SkybridgeFace
    {
        struct Cell { public float u0, u1, v0, v1; }
        struct Ring { public float cu, cv, h, r; public int n; }

        readonly List<Cell> _cells = new List<Cell>();
        readonly List<Ring> _rings = new List<Ring>();
        readonly Dictionary<float, List<float>> _onU = new Dictionary<float, List<float>>();   // u = const -> v values
        readonly Dictionary<float, List<float>> _onV = new Dictionary<float, List<float>>();   // v = const -> u values
        static readonly List<Vector2> s_loop = new List<Vector2>(64);
        static readonly List<float> s_tmp = new List<float>(32);

        public void AddCell(float u0, float u1, float v0, float v1)
        {
            if (u1 - u0 < 1e-4f || v1 - v0 < 1e-4f) return;
            _cells.Add(new Cell { u0 = u0, u1 = u1, v0 = v0, v1 = v1 });
            AddPoint(u0, v0); AddPoint(u1, v0); AddPoint(u0, v1); AddPoint(u1, v1);
        }

        /// <summary>Registers a vertex on the face (it lies on the cell edges through it).</summary>
        public void AddPoint(float u, float v)
        {
            Put(_onU, u, v);
            Put(_onV, v, u);
        }

        static void Put(Dictionary<float, List<float>> d, float key, float val)
        {
            if (!d.TryGetValue(key, out var l)) { l = new List<float>(8); d[key] = l; }
            for (int i = 0; i < l.Count; i++) if (l[i] == val) return;
            l.Add(val);
        }

        /// <summary>A square of half size h around (cu, cv) with a circular hole of radius r (r &lt; h), n samples (n % 8 == 0).</summary>
        public void AddRing(float cu, float cv, float h, float r, int n)
        {
            _rings.Add(new Ring { cu = cu, cv = cv, h = h, r = r, n = n });
            for (int k = 0; k < n; k++) { Vector2 q = SquarePoint(cu, cv, h, k, n); AddPoint(q.x, q.y); }
        }

        /// <summary>Square-ring boundary sample k: the radial projection of circle sample k, exact at midpoints and corners.</summary>
        public static Vector2 SquarePoint(float cu, float cv, float h, int k, int n)
        {
            int e = n / 8;
            if (k % e == 0)
            {
                int o = (k / e) % 8;       // 0 = +u, 1 = corner (+u, +v), 2 = +v, ...
                float su = (o == 0 || o == 1 || o == 7) ? 1f : (o == 3 || o == 4 || o == 5) ? -1f : 0f;
                float sv = (o == 1 || o == 2 || o == 3) ? 1f : (o == 5 || o == 6 || o == 7) ? -1f : 0f;
                return new Vector2(cu + su * h, cv + sv * h);
            }
            float a = 2f * Mathf.PI * k / n, c = Mathf.Cos(a), s = Mathf.Sin(a);
            if (Mathf.Abs(c) >= Mathf.Abs(s)) return new Vector2(c > 0f ? cu + h : cu - h, cv + h * s / Mathf.Abs(c));
            return new Vector2(cu + h * c / Mathf.Abs(s), s > 0f ? cv + h : cv - h);
        }

        public static Vector2 CirclePoint(float cu, float cv, float r, int k, int n)
        {
            float a = 2f * Mathf.PI * k / n;
            return new Vector2(cu + r * Mathf.Cos(a), cv + r * Mathf.Sin(a));
        }

        /// <summary>Emits every cell and ring into mb on the plane x with normal (nx, 0, 0) (nx = +1 or -1).</summary>
        public void Emit(MeshBuilder mb, float x, float nx)
        {
            var n = new Vector3(nx, 0f, 0f);
            foreach (var c in _cells)
            {
                s_loop.Clear();
                s_loop.Add(new Vector2(c.u0, c.v0));
                Edge(_onV, c.v0, c.u0, c.u1, false, true);
                s_loop.Add(new Vector2(c.u1, c.v0));
                Edge(_onU, c.u1, c.v0, c.v1, true, true);
                s_loop.Add(new Vector2(c.u1, c.v1));
                Edge(_onV, c.v1, c.u0, c.u1, false, false);
                s_loop.Add(new Vector2(c.u0, c.v1));
                Edge(_onU, c.u0, c.v0, c.v1, true, false);
                int centre = mb.Vertex(new Vector3(x, 0.5f * (c.v0 + c.v1), 0.5f * (c.u0 + c.u1)), n, new Vector2(0.5f * (c.u0 + c.u1), 0.5f * (c.v0 + c.v1)));
                int first = mb.VertexCount;
                for (int i = 0; i < s_loop.Count; i++) mb.Vertex(new Vector3(x, s_loop[i].y, s_loop[i].x), n, s_loop[i]);
                for (int i = 0; i < s_loop.Count; i++) MeshKit.Tri(mb, centre, first + i, first + (i + 1) % s_loop.Count);
            }
            foreach (var r in _rings)
            {
                int first = mb.VertexCount;
                for (int k = 0; k < r.n; k++)
                {
                    Vector2 ci = CirclePoint(r.cu, r.cv, r.r, k, r.n), sq = SquarePoint(r.cu, r.cv, r.h, k, r.n);
                    mb.Vertex(new Vector3(x, ci.y, ci.x), n, ci);
                    mb.Vertex(new Vector3(x, sq.y, sq.x), n, sq);
                }
                for (int k = 0; k < r.n; k++)
                {
                    int a = first + 2 * k, b = first + 2 * ((k + 1) % r.n);
                    MeshKit.Quad(mb, a, a + 1, b + 1, b);
                }
            }
        }

        /// <summary>Appends the registered points strictly inside (a, b) on the line key, ascending or descending.</summary>
        void Edge(Dictionary<float, List<float>> d, float key, float a, float b, bool alongV, bool ascending)
        {
            if (!d.TryGetValue(key, out var l)) return;
            s_tmp.Clear();
            for (int i = 0; i < l.Count; i++) if (l[i] > a && l[i] < b) s_tmp.Add(l[i]);
            s_tmp.Sort();
            if (!ascending) s_tmp.Reverse();
            for (int i = 0; i < s_tmp.Count; i++)
                s_loop.Add(alongV ? new Vector2(key, s_tmp[i]) : new Vector2(s_tmp[i], key));
        }

        /// <summary>Registered v values on the line u (sorted), e.g. for the side faces that meet this face at its outer edges.</summary>
        public List<float> BreaksOnU(float u)
        {
            var res = new List<float>();
            if (_onU.TryGetValue(u, out var l)) res.AddRange(l);
            res.Sort();
            return res;
        }
    }
}
