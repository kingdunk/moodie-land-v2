using UnityEngine;

namespace MoodieLand.EditorTools
{
    /// <summary>Backgrounds and scenery props, all drawn procedurally.</summary>
    public static class SceneArt
    {
        // ------------------------------------------------------------ primitives

        public static void Cloud(Canvas2D c, float x, float y, float scale, Color32 body, Color32 shade)
        {
            c.SoftEllipse(x, y + 8f * scale, 130f * scale, 44f * scale, shade, 0.55f);
            c.Ellipse(x, y, 120f * scale, 46f * scale, body);
            c.Ellipse(x - 62f * scale, y + 10f * scale, 62f * scale, 34f * scale, body);
            c.Ellipse(x + 66f * scale, y + 8f * scale, 66f * scale, 36f * scale, body);
            c.Ellipse(x - 18f * scale, y - 26f * scale, 56f * scale, 42f * scale, body);
            c.Ellipse(x + 34f * scale, y - 18f * scale, 44f * scale, 34f * scale, body);
        }

        public static void RoundTree(Canvas2D c, float x, float groundY, float h, Color32 trunk, Color32 leaves, Color32 leavesDark)
        {
            float w = h * 0.10f;
            c.RoundRect(x - w * 0.5f, groundY - h * 0.52f, w, h * 0.52f, w * 0.4f, trunk);
            float cy = groundY - h * 0.68f;
            float r = h * 0.30f;
            c.Circle(x - r * 0.62f, cy + r * 0.25f, r * 0.72f, leavesDark);
            c.Circle(x + r * 0.62f, cy + r * 0.25f, r * 0.72f, leavesDark);
            c.Circle(x, cy - r * 0.25f, r * 0.86f, leaves);
            c.Circle(x - r * 0.55f, cy + r * 0.18f, r * 0.62f, leaves);
            c.Circle(x + r * 0.55f, cy + r * 0.18f, r * 0.62f, leaves);
        }

        public static void PineTree(Canvas2D c, float x, float groundY, float h, Color32 trunk, Color32 leaves)
        {
            c.Rect(x - h * 0.035f, groundY - h * 0.25f, h * 0.07f, h * 0.25f, trunk);
            for (int i = 0; i < 3; i++)
            {
                float yy = groundY - h * (0.22f + i * 0.24f);
                float w = h * (0.30f - i * 0.06f);
                c.Poly(leaves, new Vector2[]
                {
                    new Vector2(x - w, yy),
                    new Vector2(x + w, yy),
                    new Vector2(x, yy - h * 0.34f)
                });
            }
        }

        public static void Rainbow(Canvas2D c, float cx, float cy, float radius, float thickness)
        {
            var cols = new[]
            {
                Canvas2D.Hex("F7A8B2"), Canvas2D.Hex("FFC7A8"), Canvas2D.Hex("FFE08A"),
                Canvas2D.Hex("A8E6CF"), Canvas2D.Hex("8FB8E8"), Canvas2D.Hex("C7CEEA")
            };

            const int steps = 64;
            for (int i = 0; i < cols.Length; i++)
            {
                float rOut = radius - i * thickness;
                float rIn = rOut - thickness * 0.96f;
                var pts = new Vector2[steps * 2];

                for (int a = 0; a <= steps; a++)
                {
                    float t = Mathf.PI * (a / (float)steps);
                    pts[a] = new Vector2(cx - Mathf.Cos(t) * rOut, cy - Mathf.Sin(t) * rOut);
                    pts[pts.Length - 1 - a] = new Vector2(cx - Mathf.Cos(t) * rIn, cy - Mathf.Sin(t) * rIn);
                }

                c.Poly(Canvas2D.Alpha(cols[i], 0.72f), pts);
            }
        }

        public static void Sparkle(Canvas2D c, float x, float y, float r, Color32 col)
        {
            c.Poly(col, new Vector2[]
            {
                new Vector2(x, y - r), new Vector2(x + r * 0.26f, y - r * 0.26f),
                new Vector2(x + r, y), new Vector2(x + r * 0.26f, y + r * 0.26f),
                new Vector2(x, y + r), new Vector2(x - r * 0.26f, y + r * 0.26f),
                new Vector2(x - r, y), new Vector2(x - r * 0.26f, y - r * 0.26f)
            });
        }

        public static void Flower(Canvas2D c, float x, float y, float s, Color32 petal, Color32 middle)
        {
            c.Rect(x - s * 0.06f, y, s * 0.12f, s * 1.1f, Pal.GrassDeep);
            for (int i = 0; i < 5; i++)
            {
                float a = i * Mathf.PI * 2f / 5f - Mathf.PI * 0.5f;
                c.Circle(x + Mathf.Cos(a) * s * 0.42f, y + Mathf.Sin(a) * s * 0.42f, s * 0.30f, petal);
            }
            c.Circle(x, y, s * 0.26f, middle);
        }

        // ------------------------------------------------------------ backgrounds

