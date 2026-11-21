// RideSound: the sound of the new ending (LEAD DECISION 1, ADDENDUM_A1 2.8 adapted): the glass express elevator down the lee
// face, the coupling at the foot, the maglev shuttle west, and the silence of the lantern platform. Unity-free, allocation-
// free; driven by the ride fields of BwAudioParams (BulwarkScore.Evaluate from EndingRide) and by one-shot RideCue events
// that AudioDirector / the offline renderer fire on the phase changes.
// Layers (levels at full mix, 3 s RMS; 2.8 targets: -40 at rest and doors, -33.5 at cruise):
//   RM  linear motor whine: f = v / 0.5 m pole pitch in the elevator (0 -> 220 Hz at 110 m/s), v / 0.25 m in the shuttle,
//       harmonics 2 and 3; -36 at cruise, x (1 + 0.7 |a| / 2.5 m/s^2) under acceleration and braking (+4.6 dB at the peak).
//   RI  inverter whine (the shuttle's rising whine): 400 + 14 v Hz (to about 2.5 kHz), 2 harmonics, rising with speed, -40.
//   RA  air rush: pink BP 400 Hz (Q 0.7) + 1.2 kHz seal whistle (Q 2), gain (v / 110)^2, -38 at cruise.
//   RJ  track joints: 2 ms clicks (LP 2 kHz) into steel modes 140/390/1100 Hz every 24 m of travel, -46.
//   RC  cabin: tungsten lamp hum 100/200 Hz, -50.
//   RP  pressure: brown LP 80 Hz rising as the cabin sinks (the air thickens: 0 at the top, full at the foot), -44; the
//       "ear" low-pass of the whole mix closes from open to 2.5 kHz over the descent and pops open in 0.4 s at the foot
//       (BwDsp master ear filter, earHz from BulwarkScore).
//   RB  braking hiss: pink HP 3 kHz, gain max(0, -a) / 2.5 x v / 110, -44.
//   cues RD doors (45 Hz thud 0.4 s + latch click + seal hiss 1.0 s pink HP 3 kHz, peak -24), RK coupling clunk (steel
//       modes struck + 60 Hz thud + 1.8 s pressure hiss, peak -26), RS motor start (soft clunk, -34).
//   Foot (the lantern platform, 2.8 FW/FJ/FC): wind pink BP 380 Hz -48, distant outlet jets brown LP 250 Hz -52, crickets
//       4.6 kHz chirps (3 x 30 ms every 1.2-3 s, 5 individuals) -56: power sum -46.
using System;

namespace Megastructure.Audio
{
    public enum RideCue { DoorsClose = 0, MotorStart = 1, Coupling = 2, DoorsOpen = 3 }

    public sealed class RideCueRing : SpscRing<int> { }

    public sealed class RideSound
    {
        public const double ElevatorPole = 0.5, ShuttlePole = 0.25, VRef = 110.0, ARef = 2.5, JointPitch = 24.0;
        // levels (dBFS RMS at full mix, cruise); calibrated with the offline ride render (AudioTests.Ride_*)
        public const double MotorDb = -36.0, InverterDb = -40.0, AirDb = -38.0, JointsDb = -46.0, CabinDb = -50.0, PressureDb = -44.0, BrakeDb = -44.0;
        public const double FootWindDb = -48.0, FootJetsDb = -52.0, CricketsDb = -56.0;
        public const double DoorPeakDb = -24.0, CouplingPeakDb = -26.0, MotorStartDb = -34.0;
        /// <summary>A1 2.8 sunset hush (fix wave S4-6): while the cabin altitude is between HushTop and HushBottom every ride
        /// layer but the air rush drops HushDb (200 m smoothstep edges: about 2 s at the descent peak of 110 m/s).</summary>
        public const double HushTop = 5467.0, HushBottom = 3421.0, HushDb = 6.0, HushEdge = 100.0;

        /// <summary>Hush weight 0..1 at an altitude (1 inside the sunset band).</summary>
        public static double HushWeight(double alt)
            => BwMath.Smoothstep(HushTop + HushEdge, HushTop - HushEdge, alt) * BwMath.Smoothstep(HushBottom - HushEdge, HushBottom + HushEdge, alt);

        readonly double m_fs;
        readonly RideCueRing m_cues = new RideCueRing();
        XorShift32 m_rng;
        PinkKellet m_pink, m_pink2, m_pink3;
        BrownNoise m_brown, m_brown2;
        SvfTpt m_airBp, m_whistle, m_presLp, m_brakeHp, m_jointLp, m_footBpL, m_footBpR, m_jetsLp, m_hissHp, m_doorClickLp, m_crickBp;
        Reson m_j0, m_j1, m_j2, m_k0, m_k1, m_k2;
        double m_phM, m_phI, m_phC1, m_phC2, m_phThud, m_dist;
        int m_doorN = 1 << 30, m_coupleN = 1 << 30, m_startN = 1 << 30, m_jointN = 1 << 30;
        // smoothed controls
        float m_mix, m_speed, m_accel, m_alt = 6000f, m_foot, m_jets, m_shuttle;
        readonly float m_sm;
        // crickets: 5 individuals, each a chirp train
        readonly double[] m_crNext = new double[5];
        readonly int[] m_crN = new int[5];
        readonly float[] m_crPan = new float[5];
        double m_t;

