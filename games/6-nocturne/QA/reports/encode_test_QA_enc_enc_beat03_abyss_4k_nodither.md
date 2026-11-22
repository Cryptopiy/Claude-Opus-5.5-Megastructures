# Encode test: QA_enc_enc_beat03_abyss_4k_nodither

ffmpeg: `C:\Users\<user>\AppData\Local\Microsoft\WinGet\Links\ffmpeg.EXE`, encoders: libvpx-vp9, libx264

Sequences: 1. Checks: 0 PASS, 1 FAIL, 0 WARN, 0 SKIP.

**Result: FAIL**

(Gate: x264 CRF 23 decode, 14.7 H1 (6); CRF 30 / VP9 rows are regression rows (INFO). PASS only when nothing FAILed and every clip measured a region; an H1 (6) clip without a region FAILs, any other reads SKIP.)

## enc_beat03_abyss_4k (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\QA_enc_enc_beat03_abyss_4k_nodither\enc_beat03_abyss_4k` 3840x2160, 60 fps, 120 frames encoded

| Stream | Region | Score | Steps/1000 | Run px | Resid RMS | Resid p-p | kbit/s | MAE codes | Result |
|---|---|---|---|---|---|---|---|---|---|
| source | abyss | 2.170 | 2.56 | 71 | - | - | - | - | FAIL |
| x264 crf 23 | abyss | 2.067 | 2.56 | 45 | - | - | 10221 | 1.265 | FAIL |

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| x264_crf23[abyss] | FAIL | 2.0670 | score <= 1 | visible steps/1000 2.56 (max 1.02 code), BandScore 3.10 (run 45 px) (source score 2.170), frame 0; Y 2.56 / 1.02, Cb 0.00 / 0.00, Cr 0.00 / 0.00 (steps/1000 / max code) |

Review crops (source | decoded; _100, _200 nearest, _x8 local contrast):

- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\encode\QA_enc_enc_beat03_abyss_4k_nodither\enc_beat03_abyss_4k\crops\x264_crf23_abyss_f00068_100.png`
- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\encode\QA_enc_enc_beat03_abyss_4k_nodither\enc_beat03_abyss_4k\crops\x264_crf23_abyss_f00068_200.png`
- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\encode\QA_enc_enc_beat03_abyss_4k_nodither\enc_beat03_abyss_4k\crops\x264_crf23_abyss_f00068_x8.png`
