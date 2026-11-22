// LobbyDoors: the elevator lobby on the terminus west face (DREAD 6, D-034; lead ruling 2026-11-21, panel p4_01 C29; owner BV).
// A lit, empty lobby (a NocShopfront room) behind a 2.8 x 3.1 m opening in the west face's southern bay (z 214.1..216.9, y 0..3.1), two brushed
// steel leaves that slide 1.40 m into pockets inside the 1.6 m wall (hidden by the outer face), a small white sign over the
// opening and the lobby's light on the paving in front (a NocAlleyPool quad whose gain follows the opening: a MaterialPropertyBlock,
// so the shared pool material of beat 1 is untouched).
//
// Timing (DREAD 6: "the doors open onto an empty lit hall and close again after 8 s"): the doors open when s first crosses
// TriggerS 350 (the walker is 40 m away, the lobby 34 deg left of the walk, in frame under the 24-30 deg gaze of s 340-370; after
// the s 345 sheet-lightning glow), over 1.5 s, stay open 8 s, close over 1.5 s; real seconds (NocState.Dt, a pause stops them).
// Capture mode: tau = max(0, (s - 350) / 2.5) (open s 353.75-373.75, shut by s 377.5). Re-armed below s 338 once shut.
// Photosensitivity: one smooth 1.5 s rise and one 1.5 s fall (smootherstep), a 60 x 65 px region at 40 m: far under D-010.
// Zero allocations per frame; the leaves move only while the opening changes.
using System;
using UnityEngine;

namespace Megastructure
{
    public sealed class LobbyDoors : IWorldUpdater
    {
        // ---- timing
        public const double TriggerS = 350.0, ReArmDrop = 12.0, CaptureSpeed = 2.5;
        public const double OpenSeconds = 1.5, HoldSeconds = 8.0, CloseSeconds = 1.5;
        public static double TotalSeconds => OpenSeconds + HoldSeconds + CloseSeconds;

        // ---- geometry (west face x 205, wall 1.6 m; the southern bay z 212..219.25)
        public const float Z0 = 214.1f, Z1 = 216.9f, Y1 = 3.1f;                    // the opening
        public const float LeafX0 = 205.28f, LeafX1 = 205.34f, LeafOverlap = 0.01f;  // the leaves' plane in the reveal (pockets in the wall)
        public static float Travel => 0.5f * (Z1 - Z0);                              // 1.40 m: each leaf fully into its pocket
        public const float RoomX1 = 210.2f, RoomZ0 = 212.6f, RoomZ1 = 218.4f, RoomY1 = 3.5f;
        public const float SpillGain = 1.6f;

        /// <summary>Capture-mode tau: max(0, (s - 350) / 2.5).</summary>
        public static double CaptureTau(double s) => Math.Max(0.0, (s - TriggerS) / CaptureSpeed);

        /// <summary>The doors' opening at tau seconds after the trigger (0 shut .. 1 open).</summary>
        public static double Opening(double tau)
        {
            if (tau <= 0.0 || tau >= TotalSeconds) return 0.0;
            return NocMath.Smootherstep(0.0, OpenSeconds, tau) * (1.0 - NocMath.Smootherstep(OpenSeconds + HoldSeconds, TotalSeconds, tau));
        }

        /// <summary>Play-mode state: armed, running (tau), done.</summary>
        public struct State { public bool Running, Done; public double Tau; }

        /// <summary>One play-mode frame: trigger on the first crossing of 350 (or at once when the walk starts beyond it and before
        /// the end of the beat), run on real seconds, re-arm below s 338 once shut. Returns the opening.</summary>
        public static double Step(ref State st, double s, double dt)
        {
            if (st.Running)
            {
                st.Tau += Math.Max(0.0, dt);
                if (st.Tau >= TotalSeconds) { st.Running = false; st.Done = true; st.Tau = 0.0; }
            }
            else if (st.Done) { if (s < TriggerS - ReArmDrop) st.Done = false; }
            else if (s >= TriggerS && s < 400.0) { st.Running = true; st.Tau = 0.0; }
            return st.Running ? Opening(st.Tau) : 0.0;
        }

        readonly Transform m_a, m_b;
        readonly Renderer m_spill;
        readonly MaterialPropertyBlock m_mpb;
        readonly Vector3 m_a0, m_b0;
        State m_state;
        double m_shown = -1.0;
        static readonly int IdGain = Shader.PropertyToID("_Gain");

        /// <summary>The current opening (QA, tests).</summary>
        public double OpeningNow { get; private set; }

        public LobbyDoors(Transform a, Transform b, Renderer spill)
        {
            m_a = a; m_b = b; m_spill = spill;
            m_a0 = a != null ? a.localPosition : Vector3.zero;
            m_b0 = b != null ? b.localPosition : Vector3.zero;
            m_mpb = new MaterialPropertyBlock();
            Apply(0.0);
        }

        public void OnFrame(in FrameState f)
        {
            double o = NocState.Capture ? Opening(CaptureTau(f.s)) : Step(ref m_state, f.s, NocState.Dt);
            OpeningNow = o;
            if (o != m_shown) Apply(o);
        }

