// Beat02Crest props (14.5.2 geometry, 14.2.7 scale cues: rails 1.1, lamp posts 9, digits 1.2, cart, bollards 0.9, far
// towers 80; lifebuoy stations 0.76 m on the parapet as an extra human-scale cue). Materials: BwSteel (SteelPaint, ochre
// bands by vertex colour B = 255), BwSteel SafetyOchre for the cart body and the lifebuoys, BwConcrete for the far towers.
// Every prop near the lane keeps outside its capsule reach (x -5.75 .. 5.45) except the cart (a PathGuide obstacle).
using System;
using Megastructure.Player;
using UnityEngine;

namespace Megastructure.Beats
{
    public sealed partial class Beat02Crest
    {
        /// <summary>x of the lee lamp posts and the bollards (Wave A decision: inside the capsule reach at the spec's -5.6).</summary>
        public const double LeeKerbX = -5.85;
        /// <summary>Lamp posts of this builder (k 0-5 at z 150 + 50 k; k 12-15 stand in beat 4's band).</summary>
        public static readonly int[] LampK = { 0, 1, 2, 3, 4, 5 };
        /// <summary>Lamp pole: base diameter 0.19 m (fits x -5.945..-5.755), top 0.12 m, outreach arm 1.4 m.</summary>
        public const float PoleR0 = 0.095f, PoleR1 = 0.06f, ArmReach = 1.4f;
        /// <summary>Lifebuoy stations on the parapet top (z), ring 0.76 m in the plane of the parapet: each in the middle of a
        /// 12 m parapet segment (joints at 6 + 12 k). The third station stands at z 360, not 384: from the K2 hold (Addendum A1
        /// 1.6/1.7, eye (-1.5, 1.7, s 364-392), gaze yaw 22) a ring at 384 crossed the arm's bearing 46.7 deg at s 378 and hid
        /// its waterline (ring 1.1-2.0 m high, 9.4 m away: -3.6..+1.8 deg vs the waterline at -1.2 deg, capture seax-02k k2t);
        /// at 360 it crosses that bearing at s 353, before the arm rises (s 356), and is behind the walker during the hold.</summary>
        public static readonly double[] LifebuoyZ = { 192.0, 288.0, 360.0 };
        public const float LifebuoyR = 0.33f, LifebuoyTube = 0.05f;

        static readonly Color32 s_plain = new Color32(255, 0, 0, 255);
        static readonly Color32 s_band = new Color32(255, 0, 255, 255);   // B = 255: BwSteel ochre band

        // ================================================================ lamp posts (9.0 m, every 50 m, alternate sides)
        /// <summary>Base (x, y) of lamp post k: lee side (even k) at the kerb on the deck, sea side (odd k) on the parapet top.</summary>
        public static void LampBase(int k, out double x, out double yBase, out double z, out bool lee)
        {
            z = BulwarkConst.LampPostZ0 + BulwarkConst.LampPostPitch * k;
            lee = (k & 1) == 0;
            x = lee ? LeeKerbX : BulwarkConst.LampPostSeaX;
            yBase = lee ? 0.0 : BulwarkConst.ParapetTop;
        }

