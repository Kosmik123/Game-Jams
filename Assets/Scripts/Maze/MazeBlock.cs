using UnityEngine;

public class MazeBlock : MonoBehaviour
{
    [Header("Walls")]
    [SerializeField]
    private MainWall frontWall;
    public MainWall FrontWall => frontWall;
    
    [SerializeField]
    private MainWall rightWall;
    public MainWall RightWall => rightWall;

    [SerializeField]
    private GameObject leftWall;
    public GameObject LeftWall => leftWall;
    
    [SerializeField]
    private GameObject backWall;
    public GameObject BackWall => backWall;

    [Header("Corners")]
    [SerializeField]
    private GameObject frontLeftCorner;
    [SerializeField]
    private GameObject frontRightCorner;
    [SerializeField]
    private GameObject backLeftCorner;
    [SerializeField]
    private GameObject backRightCorner;

    public void Configure(int x, int y, Passage passage)
    {
        if (passage.HasFlag(Passage.Right))
            rightWall.DisableWall();
        if (passage.HasFlag(Passage.Front))
            frontWall.DisableWall();

        if (x != 0)
        {
            leftWall.SetActive(false);
            frontLeftCorner.SetActive(false);
            backLeftCorner.SetActive(false);
        }

        if (y != 0)
        {
            backWall.SetActive(false);
            backRightCorner.SetActive(false);
            backLeftCorner.SetActive(false);
        }
    }

    public void DisableWall(int index)
    {
        var wall = index % 2 == 0 ? frontWall : rightWall;
        wall.DisableWall();
    }

    public Vector3 GetVasePosition()
    {
        const float vaseOffset = 1.2f;
        var position = transform.position;
        position.x += Random.value < 0.5f ? vaseOffset : -vaseOffset;
        position.z += Random.value < 0.5f ? vaseOffset : -vaseOffset;
        return position; 
    }
}
