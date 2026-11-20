// AlleyFixtures: the near detail of beat 1 at graybox-plus (MASTER_PROMPT 14.5.1; the Simplified tier's counts or fewer):
//   * the catwalk z 96.0-97.5 (D-029 / GB-12: the spec's z 97.6 put its bottom edge at NDC -0.9404, outside the 3 % margin;
//     97.5 projects to -0.930), y 8.8-9.2 across the alley (KS02: the dark band across the slit at y -0.93..-0.70), ends 0.25 m
//     inside both walls, 1.0 m railings (NocLine rods) with posts every 1.1 m;
//   * fire escapes (FireEscape): 3 modules per side, 4 landings 0.9 x 2.0 m from y 4.2 every 3.4 m, 1.0 m rails, flights between
//     the landings (stringers and 0.18 m treads), wall brackets; everything above y 3.9 (the camera sphere tops out at 1.9 m);
//   * 24 catenary cables 6 mm (NocLine, >= 1.75 px) wall to wall at y 5-19, 0.35 m sag, ends 5 cm inside the walls;
//   * 22 blade signs 0.5 x 2.0 m (NocNeon, perpendicular to the wall, bottoms y 3.8-6.0, palette cycle) with their brackets, as
//     wet-floor sources; every third one carries a small light (14.2.3 beat 1: 8 blade signs);
//   * 10 AC units 0.9 x 0.6 x 0.6 m on the walls above y 3.5;
//   * the mouth light CloudHot r 25 i 4 at (0, 6, 121) (14.2.3).
// Every fixture stays outside the camera sphere's reach: nothing below y 2.6 lies inside the clear width.
using System.Collections.Generic;
using UnityEngine;

namespace Megastructure
{
    public static class AlleyFixtures
    {
        public const float SignLightRange = 6f, SignLightIntensity = 1.5f, SignRadiance = 0.6f;
        /// <summary>LOOK_V2 6 beat 1 (PA): the mouth light lerp(HazeLow, CloudHot, 0.5) r 25 i 2.0 (v1 CloudHot i 4) and its wet strip
        /// CloudHot x 0.55; ten door bulkheads LampWarm r 7 i 1.6, 0.25 m off the wall at y 2.6, alternating walls every 11.5 m.</summary>
        public const float MouthLightIntensityV2 = 2.0f, MouthSourceGain = 0.55f;
        public const float BulkheadRange = 7f, BulkheadIntensity = 1.6f, BulkheadY = 2.6f, BulkheadOff = 0.25f, BulkheadZ0 = 6f, BulkheadPitch = 11.5f;
        public const int BulkheadCount = 10;
        /// <summary>The blade signs that carry a real light (4: LOOK_V2 6 beat 1, 19 alley lights under the cap of 20).</summary>
        public static bool SignHasLight(int i) => i % 6 == 0;
        /// <summary>Catwalk extent along z (D-029, GB-12; AlleyLayout keeps the spec's 96.0 / 97.6 for the window keep-out band).</summary>
        public const double CatwalkZ0 = 96.0, CatwalkZ1 = 97.5;
        /// <summary>Look v2 grating frames (TEXTURE_PLAN 3.5): stringer / edge-angle height and width, the gap under the plate.</summary>
        public const float StringerH = 0.15f, StringerW = 0.05f, AngleH = 0.06f, AngleW = 0.04f, PlateGap = 0.005f;

        public static void Build(G1Parts parts)
        {
            Catwalk(parts);
            FireEscapes(parts);
            Cables(parts);
            BladeSigns(parts);
            AcUnits(parts);
            Color mouth = Color.Lerp(NocturnePalette.Linear(NocPal.HazeLow), NocturnePalette.Linear(NocPal.CloudHot), 0.5f);
            mouth.a = 1f;
            parts.Light("mouthLight", new Vector3(0f, (float)AlleyLayout.MouthLightY, (float)AlleyLayout.MouthLightZ),
                mouth, (float)AlleyLayout.MouthLightRange, MouthLightIntensityV2);
            Bulkheads(parts);
        }

        /// <summary>Bulkhead z and wall side (-1 west first, alternating) of bulkhead k.</summary>
        public static float BulkheadZ(int k) => BulkheadZ0 + BulkheadPitch * k;
        public static int BulkheadSide(int k) => (k & 1) == 0 ? -1 : 1;

