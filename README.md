# Moodie Land (version 2)

一款关于「认识情绪 / 社交沟通 / 自我表达」的 2D 选择式严肃游戏。
按 Moodie Land 设计清单实现的完整 MVP：7 个场景、3 个关卡、3 枚徽章、My Feelings Board 与匿名行为日志。

Unity 版本：**6000.6.2f1 (Unity 6.2)** ｜ 渲染管线：URP 2D ｜ 输入：Input System (New)
参考分辨率：**1920 × 1080**，横屏。

---

## 一、如何运行

1. 用 Unity Hub 打开本项目（`D:\Unity\Moodie land version 2`）。
2. 打开 `Assets/Scenes/MainMenu.unity`（一般会自动打开上次的场景）。
3. 点顶部 **Play**。

Build Settings 里 7 个场景已按顺序配好，直接 Build 即可打包。

快捷键：`Esc` 暂停／继续。所有界面元素都可鼠标点击，没有任何限时操作。

---

## 二、游戏流程

```
MainMenu → StoryIntro（云船抵达） → MoodieHarbor（中央地图）
                                        ├── Level 1 Rainbow Forest（Mimi）  → Rainbow Badge
                                        ├── Level 2 Blocky Village（Bobo）  → Star Badge
                                        └── Level 3 Cloudy Cove  （Nunu）   → Moon Badge
                                        └── 三关完成后解锁 → GameEnding（Moodie Explorer Card）
```

### 存档与重开

| 按钮 | 行为 |
|---|---|
| **Begin the journey** | 新周目：清空徽章 / 消息 / 港口开场标记，从 StoryIntro 重新开始（有存档时下方会出现小字提示） |
| **Continue** | 保留存档，直接回到 Moodie Harbor 继续（完成三关后可再次进入结局） |
| 结局卡 **Play Again** | 等同于新周目：清档后回到 StoryIntro |
| 结局卡 **Back to Harbor** | 保留存档回到港口，可重玩任意关卡或再看一次结局 |
| 结局卡 **Exit** | 退出游戏（编辑器里停止 Play） |
| 设置里的 **Reset progress** | 随时清档 |

港口开场白只在**第一次抵达**时播放；之后回到港口直接就是可点击的地图。

### Level 1 — Rainbow Forest（情绪识别）
| 任务 | 内容 | 交互 |
|---|---|---|
| Task 1 What's Happening? | Mimi 找不到松果篮 | 4 张情绪卡（Worried / Sad / Happy / Not sure），每种都有对应回应，答错会温和引导而不是卡住 |
| Task 2 Look for the Clues | 松果塔倒了 | 先选情绪，再点击 3 个线索卡（Face / Body / Situation），每个线索给出解释并高亮 |
| Task 3 Everyone Has Feelings | Mimi 找回篮子 | 3 个小情境各选一次情绪，给出解释 |

### Level 2 — Blocky Village（社交沟通）
| 任务 | 内容 | 分支 |
|---|---|---|
| Task 1 Would You Like to Play? | Bobo 邀请一起搭积木 | Accept / Watch first / Maybe later / No thank you，四条不同回应 |
| Task 2 Let's Take Turns! | 争用蓝色积木 | 4 种沟通方式；"I want it now!" 会触发一次协商重试；之后点积木 → 点城堡空位 |
| Task 3 Can I Help You? | 城堡倒了 | 帮忙 / 让他自己试 / 休息一下 / 我在旁边等你，四分支最后都回到同一完成节点 |

### Level 3 — Cloudy Cove（自我表达）
My Feelings Board 五步：Emotion → Intensity → Reason（多选）→ Need（多选）→ **My Little Message**。
第 1 步可以选 "I don't want to say"（跳过）或 "Let's talk about Nunu instead"（换成聊 Nunu，同样能通关）。
生成消息后有三个按钮：**Show my message to Nunu / Keep my message private / Change my message**。
跳过任何一步都不会出现空白句子（`MyMessage.Compose()` 会自动隐藏未选择的行）。

---

## 三、目录结构

```
Assets/
├── Art/                     # 全部由代码生成的 PNG（见第七节）
│   ├── Backgrounds/         # 7 张场景背景
│   ├── Characters/pip|mimi|bobo|nunu/   # 每角色 6 个表情（全身 + 头像）
│   ├── Emotions/            # 情绪图标
│   ├── Icons/               # 沟通 / 原因 / 需求符号图标
│   ├── Badges/              # Rainbow / Star / Moon
│   ├── Props/               # 树、篮、积木、传送门、线索卡…
│   └── UI/                  # 九宫格面板、按钮、卡片、光晕…
├── Prefabs/UI/CardView.prefab   # 通用选择卡片
├── Scenes/                  # 7 个场景
├── Scripts/
│   ├── Core/                # GameManager, ProgressManager, AudioManager, DataLogger, UIFx, SceneFader
│   ├── UI/                  # Dialogue, CardPanel, Hint, Pause, Settings, Badge, Toast, Message…
│   ├── Characters/          # CharacterView（表情 + 反馈动画）
│   ├── Levels/              # Level01/02/03, Harbor, MainMenu, StoryIntro, Ending, SceneBase
│   └── Data/                # CardLibrary（图标查表）
└── Editor/MoodieArt, Editor/MoodieBuild   # 美术生成器 + 场景生成器
```

场景内层级（以 Level01 为例）：

