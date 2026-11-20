# Frame sequences: a1_boundaries

Sequences: 9. Checks: 27 PASS, 0 FAIL, 0 WARN, 0 SKIP.

**Result: PASS**

## Summary

| Sequence | Kind | Frames | Max luma step | Mean diff | Max diff | Max px diff | Result | Failed checks |
|---|---|---|---|---|---|---|---|---|
| b12 | boundary | 601 | 0.0026 | 0.284 | 1.119 | 177.7 | PASS |  |
| b23 | boundary | 601 | 0.0007 | 0.239 | 0.412 | 138.4 | PASS |  |
| b34 | boundary | 601 | 0.0029 | 0.211 | 1.099 | 142.1 | PASS |  |
| b45 | boundary | 601 | 0.0020 | 0.254 | 0.807 | 155.9 | PASS |  |
| b56 | boundary | 601 | 0.0006 | 0.233 | 0.499 | 188.4 | PASS |  |
| b67 | boundary | 601 | 0.0005 | 0.279 | 0.422 | 185.4 | PASS |  |
| s1240 | boundary | 1201 | 0.0005 | 0.245 | 0.422 | 187.4 | PASS |  |
| s380 | boundary | 801 | 0.0007 | 0.241 | 0.412 | 138.5 | PASS |  |
| s96 | boundary | 681 | 0.0026 | 0.258 | 1.119 | 177.7 | PASS |  |

## b12 (PASS)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\a1_boundaries\seq_b12`, 601 frames at 30 fps, kind boundary

- luma      `▆▆▆▆▆▆▆▆▆▆▆▆▆▆▆▇▇▇██▂▁▁▁▁▁▂▂▂▃▃▃▃▃▃▃▃▃▃▄▄▄▄▄▅▅▅▆▆▆▆▇▇▇▇▇▇▇▇▇`
- mean diff `▁▁▁▁▁▂▂▂▂▂▂▃▃▄▄▅▆▇██▅▂▂▂▁▁▁▁▁▁▁▁▁▁▁▁▂▂▂▂▂▂▂▂▂▂▂▂▂▂▂▂▂▂▂▂▂▂▂▂`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.6660 | <= 1.2 levels | at frame 200 (14.10) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| activation-frame diff | PASS | 0.0000 | <= 0.5 level | boundaries [120.0], worst at frame None |

## b23 (PASS)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\a1_boundaries\seq_b23`, 601 frames at 30 fps, kind boundary

- luma      `████▇▆▆▆▅▅▅▅▅▅▅▅▅▄▄▄▄▄▃▃▃▃▃▃▃▃▃▃▃▃▃▂▂▂▂▂▃▃▃▃▃▂▂▂▂▂▂▂▁▁▁▁▁▁▂▂`
- mean diff `▇▇███▂▂▂▂▂▂▂▂▂▂▂▂▂▂▂▂▂▂▂▂▂▂▂▂▂▂▂▂▂▂▂▂▂▂▂▁▁▁▁▁▁▁▁▁▁▁▁▂▂▂▁▁▁▁▁`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.1670 | <= 1.2 levels | at frame 44 (14.10) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| activation-frame diff | PASS | 0.0071 | <= 0.5 level | boundaries [400.0], worst at frame 300 |

## b34 (PASS)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\a1_boundaries\seq_b34`, 601 frames at 30 fps, kind boundary

- luma      `▃▃▃▃▃▃▃▃▃▄▄▄▅▅▅▆▆▇▇██▃▂▂▂▂▂▂▂▂▂▂▂▂▂▂▂▃▃▃▃▃▃▃▂▂▂▂▂▂▂▁▁▁▁▁▁▁▁▁`
- mean diff `▂▂▂▂▂▂▂▂▂▂▃▃▃▃▃▄▄▅▆██▆▂▂▂▂▂▂▂▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▂▂▂▂▂▂▂▂▂▂▂▂`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.7460 | <= 1.2 levels | at frame 211 (14.10) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| activation-frame diff | PASS | 0.0000 | <= 0.5 level | boundaries [560.0], worst at frame None |

## b45 (PASS)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\a1_boundaries\seq_b45`, 601 frames at 30 fps, kind boundary

- luma      `█████▇▇▇▇▇▇▇▆▇▇▇▇▇▆▄▄▄▄▄▃▃▃▃▃▃▂▂▂▂▂▂▂▂▂▂▂▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁`
- mean diff `▂▂▂▂▃▃▃▃▄▄▄▅▅▅▆▇██▆▁▁▁▁▁▁▁▁▁▁▁▁▁▁▁▂▂▂▂▂▂▂▂▂▂▂▂▂▂▂▁▁▁▁▁▁▁▁▁▁▁`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.5200 | <= 1.2 levels | at frame 183 (14.10) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| activation-frame diff | PASS | 0.0000 | <= 0.5 level | boundaries [880.0], worst at frame None |

