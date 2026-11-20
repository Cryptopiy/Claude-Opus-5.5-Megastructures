// FacadeKit: NocFacade faces for G1's hero shafts (the alley towers of 14.5.1, the boulevard slabs of 14.5.2), with the vertex
// contract of Shaders/NocFacade.shader that CityMesher uses: uv0 = (u metres along the face, 16-bit face id as float),
// uv1 = (uMin, uMax, y0, y1) of the face, colour R = AO 255, G = skin byte (NocFacadeMath.SkinByte), B = mode 0 (facade),
// A = edge-light kind x 63. u and the face id are
// world-derived exactly as CityField.FaceFrame / NocFacadeMath.FaceId do, so co-planar neighbours (a slab and its closure) share
// one continuous window field and the windows never jump at a seam. Vertical faces only; a face turned away from the whole walk
// region (CityField.FacesWalk) is a back face for every camera and is skipped (exact cull). The warm-window fraction is the beat's
// global (_NocFacade.y = NocturneConstants.WarmFraction: 0.08 alley, 0.10 boulevard), set by the AtmosphereDriver.
using System;
using UnityEngine;

namespace Megastructure
{
    public static class FacadeKit
    {
        /// <summary>Edge-light kinds of the facade shader (0 none, 1 cyan, 2 magenta, 3 white, 4 violet).</summary>
        public const int EdgeNone = 0, EdgeCyan = 1, EdgeMagenta = 2, EdgeWhite = 3, EdgeViolet = 4;

        /// <summary>One vertical facade face from plan point a to b (world xz), y0..y1, outward normal (nx, nz) (unit, axis or not).
        /// Returns false when the face faces away from the whole walk region (not built).</summary>
        public static bool Face(MeshBuilder mb, double ax, double az, double bx, double bz, double y0, double y1, double nx, double nz,
            uint seed, int edge, bool cull = true, int region = -1)
        {
            // Look v2 (TEXTURE_PLAN 2.2): G = NocFacadeMath.SkinByte(seed, region of the box centre, or of the face centre for a
            // lone face); uv1.zw = (y0, y1). region < 0: the face centre's region.
            if (region < 0) region = NocFacadeMath.Region(0.5 * (ax + bx), 0.5 * (az + bz));
            double yawN = Math.Atan2(nx, nz) * (180.0 / Math.PI);
            yawN = Math.Round(yawN / NocturneConstants.CityYawQuantum) * NocturneConstants.CityYawQuantum;
            var f = new CityField.Face { ax = ax, az = az, bx = bx, bz = bz, nx = nx, nz = nz, yawN = yawN, y0 = y0, y1 = y1 };
            if (cull && !CityField.FacesWalk(f)) return false;
            CityField.FaceFrame(f, out _, out _, out double uA, out double uB, out double offset);
            uint id = NocFacadeMath.FaceId(f.yawN, offset);
            float uMin = NocFacadeMath.SafeExtent(Math.Min(uA, uB)), uMax = NocFacadeMath.SafeExtent(Math.Max(uA, uB));   // v2 snap-safe (<= 1 mm)
            bool wasUv1 = mb.WriteUV1;
            Color32 oldColour = mb.Color;
            mb.WriteUV1 = true;
            mb.CurrentUV1 = new Vector4(uMin, uMax, NocFacadeMath.GridY0(y0), NocFacadeMath.GridY1(y1));
            mb.Color = new Color32(255, NocFacadeMath.SkinByte(seed, region), 0, (byte)(63 * Mathf.Clamp(edge, 0, 4)));
            var n = new Vector3((float)nx, 0f, (float)nz);
            var a0 = new Vector3((float)ax, (float)y0, (float)az);
            var a1 = new Vector3((float)ax, (float)y1, (float)az);
            var b1 = new Vector3((float)bx, (float)y1, (float)bz);
            var b0 = new Vector3((float)bx, (float)y0, (float)bz);
            float fid = id;
            int i0 = mb.Vertex(a0, n, new Vector2((float)uA, fid));
            int i1 = mb.Vertex(a1, n, new Vector2((float)uA, fid));
            int i2 = mb.Vertex(b1, n, new Vector2((float)uB, fid));
            int i3 = mb.Vertex(b0, n, new Vector2((float)uB, fid));
            Vector3 c = Vector3.Cross(a1 - a0, b1 - a0);
            if (Vector3.Dot(c, n) >= 0f) mb.Quad(i0, i1, i2, i3);
            else mb.Quad(i0, i3, i2, i1);
            mb.WriteUV1 = wasUv1;
            mb.Color = oldColour;
            return true;
        }

        /// <summary>The vertical faces of an axis-aligned box x0..x1, z0..z1, y0..y1 selected by G1Mesh.PX / NX / PZ / NZ.</summary>
        public static int Box(MeshBuilder mb, double x0, double x1, double z0, double z1, double y0, double y1, int mask, uint seed, int edge)
        {
            int n = 0;
            int region = NocFacadeMath.Region(0.5 * (x0 + x1), 0.5 * (z0 + z1));     // one skin per box (TEXTURE_PLAN 2.3)
            if ((mask & G1Mesh.PZ) != 0 && Face(mb, x0, z1, x1, z1, y0, y1, 0, 1, seed, edge, true, region)) n++;
            if ((mask & G1Mesh.NZ) != 0 && Face(mb, x1, z0, x0, z0, y0, y1, 0, -1, seed, edge, true, region)) n++;
            if ((mask & G1Mesh.PX) != 0 && Face(mb, x1, z1, x1, z0, y0, y1, 1, 0, seed, edge, true, region)) n++;
            if ((mask & G1Mesh.NX) != 0 && Face(mb, x0, z0, x0, z1, y0, y1, -1, 0, seed, edge, true, region)) n++;
            return n;
        }
    }
}
