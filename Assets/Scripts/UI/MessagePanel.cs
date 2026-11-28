using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace MoodieLand
{
    /// <summary>
    /// "My Little Message" preview. The player always decides what happens to the
    /// message: show it to Nunu, keep it private, or go back and edit it.
    /// </summary>
    [DisallowMultipleComponent]
    public class MessagePanel : MonoBehaviour
    {
        [Header("Wiring")]
        public CanvasGroup group;
        public Text titleText;
        public Text bodyText;
        public Text footerText;
        public Button shareButton;
        public Button privateButton;
        public Button changeButton;
        public RectTransform card;
        public Image cardBackground;

        Action onShare, onPrivate, onChange;
        Coroutine fade;

        public bool IsVisible => group != null && group.alpha > 0f;

        void Awake()
        {
            if (group != null)
            {
                group.alpha = 0f;
                group.blocksRaycasts = false;
            }
            if (shareButton != null) shareButton.onClick.AddListener(() => Pick(onShare, true));
            if (privateButton != null) privateButton.onClick.AddListener(() => Pick(onPrivate, false));
            if (changeButton != null) changeButton.onClick.AddListener(() => Pick(onChange, false));
        }

        void Pick(Action action, bool success)
        {
            if (success) AudioManager.I?.PlaySuccess();
            else AudioManager.I?.PlayClick();
            Hide();
            action?.Invoke();
        }

        public void Show(string message, Action share, Action keepPrivate, Action change, string title = "My Little Message")
        {
            onShare = share;
            onPrivate = keepPrivate;
            onChange = change;

            if (titleText != null) titleText.text = title;
            if (bodyText != null) bodyText.text = message;
            if (footerText != null) footerText.text = "You decide who sees this.";

            if (group != null)
            {
                group.blocksRaycasts = true;
                group.interactable = true;
                FadeTo(1f, 0.25f);
                if (card != null) StartCoroutine(UIFx.Pop(card, 0.88f, 1f, 0.4f));
            }
        }

        public void Hide()
        {
            if (group == null) return;
            group.blocksRaycasts = false;
            group.interactable = false;
            FadeTo(0f, 0.2f);
        }

        public void HideImmediate()
        {
            if (group == null) return;
            if (fade != null) StopCoroutine(fade);
            fade = null;
            group.alpha = 0f;
            group.blocksRaycasts = false;
            group.interactable = false;
        }

        void FadeTo(float alpha, float duration)
        {
            if (fade != null) StopCoroutine(fade);
            fade = StartCoroutine(UIFx.FadeGroup(group, group.alpha, alpha, duration));
        }
    }
}
