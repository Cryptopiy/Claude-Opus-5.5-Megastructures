"""Body edits on the welded high-poly mesh (eye sockets, arm cut and tube morph), smooth normals, AO, decimation."""
import os, sys
sys.path.insert(0, os.path.dirname(__file__))
import numpy as np
import scipy.sparse as sp
from scipy.spatial import cKDTree
from kr_common import *

# Eyes (glTF units, Unity handedness): guess point on the head, eyeball radius (units; 220 m/u). Addendum A1 1.2: per side
# one main eye (eyeball 20 m, the measured painted eye of the Tripo model at (0.182, 0.521, +-0.223)) and three lesser
# eyes (8, 6 milky, 4 m) on an arc above-behind it ("behind" = +x here, towards the mantle). Names ending in _r / _l.
EYES = [
    ("main_r", (0.182, 0.521, 0.223), 10.0 / 220),
    ("main_l", (0.180, 0.526, -0.223), 10.0 / 220),
    ("l8_r", (0.232, 0.588, 0.212), 4.0 / 220),
    ("l6milky_r", (0.266, 0.622, 0.190), 3.0 / 220),
    ("l4_r", (0.296, 0.641, 0.160), 2.0 / 220),
    ("l8_l", (0.236, 0.584, -0.214), 4.0 / 220),
    ("l6milky_l", (0.270, 0.618, -0.192), 3.0 / 220),
    ("l4_l", (0.300, 0.638, -0.162), 2.0 / 220),
]
# socket aperture (A1 1.2: lid aperture 16 x 11 m for the 20 m eye): horizontal half-width 0.8 R, vertical 0.55 R
APERTURE_V = 0.55 / 0.8
U_CUT = 1.30            # arc from the crown where the Tripo arms end and the procedural tentacles continue (units)
U_MORPH = 0.16          # length of the zone whose cross-section is morphed to the tube's circle (units)


def neighbours(nv, I):
    e = np.concatenate([I[:, [0, 1]], I[:, [1, 2]], I[:, [2, 0]]])
    r = np.r_[e[:, 0], e[:, 1]]; c = np.r_[e[:, 1], e[:, 0]]
    A = sp.coo_matrix((np.ones(len(r), np.float32), (r, c)), shape=(nv, nv)).tocsr()
    A.data[:] = 1.0
    return A


def smooth_field(A, F, iters):
    deg = np.asarray(A.sum(1)).ravel()
    Dinv = sp.diags(1.0 / np.maximum(deg, 1))
    M = (Dinv @ A).tocsr()
    for _ in range(iters):
        F = 0.5 * F + 0.5 * (M @ F)
    return F


def normalize(v):
    return v / np.maximum(np.linalg.norm(v, axis=-1, keepdims=True), 1e-20)


def _ss(a, b, t):
    t = np.clip((t - a) / (b - a), 0, 1)
    return t * t * t * (t * (t * 6 - 15) + 10)


def carve_eyes(W, w_skin, n_smooth):
    """Deep sockets with a raised, folded rim; returns (W', eyes list, socket glow per vertex)."""
    W = W.copy()
    glow = np.zeros(len(W))
    head = np.where((w_skin < 0.25) & (W[:, 1] > 0.33))[0]
    tree = cKDTree(W[head])
    eyes = []
    for name, g, R in EYES:
        _, j = tree.query(np.array(g))
        S = W[head[j]].copy()
        near = np.linalg.norm(W - S, axis=1) < 0.6 * R
        n = normalize(n_smooth[near].mean(0))
        d = W - S
        h = d @ n
        latv = d - h[:, None] * n
        lat = np.linalg.norm(latv, axis=1)
        sel = (lat < 2.3 * R) & (np.abs(h) < 1.2 * R) & ((n_smooth @ n) > 0.0)
        ref = normalize(np.cross(n, [0.0, 1.0, 0.0]) if abs(n[1]) < 0.9 else np.cross(n, [1.0, 0.0, 0.0]))   # horizontal
        ref2 = np.cross(n, ref)                                                                                  # vertical
        phi = np.arctan2(latv @ ref2, latv @ ref)
        # elliptical aperture: the vertical axis is compressed (16 x 11 m lids), the rim and folds follow the ellipse
        x = np.sqrt((latv @ ref) ** 2 + ((latv @ ref2) / APERTURE_V) ** 2) / R
        rim = 0.45 * _ss(0.70, 1.05, x) * (1 - _ss(1.12, 2.25, x))
        folds = 0.13 * np.sin(7 * phi + 3.0 * np.sin(3 * phi + 1.7 * len(eyes))) * _ss(1.0, 1.25, x) * (1 - _ss(1.4, 2.25, x))
        pit = -0.85 * (1 - _ss(0.62, 0.80, x))
        disp = (pit + rim + folds) * R
        W[sel] += disp[sel, None] * n[None, :]
        gl = np.exp(-((x - 0.92) / 0.35) ** 2) * min(1.0, (R / 0.057) ** 0.5)
        glow[sel] = np.maximum(glow[sel], gl[sel])
        centre = S - n * 0.70 * R          # sunk: the lids (rim + 0.2..0.45 R) overlap the eyeball's edge everywhere
        eyes.append(dict(name=name, surface=S, normal=n, radius=R, centre=centre))
        log("eye %-8s at %s n %s R %.3f u (%.1f m)" % (name, np.round(S, 3), np.round(n, 2), R, R * MODEL_SCALE))
    return W, eyes, glow


