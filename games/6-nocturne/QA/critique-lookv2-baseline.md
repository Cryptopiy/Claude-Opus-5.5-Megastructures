# CRIT0 - Look v2 baseline critique (D-027)

Purpose: a frame-by-frame record of what, in the current look, breaks the illusion of a real, textured, dark night megacity.
It is the "before" picture for Look v2 (user directive D-027, 2026-11-14): (1) no violet/magenta everywhere, darker tones;
(2) real texture on every building, window and surface; (3) CC0 photo textures allowed; (4) the visuals must be perfect.

## Inputs and method
- `QA/captures/G4_r04`: the 20 key shots KS00-KS19 at 1920x1080, built player, `-freeze-clock`.
- `QA/captures/G1_r06b`: the 51 beat stills, i.e. start / mid / end plus free-look l / r / back / up60, and down60 for beats 3 and 4.
- No `Q1_*` folder exists. Q1's newest captures are `graybox01` and `graybox01b` (main build, 06:43). They are pixel-identical to
  G4_r04 and G1_r06b: mean absolute difference 0.000-0.001 per image, 0 pixels differ by more than 8 codes. Everything below
  therefore also applies to them. `graybox01_1610` has the same 20 poses at 16:10; it was not reviewed separately.
- I opened every PNG of both folders at full size with the Read tool, plus the contact sheets.
- The numbers come from a read-only Python pass over all 71 PNGs (Pillow/numpy, script kept outside the project):
  - **violet share**: the fraction of pixels with HSV hue 255-335 deg, saturation > 0.25 and value > 0.08;
  - **dark cast**: the mean B-G and R-G in 8-bit codes over pixels with luma < 0.25;
  - **material energy (hf)**: the mean |luma - Gaussian(3 px) luma| in codes over non-emissive pixels (luma < 0.30). The
    one TPDF dither alone (2-code peak) gives 0.71 on a flat field in this measure; I simulated that.
  - **lit-pixel palette**: the hue classes of pixels with value > 0.45.

## 1. Measured baseline (the numbers Look v2 has to move)

| Measure | Key shots G4_r04 | Beat stills G1_r06b | What it means |
|---|---|---|---|
| Violet share of the frame | mean 0.78 (0.34 KS15 .. 0.996 KS13) | mean 0.74 (0.20 garden up60 .. 0.985 edge down60) | three quarters of every frame is violet/magenta |
| Dark cast B-G / R-G (codes) | +35.1 / +18.4 | +35.4 / +18.1 | even the "blacks" are violet; neutral darks sit within about +-4 |
| Sky sample (rows 40-80, x 940-980) | KS04 (166, 54, 158), KS05 (177, 59, 168), KS18 (165, 56, 157) | edge up60 (156, 51, 149) | saturated magenta at luma about 85-95, brighter than most of the city |
| Frame mean RGB, worst | KS18 (121, 64, 129), luma 80.7 | heart up60 (145, 86, 154), luma 103 | look-ups are dominated by the bright magenta sky |
| Material energy hf, close-range surfaces | KS13 0.86, KS19 1.18, KS00 1.33, KS01 1.58 | edge down60 0.84, alley look_l 0.96, skybridge down60 1.15, heart end 1.27 | 0.1-0.6 code above the dither alone: walls, decks and floors carry no texture at all |
| Material energy hf, facade frames | 3-6.4 (only the window grid) | 2.7-7.1 | the only "detail" in the walk is the window confetti |
| Lit-pixel palette, frontal boulevard facade (look_l / look_r) | - | neutral 14-17 %, cyan 7-8 %, magenta 3-6 %, amber 6-10 %, pale grey-blue (WinCold) about 60 % | every lit window is one flat colour at one flat level |

Proposed numeric gates for Look v2. The lead adopts them or not; they are measurable with the same script.
- Violet share at most 0.10 outside the neon masks.
- Dark cast: |B-G| <= 6 and |R-G| <= 4 codes.
- Sky luma at most 35 with chroma at most 10 codes, except at the city-glow hot spots.
- Material energy hf >= 3 codes on every close-range surface (within 30 m).
- Lit windows:
  - at least 85 % in the warm-white / neutral / cool-white family (2 700-6 500 K);
  - at most 5 % saturated;
  - per-window luminance spread at least x0.4-x1.6.

Two things to note before the gates are adopted:
- The 14.10 mean-luma windows and the H1 "1st percentile >= 9" floor were derived for the violet palette. They must be
  re-derived for the dark palette.
