# Encode test: QA_enc_enc_beat07_plaza_4k

ffmpeg: `C:\Users\<user>\AppData\Local\Microsoft\WinGet\Links\ffmpeg.EXE`, encoders: libvpx-vp9, libx264

Sequences: 1. Checks: 0 PASS, 2 FAIL, 0 WARN, 0 SKIP.

**Result: FAIL**

(Gate: x264 CRF 23 decode, 14.7 H1 (6); CRF 30 / VP9 rows are regression rows (INFO). PASS only when nothing FAILed and every clip measured a region; an H1 (6) clip without a region FAILs, any other reads SKIP.)

## enc_beat07_plaza_4k (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\QA_enc_enc_beat07_plaza_4k\enc_beat07_plaza_4k` 3840x2160, 60 fps, 120 frames encoded

| Stream | Region | Score | Steps/1000 | Run px | Resid RMS | Resid p-p | kbit/s | MAE codes | Result |
|---|---|---|---|---|---|---|---|---|---|
| source | sky_koi_spire | 0.638 | 0.00 | 8 | - | - | - | - | PASS |
| source | plaza | 2.000 | 4.18 | 8 | - | - | - | - | FAIL |
| x264 crf 23 | sky_koi_spire | 2.516 | 3.29 | 88 | - | - | 7981 | 1.555 | FAIL |
| x264 crf 23 | plaza | 2.092 | 4.18 | 77 | - | - | 7981 | 1.555 | FAIL |
| x264 crf 30 | sky_koi_spire | 2.862 | 3.29 | 95 | - | - | 3170 | 2.010 | FAIL |
| x264 crf 30 | plaza | 4.184 | 8.37 | 184 | - | - | 3170 | 2.010 | FAIL |
| vp9 crf 31 | sky_koi_spire | 3.289 | 6.58 | 99 | - | - | 6845 | 1.440 | FAIL |
| vp9 crf 31 | plaza | 4.184 | 8.37 | 52 | - | - | 6845 | 1.440 | FAIL |

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| x264_crf23[sky_koi_spire] | FAIL | 3.2890 | <= 2 steps / 1000, max <= 1.5 code | visible steps/1000 3.29 (max 1.99 code), BandScore 3.77 (run 88 px) (source score 0.638), frame 0; Y 3.29 / 1.24, Cb 0.00 / 0.00, Cr 0.00 / 0.00 (steps/1000 / max code) |
| x264_crf23[plaza] | FAIL | 4.1840 | <= 2 steps / 1000, max <= 1.5 code | visible steps/1000 4.18 (max 1.05 code), BandScore 6.04 (run 77 px) (source score 2.000), frame 0; Y 0.00 / 0.00, Cb 8.37 / 1.34, Cr 4.18 / 1.28 (steps/1000 / max code) |
| x264_crf30[sky_koi_spire] | INFO | 3.2890 | <= 2 steps / 1000, max <= 1.5 code | visible steps/1000 3.29 (max 1.20 code), BandScore 4.29 (run 95 px) (source score 0.638), frame 0; Y 0.00 / 0.00, Cb 0.00 / 0.00, Cr 0.00 / 0.00 (steps/1000 / max code); regression row (the gate is x264 CRF 23) |
| x264_crf30[plaza] | INFO | 8.3680 | <= 2 steps / 1000, max <= 1.5 code | visible steps/1000 8.37 (max 3.52 code), BandScore 8.92 (run 184 px) (source score 2.000), frame 0; Y 4.18 / 1.35, Cb 8.37 / 1.42, Cr 4.18 / 1.23 (steps/1000 / max code); regression row (the gate is x264 CRF 23) |
| vp9_crf31[sky_koi_spire] | INFO | 6.5790 | <= 2 steps / 1000, max <= 1.5 code | visible steps/1000 6.58 (max 2.00 code), BandScore 3.99 (run 99 px) (source score 0.638), frame 68; Y 3.29 / 1.21, Cb 0.00 / 0.00, Cr 6.58 / 1.02 (steps/1000 / max code); regression row (the gate is x264 CRF 23) |
| vp9_crf31[plaza] | INFO | 8.3680 | <= 2 steps / 1000, max <= 1.5 code | visible steps/1000 8.37 (max 1.36 code), BandScore 3.15 (run 52 px) (source score 2.000), frame 0; Y 0.00 / 0.00, Cb 8.37 / 1.36, Cr 8.37 / 1.28 (steps/1000 / max code); regression row (the gate is x264 CRF 23) |

Review crops (source | decoded; _100, _200 nearest, _x8 local contrast):

- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\encode\QA_enc_enc_beat07_plaza_4k\enc_beat07_plaza_4k\crops\x264_crf23_sky_koi_spire_f00068_100.png`
- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\encode\QA_enc_enc_beat07_plaza_4k\enc_beat07_plaza_4k\crops\x264_crf23_sky_koi_spire_f00068_200.png`
- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\encode\QA_enc_enc_beat07_plaza_4k\enc_beat07_plaza_4k\crops\x264_crf23_sky_koi_spire_f00068_x8.png`
- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\encode\QA_enc_enc_beat07_plaza_4k\enc_beat07_plaza_4k\crops\x264_crf23_plaza_f00068_100.png`
- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\encode\QA_enc_enc_beat07_plaza_4k\enc_beat07_plaza_4k\crops\x264_crf23_plaza_f00068_200.png`
- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\encode\QA_enc_enc_beat07_plaza_4k\enc_beat07_plaza_4k\crops\x264_crf23_plaza_f00068_x8.png`
- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\encode\QA_enc_enc_beat07_plaza_4k\enc_beat07_plaza_4k\crops\x264_crf30_sky_koi_spire_f00068_100.png`
- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\encode\QA_enc_enc_beat07_plaza_4k\enc_beat07_plaza_4k\crops\x264_crf30_sky_koi_spire_f00068_200.png`
- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\encode\QA_enc_enc_beat07_plaza_4k\enc_beat07_plaza_4k\crops\x264_crf30_sky_koi_spire_f00068_x8.png`
- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\encode\QA_enc_enc_beat07_plaza_4k\enc_beat07_plaza_4k\crops\x264_crf30_plaza_f00068_100.png`
- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\encode\QA_enc_enc_beat07_plaza_4k\enc_beat07_plaza_4k\crops\x264_crf30_plaza_f00068_200.png`
- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\encode\QA_enc_enc_beat07_plaza_4k\enc_beat07_plaza_4k\crops\x264_crf30_plaza_f00068_x8.png`
- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\encode\QA_enc_enc_beat07_plaza_4k\enc_beat07_plaza_4k\crops\vp9_crf31_sky_koi_spire_f00068_100.png`
- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\encode\QA_enc_enc_beat07_plaza_4k\enc_beat07_plaza_4k\crops\vp9_crf31_sky_koi_spire_f00068_200.png`
- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\encode\QA_enc_enc_beat07_plaza_4k\enc_beat07_plaza_4k\crops\vp9_crf31_sky_koi_spire_f00068_x8.png`
- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\encode\QA_enc_enc_beat07_plaza_4k\enc_beat07_plaza_4k\crops\vp9_crf31_plaza_f00068_100.png`
- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\encode\QA_enc_enc_beat07_plaza_4k\enc_beat07_plaza_4k\crops\vp9_crf31_plaza_f00068_200.png`
- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\encode\QA_enc_enc_beat07_plaza_4k\enc_beat07_plaza_4k\crops\vp9_crf31_plaza_f00068_x8.png`
