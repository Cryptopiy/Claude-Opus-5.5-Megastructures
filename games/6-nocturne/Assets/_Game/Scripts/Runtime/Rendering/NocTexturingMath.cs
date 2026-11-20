// NocTexturingMath: the double-precision C# twin of Shaders/Include/NocTexturing.hlsl and of NocFaceFrame (NocLitCore.hlsl)
// (docs/TEXTURE_PLAN.md 1.6, 2.5; owner TP). NocTexturingTests check the library through it: hex weights sum to 1 and are
// continuous, module and panel seams fall inside the joints, triplanar and UV0 surface-gradient normals match the analytic
// gradient, the facade frame sign sigma on faces of both orientations and on tilted normals, the float32 uv error at 2 km.
// Same constants (NocTexConstants), same hashes (NocHash.Pcg3d), same arithmetic order where it matters. No UnityEngine dependency.
using System;

namespace Megastructure
{
    public static class NocTexturingMath
    {
        /// <summary>A double 3-vector (no UnityEngine dependency).</summary>
        public struct V3d
        {
            public double x, y, z;
            public V3d(double x, double y, double z) { this.x = x; this.y = y; this.z = z; }
            public static V3d operator +(V3d a, V3d b) => new V3d(a.x + b.x, a.y + b.y, a.z + b.z);
            public static V3d operator -(V3d a, V3d b) => new V3d(a.x - b.x, a.y - b.y, a.z - b.z);
            public static V3d operator -(V3d a) => new V3d(-a.x, -a.y, -a.z);
            public static V3d operator *(V3d a, double s) => new V3d(a.x * s, a.y * s, a.z * s);
            public static V3d operator *(double s, V3d a) => new V3d(a.x * s, a.y * s, a.z * s);
            public double this[int k] => k == 0 ? x : (k == 1 ? y : z);
            public override string ToString() => FormattableString.Invariant($"({x:0.#####}, {y:0.#####}, {z:0.#####})");
        }

        public static double Dot(V3d a, V3d b) => a.x * b.x + a.y * b.y + a.z * b.z;
        public static V3d Cross(V3d a, V3d b) => new V3d(a.y * b.z - a.z * b.y, a.z * b.x - a.x * b.z, a.x * b.y - a.y * b.x);
        public static V3d Normalize(V3d a) { double l = Math.Sqrt(Dot(a, a)); return l > 1e-300 ? a * (1.0 / l) : a; }
        public static double AngleDeg(V3d a, V3d b) => Math.Acos(Math.Max(-1.0, Math.Min(1.0, Dot(Normalize(a), Normalize(b))))) * 180.0 / Math.PI;

        static double Smoothstep(double e0, double e1, double x) { double t = Math.Max(0.0, Math.Min(1.0, (x - e0) / (e1 - e0))); return t * t * (3.0 - 2.0 * t); }
        static uint H(int a, int b, uint c, int k) => NocHash.H3(unchecked((uint)a), unchecked((uint)b), c, k);

        // ================================================================================================= footprint and fade

        public static double FootprintBox(double dxu, double dxv, double dyu, double dyv) => Math.Max(Math.Abs(dxu) + Math.Abs(dyu), Math.Abs(dxv) + Math.Abs(dyv));
        /// <summary>w_tex = smoothstep(2, 4, tile / fp_box).</summary>
        public static double TexFade(double tile, double fpBox) => Smoothstep(NocTexConstants.TexFadePx0, NocTexConstants.TexFadePx1, tile / Math.Max(fpBox, 1e-7));

        // ================================================================================================= hex tiling

        /// <summary>The three vertices and their continuous weights (w^gamma, normalised, cut, renormalised) at st = uv x rate.</summary>
        public struct HexTaps
        {
            public int v1x, v1y, v2x, v2y, v3x, v3y;
            public double w1, w2, w3;
        }

