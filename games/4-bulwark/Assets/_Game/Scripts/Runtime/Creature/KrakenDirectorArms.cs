// KrakenDirectorArms: writes a hero arm's centre line over the rig's chain (show owner). The choreography gives world
// waypoints of an arch (K2 column, K4 drape, K5 whip) or a free-tip column (stinger) and one ANCHOR: the waypoint that holds
// a fixed material arc (the apex of an arch, the tip of a column). After KrakenRig.Evaluate this class
//   1. starts the path at the rig's own current point and tangent at arc UStart (C1 with the rig's chain),
//   2. inserts one free "dip" waypoint between that start and the first choreography waypoint and solves its depth by
//      bisection so the path's arc length from the start to the anchor equals AnchorU - UStart (so the anchored material
//      never slides: both legs of an arch feed from the water, LEAD DECISION 2 "slow and heavy"),
//   3. places every control point u >= UStart at path arc (u - UStart) (beyond the path end: straight on along its last
//      tangent) and gives it a parallel-transported frame rolled so the sucker side faces the requested underside.
// Everything happens in kraken local space (KrakenRig.WorldToLocal), so the root's heave never moves the path in the world.
// Pure in its inputs, allocation-free (fixed sample buffers).
using UnityEngine;

namespace Megastructure.Creature
{
    public sealed class KrakenDirectorArms
    {
        const int SamplesPerSeg = 20;
        const int MaxPts = KrakenArmPose.MaxWaypoints + 3;
        readonly Vector3[] m_pts = new Vector3[MaxPts];
        readonly Vector3[] m_samp = new Vector3[MaxPts * SamplesPerSeg + 2];
        readonly float[] m_len = new float[MaxPts * SamplesPerSeg + 2];
        readonly Vector3[] m_tmpP = new Vector3[KrakenRig.ControlPoints];
        int m_nPts, m_nSamp, m_anchorSample;

        /// <summary>Diagnostics of the last Apply: arc stretch (1 = exact), dip depth (m), solved anchor error (m).</summary>
        public float LastStretch { get; private set; } = 1f;
        public float LastDip { get; private set; }
        public float LastAnchorError { get; private set; }

