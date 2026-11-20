# Critique lookv2-01 - Look v2 integration gate (INT)

Build: `Builds/Nocturne` (INT-build02, 2026-11-16 04:14, 0 errors / 0 warnings, 462.5 MB) on the integrated tree of all eight
Look v2 packages (TP, TQ, PA, KO, FA, SU, FO, GE) plus INT's integration changes (docs/logs/INT.md). Default flags:
`-facade-v 2` (switched by INT at this gate: FA-5, proposed decision D-031 in the INT report), `-surf-v 2`, `-foliage-v 2`, relief on (follows -facade-v), prepass on.

Captures (all looked at; see "What I looked at"):
- `QA/captures/lookv2_01` keyshots1080 (20) and `lookv2_01_ksv1` (the same build with `-facade-v 1`, the old default, for the A/B);
- `lookv2_01_4k` (20 x 3840 x 2160; money shots KS04, KS05, KS09, KS10, KS17, KS18, KS19 kept);
- `lookv2_01b` beats (51 stills incl. free look), `lookv2_01tc` texclose (8), `lookv2_01ms` material sheet (sheetL, sheetW),
  `lookv2_01fs` facade sheet (27);
- sequences (frames deleted after analysis, reports kept): statics, texpans + `-capture-ss 2` reference, dolly, koi, rain,
  boundaries (9 windows), encode clips; montages in `lookv2_01seq`.

Severity: S1 breaks the illusion in a money shot or blocks a gate; S2 major (visible to a viewer, or a gate fails); S3 minor.
Status: open / partly / fixed / QA (a measurement issue, not a picture defect).

## Verdict
Look v2 does what the user asked in its two main points. The violet/magenta is gone from every frame (R1 violet <= 0.03 % in
20 / 20 key shots against 17-81 % in v1; dark cast B - G -1.3..+2.6 codes against +27..+40), and every building, wall, deck and
floor now carries a real material: punched concrete, precast, brick and glass skins with mullions, recesses, slab lines and
lit rooms on the towers; cast concrete, brick, granite pavers, diamond plate, wood and bark near the eye. With the v2 facades
as the default the canyon finally reads as buildings rather than confetti (lookv2_01 vs lookv2_01_ksv1, KS04). The koi reads
as a translucent scaled Kohaku hologram, the garden as leaf crowns, the goal window as the one warm point.

It is not "perfect" yet. The biggest gaps are content and light, not texture:
- the abyss of beats 3-4 is a flat brown field (money shot 2 has no vertigo);
- the wall of signs and its back are smooth flat panels (no LED, no imagery);
- the garden and the Edge / bridge shots have almost no highlights (p99 44-93 codes);
- street level and the plazas are empty (no tram, trains, lanterns, clutter: Phase 4 content);
- the alley still shows flat white window cards and flat hanging-sign slabs;
- the tower-corner edge lights shimmer in pans.

Counts: **S1 1, S2 15, S3 26** (plus 7 QA items, 2 of them fixed by INT).

