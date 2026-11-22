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
| `DreadScore.cs`, `DreadVoices.cs`, `DreadAnalysis.cs` | the D-034 dread layer, stem E (section 11): its score, its voices and `DreadLayer`, `DreadProbe` |

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

`NocturneScore.StemCount` = 10 stems: R rain, D drips, H hum, T traffic, W wind, P pad (with its FDN), K koi, X beat-specific
(beds, tram, trains, leaf rain, lamp), F footsteps, E the dread layer (section 11, with its Gulf echoes). The environment reverb returns into the mix only (it is not a stem).
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

- D-034 timing (section 11): the seconds of DREAD.md are metres at the reference speed 2.5 m/s (the cinematic walk the video is
  cut from), so every dread cue is a pure function of s like the rest of the score: exact in the video, 1.67 x longer at
  1.5 m/s (a drop lead of 2.0 s becomes 3.3 s), held while the player stands. (AUDIO.md section 11 of 2026-11-19 had 2.0 m/s.)
- D-034 echo: image sources (A1 = 2 d_A / 343, B1 = 2 d_B / 343, AB2 = 1 200 / 343 = 3.50 s) instead of "repeats spaced by
  1.75 s": at mid-Gulf (x 535) the arrivals 1.63 / 1.86 / 3.50 s are 1.75 s apart as DREAD says; at the mouth B1 (3.41 s at
  x 230) and AB2 (3.50 s) merge into its "one long echo".
- The dread stem E counts in the 14.3 level gate like every layer (no new exclusion window): the beat medians stay within
  +0.53 dB of Sum (beat 3 at 1.5 m/s; 2.5 m/s +0.38 at most).

## 10. Open items

- (closed 2026-11-19) The in-game `-audio-dump` check passes in a built player: +0.10 dB at s 1125 (docs/logs/AU.md).
- The integrated loudness of the walk is -26.8 LUFS by the 14.3 table; a video upload normalised to -14 LUFS needs +12.8 dB of
  make-up gain at the edit (the limiter leaves 3.4 dB of true-peak headroom, so the edit needs its own limiter), or the table is
  raised as a whole (a lead decision; the per-beat shape is unchanged either way). The step is written below (fix wave C50).

**Upload loudness: the step at the edit (D-038, fix wave C50; the lead copies it into README).** The game is mixed to the 14.3
table: the cinematic walk measures -27.3 LUFS integrated (BS.1770-4) with its true peak at -7.6 dBTP (harness render f08 at 2.5 m/s,
QA/reports/audio_AU-walk25-f08.md). YouTube turns loud uploads down to about -14 LUFS and never turns quiet ones up, so an untreated
capture plays about 13.3 dB below a typical upload.
1. The audio track. Best: replace the screen capture's audio with the offline render of the same walk,
   `Nocturne.exe -render-audio walk.wav -cinematic` (48 kHz, 32-bit float; the same DSP as the game, a pure function of s at the
   cinematic pace; the footsteps are scheduled by distance, so they can sit a few ms off the picture's strides). Align it on the
   0.6 s fade-in: cross-correlate the first 10 s with the capture's own track and shift by the peak (a constant offset; no drift,
   both run at 48 kHz). Otherwise capture the game at unity gain: the Windows output device and the OBS source at 100 % / 0 dB,
   48 kHz, no OBS filters (no noise suppression, gain, compressor or limiter on the source or the master).
2. The gain. Add +13.3 dB into a true-peak limiter with its ceiling at -1.0 dBTP (oversampled detection, look-ahead >= 1 ms,
   release 50-100 ms) for about -14 LUFS integrated. In the f08 render this asks the limiter for up to 6.7 dB on the loudest
   peaks, in 2.7 % of the 10 ms blocks (mostly the train passes of beat 4, s 600-830): listen there, and
   through the KS09 and KS17 silence drops, for pumping or a lifted floor. If it pumps, use +11 dB (about -16.3 LUFS:
   YouTube then plays it about 2.3 dB under -14, which is still far better than untreated). Do not compress or normalise
   per scene: the dread layer's drops and swells are level contrasts of 10-15 dB by design.
