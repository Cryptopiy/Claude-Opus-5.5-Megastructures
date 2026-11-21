"""Vectorised triangle rasteriser in texture space (barycentric interpolation of per-corner values) and gutter dilation."""
import numpy as np


def raster(tri_uv, tri_val, size, chunk=200000):
    """tri_uv: (M,3,2) texel coordinates (x right, y down), tri_val: (M,3,C). Returns (img (H,W,C) float32, mask (H,W) bool).
    Every texel centre covered by a triangle gets the interpolated value (last writer wins)."""
    H = W = size
    C = tri_val.shape[2]
    img = np.zeros((H * W, C), np.float32)
    msk = np.zeros(H * W, bool)
    lo = np.floor(tri_uv.min(1) - 0.5).astype(np.int64)
    hi = np.ceil(tri_uv.max(1) - 0.5).astype(np.int64)
    ext = (hi - lo + 1).max(1)
    for lim in (2, 4, 8, 16, 32, 64, 128, 4096):
        sel = np.where((ext <= lim) & (ext > lim // 2 if lim > 2 else ext <= lim))[0]
        if len(sel) == 0: continue
        n = lim
        g = np.stack(np.meshgrid(np.arange(n), np.arange(n), indexing="xy"), -1).reshape(-1, 2)   # (n*n, 2)
        step = max(1, chunk // (n * n))
        for c0 in range(0, len(sel), step):
            ids = sel[c0:c0 + step]
            P = tri_uv[ids]                      # (m,3,2)
            px = lo[ids][:, None, :] + g[None, :, :]          # (m,k,2) texel ints
            pc = px + 0.5
            a, b, c = P[:, 0][:, None, :], P[:, 1][:, None, :], P[:, 2][:, None, :]
            v0, v1, v2 = b - a, c - a, pc - a
            d00 = (v0 * v0).sum(-1); d01 = (v0 * v1).sum(-1); d11 = (v1 * v1).sum(-1)
            d20 = (v2 * v0).sum(-1); d21 = (v2 * v1).sum(-1)
            den = d00 * d11 - d01 * d01
            ok = np.abs(den) > 1e-12
            den = np.where(ok, den, 1.0)
            l1 = (d11 * d20 - d01 * d21) / den
            l2 = (d00 * d21 - d01 * d20) / den
            l0 = 1 - l1 - l2
            e = -1e-4
            inside = ok & (l0 >= e) & (l1 >= e) & (l2 >= e) & (px[..., 0] >= 0) & (px[..., 0] < W) & (px[..., 1] >= 0) & (px[..., 1] < H)
            mi, ki = np.nonzero(inside)
            if len(mi) == 0: continue
            vals = tri_val[ids[mi]]               # (q,3,C)
            lam = np.stack([l0[mi, ki], l1[mi, ki], l2[mi, ki]], 1)[:, :, None]
            out = (vals * lam).sum(1)
            lin = px[mi, ki, 1] * W + px[mi, ki, 0]
            img[lin] = out
            msk[lin] = True
    return img.reshape(H, W, C), msk.reshape(H, W)


def dilate(img, msk, iters=24):
    """Fills uncovered texels from covered neighbours (mean of the covered 3x3), `iters` rings: mip-safe gutters."""
    img = img.copy(); m = msk.copy()
    for _ in range(iters):
        acc = np.zeros_like(img); cnt = np.zeros(m.shape, np.float32)
        for dy in (-1, 0, 1):
            for dx in (-1, 0, 1):
                if dx == 0 and dy == 0: continue
                sm = np.roll(np.roll(m, dy, 0), dx, 1)
                si = np.roll(np.roll(img, dy, 0), dx, 1)
                acc += si * sm[..., None]; cnt += sm
        new = (~m) & (cnt > 0)
        if not new.any(): break
        img[new] = acc[new] / cnt[new][:, None]
        m |= new
    if (~m).any():
        mean = img[m].mean(0)
        img[~m] = mean
    return img
