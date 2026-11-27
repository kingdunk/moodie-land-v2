using System;
using UnityEngine;
using UnityEngine.UI;

namespace MoodieLand
{
    /// <summary>
    /// The Hint button and its bubble. Hints can be replayed as often as the
    /// player likes (accessibility requirement) and each use is logged.
    /// </summary>
    [DisallowMultipleComponent]
    public class HintController : MonoBehaviour
    {
        [Header("Wiring")]
        public Button hintButton;
        public Text hintButtonLabel;
        public CanvasGroup bubble;
        public Text bubbleText;
        public Button bubbleClose;

        [Header("Behaviour")]
        public float autoHideAfter = 9f;

        public event Action Used;
        public int UseCount { get; private set; }

        string currentHint = "";
        Coroutine autoHide;

        void Awake()
        {
            if (bubble != null)
            {
                bubble.alpha = 0f;
                bubble.blocksRaycasts = false;
                bubble.interactable = false;
            }
            if (hintButton != null) hintButton.onClick.AddListener(ShowHint);
            if (bubbleClose != null) bubbleClose.onClick.AddListener(HideBubble);

            // nothing to hint at until a level sets one
            if (hintButton != null) hintButton.gameObject.SetActive(false);
        }

        public void SetHint(string text)
        {
            currentHint = text ?? "";
            if (hintButton != null) hintButton.gameObject.SetActive(!string.IsNullOrEmpty(currentHint));
            HideBubble();
        }

        public void ShowHint()
        {
            if (string.IsNullOrEmpty(currentHint)) return;
            AudioManager.I?.PlayPop();
            UseCount++;
            Used?.Invoke();

            if (bubbleText != null) bubbleText.text = currentHint;
            if (bubble != null)
            {
                StopAllCoroutines();
                bubble.blocksRaycasts = true;
                bubble.interactable = true;
                StartCoroutine(UIFx.FadeGroup(bubble, bubble.alpha, 1f, 0.18f));
                StartCoroutine(UIFx.Pop(bubble.transform, 0.9f, 1f, 0.25f));
            }

            if (autoHide != null) StopCoroutine(autoHide);
            autoHide = StartCoroutine(AutoHide());
        }

        System.Collections.IEnumerator AutoHide()
        {
            yield return new WaitForSecondsRealtime(autoHideAfter);
            HideBubble();
        }

        public void HideBubble()
        {
            if (bubble == null) return;
            // the whole hint group can be switched off (e.g. on the ending screen)
            if (!gameObject.activeInHierarchy)
            {
                bubble.alpha = 0f;
                bubble.blocksRaycasts = false;
                bubble.interactable = false;
                return;
            }

            StopAllCoroutines();
            bubble.blocksRaycasts = false;
            bubble.interactable = false;
            StartCoroutine(UIFx.FadeGroup(bubble, bubble.alpha, 0f, 0.15f));
        }

        public void SetVisible(bool visible)
        {
            if (hintButton != null) hintButton.gameObject.SetActive(visible && !string.IsNullOrEmpty(currentHint));
            if (!visible) HideBubble();
        }
    }
}
