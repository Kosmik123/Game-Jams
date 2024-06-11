using UnityEngine;

public class Vase : MonoBehaviour
{
    [SerializeField]
    private VaseType vaseType;
    public VaseType VaseType => vaseType;
}
