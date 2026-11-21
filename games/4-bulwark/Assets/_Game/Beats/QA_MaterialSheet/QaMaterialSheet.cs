// QaMaterialSheet: the material-sheet rigs of MASTER_PROMPT 8.11.8 / 11.2 (materials owner, Wave A). Built ONLY when the
// player runs with -capture <dir> and a -capture-set list that names "materials" (ShotPlan.MaterialSheet.Requested), so
// the shipped game and every other capture never contain it. Layout, poses and the per-beat material lists are pure data
// in ShotPlan.MaterialSheet (the stills read the same numbers); the shot's s drives AtmosphereDriver, so each sheet is lit
// by its beat's key light, sky, fog and exposure. Off the path: 60 m west of the lee face and 20 m above the road (over
// the 6 km drop), and the crest-road patch 40 m above the real road at its true x / z (so the lane numerals of BwRoad show
// "03" at z 300). Interior beats get a closed booth lit by real point lights in the beat's lamp colour (14.2.3).
// Stand = chamfered slab 1.6 x 0.25 x 1.6 m (2 cm chamfers carry vertex G wear), sphere r 0.45 m, wall panel 1.7 x 1.9 m;
// "Armour@sea" sets the sample's sea level just above its slab so the wet / crust / foam / dry zones of BwArmour show on
// one stand; "Armour@gauge" switches the ochre gauge bands on (MaterialPropertyBlock, QA only). Beat 3 adds the 12 x 50 m
// grating field (bars along world z, fade regime 6 .. 1.5 px per period in view), beat 4 the 40 x 12 m crust wall and a
// 5 m drum 80-125 m from the sheet's cameras (the KS10 distances of the islets).
// Wave B (integrate): the surface state _BwSurface has ONE writer, AtmosphereDriver (Validate row "one writer of
// _BwSurface"); the sheet's s drives its wetness like every other frame, and the QA stand-in writer of Wave A is gone. The
// booth lamps are anchors of the shared LocalLightPool (14.2.3: at most 4 real local lights; rank 0.5, picked first at the
// booth), never Light components of their own. A booth is an interior for the region ambient only when the atmosphere
// owner's driver adds MaterialSheet.BoothBox to its interior boxes for the shot (shared change request, Wave B report).
using System.Reflection;
using Megastructure.QA;
using Megastructure.Rendering;
using UnityEngine;

namespace Megastructure.Beats
{
    public sealed class QaMaterialSheet : IBeatBuilder
    {
        public string Name => "QA_MaterialSheet";

        const float Bevel = 0.02f;
        /// <summary>Pool rank of the booth lamps (below 1: picked before any other anchor within reach).</summary>
        public const float BoothLampRank = 0.5f;

        public void Build(WorldContext ctx)
        {
            if (!MaterialSheet.Requested(CliArgs.CaptureDir, CliArgs.CaptureSet)) return;
            var root = new GameObject(Name).transform;
            root.SetParent(ctx.Root, false);
            int stands = 0;
            foreach (var d in MaterialSheet.Sheets)
            {
                var sheet = new GameObject("sheet_" + d.Name).transform;
                sheet.SetParent(root, false);
                sheet.SetPositionAndRotation(new Vector3((float)d.X, (float)d.Y, (float)d.Z), Quaternion.Euler(0f, (float)d.Yaw, 0f));
                BuildStage(ctx, sheet, d);
                for (int i = 0; i < d.Items.Length; i++) { BuildStand(ctx, sheet, d, i); stands++; }
                if (d.Grating) BuildGrating(ctx, root, sheet, d, false);
                if (d.GratingWall) BuildGrating(ctx, root, sheet, d, true);
                if (d.Booth) BuildBooth(ctx, sheet, d);
                if (d.RoadPatch) BuildRoadPatch(ctx, root);
                if (d.GratingField) BuildGratingField(ctx, root, d);
                if (d.CrustWall) BuildCrustWall(ctx, sheet, d);
            }
            ctx.Log($"QA material sheets: {MaterialSheet.Sheets.Length} sheets, {stands} stands (capture set 'materials')");
        }

        static Material Mat(WorldContext ctx, string field)
        {
            if (ctx.Materials == null) return null;
            FieldInfo f = typeof(MaterialLibrary).GetField(field, BindingFlags.Public | BindingFlags.Instance);
            var m = f != null ? f.GetValue(ctx.Materials) as Material : null;
            if (m == null) ctx.MeshProblems.Add("QA material sheet: MaterialLibrary." + field + " missing");
            return m;
        }

        static void Split(string item, out string field, out string variant)
        {
            int at = item.IndexOf('@');
            field = at < 0 ? item : item.Substring(0, at);
            variant = at < 0 ? "" : item.Substring(at + 1);
        }

