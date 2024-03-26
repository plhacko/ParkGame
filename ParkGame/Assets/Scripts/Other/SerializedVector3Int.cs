using System;
using UnityEngine;

[Serializable]
public class SerializedVector3Int
{
    public int x;
    public int y;
    public int z;
    public SerializedVector3Int()
    {
        x = 0;
        y = 0;
        z = 0;
    }
    public SerializedVector3Int(Vector3Int vector)
    {
        x = vector.x;
        y = vector.y;
        z = vector.z;
    }
    public Vector3Int ToVector3Int()
    {
        return new Vector3Int(x, y, z);
    }
}
