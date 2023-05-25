using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EnemyDetector : MonoBehaviour
{
    public List<Soldier> enemySoldiersInMyVicinity = new List<Soldier>();
    private Soldier localSoldier;

    private void Awake()
    {
        localSoldier = GetComponentInParent<Soldier>();
    }

    public Soldier GetClosesEnemy()
    {
        Soldier closestEnemy = null;
        float closestDistance = float.MaxValue;
        foreach (Soldier e in enemySoldiersInMyVicinity)
        {
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
            || enemySoldiersInMyVicinity.Contains(otherSoldier))
        { return; }

        enemySoldiersInMyVicinity.Add(otherSoldier);
    }
    private void OnTriggerExit2D(Collider2D other)
    {
        Debug.Log($"triger EXIT {other.gameObject.name}");

        // removes the soldier from a list, because it is no longer visible
        Soldier otherSoldier = other.gameObject.GetComponent<Soldier>();
        if (otherSoldier == null) { return; }
        enemySoldiersInMyVicinity.Remove(otherSoldier);
    }
}
