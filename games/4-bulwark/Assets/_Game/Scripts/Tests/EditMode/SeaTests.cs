// SeaTests: EditMode tests of the sea (MASTER_PROMPT 14.7 H1, 14.10.1 T-NUM "Hs 2.80 / 2.00 / 3.20 +-0.03 with slope
// variance within 3 % of 256 components", 14.5.2 acceptance (1) "mean sea y -8.0 +-0.05 over 60 s" and (7) "at least 3
// plumes in 60 s at KS04 framing", 14.3 impact intervals, the clipmap's watertightness, the camera-relative phase precision
// at z 1500 after an hour of ambient clock, and the numbers the far field must share at the 2048 m crossover (T7).
// Everything runs on the pure C# twins in SeaState.cs (SeaSpectrum, SeaState statics) and SeaClipmap.cs.
using System;
using Megastructure.Beats;
using NUnit.Framework;
using UnityEngine;

namespace Megastructure.Tests
{
    public class SeaTests
    {
        static readonly double[] Hs = { 2.8, 2.0, 3.2 };
        static readonly double[] Tp = { 9.0, 8.0, 11.0 };

        static SeaWave[] Spectrum(int set, int n = SeaSpectrum.N)
        {
            var w = new SeaWave[n];
            SeaSpectrum.Build(Hs[set], Tp[set], n, set, w);
            return w;
        }

        // ============================================================================ spectrum (H1, T-NUM)
        [Test]
        public void Spectrum_HsIsTheSeaStateOfEachBeat()
        {
            for (int s = 0; s < 3; s++)
            {
                var w = Spectrum(s);
                double hs = SeaSpectrum.Hs(w, SeaSpectrum.N);
                Debug.Log($"[BW] sea set {s}: Hs {hs:F4} (target {Hs[s]}), peak wavelength {SeaSpectrum.PeakWavelength(Tp[s]):F1} m");
                Assert.AreEqual(Hs[s], hs, 0.03, "Hs of set " + s);
                Assert.AreEqual(Hs[s], hs, 1e-9, "scaled exactly");
            }
            Assert.AreEqual(126.0, SeaSpectrum.PeakWavelength(9.0), 1.0, "14.7 H1: peak 126 m (beat 2)");
            Assert.AreEqual(100.0, SeaSpectrum.PeakWavelength(8.0), 1.0, "peak 100 m (beat 3)");
            Assert.AreEqual(189.0, SeaSpectrum.PeakWavelength(11.0), 1.0, "peak 189 m (beat 4)");
        }

        [Test]
        public void Spectrum_GeometricWavelengthsDeepWaterOmegaWestDirections()
        {
            for (int s = 0; s < 3; s++)
            {
                var w = Spectrum(s);
                double kp = SeaSpectrum.PeakK(Tp[s]);
                Assert.AreEqual(2.0 * Math.PI / (0.6 * kp), w[0].Wavelength, 1e-6, "longest = 2 pi / (0.6 kp)");
                Assert.AreEqual(0.5, w[SeaSpectrum.N - 1].Wavelength, 1e-9, "shortest 0.5 m");
                double r = w[1].K / w[0].K;
                double azMin = 999, azMax = -999;
                for (int i = 0; i < SeaSpectrum.N; i++)
                {
                    if (i > 0) Assert.AreEqual(r, w[i].K / w[i - 1].K, 1e-9, "geometric");
                    Assert.AreEqual(Math.Sqrt(9.81 * w[i].K), w[i].Omega, 1e-12, "omega = sqrt(g k)");
                    Assert.AreEqual(1.0, w[i].DirX * w[i].DirX + w[i].DirZ * w[i].DirZ, 1e-12, "unit direction");
                    double az = Math.Atan2(w[i].DirX, w[i].DirZ) * 180.0 / Math.PI;
                    if (az < 0) az += 360.0;
                    Assert.LessOrEqual(Math.Abs(az - 270.0), 35.0 + 1e-9, "west +-35 deg");
                    azMin = Math.Min(azMin, az); azMax = Math.Max(azMax, az);
                    Assert.Less(w[i].DirX, 0.0, "every component runs toward the dam (west)");
                    Assert.That(w[i].Phase0, Is.InRange(0.0, 2.0 * Math.PI));
                }
                Assert.Less(azMin, 245.0, "the fan is used on both sides");
                Assert.Greater(azMax, 295.0);
            }
            // phases: deterministic, different per set
            var a = Spectrum(0); var b = Spectrum(0); var c = Spectrum(1);
            int same = 0;
            for (int i = 0; i < SeaSpectrum.N; i++) { Assert.AreEqual(a[i].Phase0, b[i].Phase0); if (Math.Abs(a[i].Phase0 - c[i].Phase0) < 1e-9) same++; }
            Assert.Less(same, 2, "PCG phases differ between the sea states");
        }

