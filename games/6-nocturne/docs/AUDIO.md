# Nocturne audio: the score, the stems and how to render them

Owner AU (Phase 4 wave A). Spec: MASTER_PROMPT 14.3 (sound bible), 14.6 (the per-boundary windows), 14.7 H7 (vehicles) and H8
(the pad and the mix), 11.9 (audio verification), Part 10 item 25 (audio defects); decisions D-011 (R5), D-032, D-033; the design
research docs/research/R5_pad_rain_audio.md. Code: `Assets/_Game/Scripts/Runtime/Audio/`. Tests:
`Assets/_Game/Scripts/Tests/EditMode/AudioTests.cs` (DSP blocks) and `AudioScoreTests.cs` (the score). Tools: `Tools/audio/`.

## 1. What the walk sounds like

Everything is synthesised; there are no samples. All levels are keyed to progress s (m), all phases to time t (s), so the score
is the same piece at 1.5 m/s (1 000 s), 2.5 m/s (600 s) or any speed, standing still or walking back.

| Beat (s) | What you hear | Pad chord(s) | Reverb |
|---|---|---|---|
| 1 Alley (0-120) | S1: only rain and the city hum for 8 m, then the pad enters over 6 s. Rain patter on concrete and awnings, awning drips ringing at 1.6 kHz, puddle bubbles, 40 AC units droning 40-75 Hz, the four steam vents (s 24, 58, 86, 104) hissing, the vending machines' 100 Hz hum and cycling compressor, a neon buzz near 3 kHz, footsteps on wet concrete with splashes. S2 (s 104-124): the reverb opens and the traffic rises 6 dB as the mouth nears. | C0 Dm9, C1 Bbmaj9 at 124 | alley 0.9 s / 8 ms |
| 2 Boulevard (120-400) | The canyon's breath (band 160 Hz), the traffic wash of ten lanes overhead with near pass-bys sweeping across, sodium ballast buzz, the tram wire's faint sing; the tram launches at s 230 from the terminus, rings its bell twice and passes abeam (s 272 at 2.5 m/s, 258 at 1.5) with its motor whine and rail sing. Stone footsteps. | C1, C2 Gm11 at 286 | boulevard 2.8 s / 30 ms |
| 3 The Edge (400-560) | The canyon ends: S3 (400-420) the pad drops 6 dB and the wind rises; the abyss's deep hum (brown noise LP 45 Hz), an updraft whistle, wind band-passed at 300 and 900 Hz. S4 (540-554): everything ducks 5 dB except the wind and the deep hum (+3 dB). | C3 Ebmaj7#11 at 402, C4 Fmaj9 at 540 | Edge 4.5 s / 250 ms |
| 4 Skybridge (560-880) | Steel-deck footsteps (310 / 820 Hz rings, 85 Hz thump), glass on the strip (706-726), rain pinging on steel and glass, wind gusts locked to the picture's rain slant (period 17 s), the tubes' linear-motor drone (60.0 / 60.37 Hz beating) and rail whine, and the seven trains T1-T7 (38 m/s, 174 m) launched at their s: finite line sources, panned by the radiating segment, Doppler (+12 % approaching, -10 % receding), the nose's pressure whoosh and the bogie thumps. | C4, C5 Dm11 at 700 | bridge 3.5 s / 60 ms |
| 5 Garden (880-1 080) | S6 (876-896): the wind and the traffic fall 12 dB and the leaf rain rises; dense soft patter on seventeen crowns, rustle with the crown sway, leaf drips at 3-6 kHz, soft garden footsteps. The quietest, widest beat (L/R correlation 0.05). | C6 Dmaj9 at 880 | garden 1.6 s / 20 ms |
| 6 Wall of signs (1 080-1 260) | The wall buzzes: the hum layer is at its loudest (H -33: mains 50 / 100 / 150 Hz and a neon buzz around 3 kHz), the screens' 100 Hz family with a 0.1 Hz flutter, arcade drips. From s 1 200 the koi tone rises faintly (the teaser with the cloud glow). | C7 Bm11 at 1 080 | signs 2.2 s / 25 ms |
| 7 Heart Square (1 260-1 500) | The koi tone (E1 41.2 Hz and A1 55 Hz breathing against each other with 0.07 Hz FM, partials 123.6 / 164.8 / 247.2 Hz audible on laptop speakers) follows the koi's distance and swells to -33 dBFS in S7 (1 340-1 380), ducking the pad 3 dB. S8: at s 1 486 the pad reduces to the open fifth D5; after the goal trigger (s 1 497) pad, koi, traffic and wind release over 8 s; the last seconds are rain, the hum and the goal lamp's 100 Hz buzz. | C8 Gmaj9#11 at 1 262, C9 Em9 at 1 350, C10 D5 at 1 486 | plaza 3.8 s / 70 ms |

