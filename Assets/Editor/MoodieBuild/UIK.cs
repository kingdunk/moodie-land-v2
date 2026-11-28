using UnityEngine;
using UnityEngine.UI;

namespace MoodieLand.EditorTools
{
    /// <summary>Small helpers so the scene builder stays readable.</summary>
    public static class UIK
    {
        public static readonly Vector2 BC = new Vector2(0.5f, 0f);
        public static readonly Vector2 TC = new Vector2(0.5f, 1f);
        public static readonly Vector2 CC = new Vector2(0.5f, 0.5f);
        public static readonly Vector2 TL = new Vector2(0f, 1f);
        public static readonly Vector2 TR = new Vector2(1f, 1f);
        public static readonly Vector2 BL = new Vector2(0f, 0f);
        public static readonly Vector2 BR = new Vector2(1f, 0f);
        public static readonly Vector2 ML = new Vector2(0f, 0.5f);

        public static RectTransform Node(Transform parent, string name, Vector2 anchor, Vector2 pos, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchor;
            rt.anchorMax = anchor;
            rt.pivot = anchor;
            rt.sizeDelta = size;
            rt.anchoredPosition = pos;
            return rt;
        }

        public static RectTransform Center(Transform parent, string name, Vector2 pos, Vector2 size)
        {
            var rt = Node(parent, name, CC, pos, size);
            rt.pivot = CC;
            return rt;
        }

        public static RectTransform Full(Transform parent, string name)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            return rt;
        }

        public static Image Img(Transform parent, string name, Sprite sprite, Color color,
            Vector2 anchor, Vector2 pos, Vector2 size, bool sliced = false, bool preserve = false)
        {
            var rt = Node(parent, name, anchor, pos, size);
            var img = rt.gameObject.AddComponent<Image>();
            img.sprite = sprite;
            img.color = color;
            img.type = sliced ? Image.Type.Sliced : Image.Type.Simple;
            img.preserveAspect = preserve;
            img.raycastTarget = false;
            return img;
        }

        public static Image CenterImg(Transform parent, string name, Sprite sprite, Color color, Vector2 pos, Vector2 size, bool preserve = false)
        {
            var rt = Center(parent, name, pos, size);
            var img = rt.gameObject.AddComponent<Image>();
            img.sprite = sprite;
            img.color = color;
            img.preserveAspect = preserve;
            img.raycastTarget = false;
            return img;
        }

        public static Image FullImg(Transform parent, string name, Sprite sprite, Color color, bool sliced = false)
        {
            var rt = Full(parent, name);
            var img = rt.gameObject.AddComponent<Image>();
            img.sprite = sprite;
            img.color = color;
            img.type = sliced ? Image.Type.Sliced : Image.Type.Simple;
            img.raycastTarget = false;
            return img;
        }

        public static Text Label(Transform parent, string name, string text, int size, Color color,
            TextAnchor align, Vector2 anchor, Vector2 pos, Vector2 dim, bool bold = false)
        {
            var rt = Node(parent, name, anchor, pos, dim);
            var t = rt.gameObject.AddComponent<Text>();
            t.font = MoodieUI.Body;
            t.text = text;
            t.fontSize = size;
            t.color = color;
            t.alignment = align;
            t.fontStyle = bold ? FontStyle.Bold : FontStyle.Normal;
            t.horizontalOverflow = HorizontalWrapMode.Wrap;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            t.supportRichText = true;
            t.lineSpacing = 1.12f;
            t.raycastTarget = false;
            return t;
        }

        public static Text CenterLabel(Transform parent, string name, string text, int size, Color color,
            Vector2 pos, Vector2 dim, bool bold = false)
        {
            return Label(parent, name, text, size, color, TextAnchor.MiddleCenter, CC, pos, dim, bold);
        }

