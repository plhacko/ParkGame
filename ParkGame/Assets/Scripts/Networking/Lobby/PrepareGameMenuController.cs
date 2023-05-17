using System;
using System.Collections.Generic;
using System.Linq;
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
    public struct MapData : INetworkSerializable
    {
        public string Name;
        public TeamAllocationData[] Teams;
        public int GetMaxNumPlayers() => Teams.Sum(team => team.PlayerCountRange.Max);

        public void InitTeams()
        {
            for (int i = 0; i < Teams.Length; i++)
            {
                var team = Teams[i];
                team.TeamNumber = i;
                Teams[i] = team;
            }
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref Name);
            
            int length = 0;
            if (!serializer.IsReader)
            {
                length = Teams.Length;
            }

            serializer.SerializeValue(ref length);

            // Array
            if (serializer.IsReader)
            {
                Teams = new TeamAllocationData[length];
            }

            for (int i = 0; i < length; ++i)
            {
                serializer.SerializeValue(ref Teams[i]);
            }
        }
    }
    
    [Serializable]
    public struct Range : INetworkSerializeByMemcpy
    {
        public int Min;
        public int Max;
    }
    
    [Serializable]
    public struct TeamAllocationData : INetworkSerializeByMemcpy
    {
        public Range PlayerCountRange;
        [HideInInspector] public int TeamNumber;
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
            for (int i = 0; i < maps.Count; i++)
            {
                maps[i].InitTeams();
            }
        
            createButton.onClick.AddListener(createGame);
            backButton.onClick.AddListener(backJoinGameScene);
        }

        private void backJoinGameScene()
        {
            setInteractable(false);
            OurNetworkManager.Singleton.Shutdown();
            Destroy(OurNetworkManager.Singleton.gameObject);
            SceneManager.LoadScene(joinGameSceneName, LoadSceneMode.Single);
        }

        private async void createGame()
        {
            setInteractable(false);
            try
            {
                MapData mapData = maps[0];
                
                Allocation allocation = await RelayService.Instance.CreateAllocationAsync(mapData.GetMaxNumPlayers());
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