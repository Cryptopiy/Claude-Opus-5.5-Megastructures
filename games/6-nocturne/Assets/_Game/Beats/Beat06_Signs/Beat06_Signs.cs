// Beat06_Signs: the builder of beat 6, the Wall of signs (s 1 080 - 1 260; MASTER_PROMPT 14.5.6), one IBeatBuilder per beat
// folder (7.2, Part 3.1 rule 2). SG Phase 4 wave A: the Full tier of 14.5.6 (ctx.Tier 0): the screen-wall slab with its LED
// screen face (three waves, imagery, panels, modules, pixels), the nine glyph signs and 12 wave-coloured wall lights
// (ScreenWallBuilder); the promenade floor, planters, lamp fixtures and bollards (PromenadeBuilder); the arcade block with its
// colonnade, shop rooms and lit back side (ArcadeBuilder); the shop rooms and signs of the sign-wall base (SignBaseRelief's bays,
// built here with the relief); and the per-frame SignsDriver. Simplified tier (-tier 1): one wave, no imagery, no glyph signs.
// The beat's walk from s 1 080 to the terrace end z 275 (s 1 153.56) lies on G3's garden terrace (geometry agreement 6).
// Dimensions: SignsLayout. Everything lives under BeatRoot(5) and stays active (14.6).
using System.Collections.Generic;
using UnityEngine;

namespace Megastructure
{
    public sealed class Beat06_Signs : IBeatBuilder
    {
        public string Name => "Beat06_Signs";

        public void Build(WorldContext ctx)
        {
            var root = ctx.BeatRoot(5);
            long tris0 = ctx.Triangles;
            int tier = ctx.Tier;
            ScreenWallBuilder.Build(ctx, root, tier, out var screen, out var wallLights);
            var cradles = ScreenWallBuilder.BuildService(ctx, root);
            PromenadeBuilder.Build(ctx, root);
            ArcadeBuilder.Build(ctx, root);
            if (ReliefBuilder.ReliefOn) BuildSignBaseShops(ctx, root);
            ctx.Register(new SignsDriver(screen, wallLights, SignsLayout.Waves(tier), cradles));
            ctx.Log(System.FormattableString.Invariant($"Beat06 built: {ctx.Triangles - tris0} triangles (14.5.6 budget 120 k), tier {tier}"));
        }

        /// <summary>The glass of the sign-wall base's shops (INT-34): NocShopfront rooms on the plane x 874.3 (facing +x, toward
        /// the promenade), 6-8 m deep into the slab, each with its panel state (G4Kit.PickShop: lit, dim, half shutter, dark; the
        /// closed bays of SignBaseRelief get roller shutters), and one small NocNeon blade under the canopy per open bay.</summary>
        public static MeshBuilder[] SignBaseRooms(List<ArcadeBuilder.Sign> signs)
        {
            var rooms = new[] { new MeshBuilder(), new MeshBuilder() };
            var o = ScreenWallBuilder.Origin;
            float x = (float)SignBaseRelief.ShopX + 0.01f, y0 = (float)SignBaseRelief.ShopY0, y1 = (float)SignBaseRelief.ShopY1;
            foreach (var b in SignBaseRelief.ShopBays())
            {
                float za = (float)b.za, zb = (float)b.zb;
                uint h = NocHash.H3(unchecked((uint)(za * 10.0f)), 0x53424153u, 5u, 0);
                bool closed = b.kind == 2;
                var st = G4Kit.PickShop(h, closed);
                var mb = rooms[closed ? (int)(NocHash.Pcg(h) % 2u) : b.kind];
                Color32 keep = mb.Color;
                mb.Color = G4Kit.ShopColour(st);
                G4Kit.ShopQuad(mb, new Vector3(x, y0, za) - o, new Vector3(x, y0, zb) - o, new Vector3(x, y1, zb) - o, new Vector3(x, y1, za) - o, Vector3.right);
                mb.Color = keep;
                if (!st.Open || signs == null) continue;
                int slot = (h % 4u) == 0u ? 1 : ((h % 4u) == 1u ? 5 : 4);
                signs.Add(new ArcadeBuilder.Sign { centre = SignBaseRelief.ShopSignCentre(b.za, b.zb), rotation = Quaternion.identity,
                    size = new Vector2((float)SignBaseRelief.ShopSignW, (float)SignBaseRelief.ShopSignH), palette = slot });
            }
            return rooms;
        }

        static void BuildSignBaseShops(WorldContext ctx, Transform root)
        {
            var lib = ctx.Materials;
            var signs = new List<ArcadeBuilder.Sign>(32);
            var rooms = SignBaseRooms(signs);
            Material[] m = lib == null ? new Material[2] : new[]
            {
                G4Kit.ShopRoom(lib, "Beat06_BaseShopCold", NocPal.WinCold, SignsLayout.ShopLight, 7.0f, 0.45f, 0.12f, 71),
                G4Kit.ShopRoom(lib, "Beat06_BaseShopNeutral", NocPal.WinNeutral, SignsLayout.ShopLight * 0.85, 8.0f, 0.5f, 0.14f, 72),
            };
            for (int i = 0; i < rooms.Length; i++) G4Kit.Add(ctx, root, "signBaseShops" + i, rooms[i], m[i], ScreenWallBuilder.Origin);
            int n = G4Kit.NeonSigns(lib, "Beat06_BaseSigns", signs);
            ctx.Log(System.FormattableString.Invariant($"Beat06 sign-wall base: {SignBaseRelief.ShopBays().Count} shop rooms, {n} shop signs"));
        }
    }
}
