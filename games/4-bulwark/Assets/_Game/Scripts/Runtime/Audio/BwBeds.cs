// BwBeds: the bed layers of the 14.3 sound bible as allocation-free generators (Unity-free). BulwarkScore sets every
// layer's linear gain as a pure function of s (gain 1 = 0 dBFS RMS: each generator is normalised to unit RMS per channel,
// AudioTests.Beds_LayersAreUnitRms); BwDsp mixes them before the footsteps, the DC block and the room reverb.
//   W wind     pink -> BP windHz (Q 0.7, +-30 % LFO 0.06 Hz) + BP 1100 Hz at -9 dB, gusts 0.13 Hz, pan, optional LP (beat 5)
//   S sea      brown -> LP seaLp (700 Hz; beat 1 250 Hz) under a slow swell + pink HP 1.5 kHz wash at -14 dB, pan to the sea
//   F falls    brown LP 1.1 kHz + pink BP 300 Hz + sub 38 Hz (-8 dB)
//   T room     brown LP 160 Hz (beat 1); ballast sines 100 + 200 Hz (beat 5)
//   P pressure sines 31.0 and 46.5 Hz with slow FM (0.03 Hz, +-3 Hz) doubled by 93 / 139.5 Hz (-14 dB: heard on small speakers)
//   C conduit  brown LP 500 Hz + pink BP sweeping 120-900 Hz + sub 60 Hz
//   H city hum distant mains hum 50/100/150 Hz + brown LP 120 Hz (the ledge, -52)
using System;

namespace Megastructure.Audio
{
    public sealed class BwBeds
    {
        // unit-RMS normalisations (measured with the offline harness, AudioTests.Beds_LayersAreUnitRms)
        public const float NormWind = 2.333f, NormSea = 1.093f, NormFalls = 0.909f, NormRoom = 1.040f, NormBallast = 1.265f, NormPressure = 1.083f,
                           NormConduit = 0.948f, NormCity = 1.126f;

        readonly double m_fs;
        XorShift32 m_rng;
        PinkKellet m_pkL, m_pkR, m_pk2L, m_pk2R;
        BrownNoise m_brL, m_brR, m_br2L, m_br2R;
        SvfTpt m_wBpL, m_wBpR, m_w2L, m_w2R, m_wLpL, m_wLpR;
        SvfTpt m_sLpL, m_sLpR, m_sHpL, m_sHpR;
        SvfTpt m_fLpL, m_fLpR, m_fBpL, m_fBpR;
        SvfTpt m_tLpL, m_tLpR;
        SvfTpt m_cLpL, m_cLpR, m_cBpL, m_cBpR;
        SvfTpt m_hLpL, m_hLpR;
        double m_t, m_phF, m_phB1, m_phB2, m_phP1, m_phP2, m_phC, m_phH;
        // smoothed gains
        float m_w, m_s, m_f, m_room, m_ball, m_p, m_c, m_h, m_wPan, m_sPan;
        readonly float m_sm;
        // per-block filter controls
        float m_windHz = -1f, m_windLp = -1f, m_seaLp = -1f, m_gust = 0.86f, m_swell = 0.88f;
        double m_fm;
        int m_ctl;

        public BwBeds(double fs, int seed)
        {
            m_fs = fs;
            m_rng = XorShift32.Stream(seed, 0xBEDu);
            m_brL.Init(20.0, fs); m_brR.Init(20.0, fs); m_br2L.Init(20.0, fs); m_br2R.Init(20.0, fs);
            m_w2L.Set(1100.0, 1.0, fs); m_w2R.Set(1130.0, 1.0, fs);
            m_sHpL.Set(1500.0, 0.7, fs); m_sHpR.Set(1500.0, 0.7, fs);
            m_fLpL.Set(1100.0, 0.7, fs); m_fLpR.Set(1100.0, 0.7, fs); m_fBpL.Set(300.0, 0.8, fs); m_fBpR.Set(310.0, 0.8, fs);
            m_tLpL.Set(160.0, 0.7, fs); m_tLpR.Set(160.0, 0.7, fs);
            m_cLpL.Set(500.0, 0.7, fs); m_cLpR.Set(500.0, 0.7, fs);
            m_hLpL.Set(120.0, 0.7, fs); m_hLpR.Set(120.0, 0.7, fs);
            m_sm = (float)(1.0 - Math.Exp(-1.0 / (0.05 * fs)));
        }

        public void Snap(in BwAudioParams p)
        {
            m_w = p.w; m_s = p.sea; m_f = p.falls; m_room = p.bed; m_ball = p.tonal; m_p = p.pad; m_c = p.conduit; m_h = p.voice;
            m_wPan = p.windPan; m_sPan = p.seaPan;
        }

