using System.Threading.Tasks;
using Managers;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DisconnectionHandler : NetworkBehaviour
{
    [SerializeField] private GameObject hostDisconnectScreen;
    [SerializeField] private Button backToMenuButton;
    
    private void Start()
    {
        backToMenuButton.onClick.AddListener(() =>
        {
            SceneManager.LoadScene("CleanUpScene");
        });
    }
    
    public void Disconnect()
    {
        if (IsHost)
        {
            sendDisconnectClientRpc();
        }
        else
        {
            sendDisconnectServerRpc(NetworkManager.Singleton.LocalClientId);
            NetworkManager.Singleton.Shutdown();
        }
    }
    
    [ServerRpc(RequireOwnership = false)]
    private void sendDisconnectServerRpc(ulong clientId)
    {
        // NetworkManager.Singleton.DisconnectClient(clientId);
    }

    [ClientRpc]
    private void sendDisconnectClientRpc()
    {
        if (IsHost) return;
        hostDisconnectScreen.SetActive(true);
        NetworkManager.Singleton.Shutdown();
    }
}