        [Test]
        public void Spectrum_SlopeVarianceWithin3PercentOf256Components()
        {
            for (int s = 0; s < 3; s++)
            {
                double v32 = SeaSpectrum.SlopeVariance(Spectrum(s), SeaSpectrum.N);
                double v256 = SeaSpectrum.SlopeVariance(Spectrum(s, 256), 256);
                Debug.Log($"[BW] sea set {s}: slope variance 32 = {v32:F5}, 256 = {v256:F5} ({100.0 * (v32 / v256 - 1.0):F2} %), full s2 {SeaSpectrum.S2Floor + v32:F5}, Beckmann peak {SeaSpectrum.BeckmannPeak(SeaSpectrum.S2Floor + v32):F2}");
                Assert.AreEqual(1.0, v32 / v256, 0.03, "T-NUM: within 3 % of a 256-component reference");
            }
        }

        [Test]
        public void Spectrum_FarVarianceAndBeckmannPeakBounds()
        {
            Assert.AreEqual(0.02135, SeaSpectrum.FarVariance(2.8, 9.0), 1e-4, "beat 2 full roughness (DEVELOPMENT.md: the spec's 0.0133 is not this spectrum's)");
            Assert.AreEqual(0.01843, SeaSpectrum.FarVariance(2.0, 8.0), 1e-4, "beat 3");
            Assert.AreEqual(0.01639, SeaSpectrum.FarVariance(3.2, 11.0), 1e-4, "beat 4");
            Assert.LessOrEqual(SeaSpectrum.BeckmannPeak(SeaSpectrum.S2Floor), 39.8 + 0.05, "H1: peak at most 39.8 (the roughness floor)");
            // the near sea at the 2048 m clipmap edge is fully filtered: it reaches the far-field roughness exactly
            var w = Spectrum(0);
            double f2048 = SeaSpectrum.FootprintAt(2048.0, 9.7, 2160, 64.0);
            Assert.AreEqual(SeaSpectrum.FarVariance(2.8, 9.0), SeaSpectrum.FilteredS2(w, SeaSpectrum.N, f2048), 1e-9, "T7: same roughness on both tiers");
            Assert.AreEqual(0.0, SeaSpectrum.ResolvedSlope(w, SeaSpectrum.N, f2048), 1e-12);
        }

        [Test]
        public void Footprint_SpecNumbersAndFiltering()
        {
            Assert.AreEqual(0.6, SeaSpectrum.FootprintAt(100, 9.7, 2160, 64), 0.01, "H1: 0.6 m at 100 m (4K)");
            Assert.AreEqual(60.0, SeaSpectrum.FootprintAt(1000, 9.7, 2160, 64), 0.5, "60 m at 1 km");
            Assert.AreEqual(537.0, SeaSpectrum.FootprintAt(3000, 9.7, 2160, 64), 1.0, "537 m at 3 km");
            var w = Spectrum(0);
            double f30 = SeaSpectrum.FootprintAt(30, 9.7, 2160, 64), f300 = SeaSpectrum.FootprintAt(300, 9.7, 2160, 64), f1k = SeaSpectrum.FootprintAt(1000, 9.7, 2160, 64);
            double r30 = SeaSpectrum.ResolvedSlope(w, SeaSpectrum.N, f30), r300 = SeaSpectrum.ResolvedSlope(w, SeaSpectrum.N, f300), r1k = SeaSpectrum.ResolvedSlope(w, SeaSpectrum.N, f1k);
            Debug.Log($"[BW] resolved slope: 30 m {r30:F4}, 300 m {r300:F4}, 1 km {r1k:F4}; s2 {SeaSpectrum.FilteredS2(w, 32, f30):F5} / {SeaSpectrum.FilteredS2(w, 32, f300):F5} / {SeaSpectrum.FilteredS2(w, 32, f1k):F5}");
            Assert.Greater(r30, r300);
            Assert.Greater(r300, r1k);
            // X2 S3-2 (pixel weight 3 f / lambda, was 4): at 1 km only the 210 m swell keeps a trace (w 0.2 at 4K): a 3.5 px
            // wave, below 0.003 of slope; the 173 m component and everything shorter is filtered: still no sparkle
            Assert.Less(r1k, 0.003, "H1 (rebalanced): resolved slope at 1 km only the longest swell's trace");
            // the filtered sum keeps its total: resolved^2 / 2 + unresolved = the whole variance at every distance
            foreach (double f in new[] { f30, f300, f1k, 5.0, 17.0 })
            {
                double res = SeaSpectrum.ResolvedSlope(w, 32, f);
                double total = SeaSpectrum.FilteredS2(w, 32, f) - SeaSpectrum.S2Floor + 0.5 * res * res;
                Assert.LessOrEqual(total, SeaSpectrum.SlopeVariance(w, 32) + 1e-12);
            }
            // ring weights (X2 S3-2): a ring resolves wavelengths above 5.9 spacings, none below 3.3
            Assert.AreEqual(1.0, SeaSpectrum.RingWeight(0.5, 3.0), 1e-12);
            Assert.AreEqual(0.0, SeaSpectrum.RingWeight(0.5, 1.6), 1e-12);
            Assert.Greater(SeaSpectrum.RingWeight(32.0, 142.0), 0.5, "the 142 m swell stays in the 32 m ring (crest silhouettes to 2 km)");
            Assert.Greater(SeaSpectrum.FootprintWeight(1.0, 6.0), 0.999, "whole above 6 px per wavelength");
            Assert.AreEqual(0.0, SeaSpectrum.FootprintWeight(1.0, 3.0), 1e-12, "zero at 3 px per wavelength");
        }

