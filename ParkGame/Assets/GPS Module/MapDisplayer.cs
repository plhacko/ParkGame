using System;
using Mapbox.Map;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using Unity.VisualScripting;
using System.Text.RegularExpressions;
using System.Xml.Serialization;

[System.Serializable]
public class MapboxAccesToken
{
    public string AccessToken;
}

[RequireComponent(typeof(SpriteRenderer))]
public class MapDisplayer : MonoBehaviour
{
    [Header("Request info")]
    private string accessToken;
    public MapboxRequestType RequestType = MapboxRequestType.BoundingBox;
    public enum MapboxRequestType { Center, BoundingBox };
    public string urlProperty;
    private Coroutine runningRequest;
    
    public enum MapInitiator { Query, MapData }
    public MapInitiator Initiator = MapInitiator.Query;
    public MapData mapData = null;
    [Header("Style")]
    public MapboxStyle Style = MapboxStyle.Streets;
    public enum MapboxStyle
    {
        Streets,
        Outdoors,
        Light,
        Dark,
        Satellite,
        SatelliteStreets,
        NavigationPreviewDay,
        NavigationPreviewNight,
    }
    private string[] MapboxStyleIDs = new string[] {
        "streets-v12",
        "outdoors-v12",
        "light-v11",
        "dark-v11",
        "satellite-v9",
        "satellite-streets-v12",
        "navigation-day-v1",
        "navigation-night-v1",
    };

    [Header("Image size")]
    [Range(1, 1280)]
    public int Width = 1280;
    [Range(1, 1280)]
    public int Height = 1280;

    [Header("Center")]
    [Range(0, 22)]
    public int Zoom = 9;
    [Range(0, 360)]
    public int Bearing = 0;
    [Range(0, 60)]
    public int Pitch = 0;
    [Range(-180, 180)]
    public float Longitude = -122.3486f;
    [Range(-85.0511f, 85.0511f)]
    public float Lattitude = 37.8169f;

    [Header("Bounding box")]
    public float MinLongitude = -77.043686f;
    public float MaxLongitude = -77.028923f;
    public float MinLatitude = 38.892035f;
    public float MaxLatitude = 38.904192f;

    private bool mapLoaded = false;
    public Action OnMapLoaded;

    private void Awake()
    {
        // automatically take access token from mapbox config
        var mapboxConfig = Resources.Load<TextAsset>("Mapbox/MapboxConfiguration");
        if (mapboxConfig != null)
        {
            // Parse JSON data into a dictionary
            MapboxAccesToken mapboxData = JsonUtility.FromJson<MapboxAccesToken>(mapboxConfig.text);
            if (mapboxData != null)
                accessToken = mapboxData.AccessToken;
        }
        
        if (Initiator == MapInitiator.Query)
            InitiateMapRequest();
        else
            InitiateMapFromMapData();
    }

    public Vector2 ExtractDimensionsFromUrl(string url)
    {
        // Define the regex pattern to match the dimensions
        string pattern = @"/(\d+)x(\d+)@";

        // Use regex to find a match in the URL
        Match match = Regex.Match(url, pattern);

        // If a match was found, extract the dimensions and return them as a Vector2
        if (match.Success)
        {
            float width = float.Parse(match.Groups[1].Value);
            float height = float.Parse(match.Groups[2].Value);

            return new Vector2(width, height);
        }

        // If no match was found, return a default Vector2
        return new Vector2();
    }

    public Vector4 ExtractBoundingBoxFromUrl(string url)
    {
        // Define the regex pattern to match the bounding box coordinates
        string bboxPattern = @"\[(-?\d+.\d+),(-?\d+.\d+),(-?\d+.\d+),(-?\d+.\d+)\]";

        Match match = Regex.Match(url, bboxPattern);

        // If a match was found, extract the coordinates and return them as a Vector4
        if (match.Success)
        {
            float minLon = float.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
            float minLat = float.Parse(match.Groups[2].Value, CultureInfo.InvariantCulture);
            float maxLon = float.Parse(match.Groups[3].Value, CultureInfo.InvariantCulture);
            float maxLat = float.Parse(match.Groups[4].Value, CultureInfo.InvariantCulture);

            return new Vector4(minLon, minLat, maxLon, maxLat);
        }

        // If no match was found, return a default Vector4
        return new Vector4();
    }

