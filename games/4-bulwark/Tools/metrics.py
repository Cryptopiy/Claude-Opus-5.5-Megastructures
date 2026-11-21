#!/usr/bin/env python3
"""Project metrics for Bulwark (MASTER_PROMPT Part 15.3; imported from Thread, parametrised per lesson 3.2 #16).

Measures, from the Claude Code transcripts of THIS run only:
  * total tokens per category (uncached input, cache read, cache write 5 m / 1 h, output), deduplicated by message id
    (else request id, else record uuid): a streamed message is written once per content block with the same usage, so
    it counts once (the copy with the largest output count is kept),
  * wall-clock time from the run anchor to the last record, and active time (wall-clock minus every gap longer than
    10 minutes without any transcript activity),
  * API-equivalent cost from a pricing file (Tools/pricing.json) whose numbers come from the official Anthropic
    pricing page (URL and access date stored in that file; re-fetch it at the end, 15.3.4).

Which sessions count (Part 15.3.1). A top-level session <folder>/<id>.jsonl (with its subtree <folder>/<id>/**, i.e.
subagent and workflow transcripts) is ACCEPTED when one of its transcripts has
  (a) a Write / Edit / MultiEdit / Bash / PowerShell tool_use that wrote "Run started at" into STATE.md of this project
      (the anchor write of Phase 0, or a later rewrite of the same file), or
  (b) a record at or after the anchor whose cwd is the project root (or a folder inside it).
A session whose transcript merely CONTAINS the anchor line (a tool_result that read STATE.md, a prompt that quotes it)
is not accepted for that reason. A transcript anywhere else whose records carry the sessionId of an accepted session
(a subagent stored outside the subtree) is included too. --session FOLDER/ID adds a session by hand.
Exclusions: the consulting folder named EXACTLY W--YouTube-Megastructures (a prefix of every project folder).

Which records count: only records with a timestamp at or after the anchor (the first line of STATE.md, "Run started at
<ISO 8601 with offset>"; --anchor overrides the time). A continued chat (this run started inside the Abyss chat) keeps
its earlier game's records in the same transcript: they are before the anchor and are reported separately as excluded.

Run:  python -I Tools/metrics.py [--md QA/metrics.md] [--json QA/metrics.json] [--list] [--anchor ISO] [--selftest]
Read-only on the transcript folders. No third-party packages.
"""
import argparse
import glob
import json
import os
import re
import shutil
import sys
import tempfile
from datetime import datetime, timedelta, timezone

PROJECTS_ROOT = os.path.join(os.path.expanduser("~"), ".claude", "projects")
EXCLUDE = ["W--YouTube-Megastructures"]
GAP_SECONDS = 600.0
HERE = os.path.dirname(os.path.abspath(__file__))
ROOT = os.path.dirname(HERE)
WRITE_TOOLS = {"Write", "Edit", "MultiEdit", "NotebookEdit", "Bash", "PowerShell"}
# a shell command that writes STATE.md: PowerShell write cmdlets / .NET writers, a shell redirection, tee or sed -i, with
# STATE.md as the target on the same line (S4 of the Wave A fixes critic: a grep of STATE.md is not an anchor write)
SHELL_STATE_WRITE = re.compile(r"(Set-Content|Add-Content|Out-File|WriteAllText|AppendAllText|>>?|\btee\b|\bsed\s+-i)[^\n]*state\.md",
                               re.IGNORECASE)
ANCHOR_TEXT = "Run started at"
ISO_RE = re.compile(r"(\d{4}-\d{2}-\d{2})[T ](\d{2}:\d{2}:\d{2})(?:\.(\d+))?\s*(Z|[+-]\d{2}:?\d{2})?")
CWD_RE = re.compile(r'"cwd"\s*:\s*"((?:[^"\\]|\\.)*)"')
TS_RE = re.compile(r'"timestamp"\s*:\s*"([^"]+)"')
SID_RE = re.compile(r'"sessionId"\s*:\s*"([^"]+)"')
DRIVE_RE = re.compile(r"(?<![a-z0-9:])/([a-z])/")


def parse_ts(s):
    """ISO 8601 -> aware datetime (UTC when no offset). Accepts 7-digit fractions (PowerShell 'o' format)."""
    if not s:
        return None
    m = ISO_RE.search(s)
    if not m:
        return None
    date, time, frac, off = m.groups()
    frac = (frac or "0")[:6].ljust(6, "0")
    if off is None or off == "Z":
        off = "+00:00"
    elif ":" not in off:
        off = off[:3] + ":" + off[3:]
    try:
        return datetime.fromisoformat(f"{date}T{time}.{frac}{off}")
    except ValueError:
        return None


