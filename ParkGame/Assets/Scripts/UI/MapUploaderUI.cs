using System;
using System.Collections.Generic;
using Firebase;
using Firebase.Database;
using UnityEngine;
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
        [SerializeField] private InputField mapName;

        private DatabaseReference databaseReference;

        void Awake()
        {
            uploadButton.onClick.AddListener(uploadMap);
            FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
            {
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

            string geoLocation = "654612.4564684, 14654.420"; // todo get actual location
            
            MapData mapData = new MapData(numTeams, mapName.text, geoLocation, Guid.NewGuid());
            string mapJson = JsonUtility.ToJson(mapData);
            databaseReference.Child("maps").Child(mapData.MapId).SetRawJsonValueAsync(mapJson);
        }
    }
   
}