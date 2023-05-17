using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

namespace Networking
{

    public class SessionManager : NetworkBehaviour
    {
        public static SessionManager Singleton
        {
            get
            {
                if (instance != null) return instance;
                
                instance = FindObjectOfType<SessionManager>();

                if (instance != null) return instance;
                    
                GameObject singletonObject = new GameObject();
                instance = singletonObject.AddComponent<SessionManager>();
                DontDestroyOnLoad(singletonObject);

                return instance;
            }
        }

        public Guid LocalPlayerId => localPlayerId;
        
        private static SessionManager instance;

        private readonly Dictionary<ulong, Guid> clientIdToPlayerId = new();
        private readonly Dictionary<Guid, PlayerData> clientData = new();
        
        private Guid localPlayerId;
        
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
            if (clientIdToPlayerId.TryGetValue(clientId, out Guid playerId))
            {
                return playerId;
            }

            Debug.Log($"No client player ID found mapped to the given client ID: {clientId}");
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
            
            Debug.Log($"No client player ID found mapped to the given client ID: {clientId}");
            return null;
        }

        public PlayerData? GetPlayerData(Guid playerId)
        {
            if (clientData.TryGetValue(playerId, out PlayerData data))
            {
                return data;
            }
            
            Debug.Log($"No PlayerData of matching player ID found: {playerId}");
            return null;
        }
        
        [ClientRpc]
        public void SetPlayerDataClientRpc(ulong clientId, PlayerData playerData, ClientRpcParams clientRpcParams = default)
        {
            SetPlayerId(clientId, playerData.ID);
            SetPlayerData(playerData);
        }
        
        public void SetPlayerData(PlayerData playerData)
        {
            clientData[playerData.ID] = playerData;
        }

        public void SetPlayerId(ulong clientId, Guid playerId)
        {
            if(OurNetworkManager.Singleton.LocalClientId == clientId)
                localPlayerId = playerId;
            
            if (clientIdToPlayerId.ContainsKey(clientId))
            {
                clientIdToPlayerId[clientId] = playerId;
                return;
            }
            
            clientIdToPlayerId.Add(clientId, playerId);
        }

        public bool IsConnected(Guid playerId)
        {
            ulong clientId = ulong.MaxValue;
            
            foreach (var id in clientIdToPlayerId)
            {
                if (id.Value == playerId)
                {
                    clientId = id.Key;
                    break;
                }
            }
            
            return clientId != ulong.MaxValue && OurNetworkManager.Singleton.ConnectedClientsIds.Contains(clientId);
        }
    }
}