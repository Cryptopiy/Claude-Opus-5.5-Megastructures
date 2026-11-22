// DreadCabins: elevators and gantries (docs/DREAD.md 2.2; owner DR). Scale through slowness:
//   - 32 lit cabins (3 x 3 x 3.5 m, seen as warm lit boxes: NocDreadLight cab profile 3.5 m, WinWarm / White) in eight glass shafts
//     on the planar parts of the Gulf faces (|z - 195| < 300, clear of the tongue root on face A and the garden notch on face B),
//     2 m proud of the face; a shaft runs from y -2 400 (deep in the haze: T < 0.003 from the walk) to y 600 (inside the deck: the
//     cloud mix hides every light above 560), 3 000 m: at 3-8 m/s a cabin needs 6-17 minutes for its shaft, so during the walk it
//     climbs or sinks a fraction of the face. Positions are functions of the traffic clock (_NocTraffic.x, wrapped mod 65 536 s) with
//     exact wraps: y = y0 + L frac(phase + v t / L) (down: y1 - ...), v quantised so that v 65 536 / L is an integer (the clock's
//     wrap is a whole number of shaft periods); the wrap itself happens at the hidden ends, and the light ramps to 0 over the last
//     2 % at both ends (no pop, Part 10 item 33). Computed in NocDreadLight's vertex stage (uv2 = speed, length, phase, direction):
//     no CPU work, one draw for all cabins.
//   - two maintenance gantry cranes on face B (a 50 m steel truss 6 m tall and 5 m deep, 3 m off the face, a hoist block with a lit
//     operator window, four steady LampWarm end lights) sliding along z: z(t) = z_c + A sin(2 pi t / P + phi), peak speed
//     2 pi A / P = 0.5 m/s (A 110 / 85 m), smooth turnarounds, never a wrap; each on its own rail (two 0.6 m bars), on the planar
//     parts of face B clear of the garden notch.
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Megastructure
{
    public static class DreadCabins
    {
        // the courses live in DreadCabinCourse (DreadCourse.cs: Mesh-free, read by the audio's machine hums; integration round)
        public const double Y0 = DreadCabinCourse.Y0, Y1 = DreadCabinCourse.Y1, ShaftL = DreadCabinCourse.ShaftL, Proud = DreadCabinCourse.Proud, ClockWrap = DreadCabinCourse.ClockWrap;
        public const float CabinSize = 3.5f, CabinGain = 3.0f;   // r02: 1.6 drowned among the face's windows
        /// <summary>NocDreadLight profile 2 (fix wave DR-07): a lit cab, a 3 x 3.5 m box with a dim frame and a ceiling-lit glass
        /// (the lamp blob read as one more window in stills).</summary>
        public const float CabProfile = 2f;

        /// <summary>DR-07: the shafts' guide lights, two steady rows a shaft (z +-GuideHalf about the cabins' line) every GuidePitch m
        /// from GuideY0 to GuideY1 (the deep haze to the deck), small cold lamps: a shaft reads as a lit ladder in a still.</summary>
        public const double GuideHalf = 2.4, GuidePitch = 12.0, GuideY0 = -720.0, GuideY1 = 540.0;
        public const float GuideSize = 0.7f, GuideGain = 0.9f;

        public static List<DreadLight> GuideLights()
        {
            int rows = (int)Math.Floor((GuideY1 - GuideY0) / GuidePitch) + 1;
            var list = new List<DreadLight>(Shafts.GetLength(0) * 2 * rows);
            for (int s = 0; s < Shafts.GetLength(0); s++)
                for (int side = -1; side <= 1; side += 2)
                    for (int k = 0; k < rows; k++)
                        list.Add(new DreadLight(new Vector3((float)Shafts[s, 0], (float)(GuideY0 + k * GuidePitch), (float)(Shafts[s, 1] + side * GuideHalf)),
                                                GuideSize, GuideGain, DreadLights.Cold));
            return list;
        }

        /// <summary>Shafts (face x of the cabin centre line, z): four on face A (facing +x), four on face B (facing -x).</summary>
        public static readonly double[,] Shafts = DreadCabinCourse.Shafts;
        public const int CabinsPerShaft = DreadCabinCourse.CabinsPerShaft;

        public static int CabinCount => DreadCabinCourse.CabinCount;

        /// <summary>Speed quantised so that the clock's wrap (65 536 s) is a whole number of shaft periods (exact wrap).</summary>
        public static double QuantiseSpeed(double v) => DreadCabinCourse.QuantiseSpeed(v);

        public struct Cabin
        {
            public double X, Z, Speed, Phase;
            public int Dir;      // +1 up, -1 down
            public int Colour;   // DreadLights colour index
        }

        /// <summary>The 32 cabins: speed 3-8 m/s and phase from a hash per (shaft, cabin), alternating up / down.</summary>
        public static List<Cabin> Cabins()
        {
            var list = new List<Cabin>(CabinCount);
            foreach (var k in DreadCabinCourse.Cabins())
                list.Add(new Cabin { X = k.X, Z = k.Z, Speed = k.Speed, Phase = k.Phase, Dir = k.Dir, Colour = k.Warm ? DreadLights.WinWarm : DreadLights.White });
            return list;
        }

        /// <summary>Twin of NocDreadLight's mover: the cabin's y at clock t (s), and its light ramp (0 at the hidden ends).</summary>
        public static double CabinY(Cabin c, double t, out double ramp)
            => DreadCabinCourse.CabinY(new DreadCabinCourse.Cabin { X = c.X, Z = c.Z, Speed = c.Speed, Phase = c.Phase, Dir = c.Dir }, t, out ramp);

        /// <summary>The cabins' light mesh: each light sits at its shaft's bottom (y0) and carries uv2 = (speed, length, phase, dir).</summary>
        public static Mesh CabinMesh()
        {
            var cab = Cabins();
            var pos = new List<Vector3>(4 * cab.Count);
            var nrm = new List<Vector3>(4 * cab.Count);
            var uv0 = new List<Vector2>(4 * cab.Count);
            var uv1 = new List<Vector4>(4 * cab.Count);
            var uv2 = new List<Vector4>(4 * cab.Count);
            var idx = new List<int>(6 * cab.Count);
            foreach (var c in cab)
            {
                int b = pos.Count;
                var p = new Vector3((float)c.X, (float)Y0, (float)c.Z);
                for (int k = 0; k < 4; k++)
                {
                    pos.Add(p); nrm.Add(Vector3.up);
                    uv0.Add(new Vector2(k == 1 || k == 2 ? 1f : 0f, k >= 2 ? 1f : 0f));
                    uv1.Add(new Vector4(CabinSize, CabinGain, c.Colour, CabProfile));
                    uv2.Add(new Vector4((float)c.Speed, (float)ShaftL, (float)c.Phase, c.Dir));
                }
                idx.Add(b); idx.Add(b + 2); idx.Add(b + 1);
                idx.Add(b); idx.Add(b + 3); idx.Add(b + 2);
            }
            var mesh = new Mesh { name = "Dread_Cabins" };
            mesh.SetVertices(pos); mesh.SetNormals(nrm);
            mesh.SetUVs(0, uv0); mesh.SetUVs(1, uv1); mesh.SetUVs(2, uv2);
            mesh.SetIndices(idx, MeshTopology.Triangles, 0, false);
            mesh.bounds = new Bounds(new Vector3(515.3f, (float)(0.5 * (Y0 + Y1)), 195f), new Vector3(700f, (float)(ShaftL + 100.0), 700f));
            return mesh;
        }

        // ------------------------------------------------------------------ gantries
        public const double GantryL = DreadCabinCourse.GantryL, GantryH = DreadCabinCourse.GantryH, GantryD = DreadCabinCourse.GantryD,
            GantryGap = DreadCabinCourse.GantryGap, GantrySpeed = DreadCabinCourse.GantrySpeed;
        /// <summary>Gantries: (y of the truss bottom, track centre z, phase rad, amplitude m) on the planar parts of face B: the north
        /// flank z 285..555 (north of the garden notch z 115..275) and the south flank z -110..110.</summary>
        public static readonly double[,] Gantries = DreadCabinCourse.Gantries;

        /// <summary>Period of gantry i: 2 pi A / 0.5 (peak speed 0.5 m/s).</summary>
        public static double GantryPeriod(int i) => DreadCabinCourse.GantryPeriod(i);
        public static double GantryZ(int i, double t) => DreadCabinCourse.GantryZ(i, t);

        /// <summary>A gantry in its local frame (origin at its centre on the face side; -x = away from face B, toward the rift).</summary>
        public static MeshBuilder GantryMesh()
        {
            var mb = new MeshBuilder();
            float l = (float)GantryL, h = (float)GantryH, d = (float)GantryD;
            mb.Color = new Color32(255, 0, 0, 255);
            // four chords along z, verticals every 5 m on both faces, a hoist block under the middle with its cable
            for (int a = 0; a < 2; a++)
                for (int b = 0; b < 2; b++)
                    mb.Box(new Vector3(-d * a, h * b, 0f), new Vector3(0.8f, 0.8f, l));
            for (int k = 0; k <= 10; k++)
            {
                float z = -0.5f * l + k * l / 10f;
                mb.Box(new Vector3(0f, 0.5f * h, z), new Vector3(0.6f, h, 0.6f));
                mb.Box(new Vector3(-d, 0.5f * h, z), new Vector3(0.6f, h, 0.6f));
            }
            for (int k = 0; k <= 2; k++) mb.Box(new Vector3(-0.5f * d, h, -0.5f * l + k * 0.5f * l), new Vector3(d, 0.6f, 0.6f));
            mb.Box(new Vector3(-0.5f * d, -14f, 6f), new Vector3(0.3f, 28f, 0.3f));
            mb.Box(new Vector3(-0.5f * d, -30f, 6f), new Vector3(4f, 4f, 4f));
            // the operator's lit window on the hoist block (vertex G = NocHull's emission mask), facing the rift (-x)
            mb.Color = new Color32(255, 255, 0, 255);
            float wx = -0.5f * d - 2.03f;
            HullBuilder.OutQuad(mb, new Vector3(wx, -31f, 4.8f), new Vector3(wx, -29f, 4.8f), new Vector3(wx, -29f, 7.2f), new Vector3(wx, -31f, 7.2f),
                                new Vector3(-0.5f * d, -30f, 6f));
            mb.Color = new Color32(255, 0, 0, 255);
            return mb;
        }

        /// <summary>The gantries' end lights (local frame): four steady LampWarm lamps.</summary>
        public static List<DreadLight> GantryLights()
        {
            float l = (float)GantryL, h = (float)GantryH, d = (float)GantryD;
            return new List<DreadLight>
            {
                new DreadLight(new Vector3(-d - 0.6f, h, -0.5f * l), 1.2f, 2.5f, DreadLights.Warm),
                new DreadLight(new Vector3(-d - 0.6f, h, 0.5f * l), 1.2f, 2.5f, DreadLights.Warm),
                new DreadLight(new Vector3(-d - 0.6f, 0f, -0.5f * l), 1.2f, 2.5f, DreadLights.Warm),
                new DreadLight(new Vector3(-d - 0.6f, 0f, 0.5f * l), 1.2f, 2.5f, DreadLights.Warm),
            };
        }

        /// <summary>The two rails on face B (world): bars 0.6 m at the gantry's top and bottom chord heights over its whole travel.</summary>
        public static MeshBuilder RailMesh()
        {
            var mb = new MeshBuilder();
            for (int i = 0; i < Gantries.GetLength(0); i++)
            {
                double y = Gantries[i, 0], zc = Gantries[i, 1];
                float len = (float)(2.0 * Gantries[i, 3] + GantryL + 10.0);
                float x = (float)(815.3 - 0.4);
                mb.Box(new Vector3(x, (float)y, (float)zc), new Vector3(0.8f, 0.6f, len));
                mb.Box(new Vector3(x, (float)(y + GantryH), (float)zc), new Vector3(0.8f, 0.6f, len));
            }
            return mb;
        }
    }
}
