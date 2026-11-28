using UnityEngine;
using UnityEngine.UI;

namespace MoodieLand
{
    [DisallowMultipleComponent]
    [AddComponentMenu("")]
    public sealed class PipMotion : SpriteMeshMotion
    {
        static readonly Vector2 TailCenter = new Vector2(0.82f, 0.28f);
        static readonly Vector2 WavePaw = new Vector2(0.16f, 0.49f);
        static readonly Vector2 PointPaw = new Vector2(0.85f, 0.47f);
        static readonly Vector2 WatercolorTail = new Vector2(0.25f, 0.30f);
        static readonly Vector2 WatercolorPaw = new Vector2(0.74f, 0.46f);

        public bool UseWatercolorArt { get; set; }

        string pose;
        float poseStarted;
        bool playGesture;
        float breath;
        float lean;
        float hop;
        float tail;
        Vector2 paw;

        public void SetPose(string key, bool animate)
        {
            pose = key.ToLowerInvariant();
            poseStarted = Time.unscaledTime;
            playGesture = animate;
        }

        public void Tick(bool idleMotion, bool talking, float phase)
        {
            float now = Time.unscaledTime;
            float elapsed = now - poseStarted;
            float talk = talking ? 0.004f * Mathf.Sin(now * 18f) : 0f;
            breath = (idleMotion ? 0.009f * Mathf.Sin(now * 2.6f + phase) : 0f) + talk;
            tail = idleMotion ? 0.008f * Mathf.Sin(now * 4.2f + phase) : 0f;
            lean = 0f;
            hop = 0f;
            paw = Vector2.zero;

            switch (pose)
            {
                case "waving":
                    float wave = Gesture(elapsed, 0.85f) * Mathf.Sin(2f * Mathf.PI * elapsed / 0.28f);
                    paw = new Vector2(UseWatercolorArt ? 0.031f : -0.047f, 0.022f) * wave;
                    break;
                case "pointing":
                case "inviting":
                    float reach = Gesture(elapsed, 0.48f);
                    paw = new Vector2(0.026f, 0.011f) * reach;
                    lean = 0.012f * reach;
                    break;
                case "nodding":
                    float nod = Gesture(elapsed, 0.52f);
                    breath -= 0.027f * nod;
                    lean += 0.011f * nod;
                    break;
                case "thinking":
                    lean = -0.019f * Gesture(elapsed, 0.95f);
                    break;
                case "happy":
                    float bounce = Gesture(elapsed, 0.62f);
                    hop = 0.052f * bounce;
                    breath += 0.025f * bounce;
                    tail += 0.014f * bounce * Mathf.Sin(elapsed * 22f);
                    break;
                case "calm":
                    breath = (idleMotion ? 0.014f * Mathf.Sin(now * 1.65f + phase) : 0f)
                        + talk - 0.012f * Gesture(elapsed, 0.75f);
                    tail *= 0.25f;
                    break;
            }

            graphic.SetVerticesDirty();
        }

        float Gesture(float elapsed, float duration)
        {
            return playGesture && elapsed < duration ? Mathf.Sin(Mathf.PI * elapsed / duration) : 0f;
        }

        protected override Vector2 Offset(Vector2 point)
        {
            Vector2 pawCenter = UseWatercolorArt ? WatercolorPaw : pose == "pointing" ? PointPaw : WavePaw;
            float upper = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.13f, 0.85f, point.y));
            float tailWeight = UseWatercolorArt
                ? LocalWeight(point, WatercolorTail, 0.18f)
                    * Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.41f, 0.24f, point.x))
                : LocalWeight(point, TailCenter, 0.18f)
                    * Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.73f, 0.84f, point.x));
            float pawWeight = LocalWeight(point, pawCenter, 0.23f);
            return new Vector2(lean * upper + tail * tailWeight + paw.x * pawWeight,
                hop + breath * upper + tail * 0.55f * tailWeight + paw.y * pawWeight);
        }
    }
}
