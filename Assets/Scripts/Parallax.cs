using UnityEngine;

public class Parallax : MonoBehaviour
{
    public Transform Observer;

    [SerializeField]
    private float xPosition;
    public float XPosition
    {
        get => xPosition;
        set 
        {
            xPosition = value; 
        }
    }

    [SerializeField]
    private float depth;
    public float Depth
    {
        get => depth;
        set 
        {
            depth = value;
            parallaxMultipier = Mathf.Pow(2, depth);
        }
    }
    private float parallaxMultipier;


    private void Reset()
    {
        xPosition = transform.position.x;
    }

    private void Awake()
    {
        Depth = Depth;
    }

    private void Start()
    {
        UpdatePosition();
    }

    private void Update()
    {
        UpdatePosition();
    }

    private void UpdatePosition()
    {
        float horizontalDistance = XPosition - Observer.position.x;
    }

    private void OnValidate()
    {
        Depth = Depth;
    }
}

