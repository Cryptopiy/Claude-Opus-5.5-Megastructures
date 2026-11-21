// KrakenDirector: runs the kraken's choreography in the game (Addendum A1 1.5-1.6, LEAD DECISIONS 2 and 3; show owner).
// Owns the one KrakenCreature of the game (built by Beats/Kraken_Show behind the load fade) and, every frame:
//   1. effective progress sEff (A1 1.5): on the crest (s < 920) sEff = latch(max(s, sEff + 1.5 m/s dt)) capped at the end
//      of the current choreography segment (KrakenDirectorChoreo.SegmentEnds), so at 1.5 and 2.5 m/s every pose is the same
//      function of s and a player who stops sees the current appearance finish at the 1.5 m/s rate; in the interiors
//      (s >= 920) progress only (latched); capture stills and sequences use sEff = s (reproducible); a jump of more than
//      20 m (teleport, -progress) re-evaluates the pure function;
//   2. the end clock tg (stinger): seconds since GameState.GoalReached (real time), or -show-end-t / -end-t in captures;
//   3. KrakenDirectorChoreo.Evaluate(sEff, ambient t, tg) -> KrakenRig inputs, Evaluate, hero arms
//      (KrakenDirectorArms over the rig's chain), KrakenCreature.Sync (LOD, eyes tracking the camera, lids);
//   4. publishes SeaDisturbances (one writer; every frame, also in capture mode): the choreography's head / slap entries
//      plus a TentacleRise at every place an arm crosses the sea surface (thickest first; Max 8);
//   5. publishes KrakenCalls when sEff (or tg) crosses a cue of KrakenDirectorChoreo.Calls (sound 2-6 s before sight).
// The beat-6 window encounter (gallery owner) may register KrakenDirector.Encounter: while it Wants() the frame it poses the
// shared creature itself (KrakenDirector.Instance.Creature); otherwise the kraken is hidden in s 1060-1380.
// QA: -show-off (no kraken: A/B renders), -show-end-t T (stinger end clock in captures). The director is not built when the
// kraken look-dev rig runs (-capture-set kraken / -kraken-perf: it owns its own creature) and it leaves the bus to the
// sea / mist QA writers (-sea-dist-qa, -mist-qa-sea: "never two writers in one run").
using UnityEngine;

namespace Megastructure.Creature
{
    /// <summary>The beat-6 window encounter (gallery owner): poses the shared creature while Wants() is true.</summary>
    public interface IKrakenEncounter
    {
        bool Wants(in FrameState f);
        /// <summary>Set the rig inputs, call kc.Rig.Evaluate and kc.Sync; publish its own KrakenCalls.</summary>
        void Pose(in FrameState f, KrakenCreature kc);
    }

    public sealed class KrakenDirector : IWorldUpdater
    {
        public static KrakenDirector Instance { get; private set; }
        /// <summary>Beat-6 encounter hook (gallery owner).</summary>
        public static IKrakenEncounter Encounter;

        public KrakenCreature Creature { get; private set; }
        public KrakenShowPose Pose { get; } = new KrakenShowPose();
        public KrakenDirectorArms Arms { get; } = new KrakenDirectorArms();
        public double SEff => m_sEff;
        /// <summary>The stinger is drawn only when the creature shaders clip themselves behind the dam for lee-side cameras
        /// (they declare the property _DamClip and read the global _BwDamClip): the far field draws the lee face at the far
        /// plane without depth, so an unclipped kraken east of the dam would show through it. -show-stinger forces it (QA).</summary>
        public bool DamClipReady { get; private set; }
        static readonly int IdDamClip = Shader.PropertyToID("_BwDamClip");
        public double EndTime => m_tg;
        /// <summary>False when a QA bus writer runs (sea / mist look-dev) or -show-off.</summary>
        public bool PublishBus = true;
        public bool PublishCalls = true;
        /// <summary>QA: the end clock of this frame (NaN = none; the show capture set sets it per stinger shot).</summary>
        public double ForceEndTime = double.NaN;
        /// <summary>Entries of the last published bus (diagnostics, tests).</summary>
        public int BusCount { get; private set; }

