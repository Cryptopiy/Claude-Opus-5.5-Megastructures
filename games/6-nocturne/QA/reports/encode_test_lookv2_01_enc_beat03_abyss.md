# Encode test: lookv2_01_enc_beat03_abyss

ffmpeg: `C:\Users\<user>\AppData\Local\Microsoft\WinGet\Links\ffmpeg.EXE`, encoders: libvpx-vp9, libx264

Sequences: 1. Checks: 0 PASS, 1 FAIL, 0 WARN, 0 SKIP.

**Result: FAIL**

## enc_beat03_abyss (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\lookv2_01enc\enc_beat03_abyss` 1920x1080, 60 fps, 600 frames encoded

| Stream | Region | Score | Steps/1000 | Run px | Resid RMS | Resid p-p | kbit/s | MAE codes | Result |
|---|---|---|---|---|---|---|---|---|---|
| source | cli0 | 1.311 | 0.00 | 6 | - | - | - | - | FAIL |
| x264 crf 23 | cli0 | 6.038 | 5.13 | 50 | - | - | 2473 | 1.515 | FAIL |

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| x264_crf23[cli0] | FAIL | 6.0380 | score <= 1 | visible steps/1000 5.13 (max 1.04 code), BandScore 9.06 (run 50 px) (source score 1.311), frame 0 |

Review crops (source | decoded; _100, _200 nearest, _x8 local contrast):

- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\encode\lookv2_01_enc_beat03_abyss\enc_beat03_abyss\crops\x264_crf23_cli0_f00342_100.png`
- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\encode\lookv2_01_enc_beat03_abyss\enc_beat03_abyss\crops\x264_crf23_cli0_f00342_200.png`
- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\encode\lookv2_01_enc_beat03_abyss\enc_beat03_abyss\crops\x264_crf23_cli0_f00342_x8.png`
