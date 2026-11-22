// TrainDriver: the per-frame placement of the trains T1-T7 (MASTER_PROMPT 14.5.4, 14.7 H7; owner TR, Phase 4 wave B), an
// IWorldUpdater registered by TrainBuilder. Play: TrainSchedule.Step per train (launch on the first crossing of s_launch, tau =
// real seconds from NocState.Dt, so a pause stops the trains; the reset below s_launch - 12 is applied only while a train is
// invisible). Capture mode (stills and sequences): tau = max(0, (s - s_launch) / 2.5). A train is drawn only while it runs
// (0 < tau < HiddenTau): before that its nose waits on the origin mouth plane, after that its tail is past the destination
// tunnel end, and in both states every fragment lies in the dark of a tunnel or behind a clip plane (no pop). Its lights (one
// SignWhite headlight, one or two WinNeutral wash lights in the cars) fade by TrainSchedule.LightFade: zero inside and at the
// mouths, full 30 m (headlight) or 24 m (wash) into the open section. Zero allocations per frame.
using UnityEngine;

namespace Megastructure
{
    public sealed class TrainDriver : IWorldUpdater
    {
        /// <summary>Light ramps: the headlight starts HeadAhead (4 m) out of the mouth at launch, ramp 30 m; the wash lights 24 m.</summary>
        public const double HeadStart = TrainBuilder.HeadAhead + 0.1, HeadRamp = 30.0, WashStart = 0.0, WashRamp = 24.0;

        readonly Transform[] m_tr;
        readonly Renderer[] m_r;
        readonly Light[] m_head;
        readonly Light[][] m_wash;
        readonly TrainState[] m_state;
        readonly bool[] m_on;

        /// <summary>Last tau of each train (QA, tests).</summary>
        public readonly double[] Tau;

        public TrainDriver(Transform[] tr, Renderer[] r, Light[] head, Light[][] wash)
        {
            m_tr = tr; m_r = r; m_head = head; m_wash = wash;
            int n = TrainSchedule.Trains.Length;
            m_state = new TrainState[n];
            m_on = new bool[n];
            Tau = new double[n];
            for (int i = 0; i < n; i++) TrainSchedule.Reset(ref m_state[i]);
        }

        public void OnFrame(in FrameState f)
        {
            for (int i = 0; i < m_tr.Length; i++)
            {
                double tau = NocState.Capture ? TrainSchedule.CaptureTau(i, f.s) : TrainSchedule.Step(i, ref m_state[i], f.s, NocState.Dt);
                Tau[i] = tau;
                bool on = TrainSchedule.Active(i, tau);
                if (on != m_on[i])
                {
                    m_on[i] = on;
                    if (m_r[i] != null) m_r[i].enabled = on;
                    if (!on && m_tr[i] != null) m_tr[i].position = TrainSchedule.ParkPos(i);   // parked under the world while hidden
                    if (m_head[i] != null) m_head[i].enabled = on;
                    var ws = m_wash[i];
                    if (ws != null) for (int w = 0; w < ws.Length; w++) if (ws[w] != null) ws[w].enabled = on;
                }
                if (!on) continue;
                Place(i, tau);
            }
        }

        void Place(int i, double tau)
        {
            var spec = TrainSchedule.Trains[i];
            var tb = TrainSchedule.Tubes[spec.Tube];
            double nose = TrainSchedule.NoseX(i, tau);
            if (m_tr[i] != null) m_tr[i].SetPositionAndRotation(TrainSchedule.FramePos(i, tau), TrainSchedule.FrameRot(i));
            float y = (float)tb.AxisY, z = (float)tb.AxisZ;
            if (m_head[i] != null)
            {
                double xl = nose + spec.Dir * TrainBuilder.HeadAhead;
                m_head[i].transform.position = new Vector3((float)xl, y + TrainBuilder.HeadY, z);
                m_head[i].intensity = TrainBuilder.HeadIntensity * (float)TrainSchedule.LightFade(spec.Tube, xl, HeadStart, HeadRamp);
            }
            var ws = m_wash[i];
            if (ws == null) return;
            for (int w = 0; w < ws.Length; w++)
            {
                if (ws[w] == null) continue;
                double xl = nose + spec.Dir * TrainBuilder.WashX(i, w);
                ws[w].transform.position = new Vector3((float)xl, y + 0.6f, z);
                ws[w].intensity = TrainBuilder.WashIntensity * (float)TrainSchedule.LightFade(spec.Tube, xl, WashStart, WashRamp);
            }
        }
    }
}
