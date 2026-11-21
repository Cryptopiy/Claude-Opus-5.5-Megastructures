// BwTexGen: the ONE procedural texture generator of Bulwark (MASTER_PROMPT 8.8, 8.11.2-8.11.4, 8.11.6, 8.11.9). Pure C#
// (no editor API, no Unity objects, deterministic, thread-safe), so EditMode tests can check it and Editor/TextureFactory
// can encode its output as texture assets at edit time (RebuildAll -> MaterialFactory.Build -> TextureFactory.EnsureAll).
//
// Noise toolbox, all PERIODIC on the unit tile (integer lattice periods) so every map tiles exactly:
//   Value / Grad (Perlin gradient) noise, Fbm (isotropic and anisotropic periods), Ridged, Worley / Voronoi (F1, F2, cell
//   id, nearest offset), DomainWarp, Curl (divergence-free field of an fBm potential) and a box Blur.
// Recipes (scale ladder of 8.11.3: micro mm-cm, meso cm-m; the macro m-to-tens-of-m layer is in the shaders, keyed to world
// position, so it never tiles):
//   Concrete (2048 px per 2.0 m tile, ~1 mm/texel): bug holes (Worley pits), aggregate cells, plywood formwork grain,
//            meso undulation, cement-paste micro speckle.             albedo ref ConcreteMid
//   Road     (1024 px per 1.0 m): broom-finished concrete pavement: transverse broom grooves (anisotropic ridged fBm),
//            exposed aggregate, wear polish.                           albedo ref ConcreteMid
//   Steel    (1024 px per 1.0 m): weathered industrial paint: orange peel, mottle, chalking streaks meandering on curl
//            noise, bare-metal chip potential (Mask R).                albedo ref SteelPaint
//   Crust    (1024 px per 0.5 m): barnacle crust: two Worley generations (36 mm and 14 mm cells) of volcano-shaped shells
//            with craters and plate ridges, UNIFORM over the tile (no baked colonies: their 0.5 m period read as a lattice
//            at 45-140 m); Mask R = the random rank of the nearest shell, so BwArmour thins the shells by a world-space
//            colony density.
//                                                                       absolute palette lerps (no recolour)
// Output per set (level 0 in linear float, then mips): albedo (linear RGB, encoded sRGB by the factory), tangent normal
// derived from the height (tileable central differences, metres), Mask = (metallic potential, occlusion, height, smoothness).
// Mips: 2 x 2 box; normals averaged without renormalisation to measure the Toksvig / vMF variance of the covered level-0
// normals; smoothness per mip = URP core NormalFiltering form: roughness^2' = mean(roughness^2) + min(2 var, 0.5^2) with
// var = 1 / (4 kappa), kappa = (3 L - L^3) / (1 - L^2) (CommonMaterial.hlsl TextureNormalVariance). That is the "no sparkle"
// rule of 8.11.6 for the texture detail; the shader adds the geometric term.
// Colours come only from BulwarkPalette (no literal colours).
using System;
using System.Threading.Tasks;
using UnityEngine;

namespace Megastructure.Rendering
{
    /// <summary>One generated set, level 0 in linear floats (size x size, row-major, row 0 = v 0 = texture bottom).</summary>
    public sealed class TexSet
    {
        public string Name;
        public int Size;
        public float TileMetres;          // world size of one tile
        public float HeightRangeMetres;   // height 0..1 spans this many metres (normal derivation)
        public Pal AlbedoRef;             // palette token the albedo is generated around (shader recolours by target / ref)
        public float[] R, G, B;           // albedo, linear
        public float[] Height;            // 0..1
        public float[] Metal, Ao, Smooth; // 0..1
        public float[] Nx, Ny, Nz;        // unit tangent normal (filled by DeriveNormals)

        public TexSet(string name, int size, float tile, float heightRange, Pal albedoRef)
        {
            Name = name; Size = size; TileMetres = tile; HeightRangeMetres = heightRange; AlbedoRef = albedoRef;
            int n = size * size;
            R = new float[n]; G = new float[n]; B = new float[n]; Height = new float[n];
            Metal = new float[n]; Ao = new float[n]; Smooth = new float[n];
            Nx = new float[n]; Ny = new float[n]; Nz = new float[n];
        }

        public float TexelMetres => TileMetres / Size;
    }

    /// <summary>One mip level, encoded 8-bit RGBA (albedo sRGB, normal RG, mask RGBA linear).</summary>
    public sealed class TexMip
    {
        public int Size;
        public byte[] Albedo, Normal, Mask;   // RGBA32 each, row-major from the bottom row
        public float MeanNormalLength;        // mean |average normal| over the level (Toksvig diagnostics)
        public float MeanSmoothness;          // mean encoded smoothness (tests: monotonic non-increasing)
    }

    /// <summary>
    /// C# twin of the BwLitCore.hlsl triplanar plane coordinates (MaterialTests): Plane = position / scale on the plane's
    /// axes, UNWRAPPED; Detune = the rotated / rescaled copy (k R uv + offset); Wrap64 = the wrap applied at the sample.
    /// The sampled texel is frac(Wrap64(uv)) because the sets tile per unit.
    /// </summary>
    public static class BwTriTwin
    {
        public static void Plane(double px, double py, double pz, double scale, int axis, out double u, out double v)
        {
            double x = px / scale, y = py / scale, z = pz / scale;
            if (axis == 0) { u = z; v = y; } else if (axis == 1) { u = x; v = z; } else { u = x; v = y; }
        }

        public static void Detune(double u, double v, double k, double ou, double ov, out double du, out double dv)
        {
            // float2x2(0.8, -0.6, 0.6, 0.8) * k, mul(R, uv): rows (0.8, -0.6) and (0.6, 0.8)
            du = k * (0.8 * u - 0.6 * v) + ou;
            dv = k * (0.6 * u + 0.8 * v) + ov;
        }

        public static double Wrap64(double a) => a - 64.0 * Math.Floor(a / 64.0);

