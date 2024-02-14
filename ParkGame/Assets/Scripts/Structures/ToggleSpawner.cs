using UnityEngine;
using System;
public class ToggleSpawner : MonoBehaviour
{
    [SerializeField] GameObject UnitPrefab;
    [SerializeField] GameObject ArcherPrefab;
    [SerializeField] GameObject HorsemanPrefab;

    [SerializeField] Sprite PawnIcon;
    [SerializeField] Sprite ArcherIcon;
    [SerializeField] Sprite HorsemanIcon;

    // change spawn type and icon
    public Soldier.UnitType OutpostUnitType { get => outpostUnitType; }
    private Soldier.UnitType outpostUnitType;
    public Action<Soldier.UnitType> OnUnitTypeChange;

    public Sprite ChangeSpawnType(int n) {
        Sprite icon = null;
        switch (n) {
            case 1:
                outpostUnitType = Soldier.UnitType.Archer;
                icon = ArcherIcon;
                break;
            case 2:
                outpostUnitType = Soldier.UnitType.Horseman;
                icon = HorsemanIcon;
                break;
            default:
                outpostUnitType = Soldier.UnitType.Pawn;
                icon = PawnIcon;
                break;
        }

        OnUnitTypeChange?.Invoke(outpostUnitType);
        return icon;
    }

    public void SetSpawnType(Soldier.UnitType type) {
        outpostUnitType = type;
    }

    public GameObject GetSpawnPrefab() {
        switch (outpostUnitType) {
            case Soldier.UnitType.Archer:
                return ArcherPrefab;
            case Soldier.UnitType.Horseman:
                return HorsemanPrefab;
            case Soldier.UnitType.Pawn:
            default:
                return UnitPrefab;
        }
    }

}
