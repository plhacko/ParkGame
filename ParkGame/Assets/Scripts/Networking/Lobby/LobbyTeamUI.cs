using System;
using System.Collections.Generic;
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
        private int teamNumber;

        private readonly Dictionary<Guid, LobbyPlayerUI> playerUIs = new();

        private void Awake()
        {
            joinButton.onClick.AddListener(onJoinButtonClicked);
        }

        private void onJoinButtonClicked()
        {
            lobbyMenuController2.JoinTeam(teamNumber);
        }

        public void Initialize(LobbyMenuController2 lobbyMenuController2, int teamNumber)
        {
            this.lobbyMenuController2 = lobbyMenuController2;
            this.teamNumber = teamNumber;
        }

        public void AddPlayer(PlayerData playerData)
        {
            LobbyPlayerUI playerUI = Instantiate(lobbyPlayerUIPrefab, teamParent);
            playerUI.Initialize(this, playerData);
            playerUIs.Add(playerData.ID, playerUI);
        }
        
        public void RemovePlayer(PlayerData playerData)
        {
            // lobbyMenuController2.RemoveFromTeam(playerData);
        }

        public bool CanJoin()
        {
            var teams = SessionManager.Singleton.GetTeams();
            return teams[teamNumber].Count <= 4;
        }
        
        public void TryEnableJoinButton(bool interactable)
        {
            joinButton.interactable = interactable && CanJoin();
        }

        public void RemovePlayerUI(Guid playerId)
        {
            Destroy(playerUIs[playerId].gameObject);
            playerUIs.Remove(playerId);
        }
    }
}