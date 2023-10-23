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

        private float maxImageSize;

        private void Start()
        {
            maxImageSize = drawnTexture.rectTransform.sizeDelta.x;
            backButton.onClick.AddListener(goBack);            
            OurNetworkManager.Singleton.OnClientDisconnectCallback += onClientDisconnect;
            SessionManager.Singleton.OnTeamJoined += onTeamJoined;
        }

        public void OnEnter()
        {
            FillData();
        }

        public void OnExit()
        {
            EmptyData();
        }

        private void FillData()
        {
            // Enable start button for the host
            if (OurNetworkManager.Singleton.IsHost)
            {
                startGameButton.onClick.AddListener(startGame);
            }
            else
            {
                startGameButton.gameObject.SetActive(false);
            }

            roomCodeLabel.text += LobbyManager.Singleton.Lobby.LobbyCode;

            if (LobbyManager.Singleton.IsHost)
            {
                InitializeUIwithMapData(LobbyManager.Singleton.MapData);   
            }
            else
            {
                SessionManager.Singleton.OnMapReceived += initializeUI;
            } 
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
            }
        }

        private UILobbyTeam InitializeTeamUI(int teamNumber)
        {
            var lobbyTeam = Instantiate(lobbyTeamPrefab, teamsParent);
            lobbyTeam.Initialize(teamNumber, LobbyManager.Singleton.JoinTeam);
            return lobbyTeam;
        }

        private void EmptyData()
        {
            roomCodeLabel.text = "Code: ";
            mapNameLabel.text = "";
            foreach (var teamUI in teamUIs)
            {
                Destroy(teamUI.gameObject);
            }
            teamUIs.Clear();
        }

        private void OnDestroy()
        {
            if (SessionManager.Singleton != null)
            {
                SessionManager.Singleton.OnMapReceived -= initializeUI;
                SessionManager.Singleton.OnTeamJoined -= onTeamJoined;   
            }

            if (OurNetworkManager.Singleton != null)
            {
                OurNetworkManager.Singleton.OnClientDisconnectCallback -= onClientDisconnect;
            }
        }
        
        private void Update()
        {
            if (!OurNetworkManager.Singleton.IsHost) return;
            
            PlayerData? playerData = SessionManager.Singleton.PlayersData.GetPlayerData(NetworkManager.Singleton.LocalClientId);
            if (playerData.HasValue)
            {
                // Enable start button if at least the host joined the team
                startGameButton.interactable = playerData.Value.Team != -1;   
            }
        }
        
        public void RemoveFromTeam(Guid playerId)
        {
            SessionManager.Singleton.RemoveFromTeam(playerId);
        }
        
        public void JoinTeam(int teamNumber)
        {
            SessionManager.Singleton.JoinTeam(teamNumber);
        }

        private void onTeamJoined(Guid playerId, int previousTeam, int newTeam)
        {
            if (previousTeam >= 0 && previousTeam < SessionManager.Singleton.MapData.MetaData.NumTeams)
            {
                var teamUI = teamUIs[previousTeam];
                teamUI.RemovePlayerUI(playerId);
            }

            if (newTeam < 0 || newTeam >= SessionManager.Singleton.MapData.MetaData.NumTeams) return;
            
            PlayerData? playerData = SessionManager.Singleton.PlayersData.GetPlayerData(playerId);
            if (!playerData.HasValue) return;

            teamUIs[newTeam].AddPlayer(playerData.Value, SessionManager.Singleton.IsPlayerIdLocal(playerId), 0);
        }

        private void onClientDisconnect(ulong clientId)
        {
            if (OurNetworkManager.Singleton.IsHost)
            {
                var playerData = SessionManager.Singleton.PlayersData.GetPlayerData(clientId);
                if (!playerData.HasValue) return;
                
                RemoveFromTeam(playerData.Value.ID);
                return;
            }
            goBack();
        }

        private void initializeUI(MapData mapData)
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
                LobbyTeamUI teamUI = createTeamUI(teamNumber);
                teamUIs.Add(teamUI);
            }

            var teams = SessionManager.Singleton.PlayersData.GetTeams();
            foreach (var (_, players) in teams)
            {
                foreach (var player in players)
                {
                    addPlayerToTeamUI(player);        
                }
            }
        }

        private LobbyTeamUI createTeamUI(int teamNumber)
        {
            var teamUI = Instantiate(lobbyTeamUIPrefab, teamsParent);
            teamUI.Initialize(this, teamNumber);
            return teamUI;
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

        private void addPlayerToTeamUI(PlayerData playerData)
        {
            bool isLocalPlayer = playerData.ID == SessionManager.Singleton.LocalPlayerId;
            teamUIs[playerData.Team].AddPlayer(playerData, isLocalPlayer, 0);
        }
    }
}
