#!/usr/bin/env python3
"""seq_extra: the QA fix-wave gates of Tools/qa/sequences.py (Logs/fix/QA.md C20-C24), kept apart from the older checks.

  - run conditions (C20): kinds whose gate measures the camera alone (texpan, dolly, static, pan) are REFUSED (FAIL "run
    conditions", no analysis) unless sequence.json says -rain 0 and -freeze-clock;
  - block pops and blinks (C21): per 32 px and 64 px block (sizes scale with the frame height, 1080p reference) the mean |frame
    difference| of every consecutive pair; a pop is a one-frame jump (> 3 x both neighbouring pairs and > 2 levels), a blink is a
    two-frame out-and-back (two opposite block-mean changes > 2 levels that return within half of the smaller, both > 3 x the pairs
    around them) that is also local by AL's sign rule (the frame departs from both neighbours in the same direction by
    min(|d1|, |d2|) > 2 levels and > 3 x the median of the 8 neighbour blocks; else an INFO "wide" blink); reported with frame, s and block; rects in sequence.json "mask" / "popMask" (1080p px) are excluded, and so are
    events explained by motion (every pixel inside the [min, max] of the other frame's +-2 px neighbourhood up to a mean excess under
    min(2 levels, 0.35 x the event's): a 1-2 px strip stepping a pixel row under the sway; else the block's content found in the
    previous / next frame within +-64 px under the same bound: a near sign sweeping through a block in one frame) or by rain streaks (steep
    elongated changed components, see classify_rain);
  - photosensitivity G1-G7 of R4 3.3 / D-010 on the final 8-bit frames (C22): 16 px linear block means (IEC sRGB LUT), the zig-zag
    transition tracker (threshold 0.1, darker side < 0.8: every two consecutive transitions are a pair of opposing changes, the
    WCAG 2.x general flash; a single monotonic change is one transition, never a flash); G1 (D-041 1) the flashing area, blocks
    with a pair inside the trailing 1/3 s, < 25 % of the screen; the R4 1 s pair area beside it ("G1 slow pairs": gated on a static
    camera, INFO when the camera moves, where content sweeping through a block over up to 1 s is camera motion, D-041 1);
    G2 WCAG general (> 6 transitions, i.e. > 3 flashes, in the trailing 1 s in any 1/3 x 1/3 window) < 25 %, G3 the same for transitions to / from saturated red, G4 saturated-red
    area <= 10 % of any window, G5 dominant block frequency <= 0.5 Hz (amplitude floor 0.002; per-bin RMS above 0.5 Hz <= 0.002;
    gated on a static camera, INFO when the camera moves), G6 mean-luma step <= 0.3 % / frame, G7 linear red share (blocks with
    Y > 0.02 and share > 0.70 <= 2 % per frame; p99.9 of the share over blocks with Y > 0.01 <= 0.70 on a static camera), and the
    wall rule mean change <= 4 % / s (static camera); G8 needs the 4K KS16 still (image_metrics), not a sequence;
  - rain image gate H5 (C24): streak count per frame 140-210, coverage 0.6-1.0 %, no streak shorter than 10 px (1080p; scaled),
    from the undilated drop mask (luma > temporal min + 4 codes: the drops, not the far veil), connected components (8-connected, area >= 3 px, those
    touching the frame border left out of the length test); needs -rain 1 -rain-noise-off (D-020), else SKIP;
  - montage (C21): every 10th frame in 4 x 6 pages (at most 4 pages; the stride grows for long sequences) and the worst pop / blink
    blocks at 4 x (frames before / at / after), written into the sequence folder, plus an x264 clip for the boundary, cinematic and
    dread kinds; then (--delete-frames) the PNG frames are deleted and a done marker written, in one sequential job.
"""
import json
import math
import os
import subprocess
import time

import numpy as np

import qa_common as qc
from qa_common import PASS, FAIL, SKIP, INFO, WARN, check

# ---------------------------------------------------------------------------------------------------------- configuration
POP_KINDS = {"boundary": FAIL, "pan": FAIL, "cinematic": FAIL, "dread": FAIL, "signs": FAIL, "koi": WARN, "traffic": WARN,
             "encode": WARN}
PHOTO_KINDS = ("signs", "dread", "cinematic")
CLIP_KINDS = ("boundary", "cinematic", "dread")
# kind -> (rain override, freeze clock) the gate needs (C20)
CONDITIONS = {"texpan": (0.0, True), "dolly": (0.0, True), "static": (0.0, True), "pan": (0.0, True)}
POP_RATIO, POP_FLOOR, POP_NB_FLOOR = 3.0, 2.0, 0.25
POP_BLOCKS_1080 = (32, 64)
MOTION_R_1080, MOTION_FRAC = 64, 0.35     # motion search radius (px at 1080p) and the share of the unshifted difference
MOTION_ENV_1080 = 2                       # sub-pixel motion: the +-2 px min / max envelope of the other frame
PHOTO_BLOCK_1080 = 16
FLASH_T, DARK_MAX = 0.1, 0.8
G1_AREA, G2_AREA, G4_AREA = 0.25, 0.25, 0.10
G1_WINDOW_S = 1.0 / 3.0                   # D-041 (1): a flash is a pair of opposing changes counted per 1/3 s window
G5_FLOOR, G5_FMAX, G5_RMS = 0.002, 0.5, 0.002
G6_STEP = 0.003
G7_SHARE, G7_FRAC, G7_Y, G7_Y_P999 = 0.70, 0.02, 0.02, 0.01
SAT_RED = 0.8
WALL_PER_S = 0.04
RAIN_COUNT = (140, 210)
RAIN_COVER_PCT = (0.6, 1.0)
RAIN_MIN_LEN_1080 = 10.0
# a drop streak is >= 4 codes above the rain-free background: 14.7 H5's drop adds 0.35 (0.04 + 0.9 clamp(Lloc / 0.8)) (about 10-20
# codes on the dark beat-4 frame) while the far veil adds "3 % vertical streak noise" (1-2 codes), which the 180-streak budget excludes;
# the 1.5-code mask of the flicker test (which must contain the veil) is reported beside it as INFO
RAIN_DROP_CODES = 4.0
MONTAGE_STRIDE, MONTAGE_COLS, MONTAGE_ROWS, MONTAGE_PAGES = 10, 4, 6, 4

