using UnityEngine;

namespace Megastructure.Rendering
{
    /// <summary>
    /// GPU-placed fields on top of <see cref="InstancedField"/> (imported from The Ring's TerrainIndirectField): one
    /// Graphics.RenderMeshIndirect command per field and frame, all placement in the vertex shader (BW_INDIRECT contract),
    /// fixed explicit world bounds, no per-frame CPU work or allocation. Using InstancedField keeps the capture rule in one
    /// place: no automatic submit in capture mode; CaptureHarness re-submits every live field for its camera through
    /// InstancedField.SubmitAllFor right before each capture render. A field that belongs to several sets is created once
    /// per set (identical draws), so each owner toggles its own copy.
    /// </summary>
    public static class TerrainIndirectField
    {
        /// <summary>Creates one indirect field per set (index 0 is named <paramref name="name"/>, the others name.set).
        /// Build time only. Falls back to RenderMeshPrimitives where indirect arguments are unsupported.</summary>
        public static InstancedField[] Create(WorldContext ctx, string name, Mesh mesh, Material material, int instanceCount,
                                              Bounds worldBounds, bool castShadows, string[] sets)
        {
            int n = sets != null ? sets.Length : 0;
            var fields = new InstancedField[n];
            // GetIndirectInstanceID returns SV_InstanceID unchanged on D3D, so the same shader serves both modes
            var mode = SystemInfo.supportsIndirectArgumentsBuffer ? InstancedField.DrawMode.Indirect : InstancedField.DrawMode.Primitives;
            if (mode != InstancedField.DrawMode.Indirect)
                Debug.LogWarning("[BW] " + name + ": indirect argument buffers unsupported, drawing with RenderMeshPrimitives");
            for (int i = 0; i < n; i++)
            {
                string fieldName = i == 0 ? name : name + "." + sets[i];
                fields[i] = InstancedField.Create(ctx, fieldName, sets[i], mesh, material, instanceCount, worldBounds, castShadows, mode);
            }
            return fields;
        }
    }
}
