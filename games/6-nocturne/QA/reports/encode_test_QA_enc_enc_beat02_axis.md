# Encode test: QA_enc_enc_beat02_axis

ffmpeg: `C:\Users\<user>\AppData\Local\Microsoft\WinGet\Links\ffmpeg.EXE`, encoders: libvpx-vp9, libx264

Sequences: 1. Checks: 0 PASS, 1 FAIL, 0 WARN, 0 SKIP.

**Result: FAIL**

(Gate: x264 CRF 23 decode, 14.7 H1 (6); CRF 30 / VP9 rows are regression rows (INFO). PASS only when nothing FAILed and every clip measured a region; an H1 (6) clip without a region FAILs, any other reads SKIP.)

## enc_beat02_axis (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\QA_enc_enc_beat02_axis\enc_beat02_axis` 1920x1080, 60 fps, 240 frames encoded

| Stream | Region | Score | Steps/1000 | Run px | Resid RMS | Resid p-p | kbit/s | MAE codes | Result |
|---|---|---|---|---|---|---|---|---|---|
| source | sky_above_facades | 0.597 | 0.00 | 9 | - | - | - | - | PASS |
| x264 crf 23 | sky_above_facades | 4.608 | 9.22 | 67 | - | - | 3675 | 2.019 | FAIL |
| x264 crf 30 | sky_above_facades | 4.608 | 9.22 | 47 | - | - | 1452 | 2.779 | FAIL |
| vp9 crf 31 | sky_above_facades | 6.912 | 13.82 | 75 | - | - | 5496 | 1.731 | FAIL |

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| x264_crf23[sky_above_facades] | FAIL | 4.6080 | score <= 1 | visible steps/1000 9.22 (max 1.18 code), BandScore 3.61 (run 67 px) (source score 0.597), frame 34; Y 0.00 / 0.00, Cb 0.00 / 0.00, Cr 9.22 / 1.18 (steps/1000 / max code) |
| x264_crf30[sky_above_facades] | INFO | 4.6080 | score <= 1 | visible steps/1000 9.22 (max 1.37 code), BandScore 3.95 (run 47 px) (source score 0.597), frame 0; Y 0.00 / 0.00, Cb 9.22 / 1.37, Cr 0.00 / 0.00 (steps/1000 / max code); regression row (the gate is x264 CRF 23) |
| vp9_crf31[sky_above_facades] | INFO | 6.9120 | score <= 1 | visible steps/1000 13.82 (max 1.20 code), BandScore 3.75 (run 75 px) (source score 0.597), frame 102; Y 0.00 / 0.00, Cb 13.82 / 1.12, Cr 4.61 / 1.20 (steps/1000 / max code); regression row (the gate is x264 CRF 23) |

Review crops (source | decoded; _100, _200 nearest, _x8 local contrast):

- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\encode\QA_enc_enc_beat02_axis\enc_beat02_axis\crops\x264_crf23_sky_above_facades_f00137_100.png`
- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\encode\QA_enc_enc_beat02_axis\enc_beat02_axis\crops\x264_crf23_sky_above_facades_f00137_200.png`
- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\encode\QA_enc_enc_beat02_axis\enc_beat02_axis\crops\x264_crf23_sky_above_facades_f00137_x8.png`
- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\encode\QA_enc_enc_beat02_axis\enc_beat02_axis\crops\x264_crf30_sky_above_facades_f00137_100.png`
- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\encode\QA_enc_enc_beat02_axis\enc_beat02_axis\crops\x264_crf30_sky_above_facades_f00137_200.png`
- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\encode\QA_enc_enc_beat02_axis\enc_beat02_axis\crops\x264_crf30_sky_above_facades_f00137_x8.png`
- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\encode\QA_enc_enc_beat02_axis\enc_beat02_axis\crops\vp9_crf31_sky_above_facades_f00137_100.png`
- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\encode\QA_enc_enc_beat02_axis\enc_beat02_axis\crops\vp9_crf31_sky_above_facades_f00137_200.png`
- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\encode\QA_enc_enc_beat02_axis\enc_beat02_axis\crops\vp9_crf31_sky_above_facades_f00137_x8.png`
