// NocTexRecipes: the procedural PBR texture recipes of Nocturne (MASTER_PROMPT 8.11 items 2-3, docs/MATERIALS.md), pure C#
// (no UnityEngine) so EditMode tests can check them and the editor generator (NocTextureGen) can bake them. Every recipe is a
// function of (u, v) in [0, 1)^2 that is exactly periodic with period 1 (lattice noise with integer periods), returning the
// height (0..1, for the normal), an albedo VARIATION colour (mean about 0.5 linear; the generator renormalises each channel to
// exactly 0.5 so the palette token in the material stays the mean), metallic, occlusion and smoothness (0..1, mapped by the
// material to its smoothness range). Scale ladder (8.11 item 3): micro (grain, pores, aggregate), meso (joints, seams,
// plates, cracks) in the texture; macro (stains, weathering, 10 m variation) in the shader. Noise is used where it explains
// the material: no high-frequency detail beyond what the mipmaps filter.
using System;

namespace Megastructure
{
    public enum NocTexFamily { Concrete, Paving, Asphalt, SteelDeck, PlazaStone, Metal, Moss, Bark }

    public struct NocTexel
    {
        public double h;            // height 0..1
        public double r, g, b;      // albedo variation (linear, mean ~0.5)
        public double metal, ao, smooth;
    }

    public static class NocTexRecipes
    {
        public static readonly NocTexFamily[] All = (NocTexFamily[])Enum.GetValues(typeof(NocTexFamily));

        /// <summary>Texture size per family (8.11 item 9: at most 1k-2k per tile).</summary>
        public static int Size(NocTexFamily f) => (f == NocTexFamily.Moss || f == NocTexFamily.Bark) ? 512 : 1024;
        /// <summary>World size of one tile (m) the materials use, which sets the physical depth of the normal map.</summary>
        public static double TileMetres(NocTexFamily f)
        {
            switch (f)
            {
                case NocTexFamily.Concrete: return 3.0;
                case NocTexFamily.Paving: return 2.4;
                case NocTexFamily.Asphalt: return 2.0;
                case NocTexFamily.SteelDeck: return 2.4;
                case NocTexFamily.PlazaStone: return 4.8;
                case NocTexFamily.Metal: return 2.0;
                case NocTexFamily.Moss: return 1.5;
                default: return 1.0;
            }
        }
        /// <summary>Height range (m) that 0..1 height spans (relief depth for the normal derivation).</summary>
        public static double DepthMetres(NocTexFamily f)
        {
            switch (f)
            {
                case NocTexFamily.Concrete: return 0.004;
                case NocTexFamily.Paving: return 0.012;
                case NocTexFamily.Asphalt: return 0.006;
                case NocTexFamily.SteelDeck: return 0.004;
                case NocTexFamily.PlazaStone: return 0.006;
                case NocTexFamily.Metal: return 0.0015;
                case NocTexFamily.Moss: return 0.02;
                default: return 0.03;
            }
        }

        // ---------------------------------------------------------------- periodic noise
        static double Fade(double t) => t * t * t * (t * (t * 6 - 15) + 10);
        static int Wrap(int i, int p) { int m = i % p; return m < 0 ? m + p : m; }
        static uint H(int x, int y, int seed) => NocMath.Pcg3(x, y, seed);

        /// <summary>Periodic gradient noise in about [-0.7, 0.7]: lattice period p cells over the unit square.</summary>
        public static double Grad(double u, double v, int p, int seed) => GradA(u, v, p, p, seed);

