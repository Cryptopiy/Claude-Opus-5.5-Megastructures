"""Prints facts about the two source GLBs (components, welded vertex count, extents). Analysis only."""
import sys, os
sys.path.insert(0, os.path.dirname(__file__))
import numpy as np
from kr_common import *

for path in (SRC_OCTO, SRC_TENT):
    V, N, UV, I, tex = load_glb(path)
    log(os.path.basename(path), "V", len(V), "T", len(I), "tex", tex.size)
    rep = weld(V, I, 1e-6)
    log("  welded vertices", len(np.unique(rep)))
    Iw = rep[I]
    nc, lab = components(len(V), Iw)
    cnt = np.bincount(lab[np.unique(Iw)])
    order = np.argsort(-cnt)
    log("  components", nc, "largest", cnt[order[:10]].tolist(), "small(<200 verts)", int((cnt < 200).sum()))
    for c in order[:6]:
        m = lab == c
        log("   comp", c, "n", int(cnt[c]), "min", np.round(V[m].min(0), 3).tolist(), "max", np.round(V[m].max(0), 3).tolist())
    e = np.linalg.norm(V[I[:, 0]] - V[I[:, 1]], axis=1)
    log("  edge len mean %.5f median %.5f max %.4f" % (e.mean(), np.median(e), e.max()))
    log("  uv range", UV.min(0), UV.max(0))
