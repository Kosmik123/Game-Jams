using UnityEngine;

namespace ColorValidation
{
    [RequireComponent(typeof(Camera))]
    public class CameraColorValidator : ColorValidator<Camera>
    {
        protected override void ValidateColor(Color color)
        {
            Subject.backgroundColor = color;
        }
    }
}
