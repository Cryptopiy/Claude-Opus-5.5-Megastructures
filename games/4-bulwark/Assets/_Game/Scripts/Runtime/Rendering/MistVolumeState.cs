// MistVolumeState: the per-frame inputs of the mist pass (MASTER_PROMPT 14.7 H2), written as _BwMist* shader globals once
// per frame (and before every capture render: ProgressDirector.Apply runs the updaters, the beat 3 builder calls Tick).
// Emitters (at most 8 at a time): the plume and its veil, the curtain (14 sheets beyond 90 m), up to 6 wall sprays read
// from WaveImpacts (SeaState is the only writer), and the beat 7 conduit spray; the sprays are live only while the eye is
// on the crest (s below 980) and the conduit only from s 1340, so 1 + 1 + 6 is the maximum at any time.
// Times are on the ambient clock (BwClock, frozen in captures): the fbm and streak offsets are computed in double and
// wrapped on whole noise tiles (Part 10 #33). Zero allocations per frame (fixed arrays, cached ids).
// Command line (look-dev and the acceptance renders of 14.5.3 / 14.10.5): -mist off (no mist pass), -mist-bow off (K = 0
// and no dark band: the "bow term off" render), -mist-sun off (the mist's sun term 0: "bow absent with the sun off"),
// -mist-msaa off (force the logged single-sample fallback).
using System;
using UnityEngine;

namespace Megastructure.Rendering
{
    public static class MistVolumeState
    {
        public const int SpraySlots = 7;          // 6 wall sprays + the conduit
        public const int ConduitSlot = 6;
        /// <summary>Wall sprays are drawn while the eye is on the crest (beats 1-4); the bus is cleared at the tower.</summary>
        public const double SprayEyeSMax = 980.0;
        /// <summary>The conduit spray exists from the end of the window gallery (its tunnel opens at s 1370).</summary>
        public const double ConduitEyeSMin = 1340.0;
        /// <summary>Conduit (14.5.7): behind the tunnel's east grating panel (s 1380-1423, 43 m), 8 m wide, 6 m high.</summary>
        public const double ConduitS0 = BulwarkConst.ConduitBellS0, ConduitS1 = BulwarkConst.ConduitBellS1;
        public const double ConduitPanelOffset = 1.6 + 0.3;   // tunnel half width 1.6 + the panel and its frame (m)
        /// <summary>Optical depth of the conduit spray across its 8 m width (a hint of mist behind the bars, never a wall).</summary>
        public const double ConduitTau = 0.35;

        public static readonly int IdMist0 = Shader.PropertyToID("_BwMist0");
        public static readonly int IdMist1 = Shader.PropertyToID("_BwMist1");
        public static readonly int IdGold = Shader.PropertyToID("_BwMistGold");
        public static readonly int IdWind = Shader.PropertyToID("_BwMistWind");
        public static readonly int IdSprayC = Shader.PropertyToID("_BwMistSprayC");
        public static readonly int IdSprayR = Shader.PropertyToID("_BwMistSprayR");
        public static readonly int IdSprayL = Shader.PropertyToID("_BwMistSprayL");
        public static readonly int IdNoise = Shader.PropertyToID("_BwMistNoise");
        public static readonly int IdScreen = Shader.PropertyToID("_BwMistScreen");
        public static readonly int IdDepthMS = Shader.PropertyToID("_BwMistDepthMS");
        public static readonly int IdDepth = Shader.PropertyToID("_BwMistDepth");
        public static readonly int IdNearest3x3 = Shader.PropertyToID("_BwMistNearest3x3");
        public static readonly int IdAmb = Shader.PropertyToID("_BwMistAmb");
        public static readonly int IdWarp = Shader.PropertyToID("_BwMistWarp");
        /// <summary>The lower hemisphere seen by the mist is the lowland 6 km down through the haze, darker than the haze colour
        /// at the horizon: the fog colour x 0.35 (round 6; was 0.7). Measured on the mist-off renders of wb-mist-05 (display
        /// linear luminance): KS08 lowland rows 620-760 0.093-0.100 against the horizon rows 500-540 0.231 (0.40-0.43), low520
        /// lowland 0.060-0.069 against 0.223 (0.27-0.31): mean 0.35. With 0.7 a thick plume under the storm sky had the
        /// radiance of the horizon behind it and vanished (KS06 plume +2 levels above the horizon).</summary>
        public const double LowerHemisphereFactor = 0.35;
        public static readonly int IdBox0 = Shader.PropertyToID("_BwMistBox0");
        public static readonly int IdBox1 = Shader.PropertyToID("_BwMistBox1");
        public static readonly int IdBox2 = Shader.PropertyToID("_BwMistBox2");
        public static readonly int IdBox3 = Shader.PropertyToID("_BwMistBox3");
        public static readonly int IdBoxL = Shader.PropertyToID("_BwMistBoxL");
        public static readonly int IdBoxF = Shader.PropertyToID("_BwMistBoxF");
        /// <summary>In-scatter of the conduit spray per unit of the cold-light radiance (14.5.7 "cold wall lights"): the lamps
        /// are 1.2 m bars every 6 m on the far wall, so the mist sees them over a small solid angle.</summary>
        public const float ConduitLightScatter = 0.06f;
        /// <summary>Air drift of the conduit spray along the flow (m/s; the water runs at 14).</summary>
        public const double ConduitAirDrift = 4.0;

