# Encode test: AT_e03_axis

ffmpeg: `C:\Users\<user>\AppData\Local\Microsoft\WinGet\Links\ffmpeg.EXE`, encoders: libvpx-vp9, libx264

Sequences: 1. Checks: 0 PASS, 1 FAIL, 0 WARN, 0 SKIP.

**Result: FAIL**

## enc_beat02_axis (FAIL)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\abyss\QA\captures\AT_e03\enc_beat02_axis` 1920x1080, 60 fps, 600 frames encoded

| Stream | Region | Score | Steps/1000 | Run px | Resid RMS | Resid p-p | kbit/s | MAE codes | Result |
|---|---|---|---|---|---|---|---|---|---|
| source | cli0 | 0.712 | 0.00 | 6 | - | - | - | - | PASS |
| x264 crf 23 | cli0 | 6.944 | 13.89 | 56 | - | - | 3806 | 1.963 | FAIL |

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| x264_crf23[cli0] | FAIL | 6.9440 | score <= 1 | visible steps/1000 13.89 (max 1.37 code), BandScore 3.32 (run 56 px) (source score 0.712), frame 257 |

Review crops (source | decoded; _100, _200 nearest, _x8 local contrast):

- `QA/encode/AT_e03_axis\enc_beat02_axis\crops\x264_crf23_cli0_f00342_100.png`
- `QA/encode/AT_e03_axis\enc_beat02_axis\crops\x264_crf23_cli0_f00342_200.png`
- `QA/encode/AT_e03_axis\enc_beat02_axis\crops\x264_crf23_cli0_f00342_x8.png`
