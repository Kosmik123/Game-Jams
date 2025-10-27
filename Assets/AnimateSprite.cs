using System.Collections.Generic;
using UnityEngine;

public class AnimateSprite : MonoBehaviour
{
    [SerializeField]
    private List<Sprite> sprites;
    [SerializeField]
    private float framesPerSecond = 4f;
    [SerializeField]
    private SpriteRenderer spriteRenderer;

    private float timer;
    private int currentFrame;

    private void Update()
    {
        timer += framesPerSecond * Time.deltaTime;
        if (timer > 1)
        {
            timer -= 1;
            currentFrame++;
            if (currentFrame >= sprites.Count)
                currentFrame = 0;
            spriteRenderer.sprite = sprites[currentFrame];
        }
    }
}
