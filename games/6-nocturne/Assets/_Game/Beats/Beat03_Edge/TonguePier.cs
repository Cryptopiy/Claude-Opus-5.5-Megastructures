// TonguePier: tongue A of MASTER_PROMPT 14.4 / 14.5.3 (TonguePier), Phase-3 graybox tier with the real materials: a plaza that
// funnels into a 6.4 m pier over the rift, x 215.3..375.3, half-width w(x) (EdgeDims.TongueHalfWidth; Acceptance (1)).
//   * deck slab: top y 0 (the H4 mirror plane, NocWet wet stone = lib.FloorFor(s), agreement 1), 1 m thick, 2 cm chamfer on the
//     walk edge, stone sides; it starts at x 215.3 where G1's street floor ends and ends at x 375.3 where G3's bridge deck starts
//     (agreements 2 and 3: no gap, no overlap, no end face at x 215.3);
//   * hull skirt: a ruled surface (NocFacade section mode) from the deck edges down to a keel 120 m deep at the root and 10 m at
//     the tip (the bridge's truss depth), keel half-width 0.4 w(x), a flat keel soffit, a stone tip cap;
//   * AB (Phase 4 wave A, D-032 1): the glass edge of the pier, x 338..371.95, lateral 1.75..w on both sides: a walk-on NocGlass
//     pane 3 cm below the top on a steel frame (edge girder, nosing, cross beams), the slab and the hull pulled in to 1.75 there
//     (EdgeDims.Ledge*): the eye looks through the floor past the hull into the rift (KS10's vertigo and drop share);
//   * glass balustrade 1.1 m on both edges from the face-A junction to x 371.95 (gate block A, G3, starts at x 372): a lofted steel
//     shoe, NocGlass panels (back / front materials), a NocLine top rail tube 0.06 m;
//   * on the plaza x 216-290 the visible barrier at the lane edge + 0.7 (planters 1.0 m with a moss top), 14 sodium lamps 5 m
//     (posts 0.9 m outside the planters, heads over the planters), 12 benches 0.45 m outside the barrier facing the drop;
//   * 14 balustrade lights (step-light fixtures on the shoe: steel housing, lens slit; AB INT-18) on the pier.
// Nothing stands inside the lane + 0.2 m camera sphere: planters at lane + 0.7, rail parts at w - 0.135 >= 2.43 m on the pier
// (lane 2.15). Positions relative to the chunk origin (295.3, 0, 195).
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Megastructure
{
    public static class TonguePier
    {
        public static readonly Vector3 Origin = new Vector3(295.3f, 0f, 195f);
        public const float Bevel = 0.02f;

        static Vector3 P(double x, double y, double z) => new Vector3((float)x, (float)y, (float)z);
        static double W(double x) => EdgeDims.TongueHalfWidth(Math.Max(EdgeDims.TongueX0, Math.Min(EdgeDims.TongueX1, x)));
        static double Wd(double x) => (W(x + 0.01) - W(x - 0.01)) / 0.02;

        /// <summary>Deck x samples: 1 m steps with the exact w(x) breaks.</summary>
        public static List<double> DeckSamples(double x0, double x1)
        {
            var list = new List<double>();
            int n = (int)Math.Ceiling((x1 - x0) / EdgeDims.DeckStep - 1e-9);
            for (int i = 0; i <= n; i++) list.Add(x0 + (x1 - x0) * i / n);
            foreach (double b in new[] { EdgeDims.TongueFunnelX0, EdgeDims.TongueFunnelX1, EdgeDims.TongueBowX0, EdgeDims.LedgeX0, EdgeDims.LedgeX1 })
                if (b > x0 && b < x1 && !list.Exists(v => Math.Abs(v - b) < 1e-6)) list.Add(b);
            list.Sort();
            return list;
        }

        /// <summary>Outward plan normal of the edge on side s (+1 north z = 195 + w, -1 south) at x.</summary>
        static Vector3 EdgeNormal(double x, int s)
        {
            var n = new Vector3((float)(-Wd(x)), 0f, s);
            return n.normalized;
        }

        /// <summary>LOOK_V2 6 beat 3: balustrade lights SignWhite r 10 i 2 (EdgeDims keeps the v1 2.5), tongue lamps LampWarm r 14 i 3.5
        /// (EdgeDims keeps the v1 4.0).</summary>
        public const float BalLightIntensityV2 = 2.0f, LampIntensityV2 = 3.5f;

        public static void Build(List<EdgePart> parts, List<EdgeLight> lights)
        {
            Deck(parts);
            Hull(parts);
            Balustrade(parts, lights);
            Plaza(parts, lights);
        }

        // ------------------------------------------------------------------ deck slab
        /// <summary>True when the deck interval [xa, xb] lies on the glass ledge (x 338..371.95, EdgeDims.Ledge*).</summary>
        static bool LedgeSpan(double xa, double xb) { double m = 0.5 * (xa + xb); return m > EdgeDims.LedgeX0 && m < EdgeDims.LedgeX1; }

        /// <summary>Lateral of the hull skirt's top line for the interval with midpoint `mid`: w(x), or LedgeIn from the ledge to the
        /// tip (the stone slab x 371.95..375.3 inside gate block A overhangs it; its soffit closes the gap).</summary>
        static double HullTop(double x, double mid) => mid > EdgeDims.LedgeX0 ? EdgeDims.LedgeIn : W(x);

        static void Deck(List<EdgePart> parts)
        {
            var top = new EdgePart("Tongue_deck", EdgeMat.Floor, Origin);
            var side = new EdgePart("Tongue_deckSides", EdgeMat.Stone, Origin);
            var glass = new EdgePart("Tongue_glassLedge", EdgeMat.Glass, Origin);
            var frame = new EdgePart("Tongue_ledgeFrame", EdgeMat.Steel, Origin);
            parts.Add(top); parts.Add(side); parts.Add(glass); parts.Add(frame);
            var xs = DeckSamples(EdgeDims.TongueX0, EdgeDims.TongueX1);
            double b = EdgeDims.DeckBevel, th = EdgeDims.DeckThickness, zc = EdgeDims.TongueZ, li = EdgeDims.LedgeIn;
            for (int i = 0; i < xs.Count - 1; i++)
            {
                double xa = xs[i], xb = xs[i + 1], wa = W(xa), wb = W(xb);
                if (LedgeSpan(xa, xb))
                {
                    // stone walk between the ledges, its inner side faces down to the slab soffit (the hull starts there)
                    EdgeMesh.FlatQuad(top.Mb, Origin, P(xa, 0, zc - li), P(xa, 0, zc + li), P(xb, 0, zc + li), P(xb, 0, zc - li), Vector3.up);
                    for (int s = -1; s <= 1; s += 2)
                    {
                        var n = new Vector3(0f, 0f, s);
                        EdgeMesh.FlatQuad(side.Mb, Origin, P(xa, 0, zc + s * li), P(xa, -th, zc + s * li), P(xb, -th, zc + s * li), P(xb, 0, zc + s * li), n);
                        // the glass pane from the nosing to under the girder's lip (the shoe sits on the girder)
                        double ga = wa - EdgeDims.LedgeGirderW + EdgeDims.LedgeGlassLip, gb = wb - EdgeDims.LedgeGirderW + EdgeDims.LedgeGlassLip;
                        double gy = EdgeDims.LedgeGlassY, ln = li + EdgeDims.LedgeNosing;
                        EdgeMesh.FlatQuad(glass.Mb, Origin, P(xa, gy, zc + s * ln), P(xa, gy, zc + s * ga), P(xb, gy, zc + s * gb), P(xb, gy, zc + s * ln), Vector3.up);
                    }
                    continue;
                }
                EdgeMesh.FlatQuad(top.Mb, Origin, P(xa, 0, zc - (wa - b)), P(xa, 0, zc + (wa - b)), P(xb, 0, zc + (wb - b)), P(xb, 0, zc - (wb - b)), Vector3.up);
                for (int s = -1; s <= 1; s += 2)
                {
                    Vector3 na = EdgeNormal(xa, s), nb = EdgeNormal(xb, s);
                    Vector3 ca = (na + Vector3.up).normalized, cb = (nb + Vector3.up).normalized;
                    EdgeMesh.Quad(side.Mb, Origin, P(xa, 0, zc + s * (wa - b)), P(xa, -b, zc + s * wa), P(xb, -b, zc + s * wb), P(xb, 0, zc + s * (wb - b)), ca, ca, cb, cb, ca + cb);
                    EdgeMesh.Quad(side.Mb, Origin, P(xa, -b, zc + s * wa), P(xa, -th, zc + s * wa), P(xb, -th, zc + s * wb), P(xb, -b, zc + s * wb), na, na, nb, nb, na + nb);
                    // the slab x 371.95..375.3 (inside gate block A) overhangs the hull pulled in to LedgeIn: its soffit
                    if (0.5 * (xa + xb) > EdgeDims.LedgeX1)
                        EdgeMesh.FlatQuad(side.Mb, Origin, P(xa, -th, zc + s * li), P(xa, -th, zc + s * wa), P(xb, -th, zc + s * wb), P(xb, -th, zc + s * li), Vector3.down);
                }
            }
            // the slab's end faces where the ledge opens (x 338, facing +x) and closes (x 371.95, facing -x), lateral LedgeIn..w
            foreach (double xe in new[] { EdgeDims.LedgeX0, EdgeDims.LedgeX1 })
            {
                double we = W(xe);
                var n = new Vector3(xe < 350.0 ? 1f : -1f, 0f, 0f);
                for (int s = -1; s <= 1; s += 2)
                {
                    var pent = new[] { P(xe, 0, zc + s * li), P(xe, 0, zc + s * (we - b)), P(xe, -b, zc + s * we), P(xe, -th, zc + s * we), P(xe, -th, zc + s * li) };
                    for (int k = 1; k < pent.Length - 1; k++) EdgeMesh.Tri(side.Mb, Origin, pent[0], pent[k], pent[k + 1], n);
                }
            }
            LedgeFrame(frame.Mb);
            // tip cap at x 375.3 (hexagon: chamfer corners included); G3's deck abuts it from x 375.3
            double xt = EdgeDims.TongueX1, wt = W(xt);
            var hex = new[] { P(xt, 0, zc - (wt - b)), P(xt, 0, zc + (wt - b)), P(xt, -b, zc + wt), P(xt, -th, zc + wt), P(xt, -th, zc - wt), P(xt, -b, zc - wt) };
            for (int k = 1; k < hex.Length - 1; k++) EdgeMesh.Tri(side.Mb, Origin, hex[0], hex[k], hex[k + 1], Vector3.right);
        }

        /// <summary>The glass ledge's steel (AB): per side an edge girder under the shoe (LedgeGirderW x LedgeGirderD, its top 5 mm into
        /// the shoe, the pane's LedgeGlassLip under it), a nosing on the stone edge (LedgeNosing wide, flush with the walk, down to
        /// the ribs) and cantilever ribs every LedgeBeamPitch under the pane. The girder is a prism swept along the deck samples
        /// with w(x) at both ends of each interval (exact edge, no steps), closed at x 338 and 371.95.</summary>
        static void LedgeFrame(MeshBuilder mb)
        {
            double zc = EdgeDims.TongueZ, li = EdgeDims.LedgeIn, gw = EdgeDims.LedgeGirderW, gd = EdgeDims.LedgeGirderD;
            double gy0 = -0.005, gy1 = -0.005 - gd;
            var xs = DeckSamples(EdgeDims.LedgeX0, EdgeDims.LedgeX1);
            for (int i = 0; i < xs.Count - 1; i++)
            {
                double xa = xs[i], xb = xs[i + 1], xm = 0.5 * (xa + xb), wa = W(xa), wb = W(xb), len = xb - xa + 0.002;
                for (int s = -1; s <= 1; s += 2)
                {
                    Vector3 ia0 = P(xa, gy0, zc + s * (wa - gw)), ib0 = P(xb, gy0, zc + s * (wb - gw)), ia1 = P(xa, gy1, zc + s * (wa - gw)), ib1 = P(xb, gy1, zc + s * (wb - gw));
                    Vector3 oa0 = P(xa, gy0, zc + s * wa), ob0 = P(xb, gy0, zc + s * wb), oa1 = P(xa, gy1, zc + s * wa), ob1 = P(xb, gy1, zc + s * wb);
                    EdgeMesh.FlatQuad(mb, Origin, ia0, ib0, ob0, oa0, Vector3.up);
                    EdgeMesh.FlatQuad(mb, Origin, ia1, ib1, ob1, oa1, Vector3.down);
                    EdgeMesh.FlatQuad(mb, Origin, ia0, ib0, ib1, ia1, new Vector3(0f, 0f, -s));
                    EdgeMesh.FlatQuad(mb, Origin, oa0, ob0, ob1, oa1, new Vector3(0f, 0f, s));
                    if (i == 0) EdgeMesh.FlatQuad(mb, Origin, ia0, oa0, oa1, ia1, Vector3.left);
                    if (i == xs.Count - 2) EdgeMesh.FlatQuad(mb, Origin, ib0, ob0, ob1, ib1, Vector3.right);
                    double nw = EdgeDims.LedgeNosing, nd = EdgeDims.LedgeBeamD + 0.05;
                    mb.Box(P(xm, -0.5 * nd, zc + s * (li + 0.5 * nw)) - Origin, new Vector3((float)len, (float)nd, (float)nw));
                }
            }
            for (double x = EdgeDims.LedgeX0 + 0.5 * EdgeDims.LedgeBeamPitch; x < EdgeDims.LedgeX1 - 0.5; x += EdgeDims.LedgeBeamPitch)
            {
                double w = W(x), lo = li + EdgeDims.LedgeNosing - 0.01, hi = w - gw + 0.01, bd = EdgeDims.LedgeBeamD;
                for (int s = -1; s <= 1; s += 2)
                    mb.Box(P(x, EdgeDims.LedgeGlassY - 0.02 - 0.5 * bd, zc + s * 0.5 * (lo + hi)) - Origin, new Vector3((float)EdgeDims.LedgeBeamW, (float)bd, (float)(hi - lo)));
            }
        }

        // ------------------------------------------------------------------ hull skirt
        static void Hull(List<EdgePart> parts)
        {
            var hull = new EdgePart("Tongue_hull", EdgeMat.Facade, Origin);
            var cap = new EdgePart("Tongue_hullTip", EdgeMat.Stone, Origin);
            parts.Add(hull); parts.Add(cap);
            var fw = new FacadeWriter(hull.Mb, Origin);
            double x0 = EdgeDims.TongueX0 - EdgeDims.HullRootInset, x1 = EdgeDims.TongueX1, zc = EdgeDims.TongueZ, th = EdgeDims.DeckThickness;
            var xs = DeckSamples(x0, x1);
            float uMin = (float)x0, uMax = (float)x1;
            for (int s = -1; s <= 1; s += 2)
            {
                uint id = s < 0 ? 0xC0A0u : 0xC0A1u;
                for (int i = 0; i < xs.Count - 1; i++)
                {
                    double xa = xs[i], xb = xs[i + 1], mid = 0.5 * (xa + xb);
                    Vector3 ta = P(xa, -th, zc + s * HullTop(xa, mid)), tb = P(xb, -th, zc + s * HullTop(xb, mid));
                    Vector3 ba = P(xa, -EdgeDims.HullDepth(xa), zc + s * EdgeDims.KeelHalfWidth(xa)), bb = P(xb, -EdgeDims.HullDepth(xb), zc + s * EdgeDims.KeelHalfWidth(xb));
                    Vector3 na = HullNormal(xa, s, mid), nb = HullNormal(xb, s, mid);
                    fw.Quad(ta, ba, bb, tb, na, na, nb, nb, (float)xa, (float)xa, (float)xb, (float)xb, id, FacadeWriter.ModeSection, uMin, uMax, na + nb);
                }
                // AB: where the hull steps in to LedgeIn (x 338), the end of the wider hull: a triangle (w, -1), (LedgeIn, -1), keel
                double xe = EdgeDims.LedgeX0;
                EdgeMesh.Tri(cap.Mb, Origin, P(xe, -th, zc + s * W(xe)), P(xe, -th, zc + s * EdgeDims.LedgeIn), P(xe, -EdgeDims.HullDepth(xe), zc + s * EdgeDims.KeelHalfWidth(xe)), Vector3.right);
            }
            // keel soffit (roof mode: no windows), facing down
            for (int i = 0; i < xs.Count - 1; i++)
            {
                double xa = xs[i], xb = xs[i + 1];
                double ya = -EdgeDims.HullDepth(xa), yb = -EdgeDims.HullDepth(xb), ka = EdgeDims.KeelHalfWidth(xa), kb = EdgeDims.KeelHalfWidth(xb);
                Vector3 a = P(xa, ya, zc - ka), bq = P(xa, ya, zc + ka), c = P(xb, yb, zc + kb), d = P(xb, yb, zc - kb);
                Vector3 n = Vector3.Cross(c - a, bq - a).normalized;
                if (n.y > 0f) n = -n;
                fw.Flat(a, bq, c, d, n, (float)xa, (float)xa, (float)xb, (float)xb, 0xC0A2u, FacadeWriter.ModeRoof, uMin, uMax);
            }
            // tip cap of the hull (trapezoid below the deck slab's end; the hull's top is at LedgeIn there)
            double xt = x1, wt = EdgeDims.LedgeIn, kt = EdgeDims.KeelHalfWidth(xt), ht = EdgeDims.HullDepth(xt);
            EdgeMesh.FlatQuad(cap.Mb, Origin, P(xt, -th, zc - wt), P(xt, -th, zc + wt), P(xt, -ht, zc + kt), P(xt, -ht, zc - kt), Vector3.right);
        }

        static Vector3 HullNormal(double x, int s, double mid)
        {
            double zc = EdgeDims.TongueZ, th = EdgeDims.DeckThickness;
            Vector3 t0 = P(x - 0.05, -th, zc + s * HullTop(x - 0.05, mid)), t1 = P(x + 0.05, -th, zc + s * HullTop(x + 0.05, mid));
            Vector3 top = P(x, -th, zc + s * HullTop(x, mid)), bot = P(x, -EdgeDims.HullDepth(x), zc + s * EdgeDims.KeelHalfWidth(x));
            Vector3 n = Vector3.Cross(t1 - t0, bot - top).normalized;
            if (n.z * s < 0f) n = -n;
            return n;
        }

        // ------------------------------------------------------------------ glass balustrade
        /// <summary>The rail centre line of side s (lateral w - 0.05), x 215.35..371.95, points <= 2 m apart in plan.</summary>
        public static List<Vector3> RailLine(int s, float y)
        {
            var pts = new List<Vector3>();
            double x = EdgeDims.TongueX0 + EdgeDims.RailInset, zc = EdgeDims.TongueZ;
            while (true)
            {
                pts.Add(P(x, y, zc + s * (W(x) - EdgeDims.RailInset)));
                if (x >= EdgeDims.RailEndX - 1e-6) break;
                double d = Wd(x), dx = EdgeDims.RailSegment / Math.Sqrt(1.0 + d * d);
                double dx2 = EdgeDims.RailSegment / Math.Sqrt(1.0 + Math.Pow(Wd(x + 0.5 * dx), 2));
                x = Math.Min(EdgeDims.RailEndX, x + Math.Min(dx, dx2));
            }
            return pts;
        }

        static void Balustrade(List<EdgePart> parts, List<EdgeLight> lights)
        {
            var shoe = new EdgePart("Tongue_railShoe", EdgeMat.Steel, Origin);
            var glass = new EdgePart("Tongue_railGlass", EdgeMat.Glass, Origin);
            var rail = new EdgePart("Tongue_railTop", EdgeMat.Rail, Origin);
            var puck = new EdgePart("Tongue_railLights", EdgeMat.BalLight, Origin);
            parts.Add(shoe); parts.Add(glass); parts.Add(rail); parts.Add(puck);
            var section = MeshKit.RoundedRectangle((float)EdgeDims.ShoeWidth, (float)EdgeDims.ShoeHeight, 0.01f, 1, null, default, true);
            var frames = new List<LoftFrame>();
            float shoeY = (float)(0.5 * EdgeDims.ShoeHeight - EdgeDims.ShoeSink);
            float railY = (float)(EdgeDims.RailHeight - 0.5 * EdgeDims.RailTube);
            for (int s = -1; s <= 1; s += 2)
            {
                var line = RailLine(s, shoeY);
                var local = new List<Vector3>(line.Count);
                foreach (var p in line) local.Add(p - Origin);
                frames.Clear();
                MeshKit.PathFrames(local, Vector3.up, frames, true);
                MeshKit.Loft(shoe.Mb, section, frames, true);
                // glass panels (one quad per segment; NocGlass draws back then front faces)
                for (int i = 0; i < line.Count - 1; i++)
                {
                    Vector3 a = line[i], b = line[i + 1];
                    Vector3 n = Vector3.Cross(Vector3.up, b - a).normalized;
                    if (n.z * s < 0f) n = -n;
                    EdgeMesh.FlatQuad(glass.Mb, Origin, P(a.x, EdgeDims.GlassBottom, a.z), P(a.x, EdgeDims.GlassTop, a.z), P(b.x, EdgeDims.GlassTop, b.z), P(b.x, EdgeDims.GlassBottom, b.z), n);
                }
                var top = RailLine(s, railY);
                EdgeMesh.Tube(rail.Mb, Origin, top, (float)(0.5 * EdgeDims.RailTube), 8, Vector3.up);

                // balustrade lights: WinCold pucks on the shoe's inner face (14 in all), each a realtime light and a wet source; the light sits
                // just above the puck (y 0.16, 0.2 m inside the edge): at y 0.5 / 0.4 m its specular image on the smooth glass floated as
                // a bright orb above the deck (G2 r01 look_l / look_r); here the image lies on the glass base right over the puck
                for (int k = 0; k < EdgeDims.BalustradeLights / 2; k++)
                {
                    double x = EdgeDims.BalustradeLightX0 + EdgeDims.BalustradeLightDx * k;
                    // AB (INT-18, "white blobs"): a step-light fixture instead of a glowing puck: a steel housing 0.20 x 0.07 x 0.045 on
                    // the shoe's inner face and a 0.13 x 0.012 lens slit at its bottom, 5 mm proud (the eye sees a lit slit in a dark
                    // housing; the visible emitting area is about 1 / 5 of the old puck's 0.16 x 0.05 face)
                    double lat = W(x) - EdgeDims.RailInset - 0.5 * EdgeDims.ShoeWidth - 0.0225;
                    Vector3 c = P(x, 0.07, EdgeDims.TongueZ + s * lat);
                    float yaw = Mathf.Atan2((float)(-s * Wd(x)), 1f) * Mathf.Rad2Deg;
                    var rot = Quaternion.Euler(0f, yaw, 0f);
                    MeshKit.ChamferBox(shoe.Mb, c - Origin, new Vector3(0.20f, 0.07f, 0.045f), 0.006f, rot);
                    Vector3 lens = P(x, 0.047, EdgeDims.TongueZ + s * (lat - 0.0225 - 0.002));
                    MeshKit.ChamferBox(puck.Mb, lens - Origin, new Vector3(0.13f, 0.012f, 0.006f), 0.001f, rot);
                    lights.Add(new EdgeLight
                    {
                        Pos = P(x, EdgeDims.BalustradeLightY, EdgeDims.TongueZ + s * (W(x) - 0.2)), Linear = NocturnePalette.Linear(NocPal.SignWhite),
                        Range = (float)EdgeDims.BalLightRange, Intensity = BalLightIntensityV2, Realtime = true,
                        Rho = 0.12f, Radiance = Beat03_Edge.BalWetGain,
                    });
                }
            }
        }

        // ------------------------------------------------------------------ plaza: planters, lamps, benches
        /// <summary>Centre line of the planter row on side s (barrier inner face + half the planter width), x 216..290.</summary>
        public static List<Vector3> BarrierLine(int s)
        {
            var pts = new List<Vector3>();
            for (double x = EdgeDims.BarrierX0; x <= EdgeDims.BarrierX1 + 1e-6; x += 0.25)
                pts.Add(P(x, 0, EdgeDims.TongueZ + s * (EdgeDims.BarrierInner(x) + 0.5 * EdgeDims.PlanterWidth)));
            return pts;
        }

        static void Plaza(List<EdgePart> parts, List<EdgeLight> lights)
        {
            var conc = new EdgePart("Tongue_planters", EdgeMat.Concrete, Origin);
            var moss = new EdgePart("Tongue_planterMoss", EdgeMat.Moss, Origin);
            var steel = new EdgePart("Tongue_furniture", EdgeMat.Steel, Origin);
            var heads = new EdgePart("Tongue_lampHeads", EdgeMat.LampHead, Origin);
            parts.Add(conc); parts.Add(moss); parts.Add(steel); parts.Add(heads);
            float h = (float)EdgeDims.PlanterHeight, len = (float)EdgeDims.PlanterLength, wid = (float)EdgeDims.PlanterWidth;
            for (int s = -1; s <= 1; s += 2)
            {
                var line = BarrierLine(s);
                // walk the line by arc length; one planter per pitch, centred on its chord
                double acc = 0.0, next = 0.5 * EdgeDims.PlanterPitch;
                for (int i = 0; i < line.Count - 1; i++)
                {
                    double seg = Vector3.Distance(line[i], line[i + 1]);
                    while (acc + seg >= next)
                    {
                        float t = (float)((next - acc) / seg);
                        Vector3 c = Vector3.Lerp(line[i], line[i + 1], t);
                        Vector3 dir = (line[Math.Min(line.Count - 1, i + 1)] - line[Math.Max(0, i)]).normalized;
                        // the planter must not reach past the row's end
                        if (c.x + 0.5f * len * Mathf.Abs(dir.x) > EdgeDims.BarrierX1 + 0.01) { next = double.MaxValue; break; }
                        var rot = Quaternion.LookRotation(dir, Vector3.up);
                        MeshKit.ChamferBox(conc.Mb, new Vector3(c.x, 0.5f * h - 0.01f, c.z) - Origin, new Vector3(wid, h + 0.02f, len), Bevel, rot);
                        MeshKit.ChamferBox(moss.Mb, new Vector3(c.x, h + 0.02f, c.z) - Origin, new Vector3(wid - 0.12f, 0.1f, len - 0.12f), 0.03f, rot);
                        next += EdgeDims.PlanterPitch;
                    }
                    acc += seg;
                }

                // 7 sodium lamps per side: post, arm toward the lane, head over the planter row
                for (int k = 0; k < EdgeDims.LampCount / 2; k++)
                {
                    double x = EdgeDims.LampX0 + EdgeDims.LampDx * k, z = EdgeDims.TongueZ + s * EdgeDims.LampLateral;
                    float H = (float)EdgeDims.LampHeight;
                    MeshKit.ChamferBox(steel.Mb, P(x, 0.5 * H - 0.01, z) - Origin, new Vector3(0.14f, H + 0.02f, 0.14f), Bevel);
                    double armEnd = EdgeDims.LampLateral - EdgeDims.LampArm;
                    MeshKit.ChamferBox(steel.Mb, P(x, H - 0.1, EdgeDims.TongueZ + s * (EdgeDims.LampLateral - 0.5 * EdgeDims.LampArm + 0.07)) - Origin,
                        new Vector3(0.08f, 0.08f, (float)EdgeDims.LampArm + 0.14f), 0.015f);
                    Vector3 head = P(x, H - 0.185, EdgeDims.TongueZ + s * (armEnd + 0.12));
                    MeshKit.ChamferBox(heads.Mb, head - Origin, new Vector3(0.26f, 0.1f, 0.5f), 0.015f);
                    NocHalos.Queue("TongueLamps", head, NocturnePalette.Linear(NocPal.LampWarm) * NocturnePalette.EmissionSign, EdgeDims.LampRange);   // LOOK_V2 6 halo
                    lights.Add(new EdgeLight
                    {
                        Pos = head + Vector3.down * 0.12f, Linear = NocturnePalette.Linear(NocPal.LampWarm),
                        Range = (float)EdgeDims.LampRange, Intensity = LampIntensityV2, Realtime = k * 2 >= EdgeDims.RealtimeLampFrom,
                        Rho = 0.35f, Radiance = NocturnePalette.EmissionSign,
                    });
                }

                // 6 benches per side outside the barrier, backs toward the lane (they face the drop)
                for (int k = 0; k < EdgeDims.BenchCount / 2; k++)
                {
                    double x = EdgeDims.BenchX0 + EdgeDims.BenchDx * k, zc = EdgeDims.TongueZ + s * EdgeDims.BenchLateral;
                    float L = (float)EdgeDims.BenchLength, D = (float)EdgeDims.BenchDepth, seat = (float)EdgeDims.BenchSeat;
                    MeshKit.ChamferBox(steel.Mb, P(x, seat - 0.03, zc) - Origin, new Vector3(L, 0.06f, D), 0.01f);
                    for (int l = -1; l <= 1; l += 2)
                        MeshKit.ChamferBox(steel.Mb, P(x + l * 0.75, 0.5 * (seat - 0.04) - 0.01, zc) - Origin, new Vector3(0.08f, seat - 0.04f + 0.02f, D - 0.06f), 0.01f);
                    MeshKit.ChamferBox(steel.Mb, P(x, seat + 0.22, zc - s * (0.5 * D - 0.03)) - Origin, new Vector3(L, 0.46f, 0.05f), 0.01f);
                }
            }
        }
    }
}
