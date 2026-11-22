// SkybridgeGate: gate blocks A (x 372-384, at the tongue tip) and B (x 683-695, at the garden) of 14.5.4: z 175-215, 10 m high
// (the block runs down to y -14 as the truss abutment), a 12 m x 8 m walkway opening (z 189-201), the two tube mouths (z 183 and
// 207, r 4.5, axis y 2.0) on the bridge-side face with blind 6 m tunnels behind them, and the gate frame (pylons, lintel, 16 m)
// on the west side. Gate A's passage keeps the 5 m deck with 0.5 m side trenches (the lane is 2.15 there); gate B's passage
// floor is the widened deck (SkybridgeDeck). Faces with openings come from SkybridgeFace (no T-junctions); side faces are split
// at the same heights as the faces they meet. Concrete blocks and (TR fix wave, C31) cast-concrete frames (the library's CC0
// ConcreteDark, the blocks' material), gate B with its light content (GateLights).
using System.Collections.Generic;
using UnityEngine;

namespace Megastructure
{
    public static class SkybridgeGate
    {
        /// <summary>Mouth square half size: the r 4.5 mouth sits in a 9.5 m square cell (25 cm of wall around it).</summary>
        public const float MouthSquare = 4.75f;

        // ---- TR fix wave (C31): gate B's light content (D-032 (5): KS14 meets the highlight rule with light, never brighter windows)
        /// <summary>The recessed LampWarm luminaire under the lintel: lens x, z extent (y just under the lintel bottom).</summary>
        public const float SoffitX0 = 683.6f, SoffitX1 = 684.6f, SoffitZ0 = 189.6f, SoffitZ1 = 200.4f;
        /// <summary>The two wall lanterns on the bridge faces of the pylons (flanking the walkway opening): centre height, lens size.</summary>
        public const float LanternY = 5.2f, LanternLensW = 0.36f, LanternLensH = 0.70f;
        /// <summary>Two luminaire lines in the passage ceiling (y 8) over the deck edges, along x.</summary>
        public const float PassageLineZ = 1.9f, PassageLineX0 = 684.0f, PassageLineX1 = 694.0f, PassageLineW = 0.24f;
        /// <summary>Shadowless point lights: under the lintel, at each lantern, in the passage.</summary>
        public const float SoffitRange = 14f, SoffitIntensity = 4.0f, LanternRange = 10f, LanternIntensity = 2.0f, PassageRange = 12f, PassageIntensity = 2.5f;
        /// <summary>Lights and wet-floor sources that GateLights adds (tests).</summary>
        public const int GateLightCount = 4, GateWetSources = 5;