        public static void Menu(Canvas2D c)
        {
            c.VerticalGradient(Canvas2D.Hex("FFF3E9"), Canvas2D.Hex("D8EEF7"));
            c.Glow(c.W * 0.80f, c.H * 0.16f, c.W * 0.42f, Canvas2D.Hex("FFE9B8", 0.85f), 2.2f);
            c.Circle(c.W * 0.80f, c.H * 0.16f, c.W * 0.055f, Canvas2D.Hex("FFE7A6"));

            Rainbow(c, c.W * 0.34f, c.H * 0.92f, c.W * 0.40f, 26f);

            Cloud(c, c.W * 0.14f, c.H * 0.24f, 1.15f, Pal.Cloud, Pal.CloudShade);
            Cloud(c, c.W * 0.52f, c.H * 0.12f, 0.85f, Pal.Cloud, Pal.CloudShade);
            Cloud(c, c.W * 0.90f, c.H * 0.44f, 0.7f, Pal.Cloud, Pal.CloudShade);

            // floating islands
            for (int i = 0; i < 3; i++)
            {
                float x = c.W * (0.10f + i * 0.36f);
                float y = c.H * (0.86f + (i % 2) * 0.03f);
                c.Ellipse(x, y, c.W * 0.11f, 34f, Pal.Grass);
                c.Poly(Canvas2D.Hex("C9B49B"), new Vector2[]
                {
                    new Vector2(x - c.W * 0.10f, y + 12f),
                    new Vector2(x + c.W * 0.10f, y + 12f),
                    new Vector2(x + c.W * 0.02f, y + 90f),
                    new Vector2(x - c.W * 0.02f, y + 96f)
                });
                RoundTree(c, x, y + 4f, c.H * 0.22f, Pal.Brown, Pal.GrassDark, Pal.GrassDeep);
            }

            Sparkle(c, c.W * 0.20f, c.H * 0.12f, 14f, Canvas2D.Hex("FFFFFF", 0.9f));
            Sparkle(c, c.W * 0.66f, c.H * 0.30f, 10f, Canvas2D.Hex("FFFFFF", 0.8f));
            Sparkle(c, c.W * 0.42f, c.H * 0.06f, 8f, Canvas2D.Hex("FFF3C0", 0.9f));
        }

        public static void Intro(Canvas2D c)
        {
            c.VerticalGradient(Canvas2D.Hex("FFE6D2"), Canvas2D.Hex("CFE7F4"));
            c.Glow(c.W * 0.22f, c.H * 0.28f, c.W * 0.34f, Canvas2D.Hex("FFD9A8", 0.7f), 2f);
            c.Circle(c.W * 0.22f, c.H * 0.28f, c.W * 0.048f, Canvas2D.Hex("FFE3AE"));

            Cloud(c, c.W * 0.10f, c.H * 0.10f, 1.3f, Canvas2D.Hex("FFF6EE"), Canvas2D.Hex("F0DED4"));
            Cloud(c, c.W * 0.55f, c.H * 0.06f, 1f, Canvas2D.Hex("FFF6EE"), Canvas2D.Hex("F0DED4"));
            Cloud(c, c.W * 0.86f, c.H * 0.20f, 0.9f, Canvas2D.Hex("FFF6EE"), Canvas2D.Hex("F0DED4"));

            // distant island + harbour silhouette
            float ix = c.W * 0.62f, iy = c.H * 0.74f;
            c.Ellipse(ix, iy, c.W * 0.26f, 46f, Canvas2D.Hex("B7DFA4", 0.9f));
            c.Poly(Canvas2D.Hex("C9B49B", 0.9f), new Vector2[]
            {
                new Vector2(ix - c.W * 0.24f, iy + 14f),
                new Vector2(ix + c.W * 0.24f, iy + 14f),
                new Vector2(ix + c.W * 0.06f, iy + 120f),
                new Vector2(ix - c.W * 0.06f, iy + 130f)
            });
            RoundTree(c, ix - c.W * 0.10f, iy + 8f, c.H * 0.26f, Canvas2D.Hex("9A7B60"), Canvas2D.Hex("8FCB8A"), Canvas2D.Hex("7FB673"));
            RoundTree(c, ix + c.W * 0.08f, iy + 6f, c.H * 0.18f, Canvas2D.Hex("9A7B60"), Canvas2D.Hex("A8D9A0"), Canvas2D.Hex("7FB673"));

            for (int i = 0; i < 26; i++)
            {
                float x = (i * 137 % 1000) / 1000f * c.W;
                float y = (i * 71 % 300) / 300f * c.H * 0.5f;
                Sparkle(c, x, y, 4f + (i % 3) * 2f, Canvas2D.Hex("FFFFFF", 0.5f));
            }
        }