        void BuildLampPosts(WorldContext ctx, Transform root)
        {
            var mb = new MeshBuilder();
            foreach (int k in LampK)
            {
                LampBase(k, out double xd, out double yb, out double zd, out bool lee);
                var b = new Vector3((float)xd, (float)yb, (float)zd);
                float h = (float)BulwarkConst.LampPostHeight;
                float dir = lee ? 1f : -1f;                                   // the arm reaches over the road
                float back = -dir;                                            // the hatch faces away from the lane
                mb.Color = s_plain;
                // base plate and a flared foot (bolted), door hatch on the road side, the tapered pole
                MeshKit.ChamferBox(mb, b + new Vector3(0f, 0.0125f, 0f), new Vector3(0.20f, 0.025f, 0.32f), 0.005f);
                MeshKit.TaperedCylinder(mb, b + new Vector3(0f, 0.02f, 0f), b + new Vector3(0f, 0.6f, 0f), PoleR0, PoleR0 * 0.92f, 20, false);
                MeshKit.ChamferBox(mb, b + new Vector3(back * (PoleR0 - 0.005f), 0.35f, 0f), new Vector3(0.012f, 0.32f, 0.10f), 0.003f);
                MeshKit.TaperedCylinder(mb, b + new Vector3(0f, 0.6f, 0f), b + new Vector3(0f, h - 0.40f, 0f), PoleR0 * 0.92f, PoleR1, 16, false);
                // a band at 2.5-2.75 m (ochre, B = 255) as a scale cue on every pole
                mb.Color = s_band;
                MeshKit.TaperedCylinder(mb, b + new Vector3(0f, 2.5f, 0f), b + new Vector3(0f, 2.75f, 0f), PoleR0 * 0.88f + 0.002f, PoleR0 * 0.87f + 0.002f, 16, false);
                mb.Color = s_plain;
                // the bend into the arm (three straight segments) and the arm; the luminaire housing's top is the post's
                // highest point, exactly 9.0 m above its base (14.5.2 "lamp posts 9 m")
                Vector3 p0 = b + new Vector3(0f, h - 0.40f, 0f);
                Vector3 p1 = b + new Vector3(dir * 0.10f, h - 0.22f, 0f);
                Vector3 p2 = b + new Vector3(dir * 0.30f, h - 0.14f, 0f);
                Vector3 p3 = b + new Vector3(dir * (ArmReach - 0.35f), h - 0.10f, 0f);
                MeshKit.TaperedCylinder(mb, p0, p1, PoleR1, 0.05f, 12, false);
                MeshKit.TaperedCylinder(mb, p1, p2, 0.05f, 0.045f, 12, false);
                MeshKit.TaperedCylinder(mb, p2, p3, 0.045f, 0.04f, 12, true);
                MeshKit.Sphere(mb, p0, PoleR1, 12, 6);
                MeshKit.Sphere(mb, p1, 0.05f, 12, 6);
                MeshKit.Sphere(mb, p2, 0.045f, 12, 6);
                // luminaire: a shallow housing 0.75 x 0.13 x 0.30, its lens face down (dark glass of the same steel), unlit in
                // the storm daylight (beat 2 is grey: 14.2 "colour tells the time of day")
                Vector3 hc = b + new Vector3(dir * (ArmReach - 0.05f), h - 0.065f, 0f);
                MeshKit.ChamferBox(mb, hc, new Vector3(0.75f, 0.13f, 0.30f), 0.02f);
                MeshKit.ChamferBox(mb, hc + new Vector3(0f, -0.075f, 0f), new Vector3(0.6f, 0.03f, 0.22f), 0.008f);
                // top cap of the pole
                MeshKit.Disc(mb, b + new Vector3(0f, h - 0.40f, 0f), Vector3.down, PoleR1, 12);
            }
            Add(ctx, root, mb, "crest_lamp_posts", m_steel);
        }

        // ================================================================ deck expansion joints (every 24 m)
        /// <summary>Deck expansion joints at the monolith joints, z = 18 + 24 k (mid-way between parapet joints, clear of the
        /// lamp posts at z 150 + 50 k, at least 2 m from every bollard (140 + 20 i) and at even z, between the rail posts):
        /// two 0.14 m steel cover plates either side of a 0.04 m dark neoprene seal, across the deck from the lee edge to the
        /// parapet foot (x -5.90 .. 5.49), 6 mm proud and half embedded (no co-planar faces). A walking-rhythm scale cue.</summary>
        public const double ExpansionPhase = 18.0, ExpansionPitch = 24.0;
        public const float ExpansionPlateW = 0.14f, ExpansionSealW = 0.04f, ExpansionX0 = -5.90f, ExpansionX1 = 5.49f;

        /// <summary>The z of every expansion joint of this builder's bands (none under the beat-1 shell).</summary>
        public static System.Collections.Generic.List<double> ExpansionJointZ()
        {
            var zs = new System.Collections.Generic.List<double>();
            for (int b = 0; b < 2; b++)
            {
                double k0 = Math.Ceiling((BandZ0[b] + 0.5 - ExpansionPhase) / ExpansionPitch);
                for (double z = ExpansionPhase + ExpansionPitch * k0; z < BandZ1[b] - 0.5; z += ExpansionPitch)
                {
                    if (z > BulwarkConst.ShellZMin - 0.5 && z < BulwarkConst.ShellZMax + 0.5) continue;
                    zs.Add(z);
                }
            }
            return zs;
        }

