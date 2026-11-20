// StreetFacade: the two street walls of the boulevard canyon (MASTER_PROMPT 14.5.2 BoulevardBuilder, graybox tier; BoulevardLayout):
//   * the podium street faces (NocFacade) at z 120 / 270 above the 8 m arcade, 40 m high (south: 22 m over the alley's west block
//     x -90..-2.5 and 31 m over its east block x 2.5..33, G1-D1), the arcade below (back wall at z 116 / 274 with the shops of
//     ShopFronts (BV),
//     ceiling at y 8, 0.6 m columns every 6 m: 84 on x -39..213, solid piers x 2.5..4.5 beside the alley mouth), the arcade ending
//     at x -42 in the west (the podium face runs to the ground beyond) and at x 215.25 in the east;
//   * the shafts (NocFacade, y -3 300..560, the part below the podium top hidden inside the podium is not built): 7 north slabs +
//     the north-west slab x -60..-39 inside the hero exclusion, south slabs 2-6 (x 39..213), each 30 m wide and 30 m deep behind
//     the 4 m terrace setback, a few with vertical edge lights; slab 6's closure x 213..215.25 on both sides (agreement 2: the
//     street walls meet face A without a slot, 5 cm behind its plane x 215.3);
//   * the podium's step face at x 33 (south, y 31..40 above the alley's east block) and the east end faces at x 215.25.
// Co-planar rules: every face lies on its own plane or meets its neighbours edge to edge; the columns stand 2 cm behind the
// street plane; slab street faces start at the podium top (y 40), never on the arcade back wall's plane below.
using System.Collections.Generic;
using UnityEngine;

namespace Megastructure
{
    public static class StreetFacade
    {
        public const uint SeedSouth = 0xB0A1u, SeedNorth = 0xB0A2u;

        /// <summary>Edge-light kind of slab k on a side (a few slabs carry vertical edge lights; FacadeKit kinds).</summary>
        public static int SlabEdge(int side, int k)
        {
            uint h = NocMath.Pcg((uint)(k + 17) * 0x2C1B3C6Du ^ (side > 0 ? 0x5A5Au : 0xA5A5u));
            return (h % 3u) == 0u ? 1 + (int)((h >> 8) % 4u) : FacadeKit.EdgeNone;
        }

        public static void Build(G1Parts parts)
        {
            Side(parts, -1);
            Side(parts, +1);
        }

