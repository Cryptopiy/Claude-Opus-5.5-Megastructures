// GrayboxPath: the graybox of the whole 1500 m walk (MASTER_PROMPT Part 13 graybox gate, 3.1 rule 2, 14.4, and the
// "Graybox:" fallback line of every 14.5 beat). One builder, clean volumes only, everything from BwPath and BulwarkConst
// (no hand-placed span table, lesson 3.2 #11). Files:
//   GrayboxPath.cs      core: build order, runtime stand-in materials, mesh helpers, the door and lamp lights per frame,
//                       key-shot subjects (from QA.VisibilityAudit, the same spec bounds the 11.13 audit uses)
//   GrayboxDam.cs       fixed pieces of beats 1-4 (road mesh z -300..1800): gallery shell with slits, niches with valve
//                       wheels, lamps and the door; road deck, sea and lee faces, parapet, round lee rail with posts, lamp
//                       posts, bollards, cart, far towers; the spillway bridge (15 piers, 14 slots with sill, sloping water
//                       and box gates, gate houses, sea-side rails, the fogged cone plume); the beat 4 surf strip over the
//                       belt (x 6..140) with foam and the six islets (stand-in piles with foam skirts); the gate tower with
//                       portal, vehicle arches, atrium pendants and guard rail; the sea plane; the stand-in sky (only
//                       without WorldSky)
//   GrayboxInterior.cs  lane-driven pieces from s 934: the 270 stairs, handrails and landing lamps, exit hall, window
//                       gallery (ten 20 x 36 m veil panes between 10 m piers, trench, rail at axis + 3.0, bollard lamps),
//                       sluice tunnel (conduit panel, lamps), the lee ledge slab with the z 1415.2 apron, rail, lantern
// Graybox lines (14.5): B1 box corridor with lamps and a box door; B2 plane, deck, parapet, rail (sky: WorldSky); B3 bridge,
// box gates, fogged cone plume, no bow; B4 flat surf strip with foam and the tower box; B5 floor dropping 54 m over 97.8 m,
// rails, lamps (here as the exact 270 steps); B6 hall box, glass planes with a vertical gradient shader, no whale; B7 tunnel
// box with a bright quad, ledge slab, sky gradient, lantern sphere.
// No colliders: the corridor is PathGuide's (generated from the same lane function); the cart is a PathGuide obstacle.
// Visual only; Phase 4 beat builders replace these volumes beat by beat with [ReplacesGraybox("beatN")] (keys beat1..beat7,
// sea, sky, lights; docs/FRAMEWORK.md section 4.0): every piece is emitted into the mesh of ONE part key, grouped under
// "gb_part_<key>", and a replaced key's pieces are not built at all (no surface drawn twice). Part map (fixed pieces of
// s < 934 lie on the straight crest, z = s):
//   beat1  the gallery: shell with slits, baffles and niches, lamps, fittings, door leaves, the deck under the shell
//          (x -4.1..1.1, z -0.6..120.7; the 14.4 "road deck cut")
//   beat2  the 14.5.2 road mesh: deck, faces, parapet, lee rail, lamp posts and bollards for z < 420 (beside the shell
//          too) and z >= 956 (north of the tower), the cart, the far gate towers
//   beat3  the spillway bridge z 420-704 (piers, slots, sill, gates, draw-down water, sea-side rails, the plume cone)
//   beat4  the crest z 704-920 (deck, faces, parapet, lee rail, posts), the lee face below it, the surf strip and islets,
//          the tower shell (14.5.4 TowerBuilder)
//   beat5  the tower interior z 920-956 (atrium floor and ceiling, well wall, pendants, guard rail) and every lane-driven
//          piece of s 934-1070 (stairs, handrails, landing lamps, exit hall)
//   beat6  lane-driven s 1070-1370 (window gallery, trench, rail, panes, bollard lamps, the doorway wall at s 1070)
//   beat7  lane-driven s 1370-1500 (tunnel, conduit, tunnel lamps, the hall end wall at s 1373), the ledge, its rails,
//          the lantern, the lee face north of z 920 with the mouth
//   sea    the sea plane; sky: the stand-in sky (also skipped while WorldSky exists); lights: every graybox lamp anchor
// The door gap is keyed to progress (H6): 4.0 m x smootherstep((s - 104) / 7.5), identical in captures and walks.
// Lamps: emissive stand-ins plus anchors in the shared LocalLightPool (Runtime/Rendering, at most 4 real lights, 14.2.3),
// which fades every light in and out by distance, view rank and time (no pool pops, Part 10 #6, 14.10.3 frame-step limit).
// Visible geometry stays outside the capsule reach of the lane (lane edge + PLAYER_R; checked by VisibilityAuditTests.
// Graybox_StaysOutsideTheCapsuleReach), except the spec's cart on the road (a PathGuide obstacle).
using System;
using System.Collections.Generic;
using Megastructure.Player;
using Megastructure.QA;
using Megastructure.Rendering;
using UnityEngine;

