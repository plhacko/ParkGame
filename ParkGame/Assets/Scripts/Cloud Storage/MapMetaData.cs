using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class MapStructures
{
    public SerializedVector3Int[] Outposts;
    public SerializedVector3Int[] VictoryPoints;
    public SerializedVector3Int[] Castles;

    public MapStructures(List<Vector3Int> outpostGridPositions, List<Vector3Int> victoryPointGridPositions, List<Vector3Int> castleGridPositions)
    {
        Outposts = listToSerializedVectorArray(outpostGridPositions);
        VictoryPoints = listToSerializedVectorArray(victoryPointGridPositions);
        Castles = listToSerializedVectorArray(castleGridPositions);
    }
    
    private SerializedVector3Int[] listToSerializedVectorArray(List<Vector3Int> positions)
    {
        return positions.Select(position => new SerializedVector3Int(position)).ToArray();
    }
}

[Serializable]
public class MapMetaData
{
    public string MapId;
    public string MapQuery;
    public string MapName;
    public double Longitude;
    public double Latitude;
    public int Width;
    public int Height;
    public MapStructures Structures;
    public Vector3Int TopLeftTileIdx;
    public Vector3Int BottomRightTileIdx;
    
    public int NumTeams => Structures.Castles.Length;

    public MapMetaData(Guid mapId, string mapName, string mapQuery, double longitude,
        double latitude, int width, int height, MapStructures structures,
        Vector3Int topLeftTileIdx, Vector3Int bottomRightTileIdx 
    )
    {
        MapId = mapId.ToString();
        MapQuery = mapQuery;
        MapName = mapName;
        Longitude = longitude;
        Latitude = latitude;
        Width = width;
        Height = height;
        Structures = structures;
        TopLeftTileIdx = topLeftTileIdx;
        BottomRightTileIdx = bottomRightTileIdx;
    }
}

