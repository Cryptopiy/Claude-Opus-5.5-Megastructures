// PanelBuilder: the falling panel (docs/DREAD.md 3.2; owner DR): a 12 x 8 x 0.4 m sign panel in its local frame (origin = its
// centre; its lit face looks -x, toward the walk; width along z, height along y): a dark steel box (NocHull) with a backlit face
// inset 0.6 m (vertex G = 1: NocHull's emission mask; a plain soft-white field, no text). PanelCourse moves it.
using UnityEngine;

namespace Megastructure
{
    public static class PanelBuilder
    {
        public const float Inset = 0.6f;

        public static MeshBuilder Build()
        {
            var mb = new MeshBuilder();
            float t = (float)PanelCourse.Thick, h = (float)PanelCourse.Height, w = (float)PanelCourse.Width;
            mb.Color = new Color32(255, 0, 0, 255);
            mb.Box(Vector3.zero, new Vector3(t, h, w));
            // the lit field, 2 cm in front of the face (-x), inset from the frame
            mb.Color = new Color32(255, 255, 0, 255);
            float x = -0.5f * t - 0.02f, y = 0.5f * h - Inset, z = 0.5f * w - Inset;
            HullBuilder.OutQuad(mb, new Vector3(x, -y, -z), new Vector3(x, y, -z), new Vector3(x, y, z), new Vector3(x, -y, z), Vector3.zero);
            mb.Color = new Color32(255, 0, 0, 255);
            return mb;
        }
    }
}
