using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.Rendering;

namespace Megastructure.Rendering
{
    /// <summary>
    /// One procedural instanced draw (the "horn field" of 14.1, far unit fields, ...), imported from The Ring: wraps
    /// Graphics.RenderMeshPrimitives (CPU-known count) or Graphics.RenderMeshIndirect (args in an IndirectArguments
    /// GraphicsBuffer, GraphicsBuffer.IndirectDrawIndexedArgs, one command) with EXPLICIT world bounds.
    /// Created at load (inside IBeatBuilder.Build); nothing is allocated afterwards. BeatDirector (or the owning builder's
    /// updater) toggles <see cref="Visible"/>.
    ///
    /// Submission: every frame from <see cref="InstancedFieldDriver"/> (LateUpdate 32000) for all cameras
    /// (RenderParams.camera = null), EXCEPT in capture mode (<see cref="GameState.CaptureMode"/>): then nothing is submitted
    /// automatically and CaptureHarness calls <see cref="SubmitAllFor"/>(cam) right before each render request (C-12).
    ///
    /// SHADER-SIDE CONTRACT for BW_INDIRECT draws (BwLitCore.hlsl / BwLitPass.hlsl):
    /// - In the shader's HLSLINCLUDE: #define BW_INDIRECT and #define BW_HAS_VERTEX_HOOK, then
    ///   void BwVertexHook(inout BwVertexData v). With BW_INDIRECT the attributes carry SV_InstanceID into v.instanceID and
    ///   v.positionWS / v.normalWS / v.tangentWS hold the mesh's OBJECT-space values: the hook writes the world position
    ///   (and normal, tangent) itself. unity_ObjectToWorld is not applied and must not be used.
    /// - Per-instance data: declare it OUTSIDE UnityPerMaterial, e.g. StructuredBuffer&lt;float4&gt; _BwInstances; bound
    ///   through <see cref="SetInstanceData{T}"/> or <see cref="SetBuffer"/> on this field's MaterialPropertyBlock.
    /// - Primitives mode: SV_InstanceID is 0..InstanceCount-1 directly.
    /// - Indirect mode: also #define UNITY_INDIRECT_DRAW_ARGS IndirectDrawIndexedArgs and #include "UnityIndirect.cginc",
    ///   call InitIndirectDrawArgs(0); at the top of the vertex hook in EVERY pass, then index with
    ///   GetIndirectInstanceID(v.instanceID).
    /// - No #pragma multi_compile_instancing in BW_INDIRECT shaders (INSTANCING_ON would read unfilled per-instance matrices).
    /// - Opaque fields with castShadows need the ShadowCaster pass (BwLitPass provides all four passes).
    /// </summary>
    public sealed class InstancedField
    {
        public enum DrawMode { Primitives = 0, Indirect = 1 }

        public const string DefaultBufferName = "_BwInstances";

        static readonly List<InstancedField> s_All = new List<InstancedField>();

        /// <summary>Number of live (not released) fields.</summary>
        public static int Count => s_All.Count;
        public static InstancedField At(int index) => s_All[index];

        public readonly string Name;
        public readonly DrawMode Mode;
        /// <summary>Free-form set name (logs, QA); visibility is driven by the owner.</summary>
        public string Set { get; }
        public bool Visible { get; set; } = true;
        public Mesh Mesh { get; private set; }
        public Material Material { get; private set; }
        public int SubMesh { get; private set; }
        /// <summary>CPU instance count (Primitives) or the count written into the args (Indirect; capacity when the
        /// args are GPU-written).</summary>
        public int InstanceCount { get; private set; }
        public Bounds WorldBounds => _rp.worldBounds;
        public bool CastShadows => _rp.shadowCastingMode != ShadowCastingMode.Off;
        public MaterialPropertyBlock Properties => _mpb;
        public GraphicsBuffer ArgsBuffer => _args;
        public GraphicsBuffer InstanceBuffer => _instances;
        public bool IsReleased => _released;
        /// <summary>The GameObject that owns this field's lifetime (null when created without a WorldContext).</summary>
        public GameObject Holder { get; private set; }
        /// <summary>Triangles drawn per submission (QA budgets, Part 7.4).</summary>
        public long TriangleCount => Mesh != null ? (long)(Mesh.GetIndexCount(SubMesh) / 3) * InstanceCount : 0;

