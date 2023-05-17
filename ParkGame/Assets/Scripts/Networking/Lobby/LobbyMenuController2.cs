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
        [SerializeField] private TMP_InputField nameInputField;
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

            OurNetworkManager.Singleton.OnClientConnectedCallback += onClientConnected;
            
            roomCodeLabel.text += OurNetworkManager.Singleton.RoomCode;
        
            goBackButton.onClick.AddListener(goBack);

            if (!IsHost) return;
            
            initializeTeamUI(OurNetworkManager.Singleton.MapData);
        }
        
        [ClientRpc]
        private void sendMapDataClientRpc(MapData mapData, ClientRpcParams clientRpcParams)
        {
            initializeTeamUI(mapData);
        }

        private void onClientConnected(ulong clientId)
        {
            if (IsHost)
            {
                if (clientId == OurNetworkManager.Singleton.LocalClientId) return;
                
                ClientRpcParams clientRpcParams = new ClientRpcParams
                {
                    Send = new ClientRpcSendParams
                    {
                        TargetClientIds = new ulong[]{ clientId }
                    }
                };

                sendMapDataClientRpc(OurNetworkManager.Singleton.MapData, clientRpcParams);
            }
            
        }

        private void initializeTeamUI(MapData mapData)
        {
            foreach (var team in mapData.Teams)
            {
                if (team.PlayerCountRange.Max <= 0) continue;
                
                LobbyTeamUI teamUI = createTeamUI(team);
                teamUIs.Add(teamUI);
            }
        }

        private LobbyTeamUI createTeamUI(TeamAllocationData teamAllocationData)
        {
            var teamUI = Instantiate(lobbyTeamUIPrefab, teamsParent);
            teamUI.Initialize(this, teamAllocationData);
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
            if(!playerData.HasValue) return;
            
            if (IsHost)
            {
                PlayerData data = playerData.Value;
                data.Team = teamNumber;
                data.Name = nameInputField.text;
                addPlayerToTeamUI(data);
                JoinTeamClientRpc(OurNetworkManager.Singleton.LocalClientId, data);
            }
            else
            {
                
            }
        }

        [ClientRpc]
        private void JoinTeamClientRpc(ulong clientId, PlayerData playerData, ClientRpcParams clientRpcParams = default)
        {
            if(IsHost) return;
            
            addPlayerToTeamUI(playerData);
        }
        
        private void addPlayerToTeamUI(PlayerData playerData)
        {
            SessionManager.Singleton.SetPlayerData(playerData);
            teamUIs[playerData.Team].AddPlayer(playerData);
        }

        public void RemoveFromTeamUI(Guid playerData, int teamNumber)
        {
            if (IsHost)
            {
                // RemoveFromTeamUIClientRpc(playerData, teamNumber);   
            }
            else
            {
                // RemoveFromTeamUIServerRpc(playerData, teamNumber);
            }
        }

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