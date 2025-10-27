using System;
using UnityEngine;

public class BackgroundFalling : MonoBehaviour
{
    [SerializeField]
    private Transform[] tiles;
    [SerializeField]
    private float fallSpeed = 5;
    [SerializeField]
    private float tileHeight = 24;

    private void Update()
    {
        foreach (var tile in tiles)
        {
            UpdateTile(tile);
        }
    }

    private void UpdateTile(Transform tile)
    {
        var position = tile.position;
        position += fallSpeed * Time.deltaTime * Vector3.up;
        if (position.y > tileHeight)
            position.y -= 2 * tileHeight;
        
        tile.position = position;
    }
}