        /// <summary>Per block (32 frames): filter coefficients that follow the score.</summary>
        public void Control(in BwAudioParams p)
        {
            double fs = m_fs;
            // the slow LFOs (gusts 0.13 / 0.047 Hz, the sea swell 11 / 7.3 s, the pressure FM 0.03 Hz) per block: a 32-frame step
            // of a 0.13 Hz LFO is 5e-5 of the level (inaudible), and it saves six sines per sample on the audio thread
            m_gust = (float)(0.90 + 0.07 * Math.Sin(2.0 * Math.PI * 0.13 * m_t) + 0.03 * Math.Sin(2.0 * Math.PI * 0.047 * m_t + 1.3));
            m_swell = (float)(0.91 + 0.06 * Math.Sin(2.0 * Math.PI * m_t / 11.0) + 0.03 * Math.Sin(2.0 * Math.PI * m_t / 7.3 + 0.7));
            m_fm = 3.0 * Math.Sin(2.0 * Math.PI * 0.03 * m_t);
            // wind band with the +-30 % LFO at 0.06 Hz (re-set every 4 blocks)
            if ((m_ctl++ & 3) == 0)
            {
                double hz = Math.Max(80.0, p.windHz) * (1.0 + 0.3 * Math.Sin(2.0 * Math.PI * 0.06 * m_t));
                if (Math.Abs(hz - m_windHz) > 0.5) { m_windHz = (float)hz; m_wBpL.Set(hz, 0.7, fs); m_wBpR.Set(hz * 1.03, 0.7, fs); }
                // conduit sweep 120-900 Hz (0.05 Hz)
                double cf = 120.0 * Math.Pow(7.5, 0.5 + 0.5 * Math.Sin(2.0 * Math.PI * 0.05 * m_t));
                m_cBpL.Set(cf, 0.9, fs); m_cBpR.Set(cf * 1.04, 0.9, fs);
            }
            float wl = p.windLp > 0f ? p.windLp : 20000f;
            if (Math.Abs(wl - m_windLp) > 1f) { m_windLp = wl; m_wLpL.Set(Math.Min(wl, 0.45 * fs), 0.7, fs); m_wLpR.Set(Math.Min(wl, 0.45 * fs), 0.7, fs); }
            float sl = p.seaLp > 0f ? p.seaLp : 700f;
            if (Math.Abs(sl - m_seaLp) > 1f) { m_seaLp = sl; m_sLpL.Set(sl, 0.7, fs); m_sLpR.Set(sl * 1.02, 0.7, fs); }
        }

