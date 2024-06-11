using UnityEngine;

public class RandomWallRotation : MonoBehaviour
{
    [SerializeField]
    private float[] possibleRotations;

    private void Start()
    {
        int randomIndex = Random.Range(0, possibleRotations.Length);
        var euler = transform.localEulerAngles;
        euler.y += possibleRotations[randomIndex];
        transform.localEulerAngles = euler;
    }
}
