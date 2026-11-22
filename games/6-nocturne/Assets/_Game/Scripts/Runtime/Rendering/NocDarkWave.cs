// NocDarkWave: D-034 5.1, the darkness wave (FX, fix wave p4_01; docs/DREAD.md 5.1). Twice in the walk the lit windows of one
// named tower go dark from the top down: a front y_f = top - v tau (v = DarkWaveSpeed 150 m/s, top = the shaft top y 560) behind
// which every window fades over the facade's 1.2 s toggle (a ramp of v x 1.2 = 180 m), then after DarkWaveBack s all of them
// return together over DarkWaveBackDur s. tau = (s - s0) / 2.5: DREAD's seconds are metres at the cinematic speed (D-038 (7)), so
// the wave is a pure function of s, exact in stills and in the video, held while the player stands.
// The shader term (NocFacade.shader, NocDarkWaveK) is uniform: _NocDarkWaveA = (cx, cz, cos yaw, sin yaw) of the tower footprint,
// _NocDarkWaveB = (hx + margin, hz + margin, front y, ramp m), _NocDarkWaveC.x = dark amount (0 = no wave: one uniform branch).
// Towers: beat 2 = south slab 5 of the boulevard (x 147-177, z 86-116), in the KS05 look-up and KS05 frames (FX projection:
// 86 % / 56 % of its street face in frame); beat 7 = the ring tower nearest the centre of the KS18 frame (picked once at start).
// Zero allocations per frame (the ring list is built once).
using System;
using UnityEngine;

namespace Megastructure
{
    public static class NocDarkWave
    {
        /// <summary>D-034 5.1: at s DarkWaveS[k] one tower's windows go dark from the top down, a front at DarkWaveSpeed m/s from the
        /// shaft top y 560 to its foot y -3 300 (3 860 m in 25.7 s; the 560 m above the street in 3.7 s), each window fading over the
        /// facade's 1.2 s toggle (a 180 m ramp behind the front, never a flicker); after DarkWaveBack s they return over
        /// DarkWaveBackDur s. Seconds are metres at the cinematic 2.5 m/s (D-038 (7)).</summary>
        public static readonly double[] DarkWaveS = { 300.0, 1330.0 };
        public const double DarkWaveSpeed = 150.0, DarkWaveRampT = 1.2, DarkWaveBack = 40.0, DarkWaveBackDur = 10.0, DarkWaveMargin = 0.5;

        public struct Tower { public double cx, cz, yaw, hx, hz, y0, y1; }

        static readonly Tower[] s_towers = new Tower[2];
        static bool s_init;

        public static Tower TowerOf(int k) { Init(); return s_towers[k]; }

        static void Init()
        {
            if (s_init) return;
            s_init = true;
            const int slab = 5;
            double xa = BoulevardLayout.SlabXa(slab), xb = BoulevardLayout.SlabXb(slab);
            double za = BoulevardLayout.SouthShaftZ0, zb = BoulevardLayout.SouthShaftZ1;
            s_towers[0] = new Tower { cx = 0.5 * (xa + xb), cz = 0.5 * (za + zb), yaw = 0.0, hx = 0.5 * (xb - xa), hz = 0.5 * (zb - za),
                                      y0 = BoulevardLayout.ShaftY0, y1 = BoulevardLayout.ShaftY1 };
            s_towers[1] = PickRing();
        }

