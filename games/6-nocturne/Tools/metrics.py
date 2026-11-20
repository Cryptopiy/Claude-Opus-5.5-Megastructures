#!/usr/bin/env python3
"""Project metrics for Nocturne (MASTER_PROMPT Part 15.3).

Finds the Claude Code transcripts of this run by the ANCHOR, the first line of STATE.md
("Run started at <ISO timestamp>"), not by folder name: this run continues a chat that started in
another project folder, and its subagent / workflow transcripts live under several project folders.

  1. Every *.jsonl under ~/.claude/projects (except the consulting folder W--YouTube-Megastructures and
     folders of other projects that never mention the anchor) is scanned for the exact anchor line.
     The session ids of the files that contain it are this run's sessions.
  2. Included: those session files plus every transcript stored below a folder named after one of those
     session ids (subagents/, workflows/), in any project folder.
  3. Only records with a timestamp at or after the anchor are counted (earlier records of a continued
     chat belong to the previous game).
  4. Tokens: message.usage of assistant messages, deduplicated by message id (else request id): uncached
     input, cache read, cache write 5 min / 1 h (from usage.cache_creation when split), output.
  5. Time: wall clock from the anchor to the last included timestamp; active time = wall clock minus every
     gap longer than 10 minutes between consecutive included timestamps (all records, all sessions merged).
  6. Cost: prices per million tokens from Tools/pricing.json (URL and access date stored there).

Run:  python -I Tools/metrics.py [--json out.json] [--md out.md] [--pricing Tools/pricing.json]
Read-only on the transcript folder. Standard library only.
"""
import argparse
import glob
import json
import os
import re
import sys
from datetime import datetime

HERE = os.path.dirname(os.path.abspath(__file__))
ROOT = os.path.dirname(HERE)
PROJECTS_ROOT = os.path.join(os.path.expanduser("~"), ".claude", "projects")
EXCLUDED_FOLDERS = {"W--YouTube-Megastructures"}   # the consulting session (Part 15.3 item 1)
GAP_SECONDS = 600.0


def parse_ts(s):
    if not s:
        return None
    try:
        if s.endswith("Z"):
            s = s[:-1] + "+00:00"
        return datetime.fromisoformat(s)
    except Exception:
        return None


def hms(seconds):
    seconds = int(round(seconds))
    h, rem = divmod(seconds, 3600)
    m, s = divmod(rem, 60)
    return f"{h}:{m:02d}:{s:02d}"


def read_anchor():
    with open(os.path.join(ROOT, "STATE.md"), "r", encoding="utf-8") as f:
        first = f.readline().strip()
    m = re.match(r"^Run started at (\S+)$", first)
    if not m:
        sys.exit("STATE.md does not start with 'Run started at <timestamp>'")
    ts = parse_ts(m.group(1))
    if ts is None or ts.tzinfo is None:
        sys.exit("anchor timestamp is not an ISO timestamp with a time zone: " + m.group(1))
    return first, ts


def all_transcripts():
    out = []
    for folder in glob.glob(os.path.join(PROJECTS_ROOT, "*")):
        if not os.path.isdir(folder) or os.path.basename(folder) in EXCLUDED_FOLDERS:
            continue
        out.extend(glob.glob(os.path.join(folder, "**", "*.jsonl"), recursive=True))
    return out


def contains(path, needle):
    try:
        with open(path, "r", encoding="utf-8", errors="replace") as f:
            for line in f:
                if needle in line:
                    return True
    except OSError:
        pass
    return False


