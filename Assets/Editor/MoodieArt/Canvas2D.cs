using System;
using System.IO;
using UnityEngine;

namespace MoodieLand.EditorTools
{
    /// <summary>
    /// A small software rasteriser for procedural sprites.
    /// Coordinates are in final-image space with (0,0) at the TOP-left and y down.
    /// Shapes are drawn at SS-times resolution then box-filtered down, which gives
    /// clean anti-aliased edges without any texture import fiddling.
    /// </summary>
    public class Canvas2D
    {
        public readonly int W, H;      // final image size
        public readonly int SS;        // supersample factor
        readonly int SW, SH;
        readonly Color32[] buf;

        public Canvas2D(int width, int height, int supersample = 2)
        {
            W = width;
            H = height;
            SS = Mathf.Max(1, supersample);
            SW = W * SS;
            SH = H * SS;
            buf = new Color32[SW * SH];
        }

        // ------------------------------------------------------------- helpers

        static Color32 C(float r, float g, float b, float a)
        {
            return new Color32(
                (byte)Mathf.Clamp(Mathf.RoundToInt(r * 255f), 0, 255),
                (byte)Mathf.Clamp(Mathf.RoundToInt(g * 255f), 0, 255),
                (byte)Mathf.Clamp(Mathf.RoundToInt(b * 255f), 0, 255),
                (byte)Mathf.Clamp(Mathf.RoundToInt(a * 255f), 0, 255));
        }

        public static Color32 Hex(string hex)
        {
            return Hex(hex, 1f);
        }

        public static Color32 Hex(string hex, float alpha)
        {
            hex = hex.TrimStart('#');
            byte r = Convert.ToByte(hex.Substring(0, 2), 16);
            byte g = Convert.ToByte(hex.Substring(2, 2), 16);
            byte b = Convert.ToByte(hex.Substring(4, 2), 16);
            byte a = hex.Length >= 8 ? Convert.ToByte(hex.Substring(6, 2), 16) : (byte)255;
            return new Color32(r, g, b, (byte)Mathf.Clamp(alpha * a, 0, 255));
        }

        public static Color32 Lerp(Color32 a, Color32 b, float t)
        {
            t = Mathf.Clamp01(t);
            return new Color32(
                (byte)Mathf.Lerp(a.r, b.r, t),
                (byte)Mathf.Lerp(a.g, b.g, t),
                (byte)Mathf.Lerp(a.b, b.b, t),
                (byte)Mathf.Lerp(a.a, b.a, t));
        }

        public static Color32 Alpha(Color32 c, float a)
        {
            c.a = (byte)Mathf.Clamp(Mathf.RoundToInt(a * 255f), 0, 255);
            return c;
        }

        void Blend(int x, int y, Color32 src)
        {
            if (src.a == 0) return;
            if (x < 0 || y < 0 || x >= SW || y >= SH) return;
            int i = y * SW + x;

            if (src.a == 255)
            {
                buf[i] = src;
                return;
            }

            var dst = buf[i];
            int sa = src.a;
            int da = dst.a;
            int outA = sa + da * (255 - sa) / 255;
            if (outA <= 0)
            {
                buf[i] = new Color32(0, 0, 0, 0);
                return;
            }
            int r = (src.r * sa + dst.r * da * (255 - sa) / 255) / outA;
            int g = (src.g * sa + dst.g * da * (255 - sa) / 255) / outA;
            int b = (src.b * sa + dst.b * da * (255 - sa) / 255) / outA;
            buf[i] = new Color32((byte)r, (byte)g, (byte)b, (byte)outA);
        }

        // ------------------------------------------------------------ painting

        public void Clear(Color32 c)
        {
            for (int i = 0; i < buf.Length; i++) buf[i] = c;
        }

        public void DrawPng(string path, int x, int y, int size)
        {
            var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            try
            {
                if (!texture.LoadImage(File.ReadAllBytes(path)))
                    throw new InvalidDataException("Could not read sprite: " + path);

                int sampleSize = size * SS;
                for (int row = 0; row < sampleSize; row++)
                {
                    for (int column = 0; column < sampleSize; column++)
                    {
                        float u = (column + 0.5f) / sampleSize;
                        float v = 1f - (row + 0.5f) / sampleSize;
                        Blend(x * SS + column, y * SS + row, texture.GetPixelBilinear(u, v));
                    }
                }
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(texture);
            }
        }

