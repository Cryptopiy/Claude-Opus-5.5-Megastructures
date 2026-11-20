// NocAlloc: managed (GC) allocation per frame on the main thread (Part 8 "no per-frame allocations"; Phase 3 integration task:
// 0 B per frame in steady state). One PlayerLoop system at the very start of Initialization reads the allocation counter once
// per frame, so the delta covers one whole frame cycle: scripts, UI Toolkit, input, physics and the render pipeline (the Render
// Graph records on the main thread). Steady state = after the load fade (GradeState.FadeBlack <= 0.001 with the world ready)
// plus SettleSeconds of real time; QA (AutoTestRunner, PerfMonitor) read the statistics.
//   Counter: GC.GetAllocatedBytesForCurrentThread when it advances (checked at install with a 4 KiB probe array: "exact"; Unity's
//   Boehm Mono returns 0, seen in I0-alloc01), else in a development player the ProfilerRecorder "GC Allocated In Frame"
//   ("recorder": exact bytes per frame), else the positive deltas of Profiler.GetMonoUsedSizeLong ("mono-heap": the Boehm heap
//   grows in whole blocks of 4-16 KiB, so single frames read 0 or one block; the mean over many frames is the allocation rate).
//   The probe attribution needs a running counter, so it uses "exact" or "mono-heap" (never the per-frame recorder).
// -alloc-probe: attribution. A probe after every PlayerLoop subsystem (two levels: phase.subsystem) adds the bytes since the
//   previous probe to that subsystem; NocAlloc.Section(i) / EndSection(i) pairs (a few named sections inside our own code)
//   split a subsystem further. The table (bytes per steady frame, top entries) is logged every ProbeLogSeconds and at quit.
//   In "mono-heap" mode the probe disables the GC during the steady window, so the block growth is attributed statistically
//   (and to whatever runs when another thread allocates: the attribution of I0-alloc01/02 was not usable).
// -alloc-bisect (development player, "recorder" mode): attribution by elimination. After the steady start every enabled
//   Behaviour of the scene (the camera = all rendering), every world updater (ProgressDirector skips BisectSkipUpdater) and every
//   active renderer feature is switched off in turn for BisectFrames frames (after BisectSettle frames), with baseline windows
//   between; the mean bytes per frame of each window is logged, highest reduction first. Diagnostics only.
// -alloc-profile <file> (development player): ProfileFrames frames of the Unity profiler into <file>.raw once the steady
//   window starts (Profiler.logFile + enableBinaryLog); BatchTools.AnalyzeGcProfile (-profile-in <file>.raw) loads it in the
//   Editor and sums every GC.Alloc sample by its marker stack (thread, PlayerLoop phase, script method): exact attribution.
// Zero allocations per frame itself (fixed arrays, delegates created once at install).
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.Profiling;
using UnityEngine.Scripting;

namespace Megastructure
{
    public static class NocAlloc
    {
        /// <summary>Real seconds after the load fade before the steady window starts (warm caches, first-use lazies).</summary>
        public const double SettleSeconds = 2.0;
        public const double ProbeLogSeconds = 10.0;
        public const string ProbeFlag = "-alloc-probe";

        /// <summary>"exact" (GC.GetAllocatedBytesForCurrentThread), "recorder" (development player: ProfilerRecorder "GC Allocated
        /// In Frame") or "mono-heap" (Mono used-size growth, block granular).</summary>
        public static string Mode { get; private set; } = "none";
        /// <summary>True when single frames are measured exactly ("exact" or "recorder"): then "frames allocating" is meaningful.</summary>
        public static bool PerFrameExact => Exact || s_recorderOk;
        public static bool Exact { get; private set; }
        public static bool Installed { get; private set; }
        /// <summary>Bytes allocated on the main thread during the last complete frame.</summary>
        public static long LastFrameBytes { get; private set; }
        /// <summary>Steady window (after the load fade + SettleSeconds): frames, total bytes, worst frame, frames that allocated.</summary>
        public static long SteadyFrames { get; private set; }
        public static long SteadyBytes { get; private set; }
        public static long SteadyMaxFrameBytes { get; private set; }
        public static long SteadyAllocFrames { get; private set; }
        public static int SteadyCollections => s_gc0 >= 0 ? GC.CollectionCount(0) - s_gc0 : 0;
        public static bool Steady => s_steadySince >= 0;
        /// <summary>Real seconds since the steady window started (0 before).</summary>
        public static double SteadySeconds => s_steadySince >= 0 ? Time.realtimeSinceStartupAsDouble - s_steadySince : 0.0;
        public static double SteadyBytesPerFrame => SteadyFrames > 0 ? SteadyBytes / (double)SteadyFrames : 0.0;

