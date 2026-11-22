# Encode test: QA_enc_enc_beat03_abyss

ffmpeg: `C:\Users\<user>\AppData\Local\Microsoft\WinGet\Links\ffmpeg.EXE`, encoders: libvpx-vp9, libx264

Sequences: 1. Checks: 0 PASS, 1 FAIL, 0 WARN, 0 SKIP.

**Result: FAIL**

(Gate: x264 CRF 23 decode, 14.7 H1 (6); CRF 30 / VP9 rows are regression rows (INFO). PASS only when nothing FAILed and every clip measured a region; an H1 (6) clip without a region FAILs, any other reads SKIP.)

## enc_beat03_abyss (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\QA_enc_enc_beat03_abyss\enc_beat03_abyss` 1920x1080, 60 fps, 240 frames encoded

| Stream | Region | Score | Steps/1000 | Run px | Resid RMS | Resid p-p | kbit/s | MAE codes | Result |
|---|---|---|---|---|---|---|---|---|---|
| source | abyss | 0.729 | 0.00 | 7 | - | - | - | - | PASS |
| x264 crf 23 | abyss | 2.492 | 0.00 | 49 | - | - | 3066 | 1.717 | FAIL |
| x264 crf 30 | abyss | 2.466 | 0.00 | 55 | - | - | 1361 | 2.296 | FAIL |
| vp9 crf 31 | abyss | 2.564 | 5.13 | 34 | - | - | 3460 | 1.504 | FAIL |

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| x264_crf23[abyss] | FAIL | 2.4920 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 3.74 (run 49 px) (source score 0.729), frame 171; Y 0.00 / 0.00, Cb 0.00 / 0.00, Cr 0.00 / 0.00 (steps/1000 / max code) |
| x264_crf30[abyss] | INFO | 2.4660 | score <= 1 | visible steps/1000 0.00 (max 0.00 code), BandScore 3.70 (run 55 px) (source score 0.729), frame 239; Y 0.00 / 0.00, Cb 0.00 / 0.00, Cr 0.00 / 0.00 (steps/1000 / max code); regression row (the gate is x264 CRF 23) |
| vp9_crf31[abyss] | INFO | 2.5640 | score <= 1 | visible steps/1000 5.13 (max 1.06 code), BandScore 1.78 (run 34 px) (source score 0.729), frame 0; Y 0.00 / 0.00, Cb 5.13 / 1.06, Cr 0.00 / 0.00 (steps/1000 / max code); regression row (the gate is x264 CRF 23) |

Review crops (source | decoded; _100, _200 nearest, _x8 local contrast):

- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\encode\QA_enc_enc_beat03_abyss\enc_beat03_abyss\crops\x264_crf23_abyss_f00137_100.png`
- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\encode\QA_enc_enc_beat03_abyss\enc_beat03_abyss\crops\x264_crf23_abyss_f00137_200.png`
- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\encode\QA_enc_enc_beat03_abyss\enc_beat03_abyss\crops\x264_crf23_abyss_f00137_x8.png`
- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\encode\QA_enc_enc_beat03_abyss\enc_beat03_abyss\crops\x264_crf30_abyss_f00137_100.png`
- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\encode\QA_enc_enc_beat03_abyss\enc_beat03_abyss\crops\x264_crf30_abyss_f00137_200.png`
- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\encode\QA_enc_enc_beat03_abyss\enc_beat03_abyss\crops\x264_crf30_abyss_f00137_x8.png`
- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\encode\QA_enc_enc_beat03_abyss\enc_beat03_abyss\crops\vp9_crf31_abyss_f00137_100.png`
- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\encode\QA_enc_enc_beat03_abyss\enc_beat03_abyss\crops\vp9_crf31_abyss_f00137_200.png`
- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\encode\QA_enc_enc_beat03_abyss\enc_beat03_abyss\crops\vp9_crf31_abyss_f00137_x8.png`
