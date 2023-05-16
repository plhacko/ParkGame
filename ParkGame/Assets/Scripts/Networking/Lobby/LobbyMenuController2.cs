using System;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Networking
{
    [Serializable]
    public struct Range
    {
        public int Min;
        public int Max;

        public Range(int min, int max)
        {
            Min = min;
            Max = max;
        }
    }
    
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
        [SerializeField] private List<Range> teamSizes = new();

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

            roomCodeLabel.text += OurNetworkManager.Singleton.RoomCode;
        
            goBackButton.onClick.AddListener(goBack);

            foreach (var teamSize in teamSizes)
            {
                if(teamSize.Max > 0)
                    createTeamUI(teamSize);
            }
        }

        private void createTeamUI(Range teamSize)
        {
            var teamUI = Instantiate(lobbyTeamUIPrefab, teamsParent);
            teamUI.Initialize(teamSize);
        }

        private void goBack()
        {
            throw new System.NotImplementedException();
        }

        private void startGame()
        {
            NetworkManager.Singleton.SceneManager.LoadScene(gameSceneName, LoadSceneMode.Single);
        }
    }
}