        /// <summary>
        /// The engine's own steady allocation that no game code can remove: Unity boxes the bool returned by the managed
        /// NativeInputSystem.NotifyBeforeUpdate before every native input update (one update per frame after
        /// NocInput.RestrictNativeUpdates). 17 B as the development player's recorder counts it (I0-alloc10: 18.8 B/frame with
        /// the log lines), one 32 B Boehm size class in the Mono heap (16-byte granules; release I0-alloc11: 31.5 B/frame).
        /// </summary>
        public const double EngineBoxBytesRecorder = 17.0, EngineBoxBytesHeap = 32.0;
        public static double EngineBytesPerFrame => PerFrameExact ? EngineBoxBytesRecorder : EngineBoxBytesHeap;
        /// <summary>Steady bytes per frame above the engine constant (0 when our code allocates nothing per frame).</summary>
        public static double SteadyExcessBytesPerFrame => Math.Max(0.0, SteadyBytesPerFrame - EngineBytesPerFrame);

        static long s_last = -1;
        static double s_fadeDoneAt = -1, s_steadySince = -1;
        static int s_gc0 = -1;
        static bool s_probe;
        // profiler capture (-alloc-profile)
        public const string ProfileFlag = "-alloc-profile";
        const int ProfileFrames = 300;
        static string s_profilePath;
        static int s_profileFrame = -1;
        // bisect (-alloc-bisect)
        public const string BisectFlag = "-alloc-bisect";
        const int BisectSettle = 10, BisectFrames = 90;
        /// <summary>Index of the world updater ProgressDirector skips (-1: none; set by the bisect).</summary>
        public static int BisectSkipUpdater = -1;
        static bool s_bisect;
        static List<string> s_bisNames;
        static List<Action<bool>> s_bisToggles;
        static double[] s_bisBytes;
        static int[] s_bisFrames;
        static int s_bisStep = -1, s_bisFrame;
        static double s_bisBaseBytes;
        static int s_bisBaseFrames;
        static Unity.Profiling.ProfilerRecorder s_rec;
        static bool s_recorderOk;
        static bool s_totalOk;   // GC.GetTotalAllocatedBytes(true) counts single allocations (all threads)
        static Func<bool, long> s_total;

