using UnityEngine;

namespace MoodieLand.EditorTools
{
    /// <summary>UI surfaces, badges, emotion icons and symbol icons.</summary>
    public static class UIAssets
    {
        // ---------------------------------------------------------- flat panels

        /// <summary>Rounded surface with a soft drop shadow, drawn for 9-slicing.</summary>
        public static void Panel(Canvas2D c, Color32 fill, Color32 edge, float radius, float edgeWidth, bool shadow)
        {
            float m = 10f;
            if (shadow)
            {
                for (int i = 6; i >= 1; i--)
                    c.RoundRect(m, m + i * 3.2f, c.W - m * 2f, c.H - m * 2f - i * 1.4f, radius, Canvas2D.Alpha(Pal.Shadow, 0.055f * i * 0.5f));
            }
            c.RoundRect(m, m, c.W - m * 2f, c.H - m * 2f, radius, edge);
            c.RoundRect(m + edgeWidth, m + edgeWidth, c.W - (m + edgeWidth) * 2f, c.H - (m + edgeWidth) * 2f,
                Mathf.Max(2f, radius - edgeWidth), fill);
        }

        public static void CardSurface(Canvas2D c, bool selected)
        {
            float m = 12f;
            float r = 34f;
            for (int i = 5; i >= 1; i--)
                c.RoundRect(m + 4f, m + i * 3.4f, c.W - (m + 4f) * 2f, c.H - (m + 4f) * 2f - i * 1.2f, r,
                    Canvas2D.Alpha(Pal.Shadow, 0.06f * i));

            c.RoundRect(m, m, c.W - m * 2f, c.H - m * 2f, r, Canvas2D.Hex("EFE4D6"));
            c.RoundRect(m + 4f, m + 4f, c.W - (m + 4f) * 2f, c.H - (m + 4f) * 2f, r - 3f, Pal.White);
            c.RoundRect(m + 4f, m + 4f, c.W - (m + 4f) * 2f, (c.H - (m + 4f) * 2f) * 0.42f, r - 3f,
                Canvas2D.Hex("FFFDFA", 0.9f));
        }

        public static void SelectionFrame(Canvas2D c)
        {
            float m = 10f;
            float w = c.W - m * 2f;
            float h = c.H - m * 2f;
            RoundRectOutline(c, m, m, w, h, 34f, 8f, Pal.Teal);
            Glob(c, m + 8f, m + 8f, 30f, 8f, Canvas2D.Hex("BFE8E2", 0.55f));
        }

        /// <summary>Stroked rounded rectangle (used for selection outlines).</summary>
        public static void RoundRectOutline(Canvas2D c, float x, float y, float w, float h, float r, float t, Color32 col)
        {
            r = Mathf.Min(r, Mathf.Min(w, h) * 0.5f);
            c.Rect(x + r, y, w - r * 2f, t, col);
            c.Rect(x + r, y + h - t, w - r * 2f, t, col);
            c.Rect(x, y + r, t, h - r * 2f, col);
            c.Rect(x + w - t, y + r, t, h - r * 2f, col);

            float rr = r - t * 0.5f;
            float cx0 = x + r, cy0 = y + r, cx1 = x + w - r, cy1 = y + h - r;
            c.Arc(cx0, cy0, rr, 180f, 270f, t, col);
            c.Arc(cx1, cy0, rr, 270f, 360f, t, col);
            c.Arc(cx1, cy1, rr, 0f, 90f, t, col);
            c.Arc(cx0, cy1, rr, 90f, 180f, t, col);
        }

        static void Glob(Canvas2D c, float x, float y, float w, float h, Color32 col)
        {
            c.RoundRect(x, y, w, h, 24f, col);
        }

        public static void SoftCircle(Canvas2D c, Color32 fill)
        {
            c.Circle(c.W * 0.5f, c.H * 0.5f, c.W * 0.48f, fill);
        }

