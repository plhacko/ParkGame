using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToggleSpawnedUnitScript : MonoBehaviour {
    [SerializeField] Sprite PawnIcon;
    [SerializeField] Sprite ArcherIcon;
    [SerializeField] Sprite HorsemanIcon;
    private SpriteRenderer sr;
    private int counter;
    public UnitType OutpostUnitType;

    private void Start() {
        OutpostUnitType = UnitType.Pawn;
        sr = GetComponent<SpriteRenderer>();
    }

    Sprite GetIcon(int n) {
        switch (n) {
            case 1:
                OutpostUnitType = UnitType.Archer;
                return Instantiate(ArcherIcon);
            //case 2:
            //    OutpostUnitType = UnitType.Horseman;
            //    return Instantiate(HorsemanIcon);
            default:
                OutpostUnitType = UnitType.Pawn;
                return Instantiate(PawnIcon);
        }
    }

    void OnMouseDown() {
        Debug.Log("ICON CLICKED");
        counter++;
        sr.sprite = GetIcon(counter % 2);
        
    }

}
