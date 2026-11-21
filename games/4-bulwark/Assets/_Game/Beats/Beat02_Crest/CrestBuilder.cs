// Beat02Crest: the crest of the dam (MASTER_PROMPT 14.5.2, MONEY SHOT 1) and the exterior sea of beats 1-7 (14.7 H1).
// Owner: the sea owner (Wave B). Replaces the graybox parts "beat2" (every crest piece in the bands z < 420 and z > 956,
// the far gate towers and the cart: GrayboxPath.CrestKey) and "sea" (the graybox sea plane). Files:
//   CrestBuilder.cs   build order, materials, the deck, the faces, the parapet, the lee rail, per-frame switch and the sea
//   CrestProps.cs     lamp posts, bollards, lifebuoy stations, the cart (PathGuide obstacle), far gate towers
//   SeaClipmap.cs     the clipmap mesh drawn by Megastructure/BwSea (SeaState drives its globals)
//   SeaDisturbField.cs the C# twin of the kraken disturbances BwSea reads (Addendum A1, SeaDisturbances bus)
//   SeaDisturbanceQa.cs a QA-only writer of that bus (-sea-dist-qa near|far) proving every kind at fixed poses
// Geometry (14.4, 14.12; everything from BulwarkConst): road deck x -6.0..6.0 (BwRoad: lane lines and chainage numerals;
// 1 cm chamfer at the lee edge: the KS05 deck-strip silhouette), road mesh z -300..420 and 956..1800 (beat 3 owns the spillway bridge z 420-704, beat 4 the
// crest z 704-920, beat 5 the tower z 920-956; beside the beat-1 shell x -4.1..1.1, z -0.6..120.7 only the two strips),
// then BwFarField's analytic strip. Sea-side parapet x 5.5-6.0, top 1.10 (BwConcrete, chamfered 12 m segments: 2 cm
// V-joints), sea face x 6.0 to y -20 then the 1:8 batter (ConcreteWet), lee face x -6.0 to y -1500 (ConcreteDark). Lee
// rail: round posts 0.12 m every 2 m at odd z (side-mounted on the edge, axis x -6.0), two round rails 0.10 m (tops 1.1,
// 0.55) through BwLine (H7: >= 1.5 px, energy alpha) with steel caps. Lamp posts 9.0 m at z 150 + 50 k (k 0-5): lee side
// at x -5.85 (Wave A decision: a post at -5.6 stands inside the capsule reach of the lane), sea side on the parapet (x 5.75).
// Bollards 0.9 m every 20 m on the lee kerb (x -5.85), cart 2.8 x 1.3 x 1.9 at (2.0, 0, 330) as a PathGuide obstacle.
// Nothing else stands inside the capsule reach of the road lane (x -5.75..5.45).
// Switching (14.6 "looking back"): the crest and the sea are disabled at s >= 960 (hysteresis 20 m: back on below 940);
// they are never switched inside the frustum (the stair profile hides the portal from s 938).
using System;
using System.Collections.Generic;
using Megastructure.Player;
using UnityEngine;
using UnityEngine.Rendering;

namespace Megastructure.Beats
{
    [ReplacesGraybox("beat2")]
    [ReplacesGraybox("sea")]
    public sealed partial class Beat02Crest : IBeatBuilder, IWorldUpdater
    {
        public string Name => "Beat02_Crest";

        // ---------------------------------------------------------------- crest bands of this builder (GrayboxPath.CrestKey)
        /// <summary>The road mesh bands this builder owns: z -300..420 (south of the spillway) and 956..1800 (north of the
        /// tower), the graybox's "beat2" key.</summary>
        public static readonly double[] BandZ0 = { BulwarkConst.RoadMeshZMin, BulwarkConst.TowerZ1 };
        public static readonly double[] BandZ1 = { BulwarkConst.SpillwayZ0, BulwarkConst.RoadMeshZMax };
        /// <summary>Crest renderers and the sea switch off here (14.6, Part 10 #36) and back on below SwitchOnS.</summary>
        public const double SwitchOffS = 960.0, SwitchOnS = 940.0;
        /// <summary>Parapet segment length (m): chamfered V-joints every 12 m (a cast-in-place parapet's contraction joints).</summary>
        public const double ParapetSegment = 12.0;
        /// <summary>Lee face depth of the real geometry (m below the road); the far field's analytic face continues below.</summary>
        public const double LeeFaceDepth = 1500.0;
        /// <summary>Sea face: vertical to y -20, then the 1:8 batter to y -40 (never visible below the troughs).</summary>
        public const double SeaFaceBottomY = -40.0;
        /// <summary>Lee rail posts reach this far down the lee face (side-mounted on the deck edge, m below the road).</summary>
        public const double LeePostFoot = -0.32;
        /// <summary>Chamfer of near edges (14.5 "1-3 cm bevels within 3 m").</summary>
        public const float Chamfer = 0.02f;
        /// <summary>The deck's lee edge takes the 1 cm end of the range: its outer corner is the KS05 deck-strip silhouette
        /// (row 1080 + 1728.4 tan(atan(1.71 / 1.553) - 30 deg) = 1633 at 4K, inside the accepted 1628 +-10; a 2 cm bevel
        /// lowers it to 1639).</summary>
        public const float LeeEdgeChamfer = 0.01f;

