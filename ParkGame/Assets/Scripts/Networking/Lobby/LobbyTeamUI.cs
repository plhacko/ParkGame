using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Networking
{
    public class LobbyTeamUI : MonoBehaviour
    {
        [SerializeField] private LobbyPlayerUI lobbyPlayerUIPrefab;
        [SerializeField] private Button joinButton;
        [SerializeField] private RectTransform teamParent;
        [SerializeField] private TextMeshProUGUI teamNameLabel;
        
        private LobbyMenuController lobbyMenuController;
        private int teamNumber;

        private readonly Dictionary<Guid, LobbyPlayerUI> playerUIs = new();

        private void Awake()
        {
            playerUIs.Clear();
            joinButton.onClick.AddListener(onJoinButtonClicked);
        }

        private void onJoinButtonClicked()
        {
            lobbyMenuController.JoinTeam(teamNumber);
        }

        public void Initialize(LobbyMenuController lobbyMenuController, int teamNumber)
        {
            this.lobbyMenuController = lobbyMenuController;
            this.teamNumber = teamNumber;
            teamNameLabel.text = $"Team {teamNumber + 1}";
        }

        public void AddPlayer(PlayerData playerData, bool isLocalPlayer)
        {
            LobbyPlayerUI playerUI = Instantiate(lobbyPlayerUIPrefab, teamParent);
            playerUI.Initialize(this, playerData, isLocalPlayer);
            playerUIs.Add(playerData.ID, playerUI);
        }
        
        public void RemovePlayer(PlayerData playerData)
        {
            lobbyMenuController.RemoveFromTeam(playerData);
        }

        public bool CanJoin()
        {
            var teams = SessionManager.Singleton.GetTeams();
            return teams[teamNumber].Count <= SessionManager.MaxNumPlayersPerTeam;
        }
        
        public void TryEnableJoinButton(bool interactable)
        {
            joinButton.interactable = interactable && CanJoin();
        }

        public void RemovePlayerUI(Guid playerId)
        {
            Destroy(playerUIs[playerId].gameObject);
            playerUIs.Remove(playerId);

            if (SessionManager.Singleton.LocalPlayerId == playerId)
            {
                TryEnableJoinButton(true);   
            }
        }
    }
}