- A darker, neutral palette makes the H1 encode test harder: neutral darks have less chroma to hide steps in. Re-run Part
  11.12 on the new look before calling it done.

## 2. Per-frame findings
Tags:
- **V**: violet cast
- **F**: flat surface, no material
- **S**: scale does not read
- **W**: confetti windows
- **B**: missing building structure
- **A**: sky / fog
- **G**: foliage
- **K**: koi
- **N**: signs
- **L**: lights / reflections
- **C**: composition

### Key shots (G4_r04)
| Shot | What breaks the illusion |
|---|---|
| KS00 alley_start | V: whole frame violet, 0.88 of pixels. F: both alley walls are untextured planes with no brick, concrete, joints, stains, pipes or drips; the floor is one smooth plane with large camouflage puddle blots and a magenta centre stripe (the slit reflected). B: doors are black slits with no frame or step; lamps are cream emissive rectangles with no fixture; the fire escapes are the only structure. N: hanging signs are flat teal, pink and purple slabs with no glyph, frame or housing. W: the far slot shows confetti windows. L: no light pool under any lamp; no clutter such as bins, AC units, cables or steam. |
| KS01 alley_noodle | F: the noodle bar interior is a blown-out cream box with no counter, stools, menu, noren or steam; the awning is a flat mustard slab with one specular line. W: wall windows are cream and white cards of random size, not aligned to floors, with no frames or glass. N: blades are flat colour bars. L: the warm spill on wall and floor is good (keep it); its floor reflection is only a soft blob. |
| KS02 alley_slit | A: the slit is a flat magenta gradient with no cloud, so catwalk and roof edges have nothing to read against. F: the walls are violet plaster with faint diagonal noise. W: big flat white and cream window cards. B: the AC unit is an untextured black cube; the catwalk is a dark slab; the roof edges dissolve into the fog. N: the red-magenta sign slab dominates the frame and is flat. |
| KS03 boulevard_mouth | W: the money facade is confetti: lit cells cyan, magenta, ochre and grey-white at similar weight on black, clustered into Tetris-like zone blocks. B: no floor slabs, spandrels, mullions, column bays, podium, setbacks or crowns; the towers merge into one flat wall. N: the blades are flat cyan and violet slabs with 3-5 random sticks that read as scratches, not lettering. F: the floor is 40 % purple camouflage blots. L: the shopfronts are one row of identical white squares. |
| KS04 boulevard_axis (money shot) | A: 30 % of the frame is flat magenta (166, 54, 158) with no cloud structure. Face B ends in a ruler-straight horizontal fog line about y 330 px, so the towers look sliced off. W: the near towers carry big multicoloured tiles; face B is uniform noise. B: no corner articulation, balconies, fins (the T0 tier is not visible) or crowns; face B is two monolithic slabs; the spire is a faint translucent grey pole. G: the trees are green cotton balls pasted at the end of the axis. K: a white-cyan sausage. L: lamp heads are flat yellow slabs; floor blots. |
| KS05 boulevard_up | A: 55 % flat magenta. The "cloud lines" required in this shot do not exist visually; the face-B mass at the bottom has a ruler-straight top plane. N: flat blade slabs with stick glyphs and visible colour steps across the facets. W/B: windows magnify into 3-4 m frameless squares; tower edges have no thickness or rib. Rain reads as a warp-speed starfield. |
| KS06 tram | L: the tram, a required subject, is not readable: it is a dark windowless box with one lit cell. There is a vertical colour boundary in the floor reflection at x 900-1020: at y 1 000 R steps 18 -> 42 within 140 px, blue on the left, magenta on the right. V: the right half is saturated magenta. W: near-tower tiles. K: koi and fin read as white plastic. G: cotton-ball trees. |
| KS07 edge_window | W/B: face B fills 80 % of the frame as two walls of identical static, with no bay rhythm, terraces, FaceRib, atria or mechanical bands. S: they could be 50 m or 5 km away. F: camouflage blots dominate the lower 45 % (the plaza floor). K/G: koi band, cotton-ball trees. L: lamp heads are flat discs. A horizontal smear band from the grazing-angle fade runs along the right edge (x 1 700-1 920). |
| KS08 edge_flank | A: the top of the flank face dissolves into a translucent magenta veil, so the tower reads as a hologram with no roof or crown; the sky is flat. W: the near tower shows giant diagonal tiles. F: the planters are untextured dark boxes. The abyss beyond the balustrade is a flat violet band. |
| KS09 edge_gulf (money shot) | A: the abyss, which must give vertigo, is a flat violet field (frame mean 50, 31, 73): no lit layers, no visible haze sheets, no city receding downward. B: the Gulf faces read as smooth drums wrapped in confetti, not cliffs of stacked city. S: the cross bridges are unlit hairlines; nothing gives a size. F: the deck and balustrade are flat. L: a blue lens-flare blob at the right edge. |
| KS10 edge_plunge (money shot) | A: as KS09, with 45 % flat violet below. B: the right half is an untextured black slab (gate block) crossed by one diagonal line. F: the deck tiles are barely visible (hf 1.85). S: "layers" are one orange streak and one line, so the drop reads as a hole of nothing. L: the same flare blob. |
| KS11 bridge_train | L: the T1 train, a required subject, is not visible; there is only an orange glow on the left. F: the glass tubes read as violet tinted plastic sheets with no ribs, frames, reflections or rain film. W: the faces are walls of static. G/K: cotton balls and a white band. The deck is smooth. |
| KS12 bridge_void | F: the lower 30 % of the frame is a featureless dark slab (the deck edge). A: a flat violet void. B: the faces are drums. L: an orange blob floats in mid-sky (an aviation light with no mast). The railing reads well. |
| KS13 bridge_glass | L/C: the shot fails completely. The whole frame is dark violet deck (violet share 0.996, hf 0.86) with ripple rings that read as target decals; the glass strip, the abyss and T6 are all absent. |
| KS14 garden_gate | G: the crowns are enormous green sponges, smooth convex ellipsoids with moss noise and no leaf clusters, silhouette breakup, translucency, LeafGlow or lantern points. The trunks are dark cylinders with cut stub branches like pencils. F: the gate is an untextured box frame; the tubes are glossy violet pipes. V: the saturated green clashes with the magenta sky. L: the lamp head is a flat slab. |
| KS15 garden_canopy | G: 70 % green sponge; no lanterns, although the acceptance asks for >= 25 lantern blobs; no glow; branches stop below the crowns. K: a white disc and a star-like fin. A: magenta gaps. |
| KS16 signs_wall | N: the wall of signs reads as a slate-blue glass curtain wall in daylight: smooth pink-to-teal gradient panels, thick dark seams, no LED pixel structure, no imagery and no emissive depth. V: magenta sky behind. B: the neighbouring tower edges are magenta. G: a sponge crown at top left. |
| KS17 heart_reveal (money shot) | K: the koi is an opaque white-cyan banana with no head shape, eyes, fins, Kohaku patches, scale lines, scanlines or translucency; it reads as a crescent moon or a UI swoosh. A: 45 % flat magenta; every tower dissolves upward into vertical magenta streaks ("dripping paint"). B: the spire is a straight box with confetti and four cyan lines, with no podium, crown or needle. The surrounding towers are grey-violet with a horizontal "lined paper" far-LOD texture. N: the back of the screen wall (right) reads as a flat striped poster. F: plaza camouflage blots; the bollards are tiny boxes. S: the goal tenement is a tiny dark block at the spire's foot. |
| KS18 koi_pass (money shot) | K: as KS17, and the tail veil is a flat translucent white cut-out. A: 60 % flat magenta (165, 56, 157). B: the towers converge with no crowns; their tops fade into magenta. W: window tiles. |
| KS19 goal_window (money shot) | The best frame of the walk. F: the tenement wall is a flat violet plane with a faint square grid: no brick, stucco, stains, pipes, AC units or cables. The dark windows are flat recessed rectangles with no frames, sills, glass reflections or curtains; the balcony underside is violet-lit and featureless. K: a white band at top right. V: magenta sky. L: the warm window itself works (keep it). |

