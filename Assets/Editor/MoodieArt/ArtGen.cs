using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace MoodieLand.EditorTools
{
    /// <summary>
    /// Generates procedural PNGs and imports them with the right sprite settings.
    /// Supplied artwork is kept alongside the procedural assets.
    /// </summary>
    public static class ArtGen
    {
        public const string ArtRoot = "Assets/Art";

        static readonly Dictionary<string, Sprite> cache = new Dictionary<string, Sprite>();

        public static Sprite Get(string key)
        {
            if (cache.TryGetValue(key, out var s) && s != null) return s;
            string path = FindPath(key);
            s = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (s == null)
            {
                // the asset may not have been imported yet in this frame
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
                s = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            }
            if (s != null) cache[key] = s;
            return s;
        }

        static string FindPath(string key)
        {
            if (key.Contains("/")) return ArtRoot + "/" + key + ".png";
            string[] dirs = { "UI", "Props", "Badges", "Emotions", "Icons", "Backgrounds" };
            foreach (var d in dirs)
            {
                string p = ArtRoot + "/" + d + "/" + key + ".png";
                if (File.Exists(p)) return p;
            }
            return ArtRoot + "/" + key + ".png";
        }

        public static void ClearCache() => cache.Clear();

        // ------------------------------------------------------------------ main

        public static void GenerateAll(bool verbose = true)
        {
            ClearCache();
            Directory.CreateDirectory(ArtRoot);

            if (verbose) Debug.Log("[MoodieLand] Generating backgrounds...");
            Backgrounds();


            if (verbose) Debug.Log("[MoodieLand] Generating props...");
            Props();

            if (verbose) Debug.Log("[MoodieLand] Generating badges and icons...");
            Icons();

            if (verbose) Debug.Log("[MoodieLand] Generating UI surfaces...");
            Surfaces();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[MoodieLand] Art ready in " + ArtRoot);
        }

        // ------------------------------------------------------------ generation

        static void Backgrounds()
        {
            Save("Backgrounds/bg_menu", () => { var c = new Canvas2D(1280, 720, 2); SceneArt.Menu(c); return c; });
            Save("Backgrounds/bg_intro", () => { var c = new Canvas2D(1280, 720, 2); SceneArt.Intro(c); return c; });
            Save("Backgrounds/bg_harbor", () => { var c = new Canvas2D(1280, 720, 2); SceneArt.Harbor(c); return c; });
            Save("Backgrounds/bg_forest", () => { var c = new Canvas2D(1280, 720, 2); SceneArt.Forest(c); return c; });
            Save("Backgrounds/bg_village", () => { var c = new Canvas2D(1280, 720, 2); SceneArt.Village(c); return c; });
            Save("Backgrounds/bg_cove", () => { var c = new Canvas2D(1280, 720, 2); SceneArt.Cove(c); return c; });
            Save("Backgrounds/bg_ending", () => { var c = new Canvas2D(1280, 720, 2); SceneArt.Ending(c); return c; });
        }

        static readonly string[] PipExpressions = { "idle", "pointing", "waving", "thinking", "happy", "calm" };
        static readonly string[] MimiExpressions = { "idle", "searching", "worried", "frustrated", "happy", "calm" };
        static readonly string[] BoboExpressions = { "idle", "building", "listening", "happy", "calm", "disappointed" };
        static readonly string[] NunuExpressions = { "idle", "listening", "thinking", "worried", "relieved", "calm" };

        public static string[] ExpressionsFor(string key)
        {
            switch (key)
            {
                case "mimi": return MimiExpressions;
                case "bobo": return BoboExpressions;
                case "nunu": return NunuExpressions;
                default: return PipExpressions;
            }
        }

        static void Props()
        {
            Save("Props/prop_moodtree", () => { var c = new Canvas2D(600, 600, 2); SceneArt.MoodTree(c); return c; });
            Save("Props/prop_boat", () => { var c = new Canvas2D(520, 520, 2); SceneArt.CloudBoat(c); return c; });
            Save("Props/prop_basket", () => { var c = new Canvas2D(420, 420, 2); SceneArt.Basket(c, false); return c; });
            Save("Props/prop_basketfull", () => { var c = new Canvas2D(420, 420, 2); SceneArt.Basket(c, true); return c; });
            Save("Props/prop_tower", () => { var c = new Canvas2D(420, 420, 2); SceneArt.PineconeTower(c, false); return c; });
            Save("Props/prop_towerfallen", () => { var c = new Canvas2D(420, 420, 2); SceneArt.PineconeTower(c, true); return c; });
            Save("Props/prop_castle", () => { var c = new Canvas2D(420, 420, 2); SceneArt.BlockCastle(c, false); return c; });
            Save("Props/prop_castlebroken", () => { var c = new Canvas2D(420, 420, 2); SceneArt.BlockCastle(c, true); return c; });
            Save("Props/prop_blockblue", () => { var c = new Canvas2D(300, 300, 2); SceneArt.BlueBlock(c); return c; });
            Save("Props/prop_mushroom", () => { var c = new Canvas2D(320, 320, 2); SceneArt.Mushroom(c); return c; });
            Save("Props/prop_flower", () => { var c = new Canvas2D(160, 200, 2); SceneArt.Flower(c, 80f, 70f, 52f, Pal.PinkLight, Pal.Sun); return c; });
            Save("Props/portal_forest", () => { var c = new Canvas2D(420, 480, 2); SceneArt.Portal(c, "forest"); return c; });
            Save("Props/portal_village", () => { var c = new Canvas2D(420, 480, 2); SceneArt.Portal(c, "village"); return c; });
            Save("Props/portal_cove", () => { var c = new Canvas2D(420, 480, 2); SceneArt.Portal(c, "cove"); return c; });
            GenerateMimiClues();
            Save("Props/clue_situation", () => { var c = new Canvas2D(420, 420, 2); SceneArt.ClueSituation(c); return c; });
            for (int i = 0; i < 3; i++)
            {
                int idx = i;
                Save("Props/sit_" + (i + 1), () => { var c = new Canvas2D(560, 380, 2); SceneArt.Situation(c, idx); return c; });
            }
        }

        internal static void GenerateMimiClues()
        {
            const string mimi = "Characters/mimi/mimi_frustrated";
            Save("Props/clue_face", () =>
            {
                var canvas = new Canvas2D(420, 420, 2);
                SceneArt.ClueFace(canvas, FindPath(mimi + "_head"));
                return canvas;
            });
            Save("Props/clue_body", () =>
            {
                var canvas = new Canvas2D(420, 420, 2);
                SceneArt.ClueBody(canvas, FindPath(mimi));
                return canvas;
            });
        }

        static void Icons()
        {
            // Preserve the supplied badge art. The procedural sprites are fallbacks for missing files.
            if (!File.Exists(FindPath("Badges/badge_rainbow")))
                Save("Badges/badge_rainbow", () => { var c = new Canvas2D(360, 360, 2); UIAssets.BadgeRainbow(c); return c; });
            if (!File.Exists(FindPath("Badges/badge_star")))
                Save("Badges/badge_star", () => { var c = new Canvas2D(360, 360, 2); UIAssets.BadgeStar(c); return c; });
            if (!File.Exists(FindPath("Badges/badge_moon")))
                Save("Badges/badge_moon", () => { var c = new Canvas2D(360, 360, 2); UIAssets.BadgeMoon(c); return c; });

            string[] emotions = { "happy", "sad", "angry", "worried", "calm", "notsure", "frustrated", "tired", "excited", "proud" };
            foreach (var e in emotions)
            {
                string id = e;
                string key = "Emotions/em_" + id;
                if (!File.Exists(FindPath(key)))
                    Save(key, () => { var c = new Canvas2D(256, 256, 2); UIAssets.EmotionIcon(c, id); return c; });
            }

            string[] symbols =
            {
                "speech", "speechcheck", "eye", "clock", "hand", "hourglass", "turns", "block", "alert",
                "person", "pause", "moon", "help", "heart", "company", "space", "loop", "question",
                "speaker", "crack", "changing", "quiet"
            };
            foreach (var s in symbols)
            {
                string id = s;
                Save("Icons/sym_" + id, () => { var c = new Canvas2D(256, 256, 2); UIAssets.SymbolIcon(c, id, Pal.Lav); return c; });
            }
        }

        static void Surfaces()
        {
            // main dialogue / board panels (9-sliced, border 80)
            Save("UI/ui_panel_cream", () => { var c = new Canvas2D(256, 256, 2); UIAssets.Panel(c, Pal.Cream, Canvas2D.Hex("E9DAC6"), 46f, 6f, true); return c; }, 80);
            Save("UI/ui_panel_white", () => { var c = new Canvas2D(256, 256, 2); UIAssets.Panel(c, Pal.Paper, Canvas2D.Hex("EEE2D2"), 46f, 6f, true); return c; }, 80);
            Save("UI/ui_panel_mint", () => { var c = new Canvas2D(256, 256, 2); UIAssets.Panel(c, Canvas2D.Hex("F0FBF7"), Canvas2D.Hex("C9E9E2"), 46f, 6f, true); return c; }, 80);
            Save("UI/ui_panel_lav", () => { var c = new Canvas2D(256, 256, 2); UIAssets.Panel(c, Canvas2D.Hex("F6F4FC"), Canvas2D.Hex("DCD6F0"), 46f, 6f, true); return c; }, 80);

            // buttons
            Save("UI/ui_btn_peach", () => { var c = new Canvas2D(256, 128, 2); UIAssets.Panel(c, Pal.Peach, Pal.PeachDark, 34f, 5f, false); return c; }, 56);
            Save("UI/ui_btn_teal", () => { var c = new Canvas2D(256, 128, 2); UIAssets.Panel(c, Canvas2D.Hex("399C91"), Canvas2D.Hex("2C7A72"), 34f, 5f, false); return c; }, 56);
            Save("UI/ui_btn_white", () => { var c = new Canvas2D(256, 128, 2); UIAssets.Panel(c, Pal.White, Canvas2D.Hex("E6D9C7"), 34f, 5f, false); return c; }, 56);
            Save("UI/ui_btn_sun", () => { var c = new Canvas2D(256, 128, 2); UIAssets.Panel(c, Canvas2D.Hex("FFE08A"), Canvas2D.Hex("E9C368"), 34f, 5f, false); return c; }, 56);
            Save("UI/ui_btn_ghost", () => { var c = new Canvas2D(256, 128, 2); UIAssets.Panel(c, Canvas2D.Hex("FFFFFF", 0.55f), Canvas2D.Hex("FFFFFF", 0.85f), 34f, 4f, false); return c; }, 56);

            // cards
            Save("UI/ui_card", () => { var c = new Canvas2D(300, 300, 2); UIAssets.CardSurface(c, false); return c; }, 64);
            Save("UI/ui_card_frame", () => { var c = new Canvas2D(300, 300, 2); UIAssets.SelectionFrame(c); return c; }, 64);

            // misc
            Save("UI/ui_circle", () => { var c = new Canvas2D(256, 256, 2); UIAssets.SoftCircle(c, Pal.White); return c; }, 0);
            Save("UI/ui_circle_cream", () => { var c = new Canvas2D(256, 256, 2); UIAssets.SoftCircle(c, Pal.Cream); return c; }, 0);
            Save("UI/ui_glow", () => { var c = new Canvas2D(256, 256, 2); UIAssets.GlowRing(c, Pal.Sun); return c; }, 0);
            Save("UI/ui_check", () => { var c = new Canvas2D(128, 128, 2); UIAssets.CheckMark(c, Pal.Teal); return c; }, 0);
            Save("UI/ui_dot", () => { var c = new Canvas2D(64, 64, 2); c.Circle(32f, 32f, 28f, Pal.White); return c; }, 0);
            Save("UI/ui_vignette", () =>
            {
                var c = new Canvas2D(512, 512, 1);
                c.Clear(new Color32(0, 0, 0, 0));
                return c;
            }, 0);
        }

        // ------------------------------------------------------------------ save

        delegate Canvas2D CanvasFactory();

        static void Save(string relative, CanvasFactory factory, int border = 0, float ppu = 100f)
        {
            string assetPath = ArtRoot + "/" + relative + ".png";
            string absolute = Path.GetFullPath(Path.Combine(Application.dataPath, "..", assetPath));
            Directory.CreateDirectory(Path.GetDirectoryName(absolute) ?? ".");

            var canvas = factory();
            canvas.Save(absolute);
            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);

            var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.spritePixelsPerUnit = ppu;
                importer.filterMode = FilterMode.Bilinear;
                importer.wrapMode = TextureWrapMode.Clamp;
                importer.alphaIsTransparency = true;
                importer.mipmapEnabled = false;
                importer.maxTextureSize = 2048;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.spriteBorder = border > 0
                    ? new Vector4(border, border, border, border)
                    : Vector4.zero;
                importer.SaveAndReimport();
            }

            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
            if (sprite != null) cache[relative] = sprite;
        }
    }
}
