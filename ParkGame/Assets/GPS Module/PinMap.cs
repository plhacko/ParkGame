using UnityEngine;

public class PinMap : MonoBehaviour
{
    public GameObject pin;

    public float lon = 13.404954f;
    public float lat = 52.520008f;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        SetPin();
    }

    void SetPin()
    {
        if (pin == null)
        {
            Debug.LogError("Pin Prefab is null");
            return;
        }

        var tl = new Coordinate()
        {
            lon = 5.865010,
            lat = 55.05772
        };

        var br = new Coordinate()
        {
            lon = 15.043380,
            lat = 47.269133
        };

      
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
            lon,
            lat,
            tl,
            br,
            mapXSize,
            mapYSize
            );

        // Offset it with the map
        var offset = new Vector3(
            transform.position.x - width / 2 + pixelXSize / 2,
            transform.position.y - height / 2 + pixelYSize / 2,
            transform.position.z
        );
    
        var pinworldspacex = (float)mapPosition.lon * pixelXSize + offset.x;
        var pinworldspacey = (float)mapPosition.lat * pixelYSize + offset.y;
        var pinworldspacez = offset.z;

        pin.transform.position = new Vector3(pinworldspacex, pinworldspacey, pinworldspacez);
    }
}
