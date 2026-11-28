using System.Collections;
using UnityEngine;

namespace MoodieLand
{
    /// <summary>
    /// Level 2 — Blocky Village. NPC: Bobo.
    /// Goal: social interaction and communication through real branching.
    /// Task 1 Would You Like to Play? / Task 2 Let's Take Turns / Task 3 Can I Help You?
    /// </summary>
    public class Level02Controller : SceneBase
    {
        [Header("Content")]
        public CardLibrary lib;
        public TaskTwoCastlePuzzle taskTwoPuzzle;
        public GameObject[] stageCastle;
        public GameObject[] stageBroken;
        public GameObject[] castleBlocks = new GameObject[0];

        protected override IEnumerator Flow()
        {
            levelId = "Level2";
            if (hud != null)
            {
                hud.SetTitle("Blocky Village");
                hud.SetBackAction(() => GameManager.I?.GoHarbor());
            }

            SetStage(0);
            SetTaskIndex(0, 3, "Task 1 · Would You Like to Play?");

            yield return Banner("Level 2 · Blocky Village", "Social Interaction");

            yield return Say(
                DialogueLine.Narrator("The cloud boat lands beside a village built from giant toy blocks."),
                new DialogueLine("bobo", "Bobo", "Hi! I'm Bobo. I'm building a block castle. Would you like to play with me?", "waving"),
                new DialogueLine("pip", "Pip", "There is no single right answer. Say what you really want.", "waving")
            );

            yield return TaskOne();
            yield return TaskTwo();
            yield return TaskThree();

            yield return Finish();
        }

        void SetStage(int index)
        {
            Toggle(stageCastle, index == 0);
            Toggle(stageBroken, index == 1);
            taskTwoPuzzle.gameObject.SetActive(index == 2);
            foreach (var b in castleBlocks)
                if (b != null) b.SetActive(false);
        }

        static void Toggle(GameObject[] group, bool on)
        {
            if (group == null) return;
            foreach (var go in group)
                if (go != null) go.SetActive(on);
        }

        IEnumerator BuildBlocks(float delay, float placementDuration = 0f)
        {
            var bobo = Find("bobo");
            if (bobo != null) bobo.SetExpression("building", restart: true);
            for (int i = 0; i < castleBlocks.Length; i++)
            {
                var piece = castleBlocks[i];
                if (piece == null) continue;

                if (placementDuration > 0f && piece.transform is RectTransform block)
                {
                    Vector2 landing = block.anchoredPosition;
                    float approachX = i % 2 == 0 ? -42f : 42f;
                    block.anchoredPosition = landing + new Vector2(approachX, 110f);
                    piece.SetActive(true);
                    yield return UIFx.MoveAnchored(block, landing, placementDuration);
                }
                else
                {
                    piece.SetActive(true);
                    StartCoroutine(UIFx.Pop(piece.transform, 0.4f, 1f, 0.3f));
                }

                yield return Hold(delay);
            }
        }

        // ------------------------------------------------------------- task one

        IEnumerator TaskOne()
        {
            LogStart("Task1");
            SetHint("Every answer is allowed. Choose the one that feels like you.");

            var options = new[]
            {
                lib.Card("accept", "Yes, let's build!"),
                lib.Card("watch", "I want to watch first."),
                lib.Card("later", "Maybe later."),
                lib.Card("decline", "No, thank you."),
            };

            yield return Ask(options, "Bobo asks you to play. What do you say?");
            LogChoice("Task1", lastCard.Data.id, lastCard.Data.id);

            switch (lastCard.Data.id)
            {
                case "accept":
                    yield return Say(new DialogueLine("bobo", "Bobo", "Yay! Let's build together!", "happy"));
                    SetHint("");
                    yield return BuildBlocks(0.07f, placementDuration: 0.34f);
                    yield return Say(new DialogueLine("bobo", "Bobo", "You put the blue block on top. It looks great!", "happy"));
                    Praise("You joined in!");
                    break;

                case "watch":
                    yield return Say(
                        new DialogueLine("bobo", "Bobo", "Sure! Watch me first.", "calm"),
                        new DialogueLine("bobo", "Bobo", "I put a big block at the bottom, then smaller ones on top.", "building"));
                    SetHint("");
                    yield return BuildBlocks(0.12f, placementDuration: 0.44f);
                    yield return Say(
                        new DialogueLine("bobo", "Bobo", "What do you think?", "listening"),
                        new DialogueLine("pip", "Pip", "Watching first is a good way to feel ready.", "idle"));
                    break;

                case "later":
                    yield return Say(
                        new DialogueLine("bobo", "Bobo", "That's okay! Maybe we can play after.", "calm"),
                        new DialogueLine("pip", "Pip", "Saying 'later' is honest and kind.", "happy"));
                    break;

                default:
                    yield return Say(
                        new DialogueLine("bobo", "Bobo", "No problem! Thanks for telling me.", "calm"),
                        new DialogueLine("pip", "Pip", "You said no clearly and kindly. Bobo is still your friend.", "happy"));
                    Praise("Saying no kindly is okay.");
                    break;
            }

            LogDone("Task1");
            yield return Hold(0.2f);
        }

