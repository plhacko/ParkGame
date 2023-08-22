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

public class MapMetaDataNew
{
    public string MapId;
    public string MapQuery;
    public string MapName;
    public double Longitude;
    public double Latitude;
    public int Width;
    public int Height;
    public MapStructures Structures;
    
    public int NumTeams => Structures.Castles.Length;

    public MapMetaDataNew(Guid mapId, string mapName, string mapQuery, double longitude,
        double latitude, int width, int height, MapStructures structures)
    {
        MapId = mapId.ToString();
        MapQuery = mapQuery;
        MapName = mapName;
        Longitude = longitude;
        Latitude = latitude;
        Width = width;
        Height = height;
        Structures = structures;
    }
}

