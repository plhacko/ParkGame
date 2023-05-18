using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnSoldier : MonoBehaviour
{
    // x -12 12, y -7 7

    public GameObject soldierPrefab;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.K)) {
            Debug.Log("spawn new bugger");
            GameObject sod = Instantiate(soldierPrefab);
            float rx = Random.Range(-12, 12);
            float ry = Random.Range(-7, 7);
            sod.transform.position = new Vector3(rx, ry, 0);
        }
    }
}
