using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace MoodieLand
{
    /// <summary>Small tween helpers. All feedback in the game is driven from here.</summary>
    public static class UIFx
    {
        public static float EaseOutCubic(float t) => 1f - Mathf.Pow(1f - Mathf.Clamp01(t), 3f);

        public static float EaseOutBack(float t)
        {
            t = Mathf.Clamp01(t);
            const float c1 = 1.70158f;
            const float c3 = c1 + 1f;
            return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
        }

        public static float SmoothStep(float t)
        {
            t = Mathf.Clamp01(t);
            return t * t * (3f - 2f * t);
        }

        public static IEnumerator Pop(Transform target, float from, float to, float duration, float delay = 0f)
        {
            if (target == null) yield break;
            if (delay > 0f) yield return new WaitForSecondsRealtime(delay);
            float t = 0f;
            target.localScale = Vector3.one * from;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                float k = EaseOutBack(t / duration);
                target.localScale = Vector3.one * Mathf.LerpUnclamped(from, to, k);
                yield return null;
            }
            target.localScale = Vector3.one * to;
        }

        public static IEnumerator Punch(Transform target, float amount = 0.08f, float duration = 0.25f)
        {
            if (target == null) yield break;
            Vector3 baseScale = target.localScale;
            float t = 0f;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                float k = t / duration;
                float s = 1f + Mathf.Sin(k * Mathf.PI) * amount;
                target.localScale = baseScale * s;
                yield return null;
            }
            target.localScale = baseScale;
        }

        public static IEnumerator FadeGroup(CanvasGroup group, float from, float to, float duration)
        {
            if (group == null) yield break;
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

        public static IEnumerator MoveAnchored(RectTransform rt, Vector2 to, float duration)
        {
            if (rt == null) yield break;
            Vector2 from = rt.anchoredPosition;
            float t = 0f;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                rt.anchoredPosition = Vector2.LerpUnclamped(from, to, EaseOutCubic(t / duration));
                yield return null;
            }
            rt.anchoredPosition = to;
        }

        public static IEnumerator SlideIn(RectTransform rt, float offsetY, float duration)
        {
            if (rt == null) yield break;
            Vector2 target = rt.anchoredPosition;
            rt.anchoredPosition = target + new Vector2(0f, offsetY);
            yield return MoveAnchored(rt, target, duration);
        }

        /// <summary>Typewriter reveal. Yields until finished; call Skip() to complete instantly.</summary>
        public static IEnumerator Typewriter(Text label, string content, float charsPerSecond, System.Action onDone = null)
        {
            if (label == null) yield break;
            label.text = "";
            float shown = 0f;
            int total = content.Length;
            while (shown < total)
            {
                shown += Mathf.Max(1f, charsPerSecond) * Time.unscaledDeltaTime;
                int count = Mathf.Clamp(Mathf.FloorToInt(shown), 0, total);
                label.text = content.Substring(0, count);
                yield return null;
            }
            label.text = content;
            onDone?.Invoke();
        }

        public static IEnumerator FloatLoop(RectTransform rt, float amplitude, float speed)
        {
            if (rt == null) yield break;
            Vector2 origin = rt.anchoredPosition;
            float t = Random.Range(0f, 10f);
            while (rt != null)
            {
                t += Time.unscaledDeltaTime * speed;
                rt.anchoredPosition = origin + new Vector2(0f, Mathf.Sin(t) * amplitude);
                yield return null;
            }
        }

        public static IEnumerator Shake(RectTransform rt, float strength, float duration)
        {
            if (rt == null) yield break;
            Vector2 origin = rt.anchoredPosition;
            float t = 0f;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                float k = 1f - t / duration;
                rt.anchoredPosition = origin + new Vector2(
                    Mathf.Sin(t * 62f) * strength * k,
                    Mathf.Cos(t * 78f) * strength * k * 0.6f);
                yield return null;
            }
            rt.anchoredPosition = origin;
        }

        public static IEnumerator FadeGraphic(Graphic g, float from, float to, float duration)
        {
            if (g == null) yield break;
            float t = 0f;
            var c = g.color;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                c.a = Mathf.Lerp(from, to, Mathf.Clamp01(t / duration));
                g.color = c;
                yield return null;
            }
            c.a = to;
            g.color = c;
        }
    }
}
