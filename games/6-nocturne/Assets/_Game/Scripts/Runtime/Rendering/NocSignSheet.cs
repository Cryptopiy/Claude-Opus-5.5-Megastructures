// NocSignSheet: a look-development sheet for NocNeon (MASTER_PROMPT 14.7 H2, 14.2.5) as an IBeatBuilder that builds only with the
// flag -sign-sheet: blades of 14 x 50 m at 100, 200, 400, 800 and 1 600 m (glyph strokes resolved -> flat colour), near signs of
// 0.5 x 2 m and 2 x 1.2 m (strokes above 6 px, the 1.75 px floor) and five 1 m signs at 1 km (0.8 px true: drawn at the 2.2 px clamp
// with energy x (true / drawn)^2), all north of the probe eye over the Gulf where nothing else stands. Capture:
//   -sign-sheet -capture <dir> -capture-set pose:signs:300:1.7:195:0:5:66 -freeze-clock
// The signs go through the same per-camera InstancedBatch path as every sign field (NocCameraBatches).
using UnityEngine;

namespace Megastructure
{
    public sealed class NocSignSheet : IBeatBuilder
    {
        public const string Flag = "-sign-sheet";
        public string Name => "ZZ_SignSheet";

        public void Build(WorldContext ctx)
        {
            if (!CliArgs.Has(Flag) || ctx.Materials == null || ctx.Materials.Neon == null) return;
            var b = new InstancedBatch("SignSheet", NocTraffic.PointQuad(), ctx.Materials.Neon, chunkSize: 100000f);
            const float ex = 300f, ez = 195f, eye = 1.7f, tanH = 1.155f, tilt = 0.087f;
            for (int k = 0; k < 5; k++)
            {
                float d = 100f * (1 << k), ndc = -0.7f + 0.35f * k;
                b.Add(Matrix4x4.TRS(new Vector3(ex + ndc * tanH * d, eye + d * (tilt + 0.12f), ez + d), Quaternion.identity, new Vector3(14f, 50f, 1f)));
            }
            b.Add(Matrix4x4.TRS(new Vector3(ex - 1.4f, 2.8f, ez + 6f), Quaternion.Euler(0f, 20f, 0f), new Vector3(0.5f, 2.0f, 1f)));
            b.Add(Matrix4x4.TRS(new Vector3(ex + 3.0f, 2.4f, ez + 15f), Quaternion.identity, new Vector3(2.0f, 1.2f, 1f)));
            for (int k = 0; k < 5; k++)
                b.Add(Matrix4x4.TRS(new Vector3(ex + (-0.2f + 0.1f * k) * tanH * 1000f, eye + 1000f * tilt - 60f, ez + 1000f), Quaternion.identity, Vector3.one));
            NocCameraBatches.Register(b);
            ctx.Log("sign sheet built: " + b.Count + " NocNeon instances");
        }
    }
}