        // ---------------------------------------------------------------- materials
        Material m_road, m_concrete, m_dark, m_wet, m_steel, m_ochre, m_rail10, m_post12, m_sea;
        readonly List<Renderer> m_switch = new List<Renderer>(64);
        bool m_on = true;
        MeshRenderer m_seaRenderer;
        SeaState m_seaState;
        static bool s_cameraHooked;

        /// <summary>Materials made at runtime by this builder (copies of library materials, the sea).</summary>
        public readonly List<Material> RuntimeMaterials = new List<Material>();
        /// <summary>Root of this builder's objects.</summary>
        public Transform Root { get; private set; }
        /// <summary>Triangles of the crest geometry (without the sea) and of the sea clipmap.</summary>
        public long CrestTriangles { get; private set; }
        public int SeaTriangles { get; private set; }

        // ================================================================ build
        public void Build(WorldContext ctx)
        {
            var root = new GameObject(Name).transform;
            root.SetParent(ctx.Root, false);
            Root = root;
            CreateMaterials(ctx);
            long t0 = ctx.Triangles;

            BuildDeck(ctx, root);
            BuildFaces(ctx, root);
            BuildParapet(ctx, root);
            BuildLeeRail(ctx, root);
            BuildLampPosts(ctx, root);
            // BuildExpansionJoints(ctx, root);   // fix wave X2 S4-6: BwRoad draws the joints (box-filtered, faded below 3 px)
            BuildBollards(ctx, root);
            BuildLifebuoys(ctx, root);
            BuildCart(ctx, root);
            BuildFarTowers(ctx, root);
            CrestTriangles = ctx.Triangles - t0;

            BuildSea(ctx, root);
            ctx.Log($"Beat02_Crest: crest {CrestTriangles} triangles, sea clipmap {SeaTriangles} triangles, {m_switch.Count} renderers");

            var k = BulwarkConst.KeyShots[3];   // KS04: the crest set pre-warms at the sea hold
            ctx.PrewarmSets.Add(new PrewarmSet { Name = "Crest", Eye = new Vector3((float)k.x, (float)k.y, (float)k.z), YawDeg = (float)k.yaw, PitchDeg = (float)k.pitch, Enable = _ => { } });
            ctx.Register(this);
            if (m_seaState != null) ctx.Register(m_seaState);
            // QA only (Addendum A1): a stand-in writer of the kraken's SeaDisturbances bus, behind -sea-dist-qa
            if (SeaDisturbanceQa.Requested) ctx.Register(new SeaDisturbanceQa(CliArgs.Get(SeaDisturbanceQa.Flag)));
        }

        void CreateMaterials(WorldContext ctx)
        {
            var lib = ctx.Materials;
            m_road = lib != null ? lib.Road : null;
            m_concrete = lib != null ? lib.Concrete : null;
            m_dark = lib != null ? lib.ConcreteDark : null;
            m_wet = lib != null ? lib.ConcreteWet : null;
            m_steel = lib != null ? lib.Steel : null;
            m_ochre = lib != null ? lib.SteelOchre : null;
            if (m_road == null) m_road = m_concrete;
            // BwLine tubes are built on radius _LineWidth / 2: own copies of the Rail material at the spec diameters
            // (14.5.2: rails 0.10, posts 0.12), independent of the library Rail width
            m_rail10 = LineCopy(lib != null ? lib.Rail : null, "crest_rail10", (float)BulwarkConst.LeeRailDiameter);
            m_post12 = LineCopy(lib != null ? lib.Rail : null, "crest_post12", (float)BulwarkConst.LeeRailPostSize);
        }

