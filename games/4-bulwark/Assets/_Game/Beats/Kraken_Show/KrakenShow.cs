// Kraken_Show: builds the game's one kraken and its choreography (KrakenDirector, Addendum A1 1.4-1.6; show owner) and the
// QA capture set "show" (-capture <dir> -capture-set show [-capture-only ...]):
//   stills   show_k1_ks04 (s 160, the islet), show_k1_s140 (rising), show_k2_kk1 (s 378, the arm vs the tower),
//            show_k2_s350 (rising), show_k2_s420 (sinking), show_k3_ks07 (s 492, the shadow at the gates),
//            show_k4_ks10 (s 776, the dome behind islet 3, eyes shut), show_k4_kk2 (s 806, head + eyes + drape; the A1 KK2 at
//            s 788 is show_k4_s788), show_k5_s800 (the whip cresting as it starts
//            to lean), show_k5_s814 (the topple), show_k5_s821 (just before the impact), show_k5_ks11 (s 824, the
//            slap), show_stinger_tNN (the ending's end camera at the lantern-platform rest point 3.5 km west, yaw 90, pitch of
//            the ending's look-back at end time tg = NN s: 29 to t_g 18, + 7 over 18-28: EndLookPitch)
//   sequences (960 x 540, s advancing at 2.5 m/s with a pinned camera, ambient clock running): seq_show_k1 (s 112-182, 5 fps),
//            seq_show_k2 (300-480, 4 fps), seq_show_k3 (448-528, 4 fps), seq_show_k4 (704-884, 3 fps); 30 fps smoothness
//            windows seq_show_k2_30 (s 352-358), seq_show_k4_30 (s 792-798), seq_show_k5 (s 770-830 at 3 fps: rise, topple,
//            slap), seq_show_k5_30 (s 812-822: the steady fall into the impact); seq_show_stinger (tg 8-44 at 2 fps, pitch 36).
// Flags: -show-off (no kraken at all, for the no-kraken A/B renders), -show-end-t T (KrakenDirector: stinger end clock).
// Not built with the kraken look-dev rig (-capture-set kraken, -kraken-perf), which owns its own creature.
using System.Collections.Generic;
using Megastructure.Creature;
using Megastructure.QA;
using UnityEngine;

namespace Megastructure.Beats
{
    public sealed class KrakenShow : IBeatBuilder, IWorldUpdater, IKeyShotSubjects
    {
        public string Name => "Kraken_Show";
        public const string Set = "show";
        /// <summary>The shared director (the beat-6 encounter reads KrakenDirector.Instance.Creature).</summary>
        public static KrakenDirector Director { get; private set; }

        KrakenDirector m_dir;
        readonly KrakenShowPose m_subjectPose = new KrakenShowPose();

        public void Build(WorldContext ctx)
        {
            Director = null;
            if (CliArgs.Has("-show-off")) { ctx.Log("Kraken_Show: -show-off, no kraken"); return; }
            if (QaRigs.Requested("kraken") || !string.IsNullOrEmpty(CliArgs.Get("-kraken-perf")))
            { ctx.Log("Kraken_Show: the kraken look-dev rig runs, no choreography"); return; }
            m_dir = KrakenDirector.Build(ctx);
            if (m_dir == null) return;
            Director = m_dir;
            ctx.Register(this);
            ctx.RegisterSubjects(this);
            if (QaRigs.Requested(Set)) QaRigs.Register(Set, Stills, Sequences);
            // pre-warm (14.6): the creature at its K4 pose behind the load fade
            ctx.PrewarmSets.Add(new PrewarmSet
            {
                Name = "Kraken", Eye = new Vector3(3f, 1.7f, 812f), YawDeg = 70f, PitchDeg = -2f,
                Enable = on => { m_prewarm = on; },
            });
            ctx.Log($"Kraken_Show: director built (bus {(m_dir.PublishBus ? "on" : "off: QA writer")}, {KrakenDirectorChoreo.Calls.Length} call cues)");
        }

