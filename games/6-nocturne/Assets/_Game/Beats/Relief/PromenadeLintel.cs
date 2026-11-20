// PromenadeLintel: the garden courtyard's north side above the promenade's entrance (GE, D-029 GB-03; GateDims part 4): a 6 m deep
// wall-beam from the screen-wall slab (x 874, 0.5 m into its mass) to the east block's line x 914, z 280..286, y 40..560. Without
// it the courtyard was open to the north between the screen wall and the arcade, and from the garden (KS15, the beat-5 stills)
// the koi showed over the 38 m arcade through the promenade gap. The walker passes under its 40 m soffit (a concrete head band
// and soffit) onto the promenade; the south face (toward the garden), the north face (toward the promenade) and the east end are
// NocFacade facade mode, the top a roof cap at the face top. Its west end lies inside the screen-wall slab (no face there).
using System.Collections.Generic;
using UnityEngine;

namespace Megastructure
{
    public static class PromenadeLintel
    {
        public const uint IdSouth = 0x6400u, IdNorth = 0x6401u, IdEnd = 0x6402u, IdTop = 0x6403u;
        public const int Beat = 5;

        public static void Build(List<ReliefPart> parts)
        {
            double x0 = GateDims.PlX0, x1 = GateDims.PlX1, z0 = GateDims.PlZ0, z1 = GateDims.PlZ1, y0 = GateDims.PlY0, top = EdgeDims.FaceTop;
            var origin = ReliefKit.P(894.0, 300.0, 283.0);
            var fac = new ReliefPart("PromenadeLintel_facade", ReliefMat.Facade, origin, Beat);
            var conc = new ReliefPart("PromenadeLintel_concrete", ReliefMat.Concrete, origin, Beat);
            parts.Add(fac); parts.Add(conc);
            var fw = new FacadeWriter(fac.Mb, origin);
            byte F = FacadeWriter.ModeFacade;
            fw.Flat(ReliefKit.P(x0, y0, z0), ReliefKit.P(x0, top, z0), ReliefKit.P(x1, top, z0), ReliefKit.P(x1, y0, z0), Vector3.back,
                (float)x0, (float)x0, (float)x1, (float)x1, IdSouth, F, (float)x0, (float)x1);
            fw.Flat(ReliefKit.P(x0, y0, z1), ReliefKit.P(x0, top, z1), ReliefKit.P(x1, top, z1), ReliefKit.P(x1, y0, z1), Vector3.forward,
                (float)x1, (float)x1, (float)x0, (float)x0, IdNorth, F, (float)x0, (float)x1);
            fw.Flat(ReliefKit.P(x1, y0, z0), ReliefKit.P(x1, top, z0), ReliefKit.P(x1, top, z1), ReliefKit.P(x1, y0, z1), Vector3.right,
                (float)z0, (float)z0, (float)z1, (float)z1, IdEnd, F, (float)z0, (float)z1);
            fw.Flat(ReliefKit.P(x0, top, z0), ReliefKit.P(x1, top, z0), ReliefKit.P(x1, top, z1), ReliefKit.P(x0, top, z1), Vector3.up,
                (float)x0, (float)x1, (float)x1, (float)x0, IdTop, FacadeWriter.ModeRoof, (float)x0, (float)x1);
            var o = origin; var cb = conc.Mb;
            EdgeMesh.FlatQuad(cb, o, ReliefKit.P(x0, y0, z0), ReliefKit.P(x1, y0, z0), ReliefKit.P(x1, y0, z1), ReliefKit.P(x0, y0, z1), Vector3.down);
            // head band around the soffit (2.0 m, 0.4 m proud on the three free sides; its corners overlap, tops 2 cm apart)
            ReliefKit.Band(cb, o, x0, x1 + 0.4, y0 - 0.6, y0 + 1.4, z0 - 0.4, z0 + 0.3);
            ReliefKit.Band(cb, o, x0, x1 + 0.4, y0 - 0.62, y0 + 1.38, z1 - 0.3, z1 + 0.4);
            ReliefKit.Band(cb, o, x1 - 0.3, x1 + 0.42, y0 - 0.64, y0 + 1.36, z0 - 0.38, z1 + 0.38);
        }
    }
}
