#!/usr/bin/env bash
# Sequence gate clip by clip (QA fix wave C21 / lead ruling 2026-11-21: never capture a whole "sequences" set at once).
# For each sequence name: capture it alone in the built player (-capture-only), analyse it with Tools/qa/sequences.py, write its
# montage / pop crops / x264 clip, delete its PNG frames and leave an analysed.done marker; the next one starts only then, so at
# most one sequence's frames sit on the disk. run-player refuses to start under 5 GB free (exit 94) and kills a player under 1.5 GB.
#
#   Tools/qa/run_seq_clips.sh <set> <run> <seq_name> [<seq_name> ...]
#   env: BUILD=Builds\QA (player folder), EXTRA="-rain,0,-freeze-clock" (more player args, comma separated),
#        REF=QA/captures/<ss2 run> (texpan / pan reference folder), TIMEOUT=20 (minutes per capture)
# Example: BUILD='Builds\QA' Tools/qa/run_seq_clips.sh boundaries QA_bnd seq_b23 seq_b12
# Reports: QA/reports/sequences_<run>_<seq>.md / .json; montages inside QA/captures/<run>/<seq>/.
cd "$(dirname "$0")/../.." || exit 2
set_name="$1"; run="$2"; shift 2
build="${BUILD:-Builds\\QA}"
rc_all=0
for s in "$@"; do
  d="QA/captures/$run"
  rm -rf "$d/$s"
  echo "[seq-job] $(date +%H:%M:%S) capture $s ($set_name)"
  powershell.exe -NoProfile -ExecutionPolicy Bypass -File Tools/run-player.ps1 -Name "${run}_$s" -BuildDir "$build" -TimeoutMin "${TIMEOUT:-20}" \
    -PlayerArgs "-capture,$d,-capture-set,$set_name,-capture-only,$s${EXTRA:+,$EXTRA}" | grep -E "capture done|exit code|refused|NOC-FAIL" | head -4
  if [ ! -f "$d/$s/sequence.json" ]; then echo "[seq-job] $s: no sequence.json (capture failed)"; rc_all=1; continue; fi
  ref=""; [ -n "$REF" ] && ref="--reference $REF"
  python -I Tools/qa/sequences.py "$d/$s" --run "${run}_$s" --quiet --delete-frames $ref 2>&1 | tail -6
  rc=${PIPESTATUS[0]}; [ "$rc" -ne 0 ] && rc_all=1
  [ -f "$d/$s/analysed.done" ] && echo "[seq-job] $s: analysed, frames deleted ($(du -sh "$d/$s" | cut -f1) kept)"
done
echo "[seq-job] $(date +%H:%M:%S) done, exit $rc_all"
exit $rc_all
