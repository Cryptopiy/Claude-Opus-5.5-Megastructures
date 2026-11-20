// TramTerminus: the terminus building x 205-215.3, z 212-270 (MASTER_PROMPT 14.5.2; the tram T_A of Phase 4 starts inside it on
// the north track z 227, x(tau) = 205 - 7 tau). Graybox: a 14 m stone hall (below every boulevard traffic lane: the lowest north
// lane is y 70), its west face with the tram portal (BV: 5.2 x 6.4 m, open into the hall; the tram's clip plane x 214.47 lies 2 cm
// behind the hall's east interior wall, TramSchedule), the south face toward
// the median (z 212, 5 m north of the median edge z 207: outside every lane) and the east face x 215.25 toward the Gulf (inside the
// canyon opening z 120..270 face A has no flank above y 0, so this face is seen from the Edge). The north side meets the north
// podium's street face (no face of its own there). A cold sign band over the portal (NocNeon cyan).
// GE, D-029 GB-20: the east face (seen from the Edge) and the south face (toward the median) read as a finished hall: a stone
// plinth, pilasters every 7.25 m, tall hall windows recessed 0.35 m in every bay with steel mullions and the lit hall behind them
// (BV, INT-35: a lit concrete hall with trusses, linear lamps and three dim lights instead of the EmitShopCold panes), a service
// door (with a steel leaf) to the tongue plaza, a 0.4 m cornice on all three free faces.
using System.Collections.Generic;
using UnityEngine;

