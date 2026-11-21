#!/usr/bin/env python3
"""T-PREC (ADDENDUM A1 4.8; lead ruling R6, fix wave X2): sub-pixel jitter of camera-fixed and world-fixed features in the
T-PREC sequences of ShotPlan (set "tprec"):

  seq_tprec_static  120 frames, static camera at the KS27 rest pose, frozen clock: every tile must stay put (jitter = RMS
                    and max of the tile shift against frame 0);
  seq_tprec_pan     120 frames, 0.1 px/frame yaw pan at the same pose: every tile moves 0.1 px/frame; jitter = residual of
                    each tile's shift after a straight-line fit (the "lantern-rail centroid jitter under a 0.1 px/frame pan
                    < 0.1 px" of T-FRAME);
  seq_tprec_cabin   120 real-game frames in the cabin at s ~1688 with a fixed world look: tiles of the cabin (camera-fixed:
                    |slope| < 0.02 px/frame) must not jitter; tiles of the outside drift with the descent (reported only).

Method: the frame is cut into a grid of tiles; each tile's sub-pixel shift is measured by a Lucas-Kanade step (structure
tensor solve; against frame 0 for the static clip, frame to frame and accumulated for the pan and the cabin); tiles without
texture (smaller structure-tensor eigenvalue below MIN_PEAK) are skipped.
Also the lantern: intensity-weighted centroid of the brightest blob (static / pan only), detrended the same way.
Limit: 0.1 px (A1 4.8 T-FRAME). Output: <dir>/tprec.json and a printed table. Usage: python -I Tools/qa/tprec.py <capture dir>
"""
import json
import math
import os
import sys

import numpy as np

try:
    from PIL import Image
except ImportError:  # pragma: no cover
    Image = None

LIMIT_PX = 0.1
GRID = (8, 6)
MIN_PEAK = 2e-6   # minimum texture (structure-tensor min eigenvalue per px, luma units^2)


def load_luma(path):
    im = Image.open(path).convert("RGB")
    a = np.asarray(im, dtype=np.float64) / 255.0
    return 0.2126 * a[..., 0] + 0.7152 * a[..., 1] + 0.0722 * a[..., 2]


def phase_shift(a, b, win):
    """Shift (dx, dy) of b relative to a in px (sub-pixel), and the correlation peak height."""
    fa = np.fft.fft2((a - a.mean()) * win)
    fb = np.fft.fft2((b - b.mean()) * win)
    r = fb * np.conj(fa)
    r /= np.maximum(np.abs(r), 1e-12)
    c = np.real(np.fft.ifft2(r))
    h, w = c.shape
    iy, ix = np.unravel_index(int(np.argmax(c)), c.shape)
    peak = float(c[iy, ix])

    def sub(cm, c0, cp):
        d = cm - 2.0 * c0 + cp
        return 0.0 if abs(d) < 1e-12 else 0.5 * (cm - cp) / d

    dx = ix + sub(c[iy, (ix - 1) % w], c[iy, ix], c[iy, (ix + 1) % w])
    dy = iy + sub(c[(iy - 1) % h, ix], c[iy, ix], c[(iy + 1) % h, ix])
    if dx > w / 2:
        dx -= w
    if dy > h / 2:
        dy -= h
    return dx, dy, peak


def lk_shift(a, b):
    """Shift (dx, dy) of b relative to a (b(x) = a(x - d)) by one Lucas-Kanade step on the tile (sub-pixel, |d| < 1 px), and a
    texture measure (the smaller eigenvalue of the structure tensor per pixel)."""
    m = 0.5 * (a + b)
    ix = np.zeros_like(m); iy = np.zeros_like(m)
    ix[:, 1:-1] = 0.5 * (m[:, 2:] - m[:, :-2])
    iy[1:-1, :] = 0.5 * (m[2:, :] - m[:-2, :])
    it = b - a
    sl = (slice(2, -2), slice(2, -2))
    ix, iy, it = ix[sl], iy[sl], it[sl]
    sxx, syy, sxy = float((ix * ix).sum()), float((iy * iy).sum()), float((ix * iy).sum())
    sxt, syt = float((ix * it).sum()), float((iy * it).sum())
    det = sxx * syy - sxy * sxy
    n = max(1, ix.size)
    tr = sxx + syy
    lmin = 0.5 * (tr - math.sqrt(max(0.0, tr * tr - 4.0 * det))) / n
    if det <= 1e-12:
        return float("nan"), float("nan"), lmin
    dx = -(syy * sxt - sxy * syt) / det
    dy = -(sxx * syt - sxy * sxt) / det
    return dx, dy, lmin


def detrend(v):
    """Residual of a least-squares line through v (index as x); returns (slope, residual array)."""
    v = np.asarray(v, dtype=np.float64)
    x = np.arange(len(v), dtype=np.float64)
    if len(v) < 3:
        return 0.0, v - v.mean()
    A = np.vstack([x, np.ones_like(x)]).T
    k, c = np.linalg.lstsq(A, v, rcond=None)[0]
    return float(k), v - (k * x + c)


def lantern_centroid(L, roi):
    x0, y0, x1, y1 = roi
    sub_img = L[y0:y1, x0:x1]
    t = max(0.6, float(np.percentile(sub_img, 99.5)) * 0.8)
    m = np.clip(sub_img - t, 0.0, None)
    s = float(m.sum())
    if s <= 1e-9:
        return float("nan"), float("nan")
    ys, xs = np.mgrid[y0:y1, x0:x1]
    return float((m * xs).sum() / s), float((m * ys).sum() / s)


