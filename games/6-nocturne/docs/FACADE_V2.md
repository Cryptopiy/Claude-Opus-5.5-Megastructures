# FACADE_V2 - the Look v2 facade (owner FA; TEXTURE_PLAN section 2, LOOK_V2 section 4)

The v2 facade is selected per frame by the uniform `_NocFacadeV` (`-facade-v 2`; v1 stays the default until FA-5). It keeps the
H2 machinery (stateless epochs, zones, the exact 2 x 2 box filter, the per-axis 2.5-5.5 px fade, tier identity) and adds structure,
skins, recessed openings with room light, a reflected city in the glass and a macro layer for 3 km.

## Files
| File | Role |
|---|---|
| `Shaders/NocFacade.shader` | surface hook (v1 / v2 uniform branch after every derivative), forward hook (glass reflection + skyline), debug views 1-6 and 10 |
| `Shaders/Include/NocFacadeSkin.hlsl` | skins, regions, cells, whole bays, rows, lit groups, zones, hue / brightness, windows, recess rectangles, spill, halo, band means, ledge line, edge colours, the reflected skyline |
| `Scripts/Runtime/Core/NocFacadeConstants.cs` -> `Shaders/Generated/NocFacadeConstantsGen.hlsl` | every `Fa*` constant (`NC_Fa*` in HLSL) |
| `Scripts/Runtime/Rendering/NocFacadeMath.cs` | C# twin (`EvalV2`, `MeanRadiance`, `SkinByte`, `SkinOverride`, `Region`, the cell API `CellOf` / `Bays` / `Rows` / `StratumV2`) |
| `Scripts/Runtime/Rendering/NocFacadeSheet.cs` + `Tools/qa/facade_sheet.py` | the lookdev sheet (`-facade-sheet`) and its measurements |
| `Scripts/Tests/EditMode/FacadeSkinTests.cs` | vertex contract on the built world, skin bytes, regime energy, lit probability, hue split, budgets, the sheet |

## Vertex contract (every NocFacade writer)
`uv0 = (u metres along the face, 16-bit face id)`, `uv1 = (uMin, uMax, y0, y1)` (y0 = y1 = 0: unknown, the Gulf writers),
colour R = AO, G = skin byte, B = mode (0 facade, 128 section, 255 roof), A = edge kind x 63.
Skin byte: `NocFacadeMath.SkinByte(seed, region)`: region D 0x00-0x77 (`seed % 120`), region OQ (disc (1 250, 388), R 600)
0x78-0xEF, hero override k 0xF0 + k (`seed = NocFacadeMath.SkinOverride(k)`): 0 spire S0 fins, 1 screen-wall mass S2 LED,
2 alley west S1, 3 alley east S4, 4 boulevard podium S1 + stone base. Every face of a tower or box carries one byte.

## Skins (per tower: K = pcg(byte) keys strata, zone rows, lit floor groups, multiplier, core strip, mechanical stripes)
| Skin | Share D / OQ | Residential cell 2.4 x 3.6 | Recess |
|---|---|---|---|
| S0 glass curtain wall | 25 / 10 % | slab cap 0.18, spandrel glass to 1.89, vision glass 1.95-3.54, mullions 0.12 every 1.2 | 0.05 |
| S1 precast | 30 / 25 % | slab 0.35, window 1.6 x 2.2 at sill 0.9 (50 %: 1.72 wide with a centre mullion), frame 0.12, joints | 0.22 |
| S2 metal ribbon | 15 / 5 % | panel band 0.30, ribbon 1.8 at sill 0.9, mullions 0.08 every 1.2, joints every 1.2 | 0.08 |
| S3 brick | 5 / 40 % | slab 0.12, window 1.2 x 1.6 at sill 0.85, frame 0.08 | 0.18 |
| S4 tiled residential | 25 / 20 % | slab 0.25, window 1.4 x 1.6 at sill 0.9, frame 0.08 | 0.15 |
Commercial 4.8 x 4.32 (4.4 x 3.2 openings, mullions every 1.1), mechanical 9 x 6.17 (louvre slits), sky lobby 18 x 14.4
(17 x 12 in portal frames), section 9 x 4 (mode 1). Whole bays only: openings inside [uMin + 0.6, uMax - 0.6] x
[y0 + base (6 m on podium faces), y1 - 1.6]; band means count the suppressed cells.

## Light (LOOK_V2 4)
p = clamp(0.18 x f_stratum x m_T x g_G x z_Z, 0, 1); f 1.08 / 0.69 / 0.23 / 2.31; m_T log-normal sigma 0.35 per tower; g_G
log-normal sigma 0.45 per lit floor group (4-12 floors); z = 0.40 + 0.6 (U_row(K) + U_cell(face)). E = token x (0.12 / Y(token))
x b (b = exp(0.45 N) in [0.40, 2.40], E[b] 1.0954) x I / E[I]; hue classes move with w_f(s) = `_NocFacade.y` and n_f(s) =
`_NocFacade.w`; stand-by x0.0005. Interior factor I: ceiling falloff 1.0 -> 0.7, curtains 35 % (x0.45, tint toward WinWarm),
blinds 15 % (8 cm slats); only I / E[I] - 1 fades in over 4-8 px of window height.

## Glass (FA-2)
F0 0.04, perceptual roughness 0.05, +-1.5 deg pane tilt (folded into roughness below 2 px panes). The reflection is the sky
(`NocReflection`) behind the reflected city (`NocSkylineV2`): 4 deg azimuth bins cut into 1-3 tower slots; a tower of 34-64 m
fills its slot (so it stands at d = W / slot angle); its body is the unresolved facade mean (lit glass 0.332 of the area) with
window speckle on the city's own 2.4 x 3.6 m grid (hashed phase per tower), faded by the H2 fade in blur footprints, fogged by
d and faded into the deck over the city's cloud-mix band. The skyline replaces the D-017 city term on the glass; the opaque
specular keeps it (`f.specOcc`).

## Energy and filtering rules
E1-E7 and S1-S7 of TEXTURE_PLAN 2.9: every element is an interval in cell coordinates covered by `NocCover` over the 2 x 2 cells
under the bounding-box footprint and blended per axis to its band mean below the fade; neighbour bands take their own means;
no per-pixel noise; derivatives only before the uniform v1 / v2 branch.

## Debug views (`-debug-view N`, raw codes)
1 skin x 40 / region x 128 / 16 (k + 1) for override k; 2 window weight w_u w_v; 3 frac(x / P.x), frac(y / P.y), whole-bay
mask; 4 sigma (R where -1, G where +1); 5 32 (log2(P / fp) + 2) per axis; 6 128 + 64 D kx / W, 128 + 64 D ky / H (recess shift);
10 lit probability, K & 0xFF.

## Facade sheet
`-facade-sheet` builds S0..S4, overrides 0..4 and the section mode as 30 x 30 x 200 m towers turned 0 / 45 / 75 deg in a row at
z 5 600 across the north Gulf; `NocFacadeSheet.Poses()` lists 27 stills (3 000, 1 000, 300 m and per subject 100 and 30 m).
`python -I Tools/qa/facade_sheet.py <dirs> [--debug6 <dir>] [--encode]`: floor-line autocorrelation per band, window / pier
contrast, micro / meso texture before and after x264, mean luma per skin, the reveal sign from debug view 6.
