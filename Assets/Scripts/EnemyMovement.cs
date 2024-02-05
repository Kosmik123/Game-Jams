using UnityEngine;

public enum Direction
{
    Left = -1,
    None = 0,
    Right = 1,
}

public class EnemyMovement : MonoBehaviour
{
    [SerializeField]
    private Rigidbody2D _rigidbody;

    [SerializeField]
    private float moveSpeed;

    [SerializeField]
    private Direction direction;
    public Direction Direction
    {
        get => direction;
        set
        {
            direction = value;
        }
    }

    public int DirectionInt => (int)direction;

    [SerializeField]
    private Transform rotatedGraphic;
    [SerializeField]
    private float radius;

    private void Update()
    {
        var velocity = _rigidbody.velocity;
        float xSpeed = moveSpeed * DirectionInt;
        velocity.x = xSpeed;
        _rigidbody.velocity = velocity;

        rotatedGraphic.Rotate(Vector3.back, xSpeed / radius * Time.deltaTime * Mathf.Rad2Deg);
    }
}
