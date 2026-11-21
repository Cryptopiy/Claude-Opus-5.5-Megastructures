namespace Megastructure.UI
{
    /// <summary>
    /// Pure, Unity-free timeline curves of the title card, controls hint and ending (Part 9, 14.11).
    /// Every value is an envelope in 0..1; the UiDirector multiplies it by the element's alpha.
    /// Intro t = seconds since BwEvents.LoadComplete; ending te = seconds since BwEvents.GoalReached. Imported from The Ring; the
    /// numbers are the 14.11 timings (FrameworkConst).
    /// EditMode tests can sample these directly (no scene needed).
    /// </summary>
    public static class UiTimeline
    {
        // ---------------- intro (14.11 "Title") ----------------
        public const float HintFade = (float)FrameworkConst.HintFade;
        /// <summary>14.11: the hint is "gone within 0.4 s of the first movement"; 0.35 s leaves one frame of margin.</summary>
        public const float HintKill = (float)FrameworkConst.HintKill;
        /// <summary>End of the intro (hint out at 12.5 s).</summary>
        public const float IntroEnd = (float)(FrameworkConst.HintStart + FrameworkConst.HintDuration);

        /// <summary>Picture (and the audio bed) visibility: black 0-1.0 s, smoothstep 1.0 -> 3.5 s.</summary>
        public static float Picture(float t) => BwMath.Smoothstep((float)FrameworkConst.PictureIn0, (float)FrameworkConst.PictureIn1, t);

        /// <summary>14.11: "Black 0-1.0 s with the first wave impact heard at 0.5 s".</summary>
        public const float IntroImpact = (float)FrameworkConst.IntroImpactTime;
        /// <summary>Opening ramp of the impact bus (no click when it opens at LoadComplete).</summary>
        public const float ImpactBusRamp = 0.05f;

        /// <summary>The audio's own intro envelope for events (the wave impacts): the impact bus is open from t 0 (50 ms ramp), so
        /// the 0.5 s impact is heard over black while the bed follows <see cref="Picture"/> (14.3 / 14.11). Picture(0.5) = 0,
        /// ImpactBus(0.5) = 1.</summary>
        public static float ImpactBus(float t) => BwMath.Smoothstep(0f, ImpactBusRamp, t);

        /// <summary>Title envelope: in 1.4 -> 2.6 s, hold to 4.6 s, out 4.6 -> 6.0 s.</summary>
        public static float Title(float t) =>
            BwMath.Smoothstep((float)FrameworkConst.TitleIn0, (float)FrameworkConst.TitleIn1, t) *
            (1f - BwMath.Smoothstep((float)FrameworkConst.TitleHold, (float)FrameworkConst.TitleOut, t));

        /// <summary>Hint envelope before gating: 6.5 -> 7.1 in, 11.9 -> 12.5 out (6 s including both 0.6 s fades).</summary>
        public static float Hint(float t)
        {
            float a = (float)FrameworkConst.HintStart, b = (float)(FrameworkConst.HintStart + FrameworkConst.HintDuration);
            return BwMath.Smoothstep(a, a + HintFade, t) * (1f - BwMath.Smoothstep(b - HintFade, b, t));
        }

        // ---------------- ending (14.11 "Ending") ----------------

        /// <summary>Picture fade-out starts after the 3.0 s dwell.</summary>
        public const float EndFadeStart = (float)FrameworkConst.EndDwell;
        /// <summary>Picture fully black (3.0 + 4.0 = 7.0 s).</summary>
        public const float EndFadeEnd = EndFadeStart + (float)FrameworkConst.EndFade;
        /// <summary>End line starts fading in after 1.5 s of black (8.5 s).</summary>
        public const float EndLineIn0 = EndFadeEnd + (float)FrameworkConst.EndBlack;
        /// <summary>End line fully visible (10.5 s).</summary>
        public const float EndLineIn1 = EndLineIn0 + (float)FrameworkConst.EndLineIn;
        /// <summary>End line starts fading out after the 6.0 s hold (16.5 s).</summary>
        public const float EndLineOut0 = EndLineIn1 + (float)FrameworkConst.EndLineHold;
        /// <summary>End line gone; BwEvents.EndingComplete is raised (18.5 s).</summary>
        public const float EndTotal = EndLineOut0 + (float)FrameworkConst.EndLineOut;

        /// <summary>Picture (and audio) visibility during the ending: 1 until 3.0 s, smootherstep to 0 at 7.0 s.</summary>
        public static float EndPicture(float te) => 1f - BwMath.Smootherstep(EndFadeStart, EndFadeEnd, te);

        /// <summary>End-line envelope: smoothstep in 8.5 -> 10.5 s, hold, smoothstep out 16.5 -> 18.5 s.</summary>
        public static float EndLine(float te) =>
            BwMath.Smoothstep(EndLineIn0, EndLineIn1, te) * (1f - BwMath.Smoothstep(EndLineOut0, EndTotal, te));
    }
}