def hms(seconds):
    seconds = int(round(seconds))
    h, rem = divmod(seconds, 3600)
    m, s = divmod(rem, 60)
    return f"{h}:{m:02d}:{s:02d}"


def norm_path(p):
    """Case-insensitive, forward slashes, Git-Bash drive form (/w/...) mapped to w:/..., no trailing slash."""
    p = (p or "").replace("\\\\", "/").replace("\\", "/").lower()
    p = DRIVE_RE.sub(lambda m: m.group(1) + ":/", " " + p)[1:]
    while "//" in p:
        p = p.replace("//", "/")
    return p.rstrip("/")


def under_root(cwd, root_n):
    c = norm_path(cwd)
    return c == root_n or c.startswith(root_n + "/")


def read_anchor(state_path):
    """First line of STATE.md: 'Run started at <ISO>'. Returns (line, datetime) or (None, None)."""
    try:
        with open(state_path, "r", encoding="utf-8-sig", errors="replace") as f:
            line = f.readline().strip()
    except OSError:
        return None, None
    ts = parse_ts(line) if line.lower().startswith("run started at") else None
    return (line if ts else None), ts


def session_of(folder, path):
    """Top-level session id of a transcript file: <folder>/<id>.jsonl or <folder>/<id>/..."""
    rel = os.path.relpath(path, folder).replace("\\", "/")
    first = rel.split("/")[0]
    return first[:-6] if first.endswith(".jsonl") else first


def session_files(folder, sid):
    files = []
    top = os.path.join(folder, sid + ".jsonl")
    if os.path.isfile(top):
        files.append(top)
    files.extend(sorted(glob.glob(os.path.join(folder, sid, "**", "*.jsonl"), recursive=True)))
    return files


def strings_of(x, out):
    if isinstance(x, str):
        out.append(x)
    elif isinstance(x, dict):
        for v in x.values():
            strings_of(v, out)
    elif isinstance(x, list):
        for v in x:
            strings_of(v, out)


def anchor_write(obj, root_n):
    """Name of the tool when this record is an assistant tool_use that wrote 'Run started at' into <root>/STATE.md."""
    msg = obj.get("message") or {}
    content = msg.get("content")
    if not isinstance(content, list):
        return None
    state_n = root_n + "/state.md"
    cwd_ok = under_root(obj.get("cwd") or "", root_n)
    for c in content:
        if not isinstance(c, dict) or c.get("type") != "tool_use" or c.get("name") not in WRITE_TOOLS:
            continue
        inp = c.get("input") or {}
        texts = []
        strings_of(inp, texts)
        text = "\n".join(texts)
        if ANCHOR_TEXT not in text or "state.md" not in text.lower():
            continue
        fp = inp.get("file_path") or inp.get("notebook_path") if isinstance(inp, dict) else None
        if fp:
            if norm_path(fp) == state_n:
                return c.get("name")
            continue
        # shell: the command must WRITE STATE.md (a write verb or a redirection whose target is STATE.md; reading it with
        # grep / cat / Get-Content is not a write) and name the project root (absolute path) or run inside it
        if not SHELL_STATE_WRITE.search(text):
            continue
        if root_n in norm_path(text) or cwd_ok:
            return c.get("name")
    return None


def scan_file(path, root_n, anchor_ts, anchor_line=None):
    """One pass: anchor writes, cwd evidence at/after the anchor, the session ids carried by the records, and whether the
    exact anchor line appears anywhere (only reported: quoting the anchor line is not evidence)."""
    info = {"anchor_write": None, "cwd_hits": 0, "sids": set(), "anchor_text": False}
    try:
        with open(path, "r", encoding="utf-8", errors="replace") as f:
            for line in f:
                if anchor_line and not info["anchor_text"] and anchor_line in line:
                    info["anchor_text"] = True
                if '"sessionId"' in line and len(info["sids"]) < 4:
                    m = SID_RE.search(line)
                    if m:
                        info["sids"].add(m.group(1))
                if ANCHOR_TEXT in line and '"tool_use"' in line and info["anchor_write"] is None:
                    try:
                        obj = json.loads(line)
                    except Exception:
                        obj = None
                    if isinstance(obj, dict) and obj.get("type") == "assistant":
                        tool = anchor_write(obj, root_n)
                        if tool:
                            info["anchor_write"] = (tool, obj.get("timestamp"))
                if '"cwd"' in line:
                    m = CWD_RE.search(line)
                    if not m:
                        continue
                    try:
                        cwd = json.loads('"' + m.group(1) + '"')
                    except Exception:
                        continue
                    if not under_root(cwd, root_n):
                        continue
                    t = TS_RE.search(line)
                    ts = parse_ts(t.group(1)) if t else None
                    if anchor_ts is None or (ts is not None and ts >= anchor_ts):
                        info["cwd_hits"] += 1
    except OSError:
        pass
    return info


