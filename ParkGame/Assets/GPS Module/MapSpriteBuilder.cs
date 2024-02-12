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

        Vector2 normalizedSideLengths = qt.GetSelectedRegionNormalizedSideLengths();
        var pixelSideLengths = 1280 * normalizedSideLengths;
        
        MapDisplayer mapDisplayer = mapSprite.GetComponent<MapDisplayer>();

        mapDisplayer.Width = (int)pixelSideLengths.x;
        mapDisplayer.Height = (int)pixelSideLengths.y;

        mapDisplayer.MinLongitude = bbox.x;
        mapDisplayer.MinLatitude = bbox.y;
        mapDisplayer.MaxLongitude = bbox.z;
        mapDisplayer.MaxLatitude = bbox.w;
        
        var areaM = mapDisplayer.CalculateBoundingBoxAreaInSquareMeters();
        if (areaM < 100)
        {
            // TODO show error message
            Debug.LogError("Selected region is too small. Please select a larger region.");
            return;
        }

        if (areaM > 1000 * 1000)
        {
            // TODO show error message
            Debug.LogError("Selected region is too large. Please select a smaller region.");
            return;
        }

        var mapSpriteInstance = Instantiate(mapSprite);
        // Pass map sprite to next scene and load it
        // StartCoroutine(LoadAsyncSceneWithMapSprite(mapSpriteInstance));
        LoadSceneWithMapSprite(mapSpriteInstance);
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

    private void LoadSceneWithMapSprite(GameObject mapSprite)
    {
        Debug.Log($"Loading {mapDrawingSceneName}...");
        DontDestroyOnLoad(mapSprite);
        SceneManager.LoadScene(mapDrawingSceneName);
    }
}
