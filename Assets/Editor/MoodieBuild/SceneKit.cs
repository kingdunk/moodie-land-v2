using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace MoodieLand.EditorTools
{
    /// <summary>Everything the per-scene builders need to hand to a controller.</summary>
    public class Kit
    {
        public GameObject root;
        public Canvas canvas;
        public RectTransform stage;
        public RectTransform characters;
        public RectTransform props;

        public HUDController hud;
        public DialogueController dialogue;
        public CardPanel cards;
        public HintController hint;
        public ToastController toast;
        public BadgeDisplay badge;
        public PauseController pause;
        public SettingsPanel settings;
        public TaskBanner banner;

        public MessagePanel message;
        public GameObject boardHeader;
        public Text boardTitle;
        public Text boardStep;
        public Image[] boardDots;

        public System.Collections.Generic.List<CharacterView> charViews =
            new System.Collections.Generic.List<CharacterView>();
    }

    /// <summary>Builds the shared UI stack used by every scene.</summary>
    public static class SceneKit
    {
        public const string CardPrefabPath = "Assets/Prefabs/UI/CardView.prefab";

        // --------------------------------------------------------------- prefabs

        public static void EnsureCardPrefab()
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(CardPrefabPath) != null) return;

            System.IO.Directory.CreateDirectory("Assets/Prefabs/UI");
            AssetDatabase.Refresh();

            var root = new GameObject("CardView", typeof(RectTransform), typeof(Image), typeof(Button), typeof(CardView));
            var rt = root.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(300f, 380f);

            var bg = root.GetComponent<Image>();
            bg.sprite = ArtGen.Get("UI/ui_card");
            bg.type = Image.Type.Sliced;
            bg.color = Color.white;

            var btn = root.GetComponent<Button>();
            btn.targetGraphic = bg;

            var icon = UIK.CenterImg(root.transform, "Icon", null, Color.white, new Vector2(0f, 46f), new Vector2(196f, 196f), true);
            icon.enabled = false;

            var label = UIK.Label(root.transform, "Label", "Card", 26, MoodieUI.Ink, TextAnchor.MiddleCenter,
                UIK.BC, new Vector2(0f, 22f), new Vector2(250f, 96f));
            label.fontStyle = FontStyle.Bold;

            var frame = UIK.FullImg(root.transform, "Frame", ArtGen.Get("UI/ui_card_frame"), Color.white, true);
            var frameRt = frame.rectTransform;
            frameRt.offsetMin = new Vector2(-6f, -6f);
            frameRt.offsetMax = new Vector2(6f, 6f);
            frame.gameObject.SetActive(false);

            var check = UIK.Img(root.transform, "Check", ArtGen.Get("UI/ui_check"), Color.white,
                UIK.TR, new Vector2(-14f, -14f), new Vector2(58f, 58f));
            check.gameObject.SetActive(false);

            var view = root.GetComponent<CardView>();
            view.button = btn;
            view.background = bg;
            view.icon = icon;
            view.label = label;
            view.selectionFrame = frame;
            view.checkMark = check.gameObject;

            PrefabUtility.SaveAsPrefabAsset(root, CardPrefabPath);
            Object.DestroyImmediate(root);
            AssetDatabase.Refresh();
        }

        public static CardView LoadCardPrefab() =>
            AssetDatabase.LoadAssetAtPath<GameObject>(CardPrefabPath)?.GetComponent<CardView>();

        // ------------------------------------------------------------ scene base

        public static Kit Build(string bgKey, string levelTitle, string[] charKeys, bool allowRestart = true)
        {
            var kit = new Kit();

            // ---- camera + audio listener
            var camGo = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
            camGo.tag = "MainCamera";
            var cam = camGo.GetComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 5.4f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.96f, 0.94f, 0.90f, 1f);
            cam.transform.position = new Vector3(0f, 0f, -10f);

            // ---- persistent managers
            var gameRoot = new GameObject("GameRoot", typeof(GameManager), typeof(ProgressManager),
                typeof(AudioManager), typeof(DataLogger));
            var fader = SceneFader.Create(gameRoot.transform);
            fader.transform.SetParent(gameRoot.transform, false);

            // ---- canvas
            var canvasGo = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.pixelPerfect = false;

            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            kit.canvas = canvas;

            // ---- event system
            CreateEventSystem();

            // ---- background
            var bg = UIK.FullImg(canvasGo.transform, "Background", ArtGen.Get(bgKey), Color.white);
            bg.raycastTarget = false;

            // ---- stage
            kit.stage = UIK.Full(canvasGo.transform, "Stage");
            kit.characters = UIK.Full(kit.stage, "Characters");
            kit.props = UIK.Full(kit.stage, "Props");

            // ---- HUD / panels
            kit.hud = BuildHud(canvasGo.transform, levelTitle);
            kit.dialogue = BuildDialogue(canvasGo.transform, kit.charViews);
            kit.cards = BuildCards(canvasGo.transform);
            kit.hint = BuildHint(canvasGo.transform);
            kit.toast = BuildToast(canvasGo.transform);
            kit.banner = BuildBanner(canvasGo.transform);
            kit.badge = BuildBadge(canvasGo.transform);
            kit.pause = BuildPause(canvasGo.transform, allowRestart);
            kit.settings = kit.pause.settingsPanel;

            // the HUD pause button is wired at runtime (editor-time listeners are not serialised)
            if (kit.pause != null) kit.pause.pauseButton = kit.hud != null ? kit.hud.pauseButton : null;

            kit.root = canvasGo;
            return kit;
        }

        public static void CreateEventSystem()
        {
            var es = new GameObject("EventSystem", typeof(UnityEngine.EventSystems.EventSystem));
            var moduleType = System.Type.GetType("UnityEngine.InputSystem.UI.InputSystemUIInputModule, Unity.InputSystem");
            if (moduleType != null) es.AddComponent(moduleType);
            else es.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        // ------------------------------------------------------------------- hud

        static HUDController BuildHud(Transform parent, string title)
        {
            var root = UIK.Full(parent, "HUD_Group");
            var hud = root.gameObject.AddComponent<HUDController>();
            hud.group = UIK.Group(root.gameObject, 1f, true);

            var back = UIK.MakeButton(root, "BackButton", "<", ArtGen.Get("UI/ui_circle_cream"), Color.white,
                MoodieUI.Ink, 40, UIK.TL, new Vector2(56f, -44f), new Vector2(76f, 76f));

            var pause = UIK.MakeButton(root, "PauseButton", "II", ArtGen.Get("UI/ui_circle_cream"), Color.white,
                MoodieUI.Ink, 26, UIK.TR, new Vector2(-56f, -44f), new Vector2(76f, 76f));

            var titleText = UIK.Label(root, "LevelTitle", title, 34, MoodieUI.Ink, TextAnchor.UpperLeft,
                UIK.TL, new Vector2(152f, -46f), new Vector2(680f, 50f), true);

            var taskText = UIK.Label(root, "TaskLabel", "Task 1", 24, MoodieUI.Teal, TextAnchor.UpperLeft,
                UIK.TL, new Vector2(152f, -96f), new Vector2(680f, 38f));

            var dots = new Image[3];
            for (int i = 0; i < 3; i++)
            {
                var d = UIK.Img(root, "Dot" + i, ArtGen.Get("UI/ui_dot"), Color.white,
                    UIK.TL, new Vector2(156f + i * 32f, -142f), new Vector2(18f, 18f));
                dots[i] = d;
            }

            hud.levelTitle = titleText;
            hud.taskLabel = taskText;
            hud.progressDots = dots;
            hud.backButton = back;
            hud.pauseButton = pause;
            return hud;
        }

        // -------------------------------------------------------------- dialogue

        static DialogueController BuildDialogue(Transform parent, System.Collections.Generic.List<CharacterView> views)
        {
            var root = UIK.Node(parent, "DialoguePanel", UIK.BC, new Vector2(0f, 24f), new Vector2(1660f, 282f));
            var group = UIK.Group(root.gameObject, 0f, false);

            var bg = root.gameObject.AddComponent<Image>();
            bg.sprite = ArtGen.Get("UI/ui_panel_cream");
            bg.type = Image.Type.Sliced;
            bg.color = Color.white;

            var btn = root.gameObject.AddComponent<Button>();
            btn.targetGraphic = bg;
            btn.transition = Selectable.Transition.None;

            // portrait
            var frameRt = UIK.Node(root, "PortraitFrame", UIK.TL, new Vector2(44f, -28f), new Vector2(214f, 214f));
            var frameImg = frameRt.gameObject.AddComponent<Image>();
            frameImg.sprite = ArtGen.Get("UI/ui_circle_cream");
            frameImg.color = new Color(0.94f, 0.92f, 0.86f, 1f);

            var portrait = UIK.Img(frameRt, "Portrait", null, Color.white,
                UIK.CC, Vector2.zero, new Vector2(184f, 184f), false, true);

            var nameText = UIK.Label(root, "NameText", "Pip", 30, MoodieUI.Teal, TextAnchor.UpperLeft,
                UIK.TL, new Vector2(300f, -26f), new Vector2(560f, 44f), true);

            var bodyText = UIK.Label(root, "BodyText", "", 30, MoodieUI.Ink, TextAnchor.UpperLeft,
                UIK.TL, new Vector2(300f, -78f), new Vector2(1290f, 170f));

            var hint = UIK.Label(root, "ContinueHint", "click to continue  >", 19,
                new Color(0.55f, 0.50f, 0.45f, 0.9f), TextAnchor.LowerRight,
                UIK.BR, new Vector2(-36f, 18f), new Vector2(360f, 32f));

            var ctrl = root.gameObject.AddComponent<DialogueController>();
            ctrl.group = group;
            ctrl.portrait = portrait;
            ctrl.nameText = nameText;
            ctrl.bodyText = bodyText;
            ctrl.advanceButton = btn;
            ctrl.continueHint = hint.gameObject;
            ctrl.portraitFrame = frameRt;

            // the frame image is hidden separately by the controller
            frameRt.gameObject.SetActive(true);
            return ctrl;
        }

        // ----------------------------------------------------------------- cards

        static CardPanel BuildCards(Transform parent)
        {
            var root = UIK.Full(parent, "ChoicePanel");
            var group = UIK.Group(root.gameObject, 0f, false);

            var title = UIK.Label(root, "Title", "", 32, MoodieUI.Ink, TextAnchor.MiddleCenter,
                UIK.TC, new Vector2(0f, -196f), new Vector2(1100f, 54f), true);

            var grid = UIK.Center(root, "Grid", new Vector2(0f, 26f), new Vector2(1660f, 440f));

            var prefab = LoadCardPrefab();

            var confirm = UIK.MakeButton(root, "ConfirmButton", "Continue", ArtGen.Get("UI/ui_btn_teal"),
                Color.white, Color.white, 27, UIK.BC, new Vector2(430f, 248f), new Vector2(320f, 88f));
            var confirmLabel = confirm.GetComponentInChildren<Text>();

            var skip = UIK.MakeButton(root, "SkipButton", "Skip", ArtGen.Get("UI/ui_btn_white"),
                Color.white, MoodieUI.InkSoft, 25, UIK.BC, new Vector2(-560f, 248f), new Vector2(300f, 80f));
            var skipLabel = skip.GetComponentInChildren<Text>();

            var extra = UIK.MakeButton(root, "ExtraButton", "About Nunu", ArtGen.Get("UI/ui_btn_white"),
                Color.white, MoodieUI.Teal, 23, UIK.BC, new Vector2(-160f, 248f), new Vector2(360f, 80f));
            var extraLabel = extra.GetComponentInChildren<Text>();

            var panel = root.gameObject.AddComponent<CardPanel>();
            panel.group = group;
            panel.grid = grid;
            panel.cardPrefab = prefab;
            panel.titleText = title;
            panel.confirmButton = confirm;
            panel.confirmLabel = confirmLabel;
            panel.skipButton = skip;
            panel.skipLabel = skipLabel;
            panel.extraButton = extra;
            panel.extraLabel = extraLabel;

            confirm.gameObject.SetActive(false);
            skip.gameObject.SetActive(false);
            extra.gameObject.SetActive(false);
            return panel;
        }

        // ------------------------------------------------------------------ hint

        static HintController BuildHint(Transform parent)
        {
            var root = UIK.Full(parent, "HintGroup");
            var ctrl = root.gameObject.AddComponent<HintController>();

            var btn = UIK.MakeButton(root, "HintButton", "Hint", ArtGen.Get("UI/ui_btn_sun"), Color.white,
                new Color(0.32f, 0.26f, 0.20f), 24, UIK.TR, new Vector2(-56f, -132f), new Vector2(176f, 60f));

            var bubble = UIK.Node(root, "HintBubble", UIK.TR, new Vector2(-56f, -206f), new Vector2(520f, 190f));
            var bubbleGroup = UIK.Group(bubble.gameObject, 0f, false);
            var bubbleImg = bubble.gameObject.AddComponent<Image>();
            bubbleImg.sprite = ArtGen.Get("UI/ui_panel_mint");
            bubbleImg.type = Image.Type.Sliced;
            bubbleImg.color = Color.white;
            bubbleImg.raycastTarget = true;

            var text = UIK.Label(bubble, "Text", "", 23, MoodieUI.Ink, TextAnchor.UpperLeft,
                UIK.TL, new Vector2(30f, -26f), new Vector2(456f, 120f));

            var close = UIK.MakeButton(bubble, "CloseButton", "Got it", ArtGen.Get("UI/ui_btn_white"), Color.white,
                MoodieUI.Teal, 21, UIK.BC, new Vector2(0f, 18f), new Vector2(170f, 46f));

            ctrl.hintButton = btn;
            ctrl.hintButtonLabel = btn.GetComponentInChildren<Text>();
            ctrl.bubble = bubbleGroup;
            ctrl.bubbleText = text;
            ctrl.bubbleClose = close;
            return ctrl;
        }

        // ----------------------------------------------------------------- toast

        static ToastController BuildToast(Transform parent)
        {
            var root = UIK.Node(parent, "Toast", UIK.BC, new Vector2(0f, 344f), new Vector2(900f, 78f));
            var group = UIK.Group(root.gameObject, 0f, false);

            var bg = root.gameObject.AddComponent<Image>();
            bg.sprite = ArtGen.Get("UI/ui_panel_mint");
            bg.type = Image.Type.Sliced;
            bg.color = Color.white;

            var text = UIK.Label(root, "Text", "", 26, MoodieUI.Ink, TextAnchor.MiddleCenter,
                UIK.CC, new Vector2(0f, 0f), new Vector2(840f, 64f));
            text.rectTransform.anchorMin = Vector2.zero;
            text.rectTransform.anchorMax = Vector2.one;
            text.rectTransform.offsetMin = new Vector2(28f, 8f);
            text.rectTransform.offsetMax = new Vector2(-28f, -8f);

            var ctrl = root.gameObject.AddComponent<ToastController>();
            ctrl.group = group;
            ctrl.label = text;
            ctrl.background = bg;
            ctrl.bubble = root;
            return ctrl;
        }

        // ---------------------------------------------------------------- banner

        static TaskBanner BuildBanner(Transform parent)
        {
            var root = UIK.Center(parent, "TaskBanner", new Vector2(0f, 130f), new Vector2(1100f, 200f));
            var group = UIK.Group(root.gameObject, 0f, false);

            var panel = UIK.Full(root, "Panel");
            var img = panel.gameObject.AddComponent<Image>();
            img.sprite = ArtGen.Get("UI/ui_panel_white");
            img.type = Image.Type.Sliced;
            img.color = Color.white;

            var title = UIK.Label(panel, "Title", "Task", 52, MoodieUI.Ink, TextAnchor.MiddleCenter,
                UIK.CC, new Vector2(0f, 26f), new Vector2(980f, 70f), true);
            var sub = UIK.Label(panel, "Subtitle", "", 26, MoodieUI.Teal, TextAnchor.MiddleCenter,
                UIK.CC, new Vector2(0f, -40f), new Vector2(980f, 44f));

            var ctrl = root.gameObject.AddComponent<TaskBanner>();
            ctrl.group = group;
            ctrl.titleText = title;
            ctrl.subtitleText = sub;
            ctrl.panel = panel;
            return ctrl;
        }

        // ----------------------------------------------------------------- badge

        static BadgeDisplay BuildBadge(Transform parent)
        {
            var root = UIK.Full(parent, "BadgeOverlay");
            var group = UIK.Group(root.gameObject, 0f, false);

            var dim = root.gameObject.AddComponent<Image>();
            dim.color = new Color(0.22f, 0.19f, 0.18f, 0.62f);
            dim.raycastTarget = true;

            var card = UIK.Center(root, "Card", Vector2.zero, new Vector2(820f, 760f));
            var cardImg = card.gameObject.AddComponent<Image>();
            cardImg.sprite = ArtGen.Get("UI/ui_panel_white");
            cardImg.type = Image.Type.Sliced;
            cardImg.color = Color.white;

            var glow = UIK.CenterImg(card, "Glow", ArtGen.Get("UI/ui_glow"), new Color(1f, 1f, 1f, 0.75f),
                new Vector2(0f, 176f), new Vector2(460f, 460f));

            var icon = UIK.CenterImg(card, "BadgeIcon", null, Color.white,
                new Vector2(0f, 176f), new Vector2(300f, 300f), true);

            var title = UIK.Label(card, "Title", "Badge", 44, MoodieUI.Ink, TextAnchor.MiddleCenter,
                UIK.CC, new Vector2(0f, -74f), new Vector2(720f, 62f), true);
            var sub = UIK.Label(card, "Subtitle", "", 26, MoodieUI.InkSoft, TextAnchor.MiddleCenter,
                UIK.CC, new Vector2(0f, -152f), new Vector2(700f, 96f));

            var btn = UIK.MakeButton(card, "ContinueButton", "Wonderful!", ArtGen.Get("UI/ui_btn_teal"),
                Color.white, Color.white, 28, UIK.BC, new Vector2(0f, 70f), new Vector2(380f, 92f));

            var ctrl = root.gameObject.AddComponent<BadgeDisplay>();
            ctrl.group = group;
            ctrl.dimmer = dim;
            ctrl.card = card;
            ctrl.badgeIcon = icon;
            ctrl.glow = glow;
            ctrl.titleText = title;
            ctrl.subtitleText = sub;
            ctrl.continueButton = btn;
            ctrl.continueLabel = btn.GetComponentInChildren<Text>();
            ctrl.rainbowBadge = ArtGen.Get("Badges/badge_rainbow");
            ctrl.starBadge = ArtGen.Get("Badges/badge_star");
            ctrl.moonBadge = ArtGen.Get("Badges/badge_moon");
            return ctrl;
        }

        // ----------------------------------------------------------------- pause

        static PauseController BuildPause(Transform parent, bool allowRestart)
        {
            // pause overlay first so settings (created after) draws on top of it
            var root = UIK.Full(parent, "PauseOverlay");
            var group = UIK.Group(root.gameObject, 0f, false);
            var dim = root.gameObject.AddComponent<Image>();
            dim.color = new Color(0.22f, 0.19f, 0.18f, 0.55f);
            dim.raycastTarget = true;

            var panel = UIK.Center(root, "Panel", Vector2.zero, new Vector2(700f, 720f));
            var pImg = panel.gameObject.AddComponent<Image>();
            pImg.sprite = ArtGen.Get("UI/ui_panel_cream");
            pImg.type = Image.Type.Sliced;
            pImg.color = Color.white;

            UIK.Label(panel, "Title", "Paused", 46, MoodieUI.Ink, TextAnchor.MiddleCenter,
                UIK.TC, new Vector2(0f, -40f), new Vector2(560f, 60f), true);
            UIK.Label(panel, "Sub", "Take your time.", 24, MoodieUI.InkSoft, TextAnchor.MiddleCenter,
                UIK.TC, new Vector2(0f, -96f), new Vector2(560f, 38f));

            var resume = UIK.MakeButton(panel, "Resume", "Resume", ArtGen.Get("UI/ui_btn_teal"), Color.white,
                Color.white, 28, UIK.BC, new Vector2(0f, 486f), new Vector2(460f, 88f));
            var restart = UIK.MakeButton(panel, "Restart", "Restart this scene", ArtGen.Get("UI/ui_btn_white"),
                Color.white, MoodieUI.Ink, 25, UIK.BC, new Vector2(0f, 386f), new Vector2(460f, 80f));
            var settingsBtn = UIK.MakeButton(panel, "SettingsButton", "Settings", ArtGen.Get("UI/ui_btn_white"),
                Color.white, MoodieUI.Ink, 25, UIK.BC, new Vector2(0f, 288f), new Vector2(460f, 80f));
            var harbor = UIK.MakeButton(panel, "Harbor", "Back to Moodie Harbor", ArtGen.Get("UI/ui_btn_white"),
                Color.white, MoodieUI.Teal, 24, UIK.BC, new Vector2(0f, 190f), new Vector2(460f, 80f));
            var menu = UIK.MakeButton(panel, "Menu", "Main menu", ArtGen.Get("UI/ui_btn_white"),
                Color.white, MoodieUI.InkSoft, 24, UIK.BC, new Vector2(0f, 92f), new Vector2(460f, 80f));

            // settings overlay (must be a later sibling so it can sit on top)
            var settingsRoot = UIK.Full(parent, "SettingsOverlay");
            var settingsGroup = UIK.Group(settingsRoot.gameObject, 0f, false);
            var settingsDim = settingsRoot.gameObject.AddComponent<Image>();
            settingsDim.color = new Color(0.22f, 0.19f, 0.18f, 0.55f);
            settingsDim.raycastTarget = true;

            var spanel = UIK.Center(settingsRoot, "Panel", Vector2.zero, new Vector2(900f, 880f));
            var spImg = spanel.gameObject.AddComponent<Image>();
            spImg.sprite = ArtGen.Get("UI/ui_panel_white");
            spImg.type = Image.Type.Sliced;
            spImg.color = Color.white;

            UIK.Label(spanel, "Title", "Settings", 40, MoodieUI.Ink, TextAnchor.MiddleCenter,
                UIK.TC, new Vector2(0f, -34f), new Vector2(600f, 54f), true);

            BuildSliderRow(spanel, "Music", "Music volume", new Vector2(0f, 250f), out var musicSlider, out var musicValue);
            BuildSliderRow(spanel, "Sound", "Sound effects", new Vector2(0f, 130f), out var sfxSlider, out var sfxValue);
            BuildSliderRow(spanel, "Speed", "Text speed", new Vector2(0f, 10f), out var speedSlider, out var speedValue);
            speedSlider.minValue = 20f;
            speedSlider.maxValue = 120f;
            speedSlider.value = 48f;

            UIK.Label(spanel, "Note",
                "Nothing here is timed. You can pause or skip any step.",
                21, MoodieUI.InkSoft, TextAnchor.MiddleCenter, UIK.CC, new Vector2(0f, -96f), new Vector2(760f, 40f));

            var reset = UIK.MakeButton(spanel, "ResetProgress", "Reset progress", ArtGen.Get("UI/ui_btn_white"),
                Color.white, MoodieUI.InkSoft, 23, UIK.BC, new Vector2(-200f, 200f), new Vector2(330f, 76f));
            var resetConfirm = UIK.MakeButton(spanel, "ResetConfirm", "Tap again to erase", ArtGen.Get("UI/ui_btn_peach"),
                Color.white, MoodieUI.Ink, 21, UIK.BC, new Vector2(200f, 200f), new Vector2(330f, 76f));

            var closeSettings = UIK.MakeButton(spanel, "CloseSettings", "Close", ArtGen.Get("UI/ui_btn_teal"),
                Color.white, Color.white, 27, UIK.BC, new Vector2(0f, 78f), new Vector2(300f, 84f));

            var settingsCtrl = settingsRoot.gameObject.AddComponent<SettingsPanel>();
            settingsCtrl.group = settingsGroup;
            settingsCtrl.musicSlider = musicSlider;
            settingsCtrl.sfxSlider = sfxSlider;
            settingsCtrl.textSpeedSlider = speedSlider;
            settingsCtrl.musicValue = musicValue;
            settingsCtrl.sfxValue = sfxValue;
            settingsCtrl.textSpeedValue = speedValue;
            settingsCtrl.closeButton = closeSettings;
            settingsCtrl.resetProgressButton = reset;
            settingsCtrl.confirmResetButton = resetConfirm;
            settingsCtrl.resetConfirm = resetConfirm.gameObject;
            resetConfirm.gameObject.SetActive(false);

            var ctrl = root.gameObject.AddComponent<PauseController>();
            ctrl.group = group;
            ctrl.pauseButton = null;
            ctrl.resumeButton = resume;
            ctrl.restartButton = restart;
            ctrl.settingsButton = settingsBtn;
            ctrl.harborButton = harbor;
            ctrl.menuButton = menu;
            ctrl.settingsPanel = settingsCtrl;
            ctrl.allowRestart = allowRestart;
            return ctrl;
        }

        static void BuildSliderRow(Transform panel, string name, string label, Vector2 pos,
            out Slider slider, out Text valueText)
        {
            var root = UIK.Node(panel, name + "Row", UIK.CC, pos, new Vector2(780f, 90f));

            UIK.Label(root, "Label", label, 26, MoodieUI.Ink, TextAnchor.MiddleLeft,
                UIK.CC, new Vector2(-240f, 20f), new Vector2(280f, 40f));

            slider = UIK.MakeSlider(root, name + "Slider",
                UIK.CC, new Vector2(60f, -16f), new Vector2(380f, 44f),
                new Color(0.91f, 0.87f, 0.81f, 1f), MoodieUI.Teal, ArtGen.Get("UI/ui_dot"));

            valueText = UIK.Label(root, "Value", "100%", 22, MoodieUI.InkSoft, TextAnchor.MiddleRight,
                UIK.CC, new Vector2(316f, -16f), new Vector2(140f, 36f));
        }

        // --------------------------------------------------------------- message

        public static MessagePanel BuildMessage(Transform parent)
        {
            var root = UIK.Full(parent, "MessageOverlay");
            var group = UIK.Group(root.gameObject, 0f, false);
            var dim = root.gameObject.AddComponent<Image>();
            dim.color = new Color(0.22f, 0.19f, 0.18f, 0.6f);
            dim.raycastTarget = true;

            var card = UIK.Center(root, "Card", Vector2.zero, new Vector2(1000f, 720f));
            var cardImg = card.gameObject.AddComponent<Image>();
            cardImg.sprite = ArtGen.Get("UI/ui_panel_white");
            cardImg.type = Image.Type.Sliced;
            cardImg.color = Color.white;

            var title = UIK.Label(card, "Title", "My Little Message", 40, MoodieUI.Ink, TextAnchor.MiddleCenter,
                UIK.TC, new Vector2(0f, -44f), new Vector2(800f, 56f), true);

            var body = UIK.Label(card, "Body", "", 32, MoodieUI.Ink, TextAnchor.UpperCenter,
                UIK.CC, new Vector2(0f, 40f), new Vector2(820f, 280f));
            body.lineSpacing = 1.25f;

            var footer = UIK.Label(card, "Footer", "You decide who sees this.", 22, MoodieUI.InkSoft,
                TextAnchor.MiddleCenter, UIK.BC, new Vector2(0f, 208f), new Vector2(700f, 36f));

            var share = UIK.MakeButton(card, "ShareButton", "Show my message to Nunu", ArtGen.Get("UI/ui_btn_teal"),
                Color.white, Color.white, 25, UIK.BC, new Vector2(0f, 130f), new Vector2(520f, 84f));
            var priv = UIK.MakeButton(card, "PrivateButton", "Keep my message private", ArtGen.Get("UI/ui_btn_white"),
                Color.white, MoodieUI.Ink, 23, UIK.BC, new Vector2(-250f, 40f), new Vector2(440f, 76f));
            var change = UIK.MakeButton(card, "ChangeButton", "Change my message", ArtGen.Get("UI/ui_btn_white"),
                Color.white, MoodieUI.Teal, 23, UIK.BC, new Vector2(250f, 40f), new Vector2(400f, 76f));

            var ctrl = root.gameObject.AddComponent<MessagePanel>();
            ctrl.group = group;
            ctrl.titleText = title;
            ctrl.bodyText = body;
            ctrl.footerText = footer;
            ctrl.shareButton = share;
            ctrl.privateButton = priv;
            ctrl.changeButton = change;
            ctrl.card = card;
            ctrl.cardBackground = cardImg;
            return ctrl;
        }

        // ----------------------------------------------------------------- board

        public static void BuildBoardHeader(Kit kit, Transform parent)
        {
            var root = UIK.Node(parent, "FeelingsBoardHeader", UIK.TC, new Vector2(0f, -186f), new Vector2(1120f, 130f));
            var img = root.gameObject.AddComponent<Image>();
            img.sprite = ArtGen.Get("UI/ui_panel_mint");
            img.type = Image.Type.Sliced;
            img.color = Color.white;

            var title = UIK.Label(root, "Title", "My Feelings Board", 34, MoodieUI.Ink, TextAnchor.MiddleCenter,
                UIK.TC, new Vector2(0f, -14f), new Vector2(1000f, 46f), true);
            var step = UIK.Label(root, "Step", "Step 1", 23, MoodieUI.Teal, TextAnchor.MiddleCenter,
                UIK.TC, new Vector2(0f, -60f), new Vector2(1000f, 34f));

            var dots = new Image[5];
            for (int i = 0; i < 5; i++)
            {
                var d = UIK.Img(root, "Step" + i, ArtGen.Get("UI/ui_dot"), Color.white,
                    UIK.TC, new Vector2(-56f + i * 28f, -100f), new Vector2(14f, 14f));
                dots[i] = d;
            }

            kit.boardHeader = root.gameObject;
            kit.boardTitle = title;
            kit.boardStep = step;
            kit.boardDots = dots;
            root.gameObject.SetActive(false);
        }

        // ------------------------------------------------------------ characters

        public static CharacterView AddCharacter(Kit kit, string key, string displayName, Vector2 pos, Vector2 size,
            bool faceLeft = false)
        {
            var go = new GameObject("Char_" + key, typeof(RectTransform), typeof(Image), typeof(CharacterView));
            go.transform.SetParent(kit.characters, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = UIK.BC;
            rt.anchorMax = UIK.BC;
            rt.pivot = UIK.BC;
            rt.sizeDelta = size;
            rt.anchoredPosition = pos;

            var img = go.GetComponent<Image>();
            img.preserveAspect = true;
            img.raycastTarget = false;

            var view = go.GetComponent<CharacterView>();
            view.characterKey = key;
            view.displayName = displayName;
            view.image = img;
            view.faceLeft = faceLeft;
            view.idleBob = true;

            foreach (var expr in ArtGen.ExpressionsFor(key))
            {
                view.expressions.Add(new ExpressionSprite
                {
                    name = expr,
                    sprite = ArtGen.Get("Characters/" + key + "/" + key + "_" + expr)
                });
                view.heads.Add(new ExpressionSprite
                {
                    name = expr,
                    sprite = ArtGen.Get("Characters/" + key + "/" + key + "_" + expr + "_head")
                });
            }

            // give the Image a visible sprite in the editor too (not only at runtime)
            var idle = view.GetSprite("idle");
            if (idle != null) img.sprite = idle;

            kit.charViews.Add(view);
            return view;
        }

        public static void RegisterCharacters(Kit kit, DialogueController dialogue)
        {
            dialogue.characters = kit.charViews.ToArray();
        }

        /// <summary>
        /// Overlays must be the last siblings of the canvas, otherwise anything a
        /// scene adds afterwards (titles, buttons, characters) draws on top of the
        /// pause / settings panel.
        /// </summary>
        public static void BringOverlaysToFront(Kit kit)
        {
            if (kit.badge != null) kit.badge.transform.SetAsLastSibling();
            if (kit.pause != null) kit.pause.transform.SetAsLastSibling();
            if (kit.settings != null) kit.settings.transform.SetAsLastSibling();
        }

        // ----------------------------------------------------------------- props

        public static Image AddProp(Transform parent, string name, string spriteKey, Vector2 pos, Vector2 size,
            bool preserve = true)
        {
            var img = UIK.Img(parent, name, ArtGen.Get(spriteKey), Color.white, UIK.BC, pos, size, false, preserve);
            return img;
        }

        public static ClickableProp AddClickable(Transform parent, string name, string spriteKey,
            Vector2 pos, Vector2 size, string caption = null, Vector2? captionPos = null)
        {
            var rt = UIK.Node(parent, name, UIK.BC, pos, size);
            var img = rt.gameObject.AddComponent<Image>();
            img.sprite = ArtGen.Get(spriteKey);
            img.preserveAspect = true;
            img.raycastTarget = true;

            var btn = rt.gameObject.AddComponent<Button>();
            btn.targetGraphic = img;

            var ctrl = rt.gameObject.AddComponent<ClickableProp>();
            ctrl.button = btn;
            ctrl.image = img;

            Text cap = null;
            if (!string.IsNullOrEmpty(caption))
            {
                var cp = captionPos ?? new Vector2(0f, -40f);
                cap = UIK.Label(rt, "Caption", caption, 22, MoodieUI.Ink, TextAnchor.MiddleCenter,
                    UIK.BC, cp, new Vector2(size.x + 40f, 64f));
            }
            ctrl.caption = cap;
            return ctrl;
        }
    }
}
