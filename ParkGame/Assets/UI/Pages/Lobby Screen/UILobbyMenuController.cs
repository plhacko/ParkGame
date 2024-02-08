using System.Collections.Generic;
using Managers;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Lobby
{
    
    /*
     * This class is responsible for the UI of the Lobby menu.
     * Players can see the room code, their team, and the other players in the game.
     * Host can start the game from here.
     */
    public class UILobbyMenuController : UIPageController
    {
        [SerializeField] private Button backButton;
        [SerializeField] private Button startGameButton;
        [SerializeField] private TextMeshProUGUI mapNameLabel;
        [SerializeField] private TextMeshProUGUI roomCodeLabel;
        [SerializeField] private UILobbyTeam lobbyTeamPrefab;
        [SerializeField] private RawImage drawnTexture;
        [SerializeField] private RawImage gpsTexture;
        [SerializeField] private RectTransform teamsParent;
        [SerializeField] private UIPage mainMenuPage;
        [SerializeField] private ColorSettings colorSettings;
        [Header("Structre sprites")]
        [SerializeField] private GameObject castle;
        [SerializeField] private GameObject victoryPoint;
        [SerializeField] private GameObject outpost;
        private List<UILobbyTeam> newTeamUIs = new ();

        private float maxImageSize;

        private void Start()
        {
            maxImageSize = drawnTexture.rectTransform.sizeDelta.x;
            
            backButton.onClick.AddListener(Back);  
            startGameButton.onClick.AddListener(StartGame);
        }

        private void OnDestroy()
        {
            if (LobbyManager.Singleton == null) return;
            
            LobbyManager.Singleton.OnLobbyInvalidate -= UpdateUI;
            LobbyManager.Singleton.OnDisconnect -= OnDisconnect;
        }

        public override async void OnEnter()
        {
            if (LobbyManager.Singleton.MapData == null)
            {
                await LobbyManager.Singleton.DownloadMapData();
            }

            InitializeUIwithMapData(LobbyManager.Singleton.MapData);

            LobbyManager.Singleton.OnLobbyInvalidate += UpdateUI;
            LobbyManager.Singleton.OnDisconnect += OnDisconnect;
        }

        public override void OnExit()
        {
            roomCodeLabel.text = "Code: ";
            mapNameLabel.text = "";
            foreach (var teamUI in newTeamUIs)
            {
                Destroy(teamUI.gameObject);
            }
            newTeamUIs.Clear();

            LobbyManager.Singleton.OnLobbyInvalidate -= UpdateUI;
            LobbyManager.Singleton.OnDisconnect -= OnDisconnect;
        }

        private void UpdateUI()
        {
            if (LobbyManager.Singleton.IsHost)
            {
                startGameButton.gameObject.SetActive(true);
                startGameButton.interactable = isGameValid(LobbyManager.Singleton.MapData);
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

        private void InitializeUIwithMapData(MapData mapData)
        {
            drawnTexture.texture = mapData.DrawnTexture;
            gpsTexture.texture = mapData.GPSTexture;

            gpsTexture.color = Color.white;
            drawnTexture.color = Color.white;

            Vector2 imageSize = mapData.GetImageSize() * maxImageSize;

            gpsTexture.rectTransform.sizeDelta = imageSize;
            drawnTexture.rectTransform.sizeDelta = imageSize;
            mapData.addStructuresToMap(drawnTexture, colorSettings, outpost, castle, victoryPoint);
            mapNameLabel.text = mapData.MetaData.MapName;
            for (int teamNumber = 0; teamNumber < mapData.MetaData.NumTeams; teamNumber++)
            {
                UILobbyTeam lobbyTeam = InitializeTeamUI(teamNumber);
                newTeamUIs.Add(lobbyTeam);
            }

            UpdateUI();
        }
        
        private bool isGameValid(MapData mapData)
        {
            int numTeams = mapData.MetaData.NumTeams;
            bool[] teamHasPlayer = new bool[numTeams];

            var teams = LobbyManager.Singleton.GetTeams();
            foreach (var (_, team) in teams)
            {
                if (team != -1)
                {
                    teamHasPlayer[team] = true;   
                }
            }

            bool isValid = true;
            foreach (var hasPlayer in teamHasPlayer)
            {
                isValid = isValid && hasPlayer;
            }

            return isValid;
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

        private async void Back()
        {
            backButton.interactable = false;
            await LobbyManager.Singleton.LeaveLobby();
            NetworkManager.Singleton.Shutdown();
            backButton.interactable = true;
        }

        private void StartGame()
        {
            LobbyManager.Singleton.StartGame();
        }

        private void OnDisconnect()
        {
            UIController.Singleton.PushUIPage(mainMenuPage);
        }
    }
}
