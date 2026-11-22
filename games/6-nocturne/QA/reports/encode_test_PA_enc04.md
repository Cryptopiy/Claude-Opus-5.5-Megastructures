# Encode test: PA_enc04

ffmpeg: `C:\Users\<user>\AppData\Local\Microsoft\WinGet\Links\ffmpeg.EXE`, encoders: libvpx-vp9, libx264

Sequences: 1. Checks: 0 PASS, 1 FAIL, 0 WARN, 0 SKIP.

**Result: FAIL**

## enc_beat07_goal (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\PA_enc04\enc_beat07_goal` 1920x1080, 60 fps, 600 frames encoded

| Stream | Region | Score | Steps/1000 | Run px | Resid RMS | Resid p-p | kbit/s | MAE codes | Result |
|---|---|---|---|---|---|---|---|---|---|
| source | cli0 | 2.641 | 0.00 | 6 | - | - | - | - | FAIL |
| x264 crf 23 | cli0 | 16.697 | 0.00 | 46 | - | - | 2840 | 1.677 | FAIL |

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| x264_crf23[cli0] | FAIL | 16.6970 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 25.05 (run 46 px) (source score 2.641), frame 428 |

Review crops (source | decoded; _100, _200 nearest, _x8 local contrast):

- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\encode\PA_enc04\enc_beat07_goal\crops\x264_crf23_cli0_f00342_100.png`
- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\encode\PA_enc04\enc_beat07_goal\crops\x264_crf23_cli0_f00342_200.png`
- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\encode\PA_enc04\enc_beat07_goal\crops\x264_crf23_cli0_f00342_x8.png`