        static MeshRenderer Add(WorldContext ctx, Transform parent, string name, MeshBuilder mb, Material mat, bool shadows = true)
        {
            var mr = ctx.AddMesh(parent, name, mb.ToMesh(name), mat, shadows, true);
            return mr;
        }

        // ------------------------------------------------------------------ stage, stands, booth

        static void BuildStage(WorldContext ctx, Transform sheet, MaterialSheetDef d)
        {
            var mb = new MeshBuilder();
            float w = (float)(2.0 * MaterialSheet.StageHalfWidth), len = (float)(MaterialSheet.StageFront + MaterialSheet.StageBack);
            float zc = (float)(0.5 * (MaterialSheet.StageFront - MaterialSheet.StageBack));
            MeshKit.ChamferBox(mb, new Vector3(0f, -0.2f, zc), new Vector3(w, 0.4f, len), Bevel);
            Add(ctx, sheet, "stage_floor", mb, Mat(ctx, d.Floor));
        }

        static void BuildStand(WorldContext ctx, Transform sheet, MaterialSheetDef d, int i)
        {
            Split(d.Items[i], out string field, out string variant);
            Material mat = Mat(ctx, field);
            float x = (float)((i - 0.5 * (d.Items.Length - 1)) * MaterialSheet.ItemPitch);
            float sh = (float)MaterialSheet.SlabHeight, r = (float)MaterialSheet.SphereRadius;
            bool emissive = field.StartsWith("Lamp") || field == "Lantern";
            var mb = new MeshBuilder();
            // slab 1 mm above the floor (no coplanar faces), wall panel behind, sphere in front of the panel; an emissive
            // material only on the sphere (the lamp), its slab and panel in painted steel
            MeshKit.ChamferBox(mb, new Vector3(x, 0.001f + 0.5f * sh, 0f), new Vector3((float)MaterialSheet.SlabSize, sh, (float)MaterialSheet.SlabSize), Bevel);
            MeshKit.ChamferBox(mb, new Vector3(x, sh + 0.5f * (float)MaterialSheet.PanelHeight + 0.002f, (float)MaterialSheet.PanelBack),
                               new Vector3((float)MaterialSheet.PanelWidth, (float)MaterialSheet.PanelHeight, (float)MaterialSheet.PanelThickness), Bevel);
            var ball = emissive ? new MeshBuilder() : mb;
            MeshKit.Sphere(ball, new Vector3(x, sh + r + 0.002f, -0.2f), r, 96, 48);
            var mr = Add(ctx, sheet, "stand_" + i + "_" + d.Items[i], mb, emissive ? Mat(ctx, "Steel") : mat);
            if (emissive) Add(ctx, sheet, "stand_" + i + "_" + d.Items[i] + "_lamp", ball, mat, false);
            if (field == "Armour" && variant.Length > 0)
            {
                var mpb = new MaterialPropertyBlock();
                if (variant == "sea")
                {
                    // the zones compressed onto one 2 m stand: sea 0.6 m above the floor, crust to +0.9, wet to +1.3
                    mpb.SetFloat("_SeaLevel", (float)(d.Y + 0.6));
                    mpb.SetFloat("_SwashHeight", 0.9f);
                    mpb.SetFloat("_WetHeight", 1.3f);
                    mpb.SetFloat("_FoamWidth", 0.15f);
                }
                if (variant == "gauge") mpb.SetFloat("_GaugeBands", 1f);
                mr.SetPropertyBlock(mpb);
            }
        }

