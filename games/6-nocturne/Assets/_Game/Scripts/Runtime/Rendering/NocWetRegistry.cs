// NocWetRegistry: the C# side of the analytic wet-floor mirror (14.7 H4, D-003, R1 section 2). Beat builders register, at
// load time only,
//   - area sources (signs, blades, lamp glows, windows): a segment P0-P1 (a disc when P0 == P1), glow half-width rho (m) and
//     effective radiance L_eff (linear HDR, <= 3.0 by its brightest channel, so a reflection is never brighter than its
//     emitter and nothing sparkles). They are binned by the s of their projection on the path (bins of 20 m).
//   - wall planes: a vertical finite rectangle (centre xz, outward normal xz, half length, top y) with its macro colour, active
//     over an s range.
// Every frame AtmosphereDriver calls Upload(s): the active sources are the bins overlapping s - 20 - 12 .. s + 60 + 12, each
// weighted by the 12 m crossfade at both ends of the window s - 20 .. s + 60 (a source never pops), at most 32 in bin order
// (nothing is sorted per frame); walls are weighted the same way over their own s range, at most 4. Zero allocations per
// frame: fixed Vector4 arrays, always uploaded with their full length (the array size is fixed by the first set).
using System.Collections.Generic;
using UnityEngine;

namespace Megastructure
{
    public static class NocWetRegistry
    {
        public const int MaxSources = NocturneConstants.WetSourceMax;
        public const int MaxWalls = NocturneConstants.WetWallMax;

        struct Src { public Vector4 p0, p1, l; public double s; }
        struct Wall { public Vector4 a, b, c; public double s0, s1; }

        static readonly List<Src>[] s_bins = NewBins();
        static readonly List<Wall> s_walls = new List<Wall>();
        static readonly Vector4[] s_p0 = new Vector4[MaxSources], s_p1 = new Vector4[MaxSources], s_l = new Vector4[MaxSources];
        static readonly Vector4[] s_wa = new Vector4[MaxWalls], s_wb = new Vector4[MaxWalls], s_wc = new Vector4[MaxWalls];
        static int s_count, s_wallCount, s_dropped;

        /// <summary>Number of sources / walls uploaded by the last Upload (QA).</summary>
        public static int ActiveSources => s_count;
        public static int ActiveWalls => s_wallCount;
        /// <summary>Sources cut by the cap of 32 in the last Upload (0 by construction of the beats; logged by QA).</summary>
        public static int Dropped => s_dropped;
        public static int TotalSources { get { int n = 0; foreach (var b in s_bins) n += b.Count; return n; } }

        static List<Src>[] NewBins()
        {
            int n = (int)System.Math.Ceiling((NocturneConstants.PathLength + 200.0) / NocturneConstants.WetBinSize) + 1;
            var b = new List<Src>[n];
            for (int i = 0; i < n; i++) b[i] = new List<Src>(8);
            return b;
        }

        /// <summary>Removes every source and wall. GameBootstrap.Awake calls it (with NocCloudGlow.Defaults) before any builder
        /// runs, so no stale sources survive a play session without a domain reload; tests call it too.</summary>
        public static void Clear()
        {
            foreach (var b in s_bins) b.Clear();
            s_walls.Clear();
            s_count = s_wallCount = s_dropped = 0;
        }

        static int BinOf(double s) => Mathf.Clamp((int)System.Math.Floor((s + 100.0) / NocturneConstants.WetBinSize), 0, s_bins.Length - 1);

        /// <summary>
        /// Registers an area source. radiance = linear HDR emitter radiance (clamped to L_eff 3.0 by its brightest channel);
        /// rho = glow half-width (m; for a lamp choose rho so that I / (pi rho^2) &lt;= 3); s = its path progress (NaN: projected).
        /// </summary>
        public static void AddSource(Vector3 p0, Vector3 p1, float rho, Color radiance, double s = double.NaN)
        {
            if (double.IsNaN(s)) s = NocMath.ProjectToPath(new D3(0.5 * (p0.x + p1.x), 0.5 * (p0.y + p1.y), 0.5 * (p0.z + p1.z)), 0.0);
            float peak = Mathf.Max(radiance.r, Mathf.Max(radiance.g, radiance.b));
            float k = peak > (float)NocturneConstants.WetLeffMax ? (float)NocturneConstants.WetLeffMax / peak : 1f;
            s_bins[BinOf(s)].Add(new Src
            {
                p0 = new Vector4(p0.x, p0.y, p0.z, Mathf.Max(rho, 0.01f)),
                p1 = new Vector4(p1.x, p1.y, p1.z, 0f),
                l = new Vector4(radiance.r * k, radiance.g * k, radiance.b * k, 0f),
                s = s,
            });
        }

        /// <summary>
        /// Registers a wall plane active over s0..s1 (12 m crossfades outside): centre (cx, cz) on the wall face, outward normal
        /// (nx, nz) toward the walk, half length along the wall, top height (the plane spans y 0..yTop), macro radiance (linear)
        /// and kind (0 = facade macro bands, 1 = flat emitter such as the screen wall).
        /// </summary>
        public static void AddWall(double s0, double s1, float cx, float cz, float nx, float nz, float halfLength, float yTop, Color macro, int kind = 0)
        {
            float n = Mathf.Sqrt(nx * nx + nz * nz);
            if (n < 1e-6f) return;
            s_walls.Add(new Wall
            {
                a = new Vector4(cx, cz, nx / n, nz / n),
                b = new Vector4(halfLength, yTop, kind, 1f),
                c = new Vector4(macro.r, macro.g, macro.b, 0f),
                s0 = s0, s1 = s1,
            });
        }