        bool m_prewarm;

        public void OnFrame(in FrameState f)
        {
            if (m_dir == null) return;
            // per-shot end clock for the stinger stills / sequence (QaRigs.CurrentShot)
            string shot = f.capture ? QaRigs.CurrentShot : null;
            double tg = double.NaN;
            if (!string.IsNullOrEmpty(shot))
            {
                int i = shot.IndexOf("stinger_t", System.StringComparison.Ordinal);
                if (i >= 0 && double.TryParse(shot.Substring(i + 9), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out double v)) tg = v;
                else if (shot == "seq_show_stinger") tg = StingerSeqT0 + QaRigs.CurrentFrame / 2.0;
            }
            m_dir.ForceEndTime = tg;
            K6Porthole(f.s);
            if (m_prewarm)
            {
                var g = f; g.s = 812.0; g.capture = true;
                m_dir.OnFrame(in g);
                return;
            }
            if (shot != null && shot.EndsWith("_nok", System.StringComparison.Ordinal)) { m_dir.HideForQa(in f); return; }   // A/B twin
            m_dir.OnFrame(in f);
        }

        /// <summary>K6 (fix wave S3-7): something huge passes behind the landing-8 porthole of the beat-5 stair (y -27, the K6
        /// sucker pops at s 1008 precede it): the water behind the glass darkens to 15 % (Beat05GateTower.PortholeShade, A1 K6)
        /// while the walker comes down to it: up over 7 -> 4 m before the landing centre, held, gone 2 m after it (keyed to s).</summary>
        static void K6Porthole(double s)
        {
            const int Landing = 8;
            int i = System.Array.IndexOf(Beat05Layout.PortholeLanding, Landing);
            if (i < 0 || i >= Beat05GateTower.PortholeShade.Length) return;
            double c = s > 960.0 && s < 1070.0 ? Beat05Layout.LandingCentre(Landing) : double.NaN;
            Beat05GateTower.PortholeShade[i] = double.IsNaN(c) ? 0f
                : 0.9f * KrakenDirectorChoreo.Ss(s, c - 7.0, c - 4.0) * (1f - KrakenDirectorChoreo.Ss(s, c - 1.0, c + 2.0));
        }

        // ------------------------------------------------------------------ key-shot subjects (11.13 / CompositionLint)
        public void CollectSubjects(string shotId, List<KeyShotSubject> into)
        {
            if (m_dir == null || string.IsNullOrEmpty(shotId)) return;
            double s;
            if (shotId.StartsWith("KS04")) s = 160; else if (shotId.StartsWith("KS07")) s = 492;
            else if (shotId.StartsWith("KS10")) s = 776; else if (shotId.StartsWith("KS11")) s = 824;
            else return;
            var p = m_subjectPose;
            KrakenDirectorChoreo.Evaluate(s, s / 2.5, -1.0, p);
            float sea = KrakenDirectorChoreo.SeaY;
            if (shotId.StartsWith("KS04") || shotId.StartsWith("KS10"))
            {
                if (p.HeadTopAboveSea <= 0f) return;
                KrakenDirectorChoreo.HeadSection(Mathf.Max(sea - p.Root.y, 0f), out float cx, out float cz, out float ha, out float hl);
                Vector3 c = p.Root + Quaternion.Euler(0f, p.Heading, 0f) * new Vector3(cx, 0f, cz);
                // world AABB of the waterline ellipse (half across ha on local x, half along hl on local z, rotated by the
                // heading): tighter than a circle of the larger radius, which spilled out of the 16:10 KS10 margin
                float hr = p.Heading * Mathf.Deg2Rad, sn = Mathf.Sin(hr), cs = Mathf.Cos(hr);
                float ex = Mathf.Sqrt(ha * ha * cs * cs + hl * hl * sn * sn), ez = Mathf.Sqrt(ha * ha * sn * sn + hl * hl * cs * cs);
                var b = new Bounds(new Vector3(c.x, sea + 0.5f * p.HeadTopAboveSea, c.z), new Vector3(2f * ex, p.HeadTopAboveSea, 2f * ez));
                into.Add(new KeyShotSubject(shotId.StartsWith("KS04") ? "kraken_k1_dome" : "kraken_k4_dome", b, 0f));
            }
            else
            {
                for (int i = 0; i < p.DistCount; i++)
                {
                    var d = p.Dist[i];
                    if (shotId.StartsWith("KS07") && d.Kind != SeaDisturbanceKind.Shadow) continue;
                    if (shotId.StartsWith("KS11") && d.Kind != SeaDisturbanceKind.Slap) continue;
                    float r = Mathf.Max(d.HalfAlong, d.HalfAcross);
                    float h = d.Kind == SeaDisturbanceKind.Slap ? 50f : 1f;
                    into.Add(new KeyShotSubject(shotId.StartsWith("KS07") ? "kraken_k3_shadow" : "kraken_k5_slap",
                        new Bounds(new Vector3(d.X, sea + 0.5f * h, d.Z), new Vector3(2f * r, h, 2f * r)), 0f));
                }
            }
        }

