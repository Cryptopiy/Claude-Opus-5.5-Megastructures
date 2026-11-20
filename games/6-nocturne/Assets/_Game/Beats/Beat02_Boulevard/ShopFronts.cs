// ShopFronts: the shops of the boulevard arcades and the signs of its podium faces (owner BV, Phase 4 wave A; MASTER_PROMPT 14.5.2
// "6 000 shopfront signs (1-4 m)"; INT-22: the podium shopfronts read as rows of identical flat white squares, the brightest thing
// in KS03 / KS04; CRIT0 #15 / #24). One bay between two arcade columns (5.4 m, x = 3 + 6 k) is one shop:
//   * the shop opening (5.16 x 3.13 m above a 0.32 m stone riser) with its glass on the arcade back wall (z 116 / 274), drawn by
//     NocShopfront on the G1 shop slots (cold WinCold x 0.6, warm WinWarm x 0.9; x 0.6 room gain) with a per-bay STATE in the vertex
//     colour (NocShopfront's BV block): lit (gain 0.50-0.80), dim (0.14-0.28), a closed roller shutter (some leak a line of light,
//     some are perforated), a half-lowered shutter over a lit room, or a dark shop; never brighter than the lit room of 0.6 x;
//   * the shopfront frame (dark painted steel): jambs, head, a door with its mullion and transom, a glazing mullion; the shutter
//     coil box over every shuttered bay;
//   * the fascia band (y 3.62..4.42, 0.22 m proud) with the shop's name as a row of 3-6 NocNeon glyph cells (no real text; palette
//     per shop: SignWhite, WinNeutral, tungsten, amber, a little cyan and red, never magenta), on most open shops and a third of the
//     shuttered ones; a hanging blade sign (2 cells) under the arcade ceiling, perpendicular to the wall, on 45 % of the open shops;
//     a warm pendant lamp in front of every open shop (EmitLamp box on a rod);
//   * on the podium street faces (y 9..30) vertical projecting signs (3-6 cells, 0.75-1.05 m wide) on steel brackets at the column
//     lines, denser low down: the canyon of signs that recedes along the street in KS04 (facing along x, they show their faces to a
//     walker looking down the street).
//   * small signs (1-4 m; ShaftSigns): projecting vertical signs on brackets and flat light boxes on the shafts' street faces over the
//     terrace (y 43.5..165, densest low, e-folding 34 m, clear of the blades) and flat light boxes on the podium faces (y 10..37).
// Deterministic (PCG hashes of the bay index and side); every piece stays inside the arcade or within 1.4 m of the podium face, far
// from every lane and the tram sight sector. Look v2 accent rule: the signs are small, mostly white / warm (Beat02BoulevardTests).
using System.Collections.Generic;
using UnityEngine;

namespace Megastructure
{
    public static class ShopFronts
    {
        public enum ShopState { Lit = 0, Dim = 1, Shutter = 2, HalfShutter = 3, Dark = 4 }

        public struct Bay
        {
            public int Side, K;
            public float X0, X1;            // inner bay between the column faces (clipped to the arcade)
            public ShopState State;
            public float Gain;              // room light (x the slot's emission x 0.6) for the lit / dim / half states
            public bool Warm;
            public float Rnd;               // per-panel random (shutter paint, leak)
            public float ShutterFrac;       // half shutter: its bottom edge as a fraction of the opening height
            public int SignPalette;         // -1: no fascia sign
            public int Glyphs;
            public bool Blade, Pendant;
        }

        // ---- dimensions (m)
        public const float RiserH = 0.32f, OpenY1 = 3.45f, OpenInset = 0.12f, GlassOut = 0.04f, RiserOut = 0.10f;
        public const float FrameW = 0.09f, FrameD = 0.08f, MullionW = 0.06f, TransomY = 2.55f, DoorW = 1.05f;
        public const float FasciaY0 = 3.62f, FasciaY1 = 4.42f, FasciaOut = 0.22f;
        public const float GlyphW = 0.42f, GlyphH = 0.54f, GlyphGap = 0.03f;
        public const float BladeW = 0.5f, BladeH = 1.3f, BladeY = 6.3f, BladeOut = 1.0f;
        public const float PendantY = 6.95f, PendantOut = 2.1f;
        public const float PodiumSignY0 = 9.0f, PodiumSignY1 = 30.0f, PodiumSignOff = 0.35f;