        // ------------------------------------------------------------- task two

        IEnumerator TaskTwo()
        {
            LogStart("Task2");
            SetStage(2);
            var bobo = Find("bobo");
            if (bobo != null) bobo.SetExpression("building");
            SetTaskIndex(1, 3, "Task 2 · Let's Take Turns");
            yield return Banner("Task 2", "Let's Take Turns!");
            SetHint("There are kind ways and rushed ways to ask for a turn. Try one and see.");

            yield return Say(
                DialogueLine.Narrator("Bobo is holding the blue block. You need it for your part of the castle."),
                new DialogueLine("bobo", "Bobo", "I'm using this blue block for the tower.", "building"),
                new DialogueLine("pip", "Pip", "How could you ask for a turn?", "thinking")
            );

            bool negotiated = false;
            bool done = false;

            while (!done)
            {
                var options = new[]
                {
                    lib.Card("whendone", "Can I use it when you're done?"),
                    lib.Card("turns", "Let's take turns!"),
                    lib.Card("another", "I'll choose another block."),
                    lib.Card("now", "I want it now!"),
                };

                yield return Ask(options, "What do you want to say?");
                LogChoice("Task2", lastCard.Data.id, lastCard.Data.id);

                switch (lastCard.Data.id)
                {
                    case "whendone":
                        yield return Say(
                            new DialogueLine("bobo", "Bobo", "Sure! I'll tell you when I'm done.", "building"));
                        yield return Hold(0.6f);
                        yield return Say(
                            new DialogueLine("bobo", "Bobo", "Done! Here you go.", "offering"));
                        Praise("Asking and waiting works!");
                        done = true;
                        break;

                    case "turns":
                        yield return Say(
                            new DialogueLine("bobo", "Bobo", "Great idea! You go first, then me.", "offering"),
                            new DialogueLine("bobo", "Bobo", "Taking turns is fair for both of us.", "calm"));
                        Praise("Taking turns feels fair.");
                        done = true;
                        break;

                    case "another":
                        yield return Say(
                            new DialogueLine("bobo", "Bobo", "That's okay! There are more blocks over there.", "calm"),
                            new DialogueLine("pip", "Pip", "Choosing another block is a flexible solution.", "happy"));
                        done = true;
                        break;

                    default:
                        if (!negotiated)
                        {
                            negotiated = true;
                            yield return Say(
                                new DialogueLine("bobo", "Bobo", "I'm still using it. Please wait a little.", "building"),
                                new DialogueLine("pip", "Pip", "That did not work. Let's try another way to ask.", "thinking"));
                            Gentle("Let's try a different way to ask.");
                            LogChoice("Task2", "Now_Retry", "negotiation");
                        }
                        else
                        {
                            yield return Say(
                                new DialogueLine("bobo", "Bobo", "Let's find a way that works for both of us.", "calm"));
                            yield return Say(
                                new DialogueLine("bobo", "Bobo", "You can have it after my turn. Deal?", "happy"));
                            done = true;
                        }
                        break;
                }
            }

            SetHint("Drag a block from the tray to its matching shape. You can also tap the block, then tap its spot. Take turns with Bobo.");
            if (bobo != null) bobo.SetExpression("calm");
            taskTwoPuzzle.ShowTray();

            yield return Say(new DialogueLine("pip", "Pip", "Start with the blue block. Drag it to the matching spot, or tap the block and then tap its spot. Then it's Bobo's turn!", "pointing"));
            yield return taskTwoPuzzle.Play(bobo);

            yield return Say(
                new DialogueLine("bobo", "Bobo", "It fits! We finished our castle together.", "happy"),
                new DialogueLine("pip", "Pip", "You asked, waited and shared. That is real communication.", "nodding"));

            LogDone("Task2");
            ClearHint();
        }