        public static void Harbor(Canvas2D c)
        {
            c.VerticalGradient(Canvas2D.Hex("E8F6FB"), Canvas2D.Hex("FFF1E4"));

            Cloud(c, c.W * 0.12f, c.H * 0.16f, 0.9f, Canvas2D.Hex("FFFFFF"), Canvas2D.Hex("E6EFF4"));
            Cloud(c, c.W * 0.88f, c.H * 0.12f, 0.8f, Canvas2D.Hex("FFFFFF"), Canvas2D.Hex("E6EFF4"));
            Cloud(c, c.W * 0.50f, c.H * 0.08f, 0.6f, Canvas2D.Hex("FFFFFF", 0.85f), Canvas2D.Hex("E6EFF4", 0.85f));

            float cx = c.W * 0.5f;
            float iy = c.H * 0.70f;

            // island body
            c.Ellipse(cx, iy + 40f, c.W * 0.40f, 120f, Canvas2D.Hex("C9B49B"));
            c.Poly(Canvas2D.Hex("BFA98F"), new Vector2[]
            {
                new Vector2(cx - c.W * 0.38f, iy + 60f),
                new Vector2(cx + c.W * 0.38f, iy + 60f),
                new Vector2(cx + c.W * 0.10f, iy + 210f),
                new Vector2(cx - c.W * 0.08f, iy + 220f)
            });
            c.Ellipse(cx, iy, c.W * 0.40f, 120f, Pal.Grass);
            c.Ellipse(cx, iy - 6f, c.W * 0.36f, 100f, Canvas2D.Hex("C4E7B0"));

            // three paths
            var pathCols = new[] { Canvas2D.Hex("F7C6C9"), Canvas2D.Hex("F8D9A8"), Canvas2D.Hex("C3CDF0") };
            for (int i = 0; i < 3; i++)
            {
                float ang = Mathf.Lerp(200f, 340f, i / 2f) * Mathf.Deg2Rad;
                float px = cx + Mathf.Cos(ang) * c.W * 0.26f;
                float py = iy - 10f + Mathf.Sin(ang) * 60f;
                c.Line(cx, iy - 6f, px, py, 34f, Canvas2D.Alpha(pathCols[i], 0.75f));
                c.Ellipse(px, py, 74f, 40f, Canvas2D.Alpha(pathCols[i], 0.55f));
            }

            // small bushes
            for (int i = 0; i < 6; i++)
            {
                float a = i / 6f * Mathf.PI * 2f;
                float x = cx + Mathf.Cos(a) * c.W * 0.30f;
                float y = iy + Mathf.Sin(a) * 70f;
                c.Circle(x, y, 16f, Canvas2D.Hex("8FCB8A"));
                c.Circle(x + 14f, y + 4f, 12f, Canvas2D.Hex("A3D79D"));
            }

            // floating islets
            for (int i = 0; i < 2; i++)
            {
                float x = i == 0 ? c.W * 0.06f : c.W * 0.94f;
                float y = c.H * 0.52f;
                c.Ellipse(x, y, 66f, 20f, Canvas2D.Hex("B7DFA4"));
                c.Poly(Canvas2D.Hex("C9B49B"), new Vector2[]
                {
                    new Vector2(x - 60f, y + 6f), new Vector2(x + 60f, y + 6f),
                    new Vector2(x + 12f, y + 62f), new Vector2(x - 12f, y + 66f)
                });
                RoundTree(c, x, y - 2f, 110f, Canvas2D.Hex("9A7B60"), Canvas2D.Hex("9AD190"), Canvas2D.Hex("7FB673"));
            }

            // clouds under the island
            for (int i = 0; i < 4; i++)
                c.SoftEllipse(cx + (i - 1.5f) * c.W * 0.16f, iy + 250f, 150f, 46f, Canvas2D.Hex("FFFFFF", 0.75f), 0.7f);
        }

        public static void Forest(Canvas2D c)
        {
            c.VerticalGradient(Canvas2D.Hex("EAF8F4"), Canvas2D.Hex("FFF4E2"));
            Rainbow(c, c.W * 0.50f, c.H * 0.86f, c.W * 0.46f, 22f);

            Cloud(c, c.W * 0.82f, c.H * 0.14f, 0.8f, Canvas2D.Hex("FFFFFF", 0.9f), Canvas2D.Hex("E6EFF4", 0.9f));

            // far hills
            c.Ellipse(c.W * 0.16f, c.H * 0.78f, c.W * 0.34f, c.H * 0.20f, Canvas2D.Hex("C8E7BC"));
            c.Ellipse(c.W * 0.62f, c.H * 0.80f, c.W * 0.40f, c.H * 0.22f, Canvas2D.Hex("BCE0B0"));
            c.Ellipse(c.W * 0.95f, c.H * 0.76f, c.W * 0.26f, c.H * 0.18f, Canvas2D.Hex("C8E7BC"));

            // ground
            c.Ellipse(c.W * 0.5f, c.H * 1.02f, c.W * 0.85f, c.H * 0.30f, Pal.Grass);
            c.Ellipse(c.W * 0.5f, c.H * 1.05f, c.W * 0.80f, c.H * 0.22f, Canvas2D.Hex("C4E7B0"));

            // tree band
            for (int i = 0; i < 9; i++)
            {
                float x = (i + 0.5f) / 9f * c.W;
                float h = c.H * (0.30f + (i % 3) * 0.05f);
                bool pine = i % 2 == 1;
                if (pine) PineTree(c, x, c.H * 0.86f, h, Canvas2D.Hex("A5825F"), Canvas2D.Hex("7FB673"));
                else RoundTree(c, x, c.H * 0.86f, h, Canvas2D.Hex("A5825F"), Canvas2D.Hex("8FCB8A"), Canvas2D.Hex("76B06E"));
            }

            // mushroom house
            float mx = c.W * 0.78f, my = c.H * 0.79f;
            c.RoundRect(mx - 52f, my - 26f, 104f, 92f, 18f, Canvas2D.Hex("FFF3E4"));
            c.Ellipse(mx, my - 26f, 96f, 58f, Canvas2D.Hex("F58F8F"));
            c.Circle(mx - 40f, my - 44f, 12f, Canvas2D.Hex("FFF0EC"));
            c.Circle(mx + 20f, my - 56f, 9f, Canvas2D.Hex("FFF0EC"));
            c.Circle(mx + 56f, my - 30f, 8f, Canvas2D.Hex("FFF0EC"));
            c.RoundRect(mx - 18f, my + 6f, 36f, 60f, 12f, Canvas2D.Hex("C08E63"));

            // flowers
            for (int i = 0; i < 14; i++)
            {
                float x = (i * 97 % 1280) / 1280f * c.W;
                float y = c.H * (0.88f + (i % 4) * 0.022f);
                Flower(c, x, y, 20f + (i % 3) * 5f,
                    i % 3 == 0 ? Pal.PinkLight : (i % 3 == 1 ? Canvas2D.Hex("FFF3C0") : Canvas2D.Hex("D8C8F0")), Pal.Sun);
            }
        }