        /// <summary>The pre-fix form (wrap the plane coordinates, then rotate): jumps at every 64-tile line.</summary>
        public static void DetuneWrappedFirst(double u, double v, double k, double ou, double ov, out double du, out double dv)
            => Detune(Wrap64(u), Wrap64(v), k, ou, ov, out du, out dv);
    }

    public static class BwTexGen
    {
        /// <summary>Bump whenever a recipe or the encoding changes: TextureFactory regenerates the assets.</summary>
        public const int Version = 15;

        public const int ConcreteSize = 2048, RoadSize = 1024, SteelSize = 1024, CrustSize = 1024;
        public const float ConcreteTile = 2.0f, RoadTile = 1.0f, SteelTile = 1.0f, CrustTile = 0.5f;
        /// <summary>Largest roughness added by the Toksvig term (squared: 0.25).</summary>
        public const float ToksvigThreshold = 0.5f;

        public static readonly string[] SetNames = { "Concrete", "Road", "Steel", "Crust" };

        // ================================================================ hashing and lattice noise (periodic)

        static int Mod(int a, int p) { int m = a % p; return m < 0 ? m + p : m; }

        static uint H(int x, int y, uint seed) { unchecked { return BwMath.Pcg(BwMath.Pcg2(x, y) ^ (seed * 0x9E3779B9u)); } }

        public static float Hash01(int x, int y, uint seed) => BwMath.U01(H(x, y, seed));

        static readonly float[] s_gx = new float[16], s_gy = new float[16];
        static BwTexGen()
        {
            for (int i = 0; i < 16; i++)
            {
                double a = (i + 0.5) * 2.0 * Math.PI / 16.0;
                s_gx[i] = (float)Math.Cos(a);
                s_gy[i] = (float)Math.Sin(a);
            }
        }

        static float Fade(float t) => t * t * t * (t * (t * 6f - 15f) + 10f);

        /// <summary>Periodic value noise in [0, 1]; x, y in lattice cells, periods px, py in cells.</summary>
        public static float Value(float x, float y, int px, int py, uint seed)
        {
            int x0 = (int)Math.Floor(x), y0 = (int)Math.Floor(y);
            float tx = x - x0, ty = y - y0, u = Fade(tx), v = Fade(ty);
            int xa = Mod(x0, px), xb = Mod(x0 + 1, px), ya = Mod(y0, py), yb = Mod(y0 + 1, py);
            float a = Hash01(xa, ya, seed), b = Hash01(xb, ya, seed), c = Hash01(xa, yb, seed), d = Hash01(xb, yb, seed);
            return a + (b - a) * u + (c - a) * v + (a - b - c + d) * u * v;
        }

        /// <summary>Periodic gradient (Perlin) noise, about [-1, 1]; x, y in lattice cells, periods in cells.</summary>
        public static float Grad(float x, float y, int px, int py, uint seed)
        {
            int x0 = (int)Math.Floor(x), y0 = (int)Math.Floor(y);
            float tx = x - x0, ty = y - y0, u = Fade(tx), v = Fade(ty);
            int xa = Mod(x0, px), xb = Mod(x0 + 1, px), ya = Mod(y0, py), yb = Mod(y0 + 1, py);
            uint ha = H(xa, ya, seed) & 15u, hb = H(xb, ya, seed) & 15u, hc = H(xa, yb, seed) & 15u, hd = H(xb, yb, seed) & 15u;
            float na = s_gx[ha] * tx + s_gy[ha] * ty;
            float nb = s_gx[hb] * (tx - 1f) + s_gy[hb] * ty;
            float nc = s_gx[hc] * tx + s_gy[hc] * (ty - 1f);
            float nd = s_gx[hd] * (tx - 1f) + s_gy[hd] * (ty - 1f);
            float x1 = na + (nb - na) * u, x2 = nc + (nd - nc) * u;
            return 1.4142135f * (x1 + (x2 - x1) * v);
        }

        /// <summary>Anisotropic periodic fBm of gradient noise on the unit tile (u, v in tile units), about [-1, 1].
        /// Octave k has periods (px 2^k, py 2^k), so every octave tiles.</summary>
        public static float Fbm(float u, float v, int px, int py, int octaves, float gain, uint seed)
        {
            float sum = 0f, amp = 1f, norm = 0f;
            int ppx = px, ppy = py;
            for (int o = 0; o < octaves; o++)
            {
                sum += amp * Grad(u * ppx, v * ppy, ppx, ppy, seed + (uint)o * 101u);
                norm += amp;
                amp *= gain;
                ppx *= 2; ppy *= 2;
            }
            return sum / norm;
        }

        public static float Fbm(float u, float v, int period, int octaves, float gain, uint seed) => Fbm(u, v, period, period, octaves, gain, seed);

        /// <summary>Ridged multifractal style fBm in [0, 1]: (1 - |noise|)^2 per octave.</summary>
        public static float Ridged(float u, float v, int px, int py, int octaves, float gain, uint seed)
        {
            float sum = 0f, amp = 1f, norm = 0f;
            int ppx = px, ppy = py;
            for (int o = 0; o < octaves; o++)
            {
                float r = 1f - Math.Abs(Grad(u * ppx, v * ppy, ppx, ppy, seed + (uint)o * 131u));
                sum += amp * r * r;
                norm += amp;
                amp *= gain;
                ppx *= 2; ppy *= 2;
            }
            return sum / norm;
        }

        /// <summary>Periodic Worley / Voronoi on the unit tile with period x period jittered cells: F1, F2 (cell units), the
        /// wrapped cell index of the nearest point and the offset from the nearest point to (u, v) (cell units).</summary>
        public static void Worley(float u, float v, int period, float jitter, uint seed, out float f1, out float f2, out int cellId, out float ox, out float oy)
        {
            float x = u * period, y = v * period;
            int cx = (int)Math.Floor(x), cy = (int)Math.Floor(y);
            f1 = 1e9f; f2 = 1e9f; cellId = 0; ox = 0f; oy = 0f;
            for (int j = -1; j <= 1; j++)
                for (int i = -1; i <= 1; i++)
                {
                    int gx = cx + i, gy = cy + j;
                    int wx = Mod(gx, period), wy = Mod(gy, period);
                    uint h = H(wx, wy, seed);
                    float px = gx + 0.5f + jitter * (BwMath.U01(h) - 0.5f);
                    float py = gy + 0.5f + jitter * (BwMath.U01(BwMath.Pcg(h)) - 0.5f);
                    float dx = x - px, dy = y - py;
                    float d = (float)Math.Sqrt(dx * dx + dy * dy);
                    if (d < f1) { f2 = f1; f1 = d; cellId = wy * period + wx; ox = dx; oy = dy; }
                    else if (d < f2) f2 = d;
                }
        }

