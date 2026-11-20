// BoulevardLights: the realtime local lights of beat 2 and their budget (owner BV, Phase 4 wave A; MASTER_PROMPT 14.2.3 row 2
// "28 lights", LOOK_V2 6 row 2 and its reading of the caps: "lights enabled at once, nearest first").
//   * the 40 median lamps as DOWNWARD SPOT lights under their lenses (LampWarm; LOOK_V2's r 14 i 3.5 point light 1.2 m under a 12 m
//     head put 1.2 % of its 1 m irradiance on the street: the pools never showed, INT-12 / KS04's black lower third; a 12 m street
//     lamp throws a pool of about its height in radius: spot 150 deg (inner 80), r 26, i LampIntensity, so the paving under every
//     head reads as a lit, wet pool and the lamp does not wash the facades above it);
//   * the 12 blade lights (r 40 i 4, lerp(c, WinNeutral, 0.4); BladeSign keeps their colours and positions);
//   * 8 arcade soffit lights (WinNeutral r 11 i 2.5) 0.4 m inside the column line under the arcade ceiling: they graze the columns,
//     the shop frames and the fascias (INT-23: the podium base near the eye was black);
//   * the terminus hall's three lights (TramTerminus) and the depot's bulkhead light (TramBuilder).
// Each light has a window in the camera's x (its "relevance": ahead of the walker, a little behind) with 16 m smootherstep ramps,
// so a light fades in and out over 6 s of walking where its pool is far and small; it is disabled at weight 0. With the tram's
// three moving lights (TramDriver, on while the tram is visible: camera x 45-172) at most Cap lights are non-zero at any camera x
// (Beat02BoulevardTests.LightBudget). No allocation per frame.
using System.Collections.Generic;
using UnityEngine;

namespace Megastructure
{
    public static class BoulevardLights
    {
        public const int Cap = 28;
        public const double Ramp = 16.0;
        /// <summary>The median lamps' spots (LampWarm): range, intensity, outer / inner cone (deg), the source 0.55 m under the head.</summary>
        public const float LampRange = 26f, LampIntensity = 18f, LampSpot = 150f, LampSpotInner = 80f, LampDrop = 0.55f;
        public const double LampAhead = 52.0, LampBehind = 14.0;
        public const double BladeAhead = 48.0, BladeBehind = 14.0;
        public const float SoffitRange = 11f, SoffitIntensity = 2.5f, SoffitY = 6.8f, SoffitIn = 0.4f;
        public const double SoffitAhead = 110.0, SoffitBehind = 10.0;
        /// <summary>The soffit lights: (x, side) beside lit bays in the KS03 / KS04 / KS06 views.</summary>
        public static readonly Vector2[] Soffits =
        {
            new Vector2(9.7f, 1f), new Vector2(45.7f, 1f), new Vector2(141.7f, 1f), new Vector2(177.7f, 1f),
            new Vector2(99.7f, -1f), new Vector2(135.7f, -1f), new Vector2(171.7f, -1f), new Vector2(195.7f, -1f),
        };
        /// <summary>The tram's moving lights are on while it can be seen: camera x 45.3 (launch, s 230) .. 171.1 (hidden at 2.5 m/s).</summary>
        public const double TramX0 = 45.3, TramX1 = 171.2;
        public const int TramLights = 3;

        public struct Spec
        {
            public string Name;
            public Vector3 Position;
            public Color Colour;
            public float Range, Intensity;
            public bool Spot;
            public double X0, X1;           // on window in camera x (full weight inside, Ramp outside)
        }

        /// <summary>Weight of a light for a camera at x (smootherstep ramps of Ramp m outside [x0, x1]).</summary>
        public static double Weight(double x0, double x1, double xc)
            => NocMath.Smootherstep(x0 - Ramp, x0, xc) * (1.0 - NocMath.Smootherstep(x1, x1 + Ramp, xc));

