// SkybridgeTubes: the glass train tubes of 14.5.4 as open cylinders with NocGlass on two renderers (back faces at queue 3000, then
// front faces at 3001: D-008, H6 order). Bridge tubes N (z 207) and S (z 183): r 4.5, axis y 2.0, x 384-683 between the gate
// mouths. Face-to-face tubes: upper (y 118, z 195) and lower (y -142, z 209), r 5, x 215.3 (face A) to 815.3 (face B); GE and AB
// build their portals. TR (Phase 4 wave B): the trains run on TrainBuilder's guideway beam (G3's two NocLine rails near the glass
// bottom are gone: the beam carries the train), and the bridge tubes get steel structure that makes the glass read (INT-16 /
// INT-40, the "milky capsules with pill ends" at KS14): a collar at every gate mouth (a 0.48 m deep ring r 4.52..5.0 proud of the
// block face, so a tube visibly enters a framed portal instead of ending in a glowing dome) and a rib band (0.5 m wide,
// r 4.52..4.66) over every outrigger saddle (24 m pitch, x 399.3..663.3), where the tube is carried. Painted steel.
// TR fix wave (C41, partner of AB's C03): the face tubes get the same structure on the bridge's 24 m grid (x 375.3 + 24 k inside
// each open section: rib bands r 5.02..5.22, 0.45 m wide), and on the upper tube every rib carries a faint LED arc on its outer
// face around the bottom (NocLine 0.14 m, +-75 deg, eight chords, lit on their lower facets, SignWhite x FaceLineGain), so seen from
// the walk (KS04, KS11, KS14, KS15) the tube reads as a body of lit rings in perspective whose last ring is the portal, instead of a
// ring floating in the sky. (TR_r11: two continuous lines along the flanks read as strings hanging the ring from the sky.)
using UnityEngine;

namespace Megastructure
{
    public static class SkybridgeTubes
    {
        /// <summary>Collar at a gate mouth: radii, depth proud of the block face.</summary>
        public const float CollarRi = 4.52f, CollarRo = 5.0f, CollarDepth = 0.48f;
        /// <summary>Rib bands over the saddles: radii and width along x.</summary>
        public const float RibRi = 4.52f, RibRo = 4.66f, RibW = 0.5f;
        public const int RingSegments = 48;
        /// <summary>Face-tube rib bands (C41): radii, width along x.</summary>
        public const float FaceRibRi = 5.02f, FaceRibRo = 5.22f, FaceRibW = 0.45f;
        /// <summary>The upper tube's rib arcs (C41): true width, radius of the arc axis (on the rib's outer face), half angle from the
        /// bottom (deg), chords per arc, emission gain.</summary>
        public const float FaceLineD = 0.14f, FaceLineR = FaceRibRo + 0.5f * FaceLineD + 0.005f, FaceLineAngle = 75f, FaceLineGain = 0.22f;
        public const int FaceLineChords = 8;

        public static void Build(WorldContext ctx, Transform root, SkybridgeMaterials m)
        {
            var g = BridgeGardenKit.Group(root, "tubes");
            float r = SkybridgeDims.TubeR, y = SkybridgeDims.TubeAxisY, xa = SkybridgeDims.TubeX0, xb = SkybridgeDims.TubeX1;
            var glass = new MeshBuilder();
            var steel = new MeshBuilder();
            foreach (float z in new[] { SkybridgeDims.TubeSZ, SkybridgeDims.TubeNZ })
            {
                MeshKit.Cylinder(glass, new Vector3(xa, y, z), new Vector3(xb, y, z), r, SkybridgeDims.TubeSegments, false);
                // collars: from the block face out toward the span (gate A face x 384 -> +x, gate B face x 683 -> -x)
                Ring(steel, xa, xa + CollarDepth, y, z, CollarRi, CollarRo, false, true);
                Ring(steel, xb - CollarDepth, xb, y, z, CollarRi, CollarRo, true, false);
                foreach (float xr in RibXs())
                    Ring(steel, xr - 0.5f * RibW, xr + 0.5f * RibW, y, z, RibRi, RibRo, true, true);
            }
            BridgeGardenKit.EmitGlass(ctx, g, "bridgeTubes", glass, m.TubeGlassBack, m.TubeGlassFront);
            BridgeGardenKit.Emit(ctx, g, "tubeSteel", steel, m.Steel);

            var faceGlass = new MeshBuilder();
            float fa = SkybridgeDims.FaceAX, fb = SkybridgeDims.FaceBX, fr = SkybridgeDims.FaceTubeR;
            MeshKit.Cylinder(faceGlass, new Vector3(fa, SkybridgeDims.UpperY, SkybridgeDims.UpperZ), new Vector3(fb, SkybridgeDims.UpperY, SkybridgeDims.UpperZ), fr, 64, false);
            MeshKit.Cylinder(faceGlass, new Vector3(fa, SkybridgeDims.LowerY, SkybridgeDims.LowerZ), new Vector3(fb, SkybridgeDims.LowerY, SkybridgeDims.LowerZ), fr, 64, false);
            BridgeGardenKit.EmitGlass(ctx, g, "faceTubes", faceGlass, m.GlassBack, m.GlassFront);

            // C41: rib bands on both face tubes, LED channels on the upper one
            var faceSteel = new MeshBuilder();
            foreach (int t in new[] { TrainSchedule.TubeUpper, TrainSchedule.TubeLower })
            {
                var tb = TrainSchedule.Tubes[t];
                foreach (float xr in FaceRibXs(t))
                    Ring(faceSteel, xr - 0.5f * FaceRibW, xr + 0.5f * FaceRibW, (float)tb.AxisY, (float)tb.AxisZ, FaceRibRi, FaceRibRo, true, true);
            }
            BridgeGardenKit.Emit(ctx, g, "faceTubeRibs", faceSteel, m.Steel);
            var lines = new MeshBuilder();
            var up = TrainSchedule.Tubes[TrainSchedule.TubeUpper];
            foreach (float xr in FaceRibXs(TrainSchedule.TubeUpper))
                for (int i = 0; i < FaceLineChords; i++)
                {
                    Vector3 a = FaceArcPoint(i), b = FaceArcPoint(i + 1);
                    MeshKit.Cylinder(lines, new Vector3(xr, (float)up.AxisY + a.y, (float)up.AxisZ + a.z),
                        new Vector3(xr, (float)up.AxisY + b.y, (float)up.AxisZ + b.z), 0.5f * FaceLineD, 8, false);
                }
            BridgeGardenKit.Emit(ctx, g, "faceTubeLines", lines, m.FaceLine != null ? m.FaceLine : m.Steel);
        }

