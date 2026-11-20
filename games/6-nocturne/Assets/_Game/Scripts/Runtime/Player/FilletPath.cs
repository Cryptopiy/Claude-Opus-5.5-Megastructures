using System;
using UnityEngine;

namespace Megastructure
{
    /// <summary>
    /// The sharp path polyline of 14.4 with circular fillets at its corners (xz plane), imported from The Ring's FilletPath
    /// (generic constructor (waypoints, cum, radii)) and ported to double precision (IMPORT_PLAN 3.1, 3.4: positions stay
    /// below 2 km, but the walker integrates distance over 1 500 m).
    /// Nocturne convention (14.4): the progress s IS the filleted path distance d (arc length of the walked centre line).
    /// <see cref="SharpS"/> / <see cref="DAtSharp"/> map d onto the sharp polyline progress (a straight piece maps 1:1, an arc of
    /// radius r and turn theta maps its length r theta onto [sC - t, sC + t], t = r tan(theta / 2)); they exist for the 14.4
    /// conversion table and its test only.
    /// Collinear waypoints (P1, P3, P4, P5) get no fillet (theta &lt; 1e-4). Allocation-free after construction.
    /// </summary>
    public sealed class FilletPath
    {
        struct Piece
        {
            public bool Arc;
            public double Ax, Az, Bx, Bz;      // line end points (arc: start and end points)
            public double Cx, Cz;              // arc centre
            public double R, Ang0, Sweep;      // arc radius, start angle (rad, atan2(z, x)), signed sweep (rad)
            public double D0, Len;             // path distance at the piece start, piece length
            public double S0, S1;              // sharp progress at the piece start and end
        }

        readonly Piece[] _p;

        /// <summary>Total length of the filleted path (m): 1 500.007 for the 14.4 path.</summary>
        public readonly double Length;

        /// <summary>Sharp progress at the path end (1 536.06).</summary>
        public double SharpEnd => _p[_p.Length - 1].S1;

        /// <summary>Number of pieces (lines and arcs).</summary>
        public int PieceCount => _p.Length;

        /// <summary>The Nocturne path of 14.4 (P0..P8, sharp cumulative distances, radii 24 / 30 / 30).</summary>
        public static FilletPath CreateNocturne()
            => new FilletPath(NocturneConstants.WaypointX, NocturneConstants.WaypointZ, NocturneConstants.SharpCum, NocturneConstants.CornerRadii);

        public FilletPath(double[] wx, double[] wz, double[] cum, double[] radius)
        {
            int n = wx.Length;
            var t = new double[n];
            var tmp = new Piece[2 * n];
            int count = 0;
            // tangent lengths at the corners (collinear waypoints get none)
            for (int k = 1; k < n - 1; k++)
            {
                Dir(wx[k - 1], wz[k - 1], wx[k], wz[k], out double ix, out double iz, out double lin);
                Dir(wx[k], wz[k], wx[k + 1], wz[k + 1], out double ox, out double oz, out double lout);
                double theta = Math.Acos(NocMath.Clamp(ix * ox + iz * oz, -1.0, 1.0));
                if (theta < 1e-4 || radius[k] <= 0.0) continue;
                double tk = radius[k] * Math.Tan(0.5 * theta);
                // never longer than half of either adjacent leg
                t[k] = Math.Min(tk, 0.5 * Math.Min(lin, lout));
            }
            double d = 0.0;
            for (int i = 0; i < n - 1; i++)
            {
                Dir(wx[i], wz[i], wx[i + 1], wz[i + 1], out double dx, out double dz, out _);
                double ax = wx[i] + dx * t[i], az = wz[i] + dz * t[i];
                double bx = wx[i + 1] - dx * t[i + 1], bz = wz[i + 1] - dz * t[i + 1];
                double len = Math.Sqrt((bx - ax) * (bx - ax) + (bz - az) * (bz - az));
                if (len > 1e-9)
                {
                    tmp[count++] = new Piece { Arc = false, Ax = ax, Az = az, Bx = bx, Bz = bz, D0 = d, Len = len, S0 = cum[i] + t[i], S1 = cum[i + 1] - t[i + 1] };
                    d += len;
                }
                int k = i + 1;
                if (k < n - 1 && t[k] > 0.0)
                {
                    Dir(wx[k], wz[k], wx[k + 1], wz[k + 1], out double ox, out double oz, out _);
                    double theta = Math.Acos(NocMath.Clamp(dx * ox + dz * oz, -1.0, 1.0));
                    double r = t[k] / Math.Max(1e-12, Math.Tan(0.5 * theta));
                    double cross = dx * oz - dz * ox;
                    // left normal of the incoming direction for a left turn (cross > 0 in the (x, z) plane), right normal otherwise
                    double nx = cross < 0.0 ? dz : -dz, nz = cross < 0.0 ? -dx : dx;
                    double psx = wx[k] - dx * t[k], psz = wz[k] - dz * t[k];
                    double pex = wx[k] + ox * t[k], pez = wz[k] + oz * t[k];
                    double cx = psx + nx * r, cz = psz + nz * r;
                    double ang0 = Math.Atan2(psz - cz, psx - cx);
                    double sweep = cross < 0.0 ? -theta : theta;
                    double alen = r * theta;
                    tmp[count++] = new Piece { Arc = true, Ax = psx, Az = psz, Bx = pex, Bz = pez, Cx = cx, Cz = cz, R = r, Ang0 = ang0, Sweep = sweep, D0 = d, Len = alen, S0 = cum[k] - t[k], S1 = cum[k] + t[k] };
                    d += alen;
                }
            }
            _p = new Piece[count];
            Array.Copy(tmp, _p, count);
            Length = d;
        }

