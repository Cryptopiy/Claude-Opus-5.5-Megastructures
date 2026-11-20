# Encode test: lookv2_01_enc_beat07_goal

ffmpeg: `C:\Users\Cryptopiy\AppData\Local\Microsoft\WinGet\Links\ffmpeg.EXE`, encoders: libvpx-vp9, libx264

Sequences: 1. Checks: 1 PASS, 0 FAIL, 0 WARN, 0 SKIP.

**Result: PASS**

## enc_beat07_goal (PASS)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\lookv2_01enc\enc_beat07_goal` 1920x1080, 60 fps, 600 frames encoded

| Stream | Region | Score | Steps/1000 | Run px | Resid RMS | Resid p-p | kbit/s | MAE codes | Result |
|---|---|---|---|---|---|---|---|---|---|
| source | tenement_wall | 0.054 | 0.00 | 4 | - | - | - | - | PASS |
| x264 crf 23 | tenement_wall | 0.081 | 0.00 | 6 | - | - | 742 | 1.568 | PASS |

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| x264_crf23[tenement_wall] | PASS | 0.0810 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.12 (run 6 px) (source score 0.054), frame 86 |

Review crops (source | decoded; _100, _200 nearest, _x8 local contrast):

- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\encode\lookv2_01_enc_beat07_goal\enc_beat07_goal\crops\x264_crf23_tenement_wall_f00342_100.png`
- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\encode\lookv2_01_enc_beat07_goal\enc_beat07_goal\crops\x264_crf23_tenement_wall_f00342_200.png`
- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\encode\lookv2_01_enc_beat07_goal\enc_beat07_goal\crops\x264_crf23_tenement_wall_f00342_x8.png`
