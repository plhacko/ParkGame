using System;
using System.Collections.Generic;
using Networking.Lobby;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Networking
{
    public class LobbyMenuController2 : NetworkBehaviour
    {
        [SerializeField] private string gameSceneName;
        [SerializeField] private string joinMenuSceneName;

        [SerializeField] private Button goBackButton;
        [SerializeField] private Button startGameButton;
        [SerializeField] private TextMeshProUGUI roomCodeLabel;
        [SerializeField] private LobbyTeamUI lobbyTeamUIPrefab;

        [SerializeField] private RectTransform teamsParent;
        [SerializeField] private List<LobbyTeamUI> teamUIs = new();
        
        private MapData mapData;

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            initialize();
        }
        
        void initialize()
        {
            if (IsHost)
            {
                startGameButton.onClick.AddListener(startGame);
            }
            else
            {
                startGameButton.gameObject.SetActive(false);
            }

            SessionManager.Singleton.OnSetPlayerData += onSetPlayerData;
            
            roomCodeLabel.text += OurNetworkManager.Singleton.RoomCode;
        
            goBackButton.onClick.AddListener(goBack);

            if (IsHost)
            {
                initializeTeamUI(OurNetworkManager.Singleton.MapData);   
            }
            else
            {
                SessionManager.Singleton.OnMapReceived += initializeTeamUI;
            }
        }

        private void onSetPlayerData(PlayerData playerData)
        {
            if(playerData.Team == -1) return;
            addPlayerToTeamUI(playerData);
        }
        
        private void initializeTeamUI(MapData mapData)
        {
            this.mapData = mapData;
            for (int teamNumber = 0; teamNumber < mapData.NumTeams; teamNumber++)
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
            throw new System.NotImplementedException();
        }

        private void startGame()
        {
            NetworkManager.Singleton.SceneManager.LoadScene(gameSceneName, LoadSceneMode.Single);
        }

        public void JoinTeam(int teamNumber)
        {
            PlayerData? playerData = SessionManager.Singleton.GetLocalPlayerData();
            
            if (!playerData.HasValue) return;
            if (!teamUIs[teamNumber].CanJoin()) return;
            
            foreach (var teamUI in teamUIs)
            {
                teamUI.TryEnableJoinButton(true);    
            }
            teamUIs[teamNumber].TryEnableJoinButton(false);
            
            if (IsHost)
            {
                
                PlayerData data = playerData.Value;
                int oldTeam = playerData.Value.Team;

                data.Team = teamNumber;
                SessionManager.Singleton.UpdatePlayerData(data);
                
                removeFromTeamUI(data.ID, oldTeam);
                addPlayerToTeamUI(data);
                JoinTeamClientRpc(data, oldTeam);
            }
            else
            {
                JoinTeamServerRpc(OurNetworkManager.Singleton.LocalClientId, teamNumber);
            }
        }
        
        public void RemoveFromTeam(PlayerData playerData)
        {
            if (IsHost)
            {
                removeFromTeamUI(playerData.ID, playerData.Team);
                RemoveFromTeamClientRpc(playerData);
                
                playerData.Team = -1;
                SessionManager.Singleton.UpdatePlayerData(playerData);
            }
            else
            {
                // RemoveFromTeamServerRpc(OurNetworkManager.Singleton.LocalClientId);
            }
        }

        [ClientRpc]
        private void RemoveFromTeamClientRpc(PlayerData playerData, ClientRpcParams clientRpcParams = default)
        {
            if(IsHost) return;
            
            removeFromTeamUI(playerData.ID, playerData.Team);
            playerData.Team = -1;
            SessionManager.Singleton.UpdatePlayerData(playerData);
        }

        [ServerRpc(RequireOwnership = false)]
        private void JoinTeamServerRpc(ulong clientId, int newTeam, ServerRpcParams clientRpcParams = default)
        {
            var playerData = SessionManager.Singleton.GetPlayerData(clientId);
            if(!playerData.HasValue) return;
            
            if(newTeam < 0 || newTeam >= mapData.NumTeams) return;

            var data = playerData.Value;
            int oldTeam = data.Team;
            data.Team = newTeam;
            
            removeFromTeamUI(data.ID, oldTeam);
            addPlayerToTeamUI(data);
            SessionManager.Singleton.UpdatePlayerData(data);
            JoinTeamClientRpc(data, oldTeam);
        }

        [ClientRpc]
        private void JoinTeamClientRpc(PlayerData playerData, int oldTeamNumber, ClientRpcParams clientRpcParams = default)
        {
            if(IsHost) return;
            
            removeFromTeamUI(playerData.ID, oldTeamNumber);
            addPlayerToTeamUI(playerData);
            
            SessionManager.Singleton.UpdatePlayerData(playerData);
        }
        
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
        
        // public void RemoveFromTeam(PlayerData data, int oldTeamNumber)
        // {
        //     if(data.Team == -1) return;
        //     
        //     teamUIs[data.Team].RemovePlayerUI(data.ID);
        //     data.Team = -1;
        //     SessionManager.Singleton.SetPlayerData(data);
        // }

        // [ServerRpc]
        // private void RemoveFromTeamUIServerRpc(Guid playerData, int teamNumber, ServerRpcParams clientRpcParams = default)
        // {
        //     throw new NotImplementedException();
        // }
        //
        // [ClientRpc]
        // private void RemoveFromTeamUIClientRpc(Guid playerData, int teamNumber, ClientRpcParams clientRpcParams = default)
        // {
        //     throw new NotImplementedException();
        // }
    }
}