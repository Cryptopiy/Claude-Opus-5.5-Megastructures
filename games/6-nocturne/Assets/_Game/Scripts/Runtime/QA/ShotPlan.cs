// ShotPlan: the capture lists of the harness as pure data (MASTER_PROMPT 11.2, 11.6, 11.12, 14.10, 14.6, 14.7), imported from
// Thread with the Nocturne sets. No UnityEngine dependency, so EditMode tests check the plan (every key shot at every
// resolution, tag format, window lengths, the -sequence names of 14.12, the run-player routing lists).
//
// Capture sets (-capture-set NAME, or -sequence NAME for the 14.12 sequence names; one set per run through run-player):
//   keyshots   every NocturneConstants.KeyShots (the 14.10 table) at 1920x1080 and 3840x2160, framing variants at
//              1680x1050 (_1610) and 2560x1080 (_219)
//   keyshots1080  the key shots at 1920x1080 only (fast smoke captures)
//   beats      per beat start / mid / end stills and the free-look stills of 14.10 from the middle of the beat (yaw +90, -90,
//              180 at pitch 0, pitch +60, and -60 for beats 3 and 4)
//   pose:NAME:x:y:z:yaw:pitch:fov   one free still at 1920x1080 (Abyss KeyShots.cs pose parser); s = progress of (x, z)
//   parity     one window capture and one RenderTexture capture of the same frame (player only, 11.2)
//   -sequence names of 14.12 (PNG frame sequences in virtual time):
//   boundaries 30 fps walk at 1.5 m/s, +-10 s (15 m) around each beat boundary of 14.6 plus s 96-130, 380-420, 1240-1300
//              (14.10 Sequences; alias "transitions")
//   statics    120 static frames per beat at 30 fps, mid-beat pose (14.10: run with -freeze-clock -rain 0)
//   signs      KS16 pose, 60 s at 30 fps, clock running (14.10 photosensitivity)
//   rain       static camera in beat 4 (rho 1.00), 120 frames at 60 fps, clock running (14.7 H5 test; run with -rain 1)
//   koi        12 s walks at 2.5 m/s and 60 fps through KS17 and KS18 with their world gaze (14.7 H6: 20 wave phases in
//              capture mode, body displacement per frame at 60 fps)
//   traffic    8 s walks at 2.5 m/s and 60 fps through KS06 (tram) and KS11 (train) (14.7 H7)
//   encode     10 s (30 s for the alley-mouth transition) at 60 fps of the gradient-heavy views (11.12, 14.7 H1 (6))
//   other sequence sets: pans (0.5 px/frame for 30 frames per beat, 14.10 shimmer; run with -rain 0 -freeze-clock),
//   edges (first 60 frames and the ending, defect 22)
//   Look v2 texture sets (TEXTURE_PLAN 7.5, TQ; run with -freeze-clock -rain 0):
//   texpans    0.5 px/frame yaw pans, 30 frames, at KS00, KS04, KS05, KS08, KS09, KS15 and KS17 (kind "texpan": shimmer and
//              swimming in Tools/qa/sequences.py, the shimmer gate against a -capture-ss 2 run of the same set)
//   dolly      fixed poses with no tower in the way (kind "dolly": popping): seq_dolly_face30, a line from 800 m to 25 m at 30 deg
//              to the normal of face B (planar part, z -100, y 150; FA's -facade-sheet replaces it when it exists), geometric
//              steps; seq_dolly_graze, the grazing pass along face B over the bridge deck (x 390-640, yaw 160 = 70 deg off
//              face B's normal, 12.3 m above the deck so the glass tubes and lamps stay out of the frame)
//   texclose   8 near stills at 1.5 m (alley wall, podium panel, plaza, bridge deck, tenement brick, noodle bar, trunk, glass tube)
//   facadesheet  every pose of FA's NocFacadeSheet.Poses() (TEXTURE_PLAN 7.2; run with -facade-sheet; found by reflection, so this
//              file does not depend on FA's); with -facade-sheet the dolly's 30 deg line runs to the sheet (seq_dolly_sheet30)
//   pose:NAME:x:y:z:yaw:pitch:fov[:s]  the optional s replaces the path projection of (x, z) (parameters, fog, vFOV, ambient)
//   composites: sequences = boundaries + statics + pans + edges;  stills = parity + keyshots + beats;
//               all = stills + sequences;  everything = all + encode + signs + rain + koi + traffic
// HeavySets / FourKSets are the lists Tools/run-player.ps1 routes through gpu-guard (6500 MiB for 4K); an EditMode test keeps
// the script in sync with them. texpans and dolly are heavy sets, and -capture-ss 2 is routed as a 4K job (TQ's REQUEST LEAD,
// carried by FX in Phase 4 wave A, D-032 9); the harness still refuses a run that was not routed so (GuardNote).
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Megastructure
{
    /// <summary>One still image of the plan.</summary>
    public sealed class StillShot
    {
        public string tag;            // unique; also the file name without ".png"
        public string set;            // keyshots, beats, parity
        public string baseTag;        // key-shot tag of NocturneConstants (key shots), else the tag
        public string variant;        // "1080", "4k", "1610", "219", "window"
        public int width, height;
        public double s, x, y, z, yaw, pitch, fov;
        public double ambient;        // ambient clock value used for the shot (deterministic)
        public int keyShotIndex = -1;
        public bool gameFov = true;   // FOV equals the in-game FOV(s) (framing acceptances use these)
        public bool framing;          // evaluate the projected subjects (key shots)
        public double lumaMin = double.NaN, lumaMax = double.NaN;

        public string File => tag + ".png";
        public QaView View => new QaView(x, y, z, yaw, pitch, fov, width, height);
    }

    public enum SeqMode { Walk = 0, Static = 1, PanPitch = 2, PanYaw = 3, RealStart = 4, RealEnd = 5, Poses = 6 }

    /// <summary>One frame sequence of the plan.</summary>
    public sealed class SequenceSpec
    {
        public string name;           // directory name (seq_*, enc_*)
        public string set;            // boundaries, statics, pans, edges, encode, signs, rain, koi, traffic
        public string kind;           // analysis kind for Tools/qa/sequences.py: boundary, static, pan, edge, encode, signs, rain, koi, traffic
        public string purpose;
        public SeqMode mode;
        public int fps, width, height;
        public int frames;            // fixed frame count (RealEnd: upper bound)
        public double s0, s1;         // Walk: plan distance at frame 0 and at the last frame
        public double speed = NocturneConstants.WalkSpeed;
        public bool yawWorld;         // Walk: yaw is a world yaw, else an offset to the path heading
        public double yaw, pitch;     // Walk: yaw (offset) and pitch; Static / Pan: the base pose
        public double x, y, z, fov;   // Static / Pan pose
        public double panStepDeg;     // Pan: angle per frame
        public bool advanceAmbient = true;
        public double ambient0;
        /// <summary>Beat boundaries of 14.6 crossed inside a walk (the activation-frame check of 14.10).</summary>
        public double[] boundaryS = Array.Empty<double>();
        /// <summary>Poses: one fixed pose per frame (s, position, yaw, pitch, vFOV; the dolly set, TEXTURE_PLAN 7.5).</summary>
        public double[] poseS, poseX, poseY, poseZ, poseYaw, posePitch, poseFov;
        /// <summary>Distance (m) of the camera to the subject per frame (Poses; reported in sequence.json, NaN when not set).</summary>
        public double[] poseDist;

        public double Seconds => fps > 0 ? frames / (double)fps : 0;

        /// <summary>Analytic camera pose of frame n (Walk / Static / Pan).</summary>
        public void FramePose(int n, out double s, out double px, out double py, out double pz, out double yawDeg, out double pitchDeg, out double fovDeg)
        {
            switch (mode)
            {
                case SeqMode.Walk:
                {
                    s = Math.Min(NocturneConstants.PathLength, s0 + speed * n / fps);
                    D3 p = NocMath.CorridorToWorld(s, 0, NocturneConstants.EyeHeight);
                    px = p.x; py = p.y; pz = p.z;
                    yawDeg = yawWorld ? yaw : NocMath.Heading(s) / NocMath.Deg + yaw;
                    pitchDeg = pitch;
                    fovDeg = NocMath.Fov(s);
                    return;
                }
                case SeqMode.PanPitch:
                    s = s0; px = x; py = y; pz = z; yawDeg = yaw; pitchDeg = pitch + panStepDeg * n; fovDeg = fov;
                    return;
                case SeqMode.PanYaw:
                    s = s0; px = x; py = y; pz = z; yawDeg = yaw + panStepDeg * (n - frames / 2); pitchDeg = pitch; fovDeg = fov;
                    return;
                case SeqMode.Poses:
                {
                    int i = Math.Max(0, Math.Min(n, (poseX != null ? poseX.Length : 1) - 1));
                    s = poseS[i]; px = poseX[i]; py = poseY[i]; pz = poseZ[i]; yawDeg = poseYaw[i]; pitchDeg = posePitch[i]; fovDeg = poseFov[i];
                    return;
                }
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
        public string[] only;         // keep only tags / sequence names containing one of these
        /// <summary>-facade-sheet is on (FA's sheet is built): the dolly's 30 deg line runs to the sheet instead of face B.</summary>
        public bool facadeSheet;
    }

    public static class ShotPlan
    {
        // ------------------------------------------------------------------ resolutions (11.2, 14.10)
        public const int W1080 = 1920, H1080 = 1080, W4K = 3840, H4K = 2160;
        public const int W1610 = 1680, H1610 = 1050, W219 = 2560, H219 = 1080;

        // ------------------------------------------------------------------ timings (11.6, 11.12, 14.10, 14.7)
        public const int SeqFps = 30;                  // frame sequences at 30 fps virtual time (14.10 Sequences)
        public const int MotionFps = 60;               // koi, traffic and rain: per-frame motion limits are stated at 60 fps
        public const int EncodeFps = 60;               // encode clips at 60 fps (11.12)
        public const double EncodeSeconds = 10.0, EncodeTransitionSeconds = 30.0;
        public const double BoundaryHalfSeconds = 10.0;   // 14.10: +-10 s around each boundary (at 1.5 m/s: +-15 m)
        public const double MinWindowSeconds = 20.0;      // every boundary window at least 20 s (11.6)
        public const int StaticFrames = 120;              // 14.10: 120 static frames per beat
        public const double PanPx = 0.5;                  // 14.10 shimmer: pan 0.5 px/frame for 30 frames
        public const int PanFrames = 30;
        public const double SignsSeconds = 60.0;          // 14.10 photosensitivity: 60 s at 30 fps
        public const int RainFrames = 120;                // 14.7 H5: static camera, rho 1, 120 frames
        public const double RainS = 720.0;                // mid beat 4 (rho 1.00, 14.2.4)
        public const double KoiSeconds = 12.0;            // 30 m at 2.5 m/s = 20 s of capture-mode wave time (s / 1.5): 20 phases
        public const double TrafficSeconds = 8.0;
        public const int EdgeFrames = 60;                 // defect 22: first and last 60 frames
        public const double EndLeadIn = 3.0;              // seq_end starts 3 m before the goal trigger (2 s at 1.5 m/s)
        public const double EndTail = 0.5;                // frames kept after the 17 s ending
        public const double FreeLookYaw = 90.0;
        public const double FreeLookUpPitch = 60.0, FreeLookDownPitch = -60.0;

        // 14.10 Sequences: +-15 m (10 s at 1.5 m/s) around each beat boundary of 14.6, plus s 96-130, 380-420, 1240-1300.
        public static readonly double[,] TransitionWindows =
            { { 105, 135 }, { 385, 415 }, { 545, 575 }, { 865, 895 }, { 1065, 1095 }, { 1245, 1275 }, { 96, 130 }, { 380, 420 }, { 1240, 1300 } };

        /// <summary>The -sequence NAME values of 14.12 (each is also a capture set).</summary>
        public static readonly string[] SequenceSets = { "boundaries", "statics", "signs", "rain", "koi", "traffic", "encode" };
        /// <summary>Sets that render long enough to go through gpu-guard (Part 2.5: run-player.ps1 $heavySets).</summary>
        public static readonly string[] HeavySets =
            { "keyshots", "stills", "parity", "all", "everything", "sequences", "boundaries", "transitions", "statics", "pans", "edges", "encode", "signs", "rain", "koi", "traffic", "texpans", "dolly" };
        /// <summary>Sets that contain 3840x2160 renders (gpu-guard with 6500 MiB free, Part 2.5.1; run-player.ps1 $fourKSets).</summary>
        public static readonly string[] FourKSets = { "keyshots", "stills", "all", "everything" };

        static readonly string[] s_freeLookTags = { "look_r", "look_l", "look_back" };
        static readonly double[] s_freeLookYaw = { FreeLookYaw, -FreeLookYaw, 2 * FreeLookYaw };

        public static string BeatPrefix(int beatIndex) => "beat" + (beatIndex + 1).ToString("D2", CultureInfo.InvariantCulture) + "_" + NocturneConstants.BeatNames[beatIndex];

        public static bool IsSequenceSet(string name)
        {
            if (string.IsNullOrEmpty(name)) return false;
            return Array.IndexOf(SequenceSets, name.Trim().ToLowerInvariant()) >= 0;
        }

        // ------------------------------------------------------------------ plan

        public static CapturePlan Build(string setList, CapturePlanOptions opt = null)
        {
            if (opt == null) opt = new CapturePlanOptions();
            var plan = new CapturePlan();
            bool keys1080 = false;
            var poses = new List<StillShot>();
            bool parity = false, keys = false, beats = false, bounds = false, statics = false, pans = false, edges = false, encode = false;
            bool signs = false, rain = false, koi = false, traffic = false;
            bool texpans = false, dolly = false, texclose = false, sheet = false;
            string[] parts = (string.IsNullOrEmpty(setList) ? "all" : setList).Split(',');
            for (int i = 0; i < parts.Length; i++)
            {
                string raw = parts[i].Trim();
                string p = raw.ToLowerInvariant();
                if (p.Length == 0) continue;
                if (p.StartsWith("pose:", StringComparison.Ordinal))
                {
                    var ps = ParsePose(raw);
                    if (ps != null) { poses.Add(ps); plan.sets.Add("pose"); } else plan.unknownSets.Add(raw);
                    continue;
                }
                switch (p)
                {
                    case "keyshots": keys = true; break;
                    case "keyshots1080": keys1080 = true; break;
                    case "beats": beats = true; break;
                    case "parity": parity = true; break;
                    case "sequences": bounds = statics = pans = edges = true; break;
                    case "boundaries": case "transitions": bounds = true; break;
                    case "statics": statics = true; break;
                    case "pans": pans = true; break;
                    case "edges": edges = true; break;
                    case "encode": encode = true; break;
                    case "signs": signs = true; break;
                    case "rain": rain = true; break;
                    case "koi": koi = true; break;
                    case "traffic": traffic = true; break;
                    case "texpans": texpans = true; break;
                    case "dolly": dolly = true; break;
                    case "texclose": texclose = true; break;
                    case "facadesheet": sheet = true; break;
                    case "stills": parity = keys = beats = true; break;
                    case "all": parity = keys = beats = bounds = statics = pans = edges = true; break;
                    case "everything": parity = keys = beats = bounds = statics = pans = edges = encode = signs = rain = koi = traffic = true; break;
                    default: plan.unknownSets.Add(p); continue;
                }
                plan.sets.Add(p);
            }

            if (parity) plan.parity = ParityShot();
            if (keys) plan.stills.AddRange(KeyShotStills());
            if (keys1080 && !keys) plan.stills.AddRange(KeyShotStills().FindAll(st => st.variant == "1080"));
            plan.stills.AddRange(poses);
            if (beats) plan.stills.AddRange(BeatStills());
            if (texclose) plan.stills.AddRange(TexCloseStills());
            if (sheet)
            {
                List<StillShot> fs = FacadeSheetStills();
                if (fs == null) plan.unknownSets.Add("facadesheet (NocFacadeSheet.Poses() not found)");
                else plan.stills.AddRange(fs);
            }

            int w = opt.seqWidth, h = opt.seqHeight;
            // sequences: the start first, the ending last (it finishes the game)
            if (edges) plan.sequences.Add(StartSequence(w, h));
            if (bounds) plan.sequences.AddRange(TransitionSequences(w, h));
            if (statics) plan.sequences.AddRange(StaticSequences(w, h));
            if (pans) plan.sequences.AddRange(PanSequences(w, h));
            if (signs) plan.sequences.Add(SignsSequence(w, h));
            if (rain) plan.sequences.Add(RainSequence(w, h));
            if (koi) plan.sequences.AddRange(KoiSequences(w, h));
            if (traffic) plan.sequences.AddRange(TrafficSequences(w, h));
            if (encode) plan.sequences.AddRange(EncodeClips(opt.encode4k));
            if (texpans) plan.sequences.AddRange(TexPanSequences(w, h));
            if (dolly) plan.sequences.AddRange(DollySequences(w, h, opt.facadeSheet));
            if (edges) plan.sequences.Add(EndSequence(w, h));

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

        // ------------------------------------------------------------------ key shots (14.10)

        public static List<StillShot> KeyShotStills()
        {
            var list = new List<StillShot>(160);
            var ks = NocturneConstants.KeyShots;
            // resolution-major order: one render-target size at a time (fewer reallocations)
            int[,] res = { { W1080, H1080 }, { W4K, H4K }, { W1610, H1610 }, { W219, H219 } };
            string[] suffix = { "", "_4k", "_1610", "_219" };
            string[] variant = { "1080", "4k", "1610", "219" };
            for (int r = 0; r < suffix.Length; r++)
            {
                for (int i = 0; i < ks.Length; i++)
                {
                    KeyShot k = ks[i];
                    if (k.wide)
                    {
                        // the table FOV is the picture (_wide); the in-game FOV(s) version carries the plain tag
                        list.Add(KeyStill(k, i, k.tag + "_wide" + suffix[r], k.fov, false, res[r, 0], res[r, 1], variant[r]));
                        list.Add(KeyStill(k, i, k.tag + suffix[r], NocMath.Fov(k.s), true, res[r, 0], res[r, 1], variant[r]));
                    }
                    else list.Add(KeyStill(k, i, k.tag + suffix[r], k.fov, true, res[r, 0], res[r, 1], variant[r]));
                }
            }
            return list;
        }

        static StillShot KeyStill(KeyShot k, int index, string tag, double fov, bool gameFov, int w, int h, string variant)
        {
            return new StillShot
            {
                tag = tag, set = "keyshots", baseTag = k.tag, variant = variant, width = w, height = h,
                s = k.s, x = k.x, y = k.y, z = k.z, yaw = k.yaw, pitch = k.pitch, fov = fov,
                ambient = AmbientForS(k.s), keyShotIndex = index, gameFov = gameFov, framing = true,
                lumaMin = k.lumaMin, lumaMax = k.lumaMax,
            };
        }

        /// <summary>
        /// "pose:NAME:x:y:z:yaw:pitch:fov[:s]" (Abyss KeyShots.cs pose parser): a free still at 1920 x 1080; s is the given ninth
        /// field (FA's request: the facade sheet far from the path is shot under beat 2, s 300), else the progress of (x, z) on the
        /// path; s sets the parameters, the fog, the vFOV schedule when fov &lt;= 0 and the ambient clock. Null when the text does
        /// not parse.
        /// </summary>
        public static StillShot ParsePose(string text)
        {
            string[] f = text.Split(':');
            if (f.Length != 8 && f.Length != 9) return null;
            var inv = CultureInfo.InvariantCulture;
            double x, y, z, yaw, pitch, fov;
            if (!double.TryParse(f[2], NumberStyles.Float, inv, out x) || !double.TryParse(f[3], NumberStyles.Float, inv, out y) ||
                !double.TryParse(f[4], NumberStyles.Float, inv, out z) || !double.TryParse(f[5], NumberStyles.Float, inv, out yaw) ||
                !double.TryParse(f[6], NumberStyles.Float, inv, out pitch) || !double.TryParse(f[7], NumberStyles.Float, inv, out fov)) return null;
            string name = f[1].Trim();
            if (name.Length == 0) return null;
            double s;
            if (f.Length == 9)
            {
                if (!double.TryParse(f[8], NumberStyles.Float, inv, out s) || s < 0.0 || s > NocturneConstants.PathLength) return null;
            }
            else s = NocMath.ProjectToPath(new D3(x, y, z), 0.0);
            return new StillShot
            {
                tag = "pose_" + name, set = "pose", baseTag = "pose_" + name, variant = "1080", width = W1080, height = H1080,
                s = s, x = x, y = y, z = z, yaw = yaw, pitch = pitch, fov = fov > 0 ? fov : NocMath.Fov(s), ambient = AmbientForS(s),
            };
        }

        public static int FindKeyShot(string tag)
        {
            var ks = NocturneConstants.KeyShots;
            for (int i = 0; i < ks.Length; i++) if (ks[i].tag == tag) return i;
            return -1;
        }

        /// <summary>Deterministic scene clock for a still: the time the cinematic (2.5 m/s) reaches s.</summary>
        public static double AmbientForS(double s) => s / NocturneConstants.CinematicSpeed;

        // ------------------------------------------------------------------ beat stills (11.2, 14.10 last paragraph)

        public static List<StillShot> BeatStills()
        {
            var list = new List<StillShot>(56);
            var b = NocturneConstants.BeatBoundaries;
            for (int i = 0; i < NocturneConstants.BeatCount; i++)
            {
                string pre = BeatPrefix(i);
                double s0 = b[i], s1 = b[i + 1], mid = 0.5 * (s0 + s1);
                list.Add(PathStill(pre + "_start", s0 + 2, 0, NocturneConstants.GazeDefaultPitch));
                list.Add(PathStill(pre + "_mid", mid, 0, NocturneConstants.GazeDefaultPitch));
                list.Add(PathStill(pre + "_end", s1 - 2, 0, NocturneConstants.GazeDefaultPitch));
                for (int f = 0; f < s_freeLookTags.Length; f++) list.Add(PathStill(pre + "_" + s_freeLookTags[f], mid, s_freeLookYaw[f], 0));
                list.Add(PathStill(pre + "_look_up60", mid, 0, FreeLookUpPitch));
                if (i == 2 || i == 3) list.Add(PathStill(pre + "_look_down60", mid, 0, FreeLookDownPitch));
            }
            return list;
        }

        static StillShot PathStill(string tag, double s, double yawOffset, double pitch)
        {
            D3 p = NocMath.CorridorToWorld(s, 0, NocturneConstants.EyeHeight);
            return new StillShot
            {
                tag = tag, set = "beats", baseTag = tag, variant = "1080", width = W1080, height = H1080,
                s = s, x = p.x, y = p.y, z = p.z, yaw = QaDriveMath.HeadingDeg(s) + yawOffset, pitch = pitch, fov = NocMath.Fov(s),
                ambient = AmbientForS(s),
            };
        }

        static StillShot ParityShot()
        {
            KeyShot k = NocturneConstants.KeyShots[0];
            return new StillShot
            {
                tag = "parity", set = "parity", baseTag = k.tag, variant = "window",
                s = k.s, x = k.x, y = k.y, z = k.z, yaw = k.yaw, pitch = k.pitch, fov = k.fov, ambient = AmbientForS(k.s),
            };
        }

        // ------------------------------------------------------------------ boundaries (11.6, 14.6, 14.10 Sequences)

        public static List<SequenceSpec> TransitionSequences(int w, int h)
        {
            var list = new List<SequenceSpec>(9);
            int n = TransitionWindows.GetLength(0);
            var bnd = NocturneConstants.BeatBoundaries;
            for (int i = 0; i < n; i++)
            {
                double a = TransitionWindows[i, 0], b = TransitionWindows[i, 1];
                double v = NocturneConstants.WalkSpeed;
                if ((b - a) / v < MinWindowSeconds)
                {
                    double c = 0.5 * (a + b), half = 0.5 * MinWindowSeconds * v;
                    a = c - half; b = c + half;
                }
                string nm = i < NocturneConstants.BeatCount - 1 ? "seq_b" + (i + 1) + (i + 2) : "seq_s" + ((int)TransitionWindows[i, 0]).ToString(CultureInfo.InvariantCulture);
                var sq = WalkSeq(nm, "boundaries", "boundary", "window " + a.ToString("0", CultureInfo.InvariantCulture) + "-" + b.ToString("0", CultureInfo.InvariantCulture) + " (14.6, 14.10)", a, b, SeqFps, w, h);
                var crossed = new List<double>(2);
                for (int k = 1; k < bnd.Length - 1; k++) if (bnd[k] > a && bnd[k] < b) crossed.Add(bnd[k]);
                sq.boundaryS = crossed.ToArray();
                list.Add(sq);
            }
            return list;
        }

        public static SequenceSpec WalkSeq(string name, string set, string kind, string purpose, double s0, double s1, int fps, int w, int h,
                                           double yaw = 0, bool yawWorld = false, double pitch = NocturneConstants.GazeDefaultPitch,
                                           double speed = NocturneConstants.WalkSpeed)
        {
            return new SequenceSpec
            {
                name = name, set = set, kind = kind, purpose = purpose, mode = SeqMode.Walk, fps = fps, width = w, height = h,
                s0 = s0, s1 = s1, speed = speed, frames = (int)Math.Round((s1 - s0) / speed * fps) + 1,
                yaw = yaw, yawWorld = yawWorld, pitch = pitch, ambient0 = AmbientForS(s0),
            };
        }

        // ------------------------------------------------------------------ statics, pans (14.10)

        public static List<SequenceSpec> StaticSequences(int w, int h)
        {
            var list = new List<SequenceSpec>(7);
            var b = NocturneConstants.BeatBoundaries;
            for (int i = 0; i < NocturneConstants.BeatCount; i++)
            {
                double s = 0.5 * (b[i] + b[i + 1]);
                D3 p = NocMath.CorridorToWorld(s, 0, NocturneConstants.EyeHeight);
                list.Add(StaticSeq("seq_static_" + BeatPrefix(i), "statics", "static", "120 static frames, middle of the beat (14.10: -freeze-clock -rain 0)",
                                   s, p.x, p.y, p.z, QaDriveMath.HeadingDeg(s), NocturneConstants.GazeDefaultPitch, NocMath.Fov(s), StaticFrames, SeqFps, w, h));
            }
            return list;
        }

        static SequenceSpec StaticSeq(string name, string set, string kind, string purpose, double s, double x, double y, double z,
                                      double yaw, double pitch, double fov, int frames, int fps, int w, int h)
        {
            return new SequenceSpec
            {
                name = name, set = set, kind = kind, purpose = purpose, mode = SeqMode.Static, fps = fps, width = w, height = h, frames = frames,
                s0 = s, s1 = s, x = x, y = y, z = z, yaw = yaw, pitch = pitch, fov = fov,
                ambient0 = AmbientForS(s), advanceAmbient = true,
            };
        }

        /// <summary>14.10 shimmer: a 0.5 px/frame yaw pan for 30 frames from every mid-beat pose (run with -rain 0 -freeze-clock).</summary>
        public static List<SequenceSpec> PanSequences(int w, int h)
        {
            var list = new List<SequenceSpec>(7);
            var b = NocturneConstants.BeatBoundaries;
            for (int i = 0; i < NocturneConstants.BeatCount; i++)
            {
                double s = 0.5 * (b[i] + b[i + 1]);
                D3 p = NocMath.CorridorToWorld(s, 0, NocturneConstants.EyeHeight);
                double fov = NocMath.Fov(s);
                list.Add(new SequenceSpec
                {
                    name = "seq_pan_" + BeatPrefix(i), set = "pans", kind = "pan", purpose = "0.5 px/frame yaw pan, 30 frames (14.10 shimmer)",
                    mode = SeqMode.PanYaw, fps = SeqFps, width = w, height = h, frames = PanFrames,
                    s0 = s, s1 = s, x = p.x, y = p.y, z = p.z, yaw = QaDriveMath.HeadingDeg(s), pitch = NocturneConstants.GazeDefaultPitch, fov = fov,
                    panStepDeg = QaProjection.PanStepDeg(PanPx, fov, h), ambient0 = AmbientForS(s), advanceAmbient = false,
                });
            }
            return list;
        }

        // ------------------------------------------------------------------ signs, rain, koi, traffic (14.10, 14.7 H5-H7)

        static SequenceSpec KeyStatic(string name, string set, string kind, string purpose, string keyTag, int frames, int fps, int w, int h)
        {
            KeyShot k = NocturneConstants.KeyShots[Math.Max(0, FindKeyShot(keyTag))];
            return StaticSeq(name, set, kind, purpose, k.s, k.x, k.y, k.z, k.yaw, k.pitch, k.fov, frames, fps, w, h);
        }

        public static SequenceSpec SignsSequence(int w, int h) =>
            KeyStatic("seq_signs_ks16", "signs", "signs", "KS16 pose, 60 s at 30 fps, clock running (14.10 photosensitivity)", "KS16_signs_wall",
                      (int)Math.Round(SignsSeconds * SeqFps), SeqFps, w, h);

        public static SequenceSpec RainSequence(int w, int h)
        {
            D3 p = NocMath.CorridorToWorld(RainS, 0, NocturneConstants.EyeHeight);
            return StaticSeq("seq_rain_beat04", "rain", "rain", "static camera, rho 1.00, 120 frames at 60 fps, clock running (14.7 H5)",
                             RainS, p.x, p.y, p.z, QaDriveMath.HeadingDeg(RainS), 0.0, NocMath.Fov(RainS), RainFrames, MotionFps, w, h);
        }

        static SequenceSpec KeyWalk(string name, string set, string kind, string purpose, string keyTag, double seconds, int w, int h)
        {
            KeyShot k = NocturneConstants.KeyShots[Math.Max(0, FindKeyShot(keyTag))];
            double v = NocturneConstants.CinematicSpeed, half = 0.5 * seconds * v;
            double s0 = Math.Max(0.0, k.s - half), s1 = Math.Min(NocturneConstants.GoalTriggerS - 1.0, k.s + half);
            var sq = WalkSeq(name, set, kind, purpose, s0, s1, MotionFps, w, h, k.yaw, true, k.pitch, v);
            sq.frames = (int)Math.Round((s1 - s0) / v * MotionFps);
            return sq;
        }

        public static List<SequenceSpec> KoiSequences(int w, int h)
        {
            return new List<SequenceSpec>
            {
                KeyWalk("seq_koi_ks17", "koi", "koi", "12 s at 2.5 m/s and 60 fps through KS17, its world gaze (14.7 H6)", "KS17_heart_reveal", KoiSeconds, w, h),
                KeyWalk("seq_koi_ks18", "koi", "koi", "12 s at 2.5 m/s and 60 fps through KS18, its world gaze (14.7 H6)", "KS18_koi_pass", KoiSeconds, w, h),
            };
        }

        public static List<SequenceSpec> TrafficSequences(int w, int h)
        {
            return new List<SequenceSpec>
            {
                KeyWalk("seq_traffic_ks06_tram", "traffic", "traffic", "8 s at 2.5 m/s and 60 fps through KS06 (14.7 H7)", "KS06_tram", TrafficSeconds, w, h),
                KeyWalk("seq_traffic_ks11_train", "traffic", "traffic", "8 s at 2.5 m/s and 60 fps through KS11 (14.7 H7)", "KS11_bridge_train", TrafficSeconds, w, h),
            };
        }

        // ------------------------------------------------------------------ edges (defect 22)

        public static SequenceSpec StartSequence(int w, int h)
        {
            return new SequenceSpec
            {
                name = "seq_start", set = "edges", kind = "edge", purpose = "first 60 frames: start fade (defect 22)", mode = SeqMode.RealStart,
                fps = SeqFps, width = w, height = h, frames = EdgeFrames, s0 = 0, s1 = 0, pitch = NocturneConstants.GazeDefaultPitch,
            };
        }

        public static SequenceSpec EndSequence(int w, int h)
        {
            double s0 = NocturneConstants.GoalTriggerS - EndLeadIn;
            double ending = NocturneConstants.EndTotal;   // total ending 17 s (14.11)
            int maxFrames = (int)Math.Ceiling((EndLeadIn / NocturneConstants.WalkSpeed + 2.0 + ending + EndTail) * SeqFps);
            return new SequenceSpec
            {
                name = "seq_end", set = "edges", kind = "edge", purpose = "ending: trigger, stop ramp, fade, last frames (defect 22)", mode = SeqMode.RealEnd,
                fps = SeqFps, width = w, height = h, frames = maxFrames, s0 = s0, s1 = NocturneConstants.PathLength,
                pitch = NocturneConstants.GazeDefaultPitch,
            };
        }

        /// <summary>Seconds of frames kept after the goal trigger in seq_end (the 17 s ending plus a margin).</summary>
        public static double EndSecondsAfterTrigger => NocturneConstants.EndTotal + EndTail;

        // ------------------------------------------------------------------ encode clips (11.12, 14.7 H1 (6))

        public static List<SequenceSpec> EncodeClips(bool also4k)
        {
            var list = new List<SequenceSpec>(24);
            // 14.7 H1 (6): the sky of KS04, the abyss of KS10, the wall of KS16, the plaza of KS17; plus the darkest and the
            // brightest gradient of every beat (the banding regions of the 14.10 per-beat table)
            AddEncode(list, also4k, "enc_beat01_slit", "slit cloud above the alley (KS02 view)", "KS02_alley_slit", EncodeSeconds);
            AddEncode(list, also4k, "enc_beat02_axis", "sky above the facades, canyon axis (KS04 view, H1 gate)", "KS04_boulevard_axis", EncodeSeconds);
            AddEncode(list, also4k, "enc_beat02_sky", "cloud ceiling (KS05 view)", "KS05_boulevard_up", EncodeSeconds);
            AddEncode(list, also4k, "enc_beat03_abyss", "dark haze of the abyss (KS10 view, H1 gate)", "KS10_edge_plunge", EncodeSeconds);
            AddEncode(list, also4k, "enc_beat04_void", "abyss below the bridge (KS12 view)", "KS12_bridge_void", EncodeSeconds);
            AddEncode(list, also4k, "enc_beat05_crown", "crown glow (KS15 view)", "KS15_garden_canopy", EncodeSeconds);
            AddEncode(list, also4k, "enc_beat06_wall", "wall panel gradient (KS16 view, H1 gate)", "KS16_signs_wall", EncodeSeconds);
            AddEncode(list, also4k, "enc_beat07_plaza", "sky between koi and spire, plaza (KS17 view, H1 gate)", "KS17_heart_reveal", EncodeSeconds);
            // LOOK_V2 7 (PA's request): the CRF-23 test also on the textured walls of KS00 and KS19. KS19's clip is static at its pose
            // (clock running: rain, koi): a 15 m walk at KS19's gaze scales the tenement wall 3 x (23.8 m -> 7.8 m away), so no fixed
            // banding rect stays on the wall (Tools/qa/regions_default.json seq_enc_beat07_goal = KS19's tenement_wall rect)
            AddEncodeStatic(list, also4k, "enc_beat07_goal", "tenement wall beside the goal window, static at the KS19 pose (LOOK_V2 7 H1)", "KS19_goal_window", EncodeSeconds);
            AddEncode(list, also4k, "enc_beat01_wall", "alley walls in the bulkhead pools (KS00 view, LOOK_V2 7 H1)", "KS00_alley_start", EncodeSeconds);
            // 30 s through the alley mouth (fog 5e-3 -> 4.4e-4, LOOK_V2 3), the heaviest gradient transition
            var a = WalkSeq("enc_trans_mouth", "encode", "encode", "30 s through the alley mouth (fog 5e-3 -> 4.4e-4)", 90.0, 90.0 + EncodeTransitionSeconds * NocturneConstants.WalkSpeed,
                            EncodeFps, W1080, H1080, 0, false, 14.0);
            a.frames = (int)Math.Round(EncodeTransitionSeconds * EncodeFps);
            list.Add(a);
            if (also4k)
            {
                var b = WalkSeq("enc_trans_mouth_4k", "encode", "encode", a.purpose + " (4K)", a.s0, a.s1, EncodeFps, W4K, H4K, 0, false, 14.0);
                b.frames = a.frames;
                list.Add(b);
            }
            return list;
        }

        /// <summary>A static clip at 60 fps at the key shot's pose, the clock running (rain, koi, signs move; the camera does not).</summary>
        static void AddEncodeStatic(List<SequenceSpec> list, bool also4k, string name, string purpose, string keyTag, double seconds)
        {
            KeyShot k = NocturneConstants.KeyShots[Math.Max(0, FindKeyShot(keyTag))];
            int n = (int)Math.Round(seconds * EncodeFps);
            list.Add(StaticSeq(name, "encode", "encode", purpose, k.s, k.x, k.y, k.z, k.yaw, k.pitch, k.fov, n, EncodeFps, W1080, H1080));
            if (also4k) list.Add(StaticSeq(name + "_4k", "encode", "encode", purpose + " (4K)", k.s, k.x, k.y, k.z, k.yaw, k.pitch, k.fov, n, EncodeFps, W4K, H4K));
        }

        /// <summary>A 10 s clip at 60 fps walking at 1.5 m/s and ending at the key shot's s, with its world gaze.</summary>
        static void AddEncode(List<SequenceSpec> list, bool also4k, string name, string purpose, string keyTag, double seconds)
        {
            int i = FindKeyShot(keyTag);
            KeyShot k = NocturneConstants.KeyShots[Math.Max(0, i)];
            double v = NocturneConstants.WalkSpeed;
            double s1 = Math.Min(k.s, NocturneConstants.GoalTriggerS - 1.0), s0 = Math.Max(0.0, s1 - seconds * v);
            var a = WalkSeq(name, "encode", "encode", purpose, s0, s1, EncodeFps, W1080, H1080, k.yaw, true, k.pitch);
            a.frames = (int)Math.Round(seconds * EncodeFps);
            list.Add(a);
            if (also4k)
            {
                var b = WalkSeq(name + "_4k", "encode", "encode", purpose + " (4K)", s0, s1, EncodeFps, W4K, H4K, k.yaw, true, k.pitch);
                b.frames = a.frames;
                list.Add(b);
            }
        }

        // ------------------------------------------------------------------ Look v2 texture sets (TEXTURE_PLAN 7.5, TQ)

        /// <summary>texpans: the key shots of TEXTURE_PLAN 7.5 / R1 (shimmer and swimming of the textured surfaces).</summary>
        public static readonly string[] TexPanShots =
            { "KS00_alley_start", "KS04_boulevard_axis", "KS05_boulevard_up", "KS08_edge_flank", "KS09_edge_gulf", "KS15_garden_canopy", "KS17_heart_reveal" };

        /// <summary>
        /// texpans: a 0.5 px/frame yaw pan of 30 frames centred on each key-shot pose (its world yaw, pitch and table vFOV), the
        /// ambient clock held at the key shot's (run with -freeze-clock -rain 0). The step is 0.5 px of the OUTPUT height, so a
        /// -capture-ss 2 run (rendered at twice the size, box-downsampled) pans by the same picture distance: that run is the R1
        /// reference (temporal std at most 2.0 levels above it, Tools/qa/sequences.py --reference).
        /// </summary>
        public static List<SequenceSpec> TexPanSequences(int w, int h)
        {
            var list = new List<SequenceSpec>(TexPanShots.Length);
            for (int i = 0; i < TexPanShots.Length; i++)
            {
                int ki = FindKeyShot(TexPanShots[i]);
                if (ki < 0) continue;
                KeyShot k = NocturneConstants.KeyShots[ki];
                list.Add(new SequenceSpec
                {
                    name = "seq_texpan_" + k.tag.Substring(0, 4).ToLowerInvariant(), set = "texpans", kind = "texpan",
                    purpose = "0.5 px/frame yaw pan, 30 frames, " + k.tag + " pose (TEXTURE_PLAN 7.5 / R1: shimmer, swimming)",
                    mode = SeqMode.PanYaw, fps = SeqFps, width = w, height = h, frames = PanFrames,
                    s0 = k.s, s1 = k.s, x = k.x, y = k.y, z = k.z, yaw = k.yaw, pitch = k.pitch, fov = k.fov,
                    panStepDeg = QaProjection.PanStepDeg(PanPx, k.fov, h), ambient0 = AmbientForS(k.s), advanceAmbient = false,
                });
            }
            return list;
        }

        // dolly 1 (TEXTURE_PLAN 7.5): until FA's -facade-sheet exists, the subject is face B (EdgeDims.XB) where it is planar: at
        // |z - 195| < 300 the flare F is 0, so x = 815.3 with the normal exactly -x. From the point (815.3, 150, -100) the camera
        // backs off along the line at 30 deg to the normal, toward -x and -z: at 800 m it is at (122.5, 150, -500), 27 m in front of
        // face A there (x_A(-500) = 215.3 - F(695) = 95.3); the line crosses only the open rift (no cross bridge between z -500 and
        // -100; the nearest, z -640, is at y 90; the traffic layers are at y -120 and -520). A boulevard slab face cannot be seen like
        // this: the canyon is 150 m wide and the opposite slabs reach y 560.
        public const double DollyFaceX = 815.3, DollyFaceY = 150.0, DollyFaceZ = -100.0, DollyAngleDeg = 30.0;
        public const double DollyFar = 800.0, DollyNear = 25.0, DollyS = 480.0, DollyFov = 66.0;
        public const int DollyFrames = 300;
        // dolly 2: the grazing pass along face B over the bridge deck. yaw 160 is 70 deg off face B's normal (+x is yaw 90); the eye
        // stands 12.3 m above the deck centre line (y 14), so the glass tubes (tops y 6.5 at z 183 / 207: -30 deg from there) and the
        // 4.2 m lamps stay under the frame (pitch +4, vFOV 66: bottom row -29 deg); x 390-640 keeps gate A (x 372-384) behind and
        // gate B's lintel (x 682.7-689, y 14-16) out of the left edge. s = x + 184.70 (the walk's s at that x: beat 4).
        public const double GrazeX0 = 390.0, GrazeX1 = 640.0, GrazeStep = 1.0, GrazeY = 14.0, GrazeZ = 195.0, GrazeYaw = 160.0, GrazePitch = 4.0;

        // dolly 1 on FA's facade sheet (-facade-sheet, TEXTURE_PLAN 7.2; NocFacadeSheet, read by reflection with these mirrored
        // values as the fallback): the front face (z = RowZ - Half, normal -z) of subject 0's frontal tower (x = X0, y 20..220), aimed
        // at its centre at the sheet's eye height; the camera backs off toward +x and -z. At 800 m it stands at (17, 120, 4 892): face A
        // is at full flare there (x -485), the cross bridges end at z 2 300, and the other towers stand at z >= 5 585, so no ray to
        // the target face crosses them. s 300 (beat 2's parameters, as the sheet's own stills, FA's request).
        public const double SheetX0 = -383.0, SheetRowZ = 5600.0, SheetHalf = 15.0, SheetEyeY = 120.0, SheetS = 300.0;

        static Type FacadeSheetType() => typeof(ShotPlan).Assembly.GetType("Megastructure.NocFacadeSheet", false);

        /// <summary>A public constant of FA's NocFacadeSheet (reflection), else the mirrored fallback.</summary>
        public static double SheetConst(string name, double fallback)
        {
            try
            {
                var fi = FacadeSheetType()?.GetField(name, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                if (fi != null) return Convert.ToDouble(fi.IsLiteral ? fi.GetRawConstantValue() : fi.GetValue(null), CultureInfo.InvariantCulture);
            }
            catch (Exception) { }
            return fallback;
        }

        /// <summary>facadesheet: the stills of NocFacadeSheet.Poses() through ParsePose (tags pose_NAME, set facadesheet); null when
        /// the sheet builder is not in this build.</summary>
        public static List<StillShot> FacadeSheetStills()
        {
            System.Collections.IEnumerable lines = null;
            try
            {
                var mi = FacadeSheetType()?.GetMethod("Poses", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static, null, Type.EmptyTypes, null);
                lines = mi?.Invoke(null, null) as System.Collections.IEnumerable;
            }
            catch (Exception) { lines = null; }
            if (lines == null) return null;
            var list = new List<StillShot>(32);
            foreach (object o in lines)
            {
                StillShot st = o is string line ? ParsePose(line) : null;
                if (st == null) continue;
                st.set = "facadesheet";
                list.Add(st);
            }
            return list;
        }

        /// <summary>dolly: seq_dolly_face30 (800 m -> 25 m at 30 deg to face B's normal, geometric steps of 1.16 %; seq_dolly_sheet30
        /// to FA's sheet instead with -facade-sheet) and seq_dolly_graze (1 m steps along the bridge). Fixed poses (SeqMode.Poses),
        /// the ambient clock held (run with -freeze-clock -rain 0).</summary>
        public static List<SequenceSpec> DollySequences(int w, int h, bool facadeSheet = false)
        {
            var list = new List<SequenceSpec>(2);
            // 1: the 30 deg line, distance d_n = far (near / far)^(n / (N - 1))
            int n1 = DollyFrames;
            double ca = Math.Cos(DollyAngleDeg * NocMath.Deg), sa = Math.Sin(DollyAngleDeg * NocMath.Deg);
            SequenceSpec a;
            if (facadeSheet)
            {
                double tx = SheetConst("X0", SheetX0), tz = SheetConst("RowZ", SheetRowZ) - SheetConst("Half", SheetHalf), ty = SheetConst("EyeY", SheetEyeY);
                a = PoseSeq("seq_dolly_sheet30", "dolly", "dolly 800 m -> 25 m at 30 deg to the normal of the facade sheet's subject-0 tower (TEXTURE_PLAN 7.2, 7.5 / R2)", n1, w, h);
                double yawS = Math.Atan2(-sa, ca) / NocMath.Deg;   // view direction (-sin, 0, +cos): toward the face (-30 deg)
                for (int i = 0; i < n1; i++)
                {
                    double d = DollyFar * Math.Pow(DollyNear / DollyFar, i / (double)(n1 - 1));
                    SetPose(a, i, SheetS, tx + d * sa, ty, tz - d * ca, yawS, 0.0, DollyFov, d);
                }
                a.s0 = a.s1 = SheetS;
                a.ambient0 = AmbientForS(SheetS);
            }
            else
            {
                a = PoseSeq("seq_dolly_face30", "dolly", "dolly 800 m -> 25 m at 30 deg to face B's normal (planar part z -100, y 150; TEXTURE_PLAN 7.5 / R2)", n1, w, h);
                double yawA = Math.Atan2(ca, sa) / NocMath.Deg;   // view direction (+cos, 0, +sin): toward the face (60 deg)
                for (int i = 0; i < n1; i++)
                {
                    double d = DollyFar * Math.Pow(DollyNear / DollyFar, i / (double)(n1 - 1));
                    SetPose(a, i, DollyS, DollyFaceX - d * ca, DollyFaceY, DollyFaceZ - d * sa, yawA, 0.0, DollyFov, d);
                }
                a.s0 = a.s1 = DollyS;
                a.ambient0 = AmbientForS(DollyS);
            }
            list.Add(a);
            // 2: the grazing pass
            int n2 = (int)Math.Round((GrazeX1 - GrazeX0) / GrazeStep) + 1;
            var b = PoseSeq("seq_dolly_graze", "dolly", "grazing pass along face B over the bridge deck, x 390-640, yaw 160 (70 deg off the normal), y 14 (TEXTURE_PLAN 7.5 / R1, R2)", n2, w, h);
            for (int i = 0; i < n2; i++)
            {
                double x = GrazeX0 + GrazeStep * i;
                double s = x + NocturneConstants.EastLegOffset;
                SetPose(b, i, s, x, GrazeY, GrazeZ, GrazeYaw, GrazePitch, DollyFov, (DollyFaceX - x) / Math.Sin(GrazeYaw * NocMath.Deg));
            }
            b.s0 = b.poseS[0];
            b.s1 = b.poseS[n2 - 1];
            b.ambient0 = AmbientForS(b.s0);
            list.Add(b);
            return list;
        }

        static SequenceSpec PoseSeq(string name, string kind, string purpose, int frames, int w, int h)
        {
            return new SequenceSpec
            {
                name = name, set = "dolly", kind = kind, purpose = purpose, mode = SeqMode.Poses, fps = SeqFps, width = w, height = h,
                frames = frames, advanceAmbient = false,
                poseS = new double[frames], poseX = new double[frames], poseY = new double[frames], poseZ = new double[frames],
                poseYaw = new double[frames], posePitch = new double[frames], poseFov = new double[frames], poseDist = new double[frames],
            };
        }

        static void SetPose(SequenceSpec q, int i, double s, double x, double y, double z, double yaw, double pitch, double fov, double dist)
        {
            q.poseS[i] = s; q.poseX[i] = x; q.poseY[i] = y; q.poseZ[i] = z;
            q.poseYaw[i] = yaw; q.posePitch[i] = pitch; q.poseFov[i] = fov; q.poseDist[i] = dist;
        }

        /// <summary>One near pose of texclose: the camera 1.5 m from the named surface (QaTests checks each against the beat layout).</summary>
        public readonly struct TexClosePose
        {
            public readonly string name, surface;
            public readonly double s, x, y, z, yaw, pitch, dist;
            public TexClosePose(string name, string surface, double s, double x, double y, double z, double yaw, double pitch, double dist)
            { this.name = name; this.surface = surface; this.s = s; this.x = x; this.y = y; this.z = z; this.yaw = yaw; this.pitch = pitch; this.dist = dist; }
        }

        /// <summary>texclose vFOV: 62 deg (the alley's), a 1.80 x 3.21 m patch at 1.5 m; 1.67 mm per pixel at 1080p.</summary>
        public const double TexCloseFov = 62.0, TexCloseDist = 1.5;

        /// <summary>
        /// texclose (TEXTURE_PLAN 7.5, R4 seams, R20 interiors): the distance is along the centre ray to the surface it frames.
        /// Floors are seen at pitch -50 from 1.15 m (1.15 / sin 50 = 1.50 m; the 5 m deck at -65 from 1.36 m), so the frame holds
        /// nothing but the surface.
        /// </summary>
        public static readonly TexClosePose[] TexClosePoses =
        {
            // east alley wall x 2.2 (lane 1.85 + 0.35 at s 0-26), between the east doors at z 16 and 38.5, under the blade signs (y 3.8)
            new TexClosePose("tc_alley_wall", "alley wall x 2.2 (G1 render / brick)", 22.0, 0.70, 1.60, 22.0, 90.0, 0.0, 1.50),
            // the south podium street face z 120 west of the arcade (x < -42, it runs down to the floor there: the stone base band)
            new TexClosePose("tc_podium_panel", "podium face z 120, stone base band (FA override 4)", 121.5, -50.0, 1.60, 121.5, 180.0, 0.0, 1.50),
            // Heart plaza floor inside the 12 m lane, 10.5 m from the lamp at (1070, 371.88)
            new TexClosePose("tc_plaza", "Heart plaza floor y 0", 1417.82, 1066.0, 1.15, 382.0, 90.0, -50.0, 1.50),
            // bridge deck top y 0 on the centre line, lamp 16 (x 622.3) 10 m ahead; KS13's glass strip (x 521-541) is far behind; pitch
            // -65 from 1.36 m keeps the frame's top corners 0.27 m inside the deck edges (TQ_r01 at -50 from z 195.6 saw past the edge)
            new TexClosePose("tc_bridge_deck", "bridge deck y 0 (steel)", 796.7, 612.0, 1.36, 195.0, 90.0, -65.0, 1.50),
            // the tenement's west face x 1156 in the solid band y 2.3-4.4 between the ground-floor windows and the first-floor sills
            new TexClosePose("tc_tenement_brick", "tenement west face x 1156 (StoneWall)", 1500.0, 1154.5, 3.35, 383.4, 90.0, 0.0, 1.50),
            // the noodle bar's counter front (x -3.03, y 0-1.0) under the window y 1.0-2.3
            new TexClosePose("tc_noodle_bar", "noodle bar counter and window (x -3.0)", 45.0, -1.55, 1.30, 45.0, 270.0, -15.0, 1.53),
            // tree T7 (800, 160): r(14) = 5.01 m (r0 5.589, taper 0.884, flare 1.014; buttress lobes <= 0.17 m), seen from the path side
            new TexClosePose("tc_trunk", "garden trunk T7 at y 14 (bark)", 984.7, 800.0, 14.0, 166.51, 180.0, 0.0, 1.50),
            // the south train tube (axis z 183, y 2, r 4.5): its north face z 187.5, from the gap between the tube and the deck
            new TexClosePose("tc_glass_tube", "south train tube glass z 187.5 (NocGlass)", 796.7, 612.0, 2.0, 189.0, 180.0, 0.0, 1.50),
        };

        public static List<StillShot> TexCloseStills()
        {
            var list = new List<StillShot>(TexClosePoses.Length);
            for (int i = 0; i < TexClosePoses.Length; i++)
            {
                TexClosePose p = TexClosePoses[i];
                list.Add(new StillShot
                {
                    tag = p.name, set = "texclose", baseTag = p.name, variant = "1080", width = W1080, height = H1080,
                    s = p.s, x = p.x, y = p.y, z = p.z, yaw = p.yaw, pitch = p.pitch, fov = TexCloseFov, ambient = AmbientForS(p.s),
                    gameFov = false,
                });
            }
            return list;
        }

        // ------------------------------------------------------------------ run-player routing (Part 2.5)

        /// <summary>True when a set list needs gpu-guard (a run-player heavy set; texpans and dolly are heavy sets since Phase 4
        /// wave A, so the "-sequence NAME" workaround of TQ is no longer needed).</summary>
        public static bool NeedsGuard(string setList) => IsHeavy(setList);

        /// <summary>
        /// The routing decision of Tools/run-player.ps1 for a player argument list (mirror of its rules, QaTests checks the cases):
        /// heavy for -soak, -perf-log, -autotest, -autotest-until, -wallbot, -sequence, -capture-4k, a $heavySets value of
        /// -capture-set / -sequence, -res / -perf-rt / -seq-res of 3840 or wider, -screen-width 3840 or more, -capture-ss 2 or more,
        /// and -capture without a set; 4K (6500 MiB) for -capture-4k, a $fourKSets value, those resolutions, -capture-ss 2 or more
        /// (3840 x 2160 per 1080p frame) and -capture without a set.
        /// </summary>
        public static void RunPlayerRoute(IList<string> args, out bool heavy, out bool fourK)
        {
            heavy = fourK = false;
            if (args == null) return;
            bool capture = false, set = false;
            for (int i = 0; i < args.Count; i++)
            {
                string a = args[i] ?? "";
                string v = i + 1 < args.Count && args[i + 1] != null ? args[i + 1].Trim().ToLowerInvariant() : "";
                if (a == "-soak" || a == "-perf-log" || a == "-autotest" || a == "-autotest-until" || a == "-wallbot" || a == "-sequence") heavy = true;
                if (a.IndexOf("-capture-4k", StringComparison.Ordinal) >= 0) heavy = fourK = true;
                if (a == "-capture") capture = true;
                if ((a == "-capture-set" || a == "-sequence") && i + 1 < args.Count)
                {
                    set = true;
                    if (Array.IndexOf(HeavySets, v) >= 0) heavy = true;
                    if (Array.IndexOf(FourKSets, v) >= 0) fourK = true;
                }
                if ((a == "-res" || a == "-perf-rt" || a == "-seq-res") && TryWidth(v, out int rw) && rw >= W4K) heavy = fourK = true;
                if (a == "-screen-width" && int.TryParse(v, NumberStyles.Integer, CultureInfo.InvariantCulture, out int sw) && sw >= W4K) heavy = fourK = true;
                if (a == "-capture-ss" && int.TryParse(v, NumberStyles.Integer, CultureInfo.InvariantCulture, out int ssv) && ssv >= 2) heavy = fourK = true;
            }
            if (capture && !set) heavy = fourK = true;
        }

        static bool TryWidth(string wxh, out int w)
        {
            w = 0;
            if (string.IsNullOrEmpty(wxh)) return false;
            string[] p = wxh.Split('x');
            return p.Length == 2 && int.TryParse(p[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out w)
                && int.TryParse(p[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out _);
        }

        /// <summary>
        /// Null when run-player routes the run as it needs, else the refusal note of the capture harness (player only): a heavy
        /// set without gpu-guard, or -capture-ss 2 (a 3840 x 2160 render per 1080p frame) without the 4K turn (6500 MiB). With the
        /// routing rules above both hold for every run-player call; the note catches a player started some other way.
        /// </summary>
        public static string GuardNote(IList<string> args, string setList, int supersample)
        {
            RunPlayerRoute(args, out bool heavy, out bool fourK);
            bool needHeavy = NeedsGuard(setList ?? "") || supersample > 1;
            if (supersample > 1 && !fourK)
                return "-capture-ss " + supersample + " renders " + supersample + "x per axis (3840 x 2160 for 1080p): it must run through run-player, which routes it through gpu-guard as a 4K job (6500 MiB)";
            if (needHeavy && !heavy)
                return "set '" + setList + "' needs gpu-guard: run it through run-player, which routes it through gpu-guard";
            return null;
        }

        /// <summary>True when a capture-set list contains a set that run-player.ps1 must route through gpu-guard.</summary>
        public static bool IsHeavy(string setList)
        {
            if (string.IsNullOrEmpty(setList)) return true;   // the default "all"
            foreach (string part in setList.Split(','))
            {
                string p = part.Trim().ToLowerInvariant();
                if (Array.IndexOf(HeavySets, p) >= 0) return true;
            }
            return false;
        }

        /// <summary>True when a capture-set list renders at 3840x2160 (6500 MiB of free video memory, Part 2.5.1).</summary>
        public static bool Uses4K(string setList, bool encode4k)
        {
            if (string.IsNullOrEmpty(setList)) return true;
            foreach (string part in setList.Split(','))
            {
                string p = part.Trim().ToLowerInvariant();
                if (Array.IndexOf(FourKSets, p) >= 0) return true;
                if (encode4k && p == "encode") return true;
            }
            return false;
        }
    }

    /// <summary>Pure heading helper (degrees) shared by the plan and the bots.</summary>
    public static class QaDriveMath
    {
        public static double HeadingDeg(double s) => NocMath.Heading(s) / NocMath.Deg;
    }
}
