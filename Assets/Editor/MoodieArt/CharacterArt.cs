using UnityEngine;

namespace MoodieLand.EditorTools
{
    /// <summary>
    /// Draws Pip, Mimi, Bobo and Nunu from shapes. One routine handles every
    /// character; only the palette, ear silhouette, outfit and face change.
    /// </summary>
    public static class CharacterArt
    {
        public static readonly CharStyle Pip = new CharStyle
        {
            key = "pip",
            fur = Pal.Orange,
            furDark = Pal.OrangeDark,
            belly = Canvas2D.Hex("FFE9D2"),
            earInner = Canvas2D.Hex("FFCDAE"),
            accessory = Canvas2D.Hex("7FC7A8"),
            accessory2 = Canvas2D.Hex("5FA88A"),
            ears = "fox",
            outfit = "scarf",
            headScale = 0.98f
        };

        public static readonly CharStyle Mimi = new CharStyle
        {
            key = "mimi",
            fur = Pal.Squirrel,
            furDark = Pal.SquirrelDark,
            belly = Canvas2D.Hex("F6E2CC"),
            earInner = Canvas2D.Hex("E8C7A8"),
            accessory = Pal.Sun,
            accessory2 = Pal.SunDark,
            ears = "round",
            outfit = "acorn",
            headScale = 1.0f
        };

        public static readonly CharStyle Bobo = new CharStyle
        {
            key = "bobo",
            fur = Canvas2D.Hex("FBFAF6"),
            furDark = Canvas2D.Hex("E4E2DC"),
            belly = Canvas2D.Hex("FFFFFF"),
            earInner = Canvas2D.Hex("FBD4DA"),
            accessory = Canvas2D.Hex("8FB8E8"),
            accessory2 = Canvas2D.Hex("6F98C9"),
            ears = "bunny",
            outfit = "overalls",
            headScale = 1.0f
        };

        public static readonly CharStyle Nunu = new CharStyle
        {
            key = "nunu",
            fur = Canvas2D.Hex("F6B6C2"),
            furDark = Canvas2D.Hex("E095A5"),
            belly = Canvas2D.Hex("FFE0E6"),
            earInner = Canvas2D.Hex("FFC9D4"),
            accessory = Canvas2D.Hex("BFA8D8"),
            accessory2 = Canvas2D.Hex("A38FBE"),
            ears = "spike",
            outfit = "flower",
            headScale = 1.0f
        };

        public static CharStyle Get(string key)
        {
            switch (key)
            {
                case "mimi": return Mimi;
                case "bobo": return Bobo;
                case "nunu": return Nunu;
                default: return Pip;
            }
        }

        // ------------------------------------------------------------- full body

        public static void DrawFull(Canvas2D c, CharStyle s, string expr, string pose, float cx, float groundY, float scale)
        {
            float bodyCy = groundY - 122f * scale;
            float bodyRx = 86f * scale;
            float bodyRy = 98f * scale;
            float headR = 100f * scale * s.headScale;
            float headCy = bodyCy - bodyRy - headR * 0.60f;

            // ground shadow
            c.SoftEllipse(cx, groundY + 6f * scale, bodyRx * 1.15f, 22f * scale, Pal.Shadow, 0.9f);

            DrawTail(c, s, cx, bodyCy, bodyRx, bodyRy, scale);

            // feet
            var foot = Canvas2D.Lerp(s.fur, s.belly, 0.25f);
            c.Ellipse(cx - 42f * scale, groundY - 14f * scale, 36f * scale, 19f * scale, foot);
            c.Ellipse(cx + 42f * scale, groundY - 14f * scale, 36f * scale, 19f * scale, foot);

            // body
            c.Ellipse(cx, bodyCy, bodyRx, bodyRy, s.fur);
            c.Ellipse(cx, bodyCy + 6f * scale, bodyRx * 0.62f, bodyRy * 0.66f, s.belly);

            // arms (behind head, in front of body)
            DrawArms(c, s, cx, bodyCy, bodyRx, bodyRy, scale, pose);

            // outfit
            DrawOutfit(c, s, cx, bodyCy, bodyRx, bodyRy, scale);

            // head
            DrawHeadAt(c, s, cx, headCy, headR, expr, scale);
        }

