#!/usr/bin/env python3
"""tiling: texture repetition and detail of the textured regions of Nocturne captures (TEXTURE_PLAN 10 R3; LOOK_V2 8.2 check 7).

Usage:
  python -I Tools/qa/tiling.py <capture dir | png ...> [--regions file.json] [--rect x0,y0,x1,y1] [--period MIN:MAX]
         [--run <id>] [--out-dir QA/reports] [--quiet]

Per region (a rectangle in frame fractions, or in pixels at 1920 x 1080 with "units": "px"):
  - repetition (R3): the normalised autocorrelation R(lag) of the high-passed display luma (codes minus a box mean of hp px,
    hp = max(9, min(w, h) / 6), so light pools and shading gradients do not count), Hann-windowed, zero-padded and unbiased by the
    window's own autocorrelation, R(0) = 1. The main lobe (lags inside the radius where the ring mean of R first falls below
    0.2, at least 3 px) is excluded; lags reach half the region and only where the window overlap is >= 30 %.
      * with a period range (--period MIN:MAX or the region's "period": [min, max], px at 1080p, scaled to the image height):
        "peak_in_range" = max R with |lag| in the range: FAIL above 0.25 (R3: the tile period and the hex period at most 0.25 of
        zero lag);
      * without one: "peak" = the strongest repetition peak, WARN above 0.25. Structure (brick courses, floor lines, panel joints,
        window grids) is periodic by design, so a gate needs the texture's own period; the five strongest peaks (lag in px,
        value) are listed to find it.
    A repetition peak is a 5 x 5 local maximum of R that rises >= 0.05 above the trough between it and zero lag (along the same
    direction): a streaky texture (bark, rain streaks) correlates at every lag along its streaks without repeating, and has no
    such trough. Peaks on the 64 px lattice of the screen-locked dither tile (32 px after a -capture-ss 2 downsample) are
    reported apart as "dither_peak" (in flat regions the dither is all the high-pass sees).
  - detail (LOOK_V2 8.2 check 7, the same formulas as lookv2_check.py, which is the gate): micro = median over 32 x 32 tiles
    of the std of (luma - 9 x 9 box mean) (tiles with mean < Floor + 2 or any pixel >= 200 skipped), meso = std of the 8 x 8
    block means minus their 3 x 3 block mean, and the valid tile share. INFO here (thresholds micro 1.4, meso 1.0).
Default regions: LOOK_V2's wall regions of KS00, KS01, KS02 and KS19, the KS00 floor, and the central 80 % of every texclose
still (tc_*). Regions file: {"regions": [{"match": "KS00*", "name": "wall_w", "rect": [x0, y0, x1, y1], "units": "frac|px",
"period": [min, max]}]}.  --rect applies one fraction rectangle to every image given.
Writes QA/reports/tiling_<run>.json and .md.  Exit code 1 if any check FAILs.
"""
import argparse
import fnmatch
import glob
import os
import sys

_HERE = os.path.dirname(os.path.abspath(__file__))
if _HERE not in sys.path:
    sys.path.insert(0, _HERE)

import numpy as np  # noqa: E402

import qa_common as qc  # noqa: E402
from qa_common import PASS, FAIL, SKIP, INFO, WARN, check  # noqa: E402

FLOOR_LUMA = 11.8          # LOOK_V2 2: Floor #0A0C0F
R3_MAX = 0.25              # TEXTURE_PLAN 10 R3
MAIN_LOBE_LEVEL = 0.2
MIN_LOBE_PX = 3
MIN_OVERLAP = 0.30
DITHER_TILE = 64           # NocGrade's static blue-noise tile (BlueNoise.Size, 14.12): screen-locked, so it repeats every 64 px
MIN_BUMP = 0.05            # a repetition peak rises at least this much above the trough between it and zero lag
MICRO_MIN, MESO_MIN = 1.4, 1.0   # LOOK_V2 8.2 check 7 (reported)