        void BuildExpansionJoints(WorldContext ctx, Transform root)
        {
            var plates = new MeshBuilder();
            var seals = new MeshBuilder();
            float xc = 0.5f * (ExpansionX0 + ExpansionX1), w = ExpansionX1 - ExpansionX0;
            float off = 0.5f * (ExpansionSealW + ExpansionPlateW);
            foreach (double zd in ExpansionJointZ())
            {
                float z = (float)zd;
                plates.Color = s_plain;
                MeshKit.ChamferBox(plates, new Vector3(xc, 0f, z - off), new Vector3(w, 0.012f, ExpansionPlateW), 0.003f);
                MeshKit.ChamferBox(plates, new Vector3(xc, 0f, z + off), new Vector3(w, 0.012f, ExpansionPlateW), 0.003f);
                MeshKit.ChamferBox(seals, new Vector3(xc, -0.002f, z), new Vector3(w - 0.02f, 0.008f, ExpansionSealW + 0.004f), 0.002f);
            }
            Add(ctx, root, plates, "crest_expansion_plates", m_steel);
            Add(ctx, root, seals, "crest_expansion_seals", m_dark);
        }

        // ================================================================ bollards (0.9 m every 20 m on the lee kerb)
        public static double BollardZ(int i) => 140.0 + BulwarkConst.BollardPitch * i;
        public static int BollardCount => (int)Math.Floor((BandZ1[0] - 1.0 - 140.0) / BulwarkConst.BollardPitch) + 1;

        void BuildBollards(WorldContext ctx, Transform root)
        {
            var mb = new MeshBuilder();
            const float r = 0.095f;   // 0.19 m: x -5.945..-5.755, clear of the capsule reach (-5.75)
            float h = (float)BulwarkConst.BollardHeight;
            for (int i = 0; i < BollardCount; i++)
            {
                double z = BollardZ(i);
                if (z >= BandZ1[0] - 1.0) break;
                var b = new Vector3((float)LeeKerbX, 0f, (float)z);
                mb.Color = s_plain;
                MeshKit.ChamferBox(mb, b + new Vector3(0f, 0.01f, 0f), new Vector3(0.20f, 0.02f, 0.26f), 0.004f);
                MeshKit.TaperedCylinder(mb, b + new Vector3(0f, 0.015f, 0f), b + new Vector3(0f, 0.66f, 0f), r, r * 0.97f, 20, false);
                mb.Color = s_band;
                MeshKit.TaperedCylinder(mb, b + new Vector3(0f, 0.66f, 0f), b + new Vector3(0f, 0.78f, 0f), r * 0.97f, r * 0.965f, 20, false);
                mb.Color = s_plain;
                MeshKit.TaperedCylinder(mb, b + new Vector3(0f, 0.78f, 0f), b + new Vector3(0f, h - r * 0.6f, 0f), r * 0.965f, r * 0.96f, 20, false);
                // domed top: a squashed half sphere
                Dome(mb, b + new Vector3(0f, h - r * 0.6f, 0f), r * 0.96f, r * 0.6f, 20, 5);
            }
            Add(ctx, root, mb, "crest_bollards", m_steel);
        }

        /// <summary>Upper half of an ellipsoid (radius r, height hgt) on centre c, smooth, facing out.</summary>
        static void Dome(MeshBuilder mb, Vector3 c, float r, float hgt, int lon, int lat)
        {
            int first = mb.VertexCount;
            for (int i = 0; i <= lat; i++)
            {
                float th = 0.5f * Mathf.PI * i / lat;          // 0 at the rim, pi/2 at the top
                float ct = Mathf.Cos(th), st = Mathf.Sin(th);
                for (int k = 0; k <= lon; k++)
                {
                    float ph = 2f * Mathf.PI * k / lon;
                    var p = new Vector3(r * ct * Mathf.Cos(ph), hgt * st, r * ct * Mathf.Sin(ph));
                    var n = new Vector3(ct * Mathf.Cos(ph) / r, st / hgt, ct * Mathf.Sin(ph) / r).normalized;
                    mb.Vertex(c + p, n);
                }
            }
            int row = lon + 1;
            for (int i = 0; i < lat; i++)
                for (int k = 0; k < lon; k++)
                {
                    int a = first + i * row + k;
                    MeshKit.Quad(mb, a, a + row, a + row + 1, a + 1);   // oriented by the vertex normals (outward)
                }
        }

