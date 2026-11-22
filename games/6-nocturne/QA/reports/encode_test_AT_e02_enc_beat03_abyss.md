# Encode test: AT_e02_enc_beat03_abyss

ffmpeg: `C:\Users\<user>\AppData\Local\Microsoft\WinGet\Links\ffmpeg.EXE`, encoders: libvpx-vp9, libx264

Sequences: 1. Checks: 0 PASS, 1 FAIL, 0 WARN, 0 SKIP.

**Result: FAIL**

## enc_beat03_abyss (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\abyss\QA\captures\AT_e02\enc_beat03_abyss` 1920x1080, 60 fps, 600 frames encoded

| Stream | Region | Score | Steps/1000 | Run px | Resid RMS | Resid p-p | kbit/s | MAE codes | Result |
|---|---|---|---|---|---|---|---|---|---|
| source | cli0 | 5.109 | 0.00 | 6 | - | - | - | - | FAIL |
| x264 crf 23 | cli0 | 18.138 | 0.00 | 23 | - | - | 3016 | 1.677 | FAIL |

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| x264_crf23[cli0] | FAIL | 18.1380 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 27.21 (run 23 px) (source score 5.109), frame 0 |

Review crops (source | decoded; _100, _200 nearest, _x8 local contrast):

- `QA/encode/AT_e02_abyss\enc_beat03_abyss\crops\x264_crf23_cli0_f00342_100.png`
- `QA/encode/AT_e02_abyss\enc_beat03_abyss\crops\x264_crf23_cli0_f00342_200.png`
- `QA/encode/AT_e02_abyss\enc_beat03_abyss\crops\x264_crf23_cli0_f00342_x8.png`
