// PathGuide: the invisible corridor (MASTER_PROMPT 14.4, Part 9 "corridor guarantee", 11.8; D-03). Thread's collider
// GENERATOR driven by the ONE lane function BwPath.Lane(s) (capsule-centre lateral limits, funnel and door included) and the
// analytic floor BwPath.FloorY(s); no hand-listed span table (lesson 3.2 #11). The Ring's API names are kept (FloorY,
// CentreAt, DistanceOutsideLanes, LaneSamplePoints) and its conventions: boxes 1 m thick, 6 m high, every 4 m with 1 m
// overlap, fallback floors 3 cm under the walkable surface.
// Per 4 m segment: a floor slab whose top lies 3 cm below the lowest point of the analytic floor under its chord (stairs and
// the gallery ramp included), two walls whose faces keep PlayerRadius + 3 cm from the lane edges (pushed out by the largest
// deficit over the segment and its overlap, as in Thread), and a safety trigger (layer 31) 5 m below the floor. End caps
// behind the spawn and beyond the goal. A safety trigger returns the player to the last safe lane point (PlayerController,
// The Ring's snap) and counts a kill here; it must never fire. CoverageCheck runs the 0.5 m ray grid of 14.4 / 11.8.
// Stair and gallery visuals belong to the beat builders; CorridorKit offers ramp colliders for them.
// OBSTACLES (props the spec puts inside the walkable lane, e.g. the crest cart): a beat calls PathGuide.AddObstacle(bounds,
// name) in its Build (before or after the corridor exists); the pool of named boxes becomes corridor-layer BoxColliders under
// "Corridor/Obstacles" (a name is a key: adding it again replaces the box, RemoveObstacle drops it). The CharacterController
// then stops a manual player at the box (one logged rejection, no snap). QA rule (FRAMEWORK.md 4.0): an obstacle on the
// CINEMATIC path (BwPath.CinematicLateral capsule) is a failure (AutoTestRunner check "cinematicPathClearOfObstacles", and the
// walk's CharacterController rejections stay 0); elsewhere it is not: the wall bot steers round it inside the lane and
// reports the contact as kind "obstacle" (not a lane leak, not a rejection failure). ObstacleAt / CapsuleHitsObstacle are
// analytic (capsule against the boxes, no physics), so EditMode tests and the bots agree.
// Exposes Instance, KillCount and CoverageCheck (QaCommon.PathGuideProbe reads them by reflection).
using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Megastructure.Player
{
    [DefaultExecutionOrder(-20)]
    public class PathGuide : MonoBehaviour
    {
        public static PathGuide Instance { get; private set; }

        public const float WallThickness = PlayerTuning.WallThickness;   // 1 m
        public const float WallHeight = PlayerTuning.WallHeight;         // 6 m
        public const float WallBelowFloor = 1.0f;       // walls reach into the slab: no gap under them
        public const float SlabThickness = 1.0f;
        public const float SegmentOverlap = PlayerTuning.SegmentOverlap; // 1 m
        public const double SegmentLength = PlayerTuning.SegmentLength;  // 4 m
        public const float WallGap = 0.03f;
        public const float KillDepth = 5.0f;            // safety trigger top, below the floor
        public const float KillThickness = 20.0f;
        public const float KillLateralMargin = 10.0f;
        public const double GridStepS = PlayerTuning.RayGridStep;        // 0.5 m
        public const double GridStepLateral = PlayerTuning.RayGridStep;  // 0.5 m
        public const float GridRayHeight = 1.0f;
        public const float FloorRayLength = 2.5f;
        public const float WallRayMargin = 0.5f;
        const int ShiftSamples = 24;
        const double FloorSampleStep = 0.1;

        static int s_kills;
        Transform m_root, m_killRoot;
        bool m_built;
        int m_colliders, m_segments;
        double m_maxShift;
        bool m_subscribed;

        public bool Built => m_built;
        public int ColliderCount => m_colliders;
        public int SegmentCount => m_segments;
        /// <summary>Safety / kill events (must stay 0 in every test).</summary>
        public int KillCount => s_kills;
        public Transform CorridorRoot => m_root;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics() { s_kills = 0; Instance = null; s_obstacles.Clear(); }

        // ---------------------------------------------------------------- obstacles (props inside the lane)
        /// <summary>A named axis-aligned obstacle box (world space) and its collider (null until the corridor exists).</summary>
        public sealed class Obstacle
        {
            public string Name;
            public Bounds Box;
            public BoxCollider Collider;
        }

        static readonly List<Obstacle> s_obstacles = new List<Obstacle>(16);
        Transform m_obstacleRoot;

        /// <summary>The registered obstacles (in registration order).</summary>
        public static IReadOnlyList<Obstacle> Obstacles => s_obstacles;

        /// <summary>Registers (or replaces, by name) an obstacle box: a corridor-layer BoxCollider the CharacterController
        /// cannot pass. Call from IBeatBuilder.Build; works before and after the corridor is built (also in EditMode, where
        /// only the analytic list is kept). Use it only for props the spec places inside the walkable lane.</summary>
        public static Obstacle AddObstacle(Bounds worldBox, string name)
        {
            if (string.IsNullOrEmpty(name)) name = "obstacle_" + s_obstacles.Count;
            Obstacle o = null;
            for (int i = 0; i < s_obstacles.Count; i++) if (s_obstacles[i].Name == name) { o = s_obstacles[i]; break; }
            if (o == null) { o = new Obstacle { Name = name }; s_obstacles.Add(o); }
            o.Box = worldBox;
            if (Instance != null && Instance.m_built) Instance.MakeObstacleCollider(o);
            return o;
        }

        /// <summary>Removes an obstacle by name (its collider too). Returns false when there was none.</summary>
        public static bool RemoveObstacle(string name)
        {
            for (int i = 0; i < s_obstacles.Count; i++)
            {
                if (s_obstacles[i].Name != name) continue;
                var c = s_obstacles[i].Collider;
                if (c != null) { if (Application.isPlaying) Destroy(c.gameObject); else DestroyImmediate(c.gameObject); }
                s_obstacles.RemoveAt(i);
                return true;
            }
            return false;
        }

        /// <summary>Drops every obstacle (EditMode tests; a world rebuild).</summary>
        public static void ClearObstacles()
        {
            for (int i = s_obstacles.Count - 1; i >= 0; i--) RemoveObstacle(s_obstacles[i].Name);
            s_obstacles.Clear();
        }

        void MakeObstacleCollider(Obstacle o)
        {
            if (m_obstacleRoot == null)
            {
                var go = new GameObject("Obstacles");
                go.layer = PlayerTuning.CorridorLayer;
                m_obstacleRoot = go.transform;
                m_obstacleRoot.SetParent(m_root, false);
            }
            if (o.Collider == null)
            {
                var go = new GameObject("obstacle_" + o.Name);
                go.layer = PlayerTuning.CorridorLayer;
                go.transform.SetParent(m_obstacleRoot, false);
                o.Collider = go.AddComponent<BoxCollider>();
                m_colliders++;
            }
            o.Collider.transform.SetPositionAndRotation(o.Box.center, Quaternion.identity);
            o.Collider.size = o.Box.size;
            Physics.SyncTransforms();
        }

        /// <summary>Squared distance from point p to the box (0 inside).</summary>
        static float SqrDistance(Bounds b, Vector3 p)
        {
            Vector3 c = b.ClosestPoint(p);
            return (c - p).sqrMagnitude;
        }

        /// <summary>True when the walker capsule at (s, lateral) (PlayerController.Capsule: 0.30-1.90 m above the analytic
        /// floor, radius PLAYER_R + margin) touches an obstacle box; the first such obstacle is returned. Analytic.</summary>
        public static bool ObstacleAt(double s, double lateral, float margin, out Obstacle hit)
        {
            hit = null;
            if (s_obstacles.Count == 0) return false;
            BwPath.World(s, lateral, out double x, out double y, out double z);
            float r = (float)FrameworkConst.PlayerRadius + margin;
            var a = new Vector3((float)x, (float)(y + 0.30 + FrameworkConst.PlayerRadius), (float)z);
            var b = new Vector3((float)x, (float)(y + 1.90 - FrameworkConst.PlayerRadius), (float)z);
            for (int i = 0; i < s_obstacles.Count; i++)
            {
                var box = s_obstacles[i].Box;
                // capsule segment against the box: sample the vertical segment (boxes are axis-aligned, so the closest
                // point of a vertical segment is at the clamp of the box's y range)
                float yc = Mathf.Clamp(box.center.y, a.y, b.y);
                var p = new Vector3(a.x, yc, a.z);
                if (SqrDistance(box, p) <= r * r) { hit = s_obstacles[i]; return true; }
            }
            return false;
        }

        /// <summary>The cinematic capsule path (BwPath.CinematicLateral every <paramref name="step"/> m of s): the obstacles
        /// it touches (names, first s). An empty result is the QA rule "obstacles stay off the cinematic path".</summary>
        public static int ObstaclesOnCinematicPath(double step, List<string> into)
        {
            into?.Clear();
            int n = 0;
            if (s_obstacles.Count == 0) return 0;
            var seen = new HashSet<string>();
            for (double s = 0.0; s <= BwPath.Length + 1e-9; s += Math.Max(0.05, step))
            {
                if (!ObstacleAt(s, BwPath.CinematicLateral(s), 0.0f, out Obstacle o)) continue;
                if (seen.Add(o.Name)) { n++; into?.Add(o.Name + " at s " + s.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture)); }
            }
            return n;
        }

        /// <summary>Called by PlayerController when a safety volume snaps the player back.</summary>
        public static void ReportKill()
        {
            s_kills++;
            Debug.LogError($"[BW-FAIL] safety volume event #{s_kills} (the corridor must never let the player fall)");
            Megastructure.UI.UiDirector.Blink();
        }

        void Awake() { Instance = this; }
        void OnEnable() { if (!m_subscribed) { GameBootstrap.WorldReady += OnWorldReady; m_subscribed = true; } }
        void OnDisable() { if (m_subscribed) { GameBootstrap.WorldReady -= OnWorldReady; m_subscribed = false; } }
        void OnDestroy() { if (Instance == this) Instance = null; }

        // The corridor depends only on BwPath, so it is built at Start (before the first move); WorldReady is a second chance.
        void Start() { Build(); }
        void OnWorldReady() { Build(); }

        // ---------------------------------------------------------------- lane API (The Ring's names)
        public static double FloorY(double s) => BwPath.FloorY(s);

        /// <summary>World floor point of the lane centre at s (midpoint of the capsule-centre limits).</summary>
        public static Vector3 CentreAt(double s)
        {
            BwPath.Lane(s, out double lo, out double hi);
            BwPath.World(s, 0.5 * (lo + hi), out double x, out double y, out double z);
            return new Vector3((float)x, (float)y, (float)z);
        }

        /// <summary>Lateral distance (m) by which a world xz point lies outside the capsule-centre lane (0 inside).</summary>
        public static double DistanceOutsideLanes(double x, double z, double hintS, out double s, out double lateral)
        {
            s = Math.Max(0.0, Math.Min(BwPath.Length, BwPath.Project(x, z, hintS)));
            BwPath.Point(s, out double cx, out double cz);
            BwPath.Tangent(s, out double tx, out double tz);
            lateral = (x - cx) * tz - (z - cz) * tx;
            BwPath.Lane(s, out double lo, out double hi);
            return lateral < lo ? lo - lateral : (lateral > hi ? lateral - hi : 0.0);
        }

        /// <summary>Lane sample points (s, lateral) every step metres along s and across the lane, edges included.</summary>
        public static void LaneSamplePoints(double step, List<Vector2> into)
        {
            into.Clear();
            int steps = (int)Math.Round(BwPath.Length / step);
            for (int k = 0; k <= steps; k++)
            {
                double s = Math.Min(BwPath.Length, k * step);
                BwPath.Lane(s, out double lo, out double hi);
                int n = (int)Math.Floor((hi - lo) / step + 1e-9);
                for (int j = 0; j <= n; j++) into.Add(new Vector2((float)s, (float)(lo + j * step)));
                if (lo + n * step < hi - 1e-6) into.Add(new Vector2((float)s, (float)hi));
            }
        }

        static Vector3 W(double s, double lat, double up = 0.0)
        {
            BwPath.World(s, lat, out double x, out double y, out double z);
            return new Vector3((float)x, (float)(y + up), (float)z);
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
            var killGo = new GameObject("SafetyVolumes");
            killGo.layer = PlayerTuning.SafetyLayer;
            m_killRoot = killGo.transform;
            m_killRoot.SetParent(m_root, false);

            int n = Math.Max(1, (int)Math.Ceiling(BwPath.Length / SegmentLength - 1e-9));
            for (int i = 0; i < n; i++)
            {
                double s0 = BwPath.Length * i / n, s1 = BwPath.Length * (i + 1) / n;
                BuildSegment(s0, s1);
                m_segments++;
            }
            BuildEndCap(0.0, -1.0);
            BuildEndCap(BwPath.Length, +1.0);
            m_built = true;
            for (int i = 0; i < s_obstacles.Count; i++) MakeObstacleCollider(s_obstacles[i]);
            Physics.SyncTransforms();
            Debug.Log($"[BW] corridor: {m_segments} segments, {m_colliders} colliders on layers {PlayerTuning.CorridorLayer}/{PlayerTuning.SafetyLayer} ({s_obstacles.Count} obstacles), max wall shift {m_maxShift * 100.0:F1} cm, built in {sw.Elapsed.TotalMilliseconds:F1} ms");
        }

        static double LaneEdge(double s, int side)
        {
            BwPath.Lane(s, out double lo, out double hi);
            return side < 0 ? lo : hi;
        }

        void BuildSegment(double s0, double s1)
        {
            double need = FrameworkConst.PlayerRadius + WallGap;
            double ext = 0.5 * SegmentOverlap + 0.05;

            // ---- floor slab: along the chord of the analytic floor, lowered so its top stays 3 cm under the floor everywhere
            Vector3 a0 = W(s0, 0.0), a1 = W(s1, 0.0);
            double drop = 0.0;
            int fs = Math.Max(2, (int)Math.Ceiling((s1 - s0 + 2.0 * ext) / FloorSampleStep));
            for (int k = 0; k <= fs; k++)
            {
                double sk = PlayerMath.ClampS(s0 - ext + (s1 - s0 + 2.0 * ext) * k / fs);
                double u = (sk - s0) / (s1 - s0);
                double chordY = a0.y + (a1.y - a0.y) * u;
                double d = chordY - BwPath.FloorY(sk);
                if (d > drop) drop = d;
            }
            float lowered = (float)drop + PlayerTuning.FallbackFloorDrop;
            double latMin = Math.Min(LaneEdge(s0, -1), LaneEdge(s1, -1)) - need - WallThickness;
            double latMax = Math.Max(LaneEdge(s0, +1), LaneEdge(s1, +1)) + need + WallThickness;
            float width = (float)(latMax - latMin);
            float centreLat = (float)(0.5 * (latMin + latMax));
            Vector3 b0 = W(s0, 0.0) - Vector3.up * lowered, b1 = W(s1, 0.0) - Vector3.up * lowered;
            AddBox("slab", m_root, b0, b1, width, -SlabThickness * 0.5f, SlabThickness, centreLat, false, PlayerTuning.CorridorLayer);

            // ---- walls: level boxes whose face passes through the wall points at the lane edge + clearance, pushed outward
            //      by the largest clearance deficit over the box's extended range (curvature, funnel, door)
            for (int side = -1; side <= 1; side += 2)
            {
                Vector3 f0 = W(s0, LaneEdge(s0, side) + side * need), f1 = W(s1, LaneEdge(s1, side) + side * need);
                double dx = f1.x - f0.x, dz = f1.z - f0.z, len = Math.Sqrt(dx * dx + dz * dz);
                if (len < 1e-6) continue;
                double fx = dx / len, fz = dz / len;
                double rx = fz, rz = -fx;                       // plan right of the wall direction
                double nx = -side * rx, nz = -side * rz;        // face normal, pointing into the corridor
                double shift = 0.0;
                double yLo = Math.Min(f0.y, f1.y), yHi = Math.Max(f0.y, f1.y);
                for (int k = 0; k <= ShiftSamples; k++)
                {
                    double sk = PlayerMath.ClampS(s0 - ext + (s1 - s0 + 2.0 * ext) * k / ShiftSamples);
                    Vector3 e = W(sk, LaneEdge(sk, side));
                    double dist = (e.x - f0.x) * nx + (e.z - f0.z) * nz;
                    if (need - dist > shift) shift = need - dist;
                    yLo = Math.Min(yLo, e.y); yHi = Math.Max(yHi, e.y);
                }
                yLo -= WallBelowFloor; yHi += WallHeight;
                double off = side * (0.5 * WallThickness + shift);
                var centre = new Vector3((float)(0.5 * (f0.x + f1.x) + rx * off), (float)(0.5 * (yLo + yHi)), (float)(0.5 * (f0.z + f1.z) + rz * off));
                var go = new GameObject(side < 0 ? "wallL" : "wallR");
                go.layer = PlayerTuning.CorridorLayer;
                go.transform.SetParent(m_root, false);
                go.transform.SetPositionAndRotation(centre, Quaternion.LookRotation(new Vector3((float)fx, 0f, (float)fz), Vector3.up));
                var bc = go.AddComponent<BoxCollider>();
                bc.size = new Vector3(WallThickness, (float)(yHi - yLo), (float)len + SegmentOverlap);
                m_colliders++;
                if (shift > m_maxShift) m_maxShift = shift;
            }

            // ---- safety trigger: KillDepth below the floor, wider than the corridor (layer 31)
            Vector3 k0 = W(s0, 0.0) - Vector3.up * (KillDepth + lowered), k1 = W(s1, 0.0) - Vector3.up * (KillDepth + lowered);
            AddBox("safety", m_killRoot, k0, k1, width + 2f * KillLateralMargin, -KillThickness * 0.5f, KillThickness, centreLat, true, PlayerTuning.SafetyLayer);
        }

        void BuildEndCap(double s, double dir)
        {
            BwPath.Tangent(s, out double tx, out double tz);
            BwPath.Lane(s, out double lo, out double hi);
            Vector3 p = W(s, 0.5 * (lo + hi));
            double off = FrameworkConst.PlayerRadius + WallGap + WallThickness * 0.5;
            var centre = new Vector3((float)(p.x + tx * dir * off), p.y + WallHeight * 0.5f - WallBelowFloor, (float)(p.z + tz * dir * off));
            var go = new GameObject(dir < 0 ? "capStart" : "capEnd");
            go.layer = PlayerTuning.CorridorLayer;
            go.transform.SetParent(m_root, false);
            go.transform.SetPositionAndRotation(centre, Quaternion.LookRotation(new Vector3((float)tx, 0f, (float)tz), Vector3.up));
            var bc = go.AddComponent<BoxCollider>();
            bc.size = new Vector3((float)(hi - lo) + 2f * ((float)FrameworkConst.PlayerRadius + WallGap + WallThickness), WallHeight, WallThickness);
            m_colliders++;
        }

        /// <summary>A box along a0 -> a1 (3D, follows the slope), offset laterally (along local X) and up (local Y).</summary>
        void AddBox(string name, Transform parent, Vector3 a0, Vector3 a1, float width, float upOffset, float height, float lateralOffset, bool trigger, int layer)
        {
            Vector3 dir = a1 - a0;
            float len = dir.magnitude;
            if (len < 1e-4f) return;
            Vector3 fwd = dir / len;
            Quaternion rot = Quaternion.LookRotation(fwd, Vector3.up);
            Vector3 right = rot * Vector3.right, up = rot * Vector3.up;
            Vector3 centre = 0.5f * (a0 + a1) + right * lateralOffset + up * upOffset;
            var go = new GameObject(name);
            go.layer = layer;
            go.transform.SetParent(parent, false);
            go.transform.SetPositionAndRotation(centre, rot);
            var bc = go.AddComponent<BoxCollider>();
            bc.size = new Vector3(width, height, len + SegmentOverlap);
            bc.isTrigger = trigger;
            m_colliders++;
        }

        // ---------------------------------------------------------------- fallback (NaN or below the floor)
        void LateUpdate()
        {
            if (!m_built) return;
            var p = BwServices.Player;
            if (p == null) return;
            Vector3 eye = p.EyePosition;
            // the ending (ADDENDUM A1 00): past s 1500 the floor is the cabin's (riding down the face) or the platform's
            double floorY = p.S > BwPath.Length ? EndingPath.EyeY(p.S) - BulwarkConst.Eye : BwPath.FloorY(p.S);
            if (float.IsNaN(eye.x) || float.IsNaN(eye.y) || float.IsNaN(eye.z) || eye.y < floorY - KillDepth)
            {
                Debug.LogError($"[BW-FAIL] player state invalid at s={p.S:F2} eye={eye}: returned to the lane centre");
                s_kills++;
                p.Teleport(PlayerMath.ClampEndingS(double.IsNaN(p.S) ? CliArgs.StartS : p.S), 0.0);
            }
        }

        // ---------------------------------------------------------------- coverage (11.8 ray-cast grid)
        public bool CoverageCheck(out int misses) => CoverageCheck(out misses, out _);

        /// <summary>
        /// Every 0.5 m along s and every 0.5 m across the lane (edges included): a ray from 1 m above the floor straight down
        /// must hit a slab within 2.5 m; on both sides a horizontal ray from the lane centre at 1 m must hit a wall within the
        /// lane half-width + radius + 0.5 m; at both ends a ray along the path must hit the end cap.
        /// </summary>
        public bool CoverageCheck(out int misses, out int rays)
        {
            misses = 0; rays = 0;
            if (!m_built) Build();
            Physics.SyncTransforms();
            int mask = PlayerTuning.CorridorMask;
            int firstMissLogs = 0;
            int steps = (int)Math.Round(BwPath.Length / GridStepS);
            for (int k = 0; k <= steps; k++)
            {
                double s = Math.Min(BwPath.Length, k * GridStepS);
                BwPath.Lane(s, out double lo, out double hi);
                int nLat = (int)Math.Floor((hi - lo) / GridStepLateral + 1e-9);
                for (int j = 0; j <= nLat + 1; j++)
                {
                    double lat = j <= nLat ? lo + j * GridStepLateral : hi;
                    Vector3 o = W(s, lat, GridRayHeight);
                    rays++;
                    if (!Physics.Raycast(o, Vector3.down, FloorRayLength, mask, QueryTriggerInteraction.Ignore))
                    {
                        misses++;
                        if (firstMissLogs++ < 10) Debug.LogError($"[BW-FAIL] corridor floor ray miss at s={s:F1} lateral={lat:F2}");
                    }
                }
                BwPath.Tangent(s, out double tx, out double tz);
                double mid = 0.5 * (lo + hi), half = 0.5 * (hi - lo);
                Vector3 c = W(s, mid, GridRayHeight);
                for (int side = -1; side <= 1; side += 2)
                {
                    var d = new Vector3((float)(tz * side), 0f, (float)(-tx * side));
                    rays++;
                    if (!Physics.Raycast(c, d, (float)(half + FrameworkConst.PlayerRadius) + WallRayMargin, mask, QueryTriggerInteraction.Ignore))
                    {
                        misses++;
                        if (firstMissLogs++ < 10) Debug.LogError($"[BW-FAIL] corridor wall ray miss at s={s:F1} side={side}");
                    }
                }
            }
            for (int e = 0; e < 2; e++)
            {
                double s = e == 0 ? GridStepS * 0.5 : BwPath.Length - GridStepS * 0.5;
                BwPath.Tangent(s, out double tx, out double tz);
                BwPath.Lane(s, out double lo, out double hi);
                float sign = e == 0 ? -1f : 1f;
                Vector3 o = W(s, 0.5 * (lo + hi), GridRayHeight);
                rays++;
                if (!Physics.Raycast(o, new Vector3((float)tx * sign, 0f, (float)tz * sign), 2.0f, mask, QueryTriggerInteraction.Ignore))
                {
                    misses++;
                    Debug.LogError($"[BW-FAIL] corridor end cap ray miss at s={s:F1}");
                }
            }
            Debug.Log($"[BW] corridor coverage: {rays} rays, {misses} misses");
            return misses == 0;
        }

        /// <summary>True if the walker capsule at (s, lateral) intersects a solid corridor collider (WallBot probe).</summary>
        public static bool CapsuleBlockedAt(double s, double lateral) => PlayerController.CapsuleBlockedAt(s, lateral);
    }
}