        public static void Build(WorldContext ctx, Transform root, SkybridgeMaterials m, bool gateA, Material[] trainMats = null)
        {
            float x0 = gateA ? SkybridgeDims.GateAX0 : SkybridgeDims.GateBX0;
            float x1 = gateA ? SkybridgeDims.GateAX1 : SkybridgeDims.GateBX1;
            float xBridge = gateA ? x1 : x0, nBridge = gateA ? 1f : -1f;      // the face toward the span carries the tube mouths
            float xOther = gateA ? x0 : x1, nOther = -nBridge;
            float holeBottom = gateA ? SkybridgeDims.TrenchY : 0f;
            float zA = SkybridgeDims.GateZ0, zB = SkybridgeDims.GateZ1, yB = SkybridgeDims.GateBottom, yT = SkybridgeDims.GateTop;
            float o0 = SkybridgeDims.OpeningZ0, o1 = SkybridgeDims.OpeningZ1, oT = SkybridgeDims.OpeningTop;
            float r = SkybridgeDims.TubeR, h = MouthSquare, cy = SkybridgeDims.TubeAxisY;
            float sz = SkybridgeDims.TubeSZ, nz = SkybridgeDims.TubeNZ;
            float mb0 = cy - h, mt0 = cy + h;                                  // mouth square v range -2.75 .. 6.75
            int seg = SkybridgeDims.MouthSegments;
            string tag = gateA ? "gateA" : "gateB";
            var g = BridgeGardenKit.Group(root, tag);

            // ---- bridge-side face with the walkway opening and the two tube mouths
            var fb = new SkybridgeFace();
            fb.AddCell(zA, zB, yB, mb0);                                       // bottom band
            fb.AddCell(zA, sz - h, mb0, oT);
            fb.AddRing(sz, cy, h, r, seg);
            fb.AddCell(sz - h, sz + h, mt0, oT);
            fb.AddCell(sz + h, o0, mb0, oT);
            fb.AddCell(o0, o1, mb0, holeBottom);                               // under the walkway opening
            fb.AddCell(o1, nz - h, mb0, oT);
            fb.AddRing(nz, cy, h, r, seg);
            fb.AddCell(nz - h, nz + h, mt0, oT);
            fb.AddCell(nz + h, zB, mb0, oT);
            fb.AddCell(zA, zB, oT, yT);                                        // top band
            RegisterFloorPoints(fb, gateA, holeBottom);

            // ---- the other face: walkway opening only
            var fo = new SkybridgeFace();
            fo.AddCell(zA, zB, yB, holeBottom);
            fo.AddCell(zA, o0, holeBottom, oT);
            fo.AddCell(o1, zB, holeBottom, oT);
            fo.AddCell(zA, zB, oT, yT);
            RegisterFloorPoints(fo, gateA, holeBottom);

            var mbk = new MeshBuilder();
            fb.Emit(mbk, xBridge, nBridge);
            fo.Emit(mbk, xOther, nOther);

            // side faces z 175 / 215, split at the heights the two faces use on those edges
            for (int side = 0; side < 2; side++)
            {
                float z = side == 0 ? zA : zB;
                var want = side == 0 ? Vector3.back : Vector3.forward;
                SidePolygon(mbk, z, want, xBridge, fb.BreaksOnU(z), xOther, fo.BreaksOnU(z));
            }
            BridgeGardenKit.OrientedQuad(mbk, new Vector3(x0, yT, zA), new Vector3(x1, yT, zA), new Vector3(x1, yT, zB), new Vector3(x0, yT, zB), Vector3.up);
            BridgeGardenKit.OrientedQuad(mbk, new Vector3(x0, yB, zA), new Vector3(x1, yB, zA), new Vector3(x1, yB, zB), new Vector3(x0, yB, zB), Vector3.down);

            // passage: walls, ceiling; gate A also the trench floors beside the 5 m deck (5 cm under the deck edges)
            BridgeGardenKit.OrientedQuad(mbk, new Vector3(x0, holeBottom, o0), new Vector3(x1, holeBottom, o0), new Vector3(x1, oT, o0), new Vector3(x0, oT, o0), Vector3.forward);
            BridgeGardenKit.OrientedQuad(mbk, new Vector3(x0, holeBottom, o1), new Vector3(x1, holeBottom, o1), new Vector3(x1, oT, o1), new Vector3(x0, oT, o1), Vector3.back);
            BridgeGardenKit.OrientedQuad(mbk, new Vector3(x0, oT, o0), new Vector3(x1, oT, o0), new Vector3(x1, oT, o1), new Vector3(x0, oT, o1), Vector3.down);
            if (gateA)
            {
                float d0 = SkybridgeDims.AxisZ - SkybridgeDims.DeckHalfWidth + 0.05f, d1 = SkybridgeDims.AxisZ + SkybridgeDims.DeckHalfWidth - 0.05f;
                float y = SkybridgeDims.TrenchY;
                BridgeGardenKit.OrientedQuad(mbk, new Vector3(x0, y, o0), new Vector3(x1, y, o0), new Vector3(x1, y, d0), new Vector3(x0, y, d0), Vector3.up);
                BridgeGardenKit.OrientedQuad(mbk, new Vector3(x0, y, d1), new Vector3(x1, y, d1), new Vector3(x1, y, o1), new Vector3(x0, y, o1), Vector3.up);
            }
            BridgeGardenKit.Emit(ctx, g, "block", mbk, m.Concrete);

            // ---- blind tube tunnels behind the mouths (inward cylinder on the mouth samples + back disc). TR: 11.9 m deep (through the
            // block), drawn with the tube's NocTrain material (zone 7, the tunnel lining): black from 2.5 m behind the mouth, so a mouth
            // reads as a dark hole and the trains emerge from it and vanish into it (TrainSchedule); Concrete without the shader.
            float depthDir = -nBridge;
            foreach (float zc in new[] { sz, nz })
            {
                var tun = new MeshBuilder();
                int tube = zc > SkybridgeDims.AxisZ ? TrainSchedule.TubeN : TrainSchedule.TubeS;
                Material tm = trainMats != null ? trainMats[tube] : m.Tunnel;
                if (trainMats != null) tun.Color = new Color32((byte)(TrainBuilder.ZLiner * 25), 0, 77, 255);
                float xb = xBridge + depthDir * SkybridgeDims.TunnelDepth;
                int first = tun.VertexCount;
                for (int k = 0; k < seg; k++)
                {
                    Vector2 c = SkybridgeFace.CirclePoint(zc, cy, r, k, seg);
                    var inward = new Vector3(0f, cy - c.y, zc - c.x).normalized;
                    tun.Vertex(new Vector3(xBridge, c.y, c.x), inward, new Vector2(0f, k * 2f * Mathf.PI * r / seg));
                    tun.Vertex(new Vector3(xb, c.y, c.x), inward, new Vector2(SkybridgeDims.TunnelDepth, k * 2f * Mathf.PI * r / seg));
                }
                for (int k = 0; k < seg; k++)
                {
                    int a = first + 2 * k, b = first + 2 * ((k + 1) % seg);
                    MeshKit.Quad(tun, a, a + 1, b + 1, b);
                }
                MeshKit.Disc(tun, new Vector3(xb, cy, zc), new Vector3(nBridge, 0f, 0f), r + 0.05f, seg);
                BridgeGardenKit.Emit(ctx, g, zc > SkybridgeDims.AxisZ ? "tunnelN" : "tunnelS", tun, tm);
            }

            // ---- gate frame (west side): two pylons 0.3 m proud of the block face, lintel y 14-16 (inset 5 cm: no shared planes)
            float fx0 = gateA ? SkybridgeDims.FrameAX0 : SkybridgeDims.FrameBX0, fx1 = gateA ? SkybridgeDims.FrameAX1 : SkybridgeDims.FrameBX1;
            float pw = SkybridgeDims.PylonWidth, gap = SkybridgeDims.PylonGap;
            float pyB = SkybridgeDims.FrameBottom, pyT = SkybridgeDims.FrameTop;
            var fr = new MeshBuilder();
            float pS1 = o0 - gap, pS0 = pS1 - pw, pN0 = o1 + gap, pN1 = pN0 + pw;
            MeshKit.ChamferBox(fr, new Vector3(0.5f * (fx0 + fx1), 0.5f * (pyB + pyT), 0.5f * (pS0 + pS1)), new Vector3(fx1 - fx0, pyT - pyB, pw), 0.02f);
            MeshKit.ChamferBox(fr, new Vector3(0.5f * (fx0 + fx1), 0.5f * (pyB + pyT), 0.5f * (pN0 + pN1)), new Vector3(fx1 - fx0, pyT - pyB, pw), 0.02f);
            float lx0 = fx0 + 0.05f, lx1 = fx1 - 0.05f, lz0 = pS0 + 0.05f, lz1 = pN1 - 0.05f, ly0 = SkybridgeDims.LintelBottom, ly1 = pyT - 0.05f;
            MeshKit.ChamferBox(fr, new Vector3(0.5f * (lx0 + lx1), 0.5f * (ly0 + ly1), 0.5f * (lz0 + lz1)), new Vector3(lx1 - lx0, ly1 - ly0, lz1 - lz0), 0.02f);
            BridgeGardenKit.Emit(ctx, g, "frame", fr, m.Concrete != null ? m.Concrete : m.Steel);
            if (!gateA) GateLights(ctx, g, m, pS0, pS1, pN0, pN1, fx0);
        }

