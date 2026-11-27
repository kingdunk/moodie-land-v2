using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace MoodieLand
{
    /// <summary>
    /// Title screen: start a new adventure, continue, settings, quit.
    /// "Begin" always starts from a clean save so a second playthrough really
    /// starts over; "Continue" resumes the saved one.
    /// </summary>
    public class MainMenuController : MonoBehaviour
    {
        [Header("Wiring")]
        public Button playButton;
        public Button continueButton;
        public Button settingsButton;
        public Button quitButton;
        public SettingsPanel settingsPanel;
        public Text newGameNote;
        public Image[] badgeIcons = new Image[3];
        public Sprite[] badgeSprites = new Sprite[3];
        public CharacterView[] floaters = new CharacterView[0];
        public RectTransform titleCard;
        public Text subtitleText;

        void Start()
        {
            bool hasProgress = ProgressManager.I != null && ProgressManager.I.AnyCompleted;

            if (playButton != null) playButton.onClick.AddListener(() =>
            {
                AudioManager.I?.PlayWhoosh();
                // a new journey clears badges, the message and the harbour intro flag
                ProgressManager.I?.ResetAll();
                GameManager.I?.GoStoryIntro();
            });

            if (continueButton != null)
            {
                continueButton.gameObject.SetActive(hasProgress);
                continueButton.onClick.AddListener(() =>
                {
                    AudioManager.I?.PlayWhoosh();
                    GameManager.I?.GoHarbor();
                });
            }

            if (settingsButton != null) settingsButton.onClick.AddListener(() =>
            {
                AudioManager.I?.PlayClick();
                settingsPanel?.Open();
            });

            if (quitButton != null) quitButton.onClick.AddListener(() =>
            {
                AudioManager.I?.PlayClick();
                GameManager.I?.QuitGame();
            });

            if (subtitleText != null && hasProgress)
                subtitleText.text = "Welcome back, explorer.";

            if (newGameNote != null)
                newGameNote.gameObject.SetActive(hasProgress);

            RefreshBadges();

            if (titleCard != null) StartCoroutine(UIFx.Pop(titleCard, 0.9f, 1f, 0.7f));

            for (int i = 0; i < floaters.Length; i++)
            {
                if (floaters[i] == null) continue;
                StartCoroutine(UIFx.FloatLoop((RectTransform)floaters[i].transform, 10f + i * 3f, 0.8f + i * 0.15f));
            }
        }

        void RefreshBadges()
        {
            if (badgeIcons == null) return;
            for (int i = 0; i < badgeIcons.Length; i++)
            {
                if (badgeIcons[i] == null) continue;
                bool owned = ProgressManager.I != null && ProgressManager.I.IsCompleted(i);
                badgeIcons[i].gameObject.SetActive(owned);
                if (owned && badgeSprites != null && i < badgeSprites.Length)
                    badgeIcons[i].sprite = badgeSprites[i];
            }
        }
    }
}
