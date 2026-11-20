# OPUS 5.5 FINAL REPORT — Nocturne (game 6) — DRAFT

> **Draft, kept current for a safe landing (MASTER_PROMPT Part 2.4).** Last refresh: 2026-11-16, at the start of Phase 4
> wave A. The run is not finished. `FEATURE_MATRIX.md` holds the honest status of every beat.

## What was built
- **Game.** A first-person walk of 1 500 m through a rain-night megacity with no ground. The path crosses seven beats:
  alley, boulevard canyon, the Edge, skybridge, hanging garden, wall of signs and Heart Square. It takes 600 s at the
  constant cinematic 2.5 m/s and 1 000 s at 1.5 m/s, and ends at one warm window under a 900 m hologram koi.
- **Engine and pipeline.** Unity 6000.6.0f1, URP 17.6 Render Graph, Forward+, MSAA 4x, 64-bit HDR, D3D11, no shadow maps.
  Geometry, shaders and audio are all procedural. The framework was imported from games 1-3 (Part 3.1), and every Unity
  and player call runs through mutex-serialised queues shared with the parallel games.
- **Look v2** (the user's directive D-027, accepted in D-031):
  - a dark neutral palette with no violet;
  - analytic window facades with photographic CC0 skins, recesses, interiors and section districts;
  - textured near surfaces with an analytic wet-floor mirror;
  - lobed leaf-card trees;
  - a scaled Kohaku koi hologram;
  - relief and gate structures;
  - a tinted atmosphere with fog, haze and cloud;
  - a Floor-relative grade with a static TPDF dither.
- **Phase 4 (in progress, D-033):** per-beat content and the score (see `STATE.md`).

## QA evidence (at the Look v2 gate)
| Check | Result |
|---|---|
| Validate | 103 / 103 |
| EditMode | 361 / 361 (lead re-run on HEAD) |
| PlayMode | 2 passed, 2 skipped (no input devices in batch mode) |
| Build | 0 errors / 0 warnings |
| Autotest, cinematic 2.5 m/s | PASS, trigger 599.050 s, stop 601.450 s |
| Autotest, walk 1.5 m/s | PASS, trigger 998.100 s, stop 1002.100 s |
| Wall bots 1.5 / 2.5 m/s | PASS 20 / 20 |
| Ray-grid coverage | 48 204 rays, 0 misses |
| Perf 1080p | 274.6 fps, GPU 2.88 ms average |
| Perf 4K | 105.5 fps, GPU 9.0 ms average |
| VRAM | 663 MiB (1080p), 1 622 MiB (4K) |

The critique is in `QA/critique-lookv2-01.md`: S1 1, S2 15, S3 26. Captures are in `QA/captures/lookv2_01*`.

## Known issues (current)
- The abyss of beats 3-4 lacks content (S1, INT-01).
- The wall of signs is flat panels.
- Street level, the tram, the trains, the garden lanterns and fireflies are missing.
- The koi reads as a crescent from below.
- The goal window has no frame.
- The score is a skeleton.
- The "no frame above 33 ms" gate is unmeasured on a quiet machine.
- The 4K encode test is not yet run.

All of these are scheduled in Phase 4 waves A and B and in Phase 6 (`STATE.md`).

## How to record the video
Run `Builds/Nocturne/Nocturne.exe -cinematic -res 3840x2160` in full screen and record at 60 fps with a high bitrate.
Upload at 1440p or 2160p. Details are in `README.md`.

## Project metrics
Measured by `python -I Tools/metrics.py` over every transcript of this run from the anchor `Run started at
2026-11-13T19:22:09.6952535+01:00`. The prices are carried over and will be re-fetched at the final step (Part 15.3).

| Metric | Value | How obtained |
|---|---|---|
| Wall-clock time | 60:48:27 | measured: anchor 2026-11-13T19:22:09.695253+01:00 to the last included record 2026-11-16T07:10:36.887000+00:00 |
| Active time | 26:20:32 | measured: wall clock minus 6 gap(s) longer than 10 min without any transcript activity |
| Uncached input tokens | 15,248 | measured: sum of usage.input_tokens, deduplicated by message id |
| Cache read tokens | 2,877,194,388 | measured: usage.cache_read_input_tokens |
| Cache write tokens (5 min) | 126,806,622 | measured: usage.cache_creation.ephemeral_5m_input_tokens |
| Cache write tokens (1 h) | 2,252,749 | measured: usage.cache_creation.ephemeral_1h_input_tokens |
| Output tokens (incl. thinking) | 10,451,066 | measured: usage.output_tokens |
| Total tokens | 3,016,720,073 | measured: 7,562 assistant messages in 2 session file(s) and 121 subagent/workflow file(s) |
| API-equivalent cost (USD) | $1,436.58 | estimated: input 15,248 x $4.0/MTok + cache_read 2,877,194,388 x $0.2/MTok + cache_write_5m 126,806,622 x $5.0/MTok + cache_write_1h 2,252,749 x $8.0/MTok + output 10,451,066 x $20.0/MTok; prices for claude-opus-5-5 from https://platform.claude.com/docs/en/about-claude/pricing (accessed 2026-10-30 (carried over from the Thread run; re-fetched at the final step of Part 15.3)) |

Not counted: the tokens of the final measurement turn after this script ran and of the final chat message.

Gaps longer than 10 minutes: 2026-11-14 09:11 to 11:19 (2:07:27); 2026-11-14 12:31 to 2026-11-15 20:11 (31:40:25); 2026-11-16 03:48 to 03:58 (0:10:01); 2026-11-16 04:24 to 04:34 (0:10:01); 2026-11-16 04:41 to 04:51 (0:10:01); 2026-11-16 04:51 to 05:01 (0:10:00)
