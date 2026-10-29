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
        private float critUntil, dashWindow;   // DASH CRIT, DOUBLE DASH
        private int rewindLeft;
        private Vector2 moveSmoothed;         // INPUT DELAY
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

        public void RefillShield()
        {
            shieldLeft = Game.Instance.stats.shieldMax;
            rewindLeft = Game.Instance.stats.rewind;
        }

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
            if (hp <= 0 && rewindLeft > 0)
            {
                rewindLeft--;
                hp = Mathf.Min(game.stats.maxHp, 2);
                invulnTimer = 2f;
                Sfx.Play(Sound.Rewind);
                game.Announce("TIME REWOUND");
                return;
            }
            game.NotifyHit();
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
            float startT = Mathf.Max(game.stats.InputDelay, game.stats.InertiaTime);   // INPUT DELAY i INERCJA: dluzszy czas startu
            float stopT = game.stats.InertiaTime * 1.3f;                                // hamuje tylko INERCJA
            if (startT > 0f || stopT > 0f)
            {
                bool speedingUp = move.magnitude >= moveSmoothed.magnitude - 0.001f;
                float t = speedingUp ? startT : stopT;
                moveSmoothed = t > 0f ? Vector2.MoveTowards(moveSmoothed, move, Time.deltaTime / t) : move;
                move = moveSmoothed;
            }
            else moveSmoothed = move;
            dashCooldown -= Time.deltaTime;
            dashWindow -= Time.deltaTime;
            if (Input.GetKeyDown(KeyCode.Space)) TryDash(game, move, game.display.ScreenToWorld(Input.mousePosition) - (Vector2)transform.position);
            Vector2 step = move * game.stats.MoveSpeed;
            if (dashTimer > 0)
            {
                dashTimer -= Time.deltaTime;
                if (dashTimer <= 0) EndDash(game);
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
            bool laser = game.stats.weapon == Weapon.Laser;
            if (!aimInit || (game.stats.aimLagStacks <= 0 && !laser)) { aimAngle = target; aimInit = true; }
            else
            {
                // laser obraca celowanie wolniej (ciezka wiazka); cena SLUGGISH AIM jeszcze bardziej
                float rate = (laser ? 9f : 18f) / (1f + 1.5f * game.stats.aimLagStacks);
                aimAngle = Mathf.LerpAngle(aimAngle, target, 1f - Mathf.Exp(-rate * Time.deltaTime));
            }
            Vector2 dir = new Vector2(Mathf.Cos(aimAngle * Mathf.Deg2Rad), Mathf.Sin(aimAngle * Mathf.Deg2Rad));
            transform.rotation = Art.FaceRotation(dir); // sprity patrza w gore
            aimMark.position = pos + dir * 0.75f;
            aimMark.rotation = Quaternion.identity;

            fireTimer -= Time.deltaTime;
            bool firing = Input.GetMouseButton(0) && game.state == Game.State.Playing;
            // laser: rozgrzewanie - slaby na starcie, mocny po ~1.5 s ciaglego ognia
            heat = firing ? Mathf.Min(1f, heat + Time.deltaTime / 1.5f) : Mathf.Max(0f, heat - Time.deltaTime * 2.5f);
            if (laser)
            {
                UpdateBeam(game, pos, dir, firing);
                return;
            }
            if (firing && fireTimer <= 0)
            {
                fireTimer = game.stats.FireInterval;
                Shoot(pos, dir);
            }
        }

        /// <summary>Dash (spacja albo bot): wymaga zysku DASH i odnowionego cooldownu (albo okna DOUBLE DASH).</summary>
        public bool TryDash(Game game, Vector2 move, Vector2 fallbackDir)
        {
            bool second = game.stats.doubleDash > 0 && dashWindow > 0;
            if (!game.stats.HasDash || dashTimer > 0 || (dashCooldown > 0 && !second)) return false;
            dashTimer = 0.15f;
            Sfx.Play(Sound.Dash);
            if (second) dashWindow = 0f;
            else { dashCooldown = game.stats.DashCooldown; dashWindow = game.stats.doubleDash > 0 ? 0.8f : 0f; }
            minesToDrop = game.stats.mines;
            mineTimer = 0;
            dashDir = move.sqrMagnitude > 0.01f ? move.normalized : fallbackDir.normalized;
            return true;
        }

        private void EndDash(Game game)
        {
            var st = game.stats;
            moveSmoothed = dashDir;   // po dashu krotkie hamowanie wg INERCJI
            if (st.dashCrit > 0) critUntil = Time.time + 2f;
            if (st.shockwave <= 0) return;
            Shockwave.Spawn(transform.parent, transform.position, st.ShockwaveRadius);
            foreach (var e in Enemy.All.ToArray())
            {
                if (e == null) continue;
                Vector2 rel = (Vector2)e.transform.position - (Vector2)transform.position;
                if (rel.magnitude > st.ShockwaveRadius + e.radius) continue;
                if (e.type != EnemyType.Boss) e.transform.position = (Vector2)e.transform.position + rel.normalized * 1.5f;
                e.Damage(st.shockwave >= 2 ? 2 : 1);
            }
        }

        /// <summary>Obrazenia strzalu: krytyk (szansa albo DASH CRIT) i bonus z DEBT WEAPON.</summary>
        private int RollDamage(RunStats stats, out bool crit)
        {
            crit = Time.time < critUntil || Random.value < stats.CritChance;
            return (crit ? Mathf.RoundToInt(stats.CritMultiplier) : 1) + stats.DebtDamageBonus(Game.Instance.debt);
        }

        private float heat;
        private Transform beam;
        private float beamTick;

        /// <summary>Laser: ciagla wiazka przebijajaca wszystkich w linii; co 0.1 s losowo rani kazdego (szansa rosnie z rozgrzaniem).</summary>
        private void UpdateBeam(Game game, Vector2 pos, Vector2 dir, bool firing)
        {
            if (beam == null)
            {
                var go = new GameObject("Beam");
                go.transform.SetParent(transform.parent, false);
                Art.MakeRenderer(go, Art.Square("bullet_player", 4), 7);
                beam = go.transform;
            }
            beam.gameObject.SetActive(firing);
            if (!firing) { beamTick = 0; return; }

            var stats = game.stats;
            // dlugosc: do sciany, krawedzi areny albo tarczy Straznika
            float len = 0f;
            const float maxLen = 18f;
            while (len < maxLen)
            {
                Vector2 p = pos + dir * (len + 0.3f);
                if (Obstacles.HitsSolid(p, 0.05f) || Mathf.Abs(p.x) > Game.ArenaHalf.x + 0.5f || Mathf.Abs(p.y) > Game.ArenaHalf.y + 0.5f) break;
                len += 0.3f;
            }

            float blockDist = len;
            foreach (var e in Enemy.All)
            {
                Vector2 rel = (Vector2)e.transform.position - pos;
                float along = Vector2.Dot(rel, dir);
                if (along < 0f || along > len) continue;
                if ((rel - dir * along).magnitude > e.radius + 0.12f) continue;
                if (e.Blocks(dir) && along < blockDist) blockDist = along;
            }
            len = blockDist;

            var sprite = beam.GetComponent<SpriteRenderer>().sprite;
            float unit = sprite != null ? sprite.bounds.size.x : 0.25f;
            beam.position = pos + dir * (len * 0.5f + 0.3f);
            beam.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg);
            beam.localScale = new Vector3(len / unit, 0.14f / unit * (1f + heat), 1f);

            beamTick -= Time.deltaTime;
            if (beamTick > 0) return;
            beamTick = 0.1f;
            Sfx.Play(Sound.LaserTick);
            float chance = Mathf.Lerp(0.25f, 0.7f, heat) * Mathf.Min(2f, stats.fireRateMul);
            foreach (var e in Enemy.All.ToArray())
            {
                if (e == null) continue;
                Vector2 rel = (Vector2)e.transform.position - pos;
                float along = Vector2.Dot(rel, dir);
                if (along < 0f || along > len + e.radius) continue;
                if ((rel - dir * along).magnitude > e.radius + 0.12f) continue;
                if (Random.value >= chance) continue;
                int dmg = RollDamage(stats, out bool beamCrit);
                e.Damage(dmg, beamCrit);
            }
        }

        private void Shoot(Vector2 pos, Vector2 dir)
        {
            var stats = Game.Instance.stats;
            Sfx.Play(stats.weapon == Weapon.Shotgun ? Sound.ShootShotgun : Sound.Shoot);
            MuzzleFlash(pos, dir);
            // wrog stykajacy sie z graczem jest przed punktem startu pocisku: dostaje obrazenia od razu
            foreach (var e in Enemy.All.ToArray())
            {
                if (e == null || e.type == EnemyType.Boss) continue;
                Vector2 rel = (Vector2)e.transform.position - pos;
                if (rel.magnitude > e.radius + Radius + 0.15f || e.Blocks(rel.normalized)) continue;
                e.Damage(RollDamage(stats, out bool touchCrit), touchCrit);
            }
            for (int i = 0; i < stats.shotCount; i++)
            {
                float a = (i - (stats.shotCount - 1) / 2f) * stats.SpreadStep;
                if (stats.weapon == Weapon.Shotgun) a += Random.Range(-2.5f, 2.5f);   // nieregularny stozek ok. 30 st.
                var d = (Vector2)(Quaternion.Euler(0, 0, a) * dir);
                int dmg = RollDamage(stats, out bool crit);
                float speed = stats.BulletSpeed * (stats.weapon == Weapon.Shotgun ? Random.Range(0.9f, 1.1f) : 1f);
                var b = Bullet.Spawn(transform.parent, pos + d * 0.6f, d * speed, true, stats.pierce, null, dmg, stats.ricochet, stats.BulletLife);
                b.closeBonus = stats.weapon == Weapon.Shotgun;
                b.crit = crit;
            }
        }

        private float muzzleUntil;
        private Transform muzzle;

        /// <summary>Blysk wylotu (fx_muzzle_0..2): maly dla pistoletu, duzy dla strzelby.</summary>
        private void MuzzleFlash(Vector2 pos, Vector2 dir)
        {
            if (muzzle == null)
            {
                var go = new GameObject("Muzzle");
                go.transform.SetParent(transform.parent, false);
                Art.MakeRenderer(go, Art.Square("fx_muzzle_0", 8), 9);
                muzzle = go.transform;
            }
            var weapon = Game.Instance.stats.weapon;
            var sr = muzzle.GetComponent<SpriteRenderer>();
            var sprite = Resources.Load<Sprite>("Sprites/fx_muzzle_" + (weapon == Weapon.Shotgun ? 2 : weapon == Weapon.Pistol ? 0 : 1));
            if (sprite != null) sr.sprite = sprite;
            muzzle.position = pos + dir * 0.7f;
            muzzle.rotation = Art.FaceRotation(dir);
            muzzle.localScale = Vector3.one * (weapon == Weapon.Shotgun ? 1.3f : 1f);
            muzzle.gameObject.SetActive(true);
            muzzleUntil = Time.time + 0.06f;
        }

        private void LateUpdate()
        {
            if (muzzle != null && muzzle.gameObject.activeSelf && Time.time > muzzleUntil) muzzle.gameObject.SetActive(false);
        }
    }
}