        static readonly Vector4[] s_c = new Vector4[SpraySlots];
        static readonly Vector4[] s_r = new Vector4[SpraySlots];
        static readonly Vector4[] s_l = new Vector4[SpraySlots];
        static bool s_cliRead;
        static bool s_enabled = true, s_bow = true, s_sun = true, s_msaa = true;

        /// <summary>The pass is drawn (-mist off disables it).</summary>
        public static bool Enabled { get { ReadCli(); return s_enabled; } }
        /// <summary>-mist-msaa off forces the single-sample fallback.</summary>
        public static bool AllowMsaa { get { ReadCli(); return s_msaa; } }
        public static bool BowOn { get { ReadCli(); return s_bow; } }
        public static bool SunOn { get { ReadCli(); return s_sun; } }

        /// <summary>Spray slots in use this frame (diagnostics, tests).</summary>
        public static int ActiveSprays { get; private set; }
        /// <summary>The beat 7 conduit spray box is drawn this frame (diagnostics, tests).</summary>
        public static bool ConduitActive { get; private set; }
        /// <summary>Last s and ambient time written (diagnostics).</summary>
        public static double LastS { get; private set; } = double.NaN;
        public static double LastTime { get; private set; } = double.NaN;

        static void ReadCli()
        {
            if (s_cliRead) return;
            s_cliRead = true;
            s_enabled = !IsOff("-mist");
            s_bow = !IsOff("-mist-bow");
            s_sun = !IsOff("-mist-sun");
            s_msaa = !IsOff("-mist-msaa");
            if (!s_enabled || !s_bow || !s_sun || !s_msaa)
                Debug.Log($"[BW] MistVolume: command line mist={s_enabled} bow={s_bow} sun={s_sun} msaa={s_msaa}");
        }

        static bool IsOff(string flag) => CliArgs.Has(flag) && string.Equals(CliArgs.Get(flag, "on"), "off", StringComparison.OrdinalIgnoreCase);

        /// <summary>Tests: re-read the command line (CliArgs.Override) on next use.</summary>
        public static void ResetCli() { s_cliRead = false; }

        /// <summary>Writes every _BwMist* global for progress s at ambient time t (call once per frame and per capture); the
        /// spray slots are assembled for the last known eye (the pass re-assembles them per camera, CommitSprays).</summary>
        public static void Tick(double s, double t) => Tick(s, t, s_eye);

