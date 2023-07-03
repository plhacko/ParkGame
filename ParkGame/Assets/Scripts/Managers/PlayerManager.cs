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
        public event Action OnClientReconnectedCallback;
        
        // Only on the host
        // Mapping from player's session ID to player controller
        private readonly Dictionary<Guid, PlayerController> playerControllers = new();
        
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            initialize();
        }
    
        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            OurNetworkManager.Singleton.OnClientConnectedCallback -= onClientConnect;
        }
        
        private void initialize()
        {
            if (!IsHost) return;
        
            OurNetworkManager.Singleton.OnClientConnectedCallback += onClientConnect;
        
            // Go through all connected clients and spawn a player controller for each of them
            foreach (var (clientId, _) in NetworkManager.Singleton.ConnectedClients)
            {
                PlayerData? playerData = SessionManager.Singleton.PlayersData.GetPlayerData(clientId);
                
                // Check just to make sure that the client has a player ID, but it should
                if (playerData == null) continue;
            
                PlayerController playerController = Instantiate(playerControllerPrefab);
                playerController.InitializePlayerId(playerData.Value.ID);
                
                // We spawn the player so that the client has ownership of it
                playerController.GetComponent<NetworkObject>().SpawnWithOwnership(clientId, true);
                playerController.GetComponent<NetworkObject>().DontDestroyWithOwner = true;
                playerController.InitializePlayerIdClientRpc(new SerializedGuid(playerData.Value.ID), OurNetworkManager.OneClientRpcParams(clientId));
            
                playerControllers.Add(playerData.Value.ID, playerController);
            }
        }

        // Called when a client connects to a live game
        // The connection should only be approved if the client is reconnecting
        private void onClientConnect(ulong clientId)
        {
            var clientData = SessionManager.Singleton.PlayersData.GetPlayerData(clientId);
            if (!clientData.HasValue) return;
            if (!playerControllers.TryGetValue(clientData.Value.ID, out var playerController)) return;
            
            // The reconnected player will probably get a different clientId than before so we need to change the ownership
            playerController.GetComponent<NetworkObject>().ChangeOwnership(clientId);
            
            // Send the client a message that the reconnection was successful
            sendReconnectedSuccessClientRpc(OurNetworkManager.OneClientRpcParams(clientId));
        }

        [ClientRpc]
        private void sendReconnectedSuccessClientRpc(ClientRpcParams clientRpcParams)
        {
            OnClientReconnectedCallback?.Invoke();
        }
    }
}
