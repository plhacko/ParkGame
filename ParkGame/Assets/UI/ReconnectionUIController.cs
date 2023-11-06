using System;
using System.Linq;
using Managers;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class ReconnectionUIController : MonoBehaviour
    {
        [SerializeField] private string joinGameSceneName = "JoinGameMenu";
        [SerializeField] private GameObject reconnectionUIParent;
        [SerializeField] private Button reconnectButton;

        private PlayerManager playerManager;

        private void Awake()
        {
            playerManager = FindObjectOfType<PlayerManager>();
            playerManager.OnClientReconnectedCallback += onClientReconnected;
            OurNetworkManager.Singleton.OnClientDisconnect += onClientDisconnect;
            reconnectButton.onClick.AddListener(tryReconnect);
        }

        private void Update()
        {
#if UNITY_EDITOR
            debugDisconnectFirstPlayer();
#endif
        }

#if UNITY_EDITOR
        private void debugDisconnectFirstPlayer()
        {
            if (Input.GetKeyDown(KeyCode.X) && OurNetworkManager.Singleton.IsHost)
            {
                var clientData = SessionManager.Singleton.PlayersData.GuidToPlayerData.First((pair =>
                    pair.Key != SessionManager.Singleton.PlayersData.LocalPlayerData.ID));

                var clientId = SessionManager.Singleton.PlayersData.GetClientId(clientData.Key);
                if (clientId.HasValue)
                {
                    OurNetworkManager.Singleton.DisconnectClient(clientId.Value);
                }
            }
        }
#endif

        private void OnDestroy()
        {
            playerManager.OnClientReconnectedCallback -= onClientReconnected;
            if (OurNetworkManager.Singleton != null)
                OurNetworkManager.Singleton.OnClientDisconnect -= onClientDisconnect;
        }

        private void onClientReconnected()
        {
            reconnectionUIParent.SetActive(false);
        }

        private void onClientDisconnect(bool isHost, ulong clientId)
        {
            if (isHost) return;

            reconnectionUIParent.SetActive(true);
            reconnectButton.interactable = true;
        }

        private async void tryReconnect()
        {
            reconnectButton.interactable = false;

            string playerName = PlayerPrefs.GetString("PlayerName", "");
            bool joined = await OurNetworkManager.Singleton.JoinGame(SessionManager.Singleton.RoomCode, playerName);
            if (!joined)
            {
                SessionManager.Singleton.EndSessionAndGoToScene(joinGameSceneName);
            }
        }
    }
}
