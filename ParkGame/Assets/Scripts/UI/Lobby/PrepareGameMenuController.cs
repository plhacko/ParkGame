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
        [SerializeField] private MapPicker mapPicker;
    
        void Awake()
        {
            createButton.onClick.AddListener(createGame);
            backButton.onClick.AddListener(backJoinGameScene);
            setInteractable(false);
        }

        private void Update()
        {
            setInteractable(mapPicker.IsInitialized());
        }

        // Go back to the join game scene
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
                MapData mapData = mapPicker.GetCurrentMapData();
                Allocation allocation = await RelayService.Instance.CreateAllocationAsync(mapData.MetaData.NumTeams * 4);
                string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
                
                string playerName = PlayerPrefs.GetString("PlayerName", "");
                
                SessionManager.Singleton.InitializeSession(playerName, mapData, joinCode);
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
            createButton.interactable = interactable && mapPicker.MapDatas.Count > 0;
        }
    }
}