        /// <summary>side -1 = south (street face z 120, facing +z), +1 = north (z 270, facing -z).</summary>
        static void Side(G1Parts parts, int side)
        {
            bool south = side < 0;
            string sn = south ? "S" : "N";
            double zf = south ? BoulevardLayout.SouthZ : BoulevardLayout.NorthZ;        // street face plane
            double nzOut = south ? 1.0 : -1.0;                                           // outward normal z (toward the street)
            double zBack = zf - nzOut * BoulevardLayout.ArcadeDepth;                     // arcade back wall plane (116 / 274)
            double x0 = south ? BoulevardLayout.SouthX0 : BoulevardLayout.NorthX0;
            double x1 = BoulevardLayout.ClosureX1;
            double arcX0 = -42.0;
            uint seed = south ? SeedSouth : SeedNorth;
            uint podSkin = NocFacadeMath.SkinOverride(NocFacadeMath.OverridePodium);   // Look v2: precast + stone base (TEXTURE_PLAN 2.2)
            float y8 = (float)BoulevardLayout.ArcadeH;

            // ---- podium street faces (NocFacade), in two chunks per side (chunk-local origins)
            var podW = parts.Get("podium" + sn + "W", G1Mat.Facade, new Vector3(-10f, 0f, (float)zf));
            var podE = parts.Get("podium" + sn + "E", G1Mat.Facade, new Vector3(130f, 0f, (float)zf));
            foreach (var seg in PodiumSegments(side))
            {
                double a = seg.x, b = seg.y, h = seg.z;
                var mb = 0.5 * (a + b) < 60.0 ? podW : podE;
                // the part west of the arcade runs down into the floor, the rest starts at the arcade ceiling
                if (a < arcX0)
                {
                    double e = System.Math.Min(b, arcX0);
                    if (!south)
                    {
                        // BV: the tram depot portal (TramSchedule: x -53.6..-48.4, y 0..6.4) cut out of the face west of the arcade;
                        // the three pieces share the plane, so the window field stays continuous (FacadeKit)
                        double p0 = TramSchedule.PortalX - 0.5 * TramSchedule.PortalW, p1 = TramSchedule.PortalX + 0.5 * TramSchedule.PortalW;
                        FaceSpan(mb, a, p0, zf, -1.0, h, nzOut, podSkin);
                        FaceSpan(mb, p0, p1, zf, TramSchedule.PortalH, h, nzOut, podSkin);
                        FaceSpan(mb, p1, e, zf, -1.0, h, nzOut, podSkin);
                    }
                    else FaceSpan(mb, a, e, zf, -1.0, h, nzOut, podSkin);
                    a = System.Math.Max(a, arcX0);
                }
                if (b > a) FaceSpan(mb, a, b, zf, y8, h, nzOut, podSkin);
            }
            if (south)
            {
                // step face at x 33 (above the alley's east block, y 31..40, facing -x)
                FacadeKit.Face(podE, BoulevardLayout.SouthPodiumX0, BoulevardLayout.SouthShaftZ0, BoulevardLayout.SouthPodiumX0, zf,
                    AlleyLayout.EastPodiumH, BoulevardLayout.PodiumH, -1, 0, podSkin, FacadeKit.EdgeNone);
            }
            // east end faces x 215.25 (podium full depth, facing +x; behind face A on the flanks, agreement 2)
            double zp0 = south ? BoulevardLayout.SouthShaftZ0 : zf, zp1 = south ? zf : BoulevardLayout.NorthShaftZ1;
            FacadeKit.Face(podE, x1, zp1, x1, zp0, -1.0, BoulevardLayout.PodiumH, 1, 0, podSkin, FacadeKit.EdgeNone);

            // ---- arcade: back wall (Concrete), ceiling, end walls, piers, columns, shopfronts
            var arc = parts.Get("arcade" + sn, G1Mat.Concrete, new Vector3(85f, 4f, (float)zf));
            var col = parts.Get("columns" + sn, G1Mat.StoneWall, new Vector3(85f, 4f, (float)zf));
            Vector3 outN = new Vector3(0f, 0f, (float)nzOut);
            float zfF = (float)zf, zbF = (float)zBack;
            foreach (var span in ArcadeSpans(side))
            {
                float a = (float)span.x, b = (float)span.y;
                G1Mesh.Quad(arc, new Vector3(a, -1f, zbF), new Vector3(a, y8, zbF), new Vector3(b, y8, zbF), new Vector3(b, -1f, zbF), outN);
                G1Mesh.Quad(arc, new Vector3(a, y8, zbF), new Vector3(b, y8, zbF), new Vector3(b, y8, zfF), new Vector3(a, y8, zfF), Vector3.down);
            }
            // west end wall of the arcade (x -42, facing +x) and the east end wall (x 215.25, facing -x)
            G1Mesh.Quad(arc, new Vector3((float)arcX0, -1f, zbF), new Vector3((float)arcX0, y8, zbF), new Vector3((float)arcX0, y8, zfF), new Vector3((float)arcX0, -1f, zfF), Vector3.right);
            G1Mesh.Quad(arc, new Vector3((float)x1, -1f, zbF), new Vector3((float)x1, y8, zbF), new Vector3((float)x1, y8, zfF), new Vector3((float)x1, -1f, zfF), Vector3.left);
            if (south)
            {
                // piers beside the alley mouth: street faces z 120 (y -1..8) and the faces toward the arcade (x -+4.5)
                float px = (float)BoulevardLayout.MouthPierX, mw = (float)AlleyLayout.HalfWidth(AlleyLayout.MouthZ);
                G1Mesh.Quad(arc, new Vector3(-px, -1f, zfF), new Vector3(-px, y8, zfF), new Vector3(-mw, y8, zfF), new Vector3(-mw, -1f, zfF), outN);
                G1Mesh.Quad(arc, new Vector3(mw, -1f, zfF), new Vector3(mw, y8, zfF), new Vector3(px, y8, zfF), new Vector3(px, -1f, zfF), outN);
                G1Mesh.Quad(arc, new Vector3(-px, -1f, zbF), new Vector3(-px, y8, zbF), new Vector3(-px, y8, zfF), new Vector3(-px, -1f, zfF), Vector3.left);
                G1Mesh.Quad(arc, new Vector3(px, -1f, zbF), new Vector3(px, y8, zbF), new Vector3(px, y8, zfF), new Vector3(px, -1f, zfF), Vector3.right);
            }
            for (int k = BoulevardLayout.ColumnK0; k <= BoulevardLayout.ColumnK1; k++)
            {
                float cx = (float)ColumnX(k);
                if (south && Mathf.Abs(cx) < BoulevardLayout.MouthPierX + 0.5f * BoulevardLayout.ColumnW) continue;
                float zo = zfF - (float)(nzOut * BoulevardLayout.ColumnInset), zi = zo - (float)(nzOut * BoulevardLayout.ColumnW);
                G1Mesh.Chamfer(col, new Vector3(cx - 0.3f, 0f, Mathf.Min(zo, zi)), new Vector3(cx + 0.3f, y8, Mathf.Max(zo, zi)), 0.02f);
            }
            // shopfronts (BV, Phase 4, INT-22): varied shops with frames, shutters, fascia signs, pendants and the podium signs
            ShopFronts.Build(parts, side);

            // ---- shafts (slabs) above the podium and below the street, chunked per slab
            double zs0 = south ? BoulevardLayout.SouthShaftZ0 : BoulevardLayout.NorthShaftZ0;
            double zs1 = south ? BoulevardLayout.SouthShaftZ1 : BoulevardLayout.NorthShaftZ1;
            var slabs = new List<Vector3>(8);   // (xa, xb, k)
            if (!south) slabs.Add(new Vector3((float)BoulevardLayout.NorthWestSlabX0, (float)BoulevardLayout.NorthWestSlabX1, -1));
            for (int k = south ? BoulevardLayout.SouthFirstSlab : 0; k < BoulevardLayout.SlabCount; k++)
                slabs.Add(new Vector3((float)BoulevardLayout.SlabXa(k), (float)BoulevardLayout.SlabXb(k), k));
            int last = slabs.Count - 1;
            for (int i = 0; i < slabs.Count; i++)
            {
                double xa = slabs[i].x, xb = slabs[i].y;
                int k = (int)slabs[i].z;
                int edge = SlabEdge(side, k);
                var mb = parts.Get("slab" + sn + (k + 1), G1Mat.Facade, new Vector3((float)(0.5 * (xa + xb)), 0f, (float)(0.5 * (zs0 + zs1))));
                bool isLast = i == last;
                double xbEnd = isLast ? x1 : xb;          // slab 6 continues into its closure (one face, one window field)
                // above the podium: street face, slot faces (the last one is the closure's end face at x 215.25), back face
                FacadeKit.Box(mb, xa, xbEnd, zs0, zs1, BoulevardLayout.PodiumH, BoulevardLayout.ShaftY1, G1Mesh.Sides, seed + (uint)(k + 1), edge);
                // below the street (hidden by the floor and face A; 14.5.2 "shafts y -3 300..560")
                FacadeKit.Box(mb, xa, xbEnd, zs0, zs1, BoulevardLayout.ShaftY0, -1.0, G1Mesh.Sides, seed + (uint)(k + 1), FacadeKit.EdgeNone);
            }
        }