        static void DrawTail(Canvas2D c, CharStyle s, float cx, float cy, float rx, float ry, float scale)
        {
            switch (s.ears)
            {
                case "fox":
                    c.Curve(
                        new Vector2(cx - rx * 0.7f, cy + ry * 0.45f),
                        new Vector2(cx - rx * 2.1f, cy + ry * 0.55f),
                        new Vector2(cx - rx * 2.2f, cy - ry * 0.75f),
                        new Vector2(cx - rx * 1.15f, cy - ry * 0.35f),
                        rx * 0.62f, s.fur);
                    c.Ellipse(cx - rx * 1.75f, cy - ry * 0.2f, rx * 0.46f, ry * 0.42f, s.fur);
                    c.Ellipse(cx - rx * 1.85f, cy - ry * 0.42f, rx * 0.32f, ry * 0.28f, s.belly);
                    break;

                case "round":
                    c.Curve(
                        new Vector2(cx - rx * 0.55f, cy + ry * 0.55f),
                        new Vector2(cx - rx * 1.95f, cy + ry * 0.95f),
                        new Vector2(cx - rx * 1.75f, cy - ry * 1.10f),
                        new Vector2(cx - rx * 0.55f, cy - ry * 0.35f),
                        rx * 0.74f, s.fur);
                    c.Circle(cx - rx * 1.35f, cy - ry * 0.62f, rx * 0.52f, s.fur);
                    c.Circle(cx - rx * 1.40f, cy - ry * 0.82f, rx * 0.34f, s.belly);
                    break;

                case "bunny":
                    c.Circle(cx + rx * 0.95f, cy + ry * 0.62f, rx * 0.34f, s.belly);
                    break;

                default:
                    c.Ellipse(cx + rx * 0.9f, cy + ry * 0.55f, rx * 0.28f, ry * 0.22f, s.furDark);
                    break;
            }
        }

        static void DrawArms(Canvas2D c, CharStyle s, float cx, float cy, float rx, float ry, float scale, string pose)
        {
            float armW = 26f * scale;
            var arm = s.fur;
            var hand = Canvas2D.Lerp(s.fur, s.belly, 0.35f);

            Vector2 shoulderL = new Vector2(cx - rx * 0.82f, cy - ry * 0.32f);
            Vector2 shoulderR = new Vector2(cx + rx * 0.82f, cy - ry * 0.32f);

            Vector2 handL = new Vector2(cx - rx * 1.08f, cy + ry * 0.28f);
            Vector2 handR = new Vector2(cx + rx * 1.08f, cy + ry * 0.28f);

            switch (pose)
            {
                case "pointing":
                    handR = new Vector2(cx + rx * 1.95f, cy - ry * 0.55f);
                    break;
                case "waving":
                    handR = new Vector2(cx + rx * 1.15f, cy - ry * 1.55f);
                    break;
                case "searching":
                    handR = new Vector2(cx + rx * 0.28f, cy - ry * 1.05f);
                    break;
                case "building":
                    handL = new Vector2(cx - rx * 0.55f, cy + ry * 0.05f);
                    handR = new Vector2(cx + rx * 0.55f, cy + ry * 0.05f);
                    break;
                case "hug":
                    handL = new Vector2(cx - rx * 0.35f, cy + ry * 0.35f);
                    handR = new Vector2(cx + rx * 0.35f, cy + ry * 0.35f);
                    break;
            }

            c.Line(shoulderL.x, shoulderL.y, handL.x, handL.y, armW + 3f * scale, s.furDark);
            c.Line(shoulderL.x, shoulderL.y, handL.x, handL.y, armW, arm);
            c.Circle(handL.x, handL.y, armW * 0.62f + 2.5f * scale, s.furDark);
            c.Circle(handL.x, handL.y, armW * 0.62f, hand);

            c.Line(shoulderR.x, shoulderR.y, handR.x, handR.y, armW + 3f * scale, s.furDark);
            c.Line(shoulderR.x, shoulderR.y, handR.x, handR.y, armW, arm);
            c.Circle(handR.x, handR.y, armW * 0.62f + 2.5f * scale, s.furDark);
            c.Circle(handR.x, handR.y, armW * 0.62f, hand);
        }

