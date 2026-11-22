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
                case "dread": return Dread(args);
                case "dreadcost": return DreadCost(args);
                case "stems": return Stems(args);
                case "echojump": return EchoJump();
                case "arkcheck": return ArkCheck(args);
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

        /// <summary>--solo RDHTWPKXFE letters -> NocMixer.SoloMask (null = everything).</summary>
        static int SoloOf(string letters)
        {
            if (string.IsNullOrEmpty(letters)) return -1;
            int m = 0;
            foreach (char ch in letters) { int k = "RDHTWPKXFE".IndexOf(char.ToUpperInvariant(ch)); if (k >= 0) m |= 1 << k; }
            return m;
        }

        /// <summary>The D-034 dread layer: the event table, the echo taps along the walk, the unit RMS of each voice at gain 1.</summary>
        static int Dread(string[] a)
        {
            Console.WriteLine("events (s, kind, Hz, dBFS, onset delay s, length s, position):");
            foreach (var e in DreadScore.Events)
                Console.WriteLine(FormattableString.Invariant($"  {e.S,7:0.0} {e.Kind,-6} {e.Hz,7:0.0} {e.Db,6:0.0} {e.Delay,5:0.00} {e.Seconds,4:0.0}  ({e.X:0}, {e.Y:0}, {e.Z:0})"));
            Console.WriteLine(FormattableString.Invariant($"ark (D-043 mirror): launch s {DreadScore.ArkLaunchS}, cruise {DreadScore.ArkCruise} m/s (speed at the bow pass {DreadScore.ArkVelocity(DreadScore.ArkBowOverTau):0.0}), bow over z 195 at tau {DreadScore.ArkBowOverTau:0.00} (s {DreadScore.ArkBowOverS:0.0}), stern over at tau {DreadScore.ArkSternOverTau:0.00} (s {DreadScore.ArkSternOverS:0.0}); bow z at tau 0 / 40 / 86.4 / 150: {DreadScore.ArkBowZ(0):0} / {DreadScore.ArkBowZ(40):0} / {DreadScore.ArkBowZ(86.4):0} / {DreadScore.ArkBowZ(150):0}"));
            {
                var gb = new StringBuilder("ark groans (tau / s at 2.5 / Hz / length):");
                for (int k = 0; k < DreadScore.ArkGroanTau.Length; k++) gb.Append(FormattableString.Invariant($" {DreadScore.ArkGroanTau[k]:0.0}/{DreadScore.ArkLaunchS + 2.5 * DreadScore.ArkGroanTau[k]:0.0}/{DreadScore.ArkGroanHz(k):0.0}/{DreadScore.ArkGroanSeconds(k):0.0}"));
                Console.WriteLine(gb.ToString());
            }
            Console.WriteLine(FormattableString.Invariant($"panel (PanelCourse): s {DreadScore.PanelS} at ({DreadScore.PanelX}, {DreadScore.PanelY}, {DreadScore.PanelZ})"));
            {
                var mb = new StringBuilder("vehicle meets at 2.5 m/s:");
                for (int i = 0; i < NocturneScore.Vehicles.Length; i++) mb.Append(FormattableString.Invariant($" {NocturneScore.Vehicles[i].Name} {NocturneScore.MeetS(i, 2.5):0.0}"));
                Console.WriteLine(mb.ToString());
                double worst = 1e9; string wk = "";
                foreach (var e in DreadScore.Events)
                {
                    if (e.Kind != DreadKind.Horn && e.Kind != DreadKind.Strike && e.Kind != DreadKind.Groan) continue;
                    for (int i = 0; i < NocturneScore.Vehicles.Length; i++) { double d = Math.Abs(e.S - NocturneScore.MeetS(i, 2.5)); if (d < worst) { worst = d; wk = FormattableString.Invariant($"{e.Kind} s {e.S:0.0} vs {NocturneScore.Vehicles[i].Name}"); } }
                }
                Console.WriteLine(FormattableString.Invariant($"closest horn / strike / groan to a meet: {worst:0.0} m ({wk})"));
            }
            Console.WriteLine("echo taps (x: A1 / B1 / AB2 delay s, gain dB, LP Hz; Gulf weight):");
            foreach (double x in new[] { 215.3, 230.0, 255.3, 375.0, 535.0, 695.0, 795.0, 815.3 })
            {
                var sb = new StringBuilder(FormattableString.Invariant($"  x {x,6:0.0}:"));
                for (int k = 0; k < DreadScore.EchoTapCount; k++)
                {
                    DreadScore.EchoTap(k, x, out double d, out double g, out double lp, out _);
                    sb.Append(FormattableString.Invariant($"  {d,5:0.000} s {20 * Math.Log10(Math.Max(g, 1e-9)),6:0.0} dB {lp,5:0} Hz"));
                }
                sb.Append(FormattableString.Invariant($"  w {DreadScore.EchoWeight(x):0.00}"));
                Console.WriteLine(sb.ToString());
            }
            string cues = Arg(a, "--cues", null);
            if (cues != null)
            {
                // onset times of every cue for a constant-speed walk from s 0 (Tools/audio/dread_analysis.py)
                double v = D(a, "--speed", 2.5);
                var sj = new StringBuilder("{\"speed\":" + v.ToString("R", Inv) + ",\"events\":[");
                bool first = true;
                foreach (var e in DreadScore.Events)
                {
                    double delay = e.Delay;
                    if (e.Kind == DreadKind.Groan) delay += DreadScore.NearestMass(e.S, e.Seq, out _, out _, out _) / DreadScore.SoundSpeed;
                    if (e.Ark != 0)
                    {
                        // the Ark's call: on the Ark's clock (tau = ArkCallTau), from the ship where it is then, the walker at speed v
                        // at s_L + v tau (skipped beyond its range); its onset time is s_L / v + tau + the flight
                        double ct = DreadScore.ArkCallTau(e.S);
                        D3 pp = NocMath.PathPoint(DreadScore.ArkLaunchS + v * ct);
                        DreadScore.ArkHornSource(e.Ark, ct, pp.x, pp.z, out _, out _, out _, out double hd, out _);
                        if (hd > DreadScore.ArkHornMaxDist) continue;
                        delay = hd / DreadScore.SoundSpeed + (DreadScore.ArkLaunchS / v + ct) - e.S / v;
                    }
                    if (!first) sj.Append(','); first = false;
                    sj.Append(FormattableString.Invariant($"{{\"kind\":\"{e.Kind}\",\"s\":{e.S:0.###},\"t\":{e.S / v + delay:0.###},\"len\":{e.Seconds:0.###},\"db\":{e.Db:0.##},\"hz\":{e.Hz:0.###}}}"));
                }
                sj.Append("],\"swells\":[");
                for (int k = 0; k < DreadScore.SwellS.Length; k++)
                {
                    if (k > 0) sj.Append(',');
                    sj.Append(FormattableString.Invariant($"{{\"name\":\"{DreadScore.SwellNames[k]}\",\"s\":{DreadScore.SwellS[k]:0.###},\"t\":{DreadScore.SwellS[k] / v:0.###},\"hz\":{DreadScore.SwellHz[k]:0.###},\"db\":{DreadScore.SwellPeakDb[k]:0.##}}}"));
                }
                sj.Append("],\"drops\":[");
                for (int k = 0; k < DreadScore.DropS.Length; k++) { if (k > 0) sj.Append(','); sj.Append(FormattableString.Invariant($"{{\"s\":{DreadScore.DropS[k]:0.###},\"t\":{DreadScore.DropS[k] / v:0.###}}}")); }
                sj.Append("],\"meets\":[");
                for (int i = 0; i < NocturneScore.Vehicles.Length; i++) { if (i > 0) sj.Append(','); sj.Append(FormattableString.Invariant($"{{\"name\":\"{NocturneScore.Vehicles[i].Name}\",\"s\":{NocturneScore.MeetS(i, 2.5):0.###}}}")); }
                sj.Append("],\"arkgroans\":[");
                for (int k = 0; k < DreadScore.ArkGroanTau.Length; k++)
                {
                    if (k > 0) sj.Append(',');
                    double tg = DreadScore.ArkLaunchS / v + DreadScore.ArkGroanTau[k] + (DreadScore.ArkBellyY - 1.7) / DreadScore.SoundSpeed;
                    sj.Append(FormattableString.Invariant($"{{\"t\":{tg:0.###},\"len\":{DreadScore.ArkGroanSeconds(k):0.###},\"hz\":{DreadScore.ArkGroanHz(k):0.###}}}"));
                }
                double tb = DreadScore.ArkLaunchS / v + DreadScore.ArkBowOverTau;
                sj.Append(FormattableString.Invariant($"],\"hull\":{{\"t0\":{tb - 20.0:0.###},\"t1\":{tb + 40.0:0.###}}},\"ark\":{{\"bow\":{tb:0.###},\"drop0\":{tb + DreadScore.ArkDropFall1:0.###},\"drop1\":{tb + DreadScore.ArkDropRet0:0.###},\"stern\":{DreadScore.ArkLaunchS / v + DreadScore.ArkSternOverTau:0.###}}}}}"));
                File.WriteAllText(cues, sj.ToString());
                Console.WriteLine("cues -> " + cues);
                return 0;
            }
            double fs = 48000.0;
            Console.WriteLine("unit (peak 1 s RMS at gain 1, dB; 0 = normalised):");
            foreach (DreadKind k in Enum.GetValues(typeof(DreadKind)))
                Console.WriteLine(FormattableString.Invariant($"  {k,-6} {20 * Math.Log10(Math.Max(DreadProbe.UnitRms(k, fs), 1e-12)),7:0.00} dB"));
            Console.WriteLine("per event (peak 1 s RMS at its DreadScore level, dBFS):");
            foreach (var e in DreadScore.Events)
            {
                var y = DreadProbe.RenderVoice(in e, fs);
                Console.WriteLine(FormattableString.Invariant($"  {e.S,7:0.0} {e.Kind,-6} unit {20 * Math.Log10(Math.Max(DreadProbe.PeakWindowRms(y, fs), 1e-12)),6:0.00} dB -> {e.Db + 20 * Math.Log10(Math.Max(DreadProbe.PeakWindowRms(y, fs), 1e-12)),6:0.0} dBFS"));
            }
            foreach (double x in new[] { 375.0, 535.0, 230.0 })
            {
                var ir = DreadProbe.EchoImpulse(x, fs, 4.0);
                var sb = new StringBuilder(FormattableString.Invariant($"  echo impulse x {x:0}:"));
                for (int k = 0; k < DreadScore.EchoTapCount; k++)
                {
                    DreadScore.EchoTap(k, x, out double d, out double g, out _, out _);
                    DreadProbe.Arrival(ir, fs, d, out double t, out double en);
                    sb.Append(FormattableString.Invariant($"  {t:0.0000} s ({d:0.0000}) {10 * Math.Log10(Math.Max(en, 1e-20)):0.0} dB"));
                }
                Console.WriteLine(sb.ToString());
            }
            return 0;
        }

        /// <summary>Cost of the dread layer's parts: seconds of CPU per second of audio, standing at --s (default 600).</summary>
        static int DreadCost(string[] a)
        {
            double fs = 48000.0, s0 = D(a, "--s", 600.0);
            foreach (int pm in new[] { 0, 1, 2, 4, 8, 16, 32, -1 })
            {
                var dl = new DreadLayer(fs, fs / NocMixer.Block, 1u) { PartMask = pm };
                D3 p = NocMath.PathPoint(s0);
                var f = new ScoreFrame { Fs = fs, InvFs = 1.0 / fs, Block = NocMixer.Block, S = s0, X = p.x, Z = p.z, Yaw = 90.0 };
                int n = (int)(10.0 * fs);
                var sw = System.Diagnostics.Stopwatch.StartNew();
                double acc = 0;
                for (int i = 0; i < n; i++)
                {
                    if (i % NocMixer.Block == 0) { f.N = i; f.T = i / fs; f.SceneT = f.T; f.S = s0 + 2.5 * f.T; NocMath.PathPoint(f.S); dl.Control(in f, 1.0); }
                    dl.Tick((i % NocMixer.Block) / 32.0, i, out double l, out double r, out _, out _); acc += l * l + r * r;
                }
                Console.WriteLine(FormattableString.Invariant($"  part mask {pm,3}: {sw.Elapsed.TotalSeconds / 10.0 * 100.0:0.00} % of a core  (ms {10 * Math.Log10(acc / n + 1e-30):0.0} dB)"));
            }
            return 0;
        }

        /// <summary>Stem levels in --bin second bins (default 0.5) over [s0, s1]: the dread cues in context.</summary>
        static int Stems(string[] a)
        {
            var st = Settings(a, true);
            st.PrerollSeconds = D(a, "--preroll", 4.0);
            var an = new WalkAnalysis();
            OfflineAudioRenderer.Render(st, an);
            int per = Math.Max(1, (int)Math.Round(D(a, "--bin", 0.5) / WalkAnalysis.BinSeconds));
            Console.WriteLine("   s     mix  |  R     D     H     T     W     P     K     X     F     E");
            for (int i = 0; i + per <= an.BinMs.Count; i += per)
            {
                double ms = 0; var acc = new double[NocturneScore.StemCount];
                for (int j = i; j < i + per; j++) { ms += an.BinMs[j]; var bs = an.BinStems[j]; for (int k = 0; k < acc.Length; k++) acc[k] += bs[k]; }
                var line = new StringBuilder(FormattableString.Invariant($"{an.BinS[i],7:0.0} {WalkAnalysis.Db(ms / per),6:0.0} |"));
                for (int k = 0; k < acc.Length; k++) line.Append(FormattableString.Invariant($" {Math.Max(-99, WalkAnalysis.Db(acc[k] / per)),5:0.0}"));
                Console.WriteLine(line.ToString());
            }
            return 0;
        }

        /// <summary>AudioDreadTests.EchoBus_GlidesWithTheWalkerWithoutClicks in the harness: second differences of a 220 Hz tone.</summary>
        static int EchoJump()
        {
            int Fs = 48000;
            var bus = new EchoBus(Fs, Fs / NocMixer.Block);
            var f = new ScoreFrame { Fs = Fs, InvFs = 1.0 / Fs, Block = NocMixer.Block, X = 450.0, Z = 195.0, Yaw = 90.0 };
            int n = 12 * Fs;
            double pl = 0, pl2 = 0, maxD2 = 0, maxD2Jump = 0;
            for (int i = 0; i < n; i++)
            {
                if (i % NocMixer.Block == 0) { f.N = i; f.X = 450.0 + 2.5 * i / (double)Fs + (i >= 9 * Fs ? 200.0 : 0.0); bus.Control(in f); }
                double u = (i % NocMixer.Block) / (double)NocMixer.Block;
                double x = 0.1 * Math.Sin(2.0 * Math.PI * 220.0 * i / Fs);
                bus.Tick(u, x, out double l, out _);
                if (i > 4 * Fs) { double d2 = Math.Abs(l - 2.0 * pl + pl2); if (i < 9 * Fs) maxD2 = Math.Max(maxD2, d2); else maxD2Jump = Math.Max(maxD2Jump, d2); }
                pl2 = pl; pl = l;
            }
            double sine = 0.1 * Math.Pow(2.0 * Math.PI * 220.0 / Fs, 2);
            Console.WriteLine(FormattableString.Invariant($"echo jump: walking {maxD2:0.000000} jump {maxD2Jump:0.000000} limit {3 * sine:0.000000}"));
            return 0;
        }

        /// <summary>The Ark along the walk at --speed (default 2.5): per --step m (default 10) from s 380 to 1 200 the bow's seconds,
        /// the bow / pod Dopplers, the drop and the cover, the drone's part gains, and the drone-only level (E, part mask 4) in 1 s.</summary>
        static int ArkCheck(string[] a)
        {
            double v = D(a, "--speed", 2.5), step = D(a, "--step", 10.0), s0 = D(a, "--s0", 380.0), s1 = D(a, "--s1", 1200.0);
            var st = new OfflineRenderSettings { S0 = s0, Speed = v, Seconds = (s1 - s0) / v, Seed = 1, Footsteps = false };
            var mixer = OfflineAudioRenderer.CreateMixer(st, out _);
            mixer.MeterStems = true; mixer.SoloMask = 1 << NocturneScore.E; mixer.Dread.PartMask = 4;
            int bin = 48000; var buf = new float[bin * 2];
            Console.WriteLine("   s     tau  bow_s   dopW   pod0  pod1  pod2  pod3  drop  cover  gBody gBow  gPod   E dBFS");
            double next = s0, worst = -300; int nb = (int)Math.Floor((s1 - s0) / v);
            for (int k = 0; k < nb; k++)
            {
                Array.Clear(mixer.StemSumSq, 0, mixer.StemSumSq.Length);
                mixer.Process(buf, 2);
                double db = 10.0 * Math.Log10(Math.Max(1e-30, mixer.StemSumSq[NocturneScore.E] / bin));
                worst = Math.Max(worst, db);
                double s = s0 + (k + 1) * v;
                if (s + 1e-9 < next) continue;
                next += step;
                var ark = mixer.Dread.Ark;
                Console.WriteLine(FormattableString.Invariant($"{s,7:0.0} {ark.Tau,6:0.0} {DreadScore.ArkBowSeconds(ark.Tau, NocMath.PathPoint(s).z),6:0.0} {ark.BowDoppler,6:0.000} {ark.PodDoppler[0],5:0.000} {ark.PodDoppler[1],5:0.000} {ark.PodDoppler[2],5:0.000} {ark.PodDoppler[3],5:0.000} {ark.Drop,5:0.00} {ark.Cover,5:0.00}  {ark.BodyGain,5:0.000} {ark.BowGain,5:0.000} {ark.PodGainMax,5:0.000}  {db,6:0.0}"));
            }
            Console.WriteLine(FormattableString.Invariant($"loudest 1 s of the drone {worst:0.0} dBFS (ArkPeakDb {DreadScore.ArkPeakDb}); groans fired {mixer.Dread.ArkGroansFired}"));
            return 0;
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
