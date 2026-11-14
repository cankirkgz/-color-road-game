using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RenkYolu.Grid;

namespace RenkYolu.Mobile
{
    internal sealed class RoadTheme
    {
        internal static readonly Color32 Background = Hex(0x131126);
        internal static readonly Color32 Panel = Hex(0x201D38);
        internal static readonly Color32 Rim = Hex(0x42395E);
        internal static readonly Color32 Raised = Hex(0x2C2548);
        internal static readonly Color32 Cream = Hex(0xFFF0D6);
        internal static readonly Color32 Muted = Hex(0xB4AACB);
        internal static readonly Color32 Amber = Hex(0xFFC670);
        internal static readonly Color32 Mint = Hex(0x89EBC8);
        internal static readonly Color32 Pink = Hex(0xFFA5BD);
        internal static readonly Color32 Ink = Hex(0x26213C);

        private readonly Dictionary<string, Sprite> sprites = new Dictionary<string, Sprite>();

        internal Font Display { get; } = Load("BricolageGrotesque-ExtraBold");
        internal Font Bold { get; } = Load("DMSans-Bold");
        internal Font Body { get; } = Load("DMSans-Regular");

        internal static Color32 Hex(uint rgb) =>
            new Color32((byte)(rgb >> 16), (byte)(rgb >> 8), (byte)rgb, 255);

        internal static Color32 ColorOf(TileColorType color)
        {
            return color switch
            {
                TileColorType.Red => Hex(0xFFA5B6),
                TileColorType.Blue => Hex(0x9EC9FF),
                TileColorType.Green => Hex(0x91E5BF),
                TileColorType.Yellow => Hex(0xFFE08B),
                TileColorType.Purple => Hex(0xCDB2FD),
                _ => Hex(0xFFBD95)
            };
        }

        internal Image Box(RectTransform parent, string name, float x, float y, float w, float h, Color color, int radius = 20)
        {
            return Picture(parent, name, x, y, w, h, color, Rounded(radius), Image.Type.Sliced);
        }

        internal Image Icon(RectTransform parent, string icon, float x, float y, float size, Color color) =>
            Picture(parent, icon, x, y, size, size, color, Symbol(icon), Image.Type.Simple);

        internal Text Text(RectTransform parent, string value, float x, float y, float w, float h,
            int size, Color color, Font font, TextAnchor anchor = TextAnchor.MiddleLeft)
        {
            RectTransform rt = Place(parent, value, x, y, w, h);
            Text text = rt.gameObject.AddComponent<Text>();
            text.font = font;
            text.fontSize = size;
            text.color = color;
            text.text = value;
            text.alignment = anchor;
            text.raycastTarget = false;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            return text;
        }

        internal Button Button(RectTransform parent, string label, float x, float y, float w, float h,
            Color fill, Color ink, Action click, string icon = null)
        {
            Image background = Box(parent, label, x, y, w, h, fill, 18);
            background.raycastTarget = true;
            Button button = background.gameObject.AddComponent<Button>();
            button.targetGraphic = background;
            button.colors = new ColorBlock
            {
                normalColor = Color.white,
                highlightedColor = new Color(1f, 0.96f, 0.90f),
                pressedColor = new Color(0.76f, 0.70f, 0.82f),
                selectedColor = Color.white,
                disabledColor = new Color(0.60f, 0.57f, 0.67f, 0.68f),
                colorMultiplier = 1f,
                fadeDuration = 0.10f
            };
            button.navigation = new Navigation { mode = Navigation.Mode.None };
            if (click != null)
                button.onClick.AddListener(() => click());
            if (icon != null)
                Icon(background.rectTransform, icon, label.Length == 0 ? (w - 26) / 2 : 19, (h - 26) / 2, 26, ink);
            if (label.Length > 0)
            {
                float offset = icon == null ? 0 : 25;
                Text title = Text(background.rectTransform, label, offset, 0, w - offset, h,
                    h >= 58 ? 25 : 21, ink, Bold, TextAnchor.MiddleCenter);
                title.resizeTextForBestFit = true;
                title.resizeTextMinSize = 16;
                title.resizeTextMaxSize = title.fontSize;
            }

            return button;
        }

        internal static RectTransform Place(RectTransform parent, string name, float x, float y, float w, float h)
        {
            GameObject obj = new GameObject(name, typeof(RectTransform));
            RectTransform rect = (RectTransform)obj.transform;
            rect.SetParent(parent, false);
            rect.anchorMin = rect.anchorMax = new Vector2(0, 1);
            rect.pivot = new Vector2(0, 1);
            Move(rect, x, y, w, h);
            return rect;
        }

        internal static void Move(RectTransform rect, float x, float y, float w, float h)
        {
            rect.anchoredPosition = new Vector2(x, -y);
            rect.sizeDelta = new Vector2(w, h);
        }

        internal Sprite Rounded(int radius)
        {
            if (radius < 2)
                radius = 0;
            string key = "round" + radius;
            if (!sprites.TryGetValue(key, out Sprite sprite))
            {
                int size = radius * 2 + 8;
                sprite = Raster(size, size, (x, y) =>
                {
                    var rounded = new Vector2(Mathf.Clamp(x, radius, size - radius), Mathf.Clamp(y, radius, size - radius));
                    return radius == 0 ? 1f : Mathf.Clamp01(radius + 0.5f - Vector2.Distance(new Vector2(x, y), rounded));
                }, radius + 1);
                sprites.Add(key, sprite);
            }

            return sprite;
        }

