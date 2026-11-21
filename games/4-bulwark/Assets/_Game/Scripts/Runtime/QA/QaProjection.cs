// QaProjection: double-precision rectilinear projection of named subjects through a key-shot camera
// (MASTER_PROMPT 11.2, 11.13, 14.10.2, Part 10 item 35). Imported from Thread (projection core only). The harness writes
// the projected screen boxes of every key-shot subject the beat builders register (IKeyShotSubjects) into report.json,
// so the framing acceptances (3 percent margin at 16:9, 16:10 and 21:9) are checked from the camera maths, not from the
// pixels; CompositionLint adds the depth-occlusion rays. The projection core has no UnityEngine dependency.
//
// Conventions: world yaw is the azimuth from +Z toward +X, pitch is up-positive, the vertical FOV is
// fixed (Hor+: the horizontal FOV follows the aspect, as Unity does). NDC x, y in [-1, 1] (y up);
// pixel coordinates have a top-left origin (PNG rows).
using System;
using System.Collections.Generic;

namespace Megastructure.QA
{
    /// <summary>A capture camera: position, world yaw, pitch, vertical FOV, image size.</summary>
    public struct QaView
    {
        public double x, y, z, yawDeg, pitchDeg, fovDeg;
        public int width, height;

        public QaView(double x, double y, double z, double yawDeg, double pitchDeg, double fovDeg, int width, int height)
        {
            this.x = x; this.y = y; this.z = z;
            this.yawDeg = yawDeg; this.pitchDeg = pitchDeg; this.fovDeg = fovDeg;
            this.width = width; this.height = height;
        }

        public double Aspect => height > 0 ? (double)width / height : 1.0;
        public D3 Position => new D3(x, y, z);
    }

    public static class QaProjection
    {
        /// <summary>Framing margin of 11.2: 3 percent of the frame on every side.</summary>
        public const double FrameMargin = 0.03;

        /// <summary>Unit direction of an azimuth (from +Z toward +X) and elevation, degrees.</summary>
        public static D3 AzElDir(double azDeg, double elDeg)
        {
            double a = azDeg * BwMath.Deg, e = elDeg * BwMath.Deg;
            return new D3(Math.Cos(e) * Math.Sin(a), Math.Sin(e), Math.Cos(e) * Math.Cos(a));
        }

        /// <summary>Camera basis without roll: forward, right, up (same as Quaternion.Euler(-pitch, yaw, 0)).</summary>
        public static void Basis(double yawDeg, double pitchDeg, out D3 f, out D3 r, out D3 u)
        {
            double yw = yawDeg * BwMath.Deg, p = pitchDeg * BwMath.Deg;
            f = new D3(Math.Cos(p) * Math.Sin(yw), Math.Sin(p), Math.Cos(p) * Math.Cos(yw));
            r = new D3(Math.Cos(yw), 0, -Math.Sin(yw));
            u = D3.Cross(f, r);
        }

        /// <summary>Projects a camera-relative vector. Returns false when it is behind the camera.</summary>
        public static bool ProjectDir(in QaView v, D3 d, out double ndcX, out double ndcY)
        {
            Basis(v.yawDeg, v.pitchDeg, out D3 f, out D3 r, out D3 u);
            double cz = D3.Dot(d, f);
            double cx = D3.Dot(d, r), cy = D3.Dot(d, u);
            double tanV = Math.Tan(v.fovDeg * 0.5 * BwMath.Deg);
            if (cz <= 1e-9 || tanV <= 1e-9) { ndcX = ndcY = 0; return false; }
            ndcX = cx / cz / (tanV * v.Aspect);
            ndcY = cy / cz / tanV;
            return true;
        }

        public static bool ProjectPoint(in QaView v, D3 p, out double ndcX, out double ndcY) => ProjectDir(v, p - v.Position, out ndcX, out ndcY);

        public static double PixelX(in QaView v, double ndcX) => (ndcX + 1.0) * 0.5 * v.width;
        public static double PixelYTop(in QaView v, double ndcY) => (1.0 - ndcY) * 0.5 * v.height;

        /// <summary>Focal length in pixels (vertical).</summary>
        public static double FocalPx(double fovDeg, int heightPx) => heightPx * 0.5 / Math.Tan(fovDeg * 0.5 * BwMath.Deg);

