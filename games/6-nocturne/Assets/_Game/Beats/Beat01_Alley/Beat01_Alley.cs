// Beat01_Alley: beat 1 (Alley, s 0-120) at graybox-plus (MASTER_PROMPT Part 13 phase 3, 14.5.1; owner G1). One IBeatBuilder per
// beat folder (7.2, Part 3.1 rule 2). Builds, under ctx.BeatRoot(0) (always active, 14.6):
//   * the floor: a 1 m wet-concrete slab (lib.FloorFor(s): FloorAlley, NocWet) x -4..4 over z -2.5..116 and x -4.5..4.5 over
//     z 116..120 (the mouth, up to the arcade piers), top at y 0 (agreement 1: lane + 1 m everywhere, the walls stand in it);
//   * AlleyBlocks (walls on the lane profile, podia 22 / 31 m, dead-end wall and door, the towers above the podia),
//     NoodleBar (AL: a modelled noodle bar), VendingMachines, AlleyFixtures (catwalk, fire escapes, 140 cables, blade signs in
//     housings, 40 AC units, pipes, awnings, bins, steam vents, lamp fixtures, light pools, mouth light and haze), AlleyWindows (AL:
//     framed recessed windows with rooms), AlleyDoors (18 side doors with lit transoms), AlleyRooms (AL: the two empty rooms, DREAD 6);
//   * the wet-floor registry entries (14.7 H4, D-003): wall planes x -2.2 / +2.2 (s 0-120, flat dark concrete macro, tops at the
//     podia) and the sources of the bar window and sign, the vending screens, the blade signs and the mouth glow.
// The GrayboxKit floor and markers of the Phase-2 smoke builder are replaced (this floor covers the whole lane).
using System;
using UnityEngine;

namespace Megastructure
{
    public sealed class Beat01_Alley : IBeatBuilder
    {
        public string Name => "Beat01_Alley";

        /// <summary>Alley floor extents (14.4 surfaces: s 0-120 wet concrete).</summary>
        public const float FloorHalf = 4.0f, MouthFloorHalf = 4.5f, MouthFloorZ = 116f, FloorThickness = 1.0f;
        /// <summary>Wall macro of the wet mirror (linear, flat): the walls' mean radiance. Lit windows cover 45 % x (1.0 x 1.4) /
        /// (4.5 x 3.2) = 4.4 % of the wall at the window mix 0.6 WinWarm x 0.9 + 0.4 WinCold x 0.6 = (0.66, 0.46, 0.33), i.e.
        /// (0.029, 0.020, 0.014), plus the concrete lit by the violet trilight and the signs (~0.002 / 0.001 / 0.007): about
        /// 0.031 / 0.021 / 0.021 (r03 with 0.016 blue read as a brown floor).</summary>
        public static readonly Color WallMacro = new Color(0.031f, 0.021f, 0.021f, 1f);

        /// <summary>Everything of beat 1 as parts (EditMode tests call this without a scene).</summary>
        public static G1Parts BuildParts()
        {
            var parts = new G1Parts();
            Floor(parts);
            AlleyBlocks.Build(parts);
            NoodleBar.Build(parts);
            VendingMachines.Build(parts);
            AlleyFixtures.Build(parts);
            AlleyWindows.Build(parts);
            AlleyDoors.Build(parts);
            AlleyRooms.Build(parts);
            return parts;
        }

        static void Floor(G1Parts parts)
        {
            var origin = new Vector3(0f, 0f, 60f);
            var top = parts.Get("floor", G1Mat.FloorAlley, origin);
            var side = parts.Get("floorSides", G1Mat.Concrete, origin);
            float z0 = (float)AlleyLayout.DeadEndZ, zm = MouthFloorZ, z1 = (float)AlleyLayout.MouthZ, t = FloorThickness;
            G1Mesh.Box(top, new Vector3(-FloorHalf, -t, z0), new Vector3(FloorHalf, 0f, zm), G1Mesh.PY);
            G1Mesh.Box(top, new Vector3(-MouthFloorHalf, -t, zm), new Vector3(MouthFloorHalf, 0f, z1), G1Mesh.PY);
            G1Mesh.Box(side, new Vector3(-FloorHalf, -t, z0), new Vector3(FloorHalf, 0f, zm), G1Mesh.PX | G1Mesh.NX | G1Mesh.NY | G1Mesh.NZ);
            G1Mesh.Box(side, new Vector3(-MouthFloorHalf, -t, zm), new Vector3(MouthFloorHalf, 0f, z1), G1Mesh.PX | G1Mesh.NX | G1Mesh.NY);
        }