        public static void GlowRing(Canvas2D c, Color32 col)
        {
            float cx = c.W * 0.5f, cy = c.H * 0.5f;
            c.Glow(cx, cy, c.W * 0.5f, Canvas2D.Alpha(col, 0.55f), 1.4f);
            c.Glow(cx, cy, c.W * 0.34f, Canvas2D.Alpha(col, 0.35f), 1.2f);
        }

        public static void CheckMark(Canvas2D c, Color32 bg)
        {
            float cx = c.W * 0.5f, cy = c.H * 0.5f;
            c.Circle(cx, cy, c.W * 0.44f, bg);
            c.Polyline(new[]
            {
                new Vector2(cx - c.W * 0.18f, cy + c.W * 0.01f),
                new Vector2(cx - c.W * 0.04f, cy + c.W * 0.16f),
                new Vector2(cx + c.W * 0.20f, cy - c.W * 0.15f)
            }, c.W * 0.10f, Pal.White);
        }

        // -------------------------------------------------------------- badges

        public static void BadgeRainbow(Canvas2D c)
        {
            float cx = c.W * 0.5f, cy = c.H * 0.5f, r = c.W * 0.42f;
            c.Glow(cx, cy, r * 1.25f, Canvas2D.Hex("FFE9B8", 0.6f), 2f);
            c.Circle(cx, cy, r, Canvas2D.Hex("FFF3D6"));
            c.Circle(cx, cy, r * 0.86f, Pal.White);

            var cols = new[]
            {
                Canvas2D.Hex("F7A8B2"), Canvas2D.Hex("FFC7A8"), Canvas2D.Hex("FFE08A"),
                Canvas2D.Hex("A8E6CF"), Canvas2D.Hex("8FB8E8"), Canvas2D.Hex("C7CEEA")
            };

            float baseY = cy + r * 0.34f;
            float outer = r * 0.80f;
            float band = r * 0.075f;
            const int steps = 44;

            for (int b = 0; b < cols.Length; b++)
            {
                float rOut = outer - b * band;
                float rIn = rOut - band * 0.94f;
                var pts = new Vector2[steps * 2 + 2];
                for (int a = 0; a <= steps; a++)
                {
                    float t = Mathf.PI * (a / (float)steps);
                    pts[a] = new Vector2(cx - Mathf.Cos(t) * rOut, baseY - Mathf.Sin(t) * rOut);
                    pts[pts.Length - 1 - a] = new Vector2(cx - Mathf.Cos(t) * rIn, baseY - Mathf.Sin(t) * rIn);
                }
                c.Poly(cols[b], pts);
            }

            // little clouds at the ends
            SceneArt.Cloud(c, cx - outer * 0.96f, baseY + r * 0.02f, 0.16f, Canvas2D.Hex("FFFFFF"), Canvas2D.Hex("EFE7DA"));
            SceneArt.Cloud(c, cx + outer * 0.96f, baseY + r * 0.02f, 0.16f, Canvas2D.Hex("FFFFFF"), Canvas2D.Hex("EFE7DA"));

            SceneArt.Sparkle(c, cx + r * 0.60f, cy - r * 0.54f, r * 0.15f, Canvas2D.Hex("FFFFFF", 0.95f));
            SceneArt.Sparkle(c, cx - r * 0.58f, cy - r * 0.40f, r * 0.10f, Canvas2D.Hex("FFFFFF", 0.85f));
        }

        public static void BadgeStar(Canvas2D c)
        {
            float cx = c.W * 0.5f, cy = c.H * 0.5f, r = c.W * 0.42f;
            c.Glow(cx, cy, r * 1.3f, Canvas2D.Hex("FFE9A8", 0.7f), 1.9f);
            c.Circle(cx, cy, r, Canvas2D.Hex("FFF3D6"));
            c.Circle(cx, cy, r * 0.87f, Canvas2D.Hex("FFF9EA"));
            c.Star(cx, cy, r * 0.66f, r * 0.30f, Canvas2D.Hex("F7C948"));
            c.Star(cx, cy - r * 0.03f, r * 0.56f, r * 0.25f, Canvas2D.Hex("FFE08A"));
            c.Circle(cx - r * 0.16f, cy - r * 0.18f, r * 0.05f, Canvas2D.Hex("FFFFFF", 0.8f));
        }

