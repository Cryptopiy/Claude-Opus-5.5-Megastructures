// World-building contract shared by every beat builder (Assets/_Game/Beats/BeatNN_*), from Thread's WorldContext.
// GameBootstrap creates one WorldContext, calls each IBeatBuilder.Build once behind the black load fade, then
// ProgressDirector calls every registered IWorldUpdater once per frame (after the camera moved, before rendering).
// Updaters must not allocate. Builders put their content under a per-beat root (BeatRoot); the roots stay active for the whole
// walk (14.6: the city is one persistent set; the 8 m / 12 m window of BeatDirector.InEventWindow is for events only);
// camera-following far content goes under BackdropRoot. Tier = -tier N (0 = full; 1 = simplified, 2 = graybox: the fallback
// ladder of Part 13 / 14.8): builders that have tiers read it; the graybox builders ignore it.
using System.Collections.Generic;
using UnityEngine;

namespace Megastructure
{
    /// <summary>Per-frame state handed to updaters (derived from s in double, then cast).</summary>
    public struct FrameState
    {
        public double s;              // progress of the player/camera (m)
        public double time;           // scene clock (frozen in static tests, stepped by captures)
        public double rainTime;       // rain clock t_r
        public Vector3 cameraPos;
        public Vector3 cameraForward;
        public float renderHeight;    // pixel height of the current render target
        public float fovV;            // vertical FOV of the camera (deg)
        public int beat;              // 0..6
    }

    public interface IWorldUpdater
    {
        void OnFrame(in FrameState f);
    }

    public interface IBeatBuilder
    {
        /// <summary>Short name used in logs and for the build order, e.g. "Beat04_Skybridge".</summary>
        string Name { get; }
        /// <summary>Builds all geometry of the beat (runs once at load). Register updaters on ctx.</summary>
        void Build(WorldContext ctx);
    }

    public sealed class WorldContext
    {
        public MaterialLibrary Materials;
        /// <summary>Root of the static city content in true world coordinates (fixed origin, 14.4).</summary>
        public Transform CityRoot;
        /// <summary>Root of camera-following far content (sky dome, far backdrops; BackdropRig).</summary>
        public Transform BackdropRoot;
        public Camera Camera;
        public int Seed;
        /// <summary>Forced fallback tier (-tier N; 0 = full, 1 = simplified, 2 = graybox).</summary>
        public int Tier;
        public readonly List<IWorldUpdater> Updaters = new List<IWorldUpdater>();
        public readonly List<string> BuildLog = new List<string>();
        /// <summary>Per-beat content roots (index 0..6), created on demand under CityRoot; always active (14.6).</summary>
        public readonly Transform[] BeatRoots = new Transform[NocturneConstants.BeatCount];
        /// <summary>Total triangles generated (for the budget log).</summary>
        public long Triangles;

        public void Register(IWorldUpdater u) { if (u != null && !Updaters.Contains(u)) Updaters.Add(u); }

        /// <summary>The content root of beat b (0..6), created under CityRoot on first use.</summary>
        public Transform BeatRoot(int b)
        {
            if (BeatRoots[b] == null)
            {
                var go = new GameObject("Beat" + (b + 1).ToString("00") + "_" + NocturneConstants.BeatNames[b]);
                go.transform.SetParent(CityRoot, false);
                BeatRoots[b] = go.transform;
            }
            return BeatRoots[b];
        }

        static readonly int s_leakAnchor = Shader.PropertyToID("_LeakAnchor");
        /// <summary>Meshes whose vertex colour B was baked as the leak anchor (TEXTURE_PLAN 3.3; QA and the log).</summary>
        public int LeakAnchoredMeshes;

        /// <summary>Creates a child GameObject with a MeshFilter + MeshRenderer (no shadows: 14.2.3). For a material with
        /// _LeakAnchor = 1 (NocSurface rows whose leaks hang from the top edges: TEXTURE_PLAN 3.3) the mesh's vertex colour B is
        /// baked once to the distance below its face top (VertexBake.TopDistance; idempotent for a mesh shared by renderers); only on the
        /// Look v2 path (the only one since TP-2).</summary>
        public MeshRenderer AddMesh(Transform parent, string name, Mesh mesh, Material mat)
        {
            if (mesh != null && mat != null && mat.HasProperty(s_leakAnchor) && mat.GetFloat(s_leakAnchor) > 0.5f && VertexBake.TopDistance(mesh) > 0)
                LeakAnchoredMeshes++;
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            var mr = go.AddComponent<MeshRenderer>();
            mr.sharedMaterial = mat;
            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            mr.receiveShadows = false;
            mr.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
            mr.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
            mr.allowOcclusionWhenDynamic = false;
            if (mesh != null) Triangles += TriangleCount(mesh);
            return mr;
        }

        /// <summary>Triangles of a mesh: sum of the index counts / 3 (IMPORT_PLAN 2.4: no MeshKit.TriangleCount).</summary>
        public static long TriangleCount(Mesh mesh)
        {
            long n = 0;
            for (int i = 0; i < mesh.subMeshCount; i++) n += (long)mesh.GetIndexCount(i) / 3;
            return n;
        }

        public void Log(string msg) { BuildLog.Add(msg); Debug.Log("[NOC] " + msg); }
    }
}
