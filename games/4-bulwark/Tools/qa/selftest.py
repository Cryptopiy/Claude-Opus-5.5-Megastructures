#!/usr/bin/env python3
"""selftest: synthetic end-to-end test of the Bulwark QA tools (images, sequences, encode, audio, run_all).
Imported from Thread; the synthetic scenes and expectations are rewritten for the Bulwark rules of 14.10.3.

Usage:  python -I Tools/qa/selftest.py [--work QA/tmp/selftest] [--no-encode] [--no-4k]

Generates synthetic stills, sequences and WAVs under QA/tmp/selftest (gitignored), runs every tool on them and
asserts the expected verdicts: BulwarkConst.cs parses (21 key shots, 8 boundaries); a smooth gradient without dither
must FAIL the BandScore and the same gradient with a 1 LSB TPDF dither must PASS (1080p, 4K and a dark gentle
gradient); a 2 px antialiased line must measure about 2 px; deliberate defects (black, white, magenta, NaN-like and
firefly pixels, wrong size, popping and flickering sequences, pan shimmer, aliased line, DC, click, a 0 dBFS peak and
an inter-sample peak that only the 4x true-peak meter sees, a beat 3 dB off its target) must be caught; a 997 Hz
0 dBFS sine on one channel must read -3.01 LUFS; two renders at 1.5 and 2.5 m/s must agree per s.
Exit code 1 if any assertion fails.
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


# --------------------------------------------------------------------------------------------- tests
def test_spec():
    print("[spec]")
    sp = qc.spec()
    expect("BulwarkConst.cs parsed without errors", not sp.errors, "; ".join(sp.errors))
    expect("21 key shots", len(sp.keyshots) == 21, str(len(sp.keyshots)))
    expect("8 beat boundaries ending at 1500", len(sp.beat_boundaries) == 8 and sp.beat_boundaries[-1] == 1500.0,
           str(sp.beat_boundaries))
    ks = sp.keyshots.get("KS04_crest_sea")
    expect("KS04 luma row 85..150 levels", ks and abs(ks["luma"][0] * 255 - 85) < 1e-6 and abs(ks["luma"][1] * 255 - 150) < 1e-6)
    expect("7 dither peaks and RMS targets", len(sp.dither_peak) == 7 and len(sp.target_rms_db) == 7)


def test_banding():
    print("[banding metric]")
    for label, H, top, bot in (("crest sky 1080p", 1080, (150, 160, 175), (205, 210, 215)),
                               ("crest sky 4K", 2160, (150, 160, 175), (205, 210, 215)),
                               ("dark veil", 900, (14, 18, 22), (4, 6, 8))):
        g = gradient(H, 256, top, bot)
        b0 = qc.band_score(quantise(g, 0.0).astype(np.float64))
        b1 = qc.band_score(quantise(g, 1.0).astype(np.float64))
        expect("%s undithered fails" % label, b0["pass"] is False,
               "score %.2f run %d (max %g)" % (b0["score"], b0["longest_run_px"], b0["longest_run_max_px"]))
        expect("%s 1 LSB TPDF passes" % label, b1["pass"] is True,
               "score %.2f run %d mean %s" % (b1["score"], b1["longest_run_px"], qc.fmt(b1["mean_run_px"], 2)))
        expect("%s scores clearly apart" % label, b0["score"] > 2.0 * b1["score"], "%.2f vs %.2f" % (b0["score"], b1["score"]))
    g = np.transpose(gradient(640, 300, (150, 160, 175), (205, 210, 215)), (1, 0, 2)).copy()
    m = np.zeros(g.shape[:2], dtype=bool)
    m[100:140, 200:260] = True
    bh = qc.band_score(quantise(g, 1.0).astype(np.float64), None, "auto", m)
    expect("auto direction + mask (dithered passes)", bh["pass"] is True and bh["direction"] == "h",
           "dir %s strips %d" % (bh["direction"], bh["n_strips"]))


def test_fireflies():
    print("[fireflies]")
    g = quantise(gradient(300, 400, (30, 30, 34), (20, 22, 26)), 1.0)
    n0, _ = qc.fireflies(qc.grey_levels(g))
    expect("dithered dark gradient has no fireflies", n0 == 0, str(n0))
    g[150, 200] = (250, 250, 250)
    n1, pos = qc.fireflies(qc.grey_levels(g))
    expect("one firefly caught", n1 == 1 and pos and pos[0] == [200, 150], "%d %s" % (n1, pos[:2]))


def sun_row(H, fov, el, pitch):
    f = 0.5 * H / math.tan(math.radians(fov) * 0.5)
    return 0.5 * H - f * math.tan(math.radians(el - pitch))


def make_capture(root, with_4k):
    cap = qc.ensure_dir(os.path.join(root, "cap_run"))
    shots = []
    print("[synthesis] stills", flush=True)

    def crest(W, H, dither):
        # KS04-like: storm sky gradient over the upper 55 %, a darker sea below, a clipped sun disc at az 80 el 6
        sky = gradient(int(H * 0.55), W, (150, 160, 175), (200, 205, 212))
        sea = gradient(H - sky.shape[0], W, (70, 80, 92), (50, 58, 66))
        img = quantise(np.concatenate([sky, sea], axis=0), dither)
        r = sun_row(H, 64.0, 6.0, -8.0)
        yy, xx = np.mgrid[0:H, 0:W]
        disc = (xx + 0.5 - W / 2.0) ** 2 + (yy + 0.5 - r) ** 2 <= (8.0 * H / 1080.0) ** 2
        img[disc] = 255
        return img

    reg = {"banding": [{"name": "upper_sky", "rect": [0.05, 0.02, 0.95, 0.30], "units": "frac", "dir": "v"}],
           "exclude": [{"name": "sun", "azEl": "sun", "clipDeg": 1.2, "bandDeg": 4.0}]}
    for tag, W, H, dith in (("KS04_crest_sea", 1920, 1080, 1.0), ("KS04_crest_sea_nodither", 1920, 1080, 0.0)) + \
            ((("KS04_crest_sea_3840x2160", 3840, 2160, 1.0),) if with_4k else ()):
        qc.save_png(os.path.join(cap, tag + ".png"), crest(W, H, dith))
        shots.append({"tag": tag, "file": tag + ".png", "width": W, "height": H, "s": 160.0, "sun": [80.0, 6.0],
                      "pose": {"x": 4.2, "y": 1.7, "z": 160.0, "yaw": 80.0, "pitch": -8.0, "fov": 64.0}, "regions": reg})
    qc.save_png(os.path.join(cap, "KS04_crest_sea_window.png"), crest(1920, 1080, 1.0))   # parity companion
    qc.save_png(os.path.join(cap, "b02_Crest_mid_black.png"), np.zeros((1080, 1920, 3), np.uint8))
    shots.append({"tag": "b02_Crest_mid_black", "file": "b02_Crest_mid_black.png", "width": 1920, "height": 1080})
    qc.save_png(os.path.join(cap, "b03_SpillwayBridge_end_white.png"), np.full((1080, 1920, 3), 255, np.uint8))
    shots.append({"tag": "b03_SpillwayBridge_end_white", "file": "b03_SpillwayBridge_end_white.png", "width": 1920, "height": 1080})
    g = quantise(gradient(1080, 1920, (130, 135, 140), (110, 115, 120)), 1.0)
    g[500:510, 900:910] = (255, 0, 255)
    g[200, 300] = (0, 0, 0)                      # isolated NaN-like black pixel
    g[600:800, :] = quantise(gradient(200, 1920, (22, 22, 24), (18, 19, 20)), 1.0)   # dark band (lamp-less wall)
    g[700, 1200] = (255, 255, 255)               # isolated bright pixel in the dark band: NaN-like and a firefly
    qc.save_png(os.path.join(cap, "b04_ArmourSlope_mid_defects.png"), g)
    shots.append({"tag": "b04_ArmourSlope_mid_defects", "file": "b04_ArmourSlope_mid_defects.png", "width": 1920, "height": 1080})
    g2 = quantise(gradient(720, 1280, (60, 55, 50), (30, 28, 26)), 1.0)
    qc.save_png(os.path.join(cap, "b05_GateTower_start.png"), g2)
    shots.append({"tag": "b05_GateTower_start", "file": "b05_GateTower_start.png", "width": 1920, "height": 1080,
                  "subjects": [{"name": "lamp", "box": [100, 100, 600, 500]},
                               {"name": "clipped_box", "box": [-50, 100, 400, 500]}],
                  "regions": {"stats": [{"name": "all", "rect": [0, 0, 1, 1], "units": "frac", "stdMax": 0.5}]}})
    with open(os.path.join(cap, "report.json"), "w") as f:
        json.dump({"run": "selftest", "build": "synthetic", "shots": shots}, f, indent=1)

    print("[synthesis] sequences", flush=True)
    base = gradient(256, 320, (120, 124, 130), (90, 96, 104))
    # static, clock running: dithered noise only -> PASS; one 64 x 64 block flickering +-6 levels -> FAIL
    for name, flick, fire in (("seq_b2_static_ok", 0.0, False), ("seq_b2_static_flicker", 6.0, False),
                              ("seq_b2_static_firefly", 0.0, True)):
        sd = qc.ensure_dir(os.path.join(cap, name))
        for i in range(40):
            f = base.copy()
            if flick:
                f[64:128, 64:128] += flick * (1 if i % 2 else -1)
            q = quantise(f * (0.2 if fire else 1.0), 1.0)
            if fire and i == 20:
                q[200, 200] = 255
            qc.save_png(os.path.join(sd, "frame_%05d.png" % i), q)
        json.dump({"mode": "Static", "fps": 30, "s0": 200, "s1": 200}, open(os.path.join(sd, "sequence.json"), "w"))
    # transition with a pop at frame 25 (luma step and spike) and a boundary crossing at frame 25
    sd = qc.ensure_dir(os.path.join(cap, "seq_b1_b2_pop"))
    frames = []
    for i in range(40):
        k = 1.0 + 0.001 * i + (0.12 if i >= 25 else 0.0)
        qc.save_png(os.path.join(sd, "frame_%05d.png" % i), quantise(np.clip(base * k, 0, 255), 1.0))
        frames.append({"i": i, "s": 119.02 + 0.04 * i})          # crosses s 120 between frames 24 and 25
    json.dump({"mode": "Walk", "fps": 30, "s0": 119, "s1": 122, "frames": frames}, open(os.path.join(sd, "sequence.json"), "w"))
    # smooth transition across the same boundary: PASS
    sd = qc.ensure_dir(os.path.join(cap, "seq_b1_b2_smooth"))
    frames = []
    for i in range(40):
        k = 1.0 + 0.001 * i
        qc.save_png(os.path.join(sd, "frame_%05d.png" % i), quantise(np.clip(base * k, 0, 255), 1.0))
        frames.append({"i": i, "s": 119.02 + 0.04 * i})
    json.dump({"mode": "Walk", "fps": 30, "s0": 119, "s1": 122.3, "frames": frames}, open(os.path.join(sd, "sequence.json"), "w"))
    # pans at 0.5 px/frame (yaw from vFOV): clean content -> PASS; per-frame noise of +-6 levels in a region -> FAIL
    H, W, fov = 256, 320, 60.0
    fpx = 0.5 * H / math.tan(math.radians(fov) * 0.5)
    dyaw = math.degrees(math.atan(0.5 / fpx))
    for name, noisy in (("seq_b4_pan_clean", False), ("seq_b4_pan_shimmer", True)):
        sd = qc.ensure_dir(os.path.join(cap, name))
        frames = []
        for i in range(30):
            x = np.arange(W)[None, :] + 0.5 * i                 # content moves left by 0.5 px per frame (yaw right)
            y = np.arange(H)[:, None]
            v = 110.0 + 18.0 * np.sin(x / 23.0) * np.cos(y / 31.0) + 0.05 * y
            img = np.repeat(v[..., None], 3, axis=2)
            if noisy:
                img[100:180, 100:200] += RNG.uniform(-6.0, 6.0, (80, 100, 1))
            qc.save_png(os.path.join(sd, "frame_%05d.png" % i), quantise(img, 1.0))
            frames.append({"i": i, "s": 760.0, "yaw": 78.0 + dyaw * i, "pitch": 0.0, "fov": fov})
        json.dump({"mode": "PanYaw", "fps": 30, "s0": 760, "s1": 760, "frames": frames},
                  open(os.path.join(sd, "sequence.json"), "w"))
    # thin-line ride (0.1 px/frame): AA passes the jitter test, aliased fails
    for name, aliased in (("seq_b5_ride_rail", False), ("seq_b5_ride_rail_aliased", True)):
        sd = qc.ensure_dir(os.path.join(cap, name))
        bgl = dec(gradient(300, 256, (200, 200, 205), (190, 195, 205)) / 255.0)
        for i in range(30):
            cov = coverage_line(256, 300, lambda y: 120.2 + 0.1 * i + 0.0 * y, 2.0, aliased)
            lin = bgl * (1 - cov[..., None]) + dec(np.array([40, 40, 50.]) / 255.0) * cov[..., None]
            qc.save_png(os.path.join(sd, "frame_%05d.png" % i), quantise(enc(lin) * 255.0, 1.0))
        json.dump({"kind": "ride", "fps": 30, "lines": [{"name": "rail", "x": 121, "tol": 16, "y0": 20, "h": 256}],
                   "stability": [{"name": "vp", "rect": [20, 20, 84, 84]}]}, open(os.path.join(sd, "sequence.json"), "w"))
    # encode pair: the dithered clip here, the undithered twin in a second capture folder
    nd = qc.ensure_dir(os.path.join(root, "cap_run_nodither"))
    veil = gradient(360, 640, (16, 22, 28), (5, 7, 9))
    for folder, amp in ((cap, 2.0), (nd, 0.0)):
        sd = qc.ensure_dir(os.path.join(folder, "seq_enc_b06_water_veil"))
        still = quantise(veil, amp)                 # static dither (the BwGrade default): the same pattern every frame
        for i in range(30):
            qc.save_png(os.path.join(sd, "frame_%05d.png" % i), still)
        json.dump({"set": "encode", "mode": "Walk", "fps": 30, "s0": 1110, "s1": 1110,
                   "banding": [{"name": "veil", "rect": [0, 0, 640, 360], "dir": "v"}]},
                  open(os.path.join(sd, "sequence.json"), "w"))
    return cap, nd


def make_audio(root):
    print("[synthesis] audio", flush=True)
    ad = qc.ensure_dir(os.path.join(root, "audio"))
    sp = qc.spec()
    import audio_analysis
    fs = 8000
    for name, v, offset_b3 in (("render_2.5.wav", 2.5, 0.0), ("render_1.5.wav", 1.5, 0.0), ("render_b3_loud.wav", 2.5, 3.0)):
        dur = 1500.0 / v
        n = int(fs * dur)
        t = np.arange(n) / float(fs)
        s = np.minimum(t * v, 1500.0)
        tgt = np.array([audio_analysis.target_db(float(x), sp) for x in np.arange(0, 1501.0)])
        tg = np.interp(s, np.arange(0, 1501.0), tgt)
        if offset_b3:
            tg = np.where((s >= 420) & (s < 700), tg + offset_b3, tg)
        x = np.sqrt(2.0) * 10 ** (tg / 20.0) * np.sin(2 * np.pi * 220.0 * t)
        write_wav_float(os.path.join(ad, name), x[:, None], fs)
        json.dump({"speed": v, "sPerSecond": [float(min(1500.0, k * v)) for k in range(int(dur) + 2)], "dt": 1.0},
                  open(os.path.join(ad, name[:-4] + ".json"), "w"))
    t2 = np.arange(48000 * 10) / 48000.0
    l = (32767.0 / 32768.0) * np.sin(2 * np.pi * 997.0 * t2)
    write_wav_pcm16(os.path.join(ad, "lufs_997_left.wav"), np.stack([l, np.zeros_like(l)], axis=1), 48000)
    # inter-sample peak: fs/4 sine at 45 deg phase, amplitude 0.95 -> samples at 0.672 (-3.45 dBFS), true peak -0.45
    t5 = np.arange(48000 * 2)
    isp = 0.95 * np.sin(2 * np.pi * 0.25 * t5 + math.pi / 4)
    write_wav_float(os.path.join(ad, "intersample.wav"), np.stack([isp, isp], axis=1), 48000)
    t3 = np.arange(48000 * 5) / 48000.0
    y = 0.05 * np.sin(2 * np.pi * 110.0 * t3) + 0.01
    y[48000 * 2] = 0.5
    write_wav_float(os.path.join(ad, "dc_click.wav"), np.stack([y, y], axis=1), 48000)
    t4 = np.arange(48000 * 2) / 48000.0
    z = np.sqrt(2.0) * 0.1 * np.sin(2 * np.pi * 440.0 * t4)
    write_wav_pcm24(os.path.join(ad, "tone_pcm24.wav"), np.stack([z, z], axis=1), 48000)
    return ad


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
    test_spec()
    test_banding()
    test_line_probe()
    test_fireflies()
    cap, capnd = make_capture(root, not a.no_4k)
    ad = make_audio(root)

    import image_metrics
    import contact_sheet
    import sequences
    import encode_test
    import audio_analysis
    import run_all

    print("[image_metrics]", flush=True)
    r = image_metrics.run(cap, None, qc.DEFAULT_REGIONS, "selftest", rep, 1, False, None, "display", True)
    js = qc.read_json(r["json"])
    imgs = {i["tag"]: i for i in js["images"]}
    k = imgs["KS04_crest_sea"]
    for name in ("size", "magenta", "NaN-like pixels", "fireflies", "clipped highlights", "banding[upper_sky]",
                 "mean luma", "window parity"):
        c = get_check(k, name)
        expect("KS04 %s PASS" % name, c is not None and c["status"] == "PASS",
               c and "%s %s %s" % (c["status"], qc.fmt(c["value"]), c.get("note") or ""))
    expect("KS04 sun disc excluded from clipping", k["metrics"]["clip_excluded_px"] > 0, str(k["metrics"]["clip_excluded_px"]))
    c = get_check(imgs["KS04_crest_sea_nodither"], "banding[upper_sky]")
    expect("KS04 undithered banding FAIL", c is not None and c["status"] == "FAIL", c and qc.fmt(c["value"]))
    if "KS04_crest_sea_3840x2160" in imgs:
        k4 = imgs["KS04_crest_sea_3840x2160"]
        for name in ("banding[upper_sky]", "clipped highlights", "mean luma"):
            c = get_check(k4, name)
            expect("KS04 4K %s PASS" % name, c is not None and c["status"] == "PASS",
                   c and "%s %s" % (c["status"], qc.fmt(c["value"])))
    expect("black frame caught", get_check(imgs["b02_Crest_mid_black"], "black frame")["status"] == "FAIL")
    expect("white frame caught", get_check(imgs["b03_SpillwayBridge_end_white"], "white frame")["status"] == "FAIL")
    expect("white clipping caught", get_check(imgs["b03_SpillwayBridge_end_white"], "clipped highlights")["status"] == "FAIL")
    d = imgs["b04_ArmourSlope_mid_defects"]
    expect("magenta caught", get_check(d, "magenta")["status"] == "FAIL", str(d["metrics"]["magenta_like"]))
    expect("NaN-like pixels caught", d["metrics"]["nan_like_dark"] == 1 and d["metrics"]["nan_like_bright"] == 1,
           "dark %d bright %d" % (d["metrics"]["nan_like_dark"], d["metrics"]["nan_like_bright"]))
    expect("firefly caught (exactly 1)", d["metrics"]["fireflies"] == 1, str(d["metrics"]["fireflies"]))
    sp5 = imgs["b05_GateTower_start"]
    expect("wrong size caught", get_check(sp5, "size")["status"] == "FAIL")
    expect("subject inside margin PASS", get_check(sp5, "subject[lamp] margin")["status"] == "PASS")
    expect("subject outside margin FAIL", get_check(sp5, "subject[clipped_box] margin")["status"] == "FAIL")
    expect("report.json stats region used", get_check(sp5, "stats[all] std") is not None)
    expect("beat-5 mean luma row applied (28..80 levels)", get_check(sp5, "mean luma")["threshold"] is not None,
           str(get_check(sp5, "mean luma")["threshold"]))
    expect("image_metrics overall FAIL (defects present)", r["status"] == "FAIL")
    print("[contact_sheet]", flush=True)
    rc = contact_sheet.run(cap, os.path.join(root, "contact"), 4, 2048, 2048, r["json"], True, 5, "selftest")
    from PIL import Image
    ok = bool(rc["sheets"]) and all(Image.open(p).size[0] <= 2048 for p in rc["sheets"])
    expect("contact sheets written, <= 2048 px wide", ok, "%d sheets, %d seq sheets" % (len(rc["sheets"]), len(rc["seq_sheets"])))

    print("[sequences]", flush=True)
    rs = sequences.run([cap], "selftest", rep, True)
    sj = {q["name"]: q for q in qc.read_json(rs["json"])["sequences"]}
    st = lambda q, c: next((x["status"] for x in sj[q]["checks"] if x["name"] == c), None)  # noqa: E731
    sv = lambda q, c: next((x["value"] for x in sj[q]["checks"] if x["name"] == c), None)  # noqa: E731
    expect("static flicker PASS (dither only)", st("b2_static_ok", "flicker block std") == "PASS", qc.fmt(sv("b2_static_ok", "flicker block std")))
    expect("static flicker caught", st("b2_static_flicker", "flicker block std") == "FAIL", qc.fmt(sv("b2_static_flicker", "flicker block std")))
    expect("static firefly caught", st("b2_static_firefly", "fireflies") == "FAIL")
    expect("static fireflies PASS", st("b2_static_ok", "fireflies") == "PASS")
    expect("pop: luma step caught", st("b1_b2_pop", "luma step/frame") == "FAIL", qc.fmt(sv("b1_b2_pop", "luma step/frame")))
    expect("pop: diff spike caught", st("b1_b2_pop", "diff spikes") == "FAIL", str(sj["b1_b2_pop"].get("spikes")))
    expect("pop: activation-frame diff caught", st("b1_b2_pop", "activation-frame diff") == "FAIL",
           qc.fmt(sv("b1_b2_pop", "activation-frame diff")))
    expect("smooth boundary crossing PASS", st("b1_b2_smooth", "activation-frame diff") == "PASS" and
           st("b1_b2_smooth", "luma step/frame") == "PASS", qc.fmt(sv("b1_b2_smooth", "activation-frame diff")))
    expect("pan shimmer PASS (motion compensated)", st("b4_pan_clean", "pan shimmer std") == "PASS",
           qc.fmt(sv("b4_pan_clean", "pan shimmer std")))
    expect("pan shimmer caught", st("b4_pan_shimmer", "pan shimmer std") == "FAIL", qc.fmt(sv("b4_pan_shimmer", "pan shimmer std")))
    expect("AA ride: jitter PASS", st("b5_ride_rail", "line[rail] centroid jitter") == "PASS",
           qc.fmt(sj["b5_ride_rail"]["lines"].get("rail", {}).get("jitter_max_px"), 4))
    expect("aliased ride: jitter FAIL", st("b5_ride_rail_aliased", "line[rail] centroid jitter") == "FAIL")
    expect("stability crop PASS on dithered background", st("b5_ride_rail", "stability[vp]") == "PASS")

    if not a.no_encode:
        print("[encode_test]", flush=True)
        eargs = encode_test.parser().parse_args([cap, "--run", "selftest", "--out-dir", rep, "--work-dir",
                                                 os.path.join(root, "encode"), "--sample", "4", "--codecs", "x264",
                                                 "--undithered", capnd, "--quiet"])
        re_ = encode_test.run([cap], eargs)
        ej = qc.read_json(re_["json"])
        seqs = ej["sequences"]
        expect("encode_test ran one sequence", len(seqs) == 1, str([q["name"] for q in seqs]))
        if seqs:
            v = seqs[0].get("variants", [])
            expect("encode variants decoded with full frame count", v and all(x.get("decoded_frames") == 30 for x in v),
                   str([(x["codec"], x["crf"], x.get("decoded_frames"), qc.fmt(x.get("mean_abs_error"))) for x in v]))
            expect("worst-location crops written", v and all(len(x.get("crops", [])) == 3 and all(os.path.isfile(p) for p in x["crops"])
                                                           for x in v))
            src = seqs[0]["source"]["veil"]
            expect("encode source (dithered) passes", src and src["pass"], qc.fmt(src and src["score"]))
            pair = [c for c in seqs[0]["checks"] if "dither/undithered run" in c["name"]]
            expect("dithered/undithered pair measured", len(pair) == 2, str([(c["name"], c["value"], c["status"]) for c in pair]))
            r23 = next((c for c in pair if "crf23" in c["name"]), None)
            expect("dithered run shorter than undithered at CRF 23", r23 is not None and r23["value"] is not None and r23["value"] < 1.0,
                   r23 and "%s (%s)" % (r23["value"], r23["note"]))

    print("[audio_analysis]", flush=True)
    ra = audio_analysis.run([ad], "selftest", rep, 1.0, 10.0, True)
    aj = qc.read_json(ra["json"])
    files = {os.path.basename(f["file"]): f for f in aj["files"]}
    fc = lambda f, name: next((c for c in files[f]["checks"] if c["name"] == name), None)  # noqa: E731
    for name in ("true peak", "DC offset", "sample step", "NaN/Inf samples", "not silent"):
        c = fc("render_2.5.wav", name)
        expect("mix %s PASS" % name, c is not None and c["status"] == "PASS", c and qc.fmt(c["value"], 4))
    bc = {c["name"]: c for c in aj["beat_checks"]}
    for bi in range(1, 8):
        c = bc.get("beat %d sliding 3 s RMS" % bi)
        if bi == 3:
            expect("beat 3 sliding RMS +3 dB caught", c is not None and c["status"] == "FAIL", c and qc.fmt(c["value"], 2))
        else:
            expect("beat %d sliding 3 s RMS PASS" % bi, c is not None and c["status"] == "PASS", c and "%s %s" % (qc.fmt(c["value"], 2), c["note"]))
    pairs = [c for c in aj["beat_checks"] if c["name"].startswith("speed pair 1.5/2.5") or c["name"].startswith("speed pair 2.5/1.5")]
    expect("speed pair 1.5/2.5 compared, one pair PASS", any(c["status"] == "PASS" for c in pairs),
           str([(c["name"], c["value"], c["status"]) for c in pairs]))
    lu = files["lufs_997_left.wav"]["lufs_integrated"]
    expect("997 Hz 0 dBFS left only = -3.01 LUFS", lu is not None and abs(lu + 3.01) < 0.05, qc.fmt(lu, 3))
    expect("0 dBFS true peak caught", fc("lufs_997_left.wav", "true peak")["status"] == "FAIL")
    tpv = files["intersample.wav"]
    expect("inter-sample peak: true peak about -0.45 dBTP", abs(tpv["true_peak_dbtp"] + 0.45) < 0.2,
           "true %.2f sample %.2f" % (tpv["true_peak_dbtp"], tpv["sample_peak_dbfs"]))
    expect("inter-sample peak caught by the true-peak gate", fc("intersample.wav", "true peak")["status"] == "FAIL")
    expect("DC caught", fc("dc_click.wav", "DC offset")["status"] == "FAIL", qc.fmt(fc("dc_click.wav", "DC offset")["value"], 5))
    expect("click caught", fc("dc_click.wav", "sample step")["status"] == "FAIL", qc.fmt(fc("dc_click.wav", "sample step")["value"], 3))
    pk24 = files["tone_pcm24.wav"]["sample_peak_dbfs"]
    expect("24-bit reader (-17 dBFS peak)", abs(pk24 - 20 * math.log10(math.sqrt(2) * 0.1)) < 0.05, qc.fmt(pk24, 3))
    expect("audio overall FAIL (defects present)", ra["status"] == "FAIL")

    print("[run_all]", flush=True)
    rc = run_all.main([cap, "--audio", ad, "--run", "selftest_all", "--out-dir", rep, "--encode",
                       "none" if a.no_encode else "auto", "--codecs", "x264", "--undithered", capnd, "--quiet"])
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
