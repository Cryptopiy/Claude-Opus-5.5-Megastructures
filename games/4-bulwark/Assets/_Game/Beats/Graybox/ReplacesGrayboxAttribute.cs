// ReplacesGraybox: a beat builder declares which graybox parts it replaces, so no surface is drawn twice (FRAMEWORK.md 4.0).
// Usage on a builder class:  [ReplacesGraybox("beat2")]  [ReplacesGraybox("sea")]
// Part keys: "beat1".."beat7" (the graybox pieces inside that beat's z / s range), "sea" (the graybox sea plane),
// "sky" (the stand-in sky sphere), "lights" (the graybox lamp lights). Add the attribute only once your replacement renders.
// GrayboxReplace.IsReplaced(part) is what the graybox builder checks (reflection over this assembly, cached).
using System;
using System.Collections.Generic;

namespace Megastructure.Beats
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public sealed class ReplacesGrayboxAttribute : Attribute
    {
        public readonly string Part;
        public ReplacesGrayboxAttribute(string part) { Part = part; }
    }

    public static class GrayboxReplace
    {
        static HashSet<string> s_parts;

        /// <summary>True when some class in this assembly carries [ReplacesGraybox(part)].</summary>
        public static bool IsReplaced(string part)
        {
            if (s_parts == null)
            {
                var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (var t in typeof(GrayboxReplace).Assembly.GetTypes())
                    foreach (var a in t.GetCustomAttributes(typeof(ReplacesGrayboxAttribute), false))
                        set.Add(((ReplacesGrayboxAttribute)a).Part);
                s_parts = set;
            }
            return s_parts.Contains(part);
        }
    }
}
