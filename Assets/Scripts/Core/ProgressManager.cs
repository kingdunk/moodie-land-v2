using System;
using System.Collections.Generic;
using UnityEngine;

namespace MoodieLand
{
    /// <summary>Badge identities used across the whole game.</summary>
    public enum BadgeId { None = 0, Rainbow = 1, Star = 2, Moon = 3 }

    /// <summary>The card the player builds in the Level 3 Feelings Board.</summary>
    [Serializable]
    public class MyMessage
    {
        public string emotion = "";
        public string intensity = "";
        public List<string> reasons = new List<string>();
        public List<string> needs = new List<string>();
        public bool created;          // player reached the preview screen
        public bool sharedWithNunu;   // player pressed "Show my message"
        public bool keptPrivate;

        public bool IsEmpty =>
            string.IsNullOrEmpty(emotion) &&
            string.IsNullOrEmpty(intensity) &&
            reasons.Count == 0 &&
            needs.Count == 0;

        public MyMessage Clone()
        {
            return new MyMessage
            {
                emotion = emotion,
                intensity = intensity,
                reasons = new List<string>(reasons),
                needs = new List<string>(needs),
                created = created,
                sharedWithNunu = sharedWithNunu,
                keptPrivate = keptPrivate
            };
        }

        /// <summary>Composes the little message. Skipped steps are hidden, never blank lines.</summary>
        public string Compose()
        {
            var lines = new List<string>();

            if (!string.IsNullOrEmpty(emotion))
            {
                lines.Add(emotion == "Not sure"
                    ? "I'm not sure how I feel."
                    : "I feel " + LowerFirst(emotion) + ".");
            }

            if (!string.IsNullOrEmpty(intensity))
                lines.Add("It feels " + LowerFirst(intensity) + ".");

            if (reasons.Count > 0)
                lines.Add("Maybe because " + string.Join(" ", reasons.ConvertAll(LowerFirst)));

            // need cards are already full sentences ("I need a break.")
            foreach (var need in needs)
                if (!string.IsNullOrEmpty(need)) lines.Add(need);

            if (lines.Count == 0)
                return "I'm not ready to say it yet.\nThat's okay.";

            return string.Join("\n", lines);
        }

        /// <summary>Lower-cases the first letter but keeps "I ..." sentences intact.</summary>
        static string LowerFirst(string s)
        {
            if (string.IsNullOrEmpty(s)) return s;
            s = s.Trim();
            if (s.StartsWith("I ", StringComparison.Ordinal) || s.StartsWith("I'", StringComparison.Ordinal))
                return s;
            return char.ToLowerInvariant(s[0]) + s.Substring(1);
        }
    }

    /// <summary>
    /// Tracks level completion, badges and the player's message.
    /// Persists across scenes (DontDestroyOnLoad) and to PlayerPrefs between sessions.
    /// </summary>
    [DisallowMultipleComponent]
    public class ProgressManager : MonoBehaviour
    {
        public static ProgressManager I { get; private set; }

        public const string KeyPrefix = "MoodieLand.";

        [Header("Runtime state (read only)")]
        public bool[] levelCompleted = new bool[3];
        public BadgeId[] levelBadges = { BadgeId.Rainbow, BadgeId.Star, BadgeId.Moon };
        public MyMessage myMessage = new MyMessage();

        [Tooltip("True once the harbour introduction has been played, so it never repeats.")]
        public bool harborIntroSeen;

        public event Action Changed;

        void Awake()
        {
            if (I != null && I != this) { Destroy(this); return; }
            I = this;
            DontDestroyOnLoad(gameObject);
            Load();
        }

        void OnDestroy()
        {
            if (I == this) I = null;
        }

        public bool AnyCompleted => levelCompleted[0] || levelCompleted[1] || levelCompleted[2];
        public bool AllCompleted => levelCompleted[0] && levelCompleted[1] && levelCompleted[2];
        public int CompletedCount
        {
            get
            {
                int n = 0;
                for (int i = 0; i < levelCompleted.Length; i++) if (levelCompleted[i]) n++;
                return n;
            }
        }

        public bool IsCompleted(int index) =>
            index >= 0 && index < levelCompleted.Length && levelCompleted[index];

        public void CompleteLevel(int index)
        {
            if (index < 0 || index >= levelCompleted.Length) return;
            bool isNew = !levelCompleted[index];
            levelCompleted[index] = true;
            Save();
            Changed?.Invoke();
            if (isNew)
                DataLogger.I?.LogEvent("level_completed", index + 1, "badge", levelBadges[index].ToString());
        }

        public BadgeId GetBadge(int index) =>
            index >= 0 && index < levelBadges.Length ? levelBadges[index] : BadgeId.None;

        public void SetMessage(MyMessage message)
        {
            myMessage = message ?? new MyMessage();
            Save();
            Changed?.Invoke();
        }

        public void ResetAll()
        {
            levelCompleted = new bool[3];
            myMessage = new MyMessage();
            harborIntroSeen = false;
            Save();
            Changed?.Invoke();
        }

        // ------------------------------------------------------------ persistence

        [Serializable]
        class SaveBlob
        {
            public bool l1, l2, l3;
            public string emotion, intensity;
            public List<string> reasons = new List<string>();
            public List<string> needs = new List<string>();
            public bool created, shared, priv;
            public bool harborIntro;
        }

        public void Save()
        {
            var blob = new SaveBlob
            {
                l1 = levelCompleted[0],
                l2 = levelCompleted[1],
                l3 = levelCompleted[2],
                emotion = myMessage.emotion,
                intensity = myMessage.intensity,
                reasons = myMessage.reasons,
                needs = myMessage.needs,
                created = myMessage.created,
                shared = myMessage.sharedWithNunu,
                priv = myMessage.keptPrivate,
                harborIntro = harborIntroSeen
            };
            PlayerPrefs.SetString(KeyPrefix + "progress", JsonUtility.ToJson(blob));
            PlayerPrefs.Save();
        }

        public void Load()
        {
            string json = PlayerPrefs.GetString(KeyPrefix + "progress", "");
            if (string.IsNullOrEmpty(json)) return;
            try
            {
                var blob = JsonUtility.FromJson<SaveBlob>(json);
                if (blob == null) return;
                levelCompleted[0] = blob.l1;
                levelCompleted[1] = blob.l2;
                levelCompleted[2] = blob.l3;
                harborIntroSeen = blob.harborIntro;
                myMessage = new MyMessage
                {
                    emotion = blob.emotion,
                    intensity = blob.intensity,
                    reasons = blob.reasons ?? new List<string>(),
                    needs = blob.needs ?? new List<string>(),
                    created = blob.created,
                    sharedWithNunu = blob.shared,
                    keptPrivate = blob.priv
                };
            }
            catch (Exception e)
            {
                Debug.LogWarning("[MoodieLand] Could not read saved progress: " + e.Message);
            }
        }
    }
}
