#!/usr/bin/env python3
"""sequences: frame-sequence tests of Bulwark captures (MASTER_PROMPT 11.6, 14.10.3; imported from Thread).

Usage:
  python -I Tools/qa/sequences.py QA/captures/<run-id>            (every seq_* folder inside)
  python -I Tools/qa/sequences.py QA/captures/<run-id>/seq_<name> [more folders] [--run <id>] [--out-dir QA/reports]

Per sequence (frame_NNNNN.png + sequence.json written by CaptureHarness, see qa_common): the mean absolute difference
between consecutive frames in 8-bit grey levels (Rec.709 display luma x 255) and its per-pixel maximum, the
frame-mean luma series, and the 14.10.3 checks
  - mean luma step per frame at most 1.2 levels (2.0 when the sequence touches the sun break, s 520-700), or
    sequence.json "lumaStepMax" (levels);
  - activation-frame difference: at every frame whose s crosses a beat boundary the mean difference exceeds the
    median of its +-5 neighbours by at most 0.5 levels;
  - EV slope at most 0.30 EV/s when the frames carry "ev" (else the log2 luma slope is reported as INFO);
  - difference spikes: no frame difference above 3 x the running median (31-frame window, floor 0.25 levels);
  - pan sequences (mode PanYaw / PanPitch, 0.5 px per frame, ambient clock frozen): shimmer = the largest 64 x 64
    block mean of the per-pixel temporal std after motion compensation, at most 2.0 levels (1.5 in beat 6 and on the
    beat-7 ledge, s >= 1423), or "panStdMax". Compensation (Wave B): every frame is warped into frame 0's camera by
    the EXACT rotation homography H = K R0 Rn^-1 K^-1 from the per-frame yaw / pitch / vFOV of sequence.json (pure
    rotation: no depth needed; Unity camera Euler(-pitch, yaw, 0)), sampled bilinearly, 48 px margin (1080p). A single
    shift along the pan axis is wrong away from the centre (up to 1 / cos^2 at the frame edge: 16.26 vs 1.09 levels on
    the grating at pitch -58, 2.66 vs 0.71 on the road; materials Wave A). Fine patterns (2-4 px periods, the grating
    and line fade regime) also have a bilinear interpolation floor: a control warps frame 0 forth and back with the
    same offsets (what a perfectly stable image scores). Per block the gate value is the raw std, except where the
    control floor exceeds half the limit (the fine-pattern regime): there it is max(low-pass std (Gaussian sigma 1.5 px
    before the temporal std), block-mean luma std over the frames), both free of the interpolation floor. All four
    numbers are reported; sequence.json "panGate": "raw" forces the raw gate. Old single-shift compensation only when
    the frames carry no pose;
  - static sequences (camera fixed, clock running): flicker = std over the frames (120) of every 64 x 64 block mean,
    at most 3 levels (1.5 on the beat-7 ledge, s >= 1423), or "flickerStdMax"; plus the firefly test (luma above
    8 x the 5 x 5 median and 16+ levels above it) on every frame: 0 pixels;
  - thin lines ("lines"): centroid jitter below 0.1 px and frame-to-frame energy variation below 10 percent;
  - "stability" crops: no pixel deviates more than 12 percent from the mean of 5 consecutive frames.
Writes QA/reports/sequences_<run>.json and .md.  Exit code 1 if any check FAILs.
"""
import argparse
import os
import sys

_HERE = os.path.dirname(os.path.abspath(__file__))
if _HERE not in sys.path:
    sys.path.insert(0, _HERE)

import numpy as np  # noqa: E402

import qa_common as qc  # noqa: E402
from qa_common import PASS, FAIL, SKIP, INFO, WARN, TH, check  # noqa: E402

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


def _kind(seq):
    """static | pan | encode | transition: sequence.json "kind", else the harness "mode"/"set", else the name."""
    m = seq["meta"]
    k = (m.get("kind") or "").lower()
    if k:
        return k
    mode = (m.get("mode") or "").lower()
    if mode == "static":
        return "static"
    if mode in ("panyaw", "panpitch"):
        return "pan"
    if (m.get("set") or "").lower() == "encode" or m.get("encode"):
        return "encode"
    n = seq["name"].lower()
    for key in ("static", "pan", "encode"):
        if key in n:
            return key
    return "transition"


