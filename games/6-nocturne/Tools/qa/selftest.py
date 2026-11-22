#!/usr/bin/env python3
"""selftest: synthetic end-to-end test of the Nocturne QA tools (images, sequences, encode, audio, run_all).

Usage:  python -I Tools/qa/selftest.py [--work QA/tmp/selftest] [--no-encode] [--no-4k]

Generates synthetic stills, sequences and WAVs under QA/tmp/selftest (gitignored), runs every tool on them and
asserts the expected verdicts (TQ adds: tiling.py catches a 96 px tiled texture and passes shuffled tiles; debugview.py decodes
views 4, 7 and 9; the texpan kind finds no swimming in a world-locked pan, catches a screen-locked layer and a flicker over the
-capture-ss 2 reference; the dolly kind finds a local pop with its distance): a smooth gradient without dither must FAIL the banding score and the same gradient
with a 1 LSB TPDF dither must PASS; a 2 px antialiased line must measure about 2 px; deliberate defects (black,
white, magenta, NaN-like pixel, wrong size, popping sequence, aliased line, DC, click, 0 dBFS peak) must be
caught; a 997 Hz 0 dBFS sine on one channel must read -3.01 LUFS.  Exit code 1 if any assertion fails.
"""
import argparse
import json
import math
import os
import shutil
import struct
import sys
import time
import wave

_HERE = os.path.dirname(os.path.abspath(__file__))
if _HERE not in sys.path:
    sys.path.insert(0, _HERE)

import numpy as np  # noqa: E402

import qa_common as qc  # noqa: E402

RNG = np.random.default_rng(20261029)
RESULTS = []


def expect(name, cond, detail=""):
    RESULTS.append((name, bool(cond), detail))
    print("  %s  %s  %s" % ("ok  " if cond else "FAIL", name, detail), flush=True)


# --------------------------------------------------------------------------------------------- synthesis helpers
def enc(lin):
    lin = np.clip(lin, 0.0, 1.0)
    return np.where(lin <= 0.0031308, lin * 12.92, 1.055 * np.power(lin, 1 / 2.4) - 0.055)


def dec(c):
    c = np.clip(c, 0.0, 1.0)
    return np.where(c <= 0.04045, c / 12.92, np.power((c + 0.055) / 1.055, 2.4))


def tpdf(shape, amp=1.0):
    return (RNG.random(shape) - RNG.random(shape)) * amp


def quantise(codes, amp):
    """Float sRGB codes (0..255) -> uint8 with monochrome TPDF dither of +-amp LSB."""
    n = tpdf(codes.shape[:2] + (1,), amp) if amp > 0 else 0.0
    return np.clip(np.floor(codes + n + 0.5), 0, 255).astype(np.uint8)


def gradient(H, W, top, bot):
    t = np.linspace(0.0, 1.0, H)[:, None, None]
    return np.broadcast_to(np.array(top, float) * (1 - t) + np.array(bot, float) * t, (H, W, 3)).copy()


def coverage_line(W, H, x_of_y, width, aliased=False):
    """Per-pixel coverage of a vertical band [x - w/2, x + w/2] (exact box area per pixel, or 0/1 when aliased)."""
    ys = np.arange(H) + 0.5
    xc = x_of_y(ys)[:, None]
    i = np.arange(W)[None, :].astype(np.float64)
    a, b = xc - width / 2.0, xc + width / 2.0
    if aliased:
        cx = i + 0.5
        return ((cx >= a) & (cx < b)).astype(np.float64)
    return np.clip(np.minimum(b, i + 1) - np.maximum(a, i), 0.0, 1.0)


def _herm(p0, p1, m0, m1, u):
    u2 = u * u
    u3 = u2 * u
    return (2 * u3 - 3 * u2 + 1) * p0 + (u3 - 2 * u2 + u) * m0 + (-2 * u3 + 3 * u2) * p1 + (u3 - u2) * m1


def sky_lin(dir_y, h, m, z, stop_m_deg=25.0):
    """C1 sky gradient in t = sqrt(dirY) (the DEVELOPMENT.md D-008 form), linear colour per channel."""
    tm = math.sqrt(math.sin(math.radians(stop_m_deg)))
    t = np.sqrt(np.clip(dir_y, 0.0, 1.0))[..., None]
    sl = z - h
    a = np.clip(_herm(h, m, 0.0, sl * tm, t / tm), np.minimum(h, m), np.maximum(h, m))
    b = np.clip(_herm(m, z, sl * (1 - tm), 0.0, (t - tm) / (1 - tm)), np.minimum(m, z), np.maximum(m, z))
    return np.where(t < tm, a, b)


KS04_POSE = {"x": 65.30, "y": 1.7, "z": 195.0, "yaw": 90.0, "pitch": 22.0, "fov": 66.0}
NEON_PX = (400.0, 300.0)   # 1080p pixel centre of the synthetic neon sign core (clipped, inside the exclusion disc)


def ks04_image(W, H, dither=2.0):
    """Synthetic KS04_boulevard_axis (yaw 90, pitch +22, vFOV 66): a night sky gradient over elevation as flat as the 14.7 H1
    arithmetic (green changes 4 codes over 55 deg: hundreds of px per code), dark facades below elevation 10 deg, a 2 px (at
    1080p) neon blade edge at x ~ 1115/1920 above the facades, and a small clipped neon core at NEON_PX (excluded by a disc)."""
    cam = qc.Cam(W, H, KS04_POSE["fov"], KS04_POSE["yaw"], KS04_POSE["pitch"], (KS04_POSE["x"], KS04_POSE["y"], KS04_POSE["z"]))
    uu, vv = np.meshgrid(np.arange(W) + 0.5, np.arange(H) + 0.5)
    d = cam.cam_dir_px(uu, vv)
    fwd, right, up = cam.basis()
    wd = d[..., 0:1] * right + d[..., 1:2] * up + d[..., 2:3] * fwd
    wd /= np.linalg.norm(wd, axis=-1, keepdims=True)
    el = np.degrees(np.arcsin(np.clip(wd[..., 1], -1, 1)))
    hz, zen = np.array([30, 18, 46.]), np.array([22, 14, 40.])
    tt = np.clip(el / 60.0, 0.0, 1.0)[..., None]
    sky = enc(dec(hz / 255.0) * (1 - tt) + dec(zen / 255.0) * tt) * 255.0
    facade = np.array([20, 18, 28.])
    codes = np.where((el >= 10.0)[..., None], sky, facade[None, None, :] * np.ones_like(sky))
    sc = H / 1080.0
    cov = coverage_line(W, H, lambda y: (1116.8 + 0.01 * (y / sc - 540.0)) * sc, 2.0 * sc)
    cov *= (el > 12.0)
    blade = dec(np.array([30, 120, 140.]) / 255.0)
    lin = dec(codes / 255.0)
    lin = lin * (1 - cov[..., None]) + blade * cov[..., None]
    codes = enc(lin) * 255.0
    out = quantise(codes, dither)
    r = np.hypot(uu - NEON_PX[0] * sc, vv - NEON_PX[1] * sc)
    out[r < 5.0 * sc] = (255, 120, 255)
    return out


