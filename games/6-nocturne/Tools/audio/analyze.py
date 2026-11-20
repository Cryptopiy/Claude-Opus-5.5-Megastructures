#!/usr/bin/env python3
"""AU's listening-by-numbers analysis of a score render (docs/AUDIO.md "Analysis"; MASTER_PROMPT 14.3, 14.7 H8, Part 10 item 25).

Reads a WAV written by Tools/audio/harness (32-bit float or 16-bit PCM, stereo) and its sidecar <name>.json
({"s": [progress at t = i dt], "dt": 1.0}) and prints / writes, per beat (the 14 m around each boundary left out):
  - level: RMS dBFS, the p10 / p50 / p90 of the 3 s RMS windows, the largest 1 s level change;
  - spectrum (Welch, Hann 8192, mono mid): octave bands 31.5 Hz - 16 kHz (dB re the beat RMS), spectral centroid,
    the 50 / 100 Hz hum partials and the koi partials (41.2, 55, 123.6, 164.8, 247.2 Hz) as peak-over-neighbourhood dB;
  - stereo: inter-channel correlation and the side / mid ratio;
  - onsets: spectral flux of the 1.5-8 kHz band (STFT 1024 / hop 256), adaptive peak picking, onsets per second and the
    dominant period of the flux autocorrelation in 0.25-1.0 s (the footstep cadence: 0.34 s at 2.5 m/s, 0.50 s at 1.5);
  - chords: for each steady chord segment (outside the 40 m change windows) the chroma of 60-1 000 Hz and the share of
    chroma energy on the chord's pitch classes (with --pad <pad-solo wav> also for the pad stem alone);
and a log-frequency spectrogram PNG of the whole render (--png).
Usage:
  python -I Tools/audio/analyze.py <walk.wav> [--pad <pad.wav>] [--json out.json] [--png out.png]
"""
import argparse
import json
import math
import os
import struct
import sys

import numpy as np

BOUNDS = [0.0, 120.0, 400.0, 560.0, 880.0, 1080.0, 1260.0, 1500.0]
NAMES = ["alley", "boulevard", "edge", "bridge", "garden", "signs", "plaza"]
CHORD_S = [0, 124, 286, 402, 540, 700, 880, 1080, 1262, 1350, 1486]
CHORD_H = [20, 20, 20, 20, 20, 20, 20, 20, 20, 20, 14]
CHORDS = [[38, 45, 53, 60, 64], [46, 53, 57, 60, 62], [43, 50, 53, 58, 60], [39, 46, 50, 55, 57], [41, 48, 57, 64, 67],
          [38, 45, 53, 60, 67], [38, 45, 54, 61, 64], [47, 54, 57, 62, 64], [43, 50, 54, 57, 61], [40, 47, 50, 55, 66], [38, 45, 50]]
CHORD_NAMES = ["Dm9", "Bbmaj9", "Gm11", "Ebmaj7#11", "Fmaj9", "Dm11", "Dmaj9", "Bm11", "Gmaj9#11", "Em9", "D5"]
PC = ["C", "C#", "D", "Eb", "E", "F", "F#", "G", "Ab", "A", "Bb", "B"]