    private void InitiateMapFromMapData()
    {
        var dimensions = ExtractDimensionsFromUrl(mapData.MetaData.MapQuery);
        var boundingBox = ExtractBoundingBoxFromUrl(mapData.MetaData.MapQuery);

        Width = (int) dimensions.x;
        Height = (int) dimensions.y;
        MinLongitude = boundingBox.x;
        MinLatitude = boundingBox.y;
        MaxLongitude = boundingBox.z;
        MaxLatitude = boundingBox.w;

        var sr = GetComponent<SpriteRenderer>();
        sr.sprite = Sprite.Create(mapData.GPSTexture, new Rect(0, 0, mapData.GPSTexture.width, mapData.GPSTexture.height), new Vector2(0.5f, 0.5f));

        mapLoaded = true;
        OnMapLoaded?.Invoke();
    }
    
    public double CalculateBoundingBoxAreaInSquareMeters()
    {
        // Convert degrees to radians
        double minLatRad = MinLatitude * Math.PI / 180;
        double maxLatRad = MaxLatitude * Math.PI / 180;
        double minLonRad = MinLongitude * Math.PI / 180;
        double maxLonRad = MaxLongitude * Math.PI / 180;

        // Radius of the Earth in kilometers
        double earthRadiusKm = 6371.0;

        // Calculate the area of the spherical rectangle
        return (earthRadiusKm * earthRadiusKm *
                        Math.Abs(Math.Sin(minLatRad) - Math.Sin(maxLatRad)) *
                        Math.Abs(minLonRad - maxLonRad)) * 1000000;
    }

    private void OnEnable()
    {
        InitiateMapRequest();
    }

    private void OnDisable()
    {
        if (runningRequest != null)
            StopCoroutine(runningRequest);
    }

    public void InitiateMapRequest()
    {
        if (runningRequest != null)
            StopCoroutine(runningRequest);
        runningRequest = StartCoroutine(MapRequest());        
    }

    // Map request
    IEnumerator MapRequest()
    {    
        if (!CheckValidCoordinates())
            yield break;

        string url = "";
        switch (RequestType)
        {
            case MapboxRequestType.Center:
                url = "https://api.mapbox.com/styles/v1/mapbox/"
                + MapboxStyleIDs[(int)Style]
                + "/static/"
                + Longitude.ToString(CultureInfo.InvariantCulture)
                + ","
                + Lattitude.ToString(CultureInfo.InvariantCulture)
                + ","
                + Zoom
                + ","
                + Bearing
                + ","
                + Pitch
                + "/"
                + Width
                + "x"
                + Height
                + "@2x?access_token="
                + accessToken;
                break;
            case MapboxRequestType.BoundingBox:
                url = "https://api.mapbox.com/styles/v1/mapbox/"
                + MapboxStyleIDs[(int)Style]
                + "/static/["
                + MinLongitude.ToString(CultureInfo.InvariantCulture)
                + ","
                + MinLatitude.ToString(CultureInfo.InvariantCulture)
                + ","
                + MaxLongitude.ToString(CultureInfo.InvariantCulture)
                + ","
                + MaxLatitude.ToString(CultureInfo.InvariantCulture)
                + "]/"
                + Width
                + "x"
                + Height
                + "@2x?access_token="
                + accessToken;
                break;
        }

        Debug.Log("URL: " + url);
        urlProperty = url;

        UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.Log("API Request error: " + request.error);
        }