        /// <summary>Ten door bulkheads (LOOK_V2 6 beat 1): a small LampWarm lens (0.24 x 0.12 m, 6 cm proud) and its light 0.25 m off
        /// the wall at y 2.6, grazing the wall (NdotL 0.1-0.5 across the pool reveals the relief) with a broad pool on the facing wall.</summary>
        static void Bulkheads(G1Parts parts)
        {
            var lens = parts.Get("bulkheadLens", G1Mat.EmitLamp, new Vector3(0f, BulkheadY, 60f));
            for (int k = 0; k < BulkheadCount; k++)
            {
                float z = BulkheadZ(k);
                int side = BulkheadSide(k);
                float xw = side * (float)AlleyLayout.HalfWidth(z);
                G1Mesh.Box(lens, new Vector3(side < 0 ? xw : xw - 0.06f, BulkheadY - 0.06f, z - 0.12f), new Vector3(side < 0 ? xw + 0.06f : xw, BulkheadY + 0.06f, z + 0.12f),
                    side < 0 ? (G1Mesh.PX | G1Mesh.PY | G1Mesh.NY | G1Mesh.PZ | G1Mesh.NZ) : (G1Mesh.NX | G1Mesh.PY | G1Mesh.NY | G1Mesh.PZ | G1Mesh.NZ));
                parts.Light("bulkhead" + k, new Vector3(xw - side * BulkheadOff, BulkheadY, z), G1Materials.Lin(NocPal.LampWarm, 1f), BulkheadRange, BulkheadIntensity);
                // PA (LOOK_V2 6 light in the air): an in-scatter halo per lens (NocHalos builds them after every builder)
                NocHalos.Queue("AlleyBulkheads", new Vector3(xw - side * 0.06f, BulkheadY, z), G1Materials.Lin(NocPal.LampWarm, NocturnePalette.EmissionSign), BulkheadRange);
            }
        }

        static void Catwalk(G1Parts parts)
        {
            var origin = new Vector3(0f, 9f, 96.8f);
            var deck = parts.Get("catwalk", G1Mat.SteelPainted, origin);
            float z0 = (float)CatwalkZ0, z1 = (float)CatwalkZ1, y0 = (float)AlleyLayout.CatwalkY0, y1 = (float)AlleyLayout.CatwalkY1;
            float hw = (float)AlleyLayout.HalfWidth(0.5 * (z0 + z1)) + (float)AlleyLayout.CatwalkEmbed;
            if (NocLookGlobals.SurfV == 2)
            {
                // Look v2 (TEXTURE_PLAN 3.5, KS02): an open grating plate at the deck top (two-sided NocGrating: the sky and the walls
                // show through it from below), carried by two edge stringers and cross beams every 1.1 m, 5 mm under the plate
                var grate = parts.Get("catwalkGrating", G1Mat.Grating, origin);
                G1Mesh.Quad(grate, new Vector3(-hw, y1, z0), new Vector3(hw, y1, z0), new Vector3(hw, y1, z1), new Vector3(-hw, y1, z1), Vector3.up);
                G1Mesh.Chamfer(deck, new Vector3(-hw, y1 - StringerH, z0), new Vector3(hw, y1 - PlateGap, z0 + StringerW), 0.01f);
                G1Mesh.Chamfer(deck, new Vector3(-hw, y1 - StringerH, z1 - StringerW), new Vector3(hw, y1 - PlateGap, z1), 0.01f);
                for (float x = -hw + 0.55f; x <= hw - 0.5f; x += 1.1f)
                    G1Mesh.Chamfer(deck, new Vector3(x - 0.025f, y1 - 0.10f, z0 + StringerW), new Vector3(x + 0.025f, y1 - PlateGap, z1 - StringerW), 0.005f);
            }
            else G1Mesh.Chamfer(deck, new Vector3(-hw, y0, z0), new Vector3(hw, y1, z1), 0.02f);
            var rail = parts.Get("catwalkRail", G1Mat.LineRod, origin);
            var pts = new List<Vector3>(4); var frames = new List<LoftFrame>(4); var sec = new CrossSection();
            float r = 0.5f * G1Materials.Rod, inner = hw - (float)AlleyLayout.CatwalkEmbed;
            foreach (float z in new[] { z0 + 0.04f, z1 - 0.04f })
            {
                foreach (float h in new[] { y1 + 0.5f, y1 + (float)AlleyLayout.CatwalkRail })
                {
                    pts.Clear(); pts.Add(new Vector3(-hw, h, z)); pts.Add(new Vector3(hw, h, z));
                    G1Mesh.Tube(rail, pts, r, 6, frames, sec);
                }
                for (float x = -inner + 0.3f; x <= inner - 0.29f; x += 1.1f)
                {
                    pts.Clear(); pts.Add(new Vector3(x, y1, z)); pts.Add(new Vector3(x, y1 + (float)AlleyLayout.CatwalkRail, z));
                    G1Mesh.Tube(rail, pts, r, 6, frames, sec);
                }
            }
        }

