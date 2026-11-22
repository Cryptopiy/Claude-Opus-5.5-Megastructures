#!/usr/bin/env python3
"""lookv2_check: the Look v2 acceptance checks of docs/LOOK_V2.md 8.2 on the 8-bit 1920 x 1080 key-shot PNGs (owner PA in Look v2, AT in Phase 4).

Usage:
  python -I Tools/qa/lookv2_check.py <capture dir> [<capture dir> ...] [--json out.json] [--md out.md] [--encoded]
--encoded: the dir holds frames decoded from the 11.12 CRF-23 4K encode (scaled to 1080p): texture meso >= 0.75, micro INFO.

Checks per KS*.png (the beat comes from s in report.json, else from the 14.10 table below; KS03 is beat 1, KS14 beat 4):
  1  R1a / R1b violet: hue 250-340, HSV sat > 0.35: luma 16-119 <= 1.0 % of the frame, luma >= 120 <= 1.5 %
  2  R2 largest saturated region of 32 x 32 block means (luma >= 16; blocks with >= 5 % of pixels at luma >= 160 excluded):
     sat limit 0.65 at hue 10-70, 0.60 in beat 6 and for the beat-7 koi (hue 160-220), 0.45 otherwise; any 250-340 block
     with sat > 0.20 counts; the largest 4-connected component <= 5 % of the blocks
  3  R3 mid-tone saturation: mean sat of 4 x 4 block means with luma 16-159 <= 0.35 (beats 1, 5), 0.40 (6), 0.30 (others)
  4  mean luma inside the KS window of 8.1 and the beat window
  5  sky_top: median luma / sat of the top 6 % x central 40 % (KS02: x 0.45-0.53) in the predicted window
  6  crush: share of 4 x 4 block means at luma <= Floor + 3 (14.8) per beat; 1st percentile >= 9
  7  texture (KS00, KS01, KS02, KS19 wall regions): micro >= 1.4, meso >= 1.0 (0.75 --encoded), pool share >= 40 % (KS00, KS02)
  8  warm uniqueness (KS19); 9 separation sky - unresolved towers >= 6 codes; 10 highlights p99 / p99.9;
  11 dark cast on luma 12-40: mean (B - G) in -2..+6 (beat 5: <= +6 only, D-041 4), |mean (R - G)| <= 4 (<= 6 in
      beat 1, D-032 4; beat 5 in -8..+4, D-040 5); 12 Heart discipline (KS17, KS18)
  (owner AT since Phase 4 wave A; QA-1: KS07 has no separation box, its frame holds no sky)
Values are 8-bit sRGB codes; luma is Rec.709 on the encoded values (the 14.10 convention). Exit code 1 if any shot fails.
As a run_all step: run(capture_dir) returns {"tool", "status", "fails", "shots"}.
"""
import glob
import json
import os
import sys

import numpy as np
from PIL import Image

FLOOR_LUMA = 11.8                 # Floor #0A0C0F (LOOK_V2 2.1)
KS = {  # id: (mean-luma lo, hi, beat by s) - LOOK_V2 8.1; KS01 20-36 by D-041 (3) (the modelled noodle bar is the subject)
    "KS00": (16, 31, 1), "KS01": (20, 36, 1), "KS02": (22, 40, 1), "KS03": (24, 40, 1), "KS04": (30, 48, 2), "KS05": (34, 54, 2),
    "KS06": (28, 46, 2), "KS07": (26, 40, 3), "KS08": (28, 44, 3), "KS09": (30, 46, 3), "KS10": (32, 48, 3), "KS11": (24, 40, 4),
    "KS12": (30, 46, 4), "KS13": (28, 48, 4), "KS14": (24, 40, 4), "KS15": (24, 42, 5), "KS16": (38, 62, 6), "KS17": (32, 50, 7),
    "KS18": (38, 60, 7), "KS19": (18, 32, 7)}
BEAT_S = (120, 400, 560, 880, 1080, 1260)            # beat starts 2..7
SKY_TOP = {  # id: (x0, x1, luma lo, hi, sat max); band = the top 6 % of the rows
    "KS02": (0.45, 0.53, 50, 71, 0.34), "KS04": (0.3, 0.7, 49, 70, 0.34), "KS05": (0.3, 0.7, 57, 78, 0.34),
    "KS08": (0.3, 0.7, 40, 61, 0.30), "KS09": (0.3, 0.7, 31, 51, 0.30), "KS12": (0.3, 0.7, 32, 52, 0.30),
    "KS17": (0.3, 0.7, 38, 59, 0.30), "KS18": (0.3, 0.7, 38, 59, 0.30)}
