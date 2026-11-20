#!/usr/bin/env python3
"""debugview: decode the -debug-view N captures of Nocturne (TEXTURE_PLAN 7.1; TP's NocDebugViewFeature / NocLitPass, FA's
NocFacade debug codes).

Usage:
  python -I Tools/qa/debugview.py <capture dir | png ...> [--view N] [--maps] [--run <id>] [--out-dir QA/reports] [--quiet]

The view comes from --view, else from report.json ("env"."args": "-debug-view N"). The views write raw display codes (no fog,
grade, bloom or dither), 0 where no hook wrote one (non-facade lit surfaces); the sky keeps its own colour, so every decoder
classifies pixels by their code lattice first ("written" pixels) and reports the share it could not classify (MSAA edges blend
codes, the sky is not a debug surface).
  1 skin, region, override   R = skin x 40, G = region x 128, B = 16 (k + 1) for override k (0 = regional skin): histograms
  2 regime weights            R = window weight w_u w_v, G = w_tex, B = w_room: means, fade-band shares (0.05 < w < 0.95)
  3 cell coordinates          R = frac(x / P.x), G = frac(y / P.y), B = whole-bay mask: bay share
  4 frame sign sigma          R = 255 where sigma = -1, G = 255 where +1: shares; FAIL when a pixel carries both signs
  5 footprint                 R, G = 32 (log2(P / fp) + 2): cell period in pixels 2^(code / 32 - 2) per axis; shares under 2.5 px
                              (macro regime) and in the 2.5-5.5 px fade band
  6 recess shift              R, G = 128 + 64 du / W, 128 + 64 dv / H: shift statistics
  7 world normal              128 + 127 n: unit-length share of the written pixels (FAIL under 98 %), dominant axes
  8 texel density             0.25 m checker x a mip-level colour: the colour histogram (palette set by the hooks)
  9 overdraw                  R = 8 x shaded, G = 8 x complexity, B = 255 where covered: overdraw = sum(R / 8) / covered and the
                              depth complexity; FAIL above 1.5 at KS04, KS05, KS09 and KS17 (TEXTURE_PLAN 9: else the prepass)
 10 lit probability, key      R = p, G = K & 0xFF: mean p over written pixels, distinct tower keys
--maps writes false-colour maps of views 2, 5, 9 and 10 into <dir>/_debugview/ (look at them with the Read tool).
Writes QA/reports/debugview_<run>.json and .md.  Exit code 1 if any check FAILs.
"""
import argparse
import glob
import json
import os
import re
import sys

_HERE = os.path.dirname(os.path.abspath(__file__))
if _HERE not in sys.path:
    sys.path.insert(0, _HERE)

import numpy as np  # noqa: E402

import qa_common as qc  # noqa: E402
from qa_common import PASS, FAIL, SKIP, INFO, WARN, check  # noqa: E402

OVERDRAW_MAX = 1.5                                       # TEXTURE_PLAN 9
OVERDRAW_SHOTS = ("KS04", "KS05", "KS09", "KS17")
NORMAL_UNIT_MIN = 0.98
VIEW_NAMES = {1: "skin/region/override", 2: "regime weights", 3: "cell coordinates", 4: "frame sign", 5: "footprint",
              6: "recess shift", 7: "world normal", 8: "texel density", 9: "overdraw", 10: "lit probability / key"}


def view_from_report(cap_dir):
    rep = qc.read_json(os.path.join(cap_dir, "report.json"), {}) or {}
    args = ((rep.get("env") or {}).get("args") or "")
    m = re.search(r"-debug-view\s+(\d+)", args)
    return int(m.group(1)) if m else None


def _share(mask, base):
    n = int(base.sum())
    return float((mask & base).sum()) / n if n else 0.0


