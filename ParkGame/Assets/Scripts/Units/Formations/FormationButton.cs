using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using Player;

public class FormationButton : NetworkBehaviour
{
    public Button BoxFormation_Button, CircleFormation_Button;
    public PlayerController MyCommanderCntrl;

    void Start() {
        BoxFormation_Button.onClick.AddListener(delegate { OnClick(KeyCode.R); });
        CircleFormation_Button.onClick.AddListener(delegate { OnClick(KeyCode.C); } );
    }
    void OnClick(KeyCode key) {
        Debug.Log("CLICKED BUTTON " + key);
        if (!MyCommanderCntrl) {
            MyCommanderCntrl = FindObjectOfType<PlayerController>();
        }
        MyCommanderCntrl.FormatSoldiersServerRpc(key);

    }
}
