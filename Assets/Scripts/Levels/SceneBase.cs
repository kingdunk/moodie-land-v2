using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoodieLand
{
    /// <summary>
    /// Shared plumbing for every scene controller: dialogue, card choice,
    /// hint, logging and reward helpers, so each level file stays pure content.
    /// </summary>
    public abstract class SceneBase : MonoBehaviour
    {
        [Header("Shared UI")]
        public HUDController hud;
        public DialogueController dialogue;
        public CardPanel cards;
        public HintController hint;
        public ToastController toast;
        public BadgeDisplay badge;
        public PauseController pause;
        public TaskBanner banner;

        [Header("Characters")]
        public CharacterView[] characters;

        [Header("Identity")]
        public string levelId = "Level1";

        protected CardView lastCard;
        protected List<CardData> lastSelection = new List<CardData>();
        protected ClickableProp lastProp;

        protected abstract IEnumerator Flow();

        protected virtual void Start()
        {
            StartCoroutine(Boot());
        }

        IEnumerator Boot()
        {
            yield return null;                 // let Awake/OnEnable wiring settle
            yield return Flow();
        }

        // -------------------------------------------------------------- dialogue

        protected IEnumerator Say(params DialogueLine[] lines)
        {
            bool done = false;
            dialogue.Play(lines, () => done = true);
            while (!done) yield return null;
        }

        protected IEnumerator Say(DialogueLine[][] groups)
        {
            foreach (var g in groups) yield return Say(g);
        }

        // ----------------------------------------------------------------- cards

        protected IEnumerator Ask(IList<CardData> items, string title = null)
        {
            lastCard = null;
            cards.ShowSingle(items, c => lastCard = c, title);
            while (lastCard == null) yield return null;
            cards.Hide();
            yield return new WaitForSecondsRealtime(0.22f);
        }

        protected IEnumerator AskMulti(IList<CardData> items, string title = null,
            IList<CardData> preselect = null, bool skippable = true)
        {
            lastSelection = null;
            bool skipped = false;
            cards.ShowMulti(items, sel => lastSelection = sel,
                preselect,
                () => { skipped = true; lastSelection = new List<CardData>(); },
                title);

            while (lastSelection == null && !skipped) yield return null;
            if (skipped) lastSelection = new List<CardData>();
            yield return new WaitForSecondsRealtime(0.2f);
        }

        protected bool boardSkipped;
        protected bool boardExtra;

        /// <summary>Feelings Board step: configurable footer (continue / skip / extra).</summary>
        protected IEnumerator AskBoard(IList<CardData> items, bool multiSelect, int limit,
            string title, string confirmText, string skipText, string extraText,
            IList<CardData> preselect = null)
        {
            lastSelection = null;
            boardSkipped = false;
            boardExtra = false;
            bool done = false;

            cards.ShowSelection(items, multiSelect, limit, title, confirmText, skipText, extraText,
                sel => { lastSelection = sel; done = true; },
                () => { boardSkipped = true; lastSelection = new List<CardData>(); done = true; },
                () => { boardExtra = true; lastSelection = new List<CardData>(); done = true; },
                preselect);

            while (!done) yield return null;
            yield return new WaitForSecondsRealtime(0.18f);
        }

        // ------------------------------------------------------------ internals

        protected IEnumerator WaitProp(ClickableProp prop)
        {
            lastProp = null;
            prop.Clicked += p => lastProp = p;
            while (lastProp == null) yield return null;
            yield return new WaitForSecondsRealtime(0.1f);
        }

        protected IEnumerator WaitAnyProp(params ClickableProp[] props)
        {
            lastProp = null;
            System.Action<ClickableProp> handler = p => lastProp = p;
            foreach (var p in props) p.Clicked += handler;
            while (lastProp == null) yield return null;
            foreach (var p in props) p.Clicked -= handler;
            yield return new WaitForSecondsRealtime(0.1f);
        }

        protected IEnumerator Hold(float seconds)
        {
            yield return new WaitForSecondsRealtime(seconds);
        }

        protected IEnumerator Banner(string title, string subtitle)
        {
            if (banner == null) yield break;
            yield return banner.Play(title, subtitle);
        }

        protected void SetHint(string text) => hint?.SetHint(text);
        protected void ClearHint() => hint?.SetHint("");

        protected void Praise(string message) => toast?.Show(message, ToastController.Tone.Praise);
        protected void Gentle(string message) => toast?.Show(message, ToastController.Tone.Gentle);
        protected void Info(string message) => toast?.Show(message, ToastController.Tone.Info);

        protected void LogStart(string task) => DataLogger.I?.StartTask(levelId, task);
        protected void LogChoice(string task, string choice, string branch = null) =>
            DataLogger.I?.LogChoice(levelId, task, choice, branch);
        protected void LogHintUse(string task) => DataLogger.I?.LogHint(levelId, task);
        protected void LogDone(string task) => DataLogger.I?.CompleteTask(levelId, task);

        protected void SetTaskIndex(int index, int total, string label)
        {
            if (hud != null) hud.SetTask(index, total, label);
        }

        // -------------------------------------------------------------- reward

        protected IEnumerator GiveBadge(BadgeId id, string title, string subtitle, System.Action done)
        {
            bool closed = false;
            if (badge != null)
            {
                badge.Show(id, title, subtitle, () => closed = true);
                while (!closed) yield return null;
            }
            done?.Invoke();
        }

        protected CharacterView Find(string key) => dialogue != null ? dialogue.FindCharacter(key) : null;

        protected void HideAllPanels()
        {
            dialogue?.HideImmediate();
            cards?.HideImmediate();
            hint?.HideBubble();
        }
    }
}
