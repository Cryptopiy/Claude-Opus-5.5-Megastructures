// MistVolumeNoise: the tileable 3D noise of the mist volume (MASTER_PROMPT 14.7 H2 "density x (0.65 + 0.35 fbm3(p / 60 m +
// wind t))"). One 64^3 R8 texture holds a three-octave periodic gradient-noise fBm (4, 8 and 16 cells per tile, weights
// 1 : 0.5 : 0.25), contrast-stretched so its 2nd / 98th percentiles map to 0 / 1, so the shader pays one trilinear fetch
// per octave set instead of 24 hash evaluations per sample. Periodic on exact cell multiples, so the shader wraps the
// advected coordinate (computed in double on the CPU, MistVolumeState) on whole tiles: no seam when the clock wraps
// (Part 10 #33). Generated once behind the black load fade (about 0.1 s), deterministic (PCG hashes, no UnityEngine.Random).
// Pure maths in Sample(); the texture upload in Get() is the only UnityEngine part.
using System;
using UnityEngine;

namespace Megastructure.Rendering
{
    public static class MistVolumeNoise
    {
        /// <summary>Texels per side.</summary>
        public const int Size = 64;
        /// <summary>Cells per tile of the first octave (one cell = Size / 4 = 16 texels).</summary>
        public const int BaseCells = 4;
        /// <summary>Octaves (cells per tile 4, 8, 16).</summary>
        public const int Octaves = 3;
        /// <summary>Percentile window mapped to [0, 1].</summary>
        public const double LoPercentile = 0.02, HiPercentile = 0.98;

        static Texture3D s_tex;
        static double s_lo = double.NaN, s_hi = double.NaN;

        /// <summary>The cached texture (created on first use; call it during the load fade).</summary>
        public static Texture3D Get()
        {
            if (s_tex != null) return s_tex;
            var data = Generate(out _, out _);
            var tex = new Texture3D(Size, Size, Size, TextureFormat.R8, false)
            {
                name = "BwMistNoise3D",
                wrapMode = TextureWrapMode.Repeat,
                filterMode = FilterMode.Bilinear,   // trilinear between voxels (no mips: the mist samples are far coarser)
                anisoLevel = 0,
            };
            tex.SetPixelData(data, 0);
            tex.Apply(false, true);                 // no CPU copy in the player
            s_tex = tex;
            return s_tex;
        }

        /// <summary>The 64^3 bytes (x fastest) and the raw fBm percentiles that were mapped to 0 / 1.</summary>
        public static byte[] Generate(out double lo, out double hi)
        {
            int n = Size * Size * Size;
            var raw = new float[n];
            int i = 0;
            for (int z = 0; z < Size; z++)
                for (int y = 0; y < Size; y++)
                    for (int x = 0; x < Size; x++)
                        raw[i++] = (float)Fbm((x + 0.5) / Size, (y + 0.5) / Size, (z + 0.5) / Size);
            var sorted = (float[])raw.Clone();
            Array.Sort(sorted);
            lo = sorted[(int)(LoPercentile * (n - 1))];
            hi = sorted[(int)(HiPercentile * (n - 1))];
            s_lo = lo; s_hi = hi;
            double inv = 1.0 / Math.Max(hi - lo, 1e-6);
            var bytes = new byte[n];
            for (int k = 0; k < n; k++)
            {
                double v = (raw[k] - lo) * inv;
                v = v < 0 ? 0 : (v > 1 ? 1 : v);
                bytes[k] = (byte)Math.Round(v * 255.0);
            }
            return bytes;
        }

        /// <summary>Raw periodic fBm at tile coordinates (u, v, w) in [0, 1) (period 1 on every axis), about [-0.8, 0.8].</summary>
        public static double Fbm(double u, double v, double w)
        {
            double sum = 0.0, amp = 1.0, norm = 0.0;
            int cells = BaseCells;
            for (int o = 0; o < Octaves; o++)
            {
                sum += amp * Grad3(u * cells, v * cells, w * cells, cells, (uint)(o * 7919 + 17));
                norm += amp;
                amp *= 0.5;
                cells *= 2;
            }
            return sum / norm;
        }

        /// <summary>Periodic 3D gradient (Perlin) noise with the given integer period, quintic fade, 12 edge gradients.</summary>
        static double Grad3(double x, double y, double z, int period, uint seed)
        {
            int ix = (int)Math.Floor(x), iy = (int)Math.Floor(y), iz = (int)Math.Floor(z);
            double fx = x - ix, fy = y - iy, fz = z - iz;
            double ux = Fade(fx), uy = Fade(fy), uz = Fade(fz);
            double r = 0.0;
            for (int c = 0; c < 8; c++)
            {
                int dx = c & 1, dy = (c >> 1) & 1, dz = (c >> 2) & 1;
                double g = Corner(Wrap(ix + dx, period), Wrap(iy + dy, period), Wrap(iz + dz, period), seed, fx - dx, fy - dy, fz - dz);
                double wx = dx == 1 ? ux : 1.0 - ux, wy = dy == 1 ? uy : 1.0 - uy, wz = dz == 1 ? uz : 1.0 - uz;
                r += g * wx * wy * wz;
            }
            return r;
        }

        static readonly int[] s_g = { 1, 1, 0, -1, 1, 0, 1, -1, 0, -1, -1, 0, 1, 0, 1, -1, 0, 1, 1, 0, -1, -1, 0, -1, 0, 1, 1, 0, -1, 1, 0, 1, -1, 0, -1, -1 };

        static double Corner(int x, int y, int z, uint seed, double dx, double dy, double dz)
        {
            uint h = BwMath.Pcg3(x, y, z) ^ BwMath.Pcg(seed);
            int k = (int)(BwMath.Pcg(h) % 12u) * 3;
            return s_g[k] * dx + s_g[k + 1] * dy + s_g[k + 2] * dz;
        }

        static int Wrap(int i, int p) { int m = i % p; return m < 0 ? m + p : m; }
        static double Fade(double t) => t * t * t * (t * (t * 6.0 - 15.0) + 10.0);

        /// <summary>Normalised [0, 1] value at tile coordinates (the texture's value before quantisation; tests).</summary>
        public static double Sample(double u, double v, double w)
        {
            if (double.IsNaN(s_lo)) Generate(out _, out _);
            double x = (Fbm(Frac(u), Frac(v), Frac(w)) - s_lo) / Math.Max(s_hi - s_lo, 1e-6);
            return x < 0 ? 0 : (x > 1 ? 1 : x);
        }

        static double Frac(double x) => x - Math.Floor(x);
    }
}
