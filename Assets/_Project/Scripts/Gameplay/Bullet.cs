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
        private int bouncesLeft;
        private int damage = 1;
        private float life = 2.5f;
        private readonly List<Enemy> hit = new List<Enemy>();

        public static Bullet Spawn(Transform parent, Vector2 pos, Vector2 vel, bool fromPlayer, int pierce, string spriteName = null,
            int damage = 1, int bounces = 0, float life = 2.5f)
        {
            var go = new GameObject(fromPlayer ? "Bullet" : "Enemy Bullet");
            go.transform.SetParent(parent, false);
            go.transform.position = pos;
            var sprite = fromPlayer
                ? (damage > 1 ? Art.Circle("bullet_boss", 8) : Art.Square("bullet_player", 4))
                : spriteName != null ? Art.Circle(spriteName, 8) : Art.Diamond("bullet_enemy", 6);
            Art.MakeRenderer(go, sprite, 8);
            var b = go.AddComponent<Bullet>();
            b.velocity = vel;
            go.transform.rotation = Art.FaceRotation(vel);
            b.fromPlayer = fromPlayer;
            b.pierceLeft = pierce;
            b.damage = damage;
            b.bouncesLeft = bounces;
            b.life = life;
            return b;
        }

        private void OnEnable() => All.Add(this);
        private void OnDisable() => All.Remove(this);

        private void Update()
        {
            var game = Game.Instance;
            if (game.state == Game.State.Shop || game.state == Game.State.Dead) return;
            if (!fromPlayer && game.Frozen) return;

            Vector2 pos = (Vector2)transform.position + velocity * Time.deltaTime;
            life -= Time.deltaTime;

            // sciany areny: rykoszet albo koniec
            bool outX = Mathf.Abs(pos.x) > Game.ArenaHalf.x + 0.5f;
            bool outY = Mathf.Abs(pos.y) > Game.ArenaHalf.y + 0.5f;
            if (outX || outY)
            {
                if (fromPlayer && bouncesLeft > 0)
                {
                    bouncesLeft--;
                    if (outX) { velocity.x = -velocity.x; pos.x = Mathf.Clamp(pos.x, -Game.ArenaHalf.x - 0.5f, Game.ArenaHalf.x + 0.5f); }
                    if (outY) { velocity.y = -velocity.y; pos.y = Mathf.Clamp(pos.y, -Game.ArenaHalf.y - 0.5f, Game.ArenaHalf.y + 0.5f); }
                    transform.rotation = Art.FaceRotation(velocity);
                    life = Mathf.Max(life, 1.2f);
                }
                else if (Mathf.Abs(pos.x) > Game.ArenaHalf.x + 1f || Mathf.Abs(pos.y) > Game.ArenaHalf.y + 1f)
                {
                    Destroy(gameObject);
                    return;
                }
            }
            transform.position = pos;
            if (Obstacles.HitsSolid(pos, 0.12f)) { Destroy(gameObject); return; }
            if (life <= 0) { Destroy(gameObject); return; }

            if (fromPlayer)
            {
                var enemies = Enemy.All;
                for (int i = enemies.Count - 1; i >= 0; i--)
                {
                    var e = enemies[i];
                    if (hit.Contains(e)) continue;
                    if (((Vector2)e.transform.position - pos).sqrMagnitude > (e.radius + Radius) * (e.radius + Radius)) continue;
                    hit.Add(e);
                    if (e.Blocks(velocity)) { Sfx.Play(Sound.Hit); Destroy(gameObject); return; }
                    e.Damage(damage);
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
