using Bipolar.Match3;
using UnityEngine;

public class WorldToCoordTest : MonoBehaviour
{
    public Board board;

    public Grid grid;

    [SerializeField]
    private Transform cursor;
    [SerializeField]
    private Transform snappedCursor;

    public bool useGrid;

    private void Reset()
    {
        grid = FindObjectOfType<Grid>();
        board = FindObjectOfType<Board>();
    }

    private void Update()
    {
        var mousePosition = Input.mousePosition;
        var mouseWorldPosition = Camera.main.ScreenToWorldPoint(mousePosition);
        cursor.transform.position = mouseWorldPosition;
        Vector3 snappedCursorPosition;
        if (useGrid)
        {
            var coord = grid.WorldToCell(mouseWorldPosition);
            snappedCursorPosition = grid.transform.TransformPoint(grid.CellToLocalInterpolated(coord));
        }
        else
        {
            var coord = board.WorldToCoord(mouseWorldPosition);
            snappedCursorPosition = board.CoordToWorld(coord);
        }
        snappedCursor.position = snappedCursorPosition;
    }
}
