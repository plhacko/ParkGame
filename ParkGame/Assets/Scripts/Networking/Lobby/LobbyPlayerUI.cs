using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Networking
{
    public class LobbyPlayerUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI playerNameLabel;
        [SerializeField] private Button playerUnReadyButton;

        public void Initialize(LobbyTeamUI lobbyTeamUI, PlayerData playerData)
        {
            playerUnReadyButton.onClick.AddListener(() =>
            {
                lobbyTeamUI.RemovePlayer(playerData.ID);
            });
            
            playerNameLabel.text = playerData.Name;
        }
    }
}