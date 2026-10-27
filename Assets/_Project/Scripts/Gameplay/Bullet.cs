using System.Collections.Generic;
using UnityEngine;

namespace PayWithPixels
{
    public class Bullet : MonoBehaviour
    {
        public static readonly List<Bullet> All = new List<Bullet>();

        private const float Radius = 0.15f;
        private Vector2 velocity;
        private bool fromPlayer;
        private int pierceLeft;
        private float life = 2.5f;
        private readonly List<Enemy> hit = new List<Enemy>();

        public static Bullet Spawn(Transform parent, Vector2 pos, Vector2 vel, bool fromPlayer, int pierce, string spriteName = null)
        {
            var go = new GameObject(fromPlayer ? "Bullet" : "Enemy Bullet");
            go.transform.SetParent(parent, false);
            go.transform.position = pos;
            var sprite = fromPlayer ? Art.Square("bullet_player", 4)
                : spriteName != null ? Art.Circle(spriteName, 8) : Art.Diamond("bullet_enemy", 6);
            Art.MakeRenderer(go, sprite, 8);
            var b = go.AddComponent<Bullet>();
            b.velocity = vel;
            go.transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(vel.y, vel.x) * Mathf.Rad2Deg - 90f);
            b.fromPlayer = fromPlayer;
            b.pierceLeft = pierce;
            return b;
        }

        private void OnEnable() => All.Add(this);
        private void OnDisable() => All.Remove(this);

        private void Update()
        {
            var game = Game.Instance;
            if (game.state == Game.State.Shop || game.state == Game.State.Dead) return;

            Vector2 pos = (Vector2)transform.position + velocity * Time.deltaTime;
            transform.position = pos;
            life -= Time.deltaTime;
            if (life <= 0 || Mathf.Abs(pos.x) > Game.ArenaHalf.x + 1f || Mathf.Abs(pos.y) > Game.ArenaHalf.y + 1f)
            {
                Destroy(gameObject);
                return;
            }

            if (fromPlayer)
            {
                var enemies = Enemy.All;
                for (int i = enemies.Count - 1; i >= 0; i--)
                {
                    var e = enemies[i];
                    if (hit.Contains(e)) continue;
                    if (((Vector2)e.transform.position - pos).sqrMagnitude > (e.radius + Radius) * (e.radius + Radius)) continue;
                    hit.Add(e);
                    e.Damage(1);
                    if (pierceLeft-- <= 0) { Destroy(gameObject); return; }
                }
            }
            else if (game.player != null)
            {
                float r = Player.Radius + Radius;
                if (((Vector2)game.player.transform.position - pos).sqrMagnitude < r * r)
                {
                    game.player.Hurt(1);
                    Destroy(gameObject);
                }
            }
        }
    }
}
