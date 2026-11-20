// NocFacadeMath: the double-precision C# mirror of Shaders/Include/NocFacadeSkin.hlsl (Look v2; MASTER_PROMPT 14.7 H2, D-004,
// R1 section 3) plus NocHash (pcg3d / pcg4d twins of NocCommon.hlsl). TP-2 (FX, Phase 4 wave A) deleted the v1 window model
// (Windows, Edge, WindowE / WindowLit, strata, zones, Macro) with its shader path; FacadeSkinTests test the v2 twin.
// Colours are linear RGB (C3, NocFogMath.Pal). No UnityEngine dependency.
using System;

namespace Megastructure
{
    /// <summary>pcg hashes shared by shaders and C# (Jarzynski and Olano 2020). Unchecked uint arithmetic, identical to HLSL.</summary>
    public static class NocHash
    {
        public static void Pcg3d(ref uint x, ref uint y, ref uint z)
        {
            unchecked
            {
                x = x * 1664525u + 1013904223u; y = y * 1664525u + 1013904223u; z = z * 1664525u + 1013904223u;
                x += y * z; y += z * x; z += x * y;
                x ^= x >> 16; y ^= y >> 16; z ^= z >> 16;
                x += y * z; y += z * x; z += x * y;
            }
        }

        public static void Pcg4d(ref uint x, ref uint y, ref uint z, ref uint w)
        {
            unchecked
            {
                x = x * 1664525u + 1013904223u; y = y * 1664525u + 1013904223u; z = z * 1664525u + 1013904223u; w = w * 1664525u + 1013904223u;
                x += y * w; y += z * x; z += x * y; w += y * z;
                x ^= x >> 16; y ^= y >> 16; z ^= z >> 16; w ^= w >> 16;
                x += y * w; y += z * x; z += x * y; w += y * z;
            }
        }

        /// <summary>Component k (0..2) of pcg3d(a, b, c).</summary>
        public static uint H3(uint a, uint b, uint c, int k)
        {
            Pcg3d(ref a, ref b, ref c);
            return k == 0 ? a : (k == 1 ? b : c);
        }

        public static uint H4x(uint a, uint b, uint c, uint d) { Pcg4d(ref a, ref b, ref c, ref d); return a; }

        public static double U01(uint h) => (h >> 8) * (1.0 / 16777216.0);
        public static uint Pcg(uint v) => NocMath.Pcg(v);
    }

    public static class NocFacadeMath
    {
        /// <summary>Face id of a vertical face: pcg of the yaw lattice index (0.5 deg) and the plane-offset lattice index (0.25 m),
        /// 16 bits: the vertex attribute is interpolated, and perspective interpolation of a constant moves it by ~1e-7 relative, so
        /// a 24-bit id could round to its neighbour in single pixels (a 16-bit id stays within 0.007). Coplanar faces of both city
        /// tiers share it.</summary>
        /// <summary>A face's u extent as written into uv1.xy: the value itself, nudged FaExtentGuard m off a rounding half-step of the
        /// 1 / FaExtentQ grid, so the v2 shader's snap (QuantExtent) is stable under interpolation noise (TQ-09).</summary>
        public static float SafeExtent(double u)
        {
            double q = NocFacadeConstants.FaExtentQ, s = u * q, f = s - Math.Floor(s), g = NocFacadeConstants.FaExtentGuard * q;
            if (Math.Abs(f - 0.5) < g) s += f < 0.5 ? -(g - (0.5 - f)) : (g - (f - 0.5));
            return (float)(s / q);
        }

        /// <summary>A face's y0 / y1 on the 1 / FaExtentQ grid, rounded outward (y0 down, y1 up: the face stays inside [y0, y1]).</summary>
        public static float GridY0(double y) => (float)(Math.Floor(y * NocFacadeConstants.FaExtentQ) / NocFacadeConstants.FaExtentQ);
        public static float GridY1(double y) => (float)(Math.Ceiling(y * NocFacadeConstants.FaExtentQ) / NocFacadeConstants.FaExtentQ);

        /// <summary>The v2 shader's snap of an interpolated extent (round to the nearest 1 / FaExtentQ m).</summary>
        public static double QuantExtent(double x) => Math.Round(x * NocFacadeConstants.FaExtentQ, MidpointRounding.AwayFromZero) / NocFacadeConstants.FaExtentQ;

        public static uint FaceId(double yawDeg, double planeOffset)
        {
            double y = yawDeg - 360.0 * Math.Floor(yawDeg / 360.0);
            int k = (int)Math.Round(y / NocturneConstants.CityYawQuantum) % (int)Math.Round(360.0 / NocturneConstants.CityYawQuantum);
            int o = (int)Math.Round(planeOffset / 0.25);
            return NocHash.H3(unchecked((uint)k), unchecked((uint)o), 0x4E6F63u, 0) & 0xFFFFu;
        }

        // ================================================================== Look v2 (TEXTURE_PLAN 2, LOOK_V2 4, docs/FACADE_V2.md)
        // The C# twin of Shaders/Include/NocFacadeSkin.hlsl (the only facade path since TP-2) and the cell API that GE aligns its relief to. Every
        // constant is NocFacadeConstants (NC_Fa* in HLSL). Cell coordinates: x = u - i Pu, y = vl - j Pv with vl = v - band H.

        public const int SkinS0 = 0, SkinS1 = 1, SkinS2 = 2, SkinS3 = 3, SkinS4 = 4, SkinSection = 5;
        public const int RegionD = 0, RegionOQ = 1;
        public const int StratumRes = 0, StratumCom = 1, StratumMech = 2, StratumLobby = 3, StratumSection = 4;
        /// <summary>Hero overrides (TEXTURE_PLAN 2.2): 0 spire, 1 screen-wall mass, 2 alley west tower, 3 alley east tower, 4 boulevard podium.</summary>
        public const int OverrideSpire = 0, OverrideScreenWall = 1, OverrideAlleyWest = 2, OverrideAlleyEast = 3, OverridePodium = 4;

        /// <summary>Skin of a tower or box: decoded from the skin byte (colour G) once per pixel in the shader.</summary>
        public struct Skin
        {
            public int id, region, overrideK;   // overrideK -1: regional skin
            public uint key;                    // tower key K = pcg(byte): strata, zone rows, lit groups, multiplier, variants
            public bool s1Mullion, s2Ribbed, core, ledge, fins, ledStrips, stoneBase;
        }

        /// <summary>Region of a world point (tower or box centre): OQ inside the Old Quarter disc, else D.</summary>
        public static int Region(double x, double z)
        {
            double dx = x - NocFacadeConstants.FaOqX, dz = z - NocFacadeConstants.FaOqZ;
            return dx * dx + dz * dz < NocFacadeConstants.FaOqR * NocFacadeConstants.FaOqR ? RegionOQ : RegionD;
        }

        /// <summary>The seed a builder passes for hero override k (0..15): SkinByte maps it to byte 0xF0 + k.</summary>
        public static uint SkinOverride(int k) => 0xFFFFFFF0u + (uint)(k & 15);

        /// <summary>Skin byte (vertex colour G) of a tower or box with this seed in this region (TEXTURE_PLAN 2.2).</summary>
        public static byte SkinByte(uint seed, int region)
        {
            if (seed >= 0xFFFFFFF0u) return (byte)(NocFacadeConstants.FaOverrideBase + (int)(seed - 0xFFFFFFF0u));
            uint span = (uint)NocFacadeConstants.FaRegionSpan;
            return (byte)(region == RegionOQ ? NocFacadeConstants.FaOqBase + (int)(seed % span) : (int)(seed % span));
        }

        /// <summary>Skin byte of a world point: SkinByte(seed, Region(x, z)).</summary>
        public static byte SkinByteAt(uint seed, double x, double z) => SkinByte(seed, Region(x, z));

        static double U01(uint h) => NocHash.U01(h);
        static uint H3x(uint a, uint b, uint c) => NocHash.H3(a, b, c, 0);
        static uint H3y(uint a, uint b, uint c) => NocHash.H3(a, b, c, 1);
        static uint H3z(uint a, uint b, uint c) => NocHash.H3(a, b, c, 2);
        static uint Pcg(uint v) => NocMath.Pcg(v);

        /// <summary>Decodes a skin byte. Mode 1 (section) is the fixed section skin; the byte only keys it.</summary>
        public static Skin SkinOf(int skinByte, int mode = 0)
        {
            uint b = (uint)(skinByte & 0xFF);
            var s = new Skin { key = Pcg(b), overrideK = -1 };
            uint K = s.key;
            s.region = b >= (uint)NocFacadeConstants.FaOqBase && b < (uint)NocFacadeConstants.FaOverrideBase ? RegionOQ : RegionD;
            if (mode == 1) { s.id = SkinSection; return s; }
            if (b >= (uint)NocFacadeConstants.FaOverrideBase)
            {
                int k = (int)(b - (uint)NocFacadeConstants.FaOverrideBase);
                s.overrideK = k;
                switch (k)
                {
                    case OverrideSpire: s.id = SkinS0; s.fins = true; break;
                    case OverrideScreenWall: s.id = SkinS2; s.ledStrips = true; break;
                    case OverrideAlleyEast: s.id = SkinS4; break;
                    case OverridePodium: s.id = SkinS1; s.stoneBase = true; break;
                    default: s.id = SkinS1; break;      // 2 alley west tower (precast) and the reserved 5-15
                }
            }
            else
            {
                double us = U01(Pcg(K ^ 0x534B4E31u));
                bool q = s.region == RegionOQ;
                double t0 = q ? NocFacadeConstants.FaShareQ0 : NocFacadeConstants.FaShareD0, t1 = q ? NocFacadeConstants.FaShareQ1 : NocFacadeConstants.FaShareD1;
                double t2 = q ? NocFacadeConstants.FaShareQ2 : NocFacadeConstants.FaShareD2, t3 = q ? NocFacadeConstants.FaShareQ3 : NocFacadeConstants.FaShareD3;
                s.id = us < t0 ? SkinS0 : (us < t1 ? SkinS1 : (us < t2 ? SkinS2 : (us < t3 ? SkinS3 : SkinS4)));
            }
            s.s1Mullion = U01(Pcg(K ^ 0x4D554C4Cu)) < NocFacadeConstants.FaS1MullionP;
            s.s2Ribbed = U01(Pcg(K ^ 0x52494253u)) < NocFacadeConstants.FaS2RibbedP;
            s.core = U01(Pcg(K ^ 0x434F5245u)) < NocFacadeConstants.FaCoreP;
            s.ledge = U01(Pcg(K ^ 0x4C444745u)) < NocFacadeConstants.FaLedgeP;
            return s;
        }

