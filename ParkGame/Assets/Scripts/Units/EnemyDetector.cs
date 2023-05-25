using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyDetector : MonoBehaviour
{
    public List<GameObject> soldiersInMyVicinity;

    private void Awake() {
        soldiersInMyVicinity = new List<GameObject>();
    }
    private void OnTriggerEnter2D(Collider2D other) {
        Debug.Log($"utululululu {other.gameObject.name}");
        GameObject otherSoldier = other.gameObject;
        // ptej se na tym
        soldiersInMyVicinity.Add(otherSoldier);
    }
    private void OnTriggerExit2D(Collider2D other) {
        Debug.Log("TULUU!");
        GameObject s = other.gameObject;
        soldiersInMyVicinity.Remove(s);
    }
}
