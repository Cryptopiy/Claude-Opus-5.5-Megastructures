// Beat07_Heart: the builder of beat 7, Heart Square (s 1 260 - 1 500; MASTER_PROMPT 14.5.7), one IBeatBuilder per beat folder
// (7.2, Part 3.1 rule 2). Phase-3 graybox: the heart floor (plaza circle R 400 and the floors between the promenade and the circle;
// HeartFloorBuilder), the spire with drums, fins and strips (SpireBuilder), the 22 ring towers (RingTowersBuilder), the Old Quarter
// with the goal window, its room, lamp, light and balcony (OldQuarterBuilder; replaces GrayboxKit.BuildGoal), the plaza lamps,
// bollards and end planter (PlazaFurnitureBuilder), and the graybox koi with its driver (KoiBuilder, KoiDriver; KoiMath by V).
// Dimensions: HeartLayout. Everything lives under BeatRoot(6) and stays active (14.6). No amber anywhere but the window (14.5.7).
namespace Megastructure
{
    public sealed class Beat07_Heart : IBeatBuilder
    {
        public string Name => "Beat07_Heart";

        public void Build(WorldContext ctx)
        {
            var root = ctx.BeatRoot(6);
            long tris0 = ctx.Triangles;
            HeartFloorBuilder.Build(ctx, root);
            SpireBuilder.Build(ctx, root);
            RingTowersBuilder.Build(ctx, root);
            OldQuarterBuilder.Build(ctx, root);
            PlazaFurnitureBuilder.Build(ctx, root);
            KoiBuilder.Build(ctx, root);
            ctx.Log(System.FormattableString.Invariant($"Beat07 built: {ctx.Triangles - tris0} triangles (14.5.7 budget 560 k)"));
        }
    }
}
