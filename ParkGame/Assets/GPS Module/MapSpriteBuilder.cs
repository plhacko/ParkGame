using Mapbox.Examples;
using Mapbox.Unity.Map;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MapSpriteBuilder : MonoBehaviour
{
    public GameObject mapSprite;
    public QuadTreeCameraMovement qt;

    public void InstantiateMapSprite()
    {
        Vector4 bbox = qt.GetSelectedRegionBoundingBox();
        MapDisplayer mapDisplayer = mapSprite.GetComponent<MapDisplayer>();
        mapDisplayer.MinLongitude = bbox.x;
        mapDisplayer.MinLatitude = bbox.y;
        mapDisplayer.MaxLongitude = bbox.z;
        mapDisplayer.MaxLatitude = bbox.w;

        var mapSpriteInstance = Instantiate(mapSprite);

        mapSpriteInstance.SetActive(true);
    }
}
