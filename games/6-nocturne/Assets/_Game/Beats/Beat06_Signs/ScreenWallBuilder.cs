// ScreenWallBuilder: the Wall of signs slab of beat 6 (MASTER_PROMPT 14.4 "Screen wall", 14.5.6 Geometry, Light; D-010; SG Phase 4).
// Slab x 834 - 874, z 275 - 520, y -1 500..560 (40.0 x 245 m, 14.5.6 Acceptance (1)); the face x 874.00 is one NocScreen quad
// (the LED screen facade: colour waves, imagery, panels, modules, pixels, all in the shader, NocScreenWave.hlsl); the other three
// vertical faces are NocFacade (FA's S2 metal panels with LED-strip housings) so the slab reads as a building from the garden
// (south), the plaza (north) and face B (west). Top (inside the cloud mix, never seen from below) and bottom (fog) are not built.
// The nine giant glyph signs of 14.5.6 (60-120 m at y +20..+420): NocNeon faces on painted-steel housings, five blades
// perpendicular to the face (both sides lit, a strut to the face above) and four boards flat on it (SignsLayout.GlyphSigns).
// Twelve real point lights hang 6 m in front of the face at y 14 and take the wave colour at their position desaturated 40 %
// toward WinNeutral (LOOK_V2 R4 / 6: r 45, i 3.5; SignsDriver; <= 0.017 Hz). The wet floor reflects the face as an H4 wall
// plane of kind 2 (the wave at the hit, NocScreenMirror) with the mean of the LED wall (NocScreenMath.MirrorMacro).
// Tiers (14.5.6 Fallbacks): Full = three waves, imagery, glyph signs; Simplified = one wave, no imagery, no glyph signs.
using System.Collections.Generic;
using UnityEngine;

namespace Megastructure
{
    public static class ScreenWallBuilder
    {
        /// <summary>LOOK_V2 6 wall lights: intensity 3.5 (SignsLayout.WallLightIntensity is the v1 5.0), range 45 m.</summary>
        public const double WallLightIntensity = 3.5;
        /// <summary>The H4 wall plane's kind: 2 = the screen wave (NocMirror NocScreenMirror; FX's include), 1 = flat in older mirrors.</summary>
        public const int MirrorKind = 2;

        /// <summary>The slab's local origin (its footprint centre at y 0): every vertex lies within 123 m horizontally.</summary>
        public static readonly Vector3 Origin = new Vector3(
            (float)(0.5 * (SignsLayout.WallX0 + SignsLayout.WallX1)), 0f, (float)(0.5 * (SignsLayout.WallZ0 + SignsLayout.WallZ1)));

        /// <summary>The screen face quad (x 874, normal +x), positions relative to Origin.</summary>
        public static MeshBuilder ScreenFace()
        {
            var mb = new MeshBuilder();
            float x = (float)SignsLayout.WallX1, y0 = (float)SignsLayout.WallY0, y1 = (float)SignsLayout.WallY1;
            float z0 = (float)SignsLayout.WallZ0, z1 = (float)SignsLayout.WallZ1;
            G4Kit.OrientedQuad(mb, new Vector3(x, y0, z0) - Origin, new Vector3(x, y1, z0) - Origin, new Vector3(x, y1, z1) - Origin,
                new Vector3(x, y0, z1) - Origin, Vector3.right);
            return mb;
        }

        /// <summary>The west, south and north faces of the slab (NocFacade), positions relative to Origin.</summary>
        public static MeshBuilder MassFaces()
        {
            var mb = new MeshBuilder();
            G4Kit.FacadeBox(mb, Origin, SignsLayout.WallX0, SignsLayout.WallX1, SignsLayout.WallZ0, SignsLayout.WallZ1,
                SignsLayout.WallY0, SignsLayout.WallY1, NocFacadeMath.SkinOverride(NocFacadeMath.OverrideScreenWall), 0,
                west: true, east: false, south: true, north: true);   // FA: the S2 metal-panel skin with LED-strip housings (CRIT0 #22)
            return mb;
        }

