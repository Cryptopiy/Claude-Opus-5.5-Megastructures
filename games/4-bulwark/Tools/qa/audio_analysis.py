#!/usr/bin/env python3
"""audio_analysis: offline checks of BulwarkScore renders and -audio-dump files (MASTER_PROMPT 11.9, 14.3, 14.10.3).
Imported from Thread; the Thread inhale/swell exclusions, the -9 dBFS momentary peak, the -30 LUFS target and the
wind-off check are gone.

Usage:
  python -I Tools/qa/audio_analysis.py <file.wav | folder> [more ...] [--run <id>] [--out-dir QA/reports]
         [--window 1.0] [--chunk-seconds 10]

Input: WAV files (16/24/32-bit PCM or 32-bit IEEE float, any channel count and rate; WAVE_FORMAT_EXTENSIBLE ok),
each with an optional JSON sidecar <name>.json / <name>.wav.json / <name>_s.json:
  {"s": [plan distance at t = t0 + i*dt], "dt": 1.0, "t0": 0.0}   s per second (OfflineAudioRenderer: "sPerSecond")
  {"frames": [[t, s], ...]}                                       AudioDirector -audio-dump sidecar
  {"beat": N}                                                     the whole file is beat N (also "bNN"/"beatNN" in the name)
  {"speed": 2.5}                                                  walk speed (pairs of renders are compared per s)
  {"stem": "mix" | ...}                                           default "mix"
  {"footsteps": [onset seconds]}                                  excluded from the sample-step check (+-10/300 ms)
  {"events": [{"what": "kraken"|"ride", "kind": k, "t": s}, ...]}  AudioDirector -audio-dump: kraken calls and ride cues
  {"eventStart": s, "stem": "example"}                            BwAudioExamples: one kraken event at eventStart
Per file (streamed in chunks, any length): true peak (4x oversampled, polyphase windowed-sinc FIR) and sample peak,
DC per channel, largest sample-to-sample step, NaN / Inf count, RMS per --window seconds, the sliding 3 s RMS
(100 ms hop), BS.1770-4 K-weighted loudness (reported only).  Checks (14.3 Master, 14.10.3): true peak <= -1 dBTP,
|DC| < 0.001, steps <= 0.25 FS, no NaN, a mix is not silent.  Per beat (all mix files, 100 ms blocks assigned by s,
boundaries and targets from BulwarkConst): the beat RMS and every sliding 3 s RMS window whose centre lies more than
15 m from a beat boundary within +-1.5 dB of the target (B1 -34, B2 -24, B3 -18, B4 -23, B5 -32, B6 -38, B7 -25 to
s 1423 falling linearly to -35 at 1470).  Two mix renders with different "speed" are compared per metre of s:
sliding 3 s RMS within 1.5 dB (T-AUDIO).  Kraken events (Wave X, LEAD DECISION 3): for every kraken event of a dump
sidecar ("events") or an example ("eventStart") three bands are compared with the 3 s BEFORE the event (t-3..t; in a
dump never earlier than introStart + 3 s, the load silence; zero-phase FFT filters on the event's segment):
  80-800 Hz (4th-order band)          >= +8 dB
  HP 250 Hz (4th order, a laptop)     calls / roars / stinger >= +6 dB, slaps / shudders / the strike >= +8 dB
  HP 400 Hz (4th order, a phone)      the same
(fix wave S2-1 "small-speaker gate"; levels are the power average of the channels): the loudest 1 s window (calls, roars, stinger) or 50 ms window (slap, shudder,
strike) inside the event (t .. t + min(duration, 8 s)) against the RMS of the 3 s before.  The beat-6 kinds heard through
water and glass (glass creak, suckers, eye swell, the under-water call: a 450 Hz 24 dB/oct "behind glass" low-pass by
design) are gated in 80-800 Hz only; their HP rows are INFO.  An event whose 3 s pre-window overlaps an earlier kraken
event still sounding (its "duration" in the sidecar, else the recipe default) is a continuation (the roar after its call):
its rows are INFO, the earlier event carries the gate.
Writes QA/reports/audio_<run>.json and .md; exit code 1 on any FAIL.
"""
import argparse
import math
import os
import re
import struct
import sys

_HERE = os.path.dirname(os.path.abspath(__file__))
if _HERE not in sys.path:
    sys.path.insert(0, _HERE)

import numpy as np  # noqa: E402

import qa_common as qc  # noqa: E402
from qa_common import PASS, FAIL, SKIP, INFO, WARN, TH, check  # noqa: E402


