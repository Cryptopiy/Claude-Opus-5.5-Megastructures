# Encode test: AT_e13_enc_beat03_abyss

ffmpeg: `C:\Users\<user>\AppData\Local\Microsoft\WinGet\Links\ffmpeg.EXE`, encoders: libvpx-vp9, libx264

Sequences: 1. Checks: 0 PASS, 1 FAIL, 0 WARN, 0 SKIP.

**Result: FAIL**

## enc_beat03_abyss (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\abyss\QA\captures\AT_e13\enc_beat03_abyss` 1920x1080, 60 fps, 600 frames encoded

| Stream | Region | Score | Steps/1000 | Run px | Resid RMS | Resid p-p | kbit/s | MAE codes | Result |
|---|---|---|---|---|---|---|---|---|---|
| source | cli0 | 5.198 | 0.00 | 6 | - | - | - | - | FAIL |
| x264 crf 23 | cli0 | 8.571 | 0.00 | 23 | - | - | 2975 | 1.676 | FAIL |

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| x264_crf23[cli0] | FAIL | 8.5710 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 12.86 (run 23 px) (source score 5.198), frame 171 |

Review crops (source | decoded; _100, _200 nearest, _x8 local contrast):

- `QA/encode/AT_e13_enc_beat03_abyss\enc_beat03_abyss\crops\x264_crf23_cli0_f00342_100.png`
- `QA/encode/AT_e13_enc_beat03_abyss\enc_beat03_abyss\crops\x264_crf23_cli0_f00342_200.png`
- `QA/encode/AT_e13_enc_beat03_abyss\enc_beat03_abyss\crops\x264_crf23_cli0_f00342_x8.png`
