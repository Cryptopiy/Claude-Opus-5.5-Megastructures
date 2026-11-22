#!/usr/bin/env python3
"""qa_common: shared library of the Nocturne offline QA tools (imported from Thread, IMPORT_PLAN 1.8) (MASTER_PROMPT Part 11.3, 11.6, 11.9, 11.12, 14.10).

Run every tool with  python -I Tools/qa/<tool>.py ...  (standard library + numpy + Pillow only; ffmpeg on PATH
for encode_test).  The tools add their own folder to sys.path, because -I does not.

----------------------------------------------------------------------------------------------------------------
INPUT CONTRACT (what the runtime capture harness writes; every field is optional unless marked REQUIRED)
----------------------------------------------------------------------------------------------------------------
<dir>/report.json
    { "run": "<run-id>", "build": "player|editor", "shots": [ SHOT, ... ] }
    (the list may also be called "images" / "captures", or report.json may be the bare list)
  SHOT
    "file"     REQUIRED  PNG path relative to <dir> (aliases: "path", "png", "image")
    "tag"                e.g. "KS04_boulevard_axis", "KS04_boulevard_axis_4k" or "beat02_boulevard_mid" (default: file stem)
    "width", "height"    the size the harness REQUESTED (aliases "w"/"h", "size": [w, h], "res": "WxH")
    "s"                  plan distance (m);  "beat": 1..7 (default: parsed from "beatNN" in the tag)
    "pose"               {"x","y","z","yaw","pitch","fov"} or [x, y, z, yaw, pitch]; yaw = world azimuth from +Z
                         toward +X, pitch up positive, degrees.  "fov" = VERTICAL fov in degrees (alias "vfov").
    "lumaRange"          [min, max] expected mean display luma (default: the 14.10.2 table by tag)
    "horizonRow"         pixel row (float, top = 0) of the horizon line, if the harness knows it better than the
                         flat-world formula  y = H/2 + f tan(pitch)
    "subjects"           [ {"name": "goal_window", "box": [x0, y0, x1, y1] (pixels, top-left origin),
                            "margin": 0.03, "maxCropFrac": 0.0, "minWidthFrac": 0.25} , ... ]
                         or {"goal_window": [x0, y0, x1, y1], ...}.  Framing check: box inside the frame with the margin
                         (fraction of the frame size), or at most maxCropFrac of the box width/height cropped.
    "regions"            same structure as one entry of a regions file (below); coordinates default to the
                         image's own pixels (no scaling) unless "ref" or "units" is given.

<dir>/seq_<name>/frame_NNNNN.png + <dir>/seq_<name>/sequence.json
    { "name", "kind": "boundary|static|pan|edge|encode|signs|rain|koi|traffic" (ShotPlan, 14.10 / 14.12), "fps": 30,
      "frames": [{"s": ...}, ...] or "s0"/"s1", "boundaryS": [beat boundaries crossed], "freezeClock": bool,
      "freezeDither": bool, "rainOverride": x|null (-rain X, D-020), "rho": x, "encode": bool (encode_test picks it up), "lumaStepMax",
      "meanDiffMax", "maxPixelDiff", "spikeRatio", "lines": [LINE...], "stability": [ {"rect": [...], "tol", "window"} ],
      "banding": [BAND...] }

Audio: <file>.wav (16/24/32-bit PCM or 32-bit float, any rate) + sidecar <file>.json (or <file>.wav.json,
<file>_s.json):  {"s": [s at t = t0 + i*dt], "dt": 1.0, "t0": 0.0, "beat": N (whole file is beat N),
                  "stem": "mix|...", "speed": m/s, "footsteps": [onset seconds]}
       (AudioRenderRunner, -render-audio, writes <file>.wav + <file>.json with "s" every second)

Regions file (default Tools/qa/regions_default.json):
    { "regions": [ { "match": "<fnmatch pattern on the tag>", "aspect": "16:9|16:10|21:9|any",
                     "ref": [1920, 1080], "banding": [BAND...], "lines": [LINE...], "horizon": [HZ...],
                     "stats": [STAT...], "exclude": [EXCL...] }, ... ] }
  Coordinates are pixels at "ref" (default 1920 x 1080) and are scaled per axis to the image size; a region with
  "units": "frac" uses 0..1 fractions, "units": "px" uses the image's own pixels, "units": "frac169" fractions of the 16:9
  frame projected to the image's aspect at the same vertical FOV (one rect serves 16:9, 16:10 and 21:9).
  BAND  {"name", "rect": [x0, y0, x1, y1], "dir": "v|h|auto|radial", "center": [x, y], "r0", "r1", "a0", "a1", "gate": "steps"}
        dir "v": the gradient runs along y (strips are 64 px wide columns); "radial": polar resampling (nearest)
        gate "steps" (textured surfaces, QA-6): the 14.7 H1 visible-step gate alone decides, and a step counts only when it
        continues into a neighbouring strip (same channel and sign within band_step_gap_px rows: a quantisation contour runs
        across the region, a paving joint or reflection edge does not); the BandScore (run length x slope) measures the
        texture's own plateaus there and is reported, not gated
  LINE  {"name", "x", "tol": 32, "y0", "h": 256, "w": 64, "widthMin1080", "widthMax1080", "contrastMin"}
  HZ    {"name", "a": rect, "b": rect, "tol": 0.01}  (median colour of a vs b, per channel, 8-bit sRGB)
  STAT  {"name", "rect", "units", "stdMax", "meanMin", "meanMax", "scale": "display|linear"}
  EXCL  {"name", "ndc": [x, y] | "px": [x, y] | "azEl": [az, el] | "world": [x, y, z], "clipDeg": 1.2,
         "bandDeg": 4.0, "coreDeg": 0.9, "coreMax": 0.98}  (neon masks: clipped / isolated-pixel exclusions)
----------------------------------------------------------------------------------------------------------------
Thresholds come from MASTER_PROMPT 14.10, 14.7 H1 and 14.3 (TH below, each with its spec source).  Scene numbers (beat boundaries, key shots,
audio targets and windows) are parsed from the single source of truth  Assets/_Game/Scripts/Runtime/Core/
NocturneConstants.cs;  the literals in _FALLBACK are used only when that file cannot be parsed (reported as such).
"""
import fnmatch
import json
import math
import os
import re
import sys

import numpy as np

try:
    from PIL import Image
except ImportError:  # pragma: no cover
    Image = None

HERE = os.path.dirname(os.path.abspath(__file__))
ROOT = os.path.abspath(os.path.join(HERE, "..", ".."))
REPORTS_DIR = os.path.join(ROOT, "QA", "reports")
DEFAULT_REGIONS = os.path.join(HERE, "regions_default.json")
CONSTANTS_CS = os.path.join(ROOT, "Assets", "_Game", "Scripts", "Runtime", "Core", "NocturneConstants.cs")

PASS, FAIL, SKIP, INFO, WARN = "PASS", "FAIL", "SKIP", "INFO", "WARN"

