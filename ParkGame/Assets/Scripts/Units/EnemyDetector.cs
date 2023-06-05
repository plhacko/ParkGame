using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EnemyDetector : MonoBehaviour
{
    public List<Soldier> enemySoldiersInSight = new List<Soldier>();
    private Soldier localSoldier;

    private void Awake()
    {
        localSoldier = GetComponentInParent<Soldier>();
    }
    public bool IsEnemyInSight()
    {
        enemySoldiersInSight.RemoveAll(item => item == null);
        return enemySoldiersInSight.Count > 0;
    }
    
    public Soldier GetClosesEnemy()
    {
        enemySoldiersInSight.RemoveAll(item => item == null);
        Soldier closestEnemy = null;
        float closestDistance = float.MaxValue;
        foreach (Soldier e in enemySoldiersInSight)
        {
            if (e == null) { continue; }
            float distance = Vector3.Distance(e.transform.position, localSoldier.transform.position);
            if (closestDistance > distance)
            {
                closestEnemy = e;
                closestDistance = distance;
            }
        }

        return closestEnemy;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"triger ENTER {other.gameObject.name}");

        Soldier otherSoldier = other.gameObject.GetComponent<Soldier>();

        // checks if the soldier should be added
        if (otherSoldier == null
            || otherSoldier.team == localSoldier.team
            || enemySoldiersInSight.Contains(otherSoldier))
        { return; }

        enemySoldiersInSight.Add(otherSoldier);
    }
    private void OnTriggerExit2D(Collider2D other)
    {
        // TODO: rm // Debug.Log($"triger EXIT {other.gameObject.name}");

        // removes the soldier from a list, because it is no longer visible
        Soldier otherSoldier = other.gameObject.GetComponent<Soldier>();
        if (otherSoldier == null) { return; }
        enemySoldiersInSight.Remove(otherSoldier);
    }
}
