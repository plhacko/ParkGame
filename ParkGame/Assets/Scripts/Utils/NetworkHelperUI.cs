using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase;
using Firebase.Database;
using Firebase.Storage;
using Managers;
using Player;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

namespace Utils
{
    [Serializable]
    public class PlayerDataDebug
    {
        public int Team;
        public string Name;
    }
    
    /*
     * This class is a helper to create a game session for debugging without going through the menus.
     * It actually doesn't work well anymore because it should initialize the SessionManager with some debug player data
     */
    public class NetworkHelperUI : MonoBehaviour
    {
        [SerializeField] private string mapId;
        [SerializeField] private List<PlayerDataDebug> clients = new List<PlayerDataDebug>();

        private PlayerManager playerManager;
        private bool isDebugging;
        
        private async void Start()
        {
            playerManager = FindObjectOfType<PlayerManager>();
            isDebugging = !OurNetworkManager.Singleton.IsConnectedClient && !OurNetworkManager.Singleton.IsHost;
            
            if (isDebugging)
            {
                await UnityServices.InitializeAsync();
                await AuthenticationService.Instance.SignInAnonymouslyAsync();

                for (int i = 0; i < clients.Count; i++)
                {
                    var client = clients[i];
                    byte[] guid = new byte[16];
                    guid[i] = (byte)(i + 1);
                    
                    PlayerData clientPlayerData = new PlayerData
                    {
                        Name = client.Name + " " + i,
                        ID = new Guid(guid),
                        Team = client.Team
                    };
                    SessionManager.Singleton.PlayersData.UpdatePlayerData(clientPlayerData);   
                }
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        public void StartServer()
        {
            NetworkManager.Singleton.StartServer();
        }


        public void StartHost()
        {
            OurNetworkManager.Singleton.SetServerState(ServerState.Debug);
            SessionManager.Singleton.DownloadMapData(new Guid(mapId));
            SessionManager.Singleton.OnMapReceived += startHost;
        }

        private async void startHost(MapData mapData)
        {
            Debug.Log("Map received " + mapData.MetaData.MapName);
                
            bool success = await OurNetworkManager.Singleton.HostGame(mapData, clients[0].Name, clients[0].Team, false);
            
            if (success)
            {
                // SessionManager.Singleton.JoinTeam(clients[0].Team);
                PlayerPrefs.SetString("DebugRoomCode", SessionManager.Singleton.RoomCode);
            }
            else
            {
                Debug.Log("Failed to host game");
            }
            
            SessionManager.Singleton.OnMapReceived -= startHost;
            OurNetworkManager.Singleton.SetServerState(ServerState.Debug);
        }
        
        public async void StartClient()
        {
            string joinCode = PlayerPrefs.GetString("DebugRoomCode", "");
            bool success = await OurNetworkManager.Singleton.JoinGame(joinCode, "DebugClient");

            if (!success)
            {
                Debug.Log($"Failed to join game, join code: [{joinCode}]");
            }
        }

        public void SpawnPlayers()
        {
            playerManager.DebugSpawnPlayers();
        }
    }
}
