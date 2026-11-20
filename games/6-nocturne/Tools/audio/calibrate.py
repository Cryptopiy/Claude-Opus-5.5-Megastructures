#!/usr/bin/env python3
"""AU calibration helper (docs/AUDIO.md "Calibration"): reads a harness walk JSON (bins of 100 ms: s, mix dB, stem dB, the
score's target dB per layer) and prints, per beat and per layer, the median of (stem - target) over the bins where the layer
is steady (no S-window, no beat-boundary blend, no chord window for P), and per chord the pad's deviation. Usage:
  python -I Tools/audio/calibrate.py <walk.json>
"""
import json
import sys

import numpy as np

BOUNDS = [0.0, 120.0, 400.0, 560.0, 880.0, 1080.0, 1260.0, 1500.0]
NAMES = ["R", "D", "H", "T", "W", "P", "K", "X"]
EXCL = [(0, 14), (98, 130), (397, 423), (538, 556), (873, 899), (1336, 1384), (1470, 1500)]
CHORD_S = [0, 124, 286, 402, 540, 700, 880, 1080, 1262, 1350, 1486]
CHORD_H = [20, 20, 20, 20, 20, 20, 20, 20, 20, 20, 14]


def main(path):
    d = json.load(open(path))
    s = np.asarray(d["bins"]["s"])
    stems = np.asarray(d["bins"]["stems"])
    tg = np.asarray(d["bins"]["targets"])
    n = min(len(s), stems.shape[1], tg.shape[1])
    s, stems, tg = s[:n], stems[:, :n], tg[:, :n]
    steady = np.ones(n, bool)
    for a, b in EXCL:
        steady &= ~((s >= a) & (s <= b))
    for bb in BOUNDS[1:-1]:
        steady &= ~((s >= bb - 14) & (s <= bb + 14))
    print("beat " + " ".join("%7s" % k for k in NAMES))
    for b in range(7):
        m = steady & (s >= BOUNDS[b]) & (s < BOUNDS[b + 1])
        row = []
        for k in range(8):
            v = stems[k, m] - tg[k, m]
            v = v[(tg[k, m] > -150) & (stems[k, m] > -150)]
            row.append("%+7.2f" % np.median(v) if v.size > 20 else "      -")
        print("  %d  " % (b + 1) + " ".join(row))
    print("pad per chord (stable part, median stem - target, dB):")
    for c in range(len(CHORD_S)):
        a = CHORD_S[c] + CHORD_H[c]
        b = CHORD_S[c + 1] - CHORD_H[c + 1] if c + 1 < len(CHORD_S) else 1500
        m = steady & (s >= a) & (s <= b)
        v = stems[5, m] - tg[5, m]
        v = v[(tg[5, m] > -150) & (stems[5, m] > -150)]
        print("  C%-2d s %6.0f-%6.0f: %s" % (c, a, b, ("%+.2f (n %d)" % (np.median(v), v.size)) if v.size > 10 else "-"))


if __name__ == "__main__":
    main(sys.argv[1])
