using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using Player;
using Managers;

public class FormationButton : NetworkBehaviour
{
    [SerializeField]
    private Button BoxFormation_Button, CircleFormation_Button, MoveCommand_Button, IdleCommand_Button, AttackCommand_Button;
    private PlayerManager playerManager;

    void Start() {
        BoxFormation_Button.onClick.AddListener(delegate { OnClickFormation(KeyCode.R); });
        CircleFormation_Button.onClick.AddListener(delegate { OnClickFormation(KeyCode.C); } );
        MoveCommand_Button.onClick.AddListener(delegate { OnClick(KeyCode.I); } );
        IdleCommand_Button.onClick.AddListener(delegate { OnClick(KeyCode.O); } );
        AttackCommand_Button.onClick.AddListener(delegate { OnClick(KeyCode.P); } );
        playerManager = FindObjectOfType<PlayerManager>();
    }

    [ServerRpc(RequireOwnership = false)]
    void RequestChangeOfFormationServerRpc(ulong clientID, KeyCode key) {
        PlayerController playerControler = playerManager.GetPlayerController(clientID);

        playerControler = playerManager.GetPlayerController(clientID);
        if (playerControler != null) {
            playerControler.FormatSoldiersServerRpc(key);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void RequestChangeOfSoldierCommandServerRpc(ulong clientID, KeyCode key) {
        PlayerController playerControler = playerManager.GetPlayerController(clientID);

        playerControler = playerManager.GetPlayerController(clientID);
        if (playerControler != null) {
            if (key == KeyCode.I) { playerControler.CommandMovementServerRpc(); }
            if (key == KeyCode.O) { playerControler.CommandIdleServerRpc(); }
            if (key == KeyCode.P) { playerControler.CommandAttackServerRpc(); }
        }
    }

    void OnClick(KeyCode key) {
        ulong clientID = NetworkManager.Singleton.LocalClientId;
        RequestChangeOfSoldierCommandServerRpc(clientID, key);
    }

    void OnClickFormation(KeyCode key) {
        ulong clientID = NetworkManager.Singleton.LocalClientId;
        RequestChangeOfFormationServerRpc(clientID, key);
    }
}
