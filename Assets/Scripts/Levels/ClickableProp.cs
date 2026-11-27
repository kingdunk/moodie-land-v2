using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MoodieLand
{
    /// <summary>
    /// Any clickable object in the scenery: pinecone basket, clue cards,
    /// building blocks, map entrances.
    /// </summary>
    [DisallowMultipleComponent]
    public class ClickableProp : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("Wiring")]
        public Button button;
        public Image image;
        public Image highlight;
        public Text caption;
        public GameObject badgeSlot;

        [Header("State")]
        public bool interactable = true;
        public bool clickedOnce;
        [Tooltip("Base tint applied while interactable; dimmed when disabled.")]
        public Color normalTint = Color.white;

        public event Action<ClickableProp> Clicked;

        Vector3 baseScale = Vector3.one;
        bool hovered;

        void Awake()
        {
            if (button == null) button = GetComponent<Button>();
            if (image == null) image = GetComponent<Image>();
            baseScale = transform.localScale;

            if (button != null) button.onClick.AddListener(() =>
            {
                if (!interactable) return;
                AudioManager.I?.PlaySelect();
                clickedOnce = true;
                Clicked?.Invoke(this);
            });

            if (highlight != null) highlight.gameObject.SetActive(false);
        }

        void Update()
        {
            float target = hovered && interactable ? 1.05f : (interactable ? 1f : 0.96f);
            transform.localScale = Vector3.Lerp(transform.localScale, baseScale * target, Time.unscaledDeltaTime * 12f);
            if (image != null)
                image.color = interactable
                    ? normalTint
                    : new Color(normalTint.r, normalTint.g, normalTint.b, normalTint.a * 0.7f);
        }

        public void OnPointerEnter(PointerEventData e) => hovered = true;
        public void OnPointerExit(PointerEventData e) => hovered = false;

        public void SetHighlighted(bool on)
        {
            if (highlight != null) highlight.gameObject.SetActive(on);
        }

        public void SetVisible(bool visible) => gameObject.SetActive(visible);

        public void SetInteractable(bool value) => interactable = value;

        public void SetSprite(Sprite sprite)
        {
            if (image != null) image.sprite = sprite;
        }

        public void SetCaption(string text)
        {
            if (caption == null) return;
            caption.text = text ?? "";
            caption.gameObject.SetActive(!string.IsNullOrEmpty(text));
        }

        public void Punp() => StartCoroutine(UIFx.Punch(transform, 0.12f, 0.3f));
    }
}
