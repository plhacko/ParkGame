using System;
using Mapbox.Map;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

[System.Serializable]
public class MapboxAccesToken
{
    public string AccessToken;
}

public class MapDisplayer : MonoBehaviour
{
    [Header("Request info")]
    public string accessToken;
    public MapboxRequestType RequestType = MapboxRequestType.BoundingBox;
    public enum MapboxRequestType { Center, BoundingBox };
    public string urlProperty;
    private Coroutine runningRequest;

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
        
        InitiateMapRequest();
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
}
