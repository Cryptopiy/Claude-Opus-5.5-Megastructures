"""foliage_compose: the leaf-cluster sprites of texprep step 7 (owner FO; TEXTURE_PLAN 1.4 step 7 and 4, foliage.json).

A pure library module for Tools/texprep.py (TP): numpy and PIL only, deterministic (seeded), no file IO, nothing imported from
the raw folder. texprep decodes a foliage.json set to float32 linear maps and calls

    out = compose(cfg_set, maps)

cfg_set  the set's dict from foliage.json ("sourceMetres", "sourceGrid", "leaves", "res", "flipbook", "spriteMetres",
         "compose": {...}).
maps     dict of float32 arrays at the source resolution: "albedo" (H, W, 3) linear RGB, "normal" (H, W, 3) GL tangent normal in
         [-1, 1], "opacity" (H, W) in [0, 1]; optional "rough" (H, W), "height" (H, W), "scattering" (H, W).

It returns a dict of float32 arrays at res x res (the 2 x 2 flipbook of `sprites` cluster sprites, slice k at column k % 2,
row k // 2 from the top-left, Unity's flipbook order):
    "albedo"  (res, res, 3) linear RGB, straight (not premultiplied), push-pull dilated into alpha 0
    "alpha"   (res, res) coverage (opacity)
    "normal"  (res, res, 3) GL tangent normal (unit, z >= 0.2), dilated
    "ao"      (res, res) leaf-stack occlusion in [aoMin, 1]
    "height"  (res, res) 0 = the back of the cluster, 1 = its front (layer position + the leaf's own relief)
    "rough"   (res, res) source roughness carried through (texprep maps it to the class smoothness range)
    "scatter" (res, res) thin-leaf transmission weight (source Scattering map, else 0.8 on leaves, 0 on twigs)
and "stats": {"coverage": [per sprite], "leaves": [per sprite], "twigs": [per sprite], "leafMetres": [min, max]}.

What a sprite is: a 2 m patch of crown seen from outside, made of 2-3 sprays (beech grows its leaves in flat sprays, which is
why cards work for it): a curved main twig with alternate side twigs, leaves alternating along every twig at 4.5-7 cm
internodes, each leaf pointing 40-75 deg off its twig, at true size (the source leaf of about 0.127 m x leafScale 1.0-1.3),
tilted out of the card plane by up to 60 deg (foreshortened, its normal tilted with it), 20 % showing their paler underside.
Leaves outside a lobed outline (radius fillRadius x (1 +- 0.12 lobes)) are dropped, so the card corners stay empty and the
crown never shows card squares; the twigs end at their last leaf. Sprays and leaves are composited front to back
(premultiplied, at 2x supersampling, then a 2x box downsample): AO = 1 - aoDepth x (the blurred leaf layers in front),
clamped to aoMin; height = the layer position. The border of borderPx stays empty.
"""
import math

import numpy as np
from PIL import Image

VERSION = 1


# ---------------------------------------------------------------------------------------------------------------- helpers

def _rng(seed, k):
    return np.random.default_rng(np.random.SeedSequence([int(seed), int(k), 24091]))


def _box_blur(a, r):
    """Separable box blur of radius r (edge-clamped), twice (about a Gaussian of sigma r x 0.8)."""
    if r <= 0:
        return a
    for _ in range(2):
        for axis in (0, 1):
            pad = [(0, 0)] * a.ndim
            pad[axis] = (r + 1, r)
            p = np.pad(a, pad, mode="edge")
            c = np.cumsum(p, axis=axis, dtype=np.float64)
            hi = np.take(c, np.arange(2 * r + 1, c.shape[axis]), axis=axis)
            lo = np.take(c, np.arange(0, c.shape[axis] - 2 * r - 1), axis=axis)
            a = ((hi - lo) / (2 * r + 1)).astype(np.float32)
    return a


def _down2(a):
    """2 x 2 box downsample (even sizes)."""
    return 0.25 * (a[0::2, 0::2] + a[1::2, 0::2] + a[0::2, 1::2] + a[1::2, 1::2])


