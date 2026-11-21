"""Regenerates everything under Assets/_Game/Creature/Kraken/ from the two Tripo3D GLBs (Addendum A1, kraken owner).

    Tools/.venv/Scripts/python.exe -I Tools/kraken/build_kraken.py [--fast]

Steps: load (glTF -> Unity handedness) -> weld, drop floaters -> crown centre, geodesic distance, the eight arms and
their centre lines (kr_segment) -> rig attributes per vertex (limb, arc u, angle theta, harmonic skin weight w; kr_attrs)
-> eye sockets carved (kr_body.EYES) -> arms cut at U_CUT with the last U_MORPH morphed to a circle (the procedural
tentacles continue there) -> smooth shading normals (all LODs share them: no shading pop) -> AO (voxel occlusion),
cavity, socket glow -> quadric decimation with texture seams kept: LOD0 330k, LOD1 80k, LOD2 20k, LOD3 4k triangles ->
textures: graded 4096 albedo, tangent-space normal (detail of the high-poly over the smooth normals + albedo
band-pass height), packed mask; the tentacle skin tile from the Tripo tentacle (relief-driven albedo, normal, mask, and
a 512 x 512 relief for the runtime displacement) -> gzip'd KRM1 meshes, PNGs with .meta importer settings, manifest.
Kraken local space: metres, origin at the arm crown, +Y up, +Z = the face (the eyes' front), the mantle towards -Z.
--fast skips nothing but writes 2048 textures (look-dev only; never commit a --fast build).
"""
import os, sys, time, json
sys.path.insert(0, os.path.dirname(__file__))
import numpy as np
from scipy.spatial import cKDTree
from scipy.ndimage import gaussian_filter
from kr_common import *
from kr_segment import segment, arms, edge_graph
from kr_attrs import rig_attributes
from kr_body import *
from kr_raster import raster, dilate
from kr_tex import *
from kr_export import *
from kr_tent import unwrap

FAST = "--fast" in sys.argv
TEX = 2048 if FAST else 4096
TENT_TEX = 1024 if FAST else 2048
LOD_TRIS = [330000, 80000, 20000, 4000]
VERSION = 1


def to_local(P, crown):
    q = (np.asarray(P) - crown) * MODEL_SCALE
    return np.stack([q[..., 2], q[..., 1], -q[..., 0]], -1)


def dir_local(D):
    D = np.asarray(D)
    return np.stack([D[..., 2], D[..., 1], -D[..., 0]], -1)


