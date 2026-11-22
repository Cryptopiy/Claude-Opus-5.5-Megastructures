# LOOK V2 - art direction for Nocturne (dark, not violet, textured, cinematic)

Authority: user directive D-027 (2026-11-14), the highest authority of the run. Written by ART, finalised by ART-F after two
reviews (ART-C1 cinematographer, ART-C2 rendering engineer); section 12 logs every amendment and what was done with it.
This file replaces, wherever they differ: the token values of MASTER_PROMPT 14.2.1, the colour words and stops of 14.2.2
("magenta cloud deck", violet haze), the colours and intensities of the 14.2.3 rig, the EV / saturation / bloom / vignette
columns of 14.2.4 and its fog sigma of beats 2-4 (3.6e-4 -> 4.4e-4), the hue line of NocFacade in 14.2.5, the mean-luma
windows of 14.10, and the order of exposure and vignette in the grade (now relative to the Floor, section 7). Geometry,
timing, rain, H1-H8 mechanisms, gaze and key-shot poses are unchanged. Every number below is ready to type in; the
conversions were computed with Python (exact sRGB EOTF, Rec.709 luma on 8-bit codes, the grade's identity tone curve below
linear 0.76, URP's Lambert x 0.96 and split-sum sky reflection, scripts listed at the end of section 11).

Baseline that this replaces (G4_r04 key shots, measured with the script of section 8): 17-81 % of every frame is violet or
magenta air (hue 250-340, saturation > 0.35, luma 16-119), the largest saturated region covers 28-94 % of the frame, mid-tone
mean saturation 0.53-0.73, the sky top median luma 56-85 at saturation 0.65-0.68, the darks carry a B - G cast of +27..+40
codes, and the textured walls show a micro high-pass deviation of 0.88-1.10 codes (the dither alone gives about 0.9) and a
meso deviation of 0.15-0.23 codes: the walls carry no detail. Root cause of the flatness: the albedo tokens are 3-10 x darker
than real matter (ConcreteDark 0.023 linear), so a wall lit by the ambient returns 0.0006 linear, below the Floor, and no
texture or normal map can ever show; the colour came only from the violet sky, fog and emission.

---------------------------------------------------------------------------------------------------------------------------

## 1. Mood and references

**Mood statement.**
A long exposure of a real megacity on a rainy, overcast night, shot by a cinematographer who loves darkness.
The masses are charcoal, graphite and wet stone; you read them by their texture and their volume, not by their colour.
Above hangs a low ceiling of cloud lit from below to a dull warm grey, brightest over the boulevard; below, the haze of the
lower city glows like embers in fog.
Light belongs to people: windows in a mix of tungsten, neutral and cold white, street lamps pooling warm on wet stone, a halo
of rain around every lamp.
Neon is punctuation, never wallpaper: small, saturated, reflected twice as long in the street.
The palette cools as you walk toward the Heart, so the koi's colour and the one warm window land as climaxes.
Blacks are blue-black, deep but never crushed: everything stays readable on a phone in a dark room and on a 4K TV.

**References** (descriptions of looks, not images to copy; sources in section 11):
1. **Hong Kong side streets in rain (Mong Kok, Yau Ma Tei), contemporary.** Dense projecting signs in a few hues, white LED
   shop light mixed with the last sodium lamps, black asphalt turned into a mirror, and a low ochre-grey cloud ceiling with no
   stars. The take-away: the sky has a colour, and it is a dirty warm grey, never purple.
2. **Greg Girard's Hong Kong nights (1974-1989).** Colour film of neon over a dark city: windows in mixed tungsten, green-cool
   fluorescent and white light, never one hue; the darkness between windows is deep and textured.
3. **Collateral (2004; Mann, Beebe and Cameron, digital night exteriors).** Los Angeles at night where the city's light is
   reflected in low clouds and the sky becomes a lit, warm-brown ceiling against which palms and towers read as silhouettes;
   fill light is very sparse.
4. **Blade Runner 2049 (Deakins).** A grey, decaying city mass against which saturated colour appears only in a few
   places (holograms, a club) and therefore hits hard; giant holograms read as translucent and structured, not as flat shapes.
   This is the model for the koi.
5. **The Batman (2022, Greig Fraser).** A cool, desaturated night in rain where bright accents hit "like a punch"; Fraser's
   aim of frames on the edge of unreadability is our lower bound: dark, but never mush.
6. **Tokyo bar alleys (Shinjuku, Omoide Yokocho) in rain.** Tight walls, overhead cables, 2700 K lantern and bar light that
   pools on wet stone and steams; everything outside the pools is near-black brick and plaster. Model for beat 1.
7. **Chongqing on an overcast night.** A vertical city whose towers vanish into a low, lit cloud; bridges as chains of warm
   lamps over a void of mist. Model for beats 3 and 4.
8. **Cities seen from a high floor above a fog layer.** The fog below glows from the street lights beneath it (amber where
   sodium survives, white where LED replaced it); tower crowns rise out of it. Model for the abyss.
9. **Ghost in the Shell (1995).** An overcast, rain-soaked megacity in greys and muted greens with weathered surfaces that make
   the city feel old; colour reserved for signs and water. Model for surface weathering.
10. **Rain-night street photography in general.** Light sources on wet asphalt become long vertical streaks, because ripples
    stretch the reflection along the view direction while the brightest part stays near the mirror point (already H4); every
    lamp also wears a soft halo of lit rain in the air (section 6).

Measured physics behind the numbers (section 11): an overcast sky inside a large city is roughly 10 x brighter than a clear
one (Berlin, factor 10.1), still only 0.1 to a few tenths of a cd/m2; average lit residential windows measure about 6 cd/m2
and adverts about 50 cd/m2; whole neon signs 100-300 cd/m2 and bare tubes thousands. So in real linear light, sky : window :
sign is about 1 : 20-60 : 500-2 000. The graybox inverts this (sky luma 80, windows 100-200). Look v2 restores the order but
compresses the range for display: deck 0.018 linear, median window 0.12, sign cores 1.0-1.3, i.e. about 1 : 7 : 55-75; the
sky displays at luma 37-47 (the hot lid 60-70), windows at about 100 median, signs and lamps clip into bloom.

---------------------------------------------------------------------------------------------------------------------------

## 2. Palette v2 (`NocturnePalette.cs` -> generated `NocPalette.hlsl`)

Luma = Rec.709 luma of the hex. Disp. = the luma displayed at EV 0 with the Floor added (`Floor + token x (1 - Floor)`; the
grade is identity below linear 0.76), given for the tokens that appear as sky, fog or substrate; albedo tokens never display
as such, and emissive cores pass through the Neutral curve (table in 2.3). Hue / sat = HSV of the hex. The white balance of
the whole film is a 4 300 K camera: blackbody colours were computed from the Planckian locus (Kim et al. 2002 spline) with a
Bradford adaptation from 4 300 K to D65 (2 700 K = #FFC584, 4 000 K = #FFF6EB, 5 600 K = #C5DDFF, 6 500 K = #AFD0FF,
HPS 2 050 K = #FFA042).

### 2.1 The 28 tokens of 14.2.1

| Token | v2 hex | v2 linear | Luma | Disp. | Hue / sat | v1 (hex, luma, hue) | Role and reason |
|---|---|---|---|---|---|---|---|
| NightZenith | #121417 | 0.0060, 0.0070, 0.0086 | 19.8 | 26.2 | 216 / 0.22 | #0A0C2A, 13.7, 236 | Sky gradient above 35 deg (seen only through fog and in the gaps of the deck); a cool, LED-lit air above the warm cloud gives the warm/cool split. |
| NightMid | #1A1D21 | 0.0103, 0.0123, 0.0152 | 28.7 | 33.5 | 214 / 0.21 | #1C1244, 23.7, 252 | Sky at 35 deg and the ambient sky colour (x 2.1): cool neutral, so up-facing surfaces go blue-grey, not violet. |
| HazeHorizon | #2E2925 | 0.0273, 0.0222, 0.0185 | 41.8 | 45.2 | 27 / 0.20 | #5B2F78, 61.6, 276 | The horizon glow band and the fog colour toward the horizon: kilometres of rain haze lit by warm street level; dusty umber. |
| CloudBase | #252422 | 0.0185, 0.0176, 0.0160 | 36.1 | 40.0 | 40 / 0.08 | #7A2A72, 64.2, 306 | Underside of the deck away from hot spots: the city's mixed light reflected by cloud, a near-neutral grey. Lifted so unresolved towers (luma 33-35) read darker than the sky behind them; the fill colour of beats 3-5. |
| CloudHot | #4D4034 | 0.0742, 0.0513, 0.0343 | 65.9 | 68.0 | 29 / 0.32 | #B03A8C, 89.0, 318 | Deck above warm-lit districts (boulevard axis): the luminous lid of KS04-KS06, the brightest large area of the film; also the alley mouth light and the fill colour of beats 1-2. |
| AbyssNear | #24211F | 0.0176, 0.0152, 0.0137 | 33.5 | 37.8 | 24 / 0.14 | #4A2A70, 53.9, 267 | Haze just below the horizon: the darker band between the horizon glow and the glowing depths. |
| AbyssGlow | #4F3D2E | 0.0782, 0.0467, 0.0273 | 63.7 | 66.0 | 27 / 0.42 | #5A2C5E, 57.4, 295 | Haze toward the nadir: fog lit from beneath by the lower city, amber-umber like a fog layer seen from a tower top. At the Edge the brightest large area is below the eye (nadir 66 against a sky of 40). |
| Floor | #0A0C0F | 0.0030, 0.0037, 0.0048 | 11.8 | 11.8 | 216 / 0.33 | #0B0820, 10.4, 247 | Darkest allowed colour: blue-black (B - R +5 codes), a complementary ground for the warm practicals. Luma 11.8 keeps the dithered 1st percentile at 9.9 (>= 9) because exposure and vignette are now relative to it (section 7). |
| NeonMagenta | #FF3366 | 1.0000, 0.0331, 0.1329 | 98.1 | - | 345 / 0.80 | #FF2FA3, 99.6, 327 | Rose-red Hong Kong neon, moved out of the violet band (displays at hue 349-350). Small signs only (4 %), at most one alley sign, 0.5 % of windows; never a blade, an edge light, a light that washes a wall, a sky, fog or screen colour. |
| NeonCyan | #1FE6FF | 0.0137, 0.7913, 1.0000 | 189.5 | - | 187 / 0.88 | kept | The cool accent: 18 % of small signs, tube strips, one blade in six, vending; held back in beats 1-4 so the koi's aqua and the spire are the cool payoff. |
| SodiumAmber | #FFA042 | 1.0000, 0.3515, 0.0545 | 173.4 | - | 30 / 0.74 | #FFA42E, 174.8, 33 | High-pressure sodium (2 050 K at the 4 300 K white balance): amber signs, mechanical-band slits, a few surviving old fixtures. Every city-wide use is ramped to 0 by s 1 250 (R6). |
| NeonViolet | #8A5CFF | 0.2542, 0.1070, 1.0000 | 113.5 | - | 257 / 0.64 | retired | Kept in the enum so indices stay stable; weight 0 everywhere (signs, blades, bands, screen, edge lights, windows, lights). |
| SignWhite | #DBEAFF | 0.7084, 0.8228, 1.0000 | 232.3 | - | 215 / 0.14 | #E8F0FF, 239.4, 219 | 5 000 K cool-white LED: train headlights, promenade, balustrade and plaza lamps, cool-white signs and blades, the "ice" stop of the screen wall, the replacement of every amber in beat 7. |
| AviationRed | #FF2A22 | 1.0000, 0.0232, 0.0160 | 86.7 | - | 2 / 0.87 | kept | Beacons (0.2 Hz), tail lights, red small signs (14 %); never a blade; saturated red < 1 % of any frame (D-010). |
| WinCold | #C5DDFF | 0.5583, 0.7231, 1.0000 | 218.4 | - | 215 / 0.23 | #BFD8FF, 213.5, 216 | 5 600 K cool-white window (LED / fluorescent office and flat). |
| WinWarm | #FFCE96 | 1.0000, 0.6172, 0.3050 | 212.4 | - | 32 / 0.41 | #FFC470, 202.5, 35 | 2 900 K warm window and the noodle bar: a homely warm, less orange than v1. |
| GoalLamp | #FFB866 | 1.0000, 0.4793, 0.1329 | 193.2 | - | 32 / 0.60 | kept | The goal window (about 2 450 K, HDR x3.5): the warmest, most saturated light of beat 7, by construction the only one. |
| WetAsphalt | #3D3C3A | 0.0467, 0.0452, 0.0423 | 60.1 | albedo | 40 / 0.05 | #14121E, 19.3, 250 | TRUE albedo: the graded mean of the photo asphalt, already wet (dry 0.07-0.10, wet 0.04-0.05). |
| ConcreteDark | #74736F | 0.1746, 0.1714, 0.1590 | 114.9 | albedo | 48 / 0.04 | #2A2A38, 43.0, 240 | TRUE albedo: weathered, grimy urban concrete (real 0.20-0.35, kept just under it); tower walls, alley base, slabs. |
| Moss | #3F5230 | 0.0497, 0.0844, 0.0296 | 75.5 | albedo | 94 / 0.41 | #1D3A26, 50.4, 138 | TRUE albedo of wet moss (real 0.05-0.12); yellow-green, not mint. |
| Bark | #4B423C | 0.0704, 0.0545, 0.0452 | 67.5 | albedo | 24 / 0.20 | #2A1F1A, 33.0, 18 | TRUE albedo of wet bark (real 0.05-0.15). |
| LeafGlow | #7FE0B0 | 0.2122, 0.7454, 0.4342 | 199.9 | - | 150 / 0.43 | #5CFFA0, 213.5, 145 | Bioluminescent specks and vein glints only (pale jade); no longer the colour of whole crowns. |
| LeafWarm | #D2E68A | 0.6445, 0.7913, 0.2542 | 219.1 | - | 73 / 0.40 | #D9FF7A, 237.3, 77 | Lantern-lit leaf tips (translucency highlight), small. |
| Lantern | #FFB35C | 1.0000, 0.4508, 0.1070 | 188.9 | - | 32 / 0.64 | kept | Paper lanterns (about 2 350 K, emissive); the light they cast is lerp(Lantern, WinNeutral, 0.4) (R4). |
| KoiBody | #9EE0EE | 0.3419, 0.7454, 0.8550 | 211.0 | - | 190 / 0.34 | #BFEFFF, 230.0, 195 | Pale aqua hologram body; body core displays 130-175 with both shells (section 6 ladder). |
| KoiPatch | #FF6A2A | 1.0000, 0.1441, 0.0232 | 133.1 | - | 18 / 0.84 | #FF5CA8, 132.1, 332 | Kohaku vermilion patches, on both shells, displaying hue 15 / sat 0.75 over the cool deck: the most saturated colour of the film. |
| KoiGlow | #5CDCFF | 0.1070, 0.7157, 1.0000 | 195.3 | - | 193 / 0.64 | #7FE6FF, 209.9, 191 | Rim and analytic koi light: a cooler, more saturated cyan than the body, so the silhouette separates from the cloud. |
| TitleText | #F3EFE6 | 0.8963, 0.8632, 0.7913 | 239.2 | - | 42 / 0.05 | kept | UI. |

### 2.2 New tokens (appended after TitleText so existing enum indices stay stable)

| Token | Hex | Linear | Luma | Disp. | Hue / sat | Role and reason |
|---|---|---|---|---|---|---|
| CloudCool | #2A3036 | 0.0232, 0.0296, 0.0369 | 47.2 | 50.2 | 210 / 0.22 | Deck above cold-lit districts: spire, koi orbit, sign wall; the fill colour of beats 6-7. Makes beat 7's sky cool so the warm window stays unique. |
| HazeLow | #3B3129 | 0.0437, 0.0307, 0.0222 | 50.5 | 53.4 | 27 / 0.31 | Warm low haze: the in-scatter colour of the alley haze layer (D-006) lit by bar and door light; the mouth glow. |
| LampWarm | #FFDBAF | 1.0000, 0.7084, 0.4287 | 223.5 | - | 33 / 0.31 | 3 200 K LED street lamp: boulevard median, tongue, bridge, alley bulkheads. |
| WinTungsten | #FFC07B | 1.0000, 0.5271, 0.1981 | 200.4 | - | 31 / 0.52 | 2 600 K incandescent / halogen window (one third of the warm share); one blade in six. |
| WinNeutral | #FFF6EB | 1.0000, 0.9216, 0.8308 | 247.1 | - | 33 / 0.08 | 4 000 K neutral window (offices, modern flats); the neutral-white sign and blade colour; the target of every "desaturate" rule (R4). |
| WinBlue | #8EBAFF | 0.2705, 0.4910, 1.0000 | 181.6 | - | 217 / 0.44 | 9 000 K "screen-lit room" window (static, never flickering), 3 % of lit windows. |
| ScreenTeal | #0E8C8C | 0.0044, 0.2623, 0.2623 | 113.2 | - | 180 / 0.90 | Deep teal stop of the screen-wall waves (desaturated in the ramp: NocScreen, section 10). |
| ScreenBase | #101416 | 0.0052, 0.0070, 0.0080 | 19.3 | 25.8 | 200 / 0.27 | The LED wall's off-state / panel substrate; replaces NeonViolet as the mix target of `ScreenMix`. |
| Plaster | #8B867E | 0.2582, 0.2384, 0.2086 | 134.5 | albedo | 37 / 0.09 | TRUE albedo: grimy painted render of tenements and alley walls (real 0.3-0.6, dirty). |
| Brick | #6C4D3F | 0.1500, 0.0742, 0.0497 | 82.6 | albedo | 19 / 0.42 | TRUE albedo: old red-brown brick (physicallybased.info brick 0.262, 0.095, 0.061, darkened by soot). |
| Granite | #817E7C | 0.2195, 0.2086, 0.2016 | 126.5 | albedo | 24 / 0.04 | TRUE albedo: flamed grey granite (plaza, stone cladding, pavers; real 0.15-0.35). |
| MetalPaint | #3F4042 | 0.0497, 0.0513, 0.0545 | 63.9 | albedo | 220 / 0.05 | TRUE albedo: dark graphite paint on steel (rails, posts, decks, frames; real 0.03-0.10). |
| SteelF0 | #C0BEBB | 0.5271, 0.5149, 0.4969 | 190.2 | albedo | 36 / 0.03 | Metal F0 of iron / steel (physicallybased.info iron 0.530, 0.513, 0.494); replaces SignWhite x 0.55. |
| Leaf | #3C4B30 | 0.0452, 0.0704, 0.0296 | 69.9 | albedo | 93 / 0.36 | TRUE albedo of wet leaves (real 0.05-0.10). Replaces Moss x 0.6 on crowns. |
| GlassDark | #242628 | 0.0176, 0.0194, 0.0212 | 37.7 | albedo | 210 / 0.10 | Unlit room behind glass / tube glass tint (an albedo-like interior term, section 4). |

### 2.3 Ready-to-paste C# (order = enum order)

```csharp
public enum NocPal
{
    NightZenith, NightMid, HazeHorizon, CloudBase, CloudHot, AbyssNear, AbyssGlow, Floor,
    NeonMagenta, NeonCyan, SodiumAmber, NeonViolet, SignWhite, AviationRed, WinCold, WinWarm, GoalLamp,
    WetAsphalt, ConcreteDark, Moss, Bark, LeafGlow, LeafWarm, Lantern, KoiBody, KoiPatch, KoiGlow, TitleText,
    CloudCool, HazeLow, LampWarm, WinTungsten, WinNeutral, WinBlue, ScreenTeal, ScreenBase,
    Plaster, Brick, Granite, MetalPaint, SteelF0, Leaf, GlassDark,
}
// Table
            (NocPal.NightZenith, "#121417"), (NocPal.NightMid, "#1A1D21"), (NocPal.HazeHorizon, "#2E2925"), (NocPal.CloudBase, "#252422"),
            (NocPal.CloudHot, "#4D4034"), (NocPal.AbyssNear, "#24211F"), (NocPal.AbyssGlow, "#4F3D2E"), (NocPal.Floor, "#0A0C0F"),
            (NocPal.NeonMagenta, "#FF3366"), (NocPal.NeonCyan, "#1FE6FF"), (NocPal.SodiumAmber, "#FFA042"), (NocPal.NeonViolet, "#8A5CFF"),
            (NocPal.SignWhite, "#DBEAFF"), (NocPal.AviationRed, "#FF2A22"), (NocPal.WinCold, "#C5DDFF"), (NocPal.WinWarm, "#FFCE96"),
            (NocPal.GoalLamp, "#FFB866"), (NocPal.WetAsphalt, "#3D3C3A"), (NocPal.ConcreteDark, "#74736F"), (NocPal.Moss, "#3F5230"),
            (NocPal.Bark, "#4B423C"), (NocPal.LeafGlow, "#7FE0B0"), (NocPal.LeafWarm, "#D2E68A"), (NocPal.Lantern, "#FFB35C"),
            (NocPal.KoiBody, "#9EE0EE"), (NocPal.KoiPatch, "#FF6A2A"), (NocPal.KoiGlow, "#5CDCFF"), (NocPal.TitleText, "#F3EFE6"),
            (NocPal.CloudCool, "#2A3036"), (NocPal.HazeLow, "#3B3129"), (NocPal.LampWarm, "#FFDBAF"), (NocPal.WinTungsten, "#FFC07B"),
            (NocPal.WinNeutral, "#FFF6EB"), (NocPal.WinBlue, "#8EBAFF"), (NocPal.ScreenTeal, "#0E8C8C"), (NocPal.ScreenBase, "#101416"),
            (NocPal.Plaster, "#8B867E"), (NocPal.Brick, "#6C4D3F"), (NocPal.Granite, "#817E7C"), (NocPal.MetalPaint, "#3F4042"),
            (NocPal.SteelF0, "#C0BEBB"), (NocPal.Leaf, "#3C4B30"), (NocPal.GlassDark, "#242628"),
// SpecLinear (4 decimals, the exact EOTF of the hex)
            { 0.0060, 0.0070, 0.0086 }, { 0.0103, 0.0123, 0.0152 }, { 0.0273, 0.0222, 0.0185 }, { 0.0185, 0.0176, 0.0160 },
            { 0.0742, 0.0513, 0.0343 }, { 0.0176, 0.0152, 0.0137 }, { 0.0782, 0.0467, 0.0273 }, { 0.0030, 0.0037, 0.0048 },
            { 1.0000, 0.0331, 0.1329 }, { 0.0137, 0.7913, 1.0000 }, { 1.0000, 0.3515, 0.0545 }, { 0.2542, 0.1070, 1.0000 },
            { 0.7084, 0.8228, 1.0000 }, { 1.0000, 0.0232, 0.0160 }, { 0.5583, 0.7231, 1.0000 }, { 1.0000, 0.6172, 0.3050 },
            { 1.0000, 0.4793, 0.1329 }, { 0.0467, 0.0452, 0.0423 }, { 0.1746, 0.1714, 0.1590 }, { 0.0497, 0.0844, 0.0296 },
            { 0.0704, 0.0545, 0.0452 }, { 0.2122, 0.7454, 0.4342 }, { 0.6445, 0.7913, 0.2542 }, { 1.0000, 0.4508, 0.1070 },
            { 0.3419, 0.7454, 0.8550 }, { 1.0000, 0.1441, 0.0232 }, { 0.1070, 0.7157, 1.0000 }, { 0.8963, 0.8632, 0.7913 },
            { 0.0232, 0.0296, 0.0369 }, { 0.0437, 0.0307, 0.0222 }, { 1.0000, 0.7084, 0.4287 }, { 1.0000, 0.5271, 0.1981 },
            { 1.0000, 0.9216, 0.8308 }, { 0.2705, 0.4910, 1.0000 }, { 0.0044, 0.2623, 0.2623 }, { 0.0052, 0.0070, 0.0080 },
            { 0.2582, 0.2384, 0.2086 }, { 0.1500, 0.0742, 0.0497 }, { 0.2195, 0.2086, 0.2016 }, { 0.0497, 0.0513, 0.0545 },
            { 0.5271, 0.5149, 0.4969 }, { 0.0452, 0.0704, 0.0296 }, { 0.0176, 0.0194, 0.0212 },
```
`NocturneConstants`: `FloorLinR = 0.00304f, FloorLinG = 0.00368f, FloorLinB = 0.00478f; FloorLuma = 11.8` (exact: 0.003035,
0.003677, 0.004777; codes 10, 12, 15; luma 11.79). The crush threshold is FloorLuma + 3 = 14.8.

**HDR emission factors** (`NocturnePalette` constants): EmissionTube 2.5 -> **2.0**; EmissionSign 1.8 -> **1.6** (also
`NeonGain`); EmissionWindow 1.2 -> **replaced** by the per-token luminance normalisation of section 4; EmissionGoalLamp **3.5**
(kept); EmissionKoiBody 0.9 -> **0.55**; EmissionKoiRim 2.2 -> **1.6**. Rule of 14.2.1 kept: no core above 3.0 except the 6 px
goal lamp.

**Displayed accent cores** (EV 0, saturation +4, Neutral curve with desaturation 0.15, which stays: cores whiten slightly
toward the photographic look while their halos keep the authored saturation; no test pins it):

| Core | Displayed hue / sat / luma | Sat if desaturation were 0.05 |
|---|---|---|
| NeonCyan x1.6 / x2.0 | 188 / 0.67 / 198; 189 / 0.59 / 204 | 0.80 / 0.75 |
| NeonMagenta x1.6 / x2.0 | 349 / 0.62 / 129; 350 / 0.56 / 142 | 0.73 / 0.69 |
| SodiumAmber x1.6 / x2.0 | 27 / 0.61 / 179; 26 / 0.55 / 185 | 0.70 / 0.67 |
| AviationRed x1.6 | 1 / 0.65 / 124 | 0.77 |
| KoiGlow rim x1.6 | 194 / 0.55 / 197 | 0.62 |
| GoalLamp x3.5 (the 6 px core) | 28 / 0.37 / 211 | 0.50 |
| LampWarm x1.6, SignWhite x1.6 | 33 / 0.29 / 221; 215 / 0.13 / 228 | 0.31; 0.14 |
| KoiPatch x0.55, KoiBody x0.55 (below 0.76: identity) | 18 / 0.86 / 101; 190 / 0.35 / 162 | same |

### 2.4 Colour-use rules (measurable; checked in section 8)
- **R1 Violet / magenta.** Pixels with hue 250-340 deg and HSV saturation > 0.35: **R1a** air and surfaces (luma 16-119)
  <= 1.0 % of any frame; **R1b** accents (luma >= 120) <= 1.5 %. (The window is 250-340 because the grade moves magenta cores
  to 331-335 and violet to 254-255; NeonMagenta now sits at 345-350 and NeonViolet is retired.)
- **R2 Large areas.** Any 4-connected region of 32 x 32 px block means (luma >= 16) larger than 5 % of the frame has HSV
  saturation <= 0.65 if its hue is 10-70 (warm light on matter: lamp pools on brick 0.57-0.59, window light on brick 0.61-0.65,
  lantern light on leaves 0.56-0.65), <= 0.45 at any other hue, and, if its saturation is > 0.20, a hue outside 250-340.
  Exceptions: beat 6 (the screen wall) <= 0.60 at any hue outside 250-340; beat 7, the koi (hue 160-220), <= 0.60. Blocks in
  which >= 5 % of the pixels have luma >= 160 (sign strokes, lamp heads) are excluded: signs obey the content rules instead.
- **R3 Mid-tones.** Mean HSV saturation of 4 x 4 block means with luma 16-159 <= 0.35 in beats 1 and 5 (warm by design),
  <= 0.40 in beat 6 (the wall fills 55 % of KS16 at saturation up to 0.55), <= 0.30 elsewhere (v1: 0.53-0.73). Block means
  remove the chroma-dither bias (neutral greys measure 0.04-0.09 per pixel at luma 12-25).
- **R4 Lights that wash surfaces** (Unity lights with range > 10 m, wall-plane sources of H4): saturation <= 0.45 measured as
  the HSV saturation of the sRGB light colour (the palette-table convention; LampWarm is 0.31 there, 0.57 in linear). Use
  LampWarm, WinWarm, WinNeutral, SignWhite, or an accent desaturated 40 %, defined as `lerp(c, WinNeutral, 0.4)` in linear
  (cyan 0.31, magenta 0.34, red 0.38, amber 0.36, teal 0.11, WinTungsten 0.30). Explicit exceptions: the goal lamp (GoalLamp,
  0.60) and the analytic koi light (KoiGlow, 0.64). Full-saturation tokens are for emissive surfaces only (tubes, strokes,
  lamp heads). An EditMode test walks the built scene's lights.
- **R5 Albedo grading.** No photo texture may make the product token x 2 x variation carry a hue of 260-330 with
  saturation > 0.15 in sRGB (section 5.4).
- **R6 Amber and warm discipline.** Every city-wide SodiumAmber use (small signs, edge lights, mechanical slits, streak tails)
  is multiplied by k_a(s) = w_f(s) / 0.32 (w_f of section 4: 1 to s 400, 0.625 at s 880, 0 from s 1 250) and the remainder goes
  to SignWhite; the screen wall's amber stop turns to SignWhite over s 1 230-1 290. Beat 7 has no amber and no warm light
  except the goal window.
- **R7 Violet retired.** NeonViolet has weight 0 in every list, cycle, band and light.

---------------------------------------------------------------------------------------------------------------------------

## 3. Atmosphere (14.2.2)

### 3.1 Sky stops, deck and fog
- **Gradient (unchanged code, new tokens):** e >= 0: HazeHorizon (0) -> NightMid (35) -> NightZenith (90), smoothstep in linear.
  e < 0: HazeHorizon blends to lerp(AbyssNear, AbyssGlow, smoothstep(0, 90, -e)) by smootherstep over 0..-6 deg (D-006), times
  the H3 sheet modulation, now **+15 %** (`NC_SheetSkyAmp` 0.10 -> 0.15, about +-5 codes of visible bands) at -5.7, -15.6 and
  -31 deg. Luma of the gradient alone at EV 0: e 90 26.2, e 35 33.5, e 10 43.2, e 0 45.2, e -6 40.9, e -30 50.1, e -60 60.1,
  e -90 66.0 (monotone above the horizon; the band just below it is the darker gap between the horizon glow and the lit depths).
- **Cloud deck (y 460), new colour model** (replaces `CloudBase x (1 + 0.8 glow)`, NocAtmosphere.hlsl:117-151 and its C# twin):
  ```
  n      = NocCloudNoise(xz, blurM)          // 0..1, mean 0.5: octaves 900 / 300 / 110 / 40 m, weights 0.36 / 0.19 / 0.30 / 0.15,
                                             // 3 m/s scroll; the 110 and 40 m octaves are domain-warped by 60 m x (2 n900 - 1);
                                             // each octave = lerp(0.5, value, smoothstep(4, 8, wavelength / blurM)) (no shimmer
                                             // below 8 px of wavelength; the 40 m octave is 63 px overhead in KS05)
  base   = PAL_CloudBase * (1 + 1.2 (n - 0.5)) * (1 - 0.35 g_gulf)   // +-13 % std, p1..p99 -30 / +33 %: +-5-6 codes of billows
  g_i    = a_i exp(-0.5 (dist_i / 220)^2)    // unchanged glow centres (segments / points / rings), a_i > 0 only
  W      = 1 - prod(1 - g_i)                 // unchanged soft union, 0..1
  tint   = sum(g_i tint_i) / max(sum(g_i), 1e-4)
  cloud  = lerp(base, tint * (1 + 0.5 (n - 0.5)), W)
  ```
  The four-octave sum has a std of 0.111 (simulated quintic value noise), so the 1.2 amplitude gives the +-5-6 codes of 50-200 px
  billows that keep KS02, KS05, KS15 and KS18 from showing a smooth lid and break H1 plateaus.
  Glow table (`NocCloudGlow.Defaults`, NocSchedule.cs:63-71; add a tint index in `_NocGlowPar.w`):

  | Glow | a | Tint | Why |
  |---|---|---|---|
  | boulevard axis, north leg | 0.85 | CloudHot | warm street level of the canyon below |
  | boulevard axis, east leg | 0.95 | CloudHot | idem; the luminous lid over KS04-KS06 |
  | screen wall face x 874 | 0.60 | CloudCool | a cold LED wall below; the city starts cooling |
  | spire | 0.90 | CloudCool | cyan-white spire strips below |
  | koi orbit R 260 | 0.55 | CloudCool | the koi's light |
  | (optional, off by default) Gulf shadow: segment (515, -800)-(515, 1 200), sigma 220 | g_gulf, a 1.0 | - | a separate multiplier `base x (1 - 0.35 g_gulf)` (NocCloudGlowG saturates its terms to >= 0, so it cannot be a negative glow); enable only if KS07 and KS08 keep the separation check (>= 6 codes) |

  Never add a warm glow near the Old Quarter (x 1 100-1 200, z 340-440): beat 7's ceiling must stay cool.
- **Horizon band.** HazeHorizon at e 0, fading over 0-35 deg; the deck fades in over e 3-5 (unchanged). Result: a warm band
  0-10 deg between tower silhouettes (luma 45-47 at EV 0 to +0.1), 11-13 codes above the unresolved towers (section 4).
- **Fog colour logic.** `fogColour = NocSkyColor(direction)` (unchanged), so fogged towers dissolve into the horizon haze (low)
  or the cloud (high) and the horizon always agrees. With towers now darker than the sky, distance lightens them: real aerial
  perspective. The warm low haze of the alley (D-006) is added inside the sky function itself: `NocSkyColorBlur` mixes
  `c = lerp(c, PAL_HazeLow, 0.6 x sa m / (sr + sa m))`, with sr = `_NocFog.x`, sa = `_NocFog.y` and m = `NocCapMean(y_cam, y_end)`
  along the ray to min(6 500 m, the cloud hit) (`y_end = y_cam + min(6500 sin e, 460 - y_cam)`; m = 1 for e <= 0). The path
  length cancels, so the share is continuous across the cloud fade, the dome and every fog colour carry the same value, and
  `NocFogT3` keeps its signature. In the alley the share is 0.55 at eye level and 0.20 toward the zenith (the KS02 slit gains
  +3 codes); it vanishes with the layer at s 124.
- **Fog sigma.** Beats 2-4 3.6e-4 -> **4.4e-4** (and `RegionalSigma`, so the alley total stays 5.0e-3 with an extra of 4.56e-3):
  d50 2 313 -> 1 892 m; T at 750 m 0.93 -> 0.90, at 1 km 0.88 -> 0.82, at 2 km 0.60 -> 0.46. Near, middle and far layers now
  separate (H3 only becomes safer). Beats 5-7 keep 6.0e-4, 4.5e-4, 3.4e-4; the alley 5.0e-3.
- **Cloud mix of shaft tops** (smoothstep(380, 560, y) toward the cloud colour): unchanged, now toward a grey cloud.

### 3.2 Target display luma (Rec.709 codes, Floor, beat EV and saturation of section 7, sigma above, cloud noise at its mean)

| Beat (EV) | e 90 = deck | e 60 | e 35 | e 20 | e 10 | e 5 | e 0 | e -6 | e -30 | e -60 | e -90 | hot core a 0.95 / token | cool core (token) |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| 1 Alley (-0.1) | 41.0 | 41.1 | 40.7 | 41.2 | 43.9 | 45.1 | 48.3 | - | - | - | - | 60.4 / 61.2 | 48.1 |
| 2 Boulevard (+0.1) | 40.9 | 40.9 | 40.6 | 40.9 | 43.8 | 46.1 | 46.7 | 42.3 | 51.7 | 62.1 | 68.1 | 68.0 / 69.1 | 51.1 |
| 3 Edge (0.0) | 39.6 | 39.6 | 39.3 | 39.6 | 42.4 | 44.7 | 45.2 | 40.9 | 50.1 | 60.1 | 65.9 | 65.8 / 66.9 | 49.4 |
| 4 Skybridge (0.0) | 39.6 | 39.6 | 39.3 | 39.6 | 42.4 | 44.7 | 45.2 | 40.9 | 50.1 | 60.1 | 66.0 | 65.8 / 66.9 | 49.5 |
| 5 Garden (-0.2) | 36.8 | 36.8 | 36.4 | 36.9 | 40.3 | 41.9 | 42.4 | 38.3 | 46.9 | 56.3 | 61.8 | 60.8 / 61.8 | 45.7 |
| 6 Signs (-0.1) | 38.3 | 38.3 | 38.1 | 38.4 | 41.1 | 43.2 | 43.8 | 39.6 | 48.4 | 58.1 | 63.8 | 63.6 / 64.7 | 47.8 |
| 7 Heart (0.0) | 39.8 | 39.8 | 39.6 | 39.8 | 41.8 | 44.5 | 45.2 | 40.9 | 50.0 | 60.1 | 65.9 | 66.2 / 67.3 | 49.7 |

Beat 1 includes the HazeLow share of the alley layer (the alley never sees below the horizon). Abyss profile of beat 3:
-6 40.9, -10 41.2, -15.6 43.7, -30 50.1, -45 54.0, -60 60.1, -75 64.3, -90 65.9: the nadir is 26 codes above the sky (40)
and the band under the horizon. Tolerances: +-3 codes on the means; the deck noise adds +-5-6 codes (p1..p99) of structure.
v1 at the same points (beat 2): 70 / 69 / 67 / 69 / 65 / 65 codes with Cb +25..+38 and Cr +16..+43 codes; v2 beat 2: Cb / Cr
-0.3 / +0.1 at the zenith, -0.1 / 0.0 at e 35, -0.9 / +1.3 at e 10, -1.9 / +2.3 at the horizon, -4.0 / +4.8 at e -30,
-9.3 / +9.8 at the nadir, -7.3 / +7.1 on the hot core (HSV 28 / 0.30). The sky is near-neutral above the horizon and warm
only where the city lights it.

**Floor.** `#0A0C0F`, luma 11.8, blue-black; `NocFloor` unchanged in form. Exposure and vignette are applied relative to the
Floor (section 7), so a Floor pixel displays as the Floor at every EV and in every corner, and the dithered 1st percentile
of a Floor plateau is 9.9 (simulated with the 2-code luma TPDF and the 1-code chroma TPDF): the 14.10 gate (>= 9) holds.

---------------------------------------------------------------------------------------------------------------------------

## 4. Window light (NocFacade, 14.7 H2; mechanisms unchanged, appearance new)

**Lit fraction.** Overall **0.18** (v1 0.34): residential strata 0.194, commercial 0.124, mechanical 0.041, sky lobby 0.416 -
as lit factors relative to 0.18: 1.08 / 0.69 / 0.23 / 2.31 (shares 70/15/10/5 % unchanged, sum of share x factor 1.00). Zone
clustering stays (24 columns x 2 floors) with a wider zone spread `0.40 + 1.2 U` (whole lit floors and dark floors read more
clearly; mean 1.0).

**Glass coverage.** Residential opening 1.6 x 2.2 m in the 2.4 x 3.6 m bay (coverage 0.407, v1 0.56): towers read as walls
with windows, not as glass grids. Commercial 0.679, mechanical 0.121, lobby 0.787 (unchanged); the share-weighted opening
cover is 0.4385. A 0.12 m MetalPaint frame and mullion occlude 24 % of each opening by exact filtered coverage (glass fraction
0.757), so their share is the same at every distance.

**Hue mix of lit windows** (one hash per window, class boundaries moving with s get the existing 0.03 soft band, so nothing
pops):

| Class | Token | Share | Note |
|---|---|---|---|
| warm, tungsten 2 600 K | WinTungsten | w_f / 3 | deep amber flats |
| warm 2 900 K | WinWarm | 2 w_f / 3 | homes |
| neutral 4 000 K | WinNeutral | n_f | offices, modern flats |
| cool 5 600 K | WinCold | 1 - w_f - n_f - 0.07 | LED / fluorescent |
| screen-lit 9 000 K | WinBlue | 0.03 | static, x0.6 brightness |
| coloured | NeonCyan 0.015, AviationRed 0.010, LeafGlow 0.010, NeonMagenta 0.005 | 0.04 | each `lerp(c, WinNeutral, 0.4)`, x0.8 |

Schedules (piecewise linear in s, replacing `WarmFraction` per beat and the cold/cyan/magenta/white thresholds 0.82/0.90/0.96):
w_f = 0.32 at s 0-400, 0.20 at s 880, 0.00 at s 1 250 and after; n_f = 0.30 to s 880, 0.15 at s 1 250 and after. The city
cools across beats 5-6, so at the Heart every one of the ten thousand windows is white or cold and the goal window is the only
warm one. Peak rate 0.054 per 100 m = 0.14 % of lit windows per second at 2.5 m/s (H2 allows 0.3 %/s).

**Brightness.** Each token is normalised to luminance: `E = token x (Y_med / Y(token)) x b x I / E[I]`, **Y_med = 0.12**
linear, b = exp(0.45 N(0,1)) clamped to [0.40, 2.40] per window (log-normal, mean 1.095). Normalisation factors Y_med / Y:
WinTungsten 0.199, WinWarm 0.177, WinNeutral 0.129, WinCold 0.169, WinBlue 0.250 (then x0.6), NeonCyan 0.187. Display at
EV +0.1: median lit glass luma 100 (99.8-101.6 across classes), b 0.4 -> 65, b 2.4 -> 150. The strongest channel of the
brightest window is linear 0.51 after exposure (WinTungsten at b 2.4, x2^0.1), under the bloom knee start of 0.675 (section 7):
windows never bloom; lamps and signs do.

**Interior variation (D-004, energy-neutral).** I = vertical falloff (1.0 at the head to 0.7 at the sill) x curtains (35 % of
lit windows, x0.45 and tint lerp(hue, WinWarm, 0.25)) x blinds (15 %, 8 cm slats, faded below a 3 px slat pitch) x interior
mapping where available. I is divided by its expectation E[I] (falloff 0.85 x curtains 0.8075 x blinds), so the glass mean is
Y_med x b at every footprint; only the zero-mean part I / E[I] - 1 fades in over smoothstep(4, 8 px) of window height, which
keeps FacadeTests.WForced0And1 (w forced to 0 and 1 within 3 %) and removes the 31 % pop of the draft.

**Unlit glass.** Dark, reflecting the sky and the city: `F0 0.04` Schlick x `NocSkyColorBlur(r, 0.08)` with a per-pane normal
jitter of +-1.5 deg (neighbouring panes reflect slightly different cloud, so glass reads as panes, not a mirror sheet) plus a
dim interior `GlassDark x ambient_eq + WinNeutral x 0.0005 x (0.5..1.5 hash)` (stand-by light, corridors). Display (beat 2,
EV +0.1): luma 16.3 facing the camera, 17.6 at 60 deg and 24.3 at 75 deg incidence (looking up a tower): 6 codes below the
shade-side wall face-on, a real sheen above it at grazing. With 82 % of windows dark, the punched-window grid reads everywhere.

**Unresolved facade mean** (the single city-mean number for FacadeTests and TEXTURE_PLAN E7; frontal, ambient-only, beat 2,
LightCal 1.0, unfogged): lit opening area 0.0850 (= 0.1227 x 0.18 / 0.26) x glass fraction 0.757 x Y_med 0.12 x E[b] 1.095 =
0.00845, + wall 0.00240 (0.5615 x (0.1712 x 0.96 x 0.0223 + sky reflection 0.0006)), + unlit glass and frames 0.00054 =
**0.0114 linear (+-3 %)**, luma 32.7 at EV 0 and 33.7 at EV +0.1 (fill-side faces +0.0010, luma 35.0). That is 7 codes under
the deck base (40.9), 13 under the horizon band (46.7) and 34 under the hot core: towers stand dark against the sky, and fog
lightens them with distance (face B at 750 m 35.3, 1 km 36.3, 2 km 41.1 against a horizon sky of 46.4).

**Structural contrast of tower faces** (beat 2, EV +0.1, against the shade-side wall at 22.3):

| Element | Contrast | How |
|---|---|---|
| Slab edge, sill, ledge (up-facing) | +8 codes (30.0) | sky ambient + the fill at NdotL 0.87 |
| Window reveal | -5 codes (17.6) | AO 0.5 in the 0.25 m reveal |
| Unlit glass | -6 codes (16.3) | stand-by x0.0005 |
| Per-panel albedo | +-12 % (+-0.8 codes ambient-only, +-1.5-4 codes in lamp pools) | panel-id hash in the facade skin |
| Sill streak | -20 % albedo (-1.5 codes) | the `_Streaks` macro under every sill |

**Edge lights** (12 % of towers; remap in `NocFacadeEdgeColour` only, CityField (frozen) keeps its four kinds): kind 1
SignWhite; kind 2 NeonCyan if a hash < 0.6 else SignWhite; kind 3 SodiumAmber x k_a(s) else SignWhite; kind 4 SodiumAmber if
a hash < 0.2 x k_a(s) else SignWhite (= white 55 %, amber 30 %, cyan 15 % before beat 5; no magenta, no violet; white 85 % and
cyan 15 % from s 1 250).

---------------------------------------------------------------------------------------------------------------------------

## 5. Photo-textured surfaces at night (CC0 sets from ambientCG / Poly Haven, docs/TEXTURES.md)

### 5.1 Albedo targets (linear; the token is the mean colour, the map a variation around 0.5)

| Material | Real dry albedo | v2 token / mean | Contrast std / mean (variation-map std = ratio x 0.5) | Wet albedo factor | Roughness dry -> wet (GGX perceptual) | Normal strength |
|---|---|---|---|---|---|---|
| Weathered concrete (towers, walls, slabs) | 0.20-0.35 (aged), new 0.40-0.55 | ConcreteDark 0.171 | 0.25-0.40 (map 0.125-0.20) | x0.70 (porosity 0.4) | 0.80 -> 0.55; streak lines 0.35 | 1.0-1.3 |
| Painted render / plaster (tenements, alley) | 0.30-0.60 | Plaster 0.240 | 0.20-0.35 (map 0.10-0.175) | x0.75 | 0.75 -> 0.50 | 0.8 |
| Old brick (alley, Old Quarter) | 0.15-0.30 | Brick 0.089 (0.150, 0.074, 0.050) | 0.35-0.55 (map 0.175-0.275) | x0.60 | 0.85 -> 0.55; mortar 0.9 -> 0.7 | 1.2-1.5 |
| Asphalt (streets) | fresh 0.04-0.05, aged 0.10-0.18 | WetAsphalt 0.045 (already wet) | 0.30-0.45 (map 0.15-0.225) | (in token) | 0.55 dry areas; puddles 0.08 (unchanged) | 0.6 |
| Flamed granite (plaza, pavers, cladding) | 0.15-0.35 | Granite 0.210 | 0.20-0.35 (map 0.10-0.175) | x0.70 | 0.70 -> 0.45; polished slabs 0.35 | 0.5 |
| Painted steel (rails, decks, frames) | 0.03-0.10 | MetalPaint 0.051 | 0.20-0.40 (chips, rust) | x1.0 (non-porous) | 0.60 -> 0.30 (water film) | 0.8 |
| Bare steel / iron (metal) | F0 0.53, 0.51, 0.49 | SteelF0 | rust patches albedo 0.08-0.15 orange-brown, metallic 0 | x1.0 | 0.45 -> 0.30 | 0.7 |
| Glass | F0 0.04 (water 0.02) | GlassDark interior 0.019 | - | - | 0.05 (pane jitter) | - |
| Bark | 0.05-0.15 | Bark 0.057 | 0.40-0.60 (map 0.20-0.30) | x0.65 | 0.85 -> 0.60 | 1.4 |
| Leaves | 0.05-0.10 | Leaf 0.062 | 0.30-0.50 | x0.85 | 0.60 -> 0.35 (wet sheen) | 0.8 |
| Moss | 0.05-0.12 | Moss 0.073 | 0.30-0.50 | x0.70 | 0.90 -> 0.70 | 1.2 |

All map ranges fit inside TEXTURE_PLAN's soft clamp 0.06-0.94. The v1 tokens were 3-10 x darker on purpose (docs/MATERIALS.md);
Look v2 reverses this: albedos are real (a little dirtier), the darkness comes from the lighting, so texture contrast survives
into the frame. LightCal stays 1.0; local light intensities are the calibration knobs (section 6), never darker albedos.

### 5.2 Wetness (rain everywhere)
Lagarde's model: porous dielectrics darken by `lerp(1, 0.2, porosity)` at full wetness and gain gloss
`lerp(1, gloss, lerp(1, factor, 0.5 wet))`; metals do not darken; puddles are flat water (normal toward the vertex normal,
F0 0.02-0.04). Use porosity concrete 0.4, render 0.3, brick 0.5, granite 0.35, asphalt (in token), bark 0.45, moss 0.4, paint 0.
Wet levels: up-facing 1.0; vertical faces 0.6 with drip streaks to 1.0 under ledges (existing `_Streaks`); sheltered (under
arcades, balconies, bridge decks) 0.15. The current `x0.6 soaked` of MATERIALS.md is replaced by these factors.

### 5.3 How much detail must read
Display luma budgets (beat 2, EV +0.1, saturation +4, LightCal 1.0, the ambient and fill of section 6, URP Lambert x 0.96 and
the analytic sky reflection at perceptual roughness 0.65):

| Situation | Mean luma | Visible detail target |
|---|---|---|
| Ambient-only vertical, shade side (equator only) | 21-26 (concrete 22.3, render 25.0, granite 23.9) | +-30 % albedo gives +-2.2 codes: macro stains, panel joints, streaks (1-10 m) must show; micro grain may vanish. Check 7. |
| Vertical, fill side (north wall NdotL 0.41, face B 0.29) | 25-30 (concrete 25.5, render 29.2) | 3-4 codes over the shade side: towers model in volume, warm on the lit side, cool in shadow. |
| Up-facing ledge, sill, slab edge | 30-35 (concrete 30.0, render 34.5) | slab lines and ledges read as light lines. |
| Window reveal (AO 0.5) / unlit glass frontal / glass at 75 deg | 17.6 / 16.3 / 24.3 | openings read darker than the wall; sheen looking up. |
| Ambient-only brick, asphalt, metal, foliage | 16-19 (brick 18.4, asphalt 16.2, metal 16.6, leaf 17.1) | silhouettes and macro only; these materials are read under light. |
| Lamp-lit at 4-10 m (pools, LampWarm i 3.5) | 25-35 (4 m 34.9, 6 m 28.2, 10 m 24.5) | grain and courses: +-4-5 codes; normal-map relief visible. |
| Lamp-lit at 2 m (NdotL 0.15-0.6) | 44-78 | full micro detail, +-6-11 codes; relief doubles NdotL across a 10 deg normal tilt at grazing. |
| Alley walls outside pools (beat 1, EV -0.1, ambient x0.75) | concrete 19.6, render 21.6, brick 16.9 | the pools carry the detail. |
| Alley bulkhead pools (LampWarm i 1.6, 0.25 m off the wall, r 7) | own wall: render 73 at 1 m, 45 at 1.5 m, 34 at 2 m (brick 46 / 29 / 23); facing wall 5 m away: render 26-33, brick 19-22 over +-3 m | >= 40 % of the KS00 and KS02 texture regions at luma >= 25 (check 7). |
| Wet floor reflecting the sky | 17-27 (5 m) -> 23-34 (10 m) -> 30-40 (30 m) -> 33-44 (80 m) | the floor mirrors the lid in the distance; near floor dark with streaks. |
| Fully dark (<= Floor + 3, luma <= 14.8) | - | 25-50 % of a frame by beat (check 6). |

### 5.4 Grading the downloaded textures (neutral, never violet; TEXTURE_PLAN's texprep)
1. Delight: flatten the low frequencies (a 1/4-tile Gaussian) to +-5 % so no baked sun or AO gradient tiles.
2. Normalise each channel of the de-lit map to a mean of exactly 0.5 linear (MaterialTests.AlbedoVariation_MeanIsHalfLinear).
   The token stays the mean colour: albedo = `_BaseColor` (token) x 2 x variation (NocSurface.shader:104).
3. Chroma: judged on the product token x 2 x variation in sRGB: mean HSV saturation <= 0.12 for concrete, render, granite,
   asphalt, metal; <= 0.45 for brick, bark, leaf, moss; any pixel of the product with hue 260-330 and saturation > 0.15 has its
   variation desaturated until it is not (R5). TEXTURE_PLAN's `chromaKeep` 0.8 applies first.
4. Contrast: the std of the variation map is the 5.1 ratio x 0.5 (concrete 0.125-0.20, render 0.10-0.175, brick 0.175-0.275,
   bark 0.20-0.30), compressed or expanded around 0.5 in linear, inside the 0.06-0.94 clamp.
5. Normals: ambientCG `NormalGL` (Unity is Y+), renormalised after mip generation; roughness map -> smoothness = 1 - roughness,
   remapped into the dry ranges of 5.1.
6. Texel density: >= 256 px/m within 5 m of the path (alley walls, rails, deck), 128 px/m to 30 m, towers 32-64 px/m plus a
   macro overlay (stains, rain streaks, panel joints at 1-10 m) and the triplanar / face-uv rules of docs/MATERIALS.md.
7. The token stays the single colour source (docs/MATERIALS.md colour rule).

---------------------------------------------------------------------------------------------------------------------------

## 6. Lighting rig per beat (14.2.3, retuned)

**LightCal is fixed at 1.0** (AtmosphereDriver default). Every budget of this file is at 1.0; local light intensities are the
calibration knobs. The ambient and the sky fill below are display targets: the driver uploads value / LightCal, so they hold
even if LightCal is ever changed.

**Ambient trilight.** Sky = NightMid x 2.1 = (0.0217, 0.0258, 0.0319), luminance 0.0254, cool; equator = (0.0205, 0.0225,
0.0262) linear (#27292D, hue 218, sat 0.13), luminance 0.0223, cool neutral; ground = AbyssGlow x 0.25 = (0.0195, 0.0117,
0.0068), luminance 0.0130, warm (x 0.35 in beats 3-4, luminance 0.0182: warm underlight on soffits and deck undersides above
the abyss). Cool from above and the side, warm from below and from the fill: shadows are blue-grey, lit sides warm, bounce
warm. Garden set (beat 5): sky = NightMid x 1.6, equator = CloudBase x 0.6 + Lantern x 0.010, ground = Moss x 0.12.

**Sky fill (directional, no shadow).** The cloud above as a soft key. Colour = the deck above, normalised to max 1: beats 1-2
CloudHot (1.000, 0.691, 0.463), beats 3-5 CloudBase (1.000, 0.954, 0.865), beats 6-7 CloudCool (0.628, 0.801, 1.000)
(blended across the beat boundaries like the other per-beat values). Pitch -60 deg, yaw 35 deg: it lights the boulevard's
north wall (NdotL 0.41) and face B (0.29), not the south wall; keep this asymmetry, it gives the towers volume and breaks
KS04's mirror symmetry. Up-facing surfaces get NdotL 0.87.

| Beat | Sky fill i | Ambient x | Local lights (Look v2; caps of 14.2.3 count lights enabled at once, nearest first) |
|---|---|---|---|
| 1 Alley | 0.010 (no shadow maps: a strong fill would light the floor as if roofless) | 0.75 (enclosed) | noodle bar WinWarm r 9 i 6; 10 door bulkheads / lit transoms LampWarm r 7 i 1.6, 0.25 m off the wall at 2.6 m, every <= 12 m on alternating walls (grazing on brick, a broad pool on the facing wall); the 4 nearest alley sign lights r 6 i 1.5, `lerp(c, WinNeutral, 0.4)`; 3 vending lerp(NeonCyan, SignWhite, 0.4) r 4 i 1.2; mouth light lerp(HazeLow, CloudHot, 0.5) r 25 i 2.0 (paints the floor warm from s 90). 19 lights (cap 20). |
| 2 Boulevard | 0.035 | 1.00 | 24 median lamps LampWarm r 14 i 3.5 (+ head emission LampWarm x 1.6); 4 nearest blades r 40 i 4, `lerp(c, WinNeutral, 0.4)`; 8 arcade soffit lights WinNeutral r 10 i 2 grazing the columns and podium stone |
| 3 Edge | 0.035 | 1.00 | 14 balustrade lights SignWhite r 10 i 2; tongue lamps LampWarm r 14 i 3.5 |
| 4 Skybridge | 0.030 | 0.90 | 21 lamps LampWarm r 12 i 3.5; 2 train lights SignWhite r 25 i 3 (NeonCyan stays on tube strips only) |
| 5 Garden | 0.015 | garden set x 1.0 | 10 lantern clusters lerp(Lantern, WinNeutral, 0.4) r 14 i 3.5 (sat 0.34); 8 leaf points lerp(LeafGlow, WinNeutral, 0.5) r 30 i 1.0 |
| 6 Signs | 0.025 | 0.90 | 12 wall lights: wave colour `lerp(c, WinNeutral, 0.4)` r 45 i 3.5; promenade lamps SignWhite r 12 i 3 |
| 7 Heart | 0.030 | 0.85 | goal lamp GoalLamp r 14 i 5 (kept; R4 exception); 20 plaza lamps SignWhite r 12 i 2.5; koi light KoiGlow x 0.7 (v1 0.9; R4 exception); the tenement entrance light lerp(WinCold, WinNeutral, 0.5) r 8 i 1.5, 2.8 m high, 0.4 m off the wall, >= 4 m from the goal window (grazes KS19's lower-left texture region: a warm window on a cool wall) |

**Calibration targets (LightCal 1.0).** Concrete 2 m from a LampWarm i 3.5 lamp at NdotL 0.6 displays luma 78 (75-85);
ambient-only shade-side concrete 22 (beat 2). The H4 wall-plane macros that were `ConcreteDark x factor` or `WinCold x factor`
as RADIANCE must be re-derived as `albedo x 0.96 x ambient_eq luminance (0.0223)` plus the window mean (NocMaterialSheet.cs:93,
Beat02_Boulevard.cs:32, Beat05_Garden.cs:64, Beat01_Alley.cs:26).

**Grazing light reveals the normal maps.** Every lamp that stands near a wall is mounted 0.25-1.2 m from it, so across the
pool the wall's incidence stays at 60-85 deg from the normal (NdotL 0.1-0.5): a 10 deg relief tilt changes NdotL by about x2
there, so brick courses, render cracks and concrete pores appear as light-and-shadow ripples. Pools are short (r 5-14 m) and
spaced so every 12-30 m of path has one lit wall patch in view. Wet vertical surfaces also reveal relief through the analytic
sky reflection (Fresnel at grazing view reflects the bright horizon band).

**Light in the air.** Every LampWarm and SignWhite lamp head and the 4 nearest blades get an additive, camera-facing in-scatter
sprite: a Gaussian of sigma 0.5 m (lights of r <= 10) or 0.8 m (r >= 12), peak 5 % of the head radiance x rho_eff, with
rho_eff = min(2, sigma_local / 4.4e-4) x (0.6 + 0.4 rho_rain) (twice as strong in the alley haze), faded out below 3 px of
sigma, fogged like the head and drawn after opaques without depth write. Bloom gives only lens halation; a photographed rainy
night always shows the halo in the air too.

**Climaxes.**
- **The goal window** is the warmest and most saturated light of beat 7 and the only warm one (section 4 schedule, R6, the
  cool deck tint, cool plaza lamps). Aperture: mean luma 165-195, hue 30-36, saturation 0.50-0.62, internal structure +-15
  codes (curtain folds, the lamp, the plant), the 6 px lamp core 200-235 (it displays at saturation 0.37, so the warm pixels of
  check 8 come from the lit room surfaces, GoalLamp x 0.12-0.55 at saturation 0.60). Balcony underside lit 50-80; the tenement
  wall within 3 m lit warm 26-40 against 21-25 elsewhere. No other lit window on the tenement's camera face, at most one cold
  one at x0.5. In KS17 the window already shows as one warm point of >= 4 px with a core >= 200: the set-up of the payoff.
- **The koi** is the colour climax, read as a fish by a luminance ladder: rim (KoiGlow) 210-235; body core 130-175 with both
  shells (v2 gains: about 170 at a vein mean of 0.775); fins 70-110 (body gain x 0.25); tail veil 45-75 with the deck visible
  through >= 50 % (body x 0.12); two eye spots <= 0.5 x body; veins / scale hexes with a std >= 8 codes inside the body;
  clipped koi pixels <= 0.6 % (14.10). Vermilion KoiPatch patches by cell noise (H6; not drawn yet in NocKoi.shader) cover
  15-25 % of the body; the patch mask is evaluated on both shells from (u, |theta|) and wraps >= 70 % of the circumference, so
  front and back agree and a patch sums to vermilion (computed #E56439, hue 15, sat 0.75, luma 125 over the cool deck) instead
  of the salmon #D3887C that the aqua back shell gives (fallback: back weight 0.10 inside patch cells, hue 12 / sat 0.60). The
  mask is 0 on the head (u < 0.12 L), and the patch saturation is multiplied by (1 - 0.75 smoothstep(1 440, 1 490, s)) (20 s
  at 2.5 m/s, no pop), so in KS19 the koi carries no warm saturated pixel next to the window.
- **Required subjects on the darker field.** KS06 tram: a continuous WinNeutral window band at linear 0.25-0.32 (luma
  135-155) and SignWhite headlights with a core >= 2.0. KS09 and KS10 traffic heads: >= 25 codes above the local abyss after
  fog (the abyss is about 15 codes brighter than in the draft).

---------------------------------------------------------------------------------------------------------------------------

## 7. Post per beat (14.2.4) and banding (H1)

| Beat | EV (v1) | Sat (v1) | Bloom thr / int / scatter / clamp (v1 int) | Vignette (v1) |
|---|---|---|---|---|
| 1 | -0.1 (-0.1) | 0 (+8) | 0.9 / 0.40 / 0.70 / 30 (0.45) | 0.16 (0.20) |
| 2 | +0.1 (+0.3) | +4 (+10) | 0.9 / 0.45 / 0.75 / 30 (0.55) | 0.14 (0.16) |
| 3 | 0.0 (+0.2) | +2 (+10) | 0.9 / 0.40 / 0.75 / 30 (0.50) | 0.14 (0.16) |
| 4 | 0.0 (+0.1) | 0 (+8) | 0.9 / 0.40 / 0.72 / 30 (0.45) | 0.15 (0.18) |
| 5 | -0.2 (-0.2) | 0 (+6) | 0.85 / 0.35 / 0.70 / 30 (0.40) | 0.16 (0.22) |
| 6 | -0.1 (0.0) | +4 (+8) | 0.9 / 0.40 / 0.75 / 30 (0.50) | 0.14 (0.16) |
| 7 | 0.0 (+0.1) | +4 (+8) | 0.9 / 0.45 / 0.75 / 30 (0.50) | 0.15 (0.18) |

EV knots (`EvKnotV`, same `EvKnotS`): -0.1, -0.1, +0.1, +0.1, 0.0, 0.0, 0.0, 0.0, -0.2, -0.2, -0.1, -0.1, 0.0, 0.0 (EV at s 200
0.1, 500 0.0, 700 0.0, 1 000 -0.2, 1 200 -0.1, 1 400 0.0; peak slope 0.031 EV/s at 2.5 m/s, far under the 0.30 EV/s slew).
Contrast stays 0, Neutral start 0.76 and desaturation 0.15 (displayed cores in 2.3), no grain, motion blur or DOF. Reasons:
the darkness comes from the palette, so EV stays near 0 and the tokens display as authored; saturation is only for the
accents (the large areas are near-neutral, so +4 cannot recreate a tinted frame); bloom intensity drops 10-20 % because halos
read stronger against a darker sky.

**Bloom knee.** `BloomKneeFraction` 0.5 -> **0.25**: the soft knee starts at exposed 0.675 (0.6375 in beat 5) instead of 0.45.
Windows (at most 0.51 exposed) never enter the bloom; lamps and signs at x1.6, tubes at x2.0, the goal lamp and the koi rim
bloom into wet halation. A pyramid model with 0.5-2 % emitter area gives a frame-mean veil of 0.0004-0.0015 linear with local
halos, so the sky budgets survive.

**Floor-relative grade** (FragGrade, NocGrade.shader:218 and :231, and `NocGradeMath.Display`):
`c = F + (c - F) x 2^EV + bloom x int` (step 1) and `c = (F + (c - F) x vig) x fade` (step 5), F = the Floor. A Floor pixel
displays as the Floor at every EV and in every corner (with the draft grade and Floor #0A0B0C it displayed 9.5 at EV -0.2 and
8.0 in a beat-5 corner, a dithered 1st percentile of 6.1); the fade still reaches black. Saturation (step 2) and the identity tone curve leave the near-neutral Floor
within 0.01 code. GradeTests adds `Display(Floor) == Floor` at EV -0.2 and in the 16:9 corner at vignette 0.16.

**Banding (H1) with darker gradients.** Computed for KS04 (vFOV 66, pitch 22, 1080p): the upper sky changes 43.8 -> 40.9 codes
over 716 px (246 px per code; 492 at 4K), the horizon band 46.1 -> 43.8 over 77 px (33 px per code); the abyss of KS09/KS10
(pitch -18, -60..-6 deg) 60.1 -> 40.9 over 925 px (48 px per code); the largest step of the analytic profile is 0.073 codes per
0.05 deg. The upper sky is now flatter than v1 (266 px per code at 4K) and one code at luma 35-41 is a 4.5-4.9 % luminance step
(2.9 % at v1's luma 70), so plateaus left by an encoder are about 1.7 x more visible. Defences: every sky pixel is below Y' 0.35
(luma 89), so the luma TPDF runs at its full 2.0-code peak; Cb / Cr change by less than 1 code from e 10 to 90 deg (the only
chroma gradient left is the axis glow, Cb -0.9 -> -7.3, Cr +1.3 -> +7.1), so chroma plateaus essentially disappear and the
+-1-code chroma dither (D-007) stays; the new 110 and 40 m cloud octaves put +-5-6 codes of structure at 50-200 px over the
whole deck. Before the palette is locked, run the 11.12 x264 test (CRF 23, 4K, 4:2:0) on CPU-rendered v2 strips (NocFogMath, the
64 x 64 blue-noise tile and the chroma dither) for KS02, KS04, KS10, KS17 and the KS00 / KS19 walls (large dark, flat,
textured areas at luma 15-25 are where x264 removes dither and blocks). Pre-authorised ladder if CRF 23 shows steps > 1 code or
blocking: (1) dither peak 2.5 in the dark, (2) temporal dither in captures (the per-frame R2 offset, `-freeze-dither` off), and
only then (3) the palette. Optional, only if that test shows no chroma steps: fade the chroma dither below luma about 20
(`x saturate((Y' - 0.04) / 0.06)`) for cleaner blacks.

---------------------------------------------------------------------------------------------------------------------------

## 8. Key shots and acceptance checks

### 8.1 Per key shot (14.10 poses unchanged; beat by s: KS03 belongs to beat 1, KS14 to beat 4)

| KS | Mean luma | Dominant colours | Accent | Must read |
|---|---|---|---|---|
| KS00 alley_start | 16-30 | charcoal brick and render walls (17-22), bulkhead pools (25-45), black wet asphalt with warm streaks | the warm-grey slit of the boulevard far ahead (45-48), a few cold windows | brick courses and render stains within 10 m, transom pools grazing the walls, vertical streaks on the floor, the slit (33 px) |
| KS01 alley_noodle | 20-36 (D-041 3: the modelled noodle bar is the subject) | the 2 900 K bar pool (80-140 near the bar), charcoal elsewhere | one cyan vending glow, one red sign | bar window interior, sign strokes, warm-lit steam, the bar's streak on the floor, halos in the air |
| KS02 alley_slit | 22-40 | walls rising into a warm-grey slit of cloud (55-64 over the north-leg glow, billows visible) | cold and warm windows, fire-escape silhouettes | cloud structure, catwalk and roof edges against it, wall texture in window and bulkhead spill |
| KS03 boulevard_mouth (beat 1) | 24-40 | podium stone and concrete lit by LampWarm, warm-grey sky (44-50) | first blades (white, cyan) | stone texture of the facade base, arcade columns, blades |
| KS04 boulevard_axis | 30-48 | dark textured towers (walls 22-26, unresolved mass 34) with 18 % lit mixed windows (~100), the luminous lid (55-69 over the axis), the street as a mirror of it | blades: white, cyan, warm white, amber, tungsten; no magenta, no red | face B's window strata dark against the lid, blade glyphs, long lamp streaks on the wet street, lamp halos |
| KS05 boulevard_up | 34-54 | converging facades: concrete (22-30), dark glass with sheen (16-25), punched window grid; the hot lid (top 20 %, 60-72) | blade tops | facade-cloud lines with tops dissolving into the deck, cloud billows, blade tops |
| KS06 tram | 28-46 | facades and wet street | tram window band WinNeutral (135-155), SignWhite headlights | tram body (MetalPaint, wet), facade texture |
| KS07 edge_window | 26-40 | warm horizon band (45-47) between dark masses; garden crowns as dark textured foliage with lantern-lit undersides | lanterns | far tongue and face B strata darker than the sky; crowns read as leaves, not blobs |
| KS08 edge_flank | 28-44 | the flank's section bays (dark concrete, cold interiors), cloud above (46-55) | amber mechanical slits (SodiumAmber) | slab lines, bays with interior depth |
| KS09 edge_gulf | 30-46 | sky 26 % (40-45), wet pier 39 % (granite / steel, 18-28), drop 36 % glowing umber (50-66) | white cross-bridge lights, red and white traffic far below (>= 25 codes over the abyss) | faces dissolving into haze; darker band under the horizon, brighter depths: no ground |
| KS10 edge_plunge | 32-48 | drop 43 % (50-66), deck and rail | traffic layers | layers at -120 and -520, converging faces, sheets |
| KS11 bridge_train | 24-40 | dark wet tubes and deck (MetalPaint, glass reflections), LampWarm lamp chain | T1's cool-white headlight (bloom) | train nose, glass reflections of the cloud, rain on glass |
| KS12 bridge_void | 30-46 | sky 22 %, deck 30 %, drop 48 % (50-66) | north tube strip (cyan) | far faces, traffic layer, tube |
| KS13 bridge_glass | 28-48 | the glass strip with the glowing abyss below (60-66), deck edges | T6 lights | glass dirt and water texture, depth through it |
| KS14 garden_gate (beat 4) | 24-40 | gate frame (concrete, paint), dark leaves, lantern pools 2 350 K | faint jade specks | leaf texture, bark, lantern pools on the wet path |
| KS15 garden_canopy | 24-42 | crowns from below (18-30) with lantern-lit undersides (40-70), cloud billows between | green specks, LeafWarm tips | leaf clusters with depth and translucency, trunk bark |
| KS16 signs_wall | 38-62 | the wall in slow teal / ice / amber waves (60-85, saturation <= 0.55) | promenade lamps (white) | panel seams and LED grid near, the wave on the wet promenade |
| KS17 heart_reveal | 32-50 | cool: koi aqua (body 130-175, rim 210-235), spire (graphite steel, cyan-white strips), cold windows, cloud cool glow (48-52) | koi vermilion patches; the goal window as one warm point (>= 4 px, core >= 200) | the whole koi with structure, spire, ring towers dark against the deck |
| KS18 koi_pass | 38-60 | koi against the cool deck (48-52) | vermilion patches | body structure, fin translucency, the 20 wave phases |
| KS19 goal_window | 18-32 | dark tenement render / brick (21-25) with texture, the cool entrance pool lower left (27-48); cool sky and koi head at the top | the 2 450 K window (aperture 165-195, core 200-235), balcony underside 50-80 | the window, curtain, plant silhouette, wall texture in its spill, koi head, spire |

Per-beat mean-luma windows for all stills of a beat (replacing 14.10's): 1: 14-40, 2: 24-54, 3: 24-50, 4: 24-50, 5: 20-44,
6: 32-64, 7: 18-60. 1st percentile >= 9 and the clipped limits of 14.10 unchanged.

### 8.2 Acceptance checks (Python, on the 8-bit 1920 x 1080 key-shot PNGs; the beat comes from s in report.json)
1. **R1a / R1b violet** (R1): hue 250-340, sat > 0.35: luma 16-119 <= 1.0 %; luma >= 120 <= 1.5 %.
2. **R2 largest saturated region**: 32 x 32 block means, luma >= 16, blocks with >= 5 % of pixels at luma >= 160 excluded;
   limit 0.65 for hue 10-70, 0.60 in beat 6 and for the beat-7 koi (hue 160-220), 0.45 otherwise; any 250-340 block with
   sat > 0.20 counted; largest 4-connected component <= 5 %.
3. **R3 mid-tone saturation**: 4 x 4 block means with luma 16-159: <= 0.35 (beats 1, 5), 0.40 (beat 6), 0.30 (others).
4. **mean_luma**: within the KS range of 8.1 and the beat window.
5. **sky_top**: median luma of the top 6 % x central 40 % (KS02: x 0.45-0.53, the slit) within the predicted window (model
   median +-10 codes: covers the noise, the +-3 model tolerance and a little occlusion), median saturation <= 0.34 for the
   hot-lid shots KS02, KS04, KS05, <= 0.30 elsewhere. Predicted medians: KS02 60.4, KS04 59.4, KS05 67.6, KS08 50.5, KS09 40.9,
   KS12 41.9, KS17 48.4, KS18 48.4 (KS03, KS06 and KS14-KS16 are dropped: their band is mostly wall).
6. **crush**: share of 4 x 4 block means at luma <= 14.8 (Floor + 3) <= 50 / 30 / 30 / 30 / 40 / 25 / 40 % for beats 1-7;
   1st percentile of the pixels >= 9.0 (the CaptureWriter histogram is integer, so `qa_common.py` uses p1_min = 9/255 exactly).
7. **texture** in the wall regions of KS00, KS01, KS02, KS19: micro = median over 32 x 32 tiles of the std of a 9 px
   high-pass (tiles with mean < Floor + 2 or any pixel >= 200 skipped) >= 1.4 codes; meso = std of 8 x 8 block means minus
   their 3 x 3 block mean (about 8-72 px, the 1-10 m macro detail) >= 1.0 code; fewer than 50 % valid tiles -> INFO; in KS00 and
   KS02 also >= 40 % of each region at luma >= 25 (in a pool). After the 11.12 CRF-23 4K encode, decode and downscale
   (`--encoded`), meso >= 0.75 (texture that dies in x264 is not on YouTube); micro is INFO there.
8. **warm uniqueness** (KS19): pixels with hue 22-50, sat >= 0.45, luma >= 90 outside the goal box (x 0.33-0.57, y 0.42-0.72)
   <= 0.15 %, and >= 2 % of the box (2 986 px, from the lit room); zero koi pixels (head box x 0.75-1.0, y 0-0.30) with hue
   10-50, sat >= 0.45, luma >= 40.
9. **separation** (KS04, KS05, KS07, KS08, KS17, KS18): median sky luma minus the median of the adjacent unresolved-tower box
   >= 6 codes (boxes set by hand from the 14.10 poses, in the script).
10. **highlights**: p99 >= 120 (>= 90 in beat 1 and KS13); p99.9 >= 200 in KS04-KS06, KS09-KS10 and KS17-KS19.
11. **dark cast**: on pixels at luma 12-40, mean (B - G) in -2..+6 codes and |mean (R - G)| <= 4 codes (beat 1: <= 6, D-032 4;
    beat 5, the green leaves: R - G in -8..+4, D-040 5, and no B - G lower bound, D-041 4).
12. **Heart discipline** (KS17, KS18): pixels with hue 25-50, sat >= 0.45, luma >= 90 outside the KS17 goal point <= 0.05 %;
    the KS17 goal point (box x 0.43-0.475, y 0.71-0.76 around the projected window at 0.452, 0.736) has >= 4 such pixels and a
    core >= 200; the koi patch pixels in the koi box (hue <= 40 or >= 350, sat >= 0.30, luma >= 60, at least 200 px) have a
    median hue of 10-22 and a median saturation >= 0.60.

v1 baseline (G4_r04, this script): every shot fails; R1a 17-81 %, R2 28-94 %, R3 0.53-0.73, sky top 56-85 at saturation
0.65-0.68, dark cast B - G +27..+40 and R - G +11..+29 codes, texture micro 0.88-1.10, meso 0.15-0.23, pool share 0.

```python
"""Look v2 acceptance checks (docs/LOOK_V2.md 8.2). Usage: python -I lookv2_check.py <capture dir> [--json out] [--encoded]
--encoded: the dir holds frames decoded from the 11.12 CRF-23 4K encode (scaled to 1080p): texture meso >= 0.75, micro INFO."""
import sys, os, glob, json
import numpy as np
from PIL import Image

FLOOR_LUMA = 11.8                 # Floor #0A0C0F
ENCODED = False
KS = {  # id: (mean-luma lo, hi, beat by s)
 "KS00": (16, 30, 1), "KS01": (20, 34, 1), "KS02": (22, 40, 1), "KS03": (24, 40, 1), "KS04": (30, 48, 2), "KS05": (34, 54, 2),
 "KS06": (28, 46, 2), "KS07": (26, 40, 3), "KS08": (28, 44, 3), "KS09": (30, 46, 3), "KS10": (32, 48, 3), "KS11": (24, 40, 4),
 "KS12": (30, 46, 4), "KS13": (28, 48, 4), "KS14": (24, 40, 4), "KS15": (24, 42, 5), "KS16": (38, 62, 6), "KS17": (32, 50, 7),
 "KS18": (38, 60, 7), "KS19": (18, 32, 7)}
BEAT_S = (120, 400, 560, 880, 1080, 1260)            # beat starts 2..7 (beat from report.json s when present)
SKY_TOP = {  # id: (x0, x1, luma lo, hi, sat max) band = top 6 % rows
 "KS02": (0.45, 0.53, 50, 71, 0.34), "KS04": (0.3, 0.7, 49, 70, 0.34), "KS05": (0.3, 0.7, 57, 78, 0.34),
 "KS08": (0.3, 0.7, 40, 61, 0.30), "KS09": (0.3, 0.7, 31, 51, 0.30), "KS12": (0.3, 0.7, 32, 52, 0.30),
 "KS17": (0.3, 0.7, 38, 59, 0.30), "KS18": (0.3, 0.7, 38, 59, 0.30)}
TEX_REGIONS = {  # (x0, y0, x1, y1) frame fractions
 "KS00": [(0.02, 0.15, 0.28, 0.70), (0.72, 0.15, 0.98, 0.70)], "KS01": [(0.02, 0.20, 0.30, 0.75)],
 "KS02": [(0.02, 0.40, 0.20, 0.95), (0.80, 0.40, 0.98, 0.95)], "KS19": [(0.05, 0.35, 0.30, 0.95), (0.62, 0.40, 0.95, 0.95)]}
POOL_SHOTS = ("KS00", "KS02")
SEP = {  # id: (sky box, unresolved-tower box)
 "KS04": ((0.34, 0.05, 0.45, 0.30), (0.30, 0.42, 0.44, 0.62)), "KS05": ((0.36, 0.20, 0.62, 0.50), (0.15, 0.80, 0.42, 0.95)),
 "KS07": ((0.445, 0.01, 0.555, 0.18), (0.10, 0.05, 0.40, 0.45)), "KS08": ((0.30, 0.05, 0.60, 0.35), (0.62, 0.45, 0.95, 0.70)),
 "KS17": ((0.03, 0.03, 0.20, 0.20), (0.02, 0.45, 0.20, 0.65)), "KS18": ((0.52, 0.03, 0.82, 0.25), (0.02, 0.55, 0.22, 0.80))}
P999_SHOTS = ("KS04", "KS05", "KS06", "KS09", "KS10", "KS17", "KS18", "KS19")
GOAL_BOX = (0.33, 0.42, 0.57, 0.72)       # KS19
GOAL_KS17 = (0.43, 0.71, 0.475, 0.76)     # the projected goal window in KS17 (0.452, 0.736)
KOI_BOX = {"KS17": (0.24, 0.15, 0.75, 0.52), "KS18": (0.28, 0.02, 0.78, 0.82), "KS19": (0.75, 0.00, 1.00, 0.30)}
R3_MAX = {1: 0.35, 2: 0.30, 3: 0.30, 4: 0.30, 5: 0.35, 6: 0.40, 7: 0.30}
CRUSH_MAX = {1: 0.50, 2: 0.30, 3: 0.30, 4: 0.30, 5: 0.40, 6: 0.25, 7: 0.40}
MEAN_BEAT = {1: (14, 40), 2: (24, 54), 3: (24, 50), 4: (24, 50), 5: (20, 44), 6: (32, 64), 7: (18, 60)}

def hsv(im):
    r, g, b = im[..., 0], im[..., 1], im[..., 2]; mx, mn = im.max(-1), im.min(-1); d = mx - mn
    sat = np.where(mx > 0, d / np.maximum(mx, 1e-6), 0.0); dd = np.maximum(d, 1e-6)
    hue = np.where(mx == r, ((g - b) / dd) % 6, np.where(mx == g, (b - r) / dd + 2, (r - g) / dd + 4)) * 60.0
    return np.where(d > 0, hue, 0.0), sat
def luma(im): return 0.2126 * im[..., 0] + 0.7152 * im[..., 1] + 0.0722 * im[..., 2]
def blocks(a, k):
    H, W = a.shape[:2]; h, w = H // k, W // k
    return a[:h * k, :w * k].reshape(h, k, w, k, *a.shape[2:]).mean(axis=(1, 3))
def crop(a, bx):
    H, W = a.shape[:2]; x0, y0, x1, y1 = bx; return a[int(y0 * H):int(y1 * H), int(x0 * W):int(x1 * W)]
def box3(a):
    p = np.pad(a, 1, mode='edge'); return sum(p[i:i + a.shape[0], j:j + a.shape[1]] for i in range(3) for j in range(3)) / 9.0
def largest_component(mask):
    seen = np.zeros_like(mask, bool); best = 0; H, W = mask.shape
    for y in range(H):
        for x in range(W):
            if mask[y, x] and not seen[y, x]:
                st = [(y, x)]; seen[y, x] = True; n = 0
                while st:
                    cy, cx = st.pop(); n += 1
                    for ny, nx in ((cy + 1, cx), (cy - 1, cx), (cy, cx + 1), (cy, cx - 1)):
                        if 0 <= ny < H and 0 <= nx < W and mask[ny, nx] and not seen[ny, nx]: seen[ny, nx] = True; st.append((ny, nx))
                best = max(best, n)
    return best / mask.size
def texture(Yr):
    """micro: median over 32 x 32 tiles of the std of (luma - 9 x 9 box mean); meso: std of 8 x 8 block means minus their 3 x 3
    block box mean. Tiles with mean < Floor + 2 or any pixel >= 200 are skipped."""
    k = 9; p = k // 2; c = np.pad(np.pad(Yr, p, mode='edge').cumsum(0).cumsum(1), ((1, 0), (1, 0)))
    hp = Yr - (c[k:, k:] - c[:-k, k:] - c[k:, :-k] + c[:-k, :-k]) / (k * k)
    T = 32; H, W = Yr.shape; sd = []; tot = 0
    for y in range(0, H - T + 1, T):
        for x in range(0, W - T + 1, T):
            tot += 1; t = Yr[y:y + T, x:x + T]
            if t.mean() >= FLOOR_LUMA + 2 and t.max() < 200: sd.append(hp[y:y + T, x:x + T].std())
    B = blocks(Yr, 8); meso = float((B - box3(B)).std())
    return (float(np.median(sd)) if sd else 0.0), meso, len(sd) / max(tot, 1)

def check(path, beat_of=None):
    ks = os.path.basename(path)[:4]
    im = np.asarray(Image.open(path).convert('RGB')).astype(np.float64); H, W, _ = im.shape
    Y = luma(im); hue, sat = hsv(im)
    lo, hi, beat = KS.get(ks, (0, 255, 0)); beat = beat_of or beat
    C = {}
    v = (hue >= 250) & (hue <= 340) & (sat > 0.35)
    a, b = (v & (Y >= 16) & (Y < 120)).mean() * 100, (v & (Y >= 120)).mean() * 100
    C["R1a_violet_air_pct"] = (round(a, 2), bool(a <= 1.0)); C["R1b_violet_accent_pct"] = (round(b, 2), bool(b <= 1.5))
    B = blocks(im, 32); bh, bs = hsv(B); bY = luma(B); bright = blocks((Y >= 160).astype(float), 32) >= 0.05
    lim = np.where((bh >= 10) & (bh <= 70), 0.65, np.where((beat == 6) | ((beat == 7) & (bh >= 160) & (bh <= 220)), 0.60, 0.45))
    big = ((bs > lim) | ((bh >= 250) & (bh <= 340) & (bs > 0.20))) & (bY >= 16) & ~bright
    lc = largest_component(big) * 100; C["R2_largest_saturated_region_pct"] = (round(lc, 2), bool(lc <= 5.0))
    B4 = blocks(im, 4); _, s4 = hsv(B4); Y4 = luma(B4); mid = (Y4 >= 16) & (Y4 < 160)
    ms = float(s4[mid].mean()) if mid.any() else 0.0; C["R3_midtone_mean_sat"] = (round(ms, 3), bool(ms <= R3_MAX.get(beat, 0.30)))
    ml = float(Y.mean()); C["mean_luma"] = (round(ml, 1), bool(lo <= ml <= hi and MEAN_BEAT.get(beat, (0, 255))[0] <= ml <= MEAN_BEAT.get(beat, (0, 255))[1]))
    if ks in SKY_TOP:
        x0, x1, slo, shi, smax = SKY_TOP[ks]; band = crop(im, (x0, 0.0, x1, 0.06)); bh_, bsat = hsv(band)
        sl, ss = float(np.median(luma(band))), float(np.median(bsat))
        C["sky_top_luma"] = (round(sl, 1), bool(slo <= sl <= shi)); C["sky_top_sat"] = (round(ss, 3), bool(ss <= smax))
    cr = float((Y4 <= FLOOR_LUMA + 3).mean()); C["crush_pct"] = (round(cr * 100, 1), bool(cr <= CRUSH_MAX.get(beat, 0.35)))
    p1 = float(np.percentile(Y, 1)); C["p1_luma"] = (round(p1, 2), bool(p1 >= 9.0))
    p99 = float(np.percentile(Y, 99)); C["p99_luma"] = (round(p99, 1), bool(p99 >= (90 if beat == 1 or ks == "KS13" else 120)))
    if ks in P999_SHOTS:
        p9 = float(np.percentile(Y, 99.9)); C["p999_luma"] = (round(p9, 1), bool(p9 >= 200))
    dk = (Y >= 12) & (Y <= 40)
    if dk.mean() > 0.01:
        bg, rg = float((im[..., 2] - im[..., 1])[dk].mean()), float((im[..., 0] - im[..., 1])[dk].mean())
        C["dark_cast_BG"] = (round(bg, 2), bool(-2 <= bg <= 6)); C["dark_cast_RG"] = (round(rg, 2), bool(abs(rg) <= 4))
    if ks in SEP:
        sky_, twr = SEP[ks]; d = float(np.median(crop(Y, sky_)) - np.median(crop(Y, twr)))
        C["separation"] = (round(d, 1), bool(d >= 6.0))
    for i, bx in enumerate(TEX_REGIONS.get(ks, [])):
        Yr = crop(Y, bx); mi, me, valid = texture(Yr)
        if valid < 0.5: C["texture_%d_info_valid" % i] = (round(valid, 2), True); continue
        C["texture_%d_micro" % i] = (round(mi, 2), bool(ENCODED or mi >= 1.4))
        C["texture_%d_meso" % i] = (round(me, 2), bool(me >= (0.75 if ENCODED else 1.0)))
        if ks in POOL_SHOTS: pool = float((Yr >= 25).mean()); C["texture_%d_pool" % i] = (round(pool, 2), bool(pool >= 0.40))
    if ks == "KS19":
        warm = (hue >= 22) & (hue <= 50) & (sat >= 0.45) & (Y >= 90)
        x0, y0, x1, y1 = GOAL_BOX; ys, xs = slice(int(y0 * H), int(y1 * H)), slice(int(x0 * W), int(x1 * W))
        out = warm.copy(); out[ys, xs] = False
        C["warm_outside_goal_pct"] = (round(float(out.mean() * 100), 3), bool(out.mean() * 100 <= 0.15))
        C["warm_inside_goal_pct"] = (round(float(warm[ys, xs].mean() * 100), 2), bool(warm[ys, xs].mean() * 100 >= 2.0))
        kh, ksat, kY = crop(hue, KOI_BOX[ks]), crop(sat, KOI_BOX[ks]), crop(Y, KOI_BOX[ks])
        n = int(((kh >= 10) & (kh <= 50) & (ksat >= 0.45) & (kY >= 40)).sum()); C["koi_warm_px"] = (n, bool(n == 0))
    if ks in ("KS17", "KS18"):
        stray = (hue >= 25) & (hue <= 50) & (sat >= 0.45) & (Y >= 90)
        if ks == "KS17":
            g = crop(stray, GOAL_KS17); gy = crop(Y, GOAL_KS17)
            C["goal_point_px"] = (int(g.sum()), bool(g.sum() >= 4 and gy.max() >= 200))
            x0, y0, x1, y1 = GOAL_KS17; stray[int(y0 * H):int(y1 * H), int(x0 * W):int(x1 * W)] = False
        C["warm_stray_pct"] = (round(float(stray.mean() * 100), 3), bool(stray.mean() * 100 <= 0.05))
        kh, ksat, kY = crop(hue, KOI_BOX[ks]), crop(sat, KOI_BOX[ks]), crop(Y, KOI_BOX[ks])
        pm = ((kh <= 40) | (kh >= 350)) & (ksat >= 0.30) & (kY >= 60)
        if pm.sum() >= 200:
            hh = np.where(kh[pm] >= 350, kh[pm] - 360, kh[pm]); mh, msat = float(np.median(hh)), float(np.median(ksat[pm]))
            C["koi_patch_hue"] = (round(mh, 1), bool(10 <= mh <= 22)); C["koi_patch_sat"] = (round(msat, 2), bool(msat >= 0.60))
        else: C["koi_patch_px"] = (int(pm.sum()), False)
    return {"shot": os.path.basename(path), "beat": beat, "checks": C, "pass": all(c[1] for c in C.values())}

def beats_from_report(d):
    try:
        rep = json.load(open(os.path.join(d, "report.json")))
        return {s["file"][:4]: 1 + sum(s["s"] >= b for b in BEAT_S) for s in rep.get("shots", [])}
    except Exception: return {}

if __name__ == "__main__":
    args = sys.argv[1:]; out = None
    if "--json" in args: i = args.index("--json"); out = args[i + 1]; del args[i:i + 2]
    if "--encoded" in args: ENCODED = True; args.remove("--encoded")
    res = []
    for d in args:
        bt = beats_from_report(d)
        res += [check(p, bt.get(os.path.basename(p)[:4])) for p in sorted(glob.glob(os.path.join(d, "KS*.png")))]
    for r in res:
        print("%-24s b%d %s %s" % (r["shot"], r["beat"], "PASS" if r["pass"] else "FAIL",
                                   " ".join(k + "=" + str(v[0]) for k, v in r["checks"].items() if not v[1])))
    if out: json.dump(res, open(out, "w"), indent=1)
```

---------------------------------------------------------------------------------------------------------------------------

## 9. Colour script (emotion curve of 14.1 kept)

**Beat 1 - Alley (3 -> 5).** Near-black wet asphalt and charcoal brick and render dominate; light belongs to people: the
noodle bar's 2 900 K pool, door bulkheads and transoms grazing the walls every dozen metres, a red lantern sign, one cyan
vending glow as the cold counterpoint, halos of lit rain around every lamp. The far slit shows the warm-grey cloud of the
boulevard, and from s 90 the mouth breathes warm light (HazeLow, CloudHot) onto the wet floor. No violet anywhere. Purpose:
intimacy and compression; the warmest, darkest, smallest frames of the film.

**Beat 2 - Boulevard canyon (5 -> 8).** Release into scale: the dominant colour is the cloud lid at its hottest (warm grey,
luma 60-70 over the axis) above towers that stand dark against it, textured concrete and dark glass, cold windows above,
LampWarm pools and shop light below, the street a mirror of it all. The fill from the lid models the north wall warm and
leaves the south wall cool. Accents: blades in white, cyan, warm white, amber and tungsten; rose-red only on small signs.
Purpose: awe, look up; the brightest large area of the film is the sky here, and only here.

**Beat 3 - The Edge (8 -> 10).** The ground ends and the light inverts: the brightest large area is BELOW the eye, umber haze
glowing from the lower city (60-66 at the nadir, 26 codes over the sky), above it a darker band under the horizon, and
cold-windowed section faces dissolving into the haze in three distinct layers. Accents: white cross-bridge lights, red and
white traffic streaks deep below. Purpose: vertigo and wonder; the film's peak.

**Beat 4 - Skybridge (8 -> 7).** Restraint and motion: neutral dark steel and glass tubes in rain between two glowing voids;
a chain of LampWarm lamps; trains sweep cool-white light and their reflections across the wet deck. Purpose: exposure, wind,
crossing; colour steps back so movement and rain are the subject.

**Beat 5 - Hanging garden (5 -> 4).** The organic pause: deep wet greens (leaf and moss, saturation <= 0.45), lantern pools at
2 350 K, faint jade bioluminescence as the only glow; the city falls back to a warm haze behind the tongue. Purpose: calm,
warmth, breath before the climax. The only beat where green is a dominant hue.

**Beat 6 - Wall of signs (5 -> 6).** Hypnosis and loneliness: the wall's slow waves of deep teal, ice white and amber
(desaturated, never violet), mirrored by the wet promenade under cool-white lamps. The city is cooling: warm windows and amber
signs fade to none by s 1 250, the wall's amber turns to ice as you leave it, and the deck above turns CloudCool. Purpose:
trance, and the colour preparation of the Heart.

**Beat 7 - Heart Square (9 -> 6).** First the colour climax, then the warm one. Everything is cool: the koi's aqua body and
cyan rim lighting the wet plaza, the spire's cyan-white strips, ten thousand white and cold windows, the cool-lit deck; the
koi's vermilion patches are the most saturated colour on screen, and far away one warm point already burns at the foot of the
tenements. At the end, that single 2 450 K window is the only warm, saturated light in the frame, on a cool-lit wall.
Purpose: awe, then tenderness and home.

---------------------------------------------------------------------------------------------------------------------------

## 10. Where colour lives in the code today (what to change)

Palette source, constants and grade
- `Assets/_Game/Scripts/Runtime/Core/NocturnePalette.cs`: enum 13-18 (append the 15 new tokens), `Table` 32-41, `SpecLinear`
  44-53 (section 2.3), emission factors 57 (section 2.3).
- `Assets/_Game/Shaders/Generated/NocPalette.hlsl`: regenerated by `Tools/gen_hlsl.py` and `Scripts/Editor/PaletteWriter.cs` /
  ShaderGen in RebuildAll; never hand-edit.
- `Scripts/Runtime/Core/NocturneConstants.cs`: 77-78 FloorLin / FloorLuma (2.3); 86-88 cloud model (tint model, base
  amplitude 1.2, four octaves 900 / 300 / 110 / 40 m with weights 0.36 / 0.19 / 0.30 / 0.15, warp 60 m, octave fade
  smoothstep(4, 8 px)); `NC_SheetSkyAmp` 0.10 -> 0.15; 100 RegionalSigma 3.6e-4 -> 4.4e-4 and the beats 2-4 FogSigma; 120
  KoiLightGain 0.9 -> 0.7; 128 residential window 2.0 x 2.4 -> 1.6 x 2.2; 142 FacadeLit 0.34 -> 0.18; 148 zone spread
  0.55 + 0.9 U -> 0.40 + 1.2 U; 150 hue thresholds -> the six classes and the w_f / n_f s knots; 153 FacadeWindowGain 0.20 ->
  Y_med 0.12 with per-token normalisation; 158 edge gain; 160 WarmFraction per beat -> s knots (and k_a for R6); 184 NeonGain
  1.8 -> 1.6; 219-220 EvKnotV (section 7); 230-234 BloomThreshold / Intensity / Scatter, Saturation, Vignette.
- `Shaders/NocGrade.shader`: 218 and 231 Floor-relative exposure and vignette (section 7). `Scripts/Runtime/Rendering/
  NocGradeMath.cs`: 58 `BloomKneeFraction` 0.5 -> 0.25; `Display` mirrors the Floor-relative order (NocGradePass.cs:229 reads
  the knee).

Atmosphere
- `Shaders/Include/NocAtmosphere.hlsl`: 117-131 `NocCloudGlowG` (tint accumulation; amplitudes stay positive; the Gulf shadow
  is a separate multiplier), 133-145 `NocCloudNoise` (four octaves, warp, px fade), 147-151 `NocCloudColor` (tint model),
  155-173 `NocSkyGradient` (tokens only), 177-193 `NocSkyColorBlur` (HazeLow share of the alley layer, section 3.1), 209-213
  `NocFogT3` unchanged, 221-225 `NocFloor` (constants only).
- `Scripts/Runtime/Core/NocSchedule.cs`: 63-71 glow defaults (amplitudes 0.85 / 0.95 / 0.60 / 0.90 / 0.55, tint per glow,
  optional Gulf shadow), 225-240 C# `CloudNoise` / `CloudColor` twins, 245-290 `SkyGradientC` / `SkyColor` twins (tokens, the
  HazeLow share), the FogSigma schedule.
- `Scripts/Runtime/Systems/AtmosphereDriver.cs`: 29 `SkyFill` = { 0.010, 0.035, 0.035, 0.030, 0.015, 0.025, 0.030 }; 31
  `AmbientMul` = { 0.75, 1.0, 1.0, 0.9, 1.0, 0.9, 0.85 }; 46-52 fill colour per beat (CloudHot / CloudBase / CloudCool
  normalised); 96-104 trilight (NightMid x 2.1 / equator (0.0205, 0.0225, 0.0262) / AbyssGlow x 0.25, x 0.35 in beats 3-4;
  garden set), uploaded as value / LightCal; LightCal stays 1.
- `Scripts/Editor/SceneBuilder.cs`: 76 sky-fill colour, 201-205 camera clear = Floor (follows the constants).

Facades, signs, streaks, screen, halos
- `Shaders/Include/NocFacade.hlsl`: 48-54 `NocFacadeMeanHue` (new mix), 82 hue pick per window (six classes), 86 brightness
  `0.6 + 0.8 U` -> log-normal b, token normalisation and the normalised interior factor, 202-205 `NocFacadeEdgeColour`
  (section 4 mapping with k_a).
- `Shaders/NocFacade.shader`: 13-19 defaults (`_BaseColor` = ConcreteDark v2, `_GlassDark` -> GlassDark interior term), 56-60
  wall / glass (photo texture, reveal AO 0.5, unlit glass with stand-by x0.0005).
- `Scripts/Runtime/Rendering/NocFacadeMath.cs`: 72-79 `MeanHue`, 114-119 window hue: the C# mirror of both.
- `Shaders/NocNeon.shader`: 83-87 `NocNeonColour` (4 random kinds, 2 of them violet / magenta) -> weights SodiumAmber 0.28 x k_a,
  WinNeutral 0.24, NeonCyan 0.18, AviationRed 0.14, SignWhite 0.12 + 0.28 (1 - k_a), NeonMagenta 0.04, NeonViolet 0; sign
  panel `_Panel` face <= 0.015 x colour desaturated 60 %.
- `Shaders/NocStreak.shader`: 91 tails `lerp(AviationRed, NeonMagenta, 0.2h)` -> `lerp(AviationRed, SodiumAmber, 0.25 h k_a)`;
  heads `lerp(SignWhite, NeonCyan, h)` -> `lerp(SignWhite, WinNeutral, 0.6h)`, gain so heads stay >= 25 codes over the abyss;
  102 beacons kept.
- `Shaders/Beats/NocScreen.shader`: 49 `ScreenMix` toward NeonViolet -> `lerp(c, ScreenLum(c), 0.35)` then toward ScreenBase
  0.15; 55 ramp stops -> ScreenTeal, SignWhite, SodiumAmber (-> SignWhite over s 1 230-1 290), NeonCyan (luminance-normalised,
  k 0.8). `Beats/Beat06_Signs/NocScreenMath.cs`: 20-40 the C# twin.
- `Beats/Beat06_Signs/SignsLayout.cs:64` `BandKinds` and `ArcadeBuilder.cs:66`: violet bands -> lerp(SignWhite, WinNeutral, 0.5)
  x 0.30 (#8B8C8F, sat 0.03), cyan bands -> lerp(NeonCyan, SignWhite, 0.4) x 0.30 (sat 0.44): the "stripe box" seen in beats
  5-7 loses its violet.
- New: the in-air halo sprites of section 6 (one additive camera-facing shader and a per-beat instance list of lamp heads).

Foliage and koi
- `Shaders/Beats/NocFoliage.shader`: 16-25 gains (`_GlowGain` 0.042 -> 0.012, `_RimGain` 0.06 -> 0.015, `_TipGain` 0.08 ->
  0.03), 94-97 emission and `s.albedo = PAL_Moss x 0.6` -> photo leaf atlas x PAL_Leaf; glow restricted to specks.
- `Shaders/Beats/NocKoi.shader`: 21-22 `_BodyGain` 0.9 -> 0.55, `_RimGain` 2.2 -> 1.6; 146 radiance (the luminance ladder,
  KoiPatch cells on both shells from (u, |theta|), head mask u < 0.12 L, patch saturation fade s 1 440-1 490, scale / vein
  structure of H6); 148 `_BackWeight` 0.35 stays for the body (0.10 inside patch cells only if the shared mask is not used).
- `Shaders/Include/NocLighting.hlsl`: 41-47 ambient (unchanged code), 79 koi light (`NC_KoiLightGain`).

Materials (token x factor)
- `Scripts/Editor/MaterialFactory.cs`: 83 factors (`WetStone`, `MetalDark`, `SteelF0` 0.55, brass) -> dedicated tokens; 89-90
  wear colour `SignWhite x SteelF0` -> SteelF0; 114-144 rows: StoneWall / FloorStone / FloorPlaza / FloorGardenPath -> Granite,
  SteelPainted / FloorSteelDeck / Line -> MetalPaint, SteelBare -> SteelF0, Glass tint -> GlassDark, Moss, Bark, Asphalt and
  Facade keep their names with v2 values; `GrayMarker` emissions stay accents.
- `Scripts/Runtime/Rendering/NocMaterialSheet.cs`: 67 sheet lights (amber -> LampWarm), 93 wall macro `ConcreteDark x 0.25` as a
  radiance (re-derive, section 6).

Beat code (hard-coded token choices)
- Beat 1: `Beat01_Alley.cs:26` `WallMacro = (0.031, 0.021, 0.021)` (literal radiance, re-derive); `AlleyFixtures.cs:19` sign
  light intensity, `:29` mouth light (lerp(HazeLow, CloudHot, 0.5), i 3 -> 2.0), `:171-174` `BladeColour` (case 0 NeonMagenta
  for one sign only, SignWhite otherwise; the default NeonViolet case -> WinNeutral), `:201` mouth wet source CloudHot x 0.8 ->
  x 0.55; ten bulkheads (LampWarm r 7 i 1.6, alternating walls, <= 12 m); only the 4 nearest sign lights are real lights;
  `G1Materials.cs:75-84` emissive and line tokens (`G1_Mouth` CloudHot, `G1_Rail` SignWhite -> SteelF0), `:99` WetAsphalt;
  `NoodleBar.cs:57, 65, 67`; `VendingMachines.cs:15, 51, 61` (light colour lerp(NeonCyan, SignWhite, 0.4)).
- Beat 2: `Beat02_Boulevard.cs:32` wall macro `WinCold x WallMacro` (radiance, re-derive); `BladeSign.cs:16` light i 6 -> 4,
  `:28` `Palette(i) = i % 4` -> a 6-cycle, `:30-36` colours SignWhite, NeonCyan, WinNeutral, SodiumAmber, SignWhite, WinTungsten
  (no magenta, no red, no violet), light colour `lerp(c, WinNeutral, 0.4)`; `MedianPromenade.cs:27, 167, 179` SodiumAmber ->
  LampWarm, i 4 -> 3.5; the tram's window band (WinNeutral, linear 0.25-0.32) and headlights (SignWhite >= 2.0).
- Beat 3: `Beat03_Edge.cs:104` lamp heads SodiumAmber -> LampWarm, `:106` balustrade WinCold -> SignWhite, `:107` portal ring
  NeonCyan (kept), `:109-110` bridge / layer lights, `:160-161` the old 82/8/6/4 hue mix (replace by `NocFacadeMath.MeanHue`);
  `TonguePier.cs:189, 251` light colours.
- Beat 4: `SkybridgeLamps.cs:20` and `SkybridgeMaterials.cs:33` SodiumAmber -> LampWarm; `:40` ConcreteDark x 1.05 -> MetalPaint;
  `BridgeGardenKit.cs:34, 42` helpers (no colour).
- Beat 5: `Beat05_Garden.cs:36` LeafGlow light (lerp(LeafGlow, WinNeutral, 0.5), i 1.0), `:64` macro `WinCold x 0.03`
  (radiance, re-derive); lantern lights lerp(Lantern, WinNeutral, 0.4); `GardenMaterials.cs:24` Moss.
- Beat 6: `ArcadeBuilder.cs:66` cyan / violet -> the two band colours above; `ScreenWallBuilder.cs:57` fallback NeonViolet ->
  ScreenBase, `:72-81` mean colour; wall lights lerp(c, WinNeutral, 0.4) r 45 i 3.5; `PromenadeBuilder.cs:94-95`; `G4Kit.cs:57-69`
  helpers (no colour).
- Beat 7: `OldQuarterBuilder.cs:173` GoalLamp (kept), the goal room (curtain folds, lamp, plant, aperture targets of section 6),
  no other lit window on the camera face, the entrance light, `:195` metal, `:211` dark glass `WetAsphalt x 0.5` -> GlassDark,
  `:226` `ConcreteDark x 0.08` (re-derive); `PlazaFurnitureBuilder.cs:33` plaza lamps WinCold -> SignWhite i 2.5, `:46` metal;
  `SpireBuilder.cs:109-110` strip mix (kept), base ConcreteDark -> MetalPaint.

Tests and QA that encode v1 numbers
- `Scripts/Tests/EditMode/ConstantsTests.cs:61-68` `EvScheduleAndSlew`: EV at s 200 / 500 / 700 / 1 200 / 1 400 = 0.1 / 0.0 /
  0.0 / -0.1 / 0.0 (s 1 000 stays -0.2), peak slope 0.0625 -> 0.03125; `:119` token count 28 -> 43; `:127-130` Floor: add a
  FloorLinG assert, codes (11, 8, 32) -> (10, 12, 15), message "10.4" -> 11.8.
- `FacadeTests.cs:28` `RenderPatch`: Y_med 0.12 with per-token normalisation, log-normal b, wall 0.00427, unlit glass 0.0016,
  glass fraction 0.757, normalised interior factor; city mean 0.0114 +-3 %; `:139-141` lit 0.34 -> 0.18 and the six hue classes
  (w_f/3, 2w_f/3, n_f, 1 - w_f - n_f - 0.07, 0.03, 0.04); `:155, :159` `WarmOf(..., 0.10)` -> w_f(s) (0.32 for s <= 400, 0.20 at
  880, 0 from 1 250).
- `AtmosphereTests.cs:143-171` `HorizonAgreement_FogSkyDome_12Elevations`: holds unchanged (the HazeLow share lives in the sky
  function, so the fully fogged colour is the dome colour at s 0 too); `:129` monotone abyss and `:174-208` the horizon luma
  profile hold (max step 0.073 codes per 0.05 deg).
- `GradeTests.cs:452-459` darks below the Neutral start: add Plaster, Brick, Granite, MetalPaint, SteelF0, Leaf, GlassDark,
  CloudCool, HazeLow, ScreenBase (all < 0.76; maximum SteelF0 0.527); add `Display(Floor) == Floor` at EV -0.2 and in the
  corner; the knee change (0.25) where a test reads `BloomKneeFraction`. Bloom clamp (3.5 x 2^0.3 < 30), vignette (reads
  Vignette[0]), :396 and the SkyGradient stops stay valid; MaterialTests mean 0.5 is the 5.4 rule.
- `Beats/Beat07HeartTests.cs:388-398`: WinCold -> SignWhite; add KoiPatch, the beat-7 edge and neon colours (no amber after
  s 1 250); GoalLamp hue 32 stays inside 28-48.
- New EditMode test: every built light with range > 10 m has sRGB HSV saturation <= 0.45 except the goal lamp (R4).
- `Tools/qa/qa_common.py:88` p1_min 0.0353 -> 9/255 (`CaptureWriter.cs:270` builds an integer histogram, so lumaP01 is the
  integer floor); `:90-91` mean_luma -> the per-beat windows of 8.1; `selftest.py:256, 419` CloudHot (176, 58, 140) patch: still
  a valid synthetic test, comment only.
- Add `lookv2_check.py` (section 8.2) under `Tools/qa/` and to `run_all.py`.
- Lead / TECH, not in this file: TEXTURE_PLAN.md:310 and E7 (line 426) still say D-017's 0.026 -> the city mean 0.0114;
  MASTER_PROMPT 14.0 / STATE.md digest ("magenta cloud deck"), 14.2.2's checked fog values, 14.12 and T-FOG (sigma 4.4e-4 in
  beats 2-4); docs/MATERIALS.md rows.

---------------------------------------------------------------------------------------------------------------------------

## 11. Sources (web content used as data)

Sky glow, clouds and lamp spectra
- Kyba et al. 2011, "Cloud coverage acts as an amplifier for ecological light pollution in urban ecosystems" (PLoS ONE): overcast
  amplification 10.1 inside Berlin, 2.8 at 32 km. https://pmc.ncbi.nlm.nih.gov/articles/PMC3047560
- Jechow et al. 2017, all-sky photometry of clouds and skyglow (Balaguer: cloudy zenith 3.3 and 2.3 mcd/m2 vs clear 1.2 and
  0.6). https://www.arxiv.org/abs/1705.04968
- Vienna University Observatory night-sky photometry (overcast urban zenith above 100 mcd/m2). https://arxiv.org/pdf/1304.7716
- Hong Kong night-sky brightness (Pun and So; urban 15.0 vs rural 20.1 mag/arcsec2, about 100 x).
  https://arxiv.org/pdf/1106.3842 and https://ar5iv.labs.arxiv.org/html/1401.1200
- Skyglow (spectral ratios: white LED 1.8-3.3 x the visual sky glow of HPS). https://en.wikipedia.org/wiki/Skyglow
- US DOE report on LED street lighting and sky glow. https://www.energy.gov/eere/ssl/articles/doe-publishes-report-led-street-lighting-s-impact-sky-glow
- NASA Earth Observatory, "The many colors of electric lights" (HPS orange, metal halide white, mercury green).
  https://earthobservatory.nasa.gov/images/84571/the-many-colors-of-electric-lights
- ESA, Europe at night from space (warm = sodium, white / blue = LED). https://www.esa.int/ESA_Multimedia/Images/2025/12/Europe_at_night_from_space
- Astronaut before / after photographs of cities switching from orange to white light.
  https://petapixel.com/2026/09/08/astronauts-before-and-after-photos-show-how-cities-switched-from-orange-to-white-light/

Windows, signs and light colour
- CIE 2025, light-emitting windows of a block of flats: average 6 cd/m2, at most about 50 % lit; adverts 50 cd/m2.
  https://files.cie.co.at/x051_2025/P0796_ID298TomasNovak_OA_with_cover.pdf
- Zumtobel / Fraunhofer office lighting study (most users prefer 4 000-5 000 K in offices).
  https://www.architonic.com/fr/story/ulrich-buttner-zumtobel-and-fraunhofer-institute-study/7000992
- Residential 2 700-3 500 K vs cooler offices (trade guidance). https://www.propmodo.com/what-color-temperature-should-you-use-for-your-commercial-space/
- US Sign Council Foundation 2018 (neon signs: max 317, weighted mean 117.5 cd/m2).
  https://usscfoundation.org/wp-content/uploads/2018/03/USSCF-Sign-Lighting-Impact-on-the-Environment-2018.pdf
- Lighting Research Center, commercial sign luminance (100-450 cd/m2; preferred 40-230). https://www.lrc.rpi.edu/programs/solidstate/cr_commercialSigns.asp
- Signs of the Times, exposed neon tube luminance by colour. https://signsofthetimes.com/neon-visibility/amp/
- Planckian locus (Kim et al. 2002 cubic spline used for the blackbody colours). https://en.wikipedia.org/wiki/Planckian_locus

Wet streets and materials
- Elongated reflections on a rippled water film (USPTO 9230183). https://image-ppubs.uspto.gov/dirsearch-public/print/downloadPdf/9230183
- Specular glare of wet roads. https://www.physicsclassroom.com/Class/refln/u13l1d.html
- S. Lagarde, "Water drop 3b - Physically based wet surfaces". https://seblagarde.wordpress.com/2013/04/14/water-drop-3b-physically-based-wet-surfaces/
- Lekner and Dorf 1988, "Why some things are darker when wet". https://www.wgtn.ac.nz/scps/staff/pdf/darkerwhenwet.pdf
- physicallybased.info (linear base colours: concrete 0.51, fresh asphalt 0.043, brick 0.262 / 0.095 / 0.061, iron 0.530 /
  0.513 / 0.494, grass 0.105 / 0.133 / 0.041). https://physicallybased.info/
- PBR albedo reference (new asphalt 0.04-0.05, aged 0.1-0.18, wet 0.06-0.08; aged concrete 0.2-0.3; brick 0.2-0.4).
  https://docs.studio-397.com/x/EgBDAg
- Albedo of surfaces (asphalt 5-10 %, concrete 15-25 %). https://www2.hawaii.edu/~dennis/GeoBook/chapter_3/albedo.htm
- Albedo values for modelling (dry asphalt 0.09-0.15, concrete 0.25-0.35, wet asphalt 0.18). https://pvpmc.sandia.gov/?p=313
- Unity, "Making believable visuals" (albedo of painted dielectrics 0.2-0.8; puddle rings darken albedo).
  https://docs.unity3d.com/cn/2018.3/Manual/BestPracticeMakingBelievableVisuals5.html
- Texture libraries (CC0): https://ambientcg.com/ and https://polyhaven.com/

Film and photography references
- American Cinematographer on Collateral (night exteriors on HD, "make the fill light the key light").
  https://theasc.com/magazine/aug04/collateral/page1.html ; Roger Ebert's review (the city's lights reflected in low clouds):
  https://www.rogerebert.com/?p=242493
- Blade Runner 2049: American Cinematographer https://theasc.com/articles/uncanny-valley-blade-runner-2049 ; Filmmaker
  Magazine interview with Roger Deakins https://filmmakermagazine.com/?p=103272
- Greig Fraser on The Batman (frames "teetering on the edge of unreadability").
  https://www.motionpictures.org/2022/03/the-batman-cinematographer-greig-fraser-on-finding-light-in-the-darkness/
- Greg Girard, Hong Kong nights 1974-1989. https://hongkongfp.com/2017/10/01/hkfp-lens-greg-girards-nocturnal-wanderings-hong-kong-1974-1989/
  and https://invisiblephotographer.asia/2017/01/26/conversation-greggirard/
- Ghost in the Shell (1995) cinematography analysis (weathered city surfaces). https://colorculture.org/ghost-in-the-shell-cinematography-analysis/

Computation scripts (not committed; git-ignored scratch): `ExternalTextures/tmp/art/` (ART: `cct.py` blackbody colours,
`palette.py`, `sky.py`, `budget.py`) and `ExternalTextures/tmp/artf/` (ART-F: `calc.py` final palette, Floor-relative display,
sky tables, surface / window / glass / facade budgets, light saturations, koi sums, Floor dither simulation; `skytop.py` the
sky_top predictions, cloud-noise statistics, koi ladder and structural contrasts; `lookv2_check.py` the checks, run on G4_r04
for the v1 baseline).

---------------------------------------------------------------------------------------------------------------------------

## 12. Review log (ART-C1 cinematographer, ART-C2 rendering engineer; ART-F decisions)

Every MUST was applied; where a reviewer's number did not survive recomputation, the intent was applied with the computed
number and the difference is stated. No amendment was rejected outright.

| ID | Amendment | Decision | Reason and where |
|---|---|---|---|
| C1-1 | Towers darker than the sky: lit 0.26 -> 0.18, Y_med 0.15 -> 0.12, CloudBase #211E1C -> #252422, one city-mean number | Applied, mean restated | 2.1, 4. With C2-10's corrected lit-opening area (0.1227 at 0.26, not 0.113) and the frame occlusion (glass 0.757) the city mean is 0.0114 linear, not 0.0127: luma 33.7 at EV +0.1, 7 under the deck, 13 under the horizon, 34 under the hot core. Deck columns of 3.2 recomputed (+5 codes). |
| C1-2 | Unlit glass darker than the wall: stand-by x0.0025 -> x0.0005 | Applied | 4, 5.3: frontal 16.3, 60 deg 17.6, 75 deg 24.3 against a shade-side wall of 22.3 (-6 codes face-on). |
| C1-3 | Light the masses: display targets, equator 0.0223, sky fill per beat, yaw claim | Applied | 6, 5.3. LightCal fixed at 1.0 (with C2-3). Targets recomputed with the sky reflection term: shade 22.3, fill side 25.5, ledge 30.0, reveal 17.6 at AO 0.5 (AO 0.6 gives 18.6, short of -5, so reveals use AO 0.5). Yaw 35 lights the north wall (0.41) and face B (0.29) only; asymmetry kept. |
| C1-4 | Alley ambient 0.75, bulkheads <= 12 m i 1.6, light cap, pools, KS19 entrance light, check 7 2.5 / 1.8, structural contrasts | Applied with changes | 6, 4, 8.2. Added r 5 -> 7: at r 5 a bulkhead never reaches the facing wall; at r 7 it gives render 26-33 there. 19 lights. Check 7 uses C2-9's robust metrics; C1's encode survival is kept as meso >= 0.75 after the CRF-23 encode (C1's 1.8 / 2.5 ratio applied to the meso threshold) and the >= 40 % pool share is part of check 7. |
| C1-5 | Luminous lid: CloudHot #4D4034, glow 0.95 / 0.85, tint noise 0.5, mouth i 2.0, mouth wet x0.55, sky_top 26-75, KS05 34-54 | Applied, sky_top reconciled | 2.1, 3.1, 6, 8. sky_top uses per-shot windows from the model (C2-4): KS04 49-70, KS05 57-78, saturation <= 0.34 for the hot-lid shots. |
| C1-6 | The Edge inverts: AbyssGlow #4F3D2E, sheets +15 %, ground x0.25 / x0.35, new KS windows | Applied | 2.1, 3, 6, 8.1. Recomputed with sheets and Floor the band under the horizon is 41 (not 33-37); the nadir 66 is still 26 codes over the sky. R2 holds (sat 0.42, and the warm limit is now 0.65). |
| C1-7 | Cloud mid-scale form: 110 m and 40 m octaves, warp 60 m, amplitude 0.8, 4-8 px fade | Applied, amplitude 1.2 | 3.1. The simulated four-octave sum has a std of 0.111, so 0.8 gives only +-4 codes (p1..p99); 1.2 gives the intended +-5-6. Weights 0.36 / 0.19 / 0.30 / 0.15. |
| C1-8 | Finish removing violet (NocNeon, AlleyFixtures, SignsLayout, edge lights, blade cycle) | Applied | 2.4 R7, 4, 10. Edge lights white 55 / amber 30 / cyan 15 %; blade 6-cycle without magenta or red; band colours computed (#8B8C8F, sat 0.44 cyan). |
| C1-9 | Koi patches vermilion through the shells | Applied | 6: patch mask on both shells from (u, abs(theta)): #E56439, hue 15, sat 0.75 (back aqua gives #D3887C, hue 8, sat 0.41); fallback back weight 0.10 (hue 12, sat 0.60). |
| C1-10 | Protect the warm window: head mask, patch saturation fade, check 8 hue 25-50, KS19 koi gate | Applied, band 22-50 | 6, 8.2. Check 8 uses C2-8's 22-50: the head mask and the fade already take the koi out (patch at x0.25 saturation displays sat 0.24), and 22 also counts SodiumAmber-like hues; the KS17 / KS18 stray gate keeps C1's 25-50 because full-saturation patches are in those frames. |
| C1-11 | Checks for separation, highlights, dark cast, R3 beat 6 0.40 | Applied | 8.2 checks 9-11, R3. Boxes set from the 14.10 poses on G4_r04 (no ID mask exists in captures). v1 fails the dark cast by +27..+40 codes. KS07 is the tightest separation (predicted about 6), which is why the Gulf shadow stays off. |
| C1-12 | Warm and cool separation: cool equator (0.0205, 0.0225, 0.0262), fill colour = the deck above | Applied | 6. Equator hue 218, sat 0.13; fill CloudHot / CloudBase / CloudCool normalised. |
| C1-13 | NeonMagenta -> #FF3366, small-sign weights, cyan held back | Applied | 2.1, 10: hue 345, sat 0.80, luma 98.1; displays at 349-350, outside R1. Weights 0.04 / 0.18 / 0.28 / 0.24 / 0.12 / 0 / 0.14 (sum 1.00), amber x k_a (R6). |
| C1-14 | Floor slightly cool #090B0E | Applied, merged with C2-1 | 2.1. #090B0E (luma 10.8) gives a dithered p1 of 8.93 even with the Floor-relative grade; #0A0C0F keeps C1's B - R +5 codes at luma 11.8 (p1 9.93). |
| C1-15 | More depth haze in beats 2-4, sigma 4.4e-4 | Applied | 3.1: also RegionalSigma, so the alley total stays 5.0e-3; tables recomputed with it; T values confirmed (0.90 / 0.82 / 0.46). |
| C1-16 | Halos in the air around lamps | Applied | 6: rho_eff defined as min(2, sigma_local / 4.4e-4) x (0.6 + 0.4 rho_rain). |
| C1-17 | Koi luminance ladder | Applied | 6: body about 170 with both shells (vein mean 0.775), fins at gain x0.25, veil x0.12 (75). |
| C1-18 | Goal window spec and KS17 set-up | Applied | 6, 8.2 check 12: the window projects to (0.452, 0.736) in KS17 (validated by its KS19 projection, which matches the capture); stray warm in KS17 / KS18 <= 0.05 %. |
| C1-19 | Required subjects pop (tram band, traffic heads) | Applied | 6, 10. |
| C2-1 | Floor below the Floor through exposure and vignette; Floor token; p1_min; corner claims | Applied | 7 (Floor-relative grade in FragGrade and Display), 2.1 Floor #0A0C0F (with C1-14), 8.2 (p1_min = 9/255, integer histogram), corner claims deleted. |
| C2-2 | 5.4 contradicts the engine's colour rule | Applied | 5.4: map mean 0.5 per channel, chroma and R5 on token x 2 x var, contrast as variation std (ratio x 0.5), inside 0.06-0.94. |
| C2-3 | LightCal advice contradicts the budgets; missing fill and sky reflection | Applied | 6: LightCal 1.0; 5.3 recomputed with the fill and the sky reflection (rows widened: shade 21-26, fill side 25-30). |
| C2-4 | sky_top range and shot list | Applied, ranges recomputed | 8.2 check 5: shots KS02 (x 0.45-0.53), KS04, KS05, KS08, KS09, KS12, KS17, KS18; per-shot windows (model median +-10) because C1-5's lid puts KS04 / KS05 at 59 / 68, above a single 30-62 range. |
| C2-5 | R1 hue window 250-340 and the R1a / R1b split | Applied | 2.4, 8.2. |
| C2-6 | R2 fails on designed content | Applied | 2.4, 8.2: bright-block exclusion, 0.65 for hue 10-70. |
| C2-7 | R4 ambiguous, rig violations | Applied | 2.4: sRGB HSV convention, `lerp(c, WinNeutral, 0.4)`, lantern light lerp(Lantern, WinNeutral, 0.4) (0.34), beat-6 wall lights, exceptions GoalLamp and koi light, EditMode test. |
| C2-8 | Check 8 robustness: hue 22-50; amber never removed from beat 7 | Applied, screen-wall ramp changed | 2.4 R6, 4, 10: k_a(s) on every city-wide amber. The screen wall's amber stop ramps over s 1 230-1 290 instead of with w_f, which would have removed 71 % of it before beat 6 begins. The goal-lamp core is not counted; the room surfaces are. |
| C2-9 | Check 7 not robust and at the wrong scale | Applied | 8.2: tile-median micro >= 1.4, meso >= 1.0, 50 % valid tiles; v1 measures 0.88-1.10 and 0.15-0.23. |
| C2-10 | Facade mean breaks H2 energy conservation | Applied, normalised form | 4: the interior factor is divided by its expectation (glass mean Y_med x b at every footprint) and only its zero-mean part fades in over 4-8 px; frame occlusion by filtered coverage. Facade mean restated with C1-1's values (0.0114). |
| C2-11 | HazeLow fog tint breaks horizon agreement | Applied, simpler form | 3.1: the share is computed inside NocSkyColorBlur from the ray's own CapMean (path to min(6 500 m, the cloud hit); the path length cancels, so it is continuous), so fog colour = NocSkyColor carries it, agreement is exact and NocFogT3 needs no new argument. KS02 slit +3.2 codes. |
| C2-12 | Tests that pin v1 numbers | Applied | 10: every listed test with its v2 values (Floor codes 10, 12, 15 and luma 11.8 with this Floor). |
| C2-13 | Bloom wording | Applied | 7: `BloomKneeFraction` 0.25, so "windows never bloom" is true (max 0.51 < 0.675). |
| C2-14 | Decide the Neutral desaturation, write displayed values | Applied (kept 0.15) | 2.3 table: cores whiten slightly as in photographs while halos keep the authored saturation; 0.05 values listed for reference. |
| C2-15 | R3 and crush dither-robust, per-beat limits | Applied | 2.4, 8.2: 4 x 4 block means, luma >= 160 excluded, 0.35 in beats 1 and 5 (0.40 in beat 6 from C1-11); the chroma-dither fade is optional, gated by the encode test. |
| C2-16 | Beat assignment by s and the beat-1 window | Applied | 8: beat from report.json s (KS03 beat 1, KS14 beat 4); beat 1 window 14-40. |
| C2-17 | Banding risk at luma 35; encode test before locking; temporal dither | Applied | 7: numbers recomputed (246 px per code in the upper sky), CPU-strip x264 test, temporal dither pre-authorised as ladder step 2. |
| C2-18 | Factual corrections (luma column, ratio, far skyline, Gulf shadow) | Applied | 2.1 Disp. column; 1 ratio 1 : 7 : 55-75; far skyline now 10 codes at 1 km and 5 at 2 km under the horizon band; Gulf shadow as a separate multiplier, off by default. |
