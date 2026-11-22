# Encode test: AT_e02_enc_beat01_slit

ffmpeg: `C:\Users\<user>\AppData\Local\Microsoft\WinGet\Links\ffmpeg.EXE`, encoders: libvpx-vp9, libx264

Sequences: 1. Checks: 0 PASS, 1 FAIL, 0 WARN, 0 SKIP.

**Result: FAIL**

## enc_beat01_slit (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\abyss\QA\captures\AT_e02\enc_beat01_slit` 1920x1080, 60 fps, 600 frames encoded

| Stream | Region | Score | Steps/1000 | Run px | Resid RMS | Resid p-p | kbit/s | MAE codes | Result |
|---|---|---|---|---|---|---|---|---|---|
| source | cli0 | 0.519 | 0.00 | 7 | - | - | - | - | PASS |
| x264 crf 23 | cli0 | 2.107 | 0.00 | 95 | - | - | 4218 | 1.732 | FAIL |

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| x264_crf23[cli0] | FAIL | 2.1070 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 3.16 (run 95 px) (source score 0.519), frame 599 |

Review crops (source | decoded; _100, _200 nearest, _x8 local contrast):

- `QA/encode/AT_e02_enc_beat01_slit\enc_beat01_slit\crops\x264_crf23_cli0_f00342_100.png`
- `QA/encode/AT_e02_enc_beat01_slit\enc_beat01_slit\crops\x264_crf23_cli0_f00342_200.png`
- `QA/encode/AT_e02_enc_beat01_slit\enc_beat01_slit\crops\x264_crf23_cli0_f00342_x8.png`
