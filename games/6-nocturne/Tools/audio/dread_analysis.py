#!/usr/bin/env python3
"""AU's analysis of the D-034 dread layer in a whole-walk render (docs/DREAD.md, docs/AUDIO.md section 11).

Inputs: renders of the same walk by Tools/audio/harness (same speed, seed, --ending):
  --mix  the full mix;  --noe  the mix without the dread stem E (--solo RDHTWPKXF);  --e  E alone (--solo E);
  --pad  the pad alone (--solo P);  --cues  the cue list (harness "dread --cues <json> --speed v").
For every cue (one-shots by their onset and length, the swells over +-3 s around their key shot, the silence drops over the
1.4 s of full depth, the hull over its window) it reports:
  - E: the cue's level (RMS dBFS) and its share of the mix (E re the rest of the mix, dB);
  - laptop: the same in a laptop-speaker band (Butterworth-magnitude 4th-order high-pass 200 Hz, low-pass 10 kHz) and a phone
    band (high-pass 350 Hz, low-pass 8 kHz), computed on the window's spectrum (numpy only);
    a cue is "audible" where its band level is within 10 dB of the rest (partial-masking range) or above;
  - encoder: AAC 128 kbit/s and Opus 128 kbit/s (ffmpeg, the YouTube codecs) of the mix and of the no-E mix; the decoded
    difference projected on E (retained = <diff, E> / <E, E>, correlation) in the cue window;
  - pad: octave bands 31.5 Hz - 4 kHz of E and of the pad in the window; "masks" where E is above the pad - 6 dB in a band
    of 250 Hz and up (the pad's voices and partials), the highest such band reported;
  - drops: the level of the rest (everything but E) in the drop re the 3 s before: full band, 2-16 kHz, K-weighted (BS.1770
    pre-filter, LU), the laptop model (HP 200 Hz) and the phone model (HP 400 Hz); the same K-weighted drop of the whole mix;
    gate (fix wave C07): rest_drop_db <= -10 dB, K-weighted and laptop drops >= 8, the swell the loudest element (E > rest);
  - meets: the distance (m) of every horn, strike and groan from the nearest vehicle meet in the cue list (C08 gate >= 15 m);
  - koi (--k K-solo, --nok mix without K): the koi tone re the rest in S7 (s 1 340-1 380) full band, laptop (HP 200) and phone
    (HP 400) bands (C09 gate: laptop >= -6 dB), and K's octave levels against the pad's (--pad).
Writes a JSON with every number (--json) and prints a table and the gates.
Usage:
  python -I Tools/audio/dread_analysis.py --mix m.wav --noe n.wav --e e.wav --pad p.wav --cues c.json [--k k.wav --nok nk.wav]
         [--json out.json] [--no-encode]
"""
import argparse
import json
import math
import os
import struct
import subprocess
import sys
import tempfile

import numpy as np


def read_wav(path):
    with open(path, "rb") as f:
        data = f.read()
    if data[:4] != b"RIFF" or data[8:12] != b"WAVE":
        raise ValueError("not a WAV: " + path)
    pos, fmt, ch, sr, bits, payload = 12, None, 2, 48000, 32, None
    while pos + 8 <= len(data):
        cid, size = data[pos:pos + 4], struct.unpack("<I", data[pos + 4:pos + 8])[0]
        body = data[pos + 8:pos + 8 + size]
        if cid == b"fmt ":
            fmt, ch, sr = struct.unpack("<HHI", body[:8])
            bits = struct.unpack("<H", body[14:16])[0]
            if fmt == 0xFFFE:
                fmt = struct.unpack("<H", body[24:26])[0]
        elif cid == b"data":
            payload = body
        pos += 8 + size + (size & 1)
    if fmt == 3:
        x = np.frombuffer(payload, dtype="<f4").astype(np.float32)
    elif bits == 16:
        x = (np.frombuffer(payload, dtype="<i2").astype(np.float32) / 32768.0).astype(np.float32)
    else:
        raise ValueError("unsupported WAV format")
    return x.reshape(-1, ch)[:, :2], sr