## Gate results (numbers in docs/logs/INT.md)
| Gate | Result |
|---|---|
| fastcheck | runtime 177 / editor 234 OK, 0 warnings (4 CS0618 fixed) |
| RebuildAll | SUCCESS, 0 problems, 40 / 40 materials |
| Validate | PASS 103 / 103; R14 X3570 / X4000 gate now matches Unity's message text; 0 shader warnings |
| EditMode | 361 / 361 (INT-tall01 v1 default; INT-tall02 v2 default incl. VisibilityAuditTests 10 / 10, Q1IntegrationTests, ReliefTests) |
| PlayMode | 4: 2 passed, 2 skipped (no keyboard / mouse in batch mode, as in Q1), 0 failed |
| BuildWindows | SUCCESS 0 / 0, 462.5 MB |
| smoke (brisk until 400) | PASS, beats 1-3 at t 0.02 / 48.17 / 160.17, coverage 48 204 rays 0 misses, GC 33.5 B/frame |
| autotest cinematic 2.5 m/s | PASS 21 / 21, trigger 599.050 s, stop 601.450 s, ending 17.00 s, crossings exact, roll 5e-6 deg, GC 32.2 B/frame, load 1.78 s |
| autotest walk 1.5 m/s | PASS 21 / 21, trigger 998.100 s, stop 1002.100 s, crossings exact, roll 0, GC 31.7 B/frame |
| wall bot 1.5 / 2.5 m/s | PASS 20 / 20 both; max excess -295.7 / -292.8 mm, edge followed 99.8 %, lane reach 28 / 28 at 0.0 mm, feet never below y 0 |
| ray-grid coverage | 48 204 rays, 0 misses in every run |
| VisibilityAuditTests | 10 / 10 (pre-reveal koi hidden, KS19 subjects, T-DROP); KS10 built drop 32.4 % (INT-09) |
| statics (-rain 0) | PASS 7 / 7, max pixel change 0.8 level |
| texpans vs ss2 | swimming p95 0.10-0.31 px PASS; R1 shimmer FAIL KS04 7.21, KS05 6.71, KS00 2.77, KS17 2.42 / 2.88, KS08 2.17 (INT-05, 26, 27) |
| dolly | luma step 0.39 / 0.14 levels PASS, 0 spikes, pops WARN only at first / last frames (parallax) |
| koi / rain | PASS (step 0.018 / 0.026; rain outside-streak 2 levels) |
| boundaries (9) | luma step max 0.81, 0 spikes; activation excess 0.54 at b67 / s1240 = trend of the C3 turn, no pop (QA-7) |
| encode x264 CRF 23, 1080p | step gate: KS02 slit 0, KS04 sky 0, KS19 0 PASS; **KS10 abyss 5.13 / 1000 FAIL** (INT-15); BandScore fails on KS02 1.45 / KS04 2.99; KS00 / KS17 rects measure texture (QA) |
| lookv2_check (LOOK_V2 8.2) | 8 / 20 shots pass (v1 facades: 6 / 20); R1 violet <= 0.03 % in 20 / 20 |
| tiling.py | 0 FAIL (key shots, texclose) |
| facade sheet | wall micro 2.27 codes at 75-300 m; floor-line gate 89 failures (INT-25) |
| perf 1080 (median of 3) | 274.6 fps avg, GPU 2.88 ms avg / 5.59 p99 (budget 8.2) PASS; frames > 33 ms 1 178 - external machine load (INT-41) |
| perf 4K | 105.5 fps, GPU 9.0 avg / 16.56 p99 (budget 16.5) |
| VRAM | 1080 player 663 MiB (+886 total), 4K 1 622 MiB (+2 337); textures 374.7 MiB |

## Top 15 (ranked by what a viewer sees)
1. INT-01 S1 - the abyss is an empty brown field (KS09 / KS10 / KS12 / KS13, money shot 2).
2. INT-02 S2 - the wall of signs and its back are smooth flat panels (KS16, KS17 right, beat 6-7 stills).
3. INT-03 S2 - beats 3-5 have no highlights (p99 44-93): the Edge, bridge and garden read dull.
4. INT-04 S2 - the garden is near black (KS15 mean 17.0, no lanterns).
5. INT-12 S2 - empty street level and plazas; no tram, trains, lanterns or clutter.
6. INT-15 S2 - the abyss haze bands after x264 CRF 23 (5.13 steps / 1000 at 1080p).
7. INT-11 S2 - the cloud deck shows little form in the look-ups; tower tops dissolve without crowns.
8. INT-06 S2 - the alley keeps flat white window cards and flat sign slabs.
9. INT-05 S2 - tower-corner edge lights stair-step and crawl in pans (7.2 / 6.7 levels over ss2).
10. INT-08 S2 - the goal window has no frame; the curtain reads as stone (money shot 3).
11. INT-07 S2 - the noodle bar room reads as a pastel bookshop.
12. INT-10 S2 - the koi's outline from below is a crescent.
13. INT-09 S2 - KS10's deck fills half the frame (drop 32.4 %).
14. INT-14 S2 - face B / the Gulf read as a star field at KS07 / KS11; moire behind the KS09 bridge.
15. INT-13 S2 - the wet boulevard mirrors little of the city.
(INT-41, the 33 ms frame gate, fails from external machine load and needs a quiet re-measure.)

## Findings

