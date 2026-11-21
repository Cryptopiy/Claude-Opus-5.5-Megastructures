"""Writers: gzip'd binary meshes (KRM1) the runtime decodes (KrakenAssets.cs), deterministic .meta files, the manifest.

Unity has no glTF importer in this project and the rig needs extra per-vertex channels (limb, arc length, skin weight,
angle, radius) that OBJ cannot carry, so meshes are a small binary format read with Mesh.SetVertexBufferData at load
(no Python at RebuildAll; the generated files are committed). Layout (little endian), after gzip:
  'KRM1', int32 version (1), int32 vertexCount, int32 indexCount, float32 boundsMin[3], boundsMax[3],
  float32 position[3 vc], normal[3 vc], tangent[4 vc], uv0[2 vc], uv1[2 vc], color[4 vc], uint32 index[ic].
"""
import gzip, hashlib, json, os, struct
import numpy as np
from kr_common import *

TEX_META_TEMPLATE = os.path.join(ROOT, "Assets", "_Game", "Textures", "BwConcrete_Albedo.png.meta")


def guid_for(rel):
    return hashlib.md5(("bulwark-kraken:" + rel.replace("\\", "/")).encode("utf-8")).hexdigest()


def rel_assets(path):
    return os.path.relpath(path, ROOT).replace("\\", "/")


def write_text(path, text):
    with open(path, "w", encoding="utf-8", newline="\n") as f:
        f.write(text)


def folder_meta(path):
    meta = path.rstrip("/\\") + ".meta"
    write_text(meta, "fileFormatVersion: 2\nguid: %s\nfolderAsset: yes\nDefaultImporter:\n  externalObjects: {}\n"
                     "  userData: \n  assetBundleName: \n  assetBundleVariant: \n" % guid_for(rel_assets(path)))


def text_meta(path):
    write_text(path + ".meta", "fileFormatVersion: 2\nguid: %s\nTextScriptImporter:\n  externalObjects: {}\n"
                               "  userData: \n  assetBundleName: \n  assetBundleVariant: \n" % guid_for(rel_assets(path)))


def texture_meta(path, srgb, normal, max_size, fmt):
    """fmt: Unity TextureImporterFormat for Standalone (25 BC7, 27 BC5)."""
    t = open(TEX_META_TEMPLATE, encoding="utf-8").read()
    lines = t.split("\n")
    out = []
    for ln in lines:
        s = ln.strip()
        ind = ln[:len(ln) - len(ln.lstrip())]
        if s.startswith("guid:"): ln = "guid: " + guid_for(rel_assets(path))
        elif s.startswith("sRGBTexture:"): ln = ind + "sRGBTexture: %d" % (1 if srgb else 0)
        elif s.startswith("textureType:"): ln = ind + "textureType: %d" % (1 if normal else 0)
        elif s.startswith("maxTextureSize:"): ln = ind + "maxTextureSize: %d" % max_size
        elif s.startswith("aniso:"): ln = ind + "aniso: 8"
        out.append(ln)
    text = "\n".join(out)
    # Standalone override format
    text = text.replace("    buildTarget: Standalone\n    maxTextureSize: %d\n    resizeAlgorithm: 0\n    textureFormat: 25" % max_size,
                        "    buildTarget: Standalone\n    maxTextureSize: %d\n    resizeAlgorithm: 0\n    textureFormat: %d" % (max_size, fmt))
    write_text(path + ".meta", text)


def write_mesh(path, P, Nrm, Tan, UV0, UV1, Col, Tri):
    vc = len(P); ic = Tri.size
    bmin = P.min(0); bmax = P.max(0)
    head = b"KRM1" + struct.pack("<iii", 1, vc, ic) + struct.pack("<6f", *bmin, *bmax)
    body = b"".join(a.astype("<f4").tobytes() for a in (P, Nrm, Tan, UV0, UV1, Col)) + Tri.astype("<u4").tobytes()
    with open(path, "wb") as raw, gzip.GzipFile(filename="", mode="wb", compresslevel=9, fileobj=raw, mtime=0) as f:
        f.write(head + body)
    text_meta(path)
    log("wrote %s: %d vertices, %d triangles, %.1f MB gz" % (os.path.basename(path), vc, ic // 3, os.path.getsize(path) / 1e6))


def write_json(path, obj):
    write_text(path, json.dumps(obj, indent=1, sort_keys=False))
    text_meta(path)
