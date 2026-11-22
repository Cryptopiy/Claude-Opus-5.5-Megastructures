#!/usr/bin/env python3
"""sequences: frame-sequence tests of Nocturne captures (MASTER_PROMPT 11.6, 14.10 Sequences, 14.6, 14.12 -sequence).

Usage:
  python -I Tools/qa/sequences.py QA/captures/<run-id>            (every seq_* folder inside)
  python -I Tools/qa/sequences.py QA/captures/<run-id>/seq_<name> [more folders] [--run <id>] [--out-dir QA/reports]

Per sequence (frame_NNNNN.png + sequence.json written by the CaptureHarness, see qa_common): the mean absolute difference
between consecutive frames in 8-bit grey levels (Rec.709 display luma x 255) and its per-pixel maximum, the frame-mean
luminance series (display 0..1), and the 14.10 checks by kind:
  - every kind except "edge": mean-luma step <= 1.2 levels per frame (sequence.json "lumaStepMax" overrides);
    no difference spike above 3 x the running median (31-frame window, floor 0.25 grey levels; window and sign pops);
  - "boundary" (-sequence boundaries): activation-frame difference <= 0.5 level above the centred running median of the
    surrounding frames (31-frame window, the frame itself excluded; QA-7: a median of the previous frames alone lags a smooth
    trend such as the C3 turn and read it as an activation) at every frame whose s crosses a beat boundary ("boundaryS",
    per-frame "s");
  - "static" (-sequence statics, run with -freeze-clock -rain 0): change <= 2 levels outside moving-object masks: the
    per-pixel maximum |frame - frame 0| (sequence.json "mask" rects excluded);
  - "edge" (start / end, defect 22): only black / white flashes are reported, the fades are intended;
  - "texpan" (-capture-set texpans, TEXTURE_PLAN 7.5 / R1): frames resampled into the middle frame by the exact rotation
    homography (per-frame yaw / pitch / fov of sequence.json); swimming = shift of textured 64 px blocks between the first and the
    last resampled frame (p95 <= 0.5 px, max <= 1.0 WARN); the stabilised residual std (INFO); with --reference <the same set
    captured with -capture-ss 2> the R1 / 14.10 gates: screen-space temporal std minus the reference's and the stabilised
    residual minus the reference's, worst 32 px block <= 2.0 levels each;
  - "dolly" (-capture-set dolly, R2): the luma-step and spike checks above, plus local pops of 64 px blocks (WARN) with the
    camera distance of the frame ("dist");
  - "rain" (-sequence rain: static camera, rho 1, clock running; 14.7 H5): streak pixels only ever ADD light, so the
    per-pixel temporal minimum of the integer luma is the rain-free background; the streak mask of a frame = pixels more
    than 1.5 codes above it, dilated 2 px; outside the masks of two consecutive frames the per-pixel change (max over
    R, G, B) must stay <= 6 levels (flicker of the background under the rain); coverage and p99.9 are reported (a
    brightening is inside the mask by construction: the luma-step and spike checks see it);
  - thin lines ("lines"): centroid jitter below 0.1 px and frame-to-frame energy variation below 10 percent;
  - "stability" crops: no pixel deviates more than 12 percent from the mean of 5 consecutive frames.
QA fix wave (Logs/fix/QA.md C20-C24, Tools/qa/seq_extra.py): kinds texpan / dolly / static / pan are refused unless captured with
-rain 0 -freeze-clock; "pan" gets the texpan shimmer computation; one-frame block pops and blinks (32 / 64 px) on the boundary,
pan, cinematic, dread and signs kinds (WARN on koi / traffic / encode; a blink by the temporal out-and-back rule or AL's sign
rule); the photosensitivity gates G1-G7 (R4 3.3, D-010; G1 per D-041 (1): a flash is a pair of opposing changes >= 0.1 with the
darker state < 0.8 inside a 1/3 s window, the R4 1 s pair area gated on a static camera only) on the signs, dread and cinematic
kinds; the H5 rain image gate (streak count, coverage, minimum length); a montage per sequence (and
the worst pop blocks at 4 x, an x264 clip for boundary / cinematic / dread) written into the sequence folder, then with
--delete-frames the frames are deleted and an analysed.done marker written. A sequence whose required gate could not measure
(texpan / pan without the -capture-ss 2 reference, signs / dread without photosensitivity, rain without -rain-noise-off) reads
SKIP, never PASS; the report's Result is PASS only when nothing FAILed and nothing required was skipped.
Writes QA/reports/sequences_<run>.json and .md.  Exit code 1 if any check FAILs.
"""
import argparse
import math
import os
import sys

_HERE = os.path.dirname(os.path.abspath(__file__))
if _HERE not in sys.path:
    sys.path.insert(0, _HERE)

import numpy as np  # noqa: E402

import qa_common as qc  # noqa: E402
from qa_common import PASS, FAIL, SKIP, INFO, WARN, TH, check  # noqa: E402
import seq_extra as sx  # noqa: E402

# a sequence of these kinds reads SKIP (not PASS) when the named gate did not measure (QA fix wave: no gate passes unmeasured)
REQUIRED = {"texpan": ("shimmer",), "pan": ("shimmer",), "signs": ("G1",), "dread": ("G1",), "rain": ("rain H5 streak count",)}

_BLOCKS = u"▁▂▃▄▅▆▇█"


def sparkline(v, width=60):
    v = np.asarray([x for x in v if x is not None and np.isfinite(x)], dtype=np.float64)
    if v.size == 0:
        return ""
    if v.size > width:
        edges = np.linspace(0, v.size, width + 1).astype(int)
        v = np.array([v[a:b].max() for a, b in zip(edges[:-1], edges[1:]) if b > a])
    lo, hi = v.min(), v.max()
    if hi - lo < 1e-12:
        return _BLOCKS[0] * len(v)
    return "".join(_BLOCKS[int(round((x - lo) / (hi - lo) * 7))] for x in v)


def running_median(x, win):
    n = len(x)
    h = win // 2
    out = np.empty(n)
    for i in range(n):
        out[i] = np.median(x[max(0, i - h):min(n, i + h + 1)])
    return out


def _luma_int(rgb8):
    """Integer Rec.709 display luma in codes: (2126 R + 7152 G + 722 B) // 10000 (the H5 rain gate's measure)."""
    return (rgb8.astype(np.int32) @ np.array([2126, 7152, 722], dtype=np.int32)) // 10000