        public static void BadgeMoon(Canvas2D c)
        {
            float cx = c.W * 0.5f, cy = c.H * 0.5f, r = c.W * 0.42f;
            c.Glow(cx, cy, r * 1.3f, Canvas2D.Hex("D8D0F0", 0.65f), 2f);
            c.Circle(cx, cy, r, Canvas2D.Hex("EFE8FA"));
            c.Circle(cx, cy, r * 0.87f, Canvas2D.Hex("FBFAFF"));

            c.Circle(cx + r * 0.04f, cy - r * 0.06f, r * 0.50f, Canvas2D.Hex("FFF6D0"));
            c.Circle(cx + r * 0.34f, cy - r * 0.26f, r * 0.44f, Canvas2D.Hex("FBFAFF"));

            SceneArt.Cloud(c, cx - r * 0.10f, cy + r * 0.52f, 0.26f, Canvas2D.Hex("FFFFFF", 0.95f), Canvas2D.Hex("E7E1F5", 0.9f));
            SceneArt.Sparkle(c, cx - r * 0.52f, cy - r * 0.46f, r * 0.13f, Canvas2D.Hex("FFFFFF", 0.9f));
            SceneArt.Sparkle(c, cx + r * 0.56f, cy + r * 0.30f, r * 0.09f, Canvas2D.Hex("FFFFFF", 0.85f));
        }

        // -------------------------------------------------------- emotion icons

