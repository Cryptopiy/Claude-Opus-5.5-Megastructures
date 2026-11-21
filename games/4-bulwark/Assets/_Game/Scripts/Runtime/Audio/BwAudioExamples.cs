// BwAudioExamples: offline example renders of the kraken voice (one WAV per KrakenCallKind) and of the ending ride, through
// the SAME engine as the player (BwDsp), for the lead's listening pass and for AudioTests (levels, true peak, ducking).
// Unity-free: AudioTests calls it in EditMode; it writes QA/audio/kraken_<kind>.wav (+ .json) and QA/audio/ride.wav.
// Each kraken example: the listener on the path at a representative s of the kind (the bed of the score at that s, or
// silence with withBed false), facing along the path; the source at the kind's reference distance, 60 deg to the right
// (the sea side) and below the eye for the sea events (under water for the beat 6 kinds); the event starts at 2.0 s.
using System;
using System.Globalization;
using System.IO;
using System.Text;

namespace Megastructure.Audio
{
    public sealed class AudioExampleResult
    {
        public string Name;
        public double Seconds, PeakDb, MaxRms3Db, BedRms3Db = double.NegativeInfinity, MinDuckDb, MaxStep;
        public double EventStart, LevelTargetDb;
        public int NanResets;
        internal bool Triggered;
        public float[] Buffer;   // interleaved stereo (only when keepBuffer)

        public override string ToString()
        {
            var inv = CultureInfo.InvariantCulture;
            return Name + ": " + Seconds.ToString("0.0", inv) + " s, peak " + PeakDb.ToString("0.00", inv) + " dBFS, max 3 s RMS "
                   + MaxRms3Db.ToString("0.00", inv) + " dBFS (target " + LevelTargetDb.ToString("0.0", inv) + "), bed duck "
                   + MinDuckDb.ToString("0.0", inv) + " dB, max step " + MaxStep.ToString("0.000", inv);
        }
    }

    public static class BwAudioExamples
    {
        public const double EventStart = 3.5, Tail = 4.0;   // 3.5 s: the small-speaker gate compares with the 3 s before

        /// <summary>Representative listener s, distance (m), bearing right of the view (deg), height relative to the eye (m)
        /// and medium of each kind (the 2.4 schedule: K2 at s 350, K4 water at 760, K5 slap at 820, the strike in the window
        /// gallery, glass / suckers / eye at 1200-1255, the stinger from the foot).</summary>
        public static void ExampleFor(int kind, out double s, out double distance, out double bearing, out double height, out bool underwater)
        {
            underwater = false; bearing = 60.0; height = -8.0;
            distance = KrakenRecipe.RefDistance[Math.Max(0, Math.Min(KrakenRecipe.KindCount - 1, kind))];
            switch (kind)
            {
                case KrakenRecipe.Call: s = 350.0; height = -6.0; break;
                case KrakenRecipe.SurfaceRoar: s = 760.0; break;
                case KrakenRecipe.Slap: s = 820.0; break;
                case KrakenRecipe.DamShudder: s = 380.0; height = -40.0; break;
                case KrakenRecipe.GlassCreak: s = 1200.0; underwater = true; height = 0.0; break;
                case KrakenRecipe.SuckerPops: s = 1210.0; underwater = true; bearing = 85.0; height = 2.0; break;
                case KrakenRecipe.EyeSwell: s = 1255.0; underwater = true; bearing = 80.0; height = 0.0; break;
                case KrakenRecipe.Strike: s = 1300.0; bearing = 20.0; height = -6.0; break;   // the strike ahead on the glass (beat 6)
                default: s = 1499.0; bearing = 180.0; height = 3000.0; break;   // the stinger: from the dam, behind the walker
            }
        }

        /// <summary>Renders one kraken example. withBed: the score's bed at the example s (else the kraken bus alone).</summary>
        public static AudioExampleResult RenderKraken(int kind, string wavPath, bool withBed, int seed = 1, double strength = 1.0, bool keepBuffer = false)
            => RenderKraken(kind, wavPath, withBed, seed, strength, keepBuffer, false);