def _dilate(m, r):
    """Binary dilation by a (2r + 1) square, without wrap-around at the frame edges."""
    if r <= 0:
        return m.copy()
    H, W = m.shape
    P = np.pad(m, r, mode="constant", constant_values=False)
    out = np.zeros((H, W), dtype=bool)
    for dy in range(-r, r + 1):
        for dx in range(-r, r + 1):
            out |= P[r + dy:r + dy + H, r + dx:r + dx + W]
    return out


def rain_flicker(frames, base, th=TH):
    """14.7 H5 flicker gate of a static-camera rain sequence (second pass over the frames). base = per-pixel temporal
    minimum of the integer luma (the rain-free background: streaks only add light). Streak mask of frame k =
    (Y_k - base) > rain_streak_codes; for every consecutive pair the masks are OR-ed and dilated rain_mask_dilate_px, and the
    per-pixel change max(|RGB_k - RGB_k-1|) is measured outside. Returns the worst change, the mean p99.9 and the mean
    mask coverage (percent)."""
    codes, r = float(th["rain_streak_codes"]), int(th["rain_mask_dilate_px"])
    prev_rgb = prev_mask = None
    worst, p999, cov = 0, [], []
    for idx, path in frames:
        try:
            rgb8, _ = qc.load_rgb8(path)
        except Exception:
            prev_rgb = prev_mask = None
            continue
        if rgb8.shape[:2] != base.shape:
            prev_rgb = prev_mask = None
            continue
        a = rgb8.astype(np.int16)
        mask = (_luma_int(rgb8) - base) > codes
        cov.append(float(mask.mean()))
        if prev_rgb is not None:
            m = _dilate(mask | prev_mask, r)
            d = np.abs(a - prev_rgb).max(axis=2)[~m]
            if d.size:
                worst = max(worst, int(d.max()))
                p999.append(float(np.percentile(d, 99.9)))
        prev_rgb, prev_mask = a, mask
    return {"outside_change_max": worst, "outside_p999_mean": float(np.mean(p999)) if p999 else 0.0,
            "mask_coverage_pct": 100.0 * float(np.mean(cov)) if cov else 0.0, "pairs": len(p999)}


def _kind(seq):
    m = seq["meta"]
    k = (m.get("kind") or "").lower()
    if k:
        return k
    n = seq["name"].lower()
    for key in ("texpan", "dolly", "static", "pan", "encode", "signs", "rain", "koi", "traffic", "dread", "cin"):
        if key in n:
            return "cinematic" if key == "cin" else key
    if n in ("start", "end"):
        return "edge"
    return "boundary"


def _frame_s(meta):
    """Per-frame s from sequence.json ("frames": [{"s": ...}] or "s": [...]), else None."""
    fr = meta.get("frames")
    if isinstance(fr, list) and fr and isinstance(fr[0], dict) and "s" in fr[0]:
        return [f.get("s") for f in fr]
    s = meta.get("s")
    return s if isinstance(s, list) else None


def _resolve_item(it, W, H):
    it = dict(it)
    it.setdefault("_ref", it.get("ref"))
    it.setdefault("_units", it.get("units", None if it.get("ref") else "px"))
    return it


# ------------------------------------------------------------------------------------------ texpan / dolly (TQ)
# TEXTURE_PLAN 7.5 and 10 R1 / R2. A pan is a pure camera rotation, so every world-locked pixel moves by the homography
# K R_k R_r^-1 K^-1 between frames: frames are resampled into the middle frame's grid with that exact mapping (Keys cubic),
# then
#   - stabilised residual: per-pixel temporal std of the resampled stack (shimmer of the content itself; the cubic's own
#     phase-dependent smoothing is part of it, so it is compared with the -capture-ss 2 run of the same set);
#   - swimming: sub-pixel shift (half-whitened correlation of 64 px blocks with 2-D texture, after a 5 x 5 box that removes the
#     screen-locked dither) between the first and the last resampled frame: world-locked content gives 0; a texture moving in
#     screen or view space keeps up to the whole pan (15 px). Blocks of one-directional lines are skipped (aperture problem: their
#     stair-step crawl reads as a shift along them; the shimmer gate sees that crawl).
# With --reference <ss2 capture>: the 14.10 / R1 gate, the per-pixel screen-space temporal std minus the reference's, as 32 x 32
# block means: worst block <= 2.0 levels (shimmer_std_max), and the same on the stabilised residual (a 15 px pan gives textured
# pixels a screen-space std of 10-15 levels from the motion alone, which hides a 6-level flicker; the stabilised one does not).
# dolly (R2): the generic luma-step and spike checks of every sequence, plus local pops: per 64 x 64 block a mean abs frame
# difference of more than 2 levels and 3 x the block's differences of both neighbouring frames, with the camera distance.
SWIM_P95_MAX, SWIM_MAX_WARN = 0.5, 1.0         # px
SWIM_BLOCK, SWIM_TEX_MIN = 64, 2.5             # block size, high-pass std (levels) of a textured block
SWIM_BLUR = 5                                  # box size that removes the dither before the shift estimate
SWIM_2D_MIN = 0.15                             # structure-tensor eigenvalue ratio of a block with 2-D texture (aperture problem)
SHIMMER_BLOCK = 32
# a block whose temporal mean differs from the reference's by more than this (levels, or 5 % of it) shows a different picture
# at 2x: a footprint regime of the shader switched (NocNeon glyph strokes resolved at 2x, their flat mean at 1x) or HDR-bright
# sub-pixel detail was averaged after the tonemap (the -capture-ss 2 box runs after the grade, like a 4K master seen at 1080p).
# Such blocks are reported apart and left out of the shimmer gates (their std difference is a picture difference, not shimmer).
REF_MISMATCH_LEVELS, REF_MISMATCH_FRAC = 2.0, 0.05
POP_BLOCK, POP_RATIO, POP_FLOOR = 64, 3.0, 2.0


def _cubic_weights(t):
    t2, t3 = t * t, t * t * t
    return (-0.5 * t3 + t2 - 0.5 * t, 1.5 * t3 - 2.5 * t2 + 1.0, -1.5 * t3 + 2.0 * t2 + 0.5 * t, 0.5 * t3 - 0.5 * t2)


def sample_cubic(img, xs, ys):
    """Keys (a = -0.5) bicubic sample of img at array coordinates (pixel centres on integers), edge-clamped."""
    H, W = img.shape
    x0, y0 = np.floor(xs).astype(np.int64), np.floor(ys).astype(np.int64)
    wx, wy = _cubic_weights((xs - x0).astype(np.float32)), _cubic_weights((ys - y0).astype(np.float32))
    out = np.zeros(xs.shape, dtype=np.float32)
    for j in range(4):
        yy = np.clip(y0 + j - 1, 0, H - 1)
        row = np.zeros(xs.shape, dtype=np.float32)
        for i in range(4):
            row += wx[i] * img[yy, np.clip(x0 + i - 1, 0, W - 1)]
        out += wy[j] * row
    return out


