// DreadLights: the point-light meshes of package DR (docs/DREAD.md 2.1 belly rows and engine glows, 3.1 the cold lights on the
// shadow's back), drawn by Megastructure/NocDreadLight. Each light is one quad of four vertices that all sit at the light's centre
// (object space); uv0 = the corner (0..1), uv1 = (size m: lamp diameter or halo sigma, gain, colour 0 AviationRed / 1 SignWhite /
// 2 WinCold / 3 LampWarm / 4 WinWarm, profile 0 lamp / 1 Gaussian halo). The vertex shader builds the quad in clip space (at least 2.2 px,
// energy x (true / drawn)^2: the 14.2.5 rule of NocStreak), so the mesh moves with its object's transform at no CPU cost and stays
// one draw. Steady lights: no pulse, no flash (D-010).
using System.Collections.Generic;
using UnityEngine;

namespace Megastructure
{
    public struct DreadLight
    {
        public Vector3 Pos;
        public float Size, Gain;
        public int Colour;      // 0 AviationRed, 1 SignWhite, 2 WinCold, 3 LampWarm, 4 WinWarm
        public bool Halo;
        public DreadLight(Vector3 pos, float size, float gain, int colour, bool halo = false)
        { Pos = pos; Size = size; Gain = gain; Colour = colour; Halo = halo; }
    }

    public static class DreadLights
    {
        public const int Red = 0, White = 1, Cold = 2, Warm = 3, WinWarm = 4;

        /// <summary>The quad mesh of a light list (4 vertices, 2 triangles per light), bounds padded by pad metres (the quads grow in
        /// clip space and halos reach 3 sigma).</summary>
        public static Mesh ToMesh(string name, List<DreadLight> lights, float pad)
        {
            int n = lights.Count;
            var pos = new List<Vector3>(4 * n);
            var uv0 = new List<Vector2>(4 * n);
            var uv1 = new List<Vector4>(4 * n);
            var uv2 = new List<Vector4>(4 * n);
            var nrm = new List<Vector3>(4 * n);
            var idx = new List<int>(6 * n);
            Vector3 lo = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue), hi = -lo;
            for (int i = 0; i < n; i++)
            {
                var l = lights[i];
                var u = new Vector4(l.Size, l.Gain, l.Colour, l.Halo ? 1f : 0f);
                int b = pos.Count;
                for (int c = 0; c < 4; c++)
                {
                    pos.Add(l.Pos);
                    nrm.Add(Vector3.up);
                    uv0.Add(new Vector2(c == 1 || c == 2 ? 1f : 0f, c >= 2 ? 1f : 0f));
                    uv1.Add(u);
                    uv2.Add(Vector4.zero);          // static (NocDreadLight movers carry uv2.w = +-1)
                }
                idx.Add(b); idx.Add(b + 2); idx.Add(b + 1);
                idx.Add(b); idx.Add(b + 3); idx.Add(b + 2);
                lo = Vector3.Min(lo, l.Pos); hi = Vector3.Max(hi, l.Pos);
            }
            var mesh = new Mesh { name = name };
            mesh.SetVertices(pos);
            mesh.SetNormals(nrm);
            mesh.SetUVs(0, uv0);
            mesh.SetUVs(1, uv1);
            mesh.SetUVs(2, uv2);
            mesh.SetIndices(idx, MeshTopology.Triangles, 0, false);
            if (n > 0) mesh.bounds = new Bounds(0.5f * (lo + hi), hi - lo + 2f * pad * Vector3.one);
            return mesh;
        }
    }
}
