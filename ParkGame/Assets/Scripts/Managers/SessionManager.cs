using System;
using System.Collections.Generic;
using System.Linq;
using UI.Lobby;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Managers
{
    public class PlayersData
    {
        // Mapping from unity's client ID to a unique ID for the session
        public readonly Dictionary<ulong, Guid> ClientIdToPlayerId = new();
        
        // Mapping from player's session ID to player's data
        public readonly Dictionary<Guid, PlayerData> GuidToPlayerData = new();
        
        public PlayerData LocalPlayerData => localPlayerData;
        
        private PlayerData localPlayerData;
        
        public void UpdatePlayerData(PlayerData playerData)
        {
            if(playerData.ID == localPlayerData.ID)
                localPlayerData = playerData;
            
            GuidToPlayerData[playerData.ID] = playerData;
        }
        
        // Associates a client ID with a player ID
        public void UpdateClientId(ulong clientId, Guid playerId)
        {
            // If clientId is ours cache our player ID
            if(OurNetworkManager.Singleton.LocalClientId == clientId)
                localPlayerData.ID = playerId;

            // if ClientIdToPlayerId contains the player ID, remove the old entry
            ulong? previousClientId = GetClientId(playerId);
            if(previousClientId.HasValue)
                ClientIdToPlayerId.Remove(previousClientId.Value);

            // If the client ID is already in the dictionary, update the player ID
            if (ClientIdToPlayerId.ContainsKey(clientId))
            {
                ClientIdToPlayerId[clientId] = playerId;
                return;
            }
            
            // Otherwise add the new entry
            ClientIdToPlayerId.Add(clientId, playerId);
        }
        
        public void RemovePlayerData(Guid playerId)
        {
            ulong? clientId = GetClientId(playerId);
            if(clientId.HasValue)
                ClientIdToPlayerId.Remove(clientId.Value);
            
            GuidToPlayerData.Remove(playerId);
        }
        
        public ulong? GetClientId(Guid playerId)
        {
            if (ClientIdToPlayerId.ContainsValue(playerId))
            {
                return ClientIdToPlayerId.First(kv => kv.Value == playerId).Key;
            }

            return null;
        }
        
        public void ClearData()
        {
            ClientIdToPlayerId.Clear();
            GuidToPlayerData.Clear();
            localPlayerData = new PlayerData();
        }
        
        // Try to get the player's ID from the client ID
        public Guid? GetPlayerId(ulong clientId)
        {
            if (ClientIdToPlayerId.TryGetValue(clientId, out Guid playerId))
            {
                return playerId;
            }

            return null;
        }

        // Try to get player's data from client ID
        public PlayerData? GetPlayerData(ulong clientId)
        {
            Guid? playerId = GetPlayerId(clientId);
            if (playerId.HasValue)
            {
                return GetPlayerData(playerId.Value);
            }
            
            return null;
        }

        // Try to get player's data from player ID
        public PlayerData? GetPlayerData(Guid playerId)
        {
            if (GuidToPlayerData.TryGetValue(playerId, out PlayerData data))
            {
                return data;
            }
            
            return null;
        }

        // Get a list of all players in a team
        public List<PlayerData> GetTeam(int teamNumber)
        {
            var teams = GetTeams();
            if (teams.TryGetValue(teamNumber, out var team))
            {
                return team;
            }

            return null;
        }
        
        // Get all teams and their players
        public Dictionary<int, List<PlayerData>> GetTeams()
        {
            Dictionary<int, List<PlayerData>> teams = new();
            for (int i = 0; i < SessionManager.MaxNumTeams; i++)
            {
                teams.Add(i, new List<PlayerData>());
            }
            
            foreach (var (_, playerData) in GuidToPlayerData)
            {
                if(playerData.Team == -1) continue;
                teams[playerData.Team].Add(playerData);
            }
            
            return teams;  
        }
    }
    
    /*
     * This class is responsible for managing the state of the game session.
     * It is a singleton, keeps track of IDs and names all players in the game
     */
    public class SessionManager : NetworkBehaviour
    {
        [SerializeField] private string gameSceneName;
        
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
        public event Action<MapMetaData> OnMapReceived = null;
        public event Action<Guid, int, int> OnTeamJoined = null;
        
        public readonly PlayersData PlayersData = new();
        
        public Guid LocalPlayerId => PlayersData.LocalPlayerData.ID;
        public PlayerData LocalPlayerData => PlayersData.LocalPlayerData;
        public bool IsPlayerIdLocal(Guid playerId) => playerId == LocalPlayerId;
        public string RoomCode => roomCode;
        public MapMetaData MapMetaData => mapMetaData;
        public string GameSceneName => gameSceneName;
        
        private MapMetaData mapMetaData;
        private string roomCode;
        

        private void Awake()
        {
            if (instance == null)
            {
                PlayersData.ClearData();
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
            foreach (var (clientId, playerId)  in PlayersData.ClientIdToPlayerId)
            {
                Debug.Log($"---------- \n Client {clientId} \n playerID {playerId} \n team: {PlayersData.GuidToPlayerData[playerId].Team} \n name: {PlayersData.GuidToPlayerData[playerId].Name}");
            }
        }
        
        
        // Returns true if the player is currently connected
        // finds client ID from player ID and checks if it is connected
        public bool IsConnected(Guid playerId)
        {
            foreach (var (clientId, currentPlayerId) in PlayersData.ClientIdToPlayerId)
            {
                if (currentPlayerId == playerId)
                {
                    return OurNetworkManager.Singleton.ConnectedClientsIds.Contains(clientId);
                }
            }

            return false;
        }
        
        // Set player data for a client
        [ClientRpc]
        public void SetPlayerDataClientRpc(ulong clientId, PlayerData playerData, ClientRpcParams clientRpcParams = default)
        {
            if(IsHost) return;
            
            PlayersData.UpdateClientId(clientId, playerData.ID);
            PlayersData.UpdatePlayerData(playerData);

            OnSetPlayerData?.Invoke(playerData);
        }
        
        [ClientRpc]
        public void SendMapDataClientRpc(MapMetaData mapMetaData, ClientRpcParams clientRpcParams)
        {
            if (IsHost) return;

            OnMapReceived?.Invoke(mapMetaData);
        }
        
        // Message to delete data for a given clientId
        [ClientRpc]
        public void RemovePlayerDataClientRpc(ulong clientId, ClientRpcParams clientRpcParams = default)
        {
            if (IsHost) return;

            Guid? playerId = PlayersData.GetPlayerId(clientId);
            if (playerId.HasValue)
            {
                PlayersData.RemovePlayerData(playerId.Value);
            }
        }

        public void SetRoomCode(string roomCode)
        {
            this.roomCode = roomCode;
        }

        // Clears data of the current session, disconnects and loads a scene
        public void EndSessionAndGoToScene(string sceneName)
        {
            PlayersData.ClearData();
            OurNetworkManager.Singleton.Shutdown();
            SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
        }

        public void InitializeSession(string hostName, MapMetaData mapMetaData, string roomCode)
        {
            PlayersData.ClearData();
            PlayersData.UpdatePlayerData(new PlayerData
            {
                ID = Guid.NewGuid(),
                Name = hostName,
                Team = -1
            });
            this.roomCode = roomCode;
            this.mapMetaData = mapMetaData;
        }

        public bool IsTeamFull(int teamNumber)
        {
            var team = PlayersData.GetTeam(teamNumber);
            if (team == null) return false;
            
            return team.Count <= MaxNumPlayersPerTeam;
        }
        
        public void JoinTeam(int teamNumber)
        {
            if (IsTeamFull(teamNumber)) return;
            
            if (OurNetworkManager.Singleton.IsHost)
            {
                PlayerData hostData = PlayersData.LocalPlayerData;
                int oldTeam = hostData.Team;
                hostData.Team = teamNumber;
                
                PlayersData.UpdatePlayerData(hostData);
                OnTeamJoined.Invoke(LocalPlayerData.ID, oldTeam, teamNumber);
                joinTeamClientRpc(hostData.ID, oldTeam);
            }
            else
            {
                joinTeamServerRpc(LocalPlayerData.ID, teamNumber);
            }
        }

        [ServerRpc(RequireOwnership = false)]
        private void joinTeamServerRpc(Guid playerId, int newTeam, ServerRpcParams clientRpcParams = default)
        {
            if(newTeam < 0 || newTeam >= MapMetaData.NumTeams) return;

            var playerData = PlayersData.GetPlayerData(playerId);
            if(!playerData.HasValue) return;
            
            PlayerData data = playerData.Value;
            int oldTeam = data.Team;
            data.Team = newTeam;
            
            PlayersData.UpdatePlayerData(data);
            joinTeamClientRpc(data.ID, newTeam);
            OnTeamJoined.Invoke(playerId, oldTeam, newTeam);
        }

        [ClientRpc]
        private void joinTeamClientRpc(Guid playerId, int newTeamNumber, ClientRpcParams clientRpcParams = default)
        {
            if(OurNetworkManager.Singleton.IsHost) return;
            
            PlayerData? playerData = PlayersData.GetPlayerData(playerId);
            if(!playerData.HasValue) return;
            
            PlayerData data = playerData.Value;
            int oldTeam = data.Team;
            data.Team = newTeamNumber;

            PlayersData.UpdatePlayerData(data);
            OnTeamJoined.Invoke(playerId, oldTeam, newTeamNumber);
        }
        
        public void RemoveFromTeam(Guid playerId)
        {
            var playerData = PlayersData.GetPlayerData(playerId);
            if(!playerData.HasValue) return;
            
            if (OurNetworkManager.Singleton.IsHost)
            {
                var data = playerData.Value;
                data.Team = -1;
                PlayersData.UpdatePlayerData(data);
                removeFromTeamClientRpc(data.ID);
            }
            else
            {
                removeFromTeamServerRpc(playerData.Value.ID);
            }
        }
        
        [ServerRpc(RequireOwnership = false)]
        private void removeFromTeamServerRpc(Guid playerId, ServerRpcParams clientRpcParams = default)
        {
            var playerData = PlayersData.GetPlayerData(playerId);
            if(!playerData.HasValue) return;
            
            if(playerData.Value.Team == -1) return;
            
            var data = playerData.Value;
            int oldTeam = data.Team;
            data.Team = -1;
            
            PlayersData.UpdatePlayerData(data);
            removeFromTeamClientRpc(data.ID);
            OnTeamJoined.Invoke(playerId, oldTeam, data.Team);
        }

        [ClientRpc]
        private void removeFromTeamClientRpc(Guid playerId, ClientRpcParams clientRpcParams = default)
        {
            if(OurNetworkManager.Singleton.IsHost) return;
            
            var playerData = PlayersData.GetPlayerData(playerId);
            if(!playerData.HasValue) return;
            
            if(playerData.Value.Team == -1) return;
            
            var data = playerData.Value;
            int oldTeam = data.Team;
            data.Team = -1;
            
            PlayersData.UpdatePlayerData(data);
            OnTeamJoined.Invoke(playerId, oldTeam, data.Team);
        }
    }
}