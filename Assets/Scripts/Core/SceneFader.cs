using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace MoodieLand
{
    /// <summary>
    /// Full screen colour wipe used for every scene transition.
    /// Lives under the persistent GameRoot so it survives scene loads.
    /// </summary>
    public class SceneFader : MonoBehaviour
    {
        public CanvasGroup group;
        public Image cover;

        void Awake()
        {
            if (group == null) group = GetComponent<CanvasGroup>();
            group.blocksRaycasts = false;
            group.interactable = false;
            group.alpha = 0f;
        }

        public IEnumerator FadeOut(float duration)
        {
            group.blocksRaycasts = true;
            yield return Fade(0f, 1f, duration);
        }

        public IEnumerator FadeIn(float duration)
        {
            yield return Fade(1f, 0f, duration);
            group.blocksRaycasts = false;
        }

        IEnumerator Fade(float from, float to, float duration)
        {
            float t = 0f;
            group.alpha = from;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                group.alpha = Mathf.Lerp(from, to, Mathf.Clamp01(t / duration));
                yield return null;
            }
            group.alpha = to;
        }

        /// <summary>Creates a runtime fader (fallback when a scene has none wired).</summary>
        public static SceneFader Create(Transform parent)
        {
            var go = new GameObject("SceneFader", typeof(RectTransform), typeof(Canvas), typeof(CanvasGroup), typeof(SceneFader));
            go.transform.SetParent(parent, false);

            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 900;

            var img = new GameObject("Cover", typeof(RectTransform), typeof(Image)).GetComponent<Image>();
            img.transform.SetParent(go.transform, false);
            var rt = img.rectTransform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            img.color = new Color(0.24f, 0.22f, 0.28f, 1f);
            img.raycastTarget = false;

            var fader = go.GetComponent<SceneFader>();
            fader.group = go.GetComponent<CanvasGroup>();
            fader.cover = img;
            fader.group.alpha = 0f;
            return fader;
        }
    }
}
