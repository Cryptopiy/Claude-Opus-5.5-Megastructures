# Encode test: p4_01_enc_trans_mouth

ffmpeg: `C:\Users\<user>\AppData\Local\Microsoft\WinGet\Links\ffmpeg.EXE`, encoders: libvpx-vp9, libx264

Sequences: 1. Checks: 0 PASS, 0 FAIL, 1 WARN, 0 SKIP.

**Result: PASS**

## enc_trans_mouth (WARN)

`W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\captures\p4_01_enc\enc_trans_mouth` 1920x1080, 60 fps, 1800 frames encoded

| Check | Status | Value | Threshold | Note |
|---|---|---|---|---|
| regions | WARN | 0 | >= 1 | no gradient region declared: fidelity and crops only |

Review crops (source | decoded; _100, _200 nearest, _x8 local contrast):

- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\encode\p4_01_enc_trans_mouth\enc_trans_mouth\crops\x264_crf23_centre_f01028_100.png`
- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\encode\p4_01_enc_trans_mouth\enc_trans_mouth\crops\x264_crf23_centre_f01028_200.png`
- `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\integ\QA\encode\p4_01_enc_trans_mouth\enc_trans_mouth\crops\x264_crf23_centre_f01028_x8.png`