def spectrum(x, fs):
    """Power spectrum (mean over channels) of a window, Hann-weighted, scaled so its sum is the mean square."""
    m = x.shape[0]
    w = np.hanning(m)
    X = np.fft.rfft(x.astype(np.float64) * w[:, None], axis=0)
    p = np.mean(np.abs(X) ** 2, axis=1) / (np.sum(w * w) * m) * 2.0
    p[0] *= 0.5
    f = np.fft.rfftfreq(m, 1.0 / fs)
    return f, p


def band_ms(x, fs, hp=None, lp=None, order=4):
    """Mean square of x through a Butterworth-magnitude band (|H|^2 of an order-n high-pass at hp and low-pass at lp)."""
    f, p = spectrum(x, fs)
    h = np.ones_like(f)
    if hp:
        r = (f / hp) ** (2 * order)
        h *= r / (1.0 + r)
    if lp:
        h *= 1.0 / (1.0 + (f / lp) ** (2 * order))
    return float(np.sum(p * h))


def db(p):
    return 10.0 * math.log10(max(p, 1e-30))


def kweight_ms(x, fs):
    """Mean square through the BS.1770 K-weighting (the two 48 kHz biquads' magnitude applied on the window's spectrum)."""
    f, p = spectrum(x, fs)
    z = np.exp(-2j * np.pi * f / fs)
    b1, a1 = [1.53512485958697, -2.69169618940638, 1.19839281085285], [1.0, -1.69065929318241, 0.73248077421585]
    b2, a2 = [1.0, -2.0, 1.0], [1.0, -1.99004745483398, 0.99007225036621]
    h = 1.0
    for b, a in ((b1, a1), (b2, a2)):
        h = h * (b[0] + b[1] * z + b[2] * z * z) / (a[0] + a[1] * z + a[2] * z * z)
    return float(np.sum(p * np.abs(h) ** 2))


def ms(x):
    return float(np.mean(np.square(x, dtype=np.float64))) if len(x) else 0.0


def encode_roundtrip(x, fs, codec):
    """Encode x (float stereo) with ffmpeg and decode back to float; returns the decoded array (aligned by cross-correlation)."""
    tmp = tempfile.mkdtemp(prefix="dread_enc_")
    src, enc, dec = os.path.join(tmp, "a.wav"), os.path.join(tmp, "a." + ("m4a" if codec == "aac" else "opus")), os.path.join(tmp, "b.wav")
    pcm = np.clip(x, -1.0, 1.0)
    with open(src, "wb") as f:
        n = pcm.shape[0]
        f.write(b"RIFF" + struct.pack("<I", 36 + n * 8) + b"WAVEfmt " + struct.pack("<IHHIIHH", 16, 3, 2, fs, fs * 8, 8, 32))
        f.write(b"data" + struct.pack("<I", n * 8) + pcm.astype("<f4").tobytes())
    args = ["-c:a", "aac", "-b:a", "128k"] if codec == "aac" else ["-c:a", "libopus", "-b:a", "128k"]
    subprocess.run(["ffmpeg", "-v", "error", "-y", "-i", src] + args + [enc], check=True)
    subprocess.run(["ffmpeg", "-v", "error", "-y", "-i", enc, "-ar", str(fs), "-c:a", "pcm_f32le", dec], check=True)
    y, _ = read_wav(dec)
    for p in (src, enc, dec):
        try:
            os.remove(p)
        except OSError:
            pass
    try:
        os.rmdir(tmp)
    except OSError:
        pass
    return y


def align(ref, y, maxlag):
    """Delay of y re ref (samples): the cross-correlation peak over the first 10 s (mid channel, FFT)."""
    L = min(10 * 48000, len(ref) - maxlag - 1)
    a = ref[:L, 0] + ref[:L, 1]
    b = y[:L + maxlag, 0] + y[:L + maxlag, 1]
    nfft = 1 << int(math.ceil(math.log2(len(a) + len(b))))
    c = np.fft.irfft(np.fft.rfft(b, nfft) * np.conj(np.fft.rfft(a, nfft)), nfft)
    return int(np.argmax(c[:maxlag + 1]))


