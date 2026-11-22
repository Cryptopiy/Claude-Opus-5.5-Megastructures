# Encode test: QA_enc_enc_trans_mouth

ffmpeg: `C:\Users\<user>\AppData\Local\Microsoft\WinGet\Links\ffmpeg.EXE`, encoders: libvpx-vp9, libx264

Sequences: 1. Checks: 1 PASS, 0 FAIL, 0 WARN, 0 SKIP.

**Result: PASS**

(Gate: x264 CRF 23 decode, 14.7 H1 (6); CRF 30 / VP9 rows are regression rows (INFO). PASS only when nothing FAILed and every clip measured a region; an H1 (6) clip without a region FAILs, any other reads SKIP.)

## enc_trans_mouth (PASS)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\QA_enc_enc_trans_mouth\enc_trans_mouth` 1920x1080, 60 fps, 720 frames encoded

| Stream | Region | Score | Steps/1000 | Run px | Resid RMS | Resid p-p | kbit/s | MAE codes | Result |
|---|---|---|---|---|---|---|---|---|---|
| source | mouth_dark_wall | 8.351 | 0.00 | 7 | - | - | - | - | FAIL |
| x264 crf 23 | mouth_dark_wall | 30.324 | 0.00 | 200 | - | - | 3707 | 1.920 | FAIL |
| x264 crf 30 | mouth_dark_wall | 30.718 | 0.00 | 24 | - | - | 1240 | 2.466 | FAIL |
| vp9 crf 31 | mouth_dark_wall | 58.275 | 1.85 | 48 | - | - | 3634 | 1.775 | FAIL |

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| x264_crf23[mouth_dark_wall] | PASS | 0.0000 | <= 2 steps / 1000, max <= 1.5 code | visible steps/1000 0.00 (max 0.00 code), BandScore 45.49 (run 200 px) (source score 8.351), frame 0; Y 0.00 / 0.00, Cb 0.00 / 0.00, Cr 0.00 / 0.00 (steps/1000 / max code) |
| x264_crf30[mouth_dark_wall] | INFO | 0.0000 | <= 2 steps / 1000, max <= 1.5 code | visible steps/1000 0.00 (max 0.00 code), BandScore 46.08 (run 24 px) (source score 8.351), frame 411; Y 0.00 / 0.00, Cb 0.00 / 0.00, Cr 0.00 / 0.00 (steps/1000 / max code); regression row (the gate is x264 CRF 23) |
| vp9_crf31[mouth_dark_wall] | INFO | 1.8520 | <= 2 steps / 1000, max <= 1.5 code | visible steps/1000 1.85 (max 4.32 code), BandScore 87.41 (run 48 px) (source score 8.351), frame 616; Y 0.00 / 0.00, Cb 1.85 / 4.32, Cr 0.00 / 0.00 (steps/1000 / max code); regression row (the gate is x264 CRF 23) |

Review crops (source | decoded; _100, _200 nearest, _x8 local contrast):

- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\encode\QA_enc_enc_trans_mouth\enc_trans_mouth\crops\x264_crf23_mouth_dark_wall_f00411_100.png`
- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\encode\QA_enc_enc_trans_mouth\enc_trans_mouth\crops\x264_crf23_mouth_dark_wall_f00411_200.png`
- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\encode\QA_enc_enc_trans_mouth\enc_trans_mouth\crops\x264_crf23_mouth_dark_wall_f00411_x8.png`
- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\encode\QA_enc_enc_trans_mouth\enc_trans_mouth\crops\x264_crf30_mouth_dark_wall_f00411_100.png`
- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\encode\QA_enc_enc_trans_mouth\enc_trans_mouth\crops\x264_crf30_mouth_dark_wall_f00411_200.png`
- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\encode\QA_enc_enc_trans_mouth\enc_trans_mouth\crops\x264_crf30_mouth_dark_wall_f00411_x8.png`
- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\encode\QA_enc_enc_trans_mouth\enc_trans_mouth\crops\vp9_crf31_mouth_dark_wall_f00411_100.png`
- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\encode\QA_enc_enc_trans_mouth\enc_trans_mouth\crops\vp9_crf31_mouth_dark_wall_f00411_200.png`
- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\encode\QA_enc_enc_trans_mouth\enc_trans_mouth\crops\vp9_crf31_mouth_dark_wall_f00411_x8.png`
