using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase;
using Firebase.Database;
using Firebase.Extensions;
using Firebase.Storage;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public static class FirebaseConstants
    {
        public static string STORAGE_URL = "gs://theparkgame-97204.appspot.com";
        public static string MAP_FOLDER = "MapImages";
        public static string MAP_DATA_FOLDER = "Maps";
        public static long MAX_MAP_SIZE = 1024 * 1024 * 12; // 12M
    }
    
    public class MapData
    {
        public string MapId;
        public int NumTeams;
        public string MapName;
        public double Longitude;
        public double Latitude;
        public int Width;
        public int Height;
        
        public MapData(int numTeams, string mapName, double longitude, double latitude, int width, int height, Guid mapId)
        {
            NumTeams = numTeams;
            MapName = mapName;
            Longitude = longitude;
            Latitude = latitude;
            Width = width;
            Height = height;
            MapId = mapId.ToString();
        }
        
    }

    public class MapUploaderUI : MonoBehaviour
    {
        [SerializeField] private Button uploadButton;
        [SerializeField] private List<Toggle> numTeamsToggles;
        [SerializeField] private TMP_InputField mapNameInputField;
        [SerializeField] private Texture2D texture;
        
        private DatabaseReference databaseReference;
        private StorageReference storageReference;

        void Awake()
        {
            uploadButton.onClick.AddListener(uploadMap);
            FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
            {
                storageReference = FirebaseStorage.DefaultInstance.GetReferenceFromUrl(FirebaseConstants.STORAGE_URL);
                databaseReference = FirebaseDatabase.DefaultInstance.RootReference;
            });
        }
        
        public void Update()
        {
            uploadButton.interactable = databaseReference != null;
        }

        private void uploadMap()
        {
            int numTeams = 2;
            for (int i = 0; i < numTeamsToggles.Count; i++)
            {
                if (numTeamsToggles[i].isOn)
                {
                    numTeams = i + 2;
                    break;
                }
            }

            string mapName = mapNameInputField.text;
            Guid guid = Guid.NewGuid();
            byte[] bytes = texture.EncodeToPNG(); // todo get actual map
            double longitude = 50.08044798178662; // todo get actual location
            double latitude = 14.441389839994997;
            
            if(bytes.Length > FirebaseConstants.MAX_MAP_SIZE) throw new Exception("Map is too big!");

            MapData mapData = new MapData(numTeams, mapName, longitude, latitude, texture.width, texture.height, guid);

            storageReference.Child($"{FirebaseConstants.MAP_FOLDER}/{mapData.MapName}_{mapData.MapId}.jpg").PutBytesAsync(bytes).ContinueWithOnMainThread(task =>
            {
                if (task.Status == TaskStatus.RanToCompletion) {
                    Debug.Log("Texture uploaded successfully!");
                } else {
                    Debug.Log("Error uploading texture: " + task.Exception);
                }
            });
            
            string mapJson = JsonUtility.ToJson(mapData);
            databaseReference.Child(FirebaseConstants.MAP_DATA_FOLDER).Child(mapData.MapId).SetRawJsonValueAsync(mapJson);
        }
    }
   
}