// VisibilityRaycaster: CPU ray casting against the BUILT world for the 11.13 visibility audit ("built" columns of
// QA/reports/visibility.json). A bounding-volume hierarchy over world-space triangles (spatial-median split on the longest axis of
// the centroid bounds, object median when that fails, 4 triangles per leaf), Moller-Trumbore intersection, iterative traversal
// with nearest-first child order. Triangles carry the index of their source (a renderer path), so a hit names its occluder.
// Which geometry counts as an occluder is decided by the caller (VisibilityAuditTests: opaque MeshRenderers only; instanced signs,
// streaks, traffic and beacons are not MeshRenderers and are never occluders; transparent materials (queue >= 2500: NocGlass, the
// koi, rain) and the camera-following sky dome are excluded). Float storage (1 mm at 8 km, 14.4), double rays.
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Megastructure
{
    public sealed class VisibilityRaycaster : IVisRayScene
    {
        // triangle soup (world space): v0 and the two edges
        float[] m_v0x, m_v0y, m_v0z, m_e1x, m_e1y, m_e1z, m_e2x, m_e2y, m_e2z;
        int[] m_src;
        int m_count;
        readonly List<string> m_sources = new List<string>();

        // BVH nodes: bounds, and either children (left, right) or a leaf range (start, count)
        float[] m_bmin, m_bmax;
        int[] m_left, m_right, m_start, m_num;
        int m_nodes;
        int[] m_order;
        readonly int[] m_stack = new int[1024];

        public string Name => "built";
        public int TriangleCount => m_count;
        public int NodeCount => m_nodes;
        public IReadOnlyList<string> Sources => m_sources;
        public Bounds WorldBounds { get; private set; }

        // ------------------------------------------------------------------ building
        readonly List<Vector3> m_tmpA = new List<Vector3>(), m_tmpB = new List<Vector3>(), m_tmpC = new List<Vector3>();
        readonly List<int> m_tmpS = new List<int>();

        /// <summary>Adds the triangles of a mesh (positions, indices) transformed by m; returns the source index.</summary>
        public int AddMesh(string source, IList<Vector3> positions, IList<int> indices, Matrix4x4 m)
        {
            int src = m_sources.Count;
            m_sources.Add(source);
            for (int i = 0; i + 2 < indices.Count; i += 3)
            {
                Vector3 a = m.MultiplyPoint3x4(positions[indices[i]]);
                Vector3 b = m.MultiplyPoint3x4(positions[indices[i + 1]]);
                Vector3 c = m.MultiplyPoint3x4(positions[indices[i + 2]]);
                if (Vector3.Cross(b - a, c - a).sqrMagnitude < 1e-12f) continue;   // degenerate
                m_tmpA.Add(a); m_tmpB.Add(b); m_tmpC.Add(c); m_tmpS.Add(src);
            }
            return src;
        }

        /// <summary>Builds the hierarchy over every triangle added so far.</summary>
        public void Build()
        {
            int n = m_tmpA.Count;
            m_count = n;
            m_v0x = new float[n]; m_v0y = new float[n]; m_v0z = new float[n];
            m_e1x = new float[n]; m_e1y = new float[n]; m_e1z = new float[n];
            m_e2x = new float[n]; m_e2y = new float[n]; m_e2z = new float[n];
            m_src = new int[n];
            var cx = new float[n]; var cy = new float[n]; var cz = new float[n];
            var tmin = new Vector3[n]; var tmax = new Vector3[n];
            Vector3 wmin = Vector3.positiveInfinity, wmax = Vector3.negativeInfinity;
            for (int i = 0; i < n; i++)
            {
                Vector3 a = m_tmpA[i], b = m_tmpB[i], c = m_tmpC[i];
                m_v0x[i] = a.x; m_v0y[i] = a.y; m_v0z[i] = a.z;
                m_e1x[i] = b.x - a.x; m_e1y[i] = b.y - a.y; m_e1z[i] = b.z - a.z;
                m_e2x[i] = c.x - a.x; m_e2y[i] = c.y - a.y; m_e2z[i] = c.z - a.z;
                m_src[i] = m_tmpS[i];
                tmin[i] = Vector3.Min(a, Vector3.Min(b, c));
                tmax[i] = Vector3.Max(a, Vector3.Max(b, c));
                cx[i] = (tmin[i].x + tmax[i].x) * 0.5f; cy[i] = (tmin[i].y + tmax[i].y) * 0.5f; cz[i] = (tmin[i].z + tmax[i].z) * 0.5f;
                wmin = Vector3.Min(wmin, tmin[i]); wmax = Vector3.Max(wmax, tmax[i]);
            }
            WorldBounds = n > 0 ? new Bounds((wmin + wmax) * 0.5f, wmax - wmin) : new Bounds();
            m_tmpA.Clear(); m_tmpB.Clear(); m_tmpC.Clear(); m_tmpS.Clear();
            m_order = new int[n];
            for (int i = 0; i < n; i++) m_order[i] = i;
            int cap = Math.Max(1, 2 * n / 2 + 1) * 2;
            m_bmin = new float[cap * 3]; m_bmax = new float[cap * 3];
            m_left = new int[cap]; m_right = new int[cap]; m_start = new int[cap]; m_num = new int[cap];
            m_nodes = 0;
            if (n == 0) return;
            // iterative build: (node, start, count)
            var work = new Stack<(int node, int s, int c)>();
            int root = NewNode();
            work.Push((root, 0, n));
            while (work.Count > 0)
            {
                var (node, s, c) = work.Pop();
                Vector3 bmn = Vector3.positiveInfinity, bmx = Vector3.negativeInfinity, cmn = Vector3.positiveInfinity, cmx = Vector3.negativeInfinity;
                for (int i = s; i < s + c; i++)
                {
                    int t = m_order[i];
                    bmn = Vector3.Min(bmn, tmin[t]); bmx = Vector3.Max(bmx, tmax[t]);
                    var cc = new Vector3(cx[t], cy[t], cz[t]);
                    cmn = Vector3.Min(cmn, cc); cmx = Vector3.Max(cmx, cc);
                }
                m_bmin[node * 3] = bmn.x; m_bmin[node * 3 + 1] = bmn.y; m_bmin[node * 3 + 2] = bmn.z;
                m_bmax[node * 3] = bmx.x; m_bmax[node * 3 + 1] = bmx.y; m_bmax[node * 3 + 2] = bmx.z;
                if (c <= 4) { m_left[node] = -1; m_start[node] = s; m_num[node] = c; continue; }
                Vector3 ext = cmx - cmn;
                int axis = ext.x >= ext.y && ext.x >= ext.z ? 0 : (ext.y >= ext.z ? 1 : 2);
                float[] key = axis == 0 ? cx : (axis == 1 ? cy : cz);
                float mid = 0.5f * (cmn[axis] + cmx[axis]);
                int lo = s, hi = s + c - 1;
                while (lo <= hi)
                {
                    if (key[m_order[lo]] < mid) lo++;
                    else { int k = m_order[lo]; m_order[lo] = m_order[hi]; m_order[hi] = k; hi--; }
                }
                int nl = lo - s;
                if (nl == 0 || nl == c)
                {
                    // all centroids on one side (coincident): object median by sorting the range on the axis
                    Array.Sort(m_order, s, c, new KeyComparer(key));
                    nl = c / 2;
                }
                int l = NewNode(), r = NewNode();
                m_left[node] = l; m_right[node] = r; m_num[node] = 0;
                work.Push((l, s, nl));
                work.Push((r, s + nl, c - nl));
            }
        }

        sealed class KeyComparer : IComparer<int>
        {
            readonly float[] m_k;
            public KeyComparer(float[] k) { m_k = k; }
            public int Compare(int a, int b) => m_k[a].CompareTo(m_k[b]);
        }

        int NewNode()
        {
            if (m_nodes >= m_left.Length)
            {
                int cap = m_left.Length * 2;
                Array.Resize(ref m_bmin, cap * 3); Array.Resize(ref m_bmax, cap * 3);
                Array.Resize(ref m_left, cap); Array.Resize(ref m_right, cap); Array.Resize(ref m_start, cap); Array.Resize(ref m_num, cap);
            }
            return m_nodes++;
        }

        // ------------------------------------------------------------------ queries
        public bool Raycast(D3 o, D3 d, double tMax, out double t, out VisClass cls, out string label)
        {
            bool hit = Raycast(o, d, tMax, out t, out int src);
            cls = hit ? VisClass.Built : VisClass.None;
            label = hit ? m_sources[src] : null;
            return hit;
        }

        /// <summary>Nearest hit (t, source index) of the ray o + t d with 1e-4 &lt; t &lt; tMax.</summary>
        public bool Raycast(D3 o, D3 d, double tMax, out double tHit, out int source)
        {
            tHit = tMax; source = -1;
            if (m_nodes == 0) return false;
            float ox = (float)o.x, oy = (float)o.y, oz = (float)o.z;
            float dx = (float)d.x, dy = (float)d.y, dz = (float)d.z;
            float ix = Math.Abs(dx) > 1e-12f ? 1f / dx : float.MaxValue * Math.Sign(dx == 0f ? 1f : dx);
            float iy = Math.Abs(dy) > 1e-12f ? 1f / dy : float.MaxValue * Math.Sign(dy == 0f ? 1f : dy);
            float iz = Math.Abs(dz) > 1e-12f ? 1f / dz : float.MaxValue * Math.Sign(dz == 0f ? 1f : dz);
            float best = (float)Math.Min(tMax, 1e30);
            int found = -1;
            int sp = 0;
            m_stack[sp++] = 0;
            while (sp > 0)
            {
                int node = m_stack[--sp];
                if (!NodeHit(node, ox, oy, oz, ix, iy, iz, best, out _)) continue;
                if (m_left[node] < 0)
                {
                    int s = m_start[node], e = s + m_num[node];
                    for (int k = s; k < e; k++)
                    {
                        int tri = m_order[k];
                        if (Tri(tri, ox, oy, oz, dx, dy, dz, best, out float tt)) { best = tt; found = tri; }
                    }
                    continue;
                }
                int l = m_left[node], r = m_right[node];
                bool hl = NodeHit(l, ox, oy, oz, ix, iy, iz, best, out float tl);
                bool hr = NodeHit(r, ox, oy, oz, ix, iy, iz, best, out float tr);
                if (sp + 2 > m_stack.Length) { if (hl) m_stack[sp++] = l; continue; }   // never happens below depth 512
                if (hl && hr)
                {
                    if (tl < tr) { m_stack[sp++] = r; m_stack[sp++] = l; }
                    else { m_stack[sp++] = l; m_stack[sp++] = r; }
                }
                else if (hl) m_stack[sp++] = l;
                else if (hr) m_stack[sp++] = r;
            }
            if (found < 0) return false;
            tHit = best;
            source = m_src[found];
            return true;
        }

        bool NodeHit(int node, float ox, float oy, float oz, float ix, float iy, float iz, float tMax, out float tEnter)
        {
            int b = node * 3;
            float t0 = (m_bmin[b] - ox) * ix, t1 = (m_bmax[b] - ox) * ix;
            float tmin = Math.Min(t0, t1), tmax = Math.Max(t0, t1);
            t0 = (m_bmin[b + 1] - oy) * iy; t1 = (m_bmax[b + 1] - oy) * iy;
            tmin = Math.Max(tmin, Math.Min(t0, t1)); tmax = Math.Min(tmax, Math.Max(t0, t1));
            t0 = (m_bmin[b + 2] - oz) * iz; t1 = (m_bmax[b + 2] - oz) * iz;
            tmin = Math.Max(tmin, Math.Min(t0, t1)); tmax = Math.Min(tmax, Math.Max(t0, t1));
            tEnter = tmin;
            return tmax >= Math.Max(tmin, 0f) && tmin <= tMax;
        }

        bool Tri(int i, float ox, float oy, float oz, float dx, float dy, float dz, float tMax, out float t)
        {
            t = 0f;
            float e1x = m_e1x[i], e1y = m_e1y[i], e1z = m_e1z[i], e2x = m_e2x[i], e2y = m_e2y[i], e2z = m_e2z[i];
            float px = dy * e2z - dz * e2y, py = dz * e2x - dx * e2z, pz = dx * e2y - dy * e2x;
            float det = e1x * px + e1y * py + e1z * pz;
            if (Math.Abs(det) < 1e-12f) return false;
            float inv = 1f / det;
            float sx = ox - m_v0x[i], sy = oy - m_v0y[i], sz = oz - m_v0z[i];
            float u = (sx * px + sy * py + sz * pz) * inv;
            if (u < 0f || u > 1f) return false;
            float qx = sy * e1z - sz * e1y, qy = sz * e1x - sx * e1z, qz = sx * e1y - sy * e1x;
            float v = (dx * qx + dy * qy + dz * qz) * inv;
            if (v < 0f || u + v > 1f) return false;
            t = (e2x * qx + e2y * qy + e2z * qz) * inv;
            return t > 1e-4f && t < tMax;
        }
    }
}