        /// <summary>Anisotropic periodic gradient noise: px cells along u, py cells along v (stretched features stay tileable).</summary>
        public static double GradA(double u, double v, int px, int py, int seed)
        {
            double x = u * px, y = v * py;
            double fx = Math.Floor(x), fy = Math.Floor(y);
            int ix = (int)fx, iy = (int)fy;
            double tx = x - fx, ty = y - fy;
            double n00 = GDot(Wrap(ix, px), Wrap(iy, py), seed, tx, ty);
            double n10 = GDot(Wrap(ix + 1, px), Wrap(iy, py), seed, tx - 1, ty);
            double n01 = GDot(Wrap(ix, px), Wrap(iy + 1, py), seed, tx, ty - 1);
            double n11 = GDot(Wrap(ix + 1, px), Wrap(iy + 1, py), seed, tx - 1, ty - 1);
            double ux = Fade(tx), uy = Fade(ty);
            return NocMath.Lerp(NocMath.Lerp(n00, n10, ux), NocMath.Lerp(n01, n11, ux), uy);
        }
        static double GDot(int ix, int iy, int seed, double dx, double dy)
        {
            double a = NocMath.U01(H(ix, iy, seed)) * 2.0 * Math.PI;
            return Math.Cos(a) * dx + Math.Sin(a) * dy;
        }

        /// <summary>Periodic fBm in about [0, 1] (mean 0.5): octaves double the period.</summary>
        public static double Fbm(double u, double v, int p, int octaves, int seed, double gain = 0.5) => FbmA(u, v, p, p, octaves, seed, gain);

        /// <summary>Anisotropic periodic fBm (px along u, py along v).</summary>
        public static double FbmA(double u, double v, int px, int py, int octaves, int seed, double gain = 0.5)
        {
            double sum = 0, amp = 1, norm = 0;
            for (int o = 0; o < octaves; o++)
            {
                sum += amp * GradA(u, v, px << o, py << o, seed + 101 * o);
                norm += amp;
                amp *= gain;
            }
            return NocMath.Clamp01(0.5 + 0.75 * sum / norm);
        }

        /// <summary>Ridged periodic fBm in [0, 1] (1 on the ridges), anisotropic periods.</summary>
        public static double Ridged(double u, double v, int px, int py, int octaves, int seed)
        {
            double sum = 0, amp = 1, norm = 0;
            for (int o = 0; o < octaves; o++)
            {
                sum += amp * (1.0 - Math.Abs(GradA(u, v, px << o, py << o, seed + 131 * o)) * 1.6);
                norm += amp;
                amp *= 0.5;
            }
            return NocMath.Clamp01(sum / norm);
        }

        /// <summary>Periodic Worley noise: F1, F2 (in cell units) and the hash of the nearest cell.</summary>
        public static void Worley(double u, double v, int p, int seed, out double f1, out double f2, out uint id)
        {
            double x = u * p, y = v * p;
            int ix = (int)Math.Floor(x), iy = (int)Math.Floor(y);
            f1 = 9; f2 = 9; id = 0;
            for (int j = -1; j <= 1; j++)
                for (int i = -1; i <= 1; i++)
                {
                    int cx = ix + i, cy = iy + j;
                    uint h = H(Wrap(cx, p), Wrap(cy, p), seed);
                    double px = cx + NocMath.U01(h), py = cy + NocMath.U01(NocMath.Pcg(h));
                    double d = Math.Sqrt((px - x) * (px - x) + (py - y) * (py - y));
                    if (d < f1) { f2 = f1; f1 = d; id = h; }
                    else if (d < f2) f2 = d;
                }
        }

        /// <summary>Running-bond blocks: cols x rows per tile, every other row offset by half a block; returns the distance to
        /// the nearest joint (in tile units, x and y separately) and a per-block id.</summary>
        static void Blocks(double u, double v, int cols, int rows, bool bond, out double jx, out double jy, out int bx, out int by)
        {
            double y = v * rows;
            by = (int)Math.Floor(y);
            double x = u * cols + (bond && (by & 1) == 1 ? 0.5 : 0.0);
            bx = (int)Math.Floor(x);
            jx = Math.Min(x - bx, 1.0 - (x - bx)) / cols;
            jy = Math.Min(y - by, 1.0 - (y - by)) / rows;
            bx = Wrap(bx, cols); by = Wrap(by, rows);
        }

        static double Smooth(double e0, double e1, double x) => NocMath.Smoothstep(e0, e1, x);

