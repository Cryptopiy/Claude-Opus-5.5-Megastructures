// DreadDriver: the per-frame placement of the D-034 objects of package DR (docs/DREAD.md 2.1, 3.1), an IWorldUpdater registered by
// DreadBuilder. Play: DreadClock.Step per object (launch on the first crossing of its s_L, real seconds after it; NocState.Dt, so a
// pause stops it). Capture mode (stills and sequences): tau = max(0, (s - s_L) / 2.5). Writes:
//   - THE ARK's transform (D-043: x 515.3, y 0, z = its bow; renderers off until the launch at s 380), AT's deck occluder
//     _NocDeckOcc (centre x / z, half-length 1 470, strength 0.45 x Emerge); _NocDreadHull stays 0 (DR's veil is off);
//   - the shadow's transform (x 470, y -700, z centre), its body opacity and light gain (x the s fade), and AB's NocStreak global
//     _NocAbyssOccluder (centre x / z, half-length 350, strength 0.85 x fade; zero when faded out) and AT's _NocHazeOcc.
//   - the gantries' transforms (DreadCabins.GantryZ of the scene clock f.time);
//   - the falling panel's transform (PanelCourse: on face B until s 470, then the tear, the push, the drag fall and the tumble).
// A hidden object (the hull past z 9 500, the shadow faded to 0, the panel 50 s after its fall began) has its renderers disabled: nothing is visible then (no pop).
// Zero allocations per frame (SetGlobalVector / SetFloat with cached ids, Vector4 structs).
using UnityEngine;

namespace Megastructure
{
    public sealed class DreadDriver : IWorldUpdater
    {
        public static readonly int HullId = Shader.PropertyToID("_NocDreadHull");
        public static readonly int HullRId = Shader.PropertyToID("_NocDreadHullR");
        public static readonly int AbyssOccluderId = Shader.PropertyToID("_NocAbyssOccluder");
        /// <summary>AT's NocAtmosphere D-034 hooks (the hull in the deck plane, the shadow on the haze plane y -650).</summary>
        public static readonly int DeckOccId = Shader.PropertyToID("_NocDeckOcc");
        public static readonly int HazeOccId = Shader.PropertyToID("_NocHazeOcc");
        static readonly int s_opacity = Shader.PropertyToID("_Opacity");
        static readonly int s_gain = Shader.PropertyToID("_Gain");

        readonly Transform m_hull, m_shadow, m_panel;
        readonly Transform[] m_gantries;
        readonly Renderer[] m_hullR, m_shadowR, m_panelR;
        readonly Material m_bodyMat, m_shadowLightMat;
        DreadLaunchState m_hs, m_ss, m_ps;
        bool m_hullVisible = true, m_shadowVisible = true, m_panelVisible = true;

        /// <summary>Last values (QA, tests, the log).</summary>
        public double HullTau { get; private set; }
        public double HullStern { get; private set; }
        public double HullLift { get; private set; }
        public double HullBow { get; private set; }
        public double ShadowTau { get; private set; }
        public double ShadowCentre { get; private set; }
        public double ShadowFade { get; private set; }
        public double PanelTau { get; private set; }

        public DreadDriver(Transform hull, Renderer[] hullR, Transform shadow, Renderer[] shadowR, Material bodyMat, Material shadowLightMat,
                           Transform panel = null, Renderer[] panelR = null, Transform[] gantries = null)
        {
            m_hull = hull; m_hullR = hullR; m_shadow = shadow; m_shadowR = shadowR; m_bodyMat = bodyMat; m_shadowLightMat = shadowLightMat;
            m_panel = panel; m_panelR = panelR; m_gantries = gantries;
            Shader.SetGlobalVector(HullRId, new Vector4((float)HullCourse.VeilHalfW, (float)HullCourse.VeilHalfL, (float)HullCourse.VeilR0, 0f));
            Shader.SetGlobalVector(HullId, Vector4.zero);
            Shader.SetGlobalVector(AbyssOccluderId, Vector4.zero);
            Shader.SetGlobalVector(DeckOccId, Vector4.zero);
            Shader.SetGlobalVector(HazeOccId, Vector4.zero);
        }

