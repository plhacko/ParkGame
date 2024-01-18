using Mapbox.Examples;
using Mapbox.Unity.Map;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MapSpriteBuilder : MonoBehaviour
{
    public GameObject mapSprite;
    public QuadTreeCameraMovement qt;
    public string mapDrawingSceneName = "MapDrawingScene";

    public void InstantiateMapSprite()
    {
        // Called after submitting map region
        Vector4 bbox = qt.GetSelectedRegionBoundingBox();

        var areaKm = CalculateBoundingBoxAreaInSquareKm(bbox.x, bbox.y, bbox.z, bbox.w);
        var areaM = areaKm * 1000000;
        if (areaM < 100)
        {
            // TODO show error message
            Debug.LogError("Selected region is too small. Please select a larger region.");
            return;
        } 

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

        // Pass map sprite to next scene and load it
        StartCoroutine(LoadAsyncSceneWithMapSprite(mapSpriteInstance));
    }

    public double CalculateBoundingBoxAreaInSquareKm(double minLongitude, double minLatitude, double maxLongitude, double maxLatitude)
    {
        // Convert degrees to radians
        double minLatRad = minLatitude * Math.PI / 180;
        double maxLatRad = maxLatitude * Math.PI / 180;
        double minLonRad = minLongitude * Math.PI / 180;
        double maxLonRad = maxLongitude * Math.PI / 180;

        // Radius of the Earth in kilometers
        double earthRadiusKm = 6371.0;

        // Calculate the area of the spherical rectangle
        double area = earthRadiusKm * earthRadiusKm *
                    Math.Abs(Math.Sin(minLatRad) - Math.Sin(maxLatRad)) *
                    Math.Abs(minLonRad - maxLonRad);

        return area;
    }

    IEnumerator LoadAsyncSceneWithMapSprite(GameObject mapSprite)
    {
        Scene currentScene = SceneManager.GetActiveScene(); 

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(mapDrawingSceneName, LoadSceneMode.Additive);

        // Wait until the asynchronous scene fully loads
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        // Pass map sprite to next scene
        SceneManager.MoveGameObjectToScene(mapSprite, SceneManager.GetSceneByName(mapDrawingSceneName));
        // Unload previous scene
        SceneManager.UnloadSceneAsync(currentScene);
    }
}
