using System.Collections.Generic;
using Player;
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
        
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            if (IsHost)
            {
                spawnPlayers();   
            }
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
    }
}
