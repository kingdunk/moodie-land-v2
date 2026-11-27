using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace MoodieLand
{
    /// <summary>
    /// Moodie Harbor — the central map. Shows the three areas, their badges and
    /// unlocks the ending once all three are complete.
    /// The introduction only plays the first time the player arrives.
    /// </summary>
    public class HarborController : SceneBase
    {
        [Header("Map")]
        public ClickableProp[] portals = new ClickableProp[3];
        public Image[] portalBadges = new Image[3];
        public Text[] portalStates = new Text[3];
        public Sprite[] badgeSprites = new Sprite[3];
        public Sprite lockedSprite;
        public Text progressText;
        public Button endingButton;
        public Text endingLabel;
        public Button[] lockedEndingGroup;
        public RectTransform moodTree;

        string[] names = { "Rainbow Forest", "Blocky Village", "Cloudy Cove" };

        protected override IEnumerator Flow()
        {
            levelId = "Harbor";
            if (hud != null)
            {
                hud.SetTitle("Moodie Harbor");
                hud.SetBackAction(() => GameManager.I?.GoMainMenu());
            }
            SetTaskIndex(0, 3, "Choose a place to visit");
            SetHint("Visit all three places to unlock the Moodie Tree celebration.");

            RefreshMap();

            if (moodTree != null) StartCoroutine(UIFx.FloatLoop(moodTree, 5f, 1.1f));

            // wire the map first so nothing has to be clicked through before playing
            for (int i = 0; i < portals.Length; i++)
            {
                int index = i;
                if (portals[i] == null) continue;
                portals[i].Clicked += _ => Enter(index);
            }

            if (endingButton != null)
            {
                endingButton.onClick.AddListener(TryEnding);
                endingButton.gameObject.SetActive(false);
            }

            bool firstVisit = ProgressManager.I == null || !ProgressManager.I.harborIntroSeen;
            if (firstVisit)
            {
                if (ProgressManager.I != null)
                {
                    ProgressManager.I.harborIntroSeen = true;
                    ProgressManager.I.Save();
                }

                yield return Banner("Moodie Harbor", "Where would you like to go?");

                yield return Say(
                    DialogueLine.Narrator("The cloud boat drifts into Moodie Harbor. The great Mood Tree glows in the middle of the island."),
                    new DialogueLine("pip", "Pip", "Welcome! Choose a place to visit.", "waving"));
            }

            // the button sits where the dialogue box lives, so it only appears once
            // the (optional) introduction is out of the way
            if (endingButton != null)
            {
                endingButton.gameObject.SetActive(true);
                RefreshMap();
            }

            ClearHint();
        }

        void RefreshMap()
        {
            int done = ProgressManager.I != null ? ProgressManager.I.CompletedCount : 0;

            for (int i = 0; i < portals.Length; i++)
            {
                bool complete = ProgressManager.I != null && ProgressManager.I.IsCompleted(i);

                if (portalBadges != null && i < portalBadges.Length && portalBadges[i] != null)
                {
                    portalBadges[i].gameObject.SetActive(complete);
                    if (complete)
                    {
                        portalBadges[i].sprite = badgeSprites != null && i < badgeSprites.Length ? badgeSprites[i] : null;
                        portalBadges[i].color = Color.white;
                    }
                }

                if (portalStates != null && i < portalStates.Length && portalStates[i] != null)
                    portalStates[i].text = complete ? "Completed" : "Not visited yet";

                if (portals[i] != null)
                    portals[i].SetCaption(names[i]);
            }

            if (progressText != null)
                progressText.text = done + " of 3 places explored";

            bool all = ProgressManager.I != null && ProgressManager.I.AllCompleted;

            if (endingButton != null)
            {
                endingButton.interactable = all;
                var img = endingButton.GetComponent<Image>();
                if (img != null)
                    img.color = all ? Color.white : new Color(1f, 1f, 1f, 0.55f);
            }

            if (endingLabel != null)
                endingLabel.text = all ? "Visit the Mood Tree" : "Mood Tree (visit all 3 places)";

            if (lockedEndingGroup != null)
                foreach (var b in lockedEndingGroup)
                    if (b != null) b.gameObject.SetActive(!all);
        }

        void Enter(int index)
        {
            AudioManager.I?.PlayWhoosh();
            GameManager.I?.GoLevel(index);
        }

        void TryEnding()
        {
            if (ProgressManager.I != null && ProgressManager.I.AllCompleted)
            {
                AudioManager.I?.PlaySuccess();
                GameManager.I?.GoEnding();
            }
            else
            {
                AudioManager.I?.PlaySoftError();
                Info("Visit all three places first.");
                Gentle("The Mood Tree is still sleeping. Explore the three areas.");
            }
        }
    }
}
