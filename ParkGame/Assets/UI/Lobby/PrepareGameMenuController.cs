using System;
using Managers;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UI.Lobby
{
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
            MapData mapData = mapPicker.GetCurrentMapData();
            string playerName = PlayerPrefs.GetString("PlayerName", "");
            
            bool success = await OurNetworkManager.Singleton.HostGame(mapData, playerName, -1);
            if (success)
            {
                PlayerPrefs.SetString("DebugRoomCode", SessionManager.Singleton.RoomCode);
                OurNetworkManager.Singleton.SceneManager.LoadScene(lobbySceneName, LoadSceneMode.Single);   
            }
            else
            {
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