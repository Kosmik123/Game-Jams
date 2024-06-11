using UnityEngine;

public class RandomActivation : MonoBehaviour
{
    [SerializeField]
    private float deactivationProbability = 0.5f;

    private void Start()
    {
        if (Random.value < deactivationProbability)
        {
            gameObject.SetActive(false);
        }
    }
}
