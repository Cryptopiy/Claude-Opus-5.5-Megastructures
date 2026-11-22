// NocTraffic: the traffic lane system of MASTER_PROMPT 14.7 H7 for the beat builders (boulevard rivers, Gulf layers, cross-bridge
// lights). A lane is a straight segment A -> B (a polyline is a chain of lanes); vehicle i has phase p = frac(phi_i + v_i t / L)
// with phi_i = (i + jitter) / N (jitter +-0.3: no wagon-wheel), v_i = v (1 +- 0.03) quantised so that v_i P / L is a whole
// number for the time wrap P = 65 536 s (t is wrapped by a double modulo on the CPU and the phase is continuous across the
// wrap), alpha = smoothstep(0, e, p) smoothstep(1, 1 - e, p) (e 0.04 on lanes, 0.06 on layers): the ramps must lie at T < 0.2 or
// inside a portal, which TrafficTests checks per key pose. Rendering: one NocStreak instance per vehicle (Abyss InstancedBatch,
// <= 511 per draw, bounds = the lane AABB, per-camera submission through NocCameraBatches); the vehicle state rides in the
// instance matrix (column 0 = B - A, column 1 = (0, v_i, 0), column 2 = lateral x (1 + phi_i), column 3 = A), colour and seed
// come from a hash of the position (D-008). The streak is max(v x 0.12 s, footprint) long and at least 2.2 px in both axes,
// energy x (true / drawn)^2 (NocStreak.shader). Boulevard demo: the ten lanes of 14.5.2 (south east-bound red tails, north
// west-bound cyan-white heads) from x -4 000 (T 0.11 at KS04) into face B's portals (the ramp is the last e L of the lane, east
// of x_B = 815.3). GE lane edit (D-029 GB-06, GateDims): the lanes enter the notch gate's lintel at x 815.9, cross the garden
// courtyard above the crowns and end inside the east block (the ramp x 815.3..1 015.9 runs in the lintel, the courtyard and the
// block); the four spec lanes below y 135 ((-48, 60), (-42, 95), (+44, 70), (+50, 112)) would enter the crowns at the face-B plane
// (y 98-125) and rise to y 150, 175, 145, 185, above the canyon bridge (roof y 132) and the crowns (135.4 m inside the notch).
// ARK (D-043): the two skyway lanes of 14.5.2 (z 195, y 320 east-bound and 400 west-bound) crossed the rift inside the Ark's
// volume (x 295-735, y 300-600: the ship would sail through them); they run at y 275 and 290 now, under its gondolas. Over the
// rift the walker sees no boulevard vehicle within 300 m (NocStreak NocWalkClear, AbyssTraffic.BoulevardClear).
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Megastructure
{
    public sealed class TrafficLane
    {
        public string name;
        public double ax, ay, az, bx, by, bz;     // world (m)
        public double speed = NocturneConstants.TrafficSpeed;
        public double spacing = NocturneConstants.TrafficSpacing;
        public double ramp = NocturneConstants.TrafficRampLane;
        public double Length => Math.Sqrt((bx - ax) * (bx - ax) + (by - ay) * (by - ay) + (bz - az) * (bz - az));
    }

    public static class NocTraffic
    {
        /// <summary>Speed quantised so that v P / L is a whole number of laps (P = the time wrap): no jump when t wraps.</summary>
        public static double QuantiseSpeed(double v, double L)
        {
            double P = NocturneConstants.TrafficTimeWrap;
            return Math.Max(1.0, Math.Round(v * P / L)) * L / P;
        }

        /// <summary>Scene time wrapped for the shader (double modulo).</summary>
        public static double WrapTime(double t) => t - NocturneConstants.TrafficTimeWrap * Math.Floor(t / NocturneConstants.TrafficTimeWrap);

        public static double Phase(double phi, double v, double L, double tWrapped)
        {
            double p = phi + v * tWrapped / L;
            return p - Math.Floor(p);
        }

        public static double Alpha(double p, double e) => NocMath.Smoothstep(0.0, e, p) * (1.0 - NocMath.Smoothstep(1.0 - e, 1.0, p));

        /// <summary>World length of a streak: max(v x 0.12 s, the pixel footprint x 2.2).</summary>
        public static double StreakLength(double v, double footprint) => Math.Max(v * NocturneConstants.StreakSeconds, NocturneConstants.StreakMinPx * footprint);

        /// <summary>Vehicles of a lane: (phi_i, v_i) with jittered phases and quantised speeds (deterministic per lane seed).</summary>
        public static void Vehicles(TrafficLane lane, uint seed, List<double> phis, List<double> speeds)
        {
            double L = lane.Length;
            int n = Math.Max(1, (int)Math.Round(L / lane.spacing));
            for (int i = 0; i < n; i++)
            {
                double j = NocMath.Lerp(-NocturneConstants.TrafficJitter, NocturneConstants.TrafficJitter, NocHash.U01(NocHash.H3((uint)i, seed, 0x54726166u, 0)));
                double phi = (i + j) / n;
                phi -= Math.Floor(phi);
                double dv = NocMath.Lerp(-NocturneConstants.TrafficSpeedJitter, NocturneConstants.TrafficSpeedJitter, NocHash.U01(NocHash.H3((uint)i, seed, 0x54726166u, 1)));
                phis.Add(phi);
                speeds.Add(QuantiseSpeed(lane.speed * (1.0 + dv), L));
            }
        }

        /// <summary>World position of a vehicle at wrapped time t (C# mirror of the NocStreak vertex shader).</summary>
        public static void Position(TrafficLane lane, double phi, double v, double tWrapped, out double x, out double y, out double z, out double alpha)
        {
            double L = lane.Length, p = Phase(phi, v, L, tWrapped);
            x = lane.ax + (lane.bx - lane.ax) * p; y = lane.ay + (lane.by - lane.ay) * p; z = lane.az + (lane.bz - lane.az) * p;
            alpha = Alpha(p, lane.ramp);
        }

        /// <summary>The unit streak quad (0..1 in x and y: bounds of the instance = the lane segment).</summary>
        public static Mesh LaneQuad()
        {
            var m = new Mesh { name = "NocStreakLaneQuad" };
            m.SetVertices(new[] { new Vector3(0, 0, 0), new Vector3(1, 0, 0), new Vector3(1, 1, 0), new Vector3(0, 1, 0) });
            m.SetUVs(0, new[] { new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 1) });
            m.SetNormals(new[] { Vector3.back, Vector3.back, Vector3.back, Vector3.back });
            m.SetIndices(new[] { 0, 1, 2, 0, 2, 3 }, MeshTopology.Triangles, 0);
            m.RecalculateBounds();
            return m;
        }

        /// <summary>The unit point quad (-0.5..0.5): beacons, fireflies (bounds = the light).</summary>
        public static Mesh PointQuad()
        {
            var m = new Mesh { name = "NocStreakPointQuad" };
            m.SetVertices(new[] { new Vector3(-0.5f, -0.5f, 0), new Vector3(0.5f, -0.5f, 0), new Vector3(0.5f, 0.5f, 0), new Vector3(-0.5f, 0.5f, 0) });
            m.SetUVs(0, new[] { new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 1) });
            m.SetNormals(new[] { Vector3.back, Vector3.back, Vector3.back, Vector3.back });
            m.SetIndices(new[] { 0, 1, 2, 0, 2, 3 }, MeshTopology.Triangles, 0);
            m.RecalculateBounds();
            return m;
        }

        /// <summary>AB: the strip of a layer glow (NocStreak kind 6): `segments` quads along uv.x 0..1, uv.y 0 / 1 across (positions =
        /// uv, so the instance bounds are the lane's AABB).</summary>
        public static Mesh RibbonMesh(int segments)
        {
            var m = new Mesh { name = "NocStreakRibbon" };
            var v = new Vector3[2 * (segments + 1)];
            var uv = new Vector2[v.Length];
            var nrm = new Vector3[v.Length];
            for (int i = 0; i <= segments; i++)
                for (int j = 0; j < 2; j++)
                {
                    float t = (float)i / segments;
                    v[2 * i + j] = new Vector3(t, j, 0f); uv[2 * i + j] = new Vector2(t, j); nrm[2 * i + j] = Vector3.back;
                }
            var idx = new int[6 * segments];
            for (int i = 0; i < segments; i++)
            {
                int a = 2 * i, b = a + 1, c = a + 3, d = a + 2;
                idx[6 * i] = a; idx[6 * i + 1] = b; idx[6 * i + 2] = c; idx[6 * i + 3] = a; idx[6 * i + 4] = c; idx[6 * i + 5] = d;
            }
            m.SetVertices(v); m.SetUVs(0, uv); m.SetNormals(nrm);
            m.SetIndices(idx, MeshTopology.Triangles, 0);
            m.RecalculateBounds();
            return m;
        }

        /// <summary>AB: instance matrix of a layer glow (NocStreak kind 6): col0 = B - A, col1 = (intensity, sigma m, 0), col2 = the
        /// lateral unit vector (keeps the matrix invertible), col3 = A.</summary>
        public static Matrix4x4 RibbonMatrix(TrafficLane lane, double intensity, double sigma)
        {
            double dx = lane.bx - lane.ax, dz = lane.bz - lane.az, h = Math.Sqrt(dx * dx + dz * dz);
            double nx = h > 1e-6 ? -dz / h : 1.0, nz = h > 1e-6 ? dx / h : 0.0;
            var m = new Matrix4x4();
            m.SetColumn(0, new Vector4((float)dx, (float)(lane.by - lane.ay), (float)dz, 0f));
            m.SetColumn(1, new Vector4((float)intensity, (float)sigma, 0f, 0f));
            m.SetColumn(2, new Vector4((float)nx, 0f, (float)nz, 0f));
            m.SetColumn(3, new Vector4((float)lane.ax, (float)lane.ay, (float)lane.az, 1f));
            return m;
        }

        /// <summary>Instance matrix of one vehicle (see the header): col0 = B - A, col1 = (0, v, 0), col2 = lateral (1 + phi), col3 = A.</summary>
        public static Matrix4x4 VehicleMatrix(TrafficLane lane, double phi, double v)
        {
            double dx = lane.bx - lane.ax, dz = lane.bz - lane.az, h = Math.Sqrt(dx * dx + dz * dz);
            double nx = h > 1e-6 ? -dz / h : 1.0, nz = h > 1e-6 ? dx / h : 0.0;
            var m = new Matrix4x4();
            m.SetColumn(0, new Vector4((float)dx, (float)(lane.by - lane.ay), (float)dz, 0f));
            m.SetColumn(1, new Vector4(0f, (float)v, 0f, 0f));
            m.SetColumn(2, new Vector4((float)(nx * (1.0 + phi)), 0f, (float)(nz * (1.0 + phi)), 0f));
            m.SetColumn(3, new Vector4((float)lane.ax, (float)lane.ay, (float)lane.az, 1f));
            return m;
        }

        /// <summary>Instance matrix of a static light (beacon / firefly): TRS(p, identity, size) with col2 scaled by (1 + phase).</summary>
        public static Matrix4x4 PointMatrix(double x, double y, double z, double size, double phase01)
        {
            var m = new Matrix4x4();
            m.SetColumn(0, new Vector4((float)size, 0f, 0f, 0f));
            m.SetColumn(1, new Vector4(0f, (float)size, 0f, 0f));
            m.SetColumn(2, new Vector4(0f, 0f, (float)(size * (1.0 + phase01)), 0f));
            m.SetColumn(3, new Vector4((float)x, (float)y, (float)z, 1f));
            return m;
        }

        /// <summary>QA A/B switch (ARK D-043): with the environment variable NOC_TRAFFIC_OFF=1 the vehicle batches and the layer glows
        /// are built but not registered, so an A/B capture pair isolates exactly the traffic's light (the streak-length proof).</summary>
        public static readonly bool Off = Environment.GetEnvironmentVariable("NOC_TRAFFIC_OFF") == "1";

        /// <summary>One batch of every vehicle of the lanes (registered for per-camera submission).</summary>
        public static InstancedBatch Batch(string name, IList<TrafficLane> lanes, Material mat, Mesh laneQuad, uint seed)
        {
            var b = new InstancedBatch(name, laneQuad, mat, chunkSize: 100000f);
            var phis = new List<double>(256); var vs = new List<double>(256);
            for (int l = 0; l < lanes.Count; l++)
            {
                phis.Clear(); vs.Clear();
                Vehicles(lanes[l], seed + (uint)l * 7919u, phis, vs);
                for (int i = 0; i < phis.Count; i++) b.Add(VehicleMatrix(lanes[l], phis[i], vs[i]));
            }
            if (!Off) NocCameraBatches.Register(b);
            return b;
        }

        /// <summary>The ten boulevard lanes of 14.5.2 (z offset from 195, y): south (negative offset) east-bound, north west-bound,
        /// from x -4 000 to the portals of the notch gate in face B (the alpha ramp e L lies east of x_B = 815.3: through the lintel,
        /// the courtyard and into the east block). Heights: 14.5.2, with the four lanes below y 135 raised (GE, GB-06, see above).</summary>
        public static void BoulevardLanes(List<TrafficLane> eastBound, List<TrafficLane> westBound)
        {
            double[,] lanes = { { -48, 150 }, { -42, 175 }, { -52, 140 }, { -38, 200 }, { 44, 145 }, { 50, 185 }, { 40, 160 }, { 36, 230 }, { 0, 275 }, { 0, 290 } };   // ARK D-043: skyway 320 / 400 -> 275 / 290 (out of the Ark)
            for (int k = 0; k < lanes.GetLength(0); k++)
            {
                double z = 195.0 + lanes[k, 0], y = lanes[k, 1];
                double xB = CityField.GulfXB(z);
                double e = NocturneConstants.TrafficRampLane;
                double x1 = xB + e * (xB - NocturneConstants.TrafficLaneX0) / (1.0 - e);          // ramp starts exactly at the face
                bool east = lanes[k, 0] < 0 || (lanes[k, 0] == 0 && k == 8);                        // skyway 275 east, 290 west
                var lane = new TrafficLane { name = "boulevard_" + k, ay = y, by = y, az = z, bz = z, ramp = e };
                if (east) { lane.ax = NocturneConstants.TrafficLaneX0; lane.bx = x1; eastBound.Add(lane); }
                else { lane.ax = x1; lane.bx = NocturneConstants.TrafficLaneX0; westBound.Add(lane); }
            }
        }
    }
}