        public static float BackZ(int side) => (float)(side < 0 ? BoulevardLayout.SouthZ - BoulevardLayout.ArcadeDepth : BoulevardLayout.NorthZ + BoulevardLayout.ArcadeDepth);
        public static float FaceZ(int side) => (float)(side < 0 ? BoulevardLayout.SouthZ : BoulevardLayout.NorthZ);
        /// <summary>Outward normal z of a side's walls (toward the street).</summary>
        public static float Nz(int side) => side < 0 ? 1f : -1f;

        static float U(uint h) => (float)NocMath.U01(h);

        /// <summary>The shops of a side (deterministic).</summary>
        public static List<Bay> Bays(int side)
        {
            var list = new List<Bay>(48);
            double arcX0 = -42.0, x1 = BoulevardLayout.ClosureX1;
            uint seed = side < 0 ? 0x5B0A1u : 0x5B0A2u;
            for (int k = BoulevardLayout.ColumnK0 - 1; k <= BoulevardLayout.ColumnK1; k++)
            {
                double bx0 = StreetFacade.ColumnX(k) + 0.5 * BoulevardLayout.ColumnW, bx1 = StreetFacade.ColumnX(k + 1) - 0.5 * BoulevardLayout.ColumnW;
                if (bx1 <= arcX0 || bx0 >= x1) continue;
                bx0 = System.Math.Max(bx0, arcX0); bx1 = System.Math.Min(bx1, x1 - 0.05);
                if (side < 0 && bx1 > -BoulevardLayout.MouthPierX && bx0 < BoulevardLayout.MouthPierX) continue;
                if (bx1 - bx0 < 3.0) continue;
                uint h = NocMath.Pcg((uint)(k + 4096) * 0x27D4EB2Fu ^ seed);
                float u0 = U(h), u1 = U(NocMath.Pcg(h + 1u)), u2 = U(NocMath.Pcg(h + 2u)), u3 = U(NocMath.Pcg(h + 3u)), u4 = U(NocMath.Pcg(h + 4u));
                var b = new Bay { Side = side, K = k, X0 = (float)bx0, X1 = (float)bx1, Rnd = u3, Warm = u2 < 0.36f };
                if (u0 < 0.30f) { b.State = ShopState.Lit; b.Gain = Mathf.Lerp(0.50f, 0.80f, u1); }
                else if (u0 < 0.48f) { b.State = ShopState.Dim; b.Gain = Mathf.Lerp(0.14f, 0.28f, u1); }
                else if (u0 < 0.80f) { b.State = ShopState.Shutter; b.Gain = Mathf.Lerp(0.40f, 0.70f, u1); }
                else if (u0 < 0.91f) { b.State = ShopState.HalfShutter; b.Gain = Mathf.Lerp(0.40f, 0.70f, u1); b.ShutterFrac = Mathf.Lerp(0.42f, 0.70f, u4); }
                else { b.State = ShopState.Dark; b.Gain = 0.5f; }
                bool open = b.State == ShopState.Lit || b.State == ShopState.Dim || b.State == ShopState.HalfShutter;
                float us = U(NocMath.Pcg(h + 5u));
                bool sign = open ? us < 0.85f : us < 0.33f;
                b.SignPalette = sign ? SignPalette(U(NocMath.Pcg(h + 6u))) : -1;
                b.Glyphs = 3 + (int)(U(NocMath.Pcg(h + 7u)) * 4f);
                b.Blade = open && U(NocMath.Pcg(h + 8u)) < 0.45f;
                b.Pendant = open;
                list.Add(b);
            }
            return list;
        }

        /// <summary>A shop sign palette (NocNeon fixed slots): SignWhite 4, WinNeutral 5, tungsten 7, amber 2, cyan 1, red 6; no
        /// magenta (0) and never the retired violet slot (3).</summary>
        public static int SignPalette(float u)
            => u < 0.24f ? 4 : (u < 0.46f ? 5 : (u < 0.64f ? 7 : (u < 0.84f ? 2 : (u < 0.95f ? 1 : 6))));

