// PaletteTests: T-PAL of MASTER_PROMPT 14.10.1 for BulwarkPalette (EditMode, NUnit). The 14.2.1 rows are restated
// here on purpose (an independent copy of the spec): hex must match exactly, linear within 0.004. The generated
// Assets/_Game/Shaders/Include/BulwarkPalette.hlsl must equal BulwarkPalette.ToHlsl() byte for byte.
using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using NUnit.Framework;

namespace Megastructure.Tests
{
    public class PaletteTests
    {
        // 14.2.1: token, sRGB hex, linear RGB as printed in the spec
        static readonly (string name, string hex, double r, double g, double b)[] Spec =
        {
            ("ConcreteMid", "#7F8281", 0.212, 0.223, 0.220),
            ("ConcreteDark", "#4B4F52", 0.070, 0.078, 0.084),
            ("ConcreteWet", "#363C41", 0.037, 0.045, 0.053),
            ("SteelPaint", "#5E6A73", 0.112, 0.144, 0.171),
            ("LaneWhite", "#D8D5CB", 0.687, 0.665, 0.597),
            ("SafetyOchre", "#C79A33", 0.571, 0.323, 0.033),
            ("SeaStorm", "#3F5C68", 0.050, 0.107, 0.138),
            ("SeaTeal", "#1F727C", 0.014, 0.168, 0.202),
            ("FoamWhite", "#EAF1F3", 0.823, 0.880, 0.896),
            ("StormHorizon", "#8B969C", 0.258, 0.305, 0.332),
            ("BreakGold", "#FFC987", 1.000, 0.584, 0.242),
            ("NoonHorizon", "#BCD5E7", 0.503, 0.665, 0.799),
            ("DuskAmber", "#F4A35A", 0.905, 0.366, 0.102),
            ("DuskRose", "#C9708A", 0.584, 0.162, 0.254),
            ("DuskViolet", "#5A4A8D", 0.102, 0.068, 0.266),
            ("DuskDeep", "#1E2552", 0.013, 0.019, 0.084),
            ("Tungsten", "#FFC27A", 1.000, 0.539, 0.195),
            ("SodiumWarm", "#FFB04A", 1.000, 0.434, 0.068),
            ("CityWarm", "#FFC070", 1.000, 0.527, 0.162),
            ("Lantern", "#FFB562", 1.000, 0.462, 0.122),
            ("TitleText", "#EDEBE5", 0.847, 0.831, 0.784),
        };

        // other colours the spec names (14.2.1 sky states and city cool; 14.2.3 key colours): hex only
        static readonly (string name, string hex)[] SpecExtra =
        {
            ("StormZenith", "#5F6E79"), ("NoonZenith", "#3F7FC2"), ("NoonKey", "#FFF3E3"),
            ("CityCool", "#BEE2FF"), ("StormKey", "#FFD9B0"), ("DuskKey", "#FF9A4A"),
            // Addendum A1 1.1 kraken tokens (append-only, kraken owner)
            ("KrakenSkinDark", "#1E242B"), ("KrakenMoss", "#3D4A3A"), ("KrakenUnderRed", "#7A2418"),
            ("KrakenSucker", "#D8C2B0"), ("KrakenEyeGlow", "#FF6127"), ("KrakenEyeMilky", "#B9C2BE"),
        };

        const double Tol = 0.004;

        [Test]
        public void Table_CompleteOrderedUnique()
        {
            var t = BulwarkPalette.Table;
            int count = Enum.GetValues(typeof(Pal)).Length;
            Assert.AreEqual(count, t.Length, "one row per Pal token");
            Assert.AreEqual(Spec.Length + SpecExtra.Length, t.Length);
            for (int i = 0; i < t.Length; i++)
            {
                Assert.AreEqual((Pal)i, t[i].id, $"row {i} in enum order");
                Assert.IsTrue(Regex.IsMatch(t[i].hex, "^#[0-9A-F]{6}$"), $"{t[i].id}: hex {t[i].hex} is #RRGGBB upper case");
                for (int j = 0; j < i; j++)
                    Assert.AreNotEqual(t[j].hex, t[i].hex, $"{t[i].id} duplicates {t[j].id}");
            }
        }

        [Test]
        public void Tokens_MatchSpecHexAndLinear()
        {
            for (int i = 0; i < Spec.Length; i++)
            {
                var row = Spec[i];
                Pal id = (Pal)Enum.Parse(typeof(Pal), row.name);
                Assert.AreEqual(i, (int)id, $"{row.name}: spec order");
                Assert.AreEqual(row.hex, BulwarkPalette.Hex(id), $"{row.name}: hex");
                BulwarkPalette.LinearRgb(id, out double r, out double g, out double b);
                Assert.AreEqual(row.r, r, Tol, $"{row.name}: linear r");
                Assert.AreEqual(row.g, g, Tol, $"{row.name}: linear g");
                Assert.AreEqual(row.b, b, Tol, $"{row.name}: linear b");
            }
            foreach (var row in SpecExtra)
            {
                Pal id = (Pal)Enum.Parse(typeof(Pal), row.name);
                Assert.AreEqual(row.hex, BulwarkPalette.Hex(id), $"{row.name}: hex");
            }
        }