        /// <summary>
        /// Gate B's light content (C31): the lintel luminaire (a 1.0 x 10.8 m LampWarm lens in a dark housing, 4 cm under the lintel
        /// bottom between the pylons), a wall lantern on the bridge face of each pylon (lens 0.36 x 0.70 m at y 5.2, dark housing),
        /// two 10 m luminaire lines in the passage ceiling, four shadowless point lights (14.2.3) and five wet-floor sources (the
        /// lintel line, the lanterns, the passage lines), so the garden gate reads as a lit entrance and streaks in the wet deck.
        /// Nothing lower than y 5 (the lane envelope) and nothing inside the walkway opening below the lintel but the lens lines.
        /// </summary>
        static void GateLights(WorldContext ctx, Transform g, SkybridgeMaterials m, float pS0, float pS1, float pN0, float pN1, float fx0)
        {
            var lg = BridgeGardenKit.Group(g, "lights");
            Color warm = BridgeGardenKit.Lin(NocPal.LampWarm);
            Color rad = warm * NocturnePalette.EmissionSign;
            var housing = new MeshBuilder();
            var lens = new MeshBuilder();
            float ly = SkybridgeDims.LintelBottom;
            float sx = 0.5f * (SoffitX0 + SoffitX1), sz = 0.5f * (SoffitZ0 + SoffitZ1), sl = SoffitZ1 - SoffitZ0;
            MeshKit.ChamferBox(housing, new Vector3(sx, ly - 0.03f, sz), new Vector3(SoffitX1 - SoffitX0 + 0.16f, 0.08f, sl + 0.16f), 0.01f);   // 1 cm into the lintel
            MeshKit.ChamferBox(lens, new Vector3(sx, ly - 0.074f, sz), new Vector3(SoffitX1 - SoffitX0, 0.012f, sl), 0.003f);       // 1 cm proud of the housing
            BridgeGardenKit.PointLight(lg, "soffit", new Vector3(sx, ly - 1.1f, sz), warm, SoffitRange, SoffitIntensity);
            NocWetRegistry.AddSource(new Vector3(sx, ly - 0.08f, SoffitZ0 + 0.3f), new Vector3(sx, ly - 0.08f, SoffitZ1 - 0.3f), 0.3f, rad, SkybridgeDims.SOfX(sx));
            foreach (float zc in new[] { 0.5f * (pS0 + pS1), 0.5f * (pN0 + pN1) })
            {
                float hx = fx0 - 0.19f;                                                           // 1 cm into the pylon
                MeshKit.ChamferBox(housing, new Vector3(hx, LanternY, zc), new Vector3(0.4f, 0.94f, 0.50f), 0.02f);
                var lc = new Vector3(fx0 - 0.394f, LanternY, zc);                                  // 1 cm proud of the housing
                MeshKit.ChamferBox(lens, lc, new Vector3(0.012f, LanternLensH, LanternLensW), 0.003f);
                BridgeGardenKit.PointLight(lg, zc < SkybridgeDims.AxisZ ? "lanternS" : "lanternN", new Vector3(fx0 - 0.9f, LanternY - 0.2f, zc), warm, LanternRange, LanternIntensity);
                NocHalos.Queue("SkybridgeGate", lc + new Vector3(-0.01f, 0f, 0f), rad, LanternRange);
                NocWetRegistry.AddSource(lc, lc, 0.3f, rad, SkybridgeDims.SOfX(lc.x));
            }
            float py = SkybridgeDims.OpeningTop;
            foreach (float dz in new[] { -PassageLineZ, PassageLineZ })
            {
                float z = SkybridgeDims.AxisZ + dz, cx = 0.5f * (PassageLineX0 + PassageLineX1), len = PassageLineX1 - PassageLineX0;
                MeshKit.ChamferBox(housing, new Vector3(cx, py - 0.025f, z), new Vector3(len + 0.1f, 0.06f, PassageLineW + 0.1f), 0.008f);   // 5 mm into the ceiling
                MeshKit.ChamferBox(lens, new Vector3(cx, py - 0.059f, z), new Vector3(len, 0.012f, PassageLineW), 0.003f);
                NocWetRegistry.AddSource(new Vector3(PassageLineX0 + 0.3f, py - 0.07f, z), new Vector3(PassageLineX1 - 0.3f, py - 0.07f, z), 0.2f, rad, SkybridgeDims.SOfX(cx));
            }
            BridgeGardenKit.PointLight(lg, "passage", new Vector3(0.5f * (PassageLineX0 + PassageLineX1), py - 1.0f, SkybridgeDims.AxisZ), warm, PassageRange, PassageIntensity);
            BridgeGardenKit.Emit(ctx, g, "lampHousings", housing, m.Steel);
            BridgeGardenKit.Emit(ctx, g, "lampLenses", lens, m.LampHead);
        }