# --------------------------------------------------------------------------------------------- WAV reading
class Wav(object):
    def __init__(self, path):
        self.path = path
        with open(path, "rb") as f:
            hdr = f.read(12)
            if len(hdr) < 12 or hdr[0:4] not in (b"RIFF", b"RF64") or hdr[8:12] != b"WAVE":
                raise ValueError("not a RIFF/WAVE file")
            fmt, data_off, data_size = None, None, None
            fsize = os.path.getsize(path)
            pos = 12
            while pos + 8 <= fsize:
                f.seek(pos)
                cid = f.read(4)
                size = struct.unpack("<I", f.read(4))[0]
                if cid == b"fmt ":
                    body = f.read(min(size, 64))
                    tag, ch, fs, _, block, bits = struct.unpack_from("<HHIIHH", body, 0)
                    if tag == 0xFFFE and size >= 26:
                        tag = struct.unpack_from("<H", body, 24)[0]
                    fmt = (tag, ch, fs, bits, block)
                elif cid == b"data":
                    data_off = pos + 8
                    data_size = size
                    if size in (0, 0xFFFFFFFF) or data_off + size > fsize:  # unfinalised / streamed writer
                        data_size = fsize - data_off
                    break
                pos += 8 + size + (size & 1)
        if fmt is None or data_off is None:
            raise ValueError("missing fmt or data chunk")
        self.tag, self.ch, self.fs, self.bits, self.block = fmt
        if (self.tag, self.bits) not in ((1, 16), (1, 24), (1, 32), (3, 32)):
            raise ValueError("unsupported sample format tag %d / %d bit" % (self.tag, self.bits))
        self.bps = self.bits // 8
        self.block = self.block or self.bps * self.ch
        self.data_off = data_off
        self.frames = data_size // self.block
        self.format = {1: "PCM", 3: "float"}[self.tag] + "%d" % self.bits

    def read(self, start, count):
        """Frames [start, start+count) as float64 (count, ch) in full-scale units."""
        count = max(0, min(count, self.frames - start))
        with open(self.path, "rb") as f:
            f.seek(self.data_off + start * self.block)
            raw = f.read(count * self.block)
        if self.tag == 3:
            x = np.frombuffer(raw, dtype="<f4").astype(np.float64)
        elif self.bits == 16:
            x = np.frombuffer(raw, dtype="<i2").astype(np.float64) / 32768.0
        elif self.bits == 32:
            x = np.frombuffer(raw, dtype="<i4").astype(np.float64) / 2147483648.0
        else:
            b = np.frombuffer(raw, dtype=np.uint8).reshape(-1, 3).astype(np.int32)
            v = b[:, 0] | (b[:, 1] << 8) | (b[:, 2] << 16)
            v = (v ^ 0x800000) - 0x800000
            x = v.astype(np.float64) / 8388608.0
        return x.reshape(-1, self.ch)


# --------------------------------------------------------------------------------------------- K-weighting
def k_weighting(fs):
    """BS.1770 pre-filter (shelf) and RLB high-pass for any rate (libebur128 formulas; R5 snippet D)."""
    f0, G, Q = 1681.974450955533, 3.999843853973347, 0.7071752369554196
    K = math.tan(math.pi * f0 / fs)
    Vh = 10 ** (G / 20)
    Vb = Vh ** 0.4996667741545416
    a0 = 1 + K / Q + K * K
    shelf = ([(Vh + Vb * K / Q + K * K) / a0, 2 * (K * K - Vh) / a0, (Vh - Vb * K / Q + K * K) / a0],
             [1.0, 2 * (K * K - 1) / a0, (1 - K / Q + K * K) / a0])
    f0, Q = 38.13547087602444, 0.5003270373238773
    K = math.tan(math.pi * f0 / fs)
    a0 = 1 + K / Q + K * K
    hp = ([1.0, -2.0, 1.0], [1.0, 2 * (K * K - 1) / a0, (1 - K / Q + K * K) / a0])
    return shelf, hp


def biquad_impulse(b, a, n):
    h = np.zeros(n)
    x1 = x2 = y1 = y2 = 0.0
    for i in range(n):
        x = 1.0 if i == 0 else 0.0
        y = b[0] * x + b[1] * x1 + b[2] * x2 - a[1] * y1 - a[2] * y2
        x2, x1, y2, y1 = x1, x, y1, y
        h[i] = y
    return h


def k_impulse(fs, n=1 << 15):
    (b1, a1), (b2, a2) = k_weighting(fs)
    return np.convolve(biquad_impulse(b1, a1, n), biquad_impulse(b2, a2, n))[:n]


def lk(z):
    return -0.691 + 10.0 * np.log10(np.maximum(z, 1e-30))


def db(v):
    return 20.0 * math.log10(max(float(v), 1e-12))


def db_pow(p):
    return 10.0 * math.log10(max(float(p), 1e-24))


def integrated_lufs(sub):
    """sub: per-100 ms channel-summed K-weighted mean squares.  Returns (integrated, momentary max, short max)."""
    sub = np.asarray(sub, dtype=np.float64)
    if sub.size < 4:
        return float("-inf"), float("-inf"), float("-inf")
    blocks = np.convolve(sub, np.ones(4) / 4.0, mode="valid")
    L = lk(blocks)
    g = blocks[L > -70.0]
    mm = float(L.max())
    st = float(lk(np.convolve(sub, np.ones(30) / 30.0, mode="valid")).max()) if sub.size >= 30 else float("nan")
    if g.size == 0:
        return float("-inf"), mm, st
    rel = lk(g.mean()) - 10.0
    g2 = blocks[(L > -70.0) & (L > rel)]
    return (float(lk(g2.mean())) if g2.size else float("-inf")), mm, st


# --------------------------------------------------------------------------------------------- true peak (4x)
def _tp_kernel(factor=4, taps_per_phase=24):
    """Windowed-sinc interpolation FIR for 'factor' x oversampling (gain 'factor' so the zero-stuffed signal keeps
    its level); cut-off at the original Nyquist."""
    n = factor * taps_per_phase + 1
    k = np.arange(n) - (n - 1) / 2.0
    h = np.sinc(k / float(factor)) * np.kaiser(n, 8.0)
    return h * (factor / h.sum())


