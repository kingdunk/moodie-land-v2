using UnityEngine;

namespace MoodieLand
{
    [DisallowMultipleComponent]
    [AddComponentMenu("")]
    public sealed class WatercolorAccessoryMotion : SpriteMeshMotion
    {
        public string CharacterKey { get; set; }
        float amount;
        string pose;

        public void SetAmount(float value, string expression)
        {
            if (pose == expression && Mathf.Approximately(amount, value)) return;
            amount = value;
            pose = expression;
            graphic.SetVerticesDirty();
        }

        protected override Vector2 Offset(Vector2 point)
        {
            switch (CharacterKey)
            {
                case "bobo":
                    float ears = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.64f, 0.92f, point.y));
                    Vector2 movement = new Vector2(0.038f, -0.035f) * (amount * ears);
                    if (pose == "waving")
                    {
                        float paw = LocalWeight(point, new Vector2(0.32f, 0.48f), 0.17f);
                        movement += new Vector2(0.032f, 0.018f) * (amount * paw);
                    }
                    if (pose == "clapping")
                    {
                        float left = LocalWeight(point, new Vector2(0.43f, 0.44f), 0.135f);
                        float right = LocalWeight(point, new Vector2(0.59f, 0.44f), 0.135f);
                        movement.x += 0.025f * Mathf.Abs(amount) * (right - left);
                    }
                    return movement;
                case "nunu":
                    float leaf = LocalWeight(point, new Vector2(0.54f, 0.90f), 0.17f);
                    return new Vector2(0.052f, -0.016f) * (amount * leaf);
                case "mimi":
                    float tail = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.59f, 0.84f, point.x))
                        * Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.27f, 0.47f, point.y));
                    return new Vector2(0.036f, 0.028f) * (amount * tail);
                default:
                    return Vector2.zero;
            }
        }
    }
}