        public static void Build(G1Parts parts, int side)
        {
            string sn = side < 0 ? "S" : "N";
            float zb = BackZ(side), nz = Nz(side);
            var origin = new Vector3(85f, 2f, zb);
            var shopC = parts.Get("shopsCold" + sn, G1Mat.EmitShopCold, origin);
            var shopW = parts.Get("shopsWarm" + sn, G1Mat.EmitShopWarm, origin);
            var frame = parts.Get("shopFrames" + sn, G1Mat.SteelPainted, origin);
            var stone = parts.Get("shopRisers" + sn, G1Mat.StoneWall, origin);
            var lamp = parts.Get("shopPendants" + sn, G1Mat.EmitLamp, new Vector3(85f, 7f, zb));
            var rods = parts.Get("shopRods" + sn, G1Mat.LineRod, new Vector3(85f, 7f, zb));
            var pts = new List<Vector3>(2); var frames = new List<LoftFrame>(2); var sec = new CrossSection();
            Vector3 outN = new Vector3(0f, 0f, nz);
            Quaternion faceRot = side < 0 ? Quaternion.LookRotation(Vector3.back) : Quaternion.identity;   // NocNeon quads face local -Z
            Quaternion alongRot = Quaternion.LookRotation(Vector3.right);
            float Z(float d) => zb + nz * d;
            foreach (var b in Bays(side))
            {
                float a = b.X0 + OpenInset, e = b.X1 - OpenInset, y0 = RiserH, y1 = OpenY1;
                // stone riser under the opening, the fascia band over it
                G1Mesh.Chamfer(stone, new Vector3(b.X0, -0.05f, Mathf.Min(zb, Z(RiserOut))), new Vector3(b.X1, y0, Mathf.Max(zb, Z(RiserOut))), 0.015f);
                G1Mesh.Chamfer(frame, new Vector3(b.X0, FasciaY0, Mathf.Min(zb, Z(FasciaOut))), new Vector3(b.X1, FasciaY1, Mathf.Max(zb, Z(FasciaOut))), 0.02f);
                // the glass (room) with its state in the vertex colour (NocShopfront)
                var mb = b.Warm ? shopW : shopC;
                Color32 keep = mb.Color;
                mb.Color = PanelColour(b);
                G1Mesh.Quad(mb, new Vector3(a, y0, Z(GlassOut)), new Vector3(a, y1, Z(GlassOut)), new Vector3(e, y1, Z(GlassOut)), new Vector3(e, y0, Z(GlassOut)), outN);
                mb.Color = keep;
                // frame: jambs and head (all states), the shutter coil box, or door / mullion / transom
                Box(frame, new Vector3(a - 0.02f, y0, Z(GlassOut)), new Vector3(a + FrameW, y1, Z(GlassOut + FrameD)));
                Box(frame, new Vector3(e - FrameW, y0, Z(GlassOut)), new Vector3(e + 0.02f, y1, Z(GlassOut + FrameD)));
                Box(frame, new Vector3(a - 0.02f, y1 - 0.12f, Z(GlassOut)), new Vector3(e + 0.02f, y1 + 0.03f, Z(GlassOut + FrameD)));
                bool shut = b.State == ShopState.Shutter || b.State == ShopState.HalfShutter;
                if (shut)
                    G1Mesh.Chamfer(frame, new Vector3(a, y1 - 0.05f, Mathf.Min(Z(GlassOut), Z(0.36f))), new Vector3(e, y1 + 0.17f, Mathf.Max(Z(GlassOut), Z(0.36f))), 0.03f);
                if (b.State != ShopState.Shutter)
                {
                    bool doorLeft = (b.K & 1) == 0;
                    float xd = doorLeft ? a + FrameW + DoorW : e - FrameW - DoorW;
                    Box(frame, new Vector3(xd - 0.5f * MullionW, y0, Z(GlassOut)), new Vector3(xd + 0.5f * MullionW, y1, Z(GlassOut + FrameD)));
                    Box(frame, new Vector3(a, TransomY, Z(GlassOut)), new Vector3(e, TransomY + 0.07f, Z(GlassOut + FrameD * 0.8f)));
                    float gx0 = doorLeft ? xd : a, gx1 = doorLeft ? e : xd;          // the glazed part beside the door
                    if (gx1 - gx0 > 2.6f)
                    {
                        float xm = 0.5f * (gx0 + gx1);
                        Box(frame, new Vector3(xm - 0.5f * MullionW, y0, Z(GlassOut)), new Vector3(xm + 0.5f * MullionW, TransomY, Z(GlassOut + FrameD * 0.8f)));
                    }
                    // the door's push bar
                    float xp = doorLeft ? xd - 0.25f : xd + 0.25f;
                    Box(frame, new Vector3(xp - 0.02f, 0.95f, Z(GlassOut + 0.02f)), new Vector3(xp + 0.02f, 1.55f, Z(GlassOut + 0.07f)));
                }
                // the fascia sign: a row of glyph cells
                if (b.SignPalette >= 0)
                {
                    float wTot = b.Glyphs * GlyphW + (b.Glyphs - 1) * GlyphGap;
                    float xs0 = 0.5f * (b.X0 + b.X1) - 0.5f * wTot + (b.Rnd - 0.5f) * Mathf.Max(0f, (b.X1 - b.X0) - wTot - 0.6f);
                    for (int g = 0; g < b.Glyphs; g++)
                    {
                        float xc = xs0 + 0.5f * GlyphW + g * (GlyphW + GlyphGap);
                        parts.Sign(new Vector3(xc, 0.5f * (FasciaY0 + FasciaY1), Z(FasciaOut + 0.015f)), faceRot, GlyphW, GlyphH, b.SignPalette);
                    }
                }
                // the hanging blade sign under the arcade ceiling (perpendicular to the wall, beside the west column) and its rods
                if (b.Blade)
                {
                    float xb = b.X0 + 0.35f;
                    parts.Sign(new Vector3(xb, BladeY, Z(BladeOut)), alongRot, BladeW, BladeH, b.SignPalette >= 0 ? b.SignPalette : 4);
                    foreach (float dz in new[] { -0.18f, 0.18f })
                    {
                        pts.Clear();
                        pts.Add(new Vector3(xb, BladeY + 0.5f * BladeH, Z(BladeOut + dz)));
                        pts.Add(new Vector3(xb, (float)BoulevardLayout.ArcadeH, Z(BladeOut + dz)));
                        G1Mesh.Tube(rods, pts, 0.5f * G1Materials.Rod, 6, frames, sec);
                    }
                }
                // a warm pendant lamp in front of the shop (lit lens box on a rod from the arcade ceiling)
                if (b.Pendant)
                {
                    float xm = 0.5f * (b.X0 + b.X1);
                    G1Mesh.Chamfer(lamp, new Vector3(xm - 0.15f, PendantY - 0.2f, Z(PendantOut) - 0.15f), new Vector3(xm + 0.15f, PendantY + 0.2f, Z(PendantOut) + 0.15f), 0.02f);
                    pts.Clear();
                    pts.Add(new Vector3(xm, PendantY + 0.2f, Z(PendantOut)));
                    pts.Add(new Vector3(xm, (float)BoulevardLayout.ArcadeH, Z(PendantOut)));
                    G1Mesh.Tube(rods, pts, 0.5f * G1Materials.Rod, 6, frames, sec);
                }
            }
            PodiumSigns(parts, side);
            SmallSigns(parts, side);
        }