        // ============================================================================ the surface (C# twin)
        [Test]
        public void Surface_MeasuredHsAndMeanLevelOver60s()
        {
            // 14.5.2 (1): mean sea y -8.0 +-0.05 over 60 s, Eulerian (area-weighted by the Jacobian of the Gerstner map), over
            // 60 s at 1 Hz on a patch 512 m long in z (the 210-315 m swell needs the length to average out): in the open sea
            // (x 200..704 at 8 m) and in the clapotis zone at the wall (x 6.5..132.5 at 2 m), for each beat's sea state
            var w = Spectrum(0);
            foreach (var patch in new[] { (x0: 200.0, dx: 8.0), (x0: 6.5, dx: 2.0) })
                for (int set = 0; set < 3; set++)
                {
                    var sp = Spectrum(set);
                    double sumY = 0, sumJ = 0, sumY2 = 0; long n = 0;
                    for (int ti = 0; ti < 60; ti++)
                    {
                        double t = 1000.0 + ti;
                        for (int ix = 0; ix < 64; ix++)
                            for (int iz = 0; iz < 64; iz++)
                            {
                                double x = patch.x0 + patch.dx * ix, z = 200.0 + 8.0 * iz;
                                double img = SeaSpectrum.ImageWeight(x);
                                double dx = 0, dy = 0, dz = 0, jxx = 1, jzz = 1, jxz = 0;
                                SeaSpectrum.Surface(sp, SeaSpectrum.N, 1.0, x, z, t, SeaSpectrum.Steepness, img, ref dx, ref dy, ref dz, ref jxx, ref jzz, ref jxz);
                                double j = jxx * jzz - jxz * jxz;
                                sumY += dy * j; sumJ += j; sumY2 += dy * dy; n++;
                            }
                    }
                    double mean = BulwarkConst.SeaY + sumY / sumJ;
                    Debug.Log($"[BW] sea set {set} patch x0 {patch.x0}: Eulerian mean y {mean:F4} over 60 s, rms {Math.Sqrt(sumY2 / n):F3}");
                    Assert.AreEqual(-8.0, mean, 0.05, "mean sea level over 60 s (set " + set + ", x0 " + patch.x0 + ")");
                }
            // Hs from the surface far from the wall (random points, one instant): 4 std = Hs within 3 %
            var rng = new System.Random(7);
            double s1 = 0, s2 = 0; int m = 20000;
            for (int i = 0; i < m; i++)
            {
                double dx = 0, dy = 0, dz = 0, jxx = 1, jzz = 1, jxz = 0;
                SeaSpectrum.Surface(w, SeaSpectrum.N, 1.0, 1000.0 + 4000.0 * rng.NextDouble(), 4000.0 * rng.NextDouble(), 3600.0 * rng.NextDouble(), SeaSpectrum.Steepness, 0.0, ref dx, ref dy, ref dz, ref jxx, ref jzz, ref jxz);
                dy -= SeaSpectrum.MeanLevelDrop(w, SeaSpectrum.N, SeaSpectrum.Steepness);
                s1 += dy; s2 += dy * dy;
            }
            double std = Math.Sqrt(s2 / m - (s1 / m) * (s1 / m));
            Assert.AreEqual(2.8, 4.0 * std, 0.084, "Hs measured on the surface (3 %)");
        }

        [Test]
        public void Surface_WallClapotisDoublesAndKeepsTheWallLine()
        {
            var w = Spectrum(0);
            for (int i = 0; i < 50; i++)
            {
                double t = 17.3 * i, z = 133.0 + 7.1 * i;
                double dx = 0, dy = 0, dz = 0, jxx = 1, jzz = 1, jxz = 0;
                SeaSpectrum.Surface(w, SeaSpectrum.N, 1.0, SeaSpectrum.WallX, z, t, SeaSpectrum.Steepness, 1.0, ref dx, ref dy, ref dz, ref jxx, ref jzz, ref jxz);
                double ix = 0, iy = 0, iz = 0, a = 1, b = 1, c = 0;
                SeaSpectrum.Surface(w, SeaSpectrum.N, 1.0, SeaSpectrum.WallX, z, t, SeaSpectrum.Steepness, 0.0, ref ix, ref iy, ref iz, ref a, ref b, ref c);
                Assert.AreEqual(0.0, dx, 1e-9, "the image wave cancels the horizontal displacement through the wall (x 6)");
                double drop0 = SeaSpectrum.MeanLevelDrop(w, 32, SeaSpectrum.Steepness);
                Assert.AreEqual(2.0 * (iy - drop0), dy - 2.0 * drop0, 1e-9, "the height doubles at the wall (standing wave)");
            }
            Assert.AreEqual(1.0, SeaSpectrum.ImageWeight(150.0 + 6.0), 1e-12);
            Assert.AreEqual(0.0, SeaSpectrum.ImageWeight(400.0 + 6.0), 1e-12);
        }