class TruePeak(object):
    """Streaming 4x oversampled true-peak meter (ITU-R BS.1770-4 annex 2 style)."""

    def __init__(self, ch, factor=4):
        self.f = factor
        self.h = _tp_kernel(factor)
        self.half = (len(self.h) - 1) // 2 // factor + 2      # input samples of context per side
        self.prev = np.zeros((2 * self.half, ch))
        self.peak = 0.0
        self.peak_at = 0
        self.pos = -self.half                                  # input index of prev[half]

    def _run(self, xx, first_index):
        f, half = self.f, self.half
        n = xx.shape[0]
        if n <= 2 * half:
            return
        best = 0.0
        best_i = 0
        for c in range(xx.shape[1]):
            up = np.zeros(n * f)
            up[::f] = xx[:, c]
            y = np.convolve(up, self.h, mode="same")
            seg = np.abs(y[half * f:(n - half) * f])
            if seg.size:
                j = int(np.argmax(seg))
                if seg[j] > best:
                    best, best_i = float(seg[j]), j // f
        if best > self.peak:
            self.peak, self.peak_at = best, first_index + best_i

    def push(self, x):
        xx = np.concatenate([self.prev, x])
        # evaluates input samples [pos, pos + len(x)) with half samples of context on both sides
        self._run(xx, self.pos)
        self.pos += x.shape[0]
        self.prev = xx[-2 * self.half:].copy()

    def flush(self):
        self.push(np.zeros((self.half, self.prev.shape[1])))


# --------------------------------------------------------------------------------------------- sidecar
def find_sidecar(path):
    base = os.path.splitext(path)[0]
    for c in (base + ".json", path + ".json", base + "_s.json"):
        if os.path.isfile(c):
            return c
    return None


def s_of_time(side, t):
    """Plan distance at times t from a sidecar, or None."""
    if not side:
        return None
    fr = side.get("frames")
    if isinstance(fr, list) and fr and isinstance(fr[0], (list, tuple)) and len(fr[0]) >= 2:
        a = np.asarray(fr, dtype=np.float64)
        return np.interp(t, a[:, 0], a[:, 1])
    s = side.get("s", side.get("sPerSecond", side.get("s_per_second")))
    if isinstance(s, list) and len(s) >= 1:
        dt, t0 = float(side.get("dt", 1.0)), float(side.get("t0", 0.0))
        ts = t0 + dt * np.arange(len(s))
        return np.interp(t, ts, np.asarray(s, dtype=np.float64))
    segs = side.get("segments")
    if isinstance(segs, list) and segs:
        out = np.full(np.shape(t), np.nan)
        for g in segs:
            m = (t >= g["t0"]) & (t <= g["t1"])
            out[m] = g["s0"] + (t[m] - g["t0"]) * (g["s1"] - g["s0"]) / max(g["t1"] - g["t0"], 1e-9)
        return out
    return None


