// NocScreenMath: the C# mirror of the Wall of signs (MASTER_PROMPT 14.5.6, D-010, R4 section 3.5; SG Phase 4 wave A), the same
// formulas as Shaders/Beats/NocScreenWave.hlsl (NocScreen.shader and the H4 wall plane of kind 2), used by the wall-light driver
// (12 lights take the wave colour at their position), the wet-floor wall plane macro and Beat06SignsTests (T-WAVE,
// photosensitivity, budgets). Pure double maths, no allocation.
//   phi1 = (0.85 z + 0.53 y) / 520 - t / 58,  phi2 = (-0.60 z + 0.80 y) / 360 + t / 60,  phi3 = (0.30 z - 0.95 y) / 700 - t / 58
//   h = fract(0.5 + 0.28 sin(2 pi phi1) [+ 0.18 sin(2 pi phi2) + 0.12 sin(2 pi phi3)])      (Simplified tier: phi1 only)
//   Look v2 ramp (LOOK_V2 10, R6, R7: never violet): stops ScreenTeal, SignWhite (ice), SodiumAmber, NeonCyan, ScreenTeal at h 0,
//   1/4, 1/2, 3/4, 1 (periodic), each desaturated 35 % toward its luminance and mixed 15 % toward ScreenBase, then
//   luminance-normalised with k 0.8 (D-010) to a geometric mean of 0.27 (NocturneConstants.ScreenRampY); the amber stop turns to
//   the ice stop over s 1 230-1 290 (AmberWeight, the twin of _NocAmber.y)
//   imagery (Full tier, v3): displays of 72.8 m x 3..7 panels, a programme per 40-75 s epoch (10 kinds), a display level and a
//   hue offset, with a 16 s crossfade, giving a luminance factor m (x 1.18, capped 4.5 / the resolved LED dot's peak gain),
//   an accent weight a and the hue offset dh:
//   colour = lerp(ramp(h + dh), sat(ramp(h + dh + 1/4), 1.3), a) x m
//   emission = colour x 0.45 x 0.55 x (1 + 0.0909 breath) x panel (brightness, white point, age, failures) x modules x LED x seams
//   x the LED view-angle falloff; breath sin(2 pi (n t / 600 + phase)), n = 30..60 per panel (0.05 - 0.10 Hz)
// Time arguments are the wrapped phases the driver uploads (frac(t / 58), frac(t / 60), t mod 600), so the shader and this mirror
// wrap identically (Part 10 item 33: 58 s, 60 s and 600 s are whole periods of every term they carry; every display period divides 600).
using System;

namespace Megastructure
{
    public static class NocScreenMath
    {
        /// <summary>Desaturation toward the stop's own luminance, mix toward ScreenBase, luminance-normalisation exponent.</summary>
        public const double Desat = 0.35, BaseMix = 0.15, NormK = 0.8;
        public const int StopCount = 5;
        const double Tau = 2.0 * Math.PI;
        /// <summary>The amber stop's weight (1 -> 0 over s 1 230-1 290, R6): the twin of _NocAmber.y (AtmosphereDriver, every frame).</summary>
        public static double AmberWeight => AtmosphereDriver.FrameScreenAmber;

        // ------------------------------------------------------------------ hardware constants (NocScreenWave.hlsl SCR_*)
        public const double Z0 = 275.0, PitchZ = 36.4, PitchY = 20.4, HalfGap = 0.2, HalfBezel = 0.32, Module = 4.0, Joint = 0.03;
        public const double LedPitch = 0.25, LedDot = 0.66, DisplayW = 72.8, Crossfade = 16.0;
        /// <summary>LED dots: faded in from 10 to 18 px of pitch, over a diffuser floor of 35 % (energy kept; SG r03 / r04: from 5 or
        /// 8 px the dot lattice read as a wire mesh).</summary>
        public const double LedFade0 = 10.0, LedFade1 = 18.0, LedFloor = 0.35;
        /// <summary>Glyph-column scroll speed (m/s; SCR_COLV): its 2.6 m strokes with 2 m soft edges pass a pixel at &lt;= 0.06 Hz.</summary>
        public const double ColumnSpeed = 0.15;
        /// <summary>Light the 0.4 m gap scatters (fraction of the panel's), joint darkening, LED view-angle floor at grazing and its exponent.</summary>
        public const double GapScatter = 0.10, JointDark = 0.40, AngleFloor = 0.70, AngleExp = 0.4;
        /// <summary>Failure rates per panel: dead (1.2 %), dead modules (1.8 %: 8 % of their modules), a stuck module column (1 %); aged 18 %.</summary>
        public const double DeadPanel = 0.012, DeadModulePanels = 0.030, StuckPanels = 0.040, DeadModuleFraction = 0.08, AgedFraction = 0.18;
        /// <summary>Programme weights (cumulative thresholds: wave, glyph, liquid, sun, glyph column, skyline, glyph matrix, bokeh,
        /// scan lines, standby).</summary>
        public static readonly double[] ProgramCum = { 0.06, 0.16, 0.32, 0.42, 0.50, 0.63, 0.76, 0.87, 0.95, 1.0 };
        public const int ProgramCount = 10;
        public static readonly string[] ProgramNames = { "wave", "glyph", "liquid", "sun", "column", "skyline", "matrix", "bokeh", "lines", "standby" };
        /// <summary>The cap of m where the LED dots are unresolved (SCR_MMAX); where they are resolved the cap is divided by
        /// 1 + DotGain wp, so cap x the dot's peak gain is ContentMaxM everywhere (the bloom check).</summary>
        public const double ContentMaxM = 4.5;
        /// <summary>The resolved LED dot's peak gain - 1 (SCR_DOTG): LedFloor + (1 - LedFloor) / LedDot^2 - 1.</summary>
        public const double DotGain = LedFloor + (1.0 - LedFloor) / (LedDot * LedDot) - 1.0;
        /// <summary>Imagery normalisation (SCR_MNORM): mean m x the LED hardware mean 1.56 (Beat06SignsTests), the LOOK_V2 KS16 wall
        /// of 60-85 codes (SG r03: 51.5 codes at 0.97, the bare 14.5.6 mean; r07: 61.4 at 1.70; D-036 (6): 1.85 moves it toward the
        /// middle of the band, 61.4 x (1.85 / 1.70)^(1 / 2.2) = 64.0 expected); the peak stays capped (ContentMaxM).</summary>
        public const double ContentNorm = 1.85;
        /// <summary>The LED wall's design mean relative to the bare 14.5.6 wave (mean m x the hardware mean): 1.43 x 1.85 / 1.70.</summary>
        public const double WallMeanGain = 1.556;
        /// <summary>The softest imagery edge (m) and the accent saturation (SCR_EDGE, SCR_SATK).</summary>
        public const double Edge = 0.12, AccentSat = 1.3;
        /// <summary>Display levels per epoch (25 / 50 / 25 %).</summary>
        public const double LevelLow = 0.9, LevelMid = 1.1, LevelHigh = 1.35;