        /// <summary>Tick with the camera position of this frame (FrameState.cameraPos).</summary>
        public static void Tick(double s, double t, Vector3 eye)
        {
            ReadCli();
            LastS = s; LastTime = t;
            s_eye = eye;
            double veil = MistVolumeMath.VeilWeight(s) * MistVolumeMath.VeilRho;
            Shader.SetGlobalVector(IdMist0, new Vector4((float)MistVolumeMath.PlumeRho0, (float)veil,
                s_bow ? (float)MistVolumeMath.BowK : 0f, s_sun ? 1f : 0f));
            // the sun-break gold (fix wave X2 S3-1): x = weight of the window s 560-640
            Shader.SetGlobalVector(IdGold, new Vector4((float)MistVolumeMath.GoldWeight(s), 0f, 0f, 0f));

            // fbm offsets in noise tiles (tile = 240 m = 4 cells of 60 m), wrapped on whole tiles in double
            double tile = MistVolumeMath.FbmScale * MistVolumeNoise.BaseCells;
            var wind = new Vector4((float)Wrap(MistVolumeMath.WindX * t / tile), (float)Wrap(MistVolumeMath.WindY * t / tile),
                                   (float)Wrap(MistVolumeMath.WindZ * t / tile), 0f);
            // spray noise: spray-local 16 m tiles (the texture rides with the spray), evolving at 0.5 m/s (finest 1 m cells:
            // 0.5 Hz at most, 14.2.5)
            wind.w = (float)Wrap(0.5 * t / 16.0);
            Shader.SetGlobalVector(IdWind, wind);
            // the breathing warp (large lobes): the same wind on 1.6 km tiles, wrapped on its own tile
            const double warpTile = 1600.0;
            Shader.SetGlobalVector(IdWarp, new Vector4((float)Wrap(MistVolumeMath.WindX * t / warpTile), (float)Wrap(MistVolumeMath.WindY * t / warpTile),
                                                       (float)Wrap(MistVolumeMath.WindZ * t / warpTile), 0f));

            // the beat 7 conduit spray: the box the beat 7 owner publishes (Megastructure.Beats.ConduitSpray); while the mist
            // draws it, BwConduit's own in-shader veil is off (VeilStrength 0), so the spray is never drawn twice
            bool conduit = s >= ConduitEyeSMin && Megastructure.Beats.ConduitSpray.Active;
            ConduitActive = conduit;
            if (conduit)
            {
                var o = Megastructure.Beats.ConduitSpray.Origin;
                var au = Megastructure.Beats.ConduitSpray.AxisU;
                var av = Megastructure.Beats.ConduitSpray.AxisV;
                var lmin = Megastructure.Beats.ConduitSpray.LocalMin;
                var lmax = Megastructure.Beats.ConduitSpray.LocalMax;
                Color light = Megastructure.Beats.ConduitSpray.LightColor * ConduitLightScatter;
                Shader.SetGlobalVector(IdBox0, new Vector4(o.x, o.y, o.z, Megastructure.Beats.ConduitSpray.Sigma0));
                Shader.SetGlobalVector(IdBox1, new Vector4(au.x, au.z, av.x, av.z));
                Shader.SetGlobalVector(IdBox2, new Vector4(lmin.x, lmin.y, lmin.z, Megastructure.Beats.ConduitSpray.ScaleHeight));
                Shader.SetGlobalVector(IdBox3, new Vector4(lmax.x, lmax.y, lmax.z, Megastructure.Beats.ConduitSpray.ShaftU));
                Shader.SetGlobalVector(IdBoxL, new Vector4(light.r, light.g, light.b, s_enabled ? 1f : 0f));
                Shader.SetGlobalVector(IdBoxF, new Vector4((float)Wrap(ConduitAirDrift * t / 64.0), 0f, 0f, 0f));
                Megastructure.Beats.ConduitSpray.VeilStrength = s_enabled ? 0f : 1f;
            }
            else
            {
                Shader.SetGlobalVector(IdBoxL, Vector4.zero);
                if (Megastructure.Beats.ConduitSpray.Active) Megastructure.Beats.ConduitSpray.VeilStrength = 1f;
            }
            Shader.SetGlobalVector(IdAmb, DomeAmbient());

            // curtain streak offset: departure time x 8.3 m/s in 40 m cells, x 0.25 tiles per cell (the shader's nc * 0.25)
            s_flow = Wrap(t * MistVolumeMath.LipSpeed / 40.0 * 0.25) / 0.25;
            var noise = MistVolumeNoise.Get();
            if (noise != null) Shader.SetGlobalTexture(IdNoise, noise);
            CommitSprays(eye);
        }

        // ================================================================== spray slots (wall sprays + sea disturbances)
        // Candidates: the live wall sprays of WaveImpacts (SeaState, the only writer) and the ellipsoids of the sea-disturbance
        // trackers (the kraken, SeaDisturbances); the best SprayBudget (6) by MistVolumeMath.SprayScore are drawn, the ones
        // near the cut-off faded (CutoffWeight), so the 8-emitter budget of H2 holds (plume + curtain + 6) without popping.
        // Called by Tick (with the frame's eye) and by MistVolumePass for every camera it records (that camera's eye, after
        // every updater of the frame ran, so the kraken's Publish of this frame is read even if its updater runs after beat
        // 3's). The followers advance only when the ambient clock moved (several cameras per frame advance them once).

        struct Candidate
        {
            public Vector4 C, R, L, S, N;   // the shader arrays of one slot (density in C.w before the cut-off weight)
            public float W;                 // white-water share of the slot (_BwMistSprayW.x)
            public double score;
        }

        /// <summary>One sea disturbance being followed (also after it left the bus: its spray falls).</summary>
        struct Tracker
        {
            public bool used, seen;
            public SeaDisturbanceKind kind;
            public double x, z, halfAlong, halfAcross, heading, height, strength;   // last geometry from the bus
            public double rate;                                                       // dh/dt of the bus height (m/s; 0 when unknown)
            public double dens, densV, top, topV, veil, veilV, veilTop, veilTopV;     // followers (veil = the cascade)
        }

        public const int MaxTrackers = 12;
        /// <summary>A spray below 1 % of its full density is not drawn (optical depth at most 0.025: invisible; the last
        /// step is 1 % of the peak, far below the 5 % per frame rule) and its tracker ends once it is falling.</summary>
        public const double DrawFloor = 0.01;
        /// <summary>Largest clock step (s) of a running capture sequence (30 fps: 0.033; 10 fps look-dev: 0.1); larger steps and
        /// repeats are stills, whose followers snap to their targets.</summary>
        public const double SequenceStepMax = 0.1;
        const int MaxCandidates = WaveImpacts.Max + 2 * MaxTrackers;
        static readonly Candidate[] s_cand = new Candidate[MaxCandidates];
        static readonly int[] s_order = new int[MaxCandidates];
        static readonly Tracker[] s_trk = new Tracker[MaxTrackers];
        static readonly Vector4[] s_s = new Vector4[SpraySlots];
        static readonly Vector4[] s_n = new Vector4[SpraySlots];
        public static readonly int IdSprayS = Shader.PropertyToID("_BwMistSprayS");
        public static readonly int IdSprayN = Shader.PropertyToID("_BwMistSprayN");
        public static readonly int IdSprayW = Shader.PropertyToID("_BwMistSprayW");
        static readonly Vector4[] s_w = new Vector4[SpraySlots];
        static Vector3 s_eye = new Vector3(0f, 1.7f, 0f);
        static double s_followT = double.NaN;
        static bool s_lastSnap;
        static double s_flow;