def cut_arms(W, I, attrs):
    """Morphs the last U_MORPH of every arm to a circle and drops the triangles past U_CUT.
    Returns (W', kept triangle mask, per-arm cut info)."""
    W = W.copy()
    limb, u, th, w = attrs["limb"], attrs["u"], attrs["theta"], attrs["w"]
    lines = attrs["lines"]
    arm = w > 0.5
    info = []
    for k, L in enumerate(lines):
        m = (limb == k) & arm
        s = L["s"]
        rad_s = np.convolve(np.r_[np.full(7, L["rad"][0]), L["rad"], np.full(7, L["rad"][-1])], np.ones(15) / 15, mode="valid")
        rcut = float(np.interp(U_CUT, s, rad_s))
        mm = np.where(m & (u > U_CUT - U_MORPH) & (u <= U_CUT + 0.03))[0]
        uu = np.clip(u[mm], 0, s[-1])
        P = np.stack([np.interp(uu, s, L["P"][:, j]) for j in range(3)], 1)
        Nn = normalize(np.stack([np.interp(uu, s, L["N"][:, j]) for j in range(3)], 1))
        B = normalize(np.stack([np.interp(uu, s, L["B"][:, j]) for j in range(3)], 1))
        r = np.interp(uu, s, rad_s)
        target = P + r[:, None] * (np.cos(th[mm])[:, None] * Nn + np.sin(th[mm])[:, None] * B)
        t = np.clip((uu - (U_CUT - U_MORPH)) / (0.85 * U_MORPH), 0, 1)
        b = t * t * (3 - 2 * t)
        W[mm] = W[mm] * (1 - b[:, None]) + target * b[:, None]
        # the end frame (rest frame of the procedural continuation)
        j = int(np.searchsorted(s, U_CUT))
        info.append(dict(index=k, u_cut=U_CUT, r_cut=rcut, rad_profile=rad_s, s=s))
        log("arm %d: cut at %.3f u (%.1f m), radius %.4f u (%.2f m)" % (k, U_CUT, U_CUT * MODEL_SCALE, rcut, rcut * MODEL_SCALE))
    drop = (arm & (u > U_CUT))[I].any(axis=1)
    log("cut: dropped %d of %d triangles" % (drop.sum(), len(I)))
    return W, ~drop, info


def voxel_ao(W, Wq, nq, cell=0.012, dirs=20, steps=26):
    """Ambient occlusion of query points from a filled voxelisation of the surface W. Hemisphere around nq."""
    from scipy import ndimage
    lo = W.min(0) - 4 * cell; hi = W.max(0) + 4 * cell
    dims = np.ceil((hi - lo) / cell).astype(int) + 1
    occ = np.zeros(dims, bool)
    ij = np.floor((W - lo) / cell).astype(int)
    occ[ij[:, 0], ij[:, 1], ij[:, 2]] = True
    occ = ndimage.binary_closing(occ, iterations=1)
    occ = ndimage.binary_fill_holes(occ)
    rng = np.random.default_rng(7)
    gdir = normalize(rng.normal(size=(dirs, 3)))
    ao = np.zeros(len(Wq))
    for c0 in range(0, len(Wq), 40000):
        p = Wq[c0:c0 + 40000]; nn = nq[c0:c0 + 40000]
        acc = np.zeros(len(p)); wsum = np.zeros(len(p))
        for di in range(dirs):
            d = gdir[di]
            dd = d[None, :] * np.where((nn @ d) < 0, -1.0, 1.0)[:, None]
            dd = normalize(dd + 0.35 * nn)
            cosw = np.clip((dd * nn).sum(1), 0, 1)
            hit = np.zeros(len(p), bool)
            for st in range(2, steps):
                q = p + dd * (st * cell * (1 + st * 0.06))
                qi = np.floor((q - lo) / cell).astype(int)
                inb = (qi >= 0).all(1) & (qi < dims).all(1)
                hv = np.zeros(len(p), bool)
                hv[inb] = occ[qi[inb, 0], qi[inb, 1], qi[inb, 2]]
                hit |= hv
            acc += cosw * hit; wsum += cosw
        ao[c0:c0 + 40000] = 1 - acc / np.maximum(wsum, 1e-6)
    return ao