        public static void EmotionIcon(Canvas2D c, string id)
        {
            Color32 bg, ink = Pal.Ink;
            switch (id)
            {
                case "happy": bg = Canvas2D.Hex("FFE7A8"); break;
                case "sad": bg = Canvas2D.Hex("BBD9F0"); break;
                case "angry": bg = Canvas2D.Hex("F8B3A8"); break;
                case "worried": bg = Canvas2D.Hex("DCCDEC"); break;
                case "calm": bg = Canvas2D.Hex("BFE8E2"); break;
                case "frustrated": bg = Canvas2D.Hex("F8C2A8"); break;
                case "tired": bg = Canvas2D.Hex("D6D9E8"); break;
                case "excited": bg = Canvas2D.Hex("FFD9B0"); break;
                case "proud": bg = Canvas2D.Hex("FFE7A8"); break;
                default: bg = Canvas2D.Hex("E6E0D6"); break;
            }

            float cx = c.W * 0.5f, cy = c.H * 0.5f, r = c.W * 0.46f;
            c.Circle(cx, cy + r * 0.05f, r, Canvas2D.Alpha(bg, 0.55f));
            c.Circle(cx, cy, r * 0.94f, bg);
            c.Glow(cx, cy, r, Canvas2D.Hex("FFFFFF", 0.35f), 2f);

            float eyeDx = r * 0.32f, eyeY = cy - r * 0.10f, eyeR = r * 0.11f;

            switch (id)
            {
                case "happy":
                case "excited":
                    Arch(c, cx - eyeDx, eyeY, r * 0.13f, ink, true);
                    Arch(c, cx + eyeDx, eyeY, r * 0.13f, ink, true);
                    Mouth(c, cx, cy + r * 0.30f, r * 0.30f, r * 0.16f, true);
                    if (id == "excited")
                    {
                        SceneArt.Sparkle(c, cx - r * 0.66f, cy - r * 0.52f, r * 0.14f, Canvas2D.Hex("FFFFFF", 0.9f));
                        SceneArt.Sparkle(c, cx + r * 0.68f, cy - r * 0.40f, r * 0.10f, Canvas2D.Hex("FFFFFF", 0.85f));
                    }
                    break;

                case "sad":
                    Dot(c, cx - eyeDx, eyeY, eyeR, ink);
                    Dot(c, cx + eyeDx, eyeY, eyeR, ink);
                    Brow(c, cx - eyeDx, eyeY - r * 0.24f, r * 0.17f, r * 0.08f, ink);
                    Brow(c, cx + eyeDx, eyeY - r * 0.24f, r * 0.17f, -r * 0.08f, ink);
                    Mouth(c, cx, cy + r * 0.40f, r * 0.24f, r * 0.10f, false);
                    c.Ellipse(cx - eyeDx - r * 0.02f, eyeY + r * 0.22f, r * 0.05f, r * 0.09f, Canvas2D.Hex("8FCBE8"));
                    break;

                case "angry":
                    Dot(c, cx - eyeDx, eyeY, eyeR, ink);
                    Dot(c, cx + eyeDx, eyeY, eyeR, ink);
                    Brow(c, cx - eyeDx, eyeY - r * 0.22f, r * 0.19f, -r * 0.10f, ink);
                    Brow(c, cx + eyeDx, eyeY - r * 0.22f, r * 0.19f, r * 0.10f, ink);
                    Mouth(c, cx, cy + r * 0.42f, r * 0.22f, r * 0.09f, false);
                    break;

                case "worried":
                    Dot(c, cx - eyeDx, eyeY, eyeR * 0.95f, ink);
                    Dot(c, cx + eyeDx, eyeY, eyeR * 0.95f, ink);
                    Brow(c, cx - eyeDx, eyeY - r * 0.24f, r * 0.16f, r * 0.07f, ink);
                    Brow(c, cx + eyeDx, eyeY - r * 0.24f, r * 0.16f, -r * 0.07f, ink);
                    c.Polyline(new[]
                    {
                        new Vector2(cx - r * 0.18f, cy + r * 0.38f),
                        new Vector2(cx - r * 0.06f, cy + r * 0.32f),
                        new Vector2(cx + r * 0.08f, cy + r * 0.40f),
                        new Vector2(cx + r * 0.20f, cy + r * 0.34f)
                    }, r * 0.05f, ink);
                    break;

                case "calm":
                    Arch(c, cx - eyeDx, eyeY, r * 0.12f, ink, false);
                    Arch(c, cx + eyeDx, eyeY, r * 0.12f, ink, false);
                    Mouth(c, cx, cy + r * 0.32f, r * 0.20f, r * 0.07f, true);
                    break;

                case "frustrated":
                    Squint(c, cx - eyeDx, eyeY, r * 0.12f, ink);
                    Squint(c, cx + eyeDx, eyeY, r * 0.12f, ink);
                    Brow(c, cx - eyeDx, eyeY - r * 0.22f, r * 0.18f, -r * 0.09f, ink);
                    Brow(c, cx + eyeDx, eyeY - r * 0.22f, r * 0.18f, r * 0.09f, ink);
                    c.RoundRect(cx - r * 0.20f, cy + r * 0.34f, r * 0.40f, r * 0.14f, r * 0.05f, ink);
                    break;

                case "tired":
                    c.Line(cx - eyeDx - r * 0.12f, eyeY, cx - eyeDx + r * 0.12f, eyeY, r * 0.05f, ink);
                    c.Line(cx + eyeDx - r * 0.12f, eyeY, cx + eyeDx + r * 0.12f, eyeY, r * 0.05f, ink);
                    Mouth(c, cx, cy + r * 0.38f, r * 0.14f, r * 0.05f, false);
                    break;

                case "proud":
                    Arch(c, cx - eyeDx, eyeY, r * 0.12f, ink, true);
                    Arch(c, cx + eyeDx, eyeY, r * 0.12f, ink, true);
                    Mouth(c, cx, cy + r * 0.30f, r * 0.24f, r * 0.13f, true);
                    SceneArt.Sparkle(c, cx + r * 0.60f, cy - r * 0.50f, r * 0.13f, Canvas2D.Hex("FFFFFF", 0.9f));
                    break;

                default: // not sure
                    Dot(c, cx - eyeDx, eyeY, eyeR, ink);
                    Dot(c, cx + eyeDx, eyeY, eyeR, ink);
                    Brow(c, cx - eyeDx, eyeY - r * 0.24f, r * 0.16f, 0f, ink);
                    Brow(c, cx + eyeDx, eyeY - r * 0.28f, r * 0.16f, r * 0.08f, ink);
                    c.Line(cx - r * 0.12f, cy + r * 0.36f, cx + r * 0.12f, cy + r * 0.34f, r * 0.05f, ink);
                    break;
            }
        }