def _push_pull(premult, alpha, eps=1e-4):
    """Fills alpha-0 texels with the colour of their neighbourhood (push-pull). premult (H, W, C) = colour x alpha."""
    levels = [(premult, alpha)]
    p, a = premult, alpha
    while p.shape[0] > 1 and p.shape[1] > 1:
        p = _down2(p)
        a = _down2(a)
        levels.append((p, a))
    p, a = levels[-1]
    col = p / np.maximum(a, eps)[..., None]
    for p, a in reversed(levels[:-1]):
        up = np.repeat(np.repeat(col, 2, axis=0), 2, axis=1)
        w = np.clip(a / 0.5, 0.0, 1.0)[..., None]          # texels with half coverage or more keep their own colour
        own = p / np.maximum(a, eps)[..., None]
        col = w * own + (1.0 - w) * up
    return col.astype(np.float32)


def _warp(img, matrix, size):
    """PIL affine warp of a float32 (h, w) channel (output -> input matrix), bilinear, zero outside."""
    im = Image.fromarray(np.ascontiguousarray(img, dtype=np.float32), mode="F")
    out = im.transform(size, Image.AFFINE, data=matrix, resample=Image.BILINEAR, fillcolor=0.0)
    return np.asarray(out, dtype=np.float32)


# ---------------------------------------------------------------------------------------------------------------- source

class _Leaf:
    """One source leaf, cropped and pre-scaled: channels (h, w), anchor (petiole base) and tip in crop pixels."""

    def __init__(self, chans, anchor, length_px):
        self.chans = chans
        self.anchor = anchor
        self.length_px = length_px


def _extract_leaves(maps, grid, cells, pre_scale):
    op = maps["opacity"]
    H, W = op.shape
    gx, gy = grid
    cw, ch = W // gx, H // gy
    rough = maps.get("rough")
    height = maps.get("height")
    scat = maps.get("scattering")
    leaves = []
    for idx in cells:
        cx, cy = idx % gx, idx // gx
        cell = op[cy * ch:(cy + 1) * ch, cx * cw:(cx + 1) * cw]
        ys, xs = np.nonzero(cell > 0.5)
        if len(xs) < 64:
            continue
        m = 6
        x0, x1 = max(0, xs.min() - m), min(cw, xs.max() + m + 1)
        y0, y1 = max(0, ys.min() - m), min(ch, ys.max() + m + 1)
        sl = (slice(cy * ch + y0, cy * ch + y1), slice(cx * cw + x0, cx * cw + x1))
        crop = {
            "a": op[sl],
            "r": maps["albedo"][sl][..., 0], "g": maps["albedo"][sl][..., 1], "b": maps["albedo"][sl][..., 2],
            "nx": maps["normal"][sl][..., 0], "ny": maps["normal"][sl][..., 1],
            "rough": rough[sl] if rough is not None else np.full(op[sl].shape, 0.6, np.float32),
            "h": height[sl] if height is not None else np.full(op[sl].shape, 0.5, np.float32),
            "sc": scat[sl] if scat is not None else np.full(op[sl].shape, 0.8, np.float32),
        }
        # petiole base = the lowest opaque pixel (the source leaves point up, petiole down)
        ys2, xs2 = np.nonzero(crop["a"] > 0.5)
        bottom = ys2.max()
        ax = float(xs2[ys2 >= bottom - 3].mean())
        h0, w0 = crop["a"].shape
        nw, nh = max(4, int(round(w0 * pre_scale))), max(4, int(round(h0 * pre_scale)))
        chans = {}
        for k, v in crop.items():
            im = Image.fromarray(np.ascontiguousarray(v, dtype=np.float32), mode="F").resize((nw, nh), Image.LANCZOS)
            chans[k] = np.asarray(im, dtype=np.float32)
        # premultiply the colour-like channels by alpha before any further resampling (no dark fringes)
        a = np.clip(chans["a"], 0.0, 1.0)
        chans["a"] = a
        for k in ("r", "g", "b", "nx", "ny", "rough", "h", "sc"):
            chans[k] = chans[k] * a
        anchor = (ax * nw / w0, (bottom + 0.5) * nh / h0)
        length_px = (bottom - ys2.min()) * nh / h0
        leaves.append(_Leaf(chans, anchor, length_px))
    return leaves


