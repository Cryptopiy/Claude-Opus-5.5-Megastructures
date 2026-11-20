"""texprep: the Look v2 texture pipeline (docs/TEXTURE_PLAN.md 1.4; owner TP). Run with python -I.

Turns the TEX-R final sets (docs/TEXTURES.md "Final sets", raw files under ExternalTextures/raw/) into processed maps under
Assets/_Game/Textures/External/ plus one <Key>.texmeta.json per set or array (read by Scripts/Editor/NocTextureImport.cs, MaterialFactory
.ExtMaps and TextureTests).

    python -I Tools/texprep.py [--only <Key>[,<Key>...]] [--check] [--previews] [--placeholders] [--force] [--list]

  --only K      process only these keys (configs are still all validated)
  --check       write nothing; exit 1 if any output is missing or stale against its inputs (raw hashes, its config entry, the
                texprep / foliage_compose versions) or was edited after it was written
  --previews    also write ExternalTextures/previews/<Key>*.jpg (3 x 3 tiling, 1:1 crop, wrap-seam crop, lit) and contact.jpg
  --placeholders  write 64 px neutral sets for every key that has no processed output yet (materials build before TP-1)
  --force       re-process even when the outputs are up to date
  --list        print the keys of every config and exit

Configs (one per owning package, never shared; TEXTURE_PLAN 1.4): Tools/texprep/tp_common.json (TP: OverlayArr, MicroArr),
facade.json (FA: FacadeArr, RoomArr), surface.json (SU: near sets), foliage.json (FO: leaf-cluster atlases). Each holds
"owner" and "sets" {Key: unit} and/or "arrays" {Key: {"res", "flipbook": [cols, rows], "maps": [...], "aniso", "kind",
"slices": [unit, ...]}}. A unit (a set, or one slice of an array) takes:
  source        raw folder name (ExternalTextures/raw/<source>); must be a TEX-R final set, else the run aborts
  res           output size (sets; arrays use the array's res for every slice)
  tile          metres per tile (u); tileV for an anisotropic tile; tileRef comes from TEXTURES.md
  class         contrast / chroma class: concrete plaster brick asphalt granite paint steel bark leaves moss wood tile fabric
  token         palette token for the chroma judgement (default by class, read from Shaders/Generated/NocPalette.hlsl)
  contrast      luminance std / mean of the albedo variation (LOOK_V2 5.1 class midpoint, 1.4 step 5)
  satMax        cap of the mean HSV saturation of token x 2 x variation (sRGB); chromaKeep (default 0.8) is applied first
  smooth        [lo, hi]: p5..p95 of 1 - roughness mapped to this dry range (mask A)
  normalStrength  baked into the normal (default by class, LOOK_V2 5.1)
  antiTiling    hex | hexOffsets | heightHex | module | panel | offsets | none (recorded; the shaders pick the method)
  rotate, metalFromMap (honoured only for the four allow-listed sets of 1.4 step 6), mapping ("uv0"), rows, use
  module        [mu, mv] metres (null = measured), bond stack|running|half, longAxis "v" (half bond along v), register
                "ao"|"height" (lattice registration: whole modules per tile, module boundaries in the middle of the joints),
                measureLength (measure mu), toneJitter (recorded)
  register (facade slices)  {"axis": "u"|"v"|"uv", "map": "AO"|"Displacement", "per": m | [mu, mv], "count": n | [nu, nv] |
                "nearest ...", "sourcePitch": m, "bond": "running"}: the measured period is re-scaled so a whole number of
                periods fills `per` (a floor or a bay); the scale change must stay <= 7 %; a one-axis registration scales the
                other axis by the same factor (isotropic)
  bakeFit       step 4a (only sets with a measured bake above 0.10); flatten (step 4c strength <= 0.6, plastered_wall_04 only)
  seamless      step 3 half-offset with a 12 % feathered cross-fade (only for sources failing the seam check);
  seamAccept    "real": a seam score above 1.25 is a real joint (TEX-R)
  square        "pad" | "resample" (non-square sources); padSide top|bottom; squareResample true = "resample"
  opacity       true: the albedo alpha is the source opacity (grating); heightFromNormal: Poisson-integrated height
  maps          {role: map name} overrides (roles albedo normal ao rough height metal opacity scattering)
  ignore        map names never read
  overlay slices (kind "overlay"): coverage opacity|1-metal|smoothRough; micro slices (kind "micro"): crop (m), contrast,
                avoidJoints
  foliage sets (a "compose" block): composed by Tools/texprep/foliage_compose.py (FO) into a 2 x 2 flipbook of cluster sprites,
                then steps 5, 6, 9, 10 (contrast and chroma over alpha > 0.5 texels; mask B = scattering with
                compose.scatteringToMaskB)

Steps per unit (1.4): 1 inputs and safety (raw-only real paths, .png / .jpg, PIL PNG / JPEG, MAX_IMAGE_PIXELS 8192^2, SHA-256 of
every input), 2 decode to float32 linear (normal sign check against the displacement: corr(n.x, -dh/du) > 0, else abort), 3
square / register / seamless (periodic Lanczos-3 resampling in linear light, normals as vectors), 4 baked light (4a bake fit,
4b ramp guard: measured and reported, never subtracted from a periodic tile, 4c repeat flattening, AO division
lerp(1, AO, 0.5), content gate lf >= 0.6 lf_before unless 4c), 5 colour (absolute mean recorded, per-channel mean 0.5,
contrast to the class midpoint inside [0.01, 0.99], chroma cap, violet desaturation, violet share <= 0.5 %), 6 mask pack (R
metallic, G AO, B height p1..p99, A smoothness), 7 leaves, 8 micro, 9 arrays (flipbook slices left to right, top to bottom), 10
deterministic PNGs (optimize=True), texmeta, previews. Nothing from raw is ever imported or executed.
"""
import argparse
import hashlib
import importlib.util
import json
import math
import os
import re
import sys

import numpy as np
from PIL import Image

VERSION = 4
Image.MAX_IMAGE_PIXELS = 8192 * 8192

ROOT = os.path.abspath(os.path.join(os.path.dirname(os.path.abspath(__file__)), ".."))
RAW = os.path.join(ROOT, "ExternalTextures", "raw")
OUT = os.path.join(ROOT, "Assets", "_Game", "Textures", "External")
PREV = os.path.join(ROOT, "ExternalTextures", "previews")
CFG_DIR = os.path.join(ROOT, "Tools", "texprep")
CONFIGS = ["tp_common.json", "facade.json", "surface.json", "foliage.json"]
TEXTURES_MD = os.path.join(ROOT, "docs", "TEXTURES.md")
PALETTE_HLSL = os.path.join(ROOT, "Assets", "_Game", "Shaders", "Generated", "NocPalette.hlsl")
ASSET_PREFIX = "Assets/_Game/Textures/External/"

METAL_ALLOW = ("ambientcg_MetalPlates013", "ambientcg_MetalWalkway006", "polyhaven_corrugated_iron_02", "ambientcg_DiamondPlate008C")
# seam scores above 1.25 that TEX-R checked and accepted: a real joint on the tile edge (OfficeCeiling001 2.16, plank_flooring_04
# 1.44, concrete_floor_worn_001 1.34 inside the interior column-difference p99), and fine_grained_wood (1.48: "no seam shows across
# the wrap even at 2.2x gain"), large_floor_tiles_02 (TEX: the tile edge falls on a real joint, confirmed visually)
REAL_SEAMS = ("ambientcg_OfficeCeiling001", "polyhaven_plank_flooring_04", "polyhaven_concrete_floor_worn_001", "polyhaven_fine_grained_wood",
              "polyhaven_large_floor_tiles_02", "ambientcg_Asphalt020L")
# (Asphalt020L 1.30: no seam visible across the wrap at 1.6x gain, 2026-11-15; the ratio comes from its 1 px aggregate noise)
FLATTEN_ALLOW = ("polyhaven_plastered_wall_04",)
BAKE_FIT_ALLOW = ("polyhaven_concrete_wall_006", "polyhaven_asphalt_02")
# Source-level corrections found by the step-2 sign check (applied to every use of the source, re-checked after the fix):
# concrete_floor_worn_001's nor_gl has its red channel inverted: corr(n.x, -dh/du) = -0.689 while corr(n.y, -dh/dv) = +0.813
# (every other final set: both positive, 0.35-1.00; measured 2026-11-15 with Tools/texprep.py's sign_check at blur 1.5 px).
SOURCE_FIX = {"polyhaven_concrete_floor_worn_001": {"flipNormalX": True}}

ROLE_PATTERNS = {
    "albedo": ["_Color.", "_diff_", "_diffuse_", "_col_"],
    "normal": ["_NormalGL.", "_nor_gl_"],
    "ao": ["_AmbientOcclusion.", "_ao_"],
    "rough": ["_Roughness.", "_rough_"],
    "height": ["_Displacement.", "_disp_", "_displacement_"],
    "metal": ["_Metalness.", "_metal_"],
    "opacity": ["_Opacity."],
    "scattering": ["_Scattering."],
}
MAP_ALIASES = {"Color": "albedo", "NormalGL": "normal", "AmbientOcclusion": "ao", "AO": "ao", "Roughness": "rough",
               "Displacement": "height", "Metalness": "metal", "Opacity": "opacity", "Scattering": "scattering"}

CLASS_TOKEN = {"concrete": "ConcreteDark", "plaster": "Plaster", "brick": "Brick", "asphalt": "WetAsphalt", "granite": "Granite",
               "paint": "MetalPaint", "steel": "SteelF0", "bark": "Bark", "leaves": "Leaf", "moss": "Moss", "wood": "Bark",
               "tile": "Plaster", "fabric": "Plaster"}
CLASS_NORMAL = {"concrete": 1.15, "plaster": 0.8, "brick": 1.35, "asphalt": 0.6, "granite": 0.5, "paint": 0.8, "steel": 0.7,
                "bark": 1.4, "leaves": 0.8, "moss": 1.2, "wood": 0.8, "tile": 0.6, "fabric": 0.8}
CLASS_SAT = {"brick": 0.45, "bark": 0.45, "leaves": 0.45, "moss": 0.45, "wood": 0.45, "fabric": 0.45}
LUM = np.array([0.2126, 0.7152, 0.0722], np.float64)


class TexprepError(Exception):
    pass


def log(msg):
    print("[texprep] " + msg, flush=True)


# ================================================================================================================ inputs

def final_sets():
    """{raw folder: tile metres} of the TEX-R "Final sets" table (docs/TEXTURES.md, the authority)."""
    txt = open(TEXTURES_MD, encoding="utf-8").read()
    i = txt.find("### Final sets")
    if i < 0:
        raise TexprepError("docs/TEXTURES.md has no 'Final sets' section")
    out = {}
    for line in txt[i:].splitlines()[1:]:
        if line.startswith("**Not to be processed"):
            break
        m = re.match(r"\|\s*[^|]*\|\s*([A-Za-z0-9_]+)\s*\((PH|aCG)\)\s*\|\s*([^|]*)\|\s*([^|]*)\|", line)
        if not m:
            continue
        sid, src, _status, tile = m.group(1), m.group(2), m.group(3), m.group(4)
        folder = ("polyhaven_" if src == "PH" else "ambientcg_") + sid
        t = re.match(r"\s*([0-9.]+)", tile)
        out[folder] = float(t.group(1)) if t else 0.0
    if len(out) < 20:
        raise TexprepError("TEXTURES.md 'Final sets' parsed only %d sets" % len(out))
    return out


