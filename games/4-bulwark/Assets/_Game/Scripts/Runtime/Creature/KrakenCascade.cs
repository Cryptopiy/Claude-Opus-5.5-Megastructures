// KrakenCascade - the water pouring off the emerged kraken (fix wave X2, S2-3 "no water pours off emerged parts"; kraken
// owner). Torn white-green ribbons pour off the head rim and the arm undersides while the part is out of the sea and wet, fall
// at g (BwCreatureCascade.shader: an 80 m drop takes 4 s), fray into a spray veil that drifts with the 9 m/s wind and thin to
// drip strings as the wetness falls after the emergence (the show's HeadWetness ramps: K4 1 -> 0.35 over s 786-806, K1
// 1 -> 0.4 over s 150-170, i.e. 6-8 s at 2.5 m/s). One static mesh of Slots x StrandsPerSlot strands built once; per frame
// only the 64 emitters (world position, rate, outward direction, fall time) go to the GPU as global arrays. Pure in the
// creature's state (no history), so stills, sequences and the walk agree. The renderer is a child of the kraken root (shown
// and hidden with it); the shader works in world space.
using UnityEngine;
using UnityEngine.Rendering;

namespace Megastructure.Creature
{
    public sealed class KrakenCascade
    {
        public const string ShaderName = "Megastructure/BwCreatureCascade";
        public const int Slots = 64, StrandsPerSlot = 3, Segments = 24, HeadSlots = 28, ArmSlotsPerLimb = 4;
        public const float Gravity = 9.81f;
        /// <summary>Wind of the storm that carries the spray veil (m/s, world): 9 m/s from the sea towards the dam.</summary>
        public static readonly Vector3 Wind = new Vector3(-7.8f, 0f, 4.5f);
        /// <summary>Lowest emitter height above the sea (m): below it the water simply runs into the sea.</summary>
        public const float MinDrop = 4f;

        static readonly int IdE = Shader.PropertyToID("_KrCascE");
        static readonly int IdO = Shader.PropertyToID("_KrCascO");
        static readonly int IdT = Shader.PropertyToID("_KrCascT");
        static readonly int IdW = Shader.PropertyToID("_KrCascW");
        readonly Vector4[] m_e = new Vector4[Slots], m_o = new Vector4[Slots], m_w = new Vector4[Slots];
        /// <summary>Sheet width scale of the head rim and the arm pours (x the strand's 1.5-5 m): kraken-fx2 read 1x as hair.</summary>
        public const float HeadWidth = 3.2f, ArmWidth = 1.6f;
        MeshRenderer m_mr;
        public int ActiveSlots { get; private set; }
        public int Triangles { get; private set; }

        /// <summary>Fall time from height h above the sea (s): sqrt(2 h / g).</summary>
        public static float FallTime(float h) => Mathf.Sqrt(2f * Mathf.Max(h, 0f) / Gravity);

        /// <summary>Pour rate 0..1 from the wetness the show sets (1 while it rises, ~0.35 once it has been out a while): torn
        /// ribbons at 1, drip strings (0.1) at the dry end, nothing below 0.25.</summary>
        public static float Rate(float wetness) => wetness < 0.25f ? 0f : Mathf.Lerp(0.1f, 1f, Mathf.Clamp01((wetness - 0.35f) / 0.65f));

        public static KrakenCascade Build(Transform parent)
        {
            var sh = Shader.Find(ShaderName);
            if (sh == null) { Debug.LogWarning("[BW] kraken cascade: shader " + ShaderName + " not found (RebuildAll adds it to Always Included)"); return null; }
            var c = new KrakenCascade();
            var mesh = BuildMesh();
            c.Triangles = (int)(mesh.GetIndexCount(0) / 3);
            var go = new GameObject("kraken_cascade");
            go.transform.SetParent(parent, false);
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            c.m_mr = go.AddComponent<MeshRenderer>();
            c.m_mr.sharedMaterial = new Material(sh) { name = "BwCreatureCascade" };
            c.m_mr.shadowCastingMode = ShadowCastingMode.Off;
            c.m_mr.receiveShadows = false;
            c.m_mr.lightProbeUsage = LightProbeUsage.Off;
            c.m_mr.reflectionProbeUsage = ReflectionProbeUsage.Off;
            c.m_mr.enabled = false;
            return c;
        }

