// FilletPath: a sharp polyline in the xz plane with circular fillets at its corners, in double precision.
// Port of The Ring's generic FilletPath constructor (waypoints, cumulative sharp lengths, corner radii) to double;
// the Ring's 3-radius convenience constructor and its game waypoints are not ported. Pure C#, allocation-free after
// construction. Bulwark's progress s IS the path distance d along the filleted curve (BwPath); the sharp progress
// mapping (SharpS / DAtSharp, from `cum`) is kept for completeness only.
// Conventions: points are (x, z); heading 0 = +Z, 90 = +X; right of travel = (tz, -tx) (walking +Z, right is +X).
using System;

namespace Megastructure
{
    public sealed class FilletPath
    {
        struct Piece
        {
            public bool Arc;
            public double Ax, Az, Bx, Bz;      // line end points (arc: start and end points)
            public double Cx, Cz;              // arc centre
            public double R, Ang0, Sweep;      // arc radius, start angle atan2(z, x) about the centre (rad), signed sweep (rad)
            public double D0, Len;             // path distance at the piece start, piece length
            public double S0, S1;              // sharp progress at the piece start and end
        }

        readonly Piece[] _p;

        /// <summary>Total length of the filleted path (m).</summary>
        public readonly double Length;

        /// <summary>Sharp progress at the path end (the last cumulative waypoint length).</summary>
        public double SharpEnd => _p[_p.Length - 1].S1;