        static void FireEscapes(G1Parts parts)
        {
            var steel = parts.Get("fireEscape", G1Mat.SteelPainted, new Vector3(0f, 9f, 60f));
            bool grating = NocLookGlobals.SurfV == 2;
            var grate = grating ? parts.Get("fireEscapeGrating", G1Mat.Grating, new Vector3(0f, 9f, 60f)) : null;
            var rail = parts.Get("fireEscapeRail", G1Mat.LineRod, new Vector3(0f, 9f, 60f));
            var pts = new List<Vector3>(4); var frames = new List<LoftFrame>(4); var sec = new CrossSection();
            float r = 0.5f * G1Materials.Rod;
            for (int side = -1; side <= 1; side += 2)
            {
                double[] zsArr = side < 0 ? AlleyLayout.EscapeWestZ : AlleyLayout.EscapeEastZ;
                foreach (double zc in zsArr)
                {
                    float za = (float)zc, zb = za + (float)AlleyLayout.EscapeLandingL;
                    // wall line at the module (the walls are straight 4.4 / 4.6 m sections at the chosen z)
                    float xw = side * (float)AlleyLayout.HalfWidth(zc);
                    float xo = xw - side * (float)AlleyLayout.EscapeLandingD;
                    for (int k = 0; k < AlleyLayout.EscapeLevels; k++)
                    {
                        float y = (float)(AlleyLayout.EscapeFirstY + k * AlleyLayout.EscapeRise);
                        float xmin = Mathf.Min(xw, xo), xmax = Mathf.Max(xw, xo);
                        // landing grate (into the wall by 5 cm)
                        float gx0 = side < 0 ? xmin - 0.05f : xmin, gx1 = side < 0 ? xmax : xmax + 0.05f;
                        if (grating)
                        {
                            // Look v2: the open grating plate in an edge-angle frame (4 angles 5 mm under the plate)
                            G1Mesh.Quad(grate, new Vector3(gx0, y, za), new Vector3(gx1, y, za), new Vector3(gx1, y, zb), new Vector3(gx0, y, zb), Vector3.up);
                            G1Mesh.Chamfer(steel, new Vector3(gx0, y - AngleH, za), new Vector3(gx1, y - PlateGap, za + AngleW), 0.005f);
                            G1Mesh.Chamfer(steel, new Vector3(gx0, y - AngleH, zb - AngleW), new Vector3(gx1, y - PlateGap, zb), 0.005f);
                            G1Mesh.Chamfer(steel, new Vector3(gx0, y - AngleH, za + AngleW), new Vector3(gx0 + AngleW, y - PlateGap, zb - AngleW), 0.005f);
                            G1Mesh.Chamfer(steel, new Vector3(gx1 - AngleW, y - AngleH, za + AngleW), new Vector3(gx1, y - PlateGap, zb - AngleW), 0.005f);
                        }
                        else G1Mesh.Chamfer(steel, new Vector3(gx0, y - 0.06f, za), new Vector3(gx1, y, zb), 0.01f);
                        // outer rail: top and mid rail along z, posts at the corners
                        foreach (float h in new[] { y + 0.5f, y + (float)AlleyLayout.EscapeRail })
                        {
                            pts.Clear(); pts.Add(new Vector3(xo, h, za)); pts.Add(new Vector3(xo, h, zb));
                            G1Mesh.Tube(rail, pts, r, 6, frames, sec);
                        }
                        foreach (float z in new[] { za + 0.03f, zb - 0.03f })
                        {
                            pts.Clear(); pts.Add(new Vector3(xo, y, z)); pts.Add(new Vector3(xo, y + (float)AlleyLayout.EscapeRail, z));
                            G1Mesh.Tube(rail, pts, r, 6, frames, sec);
                        }
                        // wall bracket under the landing
                        G1Mesh.Chamfer(steel, new Vector3(Mathf.Min(xw, xo + side * 0.1f) - (side < 0 ? 0.05f : 0f), y - 0.36f, za + 0.9f),
                            new Vector3(Mathf.Max(xw, xo + side * 0.1f) + (side > 0 ? 0.05f : 0f), y - 0.06f, za + 1.06f), 0.01f);
                        if (k + 1 >= AlleyLayout.EscapeLevels) continue;
                        // flight to the next landing: two stringers and treads (rise 0.18 m) inside the landing footprint
                        float yn = y + (float)AlleyLayout.EscapeRise;
                        float xi = xw - side * 0.08f, xs = xw - side * 0.78f;   // stair width 0.7 m along x
                        Vector3 s0 = new Vector3(0f, y, k % 2 == 0 ? za + 0.15f : zb - 0.15f), s1 = new Vector3(0f, yn - 0.06f, k % 2 == 0 ? zb - 0.15f : za + 0.15f);
                        foreach (float xx in new[] { xi, xs })
                        {
                            pts.Clear(); pts.Add(new Vector3(xx, s0.y, s0.z)); pts.Add(new Vector3(xx, s1.y, s1.z));
                            G1Mesh.Tube(rail, pts, r, 6, frames, sec);
                        }
                        int steps = Mathf.RoundToInt((yn - y) / 0.18f) - 1;
                        for (int t = 1; t < steps; t++)
                        {
                            float u = t / (float)steps;
                            float ty = Mathf.Lerp(s0.y, s1.y, u), tz = Mathf.Lerp(s0.z, s1.z, u);
                            G1Mesh.Chamfer(steel, new Vector3(Mathf.Min(xi, xs), ty - 0.03f, tz - 0.05f), new Vector3(Mathf.Max(xi, xs), ty, tz + 0.05f), 0.005f);
                        }
                    }
                }
            }
        }