        /// <summary>Sea-disturbance sprays among the drawn slots this frame (diagnostics, tests).</summary>
        public static int DisturbanceSprays { get; private set; }
        /// <summary>Live trackers (disturbances on the bus or still falling).</summary>
        public static int ActiveTrackers { get; private set; }
        /// <summary>Candidates that did not fit the budget this frame (diagnostics, tests).</summary>
        public static int ExcludedSprays { get; private set; }

        /// <summary>Tests: forget every tracker and the follower clock.</summary>
        public static void ResetTrackers()
        {
            for (int i = 0; i < MaxTrackers; i++) s_trk[i] = default;
            s_followT = double.NaN;
            s_lastSnap = false;
        }

        /// <summary>Assembles and uploads the spray slots for an eye at LastS / LastTime (see the section comment).</summary>
        public static void CommitSprays(Vector3 eye)
        {
            ReadCli();
            double s = LastS, t = LastTime;
            if (double.IsNaN(s) || double.IsNaN(t)) return;
            if (MistVolumeQa.Active) MistVolumeQa.Write(t);
            bool exterior = s < SprayEyeSMax;

            // followers: snap in capture stills, on the first commit and after a jump of the clock; advance once per tick
            // (Wave X: nothing sets AtmosphereDriver.SequenceMode, so a capture SEQUENCE is told from a still by its clock: it
            // steps 1/30 s per frame, a still jumps by many seconds; a repeat of the same clock (the pass commits again for
            // each camera) neither snaps nor advances. Without this every sequence frame snapped and mistx-15/16-seqk5 showed
            // the K5 column appear in one frame, which the game never does)
            double dt = t - s_followT;
            bool running = dt >= 0.0 && dt <= SequenceStepMax;
            bool still = GameState.CaptureMode && !AtmosphereDriver.SequenceMode && !running;
            // a still keeps snapping on its repeat commits at the same clock (settle frames, the pass's per-camera commits):
            // the beat 3 driver ticks before the kraken director publishes the bus in the frame order, so the first commit of
            // a still sees the PREVIOUS pose's bus; without the re-snap that stale bus stayed frozen for the whole still (fix
            // wave X2 S3-4: x2-show KS11 had no K5 column, s 828 showed the column of s 824)
            bool stillRepeat = s_lastSnap && GameState.CaptureMode && !AtmosphereDriver.SequenceMode && dt == 0.0;
            bool snap = still || stillRepeat || double.IsNaN(s_followT) || !(dt >= 0.0) || dt > 1.0;
            if (snap || dt > 0.0)
            {
                UpdateTrackers(exterior, snap ? double.PositiveInfinity : dt);
                s_followT = t;
                s_lastSnap = snap;
            }

            // candidates
            int nc = 0;
            if (exterior)
            {
                int count = WaveImpacts.Count;
                for (int i = 0; i < count && nc < MaxCandidates; i++)
                {
                    WaveImpact w = WaveImpacts.Get(i);
                    double age = t - w.T0;
                    // envelope x the young burst's core; a spray born within 10-30 m of the eye rises slower and without the
                    // dense core (Wave X: the near burst flashed half the KS11 frame white in 0.3 s, mistx-06/07-seqk5)
                    double dxe = w.X - eye.x, dze = w.Z - eye.z;
                    double near = MistVolumeMath.SprayNearness(Math.Sqrt(dxe * dxe + dze * dze));
                    double env = MistVolumeMath.SprayDensityFactorNear(age, near);
                    if (env <= 0.0) continue;
                    bool reef = w.Z >= BulwarkConst.SpillwayZ1;
                    double drift = reef ? MistVolumeMath.SprayDriftReef : MistVolumeMath.SprayDriftStorm;
                    // a wall jet (at the sea face) rises just seaward of the wall (WallJetOffset), the collision jets offshore where
                    // SeaState puts them
                    double jx = w.X <= BulwarkConst.SeaFaceX + 0.5 ? w.X + MistVolumeMath.WallJetOffset : w.X;
                    MistVolumeMath.SprayPose(jx, w.Z, w.Height, age, drift, out double cx, out double cy, out double cz, out double grow);
                    double size = 0.75 + 0.25 * MistVolumeMath.Clamp01(w.Strength);
                    double ax = MistVolumeMath.SprayAx * size * grow, ay = MistVolumeMath.SprayAy * size * grow, az = MistVolumeMath.SprayAz * size * grow;
                    double rho = MistVolumeMath.SprayRho(w.Strength) * env;
                    // thrown up from the sea: one ellipsoid from just under the water to the top of the thrown water (no floating
                    // ball, show report KS11; MistVolumeMath.WallColumn)
                    MistVolumeMath.WallColumn(cy, ay, out double cyc, out double ayc);
                    ref Candidate k = ref s_cand[nc++];
                    k.C = new Vector4((float)cx, (float)cyc, (float)cz, (float)rho);
                    k.R = new Vector4((float)ax, (float)ayc, (float)az, 0f);
                    k.L = new Vector4(0f, 0f, 0f, 1f);
                    // thrown water: the noise tile is half the 16 m evolution tile across (8 m: droplet clusters of ~3 m, which
                    // still read at 35 m: mistx-08 KS04 showed a smooth glowing pillar with 16 m) and 2.2 x taller than wide
                    // (streaks along the throw; mistx-04 KS11: with an isotropic tile the wall plume was a cumulus puff)
                    double wt = MistVolumeMath.SprayFbmScale * MistVolumeMath.WallSprayTileK;
                    // the foot stays at the wall while the wind carries the rest west: the drift D of the centre becomes a lean
                    // of the axis (-D / semi-axis per metre of height), so the base is at the jet's x and the top leans over the
                    // road (no sheet of spray standing on the road in front of the parapet)
                    double leanX = -(jx - cx) / ayc;
                    k.S = new Vector4((float)leanX, 0f, (float)(1.0 / wt), (float)(1.0 / (wt * MistVolumeMath.DistTileStreak)));
                    float wn = (float)Wrap(0.5 * t / 16.0);   // the wall sprays' own evolution (0.5 m/s on 16 m tiles)
                    k.N = new Vector4(wn, wn, wn, (float)MistVolumeMath.SprayForwardWall);
                    k.W = (float)MistVolumeMath.SprayWhiteFor(near);
                    k.score = MistVolumeMath.SprayScore(MistVolumeMath.TauForRho(rho, Math.Min(ax, az)), Math.Max(ax, Math.Max(ayc, az)), Dist(eye, cx, cyc, cz));
                }
                for (int i = 0; i < MaxTrackers && nc + 2 <= MaxCandidates; i++)
                {
                    if (!s_trk[i].used) continue;
                    AddTrackerCandidates(ref s_trk[i], eye, t, ref nc);
                }
            }

            // the best SprayBudget by score (insertion sort of indices; at most 30 entries)
            int m = 0;
            for (int i = 0; i < nc; i++)
            {
                if (!(s_cand[i].score > 0.0) || !(s_cand[i].C.w > 0f)) continue;
                int j = m++;
                while (j > 0 && s_cand[s_order[j - 1]].score < s_cand[i].score) { s_order[j] = s_order[j - 1]; j--; }
                s_order[j] = i;
            }
            int keep = Math.Min(m, MistVolumeMath.SprayBudget);
            double excluded = m > keep ? s_cand[s_order[keep]].score : 0.0;
            ExcludedSprays = m - keep;
            int n = 0, dist = 0;
            for (int i = 0; i < SpraySlots; i++)
            {
                s_c[i] = Vector4.zero; s_r[i] = new Vector4(1f, 1f, 1f, 0f); s_l[i] = Vector4.zero;
                s_s[i] = new Vector4(0f, 0f, 1f / 16f, 1f / 16f); s_n[i] = Vector4.zero; s_w[i] = Vector4.zero;
            }
            for (int r = 0; r < keep; r++)
            {
                ref Candidate k = ref s_cand[s_order[r]];
                double wgt = MistVolumeMath.CutoffWeight(k.score, excluded);
                if (wgt <= 0.0) continue;
                s_c[n] = new Vector4(k.C.x, k.C.y, k.C.z, (float)(k.C.w * wgt));
                s_r[n] = k.R; s_l[n] = k.L; s_s[n] = k.S; s_w[n] = new Vector4(k.W, 0f, 0f, 0f);
                s_n[n] = new Vector4(k.N.x + 0.37f * n, k.N.y + 0.37f * n, k.N.z + 0.37f * n, k.N.w);
                if (k.L.x < 0f) dist++;
                s_l[n].x = 0f;
                n++;
            }
            ActiveSprays = n;
            DisturbanceSprays = dist;
            Shader.SetGlobalVectorArray(IdSprayC, s_c);
            Shader.SetGlobalVectorArray(IdSprayR, s_r);
            Shader.SetGlobalVectorArray(IdSprayL, s_l);
            Shader.SetGlobalVectorArray(IdSprayS, s_s);
            Shader.SetGlobalVectorArray(IdSprayN, s_n);
            Shader.SetGlobalVectorArray(IdSprayW, s_w);
            Shader.SetGlobalVector(IdMist1, new Vector4(n, s_enabled ? 1f : 0f, 1f, (float)s_flow));
        }