def _frame_list(meta):
    fr = meta.get("frames")
    return fr if isinstance(fr, list) else []


def _s_series(meta):
    s = meta.get("s")
    if isinstance(s, list) and s:
        return [float(v) for v in s]
    out = [f.get("s") for f in _frame_list(meta) if isinstance(f, dict)]
    if out and all(v is not None for v in out):
        return [float(v) for v in out]
    s0, s1 = meta.get("s0"), meta.get("s1")
    return [float(s0), float(s1)] if s0 is not None and s1 is not None else []


def _pan_offsets(meta, W, H, n):
    """Per-frame pixel offset of a pan along its axis from the frames' yaw (PanYaw) or pitch (PanPitch) and vFOV."""
    fr = [f for f in _frame_list(meta) if isinstance(f, dict)]
    mode = (meta.get("mode") or "").lower()
    key = "pitch" if mode == "panpitch" else "yaw"
    if len(fr) < n or any(f.get(key) is None or f.get("fov") is None for f in fr[:n]):
        return None, ("y" if key == "pitch" else "x")
    f_px = 0.5 * H / np.tan(np.radians(float(fr[0]["fov"])) * 0.5)
    a0 = float(fr[0][key])
    off = []
    for f in fr[:n]:
        d = (float(f[key]) - a0 + 180.0) % 360.0 - 180.0
        # yaw right moves the image left (+x content shifts -x); pitch up moves the image down
        off.append(f_px * np.tan(np.radians(d)) * (-1.0 if key == "yaw" else 1.0))
    return np.array(off), ("y" if key == "pitch" else "x")


def _rot(yaw_deg, pitch_deg):
    """Camera-to-world rotation (columns = camera right, up, forward in world) of Unity's Euler(-pitch, yaw, 0)."""
    y, p = np.radians(yaw_deg), np.radians(-pitch_deg)
    cy, sy, cp, sp = np.cos(y), np.sin(y), np.cos(p), np.sin(p)
    ry = np.array([[cy, 0.0, sy], [0.0, 1.0, 0.0], [-sy, 0.0, cy]])
    rx = np.array([[1.0, 0.0, 0.0], [0.0, cp, -sp], [0.0, sp, cp]])
    return ry @ rx


def _frame_poses(meta, n):
    """(yaw, pitch, fov) per frame from sequence.json, or None when any is missing."""
    fr = [f for f in _frame_list(meta) if isinstance(f, dict)]
    if len(fr) < n:
        return None
    out = []
    for f in fr[:n]:
        if f.get("yaw") is None or f.get("pitch") is None or f.get("fov") is None:
            return None
        out.append((float(f["yaw"]), float(f["pitch"]), float(f["fov"])))
    return out


def _warp_coords(pose_n, pose_0, W, H):
    """Pixel coordinates (u, v) in frame n of every frame-0 pixel centre under the rotation homography."""
    f0 = 0.5 * H / np.tan(np.radians(pose_0[2]) * 0.5)
    fn = 0.5 * H / np.tan(np.radians(pose_n[2]) * 0.5)
    xs, ys = np.meshgrid(np.arange(W, dtype=np.float64) + 0.5, np.arange(H, dtype=np.float64) + 0.5)
    d = np.stack([(xs - 0.5 * W) / f0, -(ys - 0.5 * H) / f0, np.ones_like(xs)], axis=-1)
    world = d @ _rot(pose_0[0], pose_0[1]).T
    cam = world @ _rot(pose_n[0], pose_n[1])
    z = cam[..., 2]
    ok = z > 1e-6
    z = np.where(ok, z, 1.0)
    u = cam[..., 0] / z * fn + 0.5 * W - 0.5
    v = -cam[..., 1] / z * fn + 0.5 * H - 0.5
    ok &= (u >= 0) & (u <= W - 1.001) & (v >= 0) & (v <= H - 1.001)
    return u, v, ok


