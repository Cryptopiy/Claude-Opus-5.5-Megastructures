"""Analysis: arm directions, head extent, eye candidates of the octopus source. Prints numbers; writes Logs/kraken/layout.png."""
import sys, os
sys.path.insert(0, os.path.dirname(__file__))
import numpy as np
from kr_common import *
import matplotlib; matplotlib.use("Agg")
import matplotlib.pyplot as plt

V, N, UV, I, tex = load_glb(SRC_OCTO)
V, N, I = gltf_to_unity(V, N, I)
T = np.asarray(tex).astype(np.float32) / 255
h, w = T.shape[:2]
col = T[np.clip(((1 - UV[:, 1]) * (h - 1)).astype(int), 0, h - 1), np.clip((UV[:, 0] * (w - 1)).astype(int), 0, w - 1)]
r = np.hypot(V[:, 0], V[:, 2]); ang = np.degrees(np.arctan2(V[:, 0], V[:, 2]))
# arm directions: histogram of angle for r in 0.9..1.3
m = (r > 0.9) & (r < 1.3)
hist, edges = np.histogram(ang[m], bins=360, range=(-180, 180))
hs = np.convolve(np.r_[hist[-5:], hist, hist[:5]], np.ones(5) / 5, "same")[5:-5]
peaks = [i for i in range(360) if hs[i] == max(hs[(i + k) % 360] for k in range(-12, 13)) and hs[i] > hs.mean()]
log("arm peak angles (deg, 0=+z, cw to +x):", [round(edges[i] + 0.5, 1) for i in peaks])
# head: vertices above y 0.25
hd = V[:, 1] > 0.3
log("head (y>0.3) extent min", np.round(V[hd].min(0), 3), "max", np.round(V[hd].max(0), 3))
for yl in (0.1, 0.2, 0.3, 0.5, 0.7, 0.9):
    s = np.abs(V[:, 1] - yl) < 0.01
    log("  slice y=%.1f: x %.3f..%.3f z %.3f..%.3f n %d" % (yl, V[s, 0].min(), V[s, 0].max(), V[s, 2].min(), V[s, 2].max(), s.sum()))
# eye candidates: saturated orange, bright
R, G, B = col[:, 0], col[:, 1], col[:, 2]
eye = (R > 0.75) & (G > 0.2) & (G < 0.6) & (B < 0.2)
log("eye-colour vertices", int(eye.sum()))
if eye.sum():
    from scipy.cluster.vq import kmeans2
    P = V[eye]
    for k in (2, 3, 4):
        c, l = kmeans2(P, k, seed=1, minit="++")
        log("  k=%d centres" % k, np.round(c, 3).tolist(), "sizes", np.bincount(l).tolist())
fig, ax = plt.subplots(1, 3, figsize=(24, 8))
ax[0].scatter(V[::40, 0], V[::40, 2], s=0.2, c=col[::40]); ax[0].set_aspect("equal"); ax[0].set_title("top (x right, z up)")
if eye.sum(): ax[0].scatter(V[eye, 0], V[eye, 2], s=2, c="yellow")
ax[1].scatter(V[::40, 0], V[::40, 1], s=0.2, c=col[::40]); ax[1].set_aspect("equal"); ax[1].set_title("front (x, y)")
if eye.sum(): ax[1].scatter(V[eye, 0], V[eye, 1], s=2, c="yellow")
ax[2].scatter(V[::40, 2], V[::40, 1], s=0.2, c=col[::40]); ax[2].set_aspect("equal"); ax[2].set_title("side (z, y)")
if eye.sum(): ax[2].scatter(V[eye, 2], V[eye, 1], s=2, c="yellow")
for a in ax: a.set_facecolor("#777")
plt.tight_layout(); plt.savefig(os.path.join(LOG_DIR, "layout.png"), dpi=50)
