using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TestStructureTrigger : MonoBehaviour
{
    private Tilemap actionTilemap;

    private void Start()
    {
        actionTilemap = GetComponent<Tilemap>();
    }
    void GetSurroundingTiles(Vector3Int targetPos)
    {
        Vector3Int[] surroundingPositions = new Vector3Int[]
        {
            targetPos + new Vector3Int(1, 0, 0),  // Right
            targetPos + new Vector3Int(-1, 0, 0), // Left
            targetPos + new Vector3Int(0, 1, 0),  // Up
            targetPos + new Vector3Int(0, -1, 0)  // Down
        };

        foreach (Vector3Int pos in surroundingPositions)
        {
            TileBase tile = actionTilemap.GetTile(pos);
            if (tile != null)
            {
                // Do something with the surrounding tile.
                Debug.Log("Found surrounding tile: " + tile.name + " at position: " + pos);
            }
        }
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        var commander = other.gameObject.GetComponent<ICommander>();
        if (commander != null)
        {
            Vector3Int cellPosition;
            cellPosition = actionTilemap.WorldToCell(other.transform.position);
            TileBase tile = actionTilemap.GetTile(cellPosition);
            if (tile == null)
                GetSurroundingTiles(cellPosition);
        }
    }
}
