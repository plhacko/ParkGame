using System;
using System.Collections.Generic;
using System.Linq;
using Networking.Lobby;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace Networking
{

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

        public Guid LocalPlayerId => localPlayerId;
        public string LocalPlayerName => localPlayerName;

        public Dictionary<int, List<Guid>> GetTeams()
        {
            Dictionary<int, List<Guid>> teams = new();
            for (int i = 0; i < MaxNumTeams; i++)
            {
                teams.Add(i, new List<Guid>());
            }
            
            foreach (var (key, value) in ClientData)
            {
                if(value.Team == -1) continue;
                teams[value.Team].Add(key);
            }
            
            return teams;  
        } 

        private static SessionManager instance;
        
        public readonly Dictionary<ulong, Guid> ClientIdToPlayerId = new();
        public readonly Dictionary<Guid, PlayerData> ClientData = new();
        
        public event Action<PlayerData> OnSetPlayerData = null;
        public event Action<MapData> OnMapReceived = null;
        
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

        public void SetPlayerId(ulong clientId, Guid playerId)
        {
            if(OurNetworkManager.Singleton.LocalClientId == clientId)
                localPlayerId = playerId;
            
            if (ClientIdToPlayerId.ContainsKey(clientId))
            {
                ClientIdToPlayerId[clientId] = playerId;
                return;
            }
            
            ClientIdToPlayerId.Add(clientId, playerId);
        }

        public bool IsConnected(Guid playerId)
        {
            foreach (var id in ClientIdToPlayerId)
            {
                if (id.Value == playerId)
                {
                    return OurNetworkManager.Singleton.ConnectedClientsIds.Contains(id.Key);
                }
            }

            return false;
        }

        public void SetPlayerTeam(Guid playerId, int team)
        {
            if (ClientData.TryGetValue(playerId, out PlayerData data))
            {
                data.Team = team;
                ClientData[playerId] = data;
            }
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
        
        public void ClearData()
        {
            ClientIdToPlayerId.Clear();
            ClientData.Clear();
        }
    }
}