# --------------------------------------------------------------------------------------------- thresholds (14.10.3)
TH = {
    # 14.10 per-beat table: clipped (any channel >= 254) beats 1-2 1.8 %, 3-5 0.8 %, 6 1.0 %, 7 0.8 %
    "clip_pct_max": {1: 1.8, 2: 1.8, 3: 0.8, 4: 0.8, 5: 0.8, 6: 1.0, 7: 0.8},
    "clip_code": 253.5,                   # 14.10: "any channel >= 254"
    # 14.10: 1st percentile of luma >= 9 codes in every beat (9 / 255 = 0.0353)
    "p1_min": {1: 0.0353, 2: 0.0353, 3: 0.0353, 4: 0.0353, 5: 0.0353, 6: 0.0353, 7: 0.0353},
    # 14.10 mean-luma windows (codes / 255; initial, tightened after ten measurements)
    "mean_luma": {1: (28 / 255, 70 / 255), 2: (40 / 255, 90 / 255), 3: (30 / 255, 80 / 255), 4: (28 / 255, 75 / 255),
                  5: (24 / 255, 64 / 255), 6: (50 / 255, 110 / 255), 7: (34 / 255, 85 / 255)},
    # 14.7 H1 (6): confirmed visible steps = >= 1.0 code between plateaus each >= 24 px after a 9 px box smoothing, in luma,
    # Cb and Cr; 0 per 1000 px on 8-bit captures, <= 2 per 1000 px and none > 1.5 codes on the CRF 23 decode
    "band_strip_px": 64, "band_box": 9, "band_plateau_px": 24, "band_step_code": 1.0,
    "band_flat_diff": 0.05, "band_plateau_range": 0.3, "band_step_gap_px": 24,
    "band_steps_per_1000_max": 0.0, "band_steps_per_1000_encoded_max": 2.0, "band_step_max_code": 1.5,
    # 11.3 BandScore: longest run of identical 8-bit luma / (1 / slope) over regions with slope >= 0.02 level/px, <= 1.5
    "bandscore_min_slope": 0.02, "bandscore_max": 1.5,
    # 14.10 aliasing regions: thin-line probes (generic; the regions arrive with the beats)
    "line_jitter_max_px": 0.1, "line_energy_var_max": 0.10, "line_monotone_rows_min": 0.95,
    "line_stairstep_max_px": 0.25, "line_valid_rows_min": 0.5,
    # 14.7 H3 / Part 10 item 13: horizon agreement (fog vs sky vs dome), max channel difference
    "horizon_tol": 0.01,
    # 14.10: black / white / magenta 0 / NaN-like 0 / isolated pixels above 8 x the 5 x 5 median: 0 outside neon masks
    "black_code": 3, "white_code": 252, "flat_frac": 0.98,
    # NaN-like: an isolated pixel this far (display luma) below the min / above the max of its 24 neighbours; the dark limit is
    # 0.10 (Thread's 0.30 assumed day frames: a black NaN in Nocturne's luma-50 night frames is only 0.2 below)
    "nan_dark_t": 0.10, "nan_bright_t": 0.30, "isolated_factor": 8.0,
    # neon masks of the isolated-pixel test (image_metrics.neon_mask): emitter = (display luma >= 0.40, or saturation >= 0.5
    # with luma >= 0.12) and >= 2 x the 5 x 5 median (the magenta sky never qualifies); seeds need >= 3 emitter pixels in
    # their 3 x 3 (a lone firefly is never masked); dilated 2 + 1 px
    "neon_luma_min": 0.40, "neon_sat_min": 0.5, "neon_sat_luma_min": 0.12, "neon_contrast": 2.0, "neon_cluster_min": 3,
    "neon_dilate_px": 2,
    # missing-shader magenta: R and B >= 170 and G <= 0.15 min(R, B) (CloudHot #B03A8C = 176, 58, 140 never passes)
    "magenta_rb_min": 170, "magenta_g_frac": 0.15,
    # 14.10 Sequences: mean-luma step <= 1.2 levels per frame, activation-frame difference <= 0.5 level (above the running
    # median), no difference spike above 3 x the running median, static change <= 2 levels outside moving-object masks
    "seq_luma_step_max": 1.2 / 255.0, "spike_ratio": 3.0, "spike_floor_grey": 0.25, "spike_window": 31,
    "activation_diff_max": 0.5, "static_change_max": 2.0,
    # 14.7 H5 rain flicker (static camera, rho 1): streak mask = integer luma > per-pixel temporal minimum + 1.5 codes,
    # dilated 2 px; per-pixel change outside the masks of two consecutive frames <= 6 levels
    "rain_streak_codes": 1.5, "rain_mask_dilate_px": 2, "rain_outside_change_max": 6,
    # 14.10 shimmer: per-pixel temporal std over the aliasing region <= 2.0 levels above the 4x supersampled reference
    "shimmer_std_max": 2.0,
    # generic per-sequence "stability" crops (sequence.json option)
    "stability_tol": 0.12, "stability_window": 5, "stability_floor": 0.05,
    # 14.3: median of the 3 s RMS windows outside the S-windows within +-1.5 dB of the Sum column; true peak <= -1 dBFS;
    # DC < 0.001; sample step <= 0.25 (footsteps and the tram bell excepted)
    "audio_rms_window_s": 3.0, "audio_rms_tol_db": 1.5, "audio_peak_max_dbfs": -1.0, "audio_dc_max": 0.001,
    "audio_step_max": 0.25, "audio_lufs_info_tol": 3.0,
    # 14.3 swells logged separately (S3, S4, S6, S7: s ranges)
    "audio_s_windows": [(400.0, 420.0), (540.0, 554.0), (876.0, 896.0), (1340.0, 1380.0)],
}

# Literal copies used ONLY when NocturneConstants.cs cannot be parsed (spec lines in comments).
_FALLBACK = {
    "BeatBoundaries": [0, 120, 400, 560, 880, 1080, 1260, 1500],                    # 14.12 BEAT_S
    "BeatNames": ["alley", "boulevard", "edge", "skybridge", "garden", "signs", "heart"],
    "TargetRmsDb": [-34.9, -30.4, -31.3, -29.6, -33.3, -30.1, -28.9],              # 14.12 audio RMS per beat
    "KeyShots": [  # 14.10 key-shot table: tag, s, x, y, z, yaw, pitch, fov, lumaMin, lumaMax (beat window / 255), wide
        ("KS00_alley_start", 0, 0, 1.7, 0, 0, -2, 62, 28 / 255, 70 / 255, False),
        ("KS04_boulevard_axis", 250, 65.30, 1.7, 195, 90, 22, 66, 40 / 255, 90 / 255, False),
        ("KS10_edge_plunge", 548, 363.30, 1.7, 195, 20, -30, 68, 30 / 255, 80 / 255, False),
        ("KS16_signs_wall", 1150, 901.74, 1.7, 271.44, 325, 34, 66, 50 / 255, 110 / 255, False),
        ("KS17_heart_reveal", 1296, 944.18, 1.7, 387.88, 96, 18, 66, 34 / 255, 85 / 255, False),
        ("KS19_goal_window", 1500, 1148.18, 1.7, 387.88, 96, 28, 62, 34 / 255, 85 / 255, False),
    ],
}


class Spec(object):
    """Scene numbers parsed from NocturneConstants.cs (falls back to _FALLBACK, see .source)."""

    def __init__(self, path=CONSTANTS_CS):
        self.source = "fallback literals (NocturneConstants.cs not parsed)"
        self.warnings = []
        vals = dict(_FALLBACK)
        try:
            with open(path, "r", encoding="utf-8") as f:
                text = f.read()
            parsed = {}
            for name in ("BeatBoundaries", "TargetRmsDb"):
                m = re.search(r"\b%s\s*=\s*\{([^}]*)\}" % name, text)
                if m:
                    parsed[name] = [float(v) for v in m.group(1).split(",") if v.strip()]
            m = re.search(r"\bBeatNames\s*=\s*\{([^}]*)\}", text)
            if m:
                parsed["BeatNames"] = re.findall(r'"([^"]*)"', m.group(1))
            shots = []
            for m in re.finditer(r'new\s+KeyShot\(\s*"([^"]+)"\s*,([^)]*)\)', text):
                parts = [p.strip() for p in m.group(2).split(",")]
                nums = [float(p.rstrip("f")) for p in parts[:9]]
                wide = len(parts) > 9 and parts[9].lower() == "true"
                shots.append(tuple([m.group(1)] + nums + [wide]))
            if shots:
                parsed["KeyShots"] = shots
            missing = [k for k in _FALLBACK if k not in parsed]
            vals.update(parsed)
            if missing:
                self.warnings.append("NocturneConstants.cs: not found, fallback used for " + ", ".join(missing))
            self.source = "NocturneConstants.cs" + (" (partial)" if missing else "")
        except (OSError, ValueError) as e:
            self.warnings.append("NocturneConstants.cs unreadable: %s" % e)
        self.beat_boundaries = [float(v) for v in vals["BeatBoundaries"]]
        self.beat_names = list(vals["BeatNames"])
        self.target_rms_db = [float(v) for v in vals["TargetRmsDb"]]
        self.s_windows = [tuple(w) for w in TH["audio_s_windows"]]
        self.keyshots = {}
        for t in vals["KeyShots"]:
            self.keyshots[t[0]] = {"tag": t[0], "s": t[1], "pos": (t[2], t[3], t[4]), "yaw": t[5], "pitch": t[6],
                                   "fov": t[7], "luma": (t[8], t[9]), "wide": bool(t[10])}

    def beat_of_s(self, s):
        b = self.beat_boundaries
        for k in range(len(b) - 1):
            if s < b[k + 1]:
                return k + 1
        return len(b) - 1

    def keyshot_for_tag(self, tag):
        """Longest key-shot tag that prefixes `tag` at a word boundary ("KS04_boulevard_axis_4k" -> KS04)."""
        best = None
        for kt in self.keyshots:
            if tag == kt or (tag.startswith(kt) and not tag[len(kt)].isalnum()):
                if best is None or len(kt) > len(best):
                    best = kt
        return self.keyshots[best] if best else None


_SPEC = None


def spec():
    global _SPEC
    if _SPEC is None:
        _SPEC = Spec()
    return _SPEC


# --------------------------------------------------------------------------------------------- small utilities
def beat_from_tag(tag):
    m = re.search(r"beat0*([1-9])", tag or "", re.IGNORECASE)
    return int(m.group(1)) if m else None


def run_id_for(path, explicit=None):
    if explicit:
        return explicit
    p = os.path.abspath(path)
    if os.path.isfile(p):
        p = os.path.dirname(p)
    return os.path.basename(p.rstrip("\\/")) or "run"


def ensure_dir(p):
    if p:
        os.makedirs(p, exist_ok=True)
    return p