        public static void Village(Canvas2D c)
        {
            c.VerticalGradient(Canvas2D.Hex("E7F4FB"), Canvas2D.Hex("FFF6EA"));
            Cloud(c, c.W * 0.20f, c.H * 0.13f, 0.95f, Canvas2D.Hex("FFFFFF"), Canvas2D.Hex("E6EFF4"));
            Cloud(c, c.W * 0.74f, c.H * 0.09f, 0.75f, Canvas2D.Hex("FFFFFF"), Canvas2D.Hex("E6EFF4"));

            // distant hills behind the village
            c.Ellipse(c.W * 0.22f, c.H * 0.54f, c.W * 0.34f, c.H * 0.13f, Canvas2D.Hex("D3EBC7"));
            c.Ellipse(c.W * 0.70f, c.H * 0.55f, c.W * 0.38f, c.H * 0.14f, Canvas2D.Hex("CBE6BF"));

            // small toy-block houses sitting on the horizon
            var cols = new[]
            {
                Canvas2D.Hex("F7A8B2"), Canvas2D.Hex("A8E6CF"), Canvas2D.Hex("C7CEEA"),
                Canvas2D.Hex("FFC7A8"), Canvas2D.Hex("FFE08A")
            };
            float horizon = c.H * 0.545f;

            for (int i = 0; i < 6; i++)
            {
                float w = c.W * 0.075f;
                float h = c.H * (0.085f + (i % 3) * 0.012f);
                float x = c.W * (0.07f + i * 0.165f);
                float y = horizon - h;

                c.RoundRect(x - w * 0.5f, y, w, h + 8f, 10f, cols[i % cols.Length]);
                c.RoundRect(x - w * 0.5f, y - 12f, w, 16f, 6f, Canvas2D.Lerp(cols[i % cols.Length], Color.black, 0.10f));
                c.RoundRect(x - 9f, y + h * 0.42f, 18f, 26f, 5f, Canvas2D.Hex("FFF6E9"));
                c.Circle(x - w * 0.26f, y + h * 0.28f, 7f, Canvas2D.Hex("FFFCF6"));
                c.Circle(x + w * 0.24f, y + h * 0.28f, 7f, Canvas2D.Hex("FFFCF6"));

                // little flag
                if (i % 2 == 0)
                {
                    c.Rect(x + w * 0.10f, y - 34f, 3f, 24f, Canvas2D.Hex("B08968"));
                    c.Poly(cols[(i + 2) % cols.Length], new Vector2[]
                    {
                        new Vector2(x + w * 0.10f, y - 34f),
                        new Vector2(x + w * 0.10f + 22f, y - 27f),
                        new Vector2(x + w * 0.10f, y - 20f)
                    });
                }
            }

            // wide sunny meadow
            c.Ellipse(c.W * 0.5f, c.H * 1.16f, c.W * 0.98f, c.H * 0.63f, Canvas2D.Hex("D6EFC6"));
            c.Ellipse(c.W * 0.5f, c.H * 1.22f, c.W * 0.92f, c.H * 0.60f, Canvas2D.Hex("C9EAB8"));

            // a few low blocks and flowers far in the foreground
            for (int i = 0; i < 8; i++)
            {
                float x = c.W * (0.04f + i * 0.125f);
                float y = c.H * 0.955f;
                float s = 18f + (i % 3) * 6f;
                c.RoundRect(x, y, s, s, 5f, cols[i % cols.Length]);
            }

            for (int i = 0; i < 5; i++)
                Flower(c, c.W * (0.10f + i * 0.20f), c.H * 0.90f, 16f, Canvas2D.Hex("FFF3C0"), Pal.Sun);
        }