namespace Megastructure
{
    public static class TramTerminus
    {
        public static void Build(G1Parts parts)
        {
            var origin = new Vector3(210f, 7f, 241f);
            var mb = parts.Get("terminus", G1Mat.StoneWall, origin);
            float x0 = (float)BoulevardLayout.TerminusX0, x1 = (float)BoulevardLayout.TerminusX1;
            float z0 = (float)BoulevardLayout.TerminusZ0, z1 = (float)BoulevardLayout.TerminusZ1, h = (float)BoulevardLayout.TerminusH;
            float pz0 = (float)(BoulevardLayout.TrackNorthZ - 0.5 * BoulevardLayout.PortalW), pz1 = (float)(BoulevardLayout.TrackNorthZ + 0.5 * BoulevardLayout.PortalW);
            float ph = (float)BoulevardLayout.PortalH, yb = -1f;
            float xin = x1 - EastRecess - Reveal, xw = x0 + WestWall, zs = z0 + Reveal, zn = z1 - 0.4f, yIn0 = 0.05f, yIn1 = h - 1.2f;
            // west face with the portal opening (BV: 5.2 x 6.4 m, the tram and its contact wire pass)
            G1Mesh.Quad(mb, new Vector3(x0, yb, z0), new Vector3(x0, h, z0), new Vector3(x0, h, pz0), new Vector3(x0, yb, pz0), Vector3.left);
            G1Mesh.Quad(mb, new Vector3(x0, yb, pz1), new Vector3(x0, h, pz1), new Vector3(x0, h, z1), new Vector3(x0, yb, z1), Vector3.left);
            G1Mesh.Quad(mb, new Vector3(x0, ph, pz0), new Vector3(x0, h, pz0), new Vector3(x0, h, pz1), new Vector3(x0, ph, pz1), Vector3.left);
            // portal reveals through the whole west wall into the hall (x 205..206.6)
            G1Mesh.Quad(mb, new Vector3(x0, yb, pz0), new Vector3(x0, ph, pz0), new Vector3(xw, ph, pz0), new Vector3(xw, yb, pz0), Vector3.forward);
            G1Mesh.Quad(mb, new Vector3(x0, yb, pz1), new Vector3(x0, ph, pz1), new Vector3(xw, ph, pz1), new Vector3(xw, yb, pz1), Vector3.back);
            G1Mesh.Quad(mb, new Vector3(x0, ph, pz0), new Vector3(xw, ph, pz0), new Vector3(xw, ph, pz1), new Vector3(x0, ph, pz1), Vector3.down);
            // BV (INT-35): the hall is a real lit room instead of flat emissive panes: concrete walls lit by three dim hall lights
            // (varied by their falloff), steel trusses under the ceiling at every bay line and rows of linear lamps (one in five
            // off), the stone floor with the tram track. Every interior face faces into the hall: from outside it shows only
            // through the openings.
            var inner = parts.Get("terminusInner", G1Mat.Concrete, origin);
            var trim = parts.Get("terminusTrim", G1Mat.SteelPainted, origin);
            var lamps = parts.Get("terminusLamps", G1Mat.EmitLamp, origin);
            // west interior wall with the portal hole, north wall, floor, ceiling
            G1Mesh.Quad(inner, new Vector3(xw, yIn0, zs), new Vector3(xw, yIn1, zs), new Vector3(xw, yIn1, pz0), new Vector3(xw, yIn0, pz0), Vector3.right);
            G1Mesh.Quad(inner, new Vector3(xw, yIn0, pz1), new Vector3(xw, yIn1, pz1), new Vector3(xw, yIn1, zn), new Vector3(xw, yIn0, zn), Vector3.right);
            G1Mesh.Quad(inner, new Vector3(xw, ph, pz0), new Vector3(xw, yIn1, pz0), new Vector3(xw, yIn1, pz1), new Vector3(xw, ph, pz1), Vector3.right);
            G1Mesh.Quad(inner, new Vector3(xw, yIn0, zn), new Vector3(xw, yIn1, zn), new Vector3(xin, yIn1, zn), new Vector3(xin, yIn0, zn), Vector3.back);
            G1Mesh.Quad(mb, new Vector3(xw, yIn0, zs), new Vector3(xin, yIn0, zs), new Vector3(xin, yIn0, zn), new Vector3(xw, yIn0, zn), Vector3.up);
            G1Mesh.Quad(inner, new Vector3(xw, yIn1, zs), new Vector3(xin, yIn1, zs), new Vector3(xin, yIn1, zn), new Vector3(xw, yIn1, zn), Vector3.down);
            // the east and south interior walls (at the reveals' inner ends) with the facade's openings
            float xe = x1 - EastRecess;
            InteriorWall(inner, xin, z0, z1, zs, zn, yIn0, yIn1, true);
            InteriorWall(inner, zs, x0, xe, xw, xin, yIn0, yIn1, false);
            // ceiling structure: a steel truss under the ceiling at every bay line, linear lamps between them (two rows)
            int bays = Mathf.Max(1, Mathf.RoundToInt((z1 - z0) / BayPitch));
            float pitch = (z1 - z0) / bays;
            for (int k = 1; k < bays; k++)
            {
                float zk = z0 + pitch * k;
                if (zk > zn - 0.3f) continue;
                MeshKit.ChamferMember(trim, new Vector3(xw + 0.05f, yIn1 - 0.35f, zk), new Vector3(xin - 0.05f, yIn1 - 0.35f, zk), 0.3f, 0.7f, 0.02f, Vector3.up);
            }
            for (int k = 0; k < bays; k++)
            {
                float zc = z0 + pitch * (k + 0.5f);
                if (zc > zn - 1.5f) continue;
                foreach (float xl in new[] { xw + 2.6f, xin - 2.6f })
                {
                    // a varied hall: about one fixture in five is off (a dark housing instead)
                    bool off = G1Mesh.U((uint)(k * 2 + (xl > 210f ? 1 : 0)) + 0x7E41u) < 0.2f;
                    G1Mesh.Chamfer(off ? trim : lamps, new Vector3(xl - 0.12f, yIn1 - 0.12f, zc - 1.6f), new Vector3(xl + 0.12f, yIn1 - 0.02f, zc + 1.6f), 0.01f);
                }
            }
            // the three dim hall lights (HallLights) are BoulevardLights' (the beat-2 light budget)
            // the service door's leaf (dark steel, set in its reveal)
            {
                var o = Openings(z0, z1, true);
                var d = o[o.Count - 1];
                G1Mesh.Chamfer(trim, new Vector3(xe - Reveal + 0.04f, 0f, d.x + 0.01f), new Vector3(xe - Reveal + 0.12f, d.w - 0.01f, d.y - 0.01f), 0.01f);
            }
            // south and east faces with recessed hall windows (GB-20) that look into the hall
            Facade(mb, trim, xe, z0, z1, +1, yb, h, true);      // east face x 214.8, facing +x (toward the Edge)
            Facade(mb, trim, z0, x0, xe, -1, yb, h, false);     // south face z 212, facing -z (toward the median)
            // cornice band (bevelled, 0.4 m proud on the west and south faces, to x 215.23 on the east) and the sign band over the portal
            G1Mesh.Chamfer(trim, new Vector3(x0 - 0.4f, h - 0.8f, z0 - 0.4f), new Vector3(x1 - 0.02f, h, z1 - 0.02f), 0.03f);
            parts.Sign(new Vector3(x0 - 0.06f, ph + 1.4f, (float)BoulevardLayout.TrackNorthZ), Quaternion.LookRotation(Vector3.right, Vector3.forward), 1.2f, 9f, 1);
        }