        static void DrawOutfit(Canvas2D c, CharStyle s, float cx, float cy, float rx, float ry, float scale)
        {
            switch (s.outfit)
            {
                case "scarf":
                {
                    float ny = cy - ry * 0.78f;
                    c.RoundRect(cx - rx * 0.72f, ny - 16f * scale, rx * 1.44f, 30f * scale, 15f * scale, s.accessory);
                    c.RoundRect(cx + rx * 0.30f, ny + 6f * scale, 26f * scale, 62f * scale, 12f * scale, s.accessory2);
                    c.RoundRect(cx - rx * 0.72f, ny - 16f * scale, rx * 1.44f, 12f * scale, 8f * scale, s.accessory2);
                    break;
                }
                case "overalls":
                {
                    var cloth = s.accessory;
                    c.RoundRect(cx - rx * 0.55f, cy - ry * 0.28f, rx * 1.10f, ry * 0.95f, 22f * scale, cloth);
                    c.Rect(cx - rx * 0.32f, cy - ry * 0.62f, 18f * scale, ry * 0.45f, cloth);
                    c.Rect(cx + rx * 0.16f, cy - ry * 0.62f, 18f * scale, ry * 0.45f, cloth);
                    c.Circle(cx - rx * 0.32f, cy + ry * 0.42f, 7f * scale, s.accessory2);
                    c.Circle(cx + rx * 0.32f, cy + ry * 0.42f, 7f * scale, s.accessory2);
                    break;
                }
                case "acorn":
                {
                    float ax = cx + rx * 1.05f, ay = cy + ry * 0.30f;
                    c.Ellipse(ax, ay, 22f * scale, 26f * scale, Pal.Brown);
                    c.Ellipse(ax, ay - 20f * scale, 24f * scale, 13f * scale, Pal.BrownDark);
                    c.Rect(ax - 2f * scale, ay - 36f * scale, 4f * scale, 12f * scale, Pal.BrownDark);
                    break;
                }
                case "flower":
                {
                    float fx = cx - rx * 0.92f, fy = cy - ry * 0.28f;
                    for (int i = 0; i < 5; i++)
                    {
                        float a = i * Mathf.PI * 2f / 5f;
                        c.Circle(fx + Mathf.Cos(a) * 13f * scale, fy + Mathf.Sin(a) * 13f * scale, 9f * scale, Pal.White);
                    }
                    c.Circle(fx, fy, 8f * scale, Pal.Sun);
                    break;
                }
            }
        }

        // ----------------------------------------------------------------- head

        public static void DrawHeadAt(Canvas2D c, CharStyle s, float cx, float cy, float r, string expr, float scale)
        {
            DrawEars(c, s, cx, cy, r);

            c.Ellipse(cx, cy, r, r * 0.97f, s.fur);

            // cheek fluff
            c.Circle(cx - r * 0.80f, cy + r * 0.28f, r * 0.26f, s.fur);
            c.Circle(cx + r * 0.80f, cy + r * 0.28f, r * 0.26f, s.fur);

            // muzzle
            c.Ellipse(cx, cy + r * 0.36f, r * 0.44f, r * 0.30f, s.belly);

            Face(c, cx, cy, r, expr, s);

            if (s.outfit == "overalls")
            {
                // little front teeth for the bunny
                c.Rect(cx - r * 0.10f, cy + r * 0.40f, r * 0.09f, r * 0.12f, Pal.White);
                c.Rect(cx + r * 0.01f, cy + r * 0.40f, r * 0.09f, r * 0.12f, Pal.White);
            }
        }

