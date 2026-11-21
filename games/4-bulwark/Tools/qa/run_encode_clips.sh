#!/usr/bin/env bash
# Encode test (MASTER_PROMPT 11.12, 14.10.3 (2)) clip by clip, Bulwark edition (rewritten from Thread):
#   1. capture the clip twice in the built player through run-player.ps1 -Long (gpu-guard: the encode set is a long
#      GPU job, 2.5.1): once with the default static dither, once with -dither off (the undithered twin);
#      -capture-4k adds the 3840 x 2160 variant (the 14.10.3 gate is on the 4K x264 CRF 23 decode);
#   2. encode / decode with ffmpeg (x264 CRF 23 and 30, VP9 CRF 31 and 40) and measure the BandScore on the clean
#      gradient region of the clip; dithered longest run <= 0.6 x the undithered one;
#   3. keep the reports and the worst-location crops, delete every PNG frame (disk budget, lesson 3.2 #15).
# Clips (ShotPlan.EncodeClips): storm sky (beat 2), mist river (beat 3), deep water veil (beat 6), dusk sky (beat 7).
# The regions below are FIRST-PASS rectangles at 1920 x 1080 (x2 for the 4K variant): recalibrate them on the first
# capture so that they cover only a clean gradient (no rail, lamp, horizon line or sun disc).
# Usage: bash Tools/qa/run_encode_clips.sh [clip ...]      env: EXTRA="-tier-override 1" adds player arguments
cd "$(dirname "$0")/../.." || exit 1
clips="${*:-enc_b02_storm_sky enc_b03_mist enc_b06_water_veil enc_b07_dusk_sky}"
status=0
for c in $clips; do
  d="QA/captures/$c"
  du="QA/captures/${c}_nodither"
  rm -rf "$d" "$du"
  powershell.exe -NoProfile -ExecutionPolicy Bypass -File Tools/run-player.ps1 -Name "$c" -Long -TimeoutMin 30 \
    -ArgString "-capture $d -capture-set encode -capture-only $c -capture-4k ${EXTRA:-}" | grep -E "exit code|gpu-guard|refused" | tail -2
  powershell.exe -NoProfile -ExecutionPolicy Bypass -File Tools/run-player.ps1 -Name "${c}_nodither" -Long -TimeoutMin 30 \
    -ArgString "-capture $du -capture-set encode -capture-only $c -capture-4k -dither off ${EXTRA:-}" | grep -E "exit code|gpu-guard|refused" | tail -2
  case "$c" in
    enc_b02_storm_sky)  r="200,40,1720,420" ;;
    enc_b03_mist)       r="200,520,1720,1000" ;;
    enc_b06_water_veil) r="300,150,1620,930" ;;
    enc_b07_dusk_sky)   r="200,40,1720,450" ;;
    *)                  r="200,40,1720,1000" ;;
  esac
  for res in "" "_4k"; do
    seq="seq_${c}${res}"
    if [ ! -d "$d/$seq" ]; then echo "[encode] $d/$seq missing"; status=1; continue; fi
    reg="$r"
    if [ "$res" = "_4k" ]; then reg=$(echo "$r" | awk -F, '{printf "%d,%d,%d,%d", $1*2, $2*2, $3*2, $4*2}'); fi
    python -I Tools/qa/encode_test.py "$d/$seq" --undithered "$du/$seq" --region "$reg,v" --codecs x264,vp9 \
      --crf-x264 23,30 --crf-vp9 31,40 --sample 8 --run "${c}${res}${TAG:-}" 2>&1 | grep -E "^\[encode|FAIL|PASS" | tail -6
    [ "${PIPESTATUS[0]}" = "0" ] || status=1
  done
  find "$d" "$du" -name 'frame_*.png' -delete 2>/dev/null
  find "QA/encode" -path "*${c}*" -name 'frame_*.png' -delete 2>/dev/null
done
exit $status
