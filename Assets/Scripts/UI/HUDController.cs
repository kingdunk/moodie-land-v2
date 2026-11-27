using UnityEngine;
using UnityEngine.UI;

namespace MoodieLand
{
    /// <summary>Top bar: level name, task progress dots, back and pause.</summary>
    [DisallowMultipleComponent]
    public class HUDController : MonoBehaviour
    {
        [Header("Wiring")]
        public Text levelTitle;
        public Text taskLabel;
        public Image[] progressDots;
        public Button backButton;
        public Button pauseButton;
        public CanvasGroup group;

        [Header("Dot colours")]
        public Color dotDone = new Color32(0x39, 0x9C, 0x91, 0xFF);
        public Color dotActive = new Color32(0xFF, 0xC7, 0xA8, 0xFF);
        public Color dotTodo = new Color32(0xFF, 0xFF, 0xFF, 0x8C);

        public void SetTitle(string title)
        {
            if (levelTitle != null) levelTitle.text = title;
        }

        public void SetTask(int index, int total, string label)
        {
            if (taskLabel != null)
                taskLabel.text = label;

            if (progressDots == null) return;
            for (int i = 0; i < progressDots.Length; i++)
            {
                if (progressDots[i] == null) continue;
                progressDots[i].gameObject.SetActive(i < total);
                if (i >= total) continue;
                progressDots[i].color = i < index ? dotDone : (i == index ? dotActive : dotTodo);
                progressDots[i].rectTransform.localScale = Vector3.one * (i == index ? 1.15f : 1f);
            }
        }

        public void SetBackAction(UnityEngine.Events.UnityAction action)
        {
            if (backButton == null) return;
            backButton.onClick.RemoveAllListeners();
            backButton.onClick.AddListener(action);
        }

        public void SetVisible(bool visible)
        {
            if (group != null)
            {
                group.alpha = visible ? 1f : 0f;
                group.blocksRaycasts = visible;
            }
        }
    }
}