        else
        {
            // Get the texture out using the helper function
            Texture2D texture = DownloadHandlerTexture.GetContent(request);
            // Set the texture on the object
            GetComponent<SpriteRenderer>().sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            mapLoaded = true;
            OnMapLoaded?.Invoke();
        }
    }

    bool CheckValidCoordinates()
    {
        if (Width < 1 || Width > 1280)
        {
            Debug.LogError("Width must be between 1 and 1280.");
            return false;
        }

        if (Height < 1 || Height > 1280)
        {
            Debug.LogError("Height must be between 1 and 1280.");
            return false;
        }

        if (Longitude < -180 || Longitude > 180)
        {
            Debug.LogError("Longitude must be between -180 and 180.");
            return false;
        }

        if (Lattitude < -85.0511f || Lattitude > 85.0511f)
        {
            Debug.LogError("Lattitude must be between -85.0511 and 85.0511.");
            return false;
        }

        if (Zoom < 0 || Zoom > 22)
        {
            Debug.LogError("Zoom must be between 0 and 22.");
            return false;
        }

        if (Bearing < 0 || Bearing > 360)
        {
            Debug.LogError("Bearing must be between 0 and 360.");
            return false;
        }

        if (Pitch < 0 || Pitch > 60)
        {
            Debug.LogError("Pitch must be between 0 and 60.");
            return false;
        }

        if (MinLongitude >= MaxLongitude)
        {
            Debug.LogError("Min longtitude must be smaller than max longtitude.");
            return false;
        }

        if (MinLatitude >= MaxLatitude)
        {
            Debug.LogError("Min latitude must be smaller than max latitude.");
            return false;
        }

        return true;
    }

    // Resize sprite to fit screen
    void Resize()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr == null) return;

        transform.localScale = new Vector3(1, 1, 1);

        float width = sr.sprite.bounds.size.x;
        float height = sr.sprite.bounds.size.y;


        float worldScreenHeight = Camera.main.orthographicSize * 2f;
        float worldScreenWidth = worldScreenHeight / Screen.height * Screen.width;

        Vector3 xWidth = transform.localScale;
        xWidth.x = worldScreenWidth / width;
        transform.localScale = xWidth;
        //transform.localScale.x = worldScreenWidth / width;
        Vector3 yHeight = transform.localScale;
        yHeight.y = worldScreenHeight / height;
        transform.localScale = yHeight;
    }

    public bool IsMapLoaded()
    {
        return mapLoaded;
    }

    // Calculate the map scale for the bounding box given in lat/lon coordinates unity unit/m
    public double GetMapScale()
    {
        // Convert latitude and longitude to radians
        double lat1Rad = MinLatitude * Math.PI / 180.0;
        double lon1Rad = MinLongitude * Math.PI / 180.0;
        double lat2Rad = MaxLatitude * Math.PI / 180.0;
        double lon2Rad = MaxLongitude * Math.PI / 180.0;

        // Calculate the distance between the corners of the bounding box using the Haversine formula
        double dLat = lat2Rad - lat1Rad;
        double dLon = lon2Rad - lon1Rad;
        double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(lat1Rad) * Math.Cos(lat2Rad) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        double realWorldDistance = 6371 * c; // Distance in kilometers
        realWorldDistance *= 1000; // Convert to meters

        // Get the sprite renderer component
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        // Get the size of the sprite in pixels
        float width = sr.sprite.bounds.size.x;
        float height = sr.sprite.bounds.size.y;
        float diagonalLengthUnityUnits = Mathf.Sqrt(width * width + height * height);

        // Calculate the scale as the ratio of the real-world distance to the diagonal length of the bounding box in pixels
        double scale = diagonalLengthUnityUnits / realWorldDistance;

        return scale;
    }
}