# LOOK_V2 8.2 TEX_REGIONS (frame fractions), the KS00 floor and the texclose stills
DEFAULT_REGIONS = [
    {"match": "KS00*", "name": "wall_w", "rect": [0.02, 0.15, 0.28, 0.70]},
    {"match": "KS00*", "name": "wall_e", "rect": [0.72, 0.15, 0.98, 0.70]},
    {"match": "KS00*", "name": "floor", "rect": [0.30, 0.82, 0.70, 0.99]},
    {"match": "KS01*", "name": "wall_w", "rect": [0.02, 0.20, 0.30, 0.75]},
    {"match": "KS02*", "name": "wall_w", "rect": [0.02, 0.40, 0.20, 0.95]},
    {"match": "KS02*", "name": "wall_e", "rect": [0.80, 0.40, 0.98, 0.95]},
    {"match": "KS19*", "name": "wall_w", "rect": [0.05, 0.35, 0.30, 0.95]},
    {"match": "KS19*", "name": "wall_e", "rect": [0.62, 0.40, 0.95, 0.95]},
    {"match": "tc_*", "name": "centre", "rect": [0.10, 0.10, 0.90, 0.90]},
]


def luma_codes(rgb8):
    """Rec.709 luma of the display codes (0..255), float64 (LOOK_V2 8.2 measures on these)."""
    f = rgb8.astype(np.float64)
    return 0.2126 * f[..., 0] + 0.7152 * f[..., 1] + 0.0722 * f[..., 2]


def box_mean(a, k):
    """k x k box mean with edge padding (k odd)."""
    p = k // 2
    c = np.pad(np.pad(a, p, mode="edge").cumsum(0).cumsum(1), ((1, 0), (1, 0)))
    return (c[k:, k:] - c[:-k, k:] - c[k:, :-k] + c[:-k, :-k]) / float(k * k)