        static void Dot(Canvas2D c, float x, float y, float r, Color32 ink)
        {
            c.Ellipse(x, y, r, r * 1.1f, ink);
            c.Circle(x - r * 0.28f, y - r * 0.36f, r * 0.34f, Canvas2D.Hex("FFFFFF"));
        }

        static void Arch(Canvas2D c, float x, float y, float r, Color32 ink, bool up)
        {
            c.Polyline(new[]
            {
                new Vector2(x - r, y + (up ? r * 0.4f : -r * 0.4f)),
                new Vector2(x, y - (up ? r * 0.8f : -r * 0.8f)),
                new Vector2(x + r, y + (up ? r * 0.4f : -r * 0.4f))
            }, r * 0.55f, ink);
        }

        static void Squint(Canvas2D c, float x, float y, float r, Color32 ink)
        {
            c.Polyline(new[]
            {
                new Vector2(x - r, y - r * 0.8f),
                new Vector2(x + r * 0.9f, y),
                new Vector2(x - r, y + r * 0.8f)
            }, r * 0.45f, ink);
        }

        static void Brow(Canvas2D c, float x, float y, float len, float tilt, Color32 ink)
        {
            c.Line(x - len, y + tilt, x + len, y - tilt, len * 0.26f, ink);
        }

        static void Mouth(Canvas2D c, float cx, float y, float w, float h, bool smile)
        {
            if (smile)
            {
                c.Ellipse(cx, y, w, h, Canvas2D.Hex("8A5A50"));
                c.Ellipse(cx, y + h * 0.36f, w * 0.55f, h * 0.42f, Canvas2D.Hex("F58E9B"));
            }
            else
            {
                c.Polyline(new[]
                {
                    new Vector2(cx - w * 0.75f, y + h * 0.6f),
                    new Vector2(cx, y - h * 0.4f),
                    new Vector2(cx + w * 0.75f, y + h * 0.6f)
                }, h * 0.5f, Canvas2D.Hex("6B4F42"));
            }
        }

        // --------------------------------------------------------- symbol icons