def _write_obj(path, W, I, wedge_uv):
    """OBJ with per-wedge uvs and a material that names a texture: pymeshlab then gives every face texture 0 (its
    texture-aware quadric refuses faces without a texture index)."""
    d = os.path.dirname(path)
    from PIL import Image
    Image.new("RGB", (4, 4)).save(os.path.join(d, "kr_dummy.png"))
    NL = chr(10)
    with open(os.path.join(d, "kr_dummy.mtl"), "w") as f:
        f.write("newmtl m0" + NL + "Kd 1 1 1" + NL + "map_Kd kr_dummy.png" + NL)
    F = len(I)
    vt = wedge_uv.reshape(-1, 2)
    ii = I + 1
    tt = np.arange(1, 3 * F + 1).reshape(F, 3)
    fa = np.stack([ii[:, 0], tt[:, 0], ii[:, 1], tt[:, 1], ii[:, 2], tt[:, 2]], 1)
    with open(path, "w") as f:
        f.write("mtllib kr_dummy.mtl" + NL)
        np.savetxt(f, W, fmt="v %.7f %.7f %.7f")
        np.savetxt(f, vt, fmt="vt %.7f %.7f")
        f.write("usemtl m0" + NL)
        np.savetxt(f, fa, fmt="f %d/%d %d/%d %d/%d")


def decimate(W, I, wedge_uv, target, boundary_weight=3.0, preserve_boundary=True):
    import pymeshlab
    os.makedirs(CACHE_DIR, exist_ok=True)
    obj = os.path.join(CACHE_DIR, "kr_decimate_in.obj")
    _write_obj(obj, W, I, wedge_uv)
    ms = pymeshlab.MeshSet()
    ms.load_new_mesh(obj)
    ms.meshing_decimation_quadric_edge_collapse_with_texture(targetfacenum=int(target), qualitythr=0.4, extratcoordw=1.0,
        preserveboundary=bool(preserve_boundary), boundaryweight=boundary_weight, optimalplacement=True, preservenormal=True)
    ms.meshing_remove_unreferenced_vertices()
    out = ms.current_mesh()
    V = out.vertex_matrix().copy(); F = out.face_matrix().astype(np.int64).copy()
    UVw = out.wedge_tex_coord_matrix().reshape(-1, 3, 2).copy()
    log("decimated to %d triangles (%d vertices)" % (len(F), len(V)))
    return V, F, UVw


def split_by_uv(F, UVw):
    """Unity vertices = unique (position index, uv) pairs. Returns (pos idx per out vertex, uv per out vertex, triangles)."""
    key = np.concatenate([F.reshape(-1, 1).astype(np.float64), np.round(UVw.reshape(-1, 2) * 65536.0)], 1)
    uk, inv = np.unique(key, axis=0, return_inverse=True)
    inv = inv.ravel()
    pidx = uk[:, 0].astype(np.int64)
    uv = np.zeros((len(uk), 2)); uv[inv] = UVw.reshape(-1, 2)
    return pidx, uv, inv.reshape(-1, 3)


def tangents(P, Nrm, UV, T):
    """Per-vertex tangents from UV derivatives (Lengyel), orthonormalised against Nrm; w = handedness."""
    p0, p1, p2 = P[T[:, 0]], P[T[:, 1]], P[T[:, 2]]
    w0, w1, w2 = UV[T[:, 0]], UV[T[:, 1]], UV[T[:, 2]]
    e1, e2 = p1 - p0, p2 - p0
    d1, d2 = w1 - w0, w2 - w0
    r = d1[:, 0] * d2[:, 1] - d2[:, 0] * d1[:, 1]
    r = np.where(np.abs(r) < 1e-20, 1e-20, r)
    sdir = (e1 * d2[:, 1:2] - e2 * d1[:, 1:2]) / r[:, None]
    tdir = (e2 * d1[:, 0:1] - e1 * d2[:, 0:1]) / r[:, None]
    tan1 = np.zeros_like(P); tan2 = np.zeros_like(P)
    for k in range(3):
        np.add.at(tan1, T[:, k], sdir); np.add.at(tan2, T[:, k], tdir)
    t = tan1 - Nrm * (Nrm * tan1).sum(1, keepdims=True)
    bad = np.linalg.norm(t, axis=1) < 1e-12
    alt = np.cross(Nrm, np.array([0.0, 1.0, 0.0]))
    t[bad] = alt[bad]
    t = normalize(t)
    wsign = np.where((np.cross(Nrm, t) * tan2).sum(1) < 0, -1.0, 1.0)
    return t, wsign
