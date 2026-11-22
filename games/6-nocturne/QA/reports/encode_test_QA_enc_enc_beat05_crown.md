# Encode test: QA_enc_enc_beat05_crown

ffmpeg: `C:\Users\<user>\AppData\Local\Microsoft\WinGet\Links\ffmpeg.EXE`, encoders: libvpx-vp9, libx264

Sequences: 1. Checks: 1 PASS, 0 FAIL, 0 WARN, 0 SKIP.

**Result: PASS**

(Gate: x264 CRF 23 decode, 14.7 H1 (6); CRF 30 / VP9 rows are regression rows (INFO). PASS only when nothing FAILed and every clip measured a region; an H1 (6) clip without a region FAILs, any other reads SKIP.)

## enc_beat05_crown (PASS)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\QA_enc_enc_beat05_crown\enc_beat05_crown` 1920x1080, 60 fps, 240 frames encoded

| Stream | Region | Score | Steps/1000 | Run px | Resid RMS | Resid p-p | kbit/s | MAE codes | Result |
|---|---|---|---|---|---|---|---|---|---|
| source | crown_gap | 1.299 | 0.00 | 5 | - | - | - | - | FAIL |
| x264 crf 23 | crown_gap | 5.042 | 0.00 | 27 | - | - | 6161 | 2.846 | FAIL |
| x264 crf 30 | crown_gap | 4.575 | 0.00 | 27 | - | - | 1806 | 3.741 | FAIL |
| vp9 crf 31 | crown_gap | 4.963 | 0.00 | 27 | - | - | 11809 | 2.538 | FAIL |

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| x264_crf23[crown_gap] | PASS | 0.0000 | <= 2 steps / 1000, max <= 1.5 code | visible steps/1000 0.00 (max 0.00 code), BandScore 7.56 (run 27 px) (source score 1.299), frame 205; Y 0.00 / 0.00, Cb 0.00 / 0.00, Cr 0.00 / 0.00 (steps/1000 / max code) |
| x264_crf30[crown_gap] | INFO | 0.0000 | <= 2 steps / 1000, max <= 1.5 code | visible steps/1000 0.00 (max 0.00 code), BandScore 6.86 (run 27 px) (source score 1.299), frame 205; Y 0.00 / 0.00, Cb 0.00 / 0.00, Cr 0.00 / 0.00 (steps/1000 / max code); regression row (the gate is x264 CRF 23) |
| vp9_crf31[crown_gap] | INFO | 0.0000 | <= 2 steps / 1000, max <= 1.5 code | visible steps/1000 0.00 (max 0.00 code), BandScore 7.45 (run 27 px) (source score 1.299), frame 205; Y 0.00 / 0.00, Cb 0.00 / 0.00, Cr 0.00 / 0.00 (steps/1000 / max code); regression row (the gate is x264 CRF 23) |

Review crops (source | decoded; _100, _200 nearest, _x8 local contrast):

- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\encode\QA_enc_enc_beat05_crown\enc_beat05_crown\crops\x264_crf23_crown_gap_f00137_100.png`
- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\encode\QA_enc_enc_beat05_crown\enc_beat05_crown\crops\x264_crf23_crown_gap_f00137_200.png`
- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\encode\QA_enc_enc_beat05_crown\enc_beat05_crown\crops\x264_crf23_crown_gap_f00137_x8.png`
- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\encode\QA_enc_enc_beat05_crown\enc_beat05_crown\crops\x264_crf30_crown_gap_f00137_100.png`
- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\encode\QA_enc_enc_beat05_crown\enc_beat05_crown\crops\x264_crf30_crown_gap_f00137_200.png`
- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\encode\QA_enc_enc_beat05_crown\enc_beat05_crown\crops\x264_crf30_crown_gap_f00137_x8.png`
- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\encode\QA_enc_enc_beat05_crown\enc_beat05_crown\crops\vp9_crf31_crown_gap_f00137_100.png`
- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\encode\QA_enc_enc_beat05_crown\enc_beat05_crown\crops\vp9_crf31_crown_gap_f00137_200.png`
- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\encode\QA_enc_enc_beat05_crown\enc_beat05_crown\crops\vp9_crf31_crown_gap_f00137_x8.png`