        // ---------------------------------------------------------------- recipes
        public static NocTexel Sample(NocTexFamily f, double u, double v)
        {
            switch (f)
            {
                case NocTexFamily.Concrete: return Concrete(u, v);
                case NocTexFamily.Paving: return Paving(u, v);
                case NocTexFamily.Asphalt: return Asphalt(u, v);
                case NocTexFamily.SteelDeck: return SteelDeck(u, v);
                case NocTexFamily.PlazaStone: return PlazaStone(u, v);
                case NocTexFamily.Metal: return Metal(u, v);
                case NocTexFamily.Moss: return Moss(u, v);
                default: return Bark(u, v);
            }
        }

        // Cast concrete, 3 m tile: low undulation, fine aggregate, air-void pits (Worley), a formwork joint every 1.5 m.
        static NocTexel Concrete(double u, double v)
        {
            double low = Fbm(u, v, 4, 5, 11);
            double fine = Fbm(u, v, 48, 3, 12);
            Worley(u, v, 96, 13, out double f1, out _, out _);
            double pit = 1.0 - Smooth(0.08, 0.22, f1);                       // pores 1-2 cm
            double jx = Math.Min(Math.Abs(((u * 2.0) % 1.0) - 0.0), Math.Abs(1.0 - ((u * 2.0) % 1.0))) * 1.5;   // m to the joint
            double joint = 1.0 - Smooth(0.002, 0.006, jx);
            double h = 0.55 * low + 0.35 * fine - 0.35 * pit * 0.6 - 0.4 * joint;
            double stain = FbmA(u + 0.31, v, 3, 2, 4, 14);
            double a = 0.5 * (1.0 + 0.30 * (low - 0.5) + 0.22 * (fine - 0.5) - 0.35 * pit - 0.25 * joint + 0.25 * (stain - 0.5));
            return new NocTexel
            {
                h = NocMath.Clamp01(h + 0.2), r = a * (1.0 + 0.03 * (stain - 0.5)), g = a, b = a * (1.0 - 0.03 * (stain - 0.5)),
                metal = 0, ao = 1.0 - 0.5 * pit - 0.4 * joint, smooth = NocMath.Clamp01(0.45 + 0.7 * (Fbm(u, v, 8, 4, 15) - 0.5) - 0.3 * pit),
            };
        }

        // Wet stone pavers, 2.4 m tile: 4 x 4 slabs of 0.6 m, running bond, 8 mm joints, domed tops, per-slab tone and polish.
        static NocTexel Paving(double u, double v)
        {
            Blocks(u, v, 4, 4, true, out double jx, out double jy, out int bx, out int by);
            double jm = Math.Min(jx, jy) * 2.4;                               // m to the nearest joint
            double joint = 1.0 - Smooth(0.003, 0.007, jm);
            double bevel = Smooth(0.003, 0.02, jm);
            uint id = NocMath.Pcg3(bx, by, 21);
            double tone = NocMath.U01(id) - 0.5, polish = NocMath.U01(NocMath.Pcg(id)) - 0.5;
            double grain = Fbm(u, v, 64, 3, 22);
            double low = Fbm(u, v, 8, 3, 23);
            double h = 0.65 * bevel + 0.12 * grain + 0.1 * low;
            double a = 0.5 * (1.0 + 0.24 * tone + 0.25 * (grain - 0.5) + 0.12 * (low - 0.5) - 0.45 * joint);
            return new NocTexel
            {
                h = NocMath.Clamp01(h), r = a, g = a * (1.0 + 0.02 * tone), b = a * (1.0 + 0.04 * tone),
                metal = 0, ao = 1.0 - 0.6 * joint, smooth = NocMath.Clamp01(0.55 + 0.5 * polish + 0.4 * (low - 0.5) - 0.5 * joint),
            };
        }