public static class MapCameraExtensions
{
    public static IEnumerator PanToCoroutine(this Camera camera, Vector3 position, float duration)
    {
        var startPosition = camera.transform.position;
        var endPosition = camera.ClampCameraToMap(position);
        endPosition.z = camera.transform.position.z;

        var elapsed = 0f;

        while (elapsed < duration)
        {
            camera.transform.position = Vector3.Lerp(startPosition, endPosition, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        camera.transform.position = endPosition;
    }

    private static Vector3 ClampCameraToMap(this Camera camera, Vector3 position)
    {
        if (Map.GPSMap != null)
        {
            var mapBounds = Map.MapBounds;
            var cameraBounds = camera.CalculateOrthographicBounds(position);

            // dont allow camera to go outside of map bounds
            if (cameraBounds.min.x < mapBounds.min.x)
            {
                position.x = mapBounds.min.x + cameraBounds.extents.x;
            }
            if (cameraBounds.max.x > mapBounds.max.x)
            {
                position.x = mapBounds.max.x - cameraBounds.extents.x;
            }
            if (cameraBounds.min.y < mapBounds.min.y)
            {
                position.y = mapBounds.min.y + cameraBounds.extents.y;
            }
            if (cameraBounds.max.y > mapBounds.max.y)
            {
                position.y = mapBounds.max.y - cameraBounds.extents.y;
            }
        }

        return position;
    }

    public static void PointTo(this Camera camera, Vector3 position)
    {
        position.z = camera.transform.position.z;

        camera.transform.position = camera.ClampCameraToMap(position);
    }

    public static void PointToInBounds(this Camera camera, Vector3 position, Bounds bounds)
    {
        position.z = camera.transform.position.z;

        // Clamp position to bounds
        if (position.x < bounds.min.x)
        {
            position.x = bounds.min.x;
        }
        if (position.x > bounds.max.x)
        {
            position.x = bounds.max.x;
        }
        if (position.y < bounds.min.y)
        {
            position.y = bounds.min.y;
        }
        if (position.y > bounds.max.y)
        {
            position.y = bounds.max.y;
        }

        camera.transform.position = position;
    }

    public static void InGameZoomTo(this Camera camera, float size)
    {
        var min = 0.5f;
        var max = Map.GPSMap != null ? camera.MaxOrthographicSizeFor(Map.MapBounds) : float.MaxValue;

         camera.orthographicSize = Mathf.Clamp(size, min, max);

        // check if camera is within map bounds
        if (Map.GPSMap != null)
        {
            var mapBounds = Map.MapBounds;
            var cameraBounds = camera.CalculateOrthographicBounds();

            // dont allow camera to go outside of map bounds
            if (cameraBounds.min.x < mapBounds.min.x)
            {
                camera.transform.position = new Vector3(mapBounds.min.x + cameraBounds.extents.x, camera.transform.position.y, camera.transform.position.z);
            }
            if (cameraBounds.max.x > mapBounds.max.x)
            {
                camera.transform.position = new Vector3(mapBounds.max.x - cameraBounds.extents.x, camera.transform.position.y, camera.transform.position.z);
            }
            if (cameraBounds.min.y < mapBounds.min.y)
            {
                camera.transform.position = new Vector3(camera.transform.position.x, mapBounds.min.y + cameraBounds.extents.y, camera.transform.position.z);
            }
            if (cameraBounds.max.y > mapBounds.max.y)
            {
                camera.transform.position = new Vector3(camera.transform.position.x, mapBounds.max.y - cameraBounds.extents.y, camera.transform.position.z);
            }
        }
    }

    public static Bounds CalculateOrthographicBounds(this Camera camera, Vector3? position = null)
    {
        if (position == null)
        {
            position = camera.transform.position;
        }

        float cameraHeight = camera.orthographicSize * 2;
        Bounds bounds = new Bounds(
            position.Value,
            new Vector3(cameraHeight * camera.aspect, cameraHeight, 0));
        return bounds;
    }

    public static float MaxOrthographicSizeFor(this Camera camera, Bounds bounds, bool fill = true)
    {
        float mapAspect = bounds.size.x / bounds.size.y;
        float cameraAspect = camera.aspect;

        // Is the map in portrait or landscape mode?
        if (mapAspect < 1)
        {
            // Camera view is taller than the map
            if (cameraAspect < mapAspect)
            {
                // Restrict the camera size with the map's height
                if (fill)
                {
                    return bounds.size.y / 2;
                }
                else
                {
                    return bounds.size.x / cameraAspect / 2;
                }
            }
            else 
            {
                // Restrict the camera size with the map's width
                if (fill)
                {
                    return bounds.size.x / cameraAspect / 2;
                }
                else
                {
                    return bounds.size.y / 2;
                }
            }
        }
        else
        {
            // Camera view is wider than the map
            if (cameraAspect > mapAspect)
            {
                if (fill)
                {
                    return bounds.size.x / cameraAspect / 2;
                }
                else
                {
                    return bounds.size.y / 2;
                }
            }
            else 
            {
                if (fill)
                {
                    return bounds.size.y / 2;
                }
                else
                {
                    return bounds.size.x / cameraAspect / 2;
                }
            }
        }
    }
}
