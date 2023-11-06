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
using Unity.Services.Relay.Models;
using Unity.Services.Relay;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using UnityEngine.SceneManagement;
using Unity.Netcode;

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
     
        public Action OnLobbyInvalidate;
        public Action OnDisconnect;
     
        public bool IsHost { get { return Lobby != null && Lobby.HostId == AuthenticationService.Instance.PlayerId; } }

        public Task UnityServicesInitializeTask { get; private set; }

        private ILobbyEvents events;

        private float heartbeatTimer;

        private string relayJoinCode = "";
        [SerializeField] private string GameScene = "GameScene";

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

        private void Update()
        {
            HandleLobbyHeartbeat();
        }

        private async void HandleLobbyHeartbeat()
        {
            if (Lobby == null || !IsHost) return;

            heartbeatTimer -= Time.deltaTime;
            if (heartbeatTimer < 0f)
            {
                float heartbeatTimerMax = 15f;
                heartbeatTimer = heartbeatTimerMax;

                Debug.Log("Heartbeat");
                await LobbyService.Instance.SendHeartbeatPingAsync(Lobby.Id);
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

                Allocation allocation = await CreateRelayAllocation();
                relayJoinCode = await GetRelayJoinCode(allocation);

                CreateLobbyOptions createLobbyOptions = new()
                {
                    IsPrivate = true,
                    Player = GetPlayerWithData(),
                    Data = GetLobbyData(),
                };

                Lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, createLobbyOptions);
                
                LobbyModel = GetLobbyModel();

                var callbacks = new LobbyEventCallbacks();
                callbacks.LobbyChanged += OnLobbyChanged;
                callbacks.KickedFromLobby += OnKickedFromLobby;
                callbacks.LobbyEventConnectionStateChanged += OnLobbyEventConnectionStateChanged;

                events = await LobbyService.Instance.SubscribeToLobbyEventsAsync(Lobby.Id, callbacks);

                PlayerPrefs.SetString("DebugRoomCode", Lobby.LobbyCode);

                Debug.Log("Lobby created with ID: " + Lobby.Id + " and code: " + Lobby.LobbyCode);

                NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(
                    new RelayServerData (allocation, "dtls")
                );

                NetworkManager.Singleton.StartHost();

                return true;
            } 
            catch (Exception e)
            {
                Debug.LogError("Failed to create lobby: " + e.Message);
                return false;
            }
        }

        private void OnLobbyChanged(ILobbyChanges changes)
        {
            Debug.Log("Lobby changed");

            if (changes.LobbyDeleted)
            {
                OnDisconnect?.Invoke();
                Reset();
            }
            else
            {
                changes.ApplyToLobby(Lobby);
                LobbyModel = GetLobbyModel();
                OnLobbyInvalidate?.Invoke();
            }
        }

        private void OnKickedFromLobby()
        {
            Debug.Log("Kicked from lobby");

            OnDisconnect?.Invoke();
            Reset();
        }

        private void OnLobbyEventConnectionStateChanged(LobbyEventConnectionState state)
        {
            Debug.Log("Lobby event connection state changed: " + state);
            switch (state)
            {
                case LobbyEventConnectionState.Unsubscribed: /* Update the UI if necessary, as the subscription has been stopped. */ break;
                case LobbyEventConnectionState.Subscribing: /* Update the UI if necessary, while waiting to be subscribed. */ break;
                case LobbyEventConnectionState.Subscribed: /* Update the UI if necessary, to show subscription is working. */ break;
                case LobbyEventConnectionState.Unsynced: /* Update the UI to show connection problems. Lobby will attempt to reconnect automatically. */ break;
                case LobbyEventConnectionState.Error: /* Update the UI to show the connection has errored. Lobby will not attempt to reconnect as something has gone wrong. */ break;
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

                var callbacks = new LobbyEventCallbacks();
                callbacks.LobbyChanged += OnLobbyChanged;
                callbacks.KickedFromLobby += OnKickedFromLobby;
                callbacks.LobbyEventConnectionStateChanged += OnLobbyEventConnectionStateChanged;

                events = await LobbyService.Instance.SubscribeToLobbyEventsAsync(Lobby.Id, callbacks);

                Debug.Log("Lobby joined with ID: " + Lobby.Id + " and code: " + Lobby.LobbyCode);

                var relayJoinCode = Lobby.Data["RelayJoinCode"].Value;
                var joinAllocation = await JoinRelay(relayJoinCode);
                NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(
                    new RelayServerData (joinAllocation, "dtls")
                );

                NetworkManager.Singleton.StartClient();

                return true;
            }
            catch (Exception e)
            {
                Debug.LogError("Failed to join lobby: " + e.Message);
                return false;
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
                    "RelayJoinCode",
                    new DataObject
                    (
                        DataObject.VisibilityOptions.Public,
                        relayJoinCode
                    )
                },
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
            events = null;
            Lobby = null;
            LobbyModel = new LobbyModel();
            mapData = null;
            StopAllCoroutines();
        }

        private async Task<Allocation> CreateRelayAllocation()
        {
            try {
                Allocation allocation = await RelayService.Instance.CreateAllocationAsync(MapData.MetaData.NumTeams * 4 - 1);
                return allocation;
            }
            catch (Exception e)
            {
                Debug.LogError("Failed to create relay allocation: " + e.Message);
                return null;
            }
        }

        private async Task<string> GetRelayJoinCode(Allocation allocation)
        {
            try
            {
                var joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
                return joinCode;
            }
            catch (Exception e)
            {
                Debug.LogError("Failed to get relay join code: " + e.Message);
                return null;
            }
        }

        private async Task<JoinAllocation> JoinRelay(string joinCode)
        {
            try
            {
                var joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
                return joinAllocation;
            }
            catch (Exception e)
            {
                Debug.LogError("Failed to join relay: " + e.Message);
                return null;
            }
        }

        public void StartGame()
        {
            NetworkManager.Singleton.SceneManager.LoadScene(GameScene, LoadSceneMode.Single);
        }
    }
}
