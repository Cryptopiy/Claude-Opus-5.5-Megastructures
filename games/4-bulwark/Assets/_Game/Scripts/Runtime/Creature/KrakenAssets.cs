// KrakenAssets: loads what Tools/kraken/build_kraken.py generated from the user's Tripo3D models (Addendum A1, kraken owner):
// Assets/_Game/Creature/Kraken/Resources/Kraken/ kraken_manifest.json (arm centre lines, cuts, eyes, bounds), the gzip'd KRM1
// body meshes kraken_body_lod0..3.bytes, tentacle_relief.bytes (the unwrapped Tripo tentacle's relief, 256 x 512 half
// floats) and the textures (Kraken_Albedo/Normal/Mask 4096, Tentacle_Albedo/Normal/Mask 2048). Unity has no glTF importer
// here and the rig needs extra vertex channels, so meshes are decoded at load (Mesh.SetVertexBufferData, ~0.1 s); RebuildAll
// never needs Python. Kraken local space: metres, origin at the arm crown, +Y up, +Z the face, mantle towards -Z.
using System;
using System.IO;
using System.IO.Compression;
using UnityEngine;
using UnityEngine.Rendering;

namespace Megastructure.Creature
{
    [Serializable] public sealed class KrakenArmDef { public int index; public float azimuthDeg, uCut, rCut; public float[] u, p, n, t, r; }
    [Serializable] public sealed class KrakenEyeDef { public string name; public float[] centre, normal; public float radius; }
    [Serializable] public sealed class KrakenLodDef { public int lod, vertices, triangles; }
    [Serializable]
    public sealed class KrakenManifest
    {
        public int version;
        public float modelScale;
        public float[] boundsMin, boundsMax, headMin, headMax;
        public float tentacleTileRadii;
        public KrakenLodDef[] lods;
        public KrakenArmDef[] arms;
        public KrakenEyeDef[] eyes;
    }

    public sealed class KrakenAssets
    {
        public const string ResPath = "Kraken/";
        public const int BodyLods = 4;
        public KrakenManifest Manifest;
        public readonly Mesh[] Body = new Mesh[BodyLods];
        public float[] Relief;            // 256 (theta) x 512 (s), relative radius change
        public int ReliefW, ReliefH;
        public Texture2D Albedo, Normal, Mask, TentAlbedo, TentNormal, TentMask;
        public string Error;
        public bool Ok => Error == null;

        static KrakenAssets s_cached;

        /// <summary>Loads (once per run) and returns the assets; check Ok / Error.</summary>
        public static KrakenAssets Load()
        {
            if (s_cached != null) return s_cached;
            var a = new KrakenAssets();
            try { a.LoadAll(); }
            catch (Exception e) { a.Error = e.GetType().Name + ": " + e.Message; }
            s_cached = a;
            return a;
        }

        void LoadAll()
        {
            var man = Resources.Load<TextAsset>(ResPath + "kraken_manifest");
            if (man == null) throw new FileNotFoundException("Resources/" + ResPath + "kraken_manifest.json missing (run Tools/kraken/build_kraken.py)");
            Manifest = JsonUtility.FromJson<KrakenManifest>(man.text);
            if (Manifest == null || Manifest.arms == null || Manifest.arms.Length != 8) throw new InvalidDataException("kraken manifest: expected 8 arms");
            for (int i = 0; i < BodyLods; i++)
            {
                var ta = Resources.Load<TextAsset>(ResPath + "kraken_body_lod" + i);
                if (ta == null) throw new FileNotFoundException("kraken_body_lod" + i + ".bytes missing");
                Body[i] = DecodeMesh(Gunzip(ta.bytes), "kraken_body_lod" + i);
                Resources.UnloadAsset(ta);
            }
            var rel = Resources.Load<TextAsset>(ResPath + "tentacle_relief");
            if (rel == null) throw new FileNotFoundException("tentacle_relief.bytes missing");
            DecodeRelief(Gunzip(rel.bytes));
            Resources.UnloadAsset(rel);
            Albedo = Tex("Kraken_Albedo"); Normal = Tex("Kraken_Normal"); Mask = Tex("Kraken_Mask");
            TentAlbedo = Tex("Tentacle_Albedo"); TentNormal = Tex("Tentacle_Normal"); TentMask = Tex("Tentacle_Mask");
        }

        static Texture2D Tex(string name)
        {
            var t = Resources.Load<Texture2D>(ResPath + name);
            if (t == null) throw new FileNotFoundException(name + ".png missing");
            return t;
        }

        public static byte[] Gunzip(byte[] data)
        {
            using (var src = new MemoryStream(data))
            using (var gz = new GZipStream(src, CompressionMode.Decompress))
            using (var dst = new MemoryStream(data.Length * 3))
            {
                gz.CopyTo(dst);
                return dst.ToArray();
            }
        }

