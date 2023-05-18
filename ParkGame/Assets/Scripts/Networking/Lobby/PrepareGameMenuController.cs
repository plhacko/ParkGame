using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Networking.Lobby
{
    [Serializable]
    public struct MapData : INetworkSerializeByMemcpy
    {
        public FixedString64Bytes Name;
        public int NumTeams;
    }

    public class PrepareGameMenuController : MonoBehaviour
    {
        [SerializeField] private string lobbySceneName;
        [SerializeField] private string joinGameSceneName;
        [SerializeField] private Button backButton;
        [SerializeField] private Button createButton;

        [SerializeField] private List<MapData> maps = new();
    
        void Awake()
        {
            createButton.onClick.AddListener(createGame);
            backButton.onClick.AddListener(backJoinGameScene);
        }

        private void backJoinGameScene()
        {
            setInteractable(false);
            OurNetworkManager.Singleton.Shutdown();
            SceneManager.LoadScene(joinGameSceneName, LoadSceneMode.Single);
        }

        private async void createGame()
        {
            setInteractable(false);
            try
            {
                MapData mapData = maps[0];
                
                Allocation allocation = await RelayService.Instance.CreateAllocationAsync(mapData.NumTeams * 4);
                string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            
                OurNetworkManager.Singleton.RoomCode = joinCode;
                OurNetworkManager.Singleton.MapData = mapData;
                
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