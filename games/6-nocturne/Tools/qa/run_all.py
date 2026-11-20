#!/usr/bin/env python3
"""run_all: one entry point for the Nocturne offline QA tools on a capture folder (MASTER_PROMPT 11.3-11.12).

Usage:
  python -I Tools/qa/run_all.py QA/captures/<run-id> [--previous QA/captures/<old-run>] [--audio <wav|folder> ...]
         [--encode auto|all|none] [--regions Tools/qa/regions_default.json] [--run <id>] [--out-dir QA/reports]
         [--jobs 4] [--require-keyshots] [--no-contact] [--encode-max-frames N] [--codecs x264,vp9]

Steps (each one is skipped cleanly when its input is missing):
  1. image_metrics  on <dir>/*.png + report.json                 -> image_metrics_<run>.md/.json
  2. contact_sheet  of every still (failing images framed red)    -> <dir>/_contact/contact_<run>_NN.png
  3. sequences      on <dir>/seq_*/                               -> sequences_<run>.md/.json
  4. encode_test    on sequences marked "encode" (auto), all, none -> encode_test_<run>.md/.json
  5. audio_analysis on --audio paths, else <dir>/*.wav and <dir>/audio/*.wav -> audio_<run>.md/.json
  6. tiling         texture repetition and detail of the LOOK_V2 / texclose regions (TQ)   -> tiling_<run>.md/.json
  7. debugview      the -debug-view N captures (report.json args), TEXTURE_PLAN 7.1 (TQ) -> debugview_<run>.md/.json
  8. lookv2_check   LOOK_V2 8.2 acceptance checks on the key shots (PA's script, run as a process) -> lookv2_<run>.json
  9. facade_sheet   the facade lookdev sheet stills (FA's script, run as a process)           -> facade_sheet_<run>.md/.json
  Steps 8 and 9 are skipped when their script is missing; texpan sequences get the R1 shimmer gate with --reference.
Writes QA/reports/summary_<run>.md (per-step status, counts, report paths, every FAIL line).
Exit code 1 if any step FAILs or crashes, 0 otherwise.
"""
import argparse
import glob
import os
import subprocess
import sys
import time
import traceback

_HERE = os.path.dirname(os.path.abspath(__file__))
if _HERE not in sys.path:
    sys.path.insert(0, _HERE)

import qa_common as qc  # noqa: E402
from qa_common import PASS, FAIL, SKIP  # noqa: E402

import image_metrics  # noqa: E402
import contact_sheet  # noqa: E402
import sequences  # noqa: E402
import encode_test  # noqa: E402
import audio_analysis  # noqa: E402
import tiling  # noqa: E402
import debugview  # noqa: E402


def _script_step(name, script, args, out_json=None, timeout=1800):
    """Run another owner's QA script as a process (python -I), skipped when it is missing; PASS on exit code 0."""
    path = os.path.join(_HERE, script)
    if not os.path.isfile(path):
        return {"tool": name, "status": SKIP, "fails": [], "note": "%s not found" % script}
    p = subprocess.run([sys.executable, "-I", path] + args, capture_output=True, text=True, timeout=timeout)
    out = (p.stdout or "") + (p.stderr or "")
    fails = [ln.strip() for ln in out.splitlines() if "FAIL" in ln][:200]
    r = {"tool": name, "status": PASS if p.returncode == 0 else FAIL, "fails": fails if p.returncode else [],
         "note": out.strip().splitlines()[-1][:200] if out.strip() else ""}
    if out_json and os.path.isfile(out_json):
        r["json"] = out_json
        try:
            res = qc.read_json(out_json, [])
            if isinstance(res, list):
                r["n"] = len(res)
                r["counts"] = {PASS: sum(1 for x in res if x.get("pass")), FAIL: sum(1 for x in res if not x.get("pass"))}
        except Exception:  # pragma: no cover
            pass
    return r


def _step(name, fn):
    t0 = time.time()
    try:
        r = fn()
    except Exception as e:
        r = {"tool": name, "status": FAIL, "fails": ["%s crashed: %s: %s" % (name, type(e).__name__, e)],
             "trace": traceback.format_exc()}
        print(r["trace"], file=sys.stderr)
    r["seconds"] = round(time.time() - t0, 1)
    print("[run_all] %-15s %s (%.1f s)" % (name, r["status"], r["seconds"]), flush=True)
    return r