        public static void Cove(Canvas2D c)
        {
            c.VerticalGradient(Canvas2D.Hex("DCE6F7"), Canvas2D.Hex("FBE8EF"));

            // moon
            c.Glow(c.W * 0.78f, c.H * 0.16f, c.W * 0.20f, Canvas2D.Hex("FFF4D0", 0.6f), 2f);
            c.Circle(c.W * 0.78f, c.H * 0.16f, c.W * 0.046f, Canvas2D.Hex("FFF8E4"));
            c.Circle(c.W * 0.766f, c.H * 0.148f, c.W * 0.013f, Canvas2D.Hex("FFF0C8", 0.9f));
            c.Circle(c.W * 0.790f, c.H * 0.172f, c.W * 0.009f, Canvas2D.Hex("FFF0C8", 0.8f));
            c.Circle(c.W * 0.792f, c.H * 0.140f, c.W * 0.006f, Canvas2D.Hex("FFF0C8", 0.7f));

            Cloud(c, c.W * 0.14f, c.H * 0.12f, 1f, Canvas2D.Hex("FFFFFF", 0.95f), Canvas2D.Hex("E4E9F6"));
            Cloud(c, c.W * 0.46f, c.H * 0.07f, 0.7f, Canvas2D.Hex("FFFFFF", 0.9f), Canvas2D.Hex("E4E9F6"));
            Cloud(c, c.W * 0.90f, c.H * 0.38f, 0.85f, Canvas2D.Hex("FFFFFF", 0.9f), Canvas2D.Hex("E4E9F6"));

            // ground
            c.Ellipse(c.W * 0.5f, c.H * 1.06f, c.W * 0.92f, c.H * 0.28f, Canvas2D.Hex("C7DFC0"));
            c.Ellipse(c.W * 0.5f, c.H * 1.10f, c.W * 0.86f, c.H * 0.20f, Canvas2D.Hex("B7D6AE"));

            // lake
            c.Ellipse(c.W * 0.30f, c.H * 0.88f, c.W * 0.20f, c.H * 0.08f, Canvas2D.Hex("A9D6E8"));
            c.Ellipse(c.W * 0.30f, c.H * 0.875f, c.W * 0.16f, c.H * 0.055f, Canvas2D.Hex("C3E7F2"));
            c.Ellipse(c.W * 0.26f, c.H * 0.855f, 40f, 8f, Canvas2D.Hex("FFFFFF", 0.7f));

            // cottage
            float hx = c.W * 0.70f, hy = c.H * 0.80f;
            c.RoundRect(hx - 70f, hy - 40f, 140f, 96f, 16f, Canvas2D.Hex("FFF3E9"));
            c.Poly(Canvas2D.Hex("C7A8D8"), new Vector2[]
            {
                new Vector2(hx - 92f, hy - 36f),
                new Vector2(hx + 92f, hy - 36f),
                new Vector2(hx, hy - 110f)
            });
            c.RoundRect(hx - 18f, hy + 6f, 36f, 50f, 8f, Canvas2D.Hex("C08E63"));
            c.Circle(hx - 44f, hy + 2f, 14f, Canvas2D.Hex("FFE08A"));

            for (int i = 0; i < 12; i++)
            {
                float x = c.W * (0.04f + (i * 0.081f) % 0.94f);
                float y = c.H * (0.93f + (i % 3) * 0.02f);
                Flower(c, x, y, 16f + (i % 2) * 6f, i % 2 == 0 ? Canvas2D.Hex("FBD3DA") : Canvas2D.Hex("E0D4F5"), Pal.Sun);
            }

            for (int i = 0; i < 18; i++)
            {
                float x = (i * 211 % 1280) / 1280f * c.W;
                float y = (i * 97 % 400) / 400f * c.H * 0.55f;
                Sparkle(c, x, y, 3f + (i % 2) * 2f, Canvas2D.Hex("FFFFFF", 0.65f));
            }
        }

        public static void Ending(Canvas2D c)
        {
            c.VerticalGradient(Canvas2D.Hex("4E5A86"), Canvas2D.Hex("F3C9C0"));

            for (int i = 0; i < 46; i++)
            {
                float x = (i * 173 % 1279) / 1279f * c.W;
                float y = (i * 89 % 500) / 500f * c.H * 0.62f;
                float r = 3f + (i % 4) * 1.6f;
                Sparkle(c, x, y, r, Canvas2D.Hex("FFF6D8", 0.55f + (i % 3) * 0.15f));
            }

            c.Glow(c.W * 0.5f, c.H * 0.72f, c.W * 0.40f, Canvas2D.Hex("FFE9B0", 0.55f), 2.2f);

            // hill
            c.Ellipse(c.W * 0.5f, c.H * 1.10f, c.W * 0.72f, c.H * 0.32f, Canvas2D.Hex("7FA98F"));
            c.Ellipse(c.W * 0.5f, c.H * 1.14f, c.W * 0.62f, c.H * 0.24f, Canvas2D.Hex("8FB79B"));
        }

        // ----------------------------------------------------------------- props

        public static void MoodTree(Canvas2D c, float scale = 1f)
        {
            float cx = c.W * 0.5f;
            float groundY = c.H * 0.94f;
            float h = c.H * 0.62f * scale;

            c.SoftEllipse(cx, groundY + 10f, c.W * 0.30f, 26f, Pal.Shadow, 0.8f);
            c.RoundRect(cx - 26f, groundY - h * 0.55f, 52f, h * 0.55f, 22f, Canvas2D.Hex("9A7B60"));
            c.RoundRect(cx - 10f, groundY - h * 0.52f, 10f, h * 0.48f, 5f, Canvas2D.Hex("B08F72"));

            // branches
            c.Line(cx, groundY - h * 0.35f, cx - c.W * 0.16f, groundY - h * 0.62f, 22f, Canvas2D.Hex("9A7B60"));
            c.Line(cx, groundY - h * 0.40f, cx + c.W * 0.15f, groundY - h * 0.66f, 22f, Canvas2D.Hex("9A7B60"));

            // canopy blobs
            float cy = groundY - h * 0.80f;
            var leafA = Canvas2D.Hex("8FCB8A");
            var leafB = Canvas2D.Hex("A8DCA2");
            var leafC = Canvas2D.Hex("76B06E");

            c.Circle(cx - c.W * 0.14f, cy + 30f, c.W * 0.10f, leafC);
            c.Circle(cx + c.W * 0.15f, cy + 26f, c.W * 0.105f, leafC);
            c.Circle(cx, cy - 10f, c.W * 0.15f, leafA);
            c.Circle(cx - c.W * 0.11f, cy + 10f, c.W * 0.10f, leafB);
            c.Circle(cx + c.W * 0.12f, cy + 6f, c.W * 0.10f, leafB);
            c.Circle(cx - c.W * 0.03f, cy - 0.10f * c.H, c.W * 0.075f, leafB);

            // glowing fruit
            for (int i = 0; i < 9; i++)
            {
                float a = i / 9f * Mathf.PI * 2f;
                float rx = c.W * (0.08f + (i % 3) * 0.025f);
                float x = cx + Mathf.Cos(a) * rx;
                float y = cy + Mathf.Sin(a) * rx * 0.75f;
                c.Glow(x, y, 26f, Canvas2D.Hex("FFE9A8", 0.55f), 1.6f);
                c.Circle(x, y, 7f, Canvas2D.Hex("FFF6D0"));
            }
        }