        public void OnFrame(in FrameState f)
        {
            // ---- THE ARK (D-043): hidden until the launch, then HullCourse (down the rift toward -z, bow first)
            double th = NocState.Capture ? DreadClock.CaptureTau(f.s, HullCourse.LaunchS) : DreadClock.Step(ref m_hs, f.s, NocState.Dt, HullCourse.LaunchS);
            double bow = HullCourse.BowZ(th);
            HullTau = th; HullStern = HullCourse.SternZ(th); HullLift = 0.0; HullBow = bow;
            bool hv = !HullCourse.HiddenAt(th);
            if (hv != m_hullVisible) { m_hullVisible = hv; SetEnabled(m_hullR, hv); }
            if (m_hull != null) m_hull.position = new Vector3((float)HullCourse.AxisX, 0f, (float)bow);
            double zc = bow + 0.5 * HullCourse.Length;
            Shader.SetGlobalVector(HullId, Vector4.zero);
            // AT's deck occluder about the ship's centre, faded in over the first 10 s (the ship 3 km out, in the haze)
            double occW = HullCourse.DeckOccW * HullCourse.Emerge(th);
            Shader.SetGlobalVector(DeckOccId, hv && occW > 0.0 ? new Vector4((float)HullCourse.AxisX, (float)zc, (float)HullCourse.DeckOccHalf, (float)occW)
                                                               : Vector4.zero);

            // ---- the shadow below (3.1)
            double ts = NocState.Capture ? DreadClock.CaptureTau(f.s, ShadowCourse.LaunchS) : DreadClock.Step(ref m_ss, f.s, NocState.Dt, ShadowCourse.LaunchS);
            double cz = ShadowCourse.CentreZ(ts);
            double fade = ShadowCourse.Fade(f.s);
            ShadowTau = ts; ShadowCentre = cz; ShadowFade = fade;
            bool sv = fade > 0.0;
            if (sv != m_shadowVisible) { m_shadowVisible = sv; SetEnabled(m_shadowR, sv); }
            if (m_shadow != null) m_shadow.position = new Vector3((float)ShadowCourse.AxisX, (float)ShadowCourse.CentreY, (float)cz);
            if (m_bodyMat != null) m_bodyMat.SetFloat(s_opacity, (float)(ShadowCourse.Opacity * fade));
            if (m_shadowLightMat != null) m_shadowLightMat.SetFloat(s_gain, (float)fade);
            Shader.SetGlobalVector(AbyssOccluderId, ShadowCourse.Occluder(cz, fade));
            Shader.SetGlobalVector(HazeOccId, ShadowCourse.HazeOccluder(cz, fade));

            // ---- the falling panel (3.2)
            double tp = NocState.Capture ? PanelCourse.TauAtS(f.s) : DreadClock.Step(ref m_ps, f.s, NocState.Dt, PanelCourse.LaunchS);
            PanelTau = tp;
            bool pv = !PanelCourse.Hidden(tp);
            if (pv != m_panelVisible) { m_panelVisible = pv; SetEnabled(m_panelR, pv); }
            if (pv && m_panel != null) m_panel.SetPositionAndRotation(PanelCourse.Centre(tp), PanelCourse.Rotation(tp));

            // ---- the gantries (2.2): pure functions of the scene clock (the cabins move in NocDreadLight on the traffic clock)
            if (m_gantries != null)
                for (int i = 0; i < m_gantries.Length; i++)
                    if (m_gantries[i] != null) m_gantries[i].position = DreadBuilder.GantryPos(i, f.time);
        }

        static void SetEnabled(Renderer[] rs, bool on)
        {
            if (rs == null) return;
            for (int i = 0; i < rs.Length; i++) if (rs[i] != null) rs[i].enabled = on;
        }
    }
}