def decode(rgb8, view):
    """Decoded statistics of one image (module doc); returns (stats dict, checks list, maps dict of H x W x 3 uint8)."""
    a = rgb8.astype(np.int32)
    R, G, B = a[..., 0], a[..., 1], a[..., 2]
    H, W = R.shape
    N = float(H * W)
    S, C, M = {"view": view, "pixels": int(N)}, [], {}
    black = (R == 0) & (G == 0) & (B == 0)
    S["black_share"] = round(float(black.mean()), 4)
    if view == 1:
        lat = (np.abs(R - 40 * np.round(R / 40.0)) <= 1) & (np.isin(G, (0, 127, 128, 255)) | (np.abs(G - 128) <= 1)) \
              & (np.abs(B - 16 * np.round(B / 16.0)) <= 1)
        wr = lat & ~black
        S["written_share"] = round(float(wr.mean()), 4)
        skin = np.round(R[wr] / 40.0).astype(int)
        reg = np.round(G[wr] / 128.0).astype(int)
        ovr = np.round(B[wr] / 16.0).astype(int) - 1
        S["skins"] = {int(k): round(float((skin == k).mean()), 4) for k in np.unique(skin)} if skin.size else {}
        S["regions"] = {int(k): round(float((reg == k).mean()), 4) for k in np.unique(reg)} if reg.size else {}
        S["overrides"] = {int(k): round(float((ovr == k).mean()), 4) for k in np.unique(ovr)} if ovr.size else {}
        S["off_lattice_share"] = round(float((~lat & ~black).mean()), 4)
        C.append(check("v1 written pixels", INFO, S["written_share"], None, "off-lattice %.2f %% (edges, sky)" % (100 * S["off_lattice_share"])))
    elif view == 2:
        w, wt, wm = R / 255.0, G / 255.0, B / 255.0
        wr = ~black
        S["written_share"] = round(float(wr.mean()), 4)
        for nm, v in (("window", w), ("tex", wt), ("room", wm)):
            vv = v[wr]
            S[nm + "_mean"] = round(float(vv.mean()), 4) if vv.size else None
            S[nm + "_fade_share"] = round(float(((vv > 0.05) & (vv < 0.95)).mean()), 4) if vv.size else None
        C.append(check("v2 fade-band share (window)", INFO, S["window_fade_share"], None, "0.05 < w < 0.95 of written pixels"))
        M["regimes"] = np.stack([R, G, B], -1).astype(np.uint8)
    elif view == 3:
        wr = ~black
        S["written_share"] = round(float(wr.mean()), 4)
        S["bay_share"] = round(_share(B >= 128, wr), 4)
        C.append(check("v3 whole-bay share", INFO, S["bay_share"], None, "B >= 128 of written pixels"))
    elif view == 4:
        neg = (R >= 250) & (G <= 5)
        pos = (G >= 250) & (R <= 5)
        both = (R >= 128) & (G >= 128) & (B <= 5)
        wr = neg | pos
        S["written_share"] = round(float(wr.mean()), 4)
        S["sigma_neg_share"] = round(_share(neg, wr), 4)
        S["sigma_pos_share"] = round(_share(pos, wr), 4)
        S["both_share"] = round(float(both.mean()), 5)
        C.append(check("v4 sigma shares", INFO, [S["sigma_neg_share"], S["sigma_pos_share"]], None, "-1 / +1 of the facade pixels"))
        C.append(check("v4 no pixel with both signs", PASS if S["both_share"] <= 0.0005 else FAIL, S["both_share"], "<= 0.05 %",
                       "R and G both high: the sign flips inside a pixel (or MSAA blends two faces)"))
    elif view == 5:
        wr = ~black & (B <= 5)
        S["written_share"] = round(float(wr.mean()), 4)
        pu, pv = 2.0 ** (R / 32.0 - 2.0), 2.0 ** (G / 32.0 - 2.0)
        for nm, p in (("u", pu), ("v", pv)):
            pp = p[wr]
            if pp.size:
                S["period_px_" + nm] = {"p5": round(float(np.percentile(pp, 5)), 2), "median": round(float(np.median(pp)), 2),
                                        "p95": round(float(np.percentile(pp, 95)), 2)}
                S["macro_share_" + nm] = round(float((pp < 2.5).mean()), 4)
                S["fade_share_" + nm] = round(float(((pp >= 2.5) & (pp < 5.5)).mean()), 4)
        C.append(check("v5 cell period (u, v) median px", INFO, [S.get("period_px_u", {}).get("median"), S.get("period_px_v", {}).get("median")],
                       None, "macro share u %s, fade band u %s" % (S.get("macro_share_u"), S.get("fade_share_u"))))
        lg = np.clip(np.log2(np.maximum(pv, 1e-6)) / 6.0, 0, 1)
        M["footprint_v"] = heat(lg, wr)
    elif view == 6:
        wr = ~black & (B <= 5)
        S["written_share"] = round(float(wr.mean()), 4)
        du, dv = (R - 128) / 64.0, (G - 128) / 64.0
        if wr.any():
            S["du_over_W"] = {"min": round(float(du[wr].min()), 3), "max": round(float(du[wr].max()), 3), "mean": round(float(du[wr].mean()), 4)}
            S["dv_over_H"] = {"min": round(float(dv[wr].min()), 3), "max": round(float(dv[wr].max()), 3), "mean": round(float(dv[wr].mean()), 4)}
        C.append(check("v6 recess shift range", INFO, S.get("du_over_W"), None, "du / W of the written pixels"))
    elif view == 7:
        n = (a - 128) / 127.0
        ln = np.sqrt((n * n).sum(-1))
        wr = ~black
        unit = (np.abs(ln - 1.0) <= 0.05) & wr
        S["written_share"] = round(float(wr.mean()), 4)
        S["unit_share"] = round(_share(unit, wr), 4)
        ax = np.argmax(np.abs(n), -1)
        sg = np.take_along_axis(np.sign(n), ax[..., None], -1)[..., 0]
        names = {(0, 1): "+x", (0, -1): "-x", (1, 1): "+y", (1, -1): "-y", (2, 1): "+z", (2, -1): "-z"}
        S["axes"] = {names[(k, s)]: round(_share(unit & (ax == k) & (sg == s), unit), 4) for k in (0, 1, 2) for s in (1, -1)}
        C.append(check("v7 unit normals", PASS if S["unit_share"] >= NORMAL_UNIT_MIN else WARN, S["unit_share"], ">= %g" % NORMAL_UNIT_MIN,
                       "non-unit pixels are MSAA edges or non-lit surfaces (sky)"))
    elif view == 8:
        wr = ~black
        S["written_share"] = round(float(wr.mean()), 4)
        q = (a[wr] // 16).astype(np.int64)
        key = q[:, 0] * 256 + q[:, 1] * 16 + q[:, 2]
        if key.size:
            u, cnt = np.unique(key, return_counts=True)
            order = np.argsort(-cnt)[:8]
            S["colours"] = [{"rgb16": [int(u[i] // 256) * 16, int((u[i] // 16) % 16) * 16, int(u[i] % 16) * 16],
                             "share": round(float(cnt[i]) / key.size, 4)} for i in order]
        C.append(check("v8 dominant colours", INFO, len(S.get("colours", [])), None, "mip-level colours x checker (hook palette)"))
    elif view == 9:
        cov = B >= 128
        S["covered_share"] = round(float(cov.mean()), 4)
        n_cov = int(cov.sum())
        S["overdraw"] = round(float((R[cov] / 8.0).sum()) / n_cov, 3) if n_cov else 0.0
        S["complexity"] = round(float((G[cov] / 8.0).sum()) / n_cov, 3) if n_cov else 0.0
        S["shaded_max"] = round(float(R[cov].max() / 8.0), 2) if n_cov else 0.0
        M["overdraw"] = heat(np.clip((R / 8.0 - 1.0) / 3.0, 0, 1), cov)
    elif view == 10:
        wr = ~black
        S["written_share"] = round(float(wr.mean()), 4)
        p = R / 255.0
        S["p_mean"] = round(float(p[wr].mean()), 4) if wr.any() else None
        S["keys"] = int(np.unique(G[wr]).size) if wr.any() else 0
        C.append(check("v10 mean lit probability", INFO, S["p_mean"], None, "LOOK_V2 4: lit fraction 0.18 (city mean)"))
        M["lit_p"] = heat(p, wr)
    else:
        C.append(check("view", FAIL, view, "1..10"))
    return S, C, M


def heat(v, mask):
    """Black -> blue -> green -> yellow -> white false colour of v in 0..1; grey 32 outside the mask."""
    v = np.clip(v, 0, 1)
    stops = np.array([[0, 0, 0], [0, 40, 200], [0, 200, 80], [240, 220, 0], [255, 255, 255]], dtype=np.float64)
    x = v * (len(stops) - 1)
    i = np.clip(np.floor(x).astype(int), 0, len(stops) - 2)
    f = (x - i)[..., None]
    out = stops[i] * (1 - f) + stops[i + 1] * f
    out[~mask] = 32
    return out.astype(np.uint8)


def analyse(path, view, maps_dir=None):
    rgb8, _ = qc.load_rgb8(path)
    tag = os.path.splitext(os.path.basename(path))[0]
    S, C, M = decode(rgb8, view)
    if view == 9:
        ks = tag[:4]
        gate = ks in OVERDRAW_SHOTS and "_4k" not in tag and "_1610" not in tag and "_219" not in tag
        C.append(check("v9 overdraw", (PASS if S["overdraw"] <= OVERDRAW_MAX else FAIL) if gate else INFO, S["overdraw"],
                       "<= %g" % OVERDRAW_MAX if gate else None, "complexity %.2f, covered %.1f %% (TEXTURE_PLAN 9)" % (
                           S["complexity"], 100 * S["covered_share"])))
    if maps_dir and M:
        for nm, img in M.items():
            qc.save_png(os.path.join(maps_dir, "%s_v%d_%s.png" % (tag, view, nm)), img)
    return {"file": os.path.basename(path), "tag": tag, "view": view, "stats": S, "checks": C,
            "status": qc.worst_status(C) if C else SKIP}


def collect(paths):
    files, views = [], {}
    for p in paths:
        if os.path.isdir(p):
            v = view_from_report(p)
            for f in sorted(glob.glob(os.path.join(p, "*.png"))):
                files.append(f)
                views[f] = v
        elif p.lower().endswith(".png") and os.path.isfile(p):
            files.append(p)
            views[p] = view_from_report(os.path.dirname(os.path.abspath(p)))
    return files, views


def build_markdown(run, results):
    allc = [c for r in results for c in r["checks"]]
    cnt = qc.count_status(allc)
    L = ["# Debug views: %s" % run, "", "Images: %d. Checks: %d PASS, %d FAIL, %d WARN, %d INFO (TEXTURE_PLAN 7.1)." % (
        len(results), cnt[PASS], cnt[FAIL], cnt[WARN], cnt.get(INFO, 0)), ""]
    rows = []
    for r in results:
        S = r["stats"]
        key = {1: "skins", 2: "window_mean", 3: "bay_share", 4: "sigma_neg_share", 5: "period_px_v", 6: "du_over_W",
               7: "unit_share", 8: "colours", 9: "overdraw", 10: "p_mean"}.get(r["view"])
        rows.append([r["tag"], r["view"], VIEW_NAMES.get(r["view"], "?"), qc.fmt(S.get("written_share", S.get("covered_share")), 3),
                     json.dumps(qc.sanitize(S.get(key)))[:80], r["status"]])
    L.append(qc.md_table(["Image", "View", "Name", "Written", "Key value", "Status"], rows))
    for r in results:
        bad = [c for c in r["checks"] if c["status"] in (FAIL, WARN)]
        for c in bad:
            L.append("- %s %s: %s %s %s" % (c["status"], r["tag"], c["name"], qc.fmt(c["value"], 3), c["note"] or ""))
    return "\n".join(L)


def run(paths, view=None, run_name=None, out_dir=qc.REPORTS_DIR, maps=False, quiet=False):
    files, views = collect(paths)
    if run_name is None:
        p0 = paths[0] if paths else "."
        run_name = qc.run_id_for(p0 if os.path.isdir(p0) else os.path.dirname(os.path.abspath(p0)))
    results = []
    for f in files:
        v = view if view else views.get(f)
        if not v:
            continue
        md = os.path.join(os.path.dirname(os.path.abspath(f)), "_debugview") if maps else None
        r = analyse(f, int(v), md)
        results.append(r)
        if not quiet:
            print("[debugview] %-34s view %d %s" % (r["tag"], r["view"], r["status"]), flush=True)
    allc = [c for r in results for c in r["checks"]]
    status = FAIL if any(c["status"] == FAIL for c in allc) else (PASS if results else SKIP)
    js = os.path.join(out_dir, "debugview_%s.json" % run_name)
    mdp = os.path.join(out_dir, "debugview_%s.md" % run_name)
    if results:
        qc.write_json(js, {"run": run_name, "status": status, "images": results})
        qc.write_text(mdp, build_markdown(run_name, results))
    fails = ["%s: %s (%s)" % (r["tag"], c["name"], qc.fmt(c["value"], 3)) for r in results for c in r["checks"] if c["status"] == FAIL]
    return {"tool": "debugview", "status": status, "json": js if results else None, "md": mdp if results else None,
            "fails": fails, "counts": qc.count_status(allc), "n": len(results),
            "note": "" if results else "no -debug-view in report.json and no --view"}


def main(argv=None):
    ap = argparse.ArgumentParser(prog="debugview.py", description=__doc__.split("\n\n")[0],
                                 formatter_class=argparse.RawDescriptionHelpFormatter, epilog=__doc__)
    ap.add_argument("paths", nargs="+")
    ap.add_argument("--view", type=int, help="debug view 1..10 (default: from report.json)")
    ap.add_argument("--maps", action="store_true", help="write false-colour maps into <dir>/_debugview/")
    ap.add_argument("--run")
    ap.add_argument("--out-dir", default=qc.REPORTS_DIR)
    ap.add_argument("--quiet", action="store_true")
    a = ap.parse_args(argv)
    missing = [p for p in a.paths if not os.path.exists(p)]
    if missing:
        print("not found: %s" % ", ".join(missing), file=sys.stderr)
        return 1
    r = run(a.paths, a.view, a.run, a.out_dir, a.maps, a.quiet)
    print("[debugview] %s: %d images, %d FAIL -> %s" % (r["status"], r["n"], len(r["fails"]), r["md"] or r["note"]))
    for f in r["fails"][:30]:
        print("  FAIL " + f)
    return 1 if r["status"] == FAIL else 0


if __name__ == "__main__":
    sys.exit(main())
