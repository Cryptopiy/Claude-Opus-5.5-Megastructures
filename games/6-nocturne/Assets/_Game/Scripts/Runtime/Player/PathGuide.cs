// PathGuide: the invisible corridor (MASTER_PROMPT 14.4 Colliders, Part 9 "corridor guarantee", 11.8), Thread's generic
// collider builder fed by NocLanes (IMPORT_PLAN 3.11; lesson 11: colliders derived from the walkable-width function).
// BoxColliders on layer 30 (no renderers) under a root named "Corridor": per segment (at most 10 m on straight constant-width
// legs, 2 m on the fillet arcs and gentle width changes, 0.25 m on steep funnels) a 1 m thick floor box whose top is the walk
// plane y 0 (extended to overlap its neighbours by 0.5 m), and a wall 2 m thick and 6 m tall on both sides spanning exactly the
// segment, its inner face through the corridor points at lane half-width + player radius + the controller skin (WallOffset;
// PlayerTuning.WallGap) and pushed outward by the largest clearance deficit over the segment's OWN range only (chord sagitta on
// arcs (L^2 / 8R: 2 m on r 24 = 2 cm), funnel curvature; audit S2 item 5). Every joint between two wall boxes gets a corner box
// (1 m long along the mean wall direction, its own clearance shift sampled over its extent), so convex joints have no wedge gap
// and no box reaches into a neighbour's clearance. End caps behind the start and beyond the goal, and kill-volume triggers 5 m
// below the walk plane (the controller's safety volume y &lt; -3 fires first; a kill logs [NOC-FAIL] and must never happen).
// CoverageCheck runs the ray-cast grid of 11.8 with a per-s tolerance: every wall ray must hit at the analytic face distance of
// the wall box that covers s (+ 1 cm), never closer than half-width + radius + skin - 1 mm. Thread QA reads KillCount /
// CoverageCheck by reflection (QaCommon.PathGuideProbe, type name "Megastructure.PathGuide").
using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Megastructure
{
    [DefaultExecutionOrder(-20)]
    public class PathGuide : MonoBehaviour
    {
        public static PathGuide Instance { get; private set; }

        public const float WallThickness = 2.0f;
        public const float WallHeight = 6.0f;           // 14.4: 6 m side walls
        public const float WallBelowFloor = 1.0f;       // walls reach into the floor box: no gap under them
        public const float SlabThickness = 1.0f;        // 14.4: 1 m thick floor boxes
        public const float SegmentOverlap = 0.5f;
        public const double MaxSegment = 10.0;
        public const double CurvedSegment = 2.0;
        public const double SteepSegment = 0.25;
        public const float KillDepth = 5.0f;
        public const float KillThickness = 20.0f;
        public const float KillLateralMargin = 10.0f;
        public const double GridStepS = 1.0;
        public const double GridStepLateral = 0.5;
        public const float GridRayHeight = 1.0f;
        /// <summary>Coverage: a wall ray must hit within the analytic face distance + 1 cm (per s, audit S2 item 5).</summary>
        public const double WallHitTolerance = 0.01;
        /// <summary>Coverage: no wall face closer than half-width + radius + skin minus 1 mm (no intrusion into the lane).</summary>
        public const double WallIntrusionTolerance = 0.001;
        public const float CornerLength = 1.0f;
        const int ShiftSamples = 16;

        /// <summary>One wall box face (for the per-s coverage check): its s range, side, and the face line in plan.</summary>
        struct WallFace { public double s0, s1, px, pz, dx, dz, shift; public int side; public bool corner; }
        readonly List<WallFace> m_faces = new List<WallFace>(4096);

        Transform m_root, m_killRoot;
        bool m_built;
        int m_colliders, m_segments;
        readonly Collider[] m_overlap = new Collider[16];
        double m_safeS, m_safeLat;
        bool m_haveSafe;
        int m_killCount;
        double m_maxShift;

        public bool Built => m_built;
        public int ColliderCount => m_colliders;
        public int SegmentCount => m_segments;
        /// <summary>Kill-volume events (must stay 0 in every test).</summary>
        public int KillCount => m_killCount;
        public Transform CorridorRoot => m_root;

        static FilletPath P => NocLanes.Path;

        void Awake() { Instance = this; }
        void OnDestroy() { if (Instance == this) Instance = null; }

        // The corridor depends only on NocLanes, so it is built at Start (before the first move).
        void Start() { Build(); }

        // ---------------------------------------------------------------- segmentation
        /// <summary>Segment boundaries along s: every lane breakpoint and arc end, then each span split by its maximum length.</summary>
        public static List<double> SegmentBounds()
        {
            var cuts = new List<double> { 0.0, P.Length };
            foreach (double b in NocLanes.Breakpoints()) if (b > 0.0 && b < P.Length) cuts.Add(b);
            for (int i = 0; P.Arc(i, out double d0, out double d1, out _, out _, out _); i++) { cuts.Add(d0); cuts.Add(d1); }
            cuts.Sort();
            var outList = new List<double> { cuts[0] };
            for (int k = 1; k < cuts.Count; k++)
            {
                double a = outList[outList.Count - 1], b = cuts[k];
                if (b - a < 1e-3) continue;
                double maxLen = SpanMaxLength(a, b);
                int n = Math.Max(1, (int)Math.Ceiling((b - a) / maxLen - 1e-9));
                for (int i = 1; i <= n; i++) outList.Add(a + (b - a) * i / n);
            }
            return outList;
        }

        static double SpanMaxLength(double a, double b)
        {
            double mid = 0.5 * (a + b);
            if (P.OnArc(mid)) return CurvedSegment;
            double maxSlope = 0.0;
            const int n = 32;
            for (int i = 0; i < n; i++)
            {
                double s0 = a + (b - a) * i / n, s1 = a + (b - a) * (i + 1) / n;
                maxSlope = Math.Max(maxSlope, Math.Abs(NocLanes.LaneHalfWidth(s1) - NocLanes.LaneHalfWidth(s0)) / (s1 - s0));
            }
            if (maxSlope > 0.25) return SteepSegment;
            if (maxSlope > 1e-4) return CurvedSegment;
            return MaxSegment;
        }

        // ---------------------------------------------------------------- build
        public void Build()
        {
            if (m_built) return;
            var sw = Stopwatch.StartNew();
            var rootGo = new GameObject("Corridor");
            rootGo.layer = PlayerTuning.CorridorLayer;
            m_root = rootGo.transform;
            m_root.SetParent(transform, false);
            var killGo = new GameObject("KillVolume");
            killGo.layer = PlayerTuning.CorridorLayer;
            m_killRoot = killGo.transform;
            m_killRoot.SetParent(m_root, false);

            var bounds = SegmentBounds();
            for (int i = 0; i + 1 < bounds.Count; i++) { BuildSegment(bounds[i], bounds[i + 1]); m_segments++; }
            for (int i = 1; i + 1 < bounds.Count; i++) { BuildCorner(bounds[i], -1); BuildCorner(bounds[i], +1); }
            BuildEndCap(0.0, -1.0);
            BuildEndCap(P.Length, +1.0);

            Physics.SyncTransforms();
            m_built = true;
            Debug.Log(FormattableString.Invariant($"[NOC] corridor: {m_segments} segments, {m_colliders} colliders on layer {PlayerTuning.CorridorLayer}, max wall shift {m_maxShift * 100.0:F2} cm, built in {sw.Elapsed.TotalMilliseconds:F1} ms"));
        }

        /// <summary>Lateral distance of the wall face from the centre line at s: half-width + (radius + gap) measured perpendicular
        /// to the walkable edge (scaled by sqrt(1 + h'^2) where the lane narrows or widens).</summary>
        public static double WallOffset(double s)
        {
            const double e = 0.01;
            double sa = Math.Max(0.0, s - e), sb = Math.Min(P.Length, s + e);
            double dh = (NocLanes.LaneHalfWidth(sb) - NocLanes.LaneHalfWidth(sa)) / Math.Max(1e-6, sb - sa);
            double clear = NocturneConstants.PlayerRadius + PlayerTuning.WallGap;
            return NocLanes.LaneHalfWidth(s) + clear * Math.Sqrt(1.0 + dh * dh);
        }

        void BuildSegment(double s0, double s1)
        {
            double d0 = WallOffset(s0), d1 = WallOffset(s1);

            // floor box: top face at the walk plane y 0 under the centre-line chord
            Vector3 c0 = NocLanes.CentreAt(s0), c1 = NocLanes.CentreAt(s1);
            float halfW = (float)Math.Max(d0, d1) + WallThickness;
            // on an arc the outer side of a chord box is shorter than the arc it must cover: extend each end by
            // halfW tan(dTheta / 2) (r 24, 2 m segments, 13 m half-width: 0.55 m) so neighbouring floor boxes always overlap
            double dTheta = Math.Abs(NocMath.DeltaAngle(P.HeadingDeg(s0), P.HeadingDeg(s1))) * NocMath.Deg;
            float floorExtra = (float)(2.0 * halfW * Math.Tan(0.5 * dTheta));
            AddBox("floor", m_root, c0, c1, 2f * halfW, -SlabThickness * 0.5f, SlabThickness, false, floorExtra);

            // walls: level boxes spanning exactly the segment, whose face passes through the corridor points at +-WallOffset,
            // pushed outward by the largest clearance deficit over the segment's own range (audit S2 item 5: an extended range
            // over-pushed neighbouring boxes; the joints are closed by corner boxes instead of overlaps)
            for (int side = -1; side <= 1; side += 2)
            {
                Vector3 f0 = NocLanes.CorridorToWorld(s0, side * d0, 0.0);
                Vector3 f1 = NocLanes.CorridorToWorld(s1, side * d1, 0.0);
                double dx = f1.x - f0.x, dz = f1.z - f0.z, len = Math.Sqrt(dx * dx + dz * dz);
                if (len < 1e-6) continue;
                double fx = dx / len, fz = dz / len;
                double shift = ClearanceShift(f0.x, f0.z, fx, fz, side, s0, s1);
                AddWall(side < 0 ? "wallL" : "wallR", 0.5 * (f0.x + f1.x), 0.5 * (f0.z + f1.z), fx, fz, side, shift, len);
                double rx = fz, rz = -fx, off = side * shift;
                m_faces.Add(new WallFace { s0 = s0, s1 = s1, side = side, px = f0.x + rx * off, pz = f0.z + rz * off, dx = fx, dz = fz, shift = shift });
            }

            // kill volume: a trigger slab KillDepth below the walk plane, wider than the corridor
            AddBox("kill", m_killRoot, c0 - Vector3.up * KillDepth, c1 - Vector3.up * KillDepth, 2f * (halfW + KillLateralMargin), -KillThickness * 0.5f, KillThickness, true,
                   (float)(2.0 * (halfW + KillLateralMargin) * Math.Tan(0.5 * dTheta)));
        }

        /// <summary>Outward shift (m) that keeps every lane-edge point of [sa, sb] at radius + skin from the face line through
        /// (px, pz) with direction (fx, fz) on the given side.</summary>
        static double ClearanceShift(double px, double pz, double fx, double fz, int side, double sa, double sb)
        {
            double rx = fz, rz = -fx;                       // plan right of the wall direction (Unity LookRotation)
            double nx = -side * rx, nz = -side * rz;        // face normal, pointing into the corridor
            double need = NocturneConstants.PlayerRadius + PlayerTuning.WallGap, shift = 0.0;
            for (int k = 0; k <= ShiftSamples; k++)
            {
                double sk = PlayerMath.ClampS(sa + (sb - sa) * k / ShiftSamples);
                Vector3 e = NocLanes.CorridorToWorld(sk, side * NocLanes.LaneHalfWidth(sk), 0.0);
                double dist = (e.x - px) * nx + (e.z - pz) * nz;
                if (need - dist > shift) shift = need - dist;
            }
            return shift;
        }

        /// <summary>A wall box of the given length centred (plan) on (cx, cz) + the shift, its inner face on the corridor side.</summary>
        void AddWall(string name, double cx, double cz, double fx, double fz, int side, double shift, double len)
        {
            double rx = fz, rz = -fx;
            double yLo = -WallBelowFloor, yHi = WallHeight;
            double off = side * (0.5 * WallThickness + shift);
            var centre = new Vector3((float)(cx + rx * off), (float)(0.5 * (yLo + yHi)), (float)(cz + rz * off));
            var go = new GameObject(name);
            go.layer = PlayerTuning.CorridorLayer;
            go.transform.SetParent(m_root, false);
            go.transform.SetPositionAndRotation(centre, Quaternion.LookRotation(new Vector3((float)fx, 0f, (float)fz), Vector3.up));
            var bc = go.AddComponent<BoxCollider>();
            bc.size = new Vector3(WallThickness, (float)(yHi - yLo), (float)len);
            m_colliders++;
            if (shift > m_maxShift) m_maxShift = shift;
        }

        /// <summary>Corner box at the joint s of two wall boxes: the wall chord from s - 0.5 to s + 0.5 m of path (the box spans
        /// exactly that chord, centred on the corridor point at WallOffset(s)), shifted outward by its own clearance deficit over
        /// the same s range (closes convex wedges; on the inner side of an arc a fixed 1 m box would reach 0.9 m of path beyond
        /// its sampled range and intrude by ~6 mm, seen in a1_smoke at s 184-198).</summary>
        void BuildCorner(double s, int side)
        {
            const double e = 0.01;
            double h = 0.5 * CornerLength;
            double sa = PlayerMath.ClampS(s - h), sb = PlayerMath.ClampS(s + h);
            Vector3 a = NocLanes.CorridorToWorld(sa, side * WallOffset(sa), 0.0);
            Vector3 b = NocLanes.CorridorToWorld(sb, side * WallOffset(sb), 0.0);
            Vector3 f = NocLanes.CorridorToWorld(s, side * WallOffset(s), 0.0);
            double dx = b.x - a.x, dz = b.z - a.z, len = Math.Sqrt(dx * dx + dz * dz);
            if (len < e) return;
            double fx = dx / len, fz = dz / len;
            double shift = ClearanceShift(f.x, f.z, fx, fz, side, sa, sb);
            // centre on the chord midpoint projected through f: the box covers a..b along (fx, fz)
            double along = ((a.x + b.x) * 0.5 - f.x) * fx + ((a.z + b.z) * 0.5 - f.z) * fz;
            AddWall(side < 0 ? "cornerL" : "cornerR", f.x + fx * along, f.z + fz * along, fx, fz, side, shift, len);
            double rx = fz, rz = -fx, off = side * shift;
            m_faces.Add(new WallFace { s0 = sa, s1 = sb, side = side, px = f.x + rx * off, pz = f.z + rz * off, dx = fx, dz = fz, shift = shift, corner = true });
        }

        void BuildEndCap(double s, double dir)
        {
            P.TangentAt(s, out double tx, out double tz);
            Vector3 p = NocLanes.CentreAt(s);
            double off = NocturneConstants.PlayerRadius + PlayerTuning.WallGap + WallThickness * 0.5;
            var centre = new Vector3((float)(p.x + tx * dir * off), WallHeight * 0.5f - WallBelowFloor * 0.5f, (float)(p.z + tz * dir * off));
            var go = new GameObject(dir < 0 ? "capStart" : "capEnd");
            go.layer = PlayerTuning.CorridorLayer;
            go.transform.SetParent(m_root, false);
            go.transform.SetPositionAndRotation(centre, Quaternion.LookRotation(new Vector3((float)tx, 0f, (float)tz), Vector3.up));
            var bc = go.AddComponent<BoxCollider>();
            bc.size = new Vector3(2f * ((float)WallOffset(s) + WallThickness), WallHeight + WallBelowFloor, WallThickness);
            m_colliders++;
        }

        void AddBox(string name, Transform parent, Vector3 a0, Vector3 a1, float width, float upOffset, float height, bool trigger, float extraLength = 0f)
        {
            Vector3 dir = a1 - a0;
            float len = dir.magnitude;
            if (len < 1e-4f) return;
            Vector3 fwd = dir / len;
            Quaternion rot = Quaternion.LookRotation(fwd, Vector3.up);
            Vector3 up = rot * Vector3.up;
            Vector3 centre = 0.5f * (a0 + a1) + up * upOffset;
            var go = new GameObject(name);
            go.layer = PlayerTuning.CorridorLayer;
            go.transform.SetParent(parent, false);
            go.transform.SetPositionAndRotation(centre, rot);
            var bc = go.AddComponent<BoxCollider>();
            bc.size = new Vector3(width, height, len + SegmentOverlap + extraLength);
            bc.isTrigger = trigger;
            m_colliders++;
        }

        // ---------------------------------------------------------------- kill volume (runtime)
        void LateUpdate()
        {
            if (!m_built) return;
            var p = NocServices.Player;
            if (p == null) return;
            bool killed = false;
            Vector3 eye = p.EyePosition;
            if (float.IsNaN(eye.x) || float.IsNaN(eye.y) || float.IsNaN(eye.z) || eye.y < -KillDepth) killed = true;
            else
            {
                PlayerController.Capsule(p.S, p.Lateral, out Vector3 c0, out Vector3 c1, out float r);
                int n = Physics.OverlapCapsuleNonAlloc(c0, c1, r, m_overlap, PlayerTuning.CorridorMask, QueryTriggerInteraction.Collide);
                for (int i = 0; i < n; i++)
                {
                    Collider c = m_overlap[i];
                    if (c != null && c.isTrigger && c.transform.parent == m_killRoot) { killed = true; break; }
                }
            }
            if (!killed)
            {
                m_safeS = p.S; m_safeLat = p.Lateral; m_haveSafe = true;
                return;
            }
            m_killCount++;
            double s = m_haveSafe ? m_safeS : CliArgs.StartS;
            double lat = m_haveSafe ? m_safeLat : 0.0;
            Debug.LogError(FormattableString.Invariant($"[NOC-FAIL] kill volume at s={p.S:F2} lateral={p.Lateral:F2} eye={eye}; returned to s={s:F2} lateral={lat:F2}"));
            var f = NocServices.Fade;
            if (f != null) f.Blink();
            p.Teleport(s, lat);
        }

        // ---------------------------------------------------------------- coverage (11.8 ray-cast grid)
        public bool CoverageCheck(out int misses) => CoverageCheck(out misses, out _);

        /// <summary>Every 1 m along s and every 0.5 m across the walkable width (plus the exact edges): a ray from 1 m above the
        /// floor straight down must hit a floor box within 1.5 m; on both sides a horizontal ray from the centre line at 1 m must hit
        /// a wall no farther than the analytic face distance of the wall box covering s + 1 cm, and no closer than half-width +
        /// radius + skin - 1 mm (per-s tolerance; audit S2 item 5); at both ends a ray along the path must hit the end cap.</summary>
        public bool CoverageCheck(out int misses, out int rays)
        {
            misses = 0; rays = 0;
            if (!m_built) Build();
            Physics.SyncTransforms();
            int mask = PlayerTuning.CorridorMask;
            int firstMissLogs = 0;
            int steps = (int)Math.Floor(P.Length / GridStepS);
            for (int k = 0; k <= steps; k++)
            {
                double s = Math.Min(P.Length, k * GridStepS);
                double h = NocLanes.LaneHalfWidth(s);
                int nLat = (int)Math.Floor(2.0 * h / GridStepLateral + 1e-9);
                for (int j = 0; j <= nLat + 1; j++)
                {
                    double lat = j <= nLat ? -h + j * GridStepLateral : h;
                    Vector3 o = NocLanes.CorridorToWorld(s, lat, GridRayHeight);
                    rays++;
                    if (!Physics.Raycast(o, Vector3.down, GridRayHeight + 0.5f, mask, QueryTriggerInteraction.Ignore))
                    {
                        misses++;
                        if (firstMissLogs++ < 10) Debug.LogError(FormattableString.Invariant($"[NOC-FAIL] corridor floor ray miss at s={s:F1} lateral={lat:F2}"));
                    }
                }
                P.RightAt(s, out double rx, out double rz);
                Vector3 c = NocLanes.CorridorToWorld(s, 0.0, GridRayHeight);
                double minFace = h + NocturneConstants.PlayerRadius + PlayerTuning.WallGap - WallIntrusionTolerance;
                for (int side = -1; side <= 1; side += 2)
                {
                    var d = new Vector3((float)(rx * side), 0f, (float)(rz * side));
                    rays++;
                    double expect = ExpectedFaceDistance(s, side, c.x, c.z, rx * side, rz * side);   // farthest covering face
                    if (double.IsNaN(expect)) expect = WallOffset(s);
                    bool hit = Physics.Raycast(c, d, out RaycastHit rh, (float)(expect + WallHitTolerance), mask, QueryTriggerInteraction.Ignore);
                    if (!hit || rh.distance < minFace)
                    {
                        misses++;
                        if (firstMissLogs++ < 10)
                            Debug.LogError(FormattableString.Invariant($"[NOC-FAIL] corridor wall ray {(hit ? "too close" : "miss")} at s={s:F1} side={side}: hit {(hit ? rh.distance : -1f):F3} m, face expected {expect:F3} m, minimum {minFace:F3} m"));
                    }
                }
            }
            for (int e = 0; e < 2; e++)
            {
                double s = e == 0 ? GridStepS * 0.5 : P.Length - GridStepS * 0.5;
                P.TangentAt(s, out double tx, out double tz);
                float sign = e == 0 ? -1f : 1f;
                Vector3 o = NocLanes.CorridorToWorld(s, 0.0, GridRayHeight);
                rays++;
                if (!Physics.Raycast(o, new Vector3((float)tx * sign, 0f, (float)tz * sign), 1.5f, mask, QueryTriggerInteraction.Ignore))
                {
                    misses++;
                    Debug.LogError(FormattableString.Invariant($"[NOC-FAIL] corridor end cap ray miss at s={s:F1}"));
                }
            }
            Debug.Log(FormattableString.Invariant($"[NOC] corridor coverage: {rays} rays, {misses} misses"));
            return misses == 0;
        }

        /// <summary>Distance along the ray (ox, oz) + a (ux, uz) to the farthest face line of the wall and corner boxes on that side
        /// whose s range contains s (NaN if none): the ray must hit a collider no later than that (+ 1 cm); the minimum-clearance
        /// check bounds it from the other side.</summary>
        double ExpectedFaceDistance(double s, int side, double ox, double oz, double ux, double uz)
        {
            double best = double.NaN;
            for (int i = 0; i < m_faces.Count; i++)
            {
                WallFace f = m_faces[i];
                if (f.side != side || s < f.s0 - 1e-6 || s > f.s1 + 1e-6) continue;
                // o + a u = p + t d  ->  a = cross(p - o, d) / cross(u, d)
                double den = ux * f.dz - uz * f.dx;
                if (Math.Abs(den) < 1e-9) continue;
                double a = ((f.px - ox) * f.dz - (f.pz - oz) * f.dx) / den;
                if (a > 0 && (double.IsNaN(best) || a > best)) best = a;
            }
            return best;
        }

        /// <summary>Outward shift of the wall box on the given side at s (largest of the boxes covering s; QA reports).</summary>
        public double WallShiftAt(double s, int side)
        {
            double best = 0.0;
            for (int i = 0; i < m_faces.Count; i++)
                if (m_faces[i].side == side && s >= m_faces[i].s0 - 1e-6 && s <= m_faces[i].s1 + 1e-6) best = Math.Max(best, m_faces[i].shift);
            return best;
        }

        /// <summary>True if the walker capsule at (s, lateral) intersects a solid corridor collider (WallBot probe).</summary>
        public static bool CapsuleBlockedAt(double s, double lateral)
        {
            PlayerController.Capsule(s, lateral, out Vector3 p0, out Vector3 p1, out float r);
            return Physics.CheckCapsule(p0, p1, r, PlayerTuning.CorridorMask, QueryTriggerInteraction.Ignore);
        }
    }
}