        Material LineCopy(Material src, string name, float width)
        {
            if (src == null) return null;
            var m = new Material(src) { name = name };
            if (m.HasProperty("_LineWidth")) m.SetFloat("_LineWidth", width);
            RuntimeMaterials.Add(m);
            return m;
        }

        MeshRenderer Add(WorldContext ctx, Transform root, MeshBuilder mb, string name, Material mat, bool shadows = true)
        {
            if (mb.VertexCount == 0 || mat == null) return null;
            var mr = ctx.AddMesh(root, name, mb.ToMesh(name), mat, shadows, shadows);
            m_switch.Add(mr);
            return mr;
        }

        // ================================================================ deck (BwRoad)
        /// <summary>Deck x stations with the baked AO of the parapet foot (vertex R): open on the road, darker in the corner.</summary>
        static readonly double[] s_deckX = { -5.99, -5.0, 0.0, 4.6, 5.1, 5.35, 5.5, 6.0 };
        static readonly byte[] s_deckAo = { 255, 255, 255, 255, 236, 206, 178, 178 };

        void BuildDeck(WorldContext ctx, Transform root)
        {
            var mb = new MeshBuilder();
            double shZ0 = BulwarkConst.ShellZMin, shZ1 = BulwarkConst.ShellZMax, shX0 = BulwarkConst.ShellXMin, shX1 = BulwarkConst.ShellXMax;
            // south band: full width to the shell, the two strips beside it, full width from the door wall to the spillway
            DeckTop(mb, BandZ0[0], shZ0, double.NegativeInfinity, double.PositiveInfinity);
            DeckTop(mb, shZ0, shZ1, double.NegativeInfinity, shX0);
            DeckTop(mb, shZ0, shZ1, shX1, double.PositiveInfinity);
            DeckTop(mb, shZ1, BandZ1[0], double.NegativeInfinity, double.PositiveInfinity);
            DeckTop(mb, BandZ0[1], BandZ1[1], double.NegativeInfinity, double.PositiveInfinity);
            // lee edge chamfer (2 cm, 45 deg) along both bands, and the end faces at z -300 and 1800 (the open road ends)
            for (int b = 0; b < 2; b++) LeeChamfer(mb, BandZ0[b], BandZ1[b]);
            EndFace(mb, BandZ0[0], -1);
            EndFace(mb, BandZ1[1], +1);
            Add(ctx, root, mb, "crest_deck", m_road);
        }

        /// <summary>Deck top (y 0) between z0 and z1, clipped to x in [xa, xb], with rows every 10 m and the AO columns.</summary>
        static void DeckTop(MeshBuilder mb, double z0, double z1, double xa, double xb)
        {
            var xs = new List<double>(12);
            var ao = new List<byte>(12);
            for (int i = 0; i < s_deckX.Length; i++)
            {
                double x = s_deckX[i];
                if (x < xa || x > xb) continue;
                xs.Add(x); ao.Add(s_deckAo[i]);
            }
            if (xa > s_deckX[0] && xa < s_deckX[s_deckX.Length - 1]) { xs.Insert(0, xa); ao.Insert(0, AoAt(xa)); }
            if (xb < s_deckX[s_deckX.Length - 1] && xb > s_deckX[0]) { xs.Add(xb); ao.Add(AoAt(xb)); }
            int rows = Math.Max(1, (int)Math.Ceiling((z1 - z0) / 10.0));
            int first = mb.VertexCount, cols = xs.Count;
            for (int r = 0; r <= rows; r++)
            {
                float z = (float)(z0 + (z1 - z0) * r / rows);
                for (int c = 0; c < cols; c++)
                {
                    mb.Color = new Color32(ao[c], 0, 0, 255);
                    mb.Vertex(new Vector3((float)xs[c], 0f, z), Vector3.up, new Vector2((float)xs[c], z));
                }
            }
            mb.Color = new Color32(255, 0, 0, 255);
            for (int r = 0; r < rows; r++)
                for (int c = 0; c + 1 < cols; c++)
                {
                    int a = first + r * cols + c;
                    mb.Quad(a, a + cols, a + cols + 1, a + 1);   // clockwise from above: (x, z) -> (x, z+) -> (x+, z+) -> (x+, z)
                }
        }

        static byte AoAt(double x)
        {
            for (int i = 0; i + 1 < s_deckX.Length; i++)
                if (x >= s_deckX[i] && x <= s_deckX[i + 1])
                {
                    double t = (x - s_deckX[i]) / Math.Max(s_deckX[i + 1] - s_deckX[i], 1e-6);
                    return (byte)Math.Round(s_deckAo[i] + (s_deckAo[i + 1] - s_deckAo[i]) * t);
                }
            return 255;
        }

