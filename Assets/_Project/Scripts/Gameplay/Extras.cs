using System.Collections.Generic;
using UnityEngine;

namespace PayWithPixels
{
    /// <summary>Wybuch (zysk Wybuch i miny). Obrazenia sa zadawane po krotkiej zwloce, zeby lancuchy nie robily rekurencji.</summary>
    public class Explosion : MonoBehaviour
    {
        public static readonly List<Explosion> All = new List<Explosion>();

        private float timer = 0.08f;
        private float radius;
        private int damage;
        private SpriteRenderer sr;

        public static void Spawn(Transform parent, Vector2 pos, float radius, int damage)
        {
            var go = new GameObject("Explosion");
            go.transform.SetParent(parent, false);
            go.transform.position = pos;
            go.transform.localScale = Vector3.one * (radius * 2f / 0.5f);
            var e = go.AddComponent<Explosion>();
            e.sr = Art.MakeRenderer(go, Art.Circle("telegraph_ring", 8), 12);
            e.radius = radius;
            e.damage = damage;
        }

        private void OnEnable() => All.Add(this);
        private void OnDisable() => All.Remove(this);

        private void Update()
        {
            timer -= Time.deltaTime;
            if (timer > 0) return;

            Shard.Burst(transform.parent, transform.position, 6);
            Sfx.Play(Sound.Kill);
            Game.Instance.Shake(0.12f);
            var list = Enemy.All.ToArray();
            foreach (var e in list)
            {
                if (e == null) continue;
                if (((Vector2)e.transform.position - (Vector2)transform.position).magnitude < radius + e.radius)
                    e.Damage(damage);
            }
            Destroy(gameObject);
        }
    }

    /// <summary>Mina po dashu (zysk Miny z dasha).</summary>
    public class Mine : MonoBehaviour
    {
        public static readonly List<Mine> All = new List<Mine>();

        private float life = 9f;

        public static void Spawn(Transform parent, Vector2 pos)
        {
            var go = new GameObject("Mine");
            go.transform.SetParent(parent, false);
            go.transform.position = pos;
            Art.MakeRenderer(go, Art.Diamond("mine", 6), 2);
            go.AddComponent<Mine>();
        }

        private void OnEnable() => All.Add(this);
        private void OnDisable() => All.Remove(this);

        private void Update()
        {
            var game = Game.Instance;
            if (game.state != Game.State.Playing) return;
            life -= Time.deltaTime;
            if (life <= 0) { Destroy(gameObject); return; }

            foreach (var e in Enemy.All)
            {
                if (((Vector2)e.transform.position - (Vector2)transform.position).magnitude < e.radius + 0.4f)
                {
                    Explosion.Spawn(transform.parent, transform.position, 1.6f, 3);
                    Destroy(gameObject);
                    return;
                }
            }
        }
    }

    /// <summary>Pet-dron: krazy wokol gracza i strzela do najblizszego wroga.</summary>
    public class Drone : MonoBehaviour
    {
        private int index;
        private float angle;
        private float fireTimer;

        public static Drone Create(Transform parent, int index)
        {
            var go = new GameObject("Drone");
            go.transform.SetParent(parent, false);
            Art.MakeRenderer(go, Art.Diamond("drone", 6), 10);
            var d = go.AddComponent<Drone>();
            d.index = index;
            d.angle = index * 2.1f;
            return d;
        }

        private void Update()
        {
            var game = Game.Instance;
            if (game.player == null) return;
            if (index >= game.stats.drones) { Destroy(gameObject); return; }
            if (game.state != Game.State.Playing && game.state != Game.State.Clearing) return;

            angle += Time.deltaTime * 2.2f;
            Vector2 center = game.player.transform.position;
            transform.position = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * 1.2f;

            fireTimer -= Time.deltaTime;
            if (fireTimer > 0 || game.state != Game.State.Playing) return;

            Enemy best = null;
            float bestD = 9f * 9f;
            foreach (var e in Enemy.All)
            {
                float d = ((Vector2)e.transform.position - (Vector2)transform.position).sqrMagnitude;
                if (d < bestD) { bestD = d; best = e; }
            }
            if (best == null) return;

            fireTimer = game.stats.DroneInterval;
            Vector2 dir = ((Vector2)best.transform.position - (Vector2)transform.position).normalized;
            Bullet.Spawn(transform.parent, transform.position, dir * 11f, true, 0, null, game.stats.DroneDamage);
        }
    }
}
