using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using Player;
using Managers;

public class FormationButton : NetworkBehaviour
{
    [SerializeField]
    private Button BoxFormation_Button, CircleFormation_Button;
    private PlayerManager playerManager;

    void Start() {
        BoxFormation_Button.onClick.AddListener(delegate { OnClick(KeyCode.R); });
        CircleFormation_Button.onClick.AddListener(delegate { OnClick(KeyCode.C); } );
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

    void OnClick(KeyCode key) {
        ulong clientID = NetworkManager.Singleton.LocalClientId;
        RequestChangeOfFormationServerRpc(clientID, key);
    }
}