def palette():
    toks = {}
    for line in open(PALETTE_HLSL, encoding="utf-8"):
        m = re.match(r"#define PAL_(\w+) float3\(([^)]*)\)", line)
        if m:
            toks[m.group(1)] = np.array([float(x) for x in m.group(2).split(",")], np.float64)
    return toks


def sha256(path):
    h = hashlib.sha256()
    with open(path, "rb") as f:
        for b in iter(lambda: f.read(1 << 20), b""):
            h.update(b)
    return h.hexdigest()


def safe_raw_file(path):
    real = os.path.realpath(path)
    raw = os.path.realpath(RAW)
    if os.path.commonpath([real, raw]) != raw:
        raise TexprepError("refusing a file outside ExternalTextures/raw: " + path)
    if not real.lower().endswith((".png", ".jpg", ".jpeg")):
        raise TexprepError("refusing a non-image file: " + path)
    return real


def find_maps(source, unit):
    d = os.path.join(RAW, source)
    if not os.path.isdir(d):
        raise TexprepError("raw set missing: " + d)
    files = sorted(os.listdir(d))
    ignore = [("_" + x + ".") for x in unit.get("ignore", [])]
    override = {}
    for role, name in (unit.get("maps") or {}).items() if isinstance(unit.get("maps"), dict) else []:
        r = MAP_ALIASES.get(role, role)
        override[r] = name
    out = {}
    for role, pats in ROLE_PATTERNS.items():
        if role in override:
            pats = ["_" + override[role] + "."] + ["_" + override[role] + "_"]
        for f in files:
            if any(p in f for p in ignore):
                continue
            if any(p in f for p in pats) and f.lower().endswith((".jpg", ".jpeg", ".png")):
                out[role] = safe_raw_file(os.path.join(d, f))
                break
    return out


def load(path, grey):
    im = Image.open(path)
    if im.format not in ("PNG", "JPEG"):
        raise TexprepError("not a PNG / JPEG: %s (%s)" % (path, im.format))
    im.load()
    if im.mode in ("I;16", "I;16B", "I;16L", "I"):
        a = np.asarray(im, dtype=np.float64) / 65535.0
        a = a[..., None] if a.ndim == 2 else a
    else:
        if im.mode not in ("L", "RGB", "RGBA", "LA"):
            im = im.convert("RGB")
        a = np.asarray(im, dtype=np.float64) / 255.0
        a = a[..., None] if a.ndim == 2 else a
    if grey:
        a = a[..., :3].mean(axis=2) if a.shape[2] >= 3 else a[..., 0]
        return a.astype(np.float32)
    if a.shape[2] == 1:
        a = np.repeat(a, 3, axis=2)
    return a[..., :3].astype(np.float32)


def s2l(a):
    a = np.clip(a, 0, 1)
    return np.where(a <= 0.04045, a / 12.92, ((a + 0.055) / 1.055) ** 2.4)


def l2s(a):
    a = np.clip(a, 0, 1)
    return np.where(a <= 0.0031308, a * 12.92, 1.055 * np.power(a, 1 / 2.4) - 0.055)


# ================================================================================================================ signal tools

def gblur(x, sigma):
    """Periodic Gaussian blur (FFT) of a 2D array, sigma in px."""
    if sigma <= 0:
        return x
    h, w = x.shape
    fy = np.fft.fftfreq(h)[:, None]
    fx = np.fft.rfftfreq(w)[None, :]
    g = np.exp(-2 * (np.pi ** 2) * (sigma ** 2) * (fx ** 2 + fy ** 2))
    return np.fft.irfft2(np.fft.rfft2(x) * g, s=(h, w)).astype(np.float32)


def lanczos_matrix(n_in, n_out, periodic=True):
    s = n_out / float(n_in)
    scale = min(1.0, s)
    a = 3.0
    support = a / scale
    W = np.zeros((n_out, n_in), np.float64)
    for j in range(n_out):
        x = (j + 0.5) / s - 0.5
        lo = int(math.floor(x - support)) + 1
        hi = int(math.floor(x + support))
        idx = np.arange(lo, hi + 1)
        t = (idx - x) * scale
        w = np.sinc(t) * np.sinc(t / a)
        w[np.abs(t) >= a] = 0.0
        w /= w.sum()
        if periodic:
            np.add.at(W[j], idx % n_in, w)
        else:
            np.add.at(W[j], np.clip(idx, 0, n_in - 1), w)
    return W.astype(np.float32)


def resize(a, h_out, w_out):
    """Periodic Lanczos-3 resize of (H, W) or (H, W, C), per channel, in whatever space the data is (linear light)."""
    h, w = a.shape[:2]
    if (h, w) == (h_out, w_out):
        return a.astype(np.float32)
    Wv = lanczos_matrix(h, h_out)
    Wu = lanczos_matrix(w, w_out)
    if a.ndim == 2:
        return (Wv @ a.astype(np.float32) @ Wu.T).astype(np.float32)
    return np.stack([(Wv @ a[..., c].astype(np.float32) @ Wu.T) for c in range(a.shape[2])], axis=2).astype(np.float32)


def resize_normals(n, h, w):
    r = resize(n, h, w)
    return normalize(r)


def normalize(n):
    l = np.sqrt((n.astype(np.float64) ** 2).sum(axis=2, keepdims=True))
    out = n / np.maximum(l, 1e-6)
    flat = l[..., 0] < 1e-6
    out[flat] = (0, 0, 1)
    return out.astype(np.float32)


def lum(rgb):
    return (rgb.astype(np.float64) @ LUM).astype(np.float32)


def wmean(x, w):
    return float((x * w).sum() / max(w.sum(), 1e-12))


def wstd(x, w):
    m = wmean(x, w)
    return float(math.sqrt(max(wmean((x - m) ** 2, w), 0.0)))


def corr(a, b):
    a = a - a.mean()
    b = b - b.mean()
    return float((a * b).sum() / math.sqrt((a * a).sum() * (b * b).sum() + 1e-12))


def bake_metric(Y, n):
    hp = Y - gblur(Y, 6)
    nx = n[..., 0] - gblur(n[..., 0], 6)
    ny = n[..., 1] - gblur(n[..., 1], 6)
    return float(math.hypot(corr(hp, nx), corr(hp, ny)))


def seam_score(a):
    """TEX's edge / interior difference ratio (max over both axes) of a 2D array."""
    def axis_ratio(x):
        d_int = np.abs(np.diff(x, axis=1)).mean()
        d_edge = np.abs(x[:, 0] - x[:, -1]).mean()
        return float(d_edge / max(d_int, 1e-9))
    return max(axis_ratio(a), axis_ratio(a.T))


