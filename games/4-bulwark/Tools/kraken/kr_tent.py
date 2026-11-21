"""Cylindrical unwrap of the Tripo tentacle (SM_Creature_MonsterTentacles.glb) into a tileable skin tile.

The straight thick part of the source tentacle (z in [Z0, Z1], glTF units, Unity handedness) is unwrapped around its
slice-centre line: theta = angle around the axis (0 = the underside, -y), s = arc length divided by the local mean radius
(the tile is scale-free: on a procedural tube of radius r one tile covers TILE_RADII * r metres). Relief h = rho / base - 1
(base = the slice's angular median radius, smoothed), so suckers are positive bumps. Rasterised from the source triangles
with the source atlas sampled per texel; the s ends are cross-faded so the tile repeats along the tube.
"""
import os, sys
sys.path.insert(0, os.path.dirname(__file__))
import numpy as np
from scipy.ndimage import gaussian_filter1d, gaussian_filter
from kr_common import *
from kr_raster import raster, dilate

Z0, Z1 = -0.40, 0.60          # straight thick part of the source tentacle (units)
SLICE = 0.01


def unwrap(size_theta=2048, size_s=2048, fade=0.16):
    V, N, UV, I, tex = load_glb(SRC_TENT)
    V, N, I = gltf_to_unity(V, N, I)
    zs = np.arange(Z0 - 0.05, Z1 + 0.05 + SLICE, SLICE)
    cx = []; cy = []; rbar = []
    for z in zs:
        m = np.abs(V[:, 2] - z) < SLICE * 0.6
        p = V[m]
        cx.append(np.median(p[:, 0])); cy.append(0.5 * (p[:, 1].max() + p[:, 1].min()))
    cx = gaussian_filter1d(np.array(cx), 3, mode="nearest"); cy = gaussian_filter1d(np.array(cy), 3, mode="nearest")
    vc = np.stack([np.interp(V[:, 2], zs, cx), np.interp(V[:, 2], zs, cy)], 1)
    d = V[:, :2] - vc
    theta = np.arctan2(d[:, 0], -d[:, 1])                 # 0 = underside (-y), +pi/2 = +x side
    rho = np.linalg.norm(d, axis=1)
    # base radius per (slice, angle bin): median, smoothed
    nb = 48
    tb = np.clip(((theta + np.pi) / (2 * np.pi) * nb).astype(int), 0, nb - 1)
    zb = np.clip(((V[:, 2] - zs[0]) / SLICE).astype(int), 0, len(zs) - 1)
    base = np.zeros((len(zs), nb))
    for i in range(len(zs)):
        m = zb == i
        for j in range(nb):
            q = rho[m & (tb == j)]
            base[i, j] = np.median(q) if len(q) > 3 else np.nan
    # fill gaps and smooth (wrapping in angle)
    for i in range(len(zs)):
        row = base[i]; ok = np.isfinite(row)
        if ok.sum() < 3: base[i] = np.nanmean(base[max(0, i - 2):i + 3]); continue
        xs = np.arange(nb)
        base[i] = np.interp(xs, xs[ok], row[ok], period=nb)
    base = gaussian_filter(np.pad(base, ((0, 0), (nb, nb)), mode="wrap"), (2.0, 2.5))[:, nb:-nb]
    bv = base[zb, tb]
    h = rho / np.maximum(bv, 1e-6) - 1.0
    rmean = base.mean(1)
    # s = integral dz / rmean (arc of the slice centres ~ dz on this straight part)
    srad = np.r_[0, np.cumsum(SLICE / rmean[:-1])]
    s_v = np.interp(V[:, 2], zs, srad)
    s0 = np.interp(Z0, zs, srad); s1 = np.interp(Z1, zs, srad)
    span = s1 - s0
    log("tentacle tile: z %.2f..%.2f, %.2f radii long, mean radius %.4f u" % (Z0, Z1, span, rmean.mean()))
    # triangles fully inside the window (plus margin) -> (theta, s) texel coordinates
    nf = int(fade * size_s)
    pre = fade * span
    zpre = np.interp(s0 - pre, srad, zs)
    keep = ((V[I, 2] > zpre - 0.02) & (V[I, 2] < Z1 + 0.02)).all(1)
    T = I[keep]
    th = theta[T]
    ref = th[:, :1]
    th = np.where(th - ref > np.pi, th - 2 * np.pi, np.where(th - ref < -np.pi, th + 2 * np.pi, th))
    tx = (th + np.pi) / (2 * np.pi) * size_theta
    ty = (s_v[T] - (s0 - pre)) / span * size_s           # rows [0, nf) = the stretch before the tile start
    vals = np.concatenate([UV[T], h[T][..., None]], 2)
    tri = np.stack([tx, ty], 2)
    R = size_s + nf
    img = None; msk = None
    for shift in (0, -size_theta, size_theta):           # wrap copies in theta
        t2 = tri.copy(); t2[..., 0] += shift
        im, mk = raster(t2, vals, R)
        im = im[:, :size_theta]; mk = mk[:, :size_theta]
        if img is None: img, msk = im, mk
        else:
            m = mk & ~msk
            img[m] = im[m]; msk |= mk
    img = dilate(img, msk, 40)
    A = np.asarray(tex).astype(np.float32) / 255.0
    H, Wd = A.shape[:2]
    u = np.clip(img[..., 0], 0, 1); v = np.clip(img[..., 1], 0, 1)
    xi = np.clip((u * (Wd - 1)).astype(int), 0, Wd - 1); yi = np.clip(((1 - v) * (H - 1)).astype(int), 0, H - 1)
    alb = A[yi, xi]
    hgt = img[..., 2]
    tile_a = alb[nf:nf + size_s].copy(); tile_h = hgt[nf:nf + size_s].copy()
    pre_a = alb[:nf]; pre_h = hgt[:nf]
    w = (np.linspace(0, 1, nf) ** 1.0)[:, None]
    w = w * w * (3 - 2 * w)
    tile_a[size_s - nf:] = tile_a[size_s - nf:] * (1 - w[..., None]) + pre_a * w[..., None]
    tile_h[size_s - nf:] = tile_h[size_s - nf:] * (1 - w) + pre_h * w
    return dict(albedo=tile_a, height=tile_h, span=span, rmean=float(rmean.mean()))