        /// <summary>The west wall's thickness (the portal reveals run x 205..206.6) and the hall lights' range.</summary>
        public const float WestWall = 1.6f, HallLightRange = 9f;
        /// <summary>The hall lights' window in camera x (BoulevardLights): seen down the canyon from x 40, from the Edge behind it.</summary>
        public const double LightX0 = 40.0, LightX1 = 380.0;

        /// <summary>The three dim hall lights (cool neutral) at different strengths, so the hall's walls are lit unevenly (INT-35).</summary>
        public static List<G1Light> HallLights()
        {
            float x0 = (float)BoulevardLayout.TerminusX0, x1 = (float)BoulevardLayout.TerminusX1;
            float z0 = (float)BoulevardLayout.TerminusZ0, z1 = (float)BoulevardLayout.TerminusZ1;
            float xin = x1 - EastRecess - Reveal, xw = x0 + WestWall, xm = 0.5f * (xw + xin);
            Color hallC = Color.Lerp(G1Materials.Lin(NocPal.WinNeutral, 1f), G1Materials.Lin(NocPal.WinCold, 1f), 0.4f);
            return new List<G1Light>
            {
                new G1Light { name = "terminusHall0", position = new Vector3(xm, 8.5f, z0 + 9f), colour = hallC, range = HallLightRange, intensity = 1.4f },
                new G1Light { name = "terminusHall1", position = new Vector3(xm, 8.5f, 0.5f * (z0 + z1)), colour = hallC, range = HallLightRange, intensity = 0.8f },
                new G1Light { name = "terminusHall2", position = new Vector3(xm, 8.5f, z1 - 9f), colour = hallC, range = HallLightRange, intensity = 1.1f },
            };
        }

        /// <summary>The openings of a facade plane (a0..a1): one window per bay, plus the service door on the east face (last).</summary>
        static List<Vector4> Openings(float a0, float a1, bool eastFace)
        {
            int bays = Mathf.Max(1, Mathf.RoundToInt((a1 - a0) / BayPitch));
            float pitch = (a1 - a0) / bays;
            var openings = new List<Vector4>();                      // (a0, a1, y0, y1)
            for (int k = 0; k < bays; k++)
            {
                float c0 = a0 + pitch * k, c1 = c0 + pitch;
                openings.Add(new Vector4(c0 + WinMargin, c1 - WinMargin, WinSill, WinHead));
            }
            if (eastFace)                                            // the service door onto the tongue plaza, second bay from the south
            {
                float c = a0 + 1.5f * pitch;
                openings.Add(new Vector4(c - 0.5f * DoorW, c + 0.5f * DoorW, 0f, DoorH));
            }
            return openings;
        }

