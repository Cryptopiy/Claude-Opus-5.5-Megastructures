# MATERIALS - the shared PBR material sheet of Nocturne (MASTER_PROMPT 8.11, 14.2.1, 14.2.5)

One library, one generator, made once and reused by every builder:
- **Shaders.** `Megastructure/NocSurface` (lit surfaces), `Megastructure/NocWet` (every walkable floor, the analytic mirror of
  14.7 H4), `Megastructure/NocGlass` (tubes, strip). All use the Noc lit template (`Shaders/Include/NocLitCore/NocLitPass/
  NocLighting.hlsl`): URP `BRDFData` + `EnvironmentBRDF` + `LightingPhysicallyBased` (metallic workflow, GGX, Schlick Fresnel),
  analytic environment reflection = `NocSkyColorBlur` (the same sky and fog the dome shows), trilight ambient x LightCal,
  Forward+ lights x LightCal, specular AA (projected-space normal filtering, threshold 0.18) and smoothness <= 0.95, then
  `NocFog` and `NocFloor` last. No shadow maps (14.2.3).
- **Generator.** `Scripts/Runtime/Rendering/NocTexRecipes.cs` (pure functions, exactly periodic) baked by
  `Scripts/Editor/NocTextureGen.cs` (run by `MaterialFactory.Build` in RebuildAll) into `Assets/_Game/Textures/<Family>_{Albedo,
  Normal,Mask}.png`: albedo = variation around a mean of exactly 0.5 linear (sRGB on), normal from the height (wrapped central
  differences, physical relief depth), mask = R metallic, G occlusion, B height, A smoothness (linear). Mipmaps (Kaiser),
  trilinear, aniso 16 (floors) / 8, BC7/BC5 high quality, 1024 px (512 px moss, bark).
- **Colour rule.** `_BaseColor` is the palette token (x a factor, or a mix of two tokens for a metal F0); the albedo map only
  varies around it (x 2 x variation), so the token stays the mean and palette tokens stay the single colour source.
- **Projection.** NocSurface: world-space triplanar (|n|^4 weights, whiteout normal blend, per-plane offsets) + macro value
  noise (two octaves at the macro scale) on albedo (+-) and smoothness (+-). NocWet: planar xz (one tap per map).
- **Context layers (8.11 item 4).** Vertex AO (MeshKit colour R) as occlusion; edge wear (colour G, chamfers) exposes bare steel
  on painted steel; rain wetness darkens porous albedo (x0.6 soaked) and raises smoothness toward 0.85, more on up faces; drip
  streaks below edges on vertical faces (0.35 m x 7 m stretched noise, faded to its mean by footprint). Dust is not used: it rains.

## Rows (one per 14.2.5 material and albedo token)

Reference albedos are measured real-world values (linear). The tokens of 14.2.1 are deliberately 3-10 x darker (a night city
lit by emission; 14.2.1: "albedos are the dark WetAsphalt, ConcreteDark, Moss and Bark tokens"); LightCal (14.2.3) calibrates
the lights, never the albedos.