        public static void SymbolIcon(Canvas2D c, string symbol, Color32 tint)
        {
            float cx = c.W * 0.5f, cy = c.H * 0.5f, r = c.W * 0.30f;

            c.Circle(cx, cy, c.W * 0.44f, Canvas2D.Alpha(tint, 0.22f));
            c.Glow(cx, cy, c.W * 0.46f, Canvas2D.Hex("FFFFFF", 0.5f), 2f);

            var ink = Canvas2D.Hex("5B4E45");
            var light = Canvas2D.Hex("FFFDF8");

            switch (symbol)
            {
                case "speech":
                case "speechcheck":
                    c.RoundRect(cx - r, cy - r * 0.85f, r * 2f, r * 1.4f, r * 0.42f, light);
                    c.RoundRect(cx - r, cy - r * 0.85f, r * 2f, r * 1.4f, r * 0.42f, Canvas2D.Alpha(tint, 0.35f));
                    c.Poly(light, new Vector2[]
                    {
                        new Vector2(cx - r * 0.30f, cy + r * 0.52f),
                        new Vector2(cx + r * 0.12f, cy + r * 0.55f),
                        new Vector2(cx - r * 0.05f, cy + r * 1.02f)
                    });
                    if (symbol == "speechcheck")
                        c.Polyline(new[]
                        {
                            new Vector2(cx - r * 0.40f, cy - r * 0.10f),
                            new Vector2(cx - r * 0.05f, cy + r * 0.24f),
                            new Vector2(cx + r * 0.48f, cy - r * 0.45f)
                        }, r * 0.16f, Pal.Teal);
                    else
                    {
                        c.Circle(cx - r * 0.42f, cy - r * 0.10f, r * 0.11f, ink);
                        c.Circle(cx, cy - r * 0.10f, r * 0.11f, ink);
                        c.Circle(cx + r * 0.42f, cy - r * 0.10f, r * 0.11f, ink);
                    }
                    break;

                case "eye":
                    c.Ellipse(cx, cy, r * 1.25f, r * 0.80f, light);
                    c.Ellipse(cx, cy, r * 1.25f, r * 0.80f, Canvas2D.Alpha(tint, 0.25f));
                    c.Circle(cx, cy, r * 0.45f, Canvas2D.Hex("6FA9C9"));
                    c.Circle(cx, cy, r * 0.22f, ink);
                    c.Circle(cx - r * 0.16f, cy - r * 0.18f, r * 0.10f, light);
                    break;

                case "clock":
                case "waiting":
                    c.Circle(cx, cy, r, light);
                    c.Circle(cx, cy, r, Canvas2D.Alpha(tint, 0.30f));
                    c.Ring(cx, cy, r, r * 0.16f, ink);
                    c.Line(cx, cy, cx, cy - r * 0.55f, r * 0.13f, ink);
                    c.Line(cx, cy, cx + r * 0.42f, cy + r * 0.10f, r * 0.13f, ink);
                    c.Circle(cx, cy, r * 0.09f, ink);
                    break;

                case "hourglass":
                    c.Poly(light, new Vector2[]
                    {
                        new Vector2(cx - r * 0.7f, cy - r * 0.9f),
                        new Vector2(cx + r * 0.7f, cy - r * 0.9f),
                        new Vector2(cx, cy)
                    });
                    c.Poly(light, new Vector2[]
                    {
                        new Vector2(cx - r * 0.7f, cy + r * 0.9f),
                        new Vector2(cx + r * 0.7f, cy + r * 0.9f),
                        new Vector2(cx, cy)
                    });
                    c.Poly(Canvas2D.Alpha(tint, 0.85f), new Vector2[]
                    {
                        new Vector2(cx - r * 0.42f, cy - r * 0.55f),
                        new Vector2(cx + r * 0.42f, cy - r * 0.55f),
                        new Vector2(cx, cy - r * 0.06f)
                    });
                    c.Rect(cx - r * 0.8f, cy - r * 1.0f, r * 1.6f, r * 0.16f, ink);
                    c.Rect(cx - r * 0.8f, cy + r * 0.86f, r * 1.6f, r * 0.16f, ink);
                    break;

                case "turns":
                    c.Polyline(new[]
                    {
                        new Vector2(cx - r * 0.8f, cy - r * 0.35f),
                        new Vector2(cx + r * 0.55f, cy - r * 0.35f)
                    }, r * 0.20f, ink);
                    c.Polyline(new[]
                    {
                        new Vector2(cx + r * 0.8f, cy + r * 0.35f),
                        new Vector2(cx - r * 0.55f, cy + r * 0.35f)
                    }, r * 0.20f, ink);
                    c.Poly(ink, new Vector2[]
                    {
                        new Vector2(cx + r * 0.75f, cy - r * 0.68f),
                        new Vector2(cx + r * 0.75f, cy - r * 0.02f),
                        new Vector2(cx + r * 0.22f, cy - r * 0.35f)
                    });
                    c.Poly(ink, new Vector2[]
                    {
                        new Vector2(cx - r * 0.75f, cy + r * 0.02f),
                        new Vector2(cx - r * 0.75f, cy + r * 0.68f),
                        new Vector2(cx - r * 0.22f, cy + r * 0.35f)
                    });
                    break;

                case "block":
                    c.RoundRect(cx - r * 0.75f, cy - r * 0.75f, r * 1.5f, r * 1.5f, r * 0.25f, light);
                    c.RoundRect(cx - r * 0.75f, cy - r * 0.75f, r * 1.5f, r * 1.5f, r * 0.25f, Canvas2D.Alpha(tint, 0.55f));
                    c.Circle(cx - r * 0.30f, cy + r * 0.30f, r * 0.10f, light);
                    c.Circle(cx + r * 0.30f, cy + r * 0.30f, r * 0.10f, light);
                    break;

                case "alert":
                    c.Circle(cx, cy, r * 0.92f, Canvas2D.Hex("F7A8B2"));
                    c.RoundRect(cx - r * 0.11f, cy - r * 0.52f, r * 0.22f, r * 0.72f, r * 0.11f, light);
                    c.Circle(cx, cy + r * 0.42f, r * 0.14f, light);
                    break;

                case "heart":
                case "company":
                    Heart(c, cx, cy, r * 1.05f, Canvas2D.Hex("F58E9B"));
                    if (symbol == "company")
                        Heart(c, cx + r * 0.62f, cy + r * 0.30f, r * 0.62f, Canvas2D.Hex("F7A8B2"));
                    break;

                case "pause":
                    c.Circle(cx, cy, r * 0.92f, Canvas2D.Alpha(tint, 0.55f));
                    c.RoundRect(cx - r * 0.34f, cy - r * 0.40f, r * 0.20f, r * 0.80f, r * 0.08f, light);
                    c.RoundRect(cx + r * 0.14f, cy - r * 0.40f, r * 0.20f, r * 0.80f, r * 0.08f, light);
                    break;

                case "moon":
                    c.Circle(cx, cy - r * 0.05f, r * 0.78f, Canvas2D.Hex("FFF3C8"));
                    c.Circle(cx + r * 0.34f, cy - r * 0.30f, r * 0.66f, Canvas2D.Hex("FBFAFF", 0.85f));
                    break;

                case "hand":
                case "help":
                    c.RoundRect(cx - r * 0.62f, cy - r * 0.60f, r * 1.24f, r * 1.15f, r * 0.34f, light);
                    c.RoundRect(cx - r * 0.62f, cy - r * 0.60f, r * 1.24f, r * 1.15f, r * 0.34f, Canvas2D.Alpha(tint, 0.35f));
                    for (int i = 0; i < 4; i++)
                        c.RoundRect(cx - r * 0.50f + i * r * 0.30f, cy - r * 0.92f, r * 0.20f, r * 0.46f, r * 0.10f, light);
                    break;

                case "person":
                    c.Circle(cx, cy - r * 0.42f, r * 0.40f, light);
                    c.RoundRect(cx - r * 0.55f, cy + r * 0.02f, r * 1.10f, r * 0.80f, r * 0.30f, light);
                    c.Circle(cx, cy - r * 0.42f, r * 0.40f, Canvas2D.Alpha(tint, 0.30f));
                    c.RoundRect(cx - r * 0.55f, cy + r * 0.02f, r * 1.10f, r * 0.80f, r * 0.30f, Canvas2D.Alpha(tint, 0.30f));
                    break;

                case "loop":
                    c.Ring(cx, cy, r * 0.78f, r * 0.16f, ink);
                    c.Poly(ink, new Vector2[]
                    {
                        new Vector2(cx + r * 0.42f, cy - r * 0.95f),
                        new Vector2(cx + r * 1.02f, cy - r * 0.62f),
                        new Vector2(cx + r * 0.42f, cy - r * 0.30f)
                    });
                    break;

                case "question":
                    c.Circle(cx, cy, r * 0.92f, Canvas2D.Alpha(tint, 0.45f));
                    c.Polyline(new[]
                    {
                        new Vector2(cx - r * 0.34f, cy - r * 0.34f),
                        new Vector2(cx, cy - r * 0.62f),
                        new Vector2(cx + r * 0.34f, cy - r * 0.28f),
                        new Vector2(cx, cy + r * 0.02f),
                        new Vector2(cx, cy + r * 0.22f)
                    }, r * 0.16f, ink);
                    c.Circle(cx, cy + r * 0.50f, r * 0.11f, ink);
                    break;

                case "speaker":
                    c.Poly(light, new Vector2[]
                    {
                        new Vector2(cx - r * 0.75f, cy - r * 0.28f),
                        new Vector2(cx - r * 0.28f, cy - r * 0.28f),
                        new Vector2(cx + r * 0.22f, cy - r * 0.78f),
                        new Vector2(cx + r * 0.22f, cy + r * 0.78f),
                        new Vector2(cx - r * 0.28f, cy + r * 0.28f),
                        new Vector2(cx - r * 0.75f, cy + r * 0.28f)
                    });
                    c.Poly(Canvas2D.Alpha(tint, 0.45f), new Vector2[]
                    {
                        new Vector2(cx - r * 0.75f, cy - r * 0.28f),
                        new Vector2(cx - r * 0.28f, cy - r * 0.28f),
                        new Vector2(cx + r * 0.22f, cy - r * 0.78f),
                        new Vector2(cx + r * 0.22f, cy + r * 0.78f),
                        new Vector2(cx - r * 0.28f, cy + r * 0.28f),
                        new Vector2(cx - r * 0.75f, cy + r * 0.28f)
                    });
                    for (int i = 0; i < 3; i++)
                        c.Ring(cx + r * 0.30f, cy, r * (0.35f + i * 0.24f), r * 0.07f, ink);
                    break;

                case "crack":
                    Heart(c, cx - r * 0.10f, cy - r * 0.10f, r * 0.95f, Canvas2D.Hex("F58E9B"));
                    c.Polyline(new[]
                    {
                        new Vector2(cx + r * 0.10f, cy - r * 0.95f),
                        new Vector2(cx - r * 0.18f, cy - r * 0.30f),
                        new Vector2(cx + r * 0.14f, cy - r * 0.02f),
                        new Vector2(cx - r * 0.16f, cy + r * 0.55f)
                    }, r * 0.11f, Canvas2D.Hex("FFF6E9"));
                    break;

                case "changing":
                    c.Circle(cx, cy, r * 0.62f, Canvas2D.Alpha(tint, 0.55f));
                    c.Poly(ink, new Vector2[]
                    {
                        new Vector2(cx - r * 0.95f, cy - r * 0.30f),
                        new Vector2(cx - r * 0.35f, cy - r * 0.62f),
                        new Vector2(cx - r * 0.35f, cy + r * 0.02f)
                    });
                    c.Poly(ink, new Vector2[]
                    {
                        new Vector2(cx + r * 0.95f, cy + r * 0.30f),
                        new Vector2(cx + r * 0.35f, cy + r * 0.62f),
                        new Vector2(cx + r * 0.35f, cy - r * 0.02f)
                    });
                    break;

                case "space":
                    c.Ring(cx, cy, r * 0.85f, r * 0.14f, ink);
                    c.Circle(cx, cy, r * 0.30f, Canvas2D.Alpha(tint, 0.55f));
                    break;

                case "quiet":
                    c.Circle(cx - r * 0.08f, cy - r * 0.10f, r * 0.72f, Canvas2D.Hex("FFF3C8"));
                    c.Circle(cx + r * 0.30f, cy - r * 0.34f, r * 0.60f, Canvas2D.Hex("FBFAFF", 0.9f));
                    c.Line(cx - r * 0.55f, cy + r * 0.62f, cx + r * 0.55f, cy + r * 0.62f, r * 0.10f, ink);
                    break;

                default:
                    c.Circle(cx, cy, r * 0.70f, Canvas2D.Alpha(tint, 0.65f));
                    break;
            }
        }

        static void Heart(Canvas2D c, float cx, float cy, float r, Color32 col)
        {
            c.Circle(cx - r * 0.34f, cy - r * 0.22f, r * 0.42f, col);
            c.Circle(cx + r * 0.34f, cy - r * 0.22f, r * 0.42f, col);
            c.Poly(col, new Vector2[]
            {
                new Vector2(cx - r * 0.74f, cy - r * 0.02f),
                new Vector2(cx + r * 0.74f, cy - r * 0.02f),
                new Vector2(cx, cy + r * 0.86f)
            });
        }
    }
}
