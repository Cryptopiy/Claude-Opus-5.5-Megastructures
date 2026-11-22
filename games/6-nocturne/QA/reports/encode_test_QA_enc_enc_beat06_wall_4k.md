# Encode test: QA_enc_enc_beat06_wall_4k

ffmpeg: `C:\Users\<user>\AppData\Local\Microsoft\WinGet\Links\ffmpeg.EXE`, encoders: libvpx-vp9, libx264

Sequences: 1. Checks: 1 PASS, 0 FAIL, 0 WARN, 0 SKIP.

**Result: PASS**

(Gate: x264 CRF 23 decode, 14.7 H1 (6); CRF 30 / VP9 rows are regression rows (INFO). PASS only when nothing FAILed and every clip measured a region; an H1 (6) clip without a region FAILs, any other reads SKIP.)

## enc_beat06_wall_4k (PASS)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\QA_enc_enc_beat06_wall_4k\enc_beat06_wall_4k` 3840x2160, 60 fps, 120 frames encoded

| Stream | Region | Score | Steps/1000 | Run px | Resid RMS | Resid p-p | kbit/s | MAE codes | Result |
|---|---|---|---|---|---|---|---|---|---|
| source | wall_panel | 1.569 | 0.00 | 6 | - | - | - | - | FAIL |
| x264 crf 23 | wall_panel | 4.833 | 0.00 | 25 | - | - | 11609 | 1.908 | FAIL |
| x264 crf 30 | wall_panel | 6.499 | 0.00 | 31 | - | - | 5004 | 2.628 | FAIL |
| vp9 crf 31 | wall_panel | 5.555 | 0.00 | 28 | - | - | 12200 | 1.708 | FAIL |

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| x264_crf23[wall_panel] | PASS | 0.0000 | <= 2 steps / 1000, max <= 1.5 code | visible steps/1000 0.00 (max 0.00 code), BandScore 7.25 (run 25 px) (source score 1.569), frame 17; Y 0.00 / 0.00, Cb 0.00 / 0.00, Cr 0.00 / 0.00 (steps/1000 / max code) |
| x264_crf30[wall_panel] | INFO | 0.0000 | <= 2 steps / 1000, max <= 1.5 code | visible steps/1000 0.00 (max 0.00 code), BandScore 9.75 (run 31 px) (source score 1.569), frame 34; Y 0.00 / 0.00, Cb 0.00 / 0.00, Cr 0.00 / 0.00 (steps/1000 / max code); regression row (the gate is x264 CRF 23) |
| vp9_crf31[wall_panel] | INFO | 0.0000 | <= 2 steps / 1000, max <= 1.5 code | visible steps/1000 0.00 (max 0.00 code), BandScore 8.33 (run 28 px) (source score 1.569), frame 17; Y 0.00 / 0.00, Cb 0.00 / 0.00, Cr 0.00 / 0.00 (steps/1000 / max code); regression row (the gate is x264 CRF 23) |

Review crops (source | decoded; _100, _200 nearest, _x8 local contrast):

- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\encode\QA_enc_enc_beat06_wall_4k\enc_beat06_wall_4k\crops\x264_crf23_wall_panel_f00068_100.png`
- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\encode\QA_enc_enc_beat06_wall_4k\enc_beat06_wall_4k\crops\x264_crf23_wall_panel_f00068_200.png`
- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\encode\QA_enc_enc_beat06_wall_4k\enc_beat06_wall_4k\crops\x264_crf23_wall_panel_f00068_x8.png`
- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\encode\QA_enc_enc_beat06_wall_4k\enc_beat06_wall_4k\crops\x264_crf30_wall_panel_f00068_100.png`
- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\encode\QA_enc_enc_beat06_wall_4k\enc_beat06_wall_4k\crops\x264_crf30_wall_panel_f00068_200.png`
- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\encode\QA_enc_enc_beat06_wall_4k\enc_beat06_wall_4k\crops\x264_crf30_wall_panel_f00068_x8.png`
- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\encode\QA_enc_enc_beat06_wall_4k\enc_beat06_wall_4k\crops\vp9_crf31_wall_panel_f00068_100.png`
- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\encode\QA_enc_enc_beat06_wall_4k\enc_beat06_wall_4k\crops\vp9_crf31_wall_panel_f00068_200.png`
- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\encode\QA_enc_enc_beat06_wall_4k\enc_beat06_wall_4k\crops\vp9_crf31_wall_panel_f00068_x8.png`
