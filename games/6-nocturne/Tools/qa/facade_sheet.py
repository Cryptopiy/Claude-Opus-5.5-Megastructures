#!/usr/bin/env python3
"""facade_sheet: measures the Look v2 facade lookdev sheet (docs/TEXTURE_PLAN.md 7.2, owner FA) in the stills of
Scripts/Runtime/Rendering/NocFacadeSheet.cs (-facade-sheet; poses fs3000, fs1000, fs300a-c, fs100_<subject>, fs30_<subject>).

Usage:
  python -I Tools/qa/facade_sheet.py <capture dir> [<capture dir> ...] [--debug6 <dir with the same poses at -debug-view 6>]
         [--encode] [--run <id>] [--out-dir QA/reports]

The sheet's geometry is mirrored here (keep it equal to NocFacadeSheet.cs): towers 30 x 30 m, y 20..220, centres
x = -383 + 54 (3 subject + turn), z 5 600, turns 0 / 45 / 75 deg; poses look north (yaw 0, pitch 0, vFOV 66) from
y 120 at z = 5 585 - d. Every tower face is projected with the pinhole of the pose, so the crops are exact.
Per still and visible frontal (turn 0) tower face, inset 12 % (the corner piers and the parapet stay out):
  - floor: per 86.4 m band, autocorrelation of the row means (luma clipped at the band's 60th percentile, so the lit state
    does not mask the structure; high-passed over one pitch) at the floor pitch in px (the best of the residential 3.6,
    commercial 4.32, mechanical 6.171 and lobby 14.4 m pitches); gate > 0.3 wherever the pitch is >= FLOOR_GATE_PX (6) px and
    incidence <= 50 (D-032 7: the H2 fade blends the floor rows to their band mean below 5.5 px by design, FA-D18 / INT-25),
    on bands tall enough for 4 periods of every candidate pitch (a 47 m top band may be a lobby stratum: marked *, not gated);
  - contrast: p90 - p10 of the column-mean luma (window / pier contrast), codes;
  - micro / meso: Tools/qa/lookv2_check.texture on the crop (9 px high-pass tile median; 8 x 8 block meso), and the same
    after an x264 CRF 23 4:2:0 round trip (--encode, ffmpeg); gate micro >= 1.5 codes at 75-300 m after the encode;
  - mean luma of the face (unresolved at 1 000 / 3 000 m: the per-skin mean against the region-D mean).
Reveal sign (--debug6): debug view 6 writes R = 128 + 64 D kx / W, kx = (d . T) / (d . -n), T the direction in which u
grows (G4Kit.FacadeQuad: the normal turned +90 deg of yaw); per visible face the median sign of R - 128 must equal the
sign of d . T from the geometry (a wrong sigma flips it).
Writes QA/reports/facade_sheet_<run>.json and .md. Exit code 1 if a gate fails.
"""
import argparse
import glob
import json
import math
import os
import subprocess
import sys
import tempfile

_HERE = os.path.dirname(os.path.abspath(__file__))
if _HERE not in sys.path:
    sys.path.insert(0, _HERE)

import numpy as np  # noqa: E402
from PIL import Image  # noqa: E402

import qa_common as qc  # noqa: E402

# ------------------------------------------------------------------ NocFacadeSheet.cs mirror
X0, PITCH, ROWZ, HALF, Y0, Y1, EYEY = -383.0, 54.0, 5600.0, 15.0, 20.0, 220.0, 120.0
TURNS = (0.0, 45.0, 75.0)
SUBJECTS = ["S0", "S1", "S2", "S3", "S4", "O0", "O1", "O2", "O3", "O4", "SEC"]
W, H, VFOV = 1920, 1080, 66.0
FOCAL = 0.5 * H / math.tan(math.radians(0.5 * VFOV))
FLOORS = (3.6, 4.32, 6.171, 14.4)   # residential, commercial, mechanical, sky lobby (one stratum per 86.4 m band)
BAND = 86.4
FLOOR_GATE_PX, FLOOR_GATE_INC, FLOOR_GATE_AC = 6, 50.0, 0.3   # D-032 7 (was 4 px)


