// GameBootstrap: builds the whole world behind the black load fade (MASTER_PROMPT 7.3, 7.4), imported from Thread.
// Beat builders are discovered by reflection (every non-abstract IBeatBuilder in the Runtime assembly, which includes the
// Beats folder through its asmref, ordered by Name), so builders compile and ship independently. After the build the
// WarmupLoader draws every material for 3 frames behind black (14.6; replaces Thread's Shader.WarmupAllShaders), then
// NocRig.WorldReady is set and NocEvents.LoadComplete / WorldReady are raised.
// Awake resets the static run state (NocState, NocEvents, InstancedRegistry, NocWetRegistry, NocCloudGlow) and applies the clock / dither flags:
// -dither static|temporal|off (static by default), -dither-amp X, -freeze-clock, -freeze-rain, -clock T, -grade-off, -tier N
// (WorldContext.Tier). NocCulture pins the invariant culture for every thread before the first scene loads, so every
// [NOC] log line and every QA report prints '.' decimals on any Windows locale (audit S3 item 14).
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Megastructure
{
    /// <summary>Invariant culture for the whole process (logs, reports, number parsing) from the first frame on.</summary>
    public static class NocCulture
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        public static void Apply()
        {
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
            CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
        }
    }

    [DefaultExecutionOrder(-1000)]
    public class GameBootstrap : MonoBehaviour
    {
        public MaterialLibrary materials;
        public Transform cityRoot;
        public Transform backdropRoot;
        public Camera mainCamera;

        public static GameBootstrap Instance { get; private set; }
        public static bool Ready { get; private set; }
        public static event Action WorldReady;
        public WorldContext Context { get; private set; }
        public double LoadSeconds { get; private set; }
        public int BuilderCount { get; private set; }
        public int BuilderFailures { get; private set; }

        void Awake()
        {
            Instance = this;
            Ready = false;
            NocState.Reset();
            NocEvents.Reset();
            NocRig.WorldReady = false;
            InstancedRegistry.Clear();
            NocWetRegistry.Clear();          // wet-floor sources and walls of a previous play session (14.7 H4)
            NocCloudGlow.Defaults();         // the 14.2.2 cloud glow set; builders may override it in Build
            GradeState.FadeBlack = 1f;
            NocClock.Reset(CliArgs.ClockStart);
            NocClock.Dither = CliArgs.Dither;
            NocClock.Frozen = CliArgs.FreezeClock;
            NocClock.FreezeRain = CliArgs.FreezeRain;
            bool off = NocClock.Dither == DitherMode.Off;
            GradeState.DitherPeak = off ? 0f : (float)CliArgs.DitherAmp;
            GradeState.DitherChroma = off ? 0f : (float)NocturneConstants.DitherChroma;
            NocState.AutoTest = CliArgs.AutoTest;
            NocState.Capture = CliArgs.Capturing;
            if (mainCamera == null) mainCamera = Camera.main;
            Debug.Log("[NOC] args: " + string.Join(" ", CliArgs.Args));
            Debug.Log(FormattableString.Invariant($"[NOC] dither {NocClock.Dither} peak {GradeState.DitherPeak:F2} chroma {GradeState.DitherChroma:F2}; clock start {CliArgs.ClockStart:F2} frozen={NocClock.Frozen} rain frozen={NocClock.FreezeRain} rain override {(CliArgs.Has("-rain") ? CliArgs.Rain.ToString("F2", System.Globalization.CultureInfo.InvariantCulture) : "none (schedule)")} tier {CliArgs.Tier}"));
            if (CliArgs.Has("-grade-off")) { GradeState.Enabled = false; Debug.Log("[NOC] diagnostics: NocGrade disabled (-grade-off)"); }
        }

        IEnumerator Start()
        {
            yield return null;   // let every system Awake/OnEnable and register its service
            var sw = Stopwatch.StartNew();
            if (cityRoot == null) cityRoot = new GameObject("City").transform;
            if (backdropRoot == null) backdropRoot = new GameObject("Backdrop").transform;
            var ctx = new WorldContext
            {
                Materials = materials,
                CityRoot = cityRoot,
                BackdropRoot = backdropRoot,
                Camera = mainCamera,
                Seed = CliArgs.Seed,
                Tier = CliArgs.Tier,
            };
            Context = ctx;
            if (materials == null) Debug.LogError("[NOC-FAIL] GameBootstrap: MaterialLibrary not assigned (run RebuildAll)");

            var builders = DiscoverBuilders();
            BuilderCount = builders.Count;
            ctx.Log(FormattableString.Invariant($"bootstrap: {builders.Count} builders: {string.Join(", ", builders.ConvertAll(b => b.Name))}"));
            foreach (var b in builders)
            {
                var bsw = Stopwatch.StartNew();
                long tris0 = ctx.Triangles;
                try { b.Build(ctx); }
                catch (Exception e) { BuilderFailures++; Debug.LogError(FormattableString.Invariant($"[NOC-FAIL] builder {b.Name} threw: {e}")); }
                ctx.Log(FormattableString.Invariant($"built {b.Name} in {bsw.Elapsed.TotalMilliseconds:F0} ms, {ctx.Triangles - tris0} triangles"));
            }
            ctx.Log(FormattableString.Invariant($"world built in {sw.Elapsed.TotalMilliseconds:F0} ms, {ctx.Triangles} triangles total, {ctx.Updaters.Count} updaters, {InstancedRegistry.Count} instanced batches, tier {ctx.Tier}"));

            var pd = ProgressDirector.Instance;
            if (pd != null) pd.Init(ctx);
            var bd = BeatDirector.Instance;
            if (bd != null) bd.Init(ctx);

            // shader warm-up behind black: every material drawn 3 frames (WarmupLoader, 14.6)
            GradeState.FadeBlack = 1f;
            var warm = new WarmupLoader();
            IEnumerator run = null;
            try { warm.Collect(materials); run = warm.Run(mainCamera); }
            catch (Exception e) { Debug.LogError(FormattableString.Invariant($"[NOC-FAIL] warm-up collect threw: {e}")); }
            while (run != null)
            {
                bool more;
                try { more = run.MoveNext(); }
                catch (Exception e) { Debug.LogError(FormattableString.Invariant($"[NOC-FAIL] warm-up threw: {e}")); break; }
                if (!more) break;
                yield return run.Current;
            }
            warm.Dispose();
            yield return null;

            LoadSeconds = sw.Elapsed.TotalSeconds;
            ctx.Log(FormattableString.Invariant($"load finished in {LoadSeconds:F2} s"));
            Ready = true;
            NocRig.WorldReady = true;
            try { NocEvents.RaiseLoadComplete(); }
            catch (Exception e) { Debug.LogError(FormattableString.Invariant($"[NOC-FAIL] LoadComplete handler threw: {e}")); }
            try { WorldReady?.Invoke(); }
            catch (Exception e) { Debug.LogError(FormattableString.Invariant($"[NOC-FAIL] WorldReady handler threw: {e}")); }
        }

        static List<IBeatBuilder> DiscoverBuilders()
        {
            var list = new List<IBeatBuilder>();
            foreach (var t in typeof(IBeatBuilder).Assembly.GetTypes())
            {
                if (t.IsAbstract || t.IsInterface || !typeof(IBeatBuilder).IsAssignableFrom(t)) continue;
                if (t.GetConstructor(Type.EmptyTypes) == null) continue;
                try { list.Add((IBeatBuilder)Activator.CreateInstance(t)); }
                catch (Exception e) { Debug.LogError(FormattableString.Invariant($"[NOC-FAIL] cannot create builder {t.Name}: {e.Message}")); }
            }
            list.Sort((a, b) => string.CompareOrdinal(a.Name, b.Name));
            return list;
        }

        void OnDestroy()
        {
            if (Instance == this) { Instance = null; Ready = false; NocRig.WorldReady = false; }
        }
    }
}
