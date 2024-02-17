using UnityEngine;
using static Formation;

public class Horserider : ISoldier {
    [SerializeField] protected float HorseManSpeed = 0.3f;

    public override void OnNetworkSpawn() {
        base.Initialize();
        TypeOfUnit = UnitType.Horseman;
    }

    protected override void SetSoldierSpeed() {
        Agent.speed = HorseManSpeed;
        if (FormationType == FormationType.Box) {
            if (playerManager.GetLocalPlayerController().IsOnPath ||
                (ReturningToOutpost && pathTileChecker.IsNearbyPath(Agent.transform.position)) // short-circuiting for efficiency
            ) {
                Agent.speed = BaseMovementSpeed * PathMovementSpeedMultiplier;
            } else {
                Agent.speed = BaseMovementSpeed;
            }
        }
    }

    protected override Transform GetEnemy() {
        if (targetedEnemy != null) {
            return targetedEnemy;
        }
        Transform enemyT = EnemyObserver.GetClosestEnemy();
        if (EnemiesInAttackWaveCounter == 0) {
            enemyT = EnemyObserver.GetFarthestEnemy(); 
        }
        return enemyT;
    }
}
