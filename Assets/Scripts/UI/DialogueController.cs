using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace MoodieLand
{
    [Serializable]
    public class DialogueLine
    {
        [Tooltip("Who is talking: pip / mimi / bobo / nunu / narrator")]
        public string speakerKey = "pip";
        [Tooltip("Name shown in the name plate")]
        public string speaker = "Pip";
        [TextArea(2, 5)]
        public string text = "";
        [Tooltip("Expression key for the speaking character, e.g. happy / worried / thinking")]
        public string expression = "idle";

        public DialogueLine() { }

        public DialogueLine(string key, string name, string text, string expression = "idle")
        {
            speakerKey = key;
            speaker = name;
            this.text = text;
            this.expression = expression;
        }

        public static DialogueLine Narrator(string text) =>
            new DialogueLine("narrator", "", text, "idle");
    }

    /// <summary>
    /// Shared dialogue box. Handles typewriter text, expression changes,
    /// click-to-continue (and click-to-skip-typing).
    /// </summary>
    [DisallowMultipleComponent]
    public class DialogueController : MonoBehaviour
    {
        public const string TextSpeedKey = "MoodieLand.textSpeed";

        [Header("Wiring")]
        public CanvasGroup group;
        public Image portrait;
        public RectTransform portraitFrame;
        public Text nameText;
        public Text bodyText;
        public Button advanceButton;
        public GameObject continueHint;
        public CharacterView[] characters;

        [Header("Behaviour")]
        [Range(15f, 120f)] public float charsPerSecond = 48f;
        [Tooltip("Portrait is hidden for narrator lines and this tag is shown instead.")]
        public Color narratorColor = new Color32(0x7A, 0x6E, 0x64, 0xFF);

        public bool IsPlaying { get; private set; }

        bool typing;
        bool skipRequested;
        bool advanceRequested;
        float inputLockUntil;
        Coroutine current;

        void Awake()
        {
            charsPerSecond = PlayerPrefs.GetFloat(TextSpeedKey, charsPerSecond);
            if (group != null)
            {
                group.alpha = 0f;
                group.blocksRaycasts = false;
            }
            if (advanceButton != null) advanceButton.onClick.AddListener(OnAdvanceClicked);
            if (continueHint != null) continueHint.SetActive(false);
        }

        public void OnAdvanceClicked()
        {
            if (!IsPlaying || Time.unscaledTime < inputLockUntil) return;
            if (typing) { skipRequested = true; return; }
            advanceRequested = true;
        }

        public void Show()
        {
            if (group == null) return;
            group.blocksRaycasts = true;
            group.interactable = true;
            StartFade(group.alpha, 1f, 0.2f);
        }

        Coroutine fade;

        void StartFade(float from, float to, float duration)
        {
            if (fade != null) StopCoroutine(fade);
            fade = StartCoroutine(UIFx.FadeGroup(group, from, to, duration));
        }

        public void Hide()
        {
            if (group == null) return;
            IsPlaying = false;
            group.blocksRaycasts = false;
            group.interactable = false;
            if (!gameObject.activeInHierarchy)
            {
                group.alpha = 0f;
                return;
            }
            StartFade(group.alpha, 0f, 0.18f);
        }

        public void HideImmediate()
        {
            if (group == null) return;
            if (current != null) StopCoroutine(current);
            if (fade != null) StopCoroutine(fade);
            IsPlaying = false;
            group.alpha = 0f;
            group.blocksRaycasts = false;
        }

        /// <summary>Plays a sequence and calls onDone when the last line is dismissed.</summary>
        public void Play(DialogueLine[] lines, Action onDone = null)
        {
            if (current != null) StopCoroutine(current);
            current = StartCoroutine(PlayRoutine(lines, onDone));
        }

        public void PlayOne(DialogueLine line, Action onDone = null) =>
            Play(new[] { line }, onDone);

        IEnumerator PlayRoutine(DialogueLine[] lines, Action onDone)
        {
            if (lines == null || lines.Length == 0)
            {
                onDone?.Invoke();
                yield break;
            }

            IsPlaying = true;
            Show();
            yield return new WaitForSecondsRealtime(0.18f);

            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                if (line == null) continue;

                ApplySpeaker(line);

                typing = true;
                skipRequested = false;
                advanceRequested = false;
                inputLockUntil = Time.unscaledTime + 0.12f;

                if (continueHint != null) continueHint.SetActive(false);

                bodyText.text = "";
                float shown = 0f;
                int total = line.text != null ? line.text.Length : 0;

                // keep the character animating while its line types out
                CharacterView speakerView = FindCharacter(line.speakerKey);
                if (speakerView != null) speakerView.TalkFor(0.4f);

                while (shown < total)
                {
                    if (skipRequested)
                    {
                        shown = total;
                        break;
                    }
                    shown += Mathf.Max(1f, charsPerSecond) * Time.unscaledDeltaTime;
                    int count = Mathf.Clamp(Mathf.FloorToInt(shown), 0, total);
                    bodyText.text = line.text.Substring(0, count);
                    yield return null;
                }

                bodyText.text = line.text;
                typing = false;
                skipRequested = false;
                if (continueHint != null) continueHint.SetActive(true);
                inputLockUntil = Time.unscaledTime + 0.1f;

                // small bounce when the line finishes
                if (bodyText != null) StartCoroutine(UIFx.Punch(bodyText.transform, 0.012f, 0.16f));

                while (!advanceRequested) yield return null;
                advanceRequested = false;
                if (AudioManager.I != null) AudioManager.I.PlayClick();
                if (continueHint != null) continueHint.SetActive(false);
            }

            IsPlaying = false;
            Hide();
            yield return new WaitForSecondsRealtime(0.2f);
            current = null;
            onDone?.Invoke();
        }

        void ApplySpeaker(DialogueLine line)
        {
            bool narrator = string.IsNullOrEmpty(line.speakerKey) ||
                            line.speakerKey.Equals("narrator", StringComparison.OrdinalIgnoreCase);

            if (nameText != null)
            {
                nameText.text = narrator ? "" : line.speaker;
                nameText.color = narrator ? narratorColor : MoodieUI.Teal;
            }

            var view = FindCharacter(line.speakerKey);

            if (portrait != null)
            {
                if (narrator || view == null)
                {
                    portrait.gameObject.SetActive(false);
                    if (portraitFrame != null) portraitFrame.gameObject.SetActive(false);
                }
                else
                {
                    portrait.gameObject.SetActive(true);
                    if (portraitFrame != null) portraitFrame.gameObject.SetActive(true);
                    var sprite = view.GetHead(line.expression);
                    if (sprite != null) portrait.sprite = sprite;
                    view.SetExpression(line.expression);
                }
            }
            else if (view != null)
            {
                view.SetExpression(line.expression);
            }
        }

        public CharacterView FindCharacter(string key)
        {
            if (characters == null || string.IsNullOrEmpty(key)) return null;
            for (int i = 0; i < characters.Length; i++)
            {
                if (characters[i] != null &&
                    string.Equals(characters[i].characterKey, key, StringComparison.OrdinalIgnoreCase))
                    return characters[i];
            }
            return null;
        }

        /// <summary>Pushes the current text speed setting to the box.</summary>
        public void ApplyTextSpeed(float cps)
        {
            charsPerSecond = Mathf.Clamp(cps, 15f, 120f);
            PlayerPrefs.SetFloat(TextSpeedKey, charsPerSecond);
        }
    }
}