def sanitize(o):
    """JSON-safe copy: NaN/inf -> None, numpy scalars/arrays -> Python."""
    if isinstance(o, dict):
        return {str(k): sanitize(v) for k, v in o.items()}
    if isinstance(o, (list, tuple)):
        return [sanitize(v) for v in o]
    if isinstance(o, np.ndarray):
        return sanitize(o.tolist())
    if isinstance(o, (np.floating, float)):
        v = float(o)
        return v if math.isfinite(v) else None
    if isinstance(o, (np.integer,)):
        return int(o)
    if isinstance(o, np.bool_):
        return bool(o)
    return o


def write_json(path, obj):
    ensure_dir(os.path.dirname(path))
    with open(path, "w", encoding="utf-8") as f:
        json.dump(sanitize(obj), f, indent=1)
    return path


def write_text(path, text):
    ensure_dir(os.path.dirname(path))
    with open(path, "w", encoding="utf-8", newline="\n") as f:
        f.write(text)
    return path


def read_json(path, default=None):
    try:
        with open(path, "r", encoding="utf-8-sig") as f:
            return json.load(f)
    except (OSError, ValueError):
        return default


def fmt(v, nd=3):
    if v is None:
        return "-"
    if isinstance(v, bool):
        return "yes" if v else "no"
    if isinstance(v, (int, np.integer)):
        return str(int(v))
    if isinstance(v, (float, np.floating)):
        if not math.isfinite(float(v)):
            return str(v)
        return ("%." + str(nd) + "f") % float(v)
    if isinstance(v, (list, tuple)):
        return "[" + ", ".join(fmt(x, nd) for x in v) + "]"
    return str(v)


def md_escape(s):
    return str(s).replace("|", "\\|").replace("\n", " ")


def md_table(headers, rows):
    out = ["| " + " | ".join(md_escape(h) for h in headers) + " |",
           "|" + "|".join("---" for _ in headers) + "|"]
    for r in rows:
        out.append("| " + " | ".join(md_escape(c) for c in r) + " |")
    return "\n".join(out) + "\n"


def check(name, status, value=None, threshold=None, note=None):
    return {"name": name, "status": status, "value": value, "threshold": threshold, "note": note}


def worst_status(checks):
    sts = [c["status"] for c in checks]
    if FAIL in sts:
        return FAIL
    if PASS in sts:
        return PASS if WARN not in sts else WARN
    if WARN in sts:
        return WARN
    return SKIP if sts else SKIP


def count_status(checks):
    out = {PASS: 0, FAIL: 0, SKIP: 0, INFO: 0, WARN: 0}
    for c in checks:
        out[c["status"]] = out.get(c["status"], 0) + 1
    return out


def add_tool_path():
    if HERE not in sys.path:
        sys.path.insert(0, HERE)


# --------------------------------------------------------------------------------------------- images and colour
_SRGB_LUT = None


def srgb_lut():
    """Code 0..255 -> linear (exact piecewise sRGB EOTF), float32."""
    global _SRGB_LUT
    if _SRGB_LUT is None:
        c = np.arange(256, dtype=np.float64) / 255.0
        lin = np.where(c <= 0.04045, c / 12.92, ((c + 0.055) / 1.055) ** 2.4)
        _SRGB_LUT = lin.astype(np.float32)
    return _SRGB_LUT


def load_rgb8(path):
    """PNG -> H x W x 3 uint8 (alpha dropped).  Returns (array, info dict)."""
    if Image is None:
        raise RuntimeError("Pillow is not installed: python -m pip install --user numpy pillow")
    with Image.open(path) as im:
        info = {"mode": im.mode, "size": im.size}
        if im.mode in ("I;16", "I;16B", "I"):
            a = np.asarray(im, dtype=np.float64)
            a = np.clip(np.round(a / 257.0), 0, 255).astype(np.uint8)
            a = np.stack([a, a, a], axis=-1)
        else:
            if im.mode == "RGBA":
                alpha = np.asarray(im.getchannel("A"))
                info["alpha_min"] = int(alpha.min())
            a = np.asarray(im.convert("RGB"), dtype=np.uint8)
    return np.ascontiguousarray(a), info


def luma_display(rgb8):
    """Rec.709 luma on the encoded (display) values, 0..1, float32."""
    a = rgb8.astype(np.float32)
    return (0.2126 * a[..., 0] + 0.7152 * a[..., 1] + 0.0722 * a[..., 2]) * (1.0 / 255.0)


def luma_linear(rgb8):
    """Rec.709 relative luminance of the sRGB-decoded (linear) values, 0..1, float32."""
    lut = srgb_lut()
    return 0.2126 * lut[rgb8[..., 0]] + 0.7152 * lut[rgb8[..., 1]] + 0.0722 * lut[rgb8[..., 2]]


def grey_levels(rgb8):
    """8-bit grey levels (display luma x 255, not rounded) used by the frame-difference tests."""
    return luma_display(rgb8) * 255.0


def save_png(path, arr):
    ensure_dir(os.path.dirname(path))
    Image.fromarray(np.ascontiguousarray(arr.astype(np.uint8))).save(path, optimize=False, compress_level=6)
    return path


# --------------------------------------------------------------------------------------------- camera model
class Cam(object):
    """Rectilinear pinhole camera: vertical fov (deg), yaw (azimuth from +Z toward +X), pitch (up +), no roll.
    Pixel coordinates are continuous: pixel (i, j) covers [i, i+1) x [j, j+1); y grows downward."""

    def __init__(self, W, H, fov, yaw=None, pitch=None, pos=None):
        self.W, self.H = int(W), int(H)
        self.fov = float(fov)
        self.yaw = None if yaw is None else float(yaw)
        self.pitch = None if pitch is None else float(pitch)
        self.pos = None if pos is None or any(v is None for v in pos) else np.array(pos, dtype=np.float64)
        self.f = (self.H * 0.5) / math.tan(math.radians(self.fov) * 0.5)

    def world_dir_to_cam(self, d):
        fwd, right, up = self.basis()
        d = np.asarray(d, dtype=np.float64)
        return np.array([d.dot(right), d.dot(up), d.dot(fwd)])

    def elevation_row(self, u, el_deg):
        """Row v (continuous) at pixel column(s) u where the view ray has world elevation el_deg (bisection; the
        elevation grows monotonically upward along a column for |pitch| < 90).  NaN where it is outside +-3 f."""
        if self.pitch is None:
            return None
        u = np.atleast_1d(np.asarray(u, dtype=np.float64))
        p, te = math.radians(self.pitch), math.tan(math.radians(el_deg))
        xc = (u - self.W * 0.5) / self.f
        lo, hi = np.full_like(xc, -3.0), np.full_like(xc, 3.0)

        def g(yc):  # wy - tan(e) * |horizontal|  (increasing in yc)
            return (yc * math.cos(p) + math.sin(p)) - te * np.sqrt(xc * xc + (math.cos(p) - yc * math.sin(p)) ** 2)
        glo, ghi = g(lo), g(hi)
        ok = (glo <= 0) & (ghi >= 0)
        for _ in range(48):
            mid = 0.5 * (lo + hi)
            gm = g(mid)
            lo = np.where(gm < 0, mid, lo)
            hi = np.where(gm < 0, hi, mid)
        yc = 0.5 * (lo + hi)
        v = self.H * 0.5 - self.f * yc
        return np.where(ok, v, np.nan)

    def cam_dir_px(self, u, v):
        """Camera-space direction (x right, y up, z forward), unnormalised, for pixel coordinates u, v."""
        return np.stack(np.broadcast_arrays((np.asarray(u, dtype=np.float64) - self.W * 0.5) / self.f,
                                            (self.H * 0.5 - np.asarray(v, dtype=np.float64)) / self.f,
                                            np.ones_like(np.asarray(u, dtype=np.float64))), axis=-1)

    def ndc_to_px(self, nx, ny):
        return ((nx + 1.0) * 0.5 * self.W, (1.0 - ny) * 0.5 * self.H)

    def basis(self):
        y, p = math.radians(self.yaw or 0.0), math.radians(self.pitch or 0.0)
        fwd = np.array([math.sin(y) * math.cos(p), math.sin(p), math.cos(y) * math.cos(p)])
        right = np.array([math.cos(y), 0.0, -math.sin(y)])
        up = np.array([-math.sin(y) * math.sin(p), math.cos(p), -math.cos(y) * math.sin(p)])
        return fwd, right, up

    def azel_to_cam(self, az, el):
        """World azimuth/elevation (deg) -> camera-space unit direction (needs yaw/pitch)."""
        a, e = math.radians(az), math.radians(el)
        d = np.array([math.sin(a) * math.cos(e), math.sin(e), math.cos(a) * math.cos(e)])
        fwd, right, up = self.basis()
        return np.array([d.dot(right), d.dot(up), d.dot(fwd)])

    def project_cam(self, dcam):
        if dcam[2] <= 1e-9:
            return None
        return (self.W * 0.5 + self.f * dcam[0] / dcam[2], self.H * 0.5 - self.f * dcam[1] / dcam[2])

    def horizon_row(self):
        """Row (continuous) of the elevation-0 line at the image centre column (flat world, no dip)."""
        if self.pitch is None:
            return None
        return self.H * 0.5 + self.f * math.tan(math.radians(self.pitch))

    def angle_mask(self, dcam, radius_deg, shape=None):
        """Boolean H x W mask of pixels whose centre ray is within radius_deg of camera direction dcam."""
        H, W = (self.H, self.W) if shape is None else shape
        mask = np.zeros((H, W), dtype=bool)
        dn = np.asarray(dcam, dtype=np.float64)
        n = np.linalg.norm(dn)
        if n < 1e-12 or radius_deg <= 0:
            return mask
        dn = dn / n
        c = self.project_cam(dn)
        if c is None:
            return mask
        # conservative bounding box: off-axis stretch is at most 1/cos^2 of the field angle (< 4 within 60 deg)
        rpx = 4.0 * self.f * math.tan(math.radians(min(radius_deg, 80.0))) + 2.0
        x0, x1 = int(max(0, math.floor(c[0] - rpx))), int(min(W, math.ceil(c[0] + rpx)))
        y0, y1 = int(max(0, math.floor(c[1] - rpx))), int(min(H, math.ceil(c[1] + rpx)))
        if x1 <= x0 or y1 <= y0:
            return mask
        uu, vv = np.meshgrid(np.arange(x0, x1) + 0.5, np.arange(y0, y1) + 0.5)
        d = self.cam_dir_px(uu, vv)
        d /= np.linalg.norm(d, axis=-1, keepdims=True)
        cosang = np.clip(d.dot(dn), -1.0, 1.0)
        mask[y0:y1, x0:x1] = cosang >= math.cos(math.radians(radius_deg))
        return mask