3. Check: measure the exported file (ffmpeg -af ebur128=peak=true or the editor's loudness meter): integrated -14 +-1 LUFS, true
   peak <= -1 dBTP.

- (D-034) A human listen of the dread layer on a laptop and on headphones (AU-17): the swells and the hull are speaker and
  headphone effects by their spectrum; the numbers say the horn, the creak, the groans and the strikes reach laptops.
- (D-034) The picture owners take the audio's positions (section 11, Hooks): DR's hull, panel and shafts, AT's lightning
  glows, BV's door chime; if they move, DreadScore follows them.

## 11. The dread layer (D-034, docs/DREAD.md section 1 and the cues of sections 2-6; built 2026-11-20)

Stem E (`NocturneScore.E` = 9, `StemCount` 10, harness solo letter E) is `DreadLayer` (DreadVoices.cs) inside `NocMixer`:
nothing outside the audio files is wired. Its score is `DreadScore` (pure functions of s; the event table is built once).
`DreadProbe` (DreadAnalysis.cs) holds the measurements shared by the tests and the harness. Tests: `AudioDreadTests`.

**Time.** Durations of DREAD.md are metres at the reference speed 2.5 m/s (the cinematic walk the video is cut from): exact in
the video, 1.67 x longer at 1.5 m/s, held while the player stands. One-shots fire when s first crosses their s (re-armed when
s falls 12 m below, the vehicle rule; on a jump the past ones count as done), start at an exact sample (the trigger block plus
the flight time d / 343 s), and come from voice pools per kind; a busy pool skips the event (it never steals: no click).

**1.1 Echo across the Gulf** (`EchoBus`): image sources off the face planes x_A 215.3 and x_B 815.3.

| Tap | Delay | Gain | Low-pass |
|---|---|---|---|
| A1 (face A) | 2 d_A / 343 | 1 / (1 + d_A / 150) | 6 kHz / (1 + L / 250), L = 2 d |
| B1 (face B) | 2 d_B / 343 | 1 / (1 + d_B / 150) | same |
| AB2 (face to face) | 1 200 / 343 = 3.50 s | 1 / (1 + 600 / 150) x 0.7 | 1.03 kHz |

- Measured impulse arrivals (harness `dread`, `DreadProbe.EchoImpulse`): tongue tip x 375: 0.9312 / 2.5674 / 3.4986 s (DREAD
  0.93 / 2.57); bridge middle x 535: 1.8642 / 1.6344 / 3.4986 s (DREAD 1.87 / 1.63); x 230 (the mouth): 0.0857 / 3.4129 /
  3.4986 s (B1 and AB2 merge into DREAD's "one long echo" of 3.5 s). At mid-Gulf the arrivals come at 1.63, 1.86 and 3.50 s:
  spaced by 600 m = 1.75 s, as DREAD 1.1 says; elsewhere the image model gives the physical spacing (proposed decision, AU-12).
- Taps under 30 ms merge into the room (smootherstep 0.03-0.15 s on the gain): no comb filter at a face. The bus is heard inside
  the Gulf only (smootherstep over 40 m at each face: weight 0.5 at the mouth).
- One mono float line of 2^18 samples (5.46 s at 48 kHz, 1 MB, preallocated). The taps glide per block with the listener (a
  walker at 2.5 m/s moves a tap by 0.015 s per s: the echo's own Doppler). A jump over 50 ms fades the tap out over 20 ms,
  moves the head after one silent block and fades it back in (`EchoBus_GlidesWithTheWalkerWithoutClicks`).
- Fed by the horn, the strikes, the groans (x 0.8), the creak, the hull (x 0.4) and the rumbles (x 0.3); not by the chime.

**1.1 Sources.** Horn (`HornVoice`): a PolyBLEP saw fifth with a 30-cent scoop and a 0.31 Hz vibrato, LP 560 Hz, 7 s, at
s 440 (Eb2 + Bb2, 1.6 km north down the rift) and s 740 (F2 + C3, 1.8 km south; fix wave C08: s 700 sat on T4's meet): chord
tones of C3 and of C5 Dm11.
Metal strikes (`StrikeVoice`): six inharmonic modes (1, 2.32, 4.25, 6.63, 9.38, 12.53 x 170-300 Hz, T60 4.0-0.8 s) inside a
face, every 25-40 s from s 405 to 990 (six since the fix wave: s 489, 578, 673, 785, 858, 922).

**1.2 Sub-bass swells** (`SubSwell`) under KS04 (s 250, F1 43.65 Hz), KS09 (520, Eb1 38.89), KS10 (548, F1), KS13 (718, D1
36.71) and KS17 (1 296, G1 49.00): chord tones of the pad. Partials 1-5 up to 200 Hz (amplitudes 1, 0.7, 0.55, 0.45, 0.35),
each a centre voice and two side voices detuned by +-0.16 % (a slow beating, decorrelated left and right), plus a brown
pressure bed (LP 55 Hz, 35 %). Envelope: smootherstep rise over 20 m (8 s at 2.5 m/s), hold 2.5 m (1 s), decay over 20 m (8 s).
Peaks -38 / -36 / -35 / -37 / -36 dBFS through the master's 20 Hz DC blocker. Rendered over +-3 s (d05): KS04 -36.7, KS09 -35.0,
KS10 -35.3, KS13 -35.9, KS17 -36.6 dBFS.

**1.3 Structural groans** (`GroanVoice`): a resonant comb at 34-64 Hz with a 6-14 % downward bend over 4-6.5 s and a damped
loop, excited by band noise; stick-slip creak grains (9-22 Hz, band 250-480 Hz) on top; a low boom on a third of them. Every
20-60 s from s 120 (eight since the fix wave: s 163, 239, 365, 531, 873, 983, 1 131, 1 277; beat 4's train stretch s 587-831
leaves no slot 18 m clear of the meets), placed on the nearest large mass
(`NearestMass`: the canyon walls 24-40 m off the walk in beats 2, 5 and 6; the Gulf faces or the bridge truss under the deck in
beats 3-4; the spire in beat 7), heard after the flight time, attenuated by 8 log10(1 + d / 100) dB. A slot within 8-18 m of
the horns, the creak, the chime, a swell peak, a drop or a rumble moves on in 8 m steps. S7 (the koi) and the last 60 m stay
free.

**1.4 Silence drops** (fix wave C07): `DreadScore.DropShape(s)` (0..1) scales each layer's own depth (`DropGainAt`): R, D, H, T,
the X beds and the X passes -21 dB, the koi -21 dB, the pad -15 dB after its own FDN (its tail goes too), the wind -4 dB (the
breath of the place); the footsteps and E stay. The shape starts 5 m before KS09 and KS17 (2.0 s), is at full depth 1.5 m later
(0.6 s) and returns over 3.75 m (1.5 s) after the key shot, through the 50 ms ramps. Rendered (f08): everything but E falls
-11.0 / -14.8 dB at 2.5 m/s (K-weighted -11.2 / -15.1 LU, laptop model -10.6 / -14.8 dB) and -11.1 / -14.4 dB at 1.5 m/s; the
swell is then the loudest element (E re rest +8.1 / +7.4 dB). Before the fix: -1.19 / -0.83 dB (d05).

**2-6 cues.**
- Hull (`HullDrone`, 2.1): the course is DR's `HullCourse` (D-038 / D-040: one source of truth): `DreadScore.HullCentreZ(tau)` =
  `HullCourse.SternZ(tau)` + 900, launch s 430 (`VehicleClock`), 18 m/s after the 10 s smooth start (`HullVelocity`), x 515.3,
  belly y 432, 1 800 m; with DR's C01 re-park (centre -660 at launch) the centre is abeam z 195 at tau 52.5 s (s 561.25 at
  2.5 m/s, `HullAbeamTau` solved from the course). Body = 13.75 Hz x 1..12 (the fundamental infrasonic, LP 120 Hz) as a line
  source (subtended angle); stern engines = 27.5 Hz x 1..8 + an exhaust band at 150 Hz + a 0.55 Hz throb as a point source with
  the Doppler c / (c + v_r) of the course's speed (+5 % far ahead, 1.00 overhead, -5 % receding); the small-speaker harmonics
  9 / 12 / 8 x 27.5 Hz (B3 E4 A3) at the engines' Doppler on the whole hull's gain, 3 dB under it (C09). Peak -42 dBFS; it ends
  with beat 4 (s 860-880). Rendered -38.5 dBFS over s 480-630 (f08, DR's C01 course).
- Panel (`CreakVoice`, 3.2): accelerating friction grains (18 -> 120 Hz) through four resonators bending down 12 %, 4 s, at s 470
  from DR's `PanelCourse` centre at the tear (814.85, 150, 800), heard 2.38 s later (818 m). No impact.
- Elevators and gantries (`MachineHum`, 2.2; integration round, AU-28 and DR's request): one hum per shaft of DR's 8 (on the
  loudest of its 4 cabins, times the cabin light's ramp at the hidden ends, so a wrap never jumps; the traffic clock) and one per
  gantry (2, at `DreadBuilder.GantryPos`, the scene clock), all from `DreadCabinCourse` (Beats/Dread/DreadCourse.cs; Mesh-free, so
  the harness compiles it); 108-129 Hz cabins, 50 Hz gantries; -48 / -46 dBFS within 20 m, (20 / d)^1.2 beyond. DR's shafts stand
  135 m or more from the walk, so the hums are far and faint (AudioDreadTests logs the loudest on the walk); the audio's own
  boulevard-tower shafts are gone (no cabin rides there in the picture).
- Bridge (`BridgeGust`, 4.3): deck creaks at the extremes of TR's sway sin(2 pi 0.12 t), scaled by its amplitude
  (`SwayAmplitude`, the mirror of Beats/Beat04_Skybridge/BridgeSway.cs), and a cable whistle at 1.05-1.6 kHz with the heard
  wind gust; -49 dBFS at full gust; beat 4 only.
- Tram (`ChimeVoice`, 6): D5 then Bb4 bell tones 0.55 s apart from the terminus (210, 2.5, 227) at s 226.25 (BV's door close),
  1.5 s before the launch at s 230; -40 dBFS (C53: +3 dB), 0.5 s flight; T and the X beds duck 9 dB around it
  (`ChimeDuckShape`: from 0.3 s before ChimeS, fall 0.6 s, held 3.5 s, back over 1.5 s).
- Lightning (`RumbleVoice`, 5.4): brown noise LP 85 Hz with a rolling 220 Hz band, rise 1 s, decay 2.2 s, 7 s long, 3.0 s after
  each glow at s 205, 345, 553 (the hull), 790, 1 140 and 1 420 (at most one per 25 s, none in beats 1 and 5); -37 dBFS; no crack.

**Levels and gates** (harness d05 with `--ending`, whole walk; noE = the same render without E):

| Gate | Limit | 2.5 m/s | noE 2.5 | 1.5 m/s | noE 1.5 |
|---|---|---|---|---|---|
| beat medians - Sum (dB) | +-1.5 | -0.01 +0.16 +0.22 +0.38 +0.25 +0.04 +0.02 | -0.01 +0.13 -0.23 +0.27 +0.13 +0.00 -0.17 | -0.01 -0.06 +0.53 -0.17 -0.10 +0.03 -0.06 | -0.01 -0.13 +0.22 -0.27 -0.11 +0.01 -0.18 |
| true peak (dBTP) | <= -1 | -4.84 | -4.91 | -5.50 | -5.49 |
| max sample step | <= 0.25 | 0.147 | 0.147 | 0.138 | 0.138 |
| 20 ms envelope step, worst chord change | < 0.05 | 0.0427 (C4->C5: horn, trains, KS13 swell) | 0.037 | 0.0337 | 0.0330 |
| DC | < 0.001 | 0.000000 | | 0.000000 | |

- The EditMode level stretches (harness `gates`, 24 s per beat): -0.20 +0.19 +0.47 +0.81 -0.12 +0.00 -0.36 dB at 2.5 m/s;
  -0.35 -0.20 +0.22 -0.02 -0.13 +0.00 -0.17 dB at 1.5.
- Tools/qa/audio_analysis.py: PASS at both speeds (QA/reports/audio_AU-walk25-d05.md, audio_AU-walk15-d05.md).
- DSP cost of E: 2-6 % of one core (harness `dreadcost`, on a loaded machine).
- Chord chroma of the mix (analyze.py): 0.47-0.84 on the chord's pitch classes (wave A 0.48-0.85): the swells and the horns
  are chord tones.

**Audibility** (Tools/audio/dread_analysis.py on d05: E against the rest of the same render, per cue window; JSON in
QA/reports/audio_dread_AU-walk25-d05.json and audio_dread_AU-walk15-d05.json):

| Cue (2.5 m/s) | E dBFS | E - rest | laptop, best 1/3 octave | AAC 128 kept | Opus 128 kept | E - pad, pad octaves |
|---|---|---|---|---|---|---|
| horn s 440 | -40.1 | -8.9 | -4.0 (252 Hz) | 0.98 | 0.99 | -1.9 |
| creak s 470 | -42.3 | -10.6 | -2.6 (504 Hz) | 0.98 | 1.00 | -0.7 |
| groans (10) | -37.0 .. -43.3 | -6.0 .. -19.6 | -4.1 .. -21.6 | 0.96-0.99 | 0.98-1.00 | -4.5 .. -17.0 |
| strikes (7) | -40.1 .. -44.5 | -9.3 .. -20.5 | -5.5 .. -15.6 | 0.97-0.99 | 0.99-1.01 | -6.4 .. -20.4 |
| swells KS04-KS17 | -35.0 .. -36.7 | -2.5 .. -8.7 | -8.5 .. -24.5 (200 Hz) | 0.98-0.99 | 0.99 | -8.9 .. -23.7 |
| drops KS09 / KS17 | | the rest's 2-16 kHz: -10.3 / -13.8 dB | | | | |
| hull (60 s) | -39.1 | -10.5 | -14.8 | 0.99 | 0.99 | -8.8 |
| rumbles (6) | -41.4 .. -47.5 | -11.1 .. -18.5 | -13.6 .. -24.0 | 0.98-0.99 | 0.96-0.98 | -12.7 .. -21.8 |
| chime s 226 | -48.0 | -17.4 | -12.7 (504 Hz) | 0.98 | 0.99 | -4.2 |

- "Never masks the pad": in every cue window E stays under the pad in each of the pad's own octaves (250 Hz and up, within
  15 dB of its strongest). The closest are the creak (-0.7 dB at 500 Hz for 4 s) and the horn (-1.9 dB).
- "Audible after YouTube's encoder": the AAC and Opus 128 kbit/s round trips keep 96-101 % of E's projection in every window.
- "Audible on laptop speakers" (a 200 Hz 4th-order high-pass): the horn, the creak, and most groans and strikes come within
  2.6-10 dB of the rest in their best third octave (partially masked: heard as events). The swells and the hull are speaker
  and headphone effects (their partials end at 200 Hz); on a laptop the moment is carried by the drop (the rain band falls
  10-14 dB). A human listen on a laptop is still open (AU-17).

**Hooks** (since the fix wave the picture's code is the source where it exists): the hull and the panel come from DR's
`HullCourse` / `PanelCourse` (Beats/Dread/DreadCourse.cs: since the integration round the drone uses `HullCourse.CentreZ` and
`BellyY(tau)`, so it sinks out of the deck with the picture over tau 0-24, and the creak sounds from `PanelCourse.Centre(0)`); the
machine hums sit on DR's 32 cabins and 2 gantries (`DreadCabinCourse`, AU-28 closed); AT's sheet-lightning glows at `DreadScore.LightningS`; BV's
tram doors close with the chime at `DreadScore.ChimeS` (s 226.25); TR's sway is mirrored in `DreadScore.SwayAmplitude`.

**Fix wave (2026-11-21, panel p4_01: C07, C08, C09, C49-C53).** Final renders f08 (harness with DR's C01 DreadCourse from the
abyss copy; whole walk with `--ending`; JSON and tables in QA/reports/audio_dread_AU-walk25-f08.* / -walk15-f08.*, the walks'
gates in audio_walk_AU-walk25-f08.txt / -walk15-f08.txt, QA's checker in audio_AU-walk25-f08.md / -walk15-f08.md, the hum and the
pad in audio_AU-f08-hum-pad.txt). Tools/audio/dread_analysis.py now prints a gate list: 62 / 62 pass at both speeds.

| Gate | Limit | 2.5 m/s | 1.5 m/s |
|---|---|---|---|
| beat medians - Sum (dB) | +-1.5 | -0.03 +0.18 +0.18 +0.47 +0.23 -0.02 -0.14 | +0.01 -0.13 +0.61 -0.17 +0.08 +0.01 -0.02 |
| true peak (dBTP) | <= -1 | -7.62 | -8.05 |
| max sample step | <= 0.25 | 0.144 | 0.134 |
| 20 ms envelope step, worst chord change | < 0.05 | 0.0277 (C4->C5; was 0.0427) | 0.0257 |
| integrated loudness (LUFS) | info | -27.26 | -27.55 |
| C07 rest in the drops (dB): KS09 / KS17 | <= -10 | -11.0 / -14.8 | -11.1 / -14.4 |
| C07 rest K-weighted (LU) / laptop (dB) | <= -8 | -11.2 / -15.1; -10.6 / -14.8 | -12.1 / -15.6; -10.8 / -14.2 |
| C07 swell re rest in the drop (dB) | > 0 | +8.1 / +7.4 | +7.2 / +7.5 |
| C08 horn 740 / strike 784.8 E re rest (dB) | siblings | -10.6 / -11.7 (strikes -8.5..-14.6) | -12.7 / -12.4 |
| C08 closest horn / strike / groan to a meet | >= 15 m | 19.1 m (horn 740 vs T2) | |
| C09 swells, best laptop third octave re rest (dB) | >= -3 | KS04 -1.5, KS09 +3.8, KS10 +0.8, KS13 -0.6, KS17 +9.6 | -2.7 +5.8 +0.5 -0.9 +11.8 |
| C09 hull, best laptop third octave (dB) | >= -3 | -2.5 (was -14.8) | -1.2 |
| C09 koi in S7, laptop band re rest (dB) | >= -6 | -5.5 (phone -7.3; was -15.1 / -29.7) | -4.5 |
| D-038 (10) E under the pad in its octaves (swells; koi) | < 0 | -1.3..-2.8; koi -2.7 | -0.1..-5.8; koi -0.5 |
| C53 chime, best third octave re rest (dB) | about -3 | -3.3 at 504 Hz (was -12.7); under the pad -0.2 | -5.3 |
| AAC / Opus 128 kept of E (every cue) | | 0.97-0.99 / 0.95-1.00 | 0.97-0.99 / 0.95-1.01 |
| C49 H below 30 Hz (share of H) | < 15 % | 4.1-11.5 % per beat (was 20-56 %) | |
| C51 pad L/R correlation, mono loss | >= +0.1, <= 2 dB | +0.37..+0.78, -0.51..-1.64 dB (beat 5 was -0.16 / -3.74) | +0.38..+0.78, -0.51..-1.60 |
| loudest 1 s windows (train passes) | | -22.8..-23.3 dBFS, +6.3 over the beat-4 median (was +8.5..+9.0) | |

- The D-038 (10) check skips the drop spans (the pad is ducked 15 dB there by design and the swell leads: C07); outside them
  the closest margins are KS09 -0.1 and KS10 -0.3 dB at 1.5 m/s and the chime -0.2 dB at 2.5 m/s.
- C08's acceptance names "the groan 597.6 or its moved slot": there is none. With every horn, strike and groan 18 m clear of the
  meets (tram 272.0, T1 602.0, T5 624.7, T3 653.6, T4 699.3, T6 715.8, T2 759.1, T7 816.3), s 587-831 has no free slot; the drone,
  the swells KS10 / KS13, two strikes and horn 2 carry the train stretch.
- The passes are 3 dB down (TubeTrain PeakDb -24, whoosh -27, thump -33; C08's option, AU-06); the beat-4 median moved
  +0.51 -> +0.47 dB and the walk's true peak -4.92 -> -7.62 dBTP (f07 -> f08).
- The pad FDN's wet side is narrowed to 0.35 (C51; `NocMixer.PadWetWidth`), its power kept, and the per-chord level shifts went
  back into `PadChordTrimDb` (QA/audio-trims.json): the pad per chord is 0.00 dB from before.
- The koi's partials (14.3 lists 123.6, 164.8, 247.2 Hz at -6, -10, -14 dB) are now 3, 4, 6, 8, 9, 12 x E1 at -6, -10, -8, -10,
  -10, -9 dB with a 0.13 Hz +-0.6 % vibrato on the upper four: an ADR request against the 14.3 list (DEVELOPMENT line proposed
  in the report).
- C52: the player logs `[NOC] audio heartbeat ...` every 30 s after a 10 s intro baseline (the window's longest callback, the
  callbacks over 50 % and 100 % of their buffer's period) and `[NOC] audio summary ...` at quit ("audio overruns after the intro
  = N"); the 4K full-length cinematic dump is the lead's (a long GPU job).

## 12. The Ark (D-043, Logs/ark/ARK.md; built 2026-11-21 evening; replaces the 2.1 hull's drone)

The 5 000 m ship comes down the rift from the far north haze (DR's direction note: every beat-3 / beat-4 gaze looks north), bow
(the leading, south end) first, on x 515.3 with its belly at y 330, 328 m over the walker; DR's course launches it at s 380, its
bow is over z 195 at tau 86.4 (s 596 at 2.5 m/s, s 509.6 at 1.5) and its stern at tau 270.3 (s 1 055.7 at 2.5, 785 at 1.5).
`DreadScore` reads every Ark number through the alias `ArkC` (DreadScore.cs, top): DR's `HullCourse` of the abyss copy (20:41)
mirrored in `ArkCourseMirror` until the merge, when the alias becomes `using ArkC = Megastructure.HullCourse;` and the mirror is
deleted (`AudioDreadTests.Ark_MirrorIsDrsCourse_D043` holds the numbers). `ArkDir` -1: the motion is toward -z.

**What the walker hears.** In beat 3 the ship's first call (s 440, Eb2 + Bb2 over Ebmaj7#11) arrives from its bow 2.1 km up the rift
after 6.4 s of flight, and the pressure starts to rise out of the haze (the drone -56 dBFS at s 440, -37 at the bow's pass). Nine
seconds before the bow is over the walker the world falls silent: rain, drips, the city hum, the traffic wash, the X beds and the
passes -21 dB, the pad -18, the wind -12 (a breath stays), over 2 s; only the footsteps, the breath of the wind and the Ark remain.
The bow slides over in that silence with its long Doppler (the bow wave's 27.5 Hz harmonics 2-6 and its air band glide from about
+8 % to -7 % as it passes), the hull's first groan rolls down from the belly 1.8 s after the bow is overhead, and the world comes
back over 5 s from 2 s after it (T1's pass at s 602 rises with the return). Under the belly the city stays hushed: the hum and the
traffic wash -8 dB, the X beds and the train passes -6 dB (`ArkCover`, 400 m soft edges, the beam's half 220 m plus 250 m), the
Ark's body pressure is overhead (13.75 Hz x 1..12 with a detuned side voice and a brown bed, breathing over 22 s; its laptop layer
A3 E4 A4), four engine pods pass over one by one with their own Doppler glides, and the hull groans every 22-32 s from overhead
(7 groans, 22-30 Hz combs with creak grains, 7-9.5 s). The second call (s 740, F2 + C3 over Dm11) comes from the bow 1.6 km south,
the third (s 1 090, D2 + A2 over the Dmaj9 -> Bm11 change) from the stern going away; as the stern passes over in the garden the
cover lifts and the city comes back.

| Part | Where | Level |
|---|---|---|
| `ArkDrone` body (line source: the share of the zenith half-circle the hull fills, ^0.8) | 13.75 Hz x 1..12 (LP 120), side voice +0.21 % on 2-6, brown bed LP 40 Hz, breath 0.045 Hz +-15 % | `ArkPeakDb` -38 dBFS x 0.8; `ArkGardenTrimDb` -2.5 in beat 5 |
| body's small-speaker layer | 8 / 12 / 16 x 27.5 Hz (A3 E4 A4), side voice +0.35 % | `ArkBodyUpperDb` -2 re the body, x (1 - 0.8 swell envelope) |
| bow wave (point source at the bow, y 390) | 27.5 Hz x 2..6, air band 70 Hz; Doppler of the course's speed | `ArkBowGain` 1.27 / (1 + (r / 420)^2) |
| engine pods (4, 1 150 / 2 350 / 3 550 / 4 820 m behind the bow, x +-90) | 27.5 Hz x 1..6, exhaust band, throb 0.37-0.53 Hz, distance LP 2.4 kHz / (1 + r / 150); 9 / 12 / 8 x 27.5 Hz layer | 0.7 x {0.75, 0.8, 0.85, 1.25} / (1 + (r / 330)^2) |
| the silence (`ArkDropShape`, keyed to the bow's seconds to the listener) | fall -9..-7 s, hold to +2 s, back by +7 s | R D H T X passes K -21, pad -18, wind -12 dB |
| cover (`ArkCover`) | under the belly | H T -8 dB, X beds and passes -6 dB |
| calls (`HornS` 440 / 740 / 1 090 at 2.5 m/s, fired on the Ark's clock `ArkCallTau` 24 / 144 / 284 s; `HornPart` bow / bow / stern) | position, flight time at fire time; played at the heard pitch (chord tones at both speeds) | -41.5 / -41.5 / -44 dBFS at 1 700 m, +-10 log10 of the distance (-8..+3), none beyond 3.5 km |
| groans (`ArkGroanTau`: on the Ark's clock, the first 0.8 s after the bow is over z 195) | the belly point over the listener | `ArkGroanDb` -35 dBFS overhead |

Face strikes and face groans keep out of the Ark's stretch (s 565.5-1 075.7 at 2.5 m/s, `ArkQuietS0` / `ArkQuietS1`); no glow or
rumble falls in the silence (AudioDreadTests checks the schedule).

**Measured** (harness a06 with `--ending`, whole walk; QA/reports/audio_walk_AU-ark-walk25-a06.txt / -walk15-a06.txt,
audio_dread_AU-ark-walk25-a06.* / -walk15-a06.*, audio_AU-ark-walk25-a06.md / -walk15-a06.md):

| Gate | Limit | 2.5 m/s | 1.5 m/s |
|---|---|---|---|
| beat medians - Sum (dB) | +-1.5 | -0.03 +0.18 -0.01 -0.12 +0.67 -0.02 -0.12 | +0.01 -0.14 +0.32 -0.40 -0.01 +0.00 -0.09 |
| EditMode level stretches (harness `gates`) | +-1.5 | -0.25 +0.02 +0.08 -0.34 +0.88 +0.02 -0.51 | -0.38 -0.45 -0.02 -0.68 +0.66 +0.02 -0.30 |
| true peak (dBTP) / max sample step | <= -1 / <= 0.25 | -11.42 / 0.142 | -9.10 / 0.134 |
| the Ark's silence: the rest (full / K / laptop) | <= -10 dB | -13.6 dB / -15.2 LU / -14.2 dB | -16.1 / -16.3 / -15.2 (one silence with KS09's) |
| the Ark the loudest element in its silence (E re rest) | > 0 | +5.9 dB | +9.5 dB |
| under the belly: laptop best third octave re rest | >= -3 dB | +1.1 dB (200 Hz) | -5.5 dB (the bridge's train passes; deviation) |
| D-038 (10) E under the pad in its octaves (Ark windows) | < 0 | belly -4.9, groans -3.5..-6.5, calls -3.8..-8.2 | belly -5.0 |
| dread_analysis gates | all | 60 / 60 | 59 / 60 |
| AAC / Opus 128 kept of E (Ark windows) | | 0.98-0.99 / 0.99-1.00 | |

- At 1.5 m/s the bow passes at s 509.6, so the Ark's silence and KS09's drop make one silence (s 496-524); dread_analysis then
  measures KS09's drop against the Ark's reference (bow -18..-10 s).
- Spectrograms looked at: QA/reports/audio_AU-ark-a06-bowpass-spec.png (the rain band leaves over 2 s and comes back over 5 s
  with no vertical line; the footsteps alone in the hole; the Ark at -38 dBFS) and audio_AU-ark-a06-E-low-spec.png (E alone,
  20-500 Hz, s 405-680: the pods' glides, the groans, the swells).
- The calls fire on the Ark's clock, so at 1.5 m/s the ship still calls three times at the same points of its course (s 416 /
  596 / 806, from about 2.1 / 1.6 / 0.6 km; a03 had the second and third from 4-5 km): E -41.2 / -35.9 / -40.2 dBFS, under the pad
  -3.4 / -2.3 / -5.9 dB (a06).
- In the built player (AU-b01, -autotest -cinematic -beat 4, 44 s -audio-dump from about s 585; QA/reports/AU-ark-walkdump.wav,
  audio_AU-ark-walkdump-spec.png): the recording opens in the silence (no rain band, the footsteps, the 220 Hz line and the low
  stack), the world returns at t 7-12 s with T1's pass, then the hushed crossing; dump -29.97 vs offline -30.26 dBFS (+0.28 dB).
  Standing at s 590 (AU-ark-dump01): -30.20 vs -29.72 dBFS (-0.48 dB, <= 1.0: PASS), 1 s windows -32.3..-29.2, max step 0.099,
  no zero run longer than 1 sample.
