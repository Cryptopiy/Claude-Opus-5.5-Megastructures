namespace Megastructure
{
    /// <summary>
    /// Pure, Unity-free timeline curves of the title card, controls hint and ending (Part 9, 14.11), imported from The Ring and
    /// re-timed for Nocturne (The Ring's ending was a 3.0 s dwell, 1.5 s black and 18.5 s in total). Every value is an envelope in
    /// 0..1; the UiDirector multiplies it by the element's alpha. Intro t = seconds since NocEvents.LoadComplete; ending te =
    /// seconds since the goal trigger (NocEvents.GoalReached). EditMode tests sample these directly.
    /// </summary>
    public static class UiTimeline
    {
        // ---------------- float easing (UI is float) ----------------
        public static float Clamp01(float x) => x < 0f ? 0f : (x > 1f ? 1f : x);
        public static float Ss(float e0, float e1, float x) { float t = Clamp01((x - e0) / (e1 - e0)); return t * t * (3f - 2f * t); }
        public static float Sss(float e0, float e1, float x) { float t = Clamp01((x - e0) / (e1 - e0)); return t * t * t * (t * (t * 6f - 15f) + 10f); }

        // ---------------- intro (14.11 "Title") ----------------
        /// <summary>14.11: hint "at 6.0 s for 6 s with 0.6 s fades" (the 6 s include both fades).</summary>
        public const float HintFade = (float)NocturneConstants.HintFade;
        /// <summary>14.11: the hint is "gone within 0.4 s of the first movement"; 0.35 s leaves one frame of margin.</summary>
        public const float HintKill = (float)NocturneConstants.HintGone - 0.05f;
        /// <summary>End of the intro (title out at 5.4 s, hint out at 12.0 s).</summary>
        public const float IntroEnd = (float)(NocturneConstants.HintAt + NocturneConstants.HintDur);

        /// <summary>Picture (and audio) visibility: black 0.0-0.6 s, smoothstep 0.6 -> 3.1 s.</summary>
        public static float Picture(float t) => Ss((float)NocturneConstants.FadeInStart, (float)NocturneConstants.FadeInEnd, t);

        /// <summary>Title envelope: in 0.8 -> 2.0 s, hold to 4.0 s, out 4.0 -> 5.4 s.</summary>
        public static float Title(float t) =>
            Ss((float)NocturneConstants.TitleIn0, (float)NocturneConstants.TitleIn1, t) *
            (1f - Ss((float)NocturneConstants.TitleOut0, (float)NocturneConstants.TitleOut1, t));

        /// <summary>Hint envelope before gating: 6.0 -> 6.6 in, 11.4 -> 12.0 out.</summary>
        public static float Hint(float t)
        {
            float a = (float)NocturneConstants.HintAt, b = (float)(NocturneConstants.HintAt + NocturneConstants.HintDur);
            return Ss(a, a + HintFade, t) * (1f - Ss(b - HintFade, b, t));
        }

        // ---------------- ending (14.11 "Ending and the goal object") ----------------
        /// <summary>Picture fade-out starts after 1.5 s of picture.</summary>
        public const float EndFadeStart = (float)NocturneConstants.EndPictureHold;
        /// <summary>Picture fully black (1.5 + 4.0 = 5.5 s); black to 7.0 s.</summary>
        public const float EndFadeEnd = (float)(NocturneConstants.EndPictureHold + NocturneConstants.EndFade);
        /// <summary>Audio fades over 6 s from the start of the picture fade (1.5 -> 7.5 s).</summary>
        public const float EndAudioEnd = (float)(NocturneConstants.EndPictureHold + NocturneConstants.EndAudioFade);
        /// <summary>End line fades in over 7.0 -> 9.0 s.</summary>
        public const float EndLineIn0 = (float)NocturneConstants.EndLineIn0;
        public const float EndLineIn1 = (float)(NocturneConstants.EndLineIn0 + NocturneConstants.EndTextIn);
        /// <summary>End line holds 6.0 s, fades out 15.0 -> 17.0 s.</summary>
        public const float EndLineOut0 = (float)(NocturneConstants.EndLineIn0 + NocturneConstants.EndTextIn + NocturneConstants.EndTextHold);
        /// <summary>End line gone; NocEvents.EndingComplete is raised (17.0 s).</summary>
        public const float EndTotal = (float)NocturneConstants.EndTotal;

        /// <summary>Picture visibility during the ending: 1 until 1.5 s, smootherstep to 0 at 5.5 s (the lamp stays lit until the fade ends).</summary>
        public static float EndPicture(float te) => 1f - Sss(EndFadeStart, EndFadeEnd, te);

        /// <summary>Audio gain during the ending: 1 until 1.5 s, smootherstep to 0 at 7.5 s.</summary>
        public static float EndAudio(float te) => 1f - Sss(EndFadeStart, EndAudioEnd, te);

        /// <summary>End-line envelope: smoothstep in 7.0 -> 9.0 s, hold, smoothstep out 15.0 -> 17.0 s.</summary>
        public static float EndLine(float te) => Ss(EndLineIn0, EndLineIn1, te) * (1f - Ss(EndLineOut0, EndTotal, te));
    }
}
