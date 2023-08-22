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

public static class FirebaseConstants
{
    public static string STORAGE_URL = "gs://theparkgame-97204.appspot.com";
    public static string MAP_IMAGES_FOLDER = "MapImages";
    public static string MAP_DATA_FOLDER = "Maps";
    public static long MAX_MAP_SIZE = 1024 * 1024 * 12; // 12M
}

public class MapData
{
    public MapMetaDataNew MetaData;
    public Texture2D Texture;
}

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
    private int currentMapIndex = -1;

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
        if(currentMapIndex >= mapDatas.Count) return;
        
        MapMetaDataNew mapMetaDataNew = mapDatas[currentMapIndex].MetaData;
        
        (double currentLongitude, double currentLatitude) = getCurrentGeoPosition();
        double distance = getGeoDistance(currentLongitude, currentLatitude, mapMetaDataNew.Longitude, mapMetaDataNew.Latitude);
        
        image.texture = mapDatas[currentMapIndex].Texture;
        mapNameText.text = mapMetaDataNew.MapName;
        mapDistanceText.text = "(" +(distance / 1000).ToString("F1") + " km)";
        maxNumTeamsText.text = "Max teams: " + mapMetaDataNew.NumTeams;
    }

    private async void downloadMaps()
    {
        (double currentLongitude, double currentLatitude) = getCurrentGeoPosition();

        await FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task => { Debug.Log(task.Status); });
        
        storageReference = FirebaseStorage.DefaultInstance.RootReference;
        databaseReference = FirebaseDatabase.DefaultInstance.RootReference;
        
        DataSnapshot dataSnapshot = await databaseReference.Child(FirebaseConstants.MAP_DATA_FOLDER).GetValueAsync();
        
        foreach (var mapDataDataSnapshot in dataSnapshot.Children)
        {
            MapMetaDataNew mapMetaDataNew = JsonUtility.FromJson<MapMetaDataNew>(mapDataDataSnapshot.GetRawJsonValue());
            double distance = getGeoDistance(currentLongitude, currentLatitude, mapMetaDataNew.Longitude, mapMetaDataNew.Latitude);
            
            if (distance < maxDistance)
            {
                MapData mapData = new MapData
                {
                    MetaData = mapMetaDataNew
                };
                mapDatas.Add(mapData);   
            }
        }
        
        // sort by geo distance from current location
        mapDatas.Sort((mapData, mapData1) =>
        {
            double distance = getGeoDistance(currentLongitude, currentLatitude, mapData.MetaData.Longitude, mapData.MetaData.Latitude);
            double distance1 = getGeoDistance(currentLongitude, currentLatitude, mapData1.MetaData.Longitude, mapData1.MetaData.Latitude);
            return distance.CompareTo(distance1);
        });

        var tasks = mapDatas.Select(async mapData =>
        {
            var imageReference = storageReference.Child($"{FirebaseConstants.MAP_IMAGES_FOLDER}/{mapData.MetaData.MapId}.png");
            try
            {
                var imageBytes = await imageReference.GetBytesAsync(FirebaseConstants.MAX_MAP_SIZE);
                Texture2D texture = new Texture2D(mapData.MetaData.Width, mapData.MetaData.Height); 
                texture.LoadImage(imageBytes);
                mapData.Texture = texture;
            }
            catch (StorageException e)
            {
                Debug.LogWarning(mapData.MetaData.MapName + " " + mapData.MetaData.MapId + " " + e.Message);
            }

        }).ToArray();

        await Task.WhenAll(tasks);

        mapDatas = mapDatas.Where(mapData => mapData.Texture != null).ToList();
        
        nextMapButton.interactable = true;
        previousMapButton.interactable = true;

        currentMapIndex = 0;
        image.color = Color.white;
        showCurrentMap();
    }

    private (double, double) getCurrentGeoPosition()
    {
        double currentLongitude = 14.418540; // todo get actual current geographical location
        double currentLatitude = 50.073658; // todo -----------------||----------------------

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

    public MapData GetCurrentMapData()
    {
        return mapDatas[currentMapIndex];
    }

    public bool IsInitialized()
    {
        return currentMapIndex != -1;
    }
}