        /// <summary>Curl of a periodic fBm potential psi: (d psi / dv, -d psi / du), unit-tile units (divergence free).</summary>
        public static void Curl(float u, float v, int period, int octaves, uint seed, out float cu, out float cv)
        {
            float e = 0.25f / (period * (1 << Math.Max(0, octaves - 1)));
            float pu1 = Fbm(u + e, v, period, octaves, 0.5f, seed), pu0 = Fbm(u - e, v, period, octaves, 0.5f, seed);
            float pv1 = Fbm(u, v + e, period, octaves, 0.5f, seed), pv0 = Fbm(u, v - e, period, octaves, 0.5f, seed);
            float inv = 1f / (2f * e * period);
            cu = (pv1 - pv0) * inv;
            cv = -(pu1 - pu0) * inv;
        }

        /// <summary>Domain warp: (u, v) displaced by two decorrelated periodic fBm fields of amplitude amp (tile units).</summary>
        public static void DomainWarp(ref float u, ref float v, int period, int octaves, float amp, uint seed)
        {
            float wu = Fbm(u, v, period, octaves, 0.5f, seed);
            float wv = Fbm(u + 0.37f, v + 0.61f, period, octaves, 0.5f, seed + 977u);
            u += amp * wu;
            v += amp * wv;
        }

        static float Sat(float x) => x < 0f ? 0f : (x > 1f ? 1f : x);
        static float Lerp(float a, float b, float t) => a + (b - a) * t;
        static float Smooth(float e0, float e1, float x) { float t = Sat((x - e0) / (e1 - e0)); return t * t * (3f - 2f * t); }

        // ================================================================ recipes

        delegate void Texel(int x, int y, float u, float v, int i);

        static void ForEach(int size, Texel f)
        {
            Parallel.For(0, size, y =>
            {
                float v = (y + 0.5f) / size;
                for (int x = 0; x < size; x++) f(x, y, (x + 0.5f) / size, v, y * size + x);
            });
        }

        static void SetAlbedo(TexSet s, int i, Color c, float k)
        {
            s.R[i] = Math.Max(0f, c.r * k); s.G[i] = Math.Max(0f, c.g * k); s.B[i] = Math.Max(0f, c.b * k);
        }

        static Color Mix(Color a, Color b, float t) => new Color(Lerp(a.r, b.r, t), Lerp(a.g, b.g, t), Lerp(a.b, b.b, t), 1f);

        /// <summary>Weathered formwork concrete, ConcreteMid reference.</summary>
        public static TexSet Concrete(int size = ConcreteSize)
        {
            var s = new TexSet("Concrete", size, ConcreteTile, 0.004f, Pal.ConcreteMid);
            Color mid = BulwarkPalette.Linear(Pal.ConcreteMid), dark = BulwarkPalette.Linear(Pal.ConcreteDark);
            Color warm = Mix(mid, BulwarkPalette.Linear(Pal.SafetyOchre), 0.12f);
            Color cool = Mix(mid, dark, 0.35f);
            ForEach(size, (x, y, u, v, i) =>
            {
                // meso: 50 cm undulation of the pour, 14 cm paste mottling, plywood grain stretched along u
                float meso = Fbm(u, v, 4, 4, 0.5f, 11u);
                float mottle = Fbm(u, v, 14, 3, 0.55f, 17u);
                float grain = Fbm(u, v, 6, 96, 3, 0.55f, 23u);
                // micro: cement paste speckle
                float micro = Fbm(u, v, 384, 2, 0.5f, 37u);
                // aggregate (~18 mm stones) faintly through the paste skin: soft round stones, no cell edges
                Worley(u, v, 110, 0.9f, 41u, out float a1, out _, out int aid, out _, out _);
                float stone = Smooth(0.55f, 0.2f, a1);
                float aggTone = (Hash01(aid, 7, 43u) - 0.5f) * stone;
                // bug holes: 1.2-3.8 mm pits in ~31 mm cells, only inside low-frequency clusters (air trapped under lifts)
                float cluster = Smooth(0.1f, 0.45f, Fbm(u, v, 3, 3, 0.5f, 47u));
                Worley(u, v, 64, 0.85f, 53u, out float p1, out _, out int pid, out _, out _);
                float pr = Hash01(pid, 3, 59u);
                float holeR = pr < 0.30f * cluster ? Lerp(0.04f, 0.12f, Hash01(pid, 5, 61u)) : 0f;
                float hole = holeR > 0f ? Smooth(holeR, holeR * 0.5f, p1) : 0f;

                float h = 0.55f + 0.2f * meso + 0.06f * mottle + 0.05f * grain + 0.04f * micro + 0.02f * stone - 0.6f * hole;
                s.Height[i] = Sat(h);
                Color baseC = aggTone > 0f ? Mix(mid, warm, aggTone * 0.5f) : Mix(mid, cool, -aggTone * 0.5f);
                float tone = 1f + 0.08f * meso + 0.06f * mottle + 0.03f * micro + 0.015f * grain;
                SetAlbedo(s, i, baseC, tone * (1f - 0.3f * hole));
                s.Metal[i] = 0f;
                s.Ao[i] = 1f - 0.5f * hole;
                s.Smooth[i] = Sat(0.30f + 0.14f * meso + 0.08f * mottle - 0.03f * micro - 0.10f * hole);
            });
            Finish(s, 6, 0.9f);
            return s;
        }

