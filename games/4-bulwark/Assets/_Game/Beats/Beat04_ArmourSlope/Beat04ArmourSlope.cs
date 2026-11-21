// Beat04ArmourSlope: beat 4, the armour slope (MASTER_PROMPT 14.5.4, s 700-920, 88 s at 2.5 m/s; KS10-KS12): the noon road
// walk z 704-920, the belt of 40 m "Colossopod" armour units, six offshore islets of whole units, the shoal of awash tips to
// x 1600 and north of z 1560, surf and foam on the belt (BwSurf: soft, never a hard-edged patch), gauge platforms, and the
// gate tower's exterior (80 m) ahead. Owner: the reef builder (Wave X).
// Replaces the graybox part "beat4" (the crest z 704-920 with its faces, parapet, lee rail, lamp posts and bollards, the lee
// face below it, the surf strip and the six islet stand-ins, the tower shell): [ReplacesGraybox("beat4")]. The tower's
// sky openings (portal, both arches) move here from the graybox.
// Files: ReefLayout.cs (ReefUnitAt and the islet piles: the one placement function, hull distances for the kraken),
// ColossopodMesh.cs (the unit, LOD0 / LOD1, build-time clipping), Beat04Reef.cs (unit chunks, gauge platforms, surf grids),
// Beat04Crest.cs (the road band), Beat04Tower.cs (the tower shell). Shaders: BwArmour (units; dynamic waterline foam from
// BwSeaWaves), BwSurf (white water on the sea), the library's BwRoad / BwConcrete / BwSteel / BwLine / BwEmissive.
// Switching (14.6 "looking back"): every renderer of this beat is disabled at s >= 960 (back on below 940), like the crest.
// Everything is built once behind the load fade; nothing is generated after load; no allocations per frame.
using System.Collections.Generic;
using UnityEngine;

namespace Megastructure.Beats
{
    [ReplacesGraybox("beat4")]
    public sealed partial class Beat04ArmourSlope : IBeatBuilder, IWorldUpdater, IKeyShotSubjects
    {
        public string Name => "Beat04_ArmourSlope";
        public const string SurfShaderName = "Megastructure/BwSurf";
        public const double SwitchOffS = 960.0, SwitchOnS = 940.0;

        Material m_road, m_concrete, m_dark, m_wet, m_steel, m_ochre, m_rail10, m_post12, m_handrail, m_armour, m_lamp, m_surf, m_gaugeSteel, m_gaugeOchre;
        readonly List<Renderer> m_switch = new List<Renderer>(128);
        bool m_on = true;
        Bounds m_towerBounds;

        /// <summary>Materials created at runtime by this builder.</summary>
        public readonly List<Material> RuntimeMaterials = new List<Material>();
        public Transform Root { get; private set; }
        public long Triangles { get; private set; }

        public void Build(WorldContext ctx)
        {
            var root = new GameObject(Name).transform;
            root.SetParent(ctx.Root, false);
            Root = root;
            CreateMaterials(ctx);
            long t0 = ctx.Triangles;

            BuildCrest(ctx, root);
            long tc = ctx.Triangles;
            BuildTower(ctx, root);
            long tt = ctx.Triangles;
            BuildReef(ctx, root);
            BuildSurf(ctx, root);
            Triangles = ctx.Triangles - t0;
            m_towerBounds = new Bounds(new Vector3(0f, 40f, 938f), new Vector3(13.4f, 80.6f, 37.4f));

            ctx.Register(this);
            ctx.RegisterSubjects(this);
            var ks = BulwarkConst.KeyShots;
            for (int i = 0; i < ks.Length; i++)
                if (ks[i].tag.StartsWith("KS10", System.StringComparison.Ordinal))
                    ctx.PrewarmSets.Add(new PrewarmSet { Name = "Reef", Eye = new Vector3((float)ks[i].x, (float)ks[i].y, (float)ks[i].z), YawDeg = (float)ks[i].yaw, PitchDeg = (float)ks[i].pitch, Enable = null });
            ctx.Log($"Beat04_ArmourSlope: {Triangles} triangles (crest {tc - t0}, tower {tt - tc}, reef {ReefTriangles} in {UnitsBuilt} units, " +
                    $"{GaugeCount} gauges, surf {SurfTriangles}), {m_switch.Count} renderers");
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
            // the reef's own copy of the library Armour material (MaterialFactory stays untouched): weathering tuned for 40 m
            // units seen from 45-1600 m (macro tone +-14 %, chalkier dry concrete, the wet zone to 4 m above the sea)
            m_armour = lib != null && lib.Armour != null ? new Material(lib.Armour) { name = "b4_armour" } : null;
            if (m_armour != null)
            {
                RuntimeMaterials.Add(m_armour);
                SetF(m_armour, "_MacroTone", 0.14f);
                SetF(m_armour, "_Smoothness", 0.20f);
                SetF(m_armour, "_WetHeight", 4.0f);
                SetF(m_armour, "_SwashHeight", 2.6f);
                SetF(m_armour, "_FoamWidth", 0.4f);
            }
            m_lamp = lib != null ? lib.LampTungsten : null;
            if (m_road == null) m_road = m_concrete;
            if (m_armour == null) m_armour = m_dark;
            // the gauges stand 37-300 m away: their 1 cm chamfers carry the library's edge wear (bare, smooth steel) and turned
            // into lone sky-bright pixels (reef-05..07 look_r): copies without edge wear and with a matt paint
            m_gaugeSteel = PaintCopy(m_steel, "b4_gauge_steel");
            m_gaugeOchre = PaintCopy(m_ochre, "b4_gauge_ochre");
            m_rail10 = LineCopy(lib != null ? lib.Rail : null, "b4_rail10", (float)BulwarkConst.LeeRailDiameter);
            m_post12 = LineCopy(lib != null ? lib.Rail : null, "b4_post12", (float)BulwarkConst.LeeRailPostSize);
            m_handrail = LineCopy(lib != null ? (lib.Handrail != null ? lib.Handrail : lib.Rail) : null, "b4_gauge_rail", 0.05f);
            Shader sh = Shader.Find(SurfShaderName);
            if (sh != null)
            {
                m_surf = new Material(sh) { name = "b4_surf" };
                m_surf.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Geometry + 150;
                RuntimeMaterials.Add(m_surf);
            }
            else Debug.LogError("[BW-FAIL] Beat04_ArmourSlope: shader " + SurfShaderName + " not found (Always Included? run RebuildAll): no surf");
        }

