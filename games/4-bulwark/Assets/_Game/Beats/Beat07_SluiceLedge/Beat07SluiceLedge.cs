// Beat07SluiceLedge: beat 7, the sluice tunnel and the lee ledge (MASTER_PROMPT 14.5.7, MONEY SHOT 4: KS19, KS20, KS21;
// 14.11 the lantern goal). Replaces the graybox of beat 7 ([ReplacesGraybox("beat7")]). Files:
//   Beat07Layout.cs      pure geometry (double, no UnityEngine; Beat07Tests)
//   Beat07SluiceLedge.cs core: build order, materials (library + runtime BwConduit / iron / cold lamp), per-frame updates
//                        (conduit flow offsets in double: no 3600 s seam; lantern flicker on its real light), key-shot
//                        subjects, mesh helpers (lane-coordinate quads clipped to the face plane, tubes, prisms)
//   Beat07Tunnel.cs      SluiceBuilder: tunnel shell (ConcreteWet floor, ConcreteDark walls and soffit with
//                        corner AO), the 43 m east grating panel with its frame, SodiumWarm cages every 6 m (LocalLightPool
//                        anchors), the conduit (BwConduit water and walls, cold wall lights, culvert, drop shaft, trash rack)
//   Beat07Ledge.cs       LedgeBuilder + LanternBuilder: the near lee face with the mouth and the ring pockets, the ledge slab
//                        with kerb, brackets, rails (BwLine tubes 0.10) and mooring rings, the Keeper's Lantern
// Light: the tunnel cages and the lantern register anchors with LocalLightPool (at most 4 real local lights, 14.2.3);
// the conduit's cold lights are analytic in BwConduit (they only light the conduit). Sky, fog, exposure and the dusk key
// come from AtmosphereDriver (one writer); the far field (lowland, jets, city lights, the face beyond this mesh) is
// BwFarField. Everything is built once behind the load fade; per frame only material floats and one light gain change.
using System;
using System.Collections.Generic;
using Megastructure.Rendering;
using UnityEngine;

namespace Megastructure.Beats
{
    [ReplacesGraybox("beat7")]
    public sealed partial class Beat07SluiceLedge : IBeatBuilder, IWorldUpdater, IKeyShotSubjects
    {
        public string Name => "Beat07_SluiceLedge";
        public const string ConduitShader = "Megastructure/BwConduit";
        /// <summary>Macro tone amplitude of the near lee face (BwConcrete _MacroTone); BwFarField's analytic face uses 0.10 too.</summary>
        public const float FaceMacroTone = 0.10f;

        // ---- materials
        Material m_dark, m_wet, m_steel, m_iron, m_grating, m_gratingWall, m_rail, m_cageLine, m_sodium, m_lantern, m_face, m_endWall, m_slab;
        /// <summary>Paint of the tunnel grating relative to SteelPaint (sooty, dark: the water behind it is the subject of KS19).</summary>
        public const float GratingShade = 0.5f;
        /// <summary>Wetness of the ledge slab (BwConcrete _Wet): damp, a sheen of the dusk sky, not puddles.</summary>
        public const float SlabWetness = 0.35f;
        Material m_water, m_cwall;
        /// <summary>Materials created at runtime by this builder (the EditMode audit destroys them).</summary>
        public readonly List<Material> RuntimeMaterials = new List<Material>();
        public Transform Root { get; private set; }

        LocalLightPool m_pool;
        int m_lanternLight = -1;
        double m_lastFlowT = double.NaN;

        static readonly int IdFlow = Shader.PropertyToID("_FlowOffset");
        static readonly int IdVeil = Shader.PropertyToID("_Veil");
        static readonly int IdMouth0 = Shader.PropertyToID("_BwMouth0");
        static readonly int IdMouth1 = Shader.PropertyToID("_BwMouth1");

