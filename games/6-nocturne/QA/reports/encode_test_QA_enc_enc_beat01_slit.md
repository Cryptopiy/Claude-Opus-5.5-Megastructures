# Encode test: QA_enc_enc_beat01_slit

ffmpeg: `C:\Users\<user>\AppData\Local\Microsoft\WinGet\Links\ffmpeg.EXE`, encoders: libvpx-vp9, libx264

Sequences: 1. Checks: 0 PASS, 1 FAIL, 0 WARN, 0 SKIP.

**Result: FAIL**

(Gate: x264 CRF 23 decode, 14.7 H1 (6); CRF 30 / VP9 rows are regression rows (INFO). PASS only when nothing FAILed and every clip measured a region; an H1 (6) clip without a region FAILs, any other reads SKIP.)

## enc_beat01_slit (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\QA_enc_enc_beat01_slit\enc_beat01_slit` 1920x1080, 60 fps, 240 frames encoded

| Stream | Region | Score | Steps/1000 | Run px | Resid RMS | Resid p-p | kbit/s | MAE codes | Result |
|---|---|---|---|---|---|---|---|---|---|
| source | slit_cloud | 0.362 | 0.00 | 7 | - | - | - | - | PASS |
| x264 crf 23 | slit_cloud | 1.882 | 0.00 | 36 | - | - | 4288 | 1.767 | FAIL |
| x264 crf 30 | slit_cloud | 2.151 | 0.00 | 47 | - | - | 1416 | 2.311 | FAIL |
| vp9 crf 31 | slit_cloud | 2.203 | 4.41 | 35 | - | - | 3206 | 1.639 | FAIL |

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| x264_crf23[slit_cloud] | FAIL | 1.8820 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 2.82 (run 36 px) (source score 0.362), frame 68; Y 0.00 / 0.00, Cb 0.00 / 0.00, Cr 0.00 / 0.00 (steps/1000 / max code) |
| x264_crf30[slit_cloud] | INFO | 2.1510 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 3.23 (run 47 px) (source score 0.362), frame 102; Y 0.00 / 0.00, Cb 0.00 / 0.00, Cr 0.00 / 0.00 (steps/1000 / max code); regression row (the gate is x264 CRF 23) |
| vp9_crf31[slit_cloud] | INFO | 2.2030 | score <= 1 | visible steps/1000 4.41 (max 1.01 code), BandScore 2.53 (run 35 px) (source score 0.362), frame 34; Y 4.41 / 1.01, Cb 0.00 / 0.00, Cr 0.00 / 0.00 (steps/1000 / max code); regression row (the gate is x264 CRF 23) |

Review crops (source | decoded; _100, _200 nearest, _x8 local contrast):

- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\encode\QA_enc_enc_beat01_slit\enc_beat01_slit\crops\x264_crf23_slit_cloud_f00137_100.png`
- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\encode\QA_enc_enc_beat01_slit\enc_beat01_slit\crops\x264_crf23_slit_cloud_f00137_200.png`
- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\encode\QA_enc_enc_beat01_slit\enc_beat01_slit\crops\x264_crf23_slit_cloud_f00137_x8.png`
- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\encode\QA_enc_enc_beat01_slit\enc_beat01_slit\crops\x264_crf30_slit_cloud_f00137_100.png`
- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\encode\QA_enc_enc_beat01_slit\enc_beat01_slit\crops\x264_crf30_slit_cloud_f00137_200.png`
- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\encode\QA_enc_enc_beat01_slit\enc_beat01_slit\crops\x264_crf30_slit_cloud_f00137_x8.png`
- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\encode\QA_enc_enc_beat01_slit\enc_beat01_slit\crops\vp9_crf31_slit_cloud_f00137_100.png`
- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\encode\QA_enc_enc_beat01_slit\enc_beat01_slit\crops\vp9_crf31_slit_cloud_f00137_200.png`
- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\encode\QA_enc_enc_beat01_slit\enc_beat01_slit\crops\vp9_crf31_slit_cloud_f00137_x8.png`