def _frame_cams(meta, W, H, n):
    fr = meta.get("frames") if isinstance(meta.get("frames"), list) else []
    cams = []
    for i in range(n):
        f = fr[i] if i < len(fr) and isinstance(fr[i], dict) else {}
        if f.get("yaw") is None or f.get("fov") is None:
            return None
        cams.append(qc.Cam(W, H, f["fov"], f["yaw"], f.get("pitch", 0.0)))
    return cams


def _ref_rays(cam):
    """World directions of the reference frame's pixel centres (H x W x 3, float32)."""
    u, v = np.meshgrid(np.arange(cam.W, dtype=np.float64) + 0.5, np.arange(cam.H, dtype=np.float64) + 0.5)
    d = cam.cam_dir_px(u, v)
    fwd, right, up = cam.basis()
    return (d[..., 0:1] * right + d[..., 1:2] * up + d[..., 2:3] * fwd).astype(np.float32)


def _map_into(rays, cam):
    """Array coordinates (x, y) in frame `cam` of the reference rays, and the in-frame mask (2 px margin for the cubic)."""
    fwd, right, up = cam.basis()
    X, Y, Z = rays @ right.astype(np.float32), rays @ up.astype(np.float32), rays @ fwd.astype(np.float32)
    Z = np.maximum(Z, 1e-6)
    xs = cam.W * 0.5 + cam.f * X / Z - 0.5
    ys = cam.H * 0.5 - cam.f * Y / Z - 0.5
    ok = (xs >= 1.0) & (xs <= cam.W - 3.0) & (ys >= 1.0) & (ys <= cam.H - 3.0)
    return xs, ys, ok


def _block_means(a, valid, k):
    H, W = a.shape
    h, w = H // k, W // k
    s = np.where(valid, a, 0.0)[:h * k, :w * k].reshape(h, k, w, k).sum(axis=(1, 3))
    n = valid[:h * k, :w * k].reshape(h, k, w, k).sum(axis=(1, 3))
    return np.where(n >= 0.5 * k * k, s / np.maximum(n, 1), np.nan)


def _phase_shift(a, b):
    """Sub-pixel shift of b against a: half-whitened cross-correlation (the cross spectrum divided by the square root of its
    magnitude: full phase correlation lets the 8-bit quantisation noise of the many empty frequency bins of a narrow-band
    texture bury the peak), Hann window, parabolic peak; returns (dx, dy, normalised peak)."""
    h, w = a.shape
    win = np.outer(np.hanning(h), np.hanning(w))
    A, B = np.fft.fft2((a - a.mean()) * win), np.fft.fft2((b - b.mean()) * win)
    Rr = A * np.conj(B)
    Rr /= np.sqrt(np.maximum(np.abs(Rr), 1e-9))
    r = np.real(np.fft.ifft2(Rr))
    r /= max(float(np.abs(r).max()), 1e-12)
    iy, ix = np.unravel_index(int(np.argmax(r)), r.shape)

    def sub(m1, c, p1):
        den = m1 - 2 * c + p1
        return 0.0 if abs(den) < 1e-12 else 0.5 * (m1 - p1) / den
    dy = iy + sub(r[(iy - 1) % h, ix], r[iy, ix], r[(iy + 1) % h, ix])
    dx = ix + sub(r[iy, (ix - 1) % w], r[iy, ix], r[iy, (ix + 1) % w])
    if dy > h / 2:
        dy -= h
    if dx > w / 2:
        dx -= w
    return -dx, -dy, float(r[iy, ix])


def _box(a, k):
    p = k // 2
    c = np.pad(np.pad(a.astype(np.float64), p, mode="edge").cumsum(0).cumsum(1), ((1, 0), (1, 0)))
    return ((c[k:, k:] - c[:-k, k:] - c[k:, :-k] + c[:-k, :-k]) / float(k * k)).astype(np.float32)


