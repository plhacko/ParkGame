using System;
using System.Collections.Generic;
using Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Lobby
{
    /*
     * This class is responsible for the UI of a single team in the Lobby menu.
     */
    public class LobbyTeamUI : MonoBehaviour
    {
        [SerializeField] private LobbyPlayerUI lobbyPlayerUIPrefab;
        [SerializeField] private Button joinButton;
        [SerializeField] private RectTransform teamParent;
        [SerializeField] private TextMeshProUGUI teamNameLabel;
        
        private LobbyMenuController lobbyMenuController;
        private int teamNumber; // team number is in 0 - 3 range

        // player ID -> player UI
        private readonly Dictionary<Guid, LobbyPlayerUI> playerUIs = new();

        public void Initialize(LobbyMenuController lobbyMenuController, int teamNumber)
        {
            this.lobbyMenuController = lobbyMenuController;
            this.teamNumber = teamNumber;
            teamNameLabel.text = $"Team {teamNumber + 1}";
            
            // todo maybe remove this?
            // playerUIs.Clear();
            joinButton.onClick.AddListener(() => lobbyMenuController.JoinTeam(teamNumber));
        }

        // Add a player to the team UI
        public void AddPlayer(PlayerData playerData, bool isLocalPlayer)
        {
            // Create new player UI, initialize it, and add it to the team UIs
            LobbyPlayerUI playerUI = Instantiate(lobbyPlayerUIPrefab, teamParent);
            playerUI.Initialize(lobbyMenuController, playerData, isLocalPlayer);
            playerUIs.Add(playerData.ID, playerUI);
        }

        // Try to enable the join team button
        public void TryEnableJoinButton(bool interactable)
        {
            joinButton.interactable = !SessionManager.Singleton.IsTeamFull(teamNumber) && interactable;
        }

        // Remove a player from the team UI
        public void RemovePlayerUI(Guid playerId)
        {
            Destroy(playerUIs[playerId].gameObject);
            playerUIs.Remove(playerId);

            // Tru to enable the join team button if the player leaving the team was the local player
            if (SessionManager.Singleton.LocalPlayerId == playerId)
            {
                TryEnableJoinButton(true);   
            }
        }
    }
}