using Bipolar.UI;
using TMPro;
using UnityEngine;

public class LabelSizeChangeOnHover : MonoBehaviour
{
    [SerializeField]
    private UIButton button;

    [SerializeField]
    private float defaultFontSize;
    [SerializeField]
    private float highlightedFontSize;

    private void OnEnable()
    {
        button.OnHighlightChanged += Button_OnHighlightChanged;
    }

    private void Button_OnHighlightChanged(bool hovered)
    {
        button.Label.fontSize = hovered ? highlightedFontSize : defaultFontSize;
    }

    private void OnDisable()
    {
        button.OnHighlightChanged -= Button_OnHighlightChanged;
    }
}
