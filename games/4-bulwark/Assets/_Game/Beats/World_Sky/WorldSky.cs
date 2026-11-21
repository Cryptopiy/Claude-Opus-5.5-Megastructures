// WorldSky: the sky dome and the analytic far field of the whole walk (MASTER_PROMPT 14.7 H4, 14.2.2, 14.2.6 BwFarField).
// One camera-following dome: a unit cube drawn by Megastructure/BwFarField (material created at runtime from Shader.Find;
// the shader is in Always Included Shaders through BwPaths.ShaderNames discovery). The vertex shader centres the cube on
// each rendering camera (_WorldSpaceCameraPos) and forces its depth to the far plane, so the dome needs no per-frame
// update, follows capture cameras exactly and ignores the camera far plane (14.6); the mesh bounds are 200 km so it is
// never frustum-culled. All far content (sky, sun, clouds, lowland, rivers, mist rivers, city lights, sea beyond the
// clipmap, lee face, crest strip, outlet jets) is per-pixel maths on the AtmosphereDriver globals: nothing to build here.
// The graybox stand-in sky sphere (gb_sky) is switched off when it exists (GrayboxPath skips it once this type exists).
// Key-shot subjects (11.13): the sun disc at KS08 must stay out of frame (checked by the lead), at KS20 inside.
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Megastructure.Beats
{
    public sealed class WorldSky : IBeatBuilder, IKeyShotSubjects
    {
        public string Name => "World_Sky";
        public const string ShaderName = "Megastructure/BwFarField";
        public const float DomeBoundsSize = 200000f;
        /// <summary>Distance at which the sun-disc subject box is placed (a direction at infinity, projected like a disc).</summary>
        public const float SunSubjectDistance = 10000f;

        public static Material DomeMaterial { get; private set; }
        public static MeshRenderer Dome { get; private set; }

        public void Build(WorldContext ctx)
        {
            var root = new GameObject(Name).transform;
            root.SetParent(ctx.Root, false);
            var shader = Shader.Find(ShaderName);
            if (shader == null)
            {
                Debug.LogError("[BW-FAIL] WorldSky: shader " + ShaderName + " not found (run RebuildAll so it is in Always Included Shaders)");
                return;
            }
            var mat = new Material(shader) { name = "BwFarField (runtime)", enableInstancing = false };
            DomeMaterial = mat;

            var mb = new MeshBuilder();
            mb.Box(Vector3.zero, new Vector3(2f, 2f, 2f));
            var mesh = mb.ToMesh("bw_far_dome");
            mesh.bounds = new Bounds(Vector3.zero, Vector3.one * DomeBoundsSize);
            var mr = ctx.AddMesh(root, "bw_far_dome", mesh, mat, castShadows: false, receiveShadows: false);
            mr.shadowCastingMode = ShadowCastingMode.Off;
            mr.motionVectorGenerationMode = MotionVectorGenerationMode.ForceNoMotion;
            Dome = mr;

            DisableGrayboxSky(ctx);
            ctx.PrewarmSets.Add(new PrewarmSet
            {
                Name = "FarField", Eye = new Vector3(-4.5f, 1.7f, 230f), YawDeg = -75f, PitchDeg = -30f, Enable = null,
            });
            ctx.RegisterSubjects(this);
            ctx.Log("WorldSky: far-field dome (" + ShaderName + ", queue " + mat.renderQueue + ")");
        }

        static void DisableGrayboxSky(WorldContext ctx)
        {
            if (ctx.Root == null) return;
            var all = ctx.Root.GetComponentsInChildren<MeshRenderer>(true);
            for (int i = 0; i < all.Length; i++)
            {
                if (all[i] != null && all[i].gameObject.name == "gb_sky" && all[i].enabled)
                {
                    all[i].enabled = false;
                    ctx.Log("WorldSky: graybox stand-in sky sphere gb_sky disabled (the far-field dome replaces it)");
                }
            }
        }

        public void CollectSubjects(string shotId, List<KeyShotSubject> into)
        {
            if (shotId != "KS20") return;
            var ks = BulwarkConst.KeyShots;
            for (int i = 0; i < ks.Length; i++)
            {
                if (!ks[i].tag.StartsWith("KS20", System.StringComparison.Ordinal)) continue;
                var p = BulwarkSchedule.Evaluate(ks[i].s);
                var eye = new Vector3((float)ks[i].x, (float)ks[i].y, (float)ks[i].z);
                float size = 2f * SunSubjectDistance * (float)BwAtmosphereMath.SunDiscRadiusRad;
                into.Add(new KeyShotSubject("sun_disc", new Bounds(eye + p.SunDir * SunSubjectDistance, Vector3.one * size)));
            }
        }
    }
}