def third_octave_snr(xe, xr, fs, lo, hi, hp=None):
    """Best 1/3-octave band of E re the rest (dB) between lo and hi Hz (optionally through a laptop high-pass): a cue is
    audible where some critical band carries it within about 6 dB of the masker."""
    f, pe = spectrum(xe, fs)
    _, pr = spectrum(xr, fs)
    if hp:
        r = (f / hp) ** 8
        w = r / (1.0 + r)
        pe, pr = pe * w, pr * w
    best, bf = -999.0, None
    fc = lo
    while fc <= hi:
        sel = (f >= fc / 2 ** (1 / 6)) & (f < fc * 2 ** (1 / 6))
        if np.any(sel):
            d = db(float(np.sum(pe[sel]))) - db(float(np.sum(pr[sel])))
            if d > best:
                best, bf = d, fc
        fc *= 2 ** (1 / 3)
    return best, bf


def octave_levels(x, fs):
    if x.shape[0] < 8192:
        return {}
    f, p = spectrum(x, fs)
    out = {}
    for fc in [31.5, 63, 125, 250, 500, 1000, 2000, 4000]:
        sel = (f >= fc / math.sqrt(2)) & (f < fc * math.sqrt(2))
        out[fc] = db(float(np.sum(p[sel])))
    return out


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--mix", required=True)
    ap.add_argument("--noe", required=True)
    ap.add_argument("--e", required=True)
    ap.add_argument("--pad")
    ap.add_argument("--cues", required=True)
    ap.add_argument("--json")
    ap.add_argument("--k")
    ap.add_argument("--nok")
    ap.add_argument("--no-encode", action="store_true")
    a = ap.parse_args()
    mix, fs = read_wav(a.mix)
    noe, _ = read_wav(a.noe)
    e, _ = read_wav(a.e)
    pad = read_wav(a.pad)[0] if a.pad else None
    n = min(len(mix), len(noe), len(e))
    mix, noe, e = mix[:n], noe[:n], e[:n]
    cues = json.load(open(a.cues))
    enc = {}
    if not a.no_encode:
        for codec in ("aac", "opus"):
            ym, yn = encode_roundtrip(mix, fs, codec), encode_roundtrip(noe, fs, codec)
            lm, ln = align(mix, ym, 4096), align(noe, yn, 4096)
            ym, yn = ym[lm: lm + n], yn[ln: ln + n]
            k = min(len(ym), len(yn))
            enc[codec] = (ym[:k] - yn[:k], lm, ln)

    windows = []
    for c in cues["events"]:
        windows.append((c["kind"], c["s"], c["t"], c["t"] + min(c["len"], 6.0)))
    for c in cues["swells"]:
        windows.append(("Swell " + c["name"], c["s"], c["t"] - 3.0, c["t"] + 3.0))
    for c in cues["drops"]:
        windows.append(("Drop", c["s"], c["t"] - 1.4, c["t"]))
    ark = cues.get("ark")
    if not ark:
        windows.append(("Hull", None, cues["hull"]["t0"] + 20.0, cues["hull"]["t0"] + 80.0))
    # D-043 the Ark: its silence (the hold, bow -5..+2.5 s, against bow -18..-10 s), its groans, the crossing under its belly
    if ark:
        windows.append(("ArkDrop", None, ark["bow"] - 5.0, ark["bow"] + 2.5))
        windows.append(("ArkBelly", None, ark["bow"] + 30.0, min(ark["stern"], ark["bow"] + 150.0)))
        for c in cues.get("arkgroans", []):
            windows.append(("ArkGroan", round(c["t"], 1), c["t"], c["t"] + min(c["len"], 6.0)))

    rows = []
    print("cue            s       t0     E dBFS  E-rest | laptop  phone  best 1/3 oct (Hz) | AAC ret corr | Opus ret corr | pad: E-P (worst pad band)")
    for kind, s, t0, t1 in windows:
        i0, i1 = max(0, int(t0 * fs)), min(n, int(t1 * fs))
        if i1 - i0 < fs // 2:
            continue
        r = {"cue": kind, "s": s, "t0": round(t0, 3), "t1": round(t1, 3)}
        pe, pr = ms(e[i0:i1]), ms(noe[i0:i1])
        r["E_db"], r["E_re_rest_db"] = round(db(pe), 2), round(db(pe) - db(pr), 2)
        r["laptop_db"] = round(db(band_ms(e[i0:i1], fs, 200.0, 10000.0)) - db(band_ms(noe[i0:i1], fs, 200.0, 10000.0)), 2)
        r["phone_db"] = round(db(band_ms(e[i0:i1], fs, 350.0, 8000.0)) - db(band_ms(noe[i0:i1], fs, 350.0, 8000.0)), 2)
        bs, bf = third_octave_snr(e[i0:i1], noe[i0:i1], fs, 200.0, 8000.0, hp=200.0)
        r["laptop_best_band_db"], r["laptop_best_band_hz"] = round(bs, 2), round(bf, 1) if bf else None
        bs, bf = third_octave_snr(e[i0:i1], noe[i0:i1], fs, 25.0, 8000.0)
        r["full_best_band_db"], r["full_best_band_hz"] = round(bs, 2), round(bf, 1) if bf else None
        for codec, (d, _, _) in enc.items():
            dd, ee = d[i0:i1], e[i0:i1]
            m = min(len(dd), len(ee))
            dd, ee = dd[:m], ee[:m]
            dd, ee = dd.astype(np.float64), ee.astype(np.float64)
            num, den = float(np.sum(dd * ee)), float(np.sum(ee * ee))
            r[codec + "_retained"] = round(num / den, 3) if den > 0 else None
            r[codec + "_corr"] = round(num / math.sqrt(den * float(np.sum(dd * dd))), 3) if den > 0 and float(np.sum(dd * dd)) > 0 else None
        if pad is not None:
            # D-038 (10) "never mask the pad" is checked outside the D-034 1.4 drops (the pad ducks 14 dB there by design and the
            # swell leads, C07): the parts of the window outside every drop span (k - 5 m .. k + 3.75 m) when >= 0.5 s remain
            v = cues.get("speed", 2.5)
            keep = np.ones(i1 - i0, dtype=bool)
            spans = [((dct["s"] - 5.0) / v, (dct["s"] + 3.75) / v) for dct in cues["drops"]]
            if ark:
                spans.append((ark["bow"] - 8.0, ark["bow"] + 8.0))   # the Ark's silence (the pad -18 dB there by design)
            for q0, q1 in spans:
                a0, a1 = int(q0 * fs) - i0, int(q1 * fs) - i0
                if a1 > 0 and a0 < i1 - i0:
                    keep[max(0, a0):min(i1 - i0, a1)] = False
            r["pad_check_seconds"] = round(float(np.sum(keep)) / fs, 2)
            ew, pw = e[i0:i1][keep], pad[i0:i1][keep]
            if len(ew) < fs // 2:
                ew, pw = e[i0:i1][:0], pad[i0:i1][:0]
            oe, op = octave_levels(ew, fs), octave_levels(pw, fs)
            worst, wb = -999.0, None
            pmax = max(op.values()) if op else -999.0
            if not op:
                oe = {}
            for fc in oe:
                # the pad's own bands: 250 Hz and up, within 15 dB of its strongest octave
                if fc >= 250 and op[fc] >= pmax - 15.0 and oe[fc] - op[fc] > worst:
                    worst, wb = oe[fc] - op[fc], fc
            r["E_minus_pad_db_ge250"] = round(worst, 2)
            r["worst_band_hz"] = wb
            r["octaves_E"] = {str(k): round(v, 1) for k, v in oe.items()}
            r["octaves_pad"] = {str(k): round(v, 1) for k, v in op.items()}
        if kind == "Drop" or kind == "ArkDrop":
            j0, j1 = max(0, int((t0 - 4.0) * fs)), int((t0 - 1.0) * fs)
            if kind == "ArkDrop":
                j0, j1 = int((ark["bow"] - 18.0) * fs), int((ark["bow"] - 10.0) * fs)
            elif ark and (t0 - 4.0) < ark["bow"] + 7.0 and (t0 - 1.0) > ark["bow"] - 9.0:
                # a key-shot drop inside or just after the Ark's silence (1.5 m/s: the bow passes at s 509.6, KS09 at 520): one long
                # silence; its reference is the Ark's (bow -18..-10 s), before both
                j0, j1 = int((ark["bow"] - 18.0) * fs), int((ark["bow"] - 10.0) * fs)
                r["merged_with_ark_silence"] = True
            r["rest_drop_db"] = round(db(ms(noe[i0:i1])) - db(ms(noe[j0:j1])), 2)
            r["rest_drop_hf_db"] = round(db(band_ms(noe[i0:i1], fs, 2000.0, 16000.0)) - db(band_ms(noe[j0:j1], fs, 2000.0, 16000.0)), 2)
            r["rest_drop_k_lu"] = round(db(kweight_ms(noe[i0:i1], fs)) - db(kweight_ms(noe[j0:j1], fs)), 2)
            r["mix_drop_k_lu"] = round(db(kweight_ms(mix[i0:i1], fs)) - db(kweight_ms(mix[j0:j1], fs)), 2)
            r["rest_drop_laptop_db"] = round(db(band_ms(noe[i0:i1], fs, 200.0, 10000.0)) - db(band_ms(noe[j0:j1], fs, 200.0, 10000.0)), 2)
            r["rest_drop_phone_db"] = round(db(band_ms(noe[i0:i1], fs, 400.0, 8000.0)) - db(band_ms(noe[j0:j1], fs, 400.0, 8000.0)), 2)
            r["mix_drop_laptop_db"] = round(db(band_ms(mix[i0:i1], fs, 200.0, 10000.0)) - db(band_ms(mix[j0:j1], fs, 200.0, 10000.0)), 2)
        rows.append(r)
        ss = "" if s is None else "%.1f" % s
        print("%-13s %7s %7.1f  %6.1f  %6.1f | %6.1f %6.1f %6.1f (%5s) | %5s %5s | %5s %5s | %s" % (
            kind[:13], ss, t0, r["E_db"], r["E_re_rest_db"], r["laptop_db"], r["phone_db"], r["laptop_best_band_db"], r["laptop_best_band_hz"],
            r.get("aac_retained"), r.get("aac_corr"), r.get("opus_retained"), r.get("opus_corr"),
            ("%.1f (%s)" % (r["E_minus_pad_db_ge250"], r["worst_band_hz"])) if "E_minus_pad_db_ge250" in r else "-")
              + (("  rest %+.1f dB, 2-16 kHz %+.1f, K %+.1f LU, laptop %+.1f, phone %+.1f; mix K %+.1f LU, laptop %+.1f" % (
                  r["rest_drop_db"], r["rest_drop_hf_db"], r["rest_drop_k_lu"], r["rest_drop_laptop_db"], r["rest_drop_phone_db"],
                  r["mix_drop_k_lu"], r["mix_drop_laptop_db"])) if "rest_drop_db" in r else ""))
    # C08: distance of every horn / strike / groan from the nearest vehicle meet
    meets = cues.get("meets", [])
    near = []
    for c in cues["events"]:
        if c["kind"] in ("Horn", "Strike", "Groan") and meets:
            d = min(abs(c["s"] - m["s"]) for m in meets)
            near.append({"kind": c["kind"], "s": c["s"], "meet_m": round(d, 2)})
    koi = None
    if a.k and a.nok:
        kk, nk = read_wav(a.k)[0], read_wav(a.nok)[0]
        v = cues.get("speed", 2.5)
        i0, i1 = int(1340.0 / v * fs), int(1380.0 / v * fs)
        kw, nw = kk[i0:i1], nk[i0:i1]
        koi = {"s0": 1340, "s1": 1380,
               "K_db": round(db(ms(kw)), 2), "K_re_rest_db": round(db(ms(kw)) - db(ms(nw)), 2),
               "laptop_db": round(db(band_ms(kw, fs, 200.0, 10000.0)) - db(band_ms(nw, fs, 200.0, 10000.0)), 2),
               "phone_db": round(db(band_ms(kw, fs, 400.0, 8000.0)) - db(band_ms(nw, fs, 400.0, 8000.0)), 2)}
        bs, bf = third_octave_snr(kw, nw, fs, 200.0, 8000.0, hp=200.0)
        koi["laptop_best_band_db"], koi["laptop_best_band_hz"] = round(bs, 2), round(bf, 1) if bf else None
        if pad is not None:
            ok, op = octave_levels(kw, fs), octave_levels(pad[i0:i1], fs)
            pmax = max(op.values())
            koi["K_minus_pad_db_ge250"] = round(max(ok[fc] - op[fc] for fc in ok if fc >= 250 and op[fc] >= pmax - 15.0), 2)
            koi["octaves_K"] = {str(k2): round(v2, 1) for k2, v2 in ok.items()}
            koi["octaves_pad"] = {str(k2): round(v2, 1) for k2, v2 in op.items()}
        print("koi S7: K %(K_db).1f dBFS, re rest %(K_re_rest_db)+.1f dB, laptop %(laptop_db)+.1f, phone %(phone_db)+.1f, best laptop 1/3 oct %(laptop_best_band_db)+.1f" % koi
              + (" (%s Hz), K - pad (pad octaves) %+.1f dB" % (koi["laptop_best_band_hz"], koi.get("K_minus_pad_db_ge250", float("nan")))))
    # gates
    gates = []
    for r in rows:
        if r["cue"] == "Drop":
            gates.append(("C07 drop s %s: rest <= -10 dB" % r["s"], r["rest_drop_db"], r["rest_drop_db"] <= -10.0))
            gates.append(("C07 drop s %s: rest K-weighted <= -8 LU" % r["s"], r["rest_drop_k_lu"], r["rest_drop_k_lu"] <= -8.0))
            gates.append(("C07 drop s %s: rest laptop <= -8 dB" % r["s"], r["rest_drop_laptop_db"], r["rest_drop_laptop_db"] <= -8.0))
            gates.append(("C07 drop s %s: the swell is the loudest (E re rest > 0)" % r["s"], r["E_re_rest_db"], r["E_re_rest_db"] > 0.0))
        if r["cue"] == "ArkDrop":
            gates.append(("D-043 the Ark's silence: rest <= -10 dB", r["rest_drop_db"], r["rest_drop_db"] <= -10.0))
            gates.append(("D-043 the Ark's silence: rest laptop <= -8 dB", r["rest_drop_laptop_db"], r["rest_drop_laptop_db"] <= -8.0))
            gates.append(("D-043 the Ark's silence: the Ark is the loudest (E re rest > 0)", r["E_re_rest_db"], r["E_re_rest_db"] > 0.0))
        if r["cue"] == "ArkBelly":
            gates.append(("D-043 under the belly: E re rest >= -12 dB (heard, not masked)", r["E_re_rest_db"], r["E_re_rest_db"] >= -12.0))
        if r["cue"].startswith("Swell") or r["cue"] == "Hull" or r["cue"] == "ArkBelly":
            gates.append(("C09 %s: laptop best band >= -3 dB re rest" % r["cue"], r["laptop_best_band_db"], r["laptop_best_band_db"] >= -3.0))
        if "E_minus_pad_db_ge250" in r and r["E_minus_pad_db_ge250"] > -900:
            gates.append(("D-038 (10) %s s %s: E under the pad in its octaves" % (r["cue"], r["s"]), r["E_minus_pad_db_ge250"], r["E_minus_pad_db_ge250"] < 0.0))
    for n in near:
        gates.append(("C08 %s s %s: >= 15 m from a meet" % (n["kind"], n["s"]), n["meet_m"], n["meet_m"] >= 15.0))
    if koi:
        gates.append(("C09 koi S7: laptop band >= -6 dB re rest", koi["laptop_db"], koi["laptop_db"] >= -6.0))
        if "K_minus_pad_db_ge250" in koi:
            gates.append(("D-038 (10) koi under the pad in its octaves", koi["K_minus_pad_db_ge250"], koi["K_minus_pad_db_ge250"] < 0.0))
    nfail = 0
    for g, v, ok in gates:
        if not ok:
            nfail += 1
        print("%s %-66s %+.2f" % ("PASS" if ok else "FAIL", g, v))
    print("gates: %d / %d pass" % (len(gates) - nfail, len(gates)))
    out = {"mix": a.mix, "speed": cues.get("speed"), "fs": fs, "cues": rows, "meet_distances": near, "koi": koi,
           "gates": [{"gate": g, "value": v, "pass": ok} for g, v, ok in gates],
           "encode_lags": {k: [v[1], v[2]] for k, v in enc.items()}}
    if a.json:
        with open(a.json, "w") as f:
            json.dump(out, f, indent=1)
    return 0


if __name__ == "__main__":
    sys.exit(main())