### Beat stills (G1_r06b)
| Still | What breaks the illusion |
|---|---|
| beat01 start | As KS00: flat walls, floor blots, black-slit doors, cream lamp slabs, flat hanging-sign slabs. V 0.89. |
| beat01 mid | F: the vending machines are plain boxes with flat cyan rectangles, with no buttons, product rows or frame. The awning is a slab; the magenta centre stripe is on the floor; the walls are flat. |
| beat01 end | W: the boulevard facade at the mouth is a confetti wall. L: three rows of identical white shop squares. F: a faint tile grid under the blots. S: the bollards are the only human-scale cue. |
| beat01 look_back | W: windows are floating cards. N: a sign slab carries one stick glyph. B: the AC unit is a cube. F: flat walls. |
| beat01 look_l | F: the clearest "wall, not building" frame. A wall 2 m away is a uniform violet plane (hf 0.96, i.e. dither only), the door is a slab with a cube handle, and the transom is a flat cream emissive rectangle. |
| beat01 look_r | F: the vending screen is a flat cyan rectangle. Where its light grazes the wall it reveals a uniform orange-peel bump stipple that reads as foam or stucco at the wrong scale. |
| beat01 look_up60 | As KS02. |
| beat02 start | As beat01 end. |
| beat02 mid | L/S: the street is empty: no street-level vehicles, lane markings, median or curb detail. W: face B is a uniform noise wall. L: the tram is a black box with one lit cell. F: floor blots. G/K: cotton balls and a sausage. |
| beat02 end | As KS07, with the grazing smear band on the right. |
| beat02 look_back | The best depth frame of the walk (lamp rows, receding towers), but the tower tops dissolve into magenta drip streaks, the far towers are grey "lined paper" and the street is empty. |
| beat02 look_l / look_r | W: the defining confetti frames: flat solid squares in pale blue, teal, magenta and ochre on black, with no frames, mullions, slab edges or spandrels. B: the podium is a dark band with white squares, and the curb a flat strip; no awnings, street-level signage or doors. |
| beat02 look_up60 | As KS05: 46 % magenta, flat blades, no cloud deck visible. |
| beat03 start | As KS07 (identical composition). |
| beat03 mid | W/B: the Gulf faces fill 85 % of the frame with static. Horizontal smear stripes at the right edge from the grazing-angle fade. The balustrade lights read well. |
| beat03 end | F: the gate tunnel is an untextured violet box; the tubes are violet planes. |
| beat03 look_l | B: the Gulf reads as two drums. A: flat violet void with a cross bridge as a hairline; one darker haze band near the horizon is all that remains of the sheets. L: the balustrade lamp is a blue plasma blob with flare. |
| beat03 look_r | As look_l. The far face shows horizontal smear stripes (x 200-320 px), and a curved magenta veil edge hangs in the sky. |
| beat03 look_back | Good depth back down the boulevard, but magenta drip tops and an untextured black box on the right (the terminus). |
| beat03 look_up60 | A: 75 % saturated magenta. Warp-speed rain with orange and cyan sparks; the spire is a translucent tube. |
| beat03 look_down60 | F: the deck is large rectangular tiles with hairline joints, otherwise flat (hf 0.84); puddle blots at the top. |
| beat04 start | F: the tunnel mouth is a violet box and the tubes a violet film. G/K: cotton balls and a white koi band. |
| beat04 mid | F: the glass tubes wash the faces pink, and their ends are capsule "pills". B: no bridge structure (cables, pylons, truss) is visible from the deck; the deck is flat. |
| beat04 end | G: the trunks are smooth purple columns with buttresses and no bark; the crowns are sponges. F: the benches are untextured boxes; violet puddle blots lie on the moss (a wet mask on a lawn). |
| beat04 look_l / look_r | F: the top edge of the tube is a hard horizontal violet band across the frame (y about 180 px). A: flat void. L: orange blobs float in the void. Truss hairlines; smear at the face edges. |
| beat04 look_back | W: faces of static through violet tubes; the boulevard towers in the gap. |
| beat04 look_up60 | A: 60 % magenta; the face tops end at a horizontal fog plane; one crown blob. |
| beat04 look_down60 | F: the deck is a featureless dark slab (hf 1.15), with no glass strip, grating or joints. |
| beat05 start | G: good trunk silhouettes but no bark or moss, and sponge crowns. F: dark lawn with violet blots, box benches. No lanterns or fireflies; the flagstone path does not read. |
| beat05 mid | A: drip tops on the background city. B: a stripe-box building. F: lawn blots. |
| beat05 end | N/B: the sign-wall tower base is a black box with flat cyan and violet stripes. K: a giant white halo ring. F: floor blots. |
| beat05 look_l / look_r | G: a wall of 4-5 untextured trunks. A: flat violet void. F: box benches; no undergrowth or lanterns. |
| beat05 look_back | F: an untextured gate block. W: faces of static. G: sponge crowns. |
| beat05 look_up60 | G: the crowns fill 80 % of the frame with a terry-cloth noise at the wrong scale, with no sky holes and no lantern points; the koi is a white shape behind. |
| beat06 start | B: a stripe box. W: a far curved tower with horizontal smear bands (x 80-300). A: drip tops. |
| beat06 mid | N: the screen wall is smooth pink-to-teal gradient panels. L: a residual reflection boundary in the floor at x about 860 (G4-02 leftover). B: a stripe base and a stripe spire. |
| beat06 end | B/A: the background city is grey "lined paper" towers with drip tops. F: floor blots. |
| beat06 look_l | N: frontal, a screen panel is one solid magenta rectangle, with no LED grid and no content. |
| beat06 look_r | B: the base is a flat cyan band with a violet stripe; the lamps have flat white heads; the planters are boxes. |
| beat06 look_back | B: a stripe base. G: trees. W: large tiles on the distant tower. |
| beat06 look_up60 | N: from below the screen wall reads as pink-teal glass. A: magenta sky. |
| beat07 start | B/A: grey towers with drip tops. F: floor blots. S: tiny bollards on a vast empty plaza. |
| beat07 mid | B: the spire has no podium; the tenement is a dark block at its foot. A: drip tops. F: floor blots. |
| beat07 end | F: the tenement wall is flat; the planter carries an orange-peel noise; the dark windows are flat. L: the warm window works. |
| beat07 look_back | N: the back of the screen wall is a magenta tiled "bathroom wall". B: a stripe box. |
| beat07 look_l | W: a wall of static on the left with a horizontal grazing stripe band. B: drip-top towers. |
| beat07 look_r | B: drip-top towers and a stripe box. |
| beat07 look_up60 | K: the koi is a banana. A: 75 % magenta. Frame luma 103, the brightest in the walk. |

