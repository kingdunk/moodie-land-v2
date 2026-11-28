using UnityEngine;
using UnityEngine.UI;

namespace MoodieLand
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Image))]
    public sealed class MainMenuCharacterMotion : BaseMeshEffect
    {
        enum Character { Pip, Mimi, Bobo, Nunu }

        static readonly int PoseTexture = Shader.PropertyToID("_PoseTex");
        static readonly int CharacterType = Shader.PropertyToID("_Character");
        static readonly int Motion = Shader.PropertyToID("_Motion");

        [SerializeField] Character character;
        [SerializeField] Shader motionShader;
        [SerializeField] Sprite pose;

        Image image;
        Material originalMaterial;
        Material animatedMaterial;
        Vector2 restingPosition;
        float started;

        const float Cycle = 5.4f;

        void StartMotion()
        {
            image = GetComponent<Image>();
            restingPosition = image.rectTransform.anchoredPosition;
            started = Time.unscaledTime;
            originalMaterial = image.material;
            animatedMaterial = new Material(motionShader) { hideFlags = HideFlags.HideAndDontSave };
            animatedMaterial.SetFloat(CharacterType, (float)character);
            if (pose != null)
                animatedMaterial.SetTexture(PoseTexture, pose.texture);
            image.material = animatedMaterial;
            image.SetVerticesDirty();
        }

        void LateUpdate()
        {
            if (!Application.IsPlaying(gameObject))
                return;

            if (animatedMaterial == null)
            {
                if (motionShader == null || !motionShader.isSupported)
                    return;

                StartMotion();
            }

            float elapsed = Time.unscaledTime - started;
            Vector4 motion = Vector4.zero;

            switch (character)
            {
                case Character.Pip:
                    float wave = Mathf.Repeat(elapsed - 0.55f, Cycle);
                    motion.x = Mathf.Sin(wave * 12.57f) * Envelope(wave, 1.05f, 0.16f);
                    motion.w = pose == null ? 1f : Envelope(wave, 1.25f, 0.12f);
                    break;
                case Character.Mimi:
                    motion.x = Mathf.Sin(elapsed * 2.7f) * SmoothRamp(elapsed / 0.6f);
                    break;
                case Character.Bobo:
                    float jump = Mathf.Repeat(elapsed - 2.1f, Cycle);
                    float height = Mathf.Clamp01(jump / 0.76f);
                    image.rectTransform.anchoredPosition = restingPosition
                        + Vector2.up * (45f * height * (1f - height) * 4f);
                    motion.x = pose == null ? 0f : Envelope(jump, 0.76f, 0.13f);
                    motion.y = Mathf.Sin(jump * 19f) * Envelope(jump, 1.05f, 0.23f);
                    motion.z = Mathf.Sin(jump * 19f) * motion.x;
                    break;
                case Character.Nunu:
                    float smile = Mathf.Repeat(elapsed - 3.75f, Cycle);
                    motion.x = pose == null ? 0f : Envelope(smile, 0.23f, 0.07f);
                    motion.y = pose == null ? 0f : Envelope(smile - 0.18f, 1.05f, 0.16f);
                    break;
            }

            animatedMaterial.SetVector(Motion, motion);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            if (animatedMaterial == null)
                return;

            if (character == Character.Bobo)
                image.rectTransform.anchoredPosition = restingPosition;
            image.material = originalMaterial;
            if (Application.isPlaying)
                Destroy(animatedMaterial);
            else
                DestroyImmediate(animatedMaterial);
            animatedMaterial = null;
        }

        public override void ModifyMesh(VertexHelper mesh)
        {
            if (!Application.IsPlaying(gameObject) || !IsActive() || animatedMaterial == null || mesh.currentVertCount != 4)
                return;

            // An Image can trim the transparent margins of its standing sprite.
            // The gesture and tail need those margins to draw past that silhouette.
            Rect rect = image.GetPixelAdjustedRect();
            if (image.preserveAspect)
            {
                float side = Mathf.Min(rect.width, rect.height);
                Vector2 padding = (rect.size - Vector2.one * side) * image.rectTransform.pivot;
                rect = new Rect(rect.position + padding, Vector2.one * side);
            }

            UIVertex original = default;
            mesh.PopulateUIVertex(ref original, 0);
            mesh.Clear();
            mesh.AddVert(new Vector3(rect.xMin, rect.yMin), original.color, new Vector2(0f, 0f));
            mesh.AddVert(new Vector3(rect.xMin, rect.yMax), original.color, new Vector2(0f, 1f));
            mesh.AddVert(new Vector3(rect.xMax, rect.yMax), original.color, new Vector2(1f, 1f));
            mesh.AddVert(new Vector3(rect.xMax, rect.yMin), original.color, new Vector2(1f, 0f));
            mesh.AddTriangle(0, 1, 2);
            mesh.AddTriangle(2, 3, 0);
        }

        static float Envelope(float time, float duration, float fade)
        {
            return SmoothRamp(Mathf.Min(time, duration - time) / fade);
        }

        static float SmoothRamp(float value) => Mathf.SmoothStep(0f, 1f, value);
    }
}
