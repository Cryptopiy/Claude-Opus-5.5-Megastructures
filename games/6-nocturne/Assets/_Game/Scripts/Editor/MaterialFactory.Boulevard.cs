// MaterialFactory.Boulevard: the Phase 4 rows of the material contract owned by BV (D-033). Created empty by the lead; the
// owner adds its rows here, its fields in MaterialLibrary.Boulevard.cs and its row tests in MaterialTests.Boulevard.cs.
//   * Tram (Megastructure/NocTram, Shaders/Beats): the tram T_A of 14.5.2. Body MetalPaint (token + TP's processed CC0 set), the
//     saloon band WinNeutral at a mean of 0.30 (LOOK_V2 6: linear 0.25-0.32, luma 135-155), headlights SignWhite x 3.0 (core
//     >= 2.0), tail lights AviationRed x 1.2, the destination band SodiumAmber x 0.35; the clip planes x 214.47 (inside the
//     terminus) and z 278 (inside the depot tunnel) are the shader defaults and are set again by TramBuilder from TramSchedule.
using System.Collections.Generic;

namespace Megastructure.EditorTools
{
    public static partial class MaterialFactory
    {
        public const string ShaderTram = "Megastructure/NocTram";

        /// <summary>Phase 4 rows of BV (beat 2: tram, street level).</summary>
        static List<MatSpec> BoulevardRows()
        {
            return new List<MatSpec>
            {
                Make("Tram", ShaderTram).C("_BaseColor", NocPal.MetalPaint).ExtMaps("MetalPaint")
                    .C("_StripeColor", NocPal.Granite, 0.75f)
                    .C("_WinLight", NocPal.WinNeutral).F("_WinGain", TramLook.WinGain)
                    .C("_HeadLight", NocPal.SignWhite).F("_HeadGain", TramLook.HeadGain)
                    .C("_TailLight", NocPal.AviationRed).F("_TailGain", TramLook.TailGain)
                    .C("_Display", NocPal.SodiumAmber).F("_DisplayGain", TramLook.DisplayGain)
                    .F("_Cull", 2),
            };
        }
    }
}
