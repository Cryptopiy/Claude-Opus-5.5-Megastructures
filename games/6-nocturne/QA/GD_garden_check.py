"""GD_garden_check: the beat-5 picture numbers of MASTER_PROMPT 14.5.5 acceptance (2) and LOOK_V2 8.1 / 2.4 for the garden.
Usage: python -I QA/GD_garden_check.py <png> [<png> ...]
Per image (display codes, Rec.709 luma on the sRGB values):
  mean luma (KS15: 24-64 in 14.5.5, 24-42 in LOOK_V2 8.1), crush (share of pixels with luma < 12),
  green share: pixels with G > R + 15 and G > B (14.5.5: KS15 >= 55 % foliage or branch pixels),
  lantern blobs: 8-connected components of luma >= 150 whose peak is > 200 and whose bounding box is 3-12 px wide
  (14.5.5: KS15 >= 25), and the 3x3 block-mean saturation of the green / warm foliage (LOOK_V2 R2 / R3 indication).
"""
import sys
import numpy as np
from PIL import Image


def components(mask):
    """8-connected components of a boolean mask (BFS over the set pixels; the masks here are small)."""
    pts = set(zip(*np.nonzero(mask)))
    out = []
    while pts:
        seed = pts.pop()
        comp = [seed]
        stack = [seed]
        while stack:
            cy, cx = stack.pop()
            for dy in (-1, 0, 1):
                for dx in (-1, 0, 1):
                    q = (cy + dy, cx + dx)
                    if q in pts:
                        pts.remove(q)
                        comp.append(q)
                        stack.append(q)
        out.append(comp)
    return out


def check(path):
    im = np.asarray(Image.open(path).convert("RGB")).astype(np.float32)
    r, g, b = im[..., 0], im[..., 1], im[..., 2]
    y = 0.2126 * r + 0.7152 * g + 0.0722 * b
    green = (g > r + 15) & (g > b)
    blobs = 0
    sizes = []
    for comp in components(y >= 150):
        ys = [q[0] for q in comp]
        xs = [q[1] for q in comp]
        d = max(max(ys) - min(ys) + 1, max(xs) - min(xs) + 1)
        peak = max(y[q] for q in comp)
        if peak > 200 and 3 <= d <= 12:
            blobs += 1
            sizes.append(d)
    mx = np.maximum(np.maximum(r, g), b)
    mn = np.minimum(np.minimum(r, g), b)
    sat = np.where(mx > 0, (mx - mn) / np.maximum(mx, 1), 0)
    mid = (y >= 16) & (y < 160)
    out = {
        "mean": float(y.mean()), "p50": float(np.percentile(y, 50)), "p99": float(np.percentile(y, 99)),
        "crush<12": float((y < 12).mean()), "green": float(green.mean()),
        "blobs": blobs, "blob_px_med": float(np.median(sizes)) if sizes else 0.0,
        "sat_mid_mean": float(sat[mid].mean()) if mid.any() else 0.0,
        "green_luma_med": float(np.median(y[green])) if green.any() else 0.0,
    }
    return out


if __name__ == "__main__":
    for p in sys.argv[1:]:
        o = check(p)
        print(p.replace("\\", "/").split("/")[-1], " ".join("%s=%.3f" % (k, v) if isinstance(v, float) else "%s=%d" % (k, v) for k, v in o.items()))