        double m_sEff = double.NaN, m_sPrev = double.NaN, m_tPrev = double.NaN, m_tg = -1.0, m_tgPrev = -1.0;
        int m_cue;                         // next cue index of Calls (ordered by S)
        int m_stingerCue;                  // next stinger cue (KrakenDirectorChoreo.StingerCallTgs)
        bool m_stingerOn;
        /// <summary>Smoothed rate of sEff (m/s; the sound durations, fix wave S2-2). NaN until measured.</summary>
        double m_sRate = double.NaN;
        public double SRate => m_sRate;
        double m_forcedTg = double.NaN;
        readonly SeaDisturbance[] m_cross = new SeaDisturbance[24];
        readonly int[] m_crossLimb = new int[24];
        int m_crossN, m_curLimb;

        static float MotionOfLimb(KrakenShowPose p, int limb)
        {
            for (int i = 0; i < p.ArmCount; i++) if (p.Arms[i].Limb == limb && p.Arms[i].Weight > 0f) return p.Arms[i].Motion;
            return -1f;
        }

        /// <summary>True when the creature was built by another owner (the beat-6 encounter builds its own before this builder
        /// runs: builders run sorted by Name). The game then has ONE creature (its _KrCtrl buffer and limb arrays are globals):
        /// the director poses it only in its own windows and never hides it while another owner shows it.</summary>
        public bool SharedCreature { get; private set; }
        bool m_shown;