TEX_REGIONS = {  # (x0, y0, x1, y1) frame fractions
    "KS00": [(0.02, 0.15, 0.28, 0.70), (0.72, 0.15, 0.98, 0.70)], "KS01": [(0.02, 0.20, 0.30, 0.75)],
    "KS02": [(0.02, 0.40, 0.20, 0.95), (0.80, 0.40, 0.98, 0.95)], "KS19": [(0.05, 0.35, 0.30, 0.95), (0.62, 0.40, 0.95, 0.95)]}
POOL_SHOTS = ("KS00", "KS02")
# KS07 dropped (QA-1, AT 2026-11-16): GE's masses and face B fill the whole upper frame of KS07 (no sky left above the gate), so
# its 0.445-0.555 x 0.01-0.18 "sky" box measured face B against face B (separation 1.1, meaningless); no other sky exists in the shot.
SEP = {  # id: (sky box, unresolved-tower box)
    "KS04": ((0.34, 0.05, 0.45, 0.30), (0.30, 0.42, 0.44, 0.62)), "KS05": ((0.36, 0.20, 0.62, 0.50), (0.15, 0.80, 0.42, 0.95)),
    "KS08": ((0.30, 0.05, 0.60, 0.35), (0.62, 0.45, 0.95, 0.70)),
    "KS17": ((0.03, 0.03, 0.20, 0.20), (0.02, 0.45, 0.20, 0.65)), "KS18": ((0.52, 0.03, 0.82, 0.25), (0.02, 0.55, 0.22, 0.80))}
P999_SHOTS = ("KS04", "KS05", "KS06", "KS09", "KS10", "KS17", "KS18", "KS19")
RG_MAX_BEAT1 = 6          # D-032 (4): beat 1's warm pools are the design (14.5.1): |mean (R - G)| <= 6 there, 4 elsewhere
RG_WIN_BEAT5 = (-8, 4)    # D-040 (5): beat 5's green darks (the garden's identity): R - G in [-8, +4]
BG_MIN_BEAT5 = None       # D-041 (4): beat 5 is exempt from B - G >= -2 (lit leaves: B - G = -chroma for hue < 120); <= 6 stays
GOAL_BOX = (0.33, 0.42, 0.57, 0.72)       # KS19
GOAL_KS17 = (0.43, 0.71, 0.475, 0.76)     # the projected goal window in KS17 (0.452, 0.736)
KOI_BOX = {"KS17": (0.24, 0.15, 0.75, 0.52), "KS18": (0.28, 0.02, 0.78, 0.82), "KS19": (0.75, 0.00, 1.00, 0.30)}
R3_MAX = {1: 0.35, 2: 0.30, 3: 0.30, 4: 0.30, 5: 0.35, 6: 0.40, 7: 0.30}
CRUSH_MAX = {1: 0.50, 2: 0.30, 3: 0.30, 4: 0.30, 5: 0.40, 6: 0.25, 7: 0.40}
MEAN_BEAT = {1: (14, 40), 2: (24, 54), 3: (24, 50), 4: (24, 50), 5: (20, 44), 6: (32, 64), 7: (18, 60)}
# checks that depend on textured surfaces (FA, SU, FO); reported, and listed separately while those packages are open
TEXTURE_CHECKS = ("texture_",)


def hsv(im):
    r, g, b = im[..., 0], im[..., 1], im[..., 2]
    mx, mn = im.max(-1), im.min(-1)
    d = mx - mn
    sat = np.where(mx > 0, d / np.maximum(mx, 1e-6), 0.0)
    dd = np.maximum(d, 1e-6)
    hue = np.where(mx == r, ((g - b) / dd) % 6, np.where(mx == g, (b - r) / dd + 2, (r - g) / dd + 4)) * 60.0
    return np.where(d > 0, hue, 0.0), sat


def luma(im):
    return 0.2126 * im[..., 0] + 0.7152 * im[..., 1] + 0.0722 * im[..., 2]


def blocks(a, k):
    H, W = a.shape[:2]
    h, w = H // k, W // k
    return a[:h * k, :w * k].reshape(h, k, w, k, *a.shape[2:]).mean(axis=(1, 3))


def crop(a, bx):
    H, W = a.shape[:2]
    x0, y0, x1, y1 = bx
    return a[int(y0 * H):int(y1 * H), int(x0 * W):int(x1 * W)]


def box3(a):
    p = np.pad(a, 1, mode='edge')
    return sum(p[i:i + a.shape[0], j:j + a.shape[1]] for i in range(3) for j in range(3)) / 9.0


