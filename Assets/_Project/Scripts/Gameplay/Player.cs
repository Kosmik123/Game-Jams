using UnityEngine;

namespace PayWithPixels
{
    public class Player : MonoBehaviour
    {
        public const float Radius = 0.45f;

        public int hp;
        private float fireTimer;
        private float invulnTimer;
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
            if (invulnTimer > 0 || Game.Instance.state == Game.State.Dead) return;
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
            var pos = (Vector2)transform.position + move * (game.stats.MoveSpeed * Time.deltaTime);
            pos.x = Mathf.Clamp(pos.x, -Game.ArenaHalf.x, Game.ArenaHalf.x);
            pos.y = Mathf.Clamp(pos.y, -Game.ArenaHalf.y, Game.ArenaHalf.y);
            transform.position = pos;

            Vector2 aim = game.display.ScreenToWorld(Input.mousePosition) - pos;
            Vector2 dir = aim.sqrMagnitude > 0.0001f ? aim.normalized : Vector2.right;
            aimMark.localPosition = dir * 0.75f;

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