def exclusion_dir(cam, ex, beat=None):
    """Camera-space direction of an exclusion entry, or None when it cannot be placed.
    Forms: "ndc": [x, y] (half-extents of this image, or of "ndcAspect": [w, h] at the same vertical fov),
    "px": [x, y], "azEl": [az, el] (needs yaw/pitch), "world": [x, y, z] (needs the camera position too), e.g. the
    goal lamp or a neon sign whose core may clip."""
    if cam is None:
        return None
    if "ndc" in ex:
        nx, ny = float(ex["ndc"][0]), float(ex["ndc"][1])
        asp = ex.get("ndcAspect")
        aspect = (float(asp[0]) / float(asp[1])) if asp else cam.W / float(cam.H)
        t = math.tan(math.radians(cam.fov) * 0.5)
        return np.array([nx * aspect * t, ny * t, 1.0])
    if "px" in ex:
        x, y = scale_xy(float(ex["px"][0]), float(ex["px"][1]), cam.W, cam.H, ex.get("_ref"), ex.get("_units"))
        return cam.cam_dir_px(x, y)
    if cam.yaw is None or cam.pitch is None:
        return None
    if "azEl" in ex:
        a = ex["azEl"]
        if isinstance(a, str):
            return None
        return cam.azel_to_cam(float(a[0]), float(a[1]))
    if "world" in ex:
        if cam.pos is None or isinstance(ex["world"], str):
            return None
        d = np.array([float(v) for v in ex["world"]]) - cam.pos
        if np.linalg.norm(d) < 1e-9:
            return None
        return cam.world_dir_to_cam(d)
    return None


def horizon_compare(rgb8, cam, item, row_override=None):
    """Horizon colour agreement on a constant-elevation line (elevation = -dipDeg): median colour of the pixel rows
    whose centres lie within (gap, gap + band] px above versus below the line (defaults 0.25 and 1 px at 1080p,
    i.e. the two rows adjacent to the horizon: the colour step the eye would see as a line), over the column
    fraction range "cols".  Returns a dict or None if not measurable."""
    H, W = rgb8.shape[:2]
    sc = H / 1080.0
    gap = float(item.get("gapPx", 0.25)) * sc
    band = max(1.0, float(item.get("bandPx", 1.0)) * sc)
    c0f, c1f = item.get("cols", [0.35, 0.65])
    cols = np.arange(int(c0f * W), max(int(c0f * W) + 1, int(c1f * W)))
    if row_override is not None:
        v = np.full(cols.shape, float(row_override))
    elif cam is not None and cam.pitch is not None:
        v = cam.elevation_row(cols + 0.5, -float(item.get("dipDeg", 0.0)))
    else:
        return None
    ok = np.isfinite(v) & (v - gap - band >= 0) & (v + gap + band <= H)
    if ok.sum() < 8:
        return None
    cols, v = cols[ok], v[ok]
    sky, gnd = [], []
    for c, vv in zip(cols, v):
        # rows by pixel centre (r + 0.5): above in [v - gap - band, v - gap), below in (v + gap, v + gap + band]
        a0, a1 = int(math.ceil(vv - gap - band - 0.5)), int(math.ceil(vv - gap - 0.5))
        b0, b1 = int(math.floor(vv + gap - 0.5)) + 1, int(math.floor(vv + gap + band - 0.5)) + 1
        a0, b1 = max(a0, 0), min(b1, H)
        sky.append(rgb8[a0:max(a1, a0 + 1), c].astype(np.float64).mean(axis=0))
        gnd.append(rgb8[b0:max(b1, b0 + 1), c].astype(np.float64).mean(axis=0))
    sky, gnd = np.array(sky), np.array(gnd)
    spread = lambda a: float((np.percentile(a, 90, axis=0) - np.percentile(a, 10, axis=0)).max())  # noqa: E731
    cs, cg = np.median(sky, axis=0), np.median(gnd, axis=0)
    return {"sky": cs, "ground": cg, "spread_sky": spread(sky), "spread_ground": spread(gnd),
            "row_mid": float(np.median(v)), "diff": float(np.abs(cs - cg).max() / 255.0), "n_cols": int(len(cols))}


# --------------------------------------------------------------------------------------------- regions
def aspect_ok(entry, W, H):
    a = entry.get("aspect", "16:9")
    if a in (None, "any", "*"):
        return True
    if isinstance(a, str) and ":" in a:
        p, q = a.split(":")
        want = float(p) / float(q)
    elif isinstance(a, (list, tuple)) and len(a) == 2:
        want = float(a[0]) / float(a[1])
    else:
        return True
    return abs((W / float(H)) / want - 1.0) <= 0.02


def scale_xy(x, y, W, H, ref=None, units=None):
    if units == "frac":
        return x * W, y * H
    if units == "frac169":
        # QA fix wave C47: a fraction of the 16:9 frame, projected to this aspect at the same vertical FOV (the key shots keep
        # their vFOV at 16:10 and 21:9): the same world point sits at x = 0.5 + (x169 - 0.5) * (16 / 9) * H / W, y unchanged
        return (0.5 + (x - 0.5) * (16.0 / 9.0) * H / float(W)) * W, y * H
    if units == "px" or ref is None:
        return x, y
    return x * W / float(ref[0]), y * H / float(ref[1])


def scale_rect(rect, W, H, ref=None, units=None):
    x0, y0 = scale_xy(float(rect[0]), float(rect[1]), W, H, ref, units)
    x1, y1 = scale_xy(float(rect[2]), float(rect[3]), W, H, ref, units)
    x0, x1 = sorted((x0, x1))
    y0, y1 = sorted((y0, y1))
    x0, y0 = int(max(0, math.floor(x0 + 1e-6))), int(max(0, math.floor(y0 + 1e-6)))
    x1, y1 = int(min(W, math.ceil(x1 - 1e-6))), int(min(H, math.ceil(y1 - 1e-6)))
    return x0, y0, x1, y1


def load_regions(path):
    """Regions file -> list of entries (see module doc).  Missing file -> []."""
    if not path:
        return []
    d = read_json(path, None)
    if d is None:
        return []
    if isinstance(d, dict):
        d = d.get("regions", [])
    return [e for e in d if isinstance(e, dict)]


def regions_for(tag, W, H, file_entries, shot_regions=None):
    """Merge every matching regions-file entry and the shot's own regions.  Each returned item carries its
    resolved scaling (_ref/_units) so that callers can convert coordinates."""
    out = {"banding": [], "lines": [], "horizon": [], "stats": [], "exclude": [], "options": {}}
    for e in file_entries:
        pats = e.get("match", "*")
        pats = pats if isinstance(pats, list) else [pats]
        if not any(fnmatch.fnmatchcase(tag, p) for p in pats):
            continue
        if not aspect_ok(e, W, H):
            continue
        ref = e.get("ref", [1920, 1080])
        _merge(out, e, ref, e.get("units"))
    if shot_regions:
        _merge(out, shot_regions, shot_regions.get("ref"), shot_regions.get("units", None if shot_regions.get("ref") else "px"))
    return out


