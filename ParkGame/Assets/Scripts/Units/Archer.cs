using System;
using UnityEngine;

public class Archer : SoldierBase {

    private ShootScript shooting;
    protected override void Initialize() {
        shooting = GetComponent<ShootScript>();
        TypeOfUnit = UnitType.Archer;
    }
    
    public override void OnNetworkSpawn() {
        base.Initialize();
        Initialize();
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
                StopMoving();

                Networkanimator.SetTrigger("Attack");

                Vector2 direction = enemyT.position - transform.position;
                Direction directionE = GetDirectionEnum(direction);
                bool flip = directionE == Direction.Left;
                
                XSpriteFlip.Value = flip;
                //PlayArcherAttackSFXClientRpc();

                // we need to delay the sooting by 0.5s to synch with the animation 
                StartCoroutine(CallcackAfterDelayCoroutine(delay: 0.5f, callback: () =>
                {
                    if (enemyT) {
                        shooting.Shoot(enemyT.transform.position, Damage, flip, Team, true);
                    }
                }));
            }
            return true;
        }
        return false;
    }

    System.Collections.IEnumerator CallcackAfterDelayCoroutine(float delay, Action callback)
    {
        yield return new WaitForSeconds(delay);
        callback?.Invoke();
    }
}
