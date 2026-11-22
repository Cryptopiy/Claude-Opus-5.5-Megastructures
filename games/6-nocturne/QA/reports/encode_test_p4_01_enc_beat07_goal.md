# Encode test: p4_01_enc_beat07_goal

ffmpeg: `C:\Users\<user>\AppData\Local\Microsoft\WinGet\Links\ffmpeg.EXE`, encoders: libvpx-vp9, libx264

Sequences: 1. Checks: 1 PASS, 0 FAIL, 0 WARN, 0 SKIP.

**Result: PASS**

## enc_beat07_goal (PASS)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01_enc\enc_beat07_goal` 1920x1080, 60 fps, 600 frames encoded

| Stream | Region | Score | Steps/1000 | Run px | Resid RMS | Resid p-p | kbit/s | MAE codes | Result |
|---|---|---|---|---|---|---|---|---|---|
| source | tenement_wall | 0.073 | 0.00 | 5 | - | - | - | - | PASS |
| x264 crf 23 | tenement_wall | 0.115 | 0.00 | 8 | - | - | 791 | 1.636 | PASS |

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| x264_crf23[tenement_wall] | PASS | 0.1150 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 0.17 (run 8 px) (source score 0.073), frame 86 |

Review crops (source | decoded; _100, _200 nearest, _x8 local contrast):

- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\encode\p4_01_enc_beat07_goal\enc_beat07_goal\crops\x264_crf23_tenement_wall_f00342_100.png`
- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\encode\p4_01_enc_beat07_goal\enc_beat07_goal\crops\x264_crf23_tenement_wall_f00342_200.png`
- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\encode\p4_01_enc_beat07_goal\enc_beat07_goal\crops\x264_crf23_tenement_wall_f00342_x8.png`
