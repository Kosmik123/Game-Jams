using System.Collections.Generic;
using UnityEngine;

namespace PayWithPixels
{
    /// <summary>Przeszkody areny per strefa: sciany (blokuja ruch i pociski) i kolce (rania gracza).</summary>
    public static class Obstacles
    {
        private static readonly List<Rect> solids = new List<Rect>();
        private static readonly List<Rect> spikes = new List<Rect>();
        private static Transform root;

        // (srodek x, srodek y, szerokosc, wysokosc) w jednostkach swiata = kaflach 16 px
        private static readonly float[][][] Layouts =
        {
            new float[][] { },   // strefa 1: czysta arena
            new[]                // strefa 2: cztery bloki
            {
                new[] { -6f, 3f, 3f, 1f }, new[] { 6f, 3f, 3f, 1f }, new[] { 0f, 0f, 1f, 1f },
                new[] { -6f, -3f, 3f, 1f }, new[] { 6f, -3f, 3f, 1f },
            },
            new[]                // strefa 3: dwa bloki (nie ma krzyża)
            {
                //new[] { 0f, 0f, 1f, 5f }, new[] { 0f, 0f, 5f, 1f },
                new[] { -9f, 4f, 2f, 1f }, new[] { 9f, -4f, 2f, 1f }, 
                new[] { -9f, -4f, 1f, 2f }, new[] { 9f, 4f, 1f, 2f },
            },
        };

        private static readonly float[][][] SpikeLayouts =
        {
            new float[][] { },
            new float[][] { },
            new[] { new[] { -6f, 0f, 3f, 1f }, new[] { 6f, 0f, 3f, 1f } },
        };

        public static void Build(Transform parent, int zone)
        {
            Clear();
            root = new GameObject("Obstacles").transform;
            root.SetParent(parent, false);
            int z = Mathf.Clamp(zone, 1, Layouts.Length) - 1;

            foreach (var l in Layouts[z]) AddBlock(l, "tile_wall", solids, 1);
            foreach (var l in SpikeLayouts[z]) AddBlock(l, "spike", spikes, 1);
        }

        public static void Clear()
        {
            solids.Clear();
            spikes.Clear();
            if (root != null) Object.Destroy(root.gameObject);
            root = null;
        }

        private static void AddBlock(float[] l, string sprite, List<Rect> list, int order)
        {
            var rect = new Rect(l[0] - l[2] / 2f, l[1] - l[3] / 2f, l[2], l[3]);
            list.Add(rect);
            var tile = Art.Square(sprite, 16);
            for (int x = 0; x < Mathf.RoundToInt(l[2]); x++)
                for (int y = 0; y < Mathf.RoundToInt(l[3]); y++)
                {
                    var go = new GameObject(sprite);
                    go.transform.SetParent(root, false);
                    go.transform.position = new Vector3(rect.xMin + x + 0.5f, rect.yMin + y + 0.5f, 0);
                    Art.MakeRenderer(go, tile, order);
                }
        }

        /// <summary>Wypycha okrag z scian.</summary>
        public static Vector2 Push(Vector2 pos, float radius)
        {
            foreach (var r in solids)
            {
                var closest = new Vector2(Mathf.Clamp(pos.x, r.xMin, r.xMax), Mathf.Clamp(pos.y, r.yMin, r.yMax));
                Vector2 d = pos - closest;
                float dist = d.magnitude;
                if (dist >= radius) continue;
                if (dist > 0.0001f) pos = closest + d / dist * radius;
                else
                {
                    // srodek wewnatrz prostokata: wypchnij w najblizsza strone
                    float left = pos.x - r.xMin, right = r.xMax - pos.x, down = pos.y - r.yMin, up = r.yMax - pos.y;
                    float m = Mathf.Min(Mathf.Min(left, right), Mathf.Min(down, up));
                    if (m == left) pos.x = r.xMin - radius;
                    else if (m == right) pos.x = r.xMax + radius;
                    else if (m == down) pos.y = r.yMin - radius;
                    else pos.y = r.yMax + radius;
                }
            }
            return pos;
        }

        public static bool HitsSolid(Vector2 pos, float radius)
        {
            foreach (var r in solids)
            {
                var closest = new Vector2(Mathf.Clamp(pos.x, r.xMin, r.xMax), Mathf.Clamp(pos.y, r.yMin, r.yMax));
                if ((pos - closest).sqrMagnitude < radius * radius) return true;
            }
            return false;
        }

        public static bool OnSpikes(Vector2 pos, float radius)
        {
            foreach (var r in spikes)
            {
                var closest = new Vector2(Mathf.Clamp(pos.x, r.xMin, r.xMax), Mathf.Clamp(pos.y, r.yMin, r.yMax));
                if ((pos - closest).sqrMagnitude < radius * radius) return true;
            }
            return false;
        }
    }
}
