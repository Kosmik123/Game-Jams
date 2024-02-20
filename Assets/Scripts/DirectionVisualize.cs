using Bipolar.PuzzleBoard.General;
using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent (typeof(Tilemap))]
public class DirectionVisualize : MonoBehaviour
{
    private Tilemap _tilemap;
    public Tilemap Tilemap
    {
        get
        {
            if ( _tilemap == null )
                _tilemap = GetComponent<Tilemap>();
            return _tilemap;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        for (int x = 0; x < Tilemap.size.x; x++)
        {
            for (int y = 0; y < Tilemap.size.y ; y++)
            {
                for (int z = 0; z < Tilemap.size.z; z++)
                {
                    var coord = new Vector3Int(x, y, z);
                    var tile = Tilemap.GetTile<DirectionTile>(coord);
                    if (tile != null)
                    {
                        var position = Tilemap.CellToWorld(coord);
                        var direction = DirectionTile.GetTileDirection((Vector2Int)coord, tile, true);
                        var target = coord + (Vector3Int)direction;
                        var targetPosition = Tilemap.CellToWorld(target);
                        Gizmos.DrawLine(position, targetPosition);
                    }
                }
            }

        }
    }
}
