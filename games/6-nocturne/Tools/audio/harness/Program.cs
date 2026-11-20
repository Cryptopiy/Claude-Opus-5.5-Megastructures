// AU's offline audio harness (docs/AUDIO.md "Rendering"): renders the score with the game's own DSP sources (NocMixer and the
// layers of Scripts/Runtime/Audio, compiled here against UnityEngine.CoreModule) and prints / writes the numbers of the 14.3
// gates. Commands:
//   walk  --speed 2.5 [--s0 0 --s1 1500] [--wav <file>] [--json <file>] [--ending] [--seed N]   whole-walk render + analysis
//   calibrate [--speed 2.5]                                                                      stems vs their gains per beat
//   partition                                                                                    block-size invariance check
//   timing                                                                                       DSP cost per second of audio
//   gates                                                    the AudioScoreTests renders (AudioGates), with their margins
//   alloc                                                    bytes allocated by NocMixer.Process (both paths) after warm-up
// Build: dotnet build Tools/audio/harness -c Release ; run: dotnet <TEMP>/nocturne_audio_harness/bin/NocAudioHarness.dll walk ...
using System;
using System.Globalization;
using System.IO;
using System.Text;

namespace Megastructure.AudioHarness
{
    public static class Program
    {
        static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

        static string Arg(string[] a, string name, string def)
        {
            for (int i = 0; i + 1 < a.Length; i++) if (a[i] == name) return a[i + 1];
            return def;
        }
        static bool Has(string[] a, string name) => Array.IndexOf(a, name) >= 0;
        static double D(string[] a, string name, double def) => double.Parse(Arg(a, name, def.ToString(Inv)), Inv);

        public static int Main(string[] args)
        {
            CultureInfo.DefaultThreadCurrentCulture = Inv;
            string cmd = args.Length > 0 ? args[0] : "walk";
            switch (cmd)
            {
                case "walk": return Walk(args);
                case "calibrate": return Calibrate(args);
                case "partition": return Partition();
                case "timing": return Timing();
                case "unit": return Unit(args);
                case "gates": return Gates(args);
                case "alloc": return Alloc();
                case "probe": return Probe();
                default: Console.WriteLine("unknown command " + cmd); return 2;
            }
        }

        static OfflineRenderSettings Settings(string[] a, bool stems)
        {
            return new OfflineRenderSettings
            {
                S0 = D(a, "--s0", 0.0), S1 = D(a, "--s1", NocturneConstants.GoalS), Speed = D(a, "--speed", 2.5),
                Seed = (int)D(a, "--seed", 1), Ending = Has(a, "--ending"), MeterStems = stems, BlockFrames = 4800,
                PrerollSeconds = D(a, "--preroll", 0.0), RainOverride = Has(a, "--rain") ? D(a, "--rain", 1.0) : double.NaN,
                SoloMask = SoloOf(Arg(a, "--solo", null)), Footsteps = !Has(a, "--nosteps"),
            };
        }

        /// <summary>--solo RDHTWPKXF letters -> NocMixer.SoloMask (null = everything).</summary>
        static int SoloOf(string letters)
        {
            if (string.IsNullOrEmpty(letters)) return -1;
            int m = 0;
            foreach (char ch in letters) { int k = "RDHTWPKXF".IndexOf(char.ToUpperInvariant(ch)); if (k >= 0) m |= 1 << k; }
            return m;
        }

