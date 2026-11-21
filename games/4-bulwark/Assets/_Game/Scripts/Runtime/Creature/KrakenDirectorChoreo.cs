// KrakenDirectorChoreo: the kraken's choreography over the walk as a PURE function of the effective progress sEff and the
// unwrapped ambient clock t (Addendum A1 1.4-1.6, LEAD DECISIONS 2 and 3; show owner). KrakenDirector turns the player's s
// into sEff (latch + 1.5 m/s time branch, 1.5) and applies the pose to KrakenRig / KrakenCreature; tests call Evaluate
// directly. No allocation (the pose object is reused), no UnityEngine state (Vector3 / Mathf only).
//
// Geometry of the Tripo model (Assets/_Game/Creature/Kraken manifest, measured on body LOD1, A1 1.2 "the kraken builder
// measures"): head top 168.8 m above the arm crown (A1 table: 190), main eyes 66 m up (A1: 88), head 310 m long (z -211..+99)
// and 160 m wide. The A1 heights are recomputed with these numbers (decisions in the show report):
// Speeds (LEAD DECISION 2, show follow-up, lead correction 14:45): EVERY visible motion <= 8 m/s at the cinematic 2.5 m/s
// walk (so <= 4.8 at 1.5 m/s), the K5 slap included: it is a slow topple whose weight comes from the spray burst and the
// impact sound, not from speed. The motions use KrakenMove (piecewise-linear rate through knots: C1, explicit peaks); the
// parts below the sea may move faster (invisible). Peaks of the choreography at 2.5 m/s (rate x 2.5): K1 rise 7.1, sink
// 7.1; K2 apex 6.4 / 7.1; K4 head 7.2 / 7.2; K4 drape 7.5 / 6.9; K5 rise 7.1, topple 6.6 (impact at 6.6 m/s). The rig's
// slow sway adds up to about 0.6 m/s; KrakenShowTests measures every emerged point on the real rig at 1.5 and 2.5 m/s.
//   K1 islet     crown (1100, 540) heading 250: head top -10 -> +70 m over s 120-158, hold 158-164, sinks 164-214 (under at
//                196.4). Eyes stay 33 m under (70 - 102.8 + 10).
//   transit      200-302 to the K2 crown, body top 72 m deep, trapezoid (20 % ramps), peak 25.8 m/s.
//   K2 column    crown (262, 455) body top 35 m deep; limb 2 rises out at E (330, 690) as an apex-anchored arch: apex from 20 m
//                under to 178 m above the sea over s 296-374 (starts while the crown arrives deep; breaks the surface at
//                308), 178 m at KK1 (s 378), hold 374-394, back under over
//                394-476 (under at 463.5, before the KS07 gate hold at 470); re-enters 75 m beyond E (lean 60,
//                away from the walker: seen nearly end-on the arch reads as one column whose top hooks over, not a hoop).
//   K3 shadow    452-524: the crown (262, 455) -> (262, 560) -> (258, 620) heading 355, body top 35 -> 15 (472) -> 15 (502)
//                -> 70 m deep; on the bus (fix wave S2-1) the mass surges towards the gates in the KS07 pier slot: Shadow
//                130 x 100 + Wake 70 x 55 heading west, x 310 -> 230 at z 495 over 452-490; one arm tip breaks the surface
//                in the slot (176, 497) up to 14 m over 483-503 (K3Sea, K3Tip).
//   transit      516-704 through the reef channel (x 1000, z 704-1040) to the cove crown (500, 1120).
//   K4 ruler     heading 200 (the right eye, which looks sideways-forward, faces the walker; from straight ahead the
//                sockets hide both eyes): 704-712 head top -72 -> -10, 712-800 -10 -> +179 (eyes +76, lids open 792-800, then
//                they track the camera),
//                hold 800-806, sinks to -72 over 806-905 (under at 873). Limb 2 drapes over islet (260, 830) as an arch (apex
//                +80 m): up 772-816 (breaks the surface at 783.3; KS10 at 776 sees no arm), hold 816-830, back under 830-878.
//   K5 whip      a thin arch (limb 3, apex +55 m) at the belt edge (160, 960): rises 762-804 (out of the sea from 778, after
//                KS10), and from 786 it topples outward like a colossal column: the lean accelerates over 786-806 (8 s) and
//                falls at a steady 6.6 m/s over 806-822 onto the sea (Slap at s 822, KS11 at 824 sees the spray 0.8 s later).
//   beat 5..7    hidden (the beat-6 encounter is the gallery owner's: KrakenDirector.Encounter).
//   stinger      by the end time tg (KrakenDirector): three free-tip columns stand behind the crest at z 1497 (-560..+600 m),
//                a 3.5x animal (StingerScale, LEAD RULING R3 of the fix wave), seen from the lantern platform 3.5 km west (LEAD
//                DECISION 1 geometry). They appear while the end camera still looks below the crest (KrakenDirector:
//                never popping on screen), already standing 1100-1500 m tall; the end camera's tilt (t_g 1.5-8.5) brings their
//                shafts into KS28 rising out of the top edge, and the ending's slow tilt-up (t_g 18-28, +7 deg) reveals the
//                hooked tops 4-6 deg above the crest. All the while they rise slowly (StingerRise: peak 15 m/s + sway <= 12
//                m/s sideways, <= 20 m/s = LEAD DECISION 2's 8 m/s in the 2.5x animal's own proportions; 0.12 deg/s seen from
//                7.4 km), until the fade (40-44); they sink in the black (46-66).
//                Needs the creature's dam clip (KrakenDirector.DamClipReady; the kraken owner's _DamClip, in since build16).
// Every bus entry (SeaDisturbances) is derived here so the sea, mist and audio read the same numbers in play and capture.
using UnityEngine;

namespace Megastructure.Creature
{
    /// <summary>A hero arm whose centre line follows a world-space arch or column (KrakenDirectorArms writes it over the rig).</summary>
    public sealed class KrakenArmPose
    {
        public const int MaxWaypoints = 12;
        /// <summary>Rig limb index (0..7).</summary>
        public int Limb = -1;
        /// <summary>0 = the rig's own pose, 1 = the path.</summary>
        public float Weight;
        /// <summary>World waypoints of the path after the rig's own chain (Catmull-Rom through them).</summary>
        public readonly Vector3[] P = new Vector3[MaxWaypoints];
        public int Count;
        /// <summary>Waypoint index whose position is anchored to material arc AnchorU (the apex of an arch / the tip of a column).</summary>
        public int AnchorIndex;
        public float AnchorU;
        /// <summary>World direction the sucker side should face (zero = parallel transport only).</summary>
        public Vector3 Underside;
        /// <summary>Material arc where the path takes over from the rig's chain (m).</summary>
        public float UStart = 150f;
        /// <summary>0..1: how fast the emerged part rises or sinks now (1 = 6 m/s at the cinematic 2.5 m/s): cascades and spray
        /// on the bus follow the motion, a holding arm only keeps its foam collar.</summary>
        public float Motion;

        public void Clear() { Limb = -1; Weight = 0f; Count = 0; AnchorIndex = 0; AnchorU = 0f; Underside = Vector3.zero; UStart = 150f; Motion = 0f; }
        public void Add(Vector3 p) { if (Count < MaxWaypoints) P[Count++] = p; }
    }

    /// <summary>The pose of one frame (filled by KrakenDirectorChoreo.Evaluate; reused, no allocation).</summary>
    public sealed class KrakenShowPose
    {
        public const int MaxArms = 3;
        public bool Visible;
        public string Appearance = "";
        public Vector3 Root;
        /// <summary>Creature scale (1 everywhere; StingerScale for the stinger).</summary>
        public float Scale = 1f;
        public float Heading, Pitch;
        public float Lurk = 1f, Swim, Withdraw;
        /// <summary>Lids: -1 automatic blink, 0 open .. 1 closed.</summary>
        public float Lids = -1f;
        /// <summary>0 = the eyes wander, 1 = they track the camera.</summary>
        public float Look;
        public float HeadWetness = 1f;
        /// <summary>Head top above the mean sea (m) and its vertical speed (m/s at the cinematic 2.5 m/s).</summary>
        public float HeadTopAboveSea, HeadSpeed;
        public readonly KrakenArmPose[] Arms = { new KrakenArmPose(), new KrakenArmPose(), new KrakenArmPose() };
        public int ArmCount;
        /// <summary>Choreography bus entries (head breach/shadow, wake, slaps; the arm crossings are added by the director).</summary>
        public readonly SeaDisturbance[] Dist = new SeaDisturbance[SeaDisturbances.Max];
        public int DistCount;

        public void Reset()
        {
            Visible = false; Appearance = ""; Root = Vector3.zero; Heading = 0f; Pitch = 0f; Scale = 1f;
            Lurk = 1f; Swim = 0f; Withdraw = 0f; Lids = -1f; Look = 0f; HeadWetness = 1f; HeadTopAboveSea = -999f; HeadSpeed = 0f;
            for (int i = 0; i < Arms.Length; i++) Arms[i].Clear();
            ArmCount = 0; DistCount = 0;
        }

