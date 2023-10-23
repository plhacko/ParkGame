using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Services.Core;
using Unity.Services.Authentication;
using UnityEngine.SceneManagement;
using Managers;
using UnityEngine.Events;

public class UIMainMenuController : MonoBehaviour
{
    [SerializeField] private string createMapMenuSceneName; 
    [SerializeField] private Button createMapButton;
    [SerializeField] private UnityEvent onCreateMapPressed;
    
    [SerializeField] private Button hostButton;
    [SerializeField] private UnityEvent onHostPressed;
    
    [SerializeField] private TMP_InputField joinCodeInputField;
    [SerializeField] private Button joinButton;
    [SerializeField] private UnityEvent onJoinPressed;
    
    private void Start()
    {
        joinButton.onClick.AddListener(joinGame);
        joinButton.onClick.AddListener(onJoinPressed.Invoke);

        hostButton.onClick.AddListener(onHostPressed.Invoke);
        
        createMapButton.onClick.AddListener(createMap);
        createMapButton.onClick.AddListener(onCreateMapPressed.Invoke);
        
        OurNetworkManager.Singleton.OnClientDisconnectCallback += onClientDisconnect;

        joinCodeInputField.text = SessionManager.Singleton.RoomCode != null
            ? SessionManager.Singleton.RoomCode
            : PlayerPrefs.GetString("DebugRoomCode", "");
    }

    private void OnDestroy()
    {
        if(OurNetworkManager.Singleton != null)
            OurNetworkManager.Singleton.OnClientDisconnectCallback -= onClientDisconnect;
    }
    
    private void onClientDisconnect(ulong clientId)
    {
        joinCodeInputField.text = "";
        enableButtons(true);
    }

    // Go to scene where the player can create a new map
    private void createMap()
    {
        SceneManager.LoadScene(createMapMenuSceneName, LoadSceneMode.Single);
    }

    // Join a game with the room code entered in the input field
    private async void joinGame()
    {
        // enableButtons(false);
        
        // string playerName = PlayerPrefs.GetString("PlayerName", "");
        // bool joined = await OurNetworkManager.Singleton.JoinGame(joinCodeInputField.text, playerName);
        // if (joined)
        // {
        //     // Save the room code so the player can reconnect if they disconnect
        //     SessionManager.Singleton.SetRoomCode(joinCodeInputField.text.ToUpper());
        //     return;
        // }
        
        // enableButtons(true);
        // joinCodeInputField.text = "";
        LobbyManager.Singleton.JoinLobbyByCode(joinCodeInputField.text.ToUpper());
    }

    // Enable or disable all buttons
    private void enableButtons(bool isInteractable)
    {
        createMapButton.interactable = isInteractable;
        hostButton.interactable = isInteractable;
        joinCodeInputField.interactable = isInteractable;
        joinButton.interactable = isInteractable;
    }
}
