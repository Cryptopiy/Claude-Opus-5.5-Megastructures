// G1Kit: the shared build helpers of beats 1 (Alley) and 2 (Boulevard canyon), owner G1 (Phase 3 graybox, MASTER_PROMPT Part 13).
// Builders never touch the scene directly: they fill a G1Parts sink with named MeshBuilders (one per material slot and chunk, each
// with its chunk-local origin, Part 7.5: every vertex within ~200 m horizontally of its origin), NocNeon instances, local lights,
// so EditMode tests run the same builders without a scene (Beat01AlleyTests / Beat02BoulevardTests ray-cast the parts).
// Commit() validates every part (MeshValidation), creates the meshes and renderers under the beat root, seals the instanced sign
// batches (NocCameraBatches, per camera) and adds the point lights. Conventions (MeshBuilder): front face where cross(b - a, c - a)
// points; vertex colour R = AO, G = edge wear. Faces that no camera of the walk region can see (roof caps above the eye inside
// solid masses, faces turned away from the whole walk, faces inside other solids) are not built; nothing is co-planar.
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Megastructure
{
    /// <summary>Material slots of G1's geometry (resolved by G1Materials from the MaterialLibrary, variants derived at load).</summary>
    public enum G1Mat
    {
        FloorAlley, FloorStone, Concrete, StoneWall, SteelPainted, SteelBare, Asphalt, Facade,
        EmitWarm, EmitCold, EmitSodium, EmitScreen, EmitMouth, EmitShopWarm, EmitShopCold,
        Line6mm, LineRail, LineRod, LinePost, GlassBack, GlassFront,
        EmitLamp,                         // Look v2 (PA, LOOK_V2 6 beat 1): the door bulkheads' LampWarm lens x EmissionSign
        Grating,                          // Look v2 (SU, TEXTURE_PLAN 3.5): open steel grating plates (NocGrating, two-sided)
        ConcreteAlley,                    // Look v2 (SU, TEXTURE_PLAN 1.3): the alley walls (concrete_wall_006; v1: Concrete)
        Wood, TileWall,                   // Look v2 (SU, TEXTURE_PLAN 3.5): the noodle-bar counter and back wall (v1: SteelPainted, Concrete)
        AlleyWindow,                      // AL (Phase 4 wave B, INT-06): framed recessed windows with rooms (NocAlleyWindow)
        AlleyRoomBar, AlleyRoomA, AlleyRoomB,   // AL (INT-07, DREAD 6): the noodle bar and the two empty rooms (NocAlleyRoom, own lamps)
        Steam, SteamBar,                  // AL (14.5.1 vents, INT-07 pots): NocSteam plumes
        AlleyPool,                        // AL (INT-21): baked light pools on the walls (NocAlleyPool)
        AlleyGlow,                        // AL (KS00): the lit boulevard haze seen through the mouth (NocAlleyGlow)
        AlleySign,                        // AL (INT-06): light-box faces of the blade signs (NocAlleyRoom kind 4, no lamps)
        AlleyWorkPool,                    // AL (fix wave, D-039 (6)): the catwalk work floodlights' wash on the KS02 walls (NocAlleyPool spot mode)
    }

    /// <summary>One NocNeon sign instance: centre, rotation, size (W, H) and palette (-1 hash, 0 magenta, 1 cyan, 2 amber, 3 violet).</summary>
    public struct G1Sign
    {
        public Vector3 centre;
        public Quaternion rotation;
        public Vector2 size;
        public int palette;
    }

    /// <summary>One realtime point light of the 14.2.3 light kit (no shadows).</summary>
    public struct G1Light
    {
        public string name;
        public Vector3 position;
        public Color colour;
        public float range, intensity;
    }

    public sealed class G1Parts
    {
        public sealed class Part
        {
            public string Name;
            public G1Mat Mat;
            public Vector3 Origin;
            public readonly MeshBuilder Mb = new MeshBuilder();
        }

        public readonly List<Part> Parts = new List<Part>(64);
        public readonly List<G1Sign> Signs = new List<G1Sign>(256);
        public readonly List<G1Light> Lights = new List<G1Light>(8);
        readonly Dictionary<string, Part> m_byKey = new Dictionary<string, Part>(64);

        /// <summary>The builder of part `name` (created on first use with its chunk-local origin; positions are given in world
        /// coordinates and stored relative to the origin through MeshBuilder.Matrix).</summary>
        public MeshBuilder Get(string name, G1Mat mat, Vector3 origin)
        {
            if (m_byKey.TryGetValue(name, out var p)) return p.Mb;
            p = new Part { Name = name, Mat = mat, Origin = origin };
            p.Mb.Matrix = Matrix4x4.Translate(-origin);
            Parts.Add(p);
            m_byKey[name] = p;
            return p.Mb;
        }

        public Part Find(string name) => m_byKey.TryGetValue(name, out var p) ? p : null;

        public void Sign(Vector3 centre, Quaternion rotation, float w, float h, int palette)
            => Signs.Add(new G1Sign { centre = centre, rotation = rotation, size = new Vector2(w, h), palette = palette });

        public void Light(string name, Vector3 pos, Color linear, float range, float intensity)
            => Lights.Add(new G1Light { name = name, position = pos, colour = linear, range = range, intensity = intensity });

        public long Triangles { get { long n = 0; foreach (var p in Parts) n += p.Mb.TriangleCount; return n; } }

        /// <summary>Validates and adds every part, sign batch and light under root. Returns the number of failed meshes.</summary>
        public int Commit(WorldContext ctx, Transform root, string prefix)
        {
            int failed = 0;
            var lib = ctx.Materials;
            foreach (var p in Parts)
            {
                if (p.Mb.VertexCount == 0) continue;
                var rep = MeshValidation.Validate(p.Mb, prefix + "_" + p.Name);
                if (!rep.Ok) { failed++; Debug.LogError("[NOC-FAIL] " + prefix + ": " + rep); }
                var mat = G1Materials.Get(lib, p.Mat);
                // AL slots of beat 1's own transparent shaders (steam, pools, the mouth haze): skipped when the shader is missing
                if (lib != null && mat == null && (p.Mat == G1Mat.Steam || p.Mat == G1Mat.SteamBar || p.Mat == G1Mat.AlleyPool || p.Mat == G1Mat.AlleyGlow || p.Mat == G1Mat.AlleyWorkPool))
                {
                    Debug.LogWarning("[NOC] " + prefix + "_" + p.Name + ": shader of " + p.Mat + " missing, part skipped");
                    continue;
                }
                Mesh mesh = p.Mb.ToMesh(prefix + "_" + p.Name);
                var mr = ctx.AddMesh(root, p.Name, mesh, mat);
                mr.transform.localPosition = p.Origin;
                if (p.Mat == G1Mat.GlassBack) ctx.AddMesh(root, p.Name + "_front", mesh, G1Materials.Get(lib, G1Mat.GlassFront)).transform.localPosition = p.Origin;
            }
            // NocNeon signs: one batch per palette (the colour is a material property, 14.5.2 blade cycle)
            if (Signs.Count > 0 && lib != null && lib.Neon != null)
            {
                var quad = NocTraffic.PointQuad();
                for (int pal = -1; pal <= 7; pal++)   // PA (LOOK_V2 10): NocNeon fixed slots 4 SignWhite, 5 WinNeutral, 6 AviationRed, 7 WinTungsten
                {
                    InstancedBatch b = null;
                    foreach (var s in Signs)
                    {
                        if (s.palette != pal) continue;
                        if (b == null) b = new InstancedBatch(prefix + "_signs" + (pal + 1), quad, G1Materials.Neon(lib, pal), chunkSize: 120f);
                        b.Add(Matrix4x4.TRS(s.centre, s.rotation, new Vector3(s.size.x, s.size.y, 1f)));
                    }
                    if (b != null) NocCameraBatches.Register(b);
                }
            }
            foreach (var l in Lights)
            {
                var go = new GameObject(l.name);
                go.transform.SetParent(root, false);
                go.transform.position = l.position;
                var li = go.AddComponent<Light>();
                li.type = LightType.Point;
                li.color = l.colour;
                li.range = l.range;
                li.intensity = l.intensity;
                li.shadows = LightShadows.None;
                li.renderMode = LightRenderMode.ForcePixel;
            }
            return failed;
        }
    }

    /// <summary>Mesh helpers of G1 (flat quads with an explicit outward normal, axis-aligned boxes by face mask, tubes).</summary>
    public static class G1Mesh
    {
        public const int PX = 1, NX = 2, PY = 4, NY = 8, PZ = 16, NZ = 32, All = 63, Sides = PX | NX | PZ | NZ;

        /// <summary>Test hook (TEXTURE_PLAN 3.5): when set, every builder a Quad writes into is recorded (no NocFacade part may be one:
        /// FacadeKit owns the facade UVs).</summary>
        public static HashSet<MeshBuilder> QuadBuilders;

        /// <summary>
        /// A flat quad a, b, c, d (any order around the rim) wound so that its normal points along `want`. Look v2 (TEXTURE_PLAN 3.5,
        /// SU-4): UV0 = panel-local metres (u along the bottom edge, growing to the viewer's right seen from outside; v up; level quads
        /// use world x, z) and UV1 = (W, H, 0, 0), the panel size, for NocShopfront's rooms. No other G1 shader reads UV0 / UV1.
        /// </summary>
        public static void Quad(MeshBuilder mb, Vector3 a, Vector3 b, Vector3 c, Vector3 d, Vector3 want)
        {
            QuadBuilders?.Add(mb);
            Vector3 n = Vector3.Cross(b - a, c - a);
            if (Vector3.Dot(n, want) < 0f) { var t = b; b = d; d = t; }
            Vector3 nn = want.sqrMagnitude > 1e-12f ? want.normalized : Vector3.up;
            Vector3 tu, tv;
            if (Mathf.Abs(nn.y) > 0.7f) { tu = Vector3.right; tv = Vector3.forward; }
            else { tu = Vector3.Cross(nn, Vector3.up).normalized; tv = Vector3.up; }
            float u0 = Mathf.Min(Mathf.Min(Vector3.Dot(a, tu), Vector3.Dot(b, tu)), Mathf.Min(Vector3.Dot(c, tu), Vector3.Dot(d, tu)));
            float u1 = Mathf.Max(Mathf.Max(Vector3.Dot(a, tu), Vector3.Dot(b, tu)), Mathf.Max(Vector3.Dot(c, tu), Vector3.Dot(d, tu)));
            float v0 = Mathf.Min(Mathf.Min(Vector3.Dot(a, tv), Vector3.Dot(b, tv)), Mathf.Min(Vector3.Dot(c, tv), Vector3.Dot(d, tv)));
            float v1 = Mathf.Max(Mathf.Max(Vector3.Dot(a, tv), Vector3.Dot(b, tv)), Mathf.Max(Vector3.Dot(c, tv), Vector3.Dot(d, tv)));
            Vector2 UV(Vector3 p) => new Vector2(Vector3.Dot(p, tu) - u0, Vector3.Dot(p, tv) - v0);
            bool w1 = mb.WriteUV1; Vector4 c1 = mb.CurrentUV1;
            mb.WriteUV1 = true; mb.CurrentUV1 = new Vector4(u1 - u0, v1 - v0, 0f, 0f);
            mb.FlatQuad(a, b, c, d, UV(a), UV(b), UV(c), UV(d));
            mb.WriteUV1 = w1; mb.CurrentUV1 = c1;
        }

        /// <summary>Axis-aligned box faces selected by mask (PX = the +X face ...), flat-shaded, outward.</summary>
        public static void Box(MeshBuilder mb, Vector3 min, Vector3 max, int mask)
        {
            float x0 = min.x, x1 = max.x, y0 = min.y, y1 = max.y, z0 = min.z, z1 = max.z;
            if ((mask & PX) != 0) Quad(mb, new Vector3(x1, y0, z0), new Vector3(x1, y1, z0), new Vector3(x1, y1, z1), new Vector3(x1, y0, z1), Vector3.right);
            if ((mask & NX) != 0) Quad(mb, new Vector3(x0, y0, z0), new Vector3(x0, y1, z0), new Vector3(x0, y1, z1), new Vector3(x0, y0, z1), Vector3.left);
            if ((mask & PY) != 0) Quad(mb, new Vector3(x0, y1, z0), new Vector3(x1, y1, z0), new Vector3(x1, y1, z1), new Vector3(x0, y1, z1), Vector3.up);
            if ((mask & NY) != 0) Quad(mb, new Vector3(x0, y0, z0), new Vector3(x1, y0, z0), new Vector3(x1, y0, z1), new Vector3(x0, y0, z1), Vector3.down);
            if ((mask & PZ) != 0) Quad(mb, new Vector3(x0, y0, z1), new Vector3(x1, y0, z1), new Vector3(x1, y1, z1), new Vector3(x0, y1, z1), Vector3.forward);
            if ((mask & NZ) != 0) Quad(mb, new Vector3(x0, y0, z0), new Vector3(x1, y0, z0), new Vector3(x1, y1, z0), new Vector3(x0, y1, z0), Vector3.back);
        }

        /// <summary>Chamfered box from its min / max corners (near geometry, 8.7: 1-3 cm bevels).</summary>
        public static void Chamfer(MeshBuilder mb, Vector3 min, Vector3 max, float bevel = 0.02f)
            => MeshKit.ChamferBox(mb, 0.5f * (min + max), max - min, bevel);

        /// <summary>A smooth round tube through a polyline (NocLine tube contract: radial normals, radius = true width / 2, no caps).</summary>
        public static void Tube(MeshBuilder mb, IList<Vector3> points, float radius, int sides, List<LoftFrame> frames, CrossSection section)
        {
            MeshKit.Circle(radius, sides, section);
            MeshKit.PathFrames(points, Vector3.up, frames, false, 89f);
            MeshKit.Loft(mb, section, frames, false);
        }

        /// <summary>Points of a hanging cable from a to b with sag (m) at the middle (parabolic approximation of the catenary).</summary>
        public static void Catenary(Vector3 a, Vector3 b, float sag, int segments, List<Vector3> into)
        {
            into.Clear();
            for (int i = 0; i <= segments; i++)
            {
                float t = i / (float)segments;
                Vector3 p = Vector3.Lerp(a, b, t);
                p.y -= 4f * sag * t * (1f - t);
                into.Add(p);
            }
        }

        /// <summary>Deterministic hash in [0, 1) of an integer key (layout jitter; never time-dependent).</summary>
        public static float U(uint k) => (float)NocMath.U01(NocMath.Pcg(k * 0x9E3779B9u + 0x47314B31u));
    }
}
