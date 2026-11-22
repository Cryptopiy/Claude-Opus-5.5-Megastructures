#!/usr/bin/env python3
"""encode_verdict: one table of the 14.7 H1 (6) encode gate over the clip-by-clip reports of run_encode_clips.sh (QA fix wave C19).

Usage: python -I Tools/qa/encode_verdict.py <run prefix, e.g. QA_enc> [--out-dir QA/reports]

Reads QA/reports/encode_test_<run>_*.json and writes QA/reports/encode_verdict_<run>.md: per clip and region the x264 CRF 23 decode
(the gate: visible steps <= 2 / 1000 and none > 1.5 codes, in Y, Cb, Cr and R, G, B), the CRF 30 / VP9 regression rows, the BandScore
(reported, a source metric) and the verdict recomputed from the stored numbers (so reports written before the steps-only decode gate
read the same way). A clip with no measured region is FAIL for the four H1 (6) clips and SKIP otherwise.
"""
import argparse
import glob
import json
import os
import sys

_HERE = os.path.dirname(os.path.abspath(__file__))
if _HERE not in sys.path:
    sys.path.insert(0, _HERE)

import qa_common as qc  # noqa: E402
import encode_test  # noqa: E402


def main(argv=None):
    ap = argparse.ArgumentParser()
    ap.add_argument("run")
    ap.add_argument("--out-dir", default=qc.REPORTS_DIR)
    a = ap.parse_args(argv)
    smax, cmax = qc.TH["band_steps_per_1000_encoded_max"], qc.TH["band_step_max_code"]
    rows, verdicts = [], []
    for f in sorted(glob.glob(os.path.join(a.out_dir, "encode_test_%s_*.json" % a.run))):
        tag = os.path.basename(f)[len("encode_test_%s_" % a.run):-5]
        d = json.load(open(f, encoding="utf-8"))
        for s in d.get("sequences", []):
            size = "x".join(str(v) for v in s.get("size", []))
            measured = False
            clip_ok = True
            for v in s.get("variants", []):
                gate = v["codec"] == encode_test.GATE_CODEC and v["crf"] == encode_test.GATE_CRF
                for name, sc in (v.get("decoded") or {}).items():
                    if not sc:
                        continue
                    ok = sc["steps_per_1000"] <= smax and (sc.get("steps", 0) == 0 or sc["step_max_code"] <= cmax)
                    ch = sc.get("chan") or {}
                    chs = ", ".join("%s %.2f/%.2f" % (k, ch[k][0], ch[k][1]) for k in ("Y", "Cb", "Cr") if k in ch) or "-"
                    if gate:
                        measured = True
                        clip_ok &= ok
                    rows.append([tag, size, "%s %d" % (v["codec"], v["crf"]) + ("" if gate else " (regr.)"), name,
                                 qc.fmt(sc["steps_per_1000"], 2), qc.fmt(sc["step_max_code"], 2), chs, qc.fmt(sc.get("bandscore"), 2),
                                 ("PASS" if ok else "FAIL") if gate else "INFO"])
            st = ("PASS" if clip_ok else "FAIL") if measured else ("FAIL" if encode_test.is_h1(s["name"]) else "SKIP")
            verdicts.append([tag, size, s.get("frames_encoded"), st])
    overall = "FAIL" if any(v[3] == "FAIL" for v in verdicts) else ("SKIP" if any(v[3] == "SKIP" for v in verdicts) or not verdicts else "PASS")
    L = ["# Encode verdict: %s" % a.run, "", "Gate (14.7 H1 (6)): x264 CRF 23 decode, visible steps <= %g / 1000 and none > %g codes "
         "(worst of Y, Cb, Cr, R, G, B); CRF 30 / VP9 = regression rows; BandScore reported only. The H1 (6) clips are judged at "
         "3840 x 2160 (_4k)." % (smax, cmax), "", "**Result: %s**" % overall, "",
         qc.md_table(["Clip", "Size", "Frames", "Verdict"], verdicts), "",
         qc.md_table(["Clip", "Size", "Stream", "Region", "Steps/1000", "Max code", "Y / Cb / Cr (steps/1000 / max)", "BandScore", "Result"], rows)]
    md = qc.write_text(os.path.join(a.out_dir, "encode_verdict_%s.md" % a.run), "\n".join(L))
    print("[encode_verdict] %s -> %s" % (overall, md))
    return 1 if overall == "FAIL" else 0


if __name__ == "__main__":
    sys.exit(main())
