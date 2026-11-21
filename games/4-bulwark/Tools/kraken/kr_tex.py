"""Kraken textures: graded albedo (creepier: deeper, wetter, sickly), tangent-space normal, packed mask.

Grading: every source texel is classified softly by hue (red undersides / suckers, green algae, teal-blue skin, pale
rims); each class is re-targeted to a BulwarkPalette Kraken* token (parsed from BulwarkPalette.cs: the palette stays the
only place a colour is typed) while the source's relative luminance detail is kept with a contrast exponent.
Mask (linear RGBA): R = ambient occlusion, G = smoothness (wet slime), B = cavity (0.5 neutral, < 0.5 crevice),
A = socket glow (emission weight near the eyes).
"""
import os, re, sys
sys.path.insert(0, os.path.dirname(__file__))
import numpy as np
from scipy.ndimage import gaussian_filter
from kr_common import *

PALETTE_CS = os.path.join(ROOT, "Assets", "_Game", "Scripts", "Runtime", "Core", "BulwarkPalette.cs")


def palette():
    txt = open(PALETTE_CS, encoding="utf-8-sig").read()
    out = {}
    for name, hx in re.findall(r'\(Pal\.(\w+),\s*"(#[0-9A-F]{6})"\)', txt):
        out[name] = srgb_to_lin(np.array([int(hx[i:i + 2], 16) / 255.0 for i in (1, 3, 5)]))
    return out


def _hsv(c):
    mx = c.max(-1); mn = c.min(-1); d = mx - mn
    h = np.zeros_like(mx)
    r, g, b = c[..., 0], c[..., 1], c[..., 2]
    m = d > 1e-6
    hr = (mx == r) & m; hg = (mx == g) & m & ~hr; hb = m & ~hr & ~hg
    h[hr] = ((g - b)[hr] / d[hr]) % 6
    h[hg] = (b - r)[hg] / d[hg] + 2
    h[hb] = (r - g)[hb] / d[hb] + 4
    return h * 60.0, np.where(mx > 1e-6, d / np.maximum(mx, 1e-6), 0), mx


def hue_w(h, centre, width):
    d = np.abs(((h - centre) + 180) % 360 - 180)
    return np.clip(1 - (d / width) ** 2, 0, 1)


def classes(srgb):
    h, s, v = _hsv(srgb)
    red = hue_w(h, 8, 34) * np.clip((s - 0.25) / 0.25, 0, 1)
    green = hue_w(h, 95, 55) * np.clip((s - 0.18) / 0.2, 0, 1)
    teal = hue_w(h, 205, 60) * np.clip((s - 0.08) / 0.2, 0, 1)
    pale = np.clip((v - 0.55) / 0.2, 0, 1) * np.clip((0.42 - s) / 0.2, 0, 1)
    rest = np.clip(1 - (red + green + teal + pale), 0, 1)
    W = np.stack([red, green, teal + rest, pale], -1)
    W /= np.maximum(W.sum(-1, keepdims=True), 1e-6)
    return W


def grade(srgb, pal, contrast=(1.35, 1.25, 1.45, 1.1)):
    """srgb (H,W,3) 0..1 -> graded LINEAR albedo."""
    lin = srgb_to_lin(srgb)
    lum = lin @ np.array([0.2126, 0.7152, 0.0722])
    Wc = classes(srgb)
    targets = [pal["KrakenUnderRed"], pal["KrakenMoss"], pal["KrakenSkinDark"], pal["KrakenSucker"]]
    out = np.zeros_like(lin)
    for k in range(4):
        wk = Wc[..., k]
        sel = wk > 0.5
        ref = np.median(lum[sel]) if sel.sum() > 100 else np.median(lum)
        rel = np.clip(lum / max(ref, 1e-4), 0, 6) ** contrast[k]
        out += (wk * rel)[..., None] * targets[k][None, None, :] * 1.0
    # keep a little of the source chroma (organic variation), slightly desaturated
    src_rel = lin / np.maximum(lum[..., None], 1e-4)
    out = out * (0.82 + 0.18 * np.clip(src_rel, 0, 3) / np.maximum(src_rel.mean(-1, keepdims=True), 1e-3))
    return np.clip(out, 0.004, 0.75)


def bandpass_height(lin_lum, s0=1.2, s1=10.0):
    a = gaussian_filter(lin_lum, s0); b = gaussian_filter(lin_lum, s1)
    hp = a - b
    return hp / max(np.percentile(np.abs(hp), 99), 1e-4)


def height_normal(h, strength):
    """Tangent-space normal from a texel-space height field (x = +u, texel rows go -v)."""
    gx = (np.roll(h, -1, 1) - np.roll(h, 1, 1)) * 0.5
    gy = (np.roll(h, -1, 0) - np.roll(h, 1, 0)) * 0.5
    n = np.stack([-strength * gx, strength * gy, np.ones_like(h)], -1)
    return n / np.linalg.norm(n, axis=-1, keepdims=True)


def combine_normals(a, b):
    """Partial-derivative blend of two tangent-space normals."""
    x = a[..., 0] / np.maximum(a[..., 2], 0.05) + b[..., 0] / np.maximum(b[..., 2], 0.05)
    y = a[..., 1] / np.maximum(a[..., 2], 0.05) + b[..., 1] / np.maximum(b[..., 2], 0.05)
    n = np.stack([x, y, np.ones_like(x)], -1)
    return n / np.linalg.norm(n, axis=-1, keepdims=True)


def smoothness_from(Wc, hgt, rng_seed=3):
    base = Wc[..., 0] * 0.62 + Wc[..., 1] * 0.40 + Wc[..., 2] * 0.70 + Wc[..., 3] * 0.52
    rng = np.random.default_rng(rng_seed)
    noise = gaussian_filter(rng.normal(size=Wc.shape[:2]), 24)
    noise /= max(np.abs(noise).max(), 1e-6)
    return np.clip(base - 0.10 * np.clip(hgt, -1, 1) + 0.08 * noise, 0.2, 0.86)


def save_rgb(path, rgb01):
    from PIL import Image
    Image.fromarray((np.clip(rgb01, 0, 1) * 255 + 0.5).astype(np.uint8), "RGB").save(path, optimize=True)


def save_rgba(path, rgba01):
    from PIL import Image
    Image.fromarray((np.clip(rgba01, 0, 1) * 255 + 0.5).astype(np.uint8), "RGBA").save(path, optimize=True)