        /// <summary>The 1 cm chamfer of the lee edge: (-5.99, 0) -> (-6.0, -0.01), facing up-west, edge wear G 255.</summary>
        static void LeeChamfer(MeshBuilder mb, double z0, double z1)
        {
            int rows = Math.Max(1, (int)Math.Ceiling((z1 - z0) / 10.0));
            var n = new Vector3(-0.70710678f, 0.70710678f, 0f);
            mb.Color = new Color32(255, 255, 0, 255);
            int first = mb.VertexCount;
            for (int r = 0; r <= rows; r++)
            {
                float z = (float)(z0 + (z1 - z0) * r / rows);
                mb.Vertex(new Vector3(-6.0f, -LeeEdgeChamfer, z), n);
                mb.Vertex(new Vector3(-6.0f + LeeEdgeChamfer, 0f, z), n);
            }
            for (int r = 0; r < rows; r++)
            {
                int a = first + 2 * r;
                mb.Quad(a, a + 2, a + 3, a + 1);
            }
            mb.Color = new Color32(255, 0, 0, 255);
        }

        /// <summary>Open road end (z -300 / 1800): the deck slab's end face, 0.6 m deep, facing outward (sign -1 = south).</summary>
        static void EndFace(MeshBuilder mb, double z, int sign)
        {
            float zf = (float)z, y0 = -0.6f;
            var a = new Vector3(-6f, y0, zf);
            var b = new Vector3(-6f, 0f, zf);
            var c = new Vector3(6f, 0f, zf);
            var d = new Vector3(6f, y0, zf);
            if (sign < 0) mb.FlatQuad(a, b, c, d);   // seen from -z: x right
            else mb.FlatQuad(d, c, b, a);
        }

        // ================================================================ faces: sea face (ConcreteWet), lee face (ConcreteDark)
        void BuildFaces(WorldContext ctx, Transform root)
        {
            var sea = new MeshBuilder();
            var lee = new MeshBuilder();
            for (int b = 0; b < 2; b++)
            {
                double z0 = BandZ0[b], z1 = BandZ1[b];
                // sea face x 6.0, y 0 .. -20, then the batter x = 6 + (-20 - y) / 8 to y -40; facing +x (the sea)
                double xb = BulwarkConst.SeaFaceX + (BulwarkConst.SeaFaceBatterY - SeaFaceBottomY) / BulwarkConst.SeaFaceBatter;
                // (from y -0.05: the parapet's outer face covers y -0.05..1.1, no co-planar overlap)
                WallStrip(sea, z0, z1, BulwarkConst.SeaFaceX, -0.05, BulwarkConst.SeaFaceX, BulwarkConst.SeaFaceBatterY, +1);
                WallStrip(sea, z0, z1, BulwarkConst.SeaFaceX, BulwarkConst.SeaFaceBatterY, xb, SeaFaceBottomY, +1);
                // lee face x -6.0 from the chamfer foot (y -0.01) down; the beat-1 shell does not reach the lee face
                WallStrip(lee, z0, z1, BulwarkConst.LeeFaceX, -LeeEdgeChamfer, BulwarkConst.LeeFaceX, -LeeFaceDepth, -1);
            }
            Add(ctx, root, sea, "crest_sea_face", m_wet);
            Add(ctx, root, lee, "crest_lee_face", m_dark);
        }

        /// <summary>A wall strip from (xTop, yTop) down to (xBot, yBot) along z0..z1 with rows every 10 m, facing +x (side +1)
        /// or -x (side -1).</summary>
        static void WallStrip(MeshBuilder mb, double z0, double z1, double xTop, double yTop, double xBot, double yBot, int side)
        {
            int rows = Math.Max(1, (int)Math.Ceiling((z1 - z0) / 10.0));
            Vector3 down = new Vector3((float)(xBot - xTop), (float)(yBot - yTop), 0f);
            Vector3 n = Vector3.Cross(Vector3.forward, down).normalized * side;   // horizontal-ish, pointing to the open side
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
                // seen from the open side: +x side looks toward -x with z to the left
                if (side > 0) mb.Quad(a, a + 2, a + 3, a + 1);
                else mb.Quad(a, a + 1, a + 3, a + 2);
            }
        }