        /// <summary>Broom-finished concrete pavement, ConcreteMid reference (u = across the road, v = along it).</summary>
        public static TexSet Road(int size = RoadSize)
        {
            var s = new TexSet("Road", size, RoadTile, 0.0025f, Pal.ConcreteMid);
            Color mid = BulwarkPalette.Linear(Pal.ConcreteMid), dark = BulwarkPalette.Linear(Pal.ConcreteDark);
            Color warm = Mix(mid, BulwarkPalette.Linear(Pal.SafetyOchre), 0.10f);
            ForEach(size, (x, y, u, v, i) =>
            {
                // transverse broom grooves: ridges along u, ~7 mm apart along v, with a slight wander
                float wu = u, wv = v;
                DomainWarp(ref wu, ref wv, 3, 2, 0.004f, 71u);
                float broom = Ridged(wu, wv, 2, 128, 2, 0.45f, 73u);
                float wear = 0.5f + 0.5f * Fbm(u, v, 3, 3, 0.5f, 79u);           // polish of the broom ridges
                Worley(u, v, 80, 0.9f, 83u, out float a1, out float a2, out int aid, out _, out _);
                float aggTone = Hash01(aid, 9, 89u) - 0.5f;
                float exposed = Smooth(0.05f, 0.0f, a1 - 0.18f) * Smooth(0.35f, 0.75f, wear);
                float stain = Fbm(u, v, 2, 3, 0.5f, 97u);
                float micro = Fbm(u, v, 256, 2, 0.5f, 101u);

                float h = 0.5f + 0.35f * (broom - 0.5f) * (1f - 0.6f * wear) + 0.12f * exposed + 0.05f * micro;
                s.Height[i] = Sat(h);
                Color c = aggTone > 0f ? Mix(mid, warm, aggTone) : Mix(mid, dark, -aggTone * 0.6f);
                float tone = 1f + 0.06f * stain + 0.03f * micro - 0.04f * (broom - 0.5f) + 0.05f * exposed;
                SetAlbedo(s, i, c, tone);
                s.Metal[i] = 0f;
                s.Ao[i] = 1f;
                s.Smooth[i] = Sat(0.16f + 0.28f * wear + 0.08f * stain + 0.06f * exposed);
            });
            Finish(s, 4, 0.7f);
            return s;
        }

        /// <summary>Weathered industrial paint on steel, SteelPaint reference; Mask R = bare-metal chip potential.</summary>
        public static TexSet Steel(int size = SteelSize)
        {
            var s = new TexSet("Steel", size, SteelTile, 0.0006f, Pal.SteelPaint);
            Color paint = BulwarkPalette.Linear(Pal.SteelPaint);
            Color chalk = Mix(paint, BulwarkPalette.Linear(Pal.ConcreteMid), 0.35f);
            Color grime = Mix(paint, BulwarkPalette.Linear(Pal.ConcreteDark), 0.45f);
            ForEach(size, (x, y, u, v, i) =>
            {
                float peel = Fbm(u, v, 160, 2, 0.5f, 131u);
                float mottle = Fbm(u, v, 6, 4, 0.55f, 137u);
                // chalking streaks run down (v), meandering on a curl field, sparse and soft
                Curl(u, v, 4, 2, 139u, out float cu, out _);
                float streak = Fbm(u + 0.02f * cu, v, 20, 2, 3, 0.5f, 149u);
                float streakMask = Smooth(0.25f, 0.65f, streak) * Smooth(-0.1f, 0.4f, Fbm(u, v, 2, 2, 0.5f, 145u));
                // chip potential: blotchy domain-warped fBm (flakes, no cell edges)
                float wu = u, wv = v;
                DomainWarp(ref wu, ref wv, 4, 3, 0.035f, 155u);
                float chip = Sat(0.5f + 0.62f * Fbm(wu, wv, 10, 5, 0.6f, 151u));
                float grimeT = Smooth(0.2f, 0.7f, Fbm(u, v, 3, 3, 0.5f, 163u));

                s.Height[i] = Sat(0.5f + 0.35f * peel + 0.15f * mottle);
                Color c = Mix(Mix(paint, chalk, 0.35f * streakMask), grime, 0.25f * grimeT);
                SetAlbedo(s, i, c, 1f + 0.05f * mottle + 0.015f * peel);
                s.Metal[i] = chip;
                s.Ao[i] = 1f;
                s.Smooth[i] = Sat(0.52f + 0.16f * mottle - 0.10f * streakMask - 0.06f * grimeT);
            });
            Finish(s, 3, 0.5f);
            return s;
        }