_LUT = None


def srgb_lut():
    global _LUT
    if _LUT is None:
        c = np.arange(256, dtype=np.float64) / 255.0
        _LUT = np.where(c <= 0.04045, c / 12.92, ((c + 0.055) / 1.055) ** 2.4).astype(np.float32)
    return _LUT


def block_size(H, k1080):
    return max(2, int(round(k1080 * H / 1080.0)))


def block_mean(a, k):
    H, W = a.shape[:2]
    h, w = H // k, W // k
    if a.ndim == 2:
        return a[:h * k, :w * k].reshape(h, k, w, k).mean(axis=(1, 3))
    return a[:h * k, :w * k].reshape(h, k, w, k, a.shape[2]).mean(axis=(1, 3))


def _pair_gap_stats(trans, flagged, fps):
    """Diagnostics of the 1 s pairs (the D-041 1 evidence): for each block-frame flagged with >= 2 transitions in the trailing 1 s,
    the time between its last two transitions; a histogram in 1/6 s bins and the share inside 1/3 s."""
    n, h, w = trans.shape
    last = np.full((h, w), -10 ** 6, np.int64)
    prev = np.full((h, w), -10 ** 6, np.int64)
    gaps = []
    for t in range(n):
        tr = trans[t]
        prev[tr] = last[tr]
        last[tr] = t
        f = flagged[t] & tr
        if f.any():
            gaps.append((last[f] - prev[f]).astype(np.float64) / fps)
    if not gaps:
        return {"pairs_closed": 0}
    g = np.concatenate(gaps)
    edges = [0, 1 / 6, 1 / 3, 1 / 2, 2 / 3, 5 / 6, 1.0001]
    hist, _ = np.histogram(g, bins=edges)
    return {"pairs_closed": int(g.size), "share_within_1_3_s": round(float((g <= G1_WINDOW_S + 1e-9).mean()), 4),
            "median_gap_s": round(float(np.median(g)), 3), "hist_1_6_s_bins": [int(v) for v in hist]}


def _neighbour_median(v, h, w):
    """Per row of v (frames x blocks, blocks in h x w raster order): the median of each block's 8 neighbours (edge blocks: the
    neighbours that exist)."""
    n = v.shape[0]
    g = v.reshape(n, h, w)
    p = np.pad(g, ((0, 0), (1, 1), (1, 1)), constant_values=np.nan)
    nb = np.stack([p[:, 1 + dy:1 + dy + h, 1 + dx:1 + dx + w] for dy in (-1, 0, 1) for dx in (-1, 0, 1) if dy or dx], axis=-1)
    with np.errstate(all="ignore"):
        import warnings
        with warnings.catch_warnings():
            warnings.simplefilter("ignore", RuntimeWarning)
            m = np.nanmedian(nb, axis=-1)
    return np.nan_to_num(m, nan=0.0).reshape(n, h * w)


# ---------------------------------------------------------------------------------------------------------- run conditions
def conditions_check(kind, meta):
    """None when the kind has no condition or the conditions hold; else the FAIL check (the sequence is not analysed)."""
    cond = CONDITIONS.get(kind)
    if cond is None:
        return None
    want_rain, want_fz = cond
    rain, fz = meta.get("rainOverride"), meta.get("freezeClock")
    ok = rain is not None and abs(float(rain) - want_rain) < 1e-6 and bool(fz) == bool(want_fz)
    if ok:
        return None
    return check("run conditions", FAIL, "rain %s, freezeClock %s" % (rain, fz), "-rain %g -freeze-clock" % want_rain,
                 "refused, not analysed: the %s gate measures the camera alone (Logs/fix/QA.md C20); re-capture under these "
                 "conditions" % kind)


