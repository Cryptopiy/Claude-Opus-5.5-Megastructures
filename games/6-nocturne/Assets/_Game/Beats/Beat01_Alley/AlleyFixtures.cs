// AlleyFixtures: the near detail of beat 1 (MASTER_PROMPT 14.5.1 kit):
//   * the catwalk z 96.0-97.5 (D-029 / GB-12: the spec's z 97.6 put its bottom edge at NDC -0.9404, outside the 3 % margin;
//     97.5 projects to -0.930), y 8.8-9.2 across the alley (KS02: the dark band across the slit at y -0.93..-0.70), ends 0.25 m
//     inside both walls, 1.0 m railings (NocLine rods) with posts every 1.1 m;
//   * fire escapes (FireEscape): 3 modules per side, 4 landings 0.9 x 2.0 m from y 4.2 every 3.4 m, 1.0 m rails, flights between
//     the landings (stringers and 0.18 m treads), wall brackets; everything above y 3.9 (the camera sphere tops out at 1.9 m);
//   * 140 catenary cables 6 mm (NocLine, >= 1.75 px) wall to wall at y 5-19, sag 0.2-0.6 m, ends 5 cm inside the walls; none in the
//     z 86-99 band (the KS02 catwalk and its sight lines stay clean);
//   * 22 blade signs 0.5 x 2.0 m: a chamfered steel housing 0.56 x 2.06 x 0.12 m, a lit face on both sides inset by a 5 cm rim (the
//     cyan / white / neutral ones light-boxes with dark glyphs, NocAlleyRoom kind 4; the magenta and amber ones NocNeon strokes),
//     arms above and below to the wall; bottoms y 3.8-6.0; wet-floor sources; 4 carry a small light (LOOK_V2 6);
//   * 40 AC units (0.80 x 0.55 x 0.32 m condensers between the window columns, a fan grille ring and disc, louvres, brackets, pipe
//     stubs into the wall; INT-39 "an untextured dark cube"), 22 pipes (14 risers from y 2.3 to the roof with clamps, 8 horizontal
//     runs at y 3.4), 24 awnings (18 doors, the two empty rooms, the vending niche, the dead-end door and two first-floor windows),
//     6 bins (two dumpsters and four round bins in the dead end, z < -0.8: behind the start, outside every capsule position);
//   * four steam vents (s 24, 58, 86, 104): flush floor grates and two NocSteam plumes each (cut order item 4);
//   * door bulkheads as real fixtures (back plate, lens, cage bars, hood, conduit; INT-06 "a flat two-tone slab") and the dead-end
//     door lamp (the 20th light: the near walls of KS00 had no source);
//   * the light pools of the lamps on the walls (NocAlleyPool, INT-21): each bulkhead on its own wall and on the facing wall, the
//     dead-end lamp on both walls, the bar light and the vending lights on the facing walls;
//   * the mouth light lerp(HazeLow, CloudHot, 0.5) r 25 i 2.0 at (0, 6, 121) and the mouth haze card (NocAlleyGlow, KS00);
//   * fix wave (D-039 (6)): two work floodlights hung under the catwalk's south stringer at its ends, cross-aimed down the alley at
//     the opposite wall; their wash on the KS02 walls is baked (NocAlleyPool spot mode, G1Mat.AlleyWorkPool), no realtime light.
// Every fixture stays outside the camera sphere's and the capsule's reach: nothing below y 2.3 lies inside the clear width.
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
        /// <summary>AL: the lamp over the dead-end door (LampWarm r 9 i 2.2 at (0, 2.75, -2.2)): the near walls of KS00 had no source.</summary>
        public const float DeadEndLampY = 2.75f, DeadEndLampOff = 0.3f, DeadEndLampRange = 9f, DeadEndLampIntensity = 2.2f;
        /// <summary>The blade signs that carry a real light (4: LOOK_V2 6 beat 1; 20 alley lights with the dead-end lamp, the cap).</summary>
        public static bool SignHasLight(int i) => i % 6 == 0;
        /// <summary>Catwalk extent along z (D-029, GB-12; AlleyLayout keeps the spec's 96.0 / 97.6 for the window keep-out band).</summary>
        public const double CatwalkZ0 = 96.0, CatwalkZ1 = 97.5;
        /// <summary>The band kept clear of cables, pipes and AC units (the KS02 eye at z 88 and its sight lines to the catwalk).</summary>
        public const double ClearZ0 = 86.0, ClearZ1 = 99.0;
        /// <summary>Look v2 grating frames (TEXTURE_PLAN 3.5): stringer / edge-angle height and width, the gap under the plate.</summary>
        public const float StringerH = 0.15f, StringerW = 0.05f, AngleH = 0.06f, AngleW = 0.04f, PlateGap = 0.005f;
        /// <summary>Blade sign housing (W, H grown by the rim) and depth; the lit faces 5 mm proud of it.</summary>
        public const float HousingRim = 0.03f, HousingDepth = 0.12f, FaceProud = 0.005f, FaceInset = 0.05f;
        /// <summary>Steam vents (14.5.1): s and lateral x of each grate.</summary>
        public static readonly float[] VentS = { 24f, 58f, 86f, 104f };
        public static readonly float[] VentX = { 1.0f, -1.2f, 0.9f, -1.0f };
        /// <summary>Kit counts (14.5.1).</summary>
        public const int AcCount = 40, RiserCount = 14, RunCount = 8, AwningCount = 24, BinCount = 6;

        public static void Build(G1Parts parts)
        {
            Catwalk(parts);
            FireEscapes(parts);
            Cables(parts);
            BladeSigns(parts);
            AcUnits(parts);
            Pipes(parts);
            Awnings(parts);
            Bins(parts);
            SteamVents(parts);
            Color mouth = MouthColour();
            parts.Light("mouthLight", new Vector3(0f, (float)AlleyLayout.MouthLightY, (float)AlleyLayout.MouthLightZ),
                mouth, (float)AlleyLayout.MouthLightRange, MouthLightIntensityV2);
            MouthGlow(parts);
            Bulkheads(parts);
            DeadEndLamp(parts);
            Pools(parts);
            WorkLamps(parts);
        }

        public static Color MouthColour()
        {
            Color mouth = Color.Lerp(NocturnePalette.Linear(NocPal.HazeLow), NocturnePalette.Linear(NocPal.CloudHot), 0.5f);
            mouth.a = 1f;
            return mouth;
        }

        /// <summary>Bulkhead z and wall side (-1 west first, alternating) of bulkhead k.</summary>
        public static float BulkheadZ(int k) => BulkheadZ0 + BulkheadPitch * k;
        public static int BulkheadSide(int k) => (k & 1) == 0 ? -1 : 1;

        /// <summary>Ten door bulkheads (LOOK_V2 6 beat 1): a LampWarm lens (0.18 x 0.10 m, 6 cm proud) on a steel back plate, three cage
        /// bars across it, a hood above and a conduit up to a junction box at y 2.9 (under the awnings); its light 0.25 m off the wall at y 2.6, grazing the wall.</summary>
        static void Bulkheads(G1Parts parts)
        {
            var lens = parts.Get("bulkheadLens", G1Mat.EmitLamp, new Vector3(0f, BulkheadY, 60f));
            var steel = parts.Get("bulkheadFixture", G1Mat.SteelPainted, new Vector3(0f, BulkheadY, 60f));
            for (int k = 0; k < BulkheadCount; k++)
            {
                float z = BulkheadZ(k);
                int side = BulkheadSide(k);
                float xw = side * (float)AlleyLayout.HalfWidth(z);
                Fixture(lens, steel, xw, -side, BulkheadY, z, true);
                parts.Light("bulkhead" + k, new Vector3(xw - side * BulkheadOff, BulkheadY, z), G1Materials.Lin(NocPal.LampWarm, 1f), BulkheadRange, BulkheadI);
                // PA (LOOK_V2 6 light in the air): an in-scatter halo per lens (NocHalos builds them after every builder)
                NocHalos.Queue("AlleyBulkheads", new Vector3(xw - side * 0.06f, BulkheadY, z), G1Materials.Lin(NocPal.LampWarm, NocturnePalette.EmissionSign), BulkheadRange);
            }
        }

        /// <summary>One wall lamp at wall coordinate w along `outAxis` (x for side walls), centre height y, along-wall coordinate a:
        /// back plate, lens, cage, hood; with conduit, a conduit up the wall to a junction box at y 2.9.</summary>
        static void Fixture(MeshBuilder lens, MeshBuilder steel, float w, int outSign, float y, float a, bool sideWall, bool conduit = true)
        {
            // local helper: a box from (out0..out1 off the wall, y0..y1, a0..a1 along the wall)
            void B(MeshBuilder mb, float o0, float o1, float y0, float y1, float a0, float a1, float bev)
            {
                float x0 = w + outSign * o0, x1 = w + outSign * o1;
                Vector3 mn = sideWall ? new Vector3(Mathf.Min(x0, x1), y0, a0) : new Vector3(a0, y0, Mathf.Min(x0, x1));
                Vector3 mx = sideWall ? new Vector3(Mathf.Max(x0, x1), y1, a1) : new Vector3(a1, y1, Mathf.Max(x0, x1));
                G1Mesh.Chamfer(mb, mn, mx, bev);
            }
            B(steel, -0.01f, 0.025f, y - 0.15f, y + 0.15f, a - 0.12f, a + 0.12f, 0.008f);                 // back plate
            B(lens, 0.02f, 0.065f, y - 0.05f, y + 0.05f, a - 0.09f, a + 0.09f, 0.012f);                     // lens
            foreach (float da in new[] { -0.05f, 0f, 0.05f })                                                // cage bars
                B(steel, 0.068f, 0.08f, y - 0.065f, y + 0.065f, a + da - 0.006f, a + da + 0.006f, 0.003f);
            B(steel, 0.02f, 0.13f, y + 0.07f, y + 0.09f, a - 0.13f, a + 0.13f, 0.006f);                     // hood
            if (conduit)
            {
                B(steel, 0.0f, 0.03f, y + 0.15f, 2.9f, a - 0.015f, a + 0.015f, 0.006f);                     // conduit
                B(steel, -0.01f, 0.05f, 2.9f, 2.98f, a - 0.045f, a + 0.045f, 0.006f);                      // junction box
            }
        }

        /// <summary>The lamp over the dead-end door (z -2.5, facing the alley): fixture, light, halo.</summary>
        static void DeadEndLamp(G1Parts parts)
        {
            var lens = parts.Get("bulkheadLens", G1Mat.EmitLamp, new Vector3(0f, BulkheadY, 60f));
            var steel = parts.Get("bulkheadFixture", G1Mat.SteelPainted, new Vector3(0f, BulkheadY, 60f));
            float zw = (float)AlleyLayout.DeadEndZ;
            Fixture(lens, steel, zw, 1, DeadEndLampY, 0f, false, false);
            var p = new Vector3(0f, DeadEndLampY, zw + DeadEndLampOff);
            parts.Light("deadEndLamp", p, G1Materials.Lin(NocPal.LampWarm, 1f), DeadEndLampRange, DeadEndLampIntensity);
            NocHalos.Queue("AlleyBulkheads", new Vector3(0f, DeadEndLampY, zw + 0.06f), G1Materials.Lin(NocPal.LampWarm, NocturnePalette.EmissionSign), DeadEndLampRange);
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

        static bool InClearBand(float z) => z > ClearZ0 && z < ClearZ1;

        static void Cables(G1Parts parts)
        {
            var mb = parts.Get("cables", G1Mat.Line6mm, new Vector3(0f, 12f, 60f));
            var pts = new List<Vector3>(16); var frames = new List<LoftFrame>(16); var sec = new CrossSection();
            float r = 0.5f * G1Materials.Cable;
            int built = 0;
            for (int i = 0; built < AlleyLayout.CableCount && i < 4 * AlleyLayout.CableCount; i++)
            {
                // spread over z 3..117, heights 5..19 m, each cable slightly diagonal (dz up to +-2.5 m), bundles of 2-3 nearly parallel
                uint key = (uint)i;
                float za = 3f + 114f * G1Mesh.U(key + 7001u);
                float zb = Mathf.Clamp(za + 5f * (G1Mesh.U(key + 101u) - 0.5f), 3f, 117.5f);
                if (InClearBand(za) || InClearBand(zb) || (za - ClearZ0) * (zb - ClearZ0) < 0f || (za - ClearZ1) * (zb - ClearZ1) < 0f) continue;
                float ya = Mathf.Lerp((float)AlleyLayout.CableY0, (float)AlleyLayout.CableY1, Mathf.Pow(G1Mesh.U(key + 211u), 1.3f));
                float yb = ya + 1.6f * (G1Mesh.U(key + 307u) - 0.5f);
                float sag = Mathf.Lerp(0.2f, 0.6f, G1Mesh.U(key + 401u));
                float xa = (float)AlleyLayout.HalfWidth(za) + 0.05f, xb = (float)AlleyLayout.HalfWidth(zb) + 0.05f;
                bool flip = G1Mesh.U(key + 503u) < 0.5f;
                G1Mesh.Catenary(new Vector3(flip ? xa : -xa, ya, za), new Vector3(flip ? -xb : xb, yb, zb), sag, 12, pts);
                G1Mesh.Tube(mb, pts, r, 6, frames, sec);
                built++;
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

        /// <summary>22 blade signs (INT-06 "flat hanging-sign slabs"): a chamfered steel housing, a NocNeon face on each side inset by a
        /// 5 cm rim, arms above and below to the wall.</summary>
        static void BladeSigns(G1Parts parts)
        {
            var steel = parts.Get("bladeSignBrackets", G1Mat.SteelPainted, new Vector3(0f, 5f, 60f));
            float W = (float)AlleyLayout.BladeSignW, H = (float)AlleyLayout.BladeSignH;
            for (int i = 0; i < AlleyLayout.BladeSignCount; i++)
            {
                Vector3 c = SignCentre(i);
                int side = SignSide(i);
                float hd = 0.5f * HousingDepth;
                G1Mesh.Chamfer(steel, new Vector3(c.x - 0.5f * W - HousingRim, c.y - 0.5f * H - HousingRim, c.z - hd),
                    new Vector3(c.x + 0.5f * W + HousingRim, c.y + 0.5f * H + HousingRim, c.z + hd), 0.015f);
                float fw = W + 2f * HousingRim - 2f * FaceInset, fh = H + 2f * HousingRim - 2f * FaceInset;
                if (IsLightBox(i))
                {
                    var box = parts.Get("bladeLightBoxes", G1Mat.AlleySign, new Vector3(0f, 5f, 60f));
                    Color lc = LightBoxColour(i);
                    LightBoxFace(box, new Vector3(c.x, c.y, c.z - hd - FaceProud), fw, fh, -1f, lc, (uint)i);
                    LightBoxFace(box, new Vector3(c.x, c.y, c.z + hd + FaceProud), fw, fh, 1f, lc, (uint)i + 97u);
                }
                else
                {
                    parts.Sign(new Vector3(c.x, c.y, c.z - hd - FaceProud), Quaternion.identity, fw, fh, SignPalette(i));
                    parts.Sign(new Vector3(c.x, c.y, c.z + hd + FaceProud), Quaternion.Euler(0f, 180f, 0f), fw, fh, SignPalette(i));
                }
                float xw = side * (float)AlleyLayout.HalfWidth(c.z);
                float xo = c.x - side * (0.5f * W + HousingRim - 0.06f);           // the arm reaches the housing's outer third
                float x0 = Mathf.Min(xw + side * 0.05f, xo), x1 = Mathf.Max(xw + side * 0.05f, xo);
                float top = c.y + 0.5f * H + HousingRim, bot = c.y - 0.5f * H - HousingRim;
                G1Mesh.Chamfer(steel, new Vector3(x0, top, c.z - 0.025f), new Vector3(x1, top + 0.05f, c.z + 0.025f), 0.008f);
                G1Mesh.Chamfer(steel, new Vector3(x0, bot - 0.05f, c.z - 0.025f), new Vector3(x1, bot, c.z + 0.025f), 0.008f);
                if (SignHasLight(i))
                {
                    Color lc = Color.Lerp(BladeColour(i), NocturnePalette.Linear(NocPal.WinNeutral), 0.4f);   // PA: lerp(c, WinNeutral, 0.4)
                    lc.a = 1f;
                    parts.Light("bladeSignLight" + i, new Vector3(c.x - side * 0.6f, c.y, c.z), lc, SignLightRange, SignLightIntensity);
                }
            }
        }

        /// <summary>The cyan, white and neutral blades are light-boxes (a lit board with dark glyphs, the real alley sign; INT-06 "flat
        /// slabs with stick glyphs"); the one magenta and the amber blades keep their neon strokes on the dark housing.</summary>
        public static bool IsLightBox(int i) { int p = SignPalette(i); return p == 1 || p == 4 || p == 5; }
        public const float LightBoxGain = 1.4f;

        /// <summary>Light-box board colour (linear albedo; x LightBoxGain self-lit): the blade colour toward white, dimmed.</summary>
        public static Color LightBoxColour(int i)
        {
            Color c = Color.Lerp(BladeColour(i), Color.white, SignPalette(i) == 1 ? 0.3f : 0.15f) * (SignPalette(i) == 1 ? 0.30f : 0.26f);
            c.a = 1f;
            return c;
        }

        /// <summary>One light-box face facing nz (+1 / -1 along z): uv = one glyph column across (0.20) and five rows down (1.35),
        /// the glyph set shifted per face by seed.</summary>
        static void LightBoxFace(MeshBuilder mb, Vector3 c, float w, float h, float nz, Color col, uint seed)
        {
            var saved = mb.Color;
            mb.Color = new Color32((byte)(Mathf.Clamp01(col.r) * 255f), (byte)(Mathf.Clamp01(col.g) * 255f), (byte)(Mathf.Clamp01(col.b) * 255f), 4 * 32);
            float v0 = 1.35f * 7f * G1Mesh.U(seed + 0x5151u);                  // a different stretch of the glyph field per face
            float u0 = 0.2f * Mathf.Floor(40f * G1Mesh.U(seed + 0x7171u));
            Vector3 A = c + new Vector3(-0.5f * w, -0.5f * h, 0f), B = c + new Vector3(-0.5f * w, 0.5f * h, 0f), C = c + new Vector3(0.5f * w, 0.5f * h, 0f), D = c + new Vector3(0.5f * w, -0.5f * h, 0f);
            Vector2 uA = new Vector2(u0, v0), uB = new Vector2(u0, v0 + 1.35f), uC = new Vector2(u0 + 0.2f, v0 + 1.35f), uD = new Vector2(u0 + 0.2f, v0);
            Vector3 want = new Vector3(0f, 0f, nz);
            if (Vector3.Dot(Vector3.Cross(B - A, C - A), want) < 0f) mb.FlatQuad(A, D, C, B, uA, uD, uC, uB);
            else mb.FlatQuad(A, B, C, D, uA, uB, uC, uD);
            mb.Color = saved;
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

        // ---------------------------------------------------------------------------------------------------------- AC units
        static List<Vector4> s_ac;

        /// <summary>The 40 AC units: candidates between the window columns (z = 2 + 4.5 k + 2.25) on floors 1.. (y = 2.75 + 3.2 f, just
        /// under the sill line), clear of the escapes, blade signs, the bar frontage, the clear band and the mouth; the first 40 by hash.</summary>
        static List<Vector4> AcList()
        {
            if (s_ac != null) return s_ac;
            var cand = new List<Vector4>(256);
            var key = new List<float>(256);
            for (int side = -1; side <= 1; side += 2)
            {
                int floors = side < 0 ? 4 : 7;
                for (int k = 0; k < 26; k++)
                {
                    float z = 2f + 4.5f * k + 2.25f;
                    if (z > 117f || InClearBand(z - 0.6f) || InClearBand(z + 0.6f)) continue;
                    bool esc = false;
                    foreach (double e in side < 0 ? AlleyLayout.EscapeWestZ : AlleyLayout.EscapeEastZ) if (z + 0.6f > e - 0.5 && z - 0.6f < e + AlleyLayout.EscapeLandingL + 0.5) esc = true;
                    if (esc) continue;
                    for (int f = 1; f <= floors; f++)
                    {
                        float y = 2.75f + 3.2f * f;
                        if (side < 0 && z > AlleyLayout.BarZ0 - 1.5 && z < AlleyLayout.BarZ1 + 1.5 && y < 6.5f) continue;
                        bool sign = false;
                        for (int i = 0; i < AlleyLayout.BladeSignCount; i++)
                        {
                            if (SignSide(i) != side) continue;
                            Vector3 c = SignCentre(i);
                            if (Mathf.Abs(c.z - z) < 1.0f && y + 0.7f > c.y - 1.15f && y - 0.2f < c.y + 1.15f) sign = true;
                        }
                        if (sign) continue;
                        cand.Add(new Vector4(side, z, y, f));
                        key.Add(G1Mesh.U((uint)(k * 37 + f * 1013 + (side > 0 ? 50021 : 0))));
                    }
                }
            }
            var idx = new List<int>(cand.Count);
            for (int i = 0; i < cand.Count; i++) idx.Add(i);
            idx.Sort((a, b) => key[a].CompareTo(key[b]));
            s_ac = new List<Vector4>(AcCount);
            for (int i = 0; i < idx.Count && s_ac.Count < AcCount; i++) s_ac.Add(cand[idx[i]]);
            return s_ac;
        }

        /// <summary>AC unit i: wall side, centre z, bottom y and a 0..1 variant.</summary>
        public static void Ac(int i, out int side, out float z, out float y, out float variant)
        {
            var l = AcList();
            if (i < 0 || i >= l.Count) { side = 0; z = -100f; y = -100f; variant = 0f; return; }
            Vector4 v = l[i];
            side = (int)v.x; z = v.y; y = v.z; variant = G1Mesh.U((uint)i + 0xAC00u);
        }

        /// <summary>40 wall condensers (INT-39): body, fan disc and grille ring with two bars, side louvres, two brackets, two pipe
        /// stubs into the wall.</summary>
        static void AcUnits(G1Parts parts)
        {
            var body = parts.Get("acUnits", G1Mat.SteelPainted, new Vector3(0f, 9f, 60f));
            var dark = parts.Get("acFans", G1Mat.SteelBare, new Vector3(0f, 9f, 60f));
            var pts = new List<Vector3>(4); var frames = new List<LoftFrame>(4); var sec = new CrossSection();
            int n = AcList().Count;
            for (int i = 0; i < n; i++)
            {
                Ac(i, out int side, out float z, out float y, out float v);
                float xw = side * Mathf.Max((float)AlleyLayout.HalfWidth(z - 0.45f), (float)AlleyLayout.HalfWidth(z + 0.45f));
                int o = -side;                                     // toward the alley
                float D = 0.32f, Wz = 0.80f, Hy = 0.55f;
                float xf = xw + o * D;                             // front face x
                Vector3 mn = new Vector3(Mathf.Min(xw - o * 0.02f, xf), y, z - 0.5f * Wz), mx = new Vector3(Mathf.Max(xw - o * 0.02f, xf), y + Hy, z + 0.5f * Wz);
                G1Mesh.Chamfer(body, mn, mx, 0.02f);
                // fan: dark disc 1 cm proud, grille ring and two bars 1.5 cm proud
                float fz = z - 0.12f, fy = y + 0.5f * Hy;
                MeshKit.Disc(dark, new Vector3(xf + o * 0.008f, fy, fz), new Vector3(o, 0f, 0f), 0.2f, 24);
                MeshKit.Torus(body, new Vector3(xf + o * 0.012f, fy, fz), new Vector3(o, 0f, 0f), 0.2f, 0.012f, 20, 6);
                G1Mesh.Chamfer(body, new Vector3(Mathf.Min(xf, xf + o * 0.02f), fy - 0.2f, fz - 0.008f), new Vector3(Mathf.Max(xf, xf + o * 0.02f), fy + 0.2f, fz + 0.008f), 0.003f);
                G1Mesh.Chamfer(body, new Vector3(Mathf.Min(xf, xf + o * 0.02f), fy - 0.008f, fz - 0.2f), new Vector3(Mathf.Max(xf, xf + o * 0.02f), fy + 0.008f, fz + 0.2f), 0.003f);
                // louvres on the right part of the front
                for (int k = 0; k < 6; k++)
                {
                    float ly = y + 0.08f + k * 0.07f;
                    G1Mesh.Chamfer(body, new Vector3(Mathf.Min(xf, xf + o * 0.025f), ly, z + 0.14f), new Vector3(Mathf.Max(xf, xf + o * 0.025f), ly + 0.02f, z + 0.36f), 0.004f);
                }
                // brackets under the body (L arms from the wall)
                foreach (float bz in new[] { z - 0.3f, z + 0.3f })
                    G1Mesh.Chamfer(body, new Vector3(Mathf.Min(xw, xw + o * (D + 0.03f)), y - 0.05f, bz - 0.02f), new Vector3(Mathf.Max(xw, xw + o * (D + 0.03f)), y, bz + 0.02f), 0.005f);
                // two refrigerant pipe stubs from the side into the wall, and a drip pipe under it for some
                for (int k = 0; k < 2; k++)
                {
                    float pz = z + 0.5f * Wz + 0.05f, py = y + 0.12f + k * 0.08f;
                    pts.Clear(); pts.Add(new Vector3(xw + o * 0.12f, py, pz - 0.06f)); pts.Add(new Vector3(xw + o * 0.12f, py, pz + 0.05f)); pts.Add(new Vector3(xw - o * 0.02f, py, pz + 0.10f));
                    G1Mesh.Tube(body, pts, 0.012f, 6, frames, sec);
                }
                if (v < 0.5f)
                {
                    pts.Clear(); pts.Add(new Vector3(xw + o * 0.06f, y, z + 0.3f)); pts.Add(new Vector3(xw + o * 0.06f, y - 0.6f - 0.8f * v, z + 0.3f));
                    G1Mesh.Tube(body, pts, 0.01f, 6, frames, sec);
                }
            }
        }

        // ---------------------------------------------------------------------------------------------------------- pipes
        /// <summary>14 risers (r 5.5 cm, from an elbow into the wall at y 2.3 up to an elbow under the podium top, clamps every 2.4 m)
        /// and 8 horizontal runs at y 3.4 (r 4 cm, brackets every 2 m), all between the window columns and clear of the fixtures.</summary>
        static void Pipes(G1Parts parts)
        {
            var mb = parts.Get("alleyPipes", G1Mat.SteelPainted, new Vector3(0f, 10f, 60f));
            var pts = new List<Vector3>(8); var frames = new List<LoftFrame>(8); var sec = new CrossSection();
            int risers = 0;
            for (int k = 0; k < 26 && risers < RiserCount; k++)
            {
                int side = (k & 1) == 0 ? 1 : -1;
                if (G1Mesh.U((uint)k + 0x9191u) < 0.35f) continue;
                float z = 2f + 4.5f * k + 1.1f;
                if (z > 116f || InClearBand(z)) continue;
                if (side < 0 && z > AlleyLayout.BarZ0 - 1.0 && z < AlleyLayout.BarZ1 + 1.0) continue;
                bool hit = false;
                foreach (double e in side < 0 ? AlleyLayout.EscapeWestZ : AlleyLayout.EscapeEastZ) if (z > e - 0.4 && z < e + AlleyLayout.EscapeLandingL + 0.4) hit = true;
                for (int i = 0; i < AlleyLayout.BladeSignCount; i++) if (SignSide(i) == side && Mathf.Abs((float)SignZ(i) - z) < 0.5f) hit = true;
                for (int i = 0; i < AcList().Count; i++) { Ac(i, out int s, out float az, out _, out _); if (s == side && Mathf.Abs(az - z) < 0.65f) hit = true; }
                if (hit) continue;
                double[] doors = side < 0 ? AlleyLayout.DoorZWest : AlleyLayout.DoorZEast;
                if (AlleyDoors.InDoor(doors, z - 0.2) || AlleyDoors.InDoor(doors, z + 0.2)) continue;
                for (int r = 0; r < AlleyRooms.Count; r++) { AlleyRooms.Window(r, out int rs, out float ra, out float rb); if (rs == side && z > ra - 0.5f && z < rb + 0.5f) hit = true; }
                if (hit) continue;
                if (side > 0 && z > VendingMachines.NicheZ0 - 0.3 && z < VendingMachines.NicheZ1 + 0.3) continue;
                float xw = side * (float)AlleyLayout.HalfWidth(z);
                float top = (float)(side < 0 ? AlleyLayout.WestPodiumH : AlleyLayout.EastPodiumH) - 0.4f;
                float xp = xw - side * 0.09f;
                pts.Clear();
                pts.Add(new Vector3(xw + side * 0.02f, 2.3f, z)); pts.Add(new Vector3(xp, 2.45f, z));
                pts.Add(new Vector3(xp, top - 0.15f, z)); pts.Add(new Vector3(xw + side * 0.02f, top, z));
                G1Mesh.Tube(mb, pts, 0.055f, 8, frames, sec);
                for (float y = 3.2f; y < top - 0.5f; y += 2.4f)
                    G1Mesh.Chamfer(mb, new Vector3(Mathf.Min(xw, xp - side * 0.07f), y, z - 0.07f), new Vector3(Mathf.Max(xw, xp - side * 0.07f), y + 0.05f, z + 0.07f), 0.006f);
                risers++;
            }
            // utility cabinets near the start (KS00's near walls, LOOK_V2 7 meso: 1-10 m detail at eye height): a meter cabinet on each
            // wall above y 2.35 (0.14 m proud) with its conduits, and a fuse box on the dead end beside the door
            foreach (var u in new[] { new Vector4(-1f, 3.0f, 2.35f, 0.55f), new Vector4(1f, 2.6f, 2.45f, 0.45f), new Vector4(-1f, 10.4f, 2.4f, 0.4f), new Vector4(1f, 13.0f, 2.5f, 0.5f) })
            {
                int side = (int)u.x;
                float z = u.y, y0 = u.z, w = u.w, xw = side * (float)AlleyLayout.HalfWidth(z);
                G1Mesh.Chamfer(mb, new Vector3(Mathf.Min(xw + side * 0.02f, xw - side * 0.14f), y0, z - 0.5f * w), new Vector3(Mathf.Max(xw + side * 0.02f, xw - side * 0.14f), y0 + 0.75f, z + 0.5f * w), 0.012f);
                G1Mesh.Chamfer(mb, new Vector3(Mathf.Min(xw, xw - side * 0.155f), y0 + 0.08f, z - 0.5f * w + 0.04f), new Vector3(Mathf.Max(xw, xw - side * 0.155f), y0 + 0.67f, z - 0.5f * w + 0.06f), 0.004f);   // hinge strip
                foreach (float dz in new[] { -0.12f, 0.0f, 0.12f })
                {
                    pts.Clear(); pts.Add(new Vector3(xw - side * 0.05f, y0 + 0.75f, z + dz)); pts.Add(new Vector3(xw - side * 0.05f, y0 + 1.6f + 0.3f * dz, z + dz));
                    pts.Add(new Vector3(xw + side * 0.02f, y0 + 1.75f + 0.3f * dz, z + dz));
                    G1Mesh.Tube(mb, pts, 0.016f, 6, frames, sec);
                }
            }
            {
                float zw = (float)AlleyLayout.DeadEndZ;
                G1Mesh.Chamfer(mb, new Vector3(1.0f, 1.2f, zw - 0.02f), new Vector3(1.45f, 1.8f, zw + 0.12f), 0.012f);
            }
            // horizontal runs at y 3.4 (west clear of the bar frontage z 38..54; east clear of the niche top 2.4: fine at 3.4)
            float[] runZ0 = { 3f, 20f, 57f, 72f, 8f, 26f, 70f, 102f };
            float[] runZ1 = { 15f, 34f, 66f, 84f, 21f, 40f, 82f, 114f };
            for (int r = 0; r < RunCount; r++)
            {
                int side = r < 4 ? -1 : 1;
                float y = 3.4f + (r & 1) * 0.12f;
                pts.Clear();
                for (float z = runZ0[r]; z <= runZ1[r] + 1e-3f; z += 0.5f) pts.Add(new Vector3(side * ((float)AlleyLayout.HalfWidth(z) - 0.07f), y, z));
                G1Mesh.Tube(mb, pts, 0.04f, 8, frames, sec);
                for (float z = runZ0[r] + 0.6f; z < runZ1[r]; z += 2f)
                {
                    float xw = side * (float)AlleyLayout.HalfWidth(z);
                    G1Mesh.Chamfer(mb, new Vector3(Mathf.Min(xw, xw - side * 0.12f), y - 0.06f, z - 0.025f), new Vector3(Mathf.Max(xw, xw - side * 0.12f), y - 0.035f, z + 0.025f), 0.005f);
                }
            }
        }

        // ---------------------------------------------------------------------------------------------------------- awnings
        /// <summary>A sloped canopy along the wall from z0 to z1: its wall edge at yt, 0.7 m out, 0.18 m drop, 3 cm thick, a 0.15 m
        /// valance at the front; above y 2.7 everywhere (the capsule tops out at 1.8 m).</summary>
        public static void Awning(MeshBuilder mb, int side, float z0, float z1, float yt, float depth = 0.7f)
        {
            float xa = side * (float)AlleyLayout.HalfWidth(z0), xb = side * (float)AlleyLayout.HalfWidth(z1);
            Vector3 o = new Vector3(-side * depth, -0.18f, 0f), th = new Vector3(0f, -0.03f, 0f);
            Vector3 A = new Vector3(xa, yt, z0), B = new Vector3(xb, yt, z1), C = B + o, D = A + o;
            Vector3 up = new Vector3(-side * 0.25f, 1f, 0f), outN = new Vector3(-side, 0f, 0f);
            G1Mesh.Quad(mb, A, B, C, D, up);
            G1Mesh.Quad(mb, A + th, B + th, C + th, D + th, Vector3.down);
            G1Mesh.Quad(mb, A, D, D + th, A + th, Vector3.back);
            G1Mesh.Quad(mb, B, C, C + th, B + th, Vector3.forward);
            Vector3 v = new Vector3(0f, -0.15f, 0f);
            G1Mesh.Quad(mb, D, C, C + v, D + v, outN);
            G1Mesh.Quad(mb, D + new Vector3(side * 0.01f, 0f, 0f), C + new Vector3(side * 0.01f, 0f, 0f), C + v + new Vector3(side * 0.01f, 0f, 0f), D + v + new Vector3(side * 0.01f, 0f, 0f), -outN);
        }

        static void Awnings(G1Parts parts)
        {
            var mb = parts.Get("awnings", G1Mat.SteelPainted, new Vector3(0f, 3f, 60f));
            int n = 0;
            for (int side = -1; side <= 1; side += 2)
                foreach (double d in side < 0 ? AlleyLayout.DoorZWest : AlleyLayout.DoorZEast)
                {
                    Awning(mb, side, (float)d - 0.8f, (float)d + 0.8f, 3.05f, 0.6f); n++;
                }
            // the two empty rooms' windows, the vending niche, the dead-end door (its own, below), two first-floor windows
            for (int r = 0; r < AlleyRooms.Count; r++) { AlleyRooms.Window(r, out int s, out float za, out float zb); Awning(mb, s, za - 0.3f, zb + 0.3f, 2.95f, 0.7f); n++; }
            Awning(mb, 1, (float)VendingMachines.NicheZ0 - 0.2f, (float)VendingMachines.NicheZ1 + 0.2f, 2.9f, 0.8f); n++;
            // the dead-end door: a short flat hood on the dead-end face (z -2.5), above its lamp
            float zw = (float)AlleyLayout.DeadEndZ;
            G1Mesh.Chamfer(mb, new Vector3(-0.8f, 3.0f, zw - 0.02f), new Vector3(0.8f, 3.05f, zw + 0.55f), 0.01f); n++;
            foreach (float z in new[] { 74f, 110f })
            {
                if (n >= AwningCount) break;
                Awning(mb, z < 100f ? -1 : 1, z - 0.8f, z + 0.8f, 5.25f, 0.55f); n++;
            }
        }

        // ---------------------------------------------------------------------------------------------------------- bins
        /// <summary>Two dumpsters (1.4 x 1.1 x 1.0 m, lids, wheels) and four round bins (r 0.27, 0.9 m) in the dead end, z < -0.8.</summary>
        static void Bins(G1Parts parts)
        {
            var mb = parts.Get("bins", G1Mat.SteelPainted, new Vector3(0f, 0.5f, -1.5f));
            var pts = new List<Vector3>(4); var frames = new List<LoftFrame>(4); var sec = new CrossSection();
            float zb = (float)AlleyLayout.DeadEndZ + 0.05f;
            foreach (float x0 in new[] { -2.15f, 0.75f })
            {
                G1Mesh.Chamfer(mb, new Vector3(x0, 0.12f, zb), new Vector3(x0 + 1.4f, 1.1f, zb + 1.0f), 0.03f);
                G1Mesh.Chamfer(mb, new Vector3(x0 - 0.02f, 1.1f, zb - 0.02f), new Vector3(x0 + 1.42f, 1.16f, zb + 1.04f), 0.015f);   // lid
                foreach (float wx in new[] { x0 + 0.12f, x0 + 1.28f })
                    G1Mesh.Chamfer(mb, new Vector3(wx - 0.05f, 0f, zb + 0.75f), new Vector3(wx + 0.05f, 0.12f, zb + 0.85f), 0.01f);
            }
            foreach (float x in new[] { -1.9f, -1.32f, 1.32f, 1.9f })
            {
                Vector3 b = new Vector3(x, 0f, -1.2f);
                pts.Clear(); pts.Add(b); pts.Add(b + new Vector3(0f, 0.9f, 0f));
                G1Mesh.Tube(mb, pts, 0.27f, 16, frames, sec);
                MeshKit.Disc(mb, b + new Vector3(0f, 0.9f, 0f), Vector3.up, 0.27f, 16);
                pts.Clear(); pts.Add(b + new Vector3(0f, 0.88f, 0f)); pts.Add(b + new Vector3(0f, 0.95f, 0f));
                G1Mesh.Tube(mb, pts, 0.29f, 16, frames, sec);                                                     // lid rim
                MeshKit.Disc(mb, b + new Vector3(0f, 0.95f, 0f), Vector3.up, 0.29f, 16);
            }
        }

        // ---------------------------------------------------------------------------------------------------------- steam vents
        /// <summary>Four flush floor grates (0.8 x 0.5 m: a dark pit plate 4 mm and the grating 9 mm over the floor in an 8 mm
        /// frame: inside Q1's 1 cm walkable-top tolerance; AL-final had 12 / 13 mm, 45 Q1 rays HIGH) and two NocSteam plumes each (1.3 / 1.0 m wide, 4.2 / 3.4 m tall, per-plume seed in the vertex colour).</summary>
        static void SteamVents(G1Parts parts)
        {
            var frame = parts.Get("ventFrames", G1Mat.SteelPainted, new Vector3(0f, 0f, 60f));
            var pit = parts.Get("ventPits", G1Mat.SteelBare, new Vector3(0f, 0f, 60f));
            var grate = parts.Get("ventGratings", G1Mat.Grating, new Vector3(0f, 0f, 60f));
            var steam = parts.Get("steam", G1Mat.Steam, new Vector3(0f, 2f, 60f));
            for (int v = 0; v < VentS.Length; v++)
            {
                float x = VentX[v], z = VentS[v];
                G1Mesh.Quad(pit, new Vector3(x - 0.38f, 0.004f, z - 0.23f), new Vector3(x + 0.38f, 0.004f, z - 0.23f), new Vector3(x + 0.38f, 0.004f, z + 0.23f), new Vector3(x - 0.38f, 0.004f, z + 0.23f), Vector3.up);
                G1Mesh.Quad(grate, new Vector3(x - 0.38f, 0.009f, z - 0.23f), new Vector3(x + 0.38f, 0.009f, z - 0.23f), new Vector3(x + 0.38f, 0.009f, z + 0.23f), new Vector3(x - 0.38f, 0.009f, z + 0.23f), Vector3.up);
                G1Mesh.Box(frame, new Vector3(x - 0.42f, 0f, z - 0.27f), new Vector3(x + 0.42f, 0.008f, z - 0.23f), G1Mesh.PY | G1Mesh.NZ | G1Mesh.PZ | G1Mesh.PX | G1Mesh.NX);
                G1Mesh.Box(frame, new Vector3(x - 0.42f, 0f, z + 0.23f), new Vector3(x + 0.42f, 0.008f, z + 0.27f), G1Mesh.PY | G1Mesh.NZ | G1Mesh.PZ | G1Mesh.PX | G1Mesh.NX);
                G1Mesh.Box(frame, new Vector3(x - 0.42f, 0f, z - 0.23f), new Vector3(x - 0.38f, 0.008f, z + 0.23f), G1Mesh.PY | G1Mesh.PX | G1Mesh.NX);
                G1Mesh.Box(frame, new Vector3(x + 0.38f, 0f, z - 0.23f), new Vector3(x + 0.42f, 0.008f, z + 0.23f), G1Mesh.PY | G1Mesh.PX | G1Mesh.NX);
                byte lamp = SteamLampByte(new Vector3(x, 1.6f, z));
                Plume(steam, new Vector3(x, 0f, z), 1.3f, 4.2f, (byte)(v * 61 + 17), lamp);
                Plume(steam, new Vector3(x + 0.1f, 0f, z + 0.12f), 1.0f, 3.4f, (byte)(v * 61 + 140), lamp);
            }
        }

        /// <summary>Fix wave C38: the irradiance of the alley lamps at a plume, baked without the realtime lights' range window
        /// (the r 7 window left the s 24 plume 0.001 of the z 17.5 bulkhead), E = sum I / (d^2 + 0.25) over the ten bulkheads and
        /// the dead-end lamp (LampWarm), as a byte E / SteamLampScale (NocSteam: x _LampColor x _Scatter, isotropic).</summary>
        public const float SteamLampScale = 0.25f;

        public static byte SteamLampByte(Vector3 p)
        {
            float e = 0f;
            for (int k = 0; k < BulkheadCount; k++)
            {
                float z = BulkheadZ(k);
                int side = BulkheadSide(k);
                var l = new Vector3(side * ((float)AlleyLayout.HalfWidth(z) - BulkheadOff), BulkheadY, z);
                e += BulkheadI / ((l - p).sqrMagnitude + 0.25f);
            }
            var dl = new Vector3(0f, DeadEndLampY, (float)AlleyLayout.DeadEndZ + DeadEndLampOff);
            e += DeadEndLampIntensity / ((dl - p).sqrMagnitude + 0.25f);
            return (byte)Mathf.Clamp(Mathf.RoundToInt(255f * e / SteamLampScale), 0, 255);
        }

        /// <summary>One NocSteam plume quad (facing -z as authored; the shader turns it to the camera) with its seed in R and the
        /// baked lamp irradiance in G (SteamLampByte; 0 for the bar's pots, which glow by _Glow).</summary>
        public static void Plume(MeshBuilder mb, Vector3 baseCentre, float w, float h, byte seed, byte lamp = 0)
        {
            var saved = mb.Color;
            mb.Color = new Color32(seed, lamp, 0, 255);
            Vector3 a = baseCentre + new Vector3(-0.5f * w, 0f, 0f), b = baseCentre + new Vector3(0.5f * w, 0f, 0f);
            G1Mesh.Quad(mb, a, a + new Vector3(0f, h, 0f), b + new Vector3(0f, h, 0f), b, Vector3.back);
            mb.Color = saved;
        }

        // ---------------------------------------------------------------------------------------------------------- the mouth
        /// <summary>The mouth haze card (NocAlleyGlow): z 127, x -7..7, y 0..30, facing the alley; the lit boulevard air at street
        /// level, mouth colour x 1.2 (dissolves within 60 m of the walker, gone by 14 m; r01: x 0.3 lifted the slit 45.7 -> 47.9 only).</summary>
        public const float GlowZ = 127f, GlowHalfW = 7f, GlowH = 30f, GlowRadiance = 1.2f;

        static void MouthGlow(G1Parts parts)
        {
            var mb = parts.Get("mouthGlow", G1Mat.AlleyGlow, new Vector3(0f, 10f, GlowZ));
            var saved = mb.Color;
            Color c = MouthColour() * (GlowRadiance / 4f);
            mb.Color = new Color(c.r, c.g, c.b, 1f);
            G1Mesh.Quad(mb, new Vector3(-GlowHalfW, 0f, GlowZ), new Vector3(-GlowHalfW, GlowH, GlowZ), new Vector3(GlowHalfW, GlowH, GlowZ), new Vector3(GlowHalfW, 0f, GlowZ), Vector3.back);
            mb.Color = saved;
        }

        // ---------------------------------------------------------------------------------------------------------- light pools
        /// <summary>A pool panel on the wall of `side` (-1 west, +1 east) over z0..z1, y0..y1, 1 cm off the wall, for a lamp whose foot
        /// on that wall is (fz, fy) at distance d (m) from it, linear colour c x gain (the shader multiplies the wall by 1 + 4 c E).</summary>
        public static void PoolPanel(MeshBuilder mb, int side, float z0, float z1, float y0, float y1, float fz, float fy, float d, Color c)
        {
            float xa = side * ((float)AlleyLayout.HalfWidth(z0) - 0.01f), xb = side * ((float)AlleyLayout.HalfWidth(z1) - 0.01f);
            var saved = mb.Color; bool w1 = mb.WriteUV1; Vector4 c1 = mb.CurrentUV1;
            mb.Color = new Color(Mathf.Clamp01(c.r / 4f), Mathf.Clamp01(c.g / 4f), Mathf.Clamp01(c.b / 4f), Mathf.Clamp01(d / 4f));
            mb.WriteUV1 = true; mb.CurrentUV1 = new Vector4(z1 - z0, y1 - y0, fz - z0, fy - y0);
            Vector3 A = new Vector3(xa, y0, z0), B = new Vector3(xa, y1, z0), C = new Vector3(xb, y1, z1), D = new Vector3(xb, y0, z1);
            Vector2 uA = new Vector2(0f, 0f), uB = new Vector2(0f, y1 - y0), uC = new Vector2(z1 - z0, y1 - y0), uD = new Vector2(z1 - z0, 0f);
            Vector3 want = new Vector3(-side, 0f, 0f);
            if (Vector3.Dot(Vector3.Cross(B - A, C - A), want) < 0f) mb.FlatQuad(A, D, C, B, uA, uD, uC, uB);
            else mb.FlatQuad(A, B, C, D, uA, uB, uC, uD);
            mb.Color = saved; mb.WriteUV1 = w1; mb.CurrentUV1 = c1;
        }

        /// <summary>Fix wave (C37, KS01 mean 37.2 at pool 1.0 / 35.3 at 0.6 in AL_f02 / AL_f02p6): every baked pool x 0.7 (KS00
        /// keeps its check-7 pool shares at 16:9 at 0.6); -al-pool G is a look-dev knob on top (default 1).</summary>
        public const float PoolGain = 0.7f;
        public static float PoolLookDev => PoolGain * (float)CliArgs.GetDouble("-al-pool", 1.0);
        /// <summary>Look-dev knob (fix wave, KS01's right-edge bulkhead): -al-bulk K scales the bulkheads' realtime intensity (default 1).</summary>
        public static float BulkheadI => BulkheadIntensity * (float)CliArgs.GetDouble("-al-bulk", 1.0);

        static Color PoolColour(Color light, float gain)
        {
            gain *= PoolLookDev;
            Color c = Color.Lerp(light, Color.white, 0.6f) * gain;
            c.a = 1f;
            return c;
        }

        /// <summary>INT-21: every lamp's pool on its own wall (broad, d 0.9 m) and on the facing wall (d = the alley width), the
        /// dead-end lamp's on both walls near the start, the bar light's on the east wall, the vending lights' on the west wall.</summary>
        static void Pools(G1Parts parts)
        {
            var mb = parts.Get("lightPools", G1Mat.AlleyPool, new Vector3(0f, 3f, 60f));
            Color lamp = G1Materials.Lin(NocPal.LampWarm, 1f);
            for (int k = 0; k < BulkheadCount; k++)
            {
                float z = BulkheadZ(k);
                int side = BulkheadSide(k);
                float w = 2f * (float)AlleyLayout.HalfWidth(z) - BulkheadOff;
                // fix wave (C37): own-wall gain 0.55 -> 0.30: the realtime light (0.25 m off this wall) already makes this pool, and
                // the decal multiplies what is drawn, the realtime hot spot included (KS01 21:9's east lamp: a 640 x 540 px area at 76)
                PoolPanel(mb, side, z - 2.6f, z + 2.6f, 0.02f, 5.4f, z, BulkheadY, 0.9f, PoolColour(lamp, 0.30f));
                PoolPanel(mb, -side, z - 4.5f, z + 4.5f, 0.02f, 7.0f, z, BulkheadY, w, PoolColour(lamp, 0.6f));
            }
            // the dead-end lamp: both walls from the dead end to z 9
            float dz = (float)AlleyLayout.DeadEndZ + DeadEndLampOff;
            foreach (int side in new[] { -1, 1 })
                PoolPanel(mb, side, (float)AlleyLayout.DeadEndZ + 0.02f, 9.5f, 0.02f, 7.5f, dz, DeadEndLampY, (float)AlleyLayout.HalfWidth(0.0), PoolColour(lamp, 0.75f));
            // the bar light (x -2.0, y 2.2, z 45.5) on the east wall x +3.0
            PoolPanel(mb, 1, 40f, 51f, 0.02f, 6.5f, (float)AlleyLayout.BarLightZ, (float)AlleyLayout.BarLightY, 3.0f - (float)AlleyLayout.BarLightX,
                PoolColour(G1Materials.Lin(NocPal.WinWarm, 1f), 0.4f));
            // the vending lights (x ~1.7, y 1.3, z 62.4-66.2) on the west wall
            PoolPanel(mb, -1, 59.5f, 69.5f, 0.02f, 5.0f, 64.3f, 1.3f, 4.0f, PoolColour(VendingMachines.LightColour(1f), 0.35f));
        }

        // ---------------------------------------------------------------------------------------------------------- work lamps
        /// <summary>D-039 (6): the catwalk work floodlights. Lamp k (0 west, 1 east) hangs under the south stringer 0.4 m inside its
        /// wall at y 8.85 and aims at the opposite wall at (y 3.0, z 89.6), the KS02 texture regions (the walls at y 3.3-4.8,
        /// z 88.7-90.6 beside the eye). Irradiance I N.L cone / (d^2 + 0.25): I 130 gives x 1.48 at the regions' centre
        /// (E = 0.45 / 79 = 0.0057; colour ~0.95 x PoolGain 0.7: 1 + 130 x 0.0057 x 0.66 = 1.49, the test logs x1.48; AL_fin: pools
        /// 0.89 / 0.41, KS02 mean 30.6, catwalk band 24.3; AL_f02 with I 100 and pool gain 1: pools 0.96 / 0.51, at 21:9
        /// the east region 0.36; AL_f01 with I 350 measured the regions 22.5 /
        /// 19.3 -> 40.9 / 35.9 codes, a flat grey wash, KS02 mean 39.2), x 2-2.5 at the hot spot (y 5.5-6.5, z 92), nothing outside
        /// the 28 deg cone (the walls by the catwalk and the slit stay dark).</summary>
        public const float WorkLampIntensity = 130f, WorkLampOuterDeg = 28f, WorkLampInnerDeg = 12f;
        public const float WorkLampY = 8.85f, WorkLampZ = 95.86f, WorkLampInset = 0.4f, WorkAimY = 3.0f, WorkAimZ = 89.6f;

        /// <summary>Lens centre and unit aim of work lamp k (0 west, 1 east).</summary>
        public static void WorkLamp(int k, out Vector3 pos, out Vector3 aim)
        {
            float side = k == 0 ? -1f : 1f;
            float hw = (float)AlleyLayout.HalfWidth(WorkLampZ);
            Vector3 c = new Vector3(side * (hw - WorkLampInset), WorkLampY, WorkLampZ);
            Vector3 target = new Vector3(-side * (float)AlleyLayout.HalfWidth(WorkAimZ), WorkAimY, WorkAimZ);
            aim = (target - c).normalized;
            pos = c + aim * 0.075f;
        }

        /// <summary>The wash colour: LampWarm toward white 0.6 like the pools (D-032 (4): beat 1 dark cast |R - G| <= 6).</summary>
        public static Color WorkLampPoolColour() => PoolColour(G1Materials.Lin(NocPal.LampWarm, 1f), 1f);

        static void WorkLamps(G1Parts parts)
        {
            var origin = new Vector3(0f, WorkLampY, WorkLampZ);
            var steel = parts.Get("workLampFixture", G1Mat.SteelPainted, origin);
            var lens = parts.Get("workLampLens", G1Mat.EmitLamp, origin);
            float yStr = (float)AlleyLayout.CatwalkY1 - StringerH;                    // the stringer's underside (y 9.05)
            for (int k = 0; k < 2; k++)
            {
                WorkLamp(k, out Vector3 pos, out Vector3 aim);
                Vector3 c = pos - aim * 0.075f;
                // hanger: a 3 cm square stem from the stringer to the yoke, a cross plate under the stringer
                G1Mesh.Chamfer(steel, new Vector3(c.x - 0.015f, c.y + 0.10f, (float)CatwalkZ0 + 0.01f), new Vector3(c.x + 0.015f, yStr, (float)CatwalkZ0 + 0.04f), 0.004f);
                G1Mesh.Chamfer(steel, new Vector3(c.x - 0.08f, yStr - 0.012f, (float)CatwalkZ0 - 0.005f), new Vector3(c.x + 0.08f, yStr, (float)CatwalkZ0 + 0.055f), 0.003f);
                var rot = Quaternion.LookRotation(aim, Vector3.up);
                steel.Push(c, rot);
                lens.Push(c, rot);
                G1Mesh.Chamfer(steel, new Vector3(-0.13f, -0.10f, -0.08f), new Vector3(0.13f, 0.10f, 0.06f), 0.012f);   // housing
                G1Mesh.Chamfer(steel, new Vector3(-0.14f, -0.11f, 0.05f), new Vector3(0.14f, 0.11f, 0.068f), 0.006f);  // bezel
                G1Mesh.Chamfer(lens, new Vector3(-0.105f, -0.075f, 0.06f), new Vector3(0.105f, 0.075f, 0.075f), 0.008f); // lens
                foreach (float dy in new[] { -0.04f, 0f, 0.04f })                                                       // guard bars
                    G1Mesh.Chamfer(steel, new Vector3(-0.12f, dy - 0.005f, 0.078f), new Vector3(0.12f, dy + 0.005f, 0.088f), 0.002f);
                foreach (float dx in new[] { -0.16f, 0.15f })                                                           // yoke
                    G1Mesh.Chamfer(steel, new Vector3(dx, -0.02f, -0.03f), new Vector3(dx + 0.012f, 0.16f, 0.01f), 0.003f);
                for (int f = 0; f < 4; f++)                                                                             // fins
                    G1Mesh.Chamfer(steel, new Vector3(-0.11f + 0.07f * f, -0.085f, -0.10f), new Vector3(-0.10f + 0.07f * f, 0.085f, -0.08f), 0.002f);
                steel.Pop();
                lens.Pop();
                // no halo: the bulkhead family's halo (sigma of r 7) washed over the catwalk (AL_f03: KS02 catwalk band 24.7 -> 43.0 > 40)
            }
            // the wash: one panel per wall over z 84-97, y 0.02-10.5 (the shader computes both lamps per pixel)
            var mb = parts.Get("workLampPools", G1Mat.AlleyWorkPool, new Vector3(0f, 5f, 90f));
            foreach (int side in new[] { -1, 1 })
                PoolPanel(mb, side, 84f, 97f, 0.02f, 10.5f, 90f, 5f, 1f, Color.white);
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
