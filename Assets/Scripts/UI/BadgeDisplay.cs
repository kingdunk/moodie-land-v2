using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace MoodieLand
{
    /// <summary>Reward popup shown when a level is completed.</summary>
    [DisallowMultipleComponent]
    public class BadgeDisplay : MonoBehaviour
    {
        [Header("Wiring")]
        public CanvasGroup group;
        public Image dimmer;
        public RectTransform card;
        public Image badgeIcon;
        public Image glow;
        public Text titleText;
        public Text subtitleText;
        public Button continueButton;
        public Text continueLabel;

        [Header("Sprites")]
        public Sprite rainbowBadge;
        public Sprite starBadge;
        public Sprite moonBadge;

        Action onClose;

        void Awake()
        {
            if (group != null)
            {
                group.alpha = 0f;
                group.blocksRaycasts = false;
            }
            if (continueButton != null) continueButton.onClick.AddListener(Close);
        }

        public Sprite SpriteFor(BadgeId id)
        {
            switch (id)
            {
                case BadgeId.Rainbow: return rainbowBadge;
                case BadgeId.Star: return starBadge;
                case BadgeId.Moon: return moonBadge;
                default: return null;
            }
        }

        public void Show(BadgeId id, string title, string subtitle, Action closed, string buttonText = "Wonderful!")
        {
            onClose = closed;
            if (badgeIcon != null) badgeIcon.sprite = SpriteFor(id);
            if (titleText != null) titleText.text = title;
            if (subtitleText != null) subtitleText.text = subtitle;
            if (continueLabel != null) continueLabel.text = buttonText;

            if (continueButton != null) continueButton.gameObject.SetActive(true);
            gameObject.SetActive(true);
            StartCoroutine(Run());
        }

        IEnumerator Run()
        {
            if (group == null) yield break;
            group.blocksRaycasts = true;
            group.interactable = true;
            AudioManager.I?.PlayBadge();

            yield return UIFx.FadeGroup(group, group.alpha, 1f, 0.3f);

            if (card != null) yield return UIFx.Pop(card, 0.7f, 1f, 0.55f);
            if (badgeIcon != null) yield return UIFx.Pop(badgeIcon.transform, 0.2f, 1f, 0.6f);

            if (glow != null) StartCoroutine(GlowLoop());
        }

        IEnumerator GlowLoop()
        {
            float t = 0f;
            var c = glow.color;
            while (glow != null && group != null && group.alpha > 0.2f)
            {
                t += Time.unscaledDeltaTime;
                c.a = 0.25f + (Mathf.Sin(t * 2.2f) * 0.5f + 0.5f) * 0.35f;
                glow.color = c;
                float s = 1f + Mathf.Sin(t * 2.2f) * 0.04f;
                glow.transform.localScale = Vector3.one * s;
                yield return null;
            }
        }

        public void Close()
        {
            AudioManager.I?.PlayClick();
            StartCoroutine(CloseRoutine());
        }

        IEnumerator CloseRoutine()
        {
            if (group != null)
            {
                group.blocksRaycasts = false;
                group.interactable = false;
                yield return UIFx.FadeGroup(group, group.alpha, 0f, 0.25f);
            }
            var cb = onClose;
            onClose = null;
            cb?.Invoke();
        }
    }
}