def discover(a, root_n, anchor_ts):
    """Returns (accepted {(folder, sid): reason}, chosen {path: reason}, rejected [(folder, sid, why)])."""
    excluded = set(a.exclude)
    folders = [f for f in sorted(glob.glob(os.path.join(a.projects_root, "*")))
               if os.path.isdir(f) and os.path.basename(f) not in excluded]
    min_mtime = anchor_ts.timestamp() - 60 if anchor_ts else 0
    scans = {}            # path -> (folder, sid, info)
    for folder in folders:
        for path in glob.glob(os.path.join(folder, "**", "*.jsonl"), recursive=True):
            try:
                if os.path.getmtime(path) < min_mtime:
                    continue
            except OSError:
                continue
            scans[path] = (folder, session_of(folder, path), scan_file(path, root_n, anchor_ts, getattr(a, "anchor_line", None)))
    accepted = {}
    for path, (folder, sid, info) in scans.items():
        key = (folder, sid)
        if info["anchor_write"]:
            accepted[key] = "anchor write (%s at %s)" % info["anchor_write"]
        elif info["cwd_hits"] and key not in accepted:
            accepted[key] = "cwd = project root after the anchor"
    for folder, sid in a.session_pairs:
        accepted.setdefault((folder, sid), "--session")
    chosen = {}
    for (folder, sid), why in accepted.items():
        for p in session_files(folder, sid):
            chosen[p] = "session " + sid[:8]
    # transcripts stored outside an accepted subtree but carrying an accepted session id
    acc_ids = {sid for _, sid in accepted}
    for path, (folder, sid, info) in scans.items():
        if path not in chosen and info["sids"] & acc_ids:
            chosen[path] = "sessionId " + sorted(info["sids"] & acc_ids)[0][:8]
    # sessions that only quote the anchor line (the old false positive): reported, not counted
    rejected, seen = [], set()
    for path, (folder, sid, info) in sorted(scans.items()):
        if info["anchor_text"] and (folder, sid) not in accepted and (folder, sid) not in seen and path not in chosen:
            seen.add((folder, sid))
            rejected.append((os.path.basename(folder), sid, "contains the anchor line only (no anchor write, no project cwd)"))
    return accepted, chosen, rejected


def usage_rec(obj, rel):
    msg = obj.get("message") or {}
    usage = msg.get("usage")
    if not usage:
        return None, None
    key = msg.get("id") or obj.get("requestId") or obj.get("uuid")
    cc = usage.get("cache_creation") or {}
    return key, {
        "model": msg.get("model", "unknown"),
        "input": int(usage.get("input_tokens") or 0),
        "cache_read": int(usage.get("cache_read_input_tokens") or 0),
        "cache_write": int(usage.get("cache_creation_input_tokens") or 0),
        "cache_write_5m": int(cc.get("ephemeral_5m_input_tokens") or 0),
        "cache_write_1h": int(cc.get("ephemeral_1h_input_tokens") or 0),
        "output": int(usage.get("output_tokens") or 0),
        "file": rel,
    }


