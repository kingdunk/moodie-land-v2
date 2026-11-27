using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace MoodieLand
{
    /// <summary>Short opening: the cloud boat arrives and Pip introduces Moodie Land.</summary>
    public class StoryIntroController : SceneBase
    {
        [Header("Wiring")]
        public Button startButton;
        public RectTransform boat;
        public CanvasGroup startGroup;

        Vector2 boatTarget;

        protected override IEnumerator Flow()
        {
            levelId = "Intro";
            if (hud != null)
            {
                hud.SetTitle("Moodie Land");
                hud.SetVisible(false);
            }

            if (startGroup != null)
            {
                startGroup.alpha = 0f;
                startGroup.blocksRaycasts = false;
            }

            if (boat != null)
            {
                boatTarget = boat.anchoredPosition;
                boat.anchoredPosition = boatTarget + new Vector2(-700f, 30f);
                StartCoroutine(Drift());
            }

            yield return Banner("Moodie Land", "A small adventure about big feelings");

            yield return Say(
                DialogueLine.Narrator("A soft cloud boat drifts down towards a floating island. Somewhere below, a tree is glowing."),
                new DialogueLine("pip", "Pip", "Hi! I'm Pip. Welcome to Moodie Land.", "waving"),
                new DialogueLine("pip", "Pip", "Three friends live here: Mimi in Rainbow Forest, Bobo in Blocky Village, and Nunu in Cloudy Cove.", "pointing"),
                new DialogueLine("pip", "Pip", "They are learning about feelings. Will you help them?", "idle"),
                new DialogueLine("pip", "Pip", "You can take your time. Nothing here is a race.", "happy")
            );

            if (startGroup != null)
            {
                startGroup.blocksRaycasts = true;
                startGroup.interactable = true;
                StartCoroutine(UIFx.FadeGroup(startGroup, 0f, 1f, 0.3f));
                StartCoroutine(UIFx.Pop(startGroup.transform, 0.85f, 1f, 0.35f));
            }

            if (startButton != null)
                startButton.onClick.AddListener(() =>
                {
                    AudioManager.I?.PlayWhoosh();
                    GameManager.I?.GoHarbor();
                });
        }

        System.Collections.IEnumerator Drift()
        {
            yield return UIFx.MoveAnchored(boat, boatTarget, 2.2f);
            yield return UIFx.FloatLoop(boat, 6f, 1.1f);
        }
    }
}