        [Test]
        public void Precision_CameraRelativePhaseAtZ1500AfterAnHour()
        {
            // the shader adds float(k d . (x - O)) to the double-computed Phi (mod 2 pi, then float): compare with the exact
            // double phase at z 1500 after 3600.5 s of ambient clock, for the shortest and the longest component
            var w = Spectrum(0);
            foreach (int i in new[] { 0, 15, SeaSpectrum.N - 1 })
            {
                SeaWave c = w[i];
                double t = 3600.5;
                double ox = 64.0 * Math.Round(4.2 / 64.0), oz = 64.0 * Math.Round(1500.0 / 64.0);
                double phi = c.K * (c.DirX * ox + c.DirZ * oz) - c.Omega * t + c.Phase0;
                phi -= 2.0 * Math.PI * Math.Floor(phi / (2.0 * Math.PI));
                float lx = (float)(37.25 - ox), lz = (float)(1512.5 - oz);
                float thetaF = (float)(c.K * c.DirX) * lx + (float)(c.K * c.DirZ) * lz + (float)phi;
                double thetaD = c.K * (c.DirX * 37.25 + c.DirZ * 1512.5) - c.Omega * t + c.Phase0;
                double err = Math.Abs(Math.IEEERemainder(thetaF - thetaD, 2.0 * Math.PI));
                Assert.Less(err, 2e-3, "camera-relative float phase error (rad) for component " + i);
            }
        }

        // ============================================================================ the clipmap
        [Test]
        public void Clipmap_RingsAreWatertightForEveryParity()
        {
            for (int ox = 0; ox <= 1; ox++)
                for (int oz = 0; oz <= 1; oz++)
                    Assert.AreEqual(0, SeaClipmap.CoverageErrors(ox, oz), $"cells covered 0 or 2 times (offset {ox},{oz})");
            Assert.AreEqual(32.0, SeaClipmap.HalfExtent(0), 1e-12);
            Assert.AreEqual(2048.0, SeaClipmap.HalfExtent(SeaClipmap.Levels - 1), 1e-12, "14.5.2: half-extent 2048 m");
            Assert.AreEqual(32.0, SeaClipmap.Spacing(6), 1e-12);
            Mesh m = SeaClipmap.Build(out int tris);
            try
            {
                Debug.Log($"[BW] sea clipmap: {m.vertexCount} vertices, {tris} triangles");
                Assert.AreEqual(32768 + (6 + SeaClipmap.DisturbLevels) * (2 * (128 * 128 - 65 * 65) + 2 * (4 * 63 + 4)), tris,
                                "level 0 full, 6 wave rings and the disturbance ring with their trims");
                Assert.Less(tris, 230000, "within the 14.5.2 budget (229 k)");
                Assert.AreEqual(4096.0, SeaClipmap.HalfExtent(SeaClipmap.Levels), 1e-12, "the disturbance ring reaches 4096 m");
            }
            finally { UnityEngine.Object.DestroyImmediate(m); }
        }

        // ============================================================================ impacts (14.3, 14.5.2)
        [Test]
        public void Impacts_MeanIntervalsAndAtLeastThreePerMinute()
        {
            foreach (bool reef in new[] { false, true })
            {
                double T = reef ? SeaState.LatticeReef : SeaState.LatticeCrest;
                double prev = double.NaN, sum = 0, minGap = 1e9, maxGap = 0; int n = 0;
                for (long i = 0; i < 400; i++)
                {
                    SeaState.LatticeImpact(T, reef ? 0x4EEF5u : 0xC0A57u, i, out double t0, out double st, out _, out double ahead, out double off);
                    Assert.That(st, Is.InRange(BulwarkConst.ImpactStrengthMin, BulwarkConst.ImpactStrengthMax), "strength 0.3-1.0");
                    Assert.That(ahead, Is.InRange(SeaState.Ahead0, SeaState.Ahead1));
                    Assert.That(off, Is.InRange(SeaState.Collision0, SeaState.Collision1));
                    if (!double.IsNaN(prev)) { double g = t0 - prev; sum += g; n++; minGap = Math.Min(minGap, g); maxGap = Math.Max(maxGap, g); }
                    prev = t0;
                }
                Debug.Log($"[BW] impacts ({(reef ? "reef" : "crest")}): mean interval {sum / n:F3} s, gaps {minGap:F2}-{maxGap:F2} s");
                Assert.AreEqual(T, sum / n, 0.1, "14.3 mean interval");
                Assert.Less(maxGap, 20.0, "at least 3 impacts in any 60 s");
            }
            Assert.AreEqual(6.0, SeaState.HeightFor(0.3), 1e-9, "14.5.2 plumes 6-14 m");
            Assert.AreEqual(14.0, SeaState.HeightFor(1.0), 1e-9);
            // Rule-P membership: beat 2 only the crest lattice, beat 4 only the reef lattice
            Assert.AreEqual(0.0, SeaState.ReefWeight(300.0), 1e-12);
            Assert.AreEqual(1.0, SeaState.ReefWeight(800.0), 1e-12);
        }