def collect(chosen, projects_root, anchor_ts):
    messages, before = {}, {}      # dedup key -> usage record (at/after the anchor; before it)
    timestamps = []
    raw_after = 0                  # assistant records with usage at/after the anchor (before dedup)
    api_requests = 0               # 'api-request' records at/after the anchor (one per API call; cross-check)
    skipped_before = 0
    synthetic = 0
    n_main = n_sub = n_wf = 0
    per_file = {}
    for path in sorted(chosen):
        rel = os.path.relpath(path, projects_root).replace("\\", "/")
        parts = rel.split("/")
        if "workflows" in parts:
            n_wf += 1
        elif "subagents" in parts:
            n_sub += 1
        else:
            n_main += 1
        pf = per_file.setdefault(rel, {"records": 0, "ids": set(), "api": 0})
        with open(path, "r", encoding="utf-8", errors="replace") as f:
            for line in f:
                line = line.strip()
                if not line:
                    continue
                try:
                    obj = json.loads(line)
                except Exception:
                    continue
                if not isinstance(obj, dict):
                    continue
                ts = parse_ts(obj.get("timestamp"))
                is_after = anchor_ts is None or (ts is not None and ts >= anchor_ts)
                if obj.get("type") == "assistant":
                    key, rec = usage_rec(obj, rel)
                    if rec is not None and rec["model"] == "<synthetic>":
                        synthetic += 1
                        rec = None
                    if rec is not None:
                        target = messages if is_after else before
                        if is_after:
                            raw_after += 1
                            pf["ids"].add(key)
                        old = target.get(key)
                        if old is None or rec["output"] >= old["output"]:
                            target[key] = rec
                if ts is None:
                    continue
                if not is_after:
                    skipped_before += 1
                    continue
                timestamps.append(ts)
                pf["records"] += 1
                if obj.get("type") == "api-request":
                    api_requests += 1
                    pf["api"] += 1
    # a streamed message whose copies straddle the anchor counts once, after it
    for k in list(before):
        if k in messages:
            del before[k]
    models = {}
    for rec in messages.values():
        models[rec["model"]] = models.get(rec["model"], 0) + 1
    return {"messages": messages, "before": before, "timestamps": timestamps, "models": models,
            "files": (n_main, n_sub, n_wf), "skipped_before": skipped_before, "raw_after": raw_after,
            "api_requests": api_requests, "synthetic": synthetic, "per_file": per_file}


def token_sums(msgs):
    tok = {k: sum(r[k] for r in msgs.values())
           for k in ("input", "cache_read", "cache_write", "cache_write_5m", "cache_write_1h", "output")}
    tok["total"] = tok["input"] + tok["cache_read"] + tok["cache_write"] + tok["output"]
    return tok


def time_stats(timestamps, anchor_ts):
    if not timestamps:
        return None
    ts = sorted(timestamps)
    first = anchor_ts if anchor_ts is not None and anchor_ts <= ts[0] else ts[0]
    seq = [first] + ts
    wall = (ts[-1] - first).total_seconds()
    gaps = []
    for x, y in zip(seq, seq[1:]):
        d = (y - x).total_seconds()
        if d > GAP_SECONDS:
            gaps.append((x, y, d))
    active = wall - sum(g[2] for g in gaps)
    utc = timezone.utc
    return {"first": first.astimezone(utc), "last": ts[-1].astimezone(utc), "wall": wall,
            "gaps": [(x.astimezone(utc), y.astimezone(utc), d) for x, y, d in gaps], "active": active}


def cost(messages, pricing):
    """Cost per category in USD. pricing: per-million-token prices."""
    if not pricing:
        return None
    p = pricing["prices_per_mtok"]
    lc = pricing.get("long_context")  # optional {"threshold_input_tokens": N, "multipliers": {...}}
    totals = {"input": 0.0, "cache_read": 0.0, "cache_write_5m": 0.0, "cache_write_1h": 0.0, "output": 0.0}
    for rec in messages.values():
        mult = {k: 1.0 for k in totals}
        if lc:
            prompt = rec["input"] + rec["cache_read"] + rec["cache_write"]
            if prompt > lc["threshold_input_tokens"]:
                for k, v in lc["multipliers"].items():
                    mult[k] = v
        w5 = rec["cache_write_5m"]
        w1 = rec["cache_write_1h"]
        if w5 + w1 == 0 and rec["cache_write"] > 0:
            w5 = rec["cache_write"]  # no split given: price as 5-minute writes
        totals["input"] += rec["input"] * p["input"] * mult["input"] / 1e6
        totals["cache_read"] += rec["cache_read"] * p["cache_read"] * mult["cache_read"] / 1e6
        totals["cache_write_5m"] += w5 * p["cache_write_5m"] * mult["cache_write_5m"] / 1e6
        totals["cache_write_1h"] += w1 * p["cache_write_1h"] * mult["cache_write_1h"] / 1e6
        totals["output"] += rec["output"] * p["output"] * mult["output"] / 1e6
    totals["total"] = sum(totals.values())
    return totals


