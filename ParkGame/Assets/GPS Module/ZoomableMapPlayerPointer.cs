using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mapbox.Utils;
using Mapbox.Unity.Map;
using Unity.VisualScripting;
using System.Threading.Tasks;

public class ZoomableMapPlayerPointer : MonoBehaviour
{
    // Start is called before the first frame update
    private bool inUse = false;
    private AbstractMap map;
    void Awake()
    {
        if (GPSLocator.instance != null)
        {
            GPSLocator.instance.OnLocationInitialized += () =>
            {
                inUse = true;
                map = FindObjectOfType<AbstractMap>();
                GetComponent<SpriteRenderer>().enabled = true;
                transform.localScale = new Vector3(8.0f, 8.0f, 8.0f);
            };
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!inUse || !GPSLocator.instance.IsGPSUsable())
        {
            return;
        }

        var lat = GPSLocator.instance.Lattitude;
        var lon = GPSLocator.instance.Longitude;
        
        var playerPos = new Vector2d(lat, lon);
        var playerPosInWorld = map.GeoToWorldPosition(playerPos, false);
        transform.position = new Vector3(playerPosInWorld.x, 0.1f, playerPosInWorld.z);
    }
}