        /// <summary>The beat's NocWetRegistry entries (tests, after NocWetRegistry.Clear()): the alley's two walls and its sources.</summary>
        public static void RegisterWet()
        {
            RegisterWalls();
            RegisterSources();
        }

        /// <summary>The alley's two wall planes (kind 1, flat). At load they are registered by Beat02_Boulevard.Build AFTER the
        /// boulevard's two kind-0 walls (integration round, QA-F02): NocWetRegistry packs the active walls into slots in registration
        /// order and NocMirror.hlsl seeds a kind-0 wall's window pattern with its slot, so when the alley walls (registered first)
        /// faded out at s 132 the boulevard walls moved from slots 2-3 to 0-1 and their mirrored windows re-rolled in one frame
        /// (b12 f540, s 132.0). A kind-1 wall has no windows, so its own slot never shows (WetRegistryWalkTests).</summary>
        public static void RegisterWalls()
        {
            float half = 0.5f * (float)(AlleyLayout.MouthZ - AlleyLayout.DeadEndZ), cz = (float)AlleyLayout.DeadEndZ + half;
            // Look v2 (-surf-v 2, TEXTURE_PLAN 2.11): the walls' radiance under PA's rig at the middle of the alley, per facing side
            bool v2 = NocLookGlobals.SurfV == 2;
            double sm = 0.5 * (AlleyLayout.S0 + AlleyLayout.S1);
            Color west = v2 ? NocSurfaceMath.AlleyWallMacro(sm, 1.0) : WallMacro, east = v2 ? NocSurfaceMath.AlleyWallMacro(sm, -1.0) : WallMacro;
            NocWetRegistry.AddWall(AlleyLayout.S0, AlleyLayout.S1, -AlleyLayout.WetWallX, cz, 1f, 0f, half, (float)AlleyLayout.WestPodiumH, west, 1);
            NocWetRegistry.AddWall(AlleyLayout.S0, AlleyLayout.S1, AlleyLayout.WetWallX, cz, -1f, 0f, half, (float)AlleyLayout.EastPodiumH, east, 1);
        }

        /// <summary>The alley's wet sources (noodle bar, vending machines, fixtures).</summary>
        public static void RegisterSources()
        {
            NoodleBar.RegisterSources();
            VendingMachines.RegisterSources();
            AlleyFixtures.RegisterSources();
        }

        public void Build(WorldContext ctx)
        {
            Transform root = ctx.BeatRoot(0);
            long t0 = ctx.Triangles;
            var parts = BuildParts();
            int failed = parts.Commit(ctx, root, "Beat01");
            int src0 = NocWetRegistry.TotalSources;
            RegisterSources();          // the walls: Beat02_Boulevard.Build, after its own (QA-F02, see RegisterWalls)
            ctx.Log(
                FormattableString.Invariant($"beat 1 alley: {parts.Parts.Count} parts, {ctx.Triangles - t0} triangles (budget 480 000), {parts.Signs.Count} signs, {parts.Lights.Count} lights, ") +
                FormattableString.Invariant($"{NocWetRegistry.TotalSources - src0} wet sources + 2 walls, widths {AlleyLayout.ClearWidth(10):F2}/{AlleyLayout.ClearWidth(46):F2}/{AlleyLayout.ClearWidth(68):F2}/{AlleyLayout.ClearWidth(120):F2} m, ") +
                FormattableString.Invariant($"vending fronts x {VendingMachines.FrontX(0):F3}/{VendingMachines.FrontX(1):F3}/{VendingMachines.FrontX(2):F3}, ") +
                FormattableString.Invariant($"{AlleyWindows.Built} windows ({AlleyWindows.Lit} lit), {failed} invalid meshes"));
        }
    }
}
