using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MoodieLand.EditorTools
{
    /// <summary>Builds every playable scene from scratch.</summary>
    public static class BuildScenes
    {
        public const string SceneDir = "Assets/Scenes";

        public static readonly string[] Order =
        {
            "MainMenu", "StoryIntro", "MoodieHarbor",
            "Level01_RainbowForest", "Level02_BlockyVillage", "Level03_CloudyCove", "GameEnding"
        };

        public static void BuildAll()
        {
            ArtGen.GenerateAll();
            SceneKit.EnsureCardPrefab();
            Directory.CreateDirectory(SceneDir);

            MainMenu();
            StoryIntro();
            Harbor();
            Level1();
            Level2();
            Level3();
            Ending();

            ApplyBuildSettings();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[MoodieLand] All 7 scenes built.");
        }

        // ------------------------------------------------------------- utilities

        static Scene NewScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            return scene;
        }

        static void Save(Scene scene, string name)
        {
            string path = SceneDir + "/" + name + ".unity";
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, path);
        }

        static void ApplyBuildSettings()
        {
            var list = new List<EditorBuildSettingsScene>();
            foreach (var n in Order)
            {
                string path = SceneDir + "/" + n + ".unity";
                if (File.Exists(path)) list.Add(new EditorBuildSettingsScene(path, true));
            }
            EditorBuildSettings.scenes = list.ToArray();

            PlayerSettings.defaultScreenWidth = 1920;
            PlayerSettings.defaultScreenHeight = 1080;
            PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
            PlayerSettings.resizableWindow = true;
            PlayerSettings.companyName = "Moodie Land Team";
            PlayerSettings.productName = "Moodie Land";
            PlayerSettings.runInBackground = true;
        }

        static void WirePause(Kit kit)
        {
            if (kit.hud != null && kit.hud.pauseButton != null && kit.pause != null)
                kit.hud.pauseButton.onClick.AddListener(kit.pause.Pause);
        }

        // ------------------------------------------------------------- main menu

        static void MainMenu()
        {
            var scene = NewScene();
            var kit = SceneKit.Build("Backgrounds/bg_menu", "Moodie Land", new string[0], false);
            kit.root.name = "Canvas";

            kit.hud.gameObject.SetActive(false);
            kit.dialogue.gameObject.SetActive(false);
            kit.cards.gameObject.SetActive(false);
            kit.hint.gameObject.SetActive(false);
            kit.pause.gameObject.SetActive(false);

            var title = UIK.CenterLabel(kit.root.transform, "Title", "MOODIE LAND", 132, MoodieUI.Ink,
                new Vector2(0f, 296f), new Vector2(1500f, 190f), true);
            title.color = new Color(0.29f, 0.24f, 0.20f, 1f);

            var titleShadow = UIK.CenterLabel(kit.root.transform, "TitleShadow", "MOODIE LAND", 132,
                new Color(1f, 1f, 1f, 0.55f), new Vector2(4f, 288f), new Vector2(1500f, 190f), true);
            titleShadow.transform.SetSiblingIndex(title.transform.GetSiblingIndex());

            UIK.CenterLabel(kit.root.transform, "Subtitle", "A small adventure about big feelings", 32,
                MoodieUI.Teal, new Vector2(0f, 182f), new Vector2(1000f, 50f));

            var play = UIK.MakeButton(kit.root.transform, "PlayButton", "Begin the journey",
                ArtGen.Get("UI/ui_btn_peach"), Color.white, new Color(0.31f, 0.24f, 0.18f), 30,
                UIK.CC, new Vector2(0f, 46f), new Vector2(480f, 100f));

            var cont = UIK.MakeButton(kit.root.transform, "ContinueButton", "Continue",
                ArtGen.Get("UI/ui_btn_teal"), Color.white, Color.white, 27,
                UIK.CC, new Vector2(0f, -78f), new Vector2(480f, 82f));
            cont.gameObject.SetActive(false);   // shown again at runtime when there is save data

            var settings = UIK.MakeButton(kit.root.transform, "SettingsButton", "Settings",
                ArtGen.Get("UI/ui_btn_white"), Color.white, MoodieUI.Ink, 26,
                UIK.CC, new Vector2(0f, -180f), new Vector2(480f, 78f));

            var quit = UIK.MakeButton(kit.root.transform, "QuitButton", "Quit",
                ArtGen.Get("UI/ui_btn_white"), Color.white, MoodieUI.InkSoft, 26,
                UIK.CC, new Vector2(0f, -276f), new Vector2(480f, 78f));

            // earned badges, small, bottom centre
            var badges = new Image[3];
            string[] keys = { "Badges/badge_rainbow", "Badges/badge_star", "Badges/badge_moon" };
            for (int i = 0; i < 3; i++)
            {
                badges[i] = UIK.Img(kit.root.transform, "Badge" + i, ArtGen.Get(keys[i]), Color.white,
                    UIK.BC, new Vector2(-110f + i * 110f, 42f), new Vector2(88f, 88f), false, true);
                badges[i].gameObject.SetActive(false);
            }

            // shown only when there is a save, so a new journey is not a surprise wipe
            var note = UIK.Label(kit.root.transform, "NewGameNote",
                "Begin starts a new journey  路  saved badges will be cleared",
                21, MoodieUI.InkSoft, TextAnchor.MiddleCenter,
                UIK.BC, new Vector2(0f, 148f), new Vector2(900f, 34f));
            note.gameObject.SetActive(false);

            // floating friends
            var floaters = new List<CharacterView>();
            string[] friends = { "pip", "mimi", "bobo", "nunu" };
            float[] xs = { -770f, -460f, 460f, 770f };
            for (int i = 0; i < friends.Length; i++)
            {
                var v = SceneKit.AddCharacter(kit, friends[i], friends[i], new Vector2(xs[i], 24f),
                    new Vector2(290f, 290f), i >= 2);
                floaters.Add(v);
            }

            var ctrl = kit.root.AddComponent<MainMenuController>();
            ctrl.playButton = play;
            ctrl.continueButton = cont;
            ctrl.settingsButton = settings;
            ctrl.quitButton = quit;
            ctrl.settingsPanel = kit.settings;
            ctrl.newGameNote = note;
            ctrl.badgeIcons = badges;
            ctrl.badgeSprites = new[] { ArtGen.Get(keys[0]), ArtGen.Get(keys[1]), ArtGen.Get(keys[2]) };
            ctrl.floaters = floaters.ToArray();

            SceneKit.BringOverlaysToFront(kit);
            Save(scene, "MainMenu");
        }

        // ------------------------------------------------------------ story intro

        static void StoryIntro()
        {
            var scene = NewScene();
            var kit = SceneKit.Build("Backgrounds/bg_intro", "Moodie Land", new[] { "pip" }, false);

            kit.hud.gameObject.SetActive(false);
            kit.cards.gameObject.SetActive(false);
            kit.hint.gameObject.SetActive(false);

            var pip = SceneKit.AddCharacter(kit, "pip", "Pip", new Vector2(330f, 320f), new Vector2(370f, 370f), watercolor: true);
            SceneKit.RegisterCharacters(kit, kit.dialogue);

            var boat = UIK.Img(kit.props, "CloudBoat", ArtGen.Get("Props/prop_boat_watercolor"), Color.white,
                UIK.BC, new Vector2(-500f, 722f), new Vector2(300f, 300f), false, true);
            boat.transform.SetSiblingIndex(0);

            var startGroup = UIK.Node(kit.root.transform, "StartGroup", UIK.BC, new Vector2(-380f, 150f), new Vector2(460f, 110f));
            var group = UIK.Group(startGroup.gameObject, 0f, false);

            var start = UIK.MakeButton(startGroup, "StartButton", "Let's explore!", ArtGen.Get("UI/ui_btn_teal"),
                Color.white, Color.white, 30, UIK.BC, new Vector2(0f, 0f), new Vector2(440f, 100f));

            var ctrl = kit.root.AddComponent<StoryIntroController>();
            BindBase(ctrl, kit);
            ctrl.startButton = start;
            ctrl.startGroup = group;
            ctrl.boat = (RectTransform)boat.transform;

            SceneKit.BringOverlaysToFront(kit);
            Save(scene, "StoryIntro");
        }

        // ----------------------------------------------------------------- harbor

        static void Harbor()
        {
            var scene = NewScene();
            var kit = SceneKit.Build("Backgrounds/bg_harbor", "Moodie Harbor", new[] { "pip" }, false);

            var pip = SceneKit.AddCharacter(kit, "pip", "Pip", new Vector2(-300f, 344f), new Vector2(210f, 210f), watercolor: true, idleArt: "pip_waiting");
            SceneKit.RegisterCharacters(kit, kit.dialogue);

            var tree = UIK.Img(kit.props, "MoodTree", ArtGen.Get("Props/prop_moodtree_watercolor"), Color.white,
                UIK.BC, new Vector2(0f, 570f), new Vector2(340f, 340f), false, true);
            tree.transform.SetSiblingIndex(0);

            string[] portalKeys =
            {
                "Props/portal_forest_watercolor",
                "Props/portal_village_watercolor",
                "Props/portal_cove_watercolor"
            };
            string[] names = { "Rainbow Forest", "Blocky Village", "Cloudy Cove" };
            float[] xs = { -620f, 0f, 620f };

            var portals = new ClickableProp[3];
            var badges = new Image[3];
            var states = new Text[3];

            for (int i = 0; i < 3; i++)
            {
                portals[i] = SceneKit.AddClickable(kit.props, "Portal_" + names[i], portalKeys[i],
                    new Vector2(xs[i], 360f), new Vector2(320f, 380f), null);

                var label = UIK.Label(portals[i].transform, "Name", names[i], 28, MoodieUI.Ink,
                    TextAnchor.MiddleCenter, UIK.BC, new Vector2(0f, -28f), new Vector2(380f, 44f), true);
                label.color = new Color(0.29f, 0.24f, 0.20f, 1f);

                states[i] = UIK.Label(portals[i].transform, "State", "Not visited yet", 21, MoodieUI.InkSoft,
                    TextAnchor.MiddleCenter, UIK.BC, new Vector2(0f, -62f), new Vector2(380f, 34f));

                badges[i] = UIK.Img(portals[i].transform, "Badge", ArtGen.Get("Badges/badge_rainbow"), Color.white,
                    UIK.TR, new Vector2(-26f, -168f), new Vector2(92f, 92f), false, true);
                badges[i].gameObject.SetActive(false);

                portals[i].badgeSlot = badges[i].gameObject;
            }

            var progress = UIK.Label(kit.root.transform, "Progress", "0 of 3 places explored", 26, MoodieUI.Teal,
                TextAnchor.MiddleCenter, UIK.TC, new Vector2(0f, -58f), new Vector2(900f, 44f), true);

            var ending = UIK.MakeButton(kit.root.transform, "EndingButton", "Mood Tree (visit all 3 places)",
                ArtGen.Get("UI/ui_btn_sun"), Color.white, new Color(0.33f, 0.26f, 0.18f), 26,
                UIK.BC, new Vector2(0f, 140f), new Vector2(600f, 88f));
            var endingLabel = ending.GetComponentInChildren<Text>();

            var ctrl = kit.root.AddComponent<HarborController>();
            BindBase(ctrl, kit);
            ctrl.portals = portals;
            ctrl.portalBadges = badges;
            ctrl.portalStates = states;
            ctrl.badgeSprites = new[]
            {
                ArtGen.Get("Badges/badge_rainbow"),
                ArtGen.Get("Badges/badge_star"),
                ArtGen.Get("Badges/badge_moon")
            };
            ctrl.progressText = progress;
            ctrl.endingButton = ending;
            ctrl.endingLabel = endingLabel;
            ctrl.moodTree = (RectTransform)tree.transform;
            ctrl.lockedEndingGroup = new Button[0];

            SceneKit.BringOverlaysToFront(kit);
            Save(scene, "MoodieHarbor");
        }

        // ----------------------------------------------------------------- level 1

        static void Level1()
        {
            var scene = NewScene();
            var kit = SceneKit.Build("Backgrounds/bg_forest", "Rainbow Forest", new[] { "pip", "mimi" });

            var pip = SceneKit.AddCharacter(kit, "pip", "Pip", new Vector2(-660f, 236f), new Vector2(330f, 330f), watercolor: true);
            var mimi = SceneKit.AddCharacter(kit, "mimi", "Mimi", new Vector2(650f, 236f), new Vector2(330f, 330f), watercolor: true);
            SceneKit.RegisterCharacters(kit, kit.dialogue);

            // stage 0: the mushroom house, stage 1: fallen tower, stage 2: basket found
            var mushroom = SceneKit.AddProp(kit.props, "MushroomHouse", "Props/prop_mushroom",
                new Vector2(-60f, 236f), new Vector2(300f, 300f));

            var tower = SceneKit.AddProp(kit.props, "PineconeTower", "Props/prop_tower",
                new Vector2(-60f, 236f), new Vector2(300f, 300f));

            var basket = SceneKit.AddProp(kit.props, "BasketFound", "Props/prop_basketfull",
                new Vector2(-60f, 236f), new Vector2(300f, 300f));

            // three clickable clue cards
            var clueFace = SceneKit.AddClickable(kit.props, "Clue_Face", "Characters/NewWatercolor/clue_face",
                new Vector2(-470f, 566f), new Vector2(240f, 240f), "Face", new Vector2(0f, -34f));
            var clueBody = SceneKit.AddClickable(kit.props, "Clue_Body", "Characters/NewWatercolor/clue_body",
                new Vector2(0f, 566f), new Vector2(240f, 240f), "Body", new Vector2(0f, -34f));
            var clueSit = SceneKit.AddClickable(kit.props, "Clue_Situation", "Props/clue_situation",
                new Vector2(470f, 566f), new Vector2(240f, 240f), "Situation", new Vector2(0f, -34f));

            clueFace.interactable = false;
            clueBody.interactable = false;
            clueSit.interactable = false;

            // highlight frames for the clues
            AddHighlight(clueFace);
            AddHighlight(clueBody);
            AddHighlight(clueSit);

            var situation = UIK.Img(kit.props, "SituationCard", ArtGen.Get("Props/sit_1"), Color.white,
                UIK.BC, new Vector2(0f, 560f), new Vector2(520f, 350f), false, true);
            situation.gameObject.SetActive(false);

            var lib = kit.root.AddComponent<CardLibrary>();
            FillLibrary(lib);

            var ctrl = kit.root.AddComponent<Level01Controller>();
            BindBase(ctrl, kit);
            ctrl.lib = lib;
            ctrl.clueFace = clueFace;
            ctrl.clueBody = clueBody;
            ctrl.clueSituation = clueSit;
            ctrl.stageIntro = new[] { mushroom.gameObject };
            ctrl.stageTower = new[]
            {
                tower.gameObject, clueFace.gameObject, clueBody.gameObject, clueSit.gameObject
            };
            ctrl.stageFound = new[] { basket.gameObject };
            ctrl.situationImage = situation;
            ctrl.situationSprites = new[]
            {
                ArtGen.Get("Props/sit_1"), ArtGen.Get("Props/sit_2"), ArtGen.Get("Props/sit_3")
            };

            // only the first stage is visible in the editor; the controller swaps them
            tower.gameObject.SetActive(false);
            basket.gameObject.SetActive(false);
            clueFace.gameObject.SetActive(false);
            clueBody.gameObject.SetActive(false);
            clueSit.gameObject.SetActive(false);

            SceneKit.BringOverlaysToFront(kit);
            Save(scene, "Level01_RainbowForest");
        }

        // ----------------------------------------------------------------- level 2

        static void Level2()
        {
            var scene = NewScene();
            var kit = SceneKit.Build("Backgrounds/bg_village", "Blocky Village", new[] { "pip", "bobo" });

            var pip = SceneKit.AddCharacter(kit, "pip", "Pip", new Vector2(-700f, 232f), new Vector2(320f, 320f), watercolor: true);
            var bobo = SceneKit.AddCharacter(kit, "bobo", "Bobo", new Vector2(640f, 232f), new Vector2(340f, 340f), watercolor: true);
            SceneKit.RegisterCharacters(kit, kit.dialogue);

            var castle = SceneKit.AddProp(kit.props, "Castle", "Props/prop_castle",
                new Vector2(277f, 236f), new Vector2(320f, 320f));
            var broken = SceneKit.AddProp(kit.props, "CastleBroken", "Props/prop_castlebroken",
                new Vector2(-90f, 236f), new Vector2(320f, 320f));

            // Individual watercolor blocks in their bottom-to-top landing order.
            var blocks = new GameObject[6];
            Vector2[] blockPos =
            {
                new Vector2(-166f, 236f), new Vector2(-90f, 236f), new Vector2(-14f, 236f),
                new Vector2(-128f, 312f), new Vector2(-52f, 312f),
                new Vector2(-90f, 388f)
            };
            string[] blockColors = { "rose", "sage", "mauve", "apricot", "butter", "blue" };

            for (int i = 0; i < blockPos.Length; i++)
            {
                var sprite = ArtGen.Get("Props/prop_buildblock_" + blockColors[i]);
                var b = UIK.Img(kit.props, "BuildBlock" + i, sprite, Color.white,
                    UIK.BC, blockPos[i], new Vector2(96f, 96f), false, true);
                b.gameObject.SetActive(false);
                blocks[i] = b.gameObject;
            }

            var puzzlePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/UI/TaskTwoCastlePuzzle.prefab");
            var puzzle = (GameObject)PrefabUtility.InstantiatePrefab(puzzlePrefab, kit.props);

            var lib = kit.root.AddComponent<CardLibrary>();
            FillLibrary(lib);

            var ctrl = kit.root.AddComponent<Level02Controller>();
            BindBase(ctrl, kit);
            ctrl.lib = lib;
            ctrl.taskTwoPuzzle = puzzle.GetComponent<TaskTwoCastlePuzzle>();
            ctrl.stageCastle = new[] { castle.gameObject };
            ctrl.stageBroken = new[] { broken.gameObject };
            ctrl.castleBlocks = blocks;

            broken.gameObject.SetActive(false);

            SceneKit.BringOverlaysToFront(kit);
            Save(scene, "Level02_BlockyVillage");
        }

        // ----------------------------------------------------------------- level 3

        static void Level3()
        {
            var scene = NewScene();
            var kit = SceneKit.Build("Backgrounds/bg_cove", "Cloudy Cove", new[] { "pip", "nunu" });

            var pip = SceneKit.AddCharacter(kit, "pip", "Pip", new Vector2(-680f, 232f), new Vector2(320f, 320f), watercolor: true);
            var nunu = SceneKit.AddCharacter(kit, "nunu", "Nunu", new Vector2(620f, 232f), new Vector2(340f, 340f), watercolor: true);
            SceneKit.RegisterCharacters(kit, kit.dialogue);

            SceneKit.BuildBoardHeader(kit, kit.root.transform);
            kit.message = SceneKit.BuildMessage(kit.root.transform);

            // keep the board + message under the reward/pause overlays
            int layer = kit.badge.transform.GetSiblingIndex();
            kit.boardHeader.transform.SetSiblingIndex(layer);
            kit.message.transform.SetSiblingIndex(layer + 1);

            var lib = kit.root.AddComponent<CardLibrary>();
            FillLibrary(lib);

            var ctrl = kit.root.AddComponent<Level03Controller>();
            BindBase(ctrl, kit);
            ctrl.lib = lib;
            ctrl.messagePanel = kit.message;
            ctrl.boardFrame = kit.boardHeader;
            ctrl.boardTitle = kit.boardTitle;
            ctrl.boardStep = kit.boardStep;
            ctrl.boardDots = kit.boardDots;

            SceneKit.BringOverlaysToFront(kit);
            Save(scene, "Level03_CloudyCove");
        }

        // ------------------------------------------------------------------ ending

        static void Ending()
        {
            var scene = NewScene();
            var kit = SceneKit.Build("Backgrounds/bg_ending", "Moodie Harbor", new[] { "pip", "mimi", "bobo", "nunu" });

            var glow = UIK.CenterImg(kit.props, "TreeGlow", ArtGen.Get("UI/ui_glow"), new Color(1f, 0.96f, 0.78f, 0.2f),
                new Vector2(0f, 250f), new Vector2(650f, 650f));
            var tree = UIK.Img(kit.props, "MoodTree", ArtGen.Get("Props/prop_moodtree_watercolor"), Color.white,
                UIK.CC, new Vector2(0f, 180f), new Vector2(460f, 460f), false, true);

            var friends = new List<CharacterView>();
            string[] keys = { "pip", "mimi", "bobo", "nunu" };
            float[] anchors = { 0.06f, 0.152f, 0.848f, 0.94f };
            for (int i = 0; i < keys.Length; i++)
            {
                var friend = SceneKit.AddCharacter(kit, keys[i], keys[i], new Vector2(0f, 46f),
                    new Vector2(230f, 230f), watercolor: true);
                var rect = (RectTransform)friend.transform;
                var anchor = new Vector2(anchors[i], 0f);
                rect.anchorMin = rect.anchorMax = anchor;
                rect.anchoredPosition = new Vector2(0f, 46f);
                friends.Add(friend);
            }
            SceneKit.RegisterCharacters(kit, kit.dialogue);

            // explorer card
            var cardRoot = UIK.Full(kit.root.transform, "ExplorerCard");
            var cardGroup = UIK.Group(cardRoot.gameObject, 0f, false);
            var dim = cardRoot.gameObject.AddComponent<Image>();
            dim.color = new Color(0.16f, 0.14f, 0.18f, 0.72f);
            dim.raycastTarget = true;

            var card = UIK.Center(cardRoot, "Card", new Vector2(0f, 10f), new Vector2(1000f, 840f));
            var cardImg = card.gameObject.AddComponent<Image>();
            cardImg.sprite = ArtGen.Get("UI/ui_panel_white");
            cardImg.type = Image.Type.Sliced;
            cardImg.color = Color.white;

            UIK.Label(card, "CardTitle", "Moodie Explorer Card", 42, MoodieUI.Ink, TextAnchor.MiddleCenter,
                UIK.TC, new Vector2(0f, -40f), new Vector2(900f, 58f), true);

            var cardTitleText = card.Find("CardTitle").GetComponent<Text>();

            var badgeImgs = new Image[3];
            string[] badgeKeys = { "Badges/badge_rainbow", "Badges/badge_star", "Badges/badge_moon" };
            for (int i = 0; i < 3; i++)
            {
                badgeImgs[i] = UIK.Img(card, "CardBadge" + i, ArtGen.Get(badgeKeys[i]), Color.white,
                    UIK.CC, new Vector2(-190f + i * 190f, 224f), new Vector2(150f, 150f), false, true);
            }

            var msgBlock = UIK.Center(card, "MessageBlock", new Vector2(0f, 20f), new Vector2(880f, 250f));
            var msgBg = msgBlock.gameObject.AddComponent<Image>();
            msgBg.sprite = ArtGen.Get("UI/ui_panel_mint");
            msgBg.type = Image.Type.Sliced;
            msgBg.color = Color.white;

            var msgTitle = UIK.Label(msgBlock, "MsgTitle", "My Little Message  路  shared with Nunu", 21,
                MoodieUI.Teal, TextAnchor.UpperCenter, UIK.TC, new Vector2(0f, -16f), new Vector2(820f, 32f));
            var msgText = UIK.Label(msgBlock, "MsgText", "", 26, MoodieUI.Ink, TextAnchor.UpperCenter,
                UIK.TC, new Vector2(0f, -54f), new Vector2(820f, 180f));

            var playAgain = UIK.MakeButton(card, "PlayAgain", "Play Again", ArtGen.Get("UI/ui_btn_teal"),
                Color.white, Color.white, 25, UIK.BC, new Vector2(-330f, 180f), new Vector2(290f, 80f));
            var backHarbor = UIK.MakeButton(card, "BackToHarbor", "Back to Harbor", ArtGen.Get("UI/ui_btn_white"),
                Color.white, MoodieUI.Ink, 23, UIK.BC, new Vector2(0f, 180f), new Vector2(290f, 80f));
            var exit = UIK.MakeButton(card, "Exit", "Exit", ArtGen.Get("UI/ui_btn_white"),
                Color.white, MoodieUI.InkSoft, 23, UIK.BC, new Vector2(330f, 180f), new Vector2(290f, 80f));

            var ctrl = kit.root.AddComponent<EndingController>();
            BindBase(ctrl, kit);
            ctrl.cardGroup = cardGroup;
            ctrl.card = card;
            ctrl.cardBadges = badgeImgs;
            ctrl.badgeSprites = new[]
            {
                ArtGen.Get(badgeKeys[0]), ArtGen.Get(badgeKeys[1]), ArtGen.Get(badgeKeys[2])
            };
            ctrl.messageBlock = msgBlock.gameObject;
            ctrl.messageTitle = msgTitle;
            ctrl.messageText = msgText;
            ctrl.cardTitle = cardTitleText;
            ctrl.playAgainButton = playAgain;
            ctrl.backToHarborButton = backHarbor;
            ctrl.exitButton = exit;
            ctrl.moodTree = (RectTransform)tree.transform;
            ctrl.treeGlow = glow;
            ctrl.everyone = friends.ToArray();

            // pause / settings stay reachable on top of the modal card
            SceneKit.BringOverlaysToFront(kit);
            Save(scene, "GameEnding");
        }

        // ---------------------------------------------------------------- helpers

        static void BindBase(SceneBase ctrl, Kit kit)
        {
            ctrl.hud = kit.hud;
            ctrl.dialogue = kit.dialogue;
            ctrl.cards = kit.cards;
            ctrl.hint = kit.hint;
            ctrl.toast = kit.toast;
            ctrl.badge = kit.badge;
            ctrl.pause = kit.pause;
            ctrl.banner = kit.banner;
            ctrl.characters = kit.charViews.ToArray();
            WirePause(kit);
        }

        static void AddHighlight(ClickableProp prop)
        {
            var img = UIK.FullImg(prop.transform, "Highlight", ArtGen.Get("UI/ui_card_frame"), Color.white, true);
            var rt = img.rectTransform;
            rt.offsetMin = new Vector2(-10f, -10f);
            rt.offsetMax = new Vector2(10f, 10f);
            prop.highlight = img;
            img.gameObject.SetActive(false);
        }

        /// <summary>Registers every icon the card panels ask for.</summary>
        static void FillLibrary(CardLibrary lib)
        {
            lib.entries.Clear();

            void Add(string id, string spriteKey) =>
                lib.entries.Add(new CardLibrary.Entry { id = id, sprite = ArtGen.Get(spriteKey) });

            // emotions
            Add("happy", "Emotions/em_happy");
            Add("sad", "Emotions/em_sad");
            Add("angry", "Emotions/em_angry");
            Add("worried", "Emotions/em_worried");
            Add("calm", "Emotions/em_calm");
            Add("notsure", "Emotions/em_notsure");
            Add("frustrated", "Emotions/em_frustrated");
            Add("tired", "Emotions/em_tired");
            Add("excited", "Emotions/em_excited");
            Add("proud", "Emotions/em_proud");

            // level 2 communication cards
            Add("accept", "Icons/sym_speechcheck");
            Add("watch", "Icons/sym_eye");
            Add("later", "Icons/sym_clock");
            Add("decline", "Icons/sym_hand");
            Add("whendone", "Icons/sym_hourglass");
            Add("turns", "Icons/sym_turns");
            Add("another", "Icons/sym_block");
            Add("now", "Icons/sym_alert");
            Add("help", "Icons/sym_help");
            Add("myself", "Icons/sym_person");
            Add("break", "Icons/sym_pause");
            Add("here", "Icons/sym_company");

            // reasons
            Add("loud", "Icons/sym_speaker");
            Add("wrong", "Icons/sym_crack");
            Add("waiting", "Icons/sym_clock");
            Add("changed", "Icons/sym_changing");
            Add("dontknow", "Icons/sym_question");

            // intensity
            Add("little", "Icons/sym_quiet");
            Add("medium", "Icons/sym_speech");
            Add("alot", "Icons/sym_alert");

            // needs
            Add("quiet", "Icons/sym_quiet");
            Add("company", "Icons/sym_company");
            Add("space", "Icons/sym_space");
            Add("tryagain", "Icons/sym_loop");
            Add("unsure", "Icons/sym_question");
        }
    }
}
