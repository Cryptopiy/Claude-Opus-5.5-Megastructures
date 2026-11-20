// MaterialTests.Look: the per-row expectations of the light, rain, sky and post rows (owner PA; TEXTURE_PLAN 8.3, PA-2). Created
// empty by TP-0 and handed over to PA. Look v2 needs no new value in these rows (LOOK_V2 10 lists none): the colours live in the
// shaders (PAL_* tokens, the R6 / R7 weights) and in NocturneConstants; the rows carry the gains, kinds and sizes.
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using Megastructure.EditorTools;

namespace Megastructure.Tests
{
    public partial class MaterialTests
    {
        static Dictionary<string, MatSpec> LookSpecs()
        {
            var rows = new Dictionary<string, MatSpec>();
            foreach (var s in MaterialFactory.Specs()) rows[s.Field] = s;
            return rows;
        }

        [Test]
        public void LookRows_GainsKindsAndShaders()
        {
            var rows = LookSpecs();
            foreach (var f in new[] { "Neon", "StreakTail", "StreakHead", "Beacon", "Rain", "Sky", "Grade" })
                Assert.IsTrue(rows.ContainsKey(f), "Look row " + f);
            var neon = rows["Neon"];
            Assert.AreEqual(MaterialFactory.ShaderNeon, neon.Shader);
            Assert.AreEqual(NocturneConstants.NeonGain, neon.Floats["_Gain"], 1e-6, "signs x 1.6 (LOOK_V2 2.3)");
            Assert.AreEqual(1.6, NocturneConstants.NeonGain, 1e-12);
            Assert.AreEqual(-1f, neon.Floats["_Palette"], "hash colours (the R6 / R7 weights) by default");
            Assert.AreEqual(MaterialFactory.ShaderStreak, rows["StreakTail"].Shader);
            Assert.AreEqual(0f, rows["StreakTail"].Floats["_Kind"]);
            Assert.AreEqual(1f, rows["StreakHead"].Floats["_Kind"]);
            Assert.AreEqual(2f, rows["Beacon"].Floats["_Kind"], "beacons; NocHalos derives kind 3 from this row at runtime");
            Assert.AreEqual(MaterialFactory.ShaderRain, rows["Rain"].Shader);
        }

        [Test]
        public void LookShaders_V2ColourRules()
        {
            string dir = Path.Combine(Application.dataPath, "_Game/Shaders");
            string neon = File.ReadAllText(Path.Combine(dir, "NocNeon.shader"));
            string streak = File.ReadAllText(Path.Combine(dir, "NocStreak.shader"));
            // NocScreen's emission lives in Beats/NocScreenWave.hlsl since SG's Phase 4 rework (INT-02): check both files
            string screen = File.ReadAllText(Path.Combine(dir, "Beats/NocScreen.shader"));
            string wave = Path.Combine(dir, "Beats/NocScreenWave.hlsl");
            if (File.Exists(wave)) screen += "\n" + File.ReadAllText(wave);
            // NocNeon: the panel face is capped at 0.015 x the colour desaturated 60 % whatever the row says; no violet in the code
            StringAssert.Contains("min(_Panel, NC_NeonPanel)", neon);
            StringAssert.Contains("lerp(colour, PAL_WinNeutral, NC_NeonPanelDesat)", neon);
            StringAssert.DoesNotContain("PAL_NeonViolet", neon.Substring(neon.IndexOf("HLSLINCLUDE", System.StringComparison.Ordinal)));
            // NocStreak: tails toward amber x k_a, heads toward WinNeutral (no magenta, no cyan), halos as kind 3
            StringAssert.Contains("lerp(PAL_AviationRed, PAL_SodiumAmber, 0.25 * h * _NocAmber.x)", streak);
            StringAssert.Contains("lerp(PAL_SignWhite, PAL_WinNeutral, 0.6 * h)", streak);
            StringAssert.DoesNotContain("PAL_NeonMagenta", streak);
            StringAssert.Contains("float4 _Tint;", streak);
            // NocScreen: the teal / ice / amber / cyan ramp, never violet
            Assert.IsTrue(screen.Contains("ScreenMix(PAL_ScreenTeal)") || screen.Contains("NocScrMix(PAL_ScreenTeal)"), "the screen ramp starts at ScreenTeal");
            StringAssert.Contains("PAL_SodiumAmber", screen, "the amber stop of the ramp (R6)");
            StringAssert.DoesNotContain("PAL_NeonViolet", screen);
        }
    }
}
