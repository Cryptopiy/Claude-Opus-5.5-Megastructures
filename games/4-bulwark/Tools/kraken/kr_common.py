"""Shared helpers of the kraken asset pipeline (Addendum A1, kraken owner).

Run every script with Tools/.venv/Scripts/python.exe (numpy, pillow, trimesh, pygltflib, pymeshlab, fast-simplification,
scipy). Sources are READ-ONLY: "3D Models/SM_Creature_Octopus.glb" and "3D Models/SM_Creature_MonsterTentacles.glb"
(Tripo3D, the user's models). Outputs go to Assets/_Game/Creature/Kraken/ (committed, so RebuildAll never needs Python).

Coordinate conventions
  glTF is right-handed, Y up, +Z towards the viewer. Unity is left-handed, Y up. We convert with x -> -x (a mirror in x),
  which flips the triangle winding (we swap two indices per triangle). The source star pose lies on y = 0 (the arm
  undersides) with the head up to y = 1.0.
  Kraken local space (what the runtime gets): metres, origin at the centre of the arm crown (where the arms meet, on
  the body axis), +Y up through the head, +Z = the head's forward (eyes side), scale MODEL_SCALE metres per glTF unit.
"""
import io
import os
import sys
import numpy as np

ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", ".."))
SRC_OCTO = os.path.join(ROOT, "3D Models", "SM_Creature_Octopus.glb")
SRC_TENT = os.path.join(ROOT, "3D Models", "SM_Creature_MonsterTentacles.glb")
OUT_DIR = os.path.join(ROOT, "Assets", "_Game", "Creature", "Kraken")
RES_DIR = os.path.join(OUT_DIR, "Resources", "Kraken")
CACHE_DIR = os.path.join(ROOT, "Library", "KrakenCache")   # Library is gitignored; caches only
LOG_DIR = os.path.join(ROOT, "Logs", "kraken")

# 220 m per glTF unit: head/mantle ~1.0 unit -> ~220 m, the arms (1.6 units from the centre) -> ~350 m (lead decision)
MODEL_SCALE = 220.0


def log(*a):
    print("[kraken]", *a, flush=True)


def load_glb(path, use_cache=True):
    """Returns positions (N,3) f64, normals (N,3) f64, uv (N,2) f64, triangles (M,3) i64, texture PIL RGB."""
    from PIL import Image
    os.makedirs(CACHE_DIR, exist_ok=True)
    key = os.path.join(CACHE_DIR, os.path.splitext(os.path.basename(path))[0])
    st = os.stat(path)
    stamp = "%d_%d" % (st.st_size, int(st.st_mtime))
    if use_cache and os.path.exists(key + ".npz") and os.path.exists(key + ".png"):
        z = np.load(key + ".npz")
        if str(z["stamp"]) == stamp:
            return z["V"], z["N"], z["UV"], z["I"], Image.open(key + ".png").convert("RGB")
    import pygltflib
    g = pygltflib.GLTF2().load(path)
    blob = g.binary_blob()

    def acc(i):
        a = g.accessors[i]
        bv = g.bufferViews[a.bufferView]
        comp = {5126: np.float32, 5125: np.uint32, 5123: np.uint16, 5121: np.uint8}[a.componentType]
        n = {"SCALAR": 1, "VEC2": 2, "VEC3": 3, "VEC4": 4}[a.type]
        off = (bv.byteOffset or 0) + (a.byteOffset or 0)
        arr = np.frombuffer(blob, dtype=comp, count=a.count * n, offset=off)
        return arr.reshape(a.count, n) if n > 1 else arr

    p = g.meshes[0].primitives[0]
    V = acc(p.attributes.POSITION).astype(np.float64)
    N = acc(p.attributes.NORMAL).astype(np.float64)
    UV = acc(p.attributes.TEXCOORD_0).astype(np.float64)
    I = acc(p.indices).astype(np.int64).reshape(-1, 3)
    im = g.images[0]
    bv = g.bufferViews[im.bufferView]
    tex = Image.open(io.BytesIO(blob[(bv.byteOffset or 0):(bv.byteOffset or 0) + bv.byteLength])).convert("RGB")
    np.savez(key + ".npz", V=V, N=N, UV=UV, I=I, stamp=stamp)
    tex.save(key + ".png")
    return V, N, UV, I, tex


def gltf_to_unity(V, N, I):
    """Mirror x (right-handed glTF -> left-handed Unity) and fix the winding."""
    V = V.copy(); V[:, 0] = -V[:, 0]
    N = N.copy(); N[:, 0] = -N[:, 0]
    I = I[:, [0, 2, 1]].copy()
    return V, N, I


def components(nv, I):
    """Connected components of the triangle graph: (count, label per vertex)."""
    import scipy.sparse as sp
    from scipy.sparse.csgraph import connected_components
    r = np.concatenate([I[:, 0], I[:, 1], I[:, 2]])
    c = np.concatenate([I[:, 1], I[:, 2], I[:, 0]])
    A = sp.coo_matrix((np.ones(len(r), np.int8), (r, c)), shape=(nv, nv)).tocsr()
    return connected_components(A, directed=False)


def weld(V, I, tol):
    """Position weld (UV seams split vertices): returns the representative index of every vertex."""
    q = np.round(V / tol).astype(np.int64)
    _, rep, inv = np.unique(q, axis=0, return_index=True, return_inverse=True)
    return rep[inv.ravel()]


def vertex_normals(V, I):
    fn = np.cross(V[I[:, 1]] - V[I[:, 0]], V[I[:, 2]] - V[I[:, 0]])
    n = np.zeros_like(V)
    for k in range(3):
        np.add.at(n, I[:, k], fn)
    l = np.linalg.norm(n, axis=1, keepdims=True)
    return n / np.maximum(l, 1e-20)


def srgb_to_lin(c):
    c = np.asarray(c, np.float64)
    return np.where(c <= 0.04045, c / 12.92, ((c + 0.055) / 1.055) ** 2.4)


def lin_to_srgb(c):
    c = np.clip(np.asarray(c, np.float64), 0, 1)
    return np.where(c <= 0.0031308, c * 12.92, 1.055 * np.power(c, 1 / 2.4) - 0.055)