        static void BuildGrating(WorldContext ctx, Transform root, Transform sheet, MaterialSheetDef d, bool wall)
        {
            MaterialSheet.GratingCentre(d, out double gx, out double gy, out double gz);
            // horizontal panels are world-aligned (the bars run along world z, as on the spillway slots); wall panels face
            // the camera in the sheet frame
            Transform parent = wall ? sheet : root;
            Vector3 c = wall ? sheet.InverseTransformPoint(new Vector3((float)gx, (float)gy, (float)gz)) : new Vector3((float)gx, (float)gy, (float)gz);
            float s = (float)MaterialSheet.GratingSize, h = 0.5f * s;
            var mb = new MeshBuilder();
            var frame = new MeshBuilder();
            if (!wall)
            {
                c.y = (float)(d.Y + MaterialSheet.GratingLift);
                // panel 1.0 m above a wet floor; steel frame 60 mm round the edge, legs at the corners
                mb.FlatQuad(c + new Vector3(-h, 0f, -h), c + new Vector3(-h, 0f, h), c + new Vector3(h, 0f, h), c + new Vector3(h, 0f, -h));
                for (int k = 0; k < 4; k++)
                {
                    float a = k * 90f;
                    var rot = Quaternion.Euler(0f, a, 0f);
                    Vector3 edge = c + rot * new Vector3(0f, -0.03f, h + 0.03f);
                    MeshKit.ChamferBox(frame, edge, rot * new Vector3(s + 0.12f, 0.06f, 0.06f), 0.01f, Quaternion.identity);
                    Vector3 leg = c + rot * new Vector3(h + 0.03f, -0.5f * (float)MaterialSheet.GratingLift - 0.03f, h + 0.03f);
                    MeshKit.ChamferBox(frame, leg, new Vector3(0.08f, (float)MaterialSheet.GratingLift - 0.06f, 0.08f), 0.01f);
                }
                // the "water" below: a wet slab just above the stage floor
                var pit = new MeshBuilder();
                MeshKit.ChamferBox(pit, new Vector3(c.x, (float)d.Y + 0.031f, c.z), new Vector3(s + 1.0f, 0.06f, s + 1.0f), Bevel);
                Add(ctx, parent, "grating_pit", pit, Mat(ctx, "ConcreteWet"));
            }
            else
            {
                // vertical tunnel panel 2.8 m tall facing the camera, frame posts at the sides
                float ph = (float)BulwarkConst.GratingPanelHeight;
                mb.FlatQuad(c + new Vector3(-h, 0.05f, 0f), c + new Vector3(-h, 0.05f + ph, 0f), c + new Vector3(h, 0.05f + ph, 0f), c + new Vector3(h, 0.05f, 0f));
                MeshKit.ChamferBox(frame, c + new Vector3(-h - 0.05f, 0.05f + 0.5f * ph, 0f), new Vector3(0.1f, ph, 0.1f), 0.01f);
                MeshKit.ChamferBox(frame, c + new Vector3(h + 0.05f, 0.05f + 0.5f * ph, 0f), new Vector3(0.1f, ph, 0.1f), 0.01f);
                MeshKit.ChamferBox(frame, c + new Vector3(0f, 0.05f + ph + 0.05f, 0f), new Vector3(s + 0.2f, 0.1f, 0.1f), 0.01f);
            }
            Add(ctx, parent, wall ? "grating_wall" : "grating", mb, Mat(ctx, "Grating"), false);
            Add(ctx, parent, "grating_frame", frame, Mat(ctx, "Steel"));
        }

        static void BuildGratingField(WorldContext ctx, Transform root, MaterialSheetDef d)
        {
            float x0 = (float)MaterialSheet.FieldX0, x1 = (float)MaterialSheet.FieldX1;
            float z0 = (float)MaterialSheet.FieldZ0, z1 = (float)MaterialSheet.FieldZ1;
            float y = (float)(d.Y + MaterialSheet.GratingLift), yf = (float)d.Y;
            var mb = new MeshBuilder();
            mb.FlatQuad(new Vector3(x0, y, z0), new Vector3(x0, y, z1), new Vector3(x1, y, z1), new Vector3(x1, y, z0));
            Add(ctx, root, "grating_field", mb, Mat(ctx, "Grating"), false);
            // wet floor 1.0 m below (the slot water stand-in) and a steel kerb frame round the field
            var floor = new MeshBuilder();
            MeshKit.ChamferBox(floor, new Vector3(0.5f * (x0 + x1), yf - 0.2f, 0.5f * (z0 + z1)), new Vector3(x1 - x0 + 2f, 0.4f, z1 - z0 + 4f), Bevel);
            Add(ctx, root, "grating_field_floor", floor, Mat(ctx, "ConcreteWet"));
            var frame = new MeshBuilder();
            float cx = 0.5f * (x0 + x1), cz = 0.5f * (z0 + z1);
            MeshKit.ChamferBox(frame, new Vector3(cx, y - 0.03f, z0 - 0.03f), new Vector3(x1 - x0 + 0.12f, 0.06f, 0.06f), 0.01f);
            MeshKit.ChamferBox(frame, new Vector3(cx, y - 0.03f, z1 + 0.03f), new Vector3(x1 - x0 + 0.12f, 0.06f, 0.06f), 0.01f);
            MeshKit.ChamferBox(frame, new Vector3(x0 - 0.03f, y - 0.03f, cz), new Vector3(0.06f, 0.06f, z1 - z0), 0.01f);
            MeshKit.ChamferBox(frame, new Vector3(x1 + 0.03f, y - 0.03f, cz), new Vector3(0.06f, 0.06f, z1 - z0), 0.01f);
            Add(ctx, root, "grating_field_frame", frame, Mat(ctx, "Steel"));
        }

