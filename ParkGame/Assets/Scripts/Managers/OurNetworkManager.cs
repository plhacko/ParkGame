using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Managers
{
    public struct SerializedGuid : INetworkSerializable
    {
        public Guid Value;
        
        public SerializedGuid(Guid value)
        {
            Value = value;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            if (serializer.IsWriter)
            {
                byte[] idBytes = Value.ToByteArray();
                serializer.SerializeValue(ref idBytes);
            }

            if (serializer.IsReader)
            {
                byte[] idBytes = new byte[16];
                serializer.SerializeValue(ref idBytes);
                Value = new Guid(idBytes);
            }
        }
    }
    
    [Serializable]
    public struct PlayerData : INetworkSerializable
    {
        public int Team;
        public Guid ID;

        public string Name
        { 
            get => name.Value.Value;
            set => name.Value = value;
        }
        
        private ForceNetworkSerializeByMemcpy<FixedString64Bytes> name;
        
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref name);
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
        InGame,
        Debug
    }
    
    public class OurNetworkManager : NetworkManager
    {
        public new static OurNetworkManager Singleton => (OurNetworkManager)NetworkManager.Singleton;
        
        public event Action<bool, ulong> OnClientDisconnect = null;

        public ServerState ServerState => serverState;
        private ServerState serverState;
        
        private void Awake()
        {
            if(NetworkManager.Singleton != null && NetworkManager.Singleton != this)
            {
                Destroy(gameObject);
                return;
            }

            OnClientConnectedCallback += OnClientConnected;
            OnClientDisconnectCallback += clientId => OnClientDisconnect?.Invoke(IsHost, clientId);
            OnServerStarted += () => serverState = ServerState.Lobby;
            ConnectionApprovalCallback = OnConnectionApproval;
            serverState = ServerState.Lobby;
        }

        // Called when a client requests connection to the host
        // The request is created when the client calls JoinGame()
        // and it contains a player's name and a unique player ID if the player is reconnecting
        private void OnConnectionApproval(ConnectionApprovalRequest request, ConnectionApprovalResponse response)
        {
            // Debug - just approve anything
            if (serverState == ServerState.Debug)
            {
                response.Approved = true;
                return;
            }
            
            var clientId = request.ClientNetworkId;
            
            // We don't want to create a player object immediately 
            response.CreatePlayerObject = false;
            
            // Host started hosting, initialize host and approve ourselves            
            if(clientId == LocalClientId)
            {
                response.Approved = true;
                return;
            }
            
            byte[] guidBytes = new byte[16];
            byte[] nameBytes = new byte[request.Payload.Length - 16];
            
            // Load name and ID from the request
            Array.Copy(request.Payload, 0, guidBytes, 0, 16);
            Array.Copy(request.Payload, 16, nameBytes, 0, request.Payload.Length - 16);
            
            // Check if player is already connected, should not happen
            Guid playerId = new Guid(guidBytes);
            if (SessionManager.Singleton.IsConnected(playerId))
            {
                response.Approved = false;
                return;
            }
                
            // Try to get player data from the session manager
            PlayerData? playerData = SessionManager.Singleton.PlayersData.GetPlayerData(playerId);

            // Reconnect if data exists, we can reconnect into lobby or game
            // Approve the connection and associate the client's ID with the player's ID
            if (playerData.HasValue)
            {
                SessionManager.Singleton.PlayersData.UpdateClientId(clientId, playerData.Value.ID);
                response.Approved = true;
                return;
            }
            
            // If the player is trying to connect but we aren't in lobby reject
            if (serverState != ServerState.Lobby)
            {
                response.Approved = false;
                return;
            }

            // Create new player data and approve the connection
            Guid clientGuid = Guid.NewGuid();
            string playerName = System.Text.Encoding.ASCII.GetString(nameBytes);
            response.Approved = true;
            
            // Save the player's data on the host's session manager
            // In OnClientConnected we will send the player data to the client
            SessionManager.Singleton.PlayersData.UpdateClientId(clientId, clientGuid);
            SessionManager.Singleton.PlayersData.UpdatePlayerData(new PlayerData
            {
                ID = clientGuid,
                Name = playerName,
                Team = -1
            });
        }

        // When a client connects to the host
        // Send the map data and the player data to the client
        // Send the player data of all other players to the client
        // Send the player data of the client to all other players
        private void OnClientConnected(ulong clientId)
        {
            if (!IsHost) return;
            if (clientId == LocalClientId) return;

            if (serverState == ServerState.Debug)
            {
                SessionManager.Singleton.SendMapIdClientRpc(new SerializedGuid(Guid.Parse(SessionManager.Singleton.MapData.MetaData.MapId)), OneClientRpcParams(clientId));
                return;
            }
            
            // Try to get the player data of the client
            PlayerData? playerData = SessionManager.Singleton.PlayersData.GetPlayerData(clientId);
            if(playerData.HasValue)
            {
                // Send all the players the player data of the connected client
                SessionManager.Singleton.SetPlayerDataClientRpc(clientId, playerData.Value);
                
                // Send the player data of all other players to the client
                foreach (var (currentClientId, currentPlayerId) in SessionManager.Singleton.PlayersData.ClientIdToPlayerId)
                {
                    // dont send the player his own data, we just sent it 
                    if(clientId == currentClientId) continue;

                    PlayerData? otherPlayerData = SessionManager.Singleton.PlayersData.GetPlayerData(currentPlayerId);
                    
                    if(otherPlayerData.HasValue)
                    {
                        // Sent client other player's data
                        SessionManager.Singleton.SetPlayerDataClientRpc(currentClientId, otherPlayerData.Value, OneClientRpcParams(clientId));   
                    }
                }
                
                // Send map data to the client
                SessionManager.Singleton.SendMapIdClientRpc(new SerializedGuid(Guid.Parse(SessionManager.Singleton.MapData.MetaData.MapId)), OneClientRpcParams(clientId));
            }
            else
            {
                // Disconnect the client if we couldn't get his player data
                DisconnectClient(clientId);
            }
        }
        
        // Load game scene and disconnect all clients that don't have a team assigned
        public void LoadGameScene()
        {
            if (!IsHost) return;
            if (serverState != ServerState.Lobby) return;
            
            this.serverState = ServerState.InGame;

            // Kick all players that don't have a team assigned
            var copyOfKeys = new List<ulong>(SessionManager.Singleton.PlayersData.ClientIdToPlayerId.Keys);
            foreach (var clientId in copyOfKeys)
            {
                PlayerData? playerData = SessionManager.Singleton.PlayersData.GetPlayerData(clientId);
                if (!playerData.HasValue || playerData.Value.Team == -1)
                {
                    Guid? playerId = SessionManager.Singleton.PlayersData.GetPlayerId(clientId);
                    if (playerId.HasValue)
                    {
                        SessionManager.Singleton.RemovePlayerDataClientRpc(clientId);
                        SessionManager.Singleton.PlayersData.RemovePlayerData(playerId.Value);
                    }

                    if (ConnectedClients.ContainsKey(clientId))
                    {
                        DisconnectClient(clientId);   
                    }
                }
            }
            
            // Load game scene
            SceneManager.LoadScene(SessionManager.Singleton.GameSceneName, LoadSceneMode.Single);
        }
        
        public void SetServerState(ServerState serverState)
        {
            this.serverState = serverState;
        }

        public async Task<bool> HostGame(MapData mapData, string playerName, int teamNumber, bool resetData = true)
        {
            try
            {
                Allocation allocation = await RelayService.Instance.CreateAllocationAsync(mapData.MetaData.NumTeams * 4);
                string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

                SessionManager.Singleton.InitializeSession(playerName, teamNumber, mapData, joinCode, resetData);
                
                GetComponent<UnityTransport>().SetHostRelayData(
                    allocation.RelayServer.IpV4,
                    (ushort)allocation.RelayServer.Port,
                    allocation.AllocationIdBytes,
                    allocation.Key,
                    allocation.ConnectionData);
            
                Singleton.StartHost();
                return true;
            }
            catch (RelayServiceException e)
            {
                Debug.LogWarning(e);
                return false;
            }
        }

        // Join a game with a join code
        // The join code is generated by the host when the host calls StartHosting()
        // Clients can join into lobby or reconnect into a previously abandoned game
        public async Task<bool> JoinGame(string joinCode, string playerName)
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

                var playerNameBytes = System.Text.Encoding.ASCII.GetBytes(playerName);
                
                // When we are joining the session for the first time we just send empty playerId
                // Otherwise we sent our cached playerId to reconnect into the game
                var localPlayerId = SessionManager.Singleton.LocalPlayerId.ToByteArray();
                var payload = localPlayerId.Concat(playerNameBytes).ToArray();
                
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
        
        // Helper to create ClientRpcParams targeting a single client id
        public static ClientRpcParams OneClientRpcParams(ulong clientId) => 
            new()
            {
                Send = new ClientRpcSendParams { TargetClientIds = new [] { clientId } }
            };
    }
}