        // Asphalt, 2 m tile: aggregate stones (Worley, ~1.5 cm), dark binder, a few hairline cracks (ridged), polished wheel wear.
        static NocTexel Asphalt(double u, double v)
        {
            Worley(u, v, 128, 31, out double f1, out double f2, out uint id);
            double stone = Smooth(0.05, 0.25, f2 - f1);                         // inside a stone
            double light = NocMath.U01(id);
            double binder = Fbm(u, v, 16, 4, 32);
            double crack = Smooth(0.93, 0.985, Ridged(u, v, 3, 3, 3, 33)) * Smooth(0.55, 0.7, Fbm(u, v, 2, 2, 34));
            double h = 0.55 * stone * (0.6 + 0.4 * light) + 0.3 * binder - 0.5 * crack;
            double a = 0.5 * (1.0 + 0.45 * stone * (light - 0.35) + 0.2 * (binder - 0.5) - 0.5 * crack);
            return new NocTexel
            {
                h = NocMath.Clamp01(h + 0.15), r = a, g = a * 0.99, b = a * 1.02,
                metal = 0, ao = 1.0 - 0.35 * (1.0 - stone) - 0.4 * crack, smooth = NocMath.Clamp01(0.35 + 0.35 * stone * light + 0.4 * (binder - 0.5) - 0.3 * crack),
            };
        }

        // Steel deck, 2.4 m tile: 2 x 1 plates (1.2 x 2.4 m) with welded seams, raised lozenge tread (3 cm pitch) on painted plate.
        static NocTexel SteelDeck(double u, double v)
        {
            Blocks(u, v, 2, 1, false, out double jx, out double jy, out int bx, out _);
            double jm = Math.Min(jx, jy) * 2.4;
            double seam = 1.0 - Smooth(0.004, 0.012, jm);
            double weld = Math.Exp(-Math.Pow(jm / 0.006, 2)) * (0.6 + 0.4 * Fbm(u, v, 160, 2, 41));
            // lozenge tread: two crossed lattices of 80 per tile (3 cm pitch)
            double a1 = (u + v) * 80.0, a2 = (u - v) * 80.0;
            double d1 = Math.Abs(a1 - Math.Round(a1)), d2 = Math.Abs(a2 - Math.Round(a2));
            double tread = Smooth(0.28, 0.42, Math.Max(d1, d2)) * (1.0 - Smooth(0.42, 0.5, Math.Max(d1, d2)));
            double wear = Fbm(u, v, 6, 4, 42);
            double rust = Smooth(0.6, 0.8, Fbm(u, v, 12, 4, 43)) * (0.3 + 0.7 * seam);
            double h = 0.4 + 0.35 * tread * (1.0 - 0.6 * Smooth(0.5, 0.8, wear)) - 0.3 * seam + 0.2 * weld;
            double tone = NocMath.U01(NocMath.Pcg3(bx, 0, 44)) - 0.5;
            double a = 0.5 * (1.0 + 0.15 * tone + 0.25 * (wear - 0.5) + 0.25 * tread * Smooth(0.5, 0.8, wear) - 0.3 * seam);
            return new NocTexel
            {
                h = NocMath.Clamp01(h), r = a * (1.0 + 0.35 * rust), g = a * (1.0 + 0.05 * rust), b = a * (1.0 - 0.25 * rust),
                metal = 0, ao = 1.0 - 0.5 * seam, smooth = NocMath.Clamp01(0.5 + 0.5 * (wear - 0.5) + 0.25 * tread - 0.4 * rust),
            };
        }

        // Plaza granite, 4.8 m tile: 4 x 4 slabs of 1.2 m (stack bond), 5 mm joints, speckled granite (Worley grains, no sparkle).
        static NocTexel PlazaStone(double u, double v)
        {
            Blocks(u, v, 4, 4, false, out double jx, out double jy, out int bx, out int by);
            double jm = Math.Min(jx, jy) * 4.8;
            double joint = 1.0 - Smooth(0.002, 0.005, jm);
            double bevel = Smooth(0.002, 0.012, jm);
            Worley(u, v, 256, 51, out double f1, out _, out uint gid);
            double grain = NocMath.U01(gid);
            double speck = Smooth(0.15, 0.05, f1) * (grain > 0.8 ? 1.0 : (grain < 0.2 ? -1.0 : 0.0));
            double cloud = Fbm(u, v, 12, 4, 52);
            uint id = NocMath.Pcg3(bx, by, 53);
            double tone = NocMath.U01(id) - 0.5;
            double h = 0.7 * bevel + 0.08 * cloud + 0.04 * speck;
            double a = 0.5 * (1.0 + 0.16 * tone + 0.2 * (cloud - 0.5) + 0.22 * speck - 0.45 * joint);
            return new NocTexel
            {
                h = NocMath.Clamp01(h), r = a * (1.0 + 0.02 * tone), g = a, b = a * (1.0 + 0.03 * (cloud - 0.5)),
                metal = 0, ao = 1.0 - 0.6 * joint, smooth = NocMath.Clamp01(0.6 + 0.45 * (cloud - 0.5) + 0.3 * tone - 0.5 * joint),
            };
        }