        [Test]
        public void Linear_ExactEotfAndRoundTrip()
        {
            // exact piecewise sRGB EOTF, independent of the class
            foreach (var t in BulwarkPalette.Table)
            {
                BulwarkPalette.LinearRgb(t.id, out double r, out double g, out double b);
                double[] lin = { r, g, b };
                for (int c = 0; c < 3; c++)
                {
                    double v = Convert.ToInt32(t.hex.Substring(1 + 2 * c, 2), 16) / 255.0;
                    double want = v <= 0.04045 ? v / 12.92 : Math.Pow((v + 0.055) / 1.055, 2.4);
                    Assert.AreEqual(want, lin[c], 1e-12, $"{t.id} channel {c}");
                    Assert.AreEqual(v, BulwarkPalette.LinearToSrgb(lin[c]), 1e-9, $"{t.id} channel {c} round trip");
                }
            }
            double worst = 0.0;
            for (int i = 0; i <= 1000; i++)
            {
                double x = i / 1000.0;
                worst = Math.Max(worst, Math.Abs(BulwarkPalette.LinearToSrgb(BulwarkPalette.SrgbToLinear(x)) - x));
            }
            Assert.LessOrEqual(worst, 1e-9);
            // the float (Unity Color) path agrees with the double path
            var col = BulwarkPalette.Linear(Pal.SafetyOchre);
            BulwarkPalette.LinearRgb(Pal.SafetyOchre, out double sr, out double sg, out double sb);
            Assert.AreEqual(sr, col.r, 1e-6);
            Assert.AreEqual(sg, col.g, 1e-6);
            Assert.AreEqual(sb, col.b, 1e-6);
            Assert.AreEqual(1f, col.a, 0f);
        }

        [Test]
        public void F6_Format()
        {
            Assert.AreEqual("1.0", BulwarkPalette.F6(1.0));
            Assert.AreEqual("0.0", BulwarkPalette.F6(0.0));
            Assert.AreEqual("0.5", BulwarkPalette.F6(0.5));
            Assert.AreEqual("0.212231", BulwarkPalette.F6(0.2122307));
            Assert.AreEqual("0.03", BulwarkPalette.F6(0.0300001));
        }

        [Test]
        public void Hlsl_FileEqualsToHlsl()
        {
            string text = BulwarkPalette.ToHlsl();
            Assert.IsFalse(text.Contains("\r"), "LF line ends only");
            Assert.IsTrue(text.EndsWith("#endif\n", StringComparison.Ordinal));
            Assert.IsTrue(text.Contains("#ifndef " + BulwarkPalette.HlslGuard + "\n"));

            string path = FindProjectFile(BulwarkPalette.HlslAssetPath);
            Assert.IsNotNull(path, $"{BulwarkPalette.HlslAssetPath} exists (generate it once from BulwarkPalette.ToHlsl())");
            byte[] file = File.ReadAllBytes(path);
            byte[] want = new UTF8Encoding(false).GetBytes(text);
            Assert.IsFalse(file.Length >= 3 && file[0] == 0xEF && file[1] == 0xBB && file[2] == 0xBF, "no BOM");
            Assert.AreEqual(want.Length, file.Length, "file length equals ToHlsl()");
            for (int i = 0; i < want.Length; i++)
                if (want[i] != file[i]) Assert.Fail($"BulwarkPalette.hlsl differs from ToHlsl() at byte {i}: regenerate it");
        }

        [Test]
        public void Hlsl_DefinesEveryTokenWithItsLinearValue()
        {
            string text = BulwarkPalette.ToHlsl();
            var re = new Regex(@"^#define PAL_(\w+) float3\(([-0-9.]+), ([-0-9.]+), ([-0-9.]+)\)$", RegexOptions.Multiline);
            var found = re.Matches(text);
            Assert.AreEqual(BulwarkPalette.Table.Length, found.Count, "one PAL_ define per token");
            for (int i = 0; i < found.Count; i++)
            {
                var m = found[i];
                Pal id = (Pal)Enum.Parse(typeof(Pal), m.Groups[1].Value);
                Assert.AreEqual(BulwarkPalette.Table[i].id, id, "defines in table order");
                BulwarkPalette.LinearRgb(id, out double r, out double g, out double b);
                Assert.AreEqual(r, double.Parse(m.Groups[2].Value, CultureInfo.InvariantCulture), 6e-7, $"PAL_{id} r");
                Assert.AreEqual(g, double.Parse(m.Groups[3].Value, CultureInfo.InvariantCulture), 6e-7, $"PAL_{id} g");
                Assert.AreEqual(b, double.Parse(m.Groups[4].Value, CultureInfo.InvariantCulture), 6e-7, $"PAL_{id} b");
            }
        }

        /// <summary>Resolves a project-relative path from the working directory or any parent (Unity runs tests from the
        /// project root).</summary>
        static string FindProjectFile(string relative)
        {
            string dir = Directory.GetCurrentDirectory();
            for (int i = 0; i < 8 && dir != null; i++)
            {
                string p = Path.Combine(dir, relative.Replace('/', Path.DirectorySeparatorChar));
                if (File.Exists(p)) return p;
                dir = Path.GetDirectoryName(dir);
            }
            return null;
        }
    }
}
