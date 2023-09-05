using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerPointer : MonoBehaviour
{    
    public MapDisplayer mapDisplayer;

    private Coordinate pointerPosition = new Coordinate()
    {
        lon = 13.404954,
        lat = 52.520008
    };

    private Coordinate tl = new Coordinate()
    {
        lon = 5.865010,
        lat = 55.05772
    };

    private Coordinate br = new Coordinate()
    {
        lon = 15.043380,
        lat = 47.269133
    };

    void Start()
    {
        tl.lon = mapDisplayer.MaxLongitude;
        tl.lat = mapDisplayer.MinLatitude;
        br.lon = mapDisplayer.MinLongitude;
        br.lat = mapDisplayer.MaxLatitude;

        Debug.Log("TL: " + tl.lon + ", " + tl.lat);
        Debug.Log("BR: " + br.lon + ", " + br.lat);
    }

    // Update is called once per frame
    private void Update()
    {
        // Check if GPS data is available
        if (!GPSLocator.instance.IsLocationServiceEnabled())
        {
            return;
        }

        pointerPosition.lon = GPSLocator.instance.Longitude;
        pointerPosition.lat = GPSLocator.instance.Lattitude;

        SetPointer();
    }

    void SetPointer()
    { 
        var convertor = new CoordinateConverter();

        var sprite = GetComponent<SpriteRenderer>().sprite;
        
        // Size of map in pixels
        var rect = sprite.rect;
        var mapXSize = (int)rect.width;
        var mapYSize = (int)rect.height;

        // Scale of map in unity units
        var ppu = sprite.pixelsPerUnit;
        var scale = transform.localScale;

        var pixelXSize = 1 / ppu * scale.x;
        var pixelYSize = 1 / ppu * scale.y;

        // Size of map in Unity units
        int width = (int)(rect.width * pixelXSize);
        int height = (int)(rect.height * pixelYSize);


        // Pin position in pixels
        var mapPosition = convertor.ConvertFrom3857ToPixelCoordinate(
            pointerPosition,
            tl,
            br,
            mapXSize,
            mapYSize
            );

        // Offset it with the map
        var offset = new Vector3(
            transform.position.x - width / 2,
            transform.position.y - height / 2,
            transform.position.z
        );
    
        var pinworldspacex = (float)mapPosition.lon * pixelXSize + offset.x;
        var pinworldspacey = (float)mapPosition.lat * pixelYSize + offset.y;
        var pinworldspacez = offset.z;

        transform.position = new Vector3(pinworldspacex, pinworldspacey, pinworldspacez);
    }
}
