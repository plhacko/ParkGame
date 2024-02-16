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

    protected override bool AttackEnemyIfInRange(Transform enemyT, float maxAttackDistance = 0) {
        float maxRange = MaxAttackRange;
        if (maxAttackDistance > 0) {
            maxRange = maxAttackDistance;
        }
        if (Vector3.Distance(enemyT.position, transform.position) <= maxRange
            && Vector3.Distance(enemyT.position, transform.position) >= MinAttackRange) {
            if (AttackTimer >= Attackcooldown) {
                AttackTimer = 0.0f;
                Networkanimator.Animator.SetFloat(AnimatorMovementSpeedHash, 0.0f); 

                Networkanimator.SetTrigger("Attack");

                if (TypeOfUnit == UnitType.Pawn || TypeOfUnit == UnitType.Horseman)
                {
                    PlaySwordAttackSFXClientRpc();
                    enemyT.GetComponent<ISoldier>()?.TakeDamage(Damage);
                }
                if (TypeOfUnit == UnitType.Archer) {
                    Vector2 direction = enemyT.position - transform.position;
                    Direction directionE = GetDirectionEnum(direction);
                    bool flip = directionE == Direction.Left;

                    XSpriteFlip.Value = flip;
                    Debug.Log("shoot");
                    PlayArcherAttackSFXClientRpc();
                }
            }
            return true;
        }
        return false;
    }
}