# ---------------------------------------------------------------------------------------------------------------- layout

def _outline(rng, lobes_range, fill_r):
    lobes = int(rng.integers(lobes_range[0], lobes_range[1] + 1))
    phase = rng.uniform(0, 2 * math.pi)
    amp = rng.uniform(0.08, 0.14)

    def r_of(theta):
        return fill_r * (1.0 + amp * math.cos(lobes * theta + phase))
    return r_of


def _inside(p, r_of, edge):
    """Keep weight of a point p (sprite units, centre 0.5) inside the lobed outline with a soft edge band."""
    dx, dy = p[0] - 0.5, p[1] - 0.5
    r = math.hypot(dx, dy)
    lim = r_of(math.atan2(dy, dx))
    if r <= lim - edge:
        return 1.0
    if r >= lim:
        return 0.0
    return (lim - r) / edge


def _layout_sprite(rng, cfg, n_target, sprite_m):
    """Twigs (polylines with widths) and leaf placements (sprite units 0..1) of one sprite."""
    comp = cfg["compose"]
    r_of = _outline(rng, comp.get("lobes", [3, 5]), comp.get("fillRadius", 0.42))
    edge = comp.get("edgeFalloff", 0.10)
    m = 1.0 / sprite_m                                           # sprite units per metre
    tw0, tw1 = comp.get("twigWidthMetres", [0.006, 0.012])
    twigs, leaves = [], []
    sprays = int(comp.get("sprays", 3))
    internode = comp.get("internodeMetres", [0.045, 0.07])
    spread = math.radians(comp.get("leafAngleSpread", 70))

    def grow(p, ang, length, width, depth, layer, side_twigs):
        pts = [p]
        steps = max(2, int(length / (0.02 * m)) + 1)
        curl = rng.uniform(-0.6, 0.6) / max(length, 1e-3)       # total bend about 0.6 rad over the twig
        a = ang
        q = p
        seg = length / (steps - 1)
        for _ in range(steps - 1):
            a += curl * seg
            q = (q[0] + math.cos(a) * seg, q[1] + math.sin(a) * seg)
            pts.append(q)
        # leaves alternate along the twig
        s, side, last = rng.uniform(0.3, 1.0) * internode[0] * m, 1.0, 0
        cum = [0.0]
        for i in range(1, len(pts)):
            cum.append(cum[-1] + math.dist(pts[i - 1], pts[i]))
        total = cum[-1]
        while s < total:
            i = min(len(pts) - 2, int(np.searchsorted(cum, s) - 1))
            i = max(i, 0)
            f = (s - cum[i]) / max(cum[i + 1] - cum[i], 1e-6)
            base = (pts[i][0] + f * (pts[i + 1][0] - pts[i][0]), pts[i][1] + f * (pts[i + 1][1] - pts[i][1]))
            ta = math.atan2(pts[i + 1][1] - pts[i][1], pts[i + 1][0] - pts[i][0])
            keep = _inside(base, r_of, edge)
            if keep > 0 and rng.random() < keep:
                la = ta + side * rng.uniform(0.55, 1.0) * spread
                leaves.append({"pos": base, "ang": la, "layer": layer + rng.uniform(-0.04, 0.04)})
                last = i + 1
            side = -side
            s += rng.uniform(internode[0], internode[1]) * m
        # twig drawn only up to its last leaf (no bare twig ends in the air)
        if last > 0:
            twigs.append({"pts": pts[:last + 1], "w0": width, "w1": width * 0.45, "layer": layer - 0.03})
        if side_twigs > 0:
            k = 0
            s = rng.uniform(0.12, 0.2) * m
            sd = 1.0 if rng.random() < 0.5 else -1.0
            while s < total * 0.85 and k < side_twigs:
                i = max(0, min(len(pts) - 2, int(np.searchsorted(cum, s) - 1)))
                ta = math.atan2(pts[i + 1][1] - pts[i][1], pts[i + 1][0] - pts[i][0])
                sa = ta + sd * math.radians(rng.uniform(40, 65))
                sl = rng.uniform(0.15, 0.30) * (1.0 - 0.5 * s / total)   # 0.3-0.6 m, shorter toward the tip
                grow(pts[i], sa, max(sl, 0.06), width * 0.6, depth + 1, layer + rng.uniform(-0.06, 0.06), 0)
                sd = -sd
                k += 1
                s += rng.uniform(0.12, 0.2) * m

    for k in range(sprays):
        ang0 = rng.uniform(0, 2 * math.pi)
        rad = rng.uniform(0.28, 0.40)
        start = (0.5 + rad * math.cos(ang0), 0.5 + rad * math.sin(ang0))
        heading = ang0 + math.pi + rng.uniform(-0.5, 0.5)       # roughly toward and through the centre
        length = rng.uniform(0.55, 0.80)
        layer = (k + 0.5) / sprays + rng.uniform(-0.08, 0.08)
        grow(start, heading, length, rng.uniform(tw0, tw1) * m, 0, layer, int(rng.integers(4, 7)))

    # fill: extra leaves scattered inside the outline (short spurs) until the count target is met
    tries = 0
    while len(leaves) < n_target and tries < 20 * n_target:
        tries += 1
        p = (rng.uniform(0.05, 0.95), rng.uniform(0.05, 0.95))
        keep = _inside(p, r_of, edge)
        if keep <= 0 or rng.random() >= keep:
            continue
        leaves.append({"pos": p, "ang": rng.uniform(0, 2 * math.pi), "layer": rng.uniform(0.0, 1.0)})
    if len(leaves) > n_target:
        idx = rng.permutation(len(leaves))[:n_target]
        leaves = [leaves[i] for i in sorted(idx)]
    return twigs, leaves


