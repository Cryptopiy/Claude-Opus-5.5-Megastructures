# Encode test: QA_enc_enc_beat02_axis_4k

ffmpeg: `C:\Users\<user>\AppData\Local\Microsoft\WinGet\Links\ffmpeg.EXE`, encoders: libvpx-vp9, libx264

Sequences: 1. Checks: 0 PASS, 1 FAIL, 0 WARN, 0 SKIP.

**Result: FAIL**

(Gate: x264 CRF 23 decode, 14.7 H1 (6); CRF 30 / VP9 rows are regression rows (INFO). PASS only when nothing FAILed and every clip measured a region; an H1 (6) clip without a region FAILs, any other reads SKIP.)

## enc_beat02_axis_4k (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\QA_enc_enc_beat02_axis_4k\enc_beat02_axis_4k` 3840x2160, 60 fps, 120 frames encoded

| Stream | Region | Score | Steps/1000 | Run px | Resid RMS | Resid p-p | kbit/s | MAE codes | Result |
|---|---|---|---|---|---|---|---|---|---|
| source | sky_above_facades | 2.000 | 2.31 | 7 | - | - | - | - | FAIL |
| x264 crf 23 | sky_above_facades | 3.504 | 2.31 | 70 | - | - | 11270 | 1.726 | FAIL |
| x264 crf 30 | sky_above_facades | 4.455 | 2.31 | 88 | - | - | 4399 | 2.310 | FAIL |
| vp9 crf 31 | sky_above_facades | 4.262 | 2.31 | 103 | - | - | 12598 | 1.546 | FAIL |

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| x264_crf23[sky_above_facades] | FAIL | 3.5040 | score <= 1 | visible steps/1000 2.31 (max 1.08 code), BandScore 5.26 (run 70 px) (source score 2.000), frame 51; Y 0.00 / 0.00, Cb 0.00 / 0.00, Cr 2.31 / 1.08 (steps/1000 / max code) |
| x264_crf30[sky_above_facades] | INFO | 4.4550 | score <= 1 | visible steps/1000 2.31 (max 1.87 code), BandScore 6.68 (run 88 px) (source score 2.000), frame 0; Y 2.31 / 1.01, Cb 2.31 / 1.36, Cr 0.00 / 0.00 (steps/1000 / max code); regression row (the gate is x264 CRF 23) |
| vp9_crf31[sky_above_facades] | INFO | 4.2620 | score <= 1 | visible steps/1000 2.31 (max 2.27 code), BandScore 6.39 (run 103 px) (source score 2.000), frame 85; Y 0.00 / 0.00, Cb 2.31 / 1.24, Cr 0.00 / 0.00 (steps/1000 / max code); regression row (the gate is x264 CRF 23) |

Review crops (source | decoded; _100, _200 nearest, _x8 local contrast):

- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\encode\QA_enc_enc_beat02_axis_4k\enc_beat02_axis_4k\crops\x264_crf23_sky_above_facades_f00068_100.png`
- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\encode\QA_enc_enc_beat02_axis_4k\enc_beat02_axis_4k\crops\x264_crf23_sky_above_facades_f00068_200.png`
- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\encode\QA_enc_enc_beat02_axis_4k\enc_beat02_axis_4k\crops\x264_crf23_sky_above_facades_f00068_x8.png`
- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\encode\QA_enc_enc_beat02_axis_4k\enc_beat02_axis_4k\crops\x264_crf30_sky_above_facades_f00068_100.png`
- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\encode\QA_enc_enc_beat02_axis_4k\enc_beat02_axis_4k\crops\x264_crf30_sky_above_facades_f00068_200.png`
- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\encode\QA_enc_enc_beat02_axis_4k\enc_beat02_axis_4k\crops\x264_crf30_sky_above_facades_f00068_x8.png`
- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\encode\QA_enc_enc_beat02_axis_4k\enc_beat02_axis_4k\crops\vp9_crf31_sky_above_facades_f00068_100.png`
- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\encode\QA_enc_enc_beat02_axis_4k\enc_beat02_axis_4k\crops\vp9_crf31_sky_above_facades_f00068_200.png`
- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\encode\QA_enc_enc_beat02_axis_4k\enc_beat02_axis_4k\crops\vp9_crf31_sky_above_facades_f00068_x8.png`