def largest_component(mask):
    seen = np.zeros_like(mask, bool)
    best = 0
    H, W = mask.shape
    for y in range(H):
        for x in range(W):
            if mask[y, x] and not seen[y, x]:
                st = [(y, x)]
                seen[y, x] = True
                n = 0
                while st:
                    cy, cx = st.pop()
                    n += 1
                    for ny, nx in ((cy + 1, cx), (cy - 1, cx), (cy, cx + 1), (cy, cx - 1)):
                        if 0 <= ny < H and 0 <= nx < W and mask[ny, nx] and not seen[ny, nx]:
                            seen[ny, nx] = True
                            st.append((ny, nx))
                best = max(best, n)
    return best / mask.size


def texture(Yr):
    """micro: median over 32 x 32 tiles of the std of (luma - 9 x 9 box mean); meso: std of 8 x 8 block means minus their
    3 x 3 block box mean. Tiles with mean < Floor + 2 or any pixel >= 200 are skipped."""
    k = 9
    p = k // 2
    c = np.pad(np.pad(Yr, p, mode='edge').cumsum(0).cumsum(1), ((1, 0), (1, 0)))
    hp = Yr - (c[k:, k:] - c[:-k, k:] - c[k:, :-k] + c[:-k, :-k]) / (k * k)
    T = 32
    H, W = Yr.shape
    sd = []
    tot = 0
    for y in range(0, H - T + 1, T):
        for x in range(0, W - T + 1, T):
            tot += 1
            t = Yr[y:y + T, x:x + T]
            if t.mean() >= FLOOR_LUMA + 2 and t.max() < 200:
                sd.append(hp[y:y + T, x:x + T].std())
    B = blocks(Yr, 8)
    meso = float((B - box3(B)).std())
    return (float(np.median(sd)) if sd else 0.0), meso, len(sd) / max(tot, 1)


