"""Per-vertex rig attributes of the octopus body (limb id, arc length u, skin weight w, angle theta), the rest
centre lines with their frames, the eye sockets and the arm cut. glTF units, Unity handedness (see kr_common)."""
import os, sys
sys.path.insert(0, os.path.dirname(__file__))
import numpy as np
import scipy.sparse as sp
from scipy.sparse.linalg import spsolve
from scipy.spatial import cKDTree
from kr_common import *
from kr_segment import smooth_poly, resample, N_ARMS

DOWN = np.array([0.0, -1.0, 0.0])


def frames(P):
    """Rest frames along a polyline: T (tangent), N (towards the sucker side = down, orthogonalised), B = T x N."""
    T = np.gradient(P, axis=0)
    T /= np.linalg.norm(T, axis=1, keepdims=True) + 1e-12
    Nn = DOWN[None, :] - T * (T @ DOWN)[:, None]
    Nn /= np.linalg.norm(Nn, axis=1, keepdims=True) + 1e-12
    B = np.cross(T, Nn)
    return T, Nn, B


def centre_lines(arms_info, crown, step=0.004):
    """Crown -> level-set centroids, smoothed, resampled every `step` units. Returns list of dicts (P, s, T, N, B, rad)."""
    out = []
    for a in arms_info:
        cen = a["cen"]; rad = a["rad"]
        P = np.vstack([crown, cen])
        P = smooth_poly(P, 2.0)
        P, s = resample(P, step)
        # ring radius on the resampled arc (the first centroid sits at arc |cen0 - crown|)
        s_c = np.r_[0, np.cumsum(np.linalg.norm(np.diff(np.vstack([crown, cen]), axis=0), axis=1))]
        r = np.interp(s, s_c[1:], rad)
        T, Nn, B = frames(P)
        out.append(dict(P=P, s=s, T=T, N=Nn, B=B, rad=r, az=a["az"]))
    return out


def project_on_lines(W, limb, lines):
    """u (arc, units) and theta (rad, 0 = sucker side) of every vertex on its limb's centre line."""
    u = np.zeros(len(W)); th = np.zeros(len(W)); dist = np.zeros(len(W))
    for k, L in enumerate(lines):
        m = np.where(limb == k)[0]
        if len(m) == 0: continue
        tree = cKDTree(L["P"])
        d, j = tree.query(W[m])
        # refine along the segment j -> j+1 / j-1
        P = L["P"]; n = len(P)
        j0 = np.clip(j, 0, n - 2)
        a = P[j0]; b = P[j0 + 1]
        t = np.clip(np.einsum("ij,ij->i", W[m] - a, b - a) / np.maximum(np.einsum("ij,ij->i", b - a, b - a), 1e-20), 0, 1)
        u[m] = L["s"][j0] + t * (L["s"][j0 + 1] - L["s"][j0])
        c = a + t[:, None] * (b - a)
        dv = W[m] - c
        th[m] = np.arctan2(np.einsum("ij,ij->i", dv, L["B"][j0]), np.einsum("ij,ij->i", dv, L["N"][j0]))
        dist[m] = np.linalg.norm(dv, axis=1)
    return u, th, dist


def harmonic(W, I, fixed_idx, fixed_val):
    """Solves the graph Laplace equation (edge weights 1/length) with Dirichlet values at fixed vertices."""
    e = np.concatenate([I[:, [0, 1]], I[:, [1, 2]], I[:, [2, 0]]])
    e = np.unique(np.sort(e, axis=1), axis=0)
    wgt = 1.0 / np.maximum(np.linalg.norm(W[e[:, 0]] - W[e[:, 1]], axis=1), 1e-9)
    n = len(W)
    Aw = sp.coo_matrix((wgt, (e[:, 0], e[:, 1])), shape=(n, n)); Aw = (Aw + Aw.T).tocsr()
    deg = np.asarray(Aw.sum(1)).ravel()
    Lm = sp.diags(deg) - Aw
    isfix = np.zeros(n, bool); isfix[fixed_idx] = True
    val = np.zeros(n); val[fixed_idx] = fixed_val
    free = np.where(~isfix)[0]
    Lff = Lm[free][:, free].tocsc()
    rhs = -Lm[free][:, np.where(isfix)[0]] @ val[isfix]
    from scipy.sparse.linalg import cg
    x, info = cg(Lff, rhs, rtol=1e-7, maxiter=4000)
    out = val.copy(); out[free] = np.clip(x, 0, 1)
    return out


def rig_attributes(seg, arms_info, head_c, lab, g_lo=0.42, g_hi=0.72):
    W, I, geo, crown = seg["W"], seg["I"], seg["geo"], seg["crown"]
    lines = centre_lines(arms_info, crown)
    az = np.degrees(np.arctan2(W[:, 0] - crown[0], W[:, 2] - crown[2]))
    arm_az = np.array([a["az"] for a in arms_info])
    dd = np.abs(((az[:, None] - arm_az[None, :]) + 180) % 360 - 180)
    limb = np.argmin(dd, axis=1)
    for k, a in enumerate(arms_info):
        limb[a["far"]] = k                          # beyond the split the component decides
    u, th, dist = project_on_lines(W, limb, lines)
    # Dirichlet: 1 on the free arms past g_hi, 0 on the mantle, the crown disc and the web mid-lines
    far_any = np.zeros(len(W), bool)
    for a in arms_info: far_any |= a["far"]
    one = far_any & (geo > g_hi)
    srt = np.sort(arm_az)
    mids = [(srt[i] + srt[(i + 1) % N_ARMS] + (360 if i == N_ARMS - 1 else 0)) / 2 for i in range(N_ARMS)]
    mids = (np.array(mids) + 180) % 360 - 180
    dm = np.min(np.abs(((az[:, None] - mids[None, :]) + 180) % 360 - 180), axis=1)
    zero = (lab == head_c) | (geo < g_lo) | ((dm < 2.5) & (geo < g_hi) & ~far_any)
    zero &= ~one
    idx = np.r_[np.where(one)[0], np.where(zero)[0]]
    val = np.r_[np.ones(one.sum()), np.zeros(zero.sum())]
    w = harmonic(W, I, idx, val)
    w = w * w * (3 - 2 * w)                          # smoothstep: flat ends
    log("rig weights: one %d zero %d free %d, mean w %.3f" % (one.sum(), zero.sum(), len(W) - len(idx), w.mean()))
    return dict(lines=lines, limb=limb, u=u, theta=th, w=w, rdist=dist)