namespace Megastructure.Beats
{
    public sealed partial class GrayboxPath : IBeatBuilder, IWorldUpdater, IKeyShotSubjects
    {
        public string Name => "Graybox_Path";

        /// <summary>The atmosphere owner's sky dome / far field type. The stand-in sky is built only when it is absent.</summary>
        public const string WorldSkyTypeName = "Megastructure.Beats.WorldSky";
        public static bool WorldSkyPresent => typeof(GrayboxPath).Assembly.GetType(WorldSkyTypeName) != null;

        /// <summary>Shader names of the graybox stand-ins (Assets/_Game/Shaders/BwGraybox*.shader).</summary>
        public const string ShaderGlow = "Megastructure/BwGrayboxGlow", ShaderSky = "Megastructure/BwGrayboxSky", ShaderMist = "Megastructure/BwGrayboxMist";

        /// <summary>Palette token indices of BwGrayboxGlow (_Token; HLSL twin GbToken).</summary>
        public enum GbToken { SodiumWarm = 0, Tungsten = 1, Lantern = 2, FoamWhite = 3, SeaStorm = 4, SteelPaint = 5, ConcreteDark = 6, SafetyOchre = 7, SeaTeal = 8, StormHorizon = 9, LaneWhite = 10 }

        /// <summary>Renderer names that are not solid (QA occlusion ignores them): sky, sea and water, mist, glass, lamps.</summary>
        public static bool IsNonOccluder(string rendererName)
        {
            if (string.IsNullOrEmpty(rendererName)) return true;
            return rendererName.StartsWith("gb_sky", StringComparison.Ordinal) || rendererName.StartsWith("gb_sea", StringComparison.Ordinal)
                || rendererName.StartsWith("gb_water", StringComparison.Ordinal) || rendererName.StartsWith("gb_mist", StringComparison.Ordinal)
                || rendererName.StartsWith("gb_glass", StringComparison.Ordinal) || rendererName.StartsWith("gb_surf", StringComparison.Ordinal)
                || rendererName.StartsWith("gb_conduit", StringComparison.Ordinal) || rendererName.StartsWith("gb_lamp", StringComparison.Ordinal)
                || rendererName.StartsWith("gb_islet_foam", StringComparison.Ordinal);
        }

        /// <summary>Renderer names that are not physical at all (no capsule clearance check): sky, sea, water, surf, mist.</summary>
        public static bool IsNonPhysical(string rendererName)
        {
            if (string.IsNullOrEmpty(rendererName)) return true;
            return rendererName.StartsWith("gb_sky", StringComparison.Ordinal) || rendererName.StartsWith("gb_sea", StringComparison.Ordinal)
                || rendererName.StartsWith("gb_water", StringComparison.Ordinal) || rendererName.StartsWith("gb_mist", StringComparison.Ordinal)
                || rendererName.StartsWith("gb_surf", StringComparison.Ordinal) || rendererName.StartsWith("gb_islet_foam", StringComparison.Ordinal);
        }

