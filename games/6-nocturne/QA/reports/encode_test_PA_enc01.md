# Encode test: PA_enc01

ffmpeg: `C:\Users\<user>\AppData\Local\Microsoft\WinGet\Links\ffmpeg.EXE`, encoders: libvpx-vp9, libx264

Sequences: 1. Checks: 0 PASS, 2 FAIL, 0 WARN, 0 SKIP.

**Result: FAIL**

## enc_beat01_slit (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\PA_enc01\enc_beat01_slit` 1920x1080, 60 fps, 600 frames encoded

| Stream | Region | Score | Steps/1000 | Run px | Resid RMS | Resid p-p | kbit/s | MAE codes | Result |
|---|---|---|---|---|---|---|---|---|---|
| source | cli0 | 0.233 | 0.00 | 7 | - | - | - | - | PASS |
| source | cli1 | 24.875 | 0.00 | 14 | - | - | - | - | FAIL |
| x264 crf 23 | cli0 | 1.216 | 0.00 | 48 | - | - | 2480 | 1.420 | FAIL |
| x264 crf 23 | cli1 | 112.242 | 0.00 | 74 | - | - | 2480 | 1.420 | FAIL |

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| x264_crf23[cli0] | FAIL | 1.2160 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 1.82 (run 48 px) (source score 0.233), frame 513 |
| x264_crf23[cli1] | FAIL | 112.2420 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 168.36 (run 74 px) (source score 24.875), frame 171 |

Review crops (source | decoded; _100, _200 nearest, _x8 local contrast):

- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\encode\PA_enc01\enc_beat01_slit\crops\x264_crf23_cli0_f00342_100.png`
- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\encode\PA_enc01\enc_beat01_slit\crops\x264_crf23_cli0_f00342_200.png`
- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\encode\PA_enc01\enc_beat01_slit\crops\x264_crf23_cli0_f00342_x8.png`
- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\encode\PA_enc01\enc_beat01_slit\crops\x264_crf23_cli1_f00342_100.png`
- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\encode\PA_enc01\enc_beat01_slit\crops\x264_crf23_cli1_f00342_200.png`
- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\encode\PA_enc01\enc_beat01_slit\crops\x264_crf23_cli1_f00342_x8.png`
