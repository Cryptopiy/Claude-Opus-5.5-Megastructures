// KoiEye: the koi's eye follows the walker (DREAD 5.2, D-034; fix wave p4_01 C27, lead ruling: "the eye spot turns its pupil toward
// the camera, at most 6 deg/s, and keeps following while the head is in view (no flash, no pop)").
// Each eye spot (KoiShaderMath.EyeA = 36 m behind the snout, ring angle EyeTheta on the outer flank and pi - EyeTheta on the inner)
// has a gaze g, a unit vector in the eye's local frame (x along +a toward the tail, y along +theta (the ring arc), z out of the
// skin). Its target is the direction from the eye to the camera in that frame (KoiMath.Frame: the same ring frame NocKoi's vertex
// stage builds, D-009). Each frame g turns toward the target by at most MaxRateDeg x dt (a great-circle step), and the pupil sits at
// Reach x EyeR x (g.x, g.y) metres from the eye's centre (NocKoi _KoiPupil: outer a, arc; inner a, arc), i.e. inside the ring.
// Capture mode: a still (a jump of more than SnapDs metres in s) snaps g to its target, so a still is a pure function of s; a
// sequence (60 fps steps of 0.04 m at 2.5 m/s) slews like play. Zero allocations (D3 is a struct).
using System;
using UnityEngine;

namespace Megastructure
{
    public static class KoiEye
    {
        /// <summary>Fastest pupil turn (deg / s, lead ruling D-034 5.2).</summary>
        public const double MaxRateDeg = 6.0;
        /// <summary>Largest pupil offset as a fraction of the eye radius (NocKoi: at most 0.3 R off the centre).</summary>
        public const double Reach = 0.3;
        /// <summary>A capture-mode jump in s larger than this (m) is a new still: the gaze snaps to its target.</summary>
        public const double SnapDs = 0.5;

        /// <summary>The direction from eye `side` (0 outer, 1 inner) to the camera in the eye's frame (x +a, y +arc, z outward).</summary>
        public static D3 Target(double phiHead, double tw, D3 cam, int side)
        {
            double th = side == 0 ? KoiShaderMath.EyeTheta : Math.PI - KoiShaderMath.EyeTheta;
            KoiMath.Frame(KoiShaderMath.EyeA, phiHead, tw, out D3 c, out D3 f, out D3 n, out D3 b);
            double ct = Math.Cos(th), st = Math.Sin(th);
            D3 ring = n * ct + b * st, tang = n * (-st) + b * ct;
            D3 e = c + ring * KoiMath.Radius(KoiShaderMath.EyeA);
            D3 d = (cam - e).Normalized;
            return new D3(-D3.Dot(d, f), D3.Dot(d, tang), D3.Dot(d, ring));
        }

        /// <summary>Turns unit vector g toward unit vector t by at most maxRad (great circle); returns t when it is closer.</summary>
        public static D3 Slew(D3 g, D3 t, double maxRad)
        {
            double cos = NocMath.Clamp(D3.Dot(g, t), -1.0, 1.0);
            double ang = Math.Acos(cos);
            if (ang <= maxRad || ang < 1e-9) return t;
            double s = Math.Sin(ang);
            if (s < 1e-6)
            {
                // opposite: turn about any axis perpendicular to g
                D3 axis = Math.Abs(g.z) < 0.9 ? new D3(0, 0, 1) : new D3(1, 0, 0);
                D3 p = D3.Cross(g, axis).Normalized;
                return (g * Math.Cos(maxRad) + p * Math.Sin(maxRad)).Normalized;
            }
            double k = maxRad / ang;
            return (g * (Math.Sin((1.0 - k) * ang) / s) + t * (Math.Sin(k * ang) / s)).Normalized;
        }

        /// <summary>The _KoiPupil uniform (outer a, arc; inner a, arc; metres) from the two gazes.</summary>
        public static Vector4 Pupil(D3 gOuter, D3 gInner)
        {
            double r = Reach * KoiShaderMath.EyeR;
            return new Vector4((float)(r * gOuter.x), (float)(r * gOuter.y), (float)(r * gInner.x), (float)(r * gInner.y));
        }

        /// <summary>The angle between two unit gazes (deg).</summary>
        public static double AngleDeg(D3 a, D3 b) => Math.Acos(NocMath.Clamp(D3.Dot(a, b), -1.0, 1.0)) / NocMath.Deg;
    }

    /// <summary>The two eyes' gaze state (KoiDriver; tests).</summary>
    public struct KoiEyeState
    {
        public D3 outer, inner;
        public bool init;
        public double lastS;

        /// <summary>One frame: snap on the first frame or a capture-mode jump, else slew at MaxRateDeg x dt.</summary>
        public void Step(double phiHead, double tw, D3 cam, double s, double dt, bool capture)
        {
            D3 tO = KoiEye.Target(phiHead, tw, cam, 0), tI = KoiEye.Target(phiHead, tw, cam, 1);
            if (!init || (capture && Math.Abs(s - lastS) > KoiEye.SnapDs))
            {
                outer = tO; inner = tI; init = true;
            }
            else
            {
                double mr = KoiEye.MaxRateDeg * NocMath.Deg * Math.Max(0.0, dt);
                outer = KoiEye.Slew(outer, tO, mr);
                inner = KoiEye.Slew(inner, tI, mr);
            }
            lastS = s;
        }
    }
}