        // Metal panels, 2 m tile: 1 m panels with seams, faint brushing along u, scuffs; metallic everywhere except the seams.
        static NocTexel Metal(double u, double v)
        {
            Blocks(u, v, 2, 2, false, out double jx, out double jy, out _, out _);
            double jm = Math.Min(jx, jy) * 2.0;
            double seam = 1.0 - Smooth(0.0015, 0.004, jm);
            double brush = FbmA(u, v, 8, 256, 2, 61);                              // stretched along u (brushed lines)
            double scuff = Smooth(0.62, 0.8, Fbm(u, v, 10, 5, 62));
            double low = Fbm(u, v, 4, 4, 63);
            double h = 0.5 + 0.1 * (brush - 0.5) - 0.45 * seam - 0.1 * scuff;
            double a = 0.5 * (1.0 + 0.12 * (low - 0.5) + 0.06 * (brush - 0.5) + 0.15 * scuff - 0.3 * seam);
            return new NocTexel
            {
                h = NocMath.Clamp01(h), r = a, g = a, b = a,
                metal = 1.0 - seam, ao = 1.0 - 0.5 * seam, smooth = NocMath.Clamp01(0.55 + 0.35 * (low - 0.5) + 0.2 * (brush - 0.5) - 0.4 * scuff),
            };
        }

        // Moss, 1.5 m tile: cushions (inverted Worley) with a fibrous fBm, greener tips and darker hollows.
        static NocTexel Moss(double u, double v)
        {
            Worley(u, v, 24, 71, out double f1, out _, out uint id);
            double cushion = 1.0 - Smooth(0.0, 0.9, f1);
            double fib = Fbm(u, v, 96, 3, 72);
            double low = Fbm(u, v, 4, 4, 73);
            double h = 0.6 * cushion + 0.3 * fib + 0.1 * low;
            double tip = NocMath.Clamp01(h);
            double a = 0.5 * (0.65 + 0.7 * tip);
            double hue = NocMath.U01(id) - 0.5;
            return new NocTexel
            {
                h = tip, r = a * (1.0 + 0.15 * hue), g = a * (1.0 + 0.1 * (tip - 0.5)), b = a * (1.0 - 0.1 * hue),
                metal = 0, ao = 0.55 + 0.45 * tip, smooth = NocMath.Clamp01(0.35 + 0.5 * (low - 0.5) + 0.3 * (1.0 - tip)),
            };
        }

        // Bark, 1 m tile: vertical fibrous ridges (ridged noise stretched along v) with cracks between the plates.
        static NocTexel Bark(double u, double v)
        {
            double ridges = Ridged(u, v, 12, 2, 4, 81);                            // fibres run along v
            double plates = Fbm(u, v, 6, 3, 82);
            double crack = 1.0 - Smooth(0.25, 0.5, ridges);
            double h = 0.65 * ridges + 0.35 * plates;
            double a = 0.5 * (0.75 + 0.5 * ridges - 0.35 * crack + 0.2 * (plates - 0.5));
            return new NocTexel
            {
                h = NocMath.Clamp01(h), r = a * 1.04, g = a, b = a * 0.96,
                metal = 0, ao = 1.0 - 0.6 * crack, smooth = NocMath.Clamp01(0.3 + 0.4 * (plates - 0.5) + 0.4 * crack),
            };
        }
    }
}
