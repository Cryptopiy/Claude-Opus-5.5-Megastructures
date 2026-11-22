# Encode test: INTB_enc_enc_beat03_abyss_4k

ffmpeg: `C:\Users\<user>\AppData\Local\Microsoft\WinGet\Links\ffmpeg.EXE`, encoders: libvpx-vp9, libx264

Sequences: 1. Checks: 1 PASS, 0 FAIL, 0 WARN, 0 SKIP.

**Result: PASS**

(Gate: x264 CRF 23 decode, 14.7 H1 (6); CRF 30 / VP9 rows are regression rows (INFO). PASS only when nothing FAILed and every clip measured a region; an H1 (6) clip without a region FAILs, any other reads SKIP.)

## enc_beat03_abyss_4k (PASS)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\INTB_enc_enc_beat03_abyss_4k\enc_beat03_abyss_4k` 3840x2160, 60 fps, 120 frames encoded

| Stream | Region | Score | Steps/1000 | Y | Cb | Cr | R/G/B max | Run px | Resid RMS | Resid p-p | kbit/s | MAE codes | Result |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| source | abyss | 0.689 | 0.00 | 0.00 (0.00) | 0.00 (0.00) | 0.00 (0.00) | 0.00 | 7 | - | - | - | - | PASS |
| x264 crf 23 | abyss | 2.023 | 0.00 | 0.00 (0.00) | 0.00 (0.00) | 0.00 (0.00) | 2.56 | 48 | - | - | 10300 | 1.633 | FAIL |

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| x264_crf23[abyss] | PASS | 0.0000 | <= 2 steps / 1000, max <= 1.5 code | visible steps/1000 0.00 (max 0.00 code), BandScore 3.03 (run 48 px) (source score 0.689), frame 85; luma Y 0.00 / 0.00; chroma Cb 0.00 / 0.00, Cr 0.00 / 0.00 (steps/1000 / max code, gated); R 2.56, G 2.56, B 2.56 steps/1000 (reported, not gated on a decode, D-041 2) |

Review crops (source | decoded; _100, _200 nearest, _x8 local contrast):

- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\encode\INTB_enc_enc_beat03_abyss_4k\enc_beat03_abyss_4k\crops\x264_crf23_abyss_f00068_100.png`
- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\encode\INTB_enc_enc_beat03_abyss_4k\enc_beat03_abyss_4k\crops\x264_crf23_abyss_f00068_200.png`
- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\encode\INTB_enc_enc_beat03_abyss_4k\enc_beat03_abyss_4k\crops\x264_crf23_abyss_f00068_x8.png`
