# R5: pad, rain and city audio for Nocturne (14.3, 14.7 H8)

Researcher R5 (read-only), 2026-11-13. Scope: MASTER_PROMPT 14.3 (sound bible), 14.7 H8 (the pad and the mix), 11.9 (audio verification), Part 3.2 item 14 (audio lessons). This report builds on Thread's `docs/research/R5_audio.md` and `Assets/_Game/Scripts/Runtime/Audio/*` and on The Ring's `docs/research/05_procedural_audio.md` and `AudioDsp.cs`. It covers only what is new for Nocturne.

**Evidence level.** Every code block below was compiled with `LangVersion 9` (Unity's C# level) in a .NET 10 console harness and run there, except where a block is marked "sketch". The numbers in the tables come from that harness, or from numpy scripts where a table says so. The harness lives in this session's scratchpad and was not copied into the project, so the code blocks here are the deliverable. No Unity code was run.

---

## 0. Summary: the decisions for the lead

1. **One generator, no listener filter.** Run all of the audio in a single `NocturneDsp` (pure C#). It is called from `OnAudioFilterRead` on a clip-less `AudioSource` (Part 3.2 item 14) and from the EditMode `OfflineAudioRenderer`. **Synthesize footsteps inside the DSP.** Do not use footstep AudioSources: with a generator AudioSource they would bypass the reverb and the limiter. The FDN reverbs and the limiter sit at the end of the same chain, so the limiter is post-reverb by construction.
2. **The output is bit-identical for any callback size.** Run control updates at absolute sample indices k·32, carried across calls. Drive every stochastic event (drips, patter, footstep variation) from a counter-based hash, never from RNG state advanced per block. Measured: 20 s renders with buffers of 1024, 256, 1000, 441 and {37, 2048, 512, 1, 333} differ by exactly 0. The audio thread extrapolates s from a published snapshot `(s, v, t)`; offline, the snapshot is the exact motion model.
3. **Pad oscillators.** Use a PolyBLEP saw (−4 cents) and a 2-point polyBLAMP triangle (+4 cents), both stateless, with double phases and an independent 0.03 Hz drift of ±2 cents each. Measured at 392 Hz, the worst alias below 2 kHz is −41.5 dB for the naive saw and −99 dB with PolyBLEP; for the triangle it is −83 dB naive and −100 dB with polyBLAMP. Do not build the triangle with a leaky integrator: it adds a start-phase DC offset and a frequency-dependent gain.
4. **Voice leading needs one fix.** "Matched by pitch order" contradicts "common tones stay on one voice" in **8 of the 10 chord changes** of 14.3. For example, C1 to C2 keeps 53 and 60 but moves both to other slots under pitch order. Rule: pin common tones to their slot first, then match the remaining notes by pitch order. Consecutive notes of a slot alternate between two banks (`bank = run & 1`), so the plan is a pure function of s. The table is in 3.5.
5. **Crossfade with equal power where notes differ.** The spec's sum-to-1 chord weights `g_c(s)` stay as the weights, but slots whose notes differ use `cos/sin(π/2·g)`. Measured: the linear crossfade dips the pad by about 2 dB in the middle of C2 to C3; equal power has no dip. Held common tones keep one oscillator, so there is neither a click nor a phase cancellation. **C10 needs a 14 m half-window** (`[1472, 1500]`). With the default 40 m window it would still be at 97 % when the walk stops at s 1500, and it would hang there for the whole ending.
6. **Limiter.** Port The Ring's verified `DspLimiter` (Signalsmith design) with ceiling −1.5 dBFS, a tanh knee from −4.5 dBFS, 5 ms look-ahead and 80 ms release, **plus a true-peak detector** built on the ITU-R BS.1770-4 Annex 2 4× FIR. That adds 6 samples of latency and 96 MAC per stereo frame. Measured on white bursts at +12 dB: the sample-peak version gives −1.50 dBFS sample peak but **+0.49 dBTP**; the TP-aware version gives **−1.50 dBTP**. Below the knee both are a pure delay, bit-exact.
7. **The loudness table checks out, but its measurement semantics do not.** All seven Sum values equal the power sums of their rows within 0.04 dB. However, the gate is the *median* of 3 s windows, and the median ignores transient layers:
   - **Beats 2 and 4:** X (tram, trains) is 34 % and 29 % of the power. If X is only the passes, the medians land about 1.8 and 1.5 dB low. Make X a continuous bed (rail and tube hum) in beats 2 and 4, with the passes on top.
   - **Beat 7:** with the koi distance law, K is −33 only inside S7, which the gate excludes. The modelled median is −30.35 dBFS, 1.45 dB under target. Log a trim of P +1 dB in beat 7 outside S7, which gives −29.65.
   Add a **score-model test** (expected median from the gain curves) besides the render test.
8. **Swells without Doppler.** Model the tram and the trains as incoherent finite line sources: `I ∝ (atan(x2/d) − atan(x1/d))/d`. Take brightness (LP 900 Hz to 4 kHz) and stereo width from the subtended angle, and use the same `x(τ)` as the picture. Tables are in 4.6.
9. **Calibrate every layer to unit RMS at construction**, from impulse energy for LTI noise chains (Thread's method) or analytically for tones, including the 20 Hz DC-blocker loss. The DC blocker costs −0.91 dB at the koi's 41.2 Hz, −0.63 dB at 50 Hz and −0.30 dB at 73.4 Hz. Then gain g gives 20·log10(g) dBFS. Measured: city hum within 0.05 dB of unit.

Report path: `W:\YouTube\Megastructures\6 Megastructures\6 Megastructures\docs\research\R5_pad_rain_audio.md`

---

## 1. What is already settled (reuse it, do not re-research)

| Item | Where it is settled | Nocturne delta |
|---|---|---|
| Simper/Cytomic trapezoidal SVF (Chamberlin diverges above about 8.3 kHz at 48 kHz, Q 0.707) | Thread R5 §3; `ThreadDsp.cs:78-136` | use a double-state version (`SvfD` below) for the long-tau smoothers and the pad |
| 8-line Householder FDN (primes 1427-3539 × size, RT60 gains, unity-DC damping, two-tap pre-delay crossfade) | Thread `ThreadDsp.cs:187-331`, R5 §4 | **two instances**: environment (blended per 14.3, max pre-delay **0.30 s** for the Edge's 250 ms) and pad (6 s, 40 ms, **size 2.5** because the mode-density rule `ΣM ≥ 0.15·t60·fs` needs 43 200 samples at 6 s, and size 2.2 gives 42 894). Set the FDN's internal blend time to 0.5 s, because the s-keyed 40 m blends are already smooth (Thread used 8 s) |
| Kellet pink noise (UnitRms 1/1.755), xorshift32 streams, sine table, WAV writer, BS.1770 K-weighting, `WalkLevelMeter`, `AudioCapture` (`-audio-dump`) | Thread R5 §3, §5; `OfflineAudioRenderer.cs` | keep them; the meter gains a 3 s window median and S-window exclusion |
| Unity callback facts (audio thread, interleaved, first filter on a clip-less source is a generator, `Play()` after adding components, `outputSampleRate` may be 44.1 kHz, `OnAudioConfigurationChanged` rebuild, `runInBackground`, no `Stop()` for generators) | Thread R5 §1-2; Ring 05 F4-F11 | generator on an AudioSource, as the Ring did (Part 3.2 item 14: "never on the listener") |
| Look-ahead limiter design and test | Ring `AudioDsp.cs:253-322` (`DspLimiter`), `AudioTests.cs:178` | ceiling −1.5, plus the TP-aware detector (section 5) |
| Footstep recipe pattern (variants rendered once, exact zero ends, peak normalised, T60 decays) | Thread `FootstepBank.cs` | new surfaces (4.7); voices mixed **inside** the DSP, as Thread's `OfflineAudioRenderer.cs:154-206` already does offline |
| Engine-side 4 KB per-callback allocation (Unity marshalling) | Thread DEVELOPMENT D-033 | unavoidable; the DSP itself allocates 0 bytes (measured: 40 B in 60 s, all of it the Stopwatch) |

Unity 6000.6.0f1 ground truth, checked locally (`Editor\Data\Managed\UnityEngine\UnityEngine.AudioModule.xml`): `AudioSettings.dspTime` (line 2336), `outputSampleRate` "Get the mixer's current output rate" (2341), `GetDSPBufferSize` (2365), `OnAudioConfigurationChanged` / `Reset` (2424-2432), `AudioSource.bypassListenerEffects` (2455), `AudioSource.generator` (2480, the new scriptable generator path, not needed), `AudioSource.Play()` (2694). The local `ScriptReference/MonoBehaviour.OnAudioFilterRead.html` says: interleaved data, "every ~20ms", the audio thread, and "If this is the first filter in the chain and a clip isn't attached to the audio source, this filter will be played as the audio source". Project facts: `ProjectSettings/AudioManager.asset` has `m_SampleRate: 0` (device default), `m_DSPBufferSize: 1024`, `Default Speaker Mode: 2`; `ProjectSettings.asset` has `runInBackground: 0`.

---

## 2. Architecture: one DSP for both the player and the tests

```
main thread (ProgressDirector order 500 -> AudioDirector order 700)
   publishes AudioSnapshot { s, v (m/s, smoothed), tSnap (DSP sample index), cameraYaw, tramLaunchSample,
                             trainLaunchSample[7], goalTriggerSample, pause, masterFade, flags }
        |  double-buffer mailbox (Thread ThreadSynth.Publish) or a seqlock (Ring 05 §7.1)
        v
NocturneSynth : MonoBehaviour  (on "NocturneAudio" GameObject: AudioSource, clip null, loop, 2D, priority 0,
                                bypassEffects/ListenerEffects/ReverbZones true; components added, then Play())
   OnAudioFilterRead(data, ch) -> NocturneDsp.Process(data, ch)   (overwrites data; the input is silence)

NocturneDsp.Process: for every frame
   if (inBlock == 0) Control():           every 32 samples at absolute index n = 32 k
        s = snap.s + snap.v * (n - snap.tSnap) / fs   (clamped to +0.25 s of extrapolation)
        NocturneScore.Evaluate(s, t) -> layer targets (pure functions of s and t)
        drips / patter / steps / passes scheduled by hashed event indices
   synthesize layers -> sends (env FDN, pad FDN) -> sum -> DC 20 Hz -> master gain -> TP-aware limiter -> data
```

The EditMode `OfflineAudioRenderer` builds the same `NocturneDsp`, sets the snapshot each 1024-frame block from the exact motion model (constant speed, the 0.5 s start ramp, the stop ramp `v0(1 − smoothstep(0, T, t))` of 14.11 with T = 6/v0, and τ for the tram and trains), and calls `Process`. Because the DSP extrapolates s itself at control rate, the offline render does not depend on the block size (measured exactly 0 difference, section 7).

### Code: the partition-invariant driver (compiled)

```csharp
public sealed class NocDspSketch
{
    public const int Block = 32;
    readonly double m_fs; long m_n; int m_inBlock;
    public double S0, Speed = 2.5, T0;      // snapshot: progress S0 at audio time T0 (offline: exact motion)
    public readonly PadEngine Pad; public readonly RainBed Rain; public readonly DripField Drips;
    DcBlockD m_dcL, m_dcR; public readonly LookaheadLimiter Limiter = new LookaheadLimiter();
    public double PadGain = NocMath.Db(-34.0), RainGain = NocMath.Db(-40.0), DripGain = NocMath.Db(-46.0);

    public NocDspSketch(double fs, uint seed, DripField.Emitter[] emitters)
    {
        m_fs = fs; Pad = new PadEngine(fs, Block, seed); Rain = new RainBed(fs, seed + 11u); Drips = new DripField(fs, emitters);
        m_dcL.Init(20.0, fs); m_dcR.Init(20.0, fs);
        Limiter.Configure(fs, -4.5, -1.5, 5.0, 80.0);          // true-peak detector on by default
    }

    public void Process(float[] data, int channels)
    {
        int frames = data.Length / channels;
        for (int f = 0; f < frames; f++)
        {
            if (m_inBlock == 0)                                 // absolute sample index n = 32 k, whatever the callback size
            {
                double t = m_n / m_fs, s = S0 + Speed * (t - T0);
                Pad.Control(s, t); Rain.Control(t);
                Drips.Control(t, Block, 0.0, s, 0.0);           // listener (x, z) = (0, s) in the alley; yaw 0
            }
            double u = m_inBlock / (double)Block;               // ramps gains linearly inside the block (no zipper)
            Pad.Tick(u, out double pl, out double pr);
            Rain.Tick(u, m_n, out double rl, out double rr);
            double dl = 0, dr = 0; Drips.Tick(ref dl, ref dr);
            double L = PadGain * pl + RainGain * rl + DripGain * dl, R = PadGain * pr + RainGain * rr + DripGain * dr;
            L = m_dcL.Tick(L); R = m_dcR.Tick(R);
            Limiter.Process(ref L, ref R);
            int i = f * channels;
            data[i] = (float)L; if (channels > 1) data[i + 1] = (float)R;
            for (int c = 2; c < channels; c++) data[i + c] = 0f;
            m_n++; if (++m_inBlock == Block) m_inBlock = 0;
        }
    }
}
```

Shared helpers used throughout (compiled):

```csharp
public static class NocMath
{
    public const double TwoPi = 2.0 * Math.PI, InvLn2 = 1.4426950408889634;
    public static double Smootherstep(double e0, double e1, double x)
    { double t = (x - e0) / (e1 - e0); t = t < 0.0 ? 0.0 : (t > 1.0 ? 1.0 : t); return t * t * t * (t * (t * 6.0 - 15.0) + 10.0); }
    public static double Db(double db) { return Math.Pow(10.0, db / 20.0); }
    public static double MidiHz(int n) { return 440.0 * Math.Pow(2.0, (n - 69) / 12.0); }
    /// Counter-based hash to [0,1) (splitmix64 finaliser): event schedules never depend on callback slicing.
    public static double Hash01(ulong a, ulong b)
    {
        unchecked
        {
            ulong z = a * 0x9E3779B97F4A7C15UL + b * 0xD1B54A32D192ED03UL + 0x632BE59BD9B4E019UL;
            z = (z ^ (z >> 30)) * 0xBF58476D1CE4E5B9UL; z = (z ^ (z >> 27)) * 0x94D049BB133111EBUL; z ^= z >> 31;
            return (z >> 11) * (1.0 / 9007199254740992.0);
        }
    }
    /// Equal-power pan, unity on both channels at centre: the mean-over-channels power is pan-independent.
    public static void Pan(double p, out double l, out double r)
    {
        p = p < -1.0 ? -1.0 : (p > 1.0 ? 1.0 : p);
        double a = 0.25 * Math.PI * (1.0 + p);
        l = 1.4142135623730951 * Math.Cos(a); r = 1.4142135623730951 * Math.Sin(a);
    }
}
public struct Rng32   // xorshift32 -> [-1, 1)
{
    uint m_s; public Rng32(uint seed) { m_s = seed != 0u ? seed : 0x9E3779B9u; }
    public double Bipolar() { uint x = m_s; x ^= x << 13; x ^= x >> 17; x ^= x << 5; m_s = x; return unchecked((int)x) * (1.0 / 2147483648.0); }
}
public struct PinkD   // Kellet refined, double; UnitRms from Thread R5
{
    double b0, b1, b2, b3, b4, b5, b6; public const double UnitRms = 1.0 / 1.755;
    public double Next(double w)
    {
        b0 = 0.99886 * b0 + w * 0.0555179; b1 = 0.99332 * b1 + w * 0.0750759; b2 = 0.96900 * b2 + w * 0.1538520;
        b3 = 0.86650 * b3 + w * 0.3104856; b4 = 0.55000 * b4 + w * 0.5329522; b5 = -0.7616 * b5 - w * 0.0168980;
        double p = b0 + b1 + b2 + b3 + b4 + b5 + b6 + w * 0.5362; b6 = w * 0.115926; return p * UnitRms;
    }
}
```

Pitfalls of the architecture:
- **Never use `Math.Log2`**. Its availability in Unity's profile is not verified; use `Math.Log(x) * InvLn2`.
- **Use `Math.Sin` only at control rate.** In per-sample loops (chorus LFO, drip voices, buzz harmonics) use Thread's 4096-entry `SineTable`. The harness used `Math.Sin` and still ran at 1.6 % of real time on .NET 10 (pad with 10 banks, rain, 24 drip voices, DC, TP limiter). Mono in the player is slower (Thread R5 estimated 5-10× in the Editor). Watch `MaxCallbackMs` (Thread's `ThreadSynth`); it must stay well under 21 ms for 1024 frames at 48 kHz.
- **Flush denormals of double states** once per block (`< 1e-30` to 0). Double denormals start near 1e-308; a 0.999 pole reaches them after about 15 s of silence, which happens after the ending fade and while paused.
- **Use a NaN guard per block.** If the sum is not finite, reset everything and output silence (Thread `ThreadDsp.cs:682-688`). One NaN poisons an FDN forever.

---

## 3. The pad (14.3 "Pad", 14.7 H8)

### 3.1 Oscillators: PolyBLEP saw and polyBLAMP triangle

**Finding.** PolyBLEP (Välimäki and Huovilainen 2007) corrects the two samples around each jump with a closed-form polynomial residual, with no table. Martin Finke's write-up gives the common code (`t+t − t*t − 1` after the edge, `t*t + t+t + 1` before it) and builds the triangle by leaky-integrating a PolyBLEP square, `y = A x + (1 − A) y1` with `A = phase increment`. He warns that a plain integrator drifts without bound. A triangle has corners, not jumps, so the matching residual is a BLAMP, the integrated BLEP. Esqueda, Välimäki and Bilbao (DAFx-16) derive the four-point polyBLAMP (their Table 1) and point to the two-point form. For the triangle they add the residual at every corner, "scaled by 2μ", where μ is the slope.

**Derivation used here (verified numerically).** Integrating the 2-point polyBLEP residual of a unit step gives the 2-point polyBLAMP residual of a unit slope change: `R(x) = (1 − |x|)³/6` for |x| < 1 sample. That is `d³/6` on one side and `−d³/6 + d²/2 − d/2 + 1/6` on the other, with d the fractional position. A triangle of amplitude ±1 at f has slope ±4·dt per sample (dt = f/fs), so each corner changes the slope by ±8·dt.

**Measured aliasing** (numpy, 2^18 points, 4-term Blackman-Harris window; worst non-harmonic bin relative to the fundamental):

| f0 | saw naive (< 2 kHz / all) | saw PolyBLEP | tri naive | tri 2-pt polyBLAMP |
|---|---|---|---|---|
| 110 Hz (A2) | −52.2 / −46.6 dB | −97.0 / −54.7 | −97.0 / −93.5 | −97.0 / −97.0 |
| 392 Hz (G4, the top pad note C4/C5 67) | −41.5 / −35.9 | −99.0 / −44.0 | −83.2 / −71.9 | −100.5 / −80.3 |
| 1046.5 Hz | −32.9 / −27.4 | −88.2 / −35.5 | −65.9 / −54.8 | −96.2 / −62.7 |

The residual saw aliasing above 2 kHz (−44 dB at 392 Hz) is then removed by the pad low-pass (500-1600 Hz, 12 dB/oct), which leaves more than 70 dB below the pad.

```csharp
public static class Blep   // ph in [0,1), dt = f / fs (< 0.5), double phases
{
    public static double PolyBlep(double t, double dt)
    {
        if (t < dt) { double x = t / dt; return x + x - x * x - 1.0; }
        if (t > 1.0 - dt) { double x = (t - 1.0) / dt; return x * x + x + x + 1.0; }
        return 0.0;
    }
    /// Residual of a unit slope change (per sample) at phase 0: (1 - |x|)^3 / 6, |x| < 1 sample.
    public static double PolyBlamp(double t, double dt)
    {
        if (t < dt) { double x = 1.0 - t / dt; return x * x * x * (1.0 / 6.0); }
        if (t > 1.0 - dt) { double x = 1.0 + (t - 1.0) / dt; return x * x * x * (1.0 / 6.0); }
        return 0.0;
    }
    public static double Saw(double ph, double dt) { return 2.0 * ph - 1.0 - PolyBlep(ph, dt); }
    /// -1 at ph 0, +1 at ph 0.5; the corners change slope by +-8 dt per sample.
    public static double Tri(double ph, double dt)
    {
        double h = ph + 0.5; if (h >= 1.0) h -= 1.0;
        return 1.0 - 4.0 * Math.Abs(ph - 0.5) + 8.0 * dt * (PolyBlamp(ph, dt) - PolyBlamp(h, dt));
    }
}
```

Pitfalls:
1. **dt must be the actual per-sample increment of that oscillator**, including detune and drift. Pass `inc` and not a nominal value, or the residual is centred wrongly.
2. **Wrap phases with `if (ph >= 1) ph -= 1`** in double. Never use a float phase: after 10 minutes at 48 kHz a float accumulator has about 1e-3 cycles of error, which is audible as detune drift.
3. If the lead prefers the "integrated triangle" literally (leaky-integrated PolyBLEP square), apply three fixes. Scale the input by 4·dt. Initialise the integrator to the naive triangle value at the start phase, otherwise the triangle carries a DC offset that decays with the leak time constant. Keep the leak corner at or below 5 Hz: `a = 1 − 2π·5/fs`, with a gain error under 0.3 % at 73 Hz. The polyBLAMP form needs none of this and has the lower measured alias floor.

### 3.2 Detune and drift

The two oscillators of a voice are the saw at −4 cents and the triangle at +4 cents. Each has its own 0.03 Hz drift of ±2 cents, the second at 1.13 × 0.03 Hz with a hashed start phase. Both are applied as phase increments at control rate:

`inc = f_note · 2^((∓4 + 2·sin(2π(0.03·k·t + ψ)))/1200) / fs`

The 8-cent split beats at 0.34 Hz for D2 (73.4 Hz) and at 1.8 Hz for G4 (392 Hz). That is the slow "breathing" a pad wants. Phases advance with t only, never with s (H8), so the sound is continuous at any speed and when standing still.

### 3.3 TPT low-pass with a smoothed, modulated cutoff

- **Use one stereo filter for all voices.** The low-pass is linear and every voice shares the same cutoff, so filtering the sum equals summing filtered voices, even with a time-varying cutoff. That is 2 SVFs instead of 10.
- **Cutoff:** `fc = base(s) · 2^(0.3·sin(2π·0.04·t))`. Smooth it in log2 with a 20 ms one-pole at control rate (every 32 samples = 1.5 kHz) and compute `g = tan(π fc/fs)` once per block. The trapezoidal (TPT) SVF tolerates coefficient changes because its states are trapezoidal integrator states (Zavalishin, ch. 4; Cytomic). A 20 ms log-smoothed target that changes per 0.67 ms block produces no audible stepping.
- **Base cutoff from the emotion curve** (a proposal): `base = 500 · 3.2^((I − 3)/7)` Hz, with I the 14.1 intensity, which maps 3 to 500 Hz and 10 to 1600 Hz. Interpolate I linearly over each beat (beat 1: 3 to 5, beat 3: 8 to 10, ...). Use Q 0.75, with no audible resonance.
- **Level versus cutoff:** the voice normaliser is computed analytically at the base cutoff (`BankNorm` below), so the ±0.3 octave LFO "breathes" by about ±0.5 dB on high voices. This is intended.

```csharp
public struct SvfD   // Simper/Cytomic trapezoidal SVF (= Zavalishin TPT SVF), double state
{
    double ic1, ic2, a1, a2, a3, k;
    public void Set(double fc, double q, double fs)
    {
        double g = Math.Tan(Math.PI * Math.Min(Math.Max(fc, 5.0), 0.45 * fs) / fs);
        k = 1.0 / q; a1 = 1.0 / (1.0 + g * (g + k)); a2 = g * a1; a3 = g * a2;
    }
    public void CopyCoefficients(in SvfD o) { a1 = o.a1; a2 = o.a2; a3 = o.a3; k = o.k; }
    public double Low(double x)
    { double v3 = x - ic2, v1 = a1 * ic1 + a2 * v3, v2 = ic2 + a2 * ic1 + a3 * v3; ic1 = 2.0 * v1 - ic1; ic2 = 2.0 * v2 - ic2; return v2; }
    public double High(double x)
    { double v3 = x - ic2, v1 = a1 * ic1 + a2 * v3, v2 = ic2 + a2 * ic1 + a3 * v3; ic1 = 2.0 * v1 - ic1; ic2 = 2.0 * v2 - ic2; return x - k * v1 - v2; }
    public double BandNorm(double x)   // 0 dB peak at fc
    { double v3 = x - ic2, v1 = a1 * ic1 + a2 * v3, v2 = ic2 + a2 * ic1 + a3 * v3; ic1 = 2.0 * v1 - ic1; ic2 = 2.0 * v2 - ic2; return k * v1; }
    public void Flush() { if (Math.Abs(ic1) < 1e-30) ic1 = 0.0; if (Math.Abs(ic2) < 1e-30) ic2 = 0.0; }
}
public struct LogFreqSmoother   // one-pole on log2(f) at control rate
{
    double m_y, m_a;
    public void Init(double hz, double tauSeconds, double updatesPerSecond)
    { m_y = Math.Log(hz) * NocMath.InvLn2; m_a = 1.0 - Math.Exp(-1.0 / (tauSeconds * updatesPerSecond)); }
    public double Next(double targetHz) { m_y += m_a * (Math.Log(targetHz) * NocMath.InvLn2 - m_y); return Math.Pow(2.0, m_y); }
}
```

### 3.4 Stereo chorus: 14 and 19 ms, 0.6 ms depth, without zipper noise

**Finding.** A modulated delay line has two error sources:
- **The interpolator.** Linear interpolation is a time-varying low-pass: its magnitude at fractional delay 0.5 is cos(πf/fs). Dattorro's "Effect Design Part 2" (JAES 1997) covers linear and all-pass interpolation for chorus.
- **Stepped modulation (zipper).** A delay time held per block makes phase steps at the block rate.

**Measured** (numpy; the error relative to the ideal continuously delayed sine, 0.2 Hz LFO, 14 ms ± 0.6 ms):

| Tone | Linear interp. error | Hermite interp. error | LFO held per 32 samples (Hermite) | LFO held per 1024-sample callback |
|---|---|---|---|---|
| 110 Hz | −94 dB | −149 dB | −78 dB | −47 dB |
| 400 Hz | −72 dB | −116 dB | −66 dB | −36 dB |
| 1600 Hz | −48 dB | −79 dB | −54 dB | **−24 dB** |
| 4000 Hz | −32 dB | −54 dB | −46 dB | **−16 dB** |

Decision: **evaluate the LFO per sample** (two sine-table lookups per sample) and **read with 4-point cubic Hermite**. Per-callback modulation is clearly audible (−24 dB at 1.6 kHz).

**Pitch deviation** of a sinusoidal delay modulation is `2π·rate·depth`. With 0.6 ms depth: 0.98 cents at 0.15 Hz, 1.30 cents at 0.2 Hz, 1.96 cents at 0.3 Hz, 3.26 cents at 0.5 Hz. Recommendation: **0.17 Hz on the left and 0.23 Hz on the right** (incommensurate, about 1.1-1.5 cents), wet 0.7, dry 1.0.

**Level:** dry plus a correlated wet adds about 1 + w² = +1.7 dB on average, and the moving comb makes a single note swell by ±1 to 3 dB at the LFO period. Measured: the unit-normalised pad's 1 s RMS wanders between +0.6 and +2.9 dB even away from chord changes (beating plus chorus). The 3 s window median absorbs this. Calibrate the pad level *after* the chorus (section 6).

```csharp
public sealed class StereoChorus
{
    const int Size = 8192, Mask = Size - 1;      // 42 ms at 192 kHz
    readonly double[] m_l = new double[Size], m_r = new double[Size];
    int m_w; double m_phL, m_phR;
    readonly double m_incL, m_incR, m_dL, m_dR, m_depth, m_dry, m_wet;

    public StereoChorus(double fs, double delayLms = 14.0, double delayRms = 19.0, double depthMs = 0.6,
                        double rateL = 0.17, double rateR = 0.23, double wet = 0.7)
    {
        m_dL = delayLms * 1e-3 * fs; m_dR = delayRms * 1e-3 * fs; m_depth = depthMs * 1e-3 * fs;
        m_incL = rateL / fs; m_incR = rateR / fs; m_phR = 0.25; m_wet = wet; m_dry = 1.0;
    }
    static double Hermite(double[] b, double pos)   // 4-point, 3rd-order (Catmull-Rom)
    {
        double fl = Math.Floor(pos); int i = (int)fl; double f = pos - fl;
        double xm1 = b[(i - 1) & Mask], x0 = b[i & Mask], x1 = b[(i + 1) & Mask], x2 = b[(i + 2) & Mask];
        double c1 = 0.5 * (x1 - xm1), c2 = xm1 - 2.5 * x0 + 2.0 * x1 - 0.5 * x2, c3 = 0.5 * (x2 - xm1) + 1.5 * (x0 - x1);
        return ((c3 * f + c2) * f + c1) * f + x0;
    }
    public void Tick(ref double l, ref double r)
    {
        int w = m_w; m_l[w] = l; m_r[w] = r;
        double dl = m_dL + m_depth * Math.Sin(NocMath.TwoPi * m_phL);   // use the 4096 sine table in the game
        double dr = m_dR + m_depth * Math.Sin(NocMath.TwoPi * m_phR);
        m_phL += m_incL; if (m_phL >= 1.0) m_phL -= 1.0;
        m_phR += m_incR; if (m_phR >= 1.0) m_phR -= 1.0;
        double tl = Hermite(m_l, w - dl), tr = Hermite(m_r, w - dr);   // negative indices wrap through & Mask
        l = m_dry * l + m_wet * tl; r = m_dry * r + m_wet * tr;
        m_w = (w + 1) & Mask;
    }
}
```

### 3.5 Voice leading between chords

**Finding: the spec's rule is self-contradictory.** "Voices of c and c + 1 are matched by pitch order so common tones stay on one voice": I checked all ten changes, and pure pitch order keeps the common tones in place only for C4 to C5, C5 to C6 and C9 to C10. Elsewhere a common tone changes rank:

| Change | Common tones | Held in slot by pitch order | Moved (would crossfade a note into itself) |
|---|---|---|---|
| C0 to C1 (124) | 53, 60 | 60 | 53 |
| C1 to C2 (286) | 53, 60 | none | 53, 60 |
| C2 to C3 (402) | 50 | none | 50 |
| C3 to C4 (540) | 57 | none | 57 |
| C4 to C5 (700) | 67 | 67 | none |
| C5 to C6 (880) | 38, 45 | 38, 45 | none |
| C6 to C7 (1080) | 54, 64 | 64 | 54 |
| C7 to C8 (1262) | 54, 57 | none | 54, 57 |
| C8 to C9 (1350) | 50 | none | 50 |
| C9 to C10 (1486) | 50 | 50 | none |

Crossfading one pitch between two independent oscillators is not a click, but it is worse. With sum-to-1 gains the middle of the fade is `|0.5·e^{iφ1} + 0.5·e^{iφ2}| = |cos((φ1 − φ2)/2)|`, which can cancel the note completely, and the ±4-cent pairs beat against each other.

**Rule:** (1) pin every common tone to the slot it already occupies; (2) sort the free slots by their previous pitch, with silent slots last, and give them the remaining new notes in ascending order (this minimises Σ|movement|; Tymoczko's "efficient voice leading"); (3) a slot whose note changes gets the new note on its *other* bank, `bank = run(c, slot) & 1`, where `run` counts the note changes in that slot so far. The harness output follows (`*` = held common tone, b = bank):

```
C0  s    0:  38  b0   45  b0   53  b0   60  b0   64  b0
C1  s  124:  46  b1   57  b1   53* b0   60* b0   62  b1
C2  s  286:  43  b0   50  b0   53* b0   60* b0   58  b0
C3  s  402:  39  b1   50* b0   46  b1   57  b1   55  b1
C4  s  540:  41  b0   64  b1   48  b0   57* b1   67  b0
C5  s  700:  38  b1   60  b0   45  b1   53  b0   67* b0
C6  s  880:  38* b1   61  b1   45* b1   54  b1   64  b1
C7  s 1080:  47  b0   62  b0   57  b0   54* b1   64* b1
C8  s 1262:  43  b1   50  b1   57* b0   54* b1   61  b0
C9  s 1350:  40  b0   50* b1   55  b1   47  b0   66  b1
C10 s 1486:  38  b1   50* b1   --  b0   45  b1   --  b0
```

Why this is clean:
- **Pure function of s.** For any s the active pair (c − 1, c), the window weight `w = smootherstep(s_c − h_c, s_c + h_c, s)` and the bank of every note follow from the table. Walking backwards (manual mode) or jumping (`-progress`) needs no state. A jump becomes a 50 ms gain ramp through the per-bank one-pole.
- **A bank only changes its note while silent.** Windows are at least 88 m apart (1262 to 1350); each window is 40 m wide.
- **Equal power for changed slots** (`cos/sin(π/2·w)`). The two notes differ by at least a semitone, so they are uncorrelated over a 3 s window and their powers add. Measured on the unit pad over C2 to C3 at 2.5 m/s: the 1 s RMS with the linear sum-to-1 fade falls to −0.6 dB in the middle (−2 dB against its neighbours). With equal power there is no dip, and the range stays inside the normal chorus/beating wander. Analytically the linear fade dips the pad by −1.55 dB when 3 of 5 slots change and by −2.2 dB when 4 change. The spec's weights `g_c(s)` stay as the chord weights (they sum to 1); only the per-slot amplitude law is equal-power.
- **Pan by pitch, not by slot or rank.** `pan(n) = 0.6·smootherstep(36, 52, n)·(2·hash(n) − 1)`: low notes stay centred and a held note never moves.
- **C10 window.** 14.11 stops the walk at exactly s 1500. With `h = 20` the C9 to C10 fade would end at 1506, freeze at w = smootherstep(0.85) = 0.973 and hang for the 17 s ending, leaving 2.7 % of Em9 under the D5. **Use `h = 14` for C10** (`[1472, 1500]`, centre 1486 unchanged), so w = 1 exactly at the stop. Log it in `DEVELOPMENT.md`.
- **Attack 6 s and release 8 s** are the time-based gate envelope of the whole pad: it enters at s 8 (S1: "only rain and hum" for s 0-8, then "the pad enters over 6 s") and leaves with the ending (the audio fade is 6 s). Shape it linear in amplitude through smoothstep, as Thread did. Do not slew-limit the per-bank crossfade gains: the equal-power law at 2.5 m/s reaches 0.18/s, faster than the 1/8 s⁻¹ release, so a slew limit would distort the equal-power sum.

```csharp
public sealed class PadEngine   // compiled; 5 slots x 2 banks
{
    public const int Slots = 5, Banks = 2 * Slots;
    public static readonly double[] ChangeS = { 0, 124, 286, 402, 540, 700, 880, 1080, 1262, 1350, 1486 };
    public static readonly int[][] Chords =
    {
        new[] { 38, 45, 53, 60, 64 }, new[] { 46, 53, 57, 60, 62 }, new[] { 43, 50, 53, 58, 60 }, new[] { 39, 46, 50, 55, 57 },
        new[] { 41, 48, 57, 64, 67 }, new[] { 38, 45, 53, 60, 67 }, new[] { 38, 45, 54, 61, 64 }, new[] { 47, 54, 57, 62, 64 },
        new[] { 43, 50, 54, 57, 61 }, new[] { 40, 47, 50, 55, 66 }, new[] { 38, 45, 50 },
    };
    /// Half-width of each change window (m). C10 uses 14 m so w reaches 1 exactly at the stop (s 1500).
    public static readonly double[] HalfWindow = { 20, 20, 20, 20, 20, 20, 20, 20, 20, 20, 14 };
    public bool EqualPower = true;
    public readonly int[,] Plan; readonly int[,] m_run;
    readonly double m_fs, m_dt;
    readonly double[] m_phS = new double[Banks], m_phT = new double[Banks], m_incS = new double[Banks], m_incT = new double[Banks];
    readonly double[] m_g = new double[Banks], m_gPrev = new double[Banks], m_gTarget = new double[Banks];
    readonly double[] m_pl = new double[Banks], m_pr = new double[Banks], m_norm = new double[Banks];
    readonly int[] m_note = new int[Banks]; readonly double[] m_driftPh = new double[2 * Banks];
    readonly double m_aGain;            // 50 ms one-pole per control block
    SvfD m_lpL, m_lpR; LogFreqSmoother m_fc; public readonly StereoChorus Chorus;
    public double BaseCutoffHz = 900.0;
    const double SawMix = 0.55, TriMix = 0.85, DetuneCents = 4.0, DriftCents = 2.0, DriftHz = 0.03, LfoHz = 0.04, LfoOctaves = 0.3;

    public PadEngine(double fs, int block, uint seed)
    {
        m_fs = fs; m_dt = 1.0 / fs; Plan = BuildPlan(Chords, Slots, out m_run);
        double updates = fs / block; m_aGain = 1.0 - Math.Exp(-1.0 / (0.05 * updates));
        m_fc.Init(BaseCutoffHz, 0.02, updates); m_lpL.Set(BaseCutoffHz, 0.75, fs); m_lpR.CopyCoefficients(in m_lpL);
        Chorus = new StereoChorus(fs);
        for (int b = 0; b < Banks; b++)
        {
            m_phS[b] = NocMath.Hash01(seed, (ulong)(2 * b)); m_phT[b] = NocMath.Hash01(seed, (ulong)(2 * b + 1));
            m_driftPh[2 * b] = NocMath.Hash01(seed + 7u, (ulong)b); m_driftPh[2 * b + 1] = NocMath.Hash01(seed + 9u, (ulong)b);
            m_note[b] = -1;
        }
    }

    /// Common tones pinned to their slot; the rest by pitch order (silent slots sort last).
    public static int[,] BuildPlan(int[][] chords, int slots, out int[,] run)
    {
        int nc = chords.Length; var plan = new int[nc, slots]; run = new int[nc, slots];
        for (int i = 0; i < slots; i++) plan[0, i] = i < chords[0].Length ? chords[0][i] : -1;
        var slotFree = new bool[slots]; var noteFree = new bool[16]; var order = new int[slots];
        for (int c = 1; c < nc; c++)
        {
            int[] next = chords[c];
            for (int i = 0; i < slots; i++) slotFree[i] = true;
            for (int j = 0; j < next.Length; j++) noteFree[j] = true;
            for (int i = 0; i < slots; i++)
            {
                int j = Array.IndexOf(next, plan[c - 1, i]);
                if (plan[c - 1, i] >= 0 && j >= 0 && noteFree[j]) { plan[c, i] = next[j]; slotFree[i] = false; noteFree[j] = false; }
            }
            int nf = 0; for (int i = 0; i < slots; i++) if (slotFree[i]) order[nf++] = i;
            for (int a = 1; a < nf; a++)
                for (int b = a; b > 0 && Key(plan[c - 1, order[b]]) < Key(plan[c - 1, order[b - 1]]); b--)
                { int t = order[b]; order[b] = order[b - 1]; order[b - 1] = t; }
            int k = 0;
            for (int j = 0; j < next.Length; j++) if (noteFree[j]) plan[c, order[k++]] = next[j];   // next[] ascending
            for (; k < nf; k++) plan[c, order[k]] = -1;
            for (int i = 0; i < slots; i++) run[c, i] = run[c - 1, i] + (plan[c, i] != plan[c - 1, i] ? 1 : 0);
        }
        return plan;
    }
    static int Key(int note) { return note < 0 ? int.MaxValue : note; }
    static double PanOf(int note) { return NocMath.Smootherstep(36.0, 52.0, note) * 0.6 * (2.0 * NocMath.Hash01(0xC0FFEEUL, (ulong)note) - 1.0); }

    /// Unit-RMS normaliser of one bank (saw + triangle through the LP at the base cutoff). Precompute a 128-entry
    /// table at construction in production (this loop is O(fs/f) and runs at a chord change).
    double BankNorm(double hz)
    {
        double p = 0.0;
        for (int k = 1; k * hz < 0.45 * m_fs; k++)
        {
            double aS = SawMix * 2.0 / (Math.PI * k), aT = (k & 1) == 1 ? TriMix * 8.0 / (Math.PI * Math.PI * k * k) : 0.0;
            double w = k * hz / BaseCutoffHz, h2 = 1.0 / ((1 - w * w) * (1 - w * w) + w * w / (0.75 * 0.75));
            p += 0.5 * (aS * aS + aT * aT) * h2;
        }
        return 1.0 / Math.Sqrt(Math.Max(1e-30, p));
    }

    public void Control(double s, double t)
    {
        int c = 0;
        for (int i = 1; i < ChangeS.Length; i++) if (s >= ChangeS[i] - HalfWindow[i]) c = i;
        double w = c == 0 ? 1.0 : NocMath.Smootherstep(ChangeS[c] - HalfWindow[c], ChangeS[c] + HalfWindow[c], s);
        for (int b = 0; b < Banks; b++) m_gTarget[b] = 0.0;
        for (int i = 0; i < Slots; i++)
        {
            int pc = c > 0 ? c - 1 : 0;
            int oldNote = Plan[pc, i], newNote = Plan[c, i];
            int bOld = 2 * i + (m_run[pc, i] & 1), bNew = 2 * i + (m_run[c, i] & 1);
            if (oldNote == newNote || c == 0) { Assign(bNew, newNote); m_gTarget[bNew] = newNote >= 0 ? 1.0 : 0.0; continue; }
            Assign(bOld, oldNote); Assign(bNew, newNote);
            double gO = EqualPower ? Math.Cos(0.5 * Math.PI * w) : 1.0 - w, gN = EqualPower ? Math.Sin(0.5 * Math.PI * w) : w;
            if (oldNote >= 0) m_gTarget[bOld] = gO;
            if (newNote >= 0) m_gTarget[bNew] = gN;
        }
        for (int b = 0; b < Banks; b++) { m_gPrev[b] = m_g[b]; m_g[b] += m_aGain * (m_gTarget[b] - m_g[b]); }
        for (int b = 0; b < Banks; b++)
        {
            if (m_note[b] < 0) continue;
            double hz = NocMath.MidiHz(m_note[b]);
            double dS = DriftCents * Math.Sin(NocMath.TwoPi * (DriftHz * t + m_driftPh[2 * b]));
            double dT = DriftCents * Math.Sin(NocMath.TwoPi * (DriftHz * 1.13 * t + m_driftPh[2 * b + 1]));
            m_incS[b] = hz * Math.Pow(2.0, (-DetuneCents + dS) / 1200.0) * m_dt;
            m_incT[b] = hz * Math.Pow(2.0, (DetuneCents + dT) / 1200.0) * m_dt;
        }
        double lfo = LfoHz * t - Math.Floor(LfoHz * t);
        double fc = m_fc.Next(BaseCutoffHz * Math.Pow(2.0, LfoOctaves * Math.Sin(NocMath.TwoPi * lfo)));
        m_lpL.Set(fc, 0.75, m_fs); m_lpR.CopyCoefficients(in m_lpL);
    }

    void Assign(int b, int note)
    {
        if (m_note[b] == note) return;
        m_note[b] = note;                       // only while the bank is silent (windows >= 88 m apart)
        if (note < 0) return;
        m_norm[b] = BankNorm(NocMath.MidiHz(note)) / Math.Sqrt(Slots);
        NocMath.Pan(PanOf(note), out m_pl[b], out m_pr[b]);
    }

    public void Tick(double u, out double outL, out double outR)   // u = position in the control block [0,1)
    {
        double l = 0.0, r = 0.0;
        for (int b = 0; b < Banks; b++)
        {
            double g = m_gPrev[b] + u * (m_g[b] - m_gPrev[b]);
            double ps = m_phS[b], pt = m_phT[b], is_ = m_incS[b], it = m_incT[b];
            if (g > 1e-7 && m_note[b] >= 0)
            {
                double x = g * m_norm[b] * (SawMix * Blep.Saw(ps, is_) + TriMix * Blep.Tri(pt, it));
                l += m_pl[b] * x; r += m_pr[b] * x;
            }
            ps += is_; if (ps >= 1.0) ps -= 1.0; m_phS[b] = ps;   // phases always advance with t
            pt += it; if (pt >= 1.0) pt -= 1.0; m_phT[b] = pt;
        }
        l = m_lpL.Low(l); r = m_lpR.Low(r);
        Chorus.Tick(ref l, ref r);
        outL = l; outR = r;
    }
}
```

**Measured:** the maximum sample step of the unit pad is 0.256 across the C2 to C3 window, which is 0.005 at the pad's −34 dBFS. That is far under the 14.3 step limit (0.25) and under the H8 click criterion (0.05).

### 3.6 Pad reverb

Use a second `FdnReverb8`: RT60 6 s, pre-delay 40 ms, size **2.5**, damping 4 kHz, wet about −3 dB relative to dry. The pad is a send: the dry pad goes to the bus, the pad-FDN output is added, and the pad bus gain follows the P column. The environment FDN gets rain, drips, footsteps, traffic, the tram, the trains and the X layers with the 14.3 per-place values, blended over 40 m by s (alley 0.9 s / 8 ms / 0.6 through plaza 3.8 s / 70 ms / 0.45). The Edge's 250 ms pre-delay needs `maxPreDelaySeconds ≥ 0.30`. Thread's two-tap crossfade steps the pre-delay in about 1 s crossfades. That gives brief comb filtering on the wet signal only and is acceptable; do not move a read pointer (a 220 ms move over 16 s would detune the tail by about 24 cents).

---

## 4. Rain, drips, city, traffic, wind, tram, trains, footsteps

### 4.1 Rain bed R

- **Spec:** pink noise high-passed at 800 Hz, gust gain 0.6 + 0.4 smooth-random at 0.1 Hz.
- **Implementation:** decorrelated L/R pink (two xorshift streams, Kellet), a TPT high-pass at 800 Hz (Q 0.707, 12 dB/oct). The gust `n(t)` is quintic value noise of a hashed knot sequence (a pure function of t, so the main thread can compute the same value). Normalise by `1/sqrt(E[(0.6 + 0.4n)²])`, measured at about −1.9 dB, so the long-term RMS equals the layer gain. Measured: the bed at a guessed norm reads −2.03 dB re unit over 60 s, so calibrate the constant in the test.
- **Make it more than static** (recommended, inside R's budget): a sparse **patter**, about 20 % of R's power. These are heavy-tailed impulses (size = U³) at about 900/s × rho, placed by a hash of the absolute sample index (partition-free) and coloured by a 4 kHz band-pass at Q 1.2. Rain sound physics (Liu, Cheng and Tong, SIGGRAPH 2019) separates the impact transient from the bubble pulsation of each drop and makes the surface material decide the texture. The bed is the far field; the patter is the near-field impacts. The patter needs its own normaliser (measured: uncalibrated it was −32 dB under the bed). Compute it in the calibration test.
- **Link to the picture:** use the same gust function (and in beat 4 the 17 s gust period of the 0.28 rad slant) for the audio gust and the H5 `rho_eff`. Gusts of rain are then heard and seen together.

```csharp
public sealed class RainBed   // compiled
{
    Rng32 m_nl, m_nr; PinkD m_pl, m_pr; SvfD m_hl, m_hr, m_bl, m_br;
    readonly double m_fs; readonly uint m_seed; double m_gust, m_gustPrev;
    public double PatterRate = 900.0;                   // impulses per second at rho 1
    readonly double m_bedNorm, m_patNorm;
    public RainBed(double fs, uint seed)
    {
        m_fs = fs; m_seed = seed; m_nl = new Rng32(seed * 2u + 1u); m_nr = new Rng32(seed * 2u + 3u);
        m_hl.Set(800.0, 0.7071, fs); m_hr.CopyCoefficients(in m_hl);
        m_bl.Set(4000.0, 1.2, fs); m_br.CopyCoefficients(in m_bl);
        m_bedNorm = 1.0 / 0.5740;   // replace by the calibration test's constant (HP 800 on unit pink, gust folded in)
        m_patNorm = 1.0;            // replace: sqrt(share/(1 - share) * P_bed / P_patter), measured
    }
    public static double SmoothRandom01(uint seed, double t)   // quintic value noise, 1 knot per unit
    {
        double fl = Math.Floor(t); long i = (long)fl; double f = t - fl, u = f * f * f * (f * (f * 6 - 15) + 10);
        double a = NocMath.Hash01(seed, (ulong)i), b = NocMath.Hash01(seed, (ulong)(i + 1));
        return a + (b - a) * u;
    }
    public void Control(double t) { m_gustPrev = m_gust; m_gust = 0.6 + 0.4 * SmoothRandom01(m_seed ^ 0x5EEDu, 0.1 * t); }
    public void Tick(double u, long sampleIndex, out double l, out double r)
    {
        double g = (m_gustPrev + u * (m_gust - m_gustPrev)) * m_bedNorm;
        l = g * m_hl.High(m_pl.Next(m_nl.Bipolar()));
        r = g * m_hr.High(m_pr.Next(m_nr.Bipolar()));
        double h = NocMath.Hash01(m_seed ^ 0xBADu, (ulong)sampleIndex), il = 0.0, ir = 0.0;   // patter: P = rate / fs per sample
        if (h < PatterRate / m_fs)
        {
            double a = Math.Pow(NocMath.Hash01(m_seed ^ 0xA11u, (ulong)sampleIndex), 3.0);
            double pan = NocMath.Hash01(m_seed ^ 0xFA4u, (ulong)sampleIndex);
            il = a * Math.Sqrt(1.0 - pan); ir = a * Math.Sqrt(pan);
        }
        l += m_patNorm * m_bl.BandNorm(il); r += m_patNorm * m_br.BandNorm(ir);
    }
}
```

### 4.2 Drips D (awnings 1.6 kHz, puddles 300-900 Hz, leaves 3-6 kHz)

**Physics** (Moss et al., "Sounding Liquids", building on van den Doel 2005):
- A bubble rings at the Minnaert frequency, `f0·r0 ≈ 3 m/s`, so 300-900 Hz means bubbles of 10-3.3 mm.
- Its impulse response is a damped sine `A0 sin(2π f(t) t) e^{−β0 t}`, with `f(t) = f0 (1 + ξ β0 t)` and **ξ ≈ 0.1** from van den Doel's user study.
- Damping is radiative plus thermal: `δ_rad = sqrt(3γp0/(ρc²))`, `δ_th = sqrt(9(γ−1)²/(4 G_th) · f0)`, `β0 = π f0 δ`. With γ 1.4, p0 101.3 kPa, ρ 1000, c 1480 and G_th ≈ 1.6e6 (my value; verify it against van den Doel if exact constants matter), this is **β ≈ 0.044 f0 + 0.0015 f0^1.5 s⁻¹**:

| f0 | β | T60 | Note |
|---|---|---|---|
| 300 Hz | 21 s⁻¹ | 329 ms | clamp T60 to 250 ms for the mix |
| 600 Hz | 48 s⁻¹ | 143 ms | |
| 900 Hz | 80 s⁻¹ | 86 ms | |
| 3 kHz | 379 s⁻¹ | 18 ms | |

**Pitfall:** `sin(2π f(t) t)`, as the papers write it, doubles the chirp. The instantaneous frequency of `f0(1 + ξβt)·t` is `f0(1 + 2ξβt)`. Integrate the frequency into a phase accumulator (`inc += incSlope` per sample, `incSlope = f0 ξ β / fs²`).

**Voices:**
- **Puddle:** the bubble chirp above, plus a 2 ms tick at 2.5-4.5 kHz at −12 dB.
- **Awning:** two damped modes, 1.6 kHz ± 8 % (T60 60 ms) and 1.656 × that (T60 35 ms, −8 dB). The ratio is a hashed choice; any inharmonic ratio of 1.5-1.8 reads as taut canvas or metal.
- **Leaf:** a 3-6 kHz tick (T60 10 ms) plus a soft 150-210 Hz tap (T60 25 ms, −6 dB).

All start at phase 0, so a damped sine needs no attack ramp and has no click.

**Scheduling (the important part).** Each emitter is placed in the world (alley awnings, puddles on the floors, garden leaves; the beat builders export them as `(x, z, type, mean interval, id)`). Event k of emitter e happens at

`t_k = μ_e (k + 0.5 + J (hash(e, k) − 0.5))`, J = 0.8.

This is a *jittered lattice*, a pure function of (e, k), O(1) for any t, regular-but-irregular like real drip edges (intervals 0.2μ to 1.8μ), and independent of the callback slicing. Only emitters within 14 m are scheduled. Gain is `1/(1 + (d/4)²)`, level ±4 dB from a hash, and pan is `sin(azimuth − cameraYaw)`. Use a pool of 24 voices and steal the quietest. Density per beat comes from the emitter lists: alley about 4-8 drips/s (24 awnings, puddles); garden leaves 10-20/s within reach; bridge and plaza very few (puddles only).

```csharp
// DripField scheduling and voices (compiled; Tick and Trigger as in the harness)
public void Control(double t0, int n, double lx, double lz, double yaw)
{
    double t1 = t0 + n / m_fs;
    for (int i = 0; i < m_count; i++)
    {
        Emitter e = m_e[i];
        double dx = e.X - lx, dz = e.Z - lz, d = Math.Sqrt(dx * dx + dz * dz + 4.0);   // +2 m height
        if (d > ActiveRadius) continue;
        long k0 = (long)Math.Floor(t0 / e.Mu) - 1, k1 = (long)Math.Floor(t1 / e.Mu) + 1;
        for (long k = k0; k <= k1; k++)
        {
            if (k < 0) continue;
            double tk = e.Mu * (k + 0.5 + Jitter * (NocMath.Hash01(e.Id, (ulong)k) - 0.5));
            if (tk < t0 || tk >= t1) continue;
            int delay = (int)Math.Floor((tk - t0) * m_fs);                    // sample-accurate inside the block
            double g = 1.0 / (1.0 + (d / RefDistance) * (d / RefDistance));
            Trigger(e, (ulong)k, delay, g, Math.Sin(Math.Atan2(dx, dz) - yaw));
        }
    }
}
// Puddle voice parameters inside Trigger:
//   f0 = 300 + 600 h1;  beta = max(0.044 f0 + 0.0015 f0^1.5, 6.9078 / 0.25);
//   Inc1 = f0 / fs;  IncSlope = f0 * Xi * beta / fs^2;  Mul1 = exp(-beta / fs);   (Xi = 0.1)
// Per sample: x = A1 e1 sin(2 pi ph1) + A2 e2 sin(2 pi ph2); ph1 += inc1; inc1 += incSlope; e1 *= mul1; ... ; voice off below 1e-5
```

### 4.3 City hum H: brown LP 90 Hz, mains partials, neon buzz 2.4-3.6 kHz

- **Brown:** white noise into a leaky integrator with its corner at 5 Hz (`a = e^{−2π·5/fs}`), then a TPT LP at 90 Hz (Q 0.707). The master 20 Hz DC blocker removes the sub-20 Hz part. Calibrate with the impulse energy of the whole chain *including the DC blocker*. Measured: unit within 0.02 dB.
- **Mains:** magnetostriction makes transformer cores vibrate at **twice** the line frequency, mostly with even harmonics (engineering sources below). For a 50 Hz city: 100 Hz at 0 dB, 50 Hz at −6 dB, 150 Hz at −12 dB. All are integer multiples of **one double 50 Hz master phase** with ±0.02 Hz grid wander, so they stay locked. Compensate each partial for the DC-blocker loss (50 Hz −0.63 dB, 100 Hz −0.16 dB).
- **Neon buzz:** harmonics 24-36 of 100 Hz (2.4-3.6 kHz) under a Gaussian envelope centred at 3.0 kHz (σ 350 Hz). Fixed hashed phases; each harmonic has its own flutter of ±15 % at 0.2-0.7 Hz. There is no aliasing, because the sum is additive, and it costs 13 table lookups per sample.
- **Shares (power):** brown 70 %, mains 20 %, buzz 10 % in beats 1-5 and 7. In **beat 6 (H −33, the sign wall)** shift to 45/25/30 so the wall *buzzes*. The 14.3 ending's "lamp's 100 Hz buzz (−60)" is the mains partial with its own gain.
- Measured: each component and the 70/20/10 mix reach unit RMS within 0.05 dB after the DC blocker.

```csharp
public sealed class CityHum   // compiled; measured unit RMS within 0.05 dB
{
    Rng32 m_nl, m_nr; double m_il, m_ir; SvfD m_ll, m_lr;
    readonly double m_leak, m_brownNorm, m_mainsNorm, m_buzzNorm, m_fs, m_g50, m_g100, m_g150;
    double m_ph50, m_t;
    readonly double[] m_bAmp = new double[13], m_bPsi = new double[13], m_bFl = new double[13], m_flAmp = new double[13];
    public double BrownShare = 0.70, MainsShare = 0.20, BuzzShare = 0.10;

    public CityHum(double fs, uint seed)
    {
        m_fs = fs; m_nl = new Rng32(seed * 5u + 1u); m_nr = new Rng32(seed * 5u + 2u);
        m_leak = Math.Exp(-NocMath.TwoPi * 5.0 / fs);
        m_ll.Set(90.0, 0.7071, fs); m_lr.CopyCoefficients(in m_ll);
        var lp = m_ll; var dc = new DcBlockD(); dc.Init(20.0, fs); double y = 0, e = 0;   // impulse energy incl. DC blocker
        for (int i = 0; i < (int)(4 * fs); i++) { y = m_leak * y + (i == 0 ? 1.0 : 0.0); double o = dc.Tick(lp.Low(y)); e += o * o; }
        m_brownNorm = 1.0 / Math.Sqrt(e / 3.0);                                            // uniform white variance 1/3
        double p = 0;
        for (int k = 0; k < 13; k++)
        {
            double f = (24 + k) * 100.0, a = Math.Exp(-0.5 * ((f - 3000.0) / 350.0) * ((f - 3000.0) / 350.0));
            m_bAmp[k] = a; p += 0.5 * a * a;
            m_bPsi[k] = NocMath.Hash01(seed, (ulong)k); m_bFl[k] = 0.2 + 0.5 * NocMath.Hash01(seed + 1u, (ulong)k);
        }
        m_buzzNorm = 1.0 / Math.Sqrt(p);
        m_g50 = 0.5 / DcBlockD.Magnitude(50, 20, fs); m_g100 = 1.0 / DcBlockD.Magnitude(100, 20, fs); m_g150 = 0.25 / DcBlockD.Magnitude(150, 20, fs);
        m_mainsNorm = 1.0 / Math.Sqrt(0.5 * (0.25 + 1.0 + 0.0625));
    }
    public void Control(double t)
    {
        m_t = t;
        for (int k = 0; k < 13; k++) m_flAmp[k] = m_bAmp[k] * (1.0 + 0.15 * Math.Sin(NocMath.TwoPi * (m_bFl[k] * t + m_bPsi[k])));
    }
    public void Tick(out double l, out double r)
    {
        m_il = m_leak * m_il + m_nl.Bipolar(); m_ir = m_leak * m_ir + m_nr.Bipolar();
        double kb = Math.Sqrt(BrownShare) * m_brownNorm;
        double bl = kb * m_ll.Low(m_il), br = kb * m_lr.Low(m_ir);
        double drift = 1.0 + 0.0004 * Math.Sin(NocMath.TwoPi * 0.013 * m_t);              // grid wander +-0.02 Hz
        m_ph50 += 50.0 * drift / m_fs; if (m_ph50 >= 1.0) m_ph50 -= 1.0;
        double ph = NocMath.TwoPi * m_ph50;                                                 // every partial = integer multiple
        double mains = Math.Sqrt(MainsShare) * m_mainsNorm * (m_g50 * Math.Sin(ph) + m_g100 * Math.Sin(2 * ph) + m_g150 * Math.Sin(3 * ph));
        double buzz = 0.0;                                                                  // sine table in the game
        for (int k = 0; k < 13; k++) buzz += m_flAmp[k] * Math.Sin(2 * (24 + k) * ph + NocMath.TwoPi * m_bPsi[k]);
        buzz *= Math.Sqrt(BuzzShare) * m_buzzNorm;
        l = bl + mains + buzz; r = br + mains + buzz;
    }
}
```

### 4.4 Traffic wash T

- **Chain:** decorrelated pink into a TPT HP at 200 Hz (Q 0.6), then a TPT LP at 900 Hz (Q 0.6), so a band-pass of 200-900 Hz, with a slow level of 0.7 + 0.3 × smooth-random(0.07 Hz).
- **Near pass-bys:** add line-source bumps (4.6) at hashed lattice times, every 6-15 s, +2 to +4 dB for 4-8 s, with the LP opening from 900 to 1300 Hz during the bump. This is "traffic", not "a filter".
- **S2** (s 104-124): T +6 dB with smootherstep ramps of at least 4 m.
- **S6** (876-896): −12 dB.

### 4.5 Wind W

Reuse Thread's L1 wind (`ThreadDsp.cs:706-737`: pink into two `BandNorm` bands with LFO-wandering centres and a cosine-interpolated gust), with the bands set to 300 Hz (Q 0.8) and 900 Hz (Q 1.4, −9 dB). Couple the gust to the visual rain slant in beat 4.

### 4.6 Tram and tube trains: swells without Doppler, done right

**Finding.** A vehicle many metres long is not a point source. For an incoherent line from `x1` to `x2` along the track (relative to the listener's foot point) at perpendicular distance d, the intensity is proportional to the subtended angle over the distance: `I ∝ (atan(x2/d) − atan(x1/d))/d`. This is the finite-line form `L = L'w − 8 − 10 log d + 10 log(2 atan(L/2d))` used in propagation software (Arup Strutt). A point-source `1/r²` law gives a spiky, too-short pass; the line law gives the broad, plateaued swell of a real train. **No Doppler** (14.7): pitch stays constant. The proximity cues come from:

1. the line-law level;
2. brightness: an LP cutoff from 900 Hz far to 4 kHz near, interpolated geometrically by u = α/α_max;
3. width: a decorrelated L/R share of α/π, so a wide source pans less;
4. pan from the azimuth of the midpoint relative to the smoothed camera yaw (100 ms);
5. for trains, a "nose whoosh" (band-limited noise 200-1200 Hz, Gaussian envelope σ 0.25 s centred on the nose passing the foot point) and soft bogie thumps (85 Hz, T60 80 ms) each time a bogie passes the foot point (bogies at ±8.5 m from each 24 m car centre).

Optional: shift only the *noise* filter centres by ±5-10 % between approach and recession. This gives a Doppler-like timbre without any pitch shift of tonal parts. Default off.

**Measured envelopes** (`LineSource.Gain`, dB relative to the abeam peak):

| Tram 60 m, d 32 m (z 227 vs 195), 7 m/s | −200 m | −100 m | −50 m | −20 m | 0 | +30 m (centre abeam) | +60 m | +100 m | +200 m |
|---|---|---|---|---|---|---|---|---|---|
| head position relative to abeam | −16.2 | −11.3 | −7.2 | −3.8 | −1.4 | **0.0** | −1.4 | −6.2 | −13.6 |

| Train 174 m (7 × 24 + 6), 38 m/s, N tube d 12 m | nose −300 m (−7.9 s) | −150 m (−3.9 s) | −50 m (−1.3 s) | 0 | +87 m (+2.3 s) | +174 m (+4.6 s) | +250 m (+6.6 s) |
|---|---|---|---|---|---|---|---|
| level | −22.9 | −18.3 | −12.0 | −2.8 | **0.0** | −2.8 | −14.2 |
| subtended angle (rad) | 0.01 | 0.04 | 0.18 | 1.50 | 2.87 | 1.50 | 0.11 |

At the same reference, the lower tube (d about 142 m, T6) peaks at **−14.9 dB** and the upper tube (d about 116 m, T5) at −13.3 dB. These tubes give a distant rumble, not a pass. **The glass tube muffles:** cap the train LP at 2.5 kHz even at the nearest point, and add the tube's own low resonance (a 40-60 Hz band) for the "inside a pipe" colour.

**Timing:** position is `x(τ)` with τ in real seconds since the s-keyed launch (14.7 H7). The DSP gets the launch as a sample index in the snapshot, so sound and picture share one clock. In capture mode `τ = max(0, (s − s_launch)/2.5)`. Offline, the renderer computes the launch time from its motion model (the time at which s(t) crosses s_launch).

**Tram sources:**
- rolling noise: pink into BP 600 Hz Q 0.8, plus a faint BP 1.8 kHz Q 3 "rail sing";
- motor whine: 420 and 840 Hz sines, ±0.3 % slow FM, −12 dB;
- 60 Hz body rumble;
- the **tram bell**: two strikes ("ding-ding", 0.28 s apart) of a modal bell with partials 1 : 2.0 : 2.76 : 5.40 : 8.93 (the free-bar ratios; choose them by ear) at about 1.3 kHz, T60 1.2 s on the fundamental and shorter on the higher partials. Ring it at a fixed τ when the tram head is about 80 m before abeam. The bell is exempt from the step limit in 14.3. Peak it at about −24 dBFS.

```csharp
public static class LineSource   // compiled
{
    /// Amplitude relative to the abeam peak at reference distance dRef for a vehicle of the same length.
    public static double Gain(double x1, double x2, double d, double length, double dRef)
    {
        double a = Math.Atan(x2 / d) - Math.Atan(x1 / d), aRef = 2.0 * Math.Atan(0.5 * length / dRef);
        return Math.Sqrt(Math.Max(0.0, (a / d) / (aRef / dRef)));
    }
    public static double Angle(double x1, double x2, double d) { return Math.Atan(x2 / d) - Math.Atan(x1 / d); }
}
public struct PassState   // compiled
{
    public double Gain, Pan, Width, CutoffHz;
    public static PassState Evaluate(double xTail, double xHead, double perpendicular, double length, double dRef,
                                     double azimuthMidRelYaw, double fcFar, double fcNear)
    {
        PassState p;
        double a = LineSource.Angle(xTail, xHead, perpendicular), aMax = 2.0 * Math.Atan(0.5 * length / perpendicular);
        p.Gain = LineSource.Gain(xTail, xHead, perpendicular, length, dRef);
        double u = Math.Max(0.0, Math.Min(1.0, a / aMax));
        p.CutoffHz = fcFar * Math.Pow(fcNear / fcFar, u);      // brighter as it fills the view
        p.Width = Math.Min(1.0, a / Math.PI);                   // decorrelated share near abeam
        p.Pan = Math.Sin(azimuthMidRelYaw) * (1.0 - 0.5 * p.Width);
        return p;
    }
}
```

Evaluate `PassState` at control rate and feed it through the 50 ms gain one-poles and the log-cutoff smoother. Pre-roll the vehicle's noise generators: they run whenever the layer exists and only the gain moves, so a pass never starts with a filter transient.

### 4.7 Footsteps (wet concrete, wet stone, steel, glass, garden)

**Cadence keyed to distance.** The step length is `L(v) = 0.75 + 0.1 (v − 1.5)` m, which gives 0.75 m at 1.5 m/s (2.00/s) and 0.85 m at 2.5 m/s (2.94/s). A phase accumulator `+= ds/L` per control block places each step at its sample inside the block (measured: 176 steps in 60 s at 2.5 m/s = 2.93/s). Variation is a hash of the step index: alternate feet (pan ±0.12), ±0.8 dB, pitch ±4 %.

**Surfaces by s (14.4):**

| s range | Surface |
|---|---|
| 0-120 | wet concrete |
| 120-560 | wet stone |
| 560-880 | steel (glass at 706-726) |
| 880-1080 | garden (stone path and moss) |
| 1080-1260 | wet stone |
| 1260-1500 | plaza stone (the wet stone recipe, splash −3 dB) |

**Recipes.** Read every "ms" as T60, 1 ms attack, cosine fade to an exact zero, each variant normalised to a **−30 dBFS peak** (Thread's `FootstepRenderer` pattern), 8 variants per surface rendered at construction on the main thread:

| Surface | Components |
|---|---|
| wet concrete | heel thump (noise into LP 150 Hz, 25 ms) + scuff (BP 2.5 kHz, 15 ms, −8 dB) + **splash**: BP 1.2 kHz Q 1.2 noise with a 90 ms envelope, plus 8-20 hashed micro-droplets (1-3 ms windowed sines at 1.5-5 kHz, at 10-80 ms, falling level) |
| wet stone | click (0.5 ms impulse into BP 3 kHz) + 200 Hz damped sine (60 ms) + splash at −10 dB |
| steel deck | rings at 310 Hz (250 ms) and 820 Hz (150 ms) + 85 Hz thump (90 ms) + scuff |
| glass strip | 640 Hz ring (350 ms) with faint partials ×1.73 and ×2.9 (−14, −20 dB) + tick; no splash |
| garden | soft noise BP 900 Hz Q 0.8 (120 ms, 15 ms attack) + low 120 Hz thud (40 ms) |

The **voices are mixed inside the DSP** (the generator AudioSource cannot take AudioSources), with linear or cubic resampling for pitch, as Thread's offline renderer already does (`OfflineAudioRenderer.cs:154-206`). They feed the environment reverb, so the alley's 0.9 s and the plaza's 3.8 s are heard in the steps.

```csharp
public struct StepScheduler   // compiled
{
    double m_phase; long m_index;
    public long Index { get { return m_index; } }
    public int Advance(double ds, double v, int blockFrames)   // returns the sample offset of a step in the block, or -1
    {
        if (ds <= 0.0) return -1;
        double L = 0.75 + 0.1 * (Math.Max(1.5, Math.Min(2.5, v)) - 1.5);
        double before = m_phase; m_phase += ds / L;
        if (m_phase < 1.0) return -1;
        m_phase -= 1.0; m_index++;
        double frac = (1.0 - before) / (ds / L);
        return (int)Math.Min(blockFrames - 1, Math.Floor(frac * blockFrames));
    }
    public static void Variation(long index, uint seed, out double gainDb, out double pitch, out double pan)
    {
        gainDb = -0.8 + 1.6 * NocMath.Hash01(seed, (ulong)index);
        pitch = 0.96 + 0.08 * NocMath.Hash01(seed + 1u, (ulong)index);
        pan = (index & 1) == 0 ? -0.12 : 0.12;
    }
}
```

### 4.8 X layers

- **Beat 1:** steam vents at s 24, 58, 86 and 104 (spatial emitters: HP 2 kHz noise with hissing bursts, gain by distance) and the vending hum at z 62-67 (100 and 200 Hz, a 50 Hz compressor rumble, slow on/off cycling).
- **Beat 2:** the tram on top of a continuous **boulevard rail bed** (section 6).
- **Beat 4:** the trains on top of a continuous **tube hum bed** (linear-motor drone 60-240 Hz harmonics, a faint rail whine at 1-2 kHz).
- **Beat 5:** leaf rain = the patter algorithm with a 2-6 kHz band at about 2000/s, plus leaf-type drips and a soft rustle (noise BP 2-4 kHz with 8-20 Hz amplitude flutter).
- **Beat 6:** screen hum (−60), a faint 100 Hz family.

---

## 5. Master: DC blocker, look-ahead limiter, true peak

**DC blocker 20 Hz:** `y = x − x1 + R y1`, R = e^{−2π·20/fs} = **0.99738543** at 48 kHz. Measured |H|: −2.14 dB at 25 Hz, **−0.91 dB at 41.2 Hz (koi E1)**, −0.63 at 50, −0.53 at 55 (koi A1), −0.30 at 73.4 (D2), −0.16 at 100 Hz. Compensate the tonal layers analytically and the noise chains by including the blocker in their impulse-energy calibration.

**Chain order:** layers → sends (environment FDN, pad FDN) → sum → DC 20 Hz → master gain (fades, pause duck −12 dB over 0.4 s) → limiter → output. The limiter is last and post-reverb.

**Limiter.** This is the Signalsmith design as The Ring implemented and tested it:
1. the required gain per sample, with a tanh knee from the threshold to the ceiling;
2. the moving minimum over the look-ahead L;
3. instant attack and an exponential release that never rises above the minimum;
4. a box average of length L, which keeps the output under the limit because the average is finite and inside the window;
5. the audio delayed by L − 1.

Parameters: **ceiling −1.5 dBFS, threshold (knee start) −4.5 dBFS, look-ahead 5 ms (L = 240 at 48 kHz), release 80 ms**.

**True-peak detector (new).** Signalsmith notes that inter-sample peaks need upsampling *in the gain calculation*. The detector here takes the max over the four ITU-R BS.1770-4 Annex 2 polyphase outputs plus the sample, looking at sample n − 6 (the filter centre), and the audio is delayed 6 more samples. Measured (harness):

| Input | Detector | Sample peak | True peak (ITU 4× meter) | Minimum gain |
|---|---|---|---|---|
| white-ish bursts +12 dBFS | sample peak | −1.50 dBFS | **+0.49 dBTP** | −11.6 dB |
| pink LP 1.6 kHz, +6 dB hot | sample peak | −1.50 | −1.47 | −29.6 |
| white-ish bursts +12 dBFS | **true peak** | −1.55 | **−1.50 dBTP** | −13.5 |
| pink LP 1.6 kHz, +6 dB hot | **true peak** | −1.51 | −1.50 | −29.6 |
| −6 dBFS sine (below the knee) | true peak | pure delay of 245 samples, max error 0.0 | | |

The cost is 96 multiply-adds per stereo frame, 4.6 M/s. Decision: ship the TP-aware detector. 14.3 requires true peak ≤ −1 dBFS, and the sample-peak limiter cannot guarantee it once it engages on broadband content (footsteps, the bell). In normal operation the limiter should not engage at all: the mix peaks around −15 dBFS. Add a test, ported from the Ring: "the limiter is compressing: fix the mix, not the limiter" when the minimum gain is below −0.5 dB in any beat.

```csharp
public sealed class LookaheadLimiter   // compiled; ported from The Ring's DspLimiter + TP detector
{
    const int Cap = 1024;
    int m_len; double m_thrDb, m_ceilDb, m_thrLin, m_rel;
    readonly double[] m_dl = new double[Cap], m_dr = new double[Cap], m_qv = new double[Cap], m_box = new double[Cap];
    readonly long[] m_qi = new long[Cap];
    int m_wp, m_qh, m_qn, m_bp; double m_boxSum, m_gRel = 1.0; long m_n;
    public double MinGain = 1.0;
    public int LatencySamples { get { return m_len - 1 + (m_tp ? 6 : 0); } }
    // ITU-R BS.1770-4 Annex 2 example interpolator, 4 phases x 12 taps, values x / 8192 (phases 2, 3 = 1, 0 reversed)
    static readonly int[] Tp0 = { 14, 90, -161, 272, -487, 1125, 7964, -838, 390, -218, 122, -68 };
    static readonly int[] Tp1 = { -239, 240, -424, 730, -1364, 3810, 6388, -1641, 832, -477, 271, -155 };
    readonly double[] m_tpH = new double[48], m_tpL = new double[16], m_tpR = new double[16];
    int m_tpPos; bool m_tp;

    public void Configure(double fs, double thresholdDb, double ceilingDb, double lookaheadMs, double releaseMs, bool truePeak = true)
    {
        m_tp = truePeak;
        for (int j = 0; j < 12; j++)
        { m_tpH[j] = Tp0[j] / 8192.0; m_tpH[12 + j] = Tp1[j] / 8192.0; m_tpH[24 + j] = Tp1[11 - j] / 8192.0; m_tpH[36 + j] = Tp0[11 - j] / 8192.0; }
        for (int i = 0; i < 16; i++) { m_tpL[i] = 0.0; m_tpR[i] = 0.0; }
        m_tpPos = 0;
        m_len = Math.Max(2, Math.Min(Cap, (int)Math.Round(lookaheadMs * 1e-3 * fs)));
        m_thrDb = thresholdDb; m_ceilDb = ceilingDb; m_thrLin = Math.Pow(10.0, thresholdDb / 20.0);
        m_rel = 1.0 - Math.Exp(-1.0 / (releaseMs * 1e-3 * fs));
        for (int i = 0; i < Cap; i++) { m_dl[i] = m_dr[i] = 0.0; m_box[i] = 1.0; m_qv[i] = 1.0; m_qi[i] = 0; }
        m_wp = m_qh = m_qn = m_bp = 0; m_boxSum = m_len; m_gRel = 1.0; m_n = 0; MinGain = 1.0;
    }
    double Required(double p)
    {
        if (p <= m_thrLin) return 1.0;
        double pDb = 20.0 * Math.Log10(p), range = m_ceilDb - m_thrDb;
        return Math.Pow(10.0, (m_thrDb + range * Math.Tanh((pDb - m_thrDb) / range) - pDb) / 20.0);
    }
    public void Process(ref double l, ref double r)
    {
        if (!m_tp) { double al = l < 0 ? -l : l, ar = r < 0 ? -r : r; ProcessWithDetector(al > ar ? al : ar, ref l, ref r); return; }
        int p = m_tpPos; m_tpL[p] = l; m_tpR[p] = r;
        double m = 0.0;
        for (int ph = 0; ph < 4; ph++)
        {
            double a = 0.0, b = 0.0; int o = 12 * ph;
            for (int j = 0; j < 12; j++) { int k = (p - j) & 15; a += m_tpH[o + j] * m_tpL[k]; b += m_tpH[o + j] * m_tpR[k]; }
            if (a < 0) a = -a; if (b < 0) b = -b; if (a > m) m = a; if (b > m) m = b;
        }
        int d = (p - 6) & 15; double dl = m_tpL[d], dr = m_tpR[d];
        double sl = dl < 0 ? -dl : dl, sr = dr < 0 ? -dr : dr; if (sl > m) m = sl; if (sr > m) m = sr;
        m_tpPos = (p + 1) & 15;
        ProcessWithDetector(m, ref dl, ref dr);
        l = dl; r = dr;
    }
    public void ProcessWithDetector(double detector, ref double l, ref double r)
    {
        int L = m_len; double req = Required(detector);
        while (m_qn > 0 && m_qi[m_qh] <= m_n - L) { if (++m_qh == L) m_qh = 0; m_qn--; }
        while (m_qn > 0) { int last = m_qh + m_qn - 1; if (last >= L) last -= L; if (m_qv[last] >= req) m_qn--; else break; }
        int t = m_qh + m_qn; if (t >= L) t -= L;
        m_qv[t] = req; m_qi[t] = m_n; m_qn++;
        double m = m_qv[m_qh];
        m_gRel = m < m_gRel ? m : m_gRel + (m - m_gRel) * m_rel;
        m_boxSum += m_gRel - m_box[m_bp]; m_box[m_bp] = m_gRel; if (++m_bp == L) m_bp = 0;
        if ((m_n & 0xFFFF) == 0) { double s = 0; for (int i = 0; i < L; i++) s += m_box[i]; m_boxSum = s; }   // drift guard
        double g = m_boxSum / L; if (g < MinGain) MinGain = g;
        m_dl[m_wp] = l; m_dr[m_wp] = r; if (++m_wp == L) m_wp = 0;
        l = m_dl[m_wp] * g; r = m_dr[m_wp] * g;
        m_n++;
    }
}
```

**True-peak meter (tests and offline analysis).** Use the same ITU coefficients. The 48 published taps are exact multiples of 1/8192. Phase 0 is `14, 90, −161, 272, −487, 1125, 7964, −838, 390, −218, 122, −68`; phase 1 is `−239, 240, −424, 730, −1364, 3810, 6388, −1641, 832, −477, 271, −155`; phases 2 and 3 are phases 1 and 0 reversed. The 48 taps are symmetric with an even length, so the four phases sit at the fractional offsets 1/8, 3/8, 5/8 and 7/8; none reproduces the sample, so the meter also takes the sample peak. The phase DC gains are 1.0016 and 0.973, as published. The standard's example filter is not perfectly flat, and libebur128 users note that self-designed filters are more accurate. The ITU set is traceable, which is why it is used here.

Measured self-test:
- a sine at fs/4, phase π/4, amplitude 1: sample peak −3.01 dBFS, meter **+0.08 dBTP** (ideal 0);
- sines at 1, 5, 10 and 15 kHz over 16 phases: the worst reading is 0.00 dBTP;
- 20 kHz: −0.11 dBTP.

Gate the 14.3 "true peak ≤ −1 dBFS" with this meter on every offline render and on the `-audio-dump`.

---

## 6. Loudness targets per beat

**The power sums are correct.** Each Sum equals `10·log10(Σ 10^(L/10))` of its row within 0.04 dB (the table rounds to 0.1):

| Beat | Computed sum | Spec | Largest shares of power |
|---|---|---|---|
| 1 | −34.87 | −34.9 | R 30.7 %, P 30.7 %, H 19.3 %, X 9.7 % |
| 2 | −30.36 | −30.4 | **X 34.4 %**, P 21.7 %, T 17.2 %, R 13.7 % |
| 3 | −31.29 | −31.3 | P 53.6 %, W 17.0 %, R 13.5 % |
| 4 | −29.60 | −29.6 | P 28.9 %, **X 28.9 %**, W 22.9 % |
| 5 | −33.29 | −33.3 | P 53.5 %, X 21.3 %, R 16.9 % |
| 6 | −30.13 | −30.1 | H 51.6 %, P 32.6 % |
| 7 | −28.88 | −28.9 | P 48.8 %, **K 38.7 %** |

**The measurement semantics break three rows.** The gate is the *median of 3 s windows outside S3, S4, S6 and S7*, ±1.5 dB, and a median ignores transient layers.

1. **Beat 2 (X −35, 34 % of the power).** If X is only the tram pass (audible for about 25 s of 112 s), the median sees X ≈ 0 and lands at **−32.2 dBFS**, 1.8 dB low: fail.
2. **Beat 4 (X −35, 29 %).** Seven trains pass, but each is loud for only 5-8 s. Without X the median lands at **−31.1**, 1.5 dB low: the edge.
3. **Beat 7 (K −33, 39 %).** The koi law `1/(1 + (d/400)²)` with d = the head's distance peaks at s 1360-1365, which matches "peak at s 1350-1380". With the head distances of 14.5.7 (head azimuth `6.64 + 1.76(s − 1260)` degrees, orbit R 260 at y 300 around (1330, 388), listener on z 387.88), K ranges from −43.1 at s 1260 to −33 at the peak, with a median of −38.8 outside S7. Adding the teaser ramp `smootherstep(1200, 1290, s)` gives a modelled beat-7 median of **−30.35** (1.45 dB low). With P trimmed: −31.5 gives −30.01; **−31 gives −29.65**; −30.7 gives −29.42.

**Recommendations:**
- (a) Make X in beats 2 and 4 a **continuous bed** at the table level (boulevard rail and street-machinery hum; the tubes' linear-motor drone) and put the passes on top (+6 to +12 dB). Passes then lift a minority of windows, which leaves the median near the Sum.
- (b) Beat 7: log a **P trim of +1 dB outside S7** in `QA/audio-trims.json` (14.3 allows logged per-beat trims).
- (c) Add a **score-model test.** Evaluate every layer's gain curve (the table, Rule P blends, S-windows, the K law, X beds and passes) at 3 s window centres. Assert (i) the modelled median per beat within ±0.5 dB of the Sum, which is a design check, and (ii) the rendered median within ±0.5 dB of the model, which is a DSP calibration check. The render then sits within ±1.5 dB of the Sum, the spec gate.

**Calibration rules:**
- RMS dBFS = `10·log10(mean over L and R of x²)`, no +3.01 dB AES offset, as in Thread and the Ring.
- Each generator is unit RMS at gain 1 by construction:
  - LTI noise chains by impulse energy including the DC blocker (Thread `ThreadDsp.cs:447-536`);
  - tones analytically, with DC-blocker compensation;
  - stochastic event layers (drips, patter, steps) by a 60 s measurement in an EditMode calibration test that writes a constants file, plus a test asserting the constants are current;
  - the pad *after* chorus and reverb, measured, because the correlated chorus adds about +1.7 dB.
- **Footsteps** are not in the table. At −30 dBFS peaks and 2.94/s they add roughly +0.1 to +0.3 dB. Gate on the full mix, steps included, because that is what the video plays, and keep the trims for that.
- **Reverb:** Thread's energy-normalised wet adds about 0.2 dB at −14 dB wet. It is included automatically when the level is measured on the mix.
- **S-windows:** S1 (pad entry), S2 (T +6 in beat 1) and S8 are not excluded from the gate. Their effect on the medians is at most 0.1 dB, because they are short or come late in the beat.

---

## 7. Rendering offline identically to `OnAudioFilterRead` (Part 3.2 item 14, 11.9)

What makes the two paths identical:
1. **Same class, same entry point.** `NocturneDsp.Process(float[] data, int channels)` is called by `OnAudioFilterRead` and by the offline renderer. The DSP is pure C#, with no UnityEngine and no allocation after construction. Construction, calibration, footstep banks and normaliser tables run on the main thread (Awake, or the test).
2. **Control at absolute sample indices.** `m_inBlock` carries across calls, so a 1024-, 441- or 37-frame callback all see control updates at n = 32k. **Measured: 20 s renders across C3 with buffers of 1024, 256, 1000, 441 and {37, 2048, 512, 1, 333} are bit-identical (max |diff| = 0).** Thread's `ThreadDsp.Process` restarts its sub-blocks at every callback (`f0 = 0`), which is invariant only when the buffer length is a multiple of 32, and its star notes draw Poisson events from an RNG per sub-block. Fix both.
3. **No RNG consumed per event or per block.** Per-sample noise RNG streams are fine because they advance exactly once per sample. Event decisions (drips, patter, step variation, bell timing, pass-by bumps) use `Hash01(stream, index)` of the event index or the absolute sample index.
4. **Progress comes from a snapshot the DSP extrapolates.** The main thread publishes `(s, v, tSnap)` once per frame; the DSP evaluates `s = s_snap + v·(n − tSnap)/fs` per control block, clamped to 0.25 s of extrapolation, and the 50 ms gain smoothers hide the small jumps when a new snapshot arrives. Offline, the renderer sets the snapshot from the exact motion model per block, so the render is exact and block-size independent. In the player, the frame jitter changes event times by under one frame (steps, drips). That is inaudible and irrelevant to the RMS gates.
5. **Vehicle clocks are sample indices.** The tram and train launch times and the goal trigger are DSP sample indices in the snapshot. τ is computed inside the DSP.
6. **Sample rate.**
   - Request 48 kHz: set `m_SampleRate: 48000` in `AudioManager.asset` through `ProjectConfigurator`.
   - Always build the DSP with `AudioSettings.outputSampleRate` and rebuild it on `OnAudioConfigurationChanged`: build on the main thread, `SnapTo(current)`, swap with `Volatile.Write`.
   - Offline renders use 48 kHz. For the `-audio-dump` comparison, render offline **at the device's rate** over the same s range.
7. **Pre-roll.** A beat render that starts mid-walk with snapped smoothers still begins with an empty FDN, which makes the first seconds quieter. Run 8 s of pre-roll at the start s (speed 0, all layers on), discard it, then render. This applies to every per-beat WAV.
8. **Generator setup** (Ring 05 F4/F8/F11, Part 3.2 item 14):
   - a dedicated GameObject with `AudioSource` (clip null, `loop`, `spatialBlend 0`, `priority 0`, `bypassEffects`, `bypassListenerEffects` and `bypassReverbZones` true, `dopplerLevel 0`) and `NocturneSynth`;
   - add the components while the object is inactive, activate it, then `Play()` once;
   - never `Stop()` or toggle it (UUM-117120): silence through the DSP's master gain;
   - no OAFR component on the AudioListener's GameObject;
   - `Application.runInBackground = true` in autotest, capture and dump runs.
9. **Thread safety:** the double-buffer mailbox from Thread (`ThreadSynth.Publish`); exceptions stored and logged once by the main thread; a heartbeat counter; a NaN guard; flush per block.
10. **`-audio-dump`** (Thread's `AudioCapture`):
    - in the cinematic run, arm it after the intro and record 10 s;
    - write a float32 WAV from a worker, plus a JSON sidecar with fs, channels, s at start and end, callbacks and `MaxCallbackMs`;
    - compare with the offline render of the same s range at the same fs: RMS within 1 dB (Thread achieved 0.6 dB), no NaN, true peak ≤ −1 dBTP, |DC| < 0.001.
    - Bit-exactness is impossible here: snapshot timing differs, Mono and .NET math libraries may differ in the last bit, and the device may run at 44.1 kHz.

**Tests to write** (EditMode unless noted):

| Test | Assertion |
|---|---|
| `Dsp_PartitionInvariant` | 20 s render at buffers {1024}, {256}, {441}, {37, 2048, 512, 1, 333}: max \|diff\| == 0 |
| `Dsp_ZeroAlloc` | `GC.GetAllocatedBytesForCurrentThread` delta == 0 over 60 s of `Process` after warm-up |
| `Osc_Aliasing` | PolyBLEP saw at 392 Hz: worst non-harmonic bin below 2 kHz ≤ −90 dB; polyBLAMP triangle ≤ −95 dB (a test FFT with a 4-term Blackman-Harris window) |
| `Pad_PlanPinsCommonTones` | for every change, every common tone keeps its slot; no slot changes note while its bank gain is above 1e-4 |
| `Pad_Crossfade` | pad stem over each change ±20 m at 1.5 and 2.5 m/s: 3 s RMS within ±0.75 dB of the stable chords; max sample step < 0.05 at mix level; click detector (second difference above 8× its local 10 ms RMS and above 0.005) finds nothing |
| `Pad_C10Completes` | at s = 1500 the C10 weight is 1.0 (window [1472, 1500]) |
| `Chorus_NoZipper` | a 1.6 kHz sine through the chorus: the error spectrum at block-rate harmonics (1.5 kHz × k) is below −80 dB |
| `Limiter_Ceiling` | +12 dB white bursts: sample peak ≤ −1.5 dBFS and ITU true peak ≤ −1.5 dBTP; −6 dBFS sine: output == input delayed by `LatencySamples` |
| `TruePeak_SelfTest` | fs/4 sine at phase π/4: meter within 0 ± 0.15 dBTP |
| `DcBlocker` | R = 0.99738543 ± 1e-8 at 48 kHz; DC of the master < 0.001 over each beat render |
| `Calibration_Constants` | each layer at gain 1 has RMS 0 ± 0.1 dB over 60 s (seeded), so the stored constants are current |
| `Score_Model` | modelled 3 s median per beat within ±0.5 dB of the 14.3 Sum after logged trims; S-windows reported separately |
| `Render_Beats` (both speeds) | per-beat WAVs: median 3 s RMS within ±1.5 dB of the Sum (logged trims), true peak ≤ −1 dBTP, \|DC\| < 0.001, step ≤ 0.25 except steps and the bell, limiter minimum gain ≥ −0.5 dB, no NaN |
| player `-audio-dump` | RMS within 1 dB of the offline render at the device rate; heartbeat > 0; `MaxCallbackMs` < 5 ms |

---

## 8. Pitfalls checklist (new for Nocturne)

1. **Pitch order alone breaks common tones** in 8 of the 10 changes. Pin the common tones first.
2. **A sum-to-1 crossfade of different notes dips 1.5-2.2 dB.** Of the *same* note on two oscillators, it can cancel completely.
3. **C10 hangs at 97 %** with the default window because the walk stops at s 1500. Use a 14 m half-window.
4. **A delay modulated per callback zippers** (−24 dB at 1.6 kHz). Modulate per sample and read with Hermite.
5. **A sample-peak limiter is not a true-peak limiter**: +0.49 dBTP on hot broadband input. Use the ITU detector.
6. **The 20 Hz DC blocker takes 0.9 dB off the koi's 41.2 Hz.** Compensate.
7. **The median gate cannot see transient layers.** X in beats 2 and 4 must be a bed, and K in beat 7 needs a pad trim.
8. **The bubble formula `sin(2π f(t) t)` doubles the chirp.** Integrate the frequency instead.
9. **`Math.Sin` per sample per voice is the CPU hot spot.** Use the sine table; skip silent banks; measure `MaxCallbackMs` in the player.
10. **Never key anything to the callback.** Not RNG draws, not LFO updates, not event decisions: only absolute sample indices and event indices.

---

## 9. Sources (accessed 2026-11-13)

| # | Source | Used for |
|---|---|---|
| L1 | `W:\YouTube\Megastructures\3rd Megastructures\3rd Megastructures\docs\research\R5_audio.md`; `Assets\_Game\Scripts\Runtime\Audio\ThreadDsp.cs`, `ThreadSynth.cs`, `OfflineAudioRenderer.cs`, `FootstepBank.cs`, `AudioDirector.cs`; `DEVELOPMENT.md` D-012, D-021, D-033 | the settled base: SVF, FDN, calibration method, meter, capture, allocation finding |
| L2 | `W:\YouTube\Megastructures\2nd Megastructures\2nd Megastructures\docs\research\05_procedural_audio.md` (F4-F11, §6.9, §9); `Assets\_Game\Scripts\Runtime\Audio\AudioDsp.cs:253-322`; `Tests\EditMode\AudioTests.cs:163-200` | generator-on-AudioSource rules; look-ahead limiter code and tests; true-peak self-test idea |
| L3 | `C:\Program Files\Unity\Hub\Editor\6000.6.0f1\Editor\Data\Managed\UnityEngine\UnityEngine.AudioModule.xml` lines 2336, 2341, 2365, 2424-2432, 2455, 2480, 2694; `...\Documentation\en\ScriptReference\MonoBehaviour.OnAudioFilterRead.html`; project `ProjectSettings\AudioManager.asset`, `ProjectSettings.asset` | Unity 6000.6 API ground truth; project audio settings |
| W1 | https://www.martin-finke.de/articles/audio-plugins-018-polyblep-oscillator/ | polyBLEP code; leaky-integrator triangle and its caveats |
| W2 | V. Välimäki, A. Huovilainen, "Antialiasing oscillators in subtractive synthesis", IEEE Signal Processing Magazine 24(2):116-125, 2007, DOI 10.1109/MSP.2007.323276 (record: https://manuscript.isc.ac/Inventory/49/1195065.htm) | origin of polyBLEP (closed-form polynomial BLEP) |
| W3 | F. Esqueda, V. Välimäki, S. Bilbao, "Rounding Corners with BLAMP", DAFx-16: https://dafx.de/paper-archive/2016/dafxpapers/18-DAFx-16_paper_33-PN.pdf (page https://dafx.de/paper-archive/details/xRM0GiuF-JmeN0_aaP9VNg) | polyBLAMP; Table 1 (4-point residuals); triangle application ("scaled by 2µ"); the 2-point form, derived here and verified numerically |
| W4 | V. Zavalishin, "The Art of VA Filter Design" 2.1.0: https://www.native-instruments.com/fileadmin/ni_media/downloads/pdf/VAFilterDesign_2.1.0.pdf | TPT SVF (ch. 4) and modulation robustness (with Thread's Cytomic source) |
| W5 | J. Dattorro, "Effect Design Part 2: Delay-Line Modulation and Chorus", JAES 45(10):764-788, 1997: https://ccrma.stanford.edu/~dattorro/EffectDesignPart2.pdf ; AES TOC https://www.aes.org/journal/toc/oct97.html | chorus structure; the interpolation question (linear versus all-pass), checked numerically here (Hermite chosen) |
| W6 | G. Luff, "Designing a straightforward limiter" (Signalsmith Audio, 2022): https://signalsmith-audio.co.uk/writing/2022/limiter/ | limiter steps, why the output stays under the limit, true peaks need upsampling in the gain calculation |
| W7 | ebur128-stream `peak.rs`: https://docs.rs/crate/ebur128-stream/0.2.0/source/src/peak.rs | the BS.1770-4 Annex 2 48-tap coefficients (secondary copy, stated to match the libebur128 reference values; exact as k/8192) |
| W8 | https://cacophony.js.org/classes/TruePeakDetector.html ; https://github.com/audionuma/r128x/issues/10 | 4× at 48 kHz per Annex 2 (BS.1770-5 note); self-designed filters can be more accurate than the example |
| W9 | W. Moss et al., "Sounding Liquids", ACM TOG 2010: https://gamma.cs.unc.edu/SoundingLiquids/soundingliquids.pdf ; K. van den Doel, "Physically-based models for liquid sounds", ACM TAP 2(4), 2005: https://repository.gatech.edu/entities/publication/dcc121b6-5fdb-42db-b758-971522f01c70 | Minnaert f0 r0 ≈ 3 m/s; damping terms; f(t) = f0(1 + ξβt); ξ ≈ 0.1 |
| W10 | S. Liu, H. Cheng, Y. Tong, "Physically-based statistical simulation of rain sound", ACM TOG 38(4), 2019: https://history.siggraph.org/learning/physically-based-statistical-simulation-of-rain-sound-by-liu-cheng-and-tong/ | rain = impact plus bubble pulsation; material-dependent textures (motivates bed plus patter) |
| W11 | Arup Strutt, finite line attenuation: https://strutt.arup.com/help/Propagation/LineAtten.htm | finite incoherent line-source formula (angle-of-view term) for the swells |
| W12 | https://industrialmonitordirect.com/blogs/knowledgebase/transformer-magnetostriction-hum-why-100120hz-not-300360hz ; https://wiki.testguy.net/t/transformer-humming-noise-explained/88 | hum at twice the mains frequency, mostly even harmonics |
| W13 | D. Tymoczko, "The Geometry of Musical Chords", Science 313:72-74, 2006 (summary: https://www.nature.com/articles/442149a) | efficient voice leading (small, non-crossing moves); motivates the pitch-order part of the plan |
| W14 | https://www.firstpr.com.au/dsp/pink-noise/ (via Thread R5 S10) | Kellet pink noise (not re-fetched) |

Harness (scratchpad, not in the project): `NocDspSketch.cs`, `Layers2.cs`, `TpTest.cs`, `Program.cs`, `Program2.cs` (C# 9, .NET 10) and `alias.py`, `chorus.py`, `rms.py`, `koi.py`, `vl.py` (numpy 2.5). Every number in sections 3-7 comes from these runs.
