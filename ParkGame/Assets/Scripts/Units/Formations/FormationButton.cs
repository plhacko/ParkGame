using UnityEngine;
using UnityEngine.UI;

public class FormationButton : MonoBehaviour
{
    public Button BoxFormation_Button, CircleFormation_Button;
    public Player.PlayerController MyCommanderCntrl;

    void Start() {
        BoxFormation_Button.onClick.AddListener(delegate { OnClick(KeyCode.R); });
        CircleFormation_Button.onClick.AddListener(delegate { OnClick(KeyCode.C); } );
    }
    void OnClick(KeyCode key) {
        if (!MyCommanderCntrl) {
            MyCommanderCntrl = FindObjectOfType<Player.PlayerController>();
        }
        MyCommanderCntrl.FormatSoldiers(key);

    }
}
