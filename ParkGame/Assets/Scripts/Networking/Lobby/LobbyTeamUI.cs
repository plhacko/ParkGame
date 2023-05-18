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
        
        private LobbyMenuController lobbyMenuController;
        private int teamNumber;

        private readonly Dictionary<Guid, LobbyPlayerUI> playerUIs = new();

        private void Awake()
        {
            Debug.Log("created for team " + teamNumber);
            playerUIs.Clear();
            joinButton.onClick.AddListener(onJoinButtonClicked);
        }

        private void Update()
        {
            // Debug.Log(playerUIs.Count);
        }

        private void onJoinButtonClicked()
        {
            lobbyMenuController.JoinTeam(teamNumber);
        }

        public void Initialize(LobbyMenuController lobbyMenuController, int teamNumber)
        {
            Debug.Log("created for team ---- " + teamNumber);
            this.lobbyMenuController = lobbyMenuController;
            this.teamNumber = teamNumber;
        }

        public void AddPlayer(PlayerData playerData, bool isLocalPlayer)
        {
            Debug.Log(playerUIs.Count + " " + playerData.ID);
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

            if (SessionManager.Singleton.LocalPlayerId == playerId)
            {
                TryEnableJoinButton(true);   
            }
        }
    }
}