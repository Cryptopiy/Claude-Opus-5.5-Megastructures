// CityMesher: turns CityField towers into NocFacade meshes (MASTER_PROMPT 14.7 H2, 7.5). One mesh per 240 m cell with its
// chunk-local origin at the cell centre (every vertex within 170 m horizontally, 7.5 rule of <= 200 m), vertical faces only (all
// ledges are above the eye and the tops are inside the cloud mix), flat-shaded quads of 4 vertices: uv0 = (u, face id),
// uv1 = (uMin, uMax, y0, y1), colour R = AO 255, G = skin byte (NocFacadeMath.SkinByte of the tower seed and the region of the
// tower centre), B = mode 0 (facade), A = edge-light kind x 63. u and the face id are
// world-derived in double (CityField.FaceFrame, NocFacadeMath.FaceId), so a face built by either tier is identical. A face whose
// outward normal points away from the whole walk region (CityField.FacesWalk) is a back face for every camera and is not built.
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Megastructure
{
    public static class CityMesher
    {
        /// <summary>Appends the visible faces of the towers to mb, positions relative to (ox, oz). Returns the number of faces.</summary>
        public static int Append(MeshBuilder mb, IList<CityTower> towers, double ox, double oz, ref int culled)
        {
            var faces = new CityField.Face[4];
            int n = 0;
            mb.WriteUV1 = true;
            foreach (var t in towers)
            {
                for (int b = 0; b < 3; b++)
                {
                    CityField.Faces(t, b, faces);
                    for (int f = 0; f < 4; f++)
                    {
                        if (!CityField.FacesWalk(faces[f])) { culled++; continue; }
                        AddFace(mb, t, faces[f], ox, oz);
                        n++;
                    }
                }
            }
            return n;
        }

        static void AddFace(MeshBuilder mb, CityTower t, in CityField.Face f, double ox, double oz)
        {
            CityField.FaceFrame(f, out double tx, out double tz, out double uA, out double uB, out double offset);
            uint id = NocFacadeMath.FaceId(f.yawN, offset);
            float uMin = NocFacadeMath.SafeExtent(Math.Min(uA, uB)), uMax = NocFacadeMath.SafeExtent(Math.Max(uA, uB));   // v2 snap-safe (<= 1 mm)
            // Look v2 (TEXTURE_PLAN 2.2, lead decision 1): G = the skin byte of the tower (every face of a tower carries the same
            // byte, so strata, floors and lit floor groups continue around corners), uv1.zw = the face's bottom and top in world y
            mb.Color = new Color32(255, NocFacadeMath.SkinByte(t.seed, NocFacadeMath.Region(t.cx, t.cz)), 0, (byte)(63 * t.edge));
            mb.CurrentUV1 = new Vector4(uMin, uMax, NocFacadeMath.GridY0(f.y0), NocFacadeMath.GridY1(f.y1));
            var n = new Vector3((float)f.nx, 0f, (float)f.nz);
            Vector3 a0 = new Vector3((float)(f.ax - ox), (float)f.y0, (float)(f.az - oz));
            Vector3 a1 = new Vector3((float)(f.ax - ox), (float)f.y1, (float)(f.az - oz));
            Vector3 b1 = new Vector3((float)(f.bx - ox), (float)f.y1, (float)(f.bz - oz));
            Vector3 b0 = new Vector3((float)(f.bx - ox), (float)f.y0, (float)(f.bz - oz));
            float fid = id;
            int i0 = mb.Vertex(a0, n, new Vector2((float)uA, fid));
            int i1 = mb.Vertex(a1, n, new Vector2((float)uA, fid));
            int i2 = mb.Vertex(b1, n, new Vector2((float)uB, fid));
            int i3 = mb.Vertex(b0, n, new Vector2((float)uB, fid));
            // MeshBuilder winding: front-facing where cross(b - a, c - a) points
            Vector3 c = Vector3.Cross(a1 - a0, b1 - a0);
            if (Vector3.Dot(c, n) >= 0f) mb.Quad(i0, i1, i2, i3);
            else mb.Quad(i0, i3, i2, i1);
        }

        /// <summary>World position of beacon k (0..5) of a tower: two opposite corners x three heights, 0.6 m out of the corner.</summary>
        public static void Beacon(CityTower t, int k, out double x, out double y, out double z)
        {
            y = NocturneConstants.CityBeaconY[k % 3];
            int b = y < t.boxes[1].y0 ? 0 : (y < t.boxes[2].y0 ? 1 : 2);
            bool first = (k / 3) == 0;
            int s = (t.seed >> 9 & 1u) == 0 ? 1 : -1;
            int sx = first ? s : -s, sz = first ? 1 : -1;
            t.Corner(b, sx, sz, out double cx, out double cz);
            double r = t.yaw * (Math.PI / 180.0), co = Math.Cos(r), si = Math.Sin(r);
            double dx = sx * co + sz * si, dz = -sx * si + sz * co, l = Math.Sqrt(dx * dx + dz * dz);
            x = cx + 0.6 * dx / l; z = cz + 0.6 * dz / l;
        }
    }
}