        // ================================================================ build
        public void Build(WorldContext ctx)
        {
            var root = new GameObject(Name).transform;
            root.SetParent(ctx.Root, false);
            Root = root;
            CreateMaterials(ctx);
            m_pool = LocalLightPool.Ensure(ctx);

            BuildTunnel(ctx, root);       // Beat07Tunnel.cs
            BuildConduit(ctx, root);      // Beat07Tunnel.cs
            BuildFaceAndLedge(ctx, root); // Beat07Ledge.cs (the lantern moved to the lantern platform: Beat08Ending)

            ConduitSpray.Publish();
            // the mouth as an area light of sky radiance for the tunnel's threshold zone (BwMouthLight in BulwarkAtmosphere.hlsl)
            Beat07Layout.Mouth(out double mz0, out double mz1);
            Shader.SetGlobalVector(IdMouth0, new Vector4((float)Beat07Layout.FaceX, (float)mz0, (float)mz1, 1f));
            Shader.SetGlobalVector(IdMouth1, new Vector4((float)Beat07Layout.FloorY, (float)Beat07Layout.CeilingY, (float)Beat07Layout.MouthLightReach, 0f));
            ctx.PrewarmSets.Add(Prewarm("Tunnel7", BulwarkConst.KeyShots[18]));
            ctx.PrewarmSets.Add(Prewarm("Ledge", BulwarkConst.KeyShots[19]));
            ctx.Register(this);
            ctx.RegisterSubjects(this);
            ctx.Log("Beat07_SluiceLedge: tunnel s " + Beat07Layout.TunnelS0.ToString("0.0") + "-" + BulwarkConst.TunnelMouthS.ToString("0.00")
                    + ", conduit, ledge to the elevator top station (" + m_tris + " triangles)");
        }

        static PrewarmSet Prewarm(string name, KeyShot k) => new PrewarmSet
        {
            Name = name, Eye = new Vector3((float)k.x, (float)k.y, (float)k.z), YawDeg = (float)k.yaw, PitchDeg = (float)k.pitch, Enable = _ => { },
        };

        // ---------------------------------------------------------------- materials
        void CreateMaterials(WorldContext ctx)
        {
            var lib = ctx.Materials;
            m_dark = lib != null ? lib.ConcreteDark : null;
            m_wet = lib != null ? lib.ConcreteWet : null;
            m_steel = lib != null ? lib.Steel : null;
            m_grating = lib != null ? lib.Grating : null;
            m_sodium = lib != null ? lib.LampSodium : null;
            m_lantern = lib != null ? lib.Lantern : null;
            m_endWall = lib != null ? lib.Concrete : null;   // the hall side of the end wall: the hall's concrete

            // the near lee face: ConcreteDark without the rain follow (the analytic face beyond it has none either: no tier
            // mismatch at its edges, Part 10 #34)
            m_face = Copy(m_dark, "b7_face_dark");
            if (m_face != null && m_face.HasProperty("_WetFollow")) m_face.SetFloat("_WetFollow", 0f);
            // a 90 m weathered face: macro tone +-10 % (the analytic face uses the same amplitude) and full drip stains
            if (m_face != null && m_face.HasProperty("_MacroTone")) m_face.SetFloat("_MacroTone", FaceMacroTone);
            if (m_face != null && m_face.HasProperty("_DripStrength")) m_face.SetFloat("_DripStrength", 1.0f);
            // wall grating: bars across the tunnel's own axis (BwGrating _AcrossDir when the shader has it; integrate owner)
            m_gratingWall = Copy(m_grating, "b7_grating_wall");
            if (m_gratingWall != null)
            {
                BwPath.Tangent(Beat07Layout.PanelS0, out double tx, out double tz);
                m_gratingWall.SetVector("_AcrossDir", new Vector4((float)tx, 0f, (float)tz, 1f));
                // a darker, sooty paint on the bars (SteelPaint x0.5): at 1.8 m (KS19) the 40 mm bars cover half the view and
                // the sodium cages lit them as bright ochre slats in front of the water; dark bars let the lit water read
                if (m_gratingWall.HasProperty("_BaseColor")) m_gratingWall.SetColor("_BaseColor", Shade(BulwarkPalette.Linear(Pal.SteelPaint), GratingShade));
            }
            // the ledge slab: damp concrete (mist from the outlets and the conduit, the dew of the evening): darker, with a
            // sheen that mirrors the dusk sky toward the far end of the ledge (BwConcrete _Wet, no rain follow)
            m_slab = Copy(m_dark, "b7_ledge_slab");
            if (m_slab != null)
            {
                if (m_slab.HasProperty("_WetFollow")) m_slab.SetFloat("_WetFollow", 0f);
                if (m_slab.HasProperty("_Wet")) m_slab.SetFloat("_Wet", SlabWetness);
            }
            // dark iron (lantern, rings): BwSteel recoloured to the basalt token
            m_iron = Copy(m_steel, "b7_iron");
            if (m_iron != null)
            {
                m_iron.SetColor("_BaseColor", BulwarkPalette.Linear(Pal.ConcreteDark));
                if (m_iron.HasProperty("_ChipAmount")) m_iron.SetFloat("_ChipAmount", 0.05f);
            }
            // rails: round tubes 0.10 m (14.5.7); the library Rail may still be 0.05
            m_rail = lib != null ? lib.Rail : null;
            if (m_rail != null && m_rail.HasProperty("_LineWidth") && Mathf.Abs(m_rail.GetFloat("_LineWidth") - (float)Beat07Layout.RailDiameter) > 1e-4f)
            {
                m_rail = Copy(m_rail, "b7_rail_010");
                m_rail.SetFloat("_LineWidth", (float)Beat07Layout.RailDiameter);
            }
            // lantern cage bars: dark iron BwLine tubes 0.02 m (thin lines through BwLine, H7)
            m_cageLine = Copy(lib != null ? lib.Rail : null, "b7_cage_line");
            if (m_cageLine != null)
            {
                m_cageLine.SetFloat("_LineWidth", 0.02f);
                m_cageLine.SetColor("_BaseColor", BulwarkPalette.Linear(Pal.ConcreteDark));
            }

            // BwConduit (this owner's shader): water and walls of the conduit
            var sh = Shader.Find(ConduitShader);
            if (sh == null) Debug.LogError("[BW-FAIL] Beat07: shader " + ConduitShader + " not found (RebuildAll: Always Included Shaders)");
            else
            {
                m_water = NewConduit(sh, "b7_conduit_water", 0f);
                m_cwall = NewConduit(sh, "b7_conduit_wall", 1f);
                if (m_wet != null && m_cwall != null)
                {
                    foreach (var t in new[] { "_AlbedoMap", "_NormalMap", "_MaskMap" })
                        if (m_wet.HasProperty(t) && m_cwall.HasProperty(t)) m_cwall.SetTexture(t, m_wet.GetTexture(t));
                }
            }
        }

