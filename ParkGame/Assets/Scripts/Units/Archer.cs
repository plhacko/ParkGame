using UnityEngine;
using static Formation;


public class Archer : ISoldier {

    private ShootScript shooting;
    protected override void Initialize() {
  //      shooting = GetComponent<ShootScript>(); 
    }
    private void Start() {
              shooting = GetComponent<ShootScript>(); 
    }
    public override void OnNetworkSpawn() {
        base.Initialize();
 //       Initialize();
    }

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
        targetedEnemy = EnemyObserver.GetEnemyInRange(MinAttackRange, MaxAttackRange);
        return targetedEnemy;
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

                Vector2 direction = enemyT.position - transform.position;
                Direction directionE = GetDirectionEnum(direction);
                bool flip = directionE == Direction.Left;
                
                XSpriteFlip.Value = flip;
                Debug.Log("shoot");
                PlayArcherAttackSFXClientRpc();
                shooting.Shoot(enemyT.transform.position, Damage, flip, Team);
            }
            return true;
        }
        return false;
    }
}