        // ================================================================ parapet (BwConcrete, chamfered 12 m segments)
        /// <summary>Freeing ports (scuppers) through the parapet foot: 0.40 m wide, open from the deck to y 0.15, 3 m and 9 m
        /// into every 12 m segment (one every 6 m), so storm water and spray run back to the sea. The parapet is cast on a
        /// 0.17 m kicker (the base course between the ports) with the wall above from y 0.15 (2 cm overlap: no co-planar
        /// faces; the wall's 2 cm chamfer reads as the kicker joint).</summary>
        public const double ScupperWidth = 0.40, ScupperTop = 0.15;
        public static readonly double[] ScupperOffsets = { 3.0, 9.0 };

        void BuildParapet(WorldContext ctx, Transform root)
        {
            var mb = new MeshBuilder();
            float x0 = (float)BulwarkConst.ParapetXMin, x1 = (float)BulwarkConst.ParapetXMax, top = (float)BulwarkConst.ParapetTop;
            const float embed = 0.05f;   // the base sits 5 cm into the deck: no crack at the foot
            float yWall = (float)ScupperTop, yKick = (float)ScupperTop + 0.02f;
            float hw = 0.5f * (float)ScupperWidth;
            for (int b = 0; b < 2; b++)
            {
                double z0 = BandZ0[b], z1 = BandZ1[b];
                double za = z0;
                while (za < z1 - 1e-6)
                {
                    double zb = NextParapetJoint(za, z1);
                    float xc = 0.5f * (x0 + x1);
                    // the wall above the ports
                    MeshKit.ChamferBox(mb, new Vector3(xc, 0.5f * (top + yWall), (float)(0.5 * (za + zb))), new Vector3(x1 - x0, top - yWall, (float)(zb - za)), Chamfer);
                    // the kicker between the ports (ports only in full segments of the joint phase, 1 m clear of the ends)
                    double zs = za;
                    double g0 = ParapetJointPhase + ParapetSegment * Math.Floor((za - ParapetJointPhase) / ParapetSegment);
                    for (double zg = g0; zg < zb; zg += ParapetSegment)
                        foreach (double off in ScupperOffsets)   // ascending: the ports come in z order
                        {
                            double zp = zg + off;
                            if (zp < za + 1.0 || zp > zb - 1.0) continue;
                            KickerPiece(mb, xc, x1 - x0, -embed, yKick, zs, zp - hw);
                            zs = zp + hw;
                        }
                    KickerPiece(mb, xc, x1 - x0, -embed, yKick, zs, zb);
                    za = zb;
                }
            }
            Add(ctx, root, mb, "crest_parapet", m_concrete);
        }

        static void KickerPiece(MeshBuilder mb, float xc, float w, float y0, float y1, double za, double zb)
        {
            if (zb - za < 0.05) return;
            MeshKit.ChamferBox(mb, new Vector3(xc, 0.5f * (y0 + y1), (float)(0.5 * (za + zb))), new Vector3(w, y1 - y0, (float)(zb - za)), Chamfer);
        }

        /// <summary>Phase of the parapet joints: z = 6 + 12 k, so the sea-side lamp post at z 300 stands mid-segment and the
        /// posts at z 200 / 400 and the lifebuoy stations stand at least 2 m from a joint (a post plate never straddles one).</summary>
        public const double ParapetJointPhase = 6.0;

        /// <summary>The next parapet joint after za (at most z1); an end piece shorter than 3 m merges with its neighbour.</summary>
        public static double NextParapetJoint(double za, double z1)
        {
            double k = Math.Floor((za - ParapetJointPhase) / ParapetSegment) + 1.0;
            double zb = ParapetJointPhase + ParapetSegment * k;
            if (zb - za < 3.0) zb += ParapetSegment;
            if (z1 - zb < 3.0) zb = z1;
            return Math.Min(zb, z1);
        }