        /// <summary>Barnacle crust on armour concrete (absolute palette lerps).</summary>
        public static TexSet Crust(int size = CrustSize)
        {
            var s = new TexSet("Crust", size, CrustTile, 0.012f, Pal.ConcreteDark);
            Color shell = Mix(BulwarkPalette.Linear(Pal.ConcreteMid), BulwarkPalette.Linear(Pal.FoamWhite), 0.12f);
            Color shellOld = Mix(shell, BulwarkPalette.Linear(Pal.ConcreteDark), 0.55f);
            Color film = Mix(BulwarkPalette.Linear(Pal.ConcreteWet), BulwarkPalette.Linear(Pal.SeaTeal), 0.12f);
            Color crater = BulwarkPalette.Linear(Pal.ConcreteWet);
            ForEach(size, (x, y, u, v, i) =>
            {
                float filmN = Fbm(u, v, 8, 4, 0.55f, 211u);
                const float colony = 1f;   // uniform: colony density is world-space in BwArmour (no tile-periodic colonies)
                // large shells (~36 mm cells)
                Worley(u, v, 14, 0.75f, 223u, out float f1, out _, out int id, out float ox, out float oy);
                bool live = Hash01(id, 1, 227u) < 0.95f * colony;
                float rc = Lerp(0.30f, 0.47f, Hash01(id, 2, 229u));
                float body = live ? Smooth(rc, rc * 0.52f, f1) : 0f;
                float hole = live ? 0.75f * Smooth(rc * 0.36f, rc * 0.12f, f1) : 0f;   // wide, soft orifice: never a 1 px extreme
                float ang = (float)Math.Atan2(oy, ox);
                float plates = 0.5f + 0.5f * (float)Math.Cos(6f * ang + 6.2831853f * Hash01(id, 3, 233u));
                float age = Hash01(id, 4, 239u);
                // small shells (~14 mm) in the gaps
                Worley(u, v, 36, 0.85f, 241u, out float g1, out _, out int gid, out float gox, out float goy);
                float rs = Lerp(0.25f, 0.42f, Hash01(gid, 5, 251u));
                bool liveS = Hash01(gid, 6, 257u) < 0.85f * colony;
                // a small shell only grows clear of the large ones (its centre outside a live large shell plus half its own
                // radius): no overlaps, so the nearest-shell rank never cuts through a shell (crescents, slivers)
                if (liveS)
                {
                    float su = u - gox / 36f, sv = v - goy / 36f;
                    Worley(su, sv, 14, 0.75f, 223u, out float bf1, out _, out int bid, out _, out _);
                    float rcB = Lerp(0.30f, 0.47f, Hash01(bid, 2, 229u));
                    if (Hash01(bid, 1, 227u) < 0.95f * colony && bf1 < rcB + 0.5f * rs * (14f / 36f)) liveS = false;
                }
                float small = liveS ? Smooth(rs, rs * 0.5f, g1) * (1f - body) : 0f;
                float smallHole = liveS ? Smooth(rs * 0.3f, rs * 0.12f, g1) * (1f - body) : 0f;

                float h = 0.18f + 0.06f * filmN + 0.72f * body * (0.85f + 0.15f * plates) - 0.18f * hole + 0.35f * small - 0.08f * smallHole;
                s.Height[i] = Sat(h);
                float shellAmt = Sat(body + small);
                Color sc = Mix(shell, shellOld, age);
                Color c = Mix(film, sc, shellAmt);
                c = Mix(c, crater, Sat(hole + 0.3f * smallHole) * 0.2f);   // orifices soft and shallow (2 mm deep): a 4 mm hole is 1-2 px at 1.5-3 m
                SetAlbedo(s, i, c, 1f + 0.08f * filmN * (1f - shellAmt) + 0.05f * (plates - 0.5f) * shellAmt);
                // rank in [0.02, 1) of the NEAREST shell (distance in shell radii), on the shell and on the film around it:
                // constant per shell neighbourhood, so a mip never blends a shell's rim with another rank (the per-texel
                // shell / film form drew thin rings and isolated pixels where a hidden shell's rim survived the threshold).
                // BwArmour shows a shell where its world colony density exceeds the rank.
                // Large shells rank in [0.02, 0.7), small ones in [0.4, 1): sparse colonies keep the large shells (a lone 15 mm
                // shell on bare film would be a 1-2 px dot at 3 m), dense ones fill the gaps with small shells.
                float dBig = live ? f1 / rc : 1e9f, dSmall = liveS ? g1 / rs : 1e9f;
                float rank = dBig <= dSmall ? Lerp(0.02f, 0.7f, Hash01(id, 7, 263u)) : Lerp(0.4f, 1f, Hash01(gid, 8, 269u));
                s.Metal[i] = rank;
                s.Ao[i] = 1f - 0.28f * hole - 0.1f * smallHole;
                s.Smooth[i] = Sat(Lerp(0.36f + 0.06f * filmN, 0.2f, shellAmt) - 0.1f * hole);
            });
            Finish(s, 8, 0.8f);
            return s;
        }

        public static TexSet Generate(string name, int sizeOverride = 0)
        {
            switch (name)
            {
                case "Concrete": return Concrete(sizeOverride > 0 ? sizeOverride : ConcreteSize);
                case "Road": return Road(sizeOverride > 0 ? sizeOverride : RoadSize);
                case "Steel": return Steel(sizeOverride > 0 ? sizeOverride : SteelSize);
                case "Crust": return Crust(sizeOverride > 0 ? sizeOverride : CrustSize);
                default: throw new ArgumentException("unknown texture set " + name);
            }
        }

        // ================================================================ derivation

        /// <summary>Normals from the height and a cavity term from a box-blurred height (AO multiplied in).</summary>
        static void Finish(TexSet s, int cavityRadiusPx, float cavityStrength)
        {
            DeriveNormals(s);
            if (cavityRadiusPx <= 0) return;
            float[] blur = Blur(s.Height, s.Size, cavityRadiusPx);
            int n = s.Size * s.Size;
            for (int i = 0; i < n; i++)
            {
                float cav = Math.Max(0f, blur[i] - s.Height[i]);
                s.Ao[i] = Sat(s.Ao[i] * (1f - cavityStrength * Sat(cav * 4f)));
            }
        }

        /// <summary>Tileable central differences: slope = dh * HeightRange / (2 texel), n = normalize(-sx, -sy, 1).</summary>
        public static void DeriveNormals(TexSet s)
        {
            int size = s.Size;
            float k = s.HeightRangeMetres / (2f * s.TexelMetres);
            Parallel.For(0, size, y =>
            {
                int ym = (y - 1 + size) % size, yp = (y + 1) % size;
                for (int x = 0; x < size; x++)
                {
                    int xm = (x - 1 + size) % size, xp = (x + 1) % size;
                    float sx = (s.Height[y * size + xp] - s.Height[y * size + xm]) * k;
                    float sy = (s.Height[yp * size + x] - s.Height[ym * size + x]) * k;
                    float inv = 1f / (float)Math.Sqrt(sx * sx + sy * sy + 1f);
                    int i = y * size + x;
                    s.Nx[i] = -sx * inv; s.Ny[i] = -sy * inv; s.Nz[i] = inv;
                }
            });
        }

