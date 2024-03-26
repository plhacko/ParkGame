using UnityEngine;

public class Soldier : SoldierBase {

    public override void OnNetworkSpawn() {
        base.Initialize();
        TypeOfUnit = UnitType.Pawn;
    }
  
    protected override Transform GetEnemy() {
        if (targetedEnemy != null) {
            return targetedEnemy;
        }
        Transform enemyT = EnemyObserver.GetClosestEnemy();
        return enemyT;
    }
}
