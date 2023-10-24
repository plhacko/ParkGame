using System.Collections;
using System.Collections.Generic;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using Unity.Services.Lobbies;
using System;
using Unity.Services.Authentication;
using Unity.Services.Core;
using System.Threading.Tasks;
using System.Linq;
using Mapbox.Map;
using UnityEditor;
using Firebase;
using Firebase.Storage;
using Firebase.Database;
using UnityEngine.Networking;
using UnityEngine.TextCore.LowLevel;

namespace Managers
{
    public struct LobbyModel 
    {
        public string Id;
        public string LobbyCode;
        public string HostId;
        public string MapId;
        public string Name;
        public int MaxPlayers;
        public Dictionary<string, int> Teams;

        // Content equality check
        public override bool Equals(object obj)
        {
            return obj is LobbyModel model &&
                   Id == model.Id &&
                   LobbyCode == model.LobbyCode &&
                   HostId == model.HostId &&
                   MapId == model.MapId &&
                   Name == model.Name &&
                   MaxPlayers == model.MaxPlayers &&
                   Teams.OrderBy(x => x.Key).SequenceEqual(model.Teams.OrderBy(x => x.Key));
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, LobbyCode, HostId, Name, MaxPlayers, Teams);
        }
    }
    public class LobbyManager : MonoBehaviour
    {
        public static LobbyManager Singleton { get; private set; }

        private Lobby lobby;
        public Lobby Lobby { get { return lobby; } }

        private MapData mapData = null;
        public MapData MapData { get { return mapData; } }

        private LobbyModel lobbyModel;
        public LobbyModel LobbyModel { 
            get
            { 
                return lobbyModel; 
            } 
            private set 
            { 
                lobbyModel = value; 
                OnLobbyInvalidated?.Invoke();
            }
        }
     
        public Action OnLobbyInvalidated;
        public bool IsHost { get { return lobby != null && lobby.HostId == AuthenticationService.Instance.PlayerId; } }

        private void Awake()
        {
            if (Singleton != null)
            {
                Destroy(gameObject);
                return;
            }

            Singleton = this;
        }

        private void OnDestroy()
        {
            if (Singleton == this)
            {
                Singleton = null;
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.L))
            {
                PrintPlayers(lobby);
            }
        }


        public async Task<bool> CreateLobbyForMap(MapData mapData)
        {
            try 
            {
                // TODO temporary lobby name
                string lobbyName = "My Lobby";
                // TODO temporary max players
                int maxPlayers = mapData.MetaData.NumTeams * 4;
                this.mapData = mapData;

                CreateLobbyOptions createLobbyOptions = new()
                {
                    IsPrivate = true,
                    Player = GetPlayerWithData(),
                    Data = GetLobbyData(),
                };

                lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, createLobbyOptions);
                LobbyModel = GetLobbyModel();

                StartCoroutine(HeatbeatLobbyCoroutine(lobby));
                StartCoroutine(PollLobbyCoroutine());

                Debug.Log("Lobby created with ID: " + lobby.Id + " and code: " + lobby.LobbyCode);
                return true;
            } 
            catch (Exception e)
            {
                Debug.LogError("Failed to create lobby: " + e.Message);
                return false;
            }
        }

        public async Task<bool> JoinLobbyByCode(string code)
        {
            try
            {
                JoinLobbyByCodeOptions joinLobbyByCodeOptions = new()
                {
                    Player = GetPlayerWithData(),
                };

                lobby = await LobbyService.Instance.JoinLobbyByCodeAsync(code, joinLobbyByCodeOptions);
                LobbyModel = GetLobbyModel();


                StartCoroutine(PollLobbyCoroutine());

                Debug.Log("Lobby joined with ID: " + lobby.Id + " and code: " + lobby.LobbyCode);

                return true;
            }
            catch (Exception e)
            {
                Debug.LogError("Failed to join lobby: " + e.Message);
                return false;
            }
        }

        IEnumerator HeatbeatLobbyCoroutine(Lobby lobby)
        {
            var delay = new WaitForSeconds(15);

            while (true)
            {
                LobbyService.Instance.SendHeartbeatPingAsync(lobby.Id);
                yield return delay;
            }
        }

        IEnumerator PollLobbyCoroutine()
        {
            var delay = new WaitForSeconds(1);

            while (true)
            {
                var t = Task.Run(async () => await LobbyService.Instance.GetLobbyAsync(lobby.Id));
                yield return new WaitUntil(() => t.IsCompleted);

                lobby = t.Result;
                LobbyModel = GetLobbyModel();

                yield return delay;
            }
        }

        private void PrintPlayers(Lobby lobby)
        {
            foreach (var player in lobby.Players)
            {
                Debug.Log("Player: " + player.Id + " " + player.Data["PlayerName"].Value + " " + player.Data["TeamNumber"].Value);  
            }
        }

        private Unity.Services.Lobbies.Models.Player GetPlayerWithData()
        {
            return new() 
            {
                Data = new()
                {
                    {
                        "PlayerName",
                        new PlayerDataObject
                        (
                            PlayerDataObject.VisibilityOptions.Member,
                            PlayerPrefs.GetString("PlayerName", "Player")
                        )
                    },
                    {
                        "TeamNumber",
                        new PlayerDataObject
                        (
                            PlayerDataObject.VisibilityOptions.Member,
                            "-1"
                        )
                    }
                }
            };
        }

        private Dictionary<string, DataObject> GetLobbyData()
        {
            return new()
            {
                {
                    "MapId",
                    new DataObject
                    (
                        DataObject.VisibilityOptions.Public,
                        mapData.MetaData.MapId
                    )
                },
                {
                    "MapName",
                    new DataObject
                    (
                        DataObject.VisibilityOptions.Public,
                        mapData.MetaData.MapName
                    )
                }
            };
        }

        public async void JoinTeam(int teamNumber)
        {
            try 
            {
                var player = GetPlayerWithData();
                player.Data["TeamNumber"].Value = teamNumber.ToString();

                UpdatePlayerOptions updatePlayerOptions = new()
                {
                    Data = player.Data,
                };

                lobby = await Lobbies.Instance.UpdatePlayerAsync(lobby.Id, AuthenticationService.Instance.PlayerId, updatePlayerOptions);
                LobbyModel = GetLobbyModel();

                Debug.Log("Player " + AuthenticationService.Instance.PlayerId + " joined team " + teamNumber);
            }
            catch (Exception e)
            {
                Debug.LogError("Failed to join team: " + e.Message);
            }
        }

        public async void ChangeTeamForPlayer(string playerId, int teamNumber)
        {
            try 
            {
                var player = lobby.Players.Find(x => x.Id == playerId);
                player.Data["TeamNumber"].Value = teamNumber.ToString();

                UpdatePlayerOptions updatePlayerOptions = new()
                {
                    Data = player.Data,
                };

                await Lobbies.Instance.UpdatePlayerAsync(lobby.Id, playerId, updatePlayerOptions);

                Debug.Log("Player " + playerId + " joined team " + teamNumber);
            }
            catch (Exception e)
            {
                Debug.LogError("Failed to join team: " + e.Message);
            }
        }

        public LobbyModel GetLobbyModel()
        {
            return new LobbyModel
            {
                Id = lobby.Id,
                LobbyCode = lobby.LobbyCode,
                MapId = lobby.Data["MapId"].Value,
                HostId = lobby.HostId,
                Name = lobby.Name,
                MaxPlayers = lobby.MaxPlayers,
                Teams = GetTeams()
            };
        }

        public Dictionary<string, int> GetTeams()
        {
            Dictionary<string, int> teams = new();

            foreach (var player in lobby.Players)
            {
                if (player.Data.ContainsKey("TeamNumber"))
                {
                    teams.Add(player.Id, int.Parse(player.Data["TeamNumber"].Value));
                }
            }

            return teams;
        }

        public async Task GetMapData()
        {
            var mapId = lobbyModel.MapId;
            var mapDataDownload = await DownloadMapData(mapId);
            
            if (!mapDataDownload.Item1)
            {
                Debug.LogError("Failed to download map data");
            }

            mapData = mapDataDownload.Item2;
        }

        private async Task<Tuple<bool,MapData>> DownloadMapData(string mapId)
        {
            await FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
                {
                    if (task.Exception != null)
                    {
                        Debug.LogError($"Failed to intialize Firebase with {task.Exception}");
                        return;
                    }
                   
#if UNITY_EDITOR
                    // Unity sometimes crashes when Firebase Persistence is Enabled and two editors use it 
                    // FirebaseStorage.DefaultInstance.SetPersistenceEnabled(false);
                    FirebaseDatabase.DefaultInstance.SetPersistenceEnabled(false);
#endif
                    Debug.Log(task.Status);
                }
            );

            var storageReference = FirebaseStorage.DefaultInstance.RootReference;
            var databaseReference = FirebaseDatabase.DefaultInstance.RootReference;
        
            DataSnapshot dataSnapshot = await databaseReference.Child(FirebaseConstants.MAP_DATA_FOLDER).Child(mapId).GetValueAsync();
            MapMetaData mapMetaData = JsonUtility.FromJson<MapMetaData>(dataSnapshot.GetRawJsonValue());

            var imageReference = storageReference.Child($"{FirebaseConstants.MAP_IMAGES_FOLDER}/{mapMetaData.MapId}.png");

            var imageBytes = await imageReference.GetBytesAsync(FirebaseConstants.MAX_MAP_SIZE);
            Texture2D texture = new Texture2D(mapMetaData.Width, mapMetaData.Height); 
            texture.LoadImage(imageBytes);
           
            mapData = new MapData
            {
                MetaData = mapMetaData,
                DrawnTexture = texture
            };

            var request = UnityWebRequestTexture.GetTexture(mapData.MetaData.MapQuery);

            var operation = request.SendWebRequest();
            
            while (!operation.isDone)
            {
                await Task.Yield();
            }

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogWarning("API Request error: " + request.error);
                return new Tuple<bool, MapData>(false, mapData);
            }
      
            Texture2D texture2 = DownloadHandlerTexture.GetContent(request);
            mapData.GPSTexture = texture2;

            return new Tuple<bool, MapData>(true, mapData);
        }
    }
}