        static readonly double[] s_stops = BuildStops();   // teal, ice, amber, cyan x rgb (linear)

        static double[] BuildStops()
        {
            var t = Stop(NocPal.ScreenTeal);
            var w = Stop(NocPal.SignWhite);
            var a = Stop(NocPal.SodiumAmber);
            var c = Stop(NocPal.NeonCyan);
            double yt0 = Y(t), yw = Y(w), ya = Y(a), yc = Y(c);
            double yg = Math.Pow(yt0 * yw * ya * yc, 0.25);
            double ky = NocturneConstants.ScreenRampY / yg;    // the normalisation keeps the geometric mean yg: rescale it to 0.27
            Scale(t, ky * Math.Pow(yg / yt0, NormK)); Scale(w, ky * Math.Pow(yg / yw, NormK)); Scale(a, ky * Math.Pow(yg / ya, NormK)); Scale(c, ky * Math.Pow(yg / yc, NormK));
            return new[] { t[0], t[1], t[2], w[0], w[1], w[2], a[0], a[1], a[2], c[0], c[1], c[2] };
        }

        static double[] Stop(NocPal p)
        {
            var t = NocturnePalette.Linear(p);
            var b = NocturnePalette.Linear(NocPal.ScreenBase);
            double y = NocMath.Luma709(t.r, t.g, t.b);
            double r = t.r + (y - t.r) * Desat, g = t.g + (y - t.g) * Desat, bl = t.b + (y - t.b) * Desat;
            return new[] { r + (b.r - r) * BaseMix, g + (b.g - g) * BaseMix, bl + (b.b - bl) * BaseMix };
        }

        static void Scale(double[] c, double k) { c[0] *= k; c[1] *= k; c[2] *= k; }
        static double Y(double[] c) => NocMath.Luma709(c[0], c[1], c[2]);

        /// <summary>Linear colour of ramp stop i (0..4: teal, ice, amber, cyan, teal) after the desaturation, the ScreenBase mix and the
        /// k 0.8 luminance normalisation, with the amber stop at weight amber (0 = the ice stop).</summary>
        public static D3 StopColour(int i, double amber)
        {
            int k = i % 4;
            var s = new D3(s_stops[3 * k], s_stops[3 * k + 1], s_stops[3 * k + 2]);
            if (k != 2) return s;
            var w = new D3(s_stops[3], s_stops[4], s_stops[5]);
            double f = NocMath.Clamp01(amber);
            return w + (s - w) * f;
        }
        public static D3 StopColour(int i) => StopColour(i, AmberWeight);

        /// <summary>Ramp colour at index h (fract taken; linear interpolation between the five stops) at the amber weight.</summary>
        public static D3 Ramp(double h, double amber)
        {
            double x = (h - Math.Floor(h)) * 4.0;
            int i = Math.Min(3, (int)Math.Floor(x));
            double f = x - i;
            D3 a = StopColour(i, amber), b = StopColour(i + 1, amber);
            return a + (b - a) * f;
        }
        /// <summary>Ramp colour at the current frame's amber weight (AmberWeight).</summary>
        public static D3 Ramp(double h) => Ramp(h, AmberWeight);

        /// <summary>The accent colour of the imagery: the ramp one stop on, pushed away from its luminance x 1.3 (NocScrSat).</summary>
        public static D3 Accent(double h, double amber)
        {
            D3 c = Ramp(h + 0.25, amber);
            double y = NocMath.Luma709(c.x, c.y, c.z);
            return new D3(Math.Max(0.0, y + (c.x - y) * AccentSat), Math.Max(0.0, y + (c.y - y) * AccentSat), Math.Max(0.0, y + (c.z - y) * AccentSat));
        }