        // ================================================================ lee rail (BwLine tubes, steel caps)
        void BuildLeeRail(WorldContext ctx, Transform root)
        {
            var rails = new MeshBuilder();
            var posts = new MeshBuilder();
            var caps = new MeshBuilder();
            float rr = 0.5f * (float)BulwarkConst.LeeRailDiameter, pr = 0.5f * (float)BulwarkConst.LeeRailPostSize;
            float x = (float)BulwarkConst.LeeRailX;
            float yTop = (float)BulwarkConst.LeeRailTop - rr, yMid = (float)BulwarkConst.LeeRailMid - rr;
            // south band: from the shell's south end (the rail of 14.5.2 starts at the gallery) to the spillway; north band
            double[] r0 = { BulwarkConst.ShellZMin + 0.2, BandZ0[1] + 0.1 };
            double[] r1 = { BandZ1[0] - 0.1, BandZ1[1] };
            for (int b = 0; b < 2; b++)
            {
                double a0 = r0[b], a1 = r1[b];
                for (double a = a0; a < a1 - 1e-6; a += 100.0)
                {
                    double e = Math.Min(a1, a + 100.0);
                    Tube(rails, new Vector3(x, yTop, (float)a), new Vector3(x, yTop, (float)e), rr);
                    Tube(rails, new Vector3(x, yMid, (float)a), new Vector3(x, yMid, (float)e), rr);
                }
                // rail end caps (steel discs) at both ends of both rails
                foreach (double ze in new[] { a0, a1 })
                {
                    int s = ze == a0 ? -1 : 1;
                    MeshKit.Disc(caps, new Vector3(x, yTop, (float)ze), new Vector3(0f, 0f, s), rr, 12);
                    MeshKit.Disc(caps, new Vector3(x, yMid, (float)ze), new Vector3(0f, 0f, s), rr, 12);
                }
                // posts every 2 m at odd z, from the bracket on the face (y -0.32) to 2 cm above the top rail, steel caps
                for (double z = Math.Ceiling(a0 / BulwarkConst.LeeRailPostPitch) * BulwarkConst.LeeRailPostPitch + 1.0; z < a1; z += BulwarkConst.LeeRailPostPitch)
                {
                    float zf = (float)z;
                    float yCap = (float)BulwarkConst.LeeRailTop + 0.02f;
                    Tube(posts, new Vector3(x, (float)LeePostFoot, zf), new Vector3(x, yCap, zf), pr, 10);
                    MeshKit.Disc(caps, new Vector3(x, yCap, zf), Vector3.up, pr + 0.004f, 12);
                    MeshKit.Disc(caps, new Vector3(x, (float)LeePostFoot, zf), Vector3.down, pr, 10);
                    // bracket: a bolted steel plate on the deck edge behind the post (x -6.0 .. -5.84), 1.5 cm thick
                    MeshKit.ChamferBox(caps, new Vector3(x + 0.08f, 0.0075f, zf), new Vector3(0.16f, 0.015f, 0.2f), 0.004f);
                }
            }
            Add(ctx, root, rails, "crest_rails", m_rail10, false);
            Add(ctx, root, posts, "crest_rail_posts", m_post12, false);
            Add(ctx, root, caps, "crest_rail_caps", m_steel);
        }

        /// <summary>Round tube a -> b on radius r (BwLine _LINE_TUBE contract, D-07): smooth outward normals, a vertex ring at
        /// least every 4 m, open ends (capped by steel discs).</summary>
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

        // ================================================================ the sea (SeaClipmap + BwSea, SeaState)
        void BuildSea(WorldContext ctx, Transform root)
        {
            m_seaState = new SeaState();
            if (!s_cameraHooked)
            {
                RenderPipelineManager.beginCameraRendering += SeaState.OnBeginCamera;
                s_cameraHooked = true;
            }
            Shader sh = Shader.Find("Megastructure/BwSea");
            if (sh == null)
            {
                Debug.LogError("[BW-FAIL] Beat02_Crest: shader Megastructure/BwSea not found (Always Included? run RebuildAll): no sea");
                return;
            }
            m_sea = new Material(sh) { name = "crest_sea" };
            m_sea.renderQueue = (int)RenderQueue.Geometry + 100;   // after the opaques: walls and the door reject it early
            RuntimeMaterials.Add(m_sea);
            Mesh mesh = SeaClipmap.Build(out int tris);
            SeaTriangles = tris;
            var go = new GameObject("crest_sea_clipmap");
            go.transform.SetParent(root, false);
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            var mr = go.AddComponent<MeshRenderer>();
            mr.sharedMaterial = m_sea;
            mr.shadowCastingMode = ShadowCastingMode.Off;
            mr.receiveShadows = true;
            mr.lightProbeUsage = LightProbeUsage.Off;
            mr.reflectionProbeUsage = ReflectionProbeUsage.Off;
            mr.allowOcclusionWhenDynamic = false;
            ctx.Triangles += tris;
            m_seaRenderer = mr;
            m_switch.Add(mr);
        }

        // ================================================================ per frame
        public void OnFrame(in FrameState f)
        {
            bool on = m_on ? f.s < SwitchOffS : f.s < SwitchOnS;
            if (on == m_on) return;
            m_on = on;
            for (int i = 0; i < m_switch.Count; i++)
                if (m_switch[i] != null) m_switch[i].enabled = on;
        }
    }
}
