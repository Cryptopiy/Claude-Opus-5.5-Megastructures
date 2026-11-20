// MaterialLibrary: every material of the game, created as .mat assets by the editor build script (MaterialFactory) with
// colours taken from NocturnePalette only (Thread's pattern). Beat builders use these references; nobody creates a material
// at runtime. Graybox fields, then the shared PBR library of 8.11 / docs/MATERIALS.md (NocSurface surfaces, NocWet floors
// per 14.4 surface, NocGlass back/front), the city and light fields of A2b (NocFacade, NocNeon, NocStreak tails / heads /
// beacons, the NocRain pass material); the remaining 14.2.5 shaders (NocSection, NocScreen, NocFoliage, NocKoi) are added by
// their owners. Every Material field is enumerated by All() (WarmupLoader,
// Validate), so a new field is warmed and validated without further code. Look v2 (TEXTURE_PLAN 8.3 TP-0): this file keeps
// FloorFor, All and FieldNames; the fields are split by owner into MaterialLibrary.{Surface,Facade,Look,Foliage}.cs.
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Megastructure
{
    [CreateAssetMenu(menuName = "Megastructure/Material Library")]
    public partial class MaterialLibrary : ScriptableObject
    {
        // the fields live in the partial files of their owners (TEXTURE_PLAN 8.3): MaterialLibrary.Surface.cs (SU),
        // .Facade.cs (FA), .Look.cs (PA), .Foliage.cs (FO); serialised names are unchanged

        static FieldInfo[] s_fields;

        /// <summary>The wet floor of the 14.4 surface at progress s (graybox and beat floors; GrayFloor when a field is unset).</summary>
        public Material FloorFor(double s)
        {
            Material m;
            if (s < 120.0) m = FloorAlley;
            else if (s < 560.0) m = FloorStone;
            else if (s < 880.0) m = FloorSteelDeck;
            else if (s < 1080.0) m = FloorGardenPath;
            else if (s < 1260.0) m = FloorStone;
            else m = FloorPlaza;
            return m != null ? m : GrayFloor;
        }

        /// <summary>Every non-null Material field (reflection over the declared fields, evaluated at load time only).</summary>
        public IEnumerable<Material> All()
        {
            if (s_fields == null) s_fields = typeof(MaterialLibrary).GetFields(BindingFlags.Instance | BindingFlags.Public);
            foreach (var f in s_fields)
            {
                if (f.FieldType != typeof(Material)) continue;
                var m = f.GetValue(this) as Material;
                if (m != null) yield return m;
            }
        }

        /// <summary>Names of every Material field (Validate checks that none is null).</summary>
        public static IEnumerable<string> FieldNames()
        {
            foreach (var f in typeof(MaterialLibrary).GetFields(BindingFlags.Instance | BindingFlags.Public))
                if (f.FieldType == typeof(Material)) yield return f.Name;
        }
    }
}
