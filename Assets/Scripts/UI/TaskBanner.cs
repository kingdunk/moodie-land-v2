using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace MoodieLand
{
    /// <summary>"Task 1 · What's Happening?" banner that slides in between tasks.</summary>
    [DisallowMultipleComponent]
    public class TaskBanner : MonoBehaviour
    {
        public CanvasGroup group;
        public Text titleText;
        public Text subtitleText;
        public RectTransform panel;

        void Awake()
        {
            if (group != null)
            {
                group.alpha = 0f;
                group.blocksRaycasts = false;
            }
        }

        public IEnumerator Play(string title, string subtitle, float hold = 1.25f)
        {
            if (group == null) yield break;
            if (titleText != null) titleText.text = title;
            if (subtitleText != null) subtitleText.text = subtitle ?? "";

            group.blocksRaycasts = false;
            StartCoroutine(UIFx.FadeGroup(group, group.alpha, 1f, 0.22f));
            if (panel != null)
            {
                var rt = panel;
                Vector2 target = new Vector2(0f, rt.anchoredPosition.y);
                rt.anchoredPosition = target + new Vector2(-120f, 0f);
                StartCoroutine(UIFx.MoveAnchored(rt, target, 0.4f));
            }
            yield return new WaitForSecondsRealtime(hold);
            yield return UIFx.FadeGroup(group, 1f, 0f, 0.3f);
        }
    }
}