        RenderParams _rp;
        readonly MaterialPropertyBlock _mpb = new MaterialPropertyBlock();
        readonly GraphicsBuffer.IndirectDrawIndexedArgs[] _argsData = new GraphicsBuffer.IndirectDrawIndexedArgs[1];
        GraphicsBuffer _args;
        GraphicsBuffer _instances;
        bool _released;

        /// <param name="worldBounds">explicit world bounds of all instances (required for procedural draws)</param>
        /// <param name="gpuWritableArgs">Indirect only: the args buffer is also Raw so a compute shader can write it</param>
        public InstancedField(string name, string set, Mesh mesh, Material material, int instanceCount, Bounds worldBounds,
            bool castShadows, DrawMode mode = DrawMode.Primitives, bool receiveShadows = true,
            int layer = 0, int subMesh = 0, bool gpuWritableArgs = false)
        {
            Name = string.IsNullOrEmpty(name) ? "InstancedField" : name;
            Set = set;
            Mode = mode;
            Mesh = mesh;
            Material = material;
            SubMesh = mesh != null ? Mathf.Clamp(subMesh, 0, Mathf.Max(0, mesh.subMeshCount - 1)) : 0;
            InstanceCount = Mathf.Max(0, instanceCount);
            if (mesh == null) Debug.LogError("[BW-FAIL] InstancedField '" + Name + "': mesh is null");
            if (material == null) Debug.LogError("[BW-FAIL] InstancedField '" + Name + "': material is null");

            _rp = new RenderParams(material)   // never default(RenderParams): renderingLayerMask would be 0
            {
                worldBounds = worldBounds,
                shadowCastingMode = castShadows ? ShadowCastingMode.On : ShadowCastingMode.Off,
                receiveShadows = receiveShadows,
                layer = layer,
                matProps = _mpb,
                lightProbeUsage = LightProbeUsage.Off,
                reflectionProbeUsage = ReflectionProbeUsage.Off,
            };

            if (mode == DrawMode.Indirect)
            {
                var target = GraphicsBuffer.Target.IndirectArguments;
                if (gpuWritableArgs) target |= GraphicsBuffer.Target.Raw;
                _args = new GraphicsBuffer(target, 1, GraphicsBuffer.IndirectDrawIndexedArgs.size);
                WriteArgs();
            }

            s_All.Add(this);
            InstancedFieldDriver.Ensure();
        }

        /// <summary>
        /// Creates a field and gives it a holder GameObject under the WorldContext root whose destruction releases the GPU
        /// buffers (scene unload, world rebuild). Build time only.
        /// </summary>
        public static InstancedField Create(WorldContext ctx, string name, string set, Mesh mesh, Material material, int instanceCount,
            Bounds worldBounds, bool castShadows, DrawMode mode = DrawMode.Primitives,
            bool receiveShadows = true, int layer = 0, int subMesh = 0, bool gpuWritableArgs = false)
        {
            var f = new InstancedField(name, set, mesh, material, instanceCount, worldBounds, castShadows, mode,
                receiveShadows, layer, subMesh, gpuWritableArgs);
            if (ctx != null && ctx.Root != null)
            {
                var go = new GameObject(f.Name);
                go.transform.SetParent(ctx.Root, false);
                var handle = go.AddComponent<InstancedFieldHandle>();
                handle.Field = f;
                f.Holder = go;
            }
            return f;
        }

        void WriteArgs()
        {
            if (_args == null) return;
            _argsData[0].indexCountPerInstance = Mesh != null ? Mesh.GetIndexCount(SubMesh) : 0u;
            _argsData[0].instanceCount = (uint)InstanceCount;
            _argsData[0].startIndex = Mesh != null ? Mesh.GetIndexStart(SubMesh) : 0u;
            _argsData[0].baseVertexIndex = Mesh != null ? Mesh.GetBaseVertex(SubMesh) : 0u;
            _argsData[0].startInstance = 0u;
            _args.SetData(_argsData);
        }

