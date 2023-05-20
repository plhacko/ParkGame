using Networking;
using UnityEngine;
using UnityEngine.UI;

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
    
    private void OnDestroy()
    {
        playerManager.OnClientReconnectedCallback -= onClientReconnected;
        if(OurNetworkManager.Singleton != null)
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
          
        bool joined = await OurNetworkManager.Singleton.JoinGame(OurNetworkManager.Singleton.RoomCode);
        if (!joined)
        {
            SessionManager.Singleton.EndSessionAndGoToScene(joinGameSceneName);
        }        
    }
}