# --------------------------------------------------------------------------------------------- analysis
def analyse_wav(path, chunk_seconds=10.0, window_s=1.0):
    w = Wav(path)
    sp = qc.spec()
    side_path = find_sidecar(path)
    side = qc.read_json(side_path, {}) if side_path else {}
    side = side if isinstance(side, dict) else {}
    stem = (side.get("stem") or "mix").lower()
    fs, ch = w.fs, w.ch
    hop = fs // 10
    warn = []
    if fs % 10:
        warn.append("sample rate %d not divisible by 10: 100 ms blocks are %d samples" % (fs, hop))
    chunk = hop * max(1, int(round(chunk_seconds * 10)))
    h = k_impulse(fs)
    m = len(h)
    nfft = 1 << int(math.ceil(math.log2(chunk + m - 1)))
    Hf = np.fft.rfft(h, nfft)
    carry = np.zeros((m - 1, ch))
    foot = np.asarray(side.get("footsteps", []) or [], dtype=np.float64)
    peak, peak_at = 0.0, 0
    tp = TruePeak(ch, int(TH["audio_oversample"]))
    dc = np.zeros(ch)
    max_step, step_at, n_over = 0.0, 0, 0
    nan_count = 0
    last = None
    ms_blocks, pk_blocks, k_blocks = [], [], []
    tail = np.zeros((0, ch))   # samples carried to complete a 100 ms block
    ktail = np.zeros((0, ch))
    for start in range(0, w.frames, chunk):
        x = w.read(start, chunk)
        bad = ~np.isfinite(x)
        if bad.any():
            nan_count += int(bad.sum())
            x = np.where(bad, 0.0, x)
        a = np.abs(x)
        i = int(np.argmax(a.max(axis=1))) if x.shape[0] else 0
        if x.shape[0] and a[i].max() > peak:
            peak, peak_at = float(a[i].max()), start + i
        dc += x.sum(axis=0)
        tp.push(x)
        xx = np.concatenate([last, x]) if last is not None else x
        if xx.shape[0] > 1:
            st = np.abs(np.diff(xx, axis=0)).max(axis=1)
            base = start - (1 if last is not None else 0)
            if foot.size:
                tt = (base + 1 + np.arange(st.size)) / float(fs)
                near = np.zeros(st.size, dtype=bool)
                for f0 in foot[(foot >= tt[0] - 0.30) & (foot <= tt[-1] + 0.01)]:
                    near |= (tt >= f0 - 0.01) & (tt <= f0 + 0.30)
                st = np.where(near, 0.0, st)
            j = int(np.argmax(st))
            if st[j] > max_step:
                max_step, step_at = float(st[j]), base + 1 + j
            n_over += int((st > TH["audio_step_max"]).sum())
        last = x[-1:].copy()
        # K-weighting by FFT overlap-add (exact FIR of the truncated impulse response)
        y = np.fft.irfft(np.fft.rfft(x, nfft, axis=0) * Hf[:, None], nfft, axis=0)[:x.shape[0] + m - 1]
        y[:m - 1] += carry
        carry = y[x.shape[0]:x.shape[0] + m - 1].copy()
        if carry.shape[0] < m - 1:
            carry = np.concatenate([carry, np.zeros((m - 1 - carry.shape[0], ch))])
        yk = y[:x.shape[0]]
        xb = np.concatenate([tail, x])
        kb = np.concatenate([ktail, yk])
        nb = xb.shape[0] // hop
        if nb:
            blk = xb[:nb * hop].reshape(nb, hop, ch)
            ms_blocks.append((blk ** 2).mean(axis=(1, 2)))
            pk_blocks.append(np.abs(blk).max(axis=(1, 2)))
            kblk = kb[:nb * hop].reshape(nb, hop, ch)
            k_blocks.append((kblk ** 2).mean(axis=1).sum(axis=1))
        tail, ktail = xb[nb * hop:], kb[nb * hop:]
    tp.flush()
    ms = np.concatenate(ms_blocks) if ms_blocks else np.zeros(0)
    pk = np.concatenate(pk_blocks) if pk_blocks else np.zeros(0)
    kb = np.concatenate(k_blocks) if k_blocks else np.zeros(0)
    nblk = ms.size
    t_c = (np.arange(nblk) + 0.5) * hop / float(fs)
    s_c = s_of_time(side, t_c) if nblk else None
    file_beat = side.get("beat")
    if file_beat is None:
        mb = re.search(r"(?:^|[_\-])(?:b|beat)0*([1-7])(?=[_\-.]|$)", os.path.basename(path), re.I)
        file_beat = int(mb.group(1)) if mb else None
    beats = np.zeros(nblk, dtype=np.int64)
    excluded = np.zeros(nblk, dtype=bool)
    post = np.zeros(nblk, dtype=bool)   # after the goal trigger (the 14.11 ending): no sliding-window targets
    b = sp.beat_boundaries
    if s_c is not None:
        for k in range(len(b) - 1):
            beats[(s_c >= b[k]) & (s_c < b[k + 1])] = k + 1
        beats[s_c >= b[-1]] = len(b) - 1
        # the beat-7 RMS target (-25) holds to the tunnel mouth; the ledge ramp is checked by the sliding windows
        excluded |= (beats == len(b) - 1) & (s_c >= sp.ledge_strict_s)
        # the 14.11 ending (OfflineAudioRenderer "triggerTime"): after the goal trigger the master fades to black by design
        trig = side.get("triggerTime")
        if isinstance(trig, (int, float)) and trig > 0:
            post = t_c >= float(trig)
            excluded |= post
    elif file_beat:
        beats[:] = int(file_beat)
    # sliding 3 s RMS (100 ms hop), centred on each block
    w3 = max(1, int(round(TH["audio_rms_window_s"] * 10)))
    ms3 = None
    if nblk >= w3:
        ms3 = np.convolve(ms, np.ones(w3) / float(w3), mode="same")
        h3 = w3 // 2
        ms3[:h3] = np.nan                       # windows that would reach past the file ends are not measured
        if nblk - (w3 - 1 - h3) < nblk:
            ms3[nblk - (w3 - 1 - h3):] = np.nan
    wn = max(1, int(round(window_s * 10)))
    nw = nblk // wn
    rms_win = [round(db_pow(v), 2) for v in ms[:nw * wn].reshape(nw, wn).mean(axis=1)] if nw else []
    m400 = np.convolve(ms, np.ones(4) / 4.0, mode="valid") if nblk >= 4 else ms
    integ, mmax, stmax = integrated_lufs(kb)
    res = {"file": path, "sidecar": side_path, "stem": stem, "format": w.format, "fs": fs, "channels": ch,
           "seconds": w.frames / float(fs), "frames": w.frames, "nan_or_inf": nan_count,
           "sample_peak": peak, "sample_peak_dbfs": db(peak), "sample_peak_at_s": peak_at / float(fs),
           "true_peak": tp.peak, "true_peak_dbtp": db(tp.peak), "true_peak_at_s": max(tp.peak_at, 0) / float(fs),
           "speed": side.get("speed"),
           "dc_per_channel": [float(v) for v in dc / max(w.frames, 1)],
           "max_step": max_step, "max_step_at_s": step_at / float(fs), "steps_over_limit": n_over,
           "rms_dbfs": db_pow(ms.mean()) if nblk else None,
           "max_400ms_rms_dbfs": db_pow(m400.max()) if m400.size else None,
           "rms_per_window_dbfs": rms_win, "window_s": wn / 10.0,
           "lufs_integrated": integ, "lufs_momentary_max": mmax, "lufs_short_term_max": stmax,
           "warnings": warn, "_ms": ms, "_ms3": ms3, "_pk": pk, "_kb": kb, "_beats": beats, "_excl": excluded, "_post": post, "_s": s_c,
           "has_s": s_c is not None}
    return res


