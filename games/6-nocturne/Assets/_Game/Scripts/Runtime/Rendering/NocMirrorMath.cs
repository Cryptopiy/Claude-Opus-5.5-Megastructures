// NocMirrorMath: the double-precision C# mirror of Shaders/Include/NocMirror.hlsl (14.7 H4, D-003, R1 section 2) and of the
// NocWet puddle noise, for WetFloorTests (T-H4-ENERGY, T-H4-STREAK, T-H4-KS17, T-H4-CONT, Fresnel, puddle fraction). The lobe
// here has no pixel-footprint term unless a footprint variance is passed (the shader adds it from ddx / ddy of r).
using System;

namespace Megastructure
{
    public static class NocMirrorMath
    {
        public struct Lobe
        {
            public D3 r, X, Y;
            public double Sxx, Syy, Sxy, F, FG, nv;
        }

        static D3 Norm(D3 a) { double l = Math.Sqrt(D3.Dot(a, a)); return l > 1e-300 ? a * (1.0 / l) : a; }

        /// <summary>Reflectivity x Schlick Fresnel with F0 0.03 (H4).</summary>
        public static double Fresnel(double nv, double reflectivity)
            => reflectivity * (NocturneConstants.WetF0 + (1.0 - NocturneConstants.WetF0) * Math.Pow(1.0 - NocMath.Clamp01(nv), 5.0));

        /// <summary>Smith-GGX masking G1.</summary>
        public static double G1(double nv, double alpha)
        {
            nv = Math.Max(nv, 1e-3);
            return 1.0 / (0.5 + 0.5 * Math.Sqrt(1.0 + alpha * alpha * (1.0 / (nv * nv) - 1.0)));
        }

        /// <summary>The mirror lobe at floor point P seen from eye (twin of NocMirrorLobe; latMin and the AA variance optional).</summary>
        public static Lobe MirrorLobe(D3 P, D3 eye, D3 n, double sigma2, double reflectivity, double latMinRad = 0.0, double aaXX = 0, double aaYY = 0, double aaXY = 0)
        {
            var o = new Lobe();
            D3 v = Norm(P - eye);
            D3 r = v - n * (2.0 * D3.Dot(v, n));
            r.y = Math.Max(r.y, 0.01);
            o.r = Norm(r);
            double nv = NocMath.Clamp01(-D3.Dot(n, v));
            o.nv = nv;
            D3 X = D3.Cross(n, o.r);
            double lx = Math.Sqrt(D3.Dot(X, X));
            o.X = lx > 1e-4 ? X * (1.0 / lx) : Norm(D3.Cross(new D3(0, 1, 0), v + new D3(0, 0, 1e-3)));
            o.Y = D3.Cross(o.r, o.X);
            o.Sxx = Math.Max(sigma2 * nv * nv + aaXX, latMinRad * latMinRad);
            o.Syy = sigma2 + aaYY;
            o.Sxy = aaXY;
            o.F = Fresnel(nv, reflectivity);
            o.FG = o.F * G1(nv, Math.Sqrt(0.5 * sigma2));
            return o;
        }

        /// <summary>Winitzki erf (the shader's form, |err| &lt; 1.3e-4).</summary>
        public static double ErfW(double x)
        {
            double x2 = x * x, ax2 = 0.147 * x2;
            return Math.Sign(x) * Math.Sqrt(Math.Max(1.0 - Math.Exp(-x2 * (4.0 / Math.PI + ax2) / (1.0 + ax2)), 0.0));
        }

        /// <summary>Gaussian mass of the lobe over a segment (or a disc when p0 == p1) of angular width w (twin of NocSegmentMass).</summary>
        public static double SegmentMass(double p0x, double p0y, double p1x, double p1y, double Sxx, double Syy, double Sxy, double w)
        {
            double dx = p1x - p0x, dy = p1y - p0y;
            double len = Math.Sqrt(dx * dx + dy * dy);
            bool disc = len < 1e-3 * w;
            double sw = disc ? 0.125 * w * w : w * w / (2.0 * Math.PI);
            double sxx = Sxx + sw, syy = Syy + sw, sxy = Sxy;
            double det = Math.Max(sxx * syy - sxy * sxy, 1e-20);
            double ixx = syy / det, iyy = sxx / det, ixy = -sxy / det;
            double norm = 1.0 / (2.0 * Math.PI * Math.Sqrt(det));
            if (disc)
            {
                double q = p0x * p0x * ixx + p0y * p0y * iyy + 2.0 * p0x * p0y * ixy;
                return NocMath.Clamp01(Math.PI * 0.25 * w * w * norm * Math.Exp(-0.5 * q));
            }
            double tx = dx / len, ty = dy / len;
            double a = Math.Max(tx * tx * ixx + ty * ty * iyy + 2.0 * tx * ty * ixy, 1e-12);
            double b = tx * p0x * ixx + ty * p0y * iyy + (tx * p0y + ty * p0x) * ixy;
            double mx = p0x - b / a * tx, my = p0y - b / a * ty;
            double qp = mx * mx * ixx + my * my * iyy + 2.0 * mx * my * ixy;
            double k = 1.0 / Math.Sqrt(2.0 * a);
            double line = Math.Exp(-0.5 * qp) * Math.Sqrt(Math.PI / (2.0 * a)) * (ErfW((a * len + b) * k) - ErfW(b * k));
            return NocMath.Clamp01(w * line * norm);
        }