        static void Cables(G1Parts parts)
        {
            var mb = parts.Get("cables", G1Mat.Line6mm, new Vector3(0f, 12f, 60f));
            var pts = new List<Vector3>(16); var frames = new List<LoftFrame>(16); var sec = new CrossSection();
            float r = 0.5f * G1Materials.Cable;
            for (int i = 0; i < AlleyLayout.CableCount; i++)
            {
                // spread over z 4..116, heights 5..19 m, each cable slightly diagonal (dz up to 3 m)
                float za = 4f + 112f * (i + 0.5f * G1Mesh.U((uint)i)) / AlleyLayout.CableCount;
                float zb = Mathf.Min(117f, za + 3f * (G1Mesh.U((uint)i + 101u) - 0.3f));
                float ya = Mathf.Lerp((float)AlleyLayout.CableY0, (float)AlleyLayout.CableY1, G1Mesh.U((uint)i + 211u));
                float yb = ya + 1.5f * (G1Mesh.U((uint)i + 307u) - 0.5f);
                if (za > 95f && za < 98.6f) za += 3f;               // keep the catwalk's band clean (KS02)
                float xa = (float)AlleyLayout.HalfWidth(za) + 0.05f, xb = (float)AlleyLayout.HalfWidth(zb) + 0.05f;
                G1Mesh.Catenary(new Vector3(-xa, ya, za), new Vector3(xb, yb, zb), (float)AlleyLayout.CableSag, 12, pts);
                G1Mesh.Tube(mb, pts, r, 6, frames, sec);
            }
        }

        /// <summary>Wall side (-1 west, +1 east), z and palette of blade sign i (even i west, odd i east, x order irrelevant).</summary>
        public static int SignSide(int i) => (i & 1) == 0 ? -1 : 1;
        public static double SignZ(int i) => SignSide(i) < 0 ? AlleyLayout.BladeSignZWest[i / 2] : AlleyLayout.BladeSignZEast[i / 2];
        /// <summary>NocNeon palette of blade sign i (PA, LOOK_V2 10): magenta on ONE sign only (i 0), the other palette-0 signs
        /// SignWhite (slot 4), cyan, amber, and WinNeutral (slot 5) instead of violet.</summary>
        public static int SignPalette(int i)
        {
            int p = i % 4;
            if (p == 0) return i == 0 ? 0 : 4;
            return p == 3 ? 5 : p;
        }

        /// <summary>Centre of blade sign i (its face is the plane z = const, the sign runs 0.5 m out from the wall).</summary>
        public static Vector3 SignCentre(int i)
        {
            float z = (float)SignZ(i);
            float x = SignSide(i) * ((float)AlleyLayout.HalfWidth(z) - (float)AlleyLayout.BladeSignGap - 0.5f * (float)AlleyLayout.BladeSignW);
            float y0 = (float)AlleyLayout.BladeSignY + (float)AlleyLayout.BladeSignYSpan * G1Mesh.U((uint)i + 0x5157u);
            return new Vector3(x, y0 + 0.5f * (float)AlleyLayout.BladeSignH, z);
        }