        // ----------------------------------------------------------- task three

        IEnumerator RebuildCastle(float delay)
        {
            Toggle(stageBroken, false);
            yield return BuildBlocks(delay);
        }

        IEnumerator TaskThree()
        {
            LogStart("Task3");
            SetStage(1);
            SetTaskIndex(2, 3, "Task 3 · Can I Help You?");
            yield return Banner("Task 3", "Can I Help You?");
            SetHint("Helping is not always the same thing. Ask what Bobo needs.");

            yield return Say(
                DialogueLine.Narrator("The block castle wobbles... and falls down."),
                new DialogueLine("bobo", "Bobo", "Oh no... my castle fell down.", "disappointed"),
                new DialogueLine("pip", "Pip", "Bobo looks disappointed. What could you say?", "thinking")
            );

            var options = new[]
            {
                lib.Card("help", "Would you like some help?"),
                lib.Card("myself", "Do you want to try by yourself?"),
                lib.Card("break", "Shall we take a break?"),
                lib.Card("here", "I'll be right here if you need me."),
            };

            yield return Ask(options, "What do you say to Bobo?");
            LogChoice("Task3", lastCard.Data.id, lastCard.Data.id);

            switch (lastCard.Data.id)
            {
                case "help":
                    yield return Say(
                        new DialogueLine("bobo", "Bobo", "Yes, please!", "happy"),
                        new DialogueLine("bobo", "Bobo", "You hold the bottom and I will put the top on.", "building"));
                    SetHint("");
                    yield return RebuildCastle(0.2f);
                    yield return Say(new DialogueLine("bobo", "Bobo", "We did it together! Thank you for helping.", "happy"));
                    Praise("Helping was welcome!");
                    break;

                case "myself":
                    yield return Say(
                        new DialogueLine("bobo", "Bobo", "I want to try by myself.", "calm"),
                        new DialogueLine("pip", "Pip", "Sometimes helping means giving space.", "idle"),
                        new DialogueLine("bobo", "Bobo", "I'll try again... slowly...", "building"));
                    SetHint("");
                    yield return RebuildCastle(0.35f);
                    yield return Say(
                        new DialogueLine("bobo", "Bobo", "I did it! Thank you for waiting with me.", "happy"));
                    Praise("Waiting is a kind of help.");
                    break;

                case "break":
                    yield return Say(
                        new DialogueLine("bobo", "Bobo", "That sounds good. My hands were tired.", "calm"),
                        new DialogueLine("pip", "Pip", "A break can make the next try easier.", "calm"),
                        new DialogueLine("bobo", "Bobo", "Okay, I feel better now. Let's try again.", "happy"));
                    SetHint("");
                    yield return RebuildCastle(0.2f);
                    break;

                default:
                    yield return Say(
                        new DialogueLine("bobo", "Bobo", "That helps. I know you are here.", "calm"),
                        new DialogueLine("bobo", "Bobo", "I'll try again... and I did it!", "happy"));
                    SetHint("");
                    yield return RebuildCastle(0.25f);
                    break;
            }

            yield return Say(new DialogueLine("bobo", "Bobo", "The castle is standing again. Thank you for talking with me.", "clapping"));
            LogDone("Task3");
            ClearHint();
        }

        // --------------------------------------------------------------- ending

        IEnumerator Finish()
        {
            SetHint("");
            yield return Say(
                new DialogueLine("bobo", "Bobo", "Playing with you was fun. You asked, you waited, and you listened.", "happy"),
                new DialogueLine("pip", "Pip", "Talking clearly and kindly makes play better for everyone.", "happy")
            );

            ProgressManager.I?.CompleteLevel(1);
            yield return GiveBadge(BadgeId.Star, "Star Badge", "You shared, waited and helped in Blocky Village!", null);

            Info("Level 2 complete — back to Moodie Harbor.");
            yield return Hold(0.6f);
            GameManager.I?.GoHarbor();
        }
    }
}