        public void VerticalGradient(Color32 top, Color32 bottom)
        {
            for (int y = 0; y < SH; y++)
            {
                var c = Lerp(top, bottom, y / (float)(SH - 1));
                int row = y * SW;
                for (int x = 0; x < SW; x++) buf[row + x] = c;
            }
        }

        public void Rect(float x, float y, float w, float h, Color32 c)
        {
            int x0 = Mathf.FloorToInt(x * SS), y0 = Mathf.FloorToInt(y * SS);
            int x1 = Mathf.CeilToInt((x + w) * SS), y1 = Mathf.CeilToInt((y + h) * SS);
            for (int py = y0; py < y1; py++)
                for (int px = x0; px < x1; px++)
                    Blend(px, py, c);
        }

        public void Ellipse(float cx, float cy, float rx, float ry, Color32 c)
        {
            if (rx <= 0.01f || ry <= 0.01f) return;
            float x0 = cx - rx, x1 = cx + rx, y0 = cy - ry, y1 = cy + ry;
            int px0 = Mathf.FloorToInt(x0 * SS), px1 = Mathf.CeilToInt(x1 * SS);
            int py0 = Mathf.FloorToInt(y0 * SS), py1 = Mathf.CeilToInt(y1 * SS);
            float invRx = 1f / (rx * rx), invRy = 1f / (ry * ry);

            for (int py = py0; py < py1; py++)
            {
                float dy = (py + 0.5f) / SS - cy;
                for (int px = px0; px < px1; px++)
                {
                    float dx = (px + 0.5f) / SS - cx;
                    if (dx * dx * invRx + dy * dy * invRy <= 1f) Blend(px, py, c);
                }
            }
        }

        public void Circle(float cx, float cy, float r, Color32 c) => Ellipse(cx, cy, r, r, c);

        /// <summary>Ellipse whose alpha fades out towards the rim (soft clouds / glows).</summary>
        public void SoftEllipse(float cx, float cy, float rx, float ry, Color32 c, float feather)
        {
            feather = Mathf.Clamp(feather, 0.02f, 1f);
            float x0 = cx - rx, x1 = cx + rx, y0 = cy - ry, y1 = cy + ry;
            int px0 = Mathf.Max(0, Mathf.FloorToInt(x0 * SS)), px1 = Mathf.Min(SW, Mathf.CeilToInt(x1 * SS));
            int py0 = Mathf.Max(0, Mathf.FloorToInt(y0 * SS)), py1 = Mathf.Min(SH, Mathf.CeilToInt(y1 * SS));
            float invRx = 1f / (rx * rx), invRy = 1f / (ry * ry);

            for (int py = py0; py < py1; py++)
            {
                float dy = (py + 0.5f) / SS - cy;
                for (int px = px0; px < px1; px++)
                {
                    float dx = (px + 0.5f) / SS - cx;
                    float d = dx * dx * invRx + dy * dy * invRy;
                    if (d > 1f) continue;
                    float k = Mathf.Clamp01((1f - Mathf.Sqrt(d)) / feather);
                    var cc = c;
                    cc.a = (byte)(c.a * k);
                    Blend(px, py, cc);
                }
            }
        }

        public void Glow(float cx, float cy, float radius, Color32 c, float power = 2f)
        {
            int px0 = Mathf.Max(0, Mathf.FloorToInt((cx - radius) * SS));
            int px1 = Mathf.Min(SW, Mathf.CeilToInt((cx + radius) * SS));
            int py0 = Mathf.Max(0, Mathf.FloorToInt((cy - radius) * SS));
            int py1 = Mathf.Min(SH, Mathf.CeilToInt((cy + radius) * SS));

            for (int py = py0; py < py1; py++)
            {
                float dy = (py + 0.5f) / SS - cy;
                for (int px = px0; px < px1; px++)
                {
                    float dx = (px + 0.5f) / SS - cx;
                    float d = Mathf.Sqrt(dx * dx + dy * dy) / radius;
                    if (d > 1f) continue;
                    float k = Mathf.Pow(1f - d, power);
                    var cc = c;
                    cc.a = (byte)(c.a * k);
                    Blend(px, py, cc);
                }
            }
        }