        [Test]
        public void Impacts_AtLeastThreePlumesIn60sAtKS04Framing()
        {
            // 14.5.2 acceptance (7): a static camera at the KS04 pose, any 60 s window of an hour (2 s steps), the plume
            // (wall jet or its collision jet offshore) inside the frame with the 3 % margin at 16:9, 16:10 and 21:9
            var k = BulwarkConst.KeyShots[3];
            foreach (double aspect in new[] { 16.0 / 9.0, 1.6, 21.0 / 9.0 })
            {
                int min = int.MaxValue; double sum = 0; int windows = 0;
                for (double t = 0.0; t < 3600.0; t += 2.0)
                {
                    int c = SeaState.CountPlumesInFrame(t, 60.0, k.s, k.x, k.y, k.z, k.yaw, k.pitch, k.fov, aspect);
                    min = Math.Min(min, c); sum += c; windows++;
                }
                Debug.Log($"[BW] KS04 framing aspect {aspect:F3}: plumes per 60 s min {min}, mean {sum / windows:F2}");
                Assert.GreaterOrEqual(min, 3, "at least 3 plumes in 60 s at KS04 framing (aspect " + aspect + ")");
            }
            // the forced KS04 plume (collision jet 30 m off the wall at z 175, peak 10 m) at the still's age: the whole box of
            // the spray ellipsoid above the road (MistVolumeMath.SprayPose, the VisibilityAudit subject requested from
            // integrate) is inside every aspect with the 3 % margin, and its top is 8-14 m above the crest; a wall jet at
            // z 175 is not inside (the Wave A audit: 73 deg off the view axis)
            SeaState.KS04PlumeBounds(out Vector3 bmin, out Vector3 bmax);
            Debug.Log($"[BW] KS04 forced plume box {bmin} .. {bmax}");
            Assert.That(bmax.y, Is.InRange(8.0f, 14.0f), "14.5.1: a plume 8-14 m high");
            Assert.That(SeaState.KS04PlumeHeight, Is.InRange(8.0, 14.0));
            foreach (double aspect in new[] { 16.0 / 9.0, 1.6, 21.0 / 9.0 })
            {
                foreach (float x in new[] { bmin.x, bmax.x })
                    foreach (float y in new[] { bmin.y, bmax.y })
                        foreach (float z in new[] { bmin.z, bmax.z })
                            Assert.IsTrue(SeaState.InFrame(x, y, z, k.x, k.y, k.z, k.yaw, k.pitch, k.fov, aspect), $"forced plume corner ({x},{y},{z}) at aspect {aspect:F2}");
                Assert.IsFalse(SeaState.InFrame(BulwarkConst.SeaFaceX, 5.0, 175.0, k.x, k.y, k.z, k.yaw, k.pitch, k.fov, aspect), "a wall jet at z 175 is outside");
            }
        }

        [Test]
        public void Impacts_CapturePlumeIsAnchoredToTheShotClockAndKeepsBusIndex0()
        {
            // a static capture sequence at the KS04 pose with the clock running from the still's clock (s / 2.5): the forced
            // plume ages naturally from its capture age (no frozen plume) and stays the first entry on the bus while lattice
            // impacts come and go (a reader keyed to the bus index never sees it jump); the bus never overflows
            var sea = new SeaState();
            var k = BulwarkConst.KeyShots[3];
            double tShot = k.s / BulwarkConst.Cine;
            var f = new FrameState { s = k.s, cameraPos = new Vector3((float)k.x, (float)k.y, (float)k.z), capture = true, beat = 2, fovV = (float)k.fov, renderHeight = 1080f };
            try
            {
                for (int i = 0; i <= 120; i++)
                {
                    f.ambientTime = tShot + i / 30.0;
                    sea.OnFrame(f);
                    Assert.LessOrEqual(WaveImpacts.Count, WaveImpacts.Max, "bus capacity");
                    double age = SeaState.CapturePlumeAge + i / 30.0;
                    if (age > WaveImpacts.Lifetime) continue;
                    Assert.Greater(WaveImpacts.Count, 0);
                    WaveImpact w0 = WaveImpacts.Get(0);
                    Assert.AreEqual(BulwarkConst.SeaFaceX + SeaState.KS04PlumeOffshore, w0.X, 1e-4, "frame " + i + ": the KS04 plume is bus entry 0");
                    Assert.AreEqual(175.0, w0.Z, 1e-4);
                    Assert.AreEqual(tShot - SeaState.CapturePlumeAge, w0.T0, 1e-9, "anchored to the shot clock");
                }
            }
            finally { WaveImpacts.Clear(); }
        }

        // ============================================================================ the kraken's disturbances (Addendum A1)
        static SeaDisturbance Dist(SeaDisturbanceKind k, double along, double across, double headingDeg = 0.0, double s = 1.0)
            => new SeaDisturbance { Kind = k, X = 500f, Z = 800f, HalfAlong = (float)along, HalfAcross = (float)across, Heading = (float)(headingDeg * Math.PI / 180.0), Strength = (float)s };

        static SeaDisturbanceKind[] Kinds => new[] { SeaDisturbanceKind.Shadow, SeaDisturbanceKind.Breach, SeaDisturbanceKind.TentacleRise, SeaDisturbanceKind.Slap, SeaDisturbanceKind.Wake };

