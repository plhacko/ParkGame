using Mapbox.Examples;
using Mapbox.Unity.Map;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MapSpriteBuilder : MonoBehaviour
{
    public GameObject mapSprite;
    public QuadTreeCameraMovement qt;
    public SceneAsset mapDrawingScene;

    public void InstantiateMapSprite()
    {
        // Called after submitting map region
        Vector4 bbox = qt.GetSelectedRegionBoundingBox();
        Vector2 normalizedSideLengths = qt.GetSelectedRegionNormalizedSideLengths();
        var pixelSideLengths = 1280 * normalizedSideLengths;

        MapDisplayer mapDisplayer = mapSprite.GetComponent<MapDisplayer>();

        mapDisplayer.Width = (int)pixelSideLengths.x;
        mapDisplayer.Height = (int)pixelSideLengths.y;

        mapDisplayer.MinLongitude = bbox.x;
        mapDisplayer.MinLatitude = bbox.y;
        mapDisplayer.MaxLongitude = bbox.z;
        mapDisplayer.MaxLatitude = bbox.w;

        var mapSpriteInstance = Instantiate(mapSprite);

        mapSpriteInstance.SetActive(true);
        SceneManager.LoadScene(mapDrawingScene.name);
        
    }
}