        /// <summary>Angle (deg) that moves the image centre by the given number of pixels.</summary>
        public static double PanStepDeg(double px, double fovDeg, int heightPx) => Math.Atan(px / FocalPx(fovDeg, heightPx)) / BwMath.Deg;

        public static bool InsideWithMargin(double ndcX, double ndcY, double margin = FrameMargin)
        {
            double lim = 1.0 - 2.0 * margin;
            return Math.Abs(ndcX) <= lim && Math.Abs(ndcY) <= lim;
        }

        static void Orthonormal(D3 n, out D3 a, out D3 b)
        {
            D3 h = Math.Abs(n.y) < 0.9 ? new D3(0, 1, 0) : new D3(1, 0, 0);
            a = D3.Cross(h, n).Normalized;
            b = D3.Cross(n, a).Normalized;
        }

        // ------------------------------------------------------------------ subject builders

        /// <summary>A disc at infinity (the sun disc of beats 3 and 7). Round bodies are best drawn as screen-space circular impostors (lesson 3.2 #8): the
        /// rectilinear projection of the centre direction, radius FocalPx * tan(r), a circle in pixels.</summary>
        public static QaSubject DiscAtInfinity(in QaView v, string name, D3 dir, double radiusDeg, int samples = 180)
        {
            var sub = new QaSubject(name, "disc");
            D3 c = dir.Normalized;
            double rPx = FocalPx(v.fovDeg, v.height) * Math.Tan(radiusDeg * BwMath.Deg);
            sub.radiusPx = rPx;
            if (!ProjectDir(v, c, out double nx, out double ny))
            {
                sub.samples = samples;   // centre behind the camera: counted, none in front
                return sub;
            }
            sub.SetAnchor(v, nx, ny);
            // the rim of the cone of half-angle radiusDeg round c, projected exactly (off-axis the disc projects to an
            // ellipse up to 1 / cos^2 wider than the on-axis circle of radius rPx; at 11 deg off-axis that is ~1 px)
            D3 a = Math.Abs(c.y) < 0.9 ? new D3(0, 1, 0) : new D3(1, 0, 0);
            D3 e1 = D3.Cross(a, c).Normalized, e2 = D3.Cross(c, e1);
            double cr = Math.Cos(radiusDeg * BwMath.Deg), sr = Math.Sin(radiusDeg * BwMath.Deg);
            for (int k = 0; k < samples; k++)
            {
                double phi = 2.0 * Math.PI * k / samples;
                sub.AddSample(v, c * cr + (e1 * Math.Cos(phi) + e2 * Math.Sin(phi)) * sr, true);
            }
            return sub;
        }

        /// <summary>An axis-aligned world box (KeyShotSubject.WorldBounds): its 8 corners; the anchor is the centre.</summary>
        public static QaSubject Box(in QaView v, string name, D3 min, D3 max)
        {
            var sub = new QaSubject(name, "box");
            for (int i = 0; i < 8; i++)
            {
                var c = new D3((i & 1) == 0 ? min.x : max.x, (i & 2) == 0 ? min.y : max.y, (i & 4) == 0 ? min.z : max.z);
                sub.AddSample(v, c - v.Position, true);
            }
            var centre = new D3(0.5 * (min.x + max.x), 0.5 * (min.y + max.y), 0.5 * (min.z + max.z));
            if (ProjectPoint(v, centre, out double nx, out double ny)) sub.SetAnchor(v, nx, ny);
            return sub;
        }