        /// <summary>underwaterCall: the call as heard in the window gallery (beat 6 at s 1230, 30 m out behind the glass: the
        /// muffled, close, pressing variant of K7 / 1228 / 1262 / 1312).</summary>
        public static AudioExampleResult RenderKraken(int kind, string wavPath, bool withBed, int seed, double strength, bool keepBuffer, bool underwaterCall)
        {
            ExampleFor(kind, out double s, out double dist, out double bearing, out double height, out bool underwater);
            if (underwaterCall) { s = 1230.0; dist = KrakenRecipe.RefDistanceUnderwater; bearing = 80.0; height = -4.0; underwater = true; }
            int fs = BwAudioSpec.SampleRate;
            double dur = KrakenRecipe.DefaultDuration[kind];
            double total = EventStart + dur + Tail;
            var dsp = new BwDsp(fs, seed);
            BwPath.EyeWorld(s, 0.0, out double lx, out double ly, out double lz);
            double yaw = BwPath.HeadingDeg(s);
            // the stinger is heard on the lantern platform (LEAD DECISION 1: 3.5 km west of the face, on the lowland), looking
            // back east at the dam; its bed is the platform's foot ambience (the ride arrived)
            bool platform = kind == KrakenRecipe.Stinger;
            var ride = platform ? BulwarkScore.SyntheticRide(BulwarkScore.SyntheticRideArrival + 30.0) : BwRideState.None;
            // the stinger's source (KrakenDirector: y 400 over the sea beyond the crest) is 6.4 km above the platform's eye
            if (platform) { lx = -3510.0; ly = -5998.3; lz = 1500.0; yaw = 90.0; bearing = 0.0; height = 6398.0; }
            double a = (yaw + bearing) * Math.PI / 180.0;
            double horiz = Math.Sqrt(Math.Max(0.0, dist * dist - height * height));
            var e = new KrakenEvent
            {
                kind = kind, strength = (float)strength, duration = (float)dur, underwater = underwater,
                x = (float)(lx + horiz * Math.Sin(a)), y = (float)(ly + height), z = (float)(lz + horiz * Math.Cos(a)), delay = 0f,
            };
            var res = new AudioExampleResult { Name = "kraken_" + KrakenRecipe.Names[kind] + (underwaterCall ? "_underwater" : ""), EventStart = EventStart };
            res.LevelTargetDb = KrakenRecipe.LevelDb(kind, dist, strength, underwater);
            RenderLoop(dsp, fs, total, res, wavPath, seed, keepBuffer, (double t, ref BwAudioParams p) =>
            {
                p = withBed ? BulwarkScore.Evaluate(s, t, 0.0, in ride) : BwAudioParams.Silent();
                p.master = 1f; p.impactBus = 1f; p.duck = 1f;
                p.lx = (float)lx; p.ly = (float)ly; p.lz = (float)lz; p.lyaw = (float)yaw;
                p.indoors = !platform && BulwarkScore.ListenerIndoors(s) ? 1f : 0f;
            }, (t, d) => { if (t >= EventStart && !res.Triggered) { d.TriggerKraken(e); res.Triggered = true; } });
            return res;
        }

