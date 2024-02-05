using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class SpriteRendererColorValidator : ColorValidator<SpriteRenderer>
{
    protected override void ValidateColor()
    {
        Subject.color = colorsByType[color];
    }
}