        /// <summary>Generic constructor. w = waypoints (x, z); cum[i] = sharp cumulative length at w[i]; radius[i] =
        /// fillet radius at corner i (0 or collinear = sharp). The tangent length t = r tan(theta / 2) is clamped to
        /// half of either adjacent leg (the clamp then shrinks the radius).</summary>
        public FilletPath((double x, double z)[] w, double[] cum, double[] radius)
        {
            if (w == null || cum == null || radius == null) throw new ArgumentNullException();
            int n = w.Length;
            if (n < 2 || cum.Length != n || radius.Length != n) throw new ArgumentException("FilletPath: waypoints, cum and radius must have the same length (>= 2)");
            var t = new double[n];
            var tmp = new Piece[2 * n];
            int count = 0;
            for (int k = 1; k < n - 1; k++)
            {
                Dir(w[k - 1], w[k], out double ix, out double iz, out double lin);
                Dir(w[k], w[k + 1], out double ox, out double oz, out double lout);
                double theta = TurnAngle(ix, iz, ox, oz);
                if (theta < 1e-9 || radius[k] <= 0.0) continue;
                double tk = radius[k] * Math.Tan(0.5 * theta);
                t[k] = Math.Min(tk, 0.5 * Math.Min(lin, lout));
            }
            double d = 0.0;
            for (int i = 0; i < n - 1; i++)
            {
                Dir(w[i], w[i + 1], out double dx, out double dz, out _);
                double ax = w[i].x + dx * t[i], az = w[i].z + dz * t[i];
                double bx = w[i + 1].x - dx * t[i + 1], bz = w[i + 1].z - dz * t[i + 1];
                double len = Math.Sqrt((bx - ax) * (bx - ax) + (bz - az) * (bz - az));
                if (len > 1e-9)
                {
                    tmp[count++] = new Piece { Arc = false, Ax = ax, Az = az, Bx = bx, Bz = bz, D0 = d, Len = len, S0 = cum[i] + t[i], S1 = cum[i + 1] - t[i + 1] };
                    d += len;
                }
                int k = i + 1;
                if (k < n - 1 && t[k] > 0.0)
                {
                    Dir(w[k], w[k + 1], out double ox, out double oz, out _);
                    double theta = TurnAngle(dx, dz, ox, oz);
                    double r = t[k] / Math.Max(1e-12, Math.Tan(0.5 * theta));
                    double cross = dx * oz - dz * ox;
                    // centre side: cross < 0 is a clockwise turn in (x, z), i.e. a right turn (heading increases)
                    double nx = cross < 0.0 ? dz : -dz, nz = cross < 0.0 ? -dx : dx;
                    double psx = w[k].x - dx * t[k], psz = w[k].z - dz * t[k];
                    double pex = w[k].x + ox * t[k], pez = w[k].z + oz * t[k];
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

        static void Dir((double x, double z) a, (double x, double z) b, out double dx, out double dz, out double len)
        {
            dx = b.x - a.x; dz = b.z - a.z;
            len = Math.Sqrt(dx * dx + dz * dz);
            if (len > 0.0) { dx /= len; dz /= len; }
        }

        /// <summary>Unsigned turn angle between two unit directions (rad), exact at small angles (atan2, not acos).</summary>
        static double TurnAngle(double ix, double iz, double ox, double oz)
            => Math.Atan2(Math.Abs(ix * oz - iz * ox), ix * ox + iz * oz);

        // ---------------------------------------------------------------- pieces
        public int PieceCount => _p.Length;
        public double PieceStart(int i) => _p[i].D0;
        public double PieceEnd(int i) => _p[i].D0 + _p[i].Len;
        public double PieceLength(int i) => _p[i].Len;
        public bool PieceIsArc(int i) => _p[i].Arc;
        /// <summary>Arc radius of piece i (0 for a straight piece).</summary>
        public double PieceRadius(int i) => _p[i].Arc ? _p[i].R : 0.0;

        /// <summary>Index of the piece containing path distance d (clamped).</summary>
        public int PieceAt(double d)
        {
            for (int i = 0; i < _p.Length; i++) if (d < _p[i].D0 + _p[i].Len) return i;
            return _p.Length - 1;
        }

        int PieceAtS(double s)
        {
            for (int i = 0; i < _p.Length; i++) if (s < _p[i].S1) return i;
            return _p.Length - 1;
        }

        static void ArcPoint(in Piece p, double u, out double x, out double z)
        {
            double a = p.Ang0 + p.Sweep * (u / Math.Max(1e-12, p.Len));
            x = p.Cx + Math.Cos(a) * p.R;
            z = p.Cz + Math.Sin(a) * p.R;
        }

        // ---------------------------------------------------------------- evaluation
        /// <summary>Point (x, z) at path distance d (clamped to the path).</summary>
        public void PointAt(double d, out double x, out double z)
        {
            d = Clamp(d, 0.0, Length);
            var p = _p[PieceAt(d)];
            double u = Clamp(d - p.D0, 0.0, p.Len);
            if (!p.Arc)
            {
                double f = u / Math.Max(1e-12, p.Len);
                x = p.Ax + (p.Bx - p.Ax) * f;
                z = p.Az + (p.Bz - p.Az) * f;
                return;
            }
            ArcPoint(p, u, out x, out z);
        }

        /// <summary>Unit tangent (x, z) at path distance d.</summary>
        public void TangentAt(double d, out double tx, out double tz)
        {
            d = Clamp(d, 0.0, Length);
            var p = _p[PieceAt(d)];
            if (!p.Arc)
            {
                double inv = 1.0 / Math.Max(1e-12, p.Len);
                tx = (p.Bx - p.Ax) * inv; tz = (p.Bz - p.Az) * inv;
                return;
            }
            double u = Clamp(d - p.D0, 0.0, p.Len);
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
        public double HeadingAt(double d)
        {
            TangentAt(d, out double tx, out double tz);
            return Math.Atan2(tx, tz) * (180.0 / Math.PI);
        }

        /// <summary>Signed curvature at d (1/m): positive for a right turn (heading increasing), 0 on straights.</summary>
        public double CurvatureAt(double d)
        {
            d = Clamp(d, 0.0, Length);
            var p = _p[PieceAt(d)];
            if (!p.Arc) return 0.0;
            return p.Sweep < 0.0 ? 1.0 / p.R : -1.0 / p.R;
        }

        /// <summary>Sharp progress at path distance d (Ring mapping; Bulwark's s is d itself).</summary>
        public double SharpS(double d)
        {
            if (d <= 0.0) return _p[0].S0;
            if (d >= Length) return SharpEnd;
            var p = _p[PieceAt(d)];
            double u = Clamp((d - p.D0) / Math.Max(1e-12, p.Len), 0.0, 1.0);
            return p.S0 + (p.S1 - p.S0) * u;
        }

        /// <summary>Path distance d at sharp progress s (inverse of <see cref="SharpS"/>).</summary>
        public double DAtSharp(double s)
        {
            if (s <= _p[0].S0) return 0.0;
            if (s >= SharpEnd) return Length;
            var p = _p[PieceAtS(s)];
            double u = Clamp((s - p.S0) / Math.Max(1e-12, p.S1 - p.S0), 0.0, 1.0);
            return p.D0 + p.Len * u;
        }

        static double WrapPi(double a)
        {
            const double twoPi = 2.0 * Math.PI;
            return a - twoPi * Math.Floor((a + Math.PI) / twoPi);
        }

        /// <summary>Nearest path distance to (x, z). Only pieces overlapping [hintD - window, hintD + window] are searched
        /// (window &lt; 0 = whole path). dist = distance from (x, z) to the path. Arcs project through their centre and
        /// snap to the nearer end outside their sector, so the result is exact for points on any normal of the path.</summary>
        public double Project(double x, double z, double hintD, double window, out double dist)
        {
            double best = double.MaxValue, bestD = Clamp(hintD, 0.0, Length);
            for (int i = 0; i < _p.Length; i++)
            {
                var p = _p[i];
                if (window >= 0.0 && (p.D0 + p.Len < hintD - window || p.D0 > hintD + window)) continue;
                double u, dd;
                if (!p.Arc)
                {
                    double abx = p.Bx - p.Ax, abz = p.Bz - p.Az;
                    double tt = Clamp(((x - p.Ax) * abx + (z - p.Az) * abz) / Math.Max(1e-18, p.Len * p.Len), 0.0, 1.0);
                    double qx = p.Ax + abx * tt, qz = p.Az + abz * tt;
                    dd = (x - qx) * (x - qx) + (z - qz) * (z - qz);
                    u = tt * p.Len;
                }
                else
                {
                    double vx = x - p.Cx, vz = z - p.Cz;
                    double ang = vx * vx + vz * vz > 1e-20 ? Math.Atan2(vz, vx) : p.Ang0;
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
                    ArcPoint(p, u, out double qx, out double qz);
                    dd = (x - qx) * (x - qx) + (z - qz) * (z - qz);
                }
                if (dd < best - 1e-14) { best = dd; bestD = p.D0 + u; }
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

        static double Clamp(double v, double a, double b) => v < a ? a : (v > b ? b : v);
    }
}