        /// <summary>Barycentric weights and vertex ids of the triangle grid (NocHexGrid).</summary>
        public static void HexGrid(double stx, double sty, out double b1, out double b2, out double b3, out int v1x, out int v1y, out int v2x, out int v2y, out int v3x, out int v3y)
        {
            double skx = stx - 0.57735027 * sty, sky = 1.15470054 * sty;
            int bx = (int)Math.Floor(skx), by = (int)Math.Floor(sky);
            double fx = skx - Math.Floor(skx), fy = sky - Math.Floor(sky);
            double z = 1.0 - fx - fy;
            double s = z < 0.0 ? 1.0 : 0.0, s2 = 2.0 * s - 1.0;
            b1 = -z * s2; b2 = s - fy * s2; b3 = s - fx * s2;
            int si = (int)s;
            v1x = bx + si; v1y = by + si;
            v2x = bx + si; v2y = by + 1 - si;
            v3x = bx + 1 - si; v3y = by + si;
        }

        public static HexTaps HexWeights(double u, double v, double rate)
        {
            HexGrid(u * rate, v * rate, out double b1, out double b2, out double b3, out int v1x, out int v1y, out int v2x, out int v2y, out int v3x, out int v3y);
            double g = NocTexConstants.TexHexGamma, cut = NocTexConstants.TexHexCut;
            double w1 = Math.Pow(Math.Max(b1, 0), g), w2 = Math.Pow(Math.Max(b2, 0), g), w3 = Math.Pow(Math.Max(b3, 0), g);
            double sw = Math.Max(w1 + w2 + w3, 1e-7);
            w1 = Math.Max(w1 / sw - cut, 0); w2 = Math.Max(w2 / sw - cut, 0); w3 = Math.Max(w3 / sw - cut, 0);
            sw = Math.Max(w1 + w2 + w3, 1e-7);
            return new HexTaps { v1x = v1x, v1y = v1y, v2x = v2x, v2y = v2y, v3x = v3x, v3y = v3y, w1 = w1 / sw, w2 = w2 / sw, w3 = w3 / sw };
        }

        /// <summary>Weight of lattice vertex (vx, vy) at (u, v): 0 when it is not one of the three taps (the continuity test keys by vertex).</summary>
        public static double HexWeightOf(HexTaps t, int vx, int vy)
        {
            double w = 0;
            if (t.v1x == vx && t.v1y == vy) w += t.w1;
            if (t.v2x == vx && t.v2y == vy) w += t.w2;
            if (t.v3x == vx && t.v3y == vy) w += t.w3;
            return w;
        }

        /// <summary>Centre of a lattice vertex in tiles.</summary>
        public static void HexCentre(int vx, int vy, double rate, out double cx, out double cy) { cx = (vx + 0.5 * vy) / rate; cy = 0.86602540 * vy / rate; }

        /// <summary>Per-vertex offset (tiles) and rotation (cos, sin of the angle) of NocHexVertex.</summary>
        public static void HexVertex(int vx, int vy, uint seed, double rotate, out double ox, out double oy, out double cs, out double sn)
        {
            uint a = unchecked((uint)vx), b = unchecked((uint)vy), c = seed;
            NocHash.Pcg3d(ref a, ref b, ref c);
            ox = NocHash.U01(a); oy = NocHash.U01(b);
            double ang = (2.0 * NocHash.U01(c) - 1.0) * Math.PI * rotate;
            cs = Math.Cos(ang); sn = Math.Sin(ang);
        }

        /// <summary>Tap coordinate of vertex (vx, vy) for uv: mul(uv - c, R) + c + offset, R = (cs, -sn; sn, cs).</summary>
        public static void HexTapUV(double u, double v, int vx, int vy, double rate, double rotate, uint seed, out double tu, out double tv)
        {
            HexCentre(vx, vy, rate, out double cx, out double cy);
            HexVertex(vx, vy, seed, rotate, out double ox, out double oy, out double cs, out double sn);
            double du = u - cx, dv = v - cy;
            tu = du * cs + dv * sn + cx + ox;
            tv = -du * sn + dv * cs + cy + oy;
        }

        /// <summary>Mikkelsen's Gain3 (the S-curve of the weights), normalised.</summary>
        public static void HexGain(ref double a, ref double b, ref double c, double r)
        {
            double k = Math.Log(1.0 - r) / Math.Log(0.5);
            double G(double x) => x < 0.5 ? 0.5 * Math.Pow(Math.Max(0.0, 2.0 * x), k) : 1.0 - 0.5 * Math.Pow(Math.Max(0.0, 2.0 - 2.0 * x), k);
            a = G(a); b = G(b); c = G(c);
            double s = Math.Max(a + b + c, 1e-7);
            a /= s; b /= s; c /= s;
        }

