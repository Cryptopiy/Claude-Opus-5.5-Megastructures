# Encode test: PA_enc03

ffmpeg: `C:\Users\<user>\AppData\Local\Microsoft\WinGet\Links\ffmpeg.EXE`, encoders: libvpx-vp9, libx264

Sequences: 1. Checks: 0 PASS, 1 FAIL, 0 WARN, 0 SKIP.

**Result: FAIL**

## enc_beat01_slit (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\PA_enc03\enc_beat01_slit` 1920x1080, 60 fps, 600 frames encoded

| Stream | Region | Score | Steps/1000 | Run px | Resid RMS | Resid p-p | kbit/s | MAE codes | Result |
|---|---|---|---|---|---|---|---|---|---|
| source | cli0 | 0.237 | 0.00 | 9 | - | - | - | - | PASS |
| x264 crf 23 | cli0 | 2.203 | 4.41 | 91 | - | - | 2473 | 1.427 | FAIL |

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| x264_crf23[cli0] | FAIL | 2.2030 | score <= 1 | visible steps/1000 4.41 (max 1.51 code), BandScore 1.83 (run 91 px) (source score 0.237), frame 599 |

Review crops (source | decoded; _100, _200 nearest, _x8 local contrast):

- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\encode\PA_enc03\enc_beat01_slit\crops\x264_crf23_cli0_f00342_100.png`
- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\encode\PA_enc03\enc_beat01_slit\crops\x264_crf23_cli0_f00342_200.png`
- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\encode\PA_enc03\enc_beat01_slit\crops\x264_crf23_cli0_f00342_x8.png`