def tower_x(k, t):
    return X0 + PITCH * (3 * k + t)


def poses():
    front = ROWZ - HALF
    mid = 0.5 * (tower_x(0, 0) + tower_x(10, 2))
    p = {"fs3000": (mid, front - 3000.0), "fs1000": (mid, front - 1000.0),
         "fs300a": (0.5 * (tower_x(0, 0) + tower_x(3, 2)), front - 300.0),
         "fs300b": (0.5 * (tower_x(4, 0) + tower_x(7, 2)), front - 300.0),
         "fs300c": (0.5 * (tower_x(8, 0) + tower_x(10, 2)), front - 300.0)}
    for k, s in enumerate(SUBJECTS):
        p["fs100_" + s] = (tower_x(k, 1), front - 100.0)
        p["fs30_" + s] = (tower_x(k, 0), front - 30.0)
    return {k: (round(v[0]), EYEY, round(v[1])) for k, v in p.items()}


def faces(k, t):
    """The four faces of tower (k, t): (corner a, corner b, outward normal, u direction) in plan (x, z)."""
    th = math.radians(TURNS[t])
    c, s = math.cos(th), math.sin(th)
    cx, cz = tower_x(k, t), ROWZ

    def P(lx, lz):
        return (cx + lx * c + lz * s, cz - lx * s + lz * c)

    def N(lx, lz):
        return (lx * c + lz * s, -lx * s + lz * c)

    h = HALF
    out = []
    for a, b, n in (((-h, -h), (h, -h), (0, -1)), ((h, -h), (h, h), (1, 0)), ((h, h), (-h, h), (0, 1)), ((-h, h), (-h, -h), (-1, 0))):
        nn = N(*n)
        yaw = math.atan2(nn[0], nn[1]) + math.radians(90.0)   # u direction: the normal turned +90 deg of yaw
        out.append((P(*a), P(*b), nn, (math.sin(yaw), math.cos(yaw))))
    return out


def project(cam, x, y, z):
    dz = z - cam[2]
    if dz <= 1.0:
        return None
    return (0.5 * W + FOCAL * (x - cam[0]) / dz, 0.5 * H - FOCAL * (y - cam[1]) / dz)


def face_quad(cam, a, b, y0=Y0, y1=Y1):
    pts = [project(cam, a[0], y0, a[1]), project(cam, a[0], y1, a[1]), project(cam, b[0], y1, b[1]), project(cam, b[0], y0, b[1])]
    return None if any(p is None for p in pts) else pts


def visible(cam, a, b, n):
    mx, mz = 0.5 * (a[0] + b[0]), 0.5 * (a[1] + b[1])
    return (cam[0] - mx) * n[0] + (cam[2] - mz) * n[1] > 0.0


def crop_box(quad, inset=0.12, insetY=None):
    xs = [p[0] for p in quad]
    ys = [p[1] for p in quad]
    x0, x1, y0, y1 = max(min(xs), 0), min(max(xs), W - 1), max(min(ys), 0), min(max(ys), H - 1)
    dx, dy = (x1 - x0) * inset, (y1 - y0) * (inset if insetY is None else insetY)
    return int(round(x0 + dx)), int(round(y0 + dy)), int(round(x1 - dx)), int(round(y1 - dy))


def bilinear(Y, xs, ys):
    h, w = Y.shape
    xs = np.clip(xs - 0.5, 0.0, w - 1.001)
    ys = np.clip(ys - 0.5, 0.0, h - 1.001)
    x0, y0 = np.floor(xs).astype(int), np.floor(ys).astype(int)
    fx, fy = xs - x0, ys - y0
    return (Y[y0, x0] * (1 - fx) * (1 - fy) + Y[y0, x0 + 1] * fx * (1 - fy) + Y[y0 + 1, x0] * (1 - fx) * fy + Y[y0 + 1, x0 + 1] * fx * fy)


