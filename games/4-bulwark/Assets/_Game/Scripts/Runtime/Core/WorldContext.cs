// World-building contract shared by every beat builder (Assets/_Game/Beats/BeatNN_*). Imported from Thread.
// GameBootstrap creates one WorldContext, calls each IBeatBuilder.Build once behind the black load fade, then
// ProgressDirector calls every registered IWorldUpdater once per frame (after the camera has moved, before rendering;
// CaptureHarness calls ProgressDirector.Apply() before every capture so updaters re-submit for the capture pose).
// Updaters must not allocate. Builders compile into Megastructure.Runtime through the Beats asmref (discovery scans
// typeof(IBeatBuilder).Assembly only).
using System.Collections.Generic;
using UnityEngine;

namespace Megastructure
{
    /// <summary>Per-frame state handed to updaters (derived in double from BwPath / the clock, then cast).</summary>
    public struct FrameState
    {
        public double s;              // progress (filleted plan distance) of the player / capture pose
        public double ambientTime;    // ambient clock (frozen in static tests, set by captures)
        public Vector3 cameraPos;
        public Vector3 cameraForward;
        public float renderHeight;    // pixel height of the current render target
        public float fovV;            // vertical FOV of the camera (deg)
        public int beat;              // 1..7 (BwPath.BeatAt)
        public bool capture;          // capture mode (stills and sequences): no slews, frozen sea impacts per shot
    }

    public interface IWorldUpdater
    {
        void OnFrame(in FrameState f);
    }

    public interface IBeatBuilder
    {
        /// <summary>Short name used in logs, e.g. "Beat02_Crest". Builders run sorted by this name.</summary>
        string Name { get; }
        /// <summary>Builds all geometry of the beat (runs once at load). Register updaters on ctx.</summary>
        void Build(WorldContext ctx);
    }

    /// <summary>A world-space subject a key shot must show (CompositionLint, Part 11.13 visibility audit).</summary>
    public struct KeyShotSubject
    {
        public string Name;
        public Bounds WorldBounds;
        /// <summary>Minimum fraction of the frame height the subject must cover (0 = only "in frame").</summary>
        public float MinScreenFraction;
        public KeyShotSubject(string name, Bounds worldBounds, float minScreenFraction = 0f)
        {
            Name = name; WorldBounds = worldBounds; MinScreenFraction = minScreenFraction;
        }
    }

    /// <summary>Implemented by beat builders that own key-shot subjects (14.10.2 "Must be visible").</summary>
    public interface IKeyShotSubjects
    {
        /// <summary>Appends the subjects of the shot with this id (e.g. "KS04"); nothing when the builder has none.</summary>
        void CollectSubjects(string shotId, List<KeyShotSubject> into);
    }

    /// <summary>A world set for the 14.6 pre-warm: enabled for 3 frames at its key pose behind the black load fade.</summary>
    public struct PrewarmSet
    {
        public string Name;
        public Vector3 Eye;
        public float YawDeg, PitchDeg;
        /// <summary>Enable(true) shows the set, Enable(false) restores its normal state (the owner's choice).</summary>
        public System.Action<bool> Enable;
    }

    public sealed class WorldContext
    {
        /// <summary>Sets for the 14.6 pre-warm (Crest, Spillway, Reef, Interior5, Hall6, Tunnel7, Ledge, FarField).</summary>
        public readonly List<PrewarmSet> PrewarmSets = new List<PrewarmSet>();
        public MaterialLibrary Materials;
        /// <summary>Root of all built content (true world coordinates; Bulwark keeps every pose within ~1500 m of the origin).</summary>
        public Transform Root;
        public Camera Camera;
        /// <summary>Seed of every deterministic choice (VertexBake, placement). GameBootstrap sets it from -seed.</summary>
        public uint Seed = 1;
        public readonly List<IWorldUpdater> Updaters = new List<IWorldUpdater>();
        public readonly List<IKeyShotSubjects> SubjectProviders = new List<IKeyShotSubjects>();
        public readonly List<string> BuildLog = new List<string>();
        /// <summary>Meshes that failed MeshValidation (Validate reports them; the build continues).</summary>
        public readonly List<string> MeshProblems = new List<string>();
        /// <summary>Total triangles generated (for the budget log).</summary>
        public long Triangles;

        public void Register(IWorldUpdater u) { if (u != null && !Updaters.Contains(u)) Updaters.Add(u); }
        public void RegisterSubjects(IKeyShotSubjects p) { if (p != null && !SubjectProviders.Contains(p)) SubjectProviders.Add(p); }

        /// <summary>
        /// Creates a child GameObject with a MeshFilter + MeshRenderer (light and reflection probes off). The mesh is
        /// validated (MeshValidation) while it is still readable; pass upload = true to call UploadMeshData(true) afterwards.
        /// </summary>
        public MeshRenderer AddMesh(Transform parent, string name, Mesh mesh, Material mat, bool castShadows = true,
                                    bool receiveShadows = true, bool upload = false)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent != null ? parent : Root, false);
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            var mr = go.AddComponent<MeshRenderer>();
            mr.sharedMaterial = mat;
            mr.shadowCastingMode = castShadows ? UnityEngine.Rendering.ShadowCastingMode.On : UnityEngine.Rendering.ShadowCastingMode.Off;
            mr.receiveShadows = receiveShadows;
            mr.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
            mr.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
            mr.allowOcclusionWhenDynamic = false;
            if (mesh != null)
            {
                Triangles += MeshKit.TriangleCount(mesh);
                if (mesh.isReadable)
                {
                    var rep = MeshValidation.Validate(mesh);
                    if (!rep.Ok) MeshProblems.Add(name + ": " + rep.Describe());
                }
                if (upload && mesh.isReadable) mesh.UploadMeshData(true);
            }
            if (mat == null) MeshProblems.Add(name + ": material is null");
            return mr;
        }

        public void Log(string msg) { BuildLog.Add(msg); Debug.Log("[BW] " + msg); }
    }
}