        /// <summary>One stereo sample of the beds (added to l, r).</summary>
        public void Tick(in BwAudioParams p, ref float l, ref float r)
        {
            double fs = m_fs;
            m_t += 1.0 / fs;
            float sm = m_sm;
            m_w += sm * (p.w - m_w); m_s += sm * (p.sea - m_s); m_f += sm * (p.falls - m_f); m_room += sm * (p.bed - m_room);
            m_ball += sm * (p.tonal - m_ball); m_p += sm * (p.pad - m_p); m_c += sm * (p.conduit - m_c); m_h += sm * (p.voice - m_h);
            m_wPan += sm * (p.windPan - m_wPan); m_sPan += sm * (p.seaPan - m_sPan);
            float oL = 0f, oR = 0f;
            float wL = m_rng.NextBipolar(), wR = m_rng.NextBipolar();

            if (m_w > 1e-6f)
            {
                float pl = m_pkL.Next(wL), pr = m_pkR.Next(wR);
                float gust = m_gust;
                float xl = m_wBpL.BandNorm(pl) + 0.355f * m_w2L.BandNorm(pl);
                float xr = m_wBpR.BandNorm(pr) + 0.355f * m_w2R.BandNorm(pr);
                if (m_windLp < 15000f) { xl = m_wLpL.Low(xl); xr = m_wLpR.Low(xr); }
                float g = m_w * gust * NormWind;
                float panL = (float)Math.Sqrt(Math.Max(0.0, 1.0 - m_wPan)), panR = (float)Math.Sqrt(1.0 + Math.Min(1.0, m_wPan));
                oL += g * xl * panL; oR += g * xr * panR;
            }
            if (m_s > 1e-6f)
            {
                // the swell: two slow periods (11 s and 7.3 s) between 0.82 and 1.0 of the level (clock-driven: the 3 s RMS per s differs
                // by less than 1.5 dB between 1.5 and 2.5 m/s, T-AUDIO)
                float sw = m_swell;
                float bl = m_brL.Next(wL), br = m_brR.Next(wR);
                float xl = m_sLpL.Low(bl) + 0.2f * sw * m_sHpL.High(m_pk2L.Next(wR));
                float xr = m_sLpR.Low(br) + 0.2f * sw * m_sHpR.High(m_pk2R.Next(wL));
                float g = m_s * sw * NormSea;
                float panL = (float)Math.Sqrt(Math.Max(0.0, 1.0 - m_sPan)), panR = (float)Math.Sqrt(1.0 + Math.Min(1.0, m_sPan));
                oL += g * xl * panL; oR += g * xr * panR;
            }
            if (m_f > 1e-6f)
            {
                m_phF += 38.0 / fs; if (m_phF >= 1.0) m_phF -= 1.0;
                float sub = 0.4f * SineTable.Lookup(m_phF);
                float bl = m_br2L.Next(wL), br = m_br2R.Next(wR);
                float xl = m_fLpL.Low(bl) + 0.8f * m_fBpL.BandNorm(m_pk2L.Next(wL)) + sub;
                float xr = m_fLpR.Low(br) + 0.8f * m_fBpR.BandNorm(m_pk2R.Next(wR)) + sub;
                float g = m_f * NormFalls;
                oL += g * xl; oR += g * xr;
            }
            if (m_room > 1e-6f)
            {
                float g = m_room * NormRoom;
                oL += g * m_tLpL.Low(m_brL.Next(wR)); oR += g * m_tLpR.Low(m_brR.Next(wL));
            }
            if (m_ball > 1e-6f)
            {
                m_phB1 += 100.0 / fs; if (m_phB1 >= 1.0) m_phB1 -= 1.0;
                m_phB2 += 200.3 / fs; if (m_phB2 >= 1.0) m_phB2 -= 1.0;
                float x = (SineTable.Lookup(m_phB1) + 0.5f * SineTable.Lookup(m_phB2)) * m_ball * NormBallast;
                oL += x; oR += x;
            }
            if (m_p > 1e-6f)
            {
                double fm = m_fm;
                m_phP1 += (31.0 + fm) / fs; if (m_phP1 >= 1.0) m_phP1 -= 1.0;
                m_phP2 += (46.5 + 1.5 * fm) / fs; if (m_phP2 >= 1.0) m_phP2 -= 1.0;
                double h1 = m_phP1 * 3.0, h2 = m_phP2 * 3.0;
                float x = SineTable.Lookup(m_phP1) + 0.8f * SineTable.Lookup(m_phP2)
                        + 0.2f * SineTable.Lookup(h1 - Math.Floor(h1)) + 0.16f * SineTable.Lookup(h2 - Math.Floor(h2));
                x *= m_p * NormPressure;
                oL += x; oR += x;
            }
            if (m_c > 1e-6f)
            {
                m_phC += 60.0 / fs; if (m_phC >= 1.0) m_phC -= 1.0;
                float sub = 0.35f * SineTable.Lookup(m_phC);
                float xl = m_cLpL.Low(m_br2L.Next(wR)) + 0.8f * m_cBpL.BandNorm(m_pkL.Next(wR)) + sub;
                float xr = m_cLpR.Low(m_br2R.Next(wL)) + 0.8f * m_cBpR.BandNorm(m_pkR.Next(wL)) + sub;
                float g = m_c * NormConduit;
                oL += g * xl; oR += g * xr;
            }
            if (m_h > 1e-6f)
            {
                m_phH += 50.0 / fs; if (m_phH >= 1.0) m_phH -= 1.0;
                double h2 = m_phH * 2.0, h3 = m_phH * 3.0;
                float tone = 0.3f * SineTable.Lookup(m_phH) + 0.5f * SineTable.Lookup(h2 - Math.Floor(h2)) + 0.3f * SineTable.Lookup(h3 - Math.Floor(h3));
                float g = m_h * NormCity;
                oL += g * (tone + 0.8f * m_hLpL.Low(m_brL.Next(wL))); oR += g * (tone + 0.8f * m_hLpR.Low(m_brR.Next(wR)));
            }
            l += oL; r += oR;
        }

        public void Flush()
        {
            m_wBpL.Flush(); m_wBpR.Flush(); m_w2L.Flush(); m_w2R.Flush(); m_wLpL.Flush(); m_wLpR.Flush();
            m_sLpL.Flush(); m_sLpR.Flush(); m_sHpL.Flush(); m_sHpR.Flush(); m_fLpL.Flush(); m_fLpR.Flush(); m_fBpL.Flush(); m_fBpR.Flush();
            m_tLpL.Flush(); m_tLpR.Flush(); m_cLpL.Flush(); m_cLpR.Flush(); m_cBpL.Flush(); m_cBpR.Flush(); m_hLpL.Flush(); m_hLpR.Flush();
            m_pkL.Flush(); m_pkR.Flush(); m_pk2L.Flush(); m_pk2R.Flush();
        }

        public void Clear()
        {
            m_wBpL.Clear(); m_wBpR.Clear(); m_w2L.Clear(); m_w2R.Clear(); m_wLpL.Clear(); m_wLpR.Clear();
            m_sLpL.Clear(); m_sLpR.Clear(); m_sHpL.Clear(); m_sHpR.Clear(); m_fLpL.Clear(); m_fLpR.Clear(); m_fBpL.Clear(); m_fBpR.Clear();
            m_tLpL.Clear(); m_tLpR.Clear(); m_cLpL.Clear(); m_cLpR.Clear(); m_cBpL.Clear(); m_cBpR.Clear(); m_hLpL.Clear(); m_hLpR.Clear();
        }
    }
}
