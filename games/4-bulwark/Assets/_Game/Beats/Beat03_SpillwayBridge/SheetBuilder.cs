// SheetBuilder: the water of beat 3 (MASTER_PROMPT 14.5.3 "Flow" and "Sheets"). Part of Beat03SpillwayBridge.
//   Draw-down water: one continuous surface over the whole spillway (z 420..704, x 46 .. -6; the piers stand in it), the
//   smoothstep profile of SpillwayGeometry (y -8.0 at x 46 to -11.5 at the lip, zero slope at both ends, so the sea of
//   BwSea meets it flat at x 46 and the sheets leave it horizontally). UV0 = (travel time from x 46 (s), surface speed
//   (m/s)): BwSheet advects its flow streaks in the Lagrangian coordinate (time x 8.3 m/s), so a streak passes a fixed
//   point at the same rate everywhere and stretches as the flow accelerates. 1 m steps along x, 4 m across.
//   14 near sheets (16 m wide) on the parabola x = -6 - 8.3 t, y = -11.5 - 4.9 t^2 for t 0..4.7 s (the outer surface; the
//   2.5 m lip thickness and its thinning h = q / v show in the side faces), steps denser near the lip; UV0 = (across the
//   slot 0..16 m, flight time t). BwSheet dissolves them by alpha-to-coverage over t 2.5..4.55 s (D 31..101 m); beyond
//   90 m the analytic curtain of the mist pass takes over. About 14 k triangles (14.5.3: 18 k).
using UnityEngine;

namespace Megastructure.Beats
{
    public sealed partial class Beat03SpillwayBridge
    {
        public const int WaterStepsX = 52, WaterStepsZ = 71;
        public const int SheetStepsT = 44, SheetStepsU = 10;
        /// <summary>The sheets' side faces show the thickness while it matters (they fray away with the edges).</summary>
        public const double SheetSideT1 = 3.6;

        // ================================================================ draw-down water
        void BuildWater(WorldContext ctx, Transform root)
        {
            var mb = new MeshBuilder();
            double x0 = SpillwayGeometry.LipX, x1 = SpillwayGeometry.MouthX, z0 = SpillwayGeometry.Z0, z1 = SpillwayGeometry.Z1;
            int nx = WaterStepsX, nz = WaterStepsZ;
            int first = mb.VertexCount;
            for (int i = 0; i <= nx; i++)
            {
                double x = x0 + (x1 - x0) * i / nx;
                float y = (float)SpillwayGeometry.WaterY(x);
                var n = new Vector3(-(float)SpillwayGeometry.WaterSlope(x), 1f, 0f).normalized;
                var uv = new Vector2((float)SpillwayGeometry.TravelTime(x), (float)SpillwayGeometry.Speed(x));
                for (int j = 0; j <= nz; j++)
                {
                    double z = z0 + (z1 - z0) * j / nz;
                    mb.Vertex(new Vector3((float)x, y, (float)z), n, uv);
                }
            }
            int row = nz + 1;
            for (int i = 0; i < nx; i++)
                for (int j = 0; j < nz; j++)
                {
                    int A = first + i * row + j, B = A + 1, C = A + row + 1, D = A + row;
                    mb.Quad(A, B, C, D);   // up: x grows with i, z with j
                }
            var mr = Flush(ctx, root, ref mb, "b03_water", m_water, false);
            if (mr != null) mr.receiveShadows = true;
        }

        // ================================================================ near sheets
        void BuildSheets(WorldContext ctx, Transform root)
        {
            var mb = new MeshBuilder();
            int nt = SheetStepsT, nu = SheetStepsU;
            var ts = new double[nt + 1];
            for (int i = 0; i <= nt; i++) ts[i] = SpillwayGeometry.SheetT1 * System.Math.Pow(i / (double)nt, 1.25);
            for (int k = 0; k < BulwarkConst.SlotCount; k++)
            {
                double z0 = SpillwayGeometry.SlotZ0(k), z1 = SpillwayGeometry.SlotZ1(k), w = z1 - z0;
                // outer surface
                int first = mb.VertexCount;
                for (int i = 0; i <= nt; i++)
                {
                    double t = ts[i];
                    SpillwayGeometry.SheetNormal(t, out double nx, out double ny);
                    var n = new Vector3((float)nx, (float)ny, 0f);
                    float x = (float)SpillwayGeometry.SheetX(t), y = (float)SpillwayGeometry.SheetY(t);
                    for (int j = 0; j <= nu; j++)
                    {
                        double u = w * j / nu;
                        mb.Vertex(new Vector3(x, y, (float)(z0 + u)), n, new Vector2((float)u, (float)t));
                    }
                }
                int row = nu + 1;
                for (int i = 0; i < nt; i++)
                    for (int j = 0; j < nu; j++)
                    {
                        int A = first + i * row + j, B = A + 1, C = A + row + 1, D = A + row;
                        mb.Quad(A, D, C, B);   // outer side (up and away from the dam)
                    }
                // side faces at both slot edges: outer curve to the inner curve (offset by the thickness h(t))
                for (int side = 0; side < 2; side++)
                {
                    float zs = (float)(side == 0 ? z0 : z1);
                    float u = side == 0 ? 0f : (float)w;
                    var nrm = side == 0 ? Vector3.back : Vector3.forward;
                    int sf = mb.VertexCount;
                    int count = 0;
                    for (int i = 0; i <= nt && ts[i] <= SheetSideT1 + 1e-9; i++, count++)
                    {
                        double t = ts[i];
                        SpillwayGeometry.SheetNormal(t, out double nx, out double ny);
                        double h = SpillwayGeometry.SheetThickness(t);
                        float xo = (float)SpillwayGeometry.SheetX(t), yo = (float)SpillwayGeometry.SheetY(t);
                        mb.Vertex(new Vector3(xo, yo, zs), nrm, new Vector2(u, (float)t));
                        mb.Vertex(new Vector3((float)(xo - nx * h), (float)(yo - ny * h), zs), nrm, new Vector2(u, (float)t));
                    }
                    for (int i = 0; i + 1 < count; i++)
                    {
                        int O0 = sf + 2 * i, I0 = O0 + 1, O1 = O0 + 2, I1 = O0 + 3;
                        if (side == 1) mb.Quad(O0, O1, I1, I0); else mb.Quad(O0, I0, I1, O1);
                    }
                }
            }
            var mr = Flush(ctx, root, ref mb, "b03_sheets", m_sheet, false);
            if (mr != null)
            {
                mr.receiveShadows = true;
                // the sheets are 120 m tall, never culled while any part is in view
                mr.allowOcclusionWhenDynamic = false;
            }
        }
    }
}