        static void Dir(double x0, double z0, double x1, double z1, out double dx, out double dz, out double len)
        {
            dx = x1 - x0; dz = z1 - z0;
            len = Math.Sqrt(dx * dx + dz * dz);
            if (len > 1e-12) { dx /= len; dz /= len; } else { dx = 0; dz = 1; }
        }

        int PieceAtD(double d)
        {
            for (int i = 0; i < _p.Length; i++) if (d < _p[i].D0 + _p[i].Len) return i;
            return _p.Length - 1;
        }

        int PieceAtS(double s)
        {
            for (int i = 0; i < _p.Length; i++) if (s < _p[i].S1) return i;
            return _p.Length - 1;
        }

        /// <summary>Start / end distance and radius of arc number i (0-based over the arcs only; false when there is none).</summary>
        public bool Arc(int i, out double d0, out double d1, out double radius, out double cx, out double cz)
        {
            int k = -1;
            for (int j = 0; j < _p.Length; j++)
            {
                if (!_p[j].Arc) continue;
                if (++k == i) { d0 = _p[j].D0; d1 = _p[j].D0 + _p[j].Len; radius = _p[j].R; cx = _p[j].Cx; cz = _p[j].Cz; return true; }
            }
            d0 = d1 = radius = cx = cz = 0;
            return false;
        }

        /// <summary>True when d lies on a fillet arc.</summary>
        public bool OnArc(double d) { d = NocMath.Clamp(d, 0, Length); return _p[PieceAtD(d)].Arc; }

        // ------------------------------------------------------------------ double API
        /// <summary>Point (x, z) at path distance d (clamped to the path).</summary>
        public void PointAt(double d, out double x, out double z)
        {
            d = NocMath.Clamp(d, 0.0, Length);
            ref readonly Piece p = ref _p[PieceAtD(d)];
            double u = NocMath.Clamp(d - p.D0, 0.0, p.Len);
            if (!p.Arc)
            {
                double f = u / Math.Max(1e-12, p.Len);
                x = p.Ax + (p.Bx - p.Ax) * f; z = p.Az + (p.Bz - p.Az) * f;
                return;
            }
            double a = p.Ang0 + p.Sweep * (u / Math.Max(1e-12, p.Len));
            x = p.Cx + Math.Cos(a) * p.R; z = p.Cz + Math.Sin(a) * p.R;
        }

        /// <summary>Unit tangent (x, z) at path distance d.</summary>
        public void TangentAt(double d, out double tx, out double tz)
        {
            d = NocMath.Clamp(d, 0.0, Length);
            ref readonly Piece p = ref _p[PieceAtD(d)];
            if (!p.Arc)
            {
                double l = Math.Max(1e-12, p.Len);
                tx = (p.Bx - p.Ax) / l; tz = (p.Bz - p.Az) / l;
                return;
            }
            double u = NocMath.Clamp(d - p.D0, 0.0, p.Len);
            double a = p.Ang0 + p.Sweep * (u / Math.Max(1e-12, p.Len));
            double sg = p.Sweep < 0.0 ? -1.0 : 1.0;
            tx = -Math.Sin(a) * sg; tz = Math.Cos(a) * sg;
        }

        /// <summary>Right-hand normal (x, z) at d (walking +Z the right is +X).</summary>
        public void RightAt(double d, out double rx, out double rz)
        {
            TangentAt(d, out double tx, out double tz);
            rx = tz; rz = -tx;
        }

        /// <summary>Heading in degrees (0 = +Z, 90 = +X) at d.</summary>
        public double HeadingDeg(double d)
        {
            TangentAt(d, out double tx, out double tz);
            return Math.Atan2(tx, tz) / NocMath.Deg;
        }

