# Encode test: QA_enc_enc_beat02_axis_4k_nodither

ffmpeg: `C:\Users\<user>\AppData\Local\Microsoft\WinGet\Links\ffmpeg.EXE`, encoders: libvpx-vp9, libx264

Sequences: 1. Checks: 0 PASS, 1 FAIL, 0 WARN, 0 SKIP.

**Result: FAIL**

(Gate: x264 CRF 23 decode, 14.7 H1 (6); CRF 30 / VP9 rows are regression rows (INFO). PASS only when nothing FAILed and every clip measured a region; an H1 (6) clip without a region FAILs, any other reads SKIP.)

## enc_beat02_axis_4k (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\QA_enc_enc_beat02_axis_4k_nodither\enc_beat02_axis_4k` 3840x2160, 60 fps, 120 frames encoded

| Stream | Region | Score | Steps/1000 | Run px | Resid RMS | Resid p-p | kbit/s | MAE codes | Result |
|---|---|---|---|---|---|---|---|---|---|
| source | sky_above_facades | 4.462 | 0.00 | 142 | - | - | - | - | FAIL |
| x264 crf 23 | sky_above_facades | 4.781 | 2.31 | 92 | - | - | 11294 | 1.397 | FAIL |

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| x264_crf23[sky_above_facades] | FAIL | 4.7810 | score <= 1 | visible steps/1000 2.31 (max 1.17 code), BandScore 7.17 (run 92 px) (source score 4.462), frame 0; Y 0.00 / 0.00, Cb 2.31 / 1.17, Cr 0.00 / 0.00 (steps/1000 / max code) |

Review crops (source | decoded; _100, _200 nearest, _x8 local contrast):

- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\encode\QA_enc_enc_beat02_axis_4k_nodither\enc_beat02_axis_4k\crops\x264_crf23_sky_above_facades_f00068_100.png`
- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\encode\QA_enc_enc_beat02_axis_4k_nodither\enc_beat02_axis_4k\crops\x264_crf23_sky_above_facades_f00068_200.png`
- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\encode\QA_enc_enc_beat02_axis_4k_nodither\enc_beat02_axis_4k\crops\x264_crf23_sky_above_facades_f00068_x8.png`
