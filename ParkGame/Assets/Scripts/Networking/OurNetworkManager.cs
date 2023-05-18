using System;
using Networking.Lobby;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace Networking
{
    
    public struct PlayerData : INetworkSerializable
    {
         public ForceNetworkSerializeByMemcpy<FixedString64Bytes> Name;
         public int Team;
         public Guid ID;
         
         public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
         {
             serializer.SerializeValue(ref Name);
             serializer.SerializeValue(ref Team);

             if (serializer.IsWriter)
             {
                 byte[] idBytes = ID.ToByteArray();
                 serializer.SerializeValue(ref idBytes);
             }

             if (serializer.IsReader)
             {
                 byte[] idBytes = new byte[16];
                 serializer.SerializeValue(ref idBytes);
                 ID = new Guid(idBytes);
             }
         }
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

        private void OnConnectionApproval(ConnectionApprovalRequest request, ConnectionApprovalResponse response)
        {
            var clientId = request.ClientNetworkId;
            
            response.CreatePlayerObject = false;
            
            // host started hosting            
            if(clientId == LocalClientId)
            {
                SessionManager.Singleton.InitializeHost();
                response.Approved = true;
                return;
            }
            
            byte[] guidBytes = new byte[16];
            byte[] nameBytes = new byte[request.Payload.Length - 16];
            
            Array.Copy(request.Payload, 0, guidBytes, 0, 16);
            Array.Copy(request.Payload, 16, nameBytes, 0, request.Payload.Length - 16);
            
            Guid playerId = new Guid(guidBytes);
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
                string playerName = System.Text.Encoding.ASCII.GetString(nameBytes);
                
                SessionManager.Singleton.SetPlayerId(clientId, clientGuid);
                SessionManager.Singleton.UpdatePlayerData(new PlayerData
                {
                    ID = clientGuid,
                    Name = new ForceNetworkSerializeByMemcpy<FixedString64Bytes>(playerName),
                    Team = -1
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
            
            if (clientId == LocalClientId) return;
            
            SessionManager.Singleton.SendMapDataClientRpc(MapData, OneClientRpcParams(clientId));

            PlayerData? playerData = SessionManager.Singleton.GetPlayerData(clientId);
            if(playerData.HasValue)
            {
                Debug.Log($"Sending player data to {clientId} {playerData.Value.Name}");
                SessionManager.Singleton.SetPlayerDataClientRpc(clientId, playerData.Value, OneClientRpcParams(clientId));
                
                foreach (var idPair in SessionManager.Singleton.ClientIdToPlayerId)
                {
                    if(clientId == idPair.Key) continue;
                    
                    if(SessionManager.Singleton.ClientData.TryGetValue(idPair.Value, out var otherPlayerData))
                    {
                        Debug.Log($"Sending player data to {clientId} {otherPlayerData.Name}");
                        SessionManager.Singleton.SetPlayerDataClientRpc(idPair.Key, otherPlayerData, OneClientRpcParams(clientId));   
                    }
                }
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

        public static ClientRpcParams OneClientRpcParams(ulong clientId)
        {
            return new ClientRpcParams
            {
                Send = new ClientRpcSendParams { TargetClientIds = new [] { clientId } }
            };
        }
    }
}