        public KrakenArmPose NewArm() { if (ArmCount >= Arms.Length) return null; var a = Arms[ArmCount++]; a.Clear(); return a; }
        public void AddDist(in SeaDisturbance d) { if (DistCount < Dist.Length && d.Strength > 0f) Dist[DistCount++] = d; }
    }

    /// <summary>One audible event of the schedule (KrakenCalls; also the offline audio render's table).</summary>
    public struct KrakenCallCue
    {
        public double S;               // sEff at which the event starts (the sound precedes the sight by 2-6 s, LEAD 3)
        public KrakenCallKind Kind;
        public Vector3 Position;
        /// <summary>Strength 0..1; Duration = the recipe length in seconds (the minimum).</summary>
        public float Strength, Duration;
        /// <summary>sEff where the part of the appearance this sound accompanies ends (fix wave S2-2): the published duration
        /// stretches so the sound lasts until the walker reaches SEnd at the current s rate, then releases over
        /// KrakenDirectorChoreo.CueRelease seconds. SEnd &lt;= S: a fixed-length event (slaps, sound-only calls).</summary>
        public double SEnd;
        public bool Underwater;
        public string Id;
        public KrakenCallCue(string id, double s, KrakenCallKind kind, Vector3 pos, float strength, float duration, bool underwater = false)
        { Id = id; S = s; Kind = kind; Position = pos; Strength = strength; Duration = duration; Underwater = underwater; SEnd = s; }
        public KrakenCallCue(string id, double s, double sEnd, KrakenCallKind kind, Vector3 pos, float strength, float duration, bool underwater = false)
        { Id = id; S = s; Kind = kind; Position = pos; Strength = strength; Duration = duration; Underwater = underwater; SEnd = sEnd; }
    }

    /// <summary>A motion 0 -> 1 over [first knot, last knot] of s whose rate is piecewise linear through the knots (s_i, w_i)
    /// (relative weights, normalised so the motion integrates to 1): C1 position and an explicit peak rate (max w / integral
    /// per m of s), so the visible part of a rise can be capped at 8 m/s while the part under the sea goes faster. Pure; built
    /// once (static readonly), no allocation per call.</summary>
    public sealed class KrakenMove
    {
        readonly double[] m_s, m_w, m_c;
        readonly double m_total;

        public KrakenMove(params double[] knots)
        {
            int n = knots.Length / 2;
            m_s = new double[n]; m_w = new double[n]; m_c = new double[n];
            for (int i = 0; i < n; i++) { m_s[i] = knots[2 * i]; m_w[i] = knots[2 * i + 1]; }
            for (int i = 1; i < n; i++) m_c[i] = m_c[i - 1] + 0.5 * (m_w[i - 1] + m_w[i]) * (m_s[i] - m_s[i - 1]);
            m_total = System.Math.Max(m_c[n - 1], 1e-9);
        }

        public double S0 => m_s[0];
        public double S1 => m_s[m_s.Length - 1];
        /// <summary>Peak rate (per m of s; x 2.5 = m/s per metre of travel at the cinematic walk).</summary>
        public double PeakRate { get { double m = 0; for (int i = 0; i < m_w.Length; i++) m = System.Math.Max(m, m_w[i]); return m / m_total; } }

        int Seg(double s) { int i = 0; while (i < m_s.Length - 2 && m_s[i + 1] < s) i++; return i; }

        /// <summary>Fraction 0..1 at s.</summary>
        public float At(double s)
        {
            if (s <= m_s[0]) return 0f;
            if (s >= m_s[m_s.Length - 1]) return 1f;
            int i = Seg(s);
            double d = s - m_s[i], L = System.Math.Max(m_s[i + 1] - m_s[i], 1e-9);
            return (float)((m_c[i] + m_w[i] * d + 0.5 * (m_w[i + 1] - m_w[i]) * d * d / L) / m_total);
        }

        /// <summary>d(fraction)/ds at s (per m of s).</summary>
        public float Rate(double s)
        {
            if (s <= m_s[0] || s >= m_s[m_s.Length - 1]) return 0f;
            int i = Seg(s);
            double u = (s - m_s[i]) / System.Math.Max(m_s[i + 1] - m_s[i], 1e-9);
            return (float)((m_w[i] + (m_w[i + 1] - m_w[i]) * u) / m_total);
        }
    }

    public static class KrakenDirectorChoreo
    {
        // ------------------------------------------------------------------ model and world numbers
        public const float SeaY = (float)BulwarkConst.SeaY;          // -8
        /// <summary>Head top above the arm crown (manifest body LOD1).</summary>
        public const float HeadTop = 168.8f;
        /// <summary>Main eye centres above the crown (manifest).</summary>
        public const float EyeUp = 66f;
        /// <summary>Depth of the head top during deep transits (body top >= 40 m deep, A1 1.3).</summary>
        public const float DeepTop = -72f;
        public const float DeepCrownY = SeaY + DeepTop - HeadTop;
        /// <summary>Walkable corridor clearance (A1 1.4): every kraken vertex >= 140 m east of the parapet plane x 6 for s <= 920.</summary>
        public const float CorridorX = 6f + 140f;
        /// <summary>Time-branch rate of the crest appearances (A1 1.5).</summary>
        public const double TimeBranchSpeed = 1.5;
        /// <summary>Beyond this s the phases are progress only (interiors, A1 1.5).</summary>
        public const double InteriorS = 920.0;
        /// <summary>Segment ends of the crest choreography: a lingering player sees the current segment finish at 1.5 m/s,
        /// then the kraken waits (deep) until s reaches the next segment.</summary>
        public static readonly double[] SegmentEnds = { 100, 214, 302, 466, 524, 704, 910 };
        /// <summary>LEAD DECISION 2 (with the lead's correction of 2026-11-20 14:45): every visible motion at most 8 m/s at the
        /// cinematic 2.5 m/s walk, the K5 slap included.</summary>
        public const float MaxVisibleSpeed = 8f;

        // ------------------------------------------------------------------ the motions (LEAD DECISION 2: visible <= 8 m/s at 2.5)
        /// <summary>K1 head top -10 -> +70 m (s 120-158): peak 80 / 28 = 2.86 m per m of s = 7.1 m/s.</summary>
        public static readonly KrakenMove K1Rise = new KrakenMove(120, 0, 130, 1, 148, 1, 158, 0);
        /// <summary>K1 head top +70 -> -72 m (s 164-214): 2.85 m per m of s (7.1 m/s) until it is under (s 196.4), then twice
        /// that below the sea (invisible), so the transit can start at s 200 with the body top >= 40 m deep from s 205.</summary>
        public static readonly KrakenMove K1Sink = new KrakenMove(164, 0, 176, 1, 196, 1, 201, 2, 208, 2, 214, 0);
        public const double Transit12S0 = 200.0, Transit12S1 = 302.0;
        /// <summary>K2 apex -20 -> +178 m (s 296-374): 198 / 69 = 2.87 m per m of s (7.2 m/s); it starts while the crown is still
        /// arriving deep (transit end 302); out of the sea from s 308; fix wave S3-5: at the apex from s 374, so KK1 (s 378)
        /// sees the full 178 m arm (held to 394).</summary>
        public static readonly KrakenMove K2Rise = new KrakenMove(296, 0, 306, 1, 366, 1, 374, 0);
        /// <summary>K2 apex back to -20 m (s 394-476): 198 / 70 = 2.83 m per m of s (7.1 m/s); under at s 463.5.</summary>
        public static readonly KrakenMove K2Sink = new KrakenMove(394, 0, 406, 1, 464, 1, 476, 0);
        /// <summary>K4 head top -10 -> +179 m (s 712-800): 189 / 66 = 2.86 m per m of s (7.2 m/s); 144.6 m at KS10 (s 776) as
        /// before, 175 m when the lids open (s 792).</summary>
        public static readonly KrakenMove K4Rise = new KrakenMove(712, 0, 732, 1, 776, 1, 800, 0);
        /// <summary>K4 head top +179 -> -72 m (s 806-905): 251 / 90 = 2.79 m per m of s (7.0 m/s); fix wave S3-6: starts at
        /// 806 with a 6 m ramp (was 808 / 16 m), so KS11 (s 824) sees 139 m instead of 156 and the head is under at s 873
        /// (was 878). Sinking it by s 840 would take 179 m / 28 = 6.4 m per m of s = 16 m/s (LEAD DECISION 2: 8).</summary>
        public static readonly KrakenMove K4Sink = new KrakenMove(806, 0, 812, 1, 893, 1, 905, 0);
        /// <summary>K4 drape apex -25 -> +80 m (s 772-816): 105 / 35 = 3.0 m per m of s (7.5 m/s); out of the sea from s 783.3,
        /// so KS10 (s 776, apex 21 m under) shows no arm.</summary>
        public static readonly KrakenMove K4DrapeRise = new KrakenMove(772, 0, 778, 1, 804, 1, 816, 0);
        /// <summary>K4 drape back to -25 m (s 830-878): 105 / 38 = 2.76 m per m of s (6.9 m/s); under at s 866.</summary>
        public static readonly KrakenMove K4DrapeSink = new KrakenMove(830, 0, 838, 1, 866, 1, 878, 0);
        /// <summary>K5 whip apex -30 -> +55 m (s 762-804): 85 / 30 = 2.83 m per m of s (7.1 m/s); out of the sea from s 778
        /// (after KS10 at 776: apex 7 m under then).</summary>
        public static readonly KrakenMove K5Rise = new KrakenMove(762, 0, 774, 1, 792, 1, 804, 0);
        /// <summary>K5 topple along the apex path (arc length 68.8 m), LEAD DECISION 2 with the lead's correction (the slap is
        /// no exception): the lean starts while the whip is still cresting (s 786; the two motions partly cancel), accelerates
        /// for 20 m of s (8 s) like a colossal column tipping over, then falls at a steady 68.8 / 26 = 2.65 m per m of s
        /// (6.6 m/s) for 16 m of s (6.4 s) onto the sea at s 822. The weight is sold by the spray burst and the impact sound.</summary>
        public static readonly KrakenMove K5Topple = new KrakenMove(786, 0, 806, 1, 822, 1);
        /// <summary>K3: the dark mass glides along K3Path (165 m) over s 452-526: 165 / 61 = 2.70 m per m of s (6.8 m/s; the old
        /// smoothstep over 452-524 peaked at 8.6). The shadow and wake on the sea move with it.</summary>
        public static readonly KrakenMove K3Glide = new KrakenMove(452, 0, 464, 1, 512, 1, 526, 0);
        /// <summary>End of the topple's acceleration and the impact (s).</summary>
        public const double K5FallS = 806.0, K5ImpactS = 822.0;