def select_files(anchor_line):
    files = all_transcripts()
    # the anchor appears JSON-escaped inside transcript lines exactly as written (no characters to escape)
    sessions = set()
    for p in files:
        if contains(p, anchor_line):
            stem = os.path.splitext(os.path.basename(p))[0]
            parts = os.path.normpath(p).split(os.sep)
            # a main session file is <project>/<session>.jsonl; a subagent file lives below <project>/<session>/
            if os.path.dirname(p) in (os.path.join(PROJECTS_ROOT, parts[-2]),):
                sessions.add(stem)
            else:
                for comp in parts:
                    if re.fullmatch(r"[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}", comp):
                        sessions.add(comp)
                        break
    chosen = []
    for p in files:
        parts = os.path.normpath(p).split(os.sep)
        stem = os.path.splitext(parts[-1])[0]
        if stem in sessions or any(c in sessions for c in parts[:-1]):
            chosen.append(p)
    return sorted(set(chosen)), sorted(sessions)


def main():
    ap = argparse.ArgumentParser(description="Nocturne project metrics (Part 15.3)")
    ap.add_argument("--pricing", default=os.path.join(HERE, "pricing.json"))
    ap.add_argument("--json")
    ap.add_argument("--md")
    args = ap.parse_args()
    if hasattr(sys.stdout, "reconfigure"):
        sys.stdout.reconfigure(encoding="utf-8")

    anchor_line, anchor = read_anchor()
    files, sessions = select_files(anchor_line)
    if not files:
        sys.exit("no transcript contains the anchor line")

    seen = {}
    stamps = []
    models = {}
    n_main = n_sub = 0
    for p in files:
        is_sub = any(seg in ("subagents", "workflows") for seg in os.path.normpath(p).split(os.sep))
        n_sub += is_sub
        n_main += not is_sub
        with open(p, "r", encoding="utf-8", errors="replace") as f:
            for line in f:
                try:
                    obj = json.loads(line)
                except Exception:
                    continue
                ts = parse_ts(obj.get("timestamp"))
                if ts is None or ts < anchor:
                    continue
                stamps.append(ts)
                msg = obj.get("message")
                if obj.get("type") != "assistant" or not isinstance(msg, dict):
                    continue
                usage = msg.get("usage")
                if not isinstance(usage, dict):
                    continue
                key = msg.get("id") or obj.get("requestId") or (p + ":" + str(obj.get("uuid")))
                cc = usage.get("cache_creation") or {}
                rec = {
                    "input": int(usage.get("input_tokens") or 0),
                    "cache_read": int(usage.get("cache_read_input_tokens") or 0),
                    "cache_write_total": int(usage.get("cache_creation_input_tokens") or 0),
                    "cache_write_5m": int(cc.get("ephemeral_5m_input_tokens") or 0),
                    "cache_write_1h": int(cc.get("ephemeral_1h_input_tokens") or 0),
                    "output": int(usage.get("output_tokens") or 0),
                    "model": msg.get("model") or "?",
                }
                old = seen.get(key)
                # a streamed message is written several times; keep the most complete record
                if old is None or rec["output"] >= old["output"]:
                    seen[key] = rec

    tot = {k: 0 for k in ("input", "cache_read", "cache_write_5m", "cache_write_1h", "output")}
    unsplit = 0
    for r in seen.values():
        models[r["model"]] = models.get(r["model"], 0) + 1
        tot["input"] += r["input"]
        tot["cache_read"] += r["cache_read"]
        tot["output"] += r["output"]
        split = r["cache_write_5m"] + r["cache_write_1h"]
        if split == 0 and r["cache_write_total"] > 0:
            unsplit += r["cache_write_total"]          # unsplit writes are priced as 5-minute writes
            tot["cache_write_5m"] += r["cache_write_total"]
        else:
            tot["cache_write_5m"] += r["cache_write_5m"]
            tot["cache_write_1h"] += r["cache_write_1h"]
    total_tokens = sum(tot.values())

    stamps.sort()
    last = stamps[-1] if stamps else anchor
    wall = (last - anchor).total_seconds()
    gaps = []
    prev = anchor
    for t in stamps:
        d = (t - prev).total_seconds()
        if d > GAP_SECONDS:
            gaps.append((prev, t, d))
        prev = t
    active = wall - sum(g[2] for g in gaps)

    with open(args.pricing, "r", encoding="utf-8") as f:
        pricing = json.load(f)
    pr = pricing["prices_per_mtok"]
    cost = {
        "input": tot["input"] * pr["input"] / 1e6,
        "cache_read": tot["cache_read"] * pr["cache_read"] / 1e6,
        "cache_write_5m": tot["cache_write_5m"] * pr["cache_write_5m"] / 1e6,
        "cache_write_1h": tot["cache_write_1h"] * pr["cache_write_1h"] / 1e6,
        "output": tot["output"] * pr["output"] / 1e6,
    }
    total_cost = sum(cost.values())

    result = {
        "anchor": anchor.isoformat(), "last_record": last.isoformat(),
        "sessions": sessions, "files_main": n_main, "files_subagent_or_workflow": n_sub,
        "assistant_messages_dedup": len(seen), "models": models,
        "tokens": tot, "tokens_total": total_tokens, "cache_write_unsplit_priced_as_5m": unsplit,
        "wall_clock_s": wall, "wall_clock": hms(wall), "active_s": active, "active": hms(active),
        "gaps_over_10min": [{"from": a.isoformat(), "to": b.isoformat(), "length": hms(d)} for a, b, d in gaps],
        "pricing": {"model": pricing.get("model"), "source": pricing.get("source_url"), "accessed": pricing.get("accessed"), "per_mtok": pr},
        "cost_usd": {k: round(v, 2) for k, v in cost.items()}, "cost_total_usd": round(total_cost, 2),
    }

    rows = [
        ("Wall-clock time", hms(wall), f"measured: anchor {anchor.isoformat()} to the last included record {last.isoformat()}"),
        ("Active time", hms(active), f"measured: wall clock minus {len(gaps)} gap(s) longer than 10 min without any transcript activity"),
        ("Uncached input tokens", f"{tot['input']:,}", "measured: sum of usage.input_tokens, deduplicated by message id"),
        ("Cache read tokens", f"{tot['cache_read']:,}", "measured: usage.cache_read_input_tokens"),
        ("Cache write tokens (5 min)", f"{tot['cache_write_5m']:,}", "measured: usage.cache_creation.ephemeral_5m_input_tokens"),
        ("Cache write tokens (1 h)", f"{tot['cache_write_1h']:,}", "measured: usage.cache_creation.ephemeral_1h_input_tokens"),
        ("Output tokens (incl. thinking)", f"{tot['output']:,}", "measured: usage.output_tokens"),
        ("Total tokens", f"{total_tokens:,}", f"measured: {len(seen):,} assistant messages in {n_main} session file(s) and {n_sub} subagent/workflow file(s)"),
        ("API-equivalent cost (USD)", f"${total_cost:,.2f}",
         "estimated: " + " + ".join(f"{k} {tot[k]:,} x ${pr[k]}/MTok" for k in ("input", "cache_read", "cache_write_5m", "cache_write_1h", "output"))
         + f"; prices for {pricing.get('model')} from {pricing.get('source_url')} (accessed {pricing.get('accessed')})"),
    ]
    md = ["| Metric | Value | How obtained |", "|---|---|---|"] + [f"| {a} | {b} | {c} |" for a, b, c in rows]
    md.append("")
    md.append("Not counted: the tokens of the final measurement turn after this script ran and of the final chat message.")
    if gaps:
        md.append("")
        md.append("Gaps longer than 10 minutes: " + "; ".join(f"{a:%Y-%m-%d %H:%M} to " + (f"{b:%H:%M}" if b.date() == a.date() else f"{b:%Y-%m-%d %H:%M}") + f" ({hms(d)})" for a, b, d in gaps))
    text = "\n".join(md)
    print(text)
    print()
    print("models:", models, "| sessions:", sessions)
    if args.json:
        with open(args.json, "w", encoding="utf-8") as f:
            json.dump(result, f, indent=1)
    if args.md:
        with open(args.md, "w", encoding="utf-8") as f:
            f.write(text + "\n")


if __name__ == "__main__":
    main()
