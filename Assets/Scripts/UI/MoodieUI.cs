using UnityEngine;
using UnityEngine.UI;

namespace MoodieLand
{
    /// <summary>Shared font / palette access so every screen looks the same.</summary>
    public static class MoodieUI
    {
        static Font body;

        public static Font Body
        {
            get
            {
                if (body == null)
                {
                    body = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                    if (body == null) body = Resources.GetBuiltinResource<Font>("Arial.ttf");
                }
                return body;
            }
        }

        public static readonly Color Ink = new Color32(0x4A, 0x40, 0x38, 0xFF);
        public static readonly Color InkSoft = new Color32(0x7A, 0x6E, 0x64, 0xFF);
        public static readonly Color Cream = new Color32(0xFF, 0xF6, 0xE9, 0xFF);
        public static readonly Color Paper = new Color32(0xFF, 0xFC, 0xF6, 0xFF);
        public static readonly Color Teal = new Color32(0x39, 0x9C, 0x91, 0xFF);
        public static readonly Color Peach = new Color32(0xFF, 0xC7, 0xA8, 0xFF);
        public static readonly Color Pink = new Color32(0xF7, 0xA8, 0xB2, 0xFF);
        public static readonly Color Lavender = new Color32(0xC7, 0xCE, 0xEA, 0xFF);
        public static readonly Color Mint = new Color32(0xA8, 0xE6, 0xCF, 0xFF);
        public static readonly Color Sun = new Color32(0xFF, 0xE0, 0x8A, 0xFF);
        public static readonly Color Shadow = new Color32(0x5C, 0x4C, 0x43, 0x33);

        public static Text MakeText(Transform parent, string name, string content, int size, Color color,
            TextAnchor anchor = TextAnchor.UpperLeft, FontStyle style = FontStyle.Normal)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            go.transform.SetParent(parent, false);
            var t = go.GetComponent<Text>();
            t.font = Body;
            t.text = content;
            t.fontSize = size;
            t.color = color;
            t.alignment = anchor;
            t.fontStyle = style;
            t.horizontalOverflow = HorizontalWrapMode.Wrap;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            t.supportRichText = true;
            t.raycastTarget = false;
            t.lineSpacing = 1.12f;
            return t;
        }

        public static Image MakeImage(Transform parent, string name, Sprite sprite, Color color,
            bool preserveAspect = true)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(parent, false);
            var img = go.GetComponent<Image>();
            img.sprite = sprite;
            img.color = color;
            img.preserveAspect = preserveAspect;
            img.raycastTarget = false;
            return img;
        }

        public static Image MakePanel(Transform parent, string name, Sprite sprite, Color color)
        {
            var img = MakeImage(parent, name, sprite, color, false);
            img.type = Image.Type.Sliced;
            return img;
        }

        public static void Stretch(RectTransform rt, float left = 0, float bottom = 0, float right = 0, float top = 0)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(left, bottom);
            rt.offsetMax = new Vector2(-right, -top);
        }

        public static RectTransform Rect(Transform parent, string name, Vector2 pivot, Vector2 anchoredPos, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.pivot = pivot;
            rt.anchorMin = pivot;
            rt.anchorMax = pivot;
            rt.sizeDelta = size;
            rt.anchoredPosition = anchoredPos;
            return rt;
        }

        public static Button MakeButton(Transform parent, string name, Sprite bg, Color color, string label,
            int fontSize, Color textColor, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = size;

            var img = go.GetComponent<Image>();
            img.sprite = bg;
            img.type = Image.Type.Sliced;
            img.color = color;

            var btn = go.GetComponent<Button>();
            btn.targetGraphic = img;
            var colors = btn.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1f, 1f, 1f, 0.94f);
            colors.pressedColor = new Color(0.92f, 0.92f, 0.92f, 1f);
            colors.selectedColor = Color.white;
            colors.fadeDuration = 0.08f;
            btn.colors = colors;

            var text = MakeText(go.transform, "Label", label, fontSize, textColor, TextAnchor.MiddleCenter, FontStyle.Bold);
            Stretch(text.rectTransform, 12, 4, 12, 4);

            return btn;
        }
    }
}