        /// <summary>The nearest ring tower whose mid-height point (y 280) lies within 35 deg of the KS18 gaze from the KS18 eye (the
        /// largest in its frame; FX_r10: the most central one, 800 m away, showed as a 24 px sliver); the most central one if none.</summary>
        static Tower PickRing()
        {
            KeyShot ks = default;
            foreach (var k in NocturneConstants.KeyShots) if (k.tag.StartsWith("KS18", StringComparison.Ordinal)) ks = k;
            double yr = ks.yaw * NocMath.Deg, pr = ks.pitch * NocMath.Deg;
            double fx = Math.Sin(yr) * Math.Cos(pr), fy = Math.Sin(pr), fz = Math.Cos(yr) * Math.Cos(pr);
            double cone = Math.Cos(35.0 * NocMath.Deg);
            Tower best = default, central = default; double bestD = double.MaxValue, bestCos = -2.0;
            foreach (var t in RingTowersBuilder.Towers())
            {
                double dx = t.cx - ks.x, dy = 280.0 - ks.y, dz = t.cz - ks.z, d = Math.Sqrt(dx * dx + dy * dy + dz * dz);
                double c = (dx * fx + dy * fy + dz * fz) / d;
                var tw = new Tower { cx = t.cx, cz = t.cz, yaw = t.yaw, hx = t.boxes[0].hx, hz = t.boxes[0].hz, y0 = t.boxes[0].y0, y1 = t.boxes[2].y1 };
                if (c > bestCos) { bestCos = c; central = tw; }
                double dh = Math.Sqrt(dx * dx + dz * dz);
                if (c >= cone && dh < bestD) { bestD = dh; best = tw; }
            }
            return bestD < double.MaxValue ? best : central;
        }

        /// <summary>Wave time (s) of wave k at progress s (negative before it starts).</summary>
        public static double Tau(int k, double s) => (s - DarkWaveS[k]) / NocturneConstants.CinematicSpeed;

        /// <summary>Dark amount 0..1 of wave k at tau (1 while dark, easing back to 0 over the return).</summary>
        public static double Amount(double tau)
        {
            if (tau <= 0.0) return 0.0;
            double b0 = DarkWaveBack, b1 = b0 + DarkWaveBackDur;
            if (tau >= b1) return 0.0;
            return tau <= b0 ? 1.0 : 1.0 - NocMath.Smootherstep(b0, b1, tau);
        }

        /// <summary>Front height of wave k at tau (m).</summary>
        public static double Front(int k, double tau) => TowerOf(k).y1 - DarkWaveSpeed * Math.Max(0.0, tau);

        public static double RampM => DarkWaveSpeed * DarkWaveRampT;

        /// <summary>The shader's window factor (1 = lit as usual) at world (x, y, z) for wave k at tau (C# twin of NocDarkWaveK).</summary>
        public static double Factor(int k, double tau, double x, double y, double z)
        {
            double amt = Amount(tau);
            if (amt <= 0.0) return 1.0;
            var t = TowerOf(k);
            double c = Math.Cos(t.yaw * NocMath.Deg), sn = Math.Sin(t.yaw * NocMath.Deg), dx = x - t.cx, dz = z - t.cz;
            double lx = dx * c - dz * sn, lz = dx * sn + dz * c;
            if (Math.Abs(lx) > t.hx + DarkWaveMargin || Math.Abs(lz) > t.hz + DarkWaveMargin) return 1.0;
            double a = NocMath.Clamp01((y - Front(k, tau)) / RampM);
            return 1.0 - amt * a * a * (3.0 - 2.0 * a);
        }

        /// <summary>Writes the uniforms for progress s (ProgressDirector.Apply, every frame and before every capture).</summary>
        public static void Apply(double s)
        {
            Init();
            int k = -1; double tau = 0.0;
            for (int i = 0; i < DarkWaveS.Length; i++)
            {
                double ti = Tau(i, s);
                if (Amount(ti) > 0.0) { k = i; tau = ti; }
            }
            if (k < 0) { Shader.SetGlobalVector(ShaderIds.DarkWaveC, Vector4.zero); return; }
            var t = s_towers[k];
            double m = DarkWaveMargin;
            Shader.SetGlobalVector(ShaderIds.DarkWaveA, new Vector4((float)t.cx, (float)t.cz, (float)Math.Cos(t.yaw * NocMath.Deg), (float)Math.Sin(t.yaw * NocMath.Deg)));
            Shader.SetGlobalVector(ShaderIds.DarkWaveB, new Vector4((float)(t.hx + m), (float)(t.hz + m), (float)Front(k, tau), (float)RampM));
            Shader.SetGlobalVector(ShaderIds.DarkWaveC, new Vector4((float)Amount(tau), 0f, 0f, 0f));
        }
    }
}