        static Color Shade(Color c, float k) => new Color(c.r * k, c.g * k, c.b * k, 1f);

        Material Copy(Material src, string name)
        {
            if (src == null) return null;
            var m = new Material(src) { name = name };
            RuntimeMaterials.Add(m);
            return m;
        }

        Material NewConduit(Shader sh, string name, float mode)
        {
            var m = new Material(sh) { name = name };
            m.SetFloat("_Mode", mode);
            Beat07Layout.ConduitFrame(out double ox, out double oz, out double tx, out double tz, out double rx, out double rz);
            m.SetVector("_Frame0", new Vector4((float)ox, (float)Beat07Layout.FloorY, (float)oz, (float)Beat07Layout.EndU));
            m.SetVector("_Frame1", new Vector4((float)tx, (float)tz, (float)rx, (float)rz));
            m.SetVector("_Dims0", new Vector4((float)Beat07Layout.ConduitV0, (float)Beat07Layout.ConduitV1, (float)Beat07Layout.WaterY, (float)Beat07Layout.ConduitFloorY));
            m.SetVector("_Dims1", new Vector4((float)Beat07Layout.ConduitCeilingY, (float)Beat07Layout.ShaftU0, (float)Beat07Layout.ColdLampPitch, (float)Beat07Layout.ColdLampU0));
            m.SetVector("_Dims2", new Vector4((float)Beat07Layout.ColdLampY, (float)Beat07Layout.ColdLampLength, (float)Beat07Layout.CulvertU, (float)Beat07Layout.PanelTopY));
            // tunnel cages seen from the conduit (sodium spill through the panel): first s in conduit u, pitch, lateral, y
            m.SetVector("_Spill", new Vector4((float)(Beat07Layout.LampS0 - Beat07Layout.PanelS0), (float)BulwarkConst.TunnelLampPitch,
                                              (float)Beat07Layout.LampLateral, (float)(Beat07Layout.CeilingY - 0.15)));
            // the cold wall lights: half CityCool, half FoamWhite (a cool white: the water reads white-green, not blue)
            m.SetColor("_ColdColor", Color.Lerp(BulwarkPalette.Linear(Pal.CityCool), BulwarkPalette.Linear(Pal.FoamWhite), 0.5f));
            m.SetColor("_SodiumColor", BulwarkPalette.Linear(Pal.SodiumWarm));
            m.SetColor("_WaterColor", BulwarkPalette.Linear(Pal.SeaTeal));
            m.SetColor("_FoamColor", BulwarkPalette.Linear(Pal.FoamWhite));
            m.SetColor("_WallColor", BulwarkPalette.Linear(Pal.ConcreteWet));
            m.SetColor("_RefColor", BulwarkPalette.Linear(Pal.ConcreteMid));
            m.SetFloat(IdVeil, ConduitSpray.VeilStrength);
            RuntimeMaterials.Add(m);
            return m;
        }

