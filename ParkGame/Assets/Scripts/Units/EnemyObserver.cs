using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Unity.Collections;
using UnityEngine;

public class EnemyObserver : MonoBehaviour
{
    [SerializeField] // TODO: rm SerializeField
    List<Transform> visibleEnemies = new List<Transform>();

    public Transform GetClosestEnemy(Transform t)
        => visibleEnemies.Count > 0 ? visibleEnemies.Min(e => (Vector3.Distance(e.position, t.position), e)).e : null;

    void OnCollisionEnter(Collision collision)
    {
        // type of colision check
        if (/*!collision.isTrigger ||*/ !collision.gameObject.CompareTag("Unit"))
        { return; }

        Debug.Log($"TriggerEnter : {collision.gameObject.name}");
        visibleEnemies.Add(collision.transform);
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        Debug.Log($"TriggerEnter : {collision.name}");

        // type of colision check
        if (collision.isTrigger || !collision.CompareTag("Unit"))
        { return; }

        visibleEnemies.Remove(collision.transform);
    }
}