def main(argv=None):
    ap = argparse.ArgumentParser(prog="run_all.py", description=__doc__.split("\n\n")[0],
                                 formatter_class=argparse.RawDescriptionHelpFormatter, epilog=__doc__)
    ap.add_argument("capture_dir")
    ap.add_argument("--previous", help="previous capture folder (regression diff)")
    ap.add_argument("--audio", nargs="*", help="WAV files / folders (default <dir>/*.wav, <dir>/audio/*.wav)")
    ap.add_argument("--encode", choices=("auto", "all", "none"), default="auto",
                    help="auto: sequences with encode=true / kind=encode (default)")
    ap.add_argument("--encode-max-frames", type=int, help="limit the encoded frames per sequence")
    ap.add_argument("--codecs", default="x264,vp9")
    ap.add_argument("--regions", default=qc.DEFAULT_REGIONS)
    ap.add_argument("--run", help="run id (default: capture folder name)")
    ap.add_argument("--out-dir", default=qc.REPORTS_DIR)
    ap.add_argument("--jobs", type=int, default=1)
    ap.add_argument("--require-keyshots", action="store_true")
    ap.add_argument("--no-contact", action="store_true")
    ap.add_argument("--reference", nargs="*", help="-capture-ss 2 run of the same texpans set (R1 shimmer gate)")
    ap.add_argument("--quiet", action="store_true")
    a = ap.parse_args(argv)
    cap = os.path.abspath(a.capture_dir)
    if not os.path.isdir(cap):
        print("capture folder not found: %s" % cap, file=sys.stderr)
        return 1
    run_name = qc.run_id_for(cap, a.run)
    steps = []
    has_png = any(n.lower().endswith(".png") for n in os.listdir(cap))
    seqs = qc.find_sequences(cap)

    if has_png:
        im = _step("image_metrics", lambda: image_metrics.run(cap, a.previous, a.regions, run_name, a.out_dir, a.jobs,
                                                              a.require_keyshots, None, "display", a.quiet))
        steps.append(im)
        if not a.no_contact:
            steps.append(_step("contact_sheet", lambda: contact_sheet.run(cap, None, 4, 2048, 2048, im.get("json"),
                                                                          False, 15, run_name)))
    else:
        steps.append({"tool": "image_metrics", "status": SKIP, "fails": [], "note": "no PNG in the folder"})
    if seqs:
        steps.append(_step("sequences", lambda: sequences.run([cap], run_name, a.out_dir, a.quiet, a.reference)))
        if a.encode != "none":
            ea = ["--run", run_name, "--out-dir", a.out_dir, "--regions", a.regions, "--codecs", a.codecs]
            if a.encode == "all":
                ea.append("--all-seqs")
            if a.encode_max_frames:
                ea += ["--max-frames", str(a.encode_max_frames)]
            if a.quiet:
                ea.append("--quiet")
            eargs = encode_test.parser().parse_args([cap] + ea)
            steps.append(_step("encode_test", lambda: encode_test.run([cap], eargs)))
    else:
        steps.append({"tool": "sequences", "status": SKIP, "fails": [], "note": "no seq_* folder"})
    apaths = a.audio if a.audio else [p for p in (cap, os.path.join(cap, "audio")) if os.path.isdir(p)]
    if audio_analysis.collect(apaths):
        steps.append(_step("audio_analysis", lambda: audio_analysis.run(apaths, run_name, a.out_dir, 1.0, 10.0,
                                                                        a.quiet)))
    else:
        steps.append({"tool": "audio_analysis", "status": SKIP, "fails": [], "note": "no WAV given or found"})

    # Look v2 texture tools (TQ) and the package scripts of PA and FA (skipped when missing)
    if has_png:
        steps.append(_step("tiling", lambda: tiling.run([cap], run_name, a.out_dir, None, None, None, a.quiet)))
        dv = _step("debugview", lambda: debugview.run([cap], None, run_name, a.out_dir, False, a.quiet))
        steps.append(dv if dv.get("n") else {"tool": "debugview", "status": SKIP, "fails": [], "note": dv.get("note", "no debug view")})
    else:
        steps.append({"tool": "tiling", "status": SKIP, "fails": [], "note": "no PNG in the folder"})
    if glob.glob(os.path.join(cap, "KS*.png")):
        lj = os.path.join(a.out_dir, "lookv2_%s.json" % run_name)
        steps.append(_step("lookv2_check", lambda: _script_step("lookv2_check", "lookv2_check.py", [cap, "--json", lj], lj)))
    else:
        steps.append({"tool": "lookv2_check", "status": SKIP, "fails": [], "note": "no KS*.png"})
    if glob.glob(os.path.join(cap, "**", "pose_fs*.png"), recursive=True) or glob.glob(os.path.join(cap, "**", "fs*.png"), recursive=True):
        steps.append(_step("facade_sheet", lambda: _script_step("facade_sheet", "facade_sheet.py", [cap, "--run", run_name, "--out-dir", a.out_dir])))
    else:
        steps.append({"tool": "facade_sheet", "status": SKIP, "fails": [], "note": "no facade-sheet still (pose_fs*)"})

    overall = FAIL if any(s["status"] == FAIL for s in steps) else (
        PASS if any(s["status"] == PASS for s in steps) else SKIP)
    L = ["# QA summary: %s" % run_name, "", "Capture folder: `%s`" % cap, "",
         "**Overall: %s**" % overall, ""]
    rows = []
    for s in steps:
        c = s.get("counts") or {}
        rep = s.get("md") or (", ".join(s.get("sheets", [])[:3]) + (" ..." if len(s.get("sheets", [])) > 3 else "")) \
            or s.get("note", "")
        rows.append([s["tool"], s["status"], s.get("n", "-"), c.get(PASS, "-"), c.get(FAIL, len(s.get("fails", [])) or "-"),
                     qc.fmt(s.get("seconds"), 1), rep])
    L.append(qc.md_table(["Step", "Status", "Items", "PASS", "FAIL", "Seconds", "Report"], rows))
    fails = [(s["tool"], f) for s in steps for f in s.get("fails", [])]
    if fails:
        L += ["## FAIL lines (%d)" % len(fails), ""]
        L += ["- **%s** %s" % (t, f) for t, f in fails[:400]]
        if len(fails) > 400:
            L.append("- ... %d more in the step reports" % (len(fails) - 400))
        L.append("")
    sheets = [p for s in steps for p in s.get("sheets", [])]
    if sheets:
        L += ["## Contact sheets (open them with the Read tool, Part 11.4)", ""] + ["- `%s`" % p for p in sheets] + [""]
    md = qc.write_text(os.path.join(a.out_dir, "summary_%s.md" % run_name), "\n".join(L))
    print("[run_all] %s -> %s" % (overall, md))
    return 1 if overall == FAIL else 0


if __name__ == "__main__":
    sys.exit(main())
