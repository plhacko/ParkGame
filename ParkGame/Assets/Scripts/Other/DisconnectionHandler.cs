using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DisconnectionHandler : NetworkBehaviour
{
    [SerializeField] private GameObject hostDisconnectScreen;
    [SerializeField] private GameObject disconnectingScreen;
    [SerializeField] private Button backToMenuButton;
    
    private void Start()
    {
        backToMenuButton.onClick.AddListener(DisconnectAndLeave);
    }
    
    public void DisconnectAndLeave()
    {
        if (IsHost)
        {
            sendDisconnectClientRpc();
            Debug.Log("Sending bye");
        }
        else
        {
            if (NetworkManager.Singleton.IsConnectedClient)
            {
                sendDisconnectServerRpc(NetworkManager.Singleton.LocalClientId);    
            }
        }
        
        StartCoroutine(Disconnecting());
    }
    
    IEnumerator Disconnecting()
    {
        disconnectingScreen.SetActive(true);
        yield return new WaitForSeconds(1);
        NetworkManager.Singleton.Shutdown();
        SceneManager.LoadScene("CleanUpScene");
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
        
        Debug.Log("Receive bye");
        hostDisconnectScreen.SetActive(true);
        NetworkManager.Singleton.Shutdown();
    }
}
