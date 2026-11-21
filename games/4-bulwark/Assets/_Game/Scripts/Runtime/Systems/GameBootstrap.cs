// GameBootstrap: builds the whole world behind the black load fade (MASTER_PROMPT 7.3, 7.4, 8.6). Imported from Thread.
// Beat builders are discovered by reflection (every non-abstract IBeatBuilder in the Runtime assembly, which includes
// Assets/_Game/Beats through the asmref, ordered by Name), so builders compile and ship independently; each runs in its
// own try/catch. After the build: ProgressDirector.Init, the 14.6 set pre-warm (every registered set enabled for 3 frames
// at its key pose), then Abyss's WarmupLoader (every material drawn behind black in three lighting states; replaces
// Thread's Shader.WarmupAllShaders), then Ready / WorldReady / BwEvents.LoadComplete. LoadSeconds must stay below 15 s
// (T-LOAD). The cinematic flag survives Restart (The Ring's CinematicOnLoad).
// Phase 2: the only builder is the graybox of the whole path (Assets/_Game/Beats/Graybox/GrayboxPath.cs); beat builders
// replace its volumes beat by beat.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.SceneManagement;
using Megastructure.Rendering;
using Debug = UnityEngine.Debug;

namespace Megastructure
{
    [DefaultExecutionOrder(-1000)]
    public class GameBootstrap : MonoBehaviour
    {
        public MaterialLibrary materials;
        public BulwarkPalette palette;
        public Transform worldRoot;
        public Light sun;
        public Camera mainCamera;

        public static GameBootstrap Instance { get; private set; }
        public static bool Ready { get; private set; }
        public static event Action WorldReady;
        /// <summary>Set by Restart: the next load starts the cinematic again (The Ring).</summary>
        public static bool CinematicOnLoad;
        /// <summary>Set by Restart: the next load skips nothing but knows it is a restart (no -progress, s 0).</summary>
        public static bool Restarting;
        public WorldContext Context { get; private set; }
        public double LoadSeconds { get; private set; }
        public int BuilderCount { get; private set; }

        void Awake()
        {
            Instance = this;
            Ready = false;
            GradeState.ResetDefaults();
            // The load fade belongs to UiDirector (D-04: the single fade owner): its Awake holds the frame black and it keeps the
            // picture at 0 until BwEvents.LoadComplete; Start below asks it again through IFadeUI. Nothing here writes FadeBlack.
            BwClock.Reset();
            BwClock.ApplyCli();
            InstancedRegistry.Clear();
            GameState.CaptureMode = !string.IsNullOrEmpty(CliArgs.CaptureDir);
            GameState.AutoTest = CliArgs.AutoTest || CliArgs.WallBot || CliArgs.SoakSeconds > 0;
            GameState.Seed = (uint)Math.Max(1, CliArgs.Seed);
            if (CliArgs.TierOverride >= 0) DetailTier.TierOverride = (float)CliArgs.TierOverride;
            if (mainCamera == null) mainCamera = Camera.main;
            GameState.MainCamera = mainCamera;
            Debug.Log("[BW] args: " + string.Join(" ", CliArgs.Args));
            if (CliArgs.GradeOff) { GradeState.Enabled = false; Debug.Log("[BW] diagnostics: BwGrade disabled (-grade-off)"); }
        }

