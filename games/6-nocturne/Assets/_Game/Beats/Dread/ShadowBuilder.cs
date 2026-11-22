// ShadowBuilder: the shadow below (docs/DREAD.md 3.1; owner DR). A body 900 m long, 162 m wide (plus two fin pairs) and 120 m tall
// in its local frame (origin = its centre; z along its length, +z = its tail: it swims south, toward -z), drawn by
// Megastructure/NocDreadBody: a dark, fogged, alpha-blended shell whose opacity rises over the outer few metres of its silhouette.
// Fix wave C42 (r03 read as cloud shading with stars): the plan is a body, not an oval - a fuller head at u 0 (the south end), a long
// tapering tail, a broad pair of fins at 0.28 of the length (reach 72 m) and a smaller pair at 0.70 (38 m), flattened toward their
// edges, and a dorsal crest (9 m) along the back that catches the haze light (NocDreadBody's sheen); normals from the surface itself
// (central differences), so the shading follows the fins and the crest. 61 rings x 24 points. Lights: seven dim cold Gaussian glows
// (sigma 1.6 m: 4-5 px at KS13, soft, never a star point) in one row along the crest, 90 m apart, steady. About 2.9 k triangles.
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Megastructure
{
    public static class ShadowBuilder
    {
        public const int Rings = 61, Seg = 24;
        public const float BackLightSigma = 1.6f, BackLightGain = 0.6f;
        public const double CrestH = 9.0;
        /// <summary>Fin pairs: (u centre, u sigma, reach m) - u 0 the head (south end), 1 the tail.</summary>
        public static readonly double[,] Fins = { { 0.28, 0.075, 72.0 }, { 0.70, 0.05, 38.0 } };

        /// <summary>Section scale f(u), u = 0 at the head (local z -450) .. 1 at the tail (z +450).</summary>
        public static double Profile(double u)
        {
            u = Math.Max(0.0, Math.Min(1.0, u));
            return Math.Pow(Math.Sin(Math.PI * u), 0.55) * (1.0 + 0.12 * (1.0 - u)) / 1.06;
        }

        /// <summary>Lateral reach of the fins at u (m, Gaussian in u per pair).</summary>
        public static double FinReach(double u)
        {
            double r = 0.0;
            for (int i = 0; i < Fins.GetLength(0); i++)
            {
                double d = (u - Fins[i, 0]) / Fins[i, 1];
                r += Fins[i, 2] * Math.Exp(-d * d);
            }
            return r;
        }

        /// <summary>Height of the dorsal crest at u (m).</summary>
        public static double Crest(double u) => CrestH * DreadClock.Smooth01(0.08, 0.2, u) * (1.0 - DreadClock.Smooth01(0.75, 0.92, u));

        /// <summary>The surface point at u (0..1 along the length) and section angle a (0 = +x, pi/2 = up).</summary>
        public static Vector3 Point(double u, double a)
        {
            double f = Profile(u);
            double w = ShadowCourse.HalfWidth * f * (1.0 + 0.07 * Math.Sin(5.0 * Math.PI * u + 0.7));
            double h = ShadowCourse.HalfHeight * f * (1.0 + 0.10 * Math.Cos(3.0 * Math.PI * u));
            double c = Math.Cos(a), s = Math.Sin(a);
            double ec = Math.Pow(Math.Abs(c), 8.0), F = FinReach(u);
            double x = w * c + Math.Sign(c) * F * ec;
            double y = h * s * (1.0 - 0.5 * ec * F / (F + 25.0));
            if (s > 0.0) y += Crest(u) * Math.Pow(s, 16.0);
            double z = -ShadowCourse.HalfLength + 2.0 * ShadowCourse.HalfLength * u;
            return new Vector3((float)x, (float)y, (float)z);
        }

        public static MeshBuilder BuildBody()
        {
            var mb = new MeshBuilder();
            double hl = ShadowCourse.HalfLength;
            // positions of the interior rings (k 1..Rings-2) and the two tips
            var P = new Vector3[Rings, Seg];
            for (int k = 1; k < Rings - 1; k++)
            {
                double u = (double)k / (Rings - 1);
                for (int i = 0; i < Seg; i++) P[k, i] = Point(u, 2.0 * Math.PI * (i + 0.5) / Seg);
            }
            var headP = new Vector3(0f, 0f, (float)-hl);
            var tailP = new Vector3(0f, 0f, (float)hl);
            Vector3 At(int k, int i) => k <= 0 ? headP : (k >= Rings - 1 ? tailP : P[k, ((i % Seg) + Seg) % Seg]);

            mb.Color = new Color32(0, 0, 0, 255);
            int head = mb.Vertex(headP, new Vector3(0f, 0f, -1f));
            int tail = mb.Vertex(tailP, new Vector3(0f, 0f, 1f));
            int[] prev = null;
            for (int k = 1; k < Rings - 1; k++)
            {
                double u = (double)k / (Rings - 1);
                // vertex R = the end fade: 0 at the tips, 1 from 8 % of the length at the head and 10 % at the tail (r03: 18 % / 6 %
                // dissolved the head into a cloud)
                double fe = DreadClock.Smooth01(0.0, 0.08, u) * DreadClock.Smooth01(0.0, 0.10, 1.0 - u);
                mb.Color = new Color32((byte)Math.Round(255.0 * fe), 0, 0, 255);
                var ring = new int[Seg];
                for (int i = 0; i < Seg; i++)
                {
                    Vector3 p = P[k, i];
                    Vector3 along = At(k + 1, i) - At(k - 1, i);
                    Vector3 around = At(k, i + 1) - At(k, i - 1);
                    Vector3 n = Vector3.Cross(around, along);
                    if (n.sqrMagnitude < 1e-12f) n = new Vector3(p.x, p.y, 0f);
                    n.Normalize();
                    if (Vector3.Dot(n, new Vector3(p.x, p.y, 0f)) < 0f) n = -n;
                    ring[i] = mb.Vertex(p, n);
                }
                if (prev == null)
                    for (int i = 0; i < Seg; i++) HullBuilder.TriO(mb, head, ring[(i + 1) % Seg], ring[i]);
                else
                    for (int i = 0; i < Seg; i++)
                    {
                        int j = (i + 1) % Seg;
                        HullBuilder.TriO(mb, prev[i], prev[j], ring[j]);
                        HullBuilder.TriO(mb, prev[i], ring[j], ring[i]);
                    }
                prev = ring;
            }
            for (int i = 0; i < Seg; i++) HullBuilder.TriO(mb, prev[i], prev[(i + 1) % Seg], tail);
            return mb;
        }

        /// <summary>Seven dim cold glows in one row along the crest (local z -270..270, every 90 m, 1.5 m above the back).</summary>
        public static List<DreadLight> BuildLights()
        {
            var list = new List<DreadLight>(8);
            for (int i = 0; i < 7; i++)
            {
                double z = -270.0 + 90.0 * i;
                double u = (z + ShadowCourse.HalfLength) / (2.0 * ShadowCourse.HalfLength);
                double y = Point(u, 0.5 * Math.PI).y + 1.5;
                list.Add(new DreadLight(new Vector3(0f, (float)y, (float)z), BackLightSigma, BackLightGain, DreadLights.Cold, true));
            }
            return list;
        }
    }
}
