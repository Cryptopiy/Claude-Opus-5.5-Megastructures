#!/usr/bin/env python3
"""qa_common: shared library of the Bulwark offline QA tools (MASTER_PROMPT Part 11.3, 11.6, 11.9, 11.12, 14.10).
Imported from Thread; scene numbers from BulwarkConst.cs, thresholds from 14.10.3.

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
    "tag"                e.g. "KS04_crest_sea" or "KS04_crest_sea_3840x2160", "b02_Crest_mid_y090_p00" (default: file stem)
    "width", "height"    the size the harness REQUESTED (aliases "w"/"h", "size": [w, h], "res": "WxH")
    "s"                  plan distance (m);  "beat": 1..7 (default: parsed from "beatNN" in the tag)
    "pose"               {"x","y","z","yaw","pitch","fov"} or [x, y, z, yaw, pitch]; yaw = world azimuth from +Z
                         toward +X, pitch up positive, degrees.  "fov" = VERTICAL fov in degrees (alias "vfov").
    "lumaRange"          [min, max] expected mean display luma 0..1 (default: the key-shot row of BulwarkConst.KeyShots,
                         else the 14.10.3 per-beat mean-luma row)
    "horizonRow"         pixel row (float, top = 0) of the horizon line, if the harness knows it better than the
                         flat-world formula  y = H/2 + f tan(pitch)
    "sun"                [az, el] of the sun at this shot (degrees), for "azEl": "sun" exclusions
    "subjects"           [ {"name": "lantern", "box": [x0, y0, x1, y1] (pixels, top-left origin),
                            "margin": 0.03, "maxCropFrac": 0.0, "minWidthFrac": 0.25} , ... ]
                         or {"planet": [x0, y0, x1, y1], ...}.  Framing check: box inside the frame with the margin
                         (fraction of the frame size), or at most maxCropFrac of the box width/height cropped.
    "regions"            same structure as one entry of a regions file (below); coordinates default to the
                         image's own pixels (no scaling) unless "ref" or "units" is given.

<dir>/seq_<name>/frame_NNNNN.png + <dir>/seq_<name>/sequence.json
    { "name", "kind": "static|transition|approach|swap|pan|ride|encode", "fps": 30,
      "s": [per-frame s] or "s0"/"s1", "freezeDither": bool, "freezeAmbient": bool, "sunBreak": bool,
      "encode": bool (encode_test picks it up), "dither": "static|off" (encode pairs), "lumaStepMax", "panStdMax",
      "flickerStdMax", "spikeRatio",
      "lines": [LINE...], "stability": [ {"rect": [...], "tol": 0.12, "window": 5} ], "banding": [BAND...] }

Audio: <file>.wav (16/24/32-bit PCM or 32-bit float, any rate) + sidecar <file>.json (or <file>.wav.json,
<file>_s.json):  {"s": [s at t = t0 + i*dt], "dt": 1.0, "t0": 0.0, "beat": N (whole file is beat N),
                  "stem": "mix|...", "footsteps": [onset seconds]}   (OfflineAudioRenderer / AudioDirector sidecars:
                  "t" and "s" arrays are accepted too)

Regions file (default Tools/qa/regions_default.json):
    { "regions": [ { "match": "<fnmatch pattern on the tag>", "aspect": "16:9|16:10|21:9|any",
                     "ref": [1920, 1080], "banding": [BAND...], "lines": [LINE...], "horizon": [HZ...],
                     "stats": [STAT...], "exclude": [EXCL...] }, ... ] }
  Coordinates are pixels at "ref" (default 1920 x 1080) and are scaled per axis to the image size; a region with
  "units": "frac" uses 0..1 fractions, "units": "px" uses the image's own pixels.
  BAND  {"name", "rect": [x0, y0, x1, y1], "dir": "v|h|auto|radial", "center": [x, y], "r0", "r1", "a0", "a1"}
        dir "v": the gradient runs along y (strips are 64 px wide columns); "radial": polar resampling (nearest)
  LINE  {"name", "x", "tol": 32, "y0", "h": 256, "w": 64, "widthMin1080", "widthMax1080", "contrastMin"}
  HZ    {"name", "a": rect, "b": rect, "tol": 0.01}  (median colour of a vs b, per channel, 8-bit sRGB)
  STAT  {"name", "rect", "units", "stdMax", "meanMin", "meanMax", "scale": "display|linear"}
  EXCL  {"name", "ndc": [x, y] | "px": [x, y] | "azEl": [az, el] | "azEl": "sun" | "world": [x,y,z] | "world": "lantern",
         "clipDeg": 1.2, "bandDeg": 4.0,
         "coreDeg": 0.9, "coreMax": 0.98}
----------------------------------------------------------------------------------------------------------------
Thresholds come from 14.10.3 (TH below).  Scene numbers (beat boundaries, key shots, audio targets, dither peaks,
the lantern) are parsed from the single source of truth  Assets/_Game/Scripts/Runtime/Core/BulwarkConst.cs  (beat
names from FrameworkConst.cs).  There are no fallback literals: a missing value is an error (Spec.errors).
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
CONSTANTS_CS = os.path.join(ROOT, "Assets", "_Game", "Scripts", "Runtime", "Core", "BulwarkConst.cs")
FRAMEWORK_CS = os.path.join(ROOT, "Assets", "_Game", "Scripts", "Runtime", "Core", "FrameworkConst.cs")

PASS, FAIL, SKIP, INFO, WARN = "PASS", "FAIL", "SKIP", "INFO", "WARN"

# --------------------------------------------------------------------------------------------- thresholds (14.10.3)
TH = {
    # mean display luma per beat, levels 0..255 (the gate uses fractions: level / 255)
    "mean_luma_levels": {1: (28, 70), 2: (85, 150), 3: (95, 150), 4: (100, 165), 5: (28, 80), 6: (14, 85), 7: (20, 90),
                         # A1 4.10: beat 8 = the ride (cabin, descent, transfer, shuttle), beat 9 = the lantern platform and
                         # the look back (b08_* stills split by s at EndingPath.SFoot0)
                         8: (20, 80), 9: (20, 70)},
    # clipped = any channel >= 254 (sun disc and lamp cores excluded through the regions file); percent of the frame
    "clip_pct_max": {1: 1.5, 2: 0.4, 3: 0.8, 4: 0.6, 5: 0.3, 6: 0.1, 7: 1.0, 8: 1.0, 9: 0.5},   # 8: outside a 2.5 deg sun disc; 9: lantern cores excluded
    "clip_code": 254,
    "p1_min": {},                          # no 1st-percentile gate in 14.10.3 (reported as INFO)
    # BandScore: 8-bit luma profile averaged over 128+ columns, every slope included; longest run of identical values
    # <= 6 px at 1080p (12 at 4K, scaled with the height); slopes below 0.1 level/px: mean run <= 0.5 / slope px
    "band_cols": 128, "band_run_max_1080": 6.0, "band_gentle_slope": 0.1, "band_mean_run_k": 0.5,
    "band_slope_half": 32, "band_slope_floor": 0.004, "band_min_len": 48,
    # re-encode (x264 CRF 23 and 30): dithered longest run <= 0.6 x the undithered one (4K CRF 23 decode gates)
    "encode_dither_ratio_max": 0.6,
    # isolated pixels: luma above 8 x the 5 x 5 median (and at least 16 levels above it): 0
    "firefly_ratio": 8.0, "firefly_min_delta": 16.0,
    # aliasing (thin line probe, kept from Thread)
    "line_jitter_max_px": 0.1, "line_energy_var_max": 0.10, "line_monotone_rows_min": 0.95,
    "line_stairstep_max_px": 0.25, "line_valid_rows_min": 0.5,
    # horizon agreement per channel (explicit regions only; 14.8 sea: horizon dE <= 2.0) and seams
    "horizon_tol": 0.01, "seam_tol": 0.02, "horizon_default_beats": (),
    # black / white / magenta / NaN-like
    "black_code": 3, "white_code": 252, "flat_frac": 0.98,
    "nan_dark_t": 0.30, "nan_bright_t": 0.30,
    # frame sequences (30 fps): mean luma step <= 1.2 levels per frame (2.0 inside the sun break), activation-frame
    # difference <= 0.5, EV slope <= 0.30 EV/s; pan (0.5 px/frame, frozen clock) temporal std <= 2.0 levels (1.5 for
    # beat 6 water and beat 7 lowland); flicker (static camera, clock running) 64 x 64 block-mean std over 120 frames
    # <= 3 levels (1.5 for the beat 7 lowland and sky); the firefly test on every frame
    "seq_luma_step_max_levels": 1.2, "seq_luma_step_sunbreak_levels": 2.0, "seq_luma_step_doorway_levels": 2.6, "activation_diff_max": 0.5,
    "ev_slope_max": 0.30, "pan_std_max": 2.0, "pan_std_strict": 1.5, "flicker_block": 64, "flicker_frames": 120,
    "flicker_std_max": 3.0, "flicker_std_strict": 1.5,
    "spike_ratio": 3.0, "spike_floor_grey": 0.25, "spike_window": 31,
    "stability_tol": 0.12, "stability_window": 5, "stability_floor": 0.05,
    # audio (14.10.3, 14.3): true peak <= -1 dBTP (4x oversampled), 3 s RMS within +-1.5 dB of BulwarkConst targets
    "audio_true_peak_max_dbtp": -1.0, "audio_rms_window_s": 3.0, "audio_rms_tol_db": 1.5, "audio_oversample": 4,
    "audio_dc_max": 0.001, "audio_step_max": 0.25,
}


def mean_luma_range(beat):
    """14.10.3 mean-luma row of a beat as display-luma fractions, or None."""
    r = TH["mean_luma_levels"].get(beat)
    return (r[0] / 255.0, r[1] / 255.0) if r else None


def band_run_max(H, th=None):
    """Longest identical run allowed at image height H (6 px at 1080p, 12 px at 4K)."""
    th = TH if th is None else th
    return th["band_run_max_1080"] * max(1.0, H / 1080.0)


_NUM = r"-?\d+(?:\.\d+)?(?:[eE][-+]?\d+)?"


_IDENT = r"[A-Za-z_]\w*"


def _cs_number(expr, resolve=None):
    """A C# literal of BulwarkConst: 1.5, -4.0, 62.58, L(28) (a level 0..255 -> fraction), or, with a resolver for
    BulwarkConst names, Name and L(Name) (Wave B: KS03's own band is L(KS03LumaMin), L(KS03LumaMax))."""
    expr = expr.strip()
    m = re.match(r"^L\(\s*(%s|%s)\s*\)$" % (_NUM, _IDENT), expr)
    if m:
        return _cs_number(m.group(1), resolve) / 255.0
    if re.match(r"^%s$" % _IDENT, expr):
        v = resolve(expr) if resolve is not None else None
        if v is None:
            raise ValueError("unknown constant '%s'" % expr)
        return float(v)
    return float(expr.rstrip("fFdD"))


class Spec(object):
    """Scene numbers parsed from BulwarkConst.cs (+ beat names from FrameworkConst.cs). No fallback literals:
    anything missing is listed in .errors and the tools report it."""

    def __init__(self, path=CONSTANTS_CS, framework=FRAMEWORK_CS):
        self.source = "BulwarkConst.cs"
        self.warnings = []
        self.errors = []
        text = ""
        try:
            with open(path, "r", encoding="utf-8-sig") as f:
                text = f.read()
        except OSError as e:
            self.errors.append("BulwarkConst.cs unreadable: %s" % e)
        ftext = ""
        try:
            with open(framework, "r", encoding="utf-8-sig") as f:
                ftext = f.read()
        except OSError as e:
            self.warnings.append("FrameworkConst.cs unreadable: %s" % e)

        def arr(name, src=None):
            m = re.search(r"\b%s\s*=\s*\{([^}]*)\}" % name, src if src is not None else text)
            if not m:
                self.errors.append("BulwarkConst.%s not found" % name)
                return []
            return [float(v.strip().rstrip("fFdD")) for v in m.group(1).split(",") if v.strip()]

        def num(name, default=None):
            m = re.search(r"\b%s\s*=\s*(%s)" % (name, _NUM), text)
            if not m:
                if default is None:
                    self.errors.append("BulwarkConst.%s not found" % name)
                return default
            return float(m.group(1))

        self.beat_boundaries = arr("BeatS")
        m = re.search(r"\bBeatNames\s*=\s*\{([^}]*)\}", ftext)
        self.beat_names = re.findall(r'"([^"]*)"', m.group(1)) if m else ["beat%d" % (k + 1) for k in range(7)]
        self.target_rms_db = arr("TargetRmsDb")
        self.dither_peak = arr("DitherPeak")
        self.rms_tol_db = num("TargetRmsTol", TH["audio_rms_tol_db"])
        self.true_peak_db = num("TruePeakDb", TH["audio_true_peak_max_dbtp"])
        self.dc_max = num("DcMax", TH["audio_dc_max"])
        self.step_max = num("SampleStepMax", TH["audio_step_max"])
        self.ledge_rms = (num("TargetRmsB7LedgeS", 1470.0), num("TargetRmsB7Ledge", -35.0))
        self.goal_trigger_s = num("GoalTriggerS")
        # the ending timeline (EndingPath, A1 00 LEAD DECISION 1): the ride from s 1500 at RideSRate per second, the platform
        # walk from SFoot0 = 1500 + RideSRate x (board + descent + transfer + shuttle + doors); b08 stills split there
        ride_t = sum(num(n, 0.0) for n in ("RideBoardTime", "RideDescentTime", "RideTransferTime", "RideShuttleTime", "RideDoorsTime"))
        self.ending_s0 = num("GoalRestS", 1500.0)
        self.foot0_s = self.ending_s0 + num("RideSRate", 2.5) * ride_t
        # sun break of beat 3 (u_sun ramps s 520-575 and 640-700): the 2.0-level luma-step allowance of 14.10.3
        sb0, sb1, sbw = num("SunBreakS0", 520.0), num("SunBreakS1", 640.0), num("SunBreakW1", 60.0)
        self.sun_break = (sb0, sb1 + sbw)
        # beat-7 ledge (after the tunnel mouth): the strict pan / flicker limits of the lowland and sky
        self.ledge_strict_s = num("MouthDuckS", 1423.0)
        self.sample_rate = num("AudioSampleRate", 48000.0)
        lx, ly, lz = num("LanternX"), num("LanternY"), num("LanternZ")
        post, glass = num("LanternPost", 1.4), num("LanternGlass", 0.3)
        self.lantern = (lx, ly + post + 0.5 * glass, lz) if None not in (lx, ly, lz) else None
        self.keyshots = {}
        for m in re.finditer(r'new\s+KeyShot\(\s*"([^"]+)"\s*,(.*?)\)\s*,?\s*$', text, re.M):
            parts = [p.strip() for p in re.split(r",(?![^(]*\))", m.group(2)) if p.strip()]
            try:
                v = [_cs_number(p, num) for p in parts[:9]]
            except ValueError as e:
                self.errors.append("KeyShot %s: %s" % (m.group(1), e))
                continue
            if len(v) < 9:
                self.errors.append("KeyShot %s: %d numbers" % (m.group(1), len(v)))
                continue
            self.keyshots[m.group(1)] = {"tag": m.group(1), "s": v[0], "pos": (v[1], v[2], v[3]), "yaw": v[4],
                                         "pitch": v[5], "fov": v[6], "luma": (v[7], v[8]), "wide": False}
        if not self.keyshots:
            self.errors.append("BulwarkConst.KeyShots not parsed")
        if self.errors:
            self.source = "BulwarkConst.cs (ERRORS: %s)" % "; ".join(self.errors)

    def ending_beat(self, s):
        """A1 4.10 rows of the ending: 8 in the cabin and the ride (s < SFoot0), 9 on the lantern platform."""
        return 8 if s is None or s < self.foot0_s - 1e-6 else 9

    def beat_of_s(self, s):
        b = self.beat_boundaries
        for k in range(len(b) - 1):
            if s < b[k + 1]:
                return k + 1
        return max(1, len(b) - 1)

    def keyshot_match(self, tag):
        """(row, end index in tag) of the key shot named in a capture tag: ShotPlan writes "b02_Crest_KS04_crest_sea"
        (+ "_3840x2160" etc.); the table tag may also start the capture tag or appear as its KSnn id alone."""
        tag = tag or ""
        best, best_end = None, -1
        for kt in self.keyshots:
            i = tag.find(kt)
            while i >= 0:
                end = i + len(kt)
                if (i == 0 or tag[i - 1] == "_") and (end == len(tag) or not tag[end].isalnum()):
                    if best is None or len(kt) > len(best):
                        best, best_end = kt, end
                    break
                i = tag.find(kt, i + 1)
        if best is None:
            m = re.search(r"(?:^|_)(KS\d\d)(?![0-9A-Za-z])", tag)
            if m:
                for kt in self.keyshots:
                    if kt.split("_")[0] == m.group(1):
                        best, best_end = kt, m.end(1)
                        break
        return (self.keyshots[best], best_end) if best else (None, -1)

    def keyshot_for_tag(self, tag):
        return self.keyshot_match(tag)[0]


_SPEC = None


def spec():
    global _SPEC
    if _SPEC is None:
        _SPEC = Spec()
    return _SPEC


# --------------------------------------------------------------------------------------------- small utilities
def beat_from_tag(tag):
    """Beat number from a ShotPlan tag: "b03_SpillwayBridge_..." or "beat03..." (key shots resolve through s)."""
    m = re.search(r"(?:^|_)(?:b|beat)0*([1-7])(?=_|$)", tag or "", re.IGNORECASE)
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


def fireflies(grey, ratio=None, min_delta=None, mask=None, rows=256):
    """14.10.3 isolated pixels: luma above ratio x the 5 x 5 median AND at least min_delta levels above it.
    grey: H x W display luma in levels (0..255).  mask: True = excluded.  Returns (count, first positions)."""
    ratio = TH["firefly_ratio"] if ratio is None else ratio
    min_delta = TH["firefly_min_delta"] if min_delta is None else min_delta
    g = np.asarray(grey, dtype=np.float32)
    H, W = g.shape
    P = np.pad(g, 2, mode="reflect" if (H > 2 and W > 2) else "edge")
    count, pos = 0, []
    for y0 in range(0, H, rows):
        y1 = min(H, y0 + rows)
        win = np.stack([P[y0 + dy:y1 + dy, dx:dx + W] for dy in range(5) for dx in range(5)])
        med = np.median(win, axis=0)
        c = g[y0:y1]
        hit = (c > ratio * np.maximum(med, 1.0)) & (c - med >= min_delta)
        if mask is not None:
            hit &= ~mask[y0:y1]
        n = int(hit.sum())
        if n:
            count += n
            if len(pos) < 20:
                ys, xs = np.nonzero(hit)
                pos += [[int(x), int(y + y0)] for y, x in list(zip(ys, xs))[:20 - len(pos)]]
    return count, pos


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


def exclusion_dir(cam, ex, beat=None, shot=None):
    """Camera-space direction of an exclusion entry, or None when it cannot be placed.
    Forms: "ndc": [x, y] (half-extents of this image, or of "ndcAspect": [w, h] at the same vertical fov),
    "px": [x, y], "azEl": [az, el] | "sun" (needs yaw/pitch; the sun direction comes from the shot's report entry
    "sun": [az, el], because it depends on s), "world": [x, y, z] | "lantern" (needs the camera position too; the
    lantern glass centre comes from BulwarkConst)."""
    if cam is None:
        return None
    if "ndc" in ex:
        nx, ny = float(ex["ndc"][0]), float(ex["ndc"][1])
        asp = ex.get("ndcAspect")
        aspect = (float(asp[0]) / float(asp[1])) if asp else cam.W / float(cam.H)
        t = math.tan(math.radians(cam.fov) * 0.5)
        return np.array([nx * aspect * t, ny * t, 1.0])
    if "px" in ex:
        return cam.cam_dir_px(float(ex["px"][0]), float(ex["px"][1]))
    if cam.yaw is None or cam.pitch is None:
        return None
    sp = spec()
    if "azEl" in ex:
        a = ex["azEl"]
        if a == "sun":
            raw = (shot or {}).get("raw") or {}
            a = raw.get("sun")
            if not a or len(a) < 2:
                return None
        return cam.azel_to_cam(float(a[0]), float(a[1]))
    if "world" in ex:
        if cam.pos is None:
            return None
        w = sp.lantern if ex["world"] == "lantern" else ex["world"]
        if w is None:
            return None
        d = np.array([float(v) for v in w]) - cam.pos
        if np.linalg.norm(d) < 1e-9:
            return None
        return cam.world_dir_to_cam(d)
    return None


def _srgb_to_lab(rgb):
    """sRGB code values (..., 3) 0-255 -> CIE Lab (D65)."""
    c = np.asarray(rgb, dtype=np.float64) / 255.0
    lin = np.where(c <= 0.04045, c / 12.92, ((c + 0.055) / 1.055) ** 2.4)
    M = np.array([[0.4124564, 0.3575761, 0.1804375], [0.2126729, 0.7151522, 0.0721750], [0.0193339, 0.1191920, 0.9503041]])
    xyz = lin @ M.T / np.array([0.95047, 1.0, 1.08883])
    f = np.where(xyz > (6.0 / 29.0) ** 3, np.cbrt(xyz), xyz / (3 * (6.0 / 29.0) ** 2) + 4.0 / 29.0)
    return np.stack([116 * f[..., 1] - 16, 500 * (f[..., 0] - f[..., 1]), 200 * (f[..., 1] - f[..., 2])], axis=-1)


def horizon_step_trend(rgb8, rows_v, cols, H, near=2.0, far=7.0):
    """Trend-removed horizon step (Wave B; atmosphere owner's method): per column a straight line per channel is fitted
    to the rows h-far..h-near above and h+near..h+far below the horizon row h (px at 1080p, scaled with H), both lines
    are extrapolated to h and compared as the Lab dE of the column-median extrapolations. A steep but continuous
    gradient on either side (dusk sky, low-sun lowland) then scores ~0; only a real step at the horizon counts."""
    sc = H / 1080.0
    a0, a1 = near * sc, far * sc
    ups, dns = [], []
    for c, v in zip(cols, rows_v):
        ra = np.arange(int(np.floor(v - a1)), int(np.ceil(v - a0)) + 1)
        rb = np.arange(int(np.floor(v + a0)), int(np.ceil(v + a1)) + 1)
        ra = ra[(ra >= 0) & (ra < H) & (ra + 0.5 <= v - a0 + 1e-9) & (ra + 0.5 >= v - a1 - 1e-9)]
        rb = rb[(rb >= 0) & (rb < H) & (rb + 0.5 >= v + a0 - 1e-9) & (rb + 0.5 <= v + a1 + 1e-9)]
        if ra.size < 2 or rb.size < 2:
            continue
        ya = rgb8[ra, c].astype(np.float64)
        yb = rgb8[rb, c].astype(np.float64)
        xa, xb = ra + 0.5 - v, rb + 0.5 - v
        ua = [np.polyval(np.polyfit(xa, ya[:, k], 1), 0.0) for k in range(3)]
        ub = [np.polyval(np.polyfit(xb, yb[:, k], 1), 0.0) for k in range(3)]
        ups.append(ua)
        dns.append(ub)
    if len(ups) < 8:
        return None
    up, dn = np.median(np.array(ups), axis=0), np.median(np.array(dns), axis=0)
    la, lb = _srgb_to_lab(np.clip(up, 0, 255)), _srgb_to_lab(np.clip(dn, 0, 255))
    return {"above": up, "below": dn, "dE": float(np.linalg.norm(la - lb)), "n_cols": len(ups)}


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
    if item.get("row") is not None:
        # a fixed horizon row at the reference height (1080) scaled to this image
        v = np.full(cols.shape, float(item["row"]) * H / float((item.get("_ref") or [1920, 1080])[1]))
    elif row_override is not None:
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
    out = {"sky": cs, "ground": cg, "spread_sky": spread(sky), "spread_ground": spread(gnd),
           "row_mid": float(np.median(v)), "diff": float(np.abs(cs - cg).max() / 255.0), "n_cols": int(len(cols))}
    if item.get("mode") == "trend":
        t = horizon_step_trend(rgb8, v, cols, H)
        if t is None:
            return None
        out["trend"] = t
    return out


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


# --------------------------------------------------------------------------------------------- banding (14.10.3 + R6 6.2)
def _box(p, k):
    if k <= 1:
        return p.copy()
    ker = np.ones(k) / float(k)
    return np.stack([np.convolve(p[:, c], ker, mode="valid") for c in range(p.shape[1])], axis=1)


def _runs_identical(prof, eps):
    """Runs of identical values in a 1-D profile: (start, length) for every run (length >= 1, in pixels)."""
    n = prof.shape[0]
    if n == 0:
        return np.zeros(0, np.int64), np.zeros(0, np.int64)
    same = np.abs(np.diff(prof)) <= eps
    brk = np.concatenate(([True], ~same))
    starts = np.flatnonzero(brk)
    lengths = np.diff(np.concatenate((starts, [n])))
    return starts, lengths


def strip_metrics(p, th=TH, H=1080):
    """14.10.3 BandScore on one strip.  p: L x C profile (mean over the strip's 128+ columns) in 8-bit levels.
    Longest run of identical values (every slope included) and, where the local slope is below 0.1 level/px, the
    mean run against 0.5 / slope.  "Identical" = consecutive profile values closer than half a level divided by the
    column count (no column changed)."""
    L, Cn = p.shape[0], p.shape[1]
    ncols = max(1, int(p.shape[2]) if p.ndim > 2 else int(th.get("_ncols", th["band_cols"])))
    eps = 0.5 / float(ncols)
    longest, mean_runs, worst_ratio = 0, [], 0.0
    half = int(th["band_slope_half"])
    for c in range(Cn):
        prof = p[:, c]
        starts, lengths = _runs_identical(prof, eps)
        if lengths.size:
            longest = max(longest, int(lengths.max()))
        # local slope over +-32 px of a 9-px box-smoothed profile
        s9 = np.convolve(prof, np.ones(9) / 9.0, mode="same")
        i = np.arange(L)
        lo, hi = np.maximum(i - half, 0), np.minimum(i + half, L - 1)
        ls = np.abs(s9[hi] - s9[lo]) / np.maximum(hi - lo, 1)
        centres = np.clip(starts + lengths // 2, 0, L - 1)
        gentle = ls[centres] < th["band_gentle_slope"]
        if gentle.any():
            mr = float(lengths[gentle].mean())
            sl = max(float(np.median(ls[centres][gentle])), th["band_slope_floor"])
            lim = th["band_mean_run_k"] / sl
            mean_runs.append((mr, lim))
            worst_ratio = max(worst_ratio, mr / lim)
    run_max = band_run_max(H, th)
    x = np.arange(L, dtype=np.float64)
    slope = np.atleast_1d(np.polyfit(x, p, 1)[0]) if L >= 2 else np.zeros(Cn)
    mr_worst = max(mean_runs, key=lambda v: v[0] / v[1]) if mean_runs else (None, None)
    return {"len": int(L), "longest_run_px": int(longest), "longest_run_max_px": run_max,
            "mean_run_px": mr_worst[0], "mean_run_limit_px": mr_worst[1], "mean_run_ratio": worst_ratio if mean_runs else None,
            "run_applicable": True, "steps_per_1000": None, "resid_rms": None, "resid_pp": None,
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
    """14.10.3 BandScore over a region.  img: H x W x 3 (levels).  mask: H x W bool, True = excluded (sun, objects).
    The 8-bit display luma is averaged over strips of 128 columns (or the whole region width when it is narrower,
    at least 128 recommended) perpendicular to the gradient; strips split around excluded rows.  Returns the worst
    values, the per-strip list and pass/score (score <= 1 passes: max of longest run / limit and mean run / limit)."""
    H, W = img.shape[:2]
    Himg = H
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
    sw = int(th["band_cols"])
    n_strips = reg.shape[1] // sw
    widths = [(k * sw, (k + 1) * sw) for k in range(n_strips)] if n_strips else [(0, reg.shape[1])]
    min_len = int(th["band_min_len"])
    # Rec.709 luma of the 8-bit display values, rounded to 8-bit levels per pixel ("8-bit luma profile")
    lum = np.rint(0.2126 * reg[..., 0] + 0.7152 * reg[..., 1] + 0.0722 * reg[..., 2])
    strips = []
    for (a, b) in widths:
        ok = ~rmask[:, a:b].any(axis=1)
        thc = dict(th)
        thc["_ncols"] = b - a
        for (s, e) in _segments(ok, min_len):
            prof = lum[s:e, a:b].mean(axis=1)[:, None]
            m = strip_metrics(prof, thc, Himg)
            m["strip"] = [int(a), int(b)]
            m["rows"] = [int(s), int(e)]
            strips.append(m)
    res = {"direction": direction, "n_strips": len(strips), "strips": strips}
    if not strips:
        res.update({"pass": None, "score": None, "note": "no strip long enough (region too small or masked)"})
        return res
    ratios = [m["mean_run_ratio"] for m in strips if m["mean_run_ratio"] is not None]
    worst_mr = max((m for m in strips if m["mean_run_ratio"] is not None), key=lambda m: m["mean_run_ratio"], default=None)
    worst = {
        "longest_run_px": max(m["longest_run_px"] for m in strips),
        "longest_run_max_px": band_run_max(Himg, th),
        "mean_run_px": worst_mr["mean_run_px"] if worst_mr else None,
        "mean_run_limit_px": worst_mr["mean_run_limit_px"] if worst_mr else None,
        "mean_run_ratio": max(ratios) if ratios else None,
        "steps_per_1000": None, "resid_rms": None, "resid_pp": None,
        "slope_abs_max": max(max(abs(v) for v in m["slope_code_per_px"]) for m in strips),
    }
    parts = [worst["longest_run_px"] / float(worst["longest_run_max_px"])]
    if worst["mean_run_ratio"] is not None:
        parts.append(worst["mean_run_ratio"])
    score = max(parts)
    res.update(worst)
    res["score"] = float(score)
    res["pass"] = bool(score <= 1.0)
    return res


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
            sr = float(H) if units == "frac" else (1.0 if (units == "px" or not ref) else H / float(ref[1]))
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
    for key in ("position", "pos"):   # CaptureHarness writes "pos": [x, y, z]
        if out.get("x") is None and isinstance(d.get(key), (list, tuple)) and len(d[key]) == 3:
            out["x"], out["y"], out["z"] = [_num(v) for v in d[key]]
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
    if beat is None and re.search(r"(?:^|_)(?:b|beat)0*8(?=_|$)", tag or "", re.IGNORECASE):
        # the ending's stills (Beat08Ending, "b08_Ending_*"): A1 4.10 beat 8 (ride) / 9 (platform, look back) by s
        beat = sp.ending_beat(s)
    if beat is None:
        beat = beat_from_tag(tag)
    if beat is None and s is not None:
        beat = sp.beat_of_s(s)
    ks, ks_end = sp.keyshot_match(tag)
    if ks is not None:
        # a related capture (e.g. "KS09_spill_grate_s575", or a different s) is not the key shot itself
        rest = tag[ks_end:]
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
        if pose.get("fov") is None:
            pose["fov"] = ks["fov"]
    lr = it.get("lumaRange", it.get("expectedLuma"))
    if isinstance(lr, dict):
        lr = (lr.get("min"), lr.get("max"))
    if lr is None and ks:
        lr = ks["luma"]
    # off-path material sheets (set "materials", tags "mat_*") are rigs, not beat frames: no 14.10.3 beat band
    # (every other check stays)
    off_path = (it.get("set") == "materials") or tag.startswith("mat_")
    if lr is None and beat is not None and not off_path:
        lr = mean_luma_range(beat)
    dark = None
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
