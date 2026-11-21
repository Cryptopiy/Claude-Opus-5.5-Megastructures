// MaterialLibrary: every material of the game (MASTER_PROMPT 14.2.6), created as .mat assets by the editor build script
// (MaterialFactory) with colours taken from BulwarkPalette only and textures from the one generator (BwTexGen ->
// Editor/TextureFactory, Assets/_Game/Textures). Beat builders use these references; nobody creates a material at runtime.
// Wave A (materials owner) added the baseline families of 14.2.6: BwRoad, BwSteel, BwGrating, BwArmour, BwEmissive,
// BwLantern. Still to come from their owners: BwSea, BwSheet / BwConduit, BwFarField, BwWaterPortal, BwMist, BwSnow,
// BwCreature. When a family's shader lands, add its field here AND its MatSpec in MaterialFactory.Specs() (the contract
// check fails on a field without a spec and on a spec without a field). Field names are API: the graybox and the
// framework use Concrete, ConcreteWet, ConcreteDark, Rail, Handrail, Beam, Grade.
// Conventions every builder relies on (docs/MATERIALS.md): vertex colour R = baked AO (255 open), G = edge wear (255 on
// chamfers), B = family mask (BwSteel / BwArmour: ochre bands; BwConcrete: drip-stain source); UV0 unused (world-space
// triplanar), so meshes need no UVs or tangents.
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Megastructure
{
    [CreateAssetMenu(menuName = "Megastructure/Material Library")]
    public class MaterialLibrary : ScriptableObject
    {
        [Header("Structure (BwConcrete)")]
        public Material Concrete;        // ConcreteMid, dry (follows the rain wetness _BwSurface.x)
        public Material ConcreteWet;     // ConcreteMid -> ConcreteWet, _Wet 1 (spray zone, beat 3 deck, tunnel floor)
        public Material ConcreteDark;    // ConcreteDark (basalt: lee face, tunnel, ledge, units)

        [Header("Road (BwRoad)")]
        public Material Road;            // crest deck: broomed ConcreteMid pavement, lane lines, chainage numerals in LaneWhite

        [Header("Steel (BwSteel)")]
        public Material Steel;           // SteelPaint, ochre bands by vertex B, edge wear to bare metal (doors, gates, lamp posts)
        public Material SteelOchre;      // SafetyOchre paint (nosings, gauge platforms, door seams), same wear model

        [Header("Grating (BwGrating)")]
        public Material Grating;         // 40 mm bars at 80 mm pitch, analytic coverage, alpha-to-coverage

        [Header("Armour (BwArmour)")]
        public Material Armour;          // ConcreteDark / ConcreteWet by height above the sea, barnacle crust, foam line, gauges

        [Header("Thin lines (BwLine)")]
        public Material Rail;            // SteelPaint round tube (D-07)
        public Material Handrail;        // SafetyOchre round tube

        [Header("Light (BwBeam, BwEmissive, BwLantern)")]
        public Material Beam;            // additive analytic shafts
        public Material LampSodium;      // SodiumWarm x2.5 (gallery and tunnel cages, crest lamp heads), clamp 3
        public Material LampTungsten;    // Tungsten x3.0 (tower pendants, stair cages), clamp 3
        public Material Lantern;         // Lantern glass x6 with the +-3 % flicker (LanternFlicker), clamp 6

        [Header("Post")]
        public Material Grade;           // BwGrade (bloom + grade + dither), used by BwGradeFeature

        static FieldInfo[] s_fields;

        /// <summary>The Material fields of this class in declaration order (MaterialFactory contract, warm-up).</summary>
        public static FieldInfo[] MaterialFields()
        {
            if (s_fields == null)
            {
                var list = new List<FieldInfo>();
                foreach (var f in typeof(MaterialLibrary).GetFields(BindingFlags.Public | BindingFlags.Instance))
                    if (f.FieldType == typeof(Material)) list.Add(f);
                s_fields = list.ToArray();
            }
            return s_fields;
        }

        /// <summary>Every assigned material (WarmupLoader). Allocates; load time only.</summary>
        public IEnumerable<Material> All()
        {
            foreach (var f in MaterialFields())
            {
                var m = f.GetValue(this) as Material;
                if (m != null) yield return m;
            }
        }
    }

    /// <summary>
    /// C# twin of BwRoad's chainage numerals (14.2.6): seven-segment masks (bit a = 1 ... g = 64, the BwSevenSeg table of
    /// BwRoad.shader) and the placement: k = 1..9 at z = 100 k in the lee lane (x -3.0), k = 1..8 at z = 100 k + 50 in the
    /// sea lane (x +3.0, the sea-lane "09" would sit at z 950 inside the tower, which has no road), shown as "0k", tens
    /// digit toward -x (left for a walker heading north), segment a toward +z.
    /// </summary>
    public static class RoadMarkings
    {
        public static readonly int[] Segments = { 63, 6, 91, 79, 102, 109, 125, 7, 127, 111 };
        public const int LeeCount = 9, SeaCount = 8;

        public static void Number(bool seaLane, int k, out double x, out double z, out string text)
        {
            x = seaLane ? BulwarkConst.LaneDigitSeaX : BulwarkConst.LaneDigitLeeX;
            z = k * BulwarkConst.LaneDigitPitch + (seaLane ? BulwarkConst.LaneDigitSeaOffset : 0.0);
            text = (k / 10).ToString(System.Globalization.CultureInfo.InvariantCulture) + (k % 10).ToString(System.Globalization.CultureInfo.InvariantCulture);
        }

        /// <summary>Number of lit segments of a digit (tests: 6, 2, 5, 5, 4, 5, 6, 3, 7, 6).</summary>
        public static int SegmentCount(int digit)
        {
            int m = Segments[digit], n = 0;
            for (int b = 0; b < 7; b++) if ((m & (1 << b)) != 0) n++;
            return n;
        }
    }

    /// <summary>
    /// The lantern flicker of 14.2.5 / 14.11 as a pure function of the ambient clock: three sines at 0.31, 0.77 and 0.97 Hz
    /// with weights 1 : 0.6 : 0.3 and fixed phases, normalised so the total is +-3 % (TC_LanternFlicker). No component above
    /// 1 Hz (photosensitivity rule). BwLantern.shader evaluates the same sum on _BwTime.x; the beat-7 builder multiplies the
    /// lantern's point light by Gain(t) so glass and light agree.
    /// </summary>
    public static class LanternFlicker
    {
        public static readonly double[] Hz = { BulwarkConst.LanternHz1, BulwarkConst.LanternHz2, BulwarkConst.LanternHz3 };
        public static readonly double[] Weight = { BulwarkConst.LanternW1, BulwarkConst.LanternW2, BulwarkConst.LanternW3 };
        /// <summary>Phases (rad), mirrored in BwLantern.shader (BW_LANTERN_PHASE*).</summary>
        public static readonly double[] Phase = { 0.0, 1.7, 4.1 };

        /// <summary>Multiplier of the lantern radiance at ambient time t (s): 1 +- 0.03.</summary>
        public static double Gain(double t)
        {
            double sum = 0.0, norm = 0.0;
            for (int i = 0; i < 3; i++)
            {
                sum += Weight[i] * Math.Sin(2.0 * Math.PI * Hz[i] * t + Phase[i]);
                norm += Weight[i];
            }
            return 1.0 + BulwarkConst.LanternFlicker * sum / norm;
        }
    }
}
