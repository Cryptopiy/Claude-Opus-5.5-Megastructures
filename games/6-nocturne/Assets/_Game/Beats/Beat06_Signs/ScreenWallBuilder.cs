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