        /// <summary>The ending ride (BulwarkScore.SyntheticRide: boarding, 75 s descent, transfer, 35 s shuttle, arrival) heard
        /// from the cabin, starting 2 s before the boarding on the ledge (s 1500) and ending 20 s after the arrival.</summary>
        public static AudioExampleResult RenderRide(string wavPath, int seed = 1, bool keepBuffer = false)
        {
            int fs = BwAudioSpec.SampleRate;
            double total = EventStart + BulwarkScore.SyntheticRideArrival + 20.0;
            var dsp = new BwDsp(fs, seed);
            double s = 1500.0;
            BwPath.EyeWorld(s, 0.0, out double lx, out double ly, out double lz);
            var res = new AudioExampleResult { Name = "ride", EventStart = EventStart, LevelTargetDb = -33.5 };
            int phase = 0;
            double prevSpeed = 0.0;
            RenderLoop(dsp, fs, total, res, wavPath, seed, keepBuffer, (double t, ref BwAudioParams p) =>
            {
                var ride = BulwarkScore.SyntheticRide(t - EventStart);
                p = BulwarkScore.Evaluate(s, t, 0.0, in ride);
                p.master = 1f; p.impactBus = 1f; p.duck = 1f;
                p.lx = (float)lx; p.ly = (float)ly; p.lz = (float)lz; p.lyaw = 270f;
            }, (t, d) =>
            {
                var ride = BulwarkScore.SyntheticRide(t - EventStart);
                int cue = RideCues.ForTransition(phase, ride.Phase);
                if (cue >= 0) d.TriggerRide((RideCue)cue);
                phase = ride.Phase;
                prevSpeed = ride.Speed;
            });
            return res;
        }

        /// <summary>RMS (dBFS) of the ride layers alone at a steady state (QA): mute = RideSound.Mute bits of the layers to drop.</summary>
        public static double RideSteadyRmsDb(int phase, float speed, float accel, float altitude, int mute, double seconds = 8.0, int seed = 1)
        {
            int fs = BwAudioSpec.SampleRate;
            var ride = new RideSound(fs, seed) { Mute = mute };
            var p = BwAudioParams.Silent();
            p.ridePhase = phase; p.rideMix = 1f; p.rideSpeed = speed; p.rideAccel = accel; p.rideAlt = altitude;
            long n = (long)(seconds * fs);
            double sum = 0.0;
            for (long i = 0; i < n; i++)
            {
                float l = 0f, r = 0f;
                ride.Tick(in p, ref l, ref r);
                if (i > fs) sum += 0.5 * ((double)l * l + (double)r * r);
            }
            return 10.0 * Math.Log10(Math.Max(1e-30, sum / Math.Max(1, n - fs)));
        }

        /// <summary>RMS (dBFS) of one BwBeds layer alone at gain 1 (unit-RMS check), over seconds of audio.</summary>
        public static double LayerUnitRmsDb(int layer, double seconds = 20.0, int seed = 1)
        {
            int fs = BwAudioSpec.SampleRate;
            var beds = new BwBeds(fs, seed);
            var p = BwAudioParams.Silent();
            p.windHz = 380f; p.seaLp = 700f; p.windLp = 20000f;
            switch (layer)
            {
                case BulwarkScore.LayerWind: p.w = 1f; break;
                case BulwarkScore.LayerSea: p.sea = 1f; break;
                case BulwarkScore.LayerFalls: p.falls = 1f; break;
                case BulwarkScore.LayerRoom: p.bed = 1f; break;
                case BulwarkScore.LayerBallast: p.tonal = 1f; break;
                case BulwarkScore.LayerPressure: p.pad = 1f; break;
                case BulwarkScore.LayerConduit: p.conduit = 1f; break;
                default: p.voice = 1f; break;
            }
            beds.Snap(in p);
            long n = (long)(seconds * fs);
            double sum = 0.0;
            for (long i = 0; i < n; i++)
            {
                if ((i & 31) == 0) beds.Control(in p);
                float l = 0f, r = 0f;
                beds.Tick(in p, ref l, ref r);
                if (i > fs) sum += 0.5 * ((double)l * l + (double)r * r);
            }
            return 10.0 * Math.Log10(Math.Max(1e-30, sum / Math.Max(1, n - fs)));
        }

        public delegate void ParamFn(double t, ref BwAudioParams p);

