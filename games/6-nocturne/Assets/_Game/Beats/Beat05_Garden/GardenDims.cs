// GardenDims: every dimension of beat 5, the hanging garden (MASTER_PROMPT 14.5.5, 14.4, 14.12), as named constants in metres, world
// coordinates. The terrace x 695-914, z 115-275 (floor top y 0) hangs over the Gulf west of face B (x 815.3) and sits in face B's
// garden notch east of it (G2 builds the notch side walls on the planes z 115 and 275). Deviations marked "G3:" are logged in
// docs/logs/G3.md. Shared by the builders of this folder and the EditMode tests.
using UnityEngine;

namespace Megastructure
{
    /// <summary>One tree of 14.5.5: trunk position, height, crown radius, trunk base radius r0 (4-6 m by hash).</summary>
    public struct GardenTreeSpec
    {
        public string Name;
        public float X, Z, Height, CrownR, R0;
        public bool Hero;
        /// <summary>Crown centre z (the trunk leans when the crown must stay inside the notch walls; = Z otherwise).</summary>
        public float CrownZ;
        public Vector3 CrownCentre => new Vector3(X, GardenDims.CrownCentreFrac * Height, CrownZ);
        public Vector3 CrownRadii => new Vector3(CrownR, GardenDims.CrownVertFrac * Height, CrownR);
        public float CrownBottom => (GardenDims.CrownCentreFrac - GardenDims.CrownVertFrac) * Height;
        public float CrownTop => (GardenDims.CrownCentreFrac + GardenDims.CrownVertFrac) * Height;
    }

    public static class GardenDims
    {
        // ---- terrace (14.4, 14.5.5)
        public const float TerraceX0 = 695f, TerraceX1 = 914f, TerraceZ0 = 115f, TerraceZ1 = 275f;
        public const float FaceBX = 815.3f;                 // west of it the terrace hangs over the Gulf
        public const float AxisZ = 195f;
        public const float SlabDepthGulf = 6f;              // the hanging part: 6 m slab + a stepped keel below
        public const float SlabDepthNotch = 2f;             // inside face B's notch (G2's mass below must stay under y -2)
        public const float SlabEastInFace = 818f;           // the hanging part runs 2.7 m into face B (its end faces are buried)
        public const float BalustradeHeight = 1.1f, BalustradeThick = 0.25f;

        // ---- walk surfaces (14.4: s 880-1080 stone path and moss; 1080-1260 wet stone; lanes of NocLanes)
        public const float PathHalfWidth = 1.5f;            // flagstone path 3.0 m
        public const double PathS0 = 879.7;                 // x 695 (gate B east face; the deck ends there)
        public const double ArcS0 = 1056.44, ArcS1 = 1103.56;   // corner C2 (r 30, centre (871.74, 225))
        public const float PromX0 = 874f, PromX1 = 914f, PromZ0 = 225f;   // wet-stone promenade from the end of C2 to z 275 (G4 floors north of it)
        public const int ArcSegments = 48;

        // ---- furniture (14.5.5: 12 benches 0.45 m, planters; all outside the lane walls at 6.0 + 0.38)
        public const int BenchCount = 12;
        public const float BenchLateral = 7.6f, BenchLength = 1.8f, BenchHeight = 0.45f, BenchDepth = 0.5f;
        public const float BenchS0 = 896f, BenchPitch = 13.4f;
        public const float PlanterLateral = 6.9f, PlanterLength = 3.6f, PlanterHeight = 0.7f, PlanterDepth = 0.8f;
        public const float PlanterS0 = 889f, PlanterPitch = 13.4f;

