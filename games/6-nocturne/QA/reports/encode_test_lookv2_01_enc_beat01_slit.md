# Encode test: lookv2_01_enc_beat01_slit

ffmpeg: `C:\Users\Cryptopiy\AppData\Local\Microsoft\WinGet\Links\ffmpeg.EXE`, encoders: libvpx-vp9, libx264

Sequences: 1. Checks: 0 PASS, 1 FAIL, 0 WARN, 0 SKIP.

**Result: FAIL**

## enc_beat01_slit (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\lookv2_01enc\enc_beat01_slit` 1920x1080, 60 fps, 600 frames encoded

| Stream | Region | Score | Steps/1000 | Run px | Resid RMS | Resid p-p | kbit/s | MAE codes | Result |
|---|---|---|---|---|---|---|---|---|---|
| source | cli0 | 0.233 | 0.00 | 7 | - | - | - | - | PASS |
| x264 crf 23 | cli0 | 1.449 | 0.00 | 52 | - | - | 4236 | 1.741 | FAIL |

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| x264_crf23[cli0] | FAIL | 1.4490 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 2.17 (run 52 px) (source score 0.233), frame 599 |

Review crops (source | decoded; _100, _200 nearest, _x8 local contrast):

- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\encode\lookv2_01_enc_beat01_slit\enc_beat01_slit\crops\x264_crf23_cli0_f00342_100.png`
- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\encode\lookv2_01_enc_beat01_slit\enc_beat01_slit\crops\x264_crf23_cli0_f00342_200.png`
- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\encode\lookv2_01_enc_beat01_slit\enc_beat01_slit\crops\x264_crf23_cli0_f00342_x8.png`
