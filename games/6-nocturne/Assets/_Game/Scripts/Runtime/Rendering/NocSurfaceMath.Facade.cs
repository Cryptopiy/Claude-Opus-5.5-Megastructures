// NocSurfaceMath.Facade: the facade wall-plane macro of the wet mirror (TEXTURE_PLAN 2.11; owner SU): FA's facade mean radiance
// under PA's trilight. A separate part because it reads FA's NocFacadeMath (Look v2 FA-1).
using UnityEngine;

namespace Megastructure
{
    public static partial class NocSurfaceMath
    {
        /// <summary>A facade wall plane's macro (kind 0; TEXTURE_PLAN 2.11): FA's NocFacadeMath.MeanRadiance (mode 0 facade, 1 section)
        /// for the region at s, with the ambient luminance of PA's trilight equator at s (the term a vertical face sees).</summary>
        public static Color FacadeMacro(int mode, int region, double s)
        {
            AtmosphereDriver.AmbientAt(s, out Color sky, out Color eq, out Color gr);
            C3 m = NocFacadeMath.MeanRadiance(mode, region, s, NocMath.Luma709(eq.r, eq.g, eq.b));
            return new Color((float)m.r, (float)m.g, (float)m.b, 1f);
        }

    }
}