        /// <summary>The NocShopfront vertex colour of a bay (R random, G light reduction, B state x 32, A half-shutter edge).</summary>
        public static Color32 PanelColour(Bay b)
        {
            int state; float gain = b.Gain;
            switch (b.State)
            {
                case ShopState.Shutter: state = 1; break;
                case ShopState.HalfShutter: state = 2; break;
                case ShopState.Dark: state = 3; break;
                default: state = 0; break;
            }
            byte r = (byte)Mathf.Clamp(Mathf.RoundToInt(b.Rnd * 255f), 0, 255);
            byte g = (byte)Mathf.Clamp(Mathf.RoundToInt((1f - Mathf.Clamp01(gain)) * 255f), 0, 255);
            byte a = (byte)Mathf.Clamp(Mathf.RoundToInt(Mathf.Clamp01(state == 2 ? b.ShutterFrac : 1f) * 255f), 0, 255);
            return new Color32(r, g, (byte)(state * 32), a);
        }

        /// <summary>The podium-face projecting signs of a side (centre, width, height, palette); deterministic.</summary>
        public static List<Vector4> PodiumSignList(int side, List<int> palettes)
        {
            var list = new List<Vector4>(96);   // (x, y, w, h)
            uint seed = side < 0 ? 0x51C0u : 0x51C1u;
            for (int k = BoulevardLayout.ColumnK0; k <= BoulevardLayout.ColumnK1; k++)
            {
                float x = (float)StreetFacade.ColumnX(k);
                if (x < -36f || x > 210f) continue;
                if (side < 0 && x < (float)BoulevardLayout.SouthPodiumX0 + 2f) continue;   // south: the alley blocks' lower podia west of x 33
                for (int tier = 0; tier < 2; tier++)
                {
                    uint h = NocMath.Pcg((uint)(k + 512) * 0x9E3779B9u ^ seed ^ (uint)(tier * 0x68E31DA4));
                    if (U(h) > (tier == 0 ? 0.55f : 0.28f)) continue;
                    float w = Mathf.Lerp(0.75f, 1.05f, U(NocMath.Pcg(h + 1u)));
                    int cells = 3 + (int)(U(NocMath.Pcg(h + 2u)) * 4f);                 // 3-6 cells of aspect 14 x 18
                    float hh = cells * w * 18f / 14f;
                    float y0 = tier == 0 ? Mathf.Lerp(PodiumSignY0, 13f, U(NocMath.Pcg(h + 3u))) : Mathf.Lerp(17f, PodiumSignY1 - hh, U(NocMath.Pcg(h + 3u)));
                    list.Add(new Vector4(x, y0 + 0.5f * hh, w, hh));
                    palettes?.Add(SignPalette(U(NocMath.Pcg(h + 4u))));
                }
            }
            return list;
        }

