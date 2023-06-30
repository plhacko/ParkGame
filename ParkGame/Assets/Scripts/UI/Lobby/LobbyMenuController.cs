﻿using System;
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
    public class LobbyMenuController : MonoBehaviour
    {
        [SerializeField] private string joinMenuSceneName;
        
        [SerializeField] private Button goBackButton;
        [SerializeField] private Button startGameButton;
        [SerializeField] private TextMeshProUGUI roomCodeLabel;
        [SerializeField] private LobbyTeamUI lobbyTeamUIPrefab;

        [SerializeField] private RectTransform teamsParent;
        
        private List<LobbyTeamUI> teamUIs = new();

        private void Awake()
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

            SessionManager.Singleton.OnSetPlayerData += onSetPlayerData;
            roomCodeLabel.text += SessionManager.Singleton.RoomCode;
            goBackButton.onClick.AddListener(goBack);

            // Initialize team UI for the host.
            // Clients need to wait till they receive the map data and then initialize the team UI.
            if (OurNetworkManager.Singleton.IsHost)
            {
                initializeTeamUI(SessionManager.Singleton.MapMetaData);   
            }
            else
            {
                SessionManager.Singleton.OnMapReceived += initializeTeamUI;
            }
            
            OurNetworkManager.Singleton.OnClientDisconnectCallback += onClientDisconnect;
            SessionManager.Singleton.OnTeamJoined += onTeamJoined;
        }

        private void OnDestroy()
        {
            if (SessionManager.Singleton != null)
            {
                SessionManager.Singleton.OnSetPlayerData -= onSetPlayerData;
                SessionManager.Singleton.OnMapReceived -= initializeTeamUI;   
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
            
        }
        
        // public void JoinTeam(int teamNumber)
        // {
        //     PlayerData playerData = SessionManager.Singleton.LocalPlayerData;
        //     
        //     if (SessionManager.Singleton.IsTeamFull(teamNumber)) return;
        //     
        //     foreach (var teamUI in teamUIs)
        //     {
        //         teamUI.TryEnableJoinButton(true);    
        //     }
        //     teamUIs[teamNumber].TryEnableJoinButton(false);
        //     
        //     if (OurNetworkManager.Singleton.IsHost)
        //     {
        //         int oldTeam = playerData.Team;
        //     
        //         data.Team = teamNumber;
        //         SessionManager.Singleton.PlayersData.UpdatePlayerData(data);
        //         
        //         removeFromTeamUI(data.ID, oldTeam);
        //         addPlayerToTeamUI(data);
        //         joinTeamClientRpc(data, oldTeam);
        //     }
        //     else
        //     {
        //         joinTeamServerRpc(OurNetworkManager.Singleton.LocalClientId, teamNumber);
        //     }
        // }
        
        // public void RemoveFromTeam(PlayerData playerData)
        // {
        //     if (OurNetworkManager.Singleton.IsHost)
        //     {
        //         removeFromTeamUI(playerData.ID, playerData.Team);
        //         removeFromTeamClientRpc(playerData);
        //         
        //         playerData.Team = -1;
        //         SessionManager.Singleton.PlayersData.UpdatePlayerData(playerData);
        //     }
        //     else
        //     {
        //         removeFromTeamServerRpc(OurNetworkManager.Singleton.LocalClientId);
        //     }
        // }

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

        private void onSetPlayerData(PlayerData playerData)
        {
            if(playerData.Team == -1) return;
            addPlayerToTeamUI(playerData);
        }
        
        private void initializeTeamUI(MapMetaData mapMetaData)
        {
            for (int teamNumber = 0; teamNumber < mapMetaData.NumTeams; teamNumber++)
            {
                LobbyTeamUI teamUI = createTeamUI(teamNumber);
                teamUIs.Add(teamUI);
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

            SessionManager.Singleton.EndSessionAndGoToScene(joinMenuSceneName);
        }

        private void startGame()
        {
            OurNetworkManager.Singleton.LoadGameScene();
        }

        // [ServerRpc(RequireOwnership = false)]
        // private void joinTeamServerRpc(ulong clientId, int newTeam, ServerRpcParams clientRpcParams = default)
        // {
        //     var playerData = SessionManager.Singleton.PlayersData.GetPlayerData(clientId);
        //     if(!playerData.HasValue) return;
        //     
        //     if(newTeam < 0 || newTeam >= SessionManager.Singleton.MapMetaData.NumTeams) return;
        //
        //     var data = playerData.Value;
        //     int oldTeam = data.Team;
        //     data.Team = newTeam;
        //     
        //     removeFromTeamUI(data.ID, oldTeam);
        //     addPlayerToTeamUI(data);
        //     SessionManager.Singleton.PlayersData.UpdatePlayerData(data);
        //     joinTeamClientRpc(data, oldTeam);
        // }
        
        // [ClientRpc]
        // private void joinTeamClientRpc(PlayerData playerData, int oldTeamNumber, ClientRpcParams clientRpcParams = default)
        // {
        //     if(OurNetworkManager.Singleton.IsHost) return;
        //     
        //     removeFromTeamUI(playerData.ID, oldTeamNumber);
        //     addPlayerToTeamUI(playerData);
        //     
        //     SessionManager.Singleton.PlayersData.UpdatePlayerData(playerData);
        // }
        
        private void addPlayerToTeamUI(PlayerData playerData)
        {
            bool isLocalPlayer = playerData.ID == SessionManager.Singleton.LocalPlayerId;
            teamUIs[playerData.Team].AddPlayer(playerData, isLocalPlayer);
        }

        private void removeFromTeamUI(Guid playerId, int teamNumber)
        {
            if(teamNumber == -1) return;
            teamUIs[teamNumber].RemovePlayerUI(playerId);
        }
    }
}