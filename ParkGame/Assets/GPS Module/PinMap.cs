using System.Collections;
using TMPro;
using UnityEngine;

public class PinMap : MonoBehaviour
{
    public GameObject pin;

    public Coordinate pinPosition = new Coordinate()
    {
        lon = 13.404954,
        lat = 52.520008
    };

    public Coordinate tl = new Coordinate()
    {
        lon = 5.865010,
        lat = 55.05772
    };

    public Coordinate br = new Coordinate()
    {
        lon = 15.043380,
        lat = 47.269133
    };

    public TextMeshProUGUI text;


    // Update is called once per frame
    private void Update()
    {
        // Check if GPS data is available
        if (!GPSLocator.instance.IsLocationServiceEnabled())
        {
            text.text = "GPS not enabled";
            // SetPin for debugging in Unity Editor
            SetPin();
            return;
        }

        pinPosition.lon = GPSLocator.instance.Longitude;
        pinPosition.lat = GPSLocator.instance.Lattitude;

        text.text = GPSLocator.instance.Lattitude.ToString() + " " + GPSLocator.instance.Longitude.ToString();

        SetPin();
    }

    void SetPin()
    {
        if (pin == null)
        {
            Debug.LogError("Pin Prefab is null");
            return;
        }
      
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
            pinPosition,
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

        pin.transform.position = new Vector3(pinworldspacex, pinworldspacey, pinworldspacez);
    }
}