        /// <summary>Stratum of band `band` of a tower (keyed per tower, so floor lines continue around corners); mode 1: section.</summary>
        public static int StratumV2(uint K, int band, int mode = 0)
        {
            if (mode == 1) return StratumSection;
            double r = U01(H3x(K, unchecked((uint)(band + 65536)), 0x53545232u));
            double a = NocturneConstants.FacadeShareRes, b = a + NocturneConstants.FacadeShareCom, c = b + NocturneConstants.FacadeShareMech;
            return r < a ? StratumRes : (r < b ? StratumCom : (r < c ? StratumMech : StratumLobby));
        }

        /// <summary>Lit factor of a stratum (LOOK_V2 4) as a multiple of FaLit.</summary>
        public static double FactorV2(int type) => type == StratumCom ? NocFacadeConstants.FaFactorCom : (type == StratumMech ? NocFacadeConstants.FaFactorMech
            : (type == StratumLobby ? NocFacadeConstants.FaFactorLobby : (type == StratumSection ? NocFacadeConstants.FaSecLit / NocFacadeConstants.FaLit : NocFacadeConstants.FaFactorRes)));

        /// <summary>Geometry of one cell of a (skin, stratum): pitch, opening (frame outer edge), frame, mullions, transom, slab band,
        /// spandrel glass band, recess depth. All cell-local metres. This is the cell API GE aligns relief to.</summary>
        public struct Cell
        {
            public double Pu, Pv, bandH;
            public double ox0, ox1, oy0, oy1;   // opening
            public double ff;                   // frame inside the opening (all four sides)
            public double mOff, mP, mW;         // x mullions: centres at mOff + k mP strictly inside (ox0, ox1); mW 0 = none
            public double tY, tW;               // one y transom (tW 0 = none)
            public double slab;                 // slab band [0, slab)
            public double sp0, sp1;             // spandrel glass band (S0; sp1 <= sp0 none), x = the opening's x range
            public double D;                    // recess depth (glass plane behind the facade plane)
            public int louvre;                  // 1: louvre blades over the glass (mechanical slit)
            public double gx0, gx1, gy0, gy1;   // the glass rectangle at the glass plane (opening inside its frame; loggia doors)
            public int loggia;                  // 1: S4 loggia band
        }

        public static Cell CellOf(in Skin s, int type)
        {
            var c = new Cell { bandH = NocturneConstants.FacadeBand, ff = NocFacadeConstants.FaFrame };
            int sk = s.id;
            switch (type)
            {
                case StratumCom:
                    c.Pu = NocturneConstants.FacadeComP; c.Pv = NocturneConstants.FacadeComH;
                    c.ox0 = NocFacadeConstants.FaComOx0; c.ox1 = NocFacadeConstants.FaComOx1; c.oy0 = NocFacadeConstants.FaComOy0; c.oy1 = NocFacadeConstants.FaComOy1;
                    c.ff = NocFacadeConstants.FaComFrame; c.mOff = NocFacadeConstants.FaComOx0; c.mP = NocFacadeConstants.FaComMullP; c.mW = NocFacadeConstants.FaComMull;
                    c.D = sk == SkinS1 || sk == SkinS3 ? NocFacadeConstants.FaComDDeep : NocFacadeConstants.FaComD;
                    c.slab = SlabOf(sk);
                    if (sk == SkinS0) { c.sp0 = NocFacadeConstants.FaS0Cap; c.sp1 = c.oy0 - 0.06; }
                    break;
                case StratumMech:
                    c.Pu = NocturneConstants.FacadeMechP; c.Pv = NocturneConstants.FacadeMechH;
                    c.ox0 = NocFacadeConstants.FaMechOx0; c.ox1 = NocFacadeConstants.FaMechOx1; c.oy0 = NocFacadeConstants.FaMechOy0; c.oy1 = NocFacadeConstants.FaMechOy1;
                    c.ff = NocFacadeConstants.FaMechFrame; c.D = NocFacadeConstants.FaMechD; c.louvre = 1; c.slab = SlabOf(sk);
                    break;
                case StratumLobby:
                    c.Pu = NocturneConstants.FacadeLobbyP; c.Pv = NocturneConstants.FacadeLobbyH;
                    c.ox0 = NocFacadeConstants.FaLobOx0; c.ox1 = NocFacadeConstants.FaLobOx1; c.oy0 = NocFacadeConstants.FaLobOy0; c.oy1 = NocFacadeConstants.FaLobOy1;
                    c.ff = NocFacadeConstants.FaLobFrame; c.mOff = NocFacadeConstants.FaLobOx0; c.mP = NocFacadeConstants.FaLobMullP; c.mW = NocFacadeConstants.FaLobMull;
                    c.tY = 0.5 * (NocFacadeConstants.FaLobOy0 + NocFacadeConstants.FaLobOy1); c.tW = NocFacadeConstants.FaLobMull;
                    c.D = NocFacadeConstants.FaLobD; c.slab = SlabOf(sk);
                    break;
                case StratumSection:
                    c.Pu = NocFacadeConstants.FaSecP; c.Pv = NocturneConstants.FacadeSecH; c.bandH = NocturneConstants.FacadeSecBand;
                    c.ox0 = NocFacadeConstants.FaSecOx0; c.ox1 = NocFacadeConstants.FaSecOx1; c.oy0 = NocFacadeConstants.FaSecOy0; c.oy1 = NocFacadeConstants.FaSecOy1;
                    c.ff = NocFacadeConstants.FaSecFrame; c.mOff = 0.5 * NocFacadeConstants.FaSecP; c.mP = 100.0; c.mW = NocFacadeConstants.FaSecMull;
                    c.D = NocFacadeConstants.FaSecD; c.slab = NocFacadeConstants.FaSecSlab;
                    break;
                default:   // residential 2.4 x 3.6
                    c.Pu = NocturneConstants.FacadeColumn; c.Pv = NocturneConstants.FacadeFloor; c.slab = SlabOf(sk);
                    switch (sk)
                    {
                        case SkinS0:
                            c.ox0 = NocFacadeConstants.FaS0Ox0; c.ox1 = NocFacadeConstants.FaS0Ox1; c.oy0 = NocFacadeConstants.FaS0Oy0; c.oy1 = NocFacadeConstants.FaS0Oy1;
                            c.ff = 0.0; c.mOff = 0.0; c.mP = 0.5 * NocturneConstants.FacadeColumn; c.mW = NocFacadeConstants.FaS0Mull; c.D = NocFacadeConstants.FaS0D;
                            c.sp0 = NocFacadeConstants.FaS0Cap; c.sp1 = NocFacadeConstants.FaS0Sp1;
                            break;
                        case SkinS2:
                            c.ox0 = NocFacadeConstants.FaS2Ox0; c.ox1 = NocFacadeConstants.FaS2Ox1; c.oy0 = NocFacadeConstants.FaS2Oy0; c.oy1 = NocFacadeConstants.FaS2Oy1;
                            c.ff = NocFacadeConstants.FaS2Frame; c.mOff = 0.0; c.mP = 0.5 * NocturneConstants.FacadeColumn; c.mW = NocFacadeConstants.FaS2Mull; c.D = NocFacadeConstants.FaS2D;
                            break;
                        case SkinS3:
                            c.ox0 = NocFacadeConstants.FaS3Ox0; c.ox1 = NocFacadeConstants.FaS3Ox1; c.oy0 = NocFacadeConstants.FaS3Oy0; c.oy1 = NocFacadeConstants.FaS3Oy1;
                            c.ff = NocFacadeConstants.FaS3Frame; c.D = NocFacadeConstants.FaS3D;
                            break;
                        case SkinS4:
                            c.ox0 = NocFacadeConstants.FaS4Ox0; c.ox1 = NocFacadeConstants.FaS4Ox1; c.oy0 = NocFacadeConstants.FaS4Oy0; c.oy1 = NocFacadeConstants.FaS4Oy1;
                            c.ff = NocFacadeConstants.FaS4Frame; c.D = NocFacadeConstants.FaS4D;
                            break;
                        default:   // S1 precast (variant B: centre mullion, widened opening, same clear glass)
                            bool m = s.s1Mullion;
                            c.ox0 = m ? NocFacadeConstants.FaS1BOx0 : NocFacadeConstants.FaS1Ox0; c.ox1 = m ? NocFacadeConstants.FaS1BOx1 : NocFacadeConstants.FaS1Ox1;
                            c.oy0 = NocFacadeConstants.FaS1Oy0; c.oy1 = NocFacadeConstants.FaS1Oy1; c.D = NocFacadeConstants.FaS1D;
                            if (m) { c.mOff = 0.5 * NocturneConstants.FacadeColumn; c.mP = 100.0; c.mW = NocFacadeConstants.FaFrame; }
                            break;
                    }
                    break;
            }
            c.gx0 = c.ox0 + c.ff; c.gx1 = c.ox1 - c.ff; c.gy0 = c.oy0 + c.ff; c.gy1 = c.oy1 - c.ff;
            return c;
        }

        /// <summary>An S4 tower's loggia bands (FaLoggiaP of its residential bands, keyed by K).</summary>
        public static bool LoggiaBand(uint K, int band) => U01(H3x(K, unchecked((uint)(band + 65536)), 0x4C4F4747u)) < NocFacadeConstants.FaLoggiaP;

        /// <summary>The cell of band `band` (NocCellBandV2): S4 loggia bands swap the window for a whole-bay loggia.</summary>
        public static Cell CellOf(in Skin s, int type, int band)
        {
            Cell c = CellOf(s, type);
            if (type == StratumRes && s.id == SkinS4 && LoggiaBand(s.key, band))
            {
                c.ox0 = NocFacadeConstants.FaLogX0; c.ox1 = NocFacadeConstants.FaLogX1; c.oy0 = c.slab; c.oy1 = c.Pv; c.ff = 0.0;
                c.gx0 = NocFacadeConstants.FaLogDoorX0; c.gx1 = NocFacadeConstants.FaLogDoorX1; c.gy0 = c.slab; c.gy1 = c.slab + NocFacadeConstants.FaLogDoorH;
                c.mOff = 0.5 * c.Pu; c.mP = 100.0; c.mW = 0.5 * NocFacadeConstants.FaFrame; c.D = NocFacadeConstants.FaLoggiaD; c.loggia = 1;
            }
            return c;
        }