        public RideSound(double fs, int seed)
        {
            m_fs = fs;
            m_rng = XorShift32.Stream(seed, 0x52494445u);
            m_brown.Init(20.0, fs); m_brown2.Init(20.0, fs);
            m_airBp.Set(400.0, 0.7, fs); m_whistle.Set(1200.0, 2.0, fs); m_presLp.Set(80.0, 0.7, fs); m_brakeHp.Set(3000.0, 0.7, fs);
            m_jointLp.Set(2000.0, 0.7, fs); m_footBpL.Set(380.0, 0.7, fs); m_footBpR.Set(395.0, 0.7, fs); m_jetsLp.Set(250.0, 0.7, fs);
            m_hissHp.Set(3000.0, 0.7, fs); m_doorClickLp.Set(4000.0, 0.7, fs); m_crickBp.Set(4600.0, 8.0, fs);
            m_j0.Set(140, 30, 1.0, fs); m_j1.Set(390, 30, 0.7, fs); m_j2.Set(1100, 30, 0.4, fs);
            m_k0.Set(140, 30, 1.0, fs); m_k1.Set(390, 30, 0.8, fs); m_k2.Set(1100, 30, 0.5, fs);
            m_sm = (float)(1.0 - Math.Exp(-1.0 / (0.08 * fs)));
            for (int i = 0; i < 5; i++) { m_crNext[i] = 0.3 + 0.6 * i; m_crN[i] = 1 << 30; m_crPan[i] = -0.8f + 0.4f * i; }
        }

        /// <summary>QA only (offline level checks): bit i mutes layer i (0 motor, 1 inverter, 2 air, 3 brake, 4 pressure, 5 joints, 6 hum).</summary>
        public int Mute;

        /// <summary>Main thread / offline: queue a one-shot cue.</summary>
        public void Cue(RideCue c) { m_cues.Push((int)c); }

        /// <summary>Per block: start the queued cues.</summary>
        public void Control()
        {
            while (m_cues.Pop(out int c))
            {
                switch ((RideCue)c)
                {
                    case RideCue.DoorsClose:
                    case RideCue.DoorsOpen: m_doorN = 0; m_phThud = 0.0; break;
                    case RideCue.Coupling: m_coupleN = 0; m_k0.y1 = m_k0.y2 = 0f; break;
                    default: m_startN = 0; break;
                }
            }
        }

