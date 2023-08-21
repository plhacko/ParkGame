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

public class MapDataFirebaseManager : MonoBehaviour
{
    [Header("Map data")]
    public CreateMapWithOverlay MapWithOverlay;
    public StructureCounter OutpostCounter;
    public StructureCounter VictoryPointCounter;
    public StructureCounter CastleCounter;

    // TODO : Change this to the actual player ID
    public Guid PlayerGuid = Guid.NewGuid();

    private bool _isInitialized = false;
    public UnityEvent OnInitialize = new UnityEvent();

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

    private MapMetaDataNew PrepareMapMetaData()
    {
        MapDisplayer mapDisplayer = MapWithOverlay.GetFetchedMap().GetComponent<MapDisplayer>();
        return new MapMetaDataNew(PlayerGuid, Guid.NewGuid(), "map name", mapDisplayer.urlProperty);
    }

    // Upload the map data to Firebase Database and initiate the upload of the map image
    public void UploadMapData()
    {
        if (!_isInitialized)
        {
            Debug.LogError("Firebase is not initialized");
            return;
        }

        MapMetaDataNew mapMetaDataNew = PrepareMapMetaData();
        Texture2D mapImage = MapWithOverlay.GetComponent<CreateMapWithOverlay>().GetLowResTextureForTilemapCreation();
        var (outpostString, outpostGridPositions) = OutpostCounter.SavePlacedStructures(); 
        var (victoryPointString, victoryPointGridPositions) = VictoryPointCounter.SavePlacedStructures(); 
        var (castleString, castleGridPositions) = CastleCounter.SavePlacedStructures();
        
        string json = JsonUtility.ToJson(mapMetaDataNew);
        _database.GetReference($"{PlayerGuid}/{mapMetaDataNew.MapId}/").SetRawJsonValueAsync(json);

        for (int i = 0; i < outpostGridPositions.Count; i++)
        {
            SerializedVector3Int gridPosition = new SerializedVector3Int(outpostGridPositions[i]);
            json = JsonUtility.ToJson(gridPosition);
            _database.GetReference($"{PlayerGuid}/{mapMetaDataNew.MapId}/{outpostString}/{i}").SetRawJsonValueAsync(json);
        }

        for (int i = 0; i < victoryPointGridPositions.Count; i++)
        {
            SerializedVector3Int gridPosition = new SerializedVector3Int(victoryPointGridPositions[i]);
            json = JsonUtility.ToJson(gridPosition);
            _database.GetReference($"{PlayerGuid}/{mapMetaDataNew.MapId}/{victoryPointString}/{i}").SetRawJsonValueAsync(json);
        }

        for (int i = 0; i < castleGridPositions.Count; i++)
        {
            SerializedVector3Int gridPosition = new SerializedVector3Int(castleGridPositions[i]);
            json = JsonUtility.ToJson(gridPosition);
            _database.GetReference($"{PlayerGuid}/{mapMetaDataNew.MapId}/{castleString}/{i}").SetRawJsonValueAsync(json);
        }

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
        var imageReference = _storage.GetReference($"/{PlayerGuid}/{mapMetaDataNew.MapId}.png");
        var bytes = image.EncodeToPNG();
        var uploadTask = imageReference.PutBytesAsync(bytes);
        yield return new WaitUntil(() => uploadTask.IsCompleted);

        if (uploadTask.Exception != null)
        {
            Debug.LogError($"Failed to upload because {uploadTask.Exception}");
            yield break;
        }

        var getUrlTask = imageReference.GetDownloadUrlAsync();
        yield return new WaitUntil(() => getUrlTask.IsCompleted);

        if (getUrlTask.Exception != null)
        {
            Debug.LogError($"Failed to get URL because {getUrlTask.Exception}");
            yield break;
        }

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

        _database.GetReference($"{PlayerGuid}/{mapId}/").RemoveValueAsync();
        var imageReference = _storage.GetReference($"/{PlayerGuid}/{mapId}.png");
        imageReference.DeleteAsync();

        // TODO check if the map data is successfully deleted
    }
}