        // probe tables (sized at install)
        static string[] s_names;
        static long[] s_bytes;
        static long[] s_hits;
        static long s_probeLast;
        static double s_nextLog;
        const int MaxSections = 16;
        static readonly long[] s_secStart = new long[MaxSections];
        static int s_sectionBase = -1;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics()
        {
            s_last = -1; s_fadeDoneAt = -1; s_steadySince = -1; s_gc0 = -1;
            LastFrameBytes = 0; SteadyFrames = 0; SteadyBytes = 0; SteadyMaxFrameBytes = 0; SteadyAllocFrames = 0;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void Install()
        {
            if (Installed) return;
            DetectMode();
            s_probe = CliArgs.Has(ProbeFlag);
            s_bisect = CliArgs.Has(BisectFlag) && !s_probe;
            s_profilePath = Debug.isDebugBuild ? QaPaths.Resolve(CliArgs.Get(ProfileFlag)) : null;
            if (!Exact && !s_totalOk && !s_probe && Debug.isDebugBuild)
            {
                try
                {
                    s_rec = Unity.Profiling.ProfilerRecorder.StartNew(Unity.Profiling.ProfilerCategory.Memory, "GC Allocated In Frame");
                    s_recorderOk = s_rec.Valid;
                }
                catch (Exception) { s_recorderOk = false; }
                if (s_recorderOk) Mode = "recorder";
            }
            var loop = PlayerLoop.GetCurrentPlayerLoop();
            if (loop.subSystemList == null || loop.subSystemList.Length == 0) return;
            if (s_probe) InstrumentProbes(ref loop);
            // the frame counter goes first in Initialization (after the probes were placed, so it is never instrumented itself)
            for (int i = 0; i < loop.subSystemList.Length; i++)
            {
                if (loop.subSystemList[i].type != typeof(UnityEngine.PlayerLoop.Initialization)) continue;
                var phase = loop.subSystemList[i];
                var subs = new List<PlayerLoopSystem>(phase.subSystemList ?? new PlayerLoopSystem[0]);
                subs.Insert(0, new PlayerLoopSystem { type = typeof(NocAllocFrame), updateDelegate = OnFrameStart });
                phase.subSystemList = subs.ToArray();
                loop.subSystemList[i] = phase;
                break;
            }
            PlayerLoop.SetPlayerLoop(loop);
            Installed = true;
            Application.quitting += OnQuit;
            Debug.Log("[NOC] alloc: per-frame managed allocation counter installed (" + Mode + (s_probe ? ", probe on " + s_names.Length + " PlayerLoop subsystems" : "") + ")");
        }

        static void OnQuit()
        {
            Debug.Log("[NOC] alloc at quit: " + Describe());
            LogProbe("quit");
            if (s_bisect && s_bisStep < 2 * (s_bisNames != null ? s_bisNames.Count : 0)) BisectLog("partial, at quit");
            if (s_recorderOk) { s_rec.Dispose(); s_recorderOk = false; }
        }

        static void DetectMode()
        {
            // which running counters see a 4 KiB array (logged once; diagnostics of the counter choice)
            try
            {
                // GC.GetTotalAllocatedBytes(bool) is not in the .NET Standard 2.1 surface: bound by reflection when the runtime has it
                var mi = typeof(GC).GetMethod("GetTotalAllocatedBytes", new[] { typeof(bool) });
                if (mi != null) s_total = (Func<bool, long>)Delegate.CreateDelegate(typeof(Func<bool, long>), mi);
                long t0 = s_total != null ? s_total(true) : 0, m0 = GC.GetTotalMemory(false), u0 = Profiler.GetMonoUsedSizeLong();
                var arr = new byte[4096];
                long t1 = s_total != null ? s_total(true) : 0, m1 = GC.GetTotalMemory(false), u1 = Profiler.GetMonoUsedSizeLong();
                GC.KeepAlive(arr);
                s_totalOk = s_total != null && t1 - t0 >= 4096 && t1 - t0 < 8192;
                Debug.Log("[NOC] alloc counters for a 4096 B array: GetTotalAllocatedBytes(true) " + (s_total != null ? "+" + (t1 - t0) : "absent") +
                          ", GetTotalMemory +" + (m1 - m0) + ", GetMonoUsedSizeLong +" + (u1 - u0));
            }
            catch (Exception e) { s_total = null; s_totalOk = false; Debug.Log("[NOC] alloc counters: " + e.GetType().Name); }
            long a = 0, b = 0;
            try
            {
                a = GC.GetAllocatedBytesForCurrentThread();
                var probe = new byte[4096];
                b = GC.GetAllocatedBytesForCurrentThread();
                GC.KeepAlive(probe);
            }
            catch (Exception) { a = b = 0; }
            Exact = b - a >= 4096;
            Mode = Exact ? "exact" : s_totalOk ? "total" : "mono-heap";
        }

        static long Counter() => Exact ? GC.GetAllocatedBytesForCurrentThread() : s_totalOk ? s_total(true) : Profiler.GetMonoUsedSizeLong();

        static void OnFrameStart()
        {
            long now = Counter();
            if (s_last >= 0)
            {
                long d = s_recorderOk ? s_rec.LastValue : now - s_last;
                LastFrameBytes = d > 0 ? d : 0;   // mono-heap mode: a collection shrinks the used size
                if (s_steadySince >= 0)
                {
                    SteadyFrames++;
                    SteadyBytes += LastFrameBytes;
                    if (LastFrameBytes > 0) SteadyAllocFrames++;
                    if (LastFrameBytes > SteadyMaxFrameBytes) SteadyMaxFrameBytes = LastFrameBytes;
                    if (s_bisect) BisectFrame(LastFrameBytes);
                    if (s_profilePath != null) ProfileFrame();
                }
            }
            UpdateSteady();
            s_last = Counter();   // the bookkeeping above never allocates, but keep the next frame exact anyway
        }

        static void UpdateSteady()
        {
            if (s_steadySince >= 0) return;
            double t = Time.realtimeSinceStartupAsDouble;
            if (s_fadeDoneAt < 0)
            {
                if (NocRig.WorldReady && GradeState.FadeBlack <= 0.001f) s_fadeDoneAt = t;
                return;
            }
            if (t - s_fadeDoneAt < SettleSeconds) return;
            s_steadySince = t;
            s_gc0 = GC.CollectionCount(0);
            if (s_probe)
            {
                Array.Clear(s_bytes, 0, s_bytes.Length);
                Array.Clear(s_hits, 0, s_hits.Length);
                s_nextLog = t + ProbeLogSeconds;
                if (!Exact && !s_totalOk) UnityEngine.Scripting.GarbageCollector.GCMode = UnityEngine.Scripting.GarbageCollector.Mode.Disabled;
            }
        }

        /// <summary>One line for reports: mode, steady frames, mean / worst bytes per frame, frames that allocated, collections.</summary>
        public static string Describe()
        {
            var inv = CultureInfo.InvariantCulture;
            return "mode " + Mode + ", steady frames " + SteadyFrames.ToString(inv) + ", mean " + SteadyBytesPerFrame.ToString("0.0", inv) +
                   " B/frame (engine input box " + EngineBytesPerFrame.ToString("0", inv) + ", excess " + SteadyExcessBytesPerFrame.ToString("0.0", inv) + "), worst " + SteadyMaxFrameBytes.ToString(inv) + " B, frames allocating " + SteadyAllocFrames.ToString(inv) +
                   ", total " + SteadyBytes.ToString(inv) + " B, GC collections " + SteadyCollections.ToString(inv);
        }

        // ------------------------------------------------------------------------------------------------ profile (-alloc-profile)
        static void ProfileFrame()
        {
            if (s_profileFrame < 0)
            {
                try
                {
                    System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(System.IO.Path.GetFullPath(s_profilePath)));
                    Profiler.logFile = s_profilePath;
                    Profiler.enableBinaryLog = true;
                    Profiler.enabled = true;
                    Debug.Log("[NOC] alloc profile: recording " + ProfileFrames + " frames -> " + s_profilePath + ".raw");
                }
                catch (Exception e) { Debug.LogWarning("[NOC] alloc profile: " + e.Message); s_profilePath = null; return; }
                s_profileFrame = 0;
                return;
            }
            if (++s_profileFrame < ProfileFrames) return;
            Profiler.enabled = false;
            Profiler.enableBinaryLog = false;
            Profiler.logFile = "";
            Debug.Log("[NOC] alloc profile: done (" + ProfileFrames + " frames)");
            s_profilePath = null;
        }