def texpan_analysis(seq, quiet=False):
    """Stabilised residual std map, screen-space temporal std map and the swimming shifts of a pan (see above)."""
    frames, meta = seq["frames"], seq["meta"]
    n = len(frames)
    g0, _ = qc.load_rgb8(frames[0][1])
    H, W = g0.shape[:2]
    cams = _frame_cams(meta, W, H, n)
    out = {"frames": n}
    mean_s = np.zeros((H, W), np.float64)
    m2_s = np.zeros((H, W), np.float64)
    mean_w = np.zeros((H, W), np.float64)
    m2_w = np.zeros((H, W), np.float64)
    valid = np.ones((H, W), bool)
    first = last = None
    rays = _ref_rays(cams[n // 2]) if cams is not None else None
    for k, (idx, path) in enumerate(frames):
        g = qc.grey_levels(qc.load_rgb8(path)[0]).astype(np.float32)
        d = g - mean_s
        mean_s += d / (k + 1)
        m2_s += d * (g - mean_s)
        if rays is not None:
            xs, ys, ok = _map_into(rays, cams[k])
            wv = sample_cubic(g, xs, ys)
            valid &= ok
            d = wv - mean_w
            mean_w += d / (k + 1)
            m2_w += d * (wv - mean_w)
            if k == 0:
                first = wv
            last = wv
        if not quiet and k % 10 == 0:
            print("[sequences] %s texpan frame %d/%d" % (seq["name"], k + 1, n), flush=True)
    out["std_screen"] = np.sqrt(m2_s / max(n - 1, 1)).astype(np.float32)
    out["mean_screen"] = mean_s.astype(np.float32)
    if rays is not None:
        out["std_stab"] = np.sqrt(m2_w / max(n - 1, 1)).astype(np.float32)
        out["valid"] = valid
        sv = out["std_stab"][valid]
        out["stab_mean"] = float(sv.mean()) if sv.size else None
        bm = _block_means(out["std_stab"], valid, SHIMMER_BLOCK)
        out["stab_block_p99"] = float(np.nanpercentile(bm, 99)) if np.isfinite(bm).any() else None
        # swimming: textured 64 px blocks, first vs last stabilised frame
        shifts = []
        # the static blue-noise dither is screen-locked (it moves by the whole pan against the scene): a 5 x 5 box takes it out
        # (TQ_r03: dark blocks with a few window slits read the dither's 15 px, not the slits' 0 px)
        first, last = _box(first, SWIM_BLUR), _box(last, SWIM_BLUR)
        hpf, hpl = first - _box(first, 9), last - _box(last, 9)
        B = SWIM_BLOCK
        for by in range(0, H - B + 1, B):
            for bx in range(0, W - B + 1, B):
                if not valid[by:by + B, bx:bx + B].all():
                    continue
                if hpf[by:by + B, bx:bx + B].std() < SWIM_TEX_MIN or hpl[by:by + B, bx:bx + B].std() < SWIM_TEX_MIN:
                    continue
                # a block of lines in one direction (a bridge strip, slanted window rows) cannot tell a shift along them from
                # the crawl of its own stair-steps (the aperture problem; TQ_r03 KS09 / KS17): 2-D texture only
                gy, gx = np.gradient(first[by:by + B, bx:bx + B])
                jxx, jyy, jxy = float((gx * gx).sum()), float((gy * gy).sum()), float((gx * gy).sum())
                tr, det = jxx + jyy, jxx * jyy - jxy * jxy
                disc = math.sqrt(max(tr * tr / 4.0 - det, 0.0))
                if tr <= 0 or (tr / 2.0 - disc) / max(tr / 2.0 + disc, 1e-9) < SWIM_2D_MIN:
                    continue
                dx, dy, pk = _phase_shift(first[by:by + B, bx:bx + B], last[by:by + B, bx:bx + B])
                shifts.append((bx, by, float(np.hypot(dx, dy)), dx, dy, pk))
        out["swim_blocks"] = len(shifts)
        if shifts:
            mag = np.array([s[2] for s in shifts])
            out["swim_p95"] = float(np.percentile(mag, 95))
            out["swim_max"] = float(mag.max())
            out["swim_median"] = float(np.median(mag))
            out["swim_worst"] = [{"block_px": [s[0], s[1]], "shift_px": round(s[2], 3), "dxdy": [round(s[3], 3), round(s[4], 3)],
                                  "peak": round(s[5], 3)} for s in sorted(shifts, key=lambda s: -s[2])[:5]]
        yaws = [f["yaw"] for f in meta["frames"][:n]]
        out["pan_px_total"] = float(cams[0].f * math.tan(math.radians(max(yaws) - min(yaws))))
    return out


def texpan_checks(res, seq, ref_seq=None, th=TH, quiet=False):
    C, S, meta = res["checks"], res["summary"], seq["meta"]
    try:
        a = texpan_analysis(seq, quiet)
    except Exception as e:  # pragma: no cover
        C.append(check("texpan analysis", FAIL, None, None, "%s: %s" % (type(e).__name__, e)))
        return
    for k in ("stab_mean", "stab_block_p99", "swim_blocks", "swim_p95", "swim_max", "swim_median", "pan_px_total"):
        if k in a:
            S[k] = a[k]
    if a.get("swim_worst"):
        res["swim_worst"] = a["swim_worst"]
    rain, fz = meta.get("rainOverride"), meta.get("freezeClock")
    C.append(check("texpan run conditions", PASS if rain == 0 and fz else WARN, "rain %s, freezeClock %s" % (rain, fz),
                   "-rain 0 -freeze-clock", "the pan must be the only change"))
    if "std_stab" not in a:
        C.append(check("texpan stabilisation", SKIP, None, None, "no per-frame yaw / fov in sequence.json"))
    else:
        C.append(check("stabilised residual std (32 px block p99)", INFO, qc.fmt(a["stab_block_p99"], 3), None,
                       "mean %s levels: content shimmer + cubic resampling (compare with the -capture-ss 2 run)" % qc.fmt(a["stab_mean"], 3)))
        if a.get("swim_blocks"):
            st = PASS if a["swim_p95"] <= SWIM_P95_MAX else FAIL
            if st == PASS and a["swim_max"] > SWIM_MAX_WARN:
                st = WARN
            C.append(check("swimming (block shift first vs last, p95)", st, round(a["swim_p95"], 3),
                           "<= %g px (max <= %g)" % (SWIM_P95_MAX, SWIM_MAX_WARN),
                           "max %.3f px over %d textured 64 px blocks; pan %.1f px; worst %s" % (
                               a["swim_max"], a["swim_blocks"], a["pan_px_total"], a["swim_worst"][0]["block_px"])))
        else:
            C.append(check("swimming", SKIP, 0, None, "no textured 64 px block (high-pass std >= %g levels)" % SWIM_TEX_MIN))
    if ref_seq is None:
        C.append(check("shimmer vs -capture-ss 2 reference", SKIP, None, None,
                       "no --reference given: the gate did not measure (the sequence reads SKIP, not PASS)"))
        return
    try:
        b = texpan_analysis(ref_seq, True)
    except Exception as e:  # pragma: no cover
        C.append(check("shimmer vs -capture-ss 2 reference", FAIL, None, None, "reference: %s: %s" % (type(e).__name__, e)))
        return
    if b["std_screen"].shape != a["std_screen"].shape:
        C.append(check("shimmer vs -capture-ss 2 reference", FAIL, "%s vs %s" % (a["std_screen"].shape, b["std_screen"].shape), "same size"))
        return
    if (ref_seq["meta"].get("supersample") or 1) < 2:
        C.append(check("reference is supersampled", WARN, ref_seq["meta"].get("supersample"), "2", "the reference run lacks -capture-ss 2"))
    allv = np.ones(a["std_screen"].shape, bool)
    ma, mb = _block_means(a["mean_screen"], allv, SHIMMER_BLOCK), _block_means(b["mean_screen"], allv, SHIMMER_BLOCK)
    dm = np.abs(ma - mb)
    mism = dm > np.maximum(REF_MISMATCH_LEVELS, REF_MISMATCH_FRAC * mb)
    S["ref_mean_delta"] = float(b["mean_screen"].mean() - a["mean_screen"].mean())
    S["ref_mismatch_block_share"] = float(mism.mean())
    S["ref_mismatch_block_max"] = float(np.nanmax(dm))
    my, mx = np.unravel_index(int(np.nanargmax(dm)), dm.shape)
    C.append(check("picture vs the -capture-ss 2 reference (32 px blocks off by > 2 levels / 5 %)", INFO if not mism.any() else WARN,
                   round(100.0 * S["ref_mismatch_block_share"], 2), "% of blocks",
                   "frame mean delta %.3f levels (ref - normal); worst block %.2f levels at (%d, %d) px: a footprint regime that switches "
                   "with resolution or HDR highlights averaged after the tonemap; left out of the shimmer gates" % (
                       S["ref_mean_delta"], S["ref_mismatch_block_max"], mx * SHIMMER_BLOCK, my * SHIMMER_BLOCK)))
    ex = a["std_screen"] - b["std_screen"]
    bm = _block_means(ex, allv, SHIMMER_BLOCK)
    bm[mism] = np.nan
    # D-037 (14): a minimum-width line (edge lights, cables) has a higher screen-space std than the ss2 line by construction; such
    # blocks are gated on the stabilised residual below. A line block: >= 2 % of its pixels are ridges of the reference's mean
    # (a pixel > 8 levels above both pixels 2 px away, horizontally or vertically)
    rm = b["mean_screen"].astype(np.float32)
    rx = (rm - np.maximum(np.roll(rm, 2, 1), np.roll(rm, -2, 1))) > 8.0
    ry = (rm - np.maximum(np.roll(rm, 2, 0), np.roll(rm, -2, 0))) > 8.0
    lb = _block_means((rx | ry).astype(np.float32), allv, SHIMMER_BLOCK) >= 0.02
    S["line_block_share"] = float(np.nanmean(lb))
    bm[lb] = np.nan
    worst = float(np.nanmax(bm))
    wy, wx = np.unravel_index(int(np.nanargmax(bm)), bm.shape)
    lim = float(meta.get("shimmerStdMax", th["shimmer_std_max"]))
    S["shimmer_excess_block_max"] = worst
    S["shimmer_excess_block_p99"] = float(np.nanpercentile(bm, 99))
    S["shimmer_excess_mean"] = float(ex.mean())
    C.append(check("shimmer: temporal std above the -capture-ss 2 reference (32 px block max)", PASS if worst <= lim else FAIL,
                   round(worst, 3), "<= %g levels" % lim, "block at (%d, %d) px; p99 %.3f, mean %.3f (14.10, TEXTURE_PLAN 10 R1); %.1f %% "
                   "line blocks gated on the stabilised residual instead (D-037 14)" % (
                       wx * SHIMMER_BLOCK, wy * SHIMMER_BLOCK, S["shimmer_excess_block_p99"], S["shimmer_excess_mean"],
                       100.0 * S["line_block_share"])))
    if "std_stab" in a and "std_stab" in b:
        es = _block_means(a["std_stab"] - b["std_stab"], a["valid"] & b["valid"], SHIMMER_BLOCK)
        es[mism] = np.nan
        if np.isfinite(es).any():
            S["stab_excess_block_max"] = float(np.nanmax(es))
            sy, sx = np.unravel_index(int(np.nanargmax(es)), es.shape)
            C.append(check("stabilised residual above the reference (32 px block max)", PASS if S["stab_excess_block_max"] <= lim else FAIL,
                           round(S["stab_excess_block_max"], 3), "<= %g levels" % lim,
                           "block at (%d, %d) px: the shimmer of world-locked content (the screen-space std above also carries the "
                           "pan's own motion, which masks it)" % (sx * SHIMMER_BLOCK, sy * SHIMMER_BLOCK)))


def block_diffs(g, prev, k=POP_BLOCK):
    """Mean |g - prev| per k x k block (flattened)."""
    H, W = g.shape
    h, w = H // k, W // k
    return np.abs(g - prev)[:h * k, :w * k].reshape(h, k, w, k).mean(axis=(1, 3)).ravel()


def dolly_checks(res, seq, bdiffs, th=TH):
    C, meta = res["checks"], seq["meta"]
    fr = meta.get("frames") if isinstance(meta.get("frames"), list) else []
    dist = [f.get("dist") for f in fr] if fr else []
    rain, fz = meta.get("rainOverride"), meta.get("freezeClock")
    C.append(check("dolly run conditions", PASS if rain == 0 and fz else WARN, "rain %s, freezeClock %s" % (rain, fz),
                   "-rain 0 -freeze-clock", "the camera must be the only change"))
    if bdiffs is None or len(bdiffs) < 3:
        C.append(check("dolly local pops", SKIP, None, None, "fewer than 3 frame differences"))
        return
    D = np.array(bdiffs)          # (frames - 1) x blocks
    events = []
    if D.shape[0] >= 3:
        # a pop is a one-frame event: the block's difference at frame i is 3 x both neighbours' (an edge moving through a block
        # in a zoom lasts several frames; TQ_r05: a running-median test flagged 2 497 such passages)
        nb = np.maximum(np.maximum(np.vstack([D[:1], D[:-1]]), np.vstack([D[1:], D[-1:]])), 0.25)
        nb[0] = np.maximum(D[1], 0.25)
        nb[-1] = np.maximum(D[-2], 0.25)
        for i, b in zip(*np.nonzero((D > POP_RATIO * nb) & (D > POP_FLOOR))):
            events.append((int(i) + 1, int(b), float(D[i, b]), float(nb[i, b])))
    # C20: frame 1 (the first pair after the pose jump) is a start transient: listed apart, not counted
    start = [e for e in events if e[0] == 1]
    events = [e for e in events if e[0] != 1]
    if start:
        C.append(check("dolly start transient (frame 1, after the pose jump)", INFO, len(start), None,
                       "not counted (C20: dolly_face30 frame 1 block 30, 52.6 vs 3.0, also under -rain 0 -freeze-clock)"))
    res["summary"]["dolly_local_pop_events"] = len(events)
    ev = sorted(events, key=lambda e: -(e[2] - e[3]))[:10]
    res["dolly_pops"] = [{"frame": e[0], "block": e[1], "diff": round(e[2], 2), "median": round(e[3], 2),
                          "dist_m": (round(dist[e[0]], 2) if e[0] < len(dist) and dist[e[0]] is not None else None)} for e in ev]
    C.append(check("dolly local pops (64 px blocks)", INFO if not events else WARN, len(events),
                   "0 one-frame block jumps (> %gx both neighbours and > %g levels)" % (POP_RATIO, POP_FLOOR), "worst %s" % (res["dolly_pops"][:3] if events else "-")))
    sp = res.get("spikes") or []
    if dist and sp:
        res["spike_dist_m"] = [round(dist[i], 2) if i < len(dist) and dist[i] is not None else None for i in sp[:20]]


def analyse_sequence(seq, th=TH, quiet=False, ref_seq=None):
    meta, frames = seq["meta"], seq["frames"]
    kind = _kind(seq)
    res = {"name": seq["name"], "dir": seq["dir"], "kind": kind, "fps": seq["fps"], "n_frames": len(frames),
           "checks": [], "series": {}}
    C = res["checks"]
    if len(frames) < 2:
        C.append(check("frames", FAIL if not frames else SKIP, len(frames), ">= 2"))
        res["status"] = qc.worst_status(C)
        return res
    refuse = sx.conditions_check(kind, meta)
    if refuse is not None:
        C.append(refuse)
        res["refused"] = True
        res["summary"] = {}
        res["status"] = FAIL
        return res
    lines = [_resolve_item(l, 0, 0) for l in meta.get("lines", []) or []]
    stabs = [_resolve_item(s, 0, 0) for s in meta.get("stability", []) or []]
    extra = None
    luma, luma_lin, mdiff, xdiff = [], [], [], []
    line_ser = [[] for _ in lines]
    stab_crops = [[] for _ in stabs]
    prev = None
    first = None
    static_max = None
    rain_base = None
    size = None
    bdiffs = [] if kind == "dolly" else None
    for k, (idx, path) in enumerate(frames):
        try:
            rgb8, _ = qc.load_rgb8(path)
        except Exception as e:
            C.append(check("frame %d readable" % idx, FAIL, None, None, str(e)))
            prev = None
            continue
        H, W = rgb8.shape[:2]
        if size is None:
            size = (W, H)
        elif (W, H) != size:
            C.append(check("frame %d size" % idx, FAIL, "%dx%d" % (W, H), "%dx%d" % size))
            continue
        g = qc.grey_levels(rgb8)
        luma.append(float(g.mean() / 255.0))
        Ll = qc.luma_linear(rgb8)
        luma_lin.append(float(Ll.mean()))
        if extra is None:
            extra = sx.Extra(kind, meta, W, H, seq["fps"])
        d = np.abs(g - prev) if prev is not None else None
        extra.add(idx, rgb8, g, d)
        if prev is not None:
            mdiff.append(float(d.mean()))
            xdiff.append(float(d.max()))
            if bdiffs is not None:
                bdiffs.append(block_diffs(g, prev))
        prev = g
        if kind == "rain":
            y = _luma_int(rgb8)
            rain_base = y if rain_base is None else np.minimum(rain_base, y)
        if kind == "static":
            if first is None:
                first = g.copy()
                static_max = np.zeros_like(g)
            else:
                np.maximum(static_max, np.abs(g - first), out=static_max)
        for j, ln in enumerate(lines):
            x, y0, w, h, tol = qc.line_spec_resolved(ln, W, H)
            lp = qc.line_lum(Ll, ln)
            if k == 0 or "_xc" not in ln:
                ln["_xc"] = qc.locate_line(lp, x, y0, h, tol)
            r = qc.line_probe(lp, ln["_xc"], y0, w, h)
            line_ser[j].append(r if r.get("found") else None)
            # follow a slow pan: re-centre the (integer) crop when the line drifts 8 px off its centre; the
            # centroid is in absolute image coordinates, so the series stays continuous
            if r.get("found") and abs(r["centroid_mean"] - ln["_xc"]) > 8.0:
                ln["_xc"] = float(np.floor(r["centroid_mean"])) + 0.5
        for j, st in enumerate(stabs):
            x0, y0, x1, y1 = qc.item_rect(st, "rect", W, H)
            stab_crops[j].append(g[y0:y1, x0:x1].astype(np.float32) / 255.0)
        if not quiet and k % 100 == 0:
            print("[sequences] %s frame %d/%d" % (seq["name"], k + 1, len(frames)), flush=True)
    res["size"] = list(size) if size else None
    luma = np.array(luma)
    steps = np.abs(np.diff(luma)) if luma.size > 1 else np.zeros(0)
    mdiff, xdiff = np.array(mdiff), np.array(xdiff)
    res["series"] = {"luma": [round(v, 5) for v in luma], "luma_linear": [round(v, 5) for v in luma_lin],
                     "mean_abs_diff": [round(v, 4) for v in mdiff], "max_abs_diff": [round(v, 2) for v in xdiff]}
    res["summary"] = {"luma_min": float(luma.min()), "luma_max": float(luma.max()),
                      "max_luma_step": float(steps.max()) if steps.size else 0.0,
                      "max_luma_step_frame": int(np.argmax(steps)) + 1 if steps.size else None,
                      "mean_diff": float(mdiff.mean()) if mdiff.size else 0.0,
                      "max_diff": float(mdiff.max()) if mdiff.size else 0.0,
                      "max_pixel_diff": float(xdiff.max()) if xdiff.size else 0.0}
    S = res["summary"]

    # luma steps (not on the start / end fades)
    if kind != "edge":
        lim = float(meta.get("lumaStepMax", th["seq_luma_step_max"]))
        C.append(check("luma step/frame", PASS if S["max_luma_step"] <= lim else FAIL, round(S["max_luma_step"] * 255.0, 3),
                       "<= %g levels" % round(lim * 255.0, 3), "at frame %s (14.10)" % S["max_luma_step_frame"]))
    # spikes
    if mdiff.size >= 3 and kind != "edge":
        ratio = float(meta.get("spikeRatio", th["spike_ratio"]))
        floor = float(meta.get("spikeFloor", th["spike_floor_grey"]))
        med = running_median(mdiff, int(th["spike_window"]))
        sp = np.flatnonzero((mdiff > ratio * med) & (mdiff > floor))
        res["spikes"] = [int(i) + 1 for i in sp[:50]]
        C.append(check("diff spikes", PASS if sp.size == 0 else FAIL, int(sp.size), "0 above %gx running median" % ratio,
                       ("frames %s" % res["spikes"][:10]) if sp.size else "floor %g grey levels" % floor))
    # activation frames (boundary walks): the difference at a boundary crossing above the centred median of the surrounding
    # frames (QA-7, FX: a trend passes through the centred median, a pop stands above both sides of it)
    if kind == "boundary":
        fs = _frame_s(meta)
        bs = [float(b) for b in (meta.get("boundaryS") or [])]
        worst, at = 0.0, None
        if fs and bs and mdiff.size:
            for i in range(1, min(len(fs), mdiff.size + 1)):
                a, b = fs[i - 1], fs[i]
                if a is None or b is None or not any(a < x <= b for x in bs):
                    continue
                h = int(th["spike_window"]) // 2
                hist = np.concatenate((mdiff[max(0, i - 1 - h):i - 1], mdiff[i:min(mdiff.size, i + h)]))
                if hist.size >= 3:
                    ex = float(mdiff[i - 1] - np.median(hist))
                    if ex > worst:
                        worst, at = ex, i
            lim = float(meta.get("activationDiffMax", th["activation_diff_max"]))
            C.append(check("activation-frame diff", PASS if worst <= lim else FAIL, round(worst, 4), "<= %g level" % lim,
                           "boundaries %s, worst at frame %s" % (bs, at)))
        else:
            C.append(check("activation-frame diff", SKIP, None, None, "no boundary crossed or no per-frame s"))
    # static clip
    if kind == "static" and static_max is not None:
        sm = static_max
        for r in meta.get("mask", []) or []:
            x0, y0, x1, y1 = [int(v) for v in r]
            sm[y0:y1, x0:x1] = 0.0
        S["static_max_change"] = float(sm.max())
        S["static_p999_change"] = float(np.percentile(sm, 99.9))
        lim = float(meta.get("maxPixelDiff", th["static_change_max"]))
        fz = meta.get("freezeClock")
        rain = meta.get("rainOverride", meta.get("rainMultiplier"))   # D-020: -rain X overrides rho (None: schedule)
        C.append(check("static change", PASS if S["static_max_change"] <= lim else FAIL, round(S["static_max_change"], 3),
                       "<= %g levels" % lim, "max |frame - frame 0| per pixel; p99.9 %.2f; freezeClock %s, rain override %s (14.10: run with "
                       "-freeze-clock -rain 0)" % (S["static_p999_change"], fz, rain)))
    elif kind == "rain" and rain_base is not None:
        rf = rain_flicker(frames, rain_base, th)
        S.update({"rain_" + k: v for k, v in rf.items()})
        sx.rain_checks(res, frames, rain_base, meta, th)
        lim = float(meta.get("rainOutsideChangeMax", th["rain_outside_change_max"]))
        C.append(check("rain outside-streak change", PASS if rf["outside_change_max"] <= lim else FAIL,
                       rf["outside_change_max"], "<= %g levels" % lim,
                       "14.7 H5: streak mask = luma > temporal min + %g codes, dilated %d px; p99.9 mean %.2f; mask "
                       "coverage %.3f %% (mean); %d pairs" % (th["rain_streak_codes"], th["rain_mask_dilate_px"],
                                                          rf["outside_p999_mean"], rf["mask_coverage_pct"], rf["pairs"])))
    elif "meanDiffMax" in meta:
        slim = float(meta["meanDiffMax"])
        C.append(check("consecutive diff", PASS if S["max_diff"] < slim else FAIL, round(S["max_diff"], 4),
                       "< %g grey (every pair)" % slim))
    if "maxPixelDiff" in meta and kind != "static":
        plim = float(meta["maxPixelDiff"])
        C.append(check("max pixel diff", PASS if S["max_pixel_diff"] <= plim else FAIL, round(S["max_pixel_diff"], 2),
                       "<= %g grey" % plim))
    # thin lines
    res["lines"] = {}
    for j, ln in enumerate(lines):
        name = ln.get("name", "line%d" % j)
        ser = line_ser[j]
        ok = np.array([r is not None for r in ser])
        if ok.sum() < 3:
            C.append(check("line[%s] found" % name, FAIL, int(ok.sum()), ">= 3 frames"))
            continue
        n = np.flatnonzero(ok).astype(np.float64)
        cen = np.array([r["centroid_mean"] for r in ser if r is not None])
        en = np.array([r["energy_mean"] for r in ser if r is not None])
        fw = np.array([r["fwhm_px"] for r in ser if r is not None])
        fit = np.polyfit(n, cen, 1)
        resid = cen - np.polyval(fit, n)
        jit = float(np.abs(resid).max())
        ecv = float(np.std(en) / max(abs(en.mean()), 1e-12))
        res["lines"][name] = {"pan_px_per_frame": float(fit[0]), "jitter_max_px": jit,
                              "jitter_rms_px": float(np.sqrt(np.mean(resid ** 2))), "energy_cv": ecv,
                              "fwhm_median": float(np.median(fw)), "frames_found": int(ok.sum()),
                              "centroid": [round(v, 4) for v in cen]}
        C.append(check("line[%s] centroid jitter" % name, PASS if jit < th["line_jitter_max_px"] else FAIL, round(jit, 4),
                       "< %g px" % th["line_jitter_max_px"], "pan %.4f px/frame" % fit[0]))
        C.append(check("line[%s] energy variation" % name, PASS if ecv < th["line_energy_var_max"] else FAIL,
                       round(ecv, 4), "< %g" % th["line_energy_var_max"]))
        if ok.sum() < len(ser):
            C.append(check("line[%s] found in every frame" % name, FAIL, int(ok.sum()), len(ser)))
    # stability crops
    res["stability"] = {}
    for j, st in enumerate(stabs):
        name = st.get("name", "crop%d" % j)
        crops = stab_crops[j]
        win = int(st.get("window", th["stability_window"]))
        tol = float(st.get("tol", th["stability_tol"]))
        floor = float(st.get("floor", th["stability_floor"]))
        if len(crops) < win or crops[0].size == 0:
            C.append(check("stability[%s]" % name, SKIP, len(crops), ">= %d frames" % win))
            continue
        stack = np.stack(crops)
        worst, worst_n = 0.0, None
        for a in range(0, len(crops) - win + 1):
            blk = stack[a:a + win]
            m = blk.mean(axis=0)
            dev = (np.abs(blk - m[None]) / np.maximum(m, floor)[None]).max()
            if dev > worst:
                worst, worst_n = float(dev), a
        res["stability"][name] = {"max_rel_dev": worst, "window_start": worst_n}
        C.append(check("stability[%s]" % name, PASS if worst <= tol else FAIL, round(worst, 4), "<= %g" % tol,
                       "5-frame mean, floor %g" % floor))
    if extra is not None:
        fs = _frame_s(meta)
        extra.pop_checks(res, fs, frames)
        static_cam = (meta.get("mode") == "Static")
        extra.photo_checks(res, luma, static_cam)
    if kind in ("texpan", "pan"):
        texpan_checks(res, seq, ref_seq, th, quiet)
    elif kind == "dolly":
        dolly_checks(res, seq, bdiffs, th)
    res["status"] = qc.worst_status(C)
    req = REQUIRED.get(kind, ())
    if res["status"] in (PASS, WARN) and req:
        miss = [c["name"] for c in C if any(c["name"].startswith(r) for r in req) and c["status"] == SKIP]
        have = [c["name"] for c in C if any(c["name"].startswith(r) for r in req) and c["status"] in (PASS, WARN, FAIL)]
        if miss or not have:
            res["status"] = SKIP
            res["skip_reason"] = "required gate did not measure: %s" % (", ".join(miss) or "/".join(req))
    return res


def find_reference(seq, reference):
    """The sequence of the same name inside the reference folder(s) (a capture folder or a sequence folder), else None."""
    if not reference:
        return None
    for d in qc.resolve_sequence_dirs(reference):
        r = qc.load_sequence(d)
        if r["name"] == seq["name"]:
            return r
    return None


def build_markdown(run, results):
    allc = [c for r in results for c in r["checks"]]
    cnt = qc.count_status(allc)
    L = ["# Frame sequences: %s" % run, "",
         "Sequences: %d. Checks: %d PASS, %d FAIL, %d WARN, %d SKIP." % (len(results), cnt[PASS], cnt[FAIL],
                                                                       cnt[WARN], cnt[SKIP]), "",
         "**Result: %s**" % overall_status(results), "",
         "(PASS only when no check FAILed and every sequence's required gate measured; SKIP = something did not measure.)", "",
         "## Summary", ""]
    rows = []
    for r in results:
        S = r.get("summary", {})
        rows.append([r["name"], r["kind"], r["n_frames"], qc.fmt(S.get("max_luma_step"), 4), qc.fmt(S.get("mean_diff")),
                     qc.fmt(S.get("max_diff")), qc.fmt(S.get("max_pixel_diff"), 1),
                     "%s / %s" % (S.get("pop_events", "-"), S.get("blink_events", "-")), r["status"],
                     ", ".join(c["name"] for c in r["checks"] if c["status"] == FAIL) or r.get("skip_reason", "")])
    L.append(qc.md_table(["Sequence", "Kind", "Frames", "Max luma step", "Mean diff", "Max diff", "Max px diff",
                          "Pops / blinks", "Result", "Failed checks"], rows))
    for r in results:
        L += ["## %s (%s)" % (r["name"], r["status"]), "", "`%s`, %d frames at %g fps, kind %s" % (
            r["dir"], r["n_frames"], r["fps"], r["kind"]), ""]
        if r.get("series"):
            L += ["- luma      `%s`" % sparkline(r["series"].get("luma", [])),
                  "- mean diff `%s`" % sparkline(r["series"].get("mean_abs_diff", [])), ""]
        L.append(qc.md_table(["Check", "Status", "Value", "Threshold", "Note"],
                             [[c["name"], c["status"], qc.fmt(c["value"], 4), c["threshold"] or "", c["note"] or ""]
                              for c in r["checks"]]))
        if r.get("pops"):
            L += ["", "Worst block events (frame = the later frame of the pair; block = x0, y0, x1, y1 px):", ""]
            L.append(qc.md_table(["Type", "Frame", "s", "Block px", "Size", "Diff", "Neighbours"],
                                 [[e["type"], e["frame"], qc.fmt(e.get("s"), 2), e["block_px"], e["k"], qc.fmt(e["diff"], 2),
                                   qc.fmt(e["neighbours"], 2)] for e in r["pops"][:12]]))
        for key, label in (("montage", "Montage"), ("crops", "Pop crops (4 x)")):
            if r.get(key):
                L += ["", "%s: %s" % (label, ", ".join("`%s`" % p for p in r[key]))]
        if r.get("clip"):
            L += ["", "x264 clip: `%s`" % r["clip"]]
        if r.get("frames_deleted"):
            L += ["", "Frames deleted after analysis: %d (analysed.done in the folder)" % r["frames_deleted"]]
    return "\n".join(L)


def overall_status(results):
    allc = [c for r in results for c in r["checks"]]
    if any(c["status"] == FAIL for c in allc) or any(r.get("status") == FAIL for r in results):
        return FAIL
    if not results or any(r.get("status") == SKIP for r in results):
        return SKIP
    return PASS


def run(paths, run_name=None, out_dir=qc.REPORTS_DIR, quiet=False, reference=None, montage=True, delete_frames=False):
    seq_dirs = qc.resolve_sequence_dirs(paths)
    if run_name is None:
        base = paths[0] if paths else "."
        b = os.path.abspath(base)
        run_name = qc.run_id_for(os.path.dirname(b) if os.path.basename(b).lower().startswith("seq_") else b)
    results = []
    for d in seq_dirs:
        sq = qc.load_sequence(d)
        res = analyse_sequence(sq, TH, quiet, find_reference(sq, reference) if _kind(sq) in ("texpan", "pan") else None)
        if montage and not res.get("refused"):
            sx.finish_sequence(sq, res, _frame_s(sq["meta"]), delete_frames, os.path.join(out_dir, "sequences_%s.md" % run_name))
        results.append(res)
        if not quiet:
            print("[sequences] %s: %s%s" % (sq["name"], res["status"], (" (%d frames deleted)" % res["frames_deleted"]) if res.get("frames_deleted") else ""), flush=True)
    allc = [c for r in results for c in r["checks"]]
    status = overall_status(results)
    js = os.path.join(out_dir, "sequences_%s.json" % run_name)
    md = os.path.join(out_dir, "sequences_%s.md" % run_name)
    qc.write_json(js, {"run": run_name, "status": status, "sequences": results})
    qc.write_text(md, build_markdown(run_name, results))
    fails = ["%s: %s (%s)" % (r["name"], c["name"], qc.fmt(c["value"], 4)) for r in results for c in r["checks"]
             if c["status"] == FAIL]
    return {"tool": "sequences", "status": status, "json": js, "md": md, "fails": fails,
            "counts": qc.count_status(allc), "n": len(results)}


def main(argv=None):
    ap = argparse.ArgumentParser(prog="sequences.py", description=__doc__.split("\n\n")[0],
                                 formatter_class=argparse.RawDescriptionHelpFormatter, epilog=__doc__)
    ap.add_argument("paths", nargs="+", help="capture folder(s) with seq_* subfolders, or sequence folders")
    ap.add_argument("--run", help="run id (default: capture folder name)")
    ap.add_argument("--out-dir", default=qc.REPORTS_DIR)
    ap.add_argument("--quiet", action="store_true")
    ap.add_argument("--reference", nargs="*", help="the -capture-ss 2 run of the same texpans set (capture or sequence folders): the R1 shimmer gate")
    ap.add_argument("--no-montage", action="store_true", help="skip the montage, crops and clip")
    ap.add_argument("--delete-frames", action="store_true", help="after the montage, delete each sequence's PNG frames (analysed.done marker)")
    a = ap.parse_args(argv)
    missing = [p for p in a.paths + (a.reference or []) if not os.path.isdir(p)]
    if missing:
        print("not found: %s" % ", ".join(missing), file=sys.stderr)
        return 1
    r = run(a.paths, a.run, a.out_dir, a.quiet, a.reference, not a.no_montage, a.delete_frames)
    print("[sequences] %s: %d sequences, %d FAIL checks -> %s" % (r["status"], r["n"], len(r["fails"]), r["md"]))
    for f in r["fails"][:30]:
        print("  FAIL " + f)
    return 1 if r["status"] == FAIL else 0


if __name__ == "__main__":
    sys.exit(main())
