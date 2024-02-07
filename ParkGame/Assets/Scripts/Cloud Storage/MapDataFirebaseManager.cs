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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Managers;
using TMPro;
using UI;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using Mapbox.Utils;

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
                
#if UNITY_EDITOR // Unity sometimes crashes when Firebase Persistence is Enabled and two editors use it 
                // FirebaseStorage.DefaultInstance.SetPersistenceEnabled(false);
                FirebaseDatabase.DefaultInstance.SetPersistenceEnabled(false);
#endif
                
                _database = FirebaseDatabase.DefaultInstance;
                _storage = FirebaseStorage.DefaultInstance;

                OnInitialize.Invoke();
            }
        });
    }

    private MapMetaData PrepareMapMetaData(string mapName, Texture2D drawTexture)
    {
        MapDisplayer mapDisplayer = MapWithOverlay.GetFetchedMap().GetComponent<MapDisplayer>();
        
        var (outpostString, outpostGridPositions) = OutpostCounter.GetPlacedStructurePositions(); 
        var (victoryPointString, victoryPointGridPositions) = VictoryPointCounter.GetPlacedStructurePositions(); 
        var (castleString, castleGridPositions) = CastleCounter.GetPlacedStructurePositions();

        var (topLeft, bottomRight) = MapWithOverlay.GetTilemapBounds();

        var gpsMinCoords = new Vector2d(mapDisplayer.MinLongitude, mapDisplayer.MinLatitude);
        var gpsMaxCoords = new Vector2d(mapDisplayer.MaxLongitude, mapDisplayer.MaxLatitude);
        var centerCoords = gpsMinCoords + (gpsMaxCoords - gpsMinCoords) / 2;
        
        return new MapMetaData(
            Guid.NewGuid(),
            mapName == string.Empty ? "Untitled" : mapName,
            mapDisplayer.urlProperty,
            centerCoords.x,
            centerCoords.y,
            drawTexture.width,
            drawTexture.height,
            new MapStructures(outpostGridPositions, victoryPointGridPositions, castleGridPositions),
            topLeft,
            bottomRight
            );
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
        
        MapMetaData mapMetaData = PrepareMapMetaData(mapName, mapImage);
        if(mapMetaData.Structures.Castles.Length is 0 or >= 4)
        {
            // TODO show error message to the user
            // there must be at least 2 teams and maximum of 4 teams 
            Debug.LogError("Map must have at least two castles"); 
            return;
        }
        
        
        string json = JsonUtility.ToJson(mapMetaData);
        _database.GetReference($"{FirebaseConstants.MAP_DATA_FOLDER}/{mapMetaData.MapId}/").SetRawJsonValueAsync(json);
        
        StartUploadMapImage(mapMetaData, mapImage);
    }

    // Initiate the upload of the map image to Firebase Storage
    private void StartUploadMapImage(MapMetaData mapMetaData, Texture2D mapImage)
    {
        if (!_isInitialized)
        {
            Debug.LogError("Firebase is not initialized");
            return;
        }

        StartCoroutine(UploadMapImage(mapMetaData, mapImage));
    }

    // Upload the map image to Firebase Storage
    private IEnumerator UploadMapImage(MapMetaData mapMetaData, Texture2D image)
    {
        var imageReference = _storage.GetReference($"/{FirebaseConstants.MAP_IMAGES_FOLDER}/{mapMetaData.MapId}.png");
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