        // K3 at the gates (fix wave S2-1). From the KS07 pose (1, 1.7, 492; yaw 70, pitch -6, vFOV 66) the sea shows only in
        // the slot between the spillway piers: bearings 80-100 deg, 68-363 m out (rows 475-570 at 1080p); the old shadow centre
        // (75 m behind the crown, z 380-485) never entered it. Now the mass surges towards the gates along the bearing-90 line:
        // a Shadow 130 x 100 m (heading 270, the core 0.62 A x 0.62 B centred 0.25 A ahead: x 117-278, z 433-557 at the end:
        // the slot's whole width; its slick and 0.8 m swell over it) and a Wake (Kelvin V opening east, back down the
        // slot) move from x 310 to 230 over s 452-490 (80 m / 32 = 2.5 m per m of s = 6.3 m/s), then the wake dies and the
        // shadow fades in place as the crown dives (s 502-516). One arm tip breaks the surface in the slot (E (176, 497):
        // bearing 88, 175 m out, the waterline at about (1237, 499) px): up to 14 m over s 472-490 (6.1 m/s), held to 496,
        // under again by 503 (6.3 m/s). The crown keeps its path (262, 455) -> north (the head >= 146 m out, A1 1.4).
        public const double K3SeaS0 = 446.0;
        public static readonly KrakenMove K3Surge = new KrakenMove(452, 0, 458, 1, 484, 1, 490, 0);
        public static readonly Vector2 K3SurgeFrom = new Vector2(310f, 495f), K3SurgeTo = new Vector2(230f, 495f);
        public static readonly Vector3 K3TipE = new Vector3(176f, SeaY, 497f);
        /// <summary>K3 tip -24 -> +14 m (s 472-490): 38 / 15.5 = 2.45 m per m of s (6.1 m/s).</summary>
        public static readonly KrakenMove K3TipRise = new KrakenMove(472, 0, 475, 1, 488, 1, 490, 0);
        /// <summary>K3 tip back to -24 m (s 496-513): 38 / 15 = 2.53 m per m of s (6.3 m/s); under at s 502.6.</summary>
        public static readonly KrakenMove K3TipSink = new KrakenMove(496, 0, 498, 1, 511, 1, 513, 0);
        public const float K3TipHeight = 14f, K3TipDepth = -24f, K3TipAnchorU = 1120f, K3TipSpan = 18f;
        public const int K3TipLimb = 1;

        // crest positions (x, z)
        public static readonly Vector2 K1Crown = new Vector2(1100f, 540f);
        public const float K1Heading = 250f;
        public static readonly Vector2 K2Crown = new Vector2(262f, 455f);
        public static readonly Vector3 K2Exit = new Vector3(330f, SeaY, 690f);
        public const float K2Lean = 44f, K2Height = 178f, K2Span = 75f, K2ApexU = 700f;   // A1 1.6: 180 m out of the water
        public const int K2Limb = 2;
        public static readonly Vector2[] K3Path = { new Vector2(262f, 455f), new Vector2(262f, 560f), new Vector2(258f, 620f) };
        public static readonly Vector2[] Transit2 =
        {
            new Vector2(258f, 620f), new Vector2(560f, 655f), new Vector2(900f, 676f), new Vector2(1000f, 760f),
            new Vector2(1000f, 1000f), new Vector2(950f, 1092f), new Vector2(700f, 1118f), new Vector2(500f, 1120f),
        };
        public static readonly Vector2 K4Crown = new Vector2(500f, 1120f);
        public const float K4Heading = 200f, K4Top = 179f;
        public static readonly Vector2 K4Islet = new Vector2((float)BulwarkConst.IsletX[1], (float)BulwarkConst.IsletZ[1]);   // (260, 830)
        public const float K4DrapeHalf = 80f, K4DrapeHeight = 80f, K4DrapeApexU = 520f;
        public const int K4Limb = 2;
        public static readonly Vector3 K5Point = new Vector3(160f, SeaY, 960f);
        public const float K5Height = 60f, K5ApexU = 1060f;   // fix wave S3-6: the A1 60 m (rise 90 / 30 = 3.0 m per m of s = 7.5 m/s)
        public const int K5Limb = 3;
        public static readonly Vector2 K6Crown = new Vector2(300f, 1300f);
        /// <summary>Stinger: z of the lantern platform's line of sight (the ledge end / elevator, LEAD DECISION 1).</summary>
        public const float StingerZ = 1497f;
        /// <summary>Stinger scale: seen from the lantern platform the crest stands at 59.7 deg and the columns are 7.7 km
        /// away; a 1x arm (<= 21 m at the waterline) would be a 2.8 px hairline at 1080p. Wave X: 2.5x; fix wave S2-3 / LEAD
        /// RULING R3: 3.5x (the visible tip runs 1.4x thicker; the kraken builder darkens it to a silhouette).</summary>
        public const float StingerScale = 3.5f;     // LEAD RULING R3 (fix wave 2026-11-20 17:05): 3.5 approved
        /// <summary>Crown of the stinger animal: its 591 m head (3.5 x 168.8) stays behind the crest line seen from the platform
        /// (head top y 491, the line 1250 m there) and its face (375 m ahead of the crown) 155 m off the sea face.</summary>
        public static readonly Vector3 StingerCrown = new Vector3(530f, -100f, StingerZ);

        // ------------------------------------------------------------------ head cross-section (local y -> z/x extents, body LOD1)
        static readonly float[] ProfY = { 0, 10, 20, 30, 40, 50, 60, 70, 80, 90, 100, 110, 120, 130, 140, 150, 160, 168.8f };
        static readonly float[] ProfZ0 = { -179, -191, -208, -211, -210, -212, -220, -220, -214, -214, -212, -208, -204, -205, -189, -179, -159, -110 };
        static readonly float[] ProfZ1 = { 107, 96, 98, 102, 98, 90, 86, 72, 67, 64, 64, 60, 52, 29, 19, 9, -7, -60 };
        static readonly float[] ProfX0 = { -93, -82, -71, -76, -76, -83, -83, -83, -83, -83, -82, -80, -77, -69, -64, -62, -50, -8 };
        static readonly float[] ProfX1 = { 93, 80, 69, 70, 72, 74, 76, 77, 77, 78, 77, 72, 68, 63, 54, 48, 39, 8 };

        /// <summary>Head section at local height y: centre (x, z) and half extents (across = x, along = z), in metres.</summary>
        public static void HeadSection(float y, out float cx, out float cz, out float halfAcross, out float halfAlong)
        {
            int n = ProfY.Length;
            float t; int i;
            if (y <= ProfY[0]) { i = 0; t = 0f; }
            else if (y >= ProfY[n - 1]) { i = n - 2; t = 1f; }
            else { i = 0; while (i < n - 2 && ProfY[i + 1] < y) i++; t = (y - ProfY[i]) / (ProfY[i + 1] - ProfY[i]); }
            float z0 = Mathf.Lerp(ProfZ0[i], ProfZ0[i + 1], t), z1 = Mathf.Lerp(ProfZ1[i], ProfZ1[i + 1], t);
            float x0 = Mathf.Lerp(ProfX0[i], ProfX0[i + 1], t), x1 = Mathf.Lerp(ProfX1[i], ProfX1[i + 1], t);
            cx = 0.5f * (x0 + x1); cz = 0.5f * (z0 + z1); halfAcross = 0.5f * (x1 - x0); halfAlong = 0.5f * (z1 - z0);
        }

