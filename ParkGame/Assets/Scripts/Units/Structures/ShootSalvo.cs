using UnityEngine;

public class ShootSalvo : MonoBehaviour
{
    [SerializeField] int arrowCooldownTime = 5;
    [SerializeField] int salveOfArrows = 6; // number of enemies targeted at once
    [SerializeField] int Damage = 1;
    private float arrowTimer; // castle shoots arrows
    private ShootScript shooting;
    private EnemyObserver enemyObserver;

    public void Start() {
        arrowTimer = arrowCooldownTime;
        shooting = GetComponent<ShootScript>();
        enemyObserver = GetComponentInChildren<EnemyObserver>();
    }

    // called by server
    public bool Shoot(float time, int team) {
        arrowTimer -= time;
        if (arrowTimer > 0) { return false; } 

        var visibleEnemies = enemyObserver.GetAllEnemies();
        int endCond = (visibleEnemies.Count >= salveOfArrows ? salveOfArrows : visibleEnemies.Count);
        bool res = false;
        for (int i = 0; i < endCond; i++) {
            Transform enemyT = visibleEnemies[i];
            Vector2 direction = enemyT.position - transform.position;
            Direction directionE = Soldier.GetDirectionEnum(direction);
            bool flip = directionE == Direction.Left;
            shooting.Shoot(enemyT.transform.position, Damage, flip, team);

            res = true;
        }
        arrowTimer = arrowCooldownTime;

        return res;
    }

}