        static double SlabOf(int sk) => sk == SkinS0 ? NocFacadeConstants.FaS0Cap : (sk == SkinS2 ? NocFacadeConstants.FaS2Slab
            : (sk == SkinS3 ? NocFacadeConstants.FaS3Slab : (sk == SkinS4 ? NocFacadeConstants.FaS4Slab : NocFacadeConstants.FaS1Slab)));

        /// <summary>Whole bays of a face (TEXTURE_PLAN 2.5): cells iFirst..iLast have their opening inside [uMin + 0.6, uMax - 0.6];
        /// the core strip (if any) is coreI0..coreI1 (empty when coreI1 &lt; coreI0). Coordinates are world-absolute u.</summary>
        public static void Bays(in Skin s, in Cell c, int type, double uMin, double uMax, out int iFirst, out int iLast, out int coreI0, out int coreI1)
        {
            double a = uMin + NocFacadeConstants.FaCornerPier, b = uMax - NocFacadeConstants.FaCornerPier;
            iFirst = (int)Math.Ceiling((a - c.ox0) / c.Pu);
            iLast = (int)Math.Floor((b - c.ox1) / c.Pu);
            if (c.loggia == 1) { iFirst++; iLast--; }       // no loggias in the corner bays
            coreI0 = 0; coreI1 = -1;
            if (s.core && (type == StratumRes || type == StratumCom) && uMax - uMin > NocFacadeConstants.FaCoreMinFace)
            {
                double w = (U01(Pcg(s.key ^ 0x43575744u)) < 0.5 ? 3.0 : 6.0);      // lift and stair core 3 or 6 m
                int n = Math.Max(1, (int)Math.Round(w / c.Pu));
                double uc = 0.5 * (uMin + uMax);
                coreI0 = (int)Math.Floor(uc / c.Pu - 0.5 * n + 0.5);
                coreI1 = coreI0 + n - 1;
            }
        }

        /// <summary>True when bay i of the face is a window column (whole bay, not the core strip).</summary>
        public static bool BayHasWindow(int i, int iFirst, int iLast, int coreI0, int coreI1) => i >= iFirst && i <= iLast && (i < coreI0 || i > coreI1);

        /// <summary>Rows of a band that carry windows: [jFirst, jLast] after the base band, the parapet and the mechanical stripe
        /// (residential bands of towers keyed so: the top two floors are blind with louvres). y0 = y1 = 0: no vertical limits.</summary>
        public static void Rows(in Skin s, in Cell c, int type, int band, double y0, double y1, out int jFirst, out int jLast)
        {
            int rows = (int)Math.Round(c.bandH / c.Pv);
            jFirst = 0; jLast = rows - 1;
            if (type == StratumRes && MechStripe(s.key, band)) jLast = rows - 3;
            if (KnownY(y0, y1))
            {
                double baseH = y0 <= NocFacadeConstants.FaBaseY0 ? NocFacadeConstants.FaBase : 0.0;
                double lo = y0 + baseH - band * c.bandH, hi = y1 - NocFacadeConstants.FaParapet - (c.loggia == 1 ? c.Pv : 0.0) - band * c.bandH;
                jFirst = Math.Max(jFirst, (int)Math.Ceiling((lo - c.oy0) / c.Pv));
                jLast = Math.Min(jLast, (int)Math.Floor((hi - c.oy1) / c.Pv));
            }
        }

        /// <summary>A residential band of tower K ends in a two-floor mechanical stripe (louvres, no windows): every 24 floors on
        /// FaMechStripeP of the bands (TEXTURE_PLAN 2.5 macro layer: 7.2 m, 2 px at 3 km).</summary>
        /// <summary>The face's y0 / y1 are known (Gulf writers write 0, 0; a 2-component UV1 reads (0, 1)): y1 - y0 &gt; 2 m.</summary>
        public static bool KnownY(double y0, double y1) => y1 - y0 > 2.0;

        public static bool MechStripe(uint K, int band) => U01(H3y(K, unchecked((uint)(band + 65536)), 0x4D454348u)) < NocFacadeConstants.FaMechStripeP;

        // ------------------------------------------------------------------ lit probability (LOOK_V2 4, TEXTURE_PLAN 2.4)
        /// <summary>Standard normal from two hashes (Box-Muller; u1 in (0, 1]).</summary>
        public static double Normal(uint h1, uint h2)
        {
            double u1 = 1.0 - U01(h1), u2 = U01(h2);
            return Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Cos(2.0 * Math.PI * u2);
        }

        /// <summary>Log-normal of mean 1 (before the clamp) with log-sigma sigma, clamped to [FaLnMin, FaLnMax].</summary>
        public static double LogNormal1(double n, double sigma) => Math.Min(NocFacadeConstants.FaLnMax, Math.Max(NocFacadeConstants.FaLnMin, Math.Exp(sigma * n - 0.5 * sigma * sigma)));

        /// <summary>Tower multiplier m_T (never fades: a tower is 9 px or wider at 3 km).</summary>
        public static double TowerMul(uint K) => LogNormal1(Normal(H3x(K, 0x544F5752u, 7u), H3y(K, 0x544F5752u, 7u)), NocFacadeConstants.FaTowerSigma);

        /// <summary>Lit floor group of row j of band `band` of tower K: groups of 4-12 rows laid out from the band's bottom.</summary>
        public static void GroupOf(uint K, int band, int j, out int g, out int start, out int len)
        {
            start = 0; g = 0;
            len = GroupLen(K, band, 0);
            while (j >= start + len && g < 7) { start += len; g++; len = GroupLen(K, band, g); }   // 8 groups of >= 4 rows cover every band (HLSL loop)
        }

        static int GroupLen(uint K, int band, int g) =>
            NocFacadeConstants.FaGroupMinRows + (int)Math.Floor((NocFacadeConstants.FaGroupMaxRows - NocFacadeConstants.FaGroupMinRows + 1) * U01(H3x(K, unchecked((uint)(band + 65536)), 0x47525000u + (uint)g)));

        public static double GroupMul(uint K, int band, int g)
        {
            uint a = K, b = unchecked((uint)(band + 65536)), c = 0x47525000u + (uint)g;
            NocHash.Pcg3d(ref a, ref b, ref c);
            return LogNormal1(Normal(b, c), NocFacadeConstants.FaGroupSigma);
        }

        /// <summary>Zone row level U (keyed per tower: lit and dark floors continue around corners) and the zone column level U
        /// (per face), global zone row zrg = band x 16 + jl / 2.</summary>
        public static double ZoneRowU(uint K, int zrg) => U01(H3x(K, unchecked((uint)(zrg + 1048576)), 0x5A524F57u));
        public static double ZoneShiftV2(uint face, int zrg) => Math.Floor(NocturneConstants.FacadeZoneCols * U01(H3x(face, unchecked((uint)(zrg + 1048576)), 0x5A4F4E45u)));
        public static double ZoneCellU(uint face, int zrg, int zc) => U01(H3y(unchecked((uint)(zc + 1048576)), unchecked((uint)(zrg + 1048576)), face));
        public static double ZoneZ(double uRow, double uCell) => NocFacadeConstants.FaZoneLo + NocFacadeConstants.FaZoneSpan * 0.5 * (uRow + uCell);

        /// <summary>Lit probability of window (i, jl) of band `band`: min(1, FaLit f m_T g z).</summary>
        public static double PCell(uint K, uint face, int band, int type, int i, int jl, double pMul = 1.0)
        {
            int zrg = band * 16 + jl / NocturneConstants.FacadeZoneRows;
            int zc = (int)Math.Floor((i + ZoneShiftV2(face, zrg)) / NocturneConstants.FacadeZoneCols);
            GroupOf(K, band, jl, out int g, out _, out _);
            double p = NocFacadeConstants.FaLit * FactorV2(type) * TowerMul(K) * GroupMul(K, band, g) * ZoneZ(ZoneRowU(K, zrg), ZoneCellU(face, zrg, zc)) * pMul;
            return Math.Min(1.0, p);
        }

        // ------------------------------------------------------------------ hue and brightness (LOOK_V2 4)
        static double Y709(C3 c) => 0.2126 * c.r + 0.7152 * c.g + 0.0722 * c.b;
        static C3 NormY(C3 c, double g) => c * (NocFacadeConstants.FaYMed * g / Math.Max(Y709(c), 1e-6));
        static C3 Desat(NocPal p) { C3 c = NocFogMath.Pal(p); return C3.Lerp(c, NocFogMath.Pal(NocPal.WinNeutral), NocFacadeConstants.FaColDesat); }

        /// <summary>Luminance-normalised class colours: 0 tungsten, 1 warm, 2 neutral, 3 cold, 4 blue (x0.6), 5-8 coloured (x0.8).</summary>
        public static C3 ClassColour(int k)
        {
            switch (k)
            {
                case 0: return NormY(NocFogMath.Pal(NocPal.WinTungsten), 1.0);
                case 1: return NormY(NocFogMath.Pal(NocPal.WinWarm), 1.0);
                case 2: return NormY(NocFogMath.Pal(NocPal.WinNeutral), 1.0);
                case 3: return NormY(NocFogMath.Pal(NocPal.WinCold), 1.0);
                case 4: return NormY(NocFogMath.Pal(NocPal.WinBlue), NocFacadeConstants.FaBlueGain);
                case 5: return NormY(Desat(NocPal.NeonCyan), NocFacadeConstants.FaColGain);
                case 6: return NormY(Desat(NocPal.AviationRed), NocFacadeConstants.FaColGain);
                case 7: return NormY(Desat(NocPal.LeafGlow), NocFacadeConstants.FaColGain);
                default: return NormY(Desat(NocPal.NeonMagenta), NocFacadeConstants.FaColGain);
            }
        }

        static double Step(double t, double U) => NocMath.Clamp01((t - U) / NocFacadeConstants.FaHueBand + 0.5);

        /// <summary>Colour of a lit window with hue hash uh at warm fraction wf and neutral fraction nf (Y = FaYMed for the white
        /// classes, before b and the interior factor). The moving boundaries are soft over FaHueBand; wf = 0 gives no warm weight.</summary>
        public static C3 HueV2(double uh, double wf, double nf)
        {
            double gw = NocMath.Clamp01(wf / NocFacadeConstants.FaHueBand);
            double s1 = Step(wf / 3.0, uh) * gw, s2 = Step(wf, uh) * gw, s3 = Step(wf + nf, uh);
            C3 c;
            if (uh < NocFacadeConstants.FaHueBlue0)
                c = ClassColour(0) * s1 + ClassColour(1) * (s2 - s1) + ClassColour(2) * Math.Max(0.0, s3 - s2) + ClassColour(3) * (1.0 - Math.Max(s3, s2));
            else if (uh < NocFacadeConstants.FaHueCol0) c = ClassColour(4);
            else
            {
                double x = uh - NocFacadeConstants.FaHueCol0;
                c = ClassColour(x < NocFacadeConstants.FaColCyan ? 5 : (x < NocFacadeConstants.FaColCyan + NocFacadeConstants.FaColRed ? 6
                    : (x < NocFacadeConstants.FaColCyan + NocFacadeConstants.FaColRed + NocFacadeConstants.FaColLeaf ? 7 : 8)));
            }
            return c;
        }