def write_wav_float(path, x, fs):
    x = np.asarray(x, dtype="<f4")
    ch = x.shape[1]
    data = x.tobytes()
    fmt = struct.pack("<HHIIHHH", 3, ch, fs, fs * ch * 4, ch * 4, 32, 0)
    fact = struct.pack("<I", x.shape[0])
    with open(path, "wb") as f:
        f.write(b"RIFF" + struct.pack("<I", 4 + 8 + len(fmt) + 8 + len(fact) + 8 + len(data)) + b"WAVE")
        f.write(b"fmt " + struct.pack("<I", len(fmt)) + fmt)
        f.write(b"fact" + struct.pack("<I", len(fact)) + fact)
        f.write(b"data" + struct.pack("<I", len(data)) + data)


def write_wav_pcm16(path, x, fs):
    q = np.clip(np.round(np.asarray(x) * 32768.0), -32768, 32767).astype("<i2")
    with wave.open(path, "wb") as w:
        w.setnchannels(q.shape[1])
        w.setsampwidth(2)
        w.setframerate(fs)
        w.writeframes(q.tobytes())


def write_wav_pcm24(path, x, fs):
    q = np.clip(np.round(np.asarray(x) * 8388608.0), -8388608, 8388607).astype(np.int32).reshape(-1)
    b = np.stack([(q & 0xFF), (q >> 8) & 0xFF, (q >> 16) & 0xFF], axis=1).astype(np.uint8).tobytes()
    ch = x.shape[1]
    fmt = struct.pack("<HHIIHH", 1, ch, fs, fs * ch * 3, ch * 3, 24)
    with open(path, "wb") as f:
        f.write(b"RIFF" + struct.pack("<I", 4 + 8 + len(fmt) + 8 + len(b)) + b"WAVE")
        f.write(b"fmt " + struct.pack("<I", len(fmt)) + fmt)
        f.write(b"data" + struct.pack("<I", len(b)) + b)


def get_check(res_img, name):
    for c in res_img["checks"]:
        if c["name"] == name:
            return c
    return None


# --------------------------------------------------------------------------------------------- tests
def test_banding():
    print("[banding metric]")
    # 14.7 H1: night gradients (flat: hundreds of px per code), the 0.04 code/px haze and a brighter sky; the 14.12 dither
    # is a 2-code TPDF
    for label, H, top, bot in (("night sky 1080p", 640, (14, 10, 40), (20, 14, 46)),
                               ("night sky 4K", 1280, (14, 10, 40), (20, 14, 46)),
                               ("haze 0.04 code/px", 640, (10, 8, 30), (36, 26, 60)),
                               ("wall panel", 403, (243, 228, 208), (191, 217, 227))):
        g = gradient(H, 256, top, bot)
        b0 = qc.band_score(quantise(g, 0.0).astype(np.float64))
        b1 = qc.band_score(quantise(g, 2.0).astype(np.float64))
        expect("%s undithered fails" % label, b0["pass"] is False, "score %.2f steps %d bandscore %.2f" % (b0["score"], b0["steps"], b0["bandscore"]))
        expect("%s 2-code TPDF passes" % label, b1["pass"] is True, "score %.2f steps %d bandscore %.2f" % (b1["score"], b1["steps"], b1["bandscore"]))
        expect("%s scores clearly apart" % label, b0["score"] > 2.0 * b1["score"], "%.2f vs %.2f" % (b0["score"], b1["score"]))
    # horizontal gradient + mask splitting
    g = np.transpose(gradient(640, 300, (14, 10, 40), (20, 14, 46)), (1, 0, 2)).copy()
    m = np.zeros(g.shape[:2], dtype=bool)
    m[100:140, 200:260] = True
    bh = qc.band_score(quantise(g, 2.0).astype(np.float64), None, "auto", m)
    expect("auto direction + mask (dithered passes)", bh["pass"] is True and bh["direction"] == "h",
           "dir %s strips %d" % (bh["direction"], bh["n_strips"]))
    # a single visible step of 1 code between two 100-px plateaus in Cb only (luma constant) is caught
    pr = np.zeros((300, 3)) + np.array([40.0, 30.0, 60.0])
    pr[150:, 2] += 2.2
    n, mx = qc.visible_steps(qc._box(qc.ycbcr(pr), 9)[:, 1])
    expect("chroma step (Cb) caught", n == 1 and mx >= 1.0, "steps %d max %.2f" % (n, mx))
    # QA-6 gate "steps" (textured floors): an undithered gradient under a fine texture still fails (its contours cross every
    # strip), while one isolated 2-code edge inside one strip (a joint or a reflection edge) passes
    # paving patches: whole-code offsets per 40 x 40 px cell (any sub-code texture would dither the gradient itself)
    cells = np.random.default_rng(7).integers(-2, 3, (16, 7)).astype(np.float64)
    tex = np.kron(cells, np.ones((40, 40)))[:640, :256]
    g = gradient(640, 256, (14, 10, 40), (20, 14, 46)) + tex[..., None]
    bt = qc.band_score(quantise(g, 0.0).astype(np.float64), None, "v", None, qc.TH, {"gate": "steps"})
    expect("textured gate: coherent banding fails", bt["pass"] is False, "score %.2f steps %d" % (bt["score"], bt["steps"]))
    flat = np.zeros((300, 256, 3)) + np.array([40.0, 38.0, 36.0])
    flat[150:, 64:128] += 2.0
    be = qc.band_score(flat, None, "v", None, qc.TH, {"gate": "steps"})
    bd = qc.band_score(flat, None, "v", None, qc.TH, {})
    expect("textured gate: one-strip edge passes", be["pass"] is True and bd["pass"] is False,
           "gated %.2f (steps %d) vs default %.2f" % (be["score"], be["steps"], bd["score"]))


def test_line_probe():
    print("[thin-line probe]")
    W, H = 256, 300
    bg = gradient(H, W, (200, 200, 205), (190, 195, 205))
    for width, lo, hi in ((2.0, 1.85, 2.15), (1.75, 1.7, 2.05), (3.3, 3.15, 3.45)):
        cov = coverage_line(W, H, lambda y: 128.3 + 0.013 * y, width)
        lin = dec(bg / 255.0) * (1 - cov[..., None]) + dec(np.array([60, 90, 100.]) / 255.0) * cov[..., None]
        img = quantise(enc(lin) * 255.0, 1.0)
        r = qc.line_probe(qc.luma_linear(img), 130, 20, 64, 256)
        ok = r.get("found") and lo <= r["fwhm_px"] <= hi
        expect("AA line %.2f px width" % width, ok, "fwhm %.3f eq %.3f" % (r.get("fwhm_px", -1), r.get("eq_width_px", -1)))
        ev = qc.evaluate_line(r, {"name": "t"}, 1080)
        bad = [c["name"] for c in ev if c["status"] == "FAIL"]
        expect("AA line %.2f px clean (monotone, no stair-step, stable energy)" % width, not bad,
               "monotone %.2f stair %.3f ecv %.3f %s" % (r["monotone_rows"], r["centroid_resid_max_px"], r["energy_var"], bad))
    cov = coverage_line(W, H, lambda y: 128.3 + 0.013 * y, 2.0, aliased=True)
    lin = dec(bg / 255.0) * (1 - cov[..., None]) + dec(np.array([60, 90, 100.]) / 255.0) * cov[..., None]
    img = quantise(enc(lin) * 255.0, 1.0)
    r = qc.line_probe(qc.luma_linear(img), 130, 20, 64, 256)
    ev = {c["name"]: c["status"] for c in qc.evaluate_line(r, {"name": "t"}, 1080)}
    expect("aliased line caught (stair-step)", ev.get("line[t] stair-step") == "FAIL",
           "stair %.3f px" % r.get("centroid_resid_max_px", -1))


