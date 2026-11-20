// NocHalos: the in-air light of LOOK_V2 6 ("Light in the air"; owner PA). Every LampWarm / SignWhite lamp head gets an additive,
// camera-facing in-scatter sprite: a Gaussian of sigma 0.5 m (lights of range <= 10 m) or 0.8 m (range >= 12 m) whose peak is
// 5 % of the head radiance x rho_eff, rho_eff = min(2, sigma_local / 4.4e-4) x (0.6 + 0.4 rho_rain) (twice as strong in the
// alley haze), faded out below 3 px of sigma, fogged like the head and drawn after the opaques without depth write. Bloom gives
// only lens halation; a photographed rainy night also shows the halo of lit rain in the air.
// The sprite is NocStreak's kind 3: one InstancedBatch per lamp family (<= 511 per draw, explicit bounds, NocCameraBatches), the
// head radiance as the material's _Tint, sigma in the instance matrix; rho_eff reads the fog and rain globals in the shader, so
// the halos follow the atmosphere every frame. Beat builders Queue their head positions while they build (they need no material
// library); HaloBuilder (an IBeatBuilder named "ZZZ_Halos", so it runs after every beat and sheet) turns the queue into batches.
using System.Collections.Generic;
using UnityEngine;

namespace Megastructure
{
    public static class NocHalos
    {
        sealed class Family
        {
            public string Name;
            public readonly List<Vector3> Heads = new List<Vector3>();
            public Color Radiance;
            public double Sigma;
        }

        static readonly List<Family> s_pending = new List<Family>(16);
        static readonly int s_kind = Shader.PropertyToID("_Kind");
        static readonly int s_gain = Shader.PropertyToID("_Gain");
        static readonly int s_tint = Shader.PropertyToID("_Tint");

        /// <summary>Halo heads queued and not yet built (tests).</summary>
        public static int PendingHeads { get { int n = 0; foreach (var f in s_pending) n += f.Heads.Count; return n; } }
        /// <summary>Halo heads turned into batches by the last Flush.</summary>
        public static int BuiltHeads { get; private set; }

        /// <summary>LOOK_V2 6: sigma 0.5 m for lights of range <= 10 m, 0.8 m from range 12 m (linear in between).</summary>
        public static double SigmaFor(double range)
        {
            double t = System.Math.Max(0.0, System.Math.Min(1.0, (range - 10.0) / 2.0));
            return NocturneConstants.HaloSigmaSmall + (NocturneConstants.HaloSigmaLarge - NocturneConstants.HaloSigmaSmall) * t;
        }

        /// <summary>Peak radiance of a halo of a head of radiance h at the local fog sigma (regional + alley haze, 1/m) and rain rho:
        /// 0.05 h min(2, sigma / 4.4e-4) (0.6 + 0.4 rho), the C# twin of NocStreak kind 3 before fog and the pixel fade.</summary>
        public static double Peak(double head, double sigmaLocal, double rho)
            => NocturneConstants.HaloPeak * head * System.Math.Min(2.0, sigmaLocal / NocturneConstants.HaloSigmaRef) * (0.6 + 0.4 * rho);

        /// <summary>Queues the halos of one lamp family: head centres (world), the head's emitted radiance (linear, e.g. LampWarm x
        /// 1.6) and the light's range (sets sigma). Families with the same name are merged.</summary>
        public static void Queue(string name, IList<Vector3> heads, Color headRadiance, double range)
        {
            if (heads == null || heads.Count == 0) return;
            Family f = null;
            foreach (var p in s_pending) if (p.Name == name) { f = p; break; }
            if (f == null) { f = new Family { Name = name, Radiance = headRadiance, Sigma = SigmaFor(range) }; s_pending.Add(f); }
            f.Heads.AddRange(heads);
        }

        /// <summary>Queues one head (convenience for builders that place lamps one by one).</summary>
        public static void Queue(string name, Vector3 head, Color headRadiance, double range)
        {
            Family f = null;
            foreach (var p in s_pending) if (p.Name == name) { f = p; break; }
            if (f == null) { f = new Family { Name = name, Radiance = headRadiance, Sigma = SigmaFor(range) }; s_pending.Add(f); }
            f.Heads.Add(head);
        }

        /// <summary>Builds one NocStreak kind-3 batch per queued family (needs lib.Beacon) and empties the queue.</summary>
        public static int Flush(MaterialLibrary lib)
        {
            BuiltHeads = 0;
            if (lib != null && lib.Beacon != null)
            {
                foreach (var f in s_pending)
                {
                    var m = new Material(lib.Beacon) { name = "Halo_" + f.Name };
                    m.SetFloat(s_kind, 3f);
                    m.SetFloat(s_gain, 1f);
                    m.SetVector(s_tint, new Vector4(f.Radiance.r, f.Radiance.g, f.Radiance.b, 1f));
                    var b = new InstancedBatch("Halo_" + f.Name, NocTraffic.PointQuad(), m, chunkSize: 120f);
                    foreach (var h in f.Heads) b.Add(NocTraffic.PointMatrix(h.x, h.y, h.z, f.Sigma, 0.0));
                    NocCameraBatches.Register(b);
                    BuiltHeads += f.Heads.Count;
                }
            }
            s_pending.Clear();
            return BuiltHeads;
        }

        /// <summary>Drops the queue (tests).</summary>
        public static void Clear() => s_pending.Clear();
    }

    /// <summary>Turns the halo queue into batches after every beat builder ran (builders are sorted by Name: "ZZZ_" is last).</summary>
    public sealed class HaloBuilder : IBeatBuilder
    {
        public string Name => "ZZZ_Halos";

        public void Build(WorldContext ctx)
        {
            int n = NocHalos.Flush(ctx.Materials);
            ctx.Log(System.FormattableString.Invariant($"halos: {n} lamp heads (LOOK_V2 6 light in the air, NocStreak kind 3)"));
        }
    }
}
