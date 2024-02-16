using Managers;
using Player;
using UnityEngine;
using static Formation;


public class Soldier : ISoldier {

    private ShootScript shooting;
    protected override void Initialize() {
        shooting = GetComponent<ShootScript>(); // only for Archers
    }

    public override void OnNetworkSpawn() {
        base.Initialize();
        Initialize();
    }
   
    // volat na vsech typech override!!!
    protected override void SetSoldierSpeed()
    {
        if (TypeOfUnit == UnitType.Horseman) {
            Agent.speed = HorseManSpeed;
        }
        if (TypeOfUnit != UnitType.Horseman || (TypeOfUnit == UnitType.Horseman && FormationType == FormationType.Box)) {
            if (
                playerManager.GetLocalPlayerController().IsOnPath ||
                (ReturningToOutpost && pathTileChecker.IsNearbyPath(Agent.transform.position)) // short-circuiting for efficiency
            )
                Agent.speed = BaseMovementSpeed * PathMovementSpeedMultiplier;
            else
                Agent.speed = BaseMovementSpeed;
        }
    }

    // overload this !!!
    protected override Transform GetEnemy() {
        if (targetedEnemy != null) {
            return targetedEnemy;
        }
        Transform enemyT = EnemyObserver.GetClosestEnemy();
        // dat pak podle potreby do Archer a Moleridera!
        if (TypeOfUnit == UnitType.Archer) {
            enemyT = EnemyObserver.GetEnemyInRange(MinAttackRange, MaxAttackRange);
        }
        if (TypeOfUnit == UnitType.Horseman) {
            if (EnemiesInAttackWaveCounter == 0) {
                enemyT = EnemyObserver.GetFarthestEnemy(); // else attack the closest enemy
            }
        }
        //
        targetedEnemy = enemyT;
        return enemyT;
    }

    /// <summary> attacks the closest enemy in range</summary>
    /// <returns> returns if enemy was in range </returns>
    /// 
    // override everywhere!!!
    protected override bool AttackEnemyIfInRange(Transform enemyT, float maxAttackDistance = 0) {
        float maxRange = MaxAttackRange;
        if (maxAttackDistance > 0) {
            maxRange = maxAttackDistance;
        }
        if (Vector3.Distance(enemyT.position, transform.position) <= maxRange
            && Vector3.Distance(enemyT.position, transform.position) >= MinAttackRange) {
            if (AttackTimer >= Attackcooldown) {
                AttackTimer = 0.0f;
                Networkanimator.Animator.SetFloat(AnimatorMovementSpeedHash, 0.0f); //("MovementSpeed", 0);

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
                    shooting.Shoot(enemyT.transform.position, Damage, flip, Team);
                }
            }
            return true;
        }
        return false;
    }

    // override!!!
    public override void NewCommand(SoldierCommand command) {
        if (!IsServer) { return; }

        Command = command;

        if (command == SoldierCommand.Attack) {
            EnemiesInAttackWaveCounter = 0; // reset counter of targeted enemies (because of moleman's modus operandi)
        }

    }
}
