using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using Firebase;
using Firebase.Extensions;
using Firebase.Database;
using Firebase.Storage;
using System;
using Managers;
using TMPro;
using UI;
using UnityEngine.SceneManagement;

public class MapDataFirebaseManager : MonoBehaviour
{
    [Header("Map data")]
    public CreateMapWithOverlay MapWithOverlay;
    public StructureCounter OutpostCounter;
    public StructureCounter VictoryPointCounter;
    public StructureCounter CastleCounter;

    private bool _isInitialized = false;
    public UnityEvent OnInitialize = new UnityEvent();
    public UnityEvent OnMapUploaded = new UnityEvent();
    public UnityEvent OnMapUploadFailed = new UnityEvent();

    private FirebaseDatabase _database;
    private FirebaseStorage _storage;

    void Awake()
    {
        // Initialize Firebase
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Exception != null)
            {
                Debug.LogError($"Failed to intialize Firebase with {task.Exception}");
                return;
            }
            else
            {
                _isInitialized = true;
                _database = FirebaseDatabase.DefaultInstance;
                _storage = FirebaseStorage.DefaultInstance;
                OnInitialize.Invoke();
            }
        });
    }

    private MapMetaDataNew PrepareMapMetaData(string mapName, Texture2D drawTexture)
    {
        MapDisplayer mapDisplayer = MapWithOverlay.GetFetchedMap().GetComponent<MapDisplayer>();
        
        var (outpostString, outpostGridPositions) = OutpostCounter.SavePlacedStructures(); 
        var (victoryPointString, victoryPointGridPositions) = VictoryPointCounter.SavePlacedStructures(); 
        var (castleString, castleGridPositions) = CastleCounter.SavePlacedStructures();
        
        return new MapMetaDataNew(
            Guid.NewGuid(),
            mapName == string.Empty ? "Untitled" : mapName,
            mapDisplayer.urlProperty,
            14.418540,
            50.073658,
            drawTexture.width,
            drawTexture.height,
            new MapStructures(outpostGridPositions, victoryPointGridPositions, castleGridPositions));
    }

    // Upload the map data to Firebase Database and initiate the upload of the map image
    public void UploadMapData(string mapName)
    {
        if (!_isInitialized)
        {
            Debug.LogError("Firebase is not initialized");
            return;
        }

        Texture2D mapImage = MapWithOverlay.GetLowResTextureForTilemapCreation();
        
        MapMetaDataNew mapMetaDataNew = PrepareMapMetaData(mapName, mapImage);
        if(mapMetaDataNew.Structures.Castles.Length is 0 or >= 4)
        {
            // TODO show error message to the user
            // there must be at least 2 teams and maximum of 4 teams 
            Debug.LogError("Map must have at least two castles"); 
            return;
        }
        
        
        string json = JsonUtility.ToJson(mapMetaDataNew);
        _database.GetReference($"{FirebaseConstants.MAP_DATA_FOLDER}/{mapMetaDataNew.MapId}/").SetRawJsonValueAsync(json);
        
        StartUploadMapImage(mapMetaDataNew, mapImage);
    }

    // Initiate the upload of the map image to Firebase Storage
    private void StartUploadMapImage(MapMetaDataNew mapMetaDataNew, Texture2D mapImage)
    {
        if (!_isInitialized)
        {
            Debug.LogError("Firebase is not initialized");
            return;
        }

        StartCoroutine(UploadMapImage(mapMetaDataNew, mapImage));
    }

    // Upload the map image to Firebase Storage
    private IEnumerator UploadMapImage(MapMetaDataNew mapMetaDataNew, Texture2D image)
    {
        var imageReference = _storage.GetReference($"/{FirebaseConstants.MAP_IMAGES_FOLDER}/{mapMetaDataNew.MapId}.png");
        var bytes = image.EncodeToPNG();
        var uploadTask = imageReference.PutBytesAsync(bytes);
        yield return new WaitUntil(() => uploadTask.IsCompleted);

        if (uploadTask.Exception != null)
        {
            Debug.LogError($"Failed to upload because {uploadTask.Exception}");
            OnMapUploadFailed.Invoke();
            yield break;
        }

        var getUrlTask = imageReference.GetDownloadUrlAsync();
        yield return new WaitUntil(() => getUrlTask.IsCompleted);

        if (getUrlTask.Exception != null)
        {
            OnMapUploadFailed.Invoke();
            Debug.LogError($"Failed to get URL because {getUrlTask.Exception}");
            yield break;
        }

        OnMapUploaded.Invoke();
        Debug.Log($"Uploaded to {getUrlTask.Result}");
    }

    // Delete the map data from Firebase Database and initiate the deletion of the map image
    public void DeleteMapData(string mapId)
    {
        if (!_isInitialized)
        {
            Debug.LogError("Firebase is not initialized");
            return;
        }
        
        var imageReference = _storage.GetReference($"/{FirebaseConstants.MAP_IMAGES_FOLDER}/{mapId}.png");
        _database.GetReference($"/{FirebaseConstants.MAP_DATA_FOLDER}/{mapId}/").RemoveValueAsync();
        imageReference.DeleteAsync();

        // TODO check if the map data is successfully deleted
    }
}