        // ================================================================================================= triplanar

        /// <summary>Plane weights |n|^4, normalised, cut at 0.01, renormalised (NocTriSetup).</summary>
        public static V3d TriWeights(V3d n)
        {
            double p = NocTexConstants.TexTriPow, cut = NocTexConstants.TexTriCut;
            double wx = Math.Pow(Math.Abs(n.x), p), wy = Math.Pow(Math.Abs(n.y), p), wz = Math.Pow(Math.Abs(n.z), p);
            double s = Math.Max(wx + wy + wz, 1e-7);
            wx = Math.Max(wx / s - cut, 0); wy = Math.Max(wy / s - cut, 0); wz = Math.Max(wz / s - cut, 0);
            s = Math.Max(wx + wy + wz, 1e-7);
            return new V3d(wx / s, wy / s, wz / s);
        }

        /// <summary>Plane k's metric coordinate (X = (z, y), Y = (x, z), Z = (x, y)).</summary>
        public static void TriPlane(V3d p, int k, out double cu, out double cv)
        {
            if (k == 0) { cu = p.z; cv = p.y; }
            else if (k == 1) { cu = p.x; cv = p.z; }
            else { cu = p.x; cv = p.y; }
        }

        /// <summary>Volume gradient of a plane's slope (NocTriVolume).</summary>
        public static V3d TriVolume(int k, double su, double sv) => k == 0 ? new V3d(0, sv, su) : (k == 1 ? new V3d(su, 0, sv) : new V3d(su, sv, 0));

        /// <summary>Triplanar surface-gradient normal for per-plane slope functions slope(k, cu, cv) -> (su, sv) (unrotated taps).</summary>
        public static V3d TriNormal(V3d p, V3d n, Func<int, double, double, (double su, double sv)> slope)
        {
            V3d w = TriWeights(n);
            V3d vg = new V3d(0, 0, 0);
            for (int k = 0; k < 3; k++)
            {
                if (w[k] <= 0) continue;
                TriPlane(p, k, out double cu, out double cv);
                var s = slope(k, cu, cv);
                vg = vg + w[k] * TriVolume(k, s.su, s.sv);
            }
            return ResolveNormal(n, SurfGradVolume(n, vg));
        }

        public static V3d SurfGradVolume(V3d n, V3d vg) => vg - n * Dot(n, vg);
        public static V3d SurfGradTB(double su, double sv, V3d t, V3d b) => t * su + b * sv;
        public static V3d ResolveNormal(V3d n, V3d sg) { V3d r = n - sg; double l = Dot(r, r); return l > 1e-12 ? r * (1.0 / Math.Sqrt(l)) : n; }

        /// <summary>Tangent normal (GL) -> height slope: -t.xy / max(t.z, 1/128).</summary>
        public static void TangentToSlope(V3d t, out double su, out double sv) { double z = Math.Max(t.z, 1.0 / 128.0); su = -t.x / z; sv = -t.y / z; }

        // ================================================================================================= UV0 frame (GenBasisTB)

        /// <summary>NocUvFrame from the screen derivatives of P and uv.</summary>
        public static void UvFrame(V3d n, V3d dPdx, V3d dPdy, double dudx, double dvdx, double dudy, double dvdy, out V3d t, out V3d b)
        {
            V3d sx = dPdx - n * Dot(n, dPdx), sy = dPdy - n * Dot(n, dPdy);
            double det = dudx * dvdy - dvdx * dudy;
            double sd = det < 0 ? -1 : 1;
            V3d tt = sx * (sd * dvdy) + sy * (-sd * dvdx);
            V3d bb = sx * (-sd * dudy) + sy * (sd * dudx);
            double lt = Dot(tt, tt), lb = Dot(bb, bb);
            V3d fb = Math.Abs(n.y) < 0.999 ? new V3d(0, 1, 0) : new V3d(1, 0, 0);
            t = lt > 1e-20 ? tt * (1.0 / Math.Sqrt(lt)) : Normalize(Cross(fb, n));
            b = lb > 1e-20 ? bb * (1.0 / Math.Sqrt(lb)) : Cross(n, t);
        }