        /// <summary>Points where floors meet the face on the opening's bottom line (deck edges, trench edges).</summary>
        static void RegisterFloorPoints(SkybridgeFace f, bool gateA, float holeBottom)
        {
            float d0 = SkybridgeDims.AxisZ - SkybridgeDims.DeckHalfWidth, d1 = SkybridgeDims.AxisZ + SkybridgeDims.DeckHalfWidth;
            f.AddPoint(d0, holeBottom);
            f.AddPoint(d1, holeBottom);
            f.AddPoint(SkybridgeDims.AxisZ - SkybridgeDims.PathSeamHalf, holeBottom);   // the deck top's inner vertex lines
            f.AddPoint(SkybridgeDims.AxisZ + SkybridgeDims.PathSeamHalf, holeBottom);
            if (gateA)
            {
                f.AddPoint(d0 + 0.05f, holeBottom);
                f.AddPoint(d1 - 0.05f, holeBottom);
            }
        }

        static readonly List<Vector3> s_poly = new List<Vector3>(32);

        /// <summary>Side face z = const between the two faces, with every height either face uses on its edge (fan from the centre).</summary>
        static void SidePolygon(MeshBuilder mb, float z, Vector3 want, float xa, List<float> va, float xb, List<float> vb)
        {
            s_poly.Clear();
            for (int i = 0; i < va.Count; i++) s_poly.Add(new Vector3(xa, va[i], z));          // up along xa
            for (int i = vb.Count - 1; i >= 0; i--) s_poly.Add(new Vector3(xb, vb[i], z));     // down along xb
            Vector3 c = Vector3.zero;
            foreach (var p in s_poly) c += p;
            c /= s_poly.Count;
            int centre = mb.Vertex(c, want, BridgeGardenKit.Uv(c, want));
            int first = mb.VertexCount;
            foreach (var p in s_poly) mb.Vertex(p, want, BridgeGardenKit.Uv(p, want));
            for (int i = 0; i < s_poly.Count; i++) MeshKit.Tri(mb, centre, first + i, first + (i + 1) % s_poly.Count);
        }
    }
}