        static void PodiumSigns(G1Parts parts, int side)
        {
            string sn = side < 0 ? "S" : "N";
            float zf = FaceZ(side), nz = Nz(side);
            var brk = parts.Get("podiumSignBrackets" + sn, G1Mat.SteelPainted, new Vector3(85f, 18f, zf));
            var pals = new List<int>(96);
            var signs = PodiumSignList(side, pals);
            Quaternion alongRot = Quaternion.LookRotation(Vector3.right), backRot = Quaternion.LookRotation(Vector3.left);   // west / east faces
            for (int i = 0; i < signs.Count; i++)
            {
                var s = signs[i];
                float zc = zf + nz * (PodiumSignOff + 0.5f * s.z);
                // glyph faces on both sides of the case (each its own glyphs: the seed is the position hash)
                parts.Sign(new Vector3(s.x - 0.045f, s.y, zc), alongRot, s.z, s.w, pals[i]);
                parts.Sign(new Vector3(s.x + 0.045f, s.y, zc), backRot, s.z, s.w, pals[i]);
                // the sign's dark case (7 cm thick, between the two glyph faces) and two brackets to the face
                float y0 = s.y - 0.5f * s.w, y1 = s.y + 0.5f * s.w;
                G1Mesh.Chamfer(brk, new Vector3(s.x - 0.035f, y0, Mathf.Min(zc - 0.5f * s.z, zc + 0.5f * s.z)), new Vector3(s.x + 0.035f, y1, Mathf.Max(zc - 0.5f * s.z, zc + 0.5f * s.z)), 0.01f);
                foreach (float yy in new[] { y0 + 0.25f, y1 - 0.25f })
                    G1Mesh.Chamfer(brk, new Vector3(s.x - 0.04f, yy - 0.04f, Mathf.Min(zf, zf + nz * PodiumSignOff)), new Vector3(s.x + 0.04f, yy + 0.04f, Mathf.Max(zf, zf + nz * PodiumSignOff)), 0.008f);
            }
        }

        // ---- the small signs of the shafts and the podium faces (14.5.2 "6 000 shopfront signs (1-4 m)"; BV wave A)
        public const float ShaftSignY0 = 43.5f, ShaftSignY1 = 165f, ShaftSignLevel = 4.2f, ShaftSignPitch = 3.0f;
        public const float ShaftProjP = 0.30f, ShaftFlatP = 0.22f, ShaftFold = 34f, PodiumFlatP = 0.16f;

        /// <summary>One small sign: centre, size (w along its plane, h), palette, kind (0 projecting, faces +-x; 1 flat on the face).</summary>
        public struct SmallSign { public Vector3 C; public float W, H; public int Palette, Kind; }