        // ---- materials of this build (library + runtime stand-ins)
        Material m_concrete, m_dark, m_wet, m_rail, m_handrail;
        Material m_sea, m_surf, m_steel, m_ochre, m_sodium, m_tungsten, m_tungstenLow, m_lantern, m_storm, m_conduit, m_veil, m_mist, m_sky;
        /// <summary>Materials created at runtime by this builder (destroyed by the owner of the build, e.g. the EditMode audit).</summary>
        public readonly List<Material> RuntimeMaterials = new List<Material>();
        /// <summary>True when the stand-in sky sphere was built (WorldSky absent).</summary>
        public bool BuiltSky { get; private set; }
        /// <summary>Root transform of the graybox (the EditMode visibility audit adds colliders below it).</summary>
        public Transform Root { get; private set; }

        Transform m_leafW, m_leafE;
        Vector3 m_leafW0, m_leafE0;
        float m_lastGap = -1f;

        // ================================================================ part keys (ReplacesGraybox, FRAMEWORK.md 4.0)
        /// <summary>Part keys of [ReplacesGraybox]: index 0..6 = beat1..beat7, then sea, sky, lights.</summary>
        public static readonly string[] PartKeys = { "beat1", "beat2", "beat3", "beat4", "beat5", "beat6", "beat7", "sea", "sky", "lights" };
        public const int KeyCount = 10, KeySea = 7, KeySky = 8, KeyLights = 9, BeatKeys = 7;
        const int K1 = 0, K2 = 1, K3 = 2, K4 = 3, K5 = 4, K6 = 5, K7 = 6;

        /// <summary>Which part keys a beat builder replaces: GrayboxReplace.IsReplaced (the [ReplacesGraybox] attributes of
        /// this assembly) unless set before Build (the EditMode tests inject their own set; never a test type in the build).</summary>
        public Func<string, bool> ReplacedQuery = GrayboxReplace.IsReplaced;
        readonly bool[] m_off = new bool[KeyCount];
        readonly Transform[] m_groups = new Transform[KeyCount];
        WorldContext m_ctx;

        /// <summary>True when the part key (index into PartKeys) was replaced at Build, i.e. none of its pieces exist.</summary>
        public bool IsReplacedPart(int key) => m_off[key];
        /// <summary>Group transform "gb_part_&lt;key&gt;" holding the pieces of that key (null when replaced or empty).</summary>
        public Transform PartGroup(int key) => m_groups[key];
        /// <summary>The key index of a beat number 1..7.</summary>
        public static int BeatKey(int beat) => Math.Max(0, Math.Min(BeatKeys - 1, beat - 1));

        /// <summary>The group a piece of this key is parented to (created on first use).</summary>
        Transform Group(int key)
        {
            if (m_groups[key] == null)
            {
                var g = new GameObject("gb_part_" + PartKeys[key]).transform;
                g.SetParent(Root, false);
                m_groups[key] = g;
            }
            return m_groups[key];
        }

        /// <summary>One MeshBuilder per beat key (index K1..K7) for a piece name.</summary>
        static MeshBuilder[] NewSet()
        {
            var set = new MeshBuilder[BeatKeys];
            for (int i = 0; i < BeatKeys; i++) set[i] = new MeshBuilder();
            return set;
        }

        /// <summary>Flushes a piece of one key under its group (nothing when the key is replaced or the builder is empty).</summary>
        void FlushKey(WorldContext ctx, int key, ref MeshBuilder mb, string name, Material mat, bool shadows = true)
        {
            if (!m_off[key] && mb.VertexCount > 0) ctx.AddMesh(Group(key), name, mb.ToMesh(name), mat, shadows, shadows);
            mb = new MeshBuilder();
        }

