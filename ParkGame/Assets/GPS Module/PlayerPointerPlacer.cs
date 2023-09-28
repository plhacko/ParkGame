using UnityEngine;

public class PlayerPointerPlacer : MonoBehaviour
{
    public GameObject pin;

    private Coordinate pinPosition = new Coordinate()
    {
        lon = 14.311738400810569,
        lat = 50.04345107596236 
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

    private MapDisplayer mapDisplayer;

    private void Awake()
    {
        mapDisplayer = gameObject.GetComponent<MapDisplayer>();
        tl.lon = mapDisplayer.MaxLongitude;
        tl.lat = mapDisplayer.MinLatitude;
        br.lon = mapDisplayer.MinLongitude;
        br.lat = mapDisplayer.MaxLatitude;

    }

    // Update is called once per frame
    private void Update()
    {
        // Check if GPS data is available
        if (!GPSLocator.instance.IsLocationServiceEnabled())
        {
            // text.text = "GPS not enabled";
            // SetPin for debugging in Unity Editor
            #if DEBUG
            SetPin();
            #endif
            return;
        }

        pinPosition.lon = GPSLocator.instance.Longitude;
        pinPosition.lat = GPSLocator.instance.Lattitude;

        SetPin();
    }

    void SetPin()
    {
        if (pin == null)
        {
            Debug.LogError("Pin Prefab is null");
            return;
        }

        if (!mapDisplayer.IsMapLoaded())
        {
            Debug.Log("Map is not loaded");
            return;
        }
      
// Get the sprite renderer component
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();

        // Get the sprite texture
        Texture2D spriteTexture = spriteRenderer.sprite.texture;

        // Get the size of the sprite texture in pixels
        int textureWidth = spriteTexture.width;
        int textureHeight = spriteTexture.height;

        // Get the size of the sprite in world units
        float spriteWidth = spriteRenderer.bounds.size.x;
        float spriteHeight = spriteRenderer.bounds.size.y;

        var convertor = new CoordinateConverter();
        var pixelCoordinate = convertor.ConvertFrom3857ToPixelCoordinate(
            pinPosition,
            tl,
            br,
            textureWidth,
            textureHeight
            );

        // Calculate the position of the pixel in world units
        float pixelWidth = spriteWidth / textureWidth;
        float pixelHeight = spriteHeight / textureHeight;
        float worldX = transform.position.x - spriteWidth / 2 + (float)pixelCoordinate.lon * pixelWidth;
        float worldY = transform.position.y - spriteHeight / 2 + (float)pixelCoordinate.lat * pixelHeight;

        // Create a Vector3 with the calculated world position
        Vector3 worldPosition = new Vector3(worldX, worldY, transform.position.z);

        // Do something with the world position
        pin.transform.position = worldPosition;
    }
}
