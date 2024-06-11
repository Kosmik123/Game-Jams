using UnityEngine;

public class MainWall : MonoBehaviour
{
    [SerializeField]
    private GameObject wall;
    [SerializeField]
    private GameObject gate;

    private void Awake()
    {
        gate.SetActive(false);
    }

    public void DisableWall()
    {
        wall.SetActive(false);
        if (Random.value < 0.5f)
            gate.SetActive(true);
    }
}