def band_rows(Y, cam, a, b, ya, yb, inset=0.12):
    """Row means of the face band [ya, yb] resampled along the face (s in [inset, 1 - inset] of a -> b, y rows), luma clipped at the
    band's 60th percentile (lit windows clipped: the structure, not the lit state); one row per px of the face centre's depth.
    Returns (row means, metres per row, the far end's depth) or (None, ...) when the band is off screen or too small."""
    s = np.linspace(inset, 1.0 - inset, 1)
    za, zb = a[1] - cam[2], b[1] - cam[2]
    if min(za, zb) <= 1.0:
        return None, None, None
    zc = 0.5 * (za + zb)
    mpp = zc / FOCAL
    ny = int((yb - ya) / mpp)
    wpx = abs(FOCAL * (b[0] - cam[0]) / zb - FOCAL * (a[0] - cam[0]) / za) * (1.0 - 2.0 * inset)
    ns = max(3, int(wpx))
    if ny < 8 or wpx < 3:
        return None, None, None
    s = np.linspace(inset, 1.0 - inset, ns)
    yv = yb - (np.arange(ny) + 0.5) * mpp                                     # top to bottom, as image rows
    X = a[0] + (b[0] - a[0]) * s
    Z = a[1] + (b[1] - a[1]) * s
    xs = 0.5 * W + FOCAL * (X[None, :] - cam[0]) / (Z[None, :] - cam[2])
    ys = 0.5 * H - FOCAL * (yv[:, None] - cam[1]) / (Z[None, :] - cam[2])
    xs = np.broadcast_to(xs, ys.shape)
    inside = (xs >= 0) & (xs < W) & (ys >= 0) & (ys < H)
    if inside.mean() < 0.9:
        return None, None, None
    v = bilinear(Y, xs, ys)
    v = np.minimum(v, np.percentile(v, 60))
    return v.mean(axis=1), mpp, max(za, zb)