        public static void CloudBoat(Canvas2D c)
        {
            float cx = c.W * 0.5f, cy = c.H * 0.55f;
            // clouds
            c.Ellipse(cx, cy + 40f, c.W * 0.30f, 42f, Canvas2D.Hex("FFFFFF", 0.95f));
            c.Circle(cx - 90f, cy + 34f, 38f, Canvas2D.Hex("FFFFFF"));
            c.Circle(cx + 96f, cy + 32f, 34f, Canvas2D.Hex("FFFFFF"));
            c.Ellipse(cx, cy + 66f, c.W * 0.24f, 24f, Canvas2D.Hex("E6EFF4", 0.9f));

            // hull
            c.Poly(Canvas2D.Hex("C08E63"), new Vector2[]
            {
                new Vector2(cx - 130f, cy + 6f),
                new Vector2(cx + 130f, cy + 6f),
                new Vector2(cx + 88f, cy + 54f),
                new Vector2(cx - 88f, cy + 54f)
            });
            c.Rect(cx - 132f, cy - 2f, 264f, 12f, Canvas2D.Hex("A5825F"));

            // mast + sail
            c.Rect(cx - 4f, cy - 200f, 8f, 208f, Canvas2D.Hex("A5825F"));
            c.Poly(Canvas2D.Hex("FFF6E9"), new Vector2[]
            {
                new Vector2(cx + 6f, cy - 190f),
                new Vector2(cx + 6f, cy - 20f),
                new Vector2(cx + 118f, cy - 40f)
            });
            c.Poly(Canvas2D.Hex("F7C6C9"), new Vector2[]
            {
                new Vector2(cx - 6f, cy - 176f),
                new Vector2(cx - 6f, cy - 30f),
                new Vector2(cx - 92f, cy - 60f)
            });
            c.Circle(cx, cy - 204f, 10f, Canvas2D.Hex("FFE08A"));
        }

        public static void Basket(Canvas2D c, bool withPinecones = true)
        {
            float cx = c.W * 0.5f, cy = c.H * 0.62f;
            c.SoftEllipse(cx, cy + 92f, 130f, 22f, Pal.Shadow, 0.8f);
            c.Poly(Canvas2D.Hex("C89B6D"), new Vector2[]
            {
                new Vector2(cx - 120f, cy - 30f),
                new Vector2(cx + 120f, cy - 30f),
                new Vector2(cx + 92f, cy + 84f),
                new Vector2(cx - 92f, cy + 84f)
            });
            c.Rect(cx - 122f, cy - 42f, 244f, 18f, Canvas2D.Hex("A5825F"));

            // weave
            for (int i = 1; i < 4; i++)
                c.Rect(cx - 120f + i * 2f, cy - 30f + i * 28f, 236f - i * 4f, 5f, Canvas2D.Hex("A5825F", 0.55f));

            // handle
            for (int i = 0; i <= 24; i++)
            {
                float a = Mathf.PI * (i / 24f);
                float x = cx + Mathf.Cos(a) * 110f;
                float y = cy - 34f - Mathf.Sin(a) * 86f;
                c.Circle(x, y, 9f, Canvas2D.Hex("A5825F"));
            }

            if (!withPinecones) return;

            for (int i = 0; i < 5; i++)
            {
                float x = cx - 60f + i * 30f;
                float y = cy - 34f - (i % 2) * 12f;
                Pinecone(c, x, y, 0.5f);
            }
        }