        static double Dist(Vector3 eye, double x, double y, double z)
        {
            double dx = x - eye.x, dy = y - eye.y, dz = z - eye.z;
            return Math.Sqrt(dx * dx + dy * dy + dz * dz);
        }

        static bool Sprays(SeaDisturbanceKind k) => k == SeaDisturbanceKind.Breach || k == SeaDisturbanceKind.TentacleRise || k == SeaDisturbanceKind.Slap;

        /// <summary>Matches the bus to the trackers and advances their followers by dt (infinity = snap to the targets).</summary>
        static void UpdateTrackers(bool exterior, double dt)
        {
            for (int i = 0; i < MaxTrackers; i++) s_trk[i].seen = false;
            int count = exterior ? SeaDisturbances.Count : 0;
            for (int d = 0; d < count; d++)
            {
                SeaDisturbance q = SeaDisturbances.Get(d);
                if (!Sprays(q.Kind) || !(q.Strength > 0f)) continue;
                double reach = Math.Max(8.0, Math.Max(q.HalfAlong, q.HalfAcross)) + 25.0;
                int best = -1; double bestD = reach * reach;
                for (int i = 0; i < MaxTrackers; i++)
                {
                    if (!s_trk[i].used || s_trk[i].seen || s_trk[i].kind != q.Kind) continue;
                    double dx = s_trk[i].x - q.X, dz = s_trk[i].z - q.Z, dd = dx * dx + dz * dz;
                    if (dd < bestD) { bestD = dd; best = i; }
                }
                if (best < 0)
                {
                    // a new disturbance: a free tracker, else the weakest falling one
                    double weakest = double.MaxValue;
                    for (int i = 0; i < MaxTrackers; i++)
                    {
                        if (!s_trk[i].used) { best = i; break; }
                        if (s_trk[i].seen) continue;
                        double w = s_trk[i].dens + s_trk[i].veil;
                        if (w < weakest) { weakest = w; best = i; }
                    }
                    if (best < 0) continue;
                    s_trk[best] = new Tracker { used = true, kind = q.Kind, height = double.NaN };   // followers start at 0 (snap sets them)
                }
                ref Tracker tr = ref s_trk[best];
                tr.seen = true;
                double h = Math.Max(0.0, q.HeightAboveSea);
                // the vertical speed of the body (cascades): only between two ticks of a running clock
                tr.rate = !double.IsNaN(tr.height) && dt > 0.0 && !double.IsInfinity(dt) ? (h - tr.height) / dt : 0.0;
                tr.x = q.X; tr.z = q.Z; tr.halfAlong = Math.Max(0.5, q.HalfAlong); tr.halfAcross = Math.Max(0.5, q.HalfAcross);
                tr.heading = q.Heading; tr.height = h; tr.strength = MistVolumeMath.Clamp01(q.Strength);
            }
            int live = 0;
            for (int i = 0; i < MaxTrackers; i++)
            {
                ref Tracker tr = ref s_trk[i];
                if (!tr.used) continue;
                double str = tr.seen ? tr.strength : 0.0;
                double hh = double.IsNaN(tr.height) ? 0.0 : tr.height;
                // cascade target: the body's vertical speed (or the QA writer's forced amount), x its strength
                double act = tr.seen ? Math.Max(MistVolumeMath.CascadeActivity(tr.rate, str), MistVolumeQa.ForcedCascade * str) : 0.0;
                switch (tr.kind)
                {
                    case SeaDisturbanceKind.Breach:
                    {
                        double top = tr.seen ? MistVolumeMath.BreachFootTop(hh, str) : 0.0;
                        double ctop = tr.seen ? MistVolumeMath.BreachCascadeTop(hh) : 0.0;
                        MistVolumeMath.FollowAsym(ref tr.dens, ref tr.densV, str, MistVolumeMath.DistRiseOmega, MistVolumeMath.DistFallOmega, dt);
                        MistVolumeMath.FollowAsym(ref tr.top, ref tr.topV, top, MistVolumeMath.DistRiseOmega, MistVolumeMath.DistFallOmega, dt);
                        MistVolumeMath.FollowAsym(ref tr.veil, ref tr.veilV, act, MistVolumeMath.CascadeRiseOmega, MistVolumeMath.CascadeFallOmega, dt);
                        MistVolumeMath.FollowAsym(ref tr.veilTop, ref tr.veilTopV, ctop, MistVolumeMath.DistRiseOmega, MistVolumeMath.DistFallOmega, dt);
                        break;
                    }
                    case SeaDisturbanceKind.TentacleRise:
                    {
                        double r = Math.Max(tr.halfAlong, tr.halfAcross);
                        double top = tr.seen ? MistVolumeMath.TentacleFootTop(hh, r, str) : 0.0;
                        double ctop = tr.seen ? MistVolumeMath.TentacleCascadeTop * (hh + MistVolumeMath.TentacleTopPad) : 0.0;
                        MistVolumeMath.FollowAsym(ref tr.dens, ref tr.densV, str, MistVolumeMath.DistRiseOmega, MistVolumeMath.DistFallOmega, dt);
                        MistVolumeMath.FollowAsym(ref tr.top, ref tr.topV, top, MistVolumeMath.DistRiseOmega, MistVolumeMath.DistFallOmega, dt);
                        MistVolumeMath.FollowAsym(ref tr.veil, ref tr.veilV, act, MistVolumeMath.CascadeRiseOmega, MistVolumeMath.CascadeFallOmega, dt);
                        MistVolumeMath.FollowAsym(ref tr.veilTop, ref tr.veilTopV, ctop, MistVolumeMath.DistRiseOmega, MistVolumeMath.DistFallOmega, dt);
                        break;
                    }
                    default:   // Slap
                    {
                        // an event: the full column while the slap is on the bus, its density latched (SlapLatch)
                        double top = tr.seen ? MistVolumeMath.SlapTop(hh, tr.halfAlong, 1.0) : 0.0;
                        MistVolumeMath.FollowAsym(ref tr.dens, ref tr.densV, MistVolumeMath.SlapDensityTarget(str), MistVolumeMath.SlapRiseOmega, MistVolumeMath.SlapFallOmega, dt);
                        MistVolumeMath.FollowAsym(ref tr.top, ref tr.topV, top, MistVolumeMath.SlapRiseOmega, MistVolumeMath.SlapFallOmega, dt);
                        break;
                    }
                }
                // a tracker ends when its spray has fallen (and its disturbance left the bus)
                if (!tr.seen && tr.dens < DrawFloor && tr.veil < DrawFloor && tr.densV <= 0.0 && tr.veilV <= 0.0) { tr = default; continue; }
                live++;
            }
            ActiveTrackers = live;
        }