        // ------------------------------------------------------------------------------------------------ bisect (-alloc-bisect)
        static void BisectSetup()
        {
            s_bisNames = new List<string>();
            s_bisToggles = new List<Action<bool>>();
            foreach (var b in UnityEngine.Object.FindObjectsByType<Behaviour>(FindObjectsInactive.Exclude))
            {
                if (b == null || !b.isActiveAndEnabled || b is Light) continue;   // lights run no managed code
                var bb = b;
                s_bisNames.Add(b.gameObject.name + ":" + b.GetType().Name);
                s_bisToggles.Add(on => { if (bb != null) bb.enabled = on; });
            }
            var gb = GameBootstrap.Instance;
            if (gb != null && gb.Context != null)
                for (int i = 0; i < gb.Context.Updaters.Count; i++)
                {
                    int k = i;
                    s_bisNames.Add("updater:" + gb.Context.Updaters[i].GetType().Name);
                    s_bisToggles.Add(on => { BisectSkipUpdater = on ? -1 : k; });
                }
            // engine-level suspects: the Input System's automatic updates (events then wait in the queue for the window)
            s_bisNames.Add("input:automatic updates");
            s_bisToggles.Add(on => UnityEngine.InputSystem.InputSystem.settings.updateMode = on
                ? UnityEngine.InputSystem.InputSettings.UpdateMode.ProcessEventsInDynamicUpdate
                : UnityEngine.InputSystem.InputSettings.UpdateMode.ProcessEventsManually);
            foreach (var f in Resources.FindObjectsOfTypeAll<UnityEngine.Rendering.Universal.ScriptableRendererFeature>())
            {
                if (f == null || !f.isActive) continue;
                var ff = f;
                s_bisNames.Add("feature:" + f.GetType().Name);
                s_bisToggles.Add(on => ff.SetActive(on));
            }
            s_bisBytes = new double[s_bisNames.Count];
            s_bisFrames = new int[s_bisNames.Count];
            s_bisStep = -1;   // baseline first
            s_bisFrame = 0;
            Debug.Log("[NOC] alloc bisect: " + s_bisNames.Count + " suspects, " + BisectFrames + " frames each (recorder " + s_recorderOk + ")");
        }

