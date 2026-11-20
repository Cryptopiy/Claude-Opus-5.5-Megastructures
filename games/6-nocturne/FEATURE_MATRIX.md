# FEATURE MATRIX — Nocturne

Honest beat-by-beat status. Tiers per MASTER_PROMPT 14.5: Full, Simplified, Graybox. "Look v2" is the user's directive
D-027: dark neutral palette and real textures. Updated by the lead at every milestone.

**Last update:** 2026-11-16, at the Look v2 acceptance (D-031). Phase 4 wave A is running (D-033). Evidence:
`QA/critique-lookv2-01.md` and the `QA/captures/lookv2_01*` captures.

| Beat | Tier now | Done | Missing or weak (critique id) | Phase 4 owner |
|---|---|---|---|---|
| 1 Alley (s 0-120) | Full geometry, partial content | widths, podia, noodle bar, vending machines, fire escapes, catwalk, cables, textured concrete walls and wet floor, warm pools | flat white window cards and flat sign slabs (INT-06), noodle bar room reads as a bookshop (INT-07), steam vents, AC kit (INT-39), ambient-only walls under LOOK_V2 check 7 (INT-21) | AL (wave B) |
| 2 Boulevard (s 120-400), money shot 1 | Full structure, partial content | slabs with v2 facades, 12 blade signs, lanes with traffic, hero-slab balconies and fins, canyon bridge, terminus | tram T_A missing (GB-11), street level empty and near black (INT-12), podium shop squares (INT-22), weak wet reflections (INT-13), edge-light shimmer (INT-05) | BV, FX (wave A) |
| 3 The Edge (s 400-560), money shot 2 | Full geometry, content open (S1) | tongue and pier plan, Gulf faces with section relief, cross bridges, balustrade | **the abyss is an empty brown field (INT-01, S1)**: traffic layers are single lines, few lit structures below the deck, haze sheets invisible; no highlights (INT-03); KS10 drop 32.4 % against 40 % (INT-09); abyss bands after x264 (INT-15) | AB, AT (wave A) |
| 4 Skybridge (s 560-880) | Full structure, no trains | deck, rails, truss, tubes, gate blocks, glass strip | trains T1-T7 missing (GB-11), KS13 halo cut by glass bands (INT-17), floating lights (INT-19) | TR (wave B), AB lamps |
| 5 Hanging garden (s 880-1 080) | Full trees, no lanterns | 17 lobed leaf-card trees with bark, terrace, path, benches | 8 840 lanterns and 600 fireflies missing; the garden is near black (INT-04, KS15 mean 17.0) | GD (wave B) |
| 6 Wall of signs (s 1 080-1 260) | Simplified in look | slab, promenade, arcade, screen colour waves (Look v2 ramp) | smooth flat panels with no LED structure or imagery (INT-02); back side reads as striped posters; arcade panes flat (INT-34) | SG (wave A) |
| 7 Heart Square (s 1 260-1 500), money shot 3 | Full geometry, partial look | plaza, spire with belts and beacons, ring towers, Old Quarter tenement, goal window and lamp, koi v2 (scales, patches, fins, veil) | koi outline reads as a crescent from below (INT-10); goal window has no frame and the curtain reads as stone (INT-08); plaza featureless (INT-12) | HT (wave A) |
| Audio | Skeleton | DSP, synth, mixer, offline renderer, WAV writer | the score and soundscape of 14.3 (per-beat RMS 17-23 dB under target) | AU (wave A) |
| Transitions, title, ending | Working | black fade-in, beat boundaries per 14.6, goal trigger and ending | final polish in Phase 5 | Phase 5 |

## Verification at the Look v2 gate (INT, 2026-11-16; lead re-run of EditMode)
Validate 103/103; EditMode 361/361; PlayMode 2 passed and 2 skipped (no input devices in batch mode); build has 0 errors
and 0 warnings. Both autotests are exact: cinematic 599.050 / 601.450 s, walk 998.100 / 1002.100 s. The wall bots pass
20/20 at 1.5 and 2.5 m/s, and ray-grid coverage has 0 misses. Perf at 1080p: 274.6 fps, GPU 2.88 ms. At 4K: 105.5 fps,
GPU 9.0 ms. The "no frame above 33 ms" gate fails under external machine load and will be re-measured on a quiet machine
(D-032 8). The 4K CRF-23 encode test is deferred to Phase 6 (D-032 6).