        // ---- trees (14.5.5: trunks 24-gon lofts r0 4-6 with root flare tapering to 0.45 r0; crowns (r, 0.28 h, r) at 0.66 h)
        public const float CrownCentreFrac = 0.66f, CrownVertFrac = 0.28f;
        public const int TrunkSides = 24, TrunkRings = 40;
        public const float TrunkTopFrac = 0.72f, TaperEnd = 0.45f, FlareGain = 0.45f, FlareHeight = 4.0f;
        public const int Buttresses = 5;
        public const float ButtressGain = 0.25f, ButtressHeight = 7f;
        public const int Branches = 6;
        public const float BranchMinRadius = 0.15f;
        public const int CrownLon = 96, CrownLat = 48;           // 3.9 m chords at r 30 showed facets on the silhouette (KS15)
        public const float NotchClearance = 1f;            // crowns stay 1 m inside the notch walls (G3: lean, see CrownZ)
        public const float BackCrownFrac = 0.316f;          // back-tree crown radius / height (the mean ratio of the 9 hero trees)
        public const int BackTreeCount = 8;
        public const float BackDzMin = 55f, BackDzMax = 70f, BackHMin = 110f, BackHMax = 140f;
        public const float BackMaxLean = 6f;                // back crowns near the notch walls may lean this much toward the axis

        // ---- Look v2 crowns (TEXTURE_PLAN 4, FO; the only crowns since GD deleted v1 in Phase 4): an opaque core at 0.90 of the radii, leaf-cluster cards on two
        // shells (0.94, 1.02) placed with a blue-noise lattice at 1.5x area coverage (back trees 0.5x density), every card corner
        // inside 1.10x the ellipsoid (the envelope V's audit may use), the notch clearance kept
        // CoreFrac 0.86 (TEXTURE_PLAN 4: 0.90): lookdev r03 / r04 - at 0.90 the inner shell (0.94) lies 1.2-1.6 m above the core and its
        // 1.6-2.6 m cards, tilted up to 50 deg, sink half into it, so a lobe facing the camera showed the core through a single
        // layer of leaves (a smooth dark dome); 0.86 frees the inner shell
        public const float CoreFrac = 0.86f, ShellInner = 0.94f, ShellOuter = 1.02f, EnvelopeFrac = 1.10f;
        public const float CardMin = 1.6f, CardMax = 2.6f, CardMinShrunk = 1.2f;
        public const float CardCoverage = 1.5f, CardMeanArea = 4.4f, BackCardDensity = 0.5f;
        // perf A/B r03 (beat 5, cinematic autotest, 1080p): v2 - v1 GPU +0.87 ms mean, +0.90 p50, +1.7 p95 (the canopy views);
        // the inner shell (AO 0.72, behind the outer one, the textured core behind it) carries 0.7 of the outer density
        public const float InnerShellDensity = 0.7f;
        public const float CardTiltMaxDeg = 50f;            // card normal = radial normal tilted up to this angle (random azimuth)
        public const float CardJitter = 0.45f;              // lattice jitter as a fraction of the mean spacing
        public const float ClumpDepth = 0.45f, ClumpFreq = 2.6f;   // card density x (1 +- 0.45) by a low-frequency clump noise
        // Lobes (FO lookdev r01: a 0.90 core with +-6 % lumps read as ivy-covered domes in KS14 / KS15): the v2 crown surface is
        // the ellipsoid x L(d), L = LobeFloor + (1 - LobeFloor) x S(d), S = the soft union (p-norm, p = LobeP) of LobesHero
        // (LobesBack) dome-shaped lobes on a jittered Fibonacci set of directions (angular radius 0.55-0.75 rad, height 0.82-1.0),
        // times a fine +-3 % lump: sub-crowns 12-18 m wide with valleys 8-12 m deep, so the silhouette breaks into leaf masses and
        // the sky notches between them. Core and both card shells follow L; cards thin out (x ValleyDensity), and card AO / sky
        // exposure and the core AO darken (x ValleyAo, x ValleyExposure) toward the valley floors.
        public const float LobeFloor = 0.66f, LobeP = 4f, LobeAngleMin = 0.55f, LobeAngleMax = 0.75f, LobeAmpMin = 0.82f;
        public const int LobesHero = 15, LobesBack = 12;
        public const float LobeJitter = 0.30f, FineLump = 0.03f;
        public const float ValleyDensity = 0.55f, ValleyAo = 0.55f, ValleyExposure = 0.40f;
        public const float BarkTile = 1.8f;                 // trident_maple_bark (TEXTURES.md final sets): whole tiles around every limb
        public const float BarkRefY = 8f;                   // the trunk's tile count is set by its perimeter at y 8 m (above the flare)
        // 4 south then 4 north, midway between the hero trunks in x (trunk footprints with flare and buttresses reach 1.81 r0)
        static readonly float[] BackX = { 737f, 779f, 823f, 866f, 741f, 784f, 828f, 872f };