# ---------------------------------------------------------------------------------------------------------- accumulator
class Extra:
    """Per-frame accumulator of the block-pop and photosensitivity data (fed by sequences.analyse_sequence)."""

    def __init__(self, kind, meta, W, H, fps):
        self.kind, self.meta, self.W, self.H, self.fps = kind, meta, W, H, float(fps or 30.0)
        self.pop = kind in POP_KINDS
        self.photo = kind in PHOTO_KINDS
        self.kp = [block_size(H, k) for k in POP_BLOCKS_1080]
        self.D = {k: [] for k in self.kp}     # pairs x blocks: mean |diff|
        self.M = {k: [] for k in self.kp}     # frames x blocks: signed mean grey
        self.kf = block_size(H, PHOTO_BLOCK_1080)
        self.Y, self.RS = [], []              # frames x blocks: linear Y, red share
        self.Ymean = []
        self.idx = []

    def add(self, idx, rgb8, g, d):
        """g: grey levels (H x W float); d: |g - prev| or None for the first frame."""
        self.idx.append(idx)
        if self.pop:
            for k in self.kp:
                self.M[k].append(block_mean(g, k).astype(np.float32).ravel())
                if d is not None:
                    self.D[k].append(block_mean(d, k).astype(np.float32).ravel())
        if self.photo:
            lin = srgb_lut()[rgb8]
            b = block_mean(lin, self.kf)                     # h x w x 3
            Y = 0.2126 * b[..., 0] + 0.7152 * b[..., 1] + 0.0722 * b[..., 2]
            s = b.sum(axis=2)
            self.Y.append(Y.astype(np.float32))
            self.RS.append(np.where(s > 1e-6, b[..., 0] / np.maximum(s, 1e-9), 0.0).astype(np.float32))
            self.Ymean.append(float(Y.mean()))

    # ------------------------------------------------------------------ block pops (C21)
    def _excluded(self, k):
        h, w = self.H // k, self.W // k
        ex = np.zeros((h, w), bool)
        sx, sy = self.W / 1920.0, self.H / 1080.0
        for r in (self.meta.get("popMask") or []) + (self.meta.get("mask") or []):
            try:
                x0, y0, x1, y1 = [float(v) for v in r]
            except Exception:
                continue
            ex[max(0, int(y0 * sy) // k):int(math.ceil(y1 * sy / k)), max(0, int(x0 * sx) // k):int(math.ceil(x1 * sx / k))] = True
        return ex.ravel()

    def pop_events(self, k):
        D = np.asarray(self.D[k], dtype=np.float64)
        M = np.asarray(self.M[k], dtype=np.float64)
        ev = []
        if D.ndim != 2 or D.shape[0] < 3:
            return ev, D.shape[0] if D.ndim == 2 else 0
        n = D.shape[0]
        ex = self._excluded(k)
        prv = np.vstack([np.full((1, D.shape[1]), np.nan), D[:-1]])
        nxt = np.vstack([D[1:], np.full((1, D.shape[1]), np.nan)])
        nb = np.fmax(np.fmax(prv, nxt), POP_NB_FLOOR)
        hit = (D > POP_RATIO * nb) & (D > POP_FLOOR) & ~ex[None, :]
        for i, b in zip(*np.nonzero(hit)):
            ev.append({"type": "pop", "pair": int(i), "frame": int(i) + 1, "block": int(b), "k": k,
                       "diff": float(D[i, b]), "neighbours": float(nb[i, b])})
        # blinks: out at pair i, back at pair i + 1
        if M.shape[0] >= 3 and n >= 2:
            dm = np.diff(M, axis=0)                              # pairs x blocks, signed
            a, c = dm[:-1], dm[1:]
            ret = np.abs(M[2:] - M[:-2])
            prv2 = np.vstack([np.full((1, D.shape[1]), np.nan), D[:-2]]) if n >= 3 else np.full_like(a, np.nan)
            nxt2 = np.vstack([D[2:], np.full((1, D.shape[1]), np.nan)])
            nbo = np.fmax(np.fmax(prv2, nxt2), POP_NB_FLOOR)
            hb = ((np.abs(a) > POP_FLOOR) & (np.abs(c) > POP_FLOOR) & (np.sign(a) == -np.sign(c)) &
                  (ret < 0.5 * np.minimum(np.abs(a), np.abs(c))) & (D[:-1] > POP_RATIO * nbo) & (D[1:] > POP_RATIO * nbo) &
                  ~ex[None, :])
            # AL's sign rule (C21 request): count a blink only where frame i departs from both neighbours in the same direction,
            # blink = min(|m_i - m_i-1|, |m_i - m_i+1|) (0 otherwise: a bar sweeping through the block steps once and scores 0),
            # flagged above 2 levels and above 3 x the median of the 8 neighbour blocks' blink (a local blink). A restriction of the
            # out-and-back rule above (both must hold): the union counted every light moving through a dark block (INTB_cin s260:
            # 9 015 blinks against 224). Out-and-back blinks whose neighbours blink too ("wide": a frame-wide or area change, the
            # photosensitivity gates' case) are kept as INFO events, not dropped silently.
            d1, d2 = M[1:-1] - M[:-2], M[1:-1] - M[2:]
            bl = np.where(d1 * d2 > 0, np.minimum(np.abs(d1), np.abs(d2)), 0.0)
            nbm = _neighbour_median(bl, self.H // k, self.W // k)
            hs = (bl > POP_FLOOR) & (bl > POP_RATIO * np.maximum(nbm, POP_NB_FLOOR)) & ~ex[None, :]
            for i, b in zip(*np.nonzero(hb)):
                ev.append({"type": "blink", "pair": int(i), "frame": int(i) + 1, "block": int(b), "k": k,
                           "diff": float(max(abs(a[i, b]), abs(c[i, b]))), "neighbours": float(nbo[i, b]),
                           "out_back": [round(float(a[i, b]), 2), round(float(c[i, b]), 2)],
                           "sign_blink": round(float(bl[i, b]), 2), "nb8_median": round(float(nbm[i, b]), 2),
                           "rule": "temporal+sign" if hs[i, b] else "temporal (wide)", "wide": not bool(hs[i, b])})
        return ev, n

    def classify_rain(self, events, frames):
        """Marks events explained by rain streaks (C21: "excluding the rain masks"): the changed pixels (|difference| > 3 levels
        against the frame before, for a blink against the mean of both neighbours) of the block and its 8 neighbours lie for >= 60 %
        (by |difference|, components of >= 6 px) in steep, elongated components (height >= 6 px at 1080p and >= 2 x the width, in the frame or in a
        neighbour): the streaks of NocRain. A thin vertical feature that blinks would be taken for rain: the montage shows it. Horizontal lines that crawl, lights that twinkle and pops of whole objects stay events."""
        import cv2
        cache = {}

        def grey(fi):
            if fi not in cache:
                if len(cache) > 12:
                    cache.pop(next(iter(cache)))
                cache[fi] = qc.grey_levels(qc.load_rgb8(frames[fi][1])[0]).astype(np.float32)
            return cache[fi]
        lmin = 6.0 * self.H / 1080.0
        R = int(round(MOTION_R_1080 * self.H / 1080.0))
        env = max(1, int(round(MOTION_ENV_1080 * self.H / 1080.0)))
        n = len(frames)
        for e in sorted(events, key=lambda e: e["frame"]):
            f = e["frame"]
            if f < 1 or f >= n:
                continue
            x0, y0, x1, y1 = e["block_px"]
            k = x1 - x0
            # motion: the block's new content is the old content moved (a near sign sweeping through the block at 2.5 m/s moves
            # 30+ px per frame, so the block changes in one frame): search the previous frame (and for a blink the next one) for the
            # block within +-R px; a match better than MOTION_FRAC of the unshifted difference and under POP_FLOOR is motion
            pairs = [(f, f - 1)] + ([(f, f + 1)] if e["type"] == "blink" and f + 1 < n else [])
            best = []
            for a_, b_ in pairs:
                blk = grey(a_)[y0:y1, x0:x1]
                # sub-pixel motion first: every pixel of the new frame inside the [min, max] envelope of the other frame's
                # +-ENV px neighbourhood (a 1-2 px strip that steps a pixel row as the camera sways is explained; a lamp that
                # switches on is not)
                ex0, ey0, ex1, ey1 = max(0, x0 - env), max(0, y0 - env), min(self.W, x1 + env), min(self.H, y1 + env)
                o = grey(b_)[ey0:ey1, ex0:ex1]
                ker = np.ones((2 * env + 1, 2 * env + 1), np.uint8)
                hi = cv2.dilate(o, ker)[y0 - ey0:y1 - ey0, x0 - ex0:x1 - ex0]
                lo = cv2.erode(o, ker)[y0 - ey0:y1 - ey0, x0 - ex0:x1 - ex0]
                une = float((np.maximum(0.0, blk - hi) + np.maximum(0.0, lo - blk)).mean())
                if une < min(POP_FLOOR, MOTION_FRAC * e["diff"]):
                    best.append(une)
                    continue
                X0m, Y0m, X1m, Y1m = max(0, x0 - R), max(0, y0 - R), min(self.W, x1 + R), min(self.H, y1 + R)
                reg = grey(b_)[Y0m:Y1m, X0m:X1m]
                if reg.shape[0] < k or reg.shape[1] < k:
                    best.append(1e9)
                    continue
                win = np.lib.stride_tricks.sliding_window_view(reg, (k, k))
                st = max(1, k // 8)
                sad = np.abs(win[::st, ::st] - blk[None, None]).mean(axis=(2, 3))
                iy, ix = np.unravel_index(int(np.argmin(sad)), sad.shape)
                cy, cx = iy * st, ix * st
                ys0, ys1 = max(0, cy - st), min(win.shape[0], cy + st + 1)
                xs0, xs1 = max(0, cx - st), min(win.shape[1], cx + st + 1)
                fine = np.abs(win[ys0:ys1, xs0:xs1] - blk[None, None]).mean(axis=(2, 3))
                best.append(float(fine.min()))
            e["motion_sad"] = round(max(best), 3)
            if max(best) < min(POP_FLOOR, MOTION_FRAC * e["diff"]):
                e["motion"] = True
                continue
            X0, Y0, X1, Y1 = max(0, x0 - k), max(0, y0 - k), min(self.W, x1 + k), min(self.H, y1 + k)
            gf = grey(f)[Y0:Y1, X0:X1]
            if e["type"] == "blink" and f + 1 < n:
                ref = 0.5 * (grey(f - 1)[Y0:Y1, X0:X1] + grey(f + 1)[Y0:Y1, X0:X1])
            else:
                ref = grey(f - 1)[Y0:Y1, X0:X1]
            ds = gf - ref
            d = np.abs(ds)
            m = (d > 3.0).astype(np.uint8)
            nc, lab, st, _ = cv2.connectedComponentsWithStats(m, connectivity=8)
            rain_w, tot = 0.0, 0.0
            for c in range(1, nc):
                w_, h_ = st[c, cv2.CC_STAT_WIDTH], st[c, cv2.CC_STAT_HEIGHT]
                if st[c, cv2.CC_STAT_AREA] < 6:
                    continue   # specks (dither, the soft tails of a streak) decide nothing
                sel = lab == c
                tot += float(d[sel].sum())
                # a streak: steep and elongated (near streaks are 6-8 px wide and soft), in this frame or in a neighbour
                if h_ >= lmin and h_ >= 2.0 * w_:
                    rain_w += float(d[sel].sum())
            if tot <= 0:
                continue
            e["rain_share"] = round(rain_w / tot, 3)
            e["rain"] = rain_w >= 0.6 * tot

    def pop_checks(self, res, frame_s, frames=None):
        if not self.pop:
            return
        C = res["checks"]
        allev = []
        for k in self.kp:
            ev, n = self.pop_events(k)
            w = self.W // k
            for e in ev:
                by, bx = divmod(e["block"], w)
                e["block_px"] = [bx * k, by * k, (bx + 1) * k, (by + 1) * k]
                f = e["frame"]
                e["s"] = round(float(frame_s[f]), 2) if frame_s and f < len(frame_s) and frame_s[f] is not None else None
            allev += ev
        rain_n = 0
        if frames is not None and allev:
            try:
                self.classify_rain(allev, frames)
            except Exception as ex_:  # pragma: no cover
                res["rain_classify_error"] = "%s: %s" % (type(ex_).__name__, ex_)
            rain_n = sum(1 for e in allev if e.get("rain"))
            mot_n = sum(1 for e in allev if e.get("motion"))
            res["summary"]["pop_rain_excluded"] = rain_n
            res["summary"]["pop_motion_excluded"] = mot_n
            allev = [e for e in allev if not e.get("rain") and not e.get("motion")]
            rain_n = "%d rain-streak and %d motion-explained" % (rain_n, mot_n)
        # AL's sign rule (C21): an out-and-back whose 8 neighbour blocks blink as much is not a local blink (INFO, listed)
        wide = [e for e in allev if e.get("wide")]
        allev = [e for e in allev if not e.get("wide")]
        res["summary"]["blink_wide_events"] = len(wide)
        if wide:
            wide.sort(key=lambda e: -e["diff"])
            C.append(check("wide blinks (neighbour blocks blink too, AL's sign rule)", INFO, len(wide), None,
                           "not gated as block blinks (area changes are the photosensitivity gates' case); worst %s" % "; ".join(
                               "f%d s%s %dpx@%s %.1f (nb8 median %.1f)" % (e["frame"], e["s"], e["k"], e["block_px"][:2], e["diff"],
                                                                         e.get("nb8_median", 0.0)) for e in wide[:4])))
        allev.sort(key=lambda e: -(e["diff"] / max(e["neighbours"], POP_NB_FLOOR)))
        # C20: the first pair after the pose jump into the sequence is a start transient of the harness (dolly_face30 frame 1 also
        # under -rain 0 -freeze-clock): listed apart (INFO), not gated
        start = [e for e in allev if e["frame"] == 1]
        allev = [e for e in allev if e["frame"] != 1]
        if start:
            C.append(check("start transient (frame 1, after the pose jump)", INFO, len(start), None,
                           "not gated (C20); worst %s" % "; ".join("%s %dpx@%s %.1f/%.2f" % (e["type"], e["k"], e["block_px"][:2], e["diff"], e["neighbours"])
                                                              for e in start[:4])))
            res["pops_start"] = start[:10]
        res["pops"] = allev[:40]
        # every gated event in compact form (frame, block size, x, y, type, levels), so a finding can be confirmed or cleared at its
        # frame and block after the frames are deleted
        res["pops_index"] = [[e["frame"], e["k"], e["block_px"][0], e["block_px"][1], e["type"][0], round(e["diff"], 1)]
                             for e in sorted(allev, key=lambda e: e["frame"])[:20000]]
        res["summary"]["pop_events"] = sum(1 for e in allev if e["type"] == "pop")
        res["summary"]["blink_events"] = sum(1 for e in allev if e["type"] == "blink")
        st = POP_KINDS[self.kind]
        for typ, label in (("pop", "block pops (one-frame jumps)"), ("blink", "block blinks (out and back)")):
            ev = [e for e in allev if e["type"] == typ]
            frames_ = sorted(set(e["frame"] for e in ev))
            worst = ["f%d s%s %dpx@%s %.1f/%.2f" % (e["frame"], e["s"], e["k"], e["block_px"][:2], e["diff"], e["neighbours"])
                     for e in ev[:5]]
            C.append(check("%s, %s px blocks" % (label, "/".join(str(k) for k in self.kp)), PASS if not ev else st, len(ev),
                           "0 (> %gx both neighbouring pairs and > %g levels)" % (POP_RATIO, POP_FLOOR),
                           ("frames %s; worst %s; %s events excluded" % (frames_[:12], "; ".join(worst), rain_n)) if ev
                           else "%d frames; %s events excluded" % (len(self.idx), rain_n)))

    # ------------------------------------------------------------------ photosensitivity (C22)
    def photo_checks(self, res, luma_display, static_cam):
        if not self.photo or len(self.Y) < 2:
            return
        C, S = res["checks"], res["summary"]
        F = int(round(self.fps))
        Y = np.stack(self.Y)                                   # n x h x w
        RS = np.stack(self.RS)
        n, h, w = Y.shape
        # zig-zag transitions
        ext = Y[0].copy()
        lo, hi = Y[0].copy(), Y[0].copy()
        dirn = np.zeros((h, w), np.int8)
        trans = np.zeros((n, h, w), bool)
        redtr = np.zeros((n, h, w), bool)
        last_red = RS[0] >= SAT_RED
        for t in range(1, n):
            y = Y[t]
            # undecided blocks
            u = dirn == 0
            np.minimum(lo, y, out=lo, where=u)
            np.maximum(hi, y, out=hi, where=u)
            up0 = u & (y - lo >= FLASH_T) & (np.minimum(lo, y) < DARK_MAX)
            dn0 = u & (hi - y >= FLASH_T) & (np.minimum(hi, y) < DARK_MAX)
            # rising: track the peak, a fall of T from it is a transition
            r = dirn == 1
            np.maximum(ext, y, out=ext, where=r)
            dn1 = r & (ext - y >= FLASH_T) & (y < DARK_MAX)
            f = dirn == -1
            np.minimum(ext, y, out=ext, where=f)
            up1 = f & (y - ext >= FLASH_T) & (ext < DARK_MAX)
            tr = up0 | dn0 | dn1 | up1
            trans[t] = tr
            red_now = RS[t] >= SAT_RED
            redtr[t] = tr & (red_now | last_red)
            last_red = np.where(tr, red_now, last_red)
            dirn[up0 | up1] = 1
            dirn[dn0 | dn1] = -1
            ext[tr] = y[tr]
        # trailing counts: 1 s (G2, the R4 slow-pair row) and 1/3 s (G1: a flash is a pair of opposing changes inside a 1/3 s
        # window, D-041 1; a > 3 Hz flash always has its pair there)
        cs = np.cumsum(trans, axis=0, dtype=np.int16)

        def trailing(c, frames):
            return c - np.concatenate([np.zeros((min(frames, n), h, w), np.int16), c[:-frames]], axis=0) if n > frames else c
        cnt = trailing(cs, F)
        F3 = max(2, int(round(self.fps * G1_WINDOW_S)))
        cnt3 = trailing(cs, F3)
        rcs = np.cumsum(redtr, axis=0, dtype=np.int16)
        rcnt = trailing(rcs, F)
        flash_area = (cnt3 >= 2).reshape(n, -1).mean(axis=1)
        slow_area = (cnt >= 2).reshape(n, -1).mean(axis=1)
        g1 = float(flash_area.max())
        g1f = int(np.argmax(flash_area))
        g1s = float(slow_area.max())
        g1sf = int(np.argmax(slow_area))
        S["photo_g1_flash_area_max"] = g1
        S["photo_g1_slow_pair_area_max"] = g1s
        S["photo_transitions"] = int(trans.sum())
        n_flash_frames = int((flash_area >= G1_AREA).sum())
        C.append(check("G1 flashes (area with a pair of opposing changes in 1/3 s)", PASS if g1 < G1_AREA else FAIL, round(100 * g1, 2),
                       "< %g %% of the screen" % (100 * G1_AREA),
                       "worst frame %d; %d frames over; a pair = two consecutive opposing transitions of >= %g (darker < %g) inside %d "
                       "frames (1/3 s, WCAG 2.x general flash, D-041 1); %d block transitions in all; %d px linear blocks" % (
                           g1f, n_flash_frames, FLASH_T, DARK_MAX, F3, S["photo_transitions"], self.kf)))
        st1s = (PASS if g1s < G1_AREA else FAIL) if static_cam else INFO
        C.append(check("G1 slow pairs (area with a rise and a fall in 1 s, R4)", st1s, round(100 * g1s, 2),
                       "< %g %% of the screen (static camera)" % (100 * G1_AREA),
                       "worst frame %d; %s" % (g1sf, "the camera is fixed: a pair in 1 s is a light change (gated)" if static_cam else
                                               "the camera moves: content sweeping through a block over up to 1 s is camera "
                                               "motion, not a flash (D-041 1); the 1/3 s G1 above gates")))
        res["photo_pair_gaps"] = _pair_gap_stats(trans, cnt >= 2, self.fps)
        wh, ww = max(1, h // 3), max(1, w // 3)

        def window_max(mask2d):
            if not mask2d.any():
                return 0.0
            ii = np.pad(mask2d.astype(np.int32).cumsum(0).cumsum(1), ((1, 0), (1, 0)))
            s = ii[wh:, ww:] - ii[:-wh, ww:] - ii[wh:, :-ww] + ii[:-wh, :-ww]
            return float(s.max()) / float(wh * ww)

        g2 = max(window_max(cnt[t] >= 7) for t in range(n)) if (cnt >= 7).any() else 0.0
        g3 = max(window_max(rcnt[t] >= 7) for t in range(n)) if (rcnt >= 7).any() else 0.0
        sat = (RS >= SAT_RED) & (Y > 0.002)
        g4 = max(window_max(sat[t]) for t in range(n)) if sat.any() else 0.0
        S.update({"photo_g2": g2, "photo_g3": g3, "photo_g4": g4})
        C.append(check("G2 WCAG general (> 6 transitions / s in a 1/3 window)", PASS if g2 < G2_AREA else FAIL, round(100 * g2, 2),
                       "< %g %% of the window" % (100 * G2_AREA)))
        C.append(check("G3 WCAG red (saturated-red transitions)", PASS if g3 < G2_AREA else FAIL, round(100 * g3, 2),
                       "< %g %% of the window" % (100 * G2_AREA), "saturated red: R / (R + G + B) >= %g linear" % SAT_RED))
        C.append(check("G4 saturated-red area", PASS if g4 <= G4_AREA else FAIL, round(100 * g4, 2), "<= %g %% of any window" % (100 * G4_AREA)))
        # G5: dominant block frequency
        if n >= 2 * F:
            x = np.arange(n, dtype=np.float64)
            Yf = Y.reshape(n, -1).astype(np.float64)
            A = np.vstack([x, np.ones(n)]).T
            coef, *_ = np.linalg.lstsq(A, Yf, rcond=None)
            r = Yf - A @ coef
            wv = np.hanning(n)[:, None]
            X = np.fft.rfft(r * wv, axis=0)
            amp = 2.0 * np.abs(X) / wv.sum()
            fr = np.fft.rfftfreq(n, 1.0 / self.fps)
            amp[0] = 0.0
            mod = amp.max(axis=0) >= G5_FLOOR
            dom = fr[np.argmax(amp, axis=0)]
            hiF = fr > G5_FMAX
            rms_hi = np.sqrt((amp[hiF] ** 2).mean(axis=0)) if hiF.any() else np.zeros(amp.shape[1])
            dmax = float(dom[mod].max()) if mod.any() else 0.0
            over = int((mod & (dom > G5_FMAX)).sum())
            rmax = float(rms_hi.max()) if rms_hi.size else 0.0
            S.update({"photo_g5_dominant_hz_max": dmax, "photo_g5_blocks_modulated": int(mod.sum()), "photo_g5_rms_hi_max": rmax})
            st5 = (PASS if (dmax <= G5_FMAX and rmax <= G5_RMS) else FAIL) if static_cam else INFO
            C.append(check("G5 dominant block frequency", st5, round(dmax, 4), "<= %g Hz (amplitude floor %g)" % (G5_FMAX, G5_FLOOR),
                           "%d of %d blocks modulated, %d over %g Hz; per-bin RMS above %g Hz max %.5f (<= %g); df %.4f Hz%s" % (
                               int(mod.sum()), mod.size, over, G5_FMAX, G5_FMAX, rmax, G5_RMS, self.fps / n,
                               "" if static_cam else "; the camera moves: content motion, not modulation (INFO)")))
        else:
            C.append(check("G5 dominant block frequency", SKIP if not static_cam else FAIL, n, ">= %d frames (2 s)" % (2 * F)))
        # G6: mean-luma step on 8-bit Rec.709 luma (codes)
        L = np.asarray(luma_display, dtype=np.float64) * 255.0
        rel = np.abs(np.diff(L)) / np.maximum(L[:-1], 1e-6) if L.size > 1 else np.zeros(0)
        g6 = float(rel.max()) if rel.size else 0.0
        S["photo_g6_step_pct"] = 100.0 * g6
        C.append(check("G6 mean-luma step", (PASS if g6 <= G6_STEP else FAIL) if static_cam else INFO, round(100 * g6, 4),
                       "<= %g %% / frame" % (100 * G6_STEP),
                       "at frame %d%s" % (int(np.argmax(rel)) + 1 if rel.size else 0,
                                          "" if static_cam else "; R4 states G6 for a fixed camera (KS16): the camera moves, the 14.10 "
                                          "luma-step check above gates this sequence (INFO)")))
        # G7: red share
        lit = Y > G7_Y
        frac = ((RS > G7_SHARE) & lit).reshape(n, -1).mean(axis=1)
        g7f = float(frac.max())
        sel = RS[Y > G7_Y_P999]
        p999 = float(np.percentile(sel, 99.9)) if sel.size else 0.0
        S.update({"photo_g7_red_block_frac_max": g7f, "photo_g7_share_p999": p999})
        st7 = PASS if g7f <= G7_FRAC else FAIL
        if static_cam and p999 > G7_SHARE:
            st7 = FAIL
        C.append(check("G7 red share", st7, round(p999, 4), "p99.9 <= %g (static); blocks over %g <= %g %% / frame" % (G7_SHARE, G7_SHARE, 100 * G7_FRAC),
                       "worst frame %.3f %% of the blocks with Y > %g over %g; share = R / (R + G + B) linear, %d px blocks" % (
                           100 * g7f, G7_Y, G7_SHARE, self.kf)))
        # the wall rule: mean change <= 4 % per second (static camera)
        ym = np.asarray(self.Ymean)
        if ym.size > F:
            ch = np.abs(ym[F:] - ym[:-F]) / np.maximum(ym[:-F], 1e-9)
            wmax = float(ch.max())
            S["photo_wall_change_per_s_pct"] = 100.0 * wmax
            C.append(check("mean change per second (linear)", (PASS if wmax <= WALL_PER_S else FAIL) if static_cam else INFO,
                           round(100 * wmax, 3), "<= %g %% / s" % (100 * WALL_PER_S),
                           "frame-mean linear Y, 1 s apart" + ("" if static_cam else "; the camera moves (INFO)")))
        C.append(check("G8 pattern contrast", SKIP, None, None, "a still test: the 4K KS16 still (image_metrics), not a sequence"))
        res["photo_series"] = {"flash_area": [round(float(v), 4) for v in flash_area[::max(1, n // 600)]],
                               "slow_pair_area": [round(float(v), 4) for v in slow_area[::max(1, n // 600)]]}


# ---------------------------------------------------------------------------------------------------------- rain H5 (C24)
def rain_streaks(rgb8, base, codes, H):
    import cv2  # analysis-only dependency, present on this machine
    y = (rgb8.astype(np.int32) @ np.array([2126, 7152, 722], dtype=np.int32)) // 10000
    m = ((y - base) > codes).astype(np.uint8)
    n, lab, st, _ = cv2.connectedComponentsWithStats(m, connectivity=8)
    if n <= 1:
        return 0, float(m.mean()), []
    st = st[1:]
    keep = st[:, cv2.CC_STAT_AREA] >= 3
    st = st[keep]
    Hh, Ww = m.shape
    inner = (st[:, 0] > 0) & (st[:, 1] > 0) & (st[:, 0] + st[:, 2] < Ww) & (st[:, 1] + st[:, 3] < Hh)
    lens = np.hypot(st[:, 2], st[:, 3])[inner]
    return int(st.shape[0]), float(m.mean()), lens.tolist()


def rain_checks(res, frames, base, meta, th):
    C, S = res["checks"], res["summary"]
    codes = RAIN_DROP_CODES
    counts, covers, lens = [], [], []
    veil_counts, veil_covers = [], []
    H = None
    for idx, path in frames:
        try:
            rgb8, _ = qc.load_rgb8(path)
        except Exception:
            continue
        if rgb8.shape[:2] != base.shape:
            continue
        H = rgb8.shape[0]
        c, cv, ln = rain_streaks(rgb8, base, codes, H)
        counts.append(c)
        covers.append(cv)
        lens += ln
        if len(veil_counts) < 12:
            vc, vcv, _ = rain_streaks(rgb8, base, float(th["rain_streak_codes"]), H)
            veil_counts.append(vc)
            veil_covers.append(vcv)
    if not counts:
        C.append(check("rain H5 streaks", FAIL, None, None, "no frame readable"))
        return
    lmin = RAIN_MIN_LEN_1080 * H / 1080.0
    med = float(np.median(counts))
    cov = 100.0 * float(np.mean(covers))
    la = np.asarray(lens) if lens else np.zeros(0)
    short = int((la < lmin).sum())
    S.update({"rain_streaks_median": med, "rain_streaks_min": int(min(counts)), "rain_streaks_max": int(max(counts)),
              "rain_coverage_pct": cov, "rain_len_min": float(la.min()) if la.size else None,
              "rain_len_p5": float(np.percentile(la, 5)) if la.size else None, "rain_short_streaks": short,
              "rain_short_share": short / max(1, la.size)})
    noise_off = meta.get("rainNoiseOff")
    rain = meta.get("rainOverride")
    cond_ok = noise_off is True and rain is not None and abs(float(rain) - 1.0) < 1e-6
    note = "" if cond_ok else " (sequence.json: rainOverride %s, rainNoiseOff %s: the H5 gate needs -rain 1 -rain-noise-off, D-020)" % (rain, noise_off)
    gate = (lambda ok: PASS if ok else FAIL) if cond_ok else (lambda ok: SKIP)
    C.append(check("rain H5 streak count per frame (median)", gate(RAIN_COUNT[0] <= med <= RAIN_COUNT[1]), med,
                   "%d-%d" % RAIN_COUNT, "min %d, max %d over %d frames; drop streaks = components of the undilated mask (luma > temporal "
                   "min + %g codes: drops, not the far veil; 8-connected, >= 3 px)%s" % (min(counts), max(counts), len(counts), codes, note)))
    C.append(check("rain H5 coverage", gate(RAIN_COVER_PCT[0] <= cov <= RAIN_COVER_PCT[1]), round(cov, 3),
                   "%g-%g %%" % RAIN_COVER_PCT, "undilated streak mask, mean over the frames" + note))
    if veil_counts:
        C.append(check("rain mask at the flicker threshold (drops + far veil)", INFO, float(np.median(veil_counts)), None,
                       "components at > %g codes, coverage %.3f %% (first %d frames): the far veil's streak noise included" % (
                           float(th["rain_streak_codes"]), 100.0 * float(np.mean(veil_covers)), len(veil_counts))))
    C.append(check("rain H5 minimum streak length", gate(short == 0), round(float(la.min()), 1) if la.size else None,
                   ">= %.1f px (10 px at 1080p)" % lmin, "%d of %d inner streaks shorter (p5 %.1f px); border streaks excluded%s" % (
                       short, la.size, float(np.percentile(la, 5)) if la.size else 0.0, note)))


# ---------------------------------------------------------------------------------------------------------- montage, crops, clip
def _font(size):
    from PIL import ImageFont
    try:
        return ImageFont.load_default(size=size)
    except TypeError:
        return ImageFont.load_default()


def write_montage(seq, res, frame_s):
    """Pages of every stride-th frame (4 x 6 tiles) into <seq>/montage_NN.png; the worst pop / blink blocks at 4 x into
    <seq>/pops_crops.png. Returns the written paths."""
    from PIL import Image, ImageDraw
    frames = seq["frames"]
    n = len(frames)
    if n == 0:
        return []
    per = MONTAGE_COLS * MONTAGE_ROWS
    stride = max(MONTAGE_STRIDE, int(math.ceil(n / float(per * MONTAGE_PAGES))))
    pick = list(range(0, n, stride))
    out = []
    first, _ = qc.load_rgb8(frames[0][1])
    H, W = first.shape[:2]
    tw = 480
    th_ = int(round(tw * H / float(W)))
    font = _font(14)
    bad = set(e["frame"] for e in (res.get("pops") or []))
    for p in range(0, len(pick), per):
        chunk = pick[p:p + per]
        rows = (len(chunk) + MONTAGE_COLS - 1) // MONTAGE_COLS
        sheet = Image.new("RGB", (MONTAGE_COLS * tw, rows * (th_ + 18) + 22), (20, 20, 22))
        dr = ImageDraw.Draw(sheet)
        dr.text((4, 3), "%s (%s, %d frames at %g fps, every %d-th frame; red = a frame with a block pop / blink nearby)" % (
            res["name"], res["kind"], n, res["fps"], stride), fill=(230, 230, 230), font=font)
        for j, fi in enumerate(chunk):
            try:
                img, _ = qc.load_rgb8(frames[fi][1])
            except Exception:
                continue
            im = Image.fromarray(img).resize((tw, th_), Image.BILINEAR)
            x, y = (j % MONTAGE_COLS) * tw, 22 + (j // MONTAGE_COLS) * (th_ + 18)
            sheet.paste(im, (x, y))
            s = frame_s[fi] if frame_s and fi < len(frame_s) else None
            near = any(abs(fi - b) < stride for b in bad)
            dr.text((x + 3, y + th_ + 2), "f%d%s" % (frames[fi][0], (" s %.1f" % s) if s is not None else ""),
                    fill=(240, 80, 80) if near else (210, 210, 210), font=font)
        path = os.path.join(seq["dir"], "montage_%02d.png" % (p // per + 1))
        sheet.save(path)
        out.append(path)
    ev = (res.get("pops") or [])[:6]
    if ev:
        strips = []
        for e in ev:
            f = e["frame"]
            x0, y0, x1, y1 = e["block_px"]
            k = x1 - x0
            cx0, cy0 = max(0, x0 - k), max(0, y0 - k)
            cx1, cy1 = min(W, x1 + k), min(H, y1 + k)
            sc = max(1, min(4, 600 // max(1, cx1 - cx0)))
            tiles = []
            for fi in (f - 1, f, f + 1):
                if 0 <= fi < n:
                    img, _ = qc.load_rgb8(frames[fi][1])
                    c = Image.fromarray(img[cy0:cy1, cx0:cx1]).resize(((cx1 - cx0) * sc, (cy1 - cy0) * sc), Image.NEAREST)
                    tiles.append((fi, c))
            if not tiles:
                continue
            tw2 = sum(t[1].size[0] for t in tiles) + 8 * (len(tiles) + 1)
            th2 = max(t[1].size[1] for t in tiles) + 40
            st = Image.new("RGB", (tw2, th2), (20, 20, 22))
            d2 = ImageDraw.Draw(st)
            d2.text((6, 3), "%s f%d s %s: %d px block at %s, %.1f levels vs %.2f (x%d)" % (
                e["type"], f, e.get("s"), k, e["block_px"][:2], e["diff"], e["neighbours"], sc), fill=(240, 200, 120), font=font)
            x = 8
            for fi, c in tiles:
                st.paste(c, (x, 22))
                d2.text((x + 3, 24 + c.size[1]), "frame %d" % fi, fill=(220, 220, 220), font=font)
                x += c.size[0] + 8
            strips.append(st)
        if strips:
            Wc = max(s.size[0] for s in strips)
            Hc = sum(s.size[1] for s in strips)
            sheet = Image.new("RGB", (Wc, Hc), (20, 20, 22))
            y = 0
            for s_ in strips:
                sheet.paste(s_, (0, y))
                y += s_.size[1]
            path = os.path.join(seq["dir"], "pops_crops.png")
            sheet.save(path)
            out.append(path)
    return out


def write_clip(seq, res):
    """x264 CRF 20 (preset fast, yuv420p, BT.709) of the PNG frames into <seq>/clip.mp4; None without ffmpeg."""
    try:
        import encode_test
        ff = encode_test.find_ffmpeg()
    except Exception:
        ff = None
    if not ff or not seq.get("pattern"):
        return None
    out = os.path.join(seq["dir"], "clip.mp4")
    cmd = [ff, "-hide_banner", "-loglevel", "error", "-y", "-framerate", "%g" % res["fps"], "-start_number", str(seq["start"]),
           "-i", seq["pattern"], "-vf", encode_test.VF_ENC, "-c:v", "libx264", "-preset", "fast", "-crf", "20",
           "-pix_fmt", "yuv420p"] + encode_test.TAGS + ["-movflags", "+faststart", out]
    try:
        rc = subprocess.run(cmd, capture_output=True, timeout=1800).returncode
    except Exception:
        return None
    return out if rc == 0 and os.path.isfile(out) else None


def finish_sequence(seq, res, frame_s, delete_frames, report_md):
    """Montage, crops and clip, then (delete_frames) the frames go and a done marker stays (one sequential job, C21)."""
    paths = []
    try:
        paths = write_montage(seq, res, frame_s)
    except Exception as e:  # pragma: no cover
        res["montage_error"] = "%s: %s" % (type(e).__name__, e)
    res["montage"] = [p for p in paths if "montage_" in os.path.basename(p)]
    res["crops"] = [p for p in paths if "pops_crops" in os.path.basename(p)]
    if res["kind"] in CLIP_KINDS:
        res["clip"] = write_clip(seq, res)
    if delete_frames:
        if not res["montage"]:
            res["frames_kept"] = "no montage written: frames kept"
            return
        nd = 0
        for _, p in seq["frames"]:
            try:
                os.remove(p)
                nd += 1
            except OSError:
                pass
        res["frames_deleted"] = nd
        with open(os.path.join(seq["dir"], "analysed.done"), "w", encoding="utf-8") as f:
            json.dump({"time": time.strftime("%Y-%m-%d %H:%M:%S"), "report": report_md, "frames_deleted": nd,
                       "montage": res["montage"], "crops": res["crops"], "clip": res.get("clip"), "status": res.get("status")}, f, indent=1)
