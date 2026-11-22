# Encode test: p4_01_enc_beat01_wall

ffmpeg: `C:\Users\<user>\AppData\Local\Microsoft\WinGet\Links\ffmpeg.EXE`, encoders: libvpx-vp9, libx264

Sequences: 1. Checks: 0 PASS, 1 FAIL, 0 WARN, 0 SKIP.

**Result: FAIL**

## enc_beat01_wall (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01_enc\enc_beat01_wall` 1920x1080, 60 fps, 600 frames encoded

| Stream | Region | Score | Steps/1000 | Run px | Resid RMS | Resid p-p | kbit/s | MAE codes | Result |
|---|---|---|---|---|---|---|---|---|---|
| source | alley_wall | 6.323 | 1.85 | 7 | - | - | - | - | FAIL |
| x264 crf 23 | alley_wall | 19.804 | 0.00 | 29 | - | - | 3228 | 1.844 | FAIL |

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| x264_crf23[alley_wall] | FAIL | 19.8040 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 29.71 (run 29 px) (source score 6.323), frame 599 |

Review crops (source | decoded; _100, _200 nearest, _x8 local contrast):

- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\encode\p4_01_enc_beat01_wall\enc_beat01_wall\crops\x264_crf23_alley_wall_f00342_100.png`
- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\encode\p4_01_enc_beat01_wall\enc_beat01_wall\crops\x264_crf23_alley_wall_f00342_200.png`
- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\encode\p4_01_enc_beat01_wall\enc_beat01_wall\crops\x264_crf23_alley_wall_f00342_x8.png`
