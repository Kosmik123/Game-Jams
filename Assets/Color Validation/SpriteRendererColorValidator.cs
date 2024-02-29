using UnityEngine;

namespace ColorValidation
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class SpriteRendererColorValidator : ColorValidator<SpriteRenderer>
    {
        protected override void ValidateColor(Color color)
        {
            Subject.color = color;
        }
    }
}