        /// <summary>The ellipsoid candidates of one tracker (L.x = -1 marks a disturbance spray for the diagnostics).</summary>
        static void AddTrackerCandidates(ref Tracker tr, Vector3 eye, double t, ref int nc)
        {
            double sea = BulwarkConst.SeaY;
            double yaw = tr.heading;   // SeaDisturbances: 0 = +z, clockwise toward +x (the shader's yaw convention)
            double drift = tr.z >= BulwarkConst.SpillwayZ1 ? MistVolumeMath.SprayDriftReef : MistVolumeMath.SprayDriftStorm;
            double leanScale = drift / MistVolumeMath.SprayDriftStorm;   // 1 in the storm, 2.25 on the reef
            switch (tr.kind)
            {
                case SeaDisturbanceKind.Breach:
                {
                    // the white foot skirt at the waterline
                    double along = MistVolumeMath.BreachColumnScale * tr.halfAlong + MistVolumeMath.BreachColumnPad;
                    double across = MistVolumeMath.BreachColumnScale * tr.halfAcross + MistVolumeMath.BreachColumnPad;
                    if (tr.dens > DrawFloor && tr.top > 0.5)
                        Push(ref nc, eye, t, tr.x, sea, tr.z, across, tr.top, along, yaw, MistVolumeMath.BreachColumnTau * tr.dens,
                             MistVolumeMath.BreachLean * leanScale, MistVolumeMath.DistTileBurst, MistVolumeMath.DistScrollUp);
                    // the water sheeting off the body while it moves
                    double valong = MistVolumeMath.BreachVeilScale * tr.halfAlong + MistVolumeMath.BreachVeilPad;
                    double vacross = MistVolumeMath.BreachVeilScale * tr.halfAcross + MistVolumeMath.BreachVeilPad;
                    if (tr.veil > DrawFloor && tr.veilTop > 0.5)
                        Push(ref nc, eye, t, tr.x, sea, tr.z, vacross, tr.veilTop, valong, yaw, MistVolumeMath.BreachVeilTau * tr.veil,
                             MistVolumeMath.CascadeLean, MistVolumeMath.DistTileStreak, MistVolumeMath.DistScrollDown);
                    break;
                }
                case SeaDisturbanceKind.TentacleRise:
                {
                    double r = Math.Max(tr.halfAlong, tr.halfAcross);
                    double a = MistVolumeMath.TentacleScale * r + MistVolumeMath.TentaclePad;
                    if (tr.dens > DrawFloor && tr.top > 0.5)
                        Push(ref nc, eye, t, tr.x, sea, tr.z, a, tr.top, a, 0.0, MistVolumeMath.TentacleTau * tr.dens,
                             MistVolumeMath.TentacleLean * leanScale, MistVolumeMath.DistTileBurst, MistVolumeMath.DistScrollUp);
                    double ac = MistVolumeMath.TentacleCascadeScale * r + MistVolumeMath.TentacleCascadePad;
                    if (tr.veil > DrawFloor && tr.veilTop > 0.5)
                        Push(ref nc, eye, t, tr.x, sea, tr.z, ac, tr.veilTop, ac, 0.0, MistVolumeMath.TentacleCascadeTau * tr.veil,
                             MistVolumeMath.CascadeLean, MistVolumeMath.DistTileStreak, MistVolumeMath.DistScrollDown);
                    break;
                }
                default:
                {
                    double along = MistVolumeMath.SlapAlongScale * tr.halfAlong + MistVolumeMath.SlapPad;
                    double across = MistVolumeMath.SlapAcrossScale * tr.halfAcross + MistVolumeMath.SlapPad;
                    // a column, not a dome: the centre stands SlapCentreK x the height above the sea, the semi-axis is SlapUpperK
                    // x the height (the part under the water is hidden by the sea)
                    if (tr.dens > DrawFloor && tr.top > 0.5)
                        Push(ref nc, eye, t, tr.x, sea + MistVolumeMath.SlapCentreK * tr.top, tr.z, across, MistVolumeMath.SlapUpperK * tr.top, along, yaw,
                             MistVolumeMath.SlapTau * tr.dens, MistVolumeMath.SlapLean * leanScale, MistVolumeMath.DistTileBurst, MistVolumeMath.DistScrollUp,
                             MistVolumeMath.SlapWhite, true);
                    break;
                }
            }
        }

