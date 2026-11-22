// Beat02_Boulevard: beat 2 (Boulevard canyon, s 120-400) at the 14.5.2 graybox tier with every key-shot subject (Part 13 phase 3;
// owner G1). One IBeatBuilder per beat folder (7.2). Builds under ctx.BeatRoot(1) (always active, 14.6):
//   StreetLevel (street floor x -1 500..215.3, arcade floors, tram rails, buffer stop), StreetFacade (podia, arcades, columns,
//   shopfronts, slabs, closures), BladeSign (the twelve 14 x 200 m blades), MedianPromenade (bollards, barriers, 40 lamp posts,
//   kiosks, benches, planters), TramTerminus; then the wet-floor registry (14.7 H4, D-003): the street walls z 120 and z 270
//   (facade macro bands, tops 560, s 120-400) and the sources (blades, lamp heads).
// The traffic lanes are CityBuilder's (Runtime/City/NocTraffic, frozen). Phase 4 (BV): the tram T_A (TramSchedule, TramBuilder,
// TramDriver, Shaders/Beats/NocTram.shader) with its overhead line, the depot branch and portal. The GrayboxKit floor of the Phase-2
// smoke builder is replaced (this floor covers the whole lane of s 120-400, and the corridor colliders are PathGuide's).
using System;
using UnityEngine;

namespace Megastructure
{
    public sealed class Beat02_Boulevard : IBeatBuilder
    {
        public string Name => "Beat02_Boulevard";

        public static G1Parts BuildParts()
        {
            var parts = new G1Parts();
            StreetLevel.Build(parts);
            StreetFacade.Build(parts);
            BladeSign.Build(parts);
            MedianPromenade.Build(parts);
            TramTerminus.Build(parts);
            TramBuilder.BuildRoute(parts);      // BV: overhead line, depot branch rails, depot portal and tunnel
            return parts;
        }

        /// <summary>The beat's NocWetRegistry entries (load time; tests call it after NocWetRegistry.Clear()).</summary>
        public static void RegisterWet()
        {
            double s0 = BoulevardLayout.WetS0, s1 = BoulevardLayout.WetS1;
            // Look v2 (TEXTURE_PLAN 2.11): FA's facade mean radiance of the default region at the range's midpoint (TP-2: v2 only)
            Color macro = NocSurfaceMath.FacadeMacro(0, NocFacadeMath.RegionD, 0.5 * (s0 + s1));
            float sx0 = (float)BoulevardLayout.SouthX0, nx0 = (float)BoulevardLayout.NorthX0, x1 = (float)BoulevardLayout.FacadeX1;
            NocWetRegistry.AddWall(s0, s1, 0.5f * (sx0 + x1), (float)BoulevardLayout.SouthZ, 0f, 1f, 0.5f * (x1 - sx0), (float)BoulevardLayout.ShaftY1, macro, 0);
            NocWetRegistry.AddWall(s0, s1, 0.5f * (nx0 + x1), (float)BoulevardLayout.NorthZ, 0f, -1f, 0.5f * (x1 - nx0), (float)BoulevardLayout.ShaftY1, macro, 0);
            BladeSign.RegisterSources();
            MedianPromenade.RegisterSources();
            TramBuilder.RegisterSources();      // BV (fifth start): the overhead-line pole luminaires
        }

        public void Build(WorldContext ctx)
        {
            Transform root = ctx.BeatRoot(1);
            long t0 = ctx.Triangles;
            var parts = BuildParts();
            int failed = parts.Commit(ctx, root, "Beat02");
            int obstacles = MedianPromenade.AddColliders(root);     // BV fix wave (C15): the promenade furniture's box colliders
            // BV fix wave (DREAD 6): the terminus lobby's doors (their leaves and spill pool are committed parts)
            Transform lobbyA = root.Find("lobbyDoorA"), lobbyB = root.Find("lobbyDoorB"), lobbySpill = root.Find("lobbySpill");
            if (lobbyA != null && lobbyB != null) ctx.Register(new LobbyDoors(lobbyA, lobbyB, lobbySpill != null ? lobbySpill.GetComponent<Renderer>() : null));
            var tram = TramBuilder.Create(ctx, root, ref failed);   // BV: the tram T_A (14.5.2, H7) and its driver
            BoulevardLights.Create(ctx, root);                      // BV: lamps, blades, arcade soffits, terminus, depot (the 28 cap)
            int src0 = NocWetRegistry.TotalSources;
            RegisterWet();
            Beat01_Alley.RegisterWalls();   // after the boulevard's kind-0 walls: they keep their slots when the alley's fade out (QA-F02)
            int bollards = MedianPromenade.BollardPositions().Count;
            ctx.Log(
                FormattableString.Invariant($"beat 2 boulevard: {parts.Parts.Count} parts, {ctx.Triangles - t0} triangles (budget 520 000), {parts.Signs.Count} signs, {parts.Lights.Count} lights, ") +
                FormattableString.Invariant($"{NocWetRegistry.TotalSources - src0} wet sources + 2 walls, {bollards} bollards, {obstacles} promenade colliders, street {BoulevardLayout.NorthZ - BoulevardLayout.SouthZ:F1} m, ") +
                FormattableString.Invariant($"median {BoulevardLayout.MedianZ1 - BoulevardLayout.MedianZ0:F1} m, slab depth {BoulevardLayout.SlabDepth:F0} m, ") +
                FormattableString.Invariant($"tram {(tram != null ? "on" : "off (tier)")} ({TramSchedule.Length:F1} x {TramSchedule.Width:F1} x {TramSchedule.Height:F1} m, abeam s {TramSchedule.AbeamS(2.5):F2} / {TramSchedule.AbeamS(1.5):F2}), {failed} invalid meshes"));
        }
    }
}
