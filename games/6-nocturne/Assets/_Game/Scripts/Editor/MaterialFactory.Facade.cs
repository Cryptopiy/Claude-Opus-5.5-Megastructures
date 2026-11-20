// MaterialFactory.Facade: the facade rows of the material contract (owner FA; TEXTURE_PLAN 8.3). Created by TP-0 with the Facade
// row moved VERBATIM from MaterialFactory.cs (no picture changes) and handed over to FA. Arrays (FacadeArr, RoomArr, OverlayArr)
// are wired with MatSpec.Tx(prop, path) or ExtMaps(key); MaterialFactory loads every texture as Texture, so arrays work.
using System.Collections.Generic;
using UnityEngine.Rendering;

namespace Megastructure.EditorTools
{
    public static partial class MaterialFactory
    {
        /// <summary>NocFacade rows (FA).</summary>
        static List<MatSpec> FacadeRows()
        {
            var facade = Make("Facade", ShaderFacade).C("_BaseColor", NocPal.ConcreteDark).F("_WindowGain", NocturneConstants.FacadeWindowGain)
                .F("_EdgeGain", NocturneConstants.FacadeEdgeGain).F("_WallSmooth", 0.35).F("_GlassSmooth", 0.85).F("_GlassDark", 0.5)
                .F("_MacroAlbedo", 0.15).F("_Cull", (float)CullMode.Back);
            FacadeSkins(facade);
            return new List<MatSpec>
            {
                // ---- city and lights (A2b): facade (ConcreteDark wall, windows x FacadeWindowGain), instanced signs and streaks
                facade,
            };
        }

        /// <summary>Look v2 photo skins (FA-4): FacadeArr's three arrays and the slice tiles of its texmeta (TP-1); _FaTexOn 1 only when
        /// the processed arrays exist, so the v2 path stays analytic (never black) without them.</summary>
        static void FacadeSkins(MatSpec m)
        {
            string meta = NocTextureImport.ManifestOf("FacadeArr");
            if (meta == null) { m.F("_FaTexOn", 0.0); return; }
            string prefix = meta.Substring(0, meta.Length - TexMeta.Suffix.Length);
            m.Tx("_FacadeAlb", prefix + "_Albedo.png").Tx("_FacadeNrm", prefix + "_Normal.png").Tx("_FacadeMask", prefix + "_Mask.png");
            var t = TexMeta.Load(NocPaths.Abs(meta));
            for (int k = 0; k < 8; k++) m.F("_FaTile" + k, t != null ? t.SliceTileU(k) : 2.0f);
            m.F("_FaTexOn", 1.0);
            // TP's OverlayArr: slice 0 (leaks) for the sill streaks
            string ov = NocTextureImport.ManifestOf("OverlayArr");
            if (ov != null) { m.Tx("_OverlayMask", ov.Substring(0, ov.Length - TexMeta.Suffix.Length) + "_Mask.png").F("_FaStreakOn", 1.0); }
            else m.F("_FaStreakOn", 0.0);
        }
    }
}
