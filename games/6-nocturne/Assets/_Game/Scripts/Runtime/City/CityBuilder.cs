// CityBuilder: the generic city of MASTER_PROMPT 14.7 H2 as an IBeatBuilder (discovered by GameBootstrap like every builder; its
// content lives under ctx.CityRoot, always active: 14.6 "the city is one persistent set"). Builds:
//   * CityField: every 240 m cell within 6.5 km of the ring centre, ring 1 (T1, 120 m towers) and ring 2 (T2 aggregates = the exact
//     union of the four ring-1 towers of the cell), one NocFacade mesh per cell with its chunk-local origin at the cell centre;
//   * aviation beacons (NocStreak points, AviationRed, 0.2 Hz) on 20 % of the towers within 4 km;
//   * the boulevard traffic of 14.5.2 as the H7 lane demo (ten lanes, red tails east-bound, cyan-white heads west-bound).
// Every instanced field is submitted per camera (NocCameraBatches). Tier 2 (-tier 2, graybox) still builds the field: the city
// facades and fog are part of every fallback tier (14.5 graybox lines). Logs counts and triangles ([NOC] city ...).
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Megastructure
{
    public sealed class CityBuilder : IBeatBuilder
    {
        public string Name => "City00_Field";

        /// <summary>Counts of the last build (CityTests reads the generator directly; QA reads these).</summary>
        public static int Towers1, Towers2, Cells, Faces, Culled, Beacons, Vehicles;
        public static long Triangles;

        public void Build(WorldContext ctx)
        {
            var lib = ctx.Materials;
            Material facade = lib != null ? lib.Facade : null;
            if (facade == null) { Debug.LogError("[NOC-FAIL] CityBuilder: MaterialLibrary.Facade missing (run RebuildAll)"); return; }
            var root = new GameObject("CityField").transform;
            root.SetParent(ctx.CityRoot, false);

            Towers1 = Towers2 = Cells = Faces = Culled = Beacons = Vehicles = 0;
            long tris0 = ctx.Triangles;
            CityField.CellRange(out int i0, out int i1, out int j0, out int j1);
            var towers = new List<CityTower>(4);
            var beaconTowers = new List<CityTower>(1024);
            var mb = new MeshBuilder();
            double cell = NocturneConstants.CityCell2;
            for (int I = i0; I <= i1; I++)
                for (int J = j0; J <= j1; J++)
                {
                    int ring = CityField.RingOf(I, J);
                    if (ring == 0) continue;
                    towers.Clear();
                    CityField.CellTowers(I, J, ring, towers);
                    if (towers.Count == 0) continue;
                    if (ring == 1) Towers1 += towers.Count; else Towers2 += towers.Count;
                    foreach (var t in towers) if (t.beacon) beaconTowers.Add(t);
                    double ox = (I + 0.5) * cell, oz = (J + 0.5) * cell;
                    mb.Clear();
                    int faces = CityMesher.Append(mb, towers, ox, oz, ref Culled);
                    if (faces == 0) continue;
                    Faces += faces;
                    Cells++;
                    Mesh mesh = mb.ToMesh(FormattableString.Invariant($"City_T{ring}_{I}_{J}"), false, true);
                    var mr = ctx.AddMesh(root, FormattableString.Invariant($"T{ring}_{I}_{J}"), mesh, facade);
                    mr.transform.localPosition = new Vector3((float)ox, 0f, (float)oz);
                }
            Triangles = ctx.Triangles - tris0;

            // aviation beacons: two opposite corners x three heights per beacon tower
            Material beacon = lib.Beacon;
            if (beacon != null && beaconTowers.Count > 0)
            {
                var b = new InstancedBatch("CityBeacons", NocTraffic.PointQuad(), beacon, chunkSize: 600f);
                foreach (var t in beaconTowers)
                {
                    double phase = NocHash.U01(t.seed ^ 0x4265616Bu);
                    for (int k = 0; k < 6; k++)
                    {
                        CityMesher.Beacon(t, k, out double x, out double y, out double z);
                        b.Add(NocTraffic.PointMatrix(x, y, z, NocturneConstants.CityBeaconSize, phase));
                        Beacons++;
                    }
                }
                NocCameraBatches.Register(b);
            }

            // H7 demo: the ten boulevard lanes
            if (lib.StreakTail != null && lib.StreakHead != null)
            {
                var east = new List<TrafficLane>(); var west = new List<TrafficLane>();
                NocTraffic.BoulevardLanes(east, west);
                Mesh quad = NocTraffic.LaneQuad();
                Vehicles += NocTraffic.Batch("BoulevardTails", east, lib.StreakTail, quad, 0x5441u).Count;
                Vehicles += NocTraffic.Batch("BoulevardHeads", west, lib.StreakHead, quad, 0x4845u).Count;
            }

            ctx.Log(FormattableString.Invariant(
                $"city: {Towers1} T1 towers + {Towers2} T2 towers in {Cells} cell meshes, {Faces} faces ({Culled} back faces for every camera skipped), {Triangles} triangles, {Beacons} beacons, {Vehicles} vehicles, {NocCameraBatches.Count} per-camera batches"));
        }
    }
}