        internal Sprite Symbol(string name)
        {
            if (sprites.TryGetValue(name, out Sprite sprite))
                return sprite;

            Vector2[] points = name switch
            {
                "star" => StarPoints(),
                "diamond" => new[] { P(0, 0.45f), P(0.34f, 0), P(0, -0.45f), P(-0.34f, 0) },
                "spark" => new[]
                {
                    P(0, 0.46f), P(0.11f, 0.11f), P(0.46f, 0), P(0.11f, -0.11f),
                    P(0, -0.46f), P(-0.11f, -0.11f), P(-0.46f, 0), P(-0.11f, 0.11f)
                },
                _ => null
            };
            sprite = Raster(64, 64, (x, y) =>
            {
                var p = new Vector2(x / 64 - 0.5f, y / 64 - 0.5f);
                float distance = points == null ? Stroke(name, p) : PolygonDistance(points, p);
                return Mathf.Clamp01(0.5f - distance * 64);
            });
            sprites.Add(name, sprite);
            return sprite;
        }

        private static Image Picture(RectTransform parent, string name, float x, float y, float w, float h,
            Color color, Sprite sprite, Image.Type type)
        {
            Image image = Place(parent, name, x, y, w, h).gameObject.AddComponent<Image>();
            image.sprite = sprite;
            image.type = type;
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        private static Sprite Raster(int width, int height, Func<float, float, float> alpha, int border = 0)
        {
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            texture.wrapMode = TextureWrapMode.Clamp;
            var pixels = new Color32[width * height];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    byte opacity = (byte)Mathf.RoundToInt(255 * alpha(x + 0.5f, y + 0.5f));
                    pixels[y * width + x] = new Color32(255, 255, 255, opacity);
                }
            }

            texture.SetPixels32(pixels);
            texture.Apply(false, true);
            return Sprite.Create(texture, new Rect(0, 0, width, height), Vector2.zero,
                100f, 0, SpriteMeshType.FullRect, new Vector4(border, border, border, border));
        }

        private static Font Load(string name) =>
            Resources.Load<Font>("RenkYolu/Fonts/" + name) ?? Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        private static Vector2 P(float x, float y) => new Vector2(x, y);

        private static float Stroke(string icon, Vector2 p)
        {
            float Line(float x1, float y1, float x2, float y2) =>
                SegmentDistance(p, P(x1, y1), P(x2, y2)) - 0.053f;

            float Arrow(Vector2 point) =>
                Mathf.Min(SegmentDistance(point, P(-0.30f, 0), P(0.29f, 0)),
                    Mathf.Min(SegmentDistance(point, P(0.08f, 0.21f), P(0.29f, 0)),
                        SegmentDistance(point, P(0.08f, -0.21f), P(0.29f, 0)))) - 0.055f;

            switch (icon)
            {
                case "right": return Arrow(p);
                case "left": return Arrow(-p);
                case "up": return Arrow(P(p.y, -p.x));
                case "down": return Arrow(P(-p.y, p.x));
                case "close": return Mathf.Min(Line(-.27f, -.27f, .27f, .27f), Line(-.27f, .27f, .27f, -.27f));
                case "pause": return Mathf.Min(Line(-.14f, -.29f, -.14f, .29f), Line(.14f, -.29f, .14f, .29f));
                case "spike":
                    return Mathf.Min(
                        Mathf.Min(Line(-.38f, -.23f, -.22f, .22f), Line(-.22f, .22f, 0, -.23f)),
                        Mathf.Min(Line(0, -.23f, .22f, .22f), Line(.22f, .22f, .38f, -.23f)));
                case "wall":
                    return Mathf.Min(Mathf.Min(Line(-.27f, -.29f, .29f, .27f), Line(-.32f, 0, 0, .32f)),
                        Line(0, -.32f, .32f, 0));
                case "undo":
                    return Mathf.Min(Mathf.Min(Line(-.31f, .05f, .24f, .05f), Line(-.31f, .05f, -.12f, .24f)),
                        Mathf.Min(Line(-.31f, .05f, -.12f, -.14f), Line(.24f, .05f, .24f, -.20f)));
                default: return p.magnitude - 0.13f;
            }
        }

        private static Vector2[] StarPoints()
        {
            var star = new Vector2[10];
            for (int i = 0; i < star.Length; i++)
            {
                float angle = Mathf.PI / 2 + i * Mathf.PI / 5;
                star[i] = P(Mathf.Cos(angle), Mathf.Sin(angle)) * (i % 2 == 0 ? 0.47f : 0.22f);
            }

            return star;
        }

        private static float PolygonDistance(Vector2[] points, Vector2 p)
        {
            bool inside = false;
            float distance = float.MaxValue;
            for (int i = 0; i < points.Length; i++)
            {
                Vector2 a = points[i];
                Vector2 b = points[(i + 1) % points.Length];
                distance = Mathf.Min(distance, SegmentDistance(p, a, b));
                if ((a.y > p.y) != (b.y > p.y) && p.x < (b.x - a.x) * (p.y - a.y) / (b.y - a.y) + a.x)
                    inside = !inside;
            }

            return inside ? -distance : distance;
        }

        private static float SegmentDistance(Vector2 p, Vector2 a, Vector2 b)
        {
            Vector2 edge = b - a;
            return (p - a - Mathf.Clamp01(Vector2.Dot(p - a, edge) / edge.sqrMagnitude) * edge).magnitude;
        }
    }
}
