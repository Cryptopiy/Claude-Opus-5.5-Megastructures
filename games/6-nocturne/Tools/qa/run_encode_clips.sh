#!/usr/bin/env bash
# Encode test (Part 11.12, 14.7 H1 (6)) clip by clip for Nocturne (QA fix wave C19; the old list named another game's clips):
# capture ONE clip in the built player, encode / decode it (x264 CRF 23 = the gate; CRF 30 and VP9 CRF 31 = regression rows),
# measure the banding in Y, Cb and Cr on the declared regions (Tools/qa/regions_default.json), keep the report and the crops, delete
# the PNG frames, then the next clip. The four H1 (6) clips run at 3840 x 2160 (the gate is the 4K CRF-23 decode) for ENC4K_SECONDS
# (default 2 s = 120 frames, about 2 GB of PNG at once on this disk); NODITHER=1 adds an undithered run (-dither off) of each clip and
# a "dithered better than undithered" line. The texture clips (enc_beat01_wall -> KS00, enc_beat07_goal -> KS19) leave their last
# decoded frame for lookv2_check --encoded (QA/encode/lookv2_encoded).
#
#   Tools/qa/run_encode_clips.sh [clip ...]          (default: every clip at 1080p, the H1 four also at 4K)
#   env: BUILD=Builds\QA, RUN=<prefix> (default QA_enc), FOURK=0 (skip the 4K runs), ENC_SECONDS=10 (1080p clips),
#        ENC4K_SECONDS=2, NODITHER=1, EXTRA="..."
# Reports: QA/reports/encode_test_<RUN>_<clip>.md / .json, QA/reports/encode_summary_<RUN>.md.
cd "$(dirname "$0")/../.." || exit 2
build="${BUILD:-Builds\\QA}"
run="${RUN:-QA_enc}"
h1="enc_beat02_axis enc_beat03_abyss enc_beat06_wall enc_beat07_plaza"
all="enc_beat01_slit enc_beat02_axis enc_beat02_sky enc_beat03_abyss enc_beat04_void enc_beat05_crown enc_beat06_wall enc_beat07_plaza enc_beat07_goal enc_beat01_wall enc_trans_mouth"
clips="${*:-$all}"
summary="QA/reports/encode_summary_${run}.md"
rm -rf QA/encode/lookv2_encoded
echo "# Encode clips: $run ($(date '+%Y-%m-%d %H:%M'))" > "$summary"
echo "" >> "$summary"
echo "| Clip | Size | Result | Report |" >> "$summary"
echo "|---|---|---|---|" >> "$summary"

one() {   # one <clip> <tag> <player args> <codec args>
  local c="$1" tag="$2" pargs="$3" cargs="$4"
  local d="QA/captures/${run}_${tag}"
  rm -rf "$d"
  echo "[enc-job] $(date +%H:%M:%S) $tag: capture"
  powershell.exe -NoProfile -ExecutionPolicy Bypass -File Tools/run-player.ps1 -Name "${run}_${tag}" -BuildDir "$build" -TimeoutMin 30 \
    -PlayerArgs "-capture,$d,-capture-set,encode,-capture-only,$c$pargs${EXTRA:+,$EXTRA}" | grep -E "capture done|exit code|refused|NOC-FAIL" | head -3
  if [ ! -d "$d/$c" ]; then echo "[enc-job] $tag: nothing captured"; echo "| $tag | - | FAIL (no capture) | - |" >> "$summary"; return; fi
  python -I Tools/qa/encode_test.py "$d/$c" --run "${run}_${tag}" $cargs --sample 8 --quiet --no-lookv2 2>&1 | tail -3
  local st
  st=$(python -I -c "import json,sys;d=json.load(open(sys.argv[1]));s=d['sequences'][0];print(d['status'], 'x'.join(str(v) for v in s.get('size',[])))" "QA/reports/encode_test_${run}_${tag}.json" 2>/dev/null)
  echo "| $tag | ${st#* } | ${st%% *} | QA/reports/encode_test_${run}_${tag}.md |" >> "$summary"
  find "$d" -name 'frame_*.png' -delete
  rm -rf "QA/encode/${run}_${tag}"/*/x264_crf30 "QA/encode/${run}_${tag}"/*/vp9_crf31
}

for c in $clips; do
  one "$c" "$c" ",-encode-seconds,${ENC_SECONDS:-10}" "--codecs x264,vp9 --crf-x264 23,30 --crf-vp9 31 --vp9-cpu-used 4"
  case " $h1 " in *" $c "*)
    if [ "${FOURK:-1}" != "0" ]; then
      one "${c}_4k" "${c}_4k" ",-capture-4k,-encode-seconds,${ENC4K_SECONDS:-2}" "--codecs x264,vp9 --crf-x264 23,30 --crf-vp9 31 --vp9-cpu-used 5"
      if [ "${NODITHER:-0}" = "1" ]; then
        one "${c}_4k" "${c}_4k_nodither" ",-capture-4k,-encode-seconds,${ENC4K_SECONDS:-2},-dither,off" "--codecs x264 --crf-x264 23"
        python -I - "$run" "$c" >> "$summary" <<'PY'
import json, sys
run, c = sys.argv[1], sys.argv[2]
def worst(tag):
    try:
        d = json.load(open("QA/reports/encode_test_%s_%s.json" % (run, tag)))
    except Exception:
        return None
    v = [x for x in d["sequences"][0].get("variants", []) if x["codec"] == "x264" and x["crf"] == 23]
    sc = [s for s in (v[0].get("decoded") or {}).values() if s] if v else []
    return max((s["steps_per_1000"] for s in sc), default=None)
a, b = worst(c + "_4k"), worst(c + "_4k_nodither")
ok = a is not None and b is not None and a <= b
print("\n- %s 4K CRF 23: dithered %s vs undithered %s visible steps / 1000: %s\n" % (c, a, b, "dithered better or equal" if ok else "CHECK"))
PY
      fi
    fi;;
  esac
done
# lookv2_check --encoded on the decoded texture frames (C19)
if [ -d QA/encode/lookv2_encoded ]; then
  python -I Tools/qa/lookv2_check.py QA/encode/lookv2_encoded --encoded --json "QA/reports/lookv2_encoded_${run}.json" > "QA/reports/lookv2_encoded_${run}.txt" 2>&1
  echo "" >> "$summary"; echo "lookv2_check --encoded (exit $?): QA/reports/lookv2_encoded_${run}.json" >> "$summary"
  tail -3 "QA/reports/lookv2_encoded_${run}.txt" | sed 's/^/    /' >> "$summary"
fi
echo "[enc-job] $(date +%H:%M:%S) done -> $summary"