| ID | Sev | Beat | Description | Evidence (file, region px) | Cause | Fix | Status |
|---|---|---|---|---|---|---|---|
| INT-01 | S1 | 3-4 | The abyss is a flat brown field: no lit layers, no traffic far below, no visible haze sheets, no glowing depths. Money shot 2 (KS09, KS10) and KS12 / KS13 read as "fog behind a railing", not a 2 km drop. p99 79 / 49 / 78 / 95 (LOOK_V2: drop 50-66 with red / white traffic >= 25 codes over the abyss, p99 >= 120). The two white layer lines read as laser beams. | lookv2_01/KS09 (420-1060, 230-720), KS10 (760-1400, 0-520), KS12 (480-1300, 190-740), beat03_edge_look_l (800-1380, 500-800) | H3 content: the -120 / -520 traffic layers are single NocLine strokes; the lower faces fade into the haze; the sheet modulation is invisible at this exposure | Phase 4 abyss pass: dense traffic streams (heads / tails) on every layer, lit lower faces and atria continuing downward, visible sheet bands, a brighter umber nadir; replace the white layer lines | open (lead / Phase 4) |
| INT-02 | S2 | 6, 7 | The wall of signs is smooth gradient panels with thick seams (no LED grid, no imagery, no emissive depth); its back side and the screen-wall block read as flat striped posters (white / teal bands). | lookv2_01/KS16 (300-1500, 0-1000); KS17 right (1300-1920, 0-900); beat06_signs_look_r, beat07_heart_look_back | CRIT0 #14 / #22 not in any Look v2 package (palette only) | Phase 4 signs redesign (LED panels with pixel grid faded below 5 px, bezels, imagery; back side as a lit structure) | open |
| INT-03 | S2 | 3-5 | Beats 3-5 have almost no highlights: p99 KS07 83.9, KS08 92.1, KS09 79.4, KS10 49.4, KS11 92.9, KS12 78.1, KS14 77.2, KS15 44.4 (>= 120 asked). The frames read dull and flat. | lookv2_lookv2_01.json | v2 windows are ~100 codes by design (LOOK_V2 4); few lamps; no traffic heads, beacons or lanterns near | lamp halos on the tongue / bridge, traffic streams (INT-01), Phase 4 lanterns; or the lead re-derives the p99 rule for the v2 window level | open |
| INT-04 | S2 | 5 | The garden is too dark: KS15 mean 17.0 (24-42), crush 28 %; crown undersides 14-15 codes; beat-5 stills nearly black (trunks as silhouettes, black lawn). | lookv2_01/KS15 whole frame; lookv2_01b/beat05_* | FO-04: leaf lights 20 m below the crowns at intensity 1.0; no lanterns (Phase 4); v2 facades darkened the east block behind | PA: lantern / under-crown light; Phase 4 lanterns (8 840) | open |
| INT-05 | S2 | 2-4 | Tower-corner edge lights (white + cyan 3 px lines) stair-step and crawl in pans: R1 shimmer over the ss2 reference KS04 7.21, KS05 6.71 levels (limit 2.0). | QA/reports/sequences_lookv2_01tp.md; QA/reports/INT_texpan_ks04_block.png (block 256, 320) | HDR-bright thin lines resolved by MSAA before the tonemap; the 1.5 px box is not enough at 3 px | FA-D19: fade the band to its mean below ~4 px, or pre-tonemap the edge band (Karis-weighted) | open (FA) |
| INT-06 | S2 | 1 | The alley still shows flat white / cream window cards floating on the walls and flat hanging-sign slabs (teal, grey) with stick glyphs; the bulkhead lamp is a flat two-tone slab. | lookv2_01/KS00 (780-1300, 0-500), KS01 (1030-1460, 0-450), KS02 (520-1400, 0-600) | G1's alley window quads and signs were outside every Look v2 package | Phase 4 alley pass: framed windows with rooms (FA's NocInterior) or NocFacade on the alley blocks; sign housings like the blades | open |
| INT-07 | S2 | 1 | The noodle bar's room behind the counter reads as a pastel bookshop (rows of coloured "book spines"), flat and bright; KS01's key subject "bar window interior" is not a noodle bar. | lookv2_01tc/tc_noodle_bar whole frame; KS01 (810-940, 550-740) | SU's NocShopfront product rows (box rooms with shelves) on the bar slot | a bar interior: tiled back wall, menu boards, steam, a darker room with a warm pool; product rows only on shops | open (SU) |
| INT-08 | S2 | 7 | The goal window has no frame or sash; the curtain reads as a stone slab (pale marbled texture), the room has no depth. Money shot 3's subject is the window. | lookv2_01_4k/KS19 (1620-1880, 1080-1370); lookv2_01/KS19 (800-940, 540-700) | SU's Fabric row on a flat quad; OldQuarterBuilder opening without a frame | window frame and sash, fabric folds (normal), a visible room behind (lamp table, wall) | open (SU) |
| INT-09 | S2 | 3 | KS10 composition: the deck fills the lower half; built drop share 32.4 % (40 % asked) | visibility.md KS10 (built 3/50/5/10/32); lookv2_01/KS10 | gate block A and pylons (GE-18) | lead: GB-04 re-arbitration (pitch -36 or a lower deck edge) | open (lead) |
| INT-10 | S2 | 7 | The koi's outline from below is a crescent (orbit arc), pectorals are slivers | lookv2_01/KS18; lookv2_01seq/koi_ks18_montage.png | KoiMath spine on the 260 m orbit (KO-12 / KO-13) | lead: D-KO-5 (S-curved spine or larger radius, splayed pectorals) | open (lead / V) |
| INT-11 | S2 | 2, 4, 7 | Sky and cloud deck show little form in the look-ups: KS04 / KS05 / KS18 lids are a flat warm-grey (KS05) or dark grey (KS18) field; tower tops dissolve without crowns. Billows read only in beat03_edge_look_up60. | lookv2_01/KS05 (580-1340, 0-470), KS18 (1100-1900, 0-500) | cloud tint amplitude 1.0 gives +-3.5 codes; no crowns below the cloud base | PA: stronger low-frequency billow contrast under the glows; crowns / beacons where tops are seen | partly |
| INT-12 | S2 | 2, 7 | Street level and the plazas are empty: no tram (KS06), no train (KS11), no lanterns, no clutter; the Heart plaza is half the frame of featureless dark granite (beat07 mid / start), the boulevard street is near black in KS04's lower third. | lookv2_01/KS04 (0-1920, 880-1080), KS06, KS11; lookv2_01b/beat07_heart_mid | Phase 4 content (CRIT0 #15 / #16, GB-11) | Phase 4 | open |
| INT-13 | S2 | 2 | The wet boulevard mirrors little of the city; lamp streaks on the street are faint | lookv2_01/KS03 (150-1750, 780-1080), KS04 (0-1920, 880-1080) | SU-05: the wall-plane macro is FA's true dark-glass mean | lead decision on the wall-plane gain (SU-05) | open (lead) |
| INT-14 | S2 | 3, 5 | Face B / the Gulf faces still read as a star field of windows at KS07 / KS11 (section grid, sparse lit cells); moving moire on the Gulf face behind the KS09 cross bridge (texpan swimming max 2.67 px) | lookv2_01/KS07, KS11; sequences_lookv2_01tp.md texpan_ks09 [1088, 448] | section-mode lit cells at 2-5 px; TQ-06 | stronger district / void structure on section faces; FA to check the KS09 moire with v2 | partly |
| INT-15 | S2 | 3, 1 | H1 after x264 CRF 23 at 1080p: the KS10 abyss clip bands, 5.13 visible steps / 1000 px (limit 2; max 1.04 code), BandScore 9.06 (source 0 steps); the KS04 sky clip scores 2.99; the KS02 slit passes the step gate (0 steps) with BandScore 2.17 (PA-09). The 4K CRF-23 test was not run (disk). | QA/reports/encode_test_lookv2_01_enc_beat03_abyss.md, _enc_beat01_slit.md | an empty smooth dark haze is the hardest case for x264 (H1); the static TPDF dither is smoothed by the encoder | give the abyss content (INT-01) and texture in the haze; run the 4K master test (D-007 recommends a 2160p60 upload) before the dither ladder | open |
| INT-16 | S3 | 2 | The upper tube seen end-on along the boulevard reads as a translucent searchlight wedge from the canyon bridge to the face-B portal ring | lookv2_01/KS04 (920-1000, 380-740); lookv2_01_4k/KS04 | Fresnel glass at grazing over 600 m; ribs too thin to resolve | darker tube interior, visible ribs / frames, or a lit strip inside | open (SU / GE) |
| INT-17 | S3 | 4 | The KS13 lamp halo seen through the glass strip is cut by horizontal bands | lookv2_01/KS13 (1150-1330, 680-760) | the glass strip's grime / rivulet layer over the additive halo | fade the glass detail on bright backgrounds or draw halos after glass | open (SU / PA) |
| INT-18 | S3 | 3 | Balustrade lamps are white blobs with a diagonal flare spike | lookv2_01b/beat03_edge_look_l (980-1100, 560-780); KS09 right edge | NocHalos kind 3 + the lamp lens; CRIT0 #30 | fixture housing, softer halo, no spike | partly |
| INT-19 | S3 | 4 | Lights float in mid-air over the void (no mast): KS12 white blob (700, 375), orange (905, 170) | lookv2_01/KS12 | aviation / bridge lights on hidden supports | masts or remove | open |
| INT-20 | S3 | 1 | Beat-1 darks lean warm: R - G +4.67 (KS00), +4.94 (KS01) on luma 12-40 (limit 4) | lookv2_lookv2_01.json | warm light pools by design (PA-05) | lead: allow |R - G| <= 6 in beat 1 (PA's proposal) | open (lead) |
| INT-21 | S3 | 1, 7 | Ambient-only walls fall under LOOK_V2 check 7: KS00 micro 1.35 / meso 0.48-0.91, KS01 1.30 / 0.39, KS02 1.19-1.39 / 0.33-0.36, KS19 meso 0.62; KS00 / KS02 pool share 0.33-0.36 / 0.0-0.01 (>= 0.40) | lookv2_lookv2_01.json texture_* | trilight-only light shows albedo contrast only (SU-12); KS02 walls have no pools | more pools (bulkheads, window spill) on the KS02 walls; or the lead lowers check 7 for ambient walls | open |
| INT-22 | S3 | 2 | Boulevard podium shopfronts are rows of identical flat white squares, the brightest thing in KS03 / KS04 | lookv2_01/KS03 (700-1700, 740-770), KS04 (30-480, 850-890) | G1 EmitShop gains (FA-D11) | NocShopfront rooms at 0.6 x on the podium slots, varied | open (SU) |
| INT-23 | S3 | 2 | The podium stone base and walls near the eye at 1.5 m are near black (texclose podium, alley wall, bridge deck, trunk 10-15 codes) | lookv2_01tc/* | no light within range (FA-D14, TQ-04) | pools from the podium lamps | open |
| INT-24 | S3 | 2-7 | Lit window panes at 25 m read as flat beige cards with one diagonal shade | lookv2_01seq/dolly_face30_montage.png (frame 299) | box room at close range | FA: textured rooms (RoomArr) at < 50 m | open (FA) |
| INT-25 | S3 | all | Floor-line autocorrelation fails on 89 sheet faces (54 at 1 km where floors are 4-5 px, 35 at 30-300 m at 45-75 deg) | QA/reports/facade_sheet_lookv2_01fs.md | H2 per-axis fade vs the 4 px gate (FA-D18) | lead: gate at 6 px or slab lines to 3 px | open (FA / lead) |
| INT-26 | S3 | 7 | Ring-tower windows crawl in pans (KS17 2.42 / stabilised 2.88) | sequences_lookv2_01tp.md texpan_ks17 | 2.5-5.5 px H2 fade band (FA-D20) | FA crawl sweeps | open (FA) |
| INT-27 | S3 | 1, 3 | texpan KS00 2.77 (G1 sign edge), KS08 stabilised 2.17 (lamp pole) | sequences_lookv2_01tp.md | thin bright edges | sign housing filtering | open |
| INT-28 | S3 | 2-7 | Symmetric compositions repeat: two garden crowns centred between face-B walls in KS04, KS07, KS11, beat02 / beat03 / beat04 stills | lookv2_01/KS07, KS11 | CRIT0 #27 | asymmetric foreground elements | open |
| INT-29 | S3 | 2 | Aerial traffic tails are red dots over the lid (sparks) | lookv2_01/KS04 (800-1200, 0-200), KS05 | PA-07 accepted accent | - | accepted |
| INT-30 | S3 | 2 | Balconies and fins on the hero slabs barely read at KS04 | lookv2_01_4k/KS04 | GE-16 | - | accepted |
| INT-31 | S3 | 7 | Dark single-pixel pits on the tenement brick near lights (NaN-like 8) | lookv2_01/KS19 (775, 558), (864, 692), (248-397, 815-898) | real mortar pits at 1 texel / px (SU-15) | brick mip bias or a pit floor | open |
| INT-32 | S3 | 7 | Spire strips and red beacons trip the isolated-pixel test (KS18 73) | crop of KS18 (1180-1440, 640-1000) | thin emissive NocLine strips, beacons | QA: add NocLine / beacon masks | QA |
| INT-33 | S3 | 2-4 | Traffic heads trip the NaN-like test (KS04 5, KS05 6, KS07 4, KS11 1) | lookv2_01/KS04 (870-890, 520-630) | sub-pixel bright vehicles | QA: lane masks | QA |
| INT-34 | S3 | 6 | KS16 shop arcade under the wall: flat lit panes (blue-grey, amber) | lookv2_01/KS16 (300-1500, 840-1080) | GE's SignBaseRelief with G1 emissive panes | NocShopfront rooms | open |
| INT-35 | S3 | 2 | Terminus hall walls flat white through its windows | GE_r05p_terminus | GE-21 | dimmer, varied hall light | open |
| INT-36 | S3 | 5 | Beat-5 canopy GPU about +1.15 ms on the canopy views (budget 1.0) | FO_perf_r05 | FO-07 | card LOD | open (FO) |
| INT-37 | S3 | 3 | Face B at grazing: unresolved rows as grey streaks | FA_01v2 KS09 | FA-D04 | sparser lit groups | open (FA) |
| INT-38 | S3 | 4 | Catwalk grating from below and the glass-strip frame read as flat bands | SU_r01v2 KS02, KS13 | SU-03 / SU-04 | bevel highlight | open |
| INT-39 | S3 | 1 | The AC unit in KS02 is an untextured dark cube | lookv2_01/KS02 (600-730, 390-550) | G1 prop outside the packages | AC kit (T0) | open |
| INT-40 | S3 | 4 | The glass tubes at the garden gate read as milky grey plastic capsules; their bulging end caps are opaque-looking pills (CRIT0 #17 "capsule pills") | lookv2_01/KS14 (0-660, 380-1000), (1260-1920, 380-900) | Fresnel-only GlassDark at grazing over a dark background; the end-cap geometry | darker interior, end rings / frames instead of domes | open (SU / G3 geometry) |
| INT-41 | S2 | all | Frame-time gate "no frame above 33 ms" fails: 1 100-2 140 frames per 600 s at 1080p (max 1 049 ms), 490 at 4K, spread evenly over the walk | QA/reports/INT_perf1080_r1..r3.csv, INT_perf4k.csv | not the game: main thread 1.0-1.5 ms and GPU 2-4.5 ms in the slow frames; other sessions' ffmpeg x3 held ~65 % of the CPU (gpu-guard guards the GPU only); A1 on a quiet machine: 1 frame | re-measure on a quiet machine before Phase 7 (lead) | open (lead) |
| INT-42 | S3 | 2 | Red aerial tail lights smear into dark-red blobs after x264 4:2:0 (KS04 sky clip) | QA/reports/encode_test_lookv2_01_enc_beat02_axis.md (crops looked at, deleted) | chroma subsampling of 1-2 px saturated red points | 2-3 px tails or desaturated tails | open (PA) |
| QA-1 | S3 | 3 | lookv2_check separation box of KS07 (sky 0.445-0.555 x 0.01-0.18) lies on face B (no sky left in KS07): separation 1.1 is meaningless | lookv2_01/KS07 top centre | GE's masses (TQ already dropped the KS07 banding sky) | PA: drop or move the KS07 box | QA |
| QA-2 | S3 | 1 | image_metrics banding rects KS00 mouth_glow and KS03 alley_wall_dark now hold signs, windows and lamps (BandScore 1.93 / 2.27, 0 visible steps) | crop lookv2_01/KS00 (880-1040, 400-560) x3 | geometry newer than the rects | re-place on a gradient | QA |
| QA-3 | S3 | 4, 6 | KS13 abyss and KS16 wall_panel rects lay on the strip's beam / GE's arcade | regions_default.json | GE's new geometry | moved by INT (PASS now) | fixed |
| QA-4 | S3 | - | Validate's R14 gate never fired (Unity drops the fxc codes); two X4000 passed | Logs/INT-validate01.log | text match on "X4000" only | gate matches the message text; FA / FO single-return fixes | fixed |
| QA-5 | S3 | - | run-player does not route texpans / dolly / -capture-ss 2 (TQ's REQUEST LEAD) | docs/logs/TQ.md 44 | lead file | carry with TQ's ShotPlan / QaTests edits | open (lead) |
| QA-7 | S3 | 7 | Boundaries b67 / s1240 "activation-frame diff" 0.543 / 0.538 (limit 0.5) with no event: the per-frame diff rises smoothly through the C3 turn (frames 299 / 300 / 301: 4.00 / 3.99 / 4.05) and the excess is taken against the median of the previous frames; harness excess 0.206 | QA/reports/sequences_lookv2_01bd_seq_b67.md, sequences_lookv2_01bd67_v1.md (v1 0.117 PASS) | trend lag of the measure (the textured v2 turn has a steeper diff curve) | sequences.py: excess against a centred median | QA |
| QA-6 | S3 | - | The KS17 plaza banding rect scores 2.0 with 0 visible steps (granite macro patches) | lookv2_01/KS17 plaza (x3 gain crop) | GB-10 / texture, not banding | gate on visible steps only for textured floors | QA |

## CRIT0 (QA/critique-lookv2-baseline.md) - the 30 ranked issues
| # | Issue | Status | Evidence |
|---|---|---|---|
| 1 | magenta sky, deck, fog, abyss | fixed | R1a <= 0.03 % in 20 / 20; sky tops 39.7-68.3 codes, sat <= 0.30 |
| 2 | violet cast in the darks | fixed | B - G -1.3..+2.6 (v1 +35); beat-1 R - G +4.7 / +4.9 warm (INT-20) |
| 3 | confetti windows | partly: fixed on every tower (v2 default), open in the alley and the podium shops | KS04 v1 vs v2; INT-06, INT-22 |
| 4 | no building structure | partly | slabs, mullions, recesses, bays, hero-slab balconies / fins, Gulf ribs; no crowns, few setbacks |
| 5 | close surfaces without material | fixed (ambient walls under check 7: INT-21) | texclose, KS00 / KS19 crops, material sheet |
| 6 | Gulf / face B as static | partly | section bands, atria, relief visible at 4K; star field at KS07 / KS11 (INT-14) |
| 7 | structureless sky, dripping tops | partly | neutral deck, faint billows; tops dissolve (INT-11) |
| 8 | camouflage puddles, reflection seam | fixed (reflections weak: INT-13) | KS17 plaza, KS07 tongue streaks |
| 9 | koi banana | partly | scales, patches, head, eyes, barbels, fins, veil; crescent outline (INT-10) |
| 10 | foliage sponges | partly | lobed leaf-card crowns, bark; far too dark, no lanterns (INT-04) |
| 11 | scale does not read | partly | consistent window pitch, haze layering; abyss and plazas still empty |
| 12 | flat abyss | open | INT-01 |
| 13 | flat sign slabs | partly | blades: housings, strokes, 6 neutral colours; alley signs flat (INT-06) |
| 14 | wall of signs as glass / magenta rectangle | open | INT-02 |
| 15 | empty street level | open | INT-12 (GE's sign-base arcade and terminus hall only) |
| 16 | required subjects missing | partly | KS13 strip fixed; tram, T1, lanterns open (INT-12) |
| 17 | violet glass tubes | fixed | dark Fresnel glass with ribs (INT-16 end-on wedge) |
| 18 | flat luminaires, no pools | partly | halos, pools on the alley walls; fixtures still flat slabs |
| 19 | grazing smear stripes | partly | FA-D04 grey rows at grazing (INT-37) |
| 20 | far towers as lined paper | fixed | KS17 / beat07 towers read as dark buildings with sparse light |
| 21 | spire as confetti box | partly | belts, ring lines, beacons, steel base; no podium or crown |
| 22 | stripe-box buildings | partly | arcade at the sign-wall base; back side stripes (INT-02) |
| 23 | grade saturation | fixed | R3 0.09-0.21 |
| 24 | flat emissive patches | partly | shop rooms exist but read as a bookshop (INT-07); podium squares (INT-22) |
| 25 | smooth trunks | fixed | bark UV0, curved limbs (beat05 stills) |
| 26 | untextured boxes | fixed (benches still plain boxes) | planters / gate textured (KS08, KS14) |
| 27 | repeated symmetric composition | open | INT-28 |
| 28 | rain starfield with sparks | partly | rain tinted; red aerial tails remain (INT-29, accepted) |
| 29 | ripple target decals | fixed | rings only inside puddles (KS13 deck clean) |
| 30 | balustrade plasma blobs with flare | partly | white blob with a flare spike (INT-18) |

Totals: fixed 10, partly 16, open 4 (#12 abyss, #14 wall of signs, #15 street level, #27 symmetric compositions).

## Phase 3 gate findings GB-01..GB-20 (QA/critique-graybox01.md)
| ID | Status | Evidence |
|---|---|---|
| GB-01 KS19 koi head | fixed (D-029 arbitration) | VisibilityAuditTests.KS19SubjectsAreWindowLampAndKoiBody PASS |
| GB-02 upper tube in mid-air | fixed | canyon bridge + keystone portals; ReliefTests.UpperTube_EndsInsidePortals PASS (INT-16 end-on look) |
| GB-03 koi through the notch | fixed | PreRevealKoiIsHidden PASS (0 sample-phases) |
| GB-04 KS10 drop share | partly | gaze keys moved; built drop 32.4 % < 40 % (INT-09) |
| GB-05 KS13 strip | fixed | strip open, KS13 mean 36.4 in 28-48 |
| GB-06 lanes in open air / crowns | fixed | Lanes_EndInsideStructure, CrownClearance PASS |
| GB-07 exposure windows | partly | beat 1 inside the LOOK_V2 windows; KS15 17.0, KS10 28.6, KS12 29.5 under their KS windows |
| GB-08 NaN-like bay gaps | fixed with v2 (remaining flags: traffic, strips, brick pits: INT-31..33) | image_metrics_lookv2_01 |
| GB-09 isolated pixels | QA open | INT-32 / 33 |
| GB-10 KS17 plaza banding | QA open (0 visible steps) | QA-6 |
| GB-11 tram, train absent | open (Phase 4) | INT-12 |
| GB-12 KS02 catwalk | fixed | visibility KS02 5 / 5 |
| GB-13 KS03 / KS13 s | fixed | KS03 s 117, KS13 s 718 in visibility.md |
| GB-14 floating plaza | fixed | GE's east block reaches the plaza (ReliefTests) |
| GB-15 face-B back blend | fixed | three setbacks |
| GB-16 rails proud | fixed | 8 mm head, Q1IntegrationTests strict |
| GB-17 barrier gap | fixed | planters from x 214 |
| GB-18 balcony slab / core hue | fixed | slab y 3.4-3.6, core hue 32.7 |
| GB-19 -autotest -beat N | fixed | GE's AutoTestRunner |
| GB-20 terminus east face | partly | windows into a lit hall; flat white walls (INT-35) |

Totals: fixed 14, partly 3 (GB-04, GB-07, GB-20), open 3 (GB-09 and GB-10 as QA items, GB-11 Phase 4).

## What I looked at
- Every key shot of lookv2_01 at full size (KS00-KS19; KS06 and KS14 on the sheet and as crops), the v1 run's KS04 beside v2;
  crops: KS00 lamp wall and slot, KS13 lamp halo and deck plate, KS18 strips, KS16 regions, KS04 NaN-like pixels and the tube
  wedge (v1 / v2), KS17 plaza x3, KS00 mouth x3, KS13 abyss x3.
- 4K: KS04 left slab and face B at 1:1, KS17 koi head, KS19 window (all 20 4K frames exist; 13 deleted after the look).
- Beats: the three contact sheets, beat03_edge_look_l full size. Texclose: contact, tc_noodle_bar full size. Material sheet
  sheetL. Facade sheet: contact sheet 1. Sequences: texpan KS04 worst block over 4 frames, dolly pop frames (face30 296-299,
  graze 0-3), koi KS18 montage, b67 per-frame diffs (numbers), the KS04 sky encode crops (100 / 200 % / x8).
- Also full size: KS06, KS14, beat02_boulevard_look_l, beat04_skybridge_look_down60; the 4K KS10 abyss at x2 gain.
