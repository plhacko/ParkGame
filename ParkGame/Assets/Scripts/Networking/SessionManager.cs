using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Networking.Lobby;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Networking
{

    /*
     * This class is responsible for managing the state of the game session.
     * It is a singleton, keeps track of IDs and names all players in the game
     */
    public class SessionManager : NetworkBehaviour
    {
        public static int MaxNumTeams = 4;
        public static int MaxNumPlayersPerTeam = 3;

        public static SessionManager Singleton
        {
            get
            {
                if (instance != null) return instance;
                
                instance = FindObjectOfType<SessionManager>();
                
                return instance;
            }
        }
        private static SessionManager instance;

        // Called when a player's data is set
        public event Action<PlayerData> OnSetPlayerData = null;
        public event Action<MapData> OnMapReceived = null;
        
        public Guid LocalPlayerId => localPlayerId;
        public string LocalPlayerName => localPlayerName;
        public string GameSceneName => gameSceneName;
        
        // Mapping from unity's client ID to a unique ID for the session
        public readonly Dictionary<ulong, Guid> ClientIdToPlayerId = new();
        
        // Mapping from player's session ID to player's data
        public readonly Dictionary<Guid, PlayerData> ClientData = new();
        
        [SerializeField] private string gameSceneName;
        private Guid localPlayerId;
        private string localPlayerName;

        private void Awake()
        {
            if (instance == null)
            {
                localPlayerId = Guid.Empty;
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Update()
        {
            foreach (var (clientId, playerId)  in ClientIdToPlayerId)
            {
                Debug.Log($"Client {clientId} is player {playerId} in team: {ClientData[playerId].Team} with name: {ClientData[playerId].Name}");
            }
        }

        public Guid? GetPlayerId(ulong clientId)
        {
            if (ClientIdToPlayerId.TryGetValue(clientId, out Guid playerId))
            {
                return playerId;
            }

            return null;
        }

        public PlayerData? GetLocalPlayerData() => GetPlayerData(localPlayerId);

        public PlayerData? GetPlayerData(ulong clientId)
        {
            Guid? playerId = GetPlayerId(clientId);
            if (playerId.HasValue)
            {
                return GetPlayerData(playerId.Value);
            }
            
            return null;
        }

        public PlayerData? GetPlayerData(Guid playerId)
        {
            if (ClientData.TryGetValue(playerId, out PlayerData data))
            {
                return data;
            }
            
            return null;
        }

        public List<PlayerData> GetTeam(int teamNumber)
        {
            var teams = GetTeams();
            if (teams.TryGetValue(teamNumber, out var team))
            {
                return team;
            }

            return null;
        }
        
        public Dictionary<int, List<PlayerData>> GetTeams()
        {
            Dictionary<int, List<PlayerData>> teams = new();
            for (int i = 0; i < MaxNumTeams; i++)
            {
                teams.Add(i, new List<PlayerData>());
            }
            
            foreach (var (_, playerData) in ClientData)
            {
                if(playerData.Team == -1) continue;
                teams[playerData.Team].Add(playerData);
            }
            
            return teams;  
        } 
        
        [ClientRpc]
        public void SetPlayerDataClientRpc(ulong clientId, PlayerData playerData, ClientRpcParams clientRpcParams = default)
        {
            if(IsHost) return;
            
            SetPlayerId(clientId, playerData.ID);
            UpdatePlayerData(playerData);

            OnSetPlayerData?.Invoke(playerData);
        }
        
        public void UpdatePlayerData(PlayerData playerData)
        {
            ClientData[playerData.ID] = playerData;
        }
        
        public void SetName(string name)
        {
            localPlayerName = name;
        }

        // Associates a client ID with a player ID
        public void SetPlayerId(ulong clientId, Guid playerId)
        {
            // If clientId is ours cache our player ID
            if(OurNetworkManager.Singleton.LocalClientId == clientId)
                localPlayerId = playerId;
            
            // If the client ID is already in the dictionary, update the player ID
            if (ClientIdToPlayerId.ContainsKey(clientId))
            {
                ClientIdToPlayerId[clientId] = playerId;
                return;
            }
            
            // Otherwise add the new entry
            ClientIdToPlayerId.Add(clientId, playerId);
        }

        // Returns true if the player is currently connected
        // finds client ID from player ID and checks if it is connected
        public bool IsConnected(Guid playerId)
        {
            foreach (var (clientId, currentPlayerId) in ClientIdToPlayerId)
            {
                if (currentPlayerId == playerId)
                {
                    return OurNetworkManager.Singleton.ConnectedClientsIds.Contains(clientId);
                }
            }

            return false;
        }

        [ClientRpc]
        public void SendMapDataClientRpc(MapData mapData, ClientRpcParams clientRpcParams)
        {
            if (IsHost) return;

            OnMapReceived?.Invoke(mapData);
        }

        public void InitializeHost()
        {
            Guid clientGuid = Guid.NewGuid();
            SetPlayerId(OwnerClientId, clientGuid);
            UpdatePlayerData(new PlayerData
            {
                ID = clientGuid,
                Name = new ForceNetworkSerializeByMemcpy<FixedString64Bytes>(LocalPlayerName),
                Team = -1
            });
        }

        public void EndSessionAndGoToScene(string sceneName)
        {
            clearData();
            OurNetworkManager.Singleton.Shutdown();
            SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
        }

        private void clearData()
        {
            ClientIdToPlayerId.Clear();
            ClientData.Clear();
        }

        public void RemovePlayerData(Guid playerId)
        {
            ClientIdToPlayerId.Remove(ClientIdToPlayerId.First(x => x.Value == playerId).Key);
            ClientData.Remove(playerId);
        }

        [ClientRpc]
        public void RemovePlayerDataClientRpc(ulong clientId, ClientRpcParams clientRpcParams = default)
        {
            if (IsHost) return;

            Guid? playerId = GetPlayerId(clientId);
            if (playerId.HasValue)
            {
                RemovePlayerData(playerId.Value);
            }
        }
    }
}