using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace MoodieLand
{
    /// <summary>
    /// Finale: the Mood Tree wakes up, the three badges are awarded and the
    /// Moodie Explorer Card is revealed. The player's message is only shown
    /// here if they chose to share it in Level 3.
    /// </summary>
    public class EndingController : SceneBase
    {
        [Header("Wiring")]
        public CanvasGroup cardGroup;
        public RectTransform card;
        public Image[] cardBadges = new Image[3];
        public Sprite[] badgeSprites = new Sprite[3];
        public GameObject messageBlock;
        public Text messageTitle;
        public Text messageText;
        public Text cardTitle;
        public Button playAgainButton;
        public Button backToHarborButton;
        public Button exitButton;
        public RectTransform moodTree;
        public Image treeGlow;
        public CharacterView[] everyone = new CharacterView[0];

        /// <summary>
        /// Buttons are wired before the flow starts: the card is modal, so the
        /// player must never be able to reach it before its handlers exist.
        /// </summary>
        protected override void Start()
        {
            if (playAgainButton != null) playAgainButton.onClick.AddListener(PlayAgain);
            if (backToHarborButton != null) backToHarborButton.onClick.AddListener(BackToHarbor);
            if (exitButton != null) exitButton.onClick.AddListener(ExitGame);

            if (cardGroup != null)
            {
                cardGroup.alpha = 0f;
                cardGroup.blocksRaycasts = false;
                cardGroup.interactable = false;
            }

            base.Start();
        }

        protected override IEnumerator Flow()
        {
            levelId = "Ending";
            if (hud != null)
            {
                hud.SetTitle("Moodie Harbor");
                hud.SetVisible(false);
            }
            SetTaskIndex(2, 3, "The Mood Tree");

            if (hint != null) hint.gameObject.SetActive(false);

            if (moodTree != null) StartCoroutine(UIFx.FloatLoop(moodTree, 4f, 1.0f));
            if (treeGlow != null) StartCoroutine(GlowPulse());

            yield return Banner("Moodie Harbor", "The Mood Tree is waking up");

            yield return Say(
                DialogueLine.Narrator("The three badges in your pocket begin to shine. Light travels up the trunk of the Mood Tree."),
                new DialogueLine("pip", "Pip", "You did it! Three friends, three feelings, three ways of understanding each other.", "happy")
            );

            for (int i = 0; i < 3; i++)
            {
                if (cardBadges != null && i < cardBadges.Length && cardBadges[i] != null)
                    cardBadges[i].gameObject.SetActive(false);
            }

            // the closing lines are played *before* the card, because the card is
            // modal and would otherwise swallow the clicks that advance dialogue
            yield return Say(
                new DialogueLine("pip", "Pip", "Mimi learned to look for clues. Bobo learned to ask and take turns. Nunu learned to say what is needed.", "idle"),
                new DialogueLine("pip", "Pip", "And you learned that every feeling has something to tell us.", "happy"),
                new DialogueLine("nunu", "Nunu", "Thank you for listening. Come back any time.", "relieved"),
                new DialogueLine("pip", "Pip", "Here is your Moodie Explorer Card. See you in Moodie Land!", "waving")
            );

            yield return ShowCard();
        }

        IEnumerator ShowCard()
        {
            if (cardGroup == null) yield break;

            HideAllPanels();                       // keep the dialogue box out of the way

            for (int i = 0; i < 3; i++)
            {
                if (cardBadges != null && i < cardBadges.Length && cardBadges[i] != null)
                {
                    cardBadges[i].gameObject.SetActive(true);
                    if (badgeSprites != null && i < badgeSprites.Length) cardBadges[i].sprite = badgeSprites[i];
                }
            }

            // The card always carries a message block: shared content, a private
            // reassurance (never the text itself) or a closing thought.
            var msg = ProgressManager.I != null ? ProgressManager.I.myMessage : null;
            bool created = msg != null && msg.created;

            if (messageBlock != null) messageBlock.SetActive(true);

            if (created && msg.sharedWithNunu)
            {
                if (messageTitle != null) messageTitle.text = "My Little Message  ·  shared with Nunu";
                if (messageText != null) messageText.text = msg.Compose();
            }
            else if (created && msg.keptPrivate)
            {
                if (messageTitle != null) messageTitle.text = "My Little Message  ·  kept private";
                if (messageText != null) messageText.text = "You kept this one for yourself.\nThat's okay.";
            }
            else
            {
                if (messageTitle != null) messageTitle.text = "Moodie Explorer";
                if (messageText != null) messageText.text = "All three places explored.\nEvery feeling has something to tell us.";
            }

            cardGroup.blocksRaycasts = true;
            cardGroup.interactable = true;
            AudioManager.I?.PlayBadge();

            yield return UIFx.FadeGroup(cardGroup, 0f, 1f, 0.4f);
            if (card != null) yield return UIFx.Pop(card, 0.85f, 1f, 0.55f);

            for (int i = 0; i < 3; i++)
            {
                if (cardBadges != null && i < cardBadges.Length && cardBadges[i] != null)
                    StartCoroutine(UIFx.Pop(cardBadges[i].transform, 0.2f, 1f, 0.5f, i * 0.14f));
            }

            for (int i = 0; i < everyone.Length; i++)
            {
                if (everyone[i] == null) continue;
                everyone[i].SetExpression("happy");
            }
        }

        // ---------------------------------------------------------------- buttons

        void PlayAgain()
        {
            AudioManager.I?.PlaySuccess();
            // a fresh adventure: wipe badges and the message, then replay the intro
            ProgressManager.I?.ResetAll();
            GameManager.I?.GoStoryIntro();
        }

        void BackToHarbor()
        {
            AudioManager.I?.PlayClick();
            GameManager.I?.GoHarbor();
        }

        void ExitGame()
        {
            AudioManager.I?.PlayClick();
            GameManager.I?.QuitGame();
        }

        IEnumerator GlowPulse()
        {
            float t = 0f;
            var c = treeGlow.color;
            while (treeGlow != null)
            {
                t += Time.unscaledDeltaTime;
                c.a = 0.35f + (Mathf.Sin(t * 1.6f) * 0.5f + 0.5f) * 0.4f;
                treeGlow.color = c;
                yield return null;
            }
        }
    }
}
