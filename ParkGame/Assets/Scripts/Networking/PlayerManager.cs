using Player;
using Unity.Netcode;
using UnityEngine;

public class PlayerManager : NetworkBehaviour
{
    [SerializeField] PlayerController playerControllerPrefab;

    private void initialize()
    {
        if (!IsHost) return;
        
        foreach (var clientKV in NetworkManager.Singleton.ConnectedClients)
        {
            Instantiate(playerControllerPrefab).GetComponent<NetworkObject>().SpawnWithOwnership(clientKV.Key, true);
        }
    }
    
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        initialize();
    }
}