def parse_args(argv):
    ap = argparse.ArgumentParser(description=__doc__.split("\n\n")[0], epilog=__doc__,
                                 formatter_class=argparse.RawDescriptionHelpFormatter)
    ap.add_argument("--pricing", default=os.path.join(HERE, "pricing.json"))
    ap.add_argument("--json", default=None)
    ap.add_argument("--md", default=None)
    ap.add_argument("--projects-root", default=PROJECTS_ROOT)
    ap.add_argument("--project-root", default=ROOT, help="this project's root (cwd test and STATE.md path; default %(default)s)")
    ap.add_argument("--exclude", action="append", default=None,
                    help="folder basename excluded EXACTLY (repeatable; default W--YouTube-Megastructures)")
    ap.add_argument("--state", default=None, help="STATE.md whose first line is the anchor (default <project-root>/STATE.md)")
    ap.add_argument("--anchor", default=None, help="ISO 8601 start (default: first line of STATE.md)")
    ap.add_argument("--no-anchor", action="store_true", help="count every record (no start filter)")
    ap.add_argument("--session", action="append", default=[], metavar="FOLDER/ID",
                    help="add a session subtree by hand (folder basename / session id)")
    ap.add_argument("--list", action="store_true", help="print the transcript files used and why")
    ap.add_argument("--selftest", action="store_true", help="run the synthetic-transcript self-test and exit")
    a = ap.parse_args(argv)
    if a.exclude is None:
        a.exclude = list(EXCLUDE)
    if a.state is None:
        a.state = os.path.join(a.project_root, "STATE.md")
    a.session_pairs = []
    for s in a.session:
        fb, _, sid = s.replace("\\", "/").partition("/")
        a.session_pairs.append((os.path.join(a.projects_root, fb), sid))
    return a


def measure(a):
    anchor_line, anchor_ts = (None, None)
    if not a.no_anchor:
        anchor_line, anchor_ts = read_anchor(a.state)
        if a.anchor:
            anchor_ts = parse_ts(a.anchor)
            if anchor_ts is None:
                raise SystemExit("metrics: cannot parse --anchor " + a.anchor)
        elif anchor_ts is None:
            raise SystemExit(f"metrics: no 'Run started at <ISO>' first line in {a.state}; pass --anchor or --no-anchor")
    a.anchor_line = anchor_line
    root_n = norm_path(a.project_root)
    accepted, chosen, rejected = discover(a, root_n, anchor_ts)
    col = collect(chosen, a.projects_root, anchor_ts)
    return anchor_ts, accepted, chosen, rejected, col


