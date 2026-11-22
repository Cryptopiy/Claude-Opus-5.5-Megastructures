#!/usr/bin/env python3
"""perf_windows: per-second frame times of a PerfMonitor CSV in the windows the spec names (QA fix wave C24).

Usage:
  python -I Tools/qa/perf_windows.py QA/reports/<perf>.csv [--run <id>] [--out-dir QA/reports] [--frame-max 20] [--gpu-max 8.2]

Windows (s in metres of the walk, from the CSV's per-frame s): 14.5.4 (4) s 580-600 and 700-740 (<= 20 ms), and +-10 s of walk time
around each 14.6 beat boundary (s 120, 400, 560, 880, 1080, 1260; 14.10: no frame over 20 ms around the boundaries). Per window and
per 1 s bin of t_virtual: frame count, max / p99 / mean frame_ms and gpu_ms. Gates: no frame over --frame-max (20 ms) in a window;
the GPU p99 at 1080p <= --gpu-max (8.2 ms, the invariant; INFO when the run was 4K: -perf-rt 3840x2160 in the summary's
offscreenRt). A window the run never reached reads SKIP; the report's Result is PASS only when every window measured.
Writes QA/reports/perf_windows_<run>.md / .json. Exit 1 on a FAIL.
"""
import argparse
import csv
import json
import os
import sys

_HERE = os.path.dirname(os.path.abspath(__file__))
if _HERE not in sys.path:
    sys.path.insert(0, _HERE)

import numpy as np  # noqa: E402

import qa_common as qc  # noqa: E402
from qa_common import PASS, FAIL, SKIP, INFO, check  # noqa: E402

BOUNDARIES = (120.0, 400.0, 560.0, 880.0, 1080.0, 1260.0)
S_WINDOWS = ((580.0, 600.0, "14.5.4 (4) s 580-600"), (700.0, 740.0, "14.5.4 (4) s 700-740"))
HALF_S = 10.0


def load(path):
    rows = []
    with open(path, newline="", encoding="utf-8") as f:
        for r in csv.DictReader(f):
            try:
                rows.append((float(r["t_virtual"]), float(r["s"]), float(r["frame_ms"]), float(r.get("gpu_ms", "nan") or "nan")))
            except (KeyError, ValueError):
                continue
    a = np.array(rows, dtype=np.float64) if rows else np.zeros((0, 4))
    return a


def window_stats(a, mask, name, frame_max, gpu_max, gpu_gate):
    w = a[mask]
    if w.shape[0] == 0:
        return {"name": name, "frames": 0}, [check(name, SKIP, 0, "frames", "the run never reached this window")]
    t0 = w[0, 0]
    bins = []
    for b in range(int(np.floor(w[:, 0].min() - t0)), int(np.ceil(w[:, 0].max() - t0)) + 1):
        m = (w[:, 0] - t0 >= b) & (w[:, 0] - t0 < b + 1)
        if m.any():
            g = w[m, 3][np.isfinite(w[m, 3]) & (w[m, 3] > 0)]
            bins.append({"t": round(float(t0 + b), 2), "s0": round(float(w[m, 1].min()), 1), "frames": int(m.sum()),
                         "max_ms": round(float(w[m, 2].max()), 2), "mean_ms": round(float(w[m, 2].mean()), 2),
                         "gpu_max_ms": round(float(g.max()), 2) if g.size else None})
    fm = w[:, 2]
    g = w[:, 3][np.isfinite(w[:, 3]) & (w[:, 3] > 0)]
    st = {"name": name, "frames": int(w.shape[0]), "s": [round(float(w[:, 1].min()), 1), round(float(w[:, 1].max()), 1)],
          "max_ms": float(fm.max()), "p99_ms": float(np.percentile(fm, 99)), "mean_ms": float(fm.mean()),
          "over": int((fm > frame_max).sum()), "gpu_p99_ms": float(np.percentile(g, 99)) if g.size else None,
          "gpu_mean_ms": float(g.mean()) if g.size else None, "bins": bins}
    C = [check(name + ": frames over %g ms" % frame_max, PASS if st["over"] == 0 else FAIL, st["over"], "0",
               "max %.2f ms, p99 %.2f, mean %.2f over %d frames (s %s)" % (st["max_ms"], st["p99_ms"], st["mean_ms"], st["frames"], st["s"]))]
    if st["gpu_p99_ms"] is not None:
        C.append(check(name + ": GPU p99", (PASS if st["gpu_p99_ms"] <= gpu_max else FAIL) if gpu_gate else INFO,
                       round(st["gpu_p99_ms"], 3), "<= %g ms (1080p)" % gpu_max, "mean %.3f ms%s" % (st["gpu_mean_ms"], "" if gpu_gate else "; not a 1080p run (INFO)")))
    return st, C