        /// <summary>Expected lit-window colour (before b): the class shares at (wf, nf).</summary>
        public static C3 MeanHueV2(double wf, double nf)
        {
            double gw = NocMath.Clamp01(wf / NocFacadeConstants.FaHueBand);
            double tung = wf / 3.0 * gw, warm = 2.0 * wf / 3.0 * gw, neut = nf + wf * (1.0 - gw);
            double cold = NocFacadeConstants.FaHueBlue0 - wf - nf, blue = NocFacadeConstants.FaHueCol0 - NocFacadeConstants.FaHueBlue0;
            return ClassColour(0) * tung + ClassColour(1) * warm + ClassColour(2) * neut + ClassColour(3) * cold + ClassColour(4) * blue
                 + ClassColour(5) * NocFacadeConstants.FaColCyan + ClassColour(6) * NocFacadeConstants.FaColRed + ClassColour(7) * NocFacadeConstants.FaColLeaf + ClassColour(8) * NocFacadeConstants.FaColMagenta;
        }

        /// <summary>Brightness b of a window: exp(0.45 N) clamped to [0.40, 2.40] (log-normal, mean FaBMean).</summary>
        public static double BrightnessV2(uint hx) => Math.Min(NocFacadeConstants.FaBMax, Math.Max(NocFacadeConstants.FaBMin,
            Math.Exp(NocFacadeConstants.FaBSigma * Normal(hx, Pcg(hx ^ 0x42524954u)))));

        /// <summary>Mechanical slits: SodiumAmber on half of them x k_a (= wf / 0.32, LOOK_V2 R6), the rest SignWhite, at 0.6 Y_med.</summary>
        public static C3 MechHue(double uh, double wf)
        {
            double ka = NocMath.Clamp01(wf / 0.32);
            return uh < 0.5 * ka ? NormY(NocFogMath.Pal(NocPal.SodiumAmber), 0.6) : NormY(NocFogMath.Pal(NocPal.SignWhite), 0.6);
        }

        public static C3 MeanMechHue(double wf)
        {
            double ka = NocMath.Clamp01(wf / 0.32);
            return NormY(NocFogMath.Pal(NocPal.SodiumAmber), 0.6) * (0.5 * ka) + NormY(NocFogMath.Pal(NocPal.SignWhite), 0.6) * (1.0 - 0.5 * ka);
        }

        // ------------------------------------------------------------------ one window (cell) state
        public struct Win
        {
            public double lit;      // 0..1 with the toggle ramp
            public C3 eLit;         // lit glass radiance (hue x b), interior factor excluded
            public C3 eUn;          // unlit glass interior (stand-by part; the GlassDark x ambient part is added by the caller)
            public int kind;        // 0 bare, 1 curtain, 2 blinds
            public double cf, side, depth, tiltA, tiltB;
        }

        public static Win WindowV2(int i, int jl, int band, uint face, uint K, int type, double tEpoch, double wf, double nf, double pMul = 1.0)
        {
            var w = new Win();
            int jg = band * 32 + jl;
            uint a = unchecked((uint)(i + 1048576)), b = unchecked((uint)(jg + 1048576)), c = face;
            NocHash.Pcg3d(ref a, ref b, ref c);
            double p = PCell(K, face, band, type, i, jl, pMul);
            double x = tEpoch + U01(c);
            double e = Math.Floor(x);
            uint ki = unchecked((uint)(i + 1048576)), kj = unchecked((uint)(jg + 1048576)), ke = (uint)e;
            double aNow = U01(NocHash.H4x(ki, kj, face, ke)) <= p ? 1.0 : 0.0;
            double aPrev = U01(NocHash.H4x(ki, kj, face, unchecked(ke - 1u))) <= p ? 1.0 : 0.0;
            w.lit = aPrev + (aNow - aPrev) * NocMath.Smoothstep(0.0, NocFacadeConstants.FaToggle / NocFacadeConstants.FaEpoch, x - e);
            double uh = U01(b);
            C3 hue = type == StratumMech ? MechHue(uh, wf) : HueV2(uh, wf, nf);
            w.eLit = hue * BrightnessV2(a);
            w.eUn = NocFogMath.Pal(NocPal.WinNeutral) * (NocFacadeConstants.FaStandby * (0.5 + U01(Pcg(a ^ c))));
            double uk = U01(Pcg(b ^ 0x43555254u));
            w.kind = uk < NocFacadeConstants.FaCurtainP ? 1 : (uk < NocFacadeConstants.FaCurtainP + NocFacadeConstants.FaBlindP ? 2 : 0);
            uint hc = Pcg(c ^ 0x43464F4Cu);
            w.cf = 0.4 + 0.6 * U01(hc);                     // curtain closure 0.4..1.0 of the width
            w.side = U01(Pcg(hc)) ;                         // < 1/3 left panel, < 2/3 right panel, else both sides
            w.depth = U01(Pcg(b ^ c));
            w.tiltA = 2.0 * U01(Pcg(a ^ 0x54494C54u)) - 1.0; w.tiltB = 2.0 * U01(Pcg(b ^ 0x54494C54u)) - 1.0;
            return w;
        }

        /// <summary>E[I] of the interior factor: falloff mean (1 + FaFalloffSill) / 2 x the expected curtain / blind factor.</summary>
        public static double InteriorMean()
        {
            double fall = 0.5 * (1.0 + NocFacadeConstants.FaFalloffSill);
            double cfMean = 0.7, curtain = cfMean * NocFacadeConstants.FaCurtainGain + (1.0 - cfMean);
            double kinds = NocFacadeConstants.FaCurtainP * curtain + NocFacadeConstants.FaBlindP * NocFacadeConstants.FaBlindOpen + (1.0 - NocFacadeConstants.FaCurtainP - NocFacadeConstants.FaBlindP);
            return fall * kinds;
        }

        // ------------------------------------------------------------------ per-pixel evaluation (the HLSL twin)
        public struct FaceIn
        {
            public double u, v, fwu, fwv;      // face coordinates; bounding-box footprints (m per pixel) per axis
            public uint face; public int skinByte, mode;
            public double uMin, uMax, y0, y1;  // y0 = y1 = 0: unknown (Gulf writers)
            public double kx, ky;              // recess shift per metre of depth: dt / dn and (db / dn) B.y
            public double tEpoch, wf, nf;
            public double forceW;              // < 0: footprint fades; 0 or 1 forces the window axes (H2 tests)
            public C3 ambEq;                   // equator ambient (unlit glass GlassDark x ambient)
            public bool expectState;           // tests only (no HLSL twin): each resolved cell takes its expected state (lit = p,
                                               // hue x b and stand-by at their means), so regime tests carry no sampling noise
        }

        public struct FaceOut
        {
            public C3 emission;                                  // glass interiors (lit and stand-by), room spill on reveals / frames, halo
            public double aGlass, aSpandrel, aFrame, aJamb, aSillHead, aOpen, aSlab;
            public double louver;                                // section mechanical band (louvre wall) coverage, mode 1
            public double wu, wv, pEff;
            public int skin, type, band;
            public double Wall => Math.Max(0.0, 1.0 - aOpen - aSpandrel);
        }

        struct Axis { public double O0, O1, V0, V1, G0, G1, H0, H1; }   // per-cell coverages of the two cells (H: halo weight, y only)

        static double Cov(double x, double fw, double a, double b) => NocMath.Clamp01((Math.Min(x + 0.5 * fw, b) - Math.Max(x - 0.5 * fw, a)) / Math.Max(fw, 1e-9));

        /// <summary>Twin of NocCommon's NocFilteredGrid1: a line of width w every `period`, widened to >= 1 px with its energy kept,
        /// blended to its mean w / period once the period is under minPx pixels.</summary>
        public static double FilteredGrid1(double x, double period, double w, double fp, double minPx)
        {
            double fw = Math.Max(fp, 1e-5), wd = Math.Max(w, fw);
            double fr = x / period + 0.5; fr -= Math.Floor(fr);
            double d = Math.Abs(fr - 0.5) * period;
            double ln = (1.0 - NocMath.Smoothstep(0.5 * wd - 0.5 * fw, 0.5 * wd + 0.5 * fw, d)) * (w / wd);
            return ln + (NocMath.Clamp01(w / period) - ln) * NocMath.Clamp01(fw * minPx / period);
        }
        static double Len(double a, double b) => Math.Max(0.0, b - a);
        static C3 Mul(C3 a, C3 b) => new C3(a.r * b.r, a.g * b.g, a.b * b.b);

        /// <summary>Clear-glass coverage along x of the cell starting at s (world u): the visible glass behind the recess (shifted by
        /// -sx) minus the frame and the mullions (at most 4 inside the opening, as the HLSL loop). frac: the cell-mean fraction.</summary>
        static double GlassX(double x, double fw, double s, in Cell c, double sx, bool frac, double P)
        {
            double v0 = Math.Max(c.ox0, c.ox0 - sx), v1 = Math.Min(c.ox1, c.ox1 - sx);
            double g0 = Math.Max(v0, c.gx0 - sx), g1 = Math.Min(v1, c.gx1 - sx);
            double r = frac ? Len(g0, g1) / P : Cov(x, fw, s + g0, s + g1);
            if (c.mW > 0.0)
            {
                double k0 = Math.Floor((c.ox0 - c.mOff) / c.mP) + 1.0;
                for (int k = 0; k < 4; k++)
                {
                    double mc = c.mOff + (k0 + k) * c.mP;
                    if (mc >= c.ox1) break;
                    double a = Math.Max(g0, mc - 0.5 * c.mW - sx), b = Math.Min(g1, mc + 0.5 * c.mW - sx);
                    r -= frac ? Len(a, b) / P : Cov(x, fw, s + a, s + b);
                }
            }
            return Math.Max(0.0, r);
        }