def check(path, beat_of=None, encoded=False):
    ks = os.path.basename(path)[:4]
    im = np.asarray(Image.open(path).convert('RGB')).astype(np.float64)
    H, W, _ = im.shape
    Y = luma(im)
    hue, sat = hsv(im)
    lo, hi, beat = KS.get(ks, (0, 255, 0))
    beat = beat_of or beat
    C = {}
    # 1 R1 violet
    v = (hue >= 250) & (hue <= 340) & (sat > 0.35)
    a, b = (v & (Y >= 16) & (Y < 120)).mean() * 100, (v & (Y >= 120)).mean() * 100
    C["R1a_violet_air_pct"] = (round(a, 2), bool(a <= 1.0))
    C["R1b_violet_accent_pct"] = (round(b, 2), bool(b <= 1.5))
    # 2 R2 largest saturated region
    B = blocks(im, 32)
    bh, bs = hsv(B)
    bY = luma(B)
    bright = blocks((Y >= 160).astype(float), 32) >= 0.05
    lim = np.where((bh >= 10) & (bh <= 70), 0.65,
                   np.where((beat == 6) | ((beat == 7) & (bh >= 160) & (bh <= 220)), 0.60, 0.45))
    big = ((bs > lim) | ((bh >= 250) & (bh <= 340) & (bs > 0.20))) & (bY >= 16) & ~bright
    lc = largest_component(big) * 100
    C["R2_largest_saturated_region_pct"] = (round(lc, 2), bool(lc <= 5.0))
    # 3 R3 mid-tone saturation
    B4 = blocks(im, 4)
    _, s4 = hsv(B4)
    Y4 = luma(B4)
    mid = (Y4 >= 16) & (Y4 < 160)
    ms = float(s4[mid].mean()) if mid.any() else 0.0
    C["R3_midtone_mean_sat"] = (round(ms, 3), bool(ms <= R3_MAX.get(beat, 0.30)))
    # 4 mean luma. LOOK_V2 8.1's windows describe the 16:9 composition: a wider frame (the _219 framing variant, same vFOV) is
    # measured on its central 16:9 part, which is the 16:9 key shot itself; its extra side strips are framing material (QA fix wave,
    # lead ruling 2026-11-21: KS03 / KS10 at 21:9 read 22.9 / 31.8 over the whole frame against 25.1 / 33.2 at 16:9)
    if W / float(H) > 16.0 / 9.0 + 0.02:
        w169 = int(round(H * 16.0 / 9.0))
        x0 = (W - w169) // 2
        ml = float(Y[:, x0:x0 + w169].mean())
    else:
        ml = float(Y.mean())
    mb = MEAN_BEAT.get(beat, (0, 255))
    C["mean_luma"] = (round(ml, 1), bool(lo <= ml <= hi and mb[0] <= ml <= mb[1]))
    # 5 sky top
    if ks in SKY_TOP:
        x0, x1, slo, shi, smax = SKY_TOP[ks]
        band = crop(im, (x0, 0.0, x1, 0.06))
        _, bsat = hsv(band)
        sl, ss = float(np.median(luma(band))), float(np.median(bsat))
        C["sky_top_luma"] = (round(sl, 1), bool(slo <= sl <= shi))
        C["sky_top_sat"] = (round(ss, 3), bool(ss <= smax))
    # 6 crush
    cr = float((Y4 <= FLOOR_LUMA + 3).mean())
    C["crush_pct"] = (round(cr * 100, 1), bool(cr <= CRUSH_MAX.get(beat, 0.35)))
    p1 = float(np.percentile(Y, 1))
    C["p1_luma"] = (round(p1, 2), bool(p1 >= 9.0))
    # 10 highlights
    p99 = float(np.percentile(Y, 99))
    C["p99_luma"] = (round(p99, 1), bool(p99 >= (90 if beat == 1 or ks == "KS13" else 120)))
    if ks in P999_SHOTS:
        p9 = float(np.percentile(Y, 99.9))
        C["p999_luma"] = (round(p9, 1), bool(p9 >= 200))
    # 11 dark cast
    dk = (Y >= 12) & (Y <= 40)
    if dk.mean() > 0.01:
        bg, rg = float((im[..., 2] - im[..., 1])[dk].mean()), float((im[..., 0] - im[..., 1])[dk].mean())
        bg_lo = BG_MIN_BEAT5 if beat == 5 else -2
        C["dark_cast_BG"] = (round(bg, 2), bool((bg_lo is None or bg_lo <= bg) and bg <= 6))
        ok_rg = (RG_WIN_BEAT5[0] <= rg <= RG_WIN_BEAT5[1]) if beat == 5 else abs(rg) <= (RG_MAX_BEAT1 if beat == 1 else 4)
        C["dark_cast_RG"] = (round(rg, 2), bool(ok_rg))
    # 9 separation
    if ks in SEP:
        sky_, twr = SEP[ks]
        d = float(np.median(crop(Y, sky_)) - np.median(crop(Y, twr)))
        C["separation"] = (round(d, 1), bool(d >= 6.0))
    # 7 texture
    for i, bx in enumerate(TEX_REGIONS.get(ks, [])):
        Yr = crop(Y, bx)
        mi, me, valid = texture(Yr)
        if valid < 0.5:
            C["texture_%d_info_valid" % i] = (round(valid, 2), True)
            continue
        C["texture_%d_micro" % i] = (round(mi, 2), bool(encoded or mi >= 1.4))
        C["texture_%d_meso" % i] = (round(me, 2), bool(me >= (0.75 if encoded else 1.0)))
        if ks in POOL_SHOTS:
            pool = float((Yr >= 25).mean())
            C["texture_%d_pool" % i] = (round(pool, 2), bool(pool >= 0.40))
    # 8 warm uniqueness
    if ks == "KS19":
        warm = (hue >= 22) & (hue <= 50) & (sat >= 0.45) & (Y >= 90)
        x0, y0, x1, y1 = GOAL_BOX
        ys, xs = slice(int(y0 * H), int(y1 * H)), slice(int(x0 * W), int(x1 * W))
        out = warm.copy()
        out[ys, xs] = False
        C["warm_outside_goal_pct"] = (round(float(out.mean() * 100), 3), bool(out.mean() * 100 <= 0.15))
        C["warm_inside_goal_pct"] = (round(float(warm[ys, xs].mean() * 100), 2), bool(warm[ys, xs].mean() * 100 >= 2.0))
        kh, ksat, kY = crop(hue, KOI_BOX[ks]), crop(sat, KOI_BOX[ks]), crop(Y, KOI_BOX[ks])
        n = int(((kh >= 10) & (kh <= 50) & (ksat >= 0.45) & (kY >= 40)).sum())
        C["koi_warm_px"] = (n, bool(n == 0))
    # 12 Heart discipline
    if ks in ("KS17", "KS18"):
        stray = (hue >= 25) & (hue <= 50) & (sat >= 0.45) & (Y >= 90)
        if ks == "KS17":
            g = crop(stray, GOAL_KS17)
            gy = crop(Y, GOAL_KS17)
            C["goal_point_px"] = (int(g.sum()), bool(g.sum() >= 4 and gy.max() >= 200))
            x0, y0, x1, y1 = GOAL_KS17
            stray[int(y0 * H):int(y1 * H), int(x0 * W):int(x1 * W)] = False
        C["warm_stray_pct"] = (round(float(stray.mean() * 100), 3), bool(stray.mean() * 100 <= 0.05))
        kh, ksat, kY = crop(hue, KOI_BOX[ks]), crop(sat, KOI_BOX[ks]), crop(Y, KOI_BOX[ks])
        pm = ((kh <= 40) | (kh >= 350)) & (ksat >= 0.30) & (kY >= 60)
        if pm.sum() >= 200:
            hh = np.where(kh[pm] >= 350, kh[pm] - 360, kh[pm])
            mh, msat = float(np.median(hh)), float(np.median(ksat[pm]))
            C["koi_patch_hue"] = (round(mh, 1), bool(10 <= mh <= 22))
            C["koi_patch_sat"] = (round(msat, 2), bool(msat >= 0.60))
        else:
            C["koi_patch_px"] = (int(pm.sum()), False)
    return {"shot": os.path.basename(path), "beat": int(beat), "checks": C, "pass": all(c[1] for c in C.values())}