        [Test]
        public void Disturb_EveryKindFadesOutInsideItsSupport()
        {
            // the shader skips an entry beyond its support radius and fades it over the last 20 %: the field must already be
            // faint where that fade starts (no visible cut) and exactly 0 beyond (the loop's early-out is exact)
            foreach (var kind in Kinds)
                foreach (var size in new[] { (2.0, 2.0), (6.0, 4.0), (28.0, 20.0), (42.0, 20.0), (60.0, 25.0) })
                {
                    var items = new[] { Dist(kind, size.Item1, size.Item2, 37.0) };
                    double sup = SeaDisturbField.Support(kind, size.Item1, size.Item2);
                    double worst = 0.0, worstH = 0.0;
                    for (int a = 0; a < 72; a++)
                    {
                        double ang = a * 5.0 * Math.PI / 180.0;
                        foreach (double clock in new[] { 0.0, 1.3, 377.7 })
                        {
                            var s = SeaDisturbField.At(500.0 + 0.8 * sup * Math.Sin(ang), 800.0 + 0.8 * sup * Math.Cos(ang), items, 1, clock, 0.0);
                            worst = Math.Max(worst, Math.Max(Math.Max(s.Foam, s.White), Math.Max(s.Dark, s.Calm)));
                            worstH = Math.Max(worstH, Math.Abs(s.Height));
                            var o = SeaDisturbField.At(500.0 + 1.0001 * sup * Math.Sin(ang), 800.0 + 1.0001 * sup * Math.Cos(ang), items, 1, clock, 0.0);
                            Assert.AreEqual(0.0, o.Foam + o.White + o.Dark + o.Calm + Math.Abs(o.Height), 0.0, $"{kind} beyond its support");
                        }
                    }
                    Assert.Less(worst, 0.05, $"{kind} {size}: coverage at 0.8 x support {sup:F0} m");
                    Assert.Less(worstH, 0.05, $"{kind} {size}: height at 0.8 x support");
                }
        }

        [Test]
        public void Disturb_ShadowIsADarkSlickMassWithoutFoam()
        {
            var items = new[] { Dist(SeaDisturbanceKind.Shadow, 42.0, 20.0, 15.0) };
            double fwx = Math.Sin(15.0 * Math.PI / 180.0), fwz = Math.Cos(15.0 * Math.PI / 180.0);
            var c = SeaDisturbField.At(500.0 + 0.25 * 42.0 * fwx, 800.0 + 0.25 * 42.0 * fwz, items, 1, 10.0, 0.0);
            Debug.Log($"[BW] Shadow centre: dark {c.Dark:F3} calm {c.Calm:F3} swell {c.Height:F3} m");
            Assert.Greater(c.Dark, 0.8, "the mass swallows the light");
            Assert.Greater(c.Calm, 0.8, "a slick over it");
            Assert.That(c.Height, Is.InRange(0.2, 0.81), "a slight swell (0.03 B, 0.2-0.8 m)");
            double tendrilDark = 0.0;
            for (double x = 300.0; x <= 700.0; x += 2.0)
                for (double z = 600.0; z <= 1000.0; z += 2.0)
                {
                    var s = SeaDisturbField.At(x, z, items, 1, 10.0, 0.0);
                    Assert.AreEqual(0.0, s.Foam + s.White, 0.0, "no foam over a Shadow");
                    // behind the mantle (1.5 A back along the heading): the tendrils
                    double u = (x - 500.0) * fwx + (z - 800.0) * fwz;
                    if (u < -1.5 * 42.0) tendrilDark = Math.Max(tendrilDark, s.Dark);
                }
            Assert.Greater(tendrilDark, 0.3, "tendrils trail behind the mantle");
        }

        [Test]
        public void Disturb_BreachIsWhiteWaterWithAFoamRingAndAMound()
        {
            var items = new[] { Dist(SeaDisturbanceKind.Breach, 28.0, 20.0) };
            var c = SeaDisturbField.At(500.0, 800.0, items, 1, 5.0, 0.0);
            Assert.Greater(c.White, 0.99, "white water over the body");
            Assert.That(c.Height, Is.InRange(1.5, 7.0 + 0.7), "the mound 0.15 sqrt(A B), 1.5-7 m");
            double bm = Math.Sqrt(28.0 * 20.0);
            // the waterline foam ring (ragged by +-0.31 Bm): along every 30 deg ray, foam above 0.5 between 0.1 and 1.0 Bm out
            for (int a = 0; a < 12; a++)
            {
                double ang = a * 30.0 * Math.PI / 180.0, best = 0.0;
                for (double rho = 1.0 + 0.1 * bm / bm; rho <= 2.0; rho += 0.01)
                {
                    double x = 500.0 + rho * 20.0 * Math.Sin(ang), z = 800.0 + rho * 28.0 * Math.Cos(ang);   // heading 0: B along x, A along z
                    best = Math.Max(best, SeaDisturbField.At(x, z, items, 1, 5.0, 0.0).Foam);
                }
                Assert.Greater(best, 0.5, "the waterline foam ring at " + (a * 30) + " deg");
            }
        }