        static double GlassY(double y, double fw, double s, in Cell c, double sy, bool frac, double P)
        {
            double v0 = Math.Max(c.oy0, c.oy0 - sy), v1 = Math.Min(c.oy1, c.oy1 - sy);
            double g0 = Math.Max(v0, c.gy0 - sy), g1 = Math.Min(v1, c.gy1 - sy);
            double r = frac ? Len(g0, g1) / P : Cov(y, fw, s + g0, s + g1);
            if (c.tW > 0.0)
            {
                double a = Math.Max(g0, c.tY - 0.5 * c.tW - sy), b = Math.Min(g1, c.tY + 0.5 * c.tW - sy);
                r -= frac ? Len(a, b) / P : Cov(y, fw, s + a, s + b);
            }
            return Math.Max(0.0, r);
        }

        /// <summary>Box integral of the rain-scatter halo under an opening (TEXTURE_PLAN 2.6): exp(-d / FaHaloL) for d = oy0 - y in
        /// [0, FaHaloMax], over the footprint (y, fw) of the cell starting at s; frac: the cell-mean fraction.</summary>
        static double HaloY(double y, double fw, double s, in Cell c, bool frac, double P)
        {
            double L = NocFacadeConstants.FaHaloL, top = s + c.oy0, lo = top - NocFacadeConstants.FaHaloMax;
            if (frac) return L * (1.0 - Math.Exp(-NocFacadeConstants.FaHaloMax / L)) / P;
            double a = Math.Max(lo, y - 0.5 * fw), b = Math.Min(top, y + 0.5 * fw);
            if (b <= a) return 0.0;
            return L * (Math.Exp(-(top - b) / L) - Math.Exp(-(top - a) / L)) / Math.Max(fw, 1e-9);
        }

        /// <summary>The facade of one pixel (forward-pass twin, lighting excluded): emission and the element coverages, with the
        /// exact 2 x 2 box filter where the cells resolve and the per-axis blend to the band means below the H2 fade.</summary>
        public static FaceOut EvalV2(in FaceIn q)
        {
            Skin s = SkinOf(q.skinByte, q.mode);
            // the face extents snapped as the shader does (stable whole bays, core strip and rows)
            FaceIn qq = q;
            if (Math.Abs(q.uMin) < 1e4) { qq.uMin = QuantExtent(q.uMin); qq.uMax = QuantExtent(q.uMax); }
            qq.y0 = QuantExtent(q.y0); qq.y1 = QuantExtent(q.y1);
            return EvalV2Snapped(qq, s);
        }

        static FaceOut EvalV2Snapped(in FaceIn q, Skin s)
        {
            var o = new FaceOut();
            uint K = s.key;
            double fwu = Math.Max(q.fwu, 1e-4), fwv = Math.Max(q.fwv, 1e-4);
            double bandH = q.mode == 1 ? NocturneConstants.FacadeSecBand : NocturneConstants.FacadeBand;
            int band = (int)Math.Floor(q.v / bandH);
            double vl = q.v - band * bandH;
            int type = StratumV2(K, band, q.mode);
            Cell c = CellOf(s, type, band);
            o.skin = s.id; o.type = type; o.band = band;
            double wu = NocMath.Smoothstep(NocFacadeConstants.FaFadePx0, NocFacadeConstants.FaFadePx1, c.Pu / fwu);
            double wv = NocMath.Smoothstep(NocFacadeConstants.FaFadePx0, NocFacadeConstants.FaFadePx1, c.Pv / fwv);
            if (q.forceW >= 0) { wu = q.forceW; wv = q.forceW; }
            double sx = c.D * q.kx, sy = c.D * q.ky;
            if (q.forceW < 0)
            {
                // visible-glass sliver fade (NocFacadeEvalV2): a lit sliver under 1-2 px falls to the view-dependent mean
                double vgx = Math.Max(0.0, Math.Min(Math.Min(c.ox1, c.ox1 - sx), c.gx1 - sx) - Math.Max(Math.Max(c.ox0, c.ox0 - sx), c.gx0 - sx));
                double vgy = Math.Max(0.0, Math.Min(Math.Min(c.oy1, c.oy1 - sy), c.gy1 - sy) - Math.Max(Math.Max(c.oy0, c.oy0 - sy), c.gy0 - sy));
                wu *= NocMath.Smoothstep(NocFacadeConstants.FaSliverPx0, NocFacadeConstants.FaSliverPx1, vgx / fwu);
                wv *= NocMath.Smoothstep(NocFacadeConstants.FaSliverPx0, NocFacadeConstants.FaSliverPx1, vgy / fwv);
            }
            o.wu = wu; o.wv = wv;
            int rows = (int)Math.Round(c.bandH / c.Pv);

            // whole bays and rows; the allowed coverage of the footprint (the mean regime counts the suppressed cells)
            Bays(s, c, type, q.uMin, q.uMax, out int iF, out int iL, out int k0, out int k1);
            Rows(s, c, type, band, q.y0, q.y1, out int jF, out int jL);
            double Ax = Cov(q.u, fwu, iF * c.Pu, (iL + 1) * c.Pu);
            if (k1 >= k0) Ax = Math.Max(0.0, Ax - Cov(q.u, fwu, k0 * c.Pu, (k1 + 1) * c.Pu));
            double Ay = Cov(vl, fwv, jF * c.Pv, (jL + 1) * c.Pv), Aband = Cov(vl, fwv, 0.0, rows * c.Pv);

            // per-axis mean fractions and the two cells under the footprint
            double mOx = Len(c.ox0, c.ox1) / c.Pu, mOy = Len(c.oy0, c.oy1) / c.Pv;
            double mVx = Len(Math.Max(c.ox0, c.ox0 - sx), Math.Min(c.ox1, c.ox1 - sx)) / c.Pu, mVy = Len(Math.Max(c.oy0, c.oy0 - sy), Math.Min(c.oy1, c.oy1 - sy)) / c.Pv;
            double mGx = GlassX(0, 1, 0, c, sx, true, c.Pu), mGy = GlassY(0, 1, 0, c, sy, true, c.Pv), mHy = HaloY(0, 1, 0, c, true, c.Pv);
            double mSx = GlassX(0, 1, 0, c, 0.0, true, c.Pu), mSlab = c.slab / c.Pv, mSpy = Len(c.sp0, c.sp1) / c.Pv;
            int cx0 = (int)Math.Floor((q.u - 0.5 * fwu) / c.Pu), cy0 = (int)Math.Floor((vl - 0.5 * fwv) / c.Pv);
            var X = new Axis(); var Y = new Axis();
            double spx = 0, spy = 0, slabY = 0;
            for (int a = 0; a < 2; a++)
            {
                int i = cx0 + a; double su = i * c.Pu;
                bool fx = BayHasWindow(i, iF, iL, k0, k1);
                double O = fx ? Cov(q.u, fwu, su + c.ox0, su + c.ox1) : 0, V = fx ? Cov(q.u, fwu, su + Math.Max(c.ox0, c.ox0 - sx), su + Math.Min(c.ox1, c.ox1 - sx)) : 0;
                double G = fx ? GlassX(q.u, fwu, su, c, sx, false, c.Pu) : 0;
                if (fx && c.sp1 > c.sp0) spx += GlassX(q.u, fwu, su, c, 0.0, false, c.Pu);
                if (a == 0) { X.O0 = O; X.V0 = V; X.G0 = G; } else { X.O1 = O; X.V1 = V; X.G1 = G; }
                int j = cy0 + a; double sv = j * c.Pv;
                bool fy = j >= jF && j <= jL;
                O = fy ? Cov(vl, fwv, sv + c.oy0, sv + c.oy1) : 0; V = fy ? Cov(vl, fwv, sv + Math.Max(c.oy0, c.oy0 - sy), sv + Math.Min(c.oy1, c.oy1 - sy)) : 0;
                G = fy ? GlassY(vl, fwv, sv, c, sy, false, c.Pv) : 0;
                double Hh = fy ? HaloY(vl, fwv, sv, c, false, c.Pv) : 0;
                if (a == 0) { Y.O0 = O; Y.V0 = V; Y.G0 = G; Y.H0 = Hh; } else { Y.O1 = O; Y.V1 = V; Y.G1 = G; Y.H1 = Hh; }
                if (j >= 0 && j < rows) slabY += Cov(vl, fwv, sv, sv + c.slab);
                if (fy && c.sp1 > c.sp0) spy += Cov(vl, fwv, sv + c.sp0, sv + c.sp1);
            }
            // blended per-axis totals (resolved 2 x 2 coverage -> mean fraction x allowed coverage)
            double OxE = wu * (X.O0 + X.O1) + (1 - wu) * mOx * Ax, VxE = wu * (X.V0 + X.V1) + (1 - wu) * mVx * Ax, GxE = wu * (X.G0 + X.G1) + (1 - wu) * mGx * Ax;
            double OyE = wv * (Y.O0 + Y.O1) + (1 - wv) * mOy * Ay, VyE = wv * (Y.V0 + Y.V1) + (1 - wv) * mVy * Ay, GyE = wv * (Y.G0 + Y.G1) + (1 - wv) * mGy * Ay;
            double HyE = wv * (Y.H0 + Y.H1) + (1 - wv) * mHy * Ay;
            o.aOpen = OxE * OyE; o.aGlass = GxE * GyE; o.aFrame = Math.Max(0.0, VxE * VyE - o.aGlass);
            o.aJamb = Math.Max(0.0, (OxE - VxE) * OyE); o.aSillHead = Math.Max(0.0, VxE * (OyE - VyE));
            o.aSlab = wv * slabY + (1 - wv) * mSlab * Aband;
            if (c.sp1 > c.sp0) o.aSpandrel = (wu * spx + (1 - wu) * mSx * Ax) * (wv * spy + (1 - wv) * mSpy * Ay);

            // lit probability at the pixel (levels blended by their own fades) and the band means
            double rq = s.region == RegionOQ ? NocFacadeConstants.FaOqLit : 1.0;   // the Old Quarter lights fewer windows (lead decision 6)
            double aT = NocFacadeConstants.FaLit * FactorV2(type) * TowerMul(K) * rq;
            double wg = NocMath.Smoothstep(NocFacadeConstants.FaGroupFadePx0, NocFacadeConstants.FaGroupFadePx1, NocFacadeConstants.FaGroupMinRows * c.Pv / fwv);
            double gEff = wg > 0 ? 1.0 + (GroupFiltered(K, band, vl, fwv, c) - 1.0) * wg : 1.0;
            double wz = NocMath.Smoothstep(NocFacadeConstants.FaFadePx0, NocFacadeConstants.FaFadePx1, Math.Min(NocturneConstants.FacadeZoneCols * c.Pu / fwu, NocturneConstants.FacadeZoneRows * c.Pv / fwv));
            double zEff = wz > 0 ? 1.0 + (ZoneFiltered(K, q.face, band, q.u, vl, fwu, fwv, c) - 1.0) * wz : 1.0;
            double pEff = aT * gEff * zEff * (q.mode == 1 ? SecDistFiltered(q.face, q.u, q.v, fwu, fwv) : 1.0);
            o.pEff = pEff;
            C3 hueMean = type == StratumMech ? MeanMechHue(q.wf) : MeanHueV2(q.wf, q.nf);
            C3 eLitMean = hueMean * NocFacadeConstants.FaBMean;
            C3 eUnMean = NocFogMath.Pal(NocPal.WinNeutral) * NocFacadeConstants.FaStandby;
            C3 eGlassMean = eLitMean * pEff + eUnMean * (1 - pEff);
            C3 albRev = WallToken(s, type) * NocFacadeConstants.FaAoReveal, albFrame = NocFogMath.Pal(NocPal.MetalPaint), albWall = WallToken(s, type);
            // resolved cells: per-cell glass and spill coverages x the cell state
            C3 R = new C3(0, 0, 0), kS = new C3(0, 0, 0); double kG = 0;
            double wuv = wu * wv;
            // the zero-mean interior factor I / E[I] - 1 (ceiling falloff, curtains, blinds) fades in over 4-8 px of window height
            double wI = NocMath.Smoothstep(NocFacadeConstants.FaIntFadePx0, NocFacadeConstants.FaIntFadePx1, (c.oy1 - c.oy0) / fwv);
            double wBox = type == StratumMech ? 0.0 : NocMath.Smoothstep(NocFacadeConstants.FaBoxPx0, NocFacadeConstants.FaBoxPx1, (c.oy1 - c.oy0) / fwv);
            double invEI = 1.0 / InteriorMean();
            double gwI = NocMath.Clamp01(q.wf / NocFacadeConstants.FaHueBand);
            if (wuv > 0)
            {
                for (int a = 0; a < 2; a++)
                    for (int b = 0; b < 2; b++)
                    {
                        double gx = a == 0 ? X.G0 : X.G1, gy = b == 0 ? Y.G0 : Y.G1;
                        double ox = a == 0 ? X.O0 : X.O1, oy = b == 0 ? Y.O0 : Y.O1, vx = a == 0 ? X.V0 : X.V1, vy = b == 0 ? Y.V0 : Y.V1, hy = b == 0 ? Y.H0 : Y.H1;
                        double cg = gx * gy;
                        C3 cs = SpillCov(ox, oy, vx, vy, gx, gy, hy, q.ky, albRev, albFrame, albWall);
                        if (cg + cs.r + cs.g + cs.b <= 0) continue;
                        int i = cx0 + a, j = cy0 + b;
                        // mode 1: the district of the cell (cells never straddle a district: 28 bays x 75 floors)
                        double mc = q.mode == 1 ? SecDistMul(q.face, Math.Floor((i + 0.5) * c.Pu / NocFacadeConstants.FaSecDistU),
                            Math.Floor((band * bandH + (j + 0.5) * c.Pv) / NocFacadeConstants.FaSecDistV)) : 1.0;
                        mc *= rq;
                        Win w = WindowV2(i, j, band, q.face, K, type, q.tEpoch, q.wf, q.nf, mc);
                        if (q.expectState) { w.lit = PCell(K, q.face, band, type, i, j, mc); w.eLit = eLitMean; w.eUn = eUnMean; }
                        // interior factor at the pixel's glass-plane point (NocFacadeEvalV2)
                        double xg = q.u - i * c.Pu + sx, yg = vl - j * c.Pv + sy;
                        double I = NocFacadeConstants.FaFalloffSill + (1.0 - NocFacadeConstants.FaFalloffSill) * NocMath.Clamp01((yg - c.oy0) / Math.Max(c.oy1 - c.oy0, 1e-3));
                        if (wBox > 0 && w.lit > 0)
                        {
                            // the box room (NocInterior.hlsl): raw / its exact mean x E[falloff], so the glass mean is unchanged
                            double Dr = NocInteriorMath.Depth(type, w.depth), yc = Math.Max(c.Pv - NocFacadeConstants.FaRoomSoffit, c.gy1 + 0.02);
                            double rm = NocInteriorMath.RoomMean(c.gx0, c.gx1, c.gy0, c.gy1, q.kx, q.ky, c.Pu, 0.0, yc, Dr);
                            double rr = NocInteriorMath.RoomRaw(xg, yg, q.kx, q.ky, c.Pu, 0.0, yc, Dr, out _) / Math.Max(rm, 1e-4);
                            I += (0.5 * (1.0 + NocFacadeConstants.FaFalloffSill) * rr - I) * wBox;
                        }
                        C3 eL = w.eLit;
                        if (w.kind == 1)
                        {
                            double g0 = c.gx0, g1 = c.gx1, cw = w.cf * (g1 - g0);   // a share of the glass inside the frame
                            double cov = w.side < 1.0 / 3.0 ? Cov(xg, fwu, g0, g0 + cw)
                                       : (w.side < 2.0 / 3.0 ? Cov(xg, fwu, g1 - cw, g1)
                                       : Cov(xg, fwu, g0, g0 + 0.5 * cw) + Cov(xg, fwu, g1 - 0.5 * cw, g1));
                            I *= 1.0 - (1.0 - NocFacadeConstants.FaCurtainGain) * cov;
                            C3 warmL = NormY(NocFogMath.Pal(NocPal.WinWarm), Y709(w.eLit) / NocFacadeConstants.FaYMed);
                            eL = C3.Lerp(w.eLit, warmL, NocFacadeConstants.FaCurtainTint * gwI * cov * wI);
                        }
                        else if (w.kind == 2)
                            I *= 1.0 - FilteredGrid1(yg, NocFacadeConstants.FaBlindPitch, NocFacadeConstants.FaBlindPitch * (1.0 - NocFacadeConstants.FaBlindOpen), fwv, NocFacadeConstants.FaBlindFadePx);
                        double Ieff = 1.0 + (I * invEI - 1.0) * wI;
                        R += eL * (w.lit * cg * Ieff) + Mul(w.eLit, cs) * w.lit + w.eUn * ((1 - w.lit) * cg);
                        kG += cg; kS += cs;
                    }
            }
            C3 spillE = SpillCov(OxE, OyE, VxE, VyE, GxE, GyE, HyE, q.ky, albRev, albFrame, albWall);
            o.emission = R * wuv + eGlassMean * (o.aGlass - wuv * kG) + Mul(eLitMean, spillE - kS * wuv) * pEff;

            // neighbour bands: the unresolved part of a footprint that straddles a band boundary takes that band's mean
            double lo = Cov(vl, fwv, -1e6, 0.0), hi = Cov(vl, fwv, rows * c.Pv, 1e6), kN = 1.0 - wv;
            if ((lo + hi) * kN > 0.0)
            {
                if (lo > 0.0) AddBandMeanPart(ref o, s, band - 1, q, vl + bandH, fwu, fwv, kN);
                if (hi > 0.0) AddBandMeanPart(ref o, s, band + 1, q, vl - bandH, fwu, fwv, kN);
            }
            if (q.mode == 1) SecMacro(ref o, q, fwu, fwv);
            return o;
        }

