using Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Lobby
{
    /*
     * This class is responsible for one player UI item in the Lobby menu.
     */
    public class LobbyPlayerUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI playerNameLabel;
        [SerializeField] private Button playerUnReadyButton;

        public void Initialize(LobbyMenuController lobbyMenuController, PlayerData playerData, bool isLocalPlayer)
        {
            playerNameLabel.text = playerData.Name;
            
            if(!isLocalPlayer) return;
            
            playerUnReadyButton.gameObject.SetActive(true);
            playerUnReadyButton.onClick.AddListener(() => 
                lobbyMenuController.RemoveFromTeam(playerData.ID));
        }

        public void Initialize(UILobbyMenuController lobbyMenuController, PlayerData playerData, bool isLocalPlayer)
        {
            playerNameLabel.text = playerData.Name;
            
            if(!isLocalPlayer) return;
            
            playerUnReadyButton.gameObject.SetActive(true);
            playerUnReadyButton.onClick.AddListener(() => 
                lobbyMenuController.RemoveFromTeam(playerData.ID));
        }
    }
}