## b56 (PASS)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\a1_boundaries\seq_b56`, 601 frames at 30 fps, kind boundary

- luma      `▂▂▂▂▁▁▁▁▁▁▁▂▂▂▂▂▂▁▁▁▁▁▁▂▂▂▂▂▂▃▃▃▃▃▃▃▃▃▃▄▄▄▄▄▅▅▆▆▇▇▇█████████`
- mean diff `▃▄▄▄▅▅▅▆▆▆▆▆▇▇▇█▇▄▄▃▃▃▃▃▃▃▃▃▃▃▃▃▃▃▄▄▄▄▄▄▄▄▄▄▄▃▃▃▂▂▂▁▁▁▁▁▁▁▁▂`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.1550 | <= 1.2 levels | at frame 165 (14.10) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| activation-frame diff | PASS | 0.0008 | <= 0.5 level | boundaries [1080.0], worst at frame 300 |

## b67 (PASS)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\a1_boundaries\seq_b67`, 601 frames at 30 fps, kind boundary

- luma      `▃▃▂▂▂▂▂▂▂▂▃▃▃▃▄▃▂▂▂▂▁▁▁▁▁▁▁▁▂▂▂▂▂▂▂▂▂▂▂▁▂▂▂▂▃▃▃▃▄▄▄▄▅▅▅▅▆▆▇█`
- mean diff `▅▅▆▆▆▇▇▇▇▇▇▇▇██▆▃▃▃▄▄▄▄▄▄▄▄▄▃▃▃▃▃▃▃▃▄▄▄▄▄▅▄▄▄▄▄▃▃▃▃▃▃▃▃▃▃▂▂▁`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.1360 | <= 1.2 levels | at frame 154 (14.10) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| activation-frame diff | PASS | 0.0000 | <= 0.5 level | boundaries [1260.0], worst at frame None |

## s1240 (PASS)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\a1_boundaries\seq_s1240`, 1201 frames at 30 fps, kind boundary

- luma      `▄▄▄▃▃▃▂▂▂▂▃▃▃▂▂▁▁▁▁▂▂▂▂▂▁▂▂▃▃▄▄▄▅▆▇██▇▇▇▇▆▆▆▆▆▆▆▆▆▆▆▆▆▇▇▇▇██`
- mean diff `▄▄▅▅▅▆▇▇▇▇▇██▄▄▅▄▄▄▄▄▄▄▅▅▅▅▅▄▄▄▄▄▄▃▁▁▂▂▂▃▄▄▄▃▃▃▃▃▃▃▄▄▄▃▃▃▃▃▂`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.1360 | <= 1.2 levels | at frame 254 (14.10) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| activation-frame diff | PASS | 0.0000 | <= 0.5 level | boundaries [1260.0], worst at frame None |

## s380 (PASS)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\a1_boundaries\seq_s380`, 801 frames at 30 fps, kind boundary

- luma      `█████▇▇▇▇▇█▅▅▅▅▅▅▅▅▅▄▄▄▃▃▃▃▃▃▃▃▃▃▃▂▂▂▂▂▂▂▂▂▂▂▂▁▁▁▁▁▂▂▂▃▃▂▂▂▂`
- mean diff `▃▃▄▅▆▆▆▇▇██▃▃▃▃▃▃▃▃▃▃▃▃▃▃▃▃▃▃▃▃▃▃▃▃▃▃▃▂▂▂▂▂▂▂▂▃▃▃▃▂▂▂▂▁▁▁▂▂▃`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.1670 | <= 1.2 levels | at frame 144 (14.10) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| activation-frame diff | PASS | 0.0071 | <= 0.5 level | boundaries [400.0], worst at frame 400 |

## s96 (PASS)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\a1_boundaries\seq_s96`, 681 frames at 30 fps, kind boundary

- luma      `▅▅▅▅▅▅▅▅▅▅▅▅▅▆▆▆▆▆▆▆▆▆▆▆▆▆▆▆▆▇▇▇█▇▂▁▁▁▁▂▂▂▃▃▃▃▃▃▃▃▄▄▄▄▅▅▅▆▆▆`
- mean diff `▁▁▁▁▁▁▁▁▁▂▂▁▁▁▁▁▁▁▁▁▂▂▂▂▂▃▃▄▄▅▆▇█▆▂▂▂▁▁▁▁▁▁▁▁▁▁▁▂▂▂▂▂▂▂▂▂▂▂▂`

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| luma step/frame | PASS | 0.6660 | <= 1.2 levels | at frame 380 (14.10) |
| diff spikes | PASS | 0 | 0 above 3x running median | floor 0.25 grey levels |
| activation-frame diff | PASS | 0.0000 | <= 0.5 level | boundaries [120.0], worst at frame None |