        /// <summary>The street face plane z of the shafts of a side (116 south, 274 north) and the slab x spans (xa, xb).</summary>
        public static float ShaftFaceZ(int side) => (float)(side < 0 ? BoulevardLayout.SouthShaftZ1 : BoulevardLayout.NorthShaftZ0);

        public static List<Vector2> SlabSpans(int side)
        {
            var l = new List<Vector2>(8);
            if (side > 0) l.Add(new Vector2((float)BoulevardLayout.NorthWestSlabX0, (float)BoulevardLayout.NorthWestSlabX1));
            for (int k = side < 0 ? BoulevardLayout.SouthFirstSlab : 0; k < BoulevardLayout.SlabCount; k++)
                l.Add(new Vector2((float)BoulevardLayout.SlabXa(k), (float)(k == BoulevardLayout.SlabCount - 1 ? BoulevardLayout.ClosureX1 : BoulevardLayout.SlabXb(k))));
            return l;
        }

        /// <summary>True where a sign of half-width hw (x) reaching up to yTop would stand behind a blade (the blade covers it from
        /// the street, and its brackets cross the gap at x +-4.5 m): the blade's x span +-7.5 m above its bottom - 1 m.</summary>
        public static bool BehindBlade(int side, float x, float hw, float yTop)
        {
            for (int i = 0; i < BladeSign.Count; i++)
            {
                BladeSign.Blade(i, out int bs, out double bx, out double _, out double by0);
                if (bs == side && Mathf.Abs(x - (float)bx) < 0.5f * (float)BoulevardLayout.BladeW + 0.5f + hw && yTop > (float)by0 - 1f) return true;
            }
            return false;
        }

        /// <summary>The small signs of a side's shaft faces (y 43.5..165, densest over the terrace, e-folding 34 m) and podium face
        /// (flat signs y 10..37 between the column lines); deterministic.</summary>
        public static List<SmallSign> ShaftSigns(int side)
        {
            var list = new List<SmallSign>(512);
            uint seed = side < 0 ? 0x5A1F7u : 0x5A1F8u;
            float zf = ShaftFaceZ(side), nz = Nz(side);
            foreach (var sp in SlabSpans(side))
            {
                int nx = Mathf.FloorToInt((sp.y - sp.x - 2f) / ShaftSignPitch);
                for (int ix = 0; ix < nx; ix++)
                {
                    float x = sp.x + 1f + (ix + 0.5f) * ShaftSignPitch;
                    for (int iy = 0; ; iy++)
                    {
                        float yl = ShaftSignY0 + iy * ShaftSignLevel;
                        if (yl > ShaftSignY1) break;
                        uint h = NocMath.Pcg((uint)Mathf.RoundToInt(x * 4f + 8192f) * 0x9E3779B9u ^ (uint)(iy * 0x85EBCA6B) ^ seed);
                        float u = U(h), fall = Mathf.Exp(-(yl - ShaftSignY0) / ShaftFold);
                        int pal = SignPalette(U(NocMath.Pcg(h + 1u)));
                        if (u < ShaftProjP * fall)
                        {
                            float w = Mathf.Lerp(0.7f, 1.3f, U(NocMath.Pcg(h + 2u)));
                            float hh = Mathf.Lerp(1.6f, 4.0f, U(NocMath.Pcg(h + 3u)));
                            if (BehindBlade(side, x, 0.1f, yl + hh)) continue;
                            list.Add(new SmallSign { C = new Vector3(x, yl + 0.5f * hh, zf + nz * (PodiumSignOff + 0.5f * w)), W = w, H = hh, Palette = pal, Kind = 0 });
                        }
                        else if (u < (ShaftProjP + ShaftFlatP) * fall)
                        {
                            float w = Mathf.Lerp(1.4f, 2.7f, U(NocMath.Pcg(h + 2u)));
                            float hh = Mathf.Lerp(0.8f, 1.8f, U(NocMath.Pcg(h + 3u)));
                            if (BehindBlade(side, x, 0.5f * w, yl + 0.4f + hh)) continue;
                            list.Add(new SmallSign { C = new Vector3(x, yl + 0.4f + 0.5f * hh, zf + nz * 0.07f), W = w, H = hh, Palette = pal, Kind = 1 });
                        }
                    }
                }
            }
            // the podium face: flat signs between the column lines (y 10..37), the south side only over its 40 m podium (x >= 33)
            float zp = FaceZ(side);
            for (int k = BoulevardLayout.ColumnK0; k < BoulevardLayout.ColumnK1; k++)
            {
                float x = (float)(StreetFacade.ColumnX(k) + 0.5 * BoulevardLayout.ColumnPitch);
                if (x < -36f || x > 210f) continue;
                if (side < 0 && x < (float)BoulevardLayout.SouthPodiumX0 + 2f) continue;
                for (int iy = 0; iy < 7; iy++)
                {
                    float yl = 10f + iy * ShaftSignLevel;
                    uint h = NocMath.Pcg((uint)(k + 1024) * 0x27D4EB2Fu ^ (uint)(iy * 0x165667B1) ^ seed ^ 0xF1A7u);
                    if (U(h) > PodiumFlatP) continue;
                    float w = Mathf.Lerp(1.6f, 3.6f, U(NocMath.Pcg(h + 2u)));
                    float hh = Mathf.Lerp(0.8f, 1.6f, U(NocMath.Pcg(h + 3u)));
                    list.Add(new SmallSign { C = new Vector3(x, yl + 0.5f + 0.5f * hh, zp + nz * 0.07f), W = w, H = hh, Palette = SignPalette(U(NocMath.Pcg(h + 1u))), Kind = 1 });
                }
            }
            return list;
        }