        // ------------------------------------------------------------------ section macro (mode 1, NocSecMacroV2)
        /// <summary>Lit-density multiplier of district (ub, vb) of a section face (log-normal, mean 1).</summary>
        public static double SecDistMul(uint face, double ub, double vb)
        {
            uint a = unchecked((uint)(ub + 1048576.0)), b = unchecked((uint)(vb + 1048576.0)), c = face ^ 0x44495354u;
            NocHash.Pcg3d(ref a, ref b, ref c);
            return LogNormal1(Normal(a, b), NocFacadeConstants.FaSecDistSigma);
        }

        static double SecDistFiltered(uint face, double u, double v, double fwu, double fwv)
        {
            double U = NocFacadeConstants.FaSecDistU, V = NocFacadeConstants.FaSecDistV;
            double u0 = Math.Floor((u - 0.5 * fwu) / U), v0 = Math.Floor((v - 0.5 * fwv) / V), acc = 0, ws = 0;
            for (int b = 0; b < 2; b++)
                for (int a = 0; a < 2; a++)
                {
                    double ub = u0 + a, vb = v0 + b;
                    double w = Cov(u, fwu, ub * U, (ub + 1) * U) * Cov(v, fwv, vb * V, (vb + 1) * V);
                    acc += w * SecDistMul(face, ub, vb); ws += w;
                }
            return acc / Math.Max(ws, 1e-9);
        }

