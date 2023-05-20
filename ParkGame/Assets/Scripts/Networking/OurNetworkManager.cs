using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Networking.Lobby;
using Unity.Collections;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

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

        public event Action<bool, ulong> OnClientDisconnect = null;
        
        public string RoomCode;
        public MapData MapData;
        
        private ServerState serverState;
        
        private void Awake()
        {
            OnClientConnectedCallback += OnClientConnected;
            OnClientDisconnectCallback += OnClientDisconnected;
            OnServerStarted += () => serverState = ServerState.Lobby;
            ConnectionApprovalCallback = OnConnectionApproval;
            
            serverState = ServerState.Lobby;
        }

        private void OnClientDisconnected(ulong clientId)
        {
            OnClientDisconnect?.Invoke(IsHost, clientId);
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

        private void OnClientConnected(ulong clientId)
        {
            if (!IsHost) return;
            
            if (clientId == LocalClientId) return;
            
            SessionManager.Singleton.SendMapDataClientRpc(MapData, OneClientRpcParams(clientId));

            PlayerData? playerData = SessionManager.Singleton.GetPlayerData(clientId);
            if(playerData.HasValue)
            {
                SessionManager.Singleton.SetPlayerDataClientRpc(clientId, playerData.Value, OneClientRpcParams(clientId));
                
                foreach (var idPair in SessionManager.Singleton.ClientIdToPlayerId)
                {
                    if(clientId == idPair.Key) continue;
                    
                    if(SessionManager.Singleton.ClientData.TryGetValue(idPair.Value, out var otherPlayerData))
                    {
                        SessionManager.Singleton.SetPlayerDataClientRpc(idPair.Key, otherPlayerData, OneClientRpcParams(clientId));   
                    }
                }
            }
            else
            {
                DisconnectClient(clientId);
            }
        }

        public static ClientRpcParams OneClientRpcParams(ulong clientId)
        {
            return new ClientRpcParams
            {
                Send = new ClientRpcSendParams { TargetClientIds = new [] { clientId } }
            };
        }
        
        public void LoadGame(string gameSceneName)
        {
            this.serverState = ServerState.InGame;

            var copyOfKeys = new List<ulong>(ConnectedClients.Keys);

            foreach (var clientId in copyOfKeys)
            {
                PlayerData? playerData = SessionManager.Singleton.GetPlayerData(clientId);
                if (!playerData.HasValue || playerData.Value.Team == -1)
                {
                    Guid? playerId = SessionManager.Singleton.GetPlayerId(clientId);
                    if (playerId.HasValue)
                    {
                        SessionManager.Singleton.RemovePlayerDataClientRpc(clientId);
                        SessionManager.Singleton.RemovePlayerData(playerId.Value);
                    }
                    DisconnectClient(clientId);
                }
            }
            
            SceneManager.LoadScene(gameSceneName, LoadSceneMode.Single);
        }

        public async Task<bool> JoinGame(string joinCode)
        {
            try
            {
                JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

                GetComponent<UnityTransport>().SetClientRelayData(
                    joinAllocation.RelayServer.IpV4,
                    (ushort)joinAllocation.RelayServer.Port,
                    joinAllocation.AllocationIdBytes,
                    joinAllocation.Key,
                    joinAllocation.ConnectionData,
                    joinAllocation.HostConnectionData);

                var name = System.Text.Encoding.ASCII.GetBytes(SessionManager.Singleton.LocalPlayerName);
                var localPlayerId = SessionManager.Singleton.LocalPlayerId.ToByteArray();
                var payload = localPlayerId.Concat(name).ToArray();
                
                NetworkConfig.ConnectionData = payload;
                StartClient();
                return true;
            }
            catch (Exception e)
            {
                Debug.LogWarning(e);
                return false;
            }
        }
    }
}