        // ------------------------------------------------------------------ the call schedule (KrakenCalls, A1 2.4 + LEAD 3)
        /// <summary>Ordered by S. Sound precedes sight (LEAD DECISION 3, 2-6 s): each Call sits 5-9 m of s before its picture
        /// (2.0-3.6 s at 2.5 m/s, 3.3-6 s at 1.5 m/s; at 1.5 m/s the time branch keeps the same s rate, so the lead is the same
        /// in play): K1 sight s 128.4 (voice 120), K2 309 (301), K4 723.8 (716); the SurfaceRoars sit at the surface breaks
        /// (K2 306, K5 whip 778, K4 drape 783.3) and the starts of the sinks (K2 394, K4 808).
        /// Fix wave S2-2: every voice / roar carries SEnd (the end of the visible part it accompanies); KrakenDirector publishes
        /// Duration = CueDuration(cue, s rate), so at 1.5 m/s the K2 roar lasts until the sink roar takes over (s 394) instead
        /// of ending at s 353 with the 180 m arm standing in silence. S4-8: K4_rise 0.75 and K5_rise 0.35 (the reef ceiling
        /// -19 dBFS over s 784-799). S3-7: K6 sucker pops behind the stair porthole (s 1008, sound only).</summary>
        public static readonly KrakenCallCue[] Calls =
        {
            new KrakenCallCue("K0_wall", 58, KrakenCallKind.Call, new Vector3(1100f, -230f, 300f), 0.45f, 8f),
            new KrakenCallCue("K0_sway", 66, KrakenCallKind.DamShudder, new Vector3(6f, -20f, 66f), 0.25f, 3f),
            new KrakenCallCue("K1_voice", 120, 128, KrakenCallKind.Call, new Vector3(1100f, -60f, 540f), 0.7f, 8f),
            new KrakenCallCue("K1_rise", 128, 176, KrakenCallKind.SurfaceRoar, new Vector3(1100f, SeaY, 540f), 0.5f, 14f),
            new KrakenCallCue("K1_sink", 176, 197, KrakenCallKind.SurfaceRoar, new Vector3(1100f, SeaY, 540f), 0.4f, 14f),
            new KrakenCallCue("Kf_slap", 236, KrakenCallKind.Slap, new Vector3(1100f, SeaY, 520f), 0.55f, 4f),
            new KrakenCallCue("K2_voice", 301, 306, KrakenCallKind.Call, new Vector3(262f, -150f, 455f), 0.85f, 8f),
            new KrakenCallCue("K2_rise", 306, 394, KrakenCallKind.SurfaceRoar, new Vector3(330f, SeaY, 690f), 0.8f, 30f),
            new KrakenCallCue("K2_sink", 394, 464, KrakenCallKind.SurfaceRoar, new Vector3(330f, SeaY, 690f), 0.7f, 28f),
            new KrakenCallCue("K3_voice", 452, 524, KrakenCallKind.Call, new Vector3(250f, -60f, 500f), 0.65f, 24f),
            new KrakenCallCue("Kc_far", 655, KrakenCallKind.Call, new Vector3(1000f, -100f, 900f), 0.5f, 8f),
            new KrakenCallCue("K4_voice", 716, 724, KrakenCallKind.Call, new Vector3(500f, -60f, 1120f), 1.0f, 8f),
            new KrakenCallCue("K4_rise", 724, 808, KrakenCallKind.SurfaceRoar, new Vector3(500f, SeaY, 1120f), 0.75f, 30f),
            new KrakenCallCue("K5_rise", 778, 822, KrakenCallKind.SurfaceRoar, new Vector3(160f, SeaY, 960f), 0.35f, 12f),
            new KrakenCallCue("K4_drape", 782, 830, KrakenCallKind.SurfaceRoar, new Vector3(285f, SeaY, 860f), 0.6f, 16f),
            new KrakenCallCue("K4_eyes", 784, 806, KrakenCallKind.Call, new Vector3(500f, 60f, 1120f), 0.9f, 8f),
            new KrakenCallCue("K4_sink", 808, 874, KrakenCallKind.SurfaceRoar, new Vector3(500f, SeaY, 1120f), 0.45f, 28f),   // 0.45 (audio x2: the K5 slap must clear the sink roar)
            new KrakenCallCue("K5_slap", 822, KrakenCallKind.Slap, new Vector3(214f, SeaY, 991f), 1.0f, 4f),
            new KrakenCallCue("K6_pops", 1008, KrakenCallKind.SuckerPops, new Vector3(12f, -25f, 1008f), 0.5f, 10f),
        };

        /// <summary>Release after SEnd (s, fix wave S2-2).</summary>
        public const float CueRelease = 3f;
        /// <summary>The s rate a sound duration is computed for: the measured sEff rate clamped to the walk's 1.5-2.5 m/s (the
        /// crest time branch never runs slower than 1.5; faster QA runs keep the 2.5 m/s lengths).</summary>
        public static double CueRate(double sRate) => System.Math.Max(1.5, System.Math.Min(2.5, double.IsNaN(sRate) ? 2.5 : sRate));
        /// <summary>Published duration (s) of a cue at the s rate (m/s): the recipe length, or longer so the sound lasts until
        /// the walker reaches SEnd, plus the release. Pure (tests).</summary>
        public static float CueDuration(in KrakenCallCue c, double sRate)
        {
            if (!(c.SEnd > c.S)) return c.Duration;
            return Mathf.Max(c.Duration, (float)((c.SEnd - c.S) / CueRate(sRate)) + CueRelease);
        }

        /// <summary>Stinger sound on the end clock (fix wave S2-3): the first call at full strength as the columns enter the
        /// look-back, then two swells while they rise (t_g 16, 28) so the sound lasts as long as the columns are seen (fade
        /// 40-44).</summary>
        public static readonly double[] StingerCallTgs = { 3.5, 16.0, 28.0 };
        public static readonly float[] StingerCallStrength = { 1.0f, 0.9f, 1.0f };
        public static readonly float[] StingerCallDuration = { 13f, 13f, 14f };

        /// <summary>First stinger cue on the end clock tg: 3.7 s before the end camera's tilt brings the crest (and the shafts
        /// behind it) into the frame at t_g 7.2 (pitch 26.9 + 33 = 59.9 deg).</summary>
        public const double StingerCallTg = 3.5;
        /// <summary>Sink of the columns in the black after the fade (t_g 40-44).</summary>
        public const double StingerSinkT0 = 46.0, StingerSinkT1 = 66.0;
        /// <summary>Peak rise speed of the stinger columns (m/s, world). LEAD RULING R3 (fix wave): 8 m/s in the 3.5x animal's
        /// own proportions = 28 m/s: rise 20 + sideways sway and crook growth <= 9.4 + 6.2 (KrakenShowTests measures every
        /// visible control point). Seen from 7.8 km that is <= 0.1 deg/s.</summary>
        public const float StingerRiseSpeed = 20f;
        /// <summary>Rise rate profile on the end clock (fix wave S2-3: the rise happens IN the look-back frame, the ending's end
        /// camera reaching its look pitch by t_g 9): accelerate t_g 6-10, cruise 10-26, decelerate 26-30 (20 s at the peak
        /// rate): each column rises 22 x 20 = 440 m (1.3 deg seen from the platform; the flanks start 2 and 4 s later).</summary>
        public static readonly KrakenMove StingerRise = new KrakenMove(6, 0, 10, 1, 26, 1, 30, 0);
        public const float StingerRiseSeconds = 20f;
        /// <summary>Column heights above the sea at t_g 0 (main, north flank, south flank): the hooks stand 2.5 / 1.9 / 2.1 deg
        /// above the crest seen from the platform (crest 59.6-59.9 deg), inside the look-back frame at pitch 33-34 (top edge
        /// 66-67 deg) and at its top edge at pitch 29 (62 deg: the shafts rise out of it); they rise to 3.2-3.8 deg above the
        /// crest by t_g 30 (KrakenShowTests.Stinger_ReadsInTheLookBackAndRisesSlowly).</summary>
        public static readonly float[] StingerH0 = { 890f, 752f, 802f };
        /// <summary>Sideways sway amplitude of the columns (m, world; periods 29 and 37 s): <= 9.9 m/s at the top.</summary>
        public const float StingerSway = 28f;

        /// <summary>The ending owner's end camera at the lantern platform: its rest point (EndingPath: the cabin spot (-9.199,
        /// 1497.114) shifted 3500 m west, then north FootNorth + arc FootArcR + east FootEast) with the eye 1.7 m over the -5999
        /// floor; yaw / pitch read from the ending's own CameraRig.EndLookTarget (fix wave S2-3: whatever look-back timing the
        /// ending lands, the show's stills and tests follow it).</summary>
        public const double EndEyeX = -9.199 - 3500.0 + BulwarkConst.FootArcR + BulwarkConst.FootEast, EndEyeY = -5999.0 + 1.7,
                            EndEyeZ = 1497.114 + BulwarkConst.FootNorth + BulwarkConst.FootArcR;
        public const double EndLookYaw = BulwarkConst.EndLookYaw, EndLookFovV = 66.0;
        public static double EndLookPitch(double tg)
        {
            Megastructure.Player.CameraRig.EndLookTarget(tg, out _, out double pitch);
            return pitch;
        }