        /// <summary>KRM1: 'KRM1', int version, int vc, int ic, float bmin[3], bmax[3], then position, normal, tangent, uv0,
        /// uv1, color (float32 arrays) and uint32 indices.</summary>
        public static Mesh DecodeMesh(byte[] b, string name)
        {
            if (b.Length < 40 || b[0] != (byte)'K' || b[1] != (byte)'R' || b[2] != (byte)'M' || b[3] != (byte)'1')
                throw new InvalidDataException(name + ": not a KRM1 mesh");
            int vc = BitConverter.ToInt32(b, 8), ic = BitConverter.ToInt32(b, 12);
            var bmin = new Vector3(BitConverter.ToSingle(b, 16), BitConverter.ToSingle(b, 20), BitConverter.ToSingle(b, 24));
            var bmax = new Vector3(BitConverter.ToSingle(b, 28), BitConverter.ToSingle(b, 32), BitConverter.ToSingle(b, 36));
            int off = 40;
            int[] comps = { 3, 3, 4, 2, 2, 4 };
            int[] dstOff = { 0, 3, 6, 14, 16, 10 };          // interleaved order: pos, normal, tangent, color, uv0, uv1
            const int stride = 18;
            long need = 40L + 4L * vc * 18 + 4L * ic;
            if (b.Length != need) throw new InvalidDataException(name + ": size " + b.Length + " != " + need);
            var inter = new float[vc * stride];
            for (int a = 0; a < 6; a++)
            {
                int c = comps[a], d = dstOff[a];
                for (int v = 0; v < vc; v++)
                {
                    int src = off + 4 * (v * c);
                    int dst = v * stride + d;
                    for (int k = 0; k < c; k++) inter[dst + k] = BitConverter.ToSingle(b, src + 4 * k);
                }
                off += 4 * c * vc;
            }
            var idx = new int[ic];
            Buffer.BlockCopy(b, off, idx, 0, 4 * ic);
            var m = new Mesh { name = name, indexFormat = IndexFormat.UInt32 };
            m.SetVertexBufferParams(vc,
                new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3),
                new VertexAttributeDescriptor(VertexAttribute.Normal, VertexAttributeFormat.Float32, 3),
                new VertexAttributeDescriptor(VertexAttribute.Tangent, VertexAttributeFormat.Float32, 4),
                new VertexAttributeDescriptor(VertexAttribute.Color, VertexAttributeFormat.Float32, 4),
                new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, 2),
                new VertexAttributeDescriptor(VertexAttribute.TexCoord1, VertexAttributeFormat.Float32, 2));
            m.SetVertexBufferData(inter, 0, 0, inter.Length, 0, MeshUpdateFlags.DontValidateIndices | MeshUpdateFlags.DontRecalculateBounds);
            m.SetIndexBufferParams(ic, IndexFormat.UInt32);
            m.SetIndexBufferData(idx, 0, 0, ic, MeshUpdateFlags.DontValidateIndices | MeshUpdateFlags.DontRecalculateBounds);
            var bounds = new Bounds((bmin + bmax) * 0.5f, bmax - bmin);
            m.subMeshCount = 1;
            m.SetSubMesh(0, new SubMeshDescriptor(0, ic, MeshTopology.Triangles) { bounds = bounds, vertexCount = vc },
                         MeshUpdateFlags.DontRecalculateBounds | MeshUpdateFlags.DontValidateIndices);
            m.bounds = bounds;
            return m;
        }

        void DecodeRelief(byte[] b)
        {
            if (b.Length < 12 || b[0] != (byte)'K' || b[1] != (byte)'R' || b[2] != (byte)'H' || b[3] != (byte)'1')
                throw new InvalidDataException("tentacle_relief: not KRH1");
            ReliefW = BitConverter.ToInt32(b, 4); ReliefH = BitConverter.ToInt32(b, 8);
            Relief = new float[ReliefW * ReliefH];
            for (int i = 0; i < Relief.Length; i++) Relief[i] = Mathf.HalfToFloat(BitConverter.ToUInt16(b, 12 + 2 * i));
        }

        /// <summary>Bilinear relief at (theta in [0, 1) around, s in tiles along), wrapping both ways.</summary>
        public float ReliefAt(float theta01, float s)
        {
            if (Relief == null) return 0f;
            float x = (theta01 - Mathf.Floor(theta01)) * ReliefW - 0.5f;
            float y = (s - Mathf.Floor(s)) * ReliefH - 0.5f;
            int x0 = Mathf.FloorToInt(x), y0 = Mathf.FloorToInt(y);
            float fx = x - x0, fy = y - y0;
            int xa = ((x0 % ReliefW) + ReliefW) % ReliefW, xb = (xa + 1) % ReliefW;
            int ya = ((y0 % ReliefH) + ReliefH) % ReliefH, yb = (ya + 1) % ReliefH;
            float a = Relief[ya * ReliefW + xa], bb = Relief[ya * ReliefW + xb], c = Relief[yb * ReliefW + xa], d = Relief[yb * ReliefW + xb];
            return Mathf.Lerp(Mathf.Lerp(a, bb, fx), Mathf.Lerp(c, d, fx), fy);
        }

        public static Vector3 V3(float[] a, int i) => new Vector3(a[3 * i], a[3 * i + 1], a[3 * i + 2]);
    }
}
