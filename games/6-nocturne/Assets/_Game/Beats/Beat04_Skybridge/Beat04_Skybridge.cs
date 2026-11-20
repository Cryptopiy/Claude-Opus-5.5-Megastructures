// Beat04_Skybridge: the Phase-3 graybox of beat 4, Skybridge (s 560 - 880; MASTER_PROMPT 14.5.4, the "Graybox" tier plus every named
// key-shot subject of KS11-KS14 as a simple volume at the exact place and scale): the 5.0 m steel deck with its stud seams
// (SkybridgeDeck), the 1.10 m rails (SkybridgeRails), the 10 m Warren truss with outriggers and the tongue B bearing pier
// (SkybridgeTruss), gate blocks A and B with their walkway openings, tube mouths and gate frames (SkybridgeGate), the glass train
// tubes N/S and the face-to-face tubes upper/lower (SkybridgeTubes), and the 21 sodium lamps with their lights and wet sources
// (SkybridgeLamps). Dimensions: SkybridgeDims. No moving content yet (trains and the glass strip s 706-726 are Phase 4; the deck is
// continuous). Wet walls of the beat are Gulf faces A and B, registered by G2 over s 400-880 (G2-D6), so none here. Everything
// lives under the always-active beat root (14.6); meshes are validated at build; no per-frame work.
using System;
using UnityEngine;

namespace Megastructure
{
    public sealed class Beat04_Skybridge : IBeatBuilder
    {
        public const int BeatIndex = 3;
        public string Name => "Beat04_Skybridge";

        public void Build(WorldContext ctx)
        {
            Transform root = ctx.BeatRoot(BeatIndex);
            var mats = new SkybridgeMaterials(ctx.Materials);
            long t0 = ctx.Triangles;
            SkybridgeDeck.Build(ctx, root, mats);
            SkybridgeRails.Build(ctx, root, mats);
            SkybridgeTruss.Build(ctx, root, mats);
            SkybridgeGate.Build(ctx, root, mats, true);
            SkybridgeGate.Build(ctx, root, mats, false);
            SkybridgeTubes.Build(ctx, root, mats);
            SkybridgeLamps.Build(ctx, root, mats);
            ctx.Log(FormattableString.Invariant(
                $"Beat04_Skybridge: deck x {SkybridgeDims.DeckX0}-{SkybridgeDims.DeckX1}, span {SkybridgeTruss.Span:F2} m, {SkybridgeDims.LampCount} lamps, triangles {ctx.Triangles - t0}"));
        }
    }
}
