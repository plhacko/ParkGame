using System.Collections;
using System.Collections.Generic;
using Managers;
using Player;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GameManager : NetworkBehaviour 
{
    [SerializeField] private GameObject mapCreator;
    [SerializeField] private PlayerManager playerManager;

    [ServerRpc(RequireOwnership = false)]
    void RequestChangeOfFormationServerRpc(ulong clientID, KeyCode key) {
        PlayerController playerController = playerManager.GetPlayerController(clientID);
        
        if (playerController != null) {
            playerController.FormatSoldiersServerRpc(key);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void RequestChangeOfSoldierCommandServerRpc(ulong clientID, KeyCode key) {
        PlayerController playerController = playerManager.GetPlayerController(clientID);

        if (playerController != null) {
            if (key == KeyCode.I) { playerController.CommandMovementServerRpc(); }
            if (key == KeyCode.O) { playerController.CommandIdleServerRpc(); }
            if (key == KeyCode.P) { playerController.CommandAttackServerRpc(); }
        }
    }

    void Movement(KeyCode key) {
        ulong clientID = NetworkManager.Singleton.LocalClientId;
        RequestChangeOfSoldierCommandServerRpc(clientID, key);
    }

    void Formation(KeyCode key) {
        ulong clientID = NetworkManager.Singleton.LocalClientId;
        RequestChangeOfFormationServerRpc(clientID, key);
    }

    public void FormationBox()
    {
        Formation(KeyCode.R);
    }

    public void FormationCircle()
    {
        Formation(KeyCode.C);
    }

    public void CommandMove()
    {
        Movement(KeyCode.I);
    }

    public void CommandIdle()
    {
        Movement(KeyCode.O);
    }

    public void CommandAttack()
    {
        Movement(KeyCode.P);
    }

    public void Hide()
    {
        var grid = mapCreator.transform.GetComponentInChildren<Grid>().transform;
        foreach (Transform child in grid)
        {
            child.gameObject.GetComponent<TilemapRenderer>().enabled = false;
        }
    }

    public void Show()
    {
        var grid = mapCreator.transform.GetComponentInChildren<Grid>().transform;
        foreach (Transform child in grid)
        {
            child.gameObject.GetComponent<TilemapRenderer>().enabled = true;
        }
    }
}