## 3. Ranked issues (top 30, by visual impact across the whole walk)
Categories: palette/atmosphere, facade shader, surface textures, geometry detail, lighting, foliage, koi, signs, post.

| # | Issue (frames) | Category | Concrete fix |
|---|---|---|---|
| 1 | Sky, cloud deck, fog colour and abyss are saturated magenta-violet. Sky about (166, 54, 158); about 75 % of all pixels are violet (every look-up, KS04-KS18, all Edge and bridge frames). | palette/atmosphere | Replace the 14.2.1 atmosphere tokens with a dark near-neutral night and keep the NocAtmosphere functions. Indicative sRGB values (LOOK_V2.md decides):<br>- zenith #050608, mid #0A0D11;<br>- horizon haze #1A1C1F with a warm city-glow lobe #2A241C near the horizon;<br>- cloud underside #14171B, city-lit hot spots #3A3129 (sodium) / #2A3038 (LED);<br>- abyss near #13161A to glow #1C1A17.<br>Sky luma <= 35. Saturation lives only in emission. |
| 2 | Violet cast in every dark: albedo tokens (WetAsphalt, ConcreteDark), Floor #0B0820 and the NightMid/HazeHorizon ambient trilight all carry violet. Measured B-G +35 and R-G +18 codes in the darks (all frames). | palette/atmosphere | Neutral Floor at the same luma, e.g. #0B0B0D. Ambient trilight from the new neutral tokens, with sky fill a cool grey and ground bounce a warm grey. Albedos from photo textures (row 5), not tokens. Remove violet from the fog colour. |
| 3 | Windows are confetti: flat solid squares, about 20 % of lit pixels saturated cyan, magenta or ochre, one level per window, Tetris-like zone clusters (KS03-KS12, KS17-KS18, beat02 look_l/r, most stills). | facade shader | Window colour from a colour-temperature distribution: about 55 % 3 000 K, 30 % 4 000 K, 12 % 5 500-6 500 K, <= 3 % tinted (TV blue, neon spill).<br>Per-window gain x0.4-x1.6 (log-normal); a vertical falloff from the ceiling light inside each window; a blinds/curtain mask on about 40 %.<br>Unlit panes are dark reflective glass (F0 0.04, environment of the new sky), not black.<br>Interior mapping (D-004) on NocFacade within about 300 m. 0.1 m mullions and frames, filtered.<br>Lower the lit fraction toward 0.2-0.25 and break the zone mosaic with per-floor and per-tenant variation. |
| 4 | No building structure: no floor slabs, spandrels, column bays, mullions, balconies, fins, setbacks or crowns; towers read as boxes with wallpaper (all facade frames). | facade shader + geometry detail | A structural layer in NocFacade:<br>- 0.5-0.8 m slab bands and darker spandrel panels per floor, a column line every 2-4 bays;<br>- per-tower facade types (curtain wall, punched concrete, ribbon windows, panel cladding), each with photo-texture detail (albedo, normal, roughness) at near range by triplanar or face UVs, plus a normal/parallax depth for recessed windows.<br>Geometry: make the T0 tier (balconies, AC units, fins within 150 m) actually visible; give stepped setbacks and roof crowns to every tower whose top is seen; add aviation lights on the crowns. |
| 5 | Close-range surfaces carry no material: alley walls, decks, tunnels, gate blocks, tenement, planters, benches (hf 0.84-1.6 codes, dither only; KS00-KS02, KS10, KS12-KS14, KS19, beat01 look_l/r, the down60 stills, heart end). | surface textures | CC0 PBR sets (ambientCG / Poly Haven families: concrete, plaster/stucco, brick, metal plates, painted metal, wood) with triplanar projection, macro variation, grime and drip-streak masks below edges, edge wear from vertex AO, and roughness varied >= 0.1 (8.11).<br>Keep wet-dark albedos: wet concrete at linear 0.06-0.12 (photo albedo x 0.4-0.6 under the wet mask) and recalibrate LightCal once, so frames do not brighten. |
| 6 | The Gulf faces and face B read as walls of TV static, or as drums (KS07, KS09-KS12, beat03, beat04 stills: 70-85 % of the frame). | facade shader + geometry detail | Make the NocSection features dominant at macro scale: slab lines every 4 m, columns every 9 m, glowing atria every 250 m, mechanical bands with amber slits every 300 m. Make the FaceRib pieces (cantilevers, pipe bundles, stair towers) and the 300 m terraces visible, so a face reads as stacked city. Lit density varies by height and district, with dark voids and lit cores. |
| 7 | A structureless sky and cloud deck. Tower tops dissolve upward into flat magenta "dripping paint"; ruler-straight fog lines cut face B (KS04, KS05, KS08, KS17, KS18, beat06 and beat07 stills). | palette/atmosphere | A dark cloud underside with real form: low-frequency fbm/Worley billows with domain warp, lit from below by the city-glow lobes; a visible deck at y 460 with gaps.<br>Towers enter the deck through a height fade that is smooth in y and identical for neighbouring towers, plus wisps around the piercing points. Crowns and red beacons just below the deck, so a tower's end is read before it vanishes.<br>Check why the cloud mix shows vertical streaks: per-tower edge lights or the facade macro surviving the mix. |
| 8 | Wet floor: the puddle mask reads as camouflage blots; reflections are magenta sky; a vertical reflection boundary at the wall-plane switch (KS03-KS07, KS17, almost every still; KS06 x 900-1020, beat06 mid x about 860). | surface textures + lighting | Photo asphalt and paving textures (albedo, normal, roughness, height). Derive puddles from the height map (joints, low spots, curbs) plus small-scale noise: many small irregular puddles, not 10 m blots. Reflection colour from the neutral sky.<br>Blend the H4 wall-plane contributions over about 5 deg around the vanishing line, instead of switching. Keep the D-003 streak lobe. |
| 9 | The koi is an opaque white banana (KS04, KS06, KS07, KS11, KS15, KS17, KS18, beat02-beat07 stills). | koi | Implement the full H6 look:<br>- additive and translucent, with the Fresnel rim brighter than the core;<br>- body clamp about 1.5-2.0, not 2.6, so it no longer clips to a white solid;<br>- Kohaku patches (red-orange, not pink), filtered hex scale lines, slow scanlines;<br>- pectoral and dorsal fins, a head with eye spots and barbel silhouettes, a body taper;<br>- an internal caustic-like variation and a slight cyan-to-white gradient along the body. |
| 10 | Foliage is green sponges: smooth ellipsoid crowns with moss noise, stub branches, saturated green against magenta (KS04, KS07, KS11, KS14, KS15, beat04-beat06). | foliage | Break each crown into hundreds of leaf clusters: displaced lobes or opaque card clusters with filtered cutout-free masks, so the silhouette breaks up. Darker, desaturated foliage albedo (about #1A2620) with LeafGlow only as sparse bioluminescent cells and rims.<br>Branches continue into the crowns; CC0 bark on the trunks; moss and ground textures, shrubs, a flagstone path. The 8 840 lanterns and fireflies (acceptance: >= 25 blobs in KS15). No puddle mask on the lawn. |
| 11 | Scale does not read: one window size and one contrast at every distance, no aerial-perspective layering, no human-scale cues next to big masses (all canyon and Gulf frames). | palette/atmosphere + geometry detail | Neutral height and distance haze strong enough to separate four depth layers: near crisp; mid hazed about 15-25 %; far about 50-70 %; silhouettes beyond.<br>Consistent window and floor pitch, so distance shrinks them visibly. Human-scale props near the masses (doors, railings, signs, lamp posts), traffic streak rivers at several levels, beacons. |
| 12 | The abyss is a flat violet field with no layers, lights or depth (KS09, KS10, KS12, beat03 and beat04 stills). | palette/atmosphere + geometry detail | Dark warm-grey nadir glow. Visible haze sheets as soft bands with lights showing through. Lit traffic layers (8 per spec) and the faces' windows continuing downward and fading. A few light shafts or atria glows in the depths. |
| 13 | Blade and hanging signs are flat coloured slabs with 3-5 random sticks (KS00-KS06, beat01 and beat02 stills). | signs | NocNeon redesign:<br>- a dark housing with an emissive tube border;<br>- glyph rows or columns from a stroke grammar (4-8 strokes per glyph, constant stroke width, consistent baseline);<br>- some backlit lightboxes with printed-looking procedural content;<br>- a light falloff and halo on the housing;<br>- varied sizes and brightness; colour only in the emission. |
| 14 | The wall of signs reads as a daylight glass curtain wall or a solid magenta rectangle (KS16, beat06 mid / look_l / up60, beat07 look_back). | signs | Real LED panels: black off-state, a visible pixel/LED grid (faded below 5 px), bezels, and content from the three slow waves rendered as imagery (soft shapes, gradient fields, pseudo-text bands, product-like silhouettes); luminance varied across panels. Keep the D-010 photosensitivity gates. |
| 15 | Street level is empty: no street-level traffic, lane markings, curbs, awnings, shop signs or doors; the alley has no bins, pipes, cables or steam (KS00-KS07, beat01, beat02). | geometry detail | A T0 instanced clutter kit (AC units, pipes, cables, bins, steam vents, drains); road-marking decals; a storefront module (glass, lit interior, awning, small signs, doors); ground traffic on the boulevard; alley overhead cables and steam (cut order 4 permitting). |
| 16 | Required subjects are unreadable or missing: the KS06 tram (a black box), the KS11 T1 train (absent), KS13 (no glass strip, abyss or T6), the KS15 lanterns (absent). | geometry detail | The tram gets a lit window band, livery, head and tail lights. The trains get lit car windows and nose lights inside the tubes. KS13: verify the glass strip is under the pose and transparent (the frame is 100 % deck). Build the lanterns. |
| 17 | The glass tubes and glass balustrade read as violet tinted film and capsule pills; the tube's top edge draws a hard band across the frame (KS11, KS14, beat03 end, beat04 all). | surface textures + geometry detail | Neutral, dark, reflection-dominated glass (Fresnel, new-sky environment); structural ribs and frames every 6 m; a rain film and streak normal; interior rail lights. The top-edge band comes from the uniform tint: drive alpha by Fresnel only. |
| 18 | Luminaires are flat emissive slabs and points; there are no visible pools of light on walls or floors (all lamp frames). | lighting + geometry detail | Real fixtures (housing and lens). Pools of light become visible once floors and walls are textured; add a short fake light cone in rain per lamp. Colour temperature per kind: sodium about 2 000 K, LED about 4 000 K. |
| 19 | Horizontal smear stripes at grazing angles on facades (beat02 end, beat03 mid / look_r, beat04 look_l, beat06 start, beat07 look_l, KS07). | facade shader | Fade each pattern by its screen-space pitch from both derivatives (\|du/dx\| + \|du/dy\| per axis). At grazing angles fade the floor rows to the macro mean before they alias, or take 4-8 anisotropic taps along the major axis. |
| 20 | Far-LOD towers look like grey "lined paper" with blotchy colour patches (KS17, beat05-beat07 stills). | facade shader | The far macro is the mean window emission on a dark facade, with the structural vertical rhythm and sparse bright points (energy-preserving sub-pixel windows). No grey lift from the fog. Colour from the temperature distribution. |
| 21 | The spire is a straight confetti box with four cyan lines, no podium and no crown; the goal tenement is a tiny block at its foot (KS17, KS18, beat07 mid / up60). | geometry detail + facade shader | A tapering spire with setbacks, ribs and ring lines (14.10 already names "spire ribs, ring lines"); a podium; a crown or needle with beacons; its own material (dark glass with vertical fins, warm emission bands). |
| 22 | Stripe-box buildings (the sign-wall base, beats 5-7) are black boxes with flat cyan and violet stripes. | facade shader + geometry detail | An arcade and shopfront module (glazing, lit interiors, mullions, signage). Make the stripes LED strips in housings at a lower intensity, with varied colour temperature. Clad the box in a photo facade texture. |
| 23 | The grade pushes saturation: +6..+10 on top of the violet Floor and ambient, so the darks are milky violet (all frames). | post | After the palette change: saturation 0..+3, or vibrance applied to emission only; a neutral black point; a slight warm-light / cool-neutral-shadow split. Re-tune the bloom threshold for the darker frame. Keep the one dither and contrast 0 (H1). |
| 24 | Flat emissive patches: the noodle-bar interior, transoms, shop squares and vending screens are uniform clipped rectangles (KS01, KS03, beat01 all, beat02 look_l/r). | surface textures + lighting | Emissive textures with internal structure: interior gradients, shelves and product rows, menu boards, frosted glass. Emission about x0.6 with bloom carrying the brightness, instead of flat clipping. |
| 25 | Trunks and branches are smooth purple columns with pencil stubs (KS14, KS15, beat04 end, beat05 all). | foliage | CC0 bark textures (dark brown-grey) with a normal map; branches tapering into the crowns; root and buttress detail kept; neutral lighting. |
| 26 | Benches, planters, bollards, gate blocks and the tunnel are untextured boxes (KS08, KS10, KS14, beat03 end, beat05, beat06 look_r). | surface textures + geometry detail | Concrete, wood and metal sets with bevels and edge wear; slatted benches; gate blocks with panel joints, signage and lights. |
| 27 | Composition repeats: the koi plus a centred pair of crowns between two symmetric monolith walls appear in about 10 frames (KS04, KS07, KS11, beat02 mid/end, beat03 start/mid, beat04 start/mid/back). | geometry detail | Break the symmetry: vary face-B heights and setbacks left and right, offset the garden crowns, add asymmetric foreground elements (signs, cables, lamp posts) and light sources. |
| 28 | Rain on look-ups reads as a warp-speed starfield with orange and cyan sparks (KS05, KS18, edge / skybridge / heart up60). | post | Tint rain from the scene colour at about x0.3 saturation; drop the coloured sparks outside the garden; shorten and fade the streaks near the zenith (D-005 already fades by projected length). |
| 29 | Rain ripple rings on the decks read as large target decals (KS13, beat03 look_down60). | surface textures | Many small, faint ripples (a flipbook or analytic ring field at 0.1-0.4 m radius) only inside the puddles, with amplitude faded by distance. |
| 30 | Balustrade lights read as blue-violet plasma blobs with lens flares (KS09, KS10, beat03 look_l/r). | lighting | A small fixture, a 4 000 K core, no coloured flare; the halo comes from bloom only. |

## 4. What already works and must be kept
- **The walk's spatial design and its big framings.** The vertical alley slit with fire escapes and signs receding (KS00-KS02); the one-point perspective of the boulevard and its lamp rows (beat02 look_back, beat03 look_back); the converging look-ups (KS05, KS18); the sheer size of the canyon and the Gulf. The geometry is genuinely colossal: texture and light it, do not shrink it.
- **The goal window.** In KS19 and beat07 end, the only warm light in a cold field, seen through the railing with the lamp and the plant, is the one fully working emotional beat. Keep it unique: no other amber near it.
- **The noodle bar's warm spill on the wall and the floor (KS01).** The only good light-on-surface moment; use it as the template for light pools.
- **The wet-floor mechanics (H4, D-003, D-012).** Streak-shaped lobes, no fireflies, reflection fog, the analytic mirror. Change only the puddle mask and the input colours.
- **Rain at eye level (H5, D-005).** Subtle and flicker-free, and it stays out of the dark gradients.
- **H1.** No banding is visible in any frame: the 64-bit HDR, the one TPDF dither in NocGrade and contrast 0 all hold. Keep this pipeline, and re-validate it on the darker palette.
- **The single-source atmosphere.** No horizon seams, no ground, no far-plane cut and no backdrop edge in any of the 51 free-look stills. Keep `NocSkyColor` / `NocFog` / `NocFloor` and change only tokens and terms.
- **Facade filtering at distance.** Apart from the grazing-angle smear (row 19), the window field neither shimmers nor moires in the stills. Keep the footprint fade concept and the stateless toggling.
- **Clean thin geometry.** Railings, trusses and cables (NocLine) are clean and alias-free (KS12, the beat04 looks).
- **The tree trunk silhouettes.** The buttress flare in beat05 start and look_l/r is a good shape; texture it, keep the geometry.
- **Neon as an accent.** The cyan edge lines on the spire and towers (KS17, KS18) and the saturated sign tubes work as small accents. D-027 keeps violet and magenta only there, so they become the punctuation of a dark neutral city.
- **The technical cleanliness of the frames.** No visible NaN, z-fighting or popping (the QA-flagged pixels of G4-12 are
  1 px wall gaps, not defects a viewer sees); the D-008 pipeline settings are sound.
