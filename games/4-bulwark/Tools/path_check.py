#!/usr/bin/env python3
"""Self-check of the Bulwark path numbers (MASTER_PROMPT 14.4, 14.12) in double precision.
Mirrors BwPath.cs: turtle construction of the filleted polyline, the stair profile, the 2.5 m triangular-kernel
eye height (half-width 2.5 m), lanes and lateral keys. Usage: python -I Tools/path_check.py
"""
import math

D2R = math.pi / 180.0

# ------------------------------------------------------------------ table 14.4
X0, Z0 = -1.5, 0.0
L2_END = 934.0
A1_R, A1_H0, A1_H1 = 40.0, 0.0, 3.0
STAIRS_LEN = 15 * 18 * 0.3 + 14 * 1.2          # 97.8
A2_R, A2_H1 = 40.0, 0.86
GALLERY_END = 1370.0
A3_R, A3_H1 = 30.0, -18.0
TUNNEL_LEN = 50.1626   # BulwarkConst.TunnelLength (50.163 rounded in 14.4; see BulwarkConst)
A4_R, A4_H1 = 20.0, 0.0
PATH_LEN = 1500.0


def arc_len(r, h0, h1):
    return r * abs(h1 - h0) * D2R


def build():
    """Returns pieces: (kind, s0, len, x0, z0, h0deg, r, sigma) and the end state."""
    pieces = []
    st = {"s": 0.0, "x": X0, "z": Z0, "h": 0.0}

    def straight(L):
        pieces.append(("L", st["s"], L, st["x"], st["z"], st["h"], 0.0, 0))
        st["x"] += L * math.sin(st["h"] * D2R)
        st["z"] += L * math.cos(st["h"] * D2R)
        st["s"] += L

    def arc(r, h1):
        h = st["h"]
        sg = 1 if h1 > h else -1
        L = arc_len(r, h, h1)
        pieces.append(("A", st["s"], L, st["x"], st["z"], h, r, sg))
        st["x"] += sg * r * (math.cos(h * D2R) - math.cos(h1 * D2R))
        st["z"] += sg * r * (math.sin(h1 * D2R) - math.sin(h * D2R))
        st["s"] += L
        st["h"] = h1

    straight(L2_END)
    arc(A1_R, A1_H1)
    straight(STAIRS_LEN)
    arc(A2_R, A2_H1)
    straight(GALLERY_END - st["s"])
    arc(A3_R, A3_H1)
    straight(TUNNEL_LEN)
    arc(A4_R, A4_H1)
    straight(PATH_LEN - st["s"])
    return pieces, (st["s"], st["x"], st["z"], st["h"])


PIECES, END = build()


def piece_at(s):
    s = min(max(s, 0.0), PATH_LEN)
    for p in PIECES:
        if s < p[1] + p[2]:
            return p
    return PIECES[-1]


def point(s):
    s = min(max(s, 0.0), PATH_LEN)
    k, s0, L, x0, z0, h0, r, sg = piece_at(s)
    u = s - s0
    if k == "L":
        return x0 + u * math.sin(h0 * D2R), z0 + u * math.cos(h0 * D2R)
    h = h0 + sg * u / r / D2R
    return (x0 + sg * r * (math.cos(h0 * D2R) - math.cos(h * D2R)),
            z0 + sg * r * (math.sin(h * D2R) - math.sin(h0 * D2R)))


def heading(s):
    s = min(max(s, 0.0), PATH_LEN)
    k, s0, L, x0, z0, h0, r, sg = piece_at(s)
    if k == "L":
        return h0
    return h0 + sg * (s - s0) / r / D2R


# ------------------------------------------------------------------ floor profile
S_ST = PIECES[2][1]          # stairs leg start
FLIGHT_PITCH = 6.6
RISER, TREAD = 0.2, 0.3
PHASE = 0.15                  # option C: risers at u = 0.15 + 0.3 j


def risers(phase=PHASE):
    a = []
    for k in range(15):
        for j in range(18):
            a.append(S_ST + FLIGHT_PITCH * k + phase + TREAD * j)
    return a


