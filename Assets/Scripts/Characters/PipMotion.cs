using UnityEngine;
using UnityEngine.UI;

namespace MoodieLand
{
    [DisallowMultipleComponent]
    [AddComponentMenu("")]
    public sealed class PipMotion : BaseMeshEffect
    {
        const int Grid = 16;
        static readonly Vector2 TailCenter = new Vector2(0.82f, 0.28f);
        static readonly Vector2 WavePaw = new Vector2(0.16f, 0.49f);
        static readonly Vector2 PointPaw = new Vector2(0.85f, 0.47f);

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
                    paw = new Vector2(-0.047f, 0.022f) * wave;
                    break;
                case "pointing":
                    float reach = Gesture(elapsed, 0.48f);
                    paw = new Vector2(0.026f, 0.011f) * reach;
                    lean = 0.012f * reach;
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

        public override void ModifyMesh(VertexHelper mesh)
        {
            if (!IsActive() || mesh.currentVertCount != 4)
                return;

            Rect rect = graphic.rectTransform.rect;
            if (rect.width <= 0f || rect.height <= 0f)
                return;

            // Subdivide the simple Image quad to move the paw and tail locally.
            UIVertex bottomLeft = default;
            UIVertex topRight = default;
            mesh.PopulateUIVertex(ref bottomLeft, 0);
            mesh.PopulateUIVertex(ref topRight, 2);
            mesh.Clear();

            Vector3 origin = bottomLeft.position;
            Vector3 size = topRight.position - origin;
            Vector2 pawCenter = pose == "pointing" ? PointPaw : WavePaw;

            for (int row = 0; row <= Grid; row++)
            {
                float v = (float)row / Grid;
                for (int column = 0; column <= Grid; column++)
                {
                    float u = (float)column / Grid;
                    UIVertex vertex = bottomLeft;
                    float x = origin.x + size.x * u;
                    float y = origin.y + size.y * v;
                    Vector2 point = new Vector2((x - rect.xMin) / rect.width, (y - rect.yMin) / rect.height);
                    float upper = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.13f, 0.85f, point.y));
                    float tailWeight = LocalWeight(point, TailCenter, 0.18f)
                        * Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.73f, 0.84f, point.x));
                    float pawWeight = LocalWeight(point, pawCenter, 0.23f);
                    vertex.position = new Vector3(
                        x + rect.width * (lean * upper + tail * tailWeight + paw.x * pawWeight),
                        y + rect.height * (hop + breath * upper + tail * 0.55f * tailWeight + paw.y * pawWeight),
                        origin.z);
                    vertex.uv0 = new Vector4(
                        Mathf.Lerp(bottomLeft.uv0.x, topRight.uv0.x, u),
                        Mathf.Lerp(bottomLeft.uv0.y, topRight.uv0.y, v));
                    mesh.AddVert(vertex);
                }
            }

            for (int row = 0; row < Grid; row++)
            {
                for (int column = 0; column < Grid; column++)
                {
                    int lower = row * (Grid + 1) + column;
                    int upper = lower + Grid + 1;
                    mesh.AddTriangle(lower, upper, upper + 1);
                    mesh.AddTriangle(lower, upper + 1, lower + 1);
                }
            }
        }

        static float LocalWeight(Vector2 point, Vector2 center, float radius)
        {
            float strength = Mathf.Clamp01(1f - (point - center).sqrMagnitude / (radius * radius));
            return strength * strength;
        }
    }
}
