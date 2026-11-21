// QaRigs: capture sets contributed by off-path QA rigs (Wave A1: the kraken look-dev rig Assets/_Game/Beats/QA_KrakenRig;
// the pattern of the "materials" set and QA_MaterialSheet, generalised so a rig owner never edits ShotPlan or CaptureHarness).
//
// How a rig uses it (in its IBeatBuilder.Build, which runs before the harness builds its plan after GameBootstrap.Ready):
//     if (!QaRigs.Requested("kraken")) return;                   // only with -capture <dir> and a -capture-set naming "kraken"
//     ... build the rig off-path (it changes the world, so it is never part of stills / all / everything) ...
//     QaRigs.Register("kraken", () => myStills, (w, h) => mySequences(w, h));
// ShotPlan.Build then expands the set name into those stills and sequences (filtered by -capture-only like every other
// set); a set name nobody registered stays an unknown set (a logged capture failure). The rig's StillShot / SequenceSpec
// carry the pose, s (ProgressDirector override: f.s) and the ambient clock (StillShot.ambient, SequenceSpec.ambient0 /
// advanceAmbient: f.ambientTime), so an animation keyed to the ambient clock is posed per shot deterministically.
// Per shot the harness publishes CurrentShot (the still's tag or the sequence's name; "" between shots) and CurrentFrame
// (the sequence frame index; 0 for stills), so a rig's OnFrame can switch its configuration per shot (a forced LOD, a debug
// view, a turntable angle) without new command-line flags. Rig-specific flags are read with CliArgs.Get / GetDouble /
// GetInt / Has (no CliArgs edit needed; document them in the rig's header and in docs/FRAMEWORK.md section 5).
// No UnityEngine dependency (EditMode tests build plans with it).
using System;
using System.Collections.Generic;

namespace Megastructure.QA
{
    public static class QaRigs
    {
        sealed class Entry
        {
            public Func<List<StillShot>> stills;
            public Func<int, int, List<SequenceSpec>> sequences;
        }

        static readonly Dictionary<string, Entry> s_sets = new Dictionary<string, Entry>(StringComparer.OrdinalIgnoreCase);

        /// <summary>Tag of the still or name of the sequence the harness is posing now ("" outside a shot).</summary>
        public static string CurrentShot { get; private set; } = "";
        /// <summary>Frame index inside the current sequence (0 for stills).</summary>
        public static int CurrentFrame { get; private set; }

        /// <summary>True when this player run captures (-capture) and its -capture-set list names the set.</summary>
        public static bool Requested(string set) => Requested(CliArgs.CaptureDir, CliArgs.CaptureSet, set);

        /// <summary>Pure form for tests: a non-empty capture dir and the set named in the comma list (case-insensitive).</summary>
        public static bool Requested(string captureDir, string setList, string set)
        {
            if (string.IsNullOrEmpty(captureDir) || string.IsNullOrEmpty(setList) || string.IsNullOrEmpty(set)) return false;
            foreach (var p in setList.Split(','))
                if (string.Equals(p.Trim(), set.Trim(), StringComparison.OrdinalIgnoreCase)) return true;
            return false;
        }

        /// <summary>Registers (or replaces) a rig capture set. stills / sequences may be null. Names of the built-in sets
        /// (ShotPlan.BuiltInSets) are refused.</summary>
        public static bool Register(string set, Func<List<StillShot>> stills, Func<int, int, List<SequenceSpec>> sequences)
        {
            if (string.IsNullOrEmpty(set) || set.IndexOf(',') >= 0 || set.StartsWith("pose:", StringComparison.OrdinalIgnoreCase)) return false;
            string key = set.Trim().ToLowerInvariant();
            if (Array.IndexOf(ShotPlan.BuiltInSets, key) >= 0) return false;
            s_sets[key] = new Entry { stills = stills, sequences = sequences };
            return true;
        }

        public static bool Unregister(string set) => !string.IsNullOrEmpty(set) && s_sets.Remove(set.Trim().ToLowerInvariant());
        public static bool IsRegistered(string set) => !string.IsNullOrEmpty(set) && s_sets.ContainsKey(set.Trim().ToLowerInvariant());
        public static void Clear() { s_sets.Clear(); CurrentShot = ""; CurrentFrame = 0; }

        /// <summary>The stills of a registered set, each stamped with the set name (empty when unknown or none).</summary>
        public static List<StillShot> Stills(string set)
        {
            var list = new List<StillShot>();
            if (!s_sets.TryGetValue((set ?? "").Trim().ToLowerInvariant(), out Entry e) || e.stills == null) return list;
            var got = e.stills();
            if (got == null) return list;
            foreach (var st in got) if (st != null) { if (string.IsNullOrEmpty(st.set)) st.set = set; list.Add(st); }
            return list;
        }

        /// <summary>The sequences of a registered set at the plan's sequence resolution.</summary>
        public static List<SequenceSpec> Sequences(string set, int w, int h)
        {
            var list = new List<SequenceSpec>();
            if (!s_sets.TryGetValue((set ?? "").Trim().ToLowerInvariant(), out Entry e) || e.sequences == null) return list;
            var got = e.sequences(w, h);
            if (got == null) return list;
            foreach (var sq in got) if (sq != null) { if (string.IsNullOrEmpty(sq.set)) sq.set = set; list.Add(sq); }
            return list;
        }

        /// <summary>Harness: the shot being posed (null / "" = between shots).</summary>
        public static void SetCurrent(string shot, int frame)
        {
            CurrentShot = shot ?? "";
            CurrentFrame = frame;
        }
    }
}
