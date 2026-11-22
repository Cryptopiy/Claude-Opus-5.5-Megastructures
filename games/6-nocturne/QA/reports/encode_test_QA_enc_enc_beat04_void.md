# Encode test: QA_enc_enc_beat04_void

ffmpeg: `C:\Users\<user>\AppData\Local\Microsoft\WinGet\Links\ffmpeg.EXE`, encoders: libvpx-vp9, libx264

Sequences: 1. Checks: 0 PASS, 1 FAIL, 0 WARN, 0 SKIP.

**Result: FAIL**

(Gate: x264 CRF 23 decode, 14.7 H1 (6); CRF 30 / VP9 rows are regression rows (INFO). PASS only when nothing FAILed and every clip measured a region; an H1 (6) clip without a region FAILs, any other reads SKIP.)

## enc_beat04_void (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\QA_enc_enc_beat04_void\enc_beat04_void` 1920x1080, 60 fps, 240 frames encoded

| Stream | Region | Score | Steps/1000 | Run px | Resid RMS | Resid p-p | kbit/s | MAE codes | Result |
|---|---|---|---|---|---|---|---|---|---|
| source | abyss | 2.238 | 0.00 | 7 | - | - | - | - | FAIL |
| x264 crf 23 | abyss | 11.508 | 0.00 | 33 | - | - | 2885 | 1.605 | FAIL |
| x264 crf 30 | abyss | 13.044 | 0.00 | 33 | - | - | 1220 | 2.114 | FAIL |
| vp9 crf 31 | abyss | 13.640 | 6.13 | 90 | - | - | 3295 | 1.377 | FAIL |

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| x264_crf23[abyss] | FAIL | 11.5080 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 17.26 (run 33 px) (source score 2.238), frame 0; Y 0.00 / 0.00, Cb 0.00 / 0.00, Cr 0.00 / 0.00 (steps/1000 / max code) |
| x264_crf30[abyss] | INFO | 13.0440 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 19.57 (run 33 px) (source score 2.238), frame 0; Y 0.00 / 0.00, Cb 0.00 / 0.00, Cr 0.00 / 0.00 (steps/1000 / max code); regression row (the gate is x264 CRF 23) |
| vp9_crf31[abyss] | INFO | 13.6400 | score <= 1 | visible steps/1000 6.13 (max 1.22 code), BandScore 20.46 (run 90 px) (source score 2.238), frame 137; Y 0.00 / 0.00, Cb 0.00 / 0.00, Cr 6.13 / 1.22 (steps/1000 / max code); regression row (the gate is x264 CRF 23) |

Review crops (source | decoded; _100, _200 nearest, _x8 local contrast):

- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\encode\QA_enc_enc_beat04_void\enc_beat04_void\crops\x264_crf23_abyss_f00137_100.png`
- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\encode\QA_enc_enc_beat04_void\enc_beat04_void\crops\x264_crf23_abyss_f00137_200.png`
- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\encode\QA_enc_enc_beat04_void\enc_beat04_void\crops\x264_crf23_abyss_f00137_x8.png`
- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\encode\QA_enc_enc_beat04_void\enc_beat04_void\crops\x264_crf30_abyss_f00137_100.png`
- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\encode\QA_enc_enc_beat04_void\enc_beat04_void\crops\x264_crf30_abyss_f00137_200.png`
- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\encode\QA_enc_enc_beat04_void\enc_beat04_void\crops\x264_crf30_abyss_f00137_x8.png`
- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\encode\QA_enc_enc_beat04_void\enc_beat04_void\crops\vp9_crf31_abyss_f00137_100.png`
- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\encode\QA_enc_enc_beat04_void\enc_beat04_void\crops\vp9_crf31_abyss_f00137_200.png`
- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\encode\QA_enc_enc_beat04_void\enc_beat04_void\crops\vp9_crf31_abyss_f00137_x8.png`
