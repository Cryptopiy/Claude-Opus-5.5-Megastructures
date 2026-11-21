// SpillwayBuilder: the concrete and steel of beat 3 (MASTER_PROMPT 14.5.3 "Geometry"). Part of Beat03SpillwayBridge.
//   Deck 12 m wide (x -6..6, top y 0): solid wet-concrete strips over the 15 piers; over each of the 14 slots two edge
//   girders (the solid edges: lee x -6..-5.6, sea x 5.3..6, 1.6 m deep, bearing on the piers), seven transverse steel floor
//   beams (I 0.40 x 0.20, every 2 m) and one see-through BwGrating panel x -5.6..5.3 (bars along z, 40 mm at 80 mm,
//   analytic, alpha-to-coverage; no shadow casting: 40 mm bars would alias in a 150 m cascade). Every pier / girder joint
//   keeps its 2 cm chamfer, which reads as the expansion joint.
//   Piers k 0-14 (4 m): under the deck from the sill (y -14) to the deck underside; east of the sea face to x 44 with a
//   cap at y +0.5 and a half-round cutwater nose (radius 2 m) to x 46.
//   Gate houses 5 x 4 x 5 m on every pier at x 9-14 (cap y +0.5 .. +5.5), roof slab, a steel door 1.0 x 2.1 m in the west
//   face (the human scale next to the 16 m bays), a dark window in each slot face.
//   Radial gates (radius 14, 16 m wide, raised 5 m: lip y -9.0): curved skin plate, three horizontal girders, two arm pairs
//   to trunnion hubs on the pier walls (-2.0, -1.8).
//   Rails (14.5.3 "round rails 0.10 on the sea side keep the east view open"): two round rails (tops 1.1 and 0.55) on 0.12
//   posts every 2 m on both deck edges, the lee one continuing the crest rail of 14.5.2 at x -6.0.
// Nothing stands inside the capsule reach of the road lane (x -5.75 .. 5.45) above the walking surface y 0.
using System;
using UnityEngine;

namespace Megastructure.Beats
{
    public sealed partial class Beat03SpillwayBridge
    {
        // ================================================================ deck
        void BuildDeck(WorldContext ctx, Transform root)
        {
            var solid = new MeshBuilder();
            var beams = new MeshBuilder();
            var grate = new MeshBuilder();
            double y0 = SpillwayGeometry.DeckUnder, y1 = SpillwayGeometry.DeckTop;
            for (int k = 0; k < BulwarkConst.PierCount; k++)
                CBox(solid, -6.0, y0, SpillwayGeometry.PierZ0(k), 6.0, y1, SpillwayGeometry.PierZ1(k));
            float bh = (float)SpillwayGeometry.FloorBeamDepth, bf = (float)SpillwayGeometry.FloorBeamFlange;
            for (int k = 0; k < BulwarkConst.SlotCount; k++)
            {
                double z0 = SpillwayGeometry.SlotZ0(k), z1 = SpillwayGeometry.SlotZ1(k);
                // edge girders (solid edges of the span), full span between the pier decks
                CBox(solid, SpillwayGeometry.GirderLeeX0, -SpillwayGeometry.GirderDepth, z0, SpillwayGeometry.GirderLeeX1, y1, z1);
                CBox(solid, SpillwayGeometry.GirderSeaX0, -SpillwayGeometry.GirderDepth, z0, SpillwayGeometry.GirderSeaX1, y1, z1);
                // transverse floor beams every 2 m; the panel bears on them 3 mm below the grating plane
                for (double zb = z0 + SpillwayGeometry.FloorBeamPitch; zb < z1 - 0.5; zb += SpillwayGeometry.FloorBeamPitch)
                {
                    var a = new Vector3((float)SpillwayGeometry.GirderLeeX1, -0.033f - 0.5f * bh, (float)zb);
                    var b = new Vector3((float)SpillwayGeometry.GirderSeaX0, -0.033f - 0.5f * bh, (float)zb);
                    MeshKit.IBeam(beams, a, b, bh, bf, 0.012f, 0.016f, Vector3.up);
                }
                // longitudinal panel frame (the grating's edge bars) along both girders, 50 mm under the panel plane
                CBox(beams, SpillwayGeometry.GirderLeeX1, -0.083, z0 + 0.02, SpillwayGeometry.GirderLeeX1 + 0.06, -0.033, z1 - 0.02, 0.005f);
                CBox(beams, SpillwayGeometry.GirderSeaX0 - 0.06, -0.083, z0 + 0.02, SpillwayGeometry.GirderSeaX0, -0.033, z1 - 0.02, 0.005f);
                // one see-through grating panel over the span (top plane y 0; the shader models the 30 mm bar depth)
                float gx0 = (float)SpillwayGeometry.GratingX0, gx1 = (float)SpillwayGeometry.GratingX1;
                grate.FlatQuad(new Vector3(gx0, 0f, (float)z0), new Vector3(gx0, 0f, (float)z1), new Vector3(gx1, 0f, (float)z1), new Vector3(gx1, 0f, (float)z0));
            }
            // the walked deck follows the rain wetness like the crest road (BwConcrete _WetFollow: wet in the storm, drying in
            // the sun break, 14.2.6 "road darkened outside the sun-break window"); ConcreteWet (wet everywhere) read as a flat
            // featureless dark plane at 1.7 m in the gold light (wb-mist-05 pose_gap582, pose_down90)
            Flush(ctx, root, ref solid, "b03_deck", m_conc != null ? m_conc : m_wet);
            Flush(ctx, root, ref beams, "b03_deck_steel", m_steel);
            Flush(ctx, root, ref grate, "b03_grating", m_grating, false);
        }