        static void SetF(Material m, string prop, float v) { if (m.HasProperty(prop)) m.SetFloat(prop, v); }

        Material PaintCopy(Material src, string name)
        {
            if (src == null) return null;
            var m = new Material(src) { name = name };
            SetF(m, "_EdgeWear", 0f);
            SetF(m, "_ChipAmount", 0f);
            SetF(m, "_Smoothness", 0.32f);
            RuntimeMaterials.Add(m);
            return m;
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

        public void OnFrame(in FrameState f)
        {
            bool on = m_on ? f.s < SwitchOffS : f.s < SwitchOnS;
            if (on == m_on) return;
            m_on = on;
            for (int i = 0; i < m_switch.Count; i++)
                if (m_switch[i] != null) m_switch[i].enabled = on;
        }

        /// <summary>The visible pile of islet k: 0.9 R round its crest centre, from the mean sea to the crest.</summary>
        public static Bounds IsletPile(int k)
        {
            Vector2 c = ReefLayout.IsletCentre(k);
            float r = 0.9f * ReefLayout.IsletRadius[k], top = ReefLayout.IsletCrestY[k], sea = (float)BulwarkConst.SeaY;
            return new Bounds(new Vector3(c.x, 0.5f * (sea + top), c.y), new Vector3(2f * r, top - sea, 2f * r));
        }

        public void CollectSubjects(string shotId, List<KeyShotSubject> into)
        {
            if (shotId == null) return;
            if (shotId.StartsWith("KS10", System.StringComparison.Ordinal))
            {
                // the two nearest islets must lie whole inside the frame (14.5.4 acceptance 2): the visible pile (0.9 R round
                // the crest centre, from the sea to the crest), not the renderer bounds (those hold the submerged arms too)
                // islets 2 and 3 (bearings 78 and 54 deg) lie whole inside every aspect; islet 1 (bearing 101.9, its pile
                // +-17 deg at 150 m) is the near giant at the right edge: its crest unit must be inside
                into.Add(new KeyShotSubject("islet2", IsletPile(1), 0.01f));
                into.Add(new KeyShotSubject("islet3", IsletPile(2), 0.01f));
                var c1 = ReefLayout.Islet(0)[0];
                into.Add(new KeyShotSubject("islet1_crest_unit", new Bounds(new Vector3(c1.centre.x, 0.5f * (c1.TopY + (float)BulwarkConst.SeaY), c1.centre.z),
                                                                           new Vector3(40f, c1.TopY - (float)BulwarkConst.SeaY, 40f)), 0.01f));
            }
            else if (shotId.StartsWith("KS12", System.StringComparison.Ordinal))
                into.Add(new KeyShotSubject("tower", m_towerBounds, 0.5f));
            else if (shotId.StartsWith("KS11", System.StringComparison.Ordinal))
                into.Add(new KeyShotSubject("tower_foot", new Bounds(new Vector3(0f, 15f, 938f), new Vector3(12f, 30f, 36f)), 0.05f));
        }
    }
}