        static int Walk(string[] a)
        {
            var st = Settings(a, true);
            var an = new WalkAnalysis();
            var sw = System.Diagnostics.Stopwatch.StartNew();
            string wav = Arg(a, "--wav", null);
            WavStreamWriter w = null;
            if (wav != null) { Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(wav))); w = new WavStreamWriter(wav, 2, st.SampleRate, WavFormat.Float32); }
            var mixer = OfflineAudioRenderer.CreateMixer(st, out _);
            long frames = w != null ? OfflineAudioRenderer.Render(st, an, w) : OfflineAudioRenderer.Render(st, an);
            w?.Dispose();
            if (wav != null)
            {
                // sidecar for Tools/qa/audio_analysis.py: s per second of the render (beat assignment)
                var mo = new WalkMotion { S0 = st.S0, V0 = st.Speed, Ending = st.Ending };
                var sj = new StringBuilder("{\"dt\": 1.0, \"t0\": 0.0, \"stem\": \"mix\", \"s\": [");
                double dur = frames / (double)st.SampleRate;
                for (int i = 0; i <= (int)Math.Ceiling(dur); i++) { if (i > 0) sj.Append(','); sj.Append(mo.SAt(i).ToString("0.###", Inv)); }
                sj.Append("]}");
                File.WriteAllText(Path.ChangeExtension(wav, ".json"), sj.ToString());
            }
            double secs = sw.Elapsed.TotalSeconds;
            var sb = new StringBuilder();
            sb.AppendLine(FormattableString.Invariant($"walk s {st.S0:0}-{st.S1:0} at {st.Speed:0.00} m/s: {frames / (double)st.SampleRate:0.0} s of audio in {secs:0.0} s ({frames / (double)st.SampleRate / secs:0.0}x real time)"));
            sb.AppendLine(FormattableString.Invariant($"sample peak {20 * Math.Log10(Math.Max(an.SamplePeak, 1e-12)):0.00} dBFS, true peak {an.TruePeak.PeakDb:0.00} dBTP, DC {an.Dc:0.000000}, max step {an.MaxStep:0.0000} at {an.MaxStepAt:0.000} s, non-finite {an.NonFinite}"));
            sb.AppendLine("beat  target   median   dev   windows | stems (median dB): " + string.Join(" ", NocturneScore.LayerNames));
            var json = new StringBuilder();
            json.Append("{\"speed\":").Append(st.Speed.ToString("R", Inv)).Append(",\"seconds\":").Append((frames / (double)st.SampleRate).ToString("0.###", Inv));
            json.Append(",\"truePeakDb\":").Append(an.TruePeak.PeakDb.ToString("0.###", Inv)).Append(",\"samplePeakDb\":").Append((20 * Math.Log10(Math.Max(an.SamplePeak, 1e-12))).ToString("0.###", Inv));
            json.Append(",\"dc\":").Append(an.Dc.ToString("0.########", Inv)).Append(",\"maxStep\":").Append(an.MaxStep.ToString("0.#####", Inv)).Append(",\"beats\":[");
            for (int b = 0; b < NocturneConstants.BeatCount; b++)
            {
                double tgt = NocturneConstants.TargetRmsDb[b];
                double med = an.BeatMedianDb(b, out int nw);
                sb.Append(FormattableString.Invariant($" {b + 1}   {tgt,6:0.0}  {med,7:0.00}  {med - tgt,+5:+0.00;-0.00}  {nw,4}   |"));
                if (b > 0) json.Append(',');
                json.Append("{\"beat\":").Append(b + 1).Append(",\"target\":").Append(tgt.ToString("0.0", Inv)).Append(",\"median\":").Append(med.ToString("0.###", Inv)).Append(",\"windows\":").Append(nw).Append(",\"stems\":{");
                for (int k = 0; k < NocturneScore.StemCount; k++)
                {
                    double sd = an.StemMedianDb(b, k, out _);
                    sb.Append(FormattableString.Invariant($" {sd,6:0.0}"));
                    if (k > 0) json.Append(',');
                    json.Append('"').Append(NocturneScore.LayerNames[k].Substring(0, 1)).Append("\":").Append(double.IsNaN(sd) || sd < -200 ? "null" : sd.ToString("0.##", Inv));
                }
                json.Append("}}");
                sb.AppendLine();
            }
            // the loudest bins (where the peaks come from)
            var idx = new System.Collections.Generic.List<int>();
            for (int i = 0; i < an.BinPeak.Count; i++) idx.Add(i);
            idx.Sort((x, y) => an.BinPeak[y].CompareTo(an.BinPeak[x]));
            for (int k = 0; k < Math.Min(8, idx.Count); k++)
            {
                int i = idx[k];
                var stems = i < an.BinStems.Count ? an.BinStems[i] : new double[NocturneScore.StemCount];
                var line = new StringBuilder(FormattableString.Invariant($"peak {20 * Math.Log10(Math.Max(an.BinPeak[i], 1e-12)):0.0} dBFS at s {an.BinS[i]:0.0} (bin {WalkAnalysis.Db(an.BinMs[i]):0.0} dB):"));
                for (int q = 0; q < NocturneScore.StemCount; q++) line.Append(FormattableString.Invariant($" {NocturneScore.LayerNames[q].Substring(0, 1)} {Math.Max(-99, WalkAnalysis.Db(stems[q])):0}"));
                sb.AppendLine(line.ToString());
            }
            json.Append("],\"swells\":[");
            double[] s0 = { 0, 104, 400, 540, 876, 1340, 1486 }, s1 = { 8, 124, 420, 554, 896, 1380, 1500 };
            string[] nm = { "S1", "S2", "S3", "S4", "S6", "S7", "S8" };
            for (int i = 0; i < s0.Length; i++)
            {
                double r = an.RangeDb(s0[i], s1[i], out double pk);
                sb.AppendLine(FormattableString.Invariant($"{nm[i]} s {s0[i]:0}-{s1[i]:0}: {r:0.00} dBFS (peak {pk:0.00}); P {an.StemRangeDb(NocturneScore.P, s0[i], s1[i]):0.0} W {an.StemRangeDb(NocturneScore.W, s0[i], s1[i]):0.0} T {an.StemRangeDb(NocturneScore.T, s0[i], s1[i]):0.0} K {an.StemRangeDb(NocturneScore.K, s0[i], s1[i]):0.0} H {an.StemRangeDb(NocturneScore.H, s0[i], s1[i]):0.0} X {an.StemRangeDb(NocturneScore.X, s0[i], s1[i]):0.0}"));
                if (i > 0) json.Append(',');
                json.Append("{\"name\":\"").Append(nm[i]).Append("\",\"s0\":").Append(s0[i]).Append(",\"s1\":").Append(s1[i]).Append(",\"db\":").Append(r.ToString("0.##", Inv)).Append(",\"peakDb\":").Append(pk.ToString("0.##", Inv)).Append('}');
            }
            json.Append("],\"chords\":[");
            for (int c = 1; c < NocturneScore.ChordChangeS.Length; c++)
            {
                sb.AppendLine(FormattableString.Invariant($"chord C{c - 1}->C{c} at s {NocturneScore.ChordChangeS[c]:0}: max sample step {an.ChordMaxStep[c]:0.00000}, max 20 ms envelope step {an.ChordMaxEnvStep[c]:0.00000}"));
                if (c > 1) json.Append(',');
                json.Append("{\"c\":").Append(c).Append(",\"s\":").Append(NocturneScore.ChordChangeS[c]).Append(",\"maxStep\":").Append(an.ChordMaxStep[c].ToString("0.#######", Inv)).Append(",\"maxEnvStep\":").Append(an.ChordMaxEnvStep[c].ToString("0.#######", Inv)).Append('}');
            }
            json.Append("],\"bins\":{\"s\":[");
            for (int i = 0; i < an.BinS.Count; i++) { if (i > 0) json.Append(','); json.Append(an.BinS[i].ToString("0.##", Inv)); }
            json.Append("],\"db\":[");
            for (int i = 0; i < an.BinMs.Count; i++) { if (i > 0) json.Append(','); json.Append(WalkAnalysis.Db(an.BinMs[i]).ToString("0.##", Inv)); }
            json.Append("],\"stems\":[");
            for (int k = 0; k < NocturneScore.StemCount; k++)
            {
                if (k > 0) json.Append(',');
                json.Append('[');
                for (int i = 0; i < an.BinStems.Count; i++) { if (i > 0) json.Append(','); json.Append(Math.Max(-200, WalkAnalysis.Db(an.BinStems[i][k])).ToString("0.#", Inv)); }
                json.Append(']');
            }
            json.Append("],\"targets\":[");
            for (int k = 0; k < NocturneScore.LayerCount; k++)
            {
                if (k > 0) json.Append(',');
                json.Append('[');
                for (int i = 0; i < an.BinS.Count; i++) { if (i > 0) json.Append(','); json.Append(Math.Max(-200, 20 * Math.Log10(Math.Max(1e-12, NocturneScore.LayerAmp(k, an.BinS[i])))).ToString("0.##", Inv)); }
                json.Append(']');
            }
            json.Append("]}}");
            Console.Write(sb.ToString());
            string jp = Arg(a, "--json", null);
            if (jp != null) { Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(jp))); File.WriteAllText(jp, json.ToString()); }
            return 0;
        }

        /// <summary>Per beat: each stem's median against the gain the score gives it (so the generators' unit-RMS constants can be set).</summary>
        static int Calibrate(string[] a)
        {
            var st = Settings(a, true);
            var an = new WalkAnalysis();
            OfflineAudioRenderer.Render(st, an);
            Console.WriteLine("beat layer   table   stem-median   delta (stem - table): positive = generator louder than unit");
            for (int b = 0; b < NocturneConstants.BeatCount; b++)
                for (int l = 0; l < NocturneScore.LayerCount; l++)
                {
                    double tdb = NocturneScore.LayerDb[b, l];
                    if (double.IsNaN(tdb) && l != NocturneScore.K) continue;
                    double sd = an.StemMedianDb(b, l, out _);
                    Console.WriteLine(FormattableString.Invariant($" {b + 1}   {NocturneScore.LayerNames[l],-10} {tdb,6:0.0}   {sd,8:0.00}   {sd - tdb,+6:+0.00;-0.00}"));
                }
            return 0;
        }

        static int Partition()
        {
            int fs = 48000;
            var sizes = new[] { new[] { 1024 }, new[] { 256 }, new[] { 441 }, new[] { 37, 2048, 512, 1, 333 } };
            float[] reference = null;
            foreach (var pat in sizes)
            {
                var st = new OfflineRenderSettings { S0 = 390.0, Speed = 2.5, Seconds = 20.0, Seed = 3 };
                var mixer = OfflineAudioRenderer.CreateMixer(st, out _);
                int total = 20 * fs;
                var outBuf = new float[total * 2];
                int pos = 0, k = 0;
                while (pos < total)
                {
                    int n = Math.Min(pat[k++ % pat.Length], total - pos);
                    var b = new float[n * 2];
                    mixer.Process(b, 2);
                    Array.Copy(b, 0, outBuf, pos * 2, n * 2);
                    pos += n;
                }
                if (reference == null) { reference = outBuf; Console.WriteLine("reference " + string.Join(",", pat)); continue; }
                double md = 0; for (int i = 0; i < outBuf.Length; i++) md = Math.Max(md, Math.Abs(outBuf[i] - reference[i]));
                Console.WriteLine("buffers " + string.Join(",", pat) + ": max |diff| = " + md.ToString("R", Inv));
            }
            return 0;
        }

        /// <summary>Each generator alone at gain 1 for 60 s at a fixed s: its RMS (dB re unit) through the 20 Hz DC blocker.</summary>
        static int Unit(string[] a)
        {
            double fs = 48000, cr = fs / 32; uint seed = 1;
            double s = D(a, "--s", 300.0);
            var f = new ScoreFrame { Fs = fs, InvFs = 1 / fs, Block = 32, S = s, V = 2.5, Rho = NocturneScore.RainRhoSchedule(s), RhoRatio = 1, Beat = NocMath.BeatIndex(s) };
            NocLanes.Path.PointAt(s, out f.X, out f.Z); f.Yaw = NocLanes.Path.HeadingDeg(s);
            var rain = new RainBed(fs, cr, seed); var hum = new CityHum(fs, cr, seed); var tr = new TrafficWash(fs, cr, seed); var wind = new WindBed(fs, cr, seed);
            var koi = new KoiTone(fs, cr, seed); var leaf = new LeafRain(fs, cr, seed); var alley = new AlleyBed(fs, cr, seed); var canyon = new CanyonBed(fs, cr, seed);
            var tube = new TubeHum(fs, cr, seed); var screen = new ScreenHum(fs, cr, seed); var drips = new DripField(fs, cr, seed);
            string[] names = { "rain", "hum", "traffic", "wind", "koi", "leaf", "alley", "canyon", "tube", "screen", "drips" };
            var ss = new double[names.Length]; var dcs = new DcBlockD[names.Length]; for (int i = 0; i < dcs.Length; i++) dcs[i].Init(20, fs);
            long N = (long)(60 * fs), warm = (long)(5 * fs);
            for (long n = 0; n < N + warm; n++)
            {
                if (n % 32 == 0)
                {
                    f.N = n; f.T = n / fs; f.SceneT = f.T;
                    rain.Control(in f); hum.Control(in f); tr.Control(in f); wind.Control(in f); koi.Control(in f); leaf.Control(in f);
                    alley.Control(in f, 1.0); canyon.Control(in f, 1.0); tube.Control(in f, 1.0); screen.Control(in f, 1.0); drips.Control(in f);
                }
                double u = (n % 32) / 32.0, l, r;
                for (int i = 0; i < names.Length; i++)
                {
                    switch (i)
                    {
                        case 0: rain.Tick(u, n, out l, out r); break;
                        case 1: hum.Tick(u, out l, out r); break;
                        case 2: tr.Tick(u, out l, out r); break;
                        case 3: wind.Tick(u, out l, out r); break;
                        case 4: koi.Tick(u, out l, out r); break;
                        case 5: leaf.Tick(u, n, out l, out r); break;
                        case 6: alley.Tick(u, out l, out r); break;
                        case 7: canyon.Tick(u, out l, out r); break;
                        case 8: tube.Tick(u, out l, out r); break;
                        case 9: screen.Tick(u, out l, out r); break;
                        default: drips.Tick(u, out l, out r); break;
                    }
                    double y = dcs[i].Tick(0.5 * (l + r)); // mono check of the mean
                    if (n >= warm) ss[i] += 0.5 * (l * l + r * r);
                }
            }
            for (int i = 0; i < names.Length; i++) Console.WriteLine(FormattableString.Invariant($"{names[i],-8} {10 * Math.Log10(ss[i] / N):+0.00;-0.00} dB re unit (s {s})"));
            return 0;
        }

        static int Timing()
        {
            var st = new OfflineRenderSettings { S0 = 560.0, S1 = 720.0, Speed = 2.5, Seed = 1 };
            var sw = System.Diagnostics.Stopwatch.StartNew();
            var m = new MeterSink();
            long f = OfflineAudioRenderer.Render(st, m);
            double audio = f / 48000.0;
            Console.WriteLine(FormattableString.Invariant($"beat 4 (trains, wind, tube hum): {audio:0.0} s of audio in {sw.Elapsed.TotalSeconds:0.00} s = {100.0 * sw.Elapsed.TotalSeconds / audio:0.00} % of one core"));
            return 0;
        }
            /// <summary>The renders of AudioScoreTests (same AudioGates code) with their numbers, so the EditMode margins are known.</summary>
        static int Gates(string[] a)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            foreach (double v in new[] { 2.5, 1.5 })
                for (int b = 0; b < NocturneConstants.BeatCount; b++)
                {
                    var an = AudioGates.BeatStretch(b, v);
                    double med = an.BeatMedianDb(b, out int nw);
                    Console.WriteLine(FormattableString.Invariant($"level v {v:0.0} beat {b + 1} s {AudioGates.GateS0[b]:0}+: median {med:0.00} target {NocturneConstants.TargetRmsDb[b]:0.0} dev {med - NocturneConstants.TargetRmsDb[b]:+0.00;-0.00} ({nw} windows); TP {an.TruePeak.PeakDb:0.00} dBTP, DC {an.Dc:0.000000}, step {an.MaxStep:0.000}, nonfinite {an.NonFinite}"));
                }
            Console.WriteLine(FormattableString.Invariant($"  ({sw.Elapsed.TotalSeconds:0.0} s)"));
            sw.Restart();
            foreach (double v in new[] { 2.5, 1.5 })
                for (int c = 1; c < NocturneScore.ChordChangeS.Length; c++)
                {
                    var cm = AudioGates.PadChange(c, v, out WalkAnalysis an);
                    Console.WriteLine(FormattableString.Invariant($"pad solo v {v:0.0} C{c - 1}->C{c} at s {NocturneScore.ChordChangeS[c]:0}: d2 inside {cm.MaxInside:0.000000} outside {cm.MaxOutside:0.000000} (ratio {cm.MaxInside / Math.Max(1e-12, cm.MaxOutside):0.00}); env step {an.ChordMaxEnvStep[c]:0.00000} (rel {an.ChordMaxEnvRel[c]:0.000}), sample step {an.ChordMaxStep[c]:0.00000}, max step {an.MaxStep:0.00000}"));
                }
            Console.WriteLine(FormattableString.Invariant($"  ({sw.Elapsed.TotalSeconds:0.0} s)"));
            sw.Restart();
            foreach (double v in new[] { 2.5, 1.5 })
            {
                double s0 = 310.0, secs = 12.0;
                var off = new MeterSink();
                OfflineAudioRenderer.Render(new OfflineRenderSettings { S0 = s0, S1 = s0 + secs * v, Speed = v, Seed = 4, PrerollSeconds = 4.0, Footsteps = false }, off);
                var pl = new MeterSink();
                AudioGates.RenderPlayerPath(48000, 4, secs, 1024, 60.0, new ConstantWalkScript { S0 = s0, V = v }, 4.0, pl);
                Console.WriteLine(FormattableString.Invariant($"player path v {v:0.0}: offline {RmsSink.Db(off.Rms):0.000} dB, player {RmsSink.Db(pl.Rms):0.000} dB, diff {RmsSink.Db(pl.Rms) - RmsSink.Db(off.Rms):+0.000;-0.000}"));
            }
            {
                var sc = new PiecewiseWalkScript { S0 = 276.0, T = new[] { 0.0, 2.0, 8.0, 10.0, 16.0, 18.0, 21.0 }, V = new[] { 0.0, 2.5, 0.0, -1.5, 0.0, 10.0, 0.0 } };
                var an = new WalkAnalysis();
                AudioGates.RenderPlayerPath(48000, 2, 24.0, 512, 60.0, sc, 3.0, an);
                Console.WriteLine(FormattableString.Invariant($"speed script around C2 (s 276-306): step {an.MaxStep:0.000} at {an.MaxStepAt:0.00} s, chord env step {an.ChordMaxEnvStep[2]:0.00000}, nonfinite {an.NonFinite}, TP {an.TruePeak.PeakDb:0.00}"));
                var pad = new WalkAnalysis();
                Console.WriteLine(FormattableString.Invariant($"  ({sw.Elapsed.TotalSeconds:0.0} s)"));
            }
            return 0;
        }

        static int Alloc()
        {
            var mixer = new NocMixer(48000, 1);
            mixer.Motion = new WalkMotion { S0 = 560.0, V0 = 2.5 };
            mixer.AutoSteps = true;
            var buf = new float[2048];
            for (int i = 0; i < 400; i++) mixer.Process(buf, 2);
            long a0 = GC.GetAllocatedBytesForCurrentThread();
            for (int i = 0; i < 4000; i++) mixer.Process(buf, 2);
            long a1 = GC.GetAllocatedBytesForCurrentThread();
            var m2 = new NocMixer(48000, 1);
            var p = new NocAudioParams { master = 1f, s = 1300.0, speed = 2.5, rain = 1f };
            for (int i = 0; i < 400; i++) { p.seq++; p.s += 0.05; if (i % 30 == 0) p.footsteps++; m2.Process(buf, 2, in p); }
            long b0 = GC.GetAllocatedBytesForCurrentThread();
            for (int i = 0; i < 4000; i++) { p.seq++; p.s += 0.05; if (i % 30 == 0) p.footsteps++; m2.Process(buf, 2, in p); }
            long b1 = GC.GetAllocatedBytesForCurrentThread();
            Console.WriteLine($"offline path: {a1 - a0} bytes over 4000 callbacks; player path: {b1 - b0} bytes over 4000 callbacks");
            return 0;
        }
            /// <summary>Score functions at a glance: the koi law, the vehicle meets at both speeds.</summary>
        static int Probe()
        {
            Console.WriteLine(FormattableString.Invariant($"koi peak s {NocturneScore.KoiPeakS():0.00}"));
            for (double s = 1200; s <= 1500; s += 10) Console.Write(FormattableString.Invariant($"{s:0}:{20 * Math.Log10(Math.Max(1e-12, NocturneScore.KoiAmp(s))):0.0} "));
            Console.WriteLine();
            for (int i = 0; i < NocturneScore.Vehicles.Length; i++)
                Console.WriteLine(FormattableString.Invariant($"{NocturneScore.Vehicles[i].Name}: meet s {NocturneScore.MeetS(i, 2.5):0.00} at 2.5, {NocturneScore.MeetS(i, 1.5):0.00} at 1.5, diff {NocturneScore.MeetS(i, 2.5) - NocturneScore.MeetS(i, 1.5):0.00}"));
            return 0;
        }
    }
}
