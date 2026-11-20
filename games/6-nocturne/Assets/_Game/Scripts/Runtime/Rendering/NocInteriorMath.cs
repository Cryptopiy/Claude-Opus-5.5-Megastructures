// NocInteriorMath: the C# twin of Shaders/Include/NocInterior.hlsl (Look v2 box rooms, docs/TEXTURE_PLAN.md 2.8, FA-3), owner FA.
// RoomRaw = NocRoomRawV2 (the radiance of the room surface a ray from the glass plane leaves through, each surface linear in its exit
// depth), RoomMean = NocRoomMeanV2 (its exact expectation over the glass for one view, from closed-form shares and mean exit depths)
// and NumericMean (the same expectation by an n x n sample integral), which InteriorTests checks RoomMean against up to 60 deg.
using System;

namespace Megastructure
{
    public static class NocInteriorMath
    {
        static double Sat(double x) => x < 0 ? 0 : (x > 1 ? 1 : x);

        /// <summary>Radiance (relative) of the room surface hit from glass-plane point (xg, yg) with shift (kx, ky) per metre of depth;
        /// the room spans x 0..W, y yf..yc, depth 0..Dr. surf: 0 back, 1 side, 2 ceiling, 3 floor.</summary>
        public static double RoomRaw(double xg, double yg, double kx, double ky, double W, double yf, double yc, double Dr, out int surf)
        {
            xg = Math.Min(W, Math.Max(0.0, xg)); yg = Math.Min(yc, Math.Max(yf, yg));   // inside the aperture (NocRoomRawV2)
            double tx = (kx >= 0 ? W - xg : xg) / Math.Max(Math.Abs(kx), 1e-5);
            double ty = (ky >= 0 ? yc - yg : yg - yf) / Math.Max(Math.Abs(ky), 1e-5);
            double t = Math.Min(Dr, Math.Min(tx, ty));
            double w = Sat(t / Math.Max(Dr, 1e-3));
            double v = Sat((Math.Min(yc, Math.Max(yf, yg + ky * t)) - yf) / Math.Max(yc - yf, 1e-3));
            if (t >= Dr - 1e-4) { surf = 0; return NocFacadeConstants.FaRoomBack * (0.85 + 0.3 * v); }
            if (tx <= ty) { surf = 1; return NocFacadeConstants.FaRoomSide * (1.3 - 0.6 * w); }
            if (ky >= 0) { surf = 2; return NocFacadeConstants.FaRoomCeil * (1.25 - 0.5 * w); }
            surf = 3; return NocFacadeConstants.FaRoomFloor * (1.2 - 0.4 * w);
        }

        /// <summary>NocRoomPartV2: for A uniform on [A0, A1] and B on [B0, B1], the integrals over A in [A0, min(A1, Dr)] of the
        /// B-length above A (i0) and of A times it (i1).</summary>
        static void Part(double A0, double A1, double B0, double B1, double Dr, out double i0, out double i1)
        {
            double Ae = Math.Min(A1, Dr), L = B1 - B0;
            i0 = 0; i1 = 0;
            double a = A0, b = Math.Min(Ae, B0);
            if (b > a) { i0 += L * (b - a); i1 += 0.5 * L * (b * b - a * a); }
            double p = Math.Max(A0, B0), q = Math.Min(Ae, B1);
            if (q > p) { i0 += B1 * (q - p) - 0.5 * (q * q - p * p); i1 += 0.5 * B1 * (q * q - p * p) - (q * q * q - p * p * p) / 3.0; }
        }

        /// <summary>The exact expectation of RoomRaw over the glass rectangle [g0, g1] x [h0, h1] (NocRoomMeanV2).</summary>
        public static double RoomMean(double g0, double g1, double h0, double h1, double kx, double ky, double W, double yf, double yc, double Dr)
        {
            const double BIG = 1e6;
            double ax = Math.Abs(kx), ay = Math.Abs(ky);
            double A0 = ax > 1e-5 ? (kx > 0 ? W - g1 : g0) / ax : BIG, A1 = ax > 1e-5 ? (kx > 0 ? W - g0 : g1) / ax : BIG + 1;
            double B0 = ay > 1e-5 ? (ky > 0 ? yc - h1 : h0 - yf) / ay : BIG, B1 = ay > 1e-5 ? (ky > 0 ? yc - h0 : h1 - yf) / ay : BIG + 1;
            double area = Math.Max((A1 - A0) * (B1 - B0), 1e-9);
            Part(A0, A1, B0, B1, Dr, out double s0, out double s1);
            Part(B0, B1, A0, A1, Dr, out double c0, out double c1);
            double fS = s0 / area, fC = c0 / area, fB = Math.Max(0, 1 - fS - fC);
            double wS = s0 > 0 ? s1 / (s0 * Dr) : 0, wC = c0 > 0 ? c1 / (c0 * Dr) : 0;
            double sy = ky * Dr, yb0 = Math.Max(h0, yf - sy), yb1 = Math.Min(h1, yc - sy);
            double vb = Sat((0.5 * (yb0 + yb1) + sy - yf) / Math.Max(yc - yf, 1e-3));
            double mB = NocFacadeConstants.FaRoomBack * (0.85 + 0.3 * vb), mS = NocFacadeConstants.FaRoomSide * (1.3 - 0.6 * wS);
            double mC = ky >= 0 ? NocFacadeConstants.FaRoomCeil * (1.25 - 0.5 * wC) : NocFacadeConstants.FaRoomFloor * (1.2 - 0.4 * wC);
            return fB * mB + fS * mS + fC * mC;
        }

        /// <summary>RoomRaw averaged over an n x n sample grid of the glass rectangle (the reference of the tests).</summary>
        public static double NumericMean(double g0, double g1, double h0, double h1, double kx, double ky, double W, double yf, double yc, double Dr, int n = 64)
        {
            double sum = 0;
            for (int j = 0; j < n; j++)
                for (int i = 0; i < n; i++)
                    sum += RoomRaw(g0 + (g1 - g0) * (i + 0.5) / n, h0 + (h1 - h0) * (j + 0.5) / n, kx, ky, W, yf, yc, Dr, out _);
            return sum / (n * n);
        }

        /// <summary>Room depth of a window with depth hash u: residential FaRoomDepthRes0..1, commercial / lobby FaRoomDepthCom0..1,
        /// section bays FaRoomDepthSec0..1 (TEXTURE_PLAN 2.8, 2.10).</summary>
        public static double Depth(int stratum, double u)
        {
            if (stratum == NocFacadeMath.StratumSection) return NocFacadeConstants.FaRoomDepthSec0 + (NocFacadeConstants.FaRoomDepthSec1 - NocFacadeConstants.FaRoomDepthSec0) * u;
            if (stratum == NocFacadeMath.StratumCom || stratum == NocFacadeMath.StratumLobby)
                return NocFacadeConstants.FaRoomDepthCom0 + (NocFacadeConstants.FaRoomDepthCom1 - NocFacadeConstants.FaRoomDepthCom0) * u;
            return NocFacadeConstants.FaRoomDepthRes0 + (NocFacadeConstants.FaRoomDepthRes1 - NocFacadeConstants.FaRoomDepthRes0) * u;
        }
    }
}