        /// <summary>Changes the instance count (Primitives: next submission; Indirect: rewrites the args). Load time
        /// or rare events only (Indirect uploads 20 bytes).</summary>
        public void SetInstanceCount(int count)
        {
            count = Mathf.Max(0, count);
            if (count == InstanceCount) return;
            InstanceCount = count;
            if (Mode == DrawMode.Indirect) WriteArgs();
        }

        /// <summary>Moves the culling bounds (e.g. a camera-following grass patch). Allocation-free.</summary>
        public void SetWorldBounds(Bounds worldBounds)
        {
            _rp.worldBounds = worldBounds;
        }

        /// <summary>Uploads per-instance data into a field-owned StructuredBuffer bound as <paramref name="shaderName"/>.
        /// Load time only. The struct must be blittable with a size that is a multiple of 4 bytes.</summary>
        public void SetInstanceData<T>(T[] data, string shaderName = DefaultBufferName) where T : struct
        {
            if (_released || data == null || data.Length == 0) return;
            int stride = UnsafeUtility.SizeOf<T>();
            if (stride <= 0 || (stride & 3) != 0)
            {
                Debug.LogError("[BW-FAIL] InstancedField '" + Name + "': instance stride " + stride + " is not a multiple of 4");
                return;
            }
            if (_instances != null && (_instances.count != data.Length || _instances.stride != stride))
            {
                _instances.Release();
                _instances = null;
            }
            if (_instances == null) _instances = new GraphicsBuffer(GraphicsBuffer.Target.Structured, data.Length, stride);
            _instances.SetData(data);
            _mpb.SetBuffer(shaderName, _instances);
        }

        /// <summary>Binds an externally owned buffer (e.g. written by a compute shader). The caller releases it.</summary>
        public void SetBuffer(string shaderName, GraphicsBuffer buffer) { if (buffer != null) _mpb.SetBuffer(shaderName, buffer); }
        public void SetFloat(string shaderName, float value) => _mpb.SetFloat(shaderName, value);
        public void SetVector(string shaderName, Vector4 value) => _mpb.SetVector(shaderName, value);
        public void SetInt(string shaderName, int value) => _mpb.SetInteger(shaderName, value);

        /// <summary>Queues this field for <paramref name="cam"/> (null = every camera that renders this frame).
        /// Allocation-free.</summary>
        public void Submit(Camera cam)
        {
            if (!Visible || _released || Mesh == null || Material == null || InstanceCount <= 0) return;
            _rp.camera = cam;
            if (Mode == DrawMode.Indirect)
            {
                if (_args != null) Graphics.RenderMeshIndirect(in _rp, Mesh, _args, 1, 0);
            }
            else
            {
                Graphics.RenderMeshPrimitives(in _rp, Mesh, SubMesh, InstanceCount);
            }
        }

        /// <summary>Every live field for every camera (the driver, outside capture mode).</summary>
        public static void SubmitAll()
        {
            for (int i = 0; i < s_All.Count; i++) s_All[i].Submit(null);
        }

        /// <summary>Every live field for one camera; the CaptureHarness calls this before each manual render.</summary>
        public static void SubmitAllFor(Camera cam)
        {
            for (int i = 0; i < s_All.Count; i++) s_All[i].Submit(cam);
        }

        /// <summary>Releases the GPU buffers and removes the field from the registry (idempotent).</summary>
        public void Release()
        {
            if (_released) return;
            _released = true;
            if (_args != null) { _args.Release(); _args = null; }
            if (_instances != null) { _instances.Release(); _instances = null; }
            s_All.Remove(this);
        }

        public static void ReleaseAll()
        {
            while (s_All.Count > 0) s_All[s_All.Count - 1].Release();
        }

        // Enter-play-mode without domain reload keeps statics: start every session clean.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics() => ReleaseAll();
    }
}
