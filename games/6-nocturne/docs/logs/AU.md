# AU - Phase 4 wave A: the score and the soundscape (14.3, 14.6 windows, 14.7 H8, 14.10 audio rows)

Owner of: Assets/_Game/Scripts/Runtime/Audio/** (and new files there), Assets/_Game/Scripts/Tests/EditMode/AudioTests.cs and new
audio test files, Tools/audio/**, docs/AUDIO.md, docs/logs/AU.md, QA/critique-AU.md, QA/audio-trims.json (named by 14.3),
QA/reports/AU-*.xml and QA/reports/audio_*.json/md of my own renders.
Requests to AU: write "REQUEST AU: <file>: <exact change>" in your own log; answers appear here as "DONE AU <ID> <request>".

## Log
- 08:25 start (fresh: no earlier AU entries, no AU files in the tree). HEAD 01111e9. Read MASTER_PROMPT 14.0-14.4, 14.6, 14.7 H7/H8,
  14.10, 14.5 audio lines (tram T_A s 230, trains T1-T7, steam s 24/58/86/104, vending z 62-67), DEVELOPMENT D-011 / D-031..D-033,
  BUILDER_BRIEF, STATE, docs/research/R5_pad_rain_audio.md (the design, compiled code blocks), the Phase-2 audio skeleton
  (NocDsp / NocMixer / NocSynth / OfflineAudioRenderer / WavWriter / AudioDirector, AudioTests 9 tests), Thread's FootstepBank
  (read-only, 3rd Megastructures). External users of my API (keep stable): QA/AudioRenderRunner (OfflineRenderSettings,
  RmsSink, MeterSink, OfflineAudioRenderer.Render / Duration / Channels, WavStreamWriter), AudioDirector via SceneBuilder strings
  (Megastructure.NocSynth, Megastructure.AudioDirector), IAudioHooks (NocServices), PlayerController footsteps.
- Plan: R5's architecture (one DSP for player and offline, control at absolute 32-sample indices, counter-based hashes, snapshot
  extrapolation), PadVoice / PadEngine (H8), the 14.3 layers, the two FDNs, the TP-aware limiter; a .NET 10 console harness in
  Tools/audio/harness that compiles the runtime sources (like fastcheck) and renders the whole walk at 2.5 and 1.5 m/s in
  seconds, so the mix is tuned without the Unity queue; numpy analysis in Tools/audio/analyze.py.
- 10:10 RESTART (second start of wave A). The first start stopped at 09:56 mid-calibration with nothing committed and the log
  above as its only entries. In the tree: PadVoice, NocDspBlocks, NocturneScore (trims from run walk25_r03), RainBed, DripField,
  CityHum, TrafficWash, TramPass, TubeTrain, KoiTone, LeafRain, Footsteps, BeatBeds, AudioAnalysis (new), NocDsp / NocMixer /
  OfflineAudioRenderer / AudioDirector (rewritten), Tools/audio/harness (+ calibrate.py). fastcheck all OK (runtime 196 / editor
  260). Committing the tree as is (checkpoint), then: build the harness, re-measure, finish tests, AUDIO.md.
- 10:20 harness rebuilt (dotnet 10; build output in %TEMP%\nocturne_audio_harness). Whole-walk renders of the tree as left by the
  first start (run r04):
  - walk25_r04 (600 s audio in 36 s): per-beat median - Sum: -0.01 +0.13 -0.23 +0.27 +0.13 +0.00 -0.01 dB (16-42 windows); sample
    peak -4.93 dBFS, true peak -4.91 dBTP, DC 0.000000, max step 0.147 (s 760, a train pass), 0 non-finite. Chord changes: max
    sample step 0.052-0.132, max 20 ms envelope step 0.012-0.037 (C4->C5, trains T4/T6 inside the window).
  - walk15_r04 (1 000 s in 59 s): -0.01 -0.13 +0.23 -0.27 -0.11 +0.01 +0.01 dB; true peak -5.49 dBTP; max step 0.138; envelope
    steps 0.013-0.037. S-windows (logged, not gated) S1 -36.8, S2 -33.1, S3 -32.3, S4 -32.9, S6 -33.5, S7 -29.4 (K -33.0), S8 -30.3.
  - stems vs the 14.3 table within 1 dB everywhere except: beat-7 X -67 (table -60: the lamp's 100 Hz buzz is a point source at
    the goal window, -62 in S8; it rises to -60 at the stop) and beat-7 K -38.5 outside S7 (the distance law; -33.0 in S7, the
    pad +1 dB outside S7 restores the row's power sum, R5 6 (b)).
- 10:35 new: AudioGates (AudioAnalysis.cs: the level-gate stretches, pad-solo chord changes, player-path renders driven by
  60 fps snapshots, ClickMeter = max |second difference| inside / outside a window, PiecewiseWalkScript), OfflineRenderSettings
  .SoloMask, harness commands gates / alloc / probe / --solo / --nosteps. Harness numbers of the EditMode gates:
  level stretches (24 s per beat) at 2.5: -0.20 -0.05 -0.18 +0.64 -0.21 -0.03 -0.36 dB; at 1.5: -0.35 -0.20 -0.18 -0.23 -0.13
  -0.06 -0.17 dB; TP <= -6.5 dBTP. Pad solo through all 10 changes (both speeds): 20 ms envelope step <= 0.016, sample step
  <= 0.0074, second difference inside / outside 0.5-2.2 (the beat-7 cutoff rise at C7->C8 at 1.5 m/s is the 2.2). Player path vs
  offline: 0.000 dB at 2.5 and 1.5 m/s. Stand / walk 2.5 / walk back 1.5 / 10 m/s across C1->C2: max step 0.079, envelope 0.017.
  Block-size invariance: max |diff| 0 for 256 / 441 / 37,2048,512,1,333. Allocations of NocMixer.Process after warm-up: 0 bytes
  (offline and player paths, 4 000 callbacks each).
- Finding: the RELATIVE 20 ms envelope step of the pad solo is 0.3-1.1 (not a click: the 20 ms RMS of a low chord does not
  average the cross terms of its partials, e.g. 82.4 + 123.5 Hz beat at 41 Hz = 24 ms). So 14.7 H8 "step < 0.05 in 20 ms" is read
  as the absolute step of the 20 ms RMS envelope (full scale 1), and the click test adds two structural checks: (1) PadEngine
  per control block: a bank changes its note only while its gain < 1e-4, every gain moves by at most one 50 ms one-pole step
  (0.0132), the steady chord is equal power (sum of g^2 = voice count); (2) the pad's max second difference inside the change
  window <= 3 x outside.
- New tests: Tests/EditMode/AudioScoreTests.cs (19 tests), QA/audio-trims.json (the trims, checked by a test against the code).
- 11:18 t01 (Unity EditMode, AudioTests + AudioScoreTests, 148 s): 29/30 passed. Mono timings: level gates 23 s per speed, pad
  sweep 36 s, pad changes 38 s. The one failure was my own threshold (Reverb smoothness: 0.05 s per 0.5 m, but the 14.6 window
  0.9 -> 2.8 s over 30 m peaks at 0.059 per 0.5 m): the test now bounds the analytic smootherstep slope (0.065 s per 0.5 m, 0.0065
  s per 0.05 m, so a discontinuity cannot hide). The log's TypeInitializationException in NocDebugViewFeature is FX's (batchmode
  renderer data), not an audio test.
- 11:05 Tools/qa/audio_analysis.py (independent Python) on walk25_r05.wav (with the ending, 615.8 s): PASS, per-beat medians
  identical to the harness (-34.91 -30.27 -31.53 -29.33 -33.17 -30.10 -28.91), peak -4.93 dBFS, max step 0.147, DC 0, BS.1770
  integrated loudness -26.8 LUFS. Report QA/reports/audio_AU-walk25-r05.md/json.
- 11:20 Tools/audio/analyze.py (new; numpy): per beat octave bands, centroid, stereo correlation, hum / koi partials, onsets and
  the flux period, chord chroma, spectrogram PNG. walk25_r05: centroid 1970 / 999 / 854 / 711 / 2419 / 1192 / 478 Hz (alley and
  garden bright with drips and leaves, plaza dark with the koi drone); L/R correlation 0.05 (garden) to 0.54 (plaza); the
  flux period 0.341 s in beats 1, 2, 3, 7 = the footstep cadence at 2.5 m/s (0.85 m / 2.5); beat 4 0.245 s (bogie thumps of the
  passes). 100 Hz hum +11..+26 dB over its neighbourhood in beats 1-3, 5-7; koi 41.2 / 55 / 123.6 Hz +10 / +12 / +13 dB in beat 7.
  Chord chroma share on the chord's pitch classes, pad stem: 0.79 0.95 0.81 0.89 0.90 0.79 0.84 0.91 0.90 0.82 (C10 0.40 inside
  its own window); mix 0.48-0.85. The off-chord classes are the 5th harmonics of the low saws (C# of A2, F# of D2).
  Spectrogram: beat 4 has seven broadband train passes (p90 of the 3 s windows -21.3 against the median -29.3, max 1 s change
  6.4 dB): the loudest events of the walk, peaks -4.9 dBFS, by design (38 m/s trains 12 m away in glass tubes).
- 11:40 walk15_r05 (with the ending, 1 015 s): audio_analysis.py PASS, medians -34.91 -30.53 -31.07 -29.87 -33.41 -30.09 -28.97,
  peak -5.52 dBFS, step 0.138, -27.3 LUFS (QA/reports/audio_AU-walk15-r05.md). DSP cost (harness timing, beat 4 with trains,
  wind, tube hum): 5.7 % of one core on .NET.
- docs/AUDIO.md written (the score, architecture, layers, pad, stems, rendering, calibration, measured numbers, decisions).
  QA/critique-AU.md written (AU-01..AU-10).
- 11:35 lookdev_AU build started in the queue (for the in-game -audio-dump check; behind lookdev_BV 682 s and lookdev_FX 381 s).
- 2026-11-19 17:55 THIRD START (run wf_8e439303-949). The second start's AU work is all committed (67f0762..c692b3a); the
  re-applied patch 16b holds no AU files (git status: no AU paths modified). The lookdev_AU build of 11:35 was cut by the pause
  (no Builds/lookdev_AU). Left: AU-10 (the in-game -audio-dump check), the D-034 read, the full EditMode suite, the report.
  GPU care (user, 2026-11-19): AU needs the GPU for ONE short player run only (standing still, 1080p, 60 fps cap, about 30 s,
  no capture); everything else stays offline (harness on the CPU).
- 18:10 fastcheck all OK (runtime 198 / editor 263). lookdev_AU build queued (unity-queue, NOC_BUILD_DIR Builds\lookdev_AU,
  BuildWindows only; no capture), behind HT-t03.
- 18:15 read docs/DREAD.md (D-034, wave B). AU owns section 1 (echo across the Gulf, sub swells, structural groans, silence
  drops) and every sound cue of sections 2-6. Nothing in the wave-A graph blocks it; the hooks (where each item plugs in, the
  delay memory, the headroom and gate arithmetic) are written down in docs/AUDIO.md section 11 instead of as dead code, so the
  verified DSP (gates, invariance, 0 bytes per Process) stays exactly what the build checks.
- 18:21 lookdev_AU build: SUCCESS, 0 errors, 2 warnings, 463 MB, 337.6 s (the tree of 18:15 with HEAD 036b86d's audio sources).
- 18:31 AU-dump01 (run-player, Builds\lookdev_AU, 960x540 window to spare the GPU, vSync on, no capture; 64.6 s wall incl. 5.3 s
  load): -progress 0.75 -nofade -audio-dump QA\reports\AU-dump01.wav -audio-dump-seconds 10 -quit-after 40, standing at s 1125
  (beat 6). [NOC] audio dump vs offline: dump -30.12 dBFS, offline -30.23 dBFS, difference +0.10 dB (<= 1.0), 1 057 callbacks
  -> PASS (QA/reports/audio_dump.json). 0 error lines. Python check of the two WAVs: 1 s windows -30.66..-29.66 (dump) and
  -30.49..-29.65 (offline), no ramp; peak -16.8 / -17.2 dBFS; max sample step 0.064 / 0.060; max second difference 0.089 /
  0.089; longest run of exact zeros 1 sample (no dropout or underrun); L/R correlation 0.43 / 0.46; octave shares (31 Hz-16 kHz)
  within 0.8 dB of each other. The beat-6 Sum column is -30.1 dBFS: the real output sits on the 14.3 target. AU-10 FIXED.
  The player's alloc line (mean 4 421 B/frame, worst 15.1 MB, 18 allocating frames of 4 375) is the one-off -audio-dump
  capture buffer (10 s x 48 kHz x 8 ch x 4 B) and its offline render, not a per-frame allocation (NocMixer.Process: 0 B,
  AudioScoreTests). The WAVs QA/reports/AU-dump01*.wav (1.9 MB each) stay local, untracked.
- 18:49-19:06 AU-t02, the full EditMode suite (unity-queue, no filter; tree of 18:47 with every owner's uncommitted work):
  410 tests, 408 passed, 1 failed, 1 skipped (AbyssTests.KS10_CompositionSearch, explicit). All 30 audio tests passed
  (AudioTests 11 + AudioScoreTests 19). The one failure is not audio: Q1IntegrationTests.A_EveryLaneMetreIsFlooredAtYZero,
  "in-lane 1 holes": HOLE s 550.50 lat 2.43 y NaN (1 ray) and MARGIN-HOLE s 544.50 lat 2.67, beat 3 (AB's Edge / tongue-pier
  work in the tree; NOTE AB: one 0.25 m probe ray finds no floor at s 550.5, lat 2.43). Report QA/reports/AU-t02.xml.
- GPU use of AU in this start: one player run of 64.6 s (960x540 window, vSync, no capture, no gpu-guard route needed). No
  4K, perf, soak, sequence or capture run. Every other check ran on the CPU (Unity build, EditMode, harness, Python).
- Open, not AU-fixable alone: AU-06 (train passes dominate beat 4; needs a human listen), AU-09 (-26.8 LUFS; lead decision on a
  make-up gain at the edit or a table offset). Wave B (D-034 section 1) is laid out in docs/AUDIO.md section 11.
- AU DONE 2026-11-19 19:15

## Fifth start (2026-11-20 20:35): the sound of the D-034 dread layer (docs/DREAD.md section 1 and the cues of 2-6)
- 20:35 read: this log, my commits (67f0762..1e9f3e6; git show --stat 2fe0da9 holds no AU file), QA/critique-AU.md,
  docs/DREAD.md, docs/AUDIO.md section 11 (the wave-B hooks), the key-shot table (KS04 s 250, KS09 520, KS10 548, KS13 718,
  KS17 1296), BV's TramSchedule (blvd copy, read-only), TR's new BridgeSway (this copy, read-only). No "REQUEST AU" in any log.
  No DR file exists in any copy yet (Beats/Dread): the hull, the panel and the shafts are positioned by my own DreadScore,
  written as requests to DR below. Live note: Logs/AU_progress.md.
- New files: Runtime/Audio/DreadScore.cs (the dread score), DreadVoices.cs (HornVoice, StrikeVoice, GroanVoice, CreakVoice,
  ChimeVoice, RumbleVoice, SubSwell, HullDrone, MachineHum, BridgeGust, EchoBus, DreadLayer), DreadAnalysis.cs (DreadProbe),
  Tests/EditMode/AudioDreadTests.cs (12 tests), Tools/audio/dread_analysis.py, Tools/audio/harness/NocAudioHarness.csproj (the
  csproj was only in the main project, untracked: *.csproj is gitignored; build output %TEMP%\nocturne_audio_harness_waveb).
  Edited: NocMixer.cs (stem E; the drops on R, D, H, T and the X beds), NocturneScore.cs (E = 9, StemCount 10, LayerNames),
  harness Program.cs (solo letter E; commands dread [--cues], dreadcost, stems).
- Design (docs/AUDIO.md section 11, rewritten):
  - stem E = DreadLayer, inside NocMixer (no wiring outside my files): one-shots fired when s first crosses their s (re-armed
    12 m below, the vehicle rule; a jump marks the past ones done), started at an exact sample (trigger + flight time), voice
    pools per kind (a busy pool skips, never steals: no click), every gain on the 50 ms ramps or smooth per-sample envelopes;
  - seconds of DREAD.md are metres at the reference speed 2.5 m/s (the video's cinematic walk): exact in the video, longer at
    1.5 m/s, held while standing;
  - echo: image sources off the face planes: A1 = 2 d_A / 343, B1 = 2 d_B / 343, AB2 = 1 200 m / 343 = 3.50 s; gain
    1 / (1 + d / 150) (AB2: d = 600 x 0.7 second-reflection loss); LP 6 kHz / (1 + L / 250); taps under 30 ms merge (no comb);
    the bus fades in / out over 40 m at each face. One mono float line of 2^18 samples (5.5 s, 1 MB); taps glide per block,
    a jump over 50 ms fades the tap out over 20 ms, moves it, fades back in;
  - the swells: 5 voices (F1, Eb1, F1, D1, G1: chord tones), partials 1-5 up to 200 Hz in three detuned voices, a brown
    pressure bed LP 55 Hz; rise 20 m (8 s), hold 2.5 m, decay 20 m (8 s); peaks -38 / -36 / -35 / -37 / -36 dBFS through the
    master's 20 Hz DC blocker;
  - the drops: -21 dB on R, D, H, T and the X beds from 5 m before KS09 / KS17 (2.0 s), full 1.5 m later (0.6 s), back over
    3.75 m (1.5 s) after the key shot; wind, pad, koi, footsteps and E stay;
  - hull: VehicleClock launch s 430, centre abeam z 195 at tau 52.5 s (s 561 at 2.5 m/s), +z at 18 m/s, x 515.3, belly
    y 430, 1 800 m: body stack 13.75 Hz x 1..12 as a line source (subtended angle), stern engines 27.5 Hz x 1..8 + exhaust band
    150 Hz + 0.55 Hz throb as a point source with the Doppler c / (c + v_r) (+5 % far ahead, 0 overhead); ends with beat 4;
  - BridgeGust follows TR's BridgeSway (mirrored in DreadScore.SwayAmplitude): deck creaks at the extremes of sin(2 pi 0.12 t),
    cable whistle with the heard wind gust (WindBed.Gust).
- 21:20-22:25 harness iterations (renders in %TEMP%\au_runs; WAVs deleted after analysis):
  - d01 (2.5): beat devs -0.01 +0.15 +0.24 +0.33 +0.18 +0.03 +0.02 dB, TP -4.81 dBTP; KS09 drop R -42 -> -63, H -45 -> -66.
  - harness "dread": echo impulse arrivals x 375: 0.9312 / 2.5674 / 3.4986 s; x 535: 1.8642 / 1.6344 / 3.4986 s; x 230:
    0.0857 / 3.4129 / 3.4986 s; every voice 0.00..-0.10 dB at gain 1 (normalisers set from the first run: horn 0.7408,
    strike 1.346, groan 4.584, creak 39.5, chime 1.033, rumble 1.961). DSP cost of E 2-6 % of a core (dreadcost).
  - groan and strike slots that collide with another cue move on in 8 m steps (Shifted); groans keep out of S7, end at s 1440.
  - d04 -> d05 (dread_analysis.py): horn -39 -> -41.5 and creak -40 -> -43 dBFS (they rose 0.6 / 2.3 dB over the pad in its
    500 Hz octave); strikes -37 -> -33, groans -39 -> -36 (creak grains x 1.45), rumbles -40 -> -37 (roll band 220 Hz), chime
    -46 -> -43 (they sat 9-27 dB under the rest in their best laptop third octave); swells partials 1-5 up to 200 Hz; hums
    -54 -> -48, gantries -52 -> -46.
  - d05 (final levels; mix / noE): 2.5 m/s beat devs -0.01 +0.16 +0.22 +0.38 +0.25 +0.04 +0.02 / -0.01 +0.13 -0.23 +0.27
    +0.13 +0.00 -0.17 dB; TP -4.84 / -4.91 dBTP; step 0.147; worst 20 ms envelope step 0.0427 at C4->C5 (noE 0.037).
    1.5 m/s: -0.01 -0.06 +0.53 -0.17 -0.10 +0.03 -0.06 / -0.01 -0.13 +0.22 -0.27 -0.11 +0.01 -0.18 dB; TP -5.50; step 0.138;
    C4->C5 0.0337. Tools/qa/audio_analysis.py PASS at both (QA/reports/audio_AU-walk25-d05.md, audio_AU-walk15-d05.md).
    harness gates (EditMode stretches): 2.5: -0.20 +0.19 +0.47 +0.81 -0.12 +0.00 -0.36; 1.5: -0.35 -0.20 +0.22 -0.02 -0.13
    +0.00 -0.17 dB; player path vs offline 0.000 dB.
  - dread_analysis d05 (QA/reports/audio_dread_AU-walk25-d05.json / -walk15-d05.json): AAC 128 kept 0.96-0.99 and Opus 128
    0.96-1.01 of E in every cue window; E under the pad in every pad octave (closest: creak -0.7, horn -1.9 dB); laptop best
    third octave: horn -4.0, creak -2.6, groans -4.1..-21.6, strikes -5.5..-15.6, swells -8.5..-24.5, hull -14.8 dB; the drops
    take the rest's 2-16 kHz down 10.3 (KS09) and 13.8 dB (KS17). Table in docs/AUDIO.md 11.
- 21:50-21:58 AU-t01 (Unity, AudioDreadTests|AudioScoreTests|AudioTests; result kept as QA/reports/AU-d01.xml, the wave-A
  AU-t01.xml restored from HEAD): 42 tests, 39 passed. All 30 wave-A audio tests passed. Failed (mine): the echo tap snap on a
  200 m jump (second difference 2.88e-4 > 2.49e-4: the head moved while the block's fade-in interpolation was above 0 -> fixed:
  move only after a whole silent block), the face-B gain check at x 790 (the 0.03-0.15 s merge ramp; the test now stops at
  x 770 and checks the merge at x 805), the swell decay monotonicity check (the loop's start value; fixed).
- NOTE TR (BridgeSway.cs): your gust envelope g(t) = 0.5 - 0.5 cos(2 pi t / 17) has a 17 s period; the heard wind gust and the
  picture's rain slant pulse |sin(2 pi t / 17)| every 8.5 s (WindBed.Gust, NocRainMath). The creaks follow your formula
  (mirrored in DreadScore.SwayAmplitude); if you change it, tell me the new line and I mirror it.
- REQUEST DR (Beats/Dread/**, not yet in any copy): the picture follows the audio's course. Hull: tau = real seconds since s first
  crossed DreadScore.HullLaunchS (430) (capture mode: Math.Max(0, (s - 430) / 2.5)); centre z = DreadScore.HullCentreZ(tau)
  (= 195 + 18 (tau - 52.5)), x DreadScore.HullX (515.3), belly DreadScore.HullBellyY (430), length DreadScore.HullLength (1800).
  Panel: tears at s DreadScore.PanelS (470) from (DreadScore.PanelX, PanelY, PanelZ) = (815.3, 150, 335). Cabins / gantries:
  DreadScore.ShaftPosition(i, sceneT) for i < DreadScore.ShaftCount (or tell me your positions and I move the hums).
- REQUEST AT (sheet lightning, DREAD 5.4): glows at s = DreadScore.LightningS = { 205, 345, 553, 790, 1140, 1420 } (one per 25 s
  at most at 2.5 m/s, none in beats 1 and 5; s 553 outlines the hull); the far rumble follows each by 3.0 s.
- REQUEST BV (the tram's dwell, DREAD 6): the doors close with the chime at s DreadScore.ChimeS (226.25 = 230 - 1.5 s x 2.5).
- 22:35 EchoBus: the taps' low-pass coefficient now glides (a step of the coefficient kinked the waveform at a jump) and is set
  while the tap is silent; harness echojump (the test's signal): walking 6.3e-5, the 200 m jump 7.9e-5, limit 2.5e-4. The test
  skips the first 4 s (the tone's own onset through the taps).
- 22:44-22:54 AU-t02 (Unity EditMode, AudioDreadTests|AudioScoreTests|AudioTests, QA/reports/AU-t02b.xml): 42/42 passed
  (AudioDreadTests 12, AudioScoreTests 19, AudioTests 11), 465 s.
- 23:02 the queue: TR-final (TR's full EditMode suite, about 17 min) took the copy's Unity queue before AU-b01; AU-b01 would
  have ended about 23:27 and AU-final after the deadline, and W: is at 13 GB free (under the 15 GB floor; others' build
  folders). So I cancelled AU-b01 and AU-final (no Unity call of mine is left in the queue) and run the smoke and the -audio-dump
  checks from Builds\TR, read-only: TR-b03 built this copy's tree at 22:54-23:01 (Logs/TR-b03.log: "[NOC] BuildWindows:
  result=Succeeded errors=0 warnings=4 size=463.5 MB"), after my last audio edit (22:37); its Megastructure.Runtime.dll
  (22:59:58) holds DreadLayer, EchoBus, SwayAmplitude, GroanQuietS0. The full-suite evidence is TR-final (the same tree).
- 23:08-23:12 player runs from Builds\TR (the TR-b03 build of this tree; read-only use; Logs/player-AU-*.log):
  - AU-dump03 at s 520 (KS09: the swell at its peak, the drop at full depth, the hull overhead), 960x540, standing:
    "[NOC] audio dump vs offline: dump -30.81 dBFS, offline -30.72 dBFS, difference -0.09 dB (<= 1.0), 1179 callbacks, s 520.0,
    10.0 s -> PASS" (QA/reports/audio_dump_AU-dump03.json). Python check of the two WAVs: 1 s windows -31.73..-29.91 /
    -31.28..-29.95 dBFS, peak -17.3 / -17.3 dBFS, max step 0.0073 / 0.0082, longest run of exact zeros 0 (no dropout).
  - AU-dump02 at s 1125 (the wave-A point): dump -30.14, offline -30.22 dBFS, difference +0.08 dB -> PASS
    (QA/reports/audio_dump_AU-dump02.json); windows -30.50..-29.79 / -30.48..-29.64, steps 0.064 / 0.060, no zero run.
  - AU-smoke (-autotest -brisk -autotest-until 560, 1920x1080): "[NOC] AUTOTEST PASS mode=brisk_until560 ... beats=4 errors=0"
    (QA/reports/autotest_brisk_until560.json): the walk through the chime, the groans, the horn, the creak, both swells and the
    KS09 drop in the built player at 2.5 m/s; alloc at quit mean 34.2 B/frame (engine input box 32), GC collections 0.
  - The dump WAVs (1.9 MB each) stay local, untracked; the per-dump alloc lines (15 MB worst) are the one-off -audio-dump
    capture buffer and its offline render, as in wave A.
- 23:13 full EditMode suite: TR-final (this copy's tree at 23:02, after my last edit; QA/reports/TR-final.xml, 643 s): 447 tests,
  439 passed, 7 failed, 1 skipped (AbyssTests.KS10_CompositionSearch, explicit). All 42 audio tests passed (AudioDreadTests 12,
  AudioScoreTests and AudioTests 30). The 7 failures are outside audio: AbyssTests.KS10_DropShare_AtLeast40_OnTheBuiltWorld
  (0.298 < 0.40; AB), Beat07HeartTests.PlazaInlay_SameTopFan_RingsSeamsFade (962 < 1000; HT),
  BoulevardStreetTests.LightBudget_AtMost28_SmoothFades (29 > 28) and Shops_Varied_NeverBrighterThanTheLitRoom (18 > 17; BV's
  tests, beat-2 lights), Q1IntegrationTests.A_EveryLaneMetreIsFlooredAtYZero (45 tops off y 0) and B_NoGeometryInsideTheReachable
  BodyOrTheCameraSphere (848; geometry of this copy's beat owners), WetFloorTests.WallWindows_MeanOne_Bounded_StreakVertically
  (-1.4e-11 >= 0, a rounding sign; FX). No AU-final.xml of my own: the queue was held (see 23:02).
- 23:14 fastcheck all OK (runtime 209, editor 278). Shared requests for the report: DR (hull, panel, shafts), AT (lightning s),
  BV (door chime s), lead (git add -f Tools/audio/harness/NocAudioHarness.csproj; decisions). Temp renders deleted (C: only).
- AU DONE 2026-11-20 23:15


---

## FIX WAVE (2026-11-21, panel p4_01, task file Logs/fix/AU.md; copy waveb at master caddb90)

Start 01:57. Read Logs/fix/AU.md with the lead's rulings (D-040: DreadScore reads DR's DreadCourse; C01 re-parks the hull so its
centre is abeam z 195 at about s 561; C50 the upload-loudness step in docs/AUDIO.md 10). No open "REQUEST AU" in any copy's logs
(grep of */docs/logs/*.md at 02:40).

Harness: Tools/audio/harness now compiles DR's DreadCourse.cs too (csproj property DreadCourseSrc, default
Assets/_Game/Beats/Dread/DreadCourse.cs; `-p:DreadCourseSrc=<file> -p:OutputPath=<dir>` previews another course). The analysis
runs used DR's C01 file from the abyss copy (read-only; ParkCentre -660, abeam tau 52.5 = s 561.25), the course the merge brings;
this copy's DreadCourse still has the old park (stern 1 700), so the tests are written for both courses.
Render script (scratchpad run_walk.sh): six parallel walks (mix, noE, E, pad, K, noK; --ending) + `dread --cues` +
Tools/audio/dread_analysis.py (new: K-weighted / laptop / phone drops of the rest and the mix, the koi in S7, meet distances, a gate
list). WAVs in %TEMP%\au_runs (C:), deleted after analysis.

Changes (all in my files):
- C07 drops (DreadScore.DropShape / DropGainAt, NocMixer): R D H T, the X beds AND the X passes -21 dB; the pad -15 dB applied after
  its own FDN (m_gPd: the tail goes too); the koi -21 dB; the wind -4 dB; footsteps and E stay. Same 0.6 s fall / 1.5 s return
  through the 50 ms ramps.
- C08 (DreadScore): HornS {440, 740} (horn 2 at x 555 mid-Gulf, 1.8 km south: F2 + C3 over C5 Dm11); Clear() rejects any strike /
  groan slot within MeetClearM = 18 m of a vehicle meet at 2.5 m/s (NocturneScore.MeetS: tram 272.0, T1 602.0, T5 624.7, T3 653.6,
  T4 699.3, T6 715.8, T2 759.1, T7 816.3; C08 asks 15 m, 18 keeps the strike after T2 out of the pass tail). Result: strike 760.8 ->
  784.8, groan 597.6 -> no clear slot in s 587-831 (the train stretch), so beat 4 has 8 groans in the walk instead of 10 (test
  updated: >= 8); strikes 489, 578, 673, 785, 858, 922.
- C09 small speakers: SubSwell's small-speaker layer (DreadScore.SwellUpperH / SwellUpperAmp / SwellUpperDb: chord-tone harmonics
  5-12 of each fundamental at 194-466 Hz, the strongest in the third octave of the pad's 250 Hz octave where the rest is weakest,
  a centre and a 0.35 %-detuned side voice, on the swell's envelope, the total peak kept at SwellPeakDb); HullDrone's harmonics
  9 / 12 / 8 x 27.5 Hz (B3 E4 A3) at the engines' Doppler on the whole hull's gain (HullUpperDb -3); KoiTone partials
  {1, 3, 4, 6, 8, 9, 12} x E1 at {0, -6, -10, -8, -10, -10, -9} dB with their own 0.13 Hz vibrato on the upper four (ADR request
  against the 14.3 partial list below).
- DreadCourse (D-040): HullLaunchS / HullSpeed / HullLength / HullBeam / HullX / HullBellyY from HullCourse constants;
  HullCentreZ(tau) = HullCourse.SternZ(tau) + Length / 2 (SternZ(tau) is the one function with the same meaning before and after
  DR's C01 API change: DR's new CentreZ takes tau, the old one a stern z); HullVelocity(tau) (the 10 s ramp) drives the Doppler;
  HullAbeamTau solved from the course (NaN on the old course). Panel from PanelCourse: s 470 at (815.3, 150, 800): the creak is heard
  2.38 s after the tear (was 1.66 s from z 335).
- C53: DreadScore.ChimeDuckShape / ChimeDuckGain: T and the X beds duck 9 dB from 0.3 s before ChimeS (226.25, BV's door close) to
  3.5 s after it (fall 0.6 s, return 1.5 s); ChimeDb -43 -> -40.
- C49: CityHum brown and deep chains through a 4th-order Butterworth high-pass at 32 Hz (two TPT SVF sections, Q 0.5412 / 1.3066),
  included in BrownNorm (unit RMS of the audible hum). 2nd order at 29 Hz left 11-28 % below 30 Hz (harness H solo), 4th order at
  30 Hz 5.5-15.0 % (beat 3 at the limit), 32 Hz 4.1-11.5 %.
- C51: the pad FDN's wet side narrowed to NocMixer.PadWetWidth 0.35 (power kept: sqrt(2 / (1 + w^2))); the chorus is unchanged
  (StereoChorus.Width added, the pad keeps 1.0: a 0.3 width left beat 5 at corr -0.07 / mono -3.3 dB). Cause: the FDN's steady
  response to the garden's held C#4 (277 Hz) came out anti-phase (beat 5 pad corr -0.62 at 200-300 Hz). The per-chord level shifts
  (+0.31 +0.92 +0.83 +0.61 +0.11 +0.61 -1.45 +0.76 -0.31 +1.48 dB) went back into PadChordTrimDb (now 0.19 -1.12 -1.73 -0.51 1.19
  0.19 0.65 -0.66 -1.24 -1.58 0.0; all within 2 dB) and QA/audio-trims.json (AU's own calibration file since 73d315a; the test
  AudioTrimsJson_MatchesTheScore reads it). Pad solo per chord after the trims: 0.00 dB from the f05 render.
- C52: NocSynth counts callbacks over 50 % and over 100 % of their buffer's period (SlowCallbacks, Overruns) and a window maximum;
  AudioDirector marks the intro baseline 10 s after Start and logs "[NOC] audio heartbeat t N s, s S: n callbacks, window max X ms,
  since the intro: slow K, overruns M (audio overruns after the intro = M)" every 30 s and once at quit ("audio summary").
  Allocation: a string per 30 s on the main thread only; the callback adds two integer compares.
- C50: the upload-loudness step written in docs/AUDIO.md 10 (README text for the lead in my report).
- Tests (AudioDreadTests): the drop test checks the pad (DropPadDb +-2), the wind (DropWindDb +-1.5) and that E rises; new
  SilenceDrops_TheRestFallsTenDecibels_C07 (everything but E, footsteps on, 4 s pre-roll: <= -10 dB at both drops),
  ChimeClearing_TrafficDucksAroundTheDoorChime_C53, SmallSpeakerLayer_SwellsCarryChordTonesAbove200Hz_C09 (chord tones of the voiced
  chord within 20 cents, 180-500 Hz, rendered share above 200 Hz > -14 dB), Koi_ReachesLaptopBands_C09 (share above 200 Hz > -11 dB),
  CityHum_KeepsItsPowerAboveThirtyHertz_C49 (< 15 % below 30 Hz at s 60 / 420 / 600 / 1 200); Cues: horns {440, 740}, every horn /
  strike / groan >= MeetClearM from every meet, the panel from PanelCourse, the s 553 outline checked when the course passes
  abeam (logged otherwise); Hull: the course equals HullCourse, 18 m/s after the ramp, the level at the course's closest pass.
- 02:11-03:06 harness iterations f01-f08 (2.5 m/s unless noted; WAVs on C: deleted after analysis):
  - f01 (first cut, C01 preview): drops -10.06 / -12.5 dB, swells laptop KS04 -8.1 KS10 -3.6, hull -8.1, koi -7.3 -> per-third
    analysis (scratchpad thirds.py): the rest in the swell windows is weakest in different thirds per chord, and the chord tones
    collide with the pad's partials, so the strongest upper harmonic goes into the pad octave's weakest third (KS04 / KS10 C4).
  - f02-f04: levels per swell; the hull's layer moved from the engines (a point source 700 m off at the pass) onto the whole hull
    gain; f04 64 / 64 gates. KS13 h9 and KS17 h5 / h10 dropped (E and B are not in those chords' voicings: the test checks the
    voiced pitch classes).
  - f05 / f06 (DR's real C01 file from the abyss copy; DR's new API: CentreZ(tau) -> DreadScore uses SternZ(tau) + 900, valid for
    both APIs): 1.5 m/s KS09 / KS10 rose 0.5 / 0.1 dB over the pad -> -8.5 dB; the koi over the pad at 1.5 m/s (+1.15 dB in the
    250 Hz octave) -> its new power moved to the 500 Hz octave (9 x and 12 x E1); KS04 at 1.5 m/s -3.2 -> 0 dB rel.
  - C51 trials (pad solo): chorus wet width 0.3: beat 5 corr -0.07, mono -3.31 (not the cause); FDN wet width 0.5: +0.22 / -2.16;
    0.35: +0.37 / -1.64 with chord shifts -1.45..+1.48 dB -> trims; re-render: 0.00 dB per chord.
  - C49 trials (H solo): HP 29 Hz 2nd order 11-28 % below 30 Hz; 30 Hz 4th order 5.5-15.0 %; 32 Hz 4th order 4.1-11.5 %.
  - f07: 62 / 62; chime best band -3.3 dB after the 9 dB duck. Then the passes 3 dB down (C08 option) -> f08.
  - f08 (final, QA/reports/audio_dread_AU-walk25-f08.* / -walk15-f08.*, audio_walk_AU-walk*-f08.txt, audio_AU-walk*-f08.md,
    audio_AU-f08-hum-pad.txt): gates 62 / 62 at both speeds; beat devs 2.5: -0.03 +0.18 +0.18 +0.47 +0.23 -0.02 -0.14, 1.5: +0.01
    -0.13 +0.61 -0.17 +0.08 +0.01 -0.02 dB; TP -7.62 / -8.05 dBTP; steps 0.144 / 0.134; C4->C5 20 ms step 0.0277 / 0.0257;
    -27.26 / -27.55 LUFS; drops rest -11.0 / -14.8 (K -11.2 / -15.1 LU, laptop -10.6 / -14.8); swells laptop best -1.5 +3.8 +0.8
    -0.6 +9.6, hull -2.5, koi laptop -5.5; under the pad outside the drops (worst -0.1 KS09 at 1.5); chime -3.3 dB at 504 Hz; horn
    740 -10.6, strike 784.8 -11.7 dB re rest; AAC / Opus kept 0.97-0.99 / 0.95-1.01 of E; loudest 1 s windows -22.8..-23.3 dBFS
    (+6.3 over the beat-4 median; were +8.5..+9.0); QA's audio_analysis.py PASS at both speeds.
  - harness alloc: 0 bytes over 4000 callbacks (offline and player paths); partition: max |diff| 0 for 256 / 441 /
    37,2048,512,1,333 frames.
- 02:51-02:57 AU-t11 (Unity EditMode AudioDreadTests|AudioScoreTests|AudioTests, QA/reports/AU-t11.xml): 47 / 47 passed, 329 s
  (this tree's old hull course: the hull test took its "never passes over" branch). The train trim (03:00) came after it: the final
  suite covers it.
- GLOBAL 03:00: the train passes are 3 dB quieter in beat 4 (TubeTrain PeakDb -24, whoosh -27, thump -33): sound only.
- 03:25 looked at two spectrograms of the final DSP (harness f08 build; QA/reports/audio_AU-f08-ks09-spec.png, -ks17-spec.png; mono
  mix, 8192-point frames, with the 100 ms level full band and above 200 Hz): at KS09 the rain's 1-8 kHz hiss vanishes over 0.6 s
  and returns over 1.5 s with no vertical click line; the band above 200 Hz falls about 7 dB with E included (the rest alone
  -10.6 dB) while the swell's G3 / Bb3 / Eb4 lines carry on; the footsteps stand alone in the hole. At KS17 the laptop band falls
  about 10 dB, then the koi's upper partials glide against the pad's static lines into S7 (the C09 separation).
- Superseded by D-040: my fifth-start "REQUEST DR" (hull / panel / shafts on DreadScore's numbers). DR's DreadCourse is the source
  now; DreadScore reads HullCourse.LaunchS / Speed / Ramp / Length / Beam / AxisX / CentreY / HalfDown / SternZ(tau) and
  PanelCourse.LaunchS / FaceX / Y0 / Z0 (DR: please keep these names). The machine hums stay on DreadScore.Shafts (AU-28 open).
- BV's dwell (blvd copy, read-only): TramSchedule.DoorsCloseS => DreadScore.ChimeS and DoorOpen use DreadScore.M: both kept.
- 03:24-03:27 AU-b01 (Logs/AU-b01.log): "[NOC] BuildWindows: result=Succeeded errors=0 warnings=2 size=463.7 MB". My bash call
  lost the backslash of -BuildDir (the build landed in waveb/BuildsAU); moved to Builds/AU before any run.
- 03:28-03:33 AU-smoke (Builds/AU, -autotest -brisk -autotest-until 560, Logs/player-AU-smoke.log): "[NOC] AUTOTEST PASS
  mode=brisk_until560 ... beats=4 errors=0" (QA/reports/autotest_brisk_until560.json); C52 lines: "[NOC] audio heartbeat t 221 s,
  s 495.6: 1405 callbacks, window max 10.02 ms, since the intro: slow (> 50 % of the buffer) 5, overruns 0" ... "[NOC] audio
  summary t 248 s, s 560.0: ... slow 8, overruns 0 (audio overruns after the intro = 0)"; alloc at quit mean 35.2 B/frame
  (engine input box 32), GC collections 0.
- W: free space 7.0 GB at 03:28 (under the 15 GB floor; my share: Builds/AU 464 MB, reports < 2 MB, no captures).
- 03:43-03:47 AU-dump11 (Builds/AU, 960x540, standing at s 520: KS09, the drop at full depth, the swell at its peak, the hull
  overhead on this tree's course = far): "[NOC] audio dump vs offline: dump -34.55 dBFS, offline -34.51 dBFS, difference -0.04 dB
  (<= 1.0), 1130 callbacks, s 520.0, 10.0 s -> PASS" (Logs/player-AU-dump11.log). WAV check: 1 s windows -35.15..-33.42 /
  -35.09..-33.59 dBFS, max step 0.0063 / 0.0060, longest zero run 0. (At the same s the wave-A dump03 read -30.81: the drop now
  takes 3.7 dB off the whole output, swell included.)
- 03:47-03:48 AU-dump12 (standing at s 1350: S7, the koi): "dump -30.02 dBFS, offline -29.88 dBFS, difference -0.14 dB -> PASS"
  (QA/reports/audio_dump_AU-dump12.json); 1 s windows -30.31..-29.53 / -30.59..-28.96, max step 0.0552 both, zero run 0; the koi's
  new 300-520 Hz partials present in the in-game output (-40.0 vs -38.5 dBFS in that band; the koi's FM phase differs by start time).
- 03:42-03:54 AU-final (full EditMode suite, this copy's tree with all my changes, QA/reports/AU-final.xml): 476 tests, 475 passed,
  0 failed, 1 skipped (AbyssTests.KS10_CompositionSearch, explicit); audio 47 / 47 (AudioDreadTests 17, AudioScoreTests and
  AudioTests 30). GD-final (03:05-03:19, same tree incl. the train trim) had the same 476 / 475 / 0 / 1.
- 03:55 harness hullcheck (new command: the hull test's level check on the compiled course): DR's C01 course: closest pass s 562.5,
  -42.31 dBFS (test range -48.0..-40.5); this tree's old course: closest at s 440, -56.43 dBFS (the "never passes over" branch): the
  hull test passes before and after the merge. fastcheck all OK (runtime 217 / editor 287 files).
- Report items: shared requests for the lead (README loudness text; QA's overrun line; the 4K cinematic dump), DR (keep the
  DreadCourse names DreadScore reads; optional: pure cabin / gantry functions), proposed decisions (drops, meet clearance, the passes
  -3 dB, the koi partials ADR, small-speaker layers, D-038 (10) outside the drops, the hum high-pass, the pad FDN width).
  New files this wave: none in Assets (all edits in existing audio files); reports QA/reports/AU-t11.xml, AU-final.xml,
  audio_dread_AU-walk25-f08.json/.txt, audio_dread_AU-walk15-f08.json/.txt, audio_walk_AU-walk25-f08.txt, audio_walk_AU-walk15-f08.txt,
  audio_AU-walk25-f08.md/.json, audio_AU-walk15-f08.md/.json, audio_AU-f08-hum-pad.txt, audio_AU-f08-ks09-spec.png,
  audio_AU-f08-ks17-spec.png, audio_dump_AU-dump12.json, AU-dump11/12(_offline).wav.
AU DONE 2026-11-21 03:57


---

## ARK (2026-11-21 evening, D-043, Logs/ark/ARK.md; copy .worktrees/integ at f808f1b, partner FX)

Start 20:16. Order (AU's part of ARK.md): the Ark's sound from DreadCourse: a slow sub-bass pressure, hull groans, distant horn
calls, a long Doppler; a silence drop before the bow passes overhead; the 14.3 gates hold. Progress: Logs/AU_progress.md.

- 20:20 DR's Ark was not in any copy yet: first cut on a mirror of ARK.md's numbers (from the south, 28 m/s). 20:45 DR's HullCourse
  appeared in the abyss copy (Ark from the NORTH toward -z, bow = the leading south end, launch s 380, 72 -> 27 m/s approach,
  bow over z 195 at tau 86.4 = s 596, stern at tau 270.3 = s 1 055.7). The audio now reads it through the alias ArkC
  (DreadScore.cs top) = ArkCourseMirror, a copy of DR's formulas and constants (end of DreadScore.cs) until the merge, when the
  alias becomes "using ArkC = Megastructure.HullCourse;" and the mirror is deleted (shared request to the lead).
- Code (my files only): DreadScore (THE ARK section: ArkC reads, ArkDir -1, ArkBowSeconds, ArkDropShape, ArkCover, ArkQuietS0/S1,
  the calls HornS {440, 740, 1090} / HornHz / HornDbs / HornPart, ArkHornSource / ArkHornDistanceDb / ArkHornMaxDist 3.5 km,
  ArkGroanTau (on the Ark's clock) / ArkGroanSource / ArkGroanLevelDb, ArkPeakDb -38, ArkBodyUpperDb -2, ArkUpperSwellDuck 0.8,
  ArkGardenTrimDb -2.5, ArkBowGain 1.27; DropGainOf; DreadEvent.Ark), DreadVoices (ArkDrone replaces HullDrone: body line source
  + side voice + brown bed + breath, its A3 E4 A4 layer, the bow wave with its Doppler, 4 engine pods with their Dopplers and
  distance LP, the 9/12/8 layers; DreadLayer: the Ark's groans on its clock (2 voices, ticked whatever the solo mask), the calls
  placed at fire time), NocMixer (the larger of DropShape(s) and the Ark's silence; wind -12 / pad -18 in the Ark's silence; the
  cover: H T -8 dB, X beds and passes -6 dB), AudioDreadTests (Hull test -> 7 Ark tests; Cues: 3 calls, the Ark's stretch, no glow
  in the silence; C09 test uses ArkDrone.UpHarm), harness (arkcheck replaces hullcheck; cues JSON ark / arkgroans, the calls'
  delay at the run's speed), dread_analysis.py (ArkDrop / ArkBelly / ArkGroan windows and gates; a KS drop inside the Ark's
  silence is measured against the Ark's reference; the legacy Hull window only without an Ark), docs/AUDIO.md section 12.
- Scratch test runner (scratchpad au_ark/tr: AudioDreadTests compiled with the harness sources against Unity's nunit, run by
  reflection; the hums test excluded, it needs DR's builder): 22 / 22 at 21:06, Ark tests at every iteration in seconds.
- Iterations (harness whole walks, WAVs in %TEMP%\au_runs_ark on C:, deleted after analysis):
  - a01 (ARK.md mirror, peak -40): devs 2.5 -0.03 +0.18 -0.02 -0.28 +0.71 -0.04 -0.12; the silence only -5.8 dB in the first test
    (its reference window lay in KS09's drop; the wind -36 dBFS is the bridge's loudest bed) -> the Ark's own wind -12 / pad -18;
    dread_analysis 60/62 (C09 laptop band -12.6 / -13.3 dB) -> the body's A3 E4 A4 layer.
  - a03 (DR's course): 2.5 61/62 (the legacy Hull window bow..bow+60 holds three train meets: dropped when there is an Ark);
    beat 5 +1.21 in the EditMode stretches -> ArkGardenTrimDb -2.5; 1.5 m/s: KS09 / KS10 swells +0.33 / +0.28 dB over the pad in
    the 250 Hz octave -> the Ark's upper layers give way under the swells (x (1 - 0.8 env)); the stern call at 1.5 m/s came from
    5 km -> no call beyond 3.5 km.
  - a04: 2.5 60/60; 1.5 55/56 (C09 under the belly -5.5 dB: the bridge's trains). Looked at the bow-pass spectrogram: the Ark in
    the silence only about -41 dBFS, level with the footsteps -> ArkBowGain 0.9 -> 1.27 (+3 dB on the pass itself).
  - a05 (final): devs 2.5 -0.03 +0.18 -0.01 -0.12 +0.67 -0.02 -0.12, 1.5 +0.01 -0.14 +0.32 -0.40 -0.01 +0.00 -0.09; EditMode
    stretches 2.5 -0.25 +0.02 +0.08 -0.34 +0.88 +0.02 -0.51, 1.5 -0.38 -0.45 -0.02 -0.68 +0.66 +0.02 -0.30; TP -11.42 / -9.00 dBTP;
    steps 0.142 / 0.134; the silence: rest -13.6 dB (K -15.2 LU, laptop -14.2), the Ark +5.9 dB over the rest in it (1.5: -16.1,
    +9.5); under the belly laptop best band +1.1 dB (1.5: -5.5, deviation); dread_analysis 60/60 and 55/56; QA's
    audio_analysis.py PASS at both speeds (QA/reports/audio_AU-ark-walk25-a05.md / -walk15-a05.md).
- Looked at: QA/reports/audio_AU-ark-a05-bowpass-spec.png (mix, t 222-252 s: the rain band leaves over 2 s at t 229.4-231.4 and
  returns over 5 s from t 240.4 with no vertical line; the footsteps alone in the hole; E at -38 dBFS through the pass; T1's pass
  rises with the return) and audio_AU-ark-a05-E-low-spec.png (E alone, 20-500 Hz, s 405-680: the 13.75 Hz stack lines, the pods'
  slow glides down at 220-330 Hz, the groans at t 240 and 266, the KS10 / KS13 swells).
- 20:40-20:45 AU-t01 (Unity, AudioDreadTests|AudioScoreTests|AudioTests, the 20:36 code): 52 / 52 (QA/reports/AU-ark-t01.xml; the
  tracked AU-t01.xml restored from HEAD).
- 21:15-21:17 AU-b01 (Logs/AU-b01.log): "[NOC] BuildWindows: result=Succeeded errors=0 warnings=4 size=464.1 MB" (the final code:
  the last audio edit 21:12).
- 21:18-21:30 AU-ark-final (full EditMode suite on the AU-b01 tree, QA/reports/AU-ark-final.xml, 708 s): 518 tests, 517 passed,
  0 failed, 1 skipped (AbyssTests.KS10_CompositionSearch, explicit); audio 53 / 53.
- 21:20-21:24 AU-smoke on AU-b01 (Logs/player-AU-smoke.log): "[NOC] AUTOTEST PASS mode=brisk_from400_until880 ... beats=3 errors=0"
  (QA/reports/autotest_brisk_from400_until880.json); "audio summary t 203 s, s 880.0: ... slow (> 50 % of the buffer) 24, overruns 0
  (audio overruns after the intro = 0)", window max 18.52 ms (FX's players and the suite ran beside it).
- 21:22 AU-ark-dump01 (AU-b01, 960x540, standing at s 590): "[NOC] audio dump vs offline: dump -30.20 dBFS, offline -29.72 dBFS,
  difference -0.48 dB (<= 1.0), 1150 callbacks, s 590.0, 10.0 s -> PASS" (QA/reports/audio_dump_AU-ark-dump01.json; the tracked
  audio_dump.json restored from HEAD); WAV check: 1 s windows -32.3..-29.2 / -32.2..-27.8 dBFS, max step 0.099 / 0.087, zero run 1.
- 21:28 merge rehearsal (scratchpad au_ark/cmp): DreadScore with "using ArkC = Megastructure.HullCourse;" and the mirror deleted,
  compiled with the abyss copy's live DreadCourse.cs: the mirror equals DR's course to 0 m / 0 m/s over tau 0-450 (stern pass
  270.271 both), and AudioDreadTests 22 / 22 on the merged tree.
- 21:31 AU-ark-walkdump (AU-b01, -autotest -cinematic -beat 4 -autotest-until 680, a 44 s -audio-dump from about s 585): "AUTOTEST
  PASS mode=cinematic_from560_until680 ... errors=0"; dump -29.97 vs offline -30.26 dBFS; looked at the real output's spectrogram
  (QA/reports/audio_AU-ark-walkdump-spec.png): the silence, the world coming back at t 7-12 s with T1's pass, the hushed crossing.
- 21:33 the calls fire on the Ark's clock (DreadScore.ArkCallTau; a05 at 1.5 m/s sounded only the first: the second and third came
  from 4-5 km and were skipped); a06: 2.5 m/s unchanged (60 / 60), 1.5 m/s all three calls (E -41.2 / -35.9 / -40.2 dBFS, under the
  pad -3.4 / -2.3 / -5.9 dB), 59 / 60 (the C09 belly deviation). Final reports: QA/reports/audio_walk_AU-ark-walk25-a06.txt /
  -walk15-a06.txt, audio_dread_AU-ark-walk25-a06.* / -walk15-a06.*, audio_AU-ark-walk25-a06.md / -walk15-a06.md (QA's checker
  PASS), audio_AU-ark-a06-bowpass-spec.png, audio_AU-ark-a06-E-low-spec.png (looked at: as a05, no vertical line in the fall or
  the return). DSP: harness dreadcost 4.8 % of a core for the whole layer at s 700 (loaded machine); alloc 0 bytes / 4000 callbacks.
- 21:44 AU-b02 (Logs/AU-b02.log, the final code with the calls on the Ark's clock): "[NOC] BuildWindows: result=Succeeded errors=0
  warnings=4 size=464.1 MB".
- 21:45-21:48 AU-smoke2 on AU-b02 (Logs/player-AU-smoke2.log): "[NOC] AUTOTEST PASS mode=brisk_from400_until880 ... beats=3
  errors=0"; "audio summary t 198 s, s 880.0: ... window max 8.00 ms, slow 2, overruns 0 (audio overruns after the intro = 0)";
  alloc at quit mean 31.4 B/frame (engine input box 32, excess 0.0), GC collections 0.
- 21:45-21:55 AU-ark-final2 (full EditMode suite on the final tree, QA/reports/AU-ark-final2.xml, 609 s): 518 tests, 517 passed,
  0 failed, 1 skipped (AbyssTests.KS10_CompositionSearch, explicit); audio 53 / 53, the 6 Ark tests passed in Unity.
- 21:56 DR's DreadCourse.cs in the abyss copy unchanged since 21:10; the merge rehearsal still compiles and passes (the two render
  tests need Unity's logger: 22 / 22 with the logger shim at 21:28). fastcheck all OK (runtime 221 / editor 296 files).
- Open: C09 under the belly at 1.5 m/s -5.5 dB (the bridge's train passes; 2.5 m/s +1.1 dB); the audio's VehicleClock resets the
  Ark if the walker goes 12 m back below s 380 (DR's DreadClock never resets: only reachable walking back into the canyon);
  QA/reports/autotest_brisk_from400_until880.json (tracked) holds AU-smoke2's result.
- Shared: lead at the merge: "using ArkC = Megastructure.HullCourse;" and delete ArkCourseMirror (DreadScore.cs); DR keep the
  HullCourse names the audio reads; AT: any new LightningS must keep out of the Ark's silence (s 573.5-613.5 at 2.5 m/s).
AU ARK DONE 2026-11-21 21:56
