"""GD_lantern_sim (GD fix wave, C32): how a small emissive disc resolves under MSAA 4x, and whether its centre pixel is an
isolated maximum in image_metrics' sense (display luma more than 0.30 x 255 = 77 codes above ALL 24 neighbours of its 5 x 5).
A disc of diameter D px is placed at 12 x 12 sub-pixel offsets; each pixel's coverage = the share of the 4 D3D standard MSAA
sample positions inside the disc; the shading is evaluated once per pixel at the pixel centre (as MSAA does): flat 1, or the
lantern's hot profile a + b (n.v)^2 with (n.v)^2 = 1 - rho^2 (disc mean a + b / 2 = 1); display = 255 x lin^(1/2.2) with a fully
covered flat pixel at `peak` codes.  Prints per D: the worst centre lead (codes) and the std of the frame peak over the offsets
(twinkle as the disc crosses pixel centres).
Usage: python -I QA/GD_lantern_sim.py
"""
import numpy as np

S = np.array([(-2, -6), (6, -2), (-6, 2), (2, 6)]) / 16.0


def render(D, ox, oy, prof):
    R = D / 2.0
    img = np.zeros((15, 15))
    for y in range(15):
        for x in range(15):
            px, py = x - 7 + 0.5, y - 7 + 0.5
            cov = sum(0.25 for sx, sy in S if (px + sx - 0.5 - ox) ** 2 + (py + sy - 0.5 - oy) ** 2 <= R * R)
            if cov > 0:
                r2 = min(1.0, ((px - 0.5 - ox) ** 2 + (py - 0.5 - oy) ** 2) / (R * R))
                img[y, x] = cov * prof(r2)
    return img


def worst(D, prof, peak=205, n=12):
    lin = (peak / 255.0) ** 2.2
    lead, peaks = -1e9, []
    for ox in np.linspace(-0.5, 0.5, n, endpoint=False):
        for oy in np.linspace(-0.5, 0.5, n, endpoint=False):
            d = 255 * np.clip(render(D, ox, oy, prof) * lin, 0, 1) ** (1 / 2.2)
            y, x = np.unravel_index(np.argmax(d), d.shape)
            nb = d[y - 2:y + 3, x - 2:x + 3].copy()
            nb[2, 2] = -1
            lead = max(lead, d[y, x] - nb.max())
            peaks.append(d[y, x])
    return lead, float(np.std(peaks))


if __name__ == "__main__":
    flat = lambda r2: 1.0
    hot_old = lambda r2: 0.55 + 0.9 * (1 - r2)

    def ss(a, b, x):
        t = min(max((x - a) / (b - a), 0.0), 1.0)
        return t * t * (3 - 2 * t)

    for D in (1.75, 2.2, 2.6, 3.0, 4.5, 6.0):
        w = ss(1.5, 4.5, D)                       # the shader's blend at _FlatPx 3 (flat below 1.5 px, hot from 4.5 px)
        new = lambda r2, w=w: (1 - w) + w * (0.35 + 1.3 * (1 - r2))
        for peak in (205, 255):
            fl, fs = worst(D, flat, peak)
            hl, hs = worst(D, hot_old, peak)
            nl, ns = worst(D, new, peak)
            print("D %.2f px peak %d: flat lead %5.1f std %4.1f | old hot 0.55+0.9 lead %5.1f std %4.1f | fix-wave blend (w %.2f, 0.35+1.3) lead %5.1f std %4.1f"
                  % (D, peak, fl, fs, hl, hs, w, nl, ns))