        // ================================================================ per frame
        public void OnFrame(in FrameState f)
        {
            // conduit flow offsets in double from the unwrapped ambient clock, wrapped on exact periods of the shader's noise
            // lattice (64 cells of 3.0 m along the flow) and of the roll waves (9 m): no seam at the 3600 s wrap (Part 10 #33)
            double t = f.ambientTime;
            if (t != m_lastFlowT)
            {
                m_lastFlowT = t;
                const double P = 64.0 * 3.0, Lroll = 9.0;
                var v = new Vector4((float)Mod(Beat07Layout.FlowSpeed * t, P), (float)Mod(11.0 * t, P), (float)Mod(15.5 * t, Lroll), (float)Mod(0.35 * t, P));
                if (m_water != null) m_water.SetVector(IdFlow, v);
                if (m_cwall != null) m_cwall.SetVector(IdFlow, v);
            }
            float veil = ConduitSpray.VeilStrength;
            if (m_water != null && m_water.GetFloat(IdVeil) != veil) { m_water.SetFloat(IdVeil, veil); if (m_cwall != null) m_cwall.SetFloat(IdVeil, veil); }
            // lantern flicker on its real light (the glass shader evaluates the same sum on _BwTime.x, 14.2.5)
            if (m_pool != null && m_lanternLight >= 0) m_pool.SetGain(m_lanternLight, (float)LanternFlicker.Gain(t));
            // the near face panel (z 1000-1800, the top 120 m) is the far field's job beyond 1.8 km (the descent's lower half, the
            // shuttle, the lantern platform): lit and fogged as a near mesh it read 1.7x brighter than the far face round it at
            // 6.9 km (a pale 800 m bar on the crest of the money shot, ending-15 KS28c: 176 vs 103 sRGB red). Beyond 2.2 km the
            // whole beat (ledge, rails, tunnel, lantern) goes too: at 6.9 km its rails and lamps were lone bright sub-pixels on
            // the crest line (ending-22 KS28 4K: one NaN-like pixel at (1947, 102)); the far face stands in for it
            Vector3 d = f.cameraPos - new Vector3((float)Beat07Layout.FaceX, -60f, 1400f);
            float d2 = d.sqrMagnitude;
            if (m_faceMr != null)
            {
                bool off = d2 > FaceFarCull * FaceFarCull;
                if (m_faceMr.forceRenderingOff != off) m_faceMr.forceRenderingOff = off;
            }
            bool allOff = d2 > BeatFarCull * BeatFarCull;
            if (allOff != m_allOff && Root != null)
            {
                m_allOff = allOff;
                if (m_all == null) m_all = Root.GetComponentsInChildren<Renderer>(true);
                for (int i = 0; i < m_all.Length; i++)
                    if (m_all[i] != null && m_all[i] != m_faceMr) m_all[i].forceRenderingOff = allOff;
            }
        }

        Renderer m_faceMr;
        Renderer[] m_all;
        bool m_allOff;
        const float FaceFarCull = 1800f, BeatFarCull = 2200f;

        static double Mod(double a, double p) { double r = a - p * Math.Floor(a / p); return r; }