        // ================================================================ lifebuoy stations on the parapet top
        void BuildLifebuoys(WorldContext ctx, Transform root)
        {
            var ring = new MeshBuilder();
            var holder = new MeshBuilder();
            float x = (float)(0.5 * (BulwarkConst.ParapetXMin + BulwarkConst.ParapetXMax));
            float top = (float)BulwarkConst.ParapetTop;
            foreach (double zd in LifebuoyZ)
            {
                float z = (float)zd;
                // holder: a 60 mm post on a plate, a cross-arm the ring hangs on
                MeshKit.ChamferBox(holder, new Vector3(x, top + 0.01f, z), new Vector3(0.16f, 0.02f, 0.16f), 0.004f);
                MeshKit.TaperedCylinder(holder, new Vector3(x, top + 0.02f, z), new Vector3(x, top + 1.05f, z), 0.03f, 0.03f, 12, true);
                MeshKit.ChamferBox(holder, new Vector3(x, top + 0.98f, z), new Vector3(0.05f, 0.04f, 0.30f), 0.005f);
                // the ring in the plane of the parapet (axis along x), centred 0.55 m above the top
                MeshKit.Torus(ring, new Vector3(x, top + 0.55f, z), Vector3.right, LifebuoyR, LifebuoyTube, 28, 10);
            }
            Add(ctx, root, holder, "crest_lifebuoy_holders", m_steel);
            Add(ctx, root, ring, "crest_lifebuoys", m_ochre);
        }

        // ================================================================ the cart (2.8 x 1.3 x 1.9 at (2.0, 0, 330))
        /// <summary>World bounds of the cart (14.5.2, 14.12): x 1.35..2.65, y 0..1.9, z 328.6..331.4.</summary>
        public static Bounds CartBounds => new Bounds(
            new Vector3((float)BulwarkConst.CartX, (float)(0.5 * BulwarkConst.CartH), (float)BulwarkConst.CartZ),
            new Vector3((float)BulwarkConst.CartW, (float)BulwarkConst.CartH, (float)BulwarkConst.CartL));

