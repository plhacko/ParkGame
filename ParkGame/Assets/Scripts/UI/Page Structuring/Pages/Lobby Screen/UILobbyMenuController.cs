using System;
using System.Collections.Generic;
using Managers;
using TMPro;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UI.Lobby
{
    
    /*
     * This class is responsible for the UI of the Lobby menu.
     * Players can see the room code, their team, and the other players in the game.
     * Host can start the game from here.
     */
    public class UILobbyMenuController : MonoBehaviour
    {
        [SerializeField] private string joinMenuSceneName;
        
        [SerializeField] private Button backButton;
        [SerializeField] private UnityEvent onBackPressed;
        [SerializeField] private UnityEvent onGoBack;

        [SerializeField] private Button startGameButton;
        [SerializeField] private UnityEvent onStartGamePressed;

        [SerializeField] private TextMeshProUGUI mapNameLabel;
        [SerializeField] private TextMeshProUGUI roomCodeLabel;
        [SerializeField] private LobbyTeamUI lobbyTeamUIPrefab;
        [SerializeField] private UILobbyTeam lobbyTeamPrefab;
        [SerializeField] private RawImage drawnTexture;
        [SerializeField] private RawImage gpsTexture;
        
        [SerializeField] private RectTransform teamsParent;

        [SerializeField] private UnityEvent onDisconnect;
        
        private List<UILobbyTeam> newTeamUIs = new ();

        private float maxImageSize;

        private void Start()
        {
            maxImageSize = drawnTexture.rectTransform.sizeDelta.x;
            backButton.onClick.AddListener(goBack);            
            LobbyManager.Singleton.OnLobbyInvalidat += UpdateUI;
            LobbyManager.Singleton.OnDisconnect += onDisconnect.Invoke;
        }

        private void UpdateUI()
        {
            if (LobbyManager.Singleton.IsHost)
            {
                startGameButton.gameObject.SetActive(true);
            }
            else
            {
                startGameButton.gameObject.SetActive(false);
            }

            roomCodeLabel.text = "Code: " + LobbyManager.Singleton.Lobby.LobbyCode;

            var model = LobbyManager.Singleton.LobbyModel;
            var lobby = LobbyManager.Singleton.Lobby;

            foreach (var teamUI in newTeamUIs)
            {
                teamUI.Clear();
            }

            foreach (var entry in model.Teams)
            {
                var playedId = entry.Key;
                var player = lobby.Players.Find(x => x.Id == playedId);
                var teamNumber = entry.Value;
                
                if (teamNumber != -1)
                {
                    newTeamUIs[teamNumber].AddPlayerUI(
                        player, 
                        async () => await LobbyManager.Singleton.RemovePlayerFromLobby(playedId),
                        () => LobbyManager.Singleton.IsHost
                    );

                }
            }
        }

        public async void OnEnter()
        {
            if (LobbyManager.Singleton.MapData == null)
            {
                await LobbyManager.Singleton.DownloadMapData();
            }

            InitializeUIwithMapData(LobbyManager.Singleton.MapData);
        }

        public void OnExit()
        {
            roomCodeLabel.text = "Code: ";
            mapNameLabel.text = "";
            foreach (var teamUI in newTeamUIs)
            {
                Destroy(teamUI.gameObject);
            }
            newTeamUIs.Clear();
        }

        private void InitializeUIwithMapData(MapData mapData)
        {
            drawnTexture.texture = mapData.DrawnTexture;
            gpsTexture.texture = mapData.GPSTexture;

            gpsTexture.color = Color.white;
            drawnTexture.color = Color.white;

            Vector2 imageSize = mapData.GetImageSize() * maxImageSize;

            gpsTexture.rectTransform.sizeDelta = imageSize;
            drawnTexture.rectTransform.sizeDelta = imageSize;

            mapNameLabel.text = mapData.MetaData.MapName;
            for (int teamNumber = 0; teamNumber < mapData.MetaData.NumTeams; teamNumber++)
            {
                UILobbyTeam lobbyTeam = InitializeTeamUI(teamNumber);
                newTeamUIs.Add(lobbyTeam);
            }

            UpdateUI();
        }

        private UILobbyTeam InitializeTeamUI(int teamNumber)
        {
            var lobbyTeam = Instantiate(lobbyTeamPrefab, teamsParent);
            lobbyTeam.Initialize(teamNumber,
                async (teamNumber) => 
                {
                    await LobbyManager.Singleton.JoinTeam(teamNumber);
                }
            );
            return lobbyTeam;
        }

        private void OnDestroy()
        {
            if (LobbyManager.Singleton != null)
            {
                LobbyManager.Singleton.OnLobbyInvalidat -= UpdateUI;
                LobbyManager.Singleton.OnDisconnect -= onDisconnect.Invoke;
            }
        }
        
        private async void goBack()
        {
            bool success = await LobbyManager.Singleton.LeaveLobby();
            if (success)
            {
                UIController.Singleton.PopUIPage();
            }
            else
            {
                Debug.LogError("Failed to leave lobby");
            }
        }

        private void startGame()
        {
            OurNetworkManager.Singleton.LoadGameScene();
        }
    }
}
