# Encode test: QA_enc_enc_beat03_abyss_4k

ffmpeg: `C:\Users\<user>\AppData\Local\Microsoft\WinGet\Links\ffmpeg.EXE`, encoders: libvpx-vp9, libx264

Sequences: 1. Checks: 0 PASS, 1 FAIL, 0 WARN, 0 SKIP.

**Result: FAIL**

(Gate: x264 CRF 23 decode, 14.7 H1 (6); CRF 30 / VP9 rows are regression rows (INFO). PASS only when nothing FAILed and every clip measured a region; an H1 (6) clip without a region FAILs, any other reads SKIP.)

## enc_beat03_abyss_4k (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\QA_enc_enc_beat03_abyss_4k\enc_beat03_abyss_4k` 3840x2160, 60 fps, 120 frames encoded

| Stream | Region | Score | Steps/1000 | Run px | Resid RMS | Resid p-p | kbit/s | MAE codes | Result |
|---|---|---|---|---|---|---|---|---|---|
| source | abyss | 0.767 | 0.00 | 9 | - | - | - | - | PASS |
| x264 crf 23 | abyss | 2.007 | 2.56 | 40 | - | - | 9710 | 1.586 | FAIL |
| x264 crf 30 | abyss | 2.225 | 2.56 | 44 | - | - | 4151 | 2.031 | FAIL |
| vp9 crf 31 | abyss | 1.970 | 2.56 | 121 | - | - | 8306 | 1.443 | FAIL |

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| x264_crf23[abyss] | FAIL | 2.0070 | score <= 1 | visible steps/1000 2.56 (max 1.01 code), BandScore 3.01 (run 40 px) (source score 0.767), frame 119; Y 0.00 / 0.00, Cb 0.00 / 0.00, Cr 0.00 / 0.00 (steps/1000 / max code) |
| x264_crf30[abyss] | INFO | 2.2250 | score <= 1 | visible steps/1000 2.56 (max 2.00 code), BandScore 3.34 (run 44 px) (source score 0.767), frame 119; Y 2.56 / 1.14, Cb 0.00 / 0.00, Cr 2.56 / 1.53 (steps/1000 / max code); regression row (the gate is x264 CRF 23) |
| vp9_crf31[abyss] | INFO | 1.9700 | score <= 1 | visible steps/1000 2.56 (max 2.00 code), BandScore 2.95 (run 121 px) (source score 0.767), frame 85; Y 2.56 / 1.57, Cb 2.56 / 1.41, Cr 2.56 / 1.55 (steps/1000 / max code); regression row (the gate is x264 CRF 23) |

Review crops (source | decoded; _100, _200 nearest, _x8 local contrast):

- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\encode\QA_enc_enc_beat03_abyss_4k\enc_beat03_abyss_4k\crops\x264_crf23_abyss_f00068_100.png`
- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\encode\QA_enc_enc_beat03_abyss_4k\enc_beat03_abyss_4k\crops\x264_crf23_abyss_f00068_200.png`
- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\encode\QA_enc_enc_beat03_abyss_4k\enc_beat03_abyss_4k\crops\x264_crf23_abyss_f00068_x8.png`
- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\encode\QA_enc_enc_beat03_abyss_4k\enc_beat03_abyss_4k\crops\x264_crf30_abyss_f00068_100.png`
- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\encode\QA_enc_enc_beat03_abyss_4k\enc_beat03_abyss_4k\crops\x264_crf30_abyss_f00068_200.png`
- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\encode\QA_enc_enc_beat03_abyss_4k\enc_beat03_abyss_4k\crops\x264_crf30_abyss_f00068_x8.png`
- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\encode\QA_enc_enc_beat03_abyss_4k\enc_beat03_abyss_4k\crops\vp9_crf31_abyss_f00068_100.png`
- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\encode\QA_enc_enc_beat03_abyss_4k\enc_beat03_abyss_4k\crops\vp9_crf31_abyss_f00068_200.png`
- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\encode\QA_enc_enc_beat03_abyss_4k\enc_beat03_abyss_4k\crops\vp9_crf31_abyss_f00068_x8.png`
