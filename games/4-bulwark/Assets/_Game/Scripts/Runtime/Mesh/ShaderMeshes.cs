// ShaderMeshes: procedural meshes whose positions are built in a shader. Extracted from Abyss's ProceduralTextures.cs
// (BeamWedgeMesh only, for Megastructure/BwBeam recipe 0). No game content.
using UnityEngine;

namespace Megastructure
{
    public static class ShaderMeshes
    {

        /// <summary>
        /// Fan beam prisms for Megastructure/BwBeam recipe 0: 'count' wedges centred at phi_k = phase0 + k * 2 pi / count
        /// (object-space angle in the fan plane from +X toward +Y). Each wedge is 8 vertices with
        /// UV1 = (phi_k, corner 0..3, cap 0/1, 0) and 12 triangles of a closed box; the shader builds the actual positions
        /// (R0/R1/half width/length from the material) and is winding-independent. Positions here are only a rough preview.
        /// Bounds cover every fan rotation for beams up to maxRadius + length.
        /// </summary>
        public static Mesh BeamWedgeMesh(int count, float phase0Rad, float maxRadius, float length)
        {
            var pos = new Vector3[count * 8];
            var uv1 = new Vector4[count * 8];
            var idx = new int[count * 36];
            int[] box =
            {
                0, 1, 2, 0, 2, 3,   // cap 0
                4, 6, 5, 4, 7, 6,   // cap 1
                0, 4, 5, 0, 5, 1,   // side between corners 0-1
                1, 5, 6, 1, 6, 2,
                2, 6, 7, 2, 7, 3,
                3, 7, 4, 3, 4, 0,
            };
            for (int k = 0; k < count; k++)
            {
                float phi = phase0Rad + k * 2f * Mathf.PI / count;
                for (int cap = 0; cap < 2; cap++)
                {
                    for (int c = 0; c < 4; c++)
                    {
                        int v = k * 8 + cap * 4 + c;
                        float r = c < 2 ? 0.5f * maxRadius : maxRadius;
                        float a = phi + ((c == 1 || c == 2) ? 0.1f : -0.1f);
                        pos[v] = new Vector3(r * Mathf.Cos(a), r * Mathf.Sin(a), cap * length);
                        uv1[v] = new Vector4(phi, c, cap, 0f);
                    }
                }
                for (int i = 0; i < 36; i++) idx[k * 36 + i] = k * 8 + box[i];
            }
            var m = new Mesh { name = "BwBeamWedges", hideFlags = HideFlags.DontSave };
            m.SetVertices(pos);
            m.SetUVs(1, uv1);
            m.SetTriangles(idx, 0, false);
            float ext = maxRadius + length;
            m.bounds = new Bounds(new Vector3(0f, 0f, 0.5f * length), new Vector3(2f * ext, 2f * ext, length + 2f * ext));
            m.UploadMeshData(true);
            return m;
        }
    }
}
