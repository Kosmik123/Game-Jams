using System;
using UnityEngine;

/// <summary>Procedurally generated 1x1 unit sprites, so the editor needs no art assets.</summary>
public static class PixelSprites
{
    private const int Size = 8;

    private static Sprite square;
    private static Sprite arrow;

    public static Sprite Square => square != null ? square : square = Create((x, y) => true);

    /// <summary>Triangle pointing Up. Rotate by <see cref="DirectionExtensions.ToAngle"/> for other directions.</summary>
    public static Sprite Arrow => arrow != null ? arrow : arrow = Create((x, y) => Mathf.Abs(x - 3.5f) <= 3.5f - y * 0.5f);

    private static Sprite Create(Func<int, int, bool> filled)
    {
        var texture = new Texture2D(Size, Size, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp,
        };
        for (int y = 0; y < Size; y++)
        {
            for (int x = 0; x < Size; x++)
            {
                texture.SetPixel(x, y, filled(x, y) ? Color.white : Color.clear);
            }
        }
        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, Size, Size), new Vector2(0.5f, 0.5f), Size);
    }
}
