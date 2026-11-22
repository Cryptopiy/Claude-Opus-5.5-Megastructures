# Encode test: INTB_enc_enc_beat07_plaza_4k

ffmpeg: `C:\Users\<user>\AppData\Local\Microsoft\WinGet\Links\ffmpeg.EXE`, encoders: libvpx-vp9, libx264

Sequences: 1. Checks: 2 PASS, 0 FAIL, 0 WARN, 0 SKIP.

**Result: PASS**

(Gate: x264 CRF 23 decode, 14.7 H1 (6); CRF 30 / VP9 rows are regression rows (INFO). PASS only when nothing FAILed and every clip measured a region; an H1 (6) clip without a region FAILs, any other reads SKIP.)

## enc_beat07_plaza_4k (PASS)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\INTB_enc_enc_beat07_plaza_4k\enc_beat07_plaza_4k` 3840x2160, 60 fps, 120 frames encoded

| Stream | Region | Score | Steps/1000 | Y | Cb | Cr | R/G/B max | Run px | Resid RMS | Resid p-p | kbit/s | MAE codes | Result |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| source | sky_koi_spire | 0.677 | 0.00 | 0.00 (0.00) | 0.00 (0.00) | 0.00 (0.00) | 0.00 | 10 | - | - | - | - | PASS |
| source | plaza | 2.000 | 4.18 | 0.00 (0.00) | 0.00 (0.00) | 4.18 (1.94) | 0.00 | 9 | - | - | - | - | FAIL |
| x264 crf 23 | sky_koi_spire | 2.775 | 0.00 | 0.00 (0.00) | 0.00 (0.00) | 0.00 (0.00) | 0.00 | 68 | - | - | 8162 | 1.542 | FAIL |
| x264 crf 23 | plaza | 0.000 | 0.00 | 0.00 (0.00) | 0.00 (0.00) | 4.18 (1.06) | 4.18 | 83 | - | - | 8162 | 1.542 | PASS |

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| x264_crf23[sky_koi_spire] | PASS | 0.0000 | <= 2 steps / 1000, max <= 1.5 code | visible steps/1000 0.00 (max 0.00 code), BandScore 4.16 (run 68 px) (source score 0.677), frame 0; luma Y 0.00 / 0.00; chroma Cb 0.00 / 0.00, Cr 0.00 / 0.00 (steps/1000 / max code, gated); R 0.00, G 0.00, B 0.00 steps/1000 (reported, not gated on a decode, D-041 2) |
| x264_crf23[plaza] | PASS | 0.0000 | <= 2 steps / 1000, max <= 1.5 code | visible steps/1000 0.00 (max 0.00 code), BandScore 9.45 (run 83 px) (source score 2.000), frame 0; luma Y 0.00 / 0.00; chroma Cb 0.00 / 0.00, Cr 4.18 / 1.06 (steps/1000 / max code, gated); R 0.00, G 4.18, B 4.18 steps/1000 (reported, not gated on a decode, D-041 2) |

Review crops (source | decoded; _100, _200 nearest, _x8 local contrast):

- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\encode\INTB_enc_enc_beat07_plaza_4k\enc_beat07_plaza_4k\crops\x264_crf23_sky_koi_spire_f00068_100.png`
- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\encode\INTB_enc_enc_beat07_plaza_4k\enc_beat07_plaza_4k\crops\x264_crf23_sky_koi_spire_f00068_200.png`
- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\encode\INTB_enc_enc_beat07_plaza_4k\enc_beat07_plaza_4k\crops\x264_crf23_sky_koi_spire_f00068_x8.png`
- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\encode\INTB_enc_enc_beat07_plaza_4k\enc_beat07_plaza_4k\crops\x264_crf23_plaza_f00068_100.png`
- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\encode\INTB_enc_enc_beat07_plaza_4k\enc_beat07_plaza_4k\crops\x264_crf23_plaza_f00068_200.png`
- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\encode\INTB_enc_enc_beat07_plaza_4k\enc_beat07_plaza_4k\crops\x264_crf23_plaza_f00068_x8.png`
