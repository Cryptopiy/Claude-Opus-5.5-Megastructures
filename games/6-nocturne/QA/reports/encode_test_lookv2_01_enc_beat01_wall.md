# Encode test: lookv2_01_enc_beat01_wall

ffmpeg: `C:\Users\Cryptopiy\AppData\Local\Microsoft\WinGet\Links\ffmpeg.EXE`, encoders: libvpx-vp9, libx264

Sequences: 1. Checks: 0 PASS, 1 FAIL, 0 WARN, 0 SKIP.

**Result: FAIL**

## enc_beat01_wall (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\lookv2_01enc\enc_beat01_wall` 1920x1080, 60 fps, 600 frames encoded

| Stream | Region | Score | Steps/1000 | Run px | Resid RMS | Resid p-p | kbit/s | MAE codes | Result |
|---|---|---|---|---|---|---|---|---|---|
| source | alley_wall | 14.364 | 0.00 | 9 | - | - | - | - | FAIL |
| x264 crf 23 | alley_wall | 37.775 | 0.00 | 36 | - | - | 2930 | 1.771 | FAIL |

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| x264_crf23[alley_wall] | FAIL | 37.7750 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 56.66 (run 36 px) (source score 14.364), frame 599 |

Review crops (source | decoded; _100, _200 nearest, _x8 local contrast):

- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\encode\lookv2_01_enc_beat01_wall\enc_beat01_wall\crops\x264_crf23_alley_wall_f00342_100.png`
- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\encode\lookv2_01_enc_beat01_wall\enc_beat01_wall\crops\x264_crf23_alley_wall_f00342_200.png`
- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\encode\lookv2_01_enc_beat01_wall\enc_beat01_wall\crops\x264_crf23_alley_wall_f00342_x8.png`
