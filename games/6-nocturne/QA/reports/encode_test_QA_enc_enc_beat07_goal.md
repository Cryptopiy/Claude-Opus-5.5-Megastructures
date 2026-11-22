# Encode test: QA_enc_enc_beat07_goal

ffmpeg: `C:\Users\<user>\AppData\Local\Microsoft\WinGet\Links\ffmpeg.EXE`, encoders: libvpx-vp9, libx264

Sequences: 1. Checks: 1 PASS, 0 FAIL, 0 WARN, 0 SKIP.

**Result: PASS**

(Gate: x264 CRF 23 decode, 14.7 H1 (6); CRF 30 / VP9 rows are regression rows (INFO). PASS only when nothing FAILed and every clip measured a region; an H1 (6) clip without a region FAILs, any other reads SKIP.)

## enc_beat07_goal (PASS)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\QA_enc_enc_beat07_goal\enc_beat07_goal` 1920x1080, 60 fps, 240 frames encoded

| Stream | Region | Score | Steps/1000 | Run px | Resid RMS | Resid p-p | kbit/s | MAE codes | Result |
|---|---|---|---|---|---|---|---|---|---|
| source | tenement_wall | 0.073 | 0.00 | 5 | - | - | - | - | PASS |
| x264 crf 23 | tenement_wall | 0.146 | 0.00 | 10 | - | - | 569 | 1.759 | PASS |
| x264 crf 30 | tenement_wall | 0.400 | 0.00 | 30 | - | - | 195 | 2.240 | PASS |
| vp9 crf 31 | tenement_wall | 0.253 | 0.00 | 17 | - | - | 868 | 1.682 | PASS |

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| x264_crf23[tenement_wall] | PASS | 0.0000 | <= 2 steps / 1000, max <= 1.5 code | visible steps/1000 0.00 (max 0.00 code), BandScore 0.22 (run 10 px) (source score 0.073), frame 34; Y 0.00 / 0.00, Cb 0.00 / 0.00, Cr 0.00 / 0.00 (steps/1000 / max code) |
| x264_crf30[tenement_wall] | INFO | 0.0000 | <= 2 steps / 1000, max <= 1.5 code | visible steps/1000 0.00 (max 0.00 code), BandScore 0.60 (run 30 px) (source score 0.073), frame 0; Y 0.00 / 0.00, Cb 0.00 / 0.00, Cr 0.00 / 0.00 (steps/1000 / max code); regression row (the gate is x264 CRF 23) |
| vp9_crf31[tenement_wall] | INFO | 0.0000 | <= 2 steps / 1000, max <= 1.5 code | visible steps/1000 0.00 (max 0.00 code), BandScore 0.38 (run 17 px) (source score 0.073), frame 0; Y 0.00 / 0.00, Cb 0.00 / 0.00, Cr 0.00 / 0.00 (steps/1000 / max code); regression row (the gate is x264 CRF 23) |

Review crops (source | decoded; _100, _200 nearest, _x8 local contrast):

- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\encode\QA_enc_enc_beat07_goal\enc_beat07_goal\crops\x264_crf23_tenement_wall_f00137_100.png`
- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\encode\QA_enc_enc_beat07_goal\enc_beat07_goal\crops\x264_crf23_tenement_wall_f00137_200.png`
- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\encode\QA_enc_enc_beat07_goal\enc_beat07_goal\crops\x264_crf23_tenement_wall_f00137_x8.png`
- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\encode\QA_enc_enc_beat07_goal\enc_beat07_goal\crops\x264_crf30_tenement_wall_f00137_100.png`
- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\encode\QA_enc_enc_beat07_goal\enc_beat07_goal\crops\x264_crf30_tenement_wall_f00137_200.png`
- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\encode\QA_enc_enc_beat07_goal\enc_beat07_goal\crops\x264_crf30_tenement_wall_f00137_x8.png`
- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\encode\QA_enc_enc_beat07_goal\enc_beat07_goal\crops\vp9_crf31_tenement_wall_f00137_100.png`
- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\encode\QA_enc_enc_beat07_goal\enc_beat07_goal\crops\vp9_crf31_tenement_wall_f00137_200.png`
- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\encode\QA_enc_enc_beat07_goal\enc_beat07_goal\crops\vp9_crf31_tenement_wall_f00137_x8.png`