        /// <summary>Reflected radiance factor of one source (mass x reflection fog x F G1; multiply by L_eff). Twin of the loop body.</summary>
        public static double SourceFactor(D3 P, double dEye, Lobe o, D3 s0, D3 s1, double rho, double sigma)
        {
            D3 A = s0 - P, B = s1 - P, Ld = B - A;
            double rLd = D3.Dot(o.r, Ld);
            double tq = NocMath.Clamp01((D3.Dot(o.r, A) * rLd - D3.Dot(A, Ld)) / Math.Max(D3.Dot(Ld, Ld) - rLd * rLd, 1e-6));
            D3 Q = A + Ld * tq;
            double dq = Math.Max(Math.Sqrt(D3.Dot(Q, Q)), 1e-2);
            double la = D3.Dot(A, o.r), lb = D3.Dot(B, o.r), lc = 0.1 * dq;
            if (Math.Max(la, lb) <= lc) return 0.0;
            D3 Ac = la < lc ? A + (B - A) * ((lc - la) / (lb - la)) : A;
            D3 Bc = lb < lc ? B + (A - B) * ((lc - lb) / (la - lb)) : B;
            double ra = D3.Dot(Ac, o.r), rb = D3.Dot(Bc, o.r);
            double mass = SegmentMass(D3.Dot(Ac, o.X) / ra, D3.Dot(Ac, o.Y) / ra, D3.Dot(Bc, o.X) / rb, D3.Dot(Bc, o.Y) / rb,
                                      o.Sxx, o.Syy, o.Sxy, 2.0 * rho / dq);
            double Tr = Math.Exp(-sigma * sigma * dq * (dq + 2.0 * dEye));
            return o.FG * mass * Tr;
        }

        /// <summary>One registered wall plane as NocWetRegistry uploads it (_NocWallA / B / C).</summary>
        public struct WallPlane
        {
            public double cx, cz, nx, nz, halfLength, top, kind, weight;
            public D3 colour;
        }

        /// <summary>Macro radiance of a reflected wall at hit height y (twin of NocWallMacro): kind 0 adds the 86.4 m strata ripple.</summary>
        public static D3 WallMacro(double y, double kind, D3 colour, double soft)
        {
            if (kind > 0.5) return colour;
            double k = 2.0 * Math.PI / 86.4;
            double amp = 0.25 * Math.Exp(-0.5 * (k * soft) * (k * soft));
            return colour * (1.0 + amp * Math.Cos(k * y));
        }

        /// <summary>
        /// Twin of NocWallReflect (NocMirror.hlsl): the first registered wall the ray P + t r hits, fogged toward sky, and its
        /// coverage (returned); the ends and top soften over the lobe width softRad (TP-2 deleted the v1 nearest-wall-only path;
        /// CRIT0 #8): the softening is at least SurfWallBlendRad (2.5 deg), and the wall behind a fading end shows through
        /// (c1 over c2 over the sky), so the reflection has no vertical seam at a wall's vanishing line.
        /// </summary>
        public static double WallReflect(D3 P, D3 r, double softRad, double dEye, double sigma, WallPlane[] walls, int count, D3 sky, out D3 colour)
        {
            double sig2 = sigma * sigma;
            double ang = Math.Max(softRad, NocSurfaceConstants.SurfWallBlendRad);
            double t1 = 1e9, t2 = 1e9, a1 = 0, a2 = 0;
            D3 c1 = sky, c2 = sky;
            for (int i = 0; i < Math.Min(count, walls.Length); i++)
            {
                var w = walls[i];
                double denom = r.x * w.nx + r.z * w.nz;
                if (denom > -1e-4) continue;
                double t = ((w.cx - P.x) * w.nx + (w.cz - P.z) * w.nz) / denom;
                if (t <= 0.0 || t >= t2) continue;
                D3 H = P + r * t;
                double u = (H.x - w.cx) * -w.nz + (H.z - w.cz) * w.nx;
                double soft = t * ang + 1e-3;
                double cov = NocMath.Clamp01(0.5 + (w.halfLength - Math.Abs(u)) / (2.0 * soft)) * NocMath.Clamp01(0.5 + (w.top - H.y) / (2.0 * soft)) * w.weight;
                if (cov <= 0.0) continue;
                double Tr = Math.Exp(-sig2 * t * (t + 2.0 * dEye));
                D3 m = WallMacro(H.y, w.kind, w.colour, t * softRad + 1e-3);
                D3 col = sky + (m - sky) * Tr;
                if (t < t1) { t2 = t1; c2 = c1; a2 = a1; t1 = t; c1 = col; a1 = cov; }
                else { t2 = t; c2 = col; a2 = cov; }
            }
            double a = a1 + (1.0 - a1) * a2;
            colour = (c1 * a1 + c2 * ((1.0 - a1) * a2)) * (1.0 / Math.Max(a, 1e-6));
            return a;
        }

        /// <summary>The NocWet puddle noise (twin of NocPuddleNoise).</summary>
        public static double PuddleNoise(double x, double z)
        {
            double ax = x / NocturneConstants.PuddleScale0, az = z / NocturneConstants.PuddleScale0;
            double bx = (0.8 * x - 0.6 * z) / NocturneConstants.PuddleScale1 + 31.7, bz = (0.6 * x + 0.8 * z) / NocturneConstants.PuddleScale1 + 31.7;
            return 0.62 * NocMath.ValueNoise(ax, az) + 0.38 * NocMath.ValueNoise(bx, bz);
        }

        /// <summary>Puddle mask at full resolution (edge 0.04 without the fwidth term).</summary>
        public static double PuddleMask(double x, double z)
        {
            double th = NocturneConstants.PuddleThreshold;
            return NocMath.Smoothstep(th - 0.04, th + 0.04, PuddleNoise(x, z));
        }
    }
}