        public static void Pinecone(Canvas2D c, float cx, float cy, float s)
        {
            c.Ellipse(cx, cy, 20f * s, 26f * s, Canvas2D.Hex("A9713F"));
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    float x = cx + (j == 0 ? -8f : 8f) * s;
                    float y = cy - 14f * s + i * 12f * s;
                    c.Poly(Canvas2D.Hex("8E5C33"), new Vector2[]
                    {
                        new Vector2(x - 7f * s, y),
                        new Vector2(x + 7f * s, y),
                        new Vector2(x, y + 9f * s)
                    });
                }
            }
            c.Rect(cx - 2f * s, cy - 34f * s, 4f * s, 10f * s, Canvas2D.Hex("7A4C2A"));
        }

        public static void PineconeTower(Canvas2D c, bool fallen)
        {
            float cx = c.W * 0.5f, baseY = c.H * 0.92f;

            if (fallen)
            {
                c.SoftEllipse(cx, baseY + 4f, 190f, 22f, Pal.Shadow, 0.8f);
                Pinecone(c, cx - 120f, baseY - 34f, 2.2f);
                Pinecone(c, cx + 40f, baseY - 24f, 2.0f);
                Pinecone(c, cx + 140f, baseY - 40f, 1.7f);
                Pinecone(c, cx - 30f, baseY - 62f, 1.5f);
                c.Ellipse(cx + 90f, baseY - 4f, 34f, 9f, Canvas2D.Hex("A9713F", 0.5f));
                return;
            }

            c.SoftEllipse(cx, baseY + 2f, 110f, 18f, Pal.Shadow, 0.8f);
            Pinecone(c, cx, baseY - 70f, 2.6f);
            Pinecone(c, cx, baseY - 168f, 2.1f);
            Pinecone(c, cx, baseY - 248f, 1.6f);
        }

        public static void BlockCastle(Canvas2D c, bool broken, int count = 6)
        {
            float cx = c.W * 0.5f, baseY = c.H * 0.88f;
            var cols = new[]
            {
                Canvas2D.Hex("F7A8B2"), Canvas2D.Hex("A8E6CF"), Canvas2D.Hex("C7CEEA"),
                Canvas2D.Hex("FFC7A8"), Canvas2D.Hex("FFE08A"), Canvas2D.Hex("8FB8E8")
            };

            c.SoftEllipse(cx, baseY + 8f, 150f, 22f, Pal.Shadow, 0.8f);

            if (broken)
            {
                for (int i = 0; i < count; i++)
                {
                    float x = cx - 120f + (i % 3) * 88f + (i * 17 % 20);
                    float y = baseY - (i / 3) * 56f - 12f;
                    c.RoundRect(x, y - 44f, 64f, 56f, 10f, cols[i % cols.Length]);
                }
                return;
            }

            // 3-2-1 pyramid
            int[] rowCounts = { 3, 2, 1 };
            float bw = 64f, bh = 54f;
            for (int row = 0; row < rowCounts.Length; row++)
            {
                int n = rowCounts[row];
                for (int i = 0; i < n; i++)
                {
                    float x = cx - (n * bw) * 0.5f + i * bw;
                    float y = baseY - (row + 1) * bh;
                    c.RoundRect(x + 2f, y, bw - 6f, bh - 4f, 10f, cols[(row * 2 + i) % cols.Length]);
                }
            }
        }

        public static void BlueBlock(Canvas2D c)
        {
            float cx = c.W * 0.5f, cy = c.H * 0.5f;
            c.SoftEllipse(cx, cy + 74f, 90f, 18f, Pal.Shadow, 0.8f);
            c.RoundRect(cx - 62f, cy - 62f, 124f, 124f, 18f, Canvas2D.Hex("8FB8E8"));
            c.RoundRect(cx - 52f, cy - 52f, 104f, 40f, 12f, Canvas2D.Hex("A8CAF0"));
            c.Circle(cx - 26f, cy + 24f, 8f, Canvas2D.Hex("6F98C9"));
            c.Circle(cx + 26f, cy + 24f, 8f, Canvas2D.Hex("6F98C9"));
        }

        public static void Portal(Canvas2D c, string theme)
        {
            float cx = c.W * 0.5f, baseY = c.H * 0.94f;
            float halfW = c.W * 0.36f;
            float height = c.H * 0.68f;

            Color32 a, b;
            switch (theme)
            {
                case "village": a = Canvas2D.Hex("C7CEEA"); b = Canvas2D.Hex("8FB8E8"); break;
                case "cove": a = Canvas2D.Hex("FBD3DA"); b = Canvas2D.Hex("C7A8D8"); break;
                default: a = Canvas2D.Hex("FFE0A8"); b = Canvas2D.Hex("F7A8B2"); break;
            }

            c.SoftEllipse(cx, baseY + 4f, halfW * 1.1f, 20f, Pal.Shadow, 0.8f);

            // arch
            c.RoundRect(cx - halfW, baseY - height, halfW * 2f, height + 4f, halfW, Canvas2D.Hex("FFF6E9"));
            c.RoundRect(cx - halfW + 16f, baseY - height + 16f, (halfW - 16f) * 2f, height - 18f, halfW - 16f, a);

            // glow inside
            c.Glow(cx, baseY - height * 0.52f, halfW * 0.95f, Canvas2D.Alpha(b, 0.8f), 1.5f);

            float iconY = baseY - height * 0.52f;

            switch (theme)
            {
                case "forest":
                {
                    c.Circle(cx, iconY - 18f, 42f, Canvas2D.Hex("FFE08A"));
                    c.Poly(Canvas2D.Hex("76B06E"), new Vector2[]
                    {
                        new Vector2(cx - 62f, iconY + 44f),
                        new Vector2(cx + 62f, iconY + 44f),
                        new Vector2(cx, iconY - 66f)
                    });
                    c.Poly(Canvas2D.Hex("8FCB8A"), new Vector2[]
                    {
                        new Vector2(cx - 44f, iconY + 46f),
                        new Vector2(cx + 44f, iconY + 46f),
                        new Vector2(cx, iconY - 34f)
                    });
                    break;
                }
                case "village":
                {
                    c.RoundRect(cx - 58f, iconY - 6f, 116f, 58f, 12f, Canvas2D.Hex("F7A8B2"));
                    c.RoundRect(cx - 42f, iconY - 62f, 84f, 58f, 12f, Canvas2D.Hex("FFE08A"));
                    c.RoundRect(cx - 50f, iconY - 8f, 34f, 20f, 5f, Canvas2D.Hex("FFFCF6"));
                    c.RoundRect(cx + 16f, iconY - 8f, 34f, 20f, 5f, Canvas2D.Hex("FFFCF6"));
                    break;
                }
                default:
                {
                    c.Circle(cx + 14f, iconY, 44f, Canvas2D.Hex("FFF8E4"));
                    c.Circle(cx + 34f, iconY - 16f, 38f, Canvas2D.Alpha(b, 0.95f));
                    c.Circle(cx - 30f, iconY + 26f, 10f, Canvas2D.Hex("FFFFFF", 0.85f));
                    break;
                }
            }

            // little steps
            c.RoundRect(cx - halfW - 18f, baseY - 8f, (halfW + 18f) * 2f, 22f, 11f, Canvas2D.Hex("E4D4C2"));
        }

        public static void ClueFace(Canvas2D c, string portraitPath)
        {
            c.RoundRect(20f, 20f, c.W - 40f, c.H - 40f, 40f, Canvas2D.Hex("FFF6E9"));
            c.DrawPng(portraitPath, 29, 32, 362);
        }

        public static void ClueBody(Canvas2D c, string bodyPath)
        {
            c.RoundRect(20f, 20f, c.W - 40f, c.H - 40f, 40f, Canvas2D.Hex("FFF6E9"));
            float cx = c.W * 0.5f, baseY = c.H * 0.84f;
            c.SoftEllipse(cx, baseY, 100f, 18f, Pal.Shadow, 0.8f);
            c.DrawPng(bodyPath, 34, 29, 352);
        }

        public static void ClueSituation(Canvas2D c)
        {
            c.RoundRect(20f, 20f, c.W - 40f, c.H - 40f, 40f, Canvas2D.Hex("FFF6E9"));
            float cx = c.W * 0.5f, baseY = c.H * 0.84f;
            c.SoftEllipse(cx, baseY, 130f, 18f, Pal.Shadow, 0.8f);
            Pinecone(c, cx - 92f, baseY - 34f, 1.7f);
            Pinecone(c, cx + 8f, baseY - 24f, 1.6f);
            Pinecone(c, cx + 100f, baseY - 38f, 1.4f);
            // motion lines
            for (int i = 0; i < 3; i++)
                c.Line(cx - 110f + i * 110f, baseY - 130f - i * 14f, cx - 76f + i * 110f, baseY - 160f - i * 14f, 6f, Canvas2D.Hex("C0AFA0", 0.7f));
        }

        public static void Situation(Canvas2D c, int index)
        {
            c.RoundRect(16f, 16f, c.W - 32f, c.H - 32f, 44f, Canvas2D.Hex("FFFFFF"));
            c.RoundRect(16f, 16f, c.W - 32f, c.H - 32f, 44f, Canvas2D.Hex("FFF9F0", 0.75f));

            switch (index)
            {
                case 0:
                {
                    float cx = c.W * 0.5f, baseY = c.H * 0.84f;
                    var cols = new[] { Canvas2D.Hex("F7A8B2"), Canvas2D.Hex("A8E6CF"), Canvas2D.Hex("C7CEEA") };
                    c.RoundRect(cx - 70f, baseY - 60f, 60f, 60f, 10f, cols[0]);
                    c.RoundRect(cx + 10f, baseY - 60f, 60f, 60f, 10f, cols[1]);
                    c.RoundRect(cx - 40f, baseY - 130f, 60f, 60f, 10f, cols[2]);
                    c.RoundRect(cx + 70f, baseY - 26f, 52f, 52f, 10f, Canvas2D.Hex("FFE08A"));
                    c.RoundRect(cx - 110f, baseY - 22f, 48f, 48f, 10f, Canvas2D.Hex("FFC7A8"));
                    break;
                }
                case 1:
                {
                    float cx = c.W * 0.5f, cy = c.H * 0.5f;
                    c.Circle(cx, cy, 84f, Canvas2D.Hex("FFFFFF"));
                    c.Circle(cx, cy, 74f, Canvas2D.Hex("C7CEEA"));
                    c.Line(cx, cy, cx, cy - 46f, 9f, Canvas2D.Hex("FFFFFF"));
                    c.Line(cx, cy, cx + 36f, cy + 12f, 9f, Canvas2D.Hex("FFFFFF"));
                    c.Circle(cx, cy, 9f, Canvas2D.Hex("FFFFFF"));
                    c.Ellipse(cx - 110f, cy + 60f, 46f, 26f, Canvas2D.Hex("FFFFFF", 0.9f));
                    c.Ellipse(cx + 110f, cy - 60f, 40f, 24f, Canvas2D.Hex("FFFFFF", 0.9f));
                    break;
                }
                default:
                {
                    float cx = c.W * 0.5f, cy = c.H * 0.54f;
                    c.RoundRect(cx - 78f, cy - 54f, 156f, 108f, 16f, Canvas2D.Hex("F7A8B2"));
                    c.Rect(cx - 78f, cy - 12f, 156f, 26f, Canvas2D.Hex("FFF6E9"));
                    c.Rect(cx - 8f, cy - 54f, 16f, 108f, Canvas2D.Hex("FFF6E9"));
                    c.Poly(Canvas2D.Hex("8FCB8A"), new Vector2[]
                    {
                        new Vector2(cx - 34f, cy - 54f),
                        new Vector2(cx - 12f, cy - 54f),
                        new Vector2(cx - 6f, cy - 96f),
                        new Vector2(cx - 40f, cy - 96f)
                    });
                    c.Poly(Canvas2D.Hex("8FCB8A"), new Vector2[]
                    {
                        new Vector2(cx + 12f, cy - 54f),
                        new Vector2(cx + 34f, cy - 54f),
                        new Vector2(cx + 40f, cy - 96f),
                        new Vector2(cx + 6f, cy - 96f)
                    });
                    break;
                }
            }
        }

        public static void Mushroom(Canvas2D c)
        {
            float cx = c.W * 0.5f, baseY = c.H * 0.86f;
            c.SoftEllipse(cx, baseY + 6f, 80f, 16f, Pal.Shadow, 0.8f);
            c.RoundRect(cx - 26f, baseY - 92f, 52f, 92f, 16f, Canvas2D.Hex("FFF3E4"));
            c.Ellipse(cx, baseY - 96f, 88f, 54f, Canvas2D.Hex("F58F8F"));
            c.Circle(cx - 38f, baseY - 112f, 13f, Canvas2D.Hex("FFF0EC"));
            c.Circle(cx + 22f, baseY - 124f, 9f, Canvas2D.Hex("FFF0EC"));
            c.Circle(cx + 52f, baseY - 100f, 8f, Canvas2D.Hex("FFF0EC"));
        }
    }
}
