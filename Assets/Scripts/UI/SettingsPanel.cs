using System;
using UnityEngine;
using UnityEngine.UI;

namespace MoodieLand
{
    /// <summary>Volume + text speed settings, shared by the pause menu and main menu.</summary>
    [DisallowMultipleComponent]
    public class SettingsPanel : MonoBehaviour
    {
        [Header("Wiring")]
        public CanvasGroup group;
        public Slider musicSlider;
        public Slider sfxSlider;
        public Slider textSpeedSlider;
        public Text musicValue;
        public Text sfxValue;
        public Text textSpeedValue;
        public Button closeButton;
        public Button resetProgressButton;
        public Button confirmResetButton;
        public GameObject resetConfirm;

        public event Action Closed;

        bool suppress;

        void Awake()
        {
            if (group != null)
            {
                group.alpha = 0f;
                group.blocksRaycasts = false;
            }

            if (musicSlider != null) musicSlider.onValueChanged.AddListener(OnMusic);
            if (sfxSlider != null) sfxSlider.onValueChanged.AddListener(OnSfx);
            if (textSpeedSlider != null) textSpeedSlider.onValueChanged.AddListener(OnTextSpeed);
            if (closeButton != null) closeButton.onClick.AddListener(Close);
            if (resetProgressButton != null) resetProgressButton.onClick.AddListener(AskReset);
            if (confirmResetButton != null) confirmResetButton.onClick.AddListener(DoReset);
            if (resetConfirm != null) resetConfirm.SetActive(false);
        }

        public void Open()
        {
            suppress = true;
            if (musicSlider != null) musicSlider.value = AudioManager.I != null ? AudioManager.I.musicVolume : 0.35f;
            if (sfxSlider != null) sfxSlider.value = AudioManager.I != null ? AudioManager.I.sfxVolume : 0.7f;
            if (textSpeedSlider != null)
                textSpeedSlider.value = PlayerPrefs.GetFloat(DialogueController.TextSpeedKey, 48f);
            suppress = false;

            RefreshLabels();
            if (resetConfirm != null) resetConfirm.SetActive(false);

            if (group != null)
            {
                group.blocksRaycasts = true;
                group.interactable = true;
                StartCoroutine(UIFx.FadeGroup(group, group.alpha, 1f, 0.2f));
                StartCoroutine(UIFx.Pop(group.transform, 0.94f, 1f, 0.25f));
            }
        }

        public void Close()
        {
            AudioManager.I?.PlayClick();
            if (group != null)
            {
                group.blocksRaycasts = false;
                group.interactable = false;
                StartCoroutine(UIFx.FadeGroup(group, group.alpha, 0f, 0.16f));
            }
            Closed?.Invoke();
        }

        void OnMusic(float v)
        {
            if (AudioManager.I != null) AudioManager.I.SetMusicVolume(v);
            if (!suppress) RefreshLabels();
        }

        void OnSfx(float v)
        {
            if (AudioManager.I != null) AudioManager.I.SetSfxVolume(v);
            if (!suppress) RefreshLabels();
        }

        void OnTextSpeed(float v)
        {
            PlayerPrefs.SetFloat(DialogueController.TextSpeedKey, v);
            var boxes = FindObjectsByType<DialogueController>();
            foreach (var box in boxes) box.ApplyTextSpeed(v);
            if (!suppress) RefreshLabels();
        }

        void RefreshLabels()
        {
            if (musicValue != null && musicSlider != null)
                musicValue.text = Mathf.RoundToInt(musicSlider.value * 100f) + "%";
            if (sfxValue != null && sfxSlider != null)
                sfxValue.text = Mathf.RoundToInt(sfxSlider.value * 100f) + "%";
            if (textSpeedValue != null && textSpeedSlider != null)
                textSpeedValue.text = Mathf.RoundToInt(textSpeedSlider.value) + " letters/s";
        }

        void AskReset()
        {
            AudioManager.I?.PlayClick();
            if (resetConfirm != null) resetConfirm.SetActive(true);
        }

        void DoReset()
        {
            AudioManager.I?.PlayClick();
            ProgressManager.I?.ResetAll();
            if (resetConfirm != null) resetConfirm.SetActive(false);
        }
    }
}
