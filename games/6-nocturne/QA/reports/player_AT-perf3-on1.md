# player report AT-perf3-on1 (PASS)

log `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\abyss\Logs\player-AT-perf3-on1.log`

| Gate | Result | Value |
|---|---|---|
| audio overruns after the intro = 0 | PASS | 0 (callback window max 13.24 ms, 1 lines) |

AUTOTEST lines:

- `[NOC] AUTOTEST start mode=cinematic_from560_until640 speed=2.50 expected trigger=375.05 stop=377.45 +- 2.0 s virtual from s=560.0 until s=640.0`
- `[NOC] AUTOTEST FAIL mode=cinematic_from560_until640 trigger=-1.000s stop=32.250s (expected 375.05 / 377.45) ending=n/as beats=1 errors=0 failed: gcAllocPerFrame (mode mono-heap, steady frames 1549, mean 52.9 B/frame (engine input box 32, excess 20.9), worst 12288 B, frames allocating 16, total 81920 B, GC collections 0 over 13.7 s (excess over the engine constant <= 1.0 B/frame + 400 B/s = 4.53 B/frame)) report=W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\.worktrees\abyss\QA\reports\autotest_cinematic_from560_until640.json`