# ---------------------------------------------------------------------------------------------------------------- compose

def compose(cfg_set, maps):
    comp = cfg_set["compose"]
    res = int(cfg_set.get("res", 2048))
    cols, rows = cfg_set.get("flipbook", [2, 2])
    sprites = int(cfg_set.get("sprites", cols * rows))
    sprite_m = float(cfg_set.get("spriteMetres", 2.0))
    src_m = float(cfg_set.get("sourceMetres", 0.40))
    grid = cfg_set.get("sourceGrid", [3, 3])
    cells = cfg_set.get("leaves", list(range(grid[0] * grid[1])))
    if not isinstance(cells, list):
        cells = list(range(grid[0] * grid[1]))
    mask_cells = cfg_set.get("maskLeaves", [])
    if isinstance(mask_cells, list):
        cells = [c for c in cells if c not in mask_cells]
    seed = int(comp.get("seed", 1))
    ss = 2                                                        # supersampling
    sp = res // cols                                              # sprite size (px) at the output resolution
    S = sp * ss                                                   # composition size
    px_per_m = S / sprite_m
    src_px_per_m = maps["opacity"].shape[1] / src_m
    scale_lo, scale_hi = comp.get("leafScale", [1.0, 1.3])
    pre = px_per_m / src_px_per_m * scale_hi                      # every instance minifies from here by <= 1.3x
    leaves_src = _extract_leaves(maps, grid, cells, pre)
    if not leaves_src:
        raise ValueError("foliage_compose: no leaves found in the source opacity")
    counts = comp.get("leavesPerSprite", [140, 160, 170, 190])
    ao_depth, ao_min = comp.get("aoDepth", 0.10), comp.get("aoMin", 0.55)
    hue_j, val_j = comp.get("hueJitter", 0.04), comp.get("valueJitter", 0.10)
    back_share = comp.get("backShare", 0.20)
    tilt_max = math.radians(comp.get("tiltMaxDeg", 60))
    border = int(comp.get("borderPx", 24)) * ss
    twig_col = np.array(comp.get("twigAlbedo", [0.050, 0.040, 0.032]), np.float32)   # linear, about the Bark token

    out = {k: np.zeros((res, res, 3) if k in ("albedo", "normal") else (res, res), np.float32)
           for k in ("albedo", "alpha", "normal", "ao", "height", "rough", "scatter")}
    stats = {"coverage": [], "leaves": [], "twigs": [], "leafMetres": [1e9, 0.0]}
    coarse = 8                                                    # AO grid factor
    blur_r = max(1, int(round(0.035 * px_per_m / coarse)))        # leaf shadow penumbra about 3.5 cm

    for k in range(sprites):
        rng = _rng(seed, k)
        twigs, places = _layout_sprite(rng, cfg_set, int(counts[k % len(counts)]), sprite_m)
        # accumulators (premultiplied, front to back): T transmittance, C coarse blurred cover in front
        T = np.ones((S, S), np.float32)
        acc = {c: np.zeros((S, S), np.float32) for c in ("r", "g", "b", "nx", "ny", "nz", "ao", "h", "rough", "sc")}
        C = np.zeros((S // coarse, S // coarse), np.float32)

        items = []
        for p in places:
            items.append(("leaf", p["layer"], p))
        for t in twigs:
            items.append(("twig", t["layer"], t))
        items.sort(key=lambda it: -it[1])                         # front (high layer) first

        n_leaf = 0
        for kind, layer, it in items:
            if kind == "leaf":
                src = leaves_src[int(rng.integers(0, len(leaves_src)))]
                scale = rng.uniform(scale_lo, scale_hi) / scale_hi
                tilt = tilt_max * math.sqrt(rng.random())
                fore_along = rng.random() < 0.5
                fa = math.cos(tilt) if fore_along else 1.0
                fw = 1.0 if fore_along else math.cos(tilt)
                back = rng.random() < back_share
                mirror = -1.0 if back else 1.0
                ang = it["ang"]
                d = (math.cos(ang), math.sin(ang))
                P = (it["pos"][0] * S, it["pos"][1] * S)
                Ax, Ay = src.anchor
                s = scale
                # bounding box of the transformed crop
                h0, w0 = src.chans["a"].shape
                corners = []
                for cx, cy in ((0, 0), (w0, 0), (0, h0), (w0, h0)):
                    ta = -(cy - Ay) * s * fa
                    tc = (cx - Ax) * mirror * s * fw
                    corners.append((P[0] + ta * d[0] - tc * d[1], P[1] + ta * d[1] + tc * d[0]))
                X0 = int(math.floor(min(c[0] for c in corners))) - 1
                Y0 = int(math.floor(min(c[1] for c in corners))) - 1
                X1 = int(math.ceil(max(c[0] for c in corners))) + 1
                Y1 = int(math.ceil(max(c[1] for c in corners))) + 1
                X0, Y0 = max(X0, border), max(Y0, border)
                X1, Y1 = min(X1, S - border), min(Y1, S - border)
                if X1 - X0 < 2 or Y1 - Y0 < 2:
                    continue
                Px, Py = P[0] - X0, P[1] - Y0
                a_ = -mirror * d[1] / (s * fw)
                b_ = mirror * d[0] / (s * fw)
                c_ = Ax + mirror * (d[1] * Px - d[0] * Py) / (s * fw)
                d_ = -d[0] / (s * fa)
                e_ = -d[1] / (s * fa)
                f_ = Ay + (d[0] * Px + d[1] * Py) / (s * fa)
                mat = (a_, b_, c_, d_, e_, f_)
                size = (X1 - X0, Y1 - Y0)
                w = {c: _warp(v, mat, size) for c, v in src.chans.items()}
                a = np.clip(w["a"], 0.0, 1.0)
                if a.max() <= 0.0:
                    continue
                inv = 1.0 / np.maximum(a, 1e-4)
                # colour: per-leaf hue / value jitter; the underside paler and less saturated
                vj = 1.0 + val_j * rng.uniform(-1, 1)
                hj = hue_j * rng.uniform(-1, 1)
                col = np.stack([w["r"] * inv, w["g"] * inv, w["b"] * inv], -1)
                col *= np.array([1.0 + hj, 1.0, 1.0 - 1.5 * hj], np.float32) * vj
                if back:
                    lum = (col @ np.array([0.2126, 0.7152, 0.0722], np.float32))[..., None]
                    col = (0.65 * col + 0.35 * lum) * 1.25
                # normal: rotate the texture xy into the card frame, add the leaf-plane tilt
                nx, ny = w["nx"] * inv, w["ny"] * inv
                along = (d[0], -d[1])                             # leaf up (source +v) in GL card space
                across = (-d[1] * mirror, -d[0] * mirror)         # source +u in GL card space
                tx = nx * across[0] + ny * along[0]
                ty = nx * across[1] + ny * along[1]
                tz = np.sqrt(np.clip(1.0 - nx * nx - ny * ny, 0.04, 1.0))
                sgn = 1.0 if rng.random() < 0.5 else -1.0
                if fore_along:
                    base = (sgn * math.sin(tilt) * along[0], sgn * math.sin(tilt) * along[1], math.cos(tilt))
                else:
                    ax_ = (across[0] * mirror, across[1] * mirror)
                    base = (sgn * math.sin(tilt) * ax_[0], sgn * math.sin(tilt) * ax_[1], math.cos(tilt))
                gx_ = base[0] * tz + tx
                gy_ = base[1] * tz + ty
                gz_ = base[2] * tz
                nl = 1.0 / np.sqrt(gx_ * gx_ + gy_ * gy_ + gz_ * gz_ + 1e-8)
                # AO from the blurred cover of everything in front
                cy0, cy1 = Y0 // coarse, (Y1 + coarse - 1) // coarse
                cx0, cx1 = X0 // coarse, (X1 + coarse - 1) // coarse
                Cw = np.repeat(np.repeat(C[cy0:cy1, cx0:cx1], coarse, 0), coarse, 1)
                Cw = Cw[Y0 - cy0 * coarse:Y0 - cy0 * coarse + size[1], X0 - cx0 * coarse:X0 - cx0 * coarse + size[0]]
                ao = np.maximum(ao_min, 1.0 - ao_depth * Cw)
                hl = np.clip(layer, 0.0, 1.0)
                hh = 0.85 * hl + 0.15 * (w["h"] * inv)
                sc = w["sc"] * inv * (0.85 if back else 1.0)
                win = (slice(Y0, Y1), slice(X0, X1))
                wt = T[win] * a
                acc["r"][win] += wt * col[..., 0]
                acc["g"][win] += wt * col[..., 1]
                acc["b"][win] += wt * col[..., 2]
                acc["nx"][win] += wt * gx_ * nl
                acc["ny"][win] += wt * gy_ * nl
                acc["nz"][win] += wt * gz_ * nl
                acc["ao"][win] += wt * ao
                acc["h"][win] += wt * hh
                acc["rough"][win] += wt * (w["rough"] * inv)
                acc["sc"][win] += wt * sc
                T[win] *= (1.0 - a)
                # this leaf now shades what lies behind it
                pad = blur_r * 2 + 1
                gy0, gy1 = max(0, cy0 - pad), min(C.shape[0], cy1 + pad)
                gx0, gx1 = max(0, cx0 - pad), min(C.shape[1], cx1 + pad)
                fp = np.zeros((gy1 - gy0, gx1 - gx0), np.float32)
                full = np.zeros(((gy1 - gy0) * coarse, (gx1 - gx0) * coarse), np.float32)
                oy, ox = Y0 - gy0 * coarse, X0 - gx0 * coarse
                full[oy:oy + size[1], ox:ox + size[0]] = a
                fp = full.reshape(gy1 - gy0, coarse, gx1 - gx0, coarse).mean(axis=(1, 3))
                C[gy0:gy1, gx0:gx1] += _box_blur(fp, blur_r)
                n_leaf += 1
                lm = src.length_px * s / px_per_m
                stats["leafMetres"][0] = min(stats["leafMetres"][0], lm)
                stats["leafMetres"][1] = max(stats["leafMetres"][1], lm)
            else:
                _draw_twig(it, S, border, T, acc, twig_col, C, coarse)

        alpha = 1.0 - T
        # downsample the premultiplied accumulators 2x, then dilate and un-premultiply
        a2 = _down2(alpha)
        pre_col = np.stack([_down2(acc["r"]), _down2(acc["g"]), _down2(acc["b"])], -1)
        pre_n = np.stack([_down2(acc["nx"]), _down2(acc["ny"]), _down2(acc["nz"])], -1)
        pre_m = np.stack([_down2(acc["ao"]), _down2(acc["h"]), _down2(acc["rough"]), _down2(acc["sc"])], -1)
        col = _push_pull(pre_col, a2)
        nrm = _push_pull(pre_n, a2)
        nrm[..., 2] = np.maximum(nrm[..., 2], 0.2)
        nrm /= np.linalg.norm(nrm, axis=-1, keepdims=True)
        msk = _push_pull(pre_m, a2)
        ox, oy = (k % cols) * sp, (k // cols) * sp
        win = (slice(oy, oy + sp), slice(ox, ox + sp))
        out["albedo"][win] = col
        out["alpha"][win] = a2
        out["normal"][win] = nrm
        out["ao"][win] = np.clip(msk[..., 0], ao_min, 1.0)
        out["height"][win] = np.clip(msk[..., 1], 0.0, 1.0)
        out["rough"][win] = np.clip(msk[..., 2], 0.0, 1.0)
        out["scatter"][win] = np.clip(msk[..., 3], 0.0, 1.0)
        stats["coverage"].append(float(a2.mean()))
        stats["leaves"].append(n_leaf)
        stats["twigs"].append(len(twigs))
    out["stats"] = stats
    return out


def _draw_twig(t, S, border, T, acc, col, C, coarse):
    """Anti-aliased tapered polyline (capsule segments) composited under what is already in front."""
    pts = [(p[0] * S, p[1] * S) for p in t["pts"]]
    n = len(pts)
    for i in range(n - 1):
        (x0, y0), (x1, y1) = pts[i], pts[i + 1]
        r = 0.5 * S * (t["w0"] + (t["w1"] - t["w0"]) * (i + 0.5) / max(n - 1, 1)) / 1.0
        r = max(r, 0.6)
        bx0, by0 = int(max(border, math.floor(min(x0, x1) - r - 1))), int(max(border, math.floor(min(y0, y1) - r - 1)))
        bx1, by1 = int(min(S - border, math.ceil(max(x0, x1) + r + 1))), int(min(S - border, math.ceil(max(y0, y1) + r + 1)))
        if bx1 <= bx0 or by1 <= by0:
            continue
        yy, xx = np.mgrid[by0:by1, bx0:bx1].astype(np.float32) + 0.5
        dx, dy = x1 - x0, y1 - y0
        ll = dx * dx + dy * dy
        u = np.clip(((xx - x0) * dx + (yy - y0) * dy) / max(ll, 1e-6), 0.0, 1.0)
        px, py = x0 + u * dx - xx, y0 + u * dy - yy
        dist = np.sqrt(px * px + py * py)
        a = np.clip(r - dist + 0.5, 0.0, 1.0)
        win = (slice(by0, by1), slice(bx0, bx1))
        # cylinder normal across the twig
        nd = np.clip((px * (-dy) + py * dx) / (math.sqrt(ll) * r + 1e-6), -0.95, 0.95)
        perp = (-dy / math.sqrt(ll + 1e-12), dx / math.sqrt(ll + 1e-12))
        nxv, nyv = -nd * perp[0], nd * perp[1]
        nzv = np.sqrt(np.clip(1.0 - nd * nd, 0.0, 1.0))
        wt = T[win] * a
        acc["r"][win] += wt * col[0]
        acc["g"][win] += wt * col[1]
        acc["b"][win] += wt * col[2]
        acc["nx"][win] += wt * nxv
        acc["ny"][win] += wt * nyv
        acc["nz"][win] += wt * nzv
        acc["ao"][win] += wt * 0.8
        acc["h"][win] += wt * float(np.clip(t["layer"], 0.0, 1.0)) * 0.85
        acc["rough"][win] += wt * 0.8
        acc["sc"][win] += 0.0
        T[win] *= (1.0 - a)
