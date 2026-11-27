using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEngine;

namespace MoodieLand
{
    /// <summary>
    /// Anonymous, local-only behaviour logging (course research support).
    /// Deliberately never stores free text from the Feelings Board, names, or contact details.
    /// Writes CSV rows (one per task) plus an event JSONL trail inside
    /// Application.persistentDataPath/MoodieLogs/.
    /// </summary>
    [DisallowMultipleComponent]
    public class DataLogger : MonoBehaviour
    {
        public static DataLogger I { get; private set; }

        [Header("Options")]
        public bool enabledLogging = true;
        public bool writeEventTrail = true;

        public string SessionId { get; private set; } = "S000";

        string dir;
        string csvPath;
        string jsonlPath;
        readonly Dictionary<string, TaskRecord> tasks = new Dictionary<string, TaskRecord>();
        readonly List<string> pendingRows = new List<string>();

        class TaskRecord
        {
            public string level;
            public string task;
            public string firstChoice = "";
            public string finalChoice = "";
            public bool hintUsed;
            public bool completed;
            public string branch = "";
            public float startTime;
            public float duration;
        }

        void Awake()
        {
            if (I != null && I != this) { Destroy(this); return; }
            I = this;
            DontDestroyOnLoad(gameObject);
            Init();
        }

        void OnDestroy()
        {
            if (I == this) I = null;
        }

        void Init()
        {
            SessionId = "S" + UnityEngine.Random.Range(100, 999).ToString(CultureInfo.InvariantCulture);
            dir = Path.Combine(Application.persistentDataPath, "MoodieLogs");
            csvPath = Path.Combine(dir, "moodie_sessions.csv");
            jsonlPath = Path.Combine(dir, "moodie_events.jsonl");

            if (!enabledLogging) return;
            try
            {
                Directory.CreateDirectory(dir);
                if (!File.Exists(csvPath))
                {
                    File.WriteAllText(csvPath,
                        "session_id,level_id,task_id,first_choice,final_choice,hint_used,branch_id,response_time,task_completed\n",
                        Encoding.UTF8);
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning("[MoodieLand] Log folder unavailable: " + e.Message);
                enabledLogging = false;
            }
        }

        public string LogFolder => dir;

        // ------------------------------------------------------------------ tasks

        public void StartTask(string level, string task)
        {
            string key = level + "|" + task;
            if (!tasks.TryGetValue(key, out var rec))
            {
                rec = new TaskRecord { level = level, task = task };
                tasks[key] = rec;
            }
            rec.startTime = Time.realtimeSinceStartup;
            WriteEvent("task_start", level, task);
        }

        public void LogChoice(string level, string task, string choiceId, string branchId = null)
        {
            string key = level + "|" + task;
            if (!tasks.TryGetValue(key, out var rec))
            {
                rec = new TaskRecord { level = level, task = task, startTime = Time.realtimeSinceStartup };
                tasks[key] = rec;
            }
            if (string.IsNullOrEmpty(rec.firstChoice)) rec.firstChoice = choiceId ?? "";
            rec.finalChoice = choiceId ?? "";
            if (!string.IsNullOrEmpty(branchId)) rec.branch = branchId;

            WriteEvent("choice", level, task, choiceId, branchId);
        }

        public void LogHint(string level, string task)
        {
            string key = level + "|" + task;
            if (tasks.TryGetValue(key, out var rec)) rec.hintUsed = true;
            WriteEvent("hint_used", level, task);
        }

        public void CompleteTask(string level, string task)
        {
            string key = level + "|" + task;
            if (!tasks.TryGetValue(key, out var rec))
            {
                rec = new TaskRecord { level = level, task = task, startTime = Time.realtimeSinceStartup };
                tasks[key] = rec;
            }
            rec.completed = true;
            rec.duration = Mathf.Max(0f, Time.realtimeSinceStartup - rec.startTime);

            pendingRows.Add(string.Join(",",
                SessionId,
                Escape(rec.level),
                Escape(rec.task),
                Escape(rec.firstChoice),
                Escape(rec.finalChoice),
                rec.hintUsed ? "true" : "false",
                Escape(rec.branch),
                rec.duration.ToString("0.0", CultureInfo.InvariantCulture),
                "true"));

            WriteEvent("task_complete", level, task, null, rec.branch);
            Flush();
        }

        /// <summary>Generic event. Never pass personal or free-text content here.</summary>
        public void LogEvent(string name, int levelIndex, string detail = "", string branch = "")
        {
            WriteEvent(name, levelIndex > 0 ? "Level" + levelIndex : "Global", detail, branch);
        }

        // ------------------------------------------------------------------- io

        void WriteEvent(string name, string level, string task, string choice = null, string branch = null)
        {
            if (!enabledLogging || !writeEventTrail) return;
            try
            {
                var sb = new StringBuilder();
                sb.Append('{');
                sb.Append("\"ts\":\"").Append(DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture)).Append("\",");
                sb.Append("\"session\":\"").Append(SessionId).Append("\",");
                sb.Append("\"event\":\"").Append(name).Append("\",");
                sb.Append("\"level\":\"").Append(level).Append("\",");
                sb.Append("\"task\":\"").Append(task).Append("\"");
                if (!string.IsNullOrEmpty(choice)) sb.Append(",\"choice\":\"").Append(choice).Append("\"");
                if (!string.IsNullOrEmpty(branch)) sb.Append(",\"branch\":\"").Append(branch).Append("\"");
                sb.Append('}');
                File.AppendAllText(jsonlPath, sb.Append('\n').ToString(), Encoding.UTF8);
            }
            catch { /* logging must never break gameplay */ }
        }

        public void Flush()
        {
            if (!enabledLogging || pendingRows.Count == 0) return;
            try
            {
                File.AppendAllText(csvPath, string.Join("\n", pendingRows) + "\n", Encoding.UTF8);
            }
            catch (Exception e)
            {
                Debug.LogWarning("[MoodieLand] Could not write log: " + e.Message);
            }
            pendingRows.Clear();
        }

        void OnApplicationQuit() => Flush();

        static string Escape(string s)
        {
            if (string.IsNullOrEmpty(s)) return "";
            if (s.IndexOfAny(new[] { ',', '"', '\n' }) < 0) return s;
            return "\"" + s.Replace("\"", "\"\"") + "\"";
        }
    }
}
