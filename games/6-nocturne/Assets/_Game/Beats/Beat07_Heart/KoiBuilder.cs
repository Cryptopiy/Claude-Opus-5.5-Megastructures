// KoiBuilder: the hologram koi of Heart Square (MASTER_PROMPT 14.5.7 "The koi", 14.7 H6, D-009; KoiMath by V; Look v2:
// TEXTURE_PLAN 6, LOOK_V2 6). One mesh built once at load in PARAMETER space (KoiShaderMath / NocKoi.shader: uv0 = (a, theta),
// uv1 = (radial, forward, env, env'), uv2 = (0 frame part | 1 veil, sv | fin u | veil k | barbel t, fin v | veil half-span,
// part id)):
// - body: 161 rings (a = 900 i / 160 m) x 28 quads (14.5.7: 160 segments x 28, 9.0 k triangles); each ring has 29 vertices, the
//   last a copy of the first at theta = 2 pi, so theta and the scale columns run without a seam; uv2.y = KoiShaderMath.ScaleRow(a);
// - hemispherical head cap (radius rho(0) = 10.56 m, 5 rings and a split pole) and a pointed tail cap (half-ellipsoid
//   rho(L) = 9.6 m wide, 3.5 rho long, inside the veil);
// - the tail veil as KoiMath defines it (D-009, HT: 140 m long from VeilA0 = L - 30, half-span rho(VeilA0) -> 70 m along the axis
//   rolled 30 deg from the vertical toward N, split trailing edge from the lobe tips at VeilA0 + 140 to the notch at 2/3; 12 x 8 cells);
// - D-032 2 (HT): every vertex carries the reference-spine attributes of its station uv0.x (uv3 = (r_ref, alpha, dr_ref/da,
//   dalpha/da), uv4 = (Dy, dDy/da, 0, 0); KoiShaderMath.ShapeUV3 / ShapeUV4), written after ToMesh;
// - two pectoral fins exactly on KoiMath.PectoralPoint (root chord 60 m from a = 108 on the low flank, 120 m along the swept
//   direction; the mesh is a fan inside that (u, v) square: 21 m root, 60 m wide from v 0.75, rounded edge, 8 x 12 cells each)
//   and the dorsal fin on KoiMath.DorsalPoint (90 m from a = 216, height 16 -> 40 -> 12 m, 16 x 4 cells): 14.8 cut 7 is not taken;
// - KO's own frame parts (KoiShaderMath): two pelvic fans (28 m root chord from a = 0.47 L, 60 m long, splayed down and out so they
//   read from below, 6 x 8 cells) and the anal fin (40 m from a = 0.70 L, 10 -> 24 -> 8 m, swept back, 8 x 3 cells);
// - four barbels at the mouth (rigid in the snout's frame): a mouth-corner pair 34 m long (radius 1.3 -> 0.4 m) and an upper-lip
//   pair 20 m long (1.0 -> 0.35 m), curving back and down, 10 x 6 cells each.
// Every frame part is a rigid point in the ring frame of its station (KoiShaderMath.FrameOffset), so the one vertex path of the
// shader places it; POSITION and NORMAL are the koi at the reference pose (u 0, t_w 0) so the mesh validates; the sheet and tube
// normals come from the geometry and each quad is wound to agree with them. NocKoi moves every vertex from the uniforms of
// KoiDriver: no mesh generation after load. Explicit bounds: the whole orbit (x, z within 430 m of S, y 100..500).
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Megastructure
{
    public static class KoiBuilder
    {
        public const int CapRings = 5;
        public const int VeilV = 12, VeilK = 8;
        public const int PectoralU = 8, PectoralV = 12, DorsalU = 16, DorsalV = 4, BarbelT = 10, BarbelSides = 6;
        public const int PelvicU = 6, PelvicV = 8, AnalU = 8, AnalV = 3;
        /// <summary>Tail cap length in units of rho(L) (a pointed half-ellipsoid; the head cap is a hemisphere).</summary>
        public const double TailCapLength = 3.5;
        /// <summary>Barbels: (forward, side, down) root in units of rho(0) except forward (m), length (m), root and tip radius (m).</summary>
        static readonly double[,] s_barbel =
        {
            // fF, fN / rho0, fB / rho0, dir F, dir N, dir B, bend F, bend N, bend B, length, r0, r1
            { 4.0, 0.60, -0.55, -0.50, 0.32, -0.80, -0.40, 0.06, 0.30, 34.0, 1.3, 0.40 },
            { 7.0, 0.45, -0.30, -0.25, 0.60, -0.70, -0.45, 0.00, 0.25, 20.0, 1.0, 0.35 },
        };
        /// <summary>Bounds of every pose: centre (S, y 300), extents 430 x 200 x 430 (orbit R <= 278 + wave 40 + rho 48 + veil).</summary>
        public static readonly Bounds OrbitBounds = new Bounds(new Vector3((float)KoiMath.CentreX, 300f, (float)KoiMath.CentreZ), new Vector3(860f, 400f, 860f));

        static Vector4 s_prm;

        static Vector4 Uv1(double a, double radial, double fwd) =>
            new Vector4((float)radial, (float)fwd, (float)KoiMath.WaveEnvelope(a), (float)KoiShaderMath.EnvelopeDerivative(a));

        static Vector3 Place(Vector2 uv0, Vector4 uv1, Vector4 uv2, out Vector3 n)
        {
            KoiShaderMath.Vertex(uv0, uv1, uv2, s_prm.x, s_prm.y, out Vector3 p, out n);
            return p;
        }

        /// <summary>A vertex with the shader's position at the reference pose; its normal is the shader's unless one is given.</summary>
        static int V(MeshBuilder mb, Vector2 uv0, Vector4 uv1, Vector4 uv2, Vector3? normal = null)
        {
            Vector3 p = Place(uv0, uv1, uv2, out Vector3 n);
            mb.CurrentUV1 = uv1;
            mb.CurrentUV2 = uv2;
            return mb.Vertex(p, normal ?? n, uv0);
        }

        /// <summary>A frame-part vertex from its (F, N, B) offset at station a.</summary>
        static void FramePart(double a, double fF, double fN, double fB, Vector4 uv2, out Vector2 uv0, out Vector4 uv1)
        {
            KoiShaderMath.FrameOffset(fF, fN, fB, out double th, out double radial, out double fwd);
            uv0 = new Vector2((float)a, (float)th);
            uv1 = Uv1(a, radial, fwd);
        }

        /// <summary>A quad (corners in order around it) wound so that its face normal agrees with the mean of its vertex normals.</summary>
        static void QuadOriented(MeshBuilder mb, int a, int b, int c, int d)
        {
            Vector3 w = Vector3.Cross(mb.Positions[b] - mb.Positions[a], mb.Positions[c] - mb.Positions[a]);
            Vector3 m = mb.Normals[a] + mb.Normals[b] + mb.Normals[c] + mb.Normals[d];
            if (Vector3.Dot(w, m) >= 0f) mb.Quad(a, b, c, d); else mb.Quad(a, d, c, b);
        }

        /// <summary>Adds a structured sheet: positions from place(i, j), normals from the grid, quads oriented.</summary>
        static void Sheet(MeshBuilder mb, int nu, int nv, Func<int, int, (Vector2 uv0, Vector4 uv1, Vector4 uv2)> attr)
        {
            var uv0 = new Vector2[nu + 1, nv + 1];
            var uv1 = new Vector4[nu + 1, nv + 1];
            var uv2 = new Vector4[nu + 1, nv + 1];
            var pos = new Vector3[nu + 1, nv + 1];
            for (int i = 0; i <= nu; i++)
                for (int j = 0; j <= nv; j++)
                {
                    var t = attr(i, j);
                    uv0[i, j] = t.uv0; uv1[i, j] = t.uv1; uv2[i, j] = t.uv2;
                    pos[i, j] = Place(t.uv0, t.uv1, t.uv2, out _);
                }
            int first = mb.VertexCount;
            for (int i = 0; i <= nu; i++)
                for (int j = 0; j <= nv; j++)
                {
                    Vector3 pu = pos[Math.Min(i + 1, nu), j] - pos[Math.Max(i - 1, 0), j];
                    Vector3 pv = pos[i, Math.Min(j + 1, nv)] - pos[i, Math.Max(j - 1, 0)];
                    Vector3 n = Vector3.Cross(pv, pu);
                    V(mb, uv0[i, j], uv1[i, j], uv2[i, j], n.sqrMagnitude > 1e-20f ? n.normalized : Vector3.up);
                }
            int row = nv + 1;
            for (int i = 0; i < nu; i++)
                for (int j = 0; j < nv; j++)
                {
                    int q = first + i * row + j;
                    QuadOriented(mb, q, q + 1, q + 1 + row, q + row);
                }
        }

        /// <summary>The koi mesh data at the reference pose (head azimuth 6.64 deg, wave time 0).</summary>
        public static MeshBuilder Body()
        {
            var mb = new MeshBuilder { WriteUV1 = true, WriteUV2 = true };
            double phiH = KoiMath.HeadAzimuth(0.0), L = KoiMath.Length;
            s_prm = KoiShaderMath.Params(phiH, 0.0);
            int nr = KoiMath.RingVertices, ns = KoiMath.BodySegments, rv = nr + 1;
            double dth = 2.0 * Math.PI / nr;
            // body rings 0..160, 29 vertices each (theta 0 .. 2 pi)
            int body = mb.VertexCount;
            for (int i = 0; i <= ns; i++)
            {
                double a = L * i / ns, rho = KoiMath.Radius(a);
                var uv2 = new Vector4(KoiShaderMath.PartBody, (float)KoiShaderMath.ScaleRow(a), 0f, KoiShaderMath.PartBody);
                for (int j = 0; j <= nr; j++) V(mb, new Vector2((float)a, (float)(j * dth)), Uv1(a, rho, 0.0), uv2);
            }
            for (int i = 0; i < ns; i++)
                for (int j = 0; j < nr; j++)
                {
                    int a0 = body + i * rv + j, a1 = a0 + 1;
                    MeshKit.Quad(mb, a0, a1, a1 + rv, a0 + rv);
                }
            // caps: head (forward +) at a = 0 and tail (forward -) at a = L; the pole is split per sector so theta stays continuous
            for (int cap = 0; cap < 2; cap++)
            {
                // the tail cap is a half-ellipsoid 3.5 rho long, so the peduncle narrows to a point inside the veil
                double a = cap == 0 ? 0.0 : L, rho = KoiMath.Radius(a), sg = cap == 0 ? 1.0 : -1.0, len = cap == 0 ? rho : TailCapLength * rho;
                int prev = body + (cap == 0 ? 0 : ns * rv);
                for (int r = 1; r <= CapRings; r++)
                {
                    double al = 0.5 * Math.PI * r / (CapRings + 1), fwd = sg * len * Math.Sin(al);
                    var uv2 = new Vector4(KoiShaderMath.PartBody, (float)KoiShaderMath.ScaleRow(a - fwd), 0f, KoiShaderMath.PartBody);
                    int ring = mb.VertexCount;
                    for (int j = 0; j <= nr; j++) V(mb, new Vector2((float)a, (float)(j * dth)), Uv1(a, rho * Math.Cos(al), fwd), uv2);
                    for (int j = 0; j < nr; j++) MeshKit.Quad(mb, prev + j, prev + j + 1, ring + j + 1, ring + j);
                    prev = ring;
                }
                var uvp = new Vector4(KoiShaderMath.PartBody, (float)KoiShaderMath.ScaleRow(a - sg * len), 0f, KoiShaderMath.PartBody);
                int pole = mb.VertexCount;
                for (int j = 0; j < nr; j++) V(mb, new Vector2((float)a, (float)((j + 0.5) * dth)), Uv1(a, 0.0, sg * len), uvp);
                for (int j = 0; j < nr; j++) MeshKit.Tri(mb, prev + j, prev + j + 1, pole + j);
            }
            // veil (KoiMath.VeilPoint): k from -1 (lower lobe) to +1 (upper lobe); a = VeilA(v), rooted 30 m before the tail tip
            // (VeilA0 = L - 30), v in [0, vmax(k)], vmax = (notch + (length - notch) |k|) / length (split edge: lobe tips at v 1,
            // notch at v 2/3); half-span lerp(rho(VeilA0), 70, v)
            int veil = mb.VertexCount;
            for (int ik = 0; ik <= VeilK; ik++)
            {
                double k = -1.0 + 2.0 * ik / VeilK;
                double vmax = (KoiMath.VeilNotch + (KoiMath.VeilLength - KoiMath.VeilNotch) * Math.Abs(k)) / KoiMath.VeilLength;
                for (int iv = 0; iv <= VeilV; iv++)
                {
                    double v = vmax * iv / VeilV, a = KoiMath.VeilA(v);
                    double half = NocMath.Lerp(KoiMath.Radius(KoiMath.VeilA0), KoiMath.VeilHalfSpan, v);
                    V(mb, new Vector2((float)a, 0f), Uv1(a, 0.0, 0.0), new Vector4(KoiShaderMath.PartVeil, (float)k, (float)half, 0f));
                }
            }
            int row = VeilV + 1;
            for (int ik = 0; ik < VeilK; ik++)
                for (int iv = 0; iv < VeilV; iv++)
                {
                    int q = veil + ik * row + iv;
                    MeshKit.Quad(mb, q, q + 1, q + 1 + row, q + row);
                }
            // pectoral fins (KoiMath.PectoralPoint): root at the low flank (55 deg below the side axis), 120 m along
            // d = normalize(-0.80 F +- 0.12 N - 0.55 B); the mesh is the fan of KoiShaderMath.FanUV inside KoiMath's (u, v) square
            // (a 21 m root widening to the 60 m chord, rounded edge v = 1 - 0.3 s^2), so the fin stays inside the sampled corners
            double th55 = KoiMath.PectoralRootTheta * (Math.PI / 180.0);
            double dl = Math.Sqrt(KoiMath.PectoralDirF * KoiMath.PectoralDirF + KoiMath.PectoralDirN * KoiMath.PectoralDirN + KoiMath.PectoralDirB * KoiMath.PectoralDirB);
            for (int side = 1; side >= -1; side -= 2)
            {
                double sg = side;
                float part = side > 0 ? KoiShaderMath.PartPectoralOuter : KoiShaderMath.PartPectoralInner;
                Sheet(mb, PectoralU, PectoralV, (i, j) =>
                {
                    KoiShaderMath.FanUV((double)i / PectoralU, (double)j / PectoralV, out double u, out double v);
                    double a = KoiMath.PectoralA0 + KoiMath.PectoralChord * u, rho = KoiMath.Radius(a), s = KoiMath.PectoralLength * v / dl;
                    double fF = KoiMath.PectoralDirF * s;
                    double fN = sg * rho * Math.Cos(th55) + sg * KoiMath.PectoralDirN * s;
                    double fB = -rho * Math.Sin(th55) + KoiMath.PectoralDirB * s;
                    var uv2 = new Vector4(KoiShaderMath.PartBody, (float)u, (float)v, part);
                    FramePart(a, fF, fN, fB, uv2, out Vector2 uv0, out Vector4 uv1);
                    return (uv0, uv1, uv2);
                });
            }
            // dorsal fin (KoiMath.DorsalPoint): along the top (theta 90) from a = 216 for 90 m, height 16 -> 40 (25 %) -> 12 m
            Sheet(mb, DorsalU, DorsalV, (i, j) =>
            {
                double u = (double)i / DorsalU, v = (double)j / DorsalV;
                double a = KoiMath.DorsalA0 + KoiMath.DorsalLength * u;
                double fB = KoiMath.Radius(a) + KoiMath.DorsalHeight(u) * v;
                var uv2 = new Vector4(KoiShaderMath.PartBody, (float)u, (float)v, KoiShaderMath.PartDorsal);
                FramePart(a, 0.0, 0.0, fB, uv2, out Vector2 uv0, out Vector4 uv1);
                return (uv0, uv1, uv2);
            });
            // pelvic fins (KO's frame parts, KoiShaderMath.PelvicOffset): fans from the belly at a = 0.47 L, splayed down and out
            for (int side = 1; side >= -1; side -= 2)
            {
                int sd = side;
                float part = side > 0 ? KoiShaderMath.PartPelvicOuter : KoiShaderMath.PartPelvicInner;
                Sheet(mb, PelvicU, PelvicV, (i, j) =>
                {
                    KoiShaderMath.FanUV((double)i / PelvicU, (double)j / PelvicV, out double u, out double v);
                    KoiShaderMath.PelvicOffset(sd, u, v, out double a, out double fF, out double fN, out double fB);
                    var uv2 = new Vector4(KoiShaderMath.PartBody, (float)u, (float)v, part);
                    FramePart(a, fF, fN, fB, uv2, out Vector2 uv0, out Vector4 uv1);
                    return (uv0, uv1, uv2);
                });
            }
            // anal fin (KoiShaderMath.AnalOffset): on the belly from a = 0.70 L, swept back
            Sheet(mb, AnalU, AnalV, (i, j) =>
            {
                double u = (double)i / AnalU, v = (double)j / AnalV;
                KoiShaderMath.AnalOffset(u, v, out double a, out double fF, out double fN, out double fB);
                var uv2 = new Vector4(KoiShaderMath.PartBody, (float)u, (float)v, KoiShaderMath.PartAnal);
                FramePart(a, fF, fN, fB, uv2, out Vector2 uv0, out Vector4 uv1);
                return (uv0, uv1, uv2);
            });
            // barbels: tubes rigid in the snout's frame (station a = 0), curving back and down from the mouth
            double rho0 = KoiMath.Radius(0.0);
            for (int pair = 0; pair < 2; pair++)
                for (int side = 1; side >= -1; side -= 2)
                    Barbel(mb, pair, side, rho0);
            return mb;
        }

        static Vector3 BarbelCentre(int pair, double sg, double rho0, double t)
        {
            double fF = s_barbel[pair, 0], fN = sg * s_barbel[pair, 1] * rho0, fB = s_barbel[pair, 2] * rho0;
            var d = new Vector3((float)s_barbel[pair, 3], (float)(sg * s_barbel[pair, 4]), (float)s_barbel[pair, 5]).normalized;
            var bend = new Vector3((float)s_barbel[pair, 6], (float)(sg * s_barbel[pair, 7]), (float)s_barbel[pair, 8]);
            double len = s_barbel[pair, 9];
            Vector3 off = (float)(len * t) * d + (float)(0.5 * len * t * t) * bend;
            return new Vector3((float)fF, (float)fN, (float)fB) + off;          // (F, N, B) components
        }

        static void Barbel(MeshBuilder mb, int pair, int side, double rho0)
        {
            double sg = side;
            int first = mb.VertexCount;
            for (int i = 0; i <= BarbelT; i++)
            {
                double t = (double)i / BarbelT;
                Vector3 c = BarbelCentre(pair, sg, rho0, t);
                Vector3 tan = (BarbelCentre(pair, sg, rho0, Math.Min(1.0, t + 0.01)) - BarbelCentre(pair, sg, rho0, Math.Max(0.0, t - 0.01))).normalized;
                Vector3 e1 = Vector3.Cross(tan, new Vector3(0f, 0f, 1f));
                if (e1.sqrMagnitude < 1e-6f) e1 = Vector3.Cross(tan, new Vector3(0f, 1f, 0f));
                e1.Normalize();
                Vector3 e2 = Vector3.Cross(tan, e1);
                double r = NocMath.Lerp(s_barbel[pair, 10], s_barbel[pair, 11], t);
                var uv2 = new Vector4(KoiShaderMath.PartBody, (float)t, 0f, KoiShaderMath.PartBarbel);
                FramePart(0.0, c.x, c.y, c.z, uv2, out Vector2 uc0, out Vector4 uc1);
                Vector3 pc = Place(uc0, uc1, uv2, out _);
                for (int k = 0; k < BarbelSides; k++)
                {
                    double phi = 2.0 * Math.PI * (k + 0.5) / BarbelSides;
                    Vector3 o = c + (float)(r * Math.Cos(phi)) * e1 + (float)(r * Math.Sin(phi)) * e2;
                    FramePart(0.0, o.x, o.y, o.z, uv2, out Vector2 uv0, out Vector4 uv1);
                    Vector3 p = Place(uv0, uv1, uv2, out _);
                    Vector3 n = p - pc;
                    V(mb, uv0, uv1, uv2, n.sqrMagnitude > 1e-20f ? n.normalized : Vector3.up);
                }
            }
            for (int i = 0; i < BarbelT; i++)
                for (int k = 0; k < BarbelSides; k++)
                {
                    int a0 = first + i * BarbelSides + k, a1 = first + i * BarbelSides + (k + 1) % BarbelSides;
                    QuadOriented(mb, a0, a1, a1 + BarbelSides, a0 + BarbelSides);
                }
        }

        /// <summary>The reference-spine attributes (uv3, uv4) of every vertex: functions of its station uv0.x only.</summary>
        public static void ShapeAttributes(MeshBuilder mb, List<Vector4> uv3, List<Vector4> uv4)
        {
            uv3.Clear(); uv4.Clear();
            for (int i = 0; i < mb.VertexCount; i++)
            {
                double a = mb.UV0[i].x;
                uv3.Add(KoiShaderMath.ShapeUV3(a));
                uv4.Add(KoiShaderMath.ShapeUV4(a));
            }
        }

        public static Mesh BuildMesh()
        {
            MeshBuilder mb = Body();
            Mesh mesh = mb.ToMesh("Beat07_Koi");
            var uv3 = new List<Vector4>(mb.VertexCount);
            var uv4 = new List<Vector4>(mb.VertexCount);
            ShapeAttributes(mb, uv3, uv4);
            mesh.SetUVs(3, uv3);
            mesh.SetUVs(4, uv4);
            mesh.bounds = OrbitBounds;
            G4Kit.Check(mesh);
            return mesh;
        }

        public static KoiDriver Build(WorldContext ctx, Transform root)
        {
            Material mat = G4Kit.FromShader("Megastructure/NocKoi", "Beat07_Koi");
            if (mat == null) return null;
            mat.SetFloat("_BodyGain", NocturnePalette.EmissionKoiBody);
            mat.SetFloat("_RimGain", NocturnePalette.EmissionKoiRim);
            mat.SetFloat("_FrontBudget", KoiShaderMath.FrontBudget);
            mat.SetFloat("_BackBudget", KoiShaderMath.BackBudget);
            mat.SetFloat("_BackWeight", KoiShaderMath.BackWeight);
            mat.SetFloat("_FinGain", KoiShaderMath.FinGain);
            mat.SetFloat("_FinBudget", KoiShaderMath.FinBudget);
            mat.SetFloat("_VeilGain", KoiShaderMath.VeilGain);
            mat.SetFloat("_VeilBudget", KoiShaderMath.VeilBudget);
            mat.SetFloat("_CoreClear", KoiShaderMath.CoreClear);      // fix wave C02: transmission and structure
            mat.SetFloat("_GhostGain", KoiShaderMath.GhostGain);
            mat.SetFloat("_GapDepth", KoiShaderMath.GapDepth);
            Mesh mesh = BuildMesh();
            var mr = ctx.AddMesh(root, "koi", mesh, mat);
            mr.allowOcclusionWhenDynamic = false;
            ctx.Log(FormattableString.Invariant($"Beat07 koi: {mesh.vertexCount} vertices, {WorldContext.TriangleCount(mesh)} triangles (body 8 960, caps, veil, pectoral / pelvic / dorsal / anal fins, barbels; 14.5.7 budget 30 k), bounds {OrbitBounds.size}"));
            var driver = new KoiDriver(mat);
            ctx.Register(driver);
            return driver;
        }
    }
}