def autocorr(y, hp=None):
    """Normalised, window-unbiased autocorrelation of the high-passed region y. Returns (R, overlap, hp) with R and overlap
    as (2h - 1) x (2w - 1) arrays centred on zero lag (index [h - 1, w - 1])."""
    h, w = y.shape
    if hp is None:
        hp = max(9, (min(h, w) // 6) | 1)
    hp = int(hp) | 1
    x = y - box_mean(y, hp)
    win = np.outer(np.hanning(h), np.hanning(w))
    if not win.any():
        win = np.ones_like(x)
    xw = (x - (x * win).sum() / max(win.sum(), 1e-12)) * win
    H2, W2 = 2 * h, 2 * w
    F = np.fft.rfft2(xw, s=(H2, W2))
    A = np.fft.irfft2(F * np.conj(F), s=(H2, W2))
    Fw = np.fft.rfft2(win, s=(H2, W2))
    Aw = np.fft.irfft2(Fw * np.conj(Fw), s=(H2, W2))
    A = np.fft.fftshift(A)[1:, 1:]
    Aw = np.fft.fftshift(Aw)[1:, 1:]
    # fftshift of a 2h-long axis puts lag 0 at index h; after dropping index 0 it sits at h - 1
    ov = Aw / max(Aw[h - 1, w - 1], 1e-12)
    R = np.where(ov > 1e-6, A / np.maximum(Aw, 1e-12), 0.0)
    r0 = R[h - 1, w - 1]
    R = R / r0 if r0 > 1e-12 else R * 0.0
    return R, ov, hp


def repetition(y, period=None, top=5):
    """Repetition measures of one region (module doc). period = (min, max) px or None."""
    h, w = y.shape
    R, ov, hp = autocorr(y)
    cy, cx = h - 1, w - 1
    ly, lx = np.mgrid[-cy:cy + 1, -cx:cx + 1]
    rad = np.sqrt(lx * lx + ly * ly)
    # main lobe: the first ring whose mean falls under MAIN_LOBE_LEVEL
    r_lobe = MIN_LOBE_PX
    for r in range(1, int(min(h, w) // 2)):
        ring = (rad >= r - 0.5) & (rad < r + 0.5)
        if ring.any() and R[ring].mean() < MAIN_LOBE_LEVEL:
            r_lobe = max(MIN_LOBE_PX, r)
            break
    else:
        r_lobe = int(min(h, w) // 2)
    valid = (rad >= r_lobe) & (np.abs(lx) <= w // 2) & (np.abs(ly) <= h // 2) & (ov >= MIN_OVERLAP)
    # one half-plane only (R(l) = R(-l))
    half = (ly > 0) | ((ly == 0) & (lx > 0))
    v = valid & half
    out = {"hp_px": int(hp), "main_lobe_px": int(r_lobe), "lags": int(v.sum())}
    # 5 x 5 local maxima of the unmasked R (a lag on the main lobe's edge is no maximum along a decaying direction)
    P = np.pad(R, 2, mode="constant", constant_values=-np.inf)
    nb = np.max(np.stack([P[2 + dy:P.shape[0] - 2 + dy, 2 + dx:P.shape[1] - 2 + dx]
                          for dy in range(-2, 3) for dx in range(-2, 3) if dy or dx]), axis=0)
    idx = np.flatnonzero(v & (R >= nb))
    ts = np.linspace(0.3, 0.9, 13)
    peaks, dither = [], []
    for j in idx:
        ax_, ay_ = int(lx.flat[j]), int(ly.flat[j])
        trough = min(R[cy + int(round(t * ay_)), cx + int(round(t * ax_))] for t in ts)
        val = float(R.flat[j])
        if val - trough < MIN_BUMP:
            continue
        on_lat = all(min(abs(c) % L, L - abs(c) % L) <= 1 for c in (ax_, ay_) for L in (DITHER_TILE,)) or \
            all(min(abs(c) % (DITHER_TILE // 2), DITHER_TILE // 2 - abs(c) % (DITHER_TILE // 2)) <= 1 for c in (ax_, ay_))
        (dither if on_lat else peaks).append((val, ax_, ay_, float(rad.flat[j]), val - trough))
    peaks.sort(key=lambda q: -q[0])
    dither.sort(key=lambda q: -q[0])
    out["peak"] = round(peaks[0][0], 4) if peaks else 0.0
    out["peak_lag"] = [peaks[0][1], peaks[0][2]] if peaks else None
    out["dither_peak"] = round(dither[0][0], 4) if dither else 0.0
    out["top"] = [{"lag": [q[1], q[2]], "period_px": round(q[3], 1), "r": round(q[0], 3), "bump": round(q[4], 3)} for q in peaks[:top]]
    if period:
        pmin, pmax = float(period[0]), float(period[1])
        inr = [q for q in peaks if pmin <= q[3] <= pmax]
        out["peak_in_range"] = round(inr[0][0], 4) if inr else 0.0
        out["peak_in_range_lag"] = [inr[0][1], inr[0][2]] if inr else None
        out["period_px"] = [pmin, pmax]
    return out


def detail(Yr):
    """LOOK_V2 8.2 check 7 (lookv2_check.texture): micro, meso and the valid tile share."""
    k = 9
    hp = Yr - box_mean(Yr, k)
    T = 32
    H, W = Yr.shape
    sd, tot = [], 0
    for y in range(0, H - T + 1, T):
        for x in range(0, W - T + 1, T):
            tot += 1
            t = Yr[y:y + T, x:x + T]
            if t.mean() >= FLOOR_LUMA + 2 and t.max() < 200:
                sd.append(hp[y:y + T, x:x + T].std())
    hb, wb = H // 8, W // 8
    if hb >= 3 and wb >= 3:
        B = Yr[:hb * 8, :wb * 8].reshape(hb, 8, wb, 8).mean(axis=(1, 3))
        meso = float((B - box_mean(B, 3)).std())
    else:
        meso = 0.0
    return (float(np.median(sd)) if sd else 0.0), meso, len(sd) / max(tot, 1)


def regions_for(tag, file_regions, rect=None, period=None):
    if rect is not None:
        return [{"name": "rect", "rect": list(rect), "period": period}]
    out = []
    for e in file_regions:
        if fnmatch.fnmatch(tag, e.get("match", "*")):
            it = dict(e)
            if period and not it.get("period"):
                it["period"] = period
            out.append(it)
    return out


def rect_px(it, W, H):
    x0, y0, x1, y1 = [float(v) for v in it["rect"]]
    if it.get("units") == "px":
        sx, sy = W / 1920.0, H / 1080.0
        x0, x1, y0, y1 = x0 * sx, x1 * sx, y0 * sy, y1 * sy
    else:
        x0, x1, y0, y1 = x0 * W, x1 * W, y0 * H, y1 * H
    x0, y0 = max(0, int(round(x0))), max(0, int(round(y0)))
    x1, y1 = min(W, int(round(x1))), min(H, int(round(y1)))
    return x0, y0, x1, y1


def analyse_image(path, regions):
    rgb8, _ = qc.load_rgb8(path)
    H, W = rgb8.shape[:2]
    Y = luma_codes(rgb8)
    tag = os.path.splitext(os.path.basename(path))[0]
    res = {"file": os.path.basename(path), "tag": tag, "size": [W, H], "regions": [], "checks": []}
    C = res["checks"]
    for it in regions:
        x0, y0, x1, y1 = rect_px(it, W, H)
        name = it.get("name", "region")
        if x1 - x0 < 32 or y1 - y0 < 32:
            C.append(check("%s size" % name, SKIP, "%dx%d" % (x1 - x0, y1 - y0), ">= 32 x 32"))
            continue
        Yr = Y[y0:y1, x0:x1]
        per = it.get("period")
        if per:
            per = (per[0] * H / 1080.0, per[1] * H / 1080.0)
        rp = repetition(Yr, per)
        mi, me, valid = detail(Yr)
        r = {"name": name, "rect_px": [x0, y0, x1, y1], "repetition": rp, "micro": round(mi, 3), "meso": round(me, 3),
             "valid_tiles": round(valid, 3), "mean_luma": round(float(Yr.mean()), 2)}
        res["regions"].append(r)
        if per:
            v = rp.get("peak_in_range")
            C.append(check("%s repetition at %g-%g px" % (name, round(per[0], 1), round(per[1], 1)),
                           SKIP if v is None else (PASS if v <= R3_MAX else FAIL), None if v is None else round(v, 3),
                           "<= %g of zero lag" % R3_MAX, "lag %s (R3)" % rp.get("peak_in_range_lag")))
        else:
            v = rp.get("peak")
            C.append(check("%s repetition (any lag)" % name, SKIP if v is None else (INFO if v <= R3_MAX else WARN),
                           None if v is None else round(v, 3), "<= %g (no period given: structure is periodic too)" % R3_MAX,
                           "top %s" % ", ".join("%s:%s" % (t["lag"], t["r"]) for t in rp.get("top", [])[:3])))
        if valid < 0.5:
            C.append(check("%s detail" % name, INFO, round(valid, 2), ">= 0.5 valid tiles", "too dark or clipped for micro/meso"))
        else:
            C.append(check("%s micro" % name, INFO, round(mi, 2), ">= %g codes (LOOK_V2 8.2: gate in lookv2_check)" % MICRO_MIN))
            C.append(check("%s meso" % name, INFO, round(me, 2), ">= %g codes (LOOK_V2 8.2: gate in lookv2_check)" % MESO_MIN))
    res["status"] = qc.worst_status(C) if C else SKIP
    return res


def collect(paths):
    files = []
    for p in paths:
        if os.path.isdir(p):
            files += sorted(glob.glob(os.path.join(p, "*.png")))
        elif p.lower().endswith(".png") and os.path.isfile(p):
            files.append(p)
    return files


def build_markdown(run, results):
    allc = [c for r in results for c in r["checks"]]
    cnt = qc.count_status(allc)
    L = ["# Tiling and texture detail: %s" % run, "",
         "Images: %d. Checks: %d PASS, %d FAIL, %d WARN, %d INFO, %d SKIP (TEXTURE_PLAN 10 R3; LOOK_V2 8.2 check 7)." % (
             len(results), cnt[PASS], cnt[FAIL], cnt[WARN], cnt.get(INFO, 0), cnt[SKIP]), ""]
    rows = []
    for r in results:
        for g in r["regions"]:
            rp = g["repetition"]
            rows.append([r["tag"], g["name"], "%s" % g["rect_px"], qc.fmt(rp.get("peak"), 3), "%s" % rp.get("peak_lag"),
                         qc.fmt(rp.get("peak_in_range"), 3) if "peak_in_range" in rp else "-", qc.fmt(g["micro"], 2),
                         qc.fmt(g["meso"], 2), qc.fmt(g["valid_tiles"], 2), qc.fmt(g["mean_luma"], 1)])
    L.append(qc.md_table(["Image", "Region", "Rect px", "Peak R", "Peak lag", "Peak in range", "Micro", "Meso", "Valid", "Mean"],
                         rows))
    fails = [(r["tag"], c) for r in results for c in r["checks"] if c["status"] in (FAIL, WARN)]
    if fails:
        L += ["", "## FAIL and WARN", ""] + ["- %s %s: %s %s (%s)" % (c["status"], t, c["name"], qc.fmt(c["value"], 3),
                                                                     c["note"] or "") for t, c in fails]
    return "\n".join(L)


def run(paths, run_name=None, out_dir=qc.REPORTS_DIR, regions_file=None, rect=None, period=None, quiet=False):
    files = collect(paths)
    fr = DEFAULT_REGIONS
    if regions_file:
        d = qc.read_json(regions_file, {}) or {}
        fr = d.get("regions", d if isinstance(d, list) else [])
    if run_name is None:
        run_name = qc.run_id_for(paths[0] if paths and os.path.isdir(paths[0]) else os.path.dirname(os.path.abspath(paths[0])))
    results = []
    for f in files:
        tag = os.path.splitext(os.path.basename(f))[0]
        regs = regions_for(tag, fr, rect, period)
        if not regs:
            continue
        r = analyse_image(f, regs)
        results.append(r)
        if not quiet:
            print("[tiling] %-34s %s" % (tag, r["status"]), flush=True)
    allc = [c for r in results for c in r["checks"]]
    status = FAIL if any(c["status"] == FAIL for c in allc) else (PASS if results else SKIP)
    js = os.path.join(out_dir, "tiling_%s.json" % run_name)
    md = os.path.join(out_dir, "tiling_%s.md" % run_name)
    qc.write_json(js, {"run": run_name, "status": status, "images": results})
    qc.write_text(md, build_markdown(run_name, results))
    fails = ["%s: %s (%s)" % (r["tag"], c["name"], qc.fmt(c["value"], 3)) for r in results for c in r["checks"]
             if c["status"] == FAIL]
    return {"tool": "tiling", "status": status, "json": js, "md": md, "fails": fails, "counts": qc.count_status(allc),
            "n": len(results)}


def main(argv=None):
    ap = argparse.ArgumentParser(prog="tiling.py", description=__doc__.split("\n\n")[0],
                                 formatter_class=argparse.RawDescriptionHelpFormatter, epilog=__doc__)
    ap.add_argument("paths", nargs="+", help="capture folder(s) or PNG files")
    ap.add_argument("--regions", help="regions JSON (default: LOOK_V2 wall regions, KS00 floor, texclose centres)")
    ap.add_argument("--rect", help="x0,y0,x1,y1 frame fractions applied to every image")
    ap.add_argument("--period", help="MIN:MAX tile period range in px at 1080p (gates R3)")
    ap.add_argument("--run")
    ap.add_argument("--out-dir", default=qc.REPORTS_DIR)
    ap.add_argument("--quiet", action="store_true")
    a = ap.parse_args(argv)
    rect = [float(v) for v in a.rect.split(",")] if a.rect else None
    period = [float(v) for v in a.period.split(":")] if a.period else None
    missing = [p for p in a.paths if not os.path.exists(p)]
    if missing:
        print("not found: %s" % ", ".join(missing), file=sys.stderr)
        return 1
    r = run(a.paths, a.run, a.out_dir, a.regions, rect, period, a.quiet)
    print("[tiling] %s: %d images, %d FAIL -> %s" % (r["status"], r["n"], len(r["fails"]), r["md"]))
    for f in r["fails"][:30]:
        print("  FAIL " + f)
    return 1 if r["status"] == FAIL else 0


if __name__ == "__main__":
    sys.exit(main())