        void Apply(double o)
        {
            m_shown = o;
            float off = (float)o * Travel;
            if (m_a != null) m_a.localPosition = m_a0 + new Vector3(0f, 0f, -off);
            if (m_b != null) m_b.localPosition = m_b0 + new Vector3(0f, 0f, off);
            if (m_spill != null)
            {
                m_spill.enabled = o > 1e-4;
                m_mpb.SetFloat(IdGain, (float)o);
                m_spill.SetPropertyBlock(m_mpb);
            }
        }

        // ------------------------------------------------------------------ geometry (TramTerminus calls it)
        /// <summary>The lobby: the lit lobby seen through the opening is a NocShopfront room (G1 EmitCold: a cold lit room 4 m
        /// deep, the game's one interior shader; BV_dbg01 at s 260: a modelled tiled room lit only by pools read as a dim grey
        /// hole) on a panel 6 cm behind the leaves; behind it the lobby's enclosure in the hall (painted steel: east, north and top
        /// faces). The reveal faces are TramTerminus's (the opening is one of its west holes). The leaves are two parts (lobbyDoorA
        /// / B, each with its own transform), a small white sign over the opening, and the spill pool on the paving (lobbySpill).</summary>
        public static void Build(G1Parts parts, float xw, float floorY)
        {
            var origin = new Vector3(208f, 1.7f, 215.5f);
            var outer = parts.Get("lobbyShell", G1Mat.SteelPainted, origin);
            var room = parts.Get("lobbyRoom", G1Mat.EmitCold, new Vector3(RoomPanelX, 1.55f, 0.5f * (Z0 + Z1)));
            float x0 = xw, x1 = RoomX1, z0 = RoomZ0, z1 = RoomZ1, y0 = floorY, y1 = RoomY1;
            // the lit room behind the leaves: lit state, gain 0.9 (NocShopfront vertex colour: R random, G 1 - gain, B state 0)
            var saved = room.Color;
            room.Color = new Color32(140, 26, 0, 255);
            G1Mesh.Quad(room, new Vector3(RoomPanelX, 0.02f, Z0), new Vector3(RoomPanelX, Y1, Z0), new Vector3(RoomPanelX, Y1, Z1), new Vector3(RoomPanelX, 0.02f, Z1), Vector3.left);
            room.Color = saved;
            // the enclosure in the hall (5 cm outside the room): east, north and top faces (the south one stands on the hall's wall)
            const float t = 0.05f;
            G1Mesh.Quad(outer, new Vector3(x1 + t, y0, z0), new Vector3(x1 + t, y1 + t, z0), new Vector3(x1 + t, y1 + t, z1 + t), new Vector3(x1 + t, y0, z1 + t), Vector3.right);
            G1Mesh.Quad(outer, new Vector3(x0, y0, z1 + t), new Vector3(x0, y1 + t, z1 + t), new Vector3(x1 + t, y1 + t, z1 + t), new Vector3(x1 + t, y0, z1 + t), Vector3.forward);
            G1Mesh.Quad(outer, new Vector3(x0, y1 + t, z0), new Vector3(x1 + t, y1 + t, z0), new Vector3(x1 + t, y1 + t, z1 + t), new Vector3(x0, y1 + t, z1 + t), Vector3.up);
            // the leaves: brushed steel, 1.41 m each (1 cm overlap at the meeting line), in the reveal plane x 205.28..205.34
            float zm = 0.5f * (Z0 + Z1);
            var la = parts.Get("lobbyDoorA", G1Mat.SteelBare, new Vector3(LeafX0, 1.55f, 0.5f * (Z0 + zm)));
            var lb = parts.Get("lobbyDoorB", G1Mat.SteelBare, new Vector3(LeafX0, 1.55f, 0.5f * (zm + Z1)));
            G1Mesh.Chamfer(la, new Vector3(LeafX0, 0.02f, Z0), new Vector3(LeafX1, Y1 - 0.02f, zm + LeafOverlap), 0.006f);
            G1Mesh.Chamfer(lb, new Vector3(LeafX0, 0.02f, zm), new Vector3(LeafX1, Y1 - 0.02f, Z1), 0.006f);
            // a small white sign over the opening (facing the median), on the west face
            parts.Sign(new Vector3(204.94f, Y1 + 0.55f, zm), Quaternion.LookRotation(Vector3.right), 1.1f, 0.34f, 4);
            // the spill on the paving in front (its gain follows the opening; source at the opening's centre, 1.6 m up)
            Color c = Color.Lerp(G1Materials.Lin(NocPal.WinNeutral, 1f), Color.white, 0.5f) * SpillGain;
            var spill = parts.Get("lobbySpill", G1Mat.AlleyPool, new Vector3(202f, 0.01f, zm));
            MedianPromenade.FloorPoolAt(spill, 199.0f, 207.0f, Z0 - 3.0f, Z1 + 3.0f, 205.6f, zm, 0.01f, 1.6f, c);
        }

        /// <summary>The lit room panel's plane: 6 cm behind the leaves (inside the reveal).</summary>
        public const float RoomPanelX = 205.40f;
    }
}
