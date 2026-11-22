# Encode test: p4_01_enc_beat03_abyss

ffmpeg: `C:\Users\<user>\AppData\Local\Microsoft\WinGet\Links\ffmpeg.EXE`, encoders: libvpx-vp9, libx264

Sequences: 1. Checks: 0 PASS, 0 FAIL, 1 WARN, 0 SKIP.

**Result: PASS**

## enc_beat03_abyss (WARN)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01_enc\enc_beat03_abyss` 1920x1080, 60 fps, 600 frames encoded

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| regions | WARN | 0 | >= 1 | no gradient region declared: fidelity and crops only |

Review crops (source | decoded; _100, _200 nearest, _x8 local contrast):

- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\encode\p4_01_enc_beat03_abyss\enc_beat03_abyss\crops\x264_crf23_centre_f00342_100.png`
- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\encode\p4_01_enc_beat03_abyss\enc_beat03_abyss\crops\x264_crf23_centre_f00342_200.png`
- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\encode\p4_01_enc_beat03_abyss\enc_beat03_abyss\crops\x264_crf23_centre_f00342_x8.png`