```
Main Camera
GameRoot          ← GameManager / ProgressManager / AudioManager / DataLogger（跨场景单例）
Canvas            ← CanvasScaler 1920×1080
├── Background
├── Stage
│   ├── Characters  (Char_pip, Char_mimi)
│   └── Props       (MushroomHouse / PineconeTower / BasketFound / Clue_* / SituationCard)
├── HUD_Group       (BackButton, PauseButton, LevelTitle, TaskLabel, Dot0-2)
├── DialoguePanel   (PortraitFrame/Portrait, NameText, BodyText, ContinueHint)
├── ChoicePanel     (Title, Grid, Confirm/Skip/Extra Button)  ← 卡片运行时实例化
├── HintGroup       (HintButton, HintBubble)
├── Toast / TaskBanner
├── BadgeOverlay / PauseOverlay / SettingsOverlay
└── Level01Controller  ← 关卡流程与全部关卡文本
```

---

## 四、在 Inspector 里改东西（重点）

| 想改什么 | 改哪里 |
|---|---|
| **角色长相 / 换图** | 选中 `Canvas/Stage/Characters/Char_pip`，`CharacterView.expressions` 列表里换 Sprite；`Image` 上的 Sprite 是默认表情 |
| **角色位置 / 大小** | 直接拖 `RectTransform`（锚点为底边中心，Y 是从画面底部算起的高度） |
| **角色说话时是否上下浮动** | `CharacterView.idleBob / bobAmplitude / bobSpeed` |
| **界面上的固定文字**（标题、按钮、提示语） | 选中对应 `Text` 组件，例如 `HUD_Group/LevelTitle`、`ChoicePanel/ConfirmButton/Label` |
| **对话框外观** | `DialoguePanel` 的 Image（九宫格 Sprite）、字号在 `BodyText/NameText` 上 |
| **卡片图标** | `CardLibrary` 组件的 entries（id → Sprite），改一张卡片的图标只需换这里 |
| **徽章图** | `BadgeOverlay` 组件的 `rainbowBadge / starBadge / moonBadge` |
| **背景** | `Canvas/Background` 的 Image Sprite |
| **文字速度 / 音量默认值** | `DialogueController.charsPerSecond`；音量存在 PlayerPrefs，可在游戏内 Settings 面板调 |
| **关卡台词 / 分支文案** | `Level01Controller` / `Level02Controller` / `Level03Controller` 里的 `DialogueLine(...)`，顺序即播放顺序 |
| **关卡提示语** | 控制器里的 `SetHint("…")` |

> 说明：对话框台词写在关卡脚本里（`Say(new DialogueLine("pip", "Pip", "…", "happy"))`），
> 这样分支逻辑和文案在同一处、便于按流程顺序阅读。界面上的静态文字则全部是场景里的 Text 组件，可直接在 Inspector 改。

---

## 五、系统说明

- **GameManager**：场景跳转 + 淡入淡出；`DontDestroyOnLoad` 单例，重复实例自动销毁（不会出现两个 GameManager）。
- **ProgressManager**：三关完成状态、徽章、玩家消息，存 `PlayerPrefs`。
- **AudioManager**：运行时合成音乐（八音盒循环）与音效（点击/选择/成功/徽章/提示），**不依赖任何音频文件**；音量可在 Settings 调。
- **DataLogger**：匿名行为日志，写到
  `%USERPROFILE%\AppData\LocalLow\Moodie Land Team\Moodie Land\MoodieLogs\`
  - `moodie_sessions.csv`：`session_id,level_id,task_id,first_choice,final_choice,hint_used,branch_id,response_time,task_completed`
  - `moodie_events.jsonl`：每个事件一行 JSON
  - **不记录** My Little Message 原文、姓名、邮箱等任何个人信息，只记 `Shared / Private` 等行为。
- **UIFx**：所有反馈动画（卡片弹入、徽章弹出、文字轻微弹跳、船漂浮…）都在这里。

---

## 六、无障碍

- 所有按钮 ≥ 76 px，正文字号 26–32（1920 基准）。
- 无倒计时、无闪烁、无强制快速操作。
- 每个任务都有 Hint 按钮，可无限次重看。
- 情绪 / 原因 / 需求都可跳过；Level 3 可以不回答自己的感受照样通关。
- 文字速度可调，音乐与音效可分别静音。
- 随时可暂停、重开本场景、回地图或回主菜单。

---

## 七、重新生成美术与场景

菜单栏 **Moodie Land → Rebuild Art** / **Rebuild All Scenes**。
所有 PNG 由 `Editor/MoodieArt` 里的代码绘制（`Canvas2D` 是自写的矢量光栅器），
场景由 `Editor/MoodieBuild/BuildScenes.cs` 生成。`Rebuild Art` 会生成程序绘制的素材，并保留 `Assets/Art/Characters` 中的角色原画。

命令行等价方式：

```
Unity.exe -batchmode -quit -projectPath "<项目路径>" ^
  -executeMethod MoodieLand.EditorTools.BuildRunner.BuildFromCommandLine
```

> 注意：Rebuild All Scenes 会**覆盖**这 7 个场景文件（Inspector 里手改的内容会丢失），
> 想保留手改请先备份场景，或只用 Rebuild Art。

---

## 八、已知取舍

- 角色用「表情换图 + 程序化小动画」而不是 Animator 状态机（设计清单里明确允许 sprite 替换方案），
  好处是改表情只需换图，坏处是没有逐帧行走动画。
- 卡片与对话框在运行时由脚本驱动，卡片内容（id/文案/图标）在脚本与 CardLibrary 中；
  若要做成策划可配的表格，下一步可以抽成 ScriptableObject。
- 音频为程序合成，风格统一但不如真实音源精致。