        /// <summary>World point (x, z) of the corridor coordinate (d, lateral to the right).</summary>
        public void CorridorPoint(double d, double lateral, out double x, out double z)
        {
            PointAt(d, out x, out z);
            RightAt(d, out double rx, out double rz);
            x += rx * lateral; z += rz * lateral;
        }

        /// <summary>Sharp progress at path distance d (14.4 conversion).</summary>
        public double SharpS(double d)
        {
            if (d <= 0.0) return _p[0].S0;
            if (d >= Length) return SharpEnd;
            ref readonly Piece p = ref _p[PieceAtD(d)];
            double u = NocMath.Clamp01((d - p.D0) / Math.Max(1e-12, p.Len));
            return p.S0 + (p.S1 - p.S0) * u;
        }

        /// <summary>Path distance d at sharp progress s (inverse of <see cref="SharpS"/>).</summary>
        public double DAtSharp(double s)
        {
            if (s <= _p[0].S0) return 0.0;
            if (s >= SharpEnd) return Length;
            ref readonly Piece p = ref _p[PieceAtS(s)];
            double u = NocMath.Clamp01((s - p.S0) / Math.Max(1e-12, p.S1 - p.S0));
            return p.D0 + p.Len * u;
        }

        static double WrapPi(double a)
        {
            const double twoPi = 2.0 * Math.PI;
            return a - twoPi * Math.Floor((a + Math.PI) / twoPi);
        }

        /// <summary>Nearest path distance to (x, z). Only pieces within [hintD - window, hintD + window] are searched
        /// (window &lt; 0 = whole path); dist = distance from the point to the path.</summary>
        public double Project(double x, double z, double hintD, double window, out double dist)
        {
            double best = double.MaxValue, bestD = NocMath.Clamp(hintD, 0.0, Length);
            for (int i = 0; i < _p.Length; i++)
            {
                ref readonly Piece p = ref _p[i];
                if (window >= 0.0 && (p.D0 + p.Len < hintD - window || p.D0 > hintD + window)) continue;
                double u, dd;
                if (!p.Arc)
                {
                    double abx = p.Bx - p.Ax, abz = p.Bz - p.Az;
                    double tt = NocMath.Clamp01(((x - p.Ax) * abx + (z - p.Az) * abz) / Math.Max(1e-12, p.Len * p.Len));
                    double qx = p.Ax + abx * tt, qz = p.Az + abz * tt;
                    dd = (x - qx) * (x - qx) + (z - qz) * (z - qz);
                    u = tt * p.Len;
                }
                else
                {
                    double vx = x - p.Cx, vz = z - p.Cz;
                    double ang = vx * vx + vz * vz > 1e-16 ? Math.Atan2(vz, vx) : p.Ang0;
                    double sg = p.Sweep < 0.0 ? -1.0 : 1.0;
                    double rel = WrapPi(ang - p.Ang0) * sg;
                    double sw = Math.Abs(p.Sweep);
                    if (rel < 0.0 || rel > sw)
                    {
                        double toStart = Math.Abs(WrapPi(ang - p.Ang0));
                        double toEnd = Math.Abs(WrapPi(ang - (p.Ang0 + p.Sweep)));
                        rel = toStart <= toEnd ? 0.0 : sw;
                    }
                    u = p.Len * rel / Math.Max(1e-12, sw);
                    double a = p.Ang0 + p.Sweep * (u / Math.Max(1e-12, p.Len));
                    double qx = p.Cx + Math.Cos(a) * p.R, qz = p.Cz + Math.Sin(a) * p.R;
                    dd = (x - qx) * (x - qx) + (z - qz) * (z - qz);
                }
                if (dd < best - 1e-12) { best = dd; bestD = p.D0 + u; }
            }
            dist = Math.Sqrt(best);
            return bestD;
        }

        /// <summary>Signed lateral offset of (x, z) from the path point at d (positive = right of travel).</summary>
        public double Lateral(double x, double z, double d)
        {
            PointAt(d, out double px, out double pz);
            RightAt(d, out double rx, out double rz);
            return (x - px) * rx + (z - pz) * rz;
        }

        // ------------------------------------------------------------------ float conveniences (The Ring API)
        public Vector2 PointAt(double d) { PointAt(d, out double x, out double z); return new Vector2((float)x, (float)z); }
        public Vector2 TangentAt(double d) { TangentAt(d, out double x, out double z); return new Vector2((float)x, (float)z); }
        public Vector2 RightAt(double d) { RightAt(d, out double x, out double z); return new Vector2((float)x, (float)z); }
        public float HeadingAt(double d) => (float)HeadingDeg(d);
        public double Project(Vector2 xz, double hintD, double window, out double dist) => Project(xz.x, xz.y, hintD, window, out dist);
        public double Lateral(Vector2 xz, double d) => Lateral(xz.x, xz.y, d);
    }
}