        public static KrakenDirector Build(WorldContext ctx)
        {
            KrakenCreature kc = null;
            bool shared = false;
            foreach (var h in Object.FindObjectsByType<KrakenHost>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                if (h != null && h.Owner != null && h.Owner.Rig != null) { kc = h.Owner; shared = true; break; }
            if (kc == null) kc = KrakenCreature.Build(ctx, ctx != null ? ctx.Root : null);
            if (kc == null || kc.Rig == null) { Debug.LogError("[BW-FAIL] KrakenDirector: the kraken did not build"); return null; }
            var d = new KrakenDirector { Creature = kc, SharedCreature = shared };
            if (shared && ctx != null) ctx.Log("KrakenDirector: sharing the creature already built by another owner (one kraken per game)");
            d.m_forcedTg = CliArgs.GetDouble("-show-end-t", CliArgs.GetDouble("-end-t", double.NaN));
            if (CliArgs.Has("-sea-dist-qa") || CliArgs.Has("-mist-qa-sea")) d.PublishBus = false;
            Instance = d;
            if (!shared) kc.Visible = false;
            var sh = Shader.Find(KrakenCreature.BodyShader);
            d.DamClipReady = CliArgs.Has("-show-stinger") || (sh != null && sh.FindPropertyIndex("_DamClip") >= 0);
            return d;
        }

        /// <summary>True when the standing stinger columns may be switched on without a visible pop: the camera looks away from
        /// the dam (no eastward component) or its frame top is at least 1 deg under the crest line (the lee crest edge x -6,
        /// y 0) seen from the camera. Pure (tests).</summary>
        public static bool StingerMayAppear(Vector3 camPos, Vector3 camForward, float fovV)
        {
            if (camForward.x <= 0f) return true;
            float pitch = Mathf.Asin(Mathf.Clamp(camForward.y, -1f, 1f)) * Mathf.Rad2Deg;
            float crestEl = Mathf.Atan2(-camPos.y, Mathf.Max(-6f - camPos.x, 1f)) * Mathf.Rad2Deg;
            return pitch + 0.5f * fovV < crestEl - 1f;
        }

        /// <summary>Effective progress step (A1 1.5, pure; also the tests' model of a run). dt = ambient seconds.</summary>
        public static double StepEffective(double sEffPrev, double s, double dt)
        {
            if (double.IsNaN(sEffPrev)) return s;
            if (s >= KrakenDirectorChoreo.InteriorS || sEffPrev >= KrakenDirectorChoreo.InteriorS) return System.Math.Max(sEffPrev, s);
            double cap = SegmentEnd(s);          // finish the segment the PLAYER is in, never start the next one early
            double timeBranch = sEffPrev < cap ? System.Math.Min(cap, sEffPrev + KrakenDirectorChoreo.TimeBranchSpeed * System.Math.Max(0.0, dt)) : sEffPrev;
            return System.Math.Max(s, System.Math.Max(sEffPrev, timeBranch));
        }

        /// <summary>End of the choreography segment that contains s (the time branch stops there).</summary>
        public static double SegmentEnd(double s)
        {
            var e = KrakenDirectorChoreo.SegmentEnds;
            for (int i = 0; i < e.Length; i++) if (s < e[i]) return e[i];
            return KrakenDirectorChoreo.InteriorS;
        }

        public void OnFrame(in FrameState f)
        {
            var kc = Creature;
            if (kc == null || kc.Rig == null) return;
            double t = f.ambientTime;

            // ---- 1. effective progress
            bool jump = double.IsNaN(m_sPrev) || System.Math.Abs(f.s - m_sPrev) > 20.0;
            if (f.capture || jump)
            {
                m_sEff = f.s;
                if (jump) ResyncCues(f.s);
            }
            else
            {
                double dt = double.IsNaN(m_tPrev) ? 0.0 : t - m_tPrev;
                double prev = m_sEff;
                m_sEff = StepEffective(m_sEff, f.s, dt);
                // s rate for the sound durations: an exponential average over about 2 s (a stop does not cut a sound short:
                // CueRate clamps to >= 1.5 m/s, the crest time branch's own rate)
                if (dt > 1e-4 && !double.IsNaN(prev))
                {
                    double v = (m_sEff - prev) / dt;
                    m_sRate = double.IsNaN(m_sRate) ? v : m_sRate + (v - m_sRate) * System.Math.Min(1.0, dt / 2.0);
                }
            }
            m_sPrev = f.s; m_tPrev = t;

            // ---- 2. end clock
            m_tgPrev = m_tg;
            if (!double.IsNaN(ForceEndTime)) m_tg = ForceEndTime;
            else if (!double.IsNaN(m_forcedTg)) m_tg = m_forcedTg;
            else m_tg = GameState.GoalReached && GameState.GoalRealTime >= 0 ? BwClock.RealTime - GameState.GoalRealTime : -1.0;

            // ---- encounter (beat 6) takes the creature for its frames
            var enc = Encounter;
            if (enc != null && m_tg < 0.0 && enc.Wants(in f))
            {
                kc.Visible = true;
                enc.Pose(in f, kc);
                if (PublishBus) { SeaDisturbances.Begin(); SeaDisturbances.Publish(); BusCount = 0; }
                FireCues(t);
                return;
            }

            // ---- 3. pose (the dam clip: x = on when the camera is west of the lee face, y/z = eye x/y; the occluding edge is
            // the lee crest edge (x -6, y 0): the creature discards fragments east of x 6 below the eye-over-edge line)
            Shader.SetGlobalVector(IdDamClip, new Vector4(f.cameraPos.x < -6.5f ? 1f : 0f, f.cameraPos.x, f.cameraPos.y, 0f));
            // stinger: the columns already stand when the end clock starts, so they are switched on only while the camera looks
            // below the crest (the end camera eases to pitch 0 at t_g 0-1.5, frame top 33 deg vs the crest at 59.9 deg seen
            // from the lantern platform) or away from the dam: never a pop on screen; at once in captures
            if (DamClipReady && m_tg >= 0.0)
            {
                if (!m_stingerOn) m_stingerOn = f.capture || StingerMayAppear(f.cameraPos, f.cameraForward, f.fovV);
            }
            else m_stingerOn = false;
            var p = Pose;
            KrakenDirectorChoreo.Evaluate(m_sEff, t, m_stingerOn ? m_tg : -1.0, p);
            var rig = kc.Rig;
            if (p.Visible)
            {
                kc.Visible = true;
                m_shown = true;
                rig.ResetInputs();
                rig.SetPresetWeight(KrakenPreset.Lurk, p.Lurk);
                rig.SetPresetWeight(KrakenPreset.Swim, p.Swim);
                rig.SetPresetWeight(KrakenPreset.Withdraw, p.Withdraw);
                rig.Scale = p.Scale;
                rig.SetRoot(p.Root, p.Heading, p.Pitch);
                rig.Evaluate(t);
                for (int i = 0; i < p.ArmCount; i++) Arms.Apply(rig, p.Arms[i]);
                ClampCorridor(rig, m_tg < 0.0 && m_sEff < KrakenDirectorChoreo.InteriorS);
                // eyes in daylight: slit pupils (another owner may have left them round), lids by the choreography
                for (int e = 0; e < kc.EyeCount; e++) { kc.SetBlink(e, p.Lids); kc.SetPupil(e, 0f); }
                for (int k = 0; k < KrakenRig.ArmCount; k++) rig.SetWetness(k, 1f);
                kc.HeadWetness = p.HeadWetness;
                kc.Sync(f.cameraPos, f.fovV, f.renderHeight, t, f.cameraPos, p.Look, f.capture, Time.deltaTime);
                if (m_tg >= 0.0) LogStinger(in f, kc, p);
            }
            else if (!SharedCreature || m_shown)
            {   // hide it; a shared creature is hidden only when this director was the one showing it
                kc.Visible = false;
                m_shown = false;
                kc.Sync(f.cameraPos, f.fovV, f.renderHeight, t, f.cameraPos, 0f, f.capture, Time.deltaTime);
            }

            // ---- 4. bus
            if (PublishBus)
            {
                SeaDisturbances.Begin();
                int n = 0;
                for (int i = 0; i < p.DistCount && n < SeaDisturbances.Max; i++) { SeaDisturbances.Set(p.Dist[i]); n++; }
                // the stinger is seen from the lee side: the sea behind the dam is hidden, and mist sprays there would draw over
                // the far-field lee face (it has no depth), so the stinger publishes nothing
                if (p.Visible && p.Appearance != "stinger")
                {
                    CollectCrossings(rig, p);
                    // cascades and spray follow the motion of a hero arm; a holding arm keeps its foam collar (fix wave S3-4:
                    // 0.45, was 0.3: a white collar at every K2 / K4 waterline)
                    for (int i = 0; i < m_crossN; i++)
                    {
                        float m = MotionOfLimb(p, m_crossLimb[i]);
                        if (m >= 0f) m_cross[i].Strength *= 0.45f + 0.55f * m;
                    }
                    for (int i = 0; i < m_crossN && n < SeaDisturbances.Max; i++) { SeaDisturbances.Set(m_cross[i]); n++; }
                }
                SeaDisturbances.Publish();
                BusCount = SeaDisturbances.Count;
            }

            // ---- 5. calls
            FireCues(t);
        }

        static readonly double[] DiagTgs = { 0.5, 10.0, 20.0, 30.0 };
        /// <summary>QA diagnostics of the stinger in the real end path (lead task S2-1): once at a few end times, the creature's
        /// render state, its highest arm point and the camera.</summary>
        void LogStinger(in FrameState f, KrakenCreature kc, KrakenShowPose p)
        {
            for (int i = 0; i < DiagTgs.Length; i++)
            {
                if (!(m_tgPrev < DiagTgs[i] && m_tg >= DiagTgs[i])) continue;
                Vector3 top = new Vector3(0f, float.NegativeInfinity, 0f);
                var M = kc.Rig.LocalToWorld;
                for (int a = 0; a < p.ArmCount; a++)
                {
                    var L = kc.Rig.Chain(p.Arms[a].Limb);
                    if (L == null) continue;
                    for (int k = 0; k < L.K; k++) { Vector3 w = M.MultiplyPoint3x4(L.CurP[k]); if (w.y > top.y) top = w; }
                }
                var cam = Camera.main;
                string c = cam != null ? $"cam {cam.transform.position} fwd {cam.transform.forward} far {cam.farClipPlane} near {cam.nearClipPlane} mask {cam.cullingMask}" : "no Camera.main";
                Debug.Log($"[BW] stinger diag tg {m_tg:0.00} capture {f.capture} on {m_stingerOn} f.cam {f.cameraPos} top {top} root {kc.Root.position}; {c}; {kc.DebugState(p.ArmCount > 0 ? p.Arms[0].Limb : 2)}");
            }
        }

        /// <summary>QA A/B (fix wave S2-1): this frame without the kraken (creature hidden, empty bus), so a show still and its
        /// "_nok" twin differ only by the kraken (same pose, s and ambient clock).</summary>
        public void HideForQa(in FrameState f)
        {
            var kc = Creature;
            if (kc == null || kc.Rig == null) return;
            kc.Visible = false;
            m_shown = false;
            kc.Sync(f.cameraPos, f.fovV, f.renderHeight, f.ambientTime, f.cameraPos, 0f, f.capture, Time.deltaTime);
            if (PublishBus) { SeaDisturbances.Begin(); SeaDisturbances.Publish(); BusCount = 0; }
        }

        void ResyncCues(double s)
        {
            var c = KrakenDirectorChoreo.Calls;
            m_cue = 0;
            while (m_cue < c.Length && c[m_cue].S < s) m_cue++;
        }

        void FireCues(double t)
        {
            if (!PublishCalls) return;
            var c = KrakenDirectorChoreo.Calls;
            while (m_cue < c.Length && c[m_cue].S <= m_sEff)
            {
                var q = c[m_cue++];
                // fix wave S2-2: the sound lasts until the walker reaches the cue's SEnd at the current s rate (+ 3 s release)
                KrakenCalls.Publish(new KrakenCall { Kind = q.Kind, Position = q.Position, Strength = q.Strength, T0 = t,
                    Duration = KrakenDirectorChoreo.CueDuration(in q, m_sRate), Underwater = q.Underwater });
            }
            // the stinger: a full-strength call as the columns enter the look-back, then two swells while they rise (S2-3)
            var tgs = KrakenDirectorChoreo.StingerCallTgs;
            while (m_tg >= 0.0 && m_stingerCue < tgs.Length && m_tg >= tgs[m_stingerCue])
            {
                int k = m_stingerCue++;
                KrakenCalls.Publish(new KrakenCall
                {
                    Kind = KrakenCallKind.Stinger, Position = new Vector3(95f, 900f, KrakenDirectorChoreo.StingerZ),
                    Strength = KrakenDirectorChoreo.StingerCallStrength[k], T0 = t, Duration = KrakenDirectorChoreo.StingerCallDuration[k],
                    Underwater = false,
                });
            }
            if (m_tg < 0.0) m_stingerCue = 0;
            KrakenCalls.Expire(t);
        }

        /// <summary>TentacleRise entries where arms cross the sea surface (A1 1.6: HalfAlong = HalfAcross = waterline radius,
        /// heading = the lean of the emerged part, HeightAboveSea = its top). Crossings inside the head's Breach ellipse are
        /// covered by the Breach. Thickest first, no allocation.</summary>
        void CollectCrossings(KrakenRig rig, KrakenShowPose p)
        {
            m_crossN = 0;
            float sea = KrakenDirectorChoreo.SeaY;
            Matrix4x4 M = rig.LocalToWorld;
            float scale = rig.Scale;
            for (int k = 0; k < KrakenRig.ArmCount; k++)
            {
                var L = rig.Limbs[k];
                m_curLimb = k;
                Vector3 a = M.MultiplyPoint3x4(L.CurP[0]);
                for (int i = 0; i < L.K - 1; i++)
                {
                    Vector3 b = M.MultiplyPoint3x4(L.CurP[i + 1]);
                    float da = a.y - sea, db = b.y - sea;
                    if (da * db < 0f && db > 0f)          // rising out of the water (the emerged run starts here)
                    {
                        float fr = da / (da - db);
                        Vector3 c = Vector3.Lerp(a, b, fr);
                        float r = Mathf.Lerp(L.Radius[i], L.Radius[i + 1], fr) * scale;
                        // thin crossings fade in by radius (0.6 -> 1.4 m) instead of a hard cut at 1 m: the K5 whip's legs
                        // straddle 1 m while it topples, and an entry that blinks on and off makes the mist spray pop
                        float fadeR = KrakenDirectorChoreo.Ss(r, 0.6, 1.4);
                        if (fadeR > 0f && !InsideBreach(p, c))
                        {
                            // emerged run: up to the next crossing back into the water
                            float top = 0f; Vector3 last = c;
                            int j = i + 1;
                            for (; j < L.K; j++)
                            {
                                Vector3 q = M.MultiplyPoint3x4(L.CurP[j]);
                                if (q.y < sea) break;
                                top = Mathf.Max(top, q.y - sea); last = q;
                            }
                            Vector3 lean = last - c; lean.y = 0f;
                            float hd = lean.sqrMagnitude > 1f ? Mathf.Atan2(lean.x, lean.z) : 0f;
                            Insert(new SeaDisturbance
                            {
                                Kind = SeaDisturbanceKind.TentacleRise, X = c.x, Z = c.z, HalfAlong = r, HalfAcross = r, Heading = hd,
                                Strength = fadeR * Mathf.Clamp(r / 10f, 0.35f, 1f) * Mathf.Clamp01(top / 6f), HeightAboveSea = top,
                            });
                        }
                    }
                    else if (da * db < 0f && da > 0f && i > 0)
                    {   // entering the water (the far leg of an arch): a foam collar there too
                        float fr = da / (da - db);
                        Vector3 c = Vector3.Lerp(a, b, fr);
                        float r = Mathf.Lerp(L.Radius[i], L.Radius[i + 1], fr) * scale;
                        float fadeR = KrakenDirectorChoreo.Ss(r, 0.6, 1.4);
                        if (fadeR > 0f && !InsideBreach(p, c))
                            Insert(new SeaDisturbance
                            {
                                Kind = SeaDisturbanceKind.TentacleRise, X = c.x, Z = c.z, HalfAlong = r, HalfAcross = r, Heading = 0f,
                                Strength = fadeR * Mathf.Clamp(r / 10f, 0.35f, 1f) * 0.8f, HeightAboveSea = Mathf.Max(0f, a.y - sea),
                            });
                    }
                    a = b;
                }
            }
        }

        /// <summary>A1 1.4: in beats 2-4 (crest = true) every kraken vertex stays >= 140 m east of the parapet plane x 6. The
        /// 1300 m arms of a lurking crown 260 m out would reach under the dam at depth (invisible below the opaque sea, but
        /// inside concrete): every arm point is pushed east to x >= 146 + its radius (world space; only submerged lurking arms
        /// ever touch it). Otherwise (the stinger) the points stay 2 m outside the sea face (x 6, batter 1:8 below y -20), so
        /// no arm ever pokes through the lee face.</summary>
        public static int ClampCorridor(KrakenRig rig, bool crest)
        {
            int moved = 0;
            Matrix4x4 M = rig.LocalToWorld, W = rig.WorldToLocal;
            float scale = rig.Scale;
            for (int k = 0; k < KrakenRig.ArmCount; k++)
            {
                var L = rig.Limbs[k];
                for (int i = 1; i < L.K; i++)
                {
                    Vector3 w = M.MultiplyPoint3x4(L.CurP[i]);
                    float minX = crest ? KrakenDirectorChoreo.CorridorX + L.Radius[i] * scale
                                       : 6f + Mathf.Max(0f, (-20f - w.y) / 8f) + L.Radius[i] * scale + 2f;
                    if (w.x < minX) { w.x = minX; L.CurP[i] = W.MultiplyPoint3x4(w); moved++; }
                }
            }
            return moved;
        }

        void Insert(in SeaDisturbance d)
        {
            if (m_crossN >= m_cross.Length) { if (d.HalfAlong <= m_cross[m_cross.Length - 1].HalfAlong) return; m_crossN--; }
            int i = m_crossN++;
            while (i > 0 && m_cross[i - 1].HalfAlong < d.HalfAlong) { m_cross[i] = m_cross[i - 1]; m_crossLimb[i] = m_crossLimb[i - 1]; i--; }
            m_cross[i] = d; m_crossLimb[i] = m_curLimb;
        }

        static bool InsideBreach(KrakenShowPose p, Vector3 c)
        {
            for (int i = 0; i < p.DistCount; i++)
            {
                var d = p.Dist[i];
                if (d.Kind != SeaDisturbanceKind.Breach) continue;
                float dx = c.x - d.X, dz = c.z - d.Z;
                float sn = Mathf.Sin(d.Heading), cs = Mathf.Cos(d.Heading);
                float along = dx * sn + dz * cs, across = dx * cs - dz * sn;
                float u = along / Mathf.Max(d.HalfAlong, 1f), v = across / Mathf.Max(d.HalfAcross, 1f);
                if (u * u + v * v < 1.0f) return true;
            }
            return false;
        }
    }
}
