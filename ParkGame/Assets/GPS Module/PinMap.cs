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

        var berlin = new Coordinate()
        {
            lon = 13.404954,
            lat = 52.520008
        };

        var munchen = new Coordinate()
        {
            lon = 11.57549,
            lat = 48.13743
        };

        var cheb = new Coordinate()
        {
            lon = 12.36837,
            lat = 50.08375
        };

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

        var rect = GetComponent<SpriteRenderer>().sprite.rect;
        int width = (int)rect.width;
        int height = (int)rect.height;

        var mapPosition = convertor.ConvertFrom3857ToPixelCoordinate(
            lon,
            lat,
            tl,
            br,
            width,
            height
            );

        pin.transform.position = new Vector3((float)mapPosition.lon, (float)mapPosition.lat, 0);
        Debug.Log("Map position: " + mapPosition.lon + " " + mapPosition.lat);
    }
}