        /// <summary>The time phases the driver uploads to _NocScreenT: (frac(t / 58), frac(t / 60), t mod 600).</summary>
        public static void TimePhases(double t, out double p58, out double p60, out double t600)
        {
            p58 = t / 58.0 - Math.Floor(t / 58.0);
            p60 = t / 60.0 - Math.Floor(t / 60.0);
            t600 = t - 600.0 * Math.Floor(t / 600.0);
        }

        /// <summary>Colour index h at a wall point (z, y) for the uploaded phases (waves = 1 or 3).</summary>
        public static double Index(double z, double y, double p58, double p60, int waves)
        {
            double h = 0.5 + 0.28 * Math.Sin(Tau * ((0.85 * z + 0.53 * y) / 520.0 - p58));
            if (waves > 1)
            {
                h += 0.18 * Math.Sin(Tau * ((-0.60 * z + 0.80 * y) / 360.0 + p60));
                h += 0.12 * Math.Sin(Tau * ((0.30 * z - 0.95 * y) / 700.0 - p58));
            }
            return h - Math.Floor(h);
        }

        /// <summary>Colour index at scene time t.</summary>
        public static double IndexAt(double z, double y, double t, int waves)
        {
            TimePhases(t, out double p58, out double p60, out _);
            return Index(z, y, p58, p60, waves);
        }

        /// <summary>Unbroken panel emission of the wave alone (no imagery, no hardware, breath at its mean) at (z, y) and time t:
        /// ramp x 0.45 x 0.55.</summary>
        public static D3 Emission(double z, double y, double t, int waves)
            => Ramp(IndexAt(z, y, t, waves)) * (SignsLayout.EmitGain * SignsLayout.PanelLevel);

        /// <summary>Relative luminance of the unbroken wave emission.</summary>
        public static double Luminance(double z, double y, double t, int waves)
        {
            D3 c = Emission(z, y, t, waves);
            return NocMath.Luma709(c.x, c.y, c.z);
        }

        /// <summary>Panel breath of panel (iz, iy): frequency n / 600 Hz, n = 30..60, and its phase (the shader's hash, in C#).</summary>
        public static void PanelBreath(int iz, int iy, out int n, out double phase)
        {
            uint h = NocHash.H3(unchecked((uint)(iz + 4096)), unchecked((uint)(iy + 4096)), 0x5343524Eu, 0);
            n = 30 + (int)Math.Floor(31.0 * NocHash.U01(h));
            phase = NocHash.U01(NocMath.Pcg(h));
        }

        /// <summary>Panel level 0.55 + 0.05 sin(2 pi frac(n t600 / 600 + phase)).</summary>
        public static double PanelLevel(int iz, int iy, double t600)
        {
            PanelBreath(iz, iy, out int n, out double ph);
            double x = n * t600 / 600.0 + ph;
            return SignsLayout.PanelLevel + SignsLayout.BreathAmp * Math.Sin(Tau * (x - Math.Floor(x)));
        }

        // ------------------------------------------------------------------ imagery (NocScrDisplay / NocScrProgram / NocScreenContent)
        static uint P3(uint a, uint b, uint c, int k) => NocHash.H3(a, b, c, k);
        static double U(uint h) => NocHash.U01(h);

        /// <summary>The display of a wall point: id, local position (m from its lower left corner) and size (m).</summary>
        public static void Display(double zl, double y, out uint id, out double lx, out double ly, out double w, out double h)
        {
            double c = Math.Floor(zl / DisplayW);
            uint cu = unchecked((uint)(int)(c + 64.0));
            uint hc = P3(cu, 0x44495350u, 7u, 0);
            uint nhu = 3u + hc % 5u;
            double nh = nhu, off = (hc >> 4) % nhu;
            double rowf = (y / PitchY + off) / nh, r = Math.Floor(rowf);
            id = P3(cu, unchecked((uint)(int)(r + 4096.0)), 0x53435244u, 0);
            w = DisplayW; h = nh * PitchY;
            lx = zl - c * DisplayW; ly = (rowf - r) * h;
        }

        /// <summary>Display period (s): 40, 50, 60 or 75 (each divides 600).</summary>
        public static double Period(uint id) { uint k = (id >> 3) % 4u; return k == 0u ? 40.0 : (k == 1u ? 50.0 : (k == 2u ? 60.0 : 75.0)); }

        /// <summary>Programme of display id in epoch e (ProgramNames).</summary>
        public static uint ProgramOf(uint id, uint e)
        {
            double u = U(P3(id, e, 0x50524F47u, 0));
            for (uint p = 0; p < ProgramCum.Length; p++) if (u < ProgramCum[p]) return p;
            return (uint)(ProgramCount - 1);
        }

        /// <summary>Display level of epoch e (NocScrLevel).</summary>
        public static double Level(uint id, uint e)
        {
            double u = U(P3(id, e, 0x4C45564Cu, 0));
            return u < 0.25 ? LevelLow : (u < 0.75 ? LevelMid : LevelHigh);
        }

        static double Capsule(double px, double py, double ax, double ay, double bx, double by)
        {
            double pax = px - ax, pay = py - ay, bax = bx - ax, bay = by - ay;
            double h = NocMath.Clamp01((pax * bax + pay * bay) / Math.Max(bax * bax + bay * bay, 1e-8));
            double dx = pax - bax * h, dy = pay - bay * h;
            return Math.Sqrt(dx * dx + dy * dy);
        }

