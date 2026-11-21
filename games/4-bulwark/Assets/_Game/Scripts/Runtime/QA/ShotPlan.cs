// ShotPlan: the capture lists of the harness as pure data (MASTER_PROMPT 11.2, 11.6, 11.12, 14.6, 14.10.2, 14.10.3,
// Part 10 items 6, 12, 21, 22, 35, 36). Imported from Thread's ShotPlan (the set grammar and the data types); every
// Thread window, swap / deck / bank / moire extra and the _wide variants are removed; the Abyss "pose:" token and its
// sequence concepts (WalkPinned, MovingPose, Steps) are merged in. No UnityEngine dependency, so EditMode tests check
// the plan (every key shot at every resolution, tag format, window lengths).
//
// Capture sets (-capture-set, comma list; pose tokens can be mixed in):
//   keyshots   every BulwarkConst.KeyShots at 1920x1080 and 3840x2160 and the framing variants at 1680x1050 (_1610) and
//              2560x1080 (_219); 14.10.2: a key shot uses the schedule vFOV at its s (gameFov is checked)
//   beats      per beat start / mid / end stills and the free-look stills from the middle of the beat (yaw +90, -90, 180
//              at pitch 0, pitch +60 and -60; beat 3 also pitch -90)
//   sequences  = transitions + statics + pans + edges (PNG frame sequences at 30 fps virtual time)
//   transitions  +-10 s around each 14.6 boundary at the walk speed, plus s 520-640, 880-960, 1060-1080, 1405-1450, and
//              the light-pool walk through the beat-1 lamps s 30-54 (seq_lamps_b01)
//   statics    120 frames of a static camera per beat; at the KS04, KS08 and KS20 poses (sea, plume, ledge flicker); the
//              60 s KS04 plume count at 4 fps (seq_plumes_KS04)
//   pans       0.5 px per frame for 30 frames (thin lines, H7) per beat and 0.1 px per frame on the lee rail (beat 7); at
//              the KS04 pose (sea beyond 1 km), KS09 at pitch -58 and -90 (grating), KS20 and the s 1470 city lights
//   edges      the first and the last frames (start fade; seq_end: the real ending from EndingPath.SGoal - 3 through the
//              whole EndTotal with the kraken director on: stinger columns and the crest-dark time recorded per frame)
//   ride       the real game in the ride and on the platform (A1 4.10, re-keyed to EndingPath): seq_ride_depart (S0..S0+60),
//              seq_ride_arrive (SDescent1-17.5 .. STransfer1+2.5), seq_shuttle (STransfer1-0.5 .. SShuttle1 at 60 fps),
//              seq_turnback (SShuttle1 .. SGoal, then t_g 0-45); the walker and its gaze are the AutoWalker's (WalkPlanner)
//   kseq       the kraken moments at 30 fps with the AutoWalker gaze (A1 4.10): seq_k1..seq_k7
//   tprec      T-PREC (A1 4.8, lead ruling R6): 120 static frames at the KS27 rest pose (frozen clock), a 0.1 px/frame
//              pan there, and 120 frames in the cabin at s ~1690 with a fixed world look (cabin-to-camera jitter)
//   encode     10 s at 60 fps of the gradient views (storm sky, dusk sky, deep water veil, mist; 11.12, 14.10.3)
//   parity     one window capture and one RenderTexture capture of the same frame (player only)
//   stills     = parity + keyshots + beats;  all = stills + sequences;  everything = all + encode
//              (sequences = transitions + statics + pans + edges + ride + kseq; transitions walk with the AutoWalker gaze)
//   materials  material sheets (8.11.8): per beat that beat's materials on a standard slab, sphere and wall panel under the
//              beat's key light and sky (the shot's s drives AtmosphereDriver), built off-path by the QA_MaterialSheet
//              builder only when this set is requested; row / close / far stills per sheet, the road numerals and the
//              grating views, the grating field and the crust wall, plus 1080p shimmer sequences (0.5 px/frame pans and
//              3 mm/frame side steps over the road, the grating field along and across its bars at 2-52 m, the far crust
//              wall). Never part of stills / all / everything (it changes the world).
//   matcontext the in-context material shimmer sequences WITHOUT the rig (the real game state): 0.5 px/frame pans at the
//              KS05 (lee face) and KS06 (parapet receding) poses and in the beat-1 gallery, and 1.5 m forward dollies at
//              walking pace along the KS06 parapet and down the gallery (formwork at grazing angles, Part 10 #3, #4).
//              Never part of stills / all / everything.
//   <rig set>  a set registered at build time by an off-path QA rig (QaRigs.Register; Wave A1: "kraken" from QA_KrakenRig):
//              that rig's stills and sequences. Never part of stills / all / everything (a rig changes the world).
//   pose:NAME:x:y:z:yaw:pitch:fov   a free still (look-dev; fov <= 0 = schedule vFOV): s = projection of the eye onto the
//              path over the whole length, scored by the plan distance plus the height difference to BwPath.EyeY (PoseS:
//              ledge poses resolve to the ledge), clamped to <= 1497 (the goal trigger never fires), frozen clock, set "custom"
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Megastructure.QA
{
    /// <summary>One still image of the plan.</summary>
    public sealed class StillShot
    {
        public string tag;            // unique; also the file name without ".png"
        public string set;            // keyshots, beats, parity, custom
        public string baseTag;        // key-shot tag of BulwarkConst.KeyShots (key shots), else the tag
        public string variant;        // "1080", "4k", "1610", "219", "window"
        public int width, height;
        public double s, x, y, z, yaw, pitch, fov;
        public double ambient;        // ambient clock value used for the shot (deterministic)
        public int keyShotIndex = -1;
        public bool gameFov = true;   // FOV equals the schedule vFOV(s) (framing acceptances use these)
        public bool framing;          // evaluate the projected subjects (key shots)
        public double lumaMin = double.NaN, lumaMax = double.NaN;
        public double endT = double.NaN;   // end-camera time t_g (KeyShot.tg: the lantern rows KS27 1.5, KS28 8.5, KS28b 20, KS29 30), NaN = none

        public string File => tag + ".png";
        public QaView View => new QaView(x, y, z, yaw, pitch, fov, width, height);
    }

    /// <summary>
    /// Walk: the eye follows the path at speed (cinematic lateral, heading + yaw offset or a world yaw). Static: fixed pose.
    /// PanPitch / PanYaw: fixed position, the angle steps per frame. RealStart / RealEnd: the real game (start fade, ending).
    /// WalkPinned: s advances at speed (progress events fire) while the camera holds the base pose (door at 1.5 / 2.5 m/s,
    /// set pieces). MovingPose: linear move from the base pose to (x1, y1, z1, yaw1, pitch1). Steps: sideways steps of
    /// stepMetres per frame along the heading's right vector (thin-line stability, H7). WalkGaze: as Walk, but the look is the
    /// AutoWalker's (a WalkPlanner seed 1 stepped at the sequence rate from a warm-up before s0: schedule + lead, critically
    /// damped follower, 18 / 13 deg/s limits, sway). RealWalk: the real game from s0 (teleport, the player driven and looking
    /// exactly as the AutoWalker drives it: the ride clock runs by itself, the goal fires in the real path); frames is the
    /// upper bound, afterGoal seconds are kept after the goal trigger; fixedLook holds the world (yaw, pitch) instead.
    /// </summary>
    public enum SeqMode { Walk = 0, Static = 1, PanPitch = 2, PanYaw = 3, RealStart = 4, RealEnd = 5, WalkPinned = 6, MovingPose = 7, Steps = 8, WalkGaze = 9, RealWalk = 10 }

    /// <summary>One frame sequence of the plan.</summary>
    public sealed class SequenceSpec
    {
        public string name;           // directory name (seq_*, enc_*)
        public string set;            // transitions, statics, pans, edges, encode
        public string purpose;
        public SeqMode mode;
        public int fps, width, height;
        public int frames;            // fixed frame count (RealEnd: upper bound)
        public double s0, s1;         // Walk: plan distance at frame 0 and at the last frame
        public double speed = FrameworkConst.CinematicSpeed;
        public bool yawWorld;         // Walk: yaw is a world yaw, else an offset to the path heading
        public double yaw, pitch;     // Walk: yaw (offset) and pitch; Static / Pan / pinned / moving: the base pose
        public double x, y, z, fov;   // Static / Pan / pinned / moving pose
        public double x1, y1, z1, yaw1, pitch1;   // MovingPose end pose
        public double stepMetres;     // Steps: sideways metres per frame
        public double panStepDeg;     // Pan: angle per frame
        public bool advanceAmbient = true;
        public double ambient0;
        public double afterGoal = double.NaN;   // RealWalk / RealEnd: seconds kept after the goal trigger (NaN = run all frames)
        public bool fixedLook;                  // RealWalk: hold the world (yaw, pitch) instead of the AutoWalker gaze
        double[] m_gazeYaw, m_gazePitch;        // WalkGaze: the planner look per frame (built on first use)

        public double Seconds => fps > 0 ? frames / (double)fps : 0;

        /// <summary>Eye position of the walking camera at s (cinematic lateral, distance-smoothed eye height); past the walked
        /// path (s &gt; BwPath.Length) the ending timeline (EndingPath: the cabin spot in the ride, the platform walk after).</summary>
        public static void EyeAt(double s, out double x, out double y, out double z)
        {
            if (s > BwPath.Length)
            {
                EndingPath.World(s, EndingPath.ClampLateral(s, BwPath.CinematicLateral(s)), out x, out _, out z);
                y = EndingPath.EyeY(s);
                return;
            }
            BwPath.World(s, BwPath.CinematicLateral(s), out x, out _, out z);
            y = BwPath.EyeY(s);
        }

        /// <summary>Path heading at any s of the game (walked path, then the ending timeline).</summary>
        public static double HeadingAt(double s) => s > BwPath.Length ? EndingPath.HeadingDeg(s) : BwPath.HeadingDeg(s);

        /// <summary>Warm-up of the WalkGaze follower before s0 (seconds of walking at the sequence speed).</summary>
        public const double GazeWarmupSeconds = 12.0;

        /// <summary>
        /// The AutoWalker look for frames 0..count-1 of a walk from s0 at speed (fps): a WalkPlanner (seed 1, as the
        /// VisibilityAudit sweep) begins GazeWarmupSeconds before s0 at the scheduled gaze (lead included) and is stepped at
        /// 1 / fps, so the follower lag, the 18 / 13 deg/s limits and the sway of the real cinematic walk are in the frames.
        /// </summary>
        public static void PlannerGaze(double s0, double speed, int fps, int count, double[] yaw, double[] pitch)
        {
            double dt = 1.0 / Math.Max(1, fps);
            double sw = Math.Max(0.0, s0 - GazeWarmupSeconds * speed);
            if (s0 > BwPath.Length && sw < BwPath.Length) sw = Math.Min(s0, BwPath.Length);
            int warm = (int)Math.Round((s0 - sw) / Math.Max(1e-6, speed) * fps);
            double lead = 2.0 / Megastructure.Player.WalkPlanner.LookOmega * speed;
            Megastructure.Player.GazeSchedule.Evaluate(Math.Min(EndingPath.EndS, sw + lead), out double y0, out double p0);
            var wp = new Megastructure.Player.WalkPlanner(1);
            wp.Begin(sw, speed, BwPath.CinematicLateral(sw), y0, p0, speed);
            for (int i = 0; i < warm; i++) { double s = sw + speed * i * dt; wp.Step(dt, s, s / FrameworkConst.CinematicSpeed); }
            for (int n = 0; n < count; n++)
            {
                double s = Math.Min(EndingPath.EndS, s0 + speed * n * dt);
                wp.Step(dt, s, s / FrameworkConst.CinematicSpeed);
                yaw[n] = wp.OutYaw; pitch[n] = wp.OutPitch;
            }
        }

        /// <summary>Analytic camera pose (and progress s) of frame n.</summary>
        public void FramePose(int n, out double s, out double px, out double py, out double pz, out double yawDeg, out double pitchDeg, out double fovDeg)
        {
            switch (mode)
            {
                case SeqMode.Walk:
                {
                    s = Math.Min(BwPath.Length, s0 + speed * n / fps);
                    EyeAt(s, out px, out py, out pz);
                    yawDeg = yawWorld ? yaw : BwPath.HeadingDeg(s) + yaw;
                    pitchDeg = pitch;
                    fovDeg = BulwarkSchedule.Vfov(s);
                    return;
                }
                case SeqMode.WalkGaze:
                {
                    if (m_gazeYaw == null || m_gazeYaw.Length != Math.Max(1, frames))
                    {
                        m_gazeYaw = new double[Math.Max(1, frames)]; m_gazePitch = new double[m_gazeYaw.Length];
                        PlannerGaze(s0, speed, fps, m_gazeYaw.Length, m_gazeYaw, m_gazePitch);
                    }
                    int i = Math.Max(0, Math.Min(m_gazeYaw.Length - 1, n));
                    s = Math.Min(EndingPath.EndS, s0 + speed * n / fps);
                    EyeAt(s, out px, out py, out pz);
                    yawDeg = m_gazeYaw[i];
                    pitchDeg = m_gazePitch[i];
                    fovDeg = BulwarkSchedule.Vfov(s);
                    return;
                }
                case SeqMode.WalkPinned:
                    s = Math.Min(BwPath.Length, s0 + speed * n / fps);
                    px = x; py = y; pz = z; yawDeg = yaw; pitchDeg = pitch; fovDeg = fov;
                    return;
                case SeqMode.MovingPose:
                {
                    double u = frames > 1 ? n / (double)(frames - 1) : 0;
                    s = s0; px = x + (x1 - x) * u; py = y + (y1 - y) * u; pz = z + (z1 - z) * u;
                    yawDeg = yaw + BwMath.DeltaDeg(yaw, yaw1) * u; pitchDeg = pitch + (pitch1 - pitch) * u; fovDeg = fov;
                    return;
                }
                case SeqMode.Steps:
                {
                    double a = yaw * BwMath.Deg;
                    double rx = Math.Cos(a), rz = -Math.Sin(a);   // right of the view yaw
                    s = s0; px = x + rx * stepMetres * n; py = y; pz = z + rz * stepMetres * n;
                    yawDeg = yaw; pitchDeg = pitch; fovDeg = fov;
                    return;
                }
                case SeqMode.PanPitch:
                    s = s0; px = x; py = y; pz = z; yawDeg = yaw; pitchDeg = pitch + panStepDeg * n; fovDeg = fov;
                    return;
                case SeqMode.PanYaw:
                    s = s0; px = x; py = y; pz = z; yawDeg = yaw + panStepDeg * (n - frames / 2); pitchDeg = pitch; fovDeg = fov;
                    return;
                default:
                    s = s0; px = x; py = y; pz = z; yawDeg = yaw; pitchDeg = pitch; fovDeg = fov;
                    return;
            }
        }

        public double AmbientAt(int n) => advanceAmbient ? ambient0 + n / (double)fps : ambient0;
    }

    public sealed class CapturePlan
    {
        public readonly List<StillShot> stills = new List<StillShot>();
        public readonly List<SequenceSpec> sequences = new List<SequenceSpec>();
        public StillShot parity;
        public readonly List<string> sets = new List<string>();
        public readonly List<string> unknownSets = new List<string>();
    }

    public sealed class CapturePlanOptions
    {
        public int seqWidth = 1920, seqHeight = 1080;
        public bool encode4k;
        public bool keyshots4k = true;
        public double walkSpeed = FrameworkConst.CinematicSpeed;
        public string[] only;         // keep only tags / sequence names containing one of these
    }

    public static class ShotPlan
    {
        public const int W1080 = 1920, H1080 = 1080, W4K = 3840, H4K = 2160;
        public const int W1610 = 1680, H1610 = 1050, W219 = 2560, H219 = 1080;
        public const int SeqFps = 30;                  // frame sequences at 30 fps virtual time
        public const int EncodeFps = 60;               // encode clips at 60 fps x 10 s
        public const double EncodeSeconds = 10.0;
        public const double WindowHalfSeconds = 10.0;  // +-10 s around each 14.6 boundary
        public const int StaticFrames = 120;
        public const double PanPx = 0.5;               // 0.5 px per frame for 30 frames (H7)
        public const int PanFrames = 30;
        public const double SlowPanPx = 0.1;           // 0.1 px per frame on the lee rail
        public const int SlowPanFrames = 30;
        public const int EdgeFrames = 60;              // defect 22: first and last 60 frames
        public const double EndLeadIn = 3.0;           // seq_end starts 3 m before the goal trigger
        public const double EndTail = 0.5;             // frames kept after the 18.5 s ending
        public const double MaxPoseS = 1497.0;         // pose: tokens never reach the goal trigger 1498.5

        /// <summary>Extra sequence windows (s) of the 14.6 / 14.10 spec besides the boundaries.</summary>
        public static readonly double[,] ExtraWindows = { { 520, 640 }, { 880, 960 }, { 1060, 1080 }, { 1405, 1450 } };

        static readonly string[] s_freeLookTags = { "look_r", "look_l", "look_back", "look_up", "look_down" };
        static readonly double[] s_freeLookYaw = { 90, -90, 180, 0, 0 };
        static readonly double[] s_freeLookPitch = { 0, 0, 0, 60, -60 };

        public static string BeatPrefix(int beatIndex) => "b" + (beatIndex + 1).ToString("D2", CultureInfo.InvariantCulture) + "_" + FrameworkConst.BeatNames[beatIndex];

        /// <summary>Deterministic ambient clock for a still: the time the cinematic walker (2.5 m/s) reaches s.</summary>
        public static double AmbientForS(double s) => s / FrameworkConst.CinematicSpeed;

        // ------------------------------------------------------------------ plan

        /// <summary>The set names ShotPlan itself knows (QaRigs refuses to register these).</summary>
        public static readonly string[] BuiltInSets =
        {
            "keyshots", "beats", "parity", "sequences", "transitions", "statics", "pans", "edges", "encode", "stills", "all",
            "everything", "materials", "matcontext",
        };

        public static CapturePlan Build(string setList, CapturePlanOptions opt = null)
        {
            if (opt == null) opt = new CapturePlanOptions();
            var plan = new CapturePlan();
            bool parity = false, keys = false, beats = false, trans = false, statics = false, pans = false, edges = false, encode = false, mats = false, matCtx = false;
            bool ride = false, kseq = false, tprec = false;
            var rigSets = new List<string>();
            string[] parts = (string.IsNullOrEmpty(setList) ? "all" : setList).Split(',');
            for (int i = 0; i < parts.Length; i++)
            {
                string raw = parts[i].Trim();
                string p = raw.ToLowerInvariant();
                if (p.Length == 0) continue;
                if (p.StartsWith("pose:", StringComparison.Ordinal))
                {
                    if (TryPose(raw, out StillShot ps, out string err)) { plan.stills.Add(ps); plan.sets.Add("pose"); }
                    else plan.unknownSets.Add(raw + " (" + err + ")");
                    continue;
                }
                switch (p)
                {
                    case "keyshots": keys = true; break;
                    case "beats": beats = true; break;
                    case "parity": parity = true; break;
                    case "sequences": trans = statics = pans = edges = ride = kseq = true; break;
                    case "transitions": trans = true; break;
                    case "statics": statics = true; break;
                    case "pans": pans = true; break;
                    case "edges": edges = true; break;
                    case "encode": encode = true; break;
                    case "ride": ride = true; break;
                    case "kseq": kseq = true; break;
                    case "tprec": tprec = true; break;
                    case "stills": parity = keys = beats = true; break;
                    case "all": parity = keys = beats = trans = statics = pans = edges = ride = kseq = true; break;
                    case "everything": parity = keys = beats = trans = statics = pans = edges = encode = ride = kseq = true; break;
                    case "materials": mats = true; break;
                    case "matcontext": matCtx = true; break;
                    default:
                        // a set registered by an off-path QA rig (QaRigs; e.g. "kraken" from QA_KrakenRig), else unknown
                        if (QaRigs.IsRegistered(p)) { if (!rigSets.Contains(p)) rigSets.Add(p); break; }
                        plan.unknownSets.Add(p); continue;
                }
                plan.sets.Add(p);
            }

            if (parity) plan.parity = ParityShot();
            if (keys) plan.stills.AddRange(KeyShotStills(opt.keyshots4k));
            if (beats) plan.stills.AddRange(BeatStills());
            if (mats) plan.stills.AddRange(MaterialSheet.Stills());
            for (int i = 0; i < rigSets.Count; i++) plan.stills.AddRange(EndingVariants(QaRigs.Stills(rigSets[i])));

            // sequences: the start first, the ending last (it finishes the game)
            if (edges) plan.sequences.Add(StartSequence(opt.seqWidth, opt.seqHeight));
            if (trans) plan.sequences.AddRange(TransitionSequences(opt.seqWidth, opt.seqHeight, opt.walkSpeed));
            if (kseq) plan.sequences.AddRange(KrakenSequences(opt.seqWidth, opt.seqHeight));
            if (statics) plan.sequences.AddRange(StaticSequences(opt.seqWidth, opt.seqHeight));
            if (pans) plan.sequences.AddRange(PanSequences(opt.seqWidth, opt.seqHeight));
            if (tprec) plan.sequences.AddRange(TPrecSequences(opt.seqWidth, opt.seqHeight));
            // one goal per run (the player's finished state and the UI ending latch): with edges, seq_end owns the goal and
            // seq_turnback stops 0.5 m before it; alone, seq_turnback runs on through t_g 0-45
            if (ride) plan.sequences.AddRange(RideSequences(opt.seqWidth, opt.seqHeight, !edges));
            if (encode) plan.sequences.AddRange(EncodeClips(opt.encode4k, opt.walkSpeed));
            if (mats) plan.sequences.AddRange(MaterialSheet.Sequences(opt.seqWidth, opt.seqHeight));
            if (matCtx) plan.sequences.AddRange(MaterialSheet.ContextSequences(opt.seqWidth, opt.seqHeight));
            for (int i = 0; i < rigSets.Count; i++) plan.sequences.AddRange(QaRigs.Sequences(rigSets[i], opt.seqWidth, opt.seqHeight));
            if (edges) plan.sequences.Add(EndSequence(opt.seqWidth, opt.seqHeight, opt.walkSpeed));

            if (opt.only != null && opt.only.Length > 0)
            {
                plan.stills.RemoveAll(st => !Matches(st.tag, opt.only));
                plan.sequences.RemoveAll(sq => !Matches(sq.name, opt.only));
                if (plan.parity != null && !Matches(plan.parity.tag, opt.only)) plan.parity = null;
            }
            return plan;
        }

        static bool Matches(string name, string[] only)
        {
            for (int i = 0; i < only.Length; i++)
            {
                string o = only[i] != null ? only[i].Trim() : "";
                if (o.Length > 0 && name.IndexOf(o, StringComparison.OrdinalIgnoreCase) >= 0) return true;
            }
            return false;
        }

        // ------------------------------------------------------------------ pose: token (Abyss KeyShots.TryPose)

        /// <summary>Parses pose:NAME:x:y:z:yaw:pitch:fov (fov &lt;= 0 = schedule vFOV) into a frozen-clock still.</summary>
        public static bool TryPose(string token, out StillShot shot, out string error)
        {
            shot = null; error = null;
            string[] f = token.Split(':');
            if (f.Length != 8) { error = "expected pose:NAME:x:y:z:yaw:pitch:fov"; return false; }
            var inv = CultureInfo.InvariantCulture;
            var v = new double[6];
            for (int i = 0; i < 6; i++)
                if (!double.TryParse(f[i + 2], NumberStyles.Float, inv, out v[i])) { error = "bad number '" + f[i + 2] + "'"; return false; }
            string name = f[1].Trim();
            if (name.Length == 0) { error = "empty name"; return false; }
            double s = PoseS(v[0], v[1], v[2]);
            s = Math.Max(0.0, Math.Min(MaxPoseS, s));
            double fov = v[5] > 0 ? v[5] : BulwarkSchedule.Vfov(s);
            shot = new StillShot
            {
                tag = "pose_" + name, set = "custom", baseTag = "pose_" + name, variant = Megastructure.CliArgs.Capture4K ? "4k" : "1080", width = Megastructure.CliArgs.Capture4K ? W4K : W1080, height = Megastructure.CliArgs.Capture4K ? H4K : H1080,
                s = s, x = v[0], y = v[1], z = v[2], yaw = v[3], pitch = v[4], fov = fov, ambient = AmbientForS(s),
                gameFov = v[5] <= 0 || Math.Abs(v[5] - BulwarkSchedule.Vfov(s)) < 0.01, framing = false,
            };
            return true;
        }

        /// <summary>Progress s of a free eye position: the projection onto the path from every 50 m hint, scored by the plan
        /// distance plus the height difference to the walker's eye (BwPath.EyeY), so a ledge pose (y -88.3) resolves to the
        /// ledge (s about 1450), not to the crest above it (the old single hint s 750 gave s 934 for every ledge pose).</summary>
        public static double PoseS(double x, double y, double z)
        {
            double best = 0.0, bestScore = double.MaxValue;
            for (double hint = 0.0; hint <= BwPath.Length + 1e-9; hint += 50.0)
            {
                double sTry = BwPath.Project(x, z, hint, out double dist);
                double score = dist + Math.Abs(BwPath.EyeY(sTry) - y);
                if (score < bestScore) { bestScore = score; best = sTry; }
            }
            return best;
        }

        // ------------------------------------------------------------------ key shots (14.10.2)

        public static List<StillShot> KeyShotStills(bool with4k = true)
        {
            var list = new List<StillShot>(96);
            var ks = BulwarkConst.KeyShots;
            int[,] res = { { W1080, H1080 }, { W4K, H4K }, { W1610, H1610 }, { W219, H219 } };
            string[] suffix = { "", "_4k", "_1610", "_219" };
            string[] variant = { "1080", "4k", "1610", "219" };
            for (int r = 0; r < 4; r++)
            {
                if (r == 1 && !with4k) continue;
                for (int i = 0; i < ks.Length; i++)
                {
                    KeyShot k = ks[i];
                    list.Add(KeyStill(k, i, KeyTag(k) + suffix[r], res[r, 0], res[r, 1], variant[r]));
                }
            }
            return list;
        }

        /// <summary>Tag of a key shot: beat prefix + the table tag ("b02_Crest_KS04_door_sea").</summary>
        public static string KeyTag(KeyShot k) => BeatPrefix(Math.Max(0, Math.Min(FrameworkConst.BeatCount - 1, BwPath.BeatAt(k.s) - 1))) + "_" + k.tag;

        static StillShot KeyStill(KeyShot k, int index, string tag, int w, int h, string variant)
        {
            var still = new StillShot
            {
                tag = tag, set = "keyshots", baseTag = k.tag, variant = variant, width = w, height = h,
                s = k.s, x = k.x, y = k.y, z = k.z, yaw = k.yaw, pitch = k.pitch, fov = k.fov,
                ambient = AmbientForS(k.s), keyShotIndex = index, gameFov = Math.Abs(k.fov - BulwarkSchedule.Vfov(k.s)) < 0.01, framing = true,
                lumaMin = k.lumaMin, lumaMax = k.lumaMax,
            };
            // the lantern rows (KS27-KS29) read correctly only at their end-camera time (tilt, sun sink, stinger): fix wave R5
            if (!double.IsNaN(k.tg)) { still.endT = k.tg; }
            return still;
        }

        /// <summary>Index of the key shot whose table tag starts with the id ("KS04" -> "KS04_crest_sea"), or -1.</summary>
        public static int KeyShotIndex(string id)
        {
            var ks = BulwarkConst.KeyShots;
            for (int i = 0; i < ks.Length; i++) if (ks[i].tag.StartsWith(id, StringComparison.Ordinal)) return i;
            return -1;
        }

        public static int FindKeyShot(string tag)
        {
            var ks = BulwarkConst.KeyShots;
            for (int i = 0; i < ks.Length; i++) if (ks[i].tag == tag || tag.IndexOf(ks[i].tag, StringComparison.Ordinal) >= 0) return i;
            return -1;
        }

        // ------------------------------------------------------------------ beat stills (11.2, 14.10.2 last paragraph)

        public static List<StillShot> BeatStills()
        {
            var list = new List<StillShot>(64);
            var b = FrameworkConst.BeatBoundaries;
            for (int i = 0; i < FrameworkConst.BeatCount; i++)
            {
                string pre = BeatPrefix(i);
                double s0 = b[i], s1 = b[i + 1], mid = 0.5 * (s0 + s1);
                list.Add(PathStill(pre + "_start", s0 + 2, 0, 0.0));
                list.Add(PathStill(pre + "_mid", mid, 0, 0.0));
                list.Add(PathStill(pre + "_end", s1 - 2, 0, 0.0));
                for (int f = 0; f < s_freeLookTags.Length; f++) list.Add(PathStill(pre + "_" + s_freeLookTags[f], mid, s_freeLookYaw[f], s_freeLookPitch[f]));
                if (i == 2) list.Add(PathStill(pre + "_look_down90", mid, 0, -89.0));   // 14.10.2: beat 3 at pitch -90 (grating)
            }
            return list;
        }

        static StillShot PathStill(string tag, double s, double yawOffset, double pitch)
        {
            SequenceSpec.EyeAt(s, out double x, out double y, out double z);
            return new StillShot
            {
                tag = tag, set = "beats", baseTag = tag, variant = "1080", width = W1080, height = H1080,
                s = s, x = x, y = y, z = z, yaw = BwMath.WrapDeg(BwPath.HeadingDeg(s) + yawOffset), pitch = pitch, fov = BulwarkSchedule.Vfov(s),
                ambient = AmbientForS(s),
            };
        }

        static StillShot ParityShot()
        {
            var ks = BulwarkConst.KeyShots;
            if (ks.Length == 0) return null;
            KeyShot k = ks[0];
            return new StillShot
            {
                tag = "parity", set = "parity", baseTag = k.tag, variant = "window",
                s = k.s, x = k.x, y = k.y, z = k.z, yaw = k.yaw, pitch = k.pitch, fov = k.fov, ambient = AmbientForS(k.s),
            };
        }

        // ------------------------------------------------------------------ sequences (11.6, 14.6, 14.10)

        public static List<SequenceSpec> TransitionSequences(int w, int h, double speed = FrameworkConst.CinematicSpeed)
        {
            var list = new List<SequenceSpec>(12);
            var b = FrameworkConst.BeatBoundaries;
            double half = WindowHalfSeconds * speed;
            for (int i = 1; i < FrameworkConst.BeatCount; i++)
            {
                double a = Math.Max(0, b[i] - half), e = Math.Min(BwPath.Length, b[i] + half);
                list.Add(GazeSeq("seq_b" + i.ToString("D2", CultureInfo.InvariantCulture) + "_b" + (i + 1).ToString("D2", CultureInfo.InvariantCulture),
                                 "transitions", "boundary s " + b[i].ToString("0", CultureInfo.InvariantCulture) + " +-10 s (AutoWalker gaze)", a, e, SeqFps, w, h, speed));
            }
            // the LocalLightPool through the beat-1 lamps (lamps at z 36 and 48 join and leave the 4-light set; Part 10 #6:
            // luma step <= 1.2 levels per frame)
            list.Add(GazeSeq("seq_lamps_b01", "transitions", "light pool through the beat-1 lamps s 30-54 (AutoWalker gaze)", 30.0, 54.0, SeqFps, w, h, speed));
            for (int k = 0; k < ExtraWindows.GetLength(0); k++)
            {
                double a = ExtraWindows[k, 0], e = ExtraWindows[k, 1];
                list.Add(GazeSeq("seq_s" + ((int)a).ToString("D4", CultureInfo.InvariantCulture) + "_" + ((int)e).ToString("D4", CultureInfo.InvariantCulture),
                                 "transitions", "window s " + a + "-" + e + " (AutoWalker gaze)", a, e, SeqFps, w, h, speed));
            }
            return list;
        }

        /// <summary>A walk sequence that looks where the AutoWalker looks (SeqMode.WalkGaze).</summary>
        public static SequenceSpec GazeSeq(string name, string set, string purpose, double s0, double s1, int fps, int w, int h,
                                           double speed = FrameworkConst.CinematicSpeed)
        {
            var q = WalkSeq(name, set, purpose, s0, s1, fps, w, h, speed);
            q.mode = SeqMode.WalkGaze;
            return q;
        }

        /// <summary>The kraken moments of A1 4.10 at 30 fps with the AutoWalker gaze (s windows of the addendum; K5, the reef
        /// slap at s ~821-824, gets its own window inside K4's).</summary>
        public static readonly (string name, double s0, double s1, string purpose)[] KrakenWindows =
        {
            ("seq_k1", 136.0, 184.0, "K1 crown off the crest (KS04)"),
            ("seq_k2", 344.0, 414.0, "K2 arm at the tower (KK1)"),
            ("seq_k3", 452.0, 520.0, "K3 shadow under the spill gates (KS07)"),
            ("seq_k4", 738.0, 842.0, "K4 dome, head and eyes at the reef (KS10, KK2)"),
            ("seq_k5", 806.0, 842.0, "K5 the slap column at the reef (KS11)"),
            ("seq_k6", 994.0, 1022.0, "K6 the porthole (KK3)"),
            ("seq_k7", 1100.0, 1345.0, "K7 the window hall: arm, glass, eye (KS16, KK4, KS17, KS18)"),
        };

        public static List<SequenceSpec> KrakenSequences(int w, int h)
        {
            var list = new List<SequenceSpec>(KrakenWindows.Length);
            foreach (var k in KrakenWindows) list.Add(GazeSeq(k.name, "kseq", k.purpose + " (AutoWalker gaze)", k.s0, k.s1, SeqFps, w, h));
            return list;
        }

        /// <summary>Real-game frames of a ride window: from s0 the player rides / walks as under the AutoWalker until s1 (or
        /// through the goal and afterGoal seconds when afterGoal is set).</summary>
        public static SequenceSpec RealWalkSeq(string name, string set, string purpose, double s0, double s1, int fps, int w, int h,
                                               double afterGoal = double.NaN)
        {
            double sec = (s1 - s0) / FrameworkConst.CinematicSpeed + 1.0 + (double.IsNaN(afterGoal) ? 0.0 : afterGoal + 1.0);
            return new SequenceSpec
            {
                name = name, set = set, purpose = purpose, mode = SeqMode.RealWalk, fps = fps, width = w, height = h,
                s0 = s0, s1 = s1, speed = FrameworkConst.CinematicSpeed, frames = (int)Math.Ceiling(sec * fps),
                afterGoal = afterGoal, ambient0 = AmbientForS(s0),
            };
        }

        /// <summary>Seconds of the turn-back sequence after the goal (lead task S2-1: t_g 0-45).</summary>
        public const double TurnbackAfterGoal = 45.0;

        /// <summary>The ride sequences of A1 4.10 re-keyed to EndingPath (they follow the ride constants: R1 moves the shuttle).</summary>
        public static List<SequenceSpec> RideSequences(int w, int h, bool turnbackThroughGoal)
        {
            double s0 = EndingPath.S0;
            var list = new List<SequenceSpec>(4)
            {
                RealWalkSeq("seq_ride_depart", "ride", "boarding, doors, the drop begins (s S0 .. S0 + 60)", s0, s0 + 60.0, SeqFps, w, h),
                RealWalkSeq("seq_ride_arrive", "ride", "the descent's last 7 s, the stop at the foot, the transfer", EndingPath.SDescent1 - 17.5, EndingPath.STransfer1 + 2.5, SeqFps, w, h),
                RealWalkSeq("seq_shuttle", "ride", "the shuttle run at 60 fps (lamp strobing, the receding wall)", EndingPath.STransfer1 - 0.5, EndingPath.SShuttle1, 60, w, h),
                turnbackThroughGoal
                    ? RealWalkSeq("seq_turnback", "ride", "arrival, the platform walk, the goal and t_g 0-45 (end camera, stinger, crest dark)", EndingPath.SShuttle1, EndingPath.SGoal, SeqFps, w, h, TurnbackAfterGoal)
                    : RealWalkSeq("seq_turnback", "ride", "arrival and the platform walk to 0.5 m before the goal (seq_end owns the goal)", EndingPath.SShuttle1, EndingPath.SGoal - 0.5, SeqFps, w, h),
            };
            return list;
        }

        /// <summary>T-PREC (A1 4.8; lead ruling R6): the rest pose of KS27 (the Keeper's Lantern 1.9 m ahead) for 120 static
        /// frames with a frozen clock and a 0.1 px/frame yaw pan; the cabin at s ~1690 (the descent's end, the cabin and the
        /// camera 5.9 km down) for 120 real-game frames with a fixed world look at the cabin's north frame.</summary>
        public static List<SequenceSpec> TPrecSequences(int w, int h)
        {
            double s = EndingPath.SRest;
            SequenceSpec.EyeAt(s, out double x, out double y, out double z);
            double fov = 66.0;
            double pxDeg = fov / Math.Max(1, h);   // deg per px at the frame centre (vertical; square pixels)
            var list = new List<SequenceSpec>(3)
            {
                new SequenceSpec
                {
                    name = "seq_tprec_static", set = "tprec", purpose = "T-PREC: 120 static frames at the KS27 rest pose (lantern 1.9 m, platform rail)",
                    mode = SeqMode.Static, fps = SeqFps, width = w, height = h, frames = StaticFrames, s0 = s, x = x, y = y, z = z,
                    yaw = BulwarkConst.EndLookYaw, pitch = 0.0, fov = fov, advanceAmbient = false, ambient0 = AmbientForS(s),
                },
                new SequenceSpec
                {
                    name = "seq_tprec_pan", set = "tprec", purpose = "T-PREC: 0.1 px/frame yaw pan at the KS27 rest pose (lantern-rail centroid jitter < 0.1 px)",
                    mode = SeqMode.PanYaw, fps = SeqFps, width = w, height = h, frames = StaticFrames, s0 = s, x = x, y = y, z = z,
                    yaw = BulwarkConst.EndLookYaw, pitch = 0.0, fov = fov, panStepDeg = SlowPanPx * pxDeg, advanceAmbient = false, ambient0 = AmbientForS(s),
                },
            };
            var cab = RealWalkSeq("seq_tprec_cabin", "tprec", "T-PREC: the cabin at s ~1690 with a fixed world look (cabin-to-camera jitter)", 1688.0, 1688.0 + StaticFrames * FrameworkConst.CinematicSpeed / SeqFps, SeqFps, w, h);
            cab.fixedLook = true; cab.yaw = 0.0; cab.pitch = -10.0; cab.frames = StaticFrames;
            list.Add(cab);
            return list;
        }

        /// <summary>The 16:10 (1680x1050) and 21:9 (2560x1080) variants of the ending rig's 1080p game-FOV stills (A1 4.9: 1080p,
        /// 4K, 16:10, 21:9); a rig still at a wrong 21:9 size (2520x1080) is corrected to 2560x1080.</summary>
        public static List<StillShot> EndingVariants(List<StillShot> stills)
        {
            if (stills == null) return new List<StillShot>();
            var outList = new List<StillShot>(stills.Count * 3);
            var tags = new HashSet<string>(StringComparer.Ordinal);
            foreach (var st in stills) tags.Add(st.tag);
            foreach (var st in stills)
            {
                if (st.height == H219 && st.width > W1080 && st.width != W219) { st.width = W219; st.variant = "219"; }   // e.g. 2520x1080
                outList.Add(st);
            }
            foreach (var st in stills)
            {
                if (st.tag == null || !st.tag.StartsWith("b08_", StringComparison.Ordinal) || st.width != W1080 || st.height != H1080 || !st.gameFov) continue;
                AddVariant(outList, tags, st, "_1610", W1610, H1610, "1610");
                AddVariant(outList, tags, st, "_219", W219, H219, "219");
            }
            return outList;
        }

        static void AddVariant(List<StillShot> list, HashSet<string> tags, StillShot st, string suffix, int w, int h, string variant)
        {
            string tag = st.tag + suffix;
            if (tags.Contains(tag)) return;
            tags.Add(tag);
            list.Add(new StillShot
            {
                tag = tag, set = st.set, baseTag = st.baseTag, variant = variant, width = w, height = h,
                s = st.s, x = st.x, y = st.y, z = st.z, yaw = st.yaw, pitch = st.pitch, fov = st.fov, ambient = st.ambient,
                keyShotIndex = st.keyShotIndex, gameFov = st.gameFov, framing = st.framing, lumaMin = st.lumaMin, lumaMax = st.lumaMax,
                endT = st.endT,
            });
        }

        public static SequenceSpec WalkSeq(string name, string set, string purpose, double s0, double s1, int fps, int w, int h,
                                           double speed = FrameworkConst.CinematicSpeed, double yaw = 0, bool yawWorld = false, double pitch = 0.0)
        {
            return new SequenceSpec
            {
                name = name, set = set, purpose = purpose, mode = SeqMode.Walk, fps = fps, width = w, height = h,
                s0 = s0, s1 = s1, speed = speed, frames = (int)Math.Round((s1 - s0) / speed * fps) + 1,
                yaw = yaw, yawWorld = yawWorld, pitch = pitch, ambient0 = AmbientForS(s0),
            };
        }

        public static List<SequenceSpec> StaticSequences(int w, int h)
        {
            var list = new List<SequenceSpec>(8);
            var b = FrameworkConst.BeatBoundaries;
            for (int i = 0; i < FrameworkConst.BeatCount; i++)
                list.Add(StaticSeq("seq_static_" + BeatPrefix(i), "static camera, middle of the beat", 0.5 * (b[i] + b[i + 1]), StaticFrames, w, h));
            // 14.10 acceptance statics at key-shot poses (clock running): KS04 sea flicker (14.5.2 (5)), KS08 plume flicker
            // (14.5.3 (5)), KS20 lowland and sky flicker at s 1452 (14.5.7 (6)); and the 60 s KS04 plume count (14.5.2 (7):
            // at least 3 plumes in 60 s) at 4 fps (241 frames, 0.25 s apart)
            AddKeyStatic(list, "KS04", "seq_static_KS04", "14.5.2 (5): sea flicker at the KS04 framing", StaticFrames, SeqFps, w, h);
            AddKeyStatic(list, "KS08", "seq_static_KS08", "14.5.3 (5): plume flicker at the KS08 framing", StaticFrames, SeqFps, w, h);
            AddKeyStatic(list, "KS20", "seq_static_KS20", "14.5.7 (6): lowland and sky flicker at s 1452", StaticFrames, SeqFps, w, h);
            AddKeyStatic(list, "KS04", "seq_plumes_KS04", "14.5.2 (7): plume count over 60 s at the KS04 framing (4 fps)", PlumeCountFrames, PlumeCountFps, w, h);
            return list;
        }

        /// <summary>60 s at 4 fps for the plume count (14.5.2 (7)).</summary>
        public const int PlumeCountFps = 4, PlumeCountFrames = 241;

        static void AddKeyStatic(List<SequenceSpec> list, string id, string name, string purpose, int frames, int fps, int w, int h)
        {
            int i = KeyShotIndex(id);
            if (i < 0) return;
            KeyShot k = BulwarkConst.KeyShots[i];
            list.Add(new SequenceSpec
            {
                name = name, set = "statics", purpose = purpose, mode = SeqMode.Static, fps = fps, width = w, height = h, frames = frames,
                s0 = k.s, s1 = k.s, x = k.x, y = k.y, z = k.z, yaw = k.yaw, pitch = k.pitch, fov = k.fov,
                ambient0 = AmbientForS(k.s), advanceAmbient = true,
            });
        }

        /// <summary>A 0.5 px/frame yaw pan (ambient frozen) at a pose.</summary>
        static SequenceSpec PanAt(string name, string purpose, double s, double x, double y, double z, double yaw, double pitch, double fov, int w, int h)
        {
            return new SequenceSpec
            {
                name = name, set = "pans", purpose = purpose, mode = SeqMode.PanYaw, fps = SeqFps, width = w, height = h, frames = PanFrames,
                s0 = s, s1 = s, x = x, y = y, z = z, yaw = yaw, pitch = pitch, fov = fov,
                panStepDeg = QaProjection.PanStepDeg(PanPx, fov, h), ambient0 = AmbientForS(s), advanceAmbient = false,
            };
        }

        static SequenceSpec StaticSeq(string name, string purpose, double s, int frames, int w, int h)
        {
            SequenceSpec.EyeAt(s, out double x, out double y, out double z);
            return new SequenceSpec
            {
                name = name, set = "statics", purpose = purpose, mode = SeqMode.Static, fps = SeqFps, width = w, height = h, frames = frames,
                s0 = s, s1 = s, x = x, y = y, z = z, yaw = BwPath.HeadingDeg(s), pitch = 0.0, fov = BulwarkSchedule.Vfov(s),
                ambient0 = AmbientForS(s), advanceAmbient = true,
            };
        }

        public static List<SequenceSpec> PanSequences(int w, int h)
        {
            var list = new List<SequenceSpec>(9);
            var b = FrameworkConst.BeatBoundaries;
            for (int i = 0; i < FrameworkConst.BeatCount; i++)
            {
                double s = 0.5 * (b[i] + b[i + 1]);
                SequenceSpec.EyeAt(s, out double x, out double y, out double z);
                double fov = BulwarkSchedule.Vfov(s);
                list.Add(new SequenceSpec
                {
                    name = "seq_pan_" + BeatPrefix(i), set = "pans", purpose = "0.5 px/frame yaw pan (thin lines, H7)",
                    mode = SeqMode.PanYaw, fps = SeqFps, width = w, height = h, frames = PanFrames,
                    s0 = s, s1 = s, x = x, y = y, z = z, yaw = BwPath.HeadingDeg(s), pitch = 0.0, fov = fov,
                    panStepDeg = QaProjection.PanStepDeg(PanPx, fov, h), ambient0 = AmbientForS(s), advanceAmbient = false,
                });
            }
            {
                // the lee rail of beat 7 (0.1 px per frame, H7): looking left toward the rail from the ledge
                double s = 0.5 * (FrameworkConst.BeatBoundaries[6] + BwPath.Length);
                SequenceSpec.EyeAt(s, out double x, out double y, out double z);
                double fov = BulwarkSchedule.Vfov(s);
                list.Add(new SequenceSpec
                {
                    name = "seq_pan_rail_slow", set = "pans", purpose = "0.1 px/frame pan over the lee rail (H7)",
                    mode = SeqMode.PanYaw, fps = SeqFps, width = w, height = h, frames = SlowPanFrames,
                    s0 = s, s1 = s, x = x, y = y, z = z, yaw = BwMath.WrapDeg(BwPath.HeadingDeg(s) - 60.0), pitch = -10.0, fov = fov,
                    panStepDeg = QaProjection.PanStepDeg(SlowPanPx, fov, h), ambient0 = AmbientForS(s), advanceAmbient = false,
                });
            }
            // 14.10 acceptance pans at key-shot poses: the sea beyond 1 km at KS04 (H1: pan std <= 2.0 beyond 1 km), the KS09
            // grating at pitch -58 and -90 (14.5.3 (4), T3), the KS20 framing at s 1452 and the city lights at s 1470
            // (14.7 H4: no shimmer; ledge strict 1.5)
            int k4 = KeyShotIndex("KS04"), k9 = KeyShotIndex("KS09"), k20 = KeyShotIndex("KS20");
            if (k4 >= 0) { var k = BulwarkConst.KeyShots[k4]; list.Add(PanAt("seq_pan_KS04_sea", "H1: sea pan beyond 1 km at the KS04 framing", k.s, k.x, k.y, k.z, k.yaw, k.pitch, k.fov, w, h)); }
            if (k9 >= 0)
            {
                var k = BulwarkConst.KeyShots[k9];
                list.Add(PanAt("seq_pan_KS09_p58", "14.5.3 (4): grating pan at pitch -58 (KS09)", k.s, k.x, k.y, k.z, k.yaw, k.pitch, k.fov, w, h));
                list.Add(PanAt("seq_pan_KS09_p90", "14.5.3 (4) / T3: grating pan at pitch -90", k.s, k.x, k.y, k.z, k.yaw, -89.5, k.fov, w, h));
            }
            if (k20 >= 0)
            {
                var k = BulwarkConst.KeyShots[k20];
                list.Add(PanAt("seq_pan_KS20", "14.5.7: ledge pan at the KS20 framing (s 1452)", k.s, k.x, k.y, k.z, k.yaw, k.pitch, k.fov, w, h));
                // the same framing from the ledge at s 1470 (city lights N = 0.9; 14.5.7 (2) counts the blobs there)
                double s70 = CityLightS;
                BwPath.World(s70, 0.0, out _, out _, out double z70);
                list.Add(PanAt("seq_pan_s1470_city", "14.7 H4: city-light pan at s 1470 (ledge)", s70, k.x, k.y, z70, k.yaw, k.pitch, k.fov, w, h));
            }
            return list;
        }

        /// <summary>14.5.7 (2): the city-light count is taken at s 1470.</summary>
        public const double CityLightS = 1470.0;

        public static SequenceSpec StartSequence(int w, int h)
        {
            return new SequenceSpec
            {
                name = "seq_start", set = "edges", purpose = "first 60 frames: start fade (defect 22)", mode = SeqMode.RealStart,
                fps = SeqFps, width = w, height = h, frames = EdgeFrames, s0 = 0, s1 = 0, pitch = 0.0,
            };
        }

        public static SequenceSpec EndSequence(int w, int h, double speed = FrameworkConst.CinematicSpeed)
        {
            // the real goal is the Keeper's Lantern on the platform (EndingPath.SGoal; the old 1498.5 is the cabin door)
            double s0 = EndSequenceS0;
            int maxFrames = (int)Math.Ceiling((EndLeadIn / speed + 2.0 + FrameworkConst.EndTotal + EndTail) * SeqFps);
            return new SequenceSpec
            {
                name = "seq_end", set = "edges", purpose = "ending: the platform walk to the Keeper's Lantern, trigger, stop ramp, end camera, stinger, crest dark, fade, line (defect 22)", mode = SeqMode.RealEnd,
                fps = SeqFps, width = w, height = h, frames = maxFrames, s0 = s0, s1 = EndingPath.SRest, speed = speed,
                pitch = 0.0, afterGoal = EndSecondsAfterTrigger,
            };
        }

        /// <summary>seq_end starts EndLeadIn metres before the real goal trigger (EndingPath.SGoal).</summary>
        public static double EndSequenceS0 => EndingPath.SGoal - EndLeadIn;

        /// <summary>Seconds of frames kept after the goal trigger in seq_end (the whole EndTotal plus a margin).</summary>
        public static double EndSecondsAfterTrigger => FrameworkConst.EndTotal + EndTail;

        // ------------------------------------------------------------------ encode clips (11.12, 14.10.3)

        /// <summary>
        /// Gradient views for the encode test: clean-gradient regions only (storm sky, dusk sky, deep water veil, mist). Each
        /// is captured twice by Tools/qa/run_encode_clips.sh: with the static dither and with -dither off (the undithered
        /// baseline of the 0.6x gate on the 4K CRF 23 decode).
        /// </summary>
        public static List<SequenceSpec> EncodeClips(bool also4k, double speed = FrameworkConst.CinematicSpeed)
        {
            var list = new List<SequenceSpec>(8);
            AddEncode(list, also4k, "enc_b02_storm_sky", "storm sky above the sea horizon", 200.0, EncodeSeconds, speed, 80.0, true, 8.0);
            AddEncode(list, also4k, "enc_b03_mist", "mist river below the spillway", 590.0, EncodeSeconds, speed, -75.0, true, -4.0);
            AddEncode(list, also4k, "enc_b06_water_veil", "deep water veil behind the glass", 1110.0, EncodeSeconds, speed, 90.0, true, 0.0);
            AddEncode(list, also4k, "enc_b07_dusk_sky", "dusk sky over the lowland", 1455.0, EncodeSeconds, speed, -27.5, true, 10.0);
            return list;
        }

        static void AddEncode(List<SequenceSpec> list, bool also4k, string name, string purpose, double s0, double seconds, double speed, double yaw, bool yawWorld, double pitch)
        {
            double s1 = Math.Min(BwPath.Length, s0 + seconds * speed);
            var a = WalkSeq(name, "encode", purpose, s0, s1, EncodeFps, W1080, H1080, speed, yaw, yawWorld, pitch);
            a.frames = (int)Math.Round(seconds * EncodeFps);
            list.Add(a);
            if (also4k)
            {
                var b = WalkSeq(name + "_4k", "encode", purpose + " (4K)", s0, s1, EncodeFps, W4K, H4K, speed, yaw, yawWorld, pitch);
                b.frames = a.frames;
                list.Add(b);
            }
        }
    }

    /// <summary>One material sheet of the "materials" capture set (MASTER_PROMPT 8.11.8).</summary>
    public sealed class MaterialSheetDef
    {
        public string Name;          // "b02_crest"
        public int Beat;             // 1..7
        public double S;             // schedule s: key light, sky, fog, exposure and vFOV of the beat
        public double X, Y, Z;       // centre of the row on the stage floor (world)
        public double Yaw;           // camera view yaw toward the row (deg); chosen with the beat's sun behind the camera
        public bool Booth;           // enclosed booth lit by the beat's lamps (interior beats)
        public Pal Lamp = Pal.SodiumWarm;
        public double LampIntensity, LampRange, LampHeight;
        public int LampCount;
        public string Floor;         // MaterialLibrary field of the stage floor
        public string[] Items;       // MaterialLibrary fields left to right as the camera sees them ("Field@variant" allowed)
        public bool Grating;         // horizontal grating 1.0 m above a wet floor, right of the row (beat 3 slots)
        public bool GratingWall;     // vertical grating panel, right of the row (beat 7 tunnel)
        public bool RoadPatch;       // the crest deck sample above the real road (lane numerals at their true x, z)
        public bool CrustWall;       // the far BwArmour wall and drum with the crust zone (KS10-like distances, beat 4)
        public bool GratingField;    // the 12 x 50 m world-aligned grating field (fade regime 6 .. 1.5 px per period)
    }

    /// <summary>
    /// Layout of the material sheets (8.11.8): per sheet a row of stands (slab 1.6 x 0.25 x 1.6 m, sphere r 0.45 m, wall panel
    /// 1.7 x 1.9 m) on a 28 x 46 m stage floor, off the path (60 m west of the lee face, 20 m above the road, or 40 m above
    /// the real road for the road patch). Interior beats get a closed booth lit by the beat's lamps (14.2.3: sodium x2.5 with
    /// the gallery's real lights 9 m / 1.6, tungsten x3 at 8 m, the beat-6 bollard x1.5 at 0.9 m). Pure data: the
    /// QA_MaterialSheet builder and the stills read the same numbers.
    /// </summary>
    public static class MaterialSheet
    {
        public const double ItemPitch = 2.4, SlabSize = 1.6, SlabHeight = 0.25, SphereRadius = 0.45;
        public const double PanelWidth = 1.7, PanelHeight = 1.9, PanelThickness = 0.2, PanelBack = 0.75;
        public const double RigX = -60.0, RigY = 20.0;
        public const double RoadPatchY = 40.0, RoadPatchZ0 = 284.0, RoadPatchZ1 = 360.0;
        public const double GratingSize = 3.2, GratingLift = 1.0;
        public const double StageHalfWidth = 14.0, StageBack = 40.0, StageFront = 6.0;
        public const double BoothHalfWidth = 9.5, BoothBack = 38.0, BoothFront = 4.0, BoothHeight = 6.0;
        // grating field (world-aligned, bars along world z, 1.0 m above a wet floor at RigY), between the b03 and b04 sheets
        // where no sheet view looks: x -66..-54, z 670..720
        public const double FieldX0 = -66.0, FieldX1 = -54.0, FieldZ0 = 670.0, FieldZ1 = 720.0;
        // crust wall in the b04 sheet frame (x right, z along the view): a 40 x 12 m face at local z 90 plus a 5 m drum at
        // local (-13, 80), on a base slab; the sample's sea level 3 m above its base (crust about 2..6 m, foam at 3, wet to 8)
        public const double CrustWallZ = 90.0, CrustWallHalfWidth = 20.0, CrustWallHeight = 12.0, CrustWallSea = 3.0;
        public const double CrustDrumX = -13.0, CrustDrumZ = 80.0, CrustDrumRadius = 5.0;

        public static readonly MaterialSheetDef[] Sheets =
        {
            new MaterialSheetDef { Name = "b01_gallery", Beat = 1, S = 60, X = RigX, Y = RigY, Z = 60, Yaw = 270, Booth = true,
                Lamp = Pal.SodiumWarm, LampIntensity = 1.6, LampRange = 9, LampHeight = 3.0, LampCount = 2, Floor = "Concrete",
                Items = new[] { "Concrete", "ConcreteWet", "Steel", "SteelOchre", "LampSodium" } },
            new MaterialSheetDef { Name = "b02_crest", Beat = 2, S = 300, X = RigX, Y = RigY, Z = 300, Yaw = 250, Floor = "Road",
                Items = new[] { "Road", "Concrete", "ConcreteDark", "Steel", "LampSodium" }, RoadPatch = true },
            new MaterialSheetDef { Name = "b03_spillway", Beat = 3, S = 600, X = RigX, Y = RigY, Z = 600, Yaw = 250, Floor = "ConcreteWet",
                Items = new[] { "ConcreteWet", "Concrete", "Steel", "Road" }, Grating = true, GratingField = true },
            new MaterialSheetDef { Name = "b04_reef", Beat = 4, S = 776, X = RigX, Y = RigY, Z = 776, Yaw = 300, Floor = "Concrete",
                Items = new[] { "Armour@sea", "Armour", "Armour@gauge", "Concrete", "SteelOchre" }, CrustWall = true },
            new MaterialSheetDef { Name = "b05_stairs", Beat = 5, S = 985, X = RigX, Y = RigY, Z = 985, Yaw = 270, Booth = true,
                Lamp = Pal.Tungsten, LampIntensity = 3.0, LampRange = 8, LampHeight = 3.0, LampCount = 2, Floor = "Concrete",
                Items = new[] { "Steel", "SteelOchre", "Concrete", "ConcreteWet", "LampTungsten" } },
            new MaterialSheetDef { Name = "b06_windows", Beat = 6, S = 1230, X = RigX, Y = RigY, Z = 1230, Yaw = 270, Booth = true,
                Lamp = Pal.Tungsten, LampIntensity = 1.5, LampRange = 8, LampHeight = 0.9, LampCount = 1, Floor = "ConcreteDark",
                Items = new[] { "Concrete", "ConcreteDark", "ConcreteWet", "Steel" } },
            new MaterialSheetDef { Name = "b07_tunnel", Beat = 7, S = 1405, X = RigX, Y = RigY, Z = 1405, Yaw = 270, Booth = true,
                Lamp = Pal.SodiumWarm, LampIntensity = 1.6, LampRange = 9, LampHeight = 3.0, LampCount = 2, Floor = "ConcreteWet",
                Items = new[] { "ConcreteDark", "ConcreteWet", "Steel", "LampSodium" }, GratingWall = true },
            new MaterialSheetDef { Name = "b07_ledge", Beat = 7, S = 1452, X = RigX, Y = RigY, Z = 1452, Yaw = 125, Floor = "ConcreteDark",
                Items = new[] { "ConcreteDark", "Concrete", "Steel", "Lantern" } },
        };

        /// <summary>Camera forward (fx, fz) and right (rx, rz) of a view yaw (0 = +Z, 90 = +X).</summary>
        public static void Frame(double yawDeg, out double fx, out double fz, out double rx, out double rz)
        {
            double a = yawDeg * BwMath.Deg;
            fx = Math.Sin(a); fz = Math.Cos(a);
            rx = Math.Cos(a); rz = -Math.Sin(a);
        }

        /// <summary>Floor centre of stand i (left to right as the camera sees the row).</summary>
        public static void ItemCentre(MaterialSheetDef d, int i, out double x, out double y, out double z)
        {
            Frame(d.Yaw, out _, out _, out double rx, out double rz);
            double o = (i - 0.5 * (d.Items.Length - 1)) * ItemPitch;
            x = d.X + rx * o; y = d.Y; z = d.Z + rz * o;
        }

        /// <summary>Centre of the grating sample (horizontal: its plane; wall: its base centre), right of the row.</summary>
        public static void GratingCentre(MaterialSheetDef d, out double x, out double y, out double z)
        {
            Frame(d.Yaw, out double fx, out double fz, out double rx, out double rz);
            double o = (0.5 * (d.Items.Length - 1)) * ItemPitch + 1.0 + 0.5 * GratingSize + 0.6;
            x = d.X + rx * o - fx * 0.8; y = d.Y + (d.Grating ? GratingLift : 0.0); z = d.Z + rz * o - fz * 0.8;
        }

        static StillShot Still(MaterialSheetDef d, string what, double x, double y, double z, double yaw, double pitch)
        {
            string tag = "mat_" + d.Name + "_" + what;
            return new StillShot
            {
                tag = tag, set = "materials", baseTag = tag, variant = "1080", width = ShotPlan.W1080, height = ShotPlan.H1080,
                s = d.S, x = x, y = y, z = z, yaw = BwMath.WrapDeg(yaw), pitch = pitch, fov = BulwarkSchedule.Vfov(d.S),
                ambient = ShotPlan.AmbientForS(d.S),
            };
        }

        static MaterialSheetDef Find(string name)
        {
            for (int i = 0; i < Sheets.Length; i++) if (Sheets[i].Name == name) return Sheets[i];
            return null;
        }

        // special poses shared by stills and sequences
        public static StillShot RoadNear() { var d = Find("b02_crest"); return Still(d, "road_near", -3.0, RoadPatchY + 1.7, 294.0, 0.0, -14.0); }
        public static StillShot RoadFar() { var d = Find("b02_crest"); return Still(d, "road_far", -1.5, RoadPatchY + 1.7, RoadPatchZ0 + 1.0, 0.0, -4.0); }
        public static StillShot RoadTop() { var d = Find("b02_crest"); return Still(d, "road_top", -3.0, RoadPatchY + 3.2, 298.2, 0.0, -62.0); }
        public static StillShot GratingDown()
        {
            var d = Find("b03_spillway");
            GratingCentre(d, out double x, out double y, out double z);
            Frame(d.Yaw, out double fx, out double fz, out _, out _);
            return Still(d, "grating_down", x - fx * 1.0, y + 1.7, z - fz * 1.0, d.Yaw, -58.0);
        }
        public static StillShot GratingGrazing()
        {
            var d = Find("b03_spillway");
            GratingCentre(d, out double x, out double y, out double z);
            Frame(d.Yaw, out double fx, out double fz, out _, out _);
            return Still(d, "grating_grazing", x - fx * 3.4, y + 1.7, z - fz * 3.4, d.Yaw, -24.0);
        }
        public static StillShot GratingWall()
        {
            var d = Find("b07_tunnel");
            GratingCentre(d, out double x, out double y, out double z);
            Frame(d.Yaw, out double fx, out double fz, out _, out _);
            return Still(d, "grating_wall", x - fx * 1.8, y + 1.7, z - fz * 1.8, d.Yaw, 0.0);
        }

        /// <summary>World position of a point given in a sheet's frame (x right, y up from the stage, z along the view yaw).</summary>
        public static void Local(MaterialSheetDef d, double lx, double ly, double lz, out double x, out double y, out double z)
        {
            Frame(d.Yaw, out double fx, out double fz, out double rx, out double rz);
            x = d.X + rx * lx + fx * lz; y = d.Y + ly; z = d.Z + rz * lx + fz * lz;
        }

        // grating field views (beat 3 light): along the bars (yaw 0: the across-bar period is 6 px at 12 m .. 1.5 px at 45 m)
        // and across them (yaw 90: the foreshortened period is 6 px at 4.4 m .. 1.5 px at 8.7 m)
        public static StillShot FieldAlong()
        {
            var d = Find("b03_spillway");
            return Still(d, "field_along", 0.5 * (FieldX0 + FieldX1), d.Y + GratingLift + 1.7, FieldZ0 - 2.0, 0.0, -8.0);
        }
        public static StillShot FieldAcross()
        {
            var d = Find("b03_spillway");
            return Still(d, "field_across", FieldX0 - 1.0, d.Y + GratingLift + 1.7, 0.5 * (FieldZ0 + FieldZ1), 90.0, -14.0);
        }
        // crust wall views (beat 4 light): 80 m (KS10-like), 20 m (the 2-4 px fade of the shells) and 3 m (detail)
        static StillShot CrustView(string what, double back, double pitch)
        {
            var d = Find("b04_reef");
            Local(d, 6.0, 1.7, CrustWallZ - back, out double x, out double y, out double z);
            return Still(d, what, x, y, z, d.Yaw, pitch);
        }
        public static StillShot CrustFar() => CrustView("crust_far", 80.0, 1.5);
        public static StillShot CrustMid() => CrustView("crust_mid", 20.0, 4.0);
        public static StillShot CrustNear() => CrustView("crust_near", 3.0, 8.0);

        /// <summary>Row, two close-ups and a far view per sheet, plus the road, grating, grating-field and crust-wall views.</summary>
        public static List<StillShot> Stills()
        {
            var list = new List<StillShot>(48);
            for (int k = 0; k < Sheets.Length; k++)
            {
                var d = Sheets[k];
                Frame(d.Yaw, out double fx, out double fz, out double rx, out double rz);
                list.Add(Still(d, "row", d.X - fx * 7.0, d.Y + 2.0, d.Z - fz * 7.0, d.Yaw, -11.0));
                for (int i = 0; i < Math.Min(2, d.Items.Length); i++)
                {
                    ItemCentre(d, i, out double x, out double y, out double z);
                    list.Add(Still(d, "close" + i, x - fx * 1.55 + rx * 0.25, y + 1.25, z - fz * 1.55 + rz * 0.25, d.Yaw, -16.0));
                }
                list.Add(Still(d, "far", d.X - fx * 34.0, d.Y + 1.7, d.Z - fz * 34.0, d.Yaw, -2.5));
            }
            list.Add(RoadNear());
            list.Add(RoadFar());
            list.Add(RoadTop());
            list.Add(GratingDown());
            list.Add(GratingGrazing());
            list.Add(GratingWall());
            list.Add(FieldAlong());
            list.Add(FieldAcross());
            list.Add(CrustFar());
            list.Add(CrustMid());
            list.Add(CrustNear());
            return list;
        }

        static SequenceSpec FromStill(StillShot st, string name, SeqMode mode, int w, int h)
        {
            return new SequenceSpec
            {
                name = name, set = "materials", purpose = "material shimmer (8.11.6, Part 10 #3, #15)", mode = mode, fps = ShotPlan.SeqFps,
                width = w, height = h, frames = ShotPlan.PanFrames, s0 = st.s, s1 = st.s, x = st.x, y = st.y, z = st.z,
                yaw = st.yaw, pitch = st.pitch, fov = st.fov, ambient0 = st.ambient, advanceAmbient = false,
                panStepDeg = QaProjection.PanStepDeg(ShotPlan.PanPx, st.fov, h), stepMetres = 0.003,
            };
        }

        /// <summary>
        /// 0.5 px/frame pans over the road patch, the far floor, the near grating, the grating field (yaw pan along the bars,
        /// pitch pan across them) and the far crust wall; 3 mm/frame side steps over the near grating and across the bars of
        /// the field (one full 80 mm period in 27 frames).
        /// </summary>
        public static List<SequenceSpec> Sequences(int w, int h)
        {
            var d = Find("b02_crest");
            Frame(d.Yaw, out double fx, out double fz, out _, out _);
            var far = Still(d, "far", d.X - fx * 34.0, d.Y + 1.7, d.Z - fz * 34.0, d.Yaw, -2.5);
            return new List<SequenceSpec>
            {
                FromStill(RoadNear(), "seq_mat_pan_road", SeqMode.PanYaw, w, h),
                FromStill(far, "seq_mat_pan_floor_far", SeqMode.PanYaw, w, h),
                FromStill(GratingDown(), "seq_mat_pan_grating", SeqMode.PanYaw, w, h),
                FromStill(GratingGrazing(), "seq_mat_step_grating", SeqMode.Steps, w, h),
                FromStill(FieldAlong(), "seq_mat_pan_field_along", SeqMode.PanYaw, w, h),
                FromStill(FieldAcross(), "seq_mat_pan_field_across", SeqMode.PanPitch, w, h),
                FromStill(FieldAlong(), "seq_mat_step_field_along", SeqMode.Steps, w, h),
                FromStill(CrustFar(), "seq_mat_pan_crust_far", SeqMode.PanYaw, w, h),
            };
        }

        public const double DollyMetres = 1.5;   // forward dollies: 1.5 m in 30 frames at 30 fps = 1.5 m/s (the walk speed)

        static SequenceSpec Ctx(string name, string purpose, SeqMode mode, double s, double x, double y, double z, double yaw, double pitch, double fov, int w, int h)
        {
            var q = new SequenceSpec
            {
                name = name, set = "matcontext", purpose = purpose, mode = mode, fps = ShotPlan.SeqFps, width = w, height = h,
                frames = ShotPlan.PanFrames, s0 = s, s1 = s, x = x, y = y, z = z, yaw = yaw, pitch = pitch, fov = fov,
                ambient0 = ShotPlan.AmbientForS(s), advanceAmbient = false,
                panStepDeg = QaProjection.PanStepDeg(ShotPlan.PanPx, fov, h),
            };
            if (mode == SeqMode.MovingPose)
            {
                double a = yaw * BwMath.Deg;
                q.x1 = x + Math.Sin(a) * DollyMetres; q.y1 = y; q.z1 = z + Math.Cos(a) * DollyMetres; q.yaw1 = yaw; q.pitch1 = pitch;
            }
            return q;
        }

        /// <summary>In-context formwork shimmer (the real game, no rig): pans at KS05 / KS06 and in the gallery, dollies at KS06 and in the gallery.</summary>
        public static List<SequenceSpec> ContextSequences(int w, int h)
        {
            var list = new List<SequenceSpec>(5);
            var ks = BulwarkConst.KeyShots;
            int k5 = -1, k6 = -1;
            for (int i = 0; i < ks.Length; i++)
            {
                if (ks[i].tag.StartsWith("KS05", StringComparison.Ordinal)) k5 = i;
                if (ks[i].tag.StartsWith("KS06", StringComparison.Ordinal)) k6 = i;
            }
            if (k5 >= 0)
            {
                var k = ks[k5];
                list.Add(Ctx("seq_matctx_pan_ks05_leeface", "0.5 px/frame pan at KS05 (lee face formwork at grazing)", SeqMode.PanYaw, k.s, k.x, k.y, k.z, k.yaw, k.pitch, k.fov, w, h));
            }
            if (k6 >= 0)
            {
                var k = ks[k6];
                list.Add(Ctx("seq_matctx_pan_ks06_parapet", "0.5 px/frame pan at KS06 (parapet formwork receding)", SeqMode.PanYaw, k.s, k.x, k.y, k.z, k.yaw, k.pitch, k.fov, w, h));
                list.Add(Ctx("seq_matctx_dolly_ks06_parapet", "1.5 m forward dolly at KS06 (walking along the parapet)", SeqMode.MovingPose, k.s, k.x, k.y, k.z, k.yaw, k.pitch, k.fov, w, h));
            }
            {
                double s = 40.0;
                SequenceSpec.EyeAt(s, out double x, out double y, out double z);
                double yaw = BwPath.HeadingDeg(s), fov = BulwarkSchedule.Vfov(s);
                list.Add(Ctx("seq_matctx_pan_gallery", "0.5 px/frame pan in the beat-1 gallery (wall formwork receding)", SeqMode.PanYaw, s, x, y, z, yaw, 0.0, fov, w, h));
                list.Add(Ctx("seq_matctx_dolly_gallery", "1.5 m forward dolly down the beat-1 gallery", SeqMode.MovingPose, s, x, y, z, yaw, 0.0, fov, w, h));
            }
            return list;
        }

        /// <summary>True when the -capture-set list names "materials" (the builder builds the rig only then).</summary>
        public static bool Requested(string captureDir, string setList)
        {
            if (string.IsNullOrEmpty(captureDir) || string.IsNullOrEmpty(setList)) return false;
            foreach (var p in setList.Split(','))
                if (string.Equals(p.Trim(), "materials", StringComparison.OrdinalIgnoreCase)) return true;
            return false;
        }

        static int s_requestedCache = -1;
        /// <summary>Requested(CliArgs.CaptureDir, CliArgs.CaptureSet) of this run, evaluated once (allocation-free per frame).</summary>
        public static bool RequestedThisRun
        {
            get
            {
                if (s_requestedCache < 0) s_requestedCache = Requested(CliArgs.CaptureDir, CliArgs.CaptureSet) ? 1 : 0;
                return s_requestedCache == 1;
            }
        }

        /// <summary>World AABB (inner faces: floor to ceiling, wall to wall) of the booth of sheet d; false for a sheet without
        /// a booth. The QA_MaterialSheet builder puts the booth walls (0.4 m thick) just outside it.</summary>
        public static bool BoothBox(MaterialSheetDef d, out D3 min, out D3 max)
        {
            min = max = new D3(0.0, 0.0, 0.0);
            if (d == null || !d.Booth) return false;
            Frame(d.Yaw, out double fx, out double fz, out double rx, out double rz);
            double x0 = double.MaxValue, x1 = double.MinValue, z0 = double.MaxValue, z1 = double.MinValue;
            for (int i = 0; i < 4; i++)
            {
                double lx = (i & 1) == 0 ? -BoothHalfWidth : BoothHalfWidth, lz = (i & 2) == 0 ? -BoothBack : BoothFront;
                double wx = d.X + rx * lx + fx * lz, wz = d.Z + rz * lx + fz * lz;
                x0 = Math.Min(x0, wx); x1 = Math.Max(x1, wx); z0 = Math.Min(z0, wz); z1 = Math.Max(z1, wz);
            }
            min = new D3(x0, d.Y, z0);
            max = new D3(x1, d.Y + BoothHeight, z1);
            return true;
        }

        /// <summary>The booth interior box of the materials capture at schedule s: true only in a run that captures the
        /// "materials" set and when s is a booth sheet's s (+-0.5 m). For the region ambient (BwInteriorWeight, 14.6): the
        /// atmosphere owner's driver writes this box into one of its _BwIntBox slots for that frame, so the booth renders with
        /// the interior ambient (lamp colour x 0.06) of the beat it samples instead of the exterior trilight.</summary>
        public static bool BoothBoxAt(double s, out D3 min, out D3 max)
        {
            min = max = new D3(0.0, 0.0, 0.0);
            if (!RequestedThisRun) return false;
            for (int i = 0; i < Sheets.Length; i++)
                if (Sheets[i].Booth && Math.Abs(Sheets[i].S - s) < 0.5) return BoothBox(Sheets[i], out min, out max);
            return false;
        }
    }

    /// <summary>Pure heading helper (degrees) shared by the plan and the bots.</summary>
    public static class QaDriveMath
    {
        public static double HeadingDeg(double s) => SequenceSpec.HeadingAt(s);
    }
}