        static void FaceSpan(MeshBuilder mb, double a, double b, double z, double y0, double y1, double nz, uint seed)
        {
            if (nz > 0) FacadeKit.Face(mb, a, z, b, z, y0, y1, 0, 1, seed, FacadeKit.EdgeNone);
            else FacadeKit.Face(mb, b, z, a, z, y0, y1, 0, -1, seed, FacadeKit.EdgeNone);
        }

        /// <summary>Column centre x of index k (x = 3 + 6 k).</summary>
        public static double ColumnX(int k) => BoulevardLayout.ColumnX0 + BoulevardLayout.ColumnPitch * k;

        /// <summary>Podium street-face segments (x0, x1, top y) of a side: south 22 / 31 / 40 m (G1-D1), north 40 m.</summary>
        public static List<Vector3> PodiumSegments(int side)
        {
            var list = new List<Vector3>(4);
            if (side < 0)
            {
                float mw = (float)AlleyLayout.HalfWidth(AlleyLayout.MouthZ);
                list.Add(new Vector3((float)BoulevardLayout.SouthX0, -mw, (float)AlleyLayout.WestPodiumH));
                list.Add(new Vector3(mw, (float)BoulevardLayout.SouthPodiumX0, (float)AlleyLayout.EastPodiumH));
                list.Add(new Vector3((float)BoulevardLayout.SouthPodiumX0, (float)BoulevardLayout.ClosureX1, (float)BoulevardLayout.PodiumH));
            }
            else list.Add(new Vector3((float)BoulevardLayout.NorthX0, (float)BoulevardLayout.ClosureX1, (float)BoulevardLayout.PodiumH));
            return list;
        }

        /// <summary>The arcade's x spans (back wall and ceiling): x -42..215.25, south without the mouth piers.</summary>
        public static List<Vector2> ArcadeSpans(int side)
        {
            var list = new List<Vector2>(2);
            if (side < 0)
            {
                list.Add(new Vector2(-42f, -(float)BoulevardLayout.MouthPierX));
                list.Add(new Vector2((float)BoulevardLayout.MouthPierX, (float)BoulevardLayout.ClosureX1));
            }
            else list.Add(new Vector2(-42f, (float)BoulevardLayout.ClosureX1));
            return list;
        }
    }
}
