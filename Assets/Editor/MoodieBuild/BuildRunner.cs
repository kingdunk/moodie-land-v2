using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace MoodieLand.EditorTools
{
    /// <summary>
    /// Entry points for rebuilding procedural art and all seven scenes.
    /// </summary>
    public static class BuildRunner
    {
        [MenuItem("Moodie Land/Rebuild Art", false, 20)]
        public static void RebuildArt()
        {
            ArtGen.GenerateAll();
            Debug.Log("[MoodieLand] Art regenerated.");
        }

        [MenuItem("Moodie Land/Rebuild All Scenes", false, 1)]
        public static void RebuildAll()
        {
            var result = EditorUtility.DisplayDialog(
                "Rebuild Moodie Land",
                "This regenerates procedural sprites and overwrites all 7 scenes.\n\nContinue?",
                "Rebuild", "Cancel");
            if (!result) return;

            BuildScenes.BuildAll();
            EditorSceneManager.OpenScene(BuildScenes.SceneDir + "/MainMenu.unity");
        }

        [MenuItem("Moodie Land/Open Main Menu Scene", false, 40)]
        public static void OpenMainMenu()
        {
            OpenScene("MainMenu");
        }

        // ---------------------------------------------------- quick scene switch

        static void OpenScene(string name)
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            string path = BuildScenes.SceneDir + "/" + name + ".unity";
            if (!System.IO.File.Exists(path))
            {
                EditorUtility.DisplayDialog("Moodie Land", "Scene not found:\n" + path, "OK");
                return;
            }
            EditorSceneManager.OpenScene(path);
        }

        [MenuItem("Moodie Land/Scenes/1 Main Menu", false, 100)] public static void S1() => OpenScene("MainMenu");
        [MenuItem("Moodie Land/Scenes/2 Story Intro", false, 101)] public static void S2() => OpenScene("StoryIntro");
        [MenuItem("Moodie Land/Scenes/3 Moodie Harbor", false, 102)] public static void S3() => OpenScene("MoodieHarbor");
        [MenuItem("Moodie Land/Scenes/4 Level 1 - Rainbow Forest", false, 103)] public static void S4() => OpenScene("Level01_RainbowForest");
        [MenuItem("Moodie Land/Scenes/5 Level 2 - Blocky Village", false, 104)] public static void S5() => OpenScene("Level02_BlockyVillage");
        [MenuItem("Moodie Land/Scenes/6 Level 3 - Cloudy Cove", false, 105)] public static void S6() => OpenScene("Level03_CloudyCove");
        [MenuItem("Moodie Land/Scenes/7 Game Ending", false, 106)] public static void S7() => OpenScene("GameEnding");

        [MenuItem("Moodie Land/Scenes/Open All Scenes Additively (inspect only)", false, 140)]
        public static void OpenAllAdditive()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            EditorSceneManager.OpenScene(BuildScenes.SceneDir + "/MainMenu.unity");
            for (int i = 1; i < BuildScenes.Order.Length; i++)
            {
                string path = BuildScenes.SceneDir + "/" + BuildScenes.Order[i] + ".unity";
                EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
            }
        }

        // Called from the command line:
        //   Unity.exe -batchmode -quit -projectPath <path> -executeMethod MoodieLand.EditorTools.BuildRunner.BuildFromCommandLine
        public static void BuildFromCommandLine()
        {
            BuildScenes.BuildAll();
            Debug.Log("[MoodieLand] Batch build finished.");
        }
    }
}