def _bilinear(img, u, v, ok):
    H, W = img.shape
    uu = np.clip(u, 0, W - 1.001)
    vv = np.clip(v, 0, H - 1.001)
    x0, y0 = np.floor(uu).astype(np.int64), np.floor(vv).astype(np.int64)
    fx, fy = (uu - x0).astype(np.float32), (vv - y0).astype(np.float32)
    a = img[y0, x0] * (1 - fx) + img[y0, x0 + 1] * fx
    b = img[y0 + 1, x0] * (1 - fx) + img[y0 + 1, x0 + 1] * fx
    out = (a * (1 - fy) + b * fy).astype(np.float32)
    out[~ok] = np.nan
    return out


def _gauss_blur(img, sigma):
    """Separable Gaussian blur (NaN-free input), radius 3 sigma, edge-replicated."""
    r = max(1, int(np.ceil(3.0 * sigma)))
    k = np.exp(-0.5 * (np.arange(-r, r + 1) / sigma) ** 2)
    k = (k / k.sum()).astype(np.float32)
    a = np.pad(img, ((0, 0), (r, r)), mode="edge")
    a = sum(k[i] * a[:, i:i + img.shape[1]] for i in range(2 * r + 1))
    a = np.pad(a, ((r, r), (0, 0)), mode="edge")
    return sum(k[i] * a[i:i + img.shape[0], :] for i in range(2 * r + 1)).astype(np.float32)


def pan_shimmer_exact(stack, poses, bsz, margin, lim, sigma=1.5, gate="auto"):
    """Exact-homography pan shimmer of a pure-rotation sequence (see the module doc). stack: grey frames (levels),
    poses: (yaw, pitch, fov) per frame. Returns a dict: raw / low-pass / block-mean / control-floor block maxima, the
    gate value and the number of fine-pattern blocks."""
    H, W = stack[0].shape
    p0 = poses[0]
    ref = stack[0].astype(np.float32)
    raw, ctrl, lp = [], [], []
    for img, pn in zip(stack, poses):
        u, v, ok = _warp_coords(pn, p0, W, H)
        w = _bilinear(img.astype(np.float32), u, v, ok)
        raw.append(w)
        # control: frame 0 warped into camera n and back (the same sub-pixel offsets): the interpolation floor
        ub, vb, okb = _warp_coords(p0, pn, W, H)
        there = _bilinear(ref, ub, vb, okb)
        there = np.where(np.isnan(there), 0.0, there).astype(np.float32)
        ctrl.append(_bilinear(there, u, v, ok))
    raw = np.stack(raw)
    ctrl = np.stack(ctrl)
    valid = ~np.isnan(raw).any(axis=0) & ~np.isnan(ctrl).any(axis=0)
    m = int(margin)
    if m > 0:
        valid[:m, :] = False
        valid[-m:, :] = False
        valid[:, :m] = False
        valid[:, -m:] = False
    filled = np.where(valid[None], raw, 0.0).astype(np.float32)
    for f in filled:
        lp.append(_gauss_blur(f, sigma))
    lp = np.stack(lp)
    sd_raw = raw.std(axis=0)
    sd_ctrl = ctrl.std(axis=0)
    sd_lp = lp.std(axis=0)
    rows = []
    for y in range(0, H - bsz + 1, bsz):
        for x in range(0, W - bsz + 1, bsz):
            vm = valid[y:y + bsz, x:x + bsz]
            if vm.mean() <= 0.5:
                continue
            b_raw = float(sd_raw[y:y + bsz, x:x + bsz][vm].mean())
            b_ctrl = float(sd_ctrl[y:y + bsz, x:x + bsz][vm].mean())
            b_lp = float(sd_lp[y:y + bsz, x:x + bsz][vm].mean())
            means = raw[:, y:y + bsz, x:x + bsz][:, vm].mean(axis=1)
            b_bm = float(means.std())
            fine = gate != "raw" and b_ctrl > 0.5 * lim
            rows.append((b_raw, b_ctrl, b_lp, b_bm, max(b_lp, b_bm) if fine else b_raw, fine, x, y))
    if not rows:
        return None
    best = max(rows, key=lambda r: r[4])
    return {"raw_block_max": max(r[0] for r in rows), "control_floor_block_max": max(r[1] for r in rows),
            "lowpass_block_max": max(r[2] for r in rows), "block_mean_std_max": max(r[3] for r in rows),
            "gate_value": best[4], "gate_block": [best[6], best[7]], "gate_block_fine": bool(best[5]),
            "fine_blocks": int(sum(1 for r in rows if r[5])), "blocks": len(rows)}