        IEnumerator Start()
        {
            yield return null;   // let every system Awake/OnEnable and register its service
            BwServices.Fade?.SetBlackImmediate(1f);   // the world is built behind black (through the fade owner, D-04)
            var sw = Stopwatch.StartNew();
            if (worldRoot == null)
            {
                var go = GameObject.Find("World");
                worldRoot = go != null ? go.transform : new GameObject("World").transform;
            }
            var ctx = new WorldContext
            {
                Materials = materials,
                Root = worldRoot,
                Camera = mainCamera,
                Seed = GameState.Seed,
            };
            Context = ctx;

            var builders = DiscoverBuilders();
            BuilderCount = builders.Count;
            ctx.Log($"bootstrap: {builders.Count} builders: {string.Join(", ", builders.ConvertAll(b => b.Name))}");
            foreach (var b in builders)
            {
                var bsw = Stopwatch.StartNew();
                long tris0 = ctx.Triangles;
                try { b.Build(ctx); }
                catch (Exception e) { Debug.LogError($"[BW-FAIL] builder {b.Name} threw: {e}"); }
                if (b is IKeyShotSubjects ks) ctx.RegisterSubjects(ks);
                ctx.Log($"built {b.Name} in {bsw.Elapsed.TotalMilliseconds:F0} ms, {ctx.Triangles - tris0} triangles");
            }
            foreach (var problem in ctx.MeshProblems) Debug.LogWarning("[BW] mesh check: " + problem);
            ctx.Log($"world built in {sw.Elapsed.TotalMilliseconds:F0} ms, {ctx.Triangles} triangles total, {ctx.Updaters.Count} updaters");

            var pd = ProgressDirector.Instance;
            if (pd != null) pd.Init(ctx);

            // 14.6 set pre-warm: every registered set for 3 frames at its key pose, behind black
            yield return PrewarmSets(ctx);

            // shader / pipeline-state warm-up behind black (Abyss WarmupLoader; stepped by hand so an exception never stops the load)
            var warm = new WarmupLoader();
            IEnumerator run = null;
            try { warm.Collect(materials); run = warm.Run(mainCamera, sun); }
            catch (Exception e) { Debug.LogError($"[BW-FAIL] shader warm-up could not start: {e}"); }
            while (run != null)
            {
                bool more;
                try { more = run.MoveNext(); }
                catch (Exception e) { Debug.LogError($"[BW-FAIL] shader warm-up threw: {e}"); more = false; }
                if (!more) break;
                yield return run.Current;
            }
            warm.Dispose();

            LoadSeconds = sw.Elapsed.TotalSeconds;
            ctx.Log($"load finished in {LoadSeconds:F2} s");
            if (LoadSeconds > FrameworkConst.LoadBudgetSeconds) Debug.LogWarning($"[BW] load took {LoadSeconds:F2} s (budget {FrameworkConst.LoadBudgetSeconds} s)");
            Ready = true;
            try { WorldReady?.Invoke(); BwEvents.RaiseLoadComplete(); }
            catch (Exception e) { Debug.LogError($"[BW-FAIL] WorldReady handler threw: {e}"); }
            Restarting = false;
        }

        IEnumerator PrewarmSets(WorldContext ctx)
        {
            var sets = ctx.PrewarmSets;
            if (sets.Count == 0) yield break;
            var cc = BwServices.CameraControl;
            for (int i = 0; i < sets.Count; i++)
            {
                var set = sets[i];
                try
                {
                    set.Enable?.Invoke(true);
                    if (cc != null) cc.SetPoseOverride(set.Eye, set.YawDeg, set.PitchDeg, 0f);
                }
                catch (Exception e) { Debug.LogError($"[BW-FAIL] pre-warm of set {set.Name} threw: {e}"); }
                for (int f = 0; f < 3; f++) yield return null;
                try { set.Enable?.Invoke(false); }
                catch (Exception e) { Debug.LogError($"[BW-FAIL] pre-warm of set {set.Name} threw on disable: {e}"); }
            }
            if (cc != null) cc.ClearPoseOverride();
            ctx.Log($"pre-warmed {sets.Count} sets");
        }

        /// <summary>Restart from s 0 (pause menu). The cinematic flag is carried through the reload.</summary>
        public static void Restart(bool cinematic)
        {
            CinematicOnLoad = cinematic;
            Restarting = true;
            Ready = false;
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        public static List<IBeatBuilder> DiscoverBuilders()
        {
            var list = new List<IBeatBuilder>();
            foreach (var t in typeof(IBeatBuilder).Assembly.GetTypes())
            {
                if (t.IsAbstract || t.IsInterface || !typeof(IBeatBuilder).IsAssignableFrom(t)) continue;
                if (t.GetConstructor(Type.EmptyTypes) == null) continue;
                try { list.Add((IBeatBuilder)Activator.CreateInstance(t)); }
                catch (Exception e) { Debug.LogError($"[BW-FAIL] cannot create builder {t.Name}: {e.Message}"); }
            }
            list.Sort((a, b) => string.CompareOrdinal(a.Name, b.Name));
            return list;
        }

        void OnDestroy()
        {
            if (Instance == this) { Instance = null; Ready = false; }
        }
    }
}