        /// <summary>The face-tube rib positions (C41): the bridge's 24 m grid x 375.3 + 24 k, at least 1 m inside the open section.</summary>
        public static float[] FaceRibXs(int tube)
        {
            var tb = TrainSchedule.Tubes[tube];
            var l = new System.Collections.Generic.List<float>(32);
            int k0 = Mathf.CeilToInt((float)((tb.MouthLo + 1.0 - SkybridgeDims.SpanX0) / SkybridgeDims.OutriggerPitch));
            for (int k = k0; ; k++)
            {
                float x = SkybridgeDims.SpanX0 + SkybridgeDims.OutriggerPitch * k;
                if (x > tb.MouthHi - 1.0) break;
                l.Add(x);
            }
            return l.ToArray();
        }

        /// <summary>Offset (y, z) from the upper tube's axis of chord point i (0..FaceLineChords) of a rib arc: r FaceLineR, from
        /// -FaceLineAngle (south) through the bottom to +FaceLineAngle (north).</summary>
        public static Vector3 FaceArcPoint(int i)
        {
            float a = (-FaceLineAngle + 2f * FaceLineAngle * i / FaceLineChords) * Mathf.Deg2Rad;
            return new Vector3(0f, -FaceLineR * Mathf.Cos(a), FaceLineR * Mathf.Sin(a));
        }

        /// <summary>The rib positions: over every outrigger saddle (x 375.3 + 24 k, inside the open section).</summary>
        public static float[] RibXs()
        {
            var l = new System.Collections.Generic.List<float>(16);
            for (int k = 1; ; k++)
            {
                float x = SkybridgeDims.SpanX0 + SkybridgeDims.OutriggerPitch * k;
                if (x >= SkybridgeDims.SpanX1 - 1f) break;
                if (x > SkybridgeDims.TubeX0 + 1f && x < SkybridgeDims.TubeX1 - 1f) l.Add(x);
            }
            return l.ToArray();
        }

        /// <summary>A ring of rectangular section about the x axis through (y, z): x0..x1, radii ri..ro; the end annuli only where
        /// asked (an end against a block face is buried and left out: no co-planar faces).</summary>
        public static void Ring(MeshBuilder mb, float x0, float x1, float y, float z, float ri, float ro, bool capLo, bool capHi)
        {
            int seg = RingSegments;
            for (int k = 0; k < seg; k++)
            {
                float a0 = 2f * Mathf.PI * k / seg, a1 = 2f * Mathf.PI * (k + 1) / seg;
                Vector3 d0 = new Vector3(0f, Mathf.Sin(a0), Mathf.Cos(a0)), d1 = new Vector3(0f, Mathf.Sin(a1), Mathf.Cos(a1));
                Vector3 c = new Vector3(0f, y, z);
                Vector3 o0a = c + d0 * ro + Vector3.right * x0, o0b = c + d0 * ro + Vector3.right * x1, o1a = c + d1 * ro + Vector3.right * x0, o1b = c + d1 * ro + Vector3.right * x1;
                Vector3 i0a = c + d0 * ri + Vector3.right * x0, i0b = c + d0 * ri + Vector3.right * x1, i1a = c + d1 * ri + Vector3.right * x0, i1b = c + d1 * ri + Vector3.right * x1;
                float u0 = a0 * ro, u1 = a1 * ro;
                Band(mb, o0a, o0b, o1b, o1a, d0, d1, u0, u1, x0, x1);
                Band(mb, i0a, i1a, i1b, i0b, -d0, -d1, u0, u1, x0, x1);
                if (capLo) BridgeGardenKit.OrientedQuad(mb, i0a, o0a, o1a, i1a, Vector3.left);
                if (capHi) BridgeGardenKit.OrientedQuad(mb, i0b, o0b, o1b, i1b, Vector3.right);
            }
        }

        static void Band(MeshBuilder mb, Vector3 a, Vector3 b, Vector3 c, Vector3 d, Vector3 na, Vector3 nd, float u0, float u1, float x0, float x1)
        {
            // a, b on the first radial (normal na), c, d on the second (normal nd); wound to face along the normals
            int ia = mb.Vertex(a, na, new Vector2(u0, a.x)), ib = mb.Vertex(b, na, new Vector2(u0, b.x));
            int ic = mb.Vertex(c, nd, new Vector2(u1, c.x)), id = mb.Vertex(d, nd, new Vector2(u1, d.x));
            Vector3 n = Vector3.Cross(b - a, c - a);
            if (Vector3.Dot(n, na + nd) >= 0f) mb.Quad(ia, ib, ic, id); else mb.Quad(ia, id, ic, ib);
        }

        /// <summary>Tube diameter (EditMode acceptance 9.0 +- 0.02).</summary>
        public static float BridgeTubeDiameter => 2f * SkybridgeDims.TubeR;
    }
}