        /// <summary>Flushes every non-empty, non-replaced builder of a set under its key's group.</summary>
        void FlushSet(WorldContext ctx, MeshBuilder[] set, string name, Material mat, bool shadows = true)
        {
            for (int k = 0; k < set.Length; k++) FlushKey(ctx, k, ref set[k], name, mat, shadows);
        }

        /// <summary>Crest bands of the fixed pieces (z = s for s &lt; 934): the 14.5.2 road mesh is beat 2's, except the
        /// spillway bridge (beat 3, z 420-704), the crest between the spillway and the tower (beat 4, z 704-920) and the
        /// tower interior (beat 5, z 920-956).</summary>
        static readonly double[] s_crestEdge = { BulwarkConst.SpillwayZ0, BulwarkConst.SpillwayZ1, BulwarkConst.TowerZ0, BulwarkConst.TowerZ1 };
        static readonly int[] s_crestKey = { K2, K3, K4, K5, K2 };

        /// <summary>The part key of a crest piece at z.</summary>
        public static int CrestKey(double z)
        {
            int i = 0;
            while (i < s_crestEdge.Length && z >= s_crestEdge[i]) i++;
            return s_crestKey[i];
        }

        /// <summary>A box of the crest, split at the crest bands into the set of each band's key (replaced keys skipped).</summary>
        void CBox(MeshBuilder[] set, double x0, double y0, double z0, double x1, double y1, double z1)
        {
            double lo = Math.Min(z0, z1), b = Math.Max(z0, z1);
            while (lo < b - 1e-9)
            {
                int i = 0;
                while (i < s_crestEdge.Length && lo >= s_crestEdge[i] - 1e-9) i++;
                double hi = i < s_crestEdge.Length ? Math.Min(b, s_crestEdge[i]) : b;
                int key = s_crestKey[i];
                if (!m_off[key]) BoxMinMax(set[key], x0, y0, lo, x1, y1, hi);
                lo = hi;
            }
        }

        /// <summary>The z sub-ranges of [z0, z1] by crest band (for runs such as rails): key, lo, hi; replaced keys skipped.</summary>
        void ForCrestBands(double z0, double z1, Action<int, double, double> run)
        {
            double lo = Math.Min(z0, z1), b = Math.Max(z0, z1);
            while (lo < b - 1e-9)
            {
                int i = 0;
                while (i < s_crestEdge.Length && lo >= s_crestEdge[i] - 1e-9) i++;
                double hi = i < s_crestEdge.Length ? Math.Min(b, s_crestEdge[i]) : b;
                if (!m_off[s_crestKey[i]]) run(s_crestKey[i], lo, hi);
                lo = hi;
            }
        }

        // ================================================================ build
        public void Build(WorldContext ctx)
        {
            m_ctx = ctx;
            var root = new GameObject(Name).transform;
            root.SetParent(ctx.Root, false);
            Root = root;
            var replaced = new List<string>(KeyCount);
            for (int k = 0; k < KeyCount; k++)
            {
                bool off = false;
                try { off = ReplacedQuery != null && ReplacedQuery(PartKeys[k]); }
                catch (Exception e) { Debug.LogWarning("[BW] graybox: replacement query for '" + PartKeys[k] + "' threw " + e.GetType().Name + ": " + e.Message); }
                m_off[k] = off;
                if (off) replaced.Add(PartKeys[k]);
            }
            ctx.Log("graybox: replaced parts [" + string.Join(", ", replaced) + "] (ReplacesGraybox keys; their pieces are not built)");
            CreateMaterials(ctx);
            if (!m_off[KeyLights]) LightPool = LocalLightPool.Ensure(ctx);
            // sky openings (door, portal, arches) light the interiors through the pool's _BwOpen whatever "lights" says: they
            // belong to the shell pieces of beats 1 and 4 (registered by those builders below, skipped when replaced)
            m_openPool = LocalLightPool.Ensure(ctx);

            BuildGallery(ctx, root);          // beat 1
            BuildCrest(ctx, root);            // beats 2-5 road, faces, rails, posts, props by z band
            BuildSpillway(ctx, root);         // beat 3
            BuildReefAndTower(ctx, root);     // beat 4 surf strip, tower shell; beat 5 atrium
            BuildSea(ctx, root);
            BuildLaneDriven(ctx, root);       // beats 5-7 from s 934
            BuildInteriorProps(ctx, root);    // beat 5-7 lamps, panes, conduit, ledge, lantern
            if (WorldSkyPresent) ctx.Log("graybox: WorldSky present, stand-in sky not built");
            else if (m_off[KeySky]) ctx.Log("graybox: sky replaced, stand-in sky not built");
            else { BuildSky(ctx, root); BuiltSky = true; }

            AddPrewarm(ctx, "Graybox_Interior5", BulwarkConst.KeyShots[12]);
            AddPrewarm(ctx, "Graybox_Hall6", BulwarkConst.KeyShots[14]);
            AddPrewarm(ctx, "Graybox_Tunnel7", BulwarkConst.KeyShots[18]);
            AddPrewarm(ctx, "Graybox_Ledge", BulwarkConst.KeyShots[19]);
            ctx.Register(this);
            ctx.RegisterSubjects(this);
        }

