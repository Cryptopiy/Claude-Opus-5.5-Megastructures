// Harness-only shims (never compiled by Unity): the one member of the rendering schedule that NocMath references.
namespace Megastructure
{
    public static class NocSchedule
    {
        public static double Fov(double s) => 66.0;
        public static double RainRho(double s) => NocMath.BeatBlend(NocturneConstants.RainRho, s);
    }
}
