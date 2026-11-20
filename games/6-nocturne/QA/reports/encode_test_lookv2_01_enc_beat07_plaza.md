# Encode test: lookv2_01_enc_beat07_plaza

ffmpeg: `C:\Users\Cryptopiy\AppData\Local\Microsoft\WinGet\Links\ffmpeg.EXE`, encoders: libvpx-vp9, libx264

Sequences: 1. Checks: 0 PASS, 1 FAIL, 0 WARN, 0 SKIP.

**Result: FAIL**

## enc_beat07_plaza (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\lookv2_01enc\enc_beat07_plaza` 1920x1080, 60 fps, 600 frames encoded

| Stream | Region | Score | Steps/1000 | Run px | Resid RMS | Resid p-p | kbit/s | MAE codes | Result |
|---|---|---|---|---|---|---|---|---|---|
| source | cli0 | 2.000 | 8.40 | 9 | - | - | - | - | FAIL |
| x264 crf 23 | cli0 | 4.202 | 8.40 | 72 | - | - | 1570 | 1.508 | FAIL |

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| x264_crf23[cli0] | FAIL | 4.2020 | score <= 1 | visible steps/1000 8.40 (max 1.26 code), BandScore 3.32 (run 72 px) (source score 2.000), frame 0 |

Review crops (source | decoded; _100, _200 nearest, _x8 local contrast):

- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\encode\lookv2_01_enc_beat07_plaza\enc_beat07_plaza\crops\x264_crf23_cli0_f00342_100.png`
- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\encode\lookv2_01_enc_beat07_plaza\enc_beat07_plaza\crops\x264_crf23_cli0_f00342_200.png`
- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\encode\lookv2_01_enc_beat07_plaza\enc_beat07_plaza\crops\x264_crf23_cli0_f00342_x8.png`
