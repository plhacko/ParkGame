using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Firebase;
using Firebase.Database;
using Firebase.Storage;
using TMPro;
using UI;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class MapPicker : MonoBehaviour
{
    [SerializeField] private float maxDistance; 
    [SerializeField] private Button nextMapButton;
    [SerializeField] private Button previousMapButton;
    [SerializeField] private RawImage image;
    [SerializeField] private TextMeshProUGUI mapNameText;
    [SerializeField] private TextMeshProUGUI mapDistanceText;
    [SerializeField] private TextMeshProUGUI maxNumTeamsText;
    
    private DatabaseReference databaseReference;
    private StorageReference storageReference;

    private List<MapData> mapDatas = new();
    private Texture2D[] mapTextures;
    private int currentMapIndex = 0;

    void Awake()
    {
        nextMapButton.onClick.AddListener(onNextClicked);
        previousMapButton.onClick.AddListener(onPreviousClicked);
        downloadMaps();
    }

    private void onNextClicked()
    {
        currentMapIndex = (currentMapIndex + 1) % mapDatas.Count;
        showCurrentMap();
    }
    
    private void onPreviousClicked()
    {
        currentMapIndex = (currentMapIndex - 1) % mapDatas.Count;
        if (currentMapIndex == -1)
        {
            currentMapIndex = mapDatas.Count - 1;
        }
        showCurrentMap();
    }
    
    private void showCurrentMap()
    {
        if(currentMapIndex >= mapTextures.Length) return;
        
        MapData mapData = mapDatas[currentMapIndex];
        
        (double currentLongitude, double currentLatitude) = getCurrentGeoPosition();
        double distance = getGeoDistance(currentLongitude, currentLatitude, mapData.Longitude, mapData.Latitude);

        image.texture = mapTextures[currentMapIndex];
        mapNameText.text = mapData.MapName;
        mapDistanceText.text = "(" +(distance / 1000).ToString("F1") + " km)";
        maxNumTeamsText.text = "Max teams: " + mapData.NumTeams;
    }

    private async void downloadMaps()
    {
        (double currentLongitude, double currentLatitude) = getCurrentGeoPosition();

        await FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task => { Debug.Log(task.Status); });
        
        storageReference = FirebaseStorage.DefaultInstance.GetReferenceFromUrl(FirebaseConstants.STORAGE_URL);
        databaseReference = FirebaseDatabase.DefaultInstance.RootReference;
        
        DataSnapshot dataSnapshot = await databaseReference.Child(FirebaseConstants.MAP_DATA_FOLDER).GetValueAsync();
        
        foreach (var mapDataDataSnapshot in dataSnapshot.Children)
        {
            MapData mapData = JsonUtility.FromJson<MapData>(mapDataDataSnapshot.GetRawJsonValue());
            double distance = getGeoDistance(currentLongitude, currentLatitude, mapData.Longitude, mapData.Latitude);

            if (distance < maxDistance)
            {
                mapDatas.Add(mapData);   
            }
        }
        
        // sort by geo distance from current location
        mapDatas.Sort((mapData, mapData1) =>
        {
            double distance = getGeoDistance(currentLongitude, currentLatitude, mapData.Longitude, mapData.Latitude);
            double distance1 = getGeoDistance(currentLongitude, currentLatitude, mapData1.Longitude, mapData1.Latitude);
            return distance.CompareTo(distance1);
        });

        mapTextures = new Texture2D[mapDatas.Count];
        
        var tasks = mapDatas.Select(async (mapData, index) =>
        {
            var imageReference = storageReference.Child($"{FirebaseConstants.MAP_FOLDER}/{mapData.MapId}.jpg");
            var imageBytes = await imageReference.GetBytesAsync(FirebaseConstants.MAX_MAP_SIZE);
            Texture2D texture = new Texture2D(mapData.Width, mapData.Height); 
            texture.LoadImage(imageBytes);
            mapTextures[index] = texture;
            
        }).ToArray();

        await Task.WhenAll(tasks);
        
        nextMapButton.interactable = true;
        previousMapButton.interactable = true;

        currentMapIndex = 0;
        showCurrentMap();
    }

    private (double, double) getCurrentGeoPosition()
    {
        double currentLongitude = 50.08044798178662; // todo get actual current geographical location
        double currentLatitude = 14.441389839994997; // todo -----------------||----------------------

        return (currentLongitude, currentLatitude);
    }
    
    private double getGeoDistance(double longitude, double latitude, double otherLongitude, double otherLatitude)
    {
        var d1 = latitude * (math.PI / 180.0);
        var num1 = longitude * (math.PI / 180.0);
        var d2 = otherLatitude * (math.PI / 180.0);
        var num2 = otherLongitude * (math.PI / 180.0) - num1;
        var d3 = math.pow(math.sin((d2 - d1) / 2.0), 2.0) + math.cos(d1) * math.cos(d2) * math.pow(math.sin(num2 / 2.0), 2.0);
    
        return 6376500.0 * (2.0 * math.atan2(math.sqrt(d3), math.sqrt(1.0 - d3)));
    }
}