        [Test]
        public void Disturb_SlapRingRunsOutAsTheEnvelopeDecays()
        {
            double prev = 0.0;
            foreach (double s in new[] { 1.0, 0.75, 0.5, 0.25 })
            {
                var items = new[] { Dist(SeaDisturbanceKind.Slap, 3.5, 3.5, 0.0, s) };
                double rr = SeaDisturbField.SlapRingRadius(3.5, s), best = -1.0, bestR = 0.0;
                for (double r = 0.6 * rr; r <= 1.6 * rr + 3.0; r += 0.02)
                {
                    var f = SeaDisturbField.At(500.0 + r, 800.0, items, 1, 0.0, 0.0);
                    if (f.Foam > best + 1e-9) { best = f.Foam; bestR = r; }
                }
                double sr = 0.18 * 3.5 + 0.8 + 0.1 * (rr - 3.5);
                Debug.Log($"[BW] Slap S {s:F2}: ring radius {rr:F2} m (foam peak at {bestR:F2}, {best:F2})");
                Assert.AreEqual(rr, bestR, 0.5 * sr, "the foam ring sits at A (1 + 4 (1 - S))");
                Assert.Greater(rr, prev, "the ring runs outward as S decays");
                prev = rr;
            }
            Assert.AreEqual(3.5, SeaDisturbField.SlapRingRadius(3.5, 1.0), 1e-12);
            Assert.AreEqual(17.5, SeaDisturbField.SlapRingRadius(3.5, 0.0), 1e-12);
        }

        [Test]
        public void Disturb_WakeArmsFollowTheKelvinAngle()
        {
            // heading 0 (+z): the wake lies toward -z; at 100 m behind the stern the arm crest is at |x| = 0.8 B + tan(19.47) 100
            var items = new[] { Dist(SeaDisturbanceKind.Wake, 25.0, 9.0, 0.0) };
            double bh = 100.0, z = 800.0 - 0.6 * 25.0 - bh, best = -1.0, bestX = 0.0;
            for (double x = 15.0; x <= 80.0; x += 0.05)
            {
                var f = SeaDisturbField.At(500.0 + x, z, items, 1, 0.0, 0.0);
                if (f.Foam > best) { best = f.Foam; bestX = x; }
            }
            double ang = Math.Atan((bestX - 0.8 * 9.0) / bh) * 180.0 / Math.PI;
            Debug.Log($"[BW] Wake arm at {bh} m behind: |x| {bestX:F2} m, angle {ang:F2} deg, foam {best:F3}");
            Assert.AreEqual(19.47, ang, 1.0, "Kelvin half-angle");
            Assert.That(best, Is.InRange(0.05, 0.75), "foam lines, fading behind"); 
            // nothing ahead of the bow but the bow wave
            var ahead = SeaDisturbField.At(500.0, 800.0 + 3.0 * 25.0, items, 1, 0.0, 0.0);
            Assert.Less(ahead.Foam, 0.02);
        }

        [Test]
        public void Disturb_FootprintFilterKeepsTheEnergyAndNeverBrightens()
        {
            foreach (double sigma in new[] { 0.5, 2.0, 8.0 })
                foreach (double fq in new[] { 0.0, 1.0, 5.0, 50.0, 500.0 })
                {
                    double sum = 0.0, max = 0.0, dx = Math.Max(sigma, fq) / 200.0;
                    for (double x = -12.0 * Math.Max(sigma, fq); x <= 12.0 * Math.Max(sigma, fq); x += dx)
                    {
                        double g = SeaDisturbField.GaussF(x, sigma, fq);
                        sum += g * dx; max = Math.Max(max, g);
                    }
                    Assert.AreEqual(sigma * Math.Sqrt(2.0 * Math.PI), sum, 0.01 * sigma * Math.Sqrt(2.0 * Math.PI), $"energy sigma {sigma} fq {fq}");
                    Assert.LessOrEqual(max, 1.0 + 1e-12, "a filtered line never gets brighter");
                }
            // a ring train far away converges to its mean coverage (steady, no flicker as the crests travel)
            double mean = 1.2 * Math.Sqrt(2.0 * Math.PI) / (SeaDisturbField.BreachRingSpeed * SeaDisturbField.BreachRingPeriod);
            foreach (double clock in new[] { 0.0, 0.7, 1.9 })
                Assert.AreEqual(mean, SeaDisturbField.RingTrain(33.3, SeaDisturbField.BreachRingSpeed, SeaDisturbField.BreachRingPeriod, 1.2, 60.0, clock), 1e-6);
        }

        [Test]
        public void Disturb_ClockWrapsWithoutAJump()
        {
            foreach (double p in new[] { SeaDisturbField.BreachRingPeriod, SeaDisturbField.TentacleRingPeriod, SeaDisturbField.TendrilPeriod, SeaDisturbField.MantlePeriod, SeaDisturbField.WaterlinePeriod, 6.0 })
                Assert.AreEqual(0.0, Math.IEEERemainder(SeaDisturbField.ClockPeriod, p), 1e-9, "period " + p + " divides the 600 s wrap");
            Assert.AreEqual(0.5, SeaDisturbField.Clock(600.5), 1e-9);
            Assert.AreEqual(17.25, SeaDisturbField.Clock(3617.25), 1e-9);
            var items = new[] { Dist(SeaDisturbanceKind.Breach, 28.0, 20.0), Dist(SeaDisturbanceKind.Shadow, 42.0, 20.0, 90.0), Dist(SeaDisturbanceKind.TentacleRise, 2.0, 2.0) };
            items[1].X = 700f; items[2].X = 350f;
            for (int i = 0; i < 200; i++)
            {
                double x = 300.0 + 2.1 * i, z = 760.0 + 0.4 * i;
                var a = SeaDisturbField.At(x, z, items, 3, SeaDisturbField.ClockPeriod - 1e-7, 0.0);
                var b = SeaDisturbField.At(x, z, items, 3, 0.0, 0.0);
                Assert.AreEqual(a.Foam, b.Foam, 1e-4); Assert.AreEqual(a.Dark, b.Dark, 1e-4); Assert.AreEqual(a.Height, b.Height, 1e-4);
            }
        }