        static void AddPrewarm(WorldContext ctx, string name, KeyShot k)
        {
            ctx.PrewarmSets.Add(new PrewarmSet { Name = name, Eye = new Vector3((float)k.x, (float)k.y, (float)k.z), YawDeg = (float)k.yaw, PitchDeg = (float)k.pitch, Enable = _ => { } });
        }

        // ---------------------------------------------------------------- materials
        void CreateMaterials(WorldContext ctx)
        {
            var lib = ctx.Materials;
            m_concrete = lib != null ? lib.Concrete : null;
            m_dark = lib != null ? lib.ConcreteDark : null;
            m_wet = lib != null ? lib.ConcreteWet : null;
            m_rail = lib != null ? lib.Rail : null;
            m_handrail = lib != null ? lib.Handrail : null;

            Shader glow = Shader.Find(ShaderGlow);
            if (glow == null) Debug.LogWarning("[BW] graybox: shader " + ShaderGlow + " not found (Always Included?): library stand-ins used");
            m_sea = Glow(glow, "gb_sea", 2, GbToken.SeaStorm, 1.0f, 0.55f, m_wet);
            m_surf = Glow(glow, "gb_surf", 4, GbToken.FoamWhite, 0.55f, 0.55f, m_concrete);                                   // foam patches on the sea (x0.55: no clipping in the noon sun)
            m_steel = Glow(glow, "gb_steel", 2, GbToken.SteelPaint, 1.0f, 0.35f, m_dark);
            m_ochre = Glow(glow, "gb_ochre", 2, GbToken.SafetyOchre, 1.0f, 0.3f, m_dark);
            m_sodium = Glow(glow, "gb_sodium", 0, GbToken.SodiumWarm, (float)BulwarkConst.KeyB1Sodium, 0f, m_concrete);          // x2.5 (14.2.3)
            m_tungsten = Glow(glow, "gb_tungsten", 0, GbToken.Tungsten, (float)BulwarkConst.KeyB5Tungsten, 0f, m_concrete);     // x3.0 (clamp 3)
            m_tungstenLow = Glow(glow, "gb_tungsten_low", 0, GbToken.Tungsten, 1.5f, 0f, m_concrete);                          // bollard / landing lamps x1.5
            if (m_tungstenLow != null && m_tungstenLow.HasProperty("_ObjSize")) m_tungstenLow.SetFloat("_ObjSize", 0.2f);      // fades below 3.5 px (no isolated pixel)
            m_lantern = Glow(glow, "gb_lantern", 0, GbToken.Lantern, (float)BulwarkConst.LanternEmissive, 0f, m_concrete);      // x6
            m_storm = Glow(glow, "gb_stormlight", 0, GbToken.StormHorizon, 0.6f, 0f, m_concrete);                              // slit glow
            m_conduit = Glow(glow, "gb_conduit", 0, GbToken.SeaTeal, 0.45f, 0f, m_wet);                                         // conduit water (KS19 frame in the beat 7 band)
            m_veil = Glow(glow, "gb_veil", 1, GbToken.SeaTeal, 1.0f, 0f, m_wet);                                               // beat 6 panes

            Shader mist = Shader.Find(ShaderMist);
            if (mist != null)
            {
                // analytic optical depth through the cone (BwGrayboxMist): peak horizontal tau 1.0 (14.5.3 / H2, alpha <= 0.632),
                // Gaussian sigma 150 m along z about z 562, scale height 130 m, top +260; soft to zero at the silhouette
                m_mist = new Material(mist) { name = "gb_mist" };
                m_mist.SetFloat("_TauPeak", (float)BulwarkConst.PlumePeakOpticalDepth);
                m_mist.SetFloat("_TopY", (float)BulwarkConst.PlumeTopY);
                m_mist.SetFloat("_FadeH", (float)BulwarkConst.PlumeScaleHeight);
                m_mist.SetFloat("_PlumeZ", (float)BulwarkConst.PlumeZ);
                m_mist.SetFloat("_SigmaZ", (float)BulwarkConst.PlumeSigmaZ);
                m_mist.SetVector("_ConeBase", new Vector4(PlumeBase.x, PlumeBase.y, PlumeBase.z, PlumeBaseR));
                m_mist.SetVector("_ConeApex", new Vector4(PlumeApex.x, PlumeApex.y, PlumeApex.z, PlumeApexR));
                m_mist.SetFloat("_Brightness", 2.2f);   // white mist lit by the storm sky (x sky radiance)
                RuntimeMaterials.Add(m_mist);
            }
            else Debug.LogWarning("[BW] graybox: shader " + ShaderMist + " not found: no plume");
            Shader sky = Shader.Find(ShaderSky);
            if (sky != null) { m_sky = new Material(sky) { name = "gb_sky" }; RuntimeMaterials.Add(m_sky); }
            else m_sky = m_concrete;
        }