        static void SmallSigns(G1Parts parts, int side)
        {
            string sn = side < 0 ? "S" : "N";
            float nz = Nz(side);
            var brk = parts.Get("shaftSignCases" + sn, G1Mat.SteelPainted, new Vector3(85f, 60f, ShaftFaceZ(side)));
            Quaternion west = Quaternion.LookRotation(Vector3.right), east = Quaternion.LookRotation(Vector3.left);
            Quaternion face = side < 0 ? Quaternion.LookRotation(Vector3.back) : Quaternion.identity;
            foreach (var s in ShaftSigns(side))
            {
                float y0 = s.C.y - 0.5f * s.H, y1 = s.C.y + 0.5f * s.H;
                if (s.Kind == 0)
                {
                    float zw = s.C.z - nz * 0.5f * s.W;                 // the wall end of the sign
                    parts.Sign(new Vector3(s.C.x - 0.045f, s.C.y, s.C.z), west, s.W, s.H, s.Palette);
                    parts.Sign(new Vector3(s.C.x + 0.045f, s.C.y, s.C.z), east, s.W, s.H, s.Palette);
                    G1Mesh.Chamfer(brk, new Vector3(s.C.x - 0.035f, y0, Mathf.Min(s.C.z - 0.5f * s.W, s.C.z + 0.5f * s.W)), new Vector3(s.C.x + 0.035f, y1, Mathf.Max(s.C.z - 0.5f * s.W, s.C.z + 0.5f * s.W)), 0.01f);
                    float zwall = zw - nz * PodiumSignOff;
                    foreach (float yy in new[] { y0 + 0.2f, y1 - 0.2f })
                        G1Mesh.Chamfer(brk, new Vector3(s.C.x - 0.04f, yy - 0.04f, Mathf.Min(zwall, zw)), new Vector3(s.C.x + 0.04f, yy + 0.04f, Mathf.Max(zwall, zw)), 0.008f);
                }
                else
                {
                    // a light box: the glyph face 1 cm in front of a dark case 6 cm deep, 5 cm larger all round
                    parts.Sign(new Vector3(s.C.x, s.C.y, s.C.z + nz * 0.01f), face, s.W, s.H, s.Palette);
                    float zb = s.C.z - nz * 0.07f;
                    G1Mesh.Chamfer(brk, new Vector3(s.C.x - 0.5f * s.W - 0.05f, y0 - 0.05f, Mathf.Min(zb, s.C.z - nz * 0.01f)), new Vector3(s.C.x + 0.5f * s.W + 0.05f, y1 + 0.05f, Mathf.Max(zb, s.C.z - nz * 0.01f)), 0.01f);
                }
            }
        }

        static void Box(MeshBuilder mb, Vector3 a, Vector3 b) => G1Mesh.Chamfer(mb, Vector3.Min(a, b), Vector3.Max(a, b), 0.008f);
    }
}