        /// <summary>The housings of the glyph signs (painted steel, world positions relative to Origin) and their NocNeon faces.</summary>
        public static MeshBuilder SignHousings(List<ArcadeBuilder.Sign> faces)
        {
            var mb = new MeshBuilder();
            foreach (var g in SignsLayout.GlyphSigns)
            {
                double y0 = g.y0, y1 = g.y0 + g.h;
                if (g.blade)
                {
                    double xa = SignsLayout.WallX1 - 0.1, xb = SignsLayout.BladeX0 + g.w + 0.3, t = 0.5 * SignsLayout.BladeT, rim = SignsLayout.BladeRim;
                    ArcadeBuilder.Box(mb, Origin, xa, xb, y0 - rim, y1 + rim, g.z - t, g.z + t);
                    // a strut from the face, 14 m above the blade, to its outer top corner (the blade hangs from it)
                    MeshKit.ChamferMember(mb, new Vector3((float)(SignsLayout.WallX1 - 0.05), (float)(y1 + 14.0), (float)g.z) - Origin,
                        new Vector3((float)(xb - 0.4), (float)(y1 + rim - 0.2), (float)g.z) - Origin, 0.5f, 0.5f, 0.04f, Vector3.up);
                    if (faces != null)
                    {
                        var c = new Vector3((float)(SignsLayout.BladeX0 + 0.5 * g.w + 0.15), (float)(y0 + 0.5 * g.h), 0f);
                        float off = (float)(t + SignsLayout.NeonProud);
                        var size = new Vector2((float)(g.w - 0.3), (float)g.h);
                        faces.Add(new ArcadeBuilder.Sign { centre = new Vector3(c.x, c.y, (float)g.z - off), rotation = Quaternion.identity, size = size, palette = g.palette });
                        faces.Add(new ArcadeBuilder.Sign { centre = new Vector3(c.x, c.y, (float)g.z + off), rotation = Quaternion.identity, size = size, palette = g.palette });
                    }
                }
                else
                {
                    double za = g.z - 0.5 * g.w, zb = g.z + 0.5 * g.w, rim = SignsLayout.BoardRim;
                    ArcadeBuilder.Box(mb, Origin, SignsLayout.WallX1 - 0.1, SignsLayout.BoardX, y0 - rim, y1 + rim, za - rim, zb + rim);
                    faces?.Add(new ArcadeBuilder.Sign
                    {
                        centre = new Vector3((float)(SignsLayout.BoardX + SignsLayout.NeonProud), (float)(y0 + 0.5 * g.h), (float)g.z),
                        rotation = Quaternion.Euler(0f, -90f, 0f), size = new Vector2((float)g.w, (float)g.h), palette = g.palette,
                    });
                }
            }
            return mb;
        }

        // ------------------------------------------------------------------ scale markers (fix wave C17)
        /// <summary>The two service ledges (painted steel, flush: x 873.95 - 874.45) and their lamp lenses (0.3 x 0.2 m, facing down,
        /// every 6 m); the lamp heads (world) go to NocHalos. Positions relative to Origin.</summary>
        public static MeshBuilder Ledges(MeshBuilder lenses, List<Vector3> heads)
        {
            var mb = new MeshBuilder();
            foreach (double y in SignsLayout.LedgeY)
            {
                double y0 = y - 0.5 * SignsLayout.LedgeT, y1 = y + 0.5 * SignsLayout.LedgeT;
                foreach (var seg in LedgeSegments(y))
                {
                    ArcadeBuilder.Box(mb, Origin, SignsLayout.WallX1 - 0.05, SignsLayout.LedgeX1, y0, y1, seg.a, seg.b);
                    // a 0.12 m upstand at the lip (the edge reads as a walkway, not a flat strip)
                    ArcadeBuilder.Box(mb, Origin, SignsLayout.LedgeX1 - 0.05, SignsLayout.LedgeX1, y1, y1 + 0.12, seg.a, seg.b);
                }
                if (lenses == null) continue;
                for (double z = SignsLayout.LedgeLampZ0; z <= SignsLayout.WallZ1 - 2.0; z += SignsLayout.LedgeLampPitch)
                {
                    if (!OnLedge(y, z, 0.5)) continue;
                    var c = new Vector3((float)(SignsLayout.WallX1 + 0.24), (float)(y0 - 0.005), (float)z);
                    G4Kit.OrientedQuad(lenses, c + new Vector3(-0.1f, 0f, -0.15f) - Origin, c + new Vector3(0.1f, 0f, -0.15f) - Origin,
                        c + new Vector3(0.1f, 0f, 0.15f) - Origin, c + new Vector3(-0.1f, 0f, 0.15f) - Origin, Vector3.down);
                    heads?.Add(c + new Vector3(0f, -0.05f, 0f));
                }
            }
            return mb;
        }

