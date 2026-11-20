// NocMaterialSheet: the material sheet of MASTER_PROMPT 11.2 / 8.11 item 8 and the Part 8.6 lit-template test scene in one
// IBeatBuilder (discovered by GameBootstrap like every builder). It builds only with the flag -material-sheet, south of the
// alley start (behind the KS00 camera, z -64..-30), so normal runs never see it:
//   - every NocSurface material of the library as a standing chamfered slab (2.0 x 2.6 x 0.4 m, 1-3 cm bevels, edge wear
//     on the chamfers) with a sphere (r 0.65 m) in front, plus the glass pair (back faces 3000, front faces 3001);
//   - the five NocWet floors as 4.4 x 6 m tiles (1 cm proud of a FloorStone ground, never co-planar) in the foreground;
//   - a night key: the scene's shadowless sky fill (main light) + 4 point lights (2 SodiumAmber, 2 NeonCyan at y 3.4, 3.5 m in front of the slabs, r 12, i 9) under
//     Forward+, each with a visible emissive head and a glow-disc wet source, plus one vertical cyan tube with a segment
//     source (the H4 "lamp reflection reads as a vertical streak" check) and a back wall registered as a wet wall plane.
// Captures (one pose per run, 1080p, -freeze-clock): pose:sheetL:-6.4:1.7:-41:180:-14:48, pose:sheetR:6.4:1.7:-41:180:-14:48,
// pose:sheetW:0:1.7:-33:180:-4:50 (grazing floor reflections). One [NOC] line reports the Forward+ keyword and the light count.
using UnityEngine;
using UnityEngine.Rendering;

namespace Megastructure
{
    public sealed class NocMaterialSheet : IBeatBuilder, IWorldUpdater
    {
        public const string Flag = "-material-sheet";
        public const float WallZ = -61f, ItemZ = -54f, SphereZ = -51.8f, TileZ0 = -46f, TileZ1 = -40f;

        public string Name => "ZZ_MaterialSheet";
        int m_frames;
        bool m_logged;

