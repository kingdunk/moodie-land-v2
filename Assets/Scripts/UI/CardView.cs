using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MoodieLand
{
    [Serializable]
    public class CardData
    {
        public string id;
        public string label;
        public Sprite icon;
        public Color tint = Color.white;

        public CardData() { }

        public CardData(string id, string label, Sprite icon)
        {
            this.id = id;
            this.label = label;
            this.icon = icon;
        }
    }

    /// <summary>
    /// One picture choice card. Used by every level: emotion cards,
    /// communication cards, reason cards and need cards.
    /// </summary>
    [DisallowMultipleComponent]
    public class CardView : MonoBehaviour
    {
        [Header("Wiring")]
        public Button button;
        public Image background;
        public Image icon;
        public Text label;
        public Image selectionFrame;
        public GameObject checkMark;
        public Image shadow;

        [Header("Look")]
        public Color normalColor = new Color32(0xFF, 0xFC, 0xF6, 0xFF);
        public Color selectedColor = new Color32(0xE8, 0xF7, 0xF2, 0xFF);
        public Color dimColor = new Color32(0xF2, 0xEC, 0xE4, 0xFF);

        public CardData Data { get; private set; }
        public bool IsSelected { get; private set; }
        public int Index { get; private set; }

        public event Action<CardView> Clicked;

        RectTransform rt;

        void Awake()
        {
            rt = (RectTransform)transform;
            if (button == null) button = GetComponent<Button>();
            if (button != null) button.onClick.AddListener(() => Clicked?.Invoke(this));
            SetSelected(false);
        }

        public void Bind(CardData data, int index)
        {
            Data = data;
            Index = index;
            if (background == null) background = GetComponent<Image>();
            if (icon != null)
            {
                icon.sprite = data.icon;
                icon.enabled = data.icon != null;
                icon.color = data.tint;
            }
            if (label != null)
            {
                label.text = data.label;
                label.color = MoodieUI.Ink;
            }
            gameObject.SetActive(true);
            SetSelected(false);
        }

        public void SetSelected(bool selected)
        {
            IsSelected = selected;
            if (selectionFrame != null) selectionFrame.gameObject.SetActive(selected);
            if (checkMark != null) checkMark.SetActive(selected);
            if (background != null) background.color = selected ? selectedColor : normalColor;
            if (icon != null) icon.transform.localScale = Vector3.one * (selected ? 1.06f : 1f);
        }

        public void SetDim(bool dim)
        {
            if (background != null) background.color = dim ? dimColor : (IsSelected ? selectedColor : normalColor);
        }

        public System.Collections.IEnumerator PopIn(float delay)
        {
            yield return UIFx.Pop(transform, 0.82f, 1f, 0.26f, delay);
        }

        public void Shake() => StartCoroutine(UIFx.Shake(rt, 8f, 0.28f));
    }
}