        /// <summary>The ledge at height y runs z 275.3 - 519.7, interrupted 0.3 m clear of every glyph-sign housing (and blade strut)
        /// it would cross (the flush boards, the z 292 blade at y 204).</summary>
        public static List<(double a, double b)> LedgeSegments(double y)
        {
            var cuts = new List<(double a, double b)>();
            double ya = y - 0.5 * SignsLayout.LedgeT - 0.2, yb = y + 0.5 * SignsLayout.LedgeT + 0.32;
            foreach (var g in SignsLayout.GlyphSigns)
            {
                double r = g.blade ? SignsLayout.BladeRim : SignsLayout.BoardRim;
                double gy0 = g.y0 - r, gy1 = g.y0 + g.h + r + (g.blade ? 14.0 : 0.0);
                if (gy1 < ya || gy0 > yb) continue;
                double za = g.blade ? g.z - 0.5 * SignsLayout.BladeT : g.z - 0.5 * g.w - r, zb = g.blade ? g.z + 0.5 * SignsLayout.BladeT : g.z + 0.5 * g.w + r;
                cuts.Add((za - 0.3, zb + 0.3));
            }
            cuts.Sort((p, q) => p.a.CompareTo(q.a));
            var segs = new List<(double a, double b)>();
            double z0 = SignsLayout.WallZ0 + 0.3;
            foreach (var c in cuts)
            {
                if (c.a - z0 > 1.0) segs.Add((z0, c.a));
                z0 = System.Math.Max(z0, c.b);
            }
            if (SignsLayout.WallZ1 - 0.3 - z0 > 1.0) segs.Add((z0, SignsLayout.WallZ1 - 0.3));
            return segs;
        }

        /// <summary>True when z lies on a ledge segment at height y with `margin` m to spare.</summary>
        public static bool OnLedge(double y, double z, double margin)
        {
            foreach (var s in LedgeSegments(y)) if (z >= s.a + margin && z <= s.b - margin) return true;
            return false;
        }

        /// <summary>One window-cleaning cradle in its own frame (origin: the deck's centre bottom; x toward the walk): a steel deck,
        /// a lit cabin 0.9 x 1.23 x 2.9 m, corner and mid posts, a roof and two hoists that grip the cables at z +-1.2.</summary>
        public static void Cradle(MeshBuilder steel, MeshBuilder cabin)
        {
            float w = (float)SignsLayout.CradleW, d = (float)SignsLayout.CradleD;
            MeshKit.ChamferBox(steel, new Vector3(0f, 0.06f, 0f), new Vector3(d, 0.12f, w), 0.02f);                  // deck
            MeshKit.ChamferBox(cabin, new Vector3(0f, 0.735f, 0f), new Vector3(d - 0.1f, 1.23f, w - 0.1f), 0.01f);    // lit cabin
            MeshKit.ChamferBox(steel, new Vector3(0f, 1.40f, 0f), new Vector3(d, 0.10f, w), 0.02f);                   // roof
            foreach (float px in new[] { -0.46f, 0.46f })
                foreach (float pz in new[] { -1.46f, 0f, 1.46f })
                    MeshKit.ChamferBox(steel, new Vector3(px, 0.735f, pz), new Vector3(0.08f, 1.23f, 0.08f), 0.01f);    // posts
            MeshKit.ChamferBox(steel, new Vector3(0.47f, 0.80f, 0f), new Vector3(0.05f, 0.07f, w - 0.1f), 0.01f);     // guard rail
            foreach (float pz in new[] { -1.2f, 1.2f })
                MeshKit.ChamferBox(steel, new Vector3(0f, 1.70f, pz), new Vector3(0.40f, 0.50f, 0.40f), 0.03f);       // hoists
        }

