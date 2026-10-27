using UnityEngine;

namespace PayWithPixels
{
    /// <summary>Odlamek po smierci wroga. Bez alfy: miga i znika (zgodnie z zasada 2 kolorow).</summary>
    public class Shard : MonoBehaviour
    {
        private Vector2 velocity;
        private float life;
        private SpriteRenderer sr;

        public static void Burst(Transform parent, Vector2 pos, int count)
        {
            for (int i = 0; i < count; i++)
            {
                var go = new GameObject("Shard");
                go.transform.SetParent(parent, false);
                go.transform.position = pos;
                var s = go.AddComponent<Shard>();
                s.sr = Art.MakeRenderer(go, Art.Square("shard_" + Random.Range(0, 5), 3), 9);
                s.velocity = Random.insideUnitCircle.normalized * Random.Range(2f, 5f);
                s.life = Random.Range(0.3f, 0.55f);
            }
        }

        private void Update()
        {
            life -= Time.deltaTime;
            if (life <= 0) { Destroy(gameObject); return; }
            transform.position += (Vector3)(velocity * Time.deltaTime);
            velocity *= 1f - 4f * Time.deltaTime;
            transform.Rotate(0, 0, 360f * Time.deltaTime);
            sr.enabled = life > 0.15f || Mathf.FloorToInt(life * 30f) % 2 == 0;
        }
    }
}
