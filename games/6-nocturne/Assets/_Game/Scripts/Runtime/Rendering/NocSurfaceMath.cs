// NocSurfaceMath: the double-precision C# twins of the Look v2 near-surface and wet-floor code (TEXTURE_PLAN 3.3, 3.5, 2.11; owner
// SU): the NocWet v2 puddle noise and mask (NocPuddleNoise2 in NocMirror.hlsl, the height bias of NocWet), the puddle threshold
// measured from a height histogram, the rain rings (NocRippleGrad2), the anchored leak decal coordinate (NocSurface) and the NocLine
// paint variation (NocLinePaint). SurfaceTests and WetFloorTests compare them with the shaders' numbers; MaterialFactory.Surface
// reads the thresholds. Only WallRadiance touches UnityEngine (it reads PA's rig through AtmosphereDriver: one copy of the numbers).
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Megastructure
{
    public static partial class NocSurfaceMath
    {
        // ------------------------------------------------------------------ puddles (TEXTURE_PLAN 3.3)
        /// <summary>The v2 puddle noise (twin of NocPuddleNoise2): two rotated value-noise octaves at 2.0 / 0.6 m, weights 0.62 / 0.38.</summary>
        public static double PuddleNoise(double x, double z)
        {
            double s0 = NocSurfaceConstants.SurfPuddleScale0, s1 = NocSurfaceConstants.SurfPuddleScale1;
            double ax = (0.96 * x + 0.28 * z) / s0 + 7.3, az = (-0.28 * x + 0.96 * z) / s0 + 7.3;
            double bx = (0.6 * x - 0.8 * z) / s1 + 31.7, bz = (0.8 * x + 0.6 * z) / s1 + 31.7;
            return 0.62 * NocMath.ValueNoise(ax, az) + 0.38 * NocMath.ValueNoise(bx, bz);
        }

        /// <summary>The thresholded value: noise + height bias x (0.5 - h) (h = the material's height map, 0..1).</summary>
        public static double PuddleField(double x, double z, double h) => PuddleNoise(x, z) + NocSurfaceConstants.SurfPuddleHeightBias * (0.5 - h);

        /// <summary>The puddle mask at full resolution (edge SurfPuddleEdge, no fwidth term).</summary>
        public static double PuddleMask(double x, double z, double h, double th)
        {
            double e = NocSurfaceConstants.SurfPuddleEdge;
            return NocMath.Smoothstep(th - e, th + e, PuddleField(x, z, h));
        }

        /// <summary>
        /// The threshold that gives `fraction` puddles for a height distribution: the (1 - fraction) quantile of
        /// noise + bias (0.5 - h) over n samples, with the noise at hashed world points (a 400 m square) and h drawn from `heights`
        /// (the material's height texels, any order; null = 0.5 everywhere). Deterministic (seeded hashes).
        /// </summary>
        public static double PuddleThreshold(IReadOnlyList<double> heights, double fraction = NocSurfaceConstants.SurfPuddleFraction, int n = 1000000, uint seed = 1u)
        {
            var v = new double[n];
            int hn = heights?.Count ?? 0;
            for (int i = 0; i < n; i++)
            {
                uint a = NocMath.Pcg((uint)i * 2654435761u + seed), b = NocMath.Pcg(a + 0x68E31DA4u), c = NocMath.Pcg(b + 0xB5297A4Du);
                double x = 400.0 * NocMath.U01(a) - 200.0, z = 400.0 * NocMath.U01(b) - 200.0;
                double h = hn > 0 ? heights[(int)(c % (uint)hn)] : 0.5;
                v[i] = PuddleField(x, z, h);
            }
            Array.Sort(v);
            int k = (int)Math.Round((1.0 - fraction) * (n - 1));
            return v[Math.Max(0, Math.Min(n - 1, k))];
        }

        /// <summary>The zero-mean sag of an analytic deck plate (NocWet v2, FloorSteelDeck) at plate-local r = max(|2u-1|, |2v-1|):
        /// 0.25 (r^2 - 0.5), lowest at the centre (r is the max of two uniforms over a plate: E[r^2] = 1/2).</summary>
        public static double PlateSag(double r) => 0.25 * (r * r - 0.5);

        /// <summary>The height histogram (heights 0..1) of a generated texture family at n hashed (u, v) points (a regular lattice
        /// aliases with the recipes' own periods), optionally with the deck plates' sag at hashed plate positions (the shader's h).
        /// MaterialFactory measures each NocWet row's _PuddleTh from it (PuddleThreshold); the tests use another seed.</summary>
        public static List<double> HeightHistogram(NocTexFamily f, int n = 16384, bool plates = false, uint seed = 1u)
        {
            var l = new List<double>(n);
            for (int k = 0; k < n; k++)
            {
                uint a = NocMath.Pcg((uint)k * 2654435761u + seed * 0x9E3779B9u + 977u), b = NocMath.Pcg(a + 0x68E31DA4u);
                double h = NocTexRecipes.Sample(f, NocMath.U01(a), NocMath.U01(b)).h;
                if (plates)
                {
                    uint c = NocMath.Pcg(b + 0xB5297A4Du), d = NocMath.Pcg(c + 0x1B873593u);
                    h = NocMath.Clamp01(h + PlateSag(Math.Max(NocMath.U01(c), NocMath.U01(d))));
                }
                l.Add(Math.Round(255.0 * NocMath.Clamp01(h)) / 255.0);          // the mask stores B in 8 bits
            }
            return l;
        }

        /// <summary>The puddle fraction (mean of the hard mask field > th) over n hashed samples (the test of PuddleThreshold).</summary>
        public static double PuddleFraction(IReadOnlyList<double> heights, double th, int n = 1000000, uint seed = 7u)
        {
            int hn = heights?.Count ?? 0, wet = 0;
            for (int i = 0; i < n; i++)
            {
                uint a = NocMath.Pcg((uint)i * 2246822519u + seed), b = NocMath.Pcg(a + 0x1B873593u), c = NocMath.Pcg(b + 0xCC9E2D51u);
                double x = 400.0 * NocMath.U01(a) - 200.0, z = 400.0 * NocMath.U01(b) - 200.0;
                double h = hn > 0 ? heights[(int)(c % (uint)hn)] : 0.5;
                if (PuddleField(x, z, h) > th) wet++;
            }
            return wet / (double)n;
        }

        // ------------------------------------------------------------------ ripples (TEXTURE_PLAN 3.3, CRIT0 #29)
        /// <summary>Slope (gradient of the ring height field, xz) of the v2 rain rings at time t and rain rho (twin of NocRippleGrad2).</summary>
        public static void RippleGrad(double x, double z, double t, double rho, out double gx, out double gz)
        {
            gx = 0; gz = 0;
            double cell = NocSurfaceConstants.SurfRippleCell;
            double bx = Math.Floor(x / cell + 0.5) - 1.0, bz = Math.Floor(z / cell + 0.5) - 1.0;
            double period = NocSurfaceConstants.SurfRipplePeriod / Math.Max(rho, 0.05);
            for (int j = 0; j < 2; j++)
                for (int i = 0; i < 2; i++)
                {
                    int cx = (int)bx + i, cz = (int)bz + j;
                    uint h = NocMath.Pcg2(cx + 7919, cz - 104729);
                    double px = (cx + 0.25 + 0.5 * NocMath.U01(h)) * cell, pz = (cz + 0.25 + 0.5 * NocMath.U01(NocMath.Pcg(h + 11u))) * cell;
                    double ph = NocMath.U01(NocMath.Pcg(h + 23u)) * period;
                    double age = (t + ph) - period * Math.Floor((t + ph) / period);
                    double life = NocMath.Clamp01(1.0 - age / NocSurfaceConstants.SurfRippleLife);
                    double R = NocSurfaceConstants.SurfRippleR0 + NocSurfaceConstants.SurfRippleSpeed * age;
                    double dx = x - px, dz = z - pz, r = Math.Sqrt(dx * dx + dz * dz);
                    double u = (r - R) / NocSurfaceConstants.SurfRippleLambda;
                    double s = Math.Exp(-1.5 * u * u) * Math.Sin(2.0 * Math.PI * u) * life * life
                             * (NocSurfaceConstants.SurfRippleRef / (NocSurfaceConstants.SurfRippleRef + R));
                    double rr = Math.Max(r, 1e-4);
                    gx += s * dx / rr; gz += s * dz / rr;
                }
        }

        /// <summary>Largest ring radius plus 1.5 wavelengths (m): it must stay within 0.75 cell (the 2 x 2 lookup holds every ring).</summary>
        public static double RippleReach => NocSurfaceConstants.SurfRippleR0 + NocSurfaceConstants.SurfRippleSpeed * NocSurfaceConstants.SurfRippleLife
                                            + 1.5 * NocSurfaceConstants.SurfRippleLambda;

        // ------------------------------------------------------------------ anchored leaks (TEXTURE_PLAN 3.3)
        /// <summary>The distance below the face top that the vertex bake stores in colour B (0..255 = 0..25.5 m), decoded.</summary>
        public static double LeakDecode(byte b) => b / 255.0 * NocSurfaceConstants.SurfLeakMaxD;

        /// <summary>Leak column of a horizontal face coordinate u (m): its stretch (1..2) and its texture u offset (0..1), by hash.</summary>
        public static void LeakColumn(double u, uint seed, out double stretch, out double uOffset)
        {
            int col = (int)Math.Floor(u / NocSurfaceConstants.SurfLeakTileU);
            uint h = NocMath.Pcg2(col, (int)seed);
            stretch = NocSurfaceConstants.SurfLeakStretch0 + (NocSurfaceConstants.SurfLeakStretch1 - NocSurfaceConstants.SurfLeakStretch0) * NocMath.U01(h);
            uOffset = NocMath.U01(NocMath.Pcg(h + 0x9E3779B9u));
        }

        /// <summary>The decal's v of a point d m below the anchored top edge: the content fills v 1 (the edge) .. 0.5 (1 m x stretch below),
        /// the pad (opacity 0) lies below 0.5; clamped at 0.25 so no filter tap wraps to the content's top row.</summary>
        public static double LeakV(double dTop, double stretch)
            => Math.Max(0.25, 1.0 - 0.5 * dTop / (NocSurfaceConstants.SurfLeakLen * Math.Max(stretch, 1e-3)));

        // ------------------------------------------------------------------ wetness and the wall radiance (LOOK_V2 5.2, TEXTURE_PLAN 2.11)
        /// <summary>Lagarde's porous darkening: albedo x lerp(1, SurfWetDarkMin, porosity x wet) (NocSurface v2 uses the same line).</summary>
        public static double WetDarkening(double porosity, double wet)
            => NocMath.Lerp(1.0, NocSurfaceConstants.SurfWetDarkMin, NocMath.Clamp01(porosity) * NocMath.Clamp01(wet));

        /// <summary>Unit direction toward the sky fill (minus the forward of Euler(FillPitch, FillYaw, 0)).</summary>
        public static D3 FillDirection()
        {
            double p = AtmosphereDriver.FillPitch * Math.PI / 180.0, y = AtmosphereDriver.FillYaw * Math.PI / 180.0;
            return new D3(-Math.Sin(y) * Math.Cos(p), Math.Sin(p), -Math.Cos(y) * Math.Cos(p));
        }

        /// <summary>
        /// Macro radiance (linear) of a flat vertical wall of albedo token x factor at path position s, for the wet mirror's wall
        /// planes (kind 1; TEXTURE_PLAN 2.11): the wetted albedo (WetDarkening at the vertical wet level) x URP Lambert 0.96 x (the
        /// trilight's equator term, the only one a vertical normal sees, + the sky fill x max(0, N.L)), all from PA's rig
        /// (AtmosphereDriver.AmbientAt / SkyFillAt / SkyFillColourAt, LightCal 1). (nx, nz) is the wall's outward normal (any length;
        /// 0 = shade side). extra (linear, optional) adds the mean emission of what the wall carries (the alley's lit windows).
        /// </summary>
        public static Color WallRadiance(NocPal token, double s, double nx = 0.0, double nz = 0.0, double factor = 1.0,
                                         double porosity = NocSurfaceConstants.SurfPorConcrete, double wet = NocSurfaceConstants.SurfWetVertical)
        {
            AtmosphereDriver.AmbientAt(s, out Color sky, out Color eq, out Color gr);
            Color fill = AtmosphereDriver.SkyFillColourAt(s) * (float)AtmosphereDriver.SkyFillAt(s);
            double nl = Math.Sqrt(nx * nx + nz * nz), ndl = 0.0;
            if (nl > 1e-9) { D3 L = FillDirection(); ndl = Math.Max(0.0, (nx * L.x + nz * L.z) / nl); }
            Color a = NocturnePalette.Linear(token);
            double k = factor * NocSurfaceConstants.SurfLambert * WetDarkening(porosity, wet);
            var c = new Color((float)(a.r * k * (eq.r + fill.r * ndl)), (float)(a.g * k * (eq.g + fill.g * ndl)), (float)(a.b * k * (eq.b + fill.b * ndl)), 1f);
            return c;
        }

        /// <summary>The alley walls' macro (Beat01_Alley v2): WallRadiance of the ConcreteDark walls plus the mean emission of the lit
        /// windows (AlleyWindows: WinLit of the WinW x WinH openings per WinPitch x WinFloor bay, WinWarmShare of them EmitShopWarm, the
        /// rest EmitShopCold).</summary>
        public static Color AlleyWallMacro(double s, double nx)
        {
            Color wall = WallRadiance(NocPal.ConcreteDark, s, nx, 0.0);
            double cover = AlleyLayout.WinLit * AlleyLayout.WinW * AlleyLayout.WinH / (AlleyLayout.WinPitch * AlleyLayout.WinFloor);
            Color win = NocturnePalette.Linear(NocPal.WinWarm) * (float)(AlleyLayout.WinWarmShare * G1Materials.ShopWarmFactor)
                      + NocturnePalette.Linear(NocPal.WinCold) * (float)((1.0 - AlleyLayout.WinWarmShare) * G1Materials.ShopColdFactor);
            Color c = wall + win * (float)cover;
            c.a = 1f;
            return c;
        }

        // ------------------------------------------------------------------ the shaders' per-pixel lines (twins for the tests)
        /// <summary>NocWet v2 floor normal (twin): the texture slope s weighted by (1 - m), plus the ring slope g: normalize(-s (1 - m) + g, 1).
        /// In a full puddle (m = 1, no ring) it is exactly up.</summary>
        public static D3 FloorNormal(double sx, double sz, double m, double gx, double gz)
            => new D3(-sx * (1.0 - m) + gx, 1.0, -sz * (1.0 - m) + gz).Normalized;

        /// <summary>NocSurface v2 wetness (twin): saturate(wetLevel (0.55 + 0.45 up) + streak 0.5 + up 0.35 (0.5 - h)).</summary>
        public static double SurfaceWet(double wetLevel, double up, double streak, double h)
            => NocMath.Clamp01(wetLevel * (NocSurfaceConstants.SurfWetBase + NocSurfaceConstants.SurfWetUp * up) + streak * NocSurfaceConstants.SurfWetStreak
                               + up * NocSurfaceConstants.SurfWetPool * (0.5 - h));

        // ------------------------------------------------------------------ grating (TEXTURE_PLAN 3.5; twin of NocGrating.shader)
        /// <summary>Twin of NocCommon's NocFilteredGrid1 (box-filtered line of width w every period, >= 1 px with its energy kept,
        /// blended to its mean w / period once the period is under minPx pixels).</summary>
        public static double FilteredGrid1(double x, double period, double w, double fp, double minPx)
        {
            double fw = Math.Max(fp, 1e-5), wd = Math.Max(w, fw);
            double fr = x / period + 0.5; fr -= Math.Floor(fr);
            double d = Math.Abs(fr - 0.5) * period;
            double ln = (1.0 - NocMath.Smoothstep(0.5 * wd - 0.5 * fw, 0.5 * wd + 0.5 * fw, d)) * (w / wd);
            return ln + (NocMath.Clamp01(w / period) - ln) * NocMath.Clamp01(fw * minPx / period);
        }

        /// <summary>Coverage of the NocGrating bar mesh at (x, z) for the unit view direction v (toward the eye) and the planar
        /// footprint fp (m): two bar families (pitch p, widths w0 across x and w1 across z), each widened by depth x tan(theta)
        /// across it (a grating reads solid at grazing view), box filtered, the mean below 3 px of the pitch.</summary>
        public static double GratingCover(double x, double z, double vx, double vy, double vz, double fp, double pitch, double w0, double w1, double depth)
        {
            double ay = Math.Max(Math.Abs(vy), 0.05), p = Math.Max(pitch, 1e-3);
            double a = Math.Min(w0 + depth * Math.Abs(vx) / ay, p), b = Math.Min(w1 + depth * Math.Abs(vz) / ay, p);
            double gx = FilteredGrid1(x, p, a, fp, 3.0), gz = FilteredGrid1(z, p, b, fp, 3.0);
            return NocMath.Clamp01(1.0 - (1.0 - gx) * (1.0 - gz));
        }

        // ------------------------------------------------------------------ NocLine paint (TEXTURE_PLAN 3.5)
        /// <summary>The albedo factor of the NocLine paint variation at world point (x, y, z) for a pixel footprint fp (m) (twin of
        /// NocLinePaint): noise x chips x the splash band; its mean over a long tube is 1 + 0.6 x chipP above the splash band.</summary>
        public static double LinePaint(double x, double y, double z, double fp)
        {
            double qx = y + 0.37 * x, qz = z - 0.61 * x;
            double fade = NocSurfaceConstants.SurfLineFadePx;
            double wN = NocMath.Smoothstep(fade, 2.0 * fade, NocSurfaceConstants.SurfLineScale / fp);
            double sc = NocSurfaceConstants.SurfLineScale;
            double n = 0.65 * NocMath.ValueNoise(qx / sc + 3.7, qz / sc + 3.7) + 0.35 * NocMath.ValueNoise(qx * (3.1 / sc) + 11.3, qz * (3.1 / sc) + 11.3);
            double k = 1.0 + NocSurfaceConstants.SurfLineAmp * (2.0 * n - 1.0) * wN;
            double wC = NocMath.Smoothstep(fade, 2.0 * fade, NocSurfaceConstants.SurfLineChipLen / fp);
            int cx = (int)Math.Floor(qx / NocSurfaceConstants.SurfLineChipLen), cz = (int)Math.Floor(qz / NocSurfaceConstants.SurfLineChipLen);
            double chip = NocMath.U01(NocMath.Pcg2(cx + 4099, cz - 7919)) < NocSurfaceConstants.SurfLineChipP ? 1.0 : 0.0;
            double chipW = NocMath.Lerp(NocSurfaceConstants.SurfLineChipP, chip, wC);
            k *= 1.0 + 0.6 * chipW;
            double splash = 1.0 - NocMath.Smoothstep(0.0, NocSurfaceConstants.SurfLineSplash, y);
            return k * NocMath.Lerp(1.0, NocSurfaceConstants.SurfLineSplashDark, splash);
        }
    }
}
