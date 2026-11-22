// PromenadeLintelTests (integration round, D-041 (5) / SG's C16): the promenade lintel's soffit against its two jobs. KS16's built wall
// share (target 0.55 at 16:9) wants the soffit out of the frame (it leaves the frame entirely at y 70); the lintel exists to keep the
// koi hidden from the garden (D-029 GB-03: the sight lines over the 38 m arcade pass z 275 at y 58-78). The test builds the whole
// world (VisibilityWorld, the audit's own builder) at the shipped soffit and at y 70 and runs the visibility audit on each:
// the shipped soffit must keep every pre-reveal pose free of the koi and reach the share; the y 70 run is reported (it decides
// whether "out of the frame" can keep the promenade's framing).
using System;
using System.Globalization;
using NUnit.Framework;
using UnityEngine;

namespace Megastructure.Tests
{
    public class PromenadeLintelTests
    {
        static void Audit(double soffit, out int koiVisible, out int promenade, out double share169, out double share219, out string where)
        {
            double keep = PromenadeLintel.SoffitY;
            PromenadeLintel.SoffitY = soffit;
            try
            {
                VisibilityRaycaster built = VisibilityWorld.Build(out VisibilityWorld.Info info);
                var audit = new VisibilityAudit(new VisibilitySpecWorld(), built.TriangleCount > 0 ? built : null);
                audit.Run();
                koiVisible = 0; promenade = 0; where = "";
                foreach (var p in audit.PreReveal)
                {
                    koiVisible += p.visible; promenade += p.viaPromenade;
                    if (p.visible > 0) where += string.Format(CultureInfo.InvariantCulture, " {0} (s {1:0}: {2}, promenade {3})", p.tag, p.s, p.visible, p.viaPromenade);
                }
                share169 = double.NaN; share219 = double.NaN;
                foreach (var r in audit.Results)
                    if (r.shot != null && r.shot.ks.tag != null && r.shot.ks.tag.StartsWith("KS16", StringComparison.Ordinal) && r.sharesBuilt != null && r.sharesBuilt.Length >= 2)
                    { share169 = r.sharesBuilt[0]; share219 = r.sharesBuilt[1]; }
            }
            finally { PromenadeLintel.SoffitY = keep; }
        }

        [Test]
        public void Soffit_KeepsTheKoiHidden_AndOpensKS16()
        {
            Audit(GateDims.PlY0, out int koi, out int prom, out double s169, out double s219, out string where);
            Audit(70.0, out int koi70, out int prom70, out double s169b, out double s219b, out string where70);
            Debug.Log(string.Format(CultureInfo.InvariantCulture,
                "[NOC] promenade lintel: soffit y {0:0}: KS16 built wall share {1:0.000} (16:9) / {2:0.000} (21:9), pre-reveal koi sample-phases visible {3} (promenade {4}){5}; " +
                "soffit y 70 (out of the KS16 frame): share {6:0.000} / {7:0.000}, koi visible {8} (promenade {9}){10}",
                GateDims.PlY0, s169, s219, koi, prom, where, s169b, s219b, koi70, prom70, where70));
            Assert.AreEqual(50.0, GateDims.PlY0, 1e-9);
            Assert.AreEqual(0, koi, "the shipped soffit keeps the koi hidden before the reveal (D-029 GB-03)");
            Assert.GreaterOrEqual(s169, 0.55, "KS16 built wall share at 16:9 (D-041 (5) target)");
        }
    }
}