        // ------------------------------------------------------------------ capture set "show"
        /// <summary>The ending's end camera at the lantern-platform rest point (KrakenDirectorChoreo.EndEye*, mirrored from the
        /// Wave X ending owner's EndingPath).</summary>
        public const double EndEyeX = KrakenDirectorChoreo.EndEyeX, EndEyeY = KrakenDirectorChoreo.EndEyeY, EndEyeZ = KrakenDirectorChoreo.EndEyeZ;
        const double StingerSeqT0 = 8.0;

        /// <summary>The ending's look-back pitch at t_g (KrakenDirectorChoreo.EndLookPitch, mirrored from its CameraRig).</summary>
        public static double EndLookPitch(double tg) => KrakenDirectorChoreo.EndLookPitch(tg);

        static StillShot StingerStill(double tg)
        {
            var k = Still("show_stinger_t" + tg.ToString("0", System.Globalization.CultureInfo.InvariantCulture), 1497, EndEyeX, EndEyeY, EndEyeZ, 90, EndLookPitch(tg), 66);
            return k;
        }

        static StillShot Still(string tag, double s, double x, double y, double z, double yaw, double pitch, double fov)
        {
            return new StillShot
            {
                tag = tag, set = Set, baseTag = tag, variant = "1080", width = ShotPlan.W1080, height = ShotPlan.H1080,
                s = s, x = x, y = y, z = z, yaw = yaw, pitch = pitch, fov = fov, ambient = s / 2.5, gameFov = false, framing = false,
            };
        }

