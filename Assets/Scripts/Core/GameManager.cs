using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MoodieLand
{
    /// <summary>
    /// Persistent global game state and scene routing.
    /// One instance only: duplicated copies destroy themselves on Awake.
    /// </summary>
    [DisallowMultipleComponent]
    public class GameManager : MonoBehaviour
    {
        public static GameManager I { get; private set; }

        [Header("Scene Names")]
        public string mainMenuScene = "MainMenu";
        public string storyIntroScene = "StoryIntro";
        public string harborScene = "MoodieHarbor";
        public string endingScene = "GameEnding";
        public string[] levelScenes =
        {
            "Level01_RainbowForest",
            "Level02_BlockyVillage",
            "Level03_CloudyCove"
        };

        [Header("Transition")]
        [Range(0.1f, 2f)] public float fadeDuration = 0.45f;

        public bool IsLoading { get; private set; }
        public int CurrentLevelIndex { get; private set; } = -1;

        SceneFader fader;

        void Awake()
        {
            if (I != null && I != this)
            {
                Destroy(gameObject);
                return;
            }

            I = this;
            DontDestroyOnLoad(gameObject);
            fader = GetComponentInChildren<SceneFader>(true);
            if (fader == null) fader = SceneFader.Create(transform);
        }

        void OnDestroy()
        {
            if (I == this) I = null;
        }

        // ---------------------------------------------------------------- routing

        public void GoMainMenu() => LoadScene(mainMenuScene);

        public void GoStoryIntro() => LoadScene(storyIntroScene);

        public void GoHarbor()
        {
            CurrentLevelIndex = -1;
            LoadScene(harborScene);
        }

        public void GoLevel(int levelIndex)
        {
            if (levelIndex < 0 || levelIndex >= levelScenes.Length) return;
            CurrentLevelIndex = levelIndex;
            LoadScene(levelScenes[levelIndex]);
        }

        public void GoEnding() => LoadScene(endingScene);

        public void RestartCurrentScene()
        {
            Time.timeScale = 1f;
            LoadScene(SceneManager.GetActiveScene().name);
        }

        public void QuitGame()
        {
            DataLogger.I?.Flush();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        public void LoadScene(string sceneName)
        {
            if (IsLoading || string.IsNullOrEmpty(sceneName)) return;
            StartCoroutine(LoadRoutine(sceneName));
        }

        IEnumerator LoadRoutine(string sceneName)
        {
            IsLoading = true;
            Time.timeScale = 1f;

            if (fader != null) yield return fader.FadeOut(fadeDuration);

            var op = SceneManager.LoadSceneAsync(sceneName);
            while (op != null && !op.isDone) yield return null;

            yield return null;                      // let scene Start() run
            if (fader != null) yield return fader.FadeIn(fadeDuration);

            IsLoading = false;
        }
    }
}