        /// <summary>Separable periodic box blur of radius r (2r + 1 taps per axis).</summary>
        public static float[] Blur(float[] src, int size, int r)
        {
            var tmp = new float[src.Length];
            var dst = new float[src.Length];
            float inv = 1f / (2 * r + 1);
            Parallel.For(0, size, y =>
            {
                float acc = 0f;
                for (int k = -r; k <= r; k++) acc += src[y * size + Mod(k, size)];
                for (int x = 0; x < size; x++)
                {
                    tmp[y * size + x] = acc * inv;
                    acc += src[y * size + Mod(x + r + 1, size)] - src[y * size + Mod(x - r, size)];
                }
            });
            Parallel.For(0, size, x =>
            {
                float acc = 0f;
                for (int k = -r; k <= r; k++) acc += tmp[Mod(k, size) * size + x];
                for (int y = 0; y < size; y++)
                {
                    dst[y * size + x] = acc * inv;
                    acc += tmp[Mod(y + r + 1, size) * size + x] - tmp[Mod(y - r, size) * size + x];
                }
            });
            return dst;
        }

        // ================================================================ mips and encoding

        /// <summary>vMF variance of the level-0 normals whose mean has length L (URP core TextureNormalVariance).</summary>
        public static float NormalVariance(float meanLength)
        {
            if (meanLength >= 0.9999f) return 0f;
            float l = Math.Max(meanLength, 1e-3f), l2 = l * l;
            float kappa = (3f * l - l * l2) / Math.Max(1f - l2, 1e-6f);
            return 0.25f / Math.Max(kappa, 1e-6f);
        }

        /// <summary>Smoothness after adding the normal variance (URP core NormalFiltering, threshold ToksvigThreshold).</summary>
        public static float FilterSmoothness(float meanRoughness2, float variance)
        {
            float r2 = Sat(meanRoughness2 + Math.Min(2f * variance, ToksvigThreshold * ToksvigThreshold));
            return 1f - (float)Math.Sqrt(Math.Sqrt(r2));   // roughness = sqrt(r2); perceptual = sqrt(roughness)
        }

        public static float SmoothToRoughness2(float smoothness) { float pr = 1f - smoothness; float a = pr * pr; return a * a; }

        static byte B8(float x) => (byte)Math.Max(0, Math.Min(255, (int)Math.Round(x * 255f)));
        static byte Srgb8(float lin) => B8((float)BulwarkPalette.LinearToSrgb(Math.Max(0.0, Math.Min(1.0, lin))));

        /// <summary>The full mip chain (level 0 = Size, down to 1 x 1), encoded RGBA32 per map.</summary>
        public static TexMip[] BuildMips(TexSet s)
        {
            int levels = 1;
            for (int w = s.Size; w > 1; w >>= 1) levels++;
            var mips = new TexMip[levels];
            int size = s.Size;
            int n0 = size * size;
            // running level arrays (linear sums averaged per level)
            float[] r = (float[])s.R.Clone(), g = (float[])s.G.Clone(), b = (float[])s.B.Clone();
            float[] nx = (float[])s.Nx.Clone(), ny = (float[])s.Ny.Clone(), nz = (float[])s.Nz.Clone();
            float[] me = (float[])s.Metal.Clone(), ao = (float[])s.Ao.Clone(), he = (float[])s.Height.Clone();
            float[] r2 = new float[n0];
            for (int i = 0; i < n0; i++) r2[i] = SmoothToRoughness2(s.Smooth[i]);
            for (int l = 0; l < levels; l++)
            {
                mips[l] = Encode(size, r, g, b, nx, ny, nz, me, ao, he, r2);
                if (size == 1) break;
                int h = size >> 1;
                r = Down(r, size); g = Down(g, size); b = Down(b, size);
                nx = Down(nx, size); ny = Down(ny, size); nz = Down(nz, size);
                me = Down(me, size); ao = Down(ao, size); he = Down(he, size); r2 = Down(r2, size);
                size = h;
            }
            return mips;
        }

        static float[] Down(float[] src, int size)
        {
            int h = size >> 1;
            var dst = new float[h * h];
            for (int y = 0; y < h; y++)
                for (int x = 0; x < h; x++)
                {
                    int a = (2 * y) * size + 2 * x, c = (2 * y + 1) * size + 2 * x;
                    dst[y * h + x] = 0.25f * (src[a] + src[a + 1] + src[c] + src[c + 1]);
                }
            return dst;
        }

        static TexMip Encode(int size, float[] r, float[] g, float[] b, float[] nx, float[] ny, float[] nz, float[] me, float[] ao, float[] he, float[] r2)
        {
            int n = size * size;
            var m = new TexMip { Size = size, Albedo = new byte[n * 4], Normal = new byte[n * 4], Mask = new byte[n * 4] };
            double sumL = 0, sumS = 0;
            for (int i = 0; i < n; i++)
            {
                int o = i * 4;
                m.Albedo[o] = Srgb8(r[i]); m.Albedo[o + 1] = Srgb8(g[i]); m.Albedo[o + 2] = Srgb8(b[i]); m.Albedo[o + 3] = 255;
                float len = (float)Math.Sqrt(nx[i] * nx[i] + ny[i] * ny[i] + nz[i] * nz[i]);
                float inv = len > 1e-6f ? 1f / len : 0f;
                float ux = nx[i] * inv, uy = ny[i] * inv;
                m.Normal[o] = B8(ux * 0.5f + 0.5f); m.Normal[o + 1] = B8(uy * 0.5f + 0.5f); m.Normal[o + 2] = 255; m.Normal[o + 3] = 255;
                float sm = FilterSmoothness(r2[i], NormalVariance(Math.Min(1f, len)));
                m.Mask[o] = B8(me[i]); m.Mask[o + 1] = B8(ao[i]); m.Mask[o + 2] = B8(he[i]); m.Mask[o + 3] = B8(sm);
                sumL += len; sumS += sm;
            }
            m.MeanNormalLength = (float)(sumL / n);
            m.MeanSmoothness = (float)(sumS / n);
            return m;
        }

        /// <summary>Which encoded map a mip chain is rebuilt for (MipChainFromLevel0).</summary>
        public enum MipMap { Albedo, Normal, Mask }

