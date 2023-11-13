using System.Collections.Generic;
using Player;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace Managers
{
    /*
     * This class is responsible for managing the players in the game.
     * It keeps track of which player controllers belongs to a given player ID.
     * It also spawns players.
     */
    public class PlayerManager : NetworkBehaviour
    {
        [SerializeField] private PlayerController playerControllerPrefab;

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

        public PlayerController GetPlayerController(ulong clientId)
        {
            var playerData = LobbyManager.Singleton.GetPlayerData(clientId);
            return playerControllers[playerData.FirebaseId];
        }

        private void spawnPlayers()
        {
            foreach (var clientId in NetworkManager.Singleton.ConnectedClients.Keys)
            {
                spawnPlayer(clientId);
            }

            foreach (var clientId in NetworkManager.Singleton.ConnectedClients.Keys)
            {
                initPlayer(clientId);
            }
        }

        private void spawnPlayer(ulong clientId)
        {
            PlayerData clientData = LobbyManager.Singleton.GetPlayerData(clientId);
            
            Debug.Log($"spawning {clientData.Name}, id {clientData.FirebaseId}, name {clientData.Name} ");
            PlayerController playerController = Instantiate(playerControllerPrefab);
            
            // We spawn the player so that the client has ownership of it
            var networkObject = playerController.GetComponent<NetworkObject>();
            networkObject.SpawnWithOwnership(clientId, true);
            networkObject.DontDestroyWithOwner = true;
            
            playerControllers.Add(clientData.FirebaseId, playerController);
        }
        
        private void initPlayer(ulong clientId)
        {
            PlayerData clientData = LobbyManager.Singleton.GetPlayerData(clientId);
            PlayerController playerController = playerControllers[clientData.FirebaseId];
            playerController.InitializePlayerClientRpc(new FixedString64Bytes(clientData.FirebaseId), oneClientRpcParams(clientId));
        }
        
        private void initialize()
        {
            if (!IsHost || isDebugging) return;

            spawnPlayers();
        }
        
        static ClientRpcParams oneClientRpcParams(ulong clientId) => 
            new()
            {
                Send = new ClientRpcSendParams { TargetClientIds = new [] { clientId } }
            };
    }
}