        /// <summary>Adds one ellipsoid centred at (x, y, z) (a dome when y is the sea surface; the slap column stands higher) with
        /// semi-axes (across, height, along), yaw, the optical depth through its centre across, a downwind lean (west, -x), the
        /// vertical noise tile factor and scroll speed, and its white-water share.</summary>
        static void Push(ref int nc, Vector3 eye, double t, double x, double y, double z, double ax, double ay, double az, double yaw,
                         double tau, double lean, double tileY, double scroll, double white = MistVolumeMath.SprayWhite, bool slap = false)
        {
            if (nc >= MaxCandidates || !(tau > 0.0)) return;
            double rho = MistVolumeMath.RhoForTau(tau, Math.Min(ax, az));
            double rMax = Math.Max(ax, az);
            double tile = MistVolumeMath.DistTile(rMax), tY = tile * tileY;
            ref Candidate k = ref s_cand[nc++];
            k.C = new Vector4((float)x, (float)y, (float)z, (float)rho);
            k.R = new Vector4((float)ax, (float)ay, (float)az, (float)yaw);
            k.L = new Vector4(-1f, 0f, 0f, 1f);
            k.S = new Vector4((float)(-lean), 0f, (float)(1.0 / tile), (float)(1.0 / tY));
            float wn = (float)Wrap(0.5 * t / tile);
            k.N = new Vector4(wn, (float)Wrap(-scroll * t / tY), wn, (float)MistVolumeMath.SprayForward);
            k.W = (float)white;
            // the visible half (above the sea): its centre is half way up; distance and size from there
            // the K5 slap column outranks every other spray near the eye (MistVolumeMath.SlapPriority)
            double de = Dist(eye, x, y + 0.5 * ay, z);
            k.score = (slap ? MistVolumeMath.SlapPriorityFor(de) : MistVolumeMath.KrakenPriority) * MistVolumeMath.SprayScore(tau, Math.Max(rMax, ay), de);
        }