        /// <summary>Distance (m) to the 3-5 strokes of glyph `seed` in a cell cw x ch at the cell-local point (px, py).</summary>
        public static double GlyphDist(double px, double py, double cw, double ch, uint seed)
        {
            double padX = 0.16 * cw, padY = 0.10 * ch, lx = (cw - 2.0 * padX) / 3.0, ly = (ch - 2.0 * padY) / 5.0;
            uint count = 3u + seed % 3u;
            double d = 1e6;
            for (uint k = 0; k < count; k++)
            {
                uint hs = P3(seed, k + 17u, 0x53435247u, 1);
                int a0 = (int)(hs & 3u), b0 = (int)((hs >> 2) % 6u);
                uint dirK = (hs >> 5) & 3u;
                int len = 1 + (int)((hs >> 7) % 3u);
                int dx = dirK == 0u ? 1 : (dirK == 1u ? 0 : 1), dy = dirK == 0u ? 0 : (dirK == 1u ? 1 : (dirK == 2u ? 1 : -1));
                int ex = Math.Max(0, Math.Min(3, a0 + dx * len)), ey = Math.Max(0, Math.Min(5, b0 + dy * len));
                d = Math.Min(d, Capsule(px, py, padX + a0 * lx, padY + b0 * ly, padX + ex * lx, padY + ey * ly));
            }
            return d;
        }

        static double SmoothStep(double a, double b, double x) => NocMath.Smoothstep(a, b, x);
        static double Cover(double x, double fw, double x0, double x1) => NocMath.Clamp01((Math.Min(x + 0.5 * fw, x1) - Math.Max(x - 0.5 * fw, x0)) / Math.Max(fw, 1e-6));
        static double Frac(double x) => x - Math.Floor(x);
        static uint Iu(double x) => unchecked((uint)(int)x);

        /// <summary>Hue offset of epoch e on the ramp (NocScrHue): a quarter turn +- 0.04.</summary>
        public static double HueOffset(uint id, uint e)
        {
            uint h = P3(id, e, 0x48554531u, 0);
            return (h % 4u) * 0.25 + 0.08 * (U(NocMath.Pcg(h)) - 0.5);
        }

        /// <summary>Value-noise fbm of 2 or 3 octaves, normalised (NocScrFbm).</summary>
        public static double Fbm(double x, double y, int oct)
        {
            double s = 0.0, a = 0.5, f = 1.0, n = 0.0;
            for (int o = 0; o < oct; o++)
            {
                s += a * NocMath.ValueNoise(x * f + 17.3 * o, y * f - 5.1 * o);
                n += a; a *= 0.5; f *= 2.03;
            }
            return s / n;
        }