        /// <summary>The cradles' fixed cables (two per cradle at z +-1.2, x of the cradle centre) from the wall top down to 2 m
        /// below the lowest cradle position; positions relative to Origin.</summary>
        public static MeshBuilder Cables()
        {
            var mb = new MeshBuilder();
            float x = (float)(SignsLayout.CradleX0 + 0.5 * SignsLayout.CradleD);
            for (int i = 0; i < SignsLayout.CradleZ.Length; i++)
                foreach (double dz in new[] { -1.2, 1.2 })
                {
                    float z = (float)(SignsLayout.CradleZ[i] + dz), yLow = (float)(SignsLayout.CradleYc[i] - SignsLayout.CradleA[i] - 2.0);
                    MeshKit.Cylinder(mb, new Vector3(x, (float)SignsLayout.WallY1, z) - Origin, new Vector3(x, yLow, z) - Origin, (float)SignsLayout.CableR, 6, false);
                }
            return mb;
        }

        /// <summary>Builds the ledges, lamp lenses (+ halos), cables and the four cradles; returns the cradle roots (SignsDriver moves
        /// them: y = SignsLayout.CradleY(i, t600)).</summary>
        public static Transform[] BuildService(WorldContext ctx, Transform root)
        {
            var lib = ctx.Materials;
            var lenses = new MeshBuilder();
            var heads = new List<Vector3>(96);
            G4Kit.Add(ctx, root, "serviceLedges", Ledges(lenses, heads), lib != null ? lib.SteelPainted : null, Origin);
            Color white = G4Kit.Lin(NocPal.SignWhite);
            G4Kit.Add(ctx, root, "serviceLedgeLamps", lenses, lib != null ? G4Kit.Emissive(lib.SteelPainted, white, SignsLayout.LedgeLensGain, "Beat06_LedgeLamp") : null, Origin);
            NocHalos.Queue("SignsLedge", heads, G4Kit.Scale(white, SignsLayout.LedgeLampGain * 0.85), 8.0);
            G4Kit.Add(ctx, root, "cradleCables", Cables(), lib != null ? G4Kit.Line(lib.Line, 2.0 * SignsLayout.CableR, G4Kit.Lin(NocPal.MetalPaint), Color.black, "Beat06_CradleCable") : null, Origin);
            Material steel = lib != null ? lib.SteelPainted : null;
            Material cabinM = lib != null ? G4Kit.Emissive(lib.SteelPainted, G4Kit.Lin(NocPal.WinCold), SignsLayout.CradleGain, "Beat06_CradleCabin") : null;
            var roots = new Transform[SignsLayout.CradleZ.Length];
            for (int i = 0; i < roots.Length; i++)
            {
                var go = new GameObject("cradle" + i);
                go.transform.SetParent(root, false);
                var p = new Vector3((float)(SignsLayout.CradleX0 + 0.5 * SignsLayout.CradleD), (float)SignsLayout.CradleY(i, 0.0), (float)SignsLayout.CradleZ[i]);
                go.transform.position = p;
                var sm = new MeshBuilder(); var cm = new MeshBuilder();
                Cradle(sm, cm);
                G4Kit.Add(ctx, go.transform, "cradle" + i + "_steel", sm, steel, p);
                G4Kit.Add(ctx, go.transform, "cradle" + i + "_cabin", cm, cabinM, p);
                roots[i] = go.transform;
            }
            ctx.Log(System.FormattableString.Invariant($"Beat06 scale markers: {SignsLayout.LedgeY.Length} ledges, {heads.Count} ledge lamps, {roots.Length} cradles"));
            return roots;
        }

        /// <summary>World position of wall light i (0..11).</summary>
        public static Vector3 WallLightPosition(int i)
            => new Vector3((float)SignsLayout.WallLightX, (float)SignsLayout.WallLightY, (float)(SignsLayout.WallLightZ0 + SignsLayout.WallLightStep * i));

