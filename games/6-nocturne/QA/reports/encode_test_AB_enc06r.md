# Encode test: AB_enc06r

ffmpeg: `C:\Users\<user>\AppData\Local\Microsoft\WinGet\Links\ffmpeg.EXE`, encoders: libvpx-vp9, libx264

Sequences: 1. Checks: 0 PASS, 1 FAIL, 0 WARN, 0 SKIP.

**Result: FAIL**

## enc_beat03_abyss (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\abyss\QA\captures\AB_enc06\enc_beat03_abyss` 1920x1080, 60 fps, 600 frames encoded

| Stream | Region | Score | Steps/1000 | Run px | Resid RMS | Resid p-p | kbit/s | MAE codes | Result |
|---|---|---|---|---|---|---|---|---|---|
| source | cli0 | 0.503 | 0.00 | 9 | - | - | - | - | PASS |
| x264 crf 23 | cli0 | 2.632 | 5.26 | 29 | - | - | 3286 | 1.722 | FAIL |

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| x264_crf23[cli0] | FAIL | 2.6320 | score <= 1 | visible steps/1000 5.26 (max 1.41 code), BandScore 2.39 (run 29 px) (source score 0.503), frame 0 |

Review crops (source | decoded; _100, _200 nearest, _x8 local contrast):

- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\abyss\QA\encode\AB_enc06r\enc_beat03_abyss\crops\x264_crf23_cli0_f00342_100.png`
- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\abyss\QA\encode\AB_enc06r\enc_beat03_abyss\crops\x264_crf23_cli0_f00342_200.png`
- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\abyss\QA\encode\AB_enc06r\enc_beat03_abyss\crops\x264_crf23_cli0_f00342_x8.png`
