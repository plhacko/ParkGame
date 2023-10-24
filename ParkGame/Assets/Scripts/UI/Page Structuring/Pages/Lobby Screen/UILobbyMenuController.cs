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
        
        private readonly List<LobbyTeamUI> teamUIs = new();
        private List<UILobbyTeam> newTeamUIs = new ();

        private float maxImageSize;

        private void Start()
        {
            maxImageSize = drawnTexture.rectTransform.sizeDelta.x;
            backButton.onClick.AddListener(goBack);            
            LobbyManager.Singleton.OnLobbyInvalidated += UpdateUI;
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
                        () => LobbyManager.Singleton.ChangeTeamForPlayer(playedId, -1),
                        () => LobbyManager.Singleton.IsHost
                    );

                }
            }
        }

        public async void OnEnter()
        {
            if (LobbyManager.Singleton.MapData == null)
            {
                await LobbyManager.Singleton.GetMapData();
            }

            InitializeUIwithMapData(LobbyManager.Singleton.MapData);
        }

        public void OnExit()
        {
            roomCodeLabel.text = "Code: ";
            mapNameLabel.text = "";
            foreach (var teamUI in teamUIs)
            {
                Destroy(teamUI.gameObject);
            }
            teamUIs.Clear();
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
        }

        private UILobbyTeam InitializeTeamUI(int teamNumber)
        {
            var lobbyTeam = Instantiate(lobbyTeamPrefab, teamsParent);
            lobbyTeam.Initialize(teamNumber, LobbyManager.Singleton.JoinTeam);
            return lobbyTeam;
        }

        private void OnDestroy()
        {
            if (LobbyManager.Singleton != null)
            {
                LobbyManager.Singleton.OnLobbyInvalidated -= UpdateUI;
            }
        }
        
        private void goBack()
        {
            if (OurNetworkManager.Singleton.IsHost)
            {
                var copyOfKeys = new List<ulong>(OurNetworkManager.Singleton.ConnectedClients.Keys);
                
                foreach (var clientId in copyOfKeys)
                {
                    if(clientId == OurNetworkManager.Singleton.LocalClientId) continue;
                    OurNetworkManager.Singleton.DisconnectClient(clientId);
                }
            }

            SessionManager.Singleton.EndSession();
            onGoBack.Invoke();
        }

        private void startGame()
        {
            OurNetworkManager.Singleton.LoadGameScene();
        }
    }
}
