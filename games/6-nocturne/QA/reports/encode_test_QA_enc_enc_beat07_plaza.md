# Encode test: QA_enc_enc_beat07_plaza

ffmpeg: `C:\Users\<user>\AppData\Local\Microsoft\WinGet\Links\ffmpeg.EXE`, encoders: libvpx-vp9, libx264

Sequences: 1. Checks: 1 PASS, 1 FAIL, 0 WARN, 0 SKIP.

**Result: FAIL**

(Gate: x264 CRF 23 decode, 14.7 H1 (6); CRF 30 / VP9 rows are regression rows (INFO). PASS only when nothing FAILed and every clip measured a region; an H1 (6) clip without a region FAILs, any other reads SKIP.)

## enc_beat07_plaza (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\QA_enc_enc_beat07_plaza\enc_beat07_plaza` 1920x1080, 60 fps, 240 frames encoded

| Stream | Region | Score | Steps/1000 | Run px | Resid RMS | Resid p-p | kbit/s | MAE codes | Result |
|---|---|---|---|---|---|---|---|---|---|
| source | sky_koi_spire | 1.323 | 0.00 | 9 | - | - | - | - | FAIL |
| source | plaza | 0.000 | 0.00 | 11 | - | - | - | - | PASS |
| x264 crf 23 | sky_koi_spire | 3.781 | 0.00 | 56 | - | - | 2100 | 1.665 | FAIL |
| x264 crf 23 | plaza | 4.167 | 8.33 | 72 | - | - | 2100 | 1.665 | FAIL |
| x264 crf 30 | sky_koi_spire | 3.289 | 6.58 | 82 | - | - | 856 | 2.187 | FAIL |
| x264 crf 30 | plaza | 4.167 | 8.33 | 120 | - | - | 856 | 2.187 | FAIL |
| vp9 crf 31 | sky_koi_spire | 3.485 | 6.58 | 60 | - | - | 2368 | 1.512 | FAIL |
| vp9 crf 31 | plaza | 4.167 | 8.33 | 45 | - | - | 2368 | 1.512 | FAIL |

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| x264_crf23[sky_koi_spire] | PASS | 0.0000 | <= 2 steps / 1000, max <= 1.5 code | visible steps/1000 0.00 (max 0.00 code), BandScore 5.67 (run 56 px) (source score 1.323), frame 34; Y 0.00 / 0.00, Cb 0.00 / 0.00, Cr 0.00 / 0.00 (steps/1000 / max code) |
| x264_crf23[plaza] | FAIL | 8.3330 | <= 2 steps / 1000, max <= 1.5 code | visible steps/1000 8.33 (max 2.11 code), BandScore 6.50 (run 72 px) (source score 0.000), frame 0; Y 8.33 / 1.55, Cb 8.33 / 1.26, Cr 8.33 / 1.06 (steps/1000 / max code) |
| x264_crf30[sky_koi_spire] | INFO | 6.5790 | <= 2 steps / 1000, max <= 1.5 code | visible steps/1000 6.58 (max 1.58 code), BandScore 4.51 (run 82 px) (source score 1.323), frame 0; Y 0.00 / 0.00, Cb 0.00 / 0.00, Cr 0.00 / 0.00 (steps/1000 / max code); regression row (the gate is x264 CRF 23) |
| x264_crf30[plaza] | INFO | 8.3330 | <= 2 steps / 1000, max <= 1.5 code | visible steps/1000 8.33 (max 1.99 code), BandScore 6.88 (run 120 px) (source score 0.000), frame 0; Y 8.33 / 1.98, Cb 8.33 / 1.32, Cr 0.00 / 0.00 (steps/1000 / max code); regression row (the gate is x264 CRF 23) |
| vp9_crf31[sky_koi_spire] | INFO | 6.5790 | <= 2 steps / 1000, max <= 1.5 code | visible steps/1000 6.58 (max 1.03 code), BandScore 5.23 (run 60 px) (source score 1.323), frame 137; Y 0.00 / 0.00, Cb 0.00 / 0.00, Cr 6.58 / 1.03 (steps/1000 / max code); regression row (the gate is x264 CRF 23) |
| vp9_crf31[plaza] | INFO | 8.3330 | <= 2 steps / 1000, max <= 1.5 code | visible steps/1000 8.33 (max 1.22 code), BandScore 5.52 (run 45 px) (source score 0.000), frame 0; Y 0.00 / 0.00, Cb 8.33 / 1.28, Cr 8.33 / 1.23 (steps/1000 / max code); regression row (the gate is x264 CRF 23) |

Review crops (source | decoded; _100, _200 nearest, _x8 local contrast):

- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\encode\QA_enc_enc_beat07_plaza\enc_beat07_plaza\crops\x264_crf23_sky_koi_spire_f00137_100.png`
- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\encode\QA_enc_enc_beat07_plaza\enc_beat07_plaza\crops\x264_crf23_sky_koi_spire_f00137_200.png`
- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\encode\QA_enc_enc_beat07_plaza\enc_beat07_plaza\crops\x264_crf23_sky_koi_spire_f00137_x8.png`
- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\encode\QA_enc_enc_beat07_plaza\enc_beat07_plaza\crops\x264_crf23_plaza_f00137_100.png`
- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\encode\QA_enc_enc_beat07_plaza\enc_beat07_plaza\crops\x264_crf23_plaza_f00137_200.png`
- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\encode\QA_enc_enc_beat07_plaza\enc_beat07_plaza\crops\x264_crf23_plaza_f00137_x8.png`
- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\encode\QA_enc_enc_beat07_plaza\enc_beat07_plaza\crops\x264_crf30_sky_koi_spire_f00137_100.png`
- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\encode\QA_enc_enc_beat07_plaza\enc_beat07_plaza\crops\x264_crf30_sky_koi_spire_f00137_200.png`
- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\encode\QA_enc_enc_beat07_plaza\enc_beat07_plaza\crops\x264_crf30_sky_koi_spire_f00137_x8.png`
- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\encode\QA_enc_enc_beat07_plaza\enc_beat07_plaza\crops\x264_crf30_plaza_f00137_100.png`
- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\encode\QA_enc_enc_beat07_plaza\enc_beat07_plaza\crops\x264_crf30_plaza_f00137_200.png`
- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\encode\QA_enc_enc_beat07_plaza\enc_beat07_plaza\crops\x264_crf30_plaza_f00137_x8.png`
- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\encode\QA_enc_enc_beat07_plaza\enc_beat07_plaza\crops\vp9_crf31_sky_koi_spire_f00137_100.png`
- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\encode\QA_enc_enc_beat07_plaza\enc_beat07_plaza\crops\vp9_crf31_sky_koi_spire_f00137_200.png`
- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\encode\QA_enc_enc_beat07_plaza\enc_beat07_plaza\crops\vp9_crf31_sky_koi_spire_f00137_x8.png`
- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\encode\QA_enc_enc_beat07_plaza\enc_beat07_plaza\crops\vp9_crf31_plaza_f00137_100.png`
- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\encode\QA_enc_enc_beat07_plaza\enc_beat07_plaza\crops\vp9_crf31_plaza_f00137_200.png`
- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\encode\QA_enc_enc_beat07_plaza\enc_beat07_plaza\crops\vp9_crf31_plaza_f00137_x8.png`
