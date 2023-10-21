using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Managers;
using UnityEngine.Events;

public class UIPrepareGameMenuController : MonoBehaviour
{
    [SerializeField] private string lobbySceneName;
    [SerializeField] private string joinGameSceneName;
    [SerializeField] private Button backButton;
    [SerializeField] private Button createButton;
    [SerializeField] private MapPicker mapPicker;
    [SerializeField] private UnityEvent onCreatedGame;

    void Start()
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
            // TODO push lobby page without loading scene
            //OurNetworkManager.Singleton.SceneManager.LoadScene(lobbySceneName, LoadSceneMode.Single); 
            onCreatedGame.Invoke();
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
