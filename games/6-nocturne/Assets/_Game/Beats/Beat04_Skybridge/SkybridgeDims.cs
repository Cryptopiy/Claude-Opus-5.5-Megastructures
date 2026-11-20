// SkybridgeDims: every dimension of beat 4 (MASTER_PROMPT 14.5.4, 14.4, 14.12 "Bridge") as named constants, in metres, world
// coordinates (+X east along the bridge, +Z north, y 0 = the walk plane). Deviations from the text are marked "G3:" and logged
// in docs/logs/G3.md (proposed decisions in the final report). Shared by the builders of this folder and the EditMode tests.
namespace Megastructure
{
    public static class SkybridgeDims
    {
        // ---- deck (14.5.4: deck x 375.3-695.3, 5.0 m between rails, 0.5 m steel slab, stud line every 4 m)
        public const float AxisZ = 195f;
        public const float DeckX0 = 375.3f;            // s 560: tongue A tip (G2's tongue deck ends here, same width 5.0, same top y 0)
        public const float DeckX1 = 695.0f;            // G3: ends at gate B's east face = terrace west edge (spec 695.3; no overlap with the terrace)
        public const float DeckHalfWidth = 2.5f;       // 5.0 m between the rails
        public const float DeckThickness = 0.5f;
        public const float SeamPitch = 4f;             // stud line every 4 m (graybox: a smooth bevelled seam)
        public const float SeamWidth = 0.20f, SeamDepth = 0.012f;
        public const float PathSeamHalf = 1.5f;        // = GardenDims.PathHalfWidth: the deck top has vertex lines at z 195 +- 1.5 too
        /// <summary>The glass strip of 14.5.4 (s 706-726; GE, D-029 GB-05): a hole x 521.3-541.3, |z - 195| <= 1.2 in the deck under a
        /// NocGlass pane 3 cm below the top (PathGuide's floor collider at y -0.05 carries the walker); between seams (521.3 and 541.3
        /// lie 2 m from the seams at 519.3 / 523.3 and 539.3 / 543.3) and between the truss planes (z 193 / 197).</summary>
        public const float StripX0 = 521.3f, StripX1 = 541.3f, StripHalf = 1.2f, StripGlassY = -0.03f;

        // ---- span and truss (14.5.4: Warren truss beneath, 10 m deep; 14.4: neck x 375.3..675.3 = 300 m)
        public const float SpanX0 = 375.3f, SpanX1 = 675.3f;   // truss bearings (span 300.0)
        public const float TrussDepth = 10f;
        public const int TrussBays = 25;                        // 12 m bays
        public const float TrussMember = 0.5f;                  // I-section chords and diagonals
        public const float TrussPlaneOffset = 2.0f;             // two truss planes at z 195 +- 2.0 (flanges inside the 2.5 deck edge)
        public const float TrussTopY = -0.76f;                  // top chord axis: flange top 1 cm under the deck soffit (-0.5)
        public const float BraceSize = 0.3f;
        public const float OutriggerPitch = 24f;                // outriggers on the top nodes x 375.3 + 24 k, k = 1..12
        public const float OutriggerY = -3.0f, OutriggerZ0 = 176.5f, OutriggerZ1 = 213.5f;
        public const float SaddleTop = -2.52f;                  // 2 cm under the tube bottom (axis 2.0 - r 4.5 = -2.5)
        // bearing piers under the gate blocks (concrete): A carries the truss end inside gate A, B the tongue B tip x 675.3 .. 695
        public const float PierAX0 = SpanX0, PierAX1 = 384f, PierAZ0 = 191f, PierAZ1 = 199f, PierABottom = -12f;
        public const float PierX0 = SpanX1, PierX1 = 695f, PierZ0 = 190f, PierZ1 = 200f, PierBottom = -14f;

        // ---- train tubes (14.5.4: tubes N (z 207) and S (z 183), r 4.5, axis y 2.0, x 384-683, NocGlass; two tracks +-0.8)
        public const float TubeR = 4.5f, TubeAxisY = 2.0f, TubeNZ = 207f, TubeSZ = 183f, TubeX0 = 384f, TubeX1 = 683f;
        public const float TrackRailOffset = 0.8f, TrackRailD = 0.07f;   // rails at the tube axis +- 0.8 (14.5.4 "axis +-0.8 m")
        public const int TubeSegments = 48;

