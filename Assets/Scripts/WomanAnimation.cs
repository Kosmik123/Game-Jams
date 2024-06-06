using UnityEngine;

public class WomanAnimation : MonoBehaviour
{
    [SerializeField]
    private SpriteRenderer spriteRenderer;

    [SerializeField]
    private Sprite[] sprites;

    [SerializeField]
    private float unchangabilityInterval = 0.5f;
    private float timer;

    private void OnEnable()
    {
        Effect.OnEffectReachedTarget += Effect_OnEffectReachedTarget;
    }

    private void Effect_OnEffectReachedTarget()
    {
        if (timer > unchangabilityInterval)
        {
            timer = 0;
            int index = Random.Range(0, sprites.Length);
            spriteRenderer.sprite = sprites[index];
        }
    }

    private void Update()
    {
        if (timer <= unchangabilityInterval)
            timer += Time.deltaTime;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            spriteRenderer.sprite = sprites[0];
            enabled = false;
        }
    }

    private void OnDisable()
    {
        Effect.OnEffectReachedTarget -= Effect_OnEffectReachedTarget;
    }
}