# ------------------------------------------------------------------ metrics
def autocorr(prof, lag):
    p = np.asarray(prof, dtype=np.float64)
    if lag < 2 or len(p) < 4 * lag + 2:
        return None
    # high-pass: minus the box mean over one pitch (removes macro bands such as the mechanical stripes and lit floor groups)
    k = np.ones(lag) / lag
    p = (p - np.convolve(np.pad(p, (lag // 2, lag - 1 - lag // 2), mode='edge'), k, mode='valid'))[lag:-lag]
    if len(p) < lag + 2:
        return None
    a, b = p[:-lag], p[lag:]
    den = math.sqrt(float((a * a).sum() * (b * b).sum()))
    return float((a * b).sum() / den) if den > 1e-9 else None


def texture(Yr):
    try:
        import lookv2_check as lc
        return lc.texture(Yr)
    except Exception:  # pragma: no cover
        return (float("nan"), float("nan"), 0.0)


def find_ffmpeg():
    try:
        import encode_test as et
        return et.find_ffmpeg(), et.VF_ENC, et.VF_DEC, et.TAGS
    except Exception:  # pragma: no cover
        return None, None, None, None


def x264_roundtrip(png, work):
    ff, venc, vdec, tags = find_ffmpeg()
    if not ff:
        return None
    mp4, out = os.path.join(work, "rt.mp4"), os.path.join(work, "rt.png")
    for f in (mp4, out):
        if os.path.exists(f):
            os.remove(f)
    enc = [ff, "-y", "-loglevel", "error", "-i", png, "-vf", venc, "-c:v", "libx264", "-preset", "medium", "-crf", "23"] + tags + [mp4]
    dec = [ff, "-y", "-loglevel", "error", "-i", mp4, "-vf", vdec, "-frames:v", "1", out]
    if subprocess.call(enc) != 0 or subprocess.call(dec) != 0:
        return None
    return qc.load_rgb8(out)[0]


def measure(path, name, cam, encode, work, dbg):
    rgb = qc.load_rgb8(path)[0]
    Y = qc.luma_display(rgb).astype(np.float64) * 255.0   # display luma codes
    dec = x264_roundtrip(path, work) if encode else None
    Yd = qc.luma_display(dec).astype(np.float64) * 255.0 if dec is not None else None
    D = None
    if dbg:
        cands = glob.glob(os.path.join(dbg, "**", os.path.basename(path)), recursive=True)
        if cands:
            D = qc.load_rgb8(cands[0])[0].astype(np.float64)
    rows = []
    for k, subj in enumerate(SUBJECTS):
        for t in range(3):
            for fi, (a, b, n, udir) in enumerate(faces(k, t)):
                if not visible(cam, a, b, n):
                    continue
                q = face_quad(cam, a, b)
                if q is None:
                    continue
                x0, y0, x1, y1 = crop_box(q)
                if x1 - x0 < 3 or y1 - y0 < 3 or x1 <= 0 or y1 <= 0 or x0 >= W or y0 >= H:
                    continue
                mx, mz = 0.5 * (a[0] + b[0]), 0.5 * (a[1] + b[1])
                dist = math.hypot(mx - cam[0], mz - cam[2])
                d = (mx - cam[0], mz - cam[2])
                dl = math.hypot(*d)
                inc = math.degrees(math.acos(max(-1.0, min(1.0, -(d[0] * n[0] + d[1] * n[1]) / dl))))
                r = {"pose": name, "subject": subj, "turn": TURNS[t], "face": fi, "dist": round(dist, 1), "incidence": round(inc, 1),
                     "crop": [x0, y0, x1, y1]}
                c = Y[y0:y1, x0:x1]
                r["mean"] = round(float(c.mean()), 2)
                colp, rowp = c.mean(axis=0), c.mean(axis=1)
                r["contrast"] = round(float(np.percentile(colp, 90) - np.percentile(colp, 10)), 2)
                # floor lines per 86.4 m band (one stratum each): the band is resampled in face coordinates (rows along the projected
                # floor lines: on a turned face the lines converge, so image rows would mix floors), one row per px at the face
                # centre; the best pitch of the four strata; the gate px is the smallest candidate pitch at the far end of the face
                fl_rows = []
                for bnd in range(int(Y0 // BAND), int(Y1 // BAND) + 1):
                    ya, yb = max(Y0, bnd * BAND), min(Y1, (bnd + 1) * BAND)
                    if yb - ya < 20.0:
                        continue
                    rp, mpp, zfar = band_rows(Y, cam, a, b, ya + 0.03 * (yb - ya), yb - 0.03 * (yb - ya))
                    if rp is None:
                        continue
                    best, full = None, True
                    for fl in FLOORS:
                        lag = int(round(fl / mpp))
                        ac = autocorr(rp, lag)
                        full = full and ac is not None
                        if ac is not None and (best is None or ac > best[1]):
                            best = (fl, ac, lag)
                    if best:
                        gpx = int(math.floor(min(FLOORS) * FOCAL / zfar))   # every candidate stratum's floors are at least this many px
                        # full: every candidate pitch fits 4 periods in the band; a 47 m top band of a lobby stratum (14.4 m floors)
                        # cannot be measured at its own pitch, and the best of the other three is then the wrong pitch (not gated)
                        fl_rows.append([bnd, best[0], round(best[1], 3), best[2], gpx, int(full)])
                if fl_rows:
                    r["floor"] = fl_rows
                if c.shape[0] >= 32 and c.shape[1] >= 32:
                    mi, me, valid = texture(c)
                    r["micro"], r["meso"] = round(mi, 2), round(me, 2)
                    if Yd is not None:
                        mi2, me2, _ = texture(Yd[y0:y1, x0:x1])
                        r["micro_enc"], r["meso_enc"] = round(mi2, 2), round(me2, 2)
                if D is not None:
                    R = D[y0:y1, x0:x1, 0] - 128.0
                    s_img = float(np.median(np.sign(R[np.abs(R) >= 1.0]))) if np.any(np.abs(R) >= 1.0) else 0.0
                    s_geo = 1.0 if (d[0] * udir[0] + d[1] * udir[1]) > 0 else -1.0
                    r["reveal_sign"], r["reveal_expect"] = s_img, s_geo
                rows.append(r)
    return rows


def main(argv=None):
    ap = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    ap.add_argument("dirs", nargs="+")
    ap.add_argument("--debug6", default=None)
    ap.add_argument("--encode", action="store_true")
    ap.add_argument("--run", default=None)
    ap.add_argument("--out-dir", default=os.path.join("QA", "reports"))
    a = ap.parse_args(argv)
    P = poses()
    rows = []
    work = tempfile.mkdtemp(prefix="facade_sheet_")
    for d in a.dirs:
        for path in sorted(glob.glob(os.path.join(d, "**", "*.png"), recursive=True)):
            base = os.path.splitext(os.path.basename(path))[0]
            name = base[5:] if base.startswith("pose_") else base
            if name not in P:
                continue
            rows += measure(path, name, P[name], a.encode, work, a.debug6)
    run = a.run or os.path.basename(os.path.normpath(a.dirs[0]))
    fails = []
    for r in rows:
        for bnd, fl, ac, px, gpx, full in r.get("floor", []):
            if full and gpx >= FLOOR_GATE_PX and r["incidence"] <= FLOOR_GATE_INC and ac <= FLOOR_GATE_AC:
                fails.append("floor %s %s %.0f face %d band %d: ac %.3f at %d px (%.2f m; floors >= %d px)" % (r["pose"], r["subject"], r["turn"], r["face"], bnd, ac, px, fl, gpx))
        if 75 <= r["dist"] <= 300 and "micro_enc" in r and r["micro_enc"] < 1.5:
            fails.append("texture %s %s %.0f: micro after x264 %.2f" % (r["pose"], r["subject"], r["turn"], r["micro_enc"]))
        if "reveal_sign" in r and r["reveal_sign"] != 0.0 and r["reveal_sign"] != r["reveal_expect"]:
            fails.append("reveal sign %s %s %.0f face %d: image %+.0f, geometry %+.0f" % (r["pose"], r["subject"], r["turn"], r["face"], r["reveal_sign"], r["reveal_expect"]))
    skin = {}
    for r in rows:
        if r["pose"] in ("fs1000", "fs3000") and r["turn"] == 0.0 and r["face"] == 0:
            skin.setdefault(r["pose"], {})[r["subject"]] = r["mean"]
    out = {"run": run, "faces": rows, "skin_means": skin, "fails": fails}
    qc.ensure_dir(a.out_dir)
    qc.write_json(os.path.join(a.out_dir, "facade_sheet_%s.json" % run), out)
    hdr = ["pose", "subject", "turn", "face", "dist", "inc", "mean", "contrast", "floor ac per band (@px)", "micro", "micro enc", "reveal"]
    tab = [[r["pose"], r["subject"], "%.0f" % r["turn"], r["face"], r["dist"], r["incidence"], r["mean"], r["contrast"],
            " ".join("b%d %.2f@%d%s" % (f[0], f[2], f[3], "" if f[5] else "*") for f in r.get("floor", [])) or "-", r.get("micro", "-"), r.get("micro_enc", "-"),
            ("%+.0f/%+.0f" % (r["reveal_sign"], r["reveal_expect"])) if "reveal_sign" in r else "-"] for r in rows]
    md = ["# facade_sheet %s" % run, "", "%d faces, %d gate failures" % (len(rows), len(fails)), ""]
    md += ["- " + f for f in fails] + [""]
    md += ["Skin means (frontal face, display luma): " + json.dumps(skin), ""]
    md.append(qc.md_table(hdr, tab))
    qc.write_text(os.path.join(a.out_dir, "facade_sheet_%s.md" % run), "\n".join(md) + "\n")
    print("facade_sheet: %d faces, %d fails -> %s" % (len(rows), len(fails), os.path.join(a.out_dir, "facade_sheet_%s.md" % run)))
    for f in fails[:20]:
        print("  FAIL " + f)
    return 1 if fails else 0


if __name__ == "__main__":
    sys.exit(main())