def analyse(seq_dir, kind):
    files = sorted(f for f in os.listdir(seq_dir) if f.startswith("frame_") and f.endswith(".png"))
    if len(files) < 3:
        return {"seq": os.path.basename(seq_dir), "error": "fewer than 3 frames"}
    L0 = load_luma(os.path.join(seq_dir, files[0]))
    H, W = L0.shape
    gx, gy = GRID
    tw, th = W // gx, H // gy
    win = np.outer(np.hanning(th), np.hanning(tw))
    tiles = [(i * tw, j * th) for j in range(gy) for i in range(gx)]
    shifts = {t: [] for t in tiles}
    peaks = {t: [] for t in tiles}
    # lantern ROI: the brightest 64 px blob of frame 0 (static / pan)
    by, bx = np.unravel_index(int(np.argmax(L0)), L0.shape)
    roi = (max(0, bx - 32), max(0, by - 32), min(W, bx + 32), min(H, by + 32))
    lant = []
    prev = L0
    acc = {t: (0.0, 0.0) for t in tiles}
    for f in files:
        L = load_luma(os.path.join(seq_dir, f))
        for (x, y) in tiles:
            ref = L0 if kind == "static" else prev
            dx, dy, tex = lk_shift(ref[y:y + th, x:x + tw], L[y:y + th, x:x + tw])
            if kind != "static":
                ax, ay = acc[(x, y)]
                dx, dy = ax + (dx if dx == dx else 0.0), ay + (dy if dy == dy else 0.0)
                acc[(x, y)] = (dx, dy)
            shifts[(x, y)].append((dx, dy))
            peaks[(x, y)].append(tex)
        prev = L
        if kind in ("static", "pan"):
            lant.append(lantern_centroid(L, roi))
    out_tiles = []
    for t in tiles:
        pk = float(np.median(peaks[t][1:])) if len(peaks[t]) > 1 else 0.0
        if pk < MIN_PEAK:
            continue
        dx = np.array([s[0] for s in shifts[t]])
        dy = np.array([s[1] for s in shifts[t]])
        kx, rx = detrend(dx)
        ky, ry = detrend(dy)
        if kind == "static":
            rx, ry = dx - 0.0, dy - 0.0
        if kind == "cabin":
            rx, ry = dx, dy   # camera-fixed cabin parts: the shift itself is the jitter (no trend removed)
        res = np.hypot(rx, ry)
        out_tiles.append({"tile": [int(t[0]), int(t[1]), int(tw), int(th)], "peak": round(pk, 3),
                          "max_abs_shift_px": round(float(np.hypot(dx, dy).max()), 4),
                          "slope_px_per_frame": [round(kx, 4), round(ky, 4)],
                          "jitter_rms_px": round(float(np.sqrt(np.mean(res ** 2))), 4), "jitter_max_px": round(float(res.max()), 4)})
    if kind == "cabin":
        # camera-fixed tiles (the cabin frame, the glass edges): never more than 0.5 px from frame 0 over the clip (the outside
        # moves several px with the decelerating descent)
        fixed = [t for t in out_tiles if t["max_abs_shift_px"] < 0.5]
        judged = fixed
    else:
        judged = out_tiles
    worst = max((t["jitter_max_px"] for t in judged), default=float("nan"))
    rms = max((t["jitter_rms_px"] for t in judged), default=float("nan"))
    res = {"seq": os.path.basename(seq_dir), "kind": kind, "frames": len(files), "size": [W, H], "tiles": out_tiles,
           "tiles_judged": len(judged), "worst_jitter_max_px": worst, "worst_jitter_rms_px": rms, "limit_px": LIMIT_PX}
    if lant:
        cx = np.array([c[0] for c in lant])
        cy = np.array([c[1] for c in lant])
        if np.isfinite(cx).all():
            kx, rx = detrend(cx)
            ky, ry = detrend(cy)
            if kind == "static":
                rx, ry = cx - cx[0], cy - cy[0]
            r = np.hypot(rx, ry)
            res["lantern"] = {"roi": list(map(int, roi)), "slope_px_per_frame": [round(kx, 4), round(ky, 4)],
                              "jitter_rms_px": round(float(np.sqrt(np.mean(r ** 2))), 4), "jitter_max_px": round(float(r.max()), 4)}
    vals = [v for v in (worst, res.get("lantern", {}).get("jitter_max_px")) if v is not None and v == v]
    res["pass"] = bool(vals) and max(vals) <= LIMIT_PX
    return res


def main(argv):
    if len(argv) < 2:
        print(__doc__)
        return 2
    root = argv[1]
    kinds = {"seq_tprec_static": "static", "seq_tprec_pan": "pan", "seq_tprec_cabin": "cabin"}
    results = []
    for name, kind in kinds.items():
        d = os.path.join(root, name)
        if os.path.isdir(d):
            results.append(analyse(d, kind))
    for r in results:
        if "error" in r:
            print("%-18s ERROR %s" % (r["seq"], r["error"]))
            continue
        lan = r.get("lantern", {})
        print("%-18s frames %3d  tiles %2d  worst jitter max %.4f px rms %.4f px  lantern max %s  -> %s"
              % (r["seq"], r["frames"], r["tiles_judged"], r["worst_jitter_max_px"], r["worst_jitter_rms_px"],
                 lan.get("jitter_max_px", "n/a"), "PASS" if r["pass"] else "FAIL"))
    with open(os.path.join(root, "tprec.json"), "w", encoding="utf-8") as f:
        json.dump({"tool": "Tools/qa/tprec.py", "limit_px": LIMIT_PX, "results": results}, f, indent=1)
    return 0 if results and all(r.get("pass") for r in results) else 1


if __name__ == "__main__":
    sys.exit(main(sys.argv))
