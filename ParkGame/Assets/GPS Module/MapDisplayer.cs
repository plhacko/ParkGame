using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.Networking;
using static System.Net.WebRequestMethods;

public class MapDisplayer : MonoBehaviour
{
    public string accessToken;
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

    public MapboxStyle style = MapboxStyle.Streets;

    [Range(0, 22)]
    public float zoom = 15;
    
    [Range(0, 360)]
    public int bearing = 0;
    
    [Range(0, 60)]
    public int pitch = 0;

    private int width;
    private int height;

    public double longitude = 0;
    public double lattitude = 0;
    
    // Start is called before the first frame update
    void Start()
    {
        var aspect = Camera.main.aspect;
        if (aspect > 1)
        {
            width = 1280;
            height = (int)(width / aspect);
        }
        else
        {
            height = 1280;
            width = (int)(height * aspect);
        }
        InitiateMapRequest();
    }

    void InitiateMapRequest()
    {
        StartCoroutine(MapRequest());
    }

    // Map request
    IEnumerator MapRequest()
    {
        // Create a request for the URL.
        string url = "https://api.mapbox.com/styles/v1/mapbox/" 
            + MapboxStyleIDs[(int)style] + 
            "/static/" + 
            longitude.ToString(CultureInfo.InvariantCulture) + 
            "," + 
            lattitude.ToString(CultureInfo.InvariantCulture) + 
            "," +
            zoom + 
            "," + 
            bearing + 
            "," + 
            pitch + 
            "/" + 
            width + 
            "x" + 
            height 
            + 
            "?" 
            + 
            "access_token="
            + accessToken;

        Debug.Log(url);
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
            GetComponent<SpriteRenderer>().sprite = Sprite.Create(texture, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f));
            Resize();

        }
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