def report(a, anchor_ts, accepted, chosen, rejected, col):
    msgs = col["messages"]
    tok = token_sums(msgs)
    tok_before = token_sums(col["before"])
    t = time_stats(col["timestamps"], anchor_ts)
    pricing = None
    if a.pricing and os.path.exists(a.pricing):
        with open(a.pricing, "r", encoding="utf-8") as f:
            pricing = json.load(f)
    c = cost(msgs, pricing)
    counts = col["files"]
    top_folders = sorted({os.path.relpath(p, a.projects_root).replace("\\", "/").split("/")[0] for p in chosen})
    rows = []
    if anchor_ts is not None:
        rows.append(("Run anchor", anchor_ts.isoformat(), "measured: STATE.md line 1 (or --anchor); only records at or after it count"))
    rows.append(("Sessions", "; ".join(f"{os.path.basename(f)}/{s} ({why})" for (f, s), why in sorted(accepted.items())) or "none",
                 "measured: sessions with an anchor write into this project's STATE.md or a cwd inside the project after the anchor; subagent and workflow transcripts of those sessions included"))
    rows.append(("Sessions rejected (anchor text only)", str(len(rejected)), "measured: transcripts that only quote the anchor line (e.g. a tool_result that read STATE.md); not counted"))
    rows.append(("Transcript folders", f"{len(top_folders)} ({', '.join(top_folders)})", f"measured; '{', '.join(a.exclude)}' excluded by exact name"))
    rows.append(("Transcript files (main / subagent / workflow)", f"{counts[0]} / {counts[1]} / {counts[2]}", "measured: *.jsonl of the accepted sessions"))
    rows.append(("Assistant messages (deduplicated)", f"{len(msgs):,}", f"measured: unique message.id with usage at/after the anchor ({col['raw_after']:,} streamed records before dedup; {col['api_requests']:,} api-request records; {col['synthetic']} synthetic records ignored)"))
    rows.append(("Models", ", ".join(f"{k} ({v})" for k, v in sorted(col["models"].items())) or "none", "measured: message.model"))
    rows.append(("Uncached input tokens", f"{tok['input']:,}", "measured: sum usage.input_tokens"))
    rows.append(("Cache read tokens", f"{tok['cache_read']:,}", "measured: sum usage.cache_read_input_tokens"))
    rows.append(("Cache write tokens (5 min / 1 h)", f"{tok['cache_write']:,} ({tok['cache_write_5m']:,} / {tok['cache_write_1h']:,})", "measured: usage.cache_creation_input_tokens and its ephemeral split"))
    rows.append(("Output tokens (incl. thinking)", f"{tok['output']:,}", "measured: sum usage.output_tokens"))
    rows.append(("Total tokens", f"{tok['total']:,}", "measured: sum of the four categories"))
    rows.append(("Excluded: before the anchor", f"{col['skipped_before']:,} records, {len(col['before']):,} messages, {tok_before['total']:,} tokens",
                 "measured: records of the accepted sessions older than the anchor (the earlier game of a continued chat); not counted"))
    if t:
        rows.append(("Wall-clock time", hms(t["wall"]), f"measured: from {t['first'].isoformat()} (anchor) to the last record {t['last'].isoformat()}"))
        rows.append(("Idle gaps > 10 min", f"{len(t['gaps'])} gaps, {hms(sum(g[2] for g in t['gaps']))}", "measured: consecutive timestamps (anchor included) further apart than 600 s"))
        rows.append(("Active time", hms(t["active"]), "derived: wall-clock minus all gaps > 10 min"))
    if c:
        pp = pricing["prices_per_mtok"]
        rows.append(("Cost: uncached input", f"${c['input']:,.2f}", f"estimated: tokens x ${pp['input']}/MTok"))
        rows.append(("Cost: cache read", f"${c['cache_read']:,.2f}", f"estimated: tokens x ${pp['cache_read']}/MTok"))
        rows.append(("Cost: cache write 5 min", f"${c['cache_write_5m']:,.2f}", f"estimated: tokens x ${pp['cache_write_5m']}/MTok"))
        rows.append(("Cost: cache write 1 h", f"${c['cache_write_1h']:,.2f}", f"estimated: tokens x ${pp['cache_write_1h']}/MTok"))
        rows.append(("Cost: output", f"${c['output']:,.2f}", f"estimated: tokens x ${pp['output']}/MTok"))
        note = " (pricing.json is still the TEMPLATE: re-fetch at the end, 15.3.4)" if "TEMPLATE" in str(pricing.get("note", "")) else ""
        rows.append(("Total API-equivalent cost", f"${c['total']:,.2f}", f"estimated: sum of categories; prices from {pricing.get('source_url', '?')} accessed {pricing.get('accessed', '?')} for {pricing.get('model', '?')}{note}"))
    else:
        rows.append(("Total API-equivalent cost", "unavailable", "no pricing file; cost = in*P_in + cr*P_cr + w5*P_w5 + w1*P_w1 + out*P_out"))

    md = ["| Metric | Value | How obtained |", "|---|---|---|"]
    md += [f"| {x} | {y} | {z} |" for x, y, z in rows]
    if t and t["gaps"]:
        md += ["", "Gaps longer than 10 minutes (UTC):"]
        md += [f"- {x.isoformat()} -> {y.isoformat()} ({hms(d)})" for x, y, d in t["gaps"]]
    if rejected:
        md += ["", "Rejected sessions (anchor text only):"]
        md += [f"- {fb}/{sid}: {why}" for fb, sid, why in rejected]
    if a.list:
        md += ["", "Transcript files used (records at/after the anchor, unique messages, api-request records):"]
        for p, why in sorted(chosen.items()):
            rel = os.path.relpath(p, a.projects_root).replace("\\", "/")
            pf = col["per_file"].get(rel, {"records": 0, "ids": set(), "api": 0})
            md.append(f"- [{why}] {rel}: {pf['records']} records, {len(pf['ids'])} messages, {pf['api']} api-requests")
    text = "\n".join(md)
    out = {"anchor": anchor_ts.isoformat() if anchor_ts else None, "tokens": tok, "messages": len(msgs),
           "streamed_records": col["raw_after"], "api_requests": col["api_requests"], "synthetic_ignored": col["synthetic"],
           "files": counts, "folders": top_folders,
           "sessions": [{"folder": os.path.basename(f), "id": s, "reason": why} for (f, s), why in sorted(accepted.items())],
           "rejected": [{"folder": fb, "id": sid, "reason": why} for fb, sid, why in rejected],
           "excluded_before_anchor": {"records": col["skipped_before"], "messages": len(col["before"]), "tokens": tok_before},
           "models": col["models"], "cost": c,
           "time": None if not t else {"first": t["first"].isoformat(), "last": t["last"].isoformat(),
                                       "wall_s": t["wall"], "active_s": t["active"],
                                       "gaps": [(x.isoformat(), y.isoformat(), d) for x, y, d in t["gaps"]]}}
    return text, out