def _merge(out, e, ref, units):
    for key in ("banding", "lines", "horizon", "stats", "exclude"):
        for item in e.get(key, []) or []:
            it = dict(item)
            it.setdefault("_ref", it.get("ref", ref))
            it.setdefault("_units", it.get("units", units))
            out[key].append(it)
    for k, v in (e.get("options", {}) or {}).items():
        out["options"][k] = v


def item_rect(it, key, W, H):
    return scale_rect(it[key], W, H, it.get("_ref"), it.get("_units"))


# --------------------------------------------------------------------------------------------- banding (14.7 H1, 11.3)
def _box(p, k):
    if k <= 1:
        return p.copy()
    ker = np.ones(k) / float(k)
    return np.stack([np.convolve(p[:, c], ker, mode="valid") for c in range(p.shape[1])], axis=1)


def _longest_run(mask):
    m = np.concatenate(([0], mask.astype(np.int8), [0]))
    d = np.diff(m)
    s, e = np.flatnonzero(d == 1), np.flatnonzero(d == -1)
    return int((e - s).max()) if s.size else 0


CHANNELS = ("R", "G", "B", "Y", "Cb", "Cr")
DECODE_STEP_CHANNELS = ("Y", "Cb", "Cr")     # D-041 (2): the CRF 23 decode gate counts visible steps in Y, Cb and Cr


def ycbcr(p):
    """BT.709 full-range Y, Cb, Cr of an ... x 3 array of codes (the channels of the 14.7 H1 gate)."""
    r, g, b = p[..., 0], p[..., 1], p[..., 2]
    y = 0.2126 * r + 0.7152 * g + 0.0722 * b
    return np.stack([y, (b - y) / 1.8556 + 128.0, (r - y) / 1.5748 + 128.0], axis=-1)


def plateaus(p1, th=TH):
    """Plateaus of a 9-px box-smoothed profile: maximal runs whose neighbour differences stay below band_flat_diff, at
    least band_plateau_px long and flat within band_plateau_range codes.  Returns [(start, end, level)]."""
    d = np.abs(np.diff(p1))
    out = []
    for a, b in _segments(d < th["band_flat_diff"], int(th["band_plateau_px"]) - 1):
        seg = p1[a:b + 1]
        if seg.max() - seg.min() <= th["band_plateau_range"]:
            out.append((a, b + 1, float(seg.mean())))
    return out


def visible_steps(p1, th=TH, where=None):
    """14.7 H1 confirmed visible steps of one channel: consecutive plateaus at most band_step_gap_px apart whose levels
    differ by >= band_step_code.  Returns (count, max step in codes); `where` (a list) receives (position between the two
    plateaus, sign) of each step, in the profile's own index."""
    pl = plateaus(p1, th)
    n, mx = 0, 0.0
    for (a0, a1, la), (b0, b1, lb) in zip(pl[:-1], pl[1:]):
        if b0 - a1 <= th["band_step_gap_px"] and abs(lb - la) >= th["band_step_code"]:
            n += 1
            mx = max(mx, abs(lb - la))
            if where is not None:
                where.append((0.5 * (a1 + b0), 1 if lb > la else -1))
    return n, mx


def strip_metrics(p, th=TH, luma8=None):
    """p: L x 3 profile in 8-bit code units (RGB mean across the strip width); luma8: L x w integer luma of the strip
    (for the 11.3 BandScore).  Returns the H1 step counts (worst of the channels th["band_step_channels"], default R, G, B, Y,
    Cb, Cr: an 8-bit RGB capture is quantised per RGB channel, a decoded video per Y'CbCr channel, so a 1-code step shows in
    one family or the other; the decode gate counts Y, Cb, Cr only, D-041 (2): an R / G / B-only step of a decode is the
    conversion's rounding, not a visible band) and the BandScore; "chan" reports all six channels."""
    L = p.shape[0]
    ycc = ycbcr(p)
    s9 = _box(np.concatenate([p, ycc], axis=1), th["band_box"])
    off9 = (L - s9.shape[0]) // 2
    steps, step_max = 0, 0.0
    pos = []
    chan = {}
    gated = th.get("band_step_channels") or CHANNELS
    for c in range(6):
        where = []
        n, mx = visible_steps(s9[:, c], th, where)
        chan[CHANNELS[c]] = (int(n), float(mx))
        if CHANNELS[c] not in gated:
            continue
        steps = max(steps, n)
        step_max = max(step_max, mx)
        pos += [(float(w + off9), sg, c) for (w, sg) in where]
    per1000 = steps * 1000.0 / max(1, L)
    x = np.arange(L, dtype=np.float64)
    slope = np.atleast_1d(np.polyfit(x, ycc, 1)[0]) if L >= 2 else np.zeros(3)
    # 11.3 BandScore: longest run of identical 8-bit luma values x the local slope (= run / expected run 1 / slope),
    # only where the local slope (64-px central difference of the smoothed luma) is at least 0.02 level per pixel
    bs, run_px, applicable = 0.0, 0, False
    if luma8 is not None and L >= 3:
        ly = _box(ycc[:, :1], th["band_box"])[:, 0]
        n9 = ly.size
        off = (L - n9) // 2
        i = np.arange(L)
        lo, hi = np.clip(i - 32 - off, 0, n9 - 1), np.clip(i + 32 - off, 0, n9 - 1)
        local = np.abs(ly[hi] - ly[lo]) / np.maximum(hi - lo, 1)
        steep = local >= th["bandscore_min_slope"]
        if steep.any():
            applicable = True
            same = np.diff(luma8, axis=0) == 0            # (L-1) x w
            for col in range(luma8.shape[1]):
                m = np.concatenate(([0], same[:, col].astype(np.int8), [0]))
                dd = np.diff(m)
                st, en = np.flatnonzero(dd == 1), np.flatnonzero(dd == -1)
                for a, b in zip(st, en):
                    mid = (a + b + 1) // 2
                    if not steep[mid]:
                        continue
                    run = b - a + 1
                    sc = run * local[mid]
                    if sc > bs:
                        bs, run_px = float(sc), int(run)
    return {"len": int(L), "steps": int(steps), "steps_per_1000": float(per1000), "step_max_code": float(step_max),
            "step_pos": pos, "chan": {k: (v[0] * 1000.0 / max(1, L), v[1]) for k, v in chan.items()},
            "bandscore": float(bs), "longest_run_px": int(run_px), "run_applicable": bool(applicable),
            "resid_rms": None, "resid_pp": None,
            "slope_code_per_px": [float(v) for v in slope]}


def _segments(ok, min_len):
    m = np.concatenate(([0], ok.astype(np.int8), [0]))
    d = np.diff(m)
    s, e = np.flatnonzero(d == 1), np.flatnonzero(d == -1)
    return [(int(a), int(b)) for a, b in zip(s, e) if b - a >= min_len]


def polar_unwrap(img, mask, center, r0, r1, a0=0.0, a1=360.0):
    """Nearest-neighbour polar resampling: rows = radius (1 px steps), columns = arc samples (1 px at mid radius)."""
    H, W = img.shape[:2]
    rm = 0.5 * (r0 + r1)
    n_a = max(8, int(round(math.radians(abs(a1 - a0)) * rm)))
    radii = np.arange(r0, r1, 1.0)
    ang = np.radians(np.linspace(a0, a1, n_a, endpoint=False))
    rr, aa = np.meshgrid(radii, ang, indexing="ij")
    xs = np.floor(center[0] + rr * np.cos(aa)).astype(np.int64)
    ys = np.floor(center[1] - rr * np.sin(aa)).astype(np.int64)
    inside = (xs >= 0) & (xs < W) & (ys >= 0) & (ys < H)
    xs_c, ys_c = np.clip(xs, 0, W - 1), np.clip(ys, 0, H - 1)
    out = img[ys_c, xs_c].astype(np.float64)
    m = ~inside
    if mask is not None:
        m |= mask[ys_c, xs_c]
    return out, m