The pad never plays a melody: five voices per chord, voice-led so common tones hold, under a low-pass that opens with the 14.1
intensity curve (500 Hz at intensity 3, 1 600 Hz at 10) and breathes with a 0.04 Hz LFO, through a stereo chorus and its own
6 s reverb.

## 2. Architecture

One DSP, `NocMixer`, runs everywhere: in the player (`NocSynth.OnAudioFilterRead`, generator mode on a clip-less AudioSource,
played by `AudioDirector` after both components exist), in the EditMode tests and in the offline renders
(`OfflineAudioRenderer`, `NocturneScore.Render`, the player's `-render-audio`, Tools/audio/harness). Rules (D-011, R5 2):

- control runs every 32 samples at absolute sample indices n = 32 k, carried across callbacks; every event (patter, drips,
  pass-bys, footsteps, bogies, the bell) is a pure function of an event or sample index (counter hashes), so the output is
  bit-identical for any callback size (test `Mixer_OutputIndependentOfTheCallbackSize`: max |diff| 0);
- double precision for phases, filters and gains; 48 kHz nominal, any device rate (44.1 kHz renders within 0.5 dB);
- every gain moves through a 50 ms one-pole ramp interpolated per sample (`GainRamp`); nothing switches;
- no allocation in `Process` (test `Mixer_ProcessAllocatesNothing`; the harness `alloc` measures 0 bytes on .NET), no locks,
  no UnityEngine on the audio thread; a non-finite sample resets the reverbs and the limiter and plays silence for the block.

Motion: offline, `WalkMotion` gives the exact s, speed, listener position (the 14.4 centre line) and yaw (the cinematic gaze
schedule) per control block. In the player, `AudioDirector` publishes a snapshot `NocAudioParams` every frame (master gain =
UiDirector.FadeAudio x the pause duck, rain density from `NocRainState.RhoAt`, s, the 150 ms-smoothed speed, the camera position
and yaw, the scene time, the footstep counter with gain / pan / surface from the PlayerController's strides); the DSP
extrapolates s by the speed for at most 0.25 s between snapshots.

Signal flow per sample:

```
R RainBed ─┐  D DripField ─┐  H CityHum ─┐  T TrafficWash ─┐  W WindBed ─┐  K KoiTone ─┐  X beds + passes + leaf + lamp ─┐  F footsteps ─┐
           │ gains g_l(s) = NocturneScore.LayerAmp (14.3 table x Rule-P blends x trims x S-windows), 50 ms ramps                          │
P PadEngine (10 banks of PadVoice -> TPT LP -> chorus) x g_P -> pad FDN (6 s, 40 ms, wet -3 dB) ─────────────────────────────────────┤
           sends (R 0.12, D 1, T 0.25, W 0.08, K 0.2, X 0.3, passes 0.6, F 1) -> environment FDN (14.3 places, 14.6 windows) ──────┤
                                                                          sum -> 20 Hz DC blocker -> master -> true-peak limiter -> out
```

Limiter: The Ring's look-ahead limiter with an ITU-R BS.1770-4 true-peak detector (`TruePeakLimiter`): knee -4.5 dBFS, ceiling
-1.5 dBFS, 5 ms look-ahead, 80 ms release. On the whole walk it never engages (true peak -4.9 dBTP at 2.5 m/s).

### Files

| File | Contents |
|---|---|
| `NocturneScore.cs` | the score as pure functions of s and t: the 14.3 layer table and Sum, per-beat trims, S1-S8, chords and g_c(s), pad trims, the intensity curve and the pad cutoff, reverb places and windows, the koi distance law, the vehicle schedule (tram, T1-T7, `MeetS`), rain coupling, surfaces, `Render(s0, s1, speed, path)` |
| `PadVoice.cs` | `PadVoice` (saw + triangle pair) and `PadEngine` (5 slots x 2 banks, voice plan, filter, chorus) |
| `NocMixer.cs` | the DSP: layers, gains, sends, reverbs, master chain, snapshot extrapolation, stems, solo mask |
| `RainBed.cs` | R: pink HP 800 Hz bed with the 0.1 Hz gust, the surface-coloured patter; `ScoreFrame` |
| `DripField.cs` | D: virtual emitters on a 1.2 m lattice along s (awning, puddle Minnaert chirp, leaf) |
| `CityHum.cs` | H: brown LP 90 Hz, the abyss's deep hum, mains 50 / 100 / 150 Hz, neon buzz 2.4-3.6 kHz |
| `TrafficWash.cs` | T: pink band 200-900 Hz with near pass-bys; `WindBed` W: 300 / 900 Hz gusts (beat 4 locked to the rain slant) |
| `BeatBeds.cs` | X beds: AlleyBed, CanyonBed, EdgeBed, TubeHum, ScreenHum, LampBuzz |
| `TramPass.cs`, `TubeTrain.cs` | the tram and the trains: `VehicleClock` (launch by s, then real time, reset 12 m below), `PassGeometry` (finite line source, pan, width, Doppler), `PassCore` |
| `KoiTone.cs`, `LeafRain.cs` | K and the garden's leaf rain |
| `Footsteps.cs` | `FootstepBank` (6 surfaces x 8 variants, -30 dBFS peaks), `FootstepVoices`, `StepScheduler` |
| `NocDspBlocks.cs`, `NocDsp.cs` | DSP blocks (TPT SVF, DC blocker, PolyBLEP / polyBLAMP, chorus, FDN, limiters, meters, hashes) |
| `NocSynth.cs`, `AudioDirector.cs` | the player host (OnAudioFilterRead, snapshot mailbox, capture for -audio-dump) |
| `OfflineAudioRenderer.cs`, `WavWriter.cs` | offline renders, sinks (RMS, meter, WAV stream) |
| `AudioAnalysis.cs` | `WalkAnalysis` (100 ms bins, 3 s windows, gates), `ClickMeter`, `AudioGates` (the test renders), snapshot scripts |

## 3. The score (`NocturneScore`)

**Layer gains.** Every layer is built to unit RMS at gain 1, so its gain is the 14.3 table value: `LayerAmp(l, s)` =
10^(table/20) blended over the 24 m around each beat boundary (Rule P, smootherstep) x the beat trim x the S-window gain. The
table's Sum column is the power sum of each row (test `LayerTable_SumColumnIsThePowerSumAndTrimsAreSmall`).

**Trims** (QA/audio-trims.json, checked against the code by `AudioTrimsJson_MatchesTheScore`): per beat +0.25 -0.39 -0.08 -0.13
-0.21 +0.03 +0.49 dB on every layer; the pad per chord +0.5 -0.2 -0.9 +0.1 +1.3 +0.8 -0.8 +0.1 -1.55 -0.1 0 dB (the five voices of
a chord, normalised per voice, still differ by up to 1.5 dB after the chorus and the pad FDN); the beat-7 pad +1 dB outside S7
(D-011: K reaches -33 only inside S7, which the gate excludes).

**S-windows** (`SwellGain`, smootherstep ramps of 4-8 m): S1 s 0-8 rain and hum only (T, W, K, X ramp in over 4-14 m; the pad
enters by its 6 s attack once s passes 8); S2 104-124 T +6 dB; S3 400-420 P -6 dB, W +5 dB; S4 540-554 all -5 dB except W and H
+3 dB; S6 876-896 W and T -12 dB, leaf rain +4 dB; S7 1 340-1 380 the koi peaks at -33 dBFS (its distance law, peak at s 1 364)
and the pad ducks 3 dB; S8 the C10 open fifth from s 1 486 and the 8 s release after the goal trigger.

**Chords.** The 14.3 table (C0 Dm9 ... C10 D5). `ChordIndex(s, out w)` gives the active window and the smootherstep weight;
`ChordGain(c, s)` sums to 1 at every s (40 m windows centred on the change point; C10 uses [1 472, 1 500] so the fifth is complete
at the stop, D-011).

**Reverb places** (14.3) blended over the 14.6 windows (alley -> boulevard 100-130, boulevard -> Edge 380-420, the others 40 m
centred on the boundary): RT60 / pre-delay / damping 0.9 s / 8 ms / 0.6, 2.8 / 30 / 0.45, 4.5 / 250 / 0.5, 3.5 / 60 / 0.5,
1.6 / 20 / 0.7, 2.2 / 25 / 0.5, 3.8 / 70 / 0.45; damping d maps to the FDN loop low-pass 12 kHz x (1 - d)^1.5. Wet level per
place -14 ... -10 dB (not in the spec: small rooms drier).

**Koi.** `KoiDistance(s)`: the listener at the path point of s (eye 1.7 m) to the koi head (KoiMath orbit and head azimuth of
the phase u(s)); law 1 / (1 + (d/400)^2) x smootherstep(1 200, 1 290, s), normalised to -33 dBFS at its peak (s 1 364): -77 dB at
s 1 210, -47 at 1 250, -40 at 1 300, -32.5 at the peak, -39 at 1 470.

**Vehicles** (14.7 H7). Launch when s first crosses s_launch (tram 230; T1 584.5, T5 600, T3 639.5, T4 690.7, T6 695, T2 752, T7
800), then real time tau; a jump of s past the launch uses the capture rule tau = (s - s_launch)/2.5; reset when s falls 12 m
below. Abeam (`MeetS`) at 2.5 / 1.5 m/s: tram 272.0 / 258.2 (14 m apart, as 14.7 says), T1 602.0 / 595.3, T3 653.6 / 648.2, T4
699.3 / 695.7, T2 759.1 / 756.4, T7 816.3 / 809.5 (bridge tubes <= 6.8 m apart), T5 624.7 / 615.2 and T6 715.8 / 707.1 (the face
tubes far above and below).

**Rain coupling.** `RainRhoSchedule(s)` equals the picture's `NocSchedule.RainRho(s)`; `RainGustWeight(s)` equals
`NocRainMath.Gust(s)` (test `RainCoupling_EqualsThePicture`). `-rain X` overrides rho in both (D-020); the R level scales with
sqrt(rho / rho_schedule), so `-rain 0` silences the rain.

## 4. The pad (14.7 H8)

- `PadVoice`: a PolyBLEP saw at -4 cents (mix 0.55) and a 2-point polyBLAMP triangle at +4 cents (mix 0.85; the D-011 reading of
  the "integrated triangle": no leaky integrator, no start-phase DC), double phases, each with its own 0.03 Hz drift of +-2 cents
  (the triangle's at 1.13 x 0.03 Hz).
- `PadEngine`: five slots x two banks. The voice plan (`BuildPlan`) pins common tones to their slot first and assigns the rest by
  pitch order (pure pitch order would move a common tone in 8 of the 10 changes, R5 3.5); a slot whose note changes crossfades
  its old bank against its new bank with an equal-power law on w (a sum-to-1 amplitude fade of uncorrelated voices dips 1.5-2.2
  dB at the centre); the chord weights g_c(s) themselves sum to 1. A bank changes its note only while silent (gain < 1e-4): a jump
  of s fades the old note out first (50 ms), then takes the new one.
- One stereo Simper TPT low-pass (Q 0.75) for all voices: cutoff 500 x 3.2^((I - 3)/7) Hz from the 14.1 intensity curve I(s),
  x 2^(0.3 sin(2 pi 0.04 t)), smoothed in log frequency over 20 ms; each bank's normaliser comes from an analytic harmonic sum
  through the filter, so the level does not follow the cutoff.
- Stereo chorus 14 / 19 ms, depth 0.6 ms, per-sample LFOs (0.17 / 0.23 Hz) from the sine table, 4-point Hermite reads; then the
  pad FDN (6 s, 40 ms pre-delay, 4 kHz damping, -3 dB wet).
- Phases advance with t only; only gains and the cutoff depend on s: continuous at any speed (test
  `PlayerPath_ContinuousAtAnySpeedStandingWalkingBackAndSprinting`: stand, 2.5, stop, back 1.5, 10 m/s across C1 -> C2).

## 5. Stems

`NocturneScore.StemCount` = 9 stems: R rain, D drips, H hum, T traffic, W wind, P pad (with its FDN), K koi, X beat-specific
(beds, tram, trains, leaf rain, lamp), F footsteps. The environment reverb returns into the mix only (it is not a stem).
`NocMixer.MeterStems` meters each stem's mean square after its own 20 Hz DC blocker (what the master's blocker lets through);
`NocMixer.SoloMask` (bit = stem index; `OfflineRenderSettings.SoloMask`) renders any subset.

## 6. Rendering

**C#** (EditMode, any script):

```csharp
NocturneScore.Render(0.0, 1500.0, 2.5, @"C:\tmp\walk25.wav");             // 32-bit float stereo 48 kHz, footsteps on
var st = new OfflineRenderSettings { S0 = 560, S1 = 880, Speed = 1.5, SoloMask = 1 << NocturneScore.X, PrerollSeconds = 4 };
using (var w = new WavStreamWriter(@"C:\tmp\beat4_X.wav", 2, st.SampleRate, WavFormat.Float32)) OfflineAudioRenderer.Render(st, w);
```

**Player**: `Nocturne.exe -render-audio <file.wav> [-render-s0 S] [-render-s1 S] [-speed 1.5 | -brisk | -cinematic]` renders
offline and quits (AudioRenderRunner, with a sidecar for audio_analysis.py). `-audio-dump <file.wav> [-audio-dump-seconds N]`
records N s (default 10) of the real OnAudioFilterRead output 4 s after the intro, renders the same window offline (same rate,
seed, s and rain, 8 s pre-roll, standing still) to `<file>_offline.wav` and logs `[NOC] audio dump vs offline ... PASS` (limit
1 dB) with `QA/reports/.../audio_dump.json`. Run it standing still, e.g. `-progress 0.2,-nofade,-quit-after 30`.

**Harness** (fast, no Unity; .NET 10): compiles the runtime audio sources with the few pure Core / Player / KoiMath files they
need against UnityEngine.CoreModule into a console app (build output in %TEMP%\nocturne_audio_harness).

```
dotnet build "Tools/audio/harness" -c Release
dotnet %TEMP%/nocturne_audio_harness/bin/NocAudioHarness.dll walk --speed 2.5 [--s0 0 --s1 1500] [--ending] [--seed N]
       [--rain X] [--preroll S] [--solo RDHTWPKXF] [--nosteps] [--wav out.wav] [--json stats.json]
   ... gates       the AudioScoreTests renders with their margins (AudioGates: the same code)
   ... calibrate   stems vs the table per beat;   partition   callback-size invariance;   alloc   bytes per Process
   ... timing      DSP cost;   probe   koi law and vehicle meets;   unit --s S   each generator alone at gain 1
```

`walk` renders 600 s in about 31-36 s (17-20x real time), prints the per-beat gate, stems, S-windows, the loudest bins and the
chord-change steps, and with `--wav` writes the WAV plus its `.json` sidecar (s per second).

**Analysis**:

- `python -I Tools/qa/audio_analysis.py <wav> --run <id>` (QA's independent checker): per-beat medians against 14.3, peak, DC,
  steps, S-windows, BS.1770 loudness -> QA/reports/audio_<id>.md/json.
- `python -I Tools/audio/analyze.py <walk.wav> [--pad <pad-solo.wav>] [--json out.json] [--png spec.png]`: per beat the level
  spread, octave bands, centroid, stereo correlation, hum and koi partials, onset rate and the flux period (the footstep cadence),
  per chord the chroma share on the chord's pitch classes, and a log-frequency spectrogram with the boundaries and chord changes.
- `python -I Tools/audio/calibrate.py <stats.json>`: per beat and layer, the median of (stem - target) where the layer is steady,
  and the pad per chord: the numbers behind the trims.

## 7. Calibration

1. Every generator is unit RMS by construction (analytic noise bandwidths and harmonic sums, the DC blocker's loss included);
   the harness `unit --s S` checks each alone.
2. `walk --speed 2.5 --json` then `calibrate.py`: stem minus target per beat and per chord. Fix a generator constant when one
   layer is off everywhere; use a trim only for the residual per beat (logged in QA/audio-trims.json, |trim| <= 1 dB) or per
   chord (<= 2 dB).
3. Verify at 1.5 m/s and with `audio_analysis.py`; run `gates` for the EditMode margins.

## 8. Measured (2026-11-16, run r04 / r05)

| Gate (14.3 / 14.7 H8) | Limit | 2.5 m/s | 1.5 m/s |
|---|---|---|---|
| beat medians - Sum (dB), whole walk | +-1.5 | -0.01 +0.13 -0.23 +0.27 +0.13 +0.00 -0.01 | -0.01 -0.13 +0.23 -0.27 -0.11 +0.01 +0.01 |
| true peak | <= -1 dBTP | -4.91 | -5.49 |
| DC | < 0.001 | 0.000000 | 0.000000 |
| max sample step | <= 0.25 | 0.147 (a train pass) | 0.138 |
| 20 ms envelope step at chord changes (mix) | < 0.05 | 0.012-0.037 | 0.013-0.037 |
| pad solo at chord changes: envelope / sample step | < 0.05 / - | <= 0.016 / <= 0.0074 | same |
| player path (snapshots at 60 fps) vs offline | 1 dB | 0.000 dB | 0.000 dB |
| in-game -audio-dump vs offline (built player, s 1125, 10 s, 2026-11-19) | 1 dB | +0.10 dB (-30.12 / -30.23 dBFS) | |
| BS.1770 integrated loudness (INFO) | - | -26.8 LUFS | |

S-windows (logged): S1 -36.8, S2 -33.1, S3 -32.3, S4 -32.9, S6 -33.5, S7 -29.4 (K -33.0), S8 -30.3 dBFS.

## 9. Decisions and deviations

- 14.7 H8 "no click at chord changes (step < 0.05 in 20 ms)" is the absolute step of the 20 ms RMS envelope (full scale 1).
  Relative 20 ms steps cannot be a click test: a low chord's 20 ms RMS does not average the cross terms of its partials (82.4 and
  123.5 Hz beat at 41 Hz = 24 ms), so the pad alone shows relative steps of 0.3-1.1 without any discontinuity. The click test is
  therefore structural as well: a bank changes its note only in silence, every gain moves by at most one 50 ms one-pole step per
  control block, the steady chord is equal power, and the pad's second difference inside a change window stays within 3x the
  steady value.
- Doppler on the passes (the wave-A task's "panned, Doppler"; D-011 had "without Doppler"): the pitch ratio uses the
  intensity-weighted mean radial velocity of the radiating segment of the finite line source in closed form,
  v_r = u d (1/r1 - 1/r2) / (atan(x2/d) - atan(x1/d)), so it is smooth through the pass (a point-source Doppler of a 174 m train
  12 m away would sweep 23 % in 0.3 s).
- The lamp's buzz (beat-7 X, table -60) is a point source at the goal window with the law 1 / (1 + (d/8)^2): -67 dBFS as the
  beat median, -62 in S8 and -60 at the stop.
- X beds in every beat (D-011): a transient-only X misses the median gate.

## 10. Open items

- (closed 2026-11-19) The in-game `-audio-dump` check passes in a built player: +0.10 dB at s 1125 (docs/logs/AU.md).
- The integrated loudness of the walk is -26.8 LUFS by the 14.3 table; a video upload normalised to -14 LUFS needs +12.8 dB of
  make-up gain at the edit (the limiter leaves 3.4 dB of true-peak headroom, so the edit needs its own limiter), or the table is
  raised as a whole (a lead decision; the per-beat shape is unchanged either way).

## 11. Wave-B hooks (D-034, docs/DREAD.md section 1 and the cues of sections 2-6)

Nothing is built for wave B yet. This is where each item plugs into the wave-A graph, so that wave A blocks none of it.

- **One-shot sources** (horn, metal strike, groan, the hull drone): one class per source, like `TubeTrain` / `TramPass`.
  - Events come from an event-index hash at absolute sample indices, so the output stays independent of the callback size.
  - Each class owns its own unit-RMS gain law, keyed to s.
  - Each sums into the X stem, or into a new stem E. A new stem needs `StemCount`, `LayerNames`, the `SoloMask` letters of the
    harness and a row in QA/audio-trims.json.
- **Echo across the Gulf (1.1)**: an `EchoBus` after the one-shot sum and before the env FDN send.
  - It is a stereo multi-tap delay line. The taps are recomputed per 32-sample control block from the listener x:
    - delay = 2 d / 343 s, gain 1 / (1 + d / 150), then a one-pole low-pass whose cutoff falls with d;
    - d_A = |x - 215.3|, d_B = |815.3 - x| at the neck; the face-to-face repeats add 600 m = 1.75 s each.
  - The taps glide with the same 50 ms one-pole as the gains, which keeps them click-free at any walk speed. A tap that jumps
    by more than 1 ms in a block cross-fades over 20 ms instead of sliding (no pitch zip).
  - Memory: the longest path is the Boulevard mouth (3.5 s) plus one repeat (1.75 s) = 5.25 s, that is 5.25 x 48 000 x 2
    floats = 2.0 MB, preallocated in the constructor. Process stays allocation-free.
  - Only the one-shot bus feeds it. The beds already carry the env FDN.
- **Sub swell (1.2)** under KS04, KS09, KS10, KS13 and KS17: a 35-55 Hz sine pair with 2nd-3rd harmonics (<= 160 Hz).
  - Its gain is a function of s: it rises over 6-10 s of walk before each key-shot s and decays over 8 s.
  - Seconds are converted to metres at the reference speed 2.0 m/s, so the swell keeps its place at any speed and holds while
    the player stands still.
  - Headroom: the walk's true peak is -4.9 dBTP (beat 4 trains) against the limiter knee at -4.5 dBFS. A swell at -36 dBFS RMS
    adds 0.2 dB to a -30 dBFS beat median. The level gate excludes S3/S4/S6/S7 only, so swells outside those windows have to
    fit the beat's +-1.5 dB, or join the excluded windows (needs a lead decision).
- **Structural groans (1.3)**, every 20-60 s from s 120 on: resonant comb plus filtered noise with a slow pitch bend.
  - Event times come from the index hash of the walk-time counter.
  - Panned on the nearest large mass: the listener x/z and yaw are already in the snapshot.
- **Silence drop (1.4)**, 2.0 s before KS09 and KS17: a duck factor on the R and H gains in `NocturneScore`.
  - -18 to -24 dB over 0.6 s, back over 1.5 s; the window is given in s at the reference speed.
  - It passes through the existing 50 ms gain ramps, so it cannot click.
  - It lies inside S4 (KS09) and S7 (KS17), which the level gate already excludes. The click gate (20 ms envelope step
    < 0.05) and the step gate apply.
- **Hull drone (2.1)**: a point source on the hull path with the finite-source Doppler of the passes (section 9), plus the
  echo bus.
- **Tests to add with each item**: the swell and the drop as functions of s (positions, slopes, at both speeds); the echo delays
  against the DREAD table (0.93 / 2.57 s at the tongue tip, 1.87 / 1.63 s at mid-bridge); 0 bytes per Process; callback-size
  invariance; the level, peak and click gates at 1.5 and 2.5 m/s.
