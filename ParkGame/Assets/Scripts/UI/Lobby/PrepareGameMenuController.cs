using System;
using System.Collections.Generic;
using Managers;
using Unity.Collections;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UI.Lobby
{
    /*
     * This struct represents meta data for a map.
     * It contains the name of the map and the number of teams it supports.
     */
    [Serializable]
    public struct MapMetaData : INetworkSerializeByMemcpy
    {
        // Netcode doesn't doesn't support normal strings so we need to use the type FixedString64Bytes
        public FixedString64Bytes Name;
        
        // Number of teams the map supports
        public int NumTeams;
        
        public MapMetaData(MapMetaDataUI mapMetaDataUI)
        {
            Name = mapMetaDataUI.Name;
            NumTeams = mapMetaDataUI.NumTeams;
        }
    }
    
    /*
     * It's the same as MapMetaData but with a string instead of FixedString64Bytes
     * We need it because FixedString64Bytes doesn't serialize well in the unity editor
     */
    [Serializable]
    public struct MapMetaDataUI
    {
        public string Name;
        public int NumTeams;
    }

    /*
     * This class is responsible for the UI where the host prepares the game - chooses a map to play
     */
    public class PrepareGameMenuController : MonoBehaviour
    {
        [SerializeField] private string lobbySceneName;
        [SerializeField] private string joinGameSceneName;
        [SerializeField] private Button backButton;
        [SerializeField] private Button createButton;

        [SerializeField] private List<MapMetaDataUI> maps = new();
    
        void Awake()
        {
            createButton.onClick.AddListener(createGame);
            backButton.onClick.AddListener(backJoinGameScene);
        }

        // Go back to he join game scene
        // Shutdown the network manager and load the join game scene
        private void backJoinGameScene()
        {
            setInteractable(false);
            OurNetworkManager.Singleton.Shutdown();
            SceneManager.LoadScene(joinGameSceneName, LoadSceneMode.Single);
        }

        // Start hosting a new game
        private async void createGame()
        {
            setInteractable(false);
            try
            {
                MapMetaData mapMetaData = new MapMetaData(maps[0]);
                
                Allocation allocation = await RelayService.Instance.CreateAllocationAsync(mapMetaData.NumTeams * 4);
                string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
                
                string playerName = PlayerPrefs.GetString("PlayerName", "");
                
                SessionManager.Singleton.InitializeSession(playerName, mapMetaData, joinCode);
                OurNetworkManager.Singleton.GetComponent<UnityTransport>().SetHostRelayData(
                    allocation.RelayServer.IpV4,
                    (ushort)allocation.RelayServer.Port,
                    allocation.AllocationIdBytes,
                    allocation.Key,
                    allocation.ConnectionData);
            
                OurNetworkManager.Singleton.StartHost();
                OurNetworkManager.Singleton.SceneManager.LoadScene(lobbySceneName, LoadSceneMode.Single);
            }
            catch (RelayServiceException e)
            {
                Debug.LogWarning(e);
                setInteractable(true);
            }
        }

        private void setInteractable(bool interactable)
        {
            backButton.interactable = interactable;
            createButton.interactable = interactable;
        }
    }
}