        // ================================================================ piers
        void BuildPiers(WorldContext ctx, Transform root)
        {
            // under the deck the piers stay wet (spray, the racing water); east of the sea face their sunlit upper walls and
            // caps are dry concrete above a wet band 1.5 m over the sea (the low sun of the break lights them: KS07)
            var wet = new MeshBuilder();
            var dry = new MeshBuilder();
            double sea = BulwarkConst.SeaFaceX, top = BulwarkConst.PierTopY, sill = SpillwayGeometry.SillY;
            double noseC = SpillwayGeometry.MouthX - SpillwayGeometry.NoseRadius, wetTop = BulwarkConst.SeaY + 1.5;
            for (int k = 0; k < BulwarkConst.PierCount; k++)
            {
                double z0 = SpillwayGeometry.PierZ0(k), z1 = SpillwayGeometry.PierZ1(k);
                var nc = new Vector3((float)noseC, 0f, (float)(0.5 * (z0 + z1)));
                CBox(wet, SpillwayGeometry.LipX, sill, z0, sea, SpillwayGeometry.DeckUnder, z1);   // under the deck
                CBox(wet, sea, sill, z0, noseC, wetTop, z1, 0.03f);                                // east: wet below
                Nose(wet, nc, (float)SpillwayGeometry.NoseRadius, (float)sill, (float)wetTop, false);
                CBox(dry, sea, wetTop, z0, noseC, top, z1, 0.03f);                                 // dry above
                Nose(dry, nc, (float)SpillwayGeometry.NoseRadius, (float)wetTop, (float)top, true);
            }
            Flush(ctx, root, ref wet, "b03_piers_wet", m_wet);
            Flush(ctx, root, ref dry, "b03_piers", m_conc);
        }