        /// <summary>A vertical cylinder (a lamp post, the lantern post): its two silhouette lines between y0 and y1.</summary>
        public static QaSubject Cylinder(in QaView v, string name, double cx, double cz, double radius, double y0, double y1)
        {
            var sub = new QaSubject(name, "cylinder");
            double dx = cx - v.x, dz = cz - v.z;
            double dh = Math.Sqrt(dx * dx + dz * dz);
            if (dh <= radius + 1e-6) { sub.note = "camera inside the cylinder"; return sub; }
            double azc = Math.Atan2(dx, dz);
            double delta = Math.Asin(radius / dh);
            double t = Math.Sqrt(dh * dh - radius * radius);
            for (int side = -1; side <= 1; side += 2)
            {
                double az = azc + side * delta;
                double px = v.x + t * Math.Sin(az), pz = v.z + t * Math.Cos(az);
                double ea = Math.Atan2(y0 - v.y, t), eb = Math.Atan2(y1 - v.y, t);
                for (int k = 0; k <= 60; k++)
                {
                    double e = ea + (eb - ea) * k / 60.0;
                    double py = v.y + t * Math.Tan(e);
                    sub.AddSample(v, new D3(px - v.x, py - v.y, pz - v.z), true);
                }
            }
            if (ProjectDir(v, new D3(dx, 0, dz), out double nx, out double ny)) sub.SetAnchor(v, nx, ny);
            return sub;
        }

        /// <summary>A set of world points (a doorway, a lamp, the lantern); the anchor is point 0.</summary>
        public static QaSubject Points(in QaView v, string name, D3[] world)
        {
            var sub = new QaSubject(name, "points");
            for (int i = 0; i < world.Length; i++) sub.AddSample(v, world[i] - v.Position, true);
            if (world.Length > 0 && ProjectPoint(v, world[0], out double nx, out double ny)) sub.SetAnchor(v, nx, ny);
            return sub;
        }

        static D3 Dir(double azRad, double elRad) => new D3(Math.Cos(elRad) * Math.Sin(azRad), Math.Sin(elRad), Math.Cos(elRad) * Math.Cos(azRad));
    }

    public enum FramingRule
    {
        Info = 0,          // reported only
        BBoxInside = 1,    // the whole subject inside the frame with the 3 percent margin
        AnchorInside = 2,  // the anchor (centre / base / top) inside the frame with the margin
        AnchorYRange = 3,  // anchor inside and its NDC y inside [expectYMin, expectYMax]
        MinWidthFraction = 4, // visible width at least expectYMin of the frame width
        Absent = 5,        // must not be visible (alpha zero or outside the frame)
    }

    /// <summary>Projected screen box of one named subject.</summary>
    public sealed class QaSubject
    {
        public string name, kind, note;
        public FramingRule rule;
        public bool required;
        public int samples, inFront, inside;
        public double x0 = double.MaxValue, y0 = double.MaxValue, x1 = double.MinValue, y1 = double.MinValue;   // pixels, unclipped, top-left origin
        public bool hasAnchor;
        public double anchorX, anchorY, anchorNdcX, anchorNdcY;
        public double widthPx = double.NaN, radiusPx = double.NaN;
        public double alpha = 1.0;
        public double expectNdcX = double.NaN, expectNdcY = double.NaN, expectTol = double.NaN;
        public double expectYMin = double.NaN, expectYMax = double.NaN;
        public bool ok = true;
        public string verdict = "";

        int m_width, m_height;

        public QaSubject(string name, string kind) { this.name = name; this.kind = kind; }

        public bool AnyInFront => inFront > 0;
        public double InsideFraction => samples > 0 ? (double)inside / samples : 0.0;

        public void AddSample(in QaView v, D3 d, bool count)
        {
            m_width = v.width; m_height = v.height;
            if (count) samples++;
            if (!QaProjection.ProjectDir(v, d, out double nx, out double ny)) return;
            inFront++;
            if (Math.Abs(nx) <= 1 && Math.Abs(ny) <= 1) inside++;
            double px = QaProjection.PixelX(v, nx), py = QaProjection.PixelYTop(v, ny);
            if (px < x0) x0 = px;
            if (px > x1) x1 = px;
            if (py < y0) y0 = py;
            if (py > y1) y1 = py;
            if (Math.Abs(ny) <= 1)
            {
                // horizontal extent of the samples that lie within the frame's rows (visible width)
                if (px < m_rowX0) m_rowX0 = px;
                if (px > m_rowX1) m_rowX1 = px;
            }
        }

        double m_rowX0 = double.MaxValue, m_rowX1 = double.MinValue;

