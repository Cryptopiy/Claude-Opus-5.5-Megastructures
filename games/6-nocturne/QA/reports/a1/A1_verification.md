# A1 verification run (Part 11.11), player Builds/A1 (a1_build_2 for parity / statics / perf / render-audio, a1_build_3 for the rest)

| System | Command (run-player -Exe Builds\A1\Nocturne.exe) | Result |
|---|---|---|
| Parity window vs RT | `-capture <dir> -capture-set parity -freeze-clock` | PASS: mean 0.000, max 0 levels at 1920x1080 (limit 1/255 mean) |
| Statics (14.10) | `-sequence statics -freeze-clock -rain 0` | PASS 7 x 120 frames: max per-pixel change 0.0 (beat 7 0.072) <= 2 levels |
| Boundaries (14.10) | `-sequence boundaries` | PASS 9 windows / 6 290 frames: max luma step 0.75 level (<= 1.2), activation excess <= 0.007 (<= 0.5), 0 spikes. First run FAILED (graybox posts, 3.35 levels) and was fixed |
| Perf 1080p | `-cinematic -perf-log a1_perf_1080.csv -quit-after 150` | avg 1 760 fps, 1 % low 632, 0.1 % low 294; 1 frame 36.6 ms (CPU 0.55 / GPU 1.3 ms: machine stall); GC +125 B/frame (open) |
| Perf 4K | `... -perf-rt 3840x2160 -quit-after 90` | avg 738 fps, 1 % low 536, max 15.6 ms, GPU 1.19 / 3.91 ms |
| VRAM (nvidia-smi + per-process counter) | Tools/vram-sample.ps1 | 1080p: player 233 MiB (total +438 MiB); 4K: player 1 110 MiB (total +1 128 MiB); limits 4 / 6 GB |
| Audio offline render | `-render-audio <wav> -cinematic` | peak -26.5 dBFS, DC 0, step 0.0375, 600 s; beat RMS -51.3..-52.7 dBFS vs targets -28.9..-34.9 (score skeleton: FAIL until the 14.3 layers exist) |
| Audio dump vs offline | `-audio-dump <wav> -quit-after 30` | PASS: -61.46 vs -61.75 dBFS, 0.29 dB (<= 1), 693 callbacks, 48 kHz stereo |
| Smoke | `-autotest -brisk -autotest-until 400` | PASS: s 400 at 160.10 s, coverage 0 / 48 204 rays |
| Autotest 2.5 m/s cinematic | `-autotest -cinematic` | PASS: trigger 599.050, stop 601.450, ending 17.00 s; crossings = B/2.5 + 0.25 exactly; roll 0.0000 deg (<= 0.3) |
| Autotest 1.5 m/s | `-autotest` | PASS: trigger 998.100, stop 1 002.100, ending 17.00 s; crossings 80.100 / 266.767 / 373.433 / 586.767 / 720.100 / 840.100; roll 0.0000 (<= 0.01) |
| Wall bot 2.5 m/s | `-wallbot -brisk` | PASS both edges: 0 lane violations, progress never dropped, lowest feet y 0.000, lane reach 28 probes error 0.0 mm |
| Wall bot 1.5 m/s | `-wallbot` | PASS both edges: same, lane reach 0.0 mm, coverage 0 misses |
| Collider ray grid | inside every runner (PathGuide.CoverageCheck, per-s face distance + 1 cm, no face closer than h + r + skin - 1 mm) | 0 misses of 48 204 rays |
| PlayMode input tests | unity-queue `-runTests -testPlatform PlayMode` | 2 passed, 2 skipped (no devices / no focus in batch mode); the built player logs keyboard=yes mouse=yes |
| EditMode tests | unity-queue `-runTests -testPlatform EditMode` | 161 / 161 |
| Tools/qa selftest | `python -I Tools/qa/selftest.py` | 90 / 90 assertions |