        /// <summary>One stereo sample. Silent and cheap while the mix and the foot are 0 and no cue rings.</summary>
        public void Tick(in BwAudioParams p, ref float l, ref float r)
        {
            double fs = m_fs;
            m_t += 1.0 / fs;
            m_mix += m_sm * (p.rideMix - m_mix);
            m_speed += m_sm * (p.rideSpeed - m_speed);
            m_accel += m_sm * (p.rideAccel - m_accel);
            m_alt += m_sm * (p.rideAlt - m_alt);
            m_foot += m_sm * (p.foot - m_foot);
            m_jets += m_sm * (p.jets - m_jets);
            m_shuttle += m_sm * ((p.ridePhase == (int)EndingRidePhaseAudio.Shuttle ? 1f : 0f) - m_shuttle);
            float outL = 0f, outR = 0f;

            if (m_mix > 1e-5f)
            {
                double v = Math.Max(0.0, m_speed);
                double vr = Math.Min(1.5, v / VRef);
                double aa = Math.Min(1.0, Math.Abs(m_accel) / ARef);
                // RM motor (+ RI inverter in the shuttle)
                double pole = ElevatorPole + (ShuttlePole - ElevatorPole) * m_shuttle;
                double fM = v / pole;
                float motor = 0f;
                if (fM > 1.0)
                {
                    m_phM += fM / fs; if (m_phM >= 1.0) m_phM -= Math.Floor(m_phM);
                    double ph2 = m_phM * 2.0, ph3 = m_phM * 3.0;
                    motor = SineTable.Lookup(m_phM) + 0.5f * SineTable.Lookup(ph2 - Math.Floor(ph2)) + 0.3f * SineTable.Lookup(ph3 - Math.Floor(ph3));
                    double fade = Math.Min(1.0, v / 3.0);   // no click at the start
                    motor *= (float)(fade * (1.0 + 0.7 * aa) * BwAudioSpec.Db(MotorDb) / 0.818);
                }
                float inv = 0f;
                if (m_shuttle > 1e-3f && v > 1.0)
                {
                    double fI = Math.Min(4000.0, 400.0 + 14.0 * v);
                    m_phI += fI / fs; if (m_phI >= 1.0) m_phI -= Math.Floor(m_phI);
                    double ph2 = m_phI * 2.0;
                    inv = (SineTable.Lookup(m_phI) + 0.35f * SineTable.Lookup(ph2 - Math.Floor(ph2)))
                          * (float)(m_shuttle * Math.Min(1.0, v / 20.0) * (0.4 + 0.6 * vr) * BwAudioSpec.Db(InverterDb) * 1.33);
                }
                // RA air rush and the seal whistle
                float pk = m_pink.Next(m_rng.NextBipolar());
                float air = (m_airBp.BandNorm(pk) * 2.2f + 0.5f * m_whistle.BandNorm(pk)) * (float)(vr * vr * BwAudioSpec.Db(AirDb));
                // RB braking hiss
                double brake = m_accel < 0f ? Math.Min(1.0, -m_accel / ARef) * vr : 0.0;
                float br = brake > 1e-4 ? m_brakeHp.High(m_pink2.Next(m_rng.NextBipolar())) * (float)(brake * BwAudioSpec.Db(BrakeDb) * 1.6) : 0f;
                // RP pressure: the air thickens as the cabin sinks (altitude 6000 -> 0)
                double dens = BwMath.Clamp01(1.0 - m_alt / 6000.0);
                float pres = m_presLp.Low(m_brown.Next(m_rng.NextBipolar())) * (float)((0.3 + 0.7 * dens) * BwAudioSpec.Db(PressureDb) * 1.4);
                // RJ joints every 24 m of travel
                m_dist += v / fs;
                float jx = 0f;
                if (m_dist >= JointPitch) { m_dist -= JointPitch; m_jointN = 0; }
                if (m_jointN < 96) { jx = m_jointLp.Low(m_rng.NextBipolar()) * (1f - m_jointN / 96f); m_jointN++; }
                float joint = (m_j0.Tick(jx) + m_j1.Tick(jx) + m_j2.Tick(jx)) * (float)(Math.Min(1.0, v / 10.0) * BwAudioSpec.Db(JointsDb) * 0.5);
                // RC cabin hum
                m_phC1 += 100.0 / fs; if (m_phC1 >= 1.0) m_phC1 -= 1.0;
                m_phC2 += 200.0 / fs; if (m_phC2 >= 1.0) m_phC2 -= 1.0;
                float hum = (SineTable.Lookup(m_phC1) + 0.5f * SineTable.Lookup(m_phC2)) * (float)(BwAudioSpec.Db(CabinDb) * 1.26);
                if (Mute != 0)
                {
                    if ((Mute & 1) != 0) motor = 0f; if ((Mute & 2) != 0) inv = 0f; if ((Mute & 4) != 0) air = 0f; if ((Mute & 8) != 0) br = 0f;
                    if ((Mute & 16) != 0) pres = 0f; if ((Mute & 32) != 0) joint = 0f; if ((Mute & 64) != 0) hum = 0f;
                }
                float mono = motor + inv + br + pres + joint + hum;
                double hw = HushWeight(m_alt);
                if (hw > 0.0) mono *= (float)BwAudioSpec.Db(-HushDb * hw);
                outL += m_mix * (mono + air);
                outR += m_mix * (mono + 0.92f * air);
            }

            // ---- cues (doors, coupling, motor start) ring out even while the mix fades
            if (m_doorN < (int)(1.2 * fs))
            {
                double t = m_doorN / fs;
                m_phThud += 45.0 / fs; if (m_phThud >= 1.0) m_phThud -= 1.0;
                double att = t < 0.004 ? t / 0.004 : 1.0;
                float thud = (float)(att * Math.Exp(-6.9 * t / 0.4)) * SineTable.Lookup(m_phThud);
                float click = t > 0.18 && t < 0.182 ? m_doorClickLp.Low(m_rng.NextBipolar()) * 3f : 0f;
                float hiss = t < 1.0 ? m_hissHp.High(m_pink3.Next(m_rng.NextBipolar())) * (float)(0.35 * Math.Sin(Math.PI * t)) : 0f;
                float x = (thud + click + hiss) * (float)BwAudioSpec.Db(DoorPeakDb);
                outL += x; outR += x;
                m_doorN++;
            }
            if (m_coupleN < (int)(2.2 * fs))
            {
                double t = m_coupleN / fs;
                float strike = m_coupleN == 0 ? 1f : 0f;
                float modes = m_k0.Tick(strike) + m_k1.Tick(strike) + m_k2.Tick(strike);
                m_phThud += 60.0 / fs; if (m_phThud >= 1.0) m_phThud -= 1.0;
                float thud = (float)(Math.Exp(-6.9 * t / 0.5)) * SineTable.Lookup(m_phThud);
                float hiss = t > 0.2 && t < 2.0 ? m_hissHp.High(m_pink3.Next(m_rng.NextBipolar())) * (float)(0.3 * Math.Sin(Math.PI * (t - 0.2) / 1.8)) : 0f;
                float x = (0.8f * modes + thud + hiss) * (float)BwAudioSpec.Db(CouplingPeakDb);
                outL += x; outR += x;
                m_coupleN++;
            }
            if (m_startN < (int)(0.6 * fs))
            {
                double t = m_startN / fs;
                float strike = m_startN == 0 ? 0.6f : 0f;
                float modes = m_k0.Tick(strike) + 0.5f * m_k1.Tick(strike);
                float x = modes * (float)(BwAudioSpec.Db(MotorStartDb) * Math.Exp(-6.9 * t / 0.6));
                outL += x; outR += x;
                m_startN++;
            }

            // ---- the foot: wind, far jets, crickets
            if (m_foot > 1e-5f || m_jets > 1e-5f)
            {
                float w1 = m_rng.NextBipolar(), w2 = m_rng.NextBipolar();
                float gust = 0.75f + 0.25f * (float)Math.Sin(2.0 * Math.PI * 0.11 * m_t);
                float wl = m_footBpL.BandNorm(m_pink2.Next(w1)) * 3.75f * gust, wr = m_footBpR.BandNorm(m_pink3.Next(w2)) * 3.75f * gust;
                float fw = (float)(m_foot * BwAudioSpec.Db(FootWindDb));
                outL += fw * wl; outR += fw * wr;
                float jet = m_jetsLp.Low(m_brown2.Next(w1 * 0.5f + w2 * 0.5f)) * (float)(m_jets * 1.0);
                outL += jet; outR += 0.9f * jet;
                float cg = (float)(m_foot * BwAudioSpec.Db(CricketsDb) * 38.0);   // BP Q 8 of white (RMS 0.11) x chirp duty 0.21 x envelope 0.71
                if (cg > 1e-7f)
                {
                    for (int i = 0; i < 5; i++)
                    {
                        if (m_t >= m_crNext[i]) { m_crN[i] = 0; m_crNext[i] = m_t + 1.2 + 1.8 * m_rng.Next01(); }
                        int n = m_crN[i];
                        if (n < (int)(0.21 * fs))
                        {
                            // 3 chirps of 30 ms, 40 ms apart
                            double tc = n / fs;
                            int k = (int)(tc / 0.07);
                            double tin = tc - 0.07 * k;
                            if (k < 3 && tin < 0.03)
                            {
                                float env = (float)Math.Sin(Math.PI * tin / 0.03);
                                float c = m_crickBp.BandNorm(m_rng.NextBipolar()) * env * cg;
                                float pl = (float)Math.Sqrt(0.5 * (1.0 - m_crPan[i])), pr = (float)Math.Sqrt(0.5 * (1.0 + m_crPan[i]));
                                outL += c * pl; outR += c * pr;
                            }
                            m_crN[i] = n + 1;
                        }
                    }
                }
            }
            l += outL; r += outR;
        }