        static void DrawEars(Canvas2D c, CharStyle s, float cx, float cy, float r)
        {
            switch (s.ears)
            {
                case "fox":
                {
                    var l = new[]
                    {
                        new Vector2(cx - r * 0.92f, cy - r * 0.34f),
                        new Vector2(cx - r * 0.66f, cy - r * 1.30f),
                        new Vector2(cx - r * 0.12f, cy - r * 0.80f)
                    };
                    var rr = new[]
                    {
                        new Vector2(cx + r * 0.92f, cy - r * 0.34f),
                        new Vector2(cx + r * 0.66f, cy - r * 1.30f),
                        new Vector2(cx + r * 0.12f, cy - r * 0.80f)
                    };
                    c.Poly(s.furDark, l);
                    c.Poly(s.furDark, rr);
                    c.Poly(s.earInner, new[]
                    {
                        new Vector2(cx - r * 0.78f, cy - r * 0.46f),
                        new Vector2(cx - r * 0.64f, cy - r * 1.10f),
                        new Vector2(cx - r * 0.28f, cy - r * 0.78f)
                    });
                    c.Poly(s.earInner, new[]
                    {
                        new Vector2(cx + r * 0.78f, cy - r * 0.46f),
                        new Vector2(cx + r * 0.64f, cy - r * 1.10f),
                        new Vector2(cx + r * 0.28f, cy - r * 0.78f)
                    });
                    break;
                }
                case "round":
                {
                    c.Circle(cx - r * 0.62f, cy - r * 0.82f, r * 0.30f, s.furDark);
                    c.Circle(cx + r * 0.62f, cy - r * 0.82f, r * 0.30f, s.furDark);
                    c.Circle(cx - r * 0.62f, cy - r * 0.80f, r * 0.17f, s.earInner);
                    c.Circle(cx + r * 0.62f, cy - r * 0.80f, r * 0.17f, s.earInner);
                    // squirrel ear tufts
                    c.Poly(s.fur, new Vector2[]
                    {
                        new Vector2(cx - r * 0.80f, cy - r * 0.94f),
                        new Vector2(cx - r * 0.58f, cy - r * 1.26f),
                        new Vector2(cx - r * 0.44f, cy - r * 0.96f)
                    });
                    c.Poly(s.fur, new Vector2[]
                    {
                        new Vector2(cx + r * 0.80f, cy - r * 0.94f),
                        new Vector2(cx + r * 0.58f, cy - r * 1.26f),
                        new Vector2(cx + r * 0.44f, cy - r * 0.96f)
                    });
                    break;
                }
                case "bunny":
                {
                    c.Ellipse(cx - r * 0.34f, cy - r * 1.26f, r * 0.21f, r * 0.62f, s.fur);
                    c.Ellipse(cx + r * 0.34f, cy - r * 1.26f, r * 0.21f, r * 0.62f, s.fur);
                    c.Ellipse(cx - r * 0.34f, cy - r * 1.24f, r * 0.11f, r * 0.44f, s.earInner);
                    c.Ellipse(cx + r * 0.34f, cy - r * 1.24f, r * 0.11f, r * 0.44f, s.earInner);
                    break;
                }
                default:
                {
                    // hedgehog spikes around the top half
                    for (int i = 0; i < 9; i++)
                    {
                        float a = Mathf.PI * (1.05f + i * 0.115f);
                        float bx = cx + Mathf.Cos(a) * r * 0.92f;
                        float by = cy + Mathf.Sin(a) * r * 0.92f;
                        float tx = cx + Mathf.Cos(a) * r * 1.32f;
                        float ty = cy + Mathf.Sin(a) * r * 1.32f;
                        float perpX = -Mathf.Sin(a) * r * 0.20f;
                        float perpY = Mathf.Cos(a) * r * 0.20f;
                        c.Poly(s.furDark, new[]
                        {
                            new Vector2(bx - perpX, by - perpY),
                            new Vector2(tx, ty),
                            new Vector2(bx + perpX, by + perpY)
                        });
                    }
                    c.Circle(cx - r * 0.70f, cy - r * 0.52f, r * 0.20f, s.fur);
                    c.Circle(cx + r * 0.70f, cy - r * 0.52f, r * 0.20f, s.fur);
                    break;
                }
            }
        }

