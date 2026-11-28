using UnityEngine;
using UnityEngine.UI;

namespace MoodieLand
{
    public abstract class SpriteMeshMotion : BaseMeshEffect
    {
        const int Grid = 16;

        protected abstract Vector2 Offset(Vector2 point);

        public override void ModifyMesh(VertexHelper mesh)
        {
            if (!IsActive() || mesh.currentVertCount != 4)
                return;

            Rect rect = graphic.rectTransform.rect;
            if (rect.width <= 0f || rect.height <= 0f)
                return;

            // Subdivide the Image quad so an offset can move a single part of the sprite.
            UIVertex bottomLeft = default;
            UIVertex topRight = default;
            mesh.PopulateUIVertex(ref bottomLeft, 0);
            mesh.PopulateUIVertex(ref topRight, 2);
            mesh.Clear();

            Vector3 origin = bottomLeft.position;
            Vector3 size = topRight.position - origin;
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
                    Vector2 offset = Offset(point);
                    vertex.position = new Vector3(x + rect.width * offset.x, y + rect.height * offset.y, origin.z);
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

        protected static float LocalWeight(Vector2 point, Vector2 center, float radius)
        {
            float strength = Mathf.Clamp01(1f - (point - center).sqrMagnitude / (radius * radius));
            return strength * strength;
        }
    }
}
