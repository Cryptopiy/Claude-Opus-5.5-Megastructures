| Metric | Value | How obtained |
|---|---|---|
| Run anchor | 2026-11-13T19:22:09.463613+01:00 | measured: STATE.md line 1 (or --anchor); only records at or after it count |
| Sessions | W--YouTube-Megastructures-4-Megastructures-4-Megastructures/<SESSION_ID> (cwd = project root after the anchor); W--YouTube-Megastructures-4-Megastructures-4-Megastructures/<SESSION_ID> (anchor write (PowerShell at 2026-11-13T18:22:04.846Z)) | measured: sessions with an anchor write into this project's STATE.md or a cwd inside the project after the anchor; subagent and workflow transcripts of those sessions included |
| Sessions rejected (anchor text only) | 1 | measured: transcripts that only quote the anchor line (e.g. a tool_result that read STATE.md); not counted |
| Transcript folders | 1 (W--YouTube-Megastructures-4-Megastructures-4-Megastructures) | measured; 'W--YouTube-Megastructures' excluded by exact name |
| Transcript files (main / subagent / workflow) | 2 / 2 / 220 | measured: *.jsonl of the accepted sessions |
| Assistant messages (deduplicated) | 11,006 | measured: unique message.id with usage at/after the anchor (20,586 streamed records before dedup; 11,015 api-request records; 25 synthetic records ignored) |
| Models | claude-fable-5-2 (11,006) | measured: message.model |
| Uncached input tokens | 22,420 | measured: sum usage.input_tokens |
| Cache read tokens | 4,006,188,179 | measured: sum usage.cache_read_input_tokens |
| Cache write tokens (5 min / 1 h) | 98,055,658 (92,072,731 / 5,982,927) | measured: usage.cache_creation_input_tokens and its ephemeral split |
| Output tokens (incl. thinking) | 14,652,085 | measured: sum usage.output_tokens |
| Total tokens | 4,118,918,342 | measured: sum of the four categories |
| Excluded: before the anchor | 39,049 records, 8,266 messages, 2,628,644,747 tokens | measured: records of the accepted sessions older than the anchor (the earlier game of a continued chat); not counted |
| Wall-clock time | 168:35:39 | measured: from 2026-11-13T18:22:09.463613+00:00 (anchor) to the last record 2026-11-20T18:57:48.193000+00:00 |
| Idle gaps > 10 min | 13 gaps, 140:35:48 | measured: consecutive timestamps (anchor included) further apart than 600 s |
| Active time | 27:59:51 | derived: wall-clock minus all gaps > 10 min |
| Cost: uncached input | $0.09 | estimated: tokens x $4.0/MTok |
| Cost: cache read | $801.24 | estimated: tokens x $0.2/MTok |
| Cost: cache write 5 min | $460.36 | estimated: tokens x $5.0/MTok |
| Cost: cache write 1 h | $47.86 | estimated: tokens x $8.0/MTok |
| Cost: output | $293.04 | estimated: tokens x $20.0/MTok |
| Total API-equivalent cost | $1,602.60 | estimated: sum of categories; prices from https://platform.claude.com/docs/en/about-claude/pricing accessed 2026-11-20 for claude-fable-5-2 |

Gaps longer than 10 minutes (UTC):
- 2026-11-13T19:17:33.367000+00:00 -> 2026-11-13T19:28:04.941000+00:00 (0:10:32)
- 2026-11-13T23:04:30.537000+00:00 -> 2026-11-13T23:14:38.144000+00:00 (0:10:08)
- 2026-11-14T09:12:03.657000+00:00 -> 2026-11-14T11:19:20.685000+00:00 (2:07:17)
- 2026-11-14T12:29:51.152000+00:00 -> 2026-11-15T20:09:58.315000+00:00 (31:40:07)
- 2026-11-15T20:19:21.985000+00:00 -> 2026-11-19T15:08:16.994000+00:00 (90:48:55)
- 2026-11-19T15:14:54.782000+00:00 -> 2026-11-19T15:31:36.550000+00:00 (0:16:42)
- 2026-11-19T19:03:34.711000+00:00 -> 2026-11-20T07:01:53.782000+00:00 (11:58:19)
- 2026-11-20T07:15:34.026000+00:00 -> 2026-11-20T07:44:55.326000+00:00 (0:29:21)
- 2026-11-20T07:45:50.051000+00:00 -> 2026-11-20T09:27:58.157000+00:00 (1:42:08)
- 2026-11-20T09:30:00.927000+00:00 -> 2026-11-20T09:43:42.547000+00:00 (0:13:42)
- 2026-11-20T09:47:08.009000+00:00 -> 2026-11-20T10:10:37.845000+00:00 (0:23:30)
- 2026-11-20T13:09:23.726000+00:00 -> 2026-11-20T13:21:37.823000+00:00 (0:12:14)
- 2026-11-20T18:20:34.090000+00:00 -> 2026-11-20T18:43:27.717000+00:00 (0:22:54)

Rejected sessions (anchor text only):
- G--YouTube/<SESSION_ID>: contains the anchor line only (no anchor write, no project cwd)