        static void BladeSigns(G1Parts parts)
        {
            var steel = parts.Get("bladeSignBrackets", G1Mat.SteelPainted, new Vector3(0f, 5f, 60f));
            for (int i = 0; i < AlleyLayout.BladeSignCount; i++)
            {
                Vector3 c = SignCentre(i);
                parts.Sign(c, Quaternion.identity, (float)AlleyLayout.BladeSignW, (float)AlleyLayout.BladeSignH, SignPalette(i));
                float xw = SignSide(i) * (float)AlleyLayout.HalfWidth(c.z);
                foreach (float dy in new[] { -0.9f, 0.9f })
                {
                    float x0 = Mathf.Min(xw, c.x), x1 = Mathf.Max(xw, c.x);
                    G1Mesh.Chamfer(steel, new Vector3(x0 - (SignSide(i) < 0 ? 0.05f : 0f), c.y + dy - 0.03f, c.z + 0.03f),
                        new Vector3(x1 + (SignSide(i) > 0 ? 0.05f : 0f), c.y + dy + 0.03f, c.z + 0.09f), 0.008f);
                }
                if (SignHasLight(i))
                {
                    Color lc = Color.Lerp(BladeColour(i), NocturnePalette.Linear(NocPal.WinNeutral), 0.4f);   // PA: lerp(c, WinNeutral, 0.4)
                    lc.a = 1f;
                    parts.Light("bladeSignLight" + i, new Vector3(c.x - SignSide(i) * 0.6f, c.y, c.z), lc, SignLightRange, SignLightIntensity);
                }
            }
        }

        public static Color BladeColour(int i)
        {
            switch (SignPalette(i))
            {
                case 0: return G1Materials.Lin(NocPal.NeonMagenta, 1f);
                case 1: return G1Materials.Lin(NocPal.NeonCyan, 1f);
                case 2: return G1Materials.Lin(NocPal.SodiumAmber, 1f);
                case 4: return G1Materials.Lin(NocPal.SignWhite, 1f);
                default: return G1Materials.Lin(NocPal.WinNeutral, 1f);
            }
        }

        static void AcUnits(G1Parts parts)
        {
            var mb = parts.Get("acUnits", G1Mat.SteelPainted, new Vector3(0f, 6f, 60f));
            for (int i = 0; i < AlleyLayout.AcZ.Length; i++)
            {
                int side = (int)AlleyLayout.AcSide[i];
                float z = (float)AlleyLayout.AcZ[i], y = (float)AlleyLayout.AcY[i];
                float xw = side * Mathf.Max((float)AlleyLayout.HalfWidth(z - 0.45f), (float)AlleyLayout.HalfWidth(z + 0.45f));
                float x0 = side > 0 ? xw - 0.6f : xw - 0.05f, x1 = side > 0 ? xw + 0.05f : xw + 0.6f;
                G1Mesh.Chamfer(mb, new Vector3(x0, y, z - 0.45f), new Vector3(x1, y + 0.6f, z + 0.45f), 0.02f);
            }
        }

        public static void RegisterSources()
        {
            for (int i = 0; i < AlleyLayout.BladeSignCount; i++)
            {
                Vector3 c = SignCentre(i);
                float h = 0.5f * (float)AlleyLayout.BladeSignH - 0.2f;
                NocWetRegistry.AddSource(c + Vector3.down * h, c + Vector3.up * h, 0.25f, BladeColour(i) * SignRadiance, c.z);
            }
            // the west-wall door bulkheads' lenses (LampWarm x 1.6 discs, rho 0.3 m, every 23 m): the wet floor mirrors them as short
            // warm streaks; the east ones stay out of the registry (beats 1-2 would reach 34 active sources, over the cap of 32)
            for (int k = 0; k < BulkheadCount; k += 2)
            {
                float z = BulkheadZ(k);
                int side = BulkheadSide(k);
                var lens = new Vector3(side * ((float)AlleyLayout.HalfWidth(z) - 0.06f), BulkheadY, z);
                NocWetRegistry.AddSource(lens, lens, 0.3f, G1Materials.Lin(NocPal.LampWarm, NocturnePalette.EmissionSign), z);
            }
            // the mouth: the lit boulevard seen through the 5 m opening, a vertical CloudHot strip (14.5.1 "the mouth light paints
            // the wet floor magenta from s 90")
            NocWetRegistry.AddSource(new Vector3(0f, 0.5f, 124f), new Vector3(0f, 14f, 124f), 2.5f, G1Materials.Lin(NocPal.CloudHot, MouthSourceGain), 120.0);
        }
    }
}