        public static void Build(WorldContext ctx, Transform root, int tier, out Material screen, out Light[] lights)
        {
            var lib = ctx.Materials;
            int waves = SignsLayout.Waves(tier);
            double content = SignsLayout.Content(tier);
            screen = G4Kit.FromShader("Megastructure/NocScreen", "Beat06_Screen");
            if (screen != null)
            {
                screen.SetFloat("_Waves", waves);
                screen.SetFloat("_Content", (float)content);
                screen.SetFloat("_EmitGain", (float)SignsLayout.EmitGain);
                screen.SetFloat("_Level", (float)SignsLayout.PanelLevel);
                screen.SetFloat("_Breath", (float)SignsLayout.BreathAmp);
                screen.SetFloat("_SeamDark", (float)SignsLayout.SeamDark);
                screen.SetFloat("_Smooth", (float)SignsLayout.ScreenSmooth);
            }
            // fallback: a flat dark LED substrate with the wall's mean light (never a missing material, never violet)
            D3 meanF = NocScreenMath.MeanEmission(waves);
            Material face = screen != null ? screen : G4Kit.Emissive(lib != null ? lib.Concrete : null,
                new Color((float)meanF.x, (float)meanF.y, (float)meanF.z, 1f) + G4Kit.Lin(NocPal.ScreenBase), 1.0, "Beat06_ScreenFallback");
            G4Kit.Add(ctx, root, "screenFace", ScreenFace(), face, Origin);
            G4Kit.Add(ctx, root, "screenMass", MassFaces(), lib != null ? lib.Facade : null, Origin);

            int signs = 0;
            if (tier == 0)
            {
                var faces = new List<ArcadeBuilder.Sign>(24);
                G4Kit.Add(ctx, root, "glyphSignHousings", SignHousings(faces), lib != null ? lib.SteelPainted : null, Origin);
                signs = G4Kit.NeonSigns(lib, "Beat06_GlyphSigns", faces);
            }

            lights = new Light[SignsLayout.WallLightCount];
            for (int i = 0; i < lights.Length; i++)
            {
                Vector3 p = WallLightPosition(i);
                D3 c = NocScreenMath.Ramp(NocScreenMath.IndexAt(p.z, p.y, 0.0, waves));
                lights[i] = G4Kit.PointLight(root, "wallLight" + i, p, UnitColour(c), SignsLayout.WallLightRange, WallLightIntensity);
            }

            // H4 wall plane: the face as the screen wave scaled to the LED wall's mean, active while the face is beside the walk
            D3 mean = NocScreenMath.MirrorMacro(waves, content);
            // integration round (QA-F02's cause, WetRegistryWalkTests): the Heart's two kind-0 plaza walls (s 1 250-1 515) are registered
            // BEFORE this kind-1 plane, so they keep their mirror slots (the seed of their mirrored windows) when it fades out at s 1 277
            OldQuarterBuilder.RegisterPlazaWalls();
            NocWetRegistry.AddWall(1095.0, 1265.0, (float)SignsLayout.WallX1, (float)(0.5 * (SignsLayout.WallZ0 + SignsLayout.WallZ1)), 1f, 0f,
                (float)(0.5 * (SignsLayout.WallZ1 - SignsLayout.WallZ0)), (float)SignsLayout.WallY1, new Color((float)mean.x, (float)mean.y, (float)mean.z, 1f), MirrorKind);
            ctx.Log(System.FormattableString.Invariant($"Beat06 screen wall: slab x {SignsLayout.WallX0}-{SignsLayout.WallX1}, z {SignsLayout.WallZ0}-{SignsLayout.WallZ1}, y {SignsLayout.WallY0}..{SignsLayout.WallY1}; NocScreen {(screen != null ? "ok" : "MISSING (fallback)")}, tier {tier}, waves {waves}, imagery {content:F1}; {signs} glyph-sign faces; {lights.Length} wall lights; mirror macro kind {MirrorKind} ({mean.x:F4}, {mean.y:F4}, {mean.z:F4})"));
        }

        /// <summary>A light colour from a ramp colour (linear): the ramp chromaticity at max 1, desaturated 40 % toward WinNeutral
        /// (LOOK_V2 R4: a light that washes the promenade stays at sRGB saturation <= 0.45), brightest channel at 1.</summary>
        public static Color UnitColour(D3 c)
        {
            double m = System.Math.Max(c.x, System.Math.Max(c.y, c.z));
            if (m < 1e-6) return Color.black;
            Color n = NocturnePalette.Linear(NocPal.WinNeutral);
            double r = c.x / m + (n.r - c.x / m) * 0.4, g = c.y / m + (n.g - c.y / m) * 0.4, b = c.z / m + (n.b - c.z / m) * 0.4;
            double k = System.Math.Max(r, System.Math.Max(g, b));
            return new Color((float)(r / k), (float)(g / k), (float)(b / k), 1f);
        }
    }
}
