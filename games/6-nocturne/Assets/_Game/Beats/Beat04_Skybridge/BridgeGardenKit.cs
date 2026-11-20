// BridgeGardenKit: small helpers shared by the beat-4 (Skybridge) and beat-5 (Garden) builders: runtime material variants
// derived from the MaterialLibrary (properties set once at build, Phase-3 rule 9), linear colour assignment (Unity stores a
// Color property as sRGB and linearises it for the shader, so a linear palette value is passed as .gamma, like MaterialFactory),
// validated mesh emission (MeshValidation, [NOC-FAIL] on a broken mesh), shadowless point lights (14.2.3) and a few
// geometry primitives the MeshKit does not have (open boxes, a flat polygon at y = const, a quad oriented by a wanted normal).
// Load-time only: nothing here runs per frame.
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Megastructure
{
    public static class BridgeGardenKit
    {
        /// <summary>A runtime copy of a library material (null when the library or the source is missing: EditMode tests).</summary>
        public static Material Derive(Material src, string name)
        {
            if (src == null) return null;
            return new Material(src) { name = name };
        }

        /// <summary>Material from a shader name (Shaders/Beats files are Always Included by I0); null when the shader is missing.</summary>
        public static Material FromShader(string shaderName, string name)
        {
            var sh = Shader.Find(shaderName);
            if (sh == null) return null;
            return new Material(sh) { name = name };
        }

        /// <summary>Sets a Color property so the shader receives exactly the linear value (header).</summary>
        public static void SetLinear(Material m, string prop, Color linear)
        {
            if (m == null) return;
            m.SetColor(prop, new Color(linear.r, linear.g, linear.b, 1f).gamma);
        }

        public static void SetFloat(Material m, string prop, float v) { if (m != null) m.SetFloat(prop, v); }

        public static Color Lin(NocPal p, float k = 1f)
        {
            Color c = NocturnePalette.Linear(p);
            return new Color(c.r * k, c.g * k, c.b * k, 1f);
        }

        /// <summary>Validates the builder's data, builds the mesh and adds it under parent (null when the builder is empty).</summary>
        public static MeshRenderer Emit(WorldContext ctx, Transform parent, string name, MeshBuilder mb, Material mat)
        {
            if (mb == null || mb.VertexCount == 0 || mb.TriangleCount == 0) return null;
            Mesh mesh = mb.ToMesh(parent.name + "_" + name);
            var r = MeshValidation.Validate(mesh);
            if (!r.Ok) Debug.LogError(FormattableString.Invariant($"[NOC-FAIL] {parent.name}: mesh {mesh.name} failed validation: {r}"));
            return ctx.AddMesh(parent, name, mesh, mat);
        }

        /// <summary>Same mesh drawn by two renderers (NocGlass: back faces at 3000, then front faces at 3001, D-008).</summary>
        public static void EmitGlass(WorldContext ctx, Transform parent, string name, MeshBuilder mb, Material back, Material front)
        {
            var r = Emit(ctx, parent, name + "_back", mb, back);
            if (r == null) return;
            ctx.AddMesh(parent, name + "_front", r.GetComponent<MeshFilter>().sharedMesh, front);
        }

        public static Transform Group(Transform parent, string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            return go.transform;
        }

        /// <summary>A shadowless point light (14.2.3: no shadow maps; Forward+ local light).</summary>
        public static Light PointLight(Transform parent, string name, Vector3 pos, Color linear, float range, float intensity)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.position = pos;
            var l = go.AddComponent<Light>();
            l.type = LightType.Point;
            l.color = linear;
            l.range = range;
            l.intensity = intensity;
            l.shadows = LightShadows.None;
            l.renderMode = LightRenderMode.ForcePixel;
            return l;
        }

        /// <summary>A flat quad a-b-c-d (cyclic) whose normal points along want.</summary>
        public static void OrientedQuad(MeshBuilder mb, Vector3 a, Vector3 b, Vector3 c, Vector3 d, Vector3 want)
        {
            Vector3 n = Vector3.Cross(b - a, c - a);
            if (Vector3.Dot(n, want) >= 0f) mb.FlatQuad(a, b, c, d, Uv(a, want), Uv(b, want), Uv(c, want), Uv(d, want));
            else mb.FlatQuad(a, d, c, b, Uv(a, want), Uv(d, want), Uv(c, want), Uv(b, want));
        }

        /// <summary>Planar UV in metres along the dominant axis of the normal.</summary>
        public static Vector2 Uv(Vector3 p, Vector3 n)
        {
            float ax = Mathf.Abs(n.x), ay = Mathf.Abs(n.y), az = Mathf.Abs(n.z);
            if (ax >= ay && ax >= az) return new Vector2(p.z, p.y);
            if (ay >= az) return new Vector2(p.x, p.z);
            return new Vector2(p.x, p.y);
        }

        /// <summary>
        /// Axis-aligned box with only the listed faces (bit 0 -X, 1 +X, 2 -Y, 3 +Y, 4 -Z, 5 +Z): faces that would be co-planar
        /// with a neighbour or buried in it are left out (8.7: no co-planar faces).
        /// </summary>
        public const int FaceNX = 1, FacePX = 2, FaceNY = 4, FacePY = 8, FaceNZ = 16, FacePZ = 32, AllFaces = 63;

        public static void OpenBox(MeshBuilder mb, Vector3 min, Vector3 max, int faces)
        {
            Vector3 a = min, b = max;
            if ((faces & FaceNX) != 0) OrientedQuad(mb, new Vector3(a.x, a.y, a.z), new Vector3(a.x, b.y, a.z), new Vector3(a.x, b.y, b.z), new Vector3(a.x, a.y, b.z), Vector3.left);
            if ((faces & FacePX) != 0) OrientedQuad(mb, new Vector3(b.x, a.y, a.z), new Vector3(b.x, b.y, a.z), new Vector3(b.x, b.y, b.z), new Vector3(b.x, a.y, b.z), Vector3.right);
            if ((faces & FaceNY) != 0) OrientedQuad(mb, new Vector3(a.x, a.y, a.z), new Vector3(b.x, a.y, a.z), new Vector3(b.x, a.y, b.z), new Vector3(a.x, a.y, b.z), Vector3.down);
            if ((faces & FacePY) != 0) OrientedQuad(mb, new Vector3(a.x, b.y, a.z), new Vector3(b.x, b.y, a.z), new Vector3(b.x, b.y, b.z), new Vector3(a.x, b.y, b.z), Vector3.up);
            if ((faces & FaceNZ) != 0) OrientedQuad(mb, new Vector3(a.x, a.y, a.z), new Vector3(b.x, a.y, a.z), new Vector3(b.x, b.y, a.z), new Vector3(a.x, b.y, a.z), Vector3.back);
            if ((faces & FacePZ) != 0) OrientedQuad(mb, new Vector3(a.x, a.y, b.z), new Vector3(b.x, a.y, b.z), new Vector3(b.x, b.y, b.z), new Vector3(a.x, b.y, b.z), Vector3.forward);
        }

        static readonly List<int> s_tris = new List<int>(1024);

        /// <summary>A simple polygon (x, z) at height y facing up (or down), ear-clipped; UV0 = (x, z) metres.</summary>
        public static void FlatPolygon(MeshBuilder mb, List<Vector2> polyXZ, float y, bool up = true)
        {
            MeshKit.Triangulate(polyXZ, s_tris);
            Vector3 n = up ? Vector3.up : Vector3.down;
            int first = mb.VertexCount;
            for (int i = 0; i < polyXZ.Count; i++) mb.Vertex(new Vector3(polyXZ[i].x, y, polyXZ[i].y), n, polyXZ[i]);
            for (int t = 0; t + 2 < s_tris.Count; t += 3) MeshKit.Tri(mb, first + s_tris[t], first + s_tris[t + 1], first + s_tris[t + 2]);
        }

        /// <summary>Deterministic hash in [0, 1) of an integer pair (NocMath PCG).</summary>
        public static float Hash01(int a, int b) => (float)NocMath.U01(NocMath.Pcg2(a, b));
    }
}
