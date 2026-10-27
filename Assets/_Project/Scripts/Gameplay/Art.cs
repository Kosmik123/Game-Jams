using System;
using System.Collections.Generic;
using UnityEngine;

namespace PayWithPixels
{
    /// <summary>
    /// Sprity. Najpierw szuka Resources/Sprites/{name} (assety od grafika), a jak nie ma,
    /// generuje bialy placeholder. Wszystko biale na przezroczystym, Point, PPU 16.
    /// </summary>
    public static class Art
    {
        public const int PPU = 16;
        private static readonly Dictionary<string, Sprite> cache = new Dictionary<string, Sprite>();

        public static Sprite Circle(string name, int size) => Get(name, size, (x, y, s) =>
            Mathf.Sqrt(Sq(x + 0.5f - s / 2f) + Sq(y + 0.5f - s / 2f)) <= s / 2f);

        public static Sprite Square(string name, int size) => Get(name, size, (x, y, s) => true);

        public static Sprite Diamond(string name, int size) => Get(name, size, (x, y, s) =>
            Mathf.Abs(x + 0.5f - s / 2f) + Mathf.Abs(y + 0.5f - s / 2f) <= s / 2f);

        private static float Sq(float v) => v * v;

        private static Sprite Get(string name, int size, Func<int, int, int, bool> shape)
        {
            if (cache.TryGetValue(name, out var s) && s != null) return s;
            s = Resources.Load<Sprite>("Sprites/" + name);
            if (s == null)
            {
                var tex = new Texture2D(size, size, TextureFormat.RGBA32, false)
                {
                    filterMode = FilterMode.Point,
                    wrapMode = TextureWrapMode.Clamp,
                    name = name
                };
                var px = new Color32[size * size];
                for (int y = 0; y < size; y++)
                    for (int x = 0; x < size; x++)
                        px[y * size + x] = shape(x, y, size) ? new Color32(255, 255, 255, 255) : new Color32(0, 0, 0, 0);
                tex.SetPixels32(px);
                tex.Apply();
                s = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), PPU);
            }
            cache[name] = s;
            return s;
        }

        public static SpriteRenderer MakeRenderer(GameObject go, Sprite sprite, int order)
        {
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.color = Color.white;
            sr.sortingOrder = order;
            return sr;
        }
    }
}