        Material Glow(Shader sh, string name, int mode, GbToken token, float intensity, float smoothness, Material fallback)
        {
            if (sh == null) return fallback;
            var m = new Material(sh) { name = name };
            m.SetFloat("_Mode", mode);
            m.SetFloat("_Token", (float)token);
            m.SetFloat("_Intensity", intensity);
            m.SetFloat("_Smoothness", smoothness);
            m.enableInstancing = true;
            RuntimeMaterials.Add(m);
            return m;
        }

        // ================================================================ mesh helpers
        static void BoxMinMax(MeshBuilder mb, double x0, double y0, double z0, double x1, double y1, double z1)
        {
            var c = new Vector3((float)(0.5 * (x0 + x1)), (float)(0.5 * (y0 + y1)), (float)(0.5 * (z0 + z1)));
            var size = new Vector3((float)Math.Abs(x1 - x0), (float)Math.Abs(y1 - y0), (float)Math.Abs(z1 - z0));
            mb.Box(c, size);
        }

        /// <summary>Round tube from a to b (BwLine _LINE_TUBE contract, D-07): an n-gon with smooth outward normals and a
        /// vertex ring at least every 4 m; open ends (the line shader discards back faces).</summary>
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
                    mb.Quad(A, D, C, B);   // outward (cross(D - A, C - A) points away from the axis)
                }
        }

        /// <summary>Tube radius for a BwLine material: the shader assumes the ring lies on _LineWidth / 2.</summary>
        static float LineRadius(Material m, float fallback)
        {
            if (m != null && m.HasProperty("_LineWidth")) { float w = m.GetFloat("_LineWidth"); if (w > 1e-4f) return 0.5f * w; }
            return fallback;
        }

        /// <summary>Convex planar polygon, corners clockwise seen from the front (MeshBuilder convention); fan triangulated.</summary>
        static void Poly(MeshBuilder mb, List<Vector3> pts)
        {
            if (pts.Count < 3) return;
            Vector3 n = Vector3.zero;
            for (int i = 1; i + 1 < pts.Count; i++) n += Vector3.Cross(pts[i] - pts[0], pts[i + 1] - pts[0]);
            if (n.sqrMagnitude < 1e-12f) return;
            n.Normalize();
            int first = mb.VertexCount;
            for (int i = 0; i < pts.Count; i++) mb.Vertex(pts[i], n, new Vector2(pts[i].x, pts[i].z));
            for (int i = 1; i + 1 < pts.Count; i++)
            {
                if (Vector3.Cross(pts[i] - pts[0], pts[i + 1] - pts[0]).sqrMagnitude < 1e-10f) continue;
                mb.Tri(first, first + i, first + i + 1);
            }
        }

        static readonly List<Vector3> s_clipA = new List<Vector3>(8), s_clipB = new List<Vector3>(8);

        /// <summary>Clips the polygon to the half-space x >= xMin (Sutherland-Hodgman) and emits it (order preserved).</summary>
        static void PolyClipX(MeshBuilder mb, Vector3 a, Vector3 b, Vector3 c, Vector3 d, double xMin)
        {
            s_clipA.Clear(); s_clipA.Add(a); s_clipA.Add(b); s_clipA.Add(c); s_clipA.Add(d);
            s_clipB.Clear();
            float xm = (float)xMin;
            for (int i = 0; i < s_clipA.Count; i++)
            {
                Vector3 p = s_clipA[i], q = s_clipA[(i + 1) % s_clipA.Count];
                bool pin = p.x >= xm, qin = q.x >= xm;
                if (pin) s_clipB.Add(p);
                if (pin != qin)
                {
                    float t = (xm - p.x) / (q.x - p.x);
                    s_clipB.Add(Vector3.LerpUnclamped(p, q, t));
                }
            }
            Poly(mb, s_clipB);
        }

        // ---------------------------------------------------------------- quads in lane coordinates
        static Vector3 P(double s, double lat, double y)
        {
            BwPath.World(s, lat, out double x, out _, out double z);
            return new Vector3((float)x, (float)y, (float)z);
        }

        // ================================================================ lamp anchors (LocalLightPool: at most 4 real lights, 14.2.3)
        /// <summary>The shared pool the graybox registered its lamp anchors in (null when "lights" is replaced).</summary>
        public LocalLightPool LightPool { get; private set; }
        readonly List<int> m_anchorIds = new List<int>(96);
        /// <summary>Pool ids of the graybox lamp anchors (QA: the light-continuity test).</summary>
        public IReadOnlyList<int> AnchorIds => m_anchorIds;

        /// <summary>Registers a lamp of part <paramref name="key"/> in the pool (skipped when that part or "lights" is replaced).</summary>
        void AddAnchor(int key, Vector3 p, Pal colour, float range, float intensity, bool lampBoost = false)
        {
            if (m_off[key] || m_off[KeyLights] || LightPool == null) return;
            m_anchorIds.Add(LightPool.Add(p, BulwarkPalette.Linear(colour), range, intensity, "graybox_" + PartKeys[key], 1f, lampBoost));
        }

        // ================================================================ sky openings (LocalLightPool._BwOpen, Wave B)
        LocalLightPool m_openPool;
        int m_doorOpening = -1;
        /// <summary>Reach (m) of the graybox openings: F of a 4 x 3.4 m door at 40 m is about 13.6 / (pi 1600) = 0.0027.</summary>
        public const float OpeningReach = 40f;
        /// <summary>Pool id of the beat-1 door opening (-1 when beat 1 is replaced).</summary>
        public int DoorOpening => m_doorOpening;
        /// <summary>The pool holding the graybox's sky openings (also when "lights" is replaced).</summary>
        public LocalLightPool OpeningPool => m_openPool;

        /// <summary>Registers an opening of part <paramref name="key"/> (skipped when that part is replaced).</summary>
        int AddOpening(int key, string name, double x0, double x1, double y0, double y1, double zPlane, bool exteriorAtPlusZ, bool on)
        {
            if (m_off[key] || m_openPool == null) return -1;
            return m_openPool.AddOpening("graybox_" + PartKeys[key] + "_" + name, (float)x0, (float)x1, (float)y0, (float)y1, (float)zPlane, exteriorAtPlusZ, OpeningReach, on);
        }

        // ================================================================ per frame: the door (progress-keyed, H6)
        public void OnFrame(in FrameState f)
        {
            double u = (f.s - BulwarkConst.DoorTriggerS) / (BulwarkConst.DoorOpenTime * BulwarkConst.Cine);
            float gap = (float)(BulwarkConst.DoorWidth * BwMath.Smootherstep01(u));
            if (Math.Abs(gap - m_lastGap) > 1e-5f) SetDoorGap(gap);
        }

        /// <summary>Door gap (m) at progress s (the same rule as OnFrame).</summary>
        public static double DoorGapAt(double s) => BulwarkConst.DoorWidth * BwMath.Smootherstep01((s - BulwarkConst.DoorTriggerS) / (BulwarkConst.DoorOpenTime * BulwarkConst.Cine));

        /// <summary>Sets the door leaves for progress s (the EditMode audit poses the world per key shot).</summary>
        public void SetDoorForS(double s) => SetDoorGap((float)DoorGapAt(s));

        void SetDoorGap(float gap)
        {
            m_lastGap = gap;
            if (m_openPool != null && m_doorOpening >= 0)
            {
                // the opening between the sliding leaves (centred on the door, 14.5.1 gap 4.0 smootherstep(t / 3.0))
                double cx = 0.5 * (BulwarkConst.DoorOpeningXMin + BulwarkConst.DoorOpeningXMax);
                m_openPool.SetOpeningX(m_doorOpening, (float)(cx - 0.5 * gap), (float)(cx + 0.5 * gap));
                m_openPool.SetOpeningOn(m_doorOpening, gap > 1e-3f);
            }
            if (m_leafW == null || m_leafE == null) return;
            m_leafW.localPosition = m_leafW0 + new Vector3(-0.5f * gap, 0f, 0f);
            m_leafE.localPosition = m_leafE0 + new Vector3(0.5f * gap, 0f, 0f);
        }

        // ================================================================ key-shot subjects (11.13): the audit's spec boxes
        static readonly List<VisibilityAudit.Subject> s_subjects = new List<VisibilityAudit.Subject>(32);

        public void CollectSubjects(string shotId, List<KeyShotSubject> into)
        {
            s_subjects.Clear();
            VisibilityAudit.SubjectsForShot(shotId, s_subjects);
            for (int i = 0; i < s_subjects.Count; i++)
            {
                var sub = s_subjects[i];
                // the capture harness applies "bbox inside the frame" to every subject it gets, so only the audit's Inside
                // subjects go there (long or partly framed subjects, rule InFrame, are judged by the EditMode audit)
                if (sub.Kind != VisibilityAudit.SubjectKind.Box || !sub.MustBeVisible || sub.Rule != VisibilityAudit.Rule.Inside) continue;
                var c = new Vector3((float)(0.5 * (sub.Min.x + sub.Max.x)), (float)(0.5 * (sub.Min.y + sub.Max.y)), (float)(0.5 * (sub.Min.z + sub.Max.z)));
                var size = new Vector3((float)(sub.Max.x - sub.Min.x), (float)(sub.Max.y - sub.Min.y), (float)(sub.Max.z - sub.Min.z));
                into.Add(new KeyShotSubject(sub.Name, new Bounds(c, size)));
            }
        }
    }
}
