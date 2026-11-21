// SeaDisturbances: the shared bus through which the kraken (Addendum A1) tells the sea, the mist and the audio where it touches
// the sea surface. One writer: KrakenDirector (the kraken owner) calls Begin(), Set(...) per active disturbance and Publish()
// once per frame (also in capture mode, after it posed the kraken for the still). Readers:
//   - BwSea (sea owner): foam rings, white water and a local displacement around Breach/TentacleRise/Slap, the dark submerged
//     mass for Shadow (subsurface colour darkened in a soft oriented ellipse), a V-wake for Wake;
//   - MistVolumeFeature (mist owner): spray columns and falling-water mist for Breach/TentacleRise/Slap;
//   - audio and beats may read Count/Get on the CPU.
// Shader globals (written only here): _BwSeaDistCount (int as float), _BwSeaDist0[8] = (x, z, halfAlong, halfAcross) in world m,
// _BwSeaDist1[8] = (headingRad, strength 0..1, kind as float, heightAboveSea m). Heading 0 = +z (north), clockwise to +x.
// Lead-owned contract: extend, never break.
using UnityEngine;

namespace Megastructure
{
    public enum SeaDisturbanceKind
    {
        /// <summary>A dark mass just below the surface (subsurface darkening, slight swell); no foam.</summary>
        Shadow = 0,
        /// <summary>The body breaking the surface: foam ring, white water, displacement bulge, spray.</summary>
        Breach = 1,
        /// <summary>A tentacle rising out of / sinking into the sea: foam collar at the waterline, water cascading off it.</summary>
        TentacleRise = 2,
        /// <summary>A tentacle slapping the surface: a short burst of spray and a foam ring.</summary>
        Slap = 3,
        /// <summary>A moving body just below the surface: V-wake behind the heading, faint foam lines.</summary>
        Wake = 4,
    }

    public struct SeaDisturbance
    {
        public SeaDisturbanceKind Kind;
        /// <summary>World x, z of the centre (m).</summary>
        public float X, Z;
        /// <summary>Half extents along / across the heading (m). For TentacleRise: the waterline radius of the tentacle.</summary>
        public float HalfAlong, HalfAcross;
        /// <summary>Heading in radians, 0 = +z (north), clockwise towards +x.</summary>
        public float Heading;
        /// <summary>0..1 (fades in and out with the event).</summary>
        public float Strength;
        /// <summary>Height of the emerged part above the mean sea surface (m), for spray and cascades.</summary>
        public float HeightAboveSea;
    }

    public static class SeaDisturbances
    {
        public const int Max = 8;
        static readonly SeaDisturbance[] s_items = new SeaDisturbance[Max];
        static readonly Vector4[] s_g0 = new Vector4[Max];
        static readonly Vector4[] s_g1 = new Vector4[Max];
        static int s_count;

        static readonly int IdCount = Shader.PropertyToID("_BwSeaDistCount");
        static readonly int IdDist0 = Shader.PropertyToID("_BwSeaDist0");
        static readonly int IdDist1 = Shader.PropertyToID("_BwSeaDist1");

        public static int Count => s_count;
        public static SeaDisturbance Get(int i) => s_items[i];

        /// <summary>Writer: starts a new frame's list.</summary>
        public static void Begin() { s_count = 0; }

        /// <summary>Writer: adds one disturbance (ignored when full or when strength is 0).</summary>
        public static void Set(in SeaDisturbance d)
        {
            if (s_count >= Max || !(d.Strength > 0f)) return;
            s_items[s_count++] = d;
        }

        /// <summary>Writer: uploads the list to the shader globals (unused slots get strength 0).</summary>
        public static void Publish()
        {
            for (int i = 0; i < Max; i++)
            {
                if (i < s_count)
                {
                    var d = s_items[i];
                    s_g0[i] = new Vector4(d.X, d.Z, Mathf.Max(0.01f, d.HalfAlong), Mathf.Max(0.01f, d.HalfAcross));
                    s_g1[i] = new Vector4(d.Heading, Mathf.Clamp01(d.Strength), (float)d.Kind, Mathf.Max(0f, d.HeightAboveSea));
                }
                else { s_g0[i] = new Vector4(0f, 0f, 0.01f, 0.01f); s_g1[i] = Vector4.zero; }
            }
            Shader.SetGlobalFloat(IdCount, s_count);
            Shader.SetGlobalVectorArray(IdDist0, s_g0);
            Shader.SetGlobalVectorArray(IdDist1, s_g1);
        }

        /// <summary>Clears the bus and its globals (load, teleport, tests).</summary>
        public static void Clear() { s_count = 0; Publish(); }
    }
}
