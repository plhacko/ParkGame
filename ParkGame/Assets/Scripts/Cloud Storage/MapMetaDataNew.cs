using System;

public class MapMetaDataNew
{
    public string OwnerId;
    public string MapId;
    public string MapQuery;
    public string MapName;

    public MapMetaDataNew(Guid ownerId, Guid mapId, string mapName, string mapQuery)
    {
        OwnerId = ownerId.ToString();
        MapId = mapId.ToString();
        MapName = mapName;
        MapQuery = mapQuery;
    }
}