def main():
    t0 = time.time()
    os.makedirs(RES_DIR, exist_ok=True)
    V, N, UV, Iu, tex = load_glb(SRC_OCTO)
    Vu, Nu, Iu = gltf_to_unity(V, N, Iu)
    cache = os.path.join(CACHE_DIR, "seg.npz")
    if os.path.exists(cache):
        z = np.load(cache)
        seg = dict(W=z["W"], I=z["I"], used=z["used"], crown=z["crown"], angs=z["angs"], geo=z["geo"])
    else:
        seg = segment(Vu, Iu)
        np.savez(cache, **{k: seg[k] for k in ("W", "I", "used", "crown", "angs", "geo")})
    W0, Iw, crown = seg["W"], seg["I"], seg["crown"]
    rep = weld(Vu, Iu, 1e-6)
    remap = -np.ones(len(Vu), np.int64); remap[seg["used"]] = np.arange(len(seg["used"]))
    split2w = remap[rep]                                   # split (UV) vertex -> welded vertex
    assert (split2w[Iu] == Iw).all(), "welded triangles must match the source order"
    A_info, head_c, lab = arms(seg)
    R = rig_attributes(seg, A_info, head_c, lab)
    log("segmentation + attributes %.0f s" % (time.time() - t0))

    A = neighbours(len(W0), Iw)
    n0 = vertex_normals(W0, Iw)
    n0s = normalize(smooth_field(A, n0, 20))
    W1, eyes, glow = carve_eyes(W0, R["w"], n0s)
    W2, keepT, cuts = cut_arms(W1, Iw, R)
    If = Iw[keepT]; Iuf = Iu[keepT]
    n_hi = vertex_normals(W2, If)
    n_lo = normalize(smooth_field(A, n_hi, 40))
    lap = smooth_field(A, W2, 3) - W2
    cav = np.clip(0.5 + 18.0 * (lap * n_lo).sum(1), 0, 1)      # > 0.5 convex bump, < 0.5 crevice
    cav = 1.0 - cav                                              # stored: < 0.5 = crevice
    log("AO ...")
    ao = voxel_ao(W2[np.unique(If)], W2, n_lo)
    ao = np.clip(smooth_field(A, ao, 4), 0, 1)
    log("edits + AO %.0f s" % (time.time() - t0))

    # ---------------------------------------------------------------- LOD meshes
    tree = cKDTree(W2)
    u_m = R["u"] * MODEL_SCALE
    rdist_m = R["rdist"] * MODEL_SCALE
    limb_f = np.where(R["w"] > 1e-4, R["limb"].astype(np.float64), 255.0)
    wedge = UV[Iuf]                                              # (F,3,2)
    src_W, src_I, src_uv = W2, If, wedge
    lod_stats = []
    for li, target in enumerate(LOD_TRIS):
        keep_rim = li < 2                                        # LOD2/3 may simplify the arm-cut rims (tube covers them)
        import hashlib
        edits = hashlib.md5((repr(EYES) + repr(U_CUT) + repr(U_MORPH) + repr(APERTURE_V)).encode()).hexdigest()[:8]
        lc = os.path.join(CACHE_DIR, "lod%d_v%d_b%d_%s.npz" % (li, VERSION, int(keep_rim), edits))   # keyed by the body edits
        if os.path.exists(lc) and "--redo" not in sys.argv:
            z = np.load(lc); Vd, Fd, UVw = z["V"], z["F"], z["UV"]
        else:
            Vd, Fd, UVw = decimate(src_W, src_I, src_uv, target, preserve_boundary=keep_rim)
            np.savez(lc, V=Vd, F=Fd, UV=UVw)
        pidx, uv, T = split_by_uv(Fd, UVw)
        P = Vd[pidx]
        _, nn = tree.query(P)
        th = R["theta"][nn].copy()
        islimb = limb_f[nn] < 200
        # theta wraps at +-pi on the back of every arm: triangles across the wrap get duplicated corners at theta - 2 pi,
        # so the interpolated angle stays continuous inside every triangle (tentacle tile, back / underside masks)
        tth = th[T]
        wrap = ((tth.max(1) - tth.min(1)) > np.pi) & islimb[T].any(1)
        dup = {}
        extra = []
        for t in np.where(wrap)[0]:
            for c in range(3):
                vi = T[t, c]
                if th[vi] > 0:
                    if vi not in dup:
                        dup[vi] = len(nn) + len(extra); extra.append(vi)
                    T[t, c] = dup[vi]
        if extra:
            extra = np.array(extra)
            pidx = np.r_[pidx, pidx[extra]]; uv = np.vstack([uv, uv[extra]]); nn = np.r_[nn, nn[extra]]
            th = np.r_[th, th[extra] - 2 * np.pi]
        P = Vd[pidx]
        log("LOD%d: %d theta-wrap triangles, %d duplicated vertices" % (li, int(wrap.sum()), len(extra)))
        Nrm = n_lo[nn]
        tan, ws = tangents(P, Nrm, uv, T)
        Pl = to_local(P, crown); Nl = dir_local(Nrm); Tl = dir_local(tan)
        UV1 = np.stack([u_m[nn], th], 1)
        Col = np.stack([limb_f[nn], R["w"][nn], ao[nn], rdist_m[nn]], 1)
        # glTF / Tripo uv: v up (OpenGL), Unity uv is v up too
        write_mesh(os.path.join(RES_DIR, "kraken_body_lod%d.bytes" % li), Pl, Nl, np.c_[Tl, ws], uv, UV1, Col, T)
        lod_stats.append(dict(lod=li, vertices=int(len(P)), triangles=int(len(T))))
        src_W, src_I, src_uv = Vd, Fd, UVw
    log("LODs %.0f s" % (time.time() - t0))

    # ---------------------------------------------------------------- body textures
    pal = palette()
    src = np.asarray(tex).astype(np.float32) / 255.0
    if src.shape[0] != TEX:
        from PIL import Image
        src = np.asarray(tex.resize((TEX, TEX), Image.LANCZOS)).astype(np.float32) / 255.0
    Wc = classes(src)
    alb = grade(src, pal)
    lum = srgb_to_lin(src) @ np.array([0.2126, 0.7152, 0.0722])
    hbp = bandpass_height(lum, 1.0 * TEX / 4096, 9.0 * TEX / 4096)
    # tangent frames on the high-poly (split vertices), detail normal = n_hi in the (t, b, n_lo) frame
    Ps = W2[split2w]; Nl_s = n_lo[split2w]; Nh_s = n_hi[split2w]
    used_s = np.unique(Iuf)
    t_s, w_s = tangents(Ps, Nl_s, UV, Iuf)
    b_s = np.cross(Nl_s, t_s) * w_s[:, None]
    det = np.stack([(Nh_s * t_s).sum(1), (Nh_s * b_s).sum(1), (Nh_s * Nl_s).sum(1)], 1)
    vals = np.concatenate([det, ao[split2w][:, None], cav[split2w][:, None], glow[split2w][:, None]], 1)
    tri_uv = np.stack([UV[Iuf][..., 0] * TEX, (1.0 - UV[Iuf][..., 1]) * TEX], -1)
    img, msk = raster(tri_uv, vals[Iuf], TEX)
    img = dilate(img, msk, 24)
    det_n = img[..., :3] / np.maximum(np.linalg.norm(img[..., :3], axis=-1, keepdims=True), 1e-6)
    nrm = combine_normals(det_n, height_normal(hbp, 0.9))
    ao_t = img[..., 3]; cav_t = img[..., 4]; glow_t = img[..., 5]
    # dark crevices in the albedo too (8.11.4 cavity dirt), and the sockets' inner walls
    alb = alb * (0.55 + 0.45 * np.clip(ao_t, 0, 1))[..., None] * (0.75 + 0.5 * np.clip(cav_t, 0, 1))[..., None]
    smooth = smoothness_from(Wc, hbp)
    mask = np.stack([np.clip(ao_t, 0, 1), smooth, np.clip(cav_t, 0, 1), np.clip(glow_t, 0, 1)], -1)
    save_rgb(os.path.join(RES_DIR, "Kraken_Albedo.png"), lin_to_srgb(alb))
    save_rgb(os.path.join(RES_DIR, "Kraken_Normal.png"), nrm * 0.5 + 0.5)
    save_rgba(os.path.join(RES_DIR, "Kraken_Mask.png"), mask)
    texture_meta(os.path.join(RES_DIR, "Kraken_Albedo.png"), True, False, 4096, 25)
    texture_meta(os.path.join(RES_DIR, "Kraken_Normal.png"), False, True, 4096, 27)
    texture_meta(os.path.join(RES_DIR, "Kraken_Mask.png"), False, False, 4096, 25)
    log("body textures %.0f s" % (time.time() - t0))

    # ---------------------------------------------------------------- tentacle skin tile
    tt = unwrap(TENT_TEX, TENT_TEX)
    h = tt["height"]
    hs = gaussian_filter(h, 0.8 * TENT_TEX / 2048)
    theta = (np.arange(TENT_TEX) + 0.5) / TENT_TEX * 2 * np.pi - np.pi
    th = np.broadcast_to(theta[None, :], h.shape)
    under = np.exp(-(th / 1.25) ** 2)                            # underside band (theta 0) between the sucker rows
    top = np.clip((np.abs(th) - 1.9) / 0.7, 0, 1)                 # back of the tentacle
    sucker = np.clip((hs - 0.05) / 0.10, 0, 1) * np.clip(1 - top * 1.5, 0, 1)
    cup = np.clip((hs - gaussian_filter(hs, 3 * TENT_TEX / 2048)) * -14.0, 0, 1) * sucker
    growth = np.clip((hs - 0.04) / 0.08, 0, 1) * top
    srcl = srgb_to_lin(tt["albedo"]) @ np.array([0.2126, 0.7152, 0.0722])
    detail = np.clip(srcl / max(np.median(srcl), 1e-4), 0.3, 2.5) ** 0.6
    rng = np.random.default_rng(11)
    macro = gaussian_filter(rng.normal(size=h.shape), 40 * TENT_TEX / 2048, mode="wrap"); macro /= max(np.abs(macro).max(), 1e-6)
    skin = pal["KrakenSkinDark"][None, None, :] * (1.0 + 0.35 * macro[..., None])
    tal = skin * (1 - under[..., None]) + pal["KrakenUnderRed"][None, None, :] * under[..., None]
    tal = tal * (1 - growth[..., None]) + pal["KrakenMoss"][None, None, :] * growth[..., None]
    tal = tal * (1 - sucker[..., None] * 0.85) + pal["KrakenSucker"][None, None, :] * (sucker * 0.85)[..., None]
    tal = tal * (1 - 0.8 * cup)[..., None]
    # darker sucker rims (A1 1.1 rule 4: pale bone-pink suckers with darker rims) and a ring of 10 inward-curved pale hooks
    # per sucker (A1 1.2; the mask of the hooks everywhere, geometry only in beat 6 by the next wave). Suckers are the
    # connected blobs of the relief; tile pixels are anisotropic (x: 2 pi r / N, y: TILE r / N).
    from scipy import ndimage
    rim = np.clip(np.abs(hs - 0.07) < 0.025, 0, 1) * sucker
    tal = tal * (1 - 0.45 * gaussian_filter(rim.astype(np.float64), 1.0))[..., None]
    lab, nl = ndimage.label(hs > 0.06)
    px_x = 2 * np.pi / TENT_TEX; px_y = tt["span"] / TENT_TEX            # radii per pixel
    hooks = np.zeros_like(hs); nhooks = 0
    for i, sl in enumerate(ndimage.find_objects(lab)):
        if sl is None: continue
        m = lab[sl] == i + 1
        area = m.sum() * px_x * px_y
        if area < 0.002 or area > 0.08: continue
        cy, cx = ndimage.center_of_mass(m)
        cy += sl[0].start; cx += sl[1].start
        if top[int(cy) % TENT_TEX, int(cx) % TENT_TEX] > 0.3: continue      # barnacles on the back, not suckers
        rs = np.sqrt(area / np.pi)                                           # sucker radius in tentacle radii
        for k in range(10):
            a = 2 * np.pi * k / 10 + 0.31 * i
            for st in np.linspace(0.0, 1.0, 7):                             # each hook: a short inward-curling stroke
                rr = rs * (1.02 - 0.30 * st); aa = a + 0.35 * st * st
                hx = cx + rr * np.cos(aa) / px_x; hy = cy + rr * np.sin(aa) / px_y
                w = rs * (0.07 * (1 - 0.6 * st))
                x0, x1 = int(hx - w / px_x - 2), int(hx + w / px_x + 3)
                y0, y1 = int(hy - w / px_y - 2), int(hy + w / px_y + 3)
                yy, xx = np.mgrid[y0:y1, x0:x1]
                d = np.sqrt(((xx - hx) * px_x) ** 2 + ((yy - hy) * px_y) ** 2) / max(w, 1e-6)
                val = np.clip(1.0 - d, 0, 1)
                hooks[yy % TENT_TEX, xx % TENT_TEX] = np.maximum(hooks[yy % TENT_TEX, xx % TENT_TEX], val)
            nhooks += 1
    log("tentacle tile: %d hooks on %d suckers" % (nhooks, nhooks // 10))
    tal = tal * (1 - hooks[..., None]) + (pal["KrakenSucker"] * 1.05)[None, None, :] * hooks[..., None]
    hs = hs + 0.012 * hooks
    sucker = np.maximum(sucker, hooks)
    tal = np.clip(tal * detail[..., None], 0.004, 0.75)
    tn = height_normal(hs, 40.0 * TENT_TEX / 2048)
    tsm = np.clip(0.70 - 0.25 * growth + 0.1 * under - 0.12 * sucker + 0.05 * macro, 0.25, 0.86)
    tmask = np.stack([np.clip(1 - 0.6 * cup - 0.25 * np.clip(-hs * 6, 0, 1), 0, 1), tsm, np.clip(0.5 + hs * 2.5, 0, 1), sucker], -1)
    save_rgb(os.path.join(RES_DIR, "Tentacle_Albedo.png"), lin_to_srgb(tal))
    save_rgb(os.path.join(RES_DIR, "Tentacle_Normal.png"), tn * 0.5 + 0.5)
    save_rgba(os.path.join(RES_DIR, "Tentacle_Mask.png"), tmask)
    texture_meta(os.path.join(RES_DIR, "Tentacle_Albedo.png"), True, False, 2048, 25)
    texture_meta(os.path.join(RES_DIR, "Tentacle_Normal.png"), False, True, 2048, 27)
    texture_meta(os.path.join(RES_DIR, "Tentacle_Mask.png"), False, False, 2048, 25)
    # relief for the runtime displacement (LOD0 tubes): 256 (theta) x 512 (s), float16, relative radius change
    from PIL import Image
    rel = np.asarray(Image.fromarray(hs.astype(np.float32), "F").resize((256, 512), Image.BILINEAR), np.float32)
    rpath = os.path.join(RES_DIR, "tentacle_relief.bytes")
    import gzip, struct
    with open(rpath, "wb") as raw, gzip.GzipFile(filename="", mode="wb", compresslevel=9, fileobj=raw, mtime=0) as f:
        f.write(b"KRH1" + struct.pack("<ii", 256, 512) + rel.astype("<f2").tobytes())
    text_meta(rpath)
    log("tentacle tile %.0f s" % (time.time() - t0))

    # ---------------------------------------------------------------- manifest
    arms_out = []
    for k, L in enumerate(R["lines"]):
        s = L["s"]; c = cuts[k]
        n = int(np.searchsorted(s, c["u_cut"] + 0.03))
        step = max(1, int(round(0.009 / (s[1] - s[0]))))
        idx = list(range(0, n, step)) + [n - 1]
        idx = sorted(set(idx))
        P = to_local(L["P"][idx], crown); Nv = dir_local(L["N"][idx]); Tv = dir_local(L["T"][idx])
        radm = np.interp(s[idx], c["s"], c["rad_profile"]) * MODEL_SCALE
        arms_out.append(dict(index=k, azimuthDeg=float(L["az"]), uCut=float(c["u_cut"] * MODEL_SCALE), rCut=float(c["r_cut"] * MODEL_SCALE),
                             u=[float(x * MODEL_SCALE) for x in s[idx]], p=np.round(P, 4).ravel().tolist(),
                             n=np.round(Nv, 5).ravel().tolist(), t=np.round(Tv, 5).ravel().tolist(), r=np.round(radm, 4).tolist()))
    eyes_out = []
    for e in eyes:
        eyes_out.append(dict(name=e["name"], centre=np.round(to_local(e["centre"], crown), 4).tolist(),
                             normal=np.round(dir_local(e["normal"]), 5).tolist(), radius=float(e["radius"] * MODEL_SCALE)))
    Wl = to_local(W2[np.unique(If)], crown)
    head_sel = (R["w"][np.unique(If)] < 0.05)
    Hh = Wl[head_sel]
    man = dict(version=VERSION, modelScale=MODEL_SCALE, source=["3D Models/SM_Creature_Octopus.glb", "3D Models/SM_Creature_MonsterTentacles.glb"],
               boundsMin=np.round(Wl.min(0), 3).tolist(), boundsMax=np.round(Wl.max(0), 3).tolist(),
               headMin=np.round(Hh.min(0), 3).tolist(), headMax=np.round(Hh.max(0), 3).tolist(),
               tentacleTileRadii=float(tt["span"]), lods=lod_stats, arms=arms_out, eyes=eyes_out,
               textures=dict(body=TEX, tentacle=TENT_TEX), fast=FAST)
    write_json(os.path.join(RES_DIR, "kraken_manifest.json"), man)
    for d in (os.path.join(ROOT, "Assets", "_Game", "Creature"), OUT_DIR, os.path.join(OUT_DIR, "Resources"), RES_DIR):
        folder_meta(d)
    log("done in %.0f s" % (time.time() - t0))


if __name__ == "__main__":
    main()
