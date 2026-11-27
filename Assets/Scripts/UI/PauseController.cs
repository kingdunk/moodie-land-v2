using UnityEngine;
using UnityEngine.UI;

namespace MoodieLand
{
    /// <summary>
    /// Pause menu: freezes time, offers resume / restart / settings /
    /// back to harbour / main menu. Escape also works (new Input System only).
    /// </summary>
    [DisallowMultipleComponent]
    public class PauseController : MonoBehaviour
    {
        [Header("Wiring")]
        public Button pauseButton;
        public CanvasGroup group;
        public Button resumeButton;
        public Button restartButton;
        public Button settingsButton;
        public Button harborButton;
        public Button menuButton;
        public SettingsPanel settingsPanel;
        public GameObject restartRoot;

        [Header("Behaviour")]
        public bool allowRestart = true;

        public bool IsPaused { get; private set; }

        void Awake()
        {
            if (group != null)
            {
                group.alpha = 0f;
                group.blocksRaycasts = false;
            }

            if (pauseButton != null) pauseButton.onClick.AddListener(Pause);
            if (resumeButton != null) resumeButton.onClick.AddListener(Resume);
            if (settingsButton != null) settingsButton.onClick.AddListener(OpenSettings);
            if (harborButton != null) harborButton.onClick.AddListener(() => { Resume(); GameManager.I?.GoHarbor(); });
            if (menuButton != null) menuButton.onClick.AddListener(() => { Resume(); GameManager.I?.GoMainMenu(); });
            if (restartButton != null) restartButton.onClick.AddListener(() => { Resume(); GameManager.I?.RestartCurrentScene(); });

            if (restartRoot != null) restartRoot.SetActive(allowRestart);
        }

        void Update()
        {
            var kb = UnityEngine.InputSystem.Keyboard.current;
            if (kb != null && kb.escapeKey.wasPressedThisFrame)
            {
                if (IsPaused) Resume();
                else Pause();
            }
        }

        public void Pause()
        {
            if (IsPaused) return;
            IsPaused = true;
            Time.timeScale = 0f;
            AudioManager.I?.PlayClick();

            if (group != null)
            {
                group.blocksRaycasts = true;
                group.interactable = true;
                StartCoroutine(UIFx.FadeGroup(group, group.alpha, 1f, 0.18f));
                StartCoroutine(UIFx.Pop(group.transform, 0.95f, 1f, 0.22f));
            }
        }

        public void Resume()
        {
            if (!IsPaused) return;
            IsPaused = false;
            Time.timeScale = 1f;
            AudioManager.I?.PlayClick();

            if (group != null)
            {
                group.blocksRaycasts = false;
                group.interactable = false;
                StartCoroutine(UIFx.FadeGroup(group, group.alpha, 0f, 0.15f));
            }
        }

        void OpenSettings()
        {
            AudioManager.I?.PlayClick();
            if (settingsPanel != null) settingsPanel.Open();
        }
    }
}
