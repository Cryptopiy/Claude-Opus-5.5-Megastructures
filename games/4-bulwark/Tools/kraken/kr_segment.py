"""Kraken body analysis: weld, floaters, crown centre, geodesic distance, the eight arms, their centre lines.

segment(V, I) -> dict with the welded mesh and per-vertex fields; cached in Library/KrakenCache/segment.npz.
All lengths here are in glTF units (x MODEL_SCALE for metres), Unity handedness (x mirrored).
"""
import os, sys
sys.path.insert(0, os.path.dirname(__file__))
import numpy as np
import scipy.sparse as sp
from scipy.sparse.csgraph import dijkstra, connected_components
from kr_common import *

N_ARMS = 8


def edge_graph(V, I):
    e = np.concatenate([I[:, [0, 1]], I[:, [1, 2]], I[:, [2, 0]]])
    e = np.sort(e, axis=1)
    e = np.unique(e, axis=0)
    w = np.linalg.norm(V[e[:, 0]] - V[e[:, 1]], axis=1)
    A = sp.coo_matrix((w, (e[:, 0], e[:, 1])), shape=(len(V), len(V)))
    return (A + A.T).tocsr(), e


def compact(V, I):
    """Drops unreferenced vertices; returns (V', I', old index of each new vertex)."""
    used = np.unique(I)
    remap = -np.ones(len(V), np.int64); remap[used] = np.arange(len(used))
    return V[used], remap[I], used


def remove_floaters(V, I, min_frac=0.002):
    nc, lab = components(len(V), I)
    cnt = np.bincount(lab[I[:, 0]], minlength=nc)
    keep_c = cnt >= max(1, min_frac * len(I))
    keepT = keep_c[lab[I[:, 0]]]
    log("floaters: %d components with triangles, kept %d, dropped %d triangles" % (int((cnt > 0).sum()), int(keep_c.sum()), int((~keepT).sum())))
    return I[keepT]


def arm_angles(V):
    r = np.hypot(V[:, 0], V[:, 2]); ang = np.degrees(np.arctan2(V[:, 0], V[:, 2]))
    m = (r > 1.0) & (r < 1.35) & (V[:, 1] < 0.3)
    hist, edges = np.histogram(ang[m], bins=360, range=(-180, 180))
    hs = np.convolve(np.r_[hist[-6:], hist, hist[:6]], np.ones(7) / 7, "same")[6:-6]
    peaks = [i for i in range(360) if hs[i] == max(hs[(i + k) % 360] for k in range(-14, 15)) and hs[i] > 0.5 * hs.max()]
    pk = sorted(peaks, key=lambda i: -hs[i])[:N_ARMS]
    return np.sort(np.array([edges[i] + 0.5 for i in pk]))


def segment(Vu, Iu):
    """Vu, Iu: Unity-handed source mesh (UV-split vertices). Returns dict."""
    rep = weld(Vu, Iu, 1e-6)
    Iw = rep[Iu]
    Iw = remove_floaters(Vu, Iw)
    W, Iw, used = compact(Vu, Iw)        # welded vertices; `used` maps back to a source vertex
    log("welded mesh: %d vertices, %d triangles" % (len(W), len(Iw)))
    A, E = edge_graph(W, Iw)
    angs = arm_angles(W)
    log("arm azimuths (deg):", np.round(angs, 1).tolist())
    # crown centre: least-squares intersection of the 8 arm rays (xz) fitted on r 0.8..1.2
    r = np.hypot(W[:, 0], W[:, 2]); az = np.degrees(np.arctan2(W[:, 0], W[:, 2]))
    rows, rhs = [], []
    for a in angs:
        d = np.abs(((az - a) + 180) % 360 - 180)
        m = (d < 12) & (r > 0.8) & (r < 1.25) & (W[:, 1] < 0.25)
        P = W[m][:, [0, 2]]
        c = P.mean(0)
        u, s, vt = np.linalg.svd(P - c)
        dvec = vt[0]
        nrm = np.array([-dvec[1], dvec[0]])
        rows.append(nrm); rhs.append(nrm @ c)
    rows = np.array(rows); rhs = np.array(rhs)
    cxz = np.linalg.lstsq(rows, rhs, rcond=None)[0]
    near = np.hypot(W[:, 0] - cxz[0], W[:, 2] - cxz[1]) < 0.06
    cy = W[near, 1].min()
    crown = np.array([cxz[0], cy, cxz[1]])
    log("crown centre (glTF units, Unity handed):", np.round(crown, 4).tolist())
    src = np.where(np.linalg.norm(W - crown, axis=1) < 0.05)[0]
    geo = dijkstra(A, directed=False, indices=src, min_only=True)
    log("geodesic: max %.3f" % geo[np.isfinite(geo)].max())
    return dict(W=W, I=Iw, used=used, crown=crown, angs=angs, geo=geo, A=A)