        static Mesh BuildMesh()
        {
            int strands = Slots * StrandsPerSlot, vps = (Segments + 1) * 2;
            var pos = new Vector3[strands * vps];
            var uv = new Vector2[pos.Length];
            var d = new Vector4[pos.Length];
            var idx = new int[strands * Segments * 6];
            int vi = 0, ii = 0;
            for (int s = 0; s < strands; s++)
            {
                int slot = s / StrandsPerSlot, k = s % StrandsPerSlot;
                // strand k of a slot: seeds 0.15 / 0.5 / 0.85 jittered, so k = 0 stays on at the drip end
                float seed = Mathf.Clamp01((k + 0.5f) / StrandsPerSlot + 0.12f * (KrakenMath.Hash01((uint)(s * 7 + 3)) - 0.5f));
                float width = 0.7f + 0.6f * KrakenMath.Hash01((uint)(s * 13 + 5));
                float lateral = 2f * KrakenMath.Hash01((uint)(s * 31 + 11)) - 1f;
                int v0 = vi;
                for (int j = 0; j <= Segments; j++)
                {
                    float a = j / (float)Segments;
                    for (int side = 0; side < 2; side++)
                    {
                        pos[vi] = Vector3.zero;
                        uv[vi] = new Vector2(side == 0 ? -1f : 1f, a);
                        d[vi] = new Vector4(slot, seed, width, lateral);
                        vi++;
                    }
                }
                for (int j = 0; j < Segments; j++)
                {
                    int b = v0 + j * 2;
                    idx[ii++] = b; idx[ii++] = b + 2; idx[ii++] = b + 1;
                    idx[ii++] = b + 1; idx[ii++] = b + 2; idx[ii++] = b + 3;
                }
            }
            var m = new Mesh { name = "kraken_cascade", indexFormat = UnityEngine.Rendering.IndexFormat.UInt32 };
            m.vertices = pos;
            m.SetUVs(0, uv);
            m.SetUVs(1, d);
            m.SetIndices(idx, MeshTopology.Triangles, 0);
            m.bounds = new Bounds(Vector3.zero, Vector3.one * 1e5f);
            m.UploadMeshData(true);
            return m;
        }