def run(path, run_name=None, out_dir=qc.REPORTS_DIR, frame_max=20.0, gpu_max=8.2):
    a = load(path)
    run_name = run_name or os.path.splitext(os.path.basename(path))[0]
    summ = qc.read_json(os.path.splitext(path)[0] + ".summary.json", {}) or {}
    rt = str(summ.get("offscreenRt") or "")
    gpu_gate = not ("3840" in rt or "2160" in rt)
    res, checks = [], []
    if a.shape[0] == 0:
        checks.append(check("perf CSV", FAIL, 0, "rows", "no readable row in %s" % path))
    else:
        for s0, s1, nm in S_WINDOWS:
            st, C = window_stats(a, (a[:, 1] >= s0) & (a[:, 1] <= s1), nm, frame_max, gpu_max, gpu_gate)
            res.append(st)
            checks += C
        for b in BOUNDARIES:
            cross = np.flatnonzero(a[:, 1] >= b)
            nm = "boundary s %g +-%g s" % (b, HALF_S)
            if cross.size == 0 or cross[0] == 0:
                st, C = {"name": nm, "frames": 0}, [check(nm, SKIP, 0, "frames", "the run did not cross s %g" % b)]
            else:
                tb = a[cross[0], 0]
                st, C = window_stats(a, np.abs(a[:, 0] - tb) <= HALF_S, nm, frame_max, gpu_max, gpu_gate)
            res.append(st)
            checks += C
    status = FAIL if any(c["status"] == FAIL for c in checks) else (SKIP if any(c["status"] == SKIP for c in checks) else PASS)
    js = os.path.join(out_dir, "perf_windows_%s.json" % run_name)
    md = os.path.join(out_dir, "perf_windows_%s.md" % run_name)
    qc.write_json(js, {"run": run_name, "csv": path, "status": status, "offscreenRt": rt, "windows": res, "checks": checks})
    L = ["# Perf windows: %s" % run_name, "", "CSV `%s` (%d frames; offscreen RT %s)" % (path, a.shape[0], rt or "none: window size"), "",
         "**Result: %s**" % status, "", qc.md_table(["Check", "Status", "Value", "Threshold", "Note"],
                                                       [[c["name"], c["status"], qc.fmt(c["value"], 3), c["threshold"] or "", c["note"] or ""] for c in checks]), ""]
    for st in res:
        if st.get("bins"):
            L += ["## %s" % st["name"], "", qc.md_table(["t", "s", "frames", "max ms", "mean ms", "GPU max ms"],
                                                        [[b["t"], b["s0"], b["frames"], b["max_ms"], b["mean_ms"], qc.fmt(b["gpu_max_ms"], 2)] for b in st["bins"]]), ""]
    qc.write_text(md, "\n".join(L))
    return {"status": status, "md": md, "json": js, "fails": [c["name"] for c in checks if c["status"] == FAIL]}


def main(argv=None):
    ap = argparse.ArgumentParser(prog="perf_windows.py", description=__doc__.split("\n\n")[0], epilog=__doc__,
                                 formatter_class=argparse.RawDescriptionHelpFormatter)
    ap.add_argument("csv")
    ap.add_argument("--run")
    ap.add_argument("--out-dir", default=qc.REPORTS_DIR)
    ap.add_argument("--frame-max", type=float, default=20.0)
    ap.add_argument("--gpu-max", type=float, default=8.2)
    a = ap.parse_args(argv)
    r = run(a.csv, a.run, a.out_dir, a.frame_max, a.gpu_max)
    print("[perf_windows] %s -> %s" % (r["status"], r["md"]))
    for f in r["fails"][:20]:
        print("  FAIL " + f)
    return 1 if r["status"] == FAIL else 0


if __name__ == "__main__":
    sys.exit(main())
