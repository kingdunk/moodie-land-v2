using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MoodieLand
{
    /// <summary>
    /// Level 1 — Rainbow Forest. NPC: Mimi.
    /// Goal: emotion recognition through Observe → Identify → Understand.
    /// Task 1 What's Happening? / Task 2 Look for the Clues / Task 3 Everyone Has Feelings.
    /// </summary>
    public class Level01Controller : SceneBase
    {
        [Header("Content")]
        public CardLibrary lib;
        public ClickableProp clueFace;
        public ClickableProp clueBody;
        public ClickableProp clueSituation;
        public GameObject[] stageIntro;
        public GameObject[] stageTower;
        public GameObject[] stageFound;
        public Image situationImage;
        public Sprite[] situationSprites = new Sprite[3];

        readonly HashSet<ClickableProp> foundClues = new HashSet<ClickableProp>();

        protected override IEnumerator Flow()
        {
            levelId = "Level1";
            if (hud != null)
            {
                hud.SetTitle("Rainbow Forest");
                hud.SetBackAction(() => GameManager.I?.GoHarbor());
            }

            SetStage(0);
            SetTaskIndex(0, 3, "Task 1 · What's Happening?");

            yield return Banner("Level 1 · Rainbow Forest", "Emotion Recognition");

            yield return Say(
                DialogueLine.Narrator("The cloud boat sets you down between the rainbow trees. A small squirrel is searching under the mushrooms."),
                new DialogueLine("pip", "Pip", "This is Mimi. Something seems to be wrong.", "pointing"),
                new DialogueLine("mimi", "Mimi", "Oh no... where is my pinecone basket?", "searching"),
                new DialogueLine("pip", "Pip", "Before we help, let's look at how Mimi feels.", "idle")
            );

            yield return TaskOne();
            yield return TaskTwo();
            yield return TaskThree();

            yield return Finish();
        }

        void SetStage(int index)
        {
            Toggle(stageIntro, index == 0);
            Toggle(stageTower, index == 1);
            Toggle(stageFound, index == 2);
            if (situationImage != null) situationImage.gameObject.SetActive(false);
        }

        static void Toggle(GameObject[] group, bool on)
        {
            if (group == null) return;
            foreach (var go in group)
                if (go != null) go.SetActive(on);
        }

        // ------------------------------------------------------------- task one

        IEnumerator TaskOne()
        {
            LogStart("Task1");
            SetHint("Look at Mimi's eyebrows, her mouth, and her hands. They all tell us something.");

            yield return Say(new DialogueLine("mimi", "Mimi", "I had it a moment ago... I really can't find it.", "worried"),
                             new DialogueLine("pip", "Pip", "How do you think Mimi feels right now?", "thinking"));

            var options = new[]
            {
                lib.Card("worried", "Worried"),
                lib.Card("sad", "Sad"),
                lib.Card("happy", "Happy"),
                lib.Card("notsure", "Not sure"),
            };

            yield return Ask(options, "How does Mimi feel?");
            LogChoice("Task1", lastCard.Data.id);

            switch (lastCard.Data.id)
            {
                case "worried":
                    yield return Say(
                        new DialogueLine("mimi", "Mimi", "Yes, I'm worried. I can't find my basket!", "worried"),
                        new DialogueLine("pip", "Pip", "You noticed her face and listened to her words. That is a great clue!", "happy"));
                    Praise("You noticed the feeling!");
                    break;

                case "sad":
                    yield return Say(
                        new DialogueLine("mimi", "Mimi", "I feel a little sad too.", "worried"),
                        new DialogueLine("pip", "Pip", "That's a good guess. Sad and worried can look alike. Let's keep looking at her face.", "thinking"));
                    break;

                case "happy":
                    yield return Say(
                        new DialogueLine("pip", "Pip", "Hmm, look at her eyebrows and the way she holds her hands. Does she look happy?", "thinking"),
                        new DialogueLine("mimi", "Mimi", "Not really... I'm worried about my basket.", "worried"),
                        new DialogueLine("pip", "Pip", "That's okay. We can always look again.", "idle"));
                    break;

                default:
                    yield return Say(
                        new DialogueLine("pip", "Pip", "That's okay! 'Not sure' is an honest answer.", "idle"),
                        new DialogueLine("pip", "Pip", "Let's look at her face and her hands together.", "thinking"));
                    break;
            }

            LogDone("Task1");
            yield return Hold(0.2f);
        }

        // ------------------------------------------------------------- task two

        IEnumerator TaskTwo()
        {
            LogStart("Task2");
            SetStage(1);
            SetTaskIndex(1, 3, "Task 2 · Look for the Clues");
            yield return Banner("Task 2", "Look for the Clues");

            yield return Say(
                DialogueLine.Narrator("Behind the mushrooms, Mimi's pinecone tower has fallen over."),
                new DialogueLine("mimi", "Mimi", "I built it three times... and it fell down again.", "frustrated"),
                new DialogueLine("pip", "Pip", "How does Mimi feel now?", "thinking")
            );

            var options = new[]
            {
                lib.Card("frustrated", "Frustrated"),
                lib.Card("worried", "Worried"),
                lib.Card("happy", "Happy"),
                lib.Card("notsure", "Not sure"),
            };

            yield return Ask(options, "How does Mimi feel now?");
            LogChoice("Task2", lastCard.Data.id);

            if (lastCard.Data.id == "frustrated")
                yield return Say(
                    new DialogueLine("mimi", "Mimi", "Yes! I tried so hard and it fell again.", "frustrated"),
                    new DialogueLine("pip", "Pip", "Frustrated — that fits. Now let's find the clues that show it.", "happy"));
            else if (lastCard.Data.id == "worried")
                yield return Say(
                    new DialogueLine("mimi", "Mimi", "A little worried... and frustrated too.", "frustrated"),
                    new DialogueLine("pip", "Pip", "Both can be true at the same time. Let's look for clues.", "idle"));
            else if (lastCard.Data.id == "happy")
                yield return Say(
                    new DialogueLine("pip", "Pip", "Her mouth turns down and her shoulders drop. Let's find the clues.", "thinking"));
            else
                yield return Say(
                    new DialogueLine("pip", "Pip", "Not sure is fine. Let's use three clues together.", "idle"));

            // ---- three clues
            SetHint("Tap the three clue cards: Mimi's face, her body, and what happened.");
            yield return Say(new DialogueLine("pip", "Pip", "There are three clues on screen. Tap each one to look closer.", "pointing"));

            foundClues.Clear();
            if (clueFace != null) { clueFace.SetHighlighted(false); clueFace.SetInteractable(true); }
            if (clueBody != null) { clueBody.SetHighlighted(false); clueBody.SetInteractable(true); }
            if (clueSituation != null) { clueSituation.SetHighlighted(false); clueSituation.SetInteractable(true); }

            while (foundClues.Count < 3)
            {
                ClickableProp clicked = null;
                System.Action<ClickableProp> handler = p => { if (!foundClues.Contains(p)) clicked = p; };

                if (clueFace != null) clueFace.Clicked += handler;
                if (clueBody != null) clueBody.Clicked += handler;
                if (clueSituation != null) clueSituation.Clicked += handler;

                while (clicked == null) yield return null;

                if (clueFace != null) clueFace.Clicked -= handler;
                if (clueBody != null) clueBody.Clicked -= handler;
                if (clueSituation != null) clueSituation.Clicked -= handler;

                foundClues.Add(clicked);
                clicked.SetHighlighted(true);
                clicked.Punp();

                // ignore stray clicks while the explanation is on screen
                if (clueFace != null) clueFace.SetInteractable(false);
                if (clueBody != null) clueBody.SetInteractable(false);
                if (clueSituation != null) clueSituation.SetInteractable(false);

                if (clicked == clueFace)
                {
                    yield return Say(new DialogueLine("pip", "Pip", "Clue 1 — her face. Her eyebrows pull down and her mouth turns down.", "pointing"));
                    LogChoice("Task2", "ClueFace");
                    Praise("Clue found: facial expression");
                }
                else if (clicked == clueBody)
                {
                    yield return Say(new DialogueLine("pip", "Pip", "Clue 2 — her body. Her shoulders are slumped and her arms hang down.", "pointing"));
                    LogChoice("Task2", "ClueBody");
                    Praise("Clue found: body language");
                }
                else
                {
                    yield return Say(new DialogueLine("pip", "Pip", "Clue 3 — the situation. Her pinecone tower just fell down.", "pointing"));
                    LogChoice("Task2", "ClueSituation");
                    Praise("Clue found: what happened");
                }

                // re-enable whatever is still missing
                if (clueFace != null) clueFace.SetInteractable(!foundClues.Contains(clueFace));
                if (clueBody != null) clueBody.SetInteractable(!foundClues.Contains(clueBody));
                if (clueSituation != null) clueSituation.SetInteractable(!foundClues.Contains(clueSituation));
            }

            yield return Say(
                new DialogueLine("pip", "Pip", "We used three clues: her face, her body, and what happened.", "happy"),
                new DialogueLine("pip", "Pip", "Put them together and we can understand how someone feels.", "idle"),
                new DialogueLine("mimi", "Mimi", "Thanks for looking closely. I feel a bit better already.", "calm")
            );

            LogDone("Task2");
            ClearHint();
        }

        // ----------------------------------------------------------- task three

        IEnumerator TaskThree()
        {
            LogStart("Task3");
            SetStage(2);
            SetTaskIndex(2, 3, "Task 3 · Everyone Has Feelings");
            yield return Banner("Task 3", "Everyone Has Feelings");

            yield return Say(
                DialogueLine.Narrator("Mimi's basket was behind the mushroom house all along."),
                new DialogueLine("mimi", "Mimi", "I found it! Thank you for helping me look.", "happy"),
                new DialogueLine("pip", "Pip", "Everyone has feelings. Let's practise with three little stories.", "waving")
            );

            yield return Situation(
                0,
                "Bobo's block castle falls down.",
                "How might Bobo feel?",
                new[] { "Happy", "Sad", "Excited" },
                "sad",
                "When something we made falls down, we often feel sad or frustrated. Both are okay.");

            yield return Situation(
                1,
                "Nunu waits a long time for a turn.",
                "How might Nunu feel?",
                new[] { "Happy", "Tired", "Proud" },
                "tired",
                "Waiting a long time can make us feel tired or restless.");

            yield return Situation(
                2,
                "Pip gets a surprise gift from a friend.",
                "How might Pip feel?",
                new[] { "Happy", "Angry", "Sad" },
                "happy",
                "A surprise from a friend often makes us feel happy and warm.");

            LogDone("Task3");
        }

        IEnumerator Situation(int index, string scenario, string question, string[] optionLabels,
            string best, string explanation)
        {
            SetHint("Think about what happened in the story. Then choose the feeling that fits.");

            if (situationImage != null && situationSprites != null && index < situationSprites.Length)
            {
                situationImage.sprite = situationSprites[index];
                situationImage.gameObject.SetActive(true);
                yield return UIFx.Pop(situationImage.transform, 0.85f, 1f, 0.3f);
            }

            yield return Say(DialogueLine.Narrator(scenario),
                             new DialogueLine("pip", "Pip", question, "thinking"));

            var options = new CardData[optionLabels.Length];
            for (int i = 0; i < optionLabels.Length; i++)
                options[i] = lib.Card(optionLabels[i].ToLowerInvariant(), optionLabels[i]);

            yield return Ask(options, question);
            LogChoice("Task3", "S" + (index + 1) + "_" + lastCard.Data.id);

            if (situationImage != null) situationImage.gameObject.SetActive(false);

            if (lastCard.Data.id == best)
            {
                Praise("That fits the story!");
                yield return Say(new DialogueLine("pip", "Pip", explanation, "happy"));
            }
            else
            {
                Gentle("Good try — here is another idea.");
                yield return Say(new DialogueLine("pip", "Pip", explanation, "idle"));
            }

            ClearHint();
        }

        // --------------------------------------------------------------- ending

        IEnumerator Finish()
        {
            SetHint("");
            yield return Say(
                new DialogueLine("mimi", "Mimi", "Thank you for noticing how I feel. It helped me a lot.", "happy"),
                new DialogueLine("pip", "Pip", "You used her face, her body and the situation. That is empathy.", "happy")
            );

            ProgressManager.I?.CompleteLevel(0);
            yield return GiveBadge(BadgeId.Rainbow, "Rainbow Badge", "You noticed feelings in Rainbow Forest!", null);

            Info("Level 1 complete — back to Moodie Harbor.");
            yield return Hold(0.6f);
            GameManager.I?.GoHarbor();
        }
    }
}
