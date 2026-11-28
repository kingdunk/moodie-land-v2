using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MoodieLand
{
    /// <summary>
    /// Level 3 — Cloudy Cove. NPC: Nunu.
    /// Goal: emotional awareness and self-expression through the My Feelings Board,
    /// ending with "My Little Message" which the player may share, keep private or edit.
    /// </summary>
    public class Level03Controller : SceneBase
    {
        [Header("Content")]
        public CardLibrary lib;
        public MessagePanel messagePanel;
        public GameObject boardFrame;
        public Text boardTitle;
        public Text boardStep;
        public Image[] boardDots;

        string emotionId = "", emotionLabel = "";
        string intensityId = "", intensityLabel = "";
        readonly List<string> reasonIds = new List<string>();
        readonly List<string> reasonLabels = new List<string>();
        readonly List<string> needIds = new List<string>();
        readonly List<string> needLabels = new List<string>();

        int decision;      // 0 none, 1 shared, 2 private
        bool aboutNunu;

        protected override IEnumerator Flow()
        {
            levelId = "Level3";
            if (hud != null)
            {
                hud.SetTitle("Cloudy Cove");
                hud.SetBackAction(() => GameManager.I?.GoHarbor());
            }
            SetTaskIndex(0, 1, "My Feelings Board");

            yield return Banner("Level 3 · Cloudy Cove", "Self-Expression");

            yield return Say(
                DialogueLine.Narrator("Cloudy Cove is quiet. Soft clouds drift over a small blue lake."),
                new DialogueLine("nunu", "Nunu", "The clouds are rumbling today... I feel a lot of things.", "worried"),
                new DialogueLine("pip", "Pip", "This is Nunu. Nunu can use the Feelings Board to show what is going on.", "pointing"),
                new DialogueLine("nunu", "Nunu", "Will you try it with me? You can keep your message private if you want.", "listening"),
                new DialogueLine("pip", "Pip", "You decide what to share. Let's open the board.", "inviting")
            );

            yield return BoardRoutine();
            yield return Finish();
        }

        // ---------------------------------------------------------------- board

        IEnumerator BoardRoutine()
        {
            bool edit = true;

            while (edit)
            {
                edit = false;
                decision = 0;
                if (boardFrame != null) boardFrame.SetActive(true);
                if (boardTitle != null) boardTitle.text = "My Feelings Board";

                // ---- step 1: emotion
                SetStep(0, 5, "Step 1 · Name the feeling");
                SetHint("There is no wrong feeling. Choose what fits, or skip this step.");
                LogStart("FeelingsBoard");
                yield return Say(new DialogueLine("pip", "Pip", "First — how do you feel right now?", "thinking"));

                var emotions = BuildEmotions();
                yield return AskBoard(emotions, false, 1, "How do you feel right now?",
                    "Continue", "I don't want to say", "Let's talk about Nunu instead",
                    Preselect(emotionId, emotions));

                if (boardExtra)
                {
                    if (boardFrame != null) boardFrame.SetActive(false);
                    ClearHint();
                    yield return AboutNunuBranch();
                    aboutNunu = true;
                    yield break;
                }

                if (boardSkipped)
                {
                    emotionId = ""; emotionLabel = "";
                }
                else
                {
                    emotionId = lastSelection[0].id; emotionLabel = lastSelection[0].label;
                    yield return Say(new DialogueLine("pip", "Pip",
                        "Thank you for naming it. " + emotionLabel + " is a real feeling.", "idle"));
                }

                // ---- step 2: intensity
                SetStep(1, 5, "Step 2 · How strong is it?");
                SetHint("Big, medium or small — feelings come in different sizes. You can skip.");
                var intensity = BuildIntensity();
                yield return AskBoard(intensity, false, 1, "How strong is the feeling?",
                    "Continue", "Skip", null, Preselect(intensityId, intensity));

                if (boardSkipped) { intensityId = ""; intensityLabel = ""; }
                else { intensityId = lastSelection[0].id; intensityLabel = lastSelection[0].label; }

                // ---- step 3: reasons
                SetStep(2, 5, "Step 3 · Maybe something caused it");
                SetHint("Pick as many as you like. It is okay not to know.");
                var reasons = BuildReasons();
                yield return AskBoard(reasons, true, 0, "Maybe something made you feel this way?",
                    "Continue", "Skip", null, Preselect(reasonIds, reasons));

                reasonIds.Clear(); reasonLabels.Clear();
                if (!boardSkipped)
                {
                    foreach (var c in lastSelection) { reasonIds.Add(c.id); reasonLabels.Add(c.label); }
                }

                // ---- step 4: needs
                SetStep(3, 5, "Step 4 · What would help?");
                SetHint("Pick as many as you like. These are things that might help you.");
                var needs = BuildNeeds();
                yield return AskBoard(needs, true, 0, "What would help you right now?",
                    "Continue", "Skip", null, Preselect(needIds, needs));

                needIds.Clear(); needLabels.Clear();
                if (!boardSkipped)
                {
                    foreach (var c in lastSelection) { needIds.Add(c.id); needLabels.Add(c.label); }
                }

                // ---- step 5: the message
                SetStep(4, 5, "Step 5 · My Little Message");
                SetHint("This is your message. You choose to share it, keep it, or change it.");
                if (boardFrame != null) boardFrame.SetActive(false);

                var msg = new MyMessage
                {
                    emotion = emotionLabel,
                    intensity = intensityLabel,
                    reasons = new List<string>(reasonLabels),
                    needs = new List<string>(needLabels),
                    created = true
                };

                bool decided = false;
                bool changeRequested = false;

                if (messagePanel != null)
                    messagePanel.Show(msg.Compose(),
                        () => { decision = 1; msg.sharedWithNunu = true; msg.keptPrivate = false; decided = true; },
                        () => { decision = 2; msg.sharedWithNunu = false; msg.keptPrivate = true; decided = true; },
                        () => { changeRequested = true; decided = true; });

                while (!decided) yield return null;
                while (messagePanel != null && messagePanel.IsVisible) yield return null;

                if (changeRequested)
                {
                    edit = true;
                    continue;
                }

                ProgressManager.I?.SetMessage(msg);
                LogChoice("FeelingsBoard", decision == 1 ? "Shared" : "Private",
                          decision == 1 ? "shared" : "private");
                LogDone("FeelingsBoard");
            }

            ClearHint();
        }

        IEnumerator AboutNunuBranch()
        {
            yield return Say(new DialogueLine("nunu", "Nunu", "You want to talk about me instead? That is kind.", "listening"));

            var options = new[]
            {
                lib.Card("happy", "Happy"),
                lib.Card("sad", "Sad"),
                lib.Card("worried", "Worried"),
                lib.Card("angry", "Angry"),
            };

            yield return Ask(options, "How does Nunu feel?");
            LogChoice("FeelingsBoard", "AboutNunu_" + lastCard.Data.id, "about_nunu");

            if (lastCard.Data.id == "worried")
            {
                yield return Say(
                    new DialogueLine("nunu", "Nunu", "Yes... worried. The loud clouds make my chest feel tight.", "worried"),
                    new DialogueLine("pip", "Pip", "You noticed Nunu's feeling. That is empathy too.", "nodding"));
                Praise("Noticing others counts!");
            }
            else
            {
                yield return Say(
                    new DialogueLine("nunu", "Nunu", "Maybe. I feel many things at once.", "thinking"),
                    new DialogueLine("pip", "Pip", "Talking about a friend is a safe way to start.", "idle"));
            }
        }

        void SetStep(int index, int total, string label)
        {
            if (boardStep != null) boardStep.text = label;
            if (hud != null) hud.SetTask(index, total, "My Feelings Board");
            if (boardDots == null) return;
            for (int i = 0; i < boardDots.Length; i++)
            {
                if (boardDots[i] == null) continue;
                boardDots[i].gameObject.SetActive(i < total);
                boardDots[i].color = i < index ? MoodieUI.Teal
                                  : i == index ? MoodieUI.Peach
                                  : new Color(1f, 1f, 1f, 0.55f);
                boardDots[i].rectTransform.localScale = Vector3.one * (i == index ? 1.2f : 1f);
            }
        }

        // --------------------------------------------------------------- cards

        List<CardData> BuildEmotions() => new List<CardData>
        {
            lib.Card("happy", "Happy"),
            lib.Card("sad", "Sad"),
            lib.Card("angry", "Angry"),
            lib.Card("worried", "Worried"),
            lib.Card("calm", "Calm"),
            lib.Card("notsure", "Not sure"),
        };

        List<CardData> BuildIntensity() => new List<CardData>
        {
            lib.Card("little", "A little bit"),
            lib.Card("medium", "Medium"),
            lib.Card("alot", "A lot"),
        };

        List<CardData> BuildReasons() => new List<CardData>
        {
            lib.Card("loud", "The sound is too loud."),
            lib.Card("wrong", "Something went wrong."),
            lib.Card("waiting", "I have to wait."),
            lib.Card("changed", "Something changed."),
            lib.Card("tired", "I feel tired."),
            lib.Card("dontknow", "I don't know."),
        };

        List<CardData> BuildNeeds() => new List<CardData>
        {
            lib.Card("break", "I need a break."),
            lib.Card("quiet", "I want a quiet place."),
            lib.Card("help", "Please help me."),
            lib.Card("company", "I want someone with me."),
            lib.Card("space", "I need some space."),
            lib.Card("tryagain", "I want to try again."),
            lib.Card("unsure", "I'm not sure."),
        };

        static List<CardData> Preselect(string id, List<CardData> source)
        {
            var result = new List<CardData>();
            if (string.IsNullOrEmpty(id)) return result;
            var found = source.Find(c => c.id == id);
            if (found != null) result.Add(found);
            return result;
        }

        static List<CardData> Preselect(List<string> ids, List<CardData> source)
        {
            var result = new List<CardData>();
            foreach (var id in ids)
            {
                var found = source.Find(c => c.id == id);
                if (found != null) result.Add(found);
            }
            return result;
        }

        // --------------------------------------------------------------- finish

        IEnumerator Finish()
        {
            ClearHint();
            if (boardFrame != null) boardFrame.SetActive(false);

            if (aboutNunu)
            {
                yield return Say(
                    new DialogueLine("nunu", "Nunu", "Thank you for sitting with me. It feels quieter now.", "relieved"));
            }
            else if (decision == 1)
            {
                yield return Say(
                    new DialogueLine("nunu", "Nunu", "Thank you for telling me! Let's find a quiet place together.", "relieved"),
                    DialogueLine.Narrator("You sit together by the lake. The clouds slowly drift away."),
                    new DialogueLine("nunu", "Nunu", "I feel calmer now. Thank you for listening.", "calm"));
            }
            else if (decision == 2)
            {
                yield return Say(
                    new DialogueLine("pip", "Pip", "That's okay! Your message is yours.", "nodding"),
                    new DialogueLine("nunu", "Nunu", "You can keep it. I am glad you made it for yourself.", "listening"),
                    DialogueLine.Narrator("You fold the little message and put it in your pocket."));
            }

            yield return Say(new DialogueLine("pip", "Pip",
                "Naming a feeling, finding a reason and asking for what you need — that is a big step.", "happy"));

            ProgressManager.I?.CompleteLevel(2);
            yield return GiveBadge(BadgeId.Moon, "Moon Badge", "You expressed yourself in Cloudy Cove!", null);

            Info("Level 3 complete.");
            yield return Hold(0.6f);
            GameManager.I?.GoHarbor();
        }
    }
}