        public void RoundRect(float x, float y, float w, float h, float r, Color32 c)
        {
            r = Mathf.Min(r, Mathf.Min(w, h) * 0.5f);
            Rect(x + r, y, w - 2 * r, h, c);
            Rect(x, y + r, w, h - 2 * r, c);
            Ellipse(x + r, y + r, r, r, c);
            Ellipse(x + w - r, y + r, r, r, c);
            Ellipse(x + r, y + h - r, r, r, c);
            Ellipse(x + w - r, y + h - r, r, r, c);
        }

        public void Ring(float cx, float cy, float radius, float thickness, Color32 c)
        {
            float inner = Mathf.Max(0f, radius - thickness);
            int px0 = Mathf.Max(0, Mathf.FloorToInt((cx - radius) * SS));
            int px1 = Mathf.Min(SW, Mathf.CeilToInt((cx + radius) * SS));
            int py0 = Mathf.Max(0, Mathf.FloorToInt((cy - radius) * SS));
            int py1 = Mathf.Min(SH, Mathf.CeilToInt((cy + radius) * SS));

            for (int py = py0; py < py1; py++)
            {
                float dy = (py + 0.5f) / SS - cy;
                for (int px = px0; px < px1; px++)
                {
                    float dx = (px + 0.5f) / SS - cx;
                    float d = Mathf.Sqrt(dx * dx + dy * dy);
                    if (d <= radius && d >= inner) Blend(px, py, c);
                }
            }
        }

        public void Poly(Color32 c, params Vector2[] pts)
        {
            if (pts == null || pts.Length < 3) return;
            float minY = float.MaxValue, maxY = float.MinValue;
            for (int i = 0; i < pts.Length; i++)
            {
                minY = Mathf.Min(minY, pts[i].y);
                maxY = Mathf.Max(maxY, pts[i].y);
            }

            int py0 = Mathf.Max(0, Mathf.FloorToInt(minY * SS));
            int py1 = Mathf.Min(SH, Mathf.CeilToInt(maxY * SS));
            var xs = new float[pts.Length];

            for (int py = py0; py < py1; py++)
            {
                float sy = (py + 0.5f) / SS;
                int count = 0;
                for (int i = 0; i < pts.Length; i++)
                {
                    var a = pts[i];
                    var b = pts[(i + 1) % pts.Length];
                    if ((a.y <= sy && b.y > sy) || (b.y <= sy && a.y > sy))
                    {
                        float t = (sy - a.y) / (b.y - a.y);
                        xs[count++] = a.x + t * (b.x - a.x);
                    }
                }
                if (count < 2) continue;
                Array.Sort(xs, 0, count);
                for (int k = 0; k + 1 < count; k += 2)
                {
                    int x0 = Mathf.CeilToInt(xs[k] * SS - 0.5f);
                    int x1 = Mathf.FloorToInt(xs[k + 1] * SS - 0.5f);
                    for (int px = x0; px <= x1; px++) Blend(px, py, c);
                }
            }
        }

        public void Line(float x1, float y1, float x2, float y2, float width, Color32 c)
        {
            float hw = width * 0.5f;
            float minX = Mathf.Min(x1, x2) - hw, maxX = Mathf.Max(x1, x2) + hw;
            float minY = Mathf.Min(y1, y2) - hw, maxY = Mathf.Max(y1, y2) + hw;

            int px0 = Mathf.Max(0, Mathf.FloorToInt(minX * SS));
            int px1 = Mathf.Min(SW, Mathf.CeilToInt(maxX * SS));
            int py0 = Mathf.Max(0, Mathf.FloorToInt(minY * SS));
            int py1 = Mathf.Min(SH, Mathf.CeilToInt(maxY * SS));

            float dx = x2 - x1, dy = y2 - y1;
            float len2 = dx * dx + dy * dy;
            if (len2 < 1e-4f) { Circle(x1, y1, hw, c); return; }

            for (int py = py0; py < py1; py++)
            {
                float sy = (py + 0.5f) / SS;
                for (int px = px0; px < px1; px++)
                {
                    float sx = (px + 0.5f) / SS;
                    float t = ((sx - x1) * dx + (sy - y1) * dy) / len2;
                    t = Mathf.Clamp01(t);
                    float cx = x1 + t * dx, cy = y1 + t * dy;
                    float ddx = sx - cx, ddy = sy - cy;
                    if (ddx * ddx + ddy * ddy <= hw * hw) Blend(px, py, c);
                }
            }
        }