def _photo_run(frame_fn, n=90, fps=30, static=False, H=270, W=480):
    import seq_extra as sx
    e = sx.Extra("cinematic", {}, W, H, fps)
    luma = []
    for t in range(n):
        y = frame_fn(t, H, W)
        g8 = np.clip(np.round(enc(y) * 255.0), 0, 255).astype(np.uint8)
        rgb = np.repeat(g8[..., None], 3, axis=2)
        e.add(t, rgb, rgb[..., 0].astype(np.float64), None)
        luma.append(rgb[..., 0].mean() / 255.0)
    res = {"checks": [], "summary": {}}
    e.photo_checks(res, luma, static)
    return {c["name"].split(" (")[0]: (c["status"], c["value"]) for c in res["checks"]}


def test_photo_pairs():
    """D-041 (1): a flash is a pair of opposing changes >= 0.1 (darker < 0.8) inside a 1/3 s window (G1); a monotonic change and
    a slow camera sweep are not flashes (the R4 1 s pair row gates only a fixed camera); real flashes still fail."""
    print("[photosensitivity pairs]")
    base = 0.02
    r = _photo_run(lambda t, H, W: np.full((H, W), 0.5 if (t // 4) % 2 else base))
    expect("4 Hz full-screen flash fails G1 and G2", r["G1 flashes"][0] == "FAIL" and r["G2 WCAG general"][0] == "FAIL", str(r["G1 flashes"]))

    def f2(t, H, W):
        y = np.full((H, W), base)
        if (t // 7.5) % 2 >= 1:
            y[:, :int(0.278 * W)] = 0.5
        return y
    r = _photo_run(f2)
    expect("2 Hz flash over 27.8 % fails G1, passes G2", r["G1 flashes"][0] == "FAIL" and r["G2 WCAG general"][0] == "PASS", str(r["G1 flashes"]))
    r = _photo_run(lambda t, H, W: np.full((H, W), base + min(1.0, t / 30.0) * 0.58))
    expect("a monotonic full-screen ramp is no flash", r["G1 flashes"][0] == "PASS" and r["G1 slow pairs"][1] == 0.0, str(r["G1 flashes"]))

    def sweep(t, H, W):
        y = np.full((H, W), base)
        top = int((-0.3 + 0.6 * t / 30.0) * H)
        y[max(0, top):max(0, min(H, top + int(0.3 * H))), :] = 0.3
        return y
    r = _photo_run(sweep, n=75)
    expect("a slow band sweep (camera look-up) passes G1, its 1 s pairs are INFO", r["G1 flashes"][0] == "PASS" and r["G1 slow pairs"][0] == "INFO"
           and r["G1 slow pairs"][1] >= 25.0, "%s %s" % (r["G1 flashes"], r["G1 slow pairs"]))
    r = _photo_run(sweep, n=75, static=True)
    expect("the same 1 s pairs on a fixed camera fail", r["G1 slow pairs"][0] == "FAIL", str(r["G1 slow pairs"]))

    def light(t, H, W):
        tt = t / 30.0
        a = 0.0 if tt < 1 else (min(1, (tt - 1) / 0.1) if tt < 1.1 else max(0.0, 1 - (tt - 1.1) / 0.3))
        return np.full((H, W), base + 0.3 * a)
    r = _photo_run(light)
    expect("a fast single full-screen flash fails G1", r["G1 flashes"][0] == "FAIL", str(r["G1 flashes"]))


def test_blink_sign_rule():
    """AL's C21 sign rule: a one-frame dip of a block is a blink; a bar stepping through a block (once) is a pop, not a blink."""
    import seq_extra as sx
    print("[block blinks, sign rule]")
    H, W = 128, 256
    e = sx.Extra("boundary", {}, W, H, 30)
    e.kp = [32]
    e.D, e.M = {32: []}, {32: []}
    prev = None
    for f in range(10):
        g = np.full((H, W), 50.0) + np.random.RandomState(f).randn(H, W) * 0.3
        if f == 5:
            g[32:64, 64:96] -= 20.0
        if f >= 4:
            g[64:96, 160:192] += 15.0
        e.add(f, np.zeros((H, W, 3), np.uint8), g, np.abs(g - prev) if prev is not None else None)
        prev = g
    ev, _ = e.pop_events(32)
    bl = [x for x in ev if x["type"] == "blink"]
    pops = [x for x in ev if x["type"] == "pop"]
    expect("the one-frame dip is a blink (both rules)", len(bl) == 1 and bl[0]["frame"] == 5 and bl[0]["block"] == 10 and "sign" in bl[0]["rule"],
           str([(x["frame"], x["block"], x.get("rule")) for x in bl]))
    expect("the step is a pop, not a blink", len(pops) == 1 and pops[0]["block"] == 21, str([(x["frame"], x["block"]) for x in pops]))


def test_decode_step_channels():
    """D-041 (2): the decode gate counts visible steps in Y, Cb, Cr; an R-only 2-code step is reported, not gated."""
    print("[decode step channels]")
    p = np.zeros((300, 3)) + 100.0
    p[150:, 0] += 2.0
    m = qc.strip_metrics(p, dict(qc.TH))
    t = dict(qc.TH)
    t["band_step_channels"] = qc.DECODE_STEP_CHANNELS
    m2 = qc.strip_metrics(p, t)
    expect("an R-only step counts in the source gate, not in the decode gate", m["steps"] == 1 and m2["steps"] == 0 and m2["chan"]["R"][0] > 0,
           "source %d, decode %d" % (m["steps"], m2["steps"]))


def test_parity_companions():
    """Q1: X_window.png is dropped only beside X.png; key shots whose own tag ends in _window stay (KS07, KS19)."""
    shots = [{"tag": t, "file": "/c/%s.png" % t} for t in
             ("KS04_boulevard_axis", "KS04_boulevard_axis_window", "KS07_edge_window", "KS19_goal_window", "KS19_goal_window_1610")]
    kept = [s["tag"] for s in qc.drop_parity_companions(shots)]
    expect("parity companion dropped", "KS04_boulevard_axis_window" not in kept, str(kept))
    expect("key shots named *_window kept", all(t in kept for t in ("KS07_edge_window", "KS19_goal_window", "KS19_goal_window_1610")), str(kept))


def make_capture(root, with_4k):
    cap = qc.ensure_dir(os.path.join(root, "cap_run"))
    shots = []
    print("[synthesis] stills", flush=True)
    regions = {"lines": [{"name": "blade", "x": 1117, "tol": 16, "y0": 150, "h": 256, "w": 40,
                          "widthMin1080": 1.75, "widthMax1080": 2.6, "contrastMin": 0.08}],
               "exclude": [{"name": "neon_core", "px": list(NEON_PX), "clipDeg": 1.2, "bandDeg": 1.5}],
               "ref": [1920, 1080]}
    img = ks04_image(1920, 1080, 2.0)
    qc.save_png(os.path.join(cap, "KS04_boulevard_axis.png"), img)
    shots.append({"tag": "KS04_boulevard_axis", "file": "KS04_boulevard_axis.png", "width": 1920, "height": 1080, "s": 250,
                  "pose": dict(KS04_POSE), "regions": regions})
    qc.save_png(os.path.join(cap, "KS04_boulevard_axis_nodither.png"), ks04_image(1920, 1080, 0.0))
    shots.append({"tag": "KS04_boulevard_axis_nodither", "file": "KS04_boulevard_axis_nodither.png", "width": 1920,
                  "height": 1080, "s": 250, "pose": dict(KS04_POSE), "regions": regions})
    qc.save_png(os.path.join(cap, "KS04_boulevard_axis_window.png"), img)  # parity companion
    if with_4k:
        qc.save_png(os.path.join(cap, "KS04_boulevard_axis_4k.png"), ks04_image(3840, 2160, 2.0))
        shots.append({"tag": "KS04_boulevard_axis_4k", "file": "KS04_boulevard_axis_4k.png", "width": 3840,
                      "height": 2160, "s": 250, "pose": dict(KS04_POSE), "regions": regions})
    qc.save_png(os.path.join(cap, "beat02_boulevard_mid_black.png"), np.zeros((1080, 1920, 3), np.uint8))
    shots.append({"tag": "beat02_boulevard_mid_black", "file": "beat02_boulevard_mid_black.png", "width": 1920, "height": 1080})
    qc.save_png(os.path.join(cap, "beat03_edge_end_white.png"), np.full((1080, 1920, 3), 255, np.uint8))
    shots.append({"tag": "beat03_edge_end_white", "file": "beat03_edge_end_white.png", "width": 1920, "height": 1080})
    g = quantise(gradient(1080, 1920, (120, 110, 140), (16, 14, 26)), 2.0)
    g[500:510, 900:910] = (255, 0, 255)
    g[200, 300] = (0, 0, 0)                      # isolated NaN-like black pixel
    g[1000, 1500] = (250, 250, 250)              # isolated bright pixel in the dark part (> 8 x the 5 x 5 median, no neon mask)
    g[600:650, 200:250] = (176, 58, 140)         # CloudHot #B03A8C: the cloud deck is not missing-shader magenta
    g[900:902, 1200:1202] = (220, 220, 220)      # a 2 x 2 emissive marker: inside the neon mask, not an isolated pixel
    qc.save_png(os.path.join(cap, "beat04_skybridge_mid_defects.png"), g)
    shots.append({"tag": "beat04_skybridge_mid_defects", "file": "beat04_skybridge_mid_defects.png", "width": 1920, "height": 1080})
    g2 = quantise(gradient(720, 1280, (150, 160, 170), (100, 110, 130)), 1.0)
    qc.save_png(os.path.join(cap, "beat05_garden_start.png"), g2)
    shots.append({"tag": "beat05_garden_start", "file": "beat05_garden_start.png", "width": 1920, "height": 1080,
                  "subjects": [{"name": "lantern", "box": [100, 100, 600, 500]},
                               {"name": "clipped_box", "box": [-50, 100, 400, 500]}],
                  "regions": {"stats": [{"name": "all", "rect": [0, 0, 1, 1], "units": "frac", "stdMax": 0.5}]}})
    with open(os.path.join(cap, "report.json"), "w") as f:
        json.dump({"run": "selftest", "build": "synthetic", "shots": shots}, f, indent=1)

    print("[synthesis] sequences", flush=True)
    base = gradient(256, 320, (60, 52, 84), (40, 36, 62))
    sd = qc.ensure_dir(os.path.join(cap, "seq_static_beat01_frozen"))
    f0 = quantise(base, 2.0)
    for i in range(10):
        qc.save_png(os.path.join(sd, "frame_%05d.png" % i), f0)
    json.dump({"kind": "static", "fps": 30, "freezeClock": True, "freezeDither": True, "rainOverride": 0},
              open(os.path.join(sd, "sequence.json"), "w"))
    sd = qc.ensure_dir(os.path.join(cap, "seq_static_beat01_temporal"))
    for i in range(12):
        qc.save_png(os.path.join(sd, "frame_%05d.png" % i), quantise(base, 2.0))
    json.dump({"kind": "static", "fps": 30, "freezeClock": True, "freezeDither": False, "rainOverride": 0},
              open(os.path.join(sd, "sequence.json"), "w"))
    # QA fix wave C20: a static clip captured with the clock running and rain on is refused (run conditions FAIL, not analysed)
    sd = qc.ensure_dir(os.path.join(cap, "seq_static_beat02_wrongcond"))
    for i in range(4):
        qc.save_png(os.path.join(sd, "frame_%05d.png" % i), f0)
    json.dump({"kind": "static", "fps": 30, "freezeClock": False, "rainOverride": None}, open(os.path.join(sd, "sequence.json"), "w"))
    sd = qc.ensure_dir(os.path.join(cap, "seq_b23_pop"))
    for i in range(40):
        k = 1.0 + 0.002 * i + (0.12 if i == 25 else 0.0)
        qc.save_png(os.path.join(sd, "frame_%05d.png" % i), quantise(np.clip(base * k, 0, 255), 2.0))
    json.dump({"kind": "boundary", "fps": 30, "s0": 385, "s1": 415}, open(os.path.join(sd, "sequence.json"), "w"))
    # boundary walks crossing s 120 at frame 20: clean, and with a 40 x 40 patch that switches on at the crossing
    fixed = quantise(base, 2.0).astype(np.float64)
    for name, pop in (("seq_b12", False), ("seq_b12_activation", True)):
        sd = qc.ensure_dir(os.path.join(cap, name))
        frames = []
        for i in range(40):
            f = np.clip(fixed * (1.0 + 0.0004 * i), 0, 255)
            if pop and i >= 20:
                f[100:140, 100:140] = np.clip(f[100:140, 100:140] + 60.0, 0, 255)
            qc.save_png(os.path.join(sd, "frame_%05d.png" % i), np.round(f).astype(np.uint8))
            frames.append({"i": i, "s": 110.0 + 0.5 * i})
        json.dump({"kind": "boundary", "fps": 30, "boundaryS": [120.0], "frames": frames}, open(os.path.join(sd, "sequence.json"), "w"))
    # H5 rain flicker: a static background with additive streaks (clean), and the same with a 12-level chroma flicker of the
    # background at constant luma (R +12, G -4: luma -0.3 code, so it stays outside the streak mask; a brightening is inside
    # the mask by construction and is left to the luma-step / spike checks)
    rng = np.random.default_rng(5)
    for name, flick in (("seq_rain_beat04", False), ("seq_rain_beat04_flicker", True)):
        sd = qc.ensure_dir(os.path.join(cap, name))
        for i in range(16):
            f = fixed.copy()
            for _ in range(12):
                x, y = int(rng.integers(2, 318)), int(rng.integers(0, 220))
                f[y:y + 30, x] = np.clip(f[y:y + 30, x] + 40.0, 0, 255)
            if flick and i == 8:
                f[200:230, 20:60] = np.clip(f[200:230, 20:60] + np.array([12.0, -4.0, 0.0]), 0, 255)
            qc.save_png(os.path.join(sd, "frame_%05d.png" % i), np.round(f).astype(np.uint8))
        json.dump({"kind": "rain", "fps": 60, "rainOverride": 1.0}, open(os.path.join(sd, "sequence.json"), "w"))
    for name, aliased in (("seq_b5_pan_rail", False), ("seq_b5_pan_rail_aliased", True)):
        sd = qc.ensure_dir(os.path.join(cap, name))
        bgl = dec(gradient(300, 256, (200, 200, 205), (190, 195, 205)) / 255.0)
        for i in range(30):
            cov = coverage_line(256, 300, lambda y: 120.2 + 0.1 * i + 0.0 * y, 2.0, aliased)
            lin = bgl * (1 - cov[..., None]) + dec(np.array([40, 40, 50.]) / 255.0) * cov[..., None]
            qc.save_png(os.path.join(sd, "frame_%05d.png" % i), quantise(enc(lin) * 255.0, 1.0))
        json.dump({"kind": "pan", "fps": 30, "freezeClock": True, "rainOverride": 0, "lines": [{"name": "rail", "x": 121, "tol": 16, "y0": 20, "h": 256}],
                   "stability": [{"name": "vp", "rect": [20, 20, 84, 84]}]}, open(os.path.join(sd, "sequence.json"), "w"))
    sd = qc.ensure_dir(os.path.join(cap, "seq_enc_beat03_abyss"))
    night = gradient(360, 640, (30, 22, 52), (18, 14, 40))
    for i in range(30):
        qc.save_png(os.path.join(sd, "frame_%05d.png" % i), quantise(night, 2.0))
    json.dump({"kind": "encode", "encode": True, "fps": 30,
               "banding": [{"name": "abyss", "rect": [0, 0, 640, 360], "dir": "v"}]},
              open(os.path.join(sd, "sequence.json"), "w"))
    return cap


def make_audio(root):
    print("[synthesis] audio", flush=True)
    ad = qc.ensure_dir(os.path.join(root, "audio"))
    sp = qc.spec()
    fs, dur = 48000, 60.0
    n = int(fs * dur)
    t = np.arange(n) / float(fs)
    s = t * (sp.beat_boundaries[-1] / dur)
    beat = np.searchsorted(np.array(sp.beat_boundaries[1:]), s, side="right") + 1
    beat = np.clip(beat, 1, 7)
    tgt = np.array(sp.target_rms_db)[beat - 1]
    tgt = np.where((s >= 1340.0) & (s <= 1380.0), -12.0, tgt)   # a loud S7 swell must be excluded from beat 7
    amp = np.sqrt(2.0) * 10 ** (tgt / 20.0)
    x = amp * np.sin(2 * np.pi * 220.0 * t)
    write_wav_float(os.path.join(ad, "render_mix.wav"), np.stack([x, x], axis=1), fs)
    json.dump({"s": [float(v) for v in np.arange(0, dur + 1) * (sp.beat_boundaries[-1] / dur)], "dt": 1.0},
              open(os.path.join(ad, "render_mix.json"), "w"))
    t2 = np.arange(fs * 10) / float(fs)
    l = (32767.0 / 32768.0) * np.sin(2 * np.pi * 997.0 * t2)
    write_wav_pcm16(os.path.join(ad, "lufs_997_left.wav"), np.stack([l, np.zeros_like(l)], axis=1), fs)
    t3 = np.arange(fs * 5) / float(fs)
    y = 0.05 * np.sin(2 * np.pi * 110.0 * t3) + 0.01
    y[fs * 2] = 0.5
    write_wav_float(os.path.join(ad, "dc_click.wav"), np.stack([y, y], axis=1), fs)
    t4 = np.arange(fs * 2) / float(fs)
    z = np.sqrt(2.0) * 0.1 * np.sin(2 * np.pi * 440.0 * t4)
    write_wav_pcm24(os.path.join(ad, "tone_pcm24.wav"), np.stack([z, z], axis=1), fs)
    return ad


# --------------------------------------------------------------------------------------------- texture tools (TQ)
def _smooth_noise(shape, k, amp, rng):
    a = rng.normal(0.0, 1.0, shape)
    for _ in range(2):
        p = k // 2
        c = np.pad(np.pad(a, p, mode="wrap").cumsum(0).cumsum(1), ((1, 0), (1, 0)))
        a = (c[k:, k:] - c[:-k, k:] - c[k:, :-k] + c[:-k, :-k]) / float(k * k)
    return a / max(a.std(), 1e-9) * amp


def _grey_png(path, g):
    g8 = np.clip(np.round(g), 0, 255).astype(np.uint8)
    qc.save_png(path, np.stack([g8, g8, g8], -1))


def _world_texture(W, H, fov, yaw, pitch, screen=None, flicker=None):
    """A world-locked pattern on the view sphere (az / el sinusoids at 2-9 px periods) seen by a pinhole camera; optional
    screen-locked layer (swimming) and per-frame noise (shimmer)."""
    cam = qc.Cam(W, H, fov, yaw, pitch)
    u, v = np.meshgrid(np.arange(W) + 0.5, np.arange(H) + 0.5)
    d = cam.cam_dir_px(u, v)
    fwd, right, up = cam.basis()
    wd = d[..., 0:1] * right + d[..., 1:2] * up + d[..., 2:3] * fwd
    wd /= np.linalg.norm(wd, axis=-1, keepdims=True)
    az = np.degrees(np.arctan2(wd[..., 0], wd[..., 2]))
    el = np.degrees(np.arcsin(np.clip(wd[..., 1], -1, 1)))
    px = H * 0.5 / math.tan(math.radians(fov) * 0.5) * math.pi / 180.0   # px per degree at the centre
    g = 90.0 + 18.0 * np.sin(2 * math.pi * az * px / 9.0) * np.cos(2 * math.pi * el * px / 7.0) \
        + 10.0 * np.sin(2 * math.pi * (az * 0.6 + el * 0.8) * px / 4.5)
    if screen is not None:
        g = g + screen
    if flicker is not None:
        g = g + flicker
    return g


def _write_seq(root, name, kind, frames_g, poses, extra=None):
    d = qc.ensure_dir(os.path.join(root, "seq_" + name))
    fr = []
    for i, g in enumerate(frames_g):
        f = "frame_%05d.png" % i
        _grey_png(os.path.join(d, f), g)
        p = dict(poses[i])
        p.update({"i": i, "file": f})
        fr.append(p)
    meta = {"name": "seq_" + name, "kind": kind, "fps": 30, "freezeClock": True, "rainOverride": 0.0, "frames": fr,
            "supersample": 1}
    if extra:
        meta.update(extra)
    with open(os.path.join(d, "sequence.json"), "w", encoding="utf-8") as fh:
        json.dump(meta, fh)
    return d


def test_texture_tools(root, rep):
    """tiling.py (a tiled texture FAILs at its period, a shuffled one PASSes), debugview.py (views 4, 7, 9) and the texpan /
    dolly kinds of sequences.py (world-locked pan: no swimming; a screen-locked layer swims; flicker over a reference fails the
    R1 gate; a local pop in a dolly)."""
    import tiling
    import debugview
    import sequences
    rng = np.random.default_rng(7)
    td = qc.ensure_dir(os.path.join(root, "tex"))
    print("[tiling]", flush=True)
    # 1080 rows: the period range is given at 1080p (108 px tiles, 10 x 18 of them)
    tile = _smooth_noise((108, 108), 3, 14.0, rng)
    _grey_png(os.path.join(td, "tc_tiled.png"), 70.0 + np.tile(tile, (10, 18))[:, :1920] + np.linspace(0, 30, 1920)[None, :])
    src = _smooth_noise((1400, 2200), 3, 14.0, rng)
    sh = np.zeros((1080, 1944))
    for i in range(10):
        for j in range(18):
            oy, ox = rng.integers(0, 1290, 2)
            sh[i * 108:(i + 1) * 108, j * 108:(j + 1) * 108] = src[oy:oy + 108, ox:ox + 108]
    _grey_png(os.path.join(td, "tc_shuffled.png"), 70.0 + sh[:, :1920])
    r = tiling.run([td], "selftest_tiling", rep, None, [0.0, 0.0, 1.0, 1.0], [72, 120], True)
    tj = {i["tag"]: i for i in qc.read_json(r["json"])["images"]}
    rt = tj["tc_tiled"]["regions"][0]["repetition"]
    rs = tj["tc_shuffled"]["regions"][0]["repetition"]
    expect("tiling: 108 px tiles caught at 72-120 px (R3)", tj["tc_tiled"]["status"] == "FAIL" and rt["peak_in_range"] > 0.9,
           "%s at %s" % (qc.fmt(rt["peak_in_range"], 3), rt.get("peak_in_range_lag")))
    expect("tiling: shuffled tiles PASS (R3 <= 0.25)", tj["tc_shuffled"]["status"] == "PASS" and rs["peak_in_range"] <= 0.25,
           qc.fmt(rs["peak_in_range"], 3))
    expect("tiling: micro / meso reported", tj["tc_tiled"]["regions"][0]["micro"] > 1.4, qc.fmt(tj["tc_tiled"]["regions"][0]["micro"], 2))

    print("[debugview]", flush=True)
    dd = qc.ensure_dir(os.path.join(root, "debug"))
    v4 = np.zeros((270, 480, 3), np.uint8)
    v4[:, :240, 0] = 255
    v4[:, 240:, 1] = 255
    qc.save_png(os.path.join(dd, "KS04_boulevard_axis.png"), v4)
    v4b = v4.copy()
    v4b[100:110, 100:110] = (255, 255, 0)
    qc.save_png(os.path.join(dd, "KS05_boulevard_up.png"), v4b)
    r4 = debugview.run([dd], 4, "selftest_dv4", rep, False, True)
    dj = {i["tag"]: i for i in qc.read_json(r4["json"])["images"]}
    expect("debugview 4: clean signs PASS", dj["KS04_boulevard_axis"]["status"] == "PASS",
           str(dj["KS04_boulevard_axis"]["stats"].get("sigma_neg_share")))
    expect("debugview 4: both signs in one pixel FAIL", dj["KS05_boulevard_up"]["status"] == "FAIL",
           str(dj["KS05_boulevard_up"]["stats"].get("both_share")))
    d9 = qc.ensure_dir(os.path.join(root, "debug9"))
    v9 = np.zeros((270, 480, 3), np.uint8)
    v9[:, :, 2] = 255
    v9[:, :, 0] = 8
    v9[:, :120, 0] = 24                           # a quarter of the frame shaded 3 times: overdraw 1.5
    qc.save_png(os.path.join(d9, "KS09_edge_gulf.png"), v9)
    v9b = v9.copy()
    v9b[:, :, 0] = 16                             # every pixel shaded twice: overdraw 2.0
    qc.save_png(os.path.join(d9, "KS17_heart_reveal.png"), v9b)
    r9 = debugview.run([d9], 9, "selftest_dv9", rep, True, True)
    dj = {i["tag"]: i for i in qc.read_json(r9["json"])["images"]}
    expect("debugview 9: overdraw 1.5 PASS", dj["KS09_edge_gulf"]["status"] == "PASS" and abs(dj["KS09_edge_gulf"]["stats"]["overdraw"] - 1.5) < 1e-6,
           str(dj["KS09_edge_gulf"]["stats"]["overdraw"]))
    expect("debugview 9: overdraw 2.0 FAIL (> 1.5)", dj["KS17_heart_reveal"]["status"] == "FAIL", str(dj["KS17_heart_reveal"]["stats"]["overdraw"]))
    expect("debugview 9: map written", os.path.isfile(os.path.join(d9, "_debugview", "KS17_heart_reveal_v9_overdraw.png")))
    d7 = qc.ensure_dir(os.path.join(root, "debug7"))
    n = np.zeros((270, 480, 3))
    n[..., 1] = 1.0
    n[:, 240:] = (0.6, 0.0, 0.8)
    qc.save_png(os.path.join(d7, "tc_alley_wall.png"), np.round(128 + 127 * n).astype(np.uint8))
    r7 = debugview.run([d7], 7, "selftest_dv7", rep, False, True)
    s7 = qc.read_json(r7["json"])["images"][0]["stats"]
    expect("debugview 7: unit normals decoded", s7["unit_share"] > 0.99 and abs(s7["axes"]["+y"] - 0.5) < 0.01, "%s %s" % (s7["unit_share"], s7["axes"]))

    print("[sequences texpan / dolly]", flush=True)
    sd = qc.ensure_dir(os.path.join(root, "texseq"))
    rd = qc.ensure_dir(os.path.join(root, "texseq_ref"))
    W, H, fov, pitch, n_f = 640, 360, 62.0, 4.0, 30
    f = H * 0.5 / math.tan(math.radians(fov) * 0.5)
    step = math.degrees(math.atan(0.5 / f))
    poses = [{"yaw": 30.0 + step * (k - n_f // 2), "pitch": pitch, "fov": fov, "s": 0.0} for k in range(n_f)]
    clean = [_world_texture(W, H, fov, p["yaw"], p["pitch"]) for p in poses]
    scr = _smooth_noise((H, W), 3, 24.0, rng)
    swim = [_world_texture(W, H, fov, p["yaw"], p["pitch"], screen=scr) for p in poses]
    flick = [c.copy() for c in clean]
    for c in flick:
        c[100:228, 200:328] += rng.normal(0.0, 6.0, (128, 128))
    _write_seq(sd, "texpan_ks00", "texpan", flick, poses)
    _write_seq(sd, "texpan_ks04", "texpan", clean, poses)
    _write_seq(sd, "texpan_ks05", "texpan", swim, poses)
    _write_seq(rd, "texpan_ks00", "texpan", clean, poses, {"supersample": 2})
    _write_seq(rd, "texpan_ks04", "texpan", clean, poses, {"supersample": 2})
    # dolly: a smooth zoom of the same pattern with a 64 px block that pops by 40 levels at frame 20
    dposes = [{"yaw": 30.0, "pitch": pitch, "fov": 62.0 - 0.2 * k, "s": 480.0, "dist": 800.0 * 0.95 ** k} for k in range(40)]
    dol = [_world_texture(W, H, p["fov"], p["yaw"], p["pitch"]) for p in dposes]
    for k in range(20, 40):
        dol[k][128:192, 256:320] += 40.0
    _write_seq(sd, "dolly_face30", "dolly", dol, dposes)
    rq = sequences.run([sd], "selftest_texseq", rep, True, [rd])
    qj = {q["name"]: q for q in qc.read_json(rq["json"])["sequences"]}
    cs = lambda seq, name: next((c for c in qj[seq]["checks"] if c["name"].startswith(name)), None)  # noqa: E731
    c = cs("texpan_ks04", "swimming")
    expect("texpan: world-locked pan does not swim", c is not None and c["status"] == "PASS", c and "%s %s" % (c["value"], c["note"]))
    c = cs("texpan_ks05", "swimming")
    expect("texpan: a screen-locked layer swims (FAIL)", c is not None and c["status"] == "FAIL", c and "%s %s" % (c["value"], c["note"]))
    c = cs("texpan_ks04", "shimmer: temporal std above")
    expect("texpan: identical reference, shimmer excess 0 PASS", c is not None and c["status"] == "PASS" and abs(c["value"]) < 1e-3,
           c and str(c["value"]))
    c = cs("texpan_ks00", "stabilised residual above the reference")
    expect("texpan: 6-level flicker of world-locked content over the reference FAIL (R1 > 2.0)", c is not None and c["status"] == "FAIL",
           c and str(c["value"]))
    c = cs("texpan_ks00", "shimmer: temporal std above")
    print("  info texpan flicker: screen-space excess %s (the pan's own 14 px of motion masks it: the stabilised gate catches it)" % (c and c["value"]))
    c = cs("texpan_ks05", "shimmer vs -capture-ss 2 reference")
    expect("texpan: no reference -> SKIP", c is not None and c["status"] == "SKIP")
    c = cs("dolly_face30", "dolly local pops")
    expect("dolly: the 64 px pop is found with its distance", c is not None and c["status"] == "WARN" and qj["dolly_face30"]["dolly_pops"][0]["frame"] == 20
           and qj["dolly_face30"]["dolly_pops"][0]["dist_m"] is not None, c and str(qj["dolly_face30"].get("dolly_pops", [])[:1]))


def main(argv=None):
    ap = argparse.ArgumentParser(prog="selftest.py", description=__doc__.split("\n\n")[0],
                                 formatter_class=argparse.RawDescriptionHelpFormatter, epilog=__doc__)
    ap.add_argument("--work", default=os.path.join(qc.ROOT, "QA", "tmp", "selftest"))
    ap.add_argument("--no-encode", action="store_true")
    ap.add_argument("--no-4k", action="store_true")
    a = ap.parse_args(argv)
    t0 = time.time()
    root = os.path.abspath(a.work)
    if os.path.isdir(root):
        shutil.rmtree(root)
    qc.ensure_dir(root)
    rep = qc.ensure_dir(os.path.join(root, "reports"))
    print("scene constants: %s %s" % (qc.spec().source, qc.spec().warnings))
    test_banding()
    test_line_probe()
    test_parity_companions()
    test_photo_pairs()
    test_blink_sign_rule()
    test_decode_step_channels()
    cap = make_capture(root, not a.no_4k)
    ad = make_audio(root)

    import image_metrics
    import contact_sheet
    import sequences
    import encode_test
    import audio_analysis
    import run_all

    print("[image_metrics]", flush=True)
    # the synthetic KS04 sky gradient spans the top 30 % of the frame: the selftest keeps its own copy of the regions with the
    # KS04 sky rect at y 0.02-0.25 (the default moved to 0.22 for the real canyon skybridge, TQ 2026-11-16)
    regs = qc.read_json(qc.DEFAULT_REGIONS, {})
    for e in regs.get("regions", []):
        for b in e.get("banding", []):
            if e.get("match") == "KS04_*" and b.get("name") == "sky_above_facades":
                b["rect"] = [0.42, 0.02, 0.55, 0.25]
    regions_st = qc.write_json(os.path.join(root, "regions_selftest.json"), regs)
    r = image_metrics.run(cap, None, regions_st, "selftest", rep, 1, False, None, "display", True)
    js = qc.read_json(r["json"])
    imgs = {i["tag"]: i for i in js["images"]}
    k = imgs["KS04_boulevard_axis"]
    for name in ("size", "black frame", "white frame", "magenta", "NaN-like pixels", "isolated pixels > 8x median",
                 "clipped highlights", "luma p1", "banding[sky_above_facades]", "line[blade] width@1080p", "line[blade] contrast",
                 "line[blade] monotone 3-level edge", "line[blade] stair-step", "window parity"):
        c = get_check(k, name)
        expect("KS04 %s PASS" % name, c is not None and c["status"] == "PASS",
               "%s %s %s" % (c and c["status"], c and qc.fmt(c["value"], 4), c and c.get("note")))
    ml = get_check(k, "mean luma")
    print("  info KS04 mean luma %s (%s)" % (ml and ml["value"], ml and ml["status"]))
    expect("KS04 neon exclusion used", k["metrics"]["clip_excluded_px"] > 0, str(k["metrics"]["clip_excluded_px"]))
    c = get_check(imgs["KS04_boulevard_axis_nodither"], "banding[sky_above_facades]")
    expect("KS04 undithered banding FAIL", c is not None and c["status"] == "FAIL", c and qc.fmt(c["value"]))
    if not a.no_4k:
        k4 = imgs["KS04_boulevard_axis_4k"]
        for name in ("banding[sky_above_facades]", "line[blade] width@1080p", "clipped highlights", "isolated pixels > 8x median"):
            c = get_check(k4, name)
            expect("KS04 4K %s PASS" % name, c is not None and c["status"] == "PASS",
                   "%s %s" % (c and c["status"], c and qc.fmt(c["value"], 4)))
    expect("black frame caught", get_check(imgs["beat02_boulevard_mid_black"], "black frame")["status"] == "FAIL")
    expect("white frame caught", get_check(imgs["beat03_edge_end_white"], "white frame")["status"] == "FAIL")
    expect("white clipping caught", get_check(imgs["beat03_edge_end_white"], "clipped highlights")["status"] == "FAIL")
    d = imgs["beat04_skybridge_mid_defects"]
    expect("magenta caught", get_check(d, "magenta")["status"] == "FAIL", str(d["metrics"]["magenta_like"]))
    expect("NaN-like pixels caught (1 dark, 1 bright)", d["metrics"]["nan_like_dark"] == 1 and d["metrics"]["nan_like_bright"] == 1,
           "%s / %s" % (d["metrics"]["nan_like_dark"], d["metrics"]["nan_like_bright"]))
    expect("isolated bright pixel caught (> 8x median)", d["metrics"]["isolated_px"] >= 1, str(d["metrics"]["isolated_px"]))
    expect("magenta = the 10 x 10 error patch only (CloudHot patch not flagged)", d["metrics"]["magenta_like"] == 100,
           str(d["metrics"]["magenta_like"]))
    expect("2 x 2 marker inside the neon mask, lone pixel outside", d["metrics"]["isolated_px"] == 1 and
           [1500, 1000] in d["metrics"]["isolated_pos"], "%s %s" % (d["metrics"]["isolated_px"], d["metrics"]["isolated_pos"]))
    sp5 = imgs["beat05_garden_start"]
    expect("wrong size caught", get_check(sp5, "size")["status"] == "FAIL")
    expect("subject inside margin PASS", get_check(sp5, "subject[lantern] margin")["status"] == "PASS")
    expect("subject outside margin FAIL", get_check(sp5, "subject[clipped_box] margin")["status"] == "FAIL")
    expect("report.json stats region used", get_check(sp5, "stats[all] std") is not None)
    expect("image_metrics overall FAIL (defects present)", r["status"] == "FAIL")

    print("[contact_sheet]", flush=True)
    rc = contact_sheet.run(cap, os.path.join(root, "contact"), 4, 2048, 2048, r["json"], True, 5, "selftest")
    from PIL import Image
    ok = bool(rc["sheets"]) and all(Image.open(p).size[0] <= 2048 for p in rc["sheets"])
    expect("contact sheets written, <= 2048 px wide", ok, "%d sheets, %d seq sheets" % (len(rc["sheets"]), len(rc["seq_sheets"])))

    print("[sequences]", flush=True)
    rs = sequences.run([cap], "selftest", rep, True)
    sj = {q["name"]: q for q in qc.read_json(rs["json"])["sequences"]}
    st = lambda seq, name: next((c["status"] for c in sj[seq]["checks"] if c["name"] == name), None)  # noqa: E731
    expect("static frozen PASS (change 0 <= 2 levels)", st("static_beat01_frozen", "static change") == "PASS")
    expect("static with a temporal 2-code dither FAIL (change > 2 levels)", st("static_beat01_temporal", "static change") == "FAIL",
           qc.fmt(sj["static_beat01_temporal"]["summary"].get("static_max_change"), 2))
    expect("wrong run conditions refused (C20)", st("static_beat02_wrongcond", "run conditions") == "FAIL" and
           sj["static_beat02_wrongcond"].get("refused") is True)
    expect("pan without the ss2 reference reads SKIP, not PASS", sj["b5_pan_rail"]["status"] == "SKIP", sj["b5_pan_rail"]["status"])
    expect("pop: luma step caught", st("b23_pop", "luma step/frame") == "FAIL")
    expect("pop: diff spike caught", st("b23_pop", "diff spikes") == "FAIL", str(sj["b23_pop"].get("spikes")))
    expect("clean boundary walk: activation PASS", st("b12", "activation-frame diff") == "PASS")
    expect("clean boundary walk: luma step PASS", st("b12", "luma step/frame") == "PASS")
    expect("activation pop at the boundary caught", st("b12_activation", "activation-frame diff") == "FAIL")
    expect("AA pan: jitter PASS", st("b5_pan_rail", "line[rail] centroid jitter") == "PASS",
           qc.fmt(sj["b5_pan_rail"]["lines"]["rail"]["jitter_max_px"], 4))
    expect("AA pan: speed 0.1 px/frame", abs(sj["b5_pan_rail"]["lines"]["rail"]["pan_px_per_frame"] - 0.1) < 0.01,
           qc.fmt(sj["b5_pan_rail"]["lines"]["rail"]["pan_px_per_frame"], 4))
    expect("aliased pan: jitter FAIL", st("b5_pan_rail_aliased", "line[rail] centroid jitter") == "FAIL",
           qc.fmt(sj["b5_pan_rail_aliased"]["lines"]["rail"]["jitter_max_px"], 4))
    expect("stability crop PASS on dithered static background", st("b5_pan_rail", "stability[vp]") == "PASS")
    expect("rain: additive streaks only PASS (outside-streak change <= 6)", st("rain_beat04", "rain outside-streak change") == "PASS",
           str(sj["rain_beat04"]["summary"].get("rain_outside_change_max")))
    expect("rain: 12-level background chroma flicker caught", st("rain_beat04_flicker", "rain outside-streak change") == "FAIL",
           str(sj["rain_beat04_flicker"]["summary"].get("rain_outside_change_max")))

    if not a.no_encode:
        print("[encode_test]", flush=True)
        eargs = encode_test.parser().parse_args([cap, "--run", "selftest", "--out-dir", rep, "--work-dir",
                                                 os.path.join(root, "encode"), "--sample", "4", "--quiet"])
        re_ = encode_test.run([cap], eargs)
        ej = qc.read_json(re_["json"])
        seqs = ej["sequences"]
        expect("encode_test ran one sequence", len(seqs) == 1, str([q["name"] for q in seqs]))
        if seqs:
            v = seqs[0].get("variants", [])
            expect("encode variants decoded with full frame count", v and all(x.get("decoded_frames") == 30 for x in v),
                   str([(x["codec"], x["crf"], x.get("decoded_frames"), qc.fmt(x.get("mean_abs_error"))) for x in v]))
            expect("review crops written", v and all(len(x.get("crops", [])) == 3 and all(os.path.isfile(p) for p in x["crops"])
                                                     for x in v))
            src = seqs[0]["source"]["abyss"]
            expect("encode source (dithered) passes", src and src["pass"], qc.fmt(src and src["score"]))
            for x in v:
                dsc = (x.get("decoded") or {}).get("abyss")
                print("  info %s crf %d decoded score %s (%s)" % (x["codec"], x["crf"], qc.fmt(dsc and dsc["score"]),
                                                                 "PASS" if dsc and dsc["pass"] else "FAIL"))

    print("[audio_analysis]", flush=True)
    ra = audio_analysis.run([ad], "selftest", rep, 1.0, 10.0, True)
    aj = qc.read_json(ra["json"])
    files = {os.path.basename(f["file"]): f for f in aj["files"]}
    fc = lambda f, name: next((c for c in files[f]["checks"] if c["name"] == name), None)  # noqa: E731
    mix = files["render_mix.wav"]
    for name in ("sample peak", "DC offset", "sample step", "NaN/Inf samples", "not silent"):
        c = fc("render_mix.wav", name)
        expect("mix %s PASS" % name, c is not None and c["status"] == "PASS", c and qc.fmt(c["value"], 4))
    beats = {b["beat"]: b for b in aj["beats"]}
    for bi in range(1, 8):
        b = beats.get(bi)
        expect("beat %d RMS PASS" % bi, b is not None and b["status"] == "PASS",
               "%s vs %s" % (b and b["rms_dbfs"], b and b["target"]))
    expect("beat 7 S7 swell excluded (exact target)", beats.get(7) and abs(beats[7]["rms_dbfs"] - beats[7]["target"]) < 0.2,
           str(beats.get(7) and beats[7]["rms_dbfs"]))
    s7 = fc("render_mix.wav", "S-window s 1340-1380")
    expect("S7 window logged separately (INFO, about -12 dBFS)", s7 is not None and s7["status"] == "INFO" and abs(s7["value"] + 12.0) < 0.5,
           s7 and qc.fmt(s7["value"], 2))
    lu = files["lufs_997_left.wav"]["lufs_integrated"]
    expect("997 Hz 0 dBFS left only = -3.01 LUFS", lu is not None and abs(lu + 3.01) < 0.05, qc.fmt(lu, 3))
    expect("0 dBFS peak caught", fc("lufs_997_left.wav", "sample peak")["status"] == "FAIL")
    expect("DC caught", fc("dc_click.wav", "DC offset")["status"] == "FAIL", qc.fmt(fc("dc_click.wav", "DC offset")["value"], 5))
    expect("click caught", fc("dc_click.wav", "sample step")["status"] == "FAIL", qc.fmt(fc("dc_click.wav", "sample step")["value"], 3))
    pk24 = files["tone_pcm24.wav"]["sample_peak_dbfs"]
    expect("24-bit reader (-17 dBFS peak)", abs(pk24 - 20 * math.log10(math.sqrt(2) * 0.1)) < 0.05, qc.fmt(pk24, 3))
    expect("audio overall FAIL (defects present)", ra["status"] == "FAIL")

    test_texture_tools(root, rep)

    print("[run_all]", flush=True)
    rc = run_all.main([cap, "--audio", ad, "--run", "selftest_all", "--out-dir", rep, "--regions", regions_st, "--encode",
                       "none" if a.no_encode else "auto", "--quiet"])
    expect("run_all exit code 1 (defects present)", rc == 1)
    expect("summary written", os.path.isfile(os.path.join(rep, "summary_selftest_all.md")))

    n_bad = sum(1 for _, ok, _ in RESULTS if not ok)
    print("\nselftest: %d assertions, %d failed, %.1f s; outputs in %s" % (len(RESULTS), n_bad, time.time() - t0, root))
    for name, ok, det in RESULTS:
        if not ok:
            print("  FAILED: %s  %s" % (name, det))
    return 1 if n_bad else 0


if __name__ == "__main__":
    sys.exit(main())