        /// <summary>One programme (m, a) (twin of NocScrProgram); soft = the pixel footprint (m).</summary>
        public static void Program(uint p, uint seed, double lx, double ly, double w, double h, double tau, double soft, out double m, out double a)
        {
            double u1 = U(NocMath.Pcg(seed ^ 0x55310000u)), u2 = U(NocMath.Pcg(seed ^ 0x55320000u)), u3 = U(NocMath.Pcg(seed ^ 0x55330000u));
            m = 1.15; a = 0.0;
            switch (p)
            {
                case 0u:
                {
                    double bb = 0.5 + 0.5 * Math.Sin(Tau * ((ly + (u2 - 0.5) * 0.6 * lx - 0.1 * tau) / (40.0 + 30.0 * u1)));
                    m = 0.65 + 1.3 * bb * bb;
                    break;
                }
                case 1u:
                {
                    double cw = 0.62 * w, ch = Math.Min(0.80 * h, cw * (18.0 / 14.0));
                    double d = GlyphDist(lx - 0.5 * (w - cw), ly - 0.5 * (h - ch), cw, ch, seed);
                    double r = 0.05 * cw, e = Math.Max(soft, Edge);
                    double core = 1.0 - SmoothStep(r - e, r + e, d);
                    double glow = Math.Exp(-(d * d) / Math.Max(16.0 * r * r, 1e-4)) * (0.75 + 0.25 * Math.Sin(Tau * (tau / 40.0 + u3)));
                    double vg = 1.0 - 0.5 * SmoothStep(0.2, 0.8, Math.Abs(ly / h - 0.5) * 2.0);
                    double rx = (lx - 0.5 * w) / w, ry = (ly - 0.5 * h) / h;
                    double field = 0.125 + 0.30 * Math.Exp(-(rx * rx + ry * ry) / 0.12);
                    m = (field + 1.3 * glow) * vg + 3.4 * core; a = core;
                    break;
                }
                case 2u:
                {
                    double sc = 1.0 / (18.0 + 12.0 * u1);
                    double qx = lx * sc + 31.0 * u2, qy = (ly - 0.07 * tau) * sc + 57.0 * u3;
                    double wx = Fbm(qx * 0.7 + 3.1, qy * 0.7, 2) - 0.5, wy = Fbm(qx * 0.7, qy * 0.7 + 7.7, 2) - 0.5;
                    double n = Fbm(qx + 2.2 * wx, qy + 2.2 * wy, 3);
                    double pool = SmoothStep(0.38, 0.78, n);
                    double sw = 0.6 * soft * sc, s2 = 0.0018 + sw * sw;
                    double ridge = Math.Exp(-((n - 0.62) * (n - 0.62)) / s2) * Math.Sqrt(0.0018 / s2);
                    m = 0.26 + 2.6 * pool * pool + 1.0 * ridge; a = NocMath.Clamp01(0.4 * pool * pool + 0.8 * ridge);
                    break;
                }
                case 3u:
                {
                    double cx = w * (0.3 + 0.4 * u1) + 0.08 * (tau - 30.0) * (u3 < 0.5 ? 1.0 : -1.0), cy = h * (0.3 + 0.4 * u2);
                    double R = 0.26 * Math.Min(w, h), rho = Math.Sqrt((lx - cx) * (lx - cx) + (ly - cy) * (ly - cy)), e = 1.0 + soft;
                    double sun = 1.0 - SmoothStep(R - e, R + e, rho);
                    double halo = Math.Exp(-(rho * rho) / (2.6 * R * R));
                    double lam = 7.0, dr = Math.Abs(Frac((rho - 0.15 * tau) / lam) - 0.5) * lam, rr = (rho - R) / (1.6 * R);
                    double ring = (1.0 - SmoothStep(0.3, 1.0 + soft, dr)) * SmoothStep(R, R + 2.0 + soft, rho) * Math.Exp(-rr * rr);
                    ring *= SmoothStep(1.0, 3.0, lam / Math.Max(soft, 1e-3));
                    m = 0.18 + 0.30 * ly / h + 1.1 * halo + 2.6 * sun + 0.9 * ring; a = NocMath.Clamp01(sun + 0.5 * ring);
                    break;
                }
                case 4u:
                {
                    double cw = 0.24 * w, ch = cw * (18.0 / 14.0);
                    double x0 = w * (0.25 + 0.5 * u1) - 0.5 * cw;
                    double ys = ly - ColumnSpeed * tau, j = Math.Floor(ys / ch);
                    double qx = lx - x0, qy = ys - j * ch;
                    double d = GlyphDist(qx, qy, cw, ch, P3(seed, Iu(j + 4096.0), 0x434F4C4Du, 0));
                    double r = 0.075 * cw, e = Math.Max(soft, 1.0);
                    double core = 1.0 - SmoothStep(r - e, r + e, d);
                    double inS = (1.0 - SmoothStep(-1.0, 1.0, -qx)) * (1.0 - SmoothStep(cw - 1.0, cw + 1.0, qx));
                    m = 1.05 + (0.20 + 3.0 * core - 1.05) * inS; a = core * inS;
                    break;
                }
                case 5u:
                {
                    double hz = h * (0.10 + 0.10 * u1), bw = 3.0 + 3.0 * u2;
                    double bi = Math.Floor(lx / bw);
                    double hb = U(P3(Iu(bi + 4096.0), seed, 0x534B594Cu, 0));
                    double top = hz + h * (0.08 + 0.40 * hb * hb);
                    double inB = 1.0 - SmoothStep(top - 0.3 - soft, top + 0.3 + soft, ly);
                    double wi = Math.Floor(lx / 1.2), wj = Math.Floor(ly / 1.2);
                    uint hw = P3(Iu(wi + 4096.0), Iu(wj + 4096.0), seed, 0);
                    double lit = U(hw) < 0.32 ? 1.0 : 0.0;
                    double tw = 40.0 + 40.0 * U(NocMath.Pcg(hw ^ 0x77u));
                    lit *= 0.6 + 0.4 * Math.Sin(Tau * (tau / tw + U(NocMath.Pcg(hw))));
                    double win = Cover(lx - (wi + 0.5) * 1.2, soft, -0.336, 0.336) * Cover(ly - (wj + 0.5) * 1.2, soft, -0.264, 0.264) * lit;
                    double wf = SmoothStep(2.0, 4.0, 1.2 / soft);
                    win = 0.0473 + (win - 0.0473) * wf;
                    double sky = 0.35 + 2.4 * Math.Exp(-Math.Max(ly - hz, 0.0) / (0.45 * h)) * SmoothStep(-2.0, 6.0, ly - 0.5 * hz);
                    m = sky * (1.0 - inB) + inB * (0.10 + 2.2 * win); a = (1.0 - inB) * 0.12 + inB * win;
                    break;
                }
                case 6u:
                {
                    double k = u1 < 1.0 / 3.0 ? 6.0 : (u1 < 2.0 / 3.0 ? 8.0 : 10.0);
                    double cw = w / k, ch = w / k * (18.0 / 14.0);
                    double ci = Math.Floor(lx / cw), cj = Math.Floor(ly / ch), qx = lx - ci * cw, qy = ly - cj * ch;
                    uint hc = P3(Iu(ci + 64.0), Iu(cj + 4096.0), seed, 0);
                    double d = GlyphDist(qx, qy, cw, ch, hc);
                    double r = 0.07 * cw, e = Math.Max(soft, Edge);
                    double core = 1.0 - SmoothStep(r - e, r + e, d);
                    double tt = 36.0 + 36.0 * U(NocMath.Pcg(hc)), ph = U(NocMath.Pcg(hc ^ 0x9E37u));
                    double b = 0.5 + 0.5 * Math.Sin(Tau * (tau / tt + ph));
                    b *= b;
                    double on = U(NocMath.Pcg(hc ^ 0x51u)) < 0.8 ? 1.0 : 0.0;
                    double gut = Cover(qx, soft, 0.18, cw - 0.18) * Cover(qy, soft, 0.18, ch - 0.18);
                    double plate = (0.35 + 0.95 * U(NocMath.Pcg(hc ^ 0x504Cu))) * (0.8 + 0.2 * Math.Sin(Tau * (tau / (2.0 * tt) + ph))) * (1.0 - 0.35 * qy / ch);
                    m = 0.16 + gut * (plate + core * on * (0.6 + 3.0 * b)); a = gut * core * on * b;
                    break;
                }
                case 7u:
                {
                    m = 0.30 + 0.45 * ly / h;
                    for (uint layer = 0u; layer < 2u; layer++)
                    {
                        double g = 16.0 - 6.0 * layer;
                        double gi = Math.Floor(lx / g), gj = Math.Floor(ly / g);
                        uint hx = Iu(gi + 4096.0), hy = Iu(gj + 4096.0), hz = unchecked(seed + layer);
                        NocHash.Pcg3d(ref hx, ref hy, ref hz);
                        double cx = (gi + 0.38 + 0.24 * U(hx)) * g, cy = (gj + 0.38 + 0.24 * U(hy)) * g;
                        double rr = g * (0.18 + 0.12 * U(hz)), dd = Math.Sqrt((lx - cx) * (lx - cx) + (ly - cy) * (ly - cy)), ew = 0.05 * g;
                        double disc = 1.0 - SmoothStep(rr - ew, rr + ew, dd);
                        double rim = Math.Exp(-((dd - rr) * (dd - rr)) / (0.0016 * g * g)) * 0.35;
                        double fd = SmoothStep(0.02 * g, 0.06 * g, soft);
                        disc = disc + (3.14159265 * rr * rr / (g * g) - disc) * fd;
                        rim *= 1.0 - fd;
                        double tt = 20.0 + 20.0 * U(NocMath.Pcg(hy ^ 0x77u));
                        double br = (0.8 + 1.6 * U(NocMath.Pcg(hz))) * (U(NocMath.Pcg(hx ^ 5u)) < 0.75 ? 1.0 : 0.0) * (0.55 + 0.45 * Math.Sin(Tau * (tau / tt + U(hz))));
                        m += br * (disc * (0.7 + 0.3 * U(NocMath.Pcg(hy))) + rim) * (0.9 - 0.3 * layer);
                        a = Math.Max(a, disc * (U(NocMath.Pcg(hy ^ 3u)) < 0.4 ? 1.0 : 0.0));
                    }
                    break;
                }
                case 8u:
                {
                    double sp = 2.0 + 2.0 * u1;
                    double li = Math.Floor(ly / sp);
                    double wl = 0.2 + 0.25 * U(P3(Iu(li + 4096.0), seed, 0x4C494E45u, 0));
                    double ln = Cover(ly - (li + 0.5) * sp, soft, -wl, wl);
                    ln = 0.65 / sp + (ln - 0.65 / sp) * SmoothStep(3.0, 6.0, sp / soft);
                    double b = 0.5 + 0.5 * Math.Sin(Tau * (li * 0.07 - tau / 36.0));
                    b *= b;
                    double x0 = w * (0.04 + 0.12 * u2), x1 = w * (0.84 + 0.12 * u3), ew = 0.3 + soft;
                    double span = SmoothStep(x0 - ew, x0 + ew, lx) * (1.0 - SmoothStep(x1 - ew, x1 + ew, lx));
                    m = 0.15 + 0.45 * ly / h + ln * span * (1.2 + 2.6 * b); a = ln * span * b;
                    break;
                }
                case 9u:
                    m = 0.20;
                    break;
            }
        }