        void BuildCart(WorldContext ctx, Transform root)
        {
            var body = new MeshBuilder();
            var frame = new MeshBuilder();
            float cx = (float)BulwarkConst.CartX, cz = (float)BulwarkConst.CartZ;
            float W = (float)BulwarkConst.CartW, L = (float)BulwarkConst.CartL, H = (float)BulwarkConst.CartH;
            // a maintenance trailer: closed box body 2.0 m long on a steel chassis, A-frame drawbar to the south (0.8 m), two
            // axles of 0.6 m steel wheels, a roof rim and a beacon housing; its envelope is exactly the spec box
            float bodyZ0 = cz - 0.5f * L + 0.8f, bodyZ1 = cz + 0.5f * L - 0.04f;   // rear handles stay inside the 2.8 m
            float bodyY0 = 0.62f, bodyY1 = H - 0.08f;
            float halfW = 0.5f * W;
            // body (ochre) with ribs every 0.5 m on the sides (steel)
            MeshKit.ChamferBox(body, new Vector3(cx, 0.5f * (bodyY0 + bodyY1), 0.5f * (bodyZ0 + bodyZ1)), new Vector3(W - 0.06f, bodyY1 - bodyY0, bodyZ1 - bodyZ0 - 0.04f), 0.025f);
            MeshKit.ChamferBox(body, new Vector3(cx, bodyY1 + 0.03f, 0.5f * (bodyZ0 + bodyZ1)), new Vector3(W - 0.02f, 0.06f, bodyZ1 - bodyZ0), 0.015f);
            for (float rz = bodyZ0 + 0.25f; rz < bodyZ1 - 0.1f; rz += 0.5f)
                for (int s = -1; s <= 1; s += 2)
                    MeshKit.ChamferBox(frame, new Vector3(cx + s * (halfW - 0.02f), 0.5f * (bodyY0 + bodyY1), rz), new Vector3(0.04f, bodyY1 - bodyY0 - 0.04f, 0.05f), 0.008f);
            // rear doors seam and handles (steel)
            MeshKit.ChamferBox(frame, new Vector3(cx, 0.5f * (bodyY0 + bodyY1), bodyZ1 + 0.005f), new Vector3(0.03f, bodyY1 - bodyY0 - 0.1f, 0.02f), 0.006f);
            MeshKit.ChamferBox(frame, new Vector3(cx + 0.12f, 1.1f, bodyZ1 + 0.02f), new Vector3(0.03f, 0.3f, 0.03f), 0.008f);
            // chassis: two longitudinal members, cross members, mudguards
            for (int s = -1; s <= 1; s += 2)
            {
                MeshKit.ChamferBox(frame, new Vector3(cx + s * 0.42f, 0.55f, 0.5f * (bodyZ0 + bodyZ1)), new Vector3(0.08f, 0.12f, bodyZ1 - bodyZ0), 0.01f);
                for (int a = -1; a <= 1; a += 2)
                    MeshKit.ChamferBox(frame, new Vector3(cx + s * (halfW - 0.09f), 0.66f, cz + 0.4f + a * 0.45f), new Vector3(0.16f, 0.03f, 0.62f), 0.008f);
            }
            for (float rz = bodyZ0 + 0.1f; rz <= bodyZ1 - 0.1f; rz += 0.6f)
                MeshKit.ChamferBox(frame, new Vector3(cx, 0.55f, rz), new Vector3(0.84f, 0.08f, 0.08f), 0.01f);
            // A-frame drawbar to the coupling, and the jockey wheel post
            float zTip = cz - 0.5f * L + 0.06f;
            var tip = new Vector3(cx, 0.5f, zTip);
            for (int s = -1; s <= 1; s += 2)
                MeshKit.ChamferMember(frame, new Vector3(cx + s * 0.42f, 0.55f, bodyZ0), tip, 0.07f, 0.09f, 0.01f, Vector3.up);
            MeshKit.ChamferBox(frame, tip + new Vector3(0f, 0.02f, 0.03f), new Vector3(0.12f, 0.1f, 0.14f), 0.015f);
            MeshKit.TaperedCylinder(frame, new Vector3(cx + 0.1f, 0.05f, zTip + 0.35f), new Vector3(cx + 0.1f, 0.75f, zTip + 0.35f), 0.03f, 0.03f, 10, true);
            MeshKit.Torus(frame, new Vector3(cx + 0.1f, 0.08f, zTip + 0.35f), Vector3.right, 0.06f, 0.02f, 14, 6);
            // two axles, four wheels (r 0.30, 0.12 wide): steel rims and solid tyres
            for (int a = -1; a <= 1; a += 2)
            {
                float az = cz + 0.4f + a * 0.45f;
                MeshKit.TaperedCylinder(frame, new Vector3(cx - halfW + 0.1f, 0.3f, az), new Vector3(cx + halfW - 0.1f, 0.3f, az), 0.035f, 0.035f, 10, false);
                for (int s = -1; s <= 1; s += 2)
                {
                    float wx = cx + s * (halfW - 0.08f);
                    MeshKit.TaperedCylinder(frame, new Vector3(wx - 0.06f, 0.3f, az), new Vector3(wx + 0.06f, 0.3f, az), 0.30f, 0.30f, 28, true);
                    MeshKit.TaperedCylinder(frame, new Vector3(wx + s * 0.06f, 0.3f, az), new Vector3(wx + s * 0.072f, 0.3f, az), 0.13f, 0.10f, 16, true);
                }
            }
            // beacon housing on the roof (ochre dome) and a step at the rear
            Dome(body, new Vector3(cx - 0.35f, bodyY1 + 0.06f, bodyZ1 - 0.25f), 0.07f, 0.02f, 12, 4);
            MeshKit.ChamferBox(frame, new Vector3(cx, 0.42f, bodyZ1 - 0.05f), new Vector3(0.6f, 0.04f, 0.18f), 0.01f);
            Add(ctx, root, body, "crest_cart_body", m_ochre);
            Add(ctx, root, frame, "crest_cart_frame", m_steel);
            // a manual player cannot walk through it (the cinematic path passes at x -1.5, 2.85 m west of its side)
            PathGuide.AddObstacle(CartBounds, "crest_cart");
        }

        // ================================================================ far gate towers (36 x 12 x 80, 12 triangles each)
        void BuildFarTowers(WorldContext ctx, Transform root)
        {
            var mb = new MeshBuilder();
            foreach (double z in BulwarkConst.FarTowerZ)
            {
                var c = new Vector3(0f, (float)(0.5 * BulwarkConst.FarTowerH), (float)(z + 0.5 * BulwarkConst.FarTowerW));
                mb.Box(c, new Vector3((float)BulwarkConst.FarTowerD, (float)BulwarkConst.FarTowerH, (float)BulwarkConst.FarTowerW));
            }
            Add(ctx, root, mb, "crest_far_towers", m_concrete);
        }
    }
}