        public void Curve(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float width, Color32 c, int segments = 22)
        {
            var prev = p0;
            for (int i = 1; i <= segments; i++)
            {
                float t = i / (float)segments;
                var p = Bezier(p0, p1, p2, p3, t);
                Line(prev.x, prev.y, p.x, p.y, width, c);
                prev = p;
            }
        }

        /// <summary>Circular arc stroked as segments (degrees, 0 = right, CCW in screen space = up).</summary>
        public void Arc(float cx, float cy, float radius, float startDeg, float endDeg, float thickness, Color32 c, int segments = 20)
        {
            float a0 = startDeg * Mathf.Deg2Rad;
            float a1 = endDeg * Mathf.Deg2Rad;
            Vector2 prev = new Vector2(cx + Mathf.Cos(a0) * radius, cy + Mathf.Sin(a0) * radius);
            for (int i = 1; i <= segments; i++)
            {
                float t = Mathf.Lerp(a0, a1, i / (float)segments);
                var p = new Vector2(cx + Mathf.Cos(t) * radius, cy + Mathf.Sin(t) * radius);
                Line(prev.x, prev.y, p.x, p.y, thickness, c);
                prev = p;
            }
        }

        public void Polyline(Vector2[] pts, float width, Color32 c, bool closed = false)
        {
            for (int i = 0; i + 1 < pts.Length; i++)
                Line(pts[i].x, pts[i].y, pts[i + 1].x, pts[i + 1].y, width, c);
            if (closed && pts.Length > 2)
                Line(pts[pts.Length - 1].x, pts[pts.Length - 1].y, pts[0].x, pts[0].y, width, c);
        }

        public static Vector2 Bezier(Vector2 a, Vector2 b, Vector2 c, Vector2 d, float t)
        {
            float u = 1f - t;
            return u * u * u * a + 3f * u * u * t * b + 3f * u * t * t * c + t * t * t * d;
        }

        /// <summary>Simple five-pointed star centred on (cx,cy).</summary>
        public void Star(float cx, float cy, float outer, float inner, Color32 c, int points = 5, float rotation = -90f)
        {
            var pts = new Vector2[points * 2];
            for (int i = 0; i < pts.Length; i++)
            {
                float ang = (rotation + i * (360f / pts.Length)) * Mathf.Deg2Rad;
                float r = (i % 2 == 0) ? outer : inner;
                pts[i] = new Vector2(cx + Mathf.Cos(ang) * r, cy + Mathf.Sin(ang) * r);
            }
            Poly(c, pts);
        }

        // --------------------------------------------------------------- output

        public Color32[] Resolve()
        {
            var outPx = new Color32[W * H];
            for (int y = 0; y < H; y++)
            {
                for (int x = 0; x < W; x++)
                {
                    int r = 0, g = 0, b = 0, a = 0;
                    for (int sy = 0; sy < SS; sy++)
                    {
                        int row = (y * SS + sy) * SW;
                        for (int sx = 0; sx < SS; sx++)
                        {
                            var p = buf[row + x * SS + sx];
                            int pa = p.a;
                            r += p.r * pa;
                            g += p.g * pa;
                            b += p.b * pa;
                            a += pa;
                        }
                    }
                    int n = SS * SS;
                    if (a == 0)
                    {
                        outPx[y * W + x] = new Color32(0, 0, 0, 0);
                    }
                    else
                    {
                        outPx[y * W + x] = new Color32(
                            (byte)Mathf.Clamp(r / a, 0, 255),
                            (byte)Mathf.Clamp(g / a, 0, 255),
                            (byte)Mathf.Clamp(b / a, 0, 255),
                            (byte)Mathf.Clamp(a / n, 0, 255));
                    }
                }
            }
            return outPx;
        }

        /// <summary>Writes a PNG; flips vertically because Unity textures start bottom-left.</summary>
        public void Save(string absolutePath)
        {
            var px = Resolve();
            var tex = new Texture2D(W, H, TextureFormat.RGBA32, false);
            var flipped = new Color32[px.Length];
            for (int y = 0; y < H; y++)
                Array.Copy(px, y * W, flipped, (H - 1 - y) * W, W);
            tex.SetPixels32(flipped);
            tex.Apply();

            Directory.CreateDirectory(Path.GetDirectoryName(absolutePath) ?? ".");
            File.WriteAllBytes(absolutePath, tex.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(tex);
        }
    }
}
