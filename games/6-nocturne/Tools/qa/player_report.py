#!/usr/bin/env python3
"""player_report: the autotest / perf report of one player run, read from its log (Logs/player-<name>.log).

AU's request (fix wave C52, carried by QA): the player logs '[NOC] audio heartbeat t N s, s S: ... overruns M (audio overruns
after the intro = M)' every 30 s after a 10 s intro baseline and '[NOC] audio summary ...' at quit (AudioDirector / NocSynth).
This report carries "audio_overruns_after_intro" (the last such line) and "audio_callback_max_ms" (the largest 30 s window
maximum) and gates "audio overruns after the intro = 0"; a run without the lines (no audio device, -nosound) has None and
FAILs the gate, so a missing measurement never reads PASS. Beside it: the AUTOTEST result lines, the [NOC-FAIL] lines and the
exception count of the log.

Usage:
  python -I Tools/qa/player_report.py Logs/player-<name>.log [--run NAME] [--out-dir QA/reports]
Writes QA/reports/player_<run>.json and .md; exit code 1 when a gate fails.
"""
import argparse
import json
import os
import re
import sys

_HERE = os.path.dirname(os.path.abspath(__file__))
if _HERE not in sys.path:
    sys.path.insert(0, _HERE)

import qa_common as qc  # noqa: E402


def parse(log_text):
    report = {}
    gates = []
    # AU's lines (exactly as requested)
    m = re.findall(r"audio overruns after the intro = (\d+)", log_text)
    win = [float(x) for x in re.findall(r"window max ([0-9.]+) ms", log_text)]
    report["audio_overruns_after_intro"] = int(m[-1]) if m else None
    report["audio_callback_max_ms"] = max(win) if win else None
    report["audio_heartbeat_lines"] = len(m)
    gates.append(("audio overruns after the intro = 0", report["audio_overruns_after_intro"] == 0))
    # context: the autotest verdict lines, hard failures and exceptions
    report["autotest_lines"] = [ln.strip() for ln in log_text.splitlines() if "[NOC] AUTOTEST" in ln][-12:]
    report["noc_fail_lines"] = [ln.strip() for ln in log_text.splitlines() if "[NOC-FAIL]" in ln][:20]
    report["exceptions"] = len(re.findall(r"^\w*Exception\b|Exception: ", log_text, flags=re.M))
    return report, gates


def run(log_path, run_name=None, out_dir=qc.REPORTS_DIR):
    with open(log_path, encoding="utf-8", errors="replace") as f:
        text = f.read()
    report, gates = parse(text)
    name = run_name or os.path.splitext(os.path.basename(log_path))[0].replace("player-", "")
    report.update({"tool": "player_report", "run": name, "log": log_path,
                   "gates": [{"name": g, "pass": bool(ok)} for g, ok in gates]})
    report["status"] = "PASS" if all(ok for _, ok in gates) else "FAIL"
    os.makedirs(out_dir, exist_ok=True)
    jp = os.path.join(out_dir, "player_%s.json" % name)
    with open(jp, "w", encoding="utf-8") as f:
        json.dump(report, f, indent=1)
    md = ["# player report %s (%s)" % (name, report["status"]), "", "log `%s`" % log_path, "",
          "| Gate | Result | Value |", "|---|---|---|"]
    md.append("| audio overruns after the intro = 0 | %s | %s (callback window max %s ms, %d lines) |" % (
        "PASS" if gates[0][1] else "FAIL", report["audio_overruns_after_intro"], report["audio_callback_max_ms"],
        report["audio_heartbeat_lines"]))
    if report["autotest_lines"]:
        md += ["", "AUTOTEST lines:", ""] + ["- `%s`" % ln for ln in report["autotest_lines"]]
    if report["noc_fail_lines"]:
        md += ["", "[NOC-FAIL] lines:", ""] + ["- `%s`" % ln for ln in report["noc_fail_lines"]]
    with open(os.path.join(out_dir, "player_%s.md" % name), "w", encoding="utf-8") as f:
        f.write("\n".join(md) + "\n")
    return report


def main(argv=None):
    ap = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    ap.add_argument("log")
    ap.add_argument("--run")
    ap.add_argument("--out-dir", default=qc.REPORTS_DIR)
    a = ap.parse_args(argv)
    r = run(a.log, a.run, a.out_dir)
    for g in r["gates"]:
        print("%s: %s" % ("PASS" if g["pass"] else "FAIL", g["name"]))
    print("audio_overruns_after_intro %s, audio_callback_max_ms %s -> %s" % (
        r["audio_overruns_after_intro"], r["audio_callback_max_ms"], r["status"]))
    return 0 if r["status"] == "PASS" else 1


if __name__ == "__main__":
    sys.exit(main())
