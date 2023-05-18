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
                // PlayerData data = playerData.Value;
                // data.Team = teamNumber;
                // data.Name = nameInputField.text;
            }
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
            teamUIs[playerData.Team].AddPlayer(playerData);
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