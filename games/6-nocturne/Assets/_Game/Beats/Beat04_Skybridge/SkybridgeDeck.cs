// SkybridgeDeck: the walking deck of the skybridge (14.5.4): a 0.5 m steel slab, 5.0 m between the rails (z 192.5-197.5), top
// at y 0 (the H4 mirror plane, lib.FloorFor = the NocWet steel deck), from the tongue tip x 375.3 to gate B's east face x 695.
// Inside gate B (x 683-695) the top widens to the 12 m walkway opening (z 189-201), because the lane funnels 2.15 -> 3.5 there.
// The stud line every 4 m is a smooth cosine seam (0.20 m wide, 12 mm deep) in the one top surface: every seam sample is a
// shared vertex row with an interpolated normal (no hard facet that could sparkle), and the slab sides follow the same profile,
// so top, sides and soffit meet on identical vertex positions (no T-junction, no crack). The glass strip of 14.5.4 (s 706-726;
// GE, D-029 GB-05): a hole x 521.3-541.3, |z - 195| <= 1.2 through the slab (top and soffit cut on the same vertex lines, four
// painted-steel walls that follow the seam profile) under a NocGlass pane 3 cm below the top; PathGuide keeps the y -0.05 floor
// collider under it, so the walker crosses on the glass and KS13 looks through it into the abyss (T6, the y -520 layer).
using System.Collections.Generic;
using UnityEngine;

namespace Megastructure
{
    public static class SkybridgeDeck
    {
        /// <summary>One sample of the top profile: x, the seam dip y (0 on the flat) and the smooth normal.</summary>
        public struct Sample { public float x, y; public Vector3 n; }

        static readonly List<Sample> s_prof = new List<Sample>(512);

        /// <summary>The top profile along x (seam dips every SeamPitch), sorted, with smooth normals.</summary>
        public static List<Sample> Profile()
        {
            s_prof.Clear();
            float w = SkybridgeDims.SeamWidth, d = SkybridgeDims.SeamDepth;
            Add(SkybridgeDims.DeckX0, 0f, 0f);
            bool gateEdgeAdded = false;
            for (int k = 1; ; k++)
            {
                float xs = SkybridgeDims.DeckX0 + SkybridgeDims.SeamPitch * k;
                if (xs + 0.5f * w >= SkybridgeDims.DeckX1 - 0.05f) break;
                if (!gateEdgeAdded && xs - 0.5f * w > SkybridgeDims.GateBX0)
                {
                    Add(SkybridgeDims.GateBX0, 0f, 0f);
                    gateEdgeAdded = true;
                }
                for (int j = -2; j <= 2; j++)
                {
                    float t = 0.5f * j;                                     // -1 .. 1 across the seam
                    float y = -d * 0.5f * (1f + Mathf.Cos(Mathf.PI * t));
                    float slope = d * 0.5f * Mathf.PI * Mathf.Sin(Mathf.PI * t) / (0.5f * w);
                    Add(xs + t * 0.5f * w, y, slope);
                }
            }
            if (!gateEdgeAdded) Add(SkybridgeDims.GateBX0, 0f, 0f);
            Add(SkybridgeDims.StripX0, 0f, 0f);                             // the glass strip's ends lie on the flat between seams
            Add(SkybridgeDims.StripX1, 0f, 0f);
            Add(SkybridgeDims.DeckX1, 0f, 0f);
            s_prof.Sort((a, b) => a.x.CompareTo(b.x));
            return s_prof;
        }

        static void Add(float x, float y, float slope)
        {
            s_prof.Add(new Sample { x = x, y = Mathf.Abs(y) < 1e-7f ? 0f : y, n = new Vector3(-slope, 1f, 0f).normalized });
        }