def arm_labels(seg, d_split):
    """Connected components of {geo > d_split}: the arms. Returns label per vertex (-1 = body) and per-arm azimuth."""
    W, I, geo, crown = seg["W"], seg["I"], seg["geo"], seg["crown"]
    far = geo > d_split
    m = far[I].all(axis=1)
    sub = I[m]
    nc, lab = components(len(W), sub)
    lab = np.where(far, lab, -1)
    cnt = np.bincount(lab[lab >= 0])
    big = np.argsort(-cnt)[:N_ARMS + 2]
    info = []
    for c in big:
        mm = lab == c
        p = W[mm]
        az = np.degrees(np.arctan2(p[:, 0].mean() - crown[0], p[:, 2].mean() - crown[2]))
        info.append((c, int(cnt[c]), az, float(p[:, 1].mean())))
    return lab, info


def smooth_poly(P, sigma):
    """Gaussian smoothing of a polyline (index space), ends kept."""
    from scipy.ndimage import gaussian_filter1d
    Q = gaussian_filter1d(P, sigma, axis=0, mode="nearest")
    Q[0] = P[0]
    return Q


def resample(P, step):
    seg = np.linalg.norm(np.diff(P, axis=0), axis=1)
    s = np.r_[0, np.cumsum(seg)]
    n = max(2, int(np.ceil(s[-1] / step)) + 1)
    t = np.linspace(0, s[-1], n)
    return np.stack([np.interp(t, s, P[:, k]) for k in range(3)], 1), t


def arms(seg, d_split=0.65, bin_w=0.012):
    """Per arm: centre line (resampled every 0.004 u, from the crown centre), the ring radius per sample, and the
    vertex sets. Returns (list of arm dicts sorted by azimuth, head label, arm label per vertex)."""
    W, geo, crown = seg["W"], seg["geo"], seg["crown"]
    lab, info = arm_labels(seg, d_split)
    head_c = max(info, key=lambda x: x[3])[0]           # highest mean y = the mantle
    arm_cs = [x for x in info if x[0] != head_c and x[1] > 1000][:N_ARMS]
    arm_cs.sort(key=lambda x: x[2])
    az = np.degrees(np.arctan2(W[:, 0] - crown[0], W[:, 2] - crown[2]))
    out = []
    for k, (c, n, a, _) in enumerate(arm_cs):
        far = lab == c
        d = np.abs(((az - a) + 180) % 360 - 180)
        near = (~np.isin(lab, [x[0] for x in info])) & (d < 16) & (W[:, 1] < 0.36) & (geo > 0.35)
        mem = far | near
        g = geo[mem]; p = W[mem]
        bins = np.arange(0.40, g.max() + bin_w, bin_w)
        cen, rad, gb = [], [], []
        for b0 in bins:
            m = (g >= b0) & (g < b0 + bin_w)
            if m.sum() < 30: continue
            q = p[m]; cq = q.mean(0)
            cen.append(cq); rad.append(np.median(np.linalg.norm(q - cq, axis=1))); gb.append(b0)
        cen = np.array(cen); rad = np.array(rad); gb = np.array(gb)
        out.append(dict(index=k, comp=c, az=a, far=far, members=mem, cen=cen, rad=rad, gbin=gb))
    return out, head_c, lab
