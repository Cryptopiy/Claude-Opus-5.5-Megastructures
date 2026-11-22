# Encode test: QA_enc_enc_beat02_sky

ffmpeg: `C:\Users\<user>\AppData\Local\Microsoft\WinGet\Links\ffmpeg.EXE`, encoders: libvpx-vp9, libx264

Sequences: 1. Checks: 0 PASS, 1 FAIL, 0 WARN, 0 SKIP.

**Result: FAIL**

(Gate: x264 CRF 23 decode, 14.7 H1 (6); CRF 30 / VP9 rows are regression rows (INFO). PASS only when nothing FAILed and every clip measured a region; an H1 (6) clip without a region FAILs, any other reads SKIP.)

## enc_beat02_sky (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\QA_enc_enc_beat02_sky\enc_beat02_sky` 1920x1080, 60 fps, 240 frames encoded

| Stream | Region | Score | Steps/1000 | Run px | Resid RMS | Resid p-p | kbit/s | MAE codes | Result |
|---|---|---|---|---|---|---|---|---|---|
| source | cloud_ceiling | 0.672 | 0.00 | 7 | - | - | - | - | PASS |
| x264 crf 23 | cloud_ceiling | 3.448 | 0.00 | 136 | - | - | 3949 | 2.108 | FAIL |
| x264 crf 30 | cloud_ceiling | 5.086 | 2.99 | 186 | - | - | 1626 | 2.897 | FAIL |
| vp9 crf 31 | cloud_ceiling | 7.571 | 0.00 | 153 | - | - | 5686 | 1.753 | FAIL |

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| x264_crf23[cloud_ceiling] | FAIL | 3.4480 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 5.17 (run 136 px) (source score 0.672), frame 0; Y 0.00 / 0.00, Cb 0.00 / 0.00, Cr 0.00 / 0.00 (steps/1000 / max code) |
| x264_crf30[cloud_ceiling] | INFO | 5.0860 | score <= 1 | visible steps/1000 2.99 (max 1.93 code), BandScore 7.63 (run 186 px) (source score 0.672), frame 0; Y 0.00 / 0.00, Cb 0.00 / 0.00, Cr 0.00 / 0.00 (steps/1000 / max code); regression row (the gate is x264 CRF 23) |
| vp9_crf31[cloud_ceiling] | INFO | 7.5710 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 11.36 (run 153 px) (source score 0.672), frame 239; Y 0.00 / 0.00, Cb 0.00 / 0.00, Cr 0.00 / 0.00 (steps/1000 / max code); regression row (the gate is x264 CRF 23) |

Review crops (source | decoded; _100, _200 nearest, _x8 local contrast):

- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\encode\QA_enc_enc_beat02_sky\enc_beat02_sky\crops\x264_crf23_cloud_ceiling_f00137_100.png`
- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\encode\QA_enc_enc_beat02_sky\enc_beat02_sky\crops\x264_crf23_cloud_ceiling_f00137_200.png`
- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\encode\QA_enc_enc_beat02_sky\enc_beat02_sky\crops\x264_crf23_cloud_ceiling_f00137_x8.png`
- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\encode\QA_enc_enc_beat02_sky\enc_beat02_sky\crops\x264_crf30_cloud_ceiling_f00137_100.png`
- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\encode\QA_enc_enc_beat02_sky\enc_beat02_sky\crops\x264_crf30_cloud_ceiling_f00137_200.png`
- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\encode\QA_enc_enc_beat02_sky\enc_beat02_sky\crops\x264_crf30_cloud_ceiling_f00137_x8.png`
- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\encode\QA_enc_enc_beat02_sky\enc_beat02_sky\crops\vp9_crf31_cloud_ceiling_f00137_100.png`
- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\encode\QA_enc_enc_beat02_sky\enc_beat02_sky\crops\vp9_crf31_cloud_ceiling_f00137_200.png`
- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\encode\QA_enc_enc_beat02_sky\enc_beat02_sky\crops\vp9_crf31_cloud_ceiling_f00137_x8.png`