def _shift_linear(img, d, axis):
    """Shift a 2-D image by d pixels along axis (1 = x, 0 = y) with linear interpolation; edge pixels are NaN."""
    n = img.shape[axis]
    i0 = int(np.floor(d))
    fr = d - i0
    out = np.full(img.shape, np.nan, dtype=np.float32)
    src = img.astype(np.float32)
    a = np.roll(src, -i0, axis=axis)
    b = np.roll(src, -(i0 + 1), axis=axis)
    v = (1.0 - fr) * a + fr * b
    lo, hi = max(0, -i0), min(n, n - i0 - 1)
    if hi <= lo:
        return out
    if axis == 1:
        out[:, lo:hi] = v[:, lo:hi]
    else:
        out[lo:hi, :] = v[lo:hi, :]
    return out


def _block_means(g, b):
    H, W = g.shape
    hb, wb = H // b, W // b
    if hb == 0 or wb == 0:
        return np.array([[float(g.mean())]])
    return g[:hb * b, :wb * b].reshape(hb, b, wb, b).mean(axis=(1, 3))


def _resolve_item(it, W, H):
    it = dict(it)
    it.setdefault("_ref", it.get("ref"))
    it.setdefault("_units", it.get("units", None if it.get("ref") else "px"))
    return it


def analyse_sequence(seq, th=TH, quiet=False):
    meta, frames = seq["meta"], seq["frames"]
    kind = _kind(seq)
    res = {"name": seq["name"], "dir": seq["dir"], "kind": kind, "fps": seq["fps"], "n_frames": len(frames),
           "checks": [], "series": {}}
    C = res["checks"]
    if len(frames) < 2:
        C.append(check("frames", FAIL if not frames else SKIP, len(frames), ">= 2"))
        res["status"] = qc.worst_status(C)
        return res
    lines = [_resolve_item(l, 0, 0) for l in meta.get("lines", []) or []]
    stabs = [_resolve_item(s, 0, 0) for s in meta.get("stability", []) or []]
    luma, luma_lin, mdiff, xdiff = [], [], [], []
    pan_stack, blocks, fireflies = [], [], []
    bsz = int(th["flicker_block"])
    line_ser = [[] for _ in lines]
    stab_crops = [[] for _ in stabs]
    prev = None
    size = None
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
        if prev is not None:
            d = np.abs(g - prev)
            mdiff.append(float(d.mean()))
            xdiff.append(float(d.max()))
        prev = g
        if kind == "pan":
            pan_stack.append(g.astype(np.float16))
        if kind == "static":
            blocks.append(_block_means(g.astype(np.float32), bsz))
            fireflies.append(qc.fireflies(g)[0])
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

    sp = qc.spec()
    s_ser = _s_series(meta)
    s_lo = min(s_ser) if s_ser else None
    s_hi = max(s_ser) if s_ser else None
    beat = sp.beat_of_s(0.5 * (s_lo + s_hi)) if s_ser else None
    sb0, sb1 = sp.sun_break
    in_break = bool(s_ser) and s_hi >= sb0 and s_lo <= sb1
    # 14.10.5 ruling (Wave X2, lead): walking through the lit beat-1 door (s 112-120) or into the gate-tower portal
    # (s 912-920) at 2.5 m/s is a content ramp, not an exposure step: the 4 m opening fills the frame in about 2 s while
    # EV follows its 0.30 EV/s slew (measured 2.11 and 2.55 levels per frame, 0 difference spikes, activation diffs
    # 0.0003 / 0.07). Limit 2.6 there; spikes and activation diffs keep their own gates.
    in_door = bool(s_ser) and any(s_hi >= a and s_lo <= b for a, b in ((112.0, 120.0), (912.0, 920.0)))
    ledge = bool(s_ser) and s_lo >= sp.ledge_strict_s
    S["max_luma_step_levels"] = S["max_luma_step"] * 255.0
    # luma steps (levels per frame)
    lim = float(meta.get("lumaStepMax", th["seq_luma_step_sunbreak_levels"] if in_break else (th["seq_luma_step_doorway_levels"] if in_door else th["seq_luma_step_max_levels"])))
    C.append(check("luma step/frame", PASS if S["max_luma_step_levels"] <= lim else FAIL, round(S["max_luma_step_levels"], 4),
                   "<= %g levels" % lim, "at frame %s%s" % (S["max_luma_step_frame"], ", sun break" if in_break else "")))
    # activation frames: beat boundaries crossed between consecutive frames
    fr_s = [f.get("s") for f in _frame_list(meta) if isinstance(f, dict)]
    if mdiff.size >= 3 and len(fr_s) == len(frames) and all(v is not None for v in fr_s):
        acts = []
        for i in range(1, len(fr_s)):
            for b in sp.beat_boundaries[1:-1]:
                if (fr_s[i - 1] - b) * (fr_s[i] - b) <= 0 and fr_s[i - 1] != fr_s[i]:
                    acts.append(i)
        worst, at = 0.0, None
        for i in acts:
            k = i - 1   # mdiff index of the pair (i-1, i)
            nb = np.concatenate((mdiff[max(0, k - 5):k], mdiff[k + 1:k + 6]))
            if nb.size:
                ex = float(mdiff[k] - np.median(nb))
                if ex > worst:
                    worst, at = ex, i
        if acts:
            C.append(check("activation-frame diff", PASS if worst <= th["activation_diff_max"] else FAIL, round(worst, 4),
                           "<= %g levels over the local median" % th["activation_diff_max"],
                           "%d boundary frames%s" % (len(acts), (", worst frame %d" % at) if at is not None else "")))
    # EV slope
    evs = [f.get("ev") for f in _frame_list(meta) if isinstance(f, dict)]
    fps = float(seq["fps"] or 30.0)
    if len(evs) >= 2 and all(v is not None for v in evs):
        ev = np.array(evs, dtype=np.float64)
        slope = float(np.abs(np.diff(ev)).max() * fps)
        C.append(check("EV slope", PASS if slope <= th["ev_slope_max"] + 1e-9 else FAIL, round(slope, 4),
                       "<= %g EV/s" % th["ev_slope_max"]))
    elif len(luma_lin) >= 2:
        ll = np.log2(np.maximum(np.array(luma_lin), 1e-6))
        C.append(check("EV slope", INFO, round(float(np.abs(np.diff(ll)).max() * fps), 4), None,
                       "no per-frame 'ev' in sequence.json: log2 linear-luma slope (content changes included)"))
    # spikes
    if mdiff.size >= 3:
        ratio = float(meta.get("spikeRatio", th["spike_ratio"]))
        floor = float(meta.get("spikeFloor", th["spike_floor_grey"]))
        med = running_median(mdiff, int(th["spike_window"]))
        spk = np.flatnonzero((mdiff > ratio * med) & (mdiff > floor))
        res["spikes"] = [int(i) + 1 for i in spk[:50]]
        C.append(check("diff spikes", PASS if spk.size == 0 else FAIL, int(spk.size), "0 above %gx running median" % ratio,
                       ("frames %s" % res["spikes"][:10]) if spk.size else "floor %g grey levels" % floor))
    # pan shimmer (motion-compensated temporal std)
    poses = _frame_poses(meta, len(pan_stack)) if kind == "pan" else None
    if kind == "pan" and len(pan_stack) >= 3 and poses is not None:
        Hh, Ww = pan_stack[0].shape
        strict = beat == 6 or ledge
        lim = float(meta.get("panStdMax", th["pan_std_strict"] if strict else th["pan_std_max"]))
        margin = int(round(48 * Hh / 1080.0))
        r = pan_shimmer_exact(pan_stack, poses, bsz, margin, lim, gate=str(meta.get("panGate", "auto")).lower())
        if r is None:
            C.append(check("pan shimmer std", SKIP, None, None, "no valid block after the warp"))
        else:
            S["pan_std_block_max"] = r["raw_block_max"]
            S["pan_exact"] = r
            note = ("exact rotation homography; raw %.3f, control floor %.3f, low-pass (sigma 1.5) %.3f, block-mean %.3f; "
                    "gate block %s%s; %d of %d blocks in the fine-pattern regime%s") % (
                r["raw_block_max"], r["control_floor_block_max"], r["lowpass_block_max"], r["block_mean_std_max"],
                r["gate_block"], " (fine: max(low-pass, block-mean))" if r["gate_block_fine"] else " (raw)",
                r["fine_blocks"], r["blocks"], "; strict (beat 6 / ledge)" if strict else "")
            C.append(check("pan shimmer std", PASS if r["gate_value"] <= lim else FAIL, round(r["gate_value"], 4),
                           "<= %g levels" % lim, note))
    elif kind == "pan" and len(pan_stack) >= 3:
        Hh, Ww = pan_stack[0].shape
        off, axis_name = _pan_offsets(meta, Ww, Hh, len(pan_stack))
        if off is None:
            step = float(meta.get("panPxPerFrame", 0.5))
            off = np.arange(len(pan_stack)) * step
            note = "offsets assumed %g px/frame along x (no yaw/fov per frame)" % step
            axis_name = "x"
        else:
            note = "offsets from %s and vFOV, total %.2f px" % ("pitch" if axis_name == "y" else "yaw", float(off[-1] - off[0]))
        ax = 1 if axis_name == "x" else 0
        comp = np.stack([_shift_linear(f, float(d), ax) for f, d in zip(pan_stack, off)])
        valid = ~np.isnan(comp).any(axis=0)
        sd = np.where(valid, np.nanstd(comp, axis=0), np.nan)
        bm = []
        for y in range(0, Hh - bsz + 1, bsz):
            for x in range(0, Ww - bsz + 1, bsz):
                blk = sd[y:y + bsz, x:x + bsz]
                if np.isfinite(blk).mean() > 0.5:
                    bm.append(float(np.nanmean(blk)))
        worst = max(bm) if bm else float(np.nanmean(sd))
        strict = beat == 6 or ledge
        lim = float(meta.get("panStdMax", th["pan_std_strict"] if strict else th["pan_std_max"]))
        S["pan_std_block_max"] = worst
        C.append(check("pan shimmer std", PASS if worst <= lim else FAIL, round(worst, 4), "<= %g levels" % lim,
                       note + ("; strict (beat 6 / ledge)" if strict else "")))
    # static flicker + fireflies
    if kind == "static" and len(blocks) >= 3:
        stack = np.stack(blocks)
        raw = float(stack.std(axis=0).max())
        # 14.10.5 lead ruling (fix wave X2): flicker is temporal change above 1 Hz (the 14.2.5 photosensitivity
        # bound). The slow swell (Tp 8-11 s), drifting plumes and the kraken (<= 8 m/s) are content, not flicker:
        # every 64 px block series is high-passed by subtracting its centred 0.5 s moving average (edges: the
        # shortened window), then the same limit applies. The raw std stays in the report.
        fps_s = float(seq["fps"] or 30.0)
        win = max(3, int(round(0.5 * fps_s)) | 1)
        nfr = stack.shape[0]
        if nfr > win:
            k = np.ones(win) / win
            pad = win // 2
            padded = np.pad(stack, ((pad, pad), (0, 0), (0, 0)), mode="edge")
            csum = np.cumsum(np.concatenate([np.zeros((1,) + stack.shape[1:]), padded], axis=0), axis=0)
            trend = (csum[win:] - csum[:-win]) / win
            bstd = (stack - trend[:nfr]).std(axis=0)
        else:
            bstd = stack.std(axis=0)
        worst = float(bstd.max())
        lim = float(meta.get("flickerStdMax", th["flicker_std_strict"] if ledge else th["flicker_std_max"]))
        S["flicker_block_std_max"] = worst
        S["flicker_block_std_raw_max"] = raw
        C.append(check("flicker block std", PASS if worst <= lim else FAIL, round(worst, 4), "<= %g levels" % lim,
                       "%d frames, %d px blocks, high-passed above 1 Hz (raw %.2f)%s" % (nfr, bsz, raw, "" if nfr >= th["flicker_frames"] else
                                                      " (fewer than %d frames)" % th["flicker_frames"])))
        nf = int(sum(fireflies))
        C.append(check("fireflies", PASS if nf == 0 else FAIL, nf, "0 px",
                       "frames with any: %d" % sum(1 for v in fireflies if v)))
    if "maxPixelDiff" in meta:
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
    res["status"] = qc.worst_status(C)
    return res


