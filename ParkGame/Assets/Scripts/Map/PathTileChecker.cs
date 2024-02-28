using System;
using System.Collections;
using System.Collections.Generic;
using Managers;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PathTileChecker : MonoBehaviour
{
    private Tilemap baseTilemap;
    [SerializeField] private TileBase pathTile;

    private void Start()
    {
        baseTilemap = GetComponent<Tilemap>();
    }
    public bool IsNearbyPath(Vector3 agentPos, int surroundingRadius = 3)
    {
        Vector3Int cellPosition = baseTilemap.WorldToCell(agentPos);
        // Calculate the starting position of the block
        Vector3Int origin = cellPosition - new Vector3Int(surroundingRadius, surroundingRadius, 0); 
        // Define the block's bounds
        var diameter = surroundingRadius * 2 + 1;
        BoundsInt bounds = new BoundsInt(origin, new Vector3Int(diameter, diameter, 1)); 
        foreach (TileBase tile in baseTilemap.GetTilesBlock(bounds))
        {
            if (tile == pathTile)
            {
                //Debug.Log("OnPath");
                return true;
            }
        }

        return false;
    }
}
