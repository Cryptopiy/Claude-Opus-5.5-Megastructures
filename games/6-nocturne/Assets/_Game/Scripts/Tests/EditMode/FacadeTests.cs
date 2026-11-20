// FacadeTests: EditMode tests of the facade's face id (MASTER_PROMPT 14.7 H2, D-004): world-derived (same face from two
// constructions = same id) and exact in a float vertex attribute. TP-2 (FX, Phase 4 wave A) deleted the v1 window model with
// its tests (T-WIN-MEAN, T-WIN-BOX, T-WIN-RATE, T-WIN-WARM of NocFacadeMath.Windows); the Look v2 facade's energy, filter,
// toggle-rate and warm-share tests are FacadeSkinTests.
using System;
using NUnit.Framework;
using UnityEngine;

namespace Megastructure.Tests
{
    public class FacadeTests
    {
        [Test]
        public void FaceId_WorldDerivedAndFloatExact()
        {
            Assert.AreEqual(NocFacadeMath.FaceId(30.0, 1234.5), NocFacadeMath.FaceId(390.0, 1234.5 + 0.05), "same yaw lattice / offset lattice");
            Assert.AreNotEqual(NocFacadeMath.FaceId(30.0, 1234.5), NocFacadeMath.FaceId(30.5, 1234.5));
            // perspective interpolation of a constant id: (a w0/z0 + a w1/z1 + a w2/z2) / (w0/z0 + w1/z1 + w2/z2) in float
            var rng = new System.Random(3);
            for (int k = 0; k < 20000; k++)
            {
                float a = NocFacadeMath.FaceId(0.5 * k, 7.0 * k);
                float w0 = (float)rng.NextDouble(), w1 = (float)rng.NextDouble() * (1 - w0), w2 = 1 - w0 - w1;
                float z0 = 10 + 6000 * (float)rng.NextDouble(), z1 = 10 + 6000 * (float)rng.NextDouble(), z2 = 10 + 6000 * (float)rng.NextDouble();
                float v = (a * w0 / z0 + a * w1 / z1 + a * w2 / z2) / (w0 / z0 + w1 / z1 + w2 / z2);
                Assert.AreEqual((uint)a, (uint)(v + 0.5f), "interpolated id rounds back");
            }
            for (int k = 0; k < 1000; k++)
            {
                uint id = NocFacadeMath.FaceId(0.5 * k, 3.25 * k - 900);
                Assert.Less(id, 1u << 16, "16-bit id (survives attribute interpolation)");
                Assert.AreEqual(id, (uint)(float)id, "exact in a float vertex attribute");
            }
        }
    }
}
