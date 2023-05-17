using System;
using System.Collections.Generic;
using Networking.Lobby;
using UnityEngine;
using UnityEngine.UI;

namespace Networking
{
    public class LobbyTeamUI : MonoBehaviour
    {
        [SerializeField] private LobbyPlayerUI lobbyPlayerUIPrefab;
        [SerializeField] private Button joinButton;
        [SerializeField] private RectTransform teamParent;
        
        private LobbyMenuController2 lobbyMenuController2;
        private TeamAllocationData teamAllocationData;

        private readonly Dictionary<Guid, LobbyPlayerUI> playerUIs = new();

        private void Awake()
        {
            joinButton.onClick.AddListener(onJoinButtonClicked);
        }

        private void onJoinButtonClicked()
        {
            lobbyMenuController2.JoinTeam(teamAllocationData.TeamNumber);
        }

        public void Initialize(LobbyMenuController2 lobbyMenuController2, TeamAllocationData teamAllocationData)
        {
            this.lobbyMenuController2 = lobbyMenuController2;
            this.teamAllocationData = teamAllocationData;
        }

        public void AddPlayer(PlayerData playerData)
        {
            LobbyPlayerUI playerUI = Instantiate(lobbyPlayerUIPrefab, teamParent);
            playerUI.Initialize(this, playerData);
            playerUIs.Add(playerData.ID, playerUI);
        }
        
        public void RemovePlayer(Guid playerData)
        {
            lobbyMenuController2.RemoveFromTeamUI(playerData, teamAllocationData.TeamNumber);
        }
    }
}