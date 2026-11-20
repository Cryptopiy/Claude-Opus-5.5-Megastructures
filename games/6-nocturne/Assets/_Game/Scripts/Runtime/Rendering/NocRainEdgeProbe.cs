// NocRainEdgeProbe: the T-RAIN-EDGE scene of MASTER_PROMPT 8.6 / 14.7 H5 (R3 1.3, R2 1.10) as an IBeatBuilder that builds only
// with the flag -rain-edge-probe: a bright emissive panel 40 m north of the probe eye (so Lloc is high and the rain shows) and,
// 1.8 m in front of the eye (inside the 3 m layer), a 4 mm vertical post and two horizontal rails of 4 and 10 mm (1.85 and 4.6 px at
// 1080p, vFOV 66) in front of the panel.
// Capture it with rain on and off from the same pose:
//   -rain-edge-probe -capture <dir> -capture-set pose:rainedge:300:1.7:195:0:0:66 -freeze-clock -freeze-rain -rain 1 (and -rain 0)
// A per-sample occluded rain adds nothing to the post's interior pixels and never makes an edge pixel brighter than its
// uncovered neighbour (no halo); the analysis compares the two PNGs (docs/logs/A2b.md).
using UnityEngine;

namespace Megastructure
{
    public sealed class NocRainEdgeProbe : IBeatBuilder
    {
        public const string Flag = "-rain-edge-probe";
        public const float EyeX = 300f, EyeZ = 195f, PostDist = 1.8f, Thin = 0.004f;

        public string Name => "ZZ_RainEdgeProbe";

        public void Build(WorldContext ctx)
        {
            if (!CliArgs.Has(Flag)) return;
            var lib = ctx.Materials;
            if (lib == null) return;
            var root = new GameObject("RainEdgeProbe").transform;
            root.SetParent(ctx.CityRoot, false);
            var panel = new MeshBuilder();
            MeshKit.ChamferBox(panel, new Vector3(EyeX, 12f, EyeZ + 40f), new Vector3(60f, 24f, 0.5f), 0.02f);
            var pr = ctx.AddMesh(root, "panel", panel.ToMesh("RainProbePanel"), lib.GrayMarker);
            // a mid-grey cyan panel (linear luma ~0.17): rain is gated by Lloc and compressed by the tone curve near white
            var mpb = new MaterialPropertyBlock();
            mpb.SetVector("_EmissionColor", new Vector4(0.05f, 0.18f, 0.25f, 1f));
            pr.SetPropertyBlock(mpb);
            var post = new MeshBuilder();
            MeshKit.ChamferBox(post, new Vector3(EyeX + 0.05f, 1.7f, EyeZ + PostDist), new Vector3(Thin, 3.0f, Thin), 0.0005f);
            MeshKit.ChamferBox(post, new Vector3(EyeX, 2.2f, EyeZ + PostDist + 0.02f), new Vector3(3.0f, Thin, Thin), 0.0005f);
            MeshKit.ChamferBox(post, new Vector3(EyeX, 2.6f, EyeZ + PostDist + 0.02f), new Vector3(3.0f, 2.5f * Thin, Thin), 0.0005f);
            ctx.AddMesh(root, "post", post.ToMesh("RainProbePost"), lib.GrayWall);
            ctx.Log("rain edge probe built (T-RAIN-EDGE): panel 40 m, 4 mm post, 4 and 10 mm rails at 1.8 m");
        }
    }
}
