using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [SerializeField]
	private float moveSpeed = 4;
	[SerializeField]
    private float descentSpeed = 1;

    private float horizontalInput, verticalInput;

    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        horizontalInput = Input.GetAxis("Horizontal");
        verticalInput = Input.GetAxis("Vertical");
        
        rb.velocity = new Vector2(horizontalInput * moveSpeed, verticalInput * moveSpeed - descentSpeed);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            Application.Quit();
        }
    }

    private void OnDisable()
    {
        rb.velocity = Vector2.zero;
    }
}
