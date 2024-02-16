using UnityEngine;
using static Formation;


public class Soldier : ISoldier {

    public override void OnNetworkSpawn() {
        base.Initialize();
    }
   
    // volat na vsech typech override!!!
    protected override void SetSoldierSpeed() {
        if (playerManager.GetLocalPlayerController().IsOnPath ||
           (ReturningToOutpost && pathTileChecker.IsNearbyPath(Agent.transform.position)) // short-circuiting for efficiency
           ) {
            Agent.speed = BaseMovementSpeed * PathMovementSpeedMultiplier;
        } else {
            Agent.speed = BaseMovementSpeed;
        }
    }

    protected override Transform GetEnemy() {
        if (targetedEnemy != null) {
            return targetedEnemy;
        }
        Transform enemyT = EnemyObserver.GetClosestEnemy();
        return enemyT;
    }
}