        /// <summary>The section macro of the pixel's district: mechanical band with lit slits, the glazed atrium (exact coverages),
        /// blended over the window field (NocSecMacroV2 + the blend in NocFacadeEvalV2).</summary>
        static void SecMacro(ref FaceOut o, in FaceIn q, double fwu, double fwv)
        {
            double U = NocFacadeConstants.FaSecDistU, V = NocFacadeConstants.FaSecDistV, P = NocFacadeConstants.FaSecColP, Hf = NocturneConstants.FacadeSecH;
            double ub = Math.Floor(q.u / U), vb = Math.Floor(q.v / V), du = q.u - ub * U, dv = q.v - vb * V;
            uint hx = unchecked((uint)(ub + 1048576.0)), hy = unchecked((uint)(vb + 1048576.0)), hz = q.face ^ 0x4154524Du;
            NocHash.Pcg3d(ref hx, ref hy, ref hz);
            double mech = Cov(dv, fwv, 0.0, NocFacadeConstants.FaSecMechH);
            double ka = NocMath.Clamp01(q.wf / 0.32);
            C3 slitE = U01(hz) < 0.5 * ka ? NormY(NocFogMath.Pal(NocPal.SodiumAmber), 0.6) : NormY(NocFogMath.Pal(NocPal.SignWhite), 0.6);
            double d = Math.Max(NocFacadeConstants.FaSecSlitH, fwv);
            C3 em = new C3(0, 0, 0);
            double b0 = Math.Floor((du - 0.5 * fwu) / P), gap = NocFacadeConstants.FaSecSlitGap;
            if (mech > 0)
                for (int f = 0; f < 3; f++)
                {
                    double cc = Hf * (f + 0.5);
                    double cv = Cov(dv, fwv, cc - 0.5 * d, cc + 0.5 * d) * (NocFacadeConstants.FaSecSlitH / d);
                    for (int bb = 0; bb < 2; bb++)
                    {
                        double bay = b0 + bb;
                        double lit = U01(Pcg(hx ^ unchecked((uint)(f * 0x9E37 + 0x51)) ^ Pcg(unchecked((uint)(bay + 4096.0))))) < NocFacadeConstants.FaSecSlitLit ? 1.0 : 0.0;
                        em += slitE * (lit * cv * Cov(du, fwu, bay * P + 0.5 * gap, (bay + 1) * P - 0.5 * gap));
                    }
                }
            double nbu = Math.Floor(U / P), nbv = Math.Floor(V / Hf);
            double aw = NocFacadeConstants.FaSecAtriumW / P, ah = NocFacadeConstants.FaSecAtriumH / Hf, mb = NocFacadeConstants.FaSecMechH / Hf;
            double ia = 1.0 + Math.Floor(U01(hx) * (nbu - aw - 2.0)), ja = mb + 1.0 + Math.Floor(U01(hy) * (nbv - ah - mb - 2.0));
            double ax0 = ia * P, ax1 = ax0 + NocFacadeConstants.FaSecAtriumW, ay0 = ja * Hf, ay1 = ay0 + NocFacadeConstants.FaSecAtriumH;
            double atrium = Cov(du, fwu, ax0, ax1) * Cov(dv, fwv, ay0, ay1);
            double atLit = U01(Pcg(hz ^ 0x4C495431u)) < NocFacadeConstants.FaSecAtriumLit ? 1.0 : 0.0;
            C3 atHue = C3.Lerp(NormY(NocFogMath.Pal(NocPal.WinNeutral), 1.0), NormY(NocFogMath.Pal(NocPal.WinWarm), 1.0), ka * U01(Pcg(hy ^ 0x48554555u)));
            double gx = 1.0 - FilteredGrid1(du - ax0, NocFacadeConstants.FaSecAtriumMullU, NocFacadeConstants.FaSecAtriumMull, fwu, 4.0);
            double gy = 1.0 - FilteredGrid1(dv - ay0, Hf, NocFacadeConstants.FaSecAtriumMull, fwv, 4.0);
            double atGlass = gx * gy;
            double edge = FilteredGrid1(dv - ay0, Hf, NocFacadeConstants.FaSecAtriumEdge, fwv, 3.0), tH = NocMath.Clamp01((dv - ay0) / NocFacadeConstants.FaSecAtriumH);
            double hall = (NocFacadeConstants.FaSecHallBase + NocFacadeConstants.FaSecHallEdge * edge) * (1.3 - 0.6 * tH);
            em += atHue * (NocFacadeConstants.FaSecAtriumGain * atLit * atGlass * hall * atrium);
            double k = NocMath.Clamp01(1.0 - mech - atrium);
            o.emission = o.emission * k + em;
            o.aGlass = o.aGlass * k + atrium * atGlass; o.aOpen = o.aOpen * k + atrium; o.aFrame = o.aFrame * k + atrium * (1.0 - atGlass);
            o.aJamb *= k; o.aSillHead *= k; o.aSlab *= k; o.aSpandrel *= k;
            o.louver = mech;
        }

        /// <summary>The fully unresolved fractions and emission of band `band` at band-local vlNb, x k (NocBandMeanPartV2).</summary>
        static void AddBandMeanPart(ref FaceOut o, in Skin s, int band, in FaceIn q, double vlNb, double fwu, double fwv, double k)
        {
            int type = StratumV2(s.key, band, q.mode);
            Cell c = CellOf(s, type, band);
            double sx = c.D * q.kx, sy = c.D * q.ky;
            Bays(s, c, type, q.uMin, q.uMax, out int iF, out int iL, out int k0, out int k1);
            Rows(s, c, type, band, q.y0, q.y1, out int jF, out int jL);
            int rows = (int)Math.Round(c.bandH / c.Pv);
            double Ax = Cov(q.u, fwu, iF * c.Pu, (iL + 1) * c.Pu);
            if (k1 >= k0) Ax = Math.Max(0.0, Ax - Cov(q.u, fwu, k0 * c.Pu, (k1 + 1) * c.Pu));
            double Ay = Cov(vlNb, fwv, jF * c.Pv, (jL + 1) * c.Pv), Aband = Cov(vlNb, fwv, 0.0, rows * c.Pv);
            double mOx = Len(c.ox0, c.ox1) / c.Pu, mOy = Len(c.oy0, c.oy1) / c.Pv;
            double mVx = Len(Math.Max(c.ox0, c.ox0 - sx), Math.Min(c.ox1, c.ox1 - sx)) / c.Pu, mVy = Len(Math.Max(c.oy0, c.oy0 - sy), Math.Min(c.oy1, c.oy1 - sy)) / c.Pv;
            double mGx = GlassX(0, 1, 0, c, sx, true, c.Pu), mGy = GlassY(0, 1, 0, c, sy, true, c.Pv);
            double Ox = mOx * Ax, Oy = mOy * Ay, Vx = mVx * Ax, Vy = mVy * Ay, Gx = mGx * Ax, Gy = mGy * Ay, Hy = HaloY(0, 1, 0, c, true, c.Pv) * Ay;
            double aGlass = Gx * Gy;
            o.aOpen += k * Ox * Oy; o.aGlass += k * aGlass; o.aFrame += k * Math.Max(0.0, Vx * Vy - aGlass);
            o.aJamb += k * Math.Max(0.0, (Ox - Vx) * Oy); o.aSillHead += k * Math.Max(0.0, Vx * (Oy - Vy));
            o.aSlab += k * (c.slab / c.Pv) * Aband;
            if (c.sp1 > c.sp0) o.aSpandrel += k * GlassX(0, 1, 0, c, 0.0, true, c.Pu) * Ax * (Len(c.sp0, c.sp1) / c.Pv) * Ay;
            double p = NocFacadeConstants.FaLit * FactorV2(type) * TowerMul(s.key) * (s.region == RegionOQ ? NocFacadeConstants.FaOqLit : 1.0);
            C3 hue = type == StratumMech ? MeanMechHue(q.wf) : MeanHueV2(q.wf, q.nf);
            C3 eLitMean = hue * NocFacadeConstants.FaBMean, eUnMean = NocFogMath.Pal(NocPal.WinNeutral) * NocFacadeConstants.FaStandby;
            C3 albRev = WallToken(s, type) * NocFacadeConstants.FaAoReveal;
            C3 sp = SpillCov(Ox, Oy, Vx, Vy, Gx, Gy, Hy, q.ky, albRev, NocFogMath.Pal(NocPal.MetalPaint), WallToken(s, type));
            o.emission += (eLitMean * p + eUnMean * (1 - p)) * (k * aGlass) + Mul(eLitMean, sp) * (k * p);
        }

        /// <summary>Room-light spill of a lit cell (TEXTURE_PLAN 2.6) as a colour weight: jambs x 0.25, sill x 0.35 or head x 0.15 (by
        /// the sign of the vertical shift) at the reveal albedo, frame edges x 0.20 at MetalPaint, the rain-scatter halo under the
        /// opening x 0.02 at the wall albedo. Bilinear in the per-axis coverages, so the 2 x 2 sum equals the product of the sums.</summary>
        static C3 SpillCov(double ox, double oy, double vx, double vy, double gx, double gy, double hy, double ky, C3 albRev, C3 albFrame, C3 albWall)
        {
            double jamb = Math.Max(0.0, (ox - vx) * oy), sh = Math.Max(0.0, vx * (oy - vy)), fr = Math.Max(0.0, vx * vy - gx * gy);
            double fRev = NocFacadeConstants.FaSpillJamb * jamb + (ky < 0 ? NocFacadeConstants.FaSpillSill : NocFacadeConstants.FaSpillHead) * sh;
            return albRev * fRev + albFrame * (NocFacadeConstants.FaSpillFrame * fr) + new C3(1, 1, 1) * (NocFacadeConstants.FaHalo * ox * hy);
        }

        /// <summary>Mean wall albedo (the token; the photo variation has mean 1) of a skin's wall in a stratum.</summary>
        public static C3 WallToken(in Skin s, int type)
        {
            if (type == StratumSection) return NocFogMath.Pal(NocPal.ConcreteDark);
            switch (s.id)
            {
                case SkinS0: return NocFogMath.Pal(NocPal.MetalPaint);
                case SkinS2: return NocFogMath.Pal(NocPal.MetalPaint) * NocFacadeConstants.FaS2Albedo;
                case SkinS3: return NocFogMath.Pal(NocPal.Brick);
                case SkinS4: return NocFogMath.Pal(NocPal.Plaster) * NocFacadeConstants.FaS4Albedo;
                default: return NocFogMath.Pal(NocPal.ConcreteDark);
            }
        }

        /// <summary>Lit-group factor box-filtered over the groups under the v footprint (groups are full-width row intervals).</summary>
        static double GroupFiltered(uint K, int band, double vl, double fwv, in Cell c)
        {
            int rows = (int)Math.Round(c.bandH / c.Pv);
            int jc = Math.Max(0, Math.Min(rows - 1, (int)Math.Floor(vl / c.Pv)));
            GroupOf(K, band, jc, out int g, out int st, out int len);
            double y0 = st * c.Pv, y1 = (st + len) * c.Pv;
            double cIn = Cov(vl, fwv, y0, y1), cLo = Cov(vl, fwv, y0 - 64 * c.Pv, y0), cHi = Cov(vl, fwv, y1, y1 + 64 * c.Pv);
            double gLo = g > 0 ? GroupMul(K, band, g - 1) : 1.0, gHi = st + len < rows ? GroupMul(K, band, g + 1) : 1.0;
            return cIn * GroupMul(K, band, g) + cLo * gLo + cHi * gHi + Math.Max(0.0, 1.0 - cIn - cLo - cHi);
        }