def build_markdown(run, results):
    allc = [c for r in results for c in r["checks"]]
    cnt = qc.count_status(allc)
    L = ["# Frame sequences: %s" % run, "",
         "Sequences: %d. Checks: %d PASS, %d FAIL, %d WARN, %d SKIP." % (len(results), cnt[PASS], cnt[FAIL],
                                                                       cnt[WARN], cnt[SKIP]), "",
         "**Result: %s**" % (FAIL if cnt[FAIL] else (PASS if results else SKIP)), "", "## Summary", ""]
    rows = []
    for r in results:
        S = r.get("summary", {})
        rows.append([r["name"], r["kind"], r["n_frames"], qc.fmt(S.get("max_luma_step_levels"), 3), qc.fmt(S.get("mean_diff")),
                     qc.fmt(S.get("max_diff")), qc.fmt(S.get("max_pixel_diff"), 1), r["status"],
                     ", ".join(c["name"] for c in r["checks"] if c["status"] == FAIL)])
    L.append(qc.md_table(["Sequence", "Kind", "Frames", "Max luma step (levels)", "Mean diff", "Max diff", "Max px diff",
                          "Result", "Failed checks"], rows))
    for r in results:
        L += ["## %s (%s)" % (r["name"], r["status"]), "", "`%s`, %d frames at %g fps, kind %s" % (
            r["dir"], r["n_frames"], r["fps"], r["kind"]), ""]
        if r.get("series"):
            L += ["- luma      `%s`" % sparkline(r["series"].get("luma", [])),
                  "- mean diff `%s`" % sparkline(r["series"].get("mean_abs_diff", [])), ""]
        L.append(qc.md_table(["Check", "Status", "Value", "Threshold", "Note"],
                             [[c["name"], c["status"], qc.fmt(c["value"], 4), c["threshold"] or "", c["note"] or ""]
                              for c in r["checks"]]))
    return "\n".join(L)


def run(paths, run_name=None, out_dir=qc.REPORTS_DIR, quiet=False):
    seq_dirs = qc.resolve_sequence_dirs(paths)
    if run_name is None:
        base = paths[0] if paths else "."
        b = os.path.abspath(base)
        run_name = qc.run_id_for(os.path.dirname(b) if os.path.basename(b).lower().startswith("seq_") else b)
    results = [analyse_sequence(qc.load_sequence(d), TH, quiet) for d in seq_dirs]
    allc = [c for r in results for c in r["checks"]]
    status = FAIL if any(c["status"] == FAIL for c in allc) else (PASS if results else SKIP)
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
    a = ap.parse_args(argv)
    missing = [p for p in a.paths if not os.path.isdir(p)]
    if missing:
        print("not found: %s" % ", ".join(missing), file=sys.stderr)
        return 1
    r = run(a.paths, a.run, a.out_dir, a.quiet)
    print("[sequences] %s: %d sequences, %d FAIL checks -> %s" % (r["status"], r["n"], len(r["fails"]), r["md"]))
    for f in r["fails"][:30]:
        print("  FAIL " + f)
    return 1 if r["status"] == FAIL else 0


if __name__ == "__main__":
    sys.exit(main())
