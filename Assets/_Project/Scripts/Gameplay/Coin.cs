using System.Collections.Generic;
using UnityEngine;

namespace PayWithPixels
{
    public class Coin : MonoBehaviour
    {
        public static readonly List<Coin> All = new List<Coin>();

        public static Coin Spawn(Transform parent, Vector2 pos)
        {
            var go = new GameObject("Coin");
            go.transform.SetParent(parent, false);
            go.transform.position = pos;
            Art.MakeRenderer(go, Art.Circle("coin", 6), 3);
            return go.AddComponent<Coin>();
        }

        private void OnEnable() => All.Add(this);
        private void OnDisable() => All.Remove(this);

        private void Update()
        {
            var game = Game.Instance;
            if (game.player == null || game.state == Game.State.Shop || game.state == Game.State.Dead) return;

            Vector2 to = (Vector2)game.player.transform.position - (Vector2)transform.position;
            float dist = to.magnitude;
            bool pull = game.state == Game.State.Clearing || dist < game.stats.magnetRange;
            if (pull) transform.position += (Vector3)(to.normalized * (10f * Time.deltaTime));
            if (dist < 0.5f)
            {
                game.AddCoins(1);
                Destroy(gameObject);
            }
        }
    }
}