        /// <summary>A sample given directly in pixels (top-left origin), for screen-space impostors.</summary>
        public void AddPixel(in QaView v, double px, double py, bool count)
        {
            m_width = v.width; m_height = v.height;
            if (count) samples++;
            inFront++;
            if (px >= 0 && px <= v.width && py >= 0 && py <= v.height) inside++;
            if (px < x0) x0 = px;
            if (px > x1) x1 = px;
            if (py < y0) y0 = py;
            if (py > y1) y1 = py;
            if (py >= 0 && py <= v.height)
            {
                if (px < m_rowX0) m_rowX0 = px;
                if (px > m_rowX1) m_rowX1 = px;
            }
        }

        public void SetAnchor(in QaView v, double ndcX, double ndcY)
        {
            m_width = v.width; m_height = v.height;
            hasAnchor = true;
            anchorNdcX = ndcX; anchorNdcY = ndcY;
            anchorX = QaProjection.PixelX(v, ndcX);
            anchorY = QaProjection.PixelYTop(v, ndcY);
        }

        /// <summary>Clipped visible width as a fraction of the frame width.</summary>
        public double VisibleWidthFraction
        {
            get
            {
                if (inFront == 0 || m_width <= 0 || m_rowX1 < m_rowX0) return 0;
                double a = Math.Max(0, m_rowX0), b = Math.Min(m_width, m_rowX1);
                return b > a ? (b - a) / m_width : 0;
            }
        }

        public bool BBoxInsideMargin => BBoxInside(QaProjection.FrameMargin);

        /// <summary>Whole subject inside the frame with no margin (reported next to the 3 percent rule).</summary>
        public bool BBoxInsideFrame => BBoxInside(0.0);

        bool BBoxInside(double margin)
        {
            if (inFront == 0 || inFront < samples || m_width <= 0) return false;
            double mx = margin * m_width, my = margin * m_height;
            return x0 >= mx && x1 <= m_width - mx && y0 >= my && y1 <= m_height - my;
        }

        public bool AnchorInsideMargin => hasAnchor && QaProjection.InsideWithMargin(anchorNdcX, anchorNdcY);

        /// <summary>Applies the rule and the optional expected position; sets ok and verdict.</summary>
        public void Evaluate()
        {
            bool pass = true;
            var why = new System.Text.StringBuilder();
            switch (rule)
            {
                case FramingRule.BBoxInside:
                    if (!BBoxInsideMargin) { pass = false; why.Append("bbox outside the 3% margin; "); }
                    break;
                case FramingRule.AnchorInside:
                    if (!AnchorInsideMargin) { pass = false; why.Append("anchor outside the 3% margin; "); }
                    break;
                case FramingRule.AnchorYRange:
                    if (!AnchorInsideMargin) { pass = false; why.Append("anchor outside the 3% margin; "); }
                    if (hasAnchor && !double.IsNaN(expectYMin) && (anchorNdcY < expectYMin || anchorNdcY > expectYMax)) { pass = false; why.Append("anchor y outside the expected range; "); }
                    break;
                case FramingRule.MinWidthFraction:
                    if (VisibleWidthFraction < expectYMin) { pass = false; why.Append("visible width below the expected fraction; "); }
                    break;
                case FramingRule.Absent:
                    if (alpha > 1e-3 && inside > 0) { pass = false; why.Append("visible although it must be absent; "); }
                    break;
            }
            if (!double.IsNaN(expectNdcX) && hasAnchor)
            {
                double d = Math.Max(Math.Abs(anchorNdcX - expectNdcX), Math.Abs(anchorNdcY - expectNdcY));
                if (d > expectTol) { pass = false; why.Append("anchor off the expected position by ").Append(d.ToString("0.000", System.Globalization.CultureInfo.InvariantCulture)).Append("; "); }
            }
            ok = pass;
            verdict = pass ? "ok" : why.ToString().TrimEnd(' ', ';');
        }

