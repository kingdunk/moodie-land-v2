using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MoodieLand
{
    [Serializable]
    public class ExpressionSprite
    {
        public string name;
        public Sprite sprite;
    }

    /// <summary>
    /// A character on screen. Expression changes are plain sprite swaps
    /// (the design doc explicitly allows sprite swap instead of an Animator),
    /// plus procedural motion and reaction feedback.
    /// </summary>
    [DisallowMultipleComponent]
    public class CharacterView : MonoBehaviour
    {
        [Header("Identity")]
        public string characterKey = "pip";
        public string displayName = "Pip";

        [Header("Visual")]
        public Image image;
        public List<ExpressionSprite> expressions = new List<ExpressionSprite>();

        [Tooltip("Optional head-only sprites used by the dialogue portrait.")]
        public List<ExpressionSprite> heads = new List<ExpressionSprite>();

        [Header("Feedback")]
        public bool idleBob = true;
        public float bobAmplitude = 6f;
        public float bobSpeed = 1.6f;
        public bool faceLeft = false;
        public bool watercolorMotion;

        public string CurrentExpression { get; private set; } = "idle";

        RectTransform rt;
        Vector2 basePos;
        float bobPhase;
        float talkingUntil;
        Vector3 baseScale = Vector3.one;
        bool shaking;
        PipMotion pipMotion;
        WatercolorAccessoryMotion accessoryMotion;
        float expressionStarted;
        bool animateExpression;
        Quaternion baseRotation;

        void Awake()
        {
            rt = (RectTransform)transform;
            basePos = rt.anchoredPosition;
            baseScale = rt.localScale;
            baseRotation = rt.localRotation;
            bobPhase = UnityEngine.Random.Range(0f, 6.28f);
            if (image == null) image = GetComponent<Image>();
            if (isActiveAndEnabled && image != null && string.Equals(characterKey, "pip", StringComparison.OrdinalIgnoreCase))
            {
                pipMotion = image.gameObject.AddComponent<PipMotion>();
                pipMotion.UseWatercolorArt = watercolorMotion;
            }
            else if (isActiveAndEnabled && image != null && watercolorMotion)
            {
                accessoryMotion = image.gameObject.AddComponent<WatercolorAccessoryMotion>();
                accessoryMotion.CharacterKey = characterKey;
            }
            ApplyFacing();
            SetExpression(string.IsNullOrEmpty(CurrentExpression) ? "idle" : CurrentExpression, false);
        }

        void ApplyFacing()
        {
            if (image == null) return;
            var s = transform.localScale;
            s.x = Mathf.Abs(s.x) * (faceLeft ? -1f : 1f);
            transform.localScale = s;
            baseScale = s;
        }

        public void SetFaceLeft(bool left)
        {
            faceLeft = left;
            ApplyFacing();
        }

        void Update()
        {
            if (rt == null) return;

            if (pipMotion != null)
            {
                pipMotion.Tick(idleBob, Time.unscaledTime < talkingUntil, bobPhase);
                return;
            }

            if (watercolorMotion)
            {
                UpdateWatercolorPose();
                return;
            }

            float y = 0f;
            if (idleBob)
            {
                bobPhase += Time.unscaledDeltaTime * bobSpeed;
                y += Mathf.Sin(bobPhase) * bobAmplitude;
            }

            if (Time.unscaledTime < talkingUntil)
                y += Mathf.Sin(Time.unscaledTime * 18f) * 2.5f;

            if (!shaking)
                rt.anchoredPosition = basePos + new Vector2(0f, y);
        }

        void UpdateWatercolorPose()
        {
            float elapsed = Time.unscaledTime - expressionStarted;
            float sway = 0f;
            float height = 0f;
            float accessory = 0f;
            if (animateExpression)
            {
                switch (CurrentExpression)
                {
                    case "searching":
                        float search = Mathf.Clamp01(elapsed / 1.1f);
                        sway = 3f * Mathf.Sin(search * 2f * Mathf.PI) * Mathf.Sin(search * Mathf.PI);
                        break;
                    case "worried":
                        sway = -2f * Pulse(elapsed, 0.52f);
                        accessory = Pulse(elapsed, 0.58f);
                        break;
                    case "frustrated":
                        sway = 2f * Pulse(elapsed, 0.55f);
                        break;
                    case "happy":
                        if (characterKey == "nunu")
                        {
                            sway = 1.8f * Pulse(elapsed, 0.72f);
                            accessory = DampedSway(elapsed, 0.9f, 2f);
                        }
                        else
                        {
                            height = 10f * Pulse(elapsed, 0.49f);
                        }
                        break;
                    case "calm":
                        sway = -1.2f * Pulse(elapsed, 0.74f);
                        break;
                    case "waving":
                        sway = 2f * DampedSway(elapsed, 0.72f, 2f);
                        accessory = DampedSway(elapsed, 0.9f, 2f);
                        break;
                    case "building":
                    case "offering":
                        sway = 4f * Pulse(elapsed, 0.62f);
                        accessory = DampedSway(elapsed, 0.72f, 1.5f);
                        break;
                    case "listening":
                    case "thinking":
                        sway = -2.5f * Pulse(elapsed, 0.6f);
                        accessory = DampedSway(elapsed, 0.73f, 1.5f);
                        break;
                    case "disappointed":
                        sway = 3.5f * Pulse(elapsed, 0.74f);
                        accessory = Pulse(elapsed, 0.78f);
                        break;
                    case "relieved":
                        sway = -2f * Pulse(elapsed, 1.12f);
                        accessory = DampedSway(elapsed, 1.12f, 1.5f);
                        break;
                    case "clapping":
                        height = 5f * Mathf.Abs(DampedSway(elapsed, 0.66f, 1.5f));
                        accessory = DampedSway(elapsed, 0.85f, 2f);
                        break;
                    case "celebrating":
                        height = characterKey == "bobo" ? 24f * Pulse(elapsed, 0.64f) : 0f;
                        accessory = DampedSway(elapsed, 1.13f, 2f);
                        break;
                }
            }
            accessoryMotion?.SetAmount(accessory, CurrentExpression);
            rt.localRotation = baseRotation * Quaternion.Euler(0f, 0f, sway);
            if (!shaking)
                rt.anchoredPosition = basePos + Vector2.up * height;
        }

        static float Pulse(float elapsed, float duration)
            => elapsed < duration ? Mathf.Sin(Mathf.PI * elapsed / duration) : 0f;

        static float DampedSway(float elapsed, float duration, float cycles)
            => Pulse(elapsed, duration) * Mathf.Sin(2f * Mathf.PI * cycles * elapsed / duration);

        public Sprite GetSprite(string key)
        {
            return Lookup(expressions, key);
        }

        /// <summary>Head-only sprite for the dialogue portrait; falls back to the body.</summary>
        public Sprite GetHead(string key)
        {
            var head = Lookup(heads, key);
            return head != null ? head : GetSprite(key);
        }

        static Sprite Lookup(List<ExpressionSprite> list, string key)
        {
            if (list == null || list.Count == 0) return null;
            for (int i = 0; i < list.Count; i++)
            {
                if (string.Equals(list[i].name, key, System.StringComparison.OrdinalIgnoreCase))
                    return list[i].sprite;
            }
            return list[0].sprite;
        }

        public void SetExpression(string key, bool animate = true, bool restart = false)
        {
            if (string.IsNullOrEmpty(key)) return;
            bool changed = !string.Equals(CurrentExpression, key, StringComparison.OrdinalIgnoreCase);
            CurrentExpression = key;
            Sprite sprite = GetSprite(key);
            if (image != null && sprite != null) image.sprite = sprite;
            if (pipMotion != null)
                pipMotion.SetPose(key, animate);
            else if (watercolorMotion)
            {
                if (!changed && !restart) return;
                expressionStarted = Time.unscaledTime;
                animateExpression = animate;
            }
            else if (animate)
                StartCoroutine(UIFx.Punch(transform, 0.05f, 0.22f));
        }

        /// <summary>Keeps a small mouth-flap illusion while a line is being typed.</summary>
        public void TalkFor(float seconds) => talkingUntil = Mathf.Max(talkingUntil, Time.unscaledTime + seconds);

        public void React(string expression, float shakeStrength = 10f)
        {
            SetExpression(expression);
            if (shakeStrength > 0f) StartCoroutine(ShakeRoutine(shakeStrength, 0.3f));
        }

        System.Collections.IEnumerator ShakeRoutine(float strength, float duration)
        {
            shaking = true;
            Vector2 origin = rt.anchoredPosition;
            float t = 0f;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                float k = 1f - t / duration;
                rt.anchoredPosition = origin + new Vector2(Mathf.Sin(t * 70f) * strength * k, 0f);
                yield return null;
            }
            rt.anchoredPosition = origin;
            shaking = false;
        }

        public void SetDim(bool dim)
        {
            if (image == null) return;
            image.color = dim ? new Color(1f, 1f, 1f, 0.72f) : Color.white;
        }

        public void SetVisible(bool visible)
        {
            gameObject.SetActive(visible);
        }
    }
}