        /// <summary>Half-round cutwater facing +x: a vertical half cylinder (16 facets over 180 deg) with its top cap.</summary>
        static void Nose(MeshBuilder mb, Vector3 c, float r, float y0, float y1, bool cap = true)
        {
            const int n = 16;
            int first = mb.VertexCount;
            for (int i = 0; i <= n; i++)
            {
                float a = -0.5f * Mathf.PI + Mathf.PI * i / n;          // from -z through +x to +z
                var dir = new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a));
                mb.Vertex(new Vector3(c.x + r * dir.x, y0, c.z + r * dir.z), dir);
                mb.Vertex(new Vector3(c.x + r * dir.x, y1, c.z + r * dir.z), dir);
            }
            for (int i = 0; i < n; i++)
            {
                int a0 = first + 2 * i, a1 = a0 + 1, b0 = a0 + 2, b1 = a0 + 3;
                mb.Quad(a0, a1, b1, b0);
            }
            if (!cap) return;
            // top cap (fan), facing up
            int ctr = mb.Vertex(new Vector3(c.x, y1, c.z), Vector3.up);
            int ring = mb.VertexCount;
            for (int i = 0; i <= n; i++)
            {
                float a = -0.5f * Mathf.PI + Mathf.PI * i / n;
                mb.Vertex(new Vector3(c.x + r * Mathf.Cos(a), y1, c.z + r * Mathf.Sin(a)), Vector3.up);
            }
            for (int i = 0; i < n; i++) mb.Tri(ctr, ring + i + 1, ring + i);
        }

        // ================================================================ gate houses
        void BuildGateHouses(WorldContext ctx, Transform root)
        {
            var conc = new MeshBuilder();
            var steel = new MeshBuilder();
            var dark = new MeshBuilder();
            double gx0 = BulwarkConst.GateHouseX0, gx1 = BulwarkConst.GateHouseX1, gy0 = BulwarkConst.PierTopY, gy1 = gy0 + BulwarkConst.GateHouseH;
            for (int k = 0; k < BulwarkConst.PierCount; k++)
            {
                double z0 = SpillwayGeometry.PierZ0(k), z1 = SpillwayGeometry.PierZ1(k), zm = 0.5 * (z0 + z1);
                // 12 cm chamfers on the body (mistx-03 back640: the 3 cm sunlit corner chamfers of the far houses were 0.1 px wide
                // and broke into isolated gold dots, 5 NaN-like pixels; at 0.12 m they are a steady 0.6 px line at 120 m)
                CBox(conc, gx0, gy0 - 0.12, z0 + 0.02, gx1, gy1, z1 - 0.02, 0.12f);   // the base chamfer sinks into the pier cap
                CBox(conc, gx0 - 0.15, gy1, z0 - 0.13, gx1 + 0.15, gy1 + 0.3, z1 + 0.13, 0.06f);   // roof slab
                // west door 1.0 x 2.1 (steel leaf proud by 30 mm, frame 60 mm)
                CBox(steel, gx0 - 0.03, gy0, zm - 0.5, gx0, gy0 + 2.1, zm + 0.5, 0.008f);
                CBox(steel, gx0 - 0.06, gy0, zm - 0.56, gx0, gy0 + 2.16, zm - 0.5, 0.008f);
                CBox(steel, gx0 - 0.06, gy0, zm + 0.5, gx0, gy0 + 2.16, zm + 0.56, 0.008f);
                CBox(steel, gx0 - 0.06, gy0 + 2.1, zm - 0.56, gx0, gy0 + 2.16, zm + 0.56, 0.008f);
                // a dark window toward each slot (1.2 x 0.6 at y +3.2 above the cap), 20 mm proud
                CBox(dark, 0.5 * (gx0 + gx1) - 0.6, gy0 + 2.9, z0 - 0.0, 0.5 * (gx0 + gx1) + 0.6, gy0 + 3.5, z0 + 0.04, 0.005f);
                CBox(dark, 0.5 * (gx0 + gx1) - 0.6, gy0 + 2.9, z1 - 0.04, 0.5 * (gx0 + gx1) + 0.6, gy0 + 3.5, z1 + 0.0, 0.005f);
            }
            Flush(ctx, root, ref conc, "b03_gate_houses", m_conc);
            Flush(ctx, root, ref steel, "b03_gate_house_doors", m_steel);
            Flush(ctx, root, ref dark, "b03_gate_house_windows", m_dark);
        }

        // ================================================================ radial gates
        void BuildGates(WorldContext ctx, Transform root)
        {
            var mb = new MeshBuilder();
            var ochre = new MeshBuilder();
            float R = (float)SpillwayGeometry.GateRadius;
            var T = new Vector2((float)SpillwayGeometry.TrunnionX, (float)SpillwayGeometry.TrunnionY);
            float phiB = Mathf.Asin(((float)SpillwayGeometry.GateLipY - T.y) / R);
            float phiT = phiB + (float)(SpillwayGeometry.GateSpanDeg * BwMath.Deg);
            for (int k = 0; k < BulwarkConst.SlotCount; k++)
            {
                float z0 = (float)SpillwayGeometry.SlotZ0(k) + 0.05f, z1 = (float)SpillwayGeometry.SlotZ1(k) - 0.05f;
                SkinPlate(mb, T, R, phiB, phiT, z0, z1);
                // three horizontal girders on the downstream (concave) side
                for (int g = 0; g < 3; g++)
                {
                    float ph = Mathf.Lerp(phiB, phiT, 0.18f + 0.32f * g);
                    var p = T + new Vector2(Mathf.Cos(ph), Mathf.Sin(ph)) * (R - 0.35f);
                    ChamferAlong(mb, new Vector3(p.x, p.y, z0 + 0.1f), new Vector3(p.x, p.y, z1 - 0.1f), 0.5f, 0.35f, ph);
                }
                // two arm pairs from the trunnion hubs to the plate, and the hubs on the pier walls
                for (int side = 0; side < 2; side++)
                {
                    float za = side == 0 ? z0 + 0.45f : z1 - 0.45f;
                    var hub = new Vector3(T.x, T.y, za);
                    for (int a = 0; a < 2; a++)
                    {
                        float ph = Mathf.Lerp(phiB, phiT, a == 0 ? 0.2f : 0.8f);
                        var end = new Vector3(T.x + (R - 0.6f) * Mathf.Cos(ph), T.y + (R - 0.6f) * Mathf.Sin(ph), za);
                        MeshKit.ChamferMember(mb, hub, end, 0.45f, 0.55f, 0.02f, Vector3.forward);
                    }
                    float zw = side == 0 ? z0 - 0.05f : z1 + 0.05f;   // hub against the pier wall
                    MeshKit.Cylinder(ochre, new Vector3(T.x, T.y, za), new Vector3(T.x, T.y, zw), 0.42f, 16, true);
                }
            }
            Flush(ctx, root, ref mb, "b03_radial_gates", m_steel);
            Flush(ctx, root, ref ochre, "b03_gate_trunnions", m_ochre);
        }

        /// <summary>Curved skin plate (two faces 30 mm apart, rim caps) on radius R about the z-axis line through T.</summary>
        static void SkinPlate(MeshBuilder mb, Vector2 T, float R, float phi0, float phi1, float z0, float z1)
        {
            const int n = 14;
            for (int face = 0; face < 2; face++)
            {
                float r = face == 0 ? R : R - 0.03f;
                int first = mb.VertexCount;
                for (int i = 0; i <= n; i++)
                {
                    float ph = Mathf.Lerp(phi0, phi1, i / (float)n);
                    var dir = new Vector3(Mathf.Cos(ph), Mathf.Sin(ph), 0f);
                    var nrm = face == 0 ? dir : -dir;
                    mb.Vertex(new Vector3(T.x + r * dir.x, T.y + r * dir.y, z0), nrm);
                    mb.Vertex(new Vector3(T.x + r * dir.x, T.y + r * dir.y, z1), nrm);
                }
                for (int i = 0; i < n; i++)
                {
                    int a0 = first + 2 * i, a1 = a0 + 1, b0 = a0 + 2, b1 = a0 + 3;
                    if (face == 0) mb.Quad(a0, b0, b1, a1); else mb.Quad(a0, a1, b1, b0);
                }
            }
            // bottom and top rims (thin flats)
            foreach (float ph in new[] { phi0, phi1 })
            {
                var dir = new Vector3(Mathf.Cos(ph), Mathf.Sin(ph), 0f);
                var tan = new Vector3(-dir.y, dir.x, 0f) * (ph == phi0 ? -1f : 1f);
                var o = new Vector3(T.x + R * dir.x, T.y + R * dir.y, 0f);
                var ii = new Vector3(T.x + (R - 0.03f) * dir.x, T.y + (R - 0.03f) * dir.y, 0f);
                Vector3 A = new Vector3(o.x, o.y, z0), B = new Vector3(o.x, o.y, z1), C = new Vector3(ii.x, ii.y, z1), D = new Vector3(ii.x, ii.y, z0);
                Vector3 nn = Vector3.Cross(B - A, C - A);
                if (Vector3.Dot(nn, tan) < 0f) mb.FlatQuad(A, D, C, B); else mb.FlatQuad(A, B, C, D);
            }
        }

        /// <summary>Chamfered box member along z through a and b, rotated by the plate angle ph about z.</summary>
        static void ChamferAlong(MeshBuilder mb, Vector3 a, Vector3 b, float depth, float height, float ph)
        {
            var c = 0.5f * (a + b);
            var rot = Quaternion.Euler(0f, 0f, ph * Mathf.Rad2Deg);
            MeshKit.ChamferBox(mb, c, new Vector3(depth, height, (b - a).magnitude), 0.02f, rot);
        }

        // ================================================================ rails
        /// <summary>Sea-side rail axis: 0.10 m inside the sea face so its 0.12 posts clear the 0.5 m step up to the pier caps
        /// (x 6.0); lane capsule reach ends at x 5.45.</summary>
        public const double SeaRailX = 5.9;

        void BuildRails(WorldContext ctx, Transform root)
        {
            var rails = new MeshBuilder();
            var posts = new MeshBuilder();
            var caps = new MeshBuilder();
            float rr = LineRadius(m_rail, 0.5f * (float)BulwarkConst.LeeRailDiameter);
            bool round = m_post != null;
            RailRun(rails, round ? posts : caps, caps, BulwarkConst.LeeRailX, SpillwayGeometry.Z0, SpillwayGeometry.Z1, rr, round);
            RailRun(rails, round ? posts : caps, caps, SeaRailX, SpillwayGeometry.Z0, SpillwayGeometry.Z1, rr, round);
            Flush(ctx, root, ref rails, "b03_rails", m_rail, false);
            Flush(ctx, root, ref posts, "b03_rail_posts", m_post, false);
            Flush(ctx, root, ref caps, "b03_rail_caps", m_steel);
        }

        /// <summary>Two round rails (tops 1.1 and 0.55) along z at x on 0.12 m posts every 2 m at odd z (the crest pattern of
        /// 14.5.2: posts at z = 2 j + 1, so the run continues the beat 2 and beat 4 rails without a gap or a doubled post).</summary>
        static void RailRun(MeshBuilder rails, MeshBuilder posts, MeshBuilder caps, double x, double z0, double z1, float radius, bool round)
        {
            const double chunk = 96.0;
            for (double a = z0; a < z1 - 1e-6; a += chunk)
            {
                double b = Math.Min(z1, a + chunk);
                Tube(rails, new Vector3((float)x, (float)(BulwarkConst.LeeRailTop - radius), (float)a), new Vector3((float)x, (float)(BulwarkConst.LeeRailTop - radius), (float)b), radius);
                Tube(rails, new Vector3((float)x, (float)(BulwarkConst.LeeRailMid - radius), (float)a), new Vector3((float)x, (float)(BulwarkConst.LeeRailMid - radius), (float)b), radius);
            }
            double ps = 0.5 * BulwarkConst.LeeRailPostSize;
            for (double z = Math.Ceiling(z0 / BulwarkConst.LeeRailPostPitch) * BulwarkConst.LeeRailPostPitch + 1.0; z < z1; z += BulwarkConst.LeeRailPostPitch)
            {
                if (!round) { CBox(posts, x - ps, 0.0, z - ps, x + ps, BulwarkConst.LeeRailTop - 2.0 * radius, z + ps, 0.01f); continue; }
                // round post (BwLine tube, the crest's 14.5.2 pattern) from a base plate to 2 cm above the top rail, steel cap
                float zf = (float)z, xf = (float)x, pr = (float)ps;
                float yCap = (float)BulwarkConst.LeeRailTop + 0.02f;
                Tube(posts, new Vector3(xf, 0.015f, zf), new Vector3(xf, yCap, zf), pr, 10);
                MeshKit.Disc(caps, new Vector3(xf, yCap, zf), Vector3.up, pr + 0.004f, 12);
                MeshKit.ChamferBox(caps, new Vector3(xf, 0.0075f, zf), new Vector3(0.16f, 0.015f, 0.2f), 0.004f);
            }
        }
    }
}