        /// <summary>Zone level box-filtered over the 2 x 2 zones under the footprint (zone rows per tower, columns per face).</summary>
        static double ZoneFiltered(uint K, uint face, int band, double u, double vl, double fwu, double fwv, in Cell c)
        {
            double zh = NocturneConstants.FacadeZoneRows * c.Pv, zw = NocturneConstants.FacadeZoneCols * c.Pu;
            double r0 = Math.Floor((vl - 0.5 * fwv) / zh);
            double acc = 0, wsum = 0;
            for (int a = 0; a < 2; a++)
            {
                double zrl = r0 + a;
                double cvz = Cov(vl, fwv, zrl * zh, (zrl + 1) * zh);
                int zrg = band * 16 + (int)zrl;
                double sh = ZoneShiftV2(face, zrg), ur = ZoneRowU(K, zrg);
                double c0 = Math.Floor((u - 0.5 * fwu) / zw + sh / NocturneConstants.FacadeZoneCols);
                for (int b = 0; b < 2; b++)
                {
                    double cc = c0 + b;
                    double cuz = Cov(u, fwu, (NocturneConstants.FacadeZoneCols * cc - sh) * c.Pu, (NocturneConstants.FacadeZoneCols * (cc + 1) - sh) * c.Pu);
                    double w = cuz * cvz;
                    acc += w * ZoneZ(ur, ZoneCellU(face, zrg, (int)cc));
                    wsum += w;
                }
            }
            return acc / Math.Max(wsum, 1e-9);
        }

        // ------------------------------------------------------------------ schedules and means (TEXTURE_PLAN 2.11)
        /// <summary>Warm fraction w_f(s) and neutral fraction n_f(s) (LOOK_V2 4): w_f 0.32 to s 400, 0.20 at 880, 0 from 1 250;
        /// n_f 0.30 to s 880, 0.15 from 1 250 (piecewise linear).</summary>
        public static double WarmFractionV2(double s) => NocSchedule.WarmShare(s);       // PA's knots (NocturneConstants.WarmKnot*)
        public static double NeutralFractionV2(double s) => NocSchedule.NeutralShare(s); // PA's knots (NocturneConstants.NeutralKnot*)

        /// <summary>Frontal, unresolved mean radiance of a facade (lit glass + stand-by glass + wall x ambient + the analytic sky
        /// reflection term) in a region at path position s, for the ambient luminance ambY (beat 2: 0.0223); mode 0 facade or
        /// 1 section. The share-weighted expectation over skins and strata; the same expectation as the far regime.</summary>
        public static C3 MeanRadiance(int mode, int region, double s, double ambY = 0.0223)
        {
            double wf = NocSchedule.WarmShare(s), nf = NocSchedule.NeutralShare(s);
            C3 sum = new C3(0, 0, 0);
            if (mode == 1)
            {
                // the window field outside the mechanical bands and atria (district multiplier mean 1), the louvre walls with their
                // slits, the atria (lit share x gain x glass under the mullions; dark atria: glass at the ambient)
                var sk = SkinOf(0, 1);
                double fm = NocFacadeConstants.FaSecMechH / NocFacadeConstants.FaSecDistV;
                double fa = NocFacadeConstants.FaSecAtriumW * NocFacadeConstants.FaSecAtriumH / (NocFacadeConstants.FaSecDistU * NocFacadeConstants.FaSecDistV);
                double ka = NocMath.Clamp01(wf / 0.32);
                C3 slit = NormY(NocFogMath.Pal(NocPal.SodiumAmber), 0.6) * (0.5 * ka) + NormY(NocFogMath.Pal(NocPal.SignWhite), 0.6) * (1.0 - 0.5 * ka);
                double slitShare = 3.0 * NocFacadeConstants.FaSecSlitH / NocFacadeConstants.FaSecMechH * (1.0 - NocFacadeConstants.FaSecSlitGap / NocFacadeConstants.FaSecColP);
                C3 louvre = NocFogMath.Pal(NocPal.MetalPaint) * (NocFacadeConstants.FaLouvreAlb * 0.75 * 0.96 * ambY) + slit * (NocFacadeConstants.FaSecSlitLit * slitShare);
                double gx = 1.0 - NocFacadeConstants.FaSecAtriumMull / NocFacadeConstants.FaSecAtriumMullU, gy = 1.0 - NocFacadeConstants.FaSecAtriumMull / NocturneConstants.FacadeSecH;
                C3 atHue = C3.Lerp(NormY(NocFogMath.Pal(NocPal.WinNeutral), 1.0), NormY(NocFogMath.Pal(NocPal.WinWarm), 1.0), 0.5 * ka);
                double hallMean = (NocFacadeConstants.FaSecHallBase + NocFacadeConstants.FaSecHallEdge * NocFacadeConstants.FaSecAtriumEdge / NocturneConstants.FacadeSecH) * 1.0;
                C3 atrium = atHue * (NocFacadeConstants.FaSecAtriumGain * NocFacadeConstants.FaSecAtriumLit * gx * gy * hallMean)
                          + NocFogMath.Pal(NocPal.GlassDark) * (0.96 * ambY * (1.0 - NocFacadeConstants.FaSecAtriumLit) * gx * gy)
                          + NocFogMath.Pal(NocPal.MetalPaint) * (0.96 * ambY * (1.0 - gx * gy)) + new C3(1, 1, 1) * (NocFacadeConstants.FaGlassSpec * gx * gy);
                return BandMeanRadiance(sk, StratumSection, wf, nf, ambY) * (1.0 - fm - fa) + louvre * fm + atrium * fa;
            }
            for (int k = 0; k < 5; k++)
            {
                double share = SkinShare(region, k);
                var sk = new Skin { id = k, region = region, overrideK = -1, s1Mullion = false };
                C3 m = BandMeanRadiance(sk, StratumRes, wf, nf, ambY) * NocturneConstants.FacadeShareRes
                     + BandMeanRadiance(sk, StratumCom, wf, nf, ambY) * NocturneConstants.FacadeShareCom
                     + BandMeanRadiance(sk, StratumMech, wf, nf, ambY) * NocturneConstants.FacadeShareMech
                     + BandMeanRadiance(sk, StratumLobby, wf, nf, ambY) * (1.0 - NocturneConstants.FacadeShareRes - NocturneConstants.FacadeShareCom - NocturneConstants.FacadeShareMech);
                sum += m * share;
            }
            return sum;
        }

        public static double SkinShare(int region, int k)
        {
            bool q = region == RegionOQ;
            double[] t = q ? new[] { 0.0, NocFacadeConstants.FaShareQ0, NocFacadeConstants.FaShareQ1, NocFacadeConstants.FaShareQ2, NocFacadeConstants.FaShareQ3, 1.0 }
                           : new[] { 0.0, NocFacadeConstants.FaShareD0, NocFacadeConstants.FaShareD1, NocFacadeConstants.FaShareD2, NocFacadeConstants.FaShareD3, 1.0 };
            return t[k + 1] - t[k];
        }

        /// <summary>Clear-glass coverage of a cell seen frontally (no shift).</summary>
        public static double GlassCoverage(in Cell c) => GlassX(0, 1, 0, c, 0, true, c.Pu) * GlassY(0, 1, 0, c, 0, true, c.Pv);

        /// <summary>Frontal mean radiance of an unresolved band of one skin and stratum (whole-bay suppression excluded: an
        /// infinitely wide face): window term + wall x ambient x 0.96 (URP Lambert) + glass stand-by + the mean analytic sky
        /// reflection of walls and glass (LOOK_V2 4: 0.0006 per wall area, F0 0.04 x the sky on glass).</summary>
        public static C3 BandMeanRadiance(in Skin s, int type, double wf, double nf, double ambY)
        {
            Cell c = CellOf(s, type);
            double cg = GlassCoverage(c);
            if (type == StratumRes && s.id == SkinS1) cg = 0.5 * (cg + GlassCoverage(CellOf(new Skin { id = SkinS1, s1Mullion = true }, type)));
            if (type == StratumRes && s.id == SkinS4)
            {
                // loggia bands: the doors at the back (FaLoggiaP of the bands; the solid corner bays are left to the budget test)
                Cell lc = c;
                lc.gx0 = NocFacadeConstants.FaLogDoorX0; lc.gx1 = NocFacadeConstants.FaLogDoorX1; lc.gy0 = c.slab; lc.gy1 = c.slab + NocFacadeConstants.FaLogDoorH;
                lc.ox0 = NocFacadeConstants.FaLogX0; lc.ox1 = NocFacadeConstants.FaLogX1; lc.ff = 0.0; lc.mOff = 0.5 * c.Pu; lc.mP = 100.0; lc.mW = 0.5 * NocFacadeConstants.FaFrame;
                cg = (1.0 - NocFacadeConstants.FaLoggiaP) * cg + NocFacadeConstants.FaLoggiaP * GlassCoverage(lc);
            }
            double p = NocFacadeConstants.FaLit * FactorV2(type) * (s.region == RegionOQ ? NocFacadeConstants.FaOqLit : 1.0);
            C3 hue = type == StratumMech ? MeanMechHue(wf) : MeanHueV2(wf, nf);
            C3 lit = hue * (NocFacadeConstants.FaBMean * p * cg);
            C3 stand = NocFogMath.Pal(NocPal.WinNeutral) * (NocFacadeConstants.FaStandby * (1 - p) * cg) + NocFogMath.Pal(NocPal.GlassDark) * (0.96 * ambY * (1 - p) * cg);
            double open = Len(c.ox0, c.ox1) / c.Pu * (Len(c.oy0, c.oy1) / c.Pv);
            double wallA = 1.0 - open;
            C3 wall = WallToken(s, type) * (0.96 * ambY * wallA) + new C3(1, 1, 1) * (NocFacadeConstants.FaWallSpec * wallA);
            C3 glassSpec = new C3(1, 1, 1) * (NocFacadeConstants.FaGlassSpec * cg);
            return lit + stand + wall + glassSpec;
        }

        // ------------------------------------------------------------------ display-space coverage (INT-05, shader NocDisplayCover)
        /// <summary>The grade's Neutral highlight curve on a peak value (identity below FaToneStart; NocGradeMath.Neutral's peak).</summary>
        public static double ToneFwd(double x)
        {
            double s = NocFacadeConstants.FaToneStart, d = 1.0 - s;
            return x < s ? x : 1.0 - d * d / (x + d - s);
        }

        /// <summary>Inverse of ToneFwd for y &lt; 1 (clamped at 0.995 as the shader).</summary>
        public static double ToneInv(double y)
        {
            double s = NocFacadeConstants.FaToneStart, d = 1.0 - s;
            y = Math.Min(y, 0.995);
            return y < s ? y : s - d + d * d / (1.0 - y);
        }

        /// <summary>The peak radiance a pixel covering c of a band of peak radiance pk carries (shader NocDisplayCover): it displays as
        /// c x ToneFwd(pk).</summary>
        public static double DisplayCover(double pk, double c) => ToneInv(NocMath.Clamp01(c) * ToneFwd(pk));
    }
}
