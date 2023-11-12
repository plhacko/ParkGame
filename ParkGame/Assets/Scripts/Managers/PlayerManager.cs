using System;
using System.Collections.Generic;
using Player;
using Unity.Netcode;
using UnityEngine;

namespace Managers
{
    /*
     * This class is responsible for managing the players in the game.
     * It keeps track of which player controllers belongs to a given player ID.
     * It also spawns players and handles the reconnection of a player. 
     */
    public class PlayerManager : NetworkBehaviour
    {
        
        [SerializeField] private PlayerController playerControllerPrefab;

        // Invoked when a player reconnects to a live game
        // public event Action OnClientReconnectedCallback;
        
        // Only on the host
        // Mapping from player's firebase ID to player controller
        private readonly Dictionary<string, PlayerController> playerControllers = new();

        private bool isDebugging;
        
        private void Start()
        {
            isDebugging = !NetworkManager.Singleton.IsConnectedClient && !NetworkManager.Singleton.IsHost;
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            initialize();
        }
    
        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            NetworkManager.Singleton.OnClientConnectedCallback -= onClientConnect;
        }
        
        public PlayerController GetPlayerController(ulong clientId)
        {
            return null;
            // var playerId = SessionManager.Singleton.PlayersData.GetPlayerId(clientId);
            // if(!playerId.HasValue) return null;
            // if(!playerControllers.ContainsKey(playerId.Value)) return null;
            //
            // return playerControllers[playerId.Value];
        }

        // public async Task<bool> JoinGame(string joinCode, string playerName)
        // {
        //     try
        //     {
        //         JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
        //
        //         GetComponent<UnityTransport>().SetClientRelayData(
        //             joinAllocation.RelayServer.IpV4,
        //             (ushort)joinAllocation.RelayServer.Port,
        //             joinAllocation.AllocationIdBytes,
        //             joinAllocation.Key,
        //             joinAllocation.ConnectionData,
        //             joinAllocation.HostConnectionData);
        //
        //         var playerNameBytes = System.Text.Encoding.ASCII.GetBytes(playerName);
        //         
        //         // When we are joining the session for the first time we just send empty playerId
        //         // Otherwise we sent our cached playerId to reconnect into the game
        //         var localPlayerId = SessionManager.Singleton.LocalPlayerId.ToByteArray();
        //         var payload = localPlayerId.Concat(playerNameBytes).ToArray();
        //         
        //         NetworkConfig.ConnectionData = payload;
        //         StartClient();
        //         return true;
        //     }
        //     catch (Exception e)
        //     {
        //         Debug.LogWarning(e);
        //         return false;
        //     }
        // }

        // public PlayerData GetPlayerData(ulong clientId)
        // {
        //     LobbyManager.Singleton.LobbyModel.
        // }

        public void DebugSpawnPlayers()
        {
            // var clientIds = NetworkManager.Singleton.ConnectedClients.Keys.ToList();
            // var guids = SessionManager.Singleton.PlayersData.GuidToPlayerData.Keys.ToList();
            // for (int i = 0; i < clientIds.Count; i++)
            // {
            //     SessionManager.Singleton.PlayersData.UpdateClientId(clientIds[i], guids[i]);   
            // }
            //
            // foreach (var clientId in OurNetworkManager.Singleton.ConnectedClients.Keys)
            // {
            //     PlayerData? playerData = SessionManager.Singleton.PlayersData.GetPlayerData(clientId);
            //     if (playerData != null)
            //     {
            //         SessionManager.Singleton.SetPlayerDataClientRpc(clientId, playerData.Value, OurNetworkManager.OneClientRpcParams(clientId));
            //         spawnPlayer(clientId);   
            //     }
            //     else
            //     {
            //         Debug.LogWarning("Could not find player data for client " + clientId);
            //     }
            // }
        }

        private void spawnPlayer(ulong clientId)
        {
            PlayerData clientData = LobbyManager.Singleton.GetPlayerData(clientId);
            
            PlayerController playerController = Instantiate(playerControllerPrefab);
            playerController.InitializePlayer(clientData);
            
            // We spawn the player so that the client has ownership of it
            var networkObject = playerController.GetComponent<NetworkObject>();
            networkObject.SpawnWithOwnership(clientId, true);
            networkObject.DontDestroyWithOwner = true;

            playerControllers.Add(clientData.FirebaseId, playerController);
        }
        
        private void initialize()
        {
            if (!IsHost) return;
            
            // if(NetworkManager.Singleton.ServerState == ServerState.Debug) return;
        
            NetworkManager.Singleton.OnClientConnectedCallback += onClientConnect;
            foreach (var clientId in NetworkManager.Singleton.ConnectedClients.Keys)
            {
                spawnPlayer(clientId);
            }
        }

        // Called when a client connects to a live game
        // The connection should only be approved if the client is reconnecting
        private void onClientConnect(ulong clientId)
        {
            // if (isDebugging) return;
            //
            // var playerData = SessionManager.Singleton.PlayersData.GetPlayerData(clientId);
            // if (!playerData.HasValue) return;
            // if (!playerControllers.TryGetValue(playerData.Value.ID, out var playerController)) return;
            //
            // // The reconnected player will probably get a different clientId than before so we need to change the ownership
            // playerController.GetComponent<NetworkObject>().ChangeOwnership(clientId);
            // playerController.InitializePlayerIdClientRpc(new SerializedGuid(playerData.Value.ID), OurNetworkManager.OneClientRpcParams(clientId));
            //
            // // Send the client a message that the reconnection was successful
            // sendReconnectedSuccessClientRpc(OurNetworkManager.OneClientRpcParams(clientId));
        }

        // [ClientRpc]
        // private void sendReconnectedSuccessClientRpc(ClientRpcParams clientRpcParams)
        // {
        //     OnClientReconnectedCallback?.Invoke();
        // }
    }
}