        // ------------------------------------------------------------------ easing helpers
        public static float Sat(double x) => x <= 0 ? 0f : (x >= 1 ? 1f : (float)x);
        /// <summary>smoothstep of s over [a, b].</summary>
        public static float Ss(double s, double a, double b) { float t = Sat((s - a) / (b - a)); return t * t * (3f - 2f * t); }
        /// <summary>d/ds of smoothstep of s over [a, b].</summary>
        public static float DSs(double s, double a, double b) { float t = Sat((s - a) / (b - a)); return 6f * t * (1f - t) / (float)(b - a); }
        /// <summary>Lerp(v0, v1, smoothstep(s; a, b)) and its derivative per metre of s.</summary>
        static float Seg(double s, double a, double b, float v0, float v1, out float d) { d = (v1 - v0) * DSs(s, a, b); return Lerp(v0, v1, Ss(s, a, b)); }
        /// <summary>smootherstep of s over [a, b].</summary>
        public static float Sss(double s, double a, double b) { float t = Sat((s - a) / (b - a)); return t * t * t * (t * (t * 6f - 15f) + 10f); }

        /// <summary>Position fraction of a trapezoid speed profile (ramps r at both ends): C1, peak speed = 1 / (1 - r) x average.</summary>
        public static float Trapezoid(double x, double r)
        {
            double t = x <= 0 ? 0 : (x >= 1 ? 1 : x);
            r = System.Math.Max(1e-3, System.Math.Min(0.5, r));
            double vmax = 1.0 / (1.0 - r);
            double p;
            if (t < r) p = 0.5 * vmax * t * t / r;
            else if (t > 1 - r) { double u = 1 - t; p = 1 - 0.5 * vmax * u * u / r; }
            else p = 0.5 * vmax * r + vmax * (t - r);
            return (float)p;
        }

        static float Lerp(float a, float b, float t) => a + (b - a) * t;
        static Vector3 Dir(float headingDeg) { float r = headingDeg * Mathf.Deg2Rad; return new Vector3(Mathf.Sin(r), 0f, Mathf.Cos(r)); }
        static float LerpAngle(float a, float b, float t) { float d = Mathf.DeltaAngle(a, b); return a + d * t; }

        /// <summary>Arc-length table of a Catmull-Rom curve through a polyline (built once per path; uniform speed along it).</summary>
        sealed class PathTable
        {
            const int PerSeg = 64;
            public readonly Vector2[] P;
            public readonly float[] L;
            public PathTable(Vector2[] pts)
            {
                int n = (pts.Length - 1) * PerSeg + 1;
                P = new Vector2[n]; L = new float[n];
                P[0] = pts[0]; int k = 1; float acc = 0f;
                for (int seg = 0; seg < pts.Length - 1; seg++)
                {
                    Vector2 p0 = pts[Mathf.Max(seg - 1, 0)], p1 = pts[seg], p2 = pts[seg + 1], p3 = pts[Mathf.Min(seg + 2, pts.Length - 1)];
                    for (int j = 1; j <= PerSeg; j++)
                    {
                        Vector2 q = Cr(p0, p1, p2, p3, j / (float)PerSeg);
                        acc += Vector2.Distance(q, P[k - 1]);
                        P[k] = q; L[k] = acc; k++;
                    }
                }
            }
            public Vector2 At(float f)
            {
                float target = Mathf.Clamp01(f) * L[L.Length - 1];
                int lo = 0, hi = L.Length - 1;
                while (hi - lo > 1) { int m = (lo + hi) >> 1; if (L[m] <= target) lo = m; else hi = m; }
                float u = (target - L[lo]) / Mathf.Max(L[hi] - L[lo], 1e-6f);
                return Vector2.LerpUnclamped(P[lo], P[hi], u);
            }
            public float Length => L[L.Length - 1];
        }

        static PathTable s_k3, s_tr2;
        static PathTable K3Table => s_k3 ?? (s_k3 = new PathTable(K3Path));
        static PathTable Transit2Table => s_tr2 ?? (s_tr2 = new PathTable(Transit2));
        /// <summary>Length (m) of the transit through the reef channel (tests).</summary>
        public static float Transit2Length => Transit2Table.Length;

        static Vector2 Cr(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float t)
        {
            float t2 = t * t, t3 = t2 * t;
            return 0.5f * ((2f * p1) + (-p0 + p2) * t + (2f * p0 - 5f * p1 + 4f * p2 - p3) * t2 + (-p0 + 3f * p1 - 3f * p2 + p3) * t3);
        }

        /// <summary>Limb azimuth in the kraken frame (deg, 0 = +z) at the Tripo cut (manifest arm rest tips; A1 1.2).</summary>
        public static readonly float[] LimbAzimuth = { -69.1f, -25.7f, 19.5f, 64.6f, 111.6f, 154.6f, -158.4f, -116.4f };