| Material (library field) | Shader | Albedo (token, linear) | Real-world reference | Metallic | Smoothness range | Normal | Scales (tile / macro) | Noise recipe | Capture |
|---|---|---|---|---|---|---|---|---|---|
| Wet street, alley (`FloorAlley`) | NocWet | WetAsphalt 0.0070, 0.0060, 0.0130 | asphalt 0.04-0.12 dry, ~0.03 wet | 0 | puddles GGX alpha 0.08 (sm. 0.72); else alpha 0.35 x (1.25..0.8) by map (sm. 0.41-0.47) | 0.7 | 2.0 m / 11 m | Asphalt: Worley aggregate (P 128, ~1.5 cm stones, light/dark by cell hash), binder fBm (P 16, 4 oct), hairline cracks (ridged P 3 thresholded, masked by fBm P 2) | sheet tile 1; KS00 |
| Wet stone pavers (`FloorStone`, `GrayFloor`) | NocWet | ConcreteDark x0.9 (WetStone #23232F) | granite / basalt setts 0.10-0.25 | 0 | as above | 0.6 | 2.4 m (0.6 m slabs, running bond) / 11 m | Paving: blocks 4 x 4 with 8 mm joints, domed bevel 2 cm, per-slab tone +-12 % and polish, grain fBm P 64, low fBm P 8 | sheet tile 2; KS04, KS10 |
| Steel deck (`FloorSteelDeck`) | NocWet | ConcreteDark x1.05 (MetalDark #2B2E3A) | painted steel plate 0.04-0.10 | 0 (paint + water film) | puddles 0.08; else alpha 0.30 | 0.6 | 2.4 m (1.2 x 2.4 m plates) / 11 m | SteelDeck: plates 2 x 1, welded seams (Gaussian weld bead + fBm), lozenge tread 3 cm pitch worn by fBm P 6, rust in seams (fBm P 12) | sheet tile 3 |
| Garden stone path (`FloorGardenPath`) | NocWet | ConcreteDark x0.8 | flagstones 0.10-0.20 | 0 | puddles 25 %; else alpha 0.40 | 0.7 | 1.8 m / 11 m | Paving maps at 1.8 m (smaller setts) | sheet tile 4 |
| Plaza granite (`FloorPlaza`) | NocWet | ConcreteDark | flamed granite 0.15-0.35 | 0 | puddles 0.08; else alpha 0.35 | 0.5 | 4.8 m (1.2 m slabs, stack bond) / 11 m | PlazaStone: blocks 4 x 4, 5 mm joints, Worley grains P 256 (light / dark specks, no sparkle), cloudy fBm P 12, per-slab tone | sheet tile 5 |
| Concrete (`Concrete`) | NocSurface | ConcreteDark 0.0232, 0.0232, 0.0395 | weathered concrete 0.20-0.35 | 0 | 0.15-0.50 (+-0.08 macro) | 1.0 | 3.0 m (formwork joint 1.5 m) / 9 m | Concrete: low fBm P 4 (5 oct), fine aggregate fBm P 48, air-void pits (Worley P 96, 1-2 cm), stains (anisotropic fBm 3 x 2) | sheet item 1 |
| Stone cladding (`StoneWall`) | NocSurface | ConcreteDark x0.9 (WetStone) | granite cladding 0.15-0.30 | 0 | 0.30-0.60 (+-0.07) | 0.8 | 4.8 m / 13 m | PlazaStone maps (1.2 m panels) | sheet item 2 |
| Painted steel (`SteelPainted`) | NocSurface | ConcreteDark x1.05 (MetalDark) | dark paint 0.03-0.08; bare steel F0 0.56 | 0 (paint); edge wear -> 1 | 0.30-0.60 (+-0.07); wear 0.55 | 0.8 | 2.0 m (1 m panels) / 7 m | Metal: panels 2 x 2, seams 1.5-4 mm, brushing (anisotropic fBm 8 x 256), scuffs (fBm P 10 thresholded); wear colour SignWhite x0.55 on chamfers (vertex G) | sheet item 3 |
| Bare steel (`SteelBare`) | NocSurface | F0 SignWhite x0.55 = 0.44, 0.48, 0.55 | iron / steel F0 0.53-0.58 | 1 | 0.45-0.75 (+-0.06) | 0.7 | 2.0 m / 7 m | Metal (as above) | sheet item 4 |
| Brass (`Brass`) | NocSurface | F0 WinWarm x0.4 + TitleText x0.45 = 0.80, 0.61, 0.42 | brass F0 0.91, 0.78, 0.42 | 1 | 0.50-0.80 (+-0.06) | 0.6 | 2.0 m / 5 m | Metal | sheet item 5 |
| Moss (`Moss`) | NocSurface | Moss 0.0123, 0.0423, 0.0194 | moss 0.05-0.15 (green) | 0 | 0.15-0.45 (+-0.07), wet 0.6 | 1.2 | 1.5 m / 6 m | Moss: cushions (inverted Worley P 24, 6 cm), fibres fBm P 96, low fBm P 4; tips lighter, hollows dark (AO) | sheet item 6 |
| Bark (`Bark`) | NocSurface | Bark 0.0232, 0.0137, 0.0103 | bark 0.05-0.15 | 0 | 0.10-0.40 (+-0.06) | 1.4 | 1.0 m / 5 m | Bark: vertical fibres (ridged noise 12 x 2, 4 oct), plates fBm P 6, cracks between ridges (AO, darker) | sheet item 7 |
| Asphalt kerbs and patches (`Asphalt`) | NocSurface | WetAsphalt | asphalt 0.04-0.12 | 0 | 0.20-0.50 (+-0.08), wet 0.7 | 0.9 | 2.0 m / 10 m | Asphalt maps | sheet item 8 |
| Glass (`GlassBack`, `GlassFront`) | NocGlass | tint ConcreteDark | soda-lime F0 0.04 | 0 | 0.95 (analytic) | - | - | none: alpha 0.12 + 0.6 Fresnel, premultiplied, back faces 3000 then front faces 3001 | sheet item 9 |
| NocWet mirror terms | NocWet | - | water film F0 0.02-0.03 | - | sigma2 = alpha^2 + 0.004 (+ ripple variance) | ripples 0.12 x fade | puddles 4.0 / 1.3 m noise, 35 % | sky (NocSkyColorBlur), <= 4 wall planes, <= 32 area sources (14.7 H4, D-003) | sheetW (streaks) |

Smoothness variation: every NocSurface row varies by (map 5-95 % spread x range) + 2 x macro >= 0.1, every NocWet row by
puddle against dry (0.72 vs 0.41) and the map term (MaterialTests checks it). Metallic is exactly 0 or 1 (MaterialTests).

## Not in this library (other owners, 14.2.5)
NocFacade, NocSection, NocNeon, NocScreen, NocFoliage, NocKoi, NocStreak, NocSteam, NocRain: emission-driven or special
shaders built on the same template and atmosphere; their builders add rows here.

## Captures
`QA/captures/A2a_sheet02/pose_sheetL.png`, `pose_sheetR.png`, `pose_sheetW.png` (built player, 1080p, `-material-sheet
-freeze-clock`; sky fill + 4 point lights, Forward+): command in `docs/logs/A2a.md`.
