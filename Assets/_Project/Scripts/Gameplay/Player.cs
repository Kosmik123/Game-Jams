using UnityEngine;

namespace PayWithPixels
{
    public class Player : MonoBehaviour
    {
        public const float Radius = 0.45f;

        public int hp;
        private float fireTimer;
        private float invulnTimer;
        private float dashTimer, dashCooldown;
        private Vector2 dashDir;
        private SpriteRenderer sr;
        private Transform aimMark;

        public static Player Create(Transform parent)
        {
            var go = new GameObject("Player");
            go.transform.SetParent(parent, false);
            var p = go.AddComponent<Player>();
            p.sr = Art.MakeRenderer(go, Art.Circle("player", 16), 10);
            var mark = new GameObject("Aim");
            mark.transform.SetParent(go.transform, false);
            Art.MakeRenderer(mark, Art.Square("aim", 3), 11);
            p.aimMark = mark.transform;
            p.hp = Game.Instance.stats.maxHp;
            return p;
        }

        public void Heal(int amount) => hp = Mathf.Min(Game.Instance.stats.maxHp, hp + amount);

        public void Hurt(int dmg)
        {
            var game = Game.Instance;
            if (invulnTimer > 0 || dashTimer > 0 || game.state == Game.State.Dead || game.state == Game.State.Won) return;
            if (game.stats.shield > 0)
            {
                game.stats.shield--;
                invulnTimer = 0.6f;
                return;
            }
            hp -= dmg;
            invulnTimer = 1f;
            if (hp <= 0) Game.Instance.OnPlayerDied();
        }

        private void Update()
        {
            var game = Game.Instance;
            invulnTimer -= Time.deltaTime;
            sr.enabled = invulnTimer <= 0 || Mathf.FloorToInt(Time.time * 12f) % 2 == 0;
            if (game.state != Game.State.Playing && game.state != Game.State.Clearing) return;

            var move = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            if (move.sqrMagnitude > 1f) move.Normalize();
            dashCooldown -= Time.deltaTime;
            if (game.stats.hasDash && dashCooldown <= 0 && Input.GetKeyDown(KeyCode.Space))
            {
                dashTimer = 0.15f;
                dashCooldown = 1.2f;
                dashDir = move.sqrMagnitude > 0.01f ? move.normalized : (game.display.ScreenToWorld(Input.mousePosition) - (Vector2)transform.position).normalized;
            }
            Vector2 step = move * game.stats.MoveSpeed;
            if (dashTimer > 0) { dashTimer -= Time.deltaTime; step = dashDir * 20f; }
            var pos = (Vector2)transform.position + step * Time.deltaTime;
            pos.x = Mathf.Clamp(pos.x, -Game.ArenaHalf.x, Game.ArenaHalf.x);
            pos.y = Mathf.Clamp(pos.y, -Game.ArenaHalf.y, Game.ArenaHalf.y);
            transform.position = pos;

            Vector2 aim = game.display.ScreenToWorld(Input.mousePosition) - pos;
            Vector2 dir = aim.sqrMagnitude > 0.0001f ? aim.normalized : Vector2.right;
            transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f); // sprity patrza w gore
            aimMark.position = pos + dir * 0.75f;
            aimMark.rotation = Quaternion.identity;

            fireTimer -= Time.deltaTime;
            if (Input.GetMouseButton(0) && fireTimer <= 0 && game.state == Game.State.Playing)
            {
                fireTimer = game.stats.FireInterval;
                Shoot(pos, dir);
            }
        }

        private void Shoot(Vector2 pos, Vector2 dir)
        {
            var stats = Game.Instance.stats;
            float[] angles = stats.shotCount >= 3 ? new[] { -12f, 0f, 12f } : new[] { 0f };
            foreach (float a in angles)
            {
                var d = (Vector2)(Quaternion.Euler(0, 0, a) * dir);
                Bullet.Spawn(transform.parent, pos + d * 0.6f, d * 12f, true, stats.pierce);
            }
        }
    }
}