        /// <summary>Overrides limb a.Limb of the rig's current chain with the arm pose (after rig.Evaluate).</summary>
        public void Apply(KrakenRig rig, KrakenArmPose a)
        {
            if (rig == null || a == null || a.Limb < 0 || a.Limb >= KrakenRig.ArmCount || a.Weight <= 0f || a.Count < 2) return;
            var L = rig.Limbs[a.Limb];
            int K = L.K;
            int ib = 1;
            while (ib < K - 2 && L.U[ib] < a.UStart) ib++;
            float ub = L.U[ib];
            Matrix4x4 W2L = rig.WorldToLocal;
            Vector3 p0 = L.CurP[ib];
            Vector3 t0 = L.CurP[ib + 1] - L.CurP[ib - 1];
            t0 = t0.sqrMagnitude > 1e-8f ? t0.normalized : (L.CurQ[ib] * Vector3.forward);
            float scale = Mathf.Max(rig.Scale, 1e-3f);

            // waypoints in local space: start, start + tangent, dip (solved), choreography waypoints
            m_pts[0] = p0;
            m_pts[1] = p0 + t0 * (40f / scale);
            int off = 3;
            for (int i = 0; i < a.Count; i++) m_pts[off + i] = W2L.MultiplyPoint3x4(a.P[i]);
            m_nPts = off + a.Count;
            int anchorPt = off + Mathf.Clamp(a.AnchorIndex, 0, a.Count - 1);
            Vector3 mid = 0.5f * (m_pts[1] + m_pts[off]);
            Vector3 downL = W2L.MultiplyVector(Vector3.down).normalized;
            float required = a.AnchorU - ub;              // local units (the rig's arc u)

            // bisection on the dip depth: arc length to the anchor grows with the depth
            float lo = 0f, hi = 2500f / scale;
            m_pts[2] = mid;
            float len0 = Build(anchorPt);
            float dip = 0f;
            float stretch = 1f;
            if (len0 >= required)
            {
                stretch = len0 / Mathf.Max(required, 1f);        // too far: the material is stretched (logged)
            }
            else
            {
                for (int it = 0; it < 24; it++)
                {
                    float m = 0.5f * (lo + hi);
                    m_pts[2] = mid + downL * m;
                    float len = Build(anchorPt);
                    if (len < required) lo = m; else hi = m;
                }
                dip = 0.5f * (lo + hi);
                m_pts[2] = mid + downL * dip;
                float lenF = Build(anchorPt);
                LastAnchorError = (lenF - required) * scale;
            }
            if (len0 >= required) { m_pts[2] = mid; Build(anchorPt); LastAnchorError = (len0 - required) * scale; }
            LastDip = dip * scale; LastStretch = stretch;

            // place the control points
            float w = Mathf.Clamp01(a.Weight);
            for (int i = ib; i < K; i++)
            {
                float arc = (L.U[i] - ub) * stretch;
                Vector3 pp = SampleAt(arc);
                m_tmpP[i] = w >= 0.999f ? pp : Vector3.Lerp(L.CurP[i], pp, w);
            }
            // frames: parallel transport from the rig's frame at ib, then roll the sucker side (-Y) to the underside
            Vector3 under = a.Underside.sqrMagnitude > 1e-6f ? W2L.MultiplyVector(a.Underside).normalized : Vector3.zero;
            Quaternion q = L.CurQ[ib];
            Vector3 prevT = q * Vector3.forward;
            for (int i = ib; i < K; i++)
            {
                Vector3 T = (i < K - 1 ? m_tmpP[i + 1] : m_tmpP[i]) - (i > ib ? m_tmpP[i - 1] : m_tmpP[i]);
                if (T.sqrMagnitude < 1e-10f) T = prevT; else T.Normalize();
                q = Quaternion.FromToRotation(prevT, T) * q;
                prevT = T;
                Quaternion qi = q;
                if (under != Vector3.zero)
                {
                    Vector3 want = under - T * Vector3.Dot(under, T);
                    float m = want.magnitude;
                    if (m > 0.05f)
                    {
                        Vector3 cur = qi * Vector3.down;
                        float ang = Vector3.SignedAngle(cur, want / m, T);
                        float ramp = KrakenMath.Smooth(0f, 80f / scale, L.U[i] - ub) * Mathf.Clamp01((m - 0.05f) / 0.3f);
                        qi = Quaternion.AngleAxis(ang * ramp, T) * qi;
                    }
                }
                L.CurP[i] = m_tmpP[i];
                L.CurQ[i] = w >= 0.999f ? qi : Quaternion.Slerp(L.CurQ[i], qi, w);
            }
        }

        /// <summary>Samples the Catmull-Rom through m_pts; returns the arc length from the start to the anchor waypoint.</summary>
        float Build(int anchorPt)
        {
            int n = m_nPts;
            int k = 0;
            float acc = 0f;
            m_samp[0] = m_pts[0]; m_len[0] = 0f; k = 1;
            float anchorLen = 0f;
            for (int s = 0; s < n - 1; s++)
            {
                Vector3 a0 = m_pts[Mathf.Max(s - 1, 0)], a1 = m_pts[s], a2 = m_pts[s + 1], a3 = m_pts[Mathf.Min(s + 2, n - 1)];
                for (int j = 1; j <= SamplesPerSeg; j++)
                {
                    Vector3 p = KrakenMath.CatmullRom(a0, a1, a2, a3, j / (float)SamplesPerSeg);
                    acc += Vector3.Distance(p, m_samp[k - 1]);
                    m_samp[k] = p; m_len[k] = acc; k++;
                }
                if (s + 1 == anchorPt) { anchorLen = acc; m_anchorSample = k - 1; }
            }
            m_nSamp = k;
            return anchorLen;
        }

        Vector3 SampleAt(float arc)
        {
            int n = m_nSamp;
            if (arc <= 0f) return m_samp[0];
            if (arc >= m_len[n - 1])
            {
                Vector3 d = m_samp[n - 1] - m_samp[n - 2];
                d = d.sqrMagnitude > 1e-10f ? d.normalized : Vector3.down;
                return m_samp[n - 1] + d * (arc - m_len[n - 1]);
            }
            int lo = 0, hi = n - 1;
            while (hi - lo > 1) { int m = (lo + hi) >> 1; if (m_len[m] <= arc) lo = m; else hi = m; }
            float f = (arc - m_len[lo]) / Mathf.Max(m_len[hi] - m_len[lo], 1e-6f);
            return Vector3.LerpUnclamped(m_samp[lo], m_samp[hi], f);
        }
    }
}