def band_score(img, rect=None, direction="v", mask=None, th=TH, band=None):
    """Banding over a region (14.7 H1 visible steps in Y, Cb, Cr and the 11.3 BandScore).  img: H x W x 3 (codes).
    mask: H x W bool, True = excluded (neon, objects).  Strips of 64 px perpendicular to the gradient; strips split around
    excluded rows.  Returns the worst values, the per-strip list and pass/score (score <= 1 passes)."""
    H, W = img.shape[:2]
    band = band or {}
    if direction == "radial":
        c = band.get("_center_px")
        reg, rmask = polar_unwrap(img, mask, c, float(band["_r0"]), float(band["_r1"]),
                                  float(band.get("a0", 0.0)), float(band.get("a1", 360.0)))
        direction = "v"
    else:
        x0, y0, x1, y1 = rect if rect else (0, 0, W, H)
        reg = img[y0:y1, x0:x1].astype(np.float64)
        rmask = mask[y0:y1, x0:x1] if mask is not None else np.zeros(reg.shape[:2], dtype=bool)
        if direction == "auto":
            g = reg.mean(axis=2)
            gy = np.abs(np.diff(g.mean(axis=1))).mean() if g.shape[0] > 1 else 0.0
            gx = np.abs(np.diff(g.mean(axis=0))).mean() if g.shape[1] > 1 else 0.0
            direction = "v" if gy >= gx else "h"
    if direction == "h":
        reg = np.transpose(reg, (1, 0, 2))
        rmask = rmask.T
    sw = th["band_strip_px"]
    n_strips = reg.shape[1] // sw
    widths = [(k * sw, (k + 1) * sw) for k in range(n_strips)] if n_strips else [(0, reg.shape[1])]
    min_len = 2 * 12 + 18 + th["band_box"]
    strips = []
    for (a, b) in widths:
        ok = ~rmask[:, a:b].any(axis=1)
        for (s, e) in _segments(ok, min_len):
            prof = reg[s:e, a:b].mean(axis=1)
            sub = reg[s:e, a:b]
            l8 = np.round(0.2126 * sub[..., 0] + 0.7152 * sub[..., 1] + 0.0722 * sub[..., 2]).astype(np.int32)
            m = strip_metrics(prof, th, l8)
            m["strip"] = [int(a), int(b)]
            m["rows"] = [int(s), int(e)]
            strips.append(m)
    steps_only = band.get("gate") == "steps"
    if steps_only:
        _coherent_steps(strips, th)
    res = {"direction": direction, "n_strips": len(strips), "strips": strips}
    if not strips:
        res.update({"pass": None, "score": None, "note": "no strip long enough (region too small or masked)"})
        return res
    worst = {
        "steps_per_1000": max(m["steps_per_1000"] for m in strips),
        "steps": max(m["steps"] for m in strips),
        "step_max_code": max(m["step_max_code"] for m in strips),
        "bandscore": max(m["bandscore"] for m in strips),
        "longest_run_px": max(m["longest_run_px"] for m in strips),
        "resid_rms": None, "resid_pp": None,
        "slope_abs_max": max(max(abs(v) for v in m["slope_code_per_px"]) for m in strips),
        # QA fix wave C19: the worst visible steps / 1000 and max step code per channel (Y, Cb, Cr of the decode; R, G, B)
        "chan": {k: [max(m["chan"][k][0] for m in strips), max(m["chan"][k][1] for m in strips)]
                 for k in ("Y", "Cb", "Cr", "R", "G", "B")} if all("chan" in m for m in strips) else None,
    }
    lim = float(th["band_steps_per_1000_max"])
    if lim > 0:
        part_steps = worst["steps_per_1000"] / lim
    else:
        part_steps = 2.0 if worst["steps"] > 0 else 0.0
    parts = [part_steps] if steps_only else [part_steps, worst["bandscore"] / th["bandscore_max"]]
    res["gate"] = "steps" if steps_only else "steps+bandscore"
    if worst["steps"] > 0:
        parts.append(worst["step_max_code"] / th["band_step_max_code"])
    score = max(parts)
    res.update(worst)
    res["score"] = float(score)
    res["pass"] = bool(score <= 1.0)
    return res


def _coherent_steps(strips, th=TH):
    """Textured surfaces (BAND gate "steps", QA-6): a quantisation band is a contour that runs across the region, so its step
    shows in the neighbouring strip too (same channel, same sign, within band_step_gap_px rows); a texture or reflection edge
    inside one strip is not banding.  Rewrites each strip's steps / steps_per_1000 / step_max_code to its coherent steps and
    keeps the raw counts as steps_raw / steps_per_1000_raw."""
    gap = th["band_step_gap_px"]
    by_col = {}
    for m in strips:
        by_col.setdefault(m["strip"][0], []).append(m)
    cols = sorted(by_col)
    for m in strips:
        m["steps_raw"], m["steps_per_1000_raw"] = m["steps"], m["steps_per_1000"]
        k = cols.index(m["strip"][0])
        nb = []
        for j in (k - 1, k + 1):
            if 0 <= j < len(cols):
                nb += by_col[cols[j]]
        kept = []
        for (r, sg, c) in m.get("step_pos", []):
            ra = m["rows"][0] + r
            for o in nb:
                if any(sg2 == sg and c2 == c and abs(o["rows"][0] + r2 - ra) <= gap for (r2, sg2, c2) in o.get("step_pos", [])):
                    kept.append((r, sg, c))
                    break
        per_ch = {}
        for (_, _, c) in kept:
            per_ch[c] = per_ch.get(c, 0) + 1
        n = max(per_ch.values()) if per_ch else 0
        m["steps"] = int(n)
        m["steps_per_1000"] = float(n * 1000.0 / max(1, m["len"]))
        if n == 0:
            m["step_max_code"] = 0.0


def band_regions_resolved(bands, W, H, cam=None):
    """Resolve banding items to (name, rect, direction, item) with pixel coordinates."""
    out = []
    for i, b in enumerate(bands):
        name = b.get("name", "band%d" % i)
        d = b.get("dir", "v")
        if d == "radial":
            cx, cy = scale_xy(float(b["center"][0]), float(b["center"][1]), W, H, b.get("_ref"), b.get("_units"))
            units, ref = b.get("_units"), b.get("_ref")
            # radii scale with the image height (frac: fraction of H; px: as given; ref: H / ref height)
            sr = float(H) if units in ("frac", "frac169") else (1.0 if (units == "px" or not ref) else H / float(ref[1]))
            it = dict(b)
            it["_center_px"] = (cx, cy)
            it["_r0"], it["_r1"] = float(b["r0"]) * sr, float(b["r1"]) * sr
            out.append((name, None, "radial", it))
        else:
            out.append((name, item_rect(b, "rect", W, H), d, b))
    return out


