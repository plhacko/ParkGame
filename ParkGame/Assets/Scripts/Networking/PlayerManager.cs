using System;
using System.Collections.Generic;
using Networking;
using Player;
using Unity.Netcode;
using UnityEngine;

public class PlayerManager : NetworkBehaviour
{
    [SerializeField] PlayerController playerControllerPrefab;

    Dictionary<Guid, PlayerController> playerControllers = new();
    
    public event Action OnClientReconnectedCallback;
    
    private void initialize()
    {
        if (!IsHost) return;
        
        OurNetworkManager.Singleton.OnClientConnectedCallback += onClientConnect;
        
        foreach (var clientKV in NetworkManager.Singleton.ConnectedClients)
        {
            if (!SessionManager.Singleton.ClientIdToPlayerId.TryGetValue(clientKV.Key, out var playerId)) continue;
            
            PlayerController playerController = Instantiate(playerControllerPrefab);
            playerController.Initialize(playerId);
            playerController.GetComponent<NetworkObject>().SpawnWithOwnership(clientKV.Key, true);
            playerController.GetComponent<NetworkObject>().DontDestroyWithOwner = true;
            
            playerControllers.Add(playerId, playerController);
        }
    }

    private void onClientConnect(ulong clientId)
    {
        var clientData = SessionManager.Singleton.GetPlayerData(clientId);
        if (!clientData.HasValue) return;
        if (!playerControllers.TryGetValue(clientData.Value.ID, out var playerController)) return;
        
        playerController.GetComponent<NetworkObject>().ChangeOwnership(clientId);
        sendReconnectedSuccessClientRpc(OurNetworkManager.OneClientRpcParams(clientId));
    }

    [ClientRpc]
    private void sendReconnectedSuccessClientRpc(ClientRpcParams clientRpcParams)
    {
        OnClientReconnectedCallback?.Invoke();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        initialize();
    }
    
    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        OurNetworkManager.Singleton.OnClientConnectedCallback -= onClientConnect;
    }
}