        /// <summary>Per frame (KrakenCreature.Sync): the emitters on the emerged, wet parts; seaY the sea surface (world y).</summary>
        public void Update(KrakenRig rig, Bounds headLocal, float headWetness, double t, float seaY, Vector3 camPos)
        {
            if (m_mr == null) return;
            int n = 0;
            var M = rig.LocalToWorld;
            float rate = Rate(headWetness);
            var bounds = new Bounds();
            bool any = false;
            if (rate > 0f)
            {
                // head rim: an ellipse of the head bounds at 45 % of the emerged height, slightly inside the skin
                Vector3 c = M.MultiplyPoint3x4(headLocal.center);
                Vector3 ext = headLocal.extents;
                float top = c.y + ext.y * rig.Scale;
                if (top > seaY + MinDrop + 2f)
                {
                    float ye = seaY + 0.45f * (top - seaY);
                    float k = (ye - c.y) / Mathf.Max(ext.y * rig.Scale, 1e-3f);
                    float f = 0.82f * Mathf.Sqrt(Mathf.Clamp01(1f - k * k)) + 0.1f;
                    float emerged = Mathf.Clamp01((top - seaY) / 25f);
                    Vector3 toCam = camPos - c; toCam.y = 0f; toCam = toCam.sqrMagnitude > 1f ? toCam.normalized : Vector3.zero;
                    for (int i = 0; i < HeadSlots && n < Slots; i++)
                    {
                        float ang = 2f * Mathf.PI * (i + 0.5f + 0.3f * (KrakenMath.Hash01((uint)(i * 17 + 1)) - 0.5f)) / HeadSlots;
                        Vector3 off = M.MultiplyVector(new Vector3(Mathf.Cos(ang) * ext.x * f, 0f, Mathf.Sin(ang) * ext.z * f));
                        off.y = 0f;
                        Vector3 p = new Vector3(c.x + off.x, ye, c.z + off.z);
                        Vector3 o = off.sqrMagnitude > 1e-4f ? off.normalized : Vector3.right;
                        // pours off the flanks and the back read as falling water; the ones facing the camera fall in front of
                        // the face as lines (kraken-fx2 show_k4_kk2): those keep 15 %
                        float facing = Mathf.Clamp01(Vector3.Dot(o, toCam));
                        Add(ref n, p, o, rate * emerged * (1f - 0.85f * facing * facing) * (0.7f + 0.3f * KrakenMath.Hash01((uint)(i * 5 + 9))), HeadWidth, seaY, ref bounds, ref any);
                    }
                }
                // arm undersides: up to 4 points per visible limb, spread along the emerged part
                for (int l = 0; l < KrakenRig.LimbCount && n < Slots; l++)
                {
                    var L = rig.Limbs[l];
                    if (L == null || !L.Visible) continue;
                    int placed = 0;
                    const int Samples = 28;
                    for (int j = 0; j < Samples && placed < ArmSlotsPerLimb && n < Slots; j++)
                    {
                        // a fixed stagger per limb so the emitters do not line up between arms
                        float a = (j + 0.5f + 0.4f * KrakenMath.Hash01((uint)(l * 101 + j))) / Samples;
                        float u = Mathf.Lerp(0.15f * L.Length, 0.9f * L.Length, a);
                        float r = L.RadiusAt(u) * rig.Scale;
                        if (r < 1.2f) continue;
                        Vector3 p = M.MultiplyPoint3x4(L.CurrentAt(u)) - Vector3.up * (0.8f * r);
                        if (p.y < seaY + MinDrop) continue;
                        if ((j + l) % 3 != 0) continue;              // every third sample: the pours spaced along the arm
                        Vector3 o = M.MultiplyVector(L.CurrentAt(Mathf.Min(u + 5f, L.Length)) - L.CurrentAt(u));
                        o = Vector3.Cross(Vector3.up, o); o.y = 0f;
                        o = o.sqrMagnitude > 1e-4f ? o.normalized : Vector3.right;
                        if ((j & 1) == 1) o = -o;
                        Add(ref n, p, o * 0.35f, rate * Mathf.Clamp01(r / 6f), ArmWidth * Mathf.Clamp(r / 8f, 0.5f, 1.5f), seaY, ref bounds, ref any);
                        placed++;
                    }
                }
            }
            for (int i = n; i < Slots; i++) { m_e[i] = Vector4.zero; m_o[i] = new Vector4(1f, 0f, 0f, 0f); m_w[i] = Vector4.one; }
            ActiveSlots = n;
            m_mr.enabled = n > 0;
            if (n == 0) return;
            Shader.SetGlobalVectorArray(IdE, m_e);
            Shader.SetGlobalVectorArray(IdO, m_o);
            Shader.SetGlobalVectorArray(IdW, m_w);
            double tw = t - 1000.0 * System.Math.Floor(t / 1000.0);
            Shader.SetGlobalVector(IdT, new Vector4((float)tw, Wind.x, Wind.y, Wind.z));
            bounds.Expand(new Vector3(60f, 10f, 60f));
            m_mr.bounds = bounds;
        }

        void Add(ref int n, Vector3 p, Vector3 outward, float rate, float width, float seaY, ref Bounds b, ref bool any)
        {
            if (rate <= 0.001f) return;
            float tf = FallTime(p.y - seaY);
            m_e[n] = new Vector4(p.x, p.y, p.z, Mathf.Clamp01(rate));
            m_w[n] = new Vector4(width, 0f, 0f, 0f);
            float ol = outward.magnitude;
            Vector3 od = ol > 1e-4f ? outward / ol : Vector3.right;
            m_o[n] = new Vector4(od.x * Mathf.Min(ol, 1f), od.y, od.z * Mathf.Min(ol, 1f), tf);
            // the strand's reach: down to the sea, out with v0 and the wind
            Vector3 foot = p + od * (3.4f * tf) + Wind * tf;
            foot.y = seaY;
            if (!any) { b = new Bounds(p, Vector3.zero); any = true; }
            b.Encapsulate(p); b.Encapsulate(foot);
            n++;
        }
    }
}
