// NocFacadeSheet: the Look v2 facade lookdev sheet (docs/TEXTURE_PLAN.md 7.2, owner FA) as an IBeatBuilder that builds only with the
// flag -facade-sheet. Every regional skin (S0..S4, region D), every hero override (0xF0 + k, k 0..4) and the section mode stand as
// isolated towers (30 x 30 m, y 20..220, no edge lights) in one row across the north Gulf at z 5 600, where nothing else stands
// (faces A and B at full flare, x -485 and 1 515; the last cross bridge is at z 2 300). Each subject stands three times, turned 0, 45
// and 75 deg from the row's south face (incidences 0 / 45 / 75 and their complements on the second visible face), at a 54 m pitch.
// Poses() lists the stills south of the row looking north at the eye height of the towers' middle (y 120): 3 000 and 1 000 m (the
// whole row), 300 m (subjects 0-3, 4-7, 8-10), 100 m (one subject, its three turns) and 30 m (one subject, its frontal tower), as
// "pose:NAME:x:y:z:yaw:pitch:fov:s" lines (s = SheetS, beat 2) for ShotPlan's pose parser; all 27 in one run with TQ's set:
//   -facade-sheet -capture <dir> -capture-set facadesheet -freeze-clock -rain 0
// Tools/qa/facade_sheet.py measures the captures (floor-line autocorrelation, window / pier contrast, texture_std after the encode,
// the left / right reveal sign, the mean luma per skin).
using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace Megastructure
{
    public sealed class NocFacadeSheet : IBeatBuilder
    {
        public const string Flag = "-facade-sheet";
        public string Name => "ZZ_FacadeSheet";

        /// <summary>Row geometry: tower centres x = X0 + Pitch i (i = 3 subject + turn), z = RowZ; half size Half; y Y0..Y1.</summary>
        public const double X0 = -383.0, Pitch = 54.0, RowZ = 5600.0, Half = 15.0, Y0 = 20.0, Y1 = 220.0, EyeY = 120.0;
        public static readonly double[] Turns = { 0.0, 45.0, 75.0 };
        public static readonly double[] Distances = { 30.0, 100.0, 300.0, 1000.0, 3000.0 };
        public const int SubjectCount = 11;
        /// <summary>The path position whose parameters the sheet is shot under (beat 2, the boulevard; TEXTURE_PLAN 7.2).</summary>
        public const double SheetS = 300.0;

        /// <summary>Subject k: 0..4 the regional skin S_k of region D, 5..9 the hero override k - 5, 10 the section mode.</summary>
        public static string SubjectName(int k) => k < 5 ? "S" + k : (k < 10 ? "O" + (k - 5) : "SEC");

        /// <summary>The skin byte of subject k (the first region-D byte whose skin is S_k; overrides 0xF0 + k - 5; section: 0).</summary>
        public static int SubjectByte(int k)
        {
            if (k >= 10) return 0;
            if (k >= 5) return 0xF0 + (k - 5);
            for (int b = 0; b < NocFacadeConstants.FaRegionSpan; b++) if (NocFacadeMath.SkinOf(b).id == k) return b;
            return 0;
        }

        public static double TowerX(int subject, int turn) => X0 + Pitch * (3 * subject + turn);

        /// <summary>The sheet's stills (pose lines). The front face of an unturned tower is at z = RowZ - Half.</summary>
        public static List<string> Poses()
        {
            var inv = CultureInfo.InvariantCulture;
            var list = new List<string>();
            double front = RowZ - Half, mid = 0.5 * (TowerX(0, 0) + TowerX(SubjectCount - 1, 2));
            // the ninth field fixes s at beat 2 (TQ's pose s): parameters, fog, w_f / n_f and the ambient of the boulevard
            void Add(string name, double x, double d) => list.Add(string.Format(inv, "pose:{0}:{1:F0}:{2:F0}:{3:F0}:0:0:66:{4:F0}", name, x, EyeY, front - d, SheetS));
            Add("fs3000", mid, 3000.0);
            Add("fs1000", mid, 1000.0);
            Add("fs300a", 0.5 * (TowerX(0, 0) + TowerX(3, 2)), 300.0);
            Add("fs300b", 0.5 * (TowerX(4, 0) + TowerX(7, 2)), 300.0);
            Add("fs300c", 0.5 * (TowerX(8, 0) + TowerX(10, 2)), 300.0);
            for (int k = 0; k < SubjectCount; k++) Add("fs100_" + SubjectName(k), TowerX(k, 1), 100.0);
            for (int k = 0; k < SubjectCount; k++) Add("fs30_" + SubjectName(k), TowerX(k, 0), 30.0);
            return list;
        }

        public void Build(WorldContext ctx)
        {
            if (!CliArgs.Has(Flag) || ctx.Materials == null || ctx.Materials.Facade == null) return;
            var root = new GameObject("FacadeSheet").transform;
            if (ctx.BackdropRoot != null) root.SetParent(ctx.BackdropRoot, false);
            int towers = 0;
            for (int k = 0; k < SubjectCount; k++)
                for (int t = 0; t < Turns.Length; t++)
                {
                    double cx = TowerX(k, t), cz = RowZ;
                    var mb = new MeshBuilder();
                    var origin = new Vector3((float)cx, 0f, (float)cz);
                    Tower(mb, origin, cx, cz, Turns[t], SubjectByte(k), k >= 10 ? 1 : 0);
                    G4Kit.Add(ctx, root, "facadeSheet_" + SubjectName(k) + "_" + Turns[t].ToString("F0", CultureInfo.InvariantCulture), mb, ctx.Materials.Facade, origin);
                    towers++;
                }
            ctx.Log(FormattableString.Invariant($"facade sheet built: {towers} towers ({SubjectCount} subjects x {Turns.Length} turns) at z {RowZ}, x {TowerX(0, 0)}..{TowerX(SubjectCount - 1, 2)}; {Poses().Count} poses"));
        }

        /// <summary>The four vertical faces of a square tower turned by `turnDeg` about y (0: faces along x and z). Regional skins are
        /// written with seed = byte in region D (SkinByte(b, D) = b), overrides with SkinOverride(k); the section subject is mode 1
        /// with y0 = y1 = 0 (unknown, as the Gulf writers).</summary>
        static void Tower(MeshBuilder mb, Vector3 origin, double cx, double cz, double turnDeg, int skinByte, int mode)
        {
            double th = turnDeg * NocMath.Deg, c = Math.Cos(th), s = Math.Sin(th);
            Vector3 P(double lx, double lz, double y) => new Vector3((float)(cx + lx * c + lz * s), (float)y, (float)(cz - lx * s + lz * c));
            Vector3 N(double lx, double lz) => new Vector3((float)(lx * c + lz * s), 0f, (float)(-lx * s + lz * c));
            uint seed = skinByte >= NocFacadeConstants.FaOverrideBase ? NocFacadeMath.SkinOverride(skinByte - NocFacadeConstants.FaOverrideBase) : (uint)skinByte;
            int v0 = mb.VertexCount;
            double h = Half;
            // south (-z), east (+x), north (+z), west (-x) in the tower's own frame; corners a (edge A) and b (edge B) seen from outside
            G4Kit.FacadeQuad(mb, origin, P(-h, -h, Y0), P(-h, -h, Y1), P(h, -h, Y1), P(h, -h, Y0), N(0, -1), seed, 0, mode, NocFacadeMath.RegionD);
            G4Kit.FacadeQuad(mb, origin, P(h, -h, Y0), P(h, -h, Y1), P(h, h, Y1), P(h, h, Y0), N(1, 0), seed, 0, mode, NocFacadeMath.RegionD);
            G4Kit.FacadeQuad(mb, origin, P(h, h, Y0), P(h, h, Y1), P(-h, h, Y1), P(-h, h, Y0), N(0, 1), seed, 0, mode, NocFacadeMath.RegionD);
            G4Kit.FacadeQuad(mb, origin, P(-h, h, Y0), P(-h, h, Y1), P(-h, -h, Y1), P(-h, -h, Y0), N(-1, 0), seed, 0, mode, NocFacadeMath.RegionD);
            if (mode == 1)
                for (int i = v0; i < mb.UV1.Count; i++) { var q = mb.UV1[i]; mb.UV1[i] = new Vector4(q.x, q.y, 0f, 0f); }
        }
    }
}