        /// <summary>An interior wall at plane `fix` (x for the east wall facing -x, z for the south wall facing +z) over the facade
        /// extent [f0, f1] clipped to [c0, c1], y0..y1, with holes at the facade's openings.</summary>
        static void InteriorWall(MeshBuilder mb, float fix, float f0, float f1, float c0, float c1, float y0, float y1, bool eastFace)
        {
            var openings = Openings(f0, f1, eastFace);
            Vector3 n = eastFace ? Vector3.left : Vector3.forward;
            Vector3 P(float a, float y) => eastFace ? new Vector3(fix, y, a) : new Vector3(a, y, fix);
            var av = new List<float> { c0, c1 }; var yv = new List<float> { y0, y1 };
            foreach (var o in openings)
            {
                if (o.x > c0 && o.x < c1) av.Add(o.x);
                if (o.y > c0 && o.y < c1) av.Add(o.y);
                yv.Add(Mathf.Clamp(o.z, y0, y1)); yv.Add(Mathf.Clamp(o.w, y0, y1));
            }
            av.Sort(); yv.Sort();
            for (int i = 0; i + 1 < av.Count; i++)
            {
                if (av[i + 1] - av[i] < 1e-4f) continue;
                for (int j = 0; j + 1 < yv.Count; j++)
                {
                    if (yv[j + 1] - yv[j] < 1e-4f) continue;
                    float ac = 0.5f * (av[i] + av[i + 1]), yc = 0.5f * (yv[j] + yv[j + 1]);
                    bool open = false;
                    foreach (var o in openings) if (ac > o.x && ac < o.y && yc > o.z && yc < o.w) { open = true; break; }
                    if (open) continue;
                    G1Mesh.Quad(mb, P(av[i], yv[j]), P(av[i], yv[j + 1]), P(av[i + 1], yv[j + 1]), P(av[i + 1], yv[j]), n);
                }
            }
        }

        /// <summary>East / south facade bays (GB-20): pilaster pitch, window sill / head, reveal depth, plinth and door sizes.</summary>
        public const float BayPitch = 7.25f, PilasterW = 0.6f, PilasterProud = 0.25f, WinSill = 4.5f, WinHead = 11.4f, WinMargin = 1.1f;
        public const float Reveal = 0.35f, PlinthH = 0.8f, PlinthProud = 0.12f, DoorW = 2.6f, DoorH = 3.2f, MullionW = 0.12f;
        /// <summary>The east wall's recess behind x 215.25 (pilasters 0.25 and the cornice end inside x 215.28: Beat02BoulevardTests).</summary>
        public const float EastRecess = 0.45f;

