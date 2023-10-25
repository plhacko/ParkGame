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
using Unity.VisualScripting;
using Firebase.Auth;

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

        private Lobby _lobby;
        public Lobby Lobby { get { return _lobby; } private set { _lobby = value; } }

        private MapData mapData = null;
        public MapData MapData { get { return mapData; } }

        public string PlayerId => AuthenticationService.Instance.PlayerId;

        private LobbyModel _lobbyModel;
        public LobbyModel LobbyModel { get { return _lobbyModel; } private set { _lobbyModel = value; } }
     
        public Action OnLobbyInvalidat;
        public Action OnDisconnect;
     
        public bool IsHost { get { return Lobby != null && Lobby.HostId == AuthenticationService.Instance.PlayerId; } }

        public Task UnityServicesInitializeTask { get; private set; }

        private void Awake()
        {
            if (Singleton != null)
            {
                Destroy(gameObject);
                return;
            }

            UnityServicesInitializeTask = UnityServices.InitializeAsync();
            Singleton = this;
        }

        private void OnDestroy()
        {
            if (Singleton == this)
            {
                Singleton = null;
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

                Lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, createLobbyOptions);
                LobbyModel = GetLobbyModel();

                StartCoroutine(HeatbeatLobbyCoroutine(Lobby));
                StartCoroutine(PollLobbyCoroutine());

                PlayerPrefs.SetString("DebugRoomCode", Lobby.LobbyCode);

                Debug.Log("Lobby created with ID: " + Lobby.Id + " and code: " + Lobby.LobbyCode);
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

                Lobby = await LobbyService.Instance.JoinLobbyByCodeAsync(code, joinLobbyByCodeOptions);
                LobbyModel = GetLobbyModel();

                StartCoroutine(PollLobbyCoroutine());

                Debug.Log("Lobby joined with ID: " + Lobby.Id + " and code: " + Lobby.LobbyCode);

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
                var t = Task.Run(
                    async () => 
                    {
                        try
                        { 
                            return await LobbyService.Instance.GetLobbyAsync(Lobby.Id);
                        }
                        catch (Exception)
                        {
                            return null;
                        }
                    }
                );
                yield return new WaitUntil(() => t.IsCompleted);

                if (t.Result == null)
                {
                    OnDisconnect?.Invoke();
                    Reset();
                    yield break;
                }

                Lobby = t.Result;
                var model = GetLobbyModel();
                if (!model.Equals(LobbyModel))
                {
                    LobbyModel = model;
                    OnLobbyInvalidat?.Invoke();
                }

                yield return delay;
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
                            FirebaseAuth.DefaultInstance.CurrentUser?.DisplayName ?? "Name not found"
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

        public async Task<bool> JoinTeam(int teamNumber)
        {
            try 
            {
                var player = GetPlayerWithData();
                player.Data["TeamNumber"].Value = teamNumber.ToString();

                UpdatePlayerOptions updatePlayerOptions = new()
                {
                    Data = player.Data,
                };

                Lobby = await Lobbies.Instance.UpdatePlayerAsync(Lobby.Id, AuthenticationService.Instance.PlayerId, updatePlayerOptions);
                
                Debug.Log("Player " + AuthenticationService.Instance.PlayerId + " joined team " + teamNumber);

                return true;
            }
            catch (Exception e)
            {
                Debug.LogError("Failed to join team: " + e.Message);

                return false;
            }
        }

        public async Task<bool> ChangeTeamForPlayer(string playerId, int teamNumber)
        {
            try 
            {
                var player = Lobby.Players.Find(x => x.Id == playerId);
                player.Data["TeamNumber"].Value = teamNumber.ToString();

                UpdatePlayerOptions updatePlayerOptions = new()
                {
                    Data = player.Data,
                };

                await Lobbies.Instance.UpdatePlayerAsync(Lobby.Id, playerId, updatePlayerOptions);

                Debug.Log("Player " + playerId + " joined team " + teamNumber);

                return true;
            }
            catch (Exception e)
            {
                Debug.LogError("Failed to join team: " + e.Message);

                return false;
            }
        }

        public LobbyModel GetLobbyModel()
        {
            return new LobbyModel
            {
                Id = Lobby.Id,
                LobbyCode = Lobby.LobbyCode,
                MapId = Lobby.Data["MapId"].Value,
                HostId = Lobby.HostId,
                Name = Lobby.Name,
                MaxPlayers = Lobby.MaxPlayers,
                Teams = GetTeams()
            };
        }

        public Dictionary<string, int> GetTeams()
        {
            Dictionary<string, int> teams = new();

            foreach (var player in Lobby.Players)
            {
                if (player.Data.ContainsKey("TeamNumber"))
                {
                    teams.Add(player.Id, int.Parse(player.Data["TeamNumber"].Value));
                }
            }

            return teams;
        }

        public async Task DownloadMapData()
        {
            var mapId = _lobbyModel.MapId;
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
   
        public async Task<bool> RemovePlayerFromLobby(string playerId)
        {
            try
            {
                await Lobbies.Instance.RemovePlayerAsync(Lobby.Id, playerId);

                return true;
            }
            catch (Exception e)
            {
                Debug.LogError("Failed to remove player: " + e.Message);

                return false;
            }
        }

        public async Task<bool> LeaveLobby()
        {
            bool success = await RemovePlayerFromLobby(PlayerId);

            if (success)
            {
                Reset();
                return true;
            }

            return false;
        }

        private void Reset()
        {
            Lobby = null;
            LobbyModel = new LobbyModel();
            mapData = null;
            StopAllCoroutines();
        }
    }
}