        /// <summary>Crossfade weight of an item at progress si for the walker at s (window s - 20 .. s + 60, 12 m fades).</summary>
        public static double SourceWeight(double si, double s)
        {
            double lo = s - NocturneConstants.WetWindowBack, hi = s + NocturneConstants.WetWindowAhead, f = NocturneConstants.WetCrossfade;
            return NocMath.Smoothstep(lo - f, lo, si) * (1.0 - NocMath.Smoothstep(hi, hi + f, si));
        }

        /// <summary>Weight of a wall active over s0..s1 for the walker at s (12 m fades outside the range).</summary>
        public static double WallWeight(double s0, double s1, double s)
        {
            double f = NocturneConstants.WetCrossfade;
            return NocMath.Smoothstep(s0 - f, s0, s) * (1.0 - NocMath.Smoothstep(s1, s1 + f, s));
        }

        /// <summary>Fills and uploads the active set for progress s (no allocation).</summary>
        public static void Upload(double s)
        {
            Collect(s);
            Shader.SetGlobalVectorArray(ShaderIds.SrcP0, s_p0);
            Shader.SetGlobalVectorArray(ShaderIds.SrcP1, s_p1);
            Shader.SetGlobalVectorArray(ShaderIds.SrcL, s_l);
            Shader.SetGlobalFloat(ShaderIds.SrcCount, s_count);
            Shader.SetGlobalVectorArray(ShaderIds.WallA, s_wa);
            Shader.SetGlobalVectorArray(ShaderIds.WallB, s_wb);
            Shader.SetGlobalVectorArray(ShaderIds.WallC, s_wc);
            Shader.SetGlobalFloat(ShaderIds.WallCount, s_wallCount);
        }

        /// <summary>The active set for s without uploading (tests); returns the source count.</summary>
        public static int Collect(double s)
        {
            s_count = 0; s_dropped = 0;
            double f = NocturneConstants.WetCrossfade;
            int b0 = BinOf(s - NocturneConstants.WetWindowBack - f), b1 = BinOf(s + NocturneConstants.WetWindowAhead + f);
            for (int b = b0; b <= b1; b++)
            {
                var list = s_bins[b];
                for (int i = 0; i < list.Count; i++)
                {
                    var src = list[i];
                    double w = SourceWeight(src.s, s);
                    if (w <= 0.0) continue;
                    if (s_count >= MaxSources) { s_dropped++; continue; }
                    s_p0[s_count] = src.p0;
                    s_p1[s_count] = src.p1;
                    s_l[s_count] = src.l * (float)w;
                    s_count++;
                }
            }
            for (int i = s_count; i < MaxSources; i++) { s_p0[i] = Vector4.zero; s_p1[i] = Vector4.zero; s_l[i] = Vector4.zero; }

            s_wallCount = 0;
            for (int i = 0; i < s_walls.Count && s_wallCount < MaxWalls; i++)
            {
                var wl = s_walls[i];
                double w = WallWeight(wl.s0, wl.s1, s);
                if (w <= 0.0) continue;
                s_wa[s_wallCount] = wl.a;
                var bb = wl.b; bb.w = (float)w;
                s_wb[s_wallCount] = bb;
                s_wc[s_wallCount] = wl.c;
                s_wallCount++;
            }
            for (int i = s_wallCount; i < MaxWalls; i++) { s_wa[i] = Vector4.zero; s_wb[i] = Vector4.zero; s_wc[i] = Vector4.zero; }
            return s_count;
        }

        /// <summary>Largest number of sources with a non-zero weight for any walker position (load-time check: must be &lt;= 32).</summary>
        public static int MaxActive()
        {
            int best = 0;
            for (double s = 0; s <= NocturneConstants.PathLength; s += 1.0)
            {
                int n = 0;
                foreach (var b in s_bins) foreach (var src in b) if (SourceWeight(src.s, s) > 0) n++;
                if (n > best) best = n;
            }
            return best;
        }

        /// <summary>Number of registered wall planes (tests and QA).</summary>
        public static int RegisteredWalls => s_walls.Count;

        /// <summary>Registered wall plane i (tests and QA: WetMacroTests): centre / normal (a), half length / top / kind (b), macro (c)
        /// and its s range.</summary>
        public static void GetWall(int i, out Vector4 a, out Vector4 b, out Color macro, out double s0, out double s1)
        {
            var w = s_walls[i];
            a = w.a; b = w.b; macro = new Color(w.c.x, w.c.y, w.c.z, 1f); s0 = w.s0; s1 = w.s1;
        }

        /// <summary>Uploaded source i (tests and QA).</summary>
        public static void GetSource(int i, out Vector4 p0, out Vector4 p1, out Vector4 l) { p0 = s_p0[i]; p1 = s_p1[i]; l = s_l[i]; }
    }
}