        [Test]
        public void Disturb_QaWriterPutsEveryKindInFrame()
        {
            // the QA writer's "near" scenario: the five KS04 entries inside the KS04 frame (16:9, 16:10, 21:9; 3 % margin) and
            // above the parapet's shadow line, the three KS06 entries inside KS06 and beyond the parapet (x >= 120)
            var qa = new SeaDisturbanceQa("near");
            Assert.AreEqual(8, qa.Count);
            var kinds = new System.Collections.Generic.HashSet<SeaDisturbanceKind>();
            var k4 = BulwarkConst.KeyShots[3];
            var k6 = BulwarkConst.KeyShots[5];
            for (int i = 0; i < qa.Count; i++)
            {
                var d = qa.Item(i);
                kinds.Add(d.Kind);
                var k = i < 5 ? k4 : k6;
                foreach (double aspect in new[] { 16.0 / 9.0, 1.6, 21.0 / 9.0 })
                    Assert.IsTrue(SeaState.InFrame(d.X, BulwarkConst.SeaY, d.Z, k.x, k.y, k.z, k.yaw, k.pitch, k.fov, aspect), $"QA entry {i} ({d.Kind}) in frame at {aspect:F2}");
                // the ray from the eye to the entry passes above the parapet's outer top edge (x 6, y 1.1)
                double f = (BulwarkConst.SeaFaceX - k.x) / (d.X - k.x);
                Assert.Greater(k.y + (BulwarkConst.SeaY - k.y) * f, BulwarkConst.ParapetTop, $"QA entry {i} visible over the parapet");
            }
            Assert.AreEqual(5, kinds.Count, "every kind");
            var far = new SeaDisturbanceQa("far");
            Assert.AreEqual(8, far.Count);
            // Addendum A1 1.6 sizes at 800 m and 2 km from the KS04 eye
            for (int i = 0; i < far.Count; i++)
            {
                var d = far.Item(i);
                double r = Math.Sqrt((d.X - k4.x) * (d.X - k4.x) + (d.Z - k4.z) * (d.Z - k4.z));
                Assert.AreEqual(i < 4 ? 800.0 : 2000.0, r, 1e-3, $"far entry {i} distance");
                if (d.Kind == SeaDisturbanceKind.Breach) Assert.AreEqual(SeaDisturbanceQa.K1Along, d.HalfAlong, 1e-6);
                if (d.Kind == SeaDisturbanceKind.Shadow) Assert.AreEqual(SeaDisturbanceQa.K3Along, d.HalfAlong, 1e-6);
            }
            var kr = new SeaDisturbanceQa("kraken");
            Assert.AreEqual(8, kr.Count);
            double k1 = Math.Sqrt((kr.Item(0).X - k4.x) * (kr.Item(0).X - k4.x) + (kr.Item(0).Z - k4.z) * (kr.Item(0).Z - k4.z));
            Assert.AreEqual(1464.0, k1, 2.0, "K1 1464 m from KS04 (A1 1.6)");
            Assert.AreEqual(0.0, SeaDisturbanceQa.SlapEnvelope(0.0), 1e-12);
            Assert.AreEqual(1.0, SeaDisturbanceQa.SlapEnvelope(0.12), 1e-9);
            Assert.AreEqual(0.0, SeaDisturbanceQa.SlapEnvelope(3.62), 1e-9);
        }

        [Test]
        public void SetWeights_RuleP()
        {
            SeaState.SetWeights(300.0, out double a, out double b, out double c);
            Assert.AreEqual(1.0, a, 1e-12); Assert.AreEqual(0.0, b + c, 1e-12);
            SeaState.SetWeights(420.0, out a, out b, out c);
            Assert.AreEqual(0.5, a, 1e-9); Assert.AreEqual(0.5, b, 1e-9);
            SeaState.SetWeights(600.0, out a, out b, out c);
            Assert.AreEqual(1.0, b, 1e-12);
            SeaState.SetWeights(800.0, out a, out b, out c);
            Assert.AreEqual(1.0, c, 1e-12);
            // continuity over the windows (14.6 sweep, 0.1 m steps): the energy weights change smoothly
            double pa = 1, pb = 0, pc = 0;
            for (double s = 0; s <= 1500; s += 0.1)
            {
                SeaState.SetWeights(s, out a, out b, out c);
                Assert.AreEqual(1.0, a + b + c, 1e-9);
                Assert.Less(Math.Abs(a - pa) + Math.Abs(b - pb) + Math.Abs(c - pc), 0.02, "weights continuous at s " + s);
                pa = a; pb = b; pc = c;
            }
        }
    }
}