        // ------------------------------------------------------------------ evaluate
        /// <summary>The pose at effective progress sEff and ambient time t; tg >= 0 = the end clock (stinger). Pure.</summary>
        public static void Evaluate(double sEff, double t, double tg, KrakenShowPose p)
        {
            p.Reset();
            if (tg >= 0.0) { Stinger(tg, t, p); return; }
            double s = sEff;

            // ---- root (crown) position, heading and depth by s
            Vector2 xz; float heading; float top; float dtop = 0f;     // top = head top above the sea (m), dtop per m of s
            if (s < Transit12S0)
            {
                xz = K1Crown; heading = K1Heading;
                top = K1Top(s, out dtop);
                p.Appearance = s < 100 ? "K0" : "K1";
            }
            else if (s < Transit12S1)
            {
                float f = Trapezoid((s - Transit12S0) / (Transit12S1 - Transit12S0), 0.2);
                xz = Vector2.Lerp(K1Crown, K2Crown, f);
                heading = LerpAngle(K1Heading, K2Heading(), Ss(s, Transit12S0, Transit12S1));
                top = s < K1Sink.S1 ? K1Top(s, out dtop) : Seg(s, 284, Transit12S1, DeepTop, -35f, out dtop);
                p.Appearance = "transit12";
            }
            else if (s < 452)
            {
                xz = K2Crown; heading = K2Heading(); top = -35f;
                p.Appearance = "K2";
            }
            else if (s < 516)
            {
                float f = K3Glide.At(s);
                xz = K3Table.At(f);
                heading = LerpAngle(K2Heading(), 355f, Ss(s, 452, 470));
                if (s < 472) top = Lerp(-35f, -15f, Ss(s, 452, 472));
                else top = Lerp(-15f, -70f, Ss(s, 502, 524));
                p.Appearance = "K3";
            }
            else if (s < 704)
            {
                // the K3 glide continues into the transit (both eased): positions blend over 516-524
                Vector2 k3 = K3Table.At(K3Glide.At(s));
                float f = Trapezoid((s - 516) / (704 - 516), 0.12);
                Vector2 tr = Transit2Table.At(f);
                float w = Ss(s, 516, 524);
                xz = Vector2.Lerp(k3, tr, w);
                heading = LerpAngle(355f, K4Heading, Ss(s, 640, 704));
                top = s < 524 ? Lerp(-15f, -70f, Ss(s, 502, 524)) : Lerp(-70f, DeepTop, Ss(s, 524, 540));
                p.Appearance = "transit34";
            }
            else if (s < 910)
            {
                xz = K4Crown; heading = K4Heading;
                if (s < 712) top = Seg(s, 704, 712, DeepTop, -10f, out dtop);
                else if (s < K4Sink.S0) { top = -10f + (K4Top + 10f) * K4Rise.At(s); dtop = (K4Top + 10f) * K4Rise.Rate(s); }
                else { top = K4Top - (K4Top - DeepTop) * K4Sink.At(s); dtop = -(K4Top - DeepTop) * K4Sink.Rate(s); }
                p.Appearance = "K4";
            }
            else if (s < 1060)
            {
                float f = Trapezoid((s - 910) / (1060 - 910), 0.2);
                xz = Vector2.Lerp(K4Crown, K6Crown, f);
                heading = K4Heading; top = DeepTop - 200f * Ss(s, 910, 1000);
                p.Appearance = "deep";
            }
            else
            {
                xz = K6Crown; heading = K4Heading; top = DeepTop - 200f;
                p.Appearance = "hidden";
            }

            p.Root = new Vector3(xz.x, SeaY + top - HeadTop, xz.y);
            p.Heading = Wrap360(heading);
            p.HeadTopAboveSea = top;
            p.HeadSpeed = Mathf.Abs(dtop) * 2.5f;
            p.Lurk = 1f;
            p.Swim = (p.Appearance == "transit12" || p.Appearance == "transit34") ? 0.6f * Mathf.Sin(Mathf.PI * Sat(p.Appearance == "transit12" ? (s - Transit12S0) / (Transit12S1 - Transit12S0) : (s - 516) / 188)) : 0f;
            if (p.Appearance == "K3") p.Swim = 0.35f;

            // ---- head on the bus: Breach above the surface, Shadow just below (K1, K3, K4)
            float hc = p.Root.y;           // crown y
            float yWl = SeaY - hc;         // waterline in local head height
            if (top > 0f && yWl < HeadTop)
            {
                HeadSection(Mathf.Max(yWl, 0f), out float cx, out float cz, out float ha, out float hl);
                Vector3 c = p.Root + Quaternion.Euler(0f, p.Heading, 0f) * new Vector3(cx, 0f, cz);
                p.AddDist(new SeaDisturbance
                {
                    Kind = SeaDisturbanceKind.Breach, X = c.x, Z = c.z, HalfAlong = hl * 1.05f, HalfAcross = ha * 1.05f,
                    Heading = p.Heading * Mathf.Deg2Rad, Strength = Mathf.Clamp01(Ss(top, 0, 30) * (0.15f + 0.85f * Mathf.Clamp01(p.HeadSpeed / 3f)) + (p.Appearance == "K1" ? 0.2f * K1Breath(s) : 0f)),   /* lead (kraken request): white water while the head rises (full at 3 m/s), a 0.15 foam collar once it stands, so the breach veil no longer hides the head; fix wave S3-4: + 0.2 per exhalation while K1 stands */ HeightAboveSea = top,
                });
                // fix wave S3-4: the K1 breach throws a white boil ring (a Slap burst: white core, a ring running out from
                // 1.4 A = 84 m to 5 A = 300 m as it fades) when the head breaks the surface (s 128.4)
                if (p.Appearance == "K1" && s >= 126.0 && s < 142.0)
                    p.AddDist(new SeaDisturbance
                    {
                        Kind = SeaDisturbanceKind.Slap, X = c.x, Z = c.z, HalfAlong = 60f, HalfAcross = 60f, Heading = 0f,
                        Strength = 0.9f * Ss(s, 127.0, 130.0) * (1f - Ss(s, 132.0, 141.0)), HeightAboveSea = 22f,
                    });
            }
            else if (top > -60f && top <= 0f && s < 910)
            {
                HeadSection(90f, out float cx, out float cz, out _, out _);
                Vector3 c = p.Root + Quaternion.Euler(0f, p.Heading, 0f) * new Vector3(cx, 0f, cz);
                float strength = Mathf.Clamp01(1f - (-top - 15f) / 45f);
                // a shadow never moves faster than 8 m/s: it fades out while the crown accelerates away from K1 (s 200-206)
                // and in only as it settles at the K2 crown (s 297-301: crown speed 6.3 -> 1.3 m/s)
                if (p.Appearance == "transit12") strength *= s < 250 ? 1f - Ss(s, 201, 206) : Ss(s, 297, 301);
                // K3 (fix wave S2-1): the crown's own shadow dissolves (s 446-458) into the surge towards the gates (K3Sea)
                float own = s < K3SeaS0 ? 1f : 1f - Ss(s, K3SeaS0, K3SeaS0 + 12.0);
                if (own > 0f)
                    p.AddDist(new SeaDisturbance
                    {
                        Kind = SeaDisturbanceKind.Shadow, X = c.x, Z = c.z, HalfAlong = 150f, HalfAcross = 80f,
                        Heading = p.Heading * Mathf.Deg2Rad, Strength = strength * own, HeightAboveSea = 0f,
                    });
                if (s >= K3SeaS0 && s < 524) K3Sea(s, strength, p);
            }

            // ---- eyes
            if (p.Appearance == "K4")
            {
                // lids open over 792-800 (3.2 s at 2.5 m/s, A1 "3 s"); from 806 the automatic slow blink (every 20-40 s)
                p.Lids = s >= 806 ? -1f : 1f - Ss(s, 792, 800);
                p.Look = Ss(s, 794, 804);
            }
            else { p.Lids = 1f; p.Look = 0f; }    // closed while it lurks (no glow under the storm sea)
            // water sheets off for ~8 s after the head is up (A1 1.3), then the crust dries to a slime sheen
            p.HeadWetness = p.Appearance == "K4" ? 1f - 0.65f * Ss(s, 786, 806) : (p.Appearance == "K1" ? 1f - 0.6f * Ss(s, 150, 170) : 1f);

            // ---- hero arms
            if (s >= 292 && s < 480) K2Arch(s, t, p);
            if (s >= 470 && s < 515) K3Tip(s, t, p);
            if (s >= 768 && s < 882) K4Drape(s, t, p);
            if (s >= 758 && s < 850) K5Whip(s, t, p);

            // ---- Kf: the far slap behind the walker (sound and a burst on the sea, never seen)
            if (s >= 236 && s < 246)
                p.AddDist(new SeaDisturbance
                {
                    Kind = SeaDisturbanceKind.Slap, X = 1100f, Z = 520f, HalfAlong = 20f, HalfAcross = 20f, Heading = 0f,
                    Strength = SlapEnvelope(s, 236, 2.5), HeightAboveSea = 18f,
                });

            p.Visible = p.Appearance != "hidden" && (top > -45f || p.ArmCount > 0 || (s >= 440 && s < 530));
        }

        public static float K2Heading() => Wrap360(BearingDeg(new Vector2(K2Exit.x, K2Exit.z) - K2Crown) - LimbAzimuth[K2Limb]);
        public static float BearingDeg(Vector2 d) => Mathf.Atan2(d.x, d.y) * Mathf.Rad2Deg;
        public static float Wrap360(float a) { a %= 360f; return a < 0f ? a + 360f : a; }

        /// <summary>Slap burst: attack 0.2 s, decay 4 s (A1 1.6 K5), in s units at the cinematic rate.</summary>
        static float SlapEnvelope(double s, double s0, double rate)
        {
            double dt = (s - s0) / rate;
            if (dt < 0) return 0f;
            if (dt < 0.2) return (float)(dt / 0.2);
            return Mathf.Exp(-(float)(dt - 0.2) / 1.6f) * (1f - Ss(dt, 3.0, 4.2));
        }

        // ------------------------------------------------------------------ arches
        /// <summary>An apex-anchored arch from E (the thick leg) over the apex to R (re-entry): emergence e 0..1 lifts the apex
        /// from 30 m under water to height H above the sea; the material at AnchorU stays at the apex so both legs feed from the
        /// water (no material slides over the top).</summary>
        public static void Arch(KrakenArmPose a, Vector3 E, Vector3 R, float H, float apexFrac, float e, float lean, double t, float wobble, float bend = 1f)
        {
            Vector3 up = Vector3.up;
            Vector3 span = R - E; span.y = 0f;
            float dist = Mathf.Max(span.magnitude, 1f);
            Vector3 dir = span / dist;
            Vector3 side = Vector3.Cross(up, dir);
            float h = -30f + (H + 30f) * e;                  // apex above the sea
            float sink = Mathf.Max(0f, 1f - e * 3f);          // legs submerge while the apex is under
            // slow life: the apex sways sideways and breathes (<= 0.6 m/s at the apex)
            float sw = wobble * Mathf.Sin(2f * Mathf.PI * KrakenMath.Phase(t, 23.0) + 0.7f);
            float br = wobble * 0.5f * Mathf.Sin(2f * Mathf.PI * KrakenMath.Phase(t, 31.0) + 2.1f);
            float hp = Mathf.Max(h, 0f);
            // an asymmetric hook, not a hoop: the thick leg rises from E leaning back a little, bends over at the apex (past the
            // middle of the span), and the thin leg drops steeply with an S-bend sideways before it re-enters at R
            Vector3 apex = E + dir * (apexFrac * dist) + up * (h + br) + side * sw;
            a.Add(E - dir * 40f + up * (-60f));
            a.Add(E + up * (-10f - 14f * sink));
            a.Add(E - dir * (0.10f * dist * Mathf.Min(1f, hp / 60f)) + side * (0.2f * sw) + up * (0.50f * h - 12f * sink));
            a.Add(E + dir * ((apexFrac * 0.45f) * dist) + side * (0.6f * sw) + up * (0.90f * h + lean));
            a.AnchorIndex = a.Count;
            a.Add(apex);
            a.Add(E + dir * ((apexFrac + (1f - apexFrac) * 0.75f) * dist) - side * (0.06f * bend * hp) + up * (0.86f * h));
            a.Add(R + dir * (0.06f * dist) + side * (0.08f * bend * hp) + up * (0.45f * h - 12f * sink));
            a.Add(R + up * (-10f - 14f * sink));
            a.Add(R - dir * 25f + up * (-60f));
            a.Add(R - dir * 80f + side * 40f + up * (-130f));
        }

        /// <summary>K1 breathing boil 0..1 (A1 1.3, fix wave S3-4): one exhalation of 7.5 m of s (3 s at 2.5 m/s) at s 150, 160
        /// and 170 while the head stands (sin^2 bumps: C1, no step on the bus).</summary>
        public static float K1Breath(double s)
        {
            float b = 0f;
            for (int k = 0; k < 3; k++)
            {
                double u = (s - (150.0 + 10.0 * k)) / 7.5;
                if (u > 0.0 && u < 1.0) { float v = Mathf.Sin(Mathf.PI * (float)u); b = Mathf.Max(b, v * v); }
            }
            return b;
        }

        /// <summary>K1 head top above the sea (m) and its rate per m of s, s 80-214 (rise 120-158, hold, sink 164-214).</summary>
        static float K1Top(double s, out float dtop)
        {
            if (s < 118) return Seg(s, 80, 118, DeepTop, -10f, out dtop);
            if (s < K1Sink.S0) { dtop = 80f * K1Rise.Rate(s); return -10f + 80f * K1Rise.At(s); }
            dtop = -(70f - DeepTop) * K1Sink.Rate(s);
            return 70f - (70f - DeepTop) * K1Sink.At(s);
        }

