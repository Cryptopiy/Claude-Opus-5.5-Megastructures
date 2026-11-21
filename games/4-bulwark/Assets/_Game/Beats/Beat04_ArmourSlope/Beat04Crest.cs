// Beat04ArmourSlope crest band z 704-920 (14.5.4 "Parapet restored from z 704; lamp posts continue at z 750, 800, 850,
// 900"): the same road as beat 2 (Beat02_Crest owns z < 420 and z > 956; its section numbers are repeated here so the two
// bands meet without a seam): deck x -6..6 (BwRoad: lane lines and the chainage numerals 07-09 come from world z), 1 cm lee
// edge chamfer, sea face x 6.0 to y -20 then the 1:8 batter to y -40 (ConcreteWet), lee face x -6.0 to y -1500
// (ConcreteDark), parapet x 5.5-6.0 top 1.10 in chamfered 12 m segments (joints at 6 + 12 k, freeing ports 0.40 m wide
// at 3 and 9 m of every segment), the lee rail (two 0.10 m BwLine tubes, posts 0.12 m every 2 m at odd z), lamp posts
// k 12-15 (z 750 lee, 800 sea, 850 lee, 900 sea), bollards 0.9 m every 20 m on the lee kerb x -5.85 (z 720-900), deck
// expansion joints at z 18 + 24 k. Nothing stands inside the capsule reach of the road lane (x -5.75..5.45).
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Megastructure.Beats
{
    public sealed partial class Beat04ArmourSlope
    {
        public const double BandZ0 = 704.0, BandZ1 = 920.0;
        public static readonly int[] LampK = { 12, 13, 14, 15 };
        public const double LeeKerbX = -5.85;
        public const double ParapetSegment = 12.0, ParapetJointPhase = 6.0;
        public const double ScupperWidth = 0.40, ScupperTop = 0.15;
        public static readonly double[] ScupperOffsets = { 3.0, 9.0 };
        public const double SeaFaceBottomY = -40.0, LeeFaceDepth = 1500.0, LeePostFoot = -0.32;
        public const float Chamfer = 0.02f, LeeEdgeChamfer = 0.01f;
        public const double ExpansionPhase = 18.0, ExpansionPitch = 24.0;

        static readonly double[] s_deckX = { -5.99, -5.0, 0.0, 4.6, 5.1, 5.35, 5.5, 6.0 };
        static readonly byte[] s_deckAo = { 255, 255, 255, 255, 236, 206, 178, 178 };
        static readonly Color32 s_plain = new Color32(255, 0, 0, 255);
        static readonly Color32 s_band = new Color32(255, 0, 255, 255);

        void BuildCrest(WorldContext ctx, Transform root)
        {
            // ---- deck
            var deck = new MeshBuilder();
            int rows = (int)Math.Ceiling((BandZ1 - BandZ0) / 10.0), cols = s_deckX.Length;
            int first = deck.VertexCount;
            for (int r = 0; r <= rows; r++)
            {
                float z = (float)(BandZ0 + (BandZ1 - BandZ0) * r / rows);
                for (int c = 0; c < cols; c++)
                {
                    deck.Color = new Color32(s_deckAo[c], 0, 0, 255);
                    deck.Vertex(new Vector3((float)s_deckX[c], 0f, z), Vector3.up, new Vector2((float)s_deckX[c], z));
                }
            }
            for (int r = 0; r < rows; r++)
                for (int c = 0; c + 1 < cols; c++)
                {
                    int a = first + r * cols + c;
                    deck.Quad(a, a + cols, a + cols + 1, a + 1);
                }
            // lee edge chamfer (-5.99, 0) -> (-6.0, -0.01)
            var nc = new Vector3(-0.70710678f, 0.70710678f, 0f);
            deck.Color = new Color32(255, 255, 0, 255);
            int f2 = deck.VertexCount;
            for (int r = 0; r <= rows; r++)
            {
                float z = (float)(BandZ0 + (BandZ1 - BandZ0) * r / rows);
                deck.Vertex(new Vector3(-6.0f, -LeeEdgeChamfer, z), nc);
                deck.Vertex(new Vector3(-6.0f + LeeEdgeChamfer, 0f, z), nc);
            }
            for (int r = 0; r < rows; r++) { int a = f2 + 2 * r; deck.Quad(a, a + 2, a + 3, a + 1); }
            deck.Color = s_plain;
            Add(ctx, root, deck, "b4_deck", m_road);

            // ---- faces
            var sea = new MeshBuilder();
            var lee = new MeshBuilder();
            double xb = BulwarkConst.SeaFaceX + (BulwarkConst.SeaFaceBatterY - SeaFaceBottomY) / BulwarkConst.SeaFaceBatter;
            WallStrip(sea, BandZ0, BandZ1, BulwarkConst.SeaFaceX, -0.05, BulwarkConst.SeaFaceX, BulwarkConst.SeaFaceBatterY, +1);
            WallStrip(sea, BandZ0, BandZ1, BulwarkConst.SeaFaceX, BulwarkConst.SeaFaceBatterY, xb, SeaFaceBottomY, +1);
            WallStrip(lee, BandZ0, BandZ1, BulwarkConst.LeeFaceX, -LeeEdgeChamfer, BulwarkConst.LeeFaceX, -LeeFaceDepth, -1);
            Add(ctx, root, sea, "b4_sea_face", m_wet);
            Add(ctx, root, lee, "b4_lee_face", m_dark);

            BuildParapet(ctx, root);
            BuildLeeRail(ctx, root);
            BuildLampPosts(ctx, root);
            BuildBollardsAndJoints(ctx, root);
        }

        static void WallStrip(MeshBuilder mb, double z0, double z1, double xTop, double yTop, double xBot, double yBot, int side)
        {
            int rows = Math.Max(1, (int)Math.Ceiling((z1 - z0) / 10.0));
            Vector3 down = new Vector3((float)(xBot - xTop), (float)(yBot - yTop), 0f);
            Vector3 n = Vector3.Cross(Vector3.forward, down).normalized * side;
            if (n.x * side < 0f) n = -n;
            int first = mb.VertexCount;
            for (int r = 0; r <= rows; r++)
            {
                float z = (float)(z0 + (z1 - z0) * r / rows);
                mb.Vertex(new Vector3((float)xTop, (float)yTop, z), n);
                mb.Vertex(new Vector3((float)xBot, (float)yBot, z), n);
            }
            for (int r = 0; r < rows; r++)
            {
                int a = first + 2 * r;
                if (side > 0) mb.Quad(a, a + 2, a + 3, a + 1);
                else mb.Quad(a, a + 1, a + 3, a + 2);
            }
        }

        public static double NextParapetJoint(double za, double z1)
        {
            double k = Math.Floor((za - ParapetJointPhase) / ParapetSegment) + 1.0;
            double zb = ParapetJointPhase + ParapetSegment * k;
            if (zb - za < 3.0) zb += ParapetSegment;
            if (z1 - zb < 3.0) zb = z1;
            return Math.Min(zb, z1);
        }

        void BuildParapet(WorldContext ctx, Transform root)
        {
            var mb = new MeshBuilder();
            float x0 = (float)BulwarkConst.ParapetXMin, x1 = (float)BulwarkConst.ParapetXMax, top = (float)BulwarkConst.ParapetTop;
            const float embed = 0.05f;
            float yWall = (float)ScupperTop, yKick = (float)ScupperTop + 0.02f, hw = 0.5f * (float)ScupperWidth;
            float xc = 0.5f * (x0 + x1);
            double za = BandZ0;
            while (za < BandZ1 - 1e-6)
            {
                double zb = NextParapetJoint(za, BandZ1);
                MeshKit.ChamferBox(mb, new Vector3(xc, 0.5f * (top + yWall), (float)(0.5 * (za + zb))), new Vector3(x1 - x0, top - yWall, (float)(zb - za)), Chamfer);
                double zs = za;
                double g0 = ParapetJointPhase + ParapetSegment * Math.Floor((za - ParapetJointPhase) / ParapetSegment);
                for (double zg = g0; zg < zb; zg += ParapetSegment)
                    foreach (double off in ScupperOffsets)
                    {
                        double zp = zg + off;
                        if (zp < za + 1.0 || zp > zb - 1.0) continue;
                        Kicker(mb, xc, x1 - x0, -embed, yKick, zs, zp - hw);
                        zs = zp + hw;
                    }
                Kicker(mb, xc, x1 - x0, -embed, yKick, zs, zb);
                za = zb;
            }
            Add(ctx, root, mb, "b4_parapet", m_concrete);
        }

        static void Kicker(MeshBuilder mb, float xc, float w, float y0, float y1, double za, double zb)
        {
            if (zb - za < 0.05) return;
            MeshKit.ChamferBox(mb, new Vector3(xc, 0.5f * (y0 + y1), (float)(0.5 * (za + zb))), new Vector3(w, y1 - y0, (float)(zb - za)), Chamfer);
        }

        void BuildLeeRail(WorldContext ctx, Transform root)
        {
            var rails = new MeshBuilder();
            var posts = new MeshBuilder();
            var caps = new MeshBuilder();
            float rr = 0.5f * (float)BulwarkConst.LeeRailDiameter, pr = 0.5f * (float)BulwarkConst.LeeRailPostSize;
            float x = (float)BulwarkConst.LeeRailX;
            float yTop = (float)BulwarkConst.LeeRailTop - rr, yMid = (float)BulwarkConst.LeeRailMid - rr;
            double a0 = BandZ0 + 0.1, a1 = BandZ1 - 0.1;
            for (double a = a0; a < a1 - 1e-6; a += 100.0)
            {
                double e = Math.Min(a1, a + 100.0);
                Tube(rails, new Vector3(x, yTop, (float)a), new Vector3(x, yTop, (float)e), rr);
                Tube(rails, new Vector3(x, yMid, (float)a), new Vector3(x, yMid, (float)e), rr);
            }
            foreach (double ze in new[] { a0, a1 })
            {
                int s = ze == a0 ? -1 : 1;
                MeshKit.Disc(caps, new Vector3(x, yTop, (float)ze), new Vector3(0f, 0f, s), rr, 12);
                MeshKit.Disc(caps, new Vector3(x, yMid, (float)ze), new Vector3(0f, 0f, s), rr, 12);
            }
            for (double z = Math.Ceiling(a0 / BulwarkConst.LeeRailPostPitch) * BulwarkConst.LeeRailPostPitch + 1.0; z < a1; z += BulwarkConst.LeeRailPostPitch)
            {
                float zf = (float)z;
                float yCap = (float)BulwarkConst.LeeRailTop + 0.02f;
                Tube(posts, new Vector3(x, (float)LeePostFoot, zf), new Vector3(x, yCap, zf), pr, 10);
                MeshKit.Disc(caps, new Vector3(x, yCap, zf), Vector3.up, pr + 0.004f, 12);
                MeshKit.Disc(caps, new Vector3(x, (float)LeePostFoot, zf), Vector3.down, pr, 10);
                MeshKit.ChamferBox(caps, new Vector3(x + 0.08f, 0.0075f, zf), new Vector3(0.16f, 0.015f, 0.2f), 0.004f);
            }
            Add(ctx, root, rails, "b4_rails", m_rail10, false);
            Add(ctx, root, posts, "b4_rail_posts", m_post12, false);
            Add(ctx, root, caps, "b4_rail_caps", m_steel);
        }

        /// <summary>Round tube a -> b on radius r (BwLine _LINE_TUBE contract): smooth outward normals, a ring every 4 m.</summary>
        static void Tube(MeshBuilder mb, Vector3 a, Vector3 b, float radius, int sides = 8)
        {
            Vector3 d = b - a;
            float len = d.magnitude;
            if (len < 1e-4f || radius <= 0f) return;
            Vector3 dir = d / len;
            Vector3 h = Mathf.Abs(dir.y) < 0.9f ? Vector3.up : Vector3.right;
            Vector3 u = Vector3.Cross(h, dir).normalized, v = Vector3.Cross(dir, u);
            int rings = Mathf.Max(2, Mathf.CeilToInt(len / 4f) + 1);
            int first = mb.VertexCount;
            for (int i = 0; i < rings; i++)
            {
                Vector3 c = a + d * (i / (float)(rings - 1));
                for (int j = 0; j < sides; j++)
                {
                    float ph = 2f * Mathf.PI * j / sides;
                    Vector3 n = u * Mathf.Cos(ph) + v * Mathf.Sin(ph);
                    mb.Vertex(c + n * radius, n);
                }
            }
            for (int i = 0; i + 1 < rings; i++)
                for (int j = 0; j < sides; j++)
                {
                    int jn = (j + 1) % sides;
                    int A = first + i * sides + j, D = first + i * sides + jn, C = first + (i + 1) * sides + jn, B = first + (i + 1) * sides + j;
                    mb.Quad(A, D, C, B);
                }
        }

        public static void LampBase(int k, out double x, out double yBase, out double z, out bool lee)
        {
            z = BulwarkConst.LampPostZ0 + BulwarkConst.LampPostPitch * k;
            lee = (k & 1) == 0;
            x = lee ? LeeKerbX : BulwarkConst.LampPostSeaX;
            yBase = lee ? 0.0 : BulwarkConst.ParapetTop;
        }

        void BuildLampPosts(WorldContext ctx, Transform root)
        {
            const float PoleR0 = 0.095f, PoleR1 = 0.06f, ArmReach = 1.4f;
            var mb = new MeshBuilder();
            foreach (int k in LampK)
            {
                LampBase(k, out double xd, out double yb, out double zd, out bool lee);
                var b = new Vector3((float)xd, (float)yb, (float)zd);
                float h = (float)BulwarkConst.LampPostHeight;
                float dir = lee ? 1f : -1f, back = -dir;
                mb.Color = s_plain;
                MeshKit.ChamferBox(mb, b + new Vector3(0f, 0.0125f, 0f), new Vector3(0.20f, 0.025f, 0.32f), 0.005f);
                MeshKit.TaperedCylinder(mb, b + new Vector3(0f, 0.02f, 0f), b + new Vector3(0f, 0.6f, 0f), PoleR0, PoleR0 * 0.92f, 20, false);
                MeshKit.ChamferBox(mb, b + new Vector3(back * (PoleR0 - 0.005f), 0.35f, 0f), new Vector3(0.012f, 0.32f, 0.10f), 0.003f);
                MeshKit.TaperedCylinder(mb, b + new Vector3(0f, 0.6f, 0f), b + new Vector3(0f, h - 0.40f, 0f), PoleR0 * 0.92f, PoleR1, 16, false);
                mb.Color = s_band;
                MeshKit.TaperedCylinder(mb, b + new Vector3(0f, 2.5f, 0f), b + new Vector3(0f, 2.75f, 0f), PoleR0 * 0.88f + 0.002f, PoleR0 * 0.87f + 0.002f, 16, false);
                mb.Color = s_plain;
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
                Vector3 hc = b + new Vector3(dir * (ArmReach - 0.05f), h - 0.065f, 0f);
                MeshKit.ChamferBox(mb, hc, new Vector3(0.75f, 0.13f, 0.30f), 0.02f);
                MeshKit.ChamferBox(mb, hc + new Vector3(0f, -0.075f, 0f), new Vector3(0.6f, 0.03f, 0.22f), 0.008f);
                MeshKit.Disc(mb, b + new Vector3(0f, h - 0.40f, 0f), Vector3.down, PoleR1, 12);
            }
            Add(ctx, root, mb, "b4_lamp_posts", m_steel);
        }

        public static List<double> BollardZ()
        {
            var zs = new List<double>();
            for (double z = 140.0; z <= BulwarkConst.TowerZ0 - 10.0; z += BulwarkConst.BollardPitch)
                if (z > BandZ0 + 1.0) zs.Add(z);
            return zs;
        }

        public static List<double> ExpansionJointZ()
        {
            var zs = new List<double>();
            double k0 = Math.Ceiling((BandZ0 + 0.5 - ExpansionPhase) / ExpansionPitch);
            for (double z = ExpansionPhase + ExpansionPitch * k0; z < BandZ1 - 0.5; z += ExpansionPitch) zs.Add(z);
            return zs;
        }

        void BuildBollardsAndJoints(WorldContext ctx, Transform root)
        {
            var mb = new MeshBuilder();
            const float r = 0.095f;
            float h = (float)BulwarkConst.BollardHeight;
            foreach (double z in BollardZ())
            {
                var b = new Vector3((float)LeeKerbX, 0f, (float)z);
                mb.Color = s_plain;
                MeshKit.ChamferBox(mb, b + new Vector3(0f, 0.01f, 0f), new Vector3(0.20f, 0.02f, 0.26f), 0.004f);
                MeshKit.TaperedCylinder(mb, b + new Vector3(0f, 0.015f, 0f), b + new Vector3(0f, 0.66f, 0f), r, r * 0.97f, 20, false);
                mb.Color = s_band;
                MeshKit.TaperedCylinder(mb, b + new Vector3(0f, 0.66f, 0f), b + new Vector3(0f, 0.78f, 0f), r * 0.97f, r * 0.965f, 20, false);
                mb.Color = s_plain;
                MeshKit.TaperedCylinder(mb, b + new Vector3(0f, 0.78f, 0f), b + new Vector3(0f, h - 0.03f, 0f), r * 0.965f, r * 0.95f, 20, false);
                MeshKit.Sphere(mb, b + new Vector3(0f, h - 0.03f, 0f), r * 0.95f, 16, 8);
            }
            Add(ctx, root, mb, "b4_bollards", m_steel);

            var plates = new MeshBuilder();
            var seals = new MeshBuilder();
            const float PlateW = 0.14f, SealW = 0.04f, X0 = -5.90f, X1 = 5.49f;
            float xc = 0.5f * (X0 + X1), w = X1 - X0, off = 0.5f * (SealW + PlateW);
            foreach (double zd in ExpansionJointZ())
            {
                float z = (float)zd;
                plates.Color = s_plain;
                MeshKit.ChamferBox(plates, new Vector3(xc, 0f, z - off), new Vector3(w, 0.012f, PlateW), 0.003f);
                MeshKit.ChamferBox(plates, new Vector3(xc, 0f, z + off), new Vector3(w, 0.012f, PlateW), 0.003f);
                MeshKit.ChamferBox(seals, new Vector3(xc, -0.002f, z), new Vector3(w - 0.02f, 0.008f, SealW + 0.004f), 0.002f);
            }
            Add(ctx, root, plates, "b4_expansion_plates", m_steel);
            Add(ctx, root, seals, "b4_expansion_seals", m_dark);
        }
    }
}
