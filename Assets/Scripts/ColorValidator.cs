using System.Collections.Generic;
using UnityEngine;

public abstract class ColorValidator<T> : MonoBehaviour where T : Component
{
    public enum ColorType
    {
        Pink,
        Black,
        Green,
        Red,
        White,
        LightBlue,
        LightSalmon,
    }

    protected readonly Dictionary<ColorType, Color> colorsByType = new Dictionary<ColorType, Color>()
    {
        [ColorType.Pink] = new Color32(255, 192, 203, 255),
        [ColorType.Black] = Color.black,
        [ColorType.Green] = new Color32(0, 128, 0, 255),
        [ColorType.Red] = Color.red,
        [ColorType.White] = Color.white,
        [ColorType.LightBlue] = new Color32(173, 216, 230, 255),
        [ColorType.LightSalmon] = new Color32(255, 160, 122, 255),
    };

    [SerializeField]
    protected ColorType color;

    private T subject;
    public T Subject
    {
        get
        {
            if (subject == null)
                subject = GetComponent<T>();
            return subject;
        }
    }

    private void Awake()
    {
        ValidateColor();     
    }

    protected abstract void ValidateColor();

    private void OnValidate()
    {
        ValidateColor();
    }
}
