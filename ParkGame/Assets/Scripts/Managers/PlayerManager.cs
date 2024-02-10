using System;
using System.Collections;
using System.Collections.Generic;
using Player;
using Unity.Mathematics;
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
    public class PlayerManager : NetworkBehaviour
    {
        [SerializeField] private PlayerController playerControllerPrefab;
        [SerializeField] private float MinInitialDistanceFromOutpost = 5f;
        [SerializeField] private int unitCapacity = 12;
        
        private Map map;
        private Announcer announcer;
        private PlayerController localPlayerController;
        
        public event Action OnAllPlayersSceneLoaded = null;
        public event Action OnAllPlayersReady = null;
        
        // Only on the host
        // Mapping from player's firebase ID to player controller
        private readonly Dictionary<string, PlayerController> playerControllers = new();

        private int numClientsWithLoadedScene;
        private bool playersLocked = true; 
        private bool isSceneLoaded = false;
        private PlayerPointerPlacer playerPointerPlacer;

        private Dictionary<int, List<Transform>> unitsInTeam = new Dictionary<int, List<Transform>>(); 

        private void Awake()
        {
            announcer = FindObjectOfType<Announcer>();
         
            map = FindObjectOfType<Map>();
            map.OnMapLoaded += onMapLoaded;
            
            if(NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.SceneManager.OnLoadComplete += sceneLoaded;
            }

            for (int i = 0; i < 4; i++) {
                unitsInTeam[i] = new List<Transform>();
            }
        }

        private void Update()
        {
            if(!isSceneLoaded || localPlayerController == null || !localPlayerController.IsLocked) return;
            
            Outpost[] outposts = FindObjectsOfType<Outpost>();
            bool isLocalClose = isCloseToItsCastle(localPlayerController, outposts);
            playerPointerPlacer.SetReadyColor(isLocalClose);
            
            if (!NetworkManager.Singleton.IsServer) return;
            if (numClientsWithLoadedScene != NetworkManager.Singleton.ConnectedClients.Count || !playersLocked) return;

            bool allCloseToOutposts = true;
                
            foreach (var (_, controller) in playerControllers)
            {
                allCloseToOutposts = allCloseToOutposts && isCloseToItsCastle(controller, outposts);
            }

            if (!allCloseToOutposts) return;
            playersLocked = false;
            
            StartCoroutine(CountDownAndStart());
        }
        
        IEnumerator CountDownAndStart()
        {
            PlaySFXNotificationClientRpc("CountToStart");
            announcer.AnnounceEventClientRpc("3", Color.white, 2);
            yield return new WaitForSeconds(2f);
            PlaySFXNotificationClientRpc("CountToStart");
            announcer.AnnounceEventClientRpc("2", Color.white, 2);
            yield return new WaitForSeconds(2f);
            PlaySFXNotificationClientRpc("CountToStart");
            announcer.AnnounceEventClientRpc("1", Color.white, 2);
            yield return new WaitForSeconds(2f);
            PlaySFXNotificationClientRpc("StartGame");
            announcer.AnnounceEventClientRpc("GO!", Color.white, 2);

            foreach (var (_, controller) in playerControllers) 
            {
                controller.IsLocked = false;
            }
            
            invokeOnAllPlayersReadyClientRpc();
        }


        [ClientRpc] 
        void PlaySFXNotificationClientRpc(string sfxName) 
        {
            // sounds starting the match
            AudioManager.Instance.PlayCommandSFX(sfxName);
        }

        [ClientRpc]
        private void invokeOnAllPlayersReadyClientRpc()
        {
            OnAllPlayersReady?.Invoke();
        }
        
        private bool isCloseToItsCastle(PlayerController controller, Outpost[] outposts)
        {
            bool closeToItsOutpost = false;
            
            foreach (var outpost in outposts)
            {
                if (!outpost.IsCastle || outpost.Team != controller.Team) continue;
                        
                if (Vector3.Distance(controller.PointerPosition, outpost.transform.position) < MinInitialDistanceFromOutpost)
                {
                    closeToItsOutpost = true;
                }
            }

            return closeToItsOutpost;
        }

        void onMapLoaded()
        {
            playerPointerPlacer = FindObjectOfType<PlayerPointerPlacer>();
            playerPointerPlacer.SetReadyColor(false);
            isSceneLoaded = true;   
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            if (NetworkManager.Singleton)
            {
                NetworkManager.Singleton.SceneManager.OnLoadComplete -= sceneLoaded;   
            }
            
            map.OnMapLoaded -= onMapLoaded;
        }

        private void sceneLoaded(ulong clientId, string sceneName, LoadSceneMode loadSceneMode)
        {
            Debug.Log($"scene {sceneName} loaded for {clientId}");
            
            if (!NetworkManager.Singleton.IsHost) return;
            
            numClientsWithLoadedScene++;
            if (numClientsWithLoadedScene == NetworkManager.Singleton.ConnectedClients.Count)
            {
                announcer.AnnounceEventClientRpc("Walk to your castles!", Color.white);
                Debug.Log("All players scene loaded");
                OnAllPlayersSceneLoaded?.Invoke();
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
            var playerData  = LobbyManager.Singleton.GetPlayerData(clientId);
            return playerControllers[playerData.FirebaseId];
        }

        private void spawnPlayer(ulong clientId)
        {
            PlayerData clientData = LobbyManager.Singleton.GetPlayerData(clientId);
            
            Outpost[] outposts = FindObjectsOfType<Outpost>();
            Vector3 spawnPosition = Vector3.zero;
            foreach (var outpost in outposts)
            {
                if (!outpost.IsCastle || outpost.Team != clientData.Team) continue;
                        
                spawnPosition = outpost.transform.position;
                break;
            }
            
            Debug.Log($"Spawning player name: {clientData.Name} team: {clientData.Team} id: {clientData.FirebaseId}");
            PlayerController playerController = Instantiate(playerControllerPrefab, spawnPosition, quaternion.identity);
            playerController.InitializePlayer(clientData);
            
            // We spawn the player so that the client has ownership of it
            var networkObject = playerController.GetComponent<NetworkObject>();
            networkObject.SpawnWithOwnership(clientId, true);
            networkObject.DontDestroyWithOwner = true;

            playerControllers.Add(clientData.FirebaseId, playerController);
        }

        public void SetLocalPlayerController(PlayerController playerController)
        {
            this.localPlayerController = playerController;
        }

        public PlayerController GetLocalPlayerController() => localPlayerController;
        
        public List<PlayerController> GetAllMembersOfTeam(int team)
        {
            List<PlayerController> teamMembers = new List<PlayerController>();
            foreach (var (_, controller) in playerControllers)
            {
                if (controller.Team == team)
                {
                    teamMembers.Add(controller);
                }
            }

            return teamMembers;
        }
        
        public List<PlayerController> GetAllEnemyMembers(int team)
        {
            List<PlayerController> teamMembers = new List<PlayerController>();
            foreach (var (_, controller) in playerControllers)
            {
                if (controller.Team != team)
                {
                    teamMembers.Add(controller);
                }
            }

            return teamMembers;
        }

        public void DisableAllPlayers()
        {
            foreach (var (_, controller) in playerControllers) 
            {
                controller.IsLocked = true;
            }
        }

        public bool CanAddSoldierToTeam(int team) {
            if (team < 0 || team > 3) {
                return false; 
            }
            if (unitsInTeam[team].Count >= unitCapacity) {
                return false;
            }
            return true;
        }

        public void AddSoldierToTeam(int team, Transform unit) {
            unitsInTeam[team].Add(unit);
        }

        public void RemoveSoldierFromTeam(int team, Transform unit) {
            if (team < 0 || team > 3) { return; }
            unitsInTeam[team]?.Remove(unit);
        }

    }
}