        /// <summary>
        /// One facade plane: `fix` is the plane coordinate (x for the east face, z for the south face), [a0, a1] its horizontal
        /// extent, `sgn` the outward direction along the plane normal. The wall is cut on the conformal grid of the window and door
        /// openings; each opening has four reveals and a lit back pane (the hall); pilasters, the plinth and mullions are chamfered.
        /// </summary>
        static void Facade(MeshBuilder wall, MeshBuilder trim, float fix, float a0, float a1, int sgn, float yb, float h, bool eastFace)
        {
            Vector3 n = eastFace ? new Vector3(sgn, 0f, 0f) : new Vector3(0f, 0f, sgn);
            Vector3 P(float a, float y, float d) => eastFace ? new Vector3(fix + sgn * d, y, a) : new Vector3(a, y, fix + sgn * d);
            int bays = Mathf.Max(1, Mathf.RoundToInt((a1 - a0) / BayPitch));
            float pitch = (a1 - a0) / bays;
            var openings = Openings(a0, a1, eastFace);
            // the wall grid (cells inside an opening skipped)
            var av = new List<float> { a0, a1 }; var yv = new List<float> { yb, h };
            foreach (var o in openings) { av.Add(o.x); av.Add(o.y); yv.Add(o.z); yv.Add(o.w); }
            av.Sort(); yv.Sort();
            for (int i = 0; i + 1 < av.Count; i++)
            {
                if (av[i + 1] - av[i] < 1e-4f) continue;
                for (int j = 0; j + 1 < yv.Count; j++)
                {
                    if (yv[j + 1] - yv[j] < 1e-4f) continue;
                    float ac = 0.5f * (av[i] + av[i + 1]), yc = 0.5f * (yv[j] + yv[j + 1]);
                    bool open = false;
                    foreach (var o in openings) if (ac > o.x && ac < o.y && yc > o.z && yc < o.w) { open = true; break; }
                    if (open) continue;
                    G1Mesh.Quad(wall, P(av[i], yv[j], 0f), P(av[i], yv[j + 1], 0f), P(av[i + 1], yv[j + 1], 0f), P(av[i + 1], yv[j], 0f), n);
                }
            }
            // reveals (facing into the opening) and the lit back pane
            Vector3 lo = eastFace ? Vector3.forward : Vector3.right, hi = -lo;
            foreach (var o in openings)
            {
                G1Mesh.Quad(wall, P(o.x, o.z, 0f), P(o.x, o.w, 0f), P(o.x, o.w, -Reveal), P(o.x, o.z, -Reveal), lo);
                G1Mesh.Quad(wall, P(o.y, o.z, 0f), P(o.y, o.w, 0f), P(o.y, o.w, -Reveal), P(o.y, o.z, -Reveal), hi);
                G1Mesh.Quad(wall, P(o.x, o.w, 0f), P(o.y, o.w, 0f), P(o.y, o.w, -Reveal), P(o.x, o.w, -Reveal), Vector3.down);
                if (o.z > 0.01f) G1Mesh.Quad(wall, P(o.x, o.z, 0f), P(o.y, o.z, 0f), P(o.y, o.z, -Reveal), P(o.x, o.z, -Reveal), Vector3.up);
                if (o.z > 0.01f)                                      // window mullions: two vertical, one transom (steel, in the reveal)
                {
                    float w3 = (o.y - o.x) / 3f;
                    for (int m = 1; m <= 2; m++)
                    {
                        float am = o.x + w3 * m;
                        Box(trim, P(am - 0.5f * MullionW, o.z, -Reveal + 0.02f), P(am + 0.5f * MullionW, o.w, -0.12f));
                    }
                    float yt = o.z + 0.62f * (o.w - o.z);
                    Box(trim, P(o.x, yt - 0.5f * MullionW, -Reveal + 0.02f), P(o.y, yt + 0.5f * MullionW, -0.13f));
                }
            }
            // pilasters (between the bays and at the free corner), from the plinth to the cornice; the plinth (sunk 5 cm below y 0)
            for (int k = 0; k <= bays; k++)
            {
                if (eastFace && k == bays) continue;                 // the north end meets the north podium
                float ak = a0 + pitch * k;
                // corner pilasters wrap the corner (no face on the neighbouring wall's plane)
                float pa = ak - 0.5f * PilasterW, pb = ak + 0.5f * PilasterW;
                Box(wall, P(pa, PlinthH - 0.02f, -0.1f), P(pb, h - 0.78f, PilasterProud));
            }
            // the plinth, interrupted at the door; the south face's plinth sits 1.5 cm higher / lower (no shared plane at the corner)
            float ptop = PlinthH + (eastFace ? 0f : 0.015f), pbot = eastFace ? -0.05f : -0.065f, from = a0;
            foreach (var o in openings)
            {
                if (o.z > 0.01f) continue;
                Box(wall, P(from, pbot, -0.1f), P(o.x - 0.02f, ptop, PlinthProud));
                from = o.y + 0.02f;
            }
            Box(wall, P(from, pbot, -0.1f), P(a1, ptop, PlinthProud));
        }

        /// <summary>A chamfered box between two corners given in any order.</summary>
        static void Box(MeshBuilder mb, Vector3 a, Vector3 b) => G1Mesh.Chamfer(mb, Vector3.Min(a, b), Vector3.Max(a, b), 0.02f);
    }
}