KRAKEN_NAMES = ["Call", "SurfaceRoar", "Slap", "DamShudder", "GlassCreak", "SuckerPops", "EyeSwell", "Stinger", "Strike"]
KRAKEN_LUT = ["call", "surface_roar", "slap", "dam_shudder", "glass_creak", "sucker_pops", "eye_swell", "stinger", "strike"]
# small-speaker gate (fix wave S2-1): impacts are measured in 50 ms windows at +8 dB, voices in 1 s windows at +6 dB
KRAKEN_IMPACT = {2, 3, 8}
KRAKEN_GLASS = {4, 5, 6}   # heard through water and glass: HP rows INFO
KRAKEN_DUR = [8.0, 10.0, 4.0, 5.0, 8.0, 10.0, 12.0, 30.0, 6.0]


def _fft_band(x, fs, hp=None, lp=None, order=4):
    """Zero-phase Butterworth-magnitude band filter of a mono segment (FFT; energy measures do not need the phase)."""
    n = len(x)
    nf = 1 << int(math.ceil(math.log2(max(2, n))))
    X = np.fft.rfft(x, nf)
    f = np.fft.rfftfreq(nf, 1.0 / fs)
    g = np.ones_like(f)
    with np.errstate(divide="ignore"):
        if hp:
            g *= 1.0 / np.sqrt(1.0 + (hp / np.maximum(f, 1e-6)) ** (2 * order))
        if lp:
            g *= 1.0 / np.sqrt(1.0 + (f / lp) ** (2 * order))
    return np.fft.irfft(X * g, nf)[:n]


