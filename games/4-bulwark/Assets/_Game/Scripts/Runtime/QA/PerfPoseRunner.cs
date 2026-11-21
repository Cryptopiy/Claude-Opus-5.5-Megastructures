// PerfPoseRunner: frame time at fixed poses (lead task S2-1, fix wave X2: "perf at 1080p and 4K at the KS17, ride and KS28
// poses (perf JSON)"; MASTER_PROMPT 11.7). Active with -perf-poses <list> (comma list as ONE argument; names below). For each
// pose it holds the camera there (ProgressDirector override s, ICameraControl pose override, the fade off), waits WarmSeconds
// and records MeasureSeconds of frames: the real frame time (unscaledDeltaTime) and FrameTimingManager's CPU / GPU frame
// times. Combine with -perf-log <file> -uncapped (Megastructure.Player.QualityBootstrap.UncappedPerf: vSync 0, no frame cap) and -perf-rt
// 3840x2160 for the 4K budget on a smaller window (PerfMonitor renders the camera offscreen at that size).
// Poses: KS17 (the eye, BulwarkConst.KeyShots), ride (the descent at full speed, s 1600: the face rush, yaw 10 pitch -25),
// KS28 (the look back from the Keeper's Lantern: rest pose, yaw 90 pitch 29, end clock t_g 8.5 for the sun sink and the
// stinger), KS27 (t_g 1.5, pitch 0), or any KeyShots id. Report: <report dir>/perf_poses.json; exit 0 (1 on a missing pose).
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace Megastructure.QA
{
    [DefaultExecutionOrder(32000)]
    public sealed class PerfPoseRunner : QaRunnerBase
    {
        public const string Flag = "-perf-poses";
        public const double WarmSeconds = 2.0, MeasureSeconds = 6.0;
        const int MaxFrames = 20000;

        protected override QaRunMode Mode => QaRunMode.PerfPoses;
        protected override string ReportName => "perf_poses";
        protected override string ReportTag => "PERFPOSE";

        public struct PerfPose
        {
            public string name;
            public double s, x, y, z, yaw, pitch, fov, tg;
        }

        struct Result
        {
            public PerfPose pose;
            public int frames;
            public double avgMs, p99Ms, maxMs, avgFps, gpuAvgMs, gpuP99Ms, cpuAvgMs;
        }

        readonly List<Result> m_results = new List<Result>(8);
        readonly float[] m_ms = new float[MaxFrames], m_gpu = new float[MaxFrames], m_cpu = new float[MaxFrames];
        readonly FrameTiming[] m_timing = new FrameTiming[1];
        bool m_poseActive;
        PerfPose m_pose;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void EnsurePresent() { EnsureRunner<PerfPoseRunner>(QaRunMode.PerfPoses); }

        /// <summary>The named poses (pure; EditMode tests check them). Unknown names give false.</summary>
        public static bool TryPose(string name, out PerfPose p)
        {
            p = default;
            string n = (name ?? "").Trim();
            if (n.Length == 0) return false;
            if (string.Equals(n, "ride", StringComparison.OrdinalIgnoreCase))
            {
                double s = 1600.0;
                SequenceSpec.EyeAt(s, out double x, out double y, out double z);
                p = new PerfPose { name = "ride", s = s, x = x, y = y, z = z, yaw = 10.0, pitch = -25.0, fov = 66.0, tg = -1.0 };
                return true;
            }
            if (n == "KS27" || n == "KS28")
            {
                double s = EndingPath.SRest;
                SequenceSpec.EyeAt(s, out double x, out double y, out double z);
                bool k28 = n == "KS28";
                p = new PerfPose { name = n, s = s, x = x, y = y, z = z, yaw = BulwarkConst.EndLookYaw, pitch = k28 ? BulwarkConst.EndLookPitch : 0.0, fov = 66.0, tg = k28 ? 8.5 : 1.5 };
                return true;
            }
            foreach (var k in BulwarkConst.KeyShots)
            {
                if (VisibilityAudit.KeyId(k.tag) != n) continue;
                p = new PerfPose { name = n, s = k.s, x = k.x, y = k.y, z = k.z, yaw = k.yaw, pitch = k.pitch, fov = k.fov, tg = -1.0 };
                return true;
            }
            return false;
        }

        IEnumerator Start()
        {
            yield return WaitForWorld();
            if (!WorldOk) { Checks.Add("worldReady", false, "GameBootstrap.Ready never became true"); Done(); yield break; }
            var p = BwServices.Player;
            if (p != null) { p.SetDrive(false, 0f, 0f); p.Frozen = true; }
            string list = CliArgs.Get(Flag) ?? "KS17,ride,KS28";
            foreach (string raw in list.Split(','))
            {
                if (!TryPose(raw, out PerfPose pose)) { Checks.Add("pose " + raw.Trim(), false, "unknown pose"); continue; }
                m_pose = pose;
                m_poseActive = true;
                if (p != null)
                {
                    double safe = Math.Min(pose.s, FrameworkConst.GoalTriggerS - 1.0);
                    p.Frozen = false; p.Teleport(safe, BwPath.CinematicLateral(safe)); p.Frozen = true;
                }
                double t0 = Time.realtimeSinceStartupAsDouble;
                while (Time.realtimeSinceStartupAsDouble - t0 < WarmSeconds) yield return null;
                int n = 0;
                t0 = Time.realtimeSinceStartupAsDouble;
                while (Time.realtimeSinceStartupAsDouble - t0 < MeasureSeconds && n < MaxFrames)
                {
                    yield return null;
                    m_ms[n] = Time.unscaledDeltaTime * 1000f;
                    FrameTimingManager.CaptureFrameTimings();
                    if (FrameTimingManager.GetLatestTimings(1, m_timing) > 0) { m_gpu[n] = (float)m_timing[0].gpuFrameTime; m_cpu[n] = (float)m_timing[0].cpuFrameTime; }
                    else { m_gpu[n] = float.NaN; m_cpu[n] = float.NaN; }
                    n++;
                }
                m_results.Add(Summarise(pose, n));
                var r = m_results[m_results.Count - 1];
                Debug.Log("[BW] perf pose " + pose.name + ": " + r.frames + " frames, avg " + F(r.avgMs) + " ms (" + F(r.avgFps) + " fps), p99 " + F(r.p99Ms) + " ms, GPU avg " + F(r.gpuAvgMs) + " ms");
                Checks.Add("pose " + pose.name, r.frames > 0, r.frames + " frames, avg " + F(r.avgMs) + " ms, GPU " + F(r.gpuAvgMs) + " ms", true);
            }
            m_poseActive = false;
            EndOverride();
            Done();
        }

        Result Summarise(PerfPose pose, int n)
        {
            var r = new Result { pose = pose, frames = n };
            if (n <= 0) return r;
            var ms = new float[n];
            Array.Copy(m_ms, ms, n);
            var sum = PerfSummary.From(ms, n, 1000.0 / 60.0);
            r.avgMs = sum.avgMs; r.p99Ms = sum.p99Ms; r.maxMs = sum.maxMs; r.avgFps = sum.avgFps;
            r.gpuAvgMs = Mean(m_gpu, n, out r.gpuP99Ms);
            r.cpuAvgMs = Mean(m_cpu, n, out _);
            return r;
        }

        static double Mean(float[] a, int n, out double p99)
        {
            var l = new List<float>(n);
            for (int i = 0; i < n; i++) if (!float.IsNaN(a[i]) && a[i] > 0f) l.Add(a[i]);
            p99 = double.NaN;
            if (l.Count == 0) return double.NaN;
            l.Sort();
            double s = 0; foreach (float v in l) s += v;
            p99 = l[Math.Min(l.Count - 1, (int)Math.Floor(0.99 * l.Count))];
            return s / l.Count;
        }

        void LateUpdate()
        {
            if (!m_poseActive) return;
            var pd = ProgressDirector.Instance;
            if (pd != null) { pd.OverrideActive = true; pd.OverrideS = m_pose.s; }
            AtmosphereDriver.EndSinkOverride = m_pose.tg >= 0 ? m_pose.tg : double.NaN;
            var kd = Megastructure.Creature.KrakenDirector.Instance;
            if (kd != null) kd.ForceEndTime = m_pose.tg >= 0 ? m_pose.tg : double.NaN;
            var pos = new Vector3((float)m_pose.x, (float)m_pose.y, (float)m_pose.z);
            BwServices.CameraControl?.SetPoseOverride(pos, (float)m_pose.yaw, (float)m_pose.pitch, (float)m_pose.fov);
            BwServices.Fade?.SetBlackImmediate(0f);
            if (pd != null) pd.Apply();
            Megastructure.Rendering.BackdropRig.FollowAll(pos);
        }

        void EndOverride()
        {
            var pd = ProgressDirector.Instance;
            if (pd != null) pd.OverrideActive = false;
            BwServices.CameraControl?.ClearPoseOverride();
            AtmosphereDriver.EndSinkOverride = double.NaN;
            var kd = Megastructure.Creature.KrakenDirector.Instance;
            if (kd != null) kd.ForceEndTime = double.NaN;
        }

        static string F(double v) => double.IsNaN(v) ? "n/a" : v.ToString("0.00", CultureInfo.InvariantCulture);

        void Done()
        {
            AddCommonChecks();
            string rt = CliArgs.Get("-perf-rt") ?? (Screen.width + "x" + Screen.height);
            string summary = m_results.Count + " poses at " + rt + (Megastructure.Player.QualityBootstrap.UncappedPerf ? " uncapped" : " (60 fps cap)");
            Complete(summary, j =>
            {
                j.Prop("resolution", rt).Prop("uncapped", Megastructure.Player.QualityBootstrap.UncappedPerf).Prop("frameTiming", FrameTimingManager.IsFeatureEnabled());
                j.Prop("warmSeconds", WarmSeconds).Prop("measureSeconds", MeasureSeconds);
                j.Key("poses").BeginArray();
                foreach (var r in m_results)
                {
                    j.BeginObject(true).Prop("name", r.pose.name).Prop("s", r.pose.s).Prop("pos", r.pose.x, r.pose.y, r.pose.z)
                     .Prop("yaw", r.pose.yaw).Prop("pitch", r.pose.pitch).Prop("fov", r.pose.fov).Prop("tg", r.pose.tg)
                     .Prop("frames", r.frames).Prop("avgMs", r.avgMs).Prop("avgFps", r.avgFps).Prop("p99Ms", r.p99Ms).Prop("maxMs", r.maxMs)
                     .Prop("gpuAvgMs", r.gpuAvgMs).Prop("gpuP99Ms", r.gpuP99Ms).Prop("cpuAvgMs", r.cpuAvgMs).EndObject();
                }
                j.EndArray();
            });
        }
    }
}