        /// <summary>Every managed light of beat 2 (deterministic).</summary>
        public static List<Spec> Specs()
        {
            var l = new List<Spec>(72);
            float H = (float)BoulevardLayout.LampH;
            Color lamp = G1Materials.Lin(NocPal.LampWarm, 1f);
            foreach (var p in MedianPromenade.LampPositions(MedianPromenade.BollardPositions()))
            {
                float zi = p.z - p.y * (float)BoulevardLayout.LampArm;
                l.Add(new Spec { Name = "lampSpot" + l.Count, Position = new Vector3(p.x, H - MedianPromenade.LensDrop - LampDrop, zi), Colour = lamp,
                    Range = LampRange, Intensity = LampIntensity, Spot = true, X0 = p.x - LampAhead, X1 = p.x + LampBehind });
            }
            for (int i = 0; i < BladeSign.Count; i++)
            {
                BladeSign.Blade(i, out int side, out double x, out double zf, out double y0);
                l.Add(new Spec { Name = "bladeLight" + i, Position = new Vector3((float)x, (float)y0 + BladeSign.LightUp, (float)zf - side * BladeSign.LightOut),
                    Colour = BladeSign.LightColour(i), Range = BladeSign.LightRange, Intensity = BladeSign.LightIntensity, X0 = x - BladeAhead, X1 = x + BladeBehind });
            }
            Color soffit = G1Materials.Lin(NocPal.WinNeutral, 1f);
            for (int i = 0; i < Soffits.Length; i++)
            {
                int side = Soffits[i].y < 0f ? -1 : 1;
                float zf = ShopFronts.FaceZ(side), nz = ShopFronts.Nz(side);
                l.Add(new Spec { Name = "arcadeSoffit" + i, Position = new Vector3(Soffits[i].x, SoffitY, zf - nz * SoffitIn), Colour = soffit,
                    Range = SoffitRange, Intensity = SoffitIntensity, X0 = Soffits[i].x - SoffitAhead, X1 = Soffits[i].x + SoffitBehind });
            }
            foreach (var t in TramTerminus.HallLights())
                l.Add(new Spec { Name = t.name, Position = t.position, Colour = t.colour, Range = t.range, Intensity = t.intensity, X0 = TramTerminus.LightX0, X1 = TramTerminus.LightX1 });
            var dl = TramBuilder.DepotLight();
            l.Add(new Spec { Name = dl.name, Position = dl.position, Colour = dl.colour, Range = dl.range, Intensity = dl.intensity, X0 = TramBuilder.DepotLightX0, X1 = TramBuilder.DepotLightX1 });
            return l;
        }

        /// <summary>Lights with a non-zero weight for a camera at x, the tram's included.</summary>
        public static int ActiveCount(List<Spec> specs, double xc)
        {
            int n = 0;
            foreach (var s in specs) if (Weight(s.X0, s.X1, xc) > 0.0) n++;
            if (xc > TramX0 - 1e-9 && xc < TramX1 + 1e-9) n += TramLights;
            return n;
        }

        /// <summary>Creates the lights (disabled until the driver weighs them) and registers the driver.</summary>
        public static void Create(WorldContext ctx, Transform root)
        {
            var specs = Specs();
            var lights = new Light[specs.Count];
            for (int i = 0; i < specs.Count; i++)
            {
                var s = specs[i];
                var go = new GameObject(s.Name);
                go.transform.SetParent(root, false);
                go.transform.position = s.Position;
                var li = go.AddComponent<Light>();
                li.type = s.Spot ? LightType.Spot : LightType.Point;
                if (s.Spot)
                {
                    go.transform.rotation = Quaternion.LookRotation(Vector3.down, Vector3.forward);
                    li.spotAngle = LampSpot;
                    li.innerSpotAngle = LampSpotInner;
                }
                li.color = s.Colour;
                li.range = s.Range;
                li.intensity = 0f;
                li.shadows = LightShadows.None;
                li.renderMode = LightRenderMode.ForcePixel;
                li.enabled = false;
                lights[i] = li;
            }
            ctx.Register(new Driver(specs, lights));
        }

        sealed class Driver : IWorldUpdater
        {
            readonly double[] m_x0, m_x1;
            readonly float[] m_i;
            readonly Light[] m_lights;

            public Driver(List<Spec> specs, Light[] lights)
            {
                int n = specs.Count;
                m_x0 = new double[n]; m_x1 = new double[n]; m_i = new float[n];
                for (int i = 0; i < n; i++) { m_x0[i] = specs[i].X0; m_x1[i] = specs[i].X1; m_i[i] = specs[i].Intensity; }
                m_lights = lights;
            }

            public void OnFrame(in FrameState f)
            {
                double xc = f.cameraPos.x;
                for (int i = 0; i < m_lights.Length; i++)
                {
                    var li = m_lights[i];
                    if (li == null) continue;
                    double w = Weight(m_x0[i], m_x1[i], xc);
                    bool on = w > 0.0;
                    if (li.enabled != on) li.enabled = on;
                    if (on) li.intensity = m_i[i] * (float)w;
                }
            }
        }
    }
}
