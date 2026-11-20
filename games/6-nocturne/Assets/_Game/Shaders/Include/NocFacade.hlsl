// NocFacade.hlsl - the facade globals of every tower face (MASTER_PROMPT 14.7 H2, 14.2.5 NocFacade, D-004, R1 section 3).
// TP-2 (FX, Phase 4 wave A): the v1 window model that stood here (strata, zone level, epochs, 2.5-5.5 px fade; described below
// for its history) is deleted; the facade is Look v2 alone (NocFacadeSkin.hlsl, which includes this file for _NocFacade).
// No window is geometry. Face coordinates are world-derived (CityMesher writes them in double): u = metres along the face
// (u = dot(P.xz, t_face), the face yaw quantised to 0.5 deg), v = world y, face id = pcg of (yaw lattice, plane offset lattice),
// so coplanar pieces of the two city tiers get identical windows. The face is cut into strata bands of 86.4 m (24 residential
// floors): residential 2.4 x 3.6 (window 2.0 x 2.4), commercial 4.8 x 4.32, mechanical slits 9 x 6.17, sky lobby 18 x 14.4
// (D-017: whole rows per band); section bays (mode 1) 9 x 4 in 88 m bands. Lit probability 0.34 x stratum factor x zone level
// (24 columns x 2 floors: lit floors read as horizontal bands, never as square blocks; p_zone = 0.34 f (0.55 + 0.9 U), zone rows
// shifted against graph paper), stateless epochs of 150 s with a 1.2 s
// toggle (pcg4d), hues cold 82 % / cyan 8 % / magenta 6 % / white 4 % with the per-beat warm fraction in a 0.03 band (exactly 0
// at w_f = 0). Filtering: per-axis fade w = smoothstep(2.5, 5.5, pitch_px), the exact 2 x 2 box filter of the cells under the
// footprint while w > 0, separable blend of resolved / row / column / mean terms (an oblique facade keeps its floor bands), the
// zone level box-filtered over 2 x 2 zones, neighbour-band means at band boundaries. Everything converges to the mean energy,
// so the fade moves no linear-light mean (FacadeTests, D-004). The C# mirror is NocFacadeMath (Runtime/Rendering).
#ifndef NOC_FACADE_INCLUDED
#define NOC_FACADE_INCLUDED

#include "NocCommon.hlsl"

float4 _NocFacade;   // x = epoch phase t / 150 (wrapped mod 65 536 in double), y = warm fraction w_f (Rule P), z = scene time (s, wrapped 3 600), w = 0

#endif
