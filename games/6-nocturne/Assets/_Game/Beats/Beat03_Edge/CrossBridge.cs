// CrossBridge: the six cross-gulf bridges of MASTER_PROMPT 14.5.3 at (z, y) (850, -180), (1 380, +60), (2 300, -600), (-640, +90),
// (-1 500, -300), (-2 600, -40): a 14 m deck slab (1.5 m) on two Warren trusses (8 m deep, ~25 m bays, 0.8 m box members, 12
// triangles each: the nearest bridge is 655 m from the walk, so no bevels), spanning face to face at its own height
// (EdgeDims.FrontX: terraces included) and reaching 2 m into both faces. About 5-8 k triangles per bridge (14.5.3: 8 k).
// AB (Phase 4 wave A, INT-01 / INT-19): the bridges read as lit structure at 700-2 000 m:
//   * deck lamps: a 0.22 m steel post 6 m tall every 25 m on both deck edges (alternating sides by half a pitch), its head a steady
//     LampWarm point (NocStreak kind 5, Beat03_Edge builds the batch from Lamps): rows of lamps instead of the Phase-3 white strip;
//   * the edge strips stay as a dim LED handrail line (NocLine tube r 0.3, Beat03_Edge.BridgeStripGain);
//   * a lit portal where the deck meets each face: a concrete surround (two jambs 3 m wide and a 3 m lintel, 4 m proud) round a
//     16 x 8 m opening whose back wall is a WinNeutral glow 0.3 m in front of the face plane, so the deck traffic (AbyssTraffic,
//     four lanes) drives out of light and its alpha ramps lie behind the face (H7: ramps inside portals);
//   * the vehicles on the deck (AbyssTraffic.BridgeLanes).
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Megastructure
{
    public static class CrossBridge
    {
        public const int LightSides = 8;
        public const double LightStep = 25.0;
        /// <summary>Deck lamps (AB): post height / section (m), pitch (m), lateral inset from the deck edge (m), head size (m).</summary>
        public const double LampPostH = 6.0, LampPostW = 0.22, LampPitch = 25.0, LampInset = 0.5, LampHeadSize = 0.6;
        /// <summary>Portal (AB): opening width / height, surround member width, depth proud of the face, glow offset (m).</summary>
        public const double PortalW = 16.0, PortalH = 8.0, PortalFrame = 3.0, PortalDepth = 4.0, PortalGlowOffset = 0.3;

        /// <summary>Lamp head centres of the last Build (world; Beat03_Edge turns them into one kind-5 batch).</summary>
        public static readonly List<Vector3> Lamps = new List<Vector3>(800);

        public static void Build(List<EdgePart> parts)
        {
            Lamps.Clear();
            for (int i = 0; i < EdgeDims.BridgeZ.Length; i++) BuildOne(i, parts);
        }

        static void BuildOne(int i, List<EdgePart> parts)
        {
            double z = EdgeDims.BridgeZ[i], y = EdgeDims.BridgeY[i];
            double fa = EdgeDims.FrontX(0, y, z), fb = EdgeDims.FrontX(1, y, z);
            double xa = fa - EdgeDims.BridgeEmbed, xb = fb + EdgeDims.BridgeEmbed;
            var origin = new Vector3((float)Math.Round(0.5 * (xa + xb)), (float)y, (float)z);
            string nm = FormattableString.Invariant($"CrossBridge_{(int)z}_{(int)y}");
            var deck = new EdgePart(nm, EdgeMat.Steel, origin);
            var lights = new EdgePart(nm + "_lights", EdgeMat.BridgeLight, origin);
            var portal = new EdgePart(nm + "_portal", EdgeMat.Concrete, origin);
            var glow = new EdgePart(nm + "_portalGlow", EdgeMat.TunnelMouth, origin);
            parts.Add(deck); parts.Add(lights); parts.Add(portal); parts.Add(glow);
            var mb = deck.Mb;
            float hw = (float)(0.5 * EdgeDims.BridgeWidth), len = (float)(xb - xa), cx = (float)(0.5 * (xa + xb));
            // deck slab
            mb.Box(new Vector3(cx, (float)(y - 0.5 * EdgeDims.BridgeDeck), (float)z) - origin, new Vector3(len, (float)EdgeDims.BridgeDeck, (float)EdgeDims.BridgeWidth));
            // two Warren trusses under the deck edges (the top chord reaches 0.1 m into the deck slab: no gap, no shared face)
            int bays = Math.Max(2, (int)Math.Round((xb - xa) / EdgeDims.BridgeBay));
            float m = (float)EdgeDims.BridgeMember;
            float yTop = (float)(y - EdgeDims.BridgeDeck + 0.1 - 0.5 * m), yBot = (float)(yTop - EdgeDims.BridgeTruss);
            for (int s = -1; s <= 1; s += 2)
            {
                float zz = (float)z + s * (hw - 0.6f);
                for (int b = 0; b < bays; b++)
                {
                    float x0 = (float)(xa + (xb - xa) * b / bays), x1 = (float)(xa + (xb - xa) * (b + 1) / bays);
                    var t0 = new Vector3(x0, yTop, zz); var t1 = new Vector3(x1, yTop, zz);
                    var b0 = new Vector3(x0, yBot, zz); var b1 = new Vector3(x1, yBot, zz);
                    EdgeMesh.Member(mb, origin, t0, t1, m, m, Vector3.forward);
                    EdgeMesh.Member(mb, origin, b0, b1, 0.9f * m, 0.9f * m, Vector3.forward);
                    if ((b & 1) == 0) EdgeMesh.Member(mb, origin, t0, b1, 0.7f * m, 0.7f * m, Vector3.forward);
                    else EdgeMesh.Member(mb, origin, b0, t1, 0.7f * m, 0.7f * m, Vector3.forward);
                }
                // vertical end posts just outside the faces (the chords' ends run 2 m into the face masses)
                EdgeMesh.Member(mb, origin, new Vector3((float)(xa + EdgeDims.BridgeEmbed) + m, yTop, zz), new Vector3((float)(xa + EdgeDims.BridgeEmbed) + m, yBot, zz), m, m, Vector3.forward);
                EdgeMesh.Member(mb, origin, new Vector3((float)(xb - EdgeDims.BridgeEmbed) - m, yTop, zz), new Vector3((float)(xb - EdgeDims.BridgeEmbed) - m, yBot, zz), m, m, Vector3.forward);
            }
            // light strips on both deck edges, 0.35 m above the deck (a dim LED handrail line)
            var pts = new List<Vector3>();
            int nl = Math.Max(2, (int)Math.Ceiling((xb - xa) / LightStep));
            for (int s = -1; s <= 1; s += 2)
            {
                pts.Clear();
                for (int k = 0; k <= nl; k++)
                    pts.Add(new Vector3((float)(xa + (xb - xa) * k / nl), (float)(y + 0.35), (float)z + s * (hw - 0.4f)));
                EdgeMesh.Tube(lights.Mb, origin, pts, (float)EdgeDims.BridgeLightR, LightSides, Vector3.up);
            }
            // deck lamps: posts on both edges between the portals, the south row offset by half a pitch
            float pw = (float)LampPostW, ph = (float)LampPostH;
            double clear = PortalDepth + 6.0;
            for (int s = -1; s <= 1; s += 2)
            {
                float zl = (float)(z + s * (0.5 * EdgeDims.BridgeWidth - LampInset));
                double x0 = fa + clear + (s > 0 ? 0.5 * LampPitch : 0.0);
                for (double x = x0; x <= fb - clear; x += LampPitch)
                {
                    mb.Box(new Vector3((float)x, (float)(y + 0.5 * ph - 0.05), zl) - origin, new Vector3(pw, ph + 0.1f, pw));
                    Lamps.Add(new Vector3((float)x, (float)(y + ph + 0.15), zl));
                }
            }
            // the two portals
            Portal(portal.Mb, glow.Mb, origin, fa, +1.0, y, z);
            Portal(portal.Mb, glow.Mb, origin, fb, -1.0, y, z);
        }

        /// <summary>A portal on the face plane x = xf whose rift side is +dir (face A: +1, face B: -1): concrete jambs and lintel
        /// PortalDepth proud round a PortalW x PortalH opening over the deck top, the glow wall PortalGlowOffset in front of the face
        /// (behind it the vehicles' ramps); the jambs stand on the deck slab's top (y) and run 0.5 m into the face.</summary>
        static void Portal(MeshBuilder conc, MeshBuilder glow, Vector3 origin, double xf, double dir, double y, double z)
        {
            double hwo = 0.5 * PortalW, f = PortalFrame;
            double x0 = xf - 0.5 * dir, x1 = xf + PortalDepth * dir;                  // the surround runs 0.5 m into the mass
            double xm = 0.5 * (x0 + x1), dx = Math.Abs(x1 - x0);
            // jambs (deck top to the lintel), lintel over the opening
            for (int s = -1; s <= 1; s += 2)
                conc.Box(new Vector3((float)xm, (float)(y + 0.5 * (PortalH + f)), (float)(z + s * (hwo + 0.5 * f))) - origin, new Vector3((float)dx, (float)(PortalH + f), (float)f));
            conc.Box(new Vector3((float)xm, (float)(y + PortalH + 0.5 * f), (float)z) - origin, new Vector3((float)dx, (float)f, (float)PortalW));
            // the glow wall across the opening (deck top + 0.02 to the lintel's soffit), facing the rift
            double xg = xf + PortalGlowOffset * dir;
            var n = new Vector3((float)dir, 0f, 0f);
            var a = new Vector3((float)xg, (float)(y + 0.02), (float)(z - hwo));
            var b = new Vector3((float)xg, (float)(y + PortalH), (float)(z - hwo));
            var c = new Vector3((float)xg, (float)(y + PortalH), (float)(z + hwo));
            var d = new Vector3((float)xg, (float)(y + 0.02), (float)(z + hwo));
            EdgeMesh.FlatQuad(glow, origin, a, b, c, d, n);
        }
    }
}