def beats_from_report(d):
    try:
        with open(os.path.join(d, "report.json"), encoding="utf-8") as f:
            rep = json.load(f)
        return {s["file"][:4]: 1 + sum(s["s"] >= b for b in BEAT_S) for s in rep.get("shots", []) if "file" in s and "s" in s}
    except Exception:
        return {}


def failed(r):
    return [k + "=" + str(v[0]) for k, v in r["checks"].items() if not v[1]]


def run(capture_dir, encoded=False):
    """run_all entry point: every KS*.png of the folder (FAIL when any shot fails, SKIP when there is none)."""
    shots = sorted(glob.glob(os.path.join(capture_dir, "KS*.png")))
    if not shots:
        return {"tool": "lookv2_check", "status": "SKIP", "fails": [], "shots": []}
    bt = beats_from_report(capture_dir)
    res = [check(p, bt.get(os.path.basename(p)[:4]), encoded) for p in shots]
    fails = ["%s: %s" % (r["shot"], " ".join(failed(r))) for r in res if not r["pass"]]
    return {"tool": "lookv2_check", "status": "FAIL" if fails else "PASS", "fails": fails, "shots": res}


def markdown(res, dirs):
    lines = ["# lookv2_check (docs/LOOK_V2.md 8.2)", "", "Folders: " + ", ".join(dirs), "",
             "| Shot | Beat | Result | Failing checks (value) | Key numbers |", "|---|---|---|---|---|"]
    keys = ("mean_luma", "R1a_violet_air_pct", "R2_largest_saturated_region_pct", "R3_midtone_mean_sat", "sky_top_luma",
            "crush_pct", "p1_luma", "dark_cast_BG", "dark_cast_RG", "separation")
    for r in res:
        kn = ", ".join("%s %s" % (k, r["checks"][k][0]) for k in keys if k in r["checks"])
        lines.append("| %s | %d | %s | %s | %s |" % (r["shot"], r["beat"], "PASS" if r["pass"] else "FAIL",
                                                     " ".join(failed(r)) or "-", kn))
    by = {}
    for r in res:
        for k, v in r["checks"].items():
            base = k if not k.startswith("texture_") else "texture"
            t = by.setdefault(base, [0, 0])
            t[0] += 1
            t[1] += 0 if v[1] else 1
    lines += ["", "| Check | Evaluated | Failing |", "|---|---|---|"]
    lines += ["| %s | %d | %d |" % (k, v[0], v[1]) for k, v in sorted(by.items())]
    return "\n".join(lines) + "\n"


def main(argv):
    args = list(argv)
    out = md = None
    encoded = False
    if "--json" in args:
        i = args.index("--json")
        out = args[i + 1]
        del args[i:i + 2]
    if "--md" in args:
        i = args.index("--md")
        md = args[i + 1]
        del args[i:i + 2]
    if "--encoded" in args:
        encoded = True
        args.remove("--encoded")
    if not args:
        print(__doc__)
        return 2
    res = []
    for d in args:
        res += run(d, encoded)["shots"]
    for r in res:
        print("%-26s b%d %s %s" % (r["shot"], r["beat"], "PASS" if r["pass"] else "FAIL", " ".join(failed(r))))
    npass = sum(r["pass"] for r in res)
    print("lookv2_check: %d / %d shots pass" % (npass, len(res)))
    if out:
        with open(out, "w", encoding="utf-8") as f:
            json.dump(res, f, indent=1)
    if md:
        with open(md, "w", encoding="utf-8") as f:
            f.write(markdown(res, args))
    return 0 if npass == len(res) else 1


if __name__ == "__main__":
    sys.exit(main(sys.argv[1:]))