def floor_y(s, phase=PHASE):
    y = 0.0
    if s >= S_ST:
        u = s - S_ST
        k = int(u // FLIGHT_PITCH)
        if k >= 15:
            y = -54.0
        else:
            lu = u - FLIGHT_PITCH * k
            n = 0 if lu < phase else min(18, int((lu - phase) // TREAD) + 1)
            y = -3.6 * k - RISER * n
    if s > 1070.0:
        y += -0.12 * (min(s, 1370.0) - 1070.0)
    return y


H = 2.5   # triangular kernel half-width


def Phi(x, h=H):
    if x <= -h:
        return 0.0
    if x <= 0:
        return (x + h) ** 2 / (2 * h * h)
    if x <= h:
        return 1.0 - (h - x) ** 2 / (2 * h * h)
    return 1.0


def Psi(x, h=H):
    if x <= -h:
        return 0.0
    if x <= 0:
        return (x + h) ** 3 / (6 * h * h)
    if x <= h:
        return x + (h - x) ** 3 / (6 * h * h)
    return x


_RIS = {}


def eye_y(s, phase=PHASE, h=H, ramp_model=False):
    y = 0.0
    if ramp_model:
        for k in range(15):
            a = S_ST + FLIGHT_PITCH * k
            y += -(2 / 3) * (Psi(s - a, h) - Psi(s - a - 5.4, h))
    else:
        if phase not in _RIS:
            _RIS[phase] = risers(phase)
        for a in _RIS[phase]:
            if a < s - h:
                y += -RISER
            elif a < s + h:
                y += -RISER * Phi(s - a, h)
    y += -0.12 * (Psi(s - 1070.0, h) - Psi(s - 1370.0, h))
    return 1.7 + y


def tread_line(s):
    """pitch line of each flight (0,0)-(5.4,-3.6), flat on landings; ramp of the gallery."""
    y = 0.0
    if s >= S_ST:
        u = s - S_ST
        k = int(u // FLIGHT_PITCH)
        if k >= 15:
            y = -54.0
        else:
            lu = u - FLIGHT_PITCH * k
            y = -3.6 * k - (2 / 3) * min(lu, 5.4)
    if s > 1070.0:
        y += -0.12 * (min(s, 1370.0) - 1070.0)
    return y


def frange(a, b, d):
    n = int((b - a) / d)
    return [a + d * i for i in range(n + 1)]


def report():
    print("pieces:")
    for p in PIECES:
        s1 = p[1] + p[2]
        x, z = point(s1)
        print(f"  {p[0]} s {p[1]:10.4f} -> {s1:10.6f} len {p[2]:9.6f} end ({x:9.6f}, {z:10.6f}) h {heading(s1 - 1e-9):7.3f}")
    print("end", END)
    table = [(934, -1.5, 934), (936.094, -1.445, 936.093), (1033.894, 3.673, 1033.759), (1035.388, 3.724, 1035.252),
             (1370, 8.746, 1369.826), (1379.875, 7.281, 1379.547), (1430.038, -8.220, 1427.254),
             (1436.321, -9.199, 1433.435), (1500, -9.199, 1497.114)]
    ends = [p[1] + p[2] for p in PIECES]
    for i, (st, tx, tz) in enumerate(table):
        e = ends[i]
        x, z = point(e)
        print(f"  end {i}: s {e:.6f} (table {st}) d={e - st:+.6f}  ({x:.6f}, {z:.6f}) d=({x - tx:+.6f}, {z - tz:+.6f})")
    lo, hi = 1380.0, 1430.0
    for _ in range(80):
        m = 0.5 * (lo + hi)
        if point(m)[0] > -6.0:
            lo = m
        else:
            hi = m
    print("lee face crossing s", lo, point(lo))
    for s in (1070, 1115, 1370):
        print("  axis", s, point(s), 4.243 + 0.015009 * (s - 1070))
    a = risers()
    print("risers", len(a), "first", a[0], "last", a[-1], "drop", floor_y(a[-1] + 0.01), floor_y(S_ST + STAIRS_LEN + 0.01))
    for phase in (0.0, 0.15, 0.3):
        for v in (2.5, 1.5):
            ds = v / 30.0
            mx, at = 0.0, 0
            for s in frange(925.0, 1080.0, 0.005):
                d2 = eye_y(s + ds, phase) - 2 * eye_y(s, phase) + eye_y(s - ds, phase)
                if abs(d2) > mx:
                    mx, at = abs(d2), s
            print(f"  phase {phase} v {v}: max |d2y| {mx * 1000:.4f} mm at s {at:.3f}")
    for v in (2.5, 1.5):
        ds = v / 30.0
        mx = max(abs(eye_y(s + ds, ramp_model=True) - 2 * eye_y(s, ramp_model=True) + eye_y(s - ds, ramp_model=True))
                 for s in frange(925.0, 1080.0, 0.01))
        print(f"  ramp model v {v}: {mx * 1000:.4f} mm")
    mx, at, mn = -9, 0, 9
    for s in frange(930.0, 1045.0, 0.01):
        d = eye_y(s) - 1.7 - tread_line(s)
        if d > mx:
            mx, at = d, s
        mn = min(mn, d)
    print(f"  eye above tread line max {mx:.4f} at {at:.2f}, min {mn:.4f}")
    mx2 = max(eye_y(s) - 1.7 - floor_y(s) for s in frange(930.0, 1045.0, 0.01))
    print(f"  eye above floor max {mx2:.4f}")
    print("  KS14 eye at s 985:", eye_y(985.0), "x", point(985.0)[0], " A:", eye_y(985.0, 0.0), " B:", eye_y(985.0, 0.3))
    for s in (1070, 1115, 1166, 1230, 1320, 1370, 1403.44, 1449.1, 1487.2):
        print("   s", s, "pt", point(s), "eye", eye_y(s))


if __name__ == "__main__":
    report()
