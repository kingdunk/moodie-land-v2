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

        protected override void OnEnable()
        {
            base.OnEnable();
            image = GetComponent<Image>();
            restingPosition = image.rectTransform.anchoredPosition;
            started = Time.unscaledTime;
            if (motionShader == null || !motionShader.isSupported)
                return;

            originalMaterial = image.material;
            animatedMaterial = new Material(motionShader);
            animatedMaterial.SetFloat(CharacterType, (float)character);
            if (pose != null)
                animatedMaterial.SetTexture(PoseTexture, pose.texture);
            image.material = animatedMaterial;
        }

        void LateUpdate()
        {
            if (animatedMaterial == null)
                return;

            float elapsed = Time.unscaledTime - started;
            Vector4 motion = Vector4.zero;

            switch (character)
            {
                case Character.Pip:
                    float wave = Mathf.Repeat(elapsed - 0.65f, 3.5f);
                    motion.x = Envelope(wave, 1.35f, 0.17f);
                    motion.y = Mathf.Sin(wave * 15f) * motion.x;
                    break;
                case Character.Mimi:
                    motion.x = Mathf.Sin(elapsed * 4.8f) * SmoothRamp(elapsed / 0.4f);
                    break;
                case Character.Bobo:
                    float jump = Mathf.Repeat(elapsed - 1.05f, 2.65f);
                    float height = Mathf.Clamp01(jump / 0.76f);
                    image.rectTransform.anchoredPosition = restingPosition
                        + Vector2.up * (88f * height * (1f - height) * 4f);
                    motion.x = Envelope(jump, 0.76f, 0.13f);
                    motion.y = Mathf.Sin(jump * 21f) * motion.x;
                    break;
                case Character.Nunu:
                    float smile = Mathf.Repeat(elapsed - 1.55f, 3.9f);
                    motion.x = Envelope(smile, 0.23f, 0.07f);
                    motion.y = Envelope(smile, 1.05f, 0.16f);
                    break;
            }

            animatedMaterial.SetVector(Motion, motion);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            if (animatedMaterial == null)
                return;

            image.rectTransform.anchoredPosition = restingPosition;
            image.material = originalMaterial;
            Destroy(animatedMaterial);
            animatedMaterial = null;
        }

        public override void ModifyMesh(VertexHelper mesh)
        {
            if (!IsActive() || animatedMaterial == null || mesh.currentVertCount != 4)
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
