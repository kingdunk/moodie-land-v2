using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace MoodieLand
{
    /// <summary>Short opening: a sailboat arrives and Pip introduces Moodie Land.</summary>
    public class StoryIntroController : SceneBase
    {
        [Header("Wiring")]
        public Button startButton;
        public RectTransform boat;
        public CanvasGroup startGroup;

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
                StartCoroutine(SailToDock());

            yield return Banner("Moodie Land", "A small adventure about big feelings");

            yield return Say(
                DialogueLine.Narrator("A little sailboat glides toward the island. In the distance, a tree is glowing."),
                new DialogueLine("pip", "Pip", "Hi! I'm Pip. Welcome to Moodie Land.", "waving"),
                new DialogueLine("pip", "Pip", "Three friends live here: Mimi in Rainbow Forest, Bobo in Blocky Village, and Nunu in Cloudy Cove.", "pointing"),
                new DialogueLine("pip", "Pip", "They are learning about feelings. Will you help them?", "idle"),
                new DialogueLine("pip", "Pip", "You can take your time. Nothing here is a race.", "nodding")
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

        IEnumerator SailToDock()
        {
            const float travelDuration = 3.4f;
            const float wavePeriod = 2.6f;
            const float startingScale = 0.46f;

            // The bottom pivot follows the waterline. The scene position is the berth.
            Vector2 berth = boat.anchoredPosition;
            Vector2 start = berth + new Vector2(760f, 126f);
            Vector2 firstTurn = berth + new Vector2(470f, 126f);
            Vector2 dockApproach = berth + new Vector2(185f, 10f);
            Vector3 dockedScale = boat.localScale;
            Quaternion dockedRotation = boat.localRotation;
            float elapsed = 0f;

            while (boat != null)
            {
                float progress = Mathf.SmoothStep(0f, 1f, elapsed / travelDuration);
                float remaining = 1f - progress;
                Vector2 position =
                    remaining * remaining * remaining * start +
                    3f * remaining * remaining * progress * firstTurn +
                    3f * remaining * progress * progress * dockApproach +
                    progress * progress * progress * berth;

                float wave = elapsed * (2f * Mathf.PI / wavePeriod);
                float bob = Mathf.Sin(wave) * Mathf.Lerp(4f, 2f, progress);
                float rock = Mathf.Sin(wave + 0.6f) * Mathf.Lerp(2.4f, 0.9f, progress);
                boat.anchoredPosition = position + Vector2.up * bob;
                boat.localScale = dockedScale * Mathf.Lerp(startingScale, 1f, progress);
                boat.localRotation = dockedRotation * Quaternion.Euler(0f, 0f, rock);

                yield return null;
                elapsed += Time.unscaledDeltaTime;
            }
        }
    }
}