        // ================================================================ key-shot subjects (11.13)
        public void CollectSubjects(string shotId, List<KeyShotSubject> into)
        {
            if (shotId != "KS20" && shotId != "KS21") return;
            // the destination glow is now the lit elevator cabin at the ledge end (ADDENDUM A1 3.4 / 00): its interior box
            var c = new Vector3((float)(0.5 * (BulwarkConst.CabinX0 + BulwarkConst.CabinX1)), (float)(Beat07Layout.FloorY + 0.5 * BulwarkConst.CabinHeight),
                                (float)(0.5 * (BulwarkConst.CabinZ0 + BulwarkConst.CabinZ1)));
            into.Add(new KeyShotSubject("b7_cabin", new Bounds(c, new Vector3((float)(BulwarkConst.CabinX1 - BulwarkConst.CabinX0), (float)BulwarkConst.CabinHeight,
                                                                                (float)(BulwarkConst.CabinZ1 - BulwarkConst.CabinZ0)))));
        }

        // ================================================================ mesh helpers
        int m_tris;

        void Flush(WorldContext ctx, Transform root, ref MeshBuilder mb, string name, Material mat, bool shadows = false)
        {
            if (mb.VertexCount > 0 && mat != null)
            {
                var mr = ctx.AddMesh(root, name, mb.ToMesh(name), mat, shadows, true);
                mr.shadowCastingMode = shadows ? UnityEngine.Rendering.ShadowCastingMode.On : UnityEngine.Rendering.ShadowCastingMode.Off;
                m_tris += mb.TriangleCount;
                if (name == "b7_lee_face") m_faceMr = mr;
            }
            mb = new MeshBuilder();
        }

        static Vector3 P(double s, double lat, double y)
        {
            BwPath.World(s, lat, out double x, out _, out double z);
            return new Vector3((float)x, (float)y, (float)z);
        }

        static Vector3 C(double u, double v, double y)
        {
            Beat07Layout.ConduitWorld(u, v, y, out double x, out double wy, out double z);
            return new Vector3((float)x, (float)wy, (float)z);
        }

        internal static Color32 Ao(byte r) => new Color32(r, 0, 0, 255);
        internal static readonly Color32 Open = new Color32(255, 0, 0, 255);

        static readonly List<Vector3> s_pa = new List<Vector3>(8), s_pb = new List<Vector3>(8);
        static readonly List<Color32> s_ca = new List<Color32>(8), s_cb = new List<Color32>(8);

        /// <summary>Quad a, b, c, d (clockwise seen from the front) with per-corner colours, clipped to x >= xMin when clip.</summary>
        internal static void Emit(MeshBuilder mb, Vector3 a, Vector3 b, Vector3 c, Vector3 d, Color32 ca, Color32 cb, Color32 cc, Color32 cd, bool clip, double xMin = Beat07Layout.FaceX)
        {
            s_pa.Clear(); s_ca.Clear();
            s_pa.Add(a); s_pa.Add(b); s_pa.Add(c); s_pa.Add(d);
            s_ca.Add(ca); s_ca.Add(cb); s_ca.Add(cc); s_ca.Add(cd);
            if (clip)
            {
                float xm = (float)xMin;
                s_pb.Clear(); s_cb.Clear();
                for (int i = 0; i < s_pa.Count; i++)
                {
                    Vector3 p = s_pa[i], q = s_pa[(i + 1) % s_pa.Count];
                    bool pin = p.x >= xm, qin = q.x >= xm;
                    if (pin) { s_pb.Add(p); s_cb.Add(s_ca[i]); }
                    if (pin != qin)
                    {
                        float t = (xm - p.x) / (q.x - p.x);
                        s_pb.Add(Vector3.LerpUnclamped(p, q, t));
                        s_cb.Add(Color32.Lerp(s_ca[i], s_ca[(i + 1) % s_ca.Count], t));
                    }
                }
                Poly(mb, s_pb, s_cb);
            }
            else Poly(mb, s_pa, s_ca);
        }

        internal static void Emit(MeshBuilder mb, Vector3 a, Vector3 b, Vector3 c, Vector3 d, bool clip) => Emit(mb, a, b, c, d, Open, Open, Open, Open, clip);

