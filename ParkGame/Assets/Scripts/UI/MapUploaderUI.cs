using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase;
using Firebase.Database;
using Firebase.Extensions;
using Firebase.Storage;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace UI
{
    public class MapData
    {
        public int NumTeams;
        public string MapName;
        public string GeoPosition;
        public string MapId;
        
        public MapData(int numTeams, string mapName, string geoPosition, Guid mapId)
        {
            NumTeams = numTeams;
            MapName = mapName;
            GeoPosition = geoPosition;
            MapId = mapId.ToString();
        }
    }

    public class MapUploaderUI : MonoBehaviour
    {
        [SerializeField] private Button uploadButton;
        [SerializeField] private List<Toggle> numTeamsToggles;
        [FormerlySerializedAs("mapName")] [SerializeField] private TMP_InputField mapNameInputField;
        [SerializeField] private Texture2D texture;
        
        private DatabaseReference databaseReference;
        private StorageReference storageReference;

        void Awake()
        {
            uploadButton.onClick.AddListener(uploadMap);
            FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
            {
                string url = "gs://theparkgame-97204.appspot.com";
                storageReference = FirebaseStorage.DefaultInstance.GetReferenceFromUrl(url);
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
            string geoLocation = "654612.4564684, 14654.420"; // todo get actual location

            MapData mapData = new MapData(numTeams, mapName, geoLocation, guid);
            storageReference.Child($"MapImages/{mapData.MapName}_{mapData.MapId}.jpg").PutBytesAsync(bytes).ContinueWithOnMainThread(task =>
            {
                if (task.Status == TaskStatus.RanToCompletion) {
                    Debug.Log("Texture uploaded successfully!");
                } else {
                    Debug.Log("Error uploading texture: " + task.Exception);
                }
            });
            
            string mapJson = JsonUtility.ToJson(mapData);
            databaseReference.Child("Maps").Child(mapData.MapId).SetRawJsonValueAsync(mapJson);
        }
    }
   
}