def _max_window_db(y, fs, win):
    """Loudest RMS (dBFS) of a sliding window of win seconds (hop win/4)."""
    w = max(1, int(win * fs))
    if len(y) < w:
        return db_pow(np.mean(y ** 2)) if len(y) else -240.0
    c = np.concatenate(([0.0], np.cumsum(y.astype(np.float64) ** 2)))
    hop = max(1, w // 4)
    e = (c[w::hop] - c[:-w:hop]) / w
    return db_pow(float(np.max(e)))


def _band_80_800(x, fs):
    """Mono 80-800 Hz band (2nd-order Butterworth high-pass at 80 Hz and low-pass at 800 Hz, cascaded twice)."""
    # decimate by 8 first (6 kHz rate is enough for an 800 Hz band; a 16-tap box pre-filter avoids aliasing)
    m = x.mean(axis=1)
    k = 8
    m = np.convolve(m, np.ones(16) / 16.0, mode="same")[::k]
    f2 = fs / float(k)
    y = m
    for fc, hi in ((80.0, True), (80.0, True), (800.0, False), (800.0, False)):
        w0 = 2 * math.pi * fc / f2
        cw, sw = math.cos(w0), math.sin(w0)
        al = sw / (2 * 0.7071)
        b = [(1 + cw) / 2, -(1 + cw), (1 + cw) / 2] if hi else [(1 - cw) / 2, 1 - cw, (1 - cw) / 2]
        a0 = 1 + al
        y = _lfilter([v / a0 for v in b], [1.0, -2 * cw / a0, (1 - al) / a0], y)
    return y, f2


def _lfilter(b, a, x):
    """Direct-form biquad with numpy recursion in blocks (scipy-free)."""
    y = np.empty_like(x)
    x1 = x2 = y1 = y2 = 0.0
    b0, b1, b2 = b
    a1, a2 = a[1], a[2]
    for i in range(len(x)):
        v = b0 * x[i] + b1 * x1 + b2 * x2 - a1 * y1 - a2 * y2
        x2 = x1; x1 = x[i]; y2 = y1; y1 = v
        y[i] = v
    return y


def event_checks(path, side):
    """Kraken audibility per event (see the module doc: 80-800 Hz and the small-speaker gate); returns check rows."""
    C = []
    evs = []
    if isinstance(side.get("events"), list):
        evs = [e for e in side["events"] if isinstance(e, dict) and e.get("what") == "kraken"]
    elif side.get("eventStart") is not None and "kraken" in os.path.basename(path):
        name = os.path.splitext(os.path.basename(path))[0].replace("kraken_", "")
        uw = name.endswith("_underwater")
        name = name.replace("_underwater", "")
        evs = [{"what": "kraken", "kind": KRAKEN_LUT.index(name) if name in KRAKEN_LUT else 0, "t": float(side["eventStart"]),
                "underwater": uw}]
    if not evs:
        return C
    w = Wav(path)
    fs = w.fs
    intro = side.get("introStart")
    floor_t = (float(intro) + 3.0) if isinstance(intro, (int, float)) and intro >= 0 else 0.0
    evs = sorted(evs, key=lambda q: float(q["t"]))
    for ie, e in enumerate(evs):
        t, kind = float(e["t"]), int(e.get("kind", 0)) % len(KRAKEN_NAMES)
        prev = None
        for q in evs[:ie]:
            kq = int(q.get("kind", 0)) % len(KRAKEN_NAMES)
            if float(q["t"]) + float(q.get("duration", KRAKEN_DUR[kq])) > t - 3.0:
                prev = "%s at %.1f s" % (KRAKEN_NAMES[kq], float(q["t"]))
        nm = KRAKEN_NAMES[kind]
        imp = kind in KRAKEN_IMPACT
        glass = kind in KRAKEN_GLASS or bool(e.get("underwater"))
        dur = float(e.get("duration", KRAKEN_DUR[kind]))
        pa, pb = max(t - 3.0, floor_t), t
        qa, qb = t, t + min(dur, 8.0)
        label = "kraken %s at %.1f s" % (nm, t)
        if pb - pa < 1.0 or qb * fs > w.frames:
            C.append(check(label + " audible", SKIP, None, None, "pre-window under 1 s (load silence) or the event outside the file"))
            continue
        s0, s1 = int(pa * fs), int(qb * fs)
        xs = w.read(s0, s1 - s0).astype(np.float64)
        ia, ib = 0, int((pb - pa) * fs)
        ja, jb = int((qa - pa) * fs), xs.shape[0]
        win = 0.05 if imp else 1.0
        gate = 8.0 if imp else 6.0
        rows = (("80-800 Hz", 80.0, 800.0, 8.0, False), ("HP 250 (laptop)", 250.0, None, gate, glass),
                ("HP 400 (phone)", 400.0, None, gate, glass))
        for bname, hp, lp, need, info in rows:
            # the power average of the channels (an RMS meter; a panned event is not penalised by a mono downmix)
            y2 = np.zeros(xs.shape[0])
            for c in range(xs.shape[1]):
                y2 += _fft_band(xs[:, c], fs, hp, lp) ** 2
            y = np.sqrt(y2 / xs.shape[1])
            before = db_pow(np.mean(y[ia:ib] ** 2))
            during = _max_window_db(y[ja:jb], fs, win)
            d = during - before
            st = INFO if (info or prev) else (PASS if d >= need else FAIL)
            C.append(check("%s %s" % (label, bname), st, round(d, 2),
                           ">= +%g dB (loudest %g s vs the 3 s before)" % (need, win),
                           "before %.1f, loudest %.1f dBFS" % (before, during)
                           + ("; continuation of %s" % prev if prev else "")))
    return C


def file_checks(r):
    C = []
    sp = qc.spec()
    C.append(check("NaN/Inf samples", PASS if r["nan_or_inf"] == 0 else FAIL, r["nan_or_inf"], "0"))
    lim = float(sp.true_peak_db)
    C.append(check("true peak", PASS if r["true_peak_dbtp"] <= lim else FAIL, round(r["true_peak_dbtp"], 2),
                   "<= %g dBTP" % lim, "4x oversampled, at %.3f s" % r["true_peak_at_s"]))
    C.append(check("sample peak", INFO, round(r["sample_peak_dbfs"], 2), None, "at %.3f s" % r["sample_peak_at_s"]))
    dcm = max(abs(v) for v in r["dc_per_channel"]) if r["dc_per_channel"] else 0.0
    C.append(check("DC offset", PASS if dcm < sp.dc_max else FAIL, round(dcm, 6), "< %g" % sp.dc_max))
    C.append(check("sample step", PASS if r["max_step"] <= sp.step_max else FAIL, round(r["max_step"], 4),
                   "<= %g FS" % sp.step_max, "at %.4f s, %d over (footsteps excluded)" % (r["max_step_at_s"], r["steps_over_limit"])))
    if r["stem"] == "mix":
        rd = r["rms_dbfs"]
        C.append(check("not silent", PASS if rd is not None and rd > -80.0 else FAIL, qc.fmt(rd, 2), "> -80 dBFS RMS"))
    C.append(check("integrated loudness", INFO, qc.fmt(r["lufs_integrated"], 2), None, "BS.1770-4, reported only"))
    return C


def target_db(s, sp):
    """14.3 target of the sliding 3 s RMS at plan distance s (beat 7: -25 to s 1423, linear to -35 at 1470)."""
    b = sp.beat_boundaries
    k = sp.beat_of_s(s)
    tgt = sp.target_rms_db[k - 1] if k - 1 < len(sp.target_rms_db) else None
    if tgt is not None and k == len(b) - 1:
        s0, (s1, t1) = sp.ledge_strict_s, sp.ledge_rms
        if s >= s1:
            tgt = t1
        elif s > s0:
            tgt = tgt + (t1 - tgt) * (s - s0) / (s1 - s0)
    return tgt


def sliding_checks(results):
    """Every sliding 3 s RMS window (centre more than 15 m from a beat boundary) within +-tol of target_db(s)."""
    sp = qc.spec()
    tol = float(sp.rms_tol_db)
    C = []
    b = np.asarray(sp.beat_boundaries[1:-1], dtype=np.float64)
    worst = {}
    for r in results:
        if r["stem"] != "mix" or r["_s"] is None or r["_ms3"] is None:
            continue
        s = r["_s"]
        far = np.ones(s.shape, dtype=bool)
        for x in b:
            far &= np.abs(s - x) > 15.0
        for i in np.flatnonzero(far & np.isfinite(s) & np.isfinite(r["_ms3"]) & ~r["_post"]):
            tgt = target_db(float(s[i]), sp)
            if tgt is None:
                continue
            dev = db_pow(r["_ms3"][i]) - tgt
            k = sp.beat_of_s(float(s[i]))
            w = worst.get(k)
            if w is None or abs(dev) > abs(w[0]):
                worst[k] = (dev, float(s[i]), os.path.basename(r["file"]))
    for k in sorted(worst):
        dev, s_at, f = worst[k]
        C.append(check("beat %d sliding 3 s RMS" % k, PASS if abs(dev) <= tol else FAIL, round(dev, 2),
                       "+- %g dB of target" % tol, "worst at s %.1f (%s)" % (s_at, f)))
    # T-AUDIO: the same s at both speeds within tol
    mixes = [r for r in results if r["stem"] == "mix" and r["_s"] is not None and r["_ms3"] is not None
             and r.get("speed") is not None]
    for i in range(len(mixes)):
        for j in range(i + 1, len(mixes)):
            a, c = mixes[i], mixes[j]
            if abs(float(a["speed"]) - float(c["speed"])) < 1e-6:
                continue
            lo = max(np.nanmin(a["_s"]), np.nanmin(c["_s"]))
            hi = min(np.nanmax(a["_s"]), np.nanmax(c["_s"]))
            if hi - lo < 10.0:
                continue
            grid = np.arange(np.ceil(lo + 5.0), np.floor(hi - 5.0))
            far = np.ones(grid.shape, dtype=bool)
            for x in b:
                far &= np.abs(grid - x) > 15.0
            grid = grid[far]
            if grid.size == 0:
                continue
            ma = np.isfinite(a["_ms3"]) & np.isfinite(a["_s"]) & ~a["_post"]
            mc = np.isfinite(c["_ms3"]) & np.isfinite(c["_s"]) & ~c["_post"]
            ra = np.interp(grid, a["_s"][ma], 10.0 * np.log10(np.maximum(a["_ms3"][ma], 1e-20)))
            rc = np.interp(grid, c["_s"][mc], 10.0 * np.log10(np.maximum(c["_ms3"][mc], 1e-20)))
            d = np.abs(ra - rc)
            m = int(np.argmax(d))
            C.append(check("speed pair %g/%g per s" % (float(a["speed"]), float(c["speed"])),
                           PASS if d[m] <= tol else FAIL, round(float(d[m]), 2), "<= %g dB" % tol,
                           "worst at s %.0f" % grid[m]))
    return C


def beat_table(results):
    sp = qc.spec()
    rows, C = [], []
    acc = {k: [] for k in range(1, len(sp.beat_boundaries))}
    for r in results:
        if r["stem"] != "mix":
            continue
        for k in acc:
            m = (r["_beats"] == k) & ~r["_excl"]
            if m.any():
                acc[k].append(r["_ms"][m])
    for k in sorted(acc):
        name = sp.beat_names[k - 1] if k - 1 < len(sp.beat_names) else str(k)
        tgt = sp.target_rms_db[k - 1] if k - 1 < len(sp.target_rms_db) else None
        if not acc[k] or tgt is None:
            C.append(check("beat %d RMS" % k, SKIP, None, None, "no blocks (no s sidecar / beat tag)"))
            rows.append([k, name, "-", tgt, "-", "-", SKIP])
            continue
        v = np.concatenate(acc[k])
        rd = db_pow(v.mean())
        dev = rd - tgt
        tol = float(sp.rms_tol_db)
        st = PASS if abs(dev) <= tol else FAIL
        excl = "ledge ramp to %g dB after s %g is checked by the sliding windows" % (sp.ledge_rms[1], sp.ledge_strict_s) if k == 7 else ""
        C.append(check("beat %d RMS" % k, st, round(rd, 2), "%g +- %g dB" % (tgt, tol), excl))
        rows.append([k, name, round(rd, 2), tgt, "%+.2f" % dev, "%.1f s" % (v.size / 10.0), st])
    return rows, C


def build_markdown(run, results, beat_rows, beat_checks, combined_lufs, errors):
    allc = [c for r in results for c in r["checks"]] + beat_checks
    cnt = qc.count_status(allc)
    L = ["# Audio analysis: %s" % run, "",
         "Files: %d. Checks: %d PASS, %d FAIL, %d SKIP, %d INFO. Scene constants: %s." % (
             len(results), cnt[PASS], cnt[FAIL] + len(errors), cnt[SKIP], cnt[INFO], qc.spec().source), "",
         "**Result: %s**" % (FAIL if (cnt[FAIL] or errors) else (PASS if results else SKIP)), ""]
    if errors:
        L += ["## Unreadable files", ""] + ["- %s" % e for e in errors] + [""]
    L += ["## RMS per beat (mix files, 14.3.3 totals +-3 dB)", ""]
    L.append(qc.md_table(["Beat", "Name", "RMS dBFS", "Target", "Delta dB", "Measured", "Result"], beat_rows))
    if combined_lufs is not None:
        L += ["Integrated loudness over all mix files: %s LUFS (target about -30, INFO)." % qc.fmt(combined_lufs, 2), ""]
    L += ["## Files", ""]
    rows = []
    for r in results:
        rows.append([os.path.basename(r["file"]), r["stem"], "%s %d Hz x%d" % (r["format"], r["fs"], r["channels"]),
                     qc.fmt(r["seconds"], 1), qc.fmt(r["sample_peak_dbfs"], 2), qc.fmt(max(abs(v) for v in r["dc_per_channel"]), 6),
                     qc.fmt(r["max_step"], 4), qc.fmt(r["rms_dbfs"], 2), qc.fmt(r["lufs_integrated"], 2), r["status"]])
    L.append(qc.md_table(["File", "Stem", "Format", "Seconds", "Peak dBFS", "|DC|", "Max step", "RMS dBFS", "LUFS",
                          "Result"], rows))
    from sequences import sparkline  # local import: shared helper
    for r in results:
        L += ["### %s (%s)" % (os.path.basename(r["file"]), r["status"]), "",
              "RMS per %g s: `%s`" % (r["window_s"], sparkline(r["rms_per_window_dbfs"])), ""]
        for wmsg in r.get("warnings", []):
            L.append("- WARN: %s" % wmsg)
        L.append(qc.md_table(["Check", "Status", "Value", "Threshold", "Note"],
                             [[c["name"], c["status"], qc.fmt(c["value"], 4), c["threshold"] or "", c["note"] or ""]
                              for c in r["checks"]]))
    L += ["## Method notes", "",
          "- RMS in dBFS = 20 log10(rms of all samples of all channels) (a full-scale sine reads -3.01 dBFS).",
          "- LUFS sums the channel powers after K-weighting, so a centred signal reads about +3 dB above its "
          "per-channel RMS and the 36.7/55 Hz drone loses 3-6 dB (R5 section 3): the two scales differ by design.",
          ""]
    return "\n".join(L)


def collect(paths):
    out = []
    for p in paths:
        if os.path.isdir(p):
            for name in sorted(os.listdir(p)):
                if name.lower().endswith(".wav"):
                    out.append(os.path.join(p, name))
        elif os.path.isfile(p):
            out.append(p)
    return out


def run(paths, run_name=None, out_dir=qc.REPORTS_DIR, window_s=1.0, chunk_seconds=10.0, quiet=False):
    files = collect(paths)
    if run_name is None:
        run_name = qc.run_id_for(paths[0]) if paths else "audio"
    results, errors = [], []
    for f in files:
        if not quiet:
            print("[audio_analysis] %s" % f, flush=True)
        try:
            r = analyse_wav(f, chunk_seconds, window_s)
        except Exception as e:
            errors.append("%s: %s: %s" % (f, type(e).__name__, e))
            continue
        r["checks"] = file_checks(r)
        try:
            side = qc.read_json(r["sidecar"], {}) if r.get("sidecar") else {}
            r["checks"] += event_checks(f, side if isinstance(side, dict) else {})
        except Exception as e:  # the event check never hides the file checks
            r["checks"].append(check("kraken events", WARN, None, None, "%s: %s" % (type(e).__name__, e)))
        r["status"] = qc.worst_status(r["checks"])
        results.append(r)
    beat_rows, beat_checks = beat_table(results)
    beat_checks = beat_checks + sliding_checks(results)
    mix_k = [r["_kb"] for r in results if r["stem"] == "mix" and r["_kb"].size]
    combined = integrated_lufs(np.concatenate(mix_k))[0] if len(mix_k) > 1 else None
    allc = [c for r in results for c in r["checks"]] + beat_checks
    status = FAIL if (errors or any(c["status"] == FAIL for c in allc)) else (PASS if results else SKIP)
    js = os.path.join(out_dir, "audio_%s.json" % run_name)
    md = os.path.join(out_dir, "audio_%s.md" % run_name)
    pub = [{k: v for k, v in r.items() if not k.startswith("_")} for r in results]
    qc.write_json(js, {"run": run_name, "status": status, "files": pub, "errors": errors,
                       "beats": [dict(zip(["beat", "name", "rms_dbfs", "target", "delta", "measured", "status"], row))
                                 for row in beat_rows], "beat_checks": beat_checks, "lufs_combined": combined})
    qc.write_text(md, build_markdown(run_name, results, beat_rows, beat_checks, combined, errors))
    fails = ["%s: %s (%s)" % (os.path.basename(r["file"]), c["name"], qc.fmt(c["value"], 4)) for r in results
             for c in r["checks"] if c["status"] == FAIL]
    fails += ["%s (%s)" % (c["name"], qc.fmt(c["value"], 2)) for c in beat_checks if c["status"] == FAIL]
    fails += errors
    return {"tool": "audio_analysis", "status": status, "json": js, "md": md, "fails": fails,
            "counts": qc.count_status(allc), "n": len(results)}


def main(argv=None):
    ap = argparse.ArgumentParser(prog="audio_analysis.py", description=__doc__.split("\n\n")[0],
                                 formatter_class=argparse.RawDescriptionHelpFormatter, epilog=__doc__)
    ap.add_argument("paths", nargs="+", help="WAV files and/or folders with WAV files")
    ap.add_argument("--run", help="run id (default: folder name)")
    ap.add_argument("--out-dir", default=qc.REPORTS_DIR)
    ap.add_argument("--window", type=float, default=1.0, help="RMS window in seconds (default 1)")
    ap.add_argument("--chunk-seconds", type=float, default=10.0, help="streaming chunk length (default 10 s)")
    ap.add_argument("--quiet", action="store_true")
    a = ap.parse_args(argv)
    files = collect(a.paths)
    if not files:
        print("no WAV file found in %s" % ", ".join(a.paths), file=sys.stderr)
        return 1
    r = run(a.paths, a.run, a.out_dir, a.window, a.chunk_seconds, a.quiet)
    print("[audio_analysis] %s: %d files, %d FAIL -> %s" % (r["status"], r["n"], len(r["fails"]), r["md"]))
    for f in r["fails"][:30]:
        print("  FAIL " + f)
    return 1 if r["status"] == FAIL else 0


if __name__ == "__main__":
    sys.exit(main())