        /// <summary>Convex planar polygon (clockwise from the front), fan triangulated, flat normal, per-vertex colours.</summary>
        static void Poly(MeshBuilder mb, List<Vector3> pts, List<Color32> cols)
        {
            if (pts.Count < 3) return;
            Vector3 n = Vector3.zero;
            for (int i = 1; i + 1 < pts.Count; i++) n += Vector3.Cross(pts[i] - pts[0], pts[i + 1] - pts[0]);
            if (n.sqrMagnitude < 1e-14f) return;
            n.Normalize();
            Color32 saved = mb.Color;
            int first = mb.VertexCount;
            for (int i = 0; i < pts.Count; i++) { mb.Color = cols[i]; mb.Vertex(pts[i], n); }
            mb.Color = saved;
            for (int i = 1; i + 1 < pts.Count; i++)
            {
                if (Vector3.Cross(pts[i] - pts[0], pts[i + 1] - pts[0]).sqrMagnitude < 1e-12f) continue;
                mb.Tri(first, first + i, first + i + 1);
            }
        }

        /// <summary>Round tube from a to b (BwLine _LINE_TUBE contract, D-07): smooth outward normals, a ring at least every
        /// 2 m, open ends (the line shader discards back faces).</summary>
        internal static void Tube(MeshBuilder mb, Vector3 a, Vector3 b, float radius, int sides = 10)
        {
            Vector3 d = b - a;
            float len = d.magnitude;
            if (len < 1e-4f || radius <= 0f) return;
            Vector3 dir = d / len;
            Vector3 h = Mathf.Abs(dir.y) < 0.9f ? Vector3.up : Vector3.right;
            Vector3 u = Vector3.Cross(h, dir).normalized, v = Vector3.Cross(dir, u);
            int rings = Mathf.Max(2, Mathf.CeilToInt(len / 2f) + 1);
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
                    int A = first + i * sides + j, D = first + i * sides + jn, Cc = first + (i + 1) * sides + jn, B = first + (i + 1) * sides + j;
                    mb.Quad(A, D, Cc, B);
                }
        }

        /// <summary>Regular prism (n sides) around a vertical axis: radius to the face centres (across flats / 2), from y0 to y1,
        /// with caps; flat-shaded; the top cap may shrink to rTop (a frustum: roofs, collars).</summary>
        internal static void Prism(MeshBuilder mb, Vector3 centreBottom, float rBottom, float rTop, float height, int n, float startDeg = 22.5f, bool capTop = true, bool capBottom = true)
        {
            float k0 = rBottom / Mathf.Cos(Mathf.PI / n), k1 = rTop / Mathf.Cos(Mathf.PI / n);   // corner radii
            Vector3 top = centreBottom + Vector3.up * height;
            for (int i = 0; i < n; i++)
            {
                float a0 = (startDeg + 360f * i / n) * Mathf.Deg2Rad, a1 = (startDeg + 360f * (i + 1) / n) * Mathf.Deg2Rad;
                Vector3 d0 = new Vector3(Mathf.Cos(a0), 0f, Mathf.Sin(a0)), d1 = new Vector3(Mathf.Cos(a1), 0f, Mathf.Sin(a1));
                Vector3 b0 = centreBottom + d0 * k0, b1 = centreBottom + d1 * k0, t0 = top + d0 * k1, t1 = top + d1 * k1;
                // side face, clockwise seen from outside (a1 > a0 runs counter-clockwise seen from above, so b0 is on the
                // viewer's left): b0, t0, t1, b1
                if (k1 > 1e-5f) mb.FlatQuad(b0, t0, t1, b1);
                else
                {
                    Vector3 nrm = Vector3.Cross(t0 - b0, b1 - b0).normalized;
                    int i0 = mb.Vertex(b0, nrm), i1 = mb.Vertex(t0, nrm), i2 = mb.Vertex(b1, nrm);
                    mb.Tri(i0, i1, i2);
                }
                if (capTop && k1 > 1e-5f)
                {
                    int c = mb.Vertex(top, Vector3.up), p0 = mb.Vertex(t0, Vector3.up), p1 = mb.Vertex(t1, Vector3.up);
                    mb.Tri(c, p1, p0);
                }
                if (capBottom)
                {
                    int c = mb.Vertex(centreBottom, Vector3.down), p0 = mb.Vertex(b0, Vector3.down), p1 = mb.Vertex(b1, Vector3.down);
                    mb.Tri(c, p0, p1);
                }
            }
        }