def feather_seamless(a, frac=0.12):
    """Half-offset seamless blend: the source's wrap seam is covered by its half-rolled copy, whose own seam (the centre cross)
    lies where the source weight is 1. Weight = smoothstep over a feather of `frac` of the size at every edge."""
    h, w = a.shape[:2]
    def ramp(n):
        x = (np.arange(n) + 0.5) / n
        d = np.minimum(x, 1 - x) / frac
        t = np.clip(d, 0, 1)
        return t * t * (3 - 2 * t)
    wgt = np.minimum(ramp(h)[:, None], ramp(w)[None, :]).astype(np.float32)
    b = np.roll(np.roll(a, h // 2, axis=0), w // 2, axis=1)
    if a.ndim == 3:
        wgt = wgt[..., None]
    return (a * wgt + b * (1 - wgt)).astype(np.float32)


def poisson_height(n):
    """Periodic Poisson integration of a GL normal map into a height (dh/du = -nx/nz, dh/dv = -ny/nz; v up = -row)."""
    nz = np.maximum(n[..., 2], 0.1)
    gx = -n[..., 0] / nz                 # dh/dcol
    gy = n[..., 1] / nz                  # dh/drow (v up = row down)
    h, w = gx.shape
    fx = np.fft.fftfreq(w)[None, :]
    fy = np.fft.fftfreq(h)[:, None]
    GX, GY = np.fft.fft2(gx), np.fft.fft2(gy)
    denom = (2j * np.pi * fx) * np.conj(2j * np.pi * fx) + (2j * np.pi * fy) * np.conj(2j * np.pi * fy)
    num = np.conj(2j * np.pi * fx) * GX + np.conj(2j * np.pi * fy) * GY
    denom[0, 0] = 1
    H = num / denom
    H[0, 0] = 0
    return np.real(np.fft.ifft2(H)).astype(np.float32)


def sign_check(n, height):
    """corr(n.x, -dh/du) and corr(n.y, -dh/dv) of a GL normal against its displacement (both must be positive)."""
    hb = gblur(height, 1.5)
    dhdu = np.roll(hb, -1, axis=1) - np.roll(hb, 1, axis=1)
    dhdv = -(np.roll(hb, -1, axis=0) - np.roll(hb, 1, axis=0))       # v up = row down
    return corr(n[..., 0], -dhdu), corr(n[..., 1], -dhdv)


# ================================================================================================================ registration

def axis_power(img, axis):
    """Mean power per integer frequency k (cycles per tile) along `axis` (1 = u / columns, 0 = v / rows)."""
    x = img - img.mean()
    F = np.fft.rfft(x, axis=axis)
    return (np.abs(F) ** 2).mean(axis=1 - axis)


def pick_count(P, k_lo, k_hi):
    """Integer count with the most prominent spectral peak in [k_lo, k_hi] (peak over the median of its neighbours)."""
    best, best_score = None, -1.0
    k_lo = max(1, int(k_lo))
    k_hi = min(len(P) - 2, int(k_hi))
    for k in range(k_lo, k_hi + 1):
        nb = [P[j] for j in range(max(1, k - 4), min(len(P), k + 5)) if abs(j - k) > 1]
        base = float(np.median(nb)) if nb else 1e-12
        h2 = P[2 * k] if 2 * k < len(P) else 0.0
        score = (P[k] + 0.5 * h2) / max(base, 1e-12)
        if score > best_score:
            best, best_score = k, score
    return best, best_score


def joint_phase(profile, k):
    """Position (px, in [0, period)) of a joint: the minimum of the profile folded modulo the period (half-pixel bins), after a
    periodic high-pass that removes the slab-to-slab tone (a joint is a narrow dip in AO or height; the k-th harmonic alone is
    pulled off the joint when the slabs differ in tone)."""
    n = len(profile)
    p = n / float(k)
    f = np.fft.rfftfreq(n)
    sig = p / 6.0
    lp = np.fft.irfft(np.fft.rfft(profile) * np.exp(-2 * (np.pi ** 2) * (sig ** 2) * f ** 2), n=n)
    hp = profile - lp
    nb = max(8, int(round(2 * p)))
    pos = np.mod(np.arange(n, dtype=np.float64), p)
    b = np.minimum((pos / p * nb).astype(int), nb - 1)
    cnt = np.bincount(b, minlength=nb).astype(np.float64)
    fold = np.bincount(b, weights=hp, minlength=nb) / np.maximum(cnt, 1)
    fold = (np.roll(fold, 1) + 2 * fold + np.roll(fold, -1)) / 4.0
    i = int(np.argmin(fold))
    # the dip's centre, not its darkest edge: the centroid of the contiguous bins under half the dip depth
    thr = fold[i] + 0.5 * (float(np.median(fold)) - fold[i])
    lo = i
    while fold[(lo - 1) % nb] < thr and (i - lo) < nb // 2:
        lo -= 1
    hi = i
    while fold[(hi + 1) % nb] < thr and (hi - i) < nb // 2:
        hi += 1
    idx = np.arange(lo, hi + 1)
    wts = thr - fold[idx % nb]
    c = float((idx * wts).sum() / max(wts.sum(), 1e-12))
    return ((c + 0.5) * p / nb) % p


def _hp_profile(profile, p):
    n = len(profile)
    f = np.fft.rfftfreq(n)
    sig = p / 6.0
    lp = np.fft.irfft(np.fft.rfft(profile) * np.exp(-2 * (np.pi ** 2) * (sig ** 2) * f ** 2), n=n)
    hp = profile - lp
    return (np.roll(hp, 1) + 2 * hp + np.roll(hp, -1)) / 4.0


def _dip_centre(v, i):
    """Centroid (index units) of the dip around index i of v (contiguous values under half the dip depth)."""
    thr = v[i] + 0.5 * (float(np.median(v)) - v[i])
    a = i
    while a - 1 >= 0 and v[a - 1] < thr:
        a -= 1
    b = i
    while b + 1 < len(v) and v[b + 1] < thr:
        b += 1
    w = thr - v[a:b + 1]
    return float((np.arange(a, b + 1) * w).sum() / max(w.sum(), 1e-12))


def joint_positions(profile, k, x0):
    """Coordinates (px, pixel i spans [i, i + 1)) of the k joints of a periodic profile, each the dip centre within +-p/4 of
    x0 + j p (x0 from joint_phase, index units)."""
    n = len(profile)
    p = n / float(k)
    hp = _hp_profile(profile, p)
    out = []
    for j in range(k):
        ideal = x0 + j * p
        lo = int(math.floor(ideal - p / 4.0))
        hi = int(math.ceil(ideal + p / 4.0)) + 1
        idx = np.arange(lo, hi)
        v = hp[idx % n]
        c = _dip_centre(v, int(np.argmin(v)))
        out.append(lo + c + 0.5)
    return np.array(out, np.float64)


def _catmull(img, x, axis):
    """Samples img (periodic along axis) at fractional pixel positions x (index units) with Catmull-Rom weights."""
    n = img.shape[axis]
    i0 = np.floor(x).astype(np.int64)
    f = (x - i0).astype(np.float32)
    w = [(-0.5 * f ** 3 + f ** 2 - 0.5 * f), (1.5 * f ** 3 - 2.5 * f ** 2 + 1.0), (-1.5 * f ** 3 + 2.0 * f ** 2 + 0.5 * f), (0.5 * f ** 3 - 0.5 * f ** 2)]
    out = None
    for t in range(4):
        tap = np.take(img, (i0 - 1 + t) % n, axis=axis)
        shp = [1] * img.ndim
        shp[axis] = len(x)
        term = tap * w[t].reshape(shp)
        out = term if out is None else out + term
    return out.astype(np.float32)


def warp_axis(img, src_pos, axis, target0):
    """Piecewise-linear periodic warp along `axis`: source coordinate src_pos[j] lands on target0 + j p (p = n / k). The cyclic
    assignment is the one that moves the joints least. Returns (warped, max |shift| px, max |local scale - 1|)."""
    n = img.shape[axis]
    k = len(src_pos)
    p = n / float(k)
    s = np.sort(np.mod(src_pos, n))
    # cyclic assignment: the source joint nearest to target0 becomes joint 0
    d = (s - target0 + n / 2.0) % n - n / 2.0
    j0 = int(np.argmin(np.abs(d)))
    s = np.roll(s, -j0)
    s = np.where(np.arange(k) > 0, np.where(s < s[0], s + n, s), s)        # unwrap to increasing
    if s[0] > target0 + n / 2.0:
        s = s - n
    t_all = target0 + np.arange(k) * p
    # a joint that sits off the uniform grid by more than p / 5 is a mis-detection (an irregular brick, a chip): it is dropped
    # and its neighbours carry the span; when the remaining knots still need more than 10 % local stretch, the axis is only
    # shifted by its median offset (the photo's own irregularity is kept rather than a brick being stretched)
    r = s - t_all
    o = float(np.median(r))
    keep = np.abs(r - o) <= p / 5.0
    if keep.sum() < 2:
        keep[:] = False
    s_k, t_k = s[keep], t_all[keep]
    scale = 0.0
    if len(s_k) >= 2:
        s_k = np.append(s_k, s_k[0] + n)
        t_k = np.append(t_k, t_k[0] + n)
        scale = float(np.max(np.abs(np.diff(s_k) / np.diff(t_k) - 1.0)))
    if len(s_k) < 2 or scale > 0.10:
        s_k = np.array([target0 + o, target0 + o + n])
        t_k = np.array([target0, target0 + n])
        scale = 0.0
    t = np.arange(n) + 0.5
    tt = (t - t_k[0]) % n + t_k[0]
    src = np.interp(tt, t_k, s_k)
    out = _catmull(img, src - 0.5, axis)
    shift = float(np.max(np.abs(s_k - t_k)))
    return out, shift, scale


def warp_maps(maps, fn):
    out = {}
    for name, a in maps.items():
        if a is None:
            out[name] = None
            continue
        b = fn(a)
        out[name] = normalize(b) if name == "normal" else b
    return out


def _reg_source(maps, which):
    if which in ("ao", "AO") and maps.get("ao") is not None:
        return maps["ao"]
    if maps.get("height") is not None:
        return maps["height"]
    if maps.get("ao") is not None:
        return maps["ao"]
    raise TexprepError("registration needs AO or height")


def grid_register(maps, which, ku, kv, bond_axis):
    """Moves every joint onto the analytic grid (1.4 step 3: module boundaries in the middle of the joints). The straight joints
    (both axes for a stack bond; v rows for a bond along u; u columns for a bond along v) are warped globally, piecewise linearly,
    so joint j lies at j p; a bonded axis is warped per band of the straight axis: band 0 (texture v in [0, mv) = the bottom image
    rows for a bond along u; texture u in [0, mu) for a bond along v) has its joints on 0, odd bands on p / 2. ku or kv None =
    that axis is not registered. Returns (maps, report)."""
    rep = {"maxShiftPx": 0.0, "maxScale": 0.0}

    def note(sh, sc):
        rep["maxShiftPx"] = round(max(rep["maxShiftPx"], sh), 2)
        rep["maxScale"] = round(max(rep["maxScale"], sc), 4)

    straight = []
    if bond_axis == "u":
        straight = [("v", kv)]
    elif bond_axis == "v":
        straight = [("u", ku)]
    else:
        straight = [(a, k) for a, k in (("u", ku), ("v", kv)) if k]
    for axis, k in straight:
        src = _reg_source(maps, which)
        ax = 1 if axis == "u" else 0
        prof = src.mean(axis=1 - ax).astype(np.float64)
        x0 = joint_phase(prof, k)
        pos = joint_positions(prof, k, x0)
        res = {}

        def fn(a, ax=ax, pos=pos):
            w, sh, sc = warp_axis(a, pos, ax, 0.0)
            res["r"] = (sh, sc)
            return w
        maps = warp_maps(maps, fn)
        note(*res["r"])
    if bond_axis in ("u", "v"):
        kb = ku if bond_axis == "u" else kv              # joints along the bond axis
        ks = kv if bond_axis == "u" else ku              # bands along the straight axis
        src = _reg_source(maps, which)
        h, w = src.shape
        bond_ax = 1 if bond_axis == "u" else 0
        n_s = h if bond_axis == "u" else w
        p_s = n_s / float(ks)
        n_b = w if bond_axis == "u" else h
        p_b = n_b / float(kb)
        # pixel -> band: for a bond along u the band index counts texture v from the bottom image row
        centres = np.arange(n_s) + 0.5
        band_of = np.floor((n_s - centres) / p_s).astype(int) if bond_axis == "u" else np.floor(centres / p_s).astype(int)
        band_of = np.clip(band_of, 0, ks - 1)
        targets = {}
        for j in range(ks):
            sel = np.where(band_of == j)[0]
            trim = max(1, int(len(sel) / 6))
            inner = sel[trim:len(sel) - trim] if len(sel) > 2 * trim + 1 else sel
            sub = src[inner, :] if bond_axis == "u" else src[:, inner]
            prof = sub.mean(axis=1 - bond_ax).astype(np.float64)
            x0 = joint_phase(prof, kb)
            pos = joint_positions(prof, kb, x0)
            # texture parity: even bands on 0, odd on p / 2 (image rows / columns, whichever direction, give the same sets)
            t0 = 0.0 if j % 2 == 0 else 0.5 * p_b
            targets[j] = (sel, pos, t0)
        out = {}
        for name, a in maps.items():
            if a is None:
                out[name] = None
                continue
            b = a.copy()
            for j, (sel, pos, t0) in targets.items():
                sub = a[sel] if bond_axis == "u" else a[:, sel]
                wsub, sh, sc = warp_axis(sub, pos, bond_ax, t0)
                note(sh, sc)
                if bond_axis == "u":
                    b[sel] = wsub
                else:
                    b[:, sel] = wsub
            out[name] = normalize(b) if name == "normal" else b
        maps = out
    return maps, rep


def lattice_register(maps, unit, tile):
    """Near-set lattice registration (1.4 step 3): whole modules per tile (the count from the config module, measured by FFT of the
    AO or height when it is not integral or not given), every joint warped onto the module grid (grid_register). Returns
    (maps, info): moduleU, moduleV (m), counts, the bond axis, the spectral prominence and the warp report."""
    which = unit.get("register") if unit.get("register") in ("ao", "height") else "ao"
    src = _reg_source(maps, which)
    h, w = src.shape
    mod = unit.get("module") or [None, None]
    bond = unit.get("bond", "stack")
    bond_axis = "v" if (bond in ("half", "running") and unit.get("longAxis") == "v") else "u"
    if bond == "stack":
        bond_axis = ""
    counts, scores = [], []
    for ax, m in ((1, mod[0]), (0, mod[1])):
        n = w if ax == 1 else h
        P = axis_power(src, ax)
        if m:
            # the config module is the TEX-R measurement: a near-integer count is taken as is (its spectral prominence is
            # recorded); otherwise the floor / ceil count with the stronger peak wins
            k0 = tile / float(m)
            if abs(k0 - round(k0)) <= 0.05 * k0 / max(round(k0), 1):
                k = int(round(k0))
                sc = pick_count(P, k, k)[1]
            else:
                k, sc = pick_count(P, math.floor(k0), math.ceil(k0))
        else:
            k, sc = pick_count(P, 2, n // 24)
        counts.append(k)
        scores.append(round(float(sc), 2))
    ku, kv = counts
    if bond_axis == "u" and kv % 2:
        raise TexprepError("running bond along u needs an even course count per tile, measured %d" % kv)
    if bond_axis == "v" and ku % 2:
        raise TexprepError("half bond along v needs an even column count per tile, measured %d" % ku)
    maps, rep = grid_register(maps, which, ku, kv, bond_axis)
    info = {"moduleU": tile / ku, "moduleV": tile / kv, "countU": ku, "countV": kv, "bondAxis": bond_axis,
            "runningBond": bond in ("running", "half"), "peakProminence": scores, "warp": rep}
    return maps, info


def facade_register(maps, unit, tile):
    """Facade registration (1.3 slice table, 1.4 step 3): the measured period of the named map is re-scaled so a whole number of
    periods fills `per` metres (scale change <= 7 %), and every joint is warped onto that grid (grid_register). Returns
    (maps, tileU, tileV, info); a one-axis registration scales the other axis by the same factor (isotropic)."""
    reg = unit["register"]
    which = "ao" if reg.get("map", "AO") in ("AO", "ao") else "height"
    src = _reg_source(maps, which)
    axes = reg.get("axis", "v")
    per = reg.get("per")
    count = reg.get("count")
    pitch = reg.get("sourcePitch")
    info = {}

    def one(axis, per_m, count_n, pitch_m):
        ax = 1 if axis == "u" else 0
        P = axis_power(src, ax)
        if pitch_m:
            k0 = tile / float(pitch_m)
            k, sc = pick_count(P, math.floor(k0 * 0.75), math.ceil(k0 * 1.3))
        else:
            k, sc = pick_count(P, max(1, int(tile / 1.5)), max(2, int(tile / 0.08)))
        p_src = tile / k
        if isinstance(count_n, (int, float)) and not isinstance(count_n, bool):
            nn = int(count_n)
        else:
            nn = max(1, int(round(per_m / p_src)))
        p_new = per_m / nn
        scale = p_new / p_src - 1.0
        if abs(scale) > 0.07 + 1e-9:
            raise TexprepError("facade registration %s: %d periods of %.4f m -> %d per %.2f m is a %.1f %% scale change (> 7 %%)"
                               % (axis, k, p_src, nn, per_m, 100 * scale))
        info[axis] = {"count": k, "pitchSrc": round(p_src, 5), "perUnit": nn, "pitch": round(p_new, 5), "scale": round(scale, 4),
                      "prominence": round(float(sc), 2)}
        return k, k * p_new

    ku = kv = None
    if axes == "uv":
        ku, tile_u = one("u", per[0], count[0] if isinstance(count, list) else count, pitch[0] if isinstance(pitch, list) else pitch)
        kv, tile_v = one("v", per[1], count[1] if isinstance(count, list) else count, pitch[1] if isinstance(pitch, list) else pitch)
    elif axes == "u":
        ku, tile_u = one("u", per, count, pitch)
        tile_v = tile_u
    else:
        kv, tile_v = one("v", per, count, pitch)
        tile_u = tile_v
    bond_axis = ""
    if reg.get("bond") == "running" and kv:
        # brick: the courses are the straight joints; the brick count along u is measured inside the bottom course
        h = src.shape[0]
        band = int(round(h / kv))
        rows = src[h - band + band // 6: h - band // 6, :]
        ku, _ = pick_count(axis_power(rows, 1), 2, src.shape[1] // 24)
        if kv % 2:
            raise TexprepError("running bond needs an even course count per tile, measured %d" % kv)
        bond_axis = "u"
        info["u"] = {"count": ku, "brickLength": round(tile_u / ku, 5)}
    maps, rep = grid_register(maps, which, ku, kv, bond_axis)
    info["warp"] = rep
    info["bondAxis"] = bond_axis
    return maps, tile_u, tile_v, info


# ================================================================================================================ colour

def hsv_sat(srgb):
    mx = srgb.max(axis=2)
    mn = srgb.min(axis=2)
    return (mx - mn) / np.maximum(mx, 1e-6)


def hsv_hue(srgb):
    r, g, b = srgb[..., 0], srgb[..., 1], srgb[..., 2]
    mx = srgb.max(axis=2)
    mn = srgb.min(axis=2)
    d = np.maximum(mx - mn, 1e-9)
    h = np.where(mx == r, ((g - b) / d) % 6, np.where(mx == g, (b - r) / d + 2, (r - g) / d + 4))
    return (h * 60.0) % 360.0


def violet_mask(var, tok, w):
    """TEX-R tokencheck: albedo = token x 2 x var, exposure-normalised (its p95 at 0.8), hue 250-340 deg with sRGB chroma > 0.12."""
    b = tok[None, None, :] * 2.0 * var
    b = b / b.max() if b.max() > 1 else b
    s = l2s(b / max(1e-6, np.percentile(b.max(axis=2)[w > 0.5] if (w > 0.5).any() else b.max(axis=2), 95)) * 0.8)
    R, G, B = s[..., 0], s[..., 1], s[..., 2]
    hue = (np.degrees(np.arctan2(np.sqrt(3) * (G - B), 2 * R - G - B)) + 360) % 360
    cc = (s.max(axis=2) - s.min(axis=2)) / np.maximum(s.max(axis=2), 1e-6)
    return (hue > 250) & (hue < 340) & (cc > 0.12)


def off_hue_share(var, tok, w):
    """Share of product pixels (token x 2 x var) more than 90 deg of hue from the token with saturation > 0.15."""
    p = l2s(np.clip(tok[None, None, :] * 2.0 * var, 0, 1))
    th = hsv_hue(l2s(np.clip(tok[None, None, :], 0, 1)))[0, 0]
    dh = np.abs(((hsv_hue(p) - th) + 180.0) % 360.0 - 180.0)
    return wmean(((dh > 90.0) & (hsv_sat(p) > 0.15)).astype(np.float64), w)


def violet_share(var, tok, w):
    return wmean(violet_mask(var, tok, w).astype(np.float64), w)


def product_sat(var, tok, w):
    p = l2s(np.clip(tok[None, None, :] * 2.0 * var, 0, 1))
    return wmean(hsv_sat(p), w)


def normalise_mean(var, w):
    for c in range(3):
        m = wmean(var[..., c], w)
        var[..., c] *= 0.5 / max(m, 1e-6)
    return var


def set_contrast(var, target, w):
    for _ in range(8):
        Y = lum(var)
        m = wmean(Y, w)
        s = wstd(Y, w) / max(m, 1e-9)
        k = target / max(s, 1e-6)
        means = np.array([wmean(var[..., c], w) for c in range(3)], np.float32)
        var = means[None, None, :] + k * (var - means[None, None, :])
        var = np.clip(var, 0.01, 0.99).astype(np.float32)
        var = normalise_mean(var, w)
        if abs(k - 1.0) < 1e-3:
            break
    return var


def scale_chroma(var, c):
    Y = lum(var)[..., None]
    return (Y + c * (var - Y)).astype(np.float32)


def grade_colour(var, unit, tok, w):
    """1.4 step 5: per-channel mean 0.5, contrast to the class midpoint, chroma cap, violet desaturation (iterated)."""
    cls = unit.get("class", "concrete")
    target = float(unit.get("contrast", 0.32))
    sat_max = float(unit.get("satMax", CLASS_SAT.get(cls, 0.12)))
    keep = float(unit.get("chromaKeep", 0.8))
    var = normalise_mean(var.astype(np.float32), w)
    var = scale_chroma(var, keep)
    for _ in range(3):
        var = normalise_mean(var, w)
        var = set_contrast(var, target, w)
        if product_sat(var, tok, w) > sat_max:
            lo, hi = 0.0, 1.0
            for _ in range(20):
                mid = 0.5 * (lo + hi)
                if product_sat(scale_chroma(var, mid), tok, w) > sat_max:
                    hi = mid
                else:
                    lo = mid
            var = scale_chroma(var, lo)
        # violet: LOOK_V2 5.4 (HSV hue 260-330, saturation > 0.15 on the product) and TEX-R's gate measure (exposure-normalised
        # hue 250-340, chroma > 0.12) both desaturate the variation of those pixels to grey
        p = l2s(np.clip(tok[None, None, :] * 2.0 * var, 0, 1))
        hue, sat = hsv_hue(p), hsv_sat(p)
        # off-hue: the per-channel white balance turns a cast into its complement (TEX-R finding 1: grey under olive -> violet,
        # brown twigs under the leaf mean -> blue, plank grain under its red mean -> blue stripes); any pixel of the product more
        # than 90 deg of hue away from its token with saturation > 0.15 is desaturated as well (a near-neutral token: its hue
        # is that of the token's slight warmth, and the 0.12 saturation cap keeps those pixels under 0.15 anyway)
        th = hsv_hue(l2s(np.clip(tok[None, None, :], 0, 1)))[0, 0]
        dh = np.abs(((hue - th) + 180.0) % 360.0 - 180.0)
        bad = ((hue >= 260) & (hue <= 330) & (sat > 0.15)) | violet_mask(var, tok, w) | ((dh > 90.0) & (sat > 0.15))
        if bad.any():
            Y = lum(var)
            var[bad] = Y[bad][:, None]
    var = normalise_mean(np.clip(var, 0.01, 0.99).astype(np.float32), w)
    return var


# ================================================================================================================ units

def unit_key_hash(unit, extra):
    blob = json.dumps({"unit": unit, "extra": extra, "texprep": VERSION}, sort_keys=True)
    return hashlib.sha256(blob.encode("utf-8")).hexdigest()


def decode_unit(unit):
    src = unit["source"]
    files = find_maps(src, unit)
    maps = {}
    for role, path in files.items():
        if role == "albedo":
            maps[role] = s2l(load(path, False)).astype(np.float32)
        elif role == "normal":
            n = load(path, False) * 2.0 - 1.0
            if SOURCE_FIX.get(src, {}).get("flipNormalX"):
                n[..., 0] = -n[..., 0]
            maps[role] = normalize(n)
        else:
            maps[role] = load(path, True)
    inputs = [{"path": os.path.relpath(p, ROOT).replace("\\", "/"), "sha256": sha256(p)} for _, p in sorted(files.items())]
    return maps, inputs


def square_maps(maps, unit):
    shapes = {a.shape[:2] for a in maps.values()}
    if len(shapes) != 1:
        # resample every map to the albedo / normal size first
        ref = (maps.get("normal") if maps.get("normal") is not None else next(iter(maps.values()))).shape[:2]
        for k in list(maps):
            if maps[k].shape[:2] != ref:
                maps[k] = resize_normals(maps[k], *ref) if k == "normal" else resize(maps[k], *ref)
    h, w = next(iter(maps.values())).shape[:2]
    if h == w:
        return maps, None
    mode = unit.get("square") or ("resample" if unit.get("squareResample") else ("resample" if abs(h - w) / max(h, w) < 0.05 else "pad"))
    if mode == "resample":
        n = max(h, w) if max(h, w) <= 2048 else 2048
        n = 1 << int(round(math.log2(min(h, w))))
        for k in list(maps):
            maps[k] = resize_normals(maps[k], n, n) if k == "normal" else resize(maps[k], n, n)
        return maps, {"square": "resample", "from": [w, h]}
    n = max(h, w)
    side = unit.get("padSide", "bottom")
    for k in list(maps):
        a = maps[k]
        if a.ndim == 2:
            pad = np.zeros((n, n), np.float32)
            if k == "rough":
                pad[:] = float(np.median(a))
        else:
            pad = np.zeros((n, n, a.shape[2]), np.float32)
            if k == "normal":
                pad[..., 2] = 1.0
            else:
                pad[:] = a.reshape(-1, a.shape[2]).mean(axis=0)
        if h < w:
            if side == "bottom":
                pad[:h, :w] = a
            else:
                pad[n - h:, :w] = a
        else:
            pad[:h, :w] = a
        maps[k] = pad
    vr = [1.0 - h / n, 1.0] if side == "bottom" else [0.0, h / n]
    return maps, {"square": "pad", "from": [w, h], "decalV": vr}


def mask_pack(maps, unit, shape, metal_ok, aux=None):
    h, w = shape
    mask = np.zeros((h, w, 4), np.float32)
    if metal_ok and maps.get("metal") is not None:
        mask[..., 0] = np.clip(maps["metal"], 0, 1)
    ao = maps.get("ao")
    hgt = maps.get("height")
    stats = {}
    if hgt is not None:
        p1, p99 = np.percentile(hgt, 1), np.percentile(hgt, 99)
        stats["heightP1"], stats["heightP99"] = float(p1), float(p99)
        hn = np.clip((hgt - p1) / max(p99 - p1, 1e-6), 0, 1)
    else:
        hn = np.full((h, w), 0.5, np.float32)
        stats["heightP1"], stats["heightP99"] = 0.5, 0.5
    if ao is None:
        cav = gblur(hn, max(2.0, w / 256.0)) - hn
        ao = np.clip(1.0 - 2.5 * np.maximum(cav, 0), 0.4, 1.0)
    mask[..., 1] = np.clip(ao, 0, 1)
    mask[..., 2] = hn
    lo, hi = (unit.get("smooth") or [0.15, 0.35])[:2]
    if maps.get("rough") is not None:
        s_raw = 1.0 - maps["rough"]
        p5, p95 = np.percentile(s_raw, 5), np.percentile(s_raw, 95)
        s = lo + (s_raw - p5) * (hi - lo) / max(p95 - p5, 1e-6)
        mask[..., 3] = np.clip(s, 0, 1)
    else:
        mask[..., 3] = 0.5 * (lo + hi)
    if aux is not None:
        for c, a in aux.items():
            mask[..., c] = a
    a = mask[..., 3]
    stats["smoothP5"], stats["smoothP50"], stats["smoothP95"] = (float(np.percentile(a, q)) for q in (5, 50, 95))
    stats["metalMean"] = float(mask[..., 0].mean())
    return mask, stats


def bake_normal_strength(n, k):
    if abs(k - 1.0) < 1e-6:
        return n
    m = n.copy()
    m[..., 0] *= k
    m[..., 1] *= k
    return normalize(m)


def process_unit(unit, res, toks, finals, kind="set"):
    """One set or array slice -> {"albedo", "alpha", "normal", "mask"} at res x res, plus its metadata."""
    src = unit["source"]
    if src not in finals:
        raise TexprepError("%s is not a TEX-R final set (docs/TEXTURES.md 'Final sets')" % src)
    maps, inputs = decode_unit(unit)
    meta = {"source": src, "tileRef": finals[src], "inputs": inputs}
    st = {}
    if maps.get("normal") is None:
        raise TexprepError(src + ": no NormalGL map")
    # 2: sign check, height from normal
    if maps.get("height") is not None:
        cx, cy = sign_check(maps["normal"], maps["height"])
        st["signCorrX"], st["signCorrY"] = round(cx, 4), round(cy, 4)
        if SOURCE_FIX.get(src):
            st["sourceFix"] = sorted(SOURCE_FIX[src])
        if cx <= 0 or cy <= 0:
            raise TexprepError("%s: the normal does not follow the displacement (corr x %.3f, y %.3f; both must be > 0): not a GL normal?" % (src, cx, cy))
    elif unit.get("heightFromNormal") or kind != "overlay":
        maps["height"] = poisson_height(maps["normal"])
        st["heightFromNormal"] = True
    # 3: seam check on the source as downloaded (a registration roll later moves joints onto the edge on purpose), square,
    #    register, seamless
    if maps.get("albedo") is not None:
        st["seamScore"] = round(seam_score(lum(maps["albedo"])), 3)
    maps, sq = square_maps(maps, unit)
    if sq:
        meta["squaring"] = sq
    tile = float(unit.get("tile", finals[src]))
    tile_u, tile_v = tile, float(unit.get("tileV", tile))
    reg_info = None
    if unit.get("tileReason"):
        meta["registration"] = "config: " + str(unit["tileReason"])
    if isinstance(unit.get("register"), dict):
        maps, tile_u, tile_v, reg_info = facade_register(maps, unit, tile)
        meta["registration"] = "facade " + json.dumps(reg_info, sort_keys=True)
    elif unit.get("register") in ("ao", "height") or (unit.get("antiTiling") == "module" and unit.get("module")):
        maps, reg_info = lattice_register(maps, unit, tile)
        meta.update({"moduleU": reg_info["moduleU"], "moduleV": reg_info["moduleV"], "runningBond": reg_info["runningBond"],
                     "bondAxis": reg_info["bondAxis"]})
        meta.setdefault("registration", "")
        meta["lattice"] = reg_info
    elif unit.get("module"):
        mu, mv = unit["module"]
        meta.update({"moduleU": float(mu or 0), "moduleV": float(mv or 0)})
    alb = maps.get("albedo")
    if alb is not None:
        accept = unit.get("seamAccept") == "real" or src in REAL_SEAMS
        if unit.get("seamless"):
            for k in list(maps):
                maps[k] = normalize(feather_seamless(maps[k])) if k == "normal" else feather_seamless(maps[k])
            alb = maps["albedo"]
            st["seamScoreBefore"] = st["seamScore"]
            st["seamScore"] = round(seam_score(lum(alb)), 3)
        elif st["seamScore"] > 1.25 and not accept and kind not in ("overlay",):
            raise TexprepError("%s: seam score %.2f > 1.25 (set seamless or seamAccept after a look)" % (src, st["seamScore"]))
    # 4: baked light
    if alb is not None:
        Y = lum(alb)
        n = maps["normal"]
        N = Y.shape[1]
        st["bakeBefore"] = round(bake_metric(Y, n), 4)
        sig1m = min(0.5 / tile * N, N / 8.0)
        lf_before = float(np.std(gblur(Y, sig1m)) / max(Y.mean(), 1e-9))
        lp8 = gblur(Y, N / 8.0)
        st["blobBefore"] = round(float((np.abs(gblur(Y, N / 48.0) - Y.mean()) > 1.25 * Y.std()).mean()), 4)
        # 4b ramp guard (measured; a periodic tile has no plane to remove without breaking its wrap)
        yy, xx = np.mgrid[0:lp8.shape[0], 0:lp8.shape[1]]
        A = np.stack([xx.ravel() / N, yy.ravel() / N, np.ones(xx.size)], axis=1)
        coef = np.linalg.lstsq(A, lp8.ravel().astype(np.float64), rcond=None)[0]
        st["rampSlope"] = round(float(max(abs(coef[0]), abs(coef[1])) / max(Y.mean(), 1e-9)), 4)
        if st["rampSlope"] > 0.03:
            log("WARN %s: low-pass ramp %.1f %% per tile (> 3 %%), not removed (periodic tile)" % (src, 100 * st["rampSlope"]))
        if unit.get("bakeFit"):
            if src not in BAKE_FIT_ALLOW:
                raise TexprepError("%s: bakeFit is allowed only for %s (TEX-R bake > 0.10)" % (src, ", ".join(BAKE_FIT_ALLOW)))
            hp = (Y - gblur(Y, 6)).astype(np.float64)
            hx = (n[..., 0] - gblur(n[..., 0], 6)).astype(np.float64)
            hy = (n[..., 1] - gblur(n[..., 1], 6)).astype(np.float64)
            M = np.stack([hx.ravel(), hy.ravel()], axis=1)
            ab = np.linalg.lstsq(M, hp.ravel(), rcond=None)[0]
            fit = (ab[0] * hx + ab[1] * hy).astype(np.float32)
            Y2 = np.maximum(Y - fit, 0.02 * Y.mean())
            alb = alb * (Y2 / np.maximum(Y, 1e-6))[..., None]
            st["bakeFitCoef"] = [round(float(ab[0]), 5), round(float(ab[1]), 5)]
        flat = float(unit.get("flatten", 0) or 0)
        st["flatten"] = flat
        if flat > 0:
            if src not in FLATTEN_ALLOW or flat > 0.6:
                raise TexprepError("%s: flatten %.2f not allowed (only %s, <= 0.6)" % (src, flat, ", ".join(FLATTEN_ALLOW)))
            Yc = lum(alb)
            lp = gblur(Yc, N / 8.0)
            alb = alb * np.power(Yc.mean() / np.maximum(lp, 1e-6), flat)[..., None]
        if maps.get("ao") is not None and kind != "overlay":
            alb = alb / (1.0 - 0.5 * (1.0 - np.clip(maps["ao"], 0, 1)))[..., None]
        Y = lum(alb)
        st["bake"] = round(bake_metric(Y, n), 4)
        if unit.get("bakeFit") and st["bake"] > 0.10:
            raise TexprepError("%s: bake %.3f after the fit (> 0.10)" % (src, st["bake"]))
        lf_after = float(np.std(gblur(Y, sig1m)) / max(Y.mean(), 1e-9))
        st["lfBefore"], st["lf"] = round(lf_before, 4), round(lf_after, 4)
        st["blob"] = round(float((np.abs(gblur(Y, N / 48.0) - Y.mean()) > 1.25 * Y.std()).mean()), 4)
        # (a tile much smaller than 1 m has no 1 m low-pass content to lose: the gate applies from 1 % relative std)
        if flat <= 0 and lf_before >= 0.01 and lf_after < 0.6 * lf_before - 1e-6:
            raise TexprepError("%s: content gate: 1 m low-pass std %.4f < 0.6 x %.4f" % (src, lf_after, lf_before))
        maps["albedo"] = alb.astype(np.float32)
    # resize to the output resolution
    h, w = next(iter(maps.values())).shape[:2]
    if (h, w) != (res, res):
        for k in list(maps):
            maps[k] = resize_normals(maps[k], res, res) if k == "normal" else resize(maps[k], res, res)
        for k in ("ao", "rough", "metal", "opacity", "scattering"):
            if maps.get(k) is not None:
                maps[k] = np.clip(maps[k], 0, 1)
        if maps.get("albedo") is not None:
            maps["albedo"] = np.maximum(maps["albedo"], 0)
    out = {"normal": maps["normal"]}
    cls = unit.get("class", "concrete")
    tok = toks.get(unit.get("token") or CLASS_TOKEN.get(cls, "ConcreteDark"))
    if tok is None:
        raise TexprepError("unknown token for class %s" % cls)
    # 5: colour
    wgt = np.ones((res, res), np.float32)
    if unit.get("opacity") and maps.get("opacity") is not None:
        wgt = (maps["opacity"] > 0.5).astype(np.float32)
        out["alpha"] = np.clip(maps["opacity"], 0, 1)
        st["coverage"] = round(float(maps["opacity"].mean()), 4)
    if maps.get("albedo") is not None:
        alb = maps["albedo"]
        st["meanAbs"] = [round(wmean(alb[..., c], wgt), 5) for c in range(3)]
        var = grade_colour(alb, unit, tok, wgt)
        if "alpha" in out:
            var = push_pull(var, out["alpha"])            # the uncovered texels take the covered colour (mips)
            var = balance_uncovered(var, out["alpha"])    # and the whole texture averages 0.5 (its last mip)
        out["albedo"] = var
        Yv = lum(var)
        st["mean"] = [round(wmean(var[..., c], wgt), 5) for c in range(3)]
        st["stdOverMean"] = round(wstd(Yv, wgt) / max(wmean(Yv, wgt), 1e-9), 4)
        st["contrastTarget"] = float(unit.get("contrast", 0.32))
        st["satMax"] = float(unit.get("satMax", CLASS_SAT.get(cls, 0.12)))
        st["saturation"] = round(product_sat(var, tok, wgt), 4)
        st["violetShare"] = round(violet_share(var, tok, wgt), 5)
        st["offHueShare"] = round(off_hue_share(var, tok, wgt), 5)
        if st["violetShare"] > 0.005:
            raise TexprepError("%s: violet share %.4f > 0.5 %%" % (src, st["violetShare"]))
    # 6: mask
    metal_ok = bool(unit.get("metalFromMap")) and src in METAL_ALLOW
    mask, ms = mask_pack(maps, unit, (res, res), metal_ok)
    st.update(ms)
    out["mask"] = mask
    ns = float(unit.get("normalStrength", CLASS_NORMAL.get(cls, 1.0)))
    out["normal"] = bake_normal_strength(out["normal"], ns)
    st["normalStrength"] = ns
    meta.update({"tile": tile_u, "tileV": tile_v if abs(tile_v - tile_u) > 1e-6 else 0.0, "stats": st})
    if reg_info is not None and "registration" in meta and meta["registration"].startswith("facade"):
        meta["facadeReg"] = reg_info
    return out, meta


def process_overlay(unit, res, finals):
    src = unit["source"]
    if src not in finals:
        raise TexprepError("%s is not a TEX-R final set" % src)
    maps, inputs = decode_unit(unit)
    maps.pop("albedo", None)                                              # overlays carry no albedo (TEX-R)
    st = {}
    maps, sq = square_maps(maps, unit)
    h, w = next(iter(maps.values())).shape[:2]
    if (h, w) != (res, res):
        for k in list(maps):
            maps[k] = resize_normals(maps[k], res, res) if k == "normal" else np.clip(resize(maps[k], res, res), 0, 1)
    cov_kind = unit.get("coverage", "opacity")
    if cov_kind == "opacity":
        cov = maps["opacity"]
    elif cov_kind == "1-metal":
        cov = 1.0 - maps["metal"]
    elif cov_kind == "smoothRough":
        r = maps["rough"]
        p5, p95 = np.percentile(r, 5), np.percentile(r, 95)
        cov = np.clip((p95 - r) / max(p95 - p5, 1e-6), 0, 1)
    else:
        raise TexprepError("unknown coverage " + cov_kind)
    cov = np.clip(cov, 0, 1).astype(np.float32)
    mask = np.zeros((res, res, 4), np.float32)
    mask[..., 0] = cov
    mask[..., 1] = 1.0 - 0.5 * cov
    if maps.get("height") is not None:
        hg = maps["height"]
        p1, p99 = np.percentile(hg, 1), np.percentile(hg, 99)
        mask[..., 2] = np.clip((hg - p1) / max(p99 - p1, 1e-6), 0, 1)
    else:
        mask[..., 2] = 0.5
    mask[..., 3] = 1.0 - maps["rough"] if maps.get("rough") is not None else 0.5
    n = maps["normal"].copy()
    if sq and sq.get("square") == "pad":
        n = normalize(n * np.stack([cov > 0.004] * 2 + [np.ones_like(cov, bool)], axis=2) + np.stack([np.zeros_like(cov)] * 2 + [cov <= 0.004], axis=2))
    st["coverage"] = round(float(cov.mean()), 4)
    st["smoothP5"], st["smoothP50"], st["smoothP95"] = (float(np.percentile(mask[..., 3], q)) for q in (5, 50, 95))
    meta = {"source": src, "tileRef": finals[src], "inputs": inputs, "tile": float(unit.get("tile", finals[src])), "tileV": 0.0,
            "stats": st}
    if sq:
        meta["squaring"] = sq
    return {"mask": mask, "normal": n}, meta


def process_micro(unit, res, finals):
    """1.4 step 8: a crop of the parent set (about 0.5 m), made seamless, high-passed; albedo = grey modifier x 0.5 (mean 0.5),
    normal = high-passed slopes (mean flat)."""
    src = unit["source"]
    if src not in finals:
        raise TexprepError("%s is not a TEX-R final set" % src)
    maps, inputs = decode_unit(unit)
    tile = float(unit.get("tile", finals[src]))
    alb, n = maps["albedo"], maps["normal"]
    N = alb.shape[0]
    if unit.get("bakeFit"):
        Y = lum(alb)
        hp = (Y - gblur(Y, 6)).astype(np.float64)
        hx = (n[..., 0] - gblur(n[..., 0], 6)).astype(np.float64)
        hy = (n[..., 1] - gblur(n[..., 1], 6)).astype(np.float64)
        ab = np.linalg.lstsq(np.stack([hx.ravel(), hy.ravel()], axis=1), hp.ravel(), rcond=None)[0]
        Y2 = np.maximum(Y - (ab[0] * hx + ab[1] * hy), 0.02 * Y.mean())
        alb = alb * (Y2 / np.maximum(Y, 1e-6))[..., None]
    c = int(round(float(unit.get("crop", 0.5)) / tile * N))
    Y = lum(alb)
    ao = maps.get("ao")
    # crop position: the flattest low-frequency window (and, with avoidJoints, the one whose darkest AO is brightest)
    best, best_s = (0, 0), None
    lp = gblur(Y, c / 4.0)
    for y0 in range(0, N - c, max(16, c // 6)):
        for x0 in range(0, N - c, max(16, c // 6)):
            win = lp[y0:y0 + c, x0:x0 + c]
            s = float(win.std())
            if unit.get("avoidJoints") and ao is not None:
                s -= 0.5 * float(np.percentile(ao[y0:y0 + c, x0:x0 + c], 1))
            if best_s is None or s < best_s:
                best, best_s = (y0, x0), s
    y0, x0 = best
    ca = feather_seamless(alb[y0:y0 + c, x0:x0 + c].copy())
    cn = normalize(feather_seamless(n[y0:y0 + c, x0:x0 + c].copy()))
    ca = resize(ca, res, res)
    cn = resize_normals(cn, res, res)
    Yc = lum(np.maximum(ca, 0))
    mod = Yc / np.maximum(gblur(Yc, res / 8.0), 1e-6) - 1.0
    mod = mod - mod.mean()
    k = float(unit.get("contrast", 0.12)) / max(float(mod.std()), 1e-6)
    m = np.clip(1.0 + k * mod, 0.02, 1.98)
    m = m / m.mean()
    var = np.repeat((0.5 * m)[..., None], 3, axis=2).astype(np.float32)
    nz = np.maximum(cn[..., 2], 0.1)
    sx, sy = cn[..., 0] / nz, cn[..., 1] / nz
    sx = sx - gblur(sx, res / 8.0)
    sy = sy - gblur(sy, res / 8.0)
    ks = float(unit.get("normalStrength", 1.0))
    nn = normalize(np.stack([sx * ks, sy * ks, np.ones_like(sx)], axis=2))
    st = {"crop": [int(x0), int(y0), int(c)], "mean": [0.5, 0.5, 0.5], "stdOverMean": round(float(var[..., 0].std() / 0.5), 4),
          "contrastTarget": float(unit.get("contrast", 0.12)), "seamScore": round(seam_score(var[..., 0]), 3)}
    meta = {"source": src, "tileRef": finals[src], "inputs": inputs, "tile": float(unit.get("crop", 0.5)), "tileV": 0.0, "stats": st,
            "registration": "micro crop of a %.2f m tile" % tile}
    return {"albedo": var, "normal": nn}, meta


def process_leaves(unit, res, toks, finals):
    src = unit["source"]
    if src not in finals:
        raise TexprepError("%s is not a TEX-R final set" % src)
    fc_path = os.path.join(CFG_DIR, "foliage_compose.py")
    spec = importlib.util.spec_from_file_location("foliage_compose", fc_path)
    fc = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(fc)
    maps, inputs = decode_unit(unit)
    cm = {"albedo": maps["albedo"], "normal": maps["normal"], "opacity": maps["opacity"]}
    for k_src, k_dst in (("rough", "rough"), ("height", "height"), ("scattering", "scattering")):
        if maps.get(k_src) is not None:
            cm[k_dst] = maps[k_src]
    o = fc.compose(unit, cm)
    alpha = np.clip(o["alpha"], 0, 1).astype(np.float32)
    wgt = (alpha > 0.5).astype(np.float32)
    tok = toks.get(unit.get("token") or "Leaf")
    st = {"meanAbs": [round(wmean(o["albedo"][..., c], wgt), 5) for c in range(3)], "coverage": round(float(alpha.mean()), 4)}
    var = grade_colour(o["albedo"], unit, tok, wgt)
    var = push_pull(var, alpha)            # the graded covered colour into the uncovered texels again
    fbk = unit.get("flipbook", [2, 2])
    var = balance_uncovered(var, alpha, int(fbk[0]), int(fbk[1]))   # each sprite cell averages 0.5 over all texels (last mip)
    Yv = lum(var)
    st.update({"mean": [round(wmean(var[..., c], wgt), 5) for c in range(3)],
               "stdOverMean": round(wstd(Yv, wgt) / max(wmean(Yv, wgt), 1e-9), 4), "contrastTarget": float(unit.get("contrast", 0.40)),
               "satMax": float(unit.get("satMax", 0.45)),
               "saturation": round(product_sat(var, tok, wgt), 4), "violetShare": round(violet_share(var, tok, wgt), 5),
               "offHueShare": round(off_hue_share(var, tok, wgt), 5)})
    if st["violetShare"] > 0.005:
        raise TexprepError("%s: violet share %.4f > 0.5 %%" % (src, st["violetShare"]))
    mp = {"ao": o["ao"], "height": o["height"], "rough": o.get("rough")}
    aux = {2: np.clip(o["scatter"], 0, 1)} if (unit.get("compose") or {}).get("scatteringToMaskB") else None
    mask, ms = mask_pack(mp, unit, alpha.shape, False, aux)
    st.update(ms)
    st["composeStats"] = o.get("stats", {})
    ns = float(unit.get("normalStrength", CLASS_NORMAL["leaves"]))
    nrm = bake_normal_strength(normalize(o["normal"]), ns)
    fb = unit.get("flipbook", [2, 2])
    sm = float(unit.get("tile", unit.get("spriteMetres", 1.0)))
    meta = {"source": src, "tileRef": finals[src], "inputs": inputs, "tile": sm,
            "tileV": 0.0, "stats": st, "cols": int(fb[0]), "rows": int(fb[1]), "composeVersion": getattr(fc, "VERSION", 0),
            "registration": "leaf-cluster sprites of %.2f m composed from the %.2f m atlas at true leaf size (foliage_compose)" % (sm, finals[src])}
    return {"albedo": var, "alpha": alpha, "normal": nrm, "mask": mask}, meta


# ================================================================================================================ output

def push_pull(rgb, alpha, thr=0.5):
    """Bleeds the colour of covered texels (alpha > thr) into the uncovered ones (TEXTURE_PLAN 1.4 step 7): a weighted 2 x 2
    pyramid down to 1 x 1, then each uncovered texel takes its parent's mean on the way up. Covered texels keep their colour."""
    w = (alpha > thr).astype(np.float64)
    levels = [(rgb.astype(np.float64) * w[..., None], w)]
    c, ws = levels[0]
    while c.shape[0] > 1 and c.shape[1] > 1 and c.shape[0] % 2 == 0 and c.shape[1] % 2 == 0:
        c = c.reshape(c.shape[0] // 2, 2, c.shape[1] // 2, 2, 3).sum(axis=(1, 3))
        ws = ws.reshape(ws.shape[0] // 2, 2, ws.shape[1] // 2, 2).sum(axis=(1, 3))
        levels.append((c, ws))
    c, ws = levels[-1]
    col = c / np.maximum(ws, 1e-9)[..., None]
    for c, ws in reversed(levels[:-1]):
        up = np.repeat(np.repeat(col, 2, axis=0), 2, axis=1)
        own = c / np.maximum(ws, 1e-9)[..., None]
        col = np.where((ws > 0)[..., None], own, up)
    return np.where((alpha > thr)[..., None], rgb, col).astype(np.float32)


def balance_uncovered(var, alpha, cols=1, rows=1, thr=0.5):
    """Shifts the uncovered texels (alpha <= thr) of each flipbook cell by a constant per channel so the whole cell averages 0.5:
    the last mip of an alpha set (all texels) then equals the covered mean, as TextureTests and the colour rule require. The
    uncovered texels are invisible at mip 0 (alpha to coverage); they only enter the coarse mips."""
    H, W = alpha.shape
    ch, cw = H // rows, W // cols
    out = var.copy()
    for r in range(rows):
        for c in range(cols):
            sl = (slice(r * ch, (r + 1) * ch), slice(c * cw, (c + 1) * cw))
            a = alpha[sl]
            unc = a <= thr
            n_unc = int(unc.sum())
            if n_unc == 0:
                continue
            v = out[sl]
            for _ in range(4):
                for k in range(3):
                    d = 0.5 * v[..., k].size - float(v[..., k].sum())
                    ch_k = v[..., k]
                    ch_k[unc] = np.clip(ch_k[unc] + d / n_unc, 0.01, 0.99)
            out[sl] = v
    return out.astype(np.float32)


def png_rgb(a, path, srgb):
    a = np.clip(a, 0, 1)
    if srgb:
        a = l2s(a)
    img = Image.fromarray(np.round(a * 255.0).astype(np.uint8), "RGB" if a.shape[2] == 3 else "RGBA")
    img.save(path, format="PNG", optimize=True)


def write_albedo(var, alpha, path):
    s = l2s(np.clip(var, 0, 1))
    if alpha is not None:
        rgba = np.concatenate([s, np.clip(alpha, 0, 1)[..., None]], axis=2)        # alpha linear (never sRGB-decoded)
        Image.fromarray(np.round(rgba * 255.0).astype(np.uint8), "RGBA").save(path, format="PNG", optimize=True)
    else:
        Image.fromarray(np.round(s * 255.0).astype(np.uint8), "RGB").save(path, format="PNG", optimize=True)


def write_normal(n, path):
    Image.fromarray(np.round(np.clip(n * 0.5 + 0.5, 0, 1) * 255.0).astype(np.uint8), "RGB").save(path, format="PNG", optimize=True)


def write_mask(m, path):
    Image.fromarray(np.round(np.clip(m, 0, 1) * 255.0).astype(np.uint8), "RGBA").save(path, format="PNG", optimize=True)


def flipbook(slices, cols, rows, res, ch):
    canvas = np.zeros((rows * res, cols * res, ch), np.float32) if ch > 1 else np.zeros((rows * res, cols * res), np.float32)
    for k, a in enumerate(slices):
        r, c = k // cols, k % cols
        canvas[r * res:(r + 1) * res, c * res:(c + 1) * res] = a
    return canvas


def anisotropy_for(key, unit, kind):
    if unit.get("aniso"):
        return int(unit["aniso"])
    if kind == "array":
        return 16
    rows = unit.get("rows") or []
    if any(r.startswith("Floor") or r.startswith("Gray") for r in rows) or unit.get("class") == "asphalt":
        return 16
    return 8


def gridded(unit):
    return bool(unit.get("register")) or unit.get("antiTiling") in ("module", "none") or bool(unit.get("module"))


def output_paths(key, maps):
    return {m: os.path.join(OUT, "%s_%s.png" % (key, m)) for m in maps}


def write_outputs(key, maps_list, data, meta, previews):
    """Writes into <file>.tmp (Unity ignores *.tmp), then os.replace()s each file into place, the texmeta first: a Unity session of
    another agent that starts meanwhile never imports a half-written PNG (the 2026-11-15 23:04 PA_r02 build did). A file whose
    bytes did not change is left alone, so Unity does not re-import it."""
    os.makedirs(OUT, exist_ok=True)
    paths = output_paths(key, maps_list)
    tmp = {m: p + ".tmp" for m, p in paths.items()}
    if "Albedo" in paths:
        write_albedo(data["albedo"], data.get("alpha"), tmp["Albedo"])
    if "Normal" in paths:
        write_normal(data["normal"], tmp["Normal"])
    if "Mask" in paths:
        write_mask(data["mask"], tmp["Mask"])
    meta["outputs"] = [{"path": os.path.relpath(p, ROOT).replace("\\", "/"), "sha256": sha256(tmp[m])} for m, p in sorted(paths.items())]
    mp = os.path.join(OUT, key + ".texmeta.json")
    with open(mp + ".tmp", "w", encoding="utf-8", newline="\n") as f:
        json.dump(meta, f, indent=1, sort_keys=True)
        f.write("\n")
    for src, dst in [(mp + ".tmp", mp)] + [(tmp[m], paths[m]) for m in sorted(paths)]:
        if os.path.exists(dst) and sha256(dst) == sha256(src):
            os.remove(src)
        else:
            os.replace(src, dst)
    if previews:
        write_preview(key, data, meta)


def write_preview(key, data, meta):
    os.makedirs(PREV, exist_ok=True)
    cols, rows = meta.get("cols", 1), meta.get("rows", 1)
    tok = np.array(meta.get("previewToken", [0.18, 0.18, 0.18]))
    base = data.get("albedo")
    if base is None:
        base = np.repeat(data["mask"][..., :1], 3, axis=2) * 0.5 + 0.25
    H, W = base.shape[:2]
    sh, sw = H // rows, W // cols
    tiles = []
    for k in range(cols * rows):
        r, c = k // cols, k % cols
        if meta.get("previewTokens") and k < len(meta["previewTokens"]):
            tok = np.array(meta["previewTokens"][k])
        a = base[r * sh:(r + 1) * sh, c * sw:(c + 1) * sw]
        nrm = data["normal"][r * sh:(r + 1) * sh, c * sw:(c + 1) * sw] if data.get("normal") is not None else None
        alb = np.clip(tok[None, None, :] * 2.0 * a, 0, 1)
        if nrm is not None:
            L = np.array([-0.5, 0.5, 0.7071])
            lam = np.clip((nrm * L[None, None, :]).sum(axis=2), 0, 1)
            lit = alb * (0.25 + 0.95 * lam)[..., None]
        else:
            lit = alb
        if data.get("alpha") is not None:
            al = data["alpha"][r * sh:(r + 1) * sh, c * sw:(c + 1) * sw][..., None]
            alb = alb * al + 0.02 * (1 - al)
            lit = lit * al + 0.02 * (1 - al)
        expo = 0.18 / max(float(lum(alb).mean()), 1e-4)
        def enc(x):
            return Image.fromarray(np.round(l2s(np.clip(x * expo, 0, 1)) * 255).astype(np.uint8), "RGB")
        t3 = np.tile(alb, (3, 3, 1))
        p3 = enc(t3).resize((768, 768), Image.LANCZOS)
        n = alb.shape[0]
        crop = enc(alb[n // 2 - 256:n // 2 + 256, n // 2 - 256:n // 2 + 256]) if n >= 512 else enc(alb).resize((512, 512), Image.NEAREST)
        rolled = np.roll(np.roll(alb, n // 2, 0), n // 2, 1)
        seam = enc(rolled[n // 2 - 128:n // 2 + 128, n // 2 - 128:n // 2 + 128]).resize((512, 512), Image.NEAREST) if n >= 256 else crop
        litc = enc(lit[n // 2 - 256:n // 2 + 256, n // 2 - 256:n // 2 + 256]) if n >= 512 else enc(lit).resize((512, 512), Image.NEAREST)
        sheet = Image.new("RGB", (768 + 512 + 512, 768), (12, 12, 12))
        sheet.paste(p3, (0, 0))
        sheet.paste(crop, (768, 0))
        sheet.paste(seam.resize((256, 256)), (768 + 512, 0))
        sheet.paste(litc.resize((256, 256)), (768 + 512, 256))
        name = key if cols * rows == 1 else "%s_s%d" % (key, k)
        sheet.save(os.path.join(PREV, name + ".jpg"), quality=90)
        tiles.append(name)
    return tiles


def contact_sheet():
    files = sorted(f for f in os.listdir(PREV) if f.endswith(".jpg") and f != "contact.jpg")
    if not files:
        return None
    cols = 4
    cw, ch = 448, 192 + 18
    rows = (len(files) + cols - 1) // cols
    sheet = Image.new("RGB", (cols * cw, rows * ch), (8, 8, 8))
    from PIL import ImageDraw
    d = ImageDraw.Draw(sheet)
    for i, f in enumerate(files):
        im = Image.open(os.path.join(PREV, f)).convert("RGB")
        im = im.resize((448, 192), Image.LANCZOS)
        x, y = (i % cols) * cw, (i // cols) * ch
        sheet.paste(im, (x, y + 18))
        d.text((x + 4, y + 2), f[:-4], fill=(230, 230, 230))
    p = os.path.join(PREV, "contact.jpg")
    sheet.save(p, quality=88)
    return p


# ================================================================================================================ driver

def load_configs():
    units = []          # (key, owner, kind, cfg, array_cfg or None, config file)
    seen = {}
    for name in CONFIGS:
        p = os.path.join(CFG_DIR, name)
        if not os.path.exists(p):
            log("config %s missing (skipped)" % name)
            continue
        cfg = json.load(open(p, encoding="utf-8"))
        owner = cfg.get("owner", "?")
        for key, u in (cfg.get("sets") or {}).items():
            if key in seen:
                raise TexprepError("key %s in both %s and %s" % (key, seen[key], name))
            seen[key] = name
            kind = "leaves" if "compose" in u else "set"
            units.append((key, owner, kind, u, None, name))
        for key, a in (cfg.get("arrays") or {}).items():
            if key in seen:
                raise TexprepError("key %s in both %s and %s" % (key, seen[key], name))
            seen[key] = name
            units.append((key, owner, "array", a, a, name))
    return units


def source_hashes(unit_or_array):
    """SHA-256 of every raw input of a unit / array (staleness)."""
    srcs = [s for s in ([unit_or_array] if "source" in unit_or_array else unit_or_array.get("slices", []))]
    out = []
    for s in srcs:
        for _, p in sorted(find_maps(s["source"], s).items()):
            out.append(sha256(p))
    return out


def build_hash(key, cfg, extra_files):
    h = hashlib.sha256()
    h.update(json.dumps({"key": key, "cfg": cfg, "texprep": VERSION}, sort_keys=True).encode("utf-8"))
    for x in extra_files:
        h.update(x.encode("utf-8"))
    return h.hexdigest()


def compose_version():
    p = os.path.join(CFG_DIR, "foliage_compose.py")
    return sha256(p) if os.path.exists(p) else ""


def unit_build_hash(key, kind, cfg):
    extra = source_hashes(cfg)
    if kind == "leaves":
        extra.append(compose_version())
    return build_hash(key, cfg, extra)


def is_current(key, maps_list, bh):
    mp = os.path.join(OUT, key + ".texmeta.json")
    if not os.path.exists(mp):
        return False, "missing"
    try:
        meta = json.load(open(mp, encoding="utf-8"))
    except Exception as e:  # noqa: BLE001
        return False, "unreadable texmeta (%s)" % e
    if meta.get("placeholder"):
        return False, "placeholder"
    if meta.get("buildHash") != bh:
        return False, "stale (inputs, config or texprep changed)"
    for o in meta.get("outputs", []):
        p = os.path.join(ROOT, o["path"])
        if not os.path.exists(p):
            return False, "output missing: " + o["path"]
        if sha256(p) != o["sha256"]:
            return False, "output edited after texprep: " + o["path"]
    return True, "current"


def base_meta(key, owner, kind, cfg, res, maps_list):
    return {"version": VERSION, "key": key, "owner": owner, "kind": "array" if kind == "array" or kind == "leaves" else "set",
            "res": int(res), "maps": list(maps_list), "cols": 1, "rows": 1, "registration": "", "texprepVersion": VERSION}


def run_unit(key, owner, kind, cfg, toks, finals, previews):
    if kind == "array":
        res = int(cfg["res"])
        cols, rows = (int(x) for x in cfg.get("flipbook", [1, 1]))
        maps_list = cfg.get("maps", ["Albedo", "Normal", "Mask"])
        akind = cfg.get("kind", "set")
        datas, metas = [], []
        for s in cfg["slices"]:
            log("  %s slice %s %s" % (key, s.get("slice"), s.get("name", s["source"])))
            if akind == "overlay":
                d, m = process_overlay(s, res, finals)
            elif akind == "micro":
                d, m = process_micro(s, res, finals)
            else:
                d, m = process_unit(s, res, toks, finals, kind="slice")
            datas.append(d)
            metas.append(m)
        if len(datas) > cols * rows:
            raise TexprepError("%s: %d slices do not fit a %d x %d flipbook" % (key, len(datas), cols, rows))
        data = {}
        for name, ch in (("albedo", 3), ("normal", 3), ("mask", 4)):
            if all(name in d for d in datas):
                sl = list(d[name] for d in datas)
                while len(sl) < cols * rows:
                    sl.append(sl[-1])
                data[name] = flipbook(sl, cols, rows, res, ch)
        meta = base_meta(key, owner, kind, cfg, res, maps_list)
        meta.update({"cols": cols, "rows": rows, "source": "", "slices": [m["source"] for m in metas],
                     "sliceNames": [s.get("name", "") for s in cfg["slices"]],
                     "inputs": [i for m in metas for i in m["inputs"]], "stats": metas[0]["stats"],
                     "sliceStats": [m["stats"] for m in metas], "sliceTile": [m["tile"] for m in metas],
                     "sliceTileV": [m.get("tileV", 0.0) for m in metas], "sliceTileRef": [m["tileRef"] for m in metas],
                     "sliceModuleU": [float(m.get("moduleU", 0.0)) for m in metas],
                     "sliceModuleV": [float(m.get("moduleV", 0.0)) for m in metas],
                     "sliceRegistration": [m.get("registration", "") for m in metas],
                     "tile": metas[0]["tile"], "tileV": metas[0].get("tileV", 0.0), "tileRef": metas[0]["tileRef"],
                     "aniso": anisotropy_for(key, cfg, "array"), "gridded": (cfg.get("mipFilter", "box") == "box"),
                     "alpha": False, "alphaRef": 0.5, "cls": akind, "antiTiling": "panel" if key == "FacadeArr" else "",
                     "sliceSquaring": [m.get("squaring") for m in metas]})
        meta["sliceTileV"] = [v for v in meta["sliceTileV"]]
    elif kind == "leaves":
        res = int(cfg.get("res", 2048))
        maps_list = ["Albedo", "Normal", "Mask"]
        data, m = process_leaves(cfg, res, toks, finals)
        meta = base_meta(key, owner, kind, cfg, res // int(cfg.get("flipbook", [2, 2])[0]), maps_list)
        meta.update(m)
        meta.update({"kind": "array", "alpha": True, "alphaRef": float(cfg.get("alphaRef", 0.5)), "gridded": True, "cls": "leaves",
                     "aniso": anisotropy_for(key, cfg, "set"), "antiTiling": cfg.get("antiTiling", "none"), "rotate": False,
                     "metalFromMap": False, "res": res // int(cfg.get("flipbook", [2, 2])[0])})
    else:
        res = int(cfg.get("res", 2048))
        maps_list = ["Albedo", "Normal", "Mask"]
        data, m = process_unit(cfg, res, toks, finals, kind="set")
        meta = base_meta(key, owner, kind, cfg, res, maps_list)
        meta.update(m)
        meta.update({"cls": cfg.get("class", ""), "antiTiling": cfg.get("antiTiling", "hex"), "rotate": bool(cfg.get("rotate", False)),
                     "metalFromMap": bool(cfg.get("metalFromMap")) and cfg["source"] in METAL_ALLOW,
                     "gridded": gridded(cfg), "alpha": "alpha" in data, "alphaRef": 0.5,
                     "aniso": anisotropy_for(key, cfg, "set"), "flatten": float(cfg.get("flatten", 0) or 0)})
        if "moduleU" not in meta:
            meta["moduleU"], meta["moduleV"] = 0.0, 0.0
        if "registration" not in meta:
            meta["registration"] = ""
    tok_name = cfg.get("token") or CLASS_TOKEN.get(cfg.get("class", ""), "ConcreteDark")
    meta["previewToken"] = [float(x) for x in toks.get(tok_name, np.array([0.18, 0.18, 0.18]))]
    if kind == "array":
        meta["previewTokens"] = [[float(x) for x in toks.get(s.get("token") or CLASS_TOKEN.get(s.get("class", ""), "ConcreteDark"),
                                                              np.array([0.18, 0.18, 0.18]))] for s in cfg["slices"]]
    meta["buildHash"] = unit_build_hash(key, kind, cfg)
    write_outputs(key, maps_list, data, meta, previews)
    return meta


def write_placeholder(key, owner, kind, cfg, finals):
    """64 px neutral maps (albedo 0.5 linear, flat normal, mask (0, 1, 0.5, mid smooth)) so materials build before TP-1."""
    res = 64
    if kind == "array":
        cols, rows = (int(x) for x in cfg.get("flipbook", [1, 1]))
        maps_list = cfg.get("maps", ["Albedo", "Normal", "Mask"])
    elif kind == "leaves":
        cols, rows = (int(x) for x in cfg.get("flipbook", [2, 2]))
        maps_list = ["Albedo", "Normal", "Mask"]
    else:
        cols, rows = 1, 1
        maps_list = ["Albedo", "Normal", "Mask"]
    W, H = res * cols, res * rows
    data = {"albedo": np.full((H, W, 3), 0.5, np.float32), "normal": np.dstack([np.zeros((H, W)), np.zeros((H, W)), np.ones((H, W))]).astype(np.float32)}
    sm = cfg.get("smooth") or [0.2, 0.4]
    m = np.zeros((H, W, 4), np.float32)
    m[..., 1] = 1.0
    m[..., 2] = 0.5
    m[..., 3] = 0.5 * (sm[0] + sm[1])
    data["mask"] = m
    if kind == "leaves" or cfg.get("opacity"):
        data["alpha"] = np.full((H, W), 1.0, np.float32)
    meta = base_meta(key, owner, kind, cfg, res, maps_list)
    srcs = [cfg["source"]] if "source" in cfg else [s["source"] for s in cfg.get("slices", [])]
    tiles = [float(cfg.get("tile", finals.get(cfg.get("source", ""), 1.0)))] if "source" in cfg else [float(s.get("tile", 1.0)) for s in cfg.get("slices", [])]
    meta.update({"placeholder": True, "cols": cols, "rows": rows, "tile": tiles[0], "tileV": 0.0,
                 "tileRef": finals.get(srcs[0], tiles[0]) if srcs else tiles[0], "source": srcs[0] if len(srcs) == 1 and kind != "array" else "",
                 "slices": srcs if kind == "array" else [], "sliceTile": tiles if kind == "array" else [],
                 "alpha": "alpha" in data, "alphaRef": 0.5, "gridded": kind == "leaves" or gridded(cfg), "aniso": anisotropy_for(key, cfg, kind),
                 "antiTiling": cfg.get("antiTiling", ""), "cls": cfg.get("class", cfg.get("kind", "")), "moduleU": 0.0, "moduleV": 0.0,
                 "registration": "placeholder", "stats": {"mean": [0.5, 0.5, 0.5]}})
    write_outputs(key, maps_list, data, meta, False)


def main(argv):
    ap = argparse.ArgumentParser(description="Look v2 texture pipeline (TEXTURE_PLAN 1.4)")
    ap.add_argument("--only", default="")
    ap.add_argument("--check", action="store_true")
    ap.add_argument("--previews", action="store_true")
    ap.add_argument("--placeholders", action="store_true")
    ap.add_argument("--force", action="store_true")
    ap.add_argument("--list", action="store_true")
    a = ap.parse_args(argv)
    finals = final_sets()
    toks = palette()
    units = load_configs()
    for key, owner, kind, cfg, arr, name in units:
        srcs = [cfg["source"]] if "source" in cfg else [s["source"] for s in cfg.get("slices", [])]
        for s in srcs:
            if s not in finals:
                raise TexprepError("%s (%s): source %s is not a TEX-R final set" % (key, name, s))
    if a.list:
        for key, owner, kind, cfg, arr, name in units:
            print("%-14s %-4s %-7s %s" % (key, owner, kind, name))
        return 0
    only = set(x for x in a.only.split(",") if x)
    unknown = only - set(u[0] for u in units)
    if unknown:
        raise TexprepError("--only: unknown key(s) " + ", ".join(sorted(unknown)))
    sel = [u for u in units if not only or u[0] in only]
    if a.check:
        bad = 0
        for key, owner, kind, cfg, arr, name in sel:
            maps_list = cfg.get("maps", ["Albedo", "Normal", "Mask"]) if kind == "array" else ["Albedo", "Normal", "Mask"]
            ok, why = is_current(key, maps_list, unit_build_hash(key, kind, cfg))
            print("%-14s %s" % (key, "OK" if ok else "STALE: " + why))
            bad += 0 if ok else 1
        print("texprep --check: %d of %d current" % (len(sel) - bad, len(sel)))
        return 1 if bad else 0
    if a.placeholders:
        n = 0
        for key, owner, kind, cfg, arr, name in sel:
            if os.path.exists(os.path.join(OUT, key + ".texmeta.json")) and not a.force:
                continue
            write_placeholder(key, owner, kind, cfg, finals)
            n += 1
        log("%d placeholder set(s) written" % n)
        return 0
    failures = []
    for key, owner, kind, cfg, arr, name in sel:
        maps_list = cfg.get("maps", ["Albedo", "Normal", "Mask"]) if kind == "array" else ["Albedo", "Normal", "Mask"]
        bh = unit_build_hash(key, kind, cfg)
        ok, why = is_current(key, maps_list, bh)
        if ok and not a.force:
            log("%s: current" % key)
            if a.previews:
                pass
            continue
        log("%s (%s, %s): %s" % (key, owner, kind, why))
        try:
            meta = run_unit(key, owner, kind, cfg, toks, finals, a.previews)
            st = meta.get("stats", {})
            log("%s: tile %.3f%s, std/mean %s, sat %s, violet %s, bake %s -> %s, seam %s" % (
                key, meta.get("tile", 0), (" x %.3f" % meta["tileV"]) if meta.get("tileV") else "", st.get("stdOverMean"),
                st.get("saturation"), st.get("violetShare"), st.get("bakeBefore"), st.get("bake"), st.get("seamScore")))
        except TexprepError as e:
            failures.append("%s: %s" % (key, e))
            log("FAIL " + failures[-1])
    if a.previews:
        p = contact_sheet()
        if p:
            log("contact sheet " + os.path.relpath(p, ROOT))
    if failures:
        log("%d failure(s):\n  " % len(failures) + "\n  ".join(failures))
        return 1
    return 0


if __name__ == "__main__":
    try:
        sys.exit(main(sys.argv[1:]))
    except TexprepError as e:
        log("ABORT " + str(e))
        sys.exit(2)