        static void RenderLoop(BwDsp dsp, int fs, double total, AudioExampleResult res, string wavPath, int seed, bool keepBuffer,
                               ParamFn paramFn, Action<double, BwDsp> events)
        {
            const int B = 512;
            long frames = (long)Math.Ceiling(total * fs);
            var buf = new float[B * 2];
            var win = new double[3 * fs];
            int wp = 0, wl = 0; double wsum = 0.0, peak = 0.0, maxRms = 0.0, maxStep = 0.0, minDuck = 1.0;
            float pl = 0f, pr = 0f;
            float[] keep = keepBuffer ? new float[frames * 2] : null;
            WavStreamWriter w = wavPath != null ? new WavStreamWriter(EnsureDir(wavPath), 2, fs, WavFormat.Float32, seed) : null;
            try
            {
                var p = BwAudioParams.Silent();
                bool first = true;
                for (long f = 0; f < frames; f += B)
                {
                    int n = (int)Math.Min(B, frames - f);
                    double t = f / (double)fs;
                    paramFn(t, ref p);
                    if (first) { dsp.SnapTo(in p); first = false; }
                    events(t, dsp);
                    if (buf.Length != n * 2) buf = new float[n * 2];
                    dsp.Process(buf, 2, in p);
                    double dk = dsp.Kraken.DuckGain;
                    if (dk < minDuck) minDuck = dk;
                    for (int i = 0; i < n; i++)
                    {
                        float l = buf[2 * i], r = buf[2 * i + 1];
                        double e2 = 0.5 * ((double)l * l + (double)r * r);
                        wsum += e2 - win[wp]; win[wp] = e2; if (++wp == win.Length) wp = 0; if (wl < win.Length) wl++;
                        if (wl == win.Length) { double rms = wsum / wl; if (rms > maxRms) maxRms = rms; }
                        double aa = Math.Max(Math.Abs(l), Math.Abs(r)); if (aa > peak) peak = aa;
                        double st = Math.Max(Math.Abs(l - pl), Math.Abs(r - pr)); if (st > maxStep) maxStep = st;
                        pl = l; pr = r;
                    }
                    if (keep != null) Array.Copy(buf, 0, keep, f * 2, n * 2);
                    if (w != null) w.WriteSamples(buf, 0, n * 2);
                }
            }
            finally { if (w != null) w.Dispose(); }
            res.Seconds = frames / (double)fs;
            res.PeakDb = BwAudioSpec.ToDb(peak);
            res.MaxRms3Db = 10.0 * Math.Log10(Math.Max(1e-30, maxRms));
            res.MaxStep = maxStep;
            res.MinDuckDb = BwAudioSpec.ToDb(minDuck);
            res.NanResets = dsp.NanResets;
            res.Buffer = keep;
            if (wavPath != null) WriteSidecar(Path.ChangeExtension(wavPath, ".json"), res, wavPath);
        }

        static string EnsureDir(string path)
        {
            string dir = Path.GetDirectoryName(Path.GetFullPath(path));
            if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
            return path;
        }

        static void WriteSidecar(string path, AudioExampleResult r, string wavPath)
        {
            var inv = CultureInfo.InvariantCulture;
            var sb = new StringBuilder();
            sb.Append("{\n  \"wav\": \"").Append(Path.GetFileName(wavPath)).Append("\",\n  \"stem\": \"example\",\n  \"eventStart\": ")
              .Append(r.EventStart.ToString("0.000", inv)).Append(",\n  \"peakDb\": ").Append(r.PeakDb.ToString("0.00", inv))
              .Append(",\n  \"maxRms3Db\": ").Append(r.MaxRms3Db.ToString("0.00", inv)).Append(",\n  \"targetDb\": ")
              .Append(r.LevelTargetDb.ToString("0.00", inv)).Append(",\n  \"minDuckDb\": ").Append(r.MinDuckDb.ToString("0.00", inv))
              .Append("\n}\n");
            File.WriteAllText(path, sb.ToString(), new UTF8Encoding(false));
        }
    }
}