        /// <summary>Isotropic in-scatter radiance of the sky dome for the mist (rgb x the droplet albedo; w = 1 when valid):
        /// the solid-angle average of the sky itself (BwAtmosphereMath.SkyColor, the C# twin of BwSkyColor) over the upper
        /// hemisphere and of the haze colour (FogColorRaw) x 0.7 over the lower one, on a 4 x 8 grid each, from the globals
        /// AtmosphereDriver wrote this frame. A cloud of water droplets under a storm sky is as bright as that dome average:
        /// darker than the bright horizon behind it, so the plume reads as a grey column (round 2: the surface-calibrated
        /// trilight made it the colour of the fog, invisible). Allocation-free (D3 structs).</summary>
        public static Vector4 DomeAmbient()
        {
            AtmoGlobals g = AtmosphereDriver.LastGlobals;
            D3 up = new D3(0, 0, 0), lo = new D3(0, 0, 0);
            double wu = 0.0;
            for (int i = 0; i < 4; i++)
            {
                double el = 11.25 + 22.5 * i, w = Math.Cos(el * BwMath.Deg);
                for (int j = 0; j < 8; j++)
                {
                    double az = 45.0 * j + 22.5;
                    up = up + BwAtmosphereMath.SkyColor(in g, BwAtmosphereMath.Dir(az, el)) * w;
                    lo = lo + BwAtmosphereMath.FogColorRaw(in g, BwAtmosphereMath.Dir(az, -el)) * w;
                    wu += w;
                }
            }
            if (wu <= 0.0) return Vector4.zero;
            D3 a = (up * (1.0 / wu)) * 0.5 + (lo * (LowerHemisphereFactor / wu)) * 0.5;
            double k = MistVolumeMath.Albedo;
            bool valid = a.x + a.y + a.z > 1e-5 && !double.IsNaN(a.x);
            return valid ? new Vector4((float)(a.x * k), (float)(a.y * k), (float)(a.z * k), 1f) : Vector4.zero;
        }

        /// <summary>Fraction of a whole tile (exact wrap in double).</summary>
        static double Wrap(double x) => x - Math.Floor(x);
    }
}