        /// <summary>The imagery (m, a) at wall point (zl = z - 275, y) for the wrapped clock t600, far field (wp 0).</summary>
        public static void Content(double zl, double y, double t600, double soft, out double m, out double a)
            => Content(zl, y, t600, soft, 0.0, out m, out a, out _);

        /// <summary>The imagery (twin of NocScreenContent): m x level x ContentNorm capped at ContentMaxM / (1 + DotGain wp), the accent
        /// weight a and the hue offset dh, crossfaded over the last 16 s of each epoch.</summary>
        public static void Content(double zl, double y, double t600, double soft, double wp, out double m, out double a, out double dh)
        {
            Display(zl, y, out uint id, out double lx, out double ly, out double w, out double h);
            double T = Period(id), N = 600.0 / T;
            double tau = t600 + U(NocMath.Pcg(id ^ 0x50484153u)) * T;
            double ef = Math.Floor(tau / T), x = tau / T - ef;
            uint e0 = (uint)(ef % N), e1 = (uint)((ef + 1.0) % N);
            double wgt = SmoothStep(1.0 - Crossfade / T, 1.0, x);
            double cap = ContentMaxM / (1.0 + DotGain * wp);
            uint s0 = P3(id, e0, 0x53454544u, 0), s1 = P3(id, e1, 0x53454544u, 0);
            Program(ProgramOf(id, e0), s0, lx, ly, w, h, x * T, soft, out double m0, out double a0);
            m0 = Math.Min(m0 * Level(id, e0) * ContentNorm, cap);
            double m1 = m0, a1 = a0;
            if (wgt > 0.0)
            {
                Program(ProgramOf(id, e1), s1, lx, ly, w, h, (x - 1.0) * T, soft, out m1, out a1);
                m1 = Math.Min(m1 * Level(id, e1) * ContentNorm, cap);
            }
            m = m0 + (m1 - m0) * wgt; a = a0 + (a1 - a0) * wgt;
            double d0 = HueOffset(id, e0), dd = HueOffset(id, e1) - d0;
            dh = d0 + (dd - Math.Round(dd, MidpointRounding.AwayFromZero)) * wgt;
        }

