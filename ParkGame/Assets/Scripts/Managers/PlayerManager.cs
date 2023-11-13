using System;
using System.Collections.Generic;
using Player;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Managers
{
    /*
     * This class is responsible for managing the players in the game.
     * It keeps track of which player controllers belongs to a given player ID.
     * It also spawns players.
     */
    public class PlayerManager : MonoBehaviour
    {
        [SerializeField] private PlayerController playerControllerPrefab;

        public event Action OnAllPlayersSceneLoaded = null;
        
        // Only on the host
        // Mapping from player's firebase ID to player controller
        private readonly Dictionary<string, PlayerController> playerControllers = new();

        private int numClientsWithLoadedScene;

        private void Awake()
        {
            if(NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.SceneManager.OnLoadComplete += sceneLoaded;
            }
        }

        private void OnDestroy()
        {
            if (NetworkManager.Singleton)
            {
                NetworkManager.Singleton.SceneManager.OnLoadComplete -= sceneLoaded;   
            }
        }

        private void sceneLoaded(ulong clientId, string sceneName, LoadSceneMode loadSceneMode)
        {
            Debug.Log($"scene {sceneName} loaded for {clientId}");

            if (!NetworkManager.Singleton.IsHost) return;
            
            numClientsWithLoadedScene++;
            if (numClientsWithLoadedScene == NetworkManager.Singleton.ConnectedClients.Count)
            {
                Debug.Log("All players scene loaded");
                OnAllPlayersSceneLoaded.Invoke();
                spawnPlayers();
            }
        }

        private void spawnPlayers()
        {
            foreach (var clientId in NetworkManager.Singleton.ConnectedClients.Keys)
            {
                spawnPlayer(clientId);
            }   
        }

        public PlayerController GetPlayerController(ulong clientId)
        {
            var playerData = LobbyManager.Singleton.GetPlayerData(clientId);
            return playerControllers[playerData.FirebaseId];
        }

        private void spawnPlayer(ulong clientId)
        {
            PlayerData clientData = LobbyManager.Singleton.GetPlayerData(clientId);
            
            Debug.Log($"Spawning player name: {clientData.Name} team: {clientData.Team} id: {clientData.FirebaseId}");
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
