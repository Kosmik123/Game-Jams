using Bipolar.PuzzleBoard.Components;
using UnityEngine;

public class WorldToCoordTest : MonoBehaviour
{
    public BoardComponent board;

    public Grid grid;

    [SerializeField]
    private Transform cursor;
    [SerializeField]
    private Transform snappedCursor;

    public bool useGrid;

    public Vector2Int coord;

    private void Reset()
    {
        grid = FindObjectOfType<Grid>();
        board = FindObjectOfType<BoardComponent>();
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
            this.coord = (Vector2Int)coord;
            snappedCursorPosition = grid.transform.TransformPoint(grid.CellToLocalInterpolated(coord));
        }
        else
        {
            coord = board.WorldToCoord(mouseWorldPosition);
            snappedCursorPosition = board.CoordToWorld(coord);
        }
        snappedCursor.position = snappedCursorPosition;
    }
}
