// KrakenTentacleMesh: the very long procedural tentacles (Addendum A1, kraken owner). One tube per limb and LOD, built once at
// load in the limb's REST space (the shader bends it along the animated chain, BwCreatureSkin.hlsl).
//   * profile: KrakenLimb.RadiusAt (the Tripo arm's radius at the cut, then the A1 1.2 power law r0 (1 - x)^1.43 to 0.3 m;
//     the feeding tentacles end in a swollen club);
//   * skin: the unwrapped Tripo tentacle tile at (theta, v(u)) (KrakenMath.TileV, suckers shrink with the radius), the same
//     tile the arms blend into over their last 40 m, so the joint carries the same skin;
//   * LOD0: the tile's relief displaces the surface (sucker rows and barnacle crust as real geometry), 40 sides, rings every
//     0.45 r (min 0.5 m); LOD1: 16 sides, smooth, rings every 1.3 r (min 2.5 m); LOD2: 8 sides (min 9 m), the silhouette;
//   * the arm tubes start 14 m inside the arm (radius 0.97, ramping to 1 at the cut + 3 m): no crack, no co-planar overlap.
// Vertex layout as the body meshes: POSITION rest (local m), NORMAL, TANGENT, COLOR (limb, w 1, AO, radius), UV0 (theta01,
// -v tiles), UV1 (u, theta).
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Megastructure.Creature
{
    public static class KrakenTentacleMesh
    {
        public const int Lods = 3;
        public static readonly int[] Sides = { 40, 16, 8 };
        static readonly float[] Spacing = { 0.45f, 1.3f, 3.6f };
        static readonly float[] MinSpacing = { 0.5f, 2.5f, 9f };
        public const float StartInside = 14f;       // inside the arm's circular 35 m morph zone (covers the LOD2/3 rim simplification)
        public const float ReliefScale = 0.7f;
        static readonly int[] KelpSides = { 10, 6, 4 };
        static readonly float[] KelpSpacing = { 0.6f, 2.5f, 8f };

        /// <summary>Arc where the tube of a limb begins (inside the arm for the Tripo arms).</summary>
        public static float StartU(KrakenLimb L) => L.Feeding ? 0f : Mathf.Max(0f, L.U0 - StartInside);

        public static Mesh Build(KrakenLimb L, int lod, KrakenAssets assets, float tileRadii)
        {
            int sides = L.IsKelp ? KelpSides[lod] : Sides[lod];
            var rings = new List<float>(2048);
            float u = StartU(L);
            float end = L.Length;
            while (u < end - 0.02f)
            {
                rings.Add(u);
                float r = L.RadiusAt(u);
                u += L.IsKelp ? KelpSpacing[lod] : Mathf.Max(Spacing[lod] * r, MinSpacing[lod]);
            }
            rings.Add(end - 0.02f);
            int nr = rings.Count, nc = sides + 1;
            int vc = nr * nc + 1;
            var pos = new Vector3[vc];
            var radial = new Vector3[nr * nc];
            var uv0 = new Vector2[vc];
            var uv1 = new Vector2[vc];
            var col = new Color[vc];
            float u0 = L.Feeding ? 0f : L.U0;
            for (int j = 0; j < nr; j++)
            {
                float uj = rings[j];
                Vector3 c = L.RestAt(uj);
                Quaternion q = L.RestRotAt(uj);
                float r = L.RadiusAt(uj);
                if (!L.Feeding) r *= Mathf.Lerp(0.97f, 1f, KrakenMath.Smooth(L.U0 - StartInside, L.U0 + 3f, uj));
                float v = L.IsKelp ? uj / 30f : (float)KrakenMath.TileV(uj, u0, L.R0, L.Span, tileRadii);
                for (int i = 0; i < nc; i++)
                {
                    float th01 = i / (float)sides;
                    float th = (th01 - 0.5f) * 2f * Mathf.PI;
                    Vector3 d = q * new Vector3(Mathf.Sin(th), -Mathf.Cos(th) * (L.IsKelp ? 0.12f : 1f), 0f);   // kelp: a flat ribbon
                    float h = 0f;
                    if (lod == 0 && assets != null && !L.IsKelp) h = assets.ReliefAt(th01, v) * ReliefScale * KrakenMath.Smooth(L.U0 - 2f, L.U0 + 8f, uj + (L.Feeding ? 10f : 0f));
                    int id = j * nc + i;
                    radial[id] = d;
                    pos[id] = c + d * (r * (1f + h));
                    uv0[id] = new Vector2(th01, -v);
                    uv1[id] = new Vector2(uj, th);
                    col[id] = new Color(L.Index, 1f, 1f, r);
                }
            }
            // tip vertex
            int tip = nr * nc;
            {
                Vector3 c = L.RestAt(end);
                Quaternion q = L.RestRotAt(end);
                pos[tip] = c + q * Vector3.forward * (0.6f * L.RadiusAt(end - 0.5f) + 0.05f);
                uv0[tip] = new Vector2(0.5f, -(float)KrakenMath.TileV(end, u0, L.R0, L.Span, tileRadii));
                uv1[tip] = new Vector2(end, 0f);
                col[tip] = new Color(L.Index, 1f, 1f, 0.05f);
            }
            // normals and tangents from the surface grid (central differences, wrapping around)
            var nrm = new Vector3[vc];
            var tan = new Vector4[vc];
            for (int j = 0; j < nr; j++)
            {
                int jm = Mathf.Max(j - 1, 0), jp = Mathf.Min(j + 1, nr - 1);
                for (int i = 0; i < nc; i++)
                {
                    int im = i == 0 ? sides - 1 : i - 1, ip = i == sides ? 1 : i + 1;
                    int id = j * nc + i;
                    Vector3 dth = pos[j * nc + ip] - pos[j * nc + im];
                    Vector3 du = pos[jp * nc + i] - pos[jm * nc + i];
                    Vector3 n = Vector3.Cross(dth, du);
                    if (Vector3.Dot(n, radial[id]) < 0f) n = -n;
                    n = n.sqrMagnitude > 1e-14f ? n.normalized : radial[id];
                    // relief normals bend at most 75 deg away from the radial direction (no inverted shading at steep cups)
                    if (Vector3.Dot(n, radial[id]) < 0.26f) n = Vector3.Slerp(radial[id], n, 0.7f).normalized;
                    nrm[id] = n;
                    Vector3 t = dth - n * Vector3.Dot(dth, n);
                    t = t.sqrMagnitude > 1e-14f ? t.normalized : Vector3.Cross(n, du).normalized;
                    // bitangent sign: cross(n, t) * w must point along +v of the uv (v = -tile s, i.e. towards the root)
                    float w = Vector3.Dot(Vector3.Cross(n, t), -du) >= 0f ? 1f : -1f;
                    tan[id] = new Vector4(t.x, t.y, t.z, w);
                }
            }
            {
                Quaternion q = L.RestRotAt(end);
                nrm[tip] = q * Vector3.forward;
                Vector3 t = q * Vector3.right;
                tan[tip] = new Vector4(t.x, t.y, t.z, 1f);
            }
            // indices
            var idx = new int[(nr - 1) * sides * 6 + sides * 3];
            int o = 0;
            for (int j = 0; j < nr - 1; j++)
                for (int i = 0; i < sides; i++)
                {
                    int a = j * nc + i, b = a + 1, c = a + nc, d = c + 1;
                    // outward faces (Unity: clockwise seen from outside); checked by KrakenTests.Tubes_AreClosedAndOutward
                    idx[o++] = a; idx[o++] = c; idx[o++] = b;
                    idx[o++] = b; idx[o++] = c; idx[o++] = d;
                }
            int last = (nr - 1) * nc;
            for (int i = 0; i < sides; i++) { idx[o++] = last + i; idx[o++] = tip; idx[o++] = last + i + 1; }
            var m = new Mesh { name = "kraken_tentacle_" + L.Index + "_lod" + lod, indexFormat = vc > 65000 ? IndexFormat.UInt32 : IndexFormat.UInt16 };
            m.SetVertices(pos);
            m.SetNormals(nrm);
            m.SetTangents(tan);
            m.SetColors(col);
            m.SetUVs(0, uv0);
            m.SetUVs(1, uv1);
            m.SetTriangles(idx, 0, false);
            m.RecalculateBounds();
            FixWinding(m, idx, pos, nrm);
            return m;
        }

        /// <summary>Makes every triangle face along its vertex normals (one consistent flip if the generator's order is inward).</summary>
        static void FixWinding(Mesh m, int[] idx, Vector3[] pos, Vector3[] nrm)
        {
            int agree = 0, total = 0;
            for (int t = 0; t < idx.Length; t += 3 * 37)
            {
                Vector3 a = pos[idx[t]], b = pos[idx[t + 1]], c = pos[idx[t + 2]];
                Vector3 fn = Vector3.Cross(b - a, c - a);
                if (fn.sqrMagnitude < 1e-16f) continue;
                total++;
                if (Vector3.Dot(fn, nrm[idx[t]] + nrm[idx[t + 1]] + nrm[idx[t + 2]]) > 0f) agree++;
            }
            if (total > 0 && agree * 2 < total)
            {
                for (int t = 0; t < idx.Length; t += 3) { int s = idx[t + 1]; idx[t + 1] = idx[t + 2]; idx[t + 2] = s; }
                m.SetTriangles(idx, 0, false);
            }
        }

        /// <summary>The beak: two hooked chitin jaws at the mouth (kraken local, the crown is the origin), rigid (limb 255).
        /// Upper jaw 26 m, lower jaw 21 m, flattened sideways, hooked towards each other. uv0.y = 0 at the base .. 1 at the
        /// worn tip (BwCreature _KrTube 2 = chitin).</summary>
        public static Mesh Beak()
        {
            const int segs = 28, sides = 18;
            var pos = new List<Vector3>(); var nrm = new List<Vector3>(); var uv0 = new List<Vector2>(); var col = new List<Color>();
            var idx = new List<int>();
            for (int jaw = 0; jaw < 2; jaw++)
            {
                float len = jaw == 0 ? 26f : 21f, rad = jaw == 0 ? 8.5f : 7f;
                float sgn = jaw == 0 ? 1f : -1f;
                Vector3 c = new Vector3(0f, 3f, 2.5f * sgn);
                float phi = 0f;
                int baseV = pos.Count;
                float step = len / segs;
                for (int j = 0; j <= segs; j++)
                {
                    float t = j / (float)segs;
                    // tangent: starts straight down, hooks towards the other jaw (curl 1.9 rad over the length)
                    phi = 1.9f * t * t;
                    Vector3 T = new Vector3(0f, -Mathf.Cos(phi), -sgn * Mathf.Sin(phi));
                    Vector3 X = Vector3.right;
                    Vector3 Y = Vector3.Cross(T, X).normalized;
                    float r = rad * Mathf.Pow(1f - 0.97f * t, 0.85f);
                    for (int i = 0; i <= sides; i++)
                    {
                        float a = 2f * Mathf.PI * i / sides;
                        Vector3 d = X * (Mathf.Cos(a) * 0.62f) + Y * Mathf.Sin(a);
                        pos.Add(c + d * r);
                        nrm.Add((X * (Mathf.Cos(a) / 0.62f) + Y * Mathf.Sin(a)).normalized);
                        uv0.Add(new Vector2(i / (float)sides, t));
                        col.Add(new Color(255f, 0f, 1f, r));
                    }
                    c += T * step;
                }
                int tip = pos.Count;
                pos.Add(c); nrm.Add(new Vector3(0f, -Mathf.Cos(phi), -sgn * Mathf.Sin(phi))); uv0.Add(new Vector2(0.5f, 1f)); col.Add(new Color(255f, 0f, 1f, 0.1f));
                for (int j = 0; j < segs; j++)
                    for (int i = 0; i < sides; i++)
                    {
                        int a = baseV + j * (sides + 1) + i, b = a + 1, cc = a + sides + 1, d = cc + 1;
                        idx.Add(a); idx.Add(cc); idx.Add(b); idx.Add(b); idx.Add(cc); idx.Add(d);
                    }
                int last = baseV + segs * (sides + 1);
                for (int i = 0; i < sides; i++) { idx.Add(last + i); idx.Add(tip); idx.Add(last + i + 1); }
            }
            var m = new Mesh { name = "kraken_beak" };
            m.SetVertices(pos); m.SetNormals(nrm); m.SetUVs(0, uv0); m.SetUVs(1, uv0); m.SetColors(col);
            var ia = idx.ToArray();
            m.SetTriangles(ia, 0, false);
            m.RecalculateTangents();
            m.RecalculateBounds();
            FixWinding(m, ia, pos.ToArray(), nrm.ToArray());
            return m;
        }

        /// <summary>A UV sphere for an eyeball: radius 1, +Z = the pupil axis; uv0 = local (x, y), uv1 = (local z, 0).</summary>
        public static Mesh EyeSphere(int seg = 48, int rings = 32)
        {
            int vc = (seg + 1) * (rings + 1);
            var pos = new Vector3[vc]; var nrm = new Vector3[vc]; var tan = new Vector4[vc];
            var uv0 = new Vector2[vc]; var uv1 = new Vector2[vc];
            for (int j = 0; j <= rings; j++)
            {
                float th = Mathf.PI * j / rings;          // 0 at +Z (pupil), pi at -Z
                for (int i = 0; i <= seg; i++)
                {
                    float ph = 2f * Mathf.PI * i / seg;
                    var p = new Vector3(Mathf.Sin(th) * Mathf.Cos(ph), Mathf.Sin(th) * Mathf.Sin(ph), Mathf.Cos(th));
                    int id = j * (seg + 1) + i;
                    pos[id] = p; nrm[id] = p;
                    var t = new Vector3(-Mathf.Sin(ph), Mathf.Cos(ph), 0f);
                    if (t.sqrMagnitude < 1e-6f) t = Vector3.right;
                    tan[id] = new Vector4(t.x, t.y, t.z, 1f);
                    uv0[id] = new Vector2(p.x, p.y); uv1[id] = new Vector2(p.z, 0f);
                }
            }
            var idx = new int[seg * rings * 6];
            int o = 0;
            for (int j = 0; j < rings; j++)
                for (int i = 0; i < seg; i++)
                {
                    int a = j * (seg + 1) + i, b = a + 1, c = a + seg + 1, d = c + 1;
                    idx[o++] = a; idx[o++] = c; idx[o++] = b; idx[o++] = b; idx[o++] = c; idx[o++] = d;
                }
            var col = new Color[vc];
            for (int i = 0; i < vc; i++) col[i] = new Color(255f, 0f, 1f, 1f);   // limb 255 = rigid (no skinning, no thin scattering)
            var m = new Mesh { name = "kraken_eye" };
            m.SetVertices(pos); m.SetNormals(nrm); m.SetTangents(tan); m.SetUVs(0, uv0); m.SetUVs(1, uv1); m.SetColors(col);
            m.SetTriangles(idx, 0, false);
            m.RecalculateBounds();
            FixWinding(m, idx, pos, nrm);
            return m;
        }
    }
}