        /// <summary>Steps: -1 baseline, then suspect k off (odd steps) and a baseline window (even steps) alternately.</summary>
        static void BisectFrame(long bytes)
        {
            if (s_bisNames == null) BisectSetup();
            if (s_bisStep >= 2 * s_bisNames.Count) return;
            s_bisFrame++;
            if (s_bisFrame > BisectSettle)
            {
                if (s_bisStep >= 0 && (s_bisStep & 1) == 1) { int k = s_bisStep >> 1; s_bisBytes[k] += bytes; s_bisFrames[k]++; }
                else { s_bisBaseBytes += bytes; s_bisBaseFrames++; }
            }
            if (s_bisFrame < BisectSettle + BisectFrames) return;
            // close the window
            if (s_bisStep >= 0 && (s_bisStep & 1) == 1) { try { s_bisToggles[s_bisStep >> 1](true); } catch (Exception) { } }
            s_bisStep++;
            s_bisFrame = 0;
            if (s_bisStep >= 2 * s_bisNames.Count) { BisectLog("complete"); return; }
            if ((s_bisStep & 1) == 1) { try { s_bisToggles[s_bisStep >> 1](false); } catch (Exception) { } }
        }

        static void BisectLog(string why)
        {
            if (s_bisNames == null) return;
            var inv = CultureInfo.InvariantCulture;
            double baseMean = s_bisBaseFrames > 0 ? s_bisBaseBytes / s_bisBaseFrames : 0;
            var idx = new List<int>();
            for (int i = 0; i < s_bisNames.Count; i++) if (s_bisFrames[i] > 0) idx.Add(i);
            idx.Sort((a, b) => (s_bisFrames[a] > 0 ? s_bisBytes[a] / s_bisFrames[a] : baseMean).CompareTo(s_bisFrames[b] > 0 ? s_bisBytes[b] / s_bisFrames[b] : baseMean));
            var sb = new StringBuilder();
            sb.Append("[NOC] alloc bisect (").Append(why).Append(", ").Append(idx.Count.ToString(inv)).Append(" of ").Append(s_bisNames.Count.ToString(inv))
              .Append(" suspects measured): baseline ").Append(baseMean.ToString("0.00", inv)).Append(" B/frame over ").Append(s_bisBaseFrames.ToString(inv)).Append(" frames");
            foreach (int i in idx)
            {
                double m = s_bisFrames[i] > 0 ? s_bisBytes[i] / s_bisFrames[i] : double.NaN;
                sb.Append("\n  without ").Append(s_bisNames[i]).Append(": ").Append(m.ToString("0.00", inv)).Append(" B/frame (")
                  .Append((m - baseMean).ToString("+0.00;-0.00;0.00", inv)).Append(")");
            }
            Debug.Log(sb.ToString());
        }

        // ------------------------------------------------------------------------------------------------ probe (-alloc-probe)
        /// <summary>Opens named section i (0..15) inside our own code (probe mode only; free otherwise).</summary>
        public static void Section(int i)
        {
            if (!s_probe || s_sectionBase < 0 || (uint)i >= MaxSections) return;
            s_secStart[i] = Counter();
        }

