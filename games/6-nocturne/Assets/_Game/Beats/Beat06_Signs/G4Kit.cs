// G4Kit: small load-time helpers shared by the beat 6 and beat 7 builders (G4): material variants derived from the
// MaterialLibrary (properties set once at build, geometry agreement 9), point lights of the 14.2.3 rig (no shadows), NocFacade
// faces with the CityMesher vertex contract (uv0 = (u world-derived metres, 16-bit face id), uv1 = (uMin, uMax, y0, y1), colour
// R AO, G skin byte (NocFacadeMath.SkinByte), B mode, A edge kind x 63), a placer that walks a lane edge (NocLanes) at a fixed spacing, flat oriented quads and the
// MeshValidation gate (a failure logs [NOC-FAIL]). Nothing here runs per frame.
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Megastructure
{
    public static class G4Kit
    {
        static readonly int s_emission = Shader.PropertyToID("_EmissionColor");
        static readonly int s_base = Shader.PropertyToID("_BaseColor");
        static readonly int s_lineWidth = Shader.PropertyToID("_LineWidth");

        // ------------------------------------------------------------------ materials
        /// <summary>A copy of `source` with a linear HDR emission (SetVector: no gamma conversion of the value).</summary>
        public static Material Emissive(Material source, Color linear, double gain, string name)
        {
            if (source == null) return null;
            var m = new Material(source) { name = name };
            if (m.HasProperty(s_emission)) m.SetVector(s_emission, new Vector4((float)(linear.r * gain), (float)(linear.g * gain), (float)(linear.b * gain), 1f));
            return m;
        }

        /// <summary>A copy of `source` with another linear albedo token.</summary>
        public static Material Tinted(Material source, Color linear, string name)
        {
            if (source == null) return null;
            var m = new Material(source) { name = name };
            if (m.HasProperty(s_base)) m.SetVector(s_base, new Vector4(linear.r, linear.g, linear.b, 1f));
            return m;
        }

        /// <summary>A NocLine variant (tube mode of the library line) with its own true width, albedo and emission.</summary>
        public static Material Line(Material source, double width, Color albedoLinear, Color emissionLinear, string name)
        {
            if (source == null) return null;
            var m = new Material(source) { name = name };
            m.SetFloat(s_lineWidth, (float)width);
            m.SetVector(s_base, new Vector4(albedoLinear.r, albedoLinear.g, albedoLinear.b, 1f));
            m.SetVector(s_emission, new Vector4(emissionLinear.r, emissionLinear.g, emissionLinear.b, 1f));
            return m;
        }

        /// <summary>A material of a beat shader found by name (Always Included by I0's catalog); null (with a warning) when absent.</summary>
        public static Material FromShader(string shaderName, string name)
        {
            var sh = Shader.Find(shaderName);
            if (sh == null) { Debug.LogWarning("[NOC] shader " + shaderName + " not found (" + name + " uses its fallback)"); return null; }
            return new Material(sh) { name = name };
        }

        public static Color Lin(NocPal p) => NocturnePalette.Linear(p);
        public static Color Scale(Color c, double k) => new Color((float)(c.r * k), (float)(c.g * k), (float)(c.b * k), 1f);
        public static Color Mix(Color a, Color b, double t) => new Color((float)(a.r + (b.r - a.r) * t), (float)(a.g + (b.g - a.g) * t), (float)(a.b + (b.b - a.b) * t), 1f);

        /// <summary>SG: the state of one shop panel in BV's NocShopfront vertex-colour contract (R random, G light reduction, B state x 32,
        /// A half-shutter edge). Closed bays get a roller shutter (state 1; some leak a line of light, some are perforated). Open bays:
        /// 56 % lit (gain 0.55-0.85), 20 % dim (0.16-0.30), 12 % a half-lowered shutter over a lit room (0.50-0.80, edge at 42-70 % of
        /// the height), 7 % dark (state 3, stand-by light), 5 % shuttered. Gain multiplies the room light (never above the lit room).</summary>
        public struct ShopPanel { public int State; public double Gain, Rnd, ShutterFrac; public bool Open; }

        public static ShopPanel PickShop(uint h, bool closed)
        {
            double u0 = NocHash.U01(NocHash.Pcg(h ^ 0x53544154u)), u1 = NocHash.U01(NocHash.Pcg(h ^ 0x4741494Eu));
            double u2 = NocHash.U01(NocHash.Pcg(h ^ 0x524E4421u)), u3 = NocHash.U01(NocHash.Pcg(h ^ 0x46524143u));
            var p = new ShopPanel { Rnd = u2, ShutterFrac = 1.0 };
            if (closed || u0 >= 0.95) { p.State = 1; p.Gain = 0.40 + 0.30 * u1; }
            else if (u0 < 0.56) { p.State = 0; p.Gain = 0.55 + 0.30 * u1; p.Open = true; }
            else if (u0 < 0.76) { p.State = 0; p.Gain = 0.16 + 0.14 * u1; p.Open = true; }
            else if (u0 < 0.88) { p.State = 2; p.Gain = 0.50 + 0.30 * u1; p.ShutterFrac = 0.42 + 0.28 * u3; p.Open = true; }
            else { p.State = 3; p.Gain = 0.5; }
            return p;
        }

        /// <summary>The vertex colour of a shop panel (BV's ShopFronts.PanelColour contract).</summary>
        public static Color32 ShopColour(ShopPanel p)
        {
            byte r = (byte)Mathf.Clamp(Mathf.RoundToInt((float)p.Rnd * 255f), 0, 255);
            byte g = (byte)Mathf.Clamp(Mathf.RoundToInt((1f - Mathf.Clamp01((float)p.Gain)) * 255f), 0, 255);
            byte a = (byte)Mathf.Clamp(Mathf.RoundToInt(Mathf.Clamp01(p.State == 2 ? (float)p.ShutterFrac : 1f) * 255f), 0, 255);
            return new Color32(r, g, (byte)(p.State * 32), a);
        }

        static readonly int s_roomDepth = Shader.PropertyToID("_RoomDepth"), s_shelfPitch = Shader.PropertyToID("_ShelfPitch");
        static readonly int s_itemWidth = Shader.PropertyToID("_ItemWidth"), s_seed = Shader.PropertyToID("_Seed"), s_palette = Shader.PropertyToID("_Palette");

        /// <summary>SG (INT-34): a lit shop room behind glass, a variant of BV's NocShopfront row (lib.Shopfront): room light token x
        /// factor (the panel mean; the shader adds its room gain 0.6), room depth, shelf pitch, product width (m) and seed. Without the
        /// row (-surf-v 1) a flat dark-glass panel at 0.6 x the light (the room's mean).</summary>
        public static Material ShopRoom(MaterialLibrary lib, string name, NocPal token, double factor, float depth, float pitch, float itemW, int seed)
        {
            if (lib == null) return null;
            Color e = NocturnePalette.Linear(token) * (float)factor;
            e.a = 1f;
            if (NocLookGlobals.SurfV != 2 || lib.Shopfront == null) return Emissive(lib.Concrete, e, 0.6, name + "_Flat");
            var m = new Material(lib.Shopfront) { name = name };
            m.SetColor(s_emission, e.gamma);                 // an [HDR] Color property: the gamma value is linearised by Unity (G1Materials.Room)
            m.SetFloat(s_roomDepth, depth);
            m.SetFloat(s_shelfPitch, pitch);
            m.SetFloat(s_itemWidth, itemW);
            m.SetFloat(s_seed, seed);
            return m;
        }

        /// <summary>A NocNeon variant with a fixed palette slot (1 NeonCyan, 2 SodiumAmber x k_a, 4 SignWhite, 5 WinNeutral; LOOK_V2 10).</summary>
        public static Material Neon(MaterialLibrary lib, int palette, string name)
        {
            if (lib == null || lib.Neon == null) return null;
            var m = new Material(lib.Neon) { name = name };
            m.SetFloat(s_palette, palette);
            return m;
        }

        /// <summary>Registers NocNeon instances (centre, rotation, size W x H, palette slot) as one InstancedBatch per palette
        /// (per-camera submission, NocCameraBatches; the glyph seed and breathing phase hash from the centre).</summary>
        public static int NeonSigns(MaterialLibrary lib, string name, List<ArcadeBuilder.Sign> signs)
        {
            if (lib == null || lib.Neon == null || signs == null || signs.Count == 0) return 0;
            int n = 0;
            for (int pal = 0; pal <= 7; pal++)
            {
                InstancedBatch b = null;
                foreach (var s in signs)
                {
                    if (s.palette != pal) continue;
                    if (b == null) b = new InstancedBatch(name + pal, NocTraffic.PointQuad(), Neon(lib, pal, name + "_Neon" + pal), chunkSize: 200f);
                    b.Add(Matrix4x4.TRS(s.centre, s.rotation, new Vector3(s.size.x, s.size.y, 1f)));
                    n++;
                }
                if (b != null) NocCameraBatches.Register(b);
            }
            return n;
        }

        // ------------------------------------------------------------------ lights (14.2.3: no shadow maps)
        /// <summary>A point light; `linear` is the palette colour (Light.color takes gamma values, URP linearises them).</summary>
        public static Light PointLight(Transform parent, string name, Vector3 pos, Color linear, double range, double intensity)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.position = pos;
            var l = go.AddComponent<Light>();
            l.type = LightType.Point;
            l.color = new Color(linear.r, linear.g, linear.b, 1f).gamma;
            l.range = (float)range;
            l.intensity = (float)intensity;
            l.shadows = LightShadows.None;
            l.renderMode = LightRenderMode.ForcePixel;
            return l;
        }

        // ------------------------------------------------------------------ meshes
        /// <summary>Adds a mesh object at `origin` (world) under `parent`; the builder's positions are relative to origin. The
        /// mesh is validated first (MeshValidation; a failure logs [NOC-FAIL]).</summary>
        public static MeshRenderer Add(WorldContext ctx, Transform parent, string name, MeshBuilder mb, Material mat, Vector3 origin)
        {
            if (mb == null || mb.VertexCount == 0) return null;
            Mesh mesh = mb.ToMesh(name);
            Check(mesh);
            var mr = ctx.AddMesh(parent, name, mesh, mat);
            mr.transform.position = origin;
            return mr;
        }

        public static bool Check(Mesh mesh)
        {
            var r = MeshValidation.Validate(mesh);
            if (!r.Ok) Debug.LogError(FormattableString.Invariant($"[NOC-FAIL] G4 mesh {mesh.name} failed validation: {r}"));
            return r.Ok;
        }

        /// <summary>SG: a flat shop-glass quad for NocShopfront (G1Mesh.Quad's convention): UV0 = panel-local metres (u along the bottom
        /// edge, growing to the viewer's right seen from outside; v up), UV1 = (W, H, 0, 0), wound so its normal points along `want`.</summary>
        public static void ShopQuad(MeshBuilder mb, Vector3 a, Vector3 b, Vector3 c, Vector3 d, Vector3 want)
        {
            Vector3 n = Vector3.Cross(b - a, c - a);
            if (Vector3.Dot(n, want) < 0f) { var t = b; b = d; d = t; }
            Vector3 nn = want.sqrMagnitude > 1e-12f ? want.normalized : Vector3.up;
            Vector3 tu = Vector3.Cross(nn, Vector3.up).normalized, tv = Vector3.up;
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

        /// <summary>A flat quad (own four vertices) whose winding is chosen so its normal points along `want`.</summary>
        public static void OrientedQuad(MeshBuilder mb, Vector3 a, Vector3 b, Vector3 c, Vector3 d, Vector3 want)
        {
            Vector3 n = Vector3.Cross(b - a, c - a);
            if (Vector3.Dot(n, want) >= 0f) mb.FlatQuad(a, b, c, d);
            else mb.FlatQuad(a, d, c, b);
        }

        /// <summary>
        /// One planar NocFacade face (CityMesher contract) from its corners in WORLD metres: a0 / b0 bottom, a1 / b1 top (edge A and
        /// edge B), outward horizontal normal `n`. u = dot(P.xz, t) with t the normal turned +90 deg of yaw and the face id from
        /// (yaw, plane offset) as in CityField.FaceFrame, so the windows are world-derived; uMin / uMax are written per vertex
        /// (exact for tapered faces: both are linear in y). Positions are stored relative to `origin`.
        /// </summary>
        public static void FacadeQuad(MeshBuilder mb, Vector3 origin, Vector3 a0, Vector3 a1, Vector3 b1, Vector3 b0, Vector3 n, uint seed, int edge, int mode = 0, int region = -1)
        {
            double nl = Math.Sqrt((double)n.x * n.x + (double)n.z * n.z);
            if (nl < 1e-9) return;
            // Look v2 (TEXTURE_PLAN 2.2): G = the skin byte (region of the box centre, or of the face centre for a lone face) and
            // uv1.zw = the face's bottom and top in world y on both CurrentUV1 assignments
            if (region < 0) region = NocFacadeMath.Region(0.25 * ((double)a0.x + a1.x + b0.x + b1.x), 0.25 * ((double)a0.z + a1.z + b0.z + b1.z));
            float yLo = NocFacadeMath.GridY0(Mathf.Min(a0.y, b0.y)), yHi = NocFacadeMath.GridY1(Mathf.Max(a1.y, b1.y));
            double nx = n.x / nl, nz = n.z / nl;
            double yawN = Math.Atan2(nx, nz) / NocMath.Deg;
            double r = (yawN + 90.0) * NocMath.Deg;
            double tx = Math.Sin(r), tz = Math.Cos(r);
            double offset = a0.x * nx + a0.z * nz;
            uint id = NocFacadeMath.FaceId(yawN, offset);
            float fid = id;
            double uA0 = a0.x * tx + a0.z * tz, uB0 = b0.x * tx + b0.z * tz, uA1 = a1.x * tx + a1.z * tz, uB1 = b1.x * tx + b1.z * tz;
            mb.WriteUV1 = true;
            mb.Color = new Color32(255, NocFacadeMath.SkinByte(seed, region), (byte)(mode == 2 ? 255 : (mode == 1 ? 128 : 0)), (byte)(63 * edge));
            Vector3 fn = Vector3.Cross(a1 - a0, b1 - a0).normalized;
            if (Vector3.Dot(fn, n) < 0f) fn = -fn;
            mb.CurrentUV1 = new Vector4(NocFacadeMath.SafeExtent(Math.Min(uA0, uB0)), NocFacadeMath.SafeExtent(Math.Max(uA0, uB0)), yLo, yHi);
            int i0 = mb.Vertex(a0 - origin, fn, new Vector2((float)uA0, fid));
            int i3 = mb.Vertex(b0 - origin, fn, new Vector2((float)uB0, fid));
            mb.CurrentUV1 = new Vector4(NocFacadeMath.SafeExtent(Math.Min(uA1, uB1)), NocFacadeMath.SafeExtent(Math.Max(uA1, uB1)), yLo, yHi);
            int i1 = mb.Vertex(a1 - origin, fn, new Vector2((float)uA1, fid));
            int i2 = mb.Vertex(b1 - origin, fn, new Vector2((float)uB1, fid));
            Vector3 c = Vector3.Cross(a1 - a0, b1 - a0);
            if (Vector3.Dot(c, fn) >= 0f) mb.Quad(i0, i1, i2, i3);
            else mb.Quad(i0, i3, i2, i1);
            mb.Color = new Color32(255, 0, 0, 255);
        }

        /// <summary>The four vertical NocFacade faces of an axis-aligned box x0..x1, z0..z1, y0..y1 (world), positions relative to origin.</summary>
        public static void FacadeBox(MeshBuilder mb, Vector3 origin, double x0, double x1, double z0, double z1, double y0, double y1, uint seed, int edge,
            bool west = true, bool east = true, bool south = true, bool north = true)
        {
            float X0 = (float)x0, X1 = (float)x1, Z0 = (float)z0, Z1 = (float)z1, Y0 = (float)y0, Y1 = (float)y1;
            int rg = NocFacadeMath.Region(0.5 * (x0 + x1), 0.5 * (z0 + z1));      // one skin per box (TEXTURE_PLAN 2.3)
            if (west) FacadeQuad(mb, origin, new Vector3(X0, Y0, Z1), new Vector3(X0, Y1, Z1), new Vector3(X0, Y1, Z0), new Vector3(X0, Y0, Z0), Vector3.left, seed, edge, 0, rg);
            if (east) FacadeQuad(mb, origin, new Vector3(X1, Y0, Z0), new Vector3(X1, Y1, Z0), new Vector3(X1, Y1, Z1), new Vector3(X1, Y0, Z1), Vector3.right, seed, edge, 0, rg);
            if (south) FacadeQuad(mb, origin, new Vector3(X0, Y0, Z0), new Vector3(X0, Y1, Z0), new Vector3(X1, Y1, Z0), new Vector3(X1, Y0, Z0), Vector3.back, seed, edge, 0, rg);
            if (north) FacadeQuad(mb, origin, new Vector3(X1, Y0, Z1), new Vector3(X1, Y1, Z1), new Vector3(X0, Y1, Z1), new Vector3(X0, Y0, Z1), Vector3.forward, seed, edge, 0, rg);
        }

        // ------------------------------------------------------------------ placement along a lane edge
        /// <summary>
        /// Positions every `spacing` metres (measured along the placed line) on the lane edge of s0..s1 at lateral
        /// side x (LaneHalfWidth(s) + offset) (side +1 right of the walk direction, -1 left), y 0. Writes world points and the
        /// path heading (deg) into the lists. The first point sits half a spacing after s0.
        /// </summary>
        public static void AlongEdge(double s0, double s1, int side, double offset, double spacing, List<Vector3> pts, List<float> yaws)
        {
            const double ds = 0.05;
            double acc = 0.5 * spacing;
            Vector3 prev = NocLanes.CorridorToWorld(s0, side * (NocLanes.LaneHalfWidth(s0) + offset), 0.0);
            for (double s = s0 + ds; s <= s1; s += ds)
            {
                Vector3 p = NocLanes.CorridorToWorld(s, side * (NocLanes.LaneHalfWidth(s) + offset), 0.0);
                acc -= (p - prev).magnitude;
                prev = p;
                if (acc > 0.0) continue;
                pts.Add(p);
                yaws?.Add((float)NocLanes.Path.HeadingDeg(Math.Min(s, NocLanes.Path.Length)));
                acc += spacing;
            }
        }
    }
}
