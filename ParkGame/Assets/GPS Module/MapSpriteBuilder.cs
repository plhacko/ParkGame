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

        // Pass map sprite to next scene and load it
        StartCoroutine(LoadAsyncSceneWithMapSprite(mapSpriteInstance));
    }

    IEnumerator LoadAsyncSceneWithMapSprite(GameObject mapSprite)
    {
        Scene currentScene = SceneManager.GetActiveScene(); 

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(mapDrawingScene.name, LoadSceneMode.Additive);

        // Wait until the asynchronous scene fully loads
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        // Pass map sprite to next scene
        SceneManager.MoveGameObjectToScene(mapSprite, SceneManager.GetSceneByName(mapDrawingScene.name));
        // Unload previous scene
        SceneManager.UnloadSceneAsync(currentScene);
    }
}
