using UnityEngine;

public class MovingUp : MonoBehaviour
{
    public float topLimit = 10f;

    public float speed = 2f;
    
    private void FixedUpdate()
    {
        transform.Translate(speed * Time.deltaTime * Vector3.up, Space.World);
        if (transform.position.y >= topLimit)
        {
            Destroy(gameObject);
        }
    }
}