        public static Button MakeButton(Transform parent, string name, string label, Sprite bg, Color bgColor,
            Color textColor, int fontSize, Vector2 anchor, Vector2 pos, Vector2 size)
        {
            var rt = Node(parent, name, anchor, pos, size);
            var img = rt.gameObject.AddComponent<Image>();
            img.sprite = bg;
            img.type = Image.Type.Sliced;
            img.color = bgColor;

            var btn = rt.gameObject.AddComponent<Button>();
            btn.targetGraphic = img;
            var colors = btn.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(0.94f, 0.97f, 0.94f, 1f);
            colors.pressedColor = new Color(0.86f, 0.89f, 0.86f, 1f);
            colors.selectedColor = colors.highlightedColor;
            colors.disabledColor = new Color(0.82f, 0.84f, 0.82f, 0.88f);
            colors.fadeDuration = 0.08f;
            btn.colors = colors;

            var text = Label(rt, "Label", label, fontSize, textColor, TextAnchor.MiddleCenter, CC, Vector2.zero, size, true);
            text.rectTransform.anchorMin = Vector2.zero;
            text.rectTransform.anchorMax = Vector2.one;
            text.rectTransform.offsetMin = new Vector2(14f, 6f);
            text.rectTransform.offsetMax = new Vector2(-14f, -6f);

            return btn;
        }

        public static Slider MakeSlider(Transform parent, string name, Vector2 anchor, Vector2 pos, Vector2 size,
            Color trackColor, Color fillColor, Sprite handleSprite)
        {
            var rt = Node(parent, name, anchor, pos, size);
            var slider = rt.gameObject.AddComponent<Slider>();

            var bg = Node(rt, "Background", CC, Vector2.zero, new Vector2(size.x, 20f));
            bg.anchorMin = new Vector2(0f, 0.5f);
            bg.anchorMax = new Vector2(1f, 0.5f);
            bg.pivot = new Vector2(0.5f, 0.5f);
            bg.sizeDelta = new Vector2(0f, 20f);
            bg.anchoredPosition = Vector2.zero;
            var bgImg = bg.gameObject.AddComponent<Image>();
            bgImg.color = trackColor;
            bgImg.raycastTarget = false;

            var fillArea = Node(rt, "Fill Area", CC, Vector2.zero, size);
            fillArea.anchorMin = new Vector2(0f, 0.5f);
            fillArea.anchorMax = new Vector2(1f, 0.5f);
            fillArea.offsetMin = new Vector2(6f, -10f);
            fillArea.offsetMax = new Vector2(-6f, 10f);

            var fill = Node(fillArea, "Fill", CC, Vector2.zero, size);
            fill.anchorMin = Vector2.zero;
            fill.anchorMax = new Vector2(1f, 1f);
            fill.offsetMin = Vector2.zero;
            fill.offsetMax = Vector2.zero;
            var fillImg = fill.gameObject.AddComponent<Image>();
            fillImg.color = fillColor;
            fillImg.raycastTarget = false;

            var handleArea = Node(rt, "Handle Slide Area", CC, Vector2.zero, size);
            handleArea.anchorMin = Vector2.zero;
            handleArea.anchorMax = Vector2.one;
            handleArea.offsetMin = new Vector2(12f, 0f);
            handleArea.offsetMax = new Vector2(-12f, 0f);

            var handle = Center(handleArea, "Handle", Vector2.zero, new Vector2(44f, 44f));
            var handleImg = handle.gameObject.AddComponent<Image>();
            handleImg.sprite = handleSprite;
            handleImg.color = new Color32(0x2C, 0x7A, 0x72, 0xFF);
            handleImg.raycastTarget = true;

            slider.fillRect = fill;
            slider.handleRect = handle;
            slider.targetGraphic = handleImg;
            slider.direction = Slider.Direction.LeftToRight;
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value = 0.7f;

            return slider;
        }

        public static CanvasGroup Group(GameObject go, float alpha, bool blocks)
        {
            var g = go.GetComponent<CanvasGroup>();
            if (g == null) g = go.AddComponent<CanvasGroup>();
            g.alpha = alpha;
            g.blocksRaycasts = blocks;
            g.interactable = blocks;
            return g;
        }

        public static T Comp<T>(GameObject go) where T : Component
        {
            var c = go.GetComponent<T>();
            return c != null ? c : go.AddComponent<T>();
        }
    }
}