# --------------------------------------------------------------------------------------------- thin-line probe
def line_probe(lum, cx, y0, w=64, h=256, search_tol=None):
    """Thin vertical line probe on a luminance image (linear recommended).
    Crop w x h with the left edge at round(cx - w/2), rows y0..y0+h.  Per row: the background is a linear fit
    through the 8-px flanks; d = |L - background| with the line's polarity; FWHM by linear interpolation of the
    half-maximum crossings; centroid and energy over the contiguous support above 10 percent of the peak;
    monotone 3-level edge check (each edge rises monotonically with at most one intermediate pixel between 15
    and 85 percent of the row peak).  Returns per-row arrays and summary values (pixel coordinates absolute)."""
    H, W = lum.shape
    w = int(min(w, W))
    h = int(min(h, H))
    x0 = int(math.floor(cx - w * 0.5 + 0.5))
    x0 = max(0, min(W - w, x0))
    y0 = int(max(0, min(H - h, int(math.floor(y0 + 0.5)))))
    crop = lum[y0:y0 + h, x0:x0 + w].astype(np.float64)
    nf = max(4, w // 8)
    xs = np.arange(w, dtype=np.float64) + 0.5
    fl = np.r_[0:nf, w - nf:w]
    X = xs[fl]
    Y = crop[:, fl]
    xm = X.mean()
    sxx = ((X - xm) ** 2).sum()
    ym = Y.mean(axis=1)
    sl = ((Y - ym[:, None]) * (X - xm)[None, :]).sum(axis=1) / sxx
    bg = ym[:, None] + sl[:, None] * (xs[None, :] - xm)
    d = crop - bg
    rf = d[:, fl]
    noise = float(1.4826 * np.median(np.abs(rf - np.median(rf)))) + 1e-9
    inner = d[:, nf:w - nf]
    pol = 1.0 if np.median(inner.sum(axis=1)) >= 0 else -1.0
    d *= pol
    bgm = np.maximum(np.abs(bg), 1e-6)
    rows = []
    for r in range(h):
        row = d[r]
        k = nf + int(np.argmax(row[nf:w - nf]))
        peak = row[k]
        rec = {"valid": False}
        if peak > max(6.0 * noise, 1e-5):
            half = 0.5 * peak
            i = k
            while i > 0 and row[i - 1] > half:
                i -= 1
            j = k
            while j < w - 1 and row[j + 1] > half:
                j += 1
            if i > 0 and j < w - 1:
                xl = (i - 1) + (half - row[i - 1]) / max(row[i] - row[i - 1], 1e-12)
                xr = j + (row[j] - half) / max(row[j] - row[j + 1], 1e-12)
                ts = max(0.1 * peak, 3.0 * noise)
                a = k
                while a > 0 and row[a - 1] > ts:
                    a -= 1
                b = k
                while b < w - 1 and row[b + 1] > ts:
                    b += 1
                seg = np.maximum(row[a:b + 1], 0.0)
                en = float(seg.sum())
                cen = float(((np.arange(a, b + 1) + 0.5) * seg).sum() / max(en, 1e-12))
                tol = max(3.0 * noise, 0.05 * peak)
                left = row[max(a - 1, 0):k + 1]
                right = row[k:min(b + 2, w)]
                mono = bool(np.all(np.diff(left) >= -tol) and np.all(np.diff(right) <= tol))
                lo, hi = 0.15 * peak, 0.85 * peak
                nl = int(((row[a:k] > lo) & (row[a:k] < hi)).sum())
                nr = int(((row[k + 1:b + 1] > lo) & (row[k + 1:b + 1] < hi)).sum())
                c3 = int(round(cen - 0.5))
                xa, xb = c3 - 10, c3 + 10
                if 1 <= c3 < w - 1 and xa >= 0 and xb < w:
                    lc = crop[r, c3 - 1:c3 + 2].mean()
                    lr = 0.5 * (crop[r, xa] + crop[r, xb])
                    crel = float(abs(lc - lr) / max(lr, 1e-6))
                else:
                    crel = float(peak / bgm[r, k])
                rec = {"valid": True, "fwhm": float(xr - xl), "centroid": cen + x0, "energy": en,
                       "peak": float(peak), "eqw": en / peak, "mono": mono, "three_level": (nl <= 1 and nr <= 1),
                       "contrast_rel": crel}
        rows.append(rec)
    val = [rr for rr in rows if rr["valid"]]
    res = {"crop": [x0, y0, x0 + w, y0 + h], "polarity": "bright" if pol > 0 else "dark", "noise": noise,
           "rows": h, "valid_rows": len(val)}
    if len(val) < 8:
        res["found"] = False
        return res
    idx = np.array([i for i, rr in enumerate(rows) if rr["valid"]], dtype=np.float64)
    fw = np.array([rr["fwhm"] for rr in val])
    cen = np.array([rr["centroid"] for rr in val])
    en = np.array([rr["energy"] for rr in val])
    deg = 2 if len(val) >= 16 else 1
    cfit = np.polyval(np.polyfit(idx, cen, deg), idx)
    efit = np.polyval(np.polyfit(idx, en, 1), idx)
    resid = cen - cfit
    # stair-step = systematic centroid steps along the line: the residual smoothed over 5 rows (a staircase of a
    # near-vertical aliased line has steps every 1/slope >= 10 rows and survives; per-row dither noise drops by
    # sqrt(5)).  The raw residual RMS is reported too.
    r5 = np.convolve(resid, np.ones(5) / 5.0, mode="valid") if resid.size >= 5 else resid
    res.update({
        "found": True,
        "fwhm_px": float(np.median(fw)), "fwhm_p10": float(np.percentile(fw, 10)), "fwhm_p90": float(np.percentile(fw, 90)),
        "eq_width_px": float(np.median([rr["eqw"] for rr in val])),
        "centroid_mean": float(cen.mean()),
        "stairstep_px": float(np.abs(r5).max()),
        "centroid_resid_max_px": float(np.abs(resid).max()),
        "centroid_resid_rms_px": float(np.sqrt(np.mean(resid ** 2))),
        "energy_mean": float(en.mean()),
        "energy_var": float(np.std(en - efit) / max(abs(en.mean()), 1e-12)),
        "monotone_rows": float(np.mean([rr["mono"] and rr["three_level"] for rr in val])),
        "contrast_rel": float(np.median([rr["contrast_rel"] for rr in val])),
        "valid_frac": len(val) / float(h),
    })
    return res


def line_spec_resolved(ln, W, H):
    """LINE item -> (x centre, y0, crop w, crop h, search tolerance) in the image's own pixels.
    The crop stays 64 x 256 (14.10.3) at every resolution; the expected x and the search tolerance scale.
    "orientation": "h" (a near-horizontal line, item keys "y" and "x0") returns the same tuple in the TRANSPOSED
    frame: call the probe on lum.T (see line_lum)."""
    units, ref = ln.get("_units"), ln.get("_ref")
    if ln.get("orientation", "v") == "h":
        rref = (ref[1], ref[0]) if ref else None
        t = {"x": ln.get("y"), "y0": ln.get("x0"), "tol": ln.get("tol", 32.0), "w": ln.get("w", 64), "h": ln.get("h", 256),
             "_units": units, "_ref": rref}
        t = {k: v for k, v in t.items() if v is not None or k in ("_units", "_ref")}
        return line_spec_resolved(t, H, W)
    yv = ln.get("y0")
    xs, ys = scale_xy(float(ln.get("x", 0.5 if units == "frac" else (ref[0] * 0.5 if ref else W * 0.5))),
                      float(yv) if yv is not None else 0.0, W, H, ref, units)
    if yv is None:
        ys = 0.15 * H
    sx = 1.0 if (units in ("px", "frac") or not ref) else W / float(ref[0])
    tol = float(ln.get("tol", 32.0)) * sx
    w = int(round(float(ln.get("w", 64))))
    h = int(round(float(ln.get("h", 256))))
    return xs, ys, w, h, tol


def line_lum(lum, ln):
    """The luminance plane a LINE item is probed on (transposed for "orientation": "h")."""
    return lum.T if ln.get("orientation", "v") == "h" else lum


def locate_line(lum, x, y0, h, tol):
    """Column of the strongest thin feature within x +- tol over rows y0..y0+h (column-mean deviation from the
    window median).  Returns the refined centre x (pixel centre coordinates)."""
    H, W = lum.shape
    a, b = int(max(0, math.floor(x - tol))), int(min(W, math.ceil(x + tol) + 1))
    r0, r1 = int(max(0, y0)), int(min(H, y0 + h))
    if b - a < 3 or r1 - r0 < 1:
        return x
    prof = lum[r0:r1, a:b].astype(np.float64).mean(axis=0)
    dev = np.abs(prof - np.median(prof))
    return a + int(np.argmax(dev)) + 0.5


def evaluate_line(res, ln, H, th=TH):
    """PASS/FAIL checks for a line probe result against a LINE item's expectations."""
    name = ln.get("name", "line")
    out = []
    if not res.get("found"):
        out.append(check("line[%s] found" % name, FAIL, res.get("valid_rows"), ">= 8 valid rows",
                         "no line detected in crop %s" % res.get("crop")))
        return out
    s1080 = 1080.0 / H
    w1080 = res["fwhm_px"] * s1080
    if "widthMin1080" in ln or "widthMax1080" in ln:
        lo, hi = float(ln.get("widthMin1080", 0.0)), float(ln.get("widthMax1080", 1e9))
        out.append(check("line[%s] width@1080p" % name, PASS if lo <= w1080 <= hi else FAIL, round(w1080, 3),
                         "%g..%g" % (lo, hi), "native %.2f px, eq %.2f px" % (res["fwhm_px"], res["eq_width_px"])))
    else:
        out.append(check("line[%s] width@1080p" % name, INFO, round(w1080, 3), None, "native %.2f px" % res["fwhm_px"]))
    if "contrastMin" in ln:
        cm = float(ln["contrastMin"])
        out.append(check("line[%s] contrast" % name, PASS if res["contrast_rel"] >= cm else FAIL,
                         round(res["contrast_rel"], 4), ">= %g" % cm, "3-px column vs columns +-10 px, linear"))
    vf = res["valid_frac"]
    out.append(check("line[%s] coverage" % name, PASS if vf >= th["line_valid_rows_min"] else FAIL, round(vf, 3),
                     ">= %g" % th["line_valid_rows_min"]))
    mr = res["monotone_rows"]
    out.append(check("line[%s] monotone 3-level edge" % name, PASS if mr >= th["line_monotone_rows_min"] else FAIL,
                     round(mr, 3), ">= %g of rows" % th["line_monotone_rows_min"]))
    st = res["stairstep_px"]
    out.append(check("line[%s] stair-step" % name, PASS if st <= th["line_stairstep_max_px"] else FAIL, round(st, 3),
                     "<= %g px" % th["line_stairstep_max_px"],
                     "5-row smoothed centroid residual vs a quadratic fit (raw rms %.3f px)" % res["centroid_resid_rms_px"]))
    ev = res["energy_var"]
    out.append(check("line[%s] energy variation" % name, PASS if ev <= th["line_energy_var_max"] else FAIL,
                     round(ev, 4), "<= %g" % th["line_energy_var_max"]))
    return out


# --------------------------------------------------------------------------------------------- capture folder
def _num(v):
    try:
        f = float(v)
        return f if math.isfinite(f) else None
    except (TypeError, ValueError):
        return None


def _size_of(d):
    w, h = d.get("width", d.get("w")), d.get("height", d.get("h"))
    if (w is None or h is None) and isinstance(d.get("size"), (list, tuple)) and len(d["size"]) == 2:
        w, h = d["size"]
    if (w is None or h is None) and isinstance(d.get("res"), str) and "x" in d["res"].lower():
        p = d["res"].lower().split("x")
        w, h = p[0], p[1]
    try:
        return (int(w), int(h)) if w is not None and h is not None else (None, None)
    except (TypeError, ValueError):
        return (None, None)


def _pose_of(d):
    pose = d.get("pose")
    out = {}
    if isinstance(pose, dict):
        out = {k: _num(pose.get(k)) for k in ("x", "y", "z", "yaw", "pitch", "fov")}
        if out.get("fov") is None:
            out["fov"] = _num(pose.get("vfov"))
    elif isinstance(pose, (list, tuple)):
        keys = ("x", "y", "z", "yaw", "pitch", "fov")
        out = {k: _num(pose[i]) if i < len(pose) else None for i, k in enumerate(keys)}
    for k in ("yaw", "pitch"):
        if out.get(k) is None and d.get(k) is not None:
            out[k] = _num(d.get(k))
    for k in ("fov", "vfov", "fovV", "verticalFov"):
        if out.get("fov") is None and d.get(k) is not None:
            out["fov"] = _num(d.get(k))
    if isinstance(d.get("position"), (list, tuple)) and len(d["position"]) == 3:
        out["x"], out["y"], out["z"] = [_num(v) for v in d["position"]]
    return out


def _subjects_of(d):
    sub = d.get("subjects", d.get("boxes"))
    out = []
    if isinstance(sub, dict):
        for k, v in sub.items():
            if isinstance(v, (list, tuple)) and len(v) == 4:
                out.append({"name": k, "box": [float(x) for x in v]})
            elif isinstance(v, dict):
                it = dict(v)
                it.setdefault("name", k)
                out.append(it)
    elif isinstance(sub, list):
        out = [dict(s) for s in sub if isinstance(s, dict)]
    return out


def drop_parity_companions(shots):
    """Remove the window half of a window / RenderTexture parity pair (X_window.png beside X.png, 11.2).  A key shot whose own
    tag ends in "_window" (KS07_edge_window, KS19_goal_window) has no X.png partner and stays (Q1: both were dropped before)."""
    names = set(os.path.basename(s["file"]).lower() for s in shots)
    out = []
    for s in shots:
        b = os.path.basename(s["file"]).lower()
        if b.endswith("_window.png") and (b[:-len("_window.png")] + ".png") in names:
            continue
        out.append(s)
    return out


def load_capture(cap_dir):
    """Read <dir>/report.json and the PNGs.  Returns (shots, meta, warnings); every shot is a normalised dict:
    tag, file, exp_w, exp_h, s, beat, pose, subjects, regions, luma_range, horizon_row, darkness, keyshot, raw."""
    warnings = []
    rep = read_json(os.path.join(cap_dir, "report.json"), None)
    meta = {}
    items = []
    if rep is None:
        warnings.append("report.json missing or unreadable: sizes, poses and subjects unknown")
    elif isinstance(rep, list):
        items = rep
    elif isinstance(rep, dict):
        meta = {k: v for k, v in rep.items() if not isinstance(v, list)}
        for key in ("shots", "images", "captures", "stills"):
            if isinstance(rep.get(key), list):
                items = rep[key]
                break
    sp = spec()
    shots, seen = [], set()
    for it in items:
        if not isinstance(it, dict):
            continue
        f = it.get("file", it.get("path", it.get("png", it.get("image"))))
        if not f:
            warnings.append("report entry without a file: %s" % json.dumps(it)[:120])
            continue
        fp = f if os.path.isabs(f) else os.path.join(cap_dir, f)
        fp = os.path.normpath(fp)
        seen.add(os.path.normcase(fp))
        shots.append(_norm_shot(it, fp, sp))
    for name in sorted(os.listdir(cap_dir)) if os.path.isdir(cap_dir) else []:
        if name.lower().endswith(".png"):
            fp = os.path.normpath(os.path.join(cap_dir, name))
            if os.path.normcase(fp) not in seen:
                if rep is not None:
                    warnings.append("%s is not listed in report.json" % name)
                shots.append(_norm_shot({}, fp, sp))
    return shots, meta, warnings


def _norm_shot(it, fp, sp):
    tag = it.get("tag", it.get("name")) or os.path.splitext(os.path.basename(fp))[0]
    w, h = _size_of(it)
    beat = it.get("beat")
    try:
        beat = int(beat) if beat is not None else None
    except (TypeError, ValueError):
        beat = None
    s = _num(it.get("s", it.get("progress")))
    if beat is None:
        beat = beat_from_tag(tag)
    if beat is None and s is not None:
        beat = sp.beat_of_s(s)
    ks = sp.keyshot_for_tag(tag)
    if ks is not None:
        # a related capture (e.g. "beat03_stairs_KF3_s305", or a different s) is not the key shot itself
        rest = tag[len(ks["tag"]):]
        if (s is not None and abs(s - ks["s"]) > 1.0) or re.search(r"(^|_)s\d", rest):
            ks = None
    pose = _pose_of(it)
    if ks:
        for k, v in (("yaw", ks["yaw"]), ("pitch", ks["pitch"]), ("x", ks["pos"][0]), ("y", ks["pos"][1]),
                     ("z", ks["pos"][2])):
            if pose.get(k) is None:
                pose[k] = v
        # 14.10.2 FOV rule: the table FOV is the in-game FOV unless the shot is a "_wide" one; the in-game
        # version of a wide key shot needs its FOV from report.json.
        if pose.get("fov") is None and (not ks["wide"] or "_wide" in tag):
            pose["fov"] = ks["fov"]
    lr = it.get("lumaRange", it.get("expectedLuma"))
    if isinstance(lr, dict):
        lr = (lr.get("min"), lr.get("max"))
    if lr is None and ks:
        lr = ks["luma"]
    dark = _num(it.get("darkness", it.get("D")))
    return {"tag": tag, "file": fp, "exp_w": w, "exp_h": h, "s": s, "beat": beat, "pose": pose,
            "subjects": _subjects_of(it), "regions": it.get("regions") if isinstance(it.get("regions"), dict) else None,
            "luma_range": tuple(lr) if lr else None, "horizon_row": _num(it.get("horizonRow", it.get("horizonY"))),
            "darkness": dark, "keyshot": ks["tag"] if ks else None, "raw": it}


def cam_for(shot, W, H):
    pose = shot.get("pose") or {}
    fov = pose.get("fov")
    if fov is None:
        return None
    return Cam(W, H, fov, pose.get("yaw"), pose.get("pitch"), (pose.get("x"), pose.get("y"), pose.get("z")))


# --------------------------------------------------------------------------------------------- sequences
_FRAME_RE = re.compile(r"^(.*?)(\d+)\.png$", re.IGNORECASE)


def list_frames(seq_dir):
    """Sorted [(index, path)] of the numbered PNGs in a folder (the most common prefix / digit count only), plus
    the printf pattern and the start number."""
    groups = {}
    for name in os.listdir(seq_dir):
        m = _FRAME_RE.match(name)
        if m:
            groups.setdefault((m.group(1), len(m.group(2))), []).append((int(m.group(2)), os.path.join(seq_dir, name)))
    if not groups:
        return [], None, None
    (prefix, nd), fr = max(groups.items(), key=lambda kv: len(kv[1]))
    fr.sort()
    pattern = os.path.join(seq_dir, "%s%%0%dd.png" % (prefix, nd))
    return fr, pattern, fr[0][0]


def is_sequence_dir(p):
    """A folder is a frame sequence if it has sequence.json, is called seq_*, or holds >= 2 frame_NNNNN.png."""
    if not os.path.isdir(p):
        return False
    if os.path.isfile(os.path.join(p, "sequence.json")) or os.path.basename(p.rstrip("\\/")).lower().startswith("seq_"):
        return True
    n = sum(1 for f in os.listdir(p) if re.match(r"^frame_\d+\.png$", f, re.IGNORECASE))
    return n >= 2


def find_sequences(cap_dir):
    out = []
    if not os.path.isdir(cap_dir):
        return out
    for name in sorted(os.listdir(cap_dir)):
        p = os.path.join(cap_dir, name)
        if os.path.isdir(p) and name.lower().startswith("seq_"):
            out.append(p)
    return out


def resolve_sequence_dirs(paths):
    """Sequence folders from a mix of sequence folders and capture folders (their seq_* subfolders)."""
    out = []
    for p in paths:
        p = os.path.abspath(p)
        if is_sequence_dir(p):
            out.append(p)
        elif os.path.isdir(p):
            out += find_sequences(p)
    return out


def load_sequence(seq_dir):
    frames, pattern, start = list_frames(seq_dir)
    meta = read_json(os.path.join(seq_dir, "sequence.json"), {}) or {}
    name = meta.get("name") or os.path.basename(seq_dir.rstrip("\\/"))
    if name.lower().startswith("seq_"):
        name = name[4:]
    return {"name": name, "dir": seq_dir, "frames": frames, "pattern": pattern, "start": start, "meta": meta,
            "fps": float(meta.get("fps", 30.0) or 30.0)}
