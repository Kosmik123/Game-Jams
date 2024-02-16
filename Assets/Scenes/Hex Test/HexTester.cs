using UnityEngine;

public class HexTester : MonoBehaviour
{
    [SerializeField]
    private Transform movableObject;

    [SerializeField]
    private Vector3Int coord;

    private Grid grid;

    private void Awake()
    {
        grid = GetComponent<Grid>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.W))
            coord.y++;
        if (Input.GetKeyDown(KeyCode.S))
            coord.y--;
        if (Input.GetKeyDown(KeyCode.A))
            coord.x--;
        if (Input.GetKeyDown(KeyCode.D))
            coord.x++;

        movableObject.position = grid.CellToWorld(coord);    
    }
}
