# Encode test: lookv2_01_enc_beat06_wall

ffmpeg: `C:\Users\Cryptopiy\AppData\Local\Microsoft\WinGet\Links\ffmpeg.EXE`, encoders: libvpx-vp9, libx264

Sequences: 1. Checks: 0 PASS, 0 FAIL, 1 WARN, 0 SKIP.

**Result: PASS**

## enc_beat06_wall (WARN)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\captures\lookv2_01enc\enc_beat06_wall` 1920x1080, 60 fps, 600 frames encoded

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| regions | WARN | 0 | >= 1 | no gradient region declared: fidelity and crops only |

Review crops (source | decoded; _100, _200 nearest, _x8 local contrast):

- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\encode\lookv2_01_enc_beat06_wall\enc_beat06_wall\crops\x264_crf23_centre_f00342_100.png`
- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\encode\lookv2_01_enc_beat06_wall\enc_beat06_wall\crops\x264_crf23_centre_f00342_200.png`
- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\QA\encode\lookv2_01_enc_beat06_wall\enc_beat06_wall\crops\x264_crf23_centre_f00342_x8.png`