        /// <summary>K2 apex above the sea (m, before the slow wobble) and its rate per m of s: -20 -> 178 -> -20.</summary>
        public static float K2Apex(double s, out float rate)
        {
            const float span = K2Height + 20f;
            rate = span * (K2Rise.Rate(s) - K2Sink.Rate(s));
            return -20f + span * (K2Rise.At(s) - K2Sink.At(s));
        }

        static void K2Arch(double s, double t, KrakenShowPose p)
        {
            var a = p.NewArm(); if (a == null) return;
            float h = K2Apex(s, out float dh);
            float e = (h + 30f) / (K2Height + 30f);           // Arch: apex = -30 + (H + 30) e
            a.Motion = Mathf.Clamp01(Mathf.Abs(dh) * 2.5f / 6f);
            a.Limb = K2Limb; a.Weight = Ss(s, 292, 296) * (1f - Ss(s, 476, 480));
            Vector3 R = K2Exit + Dir(K2Lean) * K2Span;
            // wobble 2.5 m (sway 0.7 m/s, breath 0.25 m/s at the apex): with 6 m the rig measured 8.2 m/s on a 7.0 rise
            // fix wave S3-5 (KK1 read a two-strand hairpin): the span runs along the walker's line of sight at KK1 (bearing
            // 46.6 from (0, 378); K2Lean 44) and the thin leg's sideways S-bend is cut to 0.2, so the far leg hides behind the
            // 17 m shaft and the top hooks away: one rising arm with its tip curl
            Arch(a, K2Exit, R, K2Height, 0.62f, e, 6f, t, 2.5f, 0.2f);
            a.AnchorU = K2ApexU;
            a.Underside = (Dir(K2Lean) * 0.6f + new Vector3(-0.58f, 0f, -0.55f)).normalized;   // suckers half to the inside of the hook, half to the walker: a red strip along the dark shaft
        }

        /// <summary>K3 surge on the bus (fix wave S2-1): the mass's shadow and V-wake in the pier slot seen from KS07.</summary>
        static void K3Sea(double s, float strength, KrakenShowPose p)
        {
            Vector2 w = Vector2.Lerp(K3SurgeFrom, K3SurgeTo, K3Surge.At(s));
            float fadeIn = Ss(s, K3SeaS0, K3SeaS0 + 12.0);
            const float west = 270f * Mathf.Deg2Rad;
            p.AddDist(new SeaDisturbance
            {
                Kind = SeaDisturbanceKind.Shadow, X = w.x, Z = w.y, HalfAlong = 130f, HalfAcross = 100f,
                Heading = west, Strength = strength * fadeIn, HeightAboveSea = 0f,
            });
            float wake = Ss(s, 450, 460) * (1f - Ss(s, 494, 506));
            if (wake > 0f)
                p.AddDist(new SeaDisturbance
                {
                    Kind = SeaDisturbanceKind.Wake, X = w.x, Z = w.y, HalfAlong = 70f, HalfAcross = 55f,
                    Heading = west, Strength = strength * wake, HeightAboveSea = 0f,
                });
        }

        /// <summary>K3 tip height above the sea (m, before the sway) and its rate per m of s: -24 -> 14 -> -24.</summary>
        public static float K3TipH(double s, out float rate)
        {
            const float span = K3TipHeight - K3TipDepth;
            rate = span * (K3TipRise.Rate(s) - K3TipSink.Rate(s));
            return K3TipDepth + span * (K3TipRise.At(s) - K3TipSink.At(s));
        }

        static void K3Tip(double s, double t, KrakenShowPose p)
        {
            var a = p.NewArm(); if (a == null) return;
            float h = K3TipH(s, out float dh);
            a.Motion = Mathf.Clamp01(Mathf.Abs(dh) * 2.5f / 6f);
            a.Limb = K3TipLimb; a.Weight = Ss(s, 470, 472) * (1f - Ss(s, 513, 515));
            // the tip curls out of the sea and back in (a small apex-anchored hook, like the K5 whip: a free tip of 14 m flips
            // its last tangent while it breaks the surface), across the picture plane of the KS07 look (north), suckers to
            // the walker
            // (fix-wave capture show-fx1: a span across the picture plane read as a red hoop; now the span runs away from the
            // walker, east-north-east, so the far leg hides behind the near one and the top hooks over: a dark tip curling
            // out of the sea, the red suckers on the inside of the curl)
            Vector3 R = K3TipE + new Vector3(K3TipSpan * 0.94f, 0f, K3TipSpan * 0.34f);
            Arch(a, K3TipE, R, K3TipHeight, 0.55f, (h + 30f) / (K3TipHeight + 30f), 2f, t, 0.8f);
            a.AnchorU = K3TipAnchorU;
            a.Underside = new Vector3(0.6f, -0.8f, 0f).normalized;
        }

        /// <summary>K4 drape apex above the sea (m, before the wobble) and its rate per m of s: -25 -> 80 -> -25.</summary>
        public static float K4DrapeApex(double s, out float rate)
        {
            const float span = K4DrapeHeight + 25f;
            rate = span * (K4DrapeRise.Rate(s) - K4DrapeSink.Rate(s));
            return -25f + span * (K4DrapeRise.At(s) - K4DrapeSink.At(s));
        }

        static void K4Drape(double s, double t, KrakenShowPose p)
        {
            var a = p.NewArm(); if (a == null) return;
            float h = K4DrapeApex(s, out float dh);
            float e = (h + 30f) / (K4DrapeHeight + 30f);
            a.Motion = Mathf.Clamp01(Mathf.Abs(dh) * 2.5f / 6f);
            a.Limb = K4Limb; a.Weight = Ss(s, 768, 772) * (1f - Ss(s, 878, 882));
            Vector2 toCrown = (K4Crown - K4Islet).normalized;
            Vector3 E = new Vector3(K4Islet.x + toCrown.x * K4DrapeHalf, SeaY, K4Islet.y + toCrown.y * K4DrapeHalf);
            Vector3 R = new Vector3(K4Islet.x - toCrown.x * K4DrapeHalf, SeaY, K4Islet.y - toCrown.y * K4DrapeHalf);
            Arch(a, E, R, K4DrapeHeight, 0.58f, e, 0f, t, 3f);
            a.AnchorU = K4DrapeApexU;
            a.Underside = new Vector3(-1f, 0.15f, -0.1f).normalized;    // twist: the red underside to the walker (A1 1.6 K4)
        }

        // the K5 topple path: the whip's apex (Arch at e = 1) for a topple fraction f 0..1 (span 25 -> 70 m, apex fraction
        // 0.72 -> 0.92, height H -> 0.08 H); an arc-length table maps K5Topple (a fraction of the path length) to f, so the
        // apex speed is exactly the KrakenMove rate x the path length (68.8 m)
        const int K5ArcN = 128;
        static float[] s_k5Arc;
        static Vector2 K5ApexOf(float f) => new Vector2((0.72f + 0.2f * f) * (25f + 45f * f), K5Height * (1f - 0.92f * f));
        static float[] K5Arc
        {
            get
            {
                if (s_k5Arc != null) return s_k5Arc;
                var L = new float[K5ArcN + 1];
                Vector2 prev = K5ApexOf(0f);
                for (int i = 1; i <= K5ArcN; i++) { Vector2 q = K5ApexOf(i / (float)K5ArcN); L[i] = L[i - 1] + Vector2.Distance(q, prev); prev = q; }
                return s_k5Arc = L;
            }
        }
        /// <summary>Length of the whip apex's topple path (m).</summary>
        public static float K5ToppleLength => K5Arc[K5ArcN];
        static float K5FallOfArc(float u)
        {
            var L = K5Arc;
            float target = Mathf.Clamp01(u) * L[K5ArcN];
            int lo = 0, hi = K5ArcN;
            while (hi - lo > 1) { int m = (lo + hi) >> 1; if (L[m] <= target) lo = m; else hi = m; }
            float w = (target - L[lo]) / Mathf.Max(L[hi] - L[lo], 1e-6f);
            return (lo + w) / K5ArcN;
        }