        /// <summary>
        /// The mip chain of ONE encoded map rebuilt from its 8-bit level 0 (RGBA32, row-major from the bottom row), exactly
        /// as BuildMips makes it from the float set: albedo averaged in linear light and re-encoded sRGB, normals averaged
        /// unnormalised and renormalised, the mask's metal / AO / height box-averaged and its smoothness Toksvig-filtered per
        /// level from roughness^2 and the mean normal length of the level (the Normal map's level 0, needed for Mask only).
        /// Level 0 round-trips bit-exactly. Used by the PNG importer (TextureFactory): the PNG holds level 0, the importer's
        /// own mips are replaced by these. Returns one RGBA32 array per level (Size, Size/2, .. 1).
        /// </summary>
        public static byte[][] MipChainFromLevel0(int size, MipMap map, byte[] level0, byte[] normalLevel0 = null)
        {
            int levels = 1;
            for (int w = size; w > 1; w >>= 1) levels++;
            var outp = new byte[levels][];
            int n = size * size;
            float[] a = new float[n], b = new float[n], c = new float[n], r2 = null;
            float[] nx = null, ny = null, nz = null;
            bool needN = map == MipMap.Normal || map == MipMap.Mask;
            if (needN)
            {
                byte[] src = map == MipMap.Normal ? level0 : normalLevel0;
                if (src == null || src.Length < n * 4) throw new ArgumentException("the Mask chain needs the Normal map's level 0");
                nx = new float[n]; ny = new float[n]; nz = new float[n];
                for (int i = 0; i < n; i++)
                {
                    float ux = src[i * 4] / 255f * 2f - 1f, uy = src[i * 4 + 1] / 255f * 2f - 1f;
                    nx[i] = ux; ny[i] = uy; nz[i] = (float)Math.Sqrt(Math.Max(0f, 1f - ux * ux - uy * uy));
                }
            }
            if (map == MipMap.Albedo)
                for (int i = 0; i < n; i++)
                {
                    a[i] = (float)BulwarkPalette.SrgbToLinear(level0[i * 4] / 255.0);
                    b[i] = (float)BulwarkPalette.SrgbToLinear(level0[i * 4 + 1] / 255.0);
                    c[i] = (float)BulwarkPalette.SrgbToLinear(level0[i * 4 + 2] / 255.0);
                }
            else if (map == MipMap.Mask)
            {
                r2 = new float[n];
                for (int i = 0; i < n; i++)
                {
                    a[i] = level0[i * 4] / 255f; b[i] = level0[i * 4 + 1] / 255f; c[i] = level0[i * 4 + 2] / 255f;
                    r2[i] = SmoothToRoughness2(level0[i * 4 + 3] / 255f);
                }
            }
            int sz = size;
            for (int l = 0; l < levels; l++)
            {
                int m = sz * sz;
                var px = new byte[m * 4];
                for (int i = 0; i < m; i++)
                {
                    int o = i * 4;
                    if (map == MipMap.Albedo) { px[o] = Srgb8(a[i]); px[o + 1] = Srgb8(b[i]); px[o + 2] = Srgb8(c[i]); px[o + 3] = 255; continue; }
                    float len = (float)Math.Sqrt(nx[i] * nx[i] + ny[i] * ny[i] + nz[i] * nz[i]);
                    if (map == MipMap.Normal)
                    {
                        float inv = len > 1e-6f ? 1f / len : 0f;
                        px[o] = B8(nx[i] * inv * 0.5f + 0.5f); px[o + 1] = B8(ny[i] * inv * 0.5f + 0.5f); px[o + 2] = 255; px[o + 3] = 255;
                        continue;
                    }
                    float sm = FilterSmoothness(r2[i], NormalVariance(Math.Min(1f, len)));
                    px[o] = B8(a[i]); px[o + 1] = B8(b[i]); px[o + 2] = B8(c[i]); px[o + 3] = B8(sm);
                }
                outp[l] = px;
                if (sz == 1) break;
                if (map != MipMap.Normal) { a = Down(a, sz); b = Down(b, sz); c = Down(c, sz); }
                if (needN) { nx = Down(nx, sz); ny = Down(ny, sz); nz = Down(nz, sz); }
                if (r2 != null) r2 = Down(r2, sz);
                sz >>= 1;
            }
            return outp;
        }

        // ================================================================ shader twins (MaterialTests)

        /// <summary>Twin of BwCommon.hlsl BwFbmFiltered (value-noise fBm in [0, 1], octaves faded below 2-4 footprints).</summary>
        public static float ShaderFbm(float x, float y, float fpCells, int octaves)
        {
            float sum = 0f, amp = 0.5f, norm = 0f, scale = 1f;
            for (int o = 0; o < octaves; o++)
            {
                float t = Sat((fpCells * scale - 0.25f) / 0.25f);
                float w = 1f - t * t * (3f - 2f * t);
                float n = (float)BwMath.ValueNoise(x * scale + o * 17.31f, y * scale + o * 17.31f);
                sum += amp * Lerp(0.5f, n, w);
                norm += amp;
                amp *= 0.5f;
                scale *= 2.03f;
            }
            return sum / Math.Max(norm, 1e-4f);
        }

        /// <summary>Twin of BwCommon.hlsl BwMacroSigned.</summary>
        public static float MacroSigned(float m) => Math.Max(-1f, Math.Min(1f, (m - 0.5f) * 4f));

        /// <summary>Twin of BwCommon.hlsl BwMacroSigned3 (the 3D fBm is narrower: x5).</summary>
        public static float MacroSigned3(float m) => Math.Max(-1f, Math.Min(1f, (m - 0.5f) * 5f));

        static float Hash31(int x, int y, int z) { unchecked { return BwMath.U01(BwMath.Pcg((uint)x + BwMath.Pcg((uint)y + BwMath.Pcg((uint)z)))); } }