        // ---- light kit (14.2.3 row 5: 8 leaf points LeafGlow r 30 i 3) and wet sources under the crowns (14.7 H4)
        public const int LeafLights = 8;
        public const float LeafLightRange = 30f, LeafLightIntensity = 3f;
        public const float CrownWetRadiance = 0.12f, CrownWetRhoFrac = 0.5f;
        public const double WallS0 = 904.0;                 // notch walls fade in over s 892-904 (after G2's face walls, G2-D6)

        /// <summary>The nine hero trees of 14.5.5 (x, z, height, crown radius).</summary>
        public static readonly float[,] HeroTable =
        {
            { 720f, 238f, 95f, 30f }, { 762f, 250f, 120f, 38f }, { 806f, 232f, 88f, 28f }, { 850f, 228f, 130f, 40f },
            { 716f, 152f, 100f, 32f }, { 758f, 142f, 125f, 40f }, { 800f, 160f, 92f, 30f }, { 846f, 140f, 118f, 38f },
            { 886f, 158f, 104f, 32f },
        };

        static GardenTreeSpec[] s_trees;

        /// <summary>All 17 trees: the 9 hero trees, then the 8 back trees (|z - 195| 55-70, heights 110-140 by hash).</summary>
        public static GardenTreeSpec[] Trees
        {
            get
            {
                if (s_trees != null) return s_trees;
                var list = new GardenTreeSpec[HeroTable.GetLength(0) + BackTreeCount];
                for (int i = 0; i < HeroTable.GetLength(0); i++)
                    list[i] = Make("T" + (i + 1), HeroTable[i, 0], HeroTable[i, 1], HeroTable[i, 2], HeroTable[i, 3], true, i);
                for (int k = 0; k < BackTreeCount; k++)
                {
                    int side = k < 4 ? -1 : 1;                                     // south, then north
                    float dz = BackDzMin + (BackDzMax - BackDzMin) * BridgeGardenKit.Hash01(k, 501);
                    float h = BackHMin + (BackHMax - BackHMin) * BridgeGardenKit.Hash01(k, 502);
                    float x = BackX[k] + 4f * (BridgeGardenKit.Hash01(k, 503) - 0.5f);
                    float z = AxisZ + side * dz;
                    // G3: a back crown may not cross a notch wall: either it stays west of face B, or within the wall distance
                    // plus a small lean (spec gives no back crown radius; 0.316 h is the hero mean)
                    float wallDist = side < 0 ? z - TerraceZ0 : TerraceZ1 - z;
                    float r = Mathf.Min(BackCrownFrac * h, Mathf.Max(FaceBX - NotchClearance - x, wallDist - NotchClearance + BackMaxLean));
                    list[HeroTable.GetLength(0) + k] = Make("B" + (k + 1), x, z, h, r, false, 100 + k);
                }
                s_trees = list;
                return s_trees;
            }
        }

        static GardenTreeSpec Make(string name, float x, float z, float h, float r, bool hero, int seed)
        {
            var t = new GardenTreeSpec
            {
                Name = name, X = x, Z = z, Height = h, CrownR = r, Hero = hero,
                R0 = 4f + 2f * BridgeGardenKit.Hash01(seed, 511),
            };
            t.CrownZ = CrownZFor(x, z, r);
            return t;
        }

        /// <summary>
        /// G3: where a crown's x extent overlaps face B's notch (x 815.3..914) it must stay NotchClearance inside the notch walls
        /// (z 115 / 275, y 0..560, G2); the crown centre then moves toward the axis and the trunk leans (T8 (846, 140): 14 m).
        /// </summary>
        public static float CrownZFor(float x, float z, float r)
        {
            if (x + r <= FaceBX || x - r >= TerraceX1) return z;
            return Mathf.Clamp(z, TerraceZ0 + r + NotchClearance, TerraceZ1 - r - NotchClearance);
        }

        /// <summary>Progress s of a point (x, z) of the garden (projection on the centre line).</summary>
        public static double SOf(float x, float z) => NocMath.ProjectToPath(new D3(x, 0, z), 980.0);
    }
}