        public static void Build(WorldContext ctx, Transform root, SkybridgeMaterials m)
        {
            var prof = Profile();
            float z0 = SkybridgeDims.AxisZ - SkybridgeDims.DeckHalfWidth, z1 = SkybridgeDims.AxisZ + SkybridgeDims.DeckHalfWidth;
            float w0 = SkybridgeDims.OpeningZ0, w1 = SkybridgeDims.OpeningZ1;
            float bottom = -SkybridgeDims.DeckThickness;

            // ---- top (NocWet): a vertex grid over the profile samples x the z lines: deck edges 192.5 / 197.5 and the garden path
            // edges 193.5 / 196.5 everywhere (the deck end at x 695 meets the path ribbon and the moss on those vertices), plus the
            // walkway opening edges 189 / 201 from x 683
            var top = new MeshBuilder();
            int n = prof.Count;
            float zs0 = SkybridgeDims.AxisZ - SkybridgeDims.StripHalf, zs1 = SkybridgeDims.AxisZ + SkybridgeDims.StripHalf;
            float[] zl = { w0, z0, SkybridgeDims.AxisZ - SkybridgeDims.PathSeamHalf, zs0, zs1, SkybridgeDims.AxisZ + SkybridgeDims.PathSeamHalf, z1, w1 };
            const int stripBand = 3;                                        // zl[3]..zl[4] = the strip's z range
            var idx = new int[n, zl.Length];
            // the strip lines (zl[3], zl[4]) exist only on the samples x 521.3..541.3, so the deck's vertex set elsewhere (and on the
            // line x 695, where the garden terrace has twins of every vertex: Beat05GardenTests.FloorSeam) is unchanged (FO's request)
            for (int i = 0; i < n; i++)
            {
                Sample p = prof[i];
                bool wide = p.x >= SkybridgeDims.GateBX0, strip = OnStrip(p.x);
                for (int j = 0; j < zl.Length; j++)
                {
                    bool use = (wide || (j > 0 && j < zl.Length - 1)) && (strip || (j != stripBand && j != stripBand + 1));
                    idx[i, j] = use ? top.Vertex(new Vector3(p.x, p.y, zl[j]), p.n, new Vector2(p.x, zl[j])) : -1;
                }
            }
            for (int i = 0; i + 1 < n; i++)
            {
                bool sa = OnStrip(prof[i].x), sb2 = OnStrip(prof[i + 1].x);
                for (int j = 0; j + 1 < zl.Length; j++)
                {
                    if (j >= stripBand - 1 && j <= stripBand + 1 && !(sa && sb2))
                    {
                        // the path band 193.5..196.5 next to (or away from) the strip: one quad, or a fan from the side without the
                        // strip lines onto the side with them (the collinear strip vertices stay on the shared edge: no T-junction)
                        if (j != stripBand - 1) continue;
                        int lo = stripBand - 1, hi = stripBand + 2;
                        if (!sa && !sb2) { MeshKit.Quad(top, idx[i, lo], idx[i + 1, lo], idx[i + 1, hi], idx[i, hi]); continue; }
                        int k = sa ? i : i + 1, f = sa ? i + 1 : i;            // k: the sample with the strip lines
                        var fan = new int[] { idx[k, lo], idx[k, stripBand], idx[k, stripBand + 1], idx[k, hi] };
                        for (int t = 0; t + 1 < fan.Length; t++)
                            TriUp(top, idx[f, lo], fan[t], fan[t + 1]);
                        TriUp(top, idx[f, lo], idx[k, hi], idx[f, hi]);
                        continue;
                    }
                    int a = idx[i, j], b = idx[i + 1, j], c = idx[i + 1, j + 1], d = idx[i, j + 1];
                    if (j == stripBand && InStrip(prof[i].x, prof[i + 1].x)) continue;
                    if (a >= 0 && b >= 0 && c >= 0 && d >= 0) MeshKit.Quad(top, a, b, c, d);
                }
            }
            BridgeGardenKit.Emit(ctx, root, "deckTop", top, m.Deck);

            // ---- sides, soffit and the west end (painted steel) over the 5 m part x 375.3 .. 683 (inside gate B the slab is buried
            // in the block); the side top edges use the same profile samples as the top
            var sb = new MeshBuilder();
            int last = 0;
            while (last + 1 < n && prof[last + 1].x <= SkybridgeDims.GateBX0) last++;
            for (int side = 0; side < 2; side++)
            {
                float z = side == 0 ? z0 : z1;
                var nrm = side == 0 ? Vector3.back : Vector3.forward;
                int first = sb.VertexCount;
                for (int i = 0; i <= last; i++)
                {
                    sb.Vertex(new Vector3(prof[i].x, prof[i].y, z), nrm, new Vector2(prof[i].x, prof[i].y));
                    sb.Vertex(new Vector3(prof[i].x, bottom, z), nrm, new Vector2(prof[i].x, bottom));
                }
                for (int i = 0; i < last; i++)
                {
                    int a = first + 2 * i;
                    MeshKit.Quad(sb, a, a + 2, a + 3, a + 1);
                }
            }
            {
                // soffit on the z lines z0 / z1, plus the strip lines on the strip's samples (its band left open inside x 521.3..541.3;
                // a fan onto the strip lines at both ends, so no T-junction and the slab ends keep their two-vertex edges)
                float[] zb = { z0, zs0, zs1, z1 };
                var sv = new int[last + 1, zb.Length];
                for (int i = 0; i <= last; i++)
                    for (int j = 0; j < zb.Length; j++)
                        sv[i, j] = (j == 0 || j == 3 || OnStrip(prof[i].x)) ? sb.Vertex(new Vector3(prof[i].x, bottom, zb[j]), Vector3.down, new Vector2(prof[i].x, zb[j])) : -1;
                for (int i = 0; i < last; i++)
                {
                    bool sa = OnStrip(prof[i].x), sb2 = OnStrip(prof[i + 1].x);
                    if (!sa && !sb2) { MeshKit.Quad(sb, sv[i, 0], sv[i + 1, 0], sv[i + 1, 3], sv[i, 3]); continue; }
                    if (sa && sb2)
                    {
                        MeshKit.Quad(sb, sv[i, 0], sv[i + 1, 0], sv[i + 1, 1], sv[i, 1]);
                        MeshKit.Quad(sb, sv[i, 2], sv[i + 1, 2], sv[i + 1, 3], sv[i, 3]);
                        continue;
                    }
                    int k = sa ? i : i + 1, f = sa ? i + 1 : i;
                    MeshKit.Tri(sb, sv[f, 0], sv[k, 0], sv[k, 1]);
                    MeshKit.Tri(sb, sv[f, 0], sv[k, 1], sv[k, 2]);
                    MeshKit.Tri(sb, sv[f, 0], sv[k, 2], sv[k, 3]);
                    MeshKit.Tri(sb, sv[f, 0], sv[k, 3], sv[f, 3]);
                }
                // the strip's walls (facing into the hole): along z at the strip's two sides (following the seam profile), across x at its ends
                for (int i = 0; i < last; i++)
                {
                    if (!InStrip(prof[i].x, prof[i + 1].x)) continue;
                    Sample pa = prof[i], pb = prof[i + 1];
                    BridgeGardenKit.OrientedQuad(sb, new Vector3(pa.x, bottom, zs0), new Vector3(pa.x, pa.y, zs0), new Vector3(pb.x, pb.y, zs0), new Vector3(pb.x, bottom, zs0), Vector3.forward);
                    BridgeGardenKit.OrientedQuad(sb, new Vector3(pa.x, bottom, zs1), new Vector3(pa.x, pa.y, zs1), new Vector3(pb.x, pb.y, zs1), new Vector3(pb.x, bottom, zs1), Vector3.back);
                }
                BridgeGardenKit.OrientedQuad(sb, new Vector3(SkybridgeDims.StripX0, bottom, zs0), new Vector3(SkybridgeDims.StripX0, 0f, zs0),
                    new Vector3(SkybridgeDims.StripX0, 0f, zs1), new Vector3(SkybridgeDims.StripX0, bottom, zs1), Vector3.right);
                BridgeGardenKit.OrientedQuad(sb, new Vector3(SkybridgeDims.StripX1, bottom, zs0), new Vector3(SkybridgeDims.StripX1, 0f, zs0),
                    new Vector3(SkybridgeDims.StripX1, 0f, zs1), new Vector3(SkybridgeDims.StripX1, bottom, zs1), Vector3.left);
            }
            BridgeGardenKit.OrientedQuad(sb, new Vector3(SkybridgeDims.DeckX0, bottom, z0), new Vector3(SkybridgeDims.DeckX0, 0f, z0),
                new Vector3(SkybridgeDims.DeckX0, 0f, z1), new Vector3(SkybridgeDims.DeckX0, bottom, z1), Vector3.left);
            BridgeGardenKit.Emit(ctx, root, "deckSlab", sb, m.Steel);

            // the glass pane of the strip (NocGlass back / front faces, D-008), facing up, 3 cm below the deck top
            var glass = new MeshBuilder();
            float gy = SkybridgeDims.StripGlassY;
            BridgeGardenKit.OrientedQuad(glass, new Vector3(SkybridgeDims.StripX0, gy, zs0), new Vector3(SkybridgeDims.StripX1, gy, zs0),
                new Vector3(SkybridgeDims.StripX1, gy, zs1), new Vector3(SkybridgeDims.StripX0, gy, zs1), Vector3.up);
            BridgeGardenKit.EmitGlass(ctx, root, "glassStrip", glass, m.GlassBack, m.GlassFront);
        }

