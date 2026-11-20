// TramDriver: the per-frame placement of the tram T_A (MASTER_PROMPT 14.5.2, 14.7 H7; owner BV, Phase 4 wave A), an IWorldUpdater
// registered by Beat02_Boulevard. Play: TramSchedule.Step (launch on the first crossing of s 230, tau = real seconds from
// NocState.Dt, so a pause stops the tram; the reset below s 218 is applied only while the tram is invisible). Capture mode (stills
// and sequences): tau = max(0, (s - 230) / 2.5). Each of the five modules is placed on the route by its two bogie centres (on
// the straight all are collinear; on the r 35 curve each module is a chord). Three moving point lights (no shadows): the cool
// headlight 1.2 m ahead of the nose (SignWhite r 22 i 3) and two saloon lights in modules 1 and 3 (WinNeutral r 11 i 1.4) that
// put the tram's light on the wet street; each fades by TramSchedule before it could shine out of a sealed mesh. Once the tram is
// fully clipped inside the depot its renderers and lights are disabled (nothing is visible: no pop). Zero allocations per frame.
using UnityEngine;

namespace Megastructure
{
    public sealed class TramDriver : IWorldUpdater
    {
        public const float HeadRange = 22f, HeadIntensity = 3f, SaloonRange = 11f, SaloonIntensity = 1.4f;
        public static readonly int[] SaloonCars = { 1, 3 };

        readonly Transform[] m_cars;
        readonly Renderer[] m_renderers;
        readonly Light m_head;
        readonly Light[] m_saloon;
        TramState m_state;
        bool m_visible = true;

        /// <summary>Last tau, head arc position and phase (QA, tests).</summary>
        public double Tau { get; private set; }
        public double HeadP { get; private set; }
        public TramPhase Phase => NocState.Capture ? (TramSchedule.Hidden(HeadP) ? TramPhase.Done : TramPhase.Running) : m_state.Phase;

        public TramDriver(Transform[] cars, Renderer[] renderers, Light head, Light[] saloon)
        {
            m_cars = cars; m_renderers = renderers; m_head = head; m_saloon = saloon;
            TramSchedule.Reset(ref m_state);
        }

        public void OnFrame(in FrameState f)
        {
            double tau = NocState.Capture ? TramSchedule.CaptureTau(f.s) : TramSchedule.Step(ref m_state, f.s, NocState.Dt);
            double p = TramSchedule.HeadP(tau);
            Tau = tau; HeadP = p;
            bool visible = !TramSchedule.Hidden(p);
            if (visible != m_visible)
            {
                m_visible = visible;
                for (int i = 0; i < m_renderers.Length; i++) if (m_renderers[i] != null) m_renderers[i].enabled = visible;
                if (m_head != null) m_head.enabled = visible;
                for (int i = 0; i < m_saloon.Length; i++) if (m_saloon[i] != null) m_saloon[i].enabled = visible;
            }
            if (!visible) return;
            for (int k = 0; k < m_cars.Length; k++)
            {
                if (m_cars[k] == null) continue;
                Place(p, k, out Vector3 pos, out Quaternion rot);
                m_cars[k].SetPositionAndRotation(pos, rot);
            }
            if (m_head != null)
            {
                m_head.transform.position = TramSchedule.PointV(p + 1.2, 0.95);
                m_head.intensity = HeadIntensity * (float)TramSchedule.HeadlightFade(p);
            }
            for (int i = 0; i < m_saloon.Length; i++)
            {
                if (m_saloon[i] == null) continue;
                double pc = p - TramSchedule.CarFront(SaloonCars[i]) - 0.5 * TramSchedule.CarL;
                m_saloon[i].transform.position = TramSchedule.PointV(pc, 2.1);
                m_saloon[i].intensity = SaloonIntensity * (float)TramSchedule.RideLightFade(pc);
            }
        }

        /// <summary>Module k's pose for a head at p: the midpoint of its two bogie centres on the route, +z toward the front bogie.</summary>
        public static void Place(double p, int k, out Vector3 pos, out Quaternion rot)
        {
            double front = p - TramSchedule.CarFront(k);
            double bf = front - TramSchedule.BogieInset, br = front - TramSchedule.CarL + TramSchedule.BogieInset;
            Vector3 a = TramSchedule.PointV(bf), b = TramSchedule.PointV(br);
            Vector3 d = a - b;
            // the module centre lies on the chord, half a module behind its front end along the chord direction
            Vector3 dir = d.sqrMagnitude > 1e-8f ? d.normalized : Vector3.left;
            pos = 0.5f * (a + b);
            rot = Quaternion.LookRotation(dir, Vector3.up);
        }
    }
}
