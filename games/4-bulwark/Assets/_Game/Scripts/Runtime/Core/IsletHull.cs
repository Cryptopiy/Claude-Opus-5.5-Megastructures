namespace Megastructure
{
    /// <summary>Hulls of the six beat-4 islets for the kraken's 2 m clearance (Addendum A1 1.4, T-KRAKEN), backed by the reef
    /// layout (Beats/Beat04_ArmourSlope/ReefLayout.cs: arms as tapered capsules plus the hub sphere; negative inside).
    /// Lead contract (Wave X reef request); the reef owner keeps ReefLayout's signatures stable.</summary>
    public static class IsletHull
    {
        public static int Count => Megastructure.Beats.ReefLayout.IsletCount;
        public static UnityEngine.Vector2 Centre(int k) => Megastructure.Beats.ReefLayout.IsletCentre(k);
        public static float CrestY(int k) => Megastructure.Beats.ReefLayout.IsletCrestY[k];
        public static float Radius(int k) => Megastructure.Beats.ReefLayout.IsletRadius[k];
        public static float Distance(int k, UnityEngine.Vector3 p) => Megastructure.Beats.ReefLayout.IsletHullDistance(k, p);
        /// <summary>Distance to the nearest built unit of the whole reef (islets, belt, shoal).</summary>
        public static float DistanceAll(UnityEngine.Vector3 p) => Megastructure.Beats.ReefLayout.HullDistance(p);
    }
}