        public void Build(WorldContext ctx)
        {
            if (!CliArgs.Has(Flag)) return;
            var lib = ctx.Materials;
            if (lib == null) { Debug.LogError("[NOC-FAIL] material sheet: no MaterialLibrary"); return; }
            var root = new GameObject("MaterialSheet").transform;
            root.SetParent(ctx.CityRoot, false);
            const float bevel = 0.03f;

            // ground and back wall
            var mb = new MeshBuilder();
            MeshKit.ChamferBox(mb, new Vector3(0f, -0.25f, -47f), new Vector3(48f, 0.5f, 34f), bevel);
            ctx.AddMesh(root, "ground", mb.ToMesh("Sheet_ground"), lib.FloorStone);
            mb = new MeshBuilder();
            MeshKit.ChamferBox(mb, new Vector3(0f, 5f, WallZ - 0.5f), new Vector3(44f, 10f, 1f), bevel);
            ctx.AddMesh(root, "wall", mb.ToMesh("Sheet_wall"), lib.Concrete);

            // floor tiles (NocWet), 1 cm proud of the ground
            Material[] floors = { lib.FloorAlley, lib.FloorStone, lib.FloorSteelDeck, lib.FloorGardenPath, lib.FloorPlaza };
            for (int i = 0; i < floors.Length; i++)
            {
                mb = new MeshBuilder();
                float x = -11f + 5.5f * i;
                MeshKit.ChamferBox(mb, new Vector3(x, -0.095f, 0.5f * (TileZ0 + TileZ1)), new Vector3(4.4f, 0.21f, TileZ1 - TileZ0), 0.01f);
                ctx.AddMesh(root, "floor_" + i, mb.ToMesh("Sheet_floor_" + i), floors[i]);
            }

            // surfaces: slab + sphere per material, glass as two renderers (back faces then front faces)
            Material[] mats = { lib.Concrete, lib.StoneWall, lib.SteelPainted, lib.SteelBare, lib.Brass, lib.Moss, lib.Bark, lib.Asphalt, lib.GlassBack };
            for (int i = 0; i < mats.Length; i++)
            {
                float x = -12.8f + 3.2f * i;
                mb = new MeshBuilder();
                MeshKit.ChamferBox(mb, new Vector3(x, 1.3f, ItemZ), new Vector3(2.0f, 2.6f, 0.4f), bevel, Quaternion.Euler(0f, 18f, 0f));
                MeshKit.Sphere(mb, new Vector3(x, 0.7f, SphereZ), 0.65f, 48, 24);
                var mesh = mb.ToMesh("Sheet_item_" + i);
                ctx.AddMesh(root, "item_" + i, mesh, mats[i]);
                if (mats[i] == lib.GlassBack) ctx.AddMesh(root, "item_" + i + "_front", mesh, lib.GlassFront);
            }

            // Look v2 rows (SU): the GlassDark pane slab left of the row and an
            // open Grating plate (2.0 x 2.6 m, horizontal at y 1.2, the floor tile below shows through it) right of it
            {
                if (lib.GlassDark != null)
                {
                    mb = new MeshBuilder();
                    MeshKit.ChamferBox(mb, new Vector3(-16f, 1.3f, ItemZ), new Vector3(2.0f, 2.6f, 0.4f), bevel, Quaternion.Euler(0f, 18f, 0f));
                    ctx.AddMesh(root, "item_glassDark", mb.ToMesh("Sheet_item_glassDark"), lib.GlassDark);
                }
                if (lib.Grating != null)
                {
                    mb = new MeshBuilder();
                    mb.FlatQuad(new Vector3(15f, 1.2f, ItemZ - 1.3f), new Vector3(15f, 1.2f, ItemZ + 1.3f), new Vector3(17f, 1.2f, ItemZ + 1.3f), new Vector3(17f, 1.2f, ItemZ - 1.3f));
                    ctx.AddMesh(root, "item_grating", mb.ToMesh("Sheet_item_grating"), lib.Grating);
                }
            }

            // night key: 4 point lights with emissive heads and glow-disc wet sources
            Color amber = NocturnePalette.Linear(NocPal.SodiumAmber), cyan = NocturnePalette.Linear(NocPal.NeonCyan);
            float[] lx = { -9f, -3f, 3f, 9f };
            for (int i = 0; i < 4; i++)
            {
                bool warm = (i & 1) == 0;
                var pos = new Vector3(lx[i], 3.4f, -50.5f);
                var go = new GameObject("key_" + i);
                go.transform.SetParent(root, false);
                go.transform.position = pos;
                var l = go.AddComponent<Light>();
                l.type = LightType.Point;
                l.color = warm ? amber : cyan;
                l.intensity = 9f;
                l.range = 12f;
                l.shadows = LightShadows.None;
                mb = new MeshBuilder();
                MeshKit.Sphere(mb, pos, 0.15f, 16, 8);
                ctx.AddMesh(root, "keyHead_" + i, mb.ToMesh("Sheet_head_" + i), warm ? lib.GrayMarkerWarm : lib.GrayMarker);
                NocWetRegistry.AddSource(pos, pos, 0.5f, (warm ? amber : cyan) * NocturnePalette.EmissionSign, 0.0);
            }
            // vertical cyan tube (segment source) and the back wall as a wet wall plane
            var t0 = new Vector3(14.5f, 0.4f, -56f); var t1 = new Vector3(14.5f, 4.4f, -56f);
            mb = new MeshBuilder();
            MeshKit.Cylinder(mb, t0, t1, 0.06f, 16);
            ctx.AddMesh(root, "tube", mb.ToMesh("Sheet_tube"), lib.GrayMarker);
            NocWetRegistry.AddSource(t0, t1, 0.15f, cyan * NocturnePalette.EmissionSign, 0.0);
            // Look v2 (TEXTURE_PLAN 2.11): the back wall (faces +z) under PA's rig at s 0
            Color wallMacro = NocSurfaceMath.WallRadiance(NocPal.ConcreteDark, 0.0, 0.0, 1.0);
            NocWetRegistry.AddWall(-200.0, 2000.0, 0f, WallZ, 0f, 1f, 22f, 10f, wallMacro, 1);
            ctx.Register(this);
            ctx.Log($"material sheet built: {mats.Length} surfaces + glass pair, {floors.Length} wet floors, 4 point lights, {NocWetRegistry.TotalSources} wet sources");
        }

        public void OnFrame(in FrameState f)
        {
            if (m_logged || ++m_frames < 5) return;
            m_logged = true;
            int lights = Object.FindObjectsByType<Light>().Length;
            Debug.Log($"[NOC] material sheet: _CLUSTER_LIGHT_LOOP={Shader.IsKeywordEnabled("_CLUSTER_LIGHT_LOOP")} lights={lights} " +
                      $"wet sources active={NocWetRegistry.ActiveSources} walls={NocWetRegistry.ActiveWalls} LightCal={AtmosphereDriver.LightCal:F2}");
        }
    }
}
