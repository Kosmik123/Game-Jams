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
        private float walkClock;
        public int shieldLeft;
        private float shieldRegenTimer;
        private int minesToDrop;
        private float mineTimer;
        private float aimAngle;
        private bool aimInit;
        private SpriteRenderer sr, halo;
        private Transform aimMark;

        public static Player Create(Transform parent)
        {
            var go = new GameObject("Player");
            go.transform.SetParent(parent, false);
            var p = go.AddComponent<Player>();
            // obrys w kolorze tla (czarny sprite = tlo po progu), zeby gracz odcinal sie od areny i wrogow
            var halo = new GameObject("Halo");
            halo.transform.SetParent(go.transform, false);
            var haloSr = Art.MakeRenderer(halo, Art.Circle("player_halo", 22), 9);
            haloSr.color = Color.black;
            p.sr = Art.MakeRenderer(go, Art.Circle("player", 16), 10);
            p.halo = haloSr;
            var mark = new GameObject("Aim");
            mark.transform.SetParent(go.transform, false);
            Art.MakeRenderer(mark, Art.Square("aim", 3), 11);
            p.aimMark = mark.transform;
            p.hp = Game.Instance.stats.maxHp;
            return p;
        }

        public void RefillShield() => shieldLeft = Game.Instance.stats.shieldMax;

        public void Heal(int amount) => hp = Mathf.Min(Game.Instance.stats.maxHp, hp + amount);

        public void Hurt(int dmg)
        {
            var game = Game.Instance;
            if (game.GodMode) return;
            if (invulnTimer > 0 || dashTimer > 0 || game.state == Game.State.Dead || game.state == Game.State.Won) return;
            if (shieldLeft > 0)
            {
                shieldLeft--;
                invulnTimer = 0.6f;
                return;
            }
            hp -= dmg;
            invulnTimer = 1f;
            Sfx.Play(Sound.Hurt);
            game.HitStop(0.07f);
            game.Shake(0.3f);
            if (hp <= 0) Game.Instance.OnPlayerDied();
        }

        private void Update()
        {
            var game = Game.Instance;
            if (game.Paused) return;
            invulnTimer -= Time.deltaTime;
            sr.enabled = invulnTimer <= 0 || Mathf.FloorToInt(Time.time * 12f) % 2 == 0;
            halo.enabled = sr.enabled;
            if (game.state != Game.State.Playing && game.state != Game.State.Clearing) return;

            // synergia Ciezkie nogi + Tarcza: tarcza odnawia sie w trakcie fali
            float regen = game.stats.ShieldRegen;
            if (regen > 0 && shieldLeft < game.stats.shieldMax)
            {
                shieldRegenTimer += Time.deltaTime;
                if (shieldRegenTimer >= regen) { shieldRegenTimer = 0; shieldLeft++; }
            }

            var move = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            if (game.ControlsInverted) move = -move;
            if (move.sqrMagnitude > 1f) move.Normalize();
            dashCooldown -= Time.deltaTime;
            if (game.stats.HasDash && dashCooldown <= 0 && Input.GetKeyDown(KeyCode.Space))
            {
                dashTimer = 0.15f;
                Sfx.Play(Sound.Dash);
                dashCooldown = game.stats.DashCooldown;
                minesToDrop = game.stats.mines;
                mineTimer = 0;
                dashDir = move.sqrMagnitude > 0.01f ? move.normalized : (game.display.ScreenToWorld(Input.mousePosition) - (Vector2)transform.position).normalized;
            }
            Vector2 step = move * game.stats.MoveSpeed;
            if (dashTimer > 0)
            {
                dashTimer -= Time.deltaTime;
                step = dashDir * 20f;
                mineTimer -= Time.deltaTime;
                if (minesToDrop > 0 && mineTimer <= 0)
                {
                    Mine.Spawn(transform.parent, transform.position);
                    minesToDrop--;
                    mineTimer = 0.045f;
                }
            }
            if (move.sqrMagnitude > 0.01f) walkClock += Time.deltaTime;
            sr.sprite = move.sqrMagnitude > 0.01f
                ? Art.Circle("player_walk_" + (Mathf.FloorToInt(walkClock / 0.15f) % 2), 16)
                : Art.Circle("player", 16);
            var pos = (Vector2)transform.position + step * Time.deltaTime;
            pos.x = Mathf.Clamp(pos.x, -Game.ArenaHalf.x, Game.ArenaHalf.x);
            pos.y = Mathf.Clamp(pos.y, -Game.ArenaHalf.y, Game.ArenaHalf.y);
            pos = Obstacles.Push(pos, 0.38f);
            transform.position = pos;
            if (dashTimer <= 0 && Obstacles.OnSpikes(pos, 0.3f)) Hurt(1);

            Vector2 aim = game.display.ScreenToWorld(Input.mousePosition) - pos;
            float target = aim.sqrMagnitude > 0.0001f ? Mathf.Atan2(aim.y, aim.x) * Mathf.Rad2Deg : aimAngle;
            if (!aimInit || game.stats.aimLagStacks <= 0) { aimAngle = target; aimInit = true; }
            else
            {
                float rate = 18f / (1f + 1.5f * game.stats.aimLagStacks);
                aimAngle = Mathf.LerpAngle(aimAngle, target, 1f - Mathf.Exp(-rate * Time.deltaTime));
            }
            Vector2 dir = new Vector2(Mathf.Cos(aimAngle * Mathf.Deg2Rad), Mathf.Sin(aimAngle * Mathf.Deg2Rad));
            transform.rotation = Art.FaceRotation(dir); // sprity patrza w gore
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
            Sfx.Play(Sound.Shoot);
            for (int i = 0; i < stats.shotCount; i++)
            {
                float a = (i - (stats.shotCount - 1) / 2f) * 12f;
                var d = (Vector2)(Quaternion.Euler(0, 0, a) * dir);
                int dmg = Random.value < stats.CritChance ? Mathf.RoundToInt(stats.CritMultiplier) : 1;
                Bullet.Spawn(transform.parent, pos + d * 0.6f, d * 12f, true, stats.pierce, null, dmg, stats.ricochet);
            }
        }
    }
}
