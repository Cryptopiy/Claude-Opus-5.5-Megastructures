# Encode test: QA_enc_enc_beat01_wall

ffmpeg: `C:\Users\<user>\AppData\Local\Microsoft\WinGet\Links\ffmpeg.EXE`, encoders: libvpx-vp9, libx264

Sequences: 1. Checks: 1 PASS, 0 FAIL, 0 WARN, 0 SKIP.

**Result: PASS**

(Gate: x264 CRF 23 decode, 14.7 H1 (6); CRF 30 / VP9 rows are regression rows (INFO). PASS only when nothing FAILed and every clip measured a region; an H1 (6) clip without a region FAILs, any other reads SKIP.)

## enc_beat01_wall (PASS)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\QA_enc_enc_beat01_wall\enc_beat01_wall` 1920x1080, 60 fps, 240 frames encoded

| Stream | Region | Score | Steps/1000 | Run px | Resid RMS | Resid p-p | kbit/s | MAE codes | Result |
|---|---|---|---|---|---|---|---|---|---|
| source | alley_wall | 6.166 | 0.00 | 10 | - | - | - | - | FAIL |
| x264 crf 23 | alley_wall | 10.075 | 0.00 | 27 | - | - | 3623 | 1.931 | FAIL |
| x264 crf 30 | alley_wall | 9.896 | 0.00 | 17 | - | - | 1261 | 2.518 | FAIL |
| vp9 crf 31 | alley_wall | 12.797 | 0.00 | 27 | - | - | 3846 | 1.787 | FAIL |

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| x264_crf23[alley_wall] | PASS | 0.0000 | <= 2 steps / 1000, max <= 1.5 code | visible steps/1000 0.00 (max 0.00 code), BandScore 15.11 (run 27 px) (source score 6.166), frame 137; Y 0.00 / 0.00, Cb 0.00 / 0.00, Cr 0.00 / 0.00 (steps/1000 / max code) |
| x264_crf30[alley_wall] | INFO | 0.0000 | <= 2 steps / 1000, max <= 1.5 code | visible steps/1000 0.00 (max 0.00 code), BandScore 14.84 (run 17 px) (source score 6.166), frame 102; Y 0.00 / 0.00, Cb 0.00 / 0.00, Cr 0.00 / 0.00 (steps/1000 / max code); regression row (the gate is x264 CRF 23) |
| vp9_crf31[alley_wall] | INFO | 0.0000 | <= 2 steps / 1000, max <= 1.5 code | visible steps/1000 0.00 (max 0.00 code), BandScore 19.20 (run 27 px) (source score 6.166), frame 102; Y 0.00 / 0.00, Cb 0.00 / 0.00, Cr 0.00 / 0.00 (steps/1000 / max code); regression row (the gate is x264 CRF 23) |

Review crops (source | decoded; _100, _200 nearest, _x8 local contrast):

- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\encode\QA_enc_enc_beat01_wall\enc_beat01_wall\crops\x264_crf23_alley_wall_f00137_100.png`
- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\encode\QA_enc_enc_beat01_wall\enc_beat01_wall\crops\x264_crf23_alley_wall_f00137_200.png`
- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\encode\QA_enc_enc_beat01_wall\enc_beat01_wall\crops\x264_crf23_alley_wall_f00137_x8.png`
- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\encode\QA_enc_enc_beat01_wall\enc_beat01_wall\crops\x264_crf30_alley_wall_f00137_100.png`
- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\encode\QA_enc_enc_beat01_wall\enc_beat01_wall\crops\x264_crf30_alley_wall_f00137_200.png`
- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\encode\QA_enc_enc_beat01_wall\enc_beat01_wall\crops\x264_crf30_alley_wall_f00137_x8.png`
- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\encode\QA_enc_enc_beat01_wall\enc_beat01_wall\crops\vp9_crf31_alley_wall_f00137_100.png`
- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\encode\QA_enc_enc_beat01_wall\enc_beat01_wall\crops\vp9_crf31_alley_wall_f00137_200.png`
- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\encode\QA_enc_enc_beat01_wall\enc_beat01_wall\crops\vp9_crf31_alley_wall_f00137_x8.png`