        static void K5Whip(double s, double t, KrakenShowPose p)
        {
            var a = p.NewArm(); if (a == null) return;
            // rises as a thin arch (apex 55 m), leans over and topples slowly outward onto the sea (the slap at s 822)
            float e = K5Rise.At(s);
            float fall = K5FallOfArc(K5Topple.At(s));
            float down = s >= K5ImpactS ? 1f - Ss(s, 826, 846) : 1f;    // then the whole arch sinks (from 4.4 m, slowly)
            a.Limb = K5Limb; a.Weight = Ss(s, 758, 762) * (1f - Ss(s, 846, 850));
            float speed = Mathf.Max((K5Height + 30f) * K5Rise.Rate(s), K5ToppleLength * K5Topple.Rate(s)) * 2.5f;   // m/s at 2.5 m/s
            a.Motion = s < K5ImpactS ? Mathf.Clamp01(speed / 6f) : 1f - 0.7f * Ss(s, 826, 834);   // no step in the spray
            Vector3 E = K5Point;
            Vector3 R = K5Point + Dir(60f) * (25f + 45f * fall);
            float H = K5Height * (1f - 0.92f * fall);
            Arch(a, E, R, H, 0.72f + 0.2f * fall, e * down, 0f, t, 1.5f);
            a.AnchorU = K5ApexU;
            a.Underside = new Vector3(-0.8f, 0f, -0.6f).normalized;
            if (s >= K5ImpactS && s < 834)
            {
                Vector3 c = K5Point + Dir(60f) * 62f;
                p.AddDist(new SeaDisturbance
                {
                    Kind = SeaDisturbanceKind.Slap, X = c.x, Z = c.z, HalfAlong = 25f, HalfAcross = 25f, Heading = 60f * Mathf.Deg2Rad,
                    Strength = SlapEnvelope(s, K5ImpactS, 2.5), HeightAboveSea = 50f,
                });
            }
        }

        // ------------------------------------------------------------------ stinger
        /// <summary>Free-tip column: the tip (material arc 1300) is anchored at the top; it rises from E to height H. The S-bend
        /// sways along `side` and the top hooks over towards `lean` (both in the viewer's picture plane for a readable
        /// silhouette: seen end-on a hook vanishes).</summary>
        public static void Column(KrakenArmPose a, Vector3 E, float H, float e, Vector3 lean, Vector3 side, double t, float phase, float sway)
        {
            float h = -40f + (H + 40f) * e;
            side = side.sqrMagnitude > 1e-6f ? side.normalized : Vector3.forward;
            float w1 = sway * Mathf.Sin(2f * Mathf.PI * KrakenMath.Phase(t, 29.0) + phase);
            float w2 = sway * Mathf.Sin(2f * Mathf.PI * KrakenMath.Phase(t, 37.0) + 1.3f * phase + 1f);
            a.Add(E + new Vector3(60f, -120f, 0f) * (1f + Ss(h, 600f, 1200f)));
            a.Add(E + Vector3.up * (-20f));
            // an S-bend up the shaft, then the tip hooks over and curls down (a hook ~0.15 h wide, so the silhouette reads as a
            // tentacle, not a needle)
            float hp = Mathf.Max(h, 0f);
            a.Add(E + Vector3.up * (0.28f * h) + side * (0.15f * w1) + lean * (-0.03f * hp));
            a.Add(E + Vector3.up * (0.58f * h) + side * (0.45f * w1 + 0.2f * w2 + 0.04f * hp) + lean * (0.02f * hp));
            a.Add(E + Vector3.up * (0.82f * h) + side * (0.8f * w1 + 0.5f * w2 - 0.02f * hp) + lean * (0.10f * hp));
            a.Add(E + Vector3.up * (0.97f * h) + side * (w1 + 0.7f * w2) + lean * (0.19f * hp));
            a.AnchorIndex = a.Count;
            a.Add(E + Vector3.up * (0.90f * h) + side * (w1 + 0.8f * w2 + 0.03f * hp) + lean * (0.27f * hp));
            a.AnchorU = 1300f;
        }

        /// <summary>Stinger crook (fix wave S2-3): the column's TOP is material arc StingerAnchorU (the thick middle of the arm,
        /// not its tip: the visible shaft runs 3x wider than the tip-anchored column of Wave X), and the rest of the arm curls
        /// over towards `lean`, down to 0.8 h and back in under the hook, then runs away from the viewer (east) and drops
        /// behind the dam line: a tentacle tip curling over, not a needle and not a hoop.</summary>
        public static void Crook(KrakenArmPose a, Vector3 E, float H, float e, Vector3 lean, Vector3 side, double t, float phase, float sway)
        {
            float h = -40f + (H + 40f) * e;
            side = side.sqrMagnitude > 1e-6f ? side.normalized : Vector3.forward;
            float w1 = sway * Mathf.Sin(2f * Mathf.PI * KrakenMath.Phase(t, 29.0) + phase);
            float w2 = sway * Mathf.Sin(2f * Mathf.PI * KrakenMath.Phase(t, 37.0) + 1.3f * phase + 1f);
            float hp = Mathf.Max(h, 0f);
            a.Add(E + new Vector3(60f, -120f, 0f) * (1f + Ss(h, 600f, 1200f)));
            a.Add(E + Vector3.up * (-20f));
            a.Add(E + Vector3.up * (0.28f * h) + side * (0.15f * w1) + lean * (-0.03f * hp));
            a.Add(E + Vector3.up * (0.58f * h) + side * (0.45f * w1 + 0.2f * w2) + lean * (0.01f * hp));
            a.Add(E + Vector3.up * (0.84f * h) + side * (0.8f * w1 + 0.5f * w2) + lean * (0.07f * hp));
            a.AnchorIndex = a.Count;
            a.Add(E + Vector3.up * (1.0f * h) + side * (w1 + 0.7f * w2) + lean * (0.16f * hp));
            // (show-fx3: a tip hanging down to the dam line read as a hoop): the tip curls over, down to 0.8 h and back in
            // under the hook, then the rest of the arm runs away from the viewer (east) and drops behind the dam line
            Vector3 away = new Vector3(1f, 0f, 0f);
            // (show-fx4: a remainder running east at 0.72 h projected as a second leg down to the dam line, a hoop again): the
            // curl now comes back in under the hook and drops BEHIND the shaft (same lean as the shaft, 0.2-0.24 h east of
            // it), hidden by the shaft from the platform, so the silhouette is one thick arm whose top curls over
            a.Add(E + Vector3.up * (0.95f * h) + side * (w1 + 0.8f * w2) + lean * (0.26f * hp));
            a.Add(E + Vector3.up * (0.83f * h) + side * (w1 + 0.8f * w2) + lean * (0.28f * hp) + away * (0.02f * hp));
            a.Add(E + Vector3.up * (0.74f * h) + side * (0.9f * w1 + 0.7f * w2) + lean * (0.18f * hp) + away * (0.12f * hp));
            a.Add(E + Vector3.up * (0.66f * h) + side * (0.7f * w1 + 0.5f * w2) + lean * (0.09f * hp) + away * (0.20f * hp));
            a.Add(E + Vector3.up * (0.50f * h) + side * (0.5f * w1 + 0.3f * w2) + lean * (0.03f * hp) + away * (0.24f * hp));
            a.AnchorU = StingerAnchorU;
        }

        /// <summary>Material arc at the top of each stinger crook (m, unscaled): (740 - 150) x 3.5 = 2065 m of world path from
        /// the take-over arc to the top covers crown -> E (445 m) + the shaft (<= 1330 m) with a short dip.</summary>
        public const float StingerAnchorU = 740f;

        /// <summary>Height above the sea of stinger column i (0 main, 1 north flank, 2 south flank) at end time tg (before the
        /// sink): StingerH0 + the slow rise (the flanks start 2 and 4 s later).</summary>
        public static float StingerHeight(int i, double tg)
        {
            double lag = i == 0 ? 0.0 : (i == 1 ? 4.0 : 2.0);
            return StingerH0[i] + StingerRiseSpeed * StingerRiseSeconds * StingerRise.At(tg - lag);
        }

        static void Stinger(double tg, double t, KrakenShowPose p)
        {
            p.Appearance = "stinger";
            float k = StingerScale;
            p.Scale = k;
            p.Root = StingerCrown;
            p.Heading = 270f;                     // the face (arms 1, 2) towards the dam
            p.HeadTopAboveSea = StingerCrown.y + HeadTop * k - SeaY;
            p.Lurk = 1f; p.Lids = 1f; p.Look = 0f;
            // the columns stand from the moment the end clock starts (KrakenDirector shows them only once the end camera looks
            // below the crest, so they never pop on screen) and sink in the black after the fade
            float e = 1f - Ss(tg, StingerSinkT0, StingerSinkT1);
            p.Visible = e > 0.001f;
            Vector3 side = new Vector3(0f, 0f, 1f);    // the picture plane of the look-back (yaw 90): sway and hooks along z
            var a = p.NewArm();
            a.Limb = 2; a.Weight = 1f;
            Crook(a, new Vector3(95f, SeaY, StingerZ + 40f), StingerHeight(0, tg), e, new Vector3(-0.12f, 0f, 1f).normalized, side, t, 0f, StingerSway);
            a.Underside = new Vector3(-1f, 0f, 0f);
            var b = p.NewArm();
            b.Limb = 3; b.Weight = 1f;
            Crook(b, new Vector3(130f, SeaY, StingerZ + 600f), StingerHeight(1, tg), e, new Vector3(-0.12f, 0f, 1f).normalized, side, t, 2.1f, StingerSway * 0.85f);
            b.Underside = new Vector3(-1f, 0f, 0f);
            var c = p.NewArm();
            c.Limb = 1; c.Weight = 1f;
            Crook(c, new Vector3(120f, SeaY, StingerZ - 560f), StingerHeight(2, tg), e, new Vector3(-0.12f, 0f, -1f).normalized, side, t, 4.0f, StingerSway * 0.9f);
            c.Underside = new Vector3(-1f, 0f, 0f);
        }
    }
}