        /// <summary>Twin of BwCommon.hlsl BwValueNoise3 (quintic 3D value noise in [0, 1]).</summary>
        public static float ShaderNoise3(float x, float y, float z)
        {
            int ix = (int)Math.Floor(x), iy = (int)Math.Floor(y), iz = (int)Math.Floor(z);
            float tx = x - ix, ty = y - iy, tz = z - iz;
            float ux = Fade(tx), uy = Fade(ty), uz = Fade(tz);
            float a = Hash31(ix, iy, iz), b = Hash31(ix + 1, iy, iz), c = Hash31(ix, iy + 1, iz), d = Hash31(ix + 1, iy + 1, iz);
            float e = Hash31(ix, iy, iz + 1), g = Hash31(ix + 1, iy, iz + 1), h = Hash31(ix, iy + 1, iz + 1), k = Hash31(ix + 1, iy + 1, iz + 1);
            float z0 = Lerp(Lerp(a, b, ux), Lerp(c, d, ux), uy);
            float z1 = Lerp(Lerp(e, g, ux), Lerp(h, k, ux), uy);
            return Lerp(z0, z1, uz);
        }

        static readonly float[,] s_g12 = { { 1, 1, 0 }, { -1, 1, 0 }, { 1, -1, 0 }, { -1, -1, 0 }, { 1, 0, 1 }, { -1, 0, 1 }, { 1, 0, -1 }, { -1, 0, -1 }, { 0, 1, 1 }, { 0, -1, 1 }, { 0, 1, -1 }, { 0, -1, -1 } };

        static float GradCorner(int x, int y, int z, float dx, float dy, float dz)
        {
            uint h;
            unchecked { h = BwMath.Pcg((uint)x + BwMath.Pcg((uint)y + BwMath.Pcg((uint)z))) % 12u; }
            return s_g12[h, 0] * dx + s_g12[h, 1] * dy + s_g12[h, 2] * dz;
        }

        /// <summary>Twin of BwCommon.hlsl BwGradNoise3 (12 cube-edge gradients, quintic).</summary>
        public static float ShaderGrad3(float x, float y, float z)
        {
            int ix = (int)Math.Floor(x), iy = (int)Math.Floor(y), iz = (int)Math.Floor(z);
            float tx = x - ix, ty = y - iy, tz = z - iz;
            float ux = Fade(tx), uy = Fade(ty), uz = Fade(tz);
            float a = GradCorner(ix, iy, iz, tx, ty, tz), b = GradCorner(ix + 1, iy, iz, tx - 1, ty, tz);
            float c = GradCorner(ix, iy + 1, iz, tx, ty - 1, tz), d = GradCorner(ix + 1, iy + 1, iz, tx - 1, ty - 1, tz);
            float e = GradCorner(ix, iy, iz + 1, tx, ty, tz - 1), g = GradCorner(ix + 1, iy, iz + 1, tx - 1, ty, tz - 1);
            float h = GradCorner(ix, iy + 1, iz + 1, tx, ty - 1, tz - 1), k = GradCorner(ix + 1, iy + 1, iz + 1, tx - 1, ty - 1, tz - 1);
            return Lerp(Lerp(Lerp(a, b, ux), Lerp(c, d, ux), uy), Lerp(Lerp(e, g, ux), Lerp(h, k, ux), uy), uz);
        }

        /// <summary>Twin of BwCommon.hlsl BwFbmGrad3 (mapped to [0, 1]: 0.5 + 0.75 fbm).</summary>
        public static float ShaderFbmGrad3(float x, float y, float z, float fpCells, int octaves)
        {
            float sum = 0f, amp = 0.5f, norm = 0f, scale = 1f;
            for (int o = 0; o < octaves; o++)
            {
                float t = Sat((fpCells * scale - 0.25f) / 0.25f);
                float w = 1f - t * t * (3f - 2f * t);
                float off = o * 17.31f;
                sum += amp * w * ShaderGrad3(x * scale + off, y * scale + off, z * scale + off);
                norm += amp;
                amp *= 0.5f;
                scale *= 2.03f;
            }
            return 0.5f + 0.75f * sum / Math.Max(norm, 1e-4f);
        }

        /// <summary>Twin of BwCommon.hlsl BwFbmFiltered3.</summary>
        public static float ShaderFbm3(float x, float y, float z, float fpCells, int octaves)
        {
            float sum = 0f, amp = 0.5f, norm = 0f, scale = 1f;
            for (int o = 0; o < octaves; o++)
            {
                float t = Sat((fpCells * scale - 0.25f) / 0.25f);
                float w = 1f - t * t * (3f - 2f * t);
                float off = o * 17.31f;
                sum += amp * Lerp(0.5f, ShaderNoise3(x * scale + off, y * scale + off, z * scale + off), w);
                norm += amp;
                amp *= 0.5f;
                scale *= 2.03f;
            }
            return sum / Math.Max(norm, 1e-4f);
        }

        // ================================================================ statistics (tests, docs/MATERIALS.md)

        /// <summary>Mean and min/max of a channel (level 0).</summary>
        public static void Stats(float[] a, out float mean, out float min, out float max)
        {
            double sum = 0; min = float.MaxValue; max = float.MinValue;
            for (int i = 0; i < a.Length; i++) { sum += a[i]; if (a[i] < min) min = a[i]; if (a[i] > max) max = a[i]; }
            mean = (float)(sum / Math.Max(1, a.Length));
        }

        /// <summary>Largest jump across the tile seam relative to the largest jump inside the tile (1 = seamless).</summary>
        public static float SeamRatio(float[] a, int size)
        {
            double seam = 0, inner = 0;
            for (int y = 0; y < size; y++)
            {
                seam = Math.Max(seam, Math.Abs(a[y * size + size - 1] - a[y * size]));
                for (int x = 1; x < size; x++) inner = Math.Max(inner, Math.Abs(a[y * size + x] - a[y * size + x - 1]));
            }
            for (int x = 0; x < size; x++)
            {
                seam = Math.Max(seam, Math.Abs(a[(size - 1) * size + x] - a[x]));
                for (int y = 1; y < size; y++) inner = Math.Max(inner, Math.Abs(a[y * size + x] - a[(y - 1) * size + x]));
            }
            return (float)(seam / Math.Max(inner, 1e-9));
        }
    }
}
