using UnityEngine;

[CreateAssetMenu]
public class VaseType : ScriptableObject
{
    [SerializeField]
    private Color color;
    public Color Color => color;

}
