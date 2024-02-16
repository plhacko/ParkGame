using UnityEngine;
using static Formation;


public class Horserider : ISoldier {
    [SerializeField] protected float HorseManSpeed = 0.3f;

    public override void OnNetworkSpawn() {
        base.Initialize();
    }

    // basemovement speed ???
    protected override void SetSoldierSpeed() {
        Agent.speed = HorseManSpeed;
        if (FormationType == FormationType.Box) {
            if (playerManager.GetLocalPlayerController().IsOnPath ||
                (ReturningToOutpost && pathTileChecker.IsNearbyPath(Agent.transform.position)) // short-circuiting for efficiency
            )
                Agent.speed = BaseMovementSpeed * PathMovementSpeedMultiplier;
            else
                Agent.speed = BaseMovementSpeed;
        }
    }

    protected override Transform GetEnemy() {
        if (targetedEnemy != null) {
            return targetedEnemy;
        }
        Transform enemyT = EnemyObserver.GetClosestEnemy();
        if (EnemiesInAttackWaveCounter == 0) {
            enemyT = EnemyObserver.GetFarthestEnemy(); // else attack the closest enemy
        }
        targetedEnemy = enemyT;
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

                PlaySwordAttackSFXClientRpc();
                enemyT.GetComponent<ISoldier>()?.TakeDamage(Damage);
            }
            return true;
        }
        return false;
    }
}
