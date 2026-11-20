// VendingMachines: three machines 1.8 x 0.75 x 0.7 m on the east wall, z 62-67 (MASTER_PROMPT 14.5.1), in a niche of the wall.
// The wall narrows there (6.0 -> 4.6 m over s 56-66), so a machine front at a fixed x 2.2 would stand 0.1-0.35 m inside the
// clear width and the camera sphere (lane + 0.2 m) would enter it at s 62. Each front therefore sits 16 mm behind the wall line at
// the machine's south edge (the widest point of its span; the screen is 1 cm proud of the front, so 6 mm stay to the capsule):
// x 2.54, 2.35, 2.32 (logged). Niche z 61.9..67.1, y 0..2.4, back wall
// 2 cm behind the deepest machine. Dim screens (NeonCyan x 0.35, 14.5.1 "vending screens"; no animation in Phase 3), one small
// NeonCyan light each (14.2.3: 3 vending lights), wet-floor sources at the screens.
using UnityEngine;

namespace Megastructure
{
    public static class VendingMachines
    {
        public const double NicheZ0 = 61.9, NicheZ1 = 67.1, NicheTop = 2.4, FrontGap = 0.016, BackGap = 0.02;
        public const float LightRange = 4.0f, LightIntensity = 1.2f;
        /// <summary>LOOK_V2 6 beat 1 (PA): the vending light and wet-source colour lerp(NeonCyan, SignWhite, 0.4).</summary>
        public static Color LightColour(float gain)
        {
            Color c = Color.Lerp(NocturnePalette.Linear(NocPal.NeonCyan), NocturnePalette.Linear(NocPal.SignWhite), 0.4f) * gain;
            c.a = 1f;
            return c;
        }

        /// <summary>South edge z of machine i (evenly over z 62..67).</summary>
        public static double MachineZ0(int i) => AlleyLayout.VendZ0 + i * (AlleyLayout.VendZ1 - AlleyLayout.VendZ0 - AlleyLayout.VendW) / (AlleyLayout.VendingCount - 1);

        /// <summary>Front face x of machine i: the wall line at its south edge + 5 mm (never inside the clear width).</summary>
        public static double FrontX(int i) => AlleyLayout.HalfWidth(MachineZ0(i)) + FrontGap;

        public static double NicheBackX()
        {
            double m = 0;
            for (int i = 0; i < AlleyLayout.VendingCount; i++) m = System.Math.Max(m, FrontX(i));
            return m + AlleyLayout.VendD + BackGap;
        }

        public static void Build(G1Parts parts)
        {
            var origin = new Vector3(2.5f, 0f, 64.5f);
            var niche = parts.Get("vendNiche", G1Mat.Concrete, origin);
            float xb = (float)NicheBackX(), top = (float)NicheTop, z0 = (float)NicheZ0, z1 = (float)NicheZ1;
            float xs = (float)AlleyLayout.HalfWidth(NicheZ0), xn = (float)AlleyLayout.HalfWidth(NicheZ1);
            G1Mesh.Quad(niche, new Vector3(xb, 0f, z0), new Vector3(xb, top, z0), new Vector3(xb, top, z1), new Vector3(xb, 0f, z1), Vector3.left);
            G1Mesh.Quad(niche, new Vector3(xs, 0f, z0), new Vector3(xs, top, z0), new Vector3(xb, top, z0), new Vector3(xb, 0f, z0), Vector3.forward);
            G1Mesh.Quad(niche, new Vector3(xn, 0f, z1), new Vector3(xn, top, z1), new Vector3(xb, top, z1), new Vector3(xb, 0f, z1), Vector3.back);
            G1Mesh.Quad(niche, new Vector3(xs, top, z0), new Vector3(xb, top, z0), new Vector3(xb, top, z1), new Vector3(xn, top, z1), Vector3.down);

            var body = parts.Get("vendBody", G1Mat.SteelPainted, origin);
            var screen = parts.Get("vendScreen", G1Mat.EmitScreen, origin);
            for (int i = 0; i < AlleyLayout.VendingCount; i++)
            {
                float za = (float)MachineZ0(i), zb = za + (float)AlleyLayout.VendW, xf = (float)FrontX(i);
                G1Mesh.Chamfer(body, new Vector3(xf, 0f, za), new Vector3(xf + (float)AlleyLayout.VendD, (float)AlleyLayout.VendH, zb), 0.02f);
                // screen 1 cm proud of the front face (x - 0.01), never co-planar
                float zm = 0.5f * (za + zb), hw = 0.5f * (float)AlleyLayout.VendScreenW;
                G1Mesh.Quad(screen, new Vector3(xf - 0.01f, (float)AlleyLayout.VendScreenY0, zm - hw), new Vector3(xf - 0.01f, (float)AlleyLayout.VendScreenY1, zm - hw),
                    new Vector3(xf - 0.01f, (float)AlleyLayout.VendScreenY1, zm + hw), new Vector3(xf - 0.01f, (float)AlleyLayout.VendScreenY0, zm + hw), Vector3.left);
                parts.Light("vendLight" + i, new Vector3(xf - 0.6f, 1.3f, zm), LightColour(1f), LightRange, LightIntensity);
            }
        }

        public static void RegisterSources()
        {
            for (int i = 0; i < AlleyLayout.VendingCount; i++)
            {
                float zm = (float)(MachineZ0(i) + 0.5 * AlleyLayout.VendW), xf = (float)FrontX(i) - 0.01f;
                var p = new Vector3(xf, 1.3f, zm);
                NocWetRegistry.AddSource(p, p, 0.35f, LightColour(G1Materials.ScreenFactor * G1Materials.ShopSourceGain), zm);
            }
        }
    }
}
