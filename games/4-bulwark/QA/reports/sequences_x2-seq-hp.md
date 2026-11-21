# Frame sequences: x2-seq-hp

Sequences: 4. Checks: 16 PASS, 0 FAIL, 0 WARN, 0 SKIP.

**Result: PASS**

## Summary

| Sequence | Kind | Frames | Max luma step (levels) | Mean diff | Max diff | Max px diff | Result | Failed checks |
|---|---|---|---|---|---|---|---|---|
| static_KS04 | static | 120 | 0.042 | 0.742 | 0.919 | 78.1 | PASS |  |
| plumes_KS04 | static | 241 | 0.410 | 3.300 | 3.742 | 111.7 | PASS |  |
| static_b02_Crest | static | 120 | 0.005 | 0.014 | 0.020 | 35.1 | PASS |  |
| static_b04_ArmourSlope | static | 120 | 0.011 | 0.085 | 0.106 | 120.0 | PASS |  |

## static_KS04 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-seq\seq_static_KS04`, 120 frames at 30 fps, kind static

- luma      `▂▁▁▁▁▁▁▁▁▁▂▂▂▂▃▃▃▃▄▄▄▅▅▅▅▆▆▆▆▆▆▆▆▆▆▆▆▆▆▆▇▇▇▇▇▇▇▇▇▇▇█████████`
- mean diff `▂▂▁▁▁▁▁▁▁▁▂▂▂▃▃▄▄▅▅▆▆▆▇▇▇██████████▇▇▇▇▇▇▇▇▇▇▆▆▆▆▆▅▅▅▅▄▄▄▄▄▄`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.0422 | <= 1.2 levels | at frame 35 |
| EV slope | INFO | 0.0362 |  | no per-frame 'ev' in sequence.json: log2 linear-luma slope (content changes included) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| flicker block std | PASS | 1.9653 | <= 3 levels | 120 frames, 64 px blocks, high-passed above 1 Hz (raw 14.84) |
| fireflies | PASS | 0 | 0 px | frames with any: 0 |

## plumes_KS04 (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-seq\seq_plumes_KS04`, 241 frames at 4 fps, kind static

- luma      `▁▆▇██▇▄▃▃▂▂▃▄▇▇█▅▂▁▄▅▆▇▇▆▄▃▆▆▃▄▄▅▇▆▄▃▁▂▆▇▇▅▅▅▄▃▂▁▂▅▆▅▃▅▅▇▅▂▁`
- mean diff `▁██▆▆▇▅▃▅▄▆▆▄▆▄▅▅▃▂▆▇▄▆█▄▆▅▄▃▅▆▄▆▆▅▆▅▂▃▆▆▄▄▆▇▄▅▅▄▄▇▇▄▃▃▅▇▆▂▃`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.4103 | <= 1.2 levels | at frame 110 |
| EV slope | INFO | 0.0458 |  | no per-frame 'ev' in sequence.json: log2 linear-luma slope (content changes included) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| flicker block std | PASS | 2.4743 | <= 3 levels | 241 frames, 64 px blocks, high-passed above 1 Hz (raw 11.49) |
| fireflies | PASS | 0 | 0 px | frames with any: 0 |

## static_b02_Crest (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-seq\seq_static_b02_Crest`, 120 frames at 30 fps, kind static

- luma      `▅▅▆▇███▇▆▅▄▃▂▂▁▁▂▃▄▅▅▅▅▅▅▅▅▅▅▅▅▆▆▇▇█████████▇▇▇▆▆▆▆▆▆▆▆▆▅▅▅▅`
- mean diff `▅▅▆▆▆▆▅▇▇▆▆▅▄▃▂▂▅█▅▄▄▃▅▅▅▄▄▄▄▃▃▃▃▃▃▃▃▃▃▃▃▃▃▂▂▂▂▂▂▂▂▂▂▂▁▁▁▁▁▁`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.0050 | <= 1.2 levels | at frame 34 |
| EV slope | INFO | 0.0044 |  | no per-frame 'ev' in sequence.json: log2 linear-luma slope (content changes included) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| flicker block std | PASS | 0.5991 | <= 3 levels | 120 frames, 64 px blocks, high-passed above 1 Hz (raw 3.03) |
| fireflies | PASS | 0 | 0 px | frames with any: 0 |

## static_b04_ArmourSlope (PASS)

`<WORKSPACE>\4 Megastructures\4 Megastructures\QA\captures\x2-seq\seq_static_b04_ArmourSlope`, 120 frames at 30 fps, kind static

- luma      `▇███████▇▇▇▇▇▇▇▇▇▇▆▆▆▅▅▄▄▃▃▃▃▂▂▂▂▂▂▂▁▁▁▁▁▂▂▂▂▂▃▃▃▃▄▄▄▅▅▆▆▆▆▆`
- mean diff `▇▅██▇▇█▆▇▆▆▇▆▆▆▇▆▇▇▆▇▆▅▇▆▆▇▆▆▆▅▅▅▄▄▄▄▄▁▃▄▃▃▃▃▄▄▃▄▅▆▆▆▆▇▇▇███`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.0106 | <= 1.2 levels | at frame 105 |
| EV slope | INFO | 0.0084 |  | no per-frame 'ev' in sequence.json: log2 linear-luma slope (content changes included) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| flicker block std | PASS | 1.2774 | <= 3 levels | 120 frames, 64 px blocks, high-passed above 1 Hz (raw 19.66) |
| fireflies | PASS | 0 | 0 px | frames with any: 0 |