        /// <summary>True when the profile sample x lies on the glass strip's x range (its ends included).</summary>
        static bool OnStrip(float x) => x >= SkybridgeDims.StripX0 - 1e-4f && x <= SkybridgeDims.StripX1 + 1e-4f;

        /// <summary>A deck-top triangle wound to face up (the deck's quads are wound the same way by MeshKit.Quad).</summary>
        static void TriUp(MeshBuilder mb, int a, int b, int c)
        {
            Vector3 pa = mb.Positions[a], pb = mb.Positions[b], pc = mb.Positions[c];
            if (Vector3.Cross(pb - pa, pc - pa).y >= 0f) mb.Tri(a, b, c); else mb.Tri(a, c, b);
        }

        /// <summary>True when the profile interval [xa, xb] lies inside the glass strip's x range.</summary>
        static bool InStrip(float xa, float xb) => xa >= SkybridgeDims.StripX0 - 1e-4f && xb <= SkybridgeDims.StripX1 + 1e-4f;

        /// <summary>Number of seams in the deck top (EditMode test: one every 4 m).</summary>
        public static int SeamCount()
        {
            int c = 0;
            foreach (var p in Profile()) if (p.y <= -SkybridgeDims.SeamDepth + 1e-5f) c++;
            return c;
        }
    }
}