        public void Flush()
        {
            m_airBp.Flush(); m_whistle.Flush(); m_presLp.Flush(); m_brakeHp.Flush(); m_jointLp.Flush(); m_footBpL.Flush(); m_footBpR.Flush();
            m_jetsLp.Flush(); m_hissHp.Flush(); m_doorClickLp.Flush(); m_crickBp.Flush();
            m_j0.Flush(); m_j1.Flush(); m_j2.Flush(); m_k0.Flush(); m_k1.Flush(); m_k2.Flush();
            m_pink.Flush(); m_pink2.Flush(); m_pink3.Flush();
        }
    }

    /// <summary>The one-shot cues of a phase change (AudioDirector and the offline renderer use the same rule).</summary>
    public static class RideCues
    {
        /// <summary>Cue fired when the ride goes from phase prev to phase next (-1 = none).</summary>
        public static int ForTransition(int prev, int next)
        {
            if (next == prev) return -1;
            switch (next)
            {
                case 1: return (int)RideCue.DoorsClose;
                case 2: return (int)RideCue.MotorStart;
                case 3: return (int)RideCue.Coupling;
                case 4: return (int)RideCue.MotorStart;
                case 5: return (int)RideCue.DoorsOpen;
                default: return -1;
            }
        }
    }

    /// <summary>Mirror of EndingRidePhase as ints (the audio thread never touches the Core contract's statics).</summary>
    public enum EndingRidePhaseAudio { None = 0, Boarding = 1, Descent = 2, Transfer = 3, Shuttle = 4, Arrived = 5 }
}
