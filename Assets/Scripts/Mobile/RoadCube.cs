using UnityEngine;
using UnityEngine.UI;

namespace RenkYolu.Mobile
{
    internal sealed class RoadCube
    {
        private readonly Image shadow;
        private readonly Image face;
        private readonly Image edge;
        private readonly Image mouth;
        private readonly RectTransform body;
        private readonly float size;

        internal RoadCube(RoadTheme theme, RectTransform parent, float size, int style = 0)
        {
            this.size = size;
            float ratio = size / 100;
            shadow = theme.Box(parent, "Küp gölgesi", 0, 0, size * .88f, size * .24f,
                new Color(.04f, .03f, .10f, .44f), Mathf.Max(4, (int)(11 * ratio)));
            body = RoadTheme.Place(parent, "Küp", 0, 0, size, size);
            body.pivot = new Vector2(.5f, .5f);
            int radius = Mathf.Max(6, Mathf.RoundToInt(20 * ratio));
            edge = theme.Box(body, "Yüzey", 0, 6 * ratio, size, size, RoadTheme.Amber, radius);
            face = theme.Box(body, "Yüz", 0, 0, size, size * .95f, RoadTheme.Amber, radius);
            theme.Box(body, "Işık", 13 * ratio, 9 * ratio, 52 * ratio, 5 * ratio,
                new Color(1, 1, 1, .58f), Mathf.Max(2, (int)(3 * ratio)));
            Eye(26, style == 1 ? 8 : 10);
            Eye(63, style == 2 ? 7 : 10);
            mouth = theme.Box(body, "Ağız", 39 * ratio, 62 * ratio, 22 * ratio, 14 * ratio, RoadTheme.Ink,
                Mathf.Max(2, (int)(7 * ratio)));
            theme.Box(mouth.rectTransform, "Dil", 7 * ratio, 8 * ratio, 9 * ratio, 4 * ratio, RoadTheme.Pink,
                Mathf.Max(1, (int)(2 * ratio)));
            if (style == 2)
                theme.Icon(body, "spark", 70 * ratio, 13 * ratio, 18 * ratio, new Color(.22f, .12f, .35f, .75f));
            if (style == 3)
            {
                theme.Box(body, "Sol yanak", 13 * ratio, 60 * ratio, 12 * ratio, 6 * ratio, RoadTheme.Pink, 3);
                theme.Box(body, "Sağ yanak", 75 * ratio, 60 * ratio, 12 * ratio, 6 * ratio, RoadTheme.Pink, 3);
            }

            void Eye(float x, float width)
            {
                theme.Box(body, "Göz", x * ratio, 37 * ratio, width * ratio, 19 * ratio,
                    RoadTheme.Ink, Mathf.Max(2, (int)(5 * ratio)));
            }
        }

        internal void Draw(Vector2 center, float rise, float squash, float tilt, Color color, bool surprised = false)
        {
            float shadowScale = 1 - Mathf.Clamp01(rise / (size * 0.8f)) * .30f;
            float shadowWidth = size * .88f * shadowScale;
            RoadTheme.Move(shadow.rectTransform, center.x - shadowWidth / 2, center.y + size * .27f,
                shadowWidth, size * .24f);
            body.anchoredPosition = new Vector2(center.x, -center.y + rise + size * .18f);
            body.localRotation = Quaternion.Euler(0, 0, tilt);
            body.localScale = new Vector3(1 + squash, 1 - squash, 1);
            face.color = color;
            edge.color = Color.Lerp(color, RoadTheme.Ink, .35f);
            mouth.rectTransform.sizeDelta = size * (surprised ? new Vector2(.19f, .23f) : new Vector2(.22f, .14f));
        }
    }
}