        // ---- face-to-face tubes (14.5.4, 14.12: upper y +118 z 195, lower y -142 z 209, r 5, x 215.3-815.3; G2 makes the portals)
        public const float FaceAX = 215.3f, FaceBX = 815.3f, FaceTubeR = 5f;
        public const float UpperY = 118f, UpperZ = 195f, LowerY = -142f, LowerZ = 209f;

        // ---- gate blocks (14.5.4: A x 372-384, B x 683-695, z 175-215, 10 m high, walkway opening z 189-201 x 8 m, tube mouths)
        public const float GateAX0 = 372f, GateAX1 = 384f, GateBX0 = 683f, GateBX1 = 695f;
        // G3: 10 m above the deck; below it the block reaches only 3.5 m (around the tube mouths, bottom -2.5): a block down to the
        // truss bottom filled the right half of KS10 (14.5.3: drop 43 %) from 9 m away; narrow piers carry the truss ends instead
        public const float GateZ0 = 175f, GateZ1 = 215f, GateTop = 10f, GateBottom = -3.5f;
        public const float OpeningZ0 = 189f, OpeningZ1 = 201f, OpeningTop = 8f;
        public const float TrenchY = -0.5f;            // gate A passage floor beside the 5 m deck (lane 2.15 there)
        public const float TunnelDepth = 6f;           // blind tube tunnels behind the mouths (trains are clipped there, Phase 4)
        public const int MouthSegments = 32;           // divisible by 8: the square-ring corners fall on samples

        // ---- gate frames (14.5.4: pylons 3 m, lintel 2 m, 16 m high, at x 372-376 and 683-689)
        public const float FrameAX0 = 371.7f, FrameAX1 = 376f, FrameBX0 = 682.7f, FrameBX1 = 689f;   // 0.3 m proud of the block face
        public const float FrameTop = 16f, LintelBottom = 14f, FrameBottom = -3.8f;
        public const float PylonWidth = 1.4f;          // G3: 3 m pylons beside the 12 m opening would cover the tube mouths (6 + 3 > 12 - 4.5)
        public const float PylonGap = 0.05f;           // pylon inner face 5 cm behind the passage wall (no co-planar faces)

        // ---- rails (14.5.4: rails 1.10 m, posts 0.05 m every 2 m, top rail 0.06, two rods 0.03; NocLine)
        public const float RailX0 = 384f, RailX1 = 683f, RailEmbed = 0.05f;
        public const float RailHeight = 1.10f, PostPitch = 2f, PostX0 = 384.5f;
        public const float PostD = 0.05f, TopRailD = 0.06f, RodD = 0.03f;
        public const float RodY0 = 0.40f, RodY1 = 0.75f, PostBottom = -0.40f;

        // ---- lamps (14.5.4: 21 lamp posts, SodiumAmber, 4.2 m, every 30 m; 14.2.3 row 4: r 12 i 4)
        public const int LampCount = 21;
        public const float LampX0 = 385.5f, LampPitch = 14.8f;  // G3: alternate sides, 29.6 m per side, inside the 299 m between gates
        public const float LampHeight = 4.2f, LampPoleD = 0.12f, LampArmD = 0.08f, LampLateral = 2.80f, LampHeadLateral = 1.85f;
        public const float LampRange = 12f, LampIntensity = 4f, LampHeadEmission = 2.0f, LampWetRho = 0.35f;

        /// <summary>Progress s of a point on the east leg x (14.12: x = s - 184.70).</summary>
        public static double SOfX(double x) => x + NocturneConstants.EastLegOffset;

        /// <summary>Top node x of truss bay boundary i (0..TrussBays).</summary>
        public static float TrussNodeX(int i) => SpanX0 + (SpanX1 - SpanX0) * i / TrussBays;

        /// <summary>Lamp k (0..20): x and the side (+1 south = right of the walk, -1 north).</summary>
        public static float LampX(int k) => LampX0 + LampPitch * k;
        public static int LampSide(int k) => (k & 1) == 0 ? 1 : -1;
    }
}
