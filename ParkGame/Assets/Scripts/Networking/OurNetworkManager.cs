using System;
using System.Collections.Generic;
using Networking.Lobby;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

namespace Networking
{
    
    public struct PlayerData : INetworkSerializeByMemcpy
    {
         public string Name;
         public Guid ID;
         public int Team;
    }
    
    public enum ServerState
    {
        Lobby,
        InGame
    }
    
    public class OurNetworkManager : NetworkManager
    {
        public new static OurNetworkManager Singleton => (OurNetworkManager)NetworkManager.Singleton;

        public string RoomCode;
        public MapData MapData;
        
        private ServerState serverState;
        
        private void Awake()
        {
            OnClientConnectedCallback += OnClientConnected;
            OnClientDisconnectCallback += OnClientDisconnected;
            ConnectionApprovalCallback = OnConnectionApproval;
            
            serverState = ServerState.Lobby;
        }

        // private void OnServerStartedHandler()
        // {
        //     serverState = ServerState.Lobby;
        //     
        //     Guid clientGuid = Guid.NewGuid();
        //     SessionManager.Singleton.SetPlayerId(LocalClientId, clientGuid);
        //     SessionManager.Singleton.SetPlayerData(new PlayerData
        //     {
        //         ID = clientGuid,
        //         Name = "",
        //         Team = 7
        //     });
        // }

        private void OnConnectionApproval(ConnectionApprovalRequest request, ConnectionApprovalResponse response)
        {
            var clientId = request.ClientNetworkId;
            
            response.CreatePlayerObject = false;
            
            // host started hosting            
            if(clientId == LocalClientId)
            {
                Guid clientGuid = Guid.NewGuid();
                SessionManager.Singleton.SetPlayerId(clientId, clientGuid);
                SessionManager.Singleton.SetPlayerData(new PlayerData
                {
                    ID = clientGuid,
                    Name = "",
                    Team = 7
                });
                
                response.Approved = true;
                return;
            }

            Guid playerId = new Guid(request.Payload);
            if (SessionManager.Singleton.IsConnected(playerId))
            {
                response.Approved = false;
                return;
            }
                
            PlayerData? playerData = SessionManager.Singleton.GetPlayerData(playerId);

            // reconnect
            if (playerData.HasValue)
            {
                SessionManager.Singleton.SetPlayerId(clientId, playerData.Value.ID);
                response.Approved = true;
                return;
            }
            
            // register new player
            if(serverState == ServerState.Lobby)
            {
                Guid clientGuid = Guid.NewGuid();
                SessionManager.Singleton.SetPlayerId(clientId, clientGuid);
                SessionManager.Singleton.SetPlayerData(new PlayerData
                {
                    ID = clientGuid,
                    Name = "",
                    Team = 7
                });
                
                response.Approved = true;
                return;
            }
            
            response.Approved = false;
        }

        private void OnClientDisconnected(ulong clientId)
        {
            // throw new System.NotImplementedException();
        }

        private void OnClientConnected(ulong clientId)
        {
            if (!IsHost) return;

            PlayerData? playerData = SessionManager.Singleton.GetPlayerData(clientId);
            if(playerData.HasValue)
            {
                SessionManager.Singleton.SetPlayerDataClientRpc(clientId, playerData.Value);
            }
            else
            {
                DisconnectClient(clientId);
            }

            // if(clientIDToPlayerId.ContainsValue())
            //
            // Guid clientGuid = Guid.NewGuid();
            //     
            // if (!clientIDToPlayerId.ContainsKey(clientId))
            // {
            //     clientIDToPlayerId.Add(clientId, clientGuid);    
            // }
            // else
            // {
            //     clientIDToPlayerId[clientId] = clientGuid;
            // }
            //
            // if(!clientData.TryGetValue(clientGuid, out var playerData))
            // {
            //     clientData.Add(clientGuid, new PlayerData()
            //     {
            //         ID = clientGuid,
            //         Name = "",
            //         Team = 7
            //     });
            //     SetPlayerDataClientRpc(clientId, playerData);
            //
            //     foreach (var client in clientIDToPlayerId)
            //     {
            //         SetPlayerDataClientRpc(client.Key, client.Value);   
            //     }
            // }
            // else
            // {
            //     // todo reconnect
            // }
        }
    }
}