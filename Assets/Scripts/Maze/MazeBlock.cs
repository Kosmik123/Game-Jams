using UnityEngine;

public class MazeBlock : MonoBehaviour
{
    [SerializeField]
    private GameObject frontWall;
    public GameObject FrontWall => frontWall;
    
    [SerializeField]
    private GameObject rightWall;
    public GameObject RightWall => rightWall;

    public void Configure(Passage passage)
    {
        rightWall.SetActive(!passage.HasFlag(Passage.Right));
        frontWall.SetActive(!passage.HasFlag(Passage.Front));
    }

    public void DisableWall(int index)
    {
        var wall = index % 2 == 0 ? frontWall : rightWall;
        wall.SetActive(false);
    }
}