        /// <summary>Closes section i: its bytes move from the enclosing PlayerLoop subsystem to the section's row.</summary>
        public static void EndSection(int i, string name)
        {
            if (!s_probe || s_sectionBase < 0 || (uint)i >= MaxSections || s_steadySince < 0) return;
            long d = Counter() - s_secStart[i];
            if (d <= 0) return;
            int row = s_sectionBase + i;
            if (s_names[row] == null) s_names[row] = "section." + name;   // first use only (a probe-mode allocation)
            s_bytes[row] += d;
            s_hits[row]++;
            s_probeLast += d;   // the enclosing subsystem's probe does not count these bytes again
        }

        static void InstrumentProbes(ref PlayerLoopSystem loop)
        {
            var names = new List<string>(256);   // row r = the subsystem just before probe r (the first one also gets the bytes between frames)
            var phases = loop.subSystemList;
            for (int i = 0; i < phases.Length; i++)
            {
                var phase = phases[i];
                string pn = phase.type != null ? phase.type.Name : "phase" + i;
                var subs = phase.subSystemList ?? new PlayerLoopSystem[0];
                var list = new List<PlayerLoopSystem>(subs.Length * 2 + 1);
                for (int j = 0; j < subs.Length; j++)
                {
                    list.Add(subs[j]);
                    int row = names.Count;
                    names.Add(pn + "." + (subs[j].type != null ? subs[j].type.Name : "sub" + j));
                    PlayerLoopSystem.UpdateFunction f = () => Probe(row);
                    list.Add(new PlayerLoopSystem { type = typeof(NocAllocProbeMarker), updateDelegate = f });
                }
                phase.subSystemList = list.ToArray();
                phases[i] = phase;
            }
            loop.subSystemList = phases;
            s_sectionBase = names.Count;
            for (int k = 0; k < MaxSections; k++) names.Add(null);
            s_names = names.ToArray();
            s_bytes = new long[s_names.Length];
            s_hits = new long[s_names.Length];
            s_probeLast = Counter();
        }

        static void Probe(int row)
        {
            long now = Counter();
            long d = now - s_probeLast;
            if (s_steadySince >= 0 && d > 0) { s_bytes[row] += d; s_hits[row]++; }
            s_probeLast = now;
            if (s_steadySince >= 0 && Time.realtimeSinceStartupAsDouble >= s_nextLog && row == s_sectionBase - 1)
            {
                s_nextLog = Time.realtimeSinceStartupAsDouble + ProbeLogSeconds;
                LogProbe("periodic");
                s_probeLast = Counter();   // the log line itself is not attributed
            }
        }

        /// <summary>Logs the probe table (bytes per steady frame, highest first).</summary>
        public static void LogProbe(string why)
        {
            if (!s_probe || s_names == null) return;
            var inv = CultureInfo.InvariantCulture;
            var idx = new List<int>();
            for (int i = 0; i < s_names.Length; i++) if (s_bytes[i] > 0) idx.Add(i);
            idx.Sort((a, b) => s_bytes[b].CompareTo(s_bytes[a]));
            double frames = Math.Max(1, SteadyFrames);
            var sb = new StringBuilder();
            sb.Append("[NOC] alloc probe (").Append(why).Append(", ").Append(Mode).Append("): ").Append(SteadyFrames.ToString(inv))
              .Append(" steady frames, ").Append(SteadyBytesPerFrame.ToString("0.0", inv)).Append(" B/frame total");
            for (int k = 0; k < idx.Count && k < 24; k++)
            {
                int i = idx[k];
                sb.Append("\n  ").Append(s_names[i] ?? ("row" + i)).Append(": ").Append((s_bytes[i] / frames).ToString("0.00", inv))
                  .Append(" B/frame (").Append(s_hits[i].ToString(inv)).Append(" frames)");
            }
            Debug.Log(sb.ToString());
        }
    }

    /// <summary>PlayerLoop marker types (names in the profiler / PlayerLoop dumps).</summary>
    [Preserve] public struct NocAllocFrame { }
    [Preserve] public struct NocAllocProbeMarker { }
}
