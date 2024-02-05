using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraColorValidator : ColorValidator<Camera>
{
    protected override void ValidateColor()
    {
        Subject.backgroundColor = colorsByType[color];
    }
}