        static void BuildCrustWall(WorldContext ctx, Transform sheet, MaterialSheetDef d)
        {
            // sheet frame: x right, z along the view yaw; the wall's front face at local z CrustWallZ faces the cameras
            float hw = (float)MaterialSheet.CrustWallHalfWidth, ht = (float)MaterialSheet.CrustWallHeight, zf = (float)MaterialSheet.CrustWallZ;
            var wall = new MeshBuilder();
            MeshKit.ChamferBox(wall, new Vector3(0f, 0.5f * ht, zf + 1.0f), new Vector3(2f * hw, ht, 2.0f), 0.05f);
            Vector3 da = new Vector3((float)MaterialSheet.CrustDrumX, 0.002f, (float)MaterialSheet.CrustDrumZ);
            MeshKit.Cylinder(wall, da, da + new Vector3(0f, ht, 0f), (float)MaterialSheet.CrustDrumRadius, 96, true);
            var mr = Add(ctx, sheet, "crust_wall", wall, Mat(ctx, "Armour"));
            var mpb = new MaterialPropertyBlock();
            mpb.SetFloat("_SeaLevel", (float)(d.Y + MaterialSheet.CrustWallSea));
            mr.SetPropertyBlock(mpb);
            var bed = new MeshBuilder();
            MeshKit.ChamferBox(bed, new Vector3(0f, -0.2f, zf - 12f), new Vector3(2f * hw + 8f, 0.4f, 30f), Bevel);
            Add(ctx, sheet, "crust_wall_bed", bed, Mat(ctx, "ConcreteDark"));
        }

        static void BuildBooth(WorldContext ctx, Transform sheet, MaterialSheetDef d)
        {
            float hw = (float)MaterialSheet.BoothHalfWidth, back = (float)MaterialSheet.BoothBack, front = (float)MaterialSheet.BoothFront;
            float ht = (float)MaterialSheet.BoothHeight, t = 0.4f, len = back + front;
            float zc = 0.5f * (front - back);
            var mb = new MeshBuilder();
            MeshKit.ChamferBox(mb, new Vector3(-hw - 0.5f * t, 0.5f * ht, zc), new Vector3(t, ht, len + 2f * t), Bevel);
            MeshKit.ChamferBox(mb, new Vector3(hw + 0.5f * t, 0.5f * ht, zc), new Vector3(t, ht, len + 2f * t), Bevel);
            MeshKit.ChamferBox(mb, new Vector3(0f, 0.5f * ht, front + 0.5f * t), new Vector3(2f * hw, ht, t), Bevel);
            MeshKit.ChamferBox(mb, new Vector3(0f, 0.5f * ht, -back - 0.5f * t), new Vector3(2f * hw, ht, t), Bevel);
            MeshKit.ChamferBox(mb, new Vector3(0f, ht + 0.5f * t, zc), new Vector3(2f * hw + 2f * t, t, len + 2f * t), Bevel);
            string wallMat = d.Floor == "ConcreteDark" || d.Name.Contains("tunnel") ? "ConcreteDark" : "Concrete";
            Add(ctx, sheet, "booth", mb, Mat(ctx, wallMat));

            // the beat's lamps: anchors of the shared pool (real point lights without shadows, 14.2.3), a small glowing
            // bulb each; the pool stores the linear palette colour and hands URP its gamma value
            var bulbs = new MeshBuilder();
            Color lc = BulwarkPalette.Linear(d.Lamp);
            var pool = LocalLightPool.Ensure(ctx);
            for (int k = 0; k < d.LampCount; k++)
            {
                float x = d.LampCount == 1 ? -1.6f : (k == 0 ? -3.2f : 3.2f);
                var pos = new Vector3(x, (float)d.LampHeight, -2.2f);
                pool.Add(sheet.TransformPoint(pos), lc, (float)d.LampRange, (float)d.LampIntensity, "qa_sheet_" + d.Name, BoothLampRank);
                MeshKit.Sphere(bulbs, pos, 0.1f, 24, 12);
            }
            Add(ctx, sheet, "bulbs", bulbs, Mat(ctx, d.Lamp == Pal.Tungsten ? "LampTungsten" : "LampSodium"), false);
        }

        static void BuildRoadPatch(WorldContext ctx, Transform root)
        {
            var mb = new MeshBuilder();
            float z0 = (float)MaterialSheet.RoadPatchZ0, z1 = (float)MaterialSheet.RoadPatchZ1, y = (float)MaterialSheet.RoadPatchY;
            MeshKit.ChamferBox(mb, new Vector3(0f, y - 0.2f, 0.5f * (z0 + z1)), new Vector3(12f, 0.4f, z1 - z0), Bevel);
            Add(ctx, root, "road_patch", mb, Mat(ctx, "Road"));
            var par = new MeshBuilder();
            MeshKit.ChamferBox(par, new Vector3(5.75f, y + 0.551f, 0.5f * (z0 + z1)), new Vector3(0.5f, 1.1f, z1 - z0), Bevel);
            Add(ctx, root, "road_patch_parapet", par, Mat(ctx, "Concrete"));
        }
    }
}
