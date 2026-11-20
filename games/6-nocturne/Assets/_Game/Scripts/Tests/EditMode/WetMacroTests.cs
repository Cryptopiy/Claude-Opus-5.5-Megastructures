// WetMacroTests: the macro radiance of the wet mirror's wall planes (TEXTURE_PLAN 2.11, 8.3 SU; owner SU, then FX). Every
// wall plane a beat registers carries its C# mean at the midpoint of its s range within SurfMacroTol (5 %, luminance and each
// channel): the alley walls = NocSurfaceMath.AlleyWallMacro (WallRadiance of the ConcreteDark walls under PA's rig + the lit
// windows), the boulevard canyon = FA's facade mean of the default region, the Gulf faces and the garden notch = FA's section
// mean. TP-2 (FX, Phase 4 wave A) deleted -surf-v and the v1 literals' test.
// WallRadiance follows PA's rig: the shade side sees the trilight equator only, the fill side adds the sky fill x N.L, and the
// wet darkening is Lagarde's lerp(1, 0.2, porosity x wet).
using System;
using NUnit.Framework;
using UnityEngine;

namespace Megastructure.Tests
{
    public class WetMacroTests
    {
        static void WithCleanRegistry(Action body)
        {
            NocWetRegistry.Clear();
            try { body(); }
            finally { NocWetRegistry.Clear(); }
        }

        static void AssertClose(Color want, Color got, string what)
        {
            double tol = NocSurfaceConstants.SurfMacroTol;
            double lw = NocMath.Luma709(want.r, want.g, want.b), lg = NocMath.Luma709(got.r, got.g, got.b);
            Assert.Greater(lw, 1e-5, what + ": the reference is not black");
            Assert.AreEqual(lw, lg, tol * lw, FormattableString.Invariant($"{what}: luminance {lg:E3} vs {lw:E3}"));
            Assert.AreEqual(want.r, got.r, tol * Math.Max(want.r, 1e-6) + 1e-7, what + " r");
            Assert.AreEqual(want.g, got.g, tol * Math.Max(want.g, 1e-6) + 1e-7, what + " g");
            Assert.AreEqual(want.b, got.b, tol * Math.Max(want.b, 1e-6) + 1e-7, what + " b");
        }

        /// <summary>Every registered wall against reference(nx, nz, s_mid).</summary>
        static int CheckWalls(Func<double, double, double, Color> reference, string what)
        {
            int n = NocWetRegistry.RegisteredWalls;
            for (int i = 0; i < n; i++)
            {
                NocWetRegistry.GetWall(i, out Vector4 a, out Vector4 b, out Color macro, out double s0, out double s1);
                double sm = 0.5 * (s0 + s1);
                AssertClose(reference(a.z, a.w, sm), macro, FormattableString.Invariant($"{what} wall {i} (s {s0:F0}-{s1:F0})"));
                Debug.Log(FormattableString.Invariant($"[NOC] WetMacro {what} wall {i}: s {s0:F0}-{s1:F0}, n ({a.z:F2}, {a.w:F2}), macro ({macro.r:E3}, {macro.g:E3}, {macro.b:E3})"));
            }
            return n;
        }

        [Test]
        public void V2_EveryRegisteredMacro_Within5PercentOfItsMean()
        {
            WithCleanRegistry(() =>
            {
                Assert.AreEqual(2, NocLookGlobals.SurfV, "TP-2: Look v2 is the only surface path");
                Beat01_Alley.RegisterWet();
                Assert.AreEqual(2, CheckWalls((nx, nz, s) => NocSurfaceMath.AlleyWallMacro(s, nx), "alley"));
                NocWetRegistry.Clear();
                Beat02_Boulevard.RegisterWet();
                Assert.AreEqual(2, CheckWalls((nx, nz, s) => NocSurfaceMath.FacadeMacro(0, NocFacadeMath.RegionD, s), "boulevard"));
                NocWetRegistry.Clear();
                Beat03_Edge.RegisterWalls(null);
                Assert.AreEqual(4, CheckWalls((nx, nz, s) => NocSurfaceMath.FacadeMacro(1, NocFacadeMath.RegionD, s), "Gulf"));
                NocWetRegistry.Clear();
                Beat05_Garden.RegisterWalls();
                Assert.AreEqual(2, CheckWalls((nx, nz, s) => NocSurfaceMath.FacadeMacro(1, NocFacadeMath.RegionD, s), "garden notch"));
            });
        }

        [Test]
        public void WallRadiance_FollowsTheRig()
        {
            const double s = 260.0;
            AtmosphereDriver.AmbientAt(s, out Color sky, out Color eq, out Color gr);
            Color a = NocturnePalette.Linear(NocPal.ConcreteDark);
            double wet = NocSurfaceMath.WetDarkening(NocSurfaceConstants.SurfPorConcrete, NocSurfaceConstants.SurfWetVertical);
            Assert.AreEqual(1.0 - 0.8 * 0.4 * 0.6, wet, 1e-12, "Lagarde: lerp(1, 0.2, porosity x wet)");
            // the fill comes from above, from -x -z (pitch 60, yaw 35): a wall facing +x is in shade, so it sees the equator only
            D3 L = NocSurfaceMath.FillDirection();
            Assert.Less(L.x, 0.0); Assert.Less(L.z, 0.0); Assert.Greater(L.y, 0.8);
            Color shade = NocSurfaceMath.WallRadiance(NocPal.ConcreteDark, s, 1.0, 0.0);
            double k = NocSurfaceConstants.SurfLambert * wet;
            Assert.AreEqual(a.r * k * eq.r, shade.r, 1e-6 * Math.Max(shade.r, 1e-3));
            Assert.AreEqual(a.g * k * eq.g, shade.g, 1e-6 * Math.Max(shade.g, 1e-3));
            Assert.AreEqual(a.b * k * eq.b, shade.b, 1e-6 * Math.Max(shade.b, 1e-3));
            // the fill side (facing -z, N.L = 0.41) is brighter by the fill term; the unspecified normal is the shade side
            Color lit = NocSurfaceMath.WallRadiance(NocPal.ConcreteDark, s, 0.0, -1.0);
            Assert.Greater(NocMath.Luma709(lit.r, lit.g, lit.b), NocMath.Luma709(shade.r, shade.g, shade.b));
            Assert.AreEqual(0.41, -L.z, 0.01, "LOOK_V2 5.3: the north-facing wall's N.L");
            Color none = NocSurfaceMath.WallRadiance(NocPal.ConcreteDark, s);
            Assert.AreEqual(shade.g, none.g, 1e-9);
            // more porous brick darkens more; the windows add to the alley wall
            Color brick = NocSurfaceMath.WallRadiance(NocPal.ConcreteDark, s, 1.0, 0.0, 1.0, NocSurfaceConstants.SurfPorBrick);
            Assert.Less(brick.g, shade.g);
            Color alley = NocSurfaceMath.AlleyWallMacro(60.0, 1.0), alleyWall = NocSurfaceMath.WallRadiance(NocPal.ConcreteDark, 60.0, 1.0, 0.0);
            Assert.Greater(alley.r, alleyWall.r);
            foreach (var c in new[] { shade, lit, brick, alley })
                Assert.IsFalse(float.IsNaN(c.r) || float.IsNaN(c.g) || float.IsNaN(c.b) || c.r < 0f || c.g < 0f || c.b < 0f);
        }
    }
}
