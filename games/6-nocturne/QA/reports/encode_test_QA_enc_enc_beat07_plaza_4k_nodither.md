# Encode test: QA_enc_enc_beat07_plaza_4k_nodither

ffmpeg: `C:\Users\<user>\AppData\Local\Microsoft\WinGet\Links\ffmpeg.EXE`, encoders: libvpx-vp9, libx264

Sequences: 1. Checks: 0 PASS, 2 FAIL, 0 WARN, 0 SKIP.

**Result: FAIL**

(Gate: x264 CRF 23 decode, 14.7 H1 (6); CRF 30 / VP9 rows are regression rows (INFO). PASS only when nothing FAILed and every clip measured a region; an H1 (6) clip without a region FAILs, any other reads SKIP.)

## enc_beat07_plaza_4k (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\QA_enc_enc_beat07_plaza_4k_nodither\enc_beat07_plaza_4k` 3840x2160, 60 fps, 120 frames encoded

| Stream | Region | Score | Steps/1000 | Run px | Resid RMS | Resid p-p | kbit/s | MAE codes | Result |
|---|---|---|---|---|---|---|---|---|---|
| source | sky_koi_spire | 2.787 | 0.00 | 96 | - | - | - | - | FAIL |
| source | plaza | 2.000 | 4.18 | 28 | - | - | - | - | FAIL |
| x264 crf 23 | sky_koi_spire | 3.102 | 3.29 | 96 | - | - | 8204 | 1.218 | FAIL |
| x264 crf 23 | plaza | 2.092 | 4.18 | 104 | - | - | 8204 | 1.218 | FAIL |

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| x264_crf23[sky_koi_spire] | FAIL | 3.2890 | <= 2 steps / 1000, max <= 1.5 code | visible steps/1000 3.29 (max 1.34 code), BandScore 4.65 (run 96 px) (source score 2.787), frame 0; Y 0.00 / 0.00, Cb 3.29 / 1.34, Cr 0.00 / 0.00 (steps/1000 / max code) |
| x264_crf23[plaza] | FAIL | 4.1840 | <= 2 steps / 1000, max <= 1.5 code | visible steps/1000 4.18 (max 1.33 code), BandScore 5.59 (run 104 px) (source score 2.000), frame 0; Y 0.00 / 0.00, Cb 8.37 / 1.33, Cr 4.18 / 1.11 (steps/1000 / max code) |

Review crops (source | decoded; _100, _200 nearest, _x8 local contrast):

- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\encode\QA_enc_enc_beat07_plaza_4k_nodither\enc_beat07_plaza_4k\crops\x264_crf23_sky_koi_spire_f00068_100.png`
- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\encode\QA_enc_enc_beat07_plaza_4k_nodither\enc_beat07_plaza_4k\crops\x264_crf23_sky_koi_spire_f00068_200.png`
- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\encode\QA_enc_enc_beat07_plaza_4k_nodither\enc_beat07_plaza_4k\crops\x264_crf23_sky_koi_spire_f00068_x8.png`
- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\encode\QA_enc_enc_beat07_plaza_4k_nodither\enc_beat07_plaza_4k\crops\x264_crf23_plaza_f00068_100.png`
- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\encode\QA_enc_enc_beat07_plaza_4k_nodither\enc_beat07_plaza_4k\crops\x264_crf23_plaza_f00068_200.png`
- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\encode\QA_enc_enc_beat07_plaza_4k_nodither\enc_beat07_plaza_4k\crops\x264_crf23_plaza_f00068_x8.png`