        // ================================================================================================= facade frame (NocLitCore)

        /// <summary>NocFaceFrame: Tp = the horizontal tangent projected into the surface, sigma = sign(ddx(u) ddx(P).Tp + ddy(u)
        /// ddy(P).Tp) (+1 under 1e-12), T = sigma Tp, B = world up projected into the surface.</summary>
        public static void FaceFrame(V3d n, double dudx, double dudy, V3d dPdx, V3d dPdy, out V3d t, out V3d b, out double sigma)
        {
            V3d t0 = new V3d(n.z, 0, -n.x);
            V3d tp = t0 - n * Dot(n, t0);
            double lt = Dot(tp, tp);
            tp = lt > 1e-12 ? tp * (1.0 / Math.Sqrt(lt)) : new V3d(1, 0, 0);
            double sg = dudx * Dot(dPdx, tp) + dudy * Dot(dPdy, tp);
            sigma = sg < -1e-12 ? -1.0 : 1.0;
            t = tp * sigma;
            V3d bp = new V3d(0, 1, 0) - n * n.y;
            double lb = Dot(bp, bp);
            b = lb > 1e-12 ? bp * (1.0 / Math.Sqrt(lb)) : new V3d(0, 0, 1);
        }

        // ================================================================================================= module and panel shuffle

        /// <summary>NocModuleSetup: texture coordinate (tiles) of the metric point (um, vm) and the module ids (world, source).</summary>
        public static void ModuleUV(double um, double vm, double mx, double my, double countsX, double countsY, double tile, bool runningBond, uint seed,
                                    out double tu, out double tv, out int i, out int j, out int si, out int sj)
        {
            j = (int)Math.Floor(vm / my);
            int odd = runningBond ? (((j % 2) + 2) % 2) : 0;
            i = (int)Math.Floor((um - 0.5 * mx * odd) / mx);
            uint hx = unchecked((uint)i), hy = unchecked((uint)j), hz = seed;
            NocHash.Pcg3d(ref hx, ref hy, ref hz);
            int cx = Math.Max((int)Math.Round(countsX), 1), cy = Math.Max((int)Math.Round(countsY), 1);
            si = (int)(hx % (uint)cx);
            sj = runningBond ? 2 * (int)(hy % (uint)Math.Max(cy / 2, 1)) + odd : (int)(hy % (uint)cy);
            tu = (um - (i - si) * mx) / tile;
            tv = (vm - (j - sj) * my) / tile;
        }

        /// <summary>NocPanelUV: cell-local metres / tile + the panel's hashed offset (tiles).</summary>
        public static void PanelUV(double lu, double lv, uint panelHash, double tile, out double tu, out double tv)
        {
            uint a = panelHash, b = 0x50414E4Cu, c = 0x9E3779B9u;
            NocHash.Pcg3d(ref a, ref b, ref c);
            tu = lu / tile + NocHash.U01(a);
            tv = lv / tile + NocHash.U01(b);
        }

        /// <summary>NocPanelTone: 1 +- 0.12 faded by smoothstep(2, 4, panel px).</summary>
        public static double PanelTone(uint panelHash, double panelPx)
            => 1.0 + NocTexConstants.TexPanelTone * (2.0 * NocHash.U01(NocHash.Pcg(panelHash ^ 0x544F4E45u)) - 1.0)
                   * Smoothstep(NocTexConstants.TexPanelFadePx0, NocTexConstants.TexPanelFadePx1, panelPx);

        // ================================================================================================= precision

        /// <summary>Error (texels) of the shader's float32 uv = P x (1 / tile) at world coordinate p against the exact double, for a
        /// tile of `res` texels: |float(float(p) x float(1 / tile)) - p / tile| x res.</summary>
        public static double Float32UvErrorTexels(double p, double tile, int res)
        {
            float pf = (float)p, inv = (float)(1.0 / tile);
            float uvf = pf * inv;
            return Math.Abs(uvf - p / tile) * res;
        }
    }
}