        internal static void BoxMinMax(MeshBuilder mb, double x0, double y0, double z0, double x1, double y1, double z1)
        {
            var c = new Vector3((float)(0.5 * (x0 + x1)), (float)(0.5 * (y0 + y1)), (float)(0.5 * (z0 + z1)));
            var size = new Vector3((float)Math.Abs(x1 - x0), (float)Math.Abs(y1 - y0), (float)Math.Abs(z1 - z0));
            mb.Box(c, size);
        }
    }

    /// <summary>
    /// The conduit spray hook for the mist owner (MistVolumeFeature, 14.7 H2 "the beat 7 conduit spray"): the spray volume
    /// of the beat-7 conduit as an oriented box in conduit coordinates (u along the flow from the grating panel's south end,
    /// v across, y world), its density law and its light. BwConduit already draws an in-shader spray veil along every view
    /// ray inside the conduit (VeilStrength 1); when the mist feature integrates this emitter itself it sets VeilStrength to
    /// 0 (Beat07SluiceLedge applies it to the conduit materials every frame), so the spray is never counted twice.
    /// </summary>
    public static class ConduitSpray
    {
        /// <summary>True once beat 7 is built.</summary>
        public static bool Active;
        /// <summary>World origin of the conduit frame: the panel's south end on the tunnel axis at the tunnel floor (y -90).</summary>
        public static Vector3 Origin;
        /// <summary>Unit plan axes: AxisU along the flow (the tunnel tangent, heading -18), AxisV across (to the conduit).</summary>
        public static Vector3 AxisU, AxisV;
        /// <summary>Box in (u, v, y): u [culvert, end wall], v [panel, far wall], y [water surface, conduit ceiling].</summary>
        public static Vector3 LocalMin, LocalMax;
        /// <summary>Flow speed (m/s) along +u.</summary>
        public static float FlowSpeed;
        /// <summary>Extinction at the water surface (1/m) and its scale height (m): sigma(y) = Sigma0 exp(-(y - water) / H);
        /// the drop shaft (u from ShaftU) adds a plume of 3 x Sigma0 rising to the ceiling.</summary>
        public static float Sigma0 = 0.16f, ScaleHeight = 2.0f, ShaftU;
        /// <summary>Light of the spray: the cold wall lights (linear colour x radiance), every LampPitch m from LampU0 at LampY.</summary>
        public static Color LightColor;
        public static float LampPitch, LampU0, LampY;
        /// <summary>Strength of the BwConduit in-shader veil (1 = on; the mist feature sets 0 when it draws this emitter).</summary>
        public static float VeilStrength = 1f;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics() { Active = false; VeilStrength = 1f; }

        internal static void Publish()
        {
            Beat07Layout.ConduitFrame(out double ox, out double oz, out double tx, out double tz, out double rx, out double rz);
            Origin = new Vector3((float)ox, (float)Beat07Layout.FloorY, (float)oz);
            AxisU = new Vector3((float)tx, 0f, (float)tz);
            AxisV = new Vector3((float)rx, 0f, (float)rz);
            LocalMin = new Vector3((float)Beat07Layout.CulvertU, (float)Beat07Layout.ConduitV0, (float)Beat07Layout.WaterY);
            LocalMax = new Vector3((float)Beat07Layout.EndU, (float)Beat07Layout.ConduitV1, (float)Beat07Layout.ConduitCeilingY);
            FlowSpeed = (float)Beat07Layout.FlowSpeed;
            ShaftU = (float)Beat07Layout.ShaftU0;
            LightColor = Color.Lerp(BulwarkPalette.Linear(Pal.CityCool), BulwarkPalette.Linear(Pal.FoamWhite), 0.5f) * 3.0f;
            LampPitch = (float)Beat07Layout.ColdLampPitch; LampU0 = (float)Beat07Layout.ColdLampU0; LampY = (float)Beat07Layout.ColdLampY;
            Active = true;
        }
    }
}
