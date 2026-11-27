using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace MoodieLand
{
    /// <summary>Soft feedback banner used for praise and gentle corrections.</summary>
    [DisallowMultipleComponent]
    public class ToastController : MonoBehaviour
    {
        [Header("Wiring")]
        public CanvasGroup group;
        public Text label;
        public Image background;
        public RectTransform bubble;

        [Header("Look")]
        public Color praiseColor = new Color32(0xE7, 0xF6, 0xF1, 0xF2);
        public Color gentleColor = new Color32(0xFF, 0xF3, 0xE2, 0xF2);
        public Color infoColor = new Color32(0xED, 0xEB, 0xF8, 0xF2);

        public enum Tone { Praise, Gentle, Info }

        Coroutine routine;

        void Awake()
        {
            if (group != null)
            {
                group.alpha = 0f;
                group.blocksRaycasts = false;
            }
        }

        public void Show(string message, Tone tone = Tone.Info, float duration = 2.2f, string icon = null)
        {
            if (label == null) return;
            label.text = message;
            if (background != null)
            {
                background.color = tone == Tone.Praise ? praiseColor
                                  : tone == Tone.Gentle ? gentleColor
                                  : infoColor;
            }
            if (routine != null) StopCoroutine(routine);
            routine = StartCoroutine(Run(duration));
        }

        IEnumerator Run(float duration)
        {
            if (group == null) yield break;
            group.blocksRaycasts = false;
            float from = group.alpha;
            yield return UIFx.FadeGroup(group, from, 1f, 0.16f);
            if (bubble != null) yield return UIFx.Pop(bubble, 0.88f, 1f, 0.22f);
            yield return new WaitForSecondsRealtime(duration);
            yield return UIFx.FadeGroup(group, 1f, 0f, 0.3f);
            routine = null;
        }

        public void HideNow()
        {
            if (routine != null) StopCoroutine(routine);
            if (group != null) group.alpha = 0f;
        }
    }
}
