using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Networking
{
    public class LobbyPlayerUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI playerNameLabel;
        [SerializeField] private Button playerUnReadyButton;

        public void Initialize(LobbyTeamUI lobbyTeamUI, PlayerData playerData, bool isLocalPlayer)
        {
            playerNameLabel.text = playerData.Name.Value.Value;
            
            if(!isLocalPlayer) return;
            
            playerUnReadyButton.gameObject.SetActive(true);
            playerUnReadyButton.onClick.AddListener(() =>
            {
                lobbyTeamUI.RemovePlayer(playerData);
            });
        }
    }
}