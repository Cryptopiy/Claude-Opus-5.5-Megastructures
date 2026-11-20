// NoodleBar: the warm light of beat 1 (MASTER_PROMPT 14.5.1 NoodleBar, KS01 alley_noodle; dimensions in AlleyLayout).
// The west wall stands at x -3.0 over the 6.0 m flat of the profile (s 36-56), so the recess x -3.0..-2.2, z 40..52 is the bar's
// frontage. In the wall: an opening z 43..47 (4.0 m) up to y 2.3; in it the counter 4.0 x 0.9 x 1.0 m (front 3 cm behind the wall
// plane, so the capsule that touches the wall at lane + 0.35 never enters it; its top is the sill of the 4.0 x 1.3 m window
// y 1.0-2.3), behind it the lit room 1.2 m deep (back wall WinWarm x 1.2, side walls WinWarm x 0.9: the warm window; at the
// grazing KS01 view a 2.2 m room showed only its far side wall, unlit), ceiling Concrete lit by the bar light WinWarm r 9 i 7 at
// (-2.0, 2.2, 45.5) (14.2.3). Awning y 2.6-3.0 over the counter, 0.8 m out (above the camera sphere, y 1.9); sign 6.0 x 1.4 m
// at y 3.0-4.4 (NocNeon amber, three glyph cells rotated along the sign: no real text), 6 cm proud.
// KS01 (s 36, yaw -14, pitch +4, vFOV 62): the opening projects to x -0.15..-0.02, y -0.27..+0.02; the counter front y -0.50..-0.22.
using UnityEngine;

namespace Megastructure
{
    public static class NoodleBar
    {
        /// <summary>LOOK_V2 6 beat 1 (PA): the bar light WinWarm r 9 i 6 (v1 i 7).</summary>
        public const float BarLightIntensityV2 = 6f;
        public static void Build(G1Parts parts)
        {
            var origin = new Vector3(-3f, 0f, 45f);
            float xw = -(float)AlleyLayout.HalfWidth(45.0);                       // -3.0 (the wall plane)
            float xr = xw - (float)AlleyLayout.RoomDepth;
            float z0 = (float)AlleyLayout.CounterZ0, z1 = (float)AlleyLayout.CounterZ1;
            float y1 = (float)AlleyLayout.WindowY1, yc = (float)AlleyLayout.CounterH;

            // room interior (faces inward): the lit room IS the warm window (WinWarm x 1.2 back wall, x 0.9 side walls: at KS01's
            // grazing view the far side wall z 47 fills most of the opening), the ceiling Concrete lit by the bar light
            var room = parts.Get("barRoom", G1Mat.TileWall, origin);              // Look v2: wall tiles (v1: Concrete)
            var sides = parts.Get("barRoomLit", G1Mat.EmitShopWarm, origin);
            G1Mesh.Quad(sides, new Vector3(xr, 0f, z0), new Vector3(xr, y1, z0), new Vector3(xw, y1, z0), new Vector3(xw, 0f, z0), Vector3.forward);
            G1Mesh.Quad(sides, new Vector3(xr, 0f, z1), new Vector3(xr, y1, z1), new Vector3(xw, y1, z1), new Vector3(xw, 0f, z1), Vector3.back);
            G1Mesh.Quad(room, new Vector3(xr, y1, z0), new Vector3(xw, y1, z0), new Vector3(xw, y1, z1), new Vector3(xr, y1, z1), Vector3.down);

            // the warm window: the room's back wall (emissive WinWarm x 1.2)
            var warm = parts.Get("barWindow", G1Mat.EmitWarm, origin);
            G1Mesh.Quad(warm, new Vector3(xr, 0f, z0), new Vector3(xr, y1, z0), new Vector3(xr, y1, z1), new Vector3(xr, 0f, z1), Vector3.right);

            // counter: front, top and back (its ends meet the room's side walls; its bottom sits on the floor)
            var counter = parts.Get("barCounter", G1Mat.Wood, origin);            // Look v2: a wood counter (v1: SteelPainted)
            float cf = xw - (float)AlleyLayout.CounterRecess, cb = cf - (float)AlleyLayout.CounterDepth;
            G1Mesh.Box(counter, new Vector3(cb, 0f, z0), new Vector3(cf, yc, z1), G1Mesh.PX | G1Mesh.PY | G1Mesh.NX);
            // counter top edge trim (brass-like steel band, 2 cm proud of the front, bevelled)
            G1Mesh.Chamfer(counter, new Vector3(cf - 0.04f, yc - 0.05f, z0 + 0.02f), new Vector3(cf + 0.02f, yc + 0.012f, z1 - 0.02f), 0.01f);

            // awning over the frontage (bevelled box, 0.8 m out from the wall)
            var awn = parts.Get("barAwning", G1Mat.SteelPainted, origin);
            G1Mesh.Chamfer(awn, new Vector3(xw - 0.05f, (float)AlleyLayout.AwningY0, (float)AlleyLayout.AwningZ0),
                new Vector3(xw + (float)AlleyLayout.AwningOut, (float)AlleyLayout.AwningY1, (float)AlleyLayout.AwningZ1), 0.02f);

            // sign: NocNeon amber, W 1.4 (vertical) x H 6.0 (along z), facing +x (local X -> world up, local Y -> world +z)
            float sy = 0.5f * (float)(AlleyLayout.SignY0 + AlleyLayout.SignY1), sz = 0.5f * (float)(AlleyLayout.SignZ0 + AlleyLayout.SignZ1);
            parts.Sign(new Vector3(xw + (float)AlleyLayout.SignOff, sy, sz), Quaternion.LookRotation(Vector3.right, Vector3.forward),
                (float)(AlleyLayout.SignY1 - AlleyLayout.SignY0), (float)(AlleyLayout.SignZ1 - AlleyLayout.SignZ0), 2);
            // sign back plate (steel, behind the glyph plane)
            G1Mesh.Chamfer(awn, new Vector3(xw - 0.02f, (float)AlleyLayout.SignY0 - 0.05f, (float)AlleyLayout.SignZ0 - 0.05f),
                new Vector3(xw + (float)AlleyLayout.SignOff - 0.02f, (float)AlleyLayout.SignY1 + 0.05f, (float)AlleyLayout.SignZ1 + 0.05f), 0.01f);

            parts.Light("barLight", new Vector3((float)AlleyLayout.BarLightX, (float)AlleyLayout.BarLightY, (float)AlleyLayout.BarLightZ),
                G1Materials.Lin(NocPal.WinWarm, 1f), (float)AlleyLayout.BarLightRange, BarLightIntensityV2);
        }

        /// <summary>Wet-floor sources of the bar (14.7 H4): the window strip and the sign, at their own s.</summary>
        public static void RegisterSources()
        {
            float xw = -(float)AlleyLayout.HalfWidth(45.0);
            NocWetRegistry.AddSource(new Vector3(xw - 0.2f, 1.65f, (float)AlleyLayout.CounterZ0 + 0.3f), new Vector3(xw - 0.2f, 1.65f, (float)AlleyLayout.CounterZ1 - 0.3f),
                0.65f, G1Materials.Lin(NocPal.WinWarm, G1Materials.WinWarmFactor * G1Materials.ShopSourceGain), 45.0);
            NocWetRegistry.AddSource(new Vector3(xw + 0.06f, 3.7f, (float)AlleyLayout.SignZ0 + 0.5f), new Vector3(xw + 0.06f, 3.7f, (float)AlleyLayout.SignZ1 - 0.5f),
                0.7f, G1Materials.Lin(NocPal.SodiumAmber, 0.5f), 45.0);
        }
    }
}