        // ----------------------------------------------------------------- face

        static void Face(Canvas2D c, float cx, float cy, float r, string expr, CharStyle s)
        {
            var ink = Pal.Ink;
            float eyeDx = r * 0.30f;
            float eyeY = cy + r * 0.02f;
            float eyeR = r * 0.105f;

            float browY = eyeY - r * 0.22f;
            float browLen = r * 0.20f;

            void OpenEye(float x, float y, float rr, float lookX = 0f, float lookY = 0f)
            {
                c.Ellipse(x, y, rr, rr * 1.12f, ink);
                c.Circle(x - rr * 0.28f + lookX, y - rr * 0.40f + lookY, rr * 0.34f, Pal.White);
            }

            void HappyEye(float x, float y, float rr)
            {
                c.Polyline(new[]
                {
                    new Vector2(x - rr * 1.05f, y + rr * 0.35f),
                    new Vector2(x, y - rr * 0.75f),
                    new Vector2(x + rr * 1.05f, y + rr * 0.35f)
                }, rr * 0.55f, ink);
            }

            void CalmEye(float x, float y, float rr)
            {
                c.Polyline(new[]
                {
                    new Vector2(x - rr * 1.05f, y - rr * 0.45f),
                    new Vector2(x, y + rr * 0.55f),
                    new Vector2(x + rr * 1.05f, y - rr * 0.45f)
                }, rr * 0.5f, ink);
            }

            void SqueezeEye(float x, float y, float rr)
            {
                c.Polyline(new[]
                {
                    new Vector2(x - rr, y - rr * 0.9f),
                    new Vector2(x + rr * 0.9f, y),
                    new Vector2(x - rr, y + rr * 0.9f)
                }, rr * 0.45f, ink);
            }

            void Brow(float x, float y, float len, float tilt)
            {
                c.Line(x - len * 0.5f, y + tilt * 0.5f, x + len * 0.5f, y - tilt * 0.5f, r * 0.045f, ink);
            }

            void Smile(float y, float w, float h)
            {
                c.Polyline(new[]
                {
                    new Vector2(cx - w, y),
                    new Vector2(cx, y + h),
                    new Vector2(cx + w, y)
                }, r * 0.055f, ink);
            }

            void Frown(float y, float w, float h)
            {
                c.Polyline(new[]
                {
                    new Vector2(cx - w, y + h),
                    new Vector2(cx, y),
                    new Vector2(cx + w, y + h)
                }, r * 0.055f, ink);
            }

            void Wavy(float y, float w, float h)
            {
                c.Polyline(new[]
                {
                    new Vector2(cx - w, y),
                    new Vector2(cx - w * 0.4f, y - h),
                    new Vector2(cx + w * 0.2f, y + h),
                    new Vector2(cx + w, y - h * 0.4f)
                }, r * 0.05f, ink);
            }

            void OpenSmile(float y, float w, float h)
            {
                c.Ellipse(cx, y + h * 0.4f, w, h, ink);
                c.Ellipse(cx, y + h * 0.95f, w * 0.55f, h * 0.45f, Canvas2D.Hex("F58E9B"));
            }

            float mouthY = cy + r * 0.44f;

            // nose
            c.Ellipse(cx, cy + r * 0.28f, r * 0.075f, r * 0.06f, Canvas2D.Hex("6B4F42"));

            switch (expr)
            {
                case "happy":
                    HappyEye(cx - eyeDx, eyeY, eyeR);
                    HappyEye(cx + eyeDx, eyeY, eyeR);
                    OpenSmile(mouthY - r * 0.02f, r * 0.15f, r * 0.13f);
                    break;

                case "relieved":
                    CalmEye(cx - eyeDx, eyeY, eyeR);
                    CalmEye(cx + eyeDx, eyeY, eyeR);
                    Smile(mouthY, r * 0.16f, r * 0.07f);
                    c.Ellipse(cx - r * 0.62f, cy + r * 0.30f, r * 0.14f, r * 0.09f, Pal.Blush);
                    c.Ellipse(cx + r * 0.62f, cy + r * 0.30f, r * 0.14f, r * 0.09f, Pal.Blush);
                    break;

                case "sad":
                    OpenEye(cx - eyeDx, eyeY + r * 0.02f, eyeR * 0.95f, 0f, r * 0.02f);
                    OpenEye(cx + eyeDx, eyeY + r * 0.02f, eyeR * 0.95f, 0f, r * 0.02f);
                    Brow(cx - eyeDx, browY, browLen, r * 0.07f);
                    Brow(cx + eyeDx, browY, browLen, -r * 0.07f);
                    Frown(mouthY - r * 0.02f, r * 0.13f, r * 0.06f);
                    // tear
                    c.Ellipse(cx - eyeDx - r * 0.10f, eyeY + r * 0.20f, r * 0.035f, r * 0.06f, Canvas2D.Hex("8FCBE8"));
                    break;

                case "worried":
                    OpenEye(cx - eyeDx, eyeY, eyeR * 0.92f, 0f, -r * 0.01f);
                    OpenEye(cx + eyeDx, eyeY, eyeR * 0.92f, 0f, -r * 0.01f);
                    Brow(cx - eyeDx, browY - r * 0.02f, browLen * 0.9f, r * 0.06f);
                    Brow(cx + eyeDx, browY - r * 0.02f, browLen * 0.9f, -r * 0.06f);
                    Wavy(mouthY, r * 0.10f, r * 0.025f);
                    break;

                case "angry":
                    OpenEye(cx - eyeDx, eyeY, eyeR * 0.85f);
                    OpenEye(cx + eyeDx, eyeY, eyeR * 0.85f);
                    Brow(cx - eyeDx, browY, browLen * 1.05f, -r * 0.09f);
                    Brow(cx + eyeDx, browY, browLen * 1.05f, r * 0.09f);
                    Frown(mouthY - r * 0.02f, r * 0.12f, r * 0.05f);
                    break;

                case "frustrated":
                    SqueezeEye(cx - eyeDx, eyeY, eyeR * 1.05f);
                    SqueezeEye(cx + eyeDx, eyeY, eyeR * 1.05f);
                    Brow(cx - eyeDx, browY, browLen, -r * 0.08f);
                    Brow(cx + eyeDx, browY, browLen, r * 0.08f);
                    c.RoundRect(cx - r * 0.11f, mouthY - r * 0.05f, r * 0.22f, r * 0.09f, r * 0.03f, ink);
                    c.Line(cx - r * 0.03f, mouthY - r * 0.04f, cx - r * 0.03f, mouthY + r * 0.04f, r * 0.02f, Pal.White);
                    c.Line(cx + r * 0.03f, mouthY - r * 0.04f, cx + r * 0.03f, mouthY + r * 0.04f, r * 0.02f, Pal.White);
                    break;

                case "calm":
                    CalmEye(cx - eyeDx, eyeY, eyeR);
                    CalmEye(cx + eyeDx, eyeY, eyeR);
                    Smile(mouthY, r * 0.13f, r * 0.05f);
                    break;

                case "thinking":
                    OpenEye(cx - eyeDx, eyeY, eyeR * 0.9f, r * 0.02f, -r * 0.04f);
                    OpenEye(cx + eyeDx, eyeY, eyeR * 0.9f, r * 0.02f, -r * 0.04f);
                    Brow(cx - eyeDx, browY, browLen, 0f);
                    Brow(cx + eyeDx, browY - r * 0.05f, browLen, r * 0.05f);
                    c.Line(cx - r * 0.07f, mouthY, cx + r * 0.07f, mouthY - r * 0.02f, r * 0.045f, ink);
                    break;

                case "listening":
                    OpenEye(cx - eyeDx, eyeY, eyeR);
                    OpenEye(cx + eyeDx, eyeY, eyeR);
                    Brow(cx - eyeDx, browY - r * 0.02f, browLen, 0f);
                    Brow(cx + eyeDx, browY - r * 0.02f, browLen, 0f);
                    Smile(mouthY, r * 0.12f, r * 0.05f);
                    break;

                case "disappointed":
                    OpenEye(cx - eyeDx, eyeY + r * 0.01f, eyeR * 0.9f, 0f, r * 0.02f);
                    OpenEye(cx + eyeDx, eyeY + r * 0.01f, eyeR * 0.9f, 0f, r * 0.02f);
                    Brow(cx - eyeDx, browY + r * 0.01f, browLen, -r * 0.04f);
                    Brow(cx + eyeDx, browY + r * 0.01f, browLen, r * 0.04f);
                    Frown(mouthY - r * 0.01f, r * 0.09f, r * 0.035f);
                    break;

                case "searching":
                    OpenEye(cx - eyeDx, eyeY, eyeR * 1.05f, -r * 0.02f, -r * 0.02f);
                    OpenEye(cx + eyeDx, eyeY, eyeR * 1.05f, -r * 0.02f, -r * 0.02f);
                    Brow(cx - eyeDx, browY - r * 0.04f, browLen, 0f);
                    Brow(cx + eyeDx, browY - r * 0.04f, browLen, 0f);
                    c.Ellipse(cx, mouthY, r * 0.05f, r * 0.055f, ink);
                    break;

                case "pointing":
                case "waving":
                case "building":
                    OpenEye(cx - eyeDx, eyeY, eyeR);
                    OpenEye(cx + eyeDx, eyeY, eyeR);
                    Smile(mouthY, r * 0.14f, r * 0.06f);
                    break;

                default: // idle
                    OpenEye(cx - eyeDx, eyeY, eyeR);
                    OpenEye(cx + eyeDx, eyeY, eyeR);
                    Smile(mouthY, r * 0.13f, r * 0.055f);
                    break;
            }

            // soft cheeks on friendly expressions
            if (expr == "idle" || expr == "happy" || expr == "listening" || expr == "calm")
            {
                c.Ellipse(cx - r * 0.66f, cy + r * 0.26f, r * 0.13f, r * 0.08f, Pal.Blush);
                c.Ellipse(cx + r * 0.66f, cy + r * 0.26f, r * 0.13f, r * 0.08f, Pal.Blush);
            }
        }

        // -------------------------------------------------------------- portrait

        public static void DrawPortrait(Canvas2D c, CharStyle s, string expr)
        {
            float cx = c.W * 0.5f;
            float cy = c.H * 0.56f;
            float r = c.H * 0.34f;
            DrawHeadAt(c, s, cx, cy, r, expr, r / 100f);
        }

        /// <summary>Draws a head at an arbitrary position, used for clue cards.</summary>
        public static void DrawPortraitOn(Canvas2D c, CharStyle s, string expr, float cx, float cy, float r)
        {
            DrawHeadAt(c, s, cx, cy, r, expr, r / 100f);
        }
    }
}