        // ------------------------------------------------------------------ the eye (D-034 5.3, fix wave C28; twin of NocScrEye)
        /// <summary>The eye's canvas: 4 x 5 panels (z 311.4 - 457.0, y 20.4 - 122.4) and the eye's centre.</summary>
        public const double EyeZ0 = 311.4, EyeZ1 = 457.0, EyeY0 = 20.4, EyeY1 = 122.4, EyeCz = 384.2, EyeCy = 71.4;
        /// <summary>Clock (t mod 600) of the event: canvas in from 448, opening from 454, closing from 472, canvas out from 478, each
        /// over 6 s (smoothstep). KS16's readable window is t 453.6 - 478.4 at 2.5 m/s; the KS16 still is t 460 (fully open).</summary>
        public const double EyeIn0 = 448.0, EyeOpen0 = 454.0, EyeClose0 = 472.0, EyeOut0 = 478.0, EyeRamp = 6.0;

        static double S01(double t) { t = NocMath.Clamp01(t); return t * t * (3.0 - 2.0 * t); }
        static D3 Lerp(D3 a, D3 b, double t) => a + (b - a) * t;

        /// <summary>The canvas weight of the eye event at the wrapped clock t600 and the lid opening (0 closed, 1 open).</summary>
        public static double EyeEnvelope(double t600, out double open)
        {
            open = S01((t600 - EyeOpen0) / EyeRamp) * (1.0 - S01((t600 - EyeClose0) / EyeRamp));
            return S01((t600 - EyeIn0) / EyeRamp) * (1.0 - S01((t600 - EyeOut0) / EyeRamp));
        }

        /// <summary>The eye canvas at world (z, y): colour x m in the mosaic's units (x LevelMid x ContentNorm, capped at cap) and its
        /// weight w (0 outside the event or the canvas). soft = the pixel footprint (m).</summary>
        public static D3 Eye(double z, double y, double t600, double soft, double cap, double amber, out double w)
        {
            double env = EyeEnvelope(t600, out double open);
            w = env * Cover(z, soft, EyeZ0, EyeZ1) * Cover(y, soft, EyeY0, EyeY1);
            if (w <= 0.0) { w = 0.0; return new D3(0, 0, 0); }
            double px = z - EyeCz, py = y - EyeCy;
            double u = NocMath.Clamp01(Math.Abs(px) / 64.0);
            double bow = Math.Pow(Math.Max(1.0 - u * u, 0.0), 0.85);
            double e = Math.Max(10.0, soft);
            double yu = 34.0 * open * bow + 0.8, yl = -0.72 * 34.0 * open * bow - 0.8;
            double eyeA = SmoothStep(yl - e, yl + e, py) * (1.0 - SmoothStep(yu - e, yu + e, py)) * (1.0 - SmoothStep(64.0 - 2.0 * e, 64.0, Math.Abs(px)));
            double qx = px + 9.0, qy = py + 4.0;
            double rr = Math.Sqrt(qx * qx + qy * qy);
            double irisA = 1.0 - SmoothStep(21.0, 27.0 + soft, rr);
            double pupilA = 1.0 - SmoothStep(7.0, 12.0 + soft, rr);
            double ang = Math.Atan2(qy, qx);
            double fib = 0.5 + 0.3 * Math.Cos(40.0 * ang + 5.0 * Math.Sin(3.0 * ang) + 0.35 * rr) + 0.2 * Math.Cos(73.0 * ang + 3.0 * Math.Cos(5.0 * ang) - 0.21 * rr);
            fib = 0.5 + (fib - 0.5) * SmoothStep(2.0, 4.0, Tau * rr / 73.0 / Math.Max(soft, 0.1));
            double cx = qx + 6.0, cy = qy - 8.0;
            double cl = Math.Exp(-(cx * cx + cy * cy) / 20.0);
            double ms = 1.6 * (1.0 - 0.45 * u * u) * (1.0 - 0.35 * SmoothStep(yu - 16.0, yu, py));
            double mi = 1.25 * (0.6 + 0.8 * fib) * (0.55 + 0.45 * SmoothStep(9.5, 24.0, rr)) * (1.0 - 0.55 * SmoothStep(18.0, 24.0, rr));
            double dc = py - 34.0 * bow - 10.0;
            double gx = px / 84.0, gy = py / 50.0;
            double mf = 0.20 + 0.34 * Math.Exp(-(gx * gx + gy * gy)) + 0.45 * Math.Exp(-dc * dc / 18.0) * bow * open;
            double k = LevelMid * ContentNorm;
            D3 cS = Ramp(0.25, amber), cI = Accent(0.27, amber), cF = Ramp(0.0, amber);
            D3 inside = Lerp(cS * Math.Min(ms * k, cap), cI * Math.Min(mi * k, cap), irisA);
            inside = Lerp(inside, cS * (0.04 * k), pupilA);
            inside = Lerp(inside, cS * Math.Min(1.6 * k, cap), cl);
            return Lerp(cF * Math.Min(mf * k, cap), inside, eyeA);
        }