        public void Write(QaJson j)
        {
            j.BeginObject();
            j.Prop("name", name).Prop("kind", kind).Prop("rule", rule.ToString()).Prop("required", required).Prop("ok", ok).Prop("verdict", verdict);
            j.Prop("alpha", alpha).Prop("samples", samples).Prop("inFront", inFront).Prop("insideFraction", InsideFraction);
            if (inFront > 0)
            {
                j.Key("bboxPx").BeginArray(true).Value(x0).Value(y0).Value(x1).Value(y1).EndArray();
                j.Prop("bboxInsideMargin", BBoxInsideMargin).Prop("bboxInsideFrame", BBoxInsideFrame);
                j.Prop("visibleWidthFraction", VisibleWidthFraction);
            }
            if (hasAnchor)
            {
                j.Key("anchorPx").BeginArray(true).Value(anchorX).Value(anchorY).EndArray();
                j.Key("anchorNdc").BeginArray(true).Value(anchorNdcX).Value(anchorNdcY).EndArray();
                j.Prop("anchorInsideMargin", AnchorInsideMargin);
            }
            if (!double.IsNaN(widthPx)) j.Prop("widthPx", widthPx);
            if (!double.IsNaN(radiusPx)) j.Prop("radiusPx", radiusPx);
            if (!double.IsNaN(expectNdcX)) j.Key("expectNdc").BeginArray(true).Value(expectNdcX).Value(expectNdcY).Value(expectTol).EndArray();
            if (!double.IsNaN(expectYMin)) j.Key("expectRange").BeginArray(true).Value(expectYMin).Value(expectYMax).EndArray();
            if (!string.IsNullOrEmpty(note)) j.Prop("note", note);
            j.EndObject();
        }
    }

    /// <summary>
    /// Which subjects each key shot names (14.10.2 "Must be visible" column) and how they are judged. Bulwark's subjects come
    /// from the beat builders (IKeyShotSubjects registered on the WorldContext: door, horizon row, parapet row, plume, lamp
    /// post, cart, bow, islets, tower top, pendants, whale, lantern); no game-specific subject table is built in here. A subject
    /// that fits the frame must be inside with the 3 % margin (box rule); an oversize subject needs its centre in frame
    /// (CompositionLint rule).
    /// </summary>
    public static class QaFraming
    {
        /// <summary>The key-shot id of a capture tag: "KS04" from "b02_Crest_KS04_crest_sea_4k" (empty when none).</summary>
        public static string KeyFrameId(string tag)
        {
            if (string.IsNullOrEmpty(tag)) return "";
            int i = tag.IndexOf("KS", StringComparison.Ordinal);
            while (i >= 0)
            {
                bool startOk = i == 0 || tag[i - 1] == '_';
                int k = i + 2;
                while (k < tag.Length && char.IsDigit(tag[k])) k++;
                if (startOk && k > i + 2) return tag.Substring(i, k - i);
                i = tag.IndexOf("KS", i + 2, StringComparison.Ordinal);
            }
            return "";
        }

        static readonly List<KeyShotSubject> s_scratch = new List<KeyShotSubject>(16);

        /// <summary>Evaluates every subject of a key shot for one camera (subjects from the registered beat builders).</summary>
        public static List<QaSubject> Evaluate(string tag, double s, in QaView v)
        {
            var list = new List<QaSubject>(8);
            string id = KeyFrameId(tag);
            var gb = GameBootstrap.Instance;
            if (id.Length > 0 && gb != null && gb.Context != null)
            {
                s_scratch.Clear();
                foreach (var provider in gb.Context.SubjectProviders)
                {
                    try { provider.CollectSubjects(id, s_scratch); }
                    catch (Exception ex) { UnityEngine.Debug.LogError("[BW-FAIL] subjects of " + provider.GetType().Name + " threw: " + ex.Message); }
                }
                foreach (var ks in s_scratch)
                {
                    var b = ks.WorldBounds;
                    var sub = QaProjection.Box(v, ks.Name, new D3(b.min.x, b.min.y, b.min.z), new D3(b.max.x, b.max.y, b.max.z));
                    sub.required = true;
                    double hFrac = (sub.y1 - sub.y0) / Math.Max(1, v.height), wFrac = (sub.x1 - sub.x0) / Math.Max(1, v.width);
                    bool oversize = hFrac > 1.0 - 2.0 * QaProjection.FrameMargin || wFrac > 1.0 - 2.0 * QaProjection.FrameMargin;
                    sub.rule = oversize ? FramingRule.AnchorInside : FramingRule.BBoxInside;
                    if (ks.MinScreenFraction > 0) sub.note = "min screen fraction " + ks.MinScreenFraction.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture);
                    list.Add(sub);
                }
            }
            for (int i = 0; i < list.Count; i++) list[i].Evaluate();
            return list;
        }
    }
}
