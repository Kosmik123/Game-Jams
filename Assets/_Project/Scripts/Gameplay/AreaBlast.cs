using System.Collections.Generic;
using UnityEngine;

namespace PayWithPixels
{
    /// <summary>Pocisk obszarowy Oblezniaka: pierscien ostrzegawczy (miga), po czasie wybucha.</summary>
    public class AreaBlast : MonoBehaviour
    {
        public static readonly List<AreaBlast> All = new List<AreaBlast>();

        private float timer;
        private float radius;
        private int damage;
        private SpriteRenderer sr;

        public static AreaBlast Spawn(Transform parent, Vector2 pos, float delay, float radius, int damage)
        {
            var go = new GameObject("Area Blast");
            go.transform.SetParent(parent, false);
            go.transform.position = pos;
            // sprite 8 px = 0.5 j.; skalujemy do srednicy radius*2
            go.transform.localScale = Vector3.one * (radius * 2f / 0.5f);
            var b = go.AddComponent<AreaBlast>();
            b.sr = Art.MakeRenderer(go, Art.Circle("telegraph_ring", 8), 2);
            b.timer = delay;
            b.radius = radius;
            b.damage = damage;
            return b;
        }

        private void OnEnable() => All.Add(this);
        private void OnDisable() => All.Remove(this);

        private void Update()
        {
            var game = Game.Instance;
            if (game.state != Game.State.Playing && game.state != Game.State.Clearing) return;

            if (game.Frozen) return;
            timer -= Time.deltaTime;
            sr.enabled = timer > 0.35f || Mathf.FloorToInt(timer * 24f) % 2 == 0;
            if (timer > 0) return;

            Shard.Burst(transform.parent, transform.position, 8);
            Sfx.Play(Sound.Kill);
            if (game.player != null &&
                ((Vector2)game.player.transform.position - (Vector2)transform.position).magnitude < radius + Player.Radius * 0.5f)
                game.player.Hurt(damage);
            Destroy(gameObject);
        }
    }
}