        // ------------------------------------------------------------------ hardware (per panel; the far-field means)
        /// <summary>Panel (iz, iy): brightness kb, white point tint, aged, dead panel factor, dead-module mean, stuck column (-1 none).</summary>
        public static void Panel(int iz, int iy, out double kb, out D3 tint, out bool aged, out double dead, out double moduleMean, out int stuckColumn)
        {
            uint hx = unchecked((uint)(iz + 4096)), hy = unchecked((uint)(iy + 4096)), hz = 0x50414E4Cu;
            NocHash.Pcg3d(ref hx, ref hy, ref hz);
            double u1 = U(hx), u2 = U(hy), u3 = U(hz), u4 = U(NocMath.Pcg(hx ^ 0x41474544u)), u5 = U(NocMath.Pcg(hy ^ 0x44454144u));
            aged = u4 < AgedFraction;
            kb = (0.92 + 0.12 * u1) * (aged ? 0.86 : 1.0);
            tint = new D3(1.0 + 0.10 * (u2 - 0.5), 1.0 + 0.02 * (u3 - 0.5), 1.0 - 0.10 * (u2 - 0.5));
            if (aged) tint = new D3(tint.x * 1.04, tint.y, tint.z * 0.86);
            dead = u5 < DeadPanel ? 0.04 : 1.0;
            moduleMean = u5 >= DeadPanel && u5 < DeadModulePanels ? 1.0 - DeadModuleFraction * 0.96 : 1.0;
            stuckColumn = u5 >= DeadModulePanels && u5 < StuckPanels ? (int)(hz % 9u) : -1;
        }

        /// <summary>Mean emission factor of the seams and bezels over a panel pitch (unresolved): (36.4 - 0.64 + 0.1 x 0.4) / 36.4 per axis.</summary>
        public static double SeamMean
        {
            get
            {
                double fz = (PitchZ - 2.0 * HalfBezel + GapScatter * 2.0 * HalfGap) / PitchZ, fy = (PitchY - 2.0 * HalfBezel + GapScatter * 2.0 * HalfGap) / PitchY;
                return fz * fy * (1.0 - JointDark * (1.0 - (1.0 - Joint / Module) * (1.0 - Joint / Module)));
            }
        }

        /// <summary>The LED view-angle factor of the luminance at cos(view, normal) = cosV (the shader's lerp toward (0.67, 0.71, 0.74)
        /// with cos^0.4).</summary>
        public static double AngleFactor(double cosV)
        {
            double sc = Math.Pow(NocMath.Clamp01(cosV) + 1e-4, AngleExp);
            return AngleFloor + (1.0 - AngleFloor) * sc;
        }

        /// <summary>Far-field (unresolved modules and pixels) emission of a wall pixel at (z, y), scene time t, waves and imagery
        /// strength `content`, seen frontally (cosV 1): the quantity T-WAVE and the photosensitivity model check.</summary>
        public static D3 PixelEmission(double z, double y, double t, int waves, double content, double amber)
        {
            TimePhases(t, out double p58, out double p60, out double t600);
            double h = Index(z, y, p58, p60, waves);
            double m = 1.0, a = 0.0, dh = 0.0;
            if (content > 0.0) Content(z - Z0, y, t600, 0.5, 0.0, out m, out a, out dh);
            h += dh * content;
            D3 c1 = Ramp(h, amber), c2 = Accent(h, amber);
            D3 col = (c1 + (c2 - c1) * (a * content)) * (1.0 + (m - 1.0) * content);
            if (content > 0.0)
            {
                D3 ec = Eye(z, y, t600, 0.5, ContentMaxM, amber, out double we);
                if (we > 0.0) col = col + (ec - col) * (we * content);
            }
            int iz = (int)Math.Floor((z - Z0) / PitchZ), iy = (int)Math.Floor(y / PitchY);
            Panel(iz, iy, out double kb, out D3 tint, out _, out double dead, out double mm, out _);
            double level = PanelLevel(iz, iy, t600);
            double k = SignsLayout.EmitGain * level * kb * dead * mm * SeamMean;
            return new D3(col.x * tint.x * k, col.y * tint.y * k, col.z * tint.z * k);
        }

        /// <summary>Relative luminance of PixelEmission.</summary>
        public static double PixelLuminance(double z, double y, double t, int waves, double content, double amber = 1.0)
        {
            D3 c = PixelEmission(z, y, t, waves, content, amber);
            return NocMath.Luma709(c.x, c.y, c.z);
        }

        /// <summary>Mean emission colour of the wall over z, y and one wave period (the wave alone: ramp x 0.45 x 0.55).</summary>
        public static D3 MeanEmission(int waves)
        {
            D3 acc = new D3(0, 0, 0);
            int n = 0;
            for (int iz = 0; iz <= 12; iz++)
                for (int iy = 0; iy <= 14; iy++)
                    for (int it = 0; it < 8; it++)
                    {
                        acc = acc + Emission(SignsLayout.WallZ0 + iz * 20.0, iy * 40.0, it * 58.0 / 8.0, waves);
                        n++;
                    }
            return acc * (1.0 / n);
        }

        /// <summary>The H4 wall-plane macro (kind 2): the mean far-field emission of the LED wall (imagery, panels, seams) over the
        /// face y 0..560 and 600 s, times the view-angle factor of a reflected ray (cosV about 0.35: 0.85). The mirror shader scales
        /// the wave at the hit to this mean luminance (NocScreenMirror).</summary>
        public static D3 MirrorMacro(int waves, double content)
        {
            D3 acc = new D3(0, 0, 0);
            int n = 0;
            for (int iz = 0; iz < 25; iz++)
                for (int iy = 0; iy < 14; iy++)
                    for (int it = 0; it < 6; it++)
                    {
                        acc = acc + PixelEmission(SignsLayout.WallZ0 + 4.9 + iz * 9.8, 3.1 + iy * 40.0, 7.0 + it * 100.0, waves, content, 1.0);
                        n++;
                    }
            return acc * (AngleFactor(0.35) / n);
        }
    }
}