def read_wav(path):
    with open(path, "rb") as f:
        head = f.read(12)
        if head[:4] != b"RIFF" or head[8:12] != b"WAVE":
            raise ValueError("not a WAV: " + path)
        fmt = None
        while True:
            h = f.read(8)
            if len(h) < 8:
                raise ValueError("no data chunk")
            cid, n = h[:4], struct.unpack("<I", h[4:])[0]
            if cid == b"fmt ":
                b = f.read(n)
                tag, ch, fs = struct.unpack("<HHI", b[:8])
                bits = struct.unpack("<H", b[14:16])[0]
                if tag == 0xFFFE:
                    tag = struct.unpack("<H", b[24:26])[0]
                fmt = (tag, ch, fs, bits)
                if n & 1:
                    f.read(1)
            elif cid == b"data":
                off = f.tell()
                break
            else:
                f.seek(n + (n & 1), 1)
    tag, ch, fs, bits = fmt
    if tag == 3 and bits == 32:
        x = np.memmap(path, dtype="<f4", mode="r", offset=off, shape=(n // (4 * ch), ch))
    elif tag == 1 and bits == 16:
        x = np.memmap(path, dtype="<i2", mode="r", offset=off, shape=(n // (2 * ch), ch))
    else:
        raise ValueError("unsupported WAV format %s" % (fmt,))
    return x, fs


def to_float(a):
    a = np.asarray(a)
    return a.astype(np.float64) / 32768.0 if a.dtype.kind == "i" else a.astype(np.float64)


def s_of_t(side, n_seconds):
    s = np.asarray(side["s"], float)
    dt = float(side.get("dt", 1.0))
    t = np.arange(len(s)) * dt + float(side.get("t0", 0.0))
    return lambda tt: np.interp(tt, t, s)


def frames_where(sfun, fs, total, a, b):
    """First and last frame with a <= s < b (s monotone in a constant-speed render)."""
    t = np.arange(0, total / fs, 0.01)
    s = sfun(t)
    m = (s >= a) & (s < b)
    if not m.any():
        return 0, 0
    i0, i1 = np.argmax(m), len(m) - 1 - np.argmax(m[::-1])
    return int(t[i0] * fs), int(min(total, (t[i1] + 0.01) * fs))


def welch(x, fs, n=8192):
    w = np.hanning(n)
    hop = n // 2
    acc, k = np.zeros(n // 2 + 1), 0
    for i in range(0, len(x) - n, hop):
        X = np.fft.rfft(w * x[i:i + n])
        acc += (X.real ** 2 + X.imag ** 2)
        k += 1
    f = np.fft.rfftfreq(n, 1.0 / fs)
    return f, acc / max(1, k) / (w ** 2).sum()


def band_db(f, p, lo, hi):
    m = (f >= lo) & (f < hi)
    return 10 * math.log10(max(p[m].sum(), 1e-30))


def peak_over_floor_db(f, p, hz, width=0.06):
    """Power within +-1.5 % of hz against the median of the neighbourhood +-width (dB)."""
    df = f[1] - f[0]
    m = np.abs(f - hz) <= max(0.015 * hz, 1.01 * df)
    nb = (np.abs(f - hz) <= max(width * hz, 6 * df)) & ~m
    if not m.any() or not nb.any():
        return float("nan")
    return 10 * math.log10(max(p[m].max(), 1e-30) / max(np.median(p[nb]), 1e-30))


def chroma(f, p, lo=60.0, hi=1000.0):
    m = (f >= lo) & (f <= hi)
    midi = 69 + 12 * np.log2(f[m] / 440.0)
    pc = np.mod(np.round(midi).astype(int), 12)
    c = np.bincount(pc, weights=p[m], minlength=12)
    return c / max(c.sum(), 1e-30)


def onsets(x, fs, sec_lo=0.25, sec_hi=1.0):
    n, hop = 1024, 256
    w = np.hanning(n)
    f = np.fft.rfftfreq(n, 1.0 / fs)
    band = (f >= 1500) & (f <= 8000)
    frames = (len(x) - n) // hop
    if frames < 50:
        return 0.0, float("nan"), 0.0
    flux = np.zeros(frames)
    prev = None
    for i in range(frames):
        X = np.abs(np.fft.rfft(w * x[i * hop:i * hop + n]))[band]
        if prev is not None:
            flux[i] = np.maximum(0.0, X - prev).sum()
        prev = X
    rate = fs / hop
    # adaptive threshold: running median + 3 MAD over 0.5 s; min gap 60 ms
    L = int(0.5 * rate)
    pk = []
    last = -10 ** 9
    for i in range(1, frames - 1):
        a, b = max(0, i - L), min(frames, i + L)
        seg = flux[a:b]
        med = np.median(seg)
        mad = np.median(np.abs(seg - med)) + 1e-12
        if flux[i] > med + 3.0 * 1.4826 * mad and flux[i] >= flux[i - 1] and flux[i] >= flux[i + 1] and i - last >= 0.06 * rate:
            pk.append(i)
            last = i
    per_s = len(pk) / (frames / rate)
    # dominant period of the flux autocorrelation
    g = flux - flux.mean()
    ac = np.fft.irfft(np.abs(np.fft.rfft(g, 2 * len(g))) ** 2)[:len(g)]
    ac /= max(ac[0], 1e-30)
    lo, hi = int(sec_lo * rate), int(sec_hi * rate)
    j = lo + int(np.argmax(ac[lo:hi]))
    return per_s, j / rate, float(ac[j])


def analyse(path, pad=None, png=None):
    x, fs = read_wav(path)
    side = json.load(open(os.path.splitext(path)[0] + ".json"))
    total = x.shape[0]
    sf = s_of_t(side, total / fs)
    out = {"file": os.path.basename(path), "seconds": total / fs, "beats": [], "chords": []}
    print("%s: %.1f s, %d Hz" % (os.path.basename(path), total / fs, fs))
    print("beat        rms   p10   p50   p90  max1s | centroid  corr  side | 31  63  125 250 500 1k  2k  4k  8k  16k (dB re rms) | hum50 hum100 koi41 koi55 koi124 | onsets/s period corr")
    for b in range(7):
        a = BOUNDS[b] + (14 if b > 0 else 0)
        e = BOUNDS[b + 1] - (14 if b < 6 else 3)
        i0, i1 = frames_where(sf, fs, total, a, e)
        seg = to_float(x[i0:i1])
        L, R = seg[:, 0], seg[:, 1]
        mid = 0.5 * (L + R)
        sid = 0.5 * (L - R)
        rms = 10 * math.log10(max(0.5 * (np.mean(L * L) + np.mean(R * R)), 1e-30))
        w3 = int(3 * fs)
        win = [10 * math.log10(max(0.5 * (np.mean(L[k:k + w3] ** 2) + np.mean(R[k:k + w3] ** 2)), 1e-30)) for k in range(0, len(L) - w3, w3)]
        w1 = int(fs)
        l1 = np.array([10 * math.log10(max(0.5 * (np.mean(L[k:k + w1] ** 2) + np.mean(R[k:k + w1] ** 2)), 1e-30)) for k in range(0, len(L) - w1, w1)])
        f, p = welch(mid, fs)
        tot = p.sum()
        cen = float((f * p).sum() / max(tot, 1e-30))
        oct_c = [31.5, 63, 125, 250, 500, 1000, 2000, 4000, 8000, 16000]
        ob = [band_db(f, p, c / math.sqrt(2), c * math.sqrt(2)) - 10 * math.log10(max(tot, 1e-30)) for c in oct_c]
        corr = float(np.corrcoef(L, R)[0, 1])
        sm = 10 * math.log10(max(np.mean(sid ** 2), 1e-30) / max(np.mean(mid ** 2), 1e-30))
        fh, ph = welch(mid, fs, 32768)   # 1.46 Hz bins for the hum and koi partials
        hums = [peak_over_floor_db(fh, ph, h) for h in (50.0, 100.0, 41.2, 55.0, 123.6)]
        orate, oper, ocor = onsets(mid, fs)
        row = {"beat": b + 1, "name": NAMES[b], "s0": a, "s1": e, "rmsDb": rms, "win3sP10": float(np.percentile(win, 10)), "win3sP50": float(np.percentile(win, 50)),
               "win3sP90": float(np.percentile(win, 90)), "max1sChangeDb": float(np.abs(np.diff(l1)).max()) if len(l1) > 1 else 0.0,
               "centroidHz": cen, "corrLR": corr, "sideMidDb": sm, "octaveDbReRms": dict(zip([str(c) for c in oct_c], ob)),
               "humPeakDb": {"50": hums[0], "100": hums[1]}, "koiPeakDb": {"41.2": hums[2], "55": hums[3], "123.6": hums[4]},
               "onsetsPerS": orate, "fluxPeriodS": oper, "fluxPeriodCorr": ocor}
        out["beats"].append(row)
        print("%d %-9s %5.1f %5.1f %5.1f %5.1f %5.2f | %7.0f %5.2f %5.1f | %s | %5.1f %5.1f %5.1f %5.1f %5.1f | %5.2f %5.3f %5.2f" % (
            b + 1, NAMES[b], rms, row["win3sP10"], row["win3sP50"], row["win3sP90"], row["max1sChangeDb"], cen, corr, sm,
            " ".join("%3.0f" % v for v in ob), hums[0], hums[1], hums[2], hums[3], hums[4], orate, oper, ocor))
    # chords
    srcs = [("mix", x)]
    if pad:
        px, pfs = read_wav(pad)
        srcs.append(("pad", px))
    print("chord       steady s   | chroma share on the chord's pitch classes (mix%s) | top pitch classes" % (", pad" if pad else ""))
    for c in range(len(CHORD_S)):
        a = CHORD_S[c] + CHORD_H[c]
        e = (CHORD_S[c + 1] - CHORD_H[c + 1]) if c + 1 < len(CHORD_S) else 1497.0
        if c == 0:
            a = 14.0
        if e - a < 8:
            a = CHORD_S[c]
        pcs = sorted(set(n % 12 for n in CHORDS[c]))
        shares, tops = [], ""
        for name, src in srcs:
            i0, i1 = frames_where(sf, fs, src.shape[0], a, e)
            if i1 - i0 < 8192 * 4:
                shares.append(float("nan"))
                continue
            seg = to_float(src[i0:i1])
            f, p = welch(0.5 * (seg[:, 0] + seg[:, 1]), fs)
            ch = chroma(f, p)
            shares.append(float(ch[pcs].sum()))
            if name == ("pad" if pad else "mix"):
                order = np.argsort(ch)[::-1][:len(pcs)]
                tops = " ".join(PC[k] for k in order)
        out["chords"].append({"c": c, "name": CHORD_NAMES[c], "s0": a, "s1": e, "pcs": [PC[k] for k in pcs], "share": shares, "top": tops})
        print("C%-2d %-9s %5.0f-%5.0f | %s (ideal %.2f for white) | %s  (chord %s)" % (
            c, CHORD_NAMES[c], a, e, " ".join("%.2f" % v for v in shares), len(pcs) / 12.0, tops, " ".join(PC[k] for k in pcs)))
    if png:
        spectrogram_png(x, fs, sf, png)
        out["png"] = png
    return out


def spectrogram_png(x, fs, sf, png, cols=2400, n=8192):
    import matplotlib
    matplotlib.use("Agg")
    import matplotlib.pyplot as plt
    total = x.shape[0]
    hop = max(n, total // cols)
    w = np.hanning(n)
    f = np.fft.rfftfreq(n, 1.0 / fs)
    fl = np.geomspace(25, 18000, 360)
    S = []
    T = []
    for i in range(0, total - n, hop):
        seg = to_float(x[i:i + n])
        X = np.fft.rfft(w * 0.5 * (seg[:, 0] + seg[:, 1]))
        pw = (X.real ** 2 + X.imag ** 2) / (w ** 2).sum()
        S.append(10 * np.log10(np.interp(fl, f, pw) + 1e-20))
        T.append(i / fs)
    S = np.array(S).T
    fig, ax = plt.subplots(figsize=(24, 7), dpi=100)
    ax.imshow(S, origin="lower", aspect="auto", cmap="magma", vmin=np.percentile(S, 5), vmax=np.percentile(S, 99.7),
              extent=[T[0], T[-1], 0, len(fl)])
    yt = [31.5, 63, 125, 250, 500, 1000, 2000, 4000, 8000, 16000]
    ax.set_yticks([np.searchsorted(fl, v) for v in yt])
    ax.set_yticklabels([("%g" % v) for v in yt])
    for bb in BOUNDS[1:-1]:
        tb = np.interp(bb, sf(np.array(T)), np.array(T))
        ax.axvline(tb, color="cyan", lw=0.8)
    for cs in CHORD_S[1:]:
        tc = np.interp(cs, sf(np.array(T)), np.array(T))
        ax.axvline(tc, color="white", lw=0.5, ls=":")
    ax.set_xlabel("seconds (cyan: beat boundaries, dotted: chord changes)")
    ax.set_ylabel("Hz")
    ax.set_title(os.path.basename(png))
    fig.tight_layout()
    fig.savefig(png)
    plt.close(fig)


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("wav")
    ap.add_argument("--pad")
    ap.add_argument("--json")
    ap.add_argument("--png")
    a = ap.parse_args()
    out = analyse(a.wav, a.pad, a.png)
    if a.json:
        os.makedirs(os.path.dirname(os.path.abspath(a.json)), exist_ok=True)
        json.dump(out, open(a.json, "w"), indent=1)


if __name__ == "__main__":
    main()
