using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Services.Core;
using Unity.Services.Authentication;
using UnityEngine.SceneManagement;
using Managers;

public class UIMainMenuController : MonoBehaviour
{
    [SerializeField] private string hostMenuSceneName;
    [SerializeField] private string createMapMenuSceneName;
    [SerializeField] private Button joinButton;
    [SerializeField] private Button hostButton;
    [SerializeField] private Button createMapButton;
    [SerializeField] private TMP_InputField joinCodeInputField;

    private async void Start()
    {
        joinButton.onClick.AddListener(joinGame);
        hostButton.onClick.AddListener(hostGame);
        createMapButton.onClick.AddListener(createMap);
        
        // Disable buttons until the player is signed in
        enableButtons(false);
        
        // Initialize Unity Services and sign in anonymously
        await UnityServices.InitializeAsync();
        
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();   
        }
        
        enableButtons(true);
        
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
    
    // Go to scene where the player can host a new game
    private void hostGame()
    {
    }

    // Join a game with the room code entered in the input field
    private async void joinGame()
    {
        enableButtons(false);
        
        string playerName = PlayerPrefs.GetString("PlayerName", "");
        bool joined = await OurNetworkManager.Singleton.JoinGame(joinCodeInputField.text, playerName);
        if (joined)
        {
            // Save the room code so the player can reconnect if they disconnect
            SessionManager.Singleton.SetRoomCode(joinCodeInputField.text.ToUpper());
            return;
        }
        
        enableButtons(true);
        joinCodeInputField.text = "";
    }

    // Enable or disable all buttons
    private void enableButtons(bool isInteractable)
    {
        createMapButton.interactable = isInteractable;
        joinButton.interactable = isInteractable;
        hostButton.interactable = isInteractable;
        joinCodeInputField.interactable = isInteractable;
    }
}
