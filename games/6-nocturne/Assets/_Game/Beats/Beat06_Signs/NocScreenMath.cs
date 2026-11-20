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
//   imagery (Full tier): displays of 72.8 m x 3..7 panels, a programme per 40-75 s epoch (8 kinds) and a display level, with a
//   12 s crossfade, giving a luminance factor m (capped 2.42, mean about 1.09) and an accent weight a:
//   colour = lerp(ramp(h), lerp(ramp(h + 1/4), ice, 0.35), a) x m
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
        public const double Z0 = 275.0, PitchZ = 36.4, PitchY = 20.4, HalfGap = 0.2, HalfBezel = 0.32, Module = 4.0, Joint = 0.05;
        public const double LedPitch = 0.25, LedDot = 0.66, DisplayW = 72.8, Crossfade = 12.0;
        /// <summary>LED dots: faded in from 5 to 10 px of pitch, over a diffuser floor of 15 % (energy kept).</summary>
        public const double LedFade0 = 5.0, LedFade1 = 10.0, LedFloor = 0.15;
        /// <summary>Glyph-column scroll speed (m/s; SCR_COLV): its 2.6 m strokes with 2 m soft edges pass a pixel at &lt;= 0.06 Hz.</summary>
        public const double ColumnSpeed = 0.15;
        /// <summary>Light the 0.4 m gap scatters (fraction of the panel's), joint darkening, LED view-angle floor at grazing and its exponent.</summary>
        public const double GapScatter = 0.10, JointDark = 0.85, AngleFloor = 0.70, AngleExp = 0.4;
        /// <summary>Failure rates per panel: dead (1.2 %), dead modules (1.8 %: 8 % of their modules), a stuck module column (1 %); aged 18 %.</summary>
        public const double DeadPanel = 0.012, DeadModulePanels = 0.030, StuckPanels = 0.040, DeadModuleFraction = 0.08, AgedFraction = 0.18;
        /// <summary>Programme weights (cumulative thresholds: wave, glyph, aurora, sun, glyph column, horizon, tiles, standby) and the
        /// cap of m (the bloom check).</summary>
        public static readonly double[] ProgramCum = { 0.14, 0.32, 0.42, 0.58, 0.68, 0.84, 0.92, 1.0 };
        public const int ProgramCount = 8;
        public const double ContentMaxM = 2.42;
        /// <summary>Display levels per epoch (25 / 50 / 25 %).</summary>
        public const double LevelLow = 0.95, LevelMid = 1.2, LevelHigh = 1.5;

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

        /// <summary>The accent colour of the imagery: the ramp one stop on, mixed 35 % toward the ice stop.</summary>
        public static D3 Accent(double h, double amber)
        {
            D3 c = Ramp(h + 0.25, amber), ice = StopColour(1, amber);
            return c + (ice - c) * 0.35;
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

        /// <summary>Programme of display id in epoch e (0 wave, 1 glyph, 2 aurora, 3 sun, 4 glyph column, 5 horizon, 6 tiles, 7 standby).</summary>
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

        /// <summary>One programme (m, a) (twin of NocScrProgram); panelX / panelY = the panel's lattice index (tiles).</summary>
        public static void Program(uint p, uint seed, double lx, double ly, double w, double h, double tau, double soft,
            int panelX, int panelY, out double m, out double a)
        {
            double u1 = U(NocMath.Pcg(seed ^ 0x55310000u)), u2 = U(NocMath.Pcg(seed ^ 0x55320000u)), u3 = U(NocMath.Pcg(seed ^ 0x55330000u));
            m = 1.15; a = 0.0;
            switch (p)
            {
                case 1u:
                {
                    double cw = 0.62 * w, ch = Math.Min(0.80 * h, cw * (18.0 / 14.0));
                    double ox = 0.5 * (w - cw), oy = 0.5 * (h - ch) + 0.12 * (tau - 30.0);
                    double d = GlyphDist(lx - ox, ly - oy, cw, ch, seed);
                    double r = 0.05 * cw, e = Math.Max(soft, 0.6);
                    double core = 1.0 - SmoothStep(r - e, r + e, d);
                    double glow = Math.Exp(-(d * d) / Math.Max(9.0 * r * r, 1e-4));
                    double vg = 1.0 - 0.35 * SmoothStep(0.2, 0.75, Math.Abs(ly / h - 0.5) * 2.0);
                    m = (0.55 + 0.75 * glow) * vg + 1.7 * core; a = core;
                    break;
                }
                case 2u:
                {
                    double lam = 30.0 + 15.0 * Math.Floor(3.0 * u1), v = 0.4 + 0.2 * u2;
                    double b = 0.5 + 0.5 * Math.Sin(Tau * ((ly + (u3 - 0.5) * 0.4 * lx - v * tau) / lam));
                    double b2 = 0.5 + 0.5 * Math.Sin(Tau * ((0.7 * ly - (u3 - 0.5) * 0.6 * lx + 0.7 * v * tau) / (0.62 * lam)));
                    m = 0.45 + 1.1 * b * b + 0.5 * b2 * b; a = b * b2;
                    break;
                }
                case 3u:
                {
                    double cx = w * (0.3 + 0.4 * u1) + 0.08 * (tau - 30.0) * (u3 < 0.5 ? 1.0 : -1.0), cy = h * (0.3 + 0.4 * u2);
                    double R = 0.30 * Math.Min(w, h), rho = Math.Sqrt((lx - cx) * (lx - cx) + (ly - cy) * (ly - cy)), e = 1.5 + soft;
                    double sun = 1.0 - SmoothStep(R - e, R + e, rho);
                    double halo = Math.Exp(-(rho * rho) / (3.24 * R * R));
                    m = 0.45 + 0.25 * ly / h + 0.9 * halo + 1.0 * sun; a = sun;
                    break;
                }
                case 4u:
                {
                    double cw = 0.24 * w, ch = cw * (18.0 / 14.0);
                    double x0 = w * (0.25 + 0.5 * u1) - 0.5 * cw;
                    double ys = ly - ColumnSpeed * tau, j = Math.Floor(ys / ch);
                    double qx = lx - x0, qy = ys - j * ch;
                    double d = GlyphDist(qx, qy, cw, ch, P3(seed, unchecked((uint)(int)(j + 4096.0)), 0x434F4C4Du, 0));
                    double r = 0.075 * cw, e = Math.Max(soft, 1.0);
                    double core = 1.0 - SmoothStep(r - e, r + e, d);
                    double inS = (1.0 - SmoothStep(-1.0, 1.0, -qx)) * (1.0 - SmoothStep(cw - 1.0, cw + 1.0, qx));
                    m = 1.25 + (0.35 + 2.0 * core - 1.25) * inS; a = core * inS;
                    break;
                }
                case 5u:
                {
                    double ang = (u1 - 0.5) * 0.5;
                    double hy = h * (0.3 + 0.4 * u2) + 0.10 * (tau - 30.0) * (u3 < 0.5 ? 1.0 : -1.0);
                    double dd = (ly - hy) * Math.Cos(ang) - (lx - 0.5 * w) * Math.Sin(ang);
                    double up = SmoothStep(-2.0 - soft, 2.0 + soft, dd);
                    double ln = Math.Exp(-(dd * dd) / (3.0 + soft * soft));
                    m = 0.5 + 0.9 * up + 1.0 * ln; a = NocMath.Clamp01(up + 0.5 * ln);
                    break;
                }
                case 6u:
                {
                    uint ht = P3(unchecked((uint)(panelX + 4096)), unchecked((uint)(panelY + 4096)), seed, 0);
                    double tt = 30.0 + 30.0 * U(ht), ph = U(NocMath.Pcg(ht));
                    double b = 0.5 + 0.5 * Math.Sin(Tau * (tau / tt + ph));
                    m = 0.45 + 1.4 * b; a = 0.6 * b;
                    break;
                }
                case 7u:
                    m = 0.30;
                    break;
            }
        }

        /// <summary>The imagery (m, a) at wall point (zl = z - 275, y) for the wrapped clock t600 (twin of NocScreenContent).</summary>
        public static void Content(double zl, double y, double t600, double soft, out double m, out double a)
        {
            Display(zl, y, out uint id, out double lx, out double ly, out double w, out double h);
            int iz = (int)Math.Floor(zl / PitchZ), iy = (int)Math.Floor(y / PitchY);
            double T = Period(id), N = 600.0 / T;
            double tau = t600 + U(NocMath.Pcg(id ^ 0x50484153u)) * T;
            double ef = Math.Floor(tau / T), x = tau / T - ef;
            uint e0 = (uint)(ef % N), e1 = (uint)((ef + 1.0) % N);
            double wgt = SmoothStep(1.0 - Crossfade / T, 1.0, x);
            uint s0 = P3(id, e0, 0x53454544u, 0), s1 = P3(id, e1, 0x53454544u, 0);
            Program(ProgramOf(id, e0), s0, lx, ly, w, h, x * T, soft, iz, iy, out double m0, out double a0);
            m0 = Math.Min(m0 * Level(id, e0), ContentMaxM);
            double m1 = m0, a1 = a0;
            if (wgt > 0.0)
            {
                Program(ProgramOf(id, e1), s1, lx, ly, w, h, (x - 1.0) * T, soft, iz, iy, out m1, out a1);
                m1 = Math.Min(m1 * Level(id, e1), ContentMaxM);
            }
            m = m0 + (m1 - m0) * wgt; a = a0 + (a1 - a0) * wgt;
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
            double m = 1.0, a = 0.0;
            if (content > 0.0) Content(z - Z0, y, t600, 0.5, out m, out a);
            D3 c1 = Ramp(h, amber), c2 = Accent(h, amber);
            D3 col = (c1 + (c2 - c1) * (a * content)) * (1.0 + (m - 1.0) * content);
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
