using System.Collections.Generic;
using UnityEngine;

namespace PayWithPixels
{
    public enum EnemyType { Chaser, Shooter, Tank }

    public class Enemy : MonoBehaviour
    {
        public static readonly List<Enemy> All = new List<Enemy>();

        public float radius;
        private EnemyType type;
        private int hp;
        private float speed;
        private int contactDamage = 1;
        private float shootTimer;
        private float strafeSign = 1f;

        public static Enemy Spawn(Transform parent, EnemyType type, Vector2 pos, int wave)
        {
            var go = new GameObject(type.ToString());
            go.transform.SetParent(parent, false);
            go.transform.position = pos;
            var e = go.AddComponent<Enemy>();
            e.type = type;
            float scale = 1f + 0.03f * wave;

            switch (type)
            {
                case EnemyType.Chaser:
                    Art.MakeRenderer(go, Art.Diamond("enemy_chaser", 16), 5);
                    e.radius = 0.45f; e.hp = 1 + wave / 4; e.speed = 2.6f * scale;
                    break;
                case EnemyType.Shooter:
                    Art.MakeRenderer(go, Art.Square("enemy_shooter", 16), 5);
                    e.radius = 0.45f; e.hp = 2 + wave / 4; e.speed = 2f * scale;
                    e.shootTimer = Random.Range(0.5f, 1.8f);
                    e.strafeSign = Random.value < 0.5f ? -1f : 1f;
                    break;
                default:
                    Art.MakeRenderer(go, Art.Square("enemy_tank", 24), 5);
                    e.radius = 0.7f; e.hp = 8 + wave; e.speed = 1.2f * scale; e.contactDamage = 2;
                    break;
            }
            return e;
        }

        private void OnEnable() => All.Add(this);
        private void OnDisable() => All.Remove(this);

        public void Damage(int dmg)
        {
            hp -= dmg;
            if (hp > 0) return;
            Game.Instance.SpawnCoin(transform.position, type == EnemyType.Tank ? 3 : 1);
            Destroy(gameObject);
        }

        private void Update()
        {
            var game = Game.Instance;
            if (game.state != Game.State.Playing && game.state != Game.State.Clearing) return;
            if (game.player == null) return;

            Vector2 pos = transform.position;
            Vector2 toPlayer = (Vector2)game.player.transform.position - pos;
            float dist = toPlayer.magnitude;
            Vector2 dir = dist > 0.001f ? toPlayer / dist : Vector2.zero;
            Vector2 move = Vector2.zero;

            if (type == EnemyType.Shooter)
            {
                if (dist > 6f) move = dir;
                else if (dist < 4f) move = -dir;
                else move = new Vector2(-dir.y, dir.x) * strafeSign;

                shootTimer -= Time.deltaTime;
                if (shootTimer <= 0 && game.state == Game.State.Playing)
                {
                    shootTimer = 1.8f;
                    Bullet.Spawn(transform.parent, pos + dir * 0.5f, dir * 5f, false, 0);
                }
            }
            else move = dir;

            pos += move * (speed * Time.deltaTime);
            pos.x = Mathf.Clamp(pos.x, -Game.ArenaHalf.x, Game.ArenaHalf.x);
            pos.y = Mathf.Clamp(pos.y, -Game.ArenaHalf.y, Game.ArenaHalf.y);
            transform.position = pos;

            float r = radius + Player.Radius;
            if (dist < r) game.player.Hurt(contactDamage);
        }
    }
}