        List<StillShot> Stills()
        {
            var l = new List<StillShot>
            {
                Still("show_k1_s140", 140, 4.2, 1.7, 140, 80, -6, 64),
                Still("show_k1_ks04", 160, 4.2, 1.7, 160, 80, -8, 64),
                Still("show_k2_s350", 350, -1.5, 1.7, 350, 30, 4, 64),
                Still("show_k2_kk1", 378, -1.5, 1.7, 378, 22, 4, 64),
                Still("show_k2_s420", 420, -1.5, 1.7, 420, 35, 2, 64),
                Still("show_k3_ks07", 492, 1.0, 1.7, 492, 70, -6, 66),
                Still("show_k3_s476", 476, 1.0, 1.7, 492, 70, -6, 66),
                Still("show_k3_s486", 486, 1.0, 1.7, 492, 70, -6, 66),
                Still("show_k3_ks07_nok", 492, 1.0, 1.7, 492, 70, -6, 66),
                Still("show_k3_s476_nok", 476, 1.0, 1.7, 492, 70, -6, 66),
                Still("show_k4_ks10", 776, 3.0, 1.7, 776, 78, -3, 64),
                Still("show_k4_s788", 788, 3.0, 1.7, 788, 78, -3, 64),
                Still("show_k4_kk2", 806, 3.0, 1.7, 806, 72, -2, 64),
                Still("show_k2_s450", 450, -1.5, 1.7, 450, 40, 2, 64),
                Still("show_k5_s800", 800, 3.0, 1.7, 800, 45, 3, 64),
                Still("show_k5_s814", 814, 0.9, 1.7, 814, 30, 5, 64),
                Still("show_k5_s821", 821, 0.9, 1.7, 821, 30, 5, 64),
                Still("show_k5_ks11", 824, 0.9, 1.7, 824, 25, 6, 64),
                Still("show_k5_s828", 828, 0.9, 1.7, 828, 35, 4, 64),
                StingerStill(9), StingerStill(12), StingerStill(16), StingerStill(22), StingerStill(28), StingerStill(39),
            };
            if (CliArgs.Has("-show-4k"))
            {
                int n = l.Count;
                for (int i = 0; i < n; i++)
                {
                    var b = l[i];
                    var k = Still(b.tag + "_3840x2160", b.s, b.x, b.y, b.z, b.yaw, b.pitch, b.fov);
                    k.width = ShotPlan.W4K; k.height = ShotPlan.H4K; k.variant = "4k"; k.baseTag = b.tag; k.ambient = b.ambient;
                    l.Add(k);
                }
            }
            return l;
        }

        static SequenceSpec Seq(string name, double s0, double s1, int fps, double x, double y, double z, double yaw, double pitch, double fov)
        {
            double secs = (s1 - s0) / 2.5;
            return new SequenceSpec
            {
                name = name, set = Set, purpose = "kraken choreography (s advancing at 2.5 m/s, pinned camera)", mode = SeqMode.WalkPinned,
                fps = fps, width = 960, height = 540, frames = Mathf.Max(2, (int)System.Math.Round(secs * fps)), s0 = s0, s1 = s1, speed = 2.5,
                x = x, y = y, z = z, yaw = yaw, pitch = pitch, fov = fov, ambient0 = s0 / 2.5, advanceAmbient = true,
            };
        }

        List<SequenceSpec> Sequences(int w, int h)
        {
            var l = new List<SequenceSpec>
            {
                Seq("seq_show_k1", 112, 182, 5, 4.2, 1.7, 160, 80, -7, 64),
                Seq("seq_show_k2", 300, 480, 4, -1.5, 1.7, 378, 25, 4, 64),
                Seq("seq_show_k3", 448, 528, 4, 1.0, 1.7, 492, 70, -6, 66),
                Seq("seq_show_k4", 704, 884, 3, 3.0, 1.7, 800, 66, -1, 64),
                Seq("seq_show_k2_30", 352, 358, 30, -1.5, 1.7, 378, 22, 4, 64),
                Seq("seq_show_k4_30", 792, 798, 30, 3.0, 1.7, 806, 72, -2, 64),
                Seq("seq_show_k5", 770, 830, 3, 0.9, 1.7, 810, 38, 4, 64),
                Seq("seq_show_k5_30", 812, 822, 30, 0.9, 1.7, 818, 30, 5, 64),
            };
            var st = new SequenceSpec
            {
                name = "seq_show_stinger", set = Set, purpose = "stinger from the lantern platform end camera, tg 8-44 s at pitch 36", mode = SeqMode.Static,
                fps = 2, width = 960, height = 540, frames = 72, s0 = 1497, s1 = 1497, x = EndEyeX, y = EndEyeY, z = EndEyeZ, yaw = 90, pitch = 36,
                fov = 66, ambient0 = 1497 / 2.5, advanceAmbient = true,
            };
            l.Add(st);
            return l;
        }
    }
}
