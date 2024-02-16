using UnityEngine;
using static Formation;

public class Horserider : ISoldier {
    [SerializeField] protected float HorseManSpeed = 0.3f;

    public override void OnNetworkSpawn() {
        base.Initialize();
    }

    public override void NewCommand(SoldierCommand command) {
        if (!IsServer) { return; }

        Command = command;
        if (command == SoldierCommand.Attack) {
            EnemiesInAttackWaveCounter = 0; // reset counter of targeted enemies (because of moleman's modus operandi)
        }
    }

// basemovement speed ???
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
