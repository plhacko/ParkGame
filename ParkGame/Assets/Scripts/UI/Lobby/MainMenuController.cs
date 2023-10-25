﻿using Firebase.Auth;
using Managers;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UI.Lobby
{
    
    /*
     * This class is responsible for the UI of the Join Game menu.
     * Player can join a game here with a room code or start hosting a new game.
     */ 
    public class MainMenuController : MonoBehaviour
    {
        [SerializeField] private string hostMenuSceneName;
        [SerializeField] private string createMapMenuSceneName;
        [SerializeField] private string welcomeMenuSceneName;
        [SerializeField] private Button joinButton;
        [SerializeField] private Button hostButton;
        [SerializeField] private Button createMapButton;
        [SerializeField] private Button logOutButton; 
        [SerializeField] private TMP_InputField joinCodeInputField;

        private async void Start()
        {
            joinButton.onClick.AddListener(joinGame);
            hostButton.onClick.AddListener(hostGame);
            createMapButton.onClick.AddListener(createMap);
            logOutButton.onClick.AddListener(logOut);
            
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

            // Debug.Log(SessionManager.Singleton);
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

        private void logOut()
        {
            enableButtons(false);
            FirebaseAuth.DefaultInstance.SignOut();
            SceneManager.LoadScene(welcomeMenuSceneName);
        }

        // Go to scene where the player can create a new map
        private void createMap()
        {
            enableButtons(false);
            SceneManager.LoadScene(createMapMenuSceneName, LoadSceneMode.Single);
        }
        
        // Go to scene where the player can host a new game
        private void hostGame()
        {
            enableButtons(false);
            SceneManager.LoadScene(hostMenuSceneName, LoadSceneMode.Single);
        }

        // Join a game with the room code entered in the input field
        private async void joinGame()
        {
            enableButtons(false);

            string playerName = FirebaseAuth.DefaultInstance.CurrentUser?.DisplayName ?? "Name not found";
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
            logOutButton.interactable = isInteractable;
        }
    }
}