# ------------------------------------------------------------------------------------------------ self-test
def selftest():
    """Synthetic transcripts: the anchor rules, the Abyss records before the anchor, the streamed duplicates, the exclusion."""
    tmp = tempfile.mkdtemp(prefix="bw_metrics_")
    try:
        root = r"W:\Fake\4 Megastructures\4 Megastructures"
        proj = os.path.join(tmp, "projects")
        anchor = datetime(2026, 11, 13, 18, 22, 9, tzinfo=timezone.utc)
        iso = lambda dt: dt.strftime("%Y-%m-%dT%H:%M:%S.000Z")
        state = os.path.join(tmp, "STATE.md")
        with open(state, "w", encoding="utf-8") as f:
            f.write("Run started at 2026-11-13T19:22:09.0000000+01:00\n\n# STATE\n")
        anchor_line = "Run started at 2026-11-13T19:22:09.0000000+01:00"

        def write(path, records):
            os.makedirs(os.path.dirname(path), exist_ok=True)
            with open(path, "w", encoding="utf-8") as f:
                for r in records:
                    f.write(json.dumps(r) + "\n")

        def asst(mid, dt, cwd, sid, out=10, inp=100, cr=1000, cw=50, content=None):
            return {"type": "assistant", "timestamp": iso(dt), "cwd": cwd, "sessionId": sid, "requestId": "req_" + mid,
                    "message": {"id": mid, "model": "claude-fable-5-2", "content": content or [{"type": "text", "text": "x"}],
                                "usage": {"input_tokens": inp, "cache_read_input_tokens": cr, "cache_creation_input_tokens": cw,
                                          "cache_creation": {"ephemeral_5m_input_tokens": 0, "ephemeral_1h_input_tokens": cw},
                                          "output_tokens": out}}}

        abyss = r"W:\Fake\1st Megastructures\1st Megastructures"
        sm = "main-0000"
        ps_write = [{"type": "tool_use", "name": "PowerShell", "id": "t1",
                     "input": {"command": '$p = "' + root + '"\n$ts = Get-Date -Format o\n"Run started at $ts" | Set-Content -Path "$p\\STATE.md"'}}]
        main = [
            asst("m_abyss1", anchor - timedelta(days=14), abyss, sm, out=500, inp=5000, cr=90000, cw=7000),   # earlier game
            asst("m_abyss2", anchor - timedelta(hours=1), abyss, sm, out=400),
            asst("m_write", anchor - timedelta(seconds=5), root, sm, content=ps_write),                       # the anchor write
            {"type": "user", "timestamp": iso(anchor + timedelta(seconds=1)), "cwd": root, "sessionId": sm,
             "message": {"role": "user", "content": [{"type": "tool_result", "content": anchor_line}]}},
        ]
        for k in range(3):   # one streamed message written three times (thinking, text, tool_use): counts once
            main.append(asst("m_stream", anchor + timedelta(seconds=10 + k), root, sm, out=77, inp=3, cr=2000, cw=0))
        main.append(asst("m_late", anchor + timedelta(minutes=30), root, sm, out=5, inp=1, cr=10, cw=0))   # after a 29.8 min gap
        write(os.path.join(proj, "W--Fake-1st-Megastructures", sm + ".jsonl"), main)
        # workflow subagent of the main session, no cwd marker of its own
        write(os.path.join(proj, "W--Fake-1st-Megastructures", sm, "subagents", "workflows", "wf_1", "agent-a.jsonl"),
              [asst("w1", anchor + timedelta(minutes=2), r"C:\Temp", sm, out=20, inp=2, cr=300, cw=40)])
        # another project's session that READ STATE.md of this project (tool_result with the anchor line): rejected
        other = "other-0000"
        write(os.path.join(proj, "W--Fake-5-Megastructures", other + ".jsonl"), [
            {"type": "user", "timestamp": iso(anchor + timedelta(minutes=5)), "cwd": r"W:\Fake\5 Megastructures", "sessionId": other,
             "message": {"role": "user", "content": [{"type": "tool_result", "content": anchor_line + "\n# STATE"}]}},
            asst("o1", anchor + timedelta(minutes=6), r"W:\Fake\5 Megastructures", other, out=999, inp=999, cr=999, cw=999)])
        # another project's session whose SHELL greps this project's STATE.md for the anchor line: rejected (not a write)
        reader = "reader-0000"
        ps_read = [{"type": "tool_use", "name": "Bash", "id": "t2",
                    "input": {"command": 'grep -n "Run started at" "' + root.replace("\\", "/") + '/STATE.md"'}}]
        write(os.path.join(proj, "W--Fake-6-Megastructures", reader + ".jsonl"), [
            asst("r1", anchor + timedelta(minutes=5), r"W:\Fake\6 Megastructures", reader, out=888, content=ps_read)])
        # the consulting folder (exact name excluded) even with an anchor write
        write(os.path.join(proj, "W--YouTube-Megastructures", "cons.jsonl"), [
            asst("c1", anchor + timedelta(minutes=7), root, "cons", out=555, content=ps_write)])
        # a later continuation session started elsewhere whose cwd is the project root after the anchor
        cont = "cont-0000"
        write(os.path.join(proj, "W--Fake-Other", cont + ".jsonl"), [
            asst("k1", anchor + timedelta(minutes=8), root + r"\Assets", cont, out=30, inp=4, cr=400, cw=0)])
        # a session in this project's folder that ended before the anchor only (nothing counts)
        write(os.path.join(proj, "W--Fake-4-Megastructures", "old.jsonl"), [
            asst("x1", anchor - timedelta(days=1), root, "old", out=1)])

        a = parse_args(["--projects-root", proj, "--project-root", root, "--state", state, "--pricing", ""])
        anchor_ts, accepted, chosen, rejected, col = measure(a)
        ids = {s for _, s in accepted}
        tok = token_sums(col["messages"])
        checks = [
            ("anchor parsed", anchor_ts == anchor),
            ("main session accepted by its anchor write", sm in ids and accepted[[k for k in accepted if k[1] == sm][0]].startswith("anchor write (PowerShell")),
            ("continuation accepted by cwd", cont in ids),
            ("reader of STATE.md rejected", other not in ids and any(r[1] == other for r in rejected)),
            ("shell reader of STATE.md rejected", reader not in ids),
            ("consulting folder excluded", "cons" not in ids),
            ("old session not accepted", "old" not in ids),
            ("workflow subagent included", any("wf_1" in p for p in chosen)),
            ("streamed copies count once", sum(1 for k in col["messages"] if k == "m_stream") == 1),
            ("messages after the anchor", sorted(col["messages"]) == ["k1", "m_late", "m_stream", "w1"]),
            ("Abyss records excluded", "m_abyss1" in col["before"] and "m_abyss2" in col["before"] and "m_write" in col["before"]),
            ("output tokens", tok["output"] == 77 + 5 + 20 + 30),
            ("input tokens", tok["input"] == 3 + 1 + 2 + 4),
            ("cache read", tok["cache_read"] == 2000 + 10 + 300 + 400),
            ("cache write 1 h split", tok["cache_write_1h"] == 40 and tok["cache_write"] == 40),
            ("raw streamed records", col["raw_after"] == 6),
        ]
        t = time_stats(col["timestamps"], anchor_ts)
        checks.append(("wall clock to the last record", abs(t["wall"] - 1800.0) < 1e-6))
        checks.append(("one gap > 10 min", len(t["gaps"]) == 1 and abs(t["active"] - (1800.0 - t["gaps"][0][2])) < 1e-6))
        ok = True
        for name, passed in checks:
            print(("PASS " if passed else "FAIL ") + name)
            ok = ok and passed
        print("selftest: " + ("PASS" if ok else "FAIL") + f" ({sum(1 for _, p in checks if p)}/{len(checks)})")
        return 0 if ok else 1
    finally:
        if os.environ.get("BW_METRICS_KEEP"):
            print("selftest fixture kept: " + tmp)
        else:
            shutil.rmtree(tmp, ignore_errors=True)


def main(argv=None):
    a = parse_args(argv)
    if a.selftest:
        return selftest()
    anchor_ts, accepted, chosen, rejected, col = measure(a)
    text, out = report(a, anchor_ts, accepted, chosen, rejected, col)
    print(text)
    if a.md:
        with open(a.md, "w", encoding="utf-8") as f:
            f.write(text + "\n")
    if a.json:
        with open(a.json, "w", encoding="utf-8") as f:
            json.dump(out, f, indent=2)
    return 0


if __name__ == "__main__":
    sys.exit(main())
