// SkybridgeTruss: the structure under the deck (14.5.4): two planar Warren trusses 10 m deep (25 bays of 12 m, I-section
// chords and diagonals, gusset plates; MeshKit.WarrenTruss) at z 195 +- 2.0 between the bearings x 375.3 (inside gate block A)
// and x 675.3 (the tongue B bearing pier under gate block B): span 300.0 m. Lateral braces join the two trusses at every
// node, top and bottom. Outriggers on the top nodes every 24 m (x 399.3 .. 663.3) carry the two train tubes: a cross beam
// under both tubes at y -3.0, hangers from the top chords, struts down to the bottom chords, and a saddle under each tube.
// Painted steel; the two bearing piers are concrete. Member ends always stop inside another member (no co-planar end faces).
using UnityEngine;

namespace Megastructure
{
    public static class SkybridgeTruss
    {
        public static void Build(WorldContext ctx, Transform root, SkybridgeMaterials m)
        {
            float zA = SkybridgeDims.AxisZ - SkybridgeDims.TrussPlaneOffset, zB = SkybridgeDims.AxisZ + SkybridgeDims.TrussPlaneOffset;
            float yT = SkybridgeDims.TrussTopY, yBot = yT - SkybridgeDims.TrussDepth;
            float x0 = SkybridgeDims.SpanX0, x1 = SkybridgeDims.SpanX1, s = SkybridgeDims.TrussMember;
            var mb = new MeshBuilder();
            MeshKit.WarrenTruss(mb, new Vector3(x0, yT, zA), new Vector3(x1, yT, zA), SkybridgeDims.TrussDepth, SkybridgeDims.TrussBays, s, Vector3.up);
            MeshKit.WarrenTruss(mb, new Vector3(x0, yT, zB), new Vector3(x1, yT, zB), SkybridgeDims.TrussDepth, SkybridgeDims.TrussBays, s, Vector3.up);

            // lateral braces at every node (top under the deck, bottom), ends inside the chord webs
            float b = SkybridgeDims.BraceSize;
            for (int i = 0; i <= SkybridgeDims.TrussBays; i++)
            {
                float x = SkybridgeDims.TrussNodeX(i);
                x = Mathf.Clamp(x, x0 + 0.5f * b, x1 - 0.5f * b);
                MeshKit.ChamferMember(mb, new Vector3(x, yT, zA), new Vector3(x, yT, zB), b, b, 0.02f, Vector3.up);
                MeshKit.ChamferMember(mb, new Vector3(x, yBot, zA), new Vector3(x, yBot, zB), b, b, 0.02f, Vector3.up);
            }

            // outriggers on the top nodes (every 24 m)
            float oy = SkybridgeDims.OutriggerY;
            for (int k = 1; ; k++)
            {
                float x = x0 + SkybridgeDims.OutriggerPitch * k;
                if (x >= x1 - 1f) break;
                // cross beam (0.5 x 0.6) under both tubes
                MeshKit.ChamferMember(mb, new Vector3(x, oy, SkybridgeDims.OutriggerZ0), new Vector3(x, oy, SkybridgeDims.OutriggerZ1), 0.5f, 0.6f, 0.02f, Vector3.up);
                // hangers from the top chord node down into the beam
                MeshKit.ChamferMember(mb, new Vector3(x, yT, zA), new Vector3(x, oy, zA), 0.3f, 0.3f, 0.02f, Vector3.forward);
                MeshKit.ChamferMember(mb, new Vector3(x, yT, zB), new Vector3(x, oy, zB), 0.3f, 0.3f, 0.02f, Vector3.forward);
                // struts from the beam ends down to the bottom chords
                MeshKit.ChamferMember(mb, new Vector3(x, oy, SkybridgeDims.OutriggerZ0 + 0.3f), new Vector3(x, yBot, zA), 0.3f, 0.3f, 0.02f, Vector3.right);
                MeshKit.ChamferMember(mb, new Vector3(x, oy, SkybridgeDims.OutriggerZ1 - 0.3f), new Vector3(x, yBot, zB), 0.3f, 0.3f, 0.02f, Vector3.right);
                // saddles under the tubes (top 2 cm under the glass)
                float st = SkybridgeDims.SaddleTop, sb = oy + 0.25f;
                foreach (float zc in new[] { SkybridgeDims.TubeSZ, SkybridgeDims.TubeNZ })
                    MeshKit.ChamferBox(mb, new Vector3(x, 0.5f * (st + sb), zc), new Vector3(0.6f, st - sb, 3.2f), 0.02f);
            }
            BridgeGardenKit.Emit(ctx, root, "truss", mb, m.Steel);

            // bearing piers: A under gate block A (x 375.3 .. 384, top buried in the block's underside), B under the tongue B tip and
            // gate block B (x 675.3 .. 695; its top under the deck between the trusses, buried in the block from x 683)
            var pier = new MeshBuilder();
            BridgeGardenKit.OpenBox(pier, new Vector3(SkybridgeDims.PierAX0, SkybridgeDims.PierABottom, SkybridgeDims.PierAZ0),
                new Vector3(SkybridgeDims.PierAX1, SkybridgeDims.GateBottom + 0.05f, SkybridgeDims.PierAZ1), BridgeGardenKit.AllFaces & ~BridgeGardenKit.FacePY);
            BridgeGardenKit.OpenBox(pier, new Vector3(SkybridgeDims.PierX0, SkybridgeDims.PierBottom, SkybridgeDims.PierZ0),
                new Vector3(SkybridgeDims.PierX1, -SkybridgeDims.DeckThickness, SkybridgeDims.PierZ1), BridgeGardenKit.AllFaces);
            BridgeGardenKit.Emit(ctx, root, "piers", pier, m.Concrete);
        }

        /// <summary>Span between the truss bearings (EditMode acceptance: 300.0 +- 0.05).</summary>
        public static float Span => SkybridgeDims.SpanX1 - SkybridgeDims.SpanX0;
    }
}
